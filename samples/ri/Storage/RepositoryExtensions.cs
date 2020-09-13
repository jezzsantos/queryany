using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;

namespace Storage
{
    public static class RepositoryExtensions
    {
        public static int GetDefaultSkip<TQueryableEntity>(this QueryClause<TQueryableEntity> query)
            where TQueryableEntity : IQueryableEntity
        {
            return query.ResultOptions.Offset != ResultOptions.DefaultOffset
                ? query.ResultOptions.Offset
                : 0;
        }

        public static int GetDefaultTake<TQueryableEntity>(this QueryClause<TQueryableEntity> query,
            IRepository repository)
            where TQueryableEntity : IQueryableEntity
        {
            return query.ResultOptions.Limit == ResultOptions.DefaultLimit
                ? repository.MaxQueryResults
                : query.ResultOptions.Limit;
        }

        public static string GetDefaultOrdering<TQueryableEntity>(this QueryClause<TQueryableEntity> query)
            where TQueryableEntity : IQueryableEntity
        {
            return query.IsDefaultOrdering()
                ? $"{nameof(QueryEntity.LastPersistedAtUtc)}"
                : $"{query.ResultOptions.OrderBy.By}";
        }

        private static bool IsDefaultOrdering<TQueryableEntity>(this QueryClause<TQueryableEntity> query)
            where TQueryableEntity : IQueryableEntity

        {
            var by = query.ResultOptions.OrderBy.By;
            if (!by.HasValue())
            {
                return true;
            }

            if (by.EqualsOrdinal(nameof(QueryEntity.LastPersistedAtUtc)))
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

        private static List<string> GetAllSelectedFields<TQueryableEntity>(this QueryClause<TQueryableEntity> query)
            where TQueryableEntity : IQueryableEntity

        {
            var primarySelects = query.PrimaryEntity.Selects;
            var joinedSelects = query.JoinedEntities.SelectMany(je => je.Selects);

            return primarySelects
                .Select(select => select.FieldName)
                .Concat(joinedSelects.Select(select => select.FieldName))
                .ToList();
        }

        public static List<QueryEntity> JoinResults(this JoinDefinition joinDefinition,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> leftEntities,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> rightEntities,
            RepositoryEntityMetadata metadata,
            Func<KeyValuePair<string, IReadOnlyDictionary<string, object>>,
                KeyValuePair<string, IReadOnlyDictionary<string, object>>,
                KeyValuePair<string, IReadOnlyDictionary<string, object>>> mapFunc = null)
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
                        .Select(e => EntityFromContainerProperties(e.Value, metadata))
                        .ToList();

                case JoinType.Left:
                    var leftJoin = from lefts in leftEntities
                        join rights in rightEntities on lefts.Value[joinDefinition.Left.JoinedFieldName] equals
                            rights.Value[joinDefinition.Right.JoinedFieldName]
                            into joined
                        from result in joined.DefaultIfEmpty()
                        select mapFunc?.Invoke(lefts, result) ?? lefts;

                    return leftJoin
                        .Select(e => EntityFromContainerProperties(e.Value, metadata))
                        .ToList();

                default:
                    throw new ArgumentOutOfRangeException(nameof(JoinType));
            }
        }

        public static Func<KeyValuePair<string, IReadOnlyDictionary<string, object>>,
                KeyValuePair<string, IReadOnlyDictionary<string, object>>,
                KeyValuePair<string, IReadOnlyDictionary<string, object>>>
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

        public static List<QueryEntity> CherryPickSelectedProperties<TQueryableEntity>(
            this IEnumerable<QueryEntity> entities,
            QueryClause<TQueryableEntity> query, RepositoryEntityMetadata metadata,
            IEnumerable<string> includeAdditionalProperties = null)
            where TQueryableEntity : IQueryableEntity
        {
            var selectedPropertyNames = query.GetAllSelectedFields();
            if (!selectedPropertyNames.Any())
            {
                return entities.ToList();
            }

            selectedPropertyNames = selectedPropertyNames
                .Concat(new[] {nameof(QueryEntity.Id)})
                .Concat(includeAdditionalProperties ?? Enumerable.Empty<string>())
                .ToList();

            return entities
                .Select(resultEntity => resultEntity.Properties
                    .Where(resultEntityProperty => selectedPropertyNames.Contains(resultEntityProperty.Key)))
                .Select(selectedProperties =>
                    EntityFromContainerProperties(selectedProperties.ToObjectDictionary(), metadata))
                .ToList();
        }

        private static QueryEntity EntityFromContainerProperties(
            this IReadOnlyDictionary<string, object> propertyValues, RepositoryEntityMetadata metadata)

        {
            return QueryEntity.FromProperties(propertyValues, metadata);
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