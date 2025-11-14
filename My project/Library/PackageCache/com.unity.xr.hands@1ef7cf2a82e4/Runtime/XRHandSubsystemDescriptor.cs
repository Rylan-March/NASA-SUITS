using System;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.Hands.ProviderImplementation;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// Describes the capabilities of an <see cref="XRHandSubsystem"/>.
    /// </summary>
    public class XRHandSubsystemDescriptor
        : SubsystemDescriptorWithProvider<XRHandSubsystem, XRHandSubsystemProvider>
    {
        /// <summary>
        /// Whether the provider can supply aim pose.
        /// </summary>
        public bool supportsAimPose { get; }

        /// <summary>
        /// Whether the provider can supply aim activate value.
        /// </summary>
        public bool supportsAimActivateValue { get; }

        /// <summary>
        /// Whether the provider can supply grasp value.
        /// </summary>
        public bool supportsGraspValue { get; }

        /// <summary>
        /// Whether the provider can supply grip pose.
        /// </summary>
        public bool supportsGripPose { get; }

        /// <summary>
        /// Whether the provider can supply pinch pose.
        /// </summary>
        public bool supportsPinchPose { get; }

        /// <summary>
        /// Whether the provider can supply pinch value.
        /// </summary>
        public bool supportsPinchValue { get; }

        /// <summary>
        /// Whether the provider can supply poke pose.
        /// </summary>
        public bool supportsPokePose { get; }

        /// <summary>
        /// Construction information for the <see cref="XRHandSubsystemDescriptor"/>.
        /// </summary>
        public struct Cinfo : IEquatable<Cinfo>
        {
            /// <summary>
            /// A string identifier used to name the subsystem provider.
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// Specifies the provider implementation type to use for instantiation.
            /// </summary>
            public Type providerType { get; set; }

            /// <summary>
            /// Specifies the <see cref="XRHandSubsystem"/>-derived type that
            /// forwards casted calls to its provider.
            /// </summary>
            /// <value>
            /// The type of the subsystem to use for instantiation. If null,
            /// <see cref="XRHandSubsystem"/> will be instantiated.
            /// </value>
            public Type subsystemTypeOverride { get; set; }

            /// <summary>
            /// Whether the provider can supply aim pose.
            /// </summary>
            public bool supportsAimPose { get; set; }

            /// <summary>
            /// Whether the provider can supply aim activate value.
            /// </summary>
            public bool supportsAimActivateValue { get; set; }

            /// <summary>
            /// Whether the provider can supply grasp value.
            /// </summary>
            public bool supportsGraspValue { get; set; }

            /// <summary>
            /// Whether the provider can supply grip pose.
            /// </summary>
            public bool supportsGripPose { get; set; }

            /// <summary>
            /// Whether the provider can supply pinch pose.
            /// </summary>
            public bool supportsPinchPose { get; set; }

            /// <summary>
            /// Whether the provider can supply pinch value.
            /// </summary>
            public bool supportsPinchValue { get; set; }

            /// <summary>
            /// Whether the provider can supply poke pose.
            /// </summary>
            public bool supportsPokePose { get; set; }

            /// <summary>
            /// Generates a hash suitable for use with containers like <c>HashSet</c> and <c>Dictionary</c>.
            /// </summary>
            /// <returns>A hash code generated from this object's fields.</returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = id != null ? id.GetHashCode() : 0;
                    hashCode = hashCode * 486187739 + (providerType != null ? providerType.GetHashCode() : 0);
                    hashCode = hashCode * 486187739 + (subsystemTypeOverride != null ? subsystemTypeOverride.GetHashCode() : 0);
                    hashCode = hashCode * 486187739 + supportsAimPose.GetHashCode();
                    hashCode = hashCode * 486187739 + supportsAimActivateValue.GetHashCode();
                    hashCode = hashCode * 486187739 + supportsGraspValue.GetHashCode();
                    hashCode = hashCode * 486187739 + supportsGripPose.GetHashCode();
                    hashCode = hashCode * 486187739 + supportsPinchPose.GetHashCode();
                    hashCode = hashCode * 486187739 + supportsPinchValue.GetHashCode();
                    hashCode = hashCode * 486187739 + supportsPokePose.GetHashCode();
                    return hashCode;
                }
            }

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="other">The other <see cref="Cinfo"/> to compare against.</param>
            /// <returns>
            /// Returns <see langword="true"/> if every field in <paramref name="other"/>
            /// is equal to this <see cref="Cinfo"/>, otherwise returns <see langword="true"/>.
            /// </returns>
            public bool Equals(Cinfo other)
            {
                return id == other.id &&
                    providerType == other.providerType &&
                    subsystemTypeOverride == other.subsystemTypeOverride &&
                    supportsAimPose == other.supportsAimPose &&
                    supportsAimActivateValue == other.supportsAimActivateValue &&
                    supportsGraspValue == other.supportsGraspValue &&
                    supportsGripPose == other.supportsGripPose &&
                    supportsPinchPose == other.supportsPinchPose &&
                    supportsPinchValue == other.supportsPinchValue &&
                    supportsPokePose == other.supportsPokePose;
            }

            /// <summary>
            /// Tests for equality.
            /// </summary>
            /// <param name="obj">The `object` to compare against.</param>
            /// <returns>
            /// Returns <see langword="true"/> if <paramref name="obj"/> is of
            /// type <see cref="Cinfo"/> and <see cref="Equals(Cinfo)"/> also
            /// returns <see langword="true"/>; otherwise returns <see langword="false"/>.
            /// </returns>
            public override bool Equals(object obj) => (obj is Cinfo) && Equals((Cinfo)obj);

            /// <summary>
            /// Tests for equality. Same as <see cref="Equals(Cinfo)"/>.
            /// </summary>
            /// <param name="lhs">The left-hand side of the comparison.</param>
            /// <param name="rhs">The right-hand side of the comparison.</param>
            /// <returns>
            /// Returns <see langword="true"/> if <paramref name="lhs"/> is equal
            /// to <paramref name="rhs"/>, otherwise returns <see langword="false"/>.
            /// </returns>
            public static bool operator ==(Cinfo lhs, Cinfo rhs) => lhs.Equals(rhs);

            /// <summary>
            /// Tests for inequality. Same as `!`<see cref="Equals(Cinfo)"/>.
            /// </summary>
            /// <param name="lhs">The left-hand side of the comparison.</param>
            /// <param name="rhs">The right-hand side of the comparison.</param>
            /// <returns>Returns <see langword="true"/> if <paramref name="lhs"/>
            /// is not equal to <paramref name="rhs"/>, otherwise returns
            /// <see langword="false"/>.
            /// </returns>
            public static bool operator !=(Cinfo lhs, Cinfo rhs) => !lhs.Equals(rhs);
        }

        /// <summary>
        /// Registers a new descriptor with the <c>SubsystemManager</c>.
        /// </summary>
        /// <param name="cinfo">The construction information for the new descriptor.</param>
        public static void Register(Cinfo cinfo)
        {
            SubsystemDescriptorStore.RegisterDescriptor(new XRHandSubsystemDescriptor(cinfo));
        }

        XRHandSubsystemDescriptor(Cinfo cinfo)
        {
            id = cinfo.id;
            providerType = cinfo.providerType;
            subsystemTypeOverride = cinfo.subsystemTypeOverride;
            supportsAimPose = cinfo.supportsAimPose;
            supportsAimActivateValue = cinfo.supportsAimActivateValue;
            supportsGraspValue = cinfo.supportsGraspValue;
            supportsGripPose = cinfo.supportsGripPose;
            supportsPinchPose = cinfo.supportsPinchPose;
            supportsPinchValue = cinfo.supportsPinchValue;
            supportsPokePose = cinfo.supportsPokePose;
        }
    }
}
