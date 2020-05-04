using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

// ReSharper disable UnusedMember.Global
#pragma warning disable IDE0060 // Remove unused parameter

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides extension methods to the <see cref="Asserter"/> class,
    /// corresponding to methods in NUnit 'Assert.Equality.cs' source file.
    /// </summary>
    public static class AsserterExceptionsExtensions
    {
        // TODO: finish rewriting all methods as => Assert...

        #region Throws

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="expression">A constraint to be satisfied by the exception</param>
        /// <param name="code">A TestSnippet delegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static Exception Throws(this Asserter asserter, IResolveConstraint expression, TestDelegate code, string message, params object[] args)
            => Assert.Throws(expression, code, message, args);

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="expression">A constraint to be satisfied by the exception</param>
        /// <param name="code">A TestSnippet delegate</param>
        public static Exception Throws(this Asserter asserter, IResolveConstraint expression, TestDelegate code)
        {
            return Throws(asserter, expression, code, string.Empty, null);
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="expectedExceptionType">The exception Type expected</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static Exception Throws(this Asserter asserter, Type expectedExceptionType, TestDelegate code, string message, params object[] args)
        {
            return Throws(asserter, new ExceptionTypeConstraint(expectedExceptionType), code, message, args);
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="expectedExceptionType">The exception Type expected</param>
        /// <param name="code">A TestDelegate</param>
        public static Exception Throws(this Asserter asserter, Type expectedExceptionType, TestDelegate code)
        {
            return Throws(asserter, new ExceptionTypeConstraint(expectedExceptionType), code, string.Empty, null);
        }

        #endregion

        #region Throws<TActual>

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <typeparam name="TActual">Type of the expected exception</typeparam>
        /// <param name="asserter">Asserter.</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static TActual Throws<TActual>(this Asserter asserter, TestDelegate code, string message, params object[] args) where TActual : Exception
        {
            return (TActual)Throws(asserter, typeof(TActual), code, message, args);
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception when called.
        /// </summary>
        /// <typeparam name="TActual">Type of the expected exception</typeparam>
        /// <param name="asserter">Asserter.</param>
        /// <param name="code">A TestDelegate</param>
        public static TActual Throws<TActual>(this Asserter asserter, TestDelegate code) where TActual : Exception
        {
            return Throws<TActual>(asserter, code, string.Empty, null);
        }

        #endregion

        #region Catch
        /// <summary>
        /// Verifies that a delegate throws an exception when called
        /// and returns it.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static Exception Catch(this Asserter asserter, TestDelegate code, string message, params object[] args)
        {
            return Throws(asserter, new InstanceOfTypeConstraint(typeof(Exception)), code, message, args);
        }

        /// <summary>
        /// Verifies that a delegate throws an exception when called
        /// and returns it.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="code">A TestDelegate</param>
        public static Exception Catch(this Asserter asserter, TestDelegate code)
        {
            return Throws(asserter, new InstanceOfTypeConstraint(typeof(Exception)), code);
        }

        /// <summary>
        /// Verifies that a delegate throws an exception of a certain Type
        /// or one derived from it when called and returns it.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="expectedExceptionType">The expected Exception Type</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static Exception Catch(this Asserter asserter, Type expectedExceptionType, TestDelegate code, string message, params object[] args)
        {
            return Throws(asserter, new InstanceOfTypeConstraint(expectedExceptionType), code, message, args);
        }

        /// <summary>
        /// Verifies that a delegate throws an exception of a certain Type
        /// or one derived from it when called and returns it.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="expectedExceptionType">The expected Exception Type</param>
        /// <param name="code">A TestDelegate</param>
        public static Exception Catch(this Asserter asserter, Type expectedExceptionType, TestDelegate code)
        {
            return Throws(asserter, new InstanceOfTypeConstraint(expectedExceptionType), code);
        }
        #endregion

        #region Catch<TActual>

        /// <summary>
        /// Verifies that a delegate throws an exception of a certain Type
        /// or one derived from it when called and returns it.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static TActual Catch<TActual>(this Asserter asserter, TestDelegate code, string message, params object[] args) where TActual : System.Exception
        {
            return (TActual)Throws(asserter, new InstanceOfTypeConstraint(typeof(TActual)), code, message, args);
        }

        /// <summary>
        /// Verifies that a delegate throws an exception of a certain Type
        /// or one derived from it when called and returns it.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="code">A TestDelegate</param>
        public static TActual Catch<TActual>(this Asserter asserter, TestDelegate code) where TActual : System.Exception
        {
            return (TActual)Throws(asserter, new InstanceOfTypeConstraint(typeof(TActual)), code);
        }

        #endregion

        #region DoesNotThrow

        /// <summary>
        /// Verifies that a delegate does not throw an exception
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="code">A TestDelegate</param>
        /// <param name="message">The message that will be displayed on failure</param>
        /// <param name="args">Arguments to be used in formatting the message</param>
        public static void DoesNotThrow(this Asserter asserter, TestDelegate code, string message, params object[] args)
        {
            Assert.That(code, new ThrowsNothingConstraint(), message, args);
        }

        /// <summary>
        /// Verifies that a delegate does not throw an exception.
        /// </summary>
        /// <param name="asserter">Asserter.</param>
        /// <param name="code">A TestDelegate</param>
        public static void DoesNotThrow(this Asserter asserter, TestDelegate code)
        {
            DoesNotThrow(asserter, code, string.Empty, null);
        }

        #endregion
    }
}
