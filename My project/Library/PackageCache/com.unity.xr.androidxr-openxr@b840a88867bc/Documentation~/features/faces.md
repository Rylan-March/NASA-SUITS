---
uid: androidxr-openxr-faces
---
# Face tracking

This page supplements the AR Foundation [Face tracking](xref:arfoundation-face-tracking) manual. The following sections only contain information about APIs where Google's Android XR runtime exhibits platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

> [!IMPORTANT]
> You must ensure to configure the appropriate [Permissions](#permissions) to use face tracking features on Android XR.

## Optional feature support

Android XR implements the following optional features of AR Foundation's [XRFaceSubsystem](xref:UnityEngine.XR.ARSubsystems.XRFaceSubsystem):

| Feature | Descriptor Property | Supported |
| :------ | :------------------ | :----: |
| **Face pose** | [supportsFacePose](xref:UnityEngine.XR.ARSubsystems.XRFaceSubsystemDescriptor.supportsFacePose) |     |
| **Face mesh vertices and indices** | [supportsFaceMeshVerticesAndIndices](xref:UnityEngine.XR.ARSubsystems.XRFaceSubsystemDescriptor.supportsFaceMeshVerticesAndIndices) |     |
| **Face mesh UVs** | [supportsFaceMeshUVs](xref:UnityEngine.XR.ARSubsystems.XRFaceSubsystemDescriptor.supportsFaceMeshUVs) |     |
| **Face mesh normals** | [supportsFaceMeshNormals](xref:UnityEngine.XR.ARSubsystems.XRFaceSubsystemDescriptor.supportsFaceMeshNormals) |     |
| **Eye tracking** |  [supportsEyeTracking](xref:UnityEngine.XR.ARSubsystems.XRFaceSubsystemDescriptor.supportsEyeTracking) | Yes |

> [!NOTE]
> Refer to AR Foundation [Face tracking platform support](xref:arfoundation-face-tracking-platform-support) for more information on the optional features of the Face subsystem.

## Face data

This platform exposes face data for the active user (the person wearing the headset). Currently, only gaze data is surfaced.

To know which face is the inward avatar-eyes gaze object, cast an `XRFaceSubsystem` to an `AndroidOpenXRFaceSubsystem` and read its `inwardID` property.

## Permissions

AR Foundation's face tracking feature requires an Android system permission on the Android XR runtime. Your user must grant your app the `android.permission.EYE_TRACKING_COARSE` permission before it can track face data.

To avoid permission-related errors at runtime, set up your scene with the AR Face Manager component disabled, then enable it only after the required permission is granted.

The following example code demonstrates how to handle required permissions and enable the AR Face Manager component:

[!code-cs[request_face_permission](../../Tests/Runtime/CodeSamples/PermissionSamples.cs#request_face_permission)]

For a code sample that involves multiple permissions in a single request, refer to the [Permissions](xref:androidxr-openxr-permissions) page.
