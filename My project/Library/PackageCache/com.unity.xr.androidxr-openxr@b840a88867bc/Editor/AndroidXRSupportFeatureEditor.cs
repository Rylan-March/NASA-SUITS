using UnityEngine;
using UnityEngine.XR.OpenXR;

using UnityEngine.XR.OpenXR.Features.Android;

namespace UnityEditor.XR.OpenXR.Features.Android
{
    [CustomEditor(typeof(AndroidXRSupportFeature))]
    internal class AndroidXRSupportFeatureeEditor : Editor
    {

#if UNITY_6000_1_OR_NEWER
        private static GUIContent s_OptimizeMultiviewRenderRegionsLabel = EditorGUIUtility.TrTextContent("Optimize Multiview Render Regions (Vulkan)", "Activates Multiview Render Regions optimizations at application start. Requires Unity 6.1 or later, Vulkan as the Graphics API, Render Mode set to Multi-view and Symmetric rendering enabled.");
#endif
        private SerializedProperty symmetricProjection;
        private SerializedProperty optimizeBufferDiscards;
#if UNITY_6000_1_OR_NEWER
        private SerializedProperty optimizeMultiviewRenderRegions;
#endif

        void OnEnable()
        {
            symmetricProjection = serializedObject.FindProperty("symmetricProjection");
            optimizeBufferDiscards = serializedObject.FindProperty("optimizeBufferDiscards");
#if UNITY_6000_1_OR_NEWER
            optimizeMultiviewRenderRegions = serializedObject.FindProperty("optimizeMultiviewRenderRegions");
#endif
        }

        public override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = 275.0f;

            serializedObject.Update();

            EditorGUILayout.LabelField("Rendering Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(symmetricProjection, new GUIContent("Symmetric Projection (Vulkan)"));
            EditorGUILayout.PropertyField(optimizeBufferDiscards, new GUIContent("Optimize Buffer Discards (Vulkan)"));
#if UNITY_6000_1_OR_NEWER
            EditorGUILayout.PropertyField(optimizeMultiviewRenderRegions, s_OptimizeMultiviewRenderRegionsLabel);
#endif
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            OpenXRSettings androidOpenXRSettings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            var serializedOpenXrSettings = new SerializedObject(androidOpenXRSettings);

            androidOpenXRSettings.symmetricProjection = symmetricProjection.boolValue;
            androidOpenXRSettings.optimizeBufferDiscards = optimizeBufferDiscards.boolValue;
#if UNITY_6000_1_OR_NEWER
            androidOpenXRSettings.optimizeMultiviewRenderRegions = optimizeMultiviewRenderRegions.boolValue;
#endif
            serializedOpenXrSettings.ApplyModifiedProperties();

            EditorGUIUtility.labelWidth = 0.0f;
        }
    }
}
