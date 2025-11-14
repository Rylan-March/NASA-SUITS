using UnityEngine.XR.ARFoundation;
#if UNITY_ANDROID
using System.Collections.Generic;
using UnityEngine.Android;
#endif // UNITY_ANDROID

namespace UnityEngine.XR.OpenXR.Features.Android.Tests
{
    class PlanePermissionSample : MonoBehaviour
    {
        #region request_plane_permission
        const string k_Permission = "android.permission.SCENE_UNDERSTANDING_COARSE";

        [SerializeField]
        ARPlaneManager m_ARPlaneManager;

#if UNITY_ANDROID
        void Start()
        {
            if (!Permission.HasUserAuthorizedPermission(k_Permission))
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += OnPermissionDenied;
                callbacks.PermissionGranted += OnPermissionGranted;

                Permission.RequestUserPermission(k_Permission, callbacks);
            }
            else
            {
                // enable the AR Plane Manager component if permission is already granted
                m_ARPlaneManager.enabled = true;
            }
        }

        void OnPermissionDenied(string permission)
        {
            // handle denied permission
        }

        void OnPermissionGranted(string permission)
        {
            // enable the AR Plane Manager component after permission is granted
            m_ARPlaneManager.enabled = true;
        }
#endif // UNITY_ANDROID
        #endregion
    }

    class OcclusionPermissionSample : MonoBehaviour
    {
        #region request_occlusion_permission
        const string k_Permission = "android.permission.SCENE_UNDERSTANDING_FINE";

        [SerializeField]
        AROcclusionManager m_AROcclusionManager;

#if UNITY_ANDROID
        void Start()
        {
            if (!Permission.HasUserAuthorizedPermission(k_Permission))
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += OnPermissionDenied;
                callbacks.PermissionGranted += OnPermissionGranted;

                Permission.RequestUserPermission(k_Permission, callbacks);
            }
            else
            {
                // enable the AR Occlusion Manager component if permission is already granted
                m_AROcclusionManager.enabled = true;
            }
        }

        void OnPermissionDenied(string permission)
        {
            // handle denied permission
        }

        void OnPermissionGranted(string permission)
        {
            // enable the AR Occlusion Manager component if permission is already granted
            m_AROcclusionManager.enabled = true;
            m_AROcclusionManager.subsystem.Stop();
            m_AROcclusionManager.subsystem.Start();
        }
#endif // UNITY_ANDROID
        #endregion
    }
    class LightEstimationPermissionSample : MonoBehaviour
    {
        #region request_light_estimation_permission
        const string k_Permission = "android.permission.SCENE_UNDERSTANDING_COARSE";

        [SerializeField]
        ARCameraManager m_ARCameraManager;

#if UNITY_ANDROID
        void Start()
        {
            if (!Permission.HasUserAuthorizedPermission(k_Permission))
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += OnPermissionDenied;
                callbacks.PermissionGranted += OnPermissionGranted;

                Permission.RequestUserPermission(k_Permission, callbacks);
            }
            else
            {
                // enable the AR Camera Manager component if permission is already granted
                m_ARCameraManager.enabled = true;
            }
        }

        void OnPermissionDenied(string permission)
        {
            // handle denied permission
        }

        void OnPermissionGranted(string permission)
        {
            // enable the AR Camera Manager component if permission is already granted
            m_ARCameraManager.enabled = true;
            m_ARCameraManager.subsystem.Stop();
            m_ARCameraManager.subsystem.Start();
        }
#endif // UNITY_ANDROID
        #endregion
    }

    class HandTrackingPermissionSample : MonoBehaviour
    {
        #region request_hand_tracking_permission
        const string k_Permission = "android.permission.HAND_TRACKING";

#if UNITY_ANDROID
        void Start()
        {
            if (!Permission.HasUserAuthorizedPermission(k_Permission))
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += OnPermissionDenied;
                callbacks.PermissionGranted += OnPermissionGranted;

                Permission.RequestUserPermission(k_Permission, callbacks);
            }
        }

        void OnPermissionDenied(string permission)
        {
            // handle denied permission
        }

        void OnPermissionGranted(string permission)
        {
            // handle granted permission
        }
