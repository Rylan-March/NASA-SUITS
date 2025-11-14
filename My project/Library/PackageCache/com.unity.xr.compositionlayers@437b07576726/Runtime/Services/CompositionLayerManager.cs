using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Profiling;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CompositionLayers.Provider;
using Unity.XR.CoreUtils;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace Unity.XR.CompositionLayers.Services
{
    /// <summary>
    /// Singleton manger for defined composition layers and
    /// updating layer information for a given <see cref="ILayerProvider" /> instance.
    ///
    /// The expected lifecycle of a layer in relation to the manager is as follows:
    /// | Composition Layer | Manager | Reported State |
    /// | -- | -- | -- |
    /// | Awake | <see cref="CompositionLayerCreated" /> | Created |
    /// | OnEnable | <see cref="CompositionLayerEnabled" /> | Modified, Active |
    /// | OnDisable | <see cref="CompositionLayerDisabled" /> | Modified |
    /// | OnDestroy | <see cref="CompositionLayerDestroyed" /> | Removed |
    ///
    ///
    /// The manager will report the set of created, removed, modified and active layers to the
    /// <see cref="s_LayerProvider" /> instance on every Update call. These lists are
    /// defined to contain layers a follows:
    ///
    /// **Created** : Any layer that has just been created. Populated on calls to <see cref="CompositionLayerCreated" />.
    ///
    /// This list is ephemeral and is cleared after each call to the layer provider.
    ///
    /// **Removed** : Any layer that has been destroyed will cause a call to
    /// <see cref="CompositionLayerDestroyed" />. The layer will be removed from the
    /// created, active and modified lists and added to the removed list.
    ///
    /// This list is ephemeral and is cleared after each call to the layer provider.
    ///
    /// **Modified** : Any layer that has changed in some way be added to this list. A modification could
    /// be a property change, or the layer being re-activated or de-activated. A layer is
    /// only added to this list if it isn't already in the Created or Removed lists.
    ///
    /// This list is ephemeral and is cleared after each call to the layer provider.
    ///
    /// A layer will only exist in one of Removed, Created or Modified on any call to the <see cref="s_LayerProvider" />.
    ///
    /// **Active** : This list contains the current set of active layers for this update call to
    /// the <see cref="s_LayerProvider" />. Layers passed to <see cref="CompositionLayerEnabled" /> will
    /// be added to this list, and layers passed to <see cref="CompositionLayerDisabled" /> or
    /// <see cref="CompositionLayerDestroyed" /> will be removed from this list.
    /// </summary>
    public sealed class CompositionLayerManager
    {
        /// <summary>
        /// Information about composition layers registered with the manager.
        /// </summary>
        public struct LayerInfo
        {
            /// <summary>
            /// Unique manager instance id for the registered layer.
            /// </summary>
            public int Id;

            /// <summary>
            /// The actual layer instance.
            /// </summary>
            public CompositionLayer Layer;
        }

        static CompositionLayerManager s_Instance = null;

        /// <summary>
        /// Singleton instance of <see cref="CompositionLayerManager" />.
        /// </summary>
        /// <value>Singleton instance of the <see cref="CompositionLayerManager" /></value>
        public static CompositionLayerManager Instance
        {
            get
            {
                if (s_ManagerStopped)
                {
                    return null;
                }

                if (s_Instance == null || s_ComponentInstance == null)
                    StartCompositionLayerManager();

                return s_Instance;
            }
        }

        static readonly ProfilerMarker s_AwakeMarker = new ProfilerMarker("CompositionLayerManager.Awake");
        static readonly ProfilerMarker s_StartMarker = new ProfilerMarker("CompositionLayerManager.StartCompositionLayerManager");
        static readonly ProfilerMarker s_StopMarker = new ProfilerMarker("CompositionLayerManager.StopCompositionLayerManager");
        static readonly ProfilerMarker s_UpdateMarker = new ProfilerMarker("CompositionLayerManager.Update");
        static readonly ProfilerMarker s_LateUpdateMarker = new ProfilerMarker("CompositionLayerManager.LateUpdate");
        static readonly ProfilerMarker s_LayerProviderUpdateMarker = new ProfilerMarker("CompositionLayerManager.LayerProvider.Update");
        static readonly ProfilerMarker s_EmulatedLayerProviderUpdateMarker = new ProfilerMarker("CompositionLayerManager.EmulatedLayerProvider.Update");
        static readonly ProfilerMarker s_InternalLayerProviderUpdateMarker = new ProfilerMarker("CompositionLayerManager.InternalLayerProviders.Update");
        static readonly ProfilerMarker s_LayerCreatedMarker = new ProfilerMarker("CompositionLayerManager.CompositionLayerCreated");
        static readonly ProfilerMarker s_LayerEnabledMarker = new ProfilerMarker("CompositionLayerManager.CompositionLayerEnabled");
        static readonly ProfilerMarker s_LayerDestroyedMarker = new ProfilerMarker("CompositionLayerManager.CompositionLayerDestroyed");
        static readonly ProfilerMarker s_LayerDisabledMarker = new ProfilerMarker("CompositionLayerManager.CompositionLayerDisabled");
        static readonly ProfilerMarker s_LayerStateChangededMarker = new ProfilerMarker("CompositionLayerManager.CompositionLayerStateChanged");

        /// <summary>
        /// Main camera cache accessor
        /// </summary>
        public static Camera mainCameraCache
        {
            get
            {
                if (_mainCameraCache == null)
                    _mainCameraCache = Camera.main;

                return _mainCameraCache;
            }
        }

        static Camera _mainCameraCache;
        static ILayerProvider s_LayerProvider;
        static ILayerProvider s_EmulationLayerProvider;
        static List<ILayerProvider> s_InternalLayerProviders;
        static CallbackComponent s_ComponentInstance;
        static CompositionLayer s_DefaultSceneCompositionLayer;

        static bool s_ManagerStopped;

        internal static Action OccupiedLayersUpdated;
        internal static Action ManagerStarted;
        internal static Action ManagerStopped;
        //d
        /// <summary>
        /// The <see cref="CompositionLayer"/> that is being used to render the default scene layer in a composition.
        /// </summary>
        public CompositionLayer DefaultSceneCompositionLayer
        {
            get
            {
                return s_DefaultSceneCompositionLayer;
            }

            internal set
            {
                if (value == null || value.LayerData is not DefaultLayerData)
                    return;

                if (s_DefaultSceneCompositionLayer == null)
                {
                    s_DefaultSceneCompositionLayer = value;
                }
                else if (!s_DefaultSceneCompositionLayer.gameObject.activeSelf)
                {
                    s_DefaultSceneCompositionLayer.Order = GetFirstUnusedLayer();
                    value.Order = 0;
                    s_DefaultSceneCompositionLayer = value;
                }
            }
        }

        readonly Dictionary<CompositionLayer, LayerInfo> m_KnownLayers = new Dictionary<CompositionLayer, LayerInfo>();
        readonly List<LayerInfo> m_CreatedLayers = new List<LayerInfo>();
        readonly List<int> m_RemovedLayers = new List<int>();
        readonly List<LayerInfo> m_ModifiedLayers = new List<LayerInfo>();
        readonly List<LayerInfo> m_ActiveLayers = new List<LayerInfo>();
        readonly List<LayerInfo> m_EmptyList = new List<LayerInfo>();
        internal readonly Dictionary<int, CompositionLayer> OccupiedLayers = new Dictionary<int, CompositionLayer>();
        internal bool OccupiedLayersDirty;
        ProjectionRigOffsetSynchronizer m_ProjectionRigOffsetSynchronizer;

        /// <summary>
        /// The currently assigned <see cref="ILayerProvider" /> instance that this manager
        /// instance should talk to.
        /// </summary>
        public ILayerProvider LayerProvider
        {
            get => s_LayerProvider;
            set
            {
                if (s_LayerProvider != value)
                {
                    s_LayerProvider?.CleanupState();

                    s_LayerProvider = value;

                    s_LayerProvider?.SetInitialState(m_KnownLayers.Values.ToList());
                }
            }
        }

        /// <summary>
        /// The currently assigned <see cref="ILayerProvider" /> emulation provider instance that this manager
        /// instance should talk to.
        /// </summary>
        public ILayerProvider EmulationLayerProvider
        {
            get => s_EmulationLayerProvider;
            set
            {

                if (s_EmulationLayerProvider != value)
                {
                    s_EmulationLayerProvider?.CleanupState();

                    s_EmulationLayerProvider = value;

                    s_EmulationLayerProvider?.SetInitialState(m_KnownLayers.Values.ToList());
                }
            }
        }

        internal void AddInternalLayerProvider(ILayerProvider layerProvider)
        {
            if (s_InternalLayerProviders == null)
                s_InternalLayerProviders = new List<ILayerProvider>();

            if (layerProvider != null && !s_InternalLayerProviders.Contains(layerProvider))
            {
                s_InternalLayerProviders.Add(layerProvider);
                layerProvider.SetInitialState(m_KnownLayers.Values.ToList());
            }
        }

        internal void RemoveInternalLayerProvider(ILayerProvider layerProvider)
        {
            if(s_InternalLayerProviders == null)
                return;

            if (layerProvider != null && s_InternalLayerProviders.Contains(layerProvider))
            {
                s_InternalLayerProviders.Remove(layerProvider);
                layerProvider.CleanupState();
            }
        }

        /// <summary>
        /// Can be used for other scripts to easily find existing composition layers.
        /// </summary>
        public IReadOnlyCollection<CompositionLayer> CompositionLayers => Instance?.m_KnownLayers.Keys;

        internal static bool ManagerActive => s_Instance != null;

        internal static void StartCompositionLayerManager()
        {
            s_StartMarker.Begin();
            // Ensures manager can be started from external script without
            // needing to directly call the 'instance' from that script.s
            if (s_Instance == null)
            {
                s_ManagerStopped = false;
                s_Instance = new CompositionLayerManager();
                ManagerStarted?.Invoke();
            }

            s_StartMarker.End();
        }

        internal static void StopCompositionLayerManager()
        {
            s_StopMarker.Begin();

            if (s_ManagerStopped)
            {
                s_StopMarker.End();
                return;
            }

            s_ManagerStopped = true;

            if (s_ComponentInstance != null)
            {
                s_ComponentInstance.OnAwake = null;
                s_ComponentInstance.OnUpdate = null;
                s_ComponentInstance.OnLateUpdate = null;
                UnityObjectUtils.Destroy(s_ComponentInstance.gameObject);
                s_ComponentInstance = null;
            }

            if (s_DefaultSceneCompositionLayer != null)
            {
                if (s_DefaultSceneCompositionLayer.gameObject != null)
                {
                    if (s_DefaultSceneCompositionLayer.gameObject.activeSelf)
                        UnityObjectUtils.Destroy(s_DefaultSceneCompositionLayer.gameObject);
                }
                s_DefaultSceneCompositionLayer = null;
            }

            if (s_Instance != null)
            {
                s_Instance.UpdateProviders(s_Instance.m_EmptyList, s_Instance.m_RemovedLayers, s_Instance.m_EmptyList, s_Instance.m_EmptyList);
                s_Instance.ClearAllState();
                s_Instance.m_ProjectionRigOffsetSynchronizer = null;
                s_Instance = null;
            }

            ManagerStopped?.Invoke();
            s_StopMarker.End();
        }

        internal void EnsureSceneCompositionManager()
        {
            if (s_ComponentInstance != null)
                return;

            s_Instance.m_ProjectionRigOffsetSynchronizer = new ProjectionRigOffsetSynchronizer();
            //ddd
            var sceneGameObject = new GameObject(CompositionLayerConstants.SceneManagerName);
            sceneGameObject.hideFlags = HideFlags.HideAndDontSave;

            sceneGameObject.SetActive(false);

            s_ComponentInstance = sceneGameObject.AddComponent<CallbackComponent>();
            // Using assignments since send message cannot be called in awake.
            s_ComponentInstance.OnAwake = Awake;
            s_ComponentInstance.OnUpdate = Update;
            s_ComponentInstance.OnLateUpdate = LateUpdate;

            sceneGameObject.SetActive(true);
        }

        internal void EnsureFallbackSceneCompositionLayer()
        {
            if (OccupiedLayers.Count == 0 || OccupiedLayers.ContainsKey(0))
                return;

            if (DefaultSceneCompositionLayer != null)
            {
                CompositionLayerCreated(DefaultSceneCompositionLayer);
                return;
            }

            var sceneGameObject = new GameObject(CompositionLayerConstants.DefaultSceneLayerName);
            sceneGameObject.hideFlags = HideFlags.HideAndDontSave;
            sceneGameObject.SetActive(false);
            var layerData = CompositionLayerUtils.CreateLayerData(typeof(DefaultLayerData).FullName);
            var compLayer = sceneGameObject.AddComponent<CompositionLayer>();
            compLayer.LayerData = layerData;
            compLayer.Order = 0;
            DefaultSceneCompositionLayer = compLayer;
            sceneGameObject.SetActive(true);
        }

        internal void ClearAllState()
        {
            m_KnownLayers.Clear();
            m_CreatedLayers.Clear();
            m_RemovedLayers.Clear();
            m_ModifiedLayers.Clear();
            m_ActiveLayers.Clear();
            OccupiedLayers.Clear();
        }

        internal void ClearSingleShotState()
        {
            m_CreatedLayers.Clear();
            m_RemovedLayers.Clear();
            m_ModifiedLayers.Clear();
        }

        static int LayerSorter(LayerInfo lhs, LayerInfo rhs)
        {
            if (lhs.Layer.Order == rhs.Layer.Order)
                return lhs.Id.CompareTo(rhs.Id);

            if (lhs.Layer.Order < rhs.Layer.Order)
                return -1;

            return 1;
        }

        /// <summary>
        /// Called to report that a new instance of a <see cref="CompositionLayer" /> is
        /// created. By default this is called from calls to Awake on a
        /// <see cref="CompositionLayer" /> instance.
        /// </summary>
        /// <param name="layer">New layer to add to management.</param>
        public void CompositionLayerCreated(CompositionLayer layer)
        {
            EnsureSceneCompositionManager();

            s_LayerCreatedMarker.Begin();

            var layerKnown = m_KnownLayers.ContainsKey(layer);
            if (!OccupiedLayers.TryGetValue(layer.Order, out var occupiedLayer))
            {
                if(OccupiedLayers.TryAdd(layer.Order, layer))
                    OccupiedLayersUpdated?.Invoke();
            }
            else if (occupiedLayer != layer)
            {
                var orderInitializeAndNotDefault = layer.OrderInitialized; //&& layer.LayerData is not DefaultLayerData;
                if (orderInitializeAndNotDefault)
                {
                    CompositionLayerUtils.LogLayerOrderCannotBeSet(layer, layer.Order);
                }
                var order = GetNextUnusedLayer(layer.Order);
                layer.Order = order;
                if (orderInitializeAndNotDefault)
                    Debug.Log($"{layer.gameObject.name} is set to next available Layer Order: {order.ToString()}.");
            }

            if (layerKnown)
            {
                s_LayerCreatedMarker.End();
                return;
            }

            var li = new LayerInfo() { Layer = layer, Id = layer.GetInstanceID() };
            m_KnownLayers.Add(layer, li);
            m_CreatedLayers.Add(li);

            EnsureFallbackSceneCompositionLayer();

            s_LayerCreatedMarker.End();
        }

        //d
        /// <summary>
        /// Called to report that an instance of a <see cref="CompositionLayer" /> is
        /// active and ready to be rendered. By default this is called from calls to
        /// OnEnable on a <see cref="CompositionLayer" /> instance.
        ///
        /// When called with a layer that is not currently active, the layer will be added
        /// to the active list as well as the added list.
        ///
        /// If the manager doesn't know about this layer (i.e. the layer instance was
        /// not passed to <see cref="CompositionLayerCreated" /> previously) then the
        /// layer is ignored.
        /// </summary>
        /// <param name="layer">Currently managed layer to set to active.</param>
        public void CompositionLayerEnabled(CompositionLayer layer)
        {//dd
            s_LayerEnabledMarker.Begin();

            if (!m_KnownLayers.ContainsKey(layer))
                CompositionLayerCreated(layer);

            var li = m_KnownLayers[layer];

            if (!m_CreatedLayers.Contains(li) && !m_ModifiedLayers.Contains(li))
                m_ModifiedLayers.Add(li);

            if (!m_ActiveLayers.Contains(li))
                m_ActiveLayers.Add(li);

            if (!OccupiedLayers.TryGetValue(layer.Order, out _))
            {
                OccupiedLayers.Add(layer.Order, layer);
                OccupiedLayersUpdated?.Invoke();
            }

            s_LayerEnabledMarker.End();
        }

        /// <summary>
        /// Called to report that an instance of a <see cref="CompositionLayer" /> is
        /// not active and should not be rendered. By default this is called from calls to
        /// OnDisable on a <see cref="CompositionLayer" /> instance.
        ///
        /// When called with a layer that is active, the layer will be removed from the
        /// active list.
        ///
        /// If the manager doesn't know about this layer (i.e. the layer instance was
        /// not passed to <see cref="CompositionLayerCreated" /> previously) then the
        /// layer is ignored.
        /// </summary>
        /// <param name="layer">Currently managed layer to set to disabled.</param>
        public void CompositionLayerDisabled(CompositionLayer layer)
        {
            s_LayerDisabledMarker.Begin();

            if (!m_KnownLayers.ContainsKey(layer))
            {
                s_LayerDisabledMarker.End();
                return;
            }

            var li = m_KnownLayers[layer];
            if (m_ActiveLayers.Contains(li))
                m_ActiveLayers.Remove(li);

            if (!m_ModifiedLayers.Contains(li))
                m_ModifiedLayers.Add(li);

            s_LayerDisabledMarker.End();
        }

        /// <summary>
        /// Called to report that an instance of a <see cref="CompositionLayer" /> is
        /// being destroyed and or should be removed from management. By default this
        /// is called from calls to OnDestroy on a <see cref="CompositionLayer" /> instance.
        ///
        /// When called the layer will be added to the removed layer list. If layer is
        /// currently active, the layer will be removed from the active list as well.
        ///
        /// If the manager doesn't know about this layer (i.e. the layer instance was
        /// not passed to <see cref="CompositionLayerCreated" /> previously) then the
        /// layer is ignored.
        /// </summary>
        /// <param name="layer">Currently managed layer to remove from management.</param>
        public void CompositionLayerDestroyed(CompositionLayer layer)
        {
            s_LayerDestroyedMarker.Begin();

            if (m_KnownLayers.ContainsKey(layer))
            {
                var li = m_KnownLayers[layer];
                m_KnownLayers.Remove(layer);
                if (m_CreatedLayers.Contains(li))
                    m_CreatedLayers.Remove(li);
                if (m_ActiveLayers.Contains(li))
                    m_ActiveLayers.Remove(li);
                if (m_ModifiedLayers.Contains(li))
                    m_ModifiedLayers.Remove(li);
                m_RemovedLayers.Add(li.Id);

                if (layer.LayerData?.GetType() == typeof(ProjectionLayerRigData))
                    m_ProjectionRigOffsetSynchronizer.RemoveProjectionRig(layer.GetInstanceID());
            }

            if (OccupiedLayers.TryGetValue(layer.Order, out var occupiedLayer) && occupiedLayer == layer)
            {
                OccupiedLayers.Remove(layer.Order);
                OccupiedLayersUpdated?.Invoke();
            }

            if (IsActiveLayersDestroyed && !AnyLayerExistsInScene())
            {
                StopCompositionLayerManager();
            }

            s_LayerDestroyedMarker.End();
        }

        /// <summary>
        /// Report a change in state/data for a layer. This could be direct layer state
        /// changes or it could be due to changes in data on extension components for this layer.
        /// </summary>
        /// <param name="layer">The <see cref="CompositionLayer"/> that is modified.</param>
        public void CompositionLayerStateChanged(CompositionLayer layer)
        {
            s_LayerStateChangededMarker.Begin();

            if (layer == null || !m_KnownLayers.ContainsKey(layer))
            {
                s_LayerStateChangededMarker.End();
                return;
            }

            var li = m_KnownLayers[layer];

            if (!m_CreatedLayers.Contains(li) && !m_RemovedLayers.Contains(li.Id) && !m_ModifiedLayers.Contains(li))
            {
                m_ModifiedLayers.Add(li);

                if (layer.LayerData?.GetType() == typeof(ProjectionLayerRigData))
                    m_ProjectionRigOffsetSynchronizer.AddProjectionRig(layer);
                else
                    // remove layer that may have previously been a projection rig layer.
                    m_ProjectionRigOffsetSynchronizer.RemoveProjectionRig(layer.GetInstanceID());
            }

            s_LayerStateChangededMarker.End();
        }

        internal void FindAllLayersInScene()
        {
            var isPlaying = Application.isPlaying;
            if (!isPlaying)
                ClearAllState();
            else
                OccupiedLayers.Clear();

            var foundLayers = UnityEngine.Object.FindObjectsByType<CompositionLayer>(isPlaying ? FindObjectsInactive.Exclude : FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var layer in foundLayers)
            {
                if (!IsLayerSceneValid(layer))
                    continue;

                if (layer.LayerData is DefaultLayerData)
                {
                    if (layer.gameObject.hideFlags == HideFlags.HideInHierarchy)
                        layer.gameObject.hideFlags = HideFlags.HideAndDontSave;

                    DefaultSceneCompositionLayer = layer;

                    break;
                }
            }

            foreach (var layer in foundLayers)
            {
                if (!IsLayerSceneValid(layer))
                    continue;

                if (!layer.OrderInitialized)
                    layer.InitializeLayerOrder();

                CompositionLayerCreated(layer);
                if (layer.enabled && layer.gameObject.activeInHierarchy)
                    CompositionLayerEnabled(layer);
            }

            OccupiedLayersUpdated?.Invoke();
        }

        void UpdateProviders(List<LayerInfo> createdLayers, List<int> removedLayers, List<LayerInfo> modifiedLayers, List<LayerInfo> activeLayers)
        {
            if (s_LayerProvider != null)
            {
                s_LayerProviderUpdateMarker.Begin();
                m_ActiveLayers.Sort(LayerSorter);
                s_LayerProvider.UpdateLayers(createdLayers, removedLayers, modifiedLayers, activeLayers);
                s_LayerProviderUpdateMarker.End();
            }

            if (s_EmulationLayerProvider != null)
            {
                s_EmulatedLayerProviderUpdateMarker.Begin();
                m_ActiveLayers.Sort(LayerSorter);
                s_EmulationLayerProvider.UpdateLayers(createdLayers, removedLayers, modifiedLayers,
                    m_ActiveLayers);
                s_EmulatedLayerProviderUpdateMarker.End();
            }

            if (s_InternalLayerProviders != null)
            {
                s_InternalLayerProviderUpdateMarker.Begin();
                foreach (var layerProvider in s_InternalLayerProviders)
                {
                    layerProvider?.UpdateLayers(createdLayers, removedLayers, modifiedLayers, activeLayers);
                }
                s_InternalLayerProviderUpdateMarker.End();
            }

        }

        void Awake()
        {
            s_AwakeMarker.Begin();
            ClearAllState();
            FindAllLayersInScene();
            s_AwakeMarker.End();
        }

        internal void Update()
        {
            s_UpdateMarker.Begin();

            // This is to for when deactivated game objects are deleted in the editor
            if (OccupiedLayersDirty)
            {
#if UNITY_EDITOR
                FindAllLayersInScene();
#endif
                OccupiedLayersDirty = false;
            }

            UpdateProviders(m_CreatedLayers, m_RemovedLayers, m_ModifiedLayers, m_ActiveLayers);
            EnsureFallbackSceneCompositionLayer();

            if (IsActiveLayersDestroyed && !AnyLayerExistsInScene())
            {
                StopCompositionLayerManager();
            }

            ClearSingleShotState();

            s_UpdateMarker.End();
        }

        internal void LateUpdate()
        {
            s_LateUpdateMarker.Begin();
            s_LayerProvider?.LateUpdate();
            s_EmulationLayerProvider?.LateUpdate();
            if(s_InternalLayerProviders != null)
            {
                foreach (var layerProvider in s_InternalLayerProviders)
                {
                    layerProvider?.LateUpdate(); // Note: Undo/Redo cause null referencing in Editor.
                }
            }
            m_ProjectionRigOffsetSynchronizer?.SyncRigsWithMainCameraParentOffsets();
            s_LateUpdateMarker.End();
        }

        internal static void GetOccupiedLayers(List<CompositionLayer> layers)
        {
            layers.Clear();

            if (ManagerActive)
                layers.AddRange(Instance.OccupiedLayers.Values);
        }

        internal static bool IsLayerSceneValid(CompositionLayer layer)
        {
            if (!layer.gameObject.scene.IsValid())
            {
                return false;
            }

#if UNITY_EDITOR
            // Check if the layer is being created in the active scene
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                if (layer.gameObject.scene == SceneManager.GetSceneAt(i))
                    return true;
            }

            // Do not manage layers in prefab isolation stage
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.scene.IsValid() && stage.scene == layer.gameObject.scene)
                return false;
#endif

            return true;
        }

        /// <summary>
        /// Get the first unoccupied <see cref="CompositionLayer.Order"/> value in the currently open scenes.
        /// </summary>
        /// <param name="overlay">
        /// If <c>true</c> the first unoccupied order value greater than 0 is returned. If <c>false</c> the first
        /// unoccupied order value less than 0 is returned.
        /// </param>
        /// <returns>Returns the first unoccupied order value order value.</returns>
        public static int GetFirstUnusedLayer(bool overlay)
        {
            return GetNextUnusedLayer(0, overlay);
        }

        /// <summary>
        /// Get the first unoccupied layer <see cref="CompositionLayer.Order"/> value that is greater than 0 in the currently
        /// open scenes.
        /// </summary>
        /// <returns>Returns the first unoccupied order value order value.</returns>
        public static int GetFirstUnusedLayer()
        {
            return GetNextUnusedLayer(0, true);
        }

        /// <summary>
        /// Gets the next unoccupied layer <see cref="CompositionLayer.Order"/> value that is greater than <paramref name="order"/>
        /// if the value is positive or less than if the value is negative.
        /// </summary>
        /// <param name="order">The order value to get the next unoccupied value for.</param>
        /// <returns>
        /// The next unoccupied layer <see cref="CompositionLayer.Order"/> value that is greater than <paramref name="order"/>
        /// if the value is positive or less than if the value is negative.
        /// </returns>
        public static int GetNextUnusedLayer(int order)
        {
            return GetNextUnusedLayer(order, order > -1);
        }

        /// <summary>
        /// Gets the next unoccupied layer <see cref="CompositionLayer.Order"/> value that is greater than or less than
        /// <paramref name="order"/>.
        /// </summary>
        /// <param name="order">The order value to get the next unoccupied value for.</param>
        /// <param name="overlay">
        /// If <c>true</c> the first unoccupied order value greater than <paramref name="order"/> is returned.
        /// If <c>false</c> the first unoccupied order value less than <paramref name="order"/> is returned.
        /// </param>
        /// <returns>
        /// The next unoccupied layer <see cref="CompositionLayer.Order"/> value that is greater than <paramref name="order"/>
        /// if the value is positive or less than if the value is negative.
        /// </returns>
        public static int GetNextUnusedLayer(int order, bool overlay)
        {
            if (order == 0)
                order = overlay ? 1 : -1;

            if (!ManagerActive)
                return order;

            while (Instance.OccupiedLayers.ContainsKey(order) || order == 0)
            {
                if (overlay)
                    order++;
                else
                    order--;
            }

            return order;
        }

        bool AnyLayerExistsInScene()
        {
            if (s_Instance == null)
                return false;

            foreach (var layer in s_Instance.m_KnownLayers.Keys)
            {
                if (layer == null || layer.gameObject == null || layer.gameObject.scene == null)
                    continue;

                if (layer.gameObject.scene.IsValid())
                    return true;
            }
            return false;
        }

        bool IsActiveLayersDestroyed
        {
            get
            {
                var isEmpty = m_ActiveLayers.Count == 0;
                if (isEmpty)
                    return true;

                if (m_ActiveLayers.Count == 1)
                {
                    var lastLayer = m_ActiveLayers.First().Layer;
                    return lastLayer.LayerData is DefaultLayerData && lastLayer.gameObject.hideFlags.HasFlag(HideFlags.HideAndDontSave);
                }
                return false;
            }
        }

        // This class syncs the transforms of the projection rig composition layers to be at the same total offset of the main camera's parents.
        private class ProjectionRigOffsetSynchronizer
        {
            private Dictionary<int, Transform> projectionRigs = new Dictionary<int, Transform>();
            private Transform mainCameraTransform;

            public ProjectionRigOffsetSynchronizer()
            {
                mainCameraTransform = mainCameraCache?.transform;
            }

            public void AddProjectionRig(CompositionLayer projectionRig)
            {
                // Early out if this rig's transform has already been added.
                if (projectionRigs.ContainsKey(projectionRig.GetInstanceID()))
                    return;

                // Add the rig to the dictionary and immediately sync it's transform with the main camera's parents.
                var rigTransform = projectionRig.transform;
                projectionRigs[projectionRig.GetInstanceID()] = rigTransform;
                var totalParentOffset = GetTotalLocalPoseOffsetOfMainCameraParents();
                rigTransform.SetWorldPose(totalParentOffset);
            }

            public void RemoveProjectionRig(int rigId)
            {
                projectionRigs.Remove(rigId);
            }

            public void SyncRigsWithMainCameraParentOffsets(bool forceSync = false)
            {
                // Early out if none of the main camera's parents have had their transforms changed.
                if (!forceSync && !ParentsHaveChanged())
                    return;

                // Sync all rig transforms with the main camera's parents.
                var totalParentOffset = GetTotalLocalPoseOffsetOfMainCameraParents();
                foreach (Transform projectionRig in projectionRigs.Values)
                {
                    projectionRig.SetWorldPose(totalParentOffset);
                }
            }

            bool ParentsHaveChanged()
            {
                if (mainCameraTransform == null)
                    mainCameraTransform = mainCameraCache?.transform;

                bool parentsHaveChanged = false;
                var currentParent = mainCameraTransform?.parent;

                // Loop through all of the main camera's parents and report if any have changed.
                while (currentParent != null)
                {
                    if (!parentsHaveChanged)
                        parentsHaveChanged = currentParent.hasChanged;

                    // Must reset hasChanged to false.
                    currentParent.hasChanged = false;
                    currentParent = currentParent.parent;
                }

                return parentsHaveChanged;
            }

            Pose GetTotalLocalPoseOffsetOfMainCameraParents()
            {
                if (mainCameraTransform == null)
                    mainCameraTransform = mainCameraCache?.transform;

                var totalLocalPoseOffset = Pose.identity;
                Transform currentParent = mainCameraTransform?.parent;

                // Loop through all of the main camera's parents and keep a running total of their local poses.
                while (currentParent != null)
                {
                    var parentLocalPose = currentParent.GetLocalPose();
                    totalLocalPoseOffset = new Pose(totalLocalPoseOffset.position + parentLocalPose.position, totalLocalPoseOffset.rotation * parentLocalPose.rotation);
                    currentParent = currentParent.parent;
                }

                return totalLocalPoseOffset;
            }
        }
    }
}
