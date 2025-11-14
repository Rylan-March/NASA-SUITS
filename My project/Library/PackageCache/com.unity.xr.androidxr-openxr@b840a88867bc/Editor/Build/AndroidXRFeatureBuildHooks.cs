using System;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features.Android;

namespace UnityEditor.XR.OpenXR.Features.Android
{
    internal class AndroidXRFeatureBuildHooks : OpenXRFeatureBuildHooks
    {
        // Needed to use XR_FB_foveation, XR_FB_foveation_configuration, XR_FB_foveation_vulkan and XR_META_foveation_eye_tracked
        private const string kVulkanExtensionFragmentDensityMap = "xr-vulkan-extension-fragment-density-map-enabled";
        private const string kMvpvvEnabled = "xr-mvpvv-enabled";

        public override int callbackOrder => 2;
        public override Type featureType => typeof(AndroidXRSupportFeature);
        protected override void OnPreprocessBuildExt(BuildReport report) { }
        protected override void OnPostGenerateGradleAndroidProjectExt(string path) { }
        protected override void OnPostprocessBuildExt(BuildReport report) { }

        private AndroidXRSupportFeature GetAndroidXRSupportFeature()
        {
            var featureGuids = AssetDatabase.FindAssets("t:" + typeof(AndroidXRSupportFeature).Name);

            // we should only find one
            if (featureGuids.Length != 1)
                return null;

            string path = AssetDatabase.GUIDToAssetPath(featureGuids[0]);
            return AssetDatabase.LoadAssetAtPath<AndroidXRSupportFeature>(path);
        }

        protected override void OnProcessBootConfigExt(BuildReport report, BootConfigBuilder builder)
        {
            if (report.summary.platform != BuildTarget.Android)
                return;

            if(!XRManagerEditorUtility.IsAndroidOpenXRTheActiveBuildTarget())
                return;

            var item = GetAndroidXRSupportFeature();
            if (item == null)
            {
                Debug.Log("Unable to locate the AndroidXRSupportFeature Asset");
                return;
            }

            // Update the boot config
            builder.SetBootConfigBoolean(kVulkanExtensionFragmentDensityMap, true);
#if UNITY_6000_1_OR_NEWER
            builder.SetBootConfigBoolean(kMvpvvEnabled, item.optimizeMultiviewRenderRegions);
#endif
        }
    }
}
