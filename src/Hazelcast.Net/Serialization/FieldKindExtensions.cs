// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

#nullable enable

using System;

namespace Hazelcast.Serialization
{
    internal static class FieldKindExtensions
    {
        public static int GetValueTypeSize(this FieldKind kind)
        {
            switch (kind)
            {
                case FieldKind.Int8:
                    return 1;
                case FieldKind.Int16:
                    return 2;
                case FieldKind.Int32:
                case FieldKind.Float32:
                    return 4;
                case FieldKind.Int64:
                case FieldKind.Float64:
                    return 8;
                default:
                    throw new NotSupportedException($"Cannot get the size of {kind} kind.");
            }
        }

        public static bool IsValueType(this FieldKind kind)
            => !kind.IsReferenceType();

        public static bool IsReferenceType(this FieldKind kind)
        {
            switch (kind)
            {
                case FieldKind.Boolean:
                case FieldKind.Int8:
                case FieldKind.Int16:
                case FieldKind.Int32:
                case FieldKind.Int64:
                case FieldKind.Float32:
                case FieldKind.Float64:
                    return false;
                default:
                    return true;
            }
        }
    }
}
