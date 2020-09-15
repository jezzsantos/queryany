using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using Domain.Interfaces;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Text;

namespace Storage
{
    /// <summary>
    ///     Defines a file repository on the local machine, that stores each entity as raw JSON.
    ///     store is located in named folders under the <see cref="rootPath" />
    /// </summary>
    public class LocalMachineFileRepository : IRepository
    {
        public const string NullToken = @"null";
        private readonly string rootPath;

        public LocalMachineFileRepository(string rootPath)
        {
            rootPath.GuardAgainstNullOrEmpty(nameof(rootPath));
            rootPath.GuardAgainstInvalid(ValidateRootPath, nameof(rootPath));

            this.rootPath = rootPath;
        }

        public int MaxQueryResults => 1000;

        public CommandEntity Add(string containerName, CommandEntity entity)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            entity.GuardAgainstNull(nameof(entity));

            var container = EnsureContainer(containerName);

            container.Add(entity.Id, entity.ToFileProperties());

            return CommandEntity.FromCommandEntity(
                container.Get(entity.Id).FromFileProperties(entity.Metadata),
                entity);
        }

        public void Remove(string containerName, string id)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            id.GuardAgainstNullOrEmpty(nameof(id));

            var container = EnsureContainer(containerName);

            if (container.Exists(id))
            {
                container.Remove(id);
            }
        }

        public CommandEntity Retrieve(string containerName, string id, RepositoryEntityMetadata metadata)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            id.GuardAgainstNullOrEmpty(nameof(id));
            metadata.GuardAgainstNull(nameof(metadata));

            var container = EnsureContainer(containerName);

            if (container.Exists(id))
            {
                return CommandEntity.FromCommandEntity(
                    container.Get(id).FromFileProperties(metadata), metadata);
            }

            return default;
        }

        public CommandEntity Replace(string containerName, string id, CommandEntity entity)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            id.GuardAgainstNullOrEmpty(nameof(id));
            entity.GuardAgainstNull(nameof(entity));

            var container = EnsureContainer(containerName);

            var entityProperties = entity.ToFileProperties();
            container.Update(id, entityProperties);

            return CommandEntity.FromCommandEntity(entityProperties.FromFileProperties(entity.Metadata),
                entity);
        }

        public long Count(string containerName)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));

            var container = EnsureContainer(containerName);

            return container.Count;
        }

        public List<QueryEntity> Query<TQueryableEntity>(string containerName, QueryClause<TQueryableEntity> query,
            RepositoryEntityMetadata metadata)
            where TQueryableEntity : IQueryableEntity
        {
            if (query == null || query.Options.IsEmpty)
            {
                return new List<QueryEntity>();
            }

            var container = EnsureContainer(containerName);

            if (container.IsEmpty())
            {
                return new List<QueryEntity>();
            }

            var primaryEntities = QueryPrimaryEntities(container, query, metadata);

            var joinedContainers = query.JoinedEntities
                .Where(je => je.Join != null)
                .ToDictionary(je => je.EntityName, je => new
                {
                    Collection = QueryJoiningContainer(je),
                    JoinedEntity = je
                });

            if (joinedContainers.Any())
            {
                foreach (var joinedContainer in joinedContainers)
                {
                    var joinedEntity = joinedContainer.Value.JoinedEntity;
                    var join = joinedEntity.Join;
                    var leftEntities = primaryEntities
                        .ToDictionary(e => e.Id.ToString(), e => e.Properties);
                    var rightEntities = joinedContainer.Value.Collection
                        .ToDictionary(e => e.Key, e => e.Value.Properties);

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

            var container = EnsureContainer(containerName);

            container.Erase();
        }

        public static LocalMachineFileRepository FromAppSettings(IAppSettings settings)
        {
            var rootPath = settings.GetString("LocalMachineRepositoryRootPath");
            return new LocalMachineFileRepository(rootPath);
        }

        private List<QueryEntity> QueryPrimaryEntities<TQueryableEntity>(FileContainer container,
            QueryClause<TQueryableEntity> query, RepositoryEntityMetadata metadata)
            where TQueryableEntity : IQueryableEntity
        {
            var primaryEntities = container.GetEntityIds()
                .ToDictionary(id => id, id => GetEntityFromFile(container, id, metadata));

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
                .Select(ped => QueryEntity.FromProperties(ped.Value, metadata))
                .ToList();
        }

        private Dictionary<string, QueryEntity> QueryJoiningContainer(QueriedEntity joinedEntity)
        {
            var containerName = joinedEntity.EntityName;
            var container = EnsureContainer(containerName);
            if (container.IsEmpty())
            {
                return new Dictionary<string, QueryEntity>();
            }

            var metadata = RepositoryEntityMetadata.FromType(joinedEntity.Join.Right.EntityType);
            return container.GetEntityIds()
                .ToDictionary(id => id,
                    id => QueryEntity.FromProperties(GetEntityFromFile(container, id, metadata), metadata));
        }

        private static IReadOnlyDictionary<string, object> GetEntityFromFile(FileContainer container,
            string id, RepositoryEntityMetadata metadata)

        {
            try
            {
                var containerEntityProperties = container.Get(id);
                if (containerEntityProperties == null || containerEntityProperties.Count == 0)
                {
                    return default;
                }

                return containerEntityProperties.FromFileProperties(metadata);
            }
            catch (Exception)
            {
                return default;
            }
        }

        private FileContainer EnsureContainer(string containerName)
        {
            return new FileContainer(this.rootPath, containerName);
        }

        private static bool ValidateRootPath(string path)
        {
            if (!path.HasValue())
            {
                return false;
            }

            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var dir = new DirectoryInfo(path);
                var testDir = dir.CreateSubdirectory("temp");
                testDir.Delete();

                var testFilePath = Path.Combine(path, "temp.tst");
                using (var testFile = File.CreateText(testFilePath))
                {
                    testFile.WriteLine();
                }
                File.Delete(testFilePath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private class FileContainer
        {
            private const string FileExtension = "json";
            private readonly string dirPath;

            public FileContainer(string rootPath, string containerName)
            {
                this.dirPath =
                    CleanDirectoryPath(Path.Combine(Environment.ExpandEnvironmentVariables(rootPath), containerName));
                if (!Directory.Exists(this.dirPath))
                {
                    Directory.CreateDirectory(this.dirPath);
                }
            }

            public long Count => Directory.GetFiles(this.dirPath).Length;

            public bool IsEmpty()
            {
                return Count == 0;
            }

            public void Add(string entityId, IReadOnlyDictionary<string, string> properties)
            {
                var filename = GetFullFilePathFromId(entityId);
                using (var file = File.CreateText(filename))
                {
                    var json = properties.ToJson().IndentJson();
                    file.Write(json);
                    file.Flush();
                }
            }

            public IReadOnlyDictionary<string, string> Get(string entityId)
            {
                if (Exists(entityId))
                {
                    var filename = GetFullFilePathFromId(entityId);
                    var content = File.ReadAllText(filename);
                    return content.FromJson<Dictionary<string, string>>();
                }

                return null;
            }

            public IEnumerable<string> GetEntityIds()
            {
                return Directory.GetFiles(this.dirPath)
                    .Select(GetIdFromFullFilePath)
                    .ToList();
            }

            public bool Exists(string entityId)
            {
                var filename = GetFullFilePathFromId(entityId);
                return File.Exists(filename);
            }

            public void Remove(string entityId)
            {
                if (Exists(entityId))
                {
                    var filename = GetFullFilePathFromId(entityId);
                    File.Delete(filename);
                }
            }

            public void Update(string entityId, IReadOnlyDictionary<string, string> properties)
            {
                if (Exists(entityId))
                {
                    var filename = GetFullFilePathFromId(entityId);
                    using (var file = File.CreateText(filename))
                    {
                        var json = properties.ToJson().IndentJson();
                        file.Write(json);
                        file.Flush();
                    }
                }
            }

            public void Erase()
            {
                var dir = new DirectoryInfo(this.dirPath);
                dir.Delete(true);
            }

            private static string CleanDirectoryPath(string path)
            {
                return string.Join("", path.Split(Path.GetInvalidPathChars()));
            }

            private static string CleanFileName(string fileName)
            {
                return string.Join("", fileName.Split(Path.GetInvalidFileNameChars()));
            }

            private string GetFullFilePathFromId(string entityId)
            {
                var filename = $"{CleanFileName(entityId)}.{FileExtension}";
                return Path.Combine(this.dirPath, filename);
            }

            private string GetIdFromFullFilePath(string path)
            {
                return Path.GetFileNameWithoutExtension(path);
            }
        }
    }

    internal static class LocalMachineFileRepositoryExtensions
    {
        public static IReadOnlyDictionary<string, string> ToFileProperties(this CommandEntity entity)
        {
            var entityProperties = entity.Properties;
            var containerEntityProperties = new Dictionary<string, string>();
            foreach (var pair in entityProperties)
            {
                string value;
                switch (pair.Value)
                {
                    case DateTime dateTime:
                        if (!dateTime.HasValue())
                        {
                            dateTime = DateTime.MinValue;
                        }

                        value = dateTime.ToIso8601();

                        break;

                    case DateTimeOffset dateTimeOffset:
                        if (dateTimeOffset == DateTimeOffset.MinValue)
                        {
                            dateTimeOffset = DateTimeOffset.MinValue.ToUniversalTime();
                        }

                        value = dateTimeOffset.ToIso8601();

                        break;

                    case Guid guid:
                        value = guid.ToString("D");

                        break;

                    case byte[] bytes:
                        value = Convert.ToBase64String(bytes);

                        break;

                    case string text:
                        value = text;

                        break;

                    case null:
                        value = LocalMachineFileRepository.NullToken;

                        break;

                    default:
                        value = pair.Value.ToString();

                        break;
                }

                containerEntityProperties.Add(pair.Key, value);
            }

            containerEntityProperties[nameof(CommandEntity.LastPersistedAtUtc)] = DateTime.UtcNow.ToIso8601();

            return containerEntityProperties;
        }

        public static IReadOnlyDictionary<string, object> FromFileProperties(
            this IReadOnlyDictionary<string, string> containerProperties,
            RepositoryEntityMetadata metadata)

        {
            var containerEntityProperties = containerProperties
                .Where(pair =>
                    metadata.HasType(pair.Key) && pair.Value != null)
                .ToDictionary(pair => pair.Key,
                    pair => pair.Value.FromFileProperty(metadata.GetPropertyType(pair.Key)));

            return containerEntityProperties;
        }

        private static object FromFileProperty(this string propertyValue, Type targetPropertyType)
        {
            if (propertyValue == null
                || propertyValue == LocalMachineFileRepository.NullToken)
            {
                return null;
            }

            if (targetPropertyType == typeof(bool) || targetPropertyType == typeof(bool?))
            {
                return bool.Parse(propertyValue);
            }

            if (targetPropertyType == typeof(DateTime) || targetPropertyType == typeof(DateTime?))
            {
                return propertyValue.FromIso8601();
            }

            if (targetPropertyType == typeof(DateTimeOffset) || targetPropertyType == typeof(DateTimeOffset?))
            {
                return DateTimeOffset.ParseExact(propertyValue, "O", null).ToUniversalTime();
            }

            if (targetPropertyType == typeof(Guid) || targetPropertyType == typeof(Guid?))
            {
                return Guid.Parse(propertyValue);
            }

            if (targetPropertyType == typeof(double) || targetPropertyType == typeof(double?))
            {
                return double.Parse(propertyValue);
            }

            if (targetPropertyType == typeof(long) || targetPropertyType == typeof(long?))
            {
                return long.Parse(propertyValue);
            }

            if (targetPropertyType == typeof(int) || targetPropertyType == typeof(int?))
            {
                return int.Parse(propertyValue);
            }

            if (targetPropertyType == typeof(byte[]))
            {
                return Convert.FromBase64String(propertyValue);
            }

            if (targetPropertyType.IsComplexStorageType())
            {
                if (propertyValue.HasValue())
                {
                    return propertyValue.ComplexTypeFromContainerProperty(targetPropertyType);
                }
            }

            return propertyValue;
        }
    }
}