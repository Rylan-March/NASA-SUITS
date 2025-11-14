using System.Collections.Generic;

namespace UnityEngine.XR.OpenXR.Features.Android.Tests
{
    class StatSample : MonoBehaviour
    {
        #region query_xr_stats
        void Start()
        {
            IntegratedSubsystem m_Display = GetFirstDisplaySubsystem();
            AndroidXRPerformanceMetrics androidXRPerformanceMetrics = OpenXRSettings.Instance.GetFeature<AndroidXRPerformanceMetrics>();

            if (m_Display != null && androidXRPerformanceMetrics != null && androidXRPerformanceMetrics.supportedMetricPaths != null)
            {
                foreach (var metric in androidXRPerformanceMetrics.supportedMetricPaths)
                {
                    Provider.XRStats.TryGetStat(m_Display, metric, out float stat);

                    // process stat value
                }
            }
        }

        static IntegratedSubsystem GetFirstDisplaySubsystem()
        {
            List<XRDisplaySubsystem> displays = new List<XRDisplaySubsystem>();
            SubsystemManager.GetSubsystems(displays);
            if (displays.Count == 0)
            {
                Debug.Log("No display subsystem found.");
                return null;
            }
            return displays[0];
        }
        #endregion
    }
}
