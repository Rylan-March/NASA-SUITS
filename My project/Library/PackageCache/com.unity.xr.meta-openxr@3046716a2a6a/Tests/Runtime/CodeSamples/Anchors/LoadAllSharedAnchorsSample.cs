using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Collections;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    #region TryLoadAllSharedAnchorsAsync
    using UnityEngine.XR.OpenXR.Features.Meta;

    public class LoadAllSharedAnchorsSample
    {
        async void LoadAllSharedAnchorsAsync(ARAnchorManager anchorManager)
        {
            var loadedXRAnchors = new List<XRAnchor>();
            var resultStatus = await anchorManager.TryLoadAllSharedAnchorsAsync(
                loadedXRAnchors, OnIncrementalResultsAvailable);

            if (resultStatus.IsError())
            {
                // Handle error here.
                return;
            }

            // Request completed successfully.
        }

        void OnIncrementalResultsAvailable(ReadOnlyListSpan<XRAnchor> xrAnchors)
        {
            foreach (var xrAnchor in xrAnchors)
            {
                // To get the GameObject of the loaded anchor,
                // keep track of the xrAnchor's trackableId to
                // know which anchors in
                // `ARAnchorManager.trackablesChanged.added`
                // were added as a result of this load request.
                var anchorId = xrAnchor.trackableId;
            }
        }
    }
    #endregion
}
