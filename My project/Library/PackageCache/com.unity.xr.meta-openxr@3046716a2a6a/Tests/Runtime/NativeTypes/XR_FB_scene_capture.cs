using UnityEngine.XR.OpenXR.NativeTypes;
using XrAsyncRequestIdFB = System.UInt64;
using XrSession = System.UInt64;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    unsafe struct XrSceneCaptureRequestInfoFB
    {
        public XrStructureType type;
        public void* next;
        public uint requestByteCount;
        public byte* request;
    }

    unsafe struct XrEventDataSceneCaptureCompleteFB
    {
        public XrStructureType type;
        public void* next;
        public XrAsyncRequestIdFB requestId;
        public XrResult result;
    }

    unsafe delegate int RequestSceneCaptureFB_Delegate(
        XrSession session,
        XrSceneCaptureRequestInfoFB* info,
        XrAsyncRequestIdFB* requestId);
}
