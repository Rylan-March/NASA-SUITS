using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif
using static UnityEngine.XR.OpenXR.Features.Meta.Constants.OpenXRExtensions;
using static UnityEngine.XR.OpenXR.Features.Meta.SystemCapabilityUtils;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Enables AR Foundation meshing support via OpenXR for Meta Quest devices.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = k_DisplayName,
        BuildTargetGroups = new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone },
        Company = Constants.k_CompanyName,
        Desc = "AR Foundation mesh support on Meta Quest devices",
        DocumentationLink = Constants.DocsUrls.k_MeshUrl,
        OpenxrExtensionStrings = k_OpenXRRequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class ARMeshFeature : MetaOpenXRFeature
    {
        const string k_DisplayName = "Meta Quest: Meshing";
        const string k_SubsystemId = "MetaOpenXR-Meshing";

        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.arfoundation-meta-mesh";

        /// <summary>
        /// The set of OpenXR spec extension strings to enable, separated by spaces.
        /// For more information, refer to
        /// <see href="https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.6/manual/features.html#enabling-openxr-spec-extension-strings"/>.
        /// </summary>
        /// <seeaslo href="https://developer.oculus.com/documentation/native/android/mobile-scene-api-ref/"/>
        const string k_OpenXRRequestedExtensions =
            k_XR_FB_scene + " " +
            k_XR_FB_spatial_entity + " " +
            k_XR_META_spatial_entity_discovery + " " +
            k_XR_META_spatial_entity_mesh;

        static List<XRMeshSubsystemDescriptor> s_MeshDescriptors = new();

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
            return
                OpenXRRuntime.IsExtensionEnabled(k_XR_FB_scene)
                && OpenXRRuntime.IsExtensionEnabled(k_XR_FB_spatial_entity)
                && OpenXRRuntime.IsExtensionEnabled(k_XR_META_spatial_entity_discovery)
                && OpenXRRuntime.IsExtensionEnabled(k_XR_META_spatial_entity_mesh)
                && IsCapabilitySupported(SystemCapability.SpatialEntity, xrInstance, k_DisplayName, typeof(MetaOpenXRBoundingBoxSubsystem))
                && IsCapabilitySupported(SystemCapability.SpaceDiscovery, xrInstance, k_DisplayName, typeof(MetaOpenXRBoundingBoxSubsystem));
        }

        /// <summary>
        /// Instantiates the meshing subsystem instance, but does not start it.
        /// (Start/Stop is typically handled by AR Foundation managers.)
        /// </summary>
        protected override void OnSubsystemCreate()
        {
            CreateSubsystem<XRMeshSubsystemDescriptor, XRMeshSubsystem>(
                s_MeshDescriptors,
                k_SubsystemId);
        }

        /// <summary>
        /// Destroys the mesh subsystem.
        /// </summary>
        protected override void OnSubsystemDestroy()
        {
            DestroySubsystem<XRMeshSubsystem>();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validation Rules for ARMeshFeature.
        /// </summary>
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            rules.Add(ValidationRuleFactory.CreateARSessionValidationRule(this, targetGroup));
        }
#endif
    }
}
