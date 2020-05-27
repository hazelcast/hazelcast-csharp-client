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

// ReSharper disable UnusedMember.Global
#pragma warning disable IDE0060 // Remove unused parameter

using System;
using NUnit.Framework;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides extension methods to the <see cref="Asserter"/> class,
    /// corresponding to methods in NUnit 'Assert.Equality.cs' source file.
    /// </summary>
    public static class AsserterTypeExtensions
    {
        // TODO: finish rewriting all methods as => Assert...

        #region IsAssignableFrom

        /// <summary>
        /// Asserts that an object may be assigned a value of a given Type.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="expected">The expected Type.</param>
        /// <param name="actual">The object under examination</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsAssignableFrom(this Asserter asserter, Type expected, object actual, string message, params object[] args)
        {
            Assert.That(actual, Is.AssignableFrom(expected), message, args);
        }

        /// <summary>
        /// Asserts that an object may be assigned a value of a given Type.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="expected">The expected Type.</param>
        /// <param name="actual">The object under examination</param>
        public static void IsAssignableFrom(this Asserter asserter, Type expected, object actual)
        {
            Assert.That(actual, Is.AssignableFrom(expected), null, null);
        }

        #endregion

        #region IsAssignableFrom<TExpected>

        /// <summary>
        /// Asserts that an object may be assigned a value of a given Type.
        /// </summary>
        /// <typeparam name="TExpected">The expected Type.</typeparam>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The object under examination</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsAssignableFrom<TExpected>(this Asserter asserter, object actual, string message, params object[] args)
        {
            Assert.That(actual, Is.AssignableFrom(typeof(TExpected)), message, args);
        }

        /// <summary>
        /// Asserts that an object may be assigned a value of a given Type.
        /// </summary>
        /// <typeparam name="TExpected">The expected Type.</typeparam>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The object under examination</param>
        public static void IsAssignableFrom<TExpected>(this Asserter asserter, object actual)
        {
            Assert.That(actual, Is.AssignableFrom(typeof(TExpected)), null, null);
        }

        #endregion

        #region IsNotAssignableFrom

        /// <summary>
        /// Asserts that an object may not be assigned a value of a given Type.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="expected">The expected Type.</param>
        /// <param name="actual">The object under examination</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsNotAssignableFrom(this Asserter asserter, Type expected, object actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.AssignableFrom(expected), message, args);
        }

        /// <summary>
        /// Asserts that an object may not be assigned a value of a given Type.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="expected">The expected Type.</param>
        /// <param name="actual">The object under examination</param>
        public static void IsNotAssignableFrom(this Asserter asserter, Type expected, object actual)
        {
            Assert.That(actual, Is.Not.AssignableFrom(expected), null, null);
        }

        #endregion

        #region IsNotAssignableFrom<TExpected>

        /// <summary>
        /// Asserts that an object may not be assigned a value of a given Type.
        /// </summary>
        /// <typeparam name="TExpected">The expected Type.</typeparam>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The object under examination</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsNotAssignableFrom<TExpected>(this Asserter asserter, object actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.AssignableFrom(typeof(TExpected)), message, args);
        }

        /// <summary>
        /// Asserts that an object may not be assigned a value of a given Type.
        /// </summary>
        /// <typeparam name="TExpected">The expected Type.</typeparam>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The object under examination</param>
        public static void IsNotAssignableFrom<TExpected>(this Asserter asserter, object actual)
        {
            Assert.That(actual, Is.Not.AssignableFrom(typeof(TExpected)), null, null);
        }

        #endregion

        #region IsInstanceOf

        /// <summary>
        /// Asserts that an object is an instance of a given type.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="expected">The expected Type</param>
        /// <param name="actual">The object being examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsInstanceOf(this Asserter asserter, Type expected, object actual, string message, params object[] args)
        {
            Assert.That(actual, Is.InstanceOf(expected), message, args);
        }

        /// <summary>
        /// Asserts that an object is an instance of a given type.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="expected">The expected Type</param>
        /// <param name="actual">The object being examined</param>
        public static void IsInstanceOf(this Asserter asserter, Type expected, object actual)
        {
            Assert.That(actual, Is.InstanceOf(expected), null, null);
        }

        #endregion

        #region IsInstanceOf<TExpected>

        /// <summary>
        /// Asserts that an object is an instance of a given type.
        /// </summary>
        /// <typeparam name="TExpected">The expected Type</typeparam>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The object being examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsInstanceOf<TExpected>(this Asserter asserter, object actual, string message, params object[] args)
        {
            Assert.That(actual, Is.InstanceOf(typeof(TExpected)), message, args);
        }

        /// <summary>
        /// Asserts that an object is an instance of a given type.
        /// </summary>
        /// <typeparam name="TExpected">The expected Type</typeparam>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The object being examined</param>
        public static void IsInstanceOf<TExpected>(this Asserter asserter, object actual)
        {
            Assert.That(actual, Is.InstanceOf(typeof(TExpected)), null, null);
        }

        #endregion

        #region IsNotInstanceOf

        /// <summary>
        /// Asserts that an object is not an instance of a given type.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="expected">The expected Type</param>
        /// <param name="actual">The object being examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsNotInstanceOf(this Asserter asserter, Type expected, object actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.InstanceOf(expected), message, args);
        }

        /// <summary>
        /// Asserts that an object is not an instance of a given type.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="expected">The expected Type</param>
        /// <param name="actual">The object being examined</param>
        public static void IsNotInstanceOf(this Asserter asserter, Type expected, object actual)
        {
            Assert.That(actual, Is.Not.InstanceOf(expected), null, null);
        }

        #endregion

        #region IsNotInstanceOf<TExpected>

        /// <summary>
        /// Asserts that an object is not an instance of a given type.
        /// </summary>
        /// <typeparam name="TExpected">The expected Type</typeparam>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The object being examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsNotInstanceOf<TExpected>(this Asserter asserter, object actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.InstanceOf(typeof(TExpected)), message, args);
        }

        /// <summary>
        /// Asserts that an object is not an instance of a given type.
        /// </summary>
        /// <typeparam name="TExpected">The expected Type</typeparam>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The object being examined</param>
        public static void IsNotInstanceOf<TExpected>(this Asserter asserter, object actual)
        {
            Assert.That(actual, Is.Not.InstanceOf(typeof(TExpected)), null, null);
        }

        #endregion
    }
}