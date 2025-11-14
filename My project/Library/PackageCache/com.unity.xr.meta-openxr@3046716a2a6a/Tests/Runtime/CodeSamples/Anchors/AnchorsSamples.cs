using System;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    public class AnchorsSamples
    {
        public async void AddAnchorAsync_GetNativeXRResult()
        {
            #region anchors_resultstatus_xrresult
            // This is not optimal. For better performance, reuse a saved reference instead.
            var anchorManager = Object.FindAnyObjectByType<ARAnchorManager>();

            // Create an anchor at an arbitrary pose.
            // You could modify this code to use the position of a raycast hit instead.
            var pose = new Pose(Vector3.zero, Quaternion.identity);
            var result = await anchorManager.TryAddAnchorAsync(pose);

            // To access OpenXR error and success codes, use the nativeStatusCode property
            var xrResult = (XrResult)result.status.nativeStatusCode;

            // Prints "Success", or the error or success code associated with this operation
            Debug.Log(xrResult);
            #endregion
        }
    }
}
