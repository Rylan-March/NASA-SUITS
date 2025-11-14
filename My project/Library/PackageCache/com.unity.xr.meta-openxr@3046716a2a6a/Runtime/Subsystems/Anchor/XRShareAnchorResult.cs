// ReSharper disable ConvertToAutoProperty
using static UnityEngine.XR.ARSubsystems.XRResultStatus;

namespace UnityEngine.XR.ARSubsystems
{
    /// <summary>
    /// Represents the results, per anchor, of a share anchor operation.
    /// </summary>
    public readonly struct XRShareAnchorResult
    {
        /// <summary>
        /// The status of the share anchor operation. If `resultStatus.IsError()`, the anchor was not shared.
        /// </summary>
        public XRResultStatus resultStatus => m_ResultStatus;
        readonly XRResultStatus m_ResultStatus;

        /// <summary>
        /// The `TrackableId` of the anchor requested to be shared.
        /// </summary>
        public TrackableId anchorId => m_AnchorId;
        readonly TrackableId m_AnchorId;

        /// <summary>
        /// Construct an instance.
        /// </summary>
        /// <param name="resultStatus">The status of the completed share operation.</param>
        /// <param name="anchorId">The anchor ID requested to be shared.</param>
        public XRShareAnchorResult(XRResultStatus resultStatus, TrackableId anchorId)
        {
            m_ResultStatus = resultStatus;
            m_AnchorId = anchorId;
        }

        /// <summary>
        /// Get a default instance, initialized with <see cref="StatusCode.UnknownError">StatusCode.UnknownError</see>.
        /// </summary>
        public static XRShareAnchorResult defaultValue =>
            new(new XRResultStatus(StatusCode.UnknownError), TrackableId.invalidId);
    }
}
