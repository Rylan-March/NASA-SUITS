using System.Runtime.InteropServices;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    internal unsafe struct XrRect2Df
    {
        internal XrOffset2Df offset;
        internal XrExtent2Df extent;
    }
}
