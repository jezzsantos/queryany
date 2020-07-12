﻿using QueryAny.Primitives;

namespace Storage.Azure
{
    public class AzureStorageConnection : IAzureStorageConnection
    {
        private readonly IRepository repository;

        public AzureStorageConnection(IRepository repository)
        {
            Guard.AgainstNull(() => repository, repository);
            this.repository = repository;
        }

        public IRepository Open()
        {
            return this.repository;
        }
    }
}