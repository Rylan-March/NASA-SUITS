using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.CompositionLayers;

namespace Unity.XR.CompositionLayers.Emulation
{
    /// <summary>
    /// Base class for all emulated composition layers. You must derive from this
    /// to provide support for your own custom emulated layers.
    /// </summary>
    internal class EmulatedCompositionLayer : IDisposable
    {
        struct CameraCommandBufferData
        {
            internal CommandBuffer CommandBuffer;
            internal CameraEvent[] CameraEvents;

            internal CameraCommandBufferData(CommandBuffer commandBuffer, CameraEvent[] cameraEvents)
            {
                CommandBuffer = commandBuffer;
                CameraEvents = cameraEvents;
            }
        }

        static readonly Type k_EmulatedLayerDataType = typeof(EmulatedLayerData);

        [NonSerialized]
        int m_Order;

        [NonSerialized]
        bool m_Enabled;

        [NonSerialized]
        CompositionLayer m_CompositionLayer;

        [NonSerialized]
        EmulatedLayerData m_EmulatedLayerData;

        Dictionary<Camera, CameraCommandBufferData> m_LayerCommandBuffers = new();
        List<CompositionLayerExtension> m_LayerExtensions = new();

        /// <summary>
        /// The <see cref="CompositionLayers.CompositionLayer"/> that this instance will be emulating.
        /// </summary>
        public CompositionLayer CompositionLayer
        {
            get => m_CompositionLayer;
            internal set => m_CompositionLayer = value;
        }

        /// <summary>
        /// The <see cref="Emulation.EmulatedLayerData"/> for the <see cref="Unity.XR.CompositionLayers.Layers.LayerData"/>
        /// type assigned to the <see cref="CompositionLayers.CompositionLayer"/> that this instance will be emulating.
        /// </summary>
        public EmulatedLayerData EmulatedLayerData
        {
            get => m_EmulatedLayerData;
            internal set => m_EmulatedLayerData = value;
        }

        internal List<CompositionLayerExtension> LayerExtensions
        {
            get => m_LayerExtensions;
        }

        /// <summary>
        /// The Order value of the <see cref="CompositionLayer"/> this instance is emulating.
        /// </summary>
        public int Order => CompositionLayer == null ? int.MinValue : CompositionLayer.Order;

        internal bool Enabled => CompositionLayer != null && CompositionLayer.isActiveAndEnabled;

        public void ModifyLayer()
        {
            if (CompositionLayer == null)
                return;
            var emulatedLayerDataType = EmulatedCompositionLayerUtils.GetEmulatedLayerDataType(CompositionLayer.LayerData?.GetType());
            if (CompositionLayer == null || CompositionLayer.LayerData == null || emulatedLayerDataType == null)
            {
                EmulatedLayerData?.Dispose();
                EmulatedLayerData = null;
                return;
            }

            m_LayerExtensions.Clear();
            CompositionLayer.GetComponents(m_LayerExtensions);

            if (EmulatedLayerData == null || EmulatedLayerData.GetType() != emulatedLayerDataType)
            {
                ChangeEmulatedLayerDataType(emulatedLayerDataType);
            }
        }

        /// <summary>
        /// Used to update the state of this emulated layer provider. Override to do whatever
        /// is needed to update state, create components, etc. as required to support your layer
        /// emulation.
        /// </summary>
        public void UpdateLayer()
        {
            if (CompositionLayer.LayerData == null)
                return;

            EmulatedLayerData?.UpdateEmulatedLayerData();
        }

        internal void ChangeEmulatedLayerDataType(Type emulatedLayerDataType)
        {
            if (emulatedLayerDataType != k_EmulatedLayerDataType && !emulatedLayerDataType.IsSubclassOf(k_EmulatedLayerDataType))
            {
                throw new Exception($"{emulatedLayerDataType} is not of type {k_EmulatedLayerDataType}"!);
            }

            EmulatedLayerData?.Dispose();
            EmulatedLayerData = Activator.CreateInstance(emulatedLayerDataType) as EmulatedLayerData;
            EmulatedLayerData.InitializeLayerData(this);
            EmulatedLayerData.UpdateEmulatedLayerData();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            EmulatedLayerData?.Dispose();
            CompositionLayer = null;
            EmulatedLayerData = null;
        }

        internal virtual void AddCommandBuffer(List<Camera> cameras)
        {
            foreach (var camera in cameras)
            {
                if (EmulatedLayerData.IsSupported(camera))
                {
                    var commandArgs = new EmulatedLayerData.CommandArgs(camera);
                    var commandBuffer = EmulatedLayerData.UpdateCommandBuffer(commandArgs);
                    var cameraEvents = Order >= 0 ?
                        EmulatedLayerProvider.GetOverlayCameraEvents(camera) :
                        EmulatedLayerProvider.GetUnderlayCameraEvents(camera);

                    foreach (var cameraEvent in cameraEvents)
                    {
                        camera.AddCommandBuffer(cameraEvent, commandBuffer);
                    }

                    m_LayerCommandBuffers.Add(camera, new CameraCommandBufferData(commandBuffer, cameraEvents));
                }
                else
                {
                    EmulatedLayerProvider.WarnUnsupportedEmulation(camera, CompositionLayer);
                }
            }
        }

        internal virtual void RemoveCommandBuffer(List<Camera> cameras)
        {
            foreach (var camera in cameras)
            {
                if (camera != null && m_LayerCommandBuffers.TryGetValue(camera, out var cameraCommandBufferData)) // camera will be null when camera has been destroyed.
                {
                    foreach (var cameraEvent in cameraCommandBufferData.CameraEvents)
                    {
                        camera.RemoveCommandBuffer(cameraEvent, cameraCommandBufferData.CommandBuffer);
                    }
                }
            }

            m_LayerCommandBuffers.Clear();
        }
    }
}
