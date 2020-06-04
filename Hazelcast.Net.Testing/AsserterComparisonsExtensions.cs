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
using NUnit.Framework;

// ReSharper disable UnusedMember.Global
#pragma warning disable IDE0060 // Remove unused parameter

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides extension methods to the <see cref="Asserter"/> class,
    /// corresponding to methods in NUnit 'Assert.Comparisons.cs' source file.
    /// </summary>
    public static class AsserterComparisonsExtensions
    {
        // TODO: finish rewriting all methods as => Assert...

        #region Greater

        #region Ints

        /// <summary>
        /// Verifies that the first int is greater than the second
        /// int. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Greater(this Asserter asserter, int arg1, int arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.GreaterThan(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first int is greater than the second
        /// int. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        public static void Greater(this Asserter asserter, int arg1, int arg2)
        {
            Assert.That(arg1, Is.GreaterThan(arg2), null, null);
        }

        #endregion

        #region Unsigned Ints

        /// <summary>
        /// Verifies that the first value is greater than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Greater(this Asserter asserter, uint arg1, uint arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.GreaterThan(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is greater than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        public static void Greater(this Asserter asserter, uint arg1, uint arg2)
        {
            Assert.That(arg1, Is.GreaterThan(arg2), null, null);
        }

        #endregion

        #region Longs

        /// <summary>
        /// Verifies that the first value is greater than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Greater(this Asserter asserter, long arg1, long arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.GreaterThan(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is greater than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        public static void Greater(this Asserter asserter, long arg1, long arg2)
        {
            Assert.That(arg1, Is.GreaterThan(arg2), null, null);
        }

        #endregion

        #region Unsigned Longs

        /// <summary>
        /// Verifies that the first value is greater than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Greater(this Asserter asserter, ulong arg1, ulong arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.GreaterThan(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is greater than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        public static void Greater(this Asserter asserter, ulong arg1, ulong arg2)
        {
            Assert.That(arg1, Is.GreaterThan(arg2), null, null);
        }

        #endregion

        #region Decimals

        /// <summary>
        /// Verifies that the first value is greater than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Greater(this Asserter asserter, decimal arg1, decimal arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.GreaterThan(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is greater than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        public static void Greater(this Asserter asserter, decimal arg1, decimal arg2)
        {
            Assert.That(arg1, Is.GreaterThan(arg2), null, null);
        }

        #endregion

        #region Doubles

        /// <summary>
        /// Verifies that the first value is greater than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Greater(this Asserter asserter, double arg1, double arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.GreaterThan(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is greater than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        public static void Greater(this Asserter asserter, double arg1, double arg2)
        {
            Assert.That(arg1, Is.GreaterThan(arg2), null, null);
        }

        #endregion

        #region Floats

        /// <summary>
        /// Verifies that the first value is greater than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Greater(this Asserter asserter, float arg1, float arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.GreaterThan(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is greater than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        public static void Greater(this Asserter asserter, float arg1, float arg2)
        {
            Assert.That(arg1, Is.GreaterThan(arg2), null, null);
        }

        #endregion

        #region IComparables

        /// <summary>
        /// Verifies that the first value is greater than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Greater(this Asserter asserter, IComparable arg1, IComparable arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.GreaterThan(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is greater than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        public static void Greater(this Asserter asserter, IComparable arg1, IComparable arg2)
        {
            Assert.That(arg1, Is.GreaterThan(arg2), null, null);
        }

        #endregion

        #endregion

        #region Less

        #region Ints

        /// <summary>
        /// Verifies that the first value is less than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Less(this Asserter asserter, int arg1, int arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.LessThan(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is less than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        public static void Less(this Asserter asserter, int arg1, int arg2)
        {
            Assert.That(arg1, Is.LessThan(arg2), null, null);
        }

        #endregion

        #region Unsigned Ints

        /// <summary>
        /// Verifies that the first value is less than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Less(this Asserter asserter, uint arg1, uint arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.LessThan(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is less than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        public static void Less(this Asserter asserter, uint arg1, uint arg2)
        {
            Assert.That(arg1, Is.LessThan(arg2), null, null);
        }

        #endregion

        #region Longs

        /// <summary>
        /// Verifies that the first value is less than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Less(this Asserter asserter, long arg1, long arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.LessThan(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is less than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        public static void Less(this Asserter asserter, long arg1, long arg2)
        {
            Assert.That(arg1, Is.LessThan(arg2), null, null);
        }

        #endregion

        #region Unsigned Longs

        /// <summary>
        /// Verifies that the first value is less than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Less(this Asserter asserter, ulong arg1, ulong arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.LessThan(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is less than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        public static void Less(this Asserter asserter, ulong arg1, ulong arg2)
        {
            Assert.That(arg1, Is.LessThan(arg2), null, null);
        }

        #endregion

        #region Decimals

        /// <summary>
        /// Verifies that the first value is less than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Less(this Asserter asserter, decimal arg1, decimal arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.LessThan(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is less than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        public static void Less(this Asserter asserter, decimal arg1, decimal arg2)
        {
            Assert.That(arg1, Is.LessThan(arg2), null, null);
        }

        #endregion

        #region Doubles

        /// <summary>
        /// Verifies that the first value is less than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Less(this Asserter asserter, double arg1, double arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.LessThan(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is less than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        public static void Less(this Asserter asserter, double arg1, double arg2)
        {
            Assert.That(arg1, Is.LessThan(arg2), null, null);
        }

        #endregion

        #region Floats

        /// <summary>
        /// Verifies that the first value is less than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Less(this Asserter asserter, float arg1, float arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.LessThan(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is less than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        public static void Less(this Asserter asserter, float arg1, float arg2)
        {
            Assert.That(arg1, Is.LessThan(arg2), null, null);
        }

        #endregion

        #region IComparables

        /// <summary>
        /// Verifies that the first value is less than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void Less(this Asserter asserter, IComparable arg1, IComparable arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.LessThan(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is less than the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        public static void Less(this Asserter asserter, IComparable arg1, IComparable arg2)
        {
            Assert.That(arg1, Is.LessThan(arg2), null, null);
        }

        #endregion

        #endregion

        #region GreaterOrEqual

        #region Ints

        /// <summary>
        /// Verifies that the first value is greater than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void GreaterOrEqual(this Asserter asserter, int arg1, int arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is greater than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        public static void GreaterOrEqual(this Asserter asserter, int arg1, int arg2)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), null, null);
        }

        #endregion

        #region Unsigned Ints

        /// <summary>
        /// Verifies that the first value is greater than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void GreaterOrEqual(this Asserter asserter, uint arg1, uint arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is greater than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        public static void GreaterOrEqual(this Asserter asserter, uint arg1, uint arg2)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), null, null);
        }

        #endregion

        #region Longs

        /// <summary>
        /// Verifies that the first value is greater than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void GreaterOrEqual(this Asserter asserter, long arg1, long arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is greater than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        public static void GreaterOrEqual(this Asserter asserter, long arg1, long arg2)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), null, null);
        }

        #endregion

        #region Unsigned Longs

        /// <summary>
        /// Verifies that the first value is greater than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void GreaterOrEqual(this Asserter asserter, ulong arg1, ulong arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is greater than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        public static void GreaterOrEqual(this Asserter asserter, ulong arg1, ulong arg2)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), null, null);
        }

        #endregion

        #region Decimals

        /// <summary>
        /// Verifies that the first value is greater than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void GreaterOrEqual(this Asserter asserter, decimal arg1, decimal arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is greater than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        public static void GreaterOrEqual(this Asserter asserter, decimal arg1, decimal arg2)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), null, null);
        }

        #endregion

        #region Doubles

        /// <summary>
        /// Verifies that the first value is greater than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void GreaterOrEqual(this Asserter asserter, double arg1, double arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is greater than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        public static void GreaterOrEqual(this Asserter asserter, double arg1, double arg2)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), null, null);
        }

        #endregion

        #region Floats

        /// <summary>
        /// Verifies that the first value is greater than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void GreaterOrEqual(this Asserter asserter, float arg1, float arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is greater than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        public static void GreaterOrEqual(this Asserter asserter, float arg1, float arg2)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), null, null);
        }

        #endregion

        #region IComparables

        /// <summary>
        /// Verifies that the first value is greater than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void GreaterOrEqual(this Asserter asserter, IComparable arg1, IComparable arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is greater than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="arg1">The first value, expected to be greater</param>
        /// <param name="arg2">The second value, expected to be less</param>
        /// <param name="asserter">Asserter.</param>
        public static void GreaterOrEqual(this Asserter asserter, IComparable arg1, IComparable arg2)
        {
            Assert.That(arg1, Is.GreaterThanOrEqualTo(arg2), null, null);
        }

        #endregion

        #endregion

        #region LessOrEqual

        #region Ints

        /// <summary>
        /// Verifies that the first value is less than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void LessOrEqual(this Asserter asserter, int arg1, int arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is less than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        public static void LessOrEqual(this Asserter asserter, int arg1, int arg2)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2), null, null);
        }

        #endregion

        #region Unsigned Ints

        /// <summary>
        /// Verifies that the first value is less than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void LessOrEqual(this Asserter asserter, uint arg1, uint arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is less than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        public static void LessOrEqual(this Asserter asserter, uint arg1, uint arg2)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2), null, null);
        }

        #endregion

        #region Longs

        /// <summary>
        /// Verifies that the first value is less than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void LessOrEqual(this Asserter asserter, long arg1, long arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is less than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        public static void LessOrEqual(this Asserter asserter, long arg1, long arg2)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2), null, null);
        }

        #endregion

        #region Unsigned Longs

        /// <summary>
        /// Verifies that the first value is less than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void LessOrEqual(this Asserter asserter, ulong arg1, ulong arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is less than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        public static void LessOrEqual(this Asserter asserter, ulong arg1, ulong arg2)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2), null, null);
        }

        #endregion

        #region Decimals

        /// <summary>
        /// Verifies that the first value is less than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void LessOrEqual(this Asserter asserter, decimal arg1, decimal arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is less than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        public static void LessOrEqual(this Asserter asserter, decimal arg1, decimal arg2)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2), null, null);
        }

        #endregion

        #region Doubles

        /// <summary>
        /// Verifies that the first value is less than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void LessOrEqual(this Asserter asserter, double arg1, double arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is less than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        public static void LessOrEqual(this Asserter asserter, double arg1, double arg2)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2), null, null);
        }

        #endregion

        #region Floats

        /// <summary>
        /// Verifies that the first value is less than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void LessOrEqual(this Asserter asserter, float arg1, float arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is less than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        public static void LessOrEqual(this Asserter asserter, float arg1, float arg2)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2), null, null);
        }

        #endregion

        #region IComparables

        /// <summary>
        /// Verifies that the first value is less than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        /// <param name="message">The message to display in case of failure</param>
        /// <param name="args">Array of objects to be used in formatting the message</param>
        public static void LessOrEqual(this Asserter asserter, IComparable arg1, IComparable arg2, string message, params object[] args)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2), message, args);
        }

        /// <summary>
        /// Verifies that the first value is less than or equal to the second
        /// value. If it is not, then an
        /// <see cref="AssertionException"/> is thrown.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="arg1">The first value, expected to be less</param>
        /// <param name="arg2">The second value, expected to be greater</param>
        public static void LessOrEqual(this Asserter asserter, IComparable arg1, IComparable arg2)
        {
            Assert.That(arg1, Is.LessThanOrEqualTo(arg2), null, null);
        }

        #endregion

        #endregion
    }
}
