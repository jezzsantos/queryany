using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces.Entities;
using Newtonsoft.Json;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;

namespace Storage
{
    public static class RepositoryExtensions
    {
        public static int GetDefaultSkip<TEntity>(this QueryClause<TEntity> query)
            where TEntity : IPersistableEntity
        {
            return query.ResultOptions.Offset != ResultOptions.DefaultOffset
                ? query.ResultOptions.Offset
                : 0;
        }

        public static int GetDefaultTake<TEntity>(this QueryClause<TEntity> query, IRepository repository)
            where TEntity : IPersistableEntity
        {
            return query.ResultOptions.Limit == ResultOptions.DefaultLimit
                ? repository.MaxQueryResults
                : query.ResultOptions.Limit;
        }

        public static string GetDefaultOrdering<TEntity>(this QueryClause<TEntity> query)
            where TEntity : IPersistableEntity
        {
            return query.IsDefaultOrdering()
                ? $"{Reflector<TEntity>.GetPropertyName(e => e.CreatedAtUtc)}"
                : $"{query.ResultOptions.OrderBy.By}";
        }

        private static bool IsDefaultOrdering<TEntity>(this QueryClause<TEntity> query)
            where TEntity : IQueryableEntity
        {
            var by = query.ResultOptions.OrderBy.By;
            if (!by.HasValue())
            {
                return true;
            }

            if (by.EqualsOrdinal(nameof(IModifiableEntity.CreatedAtUtc)))
            {
                return true;
            }

            var selectedFields = query.GetAllSelectedFields();
            if (selectedFields.Any())
            {
                return !query.GetAllSelectedFields().Contains(by);
            }

            return false;
        }

        private static List<string> GetAllSelectedFields<TEntity>(this QueryClause<TEntity> query)
            where TEntity : IQueryableEntity
        {
            var primarySelects = query.PrimaryEntity.Selects;
            var joinedSelects = query.JoinedEntities.SelectMany(je => je.Selects);

            return primarySelects
                .Select(select => select.FieldName)
                .Concat(joinedSelects.Select(select => select.FieldName))
                .ToList();
        }

        public static List<TEntity> JoinResults<TEntity>(this JoinDefinition joinDefinition,
            Dictionary<Identifier, Dictionary<string, object>> leftEntities,
            Dictionary<Identifier, Dictionary<string, object>> rightEntities,
            Func<KeyValuePair<Identifier, Dictionary<string, object>>,
                KeyValuePair<Identifier, Dictionary<string, object>>,
                KeyValuePair<Identifier, Dictionary<string, object>>> mapFunc = null)
        {
            switch (joinDefinition.Type)
            {
                case JoinType.Inner:
                    var innerJoin = from lefts in leftEntities
                        join rights in rightEntities on lefts.Value[joinDefinition.Left.JoinedFieldName] equals
                            rights.Value[joinDefinition.Right.JoinedFieldName]
                            into joined
                        from result in joined
                        select mapFunc?.Invoke(lefts, result) ?? lefts;

                    return innerJoin
                        .Select(e => e.Value.FromObjectDictionary<TEntity>())
                        .ToList();

                case JoinType.Left:
                    var leftJoin = from lefts in leftEntities
                        join rights in rightEntities on lefts.Value[joinDefinition.Left.JoinedFieldName] equals
                            rights.Value[joinDefinition.Right.JoinedFieldName]
                            into joined
                        from result in joined.DefaultIfEmpty()
                        select mapFunc?.Invoke(lefts, result) ?? lefts;

                    return leftJoin
                        .Select(e => e.Value.FromObjectDictionary<TEntity>())
                        .ToList();

                default:
                    throw new ArgumentOutOfRangeException(nameof(JoinType));
            }
        }

