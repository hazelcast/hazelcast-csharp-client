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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

#pragma warning disable CS1591
 namespace Hazelcast.Client.Spi
{
    internal sealed class ClientExecutionService : IClientExecutionService
    {
        private readonly TaskFactory _taskFactory = Task.Factory;
        private readonly AtomicBoolean _live = new AtomicBoolean(true);

        public ClientExecutionService(string name, int poolSize)
        {
        }

        public Task Submit(Action action)
        {
            return _taskFactory.StartNew(action);
        }

        public Task<T> Submit<T>(Func<T> function)
        {
            return _taskFactory.StartNew(function);
        }

        public void ScheduleWithFixedDelay(Action command, TimeSpan initialDelay, TimeSpan period, CancellationToken ct)
        {
            if (!_live.Get())
            {
                throw new HazelcastException("Client is shut down.");
            }

            Task.Run(async () =>
            {
                await Task.Delay(initialDelay, ct);
                while (ct.IsCancellationRequested == false)
                {
                    command();
                    await Task.Delay(period, ct);
                }
            }, ct).IgnoreExceptions();

        }

        public async Task Schedule(Action command, TimeSpan delay, CancellationToken token)
        {
            if (!_live.Get())
            {
                throw new HazelcastException("Client is shut down.");
            }

            await Task.Delay(delay, token);
            command();
        }

        public void Shutdown()
        {
            _live.Set(false);
        }

        internal Task SubmitInternal(Action action)
        {
            //TODO: should use different thread pool
            return _taskFactory.StartNew(action);
        }
    }
}