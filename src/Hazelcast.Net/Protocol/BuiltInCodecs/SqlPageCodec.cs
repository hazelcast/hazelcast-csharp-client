// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Sql;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal static class SqlPageCodec
    {
        public static SqlPage Decode(IEnumerator<Frame> iterator)
        {
            iterator.MoveNext();

            var isLast = iterator.Take().Bytes[0] == 1;

            var columnTypeIds = ListIntegerCodec.Decode(iterator);
            var columnTypes = new SqlColumnType[columnTypeIds.Count];
            var columns = new IReadOnlyList<object>[columnTypeIds.Count];

            var i = 0;
            foreach (var columnTypeId in columnTypeIds)
            {
                columnTypes[i] = (SqlColumnType)columnTypeId;
                columns[i] = columnTypes[i] switch
                {
                    // TODO [Oleksii] avoid boxing via generic column types
                    SqlColumnType.Varchar => ListMultiFrameCodec.DecodeContainsNullable(iterator, StringCodec.Decode),
                    SqlColumnType.Boolean => ListCNBoolCodec.Decode(iterator).AsReadOnlyObjectList(),
                    SqlColumnType.TinyInt => ListCNByteCodec.Decode(iterator).AsReadOnlyObjectList(),
                    SqlColumnType.SmallInt => ListCNShortCodec.Decode(iterator).AsReadOnlyObjectList(),
                    SqlColumnType.Integer => ListCNIntCodec.Decode(iterator).AsReadOnlyObjectList(),
                    SqlColumnType.BigInt => ListCNLongCodec.Decode(iterator).AsReadOnlyObjectList(),
                    SqlColumnType.Real => ListCNFloatCodec.Decode(iterator).AsReadOnlyObjectList(),
                    SqlColumnType.Double => ListCNDoubleCodec.Decode(iterator).AsReadOnlyObjectList(),
                    SqlColumnType.Decimal => ListMultiFrameCodec.Decode(iterator, BigDecimalCodec.DecodeNullable).AsReadOnlyObjectList(),
                    SqlColumnType.Date => ListCNLocalDateCodec.Decode(iterator).AsReadOnlyObjectList(),
                    SqlColumnType.Time => ListCNLocalTimeCodec.Decode(iterator).AsReadOnlyObjectList(),
                    SqlColumnType.Timestamp => ListCNLocalDateTimeCodec.Decode(iterator).AsReadOnlyObjectList(),
                    SqlColumnType.TimestampWithTimeZone => ListCNOffsetDateTimeCodec.Decode(iterator).AsReadOnlyObjectList(),
                    SqlColumnType.Object => ListMultiFrameCodec.Decode(iterator, DataCodec.DecodeNullable),
                    SqlColumnType.Json => ListMultiFrameCodec.DecodeContainsNullable(iterator, HazelcastJsonValueCodec.Decode),
                    SqlColumnType.Null => new object[iterator.Take().Bytes.ReadIntL(0)],
                    _ => throw new NotSupportedException($"Column type #{columnTypeId} is not supported.")
                };

                i++;
            }

            iterator.SkipToStructEnd();
            return new SqlPage(columnTypes, columns, isLast);
        }

        public static void Encode(ClientMessage clientMessage, SqlPage page)
        {
            throw new NotSupportedException($"Server-side {nameof(SqlPage)}.{nameof(Encode)} is not supported.");
        }
    }
}
