namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class Constants
    {
        /// <summary>
        /// Company name for OpenXR Feature implementations.
        /// </summary>
        internal const string k_CompanyName = "Unity Technologies";
        internal const string k_PackageName = "com.unity.xr.meta-openxr";
        internal const string k_MajorMinorVersion = "2.2";

        internal const string k_ARFoundationLibrary = "libUnityARFoundationMeta";
        internal const string k_AssemblyName = "Unity.XR.MetaOpenXR";

        /// <summary>
        /// Key used to store and retrieve custom configuration settings from EditorBuildSettings.
        /// </summary>
        internal const string k_SettingsKey = k_PackageName + ".settings";

        internal const string k_ErrorProviderUninitialized =
            "Provider is uninitialized. Your app must initialize XR before using any OpenXR Features.";

        internal static class DocsUrls
        {
            const string k_DocumentationBaseUrl = "https://docs.unity3d.com/Packages/" + k_PackageName + "@" + k_MajorMinorVersion + "/manual/";
            const string k_DocumentationFeatureBaseUrl = k_DocumentationBaseUrl + "features/";
            internal const string k_IndexUrl = k_DocumentationBaseUrl + "index.html";
            internal const string k_DisplayUtilitiesUrl = k_DocumentationFeatureBaseUrl + "display-utilities.html";
            internal const string k_BoundaryVisibilityUrl = k_DocumentationFeatureBaseUrl + "boundary-visibility.html";
            internal const string k_AnchorsUrl = k_DocumentationFeatureBaseUrl + "anchors.html";
            internal const string k_CameraUrl = k_DocumentationFeatureBaseUrl + "camera.html";
            internal const string k_PlanesUrl = k_DocumentationFeatureBaseUrl + "planes.html";
            internal const string k_BoundingBoxUrl = k_DocumentationFeatureBaseUrl + "bounding-boxes.html";
            internal const string k_RaycastsUrl = k_DocumentationFeatureBaseUrl + "raycasts.html";
            internal const string k_SessionUrl = k_DocumentationFeatureBaseUrl + "session.html";
            internal const string k_MeshUrl = k_DocumentationFeatureBaseUrl + "meshing.html";
            internal const string k_OcclusionUrl = k_DocumentationFeatureBaseUrl + "occlusion.html";
            internal const string k_ColocationDiscoveryUrl = k_DocumentationFeatureBaseUrl + "colocation-discovery.html";
        }

        internal static class OpenXRExtensions
        {
            internal const string k_XR_FB_display_refresh_rate = "XR_FB_display_refresh_rate";
            internal const string k_XR_FB_passthrough = "XR_FB_passthrough";
            internal const string k_XR_FB_spatial_entity = "XR_FB_spatial_entity";
            internal const string k_XR_FB_spatial_entity_container = "XR_FB_spatial_entity_container";
            internal const string k_XR_FB_scene = "XR_FB_scene";
            internal const string k_XR_FB_scene_capture = "XR_FB_scene_capture";
            internal const string k_XR_META_boundary_visibility = "XR_META_boundary_visibility";
            internal const string k_XR_META_environment_depth = "XR_META_environment_depth";
            internal const string k_XR_META_spatial_entity_discovery = "XR_META_spatial_entity_discovery";
            internal const string k_XR_META_spatial_entity_mesh = "XR_META_spatial_entity_mesh";
            internal const string k_XR_META_spatial_entity_persistence = "XR_META_spatial_entity_persistence";
            internal const string k_XR_META_spatial_entity_sharing = "XR_META_spatial_entity_sharing";
            internal const string k_XR_META_spatial_entity_group_sharing = "XR_META_spatial_entity_group_sharing";
            internal const string k_XR_FB_spatial_entity_query = "XR_FB_spatial_entity_query";
            internal const string k_XR_META_colocation_discovery = "XR_META_colocation_discovery";
        }
    }
}
