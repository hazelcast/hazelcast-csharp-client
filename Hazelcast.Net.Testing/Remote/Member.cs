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

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Thrift.Protocol;
using Thrift.Protocol.Entities;
using Thrift.Protocol.Utilities;

#pragma warning disable

namespace Hazelcast.Testing.Remote
{

  public partial class Member : TBase
  {
    private string _uuid;
    private string _host;
    private int _port;

    public string Uuid
    {
      get
      {
        return _uuid;
      }
      set
      {
        __isset.uuid = true;
        this._uuid = value;
      }
    }

    public string Host
    {
      get
      {
        return _host;
      }
      set
      {
        __isset.host = true;
        this._host = value;
      }
    }

    public int Port
    {
      get
      {
        return _port;
      }
      set
      {
        __isset.port = true;
        this._port = value;
      }
    }


    public Isset __isset;
    public struct Isset
    {
      public bool uuid;
      public bool host;
      public bool port;
    }

    public Member()
    {
    }

    public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        TField field;
        await iprot.ReadStructBeginAsync(cancellationToken).CAF();
        while (true)
        {
          field = await iprot.ReadFieldBeginAsync(cancellationToken).CAF();
          if (field.Type == TType.Stop)
          {
            break;
          }

          switch (field.ID)
          {
            case 1:
              if (field.Type == TType.String)
              {
                Uuid = await iprot.ReadStringAsync(cancellationToken).CAF();
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken).CAF();
              }
              break;
            case 2:
              if (field.Type == TType.String)
              {
                Host = await iprot.ReadStringAsync(cancellationToken).CAF();
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken).CAF();
              }
              break;
            case 3:
              if (field.Type == TType.I32)
              {
                Port = await iprot.ReadI32Async(cancellationToken).CAF();
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken).CAF();
              }
              break;
            default:
              await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken).CAF();
              break;
          }

          await iprot.ReadFieldEndAsync(cancellationToken).CAF();
        }

        await iprot.ReadStructEndAsync(cancellationToken).CAF();
      }
      finally
      {
        iprot.DecrementRecursionDepth();
      }
    }

    public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
    {
      oprot.IncrementRecursionDepth();
      try
      {
        var struc = new TStruct("Member");
        await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
        var field = new TField();
        if (Uuid != null && __isset.uuid)
        {
          field.Name = "uuid";
          field.Type = TType.String;
          field.ID = 1;
          await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
          await oprot.WriteStringAsync(Uuid, cancellationToken).CAF();
          await oprot.WriteFieldEndAsync(cancellationToken).CAF();
        }
        if (Host != null && __isset.host)
        {
          field.Name = "host";
          field.Type = TType.String;
          field.ID = 2;
          await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
          await oprot.WriteStringAsync(Host, cancellationToken).CAF();
          await oprot.WriteFieldEndAsync(cancellationToken).CAF();
        }
        if (__isset.port)
        {
          field.Name = "port";
          field.Type = TType.I32;
          field.ID = 3;
          await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
          await oprot.WriteI32Async(Port, cancellationToken).CAF();
          await oprot.WriteFieldEndAsync(cancellationToken).CAF();
        }
        await oprot.WriteFieldStopAsync(cancellationToken).CAF();
        await oprot.WriteStructEndAsync(cancellationToken).CAF();
      }
      finally
      {
        oprot.DecrementRecursionDepth();
      }
    }

    public override bool Equals(object that)
    {
      var other = that as Member;
      if (other == null) return false;
      if (ReferenceEquals(this, other)) return true;
      return ((__isset.uuid == other.__isset.uuid) && ((!__isset.uuid) || (Equals(Uuid, other.Uuid))))
        && ((__isset.host == other.__isset.host) && ((!__isset.host) || (Equals(Host, other.Host))))
        && ((__isset.port == other.__isset.port) && ((!__isset.port) || (Equals(Port, other.Port))));
    }

    public override int GetHashCode() {
      int hashcode = 157;
      unchecked {
        if(__isset.uuid)
          hashcode = (hashcode * 397) + Uuid.GetHashCode();
        if(__isset.host)
          hashcode = (hashcode * 397) + Host.GetHashCode();
        if(__isset.port)
          hashcode = (hashcode * 397) + Port.GetHashCode();
      }
      return hashcode;
    }

    public override string ToString()
    {
      var sb = new StringBuilder("Member(");
      bool __first = true;
      if (Uuid != null && __isset.uuid)
      {
        if(!__first) { sb.Append(", "); }
        __first = false;
        sb.Append("Uuid: ");
        sb.Append(Uuid);
      }
      if (Host != null && __isset.host)
      {
        if(!__first) { sb.Append(", "); }
        __first = false;
        sb.Append("Host: ");
        sb.Append(Host);
      }
      if (__isset.port)
      {
        if(!__first) { sb.Append(", "); }
        __first = false;
        sb.Append("Port: ");
        sb.Append(Port);
      }
      sb.Append(")");
      return sb.ToString();
    }
  }

}
