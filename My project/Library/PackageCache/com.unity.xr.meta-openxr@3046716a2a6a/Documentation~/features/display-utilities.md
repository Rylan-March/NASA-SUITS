---
uid: meta-openxr-display-utilities
---
# Display Utilities

The Meta Quest Display Utilities feature enables you to:
1. Get the supported display refresh rates for the device.
2. Request a selected display refresh rate.

<a id="enable-display-utilities"/>

## Enable Display Utilities

To enable Meta Quest Display Utilities in your app:

1. Go to **Project Settings** > **XR Plug-in Management** > **OpenXR**.
2. Under **OpenXR Feature Groups**, select the **Meta Quest** feature group.
3. If disabled, enable the **Meta Quest Display Utilities** OpenXR feature.

## Code sample

Once enabled, Meta Quest Display Utilities adds additional capabilities to Unity's [XRDisplaySubsystem](xref:UnityEngine.XR.XRDisplaySubsystem) using C# extension methods: [TryGetSupportedDisplayRefreshRates](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRDisplaySubsystemExtensions.TryGetSupportedDisplayRefreshRates*) and [TryRequestDisplayRefreshRate](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRDisplaySubsystemExtensions.TryRequestDisplayRefreshRate*).

> [!IMPORTANT]
> These extension methods will always return false if you did not [Enable Display Utilities](#enable-display-utilities) in **XR Plug-in Management**.

The example code below demonstrates how to use these extension methods:

[!code-cs[request_display_refreshRate](../../Tests/Runtime/CodeSamples/MetaQuestDisplayUtilitiesSample.cs#request_display_refreshRate)]
