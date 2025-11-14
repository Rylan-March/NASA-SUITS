using System;
using System.Collections.Generic;
using Unity.XR.CompositionLayers.Provider;
using Unity.XR.CompositionLayers.Services;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Profiling;
using Unity.XR.CompositionLayers.Rendering;
using Unity.XR.CompositionLayers.Emulation.Implementations;
#if UNITY_RENDER_PIPELINES_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.XR.CompositionLayers.Emulation
{
    class EmulatedLayerProvider : ILayerProvider
    {
        static readonly ProfilerMarker s_EmulatedLayerProviderCreate = new ProfilerMarker("EmulatedLayerProvider.Create");
        static readonly ProfilerMarker s_EmulatedLayerProviderRemove = new ProfilerMarker("EmulatedLayerProvider.Remove");
        static readonly ProfilerMarker s_EmulatedLayerProviderModify = new ProfilerMarker("EmulatedLayerProvider.Modify");
        static readonly ProfilerMarker s_EmulatedLayerProviderActive = new ProfilerMarker("EmulatedLayerProvider.Active");
        static readonly ProfilerMarker s_EmulatedLayerProviderSetupRenderPipeline = new ProfilerMarker("EmulatedLayerProvider.SetupRenderPipeline");

        static readonly CameraEvent[] k_DefaultUnderlayCameraEvents = { CameraEvent.BeforeForwardOpaque };
        static readonly CameraEvent[] k_DefaultOverlayCameraEvents = { CameraEvent.AfterImageEffects };
        static readonly CameraEvent[] k_DeferredUnderlayCameraEvents = { CameraEvent.BeforeGBuffer };
        static readonly CameraEvent[] k_DeferredOverlayCameraEvents = { CameraEvent.AfterImageEffects };

        static EmulatedLayerProvider s_Instance;
        static bool s_WarnUnsupportedEmulation;

        Dictionary<int, EmulatedCompositionLayer> m_AllCompositionLayers = new();

        List<EmulatedCompositionLayer> m_SortedLayers = new();

        List<Camera> m_ActiveCameras = new();

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        internal static void ConnectEmulatedLayerProvider()
        {
            CompositionLayerManager.ManagerStarted += OnCompositionLayerManagerStarted;
            CompositionLayerManager.ManagerStopped += OnCompositionLayerManagerStopped;
            s_WarnUnsupportedEmulation = true;

            if (CompositionLayerManager.ManagerActive)
                OnCompositionLayerManagerStarted();
        }

        internal static void DisconnectEmulatedLayerProvider()
        {
            CompositionLayerManager.ManagerStarted -= OnCompositionLayerManagerStarted;
            CompositionLayerManager.ManagerStopped -= OnCompositionLayerManagerStopped;

            if (s_Instance != null)
                OnCompositionLayerManagerStopped();
        }

        static void OnCompositionLayerManagerStarted()
        {
            if (s_Instance == null)
                s_Instance = new EmulatedLayerProvider();

            if (!CompositionLayerManager.ManagerActive)
                return;

            CompositionLayerManager.Instance.EmulationLayerProvider = s_Instance;

#if UNITY_RENDER_PIPELINES_UNIVERSAL
            EmulationLayerUniversalScriptableRendererPass.RegistScriptableRendererPass();
            EmulationColorScaleBiasPass.RegistScriptableRendererPass();
#endif
#if UNITY_RENDER_PIPELINES_HDRENDER
            EmulationLayerHighDefinitionVolumeManager.ActivateCustomPassVolumes();
#endif
        }

        static void OnCompositionLayerManagerStopped()
        {
            if (s_Instance == null)
                return;

#if UNITY_RENDER_PIPELINES_UNIVERSAL
            EmulationLayerUniversalScriptableRendererPass.UnregistScriptableRendererPass();
            EmulationColorScaleBiasPass.UnregistScriptableRendererPass();
#endif
#if UNITY_RENDER_PIPELINES_HDRENDER
            EmulationLayerHighDefinitionVolumeManager.DeactivateCustomPassVolumes();
#endif

            s_Instance.CleanupState();

            if (!CompositionLayerManager.ManagerActive)
                return;

            if (CompositionLayerManager.Instance.EmulationLayerProvider == s_Instance)
                CompositionLayerManager.Instance.EmulationLayerProvider = null;
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
            TearDownRenderPipelineCommandBuffers();

            foreach (var compositionLayer in m_AllCompositionLayers)
            {
                compositionLayer.Value?.Dispose();
            }

            m_AllCompositionLayers.Clear();
        }

        public void UpdateLayers(List<CompositionLayerManager.LayerInfo> createdLayers, List<int> removedLayers,
            List<CompositionLayerManager.LayerInfo> modifiedLayers, List<CompositionLayerManager.LayerInfo> activeLayers)
        {
            TearDownRenderPipelineCommandBuffers();

            AddCreatedLayers(createdLayers);
            RemoveDestroyedLayers(removedLayers);
            ModifyChangedLayers(modifiedLayers);
            UpdateActiveStateOnLayers(activeLayers);

            SetupRenderPipelineCommandBuffers();
        }

        void ModifyChangedLayers(List<CompositionLayerManager.LayerInfo> modifiedLayers)
        {
            if (modifiedLayers.Count == 0)
                return;

            foreach (var layerInfo in modifiedLayers)
            {
                s_EmulatedLayerProviderModify.Begin();

                if (!m_AllCompositionLayers.TryGetValue(layerInfo.Id, out var emulatedLayer))
                    emulatedLayer = CreateEmulationLayerObject(layerInfo);

                emulatedLayer?.ModifyLayer();

                s_EmulatedLayerProviderModify.End();
            }
        }

        void UpdateActiveStateOnLayers(List<CompositionLayerManager.LayerInfo> activeLayers)
        {
            if (activeLayers.Count == 0)
                return;


            foreach (var layerInfo in activeLayers)
            {
                s_EmulatedLayerProviderActive.Begin();

                if (!m_AllCompositionLayers.TryGetValue(layerInfo.Id, out var emulatedLayer))
                {
                    emulatedLayer = CreateEmulationLayerObject(layerInfo);
                    emulatedLayer?.ModifyLayer();
                }

                if (emulatedLayer != null)
                {
                    // Undo/Redo can cause the CompositionLayer reference to be lost
                    if (emulatedLayer.CompositionLayer == null)
                    {
                        emulatedLayer.Dispose();
                        emulatedLayer = CreateEmulationLayerObject(layerInfo);
                        emulatedLayer?.ModifyLayer();
                    }

                    if (emulatedLayer == null)
                    {
                        s_EmulatedLayerProviderActive.End();
                        return;
                    }

                    if (emulatedLayer.EmulatedLayerData == null)
                        emulatedLayer.ModifyLayer();

                    emulatedLayer.UpdateLayer();
                }

                s_EmulatedLayerProviderActive.End();
            }
        }

        void RemoveDestroyedLayers(List<int> removedLayers)
        {
            if (removedLayers.Count == 0)
                return;

            foreach (var layerId in removedLayers)
            {
                s_EmulatedLayerProviderRemove.Begin();

                if (m_AllCompositionLayers.TryGetValue(layerId, out var emulatedCompositionLayer))
                    emulatedCompositionLayer?.Dispose();

                m_AllCompositionLayers.Remove(layerId);

                s_EmulatedLayerProviderRemove.End();
            }
        }

        void AddCreatedLayers(List<CompositionLayerManager.LayerInfo> createdLayers)
        {
            if (createdLayers.Count == 0)
                return;

            foreach (var layerInfo in createdLayers)
            {
                s_EmulatedLayerProviderCreate.Begin();

                if (!m_AllCompositionLayers.ContainsKey(layerInfo.Id))
                    CreateEmulationLayerObject(layerInfo);

                s_EmulatedLayerProviderCreate.End();
            }
        }

        public void LateUpdate() { }

        internal static CameraEvent[] GetUnderlayCameraEvents(Camera camera)
        {
            return camera.actualRenderingPath != RenderingPath.DeferredShading ?
                k_DefaultUnderlayCameraEvents : k_DeferredUnderlayCameraEvents;
        }

        internal static CameraEvent[] GetOverlayCameraEvents(Camera camera)
        {
            return camera.actualRenderingPath != RenderingPath.DeferredShading ?
                k_DefaultOverlayCameraEvents : k_DeferredOverlayCameraEvents;
        }

        internal static bool IsSupported(Camera camera)
        {
            if(s_Instance == null)
                return false;

            return s_Instance.m_ActiveCameras.Contains(camera);
        }

        void SetupRenderPipelineCommandBuffers()
        {
            if (!CompositionLayerManager.ManagerActive)
                return;

            s_EmulatedLayerProviderSetupRenderPipeline.Begin();

            UpdateActiveCamerasAndSortedLayers();

            if (GraphicsSettings.currentRenderPipeline != null)
            {
#if UNITY_RENDER_PIPELINES_UNIVERSAL || UNITY_RENDER_PIPELINES_HDRENDER
                bool isSupported = false;
#if UNITY_RENDER_PIPELINES_UNIVERSAL
                isSupported |= RenderPipelineUtility.IsUniversalRenderPipeline();
#endif
#if UNITY_RENDER_PIPELINES_HDRENDER
                isSupported |= RenderPipelineUtility.IsHDRenderPipeline();
#endif
                if (isSupported)
                {
                    foreach (var commandBufferLayer in m_SortedLayers)
                    {
                        EmulationLayerScriptableRendererManager.Add(commandBufferLayer.EmulatedLayerData, commandBufferLayer.Order);
                    }

                    s_EmulatedLayerProviderSetupRenderPipeline.End();
                    return;
                }
#endif
            }

            // for Built-in Render Pipeline.
            foreach (var commandBufferLayer in m_SortedLayers)
            {
                commandBufferLayer.AddCommandBuffer(m_ActiveCameras);
            }

            s_EmulatedLayerProviderSetupRenderPipeline.End();
        }

        void TearDownRenderPipelineCommandBuffers()
        {
#if UNITY_RENDER_PIPELINES_HDRENDER || UNITY_RENDER_PIPELINES_UNIVERSAL
            EmulationLayerScriptableRendererManager.Clear();
#endif
            // for Built-in Render Pipeline.
            foreach (var commandBufferLayer in m_SortedLayers)
            {
                commandBufferLayer.RemoveCommandBuffer(m_ActiveCameras);
            }
        }

        void AddEmulationToActiveCamera()
        {
            var mainCamera = CompositionLayerManager.mainCameraCache;
            if (mainCamera != null)
                m_ActiveCameras.Add(mainCamera);
        }

        void UpdateActiveCamerasAndSortedLayers()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (!CompositionLayerManager.ManagerActive)
                return;

            m_ActiveCameras.Clear();

            if (!Application.isPlaying)
            {
               AddEmulationToActiveCamera();
            }
#if UNITY_EDITOR
            else if (EmulatedCompositionLayerUtils.EmulationInPlayMode)
            {
                AddEmulationToActiveCamera();
            }

            if (EmulatedCompositionLayerUtils.EmulationInScene)
            {
                foreach (var sceneViewObject in SceneView.sceneViews)
                {
                    if (sceneViewObject is SceneView sceneView)
                        m_ActiveCameras.Add(sceneView.camera);
                }
            }
#elif UNITY_STANDALONE
            if (EmulatedCompositionLayerUtils.EmulationInStandalone)
            {
                AddEmulationToActiveCamera();
            }
#endif
            m_SortedLayers.Clear();

            // Gather command buffer based layers
            foreach (var compositionLayerSet in m_AllCompositionLayers)
            {
                if (compositionLayerSet.Value == null || !compositionLayerSet.Value.Enabled)
                    continue;

                var emulatedRenderLayerData = compositionLayerSet.Value.EmulatedLayerData;
                if (emulatedRenderLayerData != null)
                {
                    if (!emulatedRenderLayerData.IsInitialized)
                        continue;

                    m_SortedLayers.Add(compositionLayerSet.Value);
                }
            }

            if (m_SortedLayers.Count == 0)
                return;

            // Sort emulated render layers
            m_SortedLayers.Sort(EmulatedLayerDataSorter);
#endif
        }

        static int EmulatedLayerDataSorter(EmulatedCompositionLayer lhs, EmulatedCompositionLayer rhs)
        {
            return lhs.Order.CompareTo(rhs.Order);
        }

        internal static void WarnUnsupportedEmulation(Camera camera, CompositionLayer layer)
        {
            if (s_WarnUnsupportedEmulation)
            {
                Debug.LogWarning($"Emulation of composition layers in Game View is not supported when XR device is connected and active.");
                s_WarnUnsupportedEmulation = false;
            }
        }
    }
}
