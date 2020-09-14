using System.Collections.Generic;
using Domain.Interfaces.Entities;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;

namespace Storage
{
    public sealed class QueryEntity : RepositoryEntity, IQueryableEntity
    {
        public TEntity ToEntity<TEntity>(IDomainFactory domainFactory)
            where TEntity : IQueryableEntity, new()
        {
            domainFactory.GuardAgainstNull(nameof(domainFactory));

            var properties = ConvertToDomainProperties(domainFactory);
            return properties.FromObjectDictionary<TEntity>();
        }

        public static QueryEntity FromType<TType>(TType instance)
            where TType : IQueryableEntity
        {
            instance.GuardAgainstNull(nameof(instance));

            var properties = instance.ToObjectDictionary();
            return FromProperties<TType>(properties);
        }

        private static QueryEntity FromProperties<TType>(IReadOnlyDictionary<string, object> properties)
            where TType : IQueryableEntity
        {
            properties.GuardAgainstNull(nameof(properties));

            var metadata = RepositoryEntityMetadata.FromType<TType>();

            return FromProperties(properties, metadata);
        }

        public static QueryEntity FromProperties(IReadOnlyDictionary<string, object> properties,
            RepositoryEntityMetadata metadata)
        {
            properties.GuardAgainstNull(nameof(properties));
            metadata.GuardAgainstNull(nameof(metadata));

            var dataEntity = new QueryEntity();
            foreach (var pair in properties)
            {
                var propertyType = metadata.GetPropertyType(pair.Key, false);
                if (propertyType != null)
                {
                    dataEntity.Add(pair.Key, pair.Value, propertyType);
                }
            }

            return dataEntity;
        }
    }
}