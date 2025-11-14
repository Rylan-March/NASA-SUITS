using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Unity.XR.CompositionLayers.Layers.Internal.Editor;

namespace Unity.XR.CompositionLayers.Layers.Editor
{
    /// <summary>
    /// For supporting to draw PlatformLayerData on Inspector.
    /// Supports all classes that inherit from PlatformLayerProvider.
    /// </summary>
    internal class PlatformLayerDataDrawer : IDisposable
    {
        /// <summary>
        /// Collect CustomAttributes for SerializedProperty in PlatformLayerData.
        /// Internal use only.
        /// </summary>
        class PropertyAttribute
        {
            PlatformLayerDataFieldAttribute m_PlatformLayerDataFieldAttribute;

            public PropertyAttribute(FieldInfo field)
            {
                var attributes = field?.GetCustomAttributes(true);
                if (attributes == null)
                    return;

                foreach (var attribute in attributes)
                {
                    if (attribute is PlatformLayerDataFieldAttribute)
                    {
                        m_PlatformLayerDataFieldAttribute = attribute as PlatformLayerDataFieldAttribute;
                    }
                }
            }

            /// <summary>
            /// Check the target property is supported on target LayerData.
            /// </summary>
            /// <param name="currentLayerData">Target LayerData.</param>
            /// <returns>If target LayerData is supported, this function returns true.</returns>
            public bool IsSupported(LayerData currentLayerData)
            {
                if (currentLayerData == null || m_PlatformLayerDataFieldAttribute == null ||
                    m_PlatformLayerDataFieldAttribute.SupportedLayerDataTypes == null ||
                    m_PlatformLayerDataFieldAttribute.SupportedLayerDataTypes.Length == 0)
                    return true;

                return m_PlatformLayerDataFieldAttribute.SupportedLayerDataTypes.Contains(currentLayerData.GetType());
            }
        }

        /// <summary>
        /// Container for UIElements, target property path (bindings) and property attribute.
        /// Internal use only.
        /// </summary>
        class PropertyElement
        {
            string m_PropertyPath;
            PropertyAttribute m_PropertyAttribute;
            PropertyField m_PropertyField;

            public PropertyField PropertyField { get => m_PropertyField; }

            public PropertyElement(SerializedProperty serializedProperty, PropertyAttribute propertyAttribute)
            {
                m_PropertyPath = serializedProperty.propertyPath;
                m_PropertyAttribute = propertyAttribute;
            }

            /// <summary>
            /// Bind and get property field which is visual element.
            /// </summary>
            /// <param name="propertyField">Root property field for PlatformLayerData.</param>
            /// <param name="currentLayerData">Selected LayerData.</param>
            public void Bind(PropertyField propertyField, LayerData currentLayerData)
            {
                if (m_PropertyField == null)
                {
                    m_PropertyField = FindPropertyFieldRecursively(propertyField, m_PropertyPath);
                    m_PropertyField?.SetEnabled(IsSupported(currentLayerData));
                }
            }

            /// <summary>
            /// This function is called when selected layer data is updated.
            /// </summary>
            /// <param name="currentLayerData">Selected LayerData.</param>
            public void OnUpdatedCurrentLayerData(LayerData currentLayerData)
            {
                m_PropertyField?.SetEnabled(IsSupported(currentLayerData));
            }

            /// <summary>
            /// Check the target property is supported on target LayerData.
            /// </summary>
            /// <param name="currentLayerData">Target LayerData.</param>
            /// <returns>If target LayerData is supported, this function returns true.</returns>
            bool IsSupported(LayerData currentLayerData)
            {
                if (currentLayerData == null || m_PropertyAttribute == null)
                    return true;

                return m_PropertyAttribute.IsSupported(currentLayerData);
            }
        }

        VisualElement m_RootElement;
        List<PropertyElement> m_PropertyElements;

        CompositionLayer m_CompositionLayer;
        PlatformLayerData m_PlatformLayerData;
        PlatformLayerDataHolder m_PlatformLayerDataHolder;
        PropertyField m_PlatformLayerDataField;

        /// <summary>
        /// Root element. This element is container for all properties. Supports folding.
        /// </summary>
        public VisualElement RootElement { get => m_RootElement; }

