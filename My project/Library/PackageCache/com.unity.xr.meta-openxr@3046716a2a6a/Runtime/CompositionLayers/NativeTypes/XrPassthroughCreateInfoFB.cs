using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR.NativeTypes.Meta
{
    /// <summary>
    /// A create structure for the passthrough context.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct XrPassthroughCreateInfoFB
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
        /// A bitmask of `XrPassthroughFlagBitsFB` that specify additional behavior.
        /// </summary>
        public ulong flags;
    }
}
