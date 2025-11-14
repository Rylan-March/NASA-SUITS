using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Editor
{
    static class GUIHelpers
    {
        internal static Texture2D LoadIcon(string iconPath, string name)
        {
            var themePrefix = EditorGUIUtility.isProSkin ? "d_" : "";
            var scale = Mathf.Clamp((int)EditorGUIUtility.pixelsPerPoint, 0, 4);
            var scalePostFix = scale > 1 ? $"@{scale}x" : "";
            var path = Path.Combine(iconPath, themePrefix + name + scalePostFix + ".png");
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        internal static Texture2D LoadIconMaxSize(string iconPath, string name)
        {
            var themePrefix = EditorGUIUtility.isProSkin ? "d_" : "";
            var scalePostFix = $"@4x";
            var path = Path.Combine(iconPath, themePrefix + name + scalePostFix + ".png");
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
