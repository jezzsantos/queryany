using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Interfaces.Entities
{
    /// <summary>
    ///     Defines an immutable DDD value type.
    ///     Value types are equal when their internal data is the same.
    ///     Value types support being persisted
    /// </summary>
    public abstract class ValueType<TValue> : IEquatable<TValue>, IPersistableValueType
    {
        public bool Equals(TValue other)
        {
            return Equals((object) other);
        }

        public abstract string Dehydrate();

        public abstract void Rehydrate(string value);

        protected abstract IEnumerable<object> GetAtomicValues();

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var other = (ValueType<TValue>) obj;

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
            return string.Join("::", GetAtomicValues()
                .Select(x => x.ToString()));
        }
    }

    public static class ValueTypeExtensions
    {
        public static bool HasValue<TValue>(this ValueType<TValue> valueType)
        {
            return valueType != null;
        }
    }
}