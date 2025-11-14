using System;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Extensions.Editor
{
    class DestinationRectsDrawer : RectsDrawer
    {
        protected override void PopulateShaderProperties(Material material, int index, RectData rectData)
        {
            base.PopulateShaderProperties(material, index, rectData);
            material.EnableKeyword("COMPOSITION_DEST");
            material.DisableKeyword("COMPOSITION_SOURCE");

            var propRect = (index == 0) ? texturesExtension.LeftEyeSourceRect : texturesExtension.RightEyeSourceRect;
            material.SetVector("_SrcTexBounds", new Vector4(propRect.x, propRect.y, propRect.width, propRect.height));
        }

        public DestinationRectsDrawer(SerializedObject serializedObject)
            : base(serializedObject, "Destination Rects", "m_LeftEyeDestinationRect", "m_RightEyeDestinationRect") { }
    }
}
