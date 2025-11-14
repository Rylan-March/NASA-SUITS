---
uid: meta-openxr-anchors-feature
---
# Meta Quest Anchors feature

This page is a supplement to the AR Foundation [Anchors](xref:arfoundation-anchors) manual. The following sections only contain information about APIs where Meta Quest exhibits unique platform-specific behavior.

[!include[](../../snippets/arf-docs-tip.md)]

## Platform-specific success and error codes

On OpenXR platforms, the [XRResultStatus.nativeStatusCode](xref:UnityEngine.XR.ARSubsystems.XRResultStatus.nativeStatusCode) returned by AR Foundation's [ARAnchorManager.TryAddAnchorAsync](xref:UnityEngine.XR.ARFoundation.ARAnchorManager.TryAddAnchorAsync(UnityEngine.Pose)) is a wrapper around OpenXR's [XrResult](xref:UnityEngine.XR.OpenXR.NativeTypes.XrResult).

You can use the `XRResultStatus.nativeStatusCode` property to access the underlying `XrResult` value, as shown in the following example:

[!code-cs[anchors_resultstatus_xrresult](../../../Tests/Runtime/CodeSamples/Anchors/AnchorsSamples.cs#anchors_resultstatus_xrresult)]

## Persistent and shared anchor GUIDs

On Meta's OpenXR runtime, the `SerializableGuid` returned by [ARAnchorManager.TrySaveAnchorAsync](xref:UnityEngine.XR.ARFoundation.ARAnchorManager.TrySaveAnchorAsync) and [ARAnchorManager.TryShareAnchorAsync](xref:UnityEngine.XR.ARFoundation.ARAnchorManager.TryShareAnchorAsync(UnityEngine.XR.ARSubsystems.TrackableId,CancellationToken)) is the same value as the input anchor's [trackableId](xref:UnityEngine.XR.ARSubsystems.ITrackable.trackableId).

## Batch save anchors

OpenXR Meta overrides AR Foundation's default implementation for batch save anchors by requesting to save the entire batch at once instead of one at a time. Because of how Meta's OpenXR API is defined, the entire batch either succeeds or fails to save together. In other words, if one anchor fails to save, then all anchors will fail to save.

## Batch load anchors

OpenXR Meta overrides AR Foundation's default implementation for batch load anchors by requesting to load the entire batch at once instead of one at a time. [XRAnchorSubsystem.TryLoadAnchorsAsync](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystem.TryLoadAnchorsAsync(Unity.Collections.NativeArray{UnityEngine.XR.ARSubsystems.SerializableGuid},Unity.Collections.Allocator,Action{Unity.Collections.NativeArray{UnityEngine.XR.ARSubsystems.LoadAnchorResult}},CancellationToken)) will always order successfully loaded anchors in its output results followed by anchors that failed to load.

### Incremental load results

Unlike AR Foundation's default implementation for `XRAnchorSubsystem.TryLoadAnchorsAsync` where the incremental results callback is invoked once per anchor, incremental results from OpenXR Meta can contain multiple loaded anchors.

## Batch erase anchors

OpenXR Meta overrides AR Foundation's default implementation for batch erase anchors by requesting to erase the entire batch at once instead of one at a time. Because of how Meta's OpenXR API is defined, the entire batch either succeeds or fails to erase together. In other words, if one anchor fails to erase, then all anchors will fail to erase.

## Native pointer

[XRAnchor.nativePtr](xref:UnityEngine.XR.ARSubsystems.XRAnchor.nativePtr) values returned by this package contain a pointer to the following struct:

```c
typedef struct UnityXRNativeAnchor
{
    int version;
    void* referencePointPtr;
} UnityXRNativeAnchor;
```

Cast the `void* referencePointPtr` to an [XrSpace](https://registry.khronos.org/OpenXR/specs/1.0/html/xrspec.html#spaces) handle in C++ using the following example code:

```cpp
// Marshal the native anchor data from the XRAnchor.nativePtr in C#
UnityXRNativeAnchor nativeAnchor;
XrSpace* anchorXrSpaceHandle = static_cast<XrSpace*>(nativeAnchor.referencePointPtr);
```

To learn more about native pointers and their usage, refer to [Extending AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.0/manual/architecture/extensions.html).
