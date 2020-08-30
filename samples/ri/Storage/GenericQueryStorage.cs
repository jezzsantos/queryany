using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage
{
    public abstract class GenericQueryStorage<TEntity> : IQueryStorage<TEntity> where TEntity : IPersistableEntity
    {
        private readonly ILogger logger;
        private readonly IRepository repository;

        protected GenericQueryStorage(ILogger logger, IDomainFactory domainFactory, IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            repository.GuardAgainstNull(nameof(repository));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            this.logger = logger;
            this.repository = repository;
            DomainFactory = domainFactory;
        }

        protected virtual string ContainerName => typeof(TEntity).GetEntityNameSafe();

        public IDomainFactory DomainFactory { get; }

        public QueryResults<TEntity> Query(QueryClause<TEntity> query)
        {
            if (query == null || query.Options.IsEmpty)
            {
                this.logger.LogDebug("No entities were retrieved from repository");

                return new QueryResults<TEntity>(new List<TEntity>());
            }

            var entities = this.repository.Query(ContainerName, query, DomainFactory);

            this.logger.LogDebug("Entities were retrieved from repository");

            return new QueryResults<TEntity>(entities.ConvertAll(e => e));
        }

        public long Count()
        {
            return this.repository.Count(ContainerName);
        }

        public void DestroyAll()
        {
            this.repository.DestroyAll(ContainerName);
            this.logger.LogDebug("All entities were deleted from repository");
        }
    }
}