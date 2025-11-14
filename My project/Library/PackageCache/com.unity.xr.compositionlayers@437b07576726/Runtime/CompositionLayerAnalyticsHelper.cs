using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CompositionLayers.Services;
using Unity.XR.CompositionLayers.Extensions;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Unity.XR.CompositionLayers
{
    internal partial class CompositionLayerAnalyticsHelper
    {
        internal static readonly Type k_UIMirrorComponentType = System.Reflection.Assembly.Load("Unity.XR.CompositionLayers.UIInteraction").GetType("Unity.XR.CompositionLayers.UIInteraction.InteractableUIMirror");
        /// <summary>
        /// Reference to all the Layer Types and their usage in all scenes created in the project.
        /// </summary>
        internal static Dictionary<string, int> LayerTypesUsage
        {
            get => m_layerTypesUsage;
        }
        private static Dictionary<string, int> m_layerTypesUsage = new Dictionary<string, int>();
        /// <summary>
        /// Tracks the number of ColorScaleAndBias components in the active scene
        /// </summary>
        internal static int TotalLayerTypesUsage
        {
            get => m_totalLayerTypesUsage;
        }
        private static int m_totalLayerTypesUsage = 0;
        /// <summary>
        /// Tracks the number of ColorScaleAndBias components in the active scene
        /// </summary>
        internal static int ColorScaleAndBiasExtensionsUsage
        {
            get => m_colorScaleAndBiasExtensionsUsage;
        }
        private static int m_colorScaleAndBiasExtensionsUsage = 0;

        /// <summary>
        /// Checks if the Splash Screen is enabled in the project
        /// </summary>
        internal static bool IsEnabledSplashScreen
        {
            get => m_isEnabledSplashScreen;
        }
        private static bool m_isEnabledSplashScreen = false;

        /// <summary>
        /// Loops through all scenes in the Project Assets folder
        /// Checking each Scene for CompositionLayer components
        /// </summary>
        internal static void ProcessCompositionLayersAnalyticsData()
        {
            m_layerTypesUsage = new Dictionary<string, int>();
            m_colorScaleAndBiasExtensionsUsage = 0;
            m_totalLayerTypesUsage = 0;

#if UNITY_EDITOR
            // Retrieve all scenes included in the build
            var scenePaths = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            foreach (var scenePath in scenePaths)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                ProcessSceneLayerTypesUsage();
            }
#endif
            m_totalLayerTypesUsage = m_layerTypesUsage.Values.Sum();
            m_isEnabledSplashScreen = CompositionLayersRuntimeSettings.Instance.EnableSplashScreen;
        }

        /// <summary>
        /// Method to get the number of each type of CompositionLayer components in any given scene
        /// </summary>
        /// <param name="scene"></param>
        internal static void ProcessSceneLayerTypesUsage()
        {
            var manager = CompositionLayerManager.Instance;
            if (manager == null) return;

            foreach (CompositionLayer layer in manager.CompositionLayers)
            {
                var layerDataType = layer.LayerData?.GetType();
                var layerTypeName = layerDataType.FullName;
                var extensions = layer.Extensions;

                // To prevent countinug DefaultLayers automatically generated in each new scene.
                if ((layerDataType == typeof(DefaultLayerData)) && layer.hideFlags.HasFlag(HideFlags.HideAndDontSave))
                { continue; }

                if ((layerDataType == typeof(QuadLayerData) || layerDataType == typeof(CylinderLayerData)))
                {
                    // Check for Ineractable UI Component
                    if (layer.gameObject.GetComponent(k_UIMirrorComponentType) != null)
                    {
                        layerTypeName += ".InteractableUI";
                    }

                    // Check for AndroidSurface Texture
                    var texturesExtension =
                        extensions.FirstOrDefault(ext => ext.GetType() == typeof(TexturesExtension)) as
                            TexturesExtension;
                    if (texturesExtension != null && (texturesExtension.sourceTexture ==
                                                      TexturesExtension.SourceTextureEnum.AndroidSurface))
                    {
                        layerTypeName += ".AndroidSurface";
                    }
                }

                if (extensions.Any(ext => ext.GetType() == typeof(ColorScaleBiasExtension)))
                {
                    m_colorScaleAndBiasExtensionsUsage++;
                }

                if (m_layerTypesUsage.ContainsKey(layerTypeName))
                {
                    m_layerTypesUsage[layerTypeName]++;
                }
                else
                {
                    m_layerTypesUsage[layerTypeName] = 1;
                }
            }
        }
    }
}
