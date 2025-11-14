using System;

namespace Unity.XR.CompositionLayers.Emulation
{
    /// <summary>
    /// Attribute used to designate what <see cref="UnityEngine.XR.CompositionLayers.Layers.LayerData"/> the
    /// <see cref="EmulatedLayerData"/> is used to emulate.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class EmulatedLayerDataTypeAttribute : Attribute
    {
        /// <summary>
        ///  The type of the <see cref="UnityEngine.XR.CompositionLayers.Layers.LayerData"/> this <see cref="EmulatedLayerData"/> is used to emulate.
        /// </summary>
        public Type LayerDataType;

        /// <summary>
        /// Attribute used to designate what <see cref="UnityEngine.XR.CompositionLayers.Layers.LayerData"/> the
        /// <see cref="EmulatedLayerData"/> is used to emulate.
        /// </summary>
        /// <param name="layerDataType">Type of <see cref="EmulatedLayerData"/></param>
        public EmulatedLayerDataTypeAttribute(Type layerDataType)
        {
            this.LayerDataType = layerDataType;
        }
    }
}
