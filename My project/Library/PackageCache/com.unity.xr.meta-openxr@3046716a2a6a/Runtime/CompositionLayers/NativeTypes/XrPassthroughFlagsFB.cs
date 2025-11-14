using System;

namespace UnityEngine.XR.OpenXR.NativeTypes.Meta
{
    /// <summary>
    /// Specifies additional creation behavior.
    /// </summary>
    [Flags]
    enum XrPassthroughFlagsFB : ulong
    {
        /// <summary>
        /// The object (passthrough, layer) is running at creation.
        /// </summary>
        XR_PASSTHROUGH_IS_RUNNING_AT_CREATION_BIT_FB = 0x00000001,

        /// <summary>
        /// The passthrough system sends depth information to the compositor. Only applicable to layer objects.
        /// </summary>
        XR_PASSTHROUGH_LAYER_DEPTH_BIT_FB = 0x00000002
    }
}
