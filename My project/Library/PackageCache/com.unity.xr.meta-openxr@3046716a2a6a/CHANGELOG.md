---
uid: meta-openxr-changelog
---
# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.2.1] - 2025-09-25

### Changed

- Changed the minimum version of the XR Composition Layers dependency from 2.0.0 to 2.1.0, as 2.0.0 is no longer supported.

### Fixed

- Fixed a possible `NullReferenceException` when installing Unity OpenXR: Meta in a new project. ([MOXRB-151](https://issuetracker.unity3d.com/issues/installing-unity-openxr-meta-package-throws-nullreferenceexception-error))
- Fixed the `MetaOpenXRPassthroughLayer` so that passthrough correctly displays when the app is resumed after being suspended for a system permission dialogue.
- Fixed `MetaOpenXRPlaneSubsystem` so that it doesn't cause the app to unexpectedly quit if the subsystem is destroyed while hiding planes that don't match your chosen plane detection mode.
- Fixed the MetaOpenXROcclusionSubsystem so that it destroys all native resources when stopped, reducing its resource consumption while not in use.

## [2.2.0] - 2025-06-23

### Changed

- Changed the minimum version of the OpenXR Plug-in dependency from 1.14.0 to 1.15.0-pre.1.

### Fixed

- Fixed the Android manifest build logic so that `com.oculus.permission.USE_ANCHOR_API` is correctly applied if you enable Planes, Bounding Boxes, or Meshes, but not Anchors.
- Fixed the `MetaOpenXRCameraSubsystem` so that it correctly destroys the `PassthroughLayerData` when it is stopped.
- Fixed an issue where buonding boxes were not relocalizing while an app was resumed after being paused for scene capture or sleep.

## [2.2.0-pre.1] - 2025-05-06

### Added

- Added support for the new `BoundedPlane.parentId` and `XRBoundingBox.parentId` fields in AR Foundation 6.2. Support is implemented via the `XR_FB_spatial_entity_container` OpenXR extension.
- Added a setting to the Meta Quest Boundary Visibility feature that allows you to automatically suppress boundary visibility without writing any code. Refer to [Suppress boundary visibility automatically](xref:meta-openxr-boundary-visibility#suppress-automatically) for details.
- Added overrides for `XRAnchorSubsystem.TrySaveAnchorsAsync`, `XRAnchorSubsystem.TryLoadAnchorsAsync`, and `XRAnchorSubsystem.TryEraseAnchorsAsync` that improve performance when saving, loading, and erasing a batch of anchors on Meta Quest.
- Added `PlaneProviderType` and `ARPlaneFeature.planeProviderType`. These APIs lay the groundwork for future work.
- Added `XR_META_spatial_entity_sharing` and `XR_META_spatial_entity_group_sharing` OpenXR extensions to the **Meta Quest: Anchors** feature to support shared spatial anchors in a future update.
- Added a new property [MetaOpenXRAnchorSubsystem.sharedAnchorsGroupId](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRAnchorSubsystem.sharedAnchorsGroupId) for setting the group Id that is used when sharing anchors.
- Added android permission requirement `com.oculus.permission.IMPORT_EXPORT_IOT_MAP_DATA` to support shared anchors.
- Added support for sharing and loading shared anchors between colocated users. Refer to [Shared Anchors](xref:meta-openxr-shared-anchors) docs for more info.
- Added support for [Colocation Discovery](xref:meta-openxr-colocation-discovery) to discover physically colocated devices running the same app.

### Changed

- Recompiled the native plug-in with support for [16 KB page sizes](https://developer.android.com/guide/practices/page-sizes) on Android 15 or newer.
- Re-implemented the Meta Quest Camera (Passthrough) feature using the XR Composition Layers package, allowing you more customization options for how Passthrough images are composited into your app. If you choose not to customize your app's composition layers, the `MetaOpenXRCameraSubsystem` will create the required components at runtime for you. Refer to [Composition layers](xref:meta-openxr-camera#composition-layers) for more information.
- Changed the `MetaOpenXRPlaneSubystem` so that it doesn't log any errors if `xrGetSpaceBoundingBox2DFB` fails. There is a known issue in Meta Quest software version v74 and newer that causes this API repeatedly fail in some cases.

### Fixed

- Fixed the `MetaOpenXRAnchorSubsystem` so that it correctly registers its `subsystemTypeOverride`, allowing you to typecast instances of `XRAnchorSubsystem` to `MetaOpenXRAnchorSubsystem`.
- Fixed meshing subsystem not working with Meta Quest Link.
- Fixed an issue where Space Discovery and Space Persistence were required to enable the Meta Quest Anchors feature.
- Fixed the `MetaOpenXRAnchorSubsystem` so that `TrySaveAnchorAsync`, `TryLoadAnchorsAsync`, and `TryEraseAnchorsAsync` no longer throws an exception when the scripting define `ENABLE_UNITY_COLLECTIONS_CHECKS` is present.
- Fixed `MetaOpenXRAnchorSubsystem` so that loading persistent anchors now correctly assigns their `nativePtr`.
- Fixed the following features: Meta Quest Bounding Boxes, Meta Quest Display Utilities, Meta Quest Meshing, and Meta Quest Planes, so that they disable themselves and do not attempt to create any relevant subsystems if the OpenXR runtime does not support the required capabilities.
- Fixed the `MetaOpenXROcclusionSubsystem` so that it no longer logs an error if you create and destroy the subsystem without ever starting it.
- Fixed the native plug-in so that it correctly deletes cached OpenXR function pointers after the OpenXR session ends.

## [2.1.0] - 2025-02-19

### Added

- Added support for DirectX 12 and DirectX 11 to the Meta Quest Occlusion feature.

### Fixed

- Fixed the `MetaOpenXRAnchorSubsystem` so that it correctly cancels Awaitables when the subsystem is destroyed.
- Fixed `MetaOpenXRPlaneSubsystem`, `MetaOpenXRBoundingBoxSubsystem`, and `ARMeshFeature` so that when doffing and re-donning the headset, trackables are no longer reported as removed during the brief time while the headset is re-establishing tracking.

## [2.1.0-pre.1] - 2024-12-27

### Added

- Added a validation rule to warn you that soft shadows can negatively affect performance for Meta Quest.
- Added Scene setup documentation to clarify best practices for requesting Android system permissions.
- Added support for running scenes in the editor via [Quest Link](xref:meta-openxr-link).
- Added a public class [MetaOpenXRFeature](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRFeature) as the new base class for OpenXR features in this package.
- Added the Meta Quest Boundary Visibility feature, which enables you to suppress the visibility of the boundary so users can move freely with Passthrough enabled. Refer to [Boundary visibility](xref:meta-openxr-boundary-visibility) for more information.
- Added support for [Occlusion](xref:meta-openxr-occlusion).

### Changed

- Changed the **Meta Quest: Bounding boxes**, **Meta Quest: Anchors**, **Meta Quest: Meshing**, and **Meta Quest: Planes** features implementation to replace the use of `XR_FB_spatial_entity_query` and `XR_FB_spatial_entity_storage` with newer OpenXR extensions from Meta. This is a backwards-compatible change with no effect on your code.
- Changed the **Meta Quest: Camera (Passthrough)** feature to no longer enable the `XR_FB_composition_layer_alpha_blend` OpenXR extension. This is a backwards-compatible change with no effect on your code.
- Changed the severity level of validation rules for features that depend on **Meta Quest: Session** from Warning to Error. These features are unusable at runtime if **Meta Quest: Session** is not enabled, so Error is the appropriate severity level for the validation rules.
- Changed the dependency version of XR Core Utilities from 2.4.0 to 2.5.1.

### Removed

- Removed a validation rule that warned you if your currently open scene did not have an `ARCameraManager` component while the **Meta Quest: Camera (Passthrough)** feature was enabled. Too often this warning was a false positive for valid project setups, so it wasn't helpful.

### Fixed

- Fixed an issue with [BoundedPlane.nativePtr](xref:UnityEngine.XR.ARSubsystems.BoundedPlane.nativePtr), [XRBoundingBox.nativePtr](xref:UnityEngine.XR.ARSubsystems.XRBoundingBox.nativePtr), and [XRAnchor.nativePtr](xref:UnityEngine.XR.ARSubsystems.XRAnchor.nativePtr) so they now return a pointer to a struct with a version number and a pointer to the `XrSpace` handle of their respective trackable type. Refer to [Plane native pointer](xref:meta-openxr-planes#Native-pointer), [Bounding box native pointer](xref:meta-openxr-bounding-boxes#Native-pointer), and [Anchor native pointer](xref:meta-openxr-anchors#Native-pointer) for more information.
- Fixed Android Manifest setup for builds so that if your app uses Bounding Boxes or Meshing but not Planes, your app's Android Manifest will now correctly declare the `com.oculus.permission.USE_SCENE` permission.
- Fixed the **Meta Quest: Display Utilities** feature so that it works correctly even when **Meta Quest: Session** is disabled. ([MOXRB-70](https://issuetracker.unity3d.com/product/unity/issues/guid/MOXRB-70))
- Fixed the **Meta Quest: Anchors** and **Meta Quest: Passthrough** features so they gracefully fail to create their subsystems if the OpenXR runtime does not support the required system properties.

## [2.0.0] - 2024-04-30

### Added

- Added support for saving anchors, loading anchors, and erasing anchors introduced in AR Foundation 6.
- Added explicit dependencies for the XR Plug-in Management and XR Core Utils packages, which were previously implicit.

### Changed

- Changed the `MetaOpenXRPlaneSubsystem` to correctly apply the [PlaneClassifications.InvisibleWallFace](xref:UnityEngine.XR.ARSubsystems.PlaneClassifications.InvisibleWallFace) semantic label for invisible wall faces.
- Changed the plane and bounding box subsystems to no longer attempt to request the Android system permission `com.oculus.permission.USE_SCENE` on your app's behalf. To use these features with OpenXR Plug-in version 1.11.0 or newer, your app must request this permission. Refer to [Spatial Data Permission](https://developer.oculus.com/documentation/unity/unity-spatial-data-perm/) (Meta Quest Developer Center) to learn how to request this permission.
- Changed minimum version of AR Foundation dependency from 6.0.0-pre.7 to 6.0.1.
- Changed minimum Unity Editor version from 2023.3 to 6000.0 as required by AR Foundation 6.0.1.
- Changed minimum version of OpenXR Plug-in dependency from 1.9.1 to 1.10.0.

### Fixed

- Fixed an issue where trackable poses would fail to update when the OpenXR runtime returned valid but untracked data.
- Fixed an issue where planes that should be classified as `WallArt` were classified as `Other`.

### Security

## [2.0.0-pre.1] - 2024-03-15

### Added

- Added support for the [ARPlane.classifications](xref:UnityEngine.XR.ARFoundation.ARPlane.classifications) data type introduced in AR Foundation 6.0, including new classification values and support for planes with multiple classifications.
- Added an API for the tracking of 3D bounding boxes. Refer to [Bounding boxes](xref:meta-openxr-bounding-boxes) for more information.
- Added support for the [Asynchronous TryAddAnchor API](xref:arfoundation-whats-new#asynchronous-anchor-creation) introduced in AR Foundation 6.
- Added [XRResultStatusExtensions](xref:UnityEngine.XR.OpenXR.Features.Meta.XRResultStatusExtensions), extending the [XRResultStatus](xref:UnityEngine.XR.ARSubsystems.XRResultStatus) type to support conversion to [XrResult](xref:UnityEngine.XR.OpenXR.NativeTypes.XrResult).
- Added support for `NotAxisAligned` plane detection mode introduced in AR Foundation 6.0.
- Added support for the [Meshing](xref:meta-openxr-meshing) subsystem.

### Changed

- Changed the AR Foundation dependency version to 6.0.0-pre.7.
- Changed the minimum version of Unity to 2023.3, as required by AR Foundation 6.0.0-pre.7.
- Changed the behavior of `MetaOpenXRSessionSubsystem.sessionId` to now return a non-empty, unique Guid value per Meta OpenXR session. You can access the session id using `XRSessionSubsystem.sessionId`.
- Changed the way the Android Manifest is written to override the `IAndroidManifestRequirementProvider` interface so that direct XML manipulation is not necessary.
- Changed the names of the Meta Quest features in the Editor to remove the "AR" prefix.
- Changed documentation to indicate that Meshing is now a supported feature.

### Fixed

- Fixed an issue where the subsystem descriptor for `MetaOpenXRPlaneSubsystem` incorrectly stated that horizontal plane detection, vertical plane detection, and classification were unsupported. These features have all been supported since the 1.0 release, and the subsystem descriptor now returns the correct values.
- Fixed a very rare issue where plane detection could possibly fail to update planes due to an unhandled error.

## [1.0.1] - 2023-11-02

### Changed

- Updated documentation to reflect changes to the **Room Setup** process including it being renamed to **Space Setup** and relocated to **Settings** > **Physical Space**

### Fixed

- Fixed an issue where Meta's OpenXR runtime would log a warning every frame for each `ARPlane` in the scene regarding the use of an older version of their OpenXR API for semantic labels.
- Fixed documentation links for Meta-OpenXR features in `Project Settings` > `XR Plug-in Management` > `OpenXR`.

## [1.0.0] - 2023-09-30

### Added

- Added new extension methods for the `XRDisplaySubsystem` to support getting the supported display refresh rates and requesting a display refresh rate. Refer to [Meta Quest Display Utilities](xref:meta-openxr-display-utilities) for more information.
- Added documentation for recommended settings when using [Universal Render Pipeline](xref:meta-openxr-project-setup#universal-render-pipeline).

### Changed

- Renamed the AR features in the `Meta Quest` OpenXR feature group for brevity and consistency.
- Changed the `ARPlaneFeature` implementation to request the Android permission `com.oculus.permission.USE_SCENE` on Start on OpenXR runtime versions 1.0.31 and newer, as required by the OpenXR specification.
- Changed the `Unity.XR.MetaOpenXR` runtime assembly to only be included on Android and Editor platforms. This was always intended, but previously the assembly had been included on all platforms.

### Fixed

- Fixed an issue where the `ARAnchorFeature` enabled more OpenXR extensions than were necessary for the `XRAnchorSubsystem` to function correctly.

## [0.2.1] - 2023-09-14

### Added

- Added `com.oculus.permission.USE_SCENE` permission to the manifest which will show a permission dialog to get user consent if scene data such as planes is requested.
- Added support for `XRSessionSubsystem`'s `trackingState` and `notTrackingReason`.
- Added support for [Scene capture](xref:meta-openxr-session#scene-capture).
- Added validation rules. Go to **Project Settings** > **XR Plug-in Management** > **Validation Rules** to check if your project settings match the validation rules.

### Changed

- Split **AR Foundation: Meta** Feature into separate OpenXR Features, one for each AR Foundation subsystem. Go to **Project Settings** > **XR Plug-in Management** > **OpenXR** to enable or disable these features.
- Changed minimum Unity version to 2022.3
- Updated linked OpenXR version to 1.0.28 from 1.0.25.
- Changed the package name from **Meta OpenXR Feature** to **Unity OpenXR: Meta**.
- Changed blending factors for blending Unity content over passthrough from [source alpha, 1 - source alpha] to [1, 1 - source alpha] to support rendering additive materials.

### Fixed

- Fixed Session availability API to properly check that an OpenXR session instance exists.

## [0.1.2] - 2023-06-28

### Fixed

- Fixed math operations for comparing structs with single-precision values.
- Fixed an issue where it was possible to incorrectly remove or update planes before they were added in some uncommon situations. This resulted in an `InvalidOperationException` in AR Foundation that is now no longer thrown.

## [0.1.1] - 2023-06-12

### Added

- Added support for getting plane boundary vertices via [XRPlaneSubsystem.GetBoundary](xref:UnityEngine.XR.ARSubsystems.XRPlaneSubsystem.GetBoundary(UnityEngine.XR.ARSubsystems.TrackableId,Unity.Collections.Allocator,Unity.Collections.NativeArray{UnityEngine.Vector2}@)).
- Added support for getting plane alignment via [ARPlane.alignment](xref:UnityEngine.XR.ARFoundation.ARPlane.alignment) as well as [planeAlignmentThreshold](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRPlaneSubsystem.planeAlignmentThreshold). Refer to [Plane alignment](xref:meta-openxr-planes#plane-alignment) for more information.
- Added support for [plane classification](xref:UnityEngine.XR.ARSubsystems.PlaneClassification).
- Added support for planes to be updated and removed.
- Added a custom Editor for [ARCameraManager](xref:UnityEngine.XR.ARFoundation.ARCameraManager) to clarify that its serialized properties have no effect on Meta Passthrough.
- Added a custom Editor for [ARCameraBackground](xref:UnityEngine.XR.ARFoundation.ARCameraBackground) to clarify that it has no effect on Meta Quest devices.
- Added support for [requestedDetectionMode](xref:UnityEngine.XR.ARFoundation.ARPlaneManager.requestedDetectionMode) to filter horizontal and vertical planes.

### Changed

- Changed the `trackableId` property of planes to be backed by OpenXR's `uuid` field, instead of a monotonically increasing counter.
- Changed the display name of the AR Foundation Meta Quest feature.
- Grouped the Meta Quest OpenXR features in an OpenXR feature group.
- Passthrough layer is now composited behind AR content rather than alpha blended in front of it. This fixes an issue where AR opaques always appeared semi-transparent.

### Fixed

- Fixed an issue where the `trackingState` property of planes could be incorrectly reported in some rare cases.
- Fixed an issue where the center of a plane in plane space was incorrectly reported as a value other than (0, 0).
- Fixed an issue where `ARPlane`s were not removed if the `Redo Walls` action or the `Clear Room Set Up` action was performed through the Meta Quest menu.

## [0.1.0] - 2023-04-04

- This is the first release of Meta OpenXR Plugin <com.unity.xr.meta-openxr>.