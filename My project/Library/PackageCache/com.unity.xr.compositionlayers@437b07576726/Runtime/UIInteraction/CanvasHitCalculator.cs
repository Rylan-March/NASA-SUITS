#if UNITY_XR_INTERACTION_TOOLKIT
using Unity.XR.CompositionLayers.Layers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
#if UNITY_XR_INTERACTION_TOOLKIT_3_0
using UnityEngine.XR.Interaction.Toolkit.Interactors;
#endif

namespace Unity.XR.CompositionLayers.UIInteraction
{
    /// <summary>
    /// Helper class for transforming raycast hit's to the Composition Layer to the attached Canvas
    /// </summary>
    internal class CanvasHitCalculator
    {
        // The canvas to raycast
        private Canvas canvas;

        // The GameObject of the attached Composition Layer
        private GameObject compositionLayerGameObject;

        // the RectTransform of the attached Canvas
        private RectTransform canvasRectTransform;

        /// <summary>
        /// Initializer
        /// </summary>
        /// <param name="canvas">The canvas to raycast to</param>
        /// <param name="compositionLayerGameObject">The GameObject of the composition layer</param>
        public CanvasHitCalculator(Canvas canvas, GameObject compositionLayerGameObject)
        {
            this.canvas = canvas;
            this.compositionLayerGameObject = compositionLayerGameObject;
            canvasRectTransform = canvas.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Calculates location on canvas where the raycast hit
        /// </summary>
        /// <param name="interactor">Current interactor</param>
        /// <param name="hitPose">Calculated hit pose</param>
        /// <returns>whether or not the raycast hit the Composition Layer</returns>
        public bool CalculateCanvasHit(IXRRayProvider interactor, out Pose hitPose)
        {
            hitPose = Pose.identity;
            if (interactor.rayEndTransform)
            {
                var canvasHitPosition = RecalculateHitToCanvas(interactor.rayEndPoint);
                var pointerDistance = -0.1f;

                Vector3 pointerToCanvasLocalPosition = canvas.transform.TransformPoint(canvasHitPosition);
                Vector3 finalPosition = pointerToCanvasLocalPosition + (canvas.transform.forward * pointerDistance);
                Quaternion finalRotation = Quaternion.LookRotation(canvas.transform.forward);
                hitPose = new Pose(finalPosition, finalRotation);
                return true;
            }

            return false;
        }
        /// <summary>
        /// Calculates location on canvas where the raycast hit from a RaycastHit
        /// Calls propor function depending if layer is a Quad or a Cylinder as the calculations vary
        /// </summary>
        /// <param name="hit">The hit against the composition layer</param>
        /// <returns>The location on the canvas where the ray should hit</returns>
        private Vector3 RecalculateHitToCanvas(Vector3 raycastHit)
        {
            var recalculatedHit = Vector3.zero;

            if (!compositionLayerGameObject.TryGetComponent<CompositionLayer>(out var compositionLayer) || compositionLayer.LayerData == null)
                return recalculatedHit;

            switch (compositionLayer.LayerData)
            {
                case QuadLayerData quadLayerData:
                    recalculatedHit = RecalculateHitToCanvas(raycastHit, quadLayerData);
                    break;

                case CylinderLayerData cylinderLayerData:
                    recalculatedHit = RecalculateHitToCanvas(raycastHit, cylinderLayerData);
                    break;
            }

            return recalculatedHit;
        }

        /// <summary>
        /// Calculates location on canvas where the raycast hit from a RaycastHit
        /// </summary>
        /// <param name="raycastHit">The hit position against the composition layer</param>
        /// <param name="quadLayerData">The hit layer</param>
        /// <returns>The location on the canvas where the ray should hit</returns>
        private Vector3 RecalculateHitToCanvas(Vector3 raycastHit, QuadLayerData quadLayerData)
        {
            Vector2 quadSize = quadLayerData.GetScaledSize(compositionLayerGameObject.transform.lossyScale);
            QuadUIScale quadUIScale = compositionLayerGameObject.GetComponent<QuadUIScale>();

            Vector3 scale = quadLayerData.ApplyTransformScale ? compositionLayerGameObject.transform.lossyScale : Vector3.one;
            Vector3 localHit = compositionLayerGameObject.transform.InverseTransformPoint(raycastHit);
            Vector2 localHitPosition = new Vector2((-localHit.x) / (quadSize.x * 0.5f) / quadUIScale.WidthScale,
                                                (-localHit.y) / (quadSize.y * 0.5f) / quadUIScale.HeightScale);

            var canvasHitPosition = new Vector3(localHitPosition.x * canvasRectTransform.rect.x * scale.x, localHitPosition.y * canvasRectTransform.rect.y * scale.y, 0);

            return canvasHitPosition;
        }

        /// <summary>
        /// Calculates location on canvas where the raycast hit from a raycast hit position.
        /// </summary>
        /// <param name="raycastHit">The hit position against the composition layer</param>
        /// <param name="cylinderLayerData">The hit layer</param>
        /// <returns>The location on the canvas where the ray should hit</returns>
        private Vector3 RecalculateHitToCanvas(Vector3 raycastHit, CylinderLayerData cylinderLayerData)
        {
            CylinderUIScale cylinderUIScale = compositionLayerGameObject.GetComponent<CylinderUIScale>();

            Vector3 scale = compositionLayerGameObject.transform.lossyScale;
            Vector3 layerHitPoint = compositionLayerGameObject.transform.worldToLocalMatrix.MultiplyPoint3x4(raycastHit);
            Vector3 scaledHitPoint = new Vector3(layerHitPoint.x * scale.x, 0, layerHitPoint.z);

            // angle between hit point and cylinder center
            float angle = -Vector3.SignedAngle(scaledHitPoint.normalized, Vector3.forward, Vector3.up);

            // Invert the y height to be used as a scalar
            float yHeightInverted = 1.0f / (cylinderLayerData.GetHeight() / 2.0f);

            // Scale the hit point to a relative size of 1x1
            float centralAngleHalf = cylinderLayerData.CentralAngleInDegrees / 2.0f;
            Vector2 canvasPos = new Vector2(Remap(angle, -centralAngleHalf, centralAngleHalf, -1, 1), layerHitPoint.y * yHeightInverted);

            // Scale the hit point on the composition layer to the world space canvas
            var hitPosition = new Vector2(canvasPos.x * canvasRectTransform.sizeDelta.x / 2.0f / cylinderUIScale.WidthScale,
                                            canvasPos.y * canvasRectTransform.sizeDelta.y / 2.0f / cylinderUIScale.HeightScale);
            var canvasHitPosition = new Vector3(hitPosition.x * (1 / scale.x), hitPosition.y, 0);

            return canvasHitPosition;
        }

        /// <summary>
        /// Remaps a value from one range to another
        /// </summary>
        /// <param name="value">The value to remap</param>
        /// <param name="oldMin">The min of the first range</param>
        /// <param name="oldMax">the max of the first range</param>
        /// <param name="newMin">the min of the second range</param>
        /// <param name="newMax">the max of the second range</param>
        /// <returns>The new mapped value on the second range</returns>
        private float Remap(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
        }
    }
}
#endif
