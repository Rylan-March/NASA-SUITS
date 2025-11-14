using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.CompositionLayers
{
    internal class UserLayerCache
    {
        bool[] blankLayers = new bool[32];

        public UserLayerCache()
        {
            for (int i = 0; i < 32; i++)
            {
                var name = LayerMask.LayerToName(i);
                blankLayers[i] = name == string.Empty;
            }
        }

        private void ChangeLayerOfAllChildren(GameObject gameObj, int layerBit)
        {
            gameObj.layer = layerBit;
            foreach (Transform t in gameObj.transform)
                ChangeLayerOfAllChildren(t.gameObject, layerBit);
        }

        internal int OccupyBlankLayer(GameObject gameObject, bool removeFromAllCameras = true)
        {
            int layerBit = -1;
            for (int i = 8; i < blankLayers.Length; ++i)
            {
                var name = LayerMask.LayerToName(i);
                if (name == string.Empty && blankLayers[i])
                {
                    layerBit = i;
                    blankLayers[i] = false;
                    break;
                }
            }

            if (layerBit == -1)
            {
                Debug.Log("Not enough available layers.");
                return layerBit;
            }

            ChangeLayerOfAllChildren(gameObject, layerBit);

            // Remove canvas layer from all cameras
#if UNTIY_EDITOR
            Tools.visibleLayers &= ~(1 << canvasLayerBit);
#endif
            if (removeFromAllCameras)
            {
                var cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
                foreach (var camera in cameras)
                {
                    camera.cullingMask &= ~(1 << layerBit);
                }
            }

            return layerBit;
        }

        internal void UnOccupyBlankLayer(GameObject gameObject)
        {
            if (gameObject != null)
            {
                blankLayers[gameObject.layer] = true;
                var defaultLayerBit = LayerMask.NameToLayer("Default");
                ChangeLayerOfAllChildren(gameObject, defaultLayerBit);
            }
        }
    }

}
