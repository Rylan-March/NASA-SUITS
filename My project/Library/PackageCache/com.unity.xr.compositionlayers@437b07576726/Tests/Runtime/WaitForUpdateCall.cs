using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CompositionLayers.Provider;
using Unity.XR.CompositionLayers.Services;
using LayerInfo = Unity.XR.CompositionLayers.Services.CompositionLayerManager.LayerInfo;

namespace Unity.XR.CompositionLayers.Tests
{
    sealed class WaitForUpdateCall : CustomYieldInstruction, ILayerProvider
    {
        bool m_UpdateCalled;
        bool m_WaitingComplete;
        ILayerProvider m_ReplacedProvider;
        ILayerProvider m_WrappedContext;

        public WaitForUpdateCall(ILayerProvider context)
        {
            m_WrappedContext = context;
            m_ReplacedProvider = CompositionLayerManager.Instance.LayerProvider;
            CompositionLayerManager.Instance.LayerProvider = this;
        }

        public override bool keepWaiting => !m_WaitingComplete;

        public void SetInitialState(List<LayerInfo> layers)
        {
            m_UpdateCalled = false;
            m_WaitingComplete = false;
        }

        public void CleanupState() { }

        public void UpdateLayers(List<LayerInfo> createdLayers, List<int> removedLayers, List<LayerInfo> modifiedLayers, List<LayerInfo> activeLayers)
        {
            if (m_UpdateCalled)
                return;

            m_WrappedContext?.UpdateLayers(createdLayers, removedLayers, modifiedLayers, activeLayers);

            m_UpdateCalled = true;
        }

        public void LateUpdate()
        {
            if (m_UpdateCalled)
            {
                CompositionLayerManager.Instance.LayerProvider = m_ReplacedProvider;
                m_WaitingComplete = true;
            }
        }

        public void OnRenderObject() { }
    }
}
