﻿using NUnit.Framework.Constraints;

namespace Hazelcast.Testing
{
    public static class Resolves
    {
        public static EquatableConstraint Equatable(object equal, params object[] different)
            => new EquatableConstraint(equal, different);
    }

    public class EquatableResult : ConstraintResult
    {
        private readonly string _message;

        private EquatableResult(IConstraint constraint, object actualValue)
            : base(constraint, actualValue, true)
        { }

        private EquatableResult(IConstraint constraint, object actualValue, string message)
            : base(constraint, actualValue, false)
        {
            _message = message;
        }

        public static EquatableResult Fail(IConstraint constraint, object actualValue, string message)
            => new EquatableResult(constraint, actualValue, message);

        public static EquatableResult Success(IConstraint constraint, object actualValue)
            => new EquatableResult(constraint, actualValue);

        public override void WriteMessageTo(MessageWriter writer)
        {
            writer.Write(_message);
        }
    }

    public class EquatableConstraint : Constraint // IResolveConstraint
    {
        private readonly object _equal;
        private readonly object[] _different;

        public EquatableConstraint(object equal, object[] different)
        {
            _equal = equal;
            _different = different;
        }

        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            // uses the overloaded TActual.Equals() methods

            if (!actual.Equals(actual)) return EquatableResult.Fail(this, actual, "'x.Equals(x)' should not be false");
            if (actual.Equals(null)) return EquatableResult.Fail(this, actual, "'x.Equals(null)' should not be true");
            if (!actual.Equals(_equal)) return EquatableResult.Fail(this, actual, "'x.Equals(equal)' should not be false");

            foreach (var x in _different)
                if (actual.Equals(x)) return EquatableResult.Fail(this, actual, "'x.Equals(different)' should not be true");

            // the compiler has no way to know that T overrides ==
            // so that would use object == only and fail
            // we need to use dynamic to force late binding
            //Assert.That(x == otherEqual);

            dynamic dActual = actual;

            if (dActual == null) return EquatableResult.Fail(this, actual, "'x == null' should not be true");
            if (!(dActual != null)) return EquatableResult.Fail(this, actual, "'x != null' should not be false");
            if (!(null != dActual)) return EquatableResult.Fail(this, actual, "'null != x' should not be false");
            if (!(dActual == dActual)) return EquatableResult.Fail(this, actual, "'x == x' should not be false");

            dynamic dEqual = _equal;
            if (dActual != dEqual) return EquatableResult.Fail(this, actual, "'x != equal' should not be true");
            if (!(dActual == dEqual)) return EquatableResult.Fail(this, actual, "'x == equal' should not be false");

            foreach (dynamic dDifferent in _different)
            {
                if (dActual == dDifferent) return EquatableResult.Fail(this, actual, "'x == different' should not be true");
                if (!(dActual != dDifferent)) return EquatableResult.Fail(this, actual, "'x != different' should not be false");
            }

            // success
            return EquatableResult.Success(this, actual);
        }
    }
}