        public PlatformLayerDataDrawer(CompositionLayer compositionLayer, PlatformLayerData platformLayerData)
        {
            m_RootElement = new VisualElement();
            m_PropertyElements = new List<PropertyElement>();

            m_CompositionLayer = compositionLayer;
            m_PlatformLayerData = platformLayerData;

            if (platformLayerData != null)
            {
                var propertyAttributes = GetPropertyAttributes(platformLayerData);

                m_PlatformLayerDataHolder = ScriptableObject.CreateInstance<PlatformLayerDataHolder>();
                m_PlatformLayerDataHolder.m_PlatformLayerData = platformLayerData;
                var platformLayerDataHolderObject = new SerializedObject(m_PlatformLayerDataHolder);
                var platformLayerDataProperty = platformLayerDataHolderObject.FindProperty(PlatformLayerDataHolder.k_PlatformLayerDataName);
                m_PlatformLayerDataField = new PropertyField(platformLayerDataProperty, UIHelper.GetDisplayName(platformLayerData.GetType()));
                m_PlatformLayerDataField.Bind(platformLayerDataHolderObject);
                m_PlatformLayerDataField.TrackPropertyValue(platformLayerDataProperty, OnPropertyChanged);
                m_RootElement.Add(m_PlatformLayerDataField);

                m_RootElement.RegisterCallback<GeometryChangedEvent>(evt =>
                {
                    var layerData = m_CompositionLayer?.m_LayerData;
                    foreach (var element in m_PropertyElements)
                    {
                        element.Bind(m_PlatformLayerDataField, layerData);
                    }
                });

                var enumerator = platformLayerDataProperty.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var currentProperty = enumerator.Current as SerializedProperty;
                    if (currentProperty == null)
                        continue;

                    PropertyAttribute propertyAttribute;
                    if (!propertyAttributes.TryGetValue(currentProperty.name, out propertyAttribute))
                        propertyAttribute = null;

                    m_PlatformLayerDataField.TrackPropertyValue(currentProperty, OnPropertyChanged); //Fix: Need to call TrackPropertyValue() for each child property.

                    var element = new PropertyElement(currentProperty, propertyAttribute);
                    m_PropertyElements.Add(element);
                }

                OnUpdatedCurrentLayerData();
            }
        }

        void OnPropertyChanged(SerializedProperty serializedProperty)
        {
            if (m_CompositionLayer == null || m_PlatformLayerData == null)
                return;

            Undo.IncrementCurrentGroup();
            Undo.RecordObject(m_CompositionLayer, $"Update CompositionLayer.PlatformLayerData on {m_CompositionLayer.gameObject.name}");

            PlatformLayerDataUtil.WritePlatformLayerData(m_CompositionLayer, m_PlatformLayerData);
            EditorUtility.SetDirty(m_CompositionLayer);
        }

        /// <summary>
        /// This function is called when selected layer data is updated.
        /// </summary>
        /// <param name="currentLayerData">Selected LayerData.</param>
        public void OnUpdatedCurrentLayerData()
        {
            var layerData = m_CompositionLayer?.m_LayerData;
            foreach (var element in m_PropertyElements)
            {
                element.OnUpdatedCurrentLayerData(layerData);
            }
        }

        /// <summary>
        /// Reconstruct PlatformLayerData from CompositionLayer.
        /// </summary>
        public void UndoRedoPerformed()
        {
            PlatformLayerDataUtil.ReadPlatformLayerData(m_CompositionLayer, m_PlatformLayerData);
        }

        /// <summary>
        /// Dispose for all objects.
        /// SerializedObject is destroyed for preventing memory consuming.
        /// </summary>
        public void Dispose()
        {
            if (m_PlatformLayerDataHolder != null)
                ScriptableObject.DestroyImmediate(m_PlatformLayerDataHolder, true);

            m_PlatformLayerDataHolder = null;
        }

        //----------------------------------------------------------------------------------------------------------------------------------------------------

        static Dictionary<string, PropertyAttribute> GetPropertyAttributes(PlatformLayerData platformLayerData)
        {
            if (platformLayerData == null)
                return null;

            var propertyAttributes = new Dictionary<string, PropertyAttribute>();
            var fields = platformLayerData.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var propertyAttribute = new PropertyAttribute(field);
                if (propertyAttribute != null)
                {
                    propertyAttributes.Add(field.Name, propertyAttribute);
                }
            }

            return propertyAttributes;
        }

        static PropertyField FindPropertyFieldRecursively(VisualElement visualElement, string propertyPath)
        {
            foreach (var child in visualElement.Children())
            {
                if (child is PropertyField)
                {
                    var propertyField = (PropertyField)child;
                    if (propertyField.bindingPath == propertyPath)
                    {
                        return propertyField;
                    }
                }

                {
                    var propertyField = FindPropertyFieldRecursively(child, propertyPath);
                    if (propertyField != null)
                    {
                        return propertyField;
                    }
                }
            }

            return null;
        }
    }
}
