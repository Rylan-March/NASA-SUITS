using System;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Extensions.Editor
{
    class SourceRectsDrawer : RectsDrawer
    {
        protected override void PopulateShaderProperties(Material material, int index, RectData rectData)
        {
            base.PopulateShaderProperties(material, index, rectData);
            material.EnableKeyword("COMPOSITION_SOURCE");
            material.DisableKeyword("COMPOSITION_DEST");
        }

        public SourceRectsDrawer(SerializedObject serializedObject)
            : base(serializedObject, "Source Rects", "m_LeftEyeSourceRect", "m_RightEyeSourceRect") { }
    }
}
