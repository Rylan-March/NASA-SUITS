using System;

namespace UnityEngine.XR.Hands.Capture.Recording
{
    /// <summary>
    /// The arguments required to initialize a new recording.
    /// </summary>
    public readonly struct XRHandRecordingInitializeArgs : IEquatable<XRHandRecordingInitializeArgs>
    {
        /// <summary>
        /// The <see cref="XRHandSubsystem"/> to capture hand data from.
        /// </summary>
        /// <remarks> The subsystem is expected to be running for the duration of the recording.
        /// If the subsystem is stopped, unexpected behavior may occur.
        /// </remarks>
        public XRHandSubsystem subsystem { get; init; }

        /// <summary>
        /// Computes a hash code from all fields of <see cref="XRHandRecordingInitializeArgs"/>.
        /// </summary>
        /// <returns>Returns a hash code of this object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = subsystem.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The <see cref="XRHandRecordingInitializeArgs"/> to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if every field in <paramref name="other"/>
        /// is equal to this <see cref="XRHandRecordingInitializeArgs"/>.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(XRHandRecordingInitializeArgs other)
        {
            return ReferenceEquals(subsystem, other.subsystem);
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="obj"/> is of
        /// type <see cref="XRHandRecordingInitializeArgs"/> and <see cref="Equals(XRHandRecordingInitializeArgs)"/>
        /// also returns <see langword="true"/>; otherwise returns <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj) =>
            (obj is XRHandRecordingInitializeArgs) && Equals((XRHandRecordingInitializeArgs)obj);

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRHandRecordingInitializeArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="lhs"/> is equal
        /// to <paramref name="rhs"/>, otherwise returns <see langword="false"/>.
        /// </returns>
        public static bool operator ==(XRHandRecordingInitializeArgs lhs, XRHandRecordingInitializeArgs rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRHandRecordingInitializeArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="lhs"/>
        /// is not equal to <paramref name="rhs"/>, otherwise returns
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator !=(XRHandRecordingInitializeArgs lhs, XRHandRecordingInitializeArgs rhs) => !lhs.Equals(rhs);
    }
}
