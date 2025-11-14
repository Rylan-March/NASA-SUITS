---
uid: meta-openxr-anchors-platform-support
---
# Anchors platform support

This package implements the following optional features of AR Foundation's [XRAnchorSubsystem](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystem):

| Feature | Descriptor Property | Supported |
| :------ | :------------------ | :-------: |
| **Trackable Attachment** | [supportsTrackableAttachments](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsTrackableAttachments) | |
| **Synchronous Add Anchor** | [supportsSynchronousAdd](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsSynchronousAdd) | |
| **Save Anchor** | [supportsSaveAnchor](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsSaveAnchor) | Yes |
| **Load Anchor** | [supportsLoadAnchor](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsLoadAnchor) | Yes |
| **Erase Anchor** | [supportsEraseAnchor](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsEraseAnchor) | Yes |
| **Get Saved Anchor Ids** | [supportsGetSavedAnchorIds](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsGetSavedAnchorIds) | |
| **Async Cancellation** | [supportsAsyncCancellation](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsAsyncCancellation) | |

## Shared anchors

This package also provides functionality to share anchors between devices. The implementation of shared anchors is specific to this platform. To learn more about this feature, refer to [Shared anchors](xref:meta-openxr-shared-anchors).
