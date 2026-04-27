using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Test.BuildingSystem
{
    // Rebuilds entity + group layout, recalculating matrix offsets per mesh lane
    [BurstCompile]
    public struct CompactionMainJob : IJob
    {
        [ReadOnly] public NativeList<Building> OldEntities;
        [ReadOnly] public NativeList<InstanceGroup> OldGroups;

        public float Time;
        public NativeReference<int> DeleteCounter;
        public NativeParallelHashMap<int, int> IdToEntityIndex;
        public NativeList<Building> NewEntities;
        public NativeList<InstanceGroup> NewGroups;
        public NativeArray<int> EntityRemap;

        public NativeArray<int> LaneOffsets;

        public void Execute()
        {
            IdToEntityIndex.Clear();
            for (int i = 0; i < LaneOffsets.Length; i++) LaneOffsets[i] = 0;

            for (int i = 0; i < OldEntities.Length; i++)
            {
                var ent = OldEntities[i];

                var isDead = ent.DeathStartTime > 0;
                var readyToRemove = isDead && (Time - ent.DeathStartTime >= 0.5f);

                if (readyToRemove)
                {
                    DeleteCounter.Value++;
                    EntityRemap[i] = -1;
                    continue;
                }

                var newIdx = NewEntities.Length;
                EntityRemap[i] = newIdx;

                var oldGroupStart = ent.FirstGroupIndex;
                ent.FirstGroupIndex = NewGroups.Length;

                for (int g = 0; g < ent.GroupCount; g++)
                {
                    var group = OldGroups[oldGroupStart + g];

                    var newGroup = group;
                    newGroup.MatrixStartIndex = LaneOffsets[group.MeshId];
                    LaneOffsets[group.MeshId] += group.MatrixCount;

                    newGroup.OwnerEntityIndex = newIdx;
                    NewGroups.Add(newGroup);
                }

                IdToEntityIndex.Add(ent.Id, newIdx);
                NewEntities.Add(ent);
            }
        }
    }
}