---
uid: androidxr-openxr-plane-detection
---
# Plane detection

This page supplements the AR Foundation [Plane detection](xref:arfoundation-plane-detection) manual. The following sections only contain information about APIs where Google's Android XR runtime exhibits platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

> [!IMPORTANT]
> You must ensure to configure the appropriate [Permissions](#permissions) to use plane detection features on Android XR.

## Optional feature support

Android XR implements the following optional features of AR Foundation's [XRPlaneSubsystem](xref:UnityEngine.XR.ARSubsystems.XRPlaneSubsystem):

| Feature | Descriptor Property | Supported |
| :------ | :------------------ | :----: |
| **Horizontal plane detection** | [supportsHorizontalPlaneDetection](xref:UnityEngine.XR.ARSubsystems.XRPlaneSubsystemDescriptor.supportsHorizontalPlaneDetection) | Yes |
| **Vertical plane detection** | [supportsVerticalPlaneDetection](xref:UnityEngine.XR.ARSubsystems.XRPlaneSubsystemDescriptor.supportsVerticalPlaneDetection) | Yes |
| **Arbitrary plane detection** | [supportsArbitraryPlaneDetection](xref:UnityEngine.XR.ARSubsystems.XRPlaneSubsystemDescriptor.supportsArbitraryPlaneDetection) | Yes |
| **Boundary vertices** | [supportsBoundaryVertices](xref:UnityEngine.XR.ARSubsystems.XRPlaneSubsystemDescriptor.supportsBoundaryVertices) | Yes |
| **Classification** |  [supportsClassification](xref:UnityEngine.XR.ARSubsystems.XRPlaneSubsystemDescriptor.supportsClassification) | Yes |

> [!NOTE]
> Refer to AR Foundation [Plane detection platform support](xref:arfoundation-plane-platform-support) for more information on the optional features of the Plane subsystem.

## Permissions

AR Foundation's plane detection feature requires an Android system permission on the Android XR runtime. Your user must grant your app the `android.permission.SCENE_UNDERSTANDING_COARSE` or android.permission.SCENE_UNDERSTANDING_FINE` permission before it can track plane data.

To avoid permission-related errors at runtime, set up your scene with the AR Plane Manager component disabled, then enable it only after the required permission is granted.

The following example code demonstrates how to handle required permissions and enable the AR Plane Manager component:

[!code-cs[request_plane_permission](../../Tests/Runtime/CodeSamples/PermissionSamples.cs#request_plane_permission)]

For a code sample that involves multiple permissions in a single request, refer to the [Permissions](xref:androidxr-openxr-permissions) page.

## Plane classifications

This package maps Android XR's native plane label component to AR Foundation's [PlaneClassifications](xref:UnityEngine.XR.ARFoundation.ARPlane.classifications).

Refer to the following table to understand the mapping between AR Foundation's classifications and Android XR's plane labels:

| AR Foundation label   | Android XR label    |
| :-------------------- | :------------------ |
| None                  | UNKNOWN             |
| Table                 | TABLE               |
| Couch                 |                     |
| Floor                 | FLOOR               |
| Ceiling               | CEILING             |
| WallFace              | WALL                |
| WallArt               |                     |
| DoorFrame             |                     |
| WindowFrame           |                     |
| InvisibleWallFace     |                     |
| Other                 |                     |
