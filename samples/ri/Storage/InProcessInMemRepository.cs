using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Domain.Interfaces;
using QueryAny;
using QueryAny.Primitives;

namespace Storage
{
    public class InProcessInMemRepository : IRepository
    {
        private readonly Dictionary<string, Dictionary<string, IReadOnlyDictionary<string, object>>> containers =
            new Dictionary<string, Dictionary<string, IReadOnlyDictionary<string, object>>>();

        public int MaxQueryResults => 1000;

        public CommandEntity Add(string containerName, CommandEntity entity)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            entity.GuardAgainstNull(nameof(entity));

            if (!this.containers.ContainsKey(containerName))
            {
                this.containers.Add(containerName, new Dictionary<string, IReadOnlyDictionary<string, object>>());
            }

            this.containers[containerName].Add(entity.Id, entity.ToContainerProperties());

            return CommandEntity.FromCommandEntity(this.containers[containerName][entity.Id].FromContainerProperties(),
                entity);
        }

        public void Remove(string containerName, string id)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            id.GuardAgainstNullOrEmpty(nameof(id));

            if (this.containers.ContainsKey(containerName))
            {
                if (this.containers[containerName].ContainsKey(id))
                {
                    this.containers[containerName].Remove(id);
                }
            }
        }

        public CommandEntity Retrieve(string containerName, string id, RepositoryEntityMetadata metadata)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            id.GuardAgainstNullOrEmpty(nameof(id));
            metadata.GuardAgainstNull(nameof(metadata));

            if (this.containers.ContainsKey(containerName))
            {
                if (this.containers[containerName].ContainsKey(id))
                {
                    return CommandEntity.FromCommandEntity(
                        this.containers[containerName][id].FromContainerProperties(), metadata);
                }
            }

            return default;
        }

        public CommandEntity Replace(string containerName, string id, CommandEntity entity)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            id.GuardAgainstNullOrEmpty(nameof(id));
            entity.GuardAgainstNull(nameof(entity));

            var entityProperties = entity.ToContainerProperties();
            this.containers[containerName][id] = entityProperties;

            return CommandEntity.FromCommandEntity(entityProperties.FromContainerProperties(), entity);
        }

        public long Count(string containerName)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));

            if (this.containers.ContainsKey(containerName))
            {
                return this.containers[containerName].Count;
            }

            return 0;
        }

        public List<QueryEntity> Query<TQueryableEntity>(string containerName, QueryClause<TQueryableEntity> query,
            RepositoryEntityMetadata metadata)
            where TQueryableEntity : IQueryableEntity
        {
            if (query == null || query.Options.IsEmpty)
            {
                return new List<QueryEntity>();
            }

            if (!this.containers.ContainsKey(containerName))
            {
                return new List<QueryEntity>();
            }

            var primaryEntities = QueryPrimaryEntities(containerName, query, metadata);

            var joinedContainers = query.JoinedEntities
                .Where(je => je.Join != null)
                .ToDictionary(je => je.EntityName, je => new
                {
                    Collection = this.containers.ContainsKey(je.EntityName)
                        ? this.containers[je.EntityName]
                            .ToDictionary(pair => pair.Key, pair => pair.Value.AsDictionary())
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
                        .ToDictionary(e => e.Id, e => e.Properties);
                    var rightEntities = joinedContainer.Value.Collection
                        .ToDictionary(e => e.Key, e => e.Value.AsReadOnly());

                    primaryEntities = join
                        .JoinResults(leftEntities, rightEntities, metadata,
                            joinedEntity.Selects.ProjectSelectedJoinedProperties());
                }
            }

            return primaryEntities.CherryPickSelectedProperties(query, metadata);
        }

        public void DestroyAll(string containerName)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));

            if (this.containers.ContainsKey(containerName))
            {
                this.containers.Remove(containerName);
            }
        }

        private List<QueryEntity> QueryPrimaryEntities<TQueryableEntity>(string containerName,
            QueryClause<TQueryableEntity> query, RepositoryEntityMetadata metadata)
            where TQueryableEntity : IQueryableEntity

        {
            var primaryEntities = this.containers[containerName];

            var orderByExpression = query.ToDynamicLinqOrderByClause();
            var primaryEntitiesDynamic = primaryEntities.AsQueryable()
                .OrderBy(orderByExpression)
                .Skip(query.GetDefaultSkip())
                .Take(query.GetDefaultTake(this));

            if (!query.Wheres.Any())
            {
                return primaryEntitiesDynamic
                    .Select(ped => QueryEntity.FromProperties(ped.Value, metadata))
                    .ToList();
            }

            var queryExpression = query.Wheres.ToDynamicLinqWhereClause();
            return primaryEntitiesDynamic
                .Where(queryExpression)
                .Select(pe => QueryEntity.FromProperties(pe.Value, metadata))
                .ToList();
        }
    }

    public static class InMemEntityExtensions
    {
        public static IReadOnlyDictionary<string, object> ToContainerProperties(this CommandEntity entity)
        {
            entity.LastPersistedAtUtc = DateTime.UtcNow;
            return entity.Properties;
        }

        public static IReadOnlyDictionary<string, object> FromContainerProperties(
            this IReadOnlyDictionary<string, object> entityProperties)
        {
            return entityProperties;
        }
    }
}