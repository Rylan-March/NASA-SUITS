---
uid: meta-openxr-shared-anchors
---
# Shared anchors

OpenXR Meta provides a shared anchors API that enables you to share anchors between colocated users. The shared anchors feature enables users within the same physical space to share and load anchors to their devices. You can use this API to enable local multiplayer experiences with a shared frame of reference created by shared anchors. For example, you can use the shared anchors feature to play a virtual board game on top of the same table in the physical environment.

Sharing anchors works by creating a group ID and sharing anchors with that ID. To share anchors, you must first [Set the group ID](#set-group-id). The following sections describe the shared anchors feature in more detail.

## Check support

To learn which Quest devices support shared anchors, refer to Meta’s shared spatial anchors [Device requirements](https://developers.meta.com/horizon/documentation/unity/unity-shared-spatial-anchors/#device-requirements).

You can also use [MetaOpenXRAnchorSubsystem.isSharedAnchorsSupported](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRAnchorSubsystem.isSharedAnchorsSupported) to check whether a device supports shared anchors, as shown in the following code example:

[!code-cs[CheckIsSharedAnchorsSupported](../../../Tests/Runtime/CodeSamples/Anchors/CheckSharedAnchorsSupportSample.cs#CheckIsSharedAnchorsSupported)]

<a id="set-group-id"></a>

## Set the group ID

To set the group ID for your current session, generate a new `SerializableGuid` and set the [MetaOpenXRAnchorSubsystem.sharedAnchorsGroupId](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRAnchorSubsystem.sharedAnchorsGroupId) property as shown in the following code example:

[!code-cs[SetSharedAnchorsGroupId](../../../Tests/Runtime/CodeSamples/Anchors/SetSharedAnchorsGroupIdSample.cs#SetSharedAnchorsGroupId)]

> [!NOTE]
> [!IMPORTANT]
> Set the group ID to a non-empty GUID, i.e. `new SerializableGuid(Guid.NewGuid())`, as Meta's OpenXR runtime doesn't allow sharing anchors to an empty group ID.

<a id="share-group-id"></a>

### Share the group ID

Share your group ID with colocated users of your app so they can share and load anchors from the same group. Generate a new ID for each session to prevent users in previous sessions accessing shared anchors in your current session.

> [!NOTE]
> Don't use shared anchors as an alternative to saving anchors. To re-use shared anchors across multiple sessions, save the anchor before sharing, and re-share the anchor in subsequent sessions to a new group ID.

## Share group IDs between devices

AR Foundation doesn't provide a networking solution for messaging GUIDs between users on different devices. The [AR Foundation Samples GitHub repository](https://github.com/Unity-Technologies/arfoundation-samples/blob/6.2/Assets/Scripts/Runtime/Networking/SharedAnchorsNetworkMessenger.cs) contains example code that uses [Netcode for GameObjects](https://docs-multiplayer.unity3d.com/netcode/current/about/) to demonstrate one way to share GUIDs between devices to get you started.

## Share anchor

Once you have set the group ID, all subsequent calls to [TryShareAnchorAsync](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRAnchorManagerExtensions.TryShareAnchorAsync(UnityEngine.XR.ARFoundation.ARAnchorManager,UnityEngine.XR.ARFoundation.ARAnchor)) or [TryShareAnchorsAsync](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRAnchorManagerExtensions.TryShareAnchorsAsync(UnityEngine.XR.ARFoundation.ARAnchorManager,IEnumerable{UnityEngine.XR.ARFoundation.ARAnchor},List{UnityEngine.XR.ARSubsystems.XRShareAnchorResult})) will share anchors with that group.

Most apps will only need one group per physical location of colocated users. However, you can also share with multiple groups if necessary by changing the active group ID at any time.

To share an anchor, use the [ARAnchorManager.TryShareAnchorAsync](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRAnchorManagerExtensions.TryShareAnchorAsync(UnityEngine.XR.ARFoundation.ARAnchorManager,UnityEngine.XR.ARFoundation.ARAnchor)) extension method as shown in the following code example:

[!code-cs[TryShareAnchorAsync](../../../Tests/Runtime/CodeSamples/Anchors/SingleShareAnchorSample.cs#TryShareAnchorAsync)]

## Batch share anchors

You can share a batch of anchors with the [ARAnchorManager.TryShareAnchorsAsync](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRAnchorManagerExtensions.TryShareAnchorsAsync(UnityEngine.XR.ARFoundation.ARAnchorManager,IEnumerable{UnityEngine.XR.ARFoundation.ARAnchor},List{UnityEngine.XR.ARSubsystems.XRShareAnchorResult})) extension method as shown in the following code example:

[!code-cs[TryShareAnchorsAsync](../../../Tests/Runtime/CodeSamples/Anchors/BatchShareAnchorsSample.cs#TryShareAnchorsAsync)]

Meta defines its OpenXR API such that the entire batch either succeeds or fails to share together. This means if one anchor fails to share, then all anchors will fail to share.

## Load shared anchors

Once you have set the group ID, you can load all shared anchors from the group with the [ARAnchorManager.TryLoadAllSharedAnchorsAsync](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRAnchorManagerExtensions.TryLoadAllSharedAnchorsAsync(UnityEngine.XR.ARFoundation.ARAnchorManager,List{UnityEngine.XR.ARSubsystems.XRAnchor},Action{Unity.XR.CoreUtils.Collections.ReadOnlyListSpan{UnityEngine.XR.ARSubsystems.XRAnchor}})) extension method as shown in the following code example:

[!code-cs[TryLoadAllSharedAnchorsAsync](../../../Tests/Runtime/CodeSamples/Anchors/LoadAllSharedAnchorsSample.cs#TryLoadAllSharedAnchorsAsync)]

If `TryLoadAllSharedAnchorsAsync` returns a successful [XRResultStatus](xref:UnityEngine.XR.ARSubsystems.XRResultStatus), the `List<XRAnchor>` passed in will be populated with all anchors that were successfully loaded. Use the [XRAnchor.trackableId](xref:UnityEngine.XR.ARSubsystems.XRAnchor.trackableId) to know which anchors were loaded as shared anchors when [ARAnchorManager.trackablesChanged](xref:UnityEngine.XR.ARFoundation.ARTrackableManager`5.trackablesChanged) is invoked.

To receive notifications about loaded shared anchors before `ARAnchorManager.trackablesChanged` is invoked, rely on the incremental results callback. The final result from `TryLoadAllSharedAnchorsAsync` isn't guaranteed to complete before any `ARAnchorManager.trackablesChanged` events are invoked for loaded shared anchors.

### Incremental load results

The third input parameter of the `ARAnchorManager.TryLoadAllSharedAnchorsAsync` extension method allows you to pass a callback method, which the provider will invoke whenever anchors are incrementally loaded. This enables you to work with anchors as soon as they become available without waiting for the entire load request to complete. You should use the incremental results callback to ensure you're notified when an anchor has loaded before [ARAnchorManager.trackablesChanged](xref:UnityEngine.XR.ARFoundation.ARTrackableManager`5.trackablesChanged) is raised. To ignore the incremental results, pass `null` for the callback.

The incremental results callback passes a [ReadOnlyListSpan\<XRAnchor\>](xref:Unity.XR.CoreUtils.Collections.ReadOnlyListSpan`1) when invoked that provides a read-only slice of the `List<XRAnchor>` output list passed into the `ARAnchorManager.TryLoadAllSharedAnchorsAsync` extension method.

## Shared anchors expiration

At the time of writing, shared anchors remain shared with the group ID for 30 days since the last successful share. You can't otherwise stop sharing an anchor once it's been shared. Refer to Meta's [Shared Anchors](https://developers.meta.com/horizon/documentation/native/android/openxr-spatial-anchors-api-ref/) documentation for the most recent information regarding anchor expiration.
