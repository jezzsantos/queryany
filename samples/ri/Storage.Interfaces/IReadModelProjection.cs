using System;

namespace Storage.Interfaces
{
    public interface IReadModelProjection
    {
        Type EntityType { get; }

        bool Project(object originalEvent);
    }
}