#if !UNITY_RENDER_PIPELINES_UNIVERSAL && !UNITY_RENDER_PIPELINES_HDRENDER  && (UNITY_EDITOR || UNITY_STANDALONE)
using Unity.XR.CompositionLayers;
using Unity.XR.CompositionLayers.Emulation;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.XR.CompositionLayers.Extensions;
using Unity.XR.CompositionLayers.Services;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.XR.CompositionLayers.Rendering.Internals;

namespace Unity.XR.CompositionLayers.Emulation
{
    /// <summary>
    /// Emulates color scale and bias effects for both Game View and
    /// Scene View cameras in the Built-in Render Pipeline.
    /// </summary>
    [ExecuteInEditMode]
    internal class EmulatedColorScaleBias : MonoBehaviour
    {
        static readonly string k_ShaderName = "Hidden/XRCompositionLayers/ColorScaleBias";
        static readonly string k_CommandBufferName = "EmulatedColorScaleBias";
        static readonly int k_ScaleParameterName = Shader.PropertyToID("_ColorScale");
        static readonly int k_BiasParameterName = Shader.PropertyToID("_ColorBias");
        static readonly int k_MainTexParameterName = Shader.PropertyToID("_MainTex");

        Camera m_GameViewCamera;
        Material m_EmulationMaterial;
        CameraEvent m_CameraEvent = CameraEvent.AfterImageEffects;

        Dictionary<Camera, CommandBuffer> cameraCommandBuffers = new Dictionary<Camera, CommandBuffer>();
        Dictionary<Camera, RenderTexture> cameraRenderTextures = new Dictionary<Camera, RenderTexture>();

        static bool IsSupported(Camera camera) => EmulatedLayerProvider.IsSupported(camera);

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only initialization method that triggers after
        /// domain reloads to restore Scene View effect rendering.
        /// </summary>
        [InitializeOnLoadMethod]
        static void InitializeEditor()
        {
            // Force refresh after domain reload
            EditorApplication.delayCall += () =>
            {
                var instance = FindObjectOfType<EmulatedColorScaleBias>();
                if (instance != null) instance.ForceSceneViewRefresh();
            };
        }

        // <summary>
        /// Forces complete refresh of all Scene View cameras.
        /// Necessary for domain reloads and layout changes.
        /// </summary>
        void ForceSceneViewRefresh()
        {
            foreach (var sceneView in SceneView.sceneViews)
            {
                if (sceneView is SceneView sv && sv.camera != null)
                {
                    if (cameraCommandBuffers.TryGetValue(sv.camera, out var buffer))
                    {
                        sv.camera.RemoveCommandBuffer(m_CameraEvent, buffer);
                        cameraCommandBuffers.Remove(sv.camera);
                    }
                }
            }

            foreach (var sceneView in SceneView.sceneViews)
            {
                if (sceneView is SceneView sv && sv.camera != null)
                {
                    SetupCamera(sv.camera);
                    UpdateCommandBuffer(sv.camera);
                }
            }

            SceneView.RepaintAll();
        }
#endif

        /// <summary>
        /// Initializes the component, sets up main camera resources, and adds Scene View cameras for emulation.
        /// </summary>
        void OnEnable()
        {
            m_GameViewCamera = CompositionLayerManager.mainCameraCache;
            if (m_GameViewCamera == null)
                return;

            var shader = Shader.Find(k_ShaderName);
            if (shader != null)
                m_EmulationMaterial = new Material(shader);
            else
                Debug.LogError($"Failed to find shader '{k_ShaderName}' for Color Scale Bias emulation.");

            SetupCamera(m_GameViewCamera);
            UpdateCommandBuffer(m_GameViewCamera);

#if UNITY_EDITOR
            SceneView.duringSceneGui += OnSceneGUI;

            // Immediate setup for existing Scene Views
            foreach (var sceneView in SceneView.sceneViews)
            {
                if (sceneView is SceneView sv && sv.camera != null)
                {
                    SetupCamera(sv.camera);
                    UpdateCommandBuffer(sv.camera);
                }
            }

            // Force refresh of Scene Views
            EditorApplication.delayCall += () => SceneView.RepaintAll();
#endif
        }

        /// <summary>
        /// Sets up command buffer and resources for a specific camera.
        /// </summary>
        /// <param name="camera">Target camera to configure</param>
        void SetupCamera(Camera camera)
        {
            if (camera == null || cameraCommandBuffers.ContainsKey(camera)) return;

            // Create command buffer and RenderTexture for this camera
            var cmdBuffer = new CommandBuffer { name = $"{k_CommandBufferName} ({camera.name})" };
            cameraCommandBuffers[camera] = cmdBuffer;
            camera.AddCommandBuffer(m_CameraEvent, cmdBuffer);

            UpdateCameraResources(camera);
        }


