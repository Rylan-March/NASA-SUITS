using UnityEngine.XR.OpenXR.NativeTypes;
using XrTime = System.Int64;
using XrSpace = System.UInt64;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    internal unsafe struct XrViewLocateInfo
    {
        public UnityEngine.XR.OpenXR.NativeTypes.XrStructureType type;
        public void* next;
        public XrViewConfigurationType viewConfigurationType;
        public XrTime displayTime;
        public XrSpace space;
    }
}
