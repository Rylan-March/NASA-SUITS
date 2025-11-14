using System.Collections.Generic;
using Unity.XR.CompositionLayers.Layers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace Unity.XR.CompositionLayers.Emulation.Implementations
{
    /// <summary>
    /// Base for emulating <see cref="ProjectionLayerData"/>. Used to emulate a full screen texture rendering to the compositor.
    /// </summary>
    [EmulatedLayerDataType(typeof(ProjectionLayerData))]
    internal class ProjectionEmulationLayerData : EmulatedMeshLayerData
    {
        /// <inheritdoc/>
        public override bool IsSupported(Camera camera)
        {
            if (camera.cameraType == CameraType.SceneView)
                return true;

            var isSupported = !Application.isPlaying;
#if ENABLE_UNITY_VR
            isSupported = isSupported || !XRSettings.isDeviceActive;
#endif
            return isSupported;
        }

        protected override string GetShaderLayerTypeKeyword()
        {
            return "COMPOSITION_LAYERTYPE_PROJECTION";
        }

        protected override void UpdateMesh(ref Mesh mesh)
        {
            if (mesh == null)
            {
                mesh = GeneratePlaneMesh(1.0f);
            }
        }
    }
}
