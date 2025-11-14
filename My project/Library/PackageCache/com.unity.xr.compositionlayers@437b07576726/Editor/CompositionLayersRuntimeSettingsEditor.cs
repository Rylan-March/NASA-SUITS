using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Unity.XR.CompositionLayers.Editor
{
    /// <summary>
    /// Custom editor for <see cref="CompositionLayersRuntimeSettings"/>.
    /// </summary>
    [CustomEditor(typeof(CompositionLayersRuntimeSettings))]
    public class CompositionLayersRuntimeSettingsEditor : UnityEditor.Editor
    {
        const string k_CompositionSplashScene = "Packages/com.unity.xr.compositionlayers/Runtime/Scenes/CompositionSplash.unity";

        // Splash Settings
        const string k_EnableSplashScreen = "m_EnableSplashScreen";
        const string k_SplashImage = "m_SplashImage";
        const string k_BackgroundColor = "m_BackgroundColor";
        const string k_SplashDuration = "m_SplashDuration";
        const string k_FadeInDuration = "m_FadeInDuration";
        const string k_FadeOutDuration = "m_FadeOutDuration";
        const string k_FollowSpeed = "m_FollowSpeed";
        const string k_FollowDistance = "m_FollowDistance";
        const string k_LockToHorizon = "m_LockToHorizon";
        const string k_LayerType = "m_LayerType";
        const string k_QuadLayerData = "m_QuadLayerData";
        const string k_CylinderLayerData = "m_CylinderLayerData";
        const string k_ApplyTransformScale = "m_ApplyTransformScale";

        readonly List<string> m_SplashSettings = new List<string>
        {
            k_SplashImage,
            k_BackgroundColor,
            k_SplashDuration,
            k_FadeInDuration,
            k_FadeOutDuration,
            k_FollowSpeed,
            k_FollowDistance,
            k_LockToHorizon,
            k_LayerType,
            k_QuadLayerData,
            k_CylinderLayerData
        };

        readonly List<string> m_IgnoredLayerDataSettings = new List<string>
        {
            k_ApplyTransformScale
        };

        SceneAsset m_SplashScene;
        string m_SplashScenePath;

        /// <summary>
        /// Draws the custom inspector GUI for the <see cref="CompositionLayersRuntimeSettings"/>.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var settings = target as CompositionLayersRuntimeSettings;
            SerializedProperty prop = serializedObject.GetIterator();

            if (prop.NextVisible(true))
            {
                do
                {
                    // Draw the Splash Screen settings if the splash screen is enabled
                    if(prop.name == k_EnableSplashScreen)
                    {
                        EditorGUI.BeginChangeCheck();

                        EditorGUILayout.PropertyField(serializedObject.FindProperty(prop.name), true);

                        // Show a warning if the splash screen is enabled in the Player Settings
                        bool splashEnabled = serializedObject.FindProperty(prop.name).boolValue;
                        if(splashEnabled && (PlayerSettings.SplashScreen.show || PlayerSettings.virtualRealitySplashScreen != null))
                            EditorGUILayout.HelpBox("The Splash Screen should be disabled in the Player Settings to use the Composition Layers Splash Screen.", MessageType.Warning);

                        // Show a warning if the splash screen is enabled and there is no scene assigned after the splash scene
                        if(splashEnabled && !SceneAfterSplashInBuildSettings())
                            EditorGUILayout.HelpBox("There is no scene assigned after the splash scene in the Build Settings. The splash screen will display indefinitely.", MessageType.Error);

                        if (EditorGUI.EndChangeCheck())
                        {
                            serializedObject.ApplyModifiedProperties();

                            SceneAsset splashScene = GetSplashScene();
                            if(settings.EnableSplashScreen && !SplashSceneInBuildSettings() && splashScene != null)
                                AddSceneAtIndex(0, splashScene);
                            else if(!settings.EnableSplashScreen && SplashSceneInBuildSettings()  && splashScene != null)
                                RemoveSceneAtIndex(0);
                        }
                    }
                    // Hide the Splash Settings if the splash screen is disabled, or if the LayerData is not relevant to the LayerType
                    else if(IsSplashSetting(prop.name) && !settings.EnableSplashScreen
                            || prop.name == k_QuadLayerData && settings.LayerType == CompositionLayersRuntimeSettings.Layer.Cylinder
                            || prop.name == k_CylinderLayerData && settings.LayerType == CompositionLayersRuntimeSettings.Layer.Quad)
                    {
                        continue;
                    }
                     // Draw the LayerData properties
                    else if(prop.name == k_QuadLayerData || prop.name == k_CylinderLayerData)
                    {
                        EditorGUI.BeginChangeCheck();

                        // Display each parameter of the LayerData
                        SerializedProperty layerData;
                        switch(settings.LayerType)
                        {
                            case CompositionLayersRuntimeSettings.Layer.Quad:
                                layerData = serializedObject.FindProperty(k_QuadLayerData);
                                break;
                            case CompositionLayersRuntimeSettings.Layer.Cylinder:
                                layerData = serializedObject.FindProperty(k_CylinderLayerData);
                                break;
                            default:
                                layerData = serializedObject.FindProperty(k_QuadLayerData);
                                break;
                        }

                        SerializedProperty layerDataIterator = layerData.Copy();
                        layerDataIterator.NextVisible(true);

                        do
                        {
                            if(IsIgnoredLayerDataSetting(layerDataIterator.name))
                                continue;

                            EditorGUILayout.PropertyField(layerDataIterator, true);
                        }
                        while(layerDataIterator.NextVisible(false));

                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();
                    }
                    // Draw the default inspector for all other properties
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(prop, true);
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();
                    }
                }
                while (prop.NextVisible(false));
            }
        }

        bool SplashSceneInBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            if(scenes.Count > 0 && scenes[0].path == GetSplashScenePath())
                return true;

            return false;
        }

        bool SceneAfterSplashInBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            int splashIndex = scenes.FindIndex(scene => scene.path == GetSplashScenePath());
            if(splashIndex == -1)
                return false;

            for(int i = splashIndex + 1; i < scenes.Count; i++)
                if(scenes[i].enabled)
                    return true;

            return false;
        }

        void AddSceneAtIndex(int index, SceneAsset scene)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.Insert(index, new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(scene), true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        void RemoveSceneAtIndex(int index)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAt(index);
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        bool IsSplashSetting(string name)
        {
            return m_SplashSettings.Contains(name);
        }

        bool IsIgnoredLayerDataSetting(string name)
        {
            return m_IgnoredLayerDataSettings.Contains(name);
        }

        SceneAsset GetSplashScene()
        {
            if(m_SplashScene == null)
                m_SplashScene = (SceneAsset)AssetDatabase.LoadAssetAtPath(k_CompositionSplashScene, typeof(SceneAsset));

            return m_SplashScene;
        }

        string GetSplashScenePath()
        {
            if(m_SplashScenePath == null)
                m_SplashScenePath = AssetDatabase.GetAssetPath(GetSplashScene());

            return m_SplashScenePath;
        }
    }
}
