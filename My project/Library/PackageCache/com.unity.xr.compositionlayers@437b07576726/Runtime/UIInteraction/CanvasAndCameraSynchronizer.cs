using UnityEngine;

namespace Unity.XR.CompositionLayers.UIInteraction
{
    /// <summary>
    /// Helper class for adjusting the camera bounds and render texture to match the canvas
    /// </summary>
    internal class CanvasAndCameraSynchronizer
    {
        // RectTransform of the Canvas
        private RectTransform canvasRectTransform;

        // Camera used to Render the Canvas
        private Camera canvasCamera;

        // Used to generate RenderTextures for the Camera
        private CameraTargetTextureFactory cameraTargetTextureFactory;

        // Cached scale to check for changes of the Canvas scale
        private Vector3 cachedCanvasLossyScale;

        // Cached Rect to check for changes of the Canvas Rect
        private Rect cachedCanvasRect;

        /// <summary>
        /// Initializer
        /// </summary>
        /// <param name="canvas">Canvas which should be rendered</param>
        /// <param name="canvasCamera">Camera to render the canvas</param>
        public CanvasAndCameraSynchronizer(Canvas canvas, Camera canvasCamera)
        {
            canvasRectTransform = canvas.GetComponent<RectTransform>();
            this.canvasCamera = canvasCamera;
            cameraTargetTextureFactory = new CameraTargetTextureFactory();
        }

        /// <summary>
        /// Checks whether or not there is a change in the Canvas which would require a new render texture
        /// </summary>
        /// <returns>True if the rect or scale has changed, false if neither have changed</returns>
        public bool Sync()
        {
            if (canvasRectTransform.rect != cachedCanvasRect || cachedCanvasLossyScale != canvasRectTransform.lossyScale)
            {
                SyncCameraViewWithCanvasSize();
                SyncRenderTextureWithCanvasSize();
                cachedCanvasRect = canvasRectTransform.rect;
                cachedCanvasLossyScale = canvasRectTransform.lossyScale;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Syncs the Camera bounds with the size of the Canvas
        /// </summary>
        private void SyncCameraViewWithCanvasSize()
        {
            float canvasWidth = canvasRectTransform.rect.width * canvasRectTransform.lossyScale.x;
            float canvasHeight = canvasRectTransform.rect.height * canvasRectTransform.lossyScale.y;
            canvasCamera.orthographicSize = canvasHeight * 0.5f;
            canvasCamera.aspect = canvasWidth / canvasHeight;
        }

        /// <summary>
        /// Regenerates the render texture for the canvas camera with the current canvas rect
        /// </summary>
        private void SyncRenderTextureWithCanvasSize()
        {
            cameraTargetTextureFactory.ReplaceTargetTexture(canvasCamera, canvasRectTransform.rect);
        }
    }
}
