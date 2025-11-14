using UnityEngine;
using UnityEngine.XR.OpenXR.Features;

namespace UnityEditor.XR.OpenXR.Features.Android
{
    [FilePath("Assets/XR/AndroidXR/AndroidXRSettingsInitializer", FilePathAttribute.Location.ProjectFolder)]
    class AndroidXRSettingsInitializer : ScriptableSingleton<AndroidXRSettingsInitializer>
    {
        [SerializeField, HideInInspector]
        bool isInitialized;

        [InitializeOnLoadMethod]
        static void OnLoad()
        {
#if UNITY_ANDROID_XR
            instance.InitializeXRSettings();
#else
            instance.isInitialized = false;
#endif
            instance.Save(true);
        }

        void InitializeXRSettings()
        {
            if (!isInitialized)
            {
                var androidXrFeatureSet = OpenXRFeatureSetManager.GetFeatureSetWithId(
                    BuildTargetGroup.Android,
                    AndroidFeatureSet.featureSetId
                );

                if (androidXrFeatureSet != null)
                    androidXrFeatureSet.isEnabled = true;
                else
                    Debug.LogWarning("Android XR feature set could not be enabled in OpenXR settings.");

                var foveatedRenderingFeature = FeatureHelpers.GetFeatureWithIdForActiveBuildTarget(FoveatedRenderingFeature.featureId);

                if (foveatedRenderingFeature != null)
                    foveatedRenderingFeature.enabled = true;
                else
                    Debug.LogWarning("Foveated Rendering feature could not be enabled in OpenXR settings.");

                OpenXRFeatureSetManager.SetFeaturesFromEnabledFeatureSets(BuildTargetGroup.Android);
                isInitialized = true;
            }
        }
    }
}
