//********This was used for setting and unsetting a layer to be the default scene layer.
//********But with our updated implementation, this is not allowed and eye layer will always be the default layer
//********To be discussed, comment it out for now
//********To-Do: remove relevant SetDefaultSceneCompositionLayer() and ResetDefaultSceneCompositionLayer() in code

// using System;
// using Unity.XR.CompositionLayers.Services;
// using UnityEngine;

// namespace Unity.XR.CompositionLayers.Tests
// {
//     [RequireComponent(typeof(CompositionLayer))]
//     [AddComponentMenu("")]
//     class UserDefaultLayerTestComponent : MonoBehaviour
//     {
//         [SerializeField]
//         CompositionLayer m_CompositionLayer;

//         void Awake()
//         {
//             m_CompositionLayer = GetComponent<CompositionLayer>();
//         }

//         void OnEnable()
//         {
//             CompositionLayerManager.Instance.SetDefaultSceneCompositionLayer(m_CompositionLayer);
//         }

//         void OnDisable()
//         {
//             if (CompositionLayerManager.ManagerActive && CompositionLayerManager.Instance.DefaultSceneCompositionLayer == m_CompositionLayer)
//                 CompositionLayerManager.Instance.ResetDefaultSceneCompositionLayer();
//         }

//         void OnValidate()
//         {
//             m_CompositionLayer = GetComponent<CompositionLayer>();
//         }
//     }
// }
