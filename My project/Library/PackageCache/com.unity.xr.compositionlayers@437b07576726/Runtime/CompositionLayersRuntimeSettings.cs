using Unity.XR.CompositionLayers.Layers;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CompositionLayers
{
    /// <summary>
    /// Settings class for composition layer emulation in standalone.
    /// </summary>
    [ScriptableSettingsPath("Assets/CompositionLayers/UserSettings")]
    public class CompositionLayersRuntimeSettings : ScriptableSettings<CompositionLayersRuntimeSettings>
    {
        /// <summary>
        /// Defines the types of layers that can be used.
        /// </summary>
        public enum Layer
        {
            /// <summary>
            /// Quad Layer
            /// </summary>
            Quad,

            /// <summary>
            /// Cylinder Layer
            /// </summary>
            Cylinder
        }

        [SerializeField]
        [Tooltip("Enable or disable emulation of composition layers in standalone builds when no XR provider is active or no headset connected.")]
        bool m_EmulationInStandalone = false;

        /// <summary>
        /// Gets a value indicating whether emulation of composition layers in standalone builds is enabled when no XR provider is active or no headset is connected.
        /// </summary>
        public bool EmulationInStandalone => m_EmulationInStandalone;

        [Header("Composition Layer Splash Settings")]
        [SerializeField]
        [Tooltip("Enable or disable the splash screen.")]
        bool m_EnableSplashScreen = false;

        /// <summary>
        /// Gets a value indicating whether the splash screen is enabled.
        /// </summary>
        public bool EnableSplashScreen => m_EnableSplashScreen;

        [Header("Style Settings")]
        [SerializeField]
        [Tooltip("Image to display on the splash screen.")]
        Texture m_SplashImage = null;

        /// <summary>
        /// Gets the image to display on the splash screen.
        /// </summary>
        public Texture SplashImage => m_SplashImage;

        [SerializeField]
        [Tooltip("Background color of the splash screen.")]
        Color m_BackgroundColor = new Color(0.1372549f, 0.1215686f, 0.1254902f, 1.0f);

        /// <summary>
        /// Gets the background color of the splash screen.
        /// </summary>
        public Color BackgroundColor => m_BackgroundColor;

        [Header("Duration Settings")]
        [SerializeField]
        [Tooltip("Duration of the splash screen.")]
        float m_SplashDuration = 3f;

        /// <summary>
        /// Gets the duration of the splash screen.
        /// </summary>
        public float SplashDuration => m_SplashDuration;

        [SerializeField, Min(0.0f)]
        [Tooltip("Duration of the fade in.")]
        float m_FadeInDuration = 2.0f;

        /// <summary>
        /// Gets the duration of the fade-in effect.
        /// </summary>
        public float FadeInDuration => m_FadeInDuration;

        [SerializeField, Min(0.0f)]
        [Tooltip("Duration of the fade out.")]
        float m_FadeOutDuration = 1.0f;

        /// <summary>
        /// Gets the duration of the fade-out effect.
        /// </summary>
        public float FadeOutDuration => m_FadeOutDuration;

        [Header("Follow Settings")]
        [SerializeField, Min(0.0f)]
        [Tooltip("Speed at which the layer lerps to the follow position.")]
        float m_FollowSpeed = 2.0f;

        /// <summary>
        /// Gets the speed at which the layer moves towards the follow position.
        /// </summary>
        public float FollowSpeed => m_FollowSpeed;

        [SerializeField]
        [Tooltip("Distance from the camera to the splash screen.")]
        float m_FollowDistance = 2.0f;

        /// <summary>
        /// Gets the distance from the camera to the splash screen.
        /// </summary>
        public float FollowDistance => m_FollowDistance;

        [SerializeField]
        [Tooltip("Lock the splash screen to the horizon.")]
        bool m_LockToHorizon = true;

        /// <summary>
        /// Gets a value indicating whether the splash screen is locked to the horizon.
        /// </summary>
        public bool LockToHorizon => m_LockToHorizon;

        [Header("Layer Settings")]
        [SerializeField]
        Layer m_LayerType = Layer.Quad;

        /// <summary>
        /// Gets the type of layer used for the splash screen.
        /// </summary>
        public Layer LayerType => m_LayerType;

        [SerializeField]
        QuadLayerData m_QuadLayerData = new QuadLayerData();

        /// <summary>
        /// Gets the data for the quad layer.
        /// </summary>
        public QuadLayerData QuadLayerData => m_QuadLayerData;

        [SerializeField]
        CylinderLayerData m_CylinderLayerData = new CylinderLayerData();

        /// <summary>
        /// Gets the data for the cylinder layer.
        /// </summary>
        public CylinderLayerData CylinderLayerData => m_CylinderLayerData;
    }
}
