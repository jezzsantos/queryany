using System;
using Common;
using QueryAny;
using Storage.Interfaces.ReadModels;

namespace Storage
{
    public class GeneralReadModelStorage<TDto> : IReadModelStorage<TDto> where TDto : IReadModelEntity, new()
    {
        private readonly IRecorder recorder;
        private readonly IRepository repository;

        public GeneralReadModelStorage(IRecorder recorder, IRepository repository)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            repository.GuardAgainstNull(nameof(repository));

            this.recorder = recorder;
            this.repository = repository;
        }

        public string ContainerName => typeof(TDto).GetEntityNameSafe();

        public TDto Create(string id, Action<TDto> action = null)
        {
            id.GuardAgainstNullOrEmpty(nameof(id));

            var dto = new TDto {Id = id};
            action?.Invoke(dto);

            var entity = this.repository.Add(ContainerName, CommandEntity.FromType(dto));

            this.recorder.TraceDebug("Created new read model for entity {Id}", id);

            return entity.ToReadModelEntity<TDto>();
        }

        public TDto Update(string id, Action<TDto> action)
        {
            id.GuardAgainstNullOrEmpty(nameof(id));
            action.GuardAgainstNull(nameof(action));

            var entity = this.repository.Retrieve(ContainerName, id,
                RepositoryEntityMetadata.FromType<TDto>());
            if (entity == null)
            {
                throw new ResourceNotFoundException();
            }

            var dto = entity.ToReadModelEntity<TDto>();
            action(dto);
            var updated = this.repository.Replace(ContainerName, id, CommandEntity.FromType(dto));

            this.recorder.TraceDebug("Updated read model for entity {Id}", id);

            return updated.ToReadModelEntity<TDto>();
        }
    }
}