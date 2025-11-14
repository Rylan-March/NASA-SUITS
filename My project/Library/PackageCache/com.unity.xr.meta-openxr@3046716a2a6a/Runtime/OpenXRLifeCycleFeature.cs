using System;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
using CallbackFunction = UnityEditor.EditorApplication.CallbackFunction;
using TypeCollection = UnityEditor.TypeCache.TypeCollection;
#endif

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Notifies the native plug-in of OpenXR life cycle events.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(Hidden = true,
        UiName = "",
        OpenxrExtensionStrings = "",
        Priority = int.MaxValue,
        BuildTargetGroups = new[] { BuildTargetGroup.Android, BuildTargetGroup.Standalone },
        Company = Constants.k_CompanyName,
        Desc = "Notifies the native plug-in of OpenXR life cycle callbacks.",
        Category = FeatureCategory.Feature,
        FeatureId = k_FeatureId,
        Version = "0.1.0")]
#endif
    class OpenXRLifeCycleFeature : OpenXRFeature
    {
        const string k_FeatureId = "MetaOpenXR-OpenXRLifeCycle";
        const int k_RetryCount = 3;

#if UNITY_EDITOR
        /// <summary>
        /// Called whenever a ScriptableObject is first created.
        /// </summary>
        protected override void Awake()
        {
            EditorApplication.delayCall += new CallbackFunction(GetRefreshMethodWithRemainingRetries(k_RetryCount));
        }

        internal static bool TryRefreshEnabledState()
        {
            var metaOpenXRFeatureTypes = TypeCache.GetTypesDerivedFrom<MetaOpenXRFeature>(Constants.k_AssemblyName);
            return TryRefreshEnabledStateForBuildTarget(BuildTargetGroup.Android, metaOpenXRFeatureTypes)
                && TryRefreshEnabledStateForBuildTarget(BuildTargetGroup.Standalone, metaOpenXRFeatureTypes);
        }

        static Action GetRefreshMethodWithRemainingRetries(int remainingRetries)
        {
            return () =>
            {
                if (!TryRefreshEnabledState() && --remainingRetries > 0)
                    EditorApplication.delayCall += new CallbackFunction(GetRefreshMethodWithRemainingRetries(remainingRetries));
            };
        }

        static bool TryRefreshEnabledStateForBuildTarget(BuildTargetGroup buildTarget, TypeCollection metaOpenXRFeatureTypes)
        {
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTarget);
            if (settings == null)
                return false;

            var lifeCycleFeature = settings.GetFeature<OpenXRLifeCycleFeature>();
            if (lifeCycleFeature == null)
                return false;

            foreach (var featureType in metaOpenXRFeatureTypes)
            {
                var feature = settings.GetFeature(featureType);
                if (feature != null && feature.enabled)
                {
                    lifeCycleFeature.enabled = true;
                    return true;
                }
            }

            lifeCycleFeature.enabled = false;
            return true;
        }
#endif // UNITY_EDITOR

        protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
            => NativeApi.UnityOpenXRMeta_OpenXRLifeCycle_HookGetInstanceProcAddr(func);

        protected override bool OnInstanceCreate(ulong xrInstance)
            => NativeApi.UnityOpenXRMeta_OpenXRLifeCycle_OnInstanceCreate(xrInstance);

        protected override void OnSystemChange(ulong xrSystem)
            => NativeApi.UnityOpenXRMeta_OpenXRLifeCycle_OnSystemChange(xrSystem);

        protected override void OnSessionCreate(ulong xrSession)
            => NativeApi.UnityOpenXRMeta_OpenXRLifeCycle_OnSessionCreate(xrSession);

        protected override void OnAppSpaceChange(ulong xrSpace)
            => NativeApi.UnityOpenXRMeta_OpenXRLifeCycle_OnAppSpaceChange(xrSpace);

        protected override void OnSessionDestroy(ulong xrSession)
            => NativeApi.UnityOpenXRMeta_OpenXRLifeCycle_OnSessionDestroy(xrSession);

        protected override void OnInstanceDestroy(ulong xrInstance)
        {
            SystemCapabilityUtils.ClearCachedCapabilityInfo();
            NativeApi.UnityOpenXRMeta_OpenXRLifeCycle_OnInstanceDestroy(xrInstance);
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern IntPtr UnityOpenXRMeta_OpenXRLifeCycle_HookGetInstanceProcAddr(IntPtr func);

            [DllImport(Constants.k_ARFoundationLibrary)]
            [return: MarshalAs(UnmanagedType.U1)]
            public static extern bool UnityOpenXRMeta_OpenXRLifeCycle_OnInstanceCreate(ulong xrInstance);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRMeta_OpenXRLifeCycle_OnSystemChange(ulong xrSystem);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRMeta_OpenXRLifeCycle_OnSessionCreate(ulong xrSession);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRMeta_OpenXRLifeCycle_OnAppSpaceChange(ulong xrSpace);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRMeta_OpenXRLifeCycle_OnSessionDestroy(ulong xrSession);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRMeta_OpenXRLifeCycle_OnInstanceDestroy(ulong xrInstance);
        }
    }
}
