using UnityEngine;

namespace Unity.XR.CompositionLayers.UIInteraction
{
    /// <summary>
    /// Helper class for InteractableUIMirror that helps with creating Render Textures of a world space canvas
    /// </summary>
    /// <seealso cref="InteractableUIMirror"/>
    internal class CameraTargetTextureFactory
    {
        const int MINIMUM_RENDER_TEXTURE_SIZE = 100;
        // Desired size of render texture
        private Vector2 renderTextureSize = new Vector2(500, 500);

        /// <summary>
        /// Releases the Camera's Render Texture, then recreates with new Rect size
        /// </summary>
        /// <param name="camera">Camera to assign texutre to</param>
        /// <param name="rect">Rect to render</param>
        public void ReplaceTargetTexture(Camera camera, Rect rect)
        {
            ReleaseTargetTexture(camera);
            CreateTargetTexture(camera, rect);
        }

        /// <summary>
        /// Creates a new render texture to fit the supplied Rect, then assigns it to the Camera
        /// </summary>
        /// <param name="camera">Camera to assign texture to</param>
        /// <param name="rect">Rect to render</param>
        /// <returns></returns>
        public RenderTexture CreateTargetTexture(Camera camera, Rect rect)
        {
            var largerDimension = Mathf.Max(rect.width, rect.height);
            var scale = Mathf.Max(1.0f, MINIMUM_RENDER_TEXTURE_SIZE / largerDimension);
            renderTextureSize = new Vector2(rect.width * scale, rect.height * scale);
            var rt = new RenderTexture((int)renderTextureSize.x, (int)renderTextureSize.y, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = rt;
            return rt;
        }

        /// <summary>
        /// Removes the current render texture assigned to the Camera
        /// </summary>
        /// <param name="camera">The camera to remove the texture from</param>
        /// <returns>Whether or not the texture was released</returns>
        public bool ReleaseTargetTexture(Camera camera)
        {
            if (camera == null || camera.targetTexture == null)
                return false;

            var renderTexture = camera.targetTexture;
            camera.targetTexture = null;
            renderTexture.Release();
            if (Application.isPlaying)
                Object.Destroy(renderTexture);
            else
                Object.DestroyImmediate(renderTexture);

            return true;
        }
    }
}
