using Unity.Collections;
using Unity.Mathematics;

namespace Test.BuildingSystem
{
    public partial class BuildingLogic
    {
        private class DeleteState : State
        {
            public DeleteState(ICommonContext context) : base(context) { }
            public override StateType Type => StateType.Delete;

            public override void HandleUpdate(InputData input)
            {
                if (input.MouseHold)
                {
                    int2 cell = _context.Data.Grid.WorldToGrid(input.CurrentPosition);

                    var toRemove = new NativeList<int>(Allocator.Temp);

                    if (_context.Data.Grid.Map.TryGetFirstValue(cell, out var id, out var it))
                    {
                        do
                        {
                            if (_context.Data.IdToEntityIndex.ContainsKey(id))
                            {
                                toRemove.Add(id);
                            }
                        }
                        while (_context.Data.Grid.Map.TryGetNextValue(out id, ref it));
                    }

                    for (int i = 0; i < toRemove.Length; i++)
                    {
                        BuildingDataOperations.RemoveBuilding(_context.Data, toRemove[i]);
                    }

                    toRemove.Dispose();
                }

                if (!input.MouseHold || !input.ShiftPressed)
                {
                    _context.ChangeState(StateType.Idle);
                }
            }

            public override void Enter(IStateInputParameters input)
            {
                _context.CursorController.SetCursorColor(StateType.Delete);
            }

            public override void Exit() { }
        }
    }
}