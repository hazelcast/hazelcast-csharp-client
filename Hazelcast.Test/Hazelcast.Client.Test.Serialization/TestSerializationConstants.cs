// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Client.Test.Serialization
{
    internal static class TestSerializationConstants
    {
        public const int PORTABLE_FACTORY_ID = 1;
        public const int INNER_PORTABLE = 1;
        public const int INVALID_RAW_DATA_PORTABLE = 2;
        public const int INVALID_RAW_DATA_PORTABLE_2 = 3;
        public const int RAW_DATA_PORTABLE = 4;
        public const int MAIN_PORTABLE = 5;
        public const int NAMED_PORTABLE = 6;
        public const int OBJECT_CARRYING_PORTABLE = 7;
        public const int MORPHING_PORTABLE_ID = 9;

        public const int DATA_SERIALIZABLE_FACTORY_ID = 1;
        public const int SAMPLE_IDENTIFIED_DATA_SERIALIZABLE = 0;
        public const int BYTE_ARRAY_DATA_SERIALIZABLE_ID = 1;
        public const int DATA_DATA_SERIALIZABLE_ID = 2;
        public const int COMPLEX_DATA_SERIALIZABLE_ID = 3;

    }
}