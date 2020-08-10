using System;
using System.Collections.Generic;
using System.Linq;
using QueryAny.Primitives;

namespace Domain.Interfaces.Entities
{
    /// <summary>
    ///     Defines a DDD value type.
    ///     Value types are immutable, and their properties should be set at construction, and never altered.
    ///     Value types are equal when their internal data is the same.
    ///     Value types support being persisted
    /// </summary>
    public abstract class ValueTypeBase<TValue> : IEquatable<TValue>, IPersistableValueType
    {
        protected const string DefaultHydrationDelimiter = "::";

        /// <summary>
        ///     Here so that type can be deserialized by persistence
        /// </summary>

        // ReSharper disable once EmptyConstructor
        // ReSharper disable once PublicConstructorInAbstractClass
        public ValueTypeBase()
        {
        }

        public bool Equals(TValue other)
        {
            return Equals((object) other);
        }

        public virtual string Dehydrate()
        {
            return GetAtomicValues()
                .Select(val => val != null
                    ? val.ToString()
                    : string.Empty)
                .Join(DefaultHydrationDelimiter);
        }

        public virtual void Rehydrate(string value)
        {
        }

        protected abstract IEnumerable<object> GetAtomicValues();

        public static bool operator ==(ValueTypeBase<TValue> obj1, ValueTypeBase<TValue> obj2)
        {
            if ((object) obj1 == null)
            {
                return (object) obj2 == null;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ValueTypeBase<TValue> obj1, ValueTypeBase<TValue> obj2)
        {
            return !(obj1 == obj2);
        }

        public static bool operator ==(ValueTypeBase<TValue> obj1, string obj2)
        {
            if ((object) obj1 == null)
            {
                return obj2 == null;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ValueTypeBase<TValue> obj1, string obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var other = (ValueTypeBase<TValue>) obj;

            // ReSharper disable once GenericEnumeratorNotDisposed
            var thisValues = GetAtomicValues().GetEnumerator();

            // ReSharper disable once GenericEnumeratorNotDisposed
            var otherValues = other.GetAtomicValues().GetEnumerator();
            while (thisValues.MoveNext() && otherValues.MoveNext())
            {
                if (ReferenceEquals(thisValues.Current, null) ^
                    ReferenceEquals(otherValues.Current, null))
                {
                    return false;
                }

                if (thisValues.Current != null &&
                    !thisValues.Current.Equals(otherValues.Current))
                {
                    return false;
                }
            }

            return !thisValues.MoveNext() && !otherValues.MoveNext();
        }

        public bool Equals(string other)
        {
            if (other == null || other.GetType() != typeof(string))
            {
                return false;
            }

            var value = Dehydrate();
            return value.EqualsOrdinal(other);
        }

        public override int GetHashCode()
        {
            return GetAtomicValues()
                .Select(x => x != null
                    ? x.GetHashCode()
                    : 0)
                .Aggregate((x, y) => x ^ y);
        }

        public override string ToString()
        {
            return Dehydrate();
        }
    }

    public static class ValueTypeExtensions
    {
        public static bool HasValue<TValue>(this ValueTypeBase<TValue> valueType)
        {
            return valueType != (ValueTypeBase<TValue>) null;
        }
    }
}