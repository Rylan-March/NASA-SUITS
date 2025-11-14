using UnityEngine;
using Unity.XR.CompositionLayers.Provider;

namespace Unity.XR.CompositionLayers.Extensions
{
    /// <summary>
    /// Subclass of <see cref="CompositionLayerExtension" /> to support
    /// defining textures, and the rects for rendering those textures for a <see cref="CompositionLayer"/> instance
    /// on the same game object.
    ///
    /// Source rect is used to define the rectangle within the source texture that the
    /// layer provider should use when reading texture information for the layer.
    ///
    /// Destination rect is used to define the rectangle within the source texture that the
    /// layer provider should use when writing texture information for the layer.
    ///
    /// Support for this component is up the the instance of <see cref="ILayerProvider" />
    /// currently assigned to the <see cref="Unity.XR.CompositionLayers.Services.CompositionLayerManager" />.
    ///
    /// If this extension is not added to a layer game object, it is expected that
    /// the provider will provide the necessary textures in some other way. This may
    /// occur in cases such as the rendering of protected content.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("XR/Composition Layers/Extensions/Source Textures")]
    [CompositionLayersHelpURL(typeof(TexturesExtension))]
    public class TexturesExtension : CompositionLayerExtension
    {
        /// <summary>
        /// Source Texture enumeration.
        /// </summary>
        public enum SourceTextureEnum
        {
            /// <summary>
            /// Use a Local Uploaded Texture
            /// </summary>
            LocalTexture = 0,
            /// <summary>
            /// Use an External Andriod Texture
            /// </summary>
            AndroidSurface,
        }

        /// <summary>
        /// Target eye enumeration.
        /// </summary>
        public enum TargetEyeEnum
        {
            /// <summary>
            /// Both eyes will be defined for this instance.
            /// </summary>
            Both = 0,
            /// <summary>
            /// Allow Emulation in each individual eye for seperate images to be used.
            /// </summary>
            Individual,
        }

        static readonly Rect k_DefaultRect = new(0f, 0f, 1f, 1f);

        /// <summary>
        /// Options for which type of object this extension should be associated with.
        /// </summary>
        public override ExtensionTarget Target => ExtensionTarget.Swapchain;

        [SerializeField]
        [Tooltip("Define the source texture. External texture is supported on Android to retrieve an Android Surface object to render to. (e.g.: video playback)")]
        SourceTextureEnum m_SourceTexture = SourceTextureEnum.LocalTexture;

        [SerializeField]
        [Tooltip("Defines the eye range that this instance is defined to support.")]
        TargetEyeEnum m_TargetEye = TargetEyeEnum.Both;

        [SerializeField]
        [Tooltip("The texture instance to be used for the left eye.")]
        Texture m_LeftTexture;

        [SerializeField]
        [Tooltip("The texture instance to be used for the right eye.")]
        Texture m_RightTexture;

        [SerializeField]
        [Tooltip("Select the emulation of the texture instance to be used for the left or right eye.")]
        int m_InEditorEmulation = 0;

        [SerializeField]
        [Tooltip("When true the rect values for source and destination can be modified to custom values.")]
        bool m_CustomRects;

        [SerializeField]
        [Tooltip("The resolution which will be used to create the external surface. Only effective on Android.")]
        Vector2 m_Resolution = Vector2.zero;

        [SerializeField]
        [Tooltip("Left eye source rectangle.")]
        Rect m_LeftEyeSourceRect = new(0, 0, 1, 1);

        [SerializeField]
        [Tooltip("Right eye source rectangle.")]
        Rect m_RightEyeSourceRect = new(0, 0, 1, 1);

        [SerializeField]
        [Tooltip("Left eye destination rectangle.")]
        Rect m_LeftEyeDestinationRect = new(0, 0, 1, 1);

        [SerializeField]
        [Tooltip("Right eye destination rectangle.")]
        Rect m_RightEyeDestinationRect = new(0, 0, 1, 1);

        [SerializeField]
        [Tooltip("Crop the hosting layer to the aspect ratio of the texture")]
        bool m_CropToAspect;

        bool m_TextureAdded;

