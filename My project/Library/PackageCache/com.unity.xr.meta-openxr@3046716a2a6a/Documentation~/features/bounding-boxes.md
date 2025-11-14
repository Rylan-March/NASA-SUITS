---
uid: meta-openxr-bounding-boxes
---
# Bounding boxes

This page is a supplement to the AR Foundation [Bounding box detection](xref:arfoundation-bounding-box-detection) manual. The following sections only contain information about APIs where Meta's OpenXR runtime exhibits unique behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Space Setup

Before your app can access bounding boxes from Meta's OpenXR runtime, the user must first complete [Space Setup](xref:meta-openxr-device-setup#space-setup) on their device.

Meta's OpenXR runtime doesn't dynamically discover bounding boxes at runtime. Instead, this provider queries the device's Space Setup data and returns all bounding box components that are stored in its [Scene Model](https://developer.oculus.com/documentation/native/android/openxr-scene-overview#scene-model). Some entities in the Scene Model, such as Tables or Lamps, include bounding boxes, while others do not.

> [!Important]
> If Space Setup is not complete, the user's Scene Model will not contain any 3D bounding boxes. If your app requires bounding boxes, you can use [scene capture](xref:meta-openxr-session#scene-capture) to prompt the user to complete Space Setup.

## Permissions

Your app's user must grant an Android system permission before you can access bounding box data. Refer to [Permissions](xref:meta-openxr-scene-setup#permissions) for more information.

## Trackable ID

Unlike other AR platforms, the [trackableId](xref:UnityEngine.XR.ARFoundation.ARTrackable`2.trackableId) property of any [ARBoundingBox](xref:UnityEngine.XR.ARFoundation.ARBoundingBox) from Meta's OpenXR runtime persists across multiple sessions in the same space setup. This allows you to, for instance, save the `trackableId` of a user's bed to persist a virtual pet sleeping on the bed each time the user runs your app.

## Bounding box classifications

This package maps Meta's native [semantic labels](https://developer.oculus.com/documentation/native/android/mobile-scene-api-ref#getting-semantic-label-component) to AR Foundation's [BoundingBoxClassifications](xref:UnityEngine.XR.ARFoundation.ARBoundingBox.classifications). Meta supports multiple classifications per bounding box.

Refer to the table below to understand the mapping between AR Foundation's classifications and Meta's semantic labels:

| AR Foundation Label | Meta Label |
| :------------------ | :--------- |
| Couch               | COUCH   |
| Table               | TABLE   |
| Bed                 | BED     |
| Lamp                | LAMP    |
| Plant               | PLANT   |
| Screen              | SCREEN  |
| Storage             | STORAGE |
| Other               | OTHER   |

## Native pointer

[XRBoundingBox.nativePtr](xref:UnityEngine.XR.ARSubsystems.XRBoundingBox.nativePtr) values returned by this package contain a pointer to the following struct:

```c
typedef struct UnityXRNativeBoundingBox
{
    int version;
    void* boundingBoxPtr;
} UnityXRNativeBoundingBox;
```

Cast the `void* boundingBoxPtr` to an [XrSpace](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#spaces) handle in C++ using the following example code:

```cpp
// Marhshal the native bounding box data from the XRBoundingBox.nativePtr in C#
UnityXRNativeBoundingBox nativeBoundingBoxData;
XrSpace* boundingBoxXrSpaceHandle = static_cast<XrSpace*>(nativeBoundingBoxData.boundingBoxPtr);
```

To learn more about native pointers and their usage, refer to [Extending AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.0/manual/architecture/extensions.html).
