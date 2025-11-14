using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;
using Unity.XR.CompositionLayers.Provider;

namespace Unity.XR.CompositionLayers.Services.Editor
{
    /// <summary>
    /// Check to update XRGeneralSettings for PlatformSelector.
    /// </summary>
    class PlatformAssetModificationProcessor : AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
            {
                if (path == PlatformSelector.GetXRGeneralSettingsAssetPath())
                {
                    PlatformSelector.OnUpdatedXRGeneralSettings();
                }
            }

            return paths;
        }
    }

    /// <summary>
    /// Check to change build target for PlatformSelector.
    /// </summary>
    class PlatformActiveBuildTargetChanged : IActiveBuildTargetChanged
    {
        public int callbackOrder { get { return 0; } }

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            PlatformSelector.OnActiveBuildTargetChanged(newTarget);
        }
    }

    /// <summary>
    /// Manage PlatformManager.ActivePlatformProvider (Editor only).
    /// It'll be updated on changing build target or updating build settings.
    /// </summary>
    static class PlatformSelector
    {
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            AssemblyReloadEvents.afterAssemblyReload += RefreshActivePlatformProvider;
            RefreshActivePlatformProvider();
        }

        internal static void OnUpdatedXRGeneralSettings()
        {
            RefreshActivePlatformProvider();
        }

        internal static void OnActiveBuildTargetChanged(BuildTarget buildTarget)
        {
            RefreshActivePlatformProvider();
        }

        //----------------------------------------------------------------------------------------------------------------------------

        public static void RefreshActivePlatformProvider()
        {
            UpdateActivePlatformProviders();
        }

        internal static void UpdateActivePlatformProviders()
        {
            var providers = GetPlatformProviders();
            var loaders = GetActiveXRLoadersForActiveBuildTarget();
            if (providers == null || loaders == null)
            {
                PlatformManager.ActivePlatformProvider = null;
                return;
            }

            var activePlatformPrividers = new List<PlatformProvider>();

            foreach (var loader in loaders)
            {
                var loaderType = loader.GetType();
                foreach (var provider in providers)
                {
                    if (loaderType == provider.XRLoaderType)
                    {
                        activePlatformPrividers.Add(provider);
                        break;
                    }
                }
            }

            PlatformManager.ActivePlatformProvider = activePlatformPrividers.Count > 0 ? activePlatformPrividers[0] : null;
        }

        //----------------------------------------------------------------------------------------------------------------------------

        static PlatformProvider[] GetPlatformProviders()
        {
            var types = TypeCache.GetTypesDerivedFrom(typeof(PlatformProvider)).ToList();
            var providers = new List<PlatformProvider>();
            foreach (var type in types)
            {
                providers.Add(Activator.CreateInstance(type) as PlatformProvider);
            }

            return providers.ToArray();
        }

        static XRLoader[] GetActiveXRLoadersForActiveBuildTarget()
        {
            var activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            var activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(activeBuildTarget);
            var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(activeBuildTargetGroup);
            if (generalSettings == null)
            {
                return null;
            }

            var managerSettings = generalSettings.AssignedSettings;
            return managerSettings?.activeLoaders?.ToArray();
        }

        static string s_XRGeneralSettingsAssetPath;
        const string k_XRGeneralSettingsDefaultAssetPath = "Assets/XR/XRGeneralSettings.asset";

        internal static string GetXRGeneralSettingsAssetPath()
        {
            if (s_XRGeneralSettingsAssetPath != null)
            {
                return s_XRGeneralSettingsAssetPath;
            }

            XRGeneralSettingsPerBuildTarget result = null;
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out result);
            if (result == null)
            {
                return k_XRGeneralSettingsDefaultAssetPath;
            }

            s_XRGeneralSettingsAssetPath = AssetDatabase.GetAssetPath(result);
            return (s_XRGeneralSettingsAssetPath != null) ? s_XRGeneralSettingsAssetPath : k_XRGeneralSettingsDefaultAssetPath;
        }
    }
}
