using System;

namespace Application.Storage.Interfaces.ReadModels
{
    public class ReadModelEntity : IReadModelEntity
    {
        public string Id { get; set; }

        public DateTime? LastPersistedAtUtc { get; set; }

        public bool? IsDeleted { get; set; }
    }
}