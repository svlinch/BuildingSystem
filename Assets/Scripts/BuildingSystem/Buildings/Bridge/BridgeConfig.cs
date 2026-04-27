using UnityEngine;

namespace Test.BuildingSystem
{
    [CreateAssetMenu(fileName = "BridgeConfig", menuName = "BuildingSystem/BridgeConfig")]
    public class BridgeConfig : BuildingDataSO
    {
        [SerializeField] private Mesh StartMesh, FillMesh, MidMesh, EndMesh;
        [SerializeField] private float StartLen, FillLen, MidLen, EndLen;

        public BridgeSettings Settings { get; private set; }

        public override void Initialize(DrawerRegistry registry)
        {
            Settings = new BridgeSettings
            {
                StartId = registry.GetMeshId(StartMesh),
                FillId = registry.GetMeshId(FillMesh),
                MidId = registry.GetMeshId(MidMesh),
                EndId = registry.GetMeshId(EndMesh),
                StartLen = StartLen,
                FillLen = FillLen,
                MidLen = MidLen,
                EndLen = EndLen
            };
        }
    }

    [System.Serializable]
    public struct BridgeSettings
    {
        public int StartId, FillId, MidId, EndId;
        public float StartLen, FillLen, MidLen, EndLen;
    }
}