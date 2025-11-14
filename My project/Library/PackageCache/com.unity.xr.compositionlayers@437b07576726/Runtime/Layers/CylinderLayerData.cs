using System;
using Unity.XR.CompositionLayers.Extensions;
using Unity.XR.CompositionLayers.Provider;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Layers
{
    /// <summary>
    /// Subclass of <see cref="LayerData" /> that defines a cylindrical
    /// area of the display that will be rendered with some assigned texture by the
    /// current <see cref="ILayerProvider" />. The cylinder is centered at the transform
    /// location.
    /// </summary>
    [Serializable]
    [CompositionLayerData(
        Provider = "Unity",
        Name = "Cylinder",
        Description = "Defines a cylindrical area of the display that will be rendered with some assigned texture. " +
            "The cylinder is centered at the transform location.",
        IconPath = CompositionLayerConstants.IconPath,
        InspectorIcon = "LayerCylinderColor",
        ListViewIcon = "LayerCylinder",
        SupportTransform = true,
        SuggestedExtenstionTypes = new[] { typeof(TexturesExtension) }
     )]
    [CompositionLayersHelpURL(typeof(CylinderLayerData))]
    public class CylinderLayerData : LayerData
    {

        [SerializeField]
        [Range(0f, (float)Math.PI * 2)]
        [Tooltip("The angle of the visible section of the cylinder, based at 0 radians, in the range of [0, 2Pi). " +
            "It grows symmetrically around the 0 radian angle.")]
        float m_CentralAngle = (float)Math.PI * 0.5f;

        [Min(0f)]
        [SerializeField]
        [Tooltip("Defines the radius of the cylindrical layer centered at the transform location.")]
        float m_Radius = 0.6366198f;

        bool m_MaintainAspectRatio = false;

        [Min(0.001f)]
        [SerializeField]
        [Tooltip("The ratio of the visible cylinder section width / height. " +
            "The height of the cylinder is given by: (cylinder radius x cylinder angle) / aspectRatio.")]
        float m_AspectRatio = 1f;

        [SerializeField]
        [Tooltip("Whether or not to apply the transform scale properties to the layer. " +
            "When true, the scale of the transform will be applied to the length of the arc, height, and radius respectively.")]
        bool m_ApplyTransformScale = true;

        /// <summary>
        /// Defines the radius of the cylindrical layer centered at the transform location.
        /// </summary>
        public float Radius
        {
            get => m_Radius;
            set => m_Radius = UpdateValue(m_Radius, value);
        }

        /// <summary>
        /// The angle of the visible section of the cylinder, based at 0 radians, in the range of [0, 2Pi).
        /// </summary>
        public float CentralAngle
        {
            get => Mathf.Clamp(m_CentralAngle, 0f, (float)Math.PI * 2);
            set => m_CentralAngle = UpdateValue(m_CentralAngle, value);
        }

        /// <summary>
        /// The angle of the visible section of the cylinder, based at 0 radians, in the range of [0, 360).
        /// </summary>
        public float CentralAngleInDegrees
        {
            get => m_CentralAngle * Mathf.Rad2Deg;
            set => CentralAngle = value * Mathf.Deg2Rad;
        }

        /// <summary>
        /// Option to make the aspect ration stay constant.
        /// </summary>
        public bool MaintainAspectRatio
        {
            get => m_MaintainAspectRatio;
            set => m_MaintainAspectRatio = UpdateValue(m_MaintainAspectRatio, value);
        }

        /// <summary>
        /// The ratio of the visible cylinder section width / height.
        /// </summary>
        public float AspectRatio
        {
            get => m_AspectRatio;
            set => m_AspectRatio = UpdateValue(m_AspectRatio, value);
        }

        /// <summary>
        /// Whether or not to apply the transform scale properties to the layer.
        /// When true, the scale of the transform will be applied to the length of the arc, height, and radius respectively.
        /// </summary>
        public bool ApplyTransformScale
        {
            get => m_ApplyTransformScale;
            set => m_ApplyTransformScale = UpdateValue(m_ApplyTransformScale, value);
        }

        /// <summary>
        /// Return re-calculated parameters based on whether or not apply the transform scale properties.
        /// </summary>
        /// <param name="scale">transform scale</param>
        /// <returns>Return re-calculated cylinder params.</returns>
        public Vector3 GetScaledSize(Vector3 scale)
        {
            if (m_ApplyTransformScale)
            {
                float radius = Radius * scale.z;
                float height = GetHeight() * scale.y;
                float angle = CentralAngle * scale.x / scale.z;
                float ratio = radius * angle / height;
                return new Vector3(radius, angle, ratio);
            }
            return Vector3.one;
        }

        /// <summary>
        /// Return height value of the cylinder layer.
        /// </summary>
        /// <returns>cylinder layer height value.</returns>
        public float GetHeight()
        {
            return Mathf.Max(Radius * CentralAngle / AspectRatio, 0f);
        }

        /// <summary>
        /// Return aspect ratio when changing height.
        /// </summary>
        /// <param name="height">cylinder layer height</param>
        /// <returns>Return re-calculated aspect ratio.</returns>
        public float CalculateAspectRatioFromHeight(float height)
        {
            if(Radius <= 0 || CentralAngle <= 0)
                return AspectRatio;

            return Mathf.Max(Radius * CentralAngle / Mathf.Max(height, 0f), 0.001f);
        }

        /// <summary>
        /// Return radius when changing height.
        /// </summary>
        /// <param name="height">cylinder layer height</param>
        /// <returns>Return re-calculated radius.</returns>
        public float CalculateRadiusFromHeight(float height)
        {
            if(CentralAngle <= 0)
                return Radius;

            return Mathf.Max(AspectRatio * height / CentralAngle, 0f);
        }

        /// <summary>
        /// Used to copy values from another layer data instance
        /// </summary>
        /// <inheritdoc/>
        public override void CopyFrom(LayerData layerData)
        {
            if (layerData is CylinderLayerData cylinderLayerData)
            {
                m_Radius = cylinderLayerData.Radius;
                m_CentralAngle = cylinderLayerData.CentralAngle;
                m_AspectRatio = cylinderLayerData.AspectRatio;
            }
        }
    }
}
