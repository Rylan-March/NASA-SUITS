using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// This class contains platform-specific extensions for [XRResultStatus](xref:UnityEngine.XR.ARSubsystems.XRResultStatus).
    /// </summary>
    public static class XRResultStatusExtensions
    {
        /// <summary>
        /// Convert AR Foundation's generic `XRResultStatus` to the OpenXR-specific `XrResult` type, allowing you to access
        /// the specific success codes and error codes returned by the OpenXR platform.
        /// </summary>
        /// <param name="status">The `XRResultStatus`.</param>
        /// <returns>The converted `XrResult`.</returns>
        public static XrResult AsXrResult(this XRResultStatus status)
        {
            return (XrResult)status.nativeStatusCode;
        }
    }
}
