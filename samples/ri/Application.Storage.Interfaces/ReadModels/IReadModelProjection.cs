using System;
using Domain.Interfaces.Entities;

namespace Application.Storage.Interfaces.ReadModels
{
    public interface IReadModelProjection
    {
        Type EntityType { get; }

        bool Project(IChangeEvent originalEvent);
    }
}