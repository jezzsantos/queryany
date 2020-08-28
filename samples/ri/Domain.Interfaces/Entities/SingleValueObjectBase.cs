using System.Collections.Generic;
using QueryAny.Primitives;

namespace Domain.Interfaces.Entities
{
    public abstract class SingleValueObjectBase<TValueObject, TValue> : ValueObjectBase<TValueObject>
    {
        private TValue value;

        protected SingleValueObjectBase(TValue value)
        {
            value.GuardAgainstNull(nameof(value));
            this.value = value;
        }

        protected TValue Value => this.value;

        protected abstract TValue ToValue(string value);

        public override string Dehydrate()
        {
            return this.value.ToString();
        }

        public override void Rehydrate(string hydratedValue)
        {
            this.value = ToValue(hydratedValue);
        }

        public static implicit operator TValue(SingleValueObjectBase<TValueObject, TValue> valueObject)
        {
            return valueObject.Value;
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
            return Value.GetHashCode();
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] {this.value};
        }
    }
}