using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace Unity.XR.Hands.Tests.NUnitExtensions
{
    class FloatTryGetEqualityComparer : IEqualityComparer<(bool, float)>
    {
        FloatEqualityComparer m_FloatComparer;

        public FloatTryGetEqualityComparer(float allowedError)
        {
            m_FloatComparer = new FloatEqualityComparer(allowedError);
        }

        public bool Equals((bool, float) lhs, (bool, float) rhs)
        {
            return (lhs.Item1 == rhs.Item1) && m_FloatComparer.Equals(lhs.Item2, rhs.Item2);
        }

        public int GetHashCode((bool, float) obj)
        {
            return 0;
        }
    }

    class PoseTryGetEqualityComparer : IEqualityComparer<(bool, Pose)>
    {
        PoseEqualityComparer m_PoseComparer;

        public PoseTryGetEqualityComparer(float allowedError)
        {
            m_PoseComparer = new PoseEqualityComparer(allowedError);
        }

        public bool Equals((bool, Pose) lhs, (bool, Pose) rhs)
        {
            return (lhs.Item1 == rhs.Item1) && m_PoseComparer.Equals(lhs.Item2, rhs.Item2);
        }

        public int GetHashCode((bool, Pose) obj)
        {
            return 0;
        }
    }

    class PoseEqualityComparer : IEqualityComparer<Pose>
    {
        readonly float m_AllowedError;

        Vector3EqualityComparer m_Vector3Comparer;

        public PoseEqualityComparer(float allowedError)
        {
            m_AllowedError = allowedError;
            m_Vector3Comparer = new Vector3EqualityComparer(allowedError);
        }

        public bool Equals(Pose expected, Pose actual)
        {
            return m_Vector3Comparer.Equals(expected.position, actual.position) &&
                   (Mathf.Abs(expected.rotation.x - actual.rotation.x) < m_AllowedError) &&
                   (Mathf.Abs(expected.rotation.y - actual.rotation.y) < m_AllowedError) &&
                   (Mathf.Abs(expected.rotation.z - actual.rotation.z) < m_AllowedError) &&
                   (Mathf.Abs(expected.rotation.w - actual.rotation.w) < m_AllowedError);
        }

        public int GetHashCode(Pose pose)
        {
            return 0;
        }
    }

    class Is : NUnit.Framework.Is
    {
        static readonly float k_EpsilonSqrt = Mathf.Sqrt(Mathf.Epsilon);

        public static EqualConstraint EqualTo((bool, float) expected)
        {
            return NUnit.Framework.Is.EqualTo(expected).Using(new FloatTryGetEqualityComparer(k_EpsilonSqrt));
        }

        public static EqualConstraint EqualTo((bool, Pose) expected)
        {
            return NUnit.Framework.Is.EqualTo(expected).Using(new PoseTryGetEqualityComparer(k_EpsilonSqrt));
        }
    }
}
