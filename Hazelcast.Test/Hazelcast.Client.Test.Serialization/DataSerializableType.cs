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

using System;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{
    internal class DataSerializableType : IIdentifiedDataSerializable
    {
        private int _raceCount;

        private int _runnerCount;
        private DataSerializableBasicType[,] _runnerInfos;

        public DataSerializableType()
        {
        }

        public DataSerializableType(int runnerCount, int raceCount)
        {
            _runnerCount = runnerCount;
            _raceCount = raceCount;
            _runnerInfos = new DataSerializableBasicType[runnerCount, raceCount];
            for (var i = 0; i < _runnerCount; i++)
            {
                for (var j = 0; j < _raceCount; j++)
                {
                    _runnerInfos[i, j] = new DataSerializableBasicType();
                }
            }
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteInt(_runnerCount);
            output.WriteInt(_raceCount);
            for (var i = 0; i < _runnerCount; i++)
            {
                for (var j = 0; j < _raceCount; j++)
                {
                    _runnerInfos[i, j].WriteData(output);
                }
            }
        }

        public void ReadData(IObjectDataInput input)
        {
            _runnerCount = input.ReadInt();
            _raceCount = input.ReadInt();
            _runnerInfos = new DataSerializableBasicType[_runnerCount, _raceCount];
            for (var i = 0; i < _runnerCount; i++)
            {
                for (var j = 0; j < _raceCount; j++)
                {
                    _runnerInfos[i, j] = new DataSerializableBasicType();
                    _runnerInfos[i, j].ReadData(input);
                }
            }
        }

        public int GetFactoryId()
        {
            return 1;
        }

        public int GetId()
        {
            return 2;
        }
    }

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

        public int GetFactoryId()
        {
            return 1;
        }

        public int GetId()
        {
            return 1;
        }
    }

    internal class MyDataSerializableFactory : IDataSerializableFactory
    {
        public IIdentifiedDataSerializable Create(int typeId)
        {
            switch (typeId)
            {
                case 1:
                    return new DataSerializableBasicType();
                case 2:
                    return new DataSerializableType();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}