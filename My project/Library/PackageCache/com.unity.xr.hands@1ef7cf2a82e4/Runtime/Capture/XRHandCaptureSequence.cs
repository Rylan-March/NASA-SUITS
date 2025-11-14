using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.Hands.Capture
{
    /// <summary>
    /// A sequence of captured frames, providing data for hand tracking over a period of time.
    /// </summary>
    public class XRHandCaptureSequence : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The list of captured frames containing hand data.")]
        List<XRHandCaptureFrame> m_Frames = new List<XRHandCaptureFrame>();

        [SerializeField]
        [Tooltip("The duration of the entire captured sequence in seconds.")]
        float m_DurationInSeconds;

        /// <summary>
        /// The duration of the entire captured sequence in seconds.
        /// </summary>
        /// <value> The duration in seconds.</value>
        public float durationInSeconds
        {
            get => m_DurationInSeconds;
            internal set => m_DurationInSeconds = value;
        }

        /// <summary>
        /// The captured frames in this sequence.
        /// </summary>
        /// <value> A read-only list of <see cref="XRHandCaptureFrame"/> objects. </value>
        public IReadOnlyList<XRHandCaptureFrame> frames => m_Frames;

        internal void AddFrame(XRHandCaptureFrame frame)
        {
            m_Frames.Add(frame);
        }
    }
}
