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

using Hazelcast.Core;
using Hazelcast.Networking;
using Hazelcast.Serialization;

namespace Hazelcast.Models;

internal class ReliableTopicMessage : IIdentifiedDataSerializable
{
    public ReliableTopicMessage(){}
    
    public ReliableTopicMessage(IData payload, NetworkAddress publisherAddress)
    {
        PublishTime = Clock.Milliseconds;
        PublisherAddress = publisherAddress;
        Payload = payload;
    }

    public const int CLASS_ID = 2;

    /// <summary>
    /// Gets publish time of the message.
    /// </summary>
    public long PublishTime { get; private set; }

    /// <summary>
    /// Gets the address of the publisher.
    /// </summary>
    public NetworkAddress PublisherAddress { get; private set; }

    /// <summary>
    /// Gets payload of the message.
    /// </summary>
    public IData Payload { get; private set; }

    /// <inheritdoc />
    public void ReadData(IObjectDataInput input)
    {
        PublishTime = input.ReadLong();
        PublisherAddress = input.ReadObject<NetworkAddress>();
        Payload = new HeapData(input.ReadByteArray());
    }

    /// <inheritdoc />
    public void WriteData(IObjectDataOutput output)
    {
        output.WriteLong(PublishTime);
        output.WriteObject(PublisherAddress);
        output.Write(Payload.ToByteArray());
    }

    /// <inheritdoc />
    public int FactoryId { get; } = -9;

    /// <inheritdoc />
    public int ClassId { get; } = CLASS_ID;

    /// <inheritdoc />
    public override string ToString()
    {
        return "ReliableTopicMessage{"+
               " PublishTime="+PublishTime+
               " PublisherAddress="+PublisherAddress+
               " Payload="+Payload.ToString()+
               "}";
    }
}
