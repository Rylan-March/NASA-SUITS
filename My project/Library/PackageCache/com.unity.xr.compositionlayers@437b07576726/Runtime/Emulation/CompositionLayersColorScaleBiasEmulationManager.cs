#if !UNITY_RENDER_PIPELINES_UNIVERSAL && !UNITY_RENDER_PIPELINES_HDRENDER  && (UNITY_EDITOR || UNITY_STANDALONE)
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.CompositionLayers;
using Unity.XR.CompositionLayers.Services;
using Unity.XR.CompositionLayers.Emulation;
using Unity.XR.CompositionLayers.Extensions;
using UnityEngine.XR.CompositionLayers.Rendering.Internals;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.XR.CompositionLayers.Emulation
{
    static class CompositionLayersColorScaleBiasEmulationManager
    {
        static Camera s_MainCamera;
        static bool s_EmulationEnabled; // Tracks whether emulation is enabled.

#if UNITY_EDITOR
        /// <summary>
        /// Manages the Color Scale Bias Emulation in the Built-in Render Pipeline.
        /// </summary>
        [InitializeOnLoadMethod]
        static void InitializeInEditor()
        {
            EditorApplication.update += UpdateEmulation;
        }
#endif
#if UNITY_STANDALONE
        /// <summary>
        /// Run once before runtime in standalone builds.
        /// </summary>
        [RuntimeInitializeOnLoadMethod()]
        static void InitializeBeforeRuntime()
        {
            UpdateEmulation();
        }
#endif
        /// <summary>
        /// Updates the emulation state based on the current execution environment.
        /// Ensures the correct color scale bias settings are applied.
        /// </summary>
        static void UpdateEmulation()
        {
            AssignMainCamera();
            s_EmulationEnabled = IsColorScaleBiasInScene();
            ManageBIRPEmulation();
        }

        /// <summary>
        /// Checks if the Color Scale Bias effect is currently active in the scene.
        /// </summary>
        /// <returns>True if Color Scale Bias Component is attached to the Default layer and enabled, otherwise false.</returns>
        static bool IsColorScaleBiasInScene()
        {

            CompositionLayerManager compositionLayerManager = CompositionLayerManager.Instance;
            if (compositionLayerManager == null)
            {
                return false;
            }

            var defaultLayer = compositionLayerManager.DefaultSceneCompositionLayer;
            if (defaultLayer == null)
            {
                return false;
            }

            var colorScaleBiasExtension = defaultLayer.GetComponent<ColorScaleBiasExtension>();
            if (colorScaleBiasExtension == null || colorScaleBiasExtension.isActiveAndEnabled == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Assigns the main camera to the cached reference.
        /// </summary>
        static void AssignMainCamera()
        {
            if (s_MainCamera == null)
            {
                s_MainCamera = CompositionLayerManager.mainCameraCache;
            }
        }

        /// <summary>
        /// Manages the Color Scale Bias emulation in the Built-in Render Pipeline.
        /// Adds or removes the `EmulationColorScaleBias` component on the main camera based on the emulation state.
        /// </summary>
        static void ManageBIRPEmulation()
        {
            if (s_MainCamera == null) return;

            var component = s_MainCamera.GetComponent<EmulatedColorScaleBias>();

            if (s_EmulationEnabled)
            {
                if (component == null)
                {
                    s_MainCamera.gameObject.AddComponent<EmulatedColorScaleBias>();
                }
            }
            else
            {
                if (component != null)
                {
                    UnityEngine.Object.DestroyImmediate(component);
                }
            }
        }
    }
}
#endif