#endif // UNITY_ANDROID
        #endregion
    }

    class FacePermissionSample : MonoBehaviour
    {
        #region request_face_permission
        const string k_Permission = "android.permission.EYE_TRACKING_COARSE";

        [SerializeField]
        ARFaceManager m_ARFaceManager;

#if UNITY_ANDROID
        void Start()
        {
            if (!Permission.HasUserAuthorizedPermission(k_Permission))
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += OnPermissionDenied;
                callbacks.PermissionGranted += OnPermissionGranted;

                Permission.RequestUserPermission(k_Permission, callbacks);
            }
            else
            {
                // enable the AR Face Manager component if permission is already granted
                m_ARFaceManager.enabled = true;
            }
        }

        void OnPermissionDenied(string permission)
        {
            // handle denied permission
        }

        void OnPermissionGranted(string permission)
        {
            // enable the AR Face Manager component after permission is granted
            m_ARFaceManager.enabled = true;
        }
#endif // UNITY_ANDROID
        #endregion
    }

    class GazeFinePermissionSample : MonoBehaviour
    {
        #region request_eye_tracking_fine_permission
        const string k_Permission = "android.permission.EYE_TRACKING_FINE";

#if UNITY_ANDROID
        void Start()
        {
            if (!Permission.HasUserAuthorizedPermission(k_Permission))
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += OnPermissionDenied;
                callbacks.PermissionGranted += OnPermissionGranted;

                Permission.RequestUserPermission(k_Permission, callbacks);
            }
        }

        void OnPermissionDenied(string permission)
        {
            // handle denied permission
        }

        void OnPermissionGranted(string permission)
        {
            // handle granted permission
        }
#endif // UNITY_ANDROID
        #endregion
    }

    class MultiplePermissionSample : MonoBehaviour
    {
        #region request_multiple_permissions
        const string k_SceneUnderstandingFinePermission = "android.permission.SCENE_UNDERSTANDING_FINE";
        const string k_SceneUnderstandingCoarsePermission = "android.permission.SCENE_UNDERSTANDING_COARSE";
        const string k_HandTrackingPermission = "android.permission.HAND_TRACKING";
        const string k_EyeTrckingCoarsePermission = "android.permission.EYE_TRACKING_COARSE";
        const string k_EyeTrackingFinePermission = "android.permission.EYE_TRACKING_FINE";

        [SerializeField]
        ARPlaneManager m_ARPlaneManager;

        [SerializeField]
        ARFaceManager m_ARFaceManager;

        [SerializeField]
        AROcclusionManager m_AROcclusionManager;

#if UNITY_ANDROID
        void Start()
        {
            var permissions = new List<string>();

            // add permissions that have not been granted by the user
            if (!Permission.HasUserAuthorizedPermission(k_SceneUnderstandingCoarsePermission))
            {
                permissions.Add(k_SceneUnderstandingCoarsePermission);
            }
            else
            {
                // enable the AR Plane Manager component if permission is already granted
                m_ARPlaneManager.enabled = true;
            }

            if (!Permission.HasUserAuthorizedPermission(k_SceneUnderstandingFinePermission))
            {
                permissions.Add(k_SceneUnderstandingFinePermission);
            }
            else
            {
                // enable the AR Occlusion Manager component if permission is already granted
                m_AROcclusionManager.enabled = true;
                m_AROcclusionManager.subsystem.Stop();
                m_AROcclusionManager.subsystem.Start();
            }

            if (!Permission.HasUserAuthorizedPermission(k_HandTrackingPermission))
            {
                permissions.Add(k_HandTrackingPermission);
            }

            if (!Permission.HasUserAuthorizedPermission(k_EyeTrackingFinePermission))
            {
                permissions.Add(k_EyeTrackingFinePermission);
            }
            else
            {
                // enable the AR Face Manager component if permission is already granted
                m_ARFaceManager.enabled = true;
            }

            if (!Permission.HasUserAuthorizedPermission(k_EyeTrckingCoarsePermission))
            {
                permissions.Add(k_EyeTrckingCoarsePermission);
            }

            // setup callbacks to be called depending on whether permission is granted
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += OnPermissionDenied;
            callbacks.PermissionGranted += OnPermissionGranted;

            Permission.RequestUserPermissions(permissions.ToArray(), callbacks);
        }

        void OnPermissionDenied(string permission)
        {
            // handle denied permission
        }

        void OnPermissionGranted(string permission)
        {
            // enable the corresponding AR Manager component after required permission is granted
            if (permission == k_SceneUnderstandingCoarsePermission)
            {
                m_ARPlaneManager.enabled = true;
            }
            else if (permission == k_EyeTrckingCoarsePermission)
            {
                m_ARFaceManager.enabled = true;
            }
        }
#endif // UNITY_ANDROID
        #endregion
    }
}

