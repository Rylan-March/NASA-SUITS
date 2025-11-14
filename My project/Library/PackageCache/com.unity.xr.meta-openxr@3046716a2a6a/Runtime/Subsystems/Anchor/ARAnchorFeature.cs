using System.Collections.Generic;
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
    /// Enables AR Foundation anchor support via OpenXR for Meta Quest devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = k_DisplayName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone },
        Company = Constants.k_CompanyName,
        Desc = "AR Foundation anchor support on Meta Quest devices",
        DocumentationLink = Constants.DocsUrls.k_AnchorsUrl,
        OpenxrExtensionStrings = k_OpenXRRequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class ARAnchorFeature : MetaOpenXRFeature
    {
        const string k_DisplayName = "Meta Quest: Anchors";

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.arfoundation-meta-anchor";

        /// <summary>
        /// The set of OpenXR spec extension strings to enable, separated by spaces.
        /// For more information, refer to
        /// <see href="https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.6/manual/features.html#enabling-openxr-spec-extension-strings"/>.
        /// </summary>
        /// <seeaslo href="https://developer.oculus.com/documentation/native/android/mobile-scene-api-ref/"/>
        const string k_OpenXRRequestedExtensions =
            k_XR_FB_spatial_entity + " " +
            k_XR_META_spatial_entity_discovery + " " +
            k_XR_META_spatial_entity_persistence + " " +
            k_XR_META_spatial_entity_sharing + " " +
            k_XR_META_spatial_entity_group_sharing + " " +
            k_XR_FB_spatial_entity_query;

        static List<XRAnchorSubsystemDescriptor> s_AnchorDescriptors = new();

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
            InitializeCapabilityInfo(SystemCapability.SpaceDiscovery, xrInstance);
            InitializeCapabilityInfo(SystemCapability.SpacePersistence, xrInstance);
            InitializeCapabilityInfo(SystemCapability.SharedAnchors, xrInstance);

            return
                OpenXRUtils.IsExtensionEnabled(k_XR_FB_spatial_entity) &&
                IsCapabilitySupported(SystemCapability.SpatialEntity, xrInstance, k_DisplayName, typeof(XRAnchorSubsystem));
        }

        /// <summary>
        /// Creates the <see cref="MetaOpenXRAnchorSubsystem"/>, but does not start it.
        /// (Start/Stop is typically handled by AR Foundation managers.)
        /// </summary>
        /// <remarks>
        /// This method is not invoked if <see cref="OnInstanceCreate"/> returned <see langword="false"/>.
        /// </remarks>
        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRAnchorSubsystemDescriptor, XRAnchorSubsystem>(
                s_AnchorDescriptors,
                MetaOpenXRAnchorSubsystem.k_SubsystemId);
        }

        /// <summary>
        /// Destroys the anchor subsystem.
        /// </summary>
        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRAnchorSubsystem>();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validation Rules for ARAnchorFeature.
        /// </summary>
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            rules.Add(ValidationRuleFactory.CreateARSessionValidationRule(this, targetGroup));
        }
#endif
    }
}
