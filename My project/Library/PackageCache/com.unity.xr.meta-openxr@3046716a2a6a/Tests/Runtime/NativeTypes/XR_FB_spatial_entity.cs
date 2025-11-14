using UnityEngine.XR.OpenXR.NativeTypes;
using XrAsyncRequestIdFB = System.UInt64;
using XrSpace = System.UInt64;
using XrDuration = System.Int64;
using XrUuidEXT = UnityEngine.XR.ARSubsystems.SerializableGuid;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    enum XrSpaceComponentTypeFB : uint
    {
        XR_SPACE_COMPONENT_TYPE_LOCATABLE_FB = 0,
        XR_SPACE_COMPONENT_TYPE_STORABLE_FB = 1,
        XR_SPACE_COMPONENT_TYPE_SHARABLE_FB = 2,
        XR_SPACE_COMPONENT_TYPE_BOUNDED_2D_FB = 3,
        XR_SPACE_COMPONENT_TYPE_BOUNDED_3D_FB = 4,
        XR_SPACE_COMPONENT_TYPE_SEMANTIC_LABELS_FB = 5,
        XR_SPACE_COMPONENT_TYPE_ROOM_LAYOUT_FB = 6,
        XR_SPACE_COMPONENT_TYPE_SPACE_CONTAINER_FB = 7,
        XR_SPACE_COMPONENT_TYPE_TRIANGLE_MESH_META = 1000269000,
        XR_SPACE_COMPONENT_TYPE_MAX_ENUM_FB = 0x7FFFFFFF
    }

    unsafe delegate XrResult XrSetSpaceComponentStatusFB_Delegate(
        XrSpace space,
        XrSpaceComponentStatusSetInfoFB* info,
        XrAsyncRequestIdFB* requestId);

    unsafe struct XrSpaceComponentStatusSetInfoFB
    {
        internal XrStructureType type;
        internal void* next;
        internal XrSpaceComponentTypeFB componentType;
        internal XrBool32 enabled;
        internal XrDuration timeout;
    }

    unsafe struct XrEventDataSpaceSetStatusCompleteFB
    {
        internal XrStructureType type;
        internal void* next;
        internal XrAsyncRequestIdFB requestId;
        internal XrResult result;
        internal XrSpace space;
        internal XrUuidEXT uuid;
        internal XrSpaceComponentTypeFB componentType;
        internal XrBool32 enabled;
    }

    unsafe delegate XrResult XrGetSpaceComponentStatusFB_Delegate(
        XrSpace space,
        XrSpaceComponentTypeFB componentType,
        XrSpaceComponentStatusFB* status);

    unsafe struct XrSpaceComponentStatusFB
    {
        internal XrStructureType type;
        internal void* next;
        internal XrBool32 enabled;
        internal XrBool32 changePending;
    }

    unsafe struct XrSystemSpatialEntityPropertiesFB
    {
        internal XrStructureType type;
        internal void* next;
        internal XrBool32 supportsSpatialEntity;
    };
}
