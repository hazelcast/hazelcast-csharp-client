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

namespace Hazelcast.Tests.Sandbox.ClassMaybe
{
    // keep all classes in 1 file to simplify understanding!
    //
    // https://www.dotnetcurry.com/patterns-practices/1510/maybe-monad-csharp
    // https://www.dotnetcurry.com/patterns-practices/1526/maybe-monad-csharp-examples
    // https://github.com/ymassad/MaybeExamples

    public static class Maybe
    {
        public static Maybe<T> Some<T>(T value) => new Some<T>(value);

        //public static Maybe<T> None<T>() => new Maybe<T>.None();

        public static None None { get; } = new None();
    }

    public sealed class None
    { }

    public abstract class Maybe<T>
    {
        protected Maybe()
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

        public static implicit operator Maybe<T>(None _)
            => None;

        public static implicit operator Maybe<T>(T value)
        {
            // this null thing is annoying
            // if T is int - how can it be null?
            // is 'null' always the None value?
            if (value == null)
                return None;
            return new Some<T>(value);
        }

        public Maybe<T1> Map<T1>(Func<T, T1> map)
        {
            return this is Some<T> some
                ? Maybe.Some(map(some.Value))
                : Maybe.None;
        }

        public Maybe<T1> Bind<T1>(Func<T, Maybe<T1>> bind)
        {
            return this is Some<T> some
                ? bind(some.Value)
                : Maybe.None;
        }

        public abstract T ValueOr(T value);
    }

    public sealed class Some<T> : Maybe<T>
    {
        public Some(T value) => Value = value;

        public T Value { get; }

        public override T ValueOr(T value) => Value;
    }

    public sealed class None<T> : Maybe<T>
    {
        public override T ValueOr(T value) => value;
    }
}
