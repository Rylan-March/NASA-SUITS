using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Contains extension methods for <see cref="XRFace"/> and <see cref="ARFace"/>
    /// that are relevant to the Android XR platform.
    /// </summary>
    public static class AndroidOpenXRFaceExtensions
    {
        /// <summary>
        /// Extension method to <see cref="XRFace"/> that attempts to retrieve
        /// Android XR-specific face tracking state.
        /// </summary>
        /// <param name="xrFace">
        /// <see cref="XRFace"/> to get <see cref="AndroidOpenXRFaceTrackingStates"/> for.
        /// </param>
        /// <param name="states">
        /// <see cref="AndroidOpenXRFaceTrackingStates"/> to fill out with up-to-date data,
        /// if the method returns <see langword="true"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and state was filled
        /// out with up-to-date data, returns <see langword="false"/> otherwise.
        /// </returns>
        public static bool TryGetAndroidOpenXRFaceTrackingStates(this XRFace xrFace, out AndroidOpenXRFaceTrackingStates states)
            => TryGetAndroidOpenXRFaceTrackingStates(out states, xrFace.trackableId);

        /// <summary>
        /// Extension method to <see cref="ARFace"/> that attempts to retrieve
        /// Android XR-specific face tracking state.
        /// </summary>
        /// <param name="arFace">
        /// <see cref="ARFace"/> to get <see cref="AndroidOpenXRFaceTrackingStates"/> for.
        /// </param>
        /// <param name="states">
        /// <see cref="AndroidOpenXRFaceTrackingStates"/> to fill out with up-to-date data,
        /// if the method returns <see langword="true"/>.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and state was filled
        /// out with up-to-date data, returns <see langword="false"/> otherwise.
        /// </returns>
        public static bool TryGetAndroidOpenXRFaceTrackingStates(this ARFace arFace, out AndroidOpenXRFaceTrackingStates states)
            => TryGetAndroidOpenXRFaceTrackingStates(out states, arFace.trackableId);

        static bool TryGetAndroidOpenXRFaceTrackingStates(out AndroidOpenXRFaceTrackingStates states, TrackableId id)
        {
            if (AndroidOpenXRFaceSubsystem.instance == null)
            {
                states = AndroidOpenXRFaceTrackingStates.None;
                return false;
            }

            return AndroidOpenXRFaceSubsystem.instance.TryGetExtraFaceTrackingStates(out states, id);
        }
    }
}