        /// <summary>
        /// Creates or updates render texture resources for a camera.
        /// </summary>
        /// <param name="camera">Target camera to update resources for</param>
        void UpdateCameraResources(Camera camera)
        {
            if (camera.pixelWidth <= 0 || camera.pixelHeight <= 0) return;

            // Create or resize RenderTexture for this camera
            if (!cameraRenderTextures.TryGetValue(camera, out var rt) ||
                rt.width != camera.pixelWidth ||
                rt.height != camera.pixelHeight)
            {
                if (rt != null)
                {
                    rt.Release();
                    DestroyImmediate(rt);
                }

                rt = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.DefaultHDR)
                {
                    autoGenerateMips = false,
                    useMipMap = false,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
                rt.Create();
                cameraRenderTextures[camera] = rt;
            }
        }

        /// <summary>
        /// Updates the command buffer for a specific camera with current effect parameters
        /// </summary>
        /// <param name="camera">Target camera to update</param>
        void UpdateCommandBuffer(Camera camera)
        {
            if (camera == null || !cameraCommandBuffers.TryGetValue(camera, out var cmdBuffer) ||
                !cameraRenderTextures.TryGetValue(camera, out var tempRT) ||
                m_EmulationMaterial == null)
            {
                return;
            }

            // Skip unsupported cameras or disabled effects from user Settings
            if (!IsSupported(camera) || !ShouldApplyEffectToCamera(camera))
            {
                cmdBuffer.Clear();
                return;
            }

            var manager = CompositionLayerManager.Instance;
            if (manager == null) return;

            var defaultLayer = manager.DefaultSceneCompositionLayer;
            if (defaultLayer == null) return;

            var colorScaleBiasExtension = defaultLayer.GetComponent<ColorScaleBiasExtension>();
            if (colorScaleBiasExtension == null || !colorScaleBiasExtension.isActiveAndEnabled) return;

            cmdBuffer.Clear();
            UpdateCameraResources(camera);

            m_EmulationMaterial.SetVector(k_ScaleParameterName, colorScaleBiasExtension.Scale);
            m_EmulationMaterial.SetVector(k_BiasParameterName, colorScaleBiasExtension.Bias);
            m_EmulationMaterial.SetTexture(k_MainTexParameterName, tempRT);

            cmdBuffer.Blit(BuiltinRenderTextureType.CameraTarget, tempRT);
            cmdBuffer.Blit(tempRT, BuiltinRenderTextureType.CameraTarget, m_EmulationMaterial);
        }

        /// <summary>
        /// Determines if the effect should be applied to a specific camera based on
        /// Composition Layers User Settings and current execution environment.
        /// </summary>
        /// <param name="camera">Camera to check</param>
        /// <returns>True if effect should be applied, false otherwise</returns>
        bool ShouldApplyEffectToCamera(Camera camera)
        {
#if UNITY_EDITOR
            if (IsSceneViewCamera(camera))
            {
                return EmulatedCompositionLayerUtils.EmulationInScene;
            }
            else if (!Application.isPlaying)
            {
                return true;
            }
            else
            {
                return EmulatedCompositionLayerUtils.EmulationInPlayMode && !XRHelper.GetDeviceConnected();
            }
#elif UNITY_STANDALONE
            return CompositionLayersRuntimeSettings.Instance.EmulationInStandalone && !XRHelper.GetDeviceConnected();
#endif
        }

        /// <summary>
        /// Checks if a camera belongs to a Scene View.
        /// </summary>
        /// <param name="camera">Camera to check</param>
        /// <returns>True if camera is associated with a Scene View</returns>
        bool IsSceneViewCamera(Camera camera)
        {
#if UNITY_EDITOR
            foreach (var sceneView in SceneView.sceneViews)
            {
                if (sceneView is SceneView sv && sv.camera == camera)
                    return true;
            }
#endif
            return false;
        }
#if UNITY_EDITOR
        /// <summary>
        /// Callback for Scene View rendering updates.
        /// </summary>
        /// <param name="sceneView">Active Scene View instance</param>
        void OnSceneGUI(SceneView sceneView)
        {
            if (sceneView == null || sceneView.camera == null) return;
            UpdateCommandBuffer(sceneView.camera);
        }
#endif
        /// <summary>
        /// LateUpdate callback to update all tracked cameras
        /// </summary>
        void LateUpdate()
        {
            foreach (var camera in cameraCommandBuffers.Keys)
                UpdateCommandBuffer(camera);
        }

        /// <summary>
        /// Cleans up resources when component is disabled or destroyed
        /// </summary>
        void OnDisable()
        {
            foreach (var kvp in cameraCommandBuffers)
            {
                if (kvp.Key != null)
                    kvp.Key.RemoveCommandBuffer(CameraEvent.AfterImageEffects, kvp.Value);
            }
            cameraCommandBuffers.Clear();

            foreach (var rt in cameraRenderTextures.Values)
            {
                if (rt != null)
                {
                    rt.Release();
                    DestroyImmediate(rt);
                }
            }
            cameraRenderTextures.Clear();
#if UNITY_EDITOR
            SceneView.duringSceneGui -= OnSceneGUI;
#endif
            if (m_EmulationMaterial != null)
                DestroyImmediate(m_EmulationMaterial);
        }
    }
}
#endif