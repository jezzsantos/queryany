# QueryAny Reference Implementation

This folder contains a very close to real world example of a REST based API for managing Cars for a Car Sharing software product.

The example is split into several layers to mimic a real world implementation. 

It has tried not to be too opinionated about those layers, other than to assume that you would split your projects into logical layers for maintainability, testability, reuse and future scalability as your product grows.

The RI solution demonstrates strict discipline around decoupling and separation of concerns, both of which manage change well as a product scales.

 

The RI solution is structured as follows:

## CarsApi

This is the web host. In this case its ASP.NET Core running the [ServiceStack](http://www.servicestack.net) framework on Windows. 

> We chose ServiceStack for two main reasons. (1) it makes defining and configuring services so much easier than Microsoft's WebApi, (2) it includes an `auto-mapper` essential for easily maintaining abstractions between service operations, domain logic and storage layers.

It defines the HTTP REST endpoints (service operations) and their contracts. 

It handles the conversion between HTTP requests <-> Domain Logic. Including: formats, exception mapping, routes, request validation etc.

It contains the `ServiceHost` class (specific to ServiceStack) which loads all service endpoints, and uses dependency injection for all runtime dependencies.

> A host like this one may contain the service operations of one, or more REST resources of any given API. The division of the API into deployment packages will need to remain flexible so that whole APIs can be factored out into separate hosts when the product needs to scale and be optimized.

## CarsDomain

Essentially the core domain logic layer.

> In a real architecture, would never actually name this layer or types using the suffix "Domain"

It defines the domain logic classes, all domains specific rules, etc.

It contains a thin 'application layer' (in DDD parlance) used to coordinate the various domain functions on the domain entities.  

> In this case we have included the domain entities in this assembly for simplicity, but you may want to have a separate assembly for the entities of this domain to make your domain more portable.

> The domain entities in this implementation are relatively simple in terms of functionality and rules (almost anemic - due to limited scope of the sample). 

> They are also persistent aware for simplicity. There are many ways to handle/decouple persistence from your entities, this is one pattern, you may desire another. You definitely don't want you entities to do their own persistence (like ActiveRecord), but having the knowledge of what data they need to be serialized and deserialized is something in practice they could know.

> We anticipate that there will be one of these assemblies for every major domain in the product.

## Services.Interfaces

Contains shared definitions for service operations, intended to be shared across all services.

Also intended to be shared to service client libraries (if any).

> We anticipate that there will be one of these assemblies for all domains in the product.

## Storage.Interfaces

Contains the shared definitions of the consumers of the storage layer (i.e. the domain logic).

Intended to define the interface for implementers of specific storage databases, and repositories.

## Storage

Concrete implementations of `IStorage<TEntity>` for various storage technologies.

This is where all QueryAny storage implementations will exist for this sample.

> We anticipate that there would probably be a separate project (and nuget package) for each implementation of the `IStorage<TEntity>` interface in your architecture. eg. one for SqlServer, one for Redis, one for CosmosDB, etc.. 

## CarsApi.IntegrationTests

Contains all integration tests for testing the API.

> Here we demonstrate testing API's by pre-populating data directly in the repository, as only an example of how to plug in different repository for testing. Normally, however, we would populate the state of the API through the API itself, and erase all data through the repository layer. This removes the dependency to know how application data is actually persisted in any repository, since, in fact data could be spread across multiple repositories and technologies.

## Storage.IntegrationTests

Contains integration tests for verifying various repository implementations 

## ???.UnitTests projects

Contains all unit level tests for all components in the architecture, separated by component.

> We anticipate that there would probably be a separate test project per assembly to test.

## Local Development and Testing

See: [Getting Started](https://github.com/jezzsantos/queryany/wiki/Getting-Started) for details on how to build, and execute tests in this sample.