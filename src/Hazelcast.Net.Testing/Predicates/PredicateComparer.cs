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
using System.Collections;
using System.Collections.Generic;
using Hazelcast.Query;
using Hazelcast.Serialization;

namespace Hazelcast.Testing.Predicates
{
    public class PredicateComparer : IComparer, IComparer<KeyValuePair<object, object>>, IIdentifiedDataSerializable
    {
        public const int ClassIdConst = 2;


        public PredicateComparer(int type = 0, IterationType iterationType = IterationType.Key)
        {
            Type = type;
            IterationType = iterationType;
        }

        public int Type { get; private set; }

        public IterationType IterationType { get; private set; }

        public int Compare(KeyValuePair<object, object> e1, KeyValuePair<object, object> e2)
        {
            string str1;
            string str2;
            switch (IterationType)
            {
                case IterationType.Key:
                    str1 = e1.Key.ToString();
                    str2 = e2.Key.ToString();
                    break;
                case IterationType.Value:
                    str1 = e1.Value.ToString();
                    str2 = e2.Value.ToString();
                    break;
                case IterationType.Entry:
                    str1 = e1.Key + ":::" + e1.Value;
                    str2 = e2.Key + ":::" + e2.Value;
                    break;
                default:
                    str1 = e1.Key.ToString();
                    str2 = e2.Key.ToString();
                    break;
            }

            return Type switch
            {
                0 => string.Compare(str1, str2, StringComparison.CurrentCulture),
                1 => string.Compare(str2, str1, StringComparison.CurrentCulture),
                2 => str1.Length.CompareTo(str2.Length),
                _ => 0
            };
        }

        public void ReadData(IObjectDataInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            Type = input.ReadInt();
            IterationType = (IterationType)input.ReadInt();
        }

        public void WriteData(IObjectDataOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.WriteInt(Type);
            output.WriteInt((int)IterationType);
        }

        public int FactoryId => FactoryIds.PredicateFactoryId;

        public int ClassId => ClassIdConst;

        public int Compare(object x, object y)
        {
            return Compare((KeyValuePair<object, object>)x, (KeyValuePair<object, object>)y);
        }

        /// <summary>
        /// (internal for tests only)
        /// Compares predicates.
        /// </summary>
        internal int Compare((object, object) x, (object, object) y)
        {
            return Compare((object)new KeyValuePair<object, object>(x.Item1, x.Item2), (object)new KeyValuePair<object, object>(y.Item1, y.Item2));
        }
    }
}
