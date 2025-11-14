using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.XR.ARSubsystems;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif
using static UnityEngine.XR.OpenXR.Features.Meta.Constants.OpenXRExtensions;
using static UnityEngine.XR.OpenXR.Features.Meta.SystemCapabilityUtils;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Enables AR Foundation occlusion support via OpenXR for Meta Quest devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = displayName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone },
        Company = Constants.k_CompanyName,
        Desc = "AR Foundation occlusion support on Meta Quest devices",
        DocumentationLink = Constants.DocsUrls.k_OcclusionUrl,
        OpenxrExtensionStrings = k_OpenXRRequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class AROcclusionFeature : OpenXRFeature
    {
        internal const string displayName = "Meta Quest: Occlusion";

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.arfoundation-meta-occlusion";

        /// <summary>
        /// The set of OpenXR spec extension strings to enable, separated by spaces.
        /// </summary>
        const string k_OpenXRRequestedExtensions = k_XR_META_environment_depth;

        static List<XROcclusionSubsystemDescriptor> s_OcclusionDescriptors = new();
        List<XROcclusionSubsystem> m_OcclusionSubsystems = new(1);

        [SerializeField]
        [Tooltip("Enable hand removal if your project uses a separate occlusion method for hands. This property is set" +
            " to false at runtime if the device does not support hand removal.")]
        bool m_EnableHandRemoval;

        /// <summary>
        /// Called after `xrCreateInstance`. Override this method to validate that any necessary OpenXR extensions were
        /// successfully enabled
        /// (<a href="xref:UnityEngine.XR.OpenXR.OpenXRRuntime.IsExtensionEnabled(System.String)">OpenXRRuntime.IsExtensionEnabled</a>)
        /// and that any required system properties are supported. If this method returns <see langword="false"/>,
        /// the feature's [enabled](xref:UnityEngine.XR.OpenXR.Features.OpenXRFeature.enabled) property is set to <see langword="false"/>.
        /// </summary>
        /// <param name="xrInstance">Handle of the native `xrInstance`.</param>
        /// <returns><see langword="true"/> if this feature successfully initialized. Otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// If this feature is a required feature of an enabled feature set, returning <see langword="false"/> here
        /// causes the `OpenXRLoader` to fail, and XR Plug-in Management will fall back to another loader if enabled.
        /// </remarks>
        /// <seealso href="xref:openxr-features#enabling-openxr-spec-extension-strings">Enabling OpenXR spec extension strings</seealso>
        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            var handRemovalSupported = IsCapabilitySupported(SystemCapability.HandRemoval, xrInstance);
            if (m_EnableHandRemoval && !handRemovalSupported)
            {
                Debug.Log($"This OpenXR runtime does not support removing hands from occlusion textures via {k_XR_META_environment_depth}. <b>{displayName}</b> hand removal is disabled.");
                m_EnableHandRemoval = false;
            }

            return
                OpenXRUtils.IsExtensionEnabled(k_XR_META_environment_depth, displayName, typeof(XROcclusionSubsystem)) &&
                IsCapabilitySupported(SystemCapability.EnvironmentDepth, xrInstance, displayName, typeof(XROcclusionSubsystem));
        }

        /// <summary>
        /// Creates the <see cref="MetaOpenXROcclusionSubsystem"/>, but does not start it.
        /// (Start/Stop is typically handled by AR Foundation managers.)
        /// </summary>
        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XROcclusionSubsystemDescriptor, XROcclusionSubsystem>(
                s_OcclusionDescriptors,
                MetaOpenXROcclusionSubsystem.k_SubsystemId);

            // We must use SubsystemManager to get the occlusion subsystem because the OpenXRLoader is not yet
            // considered the active loader until all subsystems are successfully created.
            SubsystemManager.GetSubsystems(m_OcclusionSubsystems);
            foreach (var s in m_OcclusionSubsystems)
            {
                if (s is MetaOpenXROcclusionSubsystem metaOcclusionSubsystem)
                {
                    // always succeeds if the subsystem isn't yet started
                    metaOcclusionSubsystem.TrySetHandRemovalEnabled(m_EnableHandRemoval);
                    break;
                }
            }

            // This list is only needed for the purposes of this API call to subsystem manager
            m_OcclusionSubsystems.Clear();
        }

        /// <summary>
        /// Destroys the occlusion subsystem.
        /// </summary>
        protected override void OnSubsystemDestroy() => DestroySubsystem<XROcclusionSubsystem>();

#if UNITY_EDITOR
        /// <summary>
        /// Validation Rules for AROcclusionFeature.
        /// </summary>
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            rules.Add(ValidationRuleFactory.CreateARSessionValidationRule(this, targetGroup));

            if (targetGroup != BuildTargetGroup.Android)
                return;

            rules.Add(new ValidationRule(this)
            {
                message = $"<b>{displayName}</b> requires that your graphics API is set to Vulkan.",
                checkPredicate = () =>
                {
                    var graphicsApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                    return graphicsApis.Length > 0 && graphicsApis[0] == GraphicsDeviceType.Vulkan;
                },
                fixItAutomatic = true,
                fixItMessage = "Go to <b>Project Settings</b> > <b>Player</b> > Android tab." +
                    " Under <b>Graphics APIs</b>, add <b>Vulkan</b> as the topmost API in the list.",
                fixIt = () =>
                {
                    var currentGraphicsApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                    int apiLength = currentGraphicsApis.Length;
                    apiLength += Array.Exists(currentGraphicsApis, element => element == GraphicsDeviceType.Vulkan) ? 0 : 1;

                    // Copy the user's graphics APIs into a new array where Vulkan is the first element
                    var correctGraphicsApis = new GraphicsDeviceType[apiLength];
                    correctGraphicsApis[0] = GraphicsDeviceType.Vulkan;
                    var dstIndex = 1;
                    for (var srcIndex = 0; srcIndex < currentGraphicsApis.Length; ++srcIndex)
                    {
                        if (currentGraphicsApis[srcIndex] != GraphicsDeviceType.Vulkan)
                        {
                            correctGraphicsApis[dstIndex] = currentGraphicsApis[srcIndex];
                            ++dstIndex;
                        }
                    }
                    PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, correctGraphicsApis);
                },
                error = true,
            });
        }
#endif
    }
}
