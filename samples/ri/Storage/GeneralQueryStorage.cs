using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage
{
    public class GeneralQueryStorage<TEntity> : IQueryStorage<TEntity> where TEntity : IPersistableEntity
    {
        private readonly string containerName;
        private readonly ILogger logger;
        private readonly IRepository repository;

        public GeneralQueryStorage(ILogger logger, IDomainFactory domainFactory, IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            repository.GuardAgainstNull(nameof(repository));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            this.logger = logger;
            this.repository = repository;
            DomainFactory = domainFactory;
            this.containerName = typeof(TEntity).GetEntityNameSafe();
        }

        public IDomainFactory DomainFactory { get; }

        public QueryResults<TEntity> Query(QueryClause<TEntity> query)
        {
            if (query == null || query.Options.IsEmpty)
            {
                this.logger.LogDebug("No entities were retrieved from repository");

                return new QueryResults<TEntity>(new List<TEntity>());
            }

            var entities = this.repository.Query(this.containerName, query, DomainFactory);

            this.logger.LogDebug("Entities were retrieved from repository");

            return new QueryResults<TEntity>(entities.ConvertAll(e => e));
        }

        public long Count()
        {
            return this.repository.Count(this.containerName);
        }

        public void DestroyAll()
        {
            this.repository.DestroyAll(this.containerName);
            this.logger.LogDebug("All entities were deleted from repository");
        }
    }
}