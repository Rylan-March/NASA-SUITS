using System;

namespace Unity.XR.CompositionLayers.Layers
{
    /// <summary>
    /// Additional attributes for SerializeField in PlatformLayerData.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PlatformLayerDataFieldAttribute : Attribute
    {
        /// <summary>
        /// Contains all supported layer data types. null or Empty means all supported.
        /// </summary>
        public Type[] SupportedLayerDataTypes;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="SupportedLayerDataTypes">platformm supported layer types</param>
        public PlatformLayerDataFieldAttribute(Type[] SupportedLayerDataTypes)
        {
            this.SupportedLayerDataTypes = SupportedLayerDataTypes;
        }
    }
}
