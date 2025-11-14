using System.Runtime.InteropServices;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    internal unsafe struct XrViewState
    {
        public UnityEngine.XR.OpenXR.NativeTypes.XrStructureType type;
        public void* next;
        public XrViewStateFlags viewStateFlags;
    }
}
