using System.Collections.Generic;
using UnityEngine;

namespace Test.BuildingSystem
{
    [CreateAssetMenu(fileName = "DrawerRegistry", menuName = "BuildingSystem/DrawerRegistry")]
    public class DrawerRegistry : ScriptableObject
    {
        [Header("Materials")]
        [SerializeField] private Material _ghostMaterial;
        [SerializeField] private Material _normalMaterial;

        [Header("Meshes")]
        [SerializeField] private List<Mesh> _meshAssets;
        public int GetMeshId(Mesh mesh) => _meshAssets.IndexOf(mesh);

        public Material GhostMaterial => _ghostMaterial;
        public Material NormalMaterial => _normalMaterial;
        public IReadOnlyList<Mesh> MeshAssets => _meshAssets;
    }
}