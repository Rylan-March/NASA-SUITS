using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.XR.CompositionLayers.Rendering.Editor
{
    /// <summary>
    /// Helper methods for working with the graphics settings.
    /// </summary>
    public static class GraphicsSettingsHelpers
    {
        /// <summary>
        /// Enumerates the types of shaders used in the graphics settings.
        /// </summary>
        public enum ShaderType
        {
            /// <summary>
            /// The Blit HDR shader type.
            /// </summary>
            BlitCopyHDR,

            /// <summary>
            /// The Uber shader type.
            /// </summary>
            Uber,

            /// <summary>
            /// The Uber shader type.
            /// </summary>
            ColorScaleBias,
        }

        static readonly string[] s_ShaderNames = new string[]
        {
            "Unlit/XRCompositionLayers/BlitCopyHDR",
            "Unlit/XRCompositionLayers/Uber",
            "Hidden/XRCompositionLayers/ColorScaleBias",
        };

        /// <summary>
        /// Adds a shader to the list of always included shaders in the graphics settings.
        /// </summary>
        /// <param name="shaderType">The type of shader to add.</param>
        /// <returns><see langword="true"/> if the shader was added; <see langword="false"/> if the shader was not found or already included.</returns>
        /// <remarks>
        /// This method ensures that the specified shader is always included in the build by adding it to the graphics settings.
        /// </remarks>
        public static bool AddAlwaysIncludedShaders(ShaderType shaderType)
        {
            return AddAlwaysIncludedShader(s_ShaderNames[(int)shaderType]);
        }

        static GraphicsSettings s_GraphicsSettings;

        static bool AddAlwaysIncludedShader(string shaderName)
        {
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError($"Shader not found: {shaderName}");
                return false;
            }

            if (s_GraphicsSettings == null)
            {
                s_GraphicsSettings = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
                if (s_GraphicsSettings == null)
                    return false;
            }

            var graphicsSettingsSerializedObject = new SerializedObject(s_GraphicsSettings);
            var alwaysIncludedShadersSerializedProperty = graphicsSettingsSerializedObject.FindProperty("m_AlwaysIncludedShaders");
            if (alwaysIncludedShadersSerializedProperty == null)
                return false;

            int arraySize = alwaysIncludedShadersSerializedProperty.arraySize;

            for (int arrayIndex = 0; arrayIndex < arraySize; ++arrayIndex)
            {
                if (alwaysIncludedShadersSerializedProperty.GetArrayElementAtIndex(arrayIndex)?.objectReferenceValue == shader)
                {
                    return false;
                }
            }

            alwaysIncludedShadersSerializedProperty.InsertArrayElementAtIndex(arraySize);
            alwaysIncludedShadersSerializedProperty.GetArrayElementAtIndex(arraySize).objectReferenceValue = shader;
            graphicsSettingsSerializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            return true;
        }
    }
}