using System;
using Unity.XR.CompositionLayers.Extensions;
using Unity.XR.CompositionLayers.Layers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using UnityEditor;
using Unity.XR.CoreUtils;
using Unity.XR.CompositionLayers.Services;

namespace Unity.XR.CompositionLayers.Emulation.Implementations
{
    /// <summary>
    /// Base for emulating <see cref="ProjectionLayerRigData"/>. Used to emulate a full screen texture rendering to the compositor.
    /// </summary>
    [EmulatedLayerDataType(typeof(ProjectionLayerRigData))]
    internal class ProjectionRigEmulationLayerData : EmulatedMeshLayerData
    {
        public const string k_ShaderLayerTypeKeyword = "COMPOSITION_LAYERTYPE_PROJECTION";

        public const int k_LayerIndexAfterDefaults = 7;

        Camera MainCamera => CompositionLayerManager.mainCameraCache;

        // Caches the last supported camera so this object knows which camera it's preparing command buffers for.
        Camera currentSupportedCamera;

        // Caches cameras and textures for left and right eyes
        internal Camera leftCam;
        internal Camera rightCam;
        RenderTexture m_emulationLeftEyeTexture;
        RenderTexture m_emulationRightEyeTexture;

        bool exitingPlayMode;

#if !UNITY_RENDER_PIPELINES_UNIVERSAL_RENDERGRAPH
        // Caches a command buffer to use during play mode.
        CommandBuffer m_playModeCommandBuffer = new CommandBuffer();
        int m_cachedLayer = -1;
#endif

        public ProjectionRigEmulationLayerData()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        protected internal override void UpdateEmulatedLayerData()
        {
            base.UpdateEmulatedLayerData();

            if (MainCamera == null)
                return;

            //re-create render texture if detected gameview window size changed
            if (m_emulationLeftEyeTexture == null || m_emulationRightEyeTexture == null || m_emulationLeftEyeTexture.width != MainCamera.pixelWidth || m_emulationLeftEyeTexture.height != MainCamera.pixelHeight)
            {
                CreateAndSetRenderTexture(MainCamera.pixelWidth, MainCamera.pixelHeight, 32);
            }

            if (leftCam == null || rightCam == null)
                return;

            if (!Application.isPlaying)
            {
                // Ensure that the projection rig cameras have the same local pose offset as the main camera.
                var parent = leftCam.transform.parent;
                if (parent != null)
                {
                    parent.SetWorldPose(GetTotalLocalPoseOffset(MainCamera.transform));
                    leftCam.transform.SetLocalPose(Pose.identity);
                    rightCam.transform.SetLocalPose(Pose.identity);
                }
            }

            if (Application.isPlaying && !XRSettings.isDeviceActive) //For emulation cases only when using no headset: project rig cams follow mainCam transform.
            {
                leftCam.transform.SetPositionAndRotation(MainCamera.transform.position, MainCamera.transform.rotation);
                rightCam.transform.SetPositionAndRotation(MainCamera.transform.position, MainCamera.transform.rotation);
            }
        }

        private Pose GetTotalLocalPoseOffset(Transform currentTransform)
        {
            var totalLocalPoseOffset = currentTransform.GetLocalPose();

            while (currentTransform.parent != null)
            {
                var parentLocalPose = currentTransform.parent.GetLocalPose();
                totalLocalPoseOffset = new Pose(totalLocalPoseOffset.position + parentLocalPose.position, totalLocalPoseOffset.rotation * parentLocalPose.rotation);
                currentTransform = currentTransform.parent;
            }

            return totalLocalPoseOffset;
        }

