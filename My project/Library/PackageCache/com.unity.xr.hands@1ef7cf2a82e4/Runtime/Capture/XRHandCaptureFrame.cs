using System;
using UnityEngine;

namespace UnityEngine.XR.Hands.Capture
{
    /// <summary>
    /// A single frame of captured hand data.
    /// </summary>
    [Serializable]
    public struct XRHandCaptureFrame : IEquatable<XRHandCaptureFrame>
    {
        [SerializeField]
        [Tooltip("The captured left hand joints.")]
        XRHandJoint[] m_LeftHandJoints;

        [SerializeField]
        [Tooltip("The captured right hand joints.")]
        XRHandJoint[] m_RightHandJoints;

        [SerializeField]
        [Tooltip("The timestamp of the captured frame in seconds.")]
        float m_Timestamp;

        [SerializeField]
        [Tooltip("Indicates whether the left hand is tracked in this frame.")]
        bool m_IsLeftHandTracked;

        [SerializeField]
        [Tooltip("Indicates whether the right hand is tracked in this frame.")]
        bool m_IsRightHandTracked;

        [SerializeField]
        [Tooltip("Indicates whether all left hand joints are valid in this frame.")]
        bool m_AreAllLeftJointsValid;

        [SerializeField]
        [Tooltip("Indicates whether all right hand joints are valid in this frame.")]
        bool m_AreAllRightJointsValid;

        internal bool isLeftHandTracked
        {
            get => m_IsLeftHandTracked;
            set => m_IsLeftHandTracked = value;
        }
        internal bool isRightHandTracked
        {
            get => m_IsRightHandTracked;
            set => m_IsRightHandTracked = value;
        }

        internal bool areAllLeftJointsValid
        {
            get => m_AreAllLeftJointsValid;
            set => m_AreAllLeftJointsValid = value;
        }

        internal bool areAllRightJointsValid
        {
            get => m_AreAllRightJointsValid;
            set => m_AreAllRightJointsValid = value;
        }

        internal XRHandJoint[] leftHandJoints
        {
            get => m_LeftHandJoints;
            set => m_LeftHandJoints = value;
        }

        internal XRHandJoint[] rightHandJoints
        {
            get => m_RightHandJoints;
            set => m_RightHandJoints = value;
        }

        /// <summary>
        /// The timestamp of this captured frame in seconds since the start of the recording.
        /// </summary>
        /// <value>The time at which this frame was captured.</value>
        public float timestamp
        {
            get => m_Timestamp;
            internal set => m_Timestamp = value;
        }

        /// <summary>
        /// Reports whether the frame contains the specified hand.
        /// </summary>
        /// <param name="handedness">The <see cref="Handedness"/> of the hand to check.</param>
        /// <returns>
        /// <c>true</c> if data for the specified hand is available; otherwise, <c>false</c>.
        /// </returns>
        public bool IsHandTracked(Handedness handedness)
        {
            return handedness == Handedness.Left ? m_IsLeftHandTracked : m_IsRightHandTracked;
        }

        /// <summary>
        /// Attempts to retrieve the joint data for the specified hand and joint ID.
        /// </summary>
        /// <param name="joint">The output parameter to store the retrieved joint data.</param>
        /// <param name="handedness">The <see cref="Handedness"/> of the hand to retrieve the joint data for.</param>
        /// <param name="id">The ID of the joint to retrieve.</param>
        /// <returns>
        /// <c>true</c> if the joint data was successfully retrieved; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetJoint(out XRHandJoint joint, Handedness handedness, XRHandJointID id)
        {
            var joints = handedness == Handedness.Left ? m_LeftHandJoints : m_RightHandJoints;
            bool isValid = joints != null && joints.Length != 0;
            joint = isValid ? joints[id.ToIndex()] : default;
            return isValid;
        }

        /// <summary>
        /// Computes a hash code from all fields of <see cref="XRHandCaptureFrame"/>.
        /// </summary>
        /// <returns>Returns a hash code of this object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = m_LeftHandJoints.GetHashCode();
                hash = hash * 486187739 + m_RightHandJoints.GetHashCode();
                hash = hash * 486187739 + m_Timestamp.GetHashCode();
                hash = hash * 486187739 + m_IsLeftHandTracked.GetHashCode();
                hash = hash * 486187739 + m_IsRightHandTracked.GetHashCode();
                hash = hash * 486187739 + m_AreAllLeftJointsValid.GetHashCode();
                hash = hash * 486187739 + m_AreAllRightJointsValid.GetHashCode();
                return hash;
            }
        }
        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The <see cref="XRHandCaptureFrame"/> to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if every field in <paramref name="other"/>
        /// is equal to this <see cref="XRHandCaptureFrame"/>.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public bool Equals(XRHandCaptureFrame other)
        {
            return ReferenceEquals(m_LeftHandJoints, other.m_LeftHandJoints) &&
                ReferenceEquals(m_RightHandJoints, other.m_RightHandJoints) &&
                m_Timestamp == other.m_Timestamp &&
                m_IsLeftHandTracked == other.m_IsLeftHandTracked &&
                m_IsRightHandTracked == other.m_IsRightHandTracked &&
                m_AreAllLeftJointsValid == other.m_AreAllLeftJointsValid &&
                m_AreAllRightJointsValid == other.m_AreAllRightJointsValid;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="obj"/> is of
        /// type <see cref="XRHandCaptureFrame"/> and <see cref="Equals(XRHandCaptureFrame)"/> also
        /// returns <see langword="true"/>; otherwise returns <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj) => (obj is XRHandCaptureFrame) && Equals((XRHandCaptureFrame)obj);

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRHandCaptureFrame)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="lhs"/> is equal
        /// to <paramref name="rhs"/>, otherwise returns <see langword="false"/>.
        /// </returns>
        public static bool operator ==(XRHandCaptureFrame lhs, XRHandCaptureFrame rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRHandCaptureFrame)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="lhs"/>
        /// is not equal to <paramref name="rhs"/>, otherwise returns
        /// <see langword="false"/>.
        /// </returns>
        public static bool operator !=(XRHandCaptureFrame lhs, XRHandCaptureFrame rhs) => !lhs.Equals(rhs);
    }
}
