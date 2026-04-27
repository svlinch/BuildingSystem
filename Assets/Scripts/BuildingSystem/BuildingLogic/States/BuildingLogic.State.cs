namespace Test.BuildingSystem
{
    public partial class BuildingLogic
    {
        private interface ICommonContext
        {
            BuildingSystemData Data { get; }
            BuildingRegistry Registry { get; }
            ref BuildingPreview Preview { get; }
            void ChangeState(StateType type, IStateInputParameters parameters = null);
            ICursorController CursorController { get; }
        }


        public interface IStateInputParameters { }

        private abstract class State
        {
            protected ICommonContext _context;
            public State(ICommonContext context)
            {
                _context = context;
            }

            public abstract StateType Type { get; }
            public virtual void HandleUpdate(InputData input) { }
            public virtual void Enter(IStateInputParameters input) { }
            public virtual void Exit() { }
        }
    }
    public enum StateType
    {
        Idle,
        Build,
        Edit,
        Delete
    }
}