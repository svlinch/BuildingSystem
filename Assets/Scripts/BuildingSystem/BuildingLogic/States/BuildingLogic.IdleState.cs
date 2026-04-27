namespace Test.BuildingSystem
{
    public partial class BuildingLogic
    {
        private class IdleState : State
        {
            public override StateType Type => StateType.Idle;

            public IdleState(ICommonContext context) : base(context) { }

            public override void HandleUpdate(InputData input)
            {
                var editPointFound = false;
                var cell = _context.Data.Grid.WorldToGrid(input.CurrentPosition);
                if (_context.Data.Grid.Map.TryGetFirstValue(cell, out var targetId, out var it))
                {
                    do
                    {
                        if (BuildingDataOperations.CheckForEditPoint(_context.Data, targetId, input.CurrentPosition))
                        {
                            editPointFound = true;
                            break;
                        }
                    }
                    while (_context.Data.Grid.Map.TryGetNextValue(out targetId, ref it));
                }
                _context.CursorController.SetCursorColor(editPointFound ? StateType.Edit : StateType.Idle);
            }

            public override void Enter(IStateInputParameters input)
            {
                _context.CursorController.SetCursorColor(StateType.Idle);
            }

            public override void Exit() { }
        }
    }
}