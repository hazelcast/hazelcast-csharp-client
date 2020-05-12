using System;

namespace Hazelcast.Core
{
    /// <summary>
    /// Creates instances of the <see cref="Attempt{TResult}"/> struct.
    /// </summary>
    public readonly struct Attempt
    {
        /// <summary>
        /// Represents a failed attempt.
        /// </summary>
        public static Attempt Failed { get; } = new Attempt();

        /// <summary>
        /// Creates a successful attempt with a result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="value">The value of the result.</param>
        /// <returns>A successful attempt.</returns>
        public static Attempt<TResult> Succeed<TResult>(TResult value)
            => new Attempt<TResult>(true, value);

        /// <summary>
        /// Creates a failed attempt.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="value">The value of the result.</param>
        /// <param name="exception">An optional captured exception.</param>
        /// <returns>A failed attempt.</returns>
        public static Attempt<TResult> Fail<TResult>(TResult value, Exception exception = default)
            => new Attempt<TResult>(false, value, exception);

        /// <summary>
        /// Creates a failed attempt.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="exception">An optional captured exception.</param>
        /// <returns>A failed attempt.</returns>
        public static Attempt<TResult> Fail<TResult>(Exception exception = default)
            => new Attempt<TResult>(false, exception: exception);
    }

    /// <summary>
    /// Represents the result of attempting an operation.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public readonly struct Attempt<TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Attempt{TResult}"/> struct.
        /// </summary>
        /// <param name="success">Whether the attempt succeeded.</param>
        /// <param name="value">The optional value of the result.</param>
        /// <param name="exception">An optional captured exception.</param>
        internal Attempt(bool success, TResult value = default, Exception exception = default)
        {
            Success = success;
            Value = value;
            Exception = exception;
        }

        /// <summary>
        /// Represents a failed attempt with no result and no exception.
        /// </summary>
        public static Attempt<TResult> Failed { get; } = new Attempt<TResult>(false);

        /// <summary>
        /// Gets a value indicating whether the attempt succeeded.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the value of the result.
        /// </summary>
        public TResult Value { get; }

        /// <summary>
        /// Gets the value of the result, if successful, else another value.
        /// </summary>
        /// <param name="other">The other value.</param>
        /// <returns>The value of the result, if successful, else the specified value.</returns>
        public TResult ValueOr(TResult other)
            => Success ? Value : other;

        /// <summary>
        /// Gets a captured exception.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets a value indicating whether the attempt contains an exception.
        /// </summary>
        public bool HasException => Exception != null;

        /// <summary>
        /// Implicitly converts an attempt into a boolean.
        /// </summary>
        /// <param name="attempt">The attempt.</param>
        public static implicit operator bool(Attempt<TResult> attempt)
            => attempt.Success;

        /// <summary>
        /// Implicitly converts an attempt into its result.
        /// </summary>
        /// <param name="attempt">The attempt.</param>
        public static implicit operator TResult(Attempt<TResult> attempt)
            => attempt.Value;

        /// <summary>
        /// Implicitly converts a non-generic attempt into a generic one.
        /// </summary>
        /// <param name="attempt">The attempt.</param>
        public static implicit operator Attempt<TResult>(Attempt attempt)
            => new Attempt<TResult>();

        /// <summary>
        /// Implicitly converts a result value into a successful attempts.
        /// </summary>
        /// <param name="result">The result value.</param>
        public static implicit operator Attempt<TResult>(TResult result)
            => new Attempt<TResult>(true, result);

        // NOTE
        //
        // there is no way to return Attempt.Fail(exception) and have it implicitly
        // converted to a new Attempt<TResult> as that would allocate first the non-
        // generic and second the generic attempt, and we want to avoid this
    }
}
