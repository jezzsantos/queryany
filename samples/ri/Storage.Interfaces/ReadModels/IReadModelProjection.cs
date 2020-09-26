using System;
using Domain.Interfaces.Entities;

namespace Storage.Interfaces.ReadModels
{
    public interface IReadModelProjection
    {
        Type EntityType { get; }

        bool Project(IChangeEvent originalEvent);
    }
}