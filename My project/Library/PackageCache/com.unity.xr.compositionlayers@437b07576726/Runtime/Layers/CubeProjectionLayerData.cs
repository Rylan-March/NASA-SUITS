using Unity.XR.CompositionLayers.Extensions;

namespace Unity.XR.CompositionLayers.Layers
{
    /// <summary>
    /// Subclass of <see cref="LayerData" /> that defines a cube layer in a scene.
    /// A cube always centered at the user's head position with only its inside faces visible.
    /// Useful for skyboxes and rendering 360 panoramic images.
    /// </summary>
    [CompositionLayerData(
        Provider = "Unity",
        Name = "Cube Projection",
        IconPath = CompositionLayerConstants.IconPath,
        InspectorIcon = "LayerCubeMap",
        ListViewIcon = "LayerCubeMap",
        PreferOverlay = false,
        SupportTransform = true,
        Description = "Cube projection layer",
        SuggestedExtenstionTypes = new[] { typeof(TexturesExtension) }
     )]
    public class CubeProjectionLayerData : LayerData
    {
    }
}
