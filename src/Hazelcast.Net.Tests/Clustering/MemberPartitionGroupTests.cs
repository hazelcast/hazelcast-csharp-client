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
using System;
using System.Collections.Generic;
using Hazelcast.Aggregation;
using Hazelcast.Clustering;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Serialization.ConstantSerializers;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
namespace Hazelcast.Tests.Clustering
{
    public class MemberPartitionGroupTests
    {
        [TestCase("[[ \"fa270257-5767-45bf-a3c6-bafe17bed525\",\"fa756344-97ca-43f5-9d8d-23b960e4d445\"]]", "fa270257-5767-45bf-a3c6-bafe17bed525", 2, 1)]
        [TestCase("[[\"fa270257-5767-45bf-a3c6-bafe17bed525\",\"fa756344-97ca-43f5-9d8d-23b960e4d445\"]]", "fa270257-5767-45bf-a3c6-bafe17bed525", 2, 1)]
        [TestCase("[[\"fa270257-5767-45bf-a3c6-bafe17bed525\"]]", "fa270257-5767-45bf-a3c6-bafe17bed525", 1, 5)]
        [TestCase("[['']]", "", 0, -1)]
        [TestCase("", "", 0, -1)]
        [TestCase("[[\"fa270257-5767-45bf-a3c6-bafe17bed525\",\"fa756344-97ca-43f5-9d8d-23b960e4d445\",\"fa756344-97ca-43f5-9d8d-23b960e4d445\"]]", "fa270257-5767-45bf-a3c6-bafe17bed525", 2, -1)]
        public void TestAuthenticatorCanParseMemberList(string memberList, string memberId, int count, int version)
        {
            ClientAuthenticationCodec.ResponseParameters response = new ClientAuthenticationCodec.ResponseParameters();
            response.KeyValuePairs = new Dictionary<string, string>();
            response.KeyValuePairs[MemberPartitionGroup.PartitionGroupJsonField] = memberList;
            response.KeyValuePairs[MemberPartitionGroup.VersionJsonField] = version.ToString();
            response.ClusterId = Guid.NewGuid();
            response.MemberUuid = string.IsNullOrEmpty(memberId) ? Guid.NewGuid() : Guid.Parse(memberId);

            var serializationService = new SerializationServiceBuilder(new NullLoggerFactory())
                .AddDefinitions(new ConstantSerializerDefinitions()) // use constant serializers not CLR serialization
                .AddHook<AggregatorDataSerializerHook>()
                .Build();

            var authenticator = new Authenticator(new AuthenticationOptions(), serializationService, NullLoggerFactory.Instance);

            var memberGroup = authenticator.ParsePartitionMemberGroups(response);
            Assert.IsNotNull(memberGroup);
            Assert.AreEqual(count, memberGroup.SelectedGroup.Count);
            Assert.AreEqual(version, memberGroup.Version);
            Assert.AreEqual(response.ClusterId, memberGroup.ClusterId);
            Assert.AreEqual(response.MemberUuid, memberGroup.MemberReceivedFrom);
        }

    }
}
