using System;

namespace UnityEngine.XR.Hands.Capture.Recording
{
    /// <summary>
    /// The arguments required to save a recording.
    /// </summary>
    public readonly struct XRHandRecordingSaveArgs : IEquatable<XRHandRecordingSaveArgs>
    {
        /// <summary>
        /// The name to assign to the recording.
        /// </summary>
        /// <remarks>
        /// This string is used as the recording asset name when
        /// the recording is imported into the Unity Editor.
        /// </remarks>
        public string recordingName { get; init; }

        /// <summary>
        /// Computes a hash code from all fields of <see cref="XRHandRecordingSaveArgs"/>.
        /// </summary>
        /// <returns>Returns a hash code of this object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = recordingName.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The <see cref="XRHandRecordingSaveArgs"/> to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if every field in <paramref name="other"/>
        /// is equal to this <see cref="XRHandRecordingSaveArgs"/>.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(XRHandRecordingSaveArgs other)
        {
            return string.Equals(recordingName, other.recordingName);
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="obj"/> is of
        /// type <see cref="XRHandRecordingSaveArgs"/> and <see cref="Equals(XRHandRecordingSaveArgs)"/>
        /// also returns <see langword="true"/>; otherwise returns <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj) =>
            (obj is XRHandRecordingSaveArgs) && Equals((XRHandRecordingSaveArgs)obj);

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRHandRecordingSaveArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="lhs"/> is equal
        /// to <paramref name="rhs"/>, otherwise returns <see langword="false"/>.
        /// </returns>
        public static bool operator ==(XRHandRecordingSaveArgs lhs, XRHandRecordingSaveArgs rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRHandRecordingSaveArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="lhs"/>
        /// is not equal to <paramref name="rhs"/>, otherwise returns
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator !=(XRHandRecordingSaveArgs lhs, XRHandRecordingSaveArgs rhs) => !lhs.Equals(rhs);
    }
}
