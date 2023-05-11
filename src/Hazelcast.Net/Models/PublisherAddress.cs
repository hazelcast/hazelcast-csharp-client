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

using System.Net;
using Hazelcast.Networking;
using Hazelcast.Serialization;

namespace Hazelcast.Models;

public class PublisherAddress : IIdentifiedDataSerializable
{
    private byte _addressType;
    public IPEndPoint IpEndPoint { get; internal set; }
    public bool IsIpV4 => _addressType == 4 ? true : false;
    public bool IsIpV6 => _addressType == 6 ? true : false;

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        var port = input.ReadInt();
        _addressType = input.ReadByte();
        var host = input.ReadString();
        IpEndPoint = new IPEndPoint(NetworkAddress.GetIPAddressByName(host), port);
    }

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteInt(IpEndPoint.Port);
        output.WriteByte((byte) (IsIpV4 ? 4 : 6));
        output.WriteString(IpEndPoint.Address.ToString());
    }

    /// <inheritdoc />
    public int FactoryId => 0;

    /// <inheritdoc />
    public int ClassId => 1;
}
