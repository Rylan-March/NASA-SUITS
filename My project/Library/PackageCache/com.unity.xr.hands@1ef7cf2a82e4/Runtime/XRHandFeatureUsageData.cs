namespace UnityEngine.XR.Hands.Analytics
{
    /// <summary>
    /// Internal struct that tracks information about the XRHands systems during a frame in PlayMode.  This is primarily used
    /// by the XRHandsAnalytics system to gather PlayMode Analytics.
    /// </summary>
    struct XRHandFeatureUsageData
    {
        static XRHandFeatureUsageData s_PlayModeInstance;

        bool m_XRHandSubsystemActive;
        bool m_XRHandCustomGestureActive;
        bool m_XRHandCustomGestureDebugActive;

        /// <summary>
        /// Flag that indicates that an XRHands subsystem is active during the Play Mode.
        /// </summary>
        public static bool xrHandSubsystemRuntimeUsed
        {
            get => s_PlayModeInstance.m_XRHandSubsystemActive;
            set => s_PlayModeInstance.m_XRHandSubsystemActive = value;
        }

        /// <summary>
        /// Flag that indicates that a user used a Custom Gesture during Play Mode.
        /// </summary>
        public static bool xrHandCustomGestureUsed
        {
            get => s_PlayModeInstance.m_XRHandCustomGestureActive;
            set => s_PlayModeInstance.m_XRHandCustomGestureActive = value;
        }

        /// <summary>
        /// Flag that indicates that a user used the XrHandShapeDebugUI during Play Mode.
        /// </summary>
        public static bool xrHandCustomGestureDebuggerUsed
        {
            get => s_PlayModeInstance.m_XRHandCustomGestureDebugActive;
            set => s_PlayModeInstance.m_XRHandCustomGestureDebugActive = value;
        }

        public static void ResetData()
        {
            s_PlayModeInstance = new XRHandFeatureUsageData();
        }
    }
}
