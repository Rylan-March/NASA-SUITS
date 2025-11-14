using System;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    #region TryShareAnchorsAsync
    using UnityEngine.XR.OpenXR.Features.Meta;

    public class BatchShareAnchorsSample
    {
        async void ShareAnchorsAsync(
            ARAnchorManager anchorManager, IEnumerable<ARAnchor> anchors)
        {
            var results = new List<XRShareAnchorResult>();
            await anchorManager.TryShareAnchorsAsync(anchors, results);

            foreach (var result in results)
            {
                if (result.resultStatus.IsSuccess())
                {
                    // Anchor with results.anchorId was successfully shared.
                }
                else
                {
                    // Anchor with results.anchorId failed to share.
                }
            }
        }
    }
    #endregion
}
