using Unity.Collections;
using UnityEngine.XR.Hands.Meshing;

namespace UnityEngine.XR.Hands.OpenXR.Meshing
{
    /// <summary>
    /// Used by platform-specific OpenXR features to supply hand mesh data to
    /// <see cref="XRHandSubsystem"/>.
    /// </summary>
    /// <remarks>
    /// Only used by developers connecting an <c>OpenXRFeature</c> to
    /// <see cref="XRHandSubsystem.TryGetMeshData"/>. If you're a user making
    /// a game or app, you do not need to worry about this type.
    /// </remarks>
    public interface IOpenXRHandMeshDataSupplier
    {
        /// <summary>
        /// Attempt to retrieve hand mesh data from the platform. Only called when
        /// <see cref="XRHandSubsystem.TryGetMeshData"/> is called.
        /// </summary>
        /// <param name="result">
        /// Output data for hand meshes.
        /// </param>
        /// <param name="queryParams">
        /// Input data for hand meshes.
        /// </param>
        /// <returns>
        /// Returns <see langword="true"/> if successful and either hand has
        /// valid data. Otherwise, returns <see langword="false"/>.
        /// </returns>
        bool TryGetMeshData(ref XRHandMeshDataQueryResult result, ref XRHandMeshDataQueryParams queryParams);
    }
}
