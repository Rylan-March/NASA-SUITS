using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.XR.CompositionLayers.Emulation.Implementations;
using System.Linq;
using Unity.XR.CompositionLayers.Layers;

#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
#endif

namespace Unity.XR.CompositionLayers.Emulation
{

#if UNITY_EDITOR
    /// <summary>
    /// This class used to emulate a Projection Rig CompositionLayer inside of all scene view windows.
    /// </summary>
    [ExecuteInEditMode]
    internal class SceneEmulatedProjectionRig : MonoBehaviour, IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public CommandBuffer CommandBufffer => m_sceneViewProjector?.CommandBuffer;
        static Dictionary<Camera, Dictionary<int, SceneEmulatedProjectionRig>> cameraToRigs = new Dictionary<Camera, Dictionary<int, SceneEmulatedProjectionRig>>();
        Camera m_SceneViewCamera;
        SceneViewProjector m_sceneViewProjector;
        CompositionLayer m_CompositionLayer;
        int m_CompositionLayerId;

        /// <summary>
        /// If no rig currently exists in the cache then this creates a gameObject and adds the Camera + SceneEmulatedProjectionRig components to it.
        /// Newly created rigs are then added to the cache.
        /// </summary>
        /// <param name="compositionLayer">The composition layer that will be emulated.</param>
        /// <param name="sceneViewCamera">The scene camera to send projections to.</param>
        /// <param name="layerName">The layer that the rig camera will render.</param>
        /// <returns>Reference to the SceneEmulatedProjectionRig component.</returns>
        internal static SceneEmulatedProjectionRig CreateOrGet(CompositionLayer compositionLayer, Camera sceneViewCamera)
        {
            var compositionLayerId = compositionLayer.GetInstanceID();

            if (!cameraToRigs.ContainsKey(sceneViewCamera))
                cameraToRigs.Add(sceneViewCamera, new Dictionary<int, SceneEmulatedProjectionRig>());

            if (cameraToRigs[sceneViewCamera].ContainsKey(compositionLayerId))
            {
                // Update the Culling Mask of the camera to match the layer of it's target Composition Layer.
                // Used mainly when a new Projection Layer is created and the target layer is updated.
                var currentRig = cameraToRigs[sceneViewCamera][compositionLayerId];
                var currentRigCamera = currentRig.m_sceneViewProjector.SourceCamera;

                int mask = 1 << compositionLayer.gameObject.layer;
                if (currentRigCamera != null && currentRigCamera.cullingMask != mask)
                {
                    currentRigCamera.cullingMask = mask;
                }

                return cameraToRigs[sceneViewCamera][compositionLayerId];
            }

            var name = sceneViewCamera.name + "_" + compositionLayerId;
            var gameObject = new GameObject(name);
            gameObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            var rig = gameObject.AddComponent<SceneEmulatedProjectionRig>();
            rig.m_SceneViewCamera = sceneViewCamera;
            rig.m_CompositionLayerId = compositionLayerId;
            rig.m_CompositionLayer = compositionLayer;
            var rigCamera = rig.CreateRigCamera(gameObject, LayerMask.LayerToName(compositionLayer.gameObject.layer));
            rig.m_sceneViewProjector = new SceneViewProjector(rigCamera, sceneViewCamera);

            cameraToRigs[sceneViewCamera].Add(compositionLayerId, rig);

            return rig;
        }

        public void ClearAndAddCommand() => m_sceneViewProjector?.ClearAndAddCommand();

#if UNITY_RENDER_PIPELINES_UNIVERSAL_RENDERGRAPH
        public void ClearAndAddCommand(RasterCommandBuffer rcb) => m_sceneViewProjector?.ClearAndAddCommand(rcb);
        public void ClearAndAddCommand(CommandBuffer cb) => m_sceneViewProjector?.ClearAndAddCommand(cb);

#endif

        public static void DestroyAllEmulatedRigsForLayerId(int layerInstanceId)
        {
            for (int i = 0; i < cameraToRigs.Keys.Count; ++i)
            {
                var idToRigs = cameraToRigs.ElementAt(i).Value;
                if (idToRigs.ContainsKey(layerInstanceId))
                {
                    var rig = idToRigs[layerInstanceId];
                    DestroyImmediate(rig.gameObject);
                }
            }
        }

        void Update()
        {
            // If the player is being built, don't update the SceneViewProjector.
            if (BuildPipeline.isBuildingPlayer)
                return;

            // LayerData should be disposed when it's Composition Layer goes out of scope.
            // However, currently it does not call Dispose() during an undo action, so this object must track the CompositionLayer reference.
            if (m_CompositionLayer == null || m_SceneViewCamera == null)
            {
                DestroyImmediate(gameObject);
                return;
            }

            m_sceneViewProjector?.UpdateResources();
        }

