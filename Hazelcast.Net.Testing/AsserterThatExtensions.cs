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
using NUnit.Framework.Constraints;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides extension methods to the <see cref="Asserter"/> class,
    /// corresponding to methods in NUnit 'Assert.Equality.cs' source file.
    /// </summary>
    public static class AsserterThatExtensions
    {
        // TODO: finish rewriting all methods as => Assert...

        #region Assert.That

        #region Boolean

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        /// <param name="message">The message to display if the condition is false</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static void That(this Asserter asserter, bool condition, string message, params object[] args)
        {
            Assert.That(condition, Is.True, message, args);
        }

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        public static void That(this Asserter asserter, bool condition)
        {
            Assert.That(condition, Is.True, null, null);
        }

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">The evaluated condition</param>
        /// <param name="getExceptionMessage">A function to build the message included with the Exception</param>
        public static void That(this Asserter asserter, bool condition, Func<string> getExceptionMessage)
        {
            Assert.That(condition, Is.True, getExceptionMessage);
        }

        #endregion

        #region Lambda returning Boolean

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">A lambda that returns a Boolean</param>
        /// <param name="message">The message to display if the condition is false</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static void That(this Asserter asserter, Func<bool> condition, string message, params object[] args)
        {
            Assert.That(condition.Invoke(), Is.True, message, args);
        }

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">A lambda that returns a Boolean</param>
        public static void That(this Asserter asserter, Func<bool> condition)
        {
            Assert.That(condition.Invoke(), Is.True, null, null);
        }

        /// <summary>
        /// Asserts that a condition is true. If the condition is false the method throws
        /// an <see cref="AssertionException"/>.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="condition">A lambda that returns a Boolean</param>
        /// <param name="getExceptionMessage">A function to build the message included with the Exception</param>
        public static void That(this Asserter asserter, Func<bool> condition, Func<string> getExceptionMessage)
        {
            Assert.That(condition.Invoke(), Is.True, getExceptionMessage);
        }

        #endregion

        #region ActualValueDelegate

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <typeparam name="TActual">The Type being compared.</typeparam>
        /// <param name="asserter">Asserter.</param>
        /// <param name="del">An ActualValueDelegate returning the value to be tested</param>
        /// <param name="expr">A Constraint expression to be applied</param>
        public static void That<TActual>(this Asserter asserter, ActualValueDelegate<TActual> del, IResolveConstraint expr)
        {
            Assert.That(del, expr.Resolve(), null, null);
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <typeparam name="TActual">The Type being compared.</typeparam>
        /// <param name="asserter">Asserter.</param>
        /// <param name="del">An ActualValueDelegate returning the value to be tested</param>
        /// <param name="expr">A Constraint expression to be applied</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static void That<TActual>(this Asserter asserter, ActualValueDelegate<TActual> del, IResolveConstraint expr, string message, params object[] args)
            => Assert.That(del, expr, message, args);

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <typeparam name="TActual">The Type being compared.</typeparam>
        /// <param name="asserter">Asserter.</param>
        /// <param name="del">An ActualValueDelegate returning the value to be tested</param>
        /// <param name="expr">A Constraint expression to be applied</param>
        /// <param name="getExceptionMessage">A function to build the message included with the Exception</param>
        public static void That<TActual>(this Asserter asserter, ActualValueDelegate<TActual> del, IResolveConstraint expr, Func<string> getExceptionMessage)
            => Assert.That(del, expr, getExceptionMessage);

        #endregion

        #region TestDelegate

        /// <summary>
        /// Asserts that the code represented by a delegate throws an exception
        /// that satisfies the constraint provided.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="code">A TestDelegate to be executed</param>
        /// <param name="constraint">A Constraint expression to be applied</param>
        public static void That(this Asserter asserter, TestDelegate code, IResolveConstraint constraint)
        {
            Assert.That(code, constraint, null, null);
        }

        /// <summary>
        /// Asserts that the code represented by a delegate throws an exception
        /// that satisfies the constraint provided.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="code">A TestDelegate to be executed</param>
        /// <param name="constraint">A Constraint expression to be applied</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static void That(this Asserter asserter, TestDelegate code, IResolveConstraint constraint, string message, params object[] args)
        {
            Assert.That((object)code, constraint, message, args);
        }

        /// <summary>
        /// Asserts that the code represented by a delegate throws an exception
        /// that satisfies the constraint provided.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="code">A TestDelegate to be executed</param>
        /// <param name="constraint">A Constraint expression to be applied</param>
        /// <param name="getExceptionMessage">A function to build the message included with the Exception</param>
        public static void That(this Asserter asserter, TestDelegate code, IResolveConstraint constraint, Func<string> getExceptionMessage)
        {
            Assert.That((object)code, constraint, getExceptionMessage);
        }

        #endregion

        #endregion

        #region Assert.That<TActual>

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <typeparam name="TActual">The Type being compared.</typeparam>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The actual value to test</param>
        /// <param name="expression">A Constraint expression to be applied</param>
        public static void That<TActual>(this Asserter asserter, TActual actual, IResolveConstraint expression)
        {
            Assert.That(actual, expression, null, null);
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <typeparam name="TActual">The Type being compared.</typeparam>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The actual value to test</param>
        /// <param name="expression">A Constraint expression to be applied</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static void That<TActual>(this Asserter asserter, TActual actual, IResolveConstraint expression, string message, params object[] args)
            => Assert.That(actual, expression, message, args);

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// </summary>
        /// <typeparam name="TActual">The Type being compared.</typeparam>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The actual value to test</param>
        /// <param name="expression">A Constraint expression to be applied</param>
        /// <param name="getExceptionMessage">A function to build the message included with the Exception</param>
        public static void That<TActual>(this Asserter asserter, TActual actual, IResolveConstraint expression, Func<string> getExceptionMessage)
            => Assert.That(actual, expression, getExceptionMessage);

        #endregion

        #region Assert.ByVal

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// Used as a synonym for That in rare cases where a private setter
        /// causes a Visual Basic compilation error.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The actual value to test</param>
        /// <param name="expression">A Constraint expression to be applied</param>
        public static void ByVal(this Asserter asserter, object actual, IResolveConstraint expression)
        {
            Assert.That(actual, expression, null, null);
        }

        /// <summary>
        /// Apply a constraint to an actual value, succeeding if the constraint
        /// is satisfied and throwing an assertion exception on failure.
        /// Used as a synonym for That in rare cases where a private setter
        /// causes a Visual Basic compilation error.
        /// </summary>
        /// <remarks>
        /// This method is provided for use by VB developers needing to test
        /// the value of properties with private setters.
        /// </remarks>
        /// <param name="asserter">Asserter.</param>
        /// <param name="actual">The actual value to test</param>
        /// <param name="expression">A Constraint expression to be applied</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static void ByVal(this Asserter asserter, object actual, IResolveConstraint expression, string message, params object[] args)
        {
            Assert.That(actual, expression, message, args);
        }

        #endregion
    }
}