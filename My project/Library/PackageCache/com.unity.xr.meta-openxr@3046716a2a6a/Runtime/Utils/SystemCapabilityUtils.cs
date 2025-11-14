using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.XR.OpenXR.Features.Meta.Constants.OpenXRExtensions;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    static class SystemCapabilityUtils
    {
        internal enum SystemCapability
        {
            SpaceDiscovery,
            SpacePersistence,
            SpatialEntity,
            Passthrough,
            BoundaryVisibility,
            EnvironmentDepth,
            HandRemoval,
            SharedAnchors,
            ColocationDiscovery,
        }

        internal struct SystemCapabilityInfo
        {
            internal readonly string extension;
            internal readonly Func<ulong, bool> getIsSupported;
            internal readonly Supported isSupported;

            internal SystemCapabilityInfo(string extension, Func<ulong, bool> getIsSupported)
            {
                this.extension = extension;
                this.getIsSupported = getIsSupported;
                isSupported = Supported.Unknown;
            }

            internal SystemCapabilityInfo(SystemCapabilityInfo info, bool isSupported)
            {
                extension = info.extension;
                getIsSupported = info.getIsSupported;
                this.isSupported = SupportedUtils.FromBool(isSupported);
            }
        }

        static List<SystemCapability> s_SystemCapabilities = new();
        static Dictionary<SystemCapability, SystemCapabilityInfo> s_InfosByCapability = new()
        {
            { SystemCapability.SpaceDiscovery, new(k_XR_META_spatial_entity_discovery, NativeApi.GetIsSpaceDiscoverySupported) },
            { SystemCapability.SpacePersistence, new(k_XR_META_spatial_entity_persistence, NativeApi.GetIsSpacePersistenceSupported) },
            { SystemCapability.SpatialEntity, new(k_XR_FB_spatial_entity, NativeApi.GetIsSpatialEntitySupported) },
            { SystemCapability.Passthrough, new(k_XR_FB_passthrough, NativeApi.GetIsPassthroughSupported) },
            { SystemCapability.BoundaryVisibility, new(k_XR_META_boundary_visibility, NativeApi.GetIsBoundaryVisibilitySupported) },
            { SystemCapability.EnvironmentDepth, new(k_XR_META_environment_depth, NativeApi.GetIsEnvironmentDepthSupported) },
            { SystemCapability.HandRemoval, new(k_XR_META_environment_depth, NativeApi.GetIsHandRemovalSupported) },
            { SystemCapability.SharedAnchors, new(k_XR_META_spatial_entity_group_sharing, NativeApi.GetIsSharedAnchorsSupported) },
            { SystemCapability.ColocationDiscovery, new(k_XR_META_colocation_discovery, NativeApi.GetIsColocationDiscoverySupported) },
        };

        static void ResetInfosByCapability()
        {
            s_SystemCapabilities.Clear();
            foreach (var sysCap in s_InfosByCapability.Keys)
            {
                s_SystemCapabilities.Add(sysCap);
            }

            foreach (var sysCap in s_SystemCapabilities)
            {
                var capInfo = s_InfosByCapability[sysCap];
                s_InfosByCapability[sysCap] = new(capInfo.extension, capInfo.getIsSupported);
            }
        }

        /// <summary>
        /// Get whether <paramref name="capability"/> is supported by the given OpenXR instance.
        /// </summary>
        /// <param name="capability">The system capability.</param>
        /// <param name="xrInstance">The OpenXR instance.</param>
        /// <param name="featureName">Optional OpenXR Feature that will be disabled if the required capability is not supported. Used for logs.</param>
        /// <param name="subsystemType">Optional subsystem that will not be created if the required capability is not supported. Used for logs.</param>
        /// <returns><see langword="true"/> if the capability is supported. Otherwise, <see langword="false"/>.</returns>
        internal static bool IsCapabilitySupported(SystemCapability capability, ulong xrInstance, string featureName = null, Type subsystemType = null)
        {
            InitializeCapabilityInfo(capability, xrInstance);
            var info = s_InfosByCapability[capability];
            if (info.isSupported == Supported.Supported)
                return true;

            if (featureName != null && subsystemType != null)
                Debug.Log($"This OpenXR runtime doesn't support {info.extension}. The {subsystemType.Name} won't be created, and <b>{featureName}</b> will be disabled.");
            else if (featureName != null)
                Debug.Log($"This OpenXR runtime doesn't support {info.extension}. <b>{featureName}</b> will be disabled.");

            return false;
        }

        internal static void InitializeCapabilityInfo(SystemCapability capability, ulong xrInstance)
        {
            // This never throws a KeyNotFoundException. Refer to SystemPropertiesUtilityTests.
            var info = s_InfosByCapability[capability];
            if (info.isSupported != Supported.Unknown)
                return;

            var isSupported = info.getIsSupported(xrInstance);
            var updatedInfo = new SystemCapabilityInfo(info, isSupported);
            s_InfosByCapability[capability] = updatedInfo;
        }

        internal static void ClearCachedCapabilityInfo()
        {
            ResetInfosByCapability();
        }

        internal static Dictionary<SystemCapability, SystemCapabilityInfo> GetCachedSystemCapabilities()
        {
            return s_InfosByCapability;
        }

        static class NativeApi
        {
            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_SystemProperties_IsBoundaryVisibilitySupported")]
            [return: MarshalAs(UnmanagedType.U1)]
            internal static extern bool GetIsBoundaryVisibilitySupported(ulong xrInstance);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_SystemProperties_IsEnvironmentDepthSupported")]
            [return: MarshalAs(UnmanagedType.U1)]
            internal static extern bool GetIsEnvironmentDepthSupported(ulong xrInstance);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_SystemProperties_IsHandRemovalSupported")]
            [return: MarshalAs(UnmanagedType.U1)]
            internal static extern bool GetIsHandRemovalSupported(ulong xrInstance);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_SystemProperties_IsPassthroughSupported")]
            [return: MarshalAs(UnmanagedType.U1)]
            internal static extern bool GetIsPassthroughSupported(ulong xrInstance);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_SystemProperties_IsSpaceDiscoverySupported")]
            [return: MarshalAs(UnmanagedType.U1)]
            internal static extern bool GetIsSpaceDiscoverySupported(ulong xrInstance);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_SystemProperties_IsSpacePersistenceSupported")]
            [return: MarshalAs(UnmanagedType.U1)]
            internal static extern bool GetIsSpacePersistenceSupported(ulong xrInstance);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_SystemProperties_IsSpatialEntitySupported")]
            [return: MarshalAs(UnmanagedType.U1)]
            internal static extern bool GetIsSpatialEntitySupported(ulong xrInstance);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_SystemProperties_IsSharedAnchorsSupported")]
            [return: MarshalAs(UnmanagedType.U1)]
            internal static extern bool GetIsSharedAnchorsSupported(ulong xrInstance);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRMeta_SystemProperties_IsColocationDiscoverySupported")]
            [return: MarshalAs(UnmanagedType.U1)]
            internal static extern bool GetIsColocationDiscoverySupported(ulong xrInstance);
        }
    }
}
