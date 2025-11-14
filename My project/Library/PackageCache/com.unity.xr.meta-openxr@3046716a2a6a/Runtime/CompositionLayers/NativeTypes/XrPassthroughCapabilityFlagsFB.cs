using System;

namespace UnityEngine.XR.OpenXR.NativeTypes.Meta
{
    /// <summary>
    /// Specify passthrough system capabilities.
    /// </summary>
    [Flags]
    enum XrPassthroughCapabilityFlagsFB : ulong
    {
        /// <summary>
        /// The system supports passthrough.
        /// </summary>
        XR_PASSTHROUGH_CAPABILITY_BIT_FB = 0x00000001,

        /// <summary>
        /// The system can show passthrough with realistic colors. `XR_PASSTHROUGH_CAPABILITY_BIT_FB` must be set if `XR_PASSTHROUGH_CAPABILITY_COLOR_BIT_FB` is set.
        /// </summary>
        XR_PASSTHROUGH_CAPABILITY_COLOR_BIT_FB = 0x00000002,

        /// <summary>
        /// The system supports passthrough layers composited using depth testing. `XR_PASSTHROUGH_CAPABILITY_BIT_FB` must be set if `XR_PASSTHROUGH_CAPABILITY_LAYER_DEPTH_BIT_FB` is set.
        /// </summary>
        XR_PASSTHROUGH_CAPABILITY_LAYER_DEPTH_BIT_FB = 0x00000004
    }
}
