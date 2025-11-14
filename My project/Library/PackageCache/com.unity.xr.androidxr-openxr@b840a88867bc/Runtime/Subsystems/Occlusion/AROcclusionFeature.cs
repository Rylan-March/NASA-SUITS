using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR.NativeTypes;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Enables AR depth occlusion support via OpenXR for AndroidXR devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(
        UiName = "Android XR: AR Occlusion",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = Constants.k_CompanyName,
        Desc = "AR Foundation occlusion support on Android XR devices.",
        DocumentationLink = Constants.DocsUrls.k_OcclusionUrl,
        OpenxrExtensionStrings = k_OpenXRRequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0"
    )]
#endif
    public class AROcclusionFeature : AndroidXROpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.arfoundation-androidxr-occlusion";

        /// <summary>
        /// The set of OpenXR spec extension strings to enable, separated by spaces.
        /// </summary>
        const string k_OpenXRRequestedExtensions = Constants
            .OpenXRExtensions
            .k_XR_ANDROID_depth_texture;

        static readonly List<XROcclusionSubsystemDescriptor> s_OcclusionDescriptors = new();

        /// <summary>
        /// Instantiates Android OpenXR occlusion subsystem instance, but does not start it.
        /// (Start/Stop is typically handled by AR Foundation managers.)
        /// </summary>
        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XROcclusionSubsystemDescriptor, XROcclusionSubsystem>(
                s_OcclusionDescriptors,
                AndroidOpenXROcclusionSubsystem.k_SubsystemId
            );
        }

        /// <summary>
        /// Destroys the occlusion subsystem.
        /// </summary>
        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XROcclusionSubsystem>();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validation Rules for ARAnchorFeature.
        /// </summary>
        protected override void GetValidationChecks(
            List<ValidationRule> rules,
            BuildTargetGroup targetGroup
        )
        {
            rules.AddRange(SharedValidationRules.EnableARSessionValidationRules(this));
        }
#endif
    }
}
