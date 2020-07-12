﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using QueryAny;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage
{
    public class InProcessInMemRepository : IRepository
    {
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, object>>> containers =
            new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();

        private readonly IIdentifierFactory idFactory;

        public InProcessInMemRepository(IIdentifierFactory idFactory)
        {
            Guard.AgainstNull(() => idFactory, idFactory);
            this.idFactory = idFactory;
        }

        public string Add<TEntity>(string containerName, TEntity entity) where TEntity : IPersistableEntity, new()
        {
            if (!this.containers.ContainsKey(containerName))
            {
                this.containers.Add(containerName, new Dictionary<string, Dictionary<string, object>>());
            }

            var id = this.idFactory.Create(entity);
            entity.Identify(id);
            this.containers[containerName].Add(entity.Id, entity.ToContainerProperties());
            return id;
        }

        public void Remove<TEntity>(string containerName, string id) where TEntity : IPersistableEntity, new()
        {
            if (this.containers.ContainsKey(containerName))
            {
                if (this.containers[containerName].ContainsKey(id))
                {
                    this.containers[containerName].Remove(id);
                }
            }
        }

        public TEntity Replace<TEntity>(string containerName, string id, TEntity entity)
            where TEntity : IPersistableEntity, new()
        {
            if (this.containers.ContainsKey(containerName))
            {
                if (this.containers[containerName].ContainsKey(id))
                {
                    var entityProperties = entity.ToContainerProperties();
                    this.containers[containerName][id] = entityProperties;
                    return entityProperties.FromContainerProperties<TEntity>();
                }
            }

            return default;
        }

        public TEntity Retrieve<TEntity>(string containerName, string id) where TEntity : IPersistableEntity, new()
        {
            if (this.containers.ContainsKey(containerName))
            {
                if (this.containers[containerName].ContainsKey(id))
                {
                    return this.containers[containerName][id].FromContainerProperties<TEntity>();
                }
            }

            return default;
        }

        public long Count(string containerName)
        {
            if (this.containers.ContainsKey(containerName))
            {
                return this.containers[containerName].Count;
            }

            return 0;
        }

        public List<TEntity> Query<TEntity>(string containerName, QueryClause<TEntity> query)
            where TEntity : IPersistableEntity, new()
        {
            if (!this.containers.ContainsKey(containerName))
            {
                return new List<TEntity>();
            }

            var primaryEntities = QueryPrimaryEntities(containerName, query);

            var joinedContainers = query.JoinedEntities
                .Where(je => je.Join != null)
                .ToDictionary(je => je.EntityName, je => new
                {
                    Collection = this.containers.ContainsKey(je.EntityName)
                        ? this.containers[je.EntityName]
                        : new Dictionary<string, Dictionary<string, object>>(),
                    JoinedEntity = je
                });

            if (joinedContainers.Any())
            {
                foreach (var joinedContainer in joinedContainers)
                {
                    var joinedEntity = joinedContainer.Value.JoinedEntity;
                    var join = joinedEntity.Join;
                    var leftEntities = primaryEntities
                        .ToDictionary(e => e.Id, e => e.Dehydrate());
                    var rightEntities = joinedContainer.Value.Collection
                        .ToDictionary(e => e.Key, e => e.Value);

                    primaryEntities = join
                        .JoinResults<TEntity>(leftEntities, rightEntities,
                            joinedEntity.Selects.ProjectSelectedJoinedProperties());
                }
            }

            return primaryEntities.CherryPickSelectedProperties(query);
        }

        public void DestroyAll(string containerName)
        {
            if (this.containers.ContainsKey(containerName))
            {
                this.containers.Remove(containerName);
            }
        }

        public void Dispose()
        {
            // No need to do anything here. IDisposable is used as a marker interface
        }

        private List<TEntity> QueryPrimaryEntities<TEntity>(string containerName, QueryClause<TEntity> query)
            where TEntity : IPersistableEntity, new()
        {
            var primaryEntities = this.containers[containerName].Values
                .ToList()
                .ConvertAll(e => e.FromContainerProperties<TEntity>());

            if (!query.Wheres.Any())
            {
                return primaryEntities;
            }

            var queryExpression = query.Wheres.ToDynamicLinqWhereClause();
            primaryEntities = primaryEntities.AsQueryable()
                .Where(queryExpression)
                .ToList();

            return primaryEntities;
        }
    }

    public static class InMemEntityExtensions
    {
        public static Dictionary<string, object> ToContainerProperties(this IPersistableEntity entity)
        {
            var nowUtc = DateTime.UtcNow;
            var entityProperties = entity.Dehydrate();
            if (!entity.CreatedAtUtc.HasValue())
            {
                entityProperties[nameof(IModifiableEntity.CreatedAtUtc)] = nowUtc;
            }

            entityProperties[nameof(IModifiableEntity.LastModifiedAtUtc)] = nowUtc;

            return entityProperties;
        }

        public static TEntity FromContainerProperties<TEntity>(this Dictionary<string, object> entityProperties)
            where TEntity : IPersistableEntity, new()
        {
            var entity = new TEntity();
            entity.Rehydrate(entityProperties);
            return entity;
        }
    }

    public static class DynamicLinqWhereExtensions
    {
        public static string ToDynamicLinqWhereClause(this IEnumerable<WhereExpression> wheres)
        {
            var builder = new StringBuilder();
            foreach (var where in wheres)
            {
                builder.Append(where.ToDynamicLinqWhereClause());
            }

            return builder.ToString();
        }

        private static string ToDynamicLinqWhereClause(this WhereExpression where)
        {
            if (where.Condition != null)
            {
                var condition = where.Condition;
                return
                    $"{where.Operator.ToDynamicLinqWhereClause()}{condition.ToDynamicLinqWhereClause()}";
            }

            if (where.NestedWheres != null && where.NestedWheres.Any())
            {
                var builder = new StringBuilder();
                builder.Append($"{where.Operator.ToDynamicLinqWhereClause()}");
                builder.Append("(");
                foreach (var nestedWhere in where.NestedWheres)
                {
                    builder.Append($"{nestedWhere.ToDynamicLinqWhereClause()}");
                }

                builder.Append(")");
                return builder.ToString();
            }

            return string.Empty;
        }

        private static string ToDynamicLinqWhereClause(this LogicalOperator op)
        {
            switch (op)
            {
                case LogicalOperator.None:
                    return string.Empty;
                case LogicalOperator.And:
                    return " and ";
                case LogicalOperator.Or:
                    return " or ";
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }

        private static string ToDynamicLinqWhereClause(this ConditionOperator op)
        {
            switch (op)
            {
                case ConditionOperator.EqualTo:
                    return "==";
                case ConditionOperator.GreaterThan:
                    return ">";
                case ConditionOperator.GreaterThanEqualTo:
                    return ">=";
                case ConditionOperator.LessThan:
                    return "<";
                case ConditionOperator.LessThanEqualTo:
                    return "<=";
                case ConditionOperator.NotEqualTo:
                    return "!=";
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, null);
            }
        }

        private static string ToDynamicLinqWhereClause(this WhereCondition condition)
        {
            var fieldName = condition.FieldName;
            var @operator = condition.Operator.ToDynamicLinqWhereClause();
            var value = condition.Value;

            if (value is DateTime dateTime)
            {
                return
                    $"{fieldName} {@operator} DateTime({dateTime.Ticks}, DateTimeKind.Utc)";
            }

            if (value is DateTimeOffset dateTimeOffset)
            {
                return
                    $"{fieldName} {@operator} DateTimeOffset({dateTimeOffset.Ticks}, TimeSpan.Zero)";
            }

            if (value is byte[] bytes)
            {
                return $"Convert.ToBase64String({fieldName}) {@operator} \"{Convert.ToBase64String(bytes)}\"";
            }

            if (value is string || value is Guid)
            {
                return $"{fieldName} {@operator} \"{value}\"";
            }

            if (value is int || value is long || value is double)
            {
                return $"{fieldName} {@operator} {value}";
            }

            // Other Types (ComplexTypes must be equatable to their ToString() forms)
            return $"iif ({fieldName} != null, {fieldName}.ToString(), null) {@operator} {value.ToEscapedString()}";
        }

        private static string ToEscapedString(this object value)
        {
            if (value == null)
            {
                return "null";
            }

            var escapedJson = value
                .ToString()
                .Replace("\"", "\\\"");

            return $"\"{escapedJson}\"";
        }
    }
}