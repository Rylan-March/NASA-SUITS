#if UNITY_EDITOR
#define SUPPORT_SETMIRRORBLITMODE_ONGUI
#endif

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Rendering;
using UnityEngine.XR.CompositionLayers.Rendering.Internals;
using Unity.XR.CompositionLayers.Services;
using System.Collections;

namespace Unity.XR.CompositionLayers.Rendering
{
    /// <summary>
    /// XR mirror view rendering on XR Composition Layers.
    /// This component works on BiRP, URP and HDRP.
    /// Call XRDisplaySystem.SetPreferredMirrorBlitMode() to set the mirror blit mode.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(32000)]
    public class MirrorViewRenderer : MonoBehaviour
    {
        const CameraEvent TargetCameraEvent = CameraEvent.AfterEverything;

        /// <summary>
        /// AlphaMode.Opaque works fine in most cases.
        /// AlphaMode.Alpha or AlphaMode.Premultily will be used for overlay drawing. (Need the alpha channel on back buffers.)
        /// </summary>
        public ImageFilters.AlphaMode AlphaMode = ImageFilters.AlphaMode.Opaque;

        Camera m_Camera;
        CommandBuffer m_CommandBuffer;
        bool m_IsAddedCommandBufferToCamera;
        XRDisplaySubsystem m_DisplaySubsystem;
        int m_MirrorBlitMode;

        ImageFilters.RenderPipeline m_RenderPipeline;

        void Start()
        {
            UpdateMirrorView();

            StartCoroutine(PostfixMirrorBlitMode());
        }

        IEnumerator PostfixMirrorBlitMode()
        {
            for (; ; )
            {
                yield return new WaitForEndOfFrame();
#if !SUPPORT_SETMIRRORBLITMODE_ONGUI
                if (m_DisplaySubsystem != null && m_DisplaySubsystem.running)
                {
                    m_DisplaySubsystem.SetPreferredMirrorBlitMode(m_MirrorBlitMode);
                }
#endif
            }
        }

        void LateUpdate()
        {
            UpdateMirrorView();
            UpdateCommandBuffer();

#if !SUPPORT_SETMIRRORBLITMODE_ONGUI
            if (m_DisplaySubsystem != null)
            {
                m_DisplaySubsystem.SetPreferredMirrorBlitMode(XRMirrorViewBlitMode.None);
            }
#endif
        }

        void OnDestroy()
        {
            CleanupMirrorView();

            m_Camera = null;
            m_CommandBuffer = null;
            m_DisplaySubsystem = null;
        }

        void OnEnable()
        {
            UpdateMirrorView();
            UpdateCommandBuffer();

            if (m_DisplaySubsystem == null)
            {
                if (XRHelper.GetActiveDisplaySubsystem() == null)
                {
                    if (Application.isPlaying)
                    {
                        Debug.LogWarning("MirrorViewRenderer works with XR mode only.");
                    }
                }
            }
        }

        void OnDisable()
        {
            CleanupMirrorView();
        }

