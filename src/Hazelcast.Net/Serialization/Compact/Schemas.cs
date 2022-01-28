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

using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Implements <see cref="ISchemas"/>
    /// </summary>
    internal class Schemas : ISchemas
    {
        private class PublishedSchema
        {
            public Schema Schema { get; set; }

            public bool Published { get; set; }
        }

        private readonly IClusterMessaging _messaging;

        // we are using a ConcurrentDictionary<,> so that this class is multi-threaded
        //
        // we do *not* use an async concurrent dictionary, this means that if a schema is missing,
        // two or more parallel flows of execution could fetch it from the cluster at the same time,
        // instead of having the first flow block the others. should this eventually become an
        // issue, we would need to implement a proper async concurrent dictionary.
        // TODO: or, do it now, as maybe Add() or TryGet() would turn async? NO THEY SHOULD NOT.
        //
        // we use the schema id as a key, i.e. we assume that if two schemas have the same id, they are the same
        // see notes on Compact Serialization design documents about collision risks with ids
        private readonly ConcurrentDictionary<long, PublishedSchema> _schemas = new ConcurrentDictionary<long, PublishedSchema>();

        public Schemas(IClusterMessaging messaging)
        {
            _messaging = messaging;
        }

        /// <inheritdoc />
        public void Add(Schema schema, bool published = false)
        {
            // don't _schemas[]= because the schema might have be there already
            // we assume that if two schemas have the same id, they are the same
            _schemas.GetOrAdd(schema.Id, _ => new PublishedSchema { Schema = schema, Published = false });
        }

        /// <inheritdoc />
        public bool TryGet(long id, out Schema schema)
        {
            if (_schemas.TryGetValue(id, out var publishedSchema))
            {
                schema = publishedSchema.Schema;
                return true;
            }

            schema = null;
            return false;
        }

        /// <inheritdoc />
        public ValueTask<Schema> GetOrFetchAsync(long id)
        {
            return _schemas.TryGetValue(id, out var publishedSchema)
                ? new ValueTask<Schema>(publishedSchema.Schema)
                : FetchAsync(id);
        }

        private async ValueTask<Schema> FetchAsync(long id)
        {
            var requestMessage = ClientFetchSchemaCodec.EncodeRequest(id);
            var response = await _messaging.SendAsync(requestMessage, CancellationToken.None).CfAwait();
            var schema = ClientFetchSchemaCodec.DecodeResponse(response).Schema;
            if (schema == null) return null;
            // don't _schemas[]= because the schema might have been added already
            _schemas.GetOrAdd(schema.Id, _ => new PublishedSchema { Schema = schema, Published = true });
            return schema;
        }

        /// <inheritdoc />
        public async ValueTask PublishAsync()
        {
            // ConcurrentDictionary.Values is creating a snapshot that is safe to enumerate
            var schemas = _schemas.Values.Where(x => !x.Published).ToList();

            switch (schemas.Count)
            {
                case 0:
                    break;

                case 1:
                {
                    var schema = schemas[0];
                    var requestMessage = ClientSendSchemaCodec.EncodeRequest(schema.Schema);
                    var response = await _messaging.SendAsync(requestMessage, CancellationToken.None).CfAwait();
                    _ = ClientSendSchemaCodec.DecodeResponse(response);
                    schema.Published = true;
                    break;
                }

                default:
                {
                    var requestMessage = ClientSendAllSchemasCodec.EncodeRequest(schemas.Select(x => x.Schema));
                    var response = await _messaging.SendAsync(requestMessage, CancellationToken.None).CfAwait();
                    _ = ClientSendAllSchemasCodec.DecodeResponse(response);

                    foreach (var schema in schemas) schema.Published = true;
                    break;
                }
            }
        }
    }
}
