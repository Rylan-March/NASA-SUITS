---
uid: androidxr-openxr-raycasts
---
# Ray casts

This page supplements the AR Foundation [Ray casts](xref:arfoundation-raycasts) manual. The following sections only contain information about APIs where Google's Android XR runtime exhibits platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Optional feature support

Android XR supports ray casting from a ray and direction (world based ray casts). Android XR implements the following optional features of AR Foundation's [XRRaycastSubsystem](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystem):

| Feature                    | Descriptor Property | Supported |
| :------------------------- | :------------------ | :-------: |
| **Viewport based raycast** | [supportsViewportBasedRaycast](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystemDescriptor.supportsViewportBasedRaycast)|     |
| **World based raycast**    |  [supportsWorldBasedRaycast](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystemDescriptor.supportsWorldBasedRaycast)   | Yes |
| **Tracked raycasts**       | [supportsTrackedRaycasts](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystemDescriptor.supportsTrackedRaycasts) |     |

> [!NOTE]
> Refer to AR Foundation [Ray cast platform support](xref:arfoundation-raycasts-platform-support) for more information on the optional features of the Raycast subsystem.

### Supported trackables

Android XR supports ray casting against the following [trackable types](xref:UnityEngine.XR.ARSubsystems.TrackableType):

| TrackableType           | Supported |
| :---------------------- | :-------: |
| **BoundingBox**         |           |
| **Depth**               |    Yes    |
| **Face**                |           |
| **FeaturePoint**        |           |
| **Image**               |           |
| **Planes**              |    Yes    |
| **PlaneEstimated**      |    Yes    |
| **PlaneWithinBounds**   |    Yes    |
| **PlaneWithinInfinity** |    Yes    |
| **PlaneWithinPolygon**  |    Yes    |