        private void CreateAndSetRenderTexture(int width, int height, int depth)
        {
            var layer = CompositionLayer;

            if (leftCam == null && rightCam == null)
            {
                leftCam = layer.gameObject.transform.GetChild(0).GetComponent<Camera>();
                rightCam = layer.gameObject.transform.GetChild(1).GetComponent<Camera>();
            }
            leftCam.targetTexture = null;
            rightCam.targetTexture = null;

            // Left Eye Emulation Texture.
            if (m_emulationLeftEyeTexture != null)
            {
                m_emulationLeftEyeTexture.Release();
                UnityEngine.Object.DestroyImmediate(m_emulationLeftEyeTexture);
            }
            m_emulationLeftEyeTexture = new RenderTexture((int)width, (int)height, depth)
            {
                name = layer.name + "_left",
                format = RenderTextureFormat.ARGB32,
                depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat
            };
            m_emulationLeftEyeTexture.Create();

            // Right Eye Emulation Texture
            if (m_emulationRightEyeTexture != null)
            {
                m_emulationRightEyeTexture.Release();
                UnityEngine.Object.DestroyImmediate(m_emulationRightEyeTexture);
            }
            m_emulationRightEyeTexture = new RenderTexture((int)width, (int)height, depth)
            {
                name = layer.name + "_right",
                format = RenderTextureFormat.ARGB32,
                depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat
            };
            m_emulationRightEyeTexture.Create();

            if (layer != null)
            {
                var textExt = layer.GetComponent<TexturesExtension>();
                if (textExt != null)
                {
                    textExt.LeftTexture = m_emulationLeftEyeTexture;
                    textExt.RightTexture = m_emulationRightEyeTexture;
                }
                if (leftCam == null && rightCam == null)
                {
                    leftCam = layer.gameObject.transform.GetChild(0).GetComponent<Camera>();
                    rightCam = layer.gameObject.transform.GetChild(1).GetComponent<Camera>();
                }
                leftCam.targetTexture = m_emulationLeftEyeTexture;
                rightCam.targetTexture = m_emulationRightEyeTexture;
            }
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            exitingPlayMode = state == PlayModeStateChange.ExitingPlayMode;
        }
#endif
        private void DestroyRig()
        {
#if UNITY_EDITOR
            // Don't destroy the layer inside of play mode
            // as all changes will be reverted on unplay
            if (exitingPlayMode) return;

            if (CompositionLayer == null) return;

            var layer = CompositionLayer;
            var textExt = layer.GetComponent<TexturesExtension>();

            if (layer == null || textExt == null) return;

            // Remove references on the TextureExtension
            if (textExt != null)
            {
                textExt.LeftTexture = null;
                textExt.RightTexture = null;

                // Show the TextureExtension component
                textExt.hideFlags = HideFlags.None;
            }

            // Destroy the cameras
            if (leftCam != null)
            {
                leftCam.targetTexture = null;
                GameObject.DestroyImmediate(leftCam.gameObject);
            }

            if (rightCam != null)
            {
                rightCam.targetTexture = null;
                GameObject.DestroyImmediate(rightCam.gameObject);
            }

            // Release render textures
            if (m_emulationLeftEyeTexture != null)
            {
                m_emulationLeftEyeTexture.Release();
                UnityEngine.Object.DestroyImmediate(m_emulationLeftEyeTexture);
                m_emulationLeftEyeTexture = null;
            }

            if (m_emulationRightEyeTexture != null)
            {
                m_emulationRightEyeTexture.Release();
                UnityEngine.Object.DestroyImmediate(m_emulationRightEyeTexture);
                m_emulationRightEyeTexture = null;
            }

            // Reset to default layer
            Transform.gameObject.layer = 0;
#endif
        }

        /// <summary>
        /// There are three ways this object may be Disposed
        ///     The layer is changed from it's current type.
        ///     The layer is destroyed or deleted.
        ///     Playmode is exited.
        /// </summary>
        public override void Dispose()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            DestroyRig();
            SceneEmulatedProjectionRig.DestroyAllEmulatedRigsForLayerId(CompositionLayer.GetInstanceID());
#endif
            base.Dispose();
        }

