using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.SubsystemsImplementation.Extensions;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.Meshing;
using UnityEngine.XR.Hands.Meshing.ProviderImplementation;
using UnityEngine.XR.Hands.OpenXR;
using UnityEngine.XR.Hands.OpenXR.Meshing;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEngine.XR.OpenXR.Features.Android
{
    /// <summary>
    /// Enables access to Android XR performance metrics.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Android XR: Hand Mesh Data",
        BuildTargetGroups = new[] { BuildTargetGroup.Android },
        Company = Constants.k_CompanyName,
        Desc = "Access to Android XR performance metrics.",
        DocumentationLink = Constants.DocsUrls.k_HandMeshDataUrl,
        OpenxrExtensionStrings = k_OpenXRRequestedExtensions,
        Category = FeatureCategory.Feature,
        FeatureId = featureId,
        Version = "0.1.0")]
#endif
    public class AndroidXRHandMeshData : OpenXRFeature, IOpenXRHandMeshDataSupplier
    {
        /// <summary>
        /// The feature id string. This is used to give the feature a well known id for reference.
        /// </summary>
        public const string featureId = "com.unity.openxr.feature.androidxr-hand-mesh-data";

        /// <inheritdoc/>
        public unsafe bool TryGetMeshData(ref XRHandMeshDataQueryResult result, ref XRHandMeshDataQueryParams queryParams)
        {
            if (!m_SuccessfullyInitialized)
                return false;

            var successFlags = NativeApi.TryGetHandMeshData(
                out Pose leftRootPose, out Pose rightRootPose,
                out void* indicesPtrLeft, out int indicesLengthLeft,
                out void* indicesPtrRight, out int indicesLengthRight,
                out int vertexCountLeft, out int vertexCountRight,
                out void* positionsPtrLeft, out void* positionsPtrRight,
                out void* normalsPtrLeft, out void* normalsPtrRight,
                out void* uvsPtrLeft, out void* uvsPtrRight);

            if (successFlags == MeshDataSuccessFlags.Failed)
                return false;

            result.FlushChanges(ConvertToMeshData(
                result.leftHand,
                queryParams.allocator,
                indicesPtrLeft,
                indicesLengthLeft,
                vertexCountLeft,
                positionsPtrLeft,
                normalsPtrLeft,
                uvsPtrLeft,
                (successFlags & MeshDataSuccessFlags.LeftPoseValid) != 0,
                leftRootPose));

            result.FlushChanges(ConvertToMeshData(
                result.rightHand,
                queryParams.allocator,
                indicesPtrRight,
                indicesLengthRight,
                vertexCountRight,
                positionsPtrRight,
                normalsPtrRight,
                uvsPtrRight,
                (successFlags & MeshDataSuccessFlags.RightPoseValid) != 0,
                rightRootPose));

            NativeApi.ReleaseConvertedMeshData();
            return true;
        }

        unsafe XRHandMeshData ConvertToMeshData(
            XRHandMeshData meshData,
            Allocator allocator,
            void* indicesPtr,
            int indicesLength,
            int vertexCount,
            void* positionsPtr,
            void* normalsPtr,
            void* uvsPtr,
            bool isRootPoseValid,
            Pose rootPose)
        {
            var indices = new NativeArray<int>(indicesLength, allocator, NativeArrayOptions.UninitializedMemory);
            UnsafeUtility.MemCpy(indices.GetUnsafePtr(), indicesPtr, sizeof(int) * indicesLength);
            meshData.SetIndices(indices);

            var positions = new NativeArray<Vector3>(vertexCount, allocator, NativeArrayOptions.UninitializedMemory);
            UnsafeUtility.MemCpy(positions.GetUnsafePtr(), positionsPtr, sizeof(Vector3) * vertexCount);
            meshData.SetPositions(positions);

            if (normalsPtr != null)
            {
                var normals = new NativeArray<Vector3>(vertexCount, allocator, NativeArrayOptions.UninitializedMemory);
                UnsafeUtility.MemCpy(normals.GetUnsafePtr(), normalsPtr, sizeof(Vector3) * vertexCount);
                meshData.SetNormals(normals);
            }

            if (uvsPtr != null)
            {
                var uvs = new NativeArray<Vector2>(vertexCount, allocator, NativeArrayOptions.UninitializedMemory);
                UnsafeUtility.MemCpy(uvs.GetUnsafePtr(), uvsPtr, sizeof(Vector2) * vertexCount);
                meshData.SetUVs(uvs);
            }

            if (isRootPoseValid)
                meshData.SetRootPose(rootPose);
            else
                meshData.InvalidateRootPose();

            return meshData;
        }

        /// <summary>
        /// The set of OpenXR spec extension strings to enable, separated by spaces.
        /// </summary>
        const string k_OpenXRRequestedExtensions =
            Constants.OpenXRExtensions.k_XR_ANDROID_hand_mesh;

        /// <summary>
        /// Initializes native resources for surfacing hand mesh data.
        /// </summary>
        protected override void OnSubsystemStart()
        {
            if (m_SuccessfullyInitialized)
                return;

            bool foundOpenXRProvider = false;
            SubsystemManager.GetSubsystems(s_SubsystemsReuse);
            foreach (var subsystem in s_SubsystemsReuse)
            {
                var provider = subsystem.GetProvider() as OpenXRHandProvider;
                if (provider == null)
                    continue;

                provider.handMeshDataSupplier = this;
                foundOpenXRProvider = true;
                break;
            }

            if (!foundOpenXRProvider)
                Debug.LogWarning("Hand Tracking Subsystem feature is not enabled - subsystem APIs for hand mesh data will fail.");

            m_SuccessfullyInitialized = NativeApi.TryEnsureInitialized();
            if (!m_SuccessfullyInitialized)
                Debug.LogWarning("Failed to initialize native resources for surfacing AndroidXR hand mesh data.");
        }
        static List<XRHandSubsystem> s_SubsystemsReuse = new List<XRHandSubsystem>();

#if UNITY_EDITOR
        /// <summary>
        /// Validation Rules for AndroidXRHandMeshData feature.
        /// </summary>
        protected override void GetValidationChecks(List<ValidationRule> rules, BuildTargetGroup targetGroup)
        {
            var moreRules = new ValidationRule[]
            {
                new ValidationRule(this)
                {
                    message = "Android XR Hand Mesh Data requires that the Hand Tracking Subsystem feature be enabled.",
                    checkPredicate = () =>
                    {
                        return OpenXRSettings.ActiveBuildTargetInstance.GetFeature<HandTracking>()?.enabled ?? false;
                    },
                    fixItAutomatic = true,
                    fixItMessage = "Enable the Hand Tracking Subsystem feature.",
                    fixIt = () =>
                    {
                        OpenXRSettings.ActiveBuildTargetInstance.GetFeature<HandTracking>().enabled = true;
                    },
                    error = false,
                }
            };

            rules.AddRange(moreRules);
        }
#endif

        /// <summary>
        /// Destroys the native resources.
        /// </summary>
        /// <param name="xrSession">Handle of the xrSession</param>
        protected override void OnSessionDestroy(ulong xrSession)
        {
            NativeApi.Destroy();
            m_SuccessfullyInitialized = false;
        }

        bool m_SuccessfullyInitialized;

        [Flags]
        enum MeshDataSuccessFlags
        {
            Failed = 0,
            GeneralSuccess = 1 << 0,
            LeftPoseValid = 1 << 1,
            RightPoseValid = 1 << 2,
        }

        static class NativeApi
        {
            public static bool TryEnsureInitialized() => TryEnsureInitializedImpl() != 0;

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_HandMeshes_TryEnsureInitialized")]
            static extern int TryEnsureInitializedImpl();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_HandMeshes_Destroy")]
            public static extern void Destroy();

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_HandMeshes_TryGetHandMeshData")]
            public static extern unsafe MeshDataSuccessFlags TryGetHandMeshData(
                out Pose leftRootPose, out Pose rightRootPose,
                out void* indicesPtrLeft, out int indicesLengthLeft,
                out void* indicesPtrRight, out int indicesLengthRight,
                out int vertexCountLeft, out int vertexCountRight,
                out void* positionsPtrLeft, out void* positionsPtrRight,
                out void* normalsPtrLeft, out void* normalsPtrRight,
                out void* uvsPtrLeft, out void* uvsPtrRight);

            [DllImport(Constants.k_ARFoundationLibrary, EntryPoint = "UnityOpenXRAndroid_HandMeshes_ReleaseConvertedMeshData")]
            public static extern unsafe int ReleaseConvertedMeshData();
        }
    }
}
