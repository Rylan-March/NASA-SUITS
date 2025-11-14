using System;

namespace UnityEngine.XR.Hands.Capture.Recording
{
    /// <summary>
    /// The status of an <see cref="XRHandRecordingBlob"/> recording session.
    /// </summary>
    public enum XRHandRecordingStatus
    {
        /// <summary>
        /// The status is unknown or the recording is not initialized.
        /// </summary>
        Unknown,

        /// <summary>
        /// The recording is initialized and ready to start.
        /// </summary>
        Ready,

        /// <summary>
        /// The recording has started.
        /// </summary>
        /// <remarks>
        /// This status is reported once when the first frame is captured.
        /// </remarks>
        Recording,

        /// <summary>
        /// The recording was stopped manually.
        /// </summary>
        StoppedManually,

        /// <summary>
        /// The recording stopped automatically because it reached the time limit
        /// specified in <see cref="XRHandRecordingSettings.timeLimitInSeconds"/>.
        /// </summary>
        StoppedAtTimeLimit,

        /// <summary>
        /// The recording stopped due to an error.
        /// Call <see cref="XRHandRecordingStatusChangedEventArgs.TryGetErrorMessage"/>
        /// to retrieve the error message.
        /// </summary>
        StoppedWithError,

        /// <summary>
        /// The recording data has been successfully saved to disk.
        /// </summary>
        Saved,
    }

    /// <summary>
    /// Provides details about recording status changes.
    /// </summary>
    public readonly struct XRHandRecordingStatusChangedEventArgs : IEquatable<XRHandRecordingStatusChangedEventArgs>
    {
        /// <summary>
        /// The new status of the recording session.
        /// </summary>
        public XRHandRecordingStatus status { get; internal init; }

        /// <summary>
        /// The elapsed time of the recording since it started in seconds.
        /// </summary>
        public float elapsedTime { get; internal init; }

        /// <summary>
        /// The name of the recording.
        /// </summary>
        public string recordingName { get; internal init; }

        internal string errorMessage { get; init; }

        /// <summary>
        /// Attempts to get the error message if the recording was stopped due to an error.
        /// </summary>
        /// <param name="errorMessage">Assigned the error message, if successful; otherwise, <c>null</c>.</param>
        /// <returns><see langword="true"/> if an error message was present;
        /// otherwise, <see langword="false"/>.</returns>
        /// <remarks>This method can only succeed if the recording was stopped with
        /// the <see cref="XRHandRecordingStatus.StoppedWithError"/> status.</remarks>
        public bool TryGetErrorMessage(out string errorMessage)
        {
            if (string.IsNullOrEmpty(this.errorMessage))
            {
                errorMessage = null;
                return false;
            }

            errorMessage = this.errorMessage;
            return true;
        }

        /// <summary>
        /// The <see cref="XRHandSubsystem"/> instance associated with the recording session.
        /// </summary>
        public XRHandSubsystem subsystem { get; internal init; }

        /// <summary>
        /// Computes a hash code from all fields of <see cref="XRHandRecordingStatusChangedEventArgs"/>.
        /// </summary>
        /// <returns>Returns a hash code of this object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = status.GetHashCode();
                hash = hash * 486187739 + elapsedTime.GetHashCode();
                hash = hash * 486187739 + recordingName.GetHashCode();
                hash = hash * 486187739 + errorMessage.GetHashCode();
                hash = hash * 486187739 + subsystem.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The <see cref="XRHandRecordingStatusChangedEventArgs"/> to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if every field in <paramref name="other"/>
        /// is equal to this <see cref="XRHandRecordingStatusChangedEventArgs"/>.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(XRHandRecordingStatusChangedEventArgs other)
        {
            return status == other.status &&
                elapsedTime == other.elapsedTime &&
                recordingName == other.recordingName &&
                errorMessage == other.errorMessage &&
                subsystem == other.subsystem;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="obj"/> is of
        /// type <see cref="XRHandRecordingStatusChangedEventArgs"/> and
        /// <see cref="Equals(XRHandRecordingStatusChangedEventArgs)"/> also
        /// returns <see langword="true"/>; otherwise returns <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj) =>
            (obj is XRHandRecordingStatusChangedEventArgs) && Equals((XRHandRecordingStatusChangedEventArgs)obj);

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRHandRecordingStatusChangedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="lhs"/> is equal
        /// to <paramref name="rhs"/>, otherwise returns <see langword="false"/>.
        /// </returns>
        public static bool operator ==(
            XRHandRecordingStatusChangedEventArgs lhs,
            XRHandRecordingStatusChangedEventArgs rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRHandRecordingStatusChangedEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="lhs"/>
        /// is not equal to <paramref name="rhs"/>, otherwise returns
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator !=(
            XRHandRecordingStatusChangedEventArgs lhs,
            XRHandRecordingStatusChangedEventArgs rhs) => !lhs.Equals(rhs);
    }
}
