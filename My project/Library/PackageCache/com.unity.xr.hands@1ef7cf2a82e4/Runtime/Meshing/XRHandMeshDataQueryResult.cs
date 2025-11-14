using System;

namespace UnityEngine.XR.Hands.Meshing
{
    /// <summary>
    /// Low-level mesh data in <see cref="XRHandMeshData"/> for each hand, useful for
    /// rendering and other use cases.
    /// </summary>
    public struct XRHandMeshDataQueryResult : IDisposable
    {
        /// <summary>
        /// Dispose of array data in the contained objects.
        /// </summary>
        public void Dispose()
        {
            leftHand.Dispose();
            rightHand.Dispose();
        }

        /// <summary>
        /// Low-level mesh data for left hand. It is up to the caller whether
        /// and how to turn the data into a <see cref="Mesh"/> or bake into
        /// physics data.
        /// </summary>
        /// <value>
        /// Data can only be valid if the call to <see cref="XRHandSubsystem.TryGetMeshData"/>
        /// was successful.
        /// </value>
        public XRHandMeshData leftHand { get; internal set; }

        /// <summary>
        /// Low-level mesh data for right hand. It is up to the caller whether
        /// and how to turn the data into a <see cref="Mesh"/> or bake into
        /// physics data.
        /// </summary>
        /// <value>
        /// Data can only be valid if the call to <see cref="XRHandSubsystem.TryGetMeshData"/>
        /// was successful.
        /// </value>
        public XRHandMeshData rightHand { get; internal set; }
    }

    namespace ProviderImplementation
    {
        /// <summary>
        /// Contains extensions to <see cref="XRHandMeshData"/> relevant
        /// to supplying data when <see cref="XRHandSubsystem.TryGetMeshData"/> is called.
        /// </summary>
        public static class XRHandMeshDataQueryResultExtensions
        {
            /// <summary>
            /// Flush <see cref="XRHandMeshData"/> changes.
            /// </summary>
            /// <param name="result">
            /// Mesh data query result to flush mesh data to.
            /// </param>
            /// <param name="meshData">
            /// Mesh data to flush to the query result.
            /// </param>
            /// <remarks>
            /// You should call this for each hand that has valid data. In
            /// other words,i.e., if both hands have any valid data, this
            /// should be called with both your altered copies of
            /// <see cref="XRHandMeshDataQueryResult.leftHand"/> and
            /// <see cref="XRHandMeshDataQueryResult.rightHand"/>.
            /// </remarks>
            public static void FlushChanges(this ref XRHandMeshDataQueryResult result, XRHandMeshData meshData)
            {
                if (meshData.handedness == Handedness.Left)
                    result.leftHand = meshData;
                else if (meshData.handedness == Handedness.Right)
                    result.rightHand = meshData;
                else
                    throw new ArgumentException("Invalid XRHandMeshData object - you should only set data on and flush one retrieved from a XRHandMeshDataQueryResult!", nameof(meshData));
            }
        }
    }
}
