using System;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CompositionLayers.Services;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.XR.CompositionLayers
{
    /// <summary>
    /// Base class for all composition layer types. Derive from this and extend to add
    /// your own layer type.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("XR/Composition Layers/Composition Layer")]
    [Icon(CompositionLayerConstants.IconPath + "d_LayerUniversal.png")]
    [CompositionLayersHelpURL(typeof(CompositionLayer))]
    [DefaultExecutionOrder(int.MinValue)]
    public sealed class CompositionLayer : MonoBehaviour
    {
#if UNITY_EDITOR
        readonly Type k_UIMirrorComponentType = System.Reflection.Assembly.Load("Unity.XR.CompositionLayers.UIInteraction").GetType("Unity.XR.CompositionLayers.UIInteraction.InteractableUIMirror");
#endif
        [SerializeField, HideInInspector]
        Canvas m_UICanvas;

        [SerializeField, HideInInspector]
        Component m_UIMirrorComponent;

        [SerializeField, HideInInspector]
        Component m_LayerOutline;

        [SerializeField]
        [Tooltip(@"The layer ordering of this layer in relation to the main eye layer.
            Order < 0 will render under the eye layer in ascending order.
            Order >= 0 will render over the eye layer in ascending order.")]
        int m_Order = 1;

        [SerializeReference]
        [Tooltip("The data associated with the layer type this layer is set to.")]
        internal LayerData m_LayerData;

        /// <summary>
        /// Current PlatformLayerData cache. This property isn't serialized immediately.
        /// </summary>
        [NonSerialized]
        internal PlatformLayerData m_PlatformLayerData;
        /// <summary>
        /// Serialized keys for PlatformLayerData.
        /// </summary>
        [SerializeReference]
        internal string[] m_PlatformLayerDataKeys;
        /// <summary>
        /// Serialized texts for PlatformLayerData.
        /// </summary>
        [SerializeReference]
        internal string[] m_PlatformLayerDataTexts;
        /// <summary>
        /// Serialized binaries for PlatformLayerData.
        /// </summary>
        [SerializeReference]
        internal int[] m_PlatformLayerDataBinary;

#pragma warning disable 0414
        // Using a NonSerialized field to make sure the value is default after domain reload.
        [NonSerialized]
        bool m_OrderInitialized;

        /// <summary>
        /// Provides access to the list of <see cref="CompositionLayerExtension"/> components that are currently enabled on this CompositionLayer gameObject.
        /// </summary>
        public List<CompositionLayerExtension> Extensions => m_Extensions;
        List<CompositionLayerExtension> m_Extensions = new List<CompositionLayerExtension>();

        /// <summary>
        /// Order Initialized is used to track if the <see cref="Order"/> is initialized to a valid value.
        /// A <see cref="Order"/> has been initialized when the <see cref="CompositionLayer"/> is managed with
        /// the <see cref="CompositionLayerManager"/>.
        /// </summary>
        public bool OrderInitialized => m_OrderInitialized;
#pragma warning restore 0414

        /// <summary>
        /// The layer ordering of this layer in relation to the main eye layer. Order less than 0 will render under the eye
        /// layer in ascending order. Order greater than or equal to 0 will render over the eye layer in ascending order.
        /// </summary>
        public int Order
        {
            get
            {
                return m_Order;
            }
            set
            {
                this.TryChangeLayerOrder(m_Order, value);
            }
        }

        internal void SetLayerOrderInternal(int value)
        {
            m_Order = UpdateValue(m_Order, value);
            m_OrderInitialized = true;
        }

        /// <summary>
        /// The data associated with the layer type this layer is set to.
        /// </summary>
        /// <value>ScriptableObject instance for layer data.</value>
        public LayerData LayerData
        {
            get => m_LayerData;
            internal set
            {
                m_LayerData = UpdateValue(m_LayerData, value);
                if (LayerData != null)
                    LayerData.ReportStateChange = ReportStateChange;
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        void Update()
        {
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                ReportStateChange();
            }
            if ((LayerData?.GetType() == typeof(ProjectionLayerData)) || (LayerData?.GetType() == typeof(ProjectionLayerRigData)))
                ReportStateChange();
        }

        /// <summary>
        /// Get/Desrialize PlatformLayerData.
        /// This function keeps deselized PlatformLayerData internally.
        /// </summary>
        /// <typeparam name="T">The type of PlatformLayerData changed to.</typeparam>
        /// <returns>PlatformLayerData.</returns>
        public T GetPlatformLayerData<T>() where T : PlatformLayerData
        {
            return GetPlatformLayerData(typeof(T)) as T;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Set/Serialize PlatformLayerData.
        /// </summary>
        /// <param name="platformLayerData">Target PlatformLayerData.</param>
        public void SetPlatformLayerData(PlatformLayerData platformLayerData)
        {
            m_PlatformLayerData = platformLayerData;
            SerializePlatformLayerData(platformLayerData);
        }
#endif

        /// <inheritdoc cref="MonoBehaviour"/>=
        void Awake()
        {
            // Apply the CompositionOutline component when the object is created
            // to handle drawing outlines for quad and cylinder layers
#if UNITY_EDITOR
            if (m_LayerOutline == null) m_LayerOutline = Undo.AddComponent(gameObject, typeof(CompositionOutline));
#endif

            // Setting up the instance of CompositionLayerManager can send message with creating a new game object
            // this is not allowed to be called from Awake
            if (!Application.isPlaying)
                return;

            // Deserialize platform layer data at least once.
            GetActivePlatformLayerData();
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        void OnEnable()
        {
            if (LayerData != null)
            {
                LayerData.ReportStateChange = ReportStateChange;
                if (!LayerData.Validate(this))
                    return;
            }

            InitializeLayerOrder();
            CompositionLayerManager.Instance?.CompositionLayerCreated(this);
            CompositionLayerManager.Instance?.CompositionLayerEnabled(this);
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        void OnDisable()
        {
            CompositionLayerManager.Instance?.CompositionLayerDisabled(this);
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        void OnDestroy()
        {
            CompositionLayerManager.Instance?.CompositionLayerDestroyed(this);
            m_OrderInitialized = false;
        }

        /// <summary>
        /// Initializes the <see cref="Order"/> value of the <see cref="CompositionLayer"/> with the
        /// <see cref="CompositionLayerManager"/> setting the <see cref="Order"/> to an unoccupied value.
        ///
        /// If the <see cref="Order"/> value is already occupied in the <see cref="CompositionLayerManager"/> another valid
        /// <see cref="Order"/> will be assigned.
        /// </summary>
        internal void InitializeLayerOrder()
        {
            // Since used in delay call the object could have bee destroyed
            if (!this || OrderInitialized)
                return;

            if (CompositionLayerManager.IsLayerSceneValid(this))
                CompositionLayerManager.StartCompositionLayerManager();

            if (!this.CanChangeOrderTo(Order))
            {
                var preferOverlay = LayerData == null || CompositionLayerUtils.GetLayerDescriptor(LayerData.GetType()).PreferOverlay;

                // Only use `preferOverlay` when layer is first created or order is 0
                var newOrder = Order == 0 ? CompositionLayerManager.GetFirstUnusedLayer(preferOverlay)
                    : CompositionLayerManager.GetNextUnusedLayer(Order);

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (!this.TryChangeLayerOrder(m_Order, newOrder) && !this.TryChangeLayerOrder(m_Order, m_Order))
                        this.TryChangeLayerOrder(m_Order, CompositionLayerManager.GetNextUnusedLayer(newOrder));
                }
#endif
                Order = newOrder;
            }
            else
            {
                CompositionLayerManager.Instance.OccupiedLayers.TryAdd(Order, this);
            }

            m_OrderInitialized = true;
        }

        /// <summary>
        /// Report a state change to the <see cref="CompositionLayerManager"/>
        /// </summary>
        internal void ReportStateChange()
        {
            if (isActiveAndEnabled && CompositionLayerManager.ManagerActive)
                CompositionLayerManager.Instance.CompositionLayerStateChanged(this);
        }

        /// <summary>
        /// Check if new value != old value. If it is, then report state change and return new value.
        /// Otherwise return old value
        /// </summary>
        /// <param name="oldValue">Current value to check for equality</param>
        /// <param name="newValue">The new value we want to change the old value to.</param>
        /// <typeparam name="T">Type of old and new value.</typeparam>
        /// <returns>Old value if new value is the same, otherwise the new value.</returns>
        T UpdateValue<T>(T oldValue, T newValue)
        {
            if (!(oldValue?.Equals(newValue) ?? false))
            {
                ReportStateChange();
                return newValue;
            }

            return oldValue;
        }

        /// <summary>
        /// Sets the <see cref="LayerData"/>.
        /// </summary>
        /// <param name="layer">The <see cref="LayerData"/> instance to assign.</param>
        public void ChangeLayerDataType(LayerData layer)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, Undo.GetCurrentGroupName());
#endif
            LayerData = layer;
        }

        /// <summary>
        /// Will populate the layer data with existing data associated with that <paramref name="typeFullName"/>.
        /// </summary>
        /// <param name="typeFullName">The layer Id for the <see cref="Layers.LayerData"/> type.</param>
        public void ChangeLayerDataType(string typeFullName)
        {
            ChangeLayerDataType(CompositionLayerUtils.CreateLayerData(typeFullName));
        }

        /// <summary>
        /// Sets the layer type from the <see cref="LayerDataDescriptor"/>. <see cref="LayerData"/> is populated from
        /// data associated with the <see cref="LayerDataDescriptor"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="LayerDataDescriptor"/> for a type of <see cref="Layers.LayerData"/></param>
        public void ChangeLayerDataType(LayerDataDescriptor descriptor)
        {
            ChangeLayerDataType(CompositionLayerUtils.CreateLayerData(descriptor.DataType));
        }

        /// <summary>
        /// Sets the <see cref="LayerData"/>  base on a <see cref="LayerDataDescriptor"/> subclass of type T.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="Layers.LayerData"/> to change the layer type to.</typeparam>
        public void ChangeLayerDataType<T>() where T : LayerData
        {
            ChangeLayerDataType(CompositionLayerUtils.GetLayerDescriptor(typeof(T)));
        }

        /// <summary>
        /// Sets the <see cref="LayerData"/> and base on a <see cref="LayerDataDescriptor"/> subclass defined
        /// by the passed in Type.
        /// </summary>
        /// <param name="type">The type of <see cref="Layers.LayerData"/> to change the layer type to.</param>
        public void ChangeLayerDataType(Type type)
        {
            ChangeLayerDataType(CompositionLayerUtils.GetLayerDescriptor(type));
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Gizmos.DrawIcon(transform.position, CompositionLayerConstants.IconPath + "d_LayerUniversal_Gizmo.png");
#endif
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        void OnValidate()
        {
#if UNITY_EDITOR
            if (LayerData != null)
            {
                LayerData.ReportStateChange = ReportStateChange;
            }
#endif
        }

        ///<summary>
        /// Checks for changes in children to see if the user has childed or unchilded a canvas.
        /// Allows for swapping between a normal layer and a UI layer
        /// </summary>
        /// <seealso cref="MonoBehaviour"/>=
        void OnTransformChildrenChanged()
        {
#if UNITY_EDITOR
            // Do not create a UI layer if not a Quad or Cylinder layer
            var layerDataType = LayerData?.GetType();
            if (layerDataType != typeof(QuadLayerData) && layerDataType != typeof(CylinderLayerData))
                return;

            var canvasChild = transform.GetComponentInChildren<Canvas>();
            var manuallyAddedComponent = gameObject.GetComponent(k_UIMirrorComponentType);

            // Make sure the child changed was the canvas
            if (m_UICanvas != canvasChild)
            {
                if (m_UIMirrorComponent != null)
                    Undo.DestroyObjectImmediate(m_UIMirrorComponent);

                // If there is no canvas, remove UI references
                if (canvasChild == null)
                {
                    m_UIMirrorComponent = null;
                    m_UICanvas = null;
                }
                // If there is a canvas and m_UIMirrorComponent is null, add UI references
                else if (manuallyAddedComponent == null)
                {
                    Undo.RecordObject(this, "Cache canvas child.");
                    m_UICanvas = canvasChild;
                    m_UIMirrorComponent = Undo.AddComponent(gameObject, k_UIMirrorComponentType);
                }
                // if the mirror component was added manually, we need to cache that component reference inside m_UIMirrorComponent.
                else
                {
                    m_UICanvas = canvasChild;
                    m_UIMirrorComponent = manuallyAddedComponent;
                }
            }
#endif
        }

        PlatformLayerData GetActivePlatformLayerData()
        {
            return GetPlatformLayerData(PlatformManager.ActivePlatformProvider?.PlatformLayerDataType);
        }

        PlatformLayerData GetPlatformLayerData(Type platformLayerDataType)
        {
            if (platformLayerDataType == null)
                return null;

            if (m_PlatformLayerData != null && m_PlatformLayerData.GetType() == platformLayerDataType)
                return m_PlatformLayerData;

            m_PlatformLayerData = DeserializePlatformLayerData(platformLayerDataType);
            return m_PlatformLayerData;
        }

#if UNITY_EDITOR
        static void ArrayRemoveAt<T>(ref T[] values, int index)
            where T : class
        {
            if (values == null)
                return;

            var length = values.Length;
            if (length == 0)
            {
                values = null;
                return;
            }

            if (index < length)
                values[index] = null;

            CompactArray(ref values);
        }

        internal static void CompactArray<T>(ref T[] values)
            where T : class
        {
            if (values == null)
                return;

            int length = values.Length, newLength = 0;
            for (int i = length - 1; i >= 0; --i)
            {
                if (values[i] != null)
                {
                    newLength = i + 1;
                    break;
                }
            }

            ShrinkArray(ref values, newLength);
        }

        static void ExpandArray<T>(ref T[] values, int newLength)
        {
            if (values != null && values.Length >= newLength)
                return;

            Array.Resize(ref values, newLength);
        }

        static void ShrinkArray<T>(ref T[] values, int newLength)
        {
            if (values == null || values.Length <= newLength)
                return;

            if (newLength <= 0)
                values = null;
            else
                Array.Resize(ref values, newLength);
        }

        internal static int[][] ToBinaryDataList(int[] binary)
        {
            var binaryDataList = new List<int[]>();
            if (binary != null)
            {
                for (int binaryPos = 0; binaryPos < binary.Length;)
                {
                    var binaryDataLength = binary[binaryPos];
                    var nextBinaryPos = binaryPos + 1 + binaryDataLength;
                    if (binaryDataLength < 0 || nextBinaryPos > binary.Length)
                    {
                        return binaryDataList.ToArray(); // Failsafe.(Deserialize error.)
                    }

                    var binaryData = new int[binaryDataLength];
                    if (binaryDataLength > 0)
                        Array.Copy(binary, binaryPos + 1, binaryData, 0, binaryDataLength);
                    binaryDataList.Add(binaryData);

                    binaryPos = nextBinaryPos;
                }
            }

            return binaryDataList.ToArray();
        }

        internal static int[] FromBinaryDataList(int[][] binaryDataList)
        {
            var binaryData = new List<int>();
            if (binaryDataList != null)
            {
                for (int i = 0; i < binaryDataList.Length; ++i)
                {
                    if (binaryDataList[i] != null)
                    {
                        binaryData.Add(binaryDataList[i].Length);
                        binaryData.AddRange(binaryDataList[i]);
                    }
                    else
                    {
                        binaryData.Add(0);
                    }
                }
            }

            return binaryData.ToArray();
        }

        internal void SerializePlatformLayerData(PlatformLayerData platformLayerData)
        {
            if (platformLayerData == null)
                return;

            var fullName = platformLayerData.GetType().FullName;
            int keyLength = m_PlatformLayerDataKeys != null ? m_PlatformLayerDataKeys.Length : 0;
            var binaries = ToBinaryDataList(m_PlatformLayerDataBinary);

            // Overwrite element.
            for (int i = 0; i < keyLength; ++i)
            {
                if (m_PlatformLayerDataKeys[i] == fullName)
                {
                    if (platformLayerData.IsSupportedSerializeBinary())
                    {
                        ExpandArray(ref binaries, i + 1);
                        binaries[i] = platformLayerData.SerializeBinary();
                        m_PlatformLayerDataBinary = FromBinaryDataList(binaries);
                        ArrayRemoveAt(ref m_PlatformLayerDataTexts, i);
                    }
                    else
                    {
                        ArrayRemoveAt(ref binaries, i);
                        m_PlatformLayerDataBinary = FromBinaryDataList(binaries);
                        ExpandArray(ref m_PlatformLayerDataTexts, i + 1);
                        m_PlatformLayerDataTexts[i] = platformLayerData.Serialize();
                    }
                    return;
                }
            }

            // Add new element.
            Array.Resize(ref m_PlatformLayerDataKeys, keyLength + 1);
            m_PlatformLayerDataKeys[keyLength] = fullName;

            if (platformLayerData.IsSupportedSerializeBinary())
            {
                Array.Resize(ref binaries, keyLength + 1);
                binaries[keyLength] = platformLayerData.SerializeBinary();
                ShrinkArray(ref m_PlatformLayerDataTexts, keyLength + 1);
            }
            else
            {
                ShrinkArray(ref binaries, keyLength + 1);
                Array.Resize(ref m_PlatformLayerDataTexts, keyLength + 1);
                m_PlatformLayerDataTexts[keyLength] = platformLayerData.Serialize();
            }
            m_PlatformLayerDataBinary = FromBinaryDataList(binaries);
        }
#endif
        internal PlatformLayerData DeserializePlatformLayerData(Type platformLayerDataType)
        {
            var keys = m_PlatformLayerDataKeys;
            var texts = m_PlatformLayerDataTexts;
            var binary = m_PlatformLayerDataBinary;
            if (platformLayerDataType == null || keys == null || (texts == null && binary == null))
                return null;

            var platformLayerData = Activator.CreateInstance(platformLayerDataType) as PlatformLayerData;
            if (platformLayerData == null)
                return null;

            DeserializePlatformLayerData(ref platformLayerData);
            return platformLayerData;
        }

        internal void DeserializePlatformLayerData(ref PlatformLayerData platformLayerData)
        {
            var keys = m_PlatformLayerDataKeys;
            var texts = m_PlatformLayerDataTexts;
            var binary = m_PlatformLayerDataBinary;
            if (platformLayerData == null || keys == null || (texts == null && binary == null))
                return;

            var fullName = platformLayerData.GetType().FullName;
            int keyLength = keys.Length;

            if (binary != null && platformLayerData.IsSupportedSerializeBinary())
            {
                int binaryPos = 0, binaryLength = binary.Length;
                for (int i = 0; i < keyLength & binaryPos < binaryLength; ++i)
                {
                    var binaryDataLength = binary[binaryPos];
                    var nextBinaryPos = binaryPos + 1 + binaryDataLength;
                    if (binaryDataLength < 0 || nextBinaryPos > binaryLength)
                        break; // Deserialize error.

                    if (keys[i] == fullName)
                    {
                        if (binaryDataLength > 0)
                        {
                            var binaryData = new int[binaryDataLength];
                            Array.Copy(binary, binaryPos + 1, binaryData, 0, binaryDataLength);
                            platformLayerData.DeserializeBinary(binaryData);
                        }
                        else // Failsafe.
                        {
                            platformLayerData.DeserializeBinary(null);
                        }

                        return;
                    }

                    binaryPos = nextBinaryPos;
                }
            }

            if (texts != null)
            {
                for (int i = 0; i < keyLength; ++i)
                {
                    if (keys[i] == fullName)
                    {
                        if (i < texts.Length && texts[i] != null)
                            platformLayerData.Deserialize(texts[i]);
                        else
                            platformLayerData.Deserialize(null);

                        return;
                    }
                }
            }

            if (platformLayerData.IsSupportedSerializeBinary())
                platformLayerData.DeserializeBinary(null);
            else
                platformLayerData.Deserialize(null);
        }
    }
}
