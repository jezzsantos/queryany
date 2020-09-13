using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces.Entities;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;

namespace Storage
{
    public sealed class CommandEntity : RepositoryEntity, IIdentifiableEntity
    {
        public CommandEntity(string id) : base(id)
        {
        }

        public TEntity ToPersistableEntity<TEntity>(IDomainFactory domainFactory)
            where TEntity : IPersistableEntity
        {
            domainFactory.GuardAgainstNull(nameof(domainFactory));

            var properties = ConvertFromRawProperties(domainFactory);
            var result = domainFactory.RehydrateEntity(typeof(TEntity), properties);
            return (TEntity) result;
        }

        public static CommandEntity FromPersistableEntity<TEntity>(TEntity entity) where TEntity : IPersistableEntity
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

            return FromProperties(properties, metadata.Types);
        }

        public static CommandEntity FromType<TType>(TType instance)
            where TType : IIdentifiableEntity, IQueryableEntity
        {
            instance.GuardAgainstNull(nameof(instance));

            var properties = instance.ToObjectDictionary();
            return FromProperties<TType>(properties);
        }

        private static CommandEntity FromProperties<TType>(IReadOnlyDictionary<string, object> properties)
            where TType : IIdentifiableEntity, IQueryableEntity
        {
            properties.GuardAgainstNull(nameof(properties));

            var metadata = RepositoryEntityMetadata.FromType<TType>();

            return FromProperties(properties, metadata);
        }

        public static CommandEntity FromProperties(IReadOnlyDictionary<string, object> properties,
            RepositoryEntityMetadata metadata)
        {
            properties.GuardAgainstNull(nameof(properties));

            return FromProperties(properties, metadata.Types);
        }

        private static CommandEntity FromProperties(IReadOnlyDictionary<string, object> properties,
            IReadOnlyDictionary<string, Type> propertyTypes)
        {
            properties.GuardAgainstNull(nameof(properties));
            propertyTypes.GuardAgainstNull(nameof(propertyTypes));

            if (!properties.ContainsKey(nameof(Id))
                || properties[nameof(Id)] == null)
            {
                throw new InvalidOperationException(
                    "Unable to create a new CommandEntity. There is no 'Id' property in this collection of values.");
            }

            var result = new CommandEntity(properties[nameof(Id)].ToString());

            foreach (var property in properties)
            {
                var propertyType = propertyTypes.First(p => p.Key.EqualsOrdinal(property.Key)).Value;
                result.Add(property.Key, property.Value, propertyType);
            }

            return result;
        }
    }
}