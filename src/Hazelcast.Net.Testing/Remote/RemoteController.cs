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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thrift;
using Thrift.Processor;
using Thrift.Protocol;
using Thrift.Protocol.Entities;
using Thrift.Protocol.Utilities;
using Thrift.Transport;

#pragma warning disable IDE0079  // remove unnecessary pragmas
#pragma warning disable IDE1006  // parts of the code use IDL spelling
#pragma warning disable IDE0083  // pattern matching "that is not SomeType" requires net5.0 but we still support earlier versions

namespace Hazelcast.Testing.Remote
{
  public partial class RemoteController
  {
    public interface IAsync
    {
      global::System.Threading.Tasks.Task<bool> ping(CancellationToken cancellationToken = default);

      global::System.Threading.Tasks.Task<bool> clean(CancellationToken cancellationToken = default);

      global::System.Threading.Tasks.Task<bool> exit(CancellationToken cancellationToken = default);

      global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Cluster> createCluster(string hzVersion, string xmlconfig, CancellationToken cancellationToken = default);

      global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Cluster> createClusterKeepClusterName(string hzVersion, string xmlconfig, CancellationToken cancellationToken = default);

      global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Member> startMember(string clusterId, CancellationToken cancellationToken = default);

      global::System.Threading.Tasks.Task<bool> shutdownMember(string clusterId, string memberId, CancellationToken cancellationToken = default);

      global::System.Threading.Tasks.Task<bool> terminateMember(string clusterId, string memberId, CancellationToken cancellationToken = default);

      global::System.Threading.Tasks.Task<bool> suspendMember(string clusterId, string memberId, CancellationToken cancellationToken = default);

      global::System.Threading.Tasks.Task<bool> resumeMember(string clusterId, string memberId, CancellationToken cancellationToken = default);

      global::System.Threading.Tasks.Task<bool> shutdownCluster(string clusterId, CancellationToken cancellationToken = default);

      global::System.Threading.Tasks.Task<bool> terminateCluster(string clusterId, CancellationToken cancellationToken = default);

      global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Cluster> splitMemberFromCluster(string memberId, CancellationToken cancellationToken = default);

      global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Cluster> mergeMemberToCluster(string clusterId, string memberId, CancellationToken cancellationToken = default);

      global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Response> executeOnController(string clusterId, string script, global::Hazelcast.Testing.Remote.Lang lang, CancellationToken cancellationToken = default);

    }


    public class Client : TBaseClient, IDisposable, IAsync
    {
      public Client(TProtocol protocol) : this(protocol, protocol)
      {
      }

      public Client(TProtocol inputProtocol, TProtocol outputProtocol) : base(inputProtocol, outputProtocol)
      {
      }

