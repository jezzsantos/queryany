using System;
using System.Collections.Generic;
using Domain.Interfaces.Entities;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;
using Storage.Interfaces.ReadModels;

namespace Storage
{
    public sealed class CommandEntity : RepositoryEntity, IHasIdentity
    {
        public CommandEntity(string id) : base(id)
        {
        }

        public TEntity ToDomainEntity<TEntity>(IDomainFactory domainFactory)
            where TEntity : IPersistableEntity
        {
            domainFactory.GuardAgainstNull(nameof(domainFactory));

            var domainProperties = ConvertToDomainProperties(domainFactory);
            var result = domainFactory.RehydrateEntity(typeof(TEntity), domainProperties);
            return (TEntity) result;
        }

        public TDto ToReadModelEntity<TDto>() where TDto : IReadModelEntity, new()
        {
            return Properties.FromObjectDictionary<TDto>();
        }

        public static CommandEntity FromDomainEntity<TEntity>(TEntity entity) where TEntity : IPersistableEntity
        {
            var properties = entity.Dehydrate();
            return FromProperties<TEntity>(properties);
        }

        public static CommandEntity FromCommandEntity(IReadOnlyDictionary<string, object> properties,
            CommandEntity descriptor)
        {
            return FromCommandEntity(properties, descriptor.Metadata);
        }

        public static CommandEntity FromCommandEntity(IReadOnlyDictionary<string, object> properties,
            RepositoryEntityMetadata metadata)
        {
            properties.GuardAgainstNull(nameof(properties));
            metadata.GuardAgainstNull(nameof(metadata));

            return FromProperties(properties, metadata);
        }

        public static CommandEntity FromType<TType>(TType instance)
            where TType : IQueryableEntity
        {
            instance.GuardAgainstNull(nameof(instance));

            var properties = instance.ToObjectDictionary();
            return FromProperties<TType>(properties);
        }

        private static CommandEntity FromProperties<TType>(IReadOnlyDictionary<string, object> properties)
            where TType : IQueryableEntity
        {
            properties.GuardAgainstNull(nameof(properties));

            var metadata = RepositoryEntityMetadata.FromType<TType>();

            return FromProperties(properties, metadata);
        }

        public static CommandEntity FromProperties(IReadOnlyDictionary<string, object> properties,
            RepositoryEntityMetadata metadata)
        {
            properties.GuardAgainstNull(nameof(properties));
            metadata.GuardAgainstNull(nameof(metadata));

            if (!properties.ContainsKey(nameof(Id))
                || properties[nameof(Id)] == null)
            {
                throw new InvalidOperationException(
                    "Unable to create a new CommandEntity. There is no 'Id' property in this collection of values.");
            }

            var result = new CommandEntity(properties[nameof(Id)].ToString());

            foreach (var property in properties)
            {
                var propertyType = metadata.GetPropertyType(property.Key, false);
                if (propertyType != null)
                {
                    result.Add(property.Key, property.Value, propertyType);
                }
            }

            return result;
        }
    }
}