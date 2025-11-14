using System;
using System.Linq;
using NUnit.Framework;
using static UnityEngine.XR.OpenXR.Features.Meta.SystemCapabilityUtils;

namespace UnityEditor.XR.OpenXR.Features.Meta.Tests
{
    class SystemCapabilityUtilsTests
    {
        [Test]
        public void InfosByCapability_ContainsAllCapabilityValues()
        {
            var allPossibleCapabilities = (SystemCapability[])Enum.GetValues(typeof(SystemCapability));
            var capabilitiesInDictionary = GetCachedSystemCapabilities().Keys;
            foreach (var capability in allPossibleCapabilities)
            {
                Assert.That(capabilitiesInDictionary.Contains(capability));
            }
        }
    }
}
