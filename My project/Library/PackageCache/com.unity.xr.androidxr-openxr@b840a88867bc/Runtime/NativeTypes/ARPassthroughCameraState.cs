namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Represents the state of the device's passthrough camera.
    /// </summary>
    public enum ARPassthroughCameraState
    {
        /// <summary>
        /// Camera is disabled.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Camera is coming online, but not yet ready for use.
        /// </summary>
        Initialized = 1,

        /// <summary>
        /// Camera is ready for use.
        /// </summary>
        Ready = 2,

        /// <summary>
        /// Camera is in an unrecoverable error state.
        /// </summary>
        Error = 3
    }
}
