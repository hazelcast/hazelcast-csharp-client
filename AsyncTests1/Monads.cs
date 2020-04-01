using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncTests1
{
    // https://www.dotnetcurry.com/patterns-practices/1510/maybe-monad-csharp
    // https://www.dotnetcurry.com/patterns-practices/1526/maybe-monad-csharp-examples
    // https://github.com/ymassad/MaybeExamples

    // could also implement Maybe as a struct?

    public static class Maybe
    {
        public static Maybe<T> Some<T>(T value) => new Maybe<T>.Some(value);

        //public static Maybe<T> None<T>() => new Maybe<T>.None();

        public class MaybeNone
        { }

        public static MaybeNone None { get; } = new MaybeNone();
    }

    public abstract class Maybe<T>
    {
        private Maybe()
        { }

        public sealed class Some : Maybe<T>
        {
            public Some(T value) => Value = value;

            public T Value { get; }

            public override T ValueOr(T value) => Value;
        }

        public sealed class MaybeNone : Maybe<T>
        {
            public override T ValueOr(T value)=> value;
        }

        public static MaybeNone None { get; } = new MaybeNone();

        public bool TryGetValue(out T value)
        {
            if (this is Some some)
            {
                value = some.Value;
                return true;
            }

            value = default;
            return false;
        }

        public static implicit operator Maybe<T>(Maybe.MaybeNone _)
            => None;

        public static implicit operator Maybe<T>(T value)
        {
            // this null thing is annoying
            // if T is int - how can it be null?
            // is 'null' always the None value?
            if (value == null)
                return None;
            return new Some(value);
        }

        public Maybe<T1> Map<T1>(Func<T, T1> map)
        {
            return this is Some some
                ? Maybe.Some(map(some.Value))
                : Maybe.None;
        }

        public Maybe<T1> Bind<T1>(Func<T, Maybe<T1>> bind)
        {
            return this is Some some
                ? bind(some.Value)
                : Maybe.None;
        }

        public abstract T ValueOr(T value);
    }

    public class TestClass
    {
        public void Test()
        {
            var foo = Maybe.Some(123);
            if (foo.TryGetValue(out var value))
                Console.WriteLine(value);

            var none = Maybe<int>.None;
            // implicit cast of Maybe.None to Maybe.None<T>?
        }

        public Maybe<int> GetMaybeInt1()
        {
            return 3;
        }

        public Maybe<int> GetMaybeInt2()
        {
            return Maybe.None;
        }
    }
}
