using System;
using Unity.XR.CompositionLayers.Layers.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Extensions.Editor
{
    /// <summary>
    /// Abstract base class used to define some common functionality for
    /// specific editor implementation.
    /// </summary>
    [CustomEditor(typeof(CompositionLayerExtension), true, isFallback = true)]
    internal class CompositionLayerExtensionEditor : UnityEditor.Editor
    {
        protected virtual void OnEnable()
        {
            foreach (var targetObject in targets)
            {
                if (targetObject is CompositionLayerExtension layerExtension)
                {
                    // Ensures `ReportStateChange` is set
                    layerExtension.Awake();
                }
            }
        }

        /// <summary>
        /// Updates the serialized properties and draws the Inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script", "m_CompositionLayer");
            if (serializedObject.hasModifiedProperties)
                ApplyChangesWithReportStateChange();
        }

        /// <summary>
        /// Used to <see cref="SerializedObject.ApplyModifiedProperties"/> then
        /// <see cref="CompositionExtensionBase.ReportStateChange"/> so any changes made in the editor are reported
        /// back to the <see cref="Unity.XR.CompositionLayers.Services.CompositionLayerManager"/>
        /// </summary>
        protected void ApplyChangesWithReportStateChange()
        {
            serializedObject.ApplyModifiedProperties();
            foreach (var targetObject in serializedObject.targetObjects)
            {
                if (targetObject is CompositionLayerExtension layerExtension)
                    layerExtension.ReportStateChange();
            }
        }
    }
}
