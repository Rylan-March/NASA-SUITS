using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Unity.XR.CompositionLayers.Rendering;
using Unity.XR.CompositionLayers.Services.Editor;
using Unity.XR.CompositionLayers.Layers.Internal.Editor;
using System.Text;
using Unity.XR.CompositionLayers.Provider;

namespace Unity.XR.CompositionLayers.Extensions.Editor
{
    /// <summary>
    /// Custom editor for the <see cref="HDRTonemappingExtension" /> component.
    /// </summary>
    [CustomEditor(typeof(HDRTonemappingExtension))]
    class HDRTonemappingEditor : CompositionLayerExtensionEditor
    {
        const string TextureNamePrefix = "HDRTonemapping Sample";
        const int ImageWidth = 256;
        const int ImageHeight = 256;

        const string WarningMessageNoSupportingHDR = "HDR isn't supported on the active platform.";

        SerializedProperty m_ColorGamut;
        SerializedProperty m_NitsForPaperWhite;
        SerializedProperty m_MaxDisplayNits;
        bool m_SampleGroup = false;

        string m_ErrorMessage;
        string m_WarningMessage;

        static GraphicsFormat[] s_Formats_SRGB = new GraphicsFormat[]
        {
            GraphicsFormat.R8G8B8A8_UNorm,
            GraphicsFormat.R8G8B8A8_SRGB,
        };

        static GraphicsFormat[] s_Formats_GenericHDR = new GraphicsFormat[]
        {
            GraphicsFormat.R32G32B32A32_SFloat,
        };

        static GraphicsFormat[] GetGraphicsFormats(ColorGamut colorGamut)
        {
            switch (colorGamut)
            {
                case ColorGamut.sRGB: return s_Formats_SRGB;
                case ColorGamut.HDR10: return s_Formats_GenericHDR;
                case ColorGamut.Rec709: return s_Formats_GenericHDR;
                case ColorGamut.Rec2020: return s_Formats_GenericHDR;
                default: return s_Formats_GenericHDR;
            }
        }

        class ColorGamutDesc
        {
            public int selectedFormatIndex;
            public GraphicsFormat[] formats;
            public string[] formatTexts;

            public ColorGamutDesc(ColorGamut colorGamut)
            {
                selectedFormatIndex = 0;
                formats = GetGraphicsFormats(colorGamut);
                formatTexts = ToStrings(formats);
            }
        };

        static Dictionary<ColorGamut, ColorGamutDesc> s_ColorGamutDescDictionary;

        static void AppendMessage(ref string messageBuf, string message)
        {
            if (messageBuf == null)
                messageBuf = message;
            else
                messageBuf += "\n" + message;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            m_ColorGamut = serializedObject.FindProperty("m_ColorGamut");
            m_NitsForPaperWhite = serializedObject.FindProperty("m_NitsForPaperWhite");
            m_MaxDisplayNits = serializedObject.FindProperty("m_MaxDisplayNits");

            var supportingHDRProviders = EditorPlatformManager.SupportingHDRProviders;

            if (supportingHDRProviders == null || supportingHDRProviders.Count == 0)
            {
                AppendMessage(ref m_WarningMessage, WarningMessageNoSupportingHDR);
            }

            if (s_ColorGamutDescDictionary == null)
            {
                s_ColorGamutDescDictionary = new Dictionary<ColorGamut, ColorGamutDesc>();
                var colorGamutValues = Enum.GetValues(typeof(ColorGamut));
                foreach (var colorGamut in colorGamutValues)
                {
                    s_ColorGamutDescDictionary.Add((ColorGamut)colorGamut, new ColorGamutDesc((ColorGamut)colorGamut));
                }
            }
        }

        static string[] ToStrings(GraphicsFormat[] formats)
        {
            var array = new string[formats.Length];
            for (int i = 0; i < formats.Length; ++i)
            {
                array[i] = formats[i].ToString();
            }

            return array;
        }

