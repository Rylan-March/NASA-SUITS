#if UNITY_RENDER_PIPELINES_HDRENDER
using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Emulation
{
    internal static class EmulationLayerHighDefinitionCustomPassImpl
    {
        internal static void Execute(CustomPassContext ctx, List<EmulatedLayerData> emulationLayers)
        {
            var commandArgs = new EmulatedLayerData.CommandArgs(ctx.hdCamera.camera);
            ctx.cmd.DisableShaderKeyword("COMPOSITION_LAYERS_OVERRIDE_SHADER_VARIABLES_GLOBAL");
            foreach (var layerData in emulationLayers)
            {
                if (layerData.IsSupported(ctx.hdCamera.camera))
                {
                    layerData.AddToCommandBuffer(ctx.cmd, commandArgs);
                }
            }
        }
    }

    public class EmulationLayerHighDefinitionUnderlayCustomPass : CustomPass
    {
        protected override void Execute(CustomPassContext ctx)
        {
            EmulationLayerHighDefinitionCustomPassImpl.Execute(ctx, EmulationLayerScriptableRendererManager.GetEmulatedLayerDataList(false));
        }
    }

    public class EmulationLayerHighDefinitionOverlayCustomPass : CustomPass
    {
        protected override void Execute(CustomPassContext ctx)
        {
            EmulationLayerHighDefinitionCustomPassImpl.Execute(ctx, EmulationLayerScriptableRendererManager.GetEmulatedLayerDataList(true));
        }
    }

    [ExecuteInEditMode]
    internal class EmulationLayerHighDefinitionVolumeManager : MonoBehaviour
    {
        static EmulationLayerHighDefinitionVolumeManager _instance = null;

        static void InternalDestroy(Object obj)
        {
            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }

        static EmulationLayerHighDefinitionVolumeManager instance
        {
            get
            {
                if (_instance == null)
                {
                    var instances = Resources.FindObjectsOfTypeAll<EmulationLayerHighDefinitionVolumeManager>();
                    if (instances != null && instances.Length > 0)
                    {
                        _instance = instances[0];
                        for (int i = 1; i < instances.Length; ++i)
                        {
                            InternalDestroy(instances[i]);
                        }
                    }
                    else
                    {
                        var gameObject = new GameObject(typeof(EmulationLayerHighDefinitionVolumeManager).ToString());
                        gameObject.hideFlags = HideFlags.HideAndDontSave;
                        if (Application.isPlaying)
                            GameObject.DontDestroyOnLoad(gameObject);

                        _instance = gameObject.AddComponent<EmulationLayerHighDefinitionVolumeManager>();
                    }
                }

                return _instance;
            }
        }

        public static void ActivateCustomPassVolumes()
        {
            bool foundOverlay = false, foundUnderlay = false;
            var volumes = instance.GetComponents<CustomPassVolume>();
            if (volumes != null)
            {
                foreach (var volume in volumes)
                {
                    var customPasses = volume.customPasses;
                    foreach (var customPass in customPasses)
                    {
                        if (customPass is EmulationLayerHighDefinitionUnderlayCustomPass)
                        {
                            foundUnderlay = true;
                        }
                        else if (customPass is EmulationLayerHighDefinitionOverlayCustomPass)
                        {
                            foundOverlay = true;
                        }
                    }
                }
            }

            if (!foundUnderlay)
            {
                var volume = instance.gameObject.AddComponent<CustomPassVolume>();
                volume.isGlobal = true;
                volume.injectionPoint = CustomPassInjectionPoint.BeforeRendering;
                volume.customPasses.Add(new EmulationLayerHighDefinitionUnderlayCustomPass());
            }
            if (!foundOverlay)
            {
                var volume = instance.gameObject.AddComponent<CustomPassVolume>();
                volume.isGlobal = true;
                volume.injectionPoint = CustomPassInjectionPoint.AfterPostProcess;
                volume.customPasses.Add(new EmulationLayerHighDefinitionOverlayCustomPass());
            }
        }

        public static void DeactivateCustomPassVolumes()
        {
            if (_instance)
            {
                var volumes = _instance.GetComponents<CustomPassVolume>();
                if (volumes != null)
                {
                    foreach (var volume in volumes)
                    {
                        InternalDestroy(volume);
                    }
                }

                InternalDestroy(_instance.gameObject);
                _instance = null;
            }
        }
    }
}
#endif
