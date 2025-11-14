using System;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Layers
{
    /// <summary>
    /// Custom transform matrix type.
    /// Defines how to handle the transform matrix to be passed to the shader.
    /// </summary>
    public enum CustomTransformMatrixType
    {
        /// <summary>
        /// Matrix is treated as Model matrix.
        /// Used for normal transforms.
        /// This is treated as equivalent as compositionLayer.transform.localToWorldMatrix or UNITY_MATRIX_M.
        /// </summary>
        Model,
        /// <summary>
        /// Matrix is treated as ModelView matrix.
        /// Used to bypass view matrix or pass a special view matrix.
        /// This is treated as equivalent as mainCamera.transform.localToWorldMatirx * compositionLayer.transform.localToWorldMatrix or UNITY_MATRIX_MV.
        /// </summary>
        ModelView,
    }

    /// <summary>
    /// Custom transform data.
    /// This value is used for custom transforming on mesh layer data. (Quad, Cylinder, EquirectMesh and CubeProjection.)
    /// </summary>
    public struct CustomTransformData
    {
        /// <summary>
        /// Transform matrix. This is treated as a model matrix by default.
        /// </summary>
        public Matrix4x4 Matrix;

        /// <summary>
        /// Indicate matrix type.
        /// </summary>
        public CustomTransformMatrixType MatrixType;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="matrix">Matrix for transforming.</param>
        /// <param name="matrixType">Indicate matrix type.</param>
        public CustomTransformData(Matrix4x4 matrix, CustomTransformMatrixType matrixType = CustomTransformMatrixType.Model)
        {
            this.Matrix = matrix;
            this.MatrixType = matrixType;
        }

        /// <summary>
        /// Create a default instance.
        /// </summary>
        /// <value>Return a default instance. Matrix is set to identity.</value>
        public static CustomTransformData Default
        {
            get => new CustomTransformData(Matrix4x4.identity, CustomTransformMatrixType.Model);
        }

        /// <summary>
        /// Get world pose matrix. Scale value is ignoired.
        /// </summary>
        /// <param name="transform">transform to translate</param>
        /// <returns>The world pose matrix for transforming.</returns>
        static Matrix4x4 GetWorldPoseMatrix(Transform transform)
        {
            var pose = transform != null ? transform.GetWorldPose() : new Pose(Vector3.zero, Quaternion.identity);
            return Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one);
        }

        /// <summary>
        /// Get local pose matrix. Scale value is ignoired.
        /// </summary>
        /// <param name="transform">transform to translate</param>
        /// <returns>The local pose matrix for transforming.</returns>
        static Matrix4x4 GetLocalPoseMatrix(Transform transform)
        {
            var pose = transform != null ? transform.GetLocalPose() : new Pose(Vector3.zero, Quaternion.identity);
            return Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one);
        }

        /// <summary>
        /// Set world pose matrix to Matrix. Scale value is ignoired.
        /// </summary>
        /// <param name="transform">transform to translate</param>
        public void SetWorldPoseMatrix(Transform transform)
        {
            this.Matrix = GetWorldPoseMatrix(transform);
        }

        /// <summary>
        /// Set local pose matrix to Matrix. Scale value is ignoired.
        /// </summary>
        /// <param name="transform">transform to translate</param>
        public void SetLocalPoseMatrix(Transform transform)
        {
            this.Matrix = GetLocalPoseMatrix(transform);
        }

        /// <summary>
        /// Set local pose matrix to Matrix in Scene View. This function simulates the local pose transforming in Scene View.
        /// </summary>
        /// <param name="transform">transform to translate</param>
        /// <param name="mainCamera">main camera</param>
        public void SetLocalPoseMatrixInSceneView(Transform transform, Camera mainCamera)
        {
            if (mainCamera != null)
                this.Matrix = mainCamera.transform.localToWorldMatrix * GetLocalPoseMatrix(transform);
            else
                this.Matrix = GetWorldPoseMatrix(transform);
        }

        /// <summary>
        /// Check if two CustomTransformData are equal.
        /// </summary>
        /// <param name="obj">customTransformData to compare</param>
        /// <returns>Return true if equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            return Equals((CustomTransformData)obj);
        }

        /// <summary>
        /// Check if two CustomTransformData are equal.
        /// </summary>
        /// <param name="rhs">customTransformData to compare</param>
        /// <returns>Return true if equal.</returns>
        public bool Equals(CustomTransformData rhs)
        {
            return this.Matrix == rhs.Matrix && this.MatrixType == rhs.MatrixType;
        }

        /// <summary>
        /// Get Hash Code.
        /// </summary>
        /// <returns>hash code.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Matrix.GetHashCode(), this.MatrixType.GetHashCode());
        }

        /// <summary>
        /// Return true if two CustomTransformDatas are equal .
        /// </summary>
        /// <param name="lhs">customTransformData to compare</param>
        /// <param name="rhs">customTransformData to compare</param>
        /// <returns>Return True if equal.</returns>
        public static bool operator ==(CustomTransformData lhs, CustomTransformData rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Return true if two CustomTransformDatas are not equal .
        /// </summary>
        /// <param name="lhs">customTransformData to compare</param>
        /// <param name="rhs">customTransformData to compare</param>
        /// <returns>Return True if not equal.</returns>
        public static bool operator !=(CustomTransformData lhs, CustomTransformData rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
