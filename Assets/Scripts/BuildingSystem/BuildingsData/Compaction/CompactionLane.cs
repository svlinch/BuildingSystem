using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;

namespace Test.BuildingSystem
{
    // Rebuilds per-mesh matrix buffers, filtering out deleted entities
    [BurstCompile]
    public struct CompactLaneJob : IJob
    {
        public int TargetMeshId;
        [ReadOnly] public NativeList<InstanceGroup> OldGroups;
        [ReadOnly] public NativeArray<int> EntityRemap;
        [ReadOnly] public NativeList<Matrix4x4> OldLane;

        public NativeList<Matrix4x4> NewLane;

        public void Execute()
        {
            for (int i = 0; i < OldGroups.Length; i++)
            {
                var group = OldGroups[i];

                if (EntityRemap[group.OwnerEntityIndex] == -1) continue;
                if (group.MeshId != TargetMeshId) continue;

                var slice = OldLane.AsArray().GetSubArray(group.MatrixStartIndex, group.MatrixCount);
                NewLane.AddRange(slice);
            }
        }
    }
}