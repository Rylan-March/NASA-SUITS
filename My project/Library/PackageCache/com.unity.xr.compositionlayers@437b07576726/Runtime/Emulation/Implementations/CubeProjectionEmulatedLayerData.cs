
using System;
using System.Linq;
using Unity.XR.CompositionLayers;
using Unity.XR.CompositionLayers.Emulation;
using Unity.XR.CompositionLayers.Emulation.Implementations;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CoreUtils;
using UnityEngine.Rendering;

namespace UnityEngine.XR.CompositionLayers.Emulation.Implementations
{
    [EmulatedLayerDataType(typeof(CubeProjectionLayerData))]
    internal class CubeProjectionEmulatedLayerData : EmulatedMeshLayerData
    {
        const float k_Size = 10000f;

        public override bool IsSupported(Camera camera)
        {
            if (camera.cameraType == CameraType.SceneView)
                return true;

            var isSupported = !Application.isPlaying;
#if ENABLE_UNITY_VR
            isSupported = isSupported || !XRSettings.isDeviceActive;
#endif
            return isSupported;
        }

        protected override string GetShaderLayerTypeKeyword()
        {
            return "COMPOSITION_LAYERTYPE_CUBEMAP";
        }

        protected override void UpdateMesh(ref Mesh mesh)
        {
            if (mesh == null)
            {
                var sphereMesh = Resources.GetBuiltinResource<Mesh>("New-Sphere.fbx");
                if (sphereMesh == null)
                {
                    Debug.LogError("GetBuiltinResource<Mesh>(\"New-Sphere.fbx\") Failed.");
                    return;
                }

                mesh = new Mesh();
                mesh.vertices = GetScaledVertices(sphereMesh.vertices, k_Size);
                mesh.uv = sphereMesh.uv;
                mesh.triangles = sphereMesh.triangles.Reverse().ToArray();
                mesh.RecalculateNormals();
                mesh.UploadMeshData(true);
            }
        }
    }
}
