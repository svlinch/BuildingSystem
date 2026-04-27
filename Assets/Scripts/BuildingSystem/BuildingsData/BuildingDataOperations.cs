using UnityEngine;

namespace Test.BuildingSystem
{
    public static class BuildingDataOperations
    {
        public static void CommitBuilding(BuildingSystemData data, BuildingPreview preview, BuildingType type, Vector3 start, Vector3 end)
        {
            if (preview.AllMatrices.Length == 0) return;

            //create entity
            var buildingId = data.IdCounter++;
            var extrasIndex = BuildingTypedLogic.AddExtrasForBuilding(data, type, buildingId, start, end);
            var entityIndex = data.Entities.Length;

            var building = new Building
            {
                Id = buildingId,
                Type = type,
                DeathStartTime = -1f,
                FirstGroupIndex = data.InstanceGroups.Length,
                GroupCount = preview.Batches.Length,
                ExtraDataIndex = extrasIndex
            };

            //registration
            data.IdToEntityIndex.Add(buildingId, entityIndex);
            data.Entities.Add(building);

            //render data
            var matrices = preview.AllMatrices.AsArray();
            for (int i = 0; i < preview.Batches.Length; i++)
            {
                var batch = preview.Batches[i];
                var lane = data.RenderLanes[batch.MeshId];

                data.InstanceGroups.Add(new InstanceGroup
                {
                    OwnerEntityIndex = entityIndex,
                    MeshId = batch.MeshId,
                    MatrixStartIndex = lane.Length,
                    MatrixCount = batch.Count
                });

                for (int m = 0; m < batch.Count; m++)
                {
                    var matrix = matrices[batch.StartIndex + m];
                    matrix.m32 = Time.time; //spawn time
                    matrix.m30 = -1f;       //death time
                    lane.Add(matrix);
                }
            }

            BuildingTypedLogic.RegisterInGrid(data, type, buildingId, extrasIndex);
        }

        public static void RemoveBuilding(BuildingSystemData data, int id, float instantly = 0f)
        {
            if (!data.IdToEntityIndex.TryGetValue(id, out int entityIdx)) return;

            var building = data.Entities[entityIdx];
            if (building.DeathStartTime > 0f) return;

            data.DeleteCounter++;
            building.DeathStartTime = Time.time;
            data.Entities[entityIdx] = building;

            for (int i = 0; i < building.GroupCount; i++)
            {
                var group = data.InstanceGroups[building.FirstGroupIndex + i];
                var lane = data.RenderLanes[group.MeshId];

                for (int j = 0; j < group.MatrixCount; j++)
                {
                    var matrix = lane[group.MatrixStartIndex + j];
                    matrix.m30 = building.DeathStartTime;
                    matrix.m31 = instantly; //1 = instantly, 0 = animated

                    lane[group.MatrixStartIndex + j] = matrix;
                }
            }

            BuildingTypedLogic.RemoveFromGrid(data, building.Type, id, building.ExtraDataIndex);
        }

        public static void RestoreBuilding(BuildingSystemData data, BuildingSnapshot snapshot)
        {
            if (!data.IdToEntityIndex.TryGetValue(snapshot.EntityId, out var entIdx)) return;
            var ent = data.Entities[entIdx];

            data.DeleteCounter--;
            ent.DeathStartTime = -1f;
            data.Entities[entIdx] = ent;

            var snapshotMatrixPtr = 0;
            for (var i = 0; i < ent.GroupCount; i++)
            {
                var group = data.InstanceGroups[ent.FirstGroupIndex + i];
                var lane = data.RenderLanes[group.MeshId];

                for (var j = 0; j < group.MatrixCount; j++)
                {
                    lane[group.MatrixStartIndex + j] = snapshot.StoredMatrices[snapshotMatrixPtr++];
                }
            }

            BuildingTypedLogic.RegisterInGrid(data, ent.Type, ent.Id, ent.ExtraDataIndex);
        }

        public static bool CheckForEditPoint(BuildingSystemData data, int id, Vector3 pos)
        {
            if (!data.IdToEntityIndex.TryGetValue(id, out int entIdx)) return false;
            var building = data.Entities[entIdx];
            return BuildingTypedLogic.IsEditablePoint(data, building.Type, building.ExtraDataIndex, pos);
        }
    }
}