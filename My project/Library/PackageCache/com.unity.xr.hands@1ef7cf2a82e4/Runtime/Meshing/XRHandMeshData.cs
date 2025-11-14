using System;
using Unity.Collections;

namespace UnityEngine.XR.Hands.Meshing
{
    /// <summary>
    /// Low-level meshing data. If the call that retrieved this from
    /// <see cref="XRHandSubsystem.TryGetMeshData"/> was successful,
    /// <see cref="XRHandMeshData.positions"/>, <see cref="XRHandMeshData.normals"/>,
    /// and <see cref="XRHandMeshData.uvs"/> will all have the same length.
    /// </summary>
    public struct XRHandMeshData : IDisposable
    {
        /// <summary>
        /// Dispose of array data in this object.
        /// </summary>
        /// <remarks>
        /// This method is idempotent, meaning it doesn't matter if it gets
        /// duplicate calls. Since <see cref="XRHandMeshDataQueryResult"/>'s
        /// <see cref="XRHandMeshDataQueryResult.Dispose"/> calls this, that
        /// that means <c>Dispose</c> is safe to call on just <see cref="XRHandMeshDataQueryResult.leftHand"/>
        /// and <see cref="XRHandMeshDataQueryResult.rightHand"/>, just the
        /// containing <see cref="XRHandMeshDataQueryResult"/>, or both sets.
        /// </remarks>
        public void Dispose()
        {
            if (indices.IsCreated)
                indices.Dispose();

            if (positions.IsCreated)
                positions.Dispose();

            if (normals.IsCreated)
                normals.Dispose();

            if (uvs.IsCreated)
                uvs.Dispose();
        }

        /// <summary>
        /// Indices into the other arrays in this type for triangle data.
        /// </summary>
        /// <value>
        /// Can only be valid if the call to <see cref="XRHandSubsystem.TryGetMeshData"/>
        /// that retrieved this way successful, but may still not be valid, so check
        /// its <c>IsCreated</c> property before use.
        /// </value>
        public NativeArray<int> indices { get; internal set; }

        /// <summary>
        /// Positions of vertices, in session space.
        /// </summary>
        /// <value>
        /// Can only be valid if the call to <see cref="XRHandSubsystem.TryGetMeshData"/>
        /// that retrieved this way successful, but may still not be valid, so check
        /// its <c>IsCreated</c> property before use.
        /// </value>
        public NativeArray<Vector3> positions { get; internal set; }

        /// <summary>
        /// Normals of vertices.
        /// </summary>
        /// <value>
        /// Can only be valid if the call to <see cref="XRHandSubsystem.TryGetMeshData"/>
        /// that retrieved this way successful, but may still not be valid, so check
        /// its <c>IsCreated</c> property before use.
        /// </value>
        public NativeArray<Vector3> normals { get; internal set; }

        /// <summary>
        /// Texture UV coordinates of vertices.
        /// </summary>
        /// <value>
        /// Can only be valid if the call to <see cref="XRHandSubsystem.TryGetMeshData"/>
        /// that retrieved this way successful, but may still not be valid, so check
        /// its <c>IsCreated</c> property before use.
        /// </value>
        public NativeArray<Vector2> uvs { get; internal set; }

        /// <summary>
        /// Represents which hand this mesh data represents.
        /// </summary>
        /// <value>
        /// Right, left, or invalid.
        /// </value>
        public Handedness handedness { get; internal set; }

        /// <summary>
        /// Retrieves root pose, if this frame's data had one available.
        /// </summary>
        /// <param name="rootPose">
        /// If this function succeeds, this will be filled out with the root
        /// <see cref="Pose"/> for how the mesh data in this object is intended
        /// to be drawn. This pose should not be used otherwise.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and the root pose was
        /// filled out, returns <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// If this function fails, you should continue to use the previous pose.
        /// If there has been no successful pose retrieval yet, the closest
        /// approximation would be the <see cref="Pose"/> of the wrist joint.
        /// </remarks>
        public bool TryGetRootPose(out Pose rootPose)
        {
            rootPose = m_IsRootPoseValid ? m_RootPose : Pose.identity;
            return m_IsRootPoseValid;
        }

        internal XRHandMeshData(Handedness handedness)
        {
            this.handedness = handedness;
            indices = default;
            positions = default;
            normals = default;
            uvs = default;
            m_RootPose = Pose.identity;
            m_IsRootPoseValid = false;
        }

        internal void SetRootPose(Pose rootPose)
        {
            m_RootPose = rootPose;
            m_IsRootPoseValid = true;
        }

        internal void InvalidateRootPose() => m_IsRootPoseValid = false;

        Pose m_RootPose;
        internal bool m_IsRootPoseValid;
    }

    namespace ProviderImplementation
    {
        /// <summary>
        /// Contains extensions to <see cref="XRHandMeshData"/> relevant
        /// to supplying data when <see cref="XRHandSubsystem.TryGetMeshData"/> is called.
        /// </summary>
        public static class XRHandMeshDataExtensions
        {
            /// <summary>
            /// Set the <see cref="XRHandMeshData.indices"/> data on the <see cref="XRHandMeshData"/>.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to set index data on.
            /// </param>
            /// <param name="indices">
            /// Index data to set on the invoking mesh data.
            /// </param>
            public static void SetIndices(this ref XRHandMeshData meshData, NativeArray<int> indices)
                => meshData.indices = indices;

            /// <summary>
            /// Set the <see cref="XRHandMeshData.positions"/> data on the <see cref="XRHandMeshData"/>.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to set position data on.
            /// </param>
            /// <param name="positions">
            /// Index data to set on the invoking mesh data.
            /// </param>
            public static void SetPositions(this ref XRHandMeshData meshData, NativeArray<Vector3> positions)
                => meshData.positions = positions;

            /// <summary>
            /// Set the <see cref="XRHandMeshData.normals"/> data on the <see cref="XRHandMeshData"/>.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to set normal data on.
            /// </param>
            /// <param name="normals">
            /// Index data to set on the invoking mesh data.
            /// </param>
            public static void SetNormals(this ref XRHandMeshData meshData, NativeArray<Vector3> normals)
                => meshData.normals = normals;

            /// <summary>
            /// Set the <see cref="XRHandMeshData.uvs"/> data on the <see cref="XRHandMeshData"/>.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to set texture coordinate data on.
            /// </param>
            /// <param name="uvs">
            /// Texture coordinate data to set on the invoking mesh data.
            /// </param>
            public static void SetUVs(this ref XRHandMeshData meshData, NativeArray<Vector2> uvs)
                => meshData.uvs = uvs;

            /// <summary>
            /// Set the root <see cref="Pose"/> on the <see cref="XRHandMeshData"/>.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to set index data on.
            /// </param>
            /// <param name="rootPose">
            /// Root pose to set on the invoking mesh data.
            /// </param>
            public static void SetRootPose(this ref XRHandMeshData meshData, Pose rootPose)
                => meshData.SetRootPose(rootPose);

            /// <summary>
            /// Invalidates the root <see cref="Pose"/> on the <see cref="XRHandMeshData"/>.
            /// </summary>
            /// <param name="meshData">
            /// Mesh data to invalidate the root pose on.
            /// </param>
            public static void InvalidateRootPose(this ref XRHandMeshData meshData)
                => meshData.InvalidateRootPose();
        }
    }
}
