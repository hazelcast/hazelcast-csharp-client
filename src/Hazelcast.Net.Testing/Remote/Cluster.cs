// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Thrift.Protocol;
using Thrift.Protocol.Entities;
using Thrift.Protocol.Utilities;

#pragma warning disable IDE0079  // remove unnecessary pragmas
#pragma warning disable IDE1006  // parts of the code use IDL spelling
#pragma warning disable IDE0083  // pattern matching "that is not SomeType" requires net5.0 but we still support earlier versions

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

    public Cluster DeepCopy()
    {
      var tmp0 = new Cluster();
      if((Id != null) && __isset.id)
      {
        tmp0.Id = this.Id;
      }
      tmp0.__isset.id = this.__isset.id;
      return tmp0;
    }

    public async global::System.Threading.Tasks.Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        TField field;
        await iprot.ReadStructBeginAsync(cancellationToken);
        while (true)
        {
          field = await iprot.ReadFieldBeginAsync(cancellationToken);
          if (field.Type == TType.Stop)
          {
            break;
          }

          switch (field.ID)
          {
            case 1:
              if (field.Type == TType.String)
              {
                Id = await iprot.ReadStringAsync(cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            default:
              await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              break;
          }

          await iprot.ReadFieldEndAsync(cancellationToken);
        }

        await iprot.ReadStructEndAsync(cancellationToken);
      }
      finally
      {
        iprot.DecrementRecursionDepth();
      }
    }

    public async global::System.Threading.Tasks.Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
    {
      oprot.IncrementRecursionDepth();
      try
      {
        var tmp1 = new TStruct("Cluster");
        await oprot.WriteStructBeginAsync(tmp1, cancellationToken);
        var tmp2 = new TField();
        if((Id != null) && __isset.id)
        {
          tmp2.Name = "id";
          tmp2.Type = TType.String;
          tmp2.ID = 1;
          await oprot.WriteFieldBeginAsync(tmp2, cancellationToken);
          await oprot.WriteStringAsync(Id, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        await oprot.WriteFieldStopAsync(cancellationToken);
        await oprot.WriteStructEndAsync(cancellationToken);
      }
      finally
      {
        oprot.DecrementRecursionDepth();
      }
    }

    public override bool Equals(object that)
    {
      if (!(that is Cluster other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return ((__isset.id == other.__isset.id) && ((!__isset.id) || (System.Object.Equals(Id, other.Id))));
    }

    public override int GetHashCode() {
      int hashcode = 157;
      unchecked {
        if((Id != null) && __isset.id)
        {
          hashcode = (hashcode * 397) + Id.GetHashCode();
        }
      }
      return hashcode;
    }

    public override string ToString()
    {
      var tmp3 = new StringBuilder("Cluster(");
      int tmp4 = 0;
      if((Id != null) && __isset.id)
      {
        if(0 < tmp4++) { tmp3.Append(", "); }
        tmp3.Append("Id: ");
        Id.ToString(tmp3);
      }
      tmp3.Append(')');
      return tmp3.ToString();
    }
  }

}
