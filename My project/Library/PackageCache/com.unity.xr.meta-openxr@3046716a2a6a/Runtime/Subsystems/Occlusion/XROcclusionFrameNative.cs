using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    unsafe struct XROcclusionFrameNative
    {
        internal XROcclusionFrameProperties properties;
        internal long timestampNs;
        internal XRNearFarPlanes nearFarPlanes;
        internal void* poses;
        internal void* fovs;
    }
}
