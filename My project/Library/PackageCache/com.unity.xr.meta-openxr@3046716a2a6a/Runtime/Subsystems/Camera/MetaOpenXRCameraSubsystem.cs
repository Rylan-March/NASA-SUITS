using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.XR.CompositionLayers;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CompositionLayers.Services;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR.CompositionLayers;
using UnityEngine.XR.OpenXR.NativeTypes.Meta;
using UnityEngine.XR.OpenXR.NativeTypes;
using Unity.XR.CoreUtils;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// The Meta-OpenXR implementation of the <see cref="XRCameraSubsystem"/>.
    /// Do not create this directly. Use the <see cref="SubsystemManager"/> instead.
    /// </summary>
    public sealed class MetaOpenXRCameraSubsystem : XRCameraSubsystem
    {
        internal const string k_SubsystemId = "Meta-Camera";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            var cameraSubsystemCinfo = new XRCameraSubsystemDescriptor.Cinfo
            {
                id = k_SubsystemId,
                providerType = typeof(MetaOpenXRProvider),
                subsystemTypeOverride = typeof(MetaOpenXRCameraSubsystem),
                supportsAverageBrightness = false,
                supportsAverageColorTemperature = false,
                supportsColorCorrection = false,
                supportsDisplayMatrix = false,
                supportsProjectionMatrix = false,
                supportsTimestamp = false,
                supportsCameraConfigurations = false,
                supportsCameraImage = false,
                supportsAverageIntensityInLumens = false,
                supportsFocusModes = false,
                supportsFaceTrackingAmbientIntensityLightEstimation = false,
                supportsFaceTrackingHDRLightEstimation = false,
                supportsWorldTrackingAmbientIntensityLightEstimation = false,
                supportsWorldTrackingHDRLightEstimation = false,
                supportsCameraGrain = false,
            };

            XRCameraSubsystemDescriptor.Register(cameraSubsystemCinfo);
        }

        class MetaOpenXRProvider : Provider
        {
            protected override bool TryInitialize()
            {
                OpenXRLayerProvider.Started += CreateAndRegisterLayerHandler;
                return base.TryInitialize();
            }

            public override void Destroy()
            {
                OpenXRLayerProvider.Started -= CreateAndRegisterLayerHandler;
            }

            /// <summary>
            /// Start the camera functionality.
            /// </summary>
            public override void Start()
            {
                if (!IsPassthroughLayerActive())
                    CreatePassthroughLayer();

                var defaultLayer = FindCompositionLayerType<DefaultLayerData>(CompositionLayerManager.Instance.CompositionLayers);
                defaultLayer.LayerData.BlendType = BlendType.Premultiply;
            }

            public override void Stop()
            {
                if (IsPassthroughLayerActive())
                    DestroyPassthroughLayer();
            }

            protected void CreateAndRegisterLayerHandler()
            {
                var layerHandler = new MetaOpenXRPassthroughLayer();
                OpenXRLayerProvider.RegisterLayerHandler(typeof(PassthroughLayerData), layerHandler);
            }

            static bool IsPassthroughLayerActive()
            {
                var compositionLayerManager = CompositionLayerManager.Instance;
                if (compositionLayerManager == null)
                    return false;

                var layer = FindCompositionLayerType<PassthroughLayerData>(compositionLayerManager.CompositionLayers);
                return layer != null;
            }

            static void CreatePassthroughLayer()
            {
                var passthroughDescriptor = CompositionLayerUtils.GetLayerDescriptor(typeof(PassthroughLayerData));
                var passthroughGameObject = new GameObject("Passthrough");
                var compositionLayerComponent = passthroughGameObject.AddComponent<CompositionLayer>();
                var passthroughLayerData = CompositionLayerUtils.CreateLayerData(typeof(PassthroughLayerData));
                compositionLayerComponent.ChangeLayerDataType(passthroughLayerData);
                foreach (var extension in passthroughDescriptor.SuggestedExtensions)
                {
                    if (extension.IsSubclassOf(typeof(MonoBehaviour)))
                        passthroughGameObject.AddComponent(extension);
                }
                compositionLayerComponent.TryChangeLayerOrder(compositionLayerComponent.Order, CompositionLayerManager.GetFirstUnusedLayer(false));
            }

            static void DestroyPassthroughLayer()
            {
                if (CompositionLayerManager.Instance == null)
                    return;

                var passthroughLayer = FindCompositionLayerType<PassthroughLayerData>(CompositionLayerManager.Instance.CompositionLayers);
                if (passthroughLayer != null)
                    UnityObjectUtils.Destroy(passthroughLayer.gameObject);
            }

            static CompositionLayer FindCompositionLayerType<T>(IReadOnlyCollection<CompositionLayer> layers)
            {
                foreach (var layer in layers)
                {
                    if (layer != null && layer.LayerData.GetType() == typeof(T))
                        return layer;
                }

                return null;
            }
        }

        internal static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern XrResult UnityMetaQuest_xrCreatePassthroughFB(
                ulong session, in XrPassthroughCreateInfoFB createInfo, out ulong passthroughHandle);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern XrResult UnityMetaQuest_xrCreatePassthroughLayerFB(
                ulong session, in XrPassthroughLayerCreateInfoFB createInfo, out ulong passthroughLayerHandle);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern XrResult UnityMetaQuest_xrPassthroughStartFB(ulong passthroughHandle);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern XrResult UnityMetaQuest_xrPassthroughPauseFB(ulong passthroughHandle);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern XrResult UnityMetaQuest_xrDestroyPassthroughLayerFB(ulong passthroughLayerHandle);

            [DllImport(Constants.k_ARFoundationLibrary)]
            public static extern XrResult UnityMetaQuest_xrDestroyPassthroughFB(ulong passthroughHandle);
        }
    }
}
