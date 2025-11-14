using Unity.XR.CompositionLayers.UIInteraction;
using UnityEngine;

namespace Unity.XR.CompositionLayers.UIInteraction
{
    /// <summary>
    /// Custom gizmo creator attached to every UI element under a Composition UI Layer
    /// Handles position, rotation, and scale calculations to draw Gizmos
    /// </summary>
    [ExecuteInEditMode]
    public class UIHandle : MonoBehaviour
    {
        /// <summary>
        /// Reference to the parent Composition Layer's Transform
        /// </summary>
        public Transform CompositionLayerTransform { get => m_CompositionLayerTransform; }
        private Transform m_CompositionLayerTransform;

        /// <summary>
        /// Reference to the Canvas this element is attached to
        /// </summary>
        public Canvas Canvas { get => m_Canvas; }
        private Canvas m_Canvas;
        private RectTransform m_CanvasRectTransform;

        /// <summary>
        /// Reference to this element's RectTransform
        /// </summary>
        public RectTransform RectTransform { get => m_RectTransform; }
        private RectTransform m_RectTransform;

        /// <inheritdoc cref="MonoBehaviour"/>
        void OnEnable()
        {
            m_CompositionLayerTransform = GetComponentInParent<CompositionLayer>()?.transform;
            if (m_CompositionLayerTransform == null)
                m_CompositionLayerTransform = GetComponent<CompositionLayer>()?.transform;
            m_Canvas = GetComponentInParent<Canvas>();
            m_CanvasRectTransform = m_Canvas?.GetComponent<RectTransform>();
            m_RectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// Calculates the position of the RectTransform in the layer's local space
        /// </summary>
        /// <returns>The position of the RectTransform in the layer's local space</returns>
        public Vector3 GetHandlePosition()
        {
            if (!RectTransform && CompositionLayerTransform)
                return m_CompositionLayerTransform.position;

            float centerScale = 1.0f / GetMaxCanvasSize();
            Vector3 offsetRelativeToCanvas = m_RectTransform.position - m_CanvasRectTransform.position;
            Vector3 position = m_CompositionLayerTransform.position + offsetRelativeToCanvas * centerScale;
            return position;
        }

        /// <summary>
        /// Sets the position of the UI handle.
        /// </summary>
        /// <param name="worldPosition">The position in world space.</param>
        public void SetPosition(Vector3 worldPosition)
        {
            if (!RectTransform && CompositionLayerTransform)
                m_CompositionLayerTransform.position = worldPosition;
            else if (RectTransform)
                SetRectPosition(worldPosition);
        }

        /// <summary>
        /// Sets the position of the RectTransform
        /// </summary>
        /// <param name="worldPosition">The location in World Space</param>
        /// <remarks>
        /// Typically called with a return value from Handles.PositionHandle, Handles.RotationHandle, or Handles.ScaleHandle
        /// </remarks>
        public void SetRectPosition(Vector3 worldPosition)
        {
            float centerScale = 1.0f / GetMaxCanvasSize();
            Vector3 offsetRelativeToCompositionLayer = (worldPosition - m_CompositionLayerTransform.position) / centerScale;
            Vector3 localPosition = m_CanvasRectTransform.position + offsetRelativeToCompositionLayer;
            m_RectTransform.position = localPosition;
        }

        /// <summary>
        /// Calculates the rotation of the RectTransform in the layer's local space
        /// </summary>
        /// <returns>The RectTransform's rotation</returns>
        public Quaternion GetHandleRotation()
        {
            return RectTransform ? RectTransform.rotation : transform.rotation;
        }

        /// <summary>
        /// Calculates the scale of the RectTransform in the layer's local space
        /// </summary>
        /// <returns>The RectTransform's scale</returns>
        public Vector3 GetHandleScale()
        {
            return RectTransform ? RectTransform.localScale : transform.localScale;
        }

        /// <summary>
        /// Gets the maximum size of the canvas on the X or Y axis
        /// </summary>
        /// <param name="canvas"></param>
        /// <returns>Returns the maximum size on the X or Y axis of the canvas</returns>
        private float GetMaxCanvasSize() => Mathf.Max(m_CanvasRectTransform.sizeDelta.x, m_CanvasRectTransform.sizeDelta.y);
    }
}
