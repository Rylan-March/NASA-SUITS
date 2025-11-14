using UnityEngine.XR.OpenXR.NativeTypes;
using XrAsyncRequestIdFB = System.UInt64;
using XrSession = System.UInt64;
using XrSpace = System.UInt64;
using XrUuidEXT = UnityEngine.XR.ARSubsystems.SerializableGuid;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    unsafe delegate XrResult XrDiscoverSpacesMETA_Delegate(
        XrSession session,
        XrSpaceDiscoveryInfoMETA* info,
        XrAsyncRequestIdFB* requestId);

    unsafe struct XrSpaceDiscoveryInfoMETA
    {
        internal XrStructureType type;
        internal void* next;
        internal uint filterCount;
        internal XrSpaceFilterBaseHeaderMETA** filters;
    }

    unsafe struct XrSpaceFilterBaseHeaderMETA
    {
        internal XrStructureType type;
        internal void* next;
    }

    unsafe struct XrSpaceFilterComponentMETA
    {
        internal XrStructureType type;
        internal void* next;
        internal XrSpaceComponentTypeFB componentType;
    }

    unsafe struct XrEventDataSpaceDiscoveryResultsAvailableMETA
    {
        internal XrStructureType type;
        internal void* next;
        internal XrAsyncRequestIdFB requestId;
    }

    unsafe struct XrEventDataSpaceDiscoveryCompleteMETA
    {
        internal XrStructureType type;
        internal void* next;
        internal XrAsyncRequestIdFB requestId;
        internal XrResult result;
    }

    unsafe delegate XrResult XrRetrieveSpaceDiscoveryResultsMETA_Delegate(
        XrSession session,
        XrAsyncRequestIdFB requestId,
        XrSpaceDiscoveryResultsMETA* results);

    unsafe struct XrSpaceDiscoveryResultsMETA
    {
        internal XrStructureType type;
        internal void* next;
        internal uint resultCapacityInput;
        internal uint resultCountOutput;
        internal XrSpaceDiscoveryResultMETA* results;
    }

    unsafe struct XrSpaceDiscoveryResultMETA
    {
        internal XrSpace space;
        internal XrUuidEXT uuid;
    }
}
