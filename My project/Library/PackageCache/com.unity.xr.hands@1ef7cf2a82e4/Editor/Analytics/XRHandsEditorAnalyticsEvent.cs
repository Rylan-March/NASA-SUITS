// Add DEBUG_XR_HANDS_ANALYTICS_EVENT to the scripting defines to debug analytics events, you can also use the Analytics Debugger window (Unity 2023.1+)
#if ENABLE_CLOUD_SERVICES_ANALYTICS
using UnityEngine.Analytics;

#if UNITY_2023_2_OR_NEWER
using Unity.XR.CoreUtils.Editor.Analytics;
#endif

namespace UnityEditor.XR.Hands.Analytics
{
    abstract class XRHandsEditorAnalyticsEvent<T>
#if UNITY_2023_2_OR_NEWER
        : EditorAnalyticsEvent<T>
#endif
        where T : struct
#if UNITY_2023_2_OR_NEWER
        , IAnalytic.IData
#endif
    {
        protected const int k_MaxEventPerHour = 1000;
        protected const int k_MaxItems = 1000;

#if UNITY_2023_2_OR_NEWER
        protected override AnalyticsResult SendToAnalyticsServer(T parameter)
        {
            var result = EditorAnalytics.SendAnalytic(this);

#if DEBUG_XR_HANDS_ANALYTICS_EVENT
            Debug.Log($"[{GetType().Name}] parameter {JsonUtility.ToJson(parameter)} sent with status {result}.");
#endif // DEBUG_XR_HANDS_ANALYTICS_EVENT
            return result;
        }

        protected override AnalyticsResult RegisterWithAnalyticsServer() => AnalyticsResult.Ok;
#else // UNITY_2023_2_OR_NEWER

        protected internal abstract bool Send(T parameter);

        protected internal AnalyticsResult RegisterWithAnalyticsServer() => AnalyticsResult.Ok;
#endif // UNITY_2023_2_OR_NEWER
    }
}
#endif //ENABLE_CLOUD_SERVICES_ANALYTICS
