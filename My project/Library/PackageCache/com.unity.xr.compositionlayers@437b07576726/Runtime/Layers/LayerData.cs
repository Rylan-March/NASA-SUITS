using System;
using System.Linq;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Layers
{
    /// <summary>
    /// Blend type - Most platforms support Alpha and Premultiply.
    /// </summary>
    public enum BlendType
    {
        /// <summary>
        /// Alpha blending.
        /// </summary>
        Alpha,
        /// <summary>
        /// Premultiplied alpha blending.
        /// </summary>
        Premultiply,
        /// <summary>
        /// Additive Blending.
        /// </summary>
        Additive,
    }

    /// <summary>
    /// Base class for all layer data objects. Every layer needs to be defined by one data object
    /// even if it's just a simple subclass of <see cref="LayerData" />.
    /// </summary>
    [Serializable]
    public class LayerData
    {
        /// <summary>
        /// Blend type. If target platform doesn't support this blend type, fallback to supported blend type.
        /// Most platforms support Alpha & Premultiply.
        /// </summary>
        [SerializeField]
        [Tooltip("Blend type. If target platform doesn't support this blend type, fallback to supported blend type. Most platforms support Alpha & Premultiply.")]
        BlendType m_BlendType = BlendType.Alpha;

        /// <summary>
        /// Blend type without fallback. Some blend types aren't supported on specific platforms.
        /// </summary>
        public BlendType BlendTypeDirectly { get => m_BlendType; }

        /// <summary>
        /// Blend type. This value will be fixed with PlatformProvider.SupportedBlendTypes.
        /// </summary>
        public BlendType BlendType
        {
            get
            {
                var supportedBlendTypes = Services.PlatformManager.ActivePlatformProvider.SupportedBlendTypes;
                if (supportedBlendTypes != null && supportedBlendTypes.Length > 0 && !supportedBlendTypes.Contains(m_BlendType))
                {
                    return supportedBlendTypes[0]; // Fallback to supported 1st blend type.
                }

                return m_BlendType;
            }

            set => m_BlendType = UpdateValue(m_BlendType, value);
        }

        /// <summary>
        /// Report state change for the <see cref="CompositionLayer"/> associated with this <see cref="LayerData"/>.
        /// </summary>
        protected internal Action ReportStateChange;

        /// <summary>
        /// Check if new value != old value. If it is, then report state change and return new value.
        /// Otherwise return old value
        /// </summary>
        /// <param name="oldValue">Current value to check for equality</param>
        /// <param name="newValue">The new value we want to change the old value to.</param>
        /// <typeparam name="T">Type of old and new value.</typeparam>
        /// <returns>Old value if new value is the same, otherwise the new value.</returns>
        protected T UpdateValue<T>(T oldValue, T newValue)
        {
            if (!(oldValue?.Equals(newValue) ?? false))
            {
                ReportStateChange?.Invoke();
                return newValue;
            }
            return oldValue;
        }

        /// <summary>
        /// Allows a layer data class to validate values from it's associated composition layer.
        /// </summary>
        /// <param name="layer">Compositon layer to validate against this layer data.</param>
        /// <returns>True if validation passes, false if validation fails.</returns>
        protected internal virtual bool Validate(CompositionLayer layer) => true;

        /// <summary>
        /// Used to copy values from another layer data instance
        /// </summary>
        /// <param name="layerData">Layer data to copy from</param>
        public virtual void CopyFrom(LayerData layerData) { }
    }
}
