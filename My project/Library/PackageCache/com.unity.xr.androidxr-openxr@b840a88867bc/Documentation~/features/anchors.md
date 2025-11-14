---
uid: androidxr-openxr-anchors
---
# Anchors

This page supplements the AR Foundation [Anchors](xref:arfoundation-anchors) documentation. The following sections contain information about APIs where Google's Android XR runtime exhibits platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Optional feature support

Android XR implements the following optional features of AR Foundation's [XRAnchorSubsystem](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystem), including [persistent anchors](xref:arfoundation-anchors-persistent):

| Feature | Descriptor Property | Supported |
| :------ | :--------------- | :--------: |
| **Trackable attachments** | [supportsTrackableAttachments](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsTrackableAttachments) | Yes |
| **Synchronous add** | [supportsSynchronousAdd](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsSynchronousAdd) | Yes |
| **Save anchor** | [supportsSaveAnchor](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsSaveAnchor) | Yes |
| **Load anchor** | [supportsLoadAnchor](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsLoadAnchor) | Yes |
| **Erase anchor** | [supportsEraseAnchor](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsEraseAnchor) | Yes |
| **Get saved anchor IDs** | [supportsGetSavedAnchorIds](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsGetSavedAnchorIds) | Yes |
| **Async cancellation** | [supportsAsyncCancellation](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystemDescriptor.supportsAsyncCancellation) | |

> [!NOTE]
> Refer to AR Foundation [Anchors platform support](xref:arfoundation-anchors-platform-support) for more information on the optional features of the Anchor subsystem.
