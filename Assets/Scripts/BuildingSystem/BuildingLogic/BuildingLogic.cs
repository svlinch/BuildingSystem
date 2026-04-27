using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Test.BuildingSystem
{
    public struct BuildingPreview
    {
        public NativeList<Matrix4x4> AllMatrices; //example for a bridge: 0 = start, 1-2 = extensions, 3-6 = mids, 7 = end
        public NativeList<PreviewBatch> Batches;
        public bool IsValid;

        public void Clear()
        {
            AllMatrices.Clear();
            Batches.Clear();
        }

        public void Dispose()
        {
            AllMatrices.Dispose();
            Batches.Dispose();
        }
    }

    public struct PreviewBatch
    {
        public int MeshId;
        public int StartIndex;
        public int Count;
    }

    public partial class BuildingLogic : BuildingLogic.ICommonContext
    {
        private readonly ICursorController _ui;
        private readonly BuildingSystemData _data;
        private readonly BuildingRegistry _buildReg;
        private BuildingPreview _preview;

        BuildingSystemData ICommonContext.Data => _data;
        BuildingRegistry ICommonContext.Registry => _buildReg;
        ref BuildingPreview ICommonContext.Preview => ref _preview;
        ICursorController ICommonContext.CursorController => _ui;

        private Dictionary<StateType, State> _states;
        private State _currentState;
        public StateType CurrentState => _currentState.Type;

        public BuildingLogic(BuildingSystemData data, BuildingRegistry buildReg, ICursorController ui)
        {
            _data = data;
            _ui = ui;
            _buildReg = buildReg;

            initializePreview();
            initializeStates();

            void initializePreview()
            {
                _preview.AllMatrices = new NativeList<Matrix4x4>(500, Allocator.Persistent);
                _preview.Batches = new NativeList<PreviewBatch>(10, Allocator.Persistent);
            }

            void initializeStates()
            {
                _states = new Dictionary<StateType, State>();
                _states.Add(StateType.Idle, new IdleState(this));
                _states.Add(StateType.Build, new BuildingState(this));
                _states.Add(StateType.Edit, new EditState(this));
                _states.Add(StateType.Delete, new DeleteState(this));
                _currentState = _states[StateType.Idle];
            }
        }

        public BuildingPreview HandleUpdate(InputData input)
        {
            if (input.MouseDown)
            {
                var cell = _data.Grid.WorldToGrid(input.CurrentPosition);
                if (input.ShiftPressed)
                {
                    ChangeState(StateType.Delete);
                }
                else if (_data.Grid.Map.TryGetFirstValue(cell, out var targetId, out var it))
                {
                    do
                    {
                        if (BuildingDataOperations.CheckForEditPoint(_data, targetId, input.CurrentPosition))
                        {
                            ChangeState(StateType.Edit, new EditStateInputParameters
                            {
                                TargetId = targetId,
                                FixedPoint = input.CurrentPosition
                            });
                            return _preview;
                        }
                    }
                    while (_data.Grid.Map.TryGetNextValue(out targetId, ref it));
                }
                else
                {
                    ChangeState(StateType.Build);
                }
            }

            _currentState?.HandleUpdate(input);
            return _preview;
        }

        public void ChangeState(StateType newState, IStateInputParameters input = null)
        {
            if (_currentState.Type == newState) return;
            _currentState?.Exit();
            _currentState = _states[newState];
            _currentState.Enter(input);
        }

        public void Dispose()
        {
            _preview.Dispose();
        }
    }
}