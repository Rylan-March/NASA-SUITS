---
uid: androidxr-openxr-hand-tracking
---
# Hand tracking

This page supplements the [XR Hands](xref:xrhands-manual) manual and only contains information about APIs where Google's Android XR runtime exhibits platform-specific behavior.

> [!IMPORTANT]
> You must ensure to configure the appropriate [Permissions](#permissions) to use hand tracking features on Android XR.

## Permissions

The [XRHandSubsystem](xref:UnityEngine.XR.Hands.XRHandSubsystem) in Unity's XR Hands package requires an Android system permission on the Android XR runtime. Your user must grant your app the `android.permission.HAND_TRACKING` permission before it can receive hand tracking data.

> [!NOTE]
> The hand tracking subsystem starts immediately after OpenXR is initialized. Google's Android XR runtime logs an error every frame that the hand tracking subsystem is running and the required permission has not been granted.

Use this example code to request the required permission if it hasn't already been granted:

[!code-cs[request_hand_tracking_permission](../../Tests/Runtime/CodeSamples/PermissionSamples.cs#request_hand_tracking_permission)]

For a code sample that involves multiple permissions in a single request, refer to the [Permissions](xref:androidxr-openxr-permissions) page.
