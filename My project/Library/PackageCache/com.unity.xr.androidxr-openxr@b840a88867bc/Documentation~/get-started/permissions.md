---
uid: androidxr-openxr-permissions
---
# Permissions

Some XR features require system permissions on the Android XR runtime. Refer to the following table for a list of OpenXR features and the permissions they require. The documentation for each features provides a code example you can use to request the relevant permission.

> [!NOTE]
> If a feature requires the coarse version of a permission, you can also use the fine version of that permission. For example, if the feature requires `android.permission.SCENE_UNDERSTANDING_COARSE`, you can use `android.permission.SCENE_UNDERSTANDING_FINE`.

| Feature                  | Permission                                        |
| :----------------------- | :------------------------------------------------ |
| [Plane detection](xref:androidxr-openxr-plane-detection) | android.permission.SCENE_UNDERSTANDING_COARSE |
| [Hand tracking](xref:androidxr-openxr-hand-tracking) | android.permission.HAND_TRACKING |
| [Face tracking](xref:androidxr-openxr-faces) | android.permission.EYE_TRACKING_COARSE |
| [Gaze interaction profile](xref:UnityEngine.XR.OpenXR.Features.Interactions.EyeGazeInteraction) | android.permission.EYE_TRACKING_FINE |
| [Light estimation](xref:androidxr-openxr-camera#light-estimation) | android.permission.SCENE_UNDERSTANDING_COARSE |

## Request multiple permissions simultaneously

If your app needs to request multiple permissions at once, you should use [Permission.RequestUserPermissions](xref:UnityEngine.Android.Permission.RequestUserPermissions(System.String[],UnityEngine.Android.PermissionCallbacks)).

Use the following example code to request multiple permissions at once:

[!code-cs[request_multiple_permissions](../../Tests/Runtime/CodeSamples/PermissionSamples.cs#request_multiple_permissions)]

Refer to [Request runtime permissions](xref:um-android-requesting-permissions) for more information on Android permissions in Unity.

## Additional resources

* [Android permissions](xref:um-android-permissions-in-unity)
* [Plane detection](xref:androidxr-openxr-plane-detection)
* [Hand tracking](xref:androidxr-openxr-hand-tracking)
* [Face tracking](xref:androidxr-openxr-faces)
* [Gaze interaction profile](xref:UnityEngine.XR.OpenXR.Features.Interactions.EyeGazeInteraction)
