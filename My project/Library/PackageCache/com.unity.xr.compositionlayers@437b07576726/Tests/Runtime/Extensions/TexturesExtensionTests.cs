using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CompositionLayers.Services;
using Unity.XR.CompositionLayers.Tests;
using UnityObject = UnityEngine.Object;

namespace Unity.XR.CompositionLayers.Extensions.Tests
{
    class TexturesExtensionTests : CompositionLayerManagerTestBase
    {
        [UnityTest]
        public IEnumerator TexturesExtensionReportsEyeModification()
        {
            LogAssert.ignoreFailingMessages = true;
            var layer = CreateLayerGameObject();
            layer.ChangeLayerDataType(typeof(QuadLayerData));
            layer.gameObject.SetActive(true);

            layer.AddSuggestedExtensions();

            yield return new WaitForUpdateCall(this);

            var textureExt = layer.gameObject.GetComponent<TexturesExtension>();
            Assert.IsNotNull(textureExt);

            textureExt.TargetEye = TexturesExtension.TargetEyeEnum.Individual;
            yield return new WaitForUpdateCall(this);
            Assert.IsTrue(hasModifiedLayers);
            yield return new WaitForUpdateCall(this);
            Assert.IsFalse(hasModifiedLayers);

            textureExt.TargetEye = TexturesExtension.TargetEyeEnum.Individual;
            yield return new WaitForUpdateCall(this);
            Assert.IsFalse(hasModifiedLayers);
            LogAssert.ignoreFailingMessages = false;

        }

        [UnityTest]
        public IEnumerator TexturesExtensionReportsEyeTextureModification()
        {
            LogAssert.ignoreFailingMessages = true;
            var layer = CreateLayerGameObject();
            layer.ChangeLayerDataType(typeof(QuadLayerData));
            layer.gameObject.SetActive(true);

            layer.AddSuggestedExtensions();
            yield return new WaitForUpdateCall(this);

            var textureExt = layer.gameObject.GetComponent<TexturesExtension>();
            Assert.IsNotNull(textureExt);

            textureExt.LeftTexture = Texture2D.whiteTexture;
            yield return new WaitForUpdateCall(this);
            Assert.IsTrue(hasModifiedLayers);
            yield return new WaitForUpdateCall(this);
            Assert.IsFalse(hasModifiedLayers);

            textureExt.LeftTexture = Texture2D.whiteTexture;
            yield return new WaitForUpdateCall(this);
            Assert.IsFalse(hasModifiedLayers);

            textureExt.LeftTexture = Texture2D.blackTexture;
            yield return new WaitForUpdateCall(this);
            Assert.IsTrue(hasModifiedLayers);
            yield return new WaitForUpdateCall(this);
            Assert.IsFalse(hasModifiedLayers);

            textureExt.LeftTexture = null;
            yield return new WaitForUpdateCall(this);
            Assert.IsTrue(hasModifiedLayers);
            yield return new WaitForUpdateCall(this);
            Assert.IsFalse(hasModifiedLayers);
            LogAssert.ignoreFailingMessages = false;
        }
    }
}
