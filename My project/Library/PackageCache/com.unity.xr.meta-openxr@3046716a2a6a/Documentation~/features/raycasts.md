---
uid: meta-openxr-raycasts
---
# Ray casts

This page is a supplement to the AR Foundation [Ray casts](xref:arfoundation-raycasts) manual. The following sections only contain information about APIs where Meta Quest exhibits unique platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Ray cast architecture

This package defines an empty implementation of AR Foundation's [XRRaycastSubsystem.Provider](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystem.Provider). This implementation doesn't provide any OpenXR functionality. The empty ray cast provider enables a fallback Unity-world-space ray cast implementation in AR Foundation's [ARRaycastManager](xref:UnityEngine.XR.ARFoundation.ARRaycastManager).

>[!IMPORTANT]
> If your app uses AR ray casts, you should use the [ARRaycastManager](xref:UnityEngine.XR.ARFoundation.ARRaycastManager) APIs. Don't access `MetaOpenXRRaycastSubsystem` directly.

## Optional feature support

This package implements the following optional features of AR Foundation's [XRRaycastSubsystem](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystem):

| Feature                    | Descriptor Property | Supported |
| :------------------------- | :------------------ | :-------: |
| **Viewport based raycast** | [supportsViewportBasedRaycast](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystemDescriptor.supportsViewportBasedRaycast)|  |
| **World based raycast**    | [supportsWorldBasedRaycast](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystemDescriptor.supportsWorldBasedRaycast)   | Yes |
| **Tracked raycasts**       | [supportsTrackedRaycasts](xref:UnityEngine.XR.ARSubsystems.XRRaycastSubsystemDescriptor.supportsTrackedRaycasts) |  |

### Supported trackables

This package supports ray casting against the following [trackable types](xref:UnityEngine.XR.ARSubsystems.TrackableType):

| TrackableType           | Supported |
| :---------------------- | :-------: |
| **BoundingBox**         |    Yes    |
| **Depth**               |           |
| **Face**                |           |
| **FeaturePoint**        |           |
| **Image**               |           |
| **Planes**              |           |
| **PlaneEstimated**      |           |
| **PlaneWithinBounds**   |    Yes    |
| **PlaneWithinInfinity** |           |
| **PlaneWithinPolygon**  |           |

> [!NOTE]
> Refer to AR Foundation [Ray cast platform support](xref:arfoundation-raycasts-platform-support) for more information on the optional features of the Raycast subsystem.
