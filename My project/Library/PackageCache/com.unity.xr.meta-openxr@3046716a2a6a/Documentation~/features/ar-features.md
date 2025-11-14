---
uid: meta-openxr-ar-features
---
# AR features

AR features implement [AR Foundation](xref:arfoundation-manual) interfaces. These features are documented in the AR Foundation package manual, so this manual only includes information regarding APIs where Meta's OpenXR runtime exhibits unique platform-specific behavior.

This package implements the following AR features:

| Feature | Description |
| :------ | :---------- |
| [Session](xref:meta-openxr-session) | Enable, disable, and configure AR on the target platform. |
| [Camera](xref:meta-openxr-camera) | Render images from device cameras and perform light estimation. |
| [Planes](xref:meta-openxr-planes) | Use Scene Model data to track flat surfaces. |
| [Bounding boxes](xref:meta-openxr-bounding-boxes) | Use Scene Model data to track bounding boxes of 3D objects. |
| [Anchors](xref:meta-openxr-anchors) | Track arbitrary points in space. |
| [Ray casts](xref:meta-openxr-raycasts) | Cast rays against tracked items. |
| [Meshing](xref:meta-openxr-meshing) | Generate meshes of the environment. |
| [Occlusion](xref:meta-openxr-occlusion) | Occlude virtual content with physical objects. |

[!include[](../snippets/arf-docs-tip.md)]
