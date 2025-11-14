#if UNITY_EDITOR
namespace UnityEditor.XR.CompositionLayers.Editor.ProjectValidation
{
    /// <summary>
    /// Validation rule generator for shared validation rules between multiple features.
    /// </summary>
    static class CompositionLayerBuildValidationRuleFactory
    {
#if UNITY_RENDER_PIPELINES_UNIVERSAL
        internal static CompositionLayerBuildValidationRule CreateHDRRuleForURP(BuildTargetGroup buildTargetGroup)
        {
            return new CompositionLayerBuildValidationRule
            {
                Message = "Disable HDR on Universal Render Pipeline Asset to enable composition layer support.",
                CheckPredicate = () =>
                {
                    foreach (var urpAsset in CompositionLayerProjectValidation.GetURPAssetsForBuildTarget(buildTargetGroup))
                        if (urpAsset != null && urpAsset.supportsHDR)
                            return false;

                    return true;
                },
                FixIt = () => CompositionLayerProjectValidation.DisableHDROnURPAssets(buildTargetGroup),
                FixItAutomatic = true,
                FixItMessage = "Open Graphics Settings to disable HDR on Universal Render Pipeline Asset.",
                Error = false,
                buildTargetGroup = buildTargetGroup
            };
        }

        internal static CompositionLayerBuildValidationRule CreateAlphaProcessingRuleForURP(BuildTargetGroup buildTargetGroup)
        {
            return new CompositionLayerBuildValidationRule
            {
                Message = "Enable Alpha Processing on Universal Render Pipeline Asset under Post Processing to enable composition layer support.",
                CheckPredicate = () =>
                {
                    foreach (var urpAsset in CompositionLayerProjectValidation.GetURPAssetsForBuildTarget(buildTargetGroup))
                        if (urpAsset != null && !urpAsset.allowPostProcessAlphaOutput)
                            return false;

                    return true;
                },
                FixIt = () => CompositionLayerProjectValidation.OpenFirstURPAssetWithAlphaOutputOff(buildTargetGroup),
                FixItAutomatic = false,
                FixItMessage = "Open Universal Render Pipeline Asset to enable Alpha Processing under Post Processing.",
                Error = false,
                buildTargetGroup = buildTargetGroup
            };
        }
#endif
    }
}
#endif
