using System;
using UnityEngine;
using Unity.XR.CompositionLayers.Extensions;
using Unity.XR.CompositionLayers.Provider;

namespace Unity.XR.CompositionLayers.Layers
{
    /// <summary>
    /// Subclass of <see cref="LayerData" /> that defines a simple
    /// quad layer in a scene. A quad layer is simply a rectangular area of
    /// the display that will be rendered with some texture by the
    /// current <see cref="ILayerProvider" />. The quad should be rendered
    /// at the transform location.
    /// </summary>
    [Serializable]
    [CompositionLayerData(
        Provider = "Unity",
        Name = "Quad",
        IconPath = CompositionLayerConstants.IconPath,
        InspectorIcon = "LayerQuadColor",
        ListViewIcon = "LayerQuad",
        SupportTransform = true,
        Description = "Defines a simple quad layer in a scene. A quad layer is simply a rectangular area of the display " +
            "that will be rendered with some texture by the current ILayerProvider instance.",
        SuggestedExtenstionTypes = new[] { typeof(TexturesExtension) }
     )]
    [CompositionLayersHelpURL(typeof(QuadLayerData))]
    public class QuadLayerData : LayerData
    {
        [SerializeField]
        Vector2 m_Size = Vector2.one;

        [SerializeField]
        bool m_ApplyTransformScale = true;

        /// <summary>
        /// Return the size of quad layer - width and height.
        /// </summary>
        public Vector2 Size
        {
            get => m_Size;
            set => m_Size = UpdateValue(m_Size, value);
        }

        /// <summary>
        /// Whether or not to apply the transform scale properties to the layer.
        /// When true, the scale of the transform will be applied to the width and height respectively.
        /// </summary>
        public bool ApplyTransformScale
        {
            get => m_ApplyTransformScale;
            set => m_ApplyTransformScale = UpdateValue(m_ApplyTransformScale, value);
        }

        /// <summary>
        /// Return re-calculated parameters based on whether or not apply the transform scale properties.
        /// </summary>
        /// <param name="scale">transform scale</param>
        /// <returns>Return re-calculated quad params.</returns>
        public Vector2 GetScaledSize(Vector3 scale)
        {
            return m_ApplyTransformScale ? scale * m_Size : m_Size;
        }

        /// <summary>
        /// Used to copy values from another layer data instance
        /// </summary>
        /// <inheritdoc/>
        public override void CopyFrom(LayerData layerData)
        {
            if (layerData is QuadLayerData quadLayerData)
            {
                m_Size = quadLayerData.Size;
                m_ApplyTransformScale = quadLayerData.ApplyTransformScale;
            }
        }
    }
}
