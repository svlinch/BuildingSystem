using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Test.BuildingSystem
{
    [BurstCompile]
    public struct CalculateBridgeJob : IJob
    {
        public Vector3 StartPos;
        public Vector3 EndPos;
        public BridgeSettings Settings;
        public NativeList<Matrix4x4> OutMatrices;
        public NativeList<PreviewBatch> OutBatches;

        public void Execute()
        {
            var diff = EndPos - StartPos;
            var dist = diff.magnitude;
            if (dist < 0.01f) return;

            var dir = diff / dist;
            var rot = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 90, 0);

            //smallest bridge
            if (dist <= (Settings.StartLen + Settings.EndLen))
            {
                AddBatch(Settings.StartId, StartPos, rot, Vector3.one);
                AddBatch(Settings.EndId, EndPos, rot, Vector3.one);
                return;
            }

            var workDist = dist - (Settings.StartLen + Settings.EndLen);

            //how many mids
            int midCount = (int)Mathf.Round(workDist / Mathf.Max(Settings.MidLen, 0.1f));

            if (midCount * Settings.MidLen > workDist)
            {
                midCount--;
            }

            var totalMidDist = midCount * Settings.MidLen;

            //fillers space
            var gapPerSide = (workDist - totalMidDist) * 0.5f;

            //Start
            AddBatch(Settings.StartId, StartPos, rot, Vector3.one);
            var currentPtr = Settings.StartLen;

            //Left fillers if possible
            if (gapPerSide > 0.001f)
            {
                currentPtr = AddFillers(StartPos, dir, rot, currentPtr, gapPerSide, ref OutMatrices, ref OutBatches);
            }

            //Mids
            if (midCount > 0)
            {
                var midStartIdx = OutMatrices.Length;
                for (int i = 0; i < midCount; i++)
                {
                    var pos = StartPos + dir * (currentPtr + Settings.MidLen * 0.5f);
                    OutMatrices.Add(Matrix4x4.TRS(pos, rot, Vector3.one));
                    currentPtr += Settings.MidLen;
                }
                OutBatches.Add(new PreviewBatch { MeshId = Settings.MidId, StartIndex = midStartIdx, Count = midCount });
            }

            //Right fillers if possible
            if (gapPerSide > 0.001f)
            {
                currentPtr = AddFillers(StartPos, dir, rot, currentPtr, gapPerSide, ref OutMatrices, ref OutBatches);
            }

            //End
            AddBatch(Settings.EndId, EndPos, rot, Vector3.one);
        }

        //batches
        private void AddBatch(int meshId, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            var idx = OutMatrices.Length;
            OutMatrices.Add(Matrix4x4.TRS(pos, rot, scale));
            OutBatches.Add(new PreviewBatch { MeshId = meshId, StartIndex = idx, Count = 1 });
        }

        //fillers helper
        private float AddFillers(Vector3 start, Vector3 dir, Quaternion rot, float currentPtr, float totalGap,
                                 ref NativeList<Matrix4x4> matrices, ref NativeList<PreviewBatch> batches)
        {
            var count = Mathf.Max(1, (int)Mathf.Ceil(totalGap / Settings.FillLen));
            var step = totalGap / count;
            var stretch = step / Settings.FillLen;

            int startIdx = matrices.Length;
            for (int i = 0; i < count; i++)
            {
                var pos = start + dir * (currentPtr + step * 0.5f);
                //some scale if it does not fits
                matrices.Add(Matrix4x4.TRS(pos, rot, new Vector3(stretch, 1, 1)));
                currentPtr += step;
            }
            batches.Add(new PreviewBatch { MeshId = Settings.FillId, StartIndex = startIdx, Count = count });
            return currentPtr;
        }
    }
}