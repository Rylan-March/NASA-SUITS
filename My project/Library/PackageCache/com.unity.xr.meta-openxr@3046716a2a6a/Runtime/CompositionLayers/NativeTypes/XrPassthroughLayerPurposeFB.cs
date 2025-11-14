namespace UnityEngine.XR.OpenXR.NativeTypes.Meta
{
    /// <summary>
    /// Specifies the kind of passthrough behavior the layer provides.
    /// </summary>
    enum XrPassthroughLayerPurposeFB : uint
    {
        /// <summary>
        /// Reconstruction passthrough (full screen environment)
        /// </summary>
        XR_PASSTHROUGH_LAYER_PURPOSE_RECONSTRUCTION_FB = 0,

        /// <summary>
        /// Projected passthrough (using a custom surface)
        /// </summary>
        XR_PASSTHROUGH_LAYER_PURPOSE_PROJECTED_FB = 1,

        /// <summary>
        /// Passthrough layer purpose for keyboard hands presence.
        /// </summary>
        XR_PASSTHROUGH_LAYER_PURPOSE_TRACKED_KEYBOARD_HANDS_FB = 1000203001,

        /// <summary>
        /// Passthrough layer purpose for keyboard hands presence with keyboard masked hand transitions (i.e passthrough hands rendered only when they are over the keyboard).
        /// </summary>
        XR_PASSTHROUGH_LAYER_PURPOSE_TRACKED_KEYBOARD_MASKED_HANDS_FB = 1000203002,

        /// <summary>
        /// Max value this enum can be.
        /// </summary>
        XR_PASSTHROUGH_LAYER_PURPOSE_MAX_ENUM_FB = 0x7FFFFFFF
    }
}
