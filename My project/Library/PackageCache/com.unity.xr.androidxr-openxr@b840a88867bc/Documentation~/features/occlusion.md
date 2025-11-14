---
uid: androidxr-openxr-occlusion
---
# Occlusion

This page supplements the AR Foundation [Occlusion](xref:arfoundation-occlusion) manual. The following sections only contain information about APIs where Google's Android XR runtime exhibits platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Optional feature support

Android XR implements the following optional features of AR Foundation's `XROcclusionSubsystem`:

| Feature | Descriptor Property | Supported |
| :------ | :------ | :------:  |
| **Environment Depth Image** | [environmentDepthImageSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.environmentDepthImageSupported) | Yes |
| **Environment Depth Confidence Image** | [environmentDepthConfidenceImageSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.environmentDepthConfidenceImageSupported) | Yes |
| **Environment Depth Temporal Smoothing** | [environmentDepthTemporalSmoothingSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.environmentDepthTemporalSmoothingSupported)| Yes |
| **Human Segmentation Stencil Image** | [humanSegmentationStencilImageSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.humanSegmentationStencilImageSupported)| |
| **Human Segmentation Depth Image** | [humanSegmentationDepthImageSupported](xref:UnityEngine.XR.ARSubsystems.XROcclusionSubsystemDescriptor.humanSegmentationDepthImageSupported) | |

## Occlusion samples

AR Foundation provides the `HMDOcclusion` sample to demonstrate occlusion on head-mounted displays (HMDs). For more information about HMD occlusion, refer to [AR Shader Occlusion component](xref:arfoundation-shader-occlusion) (AR Foundation).

To test occlusion on Android XR with the occlusion sample:

1. Clone the AR Foundation Samples repo from the [AR Foundations Samples GitHub repository](https://github.com/Unity-Technologies/arfoundation-samples/tree/6.1) and open it in Unity.
2. Open the `HMDOcclusion` scene in Unity from `Assets/Scenes/Occlusion/HMDOcclusion`.
3. Create a new material and set its shader to `Assets/Shaders/Occlusion/OcclusionSimpleLit/OcclusionSimpleLit.shader`.
    Refer to [Creating Materials](xref:um-create-material) for more information about creating and applying materials.
4. Apply the new material to a sample cube in the scene and position as desired.
5. Open the **AROcclusionManager** in the **Inspector** window, and disable **Temporal Smoothing**.

## Permissions

AR Foundation's occlusion detection feature requires an Android system permission on the Android XR
runtime. Your user must grant your app the `android.permission.SCENE_UNDERSTANDING_FINE` permission
before it can track occlusion data.

To avoid permission-related errors at runtime, set up your scene with the AR Occlusion Manager
component disabled, then enable it only after the required permission is granted.

The following example code demonstrates how to handle required permissions and enable the AR
Occlusion Manager component:

[!code-cs[request_occlusion_permission](../../Tests/Runtime/CodeSamples/PermissionSamples.cs#request_occlusion_permission)]

For a code sample that involves multiple permissions in a single request, refer to the
[Permissions](xref:androidxr-openxr-permissions) page.
