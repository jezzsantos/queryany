# QueryAny Reference Implementation

This folder contains a very close to real world example of a REST based API for managing Cars for a Car Sharing software product, that follows many of the principles of Domain Driven Design (DDD) and Clean Architecture/Onion Architecture/Hexagonal Architecture. This is not an implementation of strict DDD.

The example is split into several layers to mimic the real world implementation it was extracted from and based off of.

It has tried not to be too opinionated about the layout of files/folders/assemblies on disk, other than to assume that you would likely split your projects/components into logical, testable layers for maintainability, reuse and future scalability (scale out), as your product grows.

The RI solution demonstrates strict discipline around decoupling and separation of concerns, both of which manage accidental complexity as things change, and as the codebase scales.

The RI does does implement patterns that allow clients to can have fine grained control the data returned on the wire.

> Design Choice: There are some *interesting* and pragmatic implementation patterns demonstrated within this RI. Remember that this RI has laid out just *one* way of doing things. There are many ways of doing the same kind of things, with various design tradeoffs, especially in the area of entity persistence, and in the realm of building products has prioritized maintainability over optimal performance. If you are looking for the *best* way to do things, then you haven't programmed long enough to learn that no such thing exists - every context of every product is unique. Our advice to you is start with something small, and adapt it as you learn more, avoid over-engineering it at all costs. If in doubt, favor what you definitely know you have to work with, rather than attempting to future proof it. KISS, DRY and YAGNI my friends.

# Architecture

The RI solution is structured as projects on disk, as follows:

## CarsApi

This is the web host. In this case its ASP.NET Core running the [ServiceStack](http://www.servicestack.net) framework on Windows. It could be whatever web host you like.

> Design Choice: We chose ServiceStack as the foundational web service framework for a few main reasons. (1) it makes defining and configuring services so much easier than Microsoft's WebApi (specifically in areas of extensibility), (2) it includes an `auto-mapper` essential for easily maintaining abstractions between service operations, domain entities and infrastructure layers, and (3) it has excellent reflection support for persisting .Net types that would otherwise be very difficult to persist, causing far more difficulty when it comes to pragmatic persistence.

It defines the HTTP REST endpoints (service operations) and their contracts. 

It handles the conversion between HTTP requests <-> Domain Logic. Including: wire-formats, exception mapping, routes, request validation etc.

It contains the `ServiceHost` class (specific to ServiceStack) which loads all service endpoints, and uses dependency injection for all runtime dependencies.

> A host like this one may contain the service operations of one, or more REST resources of any given API. The division of the API into deployment packages will need to remain flexible so that whole APIs can be factored out into separate hosts when the product needs to scale and be optimized.

## CarsDomain

Essentially the core domain logic layer.

> Note: In a real architecture, would never actually name this layer, or the types within it, using the suffix "Domain". It is just there as a guide post for you.

It defines the domain logic classes, entities all domains specific rules, etc.

It contains an 'application layer' (in DDD parlance) or 'Interactor' (in Clean Architecture parlance) used to coordinate the various domain functions on the various domain entities. This layer orchestrates the domain entities, and manages persistence of them, and all interactions with infrastructure layers (ie. The Web, Persistence, External Services).

> Design Choice: In this case we have included the domain entities in this assembly for simplicity, but you may want to have a separate assembly for the entities of this domain to make your domain more portable.

> The domain entities in this implementation are relatively simple in terms of functionality and rules (close to anemic - due to limited scope of the sample).

> Design Choice: Domain entities are persistent "aware" for practicality. There are many ways to handle/decouple persistence from your entities, this is *one* pattern, you may desire another. You definitely don't want you entities to do their own persistence (like ActiveRecord does), but having the knowledge of what internal data they need to be serialized (dehydrated) and de-serialized (hydrated) is something in-practice they *could* legitimately know. YMMV, this pattern is absolutely scalable and simple to maintain.

> We anticipate that there will be one of these assemblies for every major domain in the product.

## Services.Interfaces

Contains shared definitions for service operations, intended to be shared across all services.

Also intended to be shared to service client libraries (if any).

> We anticipate that there will be one of these assemblies for all domains in the product.

## Storage.Interfaces

Contains the shared definitions of the consumers of the storage layer (i.e. the domain logic).

Intended to define the interface for implementers of specific storage databases, and repositories.

## Storage and Storage.???

Concrete implementations of `IStorage<TEntity>` for various storage technologies.

This is where all QueryAny storage implementations will exist for this sample.

> We anticipate that there would probably be a separate project (and nuget package) for each implementation of the `IStorage<TEntity>` interface in your architecture. eg. one for SqlServer, one for Redis, one for CosmosDB, etc.. 

## CarsApi.IntegrationTests

Contains all integration tests for testing the API.

> Here we demonstrate testing API's by pre-populating data directly in the repository, as only an example of how to plug in different repository for testing. Normally, however, we would populate the state of the API through the API itself, and erase all data through the repository layer. This removes the dependency to know how application data is actually persisted in any repository, since, in fact data could be spread across multiple repositories and technologies.

## Storage.IntegrationTests

Contains integration tests for verifying various repository implementations. These tests have been templatized so that new implementations have a test suite to verify them.

## ???.UnitTests projects

Contains all unit level tests for all components in the architecture, separated by component.

> We anticipate that there would probably be a separate test project per assembly to test.

# Local Development and Testing

1. First: [Getting Started](https://github.com/jezzsantos/queryany/wiki/Getting-Started) for details on what you need installed on your machine.
2. Start the `CarsApi` project locally. (A browser should open to API documentation site).
3. You will need to start the Azure CosmosDB emulator on your machine (from Start Menu), and ensure that you have created a new cosmos database called: "Production".

### Test the API

To test everything is working:

1. Navigate to: [GET https://localhost:5001/cars/available](https://localhost:5001/cars/available) (you should get an empty array of cars in response)
2. Create a new car by calling (using a tool like PostMan): `POST /cars` with a JSON body like this: `{"Year": 2020,"Make": "Honda","Model": "Civic"}` 