      public async global::System.Threading.Tasks.Task<bool> ping(CancellationToken cancellationToken = default)
      {
        await send_ping(cancellationToken);
        return await recv_ping(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task send_ping(CancellationToken cancellationToken = default)
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("ping", TMessageType.Call, SeqId), cancellationToken);

        var tmp20 = new InternalStructs.ping_args() {
        };

        await tmp20.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task<bool> recv_ping(CancellationToken cancellationToken = default)
      {

        var tmp21 = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (tmp21.Type == TMessageType.Exception)
        {
          var tmp22 = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw tmp22;
        }

        var tmp23 = new InternalStructs.ping_result();
        await tmp23.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (tmp23.__isset.success)
        {
          return tmp23.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "ping failed: unknown result");
      }

      public async global::System.Threading.Tasks.Task<bool> clean(CancellationToken cancellationToken = default)
      {
        await send_clean(cancellationToken);
        return await recv_clean(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task send_clean(CancellationToken cancellationToken = default)
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("clean", TMessageType.Call, SeqId), cancellationToken);

        var tmp24 = new InternalStructs.clean_args() {
        };

        await tmp24.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task<bool> recv_clean(CancellationToken cancellationToken = default)
      {

        var tmp25 = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (tmp25.Type == TMessageType.Exception)
        {
          var tmp26 = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw tmp26;
        }

        var tmp27 = new InternalStructs.clean_result();
        await tmp27.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (tmp27.__isset.success)
        {
          return tmp27.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "clean failed: unknown result");
      }

      public async global::System.Threading.Tasks.Task<bool> exit(CancellationToken cancellationToken = default)
      {
        await send_exit(cancellationToken);
        return await recv_exit(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task send_exit(CancellationToken cancellationToken = default)
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("exit", TMessageType.Call, SeqId), cancellationToken);

        var tmp28 = new InternalStructs.exit_args() {
        };

        await tmp28.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task<bool> recv_exit(CancellationToken cancellationToken = default)
      {

        var tmp29 = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (tmp29.Type == TMessageType.Exception)
        {
          var tmp30 = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw tmp30;
        }

        var tmp31 = new InternalStructs.exit_result();
        await tmp31.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (tmp31.__isset.success)
        {
          return tmp31.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "exit failed: unknown result");
      }

      public async global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Cluster> createCluster(string hzVersion, string xmlconfig, CancellationToken cancellationToken = default)
      {
        await send_createCluster(hzVersion, xmlconfig, cancellationToken);
        return await recv_createCluster(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task send_createCluster(string hzVersion, string xmlconfig, CancellationToken cancellationToken = default)
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("createCluster", TMessageType.Call, SeqId), cancellationToken);

        var tmp32 = new InternalStructs.createCluster_args() {
          HzVersion = hzVersion,
          Xmlconfig = xmlconfig,
        };

        await tmp32.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Cluster> recv_createCluster(CancellationToken cancellationToken = default)
      {

        var tmp33 = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (tmp33.Type == TMessageType.Exception)
        {
          var tmp34 = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw tmp34;
        }

        var tmp35 = new InternalStructs.createCluster_result();
        await tmp35.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (tmp35.__isset.success)
        {
          return tmp35.Success;
        }
        if (tmp35.__isset.serverException)
        {
          throw tmp35.ServerException;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "createCluster failed: unknown result");
      }

      public async global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Cluster> createClusterKeepClusterName(string hzVersion, string xmlconfig, CancellationToken cancellationToken = default)
      {
        await send_createClusterKeepClusterName(hzVersion, xmlconfig, cancellationToken);
        return await recv_createClusterKeepClusterName(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task send_createClusterKeepClusterName(string hzVersion, string xmlconfig, CancellationToken cancellationToken = default)
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("createClusterKeepClusterName", TMessageType.Call, SeqId), cancellationToken);

        var tmp36 = new InternalStructs.createClusterKeepClusterName_args() {
          HzVersion = hzVersion,
          Xmlconfig = xmlconfig,
        };

        await tmp36.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Cluster> recv_createClusterKeepClusterName(CancellationToken cancellationToken = default)
      {

        var tmp37 = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (tmp37.Type == TMessageType.Exception)
        {
          var tmp38 = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw tmp38;
        }

        var tmp39 = new InternalStructs.createClusterKeepClusterName_result();
        await tmp39.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (tmp39.__isset.success)
        {
          return tmp39.Success;
        }
        if (tmp39.__isset.serverException)
        {
          throw tmp39.ServerException;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "createClusterKeepClusterName failed: unknown result");
      }

      public async global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Member> startMember(string clusterId, CancellationToken cancellationToken = default)
      {
        await send_startMember(clusterId, cancellationToken);
        return await recv_startMember(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task send_startMember(string clusterId, CancellationToken cancellationToken = default)
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("startMember", TMessageType.Call, SeqId), cancellationToken);

        var tmp40 = new InternalStructs.startMember_args() {
          ClusterId = clusterId,
        };

        await tmp40.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Member> recv_startMember(CancellationToken cancellationToken = default)
      {

        var tmp41 = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (tmp41.Type == TMessageType.Exception)
        {
          var tmp42 = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw tmp42;
        }

        var tmp43 = new InternalStructs.startMember_result();
        await tmp43.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (tmp43.__isset.success)
        {
          return tmp43.Success;
        }
        if (tmp43.__isset.serverException)
        {
          throw tmp43.ServerException;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "startMember failed: unknown result");
      }

      public async global::System.Threading.Tasks.Task<bool> shutdownMember(string clusterId, string memberId, CancellationToken cancellationToken = default)
      {
        await send_shutdownMember(clusterId, memberId, cancellationToken);
        return await recv_shutdownMember(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task send_shutdownMember(string clusterId, string memberId, CancellationToken cancellationToken = default)
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("shutdownMember", TMessageType.Call, SeqId), cancellationToken);

        var tmp44 = new InternalStructs.shutdownMember_args() {
          ClusterId = clusterId,
          MemberId = memberId,
        };

        await tmp44.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task<bool> recv_shutdownMember(CancellationToken cancellationToken = default)
      {

        var tmp45 = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (tmp45.Type == TMessageType.Exception)
        {
          var tmp46 = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw tmp46;
        }

        var tmp47 = new InternalStructs.shutdownMember_result();
        await tmp47.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (tmp47.__isset.success)
        {
          return tmp47.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "shutdownMember failed: unknown result");
      }

      public async global::System.Threading.Tasks.Task<bool> terminateMember(string clusterId, string memberId, CancellationToken cancellationToken = default)
      {
        await send_terminateMember(clusterId, memberId, cancellationToken);
        return await recv_terminateMember(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task send_terminateMember(string clusterId, string memberId, CancellationToken cancellationToken = default)
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("terminateMember", TMessageType.Call, SeqId), cancellationToken);

        var tmp48 = new InternalStructs.terminateMember_args() {
          ClusterId = clusterId,
          MemberId = memberId,
        };

        await tmp48.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task<bool> recv_terminateMember(CancellationToken cancellationToken = default)
      {

        var tmp49 = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (tmp49.Type == TMessageType.Exception)
        {
          var tmp50 = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw tmp50;
        }

        var tmp51 = new InternalStructs.terminateMember_result();
        await tmp51.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (tmp51.__isset.success)
        {
          return tmp51.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "terminateMember failed: unknown result");
      }

      public async global::System.Threading.Tasks.Task<bool> suspendMember(string clusterId, string memberId, CancellationToken cancellationToken = default)
      {
        await send_suspendMember(clusterId, memberId, cancellationToken);
        return await recv_suspendMember(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task send_suspendMember(string clusterId, string memberId, CancellationToken cancellationToken = default)
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("suspendMember", TMessageType.Call, SeqId), cancellationToken);

        var tmp52 = new InternalStructs.suspendMember_args() {
          ClusterId = clusterId,
          MemberId = memberId,
        };

        await tmp52.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task<bool> recv_suspendMember(CancellationToken cancellationToken = default)
      {

        var tmp53 = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (tmp53.Type == TMessageType.Exception)
        {
          var tmp54 = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw tmp54;
        }

        var tmp55 = new InternalStructs.suspendMember_result();
        await tmp55.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (tmp55.__isset.success)
        {
          return tmp55.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "suspendMember failed: unknown result");
      }

      public async global::System.Threading.Tasks.Task<bool> resumeMember(string clusterId, string memberId, CancellationToken cancellationToken = default)
      {
        await send_resumeMember(clusterId, memberId, cancellationToken);
        return await recv_resumeMember(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task send_resumeMember(string clusterId, string memberId, CancellationToken cancellationToken = default)
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("resumeMember", TMessageType.Call, SeqId), cancellationToken);

        var tmp56 = new InternalStructs.resumeMember_args() {
          ClusterId = clusterId,
          MemberId = memberId,
        };

        await tmp56.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task<bool> recv_resumeMember(CancellationToken cancellationToken = default)
      {

        var tmp57 = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (tmp57.Type == TMessageType.Exception)
        {
          var tmp58 = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw tmp58;
        }

        var tmp59 = new InternalStructs.resumeMember_result();
        await tmp59.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (tmp59.__isset.success)
        {
          return tmp59.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "resumeMember failed: unknown result");
      }

      public async global::System.Threading.Tasks.Task<bool> shutdownCluster(string clusterId, CancellationToken cancellationToken = default)
      {
        await send_shutdownCluster(clusterId, cancellationToken);
        return await recv_shutdownCluster(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task send_shutdownCluster(string clusterId, CancellationToken cancellationToken = default)
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("shutdownCluster", TMessageType.Call, SeqId), cancellationToken);

        var tmp60 = new InternalStructs.shutdownCluster_args() {
          ClusterId = clusterId,
        };

        await tmp60.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task<bool> recv_shutdownCluster(CancellationToken cancellationToken = default)
      {

        var tmp61 = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (tmp61.Type == TMessageType.Exception)
        {
          var tmp62 = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw tmp62;
        }

        var tmp63 = new InternalStructs.shutdownCluster_result();
        await tmp63.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (tmp63.__isset.success)
        {
          return tmp63.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "shutdownCluster failed: unknown result");
      }

      public async global::System.Threading.Tasks.Task<bool> terminateCluster(string clusterId, CancellationToken cancellationToken = default)
      {
        await send_terminateCluster(clusterId, cancellationToken);
        return await recv_terminateCluster(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task send_terminateCluster(string clusterId, CancellationToken cancellationToken = default)
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("terminateCluster", TMessageType.Call, SeqId), cancellationToken);

        var tmp64 = new InternalStructs.terminateCluster_args() {
          ClusterId = clusterId,
        };

        await tmp64.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task<bool> recv_terminateCluster(CancellationToken cancellationToken = default)
      {

        var tmp65 = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (tmp65.Type == TMessageType.Exception)
        {
          var tmp66 = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw tmp66;
        }

        var tmp67 = new InternalStructs.terminateCluster_result();
        await tmp67.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (tmp67.__isset.success)
        {
          return tmp67.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "terminateCluster failed: unknown result");
      }

      public async global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Cluster> splitMemberFromCluster(string memberId, CancellationToken cancellationToken = default)
      {
        await send_splitMemberFromCluster(memberId, cancellationToken);
        return await recv_splitMemberFromCluster(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task send_splitMemberFromCluster(string memberId, CancellationToken cancellationToken = default)
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("splitMemberFromCluster", TMessageType.Call, SeqId), cancellationToken);

        var tmp68 = new InternalStructs.splitMemberFromCluster_args() {
          MemberId = memberId,
        };

        await tmp68.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Cluster> recv_splitMemberFromCluster(CancellationToken cancellationToken = default)
      {

        var tmp69 = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (tmp69.Type == TMessageType.Exception)
        {
          var tmp70 = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw tmp70;
        }

        var tmp71 = new InternalStructs.splitMemberFromCluster_result();
        await tmp71.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (tmp71.__isset.success)
        {
          return tmp71.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "splitMemberFromCluster failed: unknown result");
      }

      public async global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Cluster> mergeMemberToCluster(string clusterId, string memberId, CancellationToken cancellationToken = default)
      {
        await send_mergeMemberToCluster(clusterId, memberId, cancellationToken);
        return await recv_mergeMemberToCluster(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task send_mergeMemberToCluster(string clusterId, string memberId, CancellationToken cancellationToken = default)
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("mergeMemberToCluster", TMessageType.Call, SeqId), cancellationToken);

        var tmp72 = new InternalStructs.mergeMemberToCluster_args() {
          ClusterId = clusterId,
          MemberId = memberId,
        };

        await tmp72.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Cluster> recv_mergeMemberToCluster(CancellationToken cancellationToken = default)
      {

        var tmp73 = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (tmp73.Type == TMessageType.Exception)
        {
          var tmp74 = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw tmp74;
        }

        var tmp75 = new InternalStructs.mergeMemberToCluster_result();
        await tmp75.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (tmp75.__isset.success)
        {
          return tmp75.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "mergeMemberToCluster failed: unknown result");
      }

      public async global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Response> executeOnController(string clusterId, string script, global::Hazelcast.Testing.Remote.Lang lang, CancellationToken cancellationToken = default)
      {
        await send_executeOnController(clusterId, script, lang, cancellationToken);
        return await recv_executeOnController(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task send_executeOnController(string clusterId, string script, global::Hazelcast.Testing.Remote.Lang lang, CancellationToken cancellationToken = default)
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("executeOnController", TMessageType.Call, SeqId), cancellationToken);

        var tmp76 = new InternalStructs.executeOnController_args() {
          ClusterId = clusterId,
          Script = script,
          Lang = lang,
        };

        await tmp76.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task<global::Hazelcast.Testing.Remote.Response> recv_executeOnController(CancellationToken cancellationToken = default)
      {

        var tmp77 = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (tmp77.Type == TMessageType.Exception)
        {
          var tmp78 = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw tmp78;
        }

        var tmp79 = new InternalStructs.executeOnController_result();
        await tmp79.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (tmp79.__isset.success)
        {
          return tmp79.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "executeOnController failed: unknown result");
      }

    }

    public class AsyncProcessor : ITAsyncProcessor
    {
      private readonly IAsync _iAsync;
      private readonly ILogger<AsyncProcessor> _logger;

      public AsyncProcessor(IAsync iAsync, ILogger<AsyncProcessor> logger = default)
      {
        _iAsync = iAsync ?? throw new ArgumentNullException(nameof(iAsync));
        _logger = logger;
        processMap_["ping"] = ping_ProcessAsync;
        processMap_["clean"] = clean_ProcessAsync;
        processMap_["exit"] = exit_ProcessAsync;
        processMap_["createCluster"] = createCluster_ProcessAsync;
        processMap_["createClusterKeepClusterName"] = createClusterKeepClusterName_ProcessAsync;
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

      protected delegate global::System.Threading.Tasks.Task ProcessFunction(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken);
      protected Dictionary<string, ProcessFunction> processMap_ = new Dictionary<string, ProcessFunction>();

      public async Task<bool> ProcessAsync(TProtocol iprot, TProtocol oprot)
      {
        return await ProcessAsync(iprot, oprot, CancellationToken.None);
      }

      public async Task<bool> ProcessAsync(TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        try
        {
          var msg = await iprot.ReadMessageBeginAsync(cancellationToken);

          processMap_.TryGetValue(msg.Name, out ProcessFunction fn);

          if (fn == null)
          {
            await TProtocolUtil.SkipAsync(iprot, TType.Struct, cancellationToken);
            await iprot.ReadMessageEndAsync(cancellationToken);
            var x = new TApplicationException (TApplicationException.ExceptionType.UnknownMethod, "Invalid method name: '" + msg.Name + "'");
            await oprot.WriteMessageBeginAsync(new TMessage(msg.Name, TMessageType.Exception, msg.SeqID), cancellationToken);
            await x.WriteAsync(oprot, cancellationToken);
            await oprot.WriteMessageEndAsync(cancellationToken);
            await oprot.Transport.FlushAsync(cancellationToken);
            return true;
          }

          await fn(msg.SeqID, iprot, oprot, cancellationToken);

        }
        catch (IOException)
        {
          return false;
        }

        return true;
      }

      public async global::System.Threading.Tasks.Task ping_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var tmp80 = new InternalStructs.ping_args();
        await tmp80.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var tmp81 = new InternalStructs.ping_result();
        try
        {
          tmp81.Success = await _iAsync.ping(cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("ping", TMessageType.Reply, seqid), cancellationToken);
          await tmp81.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception tmp82)
        {
          var tmp83 = $"Error occurred in {GetType().FullName}: {tmp82.Message}";
          if(_logger != null)
            _logger.LogError(tmp82, tmp83);
          else
            Console.Error.WriteLine(tmp83);
          var tmp84 = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("ping", TMessageType.Exception, seqid), cancellationToken);
          await tmp84.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task clean_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var tmp85 = new InternalStructs.clean_args();
        await tmp85.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var tmp86 = new InternalStructs.clean_result();
        try
        {
          tmp86.Success = await _iAsync.clean(cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("clean", TMessageType.Reply, seqid), cancellationToken);
          await tmp86.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception tmp87)
        {
          var tmp88 = $"Error occurred in {GetType().FullName}: {tmp87.Message}";
          if(_logger != null)
            _logger.LogError(tmp87, tmp88);
          else
            Console.Error.WriteLine(tmp88);
          var tmp89 = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("clean", TMessageType.Exception, seqid), cancellationToken);
          await tmp89.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task exit_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var tmp90 = new InternalStructs.exit_args();
        await tmp90.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var tmp91 = new InternalStructs.exit_result();
        try
        {
          tmp91.Success = await _iAsync.exit(cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("exit", TMessageType.Reply, seqid), cancellationToken);
          await tmp91.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception tmp92)
        {
          var tmp93 = $"Error occurred in {GetType().FullName}: {tmp92.Message}";
          if(_logger != null)
            _logger.LogError(tmp92, tmp93);
          else
            Console.Error.WriteLine(tmp93);
          var tmp94 = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("exit", TMessageType.Exception, seqid), cancellationToken);
          await tmp94.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task createCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var tmp95 = new InternalStructs.createCluster_args();
        await tmp95.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var tmp96 = new InternalStructs.createCluster_result();
        try
        {
          try
          {
            tmp96.Success = await _iAsync.createCluster(tmp95.HzVersion, tmp95.Xmlconfig, cancellationToken);
          }
          catch (global::Hazelcast.Testing.Remote.ServerException tmp97)
          {
            tmp96.ServerException = tmp97;
          }
          await oprot.WriteMessageBeginAsync(new TMessage("createCluster", TMessageType.Reply, seqid), cancellationToken);
          await tmp96.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception tmp98)
        {
          var tmp99 = $"Error occurred in {GetType().FullName}: {tmp98.Message}";
          if(_logger != null)
            _logger.LogError(tmp98, tmp99);
          else
            Console.Error.WriteLine(tmp99);
          var tmp100 = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("createCluster", TMessageType.Exception, seqid), cancellationToken);
          await tmp100.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task createClusterKeepClusterName_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var tmp101 = new InternalStructs.createClusterKeepClusterName_args();
        await tmp101.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var tmp102 = new InternalStructs.createClusterKeepClusterName_result();
        try
        {
          try
          {
            tmp102.Success = await _iAsync.createClusterKeepClusterName(tmp101.HzVersion, tmp101.Xmlconfig, cancellationToken);
          }
          catch (global::Hazelcast.Testing.Remote.ServerException tmp103)
          {
            tmp102.ServerException = tmp103;
          }
          await oprot.WriteMessageBeginAsync(new TMessage("createClusterKeepClusterName", TMessageType.Reply, seqid), cancellationToken);
          await tmp102.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception tmp104)
        {
          var tmp105 = $"Error occurred in {GetType().FullName}: {tmp104.Message}";
          if(_logger != null)
            _logger.LogError(tmp104, tmp105);
          else
            Console.Error.WriteLine(tmp105);
          var tmp106 = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("createClusterKeepClusterName", TMessageType.Exception, seqid), cancellationToken);
          await tmp106.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task startMember_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var tmp107 = new InternalStructs.startMember_args();
        await tmp107.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var tmp108 = new InternalStructs.startMember_result();
        try
        {
          try
          {
            tmp108.Success = await _iAsync.startMember(tmp107.ClusterId, cancellationToken);
          }
          catch (global::Hazelcast.Testing.Remote.ServerException tmp109)
          {
            tmp108.ServerException = tmp109;
          }
          await oprot.WriteMessageBeginAsync(new TMessage("startMember", TMessageType.Reply, seqid), cancellationToken);
          await tmp108.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception tmp110)
        {
          var tmp111 = $"Error occurred in {GetType().FullName}: {tmp110.Message}";
          if(_logger != null)
            _logger.LogError(tmp110, tmp111);
          else
            Console.Error.WriteLine(tmp111);
          var tmp112 = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("startMember", TMessageType.Exception, seqid), cancellationToken);
          await tmp112.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task shutdownMember_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var tmp113 = new InternalStructs.shutdownMember_args();
        await tmp113.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var tmp114 = new InternalStructs.shutdownMember_result();
        try
        {
          tmp114.Success = await _iAsync.shutdownMember(tmp113.ClusterId, tmp113.MemberId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("shutdownMember", TMessageType.Reply, seqid), cancellationToken);
          await tmp114.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception tmp115)
        {
          var tmp116 = $"Error occurred in {GetType().FullName}: {tmp115.Message}";
          if(_logger != null)
            _logger.LogError(tmp115, tmp116);
          else
            Console.Error.WriteLine(tmp116);
          var tmp117 = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("shutdownMember", TMessageType.Exception, seqid), cancellationToken);
          await tmp117.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task terminateMember_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var tmp118 = new InternalStructs.terminateMember_args();
        await tmp118.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var tmp119 = new InternalStructs.terminateMember_result();
        try
        {
          tmp119.Success = await _iAsync.terminateMember(tmp118.ClusterId, tmp118.MemberId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("terminateMember", TMessageType.Reply, seqid), cancellationToken);
          await tmp119.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception tmp120)
        {
          var tmp121 = $"Error occurred in {GetType().FullName}: {tmp120.Message}";
          if(_logger != null)
            _logger.LogError(tmp120, tmp121);
          else
            Console.Error.WriteLine(tmp121);
          var tmp122 = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("terminateMember", TMessageType.Exception, seqid), cancellationToken);
          await tmp122.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task suspendMember_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var tmp123 = new InternalStructs.suspendMember_args();
        await tmp123.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var tmp124 = new InternalStructs.suspendMember_result();
        try
        {
          tmp124.Success = await _iAsync.suspendMember(tmp123.ClusterId, tmp123.MemberId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("suspendMember", TMessageType.Reply, seqid), cancellationToken);
          await tmp124.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception tmp125)
        {
          var tmp126 = $"Error occurred in {GetType().FullName}: {tmp125.Message}";
          if(_logger != null)
            _logger.LogError(tmp125, tmp126);
          else
            Console.Error.WriteLine(tmp126);
          var tmp127 = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("suspendMember", TMessageType.Exception, seqid), cancellationToken);
          await tmp127.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task resumeMember_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var tmp128 = new InternalStructs.resumeMember_args();
        await tmp128.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var tmp129 = new InternalStructs.resumeMember_result();
        try
        {
          tmp129.Success = await _iAsync.resumeMember(tmp128.ClusterId, tmp128.MemberId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("resumeMember", TMessageType.Reply, seqid), cancellationToken);
          await tmp129.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception tmp130)
        {
          var tmp131 = $"Error occurred in {GetType().FullName}: {tmp130.Message}";
          if(_logger != null)
            _logger.LogError(tmp130, tmp131);
          else
            Console.Error.WriteLine(tmp131);
          var tmp132 = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("resumeMember", TMessageType.Exception, seqid), cancellationToken);
          await tmp132.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task shutdownCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var tmp133 = new InternalStructs.shutdownCluster_args();
        await tmp133.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var tmp134 = new InternalStructs.shutdownCluster_result();
        try
        {
          tmp134.Success = await _iAsync.shutdownCluster(tmp133.ClusterId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("shutdownCluster", TMessageType.Reply, seqid), cancellationToken);
          await tmp134.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception tmp135)
        {
          var tmp136 = $"Error occurred in {GetType().FullName}: {tmp135.Message}";
          if(_logger != null)
            _logger.LogError(tmp135, tmp136);
          else
            Console.Error.WriteLine(tmp136);
          var tmp137 = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("shutdownCluster", TMessageType.Exception, seqid), cancellationToken);
          await tmp137.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task terminateCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var tmp138 = new InternalStructs.terminateCluster_args();
        await tmp138.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var tmp139 = new InternalStructs.terminateCluster_result();
        try
        {
          tmp139.Success = await _iAsync.terminateCluster(tmp138.ClusterId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("terminateCluster", TMessageType.Reply, seqid), cancellationToken);
          await tmp139.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception tmp140)
        {
          var tmp141 = $"Error occurred in {GetType().FullName}: {tmp140.Message}";
          if(_logger != null)
            _logger.LogError(tmp140, tmp141);
          else
            Console.Error.WriteLine(tmp141);
          var tmp142 = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("terminateCluster", TMessageType.Exception, seqid), cancellationToken);
          await tmp142.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task splitMemberFromCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var tmp143 = new InternalStructs.splitMemberFromCluster_args();
        await tmp143.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var tmp144 = new InternalStructs.splitMemberFromCluster_result();
        try
        {
          tmp144.Success = await _iAsync.splitMemberFromCluster(tmp143.MemberId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("splitMemberFromCluster", TMessageType.Reply, seqid), cancellationToken);
          await tmp144.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception tmp145)
        {
          var tmp146 = $"Error occurred in {GetType().FullName}: {tmp145.Message}";
          if(_logger != null)
            _logger.LogError(tmp145, tmp146);
          else
            Console.Error.WriteLine(tmp146);
          var tmp147 = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("splitMemberFromCluster", TMessageType.Exception, seqid), cancellationToken);
          await tmp147.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task mergeMemberToCluster_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var tmp148 = new InternalStructs.mergeMemberToCluster_args();
        await tmp148.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var tmp149 = new InternalStructs.mergeMemberToCluster_result();
        try
        {
          tmp149.Success = await _iAsync.mergeMemberToCluster(tmp148.ClusterId, tmp148.MemberId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("mergeMemberToCluster", TMessageType.Reply, seqid), cancellationToken);
          await tmp149.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception tmp150)
        {
          var tmp151 = $"Error occurred in {GetType().FullName}: {tmp150.Message}";
          if(_logger != null)
            _logger.LogError(tmp150, tmp151);
          else
            Console.Error.WriteLine(tmp151);
          var tmp152 = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("mergeMemberToCluster", TMessageType.Exception, seqid), cancellationToken);
          await tmp152.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

      public async global::System.Threading.Tasks.Task executeOnController_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var tmp153 = new InternalStructs.executeOnController_args();
        await tmp153.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var tmp154 = new InternalStructs.executeOnController_result();
        try
        {
          tmp154.Success = await _iAsync.executeOnController(tmp153.ClusterId, tmp153.Script, tmp153.Lang, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("executeOnController", TMessageType.Reply, seqid), cancellationToken);
          await tmp154.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception tmp155)
        {
          var tmp156 = $"Error occurred in {GetType().FullName}: {tmp155.Message}";
          if(_logger != null)
            _logger.LogError(tmp155, tmp156);
          else
            Console.Error.WriteLine(tmp156);
          var tmp157 = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("executeOnController", TMessageType.Exception, seqid), cancellationToken);
          await tmp157.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

    }

    public class InternalStructs
    {

      public partial class ping_args : TBase
      {

        public ping_args()
        {
        }

        public ping_args DeepCopy()
        {
          var tmp158 = new ping_args();
          return tmp158;
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
            var tmp159 = new TStruct("ping_args");
            await oprot.WriteStructBeginAsync(tmp159, cancellationToken);
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
          if (!(that is ping_args other)) return false;
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
          var tmp160 = new StringBuilder("ping_args(");
          tmp160.Append(')');
          return tmp160.ToString();
        }
      }


      public partial class ping_result : TBase
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

        public ping_result()
        {
        }

        public ping_result DeepCopy()
        {
          var tmp162 = new ping_result();
          if(__isset.success)
          {
            tmp162.Success = this.Success;
          }
          tmp162.__isset.success = this.__isset.success;
          return tmp162;
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
                case 0:
                  if (field.Type == TType.Bool)
                  {
                    Success = await iprot.ReadBoolAsync(cancellationToken);
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
            var tmp163 = new TStruct("ping_result");
            await oprot.WriteStructBeginAsync(tmp163, cancellationToken);
            var tmp164 = new TField();

            if(this.__isset.success)
            {
              tmp164.Name = "Success";
              tmp164.Type = TType.Bool;
              tmp164.ID = 0;
              await oprot.WriteFieldBeginAsync(tmp164, cancellationToken);
              await oprot.WriteBoolAsync(Success, cancellationToken);
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
          if (!(that is ping_result other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if(__isset.success)
            {
              hashcode = (hashcode * 397) + Success.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp165 = new StringBuilder("ping_result(");
          int tmp166 = 0;
          if(__isset.success)
          {
            if(0 < tmp166++) { tmp165.Append(", "); }
            tmp165.Append("Success: ");
            Success.ToString(tmp165);
          }
          tmp165.Append(')');
          return tmp165.ToString();
        }
      }


      public partial class clean_args : TBase
      {

        public clean_args()
        {
        }

        public clean_args DeepCopy()
        {
          var tmp167 = new clean_args();
          return tmp167;
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
            var tmp168 = new TStruct("clean_args");
            await oprot.WriteStructBeginAsync(tmp168, cancellationToken);
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
          if (!(that is clean_args other)) return false;
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
          var tmp169 = new StringBuilder("clean_args(");
          tmp169.Append(')');
          return tmp169.ToString();
        }
      }


      public partial class clean_result : TBase
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

        public clean_result()
        {
        }

        public clean_result DeepCopy()
        {
          var tmp171 = new clean_result();
          if(__isset.success)
          {
            tmp171.Success = this.Success;
          }
          tmp171.__isset.success = this.__isset.success;
          return tmp171;
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
                case 0:
                  if (field.Type == TType.Bool)
                  {
                    Success = await iprot.ReadBoolAsync(cancellationToken);
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
            var tmp172 = new TStruct("clean_result");
            await oprot.WriteStructBeginAsync(tmp172, cancellationToken);
            var tmp173 = new TField();

            if(this.__isset.success)
            {
              tmp173.Name = "Success";
              tmp173.Type = TType.Bool;
              tmp173.ID = 0;
              await oprot.WriteFieldBeginAsync(tmp173, cancellationToken);
              await oprot.WriteBoolAsync(Success, cancellationToken);
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
          if (!(that is clean_result other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if(__isset.success)
            {
              hashcode = (hashcode * 397) + Success.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp174 = new StringBuilder("clean_result(");
          int tmp175 = 0;
          if(__isset.success)
          {
            if(0 < tmp175++) { tmp174.Append(", "); }
            tmp174.Append("Success: ");
            Success.ToString(tmp174);
          }
          tmp174.Append(')');
          return tmp174.ToString();
        }
      }


      public partial class exit_args : TBase
      {

        public exit_args()
        {
        }

        public exit_args DeepCopy()
        {
          var tmp176 = new exit_args();
          return tmp176;
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
            var tmp177 = new TStruct("exit_args");
            await oprot.WriteStructBeginAsync(tmp177, cancellationToken);
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
          if (!(that is exit_args other)) return false;
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
          var tmp178 = new StringBuilder("exit_args(");
          tmp178.Append(')');
          return tmp178.ToString();
        }
      }


      public partial class exit_result : TBase
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

        public exit_result()
        {
        }

        public exit_result DeepCopy()
        {
          var tmp180 = new exit_result();
          if(__isset.success)
          {
            tmp180.Success = this.Success;
          }
          tmp180.__isset.success = this.__isset.success;
          return tmp180;
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
                case 0:
                  if (field.Type == TType.Bool)
                  {
                    Success = await iprot.ReadBoolAsync(cancellationToken);
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
            var tmp181 = new TStruct("exit_result");
            await oprot.WriteStructBeginAsync(tmp181, cancellationToken);
            var tmp182 = new TField();

            if(this.__isset.success)
            {
              tmp182.Name = "Success";
              tmp182.Type = TType.Bool;
              tmp182.ID = 0;
              await oprot.WriteFieldBeginAsync(tmp182, cancellationToken);
              await oprot.WriteBoolAsync(Success, cancellationToken);
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
          if (!(that is exit_result other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if(__isset.success)
            {
              hashcode = (hashcode * 397) + Success.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp183 = new StringBuilder("exit_result(");
          int tmp184 = 0;
          if(__isset.success)
          {
            if(0 < tmp184++) { tmp183.Append(", "); }
            tmp183.Append("Success: ");
            Success.ToString(tmp183);
          }
          tmp183.Append(')');
          return tmp183.ToString();
        }
      }


      public partial class createCluster_args : TBase
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

        public createCluster_args()
        {
        }

        public createCluster_args DeepCopy()
        {
          var tmp185 = new createCluster_args();
          if((HzVersion != null) && __isset.hzVersion)
          {
            tmp185.HzVersion = this.HzVersion;
          }
          tmp185.__isset.hzVersion = this.__isset.hzVersion;
          if((Xmlconfig != null) && __isset.xmlconfig)
          {
            tmp185.Xmlconfig = this.Xmlconfig;
          }
          tmp185.__isset.xmlconfig = this.__isset.xmlconfig;
          return tmp185;
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
                    HzVersion = await iprot.ReadStringAsync(cancellationToken);
                  }
                  else
                  {
                    await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                  }
                  break;
                case 2:
                  if (field.Type == TType.String)
                  {
                    Xmlconfig = await iprot.ReadStringAsync(cancellationToken);
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
            var tmp186 = new TStruct("createCluster_args");
            await oprot.WriteStructBeginAsync(tmp186, cancellationToken);
            var tmp187 = new TField();
            if((HzVersion != null) && __isset.hzVersion)
            {
              tmp187.Name = "hzVersion";
              tmp187.Type = TType.String;
              tmp187.ID = 1;
              await oprot.WriteFieldBeginAsync(tmp187, cancellationToken);
              await oprot.WriteStringAsync(HzVersion, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
            if((Xmlconfig != null) && __isset.xmlconfig)
            {
              tmp187.Name = "xmlconfig";
              tmp187.Type = TType.String;
              tmp187.ID = 2;
              await oprot.WriteFieldBeginAsync(tmp187, cancellationToken);
              await oprot.WriteStringAsync(Xmlconfig, cancellationToken);
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
          if (!(that is createCluster_args other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.hzVersion == other.__isset.hzVersion) && ((!__isset.hzVersion) || (System.Object.Equals(HzVersion, other.HzVersion))))
            && ((__isset.xmlconfig == other.__isset.xmlconfig) && ((!__isset.xmlconfig) || (System.Object.Equals(Xmlconfig, other.Xmlconfig))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((HzVersion != null) && __isset.hzVersion)
            {
              hashcode = (hashcode * 397) + HzVersion.GetHashCode();
            }
            if((Xmlconfig != null) && __isset.xmlconfig)
            {
              hashcode = (hashcode * 397) + Xmlconfig.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp188 = new StringBuilder("createCluster_args(");
          int tmp189 = 0;
          if((HzVersion != null) && __isset.hzVersion)
          {
            if(0 < tmp189++) { tmp188.Append(", "); }
            tmp188.Append("HzVersion: ");
            HzVersion.ToString(tmp188);
          }
          if((Xmlconfig != null) && __isset.xmlconfig)
          {
            if(0 < tmp189++) { tmp188.Append(", "); }
            tmp188.Append("Xmlconfig: ");
            Xmlconfig.ToString(tmp188);
          }
          tmp188.Append(')');
          return tmp188.ToString();
        }
      }


      public partial class createCluster_result : TBase
      {
        private global::Hazelcast.Testing.Remote.Cluster _success;
        private global::Hazelcast.Testing.Remote.ServerException _serverException;

        public global::Hazelcast.Testing.Remote.Cluster Success
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

        public global::Hazelcast.Testing.Remote.ServerException ServerException
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

        public createCluster_result()
        {
        }

        public createCluster_result DeepCopy()
        {
          var tmp190 = new createCluster_result();
          if((Success != null) && __isset.success)
          {
            tmp190.Success = (global::Hazelcast.Testing.Remote.Cluster)this.Success.DeepCopy();
          }
          tmp190.__isset.success = this.__isset.success;
          if((ServerException != null) && __isset.serverException)
          {
            tmp190.ServerException = (global::Hazelcast.Testing.Remote.ServerException)this.ServerException.DeepCopy();
          }
          tmp190.__isset.serverException = this.__isset.serverException;
          return tmp190;
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
                case 0:
                  if (field.Type == TType.Struct)
                  {
                    Success = new global::Hazelcast.Testing.Remote.Cluster();
                    await Success.ReadAsync(iprot, cancellationToken);
                  }
                  else
                  {
                    await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                  }
                  break;
                case 1:
                  if (field.Type == TType.Struct)
                  {
                    ServerException = new global::Hazelcast.Testing.Remote.ServerException();
                    await ServerException.ReadAsync(iprot, cancellationToken);
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
            var tmp191 = new TStruct("createCluster_result");
            await oprot.WriteStructBeginAsync(tmp191, cancellationToken);
            var tmp192 = new TField();

            if(this.__isset.success)
            {
              if (Success != null)
              {
                tmp192.Name = "Success";
                tmp192.Type = TType.Struct;
                tmp192.ID = 0;
                await oprot.WriteFieldBeginAsync(tmp192, cancellationToken);
                await Success.WriteAsync(oprot, cancellationToken);
                await oprot.WriteFieldEndAsync(cancellationToken);
              }
            }
            else if(this.__isset.serverException)
            {
              if (ServerException != null)
              {
                tmp192.Name = "ServerException";
                tmp192.Type = TType.Struct;
                tmp192.ID = 1;
                await oprot.WriteFieldBeginAsync(tmp192, cancellationToken);
                await ServerException.WriteAsync(oprot, cancellationToken);
                await oprot.WriteFieldEndAsync(cancellationToken);
              }
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
          if (!(that is createCluster_result other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))))
            && ((__isset.serverException == other.__isset.serverException) && ((!__isset.serverException) || (System.Object.Equals(ServerException, other.ServerException))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((Success != null) && __isset.success)
            {
              hashcode = (hashcode * 397) + Success.GetHashCode();
            }
            if((ServerException != null) && __isset.serverException)
            {
              hashcode = (hashcode * 397) + ServerException.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp193 = new StringBuilder("createCluster_result(");
          int tmp194 = 0;
          if((Success != null) && __isset.success)
          {
            if(0 < tmp194++) { tmp193.Append(", "); }
            tmp193.Append("Success: ");
            Success.ToString(tmp193);
          }
          if((ServerException != null) && __isset.serverException)
          {
            if(0 < tmp194++) { tmp193.Append(", "); }
            tmp193.Append("ServerException: ");
            ServerException.ToString(tmp193);
          }
          tmp193.Append(')');
          return tmp193.ToString();
        }
      }


      public partial class createClusterKeepClusterName_args : TBase
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

        public createClusterKeepClusterName_args()
        {
        }

        public createClusterKeepClusterName_args DeepCopy()
        {
          var tmp195 = new createClusterKeepClusterName_args();
          if((HzVersion != null) && __isset.hzVersion)
          {
            tmp195.HzVersion = this.HzVersion;
          }
          tmp195.__isset.hzVersion = this.__isset.hzVersion;
          if((Xmlconfig != null) && __isset.xmlconfig)
          {
            tmp195.Xmlconfig = this.Xmlconfig;
          }
          tmp195.__isset.xmlconfig = this.__isset.xmlconfig;
          return tmp195;
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
                    HzVersion = await iprot.ReadStringAsync(cancellationToken);
                  }
                  else
                  {
                    await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                  }
                  break;
                case 2:
                  if (field.Type == TType.String)
                  {
                    Xmlconfig = await iprot.ReadStringAsync(cancellationToken);
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
            var tmp196 = new TStruct("createClusterKeepClusterName_args");
            await oprot.WriteStructBeginAsync(tmp196, cancellationToken);
            var tmp197 = new TField();
            if((HzVersion != null) && __isset.hzVersion)
            {
              tmp197.Name = "hzVersion";
              tmp197.Type = TType.String;
              tmp197.ID = 1;
              await oprot.WriteFieldBeginAsync(tmp197, cancellationToken);
              await oprot.WriteStringAsync(HzVersion, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
            if((Xmlconfig != null) && __isset.xmlconfig)
            {
              tmp197.Name = "xmlconfig";
              tmp197.Type = TType.String;
              tmp197.ID = 2;
              await oprot.WriteFieldBeginAsync(tmp197, cancellationToken);
              await oprot.WriteStringAsync(Xmlconfig, cancellationToken);
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
          if (!(that is createClusterKeepClusterName_args other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.hzVersion == other.__isset.hzVersion) && ((!__isset.hzVersion) || (System.Object.Equals(HzVersion, other.HzVersion))))
            && ((__isset.xmlconfig == other.__isset.xmlconfig) && ((!__isset.xmlconfig) || (System.Object.Equals(Xmlconfig, other.Xmlconfig))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((HzVersion != null) && __isset.hzVersion)
            {
              hashcode = (hashcode * 397) + HzVersion.GetHashCode();
            }
            if((Xmlconfig != null) && __isset.xmlconfig)
            {
              hashcode = (hashcode * 397) + Xmlconfig.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp198 = new StringBuilder("createClusterKeepClusterName_args(");
          int tmp199 = 0;
          if((HzVersion != null) && __isset.hzVersion)
          {
            if(0 < tmp199++) { tmp198.Append(", "); }
            tmp198.Append("HzVersion: ");
            HzVersion.ToString(tmp198);
          }
          if((Xmlconfig != null) && __isset.xmlconfig)
          {
            if(0 < tmp199++) { tmp198.Append(", "); }
            tmp198.Append("Xmlconfig: ");
            Xmlconfig.ToString(tmp198);
          }
          tmp198.Append(')');
          return tmp198.ToString();
        }
      }


      public partial class createClusterKeepClusterName_result : TBase
      {
        private global::Hazelcast.Testing.Remote.Cluster _success;
        private global::Hazelcast.Testing.Remote.ServerException _serverException;

        public global::Hazelcast.Testing.Remote.Cluster Success
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

        public global::Hazelcast.Testing.Remote.ServerException ServerException
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

        public createClusterKeepClusterName_result()
        {
        }

        public createClusterKeepClusterName_result DeepCopy()
        {
          var tmp200 = new createClusterKeepClusterName_result();
          if((Success != null) && __isset.success)
          {
            tmp200.Success = (global::Hazelcast.Testing.Remote.Cluster)this.Success.DeepCopy();
          }
          tmp200.__isset.success = this.__isset.success;
          if((ServerException != null) && __isset.serverException)
          {
            tmp200.ServerException = (global::Hazelcast.Testing.Remote.ServerException)this.ServerException.DeepCopy();
          }
          tmp200.__isset.serverException = this.__isset.serverException;
          return tmp200;
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
                case 0:
                  if (field.Type == TType.Struct)
                  {
                    Success = new global::Hazelcast.Testing.Remote.Cluster();
                    await Success.ReadAsync(iprot, cancellationToken);
                  }
                  else
                  {
                    await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                  }
                  break;
                case 1:
                  if (field.Type == TType.Struct)
                  {
                    ServerException = new global::Hazelcast.Testing.Remote.ServerException();
                    await ServerException.ReadAsync(iprot, cancellationToken);
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
            var tmp201 = new TStruct("createClusterKeepClusterName_result");
            await oprot.WriteStructBeginAsync(tmp201, cancellationToken);
            var tmp202 = new TField();

            if(this.__isset.success)
            {
              if (Success != null)
              {
                tmp202.Name = "Success";
                tmp202.Type = TType.Struct;
                tmp202.ID = 0;
                await oprot.WriteFieldBeginAsync(tmp202, cancellationToken);
                await Success.WriteAsync(oprot, cancellationToken);
                await oprot.WriteFieldEndAsync(cancellationToken);
              }
            }
            else if(this.__isset.serverException)
            {
              if (ServerException != null)
              {
                tmp202.Name = "ServerException";
                tmp202.Type = TType.Struct;
                tmp202.ID = 1;
                await oprot.WriteFieldBeginAsync(tmp202, cancellationToken);
                await ServerException.WriteAsync(oprot, cancellationToken);
                await oprot.WriteFieldEndAsync(cancellationToken);
              }
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
          if (!(that is createClusterKeepClusterName_result other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))))
            && ((__isset.serverException == other.__isset.serverException) && ((!__isset.serverException) || (System.Object.Equals(ServerException, other.ServerException))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((Success != null) && __isset.success)
            {
              hashcode = (hashcode * 397) + Success.GetHashCode();
            }
            if((ServerException != null) && __isset.serverException)
            {
              hashcode = (hashcode * 397) + ServerException.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp203 = new StringBuilder("createClusterKeepClusterName_result(");
          int tmp204 = 0;
          if((Success != null) && __isset.success)
          {
            if(0 < tmp204++) { tmp203.Append(", "); }
            tmp203.Append("Success: ");
            Success.ToString(tmp203);
          }
          if((ServerException != null) && __isset.serverException)
          {
            if(0 < tmp204++) { tmp203.Append(", "); }
            tmp203.Append("ServerException: ");
            ServerException.ToString(tmp203);
          }
          tmp203.Append(')');
          return tmp203.ToString();
        }
      }


      public partial class startMember_args : TBase
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

        public startMember_args()
        {
        }

        public startMember_args DeepCopy()
        {
          var tmp205 = new startMember_args();
          if((ClusterId != null) && __isset.clusterId)
          {
            tmp205.ClusterId = this.ClusterId;
          }
          tmp205.__isset.clusterId = this.__isset.clusterId;
          return tmp205;
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
                    ClusterId = await iprot.ReadStringAsync(cancellationToken);
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
            var tmp206 = new TStruct("startMember_args");
            await oprot.WriteStructBeginAsync(tmp206, cancellationToken);
            var tmp207 = new TField();
            if((ClusterId != null) && __isset.clusterId)
            {
              tmp207.Name = "clusterId";
              tmp207.Type = TType.String;
              tmp207.ID = 1;
              await oprot.WriteFieldBeginAsync(tmp207, cancellationToken);
              await oprot.WriteStringAsync(ClusterId, cancellationToken);
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
          if (!(that is startMember_args other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((ClusterId != null) && __isset.clusterId)
            {
              hashcode = (hashcode * 397) + ClusterId.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp208 = new StringBuilder("startMember_args(");
          int tmp209 = 0;
          if((ClusterId != null) && __isset.clusterId)
          {
            if(0 < tmp209++) { tmp208.Append(", "); }
            tmp208.Append("ClusterId: ");
            ClusterId.ToString(tmp208);
          }
          tmp208.Append(')');
          return tmp208.ToString();
        }
      }


      public partial class startMember_result : TBase
      {
        private global::Hazelcast.Testing.Remote.Member _success;
        private global::Hazelcast.Testing.Remote.ServerException _serverException;

        public global::Hazelcast.Testing.Remote.Member Success
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

        public global::Hazelcast.Testing.Remote.ServerException ServerException
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

        public startMember_result()
        {
        }

        public startMember_result DeepCopy()
        {
          var tmp210 = new startMember_result();
          if((Success != null) && __isset.success)
          {
            tmp210.Success = (global::Hazelcast.Testing.Remote.Member)this.Success.DeepCopy();
          }
          tmp210.__isset.success = this.__isset.success;
          if((ServerException != null) && __isset.serverException)
          {
            tmp210.ServerException = (global::Hazelcast.Testing.Remote.ServerException)this.ServerException.DeepCopy();
          }
          tmp210.__isset.serverException = this.__isset.serverException;
          return tmp210;
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
                case 0:
                  if (field.Type == TType.Struct)
                  {
                    Success = new global::Hazelcast.Testing.Remote.Member();
                    await Success.ReadAsync(iprot, cancellationToken);
                  }
                  else
                  {
                    await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                  }
                  break;
                case 1:
                  if (field.Type == TType.Struct)
                  {
                    ServerException = new global::Hazelcast.Testing.Remote.ServerException();
                    await ServerException.ReadAsync(iprot, cancellationToken);
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
            var tmp211 = new TStruct("startMember_result");
            await oprot.WriteStructBeginAsync(tmp211, cancellationToken);
            var tmp212 = new TField();

            if(this.__isset.success)
            {
              if (Success != null)
              {
                tmp212.Name = "Success";
                tmp212.Type = TType.Struct;
                tmp212.ID = 0;
                await oprot.WriteFieldBeginAsync(tmp212, cancellationToken);
                await Success.WriteAsync(oprot, cancellationToken);
                await oprot.WriteFieldEndAsync(cancellationToken);
              }
            }
            else if(this.__isset.serverException)
            {
              if (ServerException != null)
              {
                tmp212.Name = "ServerException";
                tmp212.Type = TType.Struct;
                tmp212.ID = 1;
                await oprot.WriteFieldBeginAsync(tmp212, cancellationToken);
                await ServerException.WriteAsync(oprot, cancellationToken);
                await oprot.WriteFieldEndAsync(cancellationToken);
              }
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
          if (!(that is startMember_result other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))))
            && ((__isset.serverException == other.__isset.serverException) && ((!__isset.serverException) || (System.Object.Equals(ServerException, other.ServerException))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((Success != null) && __isset.success)
            {
              hashcode = (hashcode * 397) + Success.GetHashCode();
            }
            if((ServerException != null) && __isset.serverException)
            {
              hashcode = (hashcode * 397) + ServerException.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp213 = new StringBuilder("startMember_result(");
          int tmp214 = 0;
          if((Success != null) && __isset.success)
          {
            if(0 < tmp214++) { tmp213.Append(", "); }
            tmp213.Append("Success: ");
            Success.ToString(tmp213);
          }
          if((ServerException != null) && __isset.serverException)
          {
            if(0 < tmp214++) { tmp213.Append(", "); }
            tmp213.Append("ServerException: ");
            ServerException.ToString(tmp213);
          }
          tmp213.Append(')');
          return tmp213.ToString();
        }
      }


      public partial class shutdownMember_args : TBase
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

        public shutdownMember_args()
        {
        }

        public shutdownMember_args DeepCopy()
        {
          var tmp215 = new shutdownMember_args();
          if((ClusterId != null) && __isset.clusterId)
          {
            tmp215.ClusterId = this.ClusterId;
          }
          tmp215.__isset.clusterId = this.__isset.clusterId;
          if((MemberId != null) && __isset.memberId)
          {
            tmp215.MemberId = this.MemberId;
          }
          tmp215.__isset.memberId = this.__isset.memberId;
          return tmp215;
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
                    ClusterId = await iprot.ReadStringAsync(cancellationToken);
                  }
                  else
                  {
                    await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                  }
                  break;
                case 2:
                  if (field.Type == TType.String)
                  {
                    MemberId = await iprot.ReadStringAsync(cancellationToken);
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
            var tmp216 = new TStruct("shutdownMember_args");
            await oprot.WriteStructBeginAsync(tmp216, cancellationToken);
            var tmp217 = new TField();
            if((ClusterId != null) && __isset.clusterId)
            {
              tmp217.Name = "clusterId";
              tmp217.Type = TType.String;
              tmp217.ID = 1;
              await oprot.WriteFieldBeginAsync(tmp217, cancellationToken);
              await oprot.WriteStringAsync(ClusterId, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
            if((MemberId != null) && __isset.memberId)
            {
              tmp217.Name = "memberId";
              tmp217.Type = TType.String;
              tmp217.ID = 2;
              await oprot.WriteFieldBeginAsync(tmp217, cancellationToken);
              await oprot.WriteStringAsync(MemberId, cancellationToken);
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
          if (!(that is shutdownMember_args other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))))
            && ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (System.Object.Equals(MemberId, other.MemberId))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((ClusterId != null) && __isset.clusterId)
            {
              hashcode = (hashcode * 397) + ClusterId.GetHashCode();
            }
            if((MemberId != null) && __isset.memberId)
            {
              hashcode = (hashcode * 397) + MemberId.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp218 = new StringBuilder("shutdownMember_args(");
          int tmp219 = 0;
          if((ClusterId != null) && __isset.clusterId)
          {
            if(0 < tmp219++) { tmp218.Append(", "); }
            tmp218.Append("ClusterId: ");
            ClusterId.ToString(tmp218);
          }
          if((MemberId != null) && __isset.memberId)
          {
            if(0 < tmp219++) { tmp218.Append(", "); }
            tmp218.Append("MemberId: ");
            MemberId.ToString(tmp218);
          }
          tmp218.Append(')');
          return tmp218.ToString();
        }
      }


      public partial class shutdownMember_result : TBase
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

        public shutdownMember_result()
        {
        }

        public shutdownMember_result DeepCopy()
        {
          var tmp220 = new shutdownMember_result();
          if(__isset.success)
          {
            tmp220.Success = this.Success;
          }
          tmp220.__isset.success = this.__isset.success;
          return tmp220;
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
                case 0:
                  if (field.Type == TType.Bool)
                  {
                    Success = await iprot.ReadBoolAsync(cancellationToken);
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
            var tmp221 = new TStruct("shutdownMember_result");
            await oprot.WriteStructBeginAsync(tmp221, cancellationToken);
            var tmp222 = new TField();

            if(this.__isset.success)
            {
              tmp222.Name = "Success";
              tmp222.Type = TType.Bool;
              tmp222.ID = 0;
              await oprot.WriteFieldBeginAsync(tmp222, cancellationToken);
              await oprot.WriteBoolAsync(Success, cancellationToken);
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
          if (!(that is shutdownMember_result other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if(__isset.success)
            {
              hashcode = (hashcode * 397) + Success.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp223 = new StringBuilder("shutdownMember_result(");
          int tmp224 = 0;
          if(__isset.success)
          {
            if(0 < tmp224++) { tmp223.Append(", "); }
            tmp223.Append("Success: ");
            Success.ToString(tmp223);
          }
          tmp223.Append(')');
          return tmp223.ToString();
        }
      }


      public partial class terminateMember_args : TBase
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

        public terminateMember_args()
        {
        }

        public terminateMember_args DeepCopy()
        {
          var tmp225 = new terminateMember_args();
          if((ClusterId != null) && __isset.clusterId)
          {
            tmp225.ClusterId = this.ClusterId;
          }
          tmp225.__isset.clusterId = this.__isset.clusterId;
          if((MemberId != null) && __isset.memberId)
          {
            tmp225.MemberId = this.MemberId;
          }
          tmp225.__isset.memberId = this.__isset.memberId;
          return tmp225;
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
                    ClusterId = await iprot.ReadStringAsync(cancellationToken);
                  }
                  else
                  {
                    await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                  }
                  break;
                case 2:
                  if (field.Type == TType.String)
                  {
                    MemberId = await iprot.ReadStringAsync(cancellationToken);
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
            var tmp226 = new TStruct("terminateMember_args");
            await oprot.WriteStructBeginAsync(tmp226, cancellationToken);
            var tmp227 = new TField();
            if((ClusterId != null) && __isset.clusterId)
            {
              tmp227.Name = "clusterId";
              tmp227.Type = TType.String;
              tmp227.ID = 1;
              await oprot.WriteFieldBeginAsync(tmp227, cancellationToken);
              await oprot.WriteStringAsync(ClusterId, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
            if((MemberId != null) && __isset.memberId)
            {
              tmp227.Name = "memberId";
              tmp227.Type = TType.String;
              tmp227.ID = 2;
              await oprot.WriteFieldBeginAsync(tmp227, cancellationToken);
              await oprot.WriteStringAsync(MemberId, cancellationToken);
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
          if (!(that is terminateMember_args other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))))
            && ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (System.Object.Equals(MemberId, other.MemberId))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((ClusterId != null) && __isset.clusterId)
            {
              hashcode = (hashcode * 397) + ClusterId.GetHashCode();
            }
            if((MemberId != null) && __isset.memberId)
            {
              hashcode = (hashcode * 397) + MemberId.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp228 = new StringBuilder("terminateMember_args(");
          int tmp229 = 0;
          if((ClusterId != null) && __isset.clusterId)
          {
            if(0 < tmp229++) { tmp228.Append(", "); }
            tmp228.Append("ClusterId: ");
            ClusterId.ToString(tmp228);
          }
          if((MemberId != null) && __isset.memberId)
          {
            if(0 < tmp229++) { tmp228.Append(", "); }
            tmp228.Append("MemberId: ");
            MemberId.ToString(tmp228);
          }
          tmp228.Append(')');
          return tmp228.ToString();
        }
      }


      public partial class terminateMember_result : TBase
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

        public terminateMember_result()
        {
        }

        public terminateMember_result DeepCopy()
        {
          var tmp230 = new terminateMember_result();
          if(__isset.success)
          {
            tmp230.Success = this.Success;
          }
          tmp230.__isset.success = this.__isset.success;
          return tmp230;
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
                case 0:
                  if (field.Type == TType.Bool)
                  {
                    Success = await iprot.ReadBoolAsync(cancellationToken);
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
            var tmp231 = new TStruct("terminateMember_result");
            await oprot.WriteStructBeginAsync(tmp231, cancellationToken);
            var tmp232 = new TField();

            if(this.__isset.success)
            {
              tmp232.Name = "Success";
              tmp232.Type = TType.Bool;
              tmp232.ID = 0;
              await oprot.WriteFieldBeginAsync(tmp232, cancellationToken);
              await oprot.WriteBoolAsync(Success, cancellationToken);
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
          if (!(that is terminateMember_result other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if(__isset.success)
            {
              hashcode = (hashcode * 397) + Success.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp233 = new StringBuilder("terminateMember_result(");
          int tmp234 = 0;
          if(__isset.success)
          {
            if(0 < tmp234++) { tmp233.Append(", "); }
            tmp233.Append("Success: ");
            Success.ToString(tmp233);
          }
          tmp233.Append(')');
          return tmp233.ToString();
        }
      }


      public partial class suspendMember_args : TBase
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

        public suspendMember_args()
        {
        }

        public suspendMember_args DeepCopy()
        {
          var tmp235 = new suspendMember_args();
          if((ClusterId != null) && __isset.clusterId)
          {
            tmp235.ClusterId = this.ClusterId;
          }
          tmp235.__isset.clusterId = this.__isset.clusterId;
          if((MemberId != null) && __isset.memberId)
          {
            tmp235.MemberId = this.MemberId;
          }
          tmp235.__isset.memberId = this.__isset.memberId;
          return tmp235;
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
                    ClusterId = await iprot.ReadStringAsync(cancellationToken);
                  }
                  else
                  {
                    await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                  }
                  break;
                case 2:
                  if (field.Type == TType.String)
                  {
                    MemberId = await iprot.ReadStringAsync(cancellationToken);
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
            var tmp236 = new TStruct("suspendMember_args");
            await oprot.WriteStructBeginAsync(tmp236, cancellationToken);
            var tmp237 = new TField();
            if((ClusterId != null) && __isset.clusterId)
            {
              tmp237.Name = "clusterId";
              tmp237.Type = TType.String;
              tmp237.ID = 1;
              await oprot.WriteFieldBeginAsync(tmp237, cancellationToken);
              await oprot.WriteStringAsync(ClusterId, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
            if((MemberId != null) && __isset.memberId)
            {
              tmp237.Name = "memberId";
              tmp237.Type = TType.String;
              tmp237.ID = 2;
              await oprot.WriteFieldBeginAsync(tmp237, cancellationToken);
              await oprot.WriteStringAsync(MemberId, cancellationToken);
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
          if (!(that is suspendMember_args other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))))
            && ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (System.Object.Equals(MemberId, other.MemberId))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((ClusterId != null) && __isset.clusterId)
            {
              hashcode = (hashcode * 397) + ClusterId.GetHashCode();
            }
            if((MemberId != null) && __isset.memberId)
            {
              hashcode = (hashcode * 397) + MemberId.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp238 = new StringBuilder("suspendMember_args(");
          int tmp239 = 0;
          if((ClusterId != null) && __isset.clusterId)
          {
            if(0 < tmp239++) { tmp238.Append(", "); }
            tmp238.Append("ClusterId: ");
            ClusterId.ToString(tmp238);
          }
          if((MemberId != null) && __isset.memberId)
          {
            if(0 < tmp239++) { tmp238.Append(", "); }
            tmp238.Append("MemberId: ");
            MemberId.ToString(tmp238);
          }
          tmp238.Append(')');
          return tmp238.ToString();
        }
      }


      public partial class suspendMember_result : TBase
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

        public suspendMember_result()
        {
        }

        public suspendMember_result DeepCopy()
        {
          var tmp240 = new suspendMember_result();
          if(__isset.success)
          {
            tmp240.Success = this.Success;
          }
          tmp240.__isset.success = this.__isset.success;
          return tmp240;
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
                case 0:
                  if (field.Type == TType.Bool)
                  {
                    Success = await iprot.ReadBoolAsync(cancellationToken);
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
            var tmp241 = new TStruct("suspendMember_result");
            await oprot.WriteStructBeginAsync(tmp241, cancellationToken);
            var tmp242 = new TField();

            if(this.__isset.success)
            {
              tmp242.Name = "Success";
              tmp242.Type = TType.Bool;
              tmp242.ID = 0;
              await oprot.WriteFieldBeginAsync(tmp242, cancellationToken);
              await oprot.WriteBoolAsync(Success, cancellationToken);
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
          if (!(that is suspendMember_result other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if(__isset.success)
            {
              hashcode = (hashcode * 397) + Success.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp243 = new StringBuilder("suspendMember_result(");
          int tmp244 = 0;
          if(__isset.success)
          {
            if(0 < tmp244++) { tmp243.Append(", "); }
            tmp243.Append("Success: ");
            Success.ToString(tmp243);
          }
          tmp243.Append(')');
          return tmp243.ToString();
        }
      }


      public partial class resumeMember_args : TBase
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

        public resumeMember_args()
        {
        }

        public resumeMember_args DeepCopy()
        {
          var tmp245 = new resumeMember_args();
          if((ClusterId != null) && __isset.clusterId)
          {
            tmp245.ClusterId = this.ClusterId;
          }
          tmp245.__isset.clusterId = this.__isset.clusterId;
          if((MemberId != null) && __isset.memberId)
          {
            tmp245.MemberId = this.MemberId;
          }
          tmp245.__isset.memberId = this.__isset.memberId;
          return tmp245;
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
                    ClusterId = await iprot.ReadStringAsync(cancellationToken);
                  }
                  else
                  {
                    await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                  }
                  break;
                case 2:
                  if (field.Type == TType.String)
                  {
                    MemberId = await iprot.ReadStringAsync(cancellationToken);
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
            var tmp246 = new TStruct("resumeMember_args");
            await oprot.WriteStructBeginAsync(tmp246, cancellationToken);
            var tmp247 = new TField();
            if((ClusterId != null) && __isset.clusterId)
            {
              tmp247.Name = "clusterId";
              tmp247.Type = TType.String;
              tmp247.ID = 1;
              await oprot.WriteFieldBeginAsync(tmp247, cancellationToken);
              await oprot.WriteStringAsync(ClusterId, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
            if((MemberId != null) && __isset.memberId)
            {
              tmp247.Name = "memberId";
              tmp247.Type = TType.String;
              tmp247.ID = 2;
              await oprot.WriteFieldBeginAsync(tmp247, cancellationToken);
              await oprot.WriteStringAsync(MemberId, cancellationToken);
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
          if (!(that is resumeMember_args other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))))
            && ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (System.Object.Equals(MemberId, other.MemberId))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((ClusterId != null) && __isset.clusterId)
            {
              hashcode = (hashcode * 397) + ClusterId.GetHashCode();
            }
            if((MemberId != null) && __isset.memberId)
            {
              hashcode = (hashcode * 397) + MemberId.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp248 = new StringBuilder("resumeMember_args(");
          int tmp249 = 0;
          if((ClusterId != null) && __isset.clusterId)
          {
            if(0 < tmp249++) { tmp248.Append(", "); }
            tmp248.Append("ClusterId: ");
            ClusterId.ToString(tmp248);
          }
          if((MemberId != null) && __isset.memberId)
          {
            if(0 < tmp249++) { tmp248.Append(", "); }
            tmp248.Append("MemberId: ");
            MemberId.ToString(tmp248);
          }
          tmp248.Append(')');
          return tmp248.ToString();
        }
      }


      public partial class resumeMember_result : TBase
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

        public resumeMember_result()
        {
        }

        public resumeMember_result DeepCopy()
        {
          var tmp250 = new resumeMember_result();
          if(__isset.success)
          {
            tmp250.Success = this.Success;
          }
          tmp250.__isset.success = this.__isset.success;
          return tmp250;
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
                case 0:
                  if (field.Type == TType.Bool)
                  {
                    Success = await iprot.ReadBoolAsync(cancellationToken);
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
            var tmp251 = new TStruct("resumeMember_result");
            await oprot.WriteStructBeginAsync(tmp251, cancellationToken);
            var tmp252 = new TField();

            if(this.__isset.success)
            {
              tmp252.Name = "Success";
              tmp252.Type = TType.Bool;
              tmp252.ID = 0;
              await oprot.WriteFieldBeginAsync(tmp252, cancellationToken);
              await oprot.WriteBoolAsync(Success, cancellationToken);
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
          if (!(that is resumeMember_result other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if(__isset.success)
            {
              hashcode = (hashcode * 397) + Success.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp253 = new StringBuilder("resumeMember_result(");
          int tmp254 = 0;
          if(__isset.success)
          {
            if(0 < tmp254++) { tmp253.Append(", "); }
            tmp253.Append("Success: ");
            Success.ToString(tmp253);
          }
          tmp253.Append(')');
          return tmp253.ToString();
        }
      }


      public partial class shutdownCluster_args : TBase
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

        public shutdownCluster_args()
        {
        }

        public shutdownCluster_args DeepCopy()
        {
          var tmp255 = new shutdownCluster_args();
          if((ClusterId != null) && __isset.clusterId)
          {
            tmp255.ClusterId = this.ClusterId;
          }
          tmp255.__isset.clusterId = this.__isset.clusterId;
          return tmp255;
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
                    ClusterId = await iprot.ReadStringAsync(cancellationToken);
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
            var tmp256 = new TStruct("shutdownCluster_args");
            await oprot.WriteStructBeginAsync(tmp256, cancellationToken);
            var tmp257 = new TField();
            if((ClusterId != null) && __isset.clusterId)
            {
              tmp257.Name = "clusterId";
              tmp257.Type = TType.String;
              tmp257.ID = 1;
              await oprot.WriteFieldBeginAsync(tmp257, cancellationToken);
              await oprot.WriteStringAsync(ClusterId, cancellationToken);
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
          if (!(that is shutdownCluster_args other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((ClusterId != null) && __isset.clusterId)
            {
              hashcode = (hashcode * 397) + ClusterId.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp258 = new StringBuilder("shutdownCluster_args(");
          int tmp259 = 0;
          if((ClusterId != null) && __isset.clusterId)
          {
            if(0 < tmp259++) { tmp258.Append(", "); }
            tmp258.Append("ClusterId: ");
            ClusterId.ToString(tmp258);
          }
          tmp258.Append(')');
          return tmp258.ToString();
        }
      }


      public partial class shutdownCluster_result : TBase
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

        public shutdownCluster_result()
        {
        }

        public shutdownCluster_result DeepCopy()
        {
          var tmp260 = new shutdownCluster_result();
          if(__isset.success)
          {
            tmp260.Success = this.Success;
          }
          tmp260.__isset.success = this.__isset.success;
          return tmp260;
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
                case 0:
                  if (field.Type == TType.Bool)
                  {
                    Success = await iprot.ReadBoolAsync(cancellationToken);
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
            var tmp261 = new TStruct("shutdownCluster_result");
            await oprot.WriteStructBeginAsync(tmp261, cancellationToken);
            var tmp262 = new TField();

            if(this.__isset.success)
            {
              tmp262.Name = "Success";
              tmp262.Type = TType.Bool;
              tmp262.ID = 0;
              await oprot.WriteFieldBeginAsync(tmp262, cancellationToken);
              await oprot.WriteBoolAsync(Success, cancellationToken);
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
          if (!(that is shutdownCluster_result other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if(__isset.success)
            {
              hashcode = (hashcode * 397) + Success.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp263 = new StringBuilder("shutdownCluster_result(");
          int tmp264 = 0;
          if(__isset.success)
          {
            if(0 < tmp264++) { tmp263.Append(", "); }
            tmp263.Append("Success: ");
            Success.ToString(tmp263);
          }
          tmp263.Append(')');
          return tmp263.ToString();
        }
      }


      public partial class terminateCluster_args : TBase
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

        public terminateCluster_args()
        {
        }

        public terminateCluster_args DeepCopy()
        {
          var tmp265 = new terminateCluster_args();
          if((ClusterId != null) && __isset.clusterId)
          {
            tmp265.ClusterId = this.ClusterId;
          }
          tmp265.__isset.clusterId = this.__isset.clusterId;
          return tmp265;
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
                    ClusterId = await iprot.ReadStringAsync(cancellationToken);
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
            var tmp266 = new TStruct("terminateCluster_args");
            await oprot.WriteStructBeginAsync(tmp266, cancellationToken);
            var tmp267 = new TField();
            if((ClusterId != null) && __isset.clusterId)
            {
              tmp267.Name = "clusterId";
              tmp267.Type = TType.String;
              tmp267.ID = 1;
              await oprot.WriteFieldBeginAsync(tmp267, cancellationToken);
              await oprot.WriteStringAsync(ClusterId, cancellationToken);
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
          if (!(that is terminateCluster_args other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((ClusterId != null) && __isset.clusterId)
            {
              hashcode = (hashcode * 397) + ClusterId.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp268 = new StringBuilder("terminateCluster_args(");
          int tmp269 = 0;
          if((ClusterId != null) && __isset.clusterId)
          {
            if(0 < tmp269++) { tmp268.Append(", "); }
            tmp268.Append("ClusterId: ");
            ClusterId.ToString(tmp268);
          }
          tmp268.Append(')');
          return tmp268.ToString();
        }
      }


      public partial class terminateCluster_result : TBase
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

        public terminateCluster_result()
        {
        }

        public terminateCluster_result DeepCopy()
        {
          var tmp270 = new terminateCluster_result();
          if(__isset.success)
          {
            tmp270.Success = this.Success;
          }
          tmp270.__isset.success = this.__isset.success;
          return tmp270;
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
                case 0:
                  if (field.Type == TType.Bool)
                  {
                    Success = await iprot.ReadBoolAsync(cancellationToken);
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
            var tmp271 = new TStruct("terminateCluster_result");
            await oprot.WriteStructBeginAsync(tmp271, cancellationToken);
            var tmp272 = new TField();

            if(this.__isset.success)
            {
              tmp272.Name = "Success";
              tmp272.Type = TType.Bool;
              tmp272.ID = 0;
              await oprot.WriteFieldBeginAsync(tmp272, cancellationToken);
              await oprot.WriteBoolAsync(Success, cancellationToken);
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
          if (!(that is terminateCluster_result other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if(__isset.success)
            {
              hashcode = (hashcode * 397) + Success.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp273 = new StringBuilder("terminateCluster_result(");
          int tmp274 = 0;
          if(__isset.success)
          {
            if(0 < tmp274++) { tmp273.Append(", "); }
            tmp273.Append("Success: ");
            Success.ToString(tmp273);
          }
          tmp273.Append(')');
          return tmp273.ToString();
        }
      }


      public partial class splitMemberFromCluster_args : TBase
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

        public splitMemberFromCluster_args()
        {
        }

        public splitMemberFromCluster_args DeepCopy()
        {
          var tmp275 = new splitMemberFromCluster_args();
          if((MemberId != null) && __isset.memberId)
          {
            tmp275.MemberId = this.MemberId;
          }
          tmp275.__isset.memberId = this.__isset.memberId;
          return tmp275;
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
                    MemberId = await iprot.ReadStringAsync(cancellationToken);
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
            var tmp276 = new TStruct("splitMemberFromCluster_args");
            await oprot.WriteStructBeginAsync(tmp276, cancellationToken);
            var tmp277 = new TField();
            if((MemberId != null) && __isset.memberId)
            {
              tmp277.Name = "memberId";
              tmp277.Type = TType.String;
              tmp277.ID = 1;
              await oprot.WriteFieldBeginAsync(tmp277, cancellationToken);
              await oprot.WriteStringAsync(MemberId, cancellationToken);
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
          if (!(that is splitMemberFromCluster_args other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (System.Object.Equals(MemberId, other.MemberId))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((MemberId != null) && __isset.memberId)
            {
              hashcode = (hashcode * 397) + MemberId.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp278 = new StringBuilder("splitMemberFromCluster_args(");
          int tmp279 = 0;
          if((MemberId != null) && __isset.memberId)
          {
            if(0 < tmp279++) { tmp278.Append(", "); }
            tmp278.Append("MemberId: ");
            MemberId.ToString(tmp278);
          }
          tmp278.Append(')');
          return tmp278.ToString();
        }
      }


      public partial class splitMemberFromCluster_result : TBase
      {
        private global::Hazelcast.Testing.Remote.Cluster _success;

        public global::Hazelcast.Testing.Remote.Cluster Success
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

        public splitMemberFromCluster_result()
        {
        }

        public splitMemberFromCluster_result DeepCopy()
        {
          var tmp280 = new splitMemberFromCluster_result();
          if((Success != null) && __isset.success)
          {
            tmp280.Success = (global::Hazelcast.Testing.Remote.Cluster)this.Success.DeepCopy();
          }
          tmp280.__isset.success = this.__isset.success;
          return tmp280;
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
                case 0:
                  if (field.Type == TType.Struct)
                  {
                    Success = new global::Hazelcast.Testing.Remote.Cluster();
                    await Success.ReadAsync(iprot, cancellationToken);
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
            var tmp281 = new TStruct("splitMemberFromCluster_result");
            await oprot.WriteStructBeginAsync(tmp281, cancellationToken);
            var tmp282 = new TField();

            if(this.__isset.success)
            {
              if (Success != null)
              {
                tmp282.Name = "Success";
                tmp282.Type = TType.Struct;
                tmp282.ID = 0;
                await oprot.WriteFieldBeginAsync(tmp282, cancellationToken);
                await Success.WriteAsync(oprot, cancellationToken);
                await oprot.WriteFieldEndAsync(cancellationToken);
              }
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
          if (!(that is splitMemberFromCluster_result other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((Success != null) && __isset.success)
            {
              hashcode = (hashcode * 397) + Success.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp283 = new StringBuilder("splitMemberFromCluster_result(");
          int tmp284 = 0;
          if((Success != null) && __isset.success)
          {
            if(0 < tmp284++) { tmp283.Append(", "); }
            tmp283.Append("Success: ");
            Success.ToString(tmp283);
          }
          tmp283.Append(')');
          return tmp283.ToString();
        }
      }


      public partial class mergeMemberToCluster_args : TBase
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

        public mergeMemberToCluster_args()
        {
        }

        public mergeMemberToCluster_args DeepCopy()
        {
          var tmp285 = new mergeMemberToCluster_args();
          if((ClusterId != null) && __isset.clusterId)
          {
            tmp285.ClusterId = this.ClusterId;
          }
          tmp285.__isset.clusterId = this.__isset.clusterId;
          if((MemberId != null) && __isset.memberId)
          {
            tmp285.MemberId = this.MemberId;
          }
          tmp285.__isset.memberId = this.__isset.memberId;
          return tmp285;
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
                    ClusterId = await iprot.ReadStringAsync(cancellationToken);
                  }
                  else
                  {
                    await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                  }
                  break;
                case 2:
                  if (field.Type == TType.String)
                  {
                    MemberId = await iprot.ReadStringAsync(cancellationToken);
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
            var tmp286 = new TStruct("mergeMemberToCluster_args");
            await oprot.WriteStructBeginAsync(tmp286, cancellationToken);
            var tmp287 = new TField();
            if((ClusterId != null) && __isset.clusterId)
            {
              tmp287.Name = "clusterId";
              tmp287.Type = TType.String;
              tmp287.ID = 1;
              await oprot.WriteFieldBeginAsync(tmp287, cancellationToken);
              await oprot.WriteStringAsync(ClusterId, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
            if((MemberId != null) && __isset.memberId)
            {
              tmp287.Name = "memberId";
              tmp287.Type = TType.String;
              tmp287.ID = 2;
              await oprot.WriteFieldBeginAsync(tmp287, cancellationToken);
              await oprot.WriteStringAsync(MemberId, cancellationToken);
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
          if (!(that is mergeMemberToCluster_args other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))))
            && ((__isset.memberId == other.__isset.memberId) && ((!__isset.memberId) || (System.Object.Equals(MemberId, other.MemberId))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((ClusterId != null) && __isset.clusterId)
            {
              hashcode = (hashcode * 397) + ClusterId.GetHashCode();
            }
            if((MemberId != null) && __isset.memberId)
            {
              hashcode = (hashcode * 397) + MemberId.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp288 = new StringBuilder("mergeMemberToCluster_args(");
          int tmp289 = 0;
          if((ClusterId != null) && __isset.clusterId)
          {
            if(0 < tmp289++) { tmp288.Append(", "); }
            tmp288.Append("ClusterId: ");
            ClusterId.ToString(tmp288);
          }
          if((MemberId != null) && __isset.memberId)
          {
            if(0 < tmp289++) { tmp288.Append(", "); }
            tmp288.Append("MemberId: ");
            MemberId.ToString(tmp288);
          }
          tmp288.Append(')');
          return tmp288.ToString();
        }
      }


      public partial class mergeMemberToCluster_result : TBase
      {
        private global::Hazelcast.Testing.Remote.Cluster _success;

        public global::Hazelcast.Testing.Remote.Cluster Success
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

        public mergeMemberToCluster_result()
        {
        }

        public mergeMemberToCluster_result DeepCopy()
        {
          var tmp290 = new mergeMemberToCluster_result();
          if((Success != null) && __isset.success)
          {
            tmp290.Success = (global::Hazelcast.Testing.Remote.Cluster)this.Success.DeepCopy();
          }
          tmp290.__isset.success = this.__isset.success;
          return tmp290;
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
                case 0:
                  if (field.Type == TType.Struct)
                  {
                    Success = new global::Hazelcast.Testing.Remote.Cluster();
                    await Success.ReadAsync(iprot, cancellationToken);
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
            var tmp291 = new TStruct("mergeMemberToCluster_result");
            await oprot.WriteStructBeginAsync(tmp291, cancellationToken);
            var tmp292 = new TField();

            if(this.__isset.success)
            {
              if (Success != null)
              {
                tmp292.Name = "Success";
                tmp292.Type = TType.Struct;
                tmp292.ID = 0;
                await oprot.WriteFieldBeginAsync(tmp292, cancellationToken);
                await Success.WriteAsync(oprot, cancellationToken);
                await oprot.WriteFieldEndAsync(cancellationToken);
              }
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
          if (!(that is mergeMemberToCluster_result other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((Success != null) && __isset.success)
            {
              hashcode = (hashcode * 397) + Success.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp293 = new StringBuilder("mergeMemberToCluster_result(");
          int tmp294 = 0;
          if((Success != null) && __isset.success)
          {
            if(0 < tmp294++) { tmp293.Append(", "); }
            tmp293.Append("Success: ");
            Success.ToString(tmp293);
          }
          tmp293.Append(')');
          return tmp293.ToString();
        }
      }


      public partial class executeOnController_args : TBase
      {
        private string _clusterId;
        private string _script;
        private global::Hazelcast.Testing.Remote.Lang _lang;

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
        /// <seealso cref="global::Hazelcast.Testing.Remote.Lang"/>
        /// </summary>
        public global::Hazelcast.Testing.Remote.Lang Lang
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

        public executeOnController_args()
        {
        }

        public executeOnController_args DeepCopy()
        {
          var tmp295 = new executeOnController_args();
          if((ClusterId != null) && __isset.clusterId)
          {
            tmp295.ClusterId = this.ClusterId;
          }
          tmp295.__isset.clusterId = this.__isset.clusterId;
          if((Script != null) && __isset.script)
          {
            tmp295.Script = this.Script;
          }
          tmp295.__isset.script = this.__isset.script;
          if(__isset.lang)
          {
            tmp295.Lang = this.Lang;
          }
          tmp295.__isset.lang = this.__isset.lang;
          return tmp295;
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
                    ClusterId = await iprot.ReadStringAsync(cancellationToken);
                  }
                  else
                  {
                    await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                  }
                  break;
                case 2:
                  if (field.Type == TType.String)
                  {
                    Script = await iprot.ReadStringAsync(cancellationToken);
                  }
                  else
                  {
                    await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
                  }
                  break;
                case 3:
                  if (field.Type == TType.I32)
                  {
                    Lang = (global::Hazelcast.Testing.Remote.Lang)await iprot.ReadI32Async(cancellationToken);
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
            var tmp296 = new TStruct("executeOnController_args");
            await oprot.WriteStructBeginAsync(tmp296, cancellationToken);
            var tmp297 = new TField();
            if((ClusterId != null) && __isset.clusterId)
            {
              tmp297.Name = "clusterId";
              tmp297.Type = TType.String;
              tmp297.ID = 1;
              await oprot.WriteFieldBeginAsync(tmp297, cancellationToken);
              await oprot.WriteStringAsync(ClusterId, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
            if((Script != null) && __isset.script)
            {
              tmp297.Name = "script";
              tmp297.Type = TType.String;
              tmp297.ID = 2;
              await oprot.WriteFieldBeginAsync(tmp297, cancellationToken);
              await oprot.WriteStringAsync(Script, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
            if(__isset.lang)
            {
              tmp297.Name = "lang";
              tmp297.Type = TType.I32;
              tmp297.ID = 3;
              await oprot.WriteFieldBeginAsync(tmp297, cancellationToken);
              await oprot.WriteI32Async((int)Lang, cancellationToken);
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
          if (!(that is executeOnController_args other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.clusterId == other.__isset.clusterId) && ((!__isset.clusterId) || (System.Object.Equals(ClusterId, other.ClusterId))))
            && ((__isset.script == other.__isset.script) && ((!__isset.script) || (System.Object.Equals(Script, other.Script))))
            && ((__isset.lang == other.__isset.lang) && ((!__isset.lang) || (System.Object.Equals(Lang, other.Lang))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((ClusterId != null) && __isset.clusterId)
            {
              hashcode = (hashcode * 397) + ClusterId.GetHashCode();
            }
            if((Script != null) && __isset.script)
            {
              hashcode = (hashcode * 397) + Script.GetHashCode();
            }
            if(__isset.lang)
            {
              hashcode = (hashcode * 397) + Lang.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp298 = new StringBuilder("executeOnController_args(");
          int tmp299 = 0;
          if((ClusterId != null) && __isset.clusterId)
          {
            if(0 < tmp299++) { tmp298.Append(", "); }
            tmp298.Append("ClusterId: ");
            ClusterId.ToString(tmp298);
          }
          if((Script != null) && __isset.script)
          {
            if(0 < tmp299++) { tmp298.Append(", "); }
            tmp298.Append("Script: ");
            Script.ToString(tmp298);
          }
          if(__isset.lang)
          {
            if(0 < tmp299++) { tmp298.Append(", "); }
            tmp298.Append("Lang: ");
            Lang.ToString(tmp298);
          }
          tmp298.Append(')');
          return tmp298.ToString();
        }
      }


      public partial class executeOnController_result : TBase
      {
        private global::Hazelcast.Testing.Remote.Response _success;

        public global::Hazelcast.Testing.Remote.Response Success
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

        public executeOnController_result()
        {
        }

        public executeOnController_result DeepCopy()
        {
          var tmp300 = new executeOnController_result();
          if((Success != null) && __isset.success)
          {
            tmp300.Success = (global::Hazelcast.Testing.Remote.Response)this.Success.DeepCopy();
          }
          tmp300.__isset.success = this.__isset.success;
          return tmp300;
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
                case 0:
                  if (field.Type == TType.Struct)
                  {
                    Success = new global::Hazelcast.Testing.Remote.Response();
                    await Success.ReadAsync(iprot, cancellationToken);
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
            var tmp301 = new TStruct("executeOnController_result");
            await oprot.WriteStructBeginAsync(tmp301, cancellationToken);
            var tmp302 = new TField();

            if(this.__isset.success)
            {
              if (Success != null)
              {
                tmp302.Name = "Success";
                tmp302.Type = TType.Struct;
                tmp302.ID = 0;
                await oprot.WriteFieldBeginAsync(tmp302, cancellationToken);
                await Success.WriteAsync(oprot, cancellationToken);
                await oprot.WriteFieldEndAsync(cancellationToken);
              }
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
          if (!(that is executeOnController_result other)) return false;
          if (ReferenceEquals(this, other)) return true;
          return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
        }

        public override int GetHashCode() {
          int hashcode = 157;
          unchecked {
            if((Success != null) && __isset.success)
            {
              hashcode = (hashcode * 397) + Success.GetHashCode();
            }
          }
          return hashcode;
        }

        public override string ToString()
        {
          var tmp303 = new StringBuilder("executeOnController_result(");
          int tmp304 = 0;
          if((Success != null) && __isset.success)
          {
            if(0 < tmp304++) { tmp303.Append(", "); }
            tmp303.Append("Success: ");
            Success.ToString(tmp303);
          }
          tmp303.Append(')');
          return tmp303.ToString();
        }
      }

    }

  }
}
