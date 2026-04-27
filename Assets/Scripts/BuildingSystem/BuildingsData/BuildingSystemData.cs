using Unity.Collections;
using UnityEngine;

namespace Test.BuildingSystem
{
    public struct Building
    {
        public int Id;
        public BuildingType Type;
        public float DeathStartTime;

        public int ExtraDataIndex;

        public int FirstGroupIndex;
        public int GroupCount;
    }

    public struct InstanceGroup
    {
        public int OwnerEntityIndex;
        public int MeshId; //equal to the index of RenderLanes array
        public int MatrixStartIndex; //index of the first matrix of this object's mesh type in it's RanderLane
        public int MatrixCount;
    }

    public struct BridgeExtras
    {
        public Vector3 Start;
        public Vector3 End;
    }

    public class BuildingSystemData
    {
        // Counters
        public int IdCounter;
        public int DeleteCounter;

        // Core Entities
        public NativeList<Building> Entities;
        public NativeParallelHashMap<int, int> IdToEntityIndex;

        // Rendering Data
        public NativeList<InstanceGroup> InstanceGroups;
        public NativeList<Matrix4x4>[] RenderLanes;

        // Spatial & Extras
        public SpatialGrid Grid;
        public NativeList<BridgeExtras> BridgeExtras;

        public bool NeedsCompaction() => (DeleteCounter * 1f / Entities.Length) > 0.2f;

        public BuildingSystemData(DrawerRegistry registry)
        {
            Entities = new NativeList<Building>(100, Allocator.Persistent);
            IdToEntityIndex = new NativeParallelHashMap<int, int>(100, Allocator.Persistent);
            InstanceGroups = new NativeList<InstanceGroup>(400, Allocator.Persistent);
            RenderLanes = new NativeList<Matrix4x4>[registry.MeshAssets.Count];

            BridgeExtras = new NativeList<BridgeExtras>(100, Allocator.Persistent);

            Grid = new SpatialGrid(3f);

            for (int i = 0; i < registry.MeshAssets.Count; i++)
            {
                RenderLanes[i] = new NativeList<Matrix4x4>(1000, Allocator.Persistent);
            }
        }

        public BuildingSnapshot GetBuildingSnapshot(int id)
        {
            if (!IdToEntityIndex.TryGetValue(id, out int entIdx)) return default;
            var entity = Entities[entIdx];

            var snapshot = new BuildingSnapshot
            {
                EntityId = id,
                Type = entity.Type,
                StoredMatrices = new NativeList<Matrix4x4>(100, Allocator.Persistent) //foreach groups -> total += MatrixCount;
            };

            for (int i = 0; i < entity.GroupCount; i++)
            {
                var group = InstanceGroups[entity.FirstGroupIndex + i];
                var lane = RenderLanes[group.MeshId];

                for (int j = 0; j < group.MatrixCount; j++)
                {
                    int matrixIdx = group.MatrixStartIndex + j;
                    snapshot.StoredMatrices.Add(lane[matrixIdx]);
                }
            }
            return snapshot;
        }

        public void Dispose()
        {
            Entities.Dispose();
            InstanceGroups.Dispose();

            IdToEntityIndex.Dispose();
            BridgeExtras.Dispose();

            Grid.Dispose();

            foreach (var lane in RenderLanes)
            {
                lane.Dispose();
            }
        }
    }

    public enum BuildingType
    {
        Bridge
    }
}