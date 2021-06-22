using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using QueryAny;
using Storage.Interfaces;

namespace Storage
{
    public class InProcessInMemRepository : IRepository, IBlobository
    {
        private readonly Dictionary<string, Dictionary<string, IReadOnlyDictionary<string, object>>> containers =
            new Dictionary<string, Dictionary<string, IReadOnlyDictionary<string, object>>>();

        public Blob Download(string containerName, string blobName, Stream stream)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            blobName.GuardAgainstNullOrEmpty(nameof(blobName));
            stream.GuardAgainstNull(nameof(stream));

            if (this.containers.ContainsKey(containerName))
            {
                if (this.containers[containerName].ContainsKey(blobName))
                {
                    var properties = this.containers[containerName][blobName].FromDictionaryProperties();
                    stream.Write(Convert.FromBase64String(properties["Data"].ToString()));
                    return new Blob
                    {
                        ContentType = properties[nameof(Blob.ContentType)].ToString()
                    };
                }
            }

            return null;
        }

        public void Upload(string containerName, string blobName, string contentType, byte[] data)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            blobName.GuardAgainstNullOrEmpty(nameof(blobName));
            contentType.GuardAgainstNullOrEmpty(nameof(contentType));
            data.GuardAgainstNull(nameof(data));

            if (!this.containers.ContainsKey(containerName))
            {
                this.containers.Add(containerName, new Dictionary<string, IReadOnlyDictionary<string, object>>());
            }

            var properties = new Dictionary<string, object>
            {
                {"ContentType", contentType},
                {"Data", Convert.ToBase64String(data)}
            };
            if (this.containers[containerName].ContainsKey(blobName))
            {
                this.containers[containerName][blobName] = properties;
                return;
            }

            this.containers[containerName].Add(blobName, properties);
        }

        public int MaxQueryResults => 1000;

        public CommandEntity Add(string containerName, CommandEntity entity)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            entity.GuardAgainstNull(nameof(entity));

            if (!this.containers.ContainsKey(containerName))
            {
                this.containers.Add(containerName, new Dictionary<string, IReadOnlyDictionary<string, object>>());
            }

            this.containers[containerName].Add(entity.Id, entity.ToDictionaryProperties());

            return CommandEntity.FromCommandEntity(this.containers[containerName][entity.Id].FromDictionaryProperties(),
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
                        this.containers[containerName][id].FromDictionaryProperties(), metadata);
                }
            }

            return default;
        }

        public CommandEntity Replace(string containerName, string id, CommandEntity entity)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            id.GuardAgainstNullOrEmpty(nameof(id));
            entity.GuardAgainstNull(nameof(entity));

            var entityProperties = entity.ToDictionaryProperties();
            this.containers[containerName][id] = entityProperties;

            return CommandEntity.FromCommandEntity(entityProperties.FromDictionaryProperties(), entity);
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

            var results = query.FetchAllIntoMemory(this, metadata, () => QueryPrimaryEntities(containerName),
                QueryJoiningContainer);

            return results;
        }

        public void DestroyAll(string containerName)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));

            if (this.containers.ContainsKey(containerName))
            {
                this.containers.Remove(containerName);
            }
        }

        private Dictionary<string, IReadOnlyDictionary<string, object>> QueryPrimaryEntities(string containerName)
        {
            return this.containers[containerName];
        }

        private Dictionary<string, IReadOnlyDictionary<string, object>> QueryJoiningContainer(
            QueriedEntity joinedEntity)
        {
            return this.containers.ContainsKey(joinedEntity.EntityName)
                ? this.containers[joinedEntity.EntityName]
                    .ToDictionary(pair => pair.Key, pair => pair.Value)
                : new Dictionary<string, IReadOnlyDictionary<string, object>>();
        }
    }

    internal static class InProcessInMemRepositoryExtensions
    {
        public static IReadOnlyDictionary<string, object> ToDictionaryProperties(this CommandEntity entity)
        {
            entity.LastPersistedAtUtc = DateTime.UtcNow;
            return entity.Properties;
        }

        public static IReadOnlyDictionary<string, object> FromDictionaryProperties(
            this IReadOnlyDictionary<string, object> entityProperties)
        {
            return entityProperties;
        }
    }
}