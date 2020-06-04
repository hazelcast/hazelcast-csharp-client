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

  public partial class Cluster : TBase
  {
    private string _id;

    public string Id
    {
      get
      {
        return _id;
      }
      set
      {
        __isset.id = true;
        this._id = value;
      }
    }


    public Isset __isset;
    public struct Isset
    {
      public bool id;
    }

    public Cluster()
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
                Id = await iprot.ReadStringAsync(cancellationToken).CAF();
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
        var struc = new TStruct("Cluster");
        await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
        var field = new TField();
        if (Id != null && __isset.id)
        {
          field.Name = "id";
          field.Type = TType.String;
          field.ID = 1;
          await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
          await oprot.WriteStringAsync(Id, cancellationToken).CAF();
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
      var other = that as Cluster;
      if (other == null) return false;
      if (ReferenceEquals(this, other)) return true;
      return ((__isset.id == other.__isset.id) && ((!__isset.id) || (Equals(Id, other.Id))));
    }

    public override int GetHashCode() {
      int hashcode = 157;
      unchecked {
        if(__isset.id)
          hashcode = (hashcode * 397) + Id.GetHashCode();
      }
      return hashcode;
    }

    public override string ToString()
    {
      var sb = new StringBuilder("Cluster(");
      bool __first = true;
      if (Id != null && __isset.id)
      {
        if(!__first) { sb.Append(", "); }
        __first = false;
        sb.Append("Id: ");
        sb.Append(Id);
      }
      sb.Append(")");
      return sb.ToString();
    }
  }

}
