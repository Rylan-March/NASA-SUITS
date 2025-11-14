using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;

#if UNITY_EDITOR
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.Rendering;
#endif

[assembly: InternalsVisibleTo("Unity.XR.AndroidOpenXR.Editor")]
namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Enables the OpenXR Loader for Android XR.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Android XR Support",
        Desc = "Platform settings for Android XR.",
        Company = Constants.k_CompanyName,
        DocumentationLink = "",
        OpenxrExtensionStrings = "",
        Version = "1.0.0",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        FeatureId = featureId
    )]
#endif

    public class AndroidXRSupportFeature : OpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.androidxr-support";

#if UNITY_EDITOR
        /// <summary>
        /// Symmetric projection.
        /// </summary>
        [SerializeField]
        internal bool symmetricProjection = false;

        /// <summary>
        /// Optimize buffer discards.
        /// </summary>
        [SerializeField, Tooltip("Optimization that allows 4x MSAA textures to be memoryless on Vulkan")]
        internal bool optimizeBufferDiscards = true;

        /// <summary>
        /// Caches validation rules for each build target group requested by <see cref="GetValidationChecks"/>.
        /// </summary>
        private Dictionary<BuildTargetGroup, ValidationRule[]> validationRules = new Dictionary<BuildTargetGroup, ValidationRule[]>();

        /// <summary>
        /// If enabled, the application can use Multi-View Per View Viewports functionality. This feature requires Unity 6.1 or later, and usage of the Vulkan renderer.
        /// </summary>
        [SerializeField]
        internal bool optimizeMultiviewRenderRegions = false;

        private bool SettingsUseVulkan()
        {
            if (!PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android))
            {
                GraphicsDeviceType[] apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                if (apis.Length >= 1 && apis[0] == GraphicsDeviceType.Vulkan)
                {
                    return true;
                }
                return false;
            }

            return true;
        }

        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            if (!validationRules.ContainsKey(targetGroup))
                validationRules.Add(targetGroup, CreateValidationRules(targetGroup));

            rules.AddRange(validationRules[targetGroup]);
        }

        private ValidationRule[] CreateValidationRules(BuildTargetGroup targetGroup) =>

            new ValidationRule[]
            {
                    // OptimizeMultiviewRenderRegions (aka MVPVV) only supported on Unity 6.1 onwards
#if UNITY_6000_1_OR_NEWER
                    new ValidationRule(this)
                    {
                        message = "Optimize Multiview Render Regions requires symmetric projection setting turned on.",
                        checkPredicate = () =>
                        {
                            if (optimizeMultiviewRenderRegions)
                            {
                                return symmetricProjection;
                            }
                            return true;
                        },
                        error = true,
                        fixItAutomatic = true,
                        fixIt = () =>
                        {
                            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                            var feature = settings.GetFeature<AndroidXRSupportFeature>();
                            feature.symmetricProjection = true;
                        }
                    },

                    new ValidationRule(this)
                    {
                        message = "Optimize Multiview Render Regions requires Render Mode set to \"Single Pass Instanced / Multi-view\".",
                        checkPredicate = () =>
                        {
                            if (optimizeMultiviewRenderRegions)
                            {
                                var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                                return (settings.renderMode == OpenXRSettings.RenderMode.SinglePassInstanced);
                            }
                            return true;
                        },
                        error = true,
                        fixItAutomatic = true,
                        fixIt = () =>
                        {
                            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                            settings.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
                        }
                    },

                    new ValidationRule(this)
                    {
                        message = "Optimize Multiview Render Regions needs the Vulkan Graphics API to be the default Graphics API to work at runtime.",
                        helpText = "The Optimize Multiview Render Regions feature only works with the Vulkan Graphics API, which needs to be set as the first Graphics API to be loaded at application startup. Choosing other Graphics API may require you to switch to Vulkan and restart the application.",
                        checkPredicate = () =>
                        {
                            if (optimizeMultiviewRenderRegions)
                            {
                                var graphicsApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                                return graphicsApis[0] == GraphicsDeviceType.Vulkan;
                            }
                            return true;
                        },
                        error = false
                    },
#endif

#if UNITY_ANDROID
                new ValidationRule(this)
                    {
                        message = "Symmetric Projection is only supported on Vulkan graphics API",
                        checkPredicate = () =>
                        {
                            if (symmetricProjection && !SettingsUseVulkan())
                            {
                                return false;
                            }
                            return true;
                        },
                        fixIt = () =>
                        {
                            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
                        },
                        error = true,
                        fixItAutomatic = true,
                        fixItMessage = "Set Vulkan as Graphics API"
                    },

                    new ValidationRule(this)
                    {
                        message = "Symmetric Projection is only supported when using Multi-view",
                        checkPredicate = () =>
                        {
                            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                            if (null == settings)
                                return false;

                            if (symmetricProjection && (settings.renderMode != OpenXRSettings.RenderMode.SinglePassInstanced))
                            {
                                return false;
                            }
                            return true;
                        },
                        fixIt = () =>
                        {
                            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(targetGroup);
                            if (null != settings)
                            {
                                settings.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
                            }
                        },
                        error = true,
                        fixItAutomatic = true,
                        fixItMessage = "Set Render Mode to Multi-view"
                    },

                    new ValidationRule(this)
                    {
                        message = "Optimize Buffer Discards is only supported on Vulkan graphics API",
                        checkPredicate = () =>
                        {
                            if (optimizeBufferDiscards && !SettingsUseVulkan())
                            {
                                return false;
                            }

                            return true;
                        }
                    },
#endif
            };

        internal class AndroidXRSupportFeatureEditorWindow : EditorWindow
        {
            private Object feature;
            private Editor featureEditor;

            public static EditorWindow Create(Object feature)
            {
                var window = EditorWindow.GetWindow<AndroidXRSupportFeatureEditorWindow>(true, "Android XR Support Feature Configuration", true);
                window.feature = feature;
                window.featureEditor = Editor.CreateEditor(feature);
                return window;
            }

            private void OnGUI()
            {
                featureEditor.OnInspectorGUI();
            }
        }
#endif
    }
}
