using System;
using UnityEngine;

namespace Unity.XR.CompositionLayers
{
    /// <summary>
    /// Abstract class used to define a composition layer extension. A CompositionLayerExtension
    /// is a component that can be added to a game object that is already given an instance of
    /// a <see cref="CompositionLayer"/> as a means of adding additional data or usability for
    /// that given layer type.
    ///
    /// By default this requires that the game object have a <see cref="CompositionLayer" /> instance
    /// of some implementation type on it.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(CompositionLayer))]
    public abstract class CompositionLayerExtension : MonoBehaviour
    {
        /// <summary>
        /// Options for which type of object this extension should be associated with.
        /// </summary>
        public enum ExtensionTarget
        {
            /// <summary>
            /// Used for extensions that extend layer instances.
            /// </summary>
            Layer,

            /// <summary>
            /// Used for extensions that extend swapchain instances.
            /// </summary>
            Swapchain,

            /// <summary>
            /// Used for any future extensions that extend instances not currently listed.
            /// </summary>
            Custom
        }

        /// <summary>
        /// Implementations must specify which type of object this extension should be associated with.
        /// </summary>
        public abstract ExtensionTarget Target { get; }

        ///<summary>
        /// Implementations must return a pointer to this extension's native struct.
        /// </summary>
        /// <returns>the pointer to extension's native struct.</returns>
        /// <remarks>
        /// This method is called by the OpenXRLayerUtility GetExtensionsChain function when
        /// it initializes an object's <c>Next</c> pointer struct chain member. Layer handlers can
        /// use this chain to access native extension properties.
        /// </remarks>
        public abstract unsafe void* GetNativeStructPtr();

        /// <summary>
        /// Report state change for the <see cref="CompositionLayer"/> associated with this <see cref="CompositionLayerExtension"/>.
        /// </summary>
        protected internal Action ReportStateChange;

        private CompositionLayer CompositionLayer
        {
            get
            {
                if (m_compositionLayer == null)
                    m_compositionLayer = gameObject.GetComponent<CompositionLayer>();
                return m_compositionLayer;
            }
        }

        private CompositionLayer m_compositionLayer;

        /// <inheritdoc cref="MonoBehaviour"/>
        public virtual void Awake()
        {
            m_compositionLayer = gameObject.GetComponent<CompositionLayer>();
            if (!m_compositionLayer)
            {
                Debug.LogWarning("Adding a composition extension to a Game Object with no composition layer instance.");
            }
            else
            {
                ReportStateChange = m_compositionLayer.ReportStateChange;
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        public virtual void OnDestroy()
        {
            ReportStateChange?.Invoke();
            ReportStateChange = null;
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        public virtual void OnEnable()
        {
            if (!CompositionLayer.Extensions.Contains(this))
            {
                CompositionLayer.Extensions.Add(this);
            }
            ReportStateChange?.Invoke();
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        public virtual void OnDisable()
        {
            if (CompositionLayer != null)
                CompositionLayer.Extensions?.Remove(this);
            ReportStateChange?.Invoke();
        }

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
    }
}
