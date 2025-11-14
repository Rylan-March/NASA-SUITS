---
uid: xr-layers-add-layer
---

# Add a composition layer

Add a composition layer to a scene by adding a GameObject with a CompositionLayer component. You can add layers in the Editor or at runtime.

## Add a layer in the Editor

In the Editor, you can add a composition layer in the following ways:

* Add a new GameObject containing the desired type of layer component using the Unity Editor menu: __GameObject > XR > Composition Layers__. You can also right-click in the Hierarchy window to open this menu.

* Add a layer component to an existing GameObject by selecting the GameObject in the Scene __Hierarchy__ window and choosing the layer type from the Unity Editor menu: __Component > XR > Composition Layers__.

* Add a layer component to an existing GameObject using the __Add Component__ button in the __Inspector__ window. In the __Add Component__ menu, choose __XR > Composition Layers > Composition Layer__ to add the component to the GameObject. Next, set the __Layer Type__ field in the __Inspector__ .

## Remove a layer in the Editor

To remove a layer from the scene, delete its parent GameObject or the layer component and any associated components.

You can temporarily [disable a layer](xref:xr-layers-enable) by deactivating its parent GameObject.

## Add a layer at runtime

Add a layer component at runtime with the GameObject [AddComponent<T>](xref:UnityEngine.GameObject.AddComponent) method.

``` csharp
using UnityEngine;
using Unity.XR.CompositionLayers;
using Unity.XR.CompositionLayers.Services;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CompositionLayers.Extensions;

public static class LayerHelper
{
    public static void AddQuadLayer(GameObject parentGO, Texture textureForQuad)
    {
        // Add a CompositionLayer component to the GameObject
        CompositionLayer layer = parentGO.AddComponent<CompositionLayer>();

        // Set up the layer
        if (layer != null) // layer was successfully created
        {
            // Set the layer data type
            layer.ChangeLayerDataType(typeof(QuadLayerData));

            // (Optional) Add any suggested components for the layer type
            layer.AddSuggestedExtensions();

            // Set layer extension properties
            TexturesExtension textureExt = parentGO.GetComponent<TexturesExtension>();
            textureExt.TargetEye = TexturesExtension.TargetEyeEnum.Both;
            textureExt.LeftTexture = textureForQuad;
        }
    }
}
```

## Remove a layer at runtime

Remove a composition layer from a scene by destroying the [CompositionLayer] component and any associated extension components. You can also destroy the parent GameObject, which automatically destroys any associated components.

Given a reference to the parent GameObject:

``` csharp
using UnityEngine;
using Unity.XR.CompositionLayers;

public static class LayerHelper2
{
    public static void RemoveLayerFromGameObject(GameObject parent)
    {
        CompositionLayer layer = parent.GetComponent<CompositionLayer>();
        if(layer != null)
        {
            // First destroy any extensions...
            Component[] extensions = parent.GetComponents<CompositionLayerExtension>();
            foreach(Component extension in extensions)
            {
                GameObject.Destroy(extension);
            }

            // Then destroy the layer
            GameObject.Destroy(layer);
        }
        else
        {
            Debug.LogWarning("Tried to remove a layer from a GameObject that has none.");
        }
    }
}
```
