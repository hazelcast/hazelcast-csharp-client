// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Hazelcast.Benchmarks
{
    public class NullCoalesce
    {
        private bool _toggle;

        public Func<int> F { get; set; }

        public NullCoalesce()
        {
            //F = () => 0;

            F = () =>
            {
                _toggle = !_toggle;
                return _toggle ? 1 : 0;
            };
        }

        [Benchmark]
        public void BenchmarkM1()
        {
            var total = 0;
            for (var i = 0; i < 1000; i++)
                total += M1();
        }

        private int M1()
            => F == null ? 0 : F();

        [Benchmark]
        public void BenchmarkM2()
        {
            var total = 0;
            for (var i = 0; i < 1000; i++)
                total += M2();
        }

        private int M2()
            => F?.Invoke() ?? 0;

        public Func<ValueTask<int>> FAsync { get; set; }

        [StructLayout(LayoutKind.Auto)]
        [CompilerGenerated]
        private struct M1AsyncAsyncA__d__5 : IAsyncStateMachine
        {
            public int __1__state;

            public AsyncValueTaskMethodBuilder<int> __t__builder;

            public NullCoalesce __4__this;

            private ValueTaskAwaiter<int> __u__1;

            private void MoveNext()
            {
                int num = __1__state;
                NullCoalesce c = __4__this;
                int result;
                try
                {
                    /* M1AsyncAsyncA__d__5
                    ValueTaskAwaiter<int> awaiter;
                    if (num != 0)
                    {
                        ValueTask<int> valueTask = (c.FAsync != null) ? c.FAsync() : default(ValueTask<int>);
                        awaiter = valueTask.GetAwaiter();
                        if (!awaiter.IsCompleted)
                        {
                            num = (__1__state = 0);
                            __u__1 = awaiter;
                            __t__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                            return;
                        }
                    }
                    else
                    {
                        awaiter = __u__1;
                        __u__1 = default(ValueTaskAwaiter<int>);
                        num = (__1__state = -1);
                    }
                    result = awaiter.GetResult();
                    */

                    /* M1AsyncAsyncB__d__6 */

                    int num2;
                    ValueTaskAwaiter<int> awaiter;
                    if (num != 0)
                    {
                        if (c.FAsync == null)
                        {
                            num2 = 0;
                            goto IL_0082;
                        }
                        awaiter = c.FAsync().GetAwaiter();
                        if (!awaiter.IsCompleted)
                        {
                            num = (__1__state = 0);
                            __u__1 = awaiter;
                            __t__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                            return;
                        }
                    }
                    else
                    {
                        awaiter = __u__1;
                        __u__1 = default(ValueTaskAwaiter<int>);
                        num = (__1__state = -1);
                    }
                    num2 = awaiter.GetResult();
                    goto IL_0082;
                    IL_0082:
                    result = num2;
                    /**/

                    /* M2AsyncAsync__d__8

                    ValueTaskAwaiter<int> awaiter;
                    if (num != 0)
                    {
                        Func<ValueTask<int>> fAsync = c.FAsync;
                        ValueTask<int> valueTask = (fAsync != null) ? fAsync() : default(ValueTask<int>);
                        awaiter = valueTask.GetAwaiter();
                        if (!awaiter.IsCompleted)
                        {
                            num = (__1__state = 0);
                            __u__1 = awaiter;
                            __t__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                            return;
                        }
                    }
                    else
                    {
                        awaiter = __u__1;
                        __u__1 = default(ValueTaskAwaiter<int>);
                        num = (__1__state = -1);
                    }
                    result = awaiter.GetResult();
                    */

                    // ----
                }
                catch (Exception exception)
                {
                    __1__state = -2;
                    __t__builder.SetException(exception);
                    return;
                }
                __1__state = -2;
                __t__builder.SetResult(result);
            }

            void IAsyncStateMachine.MoveNext()
            {
                //ILSpy generated this explicit interface implementation from .override directive in MoveNext
                this.MoveNext();
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                __t__builder.SetStateMachine(stateMachine);
            }

            void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
            {
                //ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
                this.SetStateMachine(stateMachine);
            }
        }

        // RESULTS
        // M1Async and M2Async : exact same assembly code
        // M1AsyncAsyncA, M1AsyncAsyncB and M2AsyncASync : exact same assembly code
        //  (heavy usage of the state machine)
        // so the diff is in the state machine
        //
        // M1AsyncAsyncA
        //  ValueTask<int> valueTask = (c.FAsync != null) ? c.FAsync() : default(ValueTask<int>);
        //  awaiter = valueTask.GetAwaiter();
        //
        // M2AsyncAsync
        //  Func<ValueTask<int>> fAsync = c.FAsync;
        //  ValueTask<int> valueTask = (fAsync != null) ? fAsync() : default(ValueTask<int>);
        //  awaiter = valueTask.GetAwaiter();
        //
        // M1AsyncAsyncB
        //  if (c.FAsync == null) { result = 0; done; }
        //  awaiter = c.FAsync().GetAwaiter();
        //
        // assembly level, M1AsyncAsyncA and M2AsyncAsync are practically equivalent
        // assembly level, M1AsyncAsyncB is marginally simpler

        private ValueTask<int> M1AsyncAsyncA__generated()
        {
            M1AsyncAsyncA__d__5 stateMachine = default(M1AsyncAsyncA__d__5);

            stateMachine.__4__this = this;
            stateMachine.__t__builder = AsyncValueTaskMethodBuilder<int>.Create();
            stateMachine.__1__state = -1;

            AsyncValueTaskMethodBuilder<int> __t__builder = stateMachine.__t__builder;
            __t__builder.Start(ref stateMachine);

            return stateMachine.__t__builder.Task;
        }

        /* M1Async

        IL_0000: ldarg.0
        IL_0001: call instance class [System.Private.CoreLib]System.Func`1<valuetype [System.Private.CoreLib]System.Threading.Tasks.ValueTask`1<int32>> C::get_FAsync()
        IL_0006: brtrue.s IL_0012

        IL_0008: ldloca.s 0
        IL_000a: initobj valuetype [System.Private.CoreLib]System.Threading.Tasks.ValueTask`1<int32>
        IL_0010: ldloc.0
        IL_0011: ret

        IL_0012: ldarg.0
        IL_0013: call instance class [System.Private.CoreLib]System.Func`1<valuetype [System.Private.CoreLib]System.Threading.Tasks.ValueTask`1<int32>> C::get_FAsync()
        IL_0018: callvirt instance !0 class [System.Private.CoreLib]System.Func`1<valuetype [System.Private.CoreLib]System.Threading.Tasks.ValueTask`1<int32>>::Invoke()
        IL_001d: ret

        L0000: mov eax, [ecx+4]
        L0003: test eax, eax
        L0005: jne short L0012
        L0007: xor eax, eax
        L0009: mov [edx], eax
        L000b: mov [edx+4], eax
        L000e: mov [edx+8], eax
        L0011: ret
        L0012: mov ecx, [eax+4]
        L0015: call dword ptr [eax+0xc]
        L0018: ret

         */
        private ValueTask<int> M1Async()
            => FAsync != null ? FAsync() : default;

        /* M1AsyncAsyncA

        M1AsyncAsyncA__d__5 stateMachine = default(M1AsyncAsyncA__d__5);
        stateMachine.__4__this = this;
        stateMachine.__t__builder = AsyncValueTaskMethodBuilder<int>.Create();
        stateMachine.__1__state = -1;
        AsyncValueTaskMethodBuilder<int> __t__builder = stateMachine.__t__builder;
        __t__builder.Start(ref stateMachine);
        return stateMachine.__t__builder.Task;

        IL_0000: ldloca.s 0
        IL_0002: ldarg.0
        IL_0003: stfld class C C/'M1AsyncAsyncA__d__5'::'__4__this'
        IL_0008: ldloca.s 0
        IL_000a: call valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<!0> valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32>::Create()
        IL_000f: stfld valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32> C/'M1AsyncAsyncA__d__5'::'__t__builder'
        IL_0014: ldloca.s 0
        IL_0016: ldc.i4.m1
        IL_0017: stfld int32 C/'M1AsyncAsyncA__d__5'::'__1__state'
        IL_001c: ldloc.0
        IL_001d: ldfld valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32> C/'M1AsyncAsyncA__d__5'::'__t__builder'
        IL_0022: stloc.1
        IL_0023: ldloca.s 1
        IL_0025: ldloca.s 0
        IL_0027: call instance void valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32>::Start<valuetype C/'M1AsyncAsyncA__d__5'>(!!0&)
        IL_002c: ldloca.s 0
        IL_002e: ldflda valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32> C/'M1AsyncAsyncA__d__5'::'__t__builder'
        IL_0033: call instance valuetype [System.Private.CoreLib]System.Threading.Tasks.ValueTask`1<!0> valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32>::get_Task()
        IL_0038: ret

         */

        private async ValueTask<int> M1AsyncAsyncA()
        {
            return await (FAsync != null ? FAsync() : default);
        }

        /* M2AsyncAsyncB

        M1AsyncAsyncB__d__6 stateMachine = default(M1AsyncAsyncB__d__6);
        stateMachine.__4__this = this;
        stateMachine.__t__builder = AsyncValueTaskMethodBuilder<int>.Create();
        stateMachine.__1__state = -1;
        AsyncValueTaskMethodBuilder<int> __t__builder = stateMachine.__t__builder;
        __t__builder.Start(ref stateMachine);
        return stateMachine.__t__builder.Task;

        IL_0000: ldloca.s 0
        IL_0002: ldarg.0
        IL_0003: stfld class C C/'<M1AsyncAsyncB>d__6'::'__4__this'
        IL_0008: ldloca.s 0
        IL_000a: call valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<!0> valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32>::Create()
        IL_000f: stfld valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32> C/'<M1AsyncAsyncB>d__6'::'__t__builder'
        IL_0014: ldloca.s 0
        IL_0016: ldc.i4.m1
        IL_0017: stfld int32 C/'<M1AsyncAsyncB>d__6'::'__1__state'
        IL_001c: ldloc.0
        IL_001d: ldfld valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32> C/'<M1AsyncAsyncB>d__6'::'__t__builder'
        IL_0022: stloc.1
        IL_0023: ldloca.s 1
        IL_0025: ldloca.s 0
        IL_0027: call instance void valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32>::Start<valuetype C/'<M1AsyncAsyncB>d__6'>(!!0&)
        IL_002c: ldloca.s 0
        IL_002e: ldflda valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32> C/'<M1AsyncAsyncB>d__6'::'__t__builder'
        IL_0033: call instance valuetype [System.Private.CoreLib]System.Threading.Tasks.ValueTask`1<!0> valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32>::get_Task()
        IL_0038: ret

         */

        private async ValueTask<int> M1AsyncAsyncB()
        {
            return FAsync != null ? await FAsync() : default;
        }

        /* M2Async

        IL_0000: ldarg.0
        IL_0001: call instance class [System.Private.CoreLib]System.Func`1<valuetype [System.Private.CoreLib]System.Threading.Tasks.ValueTask`1<int32>> C::get_FAsync()
        IL_0006: dup
        IL_0007: brtrue.s IL_0014

        IL_0009: pop
        IL_000a: ldloca.s 0
        IL_000c: initobj valuetype [System.Private.CoreLib]System.Threading.Tasks.ValueTask`1<int32>
        IL_0012: ldloc.0
        IL_0013: ret

        IL_0014: callvirt instance !0 class [System.Private.CoreLib]System.Func`1<valuetype [System.Private.CoreLib]System.Threading.Tasks.ValueTask`1<int32>>::Invoke()
        IL_0019: ret

        L0000: mov eax, [ecx+4]
        L0003: test eax, eax
        L0005: jne short L0012
        L0007: xor eax, eax
        L0009: mov [edx], eax
        L000b: mov [edx+4], eax
        L000e: mov [edx+8], eax
        L0011: ret
        L0012: mov ecx, [eax+4]
        L0015: call dword ptr [eax+0xc]
        L0018: ret

         */

        private ValueTask<int> M2Async()
            => FAsync?.Invoke() ?? default;

        /* M2AsyncAsync

        M2AsyncAsync__d__8 stateMachine = default(M2AsyncAsync__d__8);
        stateMachine.__4__this = this;
        stateMachine.__t__builder = AsyncValueTaskMethodBuilder<int>.Create();
        stateMachine.__1__state = -1;
        AsyncValueTaskMethodBuilder<int> __t__builder = stateMachine.__t__builder;
        __t__builder.Start(ref stateMachine);
        return stateMachine.__t__builder.Task;

        IL_0000: ldloca.s 0
        IL_0002: ldarg.0
        IL_0003: stfld class C C/'<M2AsyncAsync>d__8'::'__4__this'
        IL_0008: ldloca.s 0
        IL_000a: call valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<!0> valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32>::Create()
        IL_000f: stfld valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32> C/'<M2AsyncAsync>d__8'::'__t__builder'
        IL_0014: ldloca.s 0
        IL_0016: ldc.i4.m1
        IL_0017: stfld int32 C/'<M2AsyncAsync>d__8'::'__1__state'
        IL_001c: ldloc.0
        IL_001d: ldfld valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32> C/'<M2AsyncAsync>d__8'::'__t__builder'
        IL_0022: stloc.1
        IL_0023: ldloca.s 1
        IL_0025: ldloca.s 0
        IL_0027: call instance void valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32>::Start<valuetype C/'<M2AsyncAsync>d__8'>(!!0&)
        IL_002c: ldloca.s 0
        IL_002e: ldflda valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32> C/'<M2AsyncAsync>d__8'::'__t__builder'
        IL_0033: call instance valuetype [System.Private.CoreLib]System.Threading.Tasks.ValueTask`1<!0> valuetype [System.Private.CoreLib]System.Runtime.CompilerServices.AsyncValueTaskMethodBuilder`1<int32>::get_Task()
        IL_0038: ret

         */

        private async ValueTask<int> M2AsyncAsync()
        {
            return await (FAsync?.Invoke() ?? default);
        }
    }
}
