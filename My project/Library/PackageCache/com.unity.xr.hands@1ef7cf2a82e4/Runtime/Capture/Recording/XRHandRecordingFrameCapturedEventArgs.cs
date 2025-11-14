namespace UnityEngine.XR.Hands.Capture.Recording
{
    /// <summary>
    /// Provides details about a new frame captured during a recording session.
    /// </summary>
    public readonly struct XRHandRecordingFrameCapturedEventArgs
    {
        /// <summary>
        /// The elapsed time since the recording started.
        /// </summary>
        public float elapsedTime { get; internal init; }

        /// <summary>
        /// The <see cref="XRHandSubsystem"/> instance associated with the recording session.
        /// </summary>
        public XRHandSubsystem subsystem { get; internal init; }

        /// <summary>
        /// The <see cref="XRHandSubsystem.updateSuccessFlags"/> flags indicating what data were updated in this frame.
        /// </summary>
        public XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags { get; internal init; }

        /// <summary>
        /// The index of the captured frame in the recording session.
        /// </summary>
        /// <remarks> The index is zero-based </remarks>
        public int frameIndex { get; internal init; }
    }
}
