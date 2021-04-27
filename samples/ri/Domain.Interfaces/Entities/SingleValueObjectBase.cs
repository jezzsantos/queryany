using System.Collections.Generic;

namespace Domain.Interfaces.Entities
{
    public abstract class SingleValueObjectBase<TValueObject, TValue> : ValueObjectBase<TValueObject>
    {
        protected SingleValueObjectBase(TValue value)
        {
            value.GuardAgainstNull(nameof(value));
            Value = value;
        }

        protected TValue Value { get; }

        public static implicit operator TValue(SingleValueObjectBase<TValueObject, TValue> valueObject)
        {
            return valueObject == null
                ? default
                : valueObject.Value;
        }

        public static bool operator ==(SingleValueObjectBase<TValueObject, TValue> obj1,
            SingleValueObjectBase<TValueObject, TValue> obj2)
        {
            if ((object) obj1 == null)
            {
                return (object) obj2 == null;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(SingleValueObjectBase<TValueObject, TValue> obj1,
            SingleValueObjectBase<TValueObject, TValue> obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Value.GetHashCode();
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] {Value};
        }
    }
}