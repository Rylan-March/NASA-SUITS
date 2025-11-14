---
uid: androidxr-openxr-changelog
---
# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.2] - 2025-10-05

### Added
- Added support for [16 KB page sizes](https://developer.android.com/guide/practices/page-sizes) in the native plugin on Android 15 or newer.

### Fixed
- Fixed the trailing space issue with "android.hardware.xr.input.controller " in AndroidManifest

## [1.0.1] - 2025-07-29

### Changed
- Updated OpenXR Package dependency to 1.15.1.
- Updated documentation for enabling Application SpaceWarp.

## [1.0.0] - 2025-06-30

### Added
- Added validation rules to ensure the proper configuration of URP settings.

### Changed
- Updated OpenXR Package dependency to 1.15.0, ARF package to 6.2.0 and XR Hands package to 1.6.0.

### Fixed
- Fixed bug so the first load of a persistent anchor will no longer fail due to specific persistence resources not being ready.
- Fixed applications not launching when the Android XR Support feature was disabled.

## [1.0.0-pre.3] - 2025-05-30

### Changed
- Changed manifest editing behavior to replace the property `android.window.PROPERTY_ACTIVITY_STARTS_IN_IMMERSIVE_XR` with `android.window.PROPERTY_XR_ACTIVITY_START_MODE` to ensure app compatibility with Google Play Store.
- Updated the dependency version of the XR Hands package to 1.6.0-pre.3
- Changed strings in the Manifest for AOSP conformance

## [1.0.0-pre.2] - 2025-04-23

### Changed

- Changed the camera feature to not retry light estimation retrieval if the platform does not support it.
- Updated the dependency version of OpenXR to 1.14.3.

### Fixed
- Fixed `ARFaceFeature` so that it enables the `XR_ANDROID_eye_tracking` extension.
- Fixed the `CameraProvider` to not write extraneous logging when the light estimator is destroyed multiple times.
- Fixed a bug so that warning for insufficient eye tracking permission only occurs if neither fine permission nor coarse permission was granted.
- Fixed plane detection filtering so that the selected plane detection mode is obeyed.
- Fixed bug that the first save a persistent anchor will no longer fail due to specific persistence resources not being ready.

## [1.0.0-pre.1] - 2025-04-11

### Added
- Added support to request SCENE_UNDERSTANDING_FINE permission needed for raycast against depth
- Added [AndroidXRHandMeshData](xref:UnityEngine.XR.OpenXR.Features.Android.AndroidXRHandMeshData), which surfaces hand mesh data to `XRHandSubsystem` via `TryGetMeshData`.
- Added a validation rule to ensure the minimum API level of 24 ("Nougat").

### Changed

### Deprecated

### Removed

### Fixed
- Fixed native Occlusion provider to avoid crashes caused by calling `xrDestroyDepthSwapchainANDROID` on an already destroyed swapchain handle.
- Fixed a bug where warning for coarse scene understanding permission would still appear if fine permission was granted.

### Security

## [0.5.0-exp.1] - 2025-03-19

### Added

- Added a public class [AndroidXROpenXRFeature](xref:UnityEngine.XR.OpenXR.Features.Android.AndroidXROpenXRFeature) as the new base class for OpenXR features in this package.
- Added **Android XR Support** feature setting under **OpenXR Feature Groups** and introduced a new **Optimize Multiview Render Regions** settings option for Android XR within it.

### Changed

- Changed manifest editing behavior to include native library element for `libopenxr.google.so` and ensure app compatibility with Google Play Store.
- Moved **Optimize Buffer Discards (Vulkan)** option to the **Android XR Support** feature setting.
- Eye Tracking permission should be requested as either `EYE_TRACKING_COARSE` or `EYE_TRACKING_FINE`. Similarly, Scene Understanding permission should be requested as either `SCENE_UNDERSTANDING_COARSE` or `SCENE_UNDERSTANDING_FINE`.
- Android XR headers have been updated.
- Changed manifest editing behavior to include feature element for `android.software.xr.api.openxr` and ensure app compatibility with Google Play Store.
- Changed avatar eyes implementation to use new coarse eye tracking api.

### Removed

- Removed unused `Unity.XR.AndroidOpenXR.Tests.asmdef` assembly definition file so that the editor does not emit compile warnings.

### Fixed
- Brought depth occlusion API use into conformance with expectations - only one swapchain of RAW or
  SMOOTH is created at a time now, and switching during runtime rebuilds with the new swapchain.
- Fixed console warning caused by usage of deprecated [ShaderKeywords](xref:UnityEngine.XR.ARSubsystems.ShaderKeywords) type.
- Fixed over allocation of XrSpaces when attempting to load anchors.
- Fixed OpenXR context management so that non-AR OpenXR features can be functional when **AndroidXR: Session** is disabled.
- Fixed Android XR feature group so that AR Face is included in the group.

## [0.4.4-exp.1] - 2024-12-31

### Added

- Added support for erasing persistent anchors.
- Added default URP assets to be used with Android XR build profiles.
- Added an occlusion shader keyword to indicate that depth is linear.

### Changed

- Switching to the Android XR build target from the build profiles window now automatically enables the Android XR feature group and the Foveated Rendering feature in XR Plugin Management Settings window.
- Renamed occlusion shader property names.
- Changed the way depth information is obtained from using Texture2DArray to a RenderTexture with dimension `TextureDimension.Tex2DArray`.
- ARF version dependency has been bumped to 6.1.0-pre.4 for updated occlusion support

## [0.4.3-exp.1] - 2024-12-10

### Fixed
- Documentation fix for package versioning.

## [0.4.2-exp.1] - 2024-12-09

### Fixed
- `AndroidOpenXRCameraSubsystem`'s `XRCameraSubsystemDescriptor` now correctly reports the features which are supported by the camera provider.

## [0.4.1-exp.1] - 2024-12-05

### Added

- Updated the `UnityXRTextureDescriptor` to include a new field of type `XRTextureType` to match a corresponding change in AR Foundation v6.1.0-pre.3.
- Added support for getting the supported display refresh rates and setting the display refresh rate via the new extension methods for the `XRDisplaySubsystem`. Refer to [Android XR Display Utilities](xref:android-openxr-display-utilities) for more information.

### Changed

- Update `AndroidXR` term to `Android XR`.
- Loading persistent anchors and persistent anchor IDs now check for completion each frame and time out after 10 seconds.
- Update the `ARCameraFeature`'s UI name so that it renders as `Android XR: AR Camera` in the OpenXR project settings window.
- Changed manifest update behavior to include feature elements based on which OpenXR interaction profiles are enabled:
    - **Oculus Touch Contollrer Profile** or **Khronos Simple Controller Profile** - `android.hardware.xr.controller`
    - **Eye Gaze Interaction Profile** - `android.hardware.xr.eye_tracking`
    - **Hand Interaction Profile** - `android.hardware.xr.hand_tracking`
- Changed manifest update behavior to include the `android.software.xr.openxr` feature element in all builds.

### Removed
- Removed the `Android XR Stylus Controller Profile`. Use the [Oculus Touch Controller Profile](#androidxr-openxr-controller-profile) instead.

### Fixed
- Occlusion subsystem now correctly requests SCENE_UNDERSTANDING permission when it's the only SCENE_UNDERSTANDING-using subsystem enabled.
- Fixed the conversion of the pose in Occlusion frame to correctly convert from OpenXR format to
  Unity engine's format.
- Fixed documentation so that it recommends the `Oculus Touch Controller Profile` for input control.
- Fixes the camera provider such that a light estimator is only created when light estimation data is requested by the AR camera manager.
- Resolved a deprecated field being used in the occlusion provider that resulted in a warning in the
  Unity console.
- Resolved an issue so that compilation errors no longer occur when this package and `com.unity.xr.meta-openxr` are both installed, and at least one is installed from a tarball.

## [0.4.0-exp.1] - 2024-11-14

### Added

- Support for Occlusion. Occlusion helps virtual objects look more realistic by ensuring they are properly hidden behind or appear in front of real-world objects, as appropriate.
- URP Application Spacewarp: You will need Google's XR Extensions package installed to use Spacewarp.

### Changed

- Update `AndroidXR` term to `Android XR`.
- Update the `ARCameraFeature`'s UI name so that it renders as `Android XR: AR Camera` in the OpenXR project settings window.

### Fixed

- Fixed issue with TrySaveAnchorAsync so that it does not return a success when the returned guid is 0.

## [0.3.0-exp.1] - 2024-10-03

### Added

- Added a validation rule to AR Face, AR Camera, AR Raycast, AR Plane, and AR Anchor to ensure AR Session is present.
- Added missing permissions for Eye Tracked Foveated Rendering.
- Added the ability to save persistent anchors, load persistent anchors, and enumerate all saved persistent anchor IDs.
- Added `AndroidXRPerformanceMetrics` feature to enable access to performance metrics through [XRStats.TryGetStat](xref:UnityEngine.XR.Provider.XRStats.TryGetStat(UnityEngine.IntegratedSubsystem,System.String,System.Single&)).
- Added support for [light estimation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.0/api/UnityEngine.XR.ARFoundation.ARCameraFrameEventArgs.html#UnityEngine_XR_ARFoundation_ARCameraFrameEventArgs_lightEstimation).

### Fixed

- `ARFace`/`XRFace` objects now correctly report `TrackingState.Tracking` when eyes are fully tracked, `TrackingState.Limited` when only some tracking data is available, and `TrackingState.None` when no tracking data is available.
- `ARFaceFeature` objects now properly supports XR session transitions for begin, end, and input focus. Resources are acquired on the first update after a session begin, and released when the session ends. When focus is lost, `ARFaceFeature` will no longer poll for updates.
- Fixed support for raycasting against depth textures.
- Ensure that the environment blend mode is restored when an app is resumed after being paused.
- Fixed manifest editing behavior so that permission injections do not persist if their corresponding OpenXR feature is disabled between builds.

## [0.2.0] - 2024-06-21

### Added

- Add support for ARPlane.nativePtr API.
- Added validation rule to warn developers that pop-up windows will not work unless they enable the [Resizeable Activity](xref:UnityEditor.PlayerSettings.Android.resizeableActivity) Android player setting. (In earlier versions of Unity, this setting was named **Resizable Window**.)
- Added validation rule that requires [Application Entry Point](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PlayerSettings.Android-applicationEntry.html) set to **GameActivity**.
- Added "Optimize Buffer Discards (Vulkan)" option for OpenXR (nested under "AR Session" feature).
- Added support to enable Foveated Rendering when ARSession Feature is enabled. This require OpenXR plugin version 1.11.0 or newer.
- Added C# API for the Raycasting subsystem.
- Added [Project Setup](xref:androidxr-openxr-project-setup) page to package documentation.
- Added [Hand Tracking](xref:androidxr-openxr-hand-tracking) feature page to package documentation for detailing unique runtime-specific behavior on Android XR.
- Added [Permissions](xref:androidxr-openxr-permissions) page, and permissions information on the [Plane Detection](xref:androidxr-openxr-plane-detection#permissions) and [Hand Tracking](xref:androidxr-openxr-hand-tracking#permissions) feature pages.
- Added support for spatial anchors and anchors attached to planes.
- Added support for the `XR_ANDROID_avatar_eyes` OpenXR extension via AR Foundation's `XRFaceSubsystem`. Refer to the [manual page](xref:androidxr-openxr-faces) for more information.

### Changed

- Unset `XR_COMPOSITION_LAYER_UNPREMULTIPLIED_ALPHA_BIT` on OpenXR projection layer to enable additive blending.
- Updated documentation so that supported and unsupported features are listed in separate tables and fixed outdated details on the [Introduction](xref:androidxr-openxr-manual) page:
- Added Session, Device tracking, Camera, and Plane detection as supported features.
- Updated requirements section to reflect current dependency versions.
- Updated linked OpenXR version to 1.0.34.
- Changed manifest editing behavior so that Android XR manifest changes only get applied if OpenXR is the active XR plug-in provider and at least one of the Android XR features are enabled.
- Changed the minimum Unity version from 2023.3 to 6000.0, reflecting the new version number of Unity 6. Refer to the official [Unity 6 New Naming Convention](https://forum.unity.com/threads/unity-6-new-naming-convention.1558592/) announcement for more information.
- Changed minimum version of AR Foundation dependency from 6.0.0-pre.7 to 6.0.1.
- Changed manifest editing behavior so that `android.permission.SCENE_UNDERSTANDING` and `android.permission.HAND_TRACKING` permission entries are added if their relevant OpenXR features are enabled. Your app will need to request these permissions, and your user must grant them to be able to use certain subsystems. Refer to the [Permissions](xref:androidxr-openxr-permissions) page for more information.
- Updated plane subsystem so that it logs a warning on start up if the `android.permission.SCENE_UNDERSTANDING` permission has not been granted.
- If the **Android XR: AR Session** feature is enabled and the XR Hands package is present on at least version 1.5.0-pre.2, gesture detection constants will be set to match the range of motion in hand-tracking for Android XR.

### Fixed

- Fixed an issue where `AndroidOpenXRSessionSubsystem.sessionId` was always returning zero.
- Fixed this plugin's build processor to correctly enable Android XR support when **GameActivity** is used as the Android [application entry point](https://docs.unity3d.com/6000.0/Documentation/Manual/android-application-entries-activity.html).

## [0.1.0] - 2023-02-28

- This is the first release of Unity OpenXR: Android XR <com.unity.xr.androidxr-openxr>.
