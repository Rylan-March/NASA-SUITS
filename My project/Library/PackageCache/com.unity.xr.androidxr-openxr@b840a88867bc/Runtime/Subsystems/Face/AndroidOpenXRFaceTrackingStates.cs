using System;

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// The Android-OpenXR representation of platform-specific eye gaze data.
    /// </summary>
    [Flags]
    public enum AndroidOpenXRFaceTrackingStates
    {
        /// <summary>
        /// Nothing represented in this enum is valid.
        /// </summary>
        None,

        /// <summary>
        /// The pose data for the left eye is valid and was updated this frame.
        /// </summary>
        LeftEyePoseValid = 1 << 0,

        /// <summary>
        /// The left eye is shut. This flag is mutually exclusive with <see cref="LeftEyeGazing"/>.
        /// </summary>
        LeftEyeShut = 1 << 1,

        /// <summary>
        /// The left eye is gazing. This flag is mutually exclusive with <see cref="LeftEyeShut"/>.
        /// </summary>
        LeftEyeGazing = 1 << 2,

        /// <summary>
        /// The pose data for the right eye is valid and was updated this frame.
        /// </summary>
        RightEyePoseValid = 1 << 3,

        /// <summary>
        /// The right eye is shut. This flag is mutually exclusive with <see cref="RightEyeGazing"/>.
        /// </summary>
        RightEyeShut = 1 << 4,

        /// <summary>
        /// The right eye is gazing. This flag is mutually exclusive with <see cref="RightEyeShut"/>.
        /// </summary>
        RightEyeGazing = 1 << 5,
    }
}
