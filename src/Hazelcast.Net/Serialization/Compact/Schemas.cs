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

#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    internal class Schemas : ISchemas, IDisposable
    {
        private readonly IClusterMessaging _messaging;

        // we are using a ConcurrentDictionary<,> so that this class is multi-threaded
        //
        // we do *not* use an async concurrent dictionary, this means that if a schema is missing,
        // two or more parallel flows of execution could fetch it from the cluster at the same time,
        // instead of having the first flow block the others. should this eventually become an
        // issue, we would need to implement a proper async concurrent dictionary.
        //
        // we use the schema id as a key, i.e. we assume that if two schemas have the same id, they are the same
        // see notes on Compact Serialization design documents about collision risks with ids
        private readonly ConcurrentDictionary<long, Schema> _schemas = new ConcurrentDictionary<long, Schema>();

        private readonly object _mutex = new object();
        private readonly HashSet<Schema> _toPublish = new HashSet<Schema>(); // protected by _mutex
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private int _publishingCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="Schemas"/> class.
        /// </summary>
        /// <param name="messaging">A messaging service.</param>
        public Schemas(IClusterMessaging messaging)
        {
            _messaging = messaging;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _semaphore.Dispose();
        }

        /// <inheritdoc />
        public void Add(Schema schema, bool isClusterSchema)
        {
            // if the schema was already there, or is already known by the cluster, nothing to do
            if (!_schemas.TryAdd(schema.Id, schema) || isClusterSchema) return;
            
            // if the schema has been added (is new) and is not a cluster schema, it needs to be published
            // hashset: Schema overrides GetHashCode with schema.Id
            lock (_mutex) _toPublish.Add(schema); 
        }

        /// <inheritdoc />
        public bool TryGet(long id, [NotNullWhen(true)] out Schema? schema)
        {
            schema = null;
            return _schemas.TryGetValue(id, out schema);
        }

        /// <inheritdoc />
        public ValueTask<Schema?> GetOrFetchAsync(long id)
        {
            return _schemas.TryGetValue(id, out var schema)
                ? new ValueTask<Schema?>(schema)
                : FetchAsync(id);
        }

        // internal for tests
        internal async ValueTask<Schema?> FetchAsync(long id)
        {
            var requestMessage = ClientFetchSchemaCodec.EncodeRequest(id);
            var response = await _messaging.SendAsync(requestMessage, CancellationToken.None).CfAwait();
            var schema = ClientFetchSchemaCodec.DecodeResponse(response).Schema;
            if (schema == null) return null;

            // if found, add
            if (_schemas.TryAdd(schema.Id, schema)) 
                return schema;
            
            // not added = was already there, remove from schemas to publish
            lock (_mutex) _toPublish.Remove(schema);
            return schema;
        }
        
        /// <inheritdoc />
        public ValueTask PublishAsync()
        {
            lock (_mutex)
            {
                // if we are not publishing, and have nothing to publish, return
                // (fast path is 1 lock + 2 comparisons)
                if (_publishingCount == 0 && _toPublish.Count == 0) return default;
                
                // else signal we will be publishing
                _publishingCount++;
            }
            
            // else we need to go async and publish for real - one at a time
            return PublishAsyncReallyAsync();
        }
        
        private async ValueTask PublishAsyncReallyAsync()
        {
            // only one at a time, wait until it's our turn
            await _semaphore.WaitAsync().CfAwait();

            // our turn to publish, capture what needs to be published now
            List<Schema> toPublish;
            lock (_mutex)
            {
                // but maybe there is nothing left to do?
                if (_toPublish.Count == 0)
                {
                    _publishingCount--;
                    _semaphore.Release();
                    return;
                }
                
                // else capture
                toPublish = _toPublish.ToList();
            }

            // publish, and remove from list of schemas to be published
            var completed = false;
            try
            {
                await PublishAsync(toPublish).CfAwait();
                lock (_mutex)
                {
                    foreach (var schema in toPublish) _toPublish.Remove(schema);
                    _publishingCount--;
                    completed = true;
                }
            }
            finally
            {
                if (!completed) lock (_mutex) _publishingCount--;
                _semaphore.Release();
            }
        }
        
        // FIXME if SendAsync triggers the event, THEN we create an infinite loop of some sort
        // FIXME need a SendAsync with a triggerSendingMessage boolean flag
        
        private async Task PublishAsync(IList<Schema> schemas)
        {
            switch (schemas.Count)
            {
                case 0:
                    return;
                
                case 1:
                {
                    var schema = schemas[0];
                    var requestMessage = ClientSendSchemaCodec.EncodeRequest(schema);
                    var response = await _messaging.SendAsync(requestMessage, CancellationToken.None).CfAwait();
                    _ = ClientSendSchemaCodec.DecodeResponse(response);
                    break;
                }

                default:
                {
                    var requestMessage = ClientSendAllSchemasCodec.EncodeRequest(schemas);
                    var response = await _messaging.SendAsync(requestMessage, CancellationToken.None).CfAwait();
                    _ = ClientSendAllSchemasCodec.DecodeResponse(response);
                    break;
                }
            }
        }
    }
}
