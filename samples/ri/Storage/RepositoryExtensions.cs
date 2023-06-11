using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Common;
using Newtonsoft.Json;
using QueryAny;
using ObjectExtensions = ServiceStack.PlatformExtensions;
using StringExtensions = ServiceStack.StringExtensions;

namespace Storage;

public static class RepositoryExtensions
{
    public const string DefaultOrderingPropertyName = nameof(QueryEntity.LastPersistedAtUtc);
    public const string BackupOrderingPropertyName = nameof(QueryEntity.Id);

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
        var by = query.ResultOptions.OrderBy.By;
        if (!by.HasValue())
        {
            by = DefaultOrderingPropertyName;
        }

        var selectedFields = query.GetAllSelectedFields();
        if (selectedFields.Any())
        {
            return selectedFields.Contains(by)
                ? by
                : selectedFields.Contains(BackupOrderingPropertyName)
                    ? BackupOrderingPropertyName
                    : selectedFields.First();
        }

        return HasProperty<TQueryableEntity>(by)
            ? by
            : HasProperty<TQueryableEntity>(BackupOrderingPropertyName)
                ? BackupOrderingPropertyName
                : FirstProperty<TQueryableEntity>();
    }

    public static List<QueryEntity> FetchAllIntoMemory<TQueryableEntity>(this QueryClause<TQueryableEntity> query,
        IRepository repository,
        RepositoryEntityMetadata metadata,
        Func<Dictionary<string, IReadOnlyDictionary<string, object>>> getPrimaryEntities,
        Func<QueriedEntity, Dictionary<string, IReadOnlyDictionary<string, object>>> getJoinedEntities)
        where TQueryableEntity : IQueryableEntity
    {
        repository.GuardAgainstNull(nameof(repository));
        query.GuardAgainstNull(nameof(query));
        metadata.GuardAgainstNull(nameof(metadata));
        getPrimaryEntities.GuardAgainstNull(nameof(getPrimaryEntities));
        getJoinedEntities.GuardAgainstNull(nameof(getJoinedEntities));

        var take = query.GetDefaultTake(repository);
        if (take == 0)
        {
            return new List<QueryEntity>();
        }

        var primaryEntities = getPrimaryEntities();
        if (!primaryEntities.HasAny())
        {
            return new List<QueryEntity>();
        }

        var joinedContainers = query.JoinedEntities
            .Where(je => je.Join.Exists())
            .ToDictionary(je => je.EntityName, je => new
            {
                Collection = getJoinedEntities(je),
                JoinedEntity = je
            });

        List<KeyValuePair<string, IReadOnlyDictionary<string, object>>> joinedEntities = null;
        if (!joinedContainers.Any())
        {
            joinedEntities = primaryEntities
                .Select(pe => new KeyValuePair<string, IReadOnlyDictionary<string, object>>(pe.Key, pe.Value))
                .ToList();
        }
        else
        {
            foreach (var joinedContainer in joinedContainers)
            {
                var joinedEntity = joinedContainer.Value.JoinedEntity;
                var join = joinedEntity.Join;
                var rightEntities = joinedContainer.Value.Collection
                    .ToDictionary(e => e.Key, e => e.Value);

                joinedEntities = join
                    .JoinResults(primaryEntities, rightEntities,
                        joinedEntity.Selects.ProjectSelectedJoinedProperties());
            }
        }

        var results = joinedEntities?.AsQueryable();
        var orderBy = query.ToDynamicLinqOrderByClause();
        var skip = query.GetDefaultSkip();

        if (query.Wheres.Any())
        {
            var whereBy = query.Wheres.ToDynamicLinqWhereClause();
            results = results!
                .Where(whereBy);
        }
        return results!
            .OrderBy(orderBy)
            .Skip(skip)
            .Take(take)
            .Select(sel => new KeyValuePair<string, IReadOnlyDictionary<string, object>>(sel.Key, sel.Value))
            .CherryPickSelectedProperties(query)
            .Select(ped => QueryEntity.FromProperties(ped.Value, metadata))
            .ToList();
    }

    public static string ComplexTypeToContainerProperty(this object propertyValue)
    {
        var propertyType = propertyValue.GetType();
        if (!propertyType.IsComplexStorageType())
        {
            return propertyValue.ToString();
        }

        if (HasToStringMethodBeenOverriden(propertyType))
        {
            return propertyValue.ToString();
        }

        return StringExtensions.ToJson(propertyValue);
    }

    public static object ComplexTypeFromContainerProperty(this string propertyValue, Type targetPropertyType)
    {
        if (propertyValue.HasValue())
        {
            if (!targetPropertyType.IsComplexStorageType())
            {
                return propertyValue;
            }

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

    private static List<KeyValuePair<string, IReadOnlyDictionary<string, object>>> JoinResults(
        this JoinDefinition joinDefinition,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> leftEntities,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, object>> rightEntities,
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
                    .Select(join =>
                        new KeyValuePair<string, IReadOnlyDictionary<string, object>>(join.Key, join.Value))
                    .ToList();

            case JoinType.Left:
                var leftJoin = from lefts in leftEntities
                    join rights in rightEntities on lefts.Value[joinDefinition.Left.JoinedFieldName] equals
                        rights.Value[joinDefinition.Right.JoinedFieldName]
                        into joined
                    from result in joined.DefaultIfEmpty()
                    select mapFunc?.Invoke(lefts, result) ?? lefts;

                return leftJoin
                    .Select(join =>
                        new KeyValuePair<string, IReadOnlyDictionary<string, object>>(join.Key, join.Value))
                    .ToList();

            default:
                throw new ArgumentOutOfRangeException(nameof(JoinType));
        }
    }

    private static Func<KeyValuePair<string, IReadOnlyDictionary<string, object>>,
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

            var leftEntityProperties = ObjectExtensions.ToObjectDictionary(leftEntity.Value);
            var rightEntityProperties = ObjectExtensions.ToObjectDictionary(rightEntity.Value);
            foreach (var select in selectedFromJoinPropertyNames)
            {
                if (!rightEntityProperties.HasPropertyValue(select.FieldName)) //select.FieldName
                {
                    continue;
                }

                leftEntityProperties.CreatePropertyIfNotExists(select.JoinedFieldName); //select.JoinedFiledName
                leftEntityProperties.CopyPropertyValue(rightEntityProperties, select);
            }

            return leftEntity;
        };
    }

    private static IEnumerable<KeyValuePair<string, IReadOnlyDictionary<string, object>>>
        CherryPickSelectedProperties<
            TQueryableEntity>(this IQueryable<KeyValuePair<string, IReadOnlyDictionary<string, object>>> entities,
            QueryClause<TQueryableEntity> query)
        where TQueryableEntity : IQueryableEntity
    {
        var selectedPropertyNames = query.GetAllSelectedFields();

        if (!selectedPropertyNames.Any())
        {
            return entities;
        }

        selectedPropertyNames = selectedPropertyNames
            .Concat(new[] { nameof(QueryEntity.Id) })
            .ToList();

        return entities
            .Select(entity => FilterSelectedFields(entity, selectedPropertyNames))
            .Select(sel => new KeyValuePair<string, IReadOnlyDictionary<string, object>>(sel.Key, sel.Value));
    }

    private static KeyValuePair<string, IReadOnlyDictionary<string, object>> FilterSelectedFields(
        KeyValuePair<string, IReadOnlyDictionary<string, object>> entity, List<string> allowedFieldNames)
    {
        return new KeyValuePair<string, IReadOnlyDictionary<string, object>>(entity.Key,
            entity.Value.Where(fieldNameValue => allowedFieldNames.Contains(fieldNameValue.Key))
                .ToDictionary(pair => pair.Key, pair => pair.Value));
    }

    private static List<string> GetAllSelectedFields<TQueryableEntity>(this QueryClause<TQueryableEntity> query)
        where TQueryableEntity : IQueryableEntity

    {
        var primarySelects = query.PrimaryEntity.Selects;
        var joinedSelects = query.JoinedEntities.SelectMany(je => je.Selects);

        return primarySelects
            .Select(select => select.FieldName)
            .Concat(joinedSelects.Select(select => select.JoinedFieldName))
            .ToList();
    }

    private static bool HasPropertyValue(this Dictionary<string, object> entityProperties, string propertyName)
    {
        return entityProperties != null && entityProperties.ContainsKey(propertyName);
    }

    private static bool HasProperty<TQueryableEntity>(string propertyName) where TQueryableEntity : IQueryableEntity
    {
        var metadata = RepositoryEntityMetadata.FromType<TQueryableEntity>();
        return metadata.HasType(propertyName);
    }

    private static string FirstProperty<TQueryableEntity>() where TQueryableEntity : IQueryableEntity
    {
        var metadata = RepositoryEntityMetadata.FromType<TQueryableEntity>();
        return metadata.Types.First().Key;
    }

    private static void CreatePropertyIfNotExists(this Dictionary<string, object> entityProperties,
        string propertyName)
    {
        entityProperties.TryAdd(propertyName, default);
    }

    private static void CopyPropertyValue(this Dictionary<string, object> toEntityProperties,
        Dictionary<string, object> fromEntityProperties,
        SelectDefinition select)
    {
        toEntityProperties[select.JoinedFieldName] =
            fromEntityProperties[select.FieldName];
    }

    private static bool HasToStringMethodBeenOverriden(Type propertyType)
    {
        return propertyType.GetMethod(nameof(ToString))?.DeclaringType == propertyType;
    }
}