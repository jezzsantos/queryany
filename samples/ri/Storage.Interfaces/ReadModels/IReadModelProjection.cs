using System;

namespace Storage.Interfaces.ReadModels
{
    public interface IReadModelProjection
    {
        Type EntityType { get; }

        bool Project(object originalEvent);
    }
}