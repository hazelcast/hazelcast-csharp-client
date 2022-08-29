// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Models;

namespace Hazelcast.Clustering
{
    internal class MemberConnectionRequest
    {
        private readonly object _mutex = new object();
        private TaskCompletionSource<object> _completionSource;
        private DateTime _requestDate;

        public MemberConnectionRequest(MemberInfo member)
        {
            Member = member;
            Reset();
        }

        public MemberInfo Member { get; }

        public TimeSpan Elapsed => DateTime.UtcNow - _requestDate;

        public event EventHandler Failed;

        public bool Cancelled { get; private set; }

        public bool Completed { get; private set; }

        public void Cancel()
        {
            Cancelled = true;
        }

        public void Complete(bool success)
        {
            // trigger before completing
            // (completing can unlock a suspend wait)
            if (!success)
            {
                Failed?.Invoke(this, default);
            }

            lock (_mutex)
            {
                Completed = true;
                _completionSource?.TrySetResult(null);
            }
        }

        public ValueTask Completion
        {
            get
            {
                lock (_mutex)
                {
                    if (Completed) return default;
                    _completionSource = new TaskCompletionSource<object>();
                }
                return new ValueTask(_completionSource.Task);
            }
        }

        public void Reset()
        {
            Cancelled = false;
            Completed = false;
            Failed = null;
            _completionSource = null;
            _requestDate = DateTime.UtcNow;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"(MemberConnectionRequest Member = {Member}, Cancelled = {Cancelled}, Completed = {Completed})";
        }
    }
}