This package defines the following OpenXR Features:

| **Feature** | **Description** |
| :---------- | :-------------- |
| [Display utilities](xref:meta-openxr-display-utilities) | Get the supported display refresh rates for the device and request a display refresh rate. |
| [Boundary visibility](xref:meta-openxr-boundary-visibility) | Suppress the visibility of the boundary. |

AR Features implement [AR Foundation](xref:arfoundation-manual) interfaces. This package implements the following AR features:

| **AR Feature** | **Description** |
| :------------- | :-------------- |
| [Session](xref:meta-openxr-session) | Enable, disable, and configure AR on the target platform. |
| [Camera](xref:meta-openxr-camera) | Render images from device cameras and perform light estimation. |
| [Colocation discovery](xref:meta-openxr-colocation-discovery) | Discover physically colocated devices running the same app. |
| [Planes](xref:meta-openxr-planes) | Use Scene Model data to track flat surfaces. |
| [Bounding boxes](xref:meta-openxr-bounding-boxes) | Use Scene Model data to track bounding boxes of 3D objects. |
| [Anchors](xref:meta-openxr-anchors) | Track arbitrary points in space. |
| [Ray casts](xref:meta-openxr-raycasts) | Cast rays against tracked items. |
| [Meshing](xref:meta-openxr-meshing) | Generate meshes of the environment. |
| [Occlusion](xref:meta-openxr-occlusion) | Occlude virtual content with physical objects. |
