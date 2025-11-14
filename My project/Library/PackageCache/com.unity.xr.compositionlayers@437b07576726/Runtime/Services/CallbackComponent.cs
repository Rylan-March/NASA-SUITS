using System;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Services
{
    /// <summary>
    /// MonoBehaviour used the drive the <see cref="CompositionLayerManager" />.
    /// There should be one and only one instance of this at any one time.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    class CallbackComponent : MonoBehaviour
    {
        /// <summary> Called at end of <see cref="Awake"/> </summary>
        internal Action OnAwake;

        /// <summary> Called at end of  <see cref="Update"/> </summary>
        internal Action OnUpdate;

        /// <summary> Called at end of  <see cref="LateUpdate"/> </summary>
        internal Action OnLateUpdate;

        void Awake()
        {
            OnAwake?.Invoke();
        }

        void Update()
        {
            OnUpdate?.Invoke();
        }

        void LateUpdate()
        {
            OnLateUpdate?.Invoke();
        }
    }
}
