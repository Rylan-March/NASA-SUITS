using UnityEngine.Scripting;
using System.Runtime.InteropServices;
using UnityEngine.Android;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// The Android-OpenXR implementation of the <see cref="XRCameraSubsystem"/>.
    /// Do not create this directly. Use the <see cref="SubsystemManager"/> instead.
    /// </summary>
    [Preserve]
    public sealed class AndroidOpenXRCameraSubsystem : XRCameraSubsystem
    {
        internal const string k_SubsystemId = "Android-Camera";

        /// <summary>
        /// Attempts to retrieve the current state of the device's passthrough camera.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        public Result<ARPassthroughCameraState> GetPassthroughCameraState()
        {
            var xrResult = ((AndroidOpenXRProvider)provider).GetPassthroughCameraState(out var cameraState);
            return new Result<ARPassthroughCameraState>(new XRResultStatus((int)xrResult), cameraState);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            XRCameraSubsystemDescriptor.Register(new XRCameraSubsystemDescriptor.Cinfo
            {
                id = k_SubsystemId,
                providerType = typeof(AndroidOpenXRProvider),
                subsystemTypeOverride = typeof(AndroidOpenXRCameraSubsystem),
                supportsAverageBrightness = true,
                supportsAverageColorTemperature = true,
                supportsColorCorrection = true,
                supportsDisplayMatrix = false,
                supportsProjectionMatrix = false,
                supportsTimestamp = false,
                supportsCameraConfigurations = false,
                supportsCameraImage = false,
                supportsAverageIntensityInLumens = true,
                supportsFocusModes = false,
                supportsFaceTrackingAmbientIntensityLightEstimation = false,
                supportsFaceTrackingHDRLightEstimation = false,
                supportsWorldTrackingAmbientIntensityLightEstimation = true,
                supportsWorldTrackingHDRLightEstimation = true,
                supportsCameraGrain = false,
            });
        }

        class AndroidOpenXRProvider : Provider
        {
            readonly string k_CameraStateNotAvailableError = $"Unable to get camera state. OpenXR extension ({Constants.OpenXRExtensions.k_XR_ANDROID_passthrough_camera_state}) is not available on the current device.";

            /// <summary>
            /// Get or set the requested light estimation mode.
            /// </summary>
            public override Feature requestedLightEstimation
            {
                get => NativeApi.GetRequestedLightEstimation();
                set
                {
                    NativeApi.SetRequestedLightEstimation(value);
                }
            }

            /// <summary>
            /// Get the current light estimation mode.
            /// </summary>
            public override Feature currentLightEstimation => NativeApi.GetCurrentLightEstimation();

            public AndroidOpenXRProvider()
            {
                NativeApi.Create();
            }

            public override void Destroy()
            {
                NativeApi.Destroy();
            }

            /// <summary>
            /// Start the camera functionality.
            /// </summary>
            public override void Start()
            {
                ARCameraFeature.SetPassthrough(true);
#if UNITY_ANDROID
                if (!Permission.HasUserAuthorizedPermission(Constants.Permissions.k_SceneUnderstandingFinePermission)
                    && !Permission.HasUserAuthorizedPermission(Constants.Permissions.k_SceneUnderstandingCoarsePermission))
                {
                    Debug.LogWarning($"AR camera light estimation requires system permission {Constants.Permissions.k_SceneUnderstandingCoarsePermission}, but permission was not granted." +
                    " Camera passthrough will not be broken by this missing permission.");
                }
#endif
            }

            /// <summary>
            /// Stop the camera functionality.
            /// </summary>
            public override void Stop()
            {
                ARCameraFeature.SetPassthrough(false);
            }

            internal XrResult GetPassthroughCameraState(out ARPassthroughCameraState cameraState)
            {
                if (!OpenXRRuntime.IsExtensionEnabled(Constants.OpenXRExtensions.k_XR_ANDROID_passthrough_camera_state))
                {
                    Debug.LogError(k_CameraStateNotAvailableError);

                    cameraState = default;
                    return XrResult.ExtensionNotPresent;
                }

                return NativeApi.GetPassthroughCameraState(out cameraState);
            }

            /// <summary>
            /// Get the current camera frame for the subsystem.
            /// </summary>
            /// <param name="cameraParams">The current Unity <c>Camera</c> parameters.</param>
            /// <param name="cameraFrame">The current camera frame returned by the method.</param>
            /// <returns><see langword="true"/> if the method successfully got a frame. Otherwise, <see langword="false"/>.</returns>
            public override bool TryGetFrame(XRCameraParams cameraParams, out XRCameraFrame cameraFrame)
            {
                if (running && NativeApi.TryGetFrame(cameraParams, out cameraFrame))
                {
                    return true;
                }

                cameraFrame = default;
                return false;
            }
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Camera_Create")]
            public static extern void Create();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Camera_Destroy")]
            public static extern void Destroy();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Camera_TryGetFrame")]
            public static extern bool TryGetFrame(XRCameraParams cameraParams, out XRCameraFrame cameraFrame);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Camera_GetPassthroughCameraState")]
            public static extern XrResult GetPassthroughCameraState(out ARPassthroughCameraState cameraState);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Camera_GetCurrentLightEstimation")]
            public static extern Feature GetCurrentLightEstimation();
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Camera_GetRequestedLightEstimation")]
            public static extern Feature GetRequestedLightEstimation();
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_Camera_SetRequestedLightEstimation")]
            public static extern void SetRequestedLightEstimation(Feature requestedLightEstimation);
        }
    }
}