        Camera CreateRigCamera(GameObject gameObject, string layerToRender)
        {
            var rigCamera = gameObject.AddComponent<Camera>();
#if !UNITY_RENDER_PIPELINES_UNIVERSAL
            rigCamera.stereoTargetEye = StereoTargetEyeMask.None;
#endif
            rigCamera.cullingMask = 1 << LayerMask.NameToLayer(layerToRender);
            rigCamera.clearFlags = CameraClearFlags.SolidColor;
            rigCamera.backgroundColor = new Color(0, 0, 0, 0);

            return rigCamera;
        }

        void RemoveRigFromSceneCamera(Camera camera)
        {
            if (cameraToRigs.ContainsKey(camera))
            {
                if (cameraToRigs[camera].ContainsKey(m_CompositionLayerId))
                    cameraToRigs[camera].Remove(m_CompositionLayerId);
            }

            if (cameraToRigs.Count == 0)
                cameraToRigs.Remove(camera);
        }

        /// <summary>
        /// Subscribe to the scene closing event to cleanup when a scene is closed.
        /// </summary>
        void OnEnable()
        {
            EditorSceneManager.sceneClosing += OnSceneClosing;
        }

        /// <summary>
        /// Cleanup for when this object is destroyed.
        /// There are three ways this object may be destroyed:
        ///     m_SceneViewCamera goes out of scope.
        ///     m_CompositionLayer goes out of scope.
        ///     ProjectionRigEmulationLayerData calls DestroyAllEmulatedRigsForLayerId() when disposed.
        /// </summary>
        void OnDestroy()
        {
            m_CompositionLayer = null;
            RemoveRigFromSceneCamera(m_SceneViewCamera);
        }

        /// <summary>
        /// Cleanup for when this object is destroyed.
        /// There are three ways this object may be destroyed:
        ///     m_SceneViewCamera goes out of scope.
        ///     m_CompositionLayer goes out of scope.
        ///     ProjectionRigEmulationLayerData calls DestroyAllEmulatedRigsForLayerId() when disposed.
        /// </summary>
        void OnDisable()
        {
            EditorSceneManager.sceneClosing -= OnSceneClosing;

            if (m_sceneViewProjector != null)
            {
                m_sceneViewProjector.Dispose();
            }
        }

        private void OnSceneClosing(Scene scene, bool removingScene)
        {
            DiposeAllSceneViewProjectors();
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            DiposeAllSceneViewProjectors();
        }

        /// <summary>
        /// Disposes of all SceneViewProjectors
        /// </summary>
        private void DiposeAllSceneViewProjectors()
        {
            foreach (var cameraToRig in cameraToRigs)
                foreach (var rig in cameraToRig.Value)
                    rig.Value.m_sceneViewProjector.Dispose();
        }

        /// <summary>
        /// This class inherits from ProjectionEmulator and syncs the source camera with the provided scene view camera.
        /// </summary>
        class SceneViewProjector : ProjectionEmulator
        {
            public SceneViewProjector(Camera sourceCamera, Camera sceneViewCamera) : base(sourceCamera, sceneViewCamera)
            {
                SceneView.duringSceneGui += OnDuringSceneGui;

                // During the first frame of play mode, the scene camera is set to the origin.
                // Disable the camera for that first frame.
                m_sourceCamera.enabled = false;

                // Re-enable the camera after the first frame.
                EditorApplication.delayCall += () =>
                {
                    if (m_sourceCamera != null)
                        m_sourceCamera.enabled = true;
                };
            }

            ~SceneViewProjector() => SceneView.duringSceneGui -= OnDuringSceneGui;

            void SyncWithSceneCamera()
            {
                if (m_sourceCamera.orthographic != m_destinationCamera.orthographic)
                    m_sourceCamera.orthographic = m_destinationCamera.orthographic;

                m_sourceCamera.transform.position = m_destinationCamera.transform.position;
                m_sourceCamera.transform.rotation = m_destinationCamera.transform.rotation;
                m_sourceCamera.orthographicSize = m_destinationCamera.orthographicSize;
                m_sourceCamera.fieldOfView = m_destinationCamera.fieldOfView;
            }

            void OnDuringSceneGui(SceneView sceneView)
            {
                if (m_sourceCamera == null || m_destinationCamera == null)
                {
                    Dispose();
                    SceneView.duringSceneGui -= OnDuringSceneGui;
                    return;
                }

                if (sceneView.camera == m_destinationCamera)
                    SyncWithSceneCamera();
            }
        }

