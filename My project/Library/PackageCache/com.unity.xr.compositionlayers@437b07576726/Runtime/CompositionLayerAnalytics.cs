using System;
using System.Linq;
using System.Collections.Generic;
using Unity.XR.CompositionLayers.Services;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif

#if UNITY_ANALYTICS && ENABLE_CLOUD_SERVICES_ANALYTICS
using UnityEngine.Analytics;
#endif

namespace Unity.XR.CompositionLayers
{
    internal static class CompositionLayerAnalytics
    {
        private const int kMaxEventsPerHour = 1000;
        private const int kMaxNumberOfElements = 1000;
        private const string kVendorKey = "unity.compositionlayers";
        private const string kEventUsageMetrics = "compositionlayers_usage";

#if ENABLE_CLOUD_SERVICES_ANALYTICS && UNITY_ANALYTICS
        private static bool s_Initialized = false;
#endif
        [Serializable]
        public struct LayerTypeUsage
        {
            public string LayerTypeName;
            public int UsageCount;
        }
        [Serializable]
        private struct UsageMetricsEvent
#if UNITY_2023_2_OR_NEWER && ENABLE_CLOUD_SERVICES_ANALYTICS && UNITY_ANALYTICS
            : IAnalytic.IData
#endif //UNITY_2023_2_OR_NEWER && ENABLE_CLOUD_SERVICES_ANALYTICS && UNITY_ANALYTICS
        {
            public List<LayerTypeUsage> layerTypesUsage;
            public int compositionLayersCreated;
            public int colorScaleAndBiasComponentsCreated;
            public bool splashScreenCreated;
            public DateTime timestamp;
        }
#if UNITY_2023_2_OR_NEWER && ENABLE_CLOUD_SERVICES_ANALYTICS && UNITY_ANALYTICS
        [AnalyticInfo(eventName: kEventUsageMetrics, vendorKey: kVendorKey, maxEventsPerHour: kMaxEventsPerHour, maxNumberOfElements: kMaxNumberOfElements)]
        private class CompositionLayersUsageAnalytic : IAnalytic
        {
            private UsageMetricsEvent? data = null;

            public CompositionLayersUsageAnalytic(UsageMetricsEvent data)
            {
                this.data = data;
            }

            public bool TryGatherData(out IAnalytic.IData data, [NotNullWhen(false)] out Exception error)
            {
                error = null;
                data = this.data;
                return data != null;
            }
        }
#endif
        private static bool Initialize()
        {
#if ENABLE_TEST_SUPPORT || !ENABLE_CLOUD_SERVICES_ANALYTICS || !UNITY_ANALYTICS
            return false;
#elif UNITY_EDITOR && UNITY_2023_2_OR_NEWER
            return EditorAnalytics.enabled;
#else

#if UNITY_EDITOR
            if (!EditorAnalytics.enabled)
                return false;

            if (AnalyticsResult.Ok != EditorAnalytics.RegisterEventWithLimit(kEventUsageMetrics, kMaxEventsPerHour, kMaxNumberOfElements, kVendorKey))
                return false;
            s_Initialized = true;
#endif //UNITY_EDITOR
            return s_Initialized;
#endif //ENABLE_TEST_SUPPORT || !ENABLE_CLOUD_SERVICES_ANALYTICS || !UNITY_ANALYTICS
        }

        public static void SendUsageMetricsEvent()
        {
#if UNITY_EDITOR && UNITY_ANALYTICS && ENABLE_CLOUD_SERVICES_ANALYTICS
            if (!s_Initialized && !Initialize())
                return;

            var data = GatherUsageMetrics();
            SendEditorAnalytics(data);

#endif //UNITY_ANALYTICS && ENABLE_CLOUD_SERVICES_ANALYTICS
        }
        private static UsageMetricsEvent GatherUsageMetrics() {
            CompositionLayerAnalyticsHelper.ProcessCompositionLayersAnalyticsData();

            return new UsageMetricsEvent
            {
                layerTypesUsage = CompositionLayerAnalyticsHelper.LayerTypesUsage
                    .Select(kvp => new LayerTypeUsage { LayerTypeName = kvp.Key, UsageCount = kvp.Value })
                    .ToList(),
                compositionLayersCreated = CompositionLayerAnalyticsHelper.TotalLayerTypesUsage,
                colorScaleAndBiasComponentsCreated = CompositionLayerAnalyticsHelper.ColorScaleAndBiasExtensionsUsage,
                splashScreenCreated = CompositionLayerAnalyticsHelper.IsEnabledSplashScreen,
                timestamp = DateTime.Now
            };
        }

#if UNITY_EDITOR && ENABLE_CLOUD_SERVICES_ANALYTICS && UNITY_ANALYTICS
        private static void SendEditorAnalytics(UsageMetricsEvent data) {
#if UNITY_2023_2_OR_NEWER
            EditorAnalytics.SendAnalytic(new CompositionLayersUsageAnalytic(data));
#else
            EditorAnalytics.SendEventWithLimit(kEventUsageMetrics, data);
#endif //UNITY_2023_2_OR_NEWER
        }
#endif //UNITY_EDITOR && ENABLE_CLOUD_SERVICES_ANALYTICS && UNITY_ANALYTICS
    }

#if UNITY_EDITOR && ENABLE_CLOUD_SERVICES_ANALYTICS && UNITY_ANALYTICS
    public class CompositionLayerAnalyticsBuildProcessor : IPostprocessBuildWithReport

    {
        // This value determines the order of execution relative to other build processors.
        public int callbackOrder => 0;

        // This method is called after the build process is complete.
        public void OnPostprocessBuild(BuildReport report)
        {
            // Call your analytics sending method here
            CompositionLayerAnalytics.SendUsageMetricsEvent();
        }
    }
#endif
}
