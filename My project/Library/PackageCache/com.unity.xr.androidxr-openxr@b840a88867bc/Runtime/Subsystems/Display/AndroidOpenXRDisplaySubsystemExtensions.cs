using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Android OpenXR extension methods for the <see cref="XRDisplaySubsystem"/>.
    /// > [!IMPORTANT]
    /// > These extension methods require that you enable the **Android OpenXR Display Utilities** feature in
    /// > **Project Settings** > **XR Plug-in Management** > **OpenXR**. If the display utilities feature is
    /// > not enabled, all extension methods will return <see langword="false"/>.
    /// </summary>
    /// <seealso cref="DisplayUtilitiesFeature"/>
    public static class AndroidOpenXRDisplaySubsystemExtensions
    {
        const string k_FeatureNotEnabledError =
            "To use Android OpenXR display uitilities, you must enable the Android OpenXR Display Utilities feature " +
            "in Project Settings > XR Plug-in Management > OpenXR.";

        /// <summary>
        /// Attempts to get the supported display refresh rates for the device.
        /// </summary>
        /// <param name="subsystem">The subsystem instance used by this extension method.</param>
        /// <param name="allocator">The allocator strategy to use for the <paramref name="refreshRates"/> out parameter.</param>
        /// <param name="refreshRates">A `NativeArray` containing all supported display refresh rates.
        /// You are responsible to `Dispose` this if you pass <see cref="Allocator.Persistent"/> as your allocator strategy.</param>
        /// <returns><see langword="true"/> if <paramref name="refreshRates"/> was successfully written. Otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > This extension method requires that you enable the **Android OpenXR Display Utilities** feature in
        /// > **Project Settings** > **XR Plug-in Management** > **OpenXR**. If the display utilities feature is
        /// > not enabled, this method will always return <see langword="false"/>.
        /// </remarks>
        public static XRResultStatus TryGetSupportedDisplayRefreshRates(
            this XRDisplaySubsystem subsystem, Allocator allocator, out NativeArray<float> refreshRates)
        {
            refreshRates = default;

            var resultStatus = new XRResultStatus();
            if (!OpenXRRuntime.IsExtensionEnabled(Constants.OpenXRExtensions.k_XR_FB_display_refresh_rate))
            {
                Debug.LogError(k_FeatureNotEnabledError);
                resultStatus = new XRResultStatus(XRResultStatus.StatusCode.ProviderUninitialized);
                return resultStatus;
            }

            var numDisplayRefreshRates = NativeApi.GetDisplayRefreshRateCount();
            if (numDisplayRefreshRates == 0)
            {
                Debug.LogError($"{nameof(TryGetSupportedDisplayRefreshRates)} failed due to an unknown error while obtaining refresh rate count.");
                resultStatus = new XRResultStatus(XRResultStatus.StatusCode.UnknownError);
                return resultStatus;
            }

            unsafe
            {
                refreshRates = new NativeArray<float>(numDisplayRefreshRates, allocator);
                if (!refreshRates.IsCreated)
                {
                    resultStatus = new XRResultStatus(XRResultStatus.StatusCode.UnknownError);
                    return resultStatus;
                }

                NativeApi.TryGetDisplayRefreshRates(
                    NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(refreshRates),
                    (uint)numDisplayRefreshRates,
                    resultStatus);
                return resultStatus;
            }
        }

        /// <summary>
        /// Attempts to request a change to the current display refresh rate. If the request is successfully created,
        /// Android OpenXR's runtime is expected to change the display refresh rate on a subsequent frame.
        /// </summary>
        /// <param name="subsystem">The subsystem instance used by this extension method.</param>
        /// <param name="refreshRate">The requested refresh rate. Must be an element of the array output from
        /// <see cref="TryGetSupportedDisplayRefreshRates">TryGetSupportedDisplayRefreshRates</see>.</param>
        /// <returns><see langword="true"/> if the request was successfully created. Otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > This extension method requires that you enable the **Android OpenXR Display Utilities** feature in
        /// > **Project Settings** > **XR Plug-in Management** > **OpenXR**. If the display utilities feature is
        /// > not enabled, this method will always return <see langword="false"/>.
        /// </remarks>
        /// <seealso cref="XRDisplaySubsystem.TryGetDisplayRefreshRate"/>
        public static XRResultStatus TrySetDisplayRefreshRate(this XRDisplaySubsystem subsystem, float refreshRate)
        {
            var resultStatus = new XRResultStatus();
            if (!OpenXRUtility.IsOpenXRFeatureEnabled<DisplayUtilitiesFeature>())
            {
                Debug.LogError(k_FeatureNotEnabledError);
                resultStatus = new XRResultStatus(XRResultStatus.StatusCode.ProviderUninitialized);
                return resultStatus;
            }

            NativeApi.TryRequestDisplayRefreshRate(refreshRate, resultStatus);
            return resultStatus;
        }

        static unsafe class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityAndroidOpenXR_Display_GetDisplayRefreshRateCount")]
            public static extern int GetDisplayRefreshRateCount();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityAndroidOpenXR_Display_TryGetDisplayRefreshRates")]
            public static extern void TryGetDisplayRefreshRates(void* refreshRates, uint capacity, XRResultStatus resultStatus);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityAndroidOpenXR_Display_TryRequestDisplayRefreshRate")]
            public static extern void TryRequestDisplayRefreshRate(float refreshRate, XRResultStatus resultStatus);
        }
    }
}
