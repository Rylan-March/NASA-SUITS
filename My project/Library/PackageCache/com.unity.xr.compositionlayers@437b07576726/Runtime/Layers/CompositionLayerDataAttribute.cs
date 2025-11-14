using System;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Layers
{
    /// <summary>
    /// An attribute for <see cref="LayerData"/> used to populate the <see cref="LayerDataDescriptor"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class CompositionLayerDataAttribute : Attribute
    {
        /// <summary>The source that is providing the <see cref="LayerData"/> type.</summary>
        public string Provider = "";

        /// <summary>The display name of the <see cref="LayerData"/> type.</summary>
        public string Name = "";

        /// <summary>A description of what the <see cref="LayerData"/> does and how it is used.</summary>
        public string Description = "";

        /// <summary>Path to the icon folder used for the <see cref="LayerData"/>.</summary>
        public string IconPath = "";

        /// <summary>The icon used for the inspector of the <see cref="LayerData"/> object.</summary>
        public string InspectorIcon = "";

        /// <summary>The icon used in the Composition Layer Window for <see cref="CompositionLayer"/>s using this type of <see cref="LayerData"/>.</summary>
        public string ListViewIcon = "";

        /// <summary> When a new instance of this <see cref="LayerData"/> is created should it be an overlay or underlay layer. </summary>
        public bool PreferOverlay = true;

        /// <summary>This layer type supports world or camera relative transforms.</summary>
        public bool SupportTransform = false;

        /// <summary>Suggested extension types to use with the <see cref="LayerData"/> on the <see cref="CompositionLayer"/>.</summary>
        public Type[] SuggestedExtenstionTypes = Type.EmptyTypes;
    }
}
