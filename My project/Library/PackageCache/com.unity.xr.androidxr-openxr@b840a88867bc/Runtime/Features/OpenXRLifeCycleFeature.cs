using System;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Notifies the native plug-in of OpenXR life cycle events.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(Hidden = true,
        UiName = "",
        OpenxrExtensionStrings = "",
        Priority = int.MaxValue,
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = Constants.k_CompanyName,
        Desc = "Notifies the native plug-in of OpenXR life cycle callbacks.",
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    class OpenXRLifeCycleFeature : OpenXRFeature
    {
        /// <summary>
        /// The feature id string. This is used to give the feature an id for reference.
        /// </summary>
        public const string featureId = "AndroidXR-OpenXRLifeCycle";

#if UNITY_EDITOR
        /// <summary>
        /// Called whenever a ScriptableObject is first created.
        /// </summary>
        protected override void Awake()
        {
            EditorApplication.delayCall += RefreshEnabledState;
        }

        internal static void RefreshEnabledState()
        {
            var AndroidXROpenXRFeatureTypes = TypeCache.GetTypesDerivedFrom<AndroidXROpenXRFeature>(Constants.k_AssemblyName);
            RefreshEnabledStateForBuildTarget(BuildTargetGroup.Android, AndroidXROpenXRFeatureTypes);
        }

        static void RefreshEnabledStateForBuildTarget(BuildTargetGroup buildTarget, TypeCache.TypeCollection AndroidXROpenXRFeatureTypes)
        {
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(buildTarget);
            var lifeCycleFeature = settings.GetFeature<OpenXRLifeCycleFeature>();

            foreach (var featureType in AndroidXROpenXRFeatureTypes)
            {
                var feature = settings.GetFeature(featureType);
                if (feature != null && feature.enabled)
                {
                    lifeCycleFeature.enabled = true;
                    return;
                }
            }

            lifeCycleFeature.enabled = false;
        }
#endif // UNITY_EDITOR

        protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
            => NativeApi.UnityOpenXRAndroidXR_OpenXRLifeCycle_HookGetInstanceProcAddr(func);

        protected override bool OnInstanceCreate(ulong xrInstance)
            => NativeApi.UnityOpenXRAndroidXR_OpenXRLifeCycle_OnInstanceCreate(xrInstance, xrGetInstanceProcAddr);

        protected override void OnSystemChange(ulong xrSystem)
            => NativeApi.UnityOpenXRAndroidXR_OpenXRLifeCycle_OnSystemChange(xrSystem);

        protected override void OnSessionCreate(ulong xrSession)
            => NativeApi.UnityOpenXRAndroidXR_OpenXRLifeCycle_OnSessionCreate(xrSession);

        protected override void OnAppSpaceChange(ulong xrSpace)
            => NativeApi.UnityOpenXRAndroidXR_OpenXRLifeCycle_OnAppSpaceChange(xrSpace);

        protected override void OnSessionDestroy(ulong xrSession)
            => NativeApi.UnityOpenXRAndroidXR_OpenXRLifeCycle_OnSessionDestroy(xrSession);

        protected override void OnInstanceDestroy(ulong xrInstance)
            => NativeApi.UnityOpenXRAndroidXR_OpenXRLifeCycle_OnInstanceDestroy(xrInstance);

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern IntPtr UnityOpenXRAndroidXR_OpenXRLifeCycle_HookGetInstanceProcAddr(IntPtr func);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern bool UnityOpenXRAndroidXR_OpenXRLifeCycle_OnInstanceCreate(ulong xrInstance, IntPtr xrGetInstanceProcAddr);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRAndroidXR_OpenXRLifeCycle_OnSystemChange(ulong xrSystem);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRAndroidXR_OpenXRLifeCycle_OnSessionCreate(ulong xrSession);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRAndroidXR_OpenXRLifeCycle_OnAppSpaceChange(ulong xrSpace);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRAndroidXR_OpenXRLifeCycle_OnSessionDestroy(ulong xrSession);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern void UnityOpenXRAndroidXR_OpenXRLifeCycle_OnInstanceDestroy(ulong xrInstance);
        }
    }
}