        static string GeneratePreferredHDRParamsMessage(IReadOnlyList<PlatformProvider> providers)
        {
            if (providers == null || providers.Count == 0)
                return null;

            bool needToAddNewLine = false;
            var r = new StringBuilder();
            foreach (var provider in providers)
            {
                var hdrParams = provider.GetPreferredHDRParams();
                if (needToAddNewLine)
                    r.AppendLine();
                r.AppendLine("Device HDR Params (" + UIHelper.GetDisplayName(provider) + ")");
                r.Append("ColorGamut: " + hdrParams.ColorGamut);
                if (hdrParams.MaxDisplayNits > 0.0f || hdrParams.NitsForPaperWhite > 0.0f)
                {
                    r.AppendLine();
                    r.AppendLine("NitsForPaperWhite: " + hdrParams.NitsForPaperWhite);
                    r.Append("MaxDisplayNits: " + hdrParams.MaxDisplayNits);
                }
                needToAddNewLine = true;
            }

            return r.Length > 0 ? r.ToString() : null;
        }

        /// <summary>
        /// Draws the custom inspector GUI for the <see cref="HDRTonemappingExtension"/>.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (!string.IsNullOrEmpty(m_ErrorMessage))
            {
                EditorGUILayout.LabelField(new GUIContent(m_ErrorMessage, UIHelper.GetErrorIcon()));
            }
            else if (!string.IsNullOrEmpty(m_WarningMessage))
            {
                EditorGUILayout.LabelField(new GUIContent(m_WarningMessage, UIHelper.GetWarningIcon()));
            }

            var supportingHDRProviders = EditorPlatformManager.SupportingHDRProviders;

            if (supportingHDRProviders == null || supportingHDRProviders.Count == 0)
                return;

            EditorGUILayout.PropertyField(m_ColorGamut);
            EditorGUILayout.PropertyField(m_NitsForPaperWhite);
            EditorGUILayout.PropertyField(m_MaxDisplayNits);

            var preferredHDRParamsMessage = GeneratePreferredHDRParamsMessage(supportingHDRProviders);
            if (preferredHDRParamsMessage != null)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.HelpBox(preferredHDRParamsMessage, MessageType.None);
            }

            if (serializedObject.hasModifiedProperties)
                ApplyChangesWithReportStateChange();

            EditorGUILayout.Separator();

