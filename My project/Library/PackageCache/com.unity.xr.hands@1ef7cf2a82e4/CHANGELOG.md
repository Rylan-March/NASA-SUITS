---
uid: xrhands-changelog
---
# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.7.0] - 2025-10-16

### Added

- Added subsystem APIs and data types for configuring the behavior of `XRHandDevice`:
    - Added `XRHandSubsystemConfiguration` and `XRHandSubsystem.UpdateHandsConfiguration` to enable subsystem configuration.
    - Added `XRHandSubsystemConfiguration.XRHandDevicePoseSource` to enable configuration of the `XRHandDevice` pose data, allowing `XRHandDevice` to report OpenXR compliant poses for existing controls.
- Added additional controls to the `XRHandDevice` for parity with the OpenXR Hand Interaction Profile.
- Added **XR Hand Capture** feature for recording and replaying hand data. Refer to the [XR Hand Capture manual](xref:xrhands-xrhandcapture). Also added a new sample [Hand Capture](xref:xrhands-xrhandcapture-recording) for recording the hand data from XR devices.

### Fixed

- Deprecated `XRHandSkeletonDriver.m_HasRootTransform` in favor of `XRHandSkeletonDriver.hasRootTransform`.
- Fixed missing Tracking State reference in the HandVisualizer and Input Action configuration in HandGestures sample scenes.
- Fixed the Hand Tracking feature so that if you deinitialize the OpenXR Loader and re-initialize it while your app is running, `XRHandProviderUtility` no longer throws a `NullReferenceException`.
- Fixed an issue with the build tooling where the native libraries were not getting correctly compiled with support for [16 KB page sizes](https://developer.android.com/guide/practices/page-sizes).
- Updated Android XR hand meshes and hand rigs in order to address visual feedback.

### Changed

- Marked `XRHand.GetJoint(XRHandJointID id)` as `readonly`.
- Made `XRHandJoint` serializable and its fields serialized.

### Known Issues

- **Hand Capture** sample scene: Quest Link is not currently supported.
- **Gestures** sample scene: Exiting Play Mode will emit `ObjectDisposedException` errors and related error output.

## [1.6.1] - 2025-07-23

### Fixed

- Fixed some deformations in the authored hand mesh provided for Android XR development that is a part of the Hand Visualizer sample. Note: A firmware update on the Android XR devices may be required for the meshes to appear correctly since they depend on an updated hand tracking data model.

## [1.6.0] - 2025-06-24

### Added

- Added new custom editor inspector for hand shapes.
- Added `XRHandSubsystem.TryGetMeshData` to attempt retrieval of runtime-supplied hand mesh data, if it's supported on that platform and enabled.
- Added ability to switch between hand meshes depending on the underlying runtime and vendor. The initial support provides a mesh for OpenXR conformant devices such as Android XR and a mesh to support Meta Quest devices by way of OpenXR.
- Added project validation rules to the Gestures sample to check if the HandVisualizer sample is missing and needs to be imported, and to verify whether the package version needs to be updated.
- Added `XRHandProviderUtility.CreateHand` for manually creating and initializing an `XRHand` instance. This is intended for advanced scenarios where `XRHand` is used independently of the `XRHandSubsystem`.
- Added `XRHandProviderUtility.DisposeHand` to release allocated resources for `XRHand` instances created manually via `XRHandProviderUtility.CreateHand`. This should not be used for `XRHand` managed by the `XRHandSubsystem`.
- Added `CalculateFingerShapeUncached`, which always calculates the requested `XRFingerShape` values without any attempt to save on performance by using previously requested results.

### Fixed

- Fixed the help documentation links in the OpenXR Project Settings where hands features were exposed in the Feature Groups editor window.
- Recompiled the native plug-in with support for [16 KB page sizes](https://developer.android.com/guide/practices/page-sizes) on Android 15 or newer.
- Fixed meta file collision with PolySpatial.
- Fixed URP material importing by adding a `MaterialPipelineHandler` script along with the corresponding asset to detect and reassign material shaders for the HandVisualizer Sample. [XRHB-78](https://issuetracker.unity3d.com/product/unity/issues/guid/XRHB-78)
- Fixed hand model references for the Hand Visualizer in the HandGesture scene of the Gestures sample.

## [1.5.0] - 2024-09-23

### Added

- Added `XRCommonHandGestures` and surfaced related data from the OpenXR Hand Interaction Profile extension.
- Added public `subsystem` property to `XRHandTrackingEvents` to let users get the `XRHandSubsystem` that is the source of the hand tracking data. Also added public `subsystem` property to `XRHandJointsUpdatedEventArgs` so it is available in the `XRHandTrackingEvents.jointsUpdated` event callback.
- Added support for a new `XRHandJointTrackingState.HighFidelityPose` flag to the XR Hands Subsystem.
    - Added support for the HighFidelityPose tracking flag to the OpenXRHandProvider.
    - Updated hand visualization sample to color debug joint cubes based on tracking fidelity: opaque white for joints which have a pose and are high fidelity; translucent red for joints which have a pose and are not high fidelity.
- Added OpenXR callbacks `HandTracking.subsystemCreated` and `HandTracking.destroyingSubsystem` for lifecycle response.
- Added OpenXR APIs `HandTracking.automaticallyInitializeSubsystem` and `HandTracking.EnsureSubsystemInitialized` to allow for delayed initialization of the `XRHandSubsystem`. To disable automatic creation in time, set `HandTracking.automaticallyInitializeSubsystem` to `false` in a `static` method decorated with `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`, then call `HandTracking.EnsureSubsystemInitialized` when ready.

### Changed

- Allows for ignoring tip-most knuckle when calculating full curl when the `XRFingerShapeConfiguration`'s `minimumFullCurlDegrees3` or `maximumFullCurlDegrees3` is non-positive.

### Fixed

- Fixed reference direction when using `XRHandUserRelativeDirection.NoseDirection` for gesture detection.
- Fixed runtime invalidation, which resulted in crashes on some OpenXR runtimes. Runtime is invalidated when the `XrInstance` is destroyed.
- Fixed threshold-checking on gesture conditions to allow for the full range in `XRFingerShapeCondition`.
- `XRCommonHandGestures` data retrieval no longer GC.Allocs.
- Updated app space change behavior for XR provider to no longer destroy hand trackers only to recreate them the next update.
- Fixed issue where XrHands would try to update hand joints with an uninitialized XrTime value of XR_INFINITE_DURATION.
- Fixed an issue with packaging that prevented MacOS compatible libraries from being included as part of the package.

## [1.4.0] - 2024-02-27

### Changed

- Changed gesture detection to allow both upper and lower threshold for more finely-tuned configuration of the finger shapes.
- Added partial-grab gesture detection to the Gestures sample.
- Changed hand and gesture tracking debug UI to support upper threshold, lower threshold, and partial-grab gesture.
- Updated visuals for hand and gesture tracking debug UI in the Gestures sample.

### Fixed

- Fixed compilation errors on tvOS platform where `ENABLE_VR` is not defined.

## [1.4.0-pre.1] - 2023-11-27

### Added

- Added support for custom gestures. For additional information, refer to the [manual documentation](xref:xrhands-custom-gestures).

### Fixed

- OpenXR implementation now correctly responds to feature lifecycle callbacks.

## [1.3.0] - 2023-08-14

### Added

- Added a root pose offset to the `XRHandSkeletonDriver` that allows an additional offset to the root pose (typically the wrist). This is useful for situations where you want the visual hand representation to stop while interacting with objects in a scene.
- Added missing sample script API documentation to make it easier to understand and build from.

### Fixed

- Fixed documentation links for OpenXR features to point at latest documentation version.

## [1.2.1] - 2023-05-30

### Fixed

- Fixed build pipeline that caused `UnityOpenXRHands` assembly to get skipped.

## [1.2.0] - 2023-05-25

### Added

- Added `IXRHandProcessor` interface that intercepts and allows post-processing joint data before being used by other scripts. More information can be found on the [XR Hands Joint Processing](xref:xrhands-process-joints) manual page.
- Added two samples to the Hand Visualizer sample for how to use the new Joint Processing feature to change the visual output of hand tracking.
- `XRHand` and `XRHandJoint` both now implement `IEquatable`, and have `Equals`, `GetHashCode`, and the `==` and `!=` operators overloaded.
- Added `XRHandTrackingEvents` component that subscribes to hand tracking events for a specific hand and surfaces Unity Events when the hand updates or tracking state changes.
- Added `XRHandMeshController` component that subscribes to events from a `XRHandTrackingEvents` and can optionally show or hide a mesh.
- Added `XRHandSkeletonDriver` component that subscribes to events from a `XRHandTrackingEvents` and updates a hierarchy of Transforms for each `XRHandJoint`.

### Changed

- Minimum supported Unity Editor version has changed to 2021.3.

### Fixed

- Fixed Hand Visualizer component so it will use another running `XRHandSubsystem` if the first is stopped.
- Fixed exception in Hand Visualizer component when a joint is invalid.
- Fixed rendering of lines that connect joints in `HandVisualizer` sample when used with the device simulator. Also removed the Origin property from the component since it was no longer used.

## [1.1.0] - 2023-03-16

### Added

- Added `isTracked` to `XRHand`.
- Added `trackingAcquired` and `trackingLost` to `XRHandSubsystem`.
- Added `XRHandDevice` and automatic managing of it if the Input System is enabled and hand-tracking is enabled in the target platform's build settings.
- The `HandVisualizer` sample now opts into using optimized controls in the Input System if you enable the **Use Optimized Controls** option. You must be on at least version 1.5.0 of the Input System package for this to have an effect.
- Added `handedness` getter property to `XRHandJoint`.
- Added `updateSuccessFlags` property to `XRHandSubsystem` to allow the most recent `UpdateSuccessFlags` to be polled. This value matches the argument to the `updatedHands` callback.

The OpenXR package must be installed and enabled for these to work:
- Added OpenXR support through `HandTracking` and `OpenXRHandProvider` types.
- Added support for Meta Hand Tracking Aim extension in OpenXR through `MetaHandTrackingAim`.

### Changed

- `MetaAimHand` and `MetaAimFlags` have been moved outside of the `MetaHandTrackingAim` type and moved from the `UnityEngine.XR.Hands.OpenXR` namespace to `UnityEngine.XR.Hands`. The same input bindings will work the same as before.
- The HandVisualizer sample meshes has been updated.
- Changed Hand Visualizer component so it skips setting the shared material on the instantiated hand mesh prefabs when the Hand Mesh Material property is not set.
- Providers must now create joints with a known `Handedness` during `TryUpdateHands` when calling `XRHandProviderUtility.CreateJoint`.
- Changed Meta Aim Hand input devices that are added to the Input System to continue updating the `devicePosition` and `deviceRotation` controls even when the `MetaAimFlags.Valid` bit flag is not set. Instead it uses whether the hand root is valid. In practice, the `MetaAimFlags.Valid` flag currently does not necessarily indicate whether the pose is valid but instead is based on whether the user is in a natural orientation for distant UI panel selection.
- Changed HandVisualizer sample by adding an Assembly Definition file (`.asmdef`).

### Fixed

- Fixed issue where OpenXR would incorrectly report joints as having updated when they weren't actually tracked that frame. `XRHandSubsystem`'s `trackingAcquired` and `trackingLost`, as well as `XRHand.isTracked`, will now work as expected.
- Fixed issue where `<XRHandDevice>/isTracked` and `<XRHandDevice>/trackingState` controls would never clear.
- Fixed `XRHandDevice` and `MetaAimHand` to also include `InputDeviceCharacteristics.TrackedDevice` in the `XRDeviceDescriptor`.
- Fixed lifetime of GameObjects in `HandVisualizer` sample.
- Fixed `HandVisualizer` so it uses the `XROrigin.Origin` property instead of the Transform of the `XROrigin` itself when transforming joints.
- Reinstated the validation rule that the Meta Touch Interaction Profile is required in the OpenXR Interaction Profiles list.
- OpenXR hand root poses now match the wrist, not the palm.

## [1.0.0-pre.2] - 2022-10-26

### Added

Added subsystem for cross-platform hand-tracking and accompanying types.

Types you might care about as a user:
- `XRHandSubsystemDescriptor`, which you can retrieve a `List` of with `SubsystemManager.GetSubsystemDescriptors`.
- `XRHandSubsystem`, which can be created with a call to `Create` on the above descriptor type.
- `XRHand`, which you can retrieve from `XRHandSubsystem` with its `leftHand` and `rightHand` properties. These contain joints and `rootPose` data, as well as its `Handedness`.
- `XRHandJoint`, which you can retrieve from each `XRHand` using `GetJoint` to query for joint tracking state, pose, radius, and velocity data on a per-joint basis.
- `XRHandJointIDUtility`, which contains extension methods for certain `enum`s listed below and also houses `FromIndex`, which you can use when looping over an array to get the corresponding `XRHandJointID` (useful when calling `XRHand.GetJoint`).

As well as these `enum`s:
- `XRHandJointTrackingState`, a flags-`enum` used to denote which fields are valid and can be retrieved via their `TryGet...` methods on `XRHandJoint`.
- `XRHandJointID`, used to identify each joint, and required when accessing joint data using `XRHand.GetJoint`. If looping over an array of joint data, use `XRHandJointIDUtility.FromIndex` to convert your index to `XRHandJointID`, which is required for `XRHand.GetJoint`.
- `Handedness`, used to identify which hand is referred to by an `XRHand` using its `handedness` property.
- `XRHandFingerID`, not used anywhere else in the API surfaced in this package besides its extension methods in `XRHandJointIDUtility`: `GetFrontJointID` and `GetBackJointID`, which together provider an inclusive range for `XRHandJointID`s spanned by the finger represented by `XRHandFingerID`.

Additional types you may need to interact with if writing a provider (not a common use case):
- `XRHandSubsystemProvider`, which the subsystem asks for data whenever its `TryUpdateHands` is called (built-in Unity setup calls this each frame) and is also queried when the subsystem and provider are created for which common joints are in the provider's layout using `GetHandLayout`.
- `XRHandProviderUtility`, which providers should call into using `CreateJoint` during the `TryUpdateHands` per-frame call to fill out the left- and right-hand joint arrays. This same type also has a nested `SubsystemUpdater` type to be used for automatically updating the subsystem each frame. Users can respond to updates by subscribing to the subsystem's `handsUpdated` callback.

Also added a visualizer sample, which demonstrates drawing using both meshes and per-joint prefabs. You can add this sample to your project through the Samples tab of this package's view in the Package Manager window. This sample has a mesh and script that assume OpenXR layout, so it is recommended you either use that plug-in or another one that conforms to the OpenXR hand joint layout.
