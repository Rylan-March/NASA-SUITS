#if UNITY_EDITOR && ENABLE_CLOUD_SERVICES_ANALYTICS
namespace UnityEngine.XR.Hands.Analytics
{
    /// <summary>
    /// This class is used to track the state of whether the XRHandShapeDebugUI was used during Play Mode.
    /// It is not intended to be used for anything else. Users do not need to use this class.
    /// </summary>
    public static class XRHandAnalyticsData
    {
        /// <summary>
        /// This flag will be set to true in Play Mode when analytics are enabled and XRHandShapeDebugUI is used.
        /// </summary>
        public static bool xrHandCustomGestureDebugActive
        {
            get => XRHandFeatureUsageData.xrHandCustomGestureDebuggerUsed;
            set => XRHandFeatureUsageData.xrHandCustomGestureDebuggerUsed = value;
        }
    }
}
#endif // UNITY_EDITOR && ENABLE_CLOUD_SERVICES_ANALYTICS
