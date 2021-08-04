// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

using System.Linq;
using Hazelcast.Sql;
using NUnit.Framework;

namespace Hazelcast.Tests.Sql
{
    [TestFixture]
    public class SqlJetTests : SqlTestBase
    {
        // enable Jet
        protected override string RcClusterConfiguration =>
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<hazelcast xmlns=\"http://www.hazelcast.com/schema/config\"" +
            "  xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
            "  xsi:schemaLocation=\"http://www.hazelcast.com/schema/config" +
            "  http://www.hazelcast.com/schema/config/hazelcast-config-5.0.xsd\">" +
            "  <jet enabled=\"true\"></jet>" +
            "</hazelcast>";

        [Test]
        [TestCase(3, 1)]
        [TestCase(3, 3)]
        [TestCase(3, 5)]
        [TestCase(5, 2)]
        [TestCase(100, 15)]
        public void ExecuteQueryJet(int total, int pageSize)
        {
            var result = Client.Sql.ExecuteQuery($"SELECT v FROM TABLE(generate_series(1,{total}))",
                options: new SqlStatementOptions { CursorBufferSize = pageSize }
            );

            var expectedValues = Enumerable.Range(1, total);
            var resultValues = result.EnumerateOnce().Select(r => r.GetColumn<int>("v"));

            CollectionAssert.AreEquivalent(expectedValues, resultValues);
        }
    }
}