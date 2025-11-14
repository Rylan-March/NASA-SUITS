using UnityEngine;

namespace Unity.XR.CompositionLayers.Layers
{
    internal static class CompositionLayerHelper
    {
        public static void DestroyCamera(Camera camera)
        {
            if (camera == null)
                return;

#if UNITY_RENDER_PIPELINES_UNIVERSAL
            DestroyObject(camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>());
#endif
#if UNITY_RENDER_PIPELINES_HDRENDER
            DestroyObject(camera.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>());
#endif

            DestroyObject(camera);
        }

        public static void DestroyObject(Object obj)
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
                Component.Destroy(obj);
            else
                Component.DestroyImmediate(obj);
        }
    }
}
