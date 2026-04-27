using UnityEngine;

namespace Test.BuildingSystem
{
    public static class BuildingTypedLogic
    {
        public static void ExecutePreviewJob(BuildingRegistry buildReg, BuildingType type, Vector3 start, Vector3 end, ref BuildingPreview preview)
        {
            switch (type)
            {
                case BuildingType.Bridge:
                    var settings = buildReg.GetData<BridgeConfig>(BuildingType.Bridge).Settings;
                    BridgeLogic.ExecuteJob(settings, start, end, ref preview); break;
            }
        }

        public static int AddExtrasForBuilding(BuildingSystemData data, BuildingType type, int ownerId, Vector3 start, Vector3 end)
        {
            switch (type)
            {
                case BuildingType.Bridge: return BridgeLogic.CreateExtras(data, ownerId, start, end);
                default: throw new System.Exception($"Extra logic for object of type {type} is not exist!");
            }
        }

        public static void RegisterInGrid(BuildingSystemData data, BuildingType type, int ownerId, int extrasIndex)
        {
            switch (type)
            {
                case BuildingType.Bridge:
                    var extras = data.BridgeExtras[extrasIndex];
                    BridgeLogic.RegisterInGrid(data, ownerId, extras); break;
            }
        }

        public static void RemoveFromGrid(BuildingSystemData data, BuildingType type, int buildingId, int extrasIndex)
        {
            switch (type)
            {
                case BuildingType.Bridge:
                    var extras = data.BridgeExtras[extrasIndex];
                    BridgeLogic.RemoveFromGrid(data, buildingId, extras);
                    break;
            }
        }

        public static bool CheckOverlap(BuildingSystemData data, BuildingRegistry buildReg, BuildingType type, Vector3 start, Vector3 end)
        {
            switch (type)
            {
                case BuildingType.Bridge:
                    var settings = buildReg.GetData<BridgeConfig>(BuildingType.Bridge).Settings;
                    return BridgeLogic.CheckOverLap(data, settings, start, end);
                default: return false;
            }
        }

        public static bool IsEditablePoint(BuildingSystemData data, BuildingType type, int extrasIndex, Vector3 pos)
        {
            switch (type)
            {
                case BuildingType.Bridge:
                    var extras = data.BridgeExtras[extrasIndex];
                    return BridgeLogic.IsEditablePoint(data, extras, pos);
                default: return false;
            }
        }

        public static void ConvertInputForEdit(BuildingSystemData data, int id, Vector3 start, Vector3 current,
                                            out Vector3 a, out Vector3 b)
        {
            var entity = data.Entities[data.IdToEntityIndex[id]];
            switch (entity.Type)
            {
                case BuildingType.Bridge:
                    var extras = data.BridgeExtras[entity.ExtraDataIndex];
                    BridgeLogic.ConvertEditPoints(data, extras, start, current, out a, out b);
                    break;
                default:
                    a = start;
                    b = current;
                    return;
            }
        }
    }
}