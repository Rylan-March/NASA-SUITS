using UnityEngine.XR.Hands.Analytics;
#if UNITY_EDITOR && ENABLE_CLOUD_SERVICES_ANALYTICS
namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// This class is used to track the state of whether the XRHandShapeDebugUI was used during Play Mode.
    /// It is not intended to be used for anything else.
    /// </summary>
    public static class XRHandAnalyticsFlags
    {
        /// <summary>
        /// Will be set to true in the editor, in play mode, and analytics are enabled, if the XRHandShapeDebugUI was used.
        /// </summary>
        public static bool xrHandCustomGestureDebugActive
        {
            get => XRHandFeatureUsageData.xrHandCustomGestureDebuggerUsed;
            set => XRHandFeatureUsageData.xrHandCustomGestureDebuggerUsed = value;
        }
    }
}
#endif // UNITY_EDITOR && ENABLE_CLOUD_SERVICES_ANALYTICS
