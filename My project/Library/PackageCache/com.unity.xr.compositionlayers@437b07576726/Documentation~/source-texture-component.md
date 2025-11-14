---
uid: xr-layers-source-textures
---

# Source Textures component

Add a Source Textures extension component to specify textures to render to a layer. See [Add or remove a composition layer]. On Android, for Quad and Cylinder layer types, you can also specify an [Android Surface](#android-surface) as the source texture.

![The Source Textures component Inspector.](images/Inspector_SourceTextures.png)<br />*The Source Textures component Inspector*

| Property:| Function: |
|:---|:---|
| Source | Specify the source of the texture - Local Texture or Android Surface. |
| Target Eye | Specify whether one texture is used for both eyes or an individual texture is used for each eye. Only shown for layer types that support stereo. |
| Texture (Local Texture Only)| Specify a texture to use. Click Select to choose a texture or drag-and-drop a texture onto the control with your mouse. |
| Resolution (Android Surface Only)| Specify the resolution for the Android Surface with X for width, and Y for height. |
| Maintain Aspect Ratio (Quad/Cylinder Layer/Android Surface Only)| Crop the layer to fit the aspect ratio of the texture. |
| In-Editor Emulation (Projection Layer Only)| Specify whether the left or right eye texture is shown in the Unity Editor. |
| Custom Rects| Enable to specify custom rects within the source and destination textures. |
| Source Rects<sup>1</sup>| Specifies a rectangle within the source texture to copy to the destination rather than copying the entire texture. Use your mouse to set the rect values or enter them into the x, y, w, and h fields. |
| Destination Rects<sup>1</sup>| Specifies a target rectangle within the destination texture to which to write the source texture rather than filling the entire destination texture. Use your mouse to set the rect values or enter them into the x, y, w, and h fields. |

<sup>1</sup> Custom source rects and custom destination rects are not supported by composition layers of cube projection or equirect types.

> [!NOTE]
> Different types of composition layers support different texture settings. Only the settings supported by the current type of layer are shown in the Inspector.

## Custom source rects

You can use a custom source rect to define a rectangle subset of the source texture to use. The upper left of the source texture is coordinate (0,0) and the lower right is coordinate (1,1). Likewise, the full width and height are normalized to (1,1).

![Demonstrative image of source rect.](images/SourceRect.png)<br />*A source rect of approximately (.3, .3, .3, .3)*

## Custom destination rects

You can use a custom destination rect to define where to place the texture within a composition layer. The upper left of the layer is coordinate (0,0) and the lower right is coordinate (1,1). Likewise, the full width and height are normalized to (1,1).

![Demonstrative image of source rect.](images/DestinationRect.png)<br />*A destination rect of approximately (.25,.25,.5,.5)*

## Android Surface

In Unity, when you are working with Android development, you can interact with the Android Surface for rendering graphics or displaying content such as hardware decoded video. You can render Android Surface directly to a quad or cylinder composition layer by choosing Android Surface as the texture Source. You can install OpenXR package to have Android Surface supported out of the box. To obtain the Android Surface object to use for a layer, you must call OpenXR package API `GetLayerAndroidSurfaceObject` in a script.

Example script of getting the layer Surface Object using OpenXR package API - [GetLayerAndroidSurfaceObject(int layerId)](xref:UnityEngine.XR.OpenXR.CompositionLayers.OpenXRLayerUtility.GetLayerAndroidSurfaceObject(System.Int32))

``` csharp
// Get Android Surface Object
IntPtr surface = IntPtr.Zero;
surface = OpenXRLayerUtility.GetLayerAndroidSurfaceObject(layer.GetInstanceID());
```
For a completed example, please see the `Sample External Android Surface Project` sample. You can import this sample into a project from the **Package Manager** window:

1. Open the **Package Manager** (menu: **Window &gt; Package Manager**)
2. Select the **XR Composition Layers** package in the list of packages in your project.
3. Towards the bottom of the window, select the **Samples** tab.
4. Click **Import** next to the **Sample External Android Surface Project** item.

For more information about  Android Surfaces, refer to:
*  [Android Surface](https://developer.android.com/reference/kotlin/android/view/Surface)
*  [XR_KHR_android_surface_swapchain](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#XR_KHR_android_surface_swapchain)

[Source Textures extension]: xref:xr-layers-source-textures
[Add or remove a composition layer]: xref:xr-layers-add-layer
