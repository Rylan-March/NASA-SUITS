---
uid: meta-openxr-planes
---
# Planes

This page is a supplement to the AR Foundation [Plane detection](xref:arfoundation-plane-detection) manual. The following sections only contain information about APIs where Meta's OpenXR runtime exhibits unique behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Space Setup

Before your app can access planes from Meta's OpenXR runtime, the user must first complete [Space Setup](xref:meta-openxr-device-setup#space-setup) on their device.

Unlike other AR platforms, Meta's OpenXR runtime doesn't dynamically discover planes at runtime. Instead, this provider queries the device's Space Setup data and returns all plane components that are stored in its [Scene Model](https://developer.oculus.com/documentation/native/android/openxr-scene-overview#scene-model). Some entities in the Scene Model, such as Tables or Couches, include planes, while others do not.

> [!IMPORTANT]
> If Space Setup is not complete, AR Foundation cannot access plane data from the device. If your app requires planes, you can use [scene capture](xref:meta-openxr-session#scene-capture) to prompt the user to complete Space Setup.

## Permissions

Your app's user must grant an Android system permission before you can access plane data. Refer to [Permissions](xref:meta-openxr-scene-setup#permissions) for more information.

## Trackable ID

Unlike other AR platforms, the [trackableId](xref:UnityEngine.XR.ARFoundation.ARTrackable`2.trackableId) property of any [ARPlane](xref:UnityEngine.XR.ARFoundation.ARPlane) from Meta's OpenXR runtime persists across multiple sessions in the same space setup. This allows you to, for instance, save the `trackableId` of a user's table to persist a virtual centerpiece on the table each time the user runs your app.

## Plane alignment

Meta's OpenXR runtime doesn't classify planes as horizontal or vertical. Therefore, this package uses a [planeAlignmentThreshold](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRPlaneSubsystem.planeAlignmentThreshold) property to determine the threshold for [PlaneAlignment](xref:UnityEngine.XR.ARSubsystems.PlaneAlignment) categorization. Plane alignment is calculated by taking each component of the plane's normal vector and finding their difference from 0. If the difference is less than the `planeAlignmentThreshold`, then the `MetaOpenXRPlaneSubsystem` will categorize the plane depending on which of the normal vector's components are within the threshold. The `x` and `z` components are both checked for horizontal planes while the `y` component is checked for vertical planes.

## Plane classifications

This package maps Meta's native [semantic label component](https://developer.oculus.com/documentation/native/android/mobile-scene-api-ref#getting-semantic-label-component) to AR Foundation's [PlaneClassifications](xref:UnityEngine.XR.ARFoundation.ARPlane.classifications). Meta supports multiple classifications per plane.

Refer to the table below to understand the mapping between AR Foundation's classifications and Meta's semantic labels:

| AR Foundation Label   | Meta Label          |
| :-------------------- | :------------------ |
| Table                 | TABLE               |
| Couch                 | COUCH               |
| Floor                 | FLOOR               |
| Ceiling               | CEILING             |
| WallFace              | WALL_FACE           |
| WallArt               | WALL_ART            |
| DoorFrame             | DOOR_FRAME          |
| WindowFrame           | WINDOW_FRAME        |
| InvisibleWallFace     | INVISIBLE_WALL_FACE |
| Other                 | OTHER               |

## Native pointer

[BoundedPlane.nativePtr](xref:UnityEngine.XR.ARSubsystems.BoundedPlane.nativePtr) values returned by this package contain a pointer to the following struct:

```c
typedef struct UnityXRNativePlane
{
    int version;
    void* planePtr;
} UnityXRNativePlane;
```

Cast the `void* planePtr` to an [XrSpace](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#spaces) handle in C++ using the following example code:

```cpp
// Marshal the native plane data from the BoundedPlane.nativePtr in C#
UnityXRNativePlane nativePlaneData;
XrSpace* planeXrSpaceHandle = static_cast<XrSpace*>(nativePlaneData.planePtr);
```

To learn more about native pointers and their usage, refer to [Extending AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.0/manual/architecture/extensions.html).