        void UpdateMirrorView()
        {
            if (m_Camera == null)
            {
                m_Camera = GetComponent<Camera>();
                if (m_Camera != null)
                {
                    m_Camera.clearFlags = CameraClearFlags.Nothing;
                    m_Camera.renderingPath = RenderingPath.Forward; // Supports build-in render pipeline only.
                    m_RenderPipeline = ImageFilters.RenderPipeline.Builtin;
                    if (GraphicsSettings.currentRenderPipeline != null)
                    {
#if UNITY_RENDER_PIPELINES_UNIVERSAL
                        var universalAdditionalCameraData = m_Camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                        if (universalAdditionalCameraData != null)
                        {
                            universalAdditionalCameraData.allowXRRendering = false;
                            if (RenderPipelineUtility.IsUniversalRenderPipeline())
                            {
                                m_RenderPipeline = ImageFilters.RenderPipeline.Universal;
                            }
                        }
#endif
#if UNITY_RENDER_PIPELINES_HDRENDER
                        var hdAdditionalCameraData = m_Camera.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                        if (hdAdditionalCameraData != null)
                        {
                            hdAdditionalCameraData.xrRendering = false;
                            if (RenderPipelineUtility.IsHDRenderPipeline())
                            {
                                m_RenderPipeline = ImageFilters.RenderPipeline.HighDefinition;
                            }
                        }
#endif
                    }
                    else
                    {
                        m_Camera.stereoTargetEye = StereoTargetEyeMask.None;
                    }

                    m_Camera.cullingMask = 0;
                }
            }

            var displaySystem = XRHelper.GetActiveDisplaySubsystem();
            if (displaySystem == null)
            {
                CleanupMirrorView();
                return;
            }

            var blitMode = displaySystem.GetPreferredMirrorBlitMode();
            if (m_DisplaySubsystem == null)
            {
                if (blitMode == XRMirrorViewBlitMode.None)
                {
                    CleanupMirrorView();
                    return;
                }

                m_MirrorBlitMode = blitMode;
            }
            else
            {
#if SUPPORT_SETMIRRORBLITMODE_ONGUI
                m_MirrorBlitMode = blitMode;
#else
                if (blitMode != XRMirrorViewBlitMode.None)
                {
                    m_MirrorBlitMode = blitMode;
                }
#endif
            }

            if (m_DisplaySubsystem == displaySystem)
            {
                return;
            }

            m_DisplaySubsystem = displaySystem;

            AddCommandBufferToCamera();

            if (m_Camera != null)
            {
                m_Camera.clearFlags = CameraClearFlags.SolidColor;
                m_Camera.backgroundColor = Color.black;
                m_Camera.enabled = true;
            }
        }

        void CleanupMirrorView()
        {
            if (m_Camera != null)
            {
                m_Camera.clearFlags = CameraClearFlags.Nothing;
                m_Camera.enabled = false;
            }

            RemoveCommandBufferFromCamera();

            if (m_DisplaySubsystem != null)
            {
                if (m_DisplaySubsystem == XRHelper.GetActiveDisplaySubsystem())
                {
                    m_DisplaySubsystem.SetPreferredMirrorBlitMode(m_MirrorBlitMode);
                }

                m_DisplaySubsystem = null;
            }
        }

        void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == m_Camera)
            {
                if (m_RenderPipeline == ImageFilters.RenderPipeline.Universal)
                    Graphics.ExecuteCommandBuffer(m_CommandBuffer);
                else
                    context.ExecuteCommandBuffer(m_CommandBuffer);
            }
        }

        void AddCommandBufferToCamera()
        {
            if (m_Camera != null && !m_IsAddedCommandBufferToCamera)
            {
                m_CommandBuffer = new CommandBuffer();
                m_CommandBuffer.name = "MirrorViewRenderer";
                m_IsAddedCommandBufferToCamera = true;
                if (GraphicsSettings.currentRenderPipeline != null)
                    RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
                else
                    m_Camera.AddCommandBuffer(TargetCameraEvent, m_CommandBuffer);
            }
        }

        void RemoveCommandBufferFromCamera()
        {
            if (m_Camera != null && m_IsAddedCommandBufferToCamera)
            {
                m_IsAddedCommandBufferToCamera = false;
                if (GraphicsSettings.currentRenderPipeline != null)
                    RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
                else
                    m_Camera.RemoveCommandBuffer(TargetCameraEvent, m_CommandBuffer);
                m_CommandBuffer = null;
            }
        }

        void UpdateCommandBuffer()
        {
            if (m_CommandBuffer == null)
            {
                return;
            }

            m_CommandBuffer.Clear();

            var mirrorViewParams = new ImageFilters.MirrorViewParams();
            mirrorViewParams.displaySubsystem = m_DisplaySubsystem;
            mirrorViewParams.mainCamera = CompositionLayerUtils.GetStereoMainCamera();
            mirrorViewParams.mirrorViewCamera = m_Camera;
            mirrorViewParams.blitMode = m_MirrorBlitMode == XRMirrorViewBlitMode.Default ? XRMirrorViewBlitMode.SideBySide : m_MirrorBlitMode;
            mirrorViewParams.alphaMode = AlphaMode;
            mirrorViewParams.renderPipeline = m_RenderPipeline;

            ImageFilters.BlitMirrorView(m_CommandBuffer, mirrorViewParams);
        }
    }
}
