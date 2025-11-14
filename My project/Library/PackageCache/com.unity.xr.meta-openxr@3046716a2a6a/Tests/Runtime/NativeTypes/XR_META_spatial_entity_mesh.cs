using UnityEngine.XR.OpenXR.NativeTypes;
using XrSpace = System.UInt64;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    unsafe struct XrSpaceTriangleMeshGetInfoMETA
    {
        internal XrStructureType type;
        internal void* next;
    }

    unsafe struct XrSpaceTriangleMeshMETA
    {
        internal XrStructureType type;
        internal void* next;
        internal uint vertexCapacityInput;
        internal uint vertexCountOutput;
        internal XrVector3f* vertices;
        internal uint indexCapacityInput;
        internal uint indexCountOutput;
        internal uint* indices;
    }

    unsafe delegate XrResult XrGetSpaceTriangleMeshMETA_Delegate(
        XrSpace space,
        XrSpaceTriangleMeshGetInfoMETA* getInfo,
        XrSpaceTriangleMeshMETA* triangleMeshOutput);
}
