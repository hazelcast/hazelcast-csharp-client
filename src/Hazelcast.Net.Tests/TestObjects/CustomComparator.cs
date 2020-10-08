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

using System.Collections;
using System.Collections.Generic;
using Hazelcast.Predicates;
using Hazelcast.Serialization;

namespace Hazelcast.Tests.TestObjects
{
    public class CustomComparator : IComparer, IComparer<KeyValuePair<object, object>>, IIdentifiedDataSerializable
    {
        public const int ClassIdConst = 2;
        private int _type;
        private IterationType _iterationType;


        public CustomComparator(int type = 0, IterationType iterationType = IterationType.Key)
        {
            _type = type;
            _iterationType = iterationType;
        }

        public int Compare(KeyValuePair<object, object> e1, KeyValuePair<object, object> e2)
        {
            string str1;
            string str2;
            switch (_iterationType)
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
                    str1 = e1.Key.ToString() + e1.Value.ToString();
                    str2 = e2.Key.ToString() + e2.Value.ToString();
                    break;
                default:
                    str1 = e1.Key.ToString();
                    str2 = e2.Key.ToString();
                    break;
            }
            switch (_type)
            {
                case 0:
                    return str1.CompareTo(str2);
                case 1:
                    return str2.CompareTo(str1);
                case 2:
                    return str1.Length.CompareTo(str2.Length);
            }
            return 0;
        }

        public void ReadData(IObjectDataInput input)
        {
            _type = input.ReadInt();
            _iterationType = (IterationType)input.ReadInt();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.Write(_type);
            output.Write((int)_iterationType);
        }

        public int FactoryId => IdentifiedFactory.FactoryId;

        public int ClassId => ClassIdConst;

        public int Compare(object x, object y)
        {
            return Compare((KeyValuePair<object, object>)x, (KeyValuePair<object, object>)y);
        }
    }
}