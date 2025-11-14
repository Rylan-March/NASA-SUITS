using System;
using System.Collections.Generic;
using Unity.XR.CompositionLayers.Provider;
using Unity.XR.CompositionLayers.Services;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.XR.CompositionLayers.Emulation;

namespace Unity.XR.CompositionLayers.Rendering
{
    internal class MirrorViewLayerProvider : ILayerProvider
    {
        static MirrorViewLayerProvider s_Instance;

        Dictionary<int, EmulatedCompositionLayer> m_AllCompositionLayers = new();

        List<EmulatedCompositionLayer> m_SortedLayers = new();

        internal static MirrorViewLayerProvider Instance
        {
            get
            {
                if (s_Instance == null)
                    ConnectMirrorViewLayerProvider();

                return s_Instance;
            }
        }

        internal static void ConnectMirrorViewLayerProvider()
        {
            if (s_Instance != null)
                return;

            CompositionLayerManager.ManagerStarted += OnCompositionLayerManagerStarted;
            CompositionLayerManager.ManagerStopped += OnCompositionLayerManagerStopped;

            if (CompositionLayerManager.ManagerActive)
                OnCompositionLayerManagerStarted();
        }

        internal static void DisconnectMirrorViewLayerProvider()
        {
            if (s_Instance == null)
                return;

            CompositionLayerManager.ManagerStarted -= OnCompositionLayerManagerStarted;
            CompositionLayerManager.ManagerStopped -= OnCompositionLayerManagerStopped;

            if (s_Instance != null)
                OnCompositionLayerManagerStopped();
        }

        static void OnCompositionLayerManagerStarted()
        {
            if (s_Instance == null)
                s_Instance = new MirrorViewLayerProvider();

            CompositionLayerManager.Instance?.AddInternalLayerProvider(s_Instance);
        }

        static void OnCompositionLayerManagerStopped()
        {
            if (s_Instance == null)
                return;

            s_Instance.CleanupState();

            CompositionLayerManager.Instance?.RemoveInternalLayerProvider(s_Instance);
            s_Instance = null;
        }

        EmulatedCompositionLayer CreateEmulationLayerObject(CompositionLayerManager.LayerInfo layerInfo)
        {
            var layer = layerInfo.Layer;
            if (layer == null)
                return null;

            var emulatedLayer = new EmulatedCompositionLayer();
            emulatedLayer.CompositionLayer = layer;

            if (layer.LayerData != null)
            {
                var emulatedLayerDataType = EmulatedCompositionLayerUtils.GetEmulatedLayerDataType(layer.LayerData.GetType());

                if (emulatedLayerDataType != null)
                    emulatedLayer.ChangeEmulatedLayerDataType(emulatedLayerDataType);
            }

            emulatedLayer.ModifyLayer();
            emulatedLayer.UpdateLayer();
            if (!m_AllCompositionLayers.ContainsKey(layerInfo.Id))
                m_AllCompositionLayers.Add(layerInfo.Id, emulatedLayer);
            else
                m_AllCompositionLayers[layerInfo.Id] = emulatedLayer;

            return emulatedLayer;
        }

        public void SetInitialState(List<CompositionLayerManager.LayerInfo> layers)
        {
            m_AllCompositionLayers.Clear();
            AddCreatedLayers(layers);
        }

        public void CleanupState()
        {
            foreach (var compositionLayer in m_AllCompositionLayers)
            {
                compositionLayer.Value?.Dispose();
            }

            m_AllCompositionLayers.Clear();
        }

        public void UpdateLayers(List<CompositionLayerManager.LayerInfo> createdLayers, List<int> removedLayers,
            List<CompositionLayerManager.LayerInfo> modifiedLayers, List<CompositionLayerManager.LayerInfo> activeLayers)
        {
            AddCreatedLayers(createdLayers);
            RemoveDestroyedLayers(removedLayers);
            ModifyChangedLayers(modifiedLayers);
            UpdateActiveStateOnLayers(activeLayers);
        }

        void ModifyChangedLayers(List<CompositionLayerManager.LayerInfo> modifiedLayers)
        {
            if (modifiedLayers.Count == 0)
                return;

            foreach (var layerInfo in modifiedLayers)
            {
                if (!m_AllCompositionLayers.TryGetValue(layerInfo.Id, out var emulatedLayer))
                    emulatedLayer = CreateEmulationLayerObject(layerInfo);

                emulatedLayer?.ModifyLayer();
            }
        }

        void UpdateActiveStateOnLayers(List<CompositionLayerManager.LayerInfo> activeLayers)
        {
            if (activeLayers.Count == 0)
                return;

            foreach (var layerInfo in activeLayers)
            {
                if (!m_AllCompositionLayers.TryGetValue(layerInfo.Id, out var emulatedLayer))
                {
                    emulatedLayer = CreateEmulationLayerObject(layerInfo);
                    emulatedLayer?.ModifyLayer();
                }

                if (emulatedLayer != null)
                {
                    if (emulatedLayer.EmulatedLayerData == null)
                        emulatedLayer.ModifyLayer();

                    emulatedLayer.UpdateLayer();
                }
            }
        }

        void RemoveDestroyedLayers(List<int> removedLayers)
        {
            if (removedLayers.Count == 0)
                return;

            foreach (var layerId in removedLayers)
            {
                if (m_AllCompositionLayers.TryGetValue(layerId, out var emulatedCompositionLayer))
                    emulatedCompositionLayer?.Dispose();

                m_AllCompositionLayers.Remove(layerId);
            }
        }

        void AddCreatedLayers(List<CompositionLayerManager.LayerInfo> createdLayers)
        {
            if (createdLayers.Count == 0)
                return;

            foreach (var layerInfo in createdLayers)
            {
                if (!m_AllCompositionLayers.ContainsKey(layerInfo.Id))
                    CreateEmulationLayerObject(layerInfo);
            }
        }

        public void LateUpdate() { }

        void UpdateSortedLayers()
        {
            if (!CompositionLayerManager.ManagerActive)
                return;

            m_SortedLayers.Clear();

            foreach (var compositionLayerSet in m_AllCompositionLayers)
            {
                if (compositionLayerSet.Value == null || !compositionLayerSet.Value.Enabled)
                    continue;

                var emulatedLayerData = compositionLayerSet.Value.EmulatedLayerData;
                if (emulatedLayerData != null)
                {
                    if (!emulatedLayerData.IsInitialized)
                        continue;

                    m_SortedLayers.Add(compositionLayerSet.Value);
                }
            }

            if (m_SortedLayers.Count == 0)
                return;

            m_SortedLayers.Sort(EmulatedLayerDataSorter);
        }

        internal enum LayerOrderType
        {
            Underlay,
            Overlay,
        }

        internal void AddToCommandBuffer(CommandBuffer commandBuffer, LayerOrderType layerOrderType)
        {
            if (!CompositionLayerManager.ManagerActive)
                return;

            UpdateSortedLayers();

            var commandArgs = new EmulatedLayerData.CommandArgs();
            foreach (var commandBufferLayer in m_SortedLayers)
            {
                if ((commandBufferLayer.Order < 0 && layerOrderType == LayerOrderType.Underlay) ||
                    (commandBufferLayer.Order >= 0 && layerOrderType == LayerOrderType.Overlay))
                {
                    commandBufferLayer.EmulatedLayerData.AddToCommandBuffer(commandBuffer, commandArgs);
                }
            }
        }

        static int EmulatedLayerDataSorter(EmulatedCompositionLayer lhs, EmulatedCompositionLayer rhs)
        {
            return lhs.Order.CompareTo(rhs.Order);
        }
    }
}
