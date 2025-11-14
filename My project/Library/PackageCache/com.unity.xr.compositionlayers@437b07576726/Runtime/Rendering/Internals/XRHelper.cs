using UnityEngine.XR.Management;

namespace UnityEngine.XR.CompositionLayers.Rendering.Internals
{
    internal static class XRHelper
    {
        public static bool GetDeviceConnected()
        {
            var manager = XRGeneralSettings.Instance?.Manager;
            var activeLoader = manager?.activeLoader;
            return (activeLoader != null) ? manager.isInitializationComplete : false;
        }

        public static XRLoader GetActiveLoader()
        {
            return XRGeneralSettings.Instance?.Manager?.activeLoader;
        }

        public static XRDisplaySubsystem GetActiveDisplaySubsystem()
        {
            var displaySubsystem = GetActiveLoader()?.GetLoadedSubsystem<XRDisplaySubsystem>();
            return displaySubsystem != null && displaySubsystem.running ? displaySubsystem : null;
        }
    }
}
