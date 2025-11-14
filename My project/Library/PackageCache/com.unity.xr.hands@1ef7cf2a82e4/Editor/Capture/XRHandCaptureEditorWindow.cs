using System;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UnityEditor.XR.Hands.Capture
{
    class XRHandCaptureEditorWindow : EditorWindow
    {
        const string k_WindowTitle = "XR Hand Capture";
        const string k_PlaybackInspectorContainerName = "PlaybackInspectorContainer";
        const string k_MenuPath = "Window/XR/XR Hand Capture";

        XRHandCaptureEditor m_PlaybackEditor;
        XRHandCapturePlayback m_CapturePlayback;
        PlayModeStateChange m_PlayMode;

        /// <summary>
        /// Show the XR Hand Capture window.
        /// </summary>
        [MenuItem(k_MenuPath)]
        public static void ShowWindow()
        {
            GetWindow<XRHandCaptureEditorWindow>(title: k_WindowTitle);
        }

        void OnEnable()
        {
            InitializePlayback();
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        void InitializePlayback()
        {
            if (m_CapturePlayback == null
                && m_PlayMode == PlayModeStateChange.EnteredEditMode)
            {
                m_CapturePlayback = XRHandCapturePlayback.GetInstance();
                m_CapturePlayback.Initialize();
            }
            BuildUI();
        }

        void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            m_PlayMode = obj;
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    InitializePlayback();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    DestroyPlayback();
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(obj), obj, null);
            }
        }

        void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            if (m_CapturePlayback != null)
            {
                m_CapturePlayback.OnDestroy();
                m_CapturePlayback = null;
                InitializePlayback();
                BuildUI();
            }
        }

        void OnDestroy()
        {
            EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            DestroyPlayback();

            if (m_PlaybackEditor != null)
            {
                DestroyImmediate(m_PlaybackEditor);
                m_PlaybackEditor = null;
            }
        }

        void DestroyPlayback()
        {
            if (m_CapturePlayback != null)
            {
                m_CapturePlayback.OnDestroy();
                m_CapturePlayback = null;
            }
        }

        void BuildUI()
        {
            // Recreate the editor instance to ensure a fresh state.
            if (m_PlaybackEditor != null)
            {
                DestroyImmediate(m_PlaybackEditor);
            }
            m_PlaybackEditor = CreateInstance<XRHandCaptureEditor>();

            rootVisualElement.Clear();

            var scrollView = new ScrollView();

            if (m_PlaybackEditor != null)
            {
                var container = new VisualElement { name = k_PlaybackInspectorContainerName };
                var inspectorGUI = m_PlaybackEditor.CreateInspectorGUI();
                container.Add(inspectorGUI);
                scrollView.Add(container);
            }

            rootVisualElement.Add(scrollView);
            rootVisualElement.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);
        }
    }

    [InitializeOnLoad]
    static class XRHandCaptureEditorCallbacks
    {
        static XRHandCaptureEditorCallbacks()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        static void OnBeforeAssemblyReload()
        {
            XRHandCapturePlayback.GetInstance().OnDestroy();
        }
    }
}
