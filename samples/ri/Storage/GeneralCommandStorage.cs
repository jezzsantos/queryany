using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny;
using QueryAny.Primitives;
using ServiceStack;
using Storage.Interfaces;

namespace Storage
{
    public class GeneralCommandStorage<TEntity> : ICommandStorage<TEntity>
        where TEntity : IPersistableEntity
    {
        private readonly string containerName;
        private readonly IDomainFactory domainFactory;
        private readonly ILogger logger;
        private readonly IRepository repository;

        public GeneralCommandStorage(ILogger logger, IDomainFactory domainFactory,
            IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            repository.GuardAgainstNull(nameof(repository));
            this.logger = logger;
            this.domainFactory = domainFactory;
            this.repository = repository;
            this.containerName = typeof(TEntity).GetEntityNameSafe();
        }

        public void Delete(Identifier id)
        {
            id.GuardAgainstNull(nameof(id));

            this.repository.Remove(this.containerName, id);
            this.logger.LogDebug("Entity {Id} was deleted from repository", id);
        }

        public TEntity Get(Identifier id)
        {
            id.GuardAgainstNull(nameof(id));

            var entity = this.repository.Retrieve(this.containerName, id,
                RepositoryEntityMetadata.FromType<TEntity>());

            this.logger.LogDebug("Entity {Id} was retrieved from repository", id);

            return entity != null
                ? entity.ToDomainEntity<TEntity>(this.domainFactory)
                : default;
        }

        public TEntity Upsert(TEntity entity)
        {
            entity.GuardAgainstNull(nameof(entity));
            if (!entity.Id.HasValue() || entity.Id.IsEmpty())
            {
                throw new ResourceNotFoundException("Entity has empty identifier");
            }

            var latest = Get(entity.Id);
            if (latest == null)
            {
                var added = this.repository.Add(this.containerName, CommandEntity.FromDomainEntity(entity));
                this.logger.LogDebug("Entity {Id} was added to repository", added.Id);

                return added.ToDomainEntity<TEntity>(this.domainFactory);
            }

            latest.PopulateWithNonDefaultValues(entity);

            var updated = this.repository.Replace(this.containerName, entity.Id,
                CommandEntity.FromDomainEntity(entity));
            this.logger.LogDebug("Entity {Id} was updated in repository", entity.Id);

            return updated.ToDomainEntity<TEntity>(this.domainFactory);
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