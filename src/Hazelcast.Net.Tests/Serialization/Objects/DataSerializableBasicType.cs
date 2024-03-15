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

using Hazelcast.Serialization;

namespace Hazelcast.Tests.Serialization.Objects
{
    internal class DataSerializableBasicType : IIdentifiedDataSerializable
    {
        private double dblValue1 = 10.1;
        private double dblValue2 = 21.34;

        public double DblValue1
        {
            get { return dblValue1; }
            set { dblValue1 = value; }
        }

        public double DblValue2
        {
            get { return dblValue2; }
            set { dblValue2 = value; }
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteDouble(dblValue1);
            output.WriteDouble(dblValue2);
        }

        public void ReadData(IObjectDataInput input)
        {
            dblValue1 = input.ReadDouble();
            dblValue2 = input.ReadDouble();
        }

        public int FactoryId => 1;

        public int ClassId => 1;
    }
}
