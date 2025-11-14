using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR.NativeTypes.Meta
{
    /// <summary>
    /// A create structure for passthrough layers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct XrPassthroughLayerCreateInfoFB
    {
        /// <summary>
        /// The `XrFbStructType` of this structure.
        /// </summary>
        public XrFbStructType type;

        /// <summary>
        /// Pointer to the next structure in a structure chain.
        /// </summary>
        public void* next;

        /// <summary>
        /// Handle to a passthrough context.
        /// </summary>
        public ulong passthroughHandle;

        /// <summary>
        /// A bitmask of `XrPassthroughFlagBitsFB` that specify additional behavior.
        /// </summary>
        public XrPassthroughFlagsFB flags;

        /// <summary>
        /// Specifies the layer purpose.
        /// </summary>
        public XrPassthroughLayerPurposeFB purpose;
    }
}
