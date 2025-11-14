---
uid: androidxr-openxr-display-utilities
---
# Display Utilities

The Android XR Display Utilities feature enables you to:

1. Get the supported display refresh rates for the device.
2. Request a selected display refresh rate.

<a id="enable-display-utilities"/>

## Enable Display Utilities

To enable Android XR Display Utilities in your app:

1. Go to **Project Settings** > **XR Plug-in Management** > **OpenXR**.
2. Under **OpenXR Feature Groups**, select the **Android XR** feature group.
3. Enable the **Android XR Display Utilities** OpenXR feature.

As a standalone feature of this package, **Android XR: Display Utilities** solely depends on Android XR Support and does not require that you enable any other feature in the **Android XR** feature group.

## Code sample

Once enabled, Android XR Display Utilities adds additional capabilities to Unity's [XRDisplaySubsystem](xref:UnityEngine.XR.XRDisplaySubsystem) using C# extension methods: [TryGetSupportedDisplayRefreshRates](xref:UnityEngine.XR.OpenXR.Features.Android.AndroidOpenXRDisplaySubsystemExtensions.TryGetSupportedDisplayRefreshRates*) and [TrySetDisplayRefreshRate](xref:UnityEngine.XR.OpenXR.Features.Android.AndroidOpenXRDisplaySubsystemExtensions.TrySetDisplayRefreshRate*) 

> [!IMPORTANT]
> These extension methods always return false if you did not [Enable Display Utilities](#enable-display-utilities) in **XR Plug-in Management**.

The following code sample demonstrates how to use these extension methods:

[!code-cs[request_display_refreshRate](../../Tests/Runtime/CodeSamples/AndroidXRDisplayUtilitiesSample.cs#request_display_refreshRate)]
