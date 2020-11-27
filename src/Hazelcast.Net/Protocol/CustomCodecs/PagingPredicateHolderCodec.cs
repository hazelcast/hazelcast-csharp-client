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

// <auto-generated>
//   This code was generated by a tool.
//     Hazelcast Client Protocol Code Generator
//     https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantUsingDirective
// ReSharper disable CheckNamespace

using System;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Protocol.CustomCodecs
{
    internal static class PagingPredicateHolderCodec
    {
        private const int PageSizeFieldOffset = 0;
        private const int PageFieldOffset = PageSizeFieldOffset + BytesExtensions.SizeOfInt;
        private const int IterationTypeIdFieldOffset = PageFieldOffset + BytesExtensions.SizeOfInt;
        private const int InitialFrameSize = IterationTypeIdFieldOffset + BytesExtensions.SizeOfByte;

        public static void Encode(ClientMessage clientMessage, Hazelcast.Protocol.Models.PagingPredicateHolder pagingPredicateHolder)
        {
            clientMessage.Append(Frame.CreateBeginStruct());

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            initialFrame.Bytes.WriteIntL(PageSizeFieldOffset, pagingPredicateHolder.PageSize);
            initialFrame.Bytes.WriteIntL(PageFieldOffset, pagingPredicateHolder.Page);
            initialFrame.Bytes.WriteByteL(IterationTypeIdFieldOffset, pagingPredicateHolder.IterationTypeId);
            clientMessage.Append(initialFrame);

            AnchorDataListHolderCodec.Encode(clientMessage, pagingPredicateHolder.AnchorDataListHolder);
            CodecUtil.EncodeNullable(clientMessage, pagingPredicateHolder.PredicateData, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, pagingPredicateHolder.ComparatorData, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, pagingPredicateHolder.PartitionKeyData, DataCodec.Encode);

            clientMessage.Append(Frame.CreateEndStruct());
        }

        public static Hazelcast.Protocol.Models.PagingPredicateHolder Decode(IEnumerator<Frame> iterator)
        {
            // begin frame
            iterator.Take();

            var initialFrame = iterator.Take();
            var pageSize = initialFrame.Bytes.ReadIntL(PageSizeFieldOffset);

            var page = initialFrame.Bytes.ReadIntL(PageFieldOffset);
            var iterationTypeId = initialFrame.Bytes.ReadByteL(IterationTypeIdFieldOffset);
            var anchorDataListHolder = AnchorDataListHolderCodec.Decode(iterator);
            var predicateData = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
            var comparatorData = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
            var partitionKeyData = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);

            iterator.SkipToStructEnd();
            return new Hazelcast.Protocol.Models.PagingPredicateHolder(anchorDataListHolder, predicateData, comparatorData, pageSize, page, iterationTypeId, partitionKeyData);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members
