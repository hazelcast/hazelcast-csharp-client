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

namespace Hazelcast.Core
{
    public class TimeoutCancellationTokenSource : IDisposable
    {
        private readonly CancellationTokenSource _source;
        private readonly CancellationTokenSource _composed;

        public TimeoutCancellationTokenSource(CancellationTokenSource source, int timeoutMilliseconds)
        {
            _source = source;
            if (timeoutMilliseconds < 0)
            {
                _composed = source;
            }
            else
            {
                _composed = CancellationTokenSource.CreateLinkedTokenSource(source.Token);
                _composed.CancelAfter(timeoutMilliseconds);
            }
        }

        public CancellationToken Token => _composed.Token;

        public bool IsCancellationRequested => _composed.IsCancellationRequested;

        public bool HasTimeout => _source != _composed;

        public bool HasTimedOut => _composed.IsCancellationRequested && !_source.IsCancellationRequested;

        public void Dispose()
        {
            if (_source != _composed)
                _composed.Dispose();
        }
    }
}