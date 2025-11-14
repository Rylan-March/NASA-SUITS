#region scene-permission
#if UNITY_ANDROID
using UnityEngine.Android;
#endif // UNITY_ANDROID
using UnityEngine.Events;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    public class PermissionsCheck : MonoBehaviour
    {
        const string k_DefaultPermissionId = "com.oculus.permission.USE_SCENE";

#pragma warning disable CS0414
        [SerializeField]
        [Tooltip("The Android system permission to request")]
        string m_PermissionId = k_DefaultPermissionId;

        [SerializeField]
        [Tooltip("Invoked when permission is denied")]
        UnityEvent<string> m_PermissionDenied;

        [SerializeField]
        [Tooltip("Invoked when permission is granted")]
        UnityEvent<string> m_PermissionGranted;
#pragma warning restore CS0414

#if UNITY_ANDROID
        void Start()
        {
            if (Permission.HasUserAuthorizedPermission(m_PermissionId))
            {
                OnPermissionGranted(m_PermissionId);
            }
            else
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += OnPermissionDenied;
                callbacks.PermissionGranted += OnPermissionGranted;

                Debug.Log($"Requesting permission for: {m_PermissionId}");
                Permission.RequestUserPermission(m_PermissionId, callbacks);
            }
        }

        void OnPermissionDenied(string permission)
        {
            Debug.LogWarning($"User denied permission for: {m_PermissionId}");
            m_PermissionDenied.Invoke(permission);
        }

        void OnPermissionGranted(string permission)
        {
            Debug.Log($"User granted permission for: {m_PermissionId}");
            m_PermissionGranted.Invoke(permission);
        }
#endif // UNITY_ANDROID
    }
}
#endregion
