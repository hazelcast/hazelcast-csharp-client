using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace AsyncTests1
{
    [TestFixture]
    public class HashTests
    {
        [Test]
        public void Test()
        {
            var d = new Dictionary<Thing, string>();
            d[new Thing(1)] = "a";
            d[new Thing(2)] = "b";
            d[new Thing(1)] = "c";
            Assert.AreEqual(2, d.Count);

            // the actual key is the hash of the object
        }

        public class Thing
        {
            public Thing(int value)
            {
                Value = value;
            }

            public int Value { get; }

            public override int GetHashCode()
            {
                //return base.GetHashCode();
                return Value;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Thing other)) return false;
                return other.Value == Value;
            }
        }
    }
}
