using Unity.XR.CompositionLayers.Extensions;
using UnityEditor;
using UnityEngine;
using System;

namespace Unity.XR.CompositionLayers.Layers
{
    /// <summary>
    /// Projection rig layer type for full screen rendering of a texture composition layer.
    /// </summary>
    [CompositionLayerData(
        Provider = "Unity",
        Name = "Projection Layer Eye Rig",
        IconPath = CompositionLayerConstants.IconPath,
        InspectorIcon = "LayerEyeColor",
        ListViewIcon = "LayerEye",
        Description = "Projection Layer Eye Rig",
        SuggestedExtenstionTypes = new Type[] { }
    )]
    public class ProjectionLayerRigData : LayerData { }
}
