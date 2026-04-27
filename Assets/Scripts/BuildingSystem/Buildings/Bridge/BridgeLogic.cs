using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Test.BuildingSystem
{
    public static class BridgeLogic
    {
        //Edit logic
        private const float EditThreshold = 2.0f;

        public static bool IsEditablePoint(BuildingSystemData data, BridgeExtras extras, Vector3 pos)
        {
            return GetTargetPointIndex(extras, pos) != -1;
        }

        public static void ConvertEditPoints(BuildingSystemData data, BridgeExtras extras, Vector3 start, Vector3 current, out Vector3 a, out Vector3 b)
        {
            a = extras.Start;
            b = extras.End;

            int targetIndex = GetTargetPointIndex(extras, start);

            if (targetIndex == 0)
            {
                a = current;
            }
            else if (targetIndex == 1)
            {
                b = current;
            }
        }

        private static int GetTargetPointIndex(BridgeExtras extras, Vector3 pos)
        {
            float dStart = Vector3.Distance(pos, extras.Start);
            float dEnd = Vector3.Distance(pos, extras.End);

            if (dStart < dEnd && dStart < EditThreshold) return 0;
            if (dEnd < EditThreshold) return 1;

            return -1;
        }

        //Overlap
        public static bool CheckOverLap(BuildingSystemData data, BridgeSettings settings, Vector3 start, Vector3 end)
        {
            var grid = data.Grid;

            var dist = Vector3.Distance(start, end);

            if (dist < settings.StartLen + settings.EndLen) return false;

            var steps = Mathf.CeilToInt(dist / (grid.CellSize * 0.5f));

            for (var i = 0; i <= steps; i++)
            {
                var pos = Vector3.Lerp(start, end, (float)i / steps);
                var cell = grid.WorldToGrid(pos);

                if (grid.Map.ContainsKey(cell))
                {
                    return false;
                }
            }
            return true;
        }
        

        //Extras
        public static int CreateExtras(BuildingSystemData data, int id, Vector3 start, Vector3 end)
        {
            var resultIndex = data.BridgeExtras.Length;
            data.BridgeExtras.Add(new BridgeExtras { Start = start, End = end });
            return resultIndex;
        }

        //Job
        public static void ExecuteJob(BridgeSettings settings, Vector3 start, Vector3 end, ref BuildingPreview preview)
        {
            var job = new CalculateBridgeJob
            {
                StartPos = start,
                EndPos = end,
                Settings = settings,
                OutMatrices = preview.AllMatrices,
                OutBatches = preview.Batches
            };

            job.Run();
        }

        //Grid
        public static void RegisterInGrid(BuildingSystemData data, int id, BridgeExtras extras)
        {
            ApplyToGrid(data, id, extras, true);
        }

        public static void RemoveFromGrid(BuildingSystemData data, int id, BridgeExtras extras)
        {
            ApplyToGrid(data, id, extras, false);
        }

        private static void ApplyToGrid(BuildingSystemData data, int id, BridgeExtras extras, bool isAdding)
        {
            var grid = data.Grid;
            var dir = extras.End - extras.Start;
            var length = dir.magnitude;

            if (length < 1e-6f)
            {
                var cell = grid.WorldToGrid(extras.Start);
                if (isAdding) grid.Map.Add(cell, id);
                else grid.Map.Remove(cell, id);
                return;
            }

            var stepSize = grid.CellSize * 0.5f;
            var steps = Mathf.CeilToInt(length / stepSize);
            var stepDir = dir / length;

            for (var i = 0; i <= steps; i++)
            {
                var dist = Mathf.Min(i * stepSize, length);
                var pos = extras.Start + stepDir * dist;
                var cell = grid.WorldToGrid(pos);

                if (isAdding) grid.Map.Add(cell, id);
                else grid.Map.Remove(cell, id);
            }
        }
    }
}