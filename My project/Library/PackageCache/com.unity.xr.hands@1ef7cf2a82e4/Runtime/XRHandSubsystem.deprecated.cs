using System;

namespace UnityEngine.XR.Hands
{
    public partial class XRHandSubsystem
    {
        /// <summary>
        /// Obsolete. Use <see cref="updatedHands"/> instead.
        /// </summary>
        [Obsolete("Use updatedHands instead.")]
        public Action<UpdateSuccessFlags, UpdateType> handsUpdated;
    }
}
