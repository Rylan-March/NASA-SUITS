using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CompositionLayers.UIInteraction.Editor
{
    /// <summary>
    /// Custom editor for the <see cref="UIFocus"/> component.
    /// Overrides the SceneView's default behavior to focus on the composition layer when the user presses the 'F' key.
    /// </summary>
    [CustomEditor(typeof(UIFocus))]
    public class UIFocusEditor : UnityEditor.Editor
    {
        void OnEnable()
        {
            SceneView.duringSceneGui += DuringSceneUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneUI;
        }

        /// <summary>
        /// Handles focusing on the composition layer when the user presses F by overriding the SceneView's default behavior
        /// </summary>
        private void DuringSceneUI(SceneView view)
        {
            var currentEvent = Event.current;
            if (currentEvent == null)
                return;

            var currentEventName = currentEvent.commandName;

            // Check if the event is a frame event
            if (currentEvent.type == EventType.ExecuteCommand && currentEventName.Contains("Frame"))
            {
                // Reset the command name to prevent the scene view from handling it
                Event.current.commandName = "";

                // Get the bounds of the composition layer
                UIFocus focus = target as UIFocus;
                Bounds rectTransformBounds = focus.GetBounds();

                // Frame the composition layer in the scene view
                SceneView.lastActiveSceneView.Frame(rectTransformBounds, false);
            }
        }
    }
}
