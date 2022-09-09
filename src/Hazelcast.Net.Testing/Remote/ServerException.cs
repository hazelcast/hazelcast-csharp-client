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
using Thrift;
using Thrift.Protocol;
using Thrift.Protocol.Entities;
using Thrift.Protocol.Utilities;

#pragma warning disable IDE0079  // remove unnecessary pragmas
#pragma warning disable IDE1006  // parts of the code use IDL spelling
#pragma warning disable IDE0083  // pattern matching "that is not SomeType" requires net5.0 but we still support earlier versions
#pragma warning disable CS0114   // member hides inherited member

namespace Hazelcast.Testing.Remote
{

  public partial class ServerException : TException, TBase
  {
    private string _message;

    public string Message
    {
      get
      {
        return _message;
      }
      set
      {
        __isset.message = true;
        this._message = value;
      }
    }


    public Isset __isset;
    public struct Isset
    {
      public bool message;
    }

    public ServerException()
    {
    }

    public ServerException DeepCopy()
    {
      var tmp15 = new ServerException();
      if((Message != null) && __isset.message)
      {
        tmp15.Message = this.Message;
      }
      tmp15.__isset.message = this.__isset.message;
      return tmp15;
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
                Message = await iprot.ReadStringAsync(cancellationToken);
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
        var tmp16 = new TStruct("ServerException");
        await oprot.WriteStructBeginAsync(tmp16, cancellationToken);
        var tmp17 = new TField();
        if((Message != null) && __isset.message)
        {
          tmp17.Name = "message";
          tmp17.Type = TType.String;
          tmp17.ID = 1;
          await oprot.WriteFieldBeginAsync(tmp17, cancellationToken);
          await oprot.WriteStringAsync(Message, cancellationToken);
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
      if (!(that is ServerException other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return ((__isset.message == other.__isset.message) && ((!__isset.message) || (System.Object.Equals(Message, other.Message))));
    }

    public override int GetHashCode() {
      int hashcode = 157;
      unchecked {
        if((Message != null) && __isset.message)
        {
          hashcode = (hashcode * 397) + Message.GetHashCode();
        }
      }
      return hashcode;
    }

    public override string ToString()
    {
      var tmp18 = new StringBuilder("ServerException(");
      int tmp19 = 0;
      if((Message != null) && __isset.message)
      {
        if(0 < tmp19++) { tmp18.Append(", "); }
        tmp18.Append("Message: ");
        Message.ToString(tmp18);
      }
      tmp18.Append(')');
      return tmp18.ToString();
    }
  }

}
