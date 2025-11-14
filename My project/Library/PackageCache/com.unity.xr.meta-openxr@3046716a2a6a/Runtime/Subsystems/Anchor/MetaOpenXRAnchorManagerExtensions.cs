using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.XR.CoreUtils.Collections;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.OpenXR.Features.Meta
{
    /// <summary>
    /// OpenXR Meta extension methods for the <see cref="ARAnchorManager"/>.
    /// > [!IMPORTANT]
    /// > These extension methods require that you enable the **Meta Quest: Anchors** feature in
    /// > **Project Settings** > **XR Plug-in Management** > **OpenXR** > **OpenXR Feature Groups**.
    /// </summary>
    /// <seealso cref="ARAnchorFeature"/>
    public static class MetaOpenXRAnchorManagerExtensions
    {
        static StringBuilder s_StringBuilder = new();

        static string GetInvalidSubsystemTypeOverrideMessage(XRAnchorSubsystem subsystem)
        {
            if (subsystem == null)
                return "There is no loaded anchor subsystem. You must initialize XR before sharing an anchor.";

            s_StringBuilder.Append(
                "Unity OpenXR: Meta extension methods require that you enable the Meta Quest: Anchors feature in "
                + "XR Plug-in Management settings. Loaded anchor subsystem is of invalid type: ");
            s_StringBuilder.Append(subsystem.GetType());

            var result = s_StringBuilder.ToString();
            s_StringBuilder.Clear();
            return result;
        }

        /// <summary>
        /// Attempts to share an anchor so that it can be loaded by colocated users of your app with the
        /// <see cref="MetaOpenXRAnchorSubsystem.sharedAnchorsGroupId"/>.
        /// Once shared, anchors cannot be unshared. Shared anchors remain shared for 30 days until they expire.
        /// > [!IMPORTANT]
        /// > This method requires that you enable the **Meta Quest: Anchors** feature in
        /// > **Project Settings** > **XR Plug-in Management** > **OpenXR** > **OpenXR Feature Groups** and it's running.
        /// </summary>
        /// <param name="anchorManager">The `ARAnchorManager` instance.</param>
        /// <param name="anchor">The anchor you wish to share.</param>
        /// <returns>The result of the async operation. You are responsible to <see langword="await"/> this result.</returns>
        /// <exception cref="NullReferenceException">Thrown if `anchorManager` is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="ARAnchorManager.subsystem"/> is not a
        /// <see cref="MetaOpenXRAnchorSubsystem"/>.</exception>
        public static Awaitable<XRResultStatus> TryShareAnchorAsync(this ARAnchorManager anchorManager, ARAnchor anchor)
        {
            if (anchorManager == null)
                throw new NullReferenceException(nameof(anchorManager));

            if (anchorManager.subsystem is not MetaOpenXRAnchorSubsystem metaAnchorSubsystem)
                throw new InvalidOperationException(GetInvalidSubsystemTypeOverrideMessage(anchorManager.subsystem));

            return metaAnchorSubsystem.TryShareAnchorAsync(anchor.trackableId);
        }

        /// <summary>
        /// Attempts to share a batch of anchors so that they can be loaded by colocated users of your app with the
        /// <see cref="MetaOpenXRAnchorSubsystem.sharedAnchorsGroupId"/>. Once shared, anchors cannot be unshared.
        /// Shared anchors remain shared for 30 days until they expire.
        /// > [!IMPORTANT]
        /// > This method requires that you enable the **Meta Quest: Anchors** feature in
        /// > **Project Settings** > **XR Plug-in Management** > **OpenXR** > **OpenXR Feature Groups** and it's running.
        /// </summary>
        /// <param name="anchorManager">The `ARAnchorManager` instance.</param>
        /// <param name="anchors">The anchors you wish to share.</param>
        /// <param name="outputShareAnchorResults">The output list that will be cleared and populated with
        /// results.</param>
        /// <returns>The async operation. You are responsible to <see langword="await"/> this `Awaitable` before you read
        /// the results.</returns>
        /// <exception cref="NullReferenceException">Thrown if the anchorManager is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="ARAnchorManager.subsystem"/> is not a
        /// <see cref="MetaOpenXRAnchorSubsystem"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="anchors"/> is `null` or
        /// <paramref name="outputShareAnchorResults"/> is `null`.</exception>
        /// <exception cref="NullReferenceException">Thrown if an anchor passed in to share is null.</exception>
        public static async Awaitable TryShareAnchorsAsync(
            this ARAnchorManager anchorManager,
            IEnumerable<ARAnchor> anchors,
            List<XRShareAnchorResult> outputShareAnchorResults)
        {
            if (anchorManager == null)
                throw new NullReferenceException(nameof(anchorManager));

            if (anchorManager.subsystem is not MetaOpenXRAnchorSubsystem metaAnchorSubsystem)
                throw new InvalidOperationException(GetInvalidSubsystemTypeOverrideMessage(anchorManager.subsystem));

            if (anchors == null)
                throw new ArgumentNullException(nameof(anchors));

            if (outputShareAnchorResults == null)
                throw new ArgumentNullException(nameof(outputShareAnchorResults));

            outputShareAnchorResults.Clear();
            if (!anchors.Any())
                return;

            var anchorIdsAsNativeArray = new NativeArray<TrackableId>(anchors.Count(), Allocator.Temp);

            var index = 0;
            foreach (var anchor in anchors)
            {
                anchorIdsAsNativeArray[index] = anchor.trackableId;
                index += 1;
            }

            var xrShareAnchorResults = await metaAnchorSubsystem.TryShareAnchorsAsync(
                anchorIdsAsNativeArray,
                Allocator.Temp);

            foreach (var shareAnchorResult in xrShareAnchorResults)
            {
                outputShareAnchorResults.Add(shareAnchorResult);
            }
        }

        /// <summary>
        /// Attempts to load all anchors that were shared with the <see cref="MetaOpenXRAnchorSubsystem.sharedAnchorsGroupId"/>.
        /// > [!IMPORTANT]
        /// > This method requires that you enable the **Meta Quest: Anchors** feature in
        /// > **Project Settings** > **XR Plug-in Management** > **OpenXR** > **OpenXR Feature Groups** and it's running.
        /// </summary>
        /// <param name="anchorManager">The `ARAnchorManager` instance.</param>
        /// <param name="outputLoadedAnchors">The list that will be populated with anchors as the runtime loads them.</param>
        /// <param name="incrementalResultsCallback">A callback method that will be called when any anchors are loaded.
        /// This callback is invoked at least once if any anchors are successfully loaded, and possibly multiple times
        /// before the async operation completes. Pass a `null` argument for this
        /// parameter to ignore it.</param>
        /// <returns>The result of the async operation. You are responsible to <see langword="await"/> this result.</returns>
        /// <exception cref="NullReferenceException">Thrown if the anchorManager is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="ARAnchorManager.subsystem"/> is not a
        /// <see cref="MetaOpenXRAnchorSubsystem"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="outputLoadedAnchors"/> is `null`.</exception>
        /// <remarks>
        /// If no anchors were shared with <see cref="MetaOpenXRAnchorSubsystem.sharedAnchorsGroupId"/>, then this
        /// method will return 0 anchors and <see cref="XRResultStatus.IsSuccess">status.IsSuccess()</see> will be `true`.
        /// </remarks>
        public static async Awaitable<XRResultStatus> TryLoadAllSharedAnchorsAsync(
            this ARAnchorManager anchorManager,
            List<XRAnchor> outputLoadedAnchors,
            Action<ReadOnlyListSpan<XRAnchor>> incrementalResultsCallback)
        {
            if (anchorManager == null)
                throw new NullReferenceException(nameof(anchorManager));

            if (anchorManager.subsystem is not MetaOpenXRAnchorSubsystem metaAnchorSubsystem)
                throw new InvalidOperationException(GetInvalidSubsystemTypeOverrideMessage(anchorManager.subsystem));

            if (outputLoadedAnchors == null)
                throw new ArgumentNullException(nameof(outputLoadedAnchors));

            var completed = 0;
            var result = await metaAnchorSubsystem.TryLoadAllSharedAnchorsAsync(
                Allocator.Temp,
                loadedAnchors =>
                {
                    foreach (var xrAnchor in loadedAnchors)
                    {
                        outputLoadedAnchors.Add(xrAnchor);
                    }

                    var loadAnchorResults = new ReadOnlyListSpan<XRAnchor>(
                        outputLoadedAnchors,
                        completed,
                        loadedAnchors.Length);

                    incrementalResultsCallback?.Invoke(loadAnchorResults);
                    completed = outputLoadedAnchors.Count;
                });

            return result.status;
        }
    }
}
