---
uid: androidxr-openxr-hand-mesh-data
---
# Hand mesh data

This page supplements the XR Hands manual and other Unity documentation concerning the many ways to construct different kinds of meshes from the base data of indices, positions, etc. It also makes brief mention of using this data for occlusion using AR Foundation samples (refer to the AR Foundation [Occlusion](xref:arfoundation-occlusion) manual for further information). The following sections only contain information about APIs where Google's Android XR runtime exhibits platform-specific behavior.

[!include[](../snippets/arf-docs-tip.md)]

To enable hand mesh data access at runtime, enable the **Hand Tracking Subsystem** and **Android XR: Hand Mesh Data** features in the Project Settings for OpenXR.

## Testing occlusion with hand mesh data

AR Foundation provides the `HandsOcclusion` sample to demonstrate occlusion. To test runtime-provided hand meshes on Android XR with the occlusion sample:

1. Download the AR Foundation Samples app from the [AR Foundations Samples GitHub repository](https://github.com/Unity-Technologies/arfoundation-samples/tree/6.2) and open it in Unity.
2. Enable **Hand Tracking Subsystem** and **Android XR: Hand Mesh Data**.
3. Open the `HandsOcclusion` scene in Unity from `Assets/Scenes/HandsOcclusion`.
4. Select the **Main Camera** object (**XR Origin Prefab** > **XR Origin** > **Camera Offset**).
5. Open **AR Shader Occlusion** in the **Inspector** window, and ensure **Use Hand Mesh** is enabled in **Occlusion Sources**.
6. Ensure `OccludedHands` is selected as the **Hands Occlusion Material**.

At runtime, the observed behavior should be that you can see your real-world hands and they occlude virtual game objects.
