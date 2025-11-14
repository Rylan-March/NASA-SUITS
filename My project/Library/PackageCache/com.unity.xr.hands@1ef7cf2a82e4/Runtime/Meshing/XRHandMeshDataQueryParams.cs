using Unity.Collections;

namespace UnityEngine.XR.Hands.Meshing
{
    /// <summary>
    /// Input parameters used when querying hand mesh data from <see cref="XRHandSubsystem.TryGetMeshData"/>.
    /// </summary>
    public struct XRHandMeshDataQueryParams
    {
        /// <summary>
        /// <see cref="Allocator"/> to be used when allocating the
        /// <see cref="NativeArray"/>s in <see cref="XRHandMeshData"/>.
        /// </summary>
        public Allocator allocator { get; set; }
    }
}
