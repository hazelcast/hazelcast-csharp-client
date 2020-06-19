using System;

namespace Hazelcast.Tests.DotNet
{
    public class EquatableTests
    {
        // contains reference implementations of equatable and equality 

        public class ThingClass : IEquatable<ThingClass>
        {
            public ThingClass(int value)
            {
                Value = value;
            }

            public int Value { get; }

            public bool Equals(ThingClass other)
            {
                if (other is null) return false;
                return ReferenceEquals(this, other) || EqualsN(this, other);
            }

            public override bool Equals(object other)
            {
                if (ReferenceEquals(this, other)) return true;
                return other is ThingClass thing && EqualsN(this, thing);
            }

            public static bool Equals(ThingClass left, ThingClass right) // do we *need* to expose that one?
            {
                if (ReferenceEquals(left, right)) return true;
                if (left is null || right is null) return false;
                return EqualsN(left, right);
            }

            public static bool operator ==(ThingClass left, ThingClass right) => Equals(left, right);
            public static bool operator !=(ThingClass left, ThingClass right) => !Equals(left, right);

            private static bool EqualsN(ThingClass left, ThingClass right)
            {
                // compare fields, assume non-null left and right
                return left.Value == right.Value;
            }

            public override int GetHashCode()
            {
                // compute based upon fields
                return default;
            }
        }

        public readonly struct ThingStruct : IEquatable<ThingStruct>
        {
            public ThingStruct(int value)
            {
                Value = value;
            }

            public int Value { get; }

            public bool Equals(ThingStruct other)
            {
                // compare fields
                return Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                return obj is ThingStruct other && Equals(other);
            }

            public static bool Equals(ThingStruct left, ThingStruct right) // do we *need* to expose that one?
            {
                return left.Equals(right);
            }

            public static bool operator ==(ThingStruct left, ThingStruct right) => left.Equals(right);
            public static bool operator !=(ThingStruct left, ThingStruct right) => !left.Equals(right);

            public override int GetHashCode()
            {
                // compute based upon fields
                return default;
            }
        }

    }
}
