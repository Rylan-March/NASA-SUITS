using UnityEngine.XR.OpenXR.Features.CompositionLayers;
using UnityEngine.XR.OpenXR.Features.Meta;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

namespace UnityEditor.XR.OpenXR.Features.Meta
{
    [OpenXRFeatureSet(
        FeatureIds = new[]
        {
            ARSessionFeature.featureId,
            ARAnchorFeature.featureId,
            ARCameraFeature.featureId,
            ARPlaneFeature.featureId,
            ARBoundingBoxFeature.featureId,
            ARRaycastFeature.featureId,
            MetaQuestFeature.featureId,
            DisplayUtilitiesFeature.featureId,
            ARMeshFeature.featureId,
            BoundaryVisibilityFeature.featureId,
            AROcclusionFeature.featureId,
            ColocationDiscoveryFeature.featureId,
        },
        DefaultFeatureIds = new []
        {
            ARSessionFeature.featureId,
            ARAnchorFeature.featureId,
            ARCameraFeature.featureId,
            ARPlaneFeature.featureId,
            ARBoundingBoxFeature.featureId,
            ARRaycastFeature.featureId,
            MetaQuestFeature.featureId,
            DisplayUtilitiesFeature.featureId,
            ARMeshFeature.featureId,
            BoundaryVisibilityFeature.featureId,
            AROcclusionFeature.featureId,
            OpenXRCompositionLayersFeature.FeatureId,
            ColocationDiscoveryFeature.featureId,
        },
        RequiredFeatureIds = new[]
        {
            MetaQuestFeature.featureId,
            OpenXRCompositionLayersFeature.FeatureId
        },
        UiName = "Meta Quest",
        FeatureSetId = featureSetId,
        SupportedBuildTargets = new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone }
    )]
    class MetaFeatureSet {
        const string featureSetId = "com.unity.openxr.featureset.meta";
    }
}
