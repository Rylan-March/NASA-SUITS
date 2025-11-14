using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.XR.CoreUtils;

#if BURST_PRESENT
using Unity.Burst;
#endif

namespace UnityEngine.XR.Hands
{
    public partial class XRHandSkeletonDriver
    {
        /// <summary>
        /// A boolean tracking whether the root transform is valid. This is calculated once when the root transform
        /// changes to avoid a null check every time the root is updated.
        /// </summary>
        [Obsolete("Use hasRootTransform instead.")]
        protected bool m_HasRootTransform;
    }
}
