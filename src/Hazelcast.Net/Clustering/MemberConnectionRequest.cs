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
using Hazelcast.Core;
using Hazelcast.Models;

namespace Hazelcast.Clustering
{
    internal class MemberConnectionRequest
    {
        private readonly object _mutex = new();
        private readonly DateTime _requestDate;

        public MemberConnectionRequest(MemberInfo member)
        {
            Member = member;
            _requestDate = DateTime.UtcNow;
        }

        public MemberInfo Member { get; }

        public TimeSpan Elapsed => DateTime.UtcNow - _requestDate;

        public long Time { get; set; } = Clock.Milliseconds;

        public event EventHandler<CompletedEventArgs> Completed;

        public class CompletedEventArgs : EventArgs
        {
            public CompletedEventArgs(bool success)
            {
                Success = success;
            }

            public bool Success { get; }
        }

        public void Complete(bool success)
        {
            lock (_mutex)
            {
                Completed?.Invoke(this, new CompletedEventArgs(success));
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"(MemberConnectionRequest Member = {Member})";
        }
    }
}