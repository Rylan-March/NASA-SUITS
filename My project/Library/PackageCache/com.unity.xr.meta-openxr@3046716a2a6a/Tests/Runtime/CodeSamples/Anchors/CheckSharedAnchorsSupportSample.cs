using System;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    #region CheckIsSharedAnchorsSupported
    using UnityEngine.XR.OpenXR.Features.Meta;

    public class CheckSharedAnchorsSupportSample
    {
        void CheckIsSharedAnchorsSupported(ARAnchorManager anchorManager)
        {
            // First get a reference to the MetaOpenXRAnchorSubsystem
            var metaAnchorSubsystem =
                (MetaOpenXRAnchorSubsystem)anchorManager.subsystem;

            if (metaAnchorSubsystem.isSharedAnchorsSupported == Supported.Supported)
            {
                // Shared anchors is supported
            }
        }
    }
    #endregion
}
