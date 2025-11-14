#if ENABLE_CLOUD_SERVICES_ANALYTICS
using System;
using UnityEngine;
using UnityEngine.Analytics;

namespace UnityEditor.XR.Hands.Analytics
{
    /// <summary>
    /// Editor event used to send editor usage <see cref="XRHandsAnalytics"/> data.
    /// Only accepts <see cref="XRHandsPlaymodeUsageEvent.Payload"/> parameters.
    /// </summary>
#if UNITY_2023_2_OR_NEWER
    [AnalyticInfo(k_EventName, XRHandsAnalytics.k_VendorKey, k_EventVersion, k_MaxEventPerHour, k_MaxItems)]
    class XRHandsPlaymodeUsageEvent : XRHandsEditorAnalyticsEvent<XRHandsPlaymodeUsageEvent.Payload>
#else
    [InitializeOnLoad]
    class XRHandsPlaymodeUsageEvent : XRHandsEditorAnalyticsEvent<XRHandsPlaymodeUsageEvent.Payload>
#endif
    {
        const string k_EventName = "xr_hands_playmode_usage";
        const int k_EventVersion = 2;

#if !UNITY_2023_2_OR_NEWER
        static XRHandsPlaymodeUsageEvent()
        {
            EditorAnalytics.RegisterEventWithLimit(k_EventName, k_MaxEventPerHour, k_MaxItems, XRHandsAnalytics.k_VendorKey);
        }

        protected internal override bool Send(Payload parameter)
        {
            return EditorAnalytics.SendEventWithLimit(k_EventName, parameter, k_EventVersion) == AnalyticsResult.Ok;
        }
#endif // !UNITY_2023_2_OR_NEWER

        /// <summary>
        /// The event parameter.
        /// Do not rename any field, the field names are used the identify the table/event column of this event payload.
        /// </summary>
        [Serializable]
        internal struct Payload
#if UNITY_2023_2_OR_NEWER
            : IAnalytic.IData
#endif
        {
            // christopherg: Normally, our coding conventions say we should wrap these fields in properties. By doing
            // that we change their names (m_XRHandsRuntimeState) and make it more difficult for a human to parse the
            // resulting JSON. We are breaking our conventions here for the sake of making the data easier to read.

            [SerializeField]
            internal bool subsystemRuntimeUsed;
            [SerializeField]
            internal bool customGestureUsed;
            [SerializeField]
            internal bool customGestureDebuggerUsed;
        }
    }
}
#endif //ENABLE_CLOUD_SERVICES_ANALYTICS
