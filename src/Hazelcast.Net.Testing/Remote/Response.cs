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

using System.Linq;
using System.Text;
using System.Threading;
using Thrift.Collections;
using Thrift.Protocol;
using Thrift.Protocol.Entities;
using Thrift.Protocol.Utilities;

#pragma warning disable IDE0079  // remove unnecessary pragmas
#pragma warning disable IDE1006  // parts of the code use IDL spelling
#pragma warning disable IDE0083  // pattern matching "that is not SomeType" requires net5.0 but we still support earlier versions

namespace Hazelcast.Testing.Remote
{

  public partial class Response : TBase
  {
    private bool _success;
    private string _message;
    private byte[] _result;

    public bool Success
    {
      get
      {
        return _success;
      }
      set
      {
        __isset.success = true;
        this._success = value;
      }
    }

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

    public byte[] Result
    {
      get
      {
        return _result;
      }
      set
      {
        __isset.result = true;
        this._result = value;
      }
    }


    public Isset __isset;
    public struct Isset
    {
      public bool success;
      public bool message;
      public bool result;
    }

    public Response()
    {
    }

    public Response DeepCopy()
    {
      var tmp10 = new Response();
      if(__isset.success)
      {
        tmp10.Success = this.Success;
      }
      tmp10.__isset.success = this.__isset.success;
      if((Message != null) && __isset.message)
      {
        tmp10.Message = this.Message;
      }
      tmp10.__isset.message = this.__isset.message;
      if((Result != null) && __isset.result)
      {
        tmp10.Result = this.Result.ToArray();
      }
      tmp10.__isset.result = this.__isset.result;
      return tmp10;
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
              if (field.Type == TType.Bool)
              {
                Success = await iprot.ReadBoolAsync(cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 2:
              if (field.Type == TType.String)
              {
                Message = await iprot.ReadStringAsync(cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 3:
              if (field.Type == TType.String)
              {
                Result = await iprot.ReadBinaryAsync(cancellationToken);
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
        var tmp11 = new TStruct("Response");
        await oprot.WriteStructBeginAsync(tmp11, cancellationToken);
        var tmp12 = new TField();
        if(__isset.success)
        {
          tmp12.Name = "success";
          tmp12.Type = TType.Bool;
          tmp12.ID = 1;
          await oprot.WriteFieldBeginAsync(tmp12, cancellationToken);
          await oprot.WriteBoolAsync(Success, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if((Message != null) && __isset.message)
        {
          tmp12.Name = "message";
          tmp12.Type = TType.String;
          tmp12.ID = 2;
          await oprot.WriteFieldBeginAsync(tmp12, cancellationToken);
          await oprot.WriteStringAsync(Message, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if((Result != null) && __isset.result)
        {
          tmp12.Name = "result";
          tmp12.Type = TType.String;
          tmp12.ID = 3;
          await oprot.WriteFieldBeginAsync(tmp12, cancellationToken);
          await oprot.WriteBinaryAsync(Result, cancellationToken);
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
      if (!(that is Response other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))))
        && ((__isset.message == other.__isset.message) && ((!__isset.message) || (System.Object.Equals(Message, other.Message))))
        && ((__isset.result == other.__isset.result) && ((!__isset.result) || (TCollections.Equals(Result, other.Result))));
    }

    public override int GetHashCode() {
      int hashcode = 157;
      unchecked {
        if(__isset.success)
        {
          hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        if((Message != null) && __isset.message)
        {
          hashcode = (hashcode * 397) + Message.GetHashCode();
        }
        if((Result != null) && __isset.result)
        {
          hashcode = (hashcode * 397) + Result.GetHashCode();
        }
      }
      return hashcode;
    }

    public override string ToString()
    {
      var tmp13 = new StringBuilder("Response(");
      int tmp14 = 0;
      if(__isset.success)
      {
        if(0 < tmp14++) { tmp13.Append(", "); }
        tmp13.Append("Success: ");
        Success.ToString(tmp13);
      }
      if((Message != null) && __isset.message)
      {
        if(0 < tmp14++) { tmp13.Append(", "); }
        tmp13.Append("Message: ");
        Message.ToString(tmp13);
      }
      if((Result != null) && __isset.result)
      {
        if(0 < tmp14++) { tmp13.Append(", "); }
        tmp13.Append("Result: ");
        Result.ToString(tmp13);
      }
      tmp13.Append(')');
      return tmp13.ToString();
    }
  }

}
