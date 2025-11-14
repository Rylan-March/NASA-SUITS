using System;
using UnityEngine.XR.ARFoundation;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    #region TryShareAnchorAsync
    using UnityEngine.XR.OpenXR.Features.Meta;

    public class SingleShareAnchorSample
    {
        async void ShareAnchorAsync(ARAnchorManager anchorManager, ARAnchor anchor)
        {
            var resultStatus = await anchorManager.TryShareAnchorAsync(anchor);
            if (resultStatus.IsError())
            {
                // Handle error
                return;
            }

            // Anchor was successfully shared.
        }
    }
    #endregion
}
