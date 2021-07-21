// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Sql
{
    /// <summary>
    /// SQL column type.
    /// </summary>
    public enum SqlColumnType
    {
        /// <summary>
        /// <c>VARCHAR</c> type, represented by <c>java.lang.String</c> in Java and by <see cref="string"/> in .NET.
        /// </summary>
        Varchar = 0,

        /// <summary>
        /// <c>BOOLEAN</c> type, represented by <c>java.lang.Boolean</c> in Java and by <see cref="bool"/> in .NET.
        /// </summary>
        Boolean = 1,

        // FIXME [Oleksii] discuss sign preservation
        /// <summary>
        /// <c>TINYINT</c> type, represented by <c>java.lang.Byte</c> in Java and by <see cref="byte"/> in .NET.
        /// </summary>
        TinyInt = 2,

        /// <summary>
        /// <c>SMALLINT</c> type, represented by <c>java.lang.Short</c> in Java and by <see cref="short"/> in .NET.
        /// </summary>
        SmallInt = 3,

        /// <summary>
        /// <c>INTEGER</c> type, represented by <c>java.lang.Integer</c> in Java and by <see cref="int"/> in .NET.
        /// </summary>
        Integer = 4,

        /// <summary>
        /// <c>BIGINT</c> type, represented by <c>java.lang.Long</c> in Java and by <see cref="long"/> in .NET.
        /// </summary>
        BigInt = 5,

        /// <summary>
        /// <c>DECIMAL</c> type, represented by <c>java.lang.BigDecimal</c> in Java and by <see cref="string"/> in .NET.
        /// </summary>
        Decimal = 6,

        /// <summary>
        /// <c>REAL</c> type, represented by <c>java.lang.Float</c> in Java and by <see cref="float"/> in .NET.
        /// </summary>
        Real = 7,

        /// <summary>
        /// <c>DOUBLE</c> type, represented by <c>java.lang.Double</c> in Java and by <see cref="double"/> in .NET.
        /// </summary>
        Double = 8,

        // FIXME [Oleksii] discuss year range in HZ SQL and Java
        /// <summary>
        /// <c>DATE</c> type, represented by <c>java.lang.LocalDate</c> in Java and by <see cref="string"/> in .NET.
        /// </summary>
        Date = 9,

        /// <summary>
        /// <c>TIME</c> type, represented by <c>java.lang.LocalTime</c> in Java and by <see cref="string"/> in .NET.
        /// </summary>
        Time = 10,

        /// <summary>
        /// <c>TIMESTAMP</c> type, represented by <c>java.lang.LocalDateTime</c> in Java and by <see cref="string"/> in .NET.
        /// </summary>
        Timestamp = 11,

        /// <summary>
        /// <c>TIMESTAMP_WITH_TIME_ZONE</c> type, represented by <c>java.lang.OffsetDateTime</c> in Java and by <see cref="string"/> in .NET.
        /// </summary>
        TimestampWithTimeZone = 12,

        /// <summary>
        /// <c>OBJECT</c> type, could be represented by any Java and .NET class.
        /// </summary>
        Object = 13,

        /// <summary>
        /// The type of the generic SQL <c>NULL</c> literal. <para/>
        /// The only valid value of <c>NULL</c> type is <c>null</c>.
        /// </summary>
        Null = 14
    }
}