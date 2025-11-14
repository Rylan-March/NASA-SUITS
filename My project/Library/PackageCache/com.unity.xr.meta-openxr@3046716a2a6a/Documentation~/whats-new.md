---
uid: meta-openxr-whats-new
---
# What's new in version 2.2

This release includes the following significant changes:

## New features

### Composition layers

- Re-implemented the Meta Quest Camera (Passthrough) feature using the XR Composition Layers package, allowing you more customization options for how Passthrough images are composited into your app. If you choose not to customize your app's composition layers, the `MetaOpenXRCameraSubsystem` will create the required components at runtime for you. Refer to [Composition layers](xref:meta-openxr-camera#composition-layers) for more information.

### Shared Anchors

- Added support for [Shared anchors](xref:meta-openxr-anchors#shared-anchors).

### Colocation Discovery

- Added support for [Colocation Discovery](xref:meta-openxr-colocation-discovery) to share small messages between colocated users. Use colocation discovery to enable colocated users of the same app to join a networked session.

## Improvements

### Automatically hide the boundary

- Added a setting to the Meta Quest Boundary Visibility feature that allows you to automatically suppress boundary visibility without writing any code. Refer to [Suppress boundary visibility automatically](xref:meta-openxr-boundary-visibility#suppress-automatically) for details.

For a full list of changes in this version including backwards-compatible bugfixes, refer to the package [changelog](xref:meta-openxr-changelog).
