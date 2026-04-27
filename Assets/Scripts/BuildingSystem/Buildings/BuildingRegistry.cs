using System;
using System.Collections.Generic;
using UnityEngine;

namespace Test.BuildingSystem
{
    [Serializable]
    public abstract class BuildingDataSO : ScriptableObject
    {
        [SerializeField] private BuildingType _type;
        public BuildingType Type => _type;
        public abstract void Initialize(DrawerRegistry drawReg);
    }

    [CreateAssetMenu(fileName = "BuildingRegistry", menuName = "BuildingSystem/BuildingRegistry")]
    public class BuildingRegistry : ScriptableObject
    {
        [SerializeField] private List<BuildingDataSO> _buildings; 
        private Dictionary<BuildingType, BuildingDataSO> _cache;

        public void Initialize(DrawerRegistry drawRegistry)
        {
            _cache = new Dictionary<BuildingType, BuildingDataSO>();

            foreach (var building in _buildings)
            {
                _cache[building.Type] = building;
                building.Initialize(drawRegistry);
            }
        }

        public T GetData<T>(BuildingType type) where T : BuildingDataSO
        {
            if (_cache != null && _cache.TryGetValue(type, out var data))
            {
                return data as T;
            }

            return _buildings.Find(x => x.Type == type) as T;
        }
    }
}