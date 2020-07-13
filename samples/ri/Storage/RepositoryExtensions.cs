using System;
using System.Collections.Generic;
using System.Linq;
using QueryAny;
using QueryAny.Primitives;
using Services.Interfaces.Entities;
using ServiceStack;
using Storage.Interfaces;

namespace Storage
{
    public static class RepositoryExtensions
    {
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
            where TEntity : IPersistableEntity, new()
        {
            var primarySelects = query.PrimaryEntity.Selects;
            var joinedSelects = query.JoinedEntities.SelectMany(je => je.Selects);

            var selectedPropertyNames = primarySelects
                .Select(select => select.FieldName)
                .Concat(joinedSelects.Select(select => select.FieldName))
                .ToList();

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