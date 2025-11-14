using System.Runtime.InteropServices;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    internal unsafe struct XrSpaceLocation
    {
        internal XrStructureType type;
        internal void* next;
        internal XrSpaceLocationFlags locationFlags;
        internal XrPosef pose;
    }
}
