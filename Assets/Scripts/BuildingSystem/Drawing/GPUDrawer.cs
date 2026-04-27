using UnityEngine;
using UnityEngine.Rendering;

namespace Test.BuildingSystem
{
    public class GPUDrawer
    {
        private readonly RenderParams _previewParams;
        private readonly RenderParams _normalParams;
        private readonly DrawerRegistry _drawerRegistry;

        public GPUDrawer(DrawerRegistry registry)
        {
            _drawerRegistry = registry;

            _previewParams = new RenderParams(_drawerRegistry.GhostMaterial)
            {
                matProps = new MaterialPropertyBlock(),
                worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000),
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false,
                lightProbeUsage = LightProbeUsage.Off,
                reflectionProbeUsage = ReflectionProbeUsage.Off
            };

            _normalParams = new RenderParams(_drawerRegistry.NormalMaterial)
            {
                worldBounds = new Bounds(Vector3.zero, Vector3.one * 10000),
                shadowCastingMode = ShadowCastingMode.On,
                receiveShadows = true
            };
        }

        public void Render(BuildingSystemData data)
        {
            for (int i = 0; i < data.RenderLanes.Length; i++)
            {
                var meshId = i;
                var matrices = data.RenderLanes[i].AsArray();
                var count = data.RenderLanes[i].Length;

                if (count == 0) continue;

                Graphics.RenderMeshInstanced(_normalParams, _drawerRegistry.MeshAssets[meshId], 0, matrices, count);
            }
        }

        public void RenderPreview(BuildingPreview preview)
        {
            if (!preview.AllMatrices.IsCreated || preview.Batches.Length == 0) return;

            _previewParams.matProps.SetColor("_BaseColor", preview.IsValid ? Color.green : Color.red);

            var fullArray = preview.AllMatrices.AsArray();

            for (int i = 0; i < preview.Batches.Length; i++)
            {
                var batch = preview.Batches[i];

                if (batch.Count == 0) continue;

                Graphics.RenderMeshInstanced(_previewParams, _drawerRegistry.MeshAssets[batch.MeshId],
                                             0, fullArray, batch.Count, batch.StartIndex);
            }
        }
    }
}