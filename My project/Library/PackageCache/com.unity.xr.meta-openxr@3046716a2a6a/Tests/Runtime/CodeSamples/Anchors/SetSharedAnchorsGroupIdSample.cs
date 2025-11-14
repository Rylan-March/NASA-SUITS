using System;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    #region SetSharedAnchorsGroupId
    using UnityEngine.XR.OpenXR.Features.Meta;

    public class SetSharedAnchorsGroupIdSample
    {
        void SetSharedAnchorsGroupId(ARAnchorManager anchorManager)
        {
            // First get a reference to the MetaOpenXRAnchorSubsystem
            var metaAnchorSubsystem =
                (MetaOpenXRAnchorSubsystem)anchorManager.subsystem;

            metaAnchorSubsystem.sharedAnchorsGroupId =
                new SerializableGuid(Guid.NewGuid());
        }
    }
    #endregion
}
