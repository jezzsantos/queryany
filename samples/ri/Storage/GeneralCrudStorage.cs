using System.Collections.Generic;
using System.Linq;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using QueryAny;
using ServiceStack;
using Storage.Interfaces;
using Storage.Properties;

namespace Storage
{
    public class GeneralCrudStorage<TDto> : ICrudStorage<TDto> where TDto : IPersistableDto, new()
    {
        private readonly string containerName;

        private readonly ILogger logger;

        private readonly IRepository repository;

        public GeneralCrudStorage(ILogger logger, IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            repository.GuardAgainstNull(nameof(repository));
            this.logger = logger;
            this.repository = repository;
            this.containerName = typeof(TDto).GetEntityNameSafe();
        }

        public void Delete(string id, bool destroy = true)
        {
            id.GuardAgainstNull(nameof(id));

            var entity = this.repository.Retrieve(this.containerName, id,
                RepositoryEntityMetadata.FromType<TDto>());
            if (entity == null)
            {
                return;
            }

            if (destroy)
            {
                this.repository.Remove(this.containerName, id);
                this.logger.LogDebug("Entity {Id} was destroyed in repository", id);
                return;
            }

            if (entity.IsDeleted.GetValueOrDefault(false))
            {
                return;
            }

            entity.IsDeleted = true;
            this.repository.Replace(this.containerName, id, entity);
            this.logger.LogDebug("Entity {Id} was soft-deleted in repository", id);
        }

        public TDto ResurrectDeleted(string id)
        {
            var entity = this.repository.Retrieve(this.containerName, id,
                RepositoryEntityMetadata.FromType<TDto>());
            if (entity == null)
            {
                return default;
            }

            if (!entity.IsDeleted.GetValueOrDefault(false))
            {
                return entity.ToDto<TDto>();
            }

            entity.IsDeleted = false;
            this.repository.Replace(this.containerName, id, entity);

            this.logger.LogDebug("Entity {Id} was resurrected in repository", id);
            return entity.ToDto<TDto>();
        }

        public TDto Upsert(TDto dto, bool includeDeleted = false)
        {
            dto.GuardAgainstNull(nameof(dto));
            if (!dto.Id.HasValue() || dto.Id.IsEmpty())
            {
                throw new ResourceNotFoundException(Resources.GeneralCrudStorage_DtoMissingIdentifier);
            }

            var current = this.repository.Retrieve(this.containerName, dto.Id,
                RepositoryEntityMetadata.FromType<TDto>());
            if (current == null)
            {
                var added = this.repository.Add(this.containerName, CommandEntity.FromDto(dto));
                this.logger.LogDebug("Entity {Id} was added to repository", added.Id);

                return added.ToDto<TDto>();
            }

            if (current.IsDeleted.GetValueOrDefault(false))
            {
                if (!includeDeleted)
                {
                    throw new ResourceNotFoundException(Resources.GeneralCrudStorage_DtoDeleted);
                }
                current.IsDeleted = false;
            }

            var latest = MergeDto(dto, current);

            var updated = this.repository.Replace(this.containerName, dto.Id, latest);
            this.logger.LogDebug("Entity {Id} was updated in repository", dto.Id);

            return updated.ToDto<TDto>();
        }

        public TDto Get(string id, bool includeDeleted = false)
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
            return entity.ToDto<TDto>();
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
            return new QueryResults<TDto>(entities.ConvertAll(x => x.ToDto<TDto>()));
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

        private static CommandEntity MergeDto(TDto dto, CommandEntity current)
        {
            var currentAsDto = current.ToDto<TDto>();
            currentAsDto.PopulateWithNonDefaultValues(dto);
            return CommandEntity.FromDto(currentAsDto);
        }
    }
}