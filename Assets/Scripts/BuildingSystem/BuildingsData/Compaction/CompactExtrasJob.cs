using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Test.BuildingSystem
{
    [BurstCompile]
    public struct CompactExtrasJob<T> : IJob where T : unmanaged
    {
        public BuildingType TargetType;

        [ReadOnly] public NativeList<Building> OldEntities;
        [ReadOnly] public NativeArray<int> EntityRemap;
        [ReadOnly] public NativeList<T> OldExtras;

        public NativeList<T> NewExtras;
        public NativeList<Building> NewEntities;

        public void Execute()
        {
            for (int i = 0; i < OldEntities.Length; i++)
            {
                var newIdx = EntityRemap[i];
                if (newIdx == -1) continue;

                var ent = OldEntities[i];
                if (ent.Type == TargetType)
                {
                    var newEnt = NewEntities[newIdx];
                    newEnt.ExtraDataIndex = NewExtras.Length;
                    NewEntities[newIdx] = newEnt;

                    NewExtras.Add(OldExtras[ent.ExtraDataIndex]);
                }
            }
        }
    }
}