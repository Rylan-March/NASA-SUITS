using UnityEditor;

namespace Unity.XR.CompositionLayers.UIInteraction
{
    /// <summary>
    /// Helper for creating a TagManager asset file for controlling Canvas Layers
    /// </summary>
    internal class TagManagerController
    {
#if UNITY_EDITOR
        // Index to start creating new layers (8 ignores the provided layers)
        private const int ArrayStartIndex = 8;

        // TagManager asset (ProjectSettings/TagManager.asset)
        private SerializedObject tagManager;

        // Layer property in tagManager
        private SerializedProperty layersProp;

        /// <summary>
        /// Initialize TagManager asset file
        /// </summary>
        public TagManagerController()
        {
            tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            layersProp = tagManager.FindProperty("layers");
        }

        /// <summary>
        /// Tries to add a Layer to tagManager
        /// </summary>
        /// <param name="layerName">Layer name to add</param>
        /// <returns>Whether or not the layer was added</returns>
        public bool TryAddLayer(string layerName)
        {
            SerializedProperty firstEmptyLayer = null;

            tagManager.Update();

            // Check if the layerName already exists and try cache the first empty layer found
            bool found = false;
            for (int i = ArrayStartIndex; i < layersProp.arraySize; i++)
            {
                SerializedProperty layer = layersProp.GetArrayElementAtIndex(i);
                if (layer.stringValue.Equals(layerName))
                {
                    found = true;
                    break;
                }

                if (firstEmptyLayer == null && string.IsNullOrEmpty(layer.stringValue))
                    firstEmptyLayer = layer;
            }

            // if layerName was not found, add it to the first empty layer
            if (!found)
            {
                if (firstEmptyLayer != null)
                {
                    firstEmptyLayer.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                }
                else
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Removes layer from tagManager
        /// </summary>
        /// <param name="layerName">Layer name to remove</param>
        public void RemoveLayer(string layerName)
        {
            tagManager.Update();

            if (layerName == null)
                return;

            SerializedProperty layer = null;

            for (int i = 0; i < layersProp.arraySize; i++)
            {
                layer = layersProp.GetArrayElementAtIndex(i);
                if (layer.stringValue.Equals(layerName))
                    break;
            }

            if (layer == null)
                return;

            layer.stringValue = string.Empty;

            tagManager.ApplyModifiedProperties();
        }

        public void RemoveAllLayersContaining(string layerName)
        {
            for (int i = ArrayStartIndex; i < layersProp.arraySize; i++)
            {
                SerializedProperty layer = layersProp.GetArrayElementAtIndex(i);
                if (layer.stringValue.Contains(layerName))
                    layer.stringValue = string.Empty;
            }
            tagManager.ApplyModifiedProperties();
        }
#endif
    }
}
