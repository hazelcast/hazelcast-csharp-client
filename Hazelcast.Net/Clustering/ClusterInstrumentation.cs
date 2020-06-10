// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Messaging;

namespace Hazelcast.Clustering
{
    public class ClusterInstrumentation
    {
        private int _missedEventsCount;
        private int _exceptionsInEventHandlersCount;

        public int MissedEventsCount => _missedEventsCount;

        public int ExceptionsInEventHandlersCount => _exceptionsInEventHandlersCount;

#pragma warning disable CA1801 // Review unused parameters - for later usage
#pragma warning disable IDE0060 // Remove unused parameter
        public void CountMissedEvent(ClientMessage message)
#pragma warning restore IDE0060
#pragma warning restore CA1801
            => Interlocked.Increment(ref _missedEventsCount);

#pragma warning disable CA1801 // Review unused parameters - for later usage
#pragma warning disable IDE0060 // Remove unused parameter
        public void CountExceptionInEventHandler(Exception exception)
#pragma warning restore IDE0060
#pragma warning restore CA1801
            => Interlocked.Increment(ref _exceptionsInEventHandlersCount);
    }
}