        /// <summary>
        /// This class used for emulating projection, but doesn't actually add the command buffer to the destination camera (depends on the EmulatedLayerProvider to add the command buffer).
        /// </summary>
        class ProjectionEmulator : IDisposable
        {
            const float k_ProjectionMeshScale = 1.0f;
            const int k_DepthBufferBits = 24;

            public CommandBuffer CommandBuffer => m_commandBuffer;
            public Camera SourceCamera => m_sourceCamera;

            protected Camera m_sourceCamera;
            protected Camera m_destinationCamera;
            protected CommandBuffer m_commandBuffer;
            protected RenderTexture m_sourceRenderTexture;
            protected Material m_sourceRenderMaterial;
            protected Mesh m_sourceProjectionMesh;
            bool m_disposed;

            public ProjectionEmulator(Camera sourceCamera, Camera destinationCamera, int nnum = 0)
            {
                m_destinationCamera = destinationCamera;
                m_sourceCamera = sourceCamera;
                CreateMaterial();
                UpdateRenderTextureScale();
                CreateCommandBuffer();
            }

            public virtual void UpdateResources()
            {
                if (m_sourceCamera == null || m_destinationCamera == null)
                    return;

                UpdateRenderTextureScale();
            }

            public void ClearAndAddCommand()
            {
                m_commandBuffer.Clear();
                m_commandBuffer.DrawMesh(m_sourceProjectionMesh, Matrix4x4.identity, m_sourceRenderMaterial);
            }

#if UNITY_RENDER_PIPELINES_UNIVERSAL_RENDERGRAPH
            public void ClearAndAddCommand(RasterCommandBuffer rcb)
            {
                rcb?.DrawMesh(m_sourceProjectionMesh, Matrix4x4.identity, m_sourceRenderMaterial);
            }

            public void ClearAndAddCommand(CommandBuffer cb)
            {
                cb?.Clear();
                cb?.DrawMesh(m_sourceProjectionMesh, Matrix4x4.identity, m_sourceRenderMaterial);
            }
#endif

            public void Dispose()
            {
                if (m_disposed)
                    return;

                if (m_sourceCamera != null)
                {
                    m_sourceCamera.targetTexture = null;
                    if (m_sourceCamera.gameObject.activeInHierarchy)
                        CompositionLayerHelper.DestroyCamera(m_sourceCamera);
                }

                if (m_sourceProjectionMesh != null)
                    m_sourceProjectionMesh.Clear();

                if (m_commandBuffer != null)
                    m_commandBuffer.Clear();

                if (m_sourceRenderTexture != null)
                {
                    m_sourceRenderTexture.Release();
                    GameObject.DestroyImmediate(m_sourceRenderTexture);
                }

                Material.DestroyImmediate(m_sourceRenderMaterial);

                m_disposed = true;
            }

            protected void UpdateRenderTextureScale()
            {
                if (m_sourceRenderTexture?.width == m_destinationCamera.pixelWidth &&
                    m_sourceRenderTexture?.height == m_destinationCamera.pixelHeight)
                    return;

                if (m_sourceRenderTexture != null)
                {
                    m_sourceCamera.targetTexture = null;
                    m_sourceRenderTexture.Release();
                    GameObject.DestroyImmediate(m_sourceRenderTexture);
                }

                m_sourceRenderTexture = new RenderTexture(m_destinationCamera.pixelWidth, m_destinationCamera.pixelHeight, k_DepthBufferBits) { name = "SceneProjection" + "_source" };
                m_sourceRenderTexture.Create();
                m_sourceCamera.targetTexture = m_sourceRenderTexture;
                m_sourceRenderMaterial.SetTexture(EmulatedLayerData.k_MainTex, m_sourceCamera.targetTexture);
            }

            void CreateMaterial()
            {
                Shader emulationShader = Shader.Find(CompositionLayerConstants.UberShader);
                m_sourceRenderMaterial = new Material(emulationShader);
                m_sourceRenderMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                m_sourceRenderMaterial.EnableKeyword(ProjectionRigEmulationLayerData.k_ShaderLayerTypeKeyword);
                m_sourceRenderMaterial.EnableKeyword(EmulatedLayerData.k_SourceTextureMaterialKeyword);
            }

            void CreateCommandBuffer()
            {
                m_sourceProjectionMesh = EmulatedMeshLayerData.GeneratePlaneMesh(k_ProjectionMeshScale);
                m_commandBuffer = new CommandBuffer();
                m_commandBuffer.name = nameof(ProjectionEmulator);
                ClearAndAddCommand();
            }

        }
    }
#endif
}
