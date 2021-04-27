using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny;
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

        public QueryResults<TDto> Query(QueryClause<TDto> query, bool includeDeleted = false)
        {
            if (query == null || query.Options.IsEmpty)
            {
                this.logger.LogDebug("No entities were retrieved from repository, the query is empty");

                return new QueryResults<TDto>(new List<TDto>());
            }

            var entities = this.repository.Query(this.containerName, query,
                RepositoryEntityMetadata.FromType<TDto>());

            entities = entities
                .Where(e => !e.IsDeleted.GetValueOrDefault(false) || includeDeleted)
                .ToList();

            this.logger.LogDebug($"{entities.Count} Entities were retrieved from repository");
            return new QueryResults<TDto>(entities.ConvertAll(x => x.ToEntity<TDto>(this.domainFactory)));
        }

        public TDtoWithId Get<TDtoWithId>(Identifier id, bool includeDeleted = false)
            where TDtoWithId : IQueryableEntity, IHasIdentity, new()
        {
            id.GuardAgainstNull(nameof(id));

            var entity = this.repository.Retrieve(this.containerName, id, RepositoryEntityMetadata.FromType<TDto>());
            if (entity == null)
            {
                return default;
            }

            if (entity.IsDeleted.GetValueOrDefault(false) && !includeDeleted)
            {
                return default;
            }

            this.logger.LogDebug($"Entity {id} was retrieved from repository");
            return entity.ToDto<TDtoWithId>();
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