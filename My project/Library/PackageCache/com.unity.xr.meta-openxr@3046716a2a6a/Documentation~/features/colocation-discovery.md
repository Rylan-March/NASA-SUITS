---
uid: meta-openxr-colocation-discovery
---
# Colocation discovery

The Meta Quest Colocation Discovery feature enables your app to discover other physically colocated devices running the same app. Colocation Discovery simplifies the process of users joining a networked session when in the same physical space, and removes the need for users of your app to manually find lobbies or share join codes.

The following sections describe how to advertise and discover messages between colocated users of your app.

## Enable colocation discovery

To enable Meta Quest Colocation Discovery in your app:

1. Go to **Project Settings** > **XR Plug-in Management** > **OpenXR**.
2. Under **OpenXR Feature Groups**, select the **Meta Quest** feature group.
3. If disabled, enable the **Meta Quest: Colocation Discovery** OpenXR feature.

## Check support

The colocation discovery feature is enabled on supported devices and disabled on unsupported devices. The following example code checks whether the feature is supported on a target device by checking whether the feature is enabled:

[!code-cs[CheckIsColocationDiscoverySupported](../../Tests/Runtime/CodeSamples/ColocationDiscoverySample.cs#CheckIsColocationDiscoverySupported)]

## Colocation advertisement

Colocation advertisement is the process of broadcasting a message for colocated users to discover. For example you might broadcast connection details such as your devices local IP address to facilitate colocated users connecting to a networked session. You might also broadcast a [shared anchor's group ID](xref:meta-openxr-shared-anchors#share-group-id) for users to synchronize reference points in your space.

> [!NOTE]
> Colocation advertisement should not be used as an alternative to traditional networking frameworks, such as [Netcode for GameObjects](https://docs-multiplayer.unity3d.com/netcode/current/about/), for creating multiplayer experiences. It is intended to simplify the initial connection process for colocated users to get into a networked session.

### Start advertisement

When you start colocation advertisement, the [advertisementState](xref:UnityEngine.XR.OpenXR.Features.Meta.ColocationDiscoveryFeature.advertisementState) transitions between `Inactive`, `Starting`, and `Active`. You can be notified when the advertisement state changes by subscribing to [ColocationDiscoveryFeature.advertisementStateChanged](xref:UnityEngine.XR.OpenXR.Features.Meta.ColocationDiscoveryFeature.advertisementStateChanged). You can query the current advertisement state via `ColocationDiscoveryFeature.advertisementState`. During the transition period when the state is `Starting` can fail and return back to `Inactive`. If the `advertisementState` is not `Inactive` when calling `TryStartAdvertisementAsync`, the request will fail with [XRResultStatus.StatusCode.ValidationFailure](xref:UnityEngine.XR.ARSubsystems.XRResultStatus.StatusCode.ValidationFailure).

To start colocation advertisement, call [ColocationDiscoveryFeature.TryStartAdvertisementAsync](xref:UnityEngine.XR.OpenXR.Features.Meta.ColocationDiscoveryFeature.TryStartAdvertisementAsync(Span{System.Byte})) as shown in the following code example:

[!code-cs[StartAdvertisementAsync](../../Tests/Runtime/CodeSamples/ColocationDiscoverySample.cs#StartAdvertisementAsync)]

### Stop advertisement

When you stop colocation advertisement, the `advertisementState` transitions between `Active`, `Stopping`, and `Inactive`. The transition period when the state is `Stopping` can fail and return back to `Active`. If the `advertisementState` is not `Active` when calling `TryStopAdvertisementAsync`, the request will fail with `XRResultStatus.StatusCode.ValidationFailure`.

To stop colocation advertisement, call [ColocationDiscoveryFeature.TryStopAdvertisementAsync](xref:UnityEngine.XR.OpenXR.Features.Meta.ColocationDiscoveryFeature.TryStopAdvertisementAsync) as shown in the following code example:

[!code-cs[StopAdvertisementAsync](../../Tests/Runtime/CodeSamples/ColocationDiscoverySample.cs#StopAdvertisementAsync)]

The OpenXR runtime can also stop advertisement on its own without a request to stop it from your app. When the runtime stops advertisement, it invokes `ColocationDiscoveryFeature.advertisementStateChanged` and sets the `ColocationState` to `ColocationState.Inactive`. If discovery stopped and you did not request it to stop, you can check the result passed with the event for an error with [Result.status.IsError()](xref:UnityEngine.XR.ARSubsystems.XRResultStatus.IsError) and inspect the [Result.status.nativeStatusCode](xref:UnityEngine.XR.ARSubsystems.XRResultStatus.nativeStatusCode) to get the native error status code.

### Advertisement message

Colocation advertisement takes a `Span<byte>` message type to broadcast information to colocated users actively engaged in [Colocation discovery](#colocation-discovery). The OpenXR runtime makes a deep copy of this `Span<byte>` and therefore does not require the data to persist after the call to `ColocationDiscoveryFeature.TryStartAdvertisementAsync`.

> [!NOTE]
> The max size of an advertisement message is 1024 bytes.

If you want to share a `SerializableGuid`, such as the [MetaOpenXRAnchorSubystem.sharedAnchorsGroupId](xref:UnityEngine.XR.OpenXR.Features.Meta.MetaOpenXRAnchorSubsystem.sharedAnchorsGroupId), you can get a `NativeArray<byte>` with [SerializableGuid.AsByteNativeArray](xref:UnityEngine.XR.ARSubsystems.SerializableGuid.AsByteNativeArray(Unity.Collections.Allocator)) and pass it with `NativeArray.AsSpan()` as shown in the following code example:

[!code-cs[AdvertiseSerializableGuid](../../Tests/Runtime/CodeSamples/ColocationDiscoverySample.cs#AdvertiseSerializableGuid)]

You can convert a `string` to a byte array and share it as shown in the following code example:

[!code-cs[AdvertiseString](../../Tests/Runtime/CodeSamples/ColocationDiscoverySample.cs#AdvertiseString)]

> [!NOTE]
> This example uses `ASCII` for the encoding type but you can use an alternative type that suits your needs.

## Colocation discovery

Colocation discovery is the process of listening for advertised messages from colocated users of your app. For example, someone might broadcast connection details such as an IP address to facilitate users to join a networked session.

### Start discovery

When you start colocation discovery, the [discoveryState](xref:UnityEngine.XR.OpenXR.Features.Meta.ColocationDiscoveryFeature.discoveryState) transitions between `Inactive`, `Starting`, and `Active`. You can be notified when the discovery state changes by subscribing to [ColocationDiscoveryFeature.discoveryStateChanged](xref:UnityEngine.XR.OpenXR.Features.Meta.ColocationDiscoveryFeature.discoveryStateChanged). You can query the current discovery state via `ColocationDiscoveryFeature.discoveryState`. During the transition period when the state is `Starting` can fail and return back to `Inactive`. If the `discoveryState` is not `Inactive` when calling `TryStartDiscoveryAsync`, the request will fail with `XRResultStatus.StatusCode.ValidationFailure`.

To start colocation discovery, call [ColocationDiscoveryFeature.TryStartDiscoveryAsync](xref:UnityEngine.XR.OpenXR.Features.Meta.ColocationDiscoveryFeature.TryStartDiscoveryAsync) as shown in the following code example:

[!code-cs[StartDiscoveryAsync](../../Tests/Runtime/CodeSamples/ColocationDiscoverySample.cs#StartDiscoveryAsync)]

### Stop discovery

When you stop colocation discovery, the `discoveryState` transitions between `Active`, `Stopping`, and `Inactive`. During the transition period when the state is `Stopping` can fail and return back to `Active`. If the `discoveryState` is not `Active` when calling `TryStopDiscoveryAsync`, the request will fail with `XRResultStatus.StatusCode.ValidationFailure`.

To stop colocation discovery, call [ColocationDiscoveryFeature.TryStopDiscoveryAsync](xref:UnityEngine.XR.OpenXR.Features.Meta.ColocationDiscoveryFeature.TryStopDiscoveryAsync) as shown in the following code example:

[!code-cs[StopDiscoveryAsync](../../Tests/Runtime/CodeSamples/ColocationDiscoverySample.cs#StopDiscoveryAsync)]

The OpenXR runtime can also stop discovery on its own without a request to stop it from your app. When the runtime stops discovery, it invokes `ColocationDiscoveryFeature.discoveryStateChanged` and sets the `ColocationState` to `ColocationState.Inactive`. If discovery stopped and you did not request it to stop, you can check the result passed with the event for an error with [Result.status.IsError()](xref:UnityEngine.XR.ARSubsystems.XRResultStatus.IsError) and inspect the [Result.status.nativeStatusCode](xref:UnityEngine.XR.ARSubsystems.XRResultStatus.nativeStatusCode)  to get the native error status code.

### Discovered messages

Subscribe to [ColocationDiscoveryFeature.messageDiscovered](xref:UnityEngine.XR.OpenXR.Features.Meta.ColocationDiscoveryFeature.messageDiscovered) to be notified when a message is discovered. The `messageDiscovered` event passes a [XRColocationDiscoveredMessage](xref:UnityEngine.XR.OpenXR.Features.Meta.XRColocationDiscoveredMessage) that contains a `SerializableGuid` representing the advertisement ID and `NativeArray<byte>` representing the data of the message. The `NativeArray<byte>` is allocated with `Allocator.Temp` and will be disposed of at the end of the frame. The advertisement ID is used to distinguish between different advertised messages. The data contains the message the advertiser is broadcasting.

If the advertised message is a `SerializableGuid`, you can convert the message of bytes back to a `SerializableGuid` by passing the bytes to a constructor of a new `Guid` and passing that guid into the constructor of a new `SerializableGuid`. The following code example demonstrates how to convert bytes to a `SerializableGuid`:

[!code-cs[ConvertBytesToSerializableGuid](../../Tests/Runtime/CodeSamples/ColocationDiscoverySample.cs#ConvertBytesToSerializableGuid)]

If the advertised message is a string, you can convert the message of bytes back to a string by using `Encoding.GetString` as shown in the following code example:

[!code-cs[ConvertBytesToString](../../Tests/Runtime/CodeSamples/ColocationDiscoverySample.cs#ConvertBytesToString)]

> [!NOTE]
> The encoding type must match the encoding type that is used when converting the string to bytes from colocation advertisement.
