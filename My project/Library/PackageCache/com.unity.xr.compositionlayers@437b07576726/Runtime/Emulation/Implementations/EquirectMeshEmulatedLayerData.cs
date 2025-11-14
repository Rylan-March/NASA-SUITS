using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CompositionLayers;
using Unity.XR.CompositionLayers.Emulation;
using Unity.XR.CompositionLayers.Emulation.Implementations;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CoreUtils;
using UnityEngine.Rendering;

namespace UnityEngine.XR.CompositionLayers.Emulation.Implementations
{
    [EmulatedLayerDataType(typeof(EquirectMeshLayerData))]
    internal class EquirectMeshEmulatedLayerData : EmulatedMeshLayerData
    {
        static readonly int k_CentralHorizontalAngle = Shader.PropertyToID("_centralHorizontalAngle");
        static readonly int k_UpperVerticalAngle = Shader.PropertyToID("_upperVerticalAngle");
        static readonly int k_LowerVerticalAngle = Shader.PropertyToID("_lowerVerticalAngle");

        Vector3[] m_SourceVertices = Array.Empty<Vector3>();
        float m_Size = 1.0f;
        float m_GeneratedSize = 0.0f;

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
            return "COMPOSITION_LAYERTYPE_EQUIRECT";
        }

        protected internal override void UpdateEmulatedLayerData()
        {
            base.UpdateEmulatedLayerData();

            var material = EmulationMaterial;
            if (material == null)
            {
                return;
            }

            var horizontal = Mathf.PI * 2f;
            var upperVertical = Mathf.PI * 0.5f;
            var lowerVertical = -Mathf.PI * 0.5f;

            if (LayerData is EquirectMeshLayerData equirectMeshLayerData)
            {
                const float inverseTwoPi = 1 / (2f * Mathf.PI);
                horizontal = equirectMeshLayerData.CentralHorizontalAngle * 0.5f * inverseTwoPi;
                upperVertical = equirectMeshLayerData.UpperVerticalAngle * inverseTwoPi;
                lowerVertical = equirectMeshLayerData.LowerVerticalAngle * inverseTwoPi;
            }

            material.SetFloat(k_CentralHorizontalAngle, horizontal);
            material.SetFloat(k_UpperVerticalAngle, upperVertical);
            material.SetFloat(k_LowerVerticalAngle, lowerVertical);
        }

        protected override void UpdateMesh(ref Mesh mesh)
        {
            m_Size = 1f;

            if (LayerData is EquirectMeshLayerData equirectMeshLayerData)
            {
                m_Size = equirectMeshLayerData.Radius * 2f;
            }

            if (m_SourceVertices.Length == 0 || mesh == null)
            {
                var sphereMesh = Resources.GetBuiltinResource<Mesh>("New-Sphere.fbx");
                if (sphereMesh == null)
                {
                    Debug.LogError("GetBuiltinResource<Mesh>(\"New-Sphere.fbx\") Failed.");
                    return;
                }

                mesh = new Mesh();
                m_SourceVertices = sphereMesh.vertices;
                mesh.vertices = GetScaledVertices(m_SourceVertices, m_Size);
                mesh.uv = sphereMesh.uv;
                mesh.triangles = sphereMesh.triangles.Reverse().ToArray();
                mesh.RecalculateNormals();
                mesh.UploadMeshData(false);
                m_GeneratedSize = m_Size;
            }
            else
            {
                if (m_GeneratedSize != m_Size)
                {
                    m_GeneratedSize = m_Size;
                    mesh.vertices = GetScaledVertices(m_SourceVertices, m_Size);
                    mesh.UploadMeshData(false);
                }
            }
        }
    }
}
