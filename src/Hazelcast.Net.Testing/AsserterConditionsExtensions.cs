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
#pragma warning disable IDE0060 // Remove unused parameter

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides extension methods to the <see cref="Asserter"/> class,
    /// corresponding to methods in NUnit 'Assert.Condition.cs' source file.
    /// </summary>
    public static class AsserterConditionsExtensions
    {
        // TODO: finish rewriting all methods as => Assert...

        #region True

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void True(this Asserter asserter, bool? condition, string message, params object[] args)
            => Assert.True(condition, message, args);

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void True(this Asserter asserter, bool condition, string message, params object[] args)
            => Assert.True(condition, message, args);

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        public static void True(this Asserter asserter, bool? condition)
            => Assert.True(condition);

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        public static void True(this Asserter asserter, bool condition)
            => Assert.True(condition);

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsTrue(this Asserter asserter, bool? condition, string message, params object[] args)
            => Assert.IsTrue(condition, message, args);

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsTrue(this Asserter asserter, bool condition, string message, params object[] args)
            => Assert.IsTrue(condition, message, args);

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        public static void IsTrue(this Asserter asserter, bool? condition)
            => Assert.IsTrue(condition);

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        public static void IsTrue(this Asserter asserter, bool condition)
            => Assert.IsTrue(condition);

        #endregion

        #region False

        /// <summary>
        /// Asserts that a condition is false. If the condition is true the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void False(this Asserter asserter, bool? condition, string message, params object[] args)
        {
            Assert.That(condition, Is.False, message, args);
        }

        /// <summary>
        /// Asserts that a condition is false. If the condition is true the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void False(this Asserter asserter, bool condition, string message, params object[] args)
        {
            Assert.That(condition, Is.False, message, args);
        }

        /// <summary>
        /// Asserts that a condition is false. If the condition is true the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        public static void False(this Asserter asserter, bool? condition)
        {
            Assert.That(condition, Is.False, null, null);
        }

        /// <summary>
        /// Asserts that a condition is false. If the condition is true the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        public static void False(this Asserter asserter, bool condition)
        {
            Assert.That(condition, Is.False, null, null);
        }

        /// <summary>
        /// Asserts that a condition is false. If the condition is true the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsFalse(this Asserter asserter, bool? condition, string message, params object[] args)
        {
            Assert.That(condition, Is.False, message, args);
        }

        /// <summary>
        /// Asserts that a condition is false. If the condition is true the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsFalse(this Asserter asserter, bool condition, string message, params object[] args)
        {
            Assert.That(condition, Is.False, message, args);
        }

        /// <summary>
        /// Asserts that a condition is false. If the condition is true the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        public static void IsFalse(this Asserter asserter, bool? condition)
        {
            Assert.That(condition, Is.False, null, null);
        }

        /// <summary>
        /// Asserts that a condition is false. If the condition is true the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        public static void IsFalse(this Asserter asserter, bool condition)
        {
            Assert.That(condition, Is.False, null, null);
        }

        #endregion

        #region NotNull

        /// <summary>
        /// Verifies that the object that is passed in is not equal to <code>null</code>
        /// If the object is <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="anObject">The object that is to be tested</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void NotNull(this Asserter asserter, object anObject, string message, params object[] args)
        {
            Assert.That(anObject, Is.Not.Null, message, args);
        }

        /// <summary>
        /// Verifies that the object that is passed in is not equal to <code>null</code>
        /// If the object is <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="anObject">The object that is to be tested</param>
        public static void NotNull(this Asserter asserter, object anObject)
        {
            Assert.That(anObject, Is.Not.Null, null, null);
        }

        /// <summary>
        /// Verifies that the object that is passed in is not equal to <code>null</code>
        /// If the object is <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="anObject">The object that is to be tested</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsNotNull(this Asserter asserter, object anObject, string message, params object[] args)
        {
            Assert.That(anObject, Is.Not.Null, message, args);
        }

        /// <summary>
        /// Verifies that the object that is passed in is not equal to <code>null</code>
        /// If the object is <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="anObject">The object that is to be tested</param>
        public static void IsNotNull(this Asserter asserter, object anObject)
        {
            Assert.That(anObject, Is.Not.Null, null, null);
        }

        #endregion

        #region Null

        /// <summary>
        /// Verifies that the object that is passed in is equal to <code>null</code>
        /// If the object is not <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="anObject">The object that is to be tested</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Null(this Asserter asserter, object anObject, string message, params object[] args)
        {
            Assert.That(anObject, Is.Null, message, args);
        }

        /// <summary>
        /// Verifies that the object that is passed in is equal to <code>null</code>
        /// If the object is not <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="anObject">The object that is to be tested</param>
        public static void Null(this Asserter asserter, object anObject)
        {
            Assert.That(anObject, Is.Null, null, null);
        }

        /// <summary>
        /// Verifies that the object that is passed in is equal to <code>null</code>
        /// If the object is not <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="anObject">The object that is to be tested</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsNull(this Asserter asserter, object anObject, string message, params object[] args)
        {
            Assert.That(anObject, Is.Null, message, args);
        }

        /// <summary>
        /// Verifies that the object that is passed in is equal to <code>null</code>
        /// If the object is not <code>null</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="anObject">The object that is to be tested</param>
        public static void IsNull(this Asserter asserter, object anObject)
        {
            Assert.That(anObject, Is.Null, null, null);
        }

        #endregion

        #region IsNaN

        /// <summary>
        /// Verifies that the double that is passed in is an <code>NaN</code> value.
        /// If the object is not <code>NaN</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="aDouble">The value that is to be tested</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsNaN(this Asserter asserter, double aDouble, string message, params object[] args)
        {
            Assert.That(aDouble, Is.NaN, message, args);
        }

        /// <summary>
        /// Verifies that the double that is passed in is an <code>NaN</code> value.
        /// If the object is not <code>NaN</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="aDouble">The value that is to be tested</param>
        public static void IsNaN(this Asserter asserter, double aDouble)
        {
            Assert.That(aDouble, Is.NaN, null, null);
        }

        /// <summary>
        /// Verifies that the double that is passed in is an <code>NaN</code> value.
        /// If the object is not <code>NaN</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="aDouble">The value that is to be tested</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsNaN(this Asserter asserter, double? aDouble, string message, params object[] args)
        {
            Assert.That(aDouble, Is.NaN, message, args);
        }

        /// <summary>
        /// Verifies that the double that is passed in is an <code>NaN</code> value.
        /// If the object is not <code>NaN</code> then an <see cref="AssertionException"/>
        /// is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="aDouble">The value that is to be tested</param>
        public static void IsNaN(this Asserter asserter, double? aDouble)
        {
            Assert.That(aDouble, Is.NaN, null, null);
        }

        #endregion

        #region IsEmpty

        #region String

        /// <summary>
        /// Assert that a string is empty - that is equal to string.Empty
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="aString">The string to be tested</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsEmpty(this Asserter asserter, string aString, string message, params object[] args)
            => Assert.IsEmpty(aString, message, args);

        /// <summary>
        /// Assert that a string is empty - that is equal to string.Empty
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="aString">The string to be tested</param>
        public static void IsEmpty(this Asserter asserter, string aString)
            => Assert.IsEmpty(aString);

        #endregion

        #region Collection

        /// <summary>
        /// Assert that an array, list or other collection is empty
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="collection">An array, list or other collection implementing ICollection</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsEmpty(this Asserter asserter, IEnumerable collection, string message, params object[] args)
            => Assert.IsEmpty(collection, message, args);

        /// <summary>
        /// Assert that an array, list or other collection is empty
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="collection">An array, list or other collection implementing ICollection</param>
        public static void IsEmpty(this Asserter asserter, IEnumerable collection)
            => Assert.IsEmpty(collection);

        #endregion

        #endregion

        #region IsNotEmpty

        #region String

        /// <summary>
        /// Assert that a string is not empty - that is not equal to string.Empty
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="aString">The string to be tested</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsNotEmpty(this Asserter asserter, string aString, string message, params object[] args)
        {
            Assert.That(aString, Is.Not.Empty, message, args);
        }

        /// <summary>
        /// Assert that a string is not empty - that is not equal to string.Empty
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="aString">The string to be tested</param>
        public static void IsNotEmpty(this Asserter asserter, string aString)
        {
            Assert.That(aString, Is.Not.Empty, null, null);
        }

        #endregion

        #region Collection

        /// <summary>
        /// Assert that an array, list or other collection is not empty
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="collection">An array, list or other collection implementing ICollection</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void IsNotEmpty(this Asserter asserter, IEnumerable collection, string message, params object[] args)
        {
            Assert.That(collection, Is.Not.Empty, message, args);
        }

        /// <summary>
        /// Assert that an array, list or other collection is not empty
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="collection">An array, list or other collection implementing ICollection</param>
        public static void IsNotEmpty(this Asserter asserter, IEnumerable collection)
        {
            Assert.That(collection, Is.Not.Empty, null, null);
        }

        #endregion

        #endregion

        #region Zero

        #region Ints

        /// <summary>
        /// Asserts that an int is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Zero(this Asserter asserter, int actual)
        {
            Assert.That(actual, Is.Zero);
        }

        /// <summary>
        /// Asserts that an int is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Zero(this Asserter asserter, int actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Zero, message, args);
        }

        #endregion

        #region UnsignedInts

        /// <summary>
        /// Asserts that an unsigned int is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Zero(this Asserter asserter, uint actual)
        {
            Assert.That(actual, Is.Zero);
        }

        /// <summary>
        /// Asserts that an unsigned int is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Zero(this Asserter asserter, uint actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Zero, message, args);
        }

        #endregion

        #region Longs

        /// <summary>
        /// Asserts that a Long is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Zero(this Asserter asserter, long actual)
        {
            Assert.That(actual, Is.Zero);
        }

        /// <summary>
        /// Asserts that a Long is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Zero(this Asserter asserter, long actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Zero, message, args);
        }

        #endregion

        #region UnsignedLongs

        /// <summary>
        /// Asserts that an unsigned Long is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Zero(this Asserter asserter, ulong actual)
        {
            Assert.That(actual, Is.Zero);
        }

        /// <summary>
        /// Asserts that an unsigned Long is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Zero(this Asserter asserter, ulong actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Zero, message, args);
        }

        #endregion

        #region Decimals

        /// <summary>
        /// Asserts that a decimal is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Zero(this Asserter asserter, decimal actual)
        {
            Assert.That(actual, Is.Zero);
        }

        /// <summary>
        /// Asserts that a decimal is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Zero(this Asserter asserter, decimal actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Zero, message, args);
        }

        #endregion

        #region Doubles

        /// <summary>
        /// Asserts that a double is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Zero(this Asserter asserter, double actual)
        {
            Assert.That(actual, Is.Zero);
        }

        /// <summary>
        /// Asserts that a double is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Zero(this Asserter asserter, double actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Zero, message, args);
        }

        #endregion

        #region Floats

        /// <summary>
        /// Asserts that a float is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Zero(this Asserter asserter, float actual)
        {
            Assert.That(actual, Is.Zero);
        }

        /// <summary>
        /// Asserts that a float is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Zero(this Asserter asserter, float actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Zero, message, args);
        }

        #endregion

        #endregion

        #region NotZero

        #region Ints

        /// <summary>
        /// Asserts that an int is not zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void NotZero(this Asserter asserter, int actual)
        {
            Assert.That(actual, Is.Not.Zero);
        }

        /// <summary>
        /// Asserts that an int is not zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void NotZero(this Asserter asserter, int actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.Zero, message, args);
        }

        #endregion

        #region UnsignedInts

        /// <summary>
        /// Asserts that an unsigned int is not zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void NotZero(this Asserter asserter, uint actual)
        {
            Assert.That(actual, Is.Not.Zero);
        }

        /// <summary>
        /// Asserts that an unsigned int is not zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void NotZero(this Asserter asserter, uint actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.Zero, message, args);
        }

        #endregion

        #region Longs

        /// <summary>
        /// Asserts that a Long is not zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void NotZero(this Asserter asserter, long actual)
        {
            Assert.That(actual, Is.Not.Zero);
        }

        /// <summary>
        /// Asserts that a Long is not zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void NotZero(this Asserter asserter, long actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.Zero, message, args);
        }

        #endregion

        #region UnsignedLongs

        /// <summary>
        /// Asserts that an unsigned Long is not zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void NotZero(this Asserter asserter, ulong actual)
        {
            Assert.That(actual, Is.Not.Zero);
        }

        /// <summary>
        /// Asserts that an unsigned Long is not zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void NotZero(this Asserter asserter, ulong actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.Zero, message, args);
        }

        #endregion

        #region Decimals

        /// <summary>
        /// Asserts that a decimal is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void NotZero(this Asserter asserter, decimal actual)
        {
            Assert.That(actual, Is.Not.Zero);
        }

        /// <summary>
        /// Asserts that a decimal is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void NotZero(this Asserter asserter, decimal actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.Zero, message, args);
        }

        #endregion

        #region Doubles

        /// <summary>
        /// Asserts that a double is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void NotZero(this Asserter asserter, double actual)
        {
            Assert.That(actual, Is.Not.Zero);
        }

        /// <summary>
        /// Asserts that a double is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void NotZero(this Asserter asserter, double actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.Zero, message, args);
        }

        #endregion

        #region Floats

        /// <summary>
        /// Asserts that a float is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void NotZero(this Asserter asserter, float actual)
        {
            Assert.That(actual, Is.Not.Zero);
        }

        /// <summary>
        /// Asserts that a float is zero.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void NotZero(this Asserter asserter, float actual, string message, params object[] args)
        {
            Assert.That(actual, Is.Not.Zero, message, args);
        }

        #endregion

        #endregion

        #region Positive

        #region Ints

        /// <summary>
        /// Asserts that an int is positive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Positive(this Asserter asserter, int actual)
            => Assert.Positive(actual);

        /// <summary>
        /// Asserts that an int is positive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Positive(this Asserter asserter, int actual, string message, params object[] args)
            => Assert.Positive(actual, message, args);

        #endregion

        #region UnsignedInts

        /// <summary>
        /// Asserts that an unsigned int is positive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Positive(this Asserter asserter, uint actual)
            => Assert.Positive(actual);

        /// <summary>
        /// Asserts that an unsigned int is positive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Positive(this Asserter asserter, uint actual, string message, params object[] args)
            => Assert.Positive(actual, message, args);

        #endregion

        #region Longs

        /// <summary>
        /// Asserts that a Long is positive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Positive(this Asserter asserter, long actual)
            => Assert.Positive(actual);

        /// <summary>
        /// Asserts that a Long is positive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Positive(this Asserter asserter, long actual, string message, params object[] args)
            => Assert.Positive(actual, message, args);

        #endregion

        #region UnsignedLongs

        /// <summary>
        /// Asserts that an unsigned Long is positive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Positive(this Asserter asserter, ulong actual)
            => Assert.Positive(actual);

        /// <summary>
        /// Asserts that an unsigned Long is positive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Positive(this Asserter asserter, ulong actual, string message, params object[] args)
            => Assert.Positive(actual, message, args);

        #endregion

        #region Decimals

        /// <summary>
        /// Asserts that a decimal is positive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Positive(this Asserter asserter, decimal actual)
            => Assert.Positive(actual);

        /// <summary>
        /// Asserts that a decimal is positive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Positive(this Asserter asserter, decimal actual, string message, params object[] args)
            => Assert.Positive(actual, message, args);

        #endregion

        #region Doubles

        /// <summary>
        /// Asserts that a double is positive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Positive(this Asserter asserter, double actual)
            => Assert.Positive(actual);

        /// <summary>
        /// Asserts that a double is positive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Positive(this Asserter asserter, double actual, string message, params object[] args)
            => Assert.Positive(actual, message, args);

        #endregion

        #region Floats

        /// <summary>
        /// Asserts that a float is positive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Positive(this Asserter asserter, float actual)
            => Assert.Positive(actual);

        /// <summary>
        /// Asserts that a float is positive.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Positive(this Asserter asserter, float actual, string message, params object[] args)
            => Assert.Positive(actual, message, args);

        #endregion

        #endregion

        #region Negative

        #region Ints

        /// <summary>
        /// Asserts that an int is negative.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Negative(this Asserter asserter, int actual)
            => Assert.Negative(actual);

        /// <summary>
        /// Asserts that an int is negative.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Negative(this Asserter asserter, int actual, string message, params object[] args)
            => Assert.Negative(actual, message, args);

        #endregion

        #region UnsignedInts

        /// <summary>
        /// Asserts that an unsigned int is negative.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Negative(this Asserter asserter, uint actual)
            => Assert.Negative(actual);

        /// <summary>
        /// Asserts that an unsigned int is negative.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Negative(this Asserter asserter, uint actual, string message, params object[] args)
            => Assert.Negative(actual, message,args);

        #endregion

        #region Longs

        /// <summary>
        /// Asserts that a Long is negative.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Negative(this Asserter asserter, long actual)
            => Assert.Negative(actual);

        /// <summary>
        /// Asserts that a Long is negative.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Negative(this Asserter asserter, long actual, string message, params object[] args)
            => Assert.Negative(actual, message, args);

        #endregion

        #region UnsignedLongs

        /// <summary>
        /// Asserts that an unsigned Long is negative.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Negative(this Asserter asserter, ulong actual)
            => Assert.Negative(actual);

        /// <summary>
        /// Asserts that an unsigned Long is negative.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Negative(this Asserter asserter, ulong actual, string message, params object[] args)
            => Assert.Negative(actual, message, args);

        #endregion

        #region Decimals

        /// <summary>
        /// Asserts that a decimal is negative.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Negative(this Asserter asserter, decimal actual)
            => Assert.Negative(actual);

        /// <summary>
        /// Asserts that a decimal is negative.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Negative(this Asserter asserter, decimal actual, string message, params object[] args)
            => Assert.Negative(actual, message, args);

        #endregion

        #region Doubles

        /// <summary>
        /// Asserts that a double is negative.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Negative(this Asserter asserter, double actual)
            => Assert.Negative(actual);

        /// <summary>
        /// Asserts that a double is negative.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Negative(this Asserter asserter, double actual, string message, params object[] args)
            => Assert.Negative(actual, message, args);

        #endregion

        #region Floats

        /// <summary>
        /// Asserts that a float is negative.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        public static void Negative(this Asserter asserter, float actual)
            => Assert.Negative(actual);

        /// <summary>
        /// Asserts that a float is negative.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The number to be examined</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Negative(this Asserter asserter, float actual, string message, params object[] args)
            => Assert.Negative(actual, message, args);

        #endregion

        #endregion
    }
}
