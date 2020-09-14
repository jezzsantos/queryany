using System.Collections.Generic;
using QueryAny;

namespace Storage
{
    public interface IRepository
    {
        int MaxQueryResults { get; }

        CommandEntity Add(string containerName, CommandEntity entity);

        void Remove(string containerName, string id);

        CommandEntity Retrieve(string containerName, string id, RepositoryEntityMetadata metadata);

        CommandEntity Replace(string containerName, string id, CommandEntity entity);

        long Count(string containerName);

        List<QueryEntity> Query<TQueryableEntity>(string containerName, QueryClause<TQueryableEntity> query,
            RepositoryEntityMetadata metadata)
            where TQueryableEntity : IQueryableEntity;

        void DestroyAll(string containerName);
    }
}