        /// <summary>
        /// Defines the eye range that this instance is defined to support.
        /// </summary>
        public TargetEyeEnum TargetEye
        {
            get => m_TargetEye;
            set => m_TargetEye = UpdateValue(m_TargetEye, value);
        }

        /// <summary>
        /// The texture instance to be used for the left eye.
        /// </summary>
        public Texture LeftTexture
        {
            get => m_LeftTexture;
            set
            {
                if (m_LeftTexture == null && value != null)
                    m_TextureAdded = true;
                m_LeftTexture = UpdateValue(m_LeftTexture, value);
            }
        }

        /// <summary>
        /// The texture instance to be used for the right eye.
        /// </summary>
        public Texture RightTexture
        {
            get => m_RightTexture;
            set
            {
                if (m_RightTexture == null && value != null)
                    m_TextureAdded = true;
                m_RightTexture = UpdateValue(m_RightTexture, value);
            }
        }

        /// <summary>
        /// Boolean value to track if layer's source texture be added during update
        /// </summary>
        public bool TextureAdded
        {
            get => m_TextureAdded;
            set => m_TextureAdded = value;
        }

        /// <summary>
        /// When 0 Emultion is used on left eye when 1 it is used on the right.
        /// </summary>
        public int InEditorEmulation
        {
            get => m_InEditorEmulation;
            set => m_InEditorEmulation = UpdateValue(m_InEditorEmulation, value);
        }

        /// <summary>
        /// When true the rect values for source and destination can be modified to custom values.
        /// </summary>
        public bool CustomRects
        {
            get => m_CustomRects;
            set => m_CustomRects = UpdateValue(m_CustomRects, value);
        }

        /// <summary>
        /// When true the layer will be used for render Android external surface.
        /// </summary>
        public SourceTextureEnum sourceTexture
        {
            get => m_SourceTexture;
            set => m_SourceTexture = UpdateValue(m_SourceTexture, value);
        }

        /// <summary>
        /// External texture resolution (Android Only).
        /// </summary>
        public Vector2 Resolution
        {
            get => m_Resolution;
            set => m_Resolution = UpdateValue(m_Resolution, value);
        }

        /// <summary>
        /// Left eye source rectangle.
        ///
        /// Rects should always be clamped to be within the [0, 1] range.
        /// </summary>
        public Rect LeftEyeSourceRect
        {
            get => m_CustomRects ? m_LeftEyeSourceRect : k_DefaultRect;
            set => m_LeftEyeSourceRect = UpdateValue(m_LeftEyeSourceRect, value);
        }

        /// <summary>
        /// Right eye source rectangle.
        ///
        /// Rects should always be clamped to be within the [0, 1] range.
        /// </summary>
        public Rect RightEyeSourceRect
        {
            get => m_CustomRects ? m_RightEyeSourceRect : k_DefaultRect;
            set => m_RightEyeSourceRect = UpdateValue(m_RightEyeSourceRect, value);
        }

        /// <summary>
        /// Left eye destination rectangle.
        ///
        /// Rects should always be clamped to be within the [0, 1] range.
        /// </summary>
        public Rect LeftEyeDestinationRect
        {
            get => m_CustomRects ? m_LeftEyeDestinationRect : k_DefaultRect;
            set => m_LeftEyeDestinationRect = UpdateValue(m_LeftEyeDestinationRect, value);
        }

        /// <summary>
        /// Right eye destination rectangle.
        ///
        /// Rects should always be clamped to be within the [0, 1] range.
        /// </summary>
        public Rect RightEyeDestinationRect
        {
            get => m_CustomRects ? m_RightEyeDestinationRect : k_DefaultRect;
            set => m_RightEyeDestinationRect = UpdateValue(m_RightEyeDestinationRect, value);
        }

        /// <summary>
        /// Crop to the texture's aspect ratio
        ///
        /// If true, the hosting layer will crop its aspect ratio within the height/width bounds set
        /// in the layer properties.
        /// </summary>
        public bool CropToAspect
        {
            get => m_CropToAspect;
            set => m_CropToAspect = UpdateValue(m_CropToAspect, value);
        }

        ///<summary>
        /// Return a pointer to this extension's native struct.
        /// </summary>
        /// <returns>the pointer to texture extension's native struct.</returns>
        public override unsafe void* GetNativeStructPtr() => null;
    }
}
