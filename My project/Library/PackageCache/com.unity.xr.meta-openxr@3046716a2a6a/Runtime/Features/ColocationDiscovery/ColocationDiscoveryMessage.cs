using System;
using Unity.Collections;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// Represents the result of a Colocation Discovery message.
    /// </summary>
    public readonly struct ColocationDiscoveryMessage
    {
        /// <summary>
        /// The ID of the advertisement.
        /// </summary>
        public SerializableGuid advertisementId { get; }

        /// <summary>
        /// The data of the message being advertised.
        /// </summary>
        public NativeArray<byte> data { get; }

        /// <summary>
        /// Construct an instance.
        /// </summary>
        /// <param name="advertisementId">The ID of the advertisement.</param>
        /// <param name="data">The data of the message being advertised. Allocated with `Allocator.Temp` and will be
        /// disposed of at the end of the frame.</param>
        public ColocationDiscoveryMessage(SerializableGuid advertisementId, NativeArray<byte> data)
        {
            this.advertisementId = advertisementId;
            this.data = data;
        }
    }
}
