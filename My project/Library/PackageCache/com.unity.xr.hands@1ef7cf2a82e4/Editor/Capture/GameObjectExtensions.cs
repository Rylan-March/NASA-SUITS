using UnityEngine;

namespace UnityEditor.XR.Hands.Capture
{
    static class GameObjectExtensions
    {
        /// <summary>
        /// Recursively sets the HideFlags for a GameObject and all its descendants.
        /// </summary>
        /// <param name="gameObject">The root GameObject to start from.</param>
        /// <param name="hideFlags">The HideFlags to apply.</param>
        internal static void SetHideFlagsRecursively(this Object gameObject, HideFlags hideFlags)
        {
            // Set HideFlags for the current GameObject
            gameObject.hideFlags = hideFlags;

            // Iterate through children and recursively call the function
            if (gameObject is GameObject go)
            {
                foreach (Transform child in go.transform)
                {
                    SetHideFlagsRecursively(child.gameObject, hideFlags);
                }
            }
        }
    }
}
