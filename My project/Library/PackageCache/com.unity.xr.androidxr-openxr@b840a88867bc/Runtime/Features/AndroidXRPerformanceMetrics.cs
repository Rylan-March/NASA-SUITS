using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.XR.CoreUtils.Collections;
using UnityEngine.XR.OpenXR.NativeTypes;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Enables access to Android XR performance metrics.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Android XR Performance Metrics",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = Constants.k_CompanyName,
        Desc = "Access to Android XR performance metrics.",
        DocumentationLink = Constants.DocsUrls.k_PerformanceMetricsUrl,
        OpenxrExtensionStrings = k_OpenXRRequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class AndroidXRPerformanceMetrics : AndroidXROpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.androidxr-performance-metrics";

        /// <summary>
        /// The set of OpenXR spec extension strings to enable, separated by spaces.
        /// </summary>
        const string k_OpenXRRequestedExtensions =
            Constants.OpenXRExtensions.k_XR_ANDROID_performance_metrics;

        List<string> m_SupportedMetricPaths;

        /// <summary>
        /// Get a list of all supported metric paths as reported by the Android XR device.
        /// </summary>
        /// <remarks>
        /// <see cref="supportedMetricPaths"/> can be null if <see cref="AndroidXRPerformanceMetrics"/> failed to initialize.
        /// </remarks>
        public ReadOnlyList<string> supportedMetricPaths
        {
            get
            {
                if (m_SupportedMetricPaths == null)
                {
                    Debug.LogWarning("Failed to initialize metrics feature. No metrics are available.");
                    return null;
                }

                return new ReadOnlyList<string>(m_SupportedMetricPaths);
            }
        }

        /// <summary>
        /// Creates and initializes native StatProvider.
        /// </summary>
        /// <param name="xrSession">Handle of the xrSession</param>
        protected override void OnSessionCreate(ulong xrSession)
        {
            InitializeStatProvider();
        }

        unsafe void InitializeStatProvider()
        {
            XrResult result = NativeApi.Create(out IntPtr androidStatKeysPtr, out IntPtr unityDisplayStatKeysPtr, out int keyCount);

            if (result != XrResult.Success)
            {
                Debug.LogError("Failed to initialize StatProvider. No metrics will be reported.");
                return;
            }

            m_SupportedMetricPaths = ProcessStatKeys(androidStatKeysPtr, keyCount);
            List<string> unityMetricPaths = ProcessStatKeys(unityDisplayStatKeysPtr, keyCount);

            NativeArray<uint> androidStatIds = new NativeArray<uint>(keyCount, Allocator.Temp);
            NativeArray<uint> unityDisplayStatIds = new NativeArray<uint>(keyCount, Allocator.Temp);

            for (int i = 0; i < keyCount; i++)
            {
                androidStatIds[i] = Convert.ToUInt32(RegisterStatsDescriptor(m_SupportedMetricPaths[i], StatFlags.StatOptionNone));

                if (unityMetricPaths[i].Length != 0)
                {
                    unityDisplayStatIds[i] = Convert.ToUInt32(RegisterStatsDescriptor(unityMetricPaths[i], StatFlags.StatOptionNone));
                }
                else
                {
                    // represents an invalid stat id as per IUnityXRStats.h
                    unityDisplayStatIds[i] = UInt32.MaxValue;
                }
            }

            NativeApi.Initialize((IntPtr)androidStatIds.GetUnsafePtr(), (IntPtr)unityDisplayStatIds.GetUnsafePtr());
        }

        /// <summary>
        /// Destroys the native StatProvider.
        /// </summary>
        /// <param name="xrSession">Handle of the xrSession</param>
        protected override void OnSessionDestroy(ulong xrSession)
        {
            NativeApi.Destroy();
        }

        /// <summary>
        /// Starts native StatProvider and attaches it to app update loop.
        /// </summary>
        /// <param name="xrSession">Handle of the xrSession</param>
        protected override void OnSessionBegin(ulong xrSession)
        {
            NativeApi.Start();
            Application.onBeforeRender += NativeApi.UpdateStats;
        }

        /// <summary>
        /// Stops native StatProvider and detaches it from app update loop.
        /// </summary>
        /// <param name="xrSession">Handle of the xrSession</param>
        protected override void OnSessionEnd(ulong xrSession)
        {
            NativeApi.Stop();
            Application.onBeforeRender -= NativeApi.UpdateStats;
        }

        unsafe struct StatKey
        {
            byte* m_Bytes;

            public override string ToString() => m_Bytes == null
                ? string.Empty
                : Marshal.PtrToStringAnsi(new IntPtr(m_Bytes));
        }

        unsafe List<string> ProcessStatKeys(IntPtr statKeysPtr, int keyCount)
        {
            List<string> paths = new List<string>(keyCount);

            var nativeKeyArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<StatKey>(
                (void*)statKeysPtr, keyCount, Allocator.None);

            foreach (var key in nativeKeyArray)
            {
                paths.Add(key.ToString());
            }

            return paths;
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Stats_Create")]
            public static extern XrResult Create(out IntPtr androidStatKeysPtr, out IntPtr unityDisplayStatKeysPtr, out int pathCount);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Stats_Destroy")]
            public static extern void Destroy();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Stats_Start")]
            public static extern XrResult Start();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Stats_Stop")]
            public static extern XrResult Stop();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Stats_Update")]
            public static extern void UpdateStats();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Stats_Initialize")]
            public static extern void Initialize(IntPtr androidStatIdsPtr, IntPtr unityStatIdsPtr);
        }
    }
}
