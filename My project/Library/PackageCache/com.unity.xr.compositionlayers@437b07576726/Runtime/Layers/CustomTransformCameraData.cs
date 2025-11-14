using System;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Layers
{
    /// <summary>
    /// Camera data for custom transform.
    /// </summary>
    public struct CustomTransformCameraData
    {
        /// <summary>
        /// Main camera.
        /// </summary>
        public Camera MainCamera;

        /// <summary>
        /// Scene view. (Editor only.)
        /// </summary>
        public bool IsSceneView;
    }
}
