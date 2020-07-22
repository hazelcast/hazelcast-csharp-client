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

#if NETSTANDARD
#pragma warning disable

//Autogenerated by Thrift Compiler (0.13.0)

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Thrift.Collections;
using Thrift.Protocol;
using Thrift.Protocol.Entities;
using Thrift.Protocol.Utilities;

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
              if (field.Type == TType.Bool)
              {
                Success = await iprot.ReadBoolAsync(cancellationToken).CAF();
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken).CAF();
              }
              break;
            case 2:
              if (field.Type == TType.String)
              {
                Message = await iprot.ReadStringAsync(cancellationToken).CAF();
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken).CAF();
              }
              break;
            case 3:
              if (field.Type == TType.String)
              {
                Result = await iprot.ReadBinaryAsync(cancellationToken).CAF();
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
        var struc = new TStruct("Response");
        await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
        var field = new TField();
        if (__isset.success)
        {
          field.Name = "success";
          field.Type = TType.Bool;
          field.ID = 1;
          await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
          await oprot.WriteBoolAsync(Success, cancellationToken).CAF();
          await oprot.WriteFieldEndAsync(cancellationToken).CAF();
        }
        if (Message != null && __isset.message)
        {
          field.Name = "message";
          field.Type = TType.String;
          field.ID = 2;
          await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
          await oprot.WriteStringAsync(Message, cancellationToken).CAF();
          await oprot.WriteFieldEndAsync(cancellationToken).CAF();
        }
        if (Result != null && __isset.result)
        {
          field.Name = "result";
          field.Type = TType.String;
          field.ID = 3;
          await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
          await oprot.WriteBinaryAsync(Result, cancellationToken).CAF();
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
      var other = that as Response;
      if (other == null) return false;
      if (ReferenceEquals(this, other)) return true;
      return ((__isset.success == other.__isset.success) && ((!__isset.success) || (Equals(Success, other.Success))))
        && ((__isset.message == other.__isset.message) && ((!__isset.message) || (Equals(Message, other.Message))))
        && ((__isset.result == other.__isset.result) && ((!__isset.result) || (TCollections.Equals(Result, other.Result))));
    }

    public override int GetHashCode() {
      int hashcode = 157;
      unchecked {
        if(__isset.success)
          hashcode = (hashcode * 397) + Success.GetHashCode();
        if(__isset.message)
          hashcode = (hashcode * 397) + Message.GetHashCode();
        if(__isset.result)
          hashcode = (hashcode * 397) + Result.GetHashCode();
      }
      return hashcode;
    }

    public override string ToString()
    {
      var sb = new StringBuilder("Response(");
      bool __first = true;
      if (__isset.success)
      {
        if(!__first) { sb.Append(", "); }
        __first = false;
        sb.Append("Success: ");
        sb.Append(Success);
      }
      if (Message != null && __isset.message)
      {
        if(!__first) { sb.Append(", "); }
        __first = false;
        sb.Append("Message: ");
        sb.Append(Message);
      }
      if (Result != null && __isset.result)
      {
        if(!__first) { sb.Append(", "); }
        __first = false;
        sb.Append("Result: ");
        sb.Append(Result);
      }
      sb.Append(")");
      return sb.ToString();
    }
  }

}

#endif
