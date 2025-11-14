using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.XR.CompositionLayers.Provider;

namespace Unity.XR.CompositionLayers.Layers.Internal.Editor
{
    internal static class UIHelper
    {
        const string k_WarningIconPath = "d_console.warnicon.sml";
        const string k_ErrorIconPath = "d_console.erroricon.sml";

        static int s_PushDisabledCount = 0;

        /// <summary>
        /// Push GUI enabled state.
        /// </summary>
        public static void PushEnabled(bool isEnabled)
        {
            GUI.enabled = GUI.enabled && isEnabled;
            if (!GUI.enabled)
                ++s_PushDisabledCount;
        }

        /// <summary>
        /// Pop GUI enabled state.
        /// </summary>
        public static void PopEnabled()
        {
            if (s_PushDisabledCount > 0 && --s_PushDisabledCount == 0)
            {
                GUI.enabled = true;
            }
        }

        /// <summary>
        /// Get the warning icon for CompositionLayerEditor.
        /// This icon is used for validation messages. (Unsupported underlay, unsuported blend types and unsupported layer types.)
        /// </summary>
        /// <returns>Texture for the warning icon.</returns>
        public static Texture GetWarningIcon()
        {
            return EditorGUIUtility.Load(k_WarningIconPath) as Texture;
        }

        /// <summary>
        /// Get the error icon for HDRTonemappingEditor.
        /// </summary>
        /// <returns>Texture for the error icon.</returns>
        public static Texture GetErrorIcon()
        {
            return EditorGUIUtility.Load(k_ErrorIconPath) as Texture;
        }

        /// <summary>
        /// Create mesasge field field with small icon.
        /// This function is used for UIElements.
        /// </summary>
        /// <param name="texture">Small icon.</param>
        /// <param name="message">Message text.</param>
        /// <returns>Container element for Label & Image.</returns>
        public static VisualElement UILabel(Texture texture, string message)
        {
            var label = new Label(message);
            var image = new Image();
            image.image = texture;
            var visualElement = new VisualElement();
            visualElement.style.flexDirection = FlexDirection.Row;
            visualElement.Add(image);
            visualElement.Add(label);
            return visualElement;
        }

        /// <summary>
        /// Create mesasge field with warning small icon.
        /// This function is used for UIElements.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <returns>Container element for Label & Image.</returns>
        public static VisualElement UIWarning(string message)
        {
            return UILabel(GetWarningIcon(), message);
        }

        /// <summary>
        /// Create mesasge field with warning small icon for specific platforms.
        /// This function is used for UIElements.
        /// </summary>
        /// <param name="message">Message text. {0} in text will be the platform name.</param>
        /// <param name="platformProvider">Target platform provider. The platform name is generated with GetDisplayName()</param>
        /// <returns>Container element for Label & Image.</returns>
        public static VisualElement UIWarning(string message, PlatformProvider platformProvider)
        {
            return UIWarning(string.Format(message, GetDisplayName(platformProvider)));
        }

        /// <summary>
        /// Draw mesasge field field with small icon.
        /// This function is used for EditorGUILayout.
        /// </summary>
        /// <param name="image">Small icon.</param>
        /// <param name="message">Message text.</param>
        public static void GUIWarning(Texture image, string message)
        {
            EditorGUILayout.LabelField(new GUIContent(message, image));
        }

        /// <summary>
        /// Draw mesasge field field with small warning icon.
        /// This function is used for EditorGUILayout.
        /// </summary>
        /// <param name="message">Message text.</param>
        public static void GUIWarning(string message)
        {
            GUIWarning(GetWarningIcon(), message);
        }

        /// <summary>
        /// Draw mesasge field with warning small icon for specific platforms.
        /// This function is used for EditorGUILayout.
        /// </summary>
        /// <param name="message">Message text. {0} in text will be the platform name.</param>
        /// <param name="platformProvider">Target platform provider. The platform name is generated with GetDisplayName()</param>
        public static void GUIWarning(string message, PlatformProvider platformProvider)
        {
            GUIWarning(string.Format(message, GetDisplayName(platformProvider)));
        }

        /// <summary>
        /// Get display name from any class.
        /// This function simulates inspector on Editor.
        /// </summary>
        /// <param name="type">Target type.</param>
        /// <returns>Generate display name.</returns>
        public static string GetDisplayName(Type type)
        {
            return GetDisplayName(type?.Name);
        }

        /// <summary>
        /// Get display name from name.
        /// This function simulates inspector on Editor.
        /// </summary>
        /// <param name="name">Target name.</param>
        /// <returns>Generate display name.</returns>
        public static string GetDisplayName(string name)
        {
            if (name == null)
                return "";

            var upperName = name.ToUpper();
            var nameLength = name.Length;
            new StringBuilder();
            var newName = new StringBuilder();

            for (int i = 0; i < nameLength; ++i)
            {
                if (name[i] != upperName[i])
                {
                    if (i > 1 && name[i - 1] == upperName[i - 1]) // Upper case
                    {
                        newName.Insert(newName.Length - 1, ' ');
                    }
                }

                newName.Append(name[i]);
            }

            return newName.ToString();
        }

        /// <summary>
        /// Get the display name from PlatformProvider.
        /// </summary>
        /// <param name="platformProvider">Target platform provider. Need to return LayerProviderType.</param>
        /// <return>Display name string.</return>
        public static string GetDisplayName(PlatformProvider platformProvider)
        {
            if (platformProvider == null)
                return "Default";

            var layerProviderType = platformProvider.LayerProviderType;
            if (layerProviderType != null)
            {
                var name = layerProviderType.Name;
                name = RemovePostfix(name, "LayerProvider");
                name = RemovePostfix(name, "Provider");
                return name;
            }

            var xrLoader = platformProvider.XRLoaderType;
            if (xrLoader != null)
            {
                var name = xrLoader.Name;
                return name;
            }

            return "Default";
        }

        /// <summary>
        /// Remove postfix in string. Internal use only.
        /// </summary>
        static string RemovePostfix(string name, string postfix)
        {
            return name.EndsWith(postfix) ? name.Substring(0, name.Length - postfix.Length) : name;
        }

        /// <summary>
        /// Concat enumerated display names.
        /// </summary>
        /// <param name="names">Text array for concatting.</param>
        /// <return>Display name.</return>
        public static string ConcatEnumeratedNames(IEnumerable<string> names)
        {
            if (names == null)
                return "";

            var str = new StringBuilder();
            int i = 0, count = names.Count();
            foreach (var name in names)
            {
                if (i != 0)
                    str.Append((i == count - 1) ? " and " : ", ");

                str.Append(name);
                ++i;
            }

            return str.ToString();
        }

        /// <summary>
        /// Concat multi line texts.
        /// </summary>
        /// <param name="texts">Text array for concatting.</param>
        /// <return>Concatted text.</return>
        public static string ConcatMultiLineTexts(IEnumerable<string> texts)
        {
            if (texts == null)
                return "";

            var str = new StringBuilder();
            int i = 0, count = texts.Count();
            foreach (var text in texts)
            {
                if (i < count - 1)
                    str.AppendLine(text);
                else
                    str.Append(text);

                ++i;
            }

            return str.ToString();
        }

    }
}
