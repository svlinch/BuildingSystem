using UnityEngine;
using Test.BuildingSystem;

public class MainGameController : MonoBehaviour
{
    [SerializeField] private DrawerRegistry _drawRegistry;
    [SerializeField] private BuildingRegistry _buildRegistry;
    [SerializeField] private UICanvas _ui;

    private BuildingSystem _buildingSystem;

    private void Awake()
    {
        _buildRegistry.Initialize(_drawRegistry);
        _buildingSystem = new BuildingSystem(_drawRegistry, _buildRegistry, _ui);
    }

    private void Update()
    {
        _buildingSystem.HandleUpdate();
        _ui.HandleUpdate();
    }

    private void OnDestroy()
    {
        Clear();
    }

    private void OnApplicationQuit()
    {
        Clear();
    }

    private void Clear()
    {
        if (_buildingSystem != null)
        {
            _buildingSystem.Dispose();
            _buildingSystem = null;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_buildingSystem == null) return;
        _buildingSystem.OnDrawGizmos();
    }
#endif
}