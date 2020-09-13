using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;

namespace Storage
{
    public abstract class RepositoryEntity
    {
        private readonly Dictionary<string, object> propertyValues = new Dictionary<string, object>();

        protected RepositoryEntity(string id) : this()
        {
            id.GuardAgainstNullOrEmpty(nameof(id));
            Id = id.ToIdentifier();
        }

        protected RepositoryEntity()
        {
            Metadata = new RepositoryEntityMetadata();
            Add(nameof(Id), null, typeof(string));
            Add(nameof(LastPersistedAtUtc), null, typeof(DateTime?));
        }

        public RepositoryEntityMetadata Metadata { get; }

        public IReadOnlyDictionary<string, object> Properties => this.propertyValues;

        public DateTime? LastPersistedAtUtc
        {
            get => this.propertyValues.GetValueOrDefault<DateTime?>(nameof(LastPersistedAtUtc));
            set => this.propertyValues[nameof(LastPersistedAtUtc)] = value;
        }

        public Identifier Id
        {
            get => this.propertyValues.GetValueOrDefault<string>(nameof(Id)).ToIdentifier();
            set => this.propertyValues[nameof(Id)] = value.ToString();
        }

        public Type GetPropertyType(string propertyName)
        {
            return Metadata.GetPropertyType(propertyName);
        }

        public void Add(string name, object value, Type type)
        {
            name.GuardAgainstNullOrEmpty(nameof(name));
            type.GuardAgainstNull(nameof(type));

            var rawValue = ConvertToRawProperty(value);
            if (this.propertyValues.ContainsKey(name))
            {
                this.propertyValues[name] = rawValue;
                Metadata.Update(name, type);
            }
            else
            {
                this.propertyValues.Add(name, rawValue);
                Metadata.Update(name, type);
            }
        }

        public TValue GetValueOrDefault<TValue>(string propertyName, TValue defaultValue = default)
        {
            if (!this.propertyValues.ContainsKey(propertyName))
            {
                return defaultValue;
            }

            return (TValue) ConvertFromRawProperty(this.propertyValues[propertyName], typeof(TValue), null);
        }

        public TValueObject GetValueOrDefault<TValueObject>(string propertyName, IDomainFactory domainFactory)
            where TValueObject : ValueObjectBase<TValueObject>
        {
            if (!this.propertyValues.ContainsKey(propertyName))
            {
                return default;
            }

            return (TValueObject) ConvertFromRawProperty(this.propertyValues[propertyName], typeof(TValueObject),
                domainFactory);
        }

        protected IReadOnlyDictionary<string, object> ConvertFromRawProperties(IDomainFactory domainFactory)
        {
            return this.propertyValues.ToDictionary(x => x.Key, x =>
            {
                domainFactory.GuardAgainstNull(nameof(domainFactory));

                var value = x.Value;
                var propertyType = GetPropertyType(x.Key);
                return ConvertFromRawProperty(value, propertyType, domainFactory);
            });
        }

        private static object ConvertToRawProperty(object originalValue)
        {
            if (originalValue == null)
            {
                return null;
            }

            if (originalValue is IPersistableValueObject valueObject)
            {
                return valueObject.Dehydrate();
            }

            return originalValue;
        }

        private static object ConvertFromRawProperty(object rawValue, Type propertyType, IDomainFactory domainFactory)
        {
            if (typeof(IPersistableValueObject).IsAssignableFrom(propertyType))
            {
                if (rawValue == null)
                {
                    return null;
                }

                domainFactory.GuardAgainstNull(nameof(domainFactory));
                return domainFactory.RehydrateValueObject(propertyType, (string) rawValue);
            }

            return rawValue;
        }
    }
}