namespace UnityEngine.XR.Hands.Capture.Recording
{
    /// <summary>
    /// Global settings for the recording session.
    /// </summary>
    public static class XRHandRecordingSettings
    {
        /// <summary>
        /// The maximum recording duration in seconds.
        /// A recording is stopped automatically when this limit is reached.
        /// Default value is 60 seconds.
        /// </summary>
        public static float timeLimitInSeconds { get; set; }

        static XRHandRecordingSettings()
        {
            timeLimitInSeconds = 60.0f;
        }
    }
}
