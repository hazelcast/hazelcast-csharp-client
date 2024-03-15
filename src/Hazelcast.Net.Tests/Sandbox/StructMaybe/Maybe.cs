// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Tests.Sandbox.StructMaybe
{
    // keep all classes in 1 file to simplify understanding!
    //
    // https://www.dotnetcurry.com/patterns-practices/1510/maybe-monad-csharp
    // https://www.dotnetcurry.com/patterns-practices/1526/maybe-monad-csharp-examples
    // https://github.com/ymassad/MaybeExamples

    public readonly struct Maybe<T>
    {
        private readonly T _value;
        private readonly bool _hasValue;

        private Maybe(T value)
        {
            _value = value;
            _hasValue = true;
        }

        public static implicit operator Maybe<T>(T value)
        {
            return value == null ? default : new Maybe<T>(value);
        }

        public static implicit operator Maybe<T>(Maybe.MaybeNone none)
        {
            return default;
        }

        public bool TryGetValue(out T value)
        {
            value = _hasValue ? _value : default;
            return _hasValue;
        }

        public Maybe<T1> Map<T1>(Func<T, T1> map)
        {
            return _hasValue ? map(_value) : default (Maybe<T1>);
        }

        public Maybe<T1> Bind<T1>(Func<T, Maybe<T1>> bind)
        {
            return _hasValue ? bind(_value) : default;
        }
    }

    public static class Maybe
    {
        public class MaybeNone { }

        public static MaybeNone None { get; } = new MaybeNone();

        public static Maybe<T> Some<T>(T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return value;
        }
    }


    public static class _MayBe
    {
        public static _MayBe<T> Some<T>(T value) => new Some<T>(value);

        //public static _MayBe<T> None<T>() => new _MayBe<T>.None();

        public static None None { get; } = new None();
    }

    public sealed class None
    { }

    public abstract class _MayBe<T>
    {
        protected _MayBe()
        { }

        public static None<T> None { get; } = new None<T>();

        public bool TryGetValue(out T value)
        {
            if (this is Some<T> some)
            {
                value = some.Value;
                return true;
            }

            value = default;
            return false;
        }

        public static implicit operator _MayBe<T>(None _)
            => None;

        public static implicit operator _MayBe<T>(T value)
        {
            // this null thing is annoying
            // if T is int - how can it be null?
            // is 'null' always the None value?
            if (value == null)
                return None;
            return new Some<T>(value);
        }

        public _MayBe<T1> Map<T1>(Func<T, T1> map)
        {
            return this is Some<T> some
                ? _MayBe.Some(map(some.Value))
                : _MayBe.None;
        }

        public _MayBe<T1> Bind<T1>(Func<T, _MayBe<T1>> bind)
        {
            return this is Some<T> some
                ? bind(some.Value)
                : _MayBe.None;
        }

        public abstract T ValueOr(T value);
    }

    public sealed class Some<T> : _MayBe<T>
    {
        public Some(T value) => Value = value;

        public T Value { get; }

        public override T ValueOr(T value) => Value;
    }

    public sealed class None<T> : _MayBe<T>
    {
        public override T ValueOr(T value) => value;
    }

    public class TestClass
    {
        public void Test()
        {
            var foo = _MayBe.Some(123);
            if (foo.TryGetValue(out var value))
                Console.WriteLine(value);

            var none = _MayBe<int>.None;
            // implicit cast of _MayBe.None to _MayBe.None<T>?
        }

        public _MayBe<int> Get_MayBeInt1()
        {
            return 3;
        }

        public _MayBe<int> Get_MayBeInt2()
        {
            return _MayBe.None;
        }
    }
}
