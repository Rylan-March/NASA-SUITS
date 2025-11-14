---
uid: androidxr-openxr-foveated-rendering
---
# Eye tracking and foveated rendering

Android XR supports foveated rendering. This page supplements the [OpenXR foveated rendering](xref:openxr-foveated-rendering) documentation and outlines how to configure foveated rendering on Android XR.

## Enable eye tracking and foveated rendering

To enable eye tracking and foveated rendering, you must configure the following settings in the **OpenXR** section of the **XR Plug-in Management** settings:

1. Add the **Eye Gaze Interaction Profile** under **Enabled Interaction Profiles**.
2. Enable the **Foveated Rendering** feature from **All features** under **OpenXR Feature Groups**.

Refer to [Configure project settings](xref:androidxr-openxr-project-setup) to understand how to configure your interaction profiles and OpenXR features.

## Permissions

To use foveated rendering, you must request the `android.permission.EYE_TRACKING_FINE` permission. Refer to [Request eye tracking permission on Android](xref:openxr-foveated-rendering#request-eye-tracking-permission-on-android) for more information.

## Build profile

The Android XR build target from the **Build Profiles** window automatically enables the Android XR feature group and the Foveated Rendering feature in **XR Plugin Management Settings** window. Refer to Build Profile in the Unity manual for more information.

## Additional resources

* [OpenXR foveated rendering](xref:openxr-foveated-rendering)
* [Foveated rendering](xref:um-xr-foveated-rendering) (Unity Manual)
