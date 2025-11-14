using System;
using System.Collections.Generic;
using Unity.XR.CompositionLayers.Services;
using UnityEngine;
using LayerInfo = Unity.XR.CompositionLayers.Services.CompositionLayerManager.LayerInfo;

namespace Unity.XR.CompositionLayers.Provider
{
    /// <summary>
    /// Interface that defines the API for an ILayerProvider.
    /// </summary>
    public interface ILayerProvider
    {
        /// <summary>
        /// Sets the layer provider state on first assignment to the <see cref="CompositionLayerManager" />.
        /// </summary>
        /// <param name="layers">The list of all currently known <see cref="CompositionLayer"/> instances, regardless of active state.</param>
        void SetInitialState(List<LayerInfo> layers);

        /// <summary>
        /// Tell the provider to clean up all layer state it maintains.
        /// </summary>
        void CleanupState();

        /// <summary>
        /// Called by the <see cref="CompositionLayerManager" /> to tell the instance of <see cref="ILayerProvider" /> about
        /// the current state of layers it is managing.
        /// </summary>
        ///
        /// <param name="createdLayers">The list of layers that were just created. Any layer in
        /// this list may be in the <paramref name="activeLayers" /> list if it is activated in the same frame.
        /// Any layer in this list should not be in <paramref name="modifiedLayers" /> or <paramref name="removedLayers" />.
        /// This list is ephemeral and cleared after each call.</param>
        ///
        /// <param name="removedLayers">The list of layers that are no longer being managed. Any layer in
        /// this list should not be in the <paramref name="createdLayers" />, <paramref name="modifiedLayers" />, or
        /// <paramref name="activeLayers" /> lists.
        /// This list is ephemeral and cleared after each call.</param>
        ///
        /// <param name="modifiedLayers">The list of layers that have been recently modified. Any layer in
        /// this list may also be in the <paramref name="activeLayers" /> list. Any layer in this list should not
        /// be in <paramref name="createdLayers" /> or <paramref name="removedLayers" />.
        /// This list is ephemeral and cleared after each call.</param>
        ///
        /// <param name="activeLayers">The list of layers currently active within the scene.
        /// Layers in this list may also be in the <paramref name="createdLayers" /> or <paramref name="modifiedLayers" /> lists
        /// if they became active in the same frame.</param>
        void UpdateLayers(List<LayerInfo> createdLayers, List<int> removedLayers, List<LayerInfo> modifiedLayers, List<LayerInfo> activeLayers);

        /// <summary>
        /// Called by the <see cref="CompositionLayerManager" /> to pass calls to LateUpdate through to
        /// the <see cref="ILayerProvider" />.
        /// </summary>
        void LateUpdate();
    }
}
