using System;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Layers
{
    /// <summary>
    /// This class is used to keep platform dependent properties.
    /// The specific platforms define additional attributes for this inherited class.
    /// </summary>
    [Serializable]
    public class PlatformLayerData
    {
        /// <summary>
        /// Serialize all SerializeField to text.
        /// </summary>
        /// <returns>Serialized text.</returns>
        public virtual string Serialize()
        {
            return JsonUtility.ToJson(this);
        }

        /// <summary>
        /// Deserialize all SerializeField from text.
        /// </summary>
        /// <param name="text">Serialized text.</param>
        public virtual void Deserialize(string text)
        {
            if (!string.IsNullOrEmpty(text))
                JsonUtility.FromJsonOverwrite(text, this);
        }

        /// <summary>
        /// Check to support binary serialize/deserialize.
        /// </summary>
        /// <returns>true if target platform supports binary serialize/deserialize.</returns>
        public virtual bool IsSupportedSerializeBinary()
        {
            return false;
        }

        /// <summary>
        /// Serialize all properties to binary.
        /// </summary>
        /// <returns>Serialized binary.</returns>
        public virtual int[] SerializeBinary()
        {
            return null;
        }

        /// <summary>
        /// Deserialize all properties from binary.
        /// </summary>
        /// <param name="binary">Serialized binary.</param>
        public virtual void DeserializeBinary(int[] binary)
        {
        }
    }
}
