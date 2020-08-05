﻿using Domain.Interfaces.Entities;

namespace Storage.Azure
{
    public class AzureCosmosTableApiRepository : AzureTableStorageRepository
    {
        public AzureCosmosTableApiRepository(string connectionString, IIdentifierFactory idFactory)
            : base(connectionString, idFactory, TableStorageApiOptions.AzureCosmosDbStorage)
        {
        }
    }
}