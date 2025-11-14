#if ENABLE_CLOUD_SERVICES_ANALYTICS
using System;
using UnityEngine;

namespace UnityEditor.XR.Hands.Analytics
{
    /// <summary>
    /// This class is used to track XR Hand Capture analytics data.
    /// It is not intended to be used for anything else. Users do not need to use this class.
    /// </summary>
    struct XRHandCaptureAnalyticsData
    {
        /// <summary>
        /// This is used to track the action of importing recordings into the Editor
        /// </summary>
        internal bool recordingsImported { get; set; }

        /// <summary>
        /// This is used to track the action of saving a new XRHandShape asset generated from the XR Hand Capture process.
        /// </summary>
        internal bool newHandShapeSaved { get; set; }

        /// <summary>
        /// Send analytics data for XR Hand Capture actions.
        /// </summary>
        internal void Send()
        {
            if (recordingsImported)
            {
                var payload = new XRHandsEditmodeUsageEvent.Payload
                {
                    recordingsImported = recordingsImported
                };
                XRHandsAnalytics.editmodeUsageEvent.Send(payload);
            }

            if (newHandShapeSaved)
            {
                var payload = new XRHandsEditmodeUsageEvent.Payload
                {
                    newHandShapeSaved = newHandShapeSaved
                };
                XRHandsAnalytics.editmodeUsageEvent.Send(payload);
            }
        }
    }
}
#endif
