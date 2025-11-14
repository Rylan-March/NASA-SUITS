---
uid: androidxr-openxr-camera
---
# Camera

On Android XR devices, AR Foundation's Camera subsystem controls [passthrough](#passthrough).

This page supplements the AR Foundation [Camera](xref:arfoundation-camera) manual. The following sections only contain information about APIs where Google's Android XR runtime exhibits platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

## Optional feature support

Android XR implements the following optional features of AR Foundation's [XRCameraSubsystem](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystem):

| Feature | Descriptor Property | Supported |
| :------ | :--------------- | :--------: |
| **Brightness** | [supportsAverageBrightness](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsAverageBrightness) | Yes |
| **Color temperature** | [supportsAverageColorTemperature](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsAverageColorTemperature) | Yes |
| **Color correction** | [supportsColorCorrection](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsColorCorrection) | Yes |
| **Display matrix** | [supportsDisplayMatrix](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsDisplayMatrix) | |
| **Projection matrix** | [supportsProjectionMatrix](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsProjectionMatrix) | |
| **Timestamp** | [supportsTimestamp](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsTimestamp) | |
| **Camera configuration** | [supportsCameraConfigurations](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsCameraConfigurations) | |
| **Camera image** | [supportsCameraImage](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsCameraImage) | |
| **Average intensity in lumens** | [supportsAverageIntensityInLumens](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsAverageIntensityInLumens) | Yes |
| **Focus modes** | [supportsFocusModes](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsFocusModes) | |
| **Face tracking ambient intensity light estimation** | [supportsFaceTrackingAmbientIntensityLightEstimation](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsFaceTrackingAmbientIntensityLightEstimation) | |
| **Face tracking HDR light estimation** | [supportsFaceTrackingHDRLightEstimation](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsFaceTrackingHDRLightEstimation) | |
| **World tracking ambient intensity light estimation** | [supportsWorldTrackingAmbientIntensityLightEstimation](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsWorldTrackingAmbientIntensityLightEstimation) | Yes |
| **World tracking HDR light estimation** | [supportsWorldTrackingHDRLightEstimation](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsWorldTrackingHDRLightEstimation) | Yes |
| **Camera grain** | [supportsCameraGrain](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsCameraGrain) | |
| **Image stabilization** | [supportsImageStabilization](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsImageStabilization) | |
| **Exif data** | [supportsExifData](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystemDescriptor.supportsExifData) | |

> [!NOTE]
> Refer to AR Foundation [Camera platform support](xref:arfoundation-camera-platform-support) for more information
> on the optional features of the camera subsystem.

## Passthrough

The passthrough camera captures real-time images of the external environment to provide the user with a view of their surroundings while wearing a headset. You can use passthrough to implement immersive mixed-reality experiences, by layering virtual content on top of passthrough images of the surrounding environment.

### Enable passthrough

Enable the [AR Camera Manager component](xref:arfoundation-camera-components#ar-camera-manager-component) to enable passthrough, and disable it to disable passthrough.

### AR Camera Background component

Android XR passthrough doesn't require the [AR Camera Background component](xref:arfoundation-camera-components#ar-camera-background-component). If `ARCameraBackground` is in your scene, it has no effect on Android XR devices. If your scene only targets Android XR devices, you can safely delete the AR Camera Background component from your XR Origin's **Main Camera** GameObject.

### Configure camera background

Android XR passthrough requires that your Camera has a transparent background. To do this, set your **Background Color** (Universal Render Pipeline) or **Clear Flags** (Built-In Render Pipeline) to **Solid Color**, with the **Background** alpha channel value set to `0`.

Refer to [Configure camera background for passthrough](xref:androidxr-openxr-scene-setup#camera-background-passthrough) to understand how to set your camera background.

## Image capture

This package does not support AR Foundation [image capture](xref:arfoundation-image-capture).

<a id="light-estimation"></a>

## Light estimation permissions

AR Foundation's light estimation feature requires an Android system permission on the Android XR runtime. Your user must grant your app the `android.permission.SCENE_UNDERSTANDING_COARSE` permission before it can track scene data.

To avoid permission-related errors at runtime, set up your scene with the AR Camera Manager component disabled, then enable it only after the required permission is granted.

The following example code demonstrates how to handle required permissions and enable the AR Camera Manager component:

[!code-cs[request_light_estimation_permission](../../Tests/Runtime/CodeSamples/PermissionSamples.cs#request_light_estimation_permission)]

For a code sample that involves multiple permissions in a single request, refer to the [Permissions](xref:androidxr-openxr-permissions) page.
