using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Test.BuildingSystem
{
    public class SpatialGrid
    {
        private float _cellSize;
        private NativeParallelMultiHashMap<int2, int> _map;

        public SpatialGrid(float size)
        {
            _cellSize = size;
            _map = new NativeParallelMultiHashMap<int2, int>(1000, Allocator.Persistent);
        }

        public NativeParallelMultiHashMap<int2, int> Map => _map;
        public float CellSize => _cellSize;

        public int2 WorldToGrid(Vector3 pos) => new int2(Mathf.FloorToInt(pos.x / _cellSize), Mathf.FloorToInt(pos.z / _cellSize));

        public void Dispose()
        {
            if (_map.IsCreated) _map.Dispose();
        }
    }
}