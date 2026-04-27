using Unity.Collections;
using UnityEngine;

namespace Test.BuildingSystem
{
    public struct BuildingSnapshot
    {
        public int EntityId;
        public BuildingType Type;
        public NativeList<Matrix4x4> StoredMatrices;

        public void Dispose()
        {
            if (StoredMatrices.IsCreated) StoredMatrices.Dispose();
        }
    }

    public partial class BuildingLogic
    {
        private class EditStateInputParameters : IStateInputParameters
        {
            public int TargetId;
            public Vector3 FixedPoint;
        }

        private class EditState : State
        {
            private EditStateInputParameters _parameters;
            private BuildingSnapshot _snapshot;
            private bool _committed;

            public override StateType Type => StateType.Edit;

            public EditState(ICommonContext context) : base(context) { }

            public override void Enter(IStateInputParameters input)
            {
                if (input is EditStateInputParameters parameters)
                {
                    _parameters = parameters;
                    _committed = false;
                    _snapshot = _context.Data.GetBuildingSnapshot(parameters.TargetId);
                    BuildingDataOperations.RemoveBuilding(_context.Data, parameters.TargetId, 1f);
                    _context.CursorController.SetCursorColor(StateType.Edit);
                }
            }

            public override void HandleUpdate(InputData input)
            {
                _context.Preview.Clear();

                BuildingTypedLogic.ConvertInputForEdit(_context.Data, _snapshot.EntityId, _parameters.FixedPoint,
                                                    input.CurrentPosition, out var pointA, out var pointB);
                _context.Preview.IsValid = BuildingTypedLogic.CheckOverlap(_context.Data, _context.Registry, _snapshot.Type, pointA, pointB);

                BuildingTypedLogic.ExecutePreviewJob(_context.Registry, _snapshot.Type, pointA, pointB, ref _context.Preview);

                if (input.MouseUp)
                {
                    if (_context.Preview.IsValid)
                    {
                        BuildingDataOperations.CommitBuilding(_context.Data, _context.Preview, _snapshot.Type, pointA, pointB);
                        _committed = true;
                    }
                    _context.ChangeState(StateType.Idle);
                }
            }

            public override void Exit()
            {
                if (!_committed)
                {
                    BuildingDataOperations.RestoreBuilding(_context.Data, _snapshot);
                }
                _snapshot.Dispose();
                _context.Preview.Clear();
            }
        }
    }
}