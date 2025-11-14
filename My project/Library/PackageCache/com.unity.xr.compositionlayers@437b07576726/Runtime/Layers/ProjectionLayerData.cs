using Unity.XR.CompositionLayers.Extensions;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Layers
{
    /// <summary>
    /// Projection layer type for full screen rendering of a texture composition layer.
    /// </summary>
    [CompositionLayerData(
        Provider = "Unity",
        Name = "Projection",
        IconPath = "",
        InspectorIcon = "",
        ListViewIcon = "",
        Description = "Projection Layer",
        SuggestedExtenstionTypes = new[] { typeof(TexturesExtension) }
     )]
    public class ProjectionLayerData : LayerData { }
}
