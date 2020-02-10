// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using Hazelcast.Client.Network;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Transaction;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    internal sealed class TransactionProxy
    {
        [ThreadStatic] private static bool? _threadFlag;

        private readonly HazelcastClient _client;

        private readonly TransactionOptions _options;
        private readonly long _threadId = ThreadUtil.GetThreadId();

        private readonly Connection _txConnection;

        private long _startTime;
        private TransactionState _state = TransactionState.NoTxn;
        private Guid _txnId;

        internal TransactionProxy(HazelcastClient client, TransactionOptions options, Connection txConnection)
        {
            _options = options;
            _client = client;
            _txConnection = txConnection;
        }

        public TransactionState GetState()
        {
            return _state;
        }

        public Guid GetTxnId()
        {
            return _txnId;
        }

        internal void Begin()
        {
            try
            {
                if (_state == TransactionState.Active)
                {
                    throw new InvalidOperationException("Transaction is already active");
                }
                CheckThread();
                if (_threadFlag != null)
                {
                    throw new InvalidOperationException("Nested transactions are not allowed!");
                }
                _threadFlag = true;
                _startTime = Clock.CurrentTimeMillis();
                var request = TransactionCreateCodec.EncodeRequest(_options.GetTimeoutMillis(), _options.GetDurability(),
                    (int) _options.GetTransactionType(), _threadId);
                var response = Invoke(request);
                _txnId = TransactionCreateCodec.DecodeResponse(response).Response;
                _state = TransactionState.Active;
            }
            catch (Exception e)
            {
                _threadFlag = null;
                throw ExceptionUtil.Rethrow(e);
            }
        }

        internal void Commit(bool prepareAndCommit)
        {
            try
            {
                if (_state != TransactionState.Active)
                {
                    throw new TransactionNotActiveException("Transaction is not active");
                }
                CheckThread();
                CheckTimeout();
                var request = TransactionCommitCodec.EncodeRequest(_txnId, _threadId);
                Invoke(request);
                _state = TransactionState.Committed;
            }
            catch (Exception e)
            {
                _state = TransactionState.RollingBack;
                throw ExceptionUtil.Rethrow(e);
            }
            finally
            {
                _threadFlag = null;
            }
        }

        internal void Rollback()
        {
            try
            {
                if (_state == TransactionState.NoTxn || _state == TransactionState.RolledBack)
                {
                    throw new InvalidOperationException("Transaction is not active");
                }
                if (_state == TransactionState.RollingBack)
                {
                    _state = TransactionState.RolledBack;
                    return;
                }
                CheckThread();
                try
                {
                    var request = TransactionRollbackCodec.EncodeRequest(_txnId, _threadId);
                    Invoke(request);
                }
                catch
                {
                    // ignored
                }
                _state = TransactionState.RolledBack;
            }
            finally
            {
                _threadFlag = null;
            }
        }

        private void CheckThread()
        {
            if (_threadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new InvalidOperationException("Transaction cannot span multiple threads!");
            }
        }

        private void CheckTimeout()
        {
            if (_startTime + _options.GetTimeoutMillis() < Clock.CurrentTimeMillis())
            {
                throw new TransactionException("Transaction is timed-out!");
            }
        }

        private ClientMessage Invoke(ClientMessage request)
        {
            var invocationService = _client.InvocationService;
            try
            {
                var task = invocationService.InvokeOnConnection(request, _txConnection);
                return ThreadUtil.GetResult(task);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e, exception => new TransactionException(exception));
            }
        }
    }
}