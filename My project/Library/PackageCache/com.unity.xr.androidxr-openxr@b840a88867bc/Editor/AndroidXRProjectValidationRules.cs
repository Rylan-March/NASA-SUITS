using Unity.XR.CoreUtils.Editor;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine.XR.Hands.OpenXR;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Android;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_RENDER_PIPELINES_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif // UNITY_RENDER_PIPELINES_UNIVERSAL

namespace UnityEditor.XR.OpenXR.Features.Android
{
    static class AndroidXRProjectValidationRules
    {
        const string k_Category = "Android XR";
        const AndroidSdkVersions k_MinSupportedSdkVersion = AndroidSdkVersions.AndroidApiLevel24;

#if UNITY_ANDROID
        [InitializeOnLoadMethod]
        static void AddAndroidXRValidationRules()
        {
            var androidXRGlobalRules = new[]
            {
                new BuildValidationRule
                {
                    Category = k_Category,
                    Message = "Most apps require the activity window to be resizable so that pop ups such as system permission requests can be rendered.",
                    IsRuleEnabled = XRManagerEditorUtility.IsAndroidOpenXRTheActiveBuildTarget,
#if UNITY_6000_0_4_OR_NEWER
                    CheckPredicate = () => PlayerSettings.Android.resizeableActivity,
                    FixItMessage = "Go to <b>Project Settings</b> > <b>Player Settings</b>, then select the Android tab and enable <b>Resizeable Activity</b>.",
                    FixIt = () => PlayerSettings.Android.resizeableActivity = true,
#else
                    CheckPredicate = () => PlayerSettings.Android.resizableWindow,
                    FixItMessage = "Go to <b>Project Settings</b> > <b>Player Settings</b>, then select the Android tab and enable <b>Resizable Window</b>.",
                    FixIt = () => PlayerSettings.Android.resizableWindow = true,
#endif // UNITY_6000_0_4_OR_NEWER
                    Error = false
                },
                new BuildValidationRule
                {
                    Category = k_Category,
                    Message = "Android XR requires Application Entry Point set to Game Activity.",
                    IsRuleEnabled = XRManagerEditorUtility.IsAndroidOpenXRTheActiveBuildTarget,
                    CheckPredicate = () => PlayerSettings.Android.applicationEntry == AndroidApplicationEntry.GameActivity,
                    FixItAutomatic = true,
                    FixItMessage = "Set your Application Entry Point to <b>Game Activity</b>.",
                    FixIt = () => PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.GameActivity,
                    Error = true,
                },
                new BuildValidationRule
                {
                    Category = k_Category,
                    Message = $"Android XR requires targeting minimum 'API Level {k_MinSupportedSdkVersion}'.",
                    IsRuleEnabled = XRManagerEditorUtility.IsAndroidOpenXRTheActiveBuildTarget,
                    CheckPredicate = () =>
                    {
                        return PlayerSettings.Android.minSdkVersion >= k_MinSupportedSdkVersion;
                    },
                    FixItAutomatic = true,
                    FixItMessage = "Open Project Settings > Player Settings > Android tab and increase the 'Minimum API Level'" +
                        $" to 'API Level {k_MinSupportedSdkVersion}'.",
                    FixIt = () =>
                    {
                        PlayerSettings.Android.minSdkVersion = k_MinSupportedSdkVersion;
                    },
                    Error = true,
                },
#if UNITY_RENDER_PIPELINES_UNIVERSAL
                new BuildValidationRule
                {
                    Category = k_Category,
                    Message = "Android XR requires HDR to be disabled on the active URP asset.",
                    IsRuleEnabled = () =>
                    {
                        if (!XRManagerEditorUtility.IsAndroidOpenXRTheActiveBuildTarget())
                            return false;

                        if (BuildProfile.GetActiveBuildProfile() != null)
                            return false;

                        if (GetActiveURPAssetForAndroid() == null)
                            return false;

                        return true;
                    },
                    CheckPredicate = () =>
                    {
                        var urpAsset = GetActiveURPAssetForAndroid();
                        if (urpAsset == null)
                            return true;

                        return !urpAsset.supportsHDR;
                    },
                    FixItAutomatic = true,
                    FixItMessage = "Disable <b>HDR</b> in your active URP asset for Android.",
                    FixIt = () =>
                    {
                        var urpAsset = GetActiveURPAssetForAndroid();
                        if (urpAsset != null)
                            urpAsset.supportsHDR = false;
                    },
                    Error = true,
                },
                new BuildValidationRule
                {
                    Category = k_Category,
                    Message = "Android XR requires Post Processing to be disabled on the active URP asset.",
                    IsRuleEnabled = () =>
                    {
                        if (!XRManagerEditorUtility.IsAndroidOpenXRTheActiveBuildTarget())
                            return false;

                        if (BuildProfile.GetActiveBuildProfile() != null)
                            return false;

                        if (GetActiveURPAssetForAndroid() == null)
                            return false;

                        return true;
                    },
                    CheckPredicate = () =>
                    {
                        var urpAsset = GetActiveURPAssetForAndroid();
                        if (urpAsset == null)
                            return true;

                        var rendererDataList = urpAsset.rendererDataList;
                        if (rendererDataList.Length > 0 && rendererDataList[0] is UniversalRendererData universalRendererData)
                        {
                            return universalRendererData.postProcessData == null;
                        }

                        return true;
                    },
                    FixItAutomatic = true,
                    FixItMessage = "Disable <b>Post Processing</b> in your Universal Renderer Data asset for Android.",
                    FixIt = () =>
                    {
                        var urpAsset = GetActiveURPAssetForAndroid();
                        if (urpAsset == null)
                            return;

                        var rendererDataList = urpAsset.rendererDataList;
                        if (rendererDataList.Length > 0 && rendererDataList[0] is UniversalRendererData universalRendererData)
                        {
                            universalRendererData.postProcessData = null;
                            EditorUtility.SetDirty(universalRendererData);
                        }
                    },
                    Error = true,
                },
#endif // UNITY_RENDER_PIPELINES_UNIVERSAL
            };

            BuildValidator.AddRules(BuildTargetGroup.Android, androidXRGlobalRules);
        }
#if UNITY_RENDER_PIPELINES_UNIVERSAL
        static UniversalRenderPipelineAsset GetActiveURPAssetForAndroid()
        {
            int qualityLevel = QualitySettings.GetQualityLevel();

            var urpAsset = QualitySettings.GetRenderPipelineAssetAt(qualityLevel) as UniversalRenderPipelineAsset;
            if (urpAsset != null)
                return urpAsset;

            return GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        }
#endif // UNITY_RENDER_PIPELINES_UNIVERSAL
#endif // UNITY_ANDROID
    }
}
