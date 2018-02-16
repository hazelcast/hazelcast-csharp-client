// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using System.Text;

#pragma warning disable CS1591
namespace Hazelcast.Client.Spi
{
    [Serializable]
    internal class ObjectNamespace
    {
        private readonly string _objectName;
        private readonly string _service;

        public ObjectNamespace()
        {
        }

        public ObjectNamespace(string serviceName, string objectName)
        {
            _service = serviceName;
            _objectName = objectName;
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }
            var that = (ObjectNamespace) o;
            if (_objectName != null ? !_objectName.Equals(that._objectName) : that._objectName != null)
            {
                return false;
            }
            if (_service != null ? !_service.Equals(that._service) : that._service != null)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            var result = _service != null ? _service.GetHashCode() : 0;
            result = 31*result + (_objectName != null ? _objectName.GetHashCode() : 0);
            return result;
        }

        public virtual string GetObjectName()
        {
            return _objectName;
        }

        ///// <exception cref="System.IO.IOException"></exception>
        //public virtual void WriteData(IObjectDataOutput output)
        //{
        //    output.WriteUTF(service);
        //    output.WriteObject(objectName);
        //}

        //// writing as object for backward-compatibility
        ///// <exception cref="System.IO.IOException"></exception>
        //public virtual void ReadData(IObjectDataInput input)
        //{
        //    service = input.ReadUTF();
        //    objectName = input.ReadObject<string>();
        //}

        public virtual string GetServiceName()
        {
            return _service;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ObjectNamespace");
            sb.Append("{service='").Append(_service).Append('\'');
            sb.Append(", objectName=").Append(_objectName);
            sb.Append('}');
            return sb.ToString();
        }
    }
}