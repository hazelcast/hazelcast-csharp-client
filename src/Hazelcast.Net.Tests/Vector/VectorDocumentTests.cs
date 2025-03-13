// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using Hazelcast.Models;
using NUnit.Framework;
namespace Hazelcast.Tests.Vector
{
    public class VectorDocumentTests
    {
        [Test]
        public void TestConstructor()
        {
            var vectorValues = VectorValues.Of(new float[] { 1.0f, 2.0f, 3.0f });
            var vectorDocument = VectorDocument<string>.Of("value", vectorValues);
            Assert.AreEqual("value", vectorDocument.Value);
            Assert.AreEqual(vectorValues, vectorDocument.Vectors);
        }

        [Test]
        public void TestConstructorWithMultipleIndexes()
        {
            var pair1 = ("index1", new float[] { 1.0f, 2.0f, 3.0f });
            var pair2 = ("index2", new float[] { 4.0f, 5.0f, 6.0f });
            var vectorValues = VectorValues.Of(pair1, pair2);

            var value = "valueForDoc";
            var vectorDocument = VectorDocument<string>.Of(value, vectorValues);

            Assert.AreEqual(value, vectorDocument.Value);

            Assert.IsInstanceOf<MultiVectorValues>(vectorDocument.Vectors);

            var multiVectorValues = (MultiVectorValues) vectorDocument.Vectors;
            Assert.AreEqual(2, multiVectorValues.IndexNameToVector.Count);
            
            Assert.AreEqual(pair1.Item2, multiVectorValues.IndexNameToVector[pair1.Item1]);
            Assert.AreEqual(pair2.Item2, multiVectorValues.IndexNameToVector[pair2.Item1]);
        }

        [Test]
        public void TestEquals()
        {
            var vectorValues = VectorValues.Of(new float[] { 1.0f, 2.0f, 3.0f });
            var vectorDocument1 = VectorDocument<string>.Of("value", vectorValues);
            var vectorDocument2 = VectorDocument<string>.Of("value", vectorValues);
            Assert.AreEqual(vectorDocument1, vectorDocument2);
        }
    }
}
