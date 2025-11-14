using System.Collections.Generic;
using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CompositionLayers.Services;
using UnityEngine.XR.OpenXR.CompositionLayers;
using UnityEngine.XR.OpenXR.NativeTypes;
using UnityEngine.XR.OpenXR.NativeTypes.Meta;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    class MetaOpenXRPassthroughLayer : OpenXRCustomLayerHandler<XrCompositionLayerPassthroughFB>
    {
        static Dictionary<int, PassthroughDataContainer> s_LayerIdsToPassthroughData = new();

        struct PassthroughDataContainer
        {
            public bool passthroughStarted;
            public ulong passthroughHandle;
            public ulong passthroughLayerHandle;
        }

        protected override bool CreateSwapchain(
            CompositionLayerManager.LayerInfo layer,
            out SwapchainCreateInfo swapchainCreateInfo)
        {
            // Swapchain not needed for this layer, so we need to manually invoke OnCreatedSwapchain().
            swapchainCreateInfo = default;
            OnCreatedSwapchain(layer, default);
            return false;
        }

        protected override bool CreateNativeLayer(
            CompositionLayerManager.LayerInfo layerInfo,
            SwapchainCreatedOutput swapchainOutput,
            out XrCompositionLayerPassthroughFB nativeLayer)
        {
            // Remove any existing passthrough layer for this ID.
            if (s_LayerIdsToPassthroughData.ContainsKey(layerInfo.Id))
                RemoveLayer(layerInfo.Id);

            var session = OpenXRLayerUtility.GetXRSession();

            var passthroughCreateInfo = new XrPassthroughCreateInfoFB()
            {
                type = XrFbStructType.XR_TYPE_PASSTHROUGH_CREATE_INFO_FB,
                next = null,
                flags = (ulong)XrPassthroughFlagsFB.XR_PASSTHROUGH_IS_RUNNING_AT_CREATION_BIT_FB,
            };

            var result = MetaOpenXRCameraSubsystem.NativeApi.UnityMetaQuest_xrCreatePassthroughFB(
                session, in passthroughCreateInfo, out ulong passthroughHandle);

            if (result != XrResult.Success)
            {
                nativeLayer = default;
                return false;
            }

            var passthroughLayerCreateInfo = new XrPassthroughLayerCreateInfoFB
            {
                type = XrFbStructType.XR_TYPE_PASSTHROUGH_LAYER_CREATE_INFO_FB,
                next = null,
                passthroughHandle = passthroughHandle,
                flags = XrPassthroughFlagsFB.XR_PASSTHROUGH_IS_RUNNING_AT_CREATION_BIT_FB,
                purpose = XrPassthroughLayerPurposeFB.XR_PASSTHROUGH_LAYER_PURPOSE_RECONSTRUCTION_FB
            };

            result = MetaOpenXRCameraSubsystem.NativeApi.UnityMetaQuest_xrCreatePassthroughLayerFB(
                session, in passthroughLayerCreateInfo, out ulong passthroughLayerHandle);

            if (result != XrResult.Success)
            {
                nativeLayer = default;
                return false;
            }

            s_LayerIdsToPassthroughData.Add(
                layerInfo.Id,
                new PassthroughDataContainer
                {
                    passthroughStarted = true,
                    passthroughHandle = passthroughHandle,
                    passthroughLayerHandle = passthroughLayerHandle
                });

            unsafe
            {
                nativeLayer = new XrCompositionLayerPassthroughFB
                {
                    type = XrFbStructType.XR_TYPE_COMPOSITION_LAYER_PASSTHROUGH_FB,
                    next = OpenXRLayerUtility.GetExtensionsChain(
                        layerInfo, Unity.XR.CompositionLayers.CompositionLayerExtension.ExtensionTarget.Layer),
                    flags = layerInfo.Layer.LayerData.BlendType == BlendType.Premultiply ?
                        XrCompositionLayerFlags.SourceAlpha :
                        XrCompositionLayerFlags.SourceAlpha | XrCompositionLayerFlags.UnPremultipliedAlpha,
                    space = 0,
                    passthroughLayerHandle = passthroughLayerHandle
                };
            }

            return true;
        }

        protected override bool ModifyNativeLayer(CompositionLayerManager.LayerInfo layerInfo, ref XrCompositionLayerPassthroughFB nativeLayer)
        {
            if (!layerInfo.Layer.enabled)
            {
                var passthroughData = s_LayerIdsToPassthroughData[layerInfo.Id];
                var result = MetaOpenXRCameraSubsystem.NativeApi.UnityMetaQuest_xrPassthroughPauseFB(passthroughData.passthroughHandle);
                var success = result == XrResult.Success;
                if (success)
                    s_LayerIdsToPassthroughData[layerInfo.Id] = new PassthroughDataContainer
                    {
                        passthroughStarted = false,
                        passthroughHandle = passthroughData.passthroughHandle,
                        passthroughLayerHandle = passthroughData.passthroughLayerHandle
                    };

                return success;
            }

            return false;
        }

        protected override bool ActiveNativeLayer(CompositionLayerManager.LayerInfo layerInfo, ref XrCompositionLayerPassthroughFB nativeLayer)
        {
            if (s_LayerIdsToPassthroughData.TryGetValue(layerInfo.Id, out var passthroughData))
            {
                if (!passthroughData.passthroughStarted)
                {
                    var result = MetaOpenXRCameraSubsystem.NativeApi.UnityMetaQuest_xrPassthroughStartFB(passthroughData.passthroughHandle);
                    var success = result == XrResult.Success;
                    if (success)
                        s_LayerIdsToPassthroughData[layerInfo.Id] = new PassthroughDataContainer
                        {
                            passthroughStarted = true,
                            passthroughHandle = passthroughData.passthroughHandle,
                            passthroughLayerHandle = passthroughData.passthroughLayerHandle
                        };

                    return success;
                }
                return true;
            }
            return false;
        }

        public override void RemoveLayer(int removedLayerId)
        {
            if (s_LayerIdsToPassthroughData.TryGetValue(removedLayerId, out var passthroughData))
            {
                MetaOpenXRCameraSubsystem.NativeApi.UnityMetaQuest_xrPassthroughPauseFB(passthroughData.passthroughHandle);
                MetaOpenXRCameraSubsystem.NativeApi.UnityMetaQuest_xrDestroyPassthroughLayerFB(passthroughData.passthroughLayerHandle);
                MetaOpenXRCameraSubsystem.NativeApi.UnityMetaQuest_xrDestroyPassthroughFB(passthroughData.passthroughHandle);
            }

            s_LayerIdsToPassthroughData.Remove(removedLayerId);
            base.RemoveLayer(removedLayerId);
        }
    }
}
