#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CompositionLayers.UIInteraction.Editor
{
    /// <summary>
    /// Editor script for UIHandle to draw gizmos
    /// Hides the gizmo from the RectTransform, and creates a new one in the Layer's local space
    /// </summary>
    /// <seealso cref="UIHandle"/>
    [CustomEditor(typeof(UIHandle)), CanEditMultipleObjects]
    public class UIHandleEditor : UnityEditor.Editor
    {
        private UIHandle handle;

        void OnEnable()
        {
            handle = (UIHandle)target;

            Tools.hidden = true;
            SceneView.duringSceneGui += DuringSceneUI;
        }

        void OnDisable()
        {
            Tools.hidden = false;
            SceneView.duringSceneGui -= DuringSceneUI;
        }

        /// <summary>
        /// Called from SceneView.duringSceneGui, which is called on scene repaint.
        /// Handles drawing UI handles in local space for position, rotation, and scale
        /// </summary>
        /// <param name="view"></param>
        private void DuringSceneUI(SceneView view)
        {
            if (!handle.CompositionLayerTransform) return;

            switch (Tools.current)
            {
                case Tool.Move:
                    Tools.hidden = true;
                    HandlePosition();
                    break;
                case Tool.Rotate:
                    Tools.hidden = true;
                    HandleRotation();
                    break;
                case Tool.Scale:
                    Tools.hidden = true;
                    HandleScale();
                    break;
                default:
                    Tools.hidden = false;
                    break;
            }
        }

        /// <summary>
        /// Handles Position Gizmo in the composition layer's local space
        /// </summary>
        void HandlePosition()
        {
            EditorGUI.BeginChangeCheck();

            Vector3 handlePosition = handle.GetHandlePosition();
            Vector3 newWorldPosition = Handles.PositionHandle(handlePosition, handle.CompositionLayerTransform.rotation);

            if (EditorGUI.EndChangeCheck())
            {
                handle.SetPosition(newWorldPosition);
                Undo.RegisterFullObjectHierarchyUndo(handle, "UI Handle Position");
            }
        }

        /// <summary>
        /// Handles Rotation Gizmo in the composition layer's local space
        /// </summary>
        void HandleRotation()
        {
            EditorGUI.BeginChangeCheck();

            Vector3 handlePosition = handle.GetHandlePosition();
            Quaternion handleRotation = handle.GetHandleRotation();
            Quaternion newWorldRotation = Handles.RotationHandle(handleRotation, handlePosition);

            if (EditorGUI.EndChangeCheck())
            {
                handle.transform.rotation = newWorldRotation;
                Undo.RegisterFullObjectHierarchyUndo(handle, "UI Handle Rotation");
            }
        }

        /// <summary>
        /// Handles Scale Gizmo in the composition layer's local space
        /// </summary>
        void HandleScale()
        {
            EditorGUI.BeginChangeCheck();

            Vector3 handlePosition = handle.GetHandlePosition();
            Vector3 handleScale = handle.GetHandleScale();
            Vector3 newWorldScale = Handles.ScaleHandle(handleScale, handlePosition, handle.CompositionLayerTransform.rotation);

            if (EditorGUI.EndChangeCheck())
            {
                handle.transform.localScale = newWorldScale;
                Undo.RegisterFullObjectHierarchyUndo(handle, "UI Handle Scale");
            }
        }
    }
}
#endif
