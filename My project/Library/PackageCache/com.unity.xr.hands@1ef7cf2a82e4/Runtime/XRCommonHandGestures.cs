using System;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// Access to common hand gesture data and callbacks.
    /// </summary>
    public class XRCommonHandGestures
    {
        /// <summary>
        /// Event-args type for when the aim pose updates.
        /// </summary>
        public class AimPoseUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the aim pose.
            /// </summary>
            /// <param name="aimPose">
            /// Will be filled out with the aim pose, if successful.
            /// </param>
            /// <returns>
            /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
            /// </returns>
            public bool TryGetAimPose(out Pose aimPose)
            {
                aimPose = m_IsAimPoseTracked ? m_AimPose : Pose.identity;
                return m_IsAimPoseTracked;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal AimPoseUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal Pose m_AimPose;
            internal bool m_IsAimPoseTracked;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Event-args type for when the aim activation value updates.
        /// </summary>
        public class AimActivateValueUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the aim activate value.
            /// </summary>
            /// <param name="aimActivateValue">
            /// Will be filled out with the aim activate value, if successful.
            /// </param>
            /// <returns>
            /// Returns <see langword="true"/> and a valid value is filled out.
            /// Returns <see langword="false"/> otherwise.
            /// </returns>
            public bool TryGetAimActivateValue(out float aimActivateValue)
            {
                aimActivateValue = m_IsAimActivateValueReady ? m_AimActivateValue : 0f;
                return m_IsAimActivateValueReady;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal AimActivateValueUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal float m_AimActivateValue;
            internal bool m_IsAimActivateValueReady;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Event-args type for when the grasp value updates.
        /// </summary>
        public class GraspValueUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the grasp value.
            /// </summary>
            /// <param name="graspValue">
            /// Will be filled out with the grasp value, if successful.
            /// </param>
            /// <returns>
            /// Returns <see langword="true"/> and a valid value is filled out.
            /// Returns <see langword="false"/> otherwise.
            /// </returns>
            public bool TryGetGraspValue(out float graspValue)
            {
                graspValue = m_IsGraspValueReady ? m_GraspValue : 0f;
                return m_IsGraspValueReady;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal GraspValueUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal float m_GraspValue;
            internal bool m_IsGraspValueReady;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Event-args type for when the grip pose updates.
        /// </summary>
        public class GripPoseUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the grip pose.
            /// </summary>
            /// <param name="gripPose">
            /// Will be filled out with the grip pose, if successful.
            /// </param>
            /// <returns>
            /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
            /// </returns>
            public bool TryGetGripPose(out Pose gripPose)
            {
                gripPose = m_IsGripPoseTracked ? m_GripPose : Pose.identity;
                return m_IsGripPoseTracked;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal GripPoseUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal Pose m_GripPose;
            internal bool m_IsGripPoseTracked;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Event-args type for when the pinch pose updates.
        /// </summary>
        public class PinchPoseUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the pinch pose.
            /// </summary>
            /// <param name="pinchPose">
            /// Will be filled out with the pinch pose, if successful.
            /// </param>
            /// <returns>
            /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
            /// </returns>
            public bool TryGetPinchPose(out Pose pinchPose)
            {
                pinchPose = m_IsPinchPoseTracked ? m_PinchPose : Pose.identity;
                return m_IsPinchPoseTracked;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal PinchPoseUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal Pose m_PinchPose;
            internal bool m_IsPinchPoseTracked;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Event-args type for when the pinch value updates.
        /// </summary>
        public class PinchValueUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the pinch value.
            /// </summary>
            /// <param name="pinchValue">
            /// Will be filled out with the pinch value, if successful.
            /// </param>
            /// <returns>
            /// Returns <see langword="true"/> and a valid value is filled out.
            /// Returns <see langword="false"/> otherwise.
            /// </returns>
            public bool TryGetPinchValue(out float pinchValue)
            {
                pinchValue = m_IsPinchValueReady ? m_PinchValue : 0f;
                return m_IsPinchValueReady;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal PinchValueUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal float m_PinchValue;
            internal bool m_IsPinchValueReady;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Event-args type for when the poke pose updates.
        /// </summary>
        public class PokePoseUpdatedEventArgs
        {
            /// <summary>
            /// Attempts to get the poke pose.
            /// </summary>
            /// <param name="pokePose">
            /// Will be filled out with the poke pose, if successful.
            /// </param>
            /// <returns>
            /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
            /// </returns>
            public bool TryGetPokePose(out Pose pokePose)
            {
                pokePose = m_IsPokePoseTracked ? m_PokePose : Pose.identity;
                return m_IsPokePoseTracked;
            }

            /// <summary>
            /// Which hand is being updated.
            /// </summary>
            public Handedness handedness => m_Handedness;

            internal PokePoseUpdatedEventArgs(Handedness handedness) => m_Handedness = handedness;

            internal Pose m_PokePose;
            internal bool m_IsPokePoseTracked;
            readonly Handedness m_Handedness;
        }

        /// <summary>
        /// Attempts to get the aim pose.
        /// </summary>
        /// <param name="aimPose">
        /// Will be filled out with the aim pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
        /// </returns>
        public bool TryGetAimPose(out Pose aimPose)
        {
            if (m_AimPose == null)
            {
                aimPose = Pose.identity;
                return false;
            }

            return m_AimPose.TryGetAimPose(out aimPose);
        }

        /// <summary>
        /// Attempts to get the aim activate value.
        /// </summary>
        /// <param name="aimActivateValue">
        /// Will be filled out with the aim activate value, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> and a valid value is filled out.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public bool TryGetAimActivateValue(out float aimActivateValue)
        {
            if (m_AimActivateValue == null)
            {
                aimActivateValue = 0f;
                return false;
            }

            return m_AimActivateValue.TryGetAimActivateValue(out aimActivateValue);
        }

        /// <summary>
        /// Attempts to get the grasp value.
        /// </summary>
        /// <param name="graspValue">
        /// Will be filled out with the grasp value, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> and a valid value is filled out.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public bool TryGetGraspValue(out float graspValue)
        {
            if (m_GraspValue == null)
            {
                graspValue = 0f;
                return false;
            }

            return m_GraspValue.TryGetGraspValue(out graspValue);
        }

        /// <summary>
        /// Attempts to get the grip pose.
        /// </summary>
        /// <param name="gripPose">
        /// Will be filled out with the grip pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
        /// </returns>
        public bool TryGetGripPose(out Pose gripPose)
        {
            if (m_GripPose == null)
            {
                gripPose = Pose.identity;
                return false;
            }

            return m_GripPose.TryGetGripPose(out gripPose);
        }

        /// <summary>
        /// Attempts to get the pinch pose.
        /// </summary>
        /// <param name="pinchPose">
        /// Will be filled out with the pinch pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
        /// </returns>
        public bool TryGetPinchPose(out Pose pinchPose)
        {
            if (m_PinchPose == null)
            {
                pinchPose = Pose.identity;
                return false;
            }

            return m_PinchPose.TryGetPinchPose(out pinchPose);
        }

        /// <summary>
        /// Attempts to get the pinch value.
        /// </summary>
        /// <param name="pinchValue">
        /// Will be filled out with the pinch value, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> and a valid value is filled out.
        /// Returns <see langword="false"/> otherwise.
        /// </returns>
        public bool TryGetPinchValue(out float pinchValue)
        {
            if (m_PinchValue == null)
            {
                pinchValue = 0f;
                return false;
            }

            return m_PinchValue.TryGetPinchValue(out pinchValue);
        }

        /// <summary>
        /// Attempts to get the poke pose.
        /// </summary>
        /// <param name="pokePose">
        /// Will be filled out with the poke pose, if successful.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful, returns <see langword="false"/> otherwise.
        /// </returns>
        public bool TryGetPokePose(out Pose pokePose)
        {
            if (m_PokePose == null)
            {
                pokePose = Pose.identity;
                return false;
            }

            return m_PokePose.TryGetPokePose(out pokePose);
        }

        /// <summary>
        /// Called when the aim pose is updated. Either the pose changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<AimPoseUpdatedEventArgs> aimPoseUpdated;

        /// <summary>
        /// Called when the aim activate value is updated. Either the value changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<AimActivateValueUpdatedEventArgs> aimActivateValueUpdated;

        /// <summary>
        /// Called when the grasp value is updated. Either the value changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<GraspValueUpdatedEventArgs> graspValueUpdated;

        /// <summary>
        /// Called when the grip pose is updated. Either the pose changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<GripPoseUpdatedEventArgs> gripPoseUpdated;

        /// <summary>
        /// Called when the pinch pose is updated. Either the pose changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<PinchPoseUpdatedEventArgs> pinchPoseUpdated;

        /// <summary>
        /// Called when the pinch value is updated. Either the value changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<PinchValueUpdatedEventArgs> pinchValueUpdated;

        /// <summary>
        /// Called when the poke pose is updated. Either the pose changed,
        /// or the ability to retrieve it changed.
        /// </summary>
        public Action<PokePoseUpdatedEventArgs> pokePoseUpdated;

        internal void UpdateAimPose(Pose aimPose)
        {
            if (m_AimPose == null)
                m_AimPose = new AimPoseUpdatedEventArgs(m_Handedness);

            bool fire = aimPoseUpdated != null &&
                (!m_AimPose.m_IsAimPoseTracked || aimPose != m_AimPose.m_AimPose);

            m_AimPose.m_IsAimPoseTracked = true;
            m_AimPose.m_AimPose = aimPose;

            if (fire)
                aimPoseUpdated.Invoke(m_AimPose);
        }

        internal void InvalidateAimPose()
        {
            if (m_AimPose == null)
                m_AimPose = new AimPoseUpdatedEventArgs(m_Handedness);

            bool fire = aimPoseUpdated != null && m_AimPose.m_IsAimPoseTracked;

            m_AimPose.m_IsAimPoseTracked = false;

            if (fire)
                aimPoseUpdated.Invoke(m_AimPose);
        }

        internal void UpdateAimActivateValue(float aimActivateValue)
        {
            if (m_AimActivateValue == null)
                m_AimActivateValue = new AimActivateValueUpdatedEventArgs(m_Handedness);

            bool fire = aimActivateValueUpdated != null &&
                (!m_AimActivateValue.m_IsAimActivateValueReady || aimActivateValue != m_AimActivateValue.m_AimActivateValue);

            m_AimActivateValue.m_IsAimActivateValueReady = true;
            m_AimActivateValue.m_AimActivateValue = aimActivateValue;

            if (fire)
                aimActivateValueUpdated.Invoke(m_AimActivateValue);
        }

        internal void InvalidateAimActivateValue()
        {
            if (m_AimActivateValue == null)
                m_AimActivateValue = new AimActivateValueUpdatedEventArgs(m_Handedness);

            bool fire = aimActivateValueUpdated != null && m_AimActivateValue.m_IsAimActivateValueReady;

            m_AimActivateValue.m_IsAimActivateValueReady = false;

            if (fire)
                aimActivateValueUpdated.Invoke(m_AimActivateValue);
        }

        internal void UpdateGraspValue(float graspValue)
        {
            if (m_GraspValue == null)
                m_GraspValue = new GraspValueUpdatedEventArgs(m_Handedness);

            bool fire = graspValueUpdated != null &&
                (!m_GraspValue.m_IsGraspValueReady || graspValue != m_GraspValue.m_GraspValue);

            m_GraspValue.m_IsGraspValueReady = true;
            m_GraspValue.m_GraspValue = graspValue;

            if (fire)
                graspValueUpdated.Invoke(m_GraspValue);
        }

        internal void InvalidateGraspValue()
        {
            if (m_GraspValue == null)
                m_GraspValue = new GraspValueUpdatedEventArgs(m_Handedness);

            bool fire = graspValueUpdated != null && m_GraspValue.m_IsGraspValueReady;

            m_GraspValue.m_IsGraspValueReady = false;

            if (fire)
                graspValueUpdated.Invoke(m_GraspValue);
        }

        internal void UpdateGripPose(Pose gripPose)
        {
            if (m_GripPose == null)
                m_GripPose = new GripPoseUpdatedEventArgs(m_Handedness);

            bool fire = gripPoseUpdated != null &&
                (!m_GripPose.m_IsGripPoseTracked || gripPose != m_GripPose.m_GripPose);

            m_GripPose.m_IsGripPoseTracked = true;
            m_GripPose.m_GripPose = gripPose;

            if (fire)
                gripPoseUpdated.Invoke(m_GripPose);
        }

        internal void InvalidateGripPose()
        {
            if (m_GripPose == null)
                m_GripPose = new GripPoseUpdatedEventArgs(m_Handedness);

            bool fire = gripPoseUpdated != null && m_GripPose.m_IsGripPoseTracked;

            m_GripPose.m_IsGripPoseTracked = false;

            if (fire)
                gripPoseUpdated.Invoke(m_GripPose);
        }

        internal void UpdatePinchPose(Pose pinchPose)
        {
            if (m_PinchPose == null)
                m_PinchPose = new PinchPoseUpdatedEventArgs(m_Handedness);

            bool fire = pinchPoseUpdated != null &&
                (!m_PinchPose.m_IsPinchPoseTracked || pinchPose != m_PinchPose.m_PinchPose);

            m_PinchPose.m_IsPinchPoseTracked = true;
            m_PinchPose.m_PinchPose = pinchPose;

            if (fire)
                pinchPoseUpdated.Invoke(m_PinchPose);
        }

        internal void InvalidatePinchPose()
        {
            if (m_PinchPose == null)
                m_PinchPose = new PinchPoseUpdatedEventArgs(m_Handedness);

            bool fire = pinchPoseUpdated != null && m_PinchPose.m_IsPinchPoseTracked;

            m_PinchPose.m_IsPinchPoseTracked = false;

            if (fire)
                pinchPoseUpdated.Invoke(m_PinchPose);
        }

        internal void UpdatePinchValue(float pinchValue)
        {
            if (m_PinchValue == null)
                m_PinchValue = new PinchValueUpdatedEventArgs(m_Handedness);

            bool fire = pinchValueUpdated != null &&
                (!m_PinchValue.m_IsPinchValueReady || pinchValue != m_PinchValue.m_PinchValue);

            m_PinchValue.m_IsPinchValueReady = true;
            m_PinchValue.m_PinchValue = pinchValue;

            if (fire)
                pinchValueUpdated.Invoke(m_PinchValue);
        }

        internal void InvalidatePinchValue()
        {
            if (m_PinchValue == null)
                m_PinchValue = new PinchValueUpdatedEventArgs(m_Handedness);

            bool fire = pinchValueUpdated != null && m_PinchValue.m_IsPinchValueReady;

            m_PinchValue.m_IsPinchValueReady = false;

            if (fire)
                pinchValueUpdated.Invoke(m_PinchValue);
        }

        internal void UpdatePokePose(Pose pokePose)
        {
            if (m_PokePose == null)
                m_PokePose = new PokePoseUpdatedEventArgs(m_Handedness);

            bool fire = pokePoseUpdated != null &&
                (!m_PokePose.m_IsPokePoseTracked || pokePose != m_PokePose.m_PokePose);

            m_PokePose.m_IsPokePoseTracked = true;
            m_PokePose.m_PokePose = pokePose;

            if (fire)
                pokePoseUpdated.Invoke(m_PokePose);
        }

        internal void InvalidatePokePose()
        {
            if (m_PokePose == null)
                m_PokePose = new PokePoseUpdatedEventArgs(m_Handedness);

            bool fire = pokePoseUpdated != null && m_PokePose.m_IsPokePoseTracked;

            m_PokePose.m_IsPokePoseTracked = false;

            if (fire)
                pokePoseUpdated.Invoke(m_PokePose);
        }

        internal XRCommonHandGestures(Handedness handedness) => m_Handedness = handedness;

        readonly Handedness m_Handedness;
        AimPoseUpdatedEventArgs m_AimPose;
        AimActivateValueUpdatedEventArgs m_AimActivateValue;
        GraspValueUpdatedEventArgs m_GraspValue;
        GripPoseUpdatedEventArgs m_GripPose;
        PinchPoseUpdatedEventArgs m_PinchPose;
        PinchValueUpdatedEventArgs m_PinchValue;
        PokePoseUpdatedEventArgs m_PokePose;
    }
}
