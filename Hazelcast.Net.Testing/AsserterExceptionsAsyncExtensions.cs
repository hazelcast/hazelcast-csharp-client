using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

// ReSharper disable UnusedMember.Global
#pragma warning disable IDE0060 // Remove unused parameter

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides extension methods to the <see cref="Asserter"/> class,
    /// corresponding to methods in NUnit 'Assert.Exceptions.Async.cs' source file.
    /// </summary>
    public static class AsserterExceptionsAsyncExtensions
    {
        // TODO: finish rewriting all methods as => Assert...

        #region ThrowsAsync

        /// <summary>
        /// Verifies that an async delegate throws a particular exception when called.
        /// </summary>
        /// <param name="expression">A constraint to be satisfied by the exception</param>
        /// <param name="code">A TestSnippet delegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static Exception ThrowsAsync(this Asserter asserter, IResolveConstraint expression, AsyncTestDelegate code, string message, params object[] args)
            => Assert.ThrowsAsync(expression, code, message, args);

        /// <summary>
        /// Verifies that an async delegate throws a particular exception when called.
        /// </summary>
        /// <param name="expression">A constraint to be satisfied by the exception</param>
        /// <param name="code">A TestSnippet delegate</param>
        public static Exception ThrowsAsync(this Asserter asserter, IResolveConstraint expression, AsyncTestDelegate code)
            => Assert.ThrowsAsync(expression, code);

        /// <summary>
        /// Verifies that an async delegate throws a particular exception when called.
        /// </summary>
        /// <param name="expectedExceptionType">The exception Type expected</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static Exception ThrowsAsync(this Asserter asserter, Type expectedExceptionType, AsyncTestDelegate code, string message, params object[] args)
            => Assert.ThrowsAsync(expectedExceptionType, code, message, args);

        /// <summary>
        /// Verifies that an async delegate throws a particular exception when called.
        /// </summary>
        /// <param name="expectedExceptionType">The exception Type expected</param>
        /// <param name="code">A TestDelegate</param>
        public static Exception ThrowsAsync(this Asserter asserter, Type expectedExceptionType, AsyncTestDelegate code)
            => Assert.ThrowsAsync(expectedExceptionType, code);

        #endregion

        #region ThrowsAsync<TActual>

        /// <summary>
        /// Verifies that an async delegate throws a particular exception when called.
        /// </summary>
        /// <typeparam name="TActual">Type of the expected exception</typeparam>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static TActual ThrowsAsync<TActual>(this Asserter asserter, AsyncTestDelegate code, string message, params object[] args) where TActual : Exception
            => Assert.ThrowsAsync<TActual>(code, message, args);

        /// <summary>
        /// Verifies that an async delegate throws a particular exception when called.
        /// </summary>
        /// <typeparam name="TActual">Type of the expected exception</typeparam>
        /// <param name="code">A TestDelegate</param>
        public static TActual ThrowsAsync<TActual>(this Asserter asserter, AsyncTestDelegate code) where TActual : Exception
            => Assert.ThrowsAsync<TActual>(code);

        #endregion

        #region CatchAsync

        /// <summary>
        /// Verifies that an async delegate throws an exception when called
        /// and returns it.
        /// </summary>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static Exception CatchAsync(this Asserter asserter, AsyncTestDelegate code, string message, params object[] args)
            => Assert.CatchAsync(code, message, args);

        /// <summary>
        /// Verifies that an async delegate throws an exception when called
        /// and returns it.
        /// </summary>
        /// <param name="code">A TestDelegate</param>
        public static Exception CatchAsync(this Asserter asserter, AsyncTestDelegate code)
            => Assert.CatchAsync(code);

        /// <summary>
        /// Verifies that an async delegate throws an exception of a certain Type
        /// or one derived from it when called and returns it.
        /// </summary>
        /// <param name="expectedExceptionType">The expected Exception Type</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static Exception CatchAsync(this Asserter asserter, Type expectedExceptionType, AsyncTestDelegate code, string message, params object[] args)
            => Assert.CatchAsync(expectedExceptionType, code, message, args);

        /// <summary>
        /// Verifies that an async delegate throws an exception of a certain Type
        /// or one derived from it when called and returns it.
        /// </summary>
        /// <param name="expectedExceptionType">The expected Exception Type</param>
        /// <param name="code">A TestDelegate</param>
        public static Exception CatchAsync(this Asserter asserter, Type expectedExceptionType, AsyncTestDelegate code)
            => Assert.ThrowsAsync(expectedExceptionType, code);

        #endregion

        #region CatchAsync<TActual>

        /// <summary>
        /// Verifies that an async delegate throws an exception of a certain Type
        /// or one derived from it when called and returns it.
        /// </summary>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static TActual CatchAsync<TActual>(this Asserter asserter, AsyncTestDelegate code, string message, params object[] args) where TActual : Exception
            => Assert.CatchAsync<TActual>(code, message, args);

        /// <summary>
        /// Verifies that an async delegate throws an exception of a certain Type
        /// or one derived from it when called and returns it.
        /// </summary>
        /// <param name="code">A TestDelegate</param>
        public static TActual CatchAsync<TActual>(this Asserter asserter, AsyncTestDelegate code) where TActual : Exception
            => Assert.CatchAsync<TActual>(code);

        #endregion

        #region DoesNotThrowAsync

        /// <summary>
        /// Verifies that an async delegate does not throw an exception
        /// </summary>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static void DoesNotThrowAsync(this Asserter asserter, AsyncTestDelegate code, string message, params object[] args)
        {
            Assert.That(code, new ThrowsNothingConstraint(), message, args);
        }

        /// <summary>
        /// Verifies that an async delegate does not throw an exception.
        /// </summary>
        /// <param name="code">A TestDelegate</param>
        public static void DoesNotThrowAsync(this Asserter asserter, AsyncTestDelegate code)
            => Assert.DoesNotThrowAsync(code);

        #endregion

    }
}
