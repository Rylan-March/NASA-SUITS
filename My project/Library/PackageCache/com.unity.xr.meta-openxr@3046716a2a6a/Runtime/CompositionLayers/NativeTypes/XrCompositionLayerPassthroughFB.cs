using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR.NativeTypes.Meta
{
    /// <summary>
    /// A composition layer for passthrough.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct XrCompositionLayerPassthroughFB
    {
        /// <summary>
        /// The XrFbStructType of this structure.
        /// </summary>
        public XrFbStructType type;

        /// <summary>
        /// Pointer to the next structure in a structure chain.
        /// </summary>
        public void* next;

        /// <summary>
        /// A bitmask of XrCompositionLayerFlagBits that specify additional behavior.
        /// </summary>
        public XrCompositionLayerFlags flags;

        /// <summary>
        /// The XrSpace that specifies the layer space - must be null.
        /// </summary>
        public ulong space;

        /// <summary>
        /// The handle of the XrPassthroughLayerFB that defines this layer behavior.
        /// </summary>
        public ulong passthroughLayerHandle;
    }
}
