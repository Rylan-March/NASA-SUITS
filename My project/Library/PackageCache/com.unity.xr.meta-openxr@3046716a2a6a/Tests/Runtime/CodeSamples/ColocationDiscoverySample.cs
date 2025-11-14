using System;
using System.Text;
using Unity.Collections;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta.Tests
{
    public class ColocationDiscoverySample
    {
        #region CheckIsColocationDiscoverySupported
        bool IsColocationDiscoverySupported()
        {
            var settings = OpenXRSettings.Instance;
            var feature = settings.GetFeature<ColocationDiscoveryFeature>();
            return feature.enabled;
        }
        #endregion

        #region StartAdvertisementAsync
        async void StartAdvertisementAsync(
            ColocationDiscoveryFeature colocationDiscovery,
            byte[] message)
        {
            // First subscribe to state changes to know when advertisement is active and
            // when it might change to inactive if the runtime stops advertisement without
            // app request.
            colocationDiscovery.advertisementStateChanged += OnAdvertisementStateChanged;

            var result = await colocationDiscovery.TryStartAdvertisementAsync(
                message.AsSpan());

            if (result.status.IsSuccess())
            {
                // Advertisement has started and is active

                // The ID of the advertisement that was started
                var advertisementId = result.value;
            }
        }

        void OnAdvertisementStateChanged(object sender, Result<ColocationState> result)
        {
            var state = result.value;
            switch (state)
            {
                case ColocationState.Starting:
                    // Advertisement is trying to start
                    break;
                case ColocationState.Active:
                    // Advertisement is active
                    break;
                case ColocationState.Stopping:
                    // Advertisement is trying to stop
                    break;
                case ColocationState.Inactive:
                    // Advertisement is inactive.
                    break;
            }
        }
        #endregion

        #region StopAdvertisementAsync
        async void StopAdvertisementAsync(ColocationDiscoveryFeature colocationDiscovery)
        {
            var resultStatus = await colocationDiscovery.TryStopAdvertisementAsync();

            if (resultStatus.IsSuccess())
            {
                // Advertisement has stopped and is inactive
            }
        }
        #endregion

        #region AdvertiseSerializableGuid
        async void AdvertiseSerializableGuid(
            ColocationDiscoveryFeature colocationDiscovery,
            SerializableGuid guid)
        {
            var bytes = guid.AsByteNativeArray();
            var result = await colocationDiscovery.TryStartAdvertisementAsync(
                bytes.AsSpan());

            if (result.status.IsSuccess())
            {
                // Advertisement started and is active
            }
        }
        #endregion

        #region AdvertiseString
        async void AdvertiseString(
            ColocationDiscoveryFeature colocationDiscovery,
            string message)
        {
            var bytes = Encoding.ASCII.GetBytes(message);
            var result = await colocationDiscovery.TryStartAdvertisementAsync(
                bytes.AsSpan());

            if (result.status.IsSuccess())
            {
                // Advertisement has started and is active
            }
        }
        #endregion

        #region StartDiscoveryAsync
        async void StartDiscoveryAsync(ColocationDiscoveryFeature colocationDiscovery)
        {
            // First subscribe to state changes to know when discovery is active and
            // when it might change to inactive if the runtime stops discovery without
            // app request.
            colocationDiscovery.discoveryStateChanged += OnDiscoveryStateChanged;

            // Subscribe to message discovery events
            colocationDiscovery.messageDiscovered += OnMessageDiscovered;

            var resultStatus = await colocationDiscovery.TryStartDiscoveryAsync();
            if (resultStatus.IsSuccess())
            {
                // Discovery has started and is active
            }
        }

        void OnDiscoveryStateChanged(object sender, Result<ColocationState> result)
        {
            var state = result.value;
            switch (state)
            {
                case ColocationState.Starting:
                    // Discovery is about to start
                    break;
                case ColocationState.Active:
                    // Discovery is active
                    break;
                case ColocationState.Stopping:
                    // Discovery is about to stop
                    break;
                case ColocationState.Inactive:
                    // Discovery is inactive. Inspect result.status if your app did not
                    // request discovery to stop to learn why the runtime stopped
                    // discovery.
                    break;
            }
        }

        void OnMessageDiscovered(object sender, ColocationDiscoveryMessage message)
        {
            // Convert message from bytes back to the structure it was advertised as.
        }
        #endregion

        #region StopDiscoveryAsync
        async void StopDiscoveryAsync(ColocationDiscoveryFeature colocationDiscovery)
        {
            var resultStatus = await colocationDiscovery.TryStopDiscoveryAsync();

            if (resultStatus.IsSuccess())
            {
                // Discovery has stopped and is inactive
            }
        }
        #endregion

        #region ConvertBytesToSerializableGuid
        void ConvertBytesToSerializableGuid(NativeArray<byte> bytes)
        {
            var guid = new Guid(bytes);
            var serializedGuid = new SerializableGuid(guid);
        }
        #endregion

        #region ConvertBytesToString
        void ConvertBytesToString(NativeArray<byte> bytes)
        {
            var message = Encoding.ASCII.GetString(bytes);
        }
        #endregion
    }
}
