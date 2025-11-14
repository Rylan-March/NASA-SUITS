using UnityEditor;

namespace Unity.XR.CompositionLayers.Rendering.Editor
{
    /// <summary>
    /// Creates the MirrorViewRenderer editor.
    /// </summary>
    [CustomEditor(typeof(MirrorViewRenderer))]
    public class MirrorViewRendererEditor : UnityEditor.Editor
    {
        SerializedProperty AlphaModeProperty;

        /// <summary>
        /// Initializes the editor when it is enabled.
        /// </summary>
        void OnEnable()
        {
            AlphaModeProperty = serializedObject.FindProperty("AlphaMode");

            GraphicsSettingsHelpers.AddAlwaysIncludedShaders(GraphicsSettingsHelpers.ShaderType.BlitCopyHDR);
            GraphicsSettingsHelpers.AddAlwaysIncludedShaders(GraphicsSettingsHelpers.ShaderType.Uber);
        }

        /// <summary>
        /// Draws the custom inspector GUI for the <see cref="MirrorViewRenderer"/>.
        /// </summary>
        public override void OnInspectorGUI()
        {
            var mirrorViewRenderer = (MirrorViewRenderer)target;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            if (AlphaModeProperty != null)
                EditorGUILayout.PropertyField(AlphaModeProperty);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
