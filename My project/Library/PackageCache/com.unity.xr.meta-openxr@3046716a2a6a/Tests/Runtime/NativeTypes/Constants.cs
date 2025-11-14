namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    internal class Constants
    {
        /// <summary>
        /// The size of the native XrEventDataBuffer struct that holds all event types.
        /// Note: this relies on the underlying c++ sizeof(XrEventDataBuffer) not changing
        /// size.  If it does, then a safer alternative would be to get the size from a
        /// function call to C++.  Making a C# XrEventDataStructure that possesses 4000
        /// byte fields is unwieldy for our purposes here.
        /// </summary>
        internal const int EventDataBufferSize = 4012;

        /// <summary>
        /// Corresponds with 'XR_FB_spatial_entity'
        /// </summary>
        internal const uint XR_FB_spatial_entity_SPEC_VERSION = 3;

        /// <summary>
        /// Corresponds with 'XR_FB_scene'
        /// </summary>
        internal const uint XR_FB_scene_SPEC_VERSION = 4;

        /// <summary>
        /// Corresponds with 'XR_META_spatial_entity_discovery'
        /// </summary>
        internal const uint XR_META_spatial_entity_discovery_SPEC_VERSION = 1;

        /// <summary>
        /// Corresponds with 'XR_META_spatial_entity_mesh'
        /// </summary>
        internal const uint XR_META_spatial_entity_mesh_SPEC_VERSION = 1;

        /// <summary>
        /// Corresponds with 'XR_META_spatial_entity_persistence'
        /// </summary>
        internal const int XR_META_spatial_entity_persistence_SPEC_VERSION = 1;

        /// <summary>
        /// Corresponds with 'XR_META_spatial_entity_sharing'
        /// </summary>
        internal const uint XR_META_spatial_entity_sharing_SPEC_VERSION = 1;

        /// <summary>
        /// Corresponds with 'XR_META_spatial_entity_group_sharing'
        /// </summary>
        internal const int XR_META_spatial_entity_group_sharing_SPEC_VERSION = 1;
    }
}
