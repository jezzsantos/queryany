﻿using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;

namespace Storage.IntegrationTests.Azure
{
    public class TestEntityAzureCommandStorage<TEntity> : GenericCommandStorage<TEntity>
        where TEntity : IPersistableEntity
    {
        public TestEntityAzureCommandStorage(ILogger logger, IDomainFactory domainFactory,
            IRepository repository, string containerName) : base(
            logger, domainFactory, repository)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            ContainerName = containerName;
        }

        protected override string ContainerName { get; }
    }

    public class TestEntityAzureQueryStorage<TEntity> : GenericQueryStorage<TEntity> where TEntity : IPersistableEntity
    {
        public TestEntityAzureQueryStorage(ILogger logger, IDomainFactory domainFactory,
            IRepository repository, string containerName) : base(
            logger, domainFactory, repository)
        {
            containerName.GuardAgainstNullOrEmpty(nameof(containerName));
            ContainerName = containerName;
        }

        protected override string ContainerName { get; }
    }
}