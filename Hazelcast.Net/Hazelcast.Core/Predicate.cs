// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Text;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Core
{
    /// <summary>
    ///     Helper mothod for creating builtin predicates
    /// </summary>
    public static class Predicates
    {
        private const string KeyConst = "__key";
        private const string ThisConst = "this";

        public static PredicateProperty Key(string property = null)
        {
            return new PredicateProperty(property != null ? KeyConst + "#" + property : KeyConst);
        }

        public static PredicateProperty Property(string property)
        {
            return new PredicateProperty(property);
        }

        public static PredicateProperty This()
        {
            return new PredicateProperty(ThisConst);
        }

        public static InstanceofPredicate InstanceOf(string fullJavaClassName)
        {
            return new InstanceofPredicate(fullJavaClassName);
        }

        public static AndPredicate And(params IPredicate[] predicates)
        {
            return new AndPredicate(predicates);
        }

        public static FalsePredicate False()
        {
            return new FalsePredicate();
        }

        public static BetweenPredicate IsBetween(string attributeName, object from, object to)
        {
            return new BetweenPredicate(attributeName, from, to);
        }

        public static EqualPredicate IsEqual(string attributeName, object value)
        {
            return new EqualPredicate(attributeName, value);
        }

        public static GreaterLessPredicate IsGreaterThan(string attributeName, object value)
        {
            return new GreaterLessPredicate(attributeName, value, false, false);
        }

        public static GreaterLessPredicate IsGreaterThanOrEqual(string attributeName, object value)
        {
            return new GreaterLessPredicate(attributeName, value, true, false);
        }

        public static ILikePredicate IsILike(string attributeName, string expression)
        {
            return new ILikePredicate(attributeName, expression);
        }

        public static InPredicate IsIn(string attributeName, params object[] values)
        {
            return new InPredicate(attributeName, values);
        }

        public static GreaterLessPredicate IsLessThan(string attributeName, object value)
        {
            return new GreaterLessPredicate(attributeName, value, false, true);
        }

        public static GreaterLessPredicate IsLessThanOrEqual(string attributeName, object value)
        {
            return new GreaterLessPredicate(attributeName, value, true, true);
        }

        public static LikePredicate IsLike(string attributeName, string expression)
        {
            return new LikePredicate(attributeName, expression);
        }

        public static NotEqualPredicate IsNotEqual(string attributeName, object value)
        {
            return new NotEqualPredicate(attributeName, value);
        }

        public static RegexPredicate MatchesRegex(string attributeName, string regex)
        {
            return new RegexPredicate(attributeName, regex);
        }

        public static NotPredicate Not(IPredicate predicate)
        {
            return new NotPredicate(predicate);
        }

        public static OrPredicate Or(params IPredicate[] predicates)
        {
            return new OrPredicate(predicates);
        }

        public static SqlPredicate Sql(string sql)
        {
            return new SqlPredicate(sql);
        }

        public static TruePredicate True()
        {
            return new TruePredicate();
        }
    }

    public class PredicateProperty
    {
        private readonly string property;

        public PredicateProperty(string property)
        {
            this.property = property;
        }

        public string Property
        {
            get { return property; }
        }
    }

    public static class PredicateExt
    {

        public static AndPredicate And(this IPredicate firstPredicate, IPredicate secondPredicate)
        {
            return new AndPredicate(firstPredicate, secondPredicate);
        }

        public static BetweenPredicate Between(this PredicateProperty predicateProperty, object from, object to)
        {
            return new BetweenPredicate(predicateProperty.Property,  from, to);
        }

        public static EqualPredicate Equal(this PredicateProperty predicateProperty, object value)
        {
            return new EqualPredicate(predicateProperty.Property, value);
        }

        public static GreaterLessPredicate GreaterThan(this PredicateProperty predicateProperty, object value)
        {
            return new GreaterLessPredicate(predicateProperty.Property, value, false, false);
        }

        public static GreaterLessPredicate GreaterThanOrEqual(this PredicateProperty predicateProperty, object value)
        {
            return new GreaterLessPredicate(predicateProperty.Property, value, true, false);
        }

        public static ILikePredicate ILike(this PredicateProperty predicateProperty, string expression)
        {
            return new ILikePredicate(predicateProperty.Property, expression);
        }

        public static InPredicate In(this PredicateProperty predicateProperty, params object[] values)
        {
            return new InPredicate(predicateProperty.Property, values);
        }

        public static GreaterLessPredicate LessThan(this PredicateProperty predicateProperty, object value)
        {
            return new GreaterLessPredicate(predicateProperty.Property, value, false, true);
        }

        public static GreaterLessPredicate LessThanOrEqual(this PredicateProperty predicateProperty, object value)
        {
            return new GreaterLessPredicate(predicateProperty.Property, value, true, true);
        }

        public static LikePredicate Like(this PredicateProperty predicateProperty, string expression)
        {
            return new LikePredicate(predicateProperty.Property, expression);
        }

        public static NotEqualPredicate NotEqual(this PredicateProperty predicateProperty, object value)
        {
            return new NotEqualPredicate(predicateProperty.Property, value);
        }

        public static RegexPredicate MatchesRegex(this PredicateProperty predicateProperty, string regex)
        {
            return new RegexPredicate(predicateProperty.Property, regex);
        }

        public static NotPredicate Not(this IPredicate predicate)
        {
            return new NotPredicate(predicate);
        }

        public static OrPredicate Or(this IPredicate firstPredicate, IPredicate secondPredicate)
        {
            return new OrPredicate(firstPredicate, secondPredicate);
        }
    }

    /// <summary>
    ///     SQL Predicate
    /// </summary>
    public class SqlPredicate : IPredicate
    {
        private string _sql;

        public SqlPredicate()
        {
        }

        public SqlPredicate(string sql)
        {
            _sql = sql;
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(_sql);
        }

        public void ReadData(IObjectDataInput input)
        {
            _sql = input.ReadUTF();
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.SqlPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SqlPredicate) obj);
        }

        public override int GetHashCode()
        {
            return (_sql != null ? _sql.GetHashCode() : 0);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(_sql);
            return builder.ToString();
        }

        protected bool Equals(SqlPredicate other)
        {
            return string.Equals(_sql, other._sql);
        }
    }

    public class AndPredicate : IPredicate
    {
        private IPredicate[] _predicates;

        public AndPredicate()
        {
        }

        public AndPredicate(params IPredicate[] predicates)
        {
            _predicates = predicates;
        }

        public void ReadData(IObjectDataInput input)
        {
            var size = input.ReadInt();
            _predicates = new IPredicate[size];
            for (var i = 0; i < size; i++)
            {
                _predicates[i] = input.ReadObject<IPredicate>();
            }
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteInt(_predicates.Length);
            foreach (var predicate in _predicates)
            {
                output.WriteObject(predicate);
            }
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.AndPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((AndPredicate) obj);
        }

        public override int GetHashCode()
        {
            return (_predicates != null ? _predicates.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Join(" AND ", _predicates.GetEnumerator());
        }

        protected bool Equals(AndPredicate other)
        {
            return _predicates.SequenceEqual(other._predicates);
        }
    }

    public class BetweenPredicate : IPredicate
    {
        private string _attributeName;
        private object _from;
        private object _to;

        public BetweenPredicate()
        {
        }

        public BetweenPredicate(string attributeName, object from, object to)
        {
            _attributeName = attributeName;
            _from = from;
            _to = to;
        }

        public void ReadData(IObjectDataInput input)
        {
            _attributeName = input.ReadUTF();
            _to = input.ReadObject<object>();
            _from = input.ReadObject<object>();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(_attributeName);
            output.WriteObject(_to);
            output.WriteObject(_from);
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.BetweenPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BetweenPredicate) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_attributeName != null ? _attributeName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (_from != null ? _from.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (_to != null ? _to.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return _attributeName + " BETWEEN " + _from + " AND " + _to;
        }

        protected bool Equals(BetweenPredicate other)
        {
            return string.Equals(_attributeName, other._attributeName) && Equals(_from, other._from) &&
                   Equals(_to, other._to);
        }
    }

    public class EqualPredicate : IPredicate
    {
        protected string AttributeName;
        protected object Value;

        public EqualPredicate()
        {
        }

        public EqualPredicate(string attributeName, object value)
        {
            AttributeName = attributeName;
            Value = value;
        }

        public void ReadData(IObjectDataInput input)
        {
            AttributeName = input.ReadUTF();
            Value = input.ReadObject<object>();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(AttributeName);
            output.WriteObject(Value);
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public virtual int GetId()
        {
            return PredicateDataSerializerHook.EqualPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EqualPredicate) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((AttributeName != null ? AttributeName.GetHashCode() : 0)*397) ^
                       (Value != null ? Value.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return AttributeName + " = " + Value;
        }

        protected bool Equals(EqualPredicate other)
        {
            return string.Equals(AttributeName, other.AttributeName) && Equals(Value, other.Value);
        }
    }

    public class GreaterLessPredicate : IPredicate
    {
        private string _attributeName;
        private bool _equal;
        private bool _less;
        private object _value;

        public GreaterLessPredicate()
        {
        }

        public GreaterLessPredicate(string attributeName, object value, bool isEqual, bool isLess)
        {
            _attributeName = attributeName;
            _value = value;
            _equal = isEqual;
            _less = isLess;
        }

        public void ReadData(IObjectDataInput input)
        {
            _attributeName = input.ReadUTF();
            _value = input.ReadObject<object>();
            _equal = input.ReadBoolean();
            _less = input.ReadBoolean();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(_attributeName);
            output.WriteObject(_value);
            output.WriteBoolean(_equal);
            output.WriteBoolean(_less);
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.GreaterLessPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GreaterLessPredicate) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _value.GetHashCode();
                hashCode = (hashCode*397) ^ _less.GetHashCode();
                hashCode = (hashCode*397) ^ _equal.GetHashCode();
                hashCode = (hashCode*397) ^ _attributeName.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(_attributeName);
            sb.Append(_less ? "<" : ">");
            if (_equal)
            {
                sb.Append("=");
            }
            sb.Append(_value);
            return sb.ToString();
        }

        protected bool Equals(GreaterLessPredicate other)
        {
            return _value.Equals(other._value) && _less == other._less && _equal == other._equal &&
                   string.Equals(_attributeName, other._attributeName);
        }
    }

    public class LikePredicate : IPredicate
    {
        protected string AttributeName;
        protected string Expression;

        public LikePredicate()
        {
        }

        public LikePredicate(string attributeName, string expression)
        {
            AttributeName = attributeName;
            Expression = expression;
        }

        public void ReadData(IObjectDataInput input)
        {
            AttributeName = input.ReadUTF();
            Expression = input.ReadUTF();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(AttributeName);
            output.WriteUTF(Expression);
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public virtual int GetId()
        {
            return PredicateDataSerializerHook.LikePredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((LikePredicate) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((AttributeName != null ? AttributeName.GetHashCode() : 0)*397) ^
                       (Expression != null ? Expression.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return AttributeName + " LIKE '" + Expression + "'";
        }

        protected bool Equals(LikePredicate other)
        {
            return string.Equals(AttributeName, other.AttributeName) && string.Equals(Expression, other.Expression);
        }
    }

    public class ILikePredicate : LikePredicate
    {
        public ILikePredicate()
        {
        }

        public ILikePredicate(string attributeName, string expression) : base(attributeName, expression)
        {
        }

        public override int GetId()
        {
            return PredicateDataSerializerHook.ILikePredicate;
        }

        public override string ToString()
        {
            return AttributeName + " ILIKE '" + Expression + "'";
        }
    }

    public class InPredicate : IPredicate
    {
        private string _attributeName;
        private object[] _values;

        public InPredicate()
        {
        }

        public InPredicate(string attributeName, params object[] values)
        {
            _attributeName = attributeName;
            if (values == null)
            {
                throw new NullReferenceException("Array can't be null");
            }
            _values = values;
        }

        public void ReadData(IObjectDataInput input)
        {
            _attributeName = input.ReadUTF();
            var size = input.ReadInt();
            _values = new object[size];
            for (var i = 0; i < size; i++)
            {
                _values[i] = input.ReadObject<object>();
            }
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(_attributeName);
            output.WriteInt(_values.Length);
            foreach (var value in _values)
            {
                output.WriteObject(value);
            }
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.InPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((InPredicate) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_attributeName != null ? _attributeName.GetHashCode() : 0)*397) ^
                       (_values != null ? _values.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return _attributeName + " IN (" + string.Join(", ", _values) + ")";
        }

        protected bool Equals(InPredicate other)
        {
            return string.Equals(_attributeName, other._attributeName) && _values.SequenceEqual(other._values);
        }
    }

    public class InstanceofPredicate : IPredicate
    {
        private string _className;

        public InstanceofPredicate()
        {
        }

        public InstanceofPredicate(string className)
        {
            _className = className;
        }

        public void ReadData(IObjectDataInput input)
        {
            _className = input.ReadUTF();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(_className);
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.InstanceofPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((InstanceofPredicate) obj);
        }

        public override int GetHashCode()
        {
            return _className != null ? _className.GetHashCode() : 0;
        }

        protected bool Equals(InstanceofPredicate other)
        {
            return string.Equals(_className, other._className);
        }

        public override string ToString()
        {
            return " InstanceOf " + _className;
        }
    }

    public class NotEqualPredicate : EqualPredicate
    {
        public NotEqualPredicate()
        {
        }

        public NotEqualPredicate(string attributeName, object value) : base(attributeName, value)
        {
        }

        public override int GetId()
        {
            return PredicateDataSerializerHook.NotEqualPredicate;
        }

        public override string ToString()
        {
            return AttributeName + " != " + Value;
        }
    }

    public class NotPredicate : IPredicate
    {
        private IPredicate _predicate;

        public NotPredicate()
        {
        }

        public NotPredicate(IPredicate predicate)
        {
            _predicate = predicate;
        }

        public void ReadData(IObjectDataInput input)
        {
            _predicate = input.ReadObject<IPredicate>();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteObject(_predicate);
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.NotPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NotPredicate) obj);
        }

        public override int GetHashCode()
        {
            return (_predicate != null ? _predicate.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return "NOT(" + _predicate + ")";
        }

        protected bool Equals(NotPredicate other)
        {
            return Equals(_predicate, other._predicate);
        }
    }

    public class OrPredicate : IPredicate
    {
        private IPredicate[] _predicates;

        public OrPredicate(params IPredicate[] predicates)
        {
            _predicates = predicates;
        }

        public void ReadData(IObjectDataInput input)
        {
            var size = input.ReadInt();
            _predicates = new IPredicate[size];
            for (var i = 0; i < size; i++)
            {
                _predicates[i] = input.ReadObject<IPredicate>();
            }
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteInt(_predicates.Length);
            foreach (var predicate in _predicates)
            {
                output.WriteObject(predicate);
            }
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.OrPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((OrPredicate) obj);
        }

        public override int GetHashCode()
        {
            return (_predicates != null ? _predicates.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Join(" OR ", _predicates.GetEnumerator());
        }

        protected bool Equals(OrPredicate other)
        {
            return _predicates.SequenceEqual(other._predicates);
        }
    }

    public class RegexPredicate : IPredicate
    {
        private string _attributeName;
        private string _regex;

        public RegexPredicate()
        {
        }

        public RegexPredicate(string attributeName, string regex)
        {
            _attributeName = attributeName;
            _regex = regex;
        }

        public void ReadData(IObjectDataInput input)
        {
            _attributeName = input.ReadUTF();
            _regex = input.ReadUTF();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(_attributeName);
            output.WriteUTF(_regex);
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.RegexPredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((RegexPredicate) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_attributeName != null ? _attributeName.GetHashCode() : 0)*397) ^
                       (_regex != null ? _regex.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return _attributeName + " REGEX '" + _regex + "'";
        }

        protected bool Equals(RegexPredicate other)
        {
            return string.Equals(_attributeName, other._attributeName) && string.Equals(_regex, other._regex);
        }
    }

    public class FalsePredicate : IPredicate
    {
        public void ReadData(IObjectDataInput input)
        {
        }

        public void WriteData(IObjectDataOutput output)
        {
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.FalsePredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FalsePredicate) obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "FalsePredicate{}";
        }

        protected bool Equals(FalsePredicate other)
        {
            return true;
        }
    }

    public class TruePredicate : IPredicate
    {
        public void ReadData(IObjectDataInput input)
        {
        }

        public void WriteData(IObjectDataOutput output)
        {
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.TruePredicate;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TruePredicate) obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "TruePredicate{}";
        }

        protected bool Equals(TruePredicate other)
        {
            return true;
        }
    }
}