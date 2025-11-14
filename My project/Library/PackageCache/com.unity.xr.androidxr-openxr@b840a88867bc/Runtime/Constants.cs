namespace UnityEngine.XR.OpenXR.Features.Android
{
    static class Constants
    {
        /// <summary>
        /// Company name for OpenXR Feature implementations.
        /// </summary>
        internal const string k_CompanyName = "Unity Technologies";
        internal const string k_PackageName = "com.unity.xr.androidxr-openxr";
        internal const string k_MajorMinorVersion = "1.0";

        internal const string k_ARFoundationLibrary = "libUnityARFoundationAndroidXR";
        internal const string k_AssemblyName = "Unity.XR.AndroidOpenXR";

        /// <summary>
        /// Key used to store and retrieve custom configuration settings from EditorBuildSettings.
        /// </summary>
        internal const string k_SettingsKey = k_PackageName + ".settings";

        internal static class DocsUrls
        {
            const string k_DocumentationBaseUrl = "https://docs.unity3d.com/Packages/" + k_PackageName + "@" + k_MajorMinorVersion + "/manual/";
            const string k_DocumentationFeatureBaseUrl = k_DocumentationBaseUrl + "features/";
            internal const string k_IndexUrl = k_DocumentationBaseUrl + "index.html";
            internal const string k_SessionUrl = k_DocumentationFeatureBaseUrl + "session.html";
            internal const string k_PlanesUrl = k_DocumentationFeatureBaseUrl + "plane-detection.html";
            internal const string k_CameraUrl = k_DocumentationFeatureBaseUrl + "camera.html";
            internal const string k_AnchorUrl = k_DocumentationFeatureBaseUrl + "anchors.html";
            internal const string k_FacesUrl = k_DocumentationFeatureBaseUrl + "faces.html";
            internal const string k_RaycastUrl = k_DocumentationFeatureBaseUrl + "raycasts.html";
            internal const string k_PerformanceMetricsUrl = k_DocumentationFeatureBaseUrl + "performance-metrics.html";
            internal const string k_OcclusionUrl = k_DocumentationFeatureBaseUrl + "occlusion.html";
            internal const string k_HandMeshDataUrl = k_DocumentationFeatureBaseUrl + "hand-mesh-data.html";
        }

        internal static class OpenXRExtensions
        {
            internal const string k_XR_ANDROID_trackables = "XR_ANDROID_trackables";
            internal const string k_XR_ANDROID_passthrough_camera_state = "XR_ANDROID_passthrough_camera_state";
            internal const string k_XR_ANDROID_proto1_controller = "XR_ANDROID_proto1_controller";
            internal const string k_XR_ANDROID_faces = "XR_ANDROID_eye_tracking";
            internal const string k_XR_ANDROID_raycast = "XR_ANDROID_raycast";
            internal const string k_XR_ANDROID_depth_texture = "XR_ANDROID_depth_texture";
            internal const string k_XR_FB_display_refresh_rate = "XR_FB_display_refresh_rate";
            internal const string k_XR_ANDROID_device_anchor_persistence = "XR_ANDROID_device_anchor_persistence";
            internal const string k_XR_ANDROID_performance_metrics = "XR_ANDROID_performance_metrics";
            internal const string k_XR_ANDROID_light_estimation = "XR_ANDROID_light_estimation";
            internal const string k_XR_ANDROID_hand_mesh = "XR_ANDROID_hand_mesh";
        }

        internal static class Permissions
        {
            internal const string k_EyeTrackingCoarsePermission = "android.permission.EYE_TRACKING_COARSE";
            internal const string k_EyeTrackingFinePermission = "android.permission.EYE_TRACKING_FINE";
            internal const string k_SceneUnderstandingCoarsePermission = "android.permission.SCENE_UNDERSTANDING_COARSE";
            internal const string k_SceneUnderstandingFinePermission = "android.permission.SCENE_UNDERSTANDING_FINE";
        }
    }
}
