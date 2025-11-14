using System;
using Unity.XR.CompositionLayers.Extensions;
using UnityEngine;

namespace Unity.XR.CompositionLayers.Layers
{
    /// <summary>
    /// Subclass of <see cref="LayerData" /> that defines a equirect layer in a scene.
    /// A equirect sphere is centered at the transform location with only its inside faces visible.
    /// </summary>
    [Serializable]
    [CompositionLayerData(
        Provider = "Unity",
        Name = "Equirect Mesh",
        IconPath = CompositionLayerConstants.IconPath,
        InspectorIcon = "LayerEquirect",
        ListViewIcon = "LayerEquirect",
        PreferOverlay = false,
        SupportTransform = true,
        Description = "Equirect mesh layer",
        SuggestedExtenstionTypes = new[] { typeof(TexturesExtension) }
     )]
    public class EquirectMeshLayerData : LayerData
    {
        const float k_HalfPi = Mathf.PI * 0.5f;
        const float k_NegativeHalfPi = -k_HalfPi;
        const float k_TwoPi = Mathf.PI * 2f;

        [SerializeField]
        [Tooltip("The non-negative radius of the sphere onto which the image data is mapped.")]
        float m_Radius = 100;

        [SerializeField]
        [Range(0, k_TwoPi)]
        [Tooltip("The visible horizontal angle of the sphere, based at 0 radians, in the range of [0, 2π].")]
        float m_CentralHorizontalAngle = Mathf.PI * 2;

        [SerializeField]
        [Range(k_NegativeHalfPi, k_HalfPi)]
        [Tooltip("The upper vertical angle of the visible portion of the sphere, in the range of [-π/2, π/2].")]
        float m_UpperVerticalAngle = k_HalfPi;

        [SerializeField]
        [Range(k_NegativeHalfPi, k_HalfPi)]
        [Tooltip("The lower vertical angle of the visible portion of the sphere, in the range of [-π/2, π/2].")]
        float m_LowerVerticalAngle = k_HalfPi;

        /// <summary>
        /// Defines radius of the sphere onto which the equirect image data is mapped.
        /// </summary>
        public float Radius
        {
            get
            {
                return m_Radius;
            }
            set
            {
                var newValue = Mathf.Max(0f, value);
                m_Radius = UpdateValue(m_Radius, newValue);
            }
        }

        /// <summary>
        /// Defines the visible horizontal angle of the sphere, based at 0 radians, in the range of [0, 2π].
        /// It grows symmetrically around the 0 radian angle.
        /// </summary>
        public float CentralHorizontalAngle
        {
            get => m_CentralHorizontalAngle;
            set => m_CentralHorizontalAngle = UpdateValue(m_CentralHorizontalAngle, value);
        }

        /// <summary>
        /// Defines the visible horizontal angle of the sphere, based at 0 radians, in the range of [0, 360].
        /// It grows symmetrically around the 0 radian angle.
        /// </summary>
        public float CentralHorizontalAngleInDegrees
        {
            get => m_CentralHorizontalAngle * Mathf.Rad2Deg;
            set => CentralHorizontalAngle = value * Mathf.Deg2Rad;
        }

        /// <summary>
        /// Defines the upper vertical angle of the visible portion of the sphere, in the range of [-π/2, π/2].
        /// </summary>
        public float UpperVerticalAngle
        {
            get => m_UpperVerticalAngle;
            set => m_UpperVerticalAngle = UpdateValue(m_UpperVerticalAngle, value);
        }

        /// <summary>
        /// Defines the upper vertical angle of the visible portion of the sphere, in the range of [-90, 90].
        /// </summary>
        public float UpperVerticalAngleInDegrees
        {
            get => m_UpperVerticalAngle * Mathf.Rad2Deg;
            set => UpperVerticalAngle = value * Mathf.Deg2Rad;
        }

        /// <summary>
        /// Defines the lower vertical angle of the visible portion of the sphere, in the range of [-π/2, π/2].
        /// </summary>
        public float LowerVerticalAngle
        {
            get => m_LowerVerticalAngle;
            set => m_LowerVerticalAngle = UpdateValue(m_LowerVerticalAngle, value);
        }

        /// <summary>
        /// Defines the lower vertical angle of the visible portion of the sphere, in the range of [-90, 90].
        /// </summary>
        public float LowerVerticalAngleInDegrees
        {
            get => m_LowerVerticalAngle * Mathf.Rad2Deg;
            set => LowerVerticalAngle = value * Mathf.Deg2Rad;
        }
    }
}
