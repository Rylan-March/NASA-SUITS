using Unity.XR.CompositionLayers;
using Unity.XR.CompositionLayers.Services;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features.Meta;

namespace UnityEditor.XR.OpenXR.Features.Meta
{
    static class PassthroughLayerCreateUtil
    {
        [MenuItem("GameObject/XR/Composition Layers/Meta Passthrough Layer", false, 80)]
        static void CreatePassthroughLayer()
        {
            var gameObject = new GameObject("Passthrough Layer");
            Undo.RegisterCreatedObjectUndo(gameObject, "Create passthrough layer");
            gameObject.SetActive(false);
            AddPassthroughCompositionLayer(gameObject);
            gameObject.SetActive(true);
        }

        [MenuItem("Component/XR/Composition Layers/Meta Passthrough Layer", false, 120)]
        static void CreatePassthroughLayerComponent()
        {
            var gameObject = Selection.activeGameObject;
            AddPassthroughCompositionLayer(gameObject);
        }

        [MenuItem("Component/XR/Composition Layers/Meta Passthrough Layer", true)]
        static bool ValidatePassthroughLayerComponent()
        {
            var gameObject = Selection.activeGameObject;
            if (gameObject == null)
                return false;

            var layer = gameObject.GetComponent(typeof(CompositionLayer));
            if (layer != null)
                return false;

            return true;
        }

        static void AddPassthroughCompositionLayer(GameObject gameObject)
        {
            var layerDataType = typeof(PassthroughLayerData);
            var descriptor = CompositionLayerUtils.GetLayerDescriptor(layerDataType);

            var layer = Undo.AddComponent<CompositionLayer>(gameObject);
            if (layer == null)
                return;

            var layerData = CompositionLayerUtils.CreateLayerData(layerDataType);
            layer.ChangeLayerDataType(layerData);
            foreach (var extension in descriptor.SuggestedExtensions)
            {
                if (extension.IsSubclassOf(typeof(MonoBehaviour)))
                    Undo.AddComponent(gameObject, extension);
            }
        }
    }
}
