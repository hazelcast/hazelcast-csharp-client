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
namespace Hazelcast.CodeGenerator;

/// <summary>
/// Provides sample data for tests.
/// </summary>
public class SampleData
{
    /// <summary>
    /// Gets sample data per CLR type.
    /// </summary>
    public static readonly IDictionary<string, string[]> TypeValues = new Dictionary<string, string[]>
    {
        { "bool", new[] { "true", "false" } },
        { "sbyte", new[] { "(sbyte)12", "(sbyte)42" } },
        { "short", new[] { "(short)12", "(short)42" } },
        { "int", new[] { "12", "42" } },
        { "long", new[] { "(long)12", "(long)42" } },
        { "float", new[] { "(float)12", "(float)42" } },
        { "double", new[] { "(double)12", "(double)42" } },
        { "HBigDecimal", new[] { "new HBigDecimal(12)", "new HBigDecimal(42)" } },
        { "string", new[] { "(string)\"aaa\"", "(string)\"xxx\"" } },
        { "HLocalTime", new[] { "new HLocalTime(1, 2, 3, 0)", "new HLocalTime(4, 5, 6, 0)" } },
        { "HLocalDate", new[] { "new HLocalDate(1, 2, 3)", "new HLocalDate(4, 5, 6)" } },
        { "HLocalDateTime", new[] { "new HLocalDateTime(1, 2, 3, 0, 4, 5, 6)", "new HLocalDateTime(4, 5, 6, 0, 1, 2, 3)" } },
        { "HOffsetDateTime", new[] { "new HOffsetDateTime(new HLocalDateTime(1, 2, 3, 0, 4, 5, 6), 0)", "new HOffsetDateTime(new HLocalDateTime(4, 5, 6, 0, 1, 2, 3), 0)" } },
    };
}