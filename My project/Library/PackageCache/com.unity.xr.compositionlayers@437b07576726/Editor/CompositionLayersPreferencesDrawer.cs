using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.XR.CompositionLayers.Editor
{
    class CompositionLayersPreferencesDrawer
    {
        SerializedObject m_SerializedObject;
        Toggle m_EmulationInSceneElement;
        Toggle m_EmulationInPlayModeElement;

        bool m_UndoEnabled;

        internal void CreateDrawerGUI(SerializedObject serializedObject, VisualElement rootElement, bool undoEnabled)
        {
            m_SerializedObject = serializedObject;
            m_UndoEnabled = undoEnabled;

            var displayAnglesAsProperty = m_SerializedObject.FindProperty("m_DisplayAnglesAs");
            var displayAnglesElement = new PropertyField(displayAnglesAsProperty);
            displayAnglesElement.RegisterValueChangeCallback(DisplayAnglesAsPropertyChanged);
            rootElement.Add(displayAnglesElement);

            var enableEmulationProperty = m_SerializedObject.FindProperty("m_EmulationInScene");
            var enabled = enableEmulationProperty.boolValue;
            m_EmulationInSceneElement = new Toggle(enableEmulationProperty.displayName);
            m_EmulationInSceneElement.tooltip = enableEmulationProperty.tooltip;
            m_EmulationInSceneElement.value = enableEmulationProperty.boolValue;
            m_EmulationInSceneElement.RegisterValueChangedCallback(OnEnableEmulationInSceneElementChanged);
            rootElement.Add(m_EmulationInSceneElement);

            var emulationInPlayModeProperty = m_SerializedObject.FindProperty("m_EmulationInPlayMode");
            m_EmulationInPlayModeElement = new Toggle(emulationInPlayModeProperty.displayName);
            m_EmulationInPlayModeElement.tooltip = emulationInPlayModeProperty.tooltip;
            m_EmulationInPlayModeElement.value = emulationInPlayModeProperty.boolValue;
            m_EmulationInPlayModeElement.RegisterValueChangedCallback(OnEmulationInPlayModeElementChanged);
            rootElement.Add(m_EmulationInPlayModeElement);

            rootElement.RegisterCallback<MouseEnterEvent>(UpdateSettingsDrawerCallback);

            m_EmulationInPlayModeElement.SetEnabled(enabled);
        }

        void DisplayAnglesAsPropertyChanged(SerializedPropertyChangeEvent evt)
        {
            if (m_UndoEnabled)
                m_SerializedObject.ApplyModifiedProperties();
            else
                m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();

            m_SerializedObject.Update();
        }

        void UpdateSettingsDrawerCallback(MouseEnterEvent evt) => m_SerializedObject.Update();


        void OnEnableEmulationInSceneElementChanged(ChangeEvent<bool> evt)
        {
            var enabled = evt.newValue;
            m_SerializedObject.FindProperty("m_EmulationInScene").boolValue = enabled;
            OnPropertyChanged();
        }

        void OnEmulationInPlayModeElementChanged(ChangeEvent<bool> evt)
        {
            var enabled = evt.newValue;
            m_SerializedObject.FindProperty("m_EmulationInPlayMode").boolValue = enabled;
            OnPropertyChanged();
        }

        void OnPropertyChanged()
        {
            if (m_UndoEnabled)
                m_SerializedObject.ApplyModifiedProperties();
            else
                m_SerializedObject.ApplyModifiedPropertiesWithoutUndo();

            m_SerializedObject.Update();

            if (m_SerializedObject.targetObject is CompositionLayersPreferences)
                CompositionLayersPreferences.RefreshEmulationSettings();
        }
    }
}
