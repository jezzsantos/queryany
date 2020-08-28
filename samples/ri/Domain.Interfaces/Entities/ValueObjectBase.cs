using System;
using System.Collections.Generic;
using System.Linq;
using QueryAny.Primitives;

namespace Domain.Interfaces.Entities
{
    /// <summary>
    ///     Defines a DDD value object.
    ///     Value objects are immutable, and their properties should be set at construction, and never altered.
    ///     Value objects are equal when their internal data is the same.
    ///     Value objects support being persisted
    /// </summary>
    public abstract class ValueObjectBase<TValueObject> : IEquatable<TValueObject>,
        IComparable<ValueObjectBase<TValueObject>>, IPersistableValueObject
    {
        private const string DefaultHydrationDelimiter = "::";

        public int CompareTo(ValueObjectBase<TValueObject> other)
        {
            var thisValue = Dehydrate();
            var otherValue = other.Dehydrate();
            return string.Compare(thisValue, otherValue, StringComparison.Ordinal);
        }

        public bool Equals(TValueObject other)
        {
            return Equals((object) other);
        }

        public virtual string Dehydrate()
        {
            var parts = GetAtomicValues().ToList();
            if (!parts.Any())
            {
                return null;
            }
            if (parts.Count == 1)
            {
                return parts[0].ToString();
            }

            return parts
                .Select(val => val != null
                    ? val.ToString()
                    : string.Empty)
                .Join(DefaultHydrationDelimiter);
        }

        public abstract void Rehydrate(string hydratedValue);

        protected static List<string> RehydrateToList(string hydratedValue)
        {
            return hydratedValue
                .SafeSplit(DefaultHydrationDelimiter)
                .ToList();
        }

        protected abstract IEnumerable<object> GetAtomicValues();

        public static bool operator ==(ValueObjectBase<TValueObject> obj1, ValueObjectBase<TValueObject> obj2)
        {
            if ((object) obj1 == null)
            {
                return (object) obj2 == null;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ValueObjectBase<TValueObject> obj1, ValueObjectBase<TValueObject> obj2)
        {
            return !(obj1 == obj2);
        }

        public static bool operator ==(ValueObjectBase<TValueObject> obj1, string obj2)
        {
            if ((object) obj1 == null)
            {
                return obj2 == null;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ValueObjectBase<TValueObject> obj1, string obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var other = (ValueObjectBase<TValueObject>) obj;

            using (var thisValues = GetAtomicValues().GetEnumerator())
            {
                using (var otherValues = other.GetAtomicValues().GetEnumerator())
                {
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
            }
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

    public static class ValueObjectExtensions
    {
        public static bool HasValue<TValue>(this ValueObjectBase<TValue> valueObject)
        {
            return valueObject != (ValueObjectBase<TValue>) null;
        }
    }
}