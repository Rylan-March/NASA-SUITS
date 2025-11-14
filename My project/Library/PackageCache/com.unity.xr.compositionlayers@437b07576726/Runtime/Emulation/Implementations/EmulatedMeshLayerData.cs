using UnityEngine;
using UnityEngine.Rendering;
using Unity.XR.CoreUtils; // GetWorldPose()
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CompositionLayers.Services;

namespace Unity.XR.CompositionLayers.Emulation.Implementations
{
    internal abstract class EmulatedMeshLayerData : EmulatedLayerData
    {
        Mesh m_Mesh;
        Matrix4x4 m_TransformMatrix;
        protected MeshCollider MeshCollider
        {
            get
            {
                if (m_MeshCollider == null)
                    CompositionLayer.gameObject.TryGetComponent<MeshCollider>(out m_MeshCollider);
                return m_MeshCollider;
            }
        }

        protected MeshCollider m_MeshCollider;

        CustomTransformData m_CustomTransformData;

#if UNITY_EDITOR
        MaterialPropertyBlock m_MaterialPropertyBlockSceneView;
        CustomTransformData m_CustomTransformDataInSceneView;
#endif

        protected override string GetShaderLayerTypeKeyword()
        {
            return "COMPOSITION_LAYERTYPE_LAYER";
        }

        protected virtual void UpdateMesh(ref Mesh mesh)
        {
            if (MeshCollider != null)
            {
                MeshCollider.sharedMesh = mesh;
            }
        }

        protected override void PrepareCommands()
        {
            bool isCreated = m_Mesh == null;
            var platformProvider = PlatformManager.ActivePlatformProvider;
            bool isEnabledCustomTransform = platformProvider.IsEnabledCustomTransform(this.CompositionLayer);
            var customTransformCameraData = new CustomTransformCameraData();
            var customTransformData = CustomTransformData.Default;
            if (isEnabledCustomTransform)
            {
                customTransformCameraData.MainCamera = Services.CompositionLayerUtils.GetStereoMainCamera();
                customTransformData = platformProvider.GetCustomTransformData(this.CompositionLayer, customTransformCameraData);
            }
            else
            {
                if (this.Transform != null)
                    customTransformData.SetWorldPoseMatrix(this.Transform);
            }

#if UNITY_EDITOR
            var customTransformDataInSceneView = m_CustomTransformData;
            if (isEnabledCustomTransform)
            {
                customTransformCameraData.IsSceneView = true;
                customTransformDataInSceneView = platformProvider.GetCustomTransformData(this.CompositionLayer, customTransformCameraData);
            }
            if (m_MaterialPropertyBlockSceneView == null || m_CustomTransformDataInSceneView != customTransformDataInSceneView)
            {
                if (m_MaterialPropertyBlockSceneView == null)
                    m_MaterialPropertyBlockSceneView = new MaterialPropertyBlock();
                m_CommandBufferTemp.IsInvalidated = true;
                m_CommandBufferTempSceneView.IsInvalidated = true;

                m_CustomTransformDataInSceneView = customTransformDataInSceneView;
                m_MaterialPropertyBlockSceneView.SetMatrix(k_TransformMatrix, customTransformDataInSceneView.Matrix);
                m_MaterialPropertyBlockSceneView.SetInteger(k_TransformMatrixType, (int)customTransformDataInSceneView.MatrixType);
            }
#endif

            if (isCreated || m_CustomTransformData != customTransformData)
            {
                m_CommandBufferTemp.IsInvalidated = true;
                m_CommandBufferTempSceneView.IsInvalidated = true;
                m_CustomTransformData = customTransformData;
                if (EmulationMaterial != null)
                {
                    EmulationMaterial.SetMatrix(k_TransformMatrix, customTransformData.Matrix);
                    EmulationMaterial.SetInteger(k_TransformMatrixType, (int)customTransformData.MatrixType);
                }
            }

            UpdateMesh(ref m_Mesh);
        }

        protected override void AddCommands(RenderContext renderContext, CommandArgs commandArgs)
        {
            if (EmulationMaterial == null)
                return;

            MaterialPropertyBlock properties = null;
            Matrix4x4 transformMatrix = m_CustomTransformData.Matrix;
#if UNITY_EDITOR
            if (commandArgs.IsSceneView)
            {
                properties = m_MaterialPropertyBlockSceneView;
                transformMatrix = m_CustomTransformDataInSceneView.Matrix;
            }
#endif
            renderContext.DrawMesh(m_Mesh, transformMatrix, EmulationMaterial, 0, -1, properties);
        }

        public override void Dispose()
        {
#if UNITY_EDITOR
            m_MaterialPropertyBlockSceneView = null;
#endif
            m_Mesh = null;
            base.Dispose();
        }

        //----------------------------------------------------------------------------------------------------------------------------------------
        // Plane mesh implementations
        //----------------------------------------------------------------------------------------------------------------------------------------

        protected internal static Mesh GeneratePlaneMesh(float scale)
        {
            return GeneratePlaneMesh(new Vector2(scale, scale));
        }

        protected internal static Mesh GeneratePlaneMesh(Vector2 scale)
        {
            Mesh mesh = null;
            GeneratePlaneMesh(ref mesh, scale, true);
            return mesh;
        }

        protected internal static void GeneratePlaneMesh(ref Mesh mesh, float scale, bool markNoLongerReadable = false)
        {
            GeneratePlaneMesh(ref mesh, new Vector2(scale, scale), markNoLongerReadable);
        }

        protected internal static void GeneratePlaneMesh(ref Mesh mesh, Vector2 scale, bool markNoLongerReadable = false)
        {
            bool isNew = (mesh == null);
            if (isNew)
            {
                mesh = new Mesh();
                mesh.name = "Composition Layer Plane";
            }
            mesh.vertices = new Vector3[]
            {
                new(-scale.x, -scale.y, 0f),
                new(-scale.x, scale.y, 0f),
                new(scale.x, scale.y, 0f),
                new(scale.x, -scale.y, 0f)
            };

            // Note: These mesh setters (mesh.trialges,...) will cause an error without mesh.vertices.
            if (isNew)
            {
                mesh.triangles = new int[] { 0, 1, 2, 2, 3, 0 };
                mesh.normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
                mesh.uv = new Vector2[] { new(0f, 0f), new(0f, 1f), new(1f, 1f), new(1f, 0f) };
            }

            mesh.UploadMeshData(false);
        }

        //----------------------------------------------------------------------------------------------------------------------------------------
        // Utilities
        //----------------------------------------------------------------------------------------------------------------------------------------

        protected static Vector3[] GetScaledVertices(Vector3[] vertices, float scale)
        {
            if (vertices == null)
            {
                return null;
            }

            var length = vertices.Length;
            var newVertices = new Vector3[length];
            for (int i = 0; i < length; ++i)
            {
                newVertices[i] = vertices[i] * scale;
            }

            return newVertices;
        }

        protected static Vector3[] GetScaledVertices(Vector3[] vertices, Vector3 scale)
        {
            if (vertices == null)
            {
                return null;
            }

            var length = vertices.Length;
            var newVertices = new Vector3[length];
            for (int i = 0; i < length; ++i)
            {
                var v = vertices[i];
                newVertices[i] = new Vector3(v.x * scale.x, v.y * scale.y, v.z * scale.z);
            }

            return newVertices;
        }
    }
}
