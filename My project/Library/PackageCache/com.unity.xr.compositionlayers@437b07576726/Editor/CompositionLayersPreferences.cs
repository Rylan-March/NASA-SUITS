using System;
using Unity.XR.CompositionLayers.Emulation;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Editor;
using UnityEditor.XR.CompositionLayers.Editor.Emulation;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.XR.CompositionLayers
{
    enum AngleDisplayType
    {
        Degrees = 0,
        Radians,
    }

    [ScriptableSettingsPath("Assets/CompositionLayers/UserSettings")]
    class CompositionLayersPreferences : EditorScriptableSettings<CompositionLayersPreferences>
    {
        [SerializeField]
        [Tooltip("Used to choose to display angles as either Degrees or Radians.")]
        AngleDisplayType m_DisplayAnglesAs = AngleDisplayType.Degrees;

        [SerializeField]
        [Tooltip("Enable or disable emulation of composition layers (Enabled only in Scene View By Default).")]
        bool m_EmulationInScene = true;

        [SerializeField]
        [Tooltip("Enable or disable emulation of composition layers in the game view in editor during play mode when no XR provider is active or no headset connected.")]
        bool m_EmulationInPlayMode = true;

        internal AngleDisplayType DisplayAnglesAs => m_DisplayAnglesAs;

        internal bool EnableEmulationInScene => m_EmulationInScene;

        internal bool EmulationInPlayMode => m_EmulationInPlayMode;

        void OnDisable()
        {
            EmulatedCompositionLayerUtils.GetEmulationInScene = null;
            EmulatedCompositionLayerUtils.GetEmulationInPlayMode = null;
        }

        internal static void RefreshEmulationSettings()
        {
            CompositionLayersEmulationLoader.ConnectCompositionLayerFunctions();

            EmulatedLayerProvider.DisconnectEmulatedLayerProvider();
            EmulatedLayerProvider.ConnectEmulatedLayerProvider();

#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
            {
                foreach (var obj in SceneView.sceneViews)
                {
                    if (obj is SceneView sceneView)
                        sceneView.Repaint();
                }
            };
#endif
        }
    }
}
