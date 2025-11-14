---
uid: mirror-view-renderer
---

# Mirror View Renderer

The `MirrorViewRenderer` supports mirror view rendering for Main Display on XR mode. It includes all composition layer drawings.

> [!TIP]
> This component is not required if mirror view rendering with all comosition layers is already implemented on the XR plugin side.

![The Mirror View Renderer Component Inspector.](images/Inspector_MirrorViewRenderer.png)<br />*The Mirror View Renderer Component Inspector*

| Property:| Function: |
|:---|:---|
| Alpha Mode| Set how draw the eye texture. (**Opaque, Alpha and Premultiply**) |

## Background

In some case, the mirror view rendering with composition layers isn't supported by default.
- Some XR Plugins
- Built-in Render Pipeline

This function is provided to assist them.

## Supplement

Below shaders are added to the **Always Include Shaders list** when the MirrorViewRenderer is added.
- Unlit/XRCompositionLayers/Uber
- Unlit/XRCompositionLayers/BlitCopyHDR
