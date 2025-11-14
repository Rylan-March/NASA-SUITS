---
uid: androidxr-openxr-performance-metrics
---
# Performance metrics

The Android XR runtime defines different metrics that you can use to assess the performance of your application.

## Android XR metrics

The following table describes the performance metrics paths that may be available on Android XR devices:

| Metric path | Description    |
| :---------- | :------------- |
| `/perfmetrics_android/app/cpu_frametime (milliseconds, float)` | Wallclock time client spent to process a frame. |
| `/perfmetrics_android/app/gpu_frametime (milliseconds, float)` | Wallclock time client spent waiting for GPU work to complete per frame. Notes: A high wait time can mean that the GPU was busy with other tasks, not necessarily that this client is doing too much GPU work. The GPU wait time can be zero if rendering was already complete when checked by the compositor. |
| `/perfmetrics_android/app/cpu_utilization (percents, float)` | Total app CPU utilization rate averaged over time. Can be higher than 100% on multi-core processors. |
| `/perfmetrics_android/app/motion_to_photon_latency (milliseconds, float)` | Time spent from user-initiated motion event to corresponding physical image update on the display. |
| `/perfmetrics_android/compositor/cpu_frametime (milliseconds, float)` | Wallclock time compositor spent to process a frame. |
|`/perfmetrics_android/compositor/gpu_frametime (milliseconds, float)` | Wallclock time compositor spent waiting for GPU work to complete per frame. |
| `/perfmetrics_android/compositor/dropped_frame_count (integer)` | Total number of dropped frames from all apps. |
| `/perfmetrics_android/compositor/frames_per_second (float)` | Number of compositor frames  drawn on device per second. |
| `/perfmetrics_android/device/cpu_utilization_average (percents, float)` | Device CPU utilization rate averaged across all cores and averaged over time. |
| `/perfmetrics_android/device/cpu_utilization_worst (percents, float)` | Device CPU utilization rate of worst performing core  averaged over time. |
| `/perfmetrics_android/device/cpu0_utilization through /perfmetrics_android/device/cpuX_utilization (percents, float, X is the number of CPU cores minus one)` | Device CPU utilization rate per CPU core averaged over one time. |
| `/perfmetrics_android/device/cpu_frequency (MHz, float)` | Device CPU frequency averaged across all cores and averaged over time. |
| `/perfmetrics_android/device/gpu_utilization (percents, float)` | Device GPU utilization rate averaged over time. |

## Query supported metrics

You can use the [supportedMetricPaths](xref:UnityEngine.XR.OpenXR.Features.Android.AndroidXRPerformanceMetrics.supportedMetricPaths) property to get a list of available metric paths at runtime.
You can use these metric paths with [XRStats.TryGetStat](xref:UnityEngine.XR.Provider.XRStats.TryGetStat(UnityEngine.IntegratedSubsystem,System.String,System.Single&)) to get the metric values.

> [!NOTE]
> If there are any issues initializing this feature, 
[supportedMetricPaths](xref:UnityEngine.XR.OpenXR.Features.Android.AndroidXRPerformanceMetrics.supportedMetricPaths) will be null and no metrics will be available. 

### Example code

The following code example demonstrates how to query for all supported metrics on the Android XR device:

[!code-cs[query_xr_stats](../../Tests/Runtime/CodeSamples/StatSample.cs#query_xr_stats)]
