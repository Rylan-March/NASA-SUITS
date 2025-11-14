---
uid: meta-openxr-occlusion-platform-support
---
# Occlusion platform support

This package implements the following optional features of AR Foundation's [XROcclusionSubsystem](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystem):

| Feature | Descriptor Property | Supported |
| :------ | :------------------ | :-------: |
| **Environment Depth Confidence Image** | [environmentDepthConfidenceImageSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.environmentDepthConfidenceImageSupported) | |
| **Environment Depth Image** | [environmentDepthImageSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.environmentDepthImageSupported) | Yes |
| **Environment Depth Temporal Smoothing** | [environmentDepthTemporalSmoothingSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.environmentDepthTemporalSmoothingSupported) | |
| **Human Segmentation Depth Image** | [humanSegmentationDepthImageSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.humanSegmentationDepthImageSupported) | |
| **Human Segmentation Stencil Image** | [humanSegmentationStencilImageSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.humanSegmentationStencilImageSupported)

## Graphics API support

To use occlusion in your app, you must build your project with a supported graphics API. The Meta Quest Occlusion feature supports the following graphics APIs:

| Build Platform | Vulkan | Direct3D12 | Direct3D11 | OpenGLES3 |
| :------------- | :----: | :--------: | :--------: | :-------: |
| Android or Meta Quest | Yes | | | |
| Windows (Editor or standalone) | Yes | Yes | Yes | |

To set your project's graphics API for each build target, refer to [Configure graphics APIs](xref:um-configure-graphics-apis) (Unity Manual).
