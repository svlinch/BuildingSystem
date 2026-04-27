using UnityEngine;

namespace Test.BuildingSystem
{
    public class BuildingSystem
    {
        private readonly InputSystem _inputSystem;
        private readonly BuildingSystemData _buildingData;
        private readonly BuildingLogic _buildingLogic;
        private readonly GPUDrawer _gpuDrawer;

        private CompactionTransaction _currentTransaction;

        public BuildingSystem(DrawerRegistry drawReg, BuildingRegistry buildReg, ICursorController ui)
        {
            _inputSystem = new InputSystem();
            _buildingData = new BuildingSystemData(drawReg);
            _buildingLogic = new BuildingLogic(_buildingData, buildReg, ui);
            _gpuDrawer = new GPUDrawer(drawReg);
        }

        public void HandleUpdate()
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Handle.Complete();
                _currentTransaction.ApplyTo(_buildingData);
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }

            var inputData = _inputSystem.HandleUpdate();
            var previewData = _buildingLogic.HandleUpdate(inputData);
            _gpuDrawer.RenderPreview(previewData);
            _gpuDrawer.Render(_buildingData);

            if (_buildingLogic.CurrentState == StateType.Idle && _buildingData.NeedsCompaction())
            {
                _currentTransaction = BuildingDataCompaction.ScheduleCompaction(_buildingData);
            }
        }

        public void Dispose()
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Handle.Complete();
                _currentTransaction.Dispose();
            }

            _buildingLogic.Dispose();
            _buildingData.Dispose();
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            if (_buildingData == null || _buildingData.Grid == null) return;

            Gizmos.color = Color.cyan;
            var size = _buildingData.Grid.CellSize;
            for (var x = -50f; x < 50f; x += size)
            {
                for (var z = -50f; z < 50f; z += size)
                {
                    Gizmos.DrawWireCube(new Vector3(x + size / 2, 0, z + size / 2), new Vector3(size, 0.01f, size));
                }
            }

            Gizmos.color = Color.red;
            foreach (var entry in _buildingData.Grid.Map)
            {
                var pos = new Vector3(entry.Key.x * size + size / 2, 0.1f, entry.Key.y * size + size / 2);
                Gizmos.DrawSphere(pos, 0.2f);
            }
        }
#endif
    }
}