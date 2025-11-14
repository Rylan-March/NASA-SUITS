using System;
using Unity.Collections;

namespace UnityEngine.XR.Hands.Capture.Recording
{
    struct XRHandRecordingRawFrame : IDisposable
    {
        NativeArray<byte> m_Blob;
        internal NativeArray<byte> blob => m_Blob;

        internal XRHandRecordingRawFrame(NativeArray<byte> blob)
        {
            m_Blob = blob;
        }

        public void Dispose()
        {
            if (m_Blob.IsCreated)
            {
                m_Blob.Dispose();
            }
        }
    }
}
