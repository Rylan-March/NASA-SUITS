---
uid: xr-layers-using
---

# Use Composition Layers

Add a GameObject containing a Composition Layer component to a Unity Scene to define a Composition Layer. The Unity XR Composition Layers package defines a set of layer types, but support for these types depends on the implementation provided for a specific device. A provider implementation for a device might not support all of these basic types and could provide additional types as well.


The XR Composition Layer package includes the following prefabs, meant to enable common use cases:

* __Cylinder UI Panel__: a Cylinder Layer prefab that has been set up to work with Unity Canvases and UI Elements.
* __Quad UI Panel__: a Quad Layer prefab that has been set up to work with Unity Canvases and UI Elements.
* __Projection Eye Rig__: a special Projection Layer prefab that allows you to render scene objects on a different Projection Layer than the Default Scene Layer. Refer to [Projection Eye Rig](projection-eye-rig.md) for more information.

You can add layer extension components to a GameObject that contains a layer component. A layer extension component defines data that can be used to determine or modify the layer rendering, depending on the device implementation. For example, the Source Texture component identifies the textures to be rendered to the layer. As with layer types, a provider implementation for a device might not support all the basic extensions and could provide additional extensions.

The basic, Unity-defined Composition Layer extensions include:

* __Source Textures__: identifies the texture or textures to be rendered to a layer.
* __Color Bias and Scale__: provides a color treatment to apply to the layer contents.


> [!NOTE]
> * XR Composition Layers are not the same as [Unity Layers]. The two features serve different purposes.
> * You can use a Unity Layer to determine which GameObjects get rendered to an XR Composition Projection Layer.


[Unity Layers]: xref:Layers
[Changing layer order]: xref:xr-layers-order
