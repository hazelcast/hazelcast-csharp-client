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

using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Hazelcast.Testing;
using Hazelcast.Tests.Serialization.Compact.SchemasRemoteTestsLocal;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    namespace SchemasRemoteTestsLocal
    {
        internal class ClusterMessaging : IClusterMessaging
        {
            private readonly Hazelcast.Clustering.ClusterMessaging _messaging;

            public ClusterMessaging(Hazelcast.Clustering.ClusterMessaging messaging)
            {
                _messaging = messaging;
            }

            public Task<ClientMessage> SendAsync(ClientMessage requestMessage, CancellationToken cancellationToken)
                => _messaging.SendAsync(requestMessage, cancellationToken);
        }

        internal static class Extensions
        {
            public static IClusterMessaging GetMessaging(this IHazelcastClient client)
                => new ClusterMessaging(((HazelcastClient)client).Cluster.Messaging);
        }
    }

    [TestFixture]
    public class SchemasRemoteTests : SingleMemberClientRemoteTestBase
    {
        [Test]
        public async Task CanPublishAndFetchSchemas()
        {
            var schemas = new Schemas(Client.GetMessaging());

            var schema = new Schema("sometype", new[]
            {
                new SchemaField("somefield", FieldKind.StringRef)
            });

            Assert.That(schemas.TryGet(schema.Id, out _), Is.False);
            var fetched = await schemas.GetOrFetchAsync(schema.Id).CfAwait();
            Assert.That(fetched, Is.Null);

            schemas.Add(schema);
            await schemas.PublishAsync().ConfigureAwait(false);

            // use another client which as an empty cache
            // TODO: consider using a static, application-wide cache?
            await using var client2 = await HazelcastClientFactory.StartNewClientAsync(Client.Options).ConfigureAwait(false);
            var schemas2 = new Schemas(client2.GetMessaging());
            Assert.That(schemas2.TryGet(schema.Id, out _), Is.False);
            var fetched2 = await schemas.GetOrFetchAsync(schema.Id).CfAwait();
            Assert.That(fetched2, Is.Not.Null);
            Assert.That(fetched2.Id, Is.EqualTo(schema.Id));
        }

        [Test]
        public async Task CanAddSchema()
        {
            var schemas = new Schemas(Client.GetMessaging());

            var schema = new Schema("sometype", new[]
            {
                new SchemaField("somefield", FieldKind.StringRef)
            });

            Assert.That(schemas.TryGet(schema.Id, out _), Is.False);
            schemas.Add(schema);
            Assert.That(schemas.TryGet(schema.Id, out var returned), Is.True);
            Assert.That(returned.Id, Is.EqualTo(schema.Id));
            returned = await schemas.GetOrFetchAsync(schema.Id).ConfigureAwait(false);
            Assert.That(returned, Is.Not.Null);
            Assert.That(returned.Id, Is.EqualTo(schema.Id));
            // TODO: with mock we can validate that no remote call was made
        }
    }
}
