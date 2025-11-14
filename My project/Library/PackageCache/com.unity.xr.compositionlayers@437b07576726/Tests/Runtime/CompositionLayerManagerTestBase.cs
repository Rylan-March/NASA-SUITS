using System;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.XR.CompositionLayers.Provider;
using Unity.XR.CompositionLayers.Services;
using UnityEngine;
using UnityEngine.TestTools;
using LayerInfo = Unity.XR.CompositionLayers.Services.CompositionLayerManager.LayerInfo;

using UnityObject = UnityEngine.Object;

namespace Unity.XR.CompositionLayers.Tests
{
    class CompositionLayerManagerTestBase : ILayerProvider
    {
        protected bool hasCreatedLayers;
        protected bool hasRemovedLayers;
        protected bool hasModifiedLayers;
        protected bool hasActiveLayers;

        protected List<LayerInfo> lastCreatedLayers = new();
        protected List<int> lastRemovedLayers = new();
        protected List<LayerInfo> lastModifiedLayers = new();
        protected List<LayerInfo> lastActiveLayers = new();

        bool m_EditorProviderState;

        [SetUp]
        public void SetUp()
        {
            Setup();
        }

        [TearDown]
        public void TearDown()
        {
            Teardown();
        }

        protected virtual void Setup()
        {
            hasCreatedLayers = false;
            hasRemovedLayers = false;
            hasModifiedLayers = false;
            hasActiveLayers = false;

            lastCreatedLayers.Clear();
            lastRemovedLayers.Clear();
            lastModifiedLayers.Clear();
            lastActiveLayers.Clear();

            LogAssert.ignoreFailingMessages = true;
            CompositionLayerManager.Instance?.ClearAllState();
            CompositionLayerManager.Instance?.EnsureSceneCompositionManager();
        }

        protected virtual void Teardown()
        {
            foreach (var layer in lastActiveLayers)
            {
                if (layer.Layer)
                    UnityObject.Destroy(layer.Layer.gameObject);
            }

            CompositionLayerManager.Instance?.ClearAllState();

            lastCreatedLayers.Clear();
            lastRemovedLayers.Clear();
            lastModifiedLayers.Clear();
            lastActiveLayers.Clear();

            hasCreatedLayers = false;
            hasRemovedLayers = false;
            hasModifiedLayers = false;
            hasActiveLayers = false;
        }

        public void SetInitialState(List<LayerInfo> layers) { }

        public void CleanupState() { }

        public void UpdateLayers(List<LayerInfo> createdLayers, List<int> removedLayers,
            List<LayerInfo> modifiedLayers, List<LayerInfo> activeLayers)
        {
            lastCreatedLayers.Clear();
            lastRemovedLayers.Clear();
            lastModifiedLayers.Clear();
            lastActiveLayers.Clear();

            if (createdLayers != null)
            {
                hasCreatedLayers = createdLayers.Count > 0;
                lastCreatedLayers.AddRange(createdLayers);
            }
            else
            {
                hasCreatedLayers = false;
            }

            if (removedLayers != null)
            {
                hasRemovedLayers = removedLayers.Count > 0;
                lastRemovedLayers.AddRange(removedLayers);
            }
            else
            {
                hasRemovedLayers = false;
            }

            if (modifiedLayers != null)
            {
                hasModifiedLayers = modifiedLayers.Count > 0;
                lastModifiedLayers.AddRange(modifiedLayers);
            }
            else
            {
                hasModifiedLayers = false;
            }

            if (activeLayers != null)
            {
                hasActiveLayers = activeLayers.Count > 0;
                lastActiveLayers.AddRange(activeLayers);
            }
            else
            {
                hasActiveLayers = false;
            }
        }

        public void LateUpdate() { }

        public void OnRenderObject() { }

        protected CompositionLayer CreateLayerGameObject(bool recycle = true)
        {
            var name = "Test";
            GameObject gameObject = null;

            if (recycle)
            {
                gameObject = GameObject.Find(name);
            }

            if (gameObject == null)
            {
                name += Guid.NewGuid().ToString();
                gameObject = new GameObject(name);
                gameObject.SetActive(false);
            }

            var layerComponent = gameObject.AddComponent<CompositionLayer>();
            return layerComponent;
        }

        protected static bool FindLayer(List<LayerInfo> layerInfos, CompositionLayer layer, out int index)
        {
            for (var i = 0; i < layerInfos.Count; i++)
            {
                if (layerInfos[i].Layer == layer)
                {
                    index = i;
                    return true;
                }
            }

            index = 0;
            return false;
        }
    }
}
