namespace UnityEngine.XR.OpenXR.NativeTypes.Meta
{
    /// <summary>
    /// Meta specific value for type members of structs.
    /// </summary>
    enum XrFbStructType : uint
    {
        /// <summary>
        /// Struct type for the creation info of the passthrough context.
        /// </summary>
        XR_TYPE_PASSTHROUGH_CREATE_INFO_FB = 1000118001,

        /// <summary>
        /// Struct type for the creation info of the passthrough layer.
        /// </summary>
        XR_TYPE_PASSTHROUGH_LAYER_CREATE_INFO_FB = 1000118002,

        /// <summary>
        /// Struct type for the passthrough layer.
        /// </summary>
        XR_TYPE_COMPOSITION_LAYER_PASSTHROUGH_FB = 1000118003,
    }
}
