using System;
using System.Collections.Generic;
using System.Linq;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;
using Storage.Interfaces;
using StringExtensions = ServiceStack.StringExtensions;

namespace Storage
{
    public static class RepositoryExtensions
    {
        public static List<TEntity> JoinResults<TEntity>(this JoinDefinition joinDefinition,
            Dictionary<string, Dictionary<string, object>> leftEntities,
            Dictionary<string, Dictionary<string, object>> rightEntities,
            Func<KeyValuePair<string, Dictionary<string, object>>, KeyValuePair<string, Dictionary<string, object>>,
                KeyValuePair<string, Dictionary<string, object>>> mapFunc = null)
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

        public static Func<KeyValuePair<string, Dictionary<string, object>>,
                KeyValuePair<string, Dictionary<string, object>>, KeyValuePair<string, Dictionary<string, object>>>
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
            QueryClause<TEntity> query) where TEntity : IKeyedEntity, new()
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

            return entities
                .Select(resultEntity => resultEntity.ToObjectDictionary()
                    .Where(resultEntityProperty =>
                        selectedPropertyNames.Contains(resultEntityProperty.Key) ||
                        StringExtensions.EqualsIgnoreCase(resultEntityProperty.Key, nameof(IKeyedEntity.Id))))
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