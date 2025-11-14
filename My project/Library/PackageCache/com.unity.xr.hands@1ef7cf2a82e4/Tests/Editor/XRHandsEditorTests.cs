#if UNITY_EDITOR
using NUnit.Framework;
using System.Reflection;
using UnityEngine.XR.Hands;

#if UNITY_OPENXR_PACKAGE
using UnityEngine.XR.Hands.OpenXR;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace UnityEditor.XR.Hands.Tests
{
    class XRHandsEditorTests
    {
        [Test]
        public void DocumentationVersion()
        {
            var myPackage = UnityEditor.PackageManager.PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
            if (myPackage == null)
                Assert.Fail();

            // allow for experimental/pre-release versions to go out without breaking docs links
            if (myPackage.version.Contains("-"))
                Assert.Pass();

            // We only need the major and minor version from the package, since that's what matters when referencing
            // the docs pages. i.e: 1.3.1 would be referred to as -> 1.3
            var splitVersion = myPackage.version.Split('.');
            var majorMinorVersion = $"{splitVersion[0]}.{splitVersion[1]}"; // Only use major and minor version

            Assert.AreEqual(majorMinorVersion, XRHelpURLConstants.currentDocsVersion);
        }

#if UNITY_OPENXR_PACKAGE
        [Test]
        public void OpenXRDocumentationVersion()
        {
            var myPackage = UnityEditor.PackageManager.PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
            if (myPackage == null)
                Assert.Fail();

            // allow for experimental/pre-release versions to go out without breaking docs links
            if (myPackage.version.Contains("-"))
                Assert.Pass();

            // We only need the major and minor version from the package, since that's what matters when referencing
            // the docs pages. i.e: 1.3.1 would be referred to as -> 1.3
            var splitVersion = myPackage.version.Split('.');
            var majorMinorVersion = $"{splitVersion[0]}.{splitVersion[1]}"; // Only use major and minor version

            UnityEngine.Debug.Log(typeof(HandTracking).GetCustomAttribute<OpenXRFeatureAttribute>().DocumentationLink);
            Assert.IsTrue(typeof(HandTracking).GetCustomAttribute<OpenXRFeatureAttribute>().DocumentationLink.Contains(majorMinorVersion));

            UnityEngine.Debug.Log(typeof(MetaHandTrackingAim).GetCustomAttribute<OpenXRFeatureAttribute>().DocumentationLink);
            Assert.IsTrue(typeof(MetaHandTrackingAim).GetCustomAttribute<OpenXRFeatureAttribute>().DocumentationLink.Contains(majorMinorVersion));
        }
#endif
    }
}
#endif
