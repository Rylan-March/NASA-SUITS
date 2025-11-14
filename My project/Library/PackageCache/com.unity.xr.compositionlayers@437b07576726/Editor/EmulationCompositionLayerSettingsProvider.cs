using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace Unity.XR.CompositionLayers.Editor
{
    class EmulationCompositionLayerSettingsProvider : SettingsProvider
    {
        const string k_SettingsRootTitle = "Preferences/XR/Composition Layers";

        SerializedObject m_SerializedObject;

        EmulationCompositionLayerSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords) { }

        [SettingsProvider]
        static SettingsProvider Create()
        {
            var provider = new EmulationCompositionLayerSettingsProvider(k_SettingsRootTitle, SettingsScope.User,
                new HashSet<string>(new[] { "composition", "layer", "layers", "xr", "emulation" }));

            return provider;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var compositionLayersPreferences = CompositionLayersPreferences.Instance;
            if (compositionLayersPreferences == null)
                compositionLayersPreferences = ScriptableObject.CreateInstance<CompositionLayersPreferences>();

            m_SerializedObject = new SerializedObject(compositionLayersPreferences);

            var scrollElement = new ScrollView(ScrollViewMode.Vertical);
            rootElement.Add(scrollElement);

            var settingsDrawer = new CompositionLayersPreferencesDrawer();
            var headerElement = new Label("Composition Layers Preferences");
            headerElement.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
            headerElement.style.fontSize = new StyleLength(19f);
            headerElement.style.marginLeft = new StyleLength(8f);
            headerElement.style.marginBottom = new StyleLength(20f);
            scrollElement.Add(headerElement);
            settingsDrawer.CreateDrawerGUI(m_SerializedObject, scrollElement, false);

            rootElement.Bind(m_SerializedObject);
        }
    }
}
