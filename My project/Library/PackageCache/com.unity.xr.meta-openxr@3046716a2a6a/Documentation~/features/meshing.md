---
uid: meta-openxr-meshing
---
# Meshing

This page is a supplement to the AR Foundation [Meshing](xref:arfoundation-meshing) manual. The following sections only contain information about APIs where Meta Quest exhibits unique platform-specific behavior.

## Space Setup

Before your app can access meshes from Meta's OpenXR runtime, the user must first complete [Space Setup](xref:meta-openxr-device-setup#space-setup) on their device.

> [!Important]
> If Space Setup is not complete, AR Foundation can't access mesh data from the device.

[!include[](../snippets/arf-docs-tip.md)]

## Permissions

Your app's user must grant an Android system permission before you can access mesh data. Refer to [Permissions](xref:meta-openxr-scene-setup#permissions) for more information.
