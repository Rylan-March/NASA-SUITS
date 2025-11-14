using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using Unity.XR.CompositionLayers.Layers;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Services
{
    /// <summary>
    /// Collection of utility methods that are useful for <see cref="CompositionLayer"/>.
    /// </summary>
    public static class CompositionLayerUtils
    {
        static readonly Dictionary<string, LayerDataDescriptor> k_LayerDescriptors = new Dictionary<string, LayerDataDescriptor>();
        internal static Action<CompositionLayer, int> SetOrderValueInEditor;
        internal static UserLayerCache UserLayers;

        static readonly List<Type> k_LayerDataTypes = new List<Type>();

        static CompositionLayerUtils()
        {
            UserLayers ??= new UserLayerCache();
            k_LayerDataTypes.Clear();
            typeof(LayerData).GetAssignableTypes(k_LayerDataTypes);
            foreach (var layerDataType in k_LayerDataTypes)
            {
                var attrs = layerDataType.GetCustomAttributes(typeof(CompositionLayerDataAttribute), false);
                foreach (var attr in attrs)
                {
                    var compAttr = attr as CompositionLayerDataAttribute;
                    var hasCompAttr = compAttr != null;

                    var provider = hasCompAttr && !string.IsNullOrEmpty(compAttr.Provider) ? compAttr.Provider : layerDataType.Module.Name;
                    var name = hasCompAttr && !string.IsNullOrEmpty(compAttr.Name) ? compAttr.Name : layerDataType.Name;
                    var description = hasCompAttr && !string.IsNullOrEmpty(compAttr.Description) ? compAttr.Description : "";
                    var iconPath = hasCompAttr && !string.IsNullOrEmpty(compAttr.IconPath) ? compAttr.IconPath : "";
                    var inspectorIcon = hasCompAttr && !string.IsNullOrEmpty(compAttr.InspectorIcon) ? compAttr.InspectorIcon : "";
                    var listViewIcon = hasCompAttr && !string.IsNullOrEmpty(compAttr.ListViewIcon) ? compAttr.ListViewIcon : "";
                    var preferOverlay = !hasCompAttr || compAttr.PreferOverlay;
                    var supportTransform = hasCompAttr && compAttr.SupportTransform;
                    var suggestedExtenstionTypes = hasCompAttr ? compAttr.SuggestedExtenstionTypes : Type.EmptyTypes;

                    var ld = new LayerDataDescriptor(
                        provider,
                        name,
                        layerDataType.FullName,
                        description,
                        iconPath,
                        inspectorIcon,
                        listViewIcon,
                        preferOverlay,
                        supportTransform,
                        layerDataType,
                        suggestedExtenstionTypes);

                    if (k_LayerDescriptors.ContainsKey(ld.TypeFullName))
                        throw new ArgumentException($"Layer Data already exists with Id of {ld.TypeFullName}");

                    k_LayerDescriptors.Add(ld.TypeFullName, ld);
                }
            }
        }

        /// <summary>
        /// Gets a new copy of a list including all known <see cref="LayerDataDescriptor"/>.
        /// </summary>
        /// <returns>A list of all known <see cref="LayerDataDescriptor"/>.</returns>
        public static List<LayerDataDescriptor> GetAllLayerDescriptors()
        {
            return new List<LayerDataDescriptor>(k_LayerDescriptors.Values);
        }

        /// <summary>
        /// Gets the `LayerDataDescriptor` that matches <paramref name="typeFullName"/>.
        /// </summary>
        /// <param name="typeFullName"> Id of the corresponding <see cref="LayerDataDescriptor"/>.</param>
        /// <returns>
        /// The <see cref="LayerDataDescriptor"/> that corresponds to the <c>string</c>
        /// <paramref name="typeFullName"/> provided. If the <c>string</c> is empty or the
        /// <see cref="LayerDataDescriptor"/> is not found <see cref="LayerDataDescriptor.Empty"/> is returned.
        /// </returns>
        public static LayerDataDescriptor GetLayerDescriptor(string typeFullName)
        {
            if (string.IsNullOrEmpty(typeFullName))
                return LayerDataDescriptor.Empty;

            if (!k_LayerDescriptors.TryGetValue(typeFullName, out var layerDescriptor))
                layerDescriptor = LayerDataDescriptor.Empty;

            return layerDescriptor;
        }

        /// <summary>
        /// Gets the <see cref="LayerDataDescriptor"/> that matches the <c>Type</c> <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <c>Type</c> of the corresponding <see cref="LayerDataDescriptor"/>.</param>
        /// <returns>
        /// The <see cref="LayerDataDescriptor"/> that corresponds to the <c>Type</c> <paramref name="type"/>
        /// provided. If the <c>Type</c> is not a subclass of <see cref="LayerData"/> or the
        /// <see cref="LayerDataDescriptor"/> is not found <see cref="LayerDataDescriptor.Empty"/> is returned.
        /// </returns>
        public static LayerDataDescriptor GetLayerDescriptor(Type type)
        {
            if (type.IsSubclassOf(typeof(LayerData)) && !type.IsAbstract)
            {
                // TODO fix for no attribute
                foreach (var attr in type.GetCustomAttributes(typeof(CompositionLayerDataAttribute), false))
                {
                    var compAttr = attr as CompositionLayerDataAttribute;
                    if (compAttr == null)
                        continue;

                    return GetLayerDescriptor(type.FullName);
                }
            }

            return LayerDataDescriptor.Empty;
        }

        /// <summary>
        /// Uses <paramref name="typeFullName"/> to find the <see cref="LayerData"/> type in the corresponding
        /// <see cref="LayerDataDescriptor"/>. Then creates and returns an instance of the <see cref="LayerData"/>.
        /// </summary>
        /// <param name="typeFullName">The type's full name that corresponds to a <see cref="LayerDataDescriptor"/>
        /// that holds the <see cref="LayerData"/> type information.</param>
        /// <returns>
        /// If the <c>string</c> <paramref name="typeFullName"/> matches a <see cref="LayerDataDescriptor"/> an
        /// instance of the <see cref="LayerData"/> type stored in the <see cref="LayerDataDescriptor"/> is returned.
        /// If no matching <see cref="LayerDataDescriptor"/> is found <c>null</c> is returned.
        /// </returns>
        public static LayerData CreateLayerData(string typeFullName)
        {
            var desc = GetLayerDescriptor(typeFullName);
            if (!desc.Equals(default) || desc.Equals(LayerDataDescriptor.Empty))
            {
                // High risk crash due to code stripping of default constructors
                try
                {
                    var layerData = Activator.CreateInstance(desc.DataType) as LayerData;
                    return layerData;
                }
                catch(Exception ex) {
                    Debug.LogException(ex);
                    throw ex;
                }
            }

            return null;
        }

        /// <summary>
        /// Uses <see cref="type"/> to find the <see cref="LayerData"/> type in the corresponding
        /// <see cref="LayerDataDescriptor"/>. Then creates and returns an instance of the <see cref="LayerData"/>.
        /// </summary>
        /// <param name="type">The type of the Layer Data</param>
        /// <returns>
        /// If the <c>string</c> matches a <see cref="LayerDataDescriptor"/> an
        /// instance of the <see cref="LayerData"/> type stored in the <see cref="LayerDataDescriptor"/> is returned.
        /// If no matching <see cref="LayerDataDescriptor"/> is found <c>null</c> is returned.
        /// </returns>
        public static LayerData CreateLayerData(Type type)
        {
            return CreateLayerData(type.FullName);
        }

        /// <summary>
        /// Try to change the <paramref name="layer"/>'s order and update the order registered with the
        /// <see cref="CompositionLayerManager"/> from <paramref name="oldOrder"/> to a new order value <paramref name="newOrder"/>.
        /// </summary>
        /// <param name="layer">The <see cref="CompositionLayer"/> to change the order value of.</param>
        /// <param name="oldOrder">The old <see cref="CompositionLayer"/> order value this is usually the current
        /// <see cref="CompositionLayer.Order"/>.</param>
        /// <param name="newOrder">The new <see cref="CompositionLayer"/> order value to set.</param>
        /// <returns>
        /// Returns <c>true</c> if the <paramref name="layer"/> is set to the <paramref name="newOrder"/> or is already set to
        /// <paramref name="newOrder"/>.
        /// </returns>
        public static bool TryChangeLayerOrder(this CompositionLayer layer, int oldOrder, int newOrder)
        {

            bool inEditor = false;
#if UNITY_EDITOR
            inEditor = true;
#endif
            CompositionLayer occupiedLayer;
            if (!CompositionLayerManager.ManagerActive || !CompositionLayerManager.IsLayerSceneValid(layer))
            {
                SetLayerOrder(layer, newOrder, inEditor);
                return true;
            }

            if (oldOrder == newOrder)
            {
                if (CompositionLayerManager.Instance.OccupiedLayers.TryGetValue(newOrder, out occupiedLayer)
                    && occupiedLayer != layer)
                {
                    if (newOrder != 0)
                        LogLayerOrderCannotBeSet(layer, newOrder);

                    return false;
                }

                CleanUpExtraInstancesOfLayer(layer, newOrder);

                if (occupiedLayer == layer)
                {
                    SetLayerOrder(layer, newOrder, inEditor);
                    return true;
                }

                // Insures default layer is added to the 0 order in OccupiedLayers
                CompositionLayerManager.Instance.OccupiedLayers.Add(newOrder, layer);

                SetLayerOrder(layer, newOrder, inEditor);
                return true;
            }

            if (!layer.CanChangeOrderTo(newOrder))
                return false;

            if (CompositionLayerManager.Instance.OccupiedLayers.TryGetValue(oldOrder, out occupiedLayer) && occupiedLayer == layer)
                CompositionLayerManager.Instance.OccupiedLayers.Remove(oldOrder);

            CleanUpExtraInstancesOfLayer(layer, newOrder);

            if (CompositionLayerManager.Instance.OccupiedLayers.TryGetValue(newOrder, out occupiedLayer) && occupiedLayer == layer)
            {
                SetLayerOrder(layer, newOrder, inEditor);
                return true;
            }

            if (CompositionLayerManager.Instance.OccupiedLayers.TryAdd(newOrder, layer))
            {
                SetLayerOrder(layer, newOrder, inEditor);
                return true;
            }

            return false;
        }

        static void CleanUpExtraInstancesOfLayer(CompositionLayer layer, int newOrder)
        {
            if (CompositionLayerManager.Instance.OccupiedLayers.ContainsValue(layer))
            {
                var oldOrderValue = 0;
                var needsCleanup = false;

                foreach (var checkLayer in CompositionLayerManager.Instance.OccupiedLayers)
                {
                    if (checkLayer.Key != newOrder && checkLayer.Value == layer)
                    {
                        oldOrderValue = checkLayer.Key;
                        needsCleanup = true;
                        break;
                    }
                }

                if (needsCleanup)
                    CompositionLayerManager.Instance.OccupiedLayers.Remove(oldOrderValue);
            }
        }

        /// <summary>
        /// Tries to swap the <see cref="CompositionLayer.Order"/> values of <paramref name="lhl"/> and <paramref name="rhl"/>.
        /// </summary>
        /// <param name="lhl">Left hand <see cref="CompositionLayer"/> to swap the order of.</param>
        /// <param name="rhl">Right hand <see cref="CompositionLayer"/> to swap the order of.</param>
        /// <returns><c>true</c> if the method was able to swap the <paramref name="lhl"/> and <paramref name="rhl"/>
        /// <see cref="CompositionLayer"/>.
        ///
        /// This can fail if the layers are not in a valid scene, one of the layers is
        /// the DefaultSceneCompositionLayer, the layers are not tracked by the
        /// <see cref="CompositionLayerManager"/>, or that do not occupy the <see cref="CompositionLayer.Order"/> layer
        /// order with the <see cref="CompositionLayerManager"/>.
        /// </returns>
        public static bool TrySwapLayers(this CompositionLayer lhl, CompositionLayer rhl)
        {
            if (lhl == rhl)
                return false;

            if (!CompositionLayerManager.ManagerActive || !CompositionLayerManager.IsLayerSceneValid(lhl) || !CompositionLayerManager.IsLayerSceneValid(rhl))
                return false;

            var layerManager = CompositionLayerManager.Instance;

            if (layerManager.DefaultSceneCompositionLayer == lhl || layerManager.DefaultSceneCompositionLayer == rhl)
                return false;

            if (!layerManager.OccupiedLayers.ContainsValue(lhl) || !layerManager.OccupiedLayers.ContainsValue(rhl))
                return false;

            var lhlOrder = lhl.Order;
            var rhlOrder = rhl.Order;
            layerManager.OccupiedLayers.TryGetValue(lhlOrder, out var lho);
            layerManager.OccupiedLayers.TryGetValue(rhlOrder, out var rho);
            if ((lho != null && lho != lhl) || (rho != null && rho != rhl))
                return false;

            layerManager.OccupiedLayers.Remove(lhlOrder);
            layerManager.OccupiedLayers.Remove(rhlOrder);

            lhl.Order = rhlOrder;
            rhl.Order = lhlOrder;
            CompositionLayerManager.OccupiedLayersUpdated?.Invoke();
            return true;
        }

        static void SetLayerOrder(CompositionLayer layer, int value, bool inEditor)
        {
            if (inEditor && SetOrderValueInEditor != null)
                SetOrderValueInEditor.Invoke(layer, value);
            else
                layer.SetLayerOrderInternal(value);
        }

        /// <summary>
        /// Checks if the <see cref="CompositionLayer"/> <paramref name="layer"/> be changed to the <paramref name="order"/> value.
        /// </summary>
        /// <param name="layer">The <see cref="CompositionLayer"/> to check if can be set to the <paramref name="order"/> value.</param>
        /// <param name="order">The value to check if the <paramref name="layer"/> can be set to.</param>
        /// <returns><c>true</c> if the <paramref name="order"/> can be set the the <paramref name="layer"/>'s order value.</returns>
        public static bool CanChangeOrderTo(this CompositionLayer layer, int order)
        {
            if (!CompositionLayerManager.ManagerActive || !CompositionLayerManager.IsLayerSceneValid(layer))
                return true;

            CompositionLayer occupiedLayer;
            // Default composition layer can only ever be at order = 0
            if (order == 0)
                return layer.LayerData is DefaultLayerData;
            else
                return !CompositionLayerManager.Instance.OccupiedLayers.TryGetValue(order, out occupiedLayer) || occupiedLayer == layer;

        }

        /// <summary>
        /// Adds the components listed in the <see cref="LayerDataDescriptor.SuggestedExtensions"/> from the
        /// <see cref="LayerDataDescriptor"/> corresponding to the <paramref name="layer"/>'s GameObject.
        /// </summary>
        /// <param name="layer">The <see cref="CompositionLayer"/> add the
        /// <see cref="LayerDataDescriptor.SuggestedExtensions"/> components to.</param>
        public static void AddSuggestedExtensions(this CompositionLayer layer)
        {
            if (layer.LayerData == null)
                return;

            var desc = GetLayerDescriptor(layer.LayerData.GetType());

            if (desc.Equals(default) || desc.Equals(LayerDataDescriptor.Empty)
                || desc.SuggestedExtensions.Length == 0)
                return;

            var gameObject = layer.gameObject;
            foreach (var extension in desc.SuggestedExtensions)
            {
                if (!extension.IsSubclassOf(typeof(MonoBehaviour)))
                    continue;

                if (gameObject.GetComponent(extension) == null)
                    layer.gameObject.AddComponent(extension);
            }
        }

        internal static void LogLayerOrderCannotBeSet(CompositionLayer layer, int order)
        {
            if (CompositionLayerManager.Instance.OccupiedLayers.TryGetValue(order, out var occupiedLayer))
                Debug.Log($"Cannot set {layer.gameObject.name} to Layer Order: {order.ToString()}! " +
                    $"Layer Order is already in use by {occupiedLayer.gameObject.name}.");
        }

        internal static Camera GetStereoMainCamera()
        {
            var mainCamera = CompositionLayerManager.mainCameraCache;
            if (ValidateStereoCamera(mainCamera))
            {
                return mainCamera;
            }

            var cameras = Camera.allCameras;
            if (cameras != null)
            {
                foreach (var camera in cameras)
                {
                    if (ValidateStereoCamera(camera))
                    {
                        return camera;
                    }
                }
            }

            return mainCamera;
        }

        static bool ValidateStereoCamera(Camera camera)
        {
            return camera != null && camera.isActiveAndEnabled && camera.stereoTargetEye != StereoTargetEyeMask.None && camera.targetTexture == null;
        }
    }
}
