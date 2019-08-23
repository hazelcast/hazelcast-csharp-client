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
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.Transaction;
using Hazelcast.Util;

namespace Hazelcast.Client.Proxy
{
    sealed class TransactionProxy
    {
        [ThreadStatic] static bool? _threadFlag;

        readonly HazelcastClient _client;

        readonly TransactionOptions _options;
        readonly long _threadId = ThreadUtil.GetThreadId();

        readonly IMember _txOwner;

        long _startTime;
        TransactionState _state = TransactionState.NoTxn;

        internal TransactionProxy(HazelcastClient client, TransactionOptions options, IMember txOwner)
        {
            _options = options;
            _client = client;
            _txOwner = txOwner;
        }

        public TransactionState GetState()
        {
            return _state;
        }

        public long GetTimeoutMillis()
        {
            return _options.GetTimeoutMillis();
        }

        public string Id { get; private set; }

        public void Begin()
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
                var request = TransactionCreateCodec.EncodeRequest(GetTimeoutMillis(), _options.GetDurability(),
                    (int) _options.GetTransactionType(), _threadId);
                var response = Invoke(request);
                Id = TransactionCreateCodec.DecodeResponse(response).response;
                _state = TransactionState.Active;
            }
            catch (Exception e)
            {
                _threadFlag = null;
                throw ExceptionUtil.Rethrow(e);
            }
        }

        public void Commit(bool prepareAndCommit)
        {
            try
            {
                if (_state != TransactionState.Active)
                {
                    throw new TransactionNotActiveException("Transaction is not active");
                }
                CheckThread();
                CheckTimeout();
                var request = TransactionCommitCodec.EncodeRequest(Id, _threadId);
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

        public void Rollback()
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
                    var request = TransactionRollbackCodec.EncodeRequest(Id, _threadId);
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

        void CheckThread()
        {
            if (_threadId != Thread.CurrentThread.ManagedThreadId)
            {
                throw new InvalidOperationException("Transaction cannot span multiple threads!");
            }
        }

        void CheckTimeout()
        {
            if (_startTime + _options.GetTimeoutMillis() < Clock.CurrentTimeMillis())
            {
                throw new TransactionException("Transaction is timed-out!");
            }
        }

        IClientMessage Invoke(IClientMessage request)
        {
            var rpc = _client.GetInvocationService();
            try
            {
                return rpc.InvokeOnMember(request, _txOwner);
            }
            catch (Exception e)
            {
                throw ExceptionUtil.Rethrow(e);
            }
        }
    }
}