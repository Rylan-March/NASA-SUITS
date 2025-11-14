using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.XR.CompositionLayers.Extensions;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CompositionLayers.Services;

using UnityObject = UnityEngine.Object;

namespace Unity.XR.CompositionLayers.Tests
{
    class CompositionLayerManagerTests : CompositionLayerManagerTestBase
    {
        [UnityTest]
        public IEnumerator ChangingTextureExtensionReportsModifiedLayer()
        {
            //To ignore logError: A camera tagged as MainCamera is required for composition layer emulation.
            LogAssert.ignoreFailingMessages = true;
            var layer = CreateLayerGameObject();
            layer.gameObject.SetActive(true);
            layer.AddSuggestedExtensions();
            yield return new WaitForUpdateCall(this);

            var textureExt = layer.gameObject.GetComponent<TexturesExtension>();
            Assert.IsNull(textureExt);
            yield return new WaitForUpdateCall(this);

            var layerId = typeof(CylinderLayerData).FullName;
            var layerData = CompositionLayerUtils.CreateLayerData(layerId);
            layer.ChangeLayerDataType(layerData);
            yield return new WaitForUpdateCall(this);

            Assert.IsFalse(hasCreatedLayers);
            Assert.IsTrue(hasActiveLayers);
            Assert.IsFalse(hasRemovedLayers);
            Assert.IsTrue(hasModifiedLayers);
            yield return new WaitForUpdateCall(this);

            layer.AddSuggestedExtensions();
            textureExt = layer.gameObject.GetComponent<TexturesExtension>();
            Assert.IsNotNull(textureExt);

            Assert.IsFalse(hasCreatedLayers);
            Assert.IsTrue(hasActiveLayers);
            Assert.IsFalse(hasRemovedLayers);
            Assert.IsFalse(hasModifiedLayers);

            textureExt.TargetEye = TexturesExtension.TargetEyeEnum.Individual;

            yield return new WaitForUpdateCall(this);

            Assert.IsTrue(hasModifiedLayers);

            yield return new WaitForUpdateCall(this);

            Assert.IsFalse(hasModifiedLayers);

            textureExt.TargetEye = TexturesExtension.TargetEyeEnum.Individual;

            yield return new WaitForUpdateCall(this);

            Assert.IsFalse(hasModifiedLayers);

            UnityObject.DestroyImmediate(layer.gameObject);
            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator CompositionLayerManagerRemovedLayerNotInAddedList()
        {
            // Create Layer to start manager
            LogAssert.ignoreFailingMessages = true;
            var layerHold = CreateLayerGameObject();
            var layerHoldGo = layerHold.gameObject;
            layerHoldGo.SetActive(true);
            yield return new WaitForUpdateCall(this);

            var layerDestroy = CreateLayerGameObject();
            var layerDestroyGo = layerDestroy.gameObject;
            layerDestroyGo.SetActive(true);
            UnityObject.Destroy(layerDestroyGo);

            yield return new WaitForUpdateCall(this);

            Assert.IsFalse(hasCreatedLayers);
            Assert.IsTrue(hasRemovedLayers);

            UnityObject.Destroy(layerHold);
            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator CompositionLayerManagerTracksLayerState()
        {
            // Create Layer to start manager
            LogAssert.ignoreFailingMessages = true;
            var layerHold = CreateLayerGameObject();
            var layerHoldGo = layerHold.gameObject;
            layerHoldGo.SetActive(true);
            layerHold.Order = 100;
            yield return new WaitForUpdateCall(this);

            var layer = CreateLayerGameObject();
            layer.Order = 1;
            layer.gameObject.SetActive(true);
            yield return new WaitForUpdateCall(this);

            Assert.IsTrue(hasCreatedLayers);
            Assert.IsTrue(lastActiveLayers.Count > 1);
            Assert.IsFalse(hasModifiedLayers);
            Assert.IsFalse(hasRemovedLayers);

            var managerLayerId = lastCreatedLayers[0].Id;
            Assert.AreEqual(layer, lastCreatedLayers[0].Layer);
            Assert.AreEqual(lastActiveLayers[1].Layer, lastCreatedLayers[0].Layer);

            layer.gameObject.SetActive(false);
            yield return new WaitForUpdateCall(this);

            Assert.IsFalse(hasCreatedLayers);
            Assert.IsFalse(lastActiveLayers.Count > 2);
            Assert.IsTrue(hasModifiedLayers);
            Assert.IsFalse(hasRemovedLayers);

            layer.gameObject.SetActive(true);
            yield return new WaitForUpdateCall(this);

            Assert.IsFalse(hasCreatedLayers);
            Assert.IsTrue(lastActiveLayers.Count > 2);
            Assert.IsTrue(hasModifiedLayers);
            Assert.IsFalse(hasRemovedLayers);

            Assert.AreEqual(managerLayerId, lastModifiedLayers[0].Id);
            Assert.AreEqual(layer, lastModifiedLayers[0].Layer);
            Assert.AreEqual(lastActiveLayers[1].Layer, lastModifiedLayers[0].Layer);


            UnityObject.Destroy(layer.gameObject);

            yield return new WaitForUpdateCall(this);

            Assert.IsFalse(hasCreatedLayers);
            Assert.IsFalse(lastActiveLayers.Count > 2);
            Assert.IsFalse(hasModifiedLayers);
            Assert.IsTrue(hasRemovedLayers);
            Assert.AreEqual(managerLayerId, lastRemovedLayers[0]);

            yield return new WaitForUpdateCall(this);

            Assert.IsFalse(hasCreatedLayers);
            Assert.IsFalse(lastActiveLayers.Count > 2);
            Assert.IsFalse(hasModifiedLayers);
            Assert.IsFalse(hasRemovedLayers);

            UnityObject.Destroy(layerHold.gameObject);
            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator CompositionLayerManagerSortsLayersCorrectly()
        {
            LogAssert.ignoreFailingMessages = true;
            var layerOne = CreateLayerGameObject(false);
            layerOne.Order = 0;

            var layerTwo = CreateLayerGameObject(false);
            var layerFive = CreateLayerGameObject(false);
            layerFive.Order = 5;

            var layerSix = CreateLayerGameObject(false);
            layerSix.Order = 5;

            var layerNegOne = CreateLayerGameObject(false);
            layerNegOne.Order = -1;

            var layerNegTwo = CreateLayerGameObject(false);
            layerNegTwo.Order = -1;

            Assert.IsFalse(hasCreatedLayers);
            Assert.IsFalse(hasModifiedLayers);
            Assert.IsFalse(hasRemovedLayers);

            // Checks that it is after default layer and does not collide with default layer order
            layerOne.gameObject.SetActive(true);

            yield return new WaitForUpdateCall(this);

            Assert.AreEqual(CompositionLayerManager.Instance.OccupiedLayers.Count, lastActiveLayers.Count);
            Assert.AreEqual(lastActiveLayers[1].Layer, layerOne);
            Assert.AreEqual(layerOne.Order, 1);

            // Layer under default is before it in the active layers
            layerNegOne.gameObject.SetActive(true);
            yield return new WaitForUpdateCall(this);
            Assert.AreEqual(lastActiveLayers[0].Layer, layerNegOne);
            Assert.AreEqual(lastActiveLayers[2].Layer, layerOne);
            Assert.AreEqual(layerNegOne.Order, -1);

            // Layer collision on new underlay layer finds next available underlay index
            layerNegTwo.gameObject.SetActive(true);
            yield return new WaitForUpdateCall(this);
            Assert.AreEqual(lastActiveLayers[0].Layer, layerNegTwo);
            Assert.AreEqual(lastActiveLayers[1].Layer, layerNegOne);
            Assert.AreEqual(layerNegTwo.Order, -2);

            // New overlay layer is above previous layers
            layerTwo.gameObject.SetActive(true);
            yield return new WaitForUpdateCall(this);
            var maxIndex = lastActiveLayers.Count - 1;
            Assert.AreEqual(lastActiveLayers[maxIndex].Layer, layerTwo);
            Assert.AreEqual(layerTwo.Order, 2);

            // non contiguous orders are respected
            layerFive.gameObject.SetActive(true);
            yield return new WaitForUpdateCall(this);
            Assert.AreEqual(layerFive.Order, 5);

            // Layer collisions of non contiguous layers fill to the outside
            layerSix.gameObject.SetActive(true);
            yield return new WaitForUpdateCall(this);
            maxIndex = lastActiveLayers.Count - 1;
            Assert.AreEqual(lastActiveLayers[maxIndex - 1].Layer, layerFive);
            Assert.AreEqual(lastActiveLayers[maxIndex].Layer, layerSix);
            Assert.AreEqual(layerFive.Order, 5);
            Assert.AreEqual(layerSix.Order, 6);

            // Able to set layers to open order
            layerSix.Order = 4;
            // Able to set layer to newly opened value as soon as other value changes
            layerFive.Order = 6;
            yield return new WaitForUpdateCall(this);
            Assert.AreEqual(layerFive.Order, 6);
            Assert.AreEqual(layerSix.Order, 4);

            // Able to set layers to order they previously held as soon as that order value is free
            layerFive.Order = 5;
            layerSix.Order = 6;
            yield return new WaitForUpdateCall(this);
            Assert.AreEqual(layerFive.Order, 5);
            Assert.AreEqual(layerSix.Order, 6);

            // Able to swap layers
            Assert.True(CompositionLayerUtils.TrySwapLayers(layerNegTwo, layerSix));
            yield return new WaitForUpdateCall(this);
            maxIndex = lastActiveLayers.Count - 1;
            Assert.AreEqual(lastActiveLayers[0].Layer, layerSix);
            Assert.AreEqual(lastActiveLayers[maxIndex].Layer, layerNegTwo);
            Assert.AreEqual(layerSix.Order, -2);
            Assert.AreEqual(layerNegTwo.Order, 6);

            // Swap back
            Assert.True(CompositionLayerUtils.TrySwapLayers(layerNegTwo, layerSix));
            yield return new WaitForUpdateCall(this);
            Assert.AreEqual(layerSix.Order, 6);
            Assert.AreEqual(layerNegTwo.Order, -2);

            // Able to disable a layer and does not sort to active layers
            layerFive.gameObject.SetActive(false);
            layerNegTwo.enabled = false;
            yield return new WaitForUpdateCall(this);
            Assert.False(FindLayer(lastActiveLayers, layerFive, out _));
            Assert.False(FindLayer(lastActiveLayers, layerNegTwo, out _));

            // Able to enable a layer and does not sort to active layers
            layerFive.gameObject.SetActive(true);
            layerNegTwo.enabled = true;
            yield return new WaitForUpdateCall(this);
            Assert.True(FindLayer(lastActiveLayers, layerFive, out _));
            Assert.True(FindLayer(lastActiveLayers, layerNegTwo, out _));

            // remove closest to default layer and next layer fills in sort wise
            UnityObject.Destroy(layerNegOne.gameObject);
            UnityObject.Destroy(layerOne.gameObject);
            yield return new WaitForUpdateCall(this);
            Assert.True(FindLayer(lastActiveLayers, CompositionLayerManager.Instance.DefaultSceneCompositionLayer, out var pivot));
            Assert.AreEqual(lastActiveLayers[pivot - 1].Layer, layerNegTwo);
            Assert.AreEqual(lastActiveLayers[pivot + 1].Layer, layerTwo);
            // Oder values were not changed due to layers removed.
            Assert.AreEqual(layerNegTwo.Order, -2);
            Assert.AreEqual(layerTwo.Order, 2);

            yield return new WaitForUpdateCall(this);
            UnityObject.Destroy(layerTwo.gameObject);
            UnityObject.Destroy(layerFive.gameObject);
            UnityObject.Destroy(layerSix.gameObject);
            UnityObject.Destroy(layerNegTwo.gameObject);
            LogAssert.ignoreFailingMessages = false;
        }

        // [UnityTest]
        // public IEnumerator SceneCompositionLayerSetToZeroBeforeFallbackLayer()
        // {
        //     var layer = CreateLayerGameObject(false);
        //     layer.ChangeLayerDataType(typeof(DefaultLayerData));
        //     layer.Order = 0;
        //     yield return new WaitForUpdateCall(this);
        //
        //     // User layer order set to 0 does not change `DefaultSceneCompositionLayer` assignment
        //     // or interfere with `FallbackDefaultSceneCompositionLayer` order being 0 when Manager is setup
        //     layer.gameObject.SetActive(true);
        //     Assert.AreEqual(layer.Order, 1);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer,
        //         CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //     yield return new WaitForUpdateCall(this);
        //
        //     yield return new WaitForUpdateCall(this);
        //     UnityObject.DestroyImmediate(layer.gameObject);
        // }
        //
        // [UnityTest]
        // public IEnumerator SceneCompositionLayerSetToDefaultLayer()
        // {
        //     var layer = CreateLayerGameObject(false);
        //     layer.ChangeLayerDataType(typeof(DefaultLayerData));
        //     layer.Order = 1;
        //     yield return new WaitForUpdateCall(this);
        //
        //     // Fallback is current DefaultSceneCompositionLayer before scene layer is set
        //     layer.gameObject.SetActive(true);
        //     Assert.AreNotEqual(layer.Order, 0);
        //     Assert.AreNotEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //     yield return new WaitForUpdateCall(this);
        //
        //     // Scene layer is set to DefaultSceneCompositionLayer and order 0
        //     CompositionLayerManager.Instance.SetDefaultSceneCompositionLayer(layer);
        //     Assert.AreEqual(layer.Order, 0);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //
        //     yield return new WaitForUpdateCall(this);
        //     UnityObject.DestroyImmediate(layer.gameObject);
        // }
        //
        // [UnityTest]
        // public IEnumerator SceneCompositionLayerUnsetFromDefaultLayerWithNull()
        // {
        //     var layer = CreateLayerGameObject(false);
        //     layer.ChangeLayerDataType(typeof(DefaultLayerData));
        //     layer.Order = 1;
        //
        //     yield return new WaitForUpdateCall(this);
        //     // This starts the manager if it was not running
        //     CompositionLayerManager.Instance.EnsureSceneCompositionManager();
        //
        //     layer.gameObject.SetActive(true);
        //     CompositionLayerManager.Instance.SetDefaultSceneCompositionLayer(layer);
        //     Assert.AreEqual(layer.Order, 0);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //     yield return new WaitForUpdateCall(this);
        //
        //     // Unset with SetDefaultSceneCompositionLayer to null
        //     CompositionLayerManager.Instance.SetDefaultSceneCompositionLayer(null);
        //     Assert.False(layer.isActiveAndEnabled);
        //     Assert.AreNotEqual(layer.Order, 0);
        //     Assert.AreNotEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //     yield return new WaitForUpdateCall(this);
        //
        //     yield return new WaitForUpdateCall(this);
        //     UnityObject.DestroyImmediate(layer.gameObject);
        // }
        //
        // [UnityTest]
        // public IEnumerator SceneCompositionLayerUnsetFromDefaultLayerWithReset()
        // {
        //     var layer = CreateLayerGameObject(false);
        //     layer.ChangeLayerDataType(typeof(DefaultLayerData));
        //     layer.Order = 1;
        //
        //     yield return new WaitForUpdateCall(this);
        //     // This starts the manager if it was not running
        //     CompositionLayerManager.Instance.EnsureSceneCompositionManager();
        //
        //     layer.gameObject.SetActive(true);
        //     CompositionLayerManager.Instance.SetDefaultSceneCompositionLayer(layer);
        //     Assert.AreEqual(layer.Order, 0);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //     yield return new WaitForUpdateCall(this);
        //
        //     // Unset with SetDefaultSceneCompositionLayer to null
        //     CompositionLayerManager.Instance.ResetDefaultSceneCompositionLayer();
        //     Assert.False(layer.isActiveAndEnabled);
        //     Assert.AreNotEqual(layer.Order, 0);
        //     Assert.AreNotEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer,
        //         CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer);
        //     yield return new WaitForUpdateCall(this);
        //
        //     yield return new WaitForUpdateCall(this);
        //     UnityObject.DestroyImmediate(layer.gameObject);
        // }
        //
        // [UnityTest]
        // public IEnumerator SceneCompositionLayerUnsetFromDefaultLayerWithDisableOnUpdate()
        // {
        //     var layer = CreateLayerGameObject(false);
        //     layer.ChangeLayerDataType(typeof(DefaultLayerData));
        //     layer.Order = 1;
        //
        //     yield return new WaitForUpdateCall(this);
        //     // This starts the manager if it was not running
        //     CompositionLayerManager.Instance.EnsureSceneCompositionManager();
        //
        //     layer.gameObject.SetActive(true);
        //     CompositionLayerManager.Instance.SetDefaultSceneCompositionLayer(layer);
        //     Assert.AreEqual(layer.Order, 0);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //     yield return new WaitForUpdateCall(this);
        //
        //     // Unset with SetDefaultSceneCompositionLayer in update with disable
        //     layer.gameObject.SetActive(false);
        //     yield return new WaitForUpdateCall(this);
        //
        //     Assert.AreNotEqual(layer.Order, 0);
        //     Assert.AreNotEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer,
        //         CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer);
        //     yield return new WaitForUpdateCall(this);
        //
        //     yield return new WaitForUpdateCall(this);
        //     UnityObject.DestroyImmediate(layer.gameObject);
        // }
        //
        // [UnityTest]
        // public IEnumerator SceneCompositionLayerUnsetFromDefaultLayerWithDestroyOnUpdate()
        // {
        //     var layer = CreateLayerGameObject(false);
        //     layer.ChangeLayerDataType(typeof(DefaultLayerData));
        //     layer.Order = 1;
        //
        //     yield return new WaitForUpdateCall(this);
        //     // This starts the manager if it was not running
        //     CompositionLayerManager.Instance.EnsureSceneCompositionManager();
        //
        //     layer.gameObject.SetActive(true);
        //     CompositionLayerManager.Instance.SetDefaultSceneCompositionLayer(layer);
        //     Assert.AreEqual(layer.Order, 0);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //     yield return new WaitForUpdateCall(this);
        //
        //     // Unset with SetDefaultSceneCompositionLayer in update with disable
        //     UnityObject.Destroy(layer.gameObject);
        //     yield return new WaitForUpdateCall(this);
        //
        //     Assert.True(layer == null);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer,
        //         CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer);
        // }
        //
        // [UnityTest]
        // public IEnumerator SceneCompositionLayerSetOnAwakeComponent()
        // {
        //     var layer = CreateLayerGameObject(false);
        //     layer.ChangeLayerDataType(typeof(DefaultLayerData));
        //     layer.gameObject.AddComponent<UserDefaultLayerTestComponent>();
        //     layer.Order = 1;
        //
        //     yield return new WaitForUpdateCall(this);
        //     // This starts the manager if it was not running
        //     CompositionLayerManager.Instance.EnsureSceneCompositionManager();
        //
        //     // Fallback is current DefaultSceneCompositionLayer before scene layer is set
        //     Assert.AreNotEqual(layer.Order, 0);
        //     Assert.AreNotEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //     yield return new WaitForUpdateCall(this);
        //
        //     // Scene layer is set to DefaultSceneCompositionLayer and order 0
        //     layer.gameObject.SetActive(true);
        //     Assert.AreEqual(layer.Order, 0);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //
        //     yield return new WaitForUpdateCall(this);
        //     UnityObject.DestroyImmediate(layer.gameObject);
        // }
        //
        // [UnityTest]
        // public IEnumerator SceneCompositionLayerUnsetOnDisableComponent()
        // {
        //     var layer = CreateLayerGameObject(false);
        //     layer.ChangeLayerDataType(typeof(DefaultLayerData));
        //     layer.gameObject.AddComponent<UserDefaultLayerTestComponent>();
        //     layer.Order = 1;
        //
        //     yield return new WaitForUpdateCall(this);
        //     // This starts the manager if it was not running
        //     CompositionLayerManager.Instance.EnsureSceneCompositionManager();
        //
        //     // Scene layer is set to DefaultSceneCompositionLayer and order 0
        //     layer.gameObject.SetActive(true);
        //     Assert.AreEqual(layer.Order, 0);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //
        //     // Fallback is current DefaultSceneCompositionLayer before scene layer is set
        //     layer.gameObject.SetActive(false);
        //     Assert.AreNotEqual(layer.Order, 0);
        //     Assert.AreNotEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer,
        //         CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer);
        //     yield return new WaitForUpdateCall(this);
        //
        //     yield return new WaitForUpdateCall(this);
        //     UnityObject.DestroyImmediate(layer.gameObject);
        // }
        //
        // [UnityTest]
        // public IEnumerator SceneCompositionLayerSettingMultipleToDefaultLayer()
        // {
        //     var layerOne = CreateLayerGameObject(false);
        //     layerOne.ChangeLayerDataType(typeof(DefaultLayerData));
        //     layerOne.Order = 1;
        //
        //     var layerTwo = CreateLayerGameObject(false);
        //     layerTwo.ChangeLayerDataType(typeof(DefaultLayerData));
        //     layerTwo.Order = 2;
        //
        //     yield return new WaitForUpdateCall(this);
        //     // This starts the manager if it was not running
        //     CompositionLayerManager.Instance.EnsureSceneCompositionManager();
        //
        //     // Scene layerOne is set to DefaultSceneCompositionLayer and order 0
        //     layerOne.gameObject.SetActive(true);
        //     layerTwo.gameObject.SetActive(true);
        //     CompositionLayerManager.Instance.SetDefaultSceneCompositionLayer(layerOne);
        //     Assert.AreEqual(layerOne.Order, 0);
        //     Assert.AreNotEqual(layerTwo.Order, 0);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layerOne);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //     yield return new WaitForUpdateCall(this);
        //
        //     CompositionLayerManager.Instance.SetDefaultSceneCompositionLayer(layerTwo);
        //     Assert.AreNotEqual(layerOne.Order, 0);
        //     Assert.AreEqual(layerTwo.Order, 0);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layerTwo);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //     yield return new WaitForUpdateCall(this);
        //
        //     yield return new WaitForUpdateCall(this);
        //     UnityObject.DestroyImmediate(layerOne.gameObject);
        //     UnityObject.DestroyImmediate(layerTwo.gameObject);
        // }
        //
        // [UnityTest]
        // public IEnumerator SceneCompositionLayerSettingMultipleToDefaultLayerSameFrame()
        // {
        //     var layerOne = CreateLayerGameObject(false);
        //     layerOne.ChangeLayerDataType(typeof(DefaultLayerData));
        //     layerOne.Order = 1;
        //
        //     var layerTwo = CreateLayerGameObject(false);
        //     layerTwo.ChangeLayerDataType(typeof(DefaultLayerData));
        //     layerTwo.Order = 2;
        //
        //     yield return new WaitForUpdateCall(this);
        //     // This starts the manager if it was not running
        //     CompositionLayerManager.Instance.EnsureSceneCompositionManager();
        //
        //     // Scene layerTwo is set to DefaultSceneCompositionLayer and order 0 since it was last
        //     layerOne.gameObject.SetActive(true);
        //     layerTwo.gameObject.SetActive(true);
        //     CompositionLayerManager.Instance.SetDefaultSceneCompositionLayer(layerOne);
        //     Assert.AreEqual(layerOne.Order, 0);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layerOne);
        //     CompositionLayerManager.Instance.SetDefaultSceneCompositionLayer(layerTwo);
        //     Assert.AreNotEqual(layerOne.Order, 0);
        //     Assert.AreEqual(layerTwo.Order, 0);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layerTwo);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //     yield return new WaitForUpdateCall(this);
        //
        //     yield return new WaitForUpdateCall(this);
        //     UnityObject.DestroyImmediate(layerOne.gameObject);
        //     UnityObject.DestroyImmediate(layerTwo.gameObject);
        // }
        //
        // [UnityTest]
        // public IEnumerator SceneCompositionLayerSettingEnablesObject()
        // {
        //     var layer = CreateLayerGameObject(false);
        //     layer.ChangeLayerDataType(typeof(DefaultLayerData));
        //     layer.Order = 1;
        //     layer.enabled = false;
        //
        //     yield return new WaitForUpdateCall(this);
        //     // This starts the manager if it was not running
        //     CompositionLayerManager.Instance.EnsureSceneCompositionManager();
        //
        //     // Fallback is current DefaultSceneCompositionLayer before scene layer is set
        //     layer.gameObject.SetActive(false);
        //     Assert.AreNotEqual(layer.Order, 0);
        //     Assert.AreNotEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //     yield return new WaitForUpdateCall(this);
        //
        //     // Scene layer is set to DefaultSceneCompositionLayer and order 0
        //     CompositionLayerManager.Instance.SetDefaultSceneCompositionLayer(layer);
        //     Assert.True(layer.isActiveAndEnabled);
        //     Assert.AreEqual(layer.Order, 0);
        //     Assert.AreEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //
        //     yield return new WaitForUpdateCall(this);
        //     UnityObject.DestroyImmediate(layer.gameObject);
        // }
        //
        // [UnityTest]
        // public IEnumerator SceneCompositionLayerCannotSetWithDisabledParent()
        // {
        //     var layer = CreateLayerGameObject(false);
        //     layer.ChangeLayerDataType(typeof(DefaultLayerData));
        //     layer.Order = 1;
        //
        //     yield return new WaitForUpdateCall(this);
        //     // This starts the manager if it was not running
        //     CompositionLayerManager.Instance.EnsureSceneCompositionManager();
        //
        //     // Fallback is current DefaultSceneCompositionLayer before scene layer is set
        //     layer.gameObject.SetActive(true);
        //     Assert.True(layer.isActiveAndEnabled);
        //     Assert.AreNotEqual(layer.Order, 0);
        //     Assert.AreNotEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //     yield return new WaitForUpdateCall(this);
        //
        //     var inactiveParent = new GameObject("Inactive Parent Test");
        //     inactiveParent.SetActive(false);
        //     layer.transform.SetParent(inactiveParent.transform);
        //
        //     // Scene layer is set to DefaultSceneCompositionLayer and order 0
        //     CompositionLayerManager.Instance.SetDefaultSceneCompositionLayer(layer);
        //     Assert.False(layer.isActiveAndEnabled);
        //     Assert.AreNotEqual(layer.Order, 0);
        //     Assert.AreNotEqual(CompositionLayerManager.Instance.DefaultSceneCompositionLayer, layer);
        //     Assert.AreEqual(CompositionLayerManager.Instance.FallbackDefaultSceneCompositionLayer.Order, 0);
        //
        //     yield return new WaitForUpdateCall(this);
        //     UnityObject.DestroyImmediate(inactiveParent);
        // }
    }
}
