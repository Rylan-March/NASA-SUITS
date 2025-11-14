using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Unity.XR.CompositionLayers.Extensions;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CompositionLayers.Services;

namespace Unity.XR.CompositionLayers.Tests.Editor
{
    class LayerComponentTests : CompositionLayerManagerTestBase
    {
        [UnityTest]
        public IEnumerator AddingLayerAddsExpectedExtensions()
        {
            var layer = CreateLayerGameObject();
            yield return null;

            layer.AddSuggestedExtensions();

            var texExt = layer.gameObject.GetComponent<TexturesExtension>();

            Assert.IsNull(texExt);

            var layerId = typeof(CylinderLayerData).FullName;
            var layerData = CompositionLayerUtils.CreateLayerData(layerId);
            layer.ChangeLayerDataType(layerData);
            layer.AddSuggestedExtensions();

            texExt = layer.gameObject.GetComponent<TexturesExtension>();

            Assert.IsNotNull(texExt);
        }
    }
}
