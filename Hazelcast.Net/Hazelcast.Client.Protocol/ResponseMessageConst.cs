/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace Hazelcast.Client.Protocol
{
    /// <summary>Message type ids of responses in client protocol.</summary>
    /// <remarks>
    ///     Message type ids of responses in client protocol. They also used to bind a request to a response inside Request
    ///     annotation.
    /// </remarks>
    public sealed class ResponseMessageConst
    {
        public const int Void = 100;
        public const int Boolean = 101;
        public const int Integer = 102;
        public const int Long = 103;
        public const int String = 104;
        public const int Data = 105;
        public const int ListData = 106;
        public const int Authentication = 107;
        public const int Partitions = 108;
        public const int Exception = 109;
        public const int DistributedObject = 110;
        public const int EntryView = 111;
        public const int JobProcessInfo = 112;
        public const int SetData = 113;
        public const int SetEntry = 114;
        public const int ReadResultSet = 115;
        public const int CacheKeyIteratorResult = 116;
        public const int ListEntry = 117;
    }
}