        protected override void PrepareCommands()
        {
            if (currentSupportedCamera == null)
                return;

            if (currentSupportedCamera.cameraType == CameraType.SceneView)
            {
                // If IsInvalidatedCommandBuffer is true and the command buffer is already initialized then AddCommands() won't be called.
                // Set the command buffer at this stage so this object sends the correct buffer to the appropriate scene camera.
                if (!m_CommandBufferTempSceneView.IsInvalidated)
                    SetCommandBufferToSceneEmulatedRigBuffer();
            }
            else
            {
                base.PrepareCommands();
                if (!m_CommandBufferTemp.IsInvalidated)
                    SetCommandBufferToPlayModeBuffer();
            }
        }

        protected override void AddCommands(RenderContext renderContext, CommandArgs commandArgs)
        {
            if (currentSupportedCamera == null)
                return;
            if (currentSupportedCamera.cameraType == CameraType.SceneView)
            {
                SetCommandBufferToSceneEmulatedRigBuffer(renderContext);
            }
            else
                SetCommandBufferToPlayModeBuffer(renderContext);
        }

        /// <inheritdoc/>
        public override bool IsSupported(Camera camera)
        {
            if (camera.cameraType == CameraType.SceneView)
            {
                currentSupportedCamera = camera;
                return true;
            }

            var isSupported = !Application.isPlaying;
#if ENABLE_UNITY_VR
            isSupported = isSupported || !XRSettings.isDeviceActive;
#endif
            isSupported &= camera == CompositionLayerManager.mainCameraCache;
            if (isSupported)
                currentSupportedCamera = camera;
            else
                currentSupportedCamera = null;
            return isSupported;
        }

        protected override string GetShaderLayerTypeKeyword()
        {
            return k_ShaderLayerTypeKeyword;
        }

        protected override void UpdateMesh(ref Mesh mesh)
        {
            if (mesh == null)
            {
                mesh = GeneratePlaneMesh(1.0f);
            }
        }

        void SetCommandBufferToSceneEmulatedRigBuffer(RenderContext renderContext = default)
        {
#if UNITY_EDITOR
            // Don't create a command buffer if we're building a player.
            if (BuildPipeline.isBuildingPlayer)
                return;

            var rig = SceneEmulatedProjectionRig.CreateOrGet(CompositionLayer, currentSupportedCamera);

            // Unity 6000+ uses this path when on URP.
#if UNITY_RENDER_PIPELINES_UNIVERSAL_RENDERGRAPH

            if (renderContext.m_CommandBuffer != null)
                rig.ClearAndAddCommand(renderContext.m_CommandBuffer);

            if (renderContext.m_RasterCommandBuffer != null)
                rig.ClearAndAddCommand(renderContext.m_RasterCommandBuffer);
#else
            rig.ClearAndAddCommand();
            m_CommandBufferTempSceneView = new CommandBufferTemp { CommandBuffer = rig.CommandBufffer, IsInvalidated = false };
            if (CompositionLayer.gameObject.layer > k_LayerIndexAfterDefaults)
                m_cachedLayer = CompositionLayer.gameObject.layer;
#endif
#endif

        }

        void SetCommandBufferToPlayModeBuffer(RenderContext renderContext = default)
        {
            // Unity 6000+ uses this path when on URP.
#if UNITY_RENDER_PIPELINES_UNIVERSAL_RENDERGRAPH
            if (renderContext.m_CommandBuffer != null)
                base.AddCommands(renderContext.m_CommandBuffer, new CommandArgs { IsSceneView = false });

            if (renderContext.m_RasterCommandBuffer != null)
                base.AddCommands(renderContext.m_RasterCommandBuffer, new CommandArgs { IsSceneView = false });
#else
            m_playModeCommandBuffer.Clear();
            base.AddCommands(m_playModeCommandBuffer, new CommandArgs { IsSceneView = false });
            m_CommandBufferTemp = new CommandBufferTemp { CommandBuffer = m_playModeCommandBuffer, IsInvalidated = false };
#endif

        }

    }
}
