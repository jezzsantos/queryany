using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common;

namespace Storage
{
    public class RepositoryEntityMetadata
    {
        private static readonly Dictionary<Type, IEnumerable<PropertyInfo>> TypePropertiesCache =
            new Dictionary<Type, IEnumerable<PropertyInfo>>();
        private readonly Dictionary<string, Type> propertyTypes;

        internal RepositoryEntityMetadata(Dictionary<string, Type> propertyTypes = null)
        {
            this.propertyTypes = propertyTypes ?? new Dictionary<string, Type>();
        }

        public static RepositoryEntityMetadata Empty => new RepositoryEntityMetadata();

        public IReadOnlyDictionary<string, Type> Types => this.propertyTypes;

        public Type GetPropertyType(string propertyName, bool throwIfNotExists = true)
        {
            propertyName.GuardAgainstNullOrEmpty(nameof(propertyName));

            if (this.propertyTypes.ContainsKey(propertyName))
            {
                return this.propertyTypes[propertyName];
            }

            if (!throwIfNotExists)
            {
                return null;
            }

            throw new InvalidOperationException(
                $"No type for property '{propertyName}' exists in this metadata");
        }

        public bool HasType(string propertyName)
        {
            propertyName.GuardAgainstNullOrEmpty(nameof(propertyName));

            return this.propertyTypes.ContainsKey(propertyName);
        }

        public static RepositoryEntityMetadata FromType<TEntity>()
        {
            return FromType(typeof(TEntity));
        }

        public static RepositoryEntityMetadata FromType(Type type)
        {
            type.GuardAgainstNull(nameof(type));

            var properties = GetProperties(type);
            return new RepositoryEntityMetadata(properties);
        }

        public void Update(string propertyName, Type type)
        {
            propertyName.GuardAgainstNullOrEmpty(nameof(propertyName));

            this.propertyTypes[propertyName] = type;
        }

        private static Dictionary<string, Type> GetProperties(Type type)
        {
            var properties = WithCache(type, t => t.GetProperties());

            return properties.ToDictionary(prop => prop.Name, prop => prop.PropertyType);
        }

        private static IEnumerable<PropertyInfo> WithCache(Type type, Func<Type, PropertyInfo[]> propertyInfoFactory)
        {
            if (!TypePropertiesCache.ContainsKey(type))
            {
                TypePropertiesCache[type] = propertyInfoFactory(type);
            }

            return TypePropertiesCache[type];
        }
    }
}