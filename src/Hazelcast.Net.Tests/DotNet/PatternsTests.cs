#define FAST_ASYNC
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public partial class PatternsTests // async
    {
        public async Task DoSomething()
        {
            await Task.Yield();
        }

        // creates a state machine for very little benefit
        // apart from showing DoSomethingWrapped in the stack trace
        public async Task DoSomethingWrapped()
        {
            await DoSomething();
        }

        // avoids the state machine, but if DoSomething() throws,
        // will throw instead of faulting the task
        public Task DoSomethingWrappedFast()
        {
            return DoSomething();
        }

        // so, that version will fault the task in case of an exception
        public Task DoSomethingWrappedFastSafe()
        {
            try
            {
                return DoSomething();
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
        }

        // and, finally, that version can be switched
        // but it's horrible code
        public
#if !FAST_ASYNC
            async
#endif
            Task DoSomethingWrappedSwitched()
        {
#if FAST_ASYNC
            try { return
#else
            var task =
#endif
                DoSomething();
#if FAST_ASYNC
            } catch (Exception e) { return Task.FromException(e); }
#else
            await task;
#endif
        }

        // this, in fact, would be less ugly,
        // BUT it has the major issue that refactoring has issues with it
        // ie if we rename DoSomething it will only rename once, etc
#if FAST_ASYNC
        public Task DoSomethingWrappedSwitched2()
        {
            try { return DoSomething(); }
            catch (Exception e) { return Task.FromException(e); }
        }
#else
        public async Task DoSomethingWrappedSwitched2() => await DoSomething();
#endif

        public
#if !FAST_ASYNC
            async
#endif
            Task DoSomethingWrappedSwitched3()
        {
#if FAST_ASYNC
            try { return
#else
            await
#endif
                DoSomething();
#if FAST_ASYNC
            }
            catch (Exception e) { return Task.FromException(e); }
#endif
        }
    }

    public partial class PatternsTests // funcs
    {
        // read https://stackoverflow.com/questions/49299443/why-are-there-memory-allocations-when-calling-a-func
        // read https://stackoverflow.com/questions/50409034/performance-of-assigning-a-simple-lambda-expression-or-a-local-function-to-a-del

        public void M()
        {
            // allocates a new Func on every call
            var a1 = G(FInstance, 1, 1); // allocates
            var a2 = G(FStatic, 1, 1); // allocates

            // caches static but not instance
            var b1 = G((i, j) => FInstance(i, j), 1, 1); // allocates
            var b2 = G((i, j) => FStatic(i, j), 1, 1); // caches

            // same as above
            var c1 = G(FInstance, 1, 1); // allocates
            var c2 = G(FStatic, 1, 1); // caches

            var fInstance = new Func<int, int, string>(FInstance);
            var fStatic = new Func<int, int, string>(FStatic);

            // caches the func - is this better?
            var x1 = G(fInstance, 1, 1);
            var x2 = G(fStatic, 1, 1);
        }

        static Func<int, int, string> _funcH;

        // caches the Func - so this would be the best?
        // but then we are polluting code?
        // TODO: could be a later optimization but not going to do it now
        public string H()
        {
            return (_funcH ??= FInstance)(1, 1);
        }

        private static class Funcs
        {
            public static Func<int, int, string> FInstance;

            public static Func<int, int, string> FInstanceP
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Funcs.FInstance ??= FInstance; // meh - FInstance here is not what we think!
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Func<int, int, string> FInstanceM()
                => Funcs.FInstance ??= FInstance; // meh - FInstance here is not what we think!
        }

        //private static class Funcs2
        //{
        //    private static Func<int, int, string> _fInstance;

        //    public static Func<int, int, string> FInstanceP
        //    {
        //        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //        get => _fInstance ??= FInstance; // and, that cannot work because we're static
        //    }
        //}

        //private class Funcs3
        //{
        //    private Func<int, int, string> _fInstance;

        //    public Func<int, int, string> FInstanceP
        //    {
        //        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //        get => _fInstance ??= FInstance; // and, that cannot work because we're static
        //    }
        //}

        public string H2()
        {
            // nice way to encapsulate static variables
            // produces long-ish asm code
            return (Funcs.FInstance ??= FInstance)(1, 1);
        }

        // no! see note in Funcs, in fact this does not work!
        //public string H3()
        //{
        //    // won't inline the call to the property
        //    // neither in compiled C# nor in IL
        //    // but the jit does inline it in asm
        //    return Funcs.FInstanceP(1, 1);
        //}

        // in fact, cannot capture a non-static method as it also captures the instance
        // in order for this to work we'd need to make the method static and pass 'this'
        // and not sure it fully makes sense?

        public string G(Func<int, int, string> func, int i, int j)
            => func(i, j);

        public string FInstance(int i, int j) => string.Empty;
        public static string FStatic(int i, int j) => string.Empty;

        public class C
        {
            private readonly string _value;

            public C(string value)
            {
                _value = value;
            }

            private static Func<string> _produce;

            private string Produce() => _value;

            public string Get()
            {
                return (_produce ??= Produce)();
            }
        }

        // yup - that proves to point, don't try to not allocate!
        [Test]
        public void CTest()
        {
            var c = new C("aaa");
            Console.WriteLine(c.Get());

            c = new C("bbb");
            Console.WriteLine(c.Get());
        }
    }
}
