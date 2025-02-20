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
    public class VectorValuesTests
    {
        [Test]
        public void TestSingleVectorValues()
        {
            var vectorValues = VectorValues.Of(new float[] { 1.0f, 2.0f, 3.0f });
            Assert.IsInstanceOf<SingleVectorValues>(vectorValues);
            Assert.AreEqual(new float[] { 1.0f, 2.0f, 3.0f }, ((SingleVectorValues)vectorValues).Vector);
        }
        
        [Test]
        public void TestMultiVectorValues()
        {
            var vectorValues = VectorValues.Of(
                ("index1", new float[] { 1.0f, 2.0f, 3.0f }), 
                ("index2", new float[] { 4.0f, 5.0f, 6.0f }));
            
            Assert.IsInstanceOf<MultiVectorValues>(vectorValues);
            Assert.AreEqual(new float[] { 1.0f, 2.0f, 3.0f },
                ((MultiVectorValues)vectorValues).IndexNameToVector["index1"]);
            
            Assert.AreEqual(new float[] { 4.0f, 5.0f, 6.0f },
                ((MultiVectorValues)vectorValues).IndexNameToVector["index2"]);
        }
    }
}
