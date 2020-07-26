using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Dapper.Contrib.Extensions;
using QueryAny;
using Services.Interfaces.Entities;
using Storage.Interfaces;

namespace Storage.Azure
{
    public class AzureSqlServerRepository : IRepository
    {
        private readonly ISqlConnectionFactory connectionFactory;
        private readonly GuidIdentifierFactory guidIdentifierFactory;
        private readonly IIdentifierFactory idFactory;
        private readonly Dictionary<string, string> containers = new Dictionary<string, string>();

        public AzureSqlServerRepository(ISqlConnectionFactory connectionFactory, GuidIdentifierFactory guidIdentifierFactory, IIdentifierFactory idFactory)
        {
            this.connectionFactory = connectionFactory;
            this.connectionFactory = connectionFactory;
            this.guidIdentifierFactory = guidIdentifierFactory;
            this.idFactory = idFactory;
        }

        public void Dispose()
        {
            // throw new System.NotImplementedException();
        }

        public int MaxQueryResults { get; }
        public Identifier Add<TEntity>(string containerName, TEntity entity) where TEntity : IPersistableEntity
        {
            // INSERT INTO table_name (column1, column2, column3, ...)
            // VALUES (value1, value2, value3, ...);
            
            var id = this.idFactory.Create(entity);
            entity.Identify(id);
            Dictionary<string, object> sqlReadyDictionary = entity.ToSqlReadyDictionary();
            string columnNames = String.Join(",", sqlReadyDictionary.ToList().Select(x => x.Key));
            
            // todo (sdv) each column's value will need to be passed through a function which applied formatting according to it's datatype and sets its value to "" if not defined
            string values = String.Join(",", sqlReadyDictionary.ToList().Select(x => x.Value));
            
            
            string sql = $"INSERT INTO @containerName ({columnNames}) VALUES ({values})";

            // // todo (sdv) do I need this?
            // var container = EnsureTable(containerName);
            // todo (sdv) what if table does not exist?

            using (var connection = this.connectionFactory.GetSqlDbConnection())
            {
                connection.Execute(sql, new
                {
                    containerName
                });
            }
            
            return id;
        }
        
        public void Remove<TEntity>(string containerName, Identifier id) where TEntity : IPersistableEntity
        {
            string sql = $"DELETE FROM [dbo].@containerName WHERE ID = @{id}";

            using (var connection = this.connectionFactory.GetSqlDbConnection())
            {
                connection.Execute(sql);
            }
        }

        public TEntity Retrieve<TEntity>(string containerName, Identifier id, EntityFactory<TEntity> entityFactory) where TEntity : IPersistableEntity
        {
            throw new System.NotImplementedException();
        }

        public TEntity Replace<TEntity>(string containerName, Identifier id, TEntity entity, EntityFactory<TEntity> entityFactory) where TEntity : IPersistableEntity
        {
            throw new System.NotImplementedException();
        }

        public long Count(string containerName)
        {
            throw new System.NotImplementedException();
        }

        public List<TEntity> Query<TEntity>(string containerName, QueryClause<TEntity> query, EntityFactory<TEntity> entityFactory) where TEntity : IPersistableEntity
        {
            throw new System.NotImplementedException();
        }

        public void DestroyAll(string containerName)
        {
            // throw new System.NotImplementedException();
        }

       
    }
    
        internal static class AzureSqlServerEntityExtensions
        {

            public static Dictionary<string, object> ToSqlReadyDictionary<TEntity>(this TEntity entity) where TEntity : IPersistableEntity
            {
                bool IsNotExcluded(string propertyName)
                {
                    var excludedPropertyNames = new[] {nameof(IPersistableEntity.Id)};
                    return !excludedPropertyNames.Contains(propertyName);
                }

                IEnumerable<KeyValuePair<string, object>> entityPropertiesList = entity.Dehydrate()
                    .Where(pair => IsNotExcluded(pair.Key));
                
                // entityPropertiesList.ToList().Select(x=>x.).First().
                
                var entityProperties = entityPropertiesList
                    .ToDictionary(pair => pair.Key ,
                        pair => pair.Value);

                // var utcNow = DateTime.UtcNow;
                // if (entityProperties[nameof(IModifiableEntity.CreatedAtUtc)].DateTime
                //     .GetValueOrDefault(DateTime.MinValue).IsMinimumAllowableDate(options))
                // {
                //     tableEntity.Properties[nameof(IModifiableEntity.CreatedAtUtc)].DateTime = utcNow;
                // }
                //
                // tableEntity.Properties[nameof(IModifiableEntity.LastModifiedAtUtc)].DateTime = utcNow;

                return entityProperties;
            }
            
            // private static EntityProperty ToTableEntityProperty(object property,
            //     AzureTableStorageRepository.TableStorageApiOptions options)
            // {
            //     switch (property)
            //     {
            //         case string text:
            //             return EntityProperty.GeneratePropertyForString(text);
            //         case DateTime dateTime:
            //             return EntityProperty.GeneratePropertyForDateTimeOffset(
            //                 ToTableEntityDateTimeOffsetProperty(dateTime, options));
            //         case DateTimeOffset dateTimeOffset:
            //             return EntityProperty.GeneratePropertyForDateTimeOffset(
            //                 ToTableEntityDateTimeOffsetProperty(dateTimeOffset, options));
            //         case bool boolean:
            //             return EntityProperty.GeneratePropertyForBool(boolean);
            //         case int int32:
            //             return EntityProperty.GeneratePropertyForInt(int32);
            //         case long int64:
            //             return EntityProperty.GeneratePropertyForLong(int64);
            //         case double @double:
            //             return EntityProperty.GeneratePropertyForDouble(@double);
            //         case Guid guid:
            //             return EntityProperty.GeneratePropertyForGuid(guid);
            //         case byte[] bytes:
            //             return EntityProperty.GeneratePropertyForByteArray(bytes);
            //         case null:
            //             return EntityProperty.CreateEntityPropertyFromObject(AzureTableStorageRepository.NullValue);
            //
            //         default:
            //             if (property is IPersistableValueType valueType)
            //             {
            //                 return EntityProperty.GeneratePropertyForString(valueType.Dehydrate());
            //             }
            //
            //             return EntityProperty.GeneratePropertyForString(property.ToString());
            //     }
            
        }
}