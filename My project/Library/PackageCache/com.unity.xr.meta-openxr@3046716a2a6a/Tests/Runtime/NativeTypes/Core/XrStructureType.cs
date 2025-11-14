namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    /// <summary>
    /// These are XrStructureType(s) which have values not found within the OpenXR
    /// package XrStructureType and should ultimately be promoted.
    /// </summary>
    internal enum XrStructureType : uint
    {
        /// <summary>
        /// Corresponds with XrSpaceComponentStatusFB
        /// </summary>
        XR_TYPE_SPACE_COMPONENT_STATUS_FB = 1000113001,

        /// <summary>
        /// Corresponds with XrSpaceComponentStatusSetInfoFB
        /// </summary>
        XR_TYPE_SPACE_COMPONENT_STATUS_SET_INFO_FB = 1000113007,

        /// <summary>
        /// Corresponds with XrSpaceDiscoveryInfoMETA
        /// </summary>
        XR_TYPE_SPACE_DISCOVERY_INFO_META = 1000247001,

        /// <summary>
        /// Corresponds with XrSpaceFilterComponentMETA
        /// </summary>
        XR_TYPE_SPACE_FILTER_COMPONENT_META = 1000247004,

        /// <summary>
        /// Corresponds with XrSpaceDiscoveryResultMETA
        /// </summary>
        XR_TYPE_SPACE_DISCOVERY_RESULT_META = 1000247005,

        /// <summary>
        /// Corresponds with XrSpaceDiscoveryResultsMETA
        /// </summary>
        XR_TYPE_SPACE_DISCOVERY_RESULTS_META = 1000247006,

        /// <summary>
        /// Corresponds with XrEventDataSpaceSetStatusCompleteFB
        /// </summary>
        XR_TYPE_EVENT_DATA_SPACE_SET_STATUS_COMPLETE_FB = 1000113006,

        /// <summary>
        /// Corresponds with XrEventDataSpaceDiscoveryResultsAvailableMETA
        /// </summary>
        XR_TYPE_EVENT_DATA_SPACE_DISCOVERY_RESULTS_AVAILABLE_META = 1000247007,

        /// <summary>
        /// Corresponds with XrEventDataSpaceDiscoveryCompleteMETA
        /// </summary>
        XR_TYPE_EVENT_DATA_SPACE_DISCOVERY_COMPLETE_META = 1000247008,

        /// <summary>
        /// Corresponds with XrSpaceTriangleMeshGetInfoMETA
        /// </summary>
        XR_TYPE_SPACE_TRIANGLE_MESH_GET_INFO_META = 1000269001,

        /// <summary>
        /// Corresponds with XrSpaceTriangleMeshMETA
        /// </summary>
        XR_TYPE_SPACE_TRIANGLE_MESH_META = 1000269002,

        /// <summary>
        /// Corresponds with XrSystemSpatialEntityPropertiesFB
        /// </summary>
        XR_TYPE_SYSTEM_SPATIAL_ENTITY_PROPERTIES_FB = 1000113004,

        /// <summary>
        /// Corresponds with XrSceneCaptureRequestInfoFB
        /// </summary>
        XR_TYPE_SCENE_CAPTURE_REQUEST_INFO_FB = 1000198050,

        /// <summary>
        /// Corresponds with XrSceneCaptureRequestCompleteFB
        /// </summary>
        XR_TYPE_SCENE_CAPTURE_REQUEST_COMPLETE_FB = 1000198001,
    }
}
