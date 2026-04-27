using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Test.BuildingSystem
{
    public static class BuildingDataCompaction
    {
        public static CompactionTransaction ScheduleCompaction(BuildingSystemData data)
        {
            var now = Time.time;

            var step1Job = new CompactionMainJob
            {
                OldEntities = data.Entities,
                OldGroups = data.InstanceGroups,
                IdToEntityIndex = data.IdToEntityIndex,
                Time = now,
                DeleteCounter = new NativeReference<int>(0, Allocator.TempJob),
                NewEntities = new NativeList<Building>(data.Entities.Length - data.DeleteCounter, Allocator.TempJob),
                NewGroups = new NativeList<InstanceGroup>(data.InstanceGroups.Length, Allocator.TempJob),
                EntityRemap = new NativeArray<int>(data.Entities.Length, Allocator.TempJob),
                LaneOffsets = new NativeArray<int>(data.RenderLanes.Length, Allocator.TempJob)
            };
            JobHandle handle1 = step1Job.Schedule();

            var bridgeJob = new CompactExtrasJob<BridgeExtras>
            {
                TargetType = BuildingType.Bridge,
                OldEntities = data.Entities,
                EntityRemap = step1Job.EntityRemap,
                OldExtras = data.BridgeExtras,
                NewExtras = new NativeList<BridgeExtras>(data.BridgeExtras.Length, Allocator.TempJob),
                NewEntities = step1Job.NewEntities
            };
            JobHandle handle2 = bridgeJob.Schedule(handle1);

            var laneHandles = new NativeArray<JobHandle>(data.RenderLanes.Length, Allocator.Temp);
            var nextLanes = new NativeList<Matrix4x4>[data.RenderLanes.Length];
            for (int i = 0; i < data.RenderLanes.Length; i++)
            {
                nextLanes[i] = new NativeList<Matrix4x4>(data.RenderLanes[i].Length, Allocator.TempJob);

                var laneJob = new CompactLaneJob
                {
                    TargetMeshId = i,
                    OldGroups = data.InstanceGroups,
                    EntityRemap = step1Job.EntityRemap,
                    OldLane = data.RenderLanes[i],
                    NewLane = nextLanes[i]
                };

                laneHandles[i] = laneJob.Schedule(handle2);
            }

            JobHandle finalHandle = JobHandle.CombineDependencies(laneHandles);

            var result = new CompactionTransaction()
            {
                Handle = finalHandle,
                NewEntities = step1Job.NewEntities,
                NewGroups = step1Job.NewGroups,
                RemapTable = step1Job.EntityRemap,
                LaneOffsets = step1Job.LaneOffsets,
                NewExtras = bridgeJob.NewExtras,
                NewLanes = nextLanes,
                Handles = laneHandles,
                DeleteCounter = step1Job.DeleteCounter
            };

            return result;
        }
    }

    public class CompactionTransaction
    {
        public JobHandle Handle;

        public NativeList<Building> NewEntities;
        public NativeList<InstanceGroup> NewGroups;
        public NativeArray<int> RemapTable;
        public NativeArray<int> LaneOffsets;
        public NativeList<BridgeExtras> NewExtras;
        public NativeList<Matrix4x4>[] NewLanes;
        public NativeArray<JobHandle> Handles;
        public NativeReference<int> DeleteCounter;

        public void Dispose()
        {
            NewEntities.Dispose();
            NewGroups.Dispose();
            RemapTable.Dispose();
            LaneOffsets.Dispose();
            NewExtras.Dispose();
            DeleteCounter.Dispose();
            foreach (var lane in NewLanes) lane.Dispose();
        }

        public void ApplyTo(BuildingSystemData data)
        {
            data.Entities.ResizeUninitialized(NewEntities.Length);
            data.Entities.CopyFrom(NewEntities);

            data.InstanceGroups.ResizeUninitialized(NewGroups.Length);
            data.InstanceGroups.CopyFrom(NewGroups);

            data.BridgeExtras.ResizeUninitialized(NewExtras.Length);
            data.BridgeExtras.CopyFrom(NewExtras);

            for (int i = 0; i < data.RenderLanes.Length; i++)
            {
                data.RenderLanes[i].ResizeUninitialized(NewLanes[i].Length);
                data.RenderLanes[i].CopyFrom(NewLanes[i]);
            }

            data.DeleteCounter -= DeleteCounter.Value;
        }
    }
}