            m_SampleGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_SampleGroup, "Sample");
            if (m_SampleGroup)
            {
                var colorGamut = (ColorGamut)m_ColorGamut.enumValueIndex;
                if (s_ColorGamutDescDictionary.TryGetValue(colorGamut, out var colorGamutDesc))
                {
                    UIHelper.PushEnabled(colorGamutDesc.formatTexts.Length > 1);
                    colorGamutDesc.selectedFormatIndex = EditorGUILayout.Popup("Format", colorGamutDesc.selectedFormatIndex, colorGamutDesc.formatTexts);
                    UIHelper.PopEnabled();
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Generate"))
                {
                    GenerateSample();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void GenerateSample()
        {
            var extension = target as HDRTonemappingExtension;
            var textureExtension = extension.GetComponent<Unity.XR.CompositionLayers.Extensions.TexturesExtension>();
            if (textureExtension == null)
            {
                return;
            }

            var colorGamut = (ColorGamut)m_ColorGamut.enumValueIndex;
            var nitsForPaperWhite = m_NitsForPaperWhite.intValue;
            var maxDisplayNits = m_MaxDisplayNits.intValue;

            s_ColorGamutDescDictionary.TryGetValue(colorGamut, out var colorGamutDesc);
            GraphicsFormat graphicsFormat = colorGamutDesc.formats[colorGamutDesc.selectedFormatIndex];

            var tempTexture = new Texture2D(ImageWidth, ImageHeight, GraphicsFormat.R8G8B8A8_SRGB, 1, TextureCreationFlags.None);
            GenerateSimpleColormap(tempTexture);

            var renderTextureDesc = new RenderTextureDescriptor(ImageWidth, ImageHeight, graphicsFormat, GraphicsFormat.None, 1);
            renderTextureDesc.volumeDepth = 1;
            renderTextureDesc.msaaSamples = 1;
            renderTextureDesc.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            if (colorGamut == ColorGamut.sRGB)
                renderTextureDesc.sRGB = graphicsFormat.ToString().Contains("SRGB");

            var renderTexture = new RenderTexture(renderTextureDesc);
            renderTexture.Create();

            var blitParams = new ImageFilters.BlitParams(tempTexture);

            var targetParams = new ImageFilters.TargetParams(renderTexture);
            targetParams.hdrParams.hdrEncoded = true;
            targetParams.hdrParams.colorGamut = colorGamut;
            targetParams.hdrParams.nitsForPaperWhite = nitsForPaperWhite;
            targetParams.hdrParams.maxDisplayNits = maxDisplayNits;

            ImageFilters.Blit(blitParams, targetParams);

            Texture2D.DestroyImmediate(tempTexture);

            var textureName = TextureNamePrefix + $"({colorGamut}:{nitsForPaperWhite}:{maxDisplayNits})";
            var tempTexture2 = new Texture2D(ImageWidth, ImageHeight, graphicsFormat, 1, TextureCreationFlags.None);
            tempTexture2.name = textureName;
            RenderTexture.active = renderTexture;
            tempTexture2.ReadPixels(new Rect(0, 0, ImageWidth, ImageHeight), 0, 0);
            tempTexture2.Apply();
            RenderTexture.active = null;
            RenderTexture.DestroyImmediate(renderTexture);

            Undo.RecordObject(textureExtension, textureName);
            DestroySampleTexture(textureExtension.LeftTexture);
            DestroySampleTexture(textureExtension.RightTexture);
            textureExtension.LeftTexture = tempTexture2;
            textureExtension.RightTexture = tempTexture2;
        }

        static void DestroySampleTexture(Texture texture)
        {
            if (texture != null && texture.name.StartsWith(TextureNamePrefix))
            {
                Texture.DestroyImmediate(texture);
            }
        }

        static void GenerateSimpleColormap(Texture2D texture)
        {
            var width = texture.width;
            var height = texture.height;

            var colorMap = new Color[width * height];

            var resolutionX = 8;
            var resolutionY = 8;
            var blockSizeX = width / resolutionX;
            var blockSizeY = height / resolutionY;

            for (int y = 0, beginPosY = 0; y < resolutionY; ++y, beginPosY += blockSizeY)
            {
                var endPosY = (y < (resolutionY - 1)) ? (beginPosY + blockSizeY) : height;
                float scalerY = ((float)y / (float)(resolutionY - 1)) * 2.0f;
                for (int colorIdx = 0, beginPosX = 0; colorIdx < resolutionX; ++colorIdx, beginPosX += blockSizeX)
                {
                    var color = GetColor(colorIdx);
                    var endPosX = (colorIdx < (resolutionX - 1)) ? (beginPosX + blockSizeX) : width;
                    color.r *= scalerY;
                    color.g *= scalerY;
                    color.b *= scalerY;
                    Fill(colorMap, width, beginPosX, beginPosY, endPosX, endPosY, color);
                }
            }

            texture.SetPixels(colorMap);
            texture.Apply();
        }

        static void Fill(Color[] colorMap, int width, int beginPosX, int beginPosY, int endPosX, int endPosY, Color color)
        {
            for (int posY = beginPosY, offsetY = beginPosY * width + beginPosX; posY < endPosY; ++posY, offsetY += width)
            {
                for (int posX = beginPosX, offsetX = offsetY; posX < endPosX; ++posX, ++offsetX)
                {
                    colorMap[offsetX] = color;
                }
            }
        }

        const uint ColorPalette = (1 << 3) | (2 << 6) | (4 << 9) | (3 << 12) | (5 << 15) | (6 << 18) | (7 << 21);

        static Color GetColor(int colorIdx)
        {
            uint color = (ColorPalette >> (colorIdx * 3)) & 7;

            uint r = color & 1;
            uint g = (color >> 1) & 1;
            uint b = (color >> 2) & 1;
            return new Color((float)r, (float)g, (float)b, 1.0f);
        }
    }
}
