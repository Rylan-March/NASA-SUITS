using System;
using NUnit.Framework;
using UnityEngine;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CompositionLayers.Services;
using UnityObject = UnityEngine.Object;

namespace Unity.XR.CompositionLayers.Tests
{
    class CompositionLayerTests : CompositionLayerManagerTestBase
    {
        GameObject m_TestGameObject;
        CompositionLayer m_TestLayer;

        protected override void Setup()
        {
            base.Setup();
            m_TestLayer = CreateLayerGameObject();
            m_TestGameObject = m_TestLayer.gameObject;
            m_TestGameObject.name = "Composition Layer Test GameObject";
        }

        protected override void Teardown()
        {
            UnityObject.Destroy(m_TestLayer);
            UnityObject.DestroyImmediate(m_TestGameObject);
            base.Teardown();
        }

        [Test]
        public void CanChangeLayerType()
        {
            var lDesc = CompositionLayerUtils.GetLayerDescriptor(typeof(CylinderLayerData).FullName);
            Assert.AreNotEqual(default, lDesc);

            var lData = CompositionLayerUtils.CreateLayerData(lDesc.TypeFullName);
            Assert.IsNotNull(lData);

            m_TestLayer.ChangeLayerDataType(lData);

            Assert.IsNotNull(m_TestLayer.LayerData as CylinderLayerData);
        }
    }
}
