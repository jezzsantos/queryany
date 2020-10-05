using System.Collections.Generic;
using System.Linq;
using QueryAny.Primitives;
using ServiceStack;

namespace Domain.Interfaces.Entities
{
    public abstract class SingleValueObjectBase<TValueObject, TValue> : ValueObjectBase<TValueObject>
    {
        protected SingleValueObjectBase(TValue value)
        {
            value.GuardAgainstNull(nameof(value));
            Value = value;
        }

        protected TValue Value { get; private set; }

        protected abstract TValue ToValue(string value);

        public override void Rehydrate(string hydratedValue)
        {
            var values = RehydrateToList(hydratedValue);
            if (Value is IEnumerable<IPersistableValueObject>)
            {
                Value = ToValue(values
                    .Where(value => value != null)
                    .ToJson());
                return;
            }

            Value = ToValue(values.FirstOrDefault());
        }

        protected new static List<string> RehydrateToList(string hydratedValue)
        {
            var isList = typeof(IEnumerable<IPersistableValueObject>).IsAssignableFrom(typeof(TValue));

            return RehydrateToList(hydratedValue, true, isList);
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
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Value.GetHashCode();
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            return new object[] {Value};
        }
    }
}