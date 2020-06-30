# QueryAny Reference Implementation

This folder contains a very close to real world example of a REST based API for managing Cars for a Car Sharing software product.

The example is split into several layers to mimic a real world implementation. 
It has tried not to be too opinionated about those layers, other than to assume that you would split your projects into logical layers for maintainability, testability, reuse and change.

The RI solution is structured as follows:

## CarsApi

This is the web host. In this case its ASP.NET Core running the [ServiceStack](http://www.servicestack.net) framework on Windows. 

> We chose ServiceStack for two main reasons. (1) it makes defining and configuring services so much easier than Microsoft's WebApi, (2) it includes an `auto-mapper` essential for creating abstractions between service operations, domain logic and storage layers.

It defines the HTTP REST endpoints (service operations) and their contracts. 

It handles the conversion between HTTP requests <-> Domain Logic. Including: formats, exception mapping, routes, validation etc.

It contains the `AppHost` class (specific to ServiceStack) which loads all service endpoints, and uses dependency injection for all runtime dependencies.

> A web host like this may contain the service operations of one, or more or all REST resources of any given API. The division of the API for deployment will need to remain portable so that whole APIs can be factored out into separate hosts for scalability.  

## CarsDomain

Essentially the domain logic layer.

It defines the domain logic classes, all domains specific rules, etc.

> In this case we have included the domain entities in this assembly for simplicity, but you may want to have a separate assembly for the entities of this domain to make your domain more portable.
 
> We anticipate that there will be one of these assemblies for every major domain in the product.

## Services.Interfaces

Contains shared definitions of the service operations, intended to be shared across all services.

Also intended to be shared to clients.

> We anticipate that there will be one of these assemblies for all domains in the product.

## Storage.Interfaces

Contains the shared definitions of the consumers of the storage layer (i.e. the domain logic).

Intended to define the interface for implementers of specific storage databases, and repositories.

## Storage

Concrete implementations of `IStorage<TEntity>` for the various storage technologies.

This is where all QueryAny implementations will exist for this sample.

> We anticipate that there would probably be a separate project (and nuget package) for each implementation of the `IStorage<TEntity>` interface in your architecture. eg. one for SqlServer, one for Redis, one for CosmosDB, etc.. 

## Local Development and Testing

You will need to: 

1. Install the `Azure Storage Emulator` locally to run the tests. Available for [download here](https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409).
1. Install the `Azure Cosmos DB Emulator` locally to run the tests. Available for [download here](https://aka.ms/cosmosdb-emulator)

> Configuration for local storage servers is in the project `Storage.UnitTests`, in the file `appsettings.json`. Change these values to match your local installations, if you have changed the defaults.

> Note: When running the tests on `Storage.UnitTests` you will need to give your IDE elevated privileges to run the Azure Cosmos DB Emulator during a test run.