        public static Func<KeyValuePair<Identifier, Dictionary<string, object>>,
                KeyValuePair<Identifier, Dictionary<string, object>>,
                KeyValuePair<Identifier, Dictionary<string, object>>>
            ProjectSelectedJoinedProperties(this IReadOnlyList<SelectDefinition> selects)
        {
            return (leftEntity, rightEntity) =>
            {
                var selectedFromJoinPropertyNames = selects
                    .Where(x => x.JoinedFieldName.HasValue())
                    .ToList();
                if (!selectedFromJoinPropertyNames.Any())
                {
                    return leftEntity;
                }

                var leftEntityProperties = leftEntity.Value.ToObjectDictionary();
                var rightEntityProperties = rightEntity.Value.ToObjectDictionary();
                foreach (var select in selectedFromJoinPropertyNames)
                {
                    if (!rightEntityProperties.HasPropertyValue(select.JoinedFieldName))
                    {
                        continue;
                    }

                    leftEntityProperties.CreatePropertyIfNotExists(select.FieldName);
                    leftEntityProperties.CopyPropertyValue(rightEntityProperties, select);
                }

                return leftEntity;
            };
        }

        public static List<TEntity> CherryPickSelectedProperties<TEntity>(this IEnumerable<TEntity> entities,
            QueryClause<TEntity> query, IEnumerable<string> includeAdditionalProperties = null)
            where TEntity : IPersistableEntity
        {
            var selectedPropertyNames = query.GetAllSelectedFields();
            if (!selectedPropertyNames.Any())
            {
                return entities.ToList();
            }

            selectedPropertyNames = selectedPropertyNames
                .Concat(new[] {nameof(IPersistableEntity.Id)})
                .Concat(includeAdditionalProperties ?? Enumerable.Empty<string>())
                .ToList();

            return entities
                .Select(resultEntity => resultEntity.ToObjectDictionary()
                    .Where(resultEntityProperty => selectedPropertyNames.Contains(resultEntityProperty.Key)))
                .Select(selectedProperties => selectedProperties.FromObjectDictionary<TEntity>())
                .ToList();
        }

        public static TEntity CreateEntity<TEntity>(this IDictionary<string, object> propertyValues,
            Identifier id, EntityFactory<TEntity> entityFactory) where TEntity : IPersistableEntity
        {
            return (TEntity) CreateEntityInternal(propertyValues, id, properties => entityFactory(properties));
        }

        public static IPersistableEntity CreateEntity(this IDictionary<string, object> propertyValues,
            string id, EntityFactory<IPersistableEntity> entityFactory)
        {
            return CreateEntityInternal(propertyValues, Identifier.Create(id), entityFactory);
        }

        private static IPersistableEntity CreateEntityInternal(this IDictionary<string, object> propertyValues,
            Identifier id, EntityFactory<IPersistableEntity> entityFactory)
        {
            try
            {
                var propertiesWithIdentifier = new Dictionary<string, object>(propertyValues);
                if (!propertiesWithIdentifier.ContainsKey(nameof(IIdentifiableEntity.Id)))
                {
                    propertiesWithIdentifier.Add(nameof(IIdentifiableEntity.Id), id);
                }

                var entity = entityFactory(propertiesWithIdentifier);
                entity.Rehydrate(propertiesWithIdentifier);

                return entity;
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static IPersistableValueType ValueTypeFromContainerProperty(this string propertyValue,
            Type targetPropertyType)
        {
            try
            {
                var valueType = (IPersistableValueType) targetPropertyType.CreateInstance();
                valueType.Rehydrate(propertyValue);

                return valueType;
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static object ComplexTypeFromContainerProperty(this string propertyValue, Type targetPropertyType)
        {
            if (propertyValue.HasValue())
            {
                try
                {
                    if (propertyValue.StartsWith("{") && propertyValue.EndsWith("}"))
                    {
                        return JsonConvert.DeserializeObject(propertyValue, targetPropertyType);
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return null;
        }

        private static bool HasPropertyValue(this Dictionary<string, object> entityProperties, string propertyName)
        {
            return entityProperties != null && entityProperties.ContainsKey(propertyName);
        }

        private static void CreatePropertyIfNotExists(this Dictionary<string, object> entityProperties,
            string propertyName)
        {
            if (!entityProperties.ContainsKey(propertyName))
            {
                entityProperties.Add(propertyName, default);
            }
        }

        private static void CopyPropertyValue(this Dictionary<string, object> toEntityProperties,
            Dictionary<string, object> fromEntityProperties,
            SelectDefinition select)
        {
            toEntityProperties[select.FieldName] =
                fromEntityProperties[select.JoinedFieldName];
        }
    }
}