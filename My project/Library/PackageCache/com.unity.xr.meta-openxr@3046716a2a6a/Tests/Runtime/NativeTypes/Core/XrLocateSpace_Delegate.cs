using UnityEngine.XR.OpenXR.NativeTypes;
using XrSpace = System.UInt64;
using XrTime = System.Int64;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    internal unsafe delegate XrResult XrLocateSpace_Delegate(
        XrSpace space,
        XrSpace baseSpace,
        XrTime time,
        XrSpaceLocation* location);
}
