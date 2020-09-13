using System.Collections.Generic;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny;
using QueryAny.Primitives;
using Storage.Interfaces;

namespace Storage
{
    public class GeneralQueryStorage<TDto> : IQueryStorage<TDto> where TDto : IQueryableEntity, new()
    {
        private readonly string containerName;
        private readonly IDomainFactory domainFactory;
        private readonly ILogger logger;
        private readonly IRepository repository;

        public GeneralQueryStorage(ILogger logger, IDomainFactory domainFactory, IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            repository.GuardAgainstNull(nameof(repository));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            this.logger = logger;
            this.repository = repository;
            this.domainFactory = domainFactory;
            this.containerName = typeof(TDto).GetEntityNameSafe();
        }

        public QueryResults<TDto> Query(QueryClause<TDto> query)
        {
            if (query == null || query.Options.IsEmpty)
            {
                this.logger.LogDebug("No entities were retrieved from repository, the query is empty");

                return new QueryResults<TDto>(new List<TDto>());
            }

            var entities = this.repository.Query(this.containerName, query,
                RepositoryEntityMetadata.FromType<TDto>());

            this.logger.LogDebug($" {entities.Count} Entities were retrieved from repository");

            return new QueryResults<TDto>(entities.ConvertAll(x => x.ToEntity<TDto>(this.domainFactory)));
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