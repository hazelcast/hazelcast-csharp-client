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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Thrift;
using Thrift.Processor;
using Thrift.Protocol;
using Thrift.Protocol.Entities;
using Thrift.Protocol.Utilities;
using Thrift.Transport;

#pragma warning disable

namespace Hazelcast.Testing.Remote
{
  public partial class RemoteController
  {
    public interface IAsync
    {
      Task<bool> pingAsync(CancellationToken cancellationToken = default(CancellationToken));

      Task<bool> cleanAsync(CancellationToken cancellationToken = default(CancellationToken));

      Task<bool> exitAsync(CancellationToken cancellationToken = default(CancellationToken));

      Task<Cluster> createClusterAsync(string hzVersion, string xmlconfig, CancellationToken cancellationToken = default(CancellationToken));

      Task<Member> startMemberAsync(string clusterId, CancellationToken cancellationToken = default(CancellationToken));

      Task<bool> shutdownMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken));

      Task<bool> terminateMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken));

      Task<bool> suspendMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken));

      Task<bool> resumeMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken));

      Task<bool> shutdownClusterAsync(string clusterId, CancellationToken cancellationToken = default(CancellationToken));

      Task<bool> terminateClusterAsync(string clusterId, CancellationToken cancellationToken = default(CancellationToken));

      Task<Cluster> splitMemberFromClusterAsync(string memberId, CancellationToken cancellationToken = default(CancellationToken));

      Task<Cluster> mergeMemberToClusterAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken));

      Task<Response> executeOnControllerAsync(string clusterId, string script, Lang lang, CancellationToken cancellationToken = default(CancellationToken));

    }


    public class Client : TBaseClient, IDisposable, IAsync
    {
      public Client(TProtocol protocol) : this(protocol, protocol)
      {
      }

      public Client(TProtocol inputProtocol, TProtocol outputProtocol) : base(inputProtocol, outputProtocol)      {
      }
      public async Task<bool> pingAsync(CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("ping", TMessageType.Call, SeqId), cancellationToken).CAF();

        var args = new pingArgs();

        await args.WriteAsync(OutputProtocol, cancellationToken).CAF();
        await OutputProtocol.WriteMessageEndAsync(cancellationToken).CAF();
        await OutputProtocol.Transport.FlushAsync(cancellationToken).CAF();

        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken).CAF();
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken).CAF();
          await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
          throw x;
        }

        var result = new pingResult();
        await result.ReadAsync(InputProtocol, cancellationToken).CAF();
        await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "ping failed: unknown result");
      }

      public async Task<bool> cleanAsync(CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("clean", TMessageType.Call, SeqId), cancellationToken).CAF();

        var args = new cleanArgs();

        await args.WriteAsync(OutputProtocol, cancellationToken).CAF();
        await OutputProtocol.WriteMessageEndAsync(cancellationToken).CAF();
        await OutputProtocol.Transport.FlushAsync(cancellationToken).CAF();

        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken).CAF();
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken).CAF();
          await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
          throw x;
        }

        var result = new cleanResult();
        await result.ReadAsync(InputProtocol, cancellationToken).CAF();
        await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "clean failed: unknown result");
      }

      public async Task<bool> exitAsync(CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("exit", TMessageType.Call, SeqId), cancellationToken).CAF();

        var args = new exitArgs();

        await args.WriteAsync(OutputProtocol, cancellationToken).CAF();
        await OutputProtocol.WriteMessageEndAsync(cancellationToken).CAF();
        await OutputProtocol.Transport.FlushAsync(cancellationToken).CAF();

        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken).CAF();
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken).CAF();
          await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
          throw x;
        }

        var result = new exitResult();
        await result.ReadAsync(InputProtocol, cancellationToken).CAF();
        await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "exit failed: unknown result");
      }

      public async Task<Cluster> createClusterAsync(string hzVersion, string xmlconfig, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("createCluster", TMessageType.Call, SeqId), cancellationToken).CAF();

        var args = new createClusterArgs();
        args.HzVersion = hzVersion;
        args.Xmlconfig = xmlconfig;

        await args.WriteAsync(OutputProtocol, cancellationToken).CAF();
        await OutputProtocol.WriteMessageEndAsync(cancellationToken).CAF();
        await OutputProtocol.Transport.FlushAsync(cancellationToken).CAF();

        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken).CAF();
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken).CAF();
          await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
          throw x;
        }

        var result = new createClusterResult();
        await result.ReadAsync(InputProtocol, cancellationToken).CAF();
        await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
        if (result.__isset.success)
        {
          return result.Success;
        }
        if (result.__isset.serverException)
        {
          throw result.ServerException;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "createCluster failed: unknown result");
      }

      public async Task<Member> startMemberAsync(string clusterId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("startMember", TMessageType.Call, SeqId), cancellationToken).CAF();

        var args = new startMemberArgs();
        args.ClusterId = clusterId;

        await args.WriteAsync(OutputProtocol, cancellationToken).CAF();
        await OutputProtocol.WriteMessageEndAsync(cancellationToken).CAF();
        await OutputProtocol.Transport.FlushAsync(cancellationToken).CAF();

        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken).CAF();
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken).CAF();
          await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
          throw x;
        }

        var result = new startMemberResult();
        await result.ReadAsync(InputProtocol, cancellationToken).CAF();
        await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
        if (result.__isset.success)
        {
          return result.Success;
        }
        if (result.__isset.serverException)
        {
          throw result.ServerException;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "startMember failed: unknown result");
      }

      public async Task<bool> shutdownMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("shutdownMember", TMessageType.Call, SeqId), cancellationToken).CAF();

        var args = new shutdownMemberArgs();
        args.ClusterId = clusterId;
        args.MemberId = memberId;

        await args.WriteAsync(OutputProtocol, cancellationToken).CAF();
        await OutputProtocol.WriteMessageEndAsync(cancellationToken).CAF();
        await OutputProtocol.Transport.FlushAsync(cancellationToken).CAF();

        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken).CAF();
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken).CAF();
          await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
          throw x;
        }

        var result = new shutdownMemberResult();
        await result.ReadAsync(InputProtocol, cancellationToken).CAF();
        await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "shutdownMember failed: unknown result");
      }

      public async Task<bool> terminateMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("terminateMember", TMessageType.Call, SeqId), cancellationToken).CAF();

        var args = new terminateMemberArgs();
        args.ClusterId = clusterId;
        args.MemberId = memberId;

        await args.WriteAsync(OutputProtocol, cancellationToken).CAF();
        await OutputProtocol.WriteMessageEndAsync(cancellationToken).CAF();
        await OutputProtocol.Transport.FlushAsync(cancellationToken).CAF();

        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken).CAF();
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken).CAF();
          await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
          throw x;
        }

        var result = new terminateMemberResult();
        await result.ReadAsync(InputProtocol, cancellationToken).CAF();
        await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "terminateMember failed: unknown result");
      }

      public async Task<bool> suspendMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("suspendMember", TMessageType.Call, SeqId), cancellationToken).CAF();

        var args = new suspendMemberArgs();
        args.ClusterId = clusterId;
        args.MemberId = memberId;

        await args.WriteAsync(OutputProtocol, cancellationToken).CAF();
        await OutputProtocol.WriteMessageEndAsync(cancellationToken).CAF();
        await OutputProtocol.Transport.FlushAsync(cancellationToken).CAF();

        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken).CAF();
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken).CAF();
          await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
          throw x;
        }

        var result = new suspendMemberResult();
        await result.ReadAsync(InputProtocol, cancellationToken).CAF();
        await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "suspendMember failed: unknown result");
      }

      public async Task<bool> resumeMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("resumeMember", TMessageType.Call, SeqId), cancellationToken).CAF();

        var args = new resumeMemberArgs();
        args.ClusterId = clusterId;
        args.MemberId = memberId;

        await args.WriteAsync(OutputProtocol, cancellationToken).CAF();
        await OutputProtocol.WriteMessageEndAsync(cancellationToken).CAF();
        await OutputProtocol.Transport.FlushAsync(cancellationToken).CAF();

        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken).CAF();
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken).CAF();
          await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
          throw x;
        }

        var result = new resumeMemberResult();
        await result.ReadAsync(InputProtocol, cancellationToken).CAF();
        await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "resumeMember failed: unknown result");
      }

      public async Task<bool> shutdownClusterAsync(string clusterId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("shutdownCluster", TMessageType.Call, SeqId), cancellationToken).CAF();

        var args = new shutdownClusterArgs();
        args.ClusterId = clusterId;

        await args.WriteAsync(OutputProtocol, cancellationToken).CAF();
        await OutputProtocol.WriteMessageEndAsync(cancellationToken).CAF();
        await OutputProtocol.Transport.FlushAsync(cancellationToken).CAF();

        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken).CAF();
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken).CAF();
          await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
          throw x;
        }

        var result = new shutdownClusterResult();
        await result.ReadAsync(InputProtocol, cancellationToken).CAF();
        await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "shutdownCluster failed: unknown result");
      }

      public async Task<bool> terminateClusterAsync(string clusterId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("terminateCluster", TMessageType.Call, SeqId), cancellationToken).CAF();

        var args = new terminateClusterArgs();
        args.ClusterId = clusterId;

        await args.WriteAsync(OutputProtocol, cancellationToken).CAF();
        await OutputProtocol.WriteMessageEndAsync(cancellationToken).CAF();
        await OutputProtocol.Transport.FlushAsync(cancellationToken).CAF();

        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken).CAF();
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken).CAF();
          await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
          throw x;
        }

        var result = new terminateClusterResult();
        await result.ReadAsync(InputProtocol, cancellationToken).CAF();
        await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "terminateCluster failed: unknown result");
      }

      public async Task<Cluster> splitMemberFromClusterAsync(string memberId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("splitMemberFromCluster", TMessageType.Call, SeqId), cancellationToken).CAF();

        var args = new splitMemberFromClusterArgs();
        args.MemberId = memberId;

        await args.WriteAsync(OutputProtocol, cancellationToken).CAF();
        await OutputProtocol.WriteMessageEndAsync(cancellationToken).CAF();
        await OutputProtocol.Transport.FlushAsync(cancellationToken).CAF();

        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken).CAF();
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken).CAF();
          await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
          throw x;
        }

        var result = new splitMemberFromClusterResult();
        await result.ReadAsync(InputProtocol, cancellationToken).CAF();
        await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "splitMemberFromCluster failed: unknown result");
      }

      public async Task<Cluster> mergeMemberToClusterAsync(string clusterId, string memberId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("mergeMemberToCluster", TMessageType.Call, SeqId), cancellationToken).CAF();

        var args = new mergeMemberToClusterArgs();
        args.ClusterId = clusterId;
        args.MemberId = memberId;

        await args.WriteAsync(OutputProtocol, cancellationToken).CAF();
        await OutputProtocol.WriteMessageEndAsync(cancellationToken).CAF();
        await OutputProtocol.Transport.FlushAsync(cancellationToken).CAF();

        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken).CAF();
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken).CAF();
          await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
          throw x;
        }

        var result = new mergeMemberToClusterResult();
        await result.ReadAsync(InputProtocol, cancellationToken).CAF();
        await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "mergeMemberToCluster failed: unknown result");
      }

      public async Task<Response> executeOnControllerAsync(string clusterId, string script, Lang lang, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("executeOnController", TMessageType.Call, SeqId), cancellationToken).CAF();

        var args = new executeOnControllerArgs();
        args.ClusterId = clusterId;
        args.Script = script;
        args.Lang = lang;

        await args.WriteAsync(OutputProtocol, cancellationToken).CAF();
        await OutputProtocol.WriteMessageEndAsync(cancellationToken).CAF();
        await OutputProtocol.Transport.FlushAsync(cancellationToken).CAF();

        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken).CAF();
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken).CAF();
          await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
          throw x;
        }

        var result = new executeOnControllerResult();
        await result.ReadAsync(InputProtocol, cancellationToken).CAF();
        await InputProtocol.ReadMessageEndAsync(cancellationToken).CAF();
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "executeOnController failed: unknown result");
      }

    }

    public class AsyncProcessor : ITAsyncProcessor
    {
      private IAsync _iAsync;

      public AsyncProcessor(IAsync iAsync)
      {
        if (iAsync == null) throw new ArgumentNullException(nameof(iAsync));

        _iAsync = iAsync;
        processMap_["ping"] = ping_ProcessAsync;
        processMap_["clean"] = clean_ProcessAsync;
        processMap_["exit"] = exit_ProcessAsync;
        processMap_["createCluster"] = createCluster_ProcessAsync;
        processMap_["startMember"] = startMember_ProcessAsync;
        processMap_["shutdownMember"] = shutdownMember_ProcessAsync;
        processMap_["terminateMember"] = terminateMember_ProcessAsync;
        processMap_["suspendMember"] = suspendMember_ProcessAsync;
        processMap_["resumeMember"] = resumeMember_ProcessAsync;
        processMap_["shutdownCluster"] = shutdownCluster_ProcessAsync;
        processMap_["terminateCluster"] = terminateCluster_ProcessAsync;
        processMap_["splitMemberFromCluster"] = splitMemberFromCluster_ProcessAsync;
        processMap_["mergeMemberToCluster"] = mergeMemberToCluster_ProcessAsync;
        processMap_["executeOnController"] = executeOnController_ProcessAsync;
      }

      protected delegate Task ProcessFunction(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken);
      protected Dictionary<string, ProcessFunction> processMap_ = new Dictionary<string, ProcessFunction>();

      public async Task<bool> ProcessAsync(TProtocol iprot, TProtocol oprot)
      {
        return await ProcessAsync(iprot, oprot, CancellationToken.None).CAF();
      }

      public async Task<bool> ProcessAsync(TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        try
        {
          var msg = await iprot.ReadMessageBeginAsync(cancellationToken).CAF();

          ProcessFunction fn;
          processMap_.TryGetValue(msg.Name, out fn);

          if (fn == null)
          {
            await TProtocolUtil.SkipAsync(iprot, TType.Struct, cancellationToken).CAF();
            await iprot.ReadMessageEndAsync(cancellationToken).CAF();
            var x = new TApplicationException (TApplicationException.ExceptionType.UnknownMethod, "Invalid method name: '" + msg.Name + "'");
            await oprot.WriteMessageBeginAsync(new TMessage(msg.Name, TMessageType.Exception, msg.SeqID), cancellationToken).CAF();
            await x.WriteAsync(oprot, cancellationToken).CAF();
            await oprot.WriteMessageEndAsync(cancellationToken).CAF();
            await oprot.Transport.FlushAsync(cancellationToken).CAF();
            return true;
          }

          await fn(msg.SeqID, iprot, oprot, cancellationToken).CAF();

        }
        catch (IOException)
        {
          return false;
        }

        return true;
      }

      public async Task ping_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new pingArgs();
        await args.ReadAsync(iprot, cancellationToken).CAF();
        await iprot.ReadMessageEndAsync(cancellationToken).CAF();
        var result = new pingResult();
        try
        {
          result.Success = await _iAsync.pingAsync(cancellationToken).CAF();
          await oprot.WriteMessageBeginAsync(new TMessage("ping", TMessageType.Reply, seqid), cancellationToken).CAF();
          await result.WriteAsync(oprot, cancellationToken).CAF();
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("ping", TMessageType.Exception, seqid), cancellationToken).CAF();
          await x.WriteAsync(oprot, cancellationToken).CAF();
        }
        await oprot.WriteMessageEndAsync(cancellationToken).CAF();
        await oprot.Transport.FlushAsync(cancellationToken).CAF();
      }

      public async Task clean_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new cleanArgs();
        await args.ReadAsync(iprot, cancellationToken).CAF();
        await iprot.ReadMessageEndAsync(cancellationToken).CAF();
        var result = new cleanResult();
        try
        {
          result.Success = await _iAsync.cleanAsync(cancellationToken).CAF();
          await oprot.WriteMessageBeginAsync(new TMessage("clean", TMessageType.Reply, seqid), cancellationToken).CAF();
          await result.WriteAsync(oprot, cancellationToken).CAF();
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("clean", TMessageType.Exception, seqid), cancellationToken).CAF();
          await x.WriteAsync(oprot, cancellationToken).CAF();
        }
        await oprot.WriteMessageEndAsync(cancellationToken).CAF();
        await oprot.Transport.FlushAsync(cancellationToken).CAF();
      }

      public async Task exit_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new exitArgs();
        await args.ReadAsync(iprot, cancellationToken).CAF();
        await iprot.ReadMessageEndAsync(cancellationToken).CAF();
        var result = new exitResult();
        try
        {
          result.Success = await _iAsync.exitAsync(cancellationToken).CAF();
          await oprot.WriteMessageBeginAsync(new TMessage("exit", TMessageType.Reply, seqid), cancellationToken).CAF();
          await result.WriteAsync(oprot, cancellationToken).CAF();
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("exit", TMessageType.Exception, seqid), cancellationToken).CAF();
          await x.WriteAsync(oprot, cancellationToken).CAF();
        }
        await oprot.WriteMessageEndAsync(cancellationToken).CAF();
        await oprot.Transport.FlushAsync(cancellationToken).CAF();
      }

      public async Task createCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new createClusterArgs();
        await args.ReadAsync(iprot, cancellationToken).CAF();
        await iprot.ReadMessageEndAsync(cancellationToken).CAF();
        var result = new createClusterResult();
        try
        {
          try
          {
            result.Success = await _iAsync.createClusterAsync(args.HzVersion, args.Xmlconfig, cancellationToken).CAF();
          }
          catch (ServerException serverException)
          {
            result.ServerException = serverException;
          }
          await oprot.WriteMessageBeginAsync(new TMessage("createCluster", TMessageType.Reply, seqid), cancellationToken).CAF();
          await result.WriteAsync(oprot, cancellationToken).CAF();
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("createCluster", TMessageType.Exception, seqid), cancellationToken).CAF();
          await x.WriteAsync(oprot, cancellationToken).CAF();
        }
        await oprot.WriteMessageEndAsync(cancellationToken).CAF();
        await oprot.Transport.FlushAsync(cancellationToken).CAF();
      }

      public async Task startMember_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new startMemberArgs();
        await args.ReadAsync(iprot, cancellationToken).CAF();
        await iprot.ReadMessageEndAsync(cancellationToken).CAF();
        var result = new startMemberResult();
        try
        {
          try
          {
            result.Success = await _iAsync.startMemberAsync(args.ClusterId, cancellationToken).CAF();
          }
          catch (ServerException serverException)
          {
            result.ServerException = serverException;
          }
          await oprot.WriteMessageBeginAsync(new TMessage("startMember", TMessageType.Reply, seqid), cancellationToken).CAF();
          await result.WriteAsync(oprot, cancellationToken).CAF();
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("startMember", TMessageType.Exception, seqid), cancellationToken).CAF();
          await x.WriteAsync(oprot, cancellationToken).CAF();
        }
        await oprot.WriteMessageEndAsync(cancellationToken).CAF();
        await oprot.Transport.FlushAsync(cancellationToken).CAF();
      }

      public async Task shutdownMember_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new shutdownMemberArgs();
        await args.ReadAsync(iprot, cancellationToken).CAF();
        await iprot.ReadMessageEndAsync(cancellationToken).CAF();
        var result = new shutdownMemberResult();
        try
        {
          result.Success = await _iAsync.shutdownMemberAsync(args.ClusterId, args.MemberId, cancellationToken).CAF();
          await oprot.WriteMessageBeginAsync(new TMessage("shutdownMember", TMessageType.Reply, seqid), cancellationToken).CAF();
          await result.WriteAsync(oprot, cancellationToken).CAF();
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("shutdownMember", TMessageType.Exception, seqid), cancellationToken).CAF();
          await x.WriteAsync(oprot, cancellationToken).CAF();
        }
        await oprot.WriteMessageEndAsync(cancellationToken).CAF();
        await oprot.Transport.FlushAsync(cancellationToken).CAF();
      }

      public async Task terminateMember_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new terminateMemberArgs();
        await args.ReadAsync(iprot, cancellationToken).CAF();
        await iprot.ReadMessageEndAsync(cancellationToken).CAF();
        var result = new terminateMemberResult();
        try
        {
          result.Success = await _iAsync.terminateMemberAsync(args.ClusterId, args.MemberId, cancellationToken).CAF();
          await oprot.WriteMessageBeginAsync(new TMessage("terminateMember", TMessageType.Reply, seqid), cancellationToken).CAF();
          await result.WriteAsync(oprot, cancellationToken).CAF();
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("terminateMember", TMessageType.Exception, seqid), cancellationToken).CAF();
          await x.WriteAsync(oprot, cancellationToken).CAF();
        }
        await oprot.WriteMessageEndAsync(cancellationToken).CAF();
        await oprot.Transport.FlushAsync(cancellationToken).CAF();
      }

      public async Task suspendMember_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new suspendMemberArgs();
        await args.ReadAsync(iprot, cancellationToken).CAF();
        await iprot.ReadMessageEndAsync(cancellationToken).CAF();
        var result = new suspendMemberResult();
        try
        {
          result.Success = await _iAsync.suspendMemberAsync(args.ClusterId, args.MemberId, cancellationToken).CAF();
          await oprot.WriteMessageBeginAsync(new TMessage("suspendMember", TMessageType.Reply, seqid), cancellationToken).CAF();
          await result.WriteAsync(oprot, cancellationToken).CAF();
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("suspendMember", TMessageType.Exception, seqid), cancellationToken).CAF();
          await x.WriteAsync(oprot, cancellationToken).CAF();
        }
        await oprot.WriteMessageEndAsync(cancellationToken).CAF();
        await oprot.Transport.FlushAsync(cancellationToken).CAF();
      }

      public async Task resumeMember_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new resumeMemberArgs();
        await args.ReadAsync(iprot, cancellationToken).CAF();
        await iprot.ReadMessageEndAsync(cancellationToken).CAF();
        var result = new resumeMemberResult();
        try
        {
          result.Success = await _iAsync.resumeMemberAsync(args.ClusterId, args.MemberId, cancellationToken).CAF();
          await oprot.WriteMessageBeginAsync(new TMessage("resumeMember", TMessageType.Reply, seqid), cancellationToken).CAF();
          await result.WriteAsync(oprot, cancellationToken).CAF();
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("resumeMember", TMessageType.Exception, seqid), cancellationToken).CAF();
          await x.WriteAsync(oprot, cancellationToken).CAF();
        }
        await oprot.WriteMessageEndAsync(cancellationToken).CAF();
        await oprot.Transport.FlushAsync(cancellationToken).CAF();
      }

      public async Task shutdownCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new shutdownClusterArgs();
        await args.ReadAsync(iprot, cancellationToken).CAF();
        await iprot.ReadMessageEndAsync(cancellationToken).CAF();
        var result = new shutdownClusterResult();
        try
        {
          result.Success = await _iAsync.shutdownClusterAsync(args.ClusterId, cancellationToken).CAF();
          await oprot.WriteMessageBeginAsync(new TMessage("shutdownCluster", TMessageType.Reply, seqid), cancellationToken).CAF();
          await result.WriteAsync(oprot, cancellationToken).CAF();
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("shutdownCluster", TMessageType.Exception, seqid), cancellationToken).CAF();
          await x.WriteAsync(oprot, cancellationToken).CAF();
        }
        await oprot.WriteMessageEndAsync(cancellationToken).CAF();
        await oprot.Transport.FlushAsync(cancellationToken).CAF();
      }

      public async Task terminateCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new terminateClusterArgs();
        await args.ReadAsync(iprot, cancellationToken).CAF();
        await iprot.ReadMessageEndAsync(cancellationToken).CAF();
        var result = new terminateClusterResult();
        try
        {
          result.Success = await _iAsync.terminateClusterAsync(args.ClusterId, cancellationToken).CAF();
          await oprot.WriteMessageBeginAsync(new TMessage("terminateCluster", TMessageType.Reply, seqid), cancellationToken).CAF();
          await result.WriteAsync(oprot, cancellationToken).CAF();
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("terminateCluster", TMessageType.Exception, seqid), cancellationToken).CAF();
          await x.WriteAsync(oprot, cancellationToken).CAF();
        }
        await oprot.WriteMessageEndAsync(cancellationToken).CAF();
        await oprot.Transport.FlushAsync(cancellationToken).CAF();
      }

      public async Task splitMemberFromCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new splitMemberFromClusterArgs();
        await args.ReadAsync(iprot, cancellationToken).CAF();
        await iprot.ReadMessageEndAsync(cancellationToken).CAF();
        var result = new splitMemberFromClusterResult();
        try
        {
          result.Success = await _iAsync.splitMemberFromClusterAsync(args.MemberId, cancellationToken).CAF();
          await oprot.WriteMessageBeginAsync(new TMessage("splitMemberFromCluster", TMessageType.Reply, seqid), cancellationToken).CAF();
          await result.WriteAsync(oprot, cancellationToken).CAF();
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("splitMemberFromCluster", TMessageType.Exception, seqid), cancellationToken).CAF();
          await x.WriteAsync(oprot, cancellationToken).CAF();
        }
        await oprot.WriteMessageEndAsync(cancellationToken).CAF();
        await oprot.Transport.FlushAsync(cancellationToken).CAF();
      }

      public async Task mergeMemberToCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new mergeMemberToClusterArgs();
        await args.ReadAsync(iprot, cancellationToken).CAF();
        await iprot.ReadMessageEndAsync(cancellationToken).CAF();
        var result = new mergeMemberToClusterResult();
        try
        {
          result.Success = await _iAsync.mergeMemberToClusterAsync(args.ClusterId, args.MemberId, cancellationToken).CAF();
          await oprot.WriteMessageBeginAsync(new TMessage("mergeMemberToCluster", TMessageType.Reply, seqid), cancellationToken).CAF();
          await result.WriteAsync(oprot, cancellationToken).CAF();
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("mergeMemberToCluster", TMessageType.Exception, seqid), cancellationToken).CAF();
          await x.WriteAsync(oprot, cancellationToken).CAF();
        }
        await oprot.WriteMessageEndAsync(cancellationToken).CAF();
        await oprot.Transport.FlushAsync(cancellationToken).CAF();
      }

      public async Task executeOnController_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new executeOnControllerArgs();
        await args.ReadAsync(iprot, cancellationToken).CAF();
        await iprot.ReadMessageEndAsync(cancellationToken).CAF();
        var result = new executeOnControllerResult();
        try
        {
          result.Success = await _iAsync.executeOnControllerAsync(args.ClusterId, args.Script, args.Lang, cancellationToken).CAF();
          await oprot.WriteMessageBeginAsync(new TMessage("executeOnController", TMessageType.Reply, seqid), cancellationToken).CAF();
          await result.WriteAsync(oprot, cancellationToken).CAF();
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("executeOnController", TMessageType.Exception, seqid), cancellationToken).CAF();
          await x.WriteAsync(oprot, cancellationToken).CAF();
        }
        await oprot.WriteMessageEndAsync(cancellationToken).CAF();
        await oprot.Transport.FlushAsync(cancellationToken).CAF();
      }

    }


    public partial class pingArgs : TBase
    {

      public pingArgs()
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
          var struc = new TStruct("ping_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
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
        var other = that as pingArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return true;
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("ping_args(");
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class pingResult : TBase
    {
      private bool _success;

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


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public pingResult()
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
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken).CAF();
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
          var struc = new TStruct("ping_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteBoolAsync(Success, cancellationToken).CAF();
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
        var other = that as pingResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("ping_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class cleanArgs : TBase
    {

      public cleanArgs()
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
          var struc = new TStruct("clean_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
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
        var other = that as cleanArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return true;
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("clean_args(");
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class cleanResult : TBase
    {
      private bool _success;

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


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public cleanResult()
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
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken).CAF();
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
          var struc = new TStruct("clean_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteBoolAsync(Success, cancellationToken).CAF();
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
        var other = that as cleanResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("clean_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class exitArgs : TBase
    {

      public exitArgs()
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
          var struc = new TStruct("exit_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
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
        var other = that as exitArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return true;
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("exit_args(");
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class exitResult : TBase
    {
      private bool _success;

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


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public exitResult()
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
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken).CAF();
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
          var struc = new TStruct("exit_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteBoolAsync(Success, cancellationToken).CAF();
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
        var other = that as exitResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("exit_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class createClusterArgs : TBase
    {
      private string _hzVersion;
      private string _xmlconfig;

      public string HzVersion
      {
        get
        {
          return _hzVersion;
        }
        set
        {
          __isset.hzVersion = true;
          this._hzVersion = value;
        }
      }

      public string Xmlconfig
      {
        get
        {
          return _xmlconfig;
        }
        set
        {
          __isset.xmlconfig = true;
          this._xmlconfig = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool hzVersion;
        public bool xmlconfig;
      }

      public createClusterArgs()
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
                  HzVersion = await iprot.ReadStringAsync(cancellationToken).CAF();
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken).CAF();
                }
                break;
              case 2:
                if (field.Type == TType.String)
                {
                  Xmlconfig = await iprot.ReadStringAsync(cancellationToken).CAF();
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
          var struc = new TStruct("createCluster_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();
          if (HzVersion != null && __isset.hzVersion)
          {
            field.Name = "hzVersion";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(HzVersion, cancellationToken).CAF();
            await oprot.WriteFieldEndAsync(cancellationToken).CAF();
          }
          if (Xmlconfig != null && __isset.xmlconfig)
          {
            field.Name = "xmlconfig";
            field.Type = TType.String;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(Xmlconfig, cancellationToken).CAF();
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
        var other = that as createClusterArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.hzVersion == other.__isset.hzVersion) && ((!__isset.hzVersion) || (Equals(HzVersion, other.HzVersion))))
          && ((__isset.xmlconfig == other.__isset.xmlconfig) && ((!__isset.xmlconfig) || (Equals(Xmlconfig, other.Xmlconfig))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.hzVersion)
            hashcode = (hashcode * 397) + HzVersion.GetHashCode();
          if(__isset.xmlconfig)
            hashcode = (hashcode * 397) + Xmlconfig.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("createCluster_args(");
        bool __first = true;
        if (HzVersion != null && __isset.hzVersion)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("HzVersion: ");
          sb.Append(HzVersion);
        }
        if (Xmlconfig != null && __isset.xmlconfig)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Xmlconfig: ");
          sb.Append(Xmlconfig);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class createClusterResult : TBase
    {
      private Cluster _success;
      private ServerException _serverException;

      public Cluster Success
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

      public ServerException ServerException
      {
        get
        {
          return _serverException;
        }
        set
        {
          __isset.serverException = true;
          this._serverException = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
        public bool serverException;
      }

      public createClusterResult()
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
              case 0:
                if (field.Type == TType.Struct)
                {
                  Success = new Cluster();
                  await Success.ReadAsync(iprot, cancellationToken).CAF();
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken).CAF();
                }
                break;
              case 1:
                if (field.Type == TType.Struct)
                {
                  ServerException = new ServerException();
                  await ServerException.ReadAsync(iprot, cancellationToken).CAF();
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
          var struc = new TStruct("createCluster_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.Struct;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
              await Success.WriteAsync(oprot, cancellationToken).CAF();
              await oprot.WriteFieldEndAsync(cancellationToken).CAF();
            }
          }
          else if(this.__isset.serverException)
          {
            if (ServerException != null)
            {
              field.Name = "ServerException";
              field.Type = TType.Struct;
              field.ID = 1;
              await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
              await ServerException.WriteAsync(oprot, cancellationToken).CAF();
              await oprot.WriteFieldEndAsync(cancellationToken).CAF();
            }
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
        var other = that as createClusterResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (Equals(Success, other.Success))))
          && ((__isset.serverException == other.__isset.serverException) && ((!__isset.serverException) || (Equals(ServerException, other.ServerException))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
          if(__isset.serverException)
            hashcode = (hashcode * 397) + ServerException.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("createCluster_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success== null ? "<null>" : Success.ToString());
        }
        if (ServerException != null && __isset.serverException)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ServerException: ");
          sb.Append(ServerException== null ? "<null>" : ServerException.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class startMemberArgs : TBase
    {
      private string _clusterId;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
      }

      public startMemberArgs()
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
                  ClusterId = await iprot.ReadStringAsync(cancellationToken).CAF();
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
          var struc = new TStruct("startMember_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(ClusterId, cancellationToken).CAF();
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
        var other = that as startMemberArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (Equals(ClusterId, other.ClusterId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("startMember_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class startMemberResult : TBase
    {
      private Member _success;
      private ServerException _serverException;

      public Member Success
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

      public ServerException ServerException
      {
        get
        {
          return _serverException;
        }
        set
        {
          __isset.serverException = true;
          this._serverException = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
        public bool serverException;
      }

      public startMemberResult()
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
              case 0:
                if (field.Type == TType.Struct)
                {
                  Success = new Member();
                  await Success.ReadAsync(iprot, cancellationToken).CAF();
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken).CAF();
                }
                break;
              case 1:
                if (field.Type == TType.Struct)
                {
                  ServerException = new ServerException();
                  await ServerException.ReadAsync(iprot, cancellationToken).CAF();
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
          var struc = new TStruct("startMember_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.Struct;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
              await Success.WriteAsync(oprot, cancellationToken).CAF();
              await oprot.WriteFieldEndAsync(cancellationToken).CAF();
            }
          }
          else if(this.__isset.serverException)
          {
            if (ServerException != null)
            {
              field.Name = "ServerException";
              field.Type = TType.Struct;
              field.ID = 1;
              await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
              await ServerException.WriteAsync(oprot, cancellationToken).CAF();
              await oprot.WriteFieldEndAsync(cancellationToken).CAF();
            }
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
        var other = that as startMemberResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (Equals(Success, other.Success))))
          && ((__isset.serverException == other.__isset.serverException) && ((!__isset.serverException) || (Equals(ServerException, other.ServerException))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
          if(__isset.serverException)
            hashcode = (hashcode * 397) + ServerException.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("startMember_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success== null ? "<null>" : Success.ToString());
        }
        if (ServerException != null && __isset.serverException)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ServerException: ");
          sb.Append(ServerException== null ? "<null>" : ServerException.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class shutdownMemberArgs : TBase
    {
      private string _clusterId;
      private string _memberId;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }

      public string MemberId
      {
        get
        {
          return _memberId;
        }
        set
        {
          __isset.memberId = true;
          this._memberId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
        public bool memberId;
      }

      public shutdownMemberArgs()
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
                  ClusterId = await iprot.ReadStringAsync(cancellationToken).CAF();
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken).CAF();
                }
                break;
              case 2:
                if (field.Type == TType.String)
                {
                  MemberId = await iprot.ReadStringAsync(cancellationToken).CAF();
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
          var struc = new TStruct("shutdownMember_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(ClusterId, cancellationToken).CAF();
            await oprot.WriteFieldEndAsync(cancellationToken).CAF();
          }
          if (MemberId != null && __isset.memberId)
          {
            field.Name = "memberId";
            field.Type = TType.String;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(MemberId, cancellationToken).CAF();
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
        var other = that as shutdownMemberArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (Equals(ClusterId, other.ClusterId))))
          && ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (Equals(MemberId, other.MemberId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
          if(__isset.memberId)
            hashcode = (hashcode * 397) + MemberId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("shutdownMember_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        if (MemberId != null && __isset.memberId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("MemberId: ");
          sb.Append(MemberId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class shutdownMemberResult : TBase
    {
      private bool _success;

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


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public shutdownMemberResult()
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
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken).CAF();
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
          var struc = new TStruct("shutdownMember_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteBoolAsync(Success, cancellationToken).CAF();
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
        var other = that as shutdownMemberResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("shutdownMember_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class terminateMemberArgs : TBase
    {
      private string _clusterId;
      private string _memberId;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }

      public string MemberId
      {
        get
        {
          return _memberId;
        }
        set
        {
          __isset.memberId = true;
          this._memberId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
        public bool memberId;
      }

      public terminateMemberArgs()
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
                  ClusterId = await iprot.ReadStringAsync(cancellationToken).CAF();
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken).CAF();
                }
                break;
              case 2:
                if (field.Type == TType.String)
                {
                  MemberId = await iprot.ReadStringAsync(cancellationToken).CAF();
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
          var struc = new TStruct("terminateMember_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(ClusterId, cancellationToken).CAF();
            await oprot.WriteFieldEndAsync(cancellationToken).CAF();
          }
          if (MemberId != null && __isset.memberId)
          {
            field.Name = "memberId";
            field.Type = TType.String;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(MemberId, cancellationToken).CAF();
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
        var other = that as terminateMemberArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (Equals(ClusterId, other.ClusterId))))
          && ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (Equals(MemberId, other.MemberId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
          if(__isset.memberId)
            hashcode = (hashcode * 397) + MemberId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("terminateMember_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        if (MemberId != null && __isset.memberId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("MemberId: ");
          sb.Append(MemberId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class terminateMemberResult : TBase
    {
      private bool _success;

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


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public terminateMemberResult()
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
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken).CAF();
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
          var struc = new TStruct("terminateMember_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteBoolAsync(Success, cancellationToken).CAF();
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
        var other = that as terminateMemberResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("terminateMember_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class suspendMemberArgs : TBase
    {
      private string _clusterId;
      private string _memberId;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }

      public string MemberId
      {
        get
        {
          return _memberId;
        }
        set
        {
          __isset.memberId = true;
          this._memberId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
        public bool memberId;
      }

      public suspendMemberArgs()
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
                  ClusterId = await iprot.ReadStringAsync(cancellationToken).CAF();
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken).CAF();
                }
                break;
              case 2:
                if (field.Type == TType.String)
                {
                  MemberId = await iprot.ReadStringAsync(cancellationToken).CAF();
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
          var struc = new TStruct("suspendMember_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(ClusterId, cancellationToken).CAF();
            await oprot.WriteFieldEndAsync(cancellationToken).CAF();
          }
          if (MemberId != null && __isset.memberId)
          {
            field.Name = "memberId";
            field.Type = TType.String;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(MemberId, cancellationToken).CAF();
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
        var other = that as suspendMemberArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (Equals(ClusterId, other.ClusterId))))
          && ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (Equals(MemberId, other.MemberId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
          if(__isset.memberId)
            hashcode = (hashcode * 397) + MemberId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("suspendMember_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        if (MemberId != null && __isset.memberId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("MemberId: ");
          sb.Append(MemberId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class suspendMemberResult : TBase
    {
      private bool _success;

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


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public suspendMemberResult()
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
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken).CAF();
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
          var struc = new TStruct("suspendMember_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteBoolAsync(Success, cancellationToken).CAF();
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
        var other = that as suspendMemberResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("suspendMember_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class resumeMemberArgs : TBase
    {
      private string _clusterId;
      private string _memberId;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }

      public string MemberId
      {
        get
        {
          return _memberId;
        }
        set
        {
          __isset.memberId = true;
          this._memberId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
        public bool memberId;
      }

      public resumeMemberArgs()
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
                  ClusterId = await iprot.ReadStringAsync(cancellationToken).CAF();
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken).CAF();
                }
                break;
              case 2:
                if (field.Type == TType.String)
                {
                  MemberId = await iprot.ReadStringAsync(cancellationToken).CAF();
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
          var struc = new TStruct("resumeMember_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(ClusterId, cancellationToken).CAF();
            await oprot.WriteFieldEndAsync(cancellationToken).CAF();
          }
          if (MemberId != null && __isset.memberId)
          {
            field.Name = "memberId";
            field.Type = TType.String;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(MemberId, cancellationToken).CAF();
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
        var other = that as resumeMemberArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (Equals(ClusterId, other.ClusterId))))
          && ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (Equals(MemberId, other.MemberId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
          if(__isset.memberId)
            hashcode = (hashcode * 397) + MemberId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("resumeMember_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        if (MemberId != null && __isset.memberId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("MemberId: ");
          sb.Append(MemberId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class resumeMemberResult : TBase
    {
      private bool _success;

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


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public resumeMemberResult()
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
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken).CAF();
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
          var struc = new TStruct("resumeMember_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteBoolAsync(Success, cancellationToken).CAF();
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
        var other = that as resumeMemberResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("resumeMember_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class shutdownClusterArgs : TBase
    {
      private string _clusterId;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
      }

      public shutdownClusterArgs()
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
                  ClusterId = await iprot.ReadStringAsync(cancellationToken).CAF();
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
          var struc = new TStruct("shutdownCluster_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(ClusterId, cancellationToken).CAF();
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
        var other = that as shutdownClusterArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (Equals(ClusterId, other.ClusterId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("shutdownCluster_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class shutdownClusterResult : TBase
    {
      private bool _success;

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


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public shutdownClusterResult()
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
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken).CAF();
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
          var struc = new TStruct("shutdownCluster_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteBoolAsync(Success, cancellationToken).CAF();
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
        var other = that as shutdownClusterResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("shutdownCluster_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class terminateClusterArgs : TBase
    {
      private string _clusterId;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
      }

      public terminateClusterArgs()
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
                  ClusterId = await iprot.ReadStringAsync(cancellationToken).CAF();
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
          var struc = new TStruct("terminateCluster_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(ClusterId, cancellationToken).CAF();
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
        var other = that as terminateClusterArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (Equals(ClusterId, other.ClusterId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("terminateCluster_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class terminateClusterResult : TBase
    {
      private bool _success;

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


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public terminateClusterResult()
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
              case 0:
                if (field.Type == TType.Bool)
                {
                  Success = await iprot.ReadBoolAsync(cancellationToken).CAF();
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
          var struc = new TStruct("terminateCluster_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();

          if(this.__isset.success)
          {
            field.Name = "Success";
            field.Type = TType.Bool;
            field.ID = 0;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteBoolAsync(Success, cancellationToken).CAF();
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
        var other = that as terminateClusterResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("terminateCluster_result(");
        bool __first = true;
        if (__isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class splitMemberFromClusterArgs : TBase
    {
      private string _memberId;

      public string MemberId
      {
        get
        {
          return _memberId;
        }
        set
        {
          __isset.memberId = true;
          this._memberId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool memberId;
      }

      public splitMemberFromClusterArgs()
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
                  MemberId = await iprot.ReadStringAsync(cancellationToken).CAF();
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
          var struc = new TStruct("splitMemberFromCluster_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();
          if (MemberId != null && __isset.memberId)
          {
            field.Name = "memberId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(MemberId, cancellationToken).CAF();
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
        var other = that as splitMemberFromClusterArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (Equals(MemberId, other.MemberId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.memberId)
            hashcode = (hashcode * 397) + MemberId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("splitMemberFromCluster_args(");
        bool __first = true;
        if (MemberId != null && __isset.memberId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("MemberId: ");
          sb.Append(MemberId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class splitMemberFromClusterResult : TBase
    {
      private Cluster _success;

      public Cluster Success
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


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public splitMemberFromClusterResult()
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
              case 0:
                if (field.Type == TType.Struct)
                {
                  Success = new Cluster();
                  await Success.ReadAsync(iprot, cancellationToken).CAF();
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
          var struc = new TStruct("splitMemberFromCluster_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.Struct;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
              await Success.WriteAsync(oprot, cancellationToken).CAF();
              await oprot.WriteFieldEndAsync(cancellationToken).CAF();
            }
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
        var other = that as splitMemberFromClusterResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("splitMemberFromCluster_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success== null ? "<null>" : Success.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class mergeMemberToClusterArgs : TBase
    {
      private string _clusterId;
      private string _memberId;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }

      public string MemberId
      {
        get
        {
          return _memberId;
        }
        set
        {
          __isset.memberId = true;
          this._memberId = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
        public bool memberId;
      }

      public mergeMemberToClusterArgs()
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
                  ClusterId = await iprot.ReadStringAsync(cancellationToken).CAF();
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken).CAF();
                }
                break;
              case 2:
                if (field.Type == TType.String)
                {
                  MemberId = await iprot.ReadStringAsync(cancellationToken).CAF();
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
          var struc = new TStruct("mergeMemberToCluster_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(ClusterId, cancellationToken).CAF();
            await oprot.WriteFieldEndAsync(cancellationToken).CAF();
          }
          if (MemberId != null && __isset.memberId)
          {
            field.Name = "memberId";
            field.Type = TType.String;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(MemberId, cancellationToken).CAF();
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
        var other = that as mergeMemberToClusterArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (Equals(ClusterId, other.ClusterId))))
          && ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (Equals(MemberId, other.MemberId))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
          if(__isset.memberId)
            hashcode = (hashcode * 397) + MemberId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("mergeMemberToCluster_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        if (MemberId != null && __isset.memberId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("MemberId: ");
          sb.Append(MemberId);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class mergeMemberToClusterResult : TBase
    {
      private Cluster _success;

      public Cluster Success
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


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public mergeMemberToClusterResult()
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
              case 0:
                if (field.Type == TType.Struct)
                {
                  Success = new Cluster();
                  await Success.ReadAsync(iprot, cancellationToken).CAF();
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
          var struc = new TStruct("mergeMemberToCluster_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.Struct;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
              await Success.WriteAsync(oprot, cancellationToken).CAF();
              await oprot.WriteFieldEndAsync(cancellationToken).CAF();
            }
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
        var other = that as mergeMemberToClusterResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("mergeMemberToCluster_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success== null ? "<null>" : Success.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class executeOnControllerArgs : TBase
    {
      private string _clusterId;
      private string _script;
      private Lang _lang;

      public string ClusterId
      {
        get
        {
          return _clusterId;
        }
        set
        {
          __isset.clusterId = true;
          this._clusterId = value;
        }
      }

      public string Script
      {
        get
        {
          return _script;
        }
        set
        {
          __isset.script = true;
          this._script = value;
        }
      }

      /// <summary>
      ///
      /// <seealso cref="Lang"/>
      /// </summary>
      public Lang Lang
      {
        get
        {
          return _lang;
        }
        set
        {
          __isset.lang = true;
          this._lang = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool clusterId;
        public bool script;
        public bool lang;
      }

      public executeOnControllerArgs()
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
                  ClusterId = await iprot.ReadStringAsync(cancellationToken).CAF();
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken).CAF();
                }
                break;
              case 2:
                if (field.Type == TType.String)
                {
                  Script = await iprot.ReadStringAsync(cancellationToken).CAF();
                }
                else
                {
                  await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken).CAF();
                }
                break;
              case 3:
                if (field.Type == TType.I32)
                {
                  Lang = (Lang)await iprot.ReadI32Async(cancellationToken).CAF();
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
          var struc = new TStruct("executeOnController_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();
          if (ClusterId != null && __isset.clusterId)
          {
            field.Name = "clusterId";
            field.Type = TType.String;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(ClusterId, cancellationToken).CAF();
            await oprot.WriteFieldEndAsync(cancellationToken).CAF();
          }
          if (Script != null && __isset.script)
          {
            field.Name = "script";
            field.Type = TType.String;
            field.ID = 2;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteStringAsync(Script, cancellationToken).CAF();
            await oprot.WriteFieldEndAsync(cancellationToken).CAF();
          }
          if (__isset.lang)
          {
            field.Name = "lang";
            field.Type = TType.I32;
            field.ID = 3;
            await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
            await oprot.WriteI32Async((int)Lang, cancellationToken).CAF();
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
        var other = that as executeOnControllerArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (Equals(ClusterId, other.ClusterId))))
          && ((__isset.script == other.__isset.script) && ((!__isset.script) || (Equals(Script, other.Script))))
          && ((__isset.lang == other.__isset.lang) && ((!__isset.lang) || (Equals(Lang, other.Lang))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.clusterId)
            hashcode = (hashcode * 397) + ClusterId.GetHashCode();
          if(__isset.script)
            hashcode = (hashcode * 397) + Script.GetHashCode();
          if(__isset.lang)
            hashcode = (hashcode * 397) + Lang.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("executeOnController_args(");
        bool __first = true;
        if (ClusterId != null && __isset.clusterId)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("ClusterId: ");
          sb.Append(ClusterId);
        }
        if (Script != null && __isset.script)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Script: ");
          sb.Append(Script);
        }
        if (__isset.lang)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Lang: ");
          sb.Append(Lang);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class executeOnControllerResult : TBase
    {
      private Response _success;

      public Response Success
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


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public executeOnControllerResult()
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
              case 0:
                if (field.Type == TType.Struct)
                {
                  Success = new Response();
                  await Success.ReadAsync(iprot, cancellationToken).CAF();
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
          var struc = new TStruct("executeOnController_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken).CAF();
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.Struct;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken).CAF();
              await Success.WriteAsync(oprot, cancellationToken).CAF();
              await oprot.WriteFieldEndAsync(cancellationToken).CAF();
            }
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
        var other = that as executeOnControllerResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("executeOnController_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success== null ? "<null>" : Success.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }

  }
}
