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

using System.Collections;
using NUnit.Framework;

// ReSharper disable UnusedMember.Global

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides extension methods to the <see cref="Asserter"/> class,
    /// corresponding to methods in NUnit 'Assert.cs' source file.
    /// </summary>
    public static class AsserterExtensions
    {
        #region Pass

        /// <summary>
        /// Throws a <see cref="SuccessException"/> with the message and arguments
        /// that are passed in. This allows a test to be cut short, with a result
        /// of success returned to NUnit.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="message">The message to initialize the <see cref="AssertionException"/> with.</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static void Pass(this Asserter asserter, string message, params object[] args)
            => Assert.Pass(message, args);

        /// <summary>
        /// Throws a <see cref="SuccessException"/> with the message and arguments
        /// that are passed in. This allows a test to be cut short, with a result
        /// of success returned to NUnit.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="message">The message to initialize the <see cref="AssertionException"/> with.</param>
        public static void Pass(this Asserter asserter, string message)
            => Assert.Pass(message);

        /// <summary>
        /// Throws a <see cref="SuccessException"/> with the message and arguments
        /// that are passed in. This allows a test to be cut short, with a result
        /// of success returned to NUnit.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        public static void Pass(this Asserter asserter)
            => Assert.Pass();

        #endregion

        #region Fail

        /// <summary>
        /// Throws an <see cref="AssertionException"/> with the message and arguments
        /// that are passed in. This is used by the other Assert functions.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="message">The message to initialize the <see cref="AssertionException"/> with.</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static void Fail(this Asserter asserter, string message, params object[] args)
            => Assert.Fail(message, args);

        /// <summary>
        /// Throws an <see cref="AssertionException"/> with the message that is
        /// passed in. This is used by the other Assert functions.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="message">The message to initialize the <see cref="AssertionException"/> with.</param>
        public static void Fail(this Asserter asserter, string message)
            => Assert.Fail(message);

        /// <summary>
        /// Throws an <see cref="AssertionException"/>.
        /// This is used by the other Assert functions.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        public static void Fail(this Asserter asserter)
            => Assert.Fail();

        #endregion

        #region Warn

        /// <summary>
        /// Issues a warning using the message and arguments provided.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="message">The message to display.</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static void Warn(this Asserter asserter, string message, params object[] args)
            => Assert.Warn(message, args);

        /// <summary>
        /// Issues a warning using the message provided.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="message">The message to display.</param>
        public static void Warn(this Asserter asserter, string message)
            => Assert.Warn(message);

        #endregion

        #region Ignore

        /// <summary>
        /// Throws an <see cref="IgnoreException"/> with the message and arguments
        /// that are passed in.  This causes the test to be reported as ignored.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="message">The message to initialize the <see cref="AssertionException"/> with.</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static void Ignore(this Asserter asserter, string message, params object[] args)
            => Assert.Ignore(message, args);

        /// <summary>
        /// Throws an <see cref="IgnoreException"/> with the message that is
        /// passed in. This causes the test to be reported as ignored.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="message">The message to initialize the <see cref="AssertionException"/> with.</param>
        public static void Ignore(this Asserter asserter, string message)
            => Assert.Ignore(message);

        /// <summary>
        /// Throws an <see cref="IgnoreException"/>.
        /// This causes the test to be reported as ignored.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        public static void Ignore(this Asserter asserter)
            => Assert.Ignore();

        #endregion

        #region InConclusive

        /// <summary>
        /// Throws an <see cref="InconclusiveException"/> with the message and arguments
        /// that are passed in.  This causes the test to be reported as inconclusive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="message">The message to initialize the <see cref="InconclusiveException"/> with.</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static void Inconclusive(this Asserter asserter, string message, params object[] args)
            => Assert.Inconclusive(message, args);

        /// <summary>
        /// Throws an <see cref="InconclusiveException"/> with the message that is
        /// passed in. This causes the test to be reported as inconclusive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="message">The message to initialize the <see cref="InconclusiveException"/> with.</param>
        public static void Inconclusive(this Asserter asserter, string message)
            => Assert.Inconclusive(message);

        /// <summary>
        /// Throws an <see cref="InconclusiveException"/>.
        /// This causes the test to be reported as Inconclusive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        public static void Inconclusive(this Asserter asserter)
            => Assert.Inconclusive();

        #endregion

        #region Contains

        /// <summary>
        /// Asserts that an object is contained in a collection.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="expected">The expected object</param>
        /// <param name="actual">The collection to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Contains(this Asserter asserter, object expected, ICollection actual, string message, params object[] args)
            => Assert.Contains(expected, actual, message, args);

        /// <summary>
        /// Asserts that an object is contained in a collection.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="expected">The expected object</param>
        /// <param name="actual">The collection to be examined</param>
        public static void Contains(this Asserter asserter, object expected, ICollection actual)
            => Assert.Contains(expected, actual);

        #endregion

        #region Multiple

        /// <summary>
        /// Wraps code containing a series of assertions, which should all
        /// be executed, even if they fail. Failed results are saved and
        /// reported at the end of the code block.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="testDelegate">A TestDelegate to be executed in Multiple Assertion mode.</param>
        public static void Multiple(this Asserter asserter, TestDelegate testDelegate)
            => Assert.Multiple(testDelegate);

        #endregion
    }
}
