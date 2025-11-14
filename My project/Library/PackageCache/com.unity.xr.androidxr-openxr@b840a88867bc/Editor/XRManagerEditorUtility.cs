using System.Linq;
using UnityEditor.XR.Management;
using UnityEngine.XR.OpenXR;

namespace UnityEditor.XR.OpenXR.Features.Android
{
    static class XRManagerEditorUtility
    {
        internal static bool IsAndroidOpenXRTheActiveBuildTarget()
        {
            var featureIds = OpenXRFeatureSetManager.GetFeatureSetWithId(BuildTargetGroup.Android, AndroidFeatureSet.featureSetId).featureIds;
            var openXRFeatures = FeatureHelpers.GetFeaturesWithIdsForBuildTarget(BuildTargetGroup.Android, featureIds);

            var isAnyFeatureActive = openXRFeatures.Any(openXRFeature => openXRFeature.enabled);

            if (!isAnyFeatureActive)
                return false;

            return IsLoaderActiveForBuildTarget<OpenXRLoader>(BuildTargetGroup.Android) &&
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
        }

        internal static bool IsLoaderActiveForBuildTarget<TLoader>(BuildTargetGroup buildTarget)
        {
            var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(
                BuildTargetGroup.Android);
            if (generalSettings == null)
                return false;

            var managerSettings = generalSettings.AssignedSettings;

            return managerSettings != null && managerSettings.activeLoaders.OfType<TLoader>().Any();
        }
    }
}
