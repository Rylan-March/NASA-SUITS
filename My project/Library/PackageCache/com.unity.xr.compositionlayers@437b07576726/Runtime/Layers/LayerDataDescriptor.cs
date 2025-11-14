using System;

namespace Unity.XR.CompositionLayers.Layers
{
    /// <summary>
    /// Defines the meta data that is associated with a <see cref="LayerData"/> type.
    /// </summary>
    public readonly struct LayerDataDescriptor : IEquatable<LayerDataDescriptor>
    {
        /// <summary>The source that is providing the <see cref="LayerData"/> type.</summary>
        public readonly string Provider;

        /// <summary>The display name of the <see cref="LayerData"/> type.</summary>
        public readonly string Name;

        /// <summary>The full type name used for identifying this <see cref="LayerData"/> type.</summary>
        public readonly string TypeFullName;

        /// <summary>A description of what the <see cref="LayerData"/> does and how it is used.</summary>
        public readonly string Description;

        /// <summary>Path to the icon folder used for the <see cref="LayerData"/>.</summary>
        public readonly string IconPath;

        /// <summary>The icon used for the inspector of the <see cref="LayerData"/> object.</summary>
        public readonly string InspectorIcon;

        /// <summary>The icon used in the Composition Layer Window for <see cref="CompositionLayer"/>s using this type of <see cref="LayerData"/>.</summary>
        public readonly string ListViewIcon;

        /// <summary> Should new instance of the <see cref="LayerData"/> be an overlay or underlay. </summary>
        public readonly bool PreferOverlay;

        /// <summary>This layer type supports world or camera relative transforms.</summary>
        public readonly bool SupportTransform;

        /// <summary>The <see cref="Type"/> of the <see cref="LayerData"/>.</summary>
        public readonly Type DataType;

        /// <summary>Suggested extension types to use with the <see cref="LayerData"/> on the <see cref="CompositionLayer"/>.</summary>
        public readonly Type[] SuggestedExtensions;

        static readonly LayerDataDescriptor k_Empty = new("", "", "", "", "", "", "", true, false, null, new Type[] { });

        /// <summary>
        /// <see cref="LayerDataDescriptor"/> with all empty or null values
        /// </summary>
        public static LayerDataDescriptor Empty => k_Empty;

        /// <summary>
        /// Creates a new <see cref="LayerDataDescriptor"/>
        /// </summary>
        /// <param name="provider">The source that is providing the <see cref="LayerData"/> type.</param>
        /// <param name="name">The display name of the <see cref="LayerData"/> type.</param>
        /// <param name="typeFullName">The unique class Id key used for finding the <see cref="LayerData"/> type.</param>
        /// <param name="description">A description of what the <see cref="LayerData"/> does and how it is used.</param>
        /// <param name="iconPath">The icon used for the inspector of the <see cref="LayerData"/> object.</param>
        /// <param name="inspectorIcon">The icon used for the inspector of the <see cref="LayerData"/> object.</param>
        /// <param name="listViewIcon">The icon used in the Composition Layer Window for <see cref="CompositionLayer"/>s using this type of <see cref="LayerData"/>.</param>
        /// <param name="preferOverlay">Should new instance of the <see cref="LayerData"/> be an overlay or underlay.</param>
        /// <param name="supportTransform">This layer type supports world or camera relative transforms.</param>
        /// <param name="dataType">The <see cref="Type"/> of the <see cref="LayerData"/>.</param>
        /// <param name="suggestedExtensions">Suggested extension types to use with the <see cref="LayerData"/> on the <see cref="CompositionLayer"/>.</param>
        public LayerDataDescriptor(string provider, string name, string typeFullName, string description, string iconPath,
                                   string inspectorIcon, string listViewIcon, bool preferOverlay, bool supportTransform, Type dataType,
                                   Type[] suggestedExtensions)
        {
            Provider = provider;
            Name = name;
            TypeFullName = typeFullName;
            Description = description;
            IconPath = iconPath;
            InspectorIcon = inspectorIcon;
            ListViewIcon = listViewIcon;
            PreferOverlay = preferOverlay;
            SupportTransform = supportTransform;
            DataType = dataType;
            SuggestedExtensions = suggestedExtensions;
        }

        /// <summary>
        /// Determines whether the specified <see cref="LayerDataDescriptor"/> is equal to the current <see cref="LayerDataDescriptor"/>.
        /// </summary>
        /// <param name="other">The <see cref="LayerDataDescriptor"/> to compare with the current <see cref="LayerDataDescriptor"/>.</param>
        /// <returns><see langword="true"/> if the specified <see cref="LayerDataDescriptor"/> is equal to the current <see cref="LayerDataDescriptor"/>. Otherwise, <see langword="false"/>.</returns>
        public bool Equals(LayerDataDescriptor other)
        {
            return Provider == other.Provider && Name == other.Name && TypeFullName == other.TypeFullName
                && Description == other.Description && IconPath == other.IconPath
                && InspectorIcon == other.InspectorIcon && ListViewIcon == other.ListViewIcon
                && PreferOverlay == other.PreferOverlay && SupportTransform == other.SupportTransform && DataType == other.DataType
                && Equals(SuggestedExtensions, other.SuggestedExtensions);
        }

        /// <summary>
        /// Determines whether this instance <paramref name="obj"/> is equal to the current <see cref="LayerDataDescriptor"/>.
        /// </summary>
        /// <param name="obj">The object for comparison.</param>
        /// <returns><see langword="true"/> if the specified object is equal to the current <see cref="LayerDataDescriptor"/>. Otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is LayerDataDescriptor other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="LayerDataDescriptor"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="LayerDataDescriptor"/>.</returns>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Provider);
            hashCode.Add(Name);
            hashCode.Add(TypeFullName);
            hashCode.Add(Description);
            hashCode.Add(IconPath);
            hashCode.Add(InspectorIcon);
            hashCode.Add(ListViewIcon);
            hashCode.Add(PreferOverlay);
            hashCode.Add(SupportTransform);
            hashCode.Add(DataType);
            hashCode.Add(SuggestedExtensions);
            return hashCode.ToHashCode();
        }

        /// <summary>
        /// Determines whether two specified <see cref="LayerDataDescriptor"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="LayerDataDescriptor"/> to compare.</param>
        /// <param name="right">The second <see cref="LayerDataDescriptor"/> to compare.</param>
        /// <returns><see langword="true"/> if the two <see cref="LayerDataDescriptor"/> instances are equal. Otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(LayerDataDescriptor left, LayerDataDescriptor right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two specified <see cref="LayerDataDescriptor"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="LayerDataDescriptor"/> to compare.</param>
        /// <param name="right">The second <see cref="LayerDataDescriptor"/> to compare.</param>
        /// <returns><see langword="true"/> if the two <see cref="LayerDataDescriptor"/> instances are not equal. Otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(LayerDataDescriptor left, LayerDataDescriptor right)
        {
            return !left.Equals(right);
        }
    }
}
