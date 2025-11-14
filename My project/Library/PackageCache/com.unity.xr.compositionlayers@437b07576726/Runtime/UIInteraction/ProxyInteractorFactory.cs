#if UNITY_XR_INTERACTION_TOOLKIT
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
#if UNITY_XR_INTERACTION_TOOLKIT_3_0
using UnityEngine.XR.Interaction.Toolkit.Interactors;
#endif
namespace Unity.XR.CompositionLayers.UIInteraction
{
    /// <summary>
    /// Handles creation and deletion of "ProxyInteractors", proxy controllers used for interaction with a composition layer's Canvas
    /// </summary>
    internal class ProxyInteractorFactory
    {
        // Name to give instantiated interactors
        const string ProxyInteractorName = "ProxyInteractor";

        // Dictionary of normal interactors to their proxy canvas interactors
        private static Dictionary<IXRRayProvider, GameObject> interactorsToProxys = new Dictionary<IXRRayProvider, GameObject>();

        /// <summary>
        /// Gets a proxy interactor based on passed proxy
        /// </summary>
        /// <param name="interactor"></param>
        /// <returns>A proxy interactor based on passed proxy</returns>
        public GameObject GetProxy(IXRRayProvider interactor)
        {
            interactorsToProxys.TryGetValue(interactor, out var proxy);
            return proxy;
        }
#if UNITY_XR_INTERACTION_TOOLKIT_3_0
        public bool TryCreateOrFind(IXRRayProvider interactor, Vector3 position, out IXRRayProvider proxyInteractor)
        {
            proxyInteractor = null;

            if (interactor is not XRBaseInputInteractor baseInteractor)
                return false;

            if (baseInteractor.name == ProxyInteractorName)
                return false;

            if (!interactorsToProxys.ContainsKey(interactor))
            {
                GameObject proxyGameObject = null;

                switch (interactor)
                {
                    case NearFarInteractor:
                        proxyGameObject = CreateProxy(interactor as NearFarInteractor, position);
                        break;
                    case XRRayInteractor:
                        proxyGameObject = CreateProxy(interactor as XRRayInteractor, position);
                        break;
                }

                if (proxyGameObject == null)
                    return false;

                interactorsToProxys.Add(interactor, proxyGameObject);
            }

            proxyInteractor = interactorsToProxys[interactor].GetComponent<XRBaseInputInteractor>() as IXRRayProvider;
            return true;
        }


        private GameObject CreateProxy(XRRayInteractor xrRayInteractor, Vector3 position)
        {
            var interactorGameObject = new GameObject(ProxyInteractorName);
            interactorGameObject.transform.position = position;
            interactorGameObject.hideFlags = HideFlags.HideInHierarchy;

            var interactor = interactorGameObject.AddComponent<XRRayInteractor>();
            interactor.rayOriginTransform = interactorGameObject.transform;
            interactor.maxRaycastDistance = 500;
            interactor.raycastMask = 0;
            interactor.uiPressInput = xrRayInteractor.uiPressInput;
            interactor.uiScrollInput = xrRayInteractor.uiScrollInput;
            interactor.selectInput = xrRayInteractor.selectInput;
            interactor.activateInput = xrRayInteractor.activateInput;
            interactor.selectActionTrigger = xrRayInteractor.selectActionTrigger;
            interactor.targetPriorityMode = xrRayInteractor.targetPriorityMode;
            interactor.allowActivate = xrRayInteractor.allowActivate;
            return interactorGameObject;
        }


        private GameObject CreateProxy(NearFarInteractor nearFarInteractor, Vector3 position)
        {
            var interactorGameObject = new GameObject(ProxyInteractorName);
            interactorGameObject.transform.position = position;
            interactorGameObject.hideFlags = HideFlags.HideInHierarchy;

            var interactor = interactorGameObject.AddComponent<NearFarInteractor>();
            interactor.uiPressInput = nearFarInteractor.uiPressInput;
            interactor.uiScrollInput = nearFarInteractor.uiScrollInput;
            interactor.selectInput = nearFarInteractor.selectInput;
            interactor.activateInput = nearFarInteractor.activateInput;
            interactor.selectActionTrigger = nearFarInteractor.selectActionTrigger;
            interactor.targetPriorityMode = nearFarInteractor.targetPriorityMode;
            interactor.allowActivate = nearFarInteractor.allowActivate;

            return interactorGameObject;
        }
#else
        /// <summary>
        /// Creates or finds a proxy interactor from a supplied interactor
        /// </summary>
        /// <param name="interactor">The interactor</param>
        /// <param name="position">The position to put the proxy interactor</param>
        /// <param name="proxyInteractor">The created or found interactor</param>
        /// <returns>Whether or not it has been created or found</returns>
        public bool TryCreateOrFind(IXRRayProvider interactor, Vector3 position, out IXRRayProvider proxyInteractor)
        {
            proxyInteractor = null;

            if (interactor is not XRRayInteractor rayInteractor)
                return false;


            if (rayInteractor.name == ProxyInteractorName)
                return false;


            if (!interactorsToProxys.ContainsKey(interactor) && rayInteractor.xrController is ActionBasedController)
            {
                var proxyGameObject = CreateProxy(rayInteractor.xrController as ActionBasedController, position);
                interactorsToProxys.Add(interactor, proxyGameObject);
            }

            proxyInteractor = interactorsToProxys[interactor].GetComponent<XRRayInteractor>() as IXRRayProvider;
            return true;
        }

        /// <summary>
        /// Helper for creating Proxy Interactors
        /// </summary>
        /// <param name="actionBasedController">Interactor to base the proxy from</param>
        /// <param name="position">Position to create the proxy controller</param>
        /// <returns>The created Proxy Interactor GameObject</returns>
        private GameObject CreateProxy(ActionBasedController actionBasedController, Vector3 position)
        {
            var interactorGameObject = new GameObject(ProxyInteractorName);
            interactorGameObject.transform.position = position;
            interactorGameObject.hideFlags = HideFlags.HideInHierarchy;

            var xrController = interactorGameObject.AddComponent<ActionBasedController>();
            xrController.selectAction = actionBasedController.selectAction;
            xrController.activateAction = actionBasedController.activateAction;
            xrController.uiPressAction = actionBasedController.uiPressAction;

            var xrRayInteractor = interactorGameObject.AddComponent<XRRayInteractor>();
            xrRayInteractor.rayOriginTransform = interactorGameObject.transform;
            xrRayInteractor.maxRaycastDistance = 500;
            xrRayInteractor.raycastMask = 0;

            return interactorGameObject;
        }
#endif
    }
}
#endif
