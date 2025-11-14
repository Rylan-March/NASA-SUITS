using NUnit.Framework;
using UnityEditor.XR.Hands.Analytics;

namespace UnityEditor.XR.Hands.Tests
{
    class XRHandsEditorAnalyticsTests
    {
        [Test]
        public void XRHandCaptureAnalyticsDataDefaultValues()
        {
            var analyticsData = new XRHandCaptureAnalyticsData();
            Assert.IsFalse(analyticsData.recordingsImported);
            Assert.IsFalse(analyticsData.newHandShapeSaved);
        }

        [Test]
        public void XRHandCaptureAnalyticsDataSetOnlyRecordingsImported()
        {
            var analyticsData = new XRHandCaptureAnalyticsData
            {
                recordingsImported = true
            };

            Assert.IsTrue(analyticsData.recordingsImported);
            Assert.IsFalse(analyticsData.newHandShapeSaved);
        }

        [Test]
        public void XRHandCaptureAnalyticsDataSetOnlyNewHandShapeSaved()
        {
            var analyticsData = new XRHandCaptureAnalyticsData
            {
                newHandShapeSaved = true
            };

            Assert.IsFalse(analyticsData.recordingsImported);
            Assert.IsTrue(analyticsData.newHandShapeSaved);
        }

        [Test]
        public void XRHandCaptureAnalyticsDataSetAll()
        {
            var analyticsData = new XRHandCaptureAnalyticsData
            {
                recordingsImported = true,
                newHandShapeSaved = true
            };

            Assert.IsTrue(analyticsData.recordingsImported);
            Assert.IsTrue(analyticsData.newHandShapeSaved);
        }
    }
}
