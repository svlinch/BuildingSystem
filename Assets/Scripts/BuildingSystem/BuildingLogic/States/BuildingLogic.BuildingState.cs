namespace Test.BuildingSystem
{
    public partial class BuildingLogic
    {
        private class BuildingStateInputParameters : IStateInputParameters
        {
            public BuildingType Type;
        }

        private class BuildingState : State
        {
            private BuildingType _buildingType;
            public override StateType Type => StateType.Build;
            public BuildingState(ICommonContext context) : base(context) { }
            public override void HandleUpdate(InputData input)
            {
                if (input.MouseHold)
                {
                    _context.Preview.Clear();

                    _context.Preview.IsValid = BuildingTypedLogic.CheckOverlap(_context.Data, _context.Registry, _buildingType, input.StartPosition, input.CurrentPosition);
                    BuildingTypedLogic.ExecutePreviewJob(_context.Registry, _buildingType, input.StartPosition, input.CurrentPosition, ref _context.Preview);
                }

                if (input.MouseUp)
                {
                    var canCommit = BuildingTypedLogic.CheckOverlap(_context.Data, _context.Registry, _buildingType, input.StartPosition, input.CurrentPosition);
                    if (canCommit)
                    {
                        BuildingDataOperations.CommitBuilding(_context.Data, _context.Preview, _buildingType, input.StartPosition, input.CurrentPosition);
                    }
                    _context.Preview.Clear();
                    _context.ChangeState(StateType.Idle);
                }
            }

            public override void Enter(IStateInputParameters input)
            {
                if (input is BuildingStateInputParameters parameters)
                {
                    _buildingType = parameters.Type;
                    _context.CursorController.SetCursorColor(StateType.Build);
                }
            }

            public override void Exit()
            {
            }
        }
    }
}