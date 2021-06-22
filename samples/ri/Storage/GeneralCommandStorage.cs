using Common;
using Domain.Interfaces.Entities;
using QueryAny;
using ServiceStack;
using Storage.Interfaces;
using Storage.Properties;

namespace Storage
{
    public class GeneralCommandStorage<TEntity> : ICommandStorage<TEntity>
        where TEntity : IPersistableEntity
    {
        private readonly string containerName;
        private readonly IDomainFactory domainFactory;
        private readonly IRecorder recorder;
        private readonly IRepository repository;

        public GeneralCommandStorage(IRecorder recorder, IDomainFactory domainFactory,
            IRepository repository)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            repository.GuardAgainstNull(nameof(repository));
            this.recorder = recorder;
            this.domainFactory = domainFactory;
            this.repository = repository;
            this.containerName = typeof(TEntity).GetEntityNameSafe();
        }

        public void Delete(Identifier id, bool destroy = true)
        {
            id.GuardAgainstNull(nameof(id));

            var entity = this.repository.Retrieve(this.containerName, id,
                RepositoryEntityMetadata.FromType<TEntity>());
            if (entity == null)
            {
                return;
            }

            if (destroy)
            {
                this.repository.Remove(this.containerName, id);
                this.recorder.TraceDebug("Entity {Id} was destroyed in repository", id);
                return;
            }

            if (entity.IsDeleted.GetValueOrDefault(false))
            {
                return;
            }

            entity.IsDeleted = true;
            this.repository.Replace(this.containerName, id, entity);
            this.recorder.TraceDebug("Entity {Id} was soft-deleted in repository", id);
        }

        public TEntity ResurrectDeleted(Identifier id)
        {
            var entity = this.repository.Retrieve(this.containerName, id,
                RepositoryEntityMetadata.FromType<TEntity>());
            if (entity == null)
            {
                return default;
            }

            if (!entity.IsDeleted.GetValueOrDefault(false))
            {
                return entity.ToDomainEntity<TEntity>(this.domainFactory);
            }

            entity.IsDeleted = false;
            this.repository.Replace(this.containerName, id, entity);

            this.recorder.TraceDebug("Entity {Id} was resurrected in repository", id);
            return entity.ToDomainEntity<TEntity>(this.domainFactory);
        }

        public TEntity Get(Identifier id, bool includeDeleted = false)
        {
            id.GuardAgainstNull(nameof(id));

            var entity = this.repository.Retrieve(this.containerName, id,
                RepositoryEntityMetadata.FromType<TEntity>());

            if (entity == null)
            {
                return default;
            }

            if (entity.IsDeleted.GetValueOrDefault(false) && !includeDeleted)
            {
                return default;
            }

            this.recorder.TraceDebug("Entity {Id} was retrieved from repository", id);
            return entity.ToDomainEntity<TEntity>(this.domainFactory);
        }

        public TEntity Upsert(TEntity entity, bool includeDeleted = false)
        {
            entity.GuardAgainstNull(nameof(entity));
            if (!entity.Id.HasValue() || entity.Id.IsEmpty())
            {
                throw new ResourceNotFoundException(Resources.GeneralCommandStorage_EntityMissingIdentifier);
            }

            var current = this.repository.Retrieve(this.containerName, entity.Id,
                RepositoryEntityMetadata.FromType<TEntity>());
            if (current == null)
            {
                var added = this.repository.Add(this.containerName, CommandEntity.FromDomainEntity(entity));
                this.recorder.TraceDebug("Entity {Id} was added to repository", added.Id);

                return added.ToDomainEntity<TEntity>(this.domainFactory);
            }

            if (current.IsDeleted.GetValueOrDefault(false))
            {
                if (!includeDeleted)
                {
                    throw new ResourceNotFoundException(Resources.GeneralCommandStorage_EntityDeleted);
                }
            }

            var latest = MergeEntity(entity, current);

            if (current.IsDeleted.GetValueOrDefault(false))
            {
                latest.IsDeleted = false;
            }

            var updated = this.repository.Replace(this.containerName, entity.Id, latest);
            this.recorder.TraceDebug("Entity {Id} was updated in repository", entity.Id);

            return updated.ToDomainEntity<TEntity>(this.domainFactory);
        }

        public long Count()
        {
            return this.repository.Count(this.containerName);
        }

        public void DestroyAll()
        {
            this.repository.DestroyAll(this.containerName);
            this.recorder.TraceDebug("All entities were deleted from repository");
        }

        private CommandEntity MergeEntity(TEntity updated, CommandEntity persisted)
        {
            var persistedAsEntity = persisted.ToDomainEntity<TEntity>(this.domainFactory);
            persistedAsEntity.PopulateWith(updated);
            return CommandEntity.FromDomainEntity(persistedAsEntity);
        }
    }
}