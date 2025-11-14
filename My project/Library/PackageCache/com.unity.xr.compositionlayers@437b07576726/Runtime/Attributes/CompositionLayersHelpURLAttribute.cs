#if UNITY_EDITOR && UNITY_2021_1_OR_NEWER
#define CAN_USE_CUSTOM_HELP_URL
#endif

using System;
using System.Diagnostics;
using UnityEngine;

namespace Unity.XR.CompositionLayers
{
#if CAN_USE_CUSTOM_HELP_URL

    using UnityEditor.PackageManager;

    [Conditional("UNITY_EDITOR")]
    class CompositionLayersHelpURLAttribute : HelpURLAttribute
    {
        const string k_BaseURL = "https://docs.unity3d.com";
        const string k_MidURL = "/Packages/com.unity.xr.compositionlayers@";
        const string k_ApiURL = "/api/";
        const string k_FallbackVersion = "0.6";
        const string k_EndURL = ".html";

        static readonly string k_PackageVersion;

        static CompositionLayersHelpURLAttribute()
        {
            var assembly = typeof(CompositionLayer).Assembly;
            var packageInfo = PackageInfo.FindForAssembly(assembly);

#if UNITY_EDITOR
            if (packageInfo == null)
            {
                k_PackageVersion = k_FallbackVersion;
                return;
            }

            var splitVersion = packageInfo.version.Split('.');
            k_PackageVersion = $"{splitVersion[0]}.{splitVersion[1]}";
#else
            k_PackageVersion = k_FallbackVersion;
#endif
        }

        public CompositionLayersHelpURLAttribute(Type type)
            : base(HelpURL(type)) {}

        static string HelpURL(Type type)
        {
            return $"{k_BaseURL}{k_MidURL}{k_PackageVersion}{k_ApiURL}{type.FullName}{k_EndURL}";
        }
    }
#else //HelpURL attribute is `sealed` in previous Unity versions
    [Conditional("UNITY_EDITOR")]
    class CompositionLayersHelpURLAttribute : Attribute
    {
        public CompositionLayersHelpURLAttribute(Type type) { }
    }
#endif
}
