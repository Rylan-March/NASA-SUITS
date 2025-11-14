using Unity.Collections;
using UnityEngine.TestTools;
using UnityEngine.XR.Management;

namespace UnityEngine.XR.OpenXR.Features.Android.Tests
{
    /// <summary>
    /// A class containing samples for the Android XR Display Utilities
    /// </summary>
    public class AndroidXRDisplayUtilitiesSample
    {
        void RequestDisplayRefreshRate()
        {
            #region request_display_refreshRate
            // Omitted null checks for brevity. You should check each line for null.
            var displaySubsystem = XRGeneralSettings.Instance
                .Manager
                .activeLoader
                .GetLoadedSubsystem<XRDisplaySubsystem>();

            // Get the supported refresh rates.
            // If you will save the refresh rate values for longer than this frame, pass
            // Allocator.Persistent and remember to Dispose the array when you are done with it.
            var getSupportedRefreshRatesStatus = displaySubsystem.TryGetSupportedDisplayRefreshRates(
                    Allocator.Temp,
                    out var refreshRates);
            if (getSupportedRefreshRatesStatus.IsError())
            {
                Debug.Log($"Failed to get available refresh rates");
                return;
            }

            // Request a refresh rate.
            // Returned status will be an error if you request a value that is not in the refreshRates array.
            var setRefreshRateStatus = displaySubsystem.TrySetDisplayRefreshRate(refreshRates[0]);
            if (setRefreshRateStatus.IsError())
            {
                Debug.Log($"Failed to set refresh rate to {refreshRates[0]}");
                return;
            }

            #endregion
        }
    }
}
