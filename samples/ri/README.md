# QueryAny Reference Implementation

This folder contains a very close to real world example of a REST based API for managing Cars for a Car Sharing software product, that follows many of the principles of Domain Driven Design (DDD) and Clean Architecture/Onion Architecture/Hexagonal Architecture. This is not an implementation of strict DDD.

We describe the major boundaries of this RI as either: Infrastructure, Application or Domain.

![Architecture](https://raw.githubusercontent.com/wiki/jezzsantos/queryany/Images/Architecture.png)

We use these kinds of terms in the architecture: 

* API (REST API), 
* Service Operations, 
* DTO (Data Transfer Objects), 
* REST Resources, 
* Application Layer (DDD Application Layer), 
* Entities (DDD Entities), ValueObject (DDD ValueObject), Aggregates
* Repositories

In terms of data flow, a typical REST API call results in an interaction like this:

* Messages come in over the wire on HTTP into Service Operations of the API. The API de-serializes the HTTP request into DTO's that define a REST command.
* The API Service Operations, grouped by REST resource type, then validate the inbound DTO command, and delegate execution to a Application  Layer. (In this scheme, DTO's are deconstructed to component properties - to save on mapping to another layer of DTO's). 
* The Application Layer takes the deconstructed DTO properties, and instantiates and/or dehydrates entities from repositories, co-ordinates and instructs entities to do things (Tell-Dont-Ask), and then if necessary dehydrates the change in Entity state back to persistence.
* The Application Layer then converts the entities to DTOs, and hands them back to the API.
* The API layer then serializes the DTO over the wire, and handles the conversion of exceptions to HTTP status codes and descriptions.

> Important: This reference implementation is not part of the QueryAny library/package, just an example of using QueryAny used in practice.

The RI has tried not to be too strongly opinionated about the layout and naming of files/folders/assemblies on disk, other than to assume that in a small to medium sized product you too would likely split your projects/components into logical, testable layers for maintainability, reuse and future scalability (scale out), as your product grows.

> Note: Some of the naming conventions here are debatable and perhaps not to your liking. That's cool too. We have named them in the way we have, primarily so that you can at a glance understand the intention of them. We would chose a different naming convention in a real product too.

The RI solution demonstrates strict discipline around decoupling and separation of concerns, both of which manage _accidental complexity_ as things change and as the codebase grows. Which is the primary concern of yours.

> Design Choice: The RI does does not quite fully implement all GET API patterns that allow clients to have fine grained enough control the data returned on the wire. So, the inclusion of a GraphQL endpoint is a reasonable thing to need to add to it.

> Design Choice: There are some pragmatic implementation patterns demonstrated within this RI. Remember that this RI has laid out just *one way* of doing things. There are *many ways* of doing the same kind of things, with various design trade offs, especially in the area of entity mapping and entity persistence. In the realm of building products, this RI demonstrates prioritization of maintainability over optimal performance. If you are looking for the *best* way to do things, then you haven't programmed long enough to learn that no such thing exists. Our advice to you is start with something small, and adapt it as you learn more, avoid over-engineering it at all costs. If in doubt, favor what you definitely know you have to work with now, rather than attempting to future proof it. KISS, DRY and YAGNI my friends.

> Note: If you don't like what you see here. That's cool, just ignore it. But don't make the mistake of coupling your persistence or your Web API to your domain entities. That's a fundamental rookie mistake in software engineering, that you are going to make without being very intentional about designing your way out of.

# Structure

The RI solution is structured into three logical parts:

* Infrastructure - This contains all infrastructure and adapters to that infrastructure (eg. Web API adapters and Storage adapters to repositories)
* Application - this contains you definition of your application. The application layer instantiates, coordinates the domain layer to do stuff. Thing commands and queries of CQRS. In the case of building stateless services, this layer would also be responsible for dehydrating and rehydrating domain entities to and from persistence.
* Domain - core domain classes, and application services unfettered by infrastructure and the application. 

There should be no dependency from: classes in Domain -> to classes in Application or from Application to Infrastructure. Ever!

## Domain

The domain layer contains  **Domain Entities and Value Objects and Aggregates:** Your smart, fully encapsulated, pure OO, 'domain classes' that have all your domain rules, logic, validation, etc encapsulated within them. 

They aggregate other entities and are self-contained. 

What they lack is only when to do the things they do in response to the world around them. That comes from the Application Layer.

### CarsDomain

Essentially the core domain logic layer (domain entities and application layers).

> Note: In a real architecture, you would never actually name this layer (or the types within it) using the suffix "Domain". It is just here as a guide post for you, so that you can tell what the layer is.

It defines the domain entities with all domains specific rules, etc.

It contains an 'application layer' (in DDD parlance) or 'Interactor' (in Clean Architecture parlance) used to coordinate the various domain functions on the various domain entities. This layer orchestrates the domain entities, and manages persistence of them, and all interactions with infrastructure layers (ie. The Web, Persistence, External Services).

> Design Choice: In this case we have included the domain entities in this assembly for simplicity, but you may want to have a separate assembly for the entities of this domain to make your domain more portable.

> Design Choice: The domain entities in this implementation are relatively simple in terms of functionality and rules (close to anemic - due to limited scope of the sample). They also do not display much in the way of aggregation which is more common than not. Usually primary entities like a 'Car' would be an aggregate entity (DDD parlance), and all operations that the aggregated entities perform would be accessible through this _aggregate root_.

> Design Choice: Domain entities in this RI have been specifically designed to be persistent "aware" for practicality. We use the terms Hydrate/Dehydrate to associate them to persistence support, and we define `Dictionary<string, object>` to remove the need to specify explicit DTO classes for mapping. Which side-steps an additional mapping layer.

> Design Choice: There are many ways to handle/decouple persistence between your entities and repositories, this is *one* pattern, you may desire another. You definitely don't want your entities to do their own persistence (like the ActiveRecord pattern does). Do could define a mapping layer to DTO's between entities and repositories (as ORM's do), but having the knowledge of what internal data an entity needs to be serialized (de-hydrated) and de-serialized (re-hydrated) is knowledge in-practice that an entity *could* legitimately have. YMMV, this pattern is absolutely flexible, scalable and simple to maintain.

> We anticipate that there will be one of these assemblies for every major domain in the product.

### Domain.Interfaces

Contains shared definitions for use by domain classes, intended to be shared across all domains.

Also intended to be shared to service client libraries (if any).

> We anticipate that there will be one of these assemblies for all domains in the product.

### ???.UnitTests projects

Contains all unit level tests for all components in the architecture, separated by component.

> We anticipate that there would probably be a separate test project per assembly to test.

## Application

This layer contains the **Transaction Scripts/Application Layer/Domain Services:** classes that co-ordinate/orchestrate/manage/script your domain entities. 

These classes contain commands and queries that essentially follow the same patter: (1) retrieve domain entities from persistence, (2) instruct the entities what they should do now (Tell-Dont-Ask), and then (3) persist the mutated entity state again. This layer represents your specific application. 

> Note: Between your 'transaction scripts/application layer/domain services' and the Infrastructure layer there will always be a mapping (logical or physical) to and from DTO (Data Transfer Objects) or POCO objects. These are bare OO objects with no behaviour in them, that do not use encapsulation (ideally not inheritance), that are the only types that traverse the boundary between Domain<->Infrastructure.

### CarsApplication

Contains the application layer consisting of services that instruct the domain entities to do things, using the Ask-Dont-Tell pattern.

> We anticipate that there will be one of these assemblies for each service you have in your domain.

### Api.Interfaces

Contains shared definitions for use by consuming APIs, intended to be shared across all APIs.

> We anticipate that there will be one of these assemblies for all APIs in the product.

### Storage.Interfaces

Contains shared definitions for access to persistence/storage (i.e. the domain application layer).

Intended to define the interface for implementers of specific storage databases, and repositories.

### ???.UnitTests projects

Contains all unit level tests for all components in the architecture, separated by component.

> We anticipate that there would probably be a separate test project per assembly to test.


## Infrastructure

Contains all ports & Adapters, all infrastructure classes and anything to do with interacting with the outside world (from the domain's perspective).

### CarsApi

This is the web host. In this case its ASP.NET Core running the [ServiceStack](http://www.servicestack.net) framework on Windows. It could be whatever web host you like.

> Design Choice: We chose ServiceStack as the foundational web service framework for a few main reasons. (1) it makes defining and configuring services so much easier than Microsoft's WebApi (specifically in areas of extensibility), (2) it includes an `auto-mapper` essential for easily maintaining abstractions between service operations, domain entities and infrastructure layers, and (3) it has excellent reflection support for persisting .Net types that would otherwise be very difficult to persist, causing far more difficulty when it comes to pragmatic persistence.

It defines the HTTP REST endpoints (service operations) and their contracts. 

It handles the conversion between HTTP requests <-> Domain Logic. Including: wire-formats, exception mapping, routes, request validation etc.

It contains the `ServiceHost` class (specific to ServiceStack) which loads all service endpoints, and uses dependency injection for all runtime dependencies.

> A host like this one may contain the service operations of one, or more REST resources of any given API. The division of the API into deployment packages will need to remain flexible so that whole APIs can be factored out into separate hosts when the product needs to scale and be optimized.

### CarsStorage

This is a library of entity specific storage implementation classes used in both production code and during integration testing.

> Typically, an implementation will have an in-memory class used in integration testing (to increase test speed), and one for (say a database) for use in a production environment - often injected in the ServiceHost of the CarsApi project.  

### Storage and Storage.???

Concrete implementations of `IStorage<TEntity>` for various storage technologies.

This is where all QueryAny storage implementations will exist for this sample.

> We anticipate that there would probably be a separate project (and nuget package) for each implementation of the `IStorage<TEntity>` interface in your architecture. eg. one for SqlServer, one for Redis, one for CosmosDB, etc.. 

### CarsApi.IntegrationTests

Contains all integration tests for testing the API.

> Design Choice: Here we demonstrate testing API's by pre-populating data through the API only. (In practice (not demonstrated here) you may need to have additional API's to load/delete resources for some domains, that would definitely not be shipped in your production distribution) Normally, we would populate the state of the domain through the API itself, and erase all data for each test through the repository we are using in testing. This removes the dependency to know how application data is actually persisted in any repository, since, given the fact that QueryAny is abstracting that knowledge from you in the first place! (since data could be spread across multiple repositories and accessed via various technologies).

### Storage.IntegrationTests

Contains integration tests for verifying various repository implementations, against their real repository technology. 

These tests have been templatized so that new implementations have a test suite to verify them.

> Note: These tests can be slow depending on the repository implementation. 

>Design Choice: We deliberately chose to use local installations of these repositories rather than cloud based instances. So at this point, you should be able to run all these tests offline.

### ???.UnitTests projects

Contains all unit level tests for all components in the architecture, separated by component.

> We anticipate that there would probably be a separate test project per assembly to test.

# Design Notes

In this RI we have a number of intentional designs for practical purposes, that reduce the overhead on developers for keeping strictly to pure OO or DDD design principles. These design decisions aim to make the code easy to work with while at the same time maintain high levels of maintainability and decoupling. 

## Design Principles

This section calls out some of the design principles used behind this reference implementation.

Many of these principles are informed by Domain Driven Design. Many are informed by writing clean testable code that changes frequently.

## Application Layers

The application you are choosing to build right now is a different thing than the domain you are choosing to model in software. This subtle distinction makes a large difference to how you design the over architecture. Whilst it is true that you are building both the domain and the application at the same time, it is not true that they are the same thing. For example: You may decide that there are a certain set of users that are going to use this application in a certain way, say a mobile application with limited functionality. Then you are are building a WebApp for another set of users to use a certain way, finally, you build a centralized API that serves both mobile app and web app. In fact you have built 3 applications, but at the heart of it is one logical domain, ideally in one central place to avoid duplication.

For this reason, it is useful to maintain a separation between you domain and the application which is hosting it.
We call this an "Application Layer", and it has responsibilities outside the domain for things like persisting stateless interactions, handing commands (like REST API commands), and responding to events in the domain by passing messages through adapters to the outside world.

The Application Layer sits between the Infrastructure layer and the Domain layer. It brokers all interactions to the domain layer through DTO objects. Never exposing domain entities or their data or their inputs, outside the domain. The domain should never have any dependency on any Application or any Infrastructural component - ever!

## Value Objects

Value objects represent most of the things in a domain.

They have the property that two instances with the same internal value are in fact equal. 
(As opposed to Entities, which despite their value, are equal if their `Id` is equal).

Like Entities, they contain data, and behavior.

Like Entities, they must not exist in the domain in an invalid state.

Unlike Entities, they do not have a unique identifier.

Unlike Entities, they are _immutable_. That means, once they have been created, they cannot be changed. New instances of them must be created instead, especially if commands change their state.

**In practice:**

 * Value Objects are often instantiated with their value in their constructor, where those values will be validated.
 * Value Objects will never allow their "value" to be changed, so must not have any public properties or methods to change their internal state. 
 * Value Objects will derive from `ValueObject<T>` and they will implement the inherited methods that provide equality and persistence support.
 * Commands that change the state of a ValueObject must return a new instance of the ValueObject, since they are immutable.
 * You should not expect a caller to know how to change the state of an ValueObject. You should provide a method that validates the value, and any constraints, that would mutate it. So, no public setters.
* Value Objects support persistence of their internal state through the `IPersistableValueObject interface, which is used by persistence layers. A ValueObject never persists itself. Only Application Layers do that.
* Value Objects derive from `ValueObjectBase<T>`, and must have a public, parameter-less constructor used for instantiation by persistence layers

## Entities

>  Note: Entities are not that common in a domain, most domain concepts are represented as Value Objects.

They have the property that two instances of the _same type_ of entity are equal if their `Id` is equal. (Irrespective of whether their internal values are equal or not).

Like ValueObjects, they contain data and behavior.

Like ValueObjects, they must not exist in the domain in an invalid state.

Unlike ValueObjects, they have a unique identifier (unique for that type across the whole domain).

Unlike ValueObjects, they are _mutable_. That means, once they have been created, they can be changed.

In practice:
 * Entities are often instantiated with any values in their constructor, where those values will be validated.
 * Entities will never allow their "value" to be changed, so must not have any public properties or methods to change their internal state. 
 * Entities will derive from `EntityBase<T>` and they will implement the inherited methods that provide identification and persistence support.
 * Commands that change the state of a Entities do not return an instance of the Entity, since they are mutable.
 * You should not expect a caller to know how to change the state of an Entity. You should provide a method that validates the value, and any constraints, that mutates it. So, no public setters.
 * Entities support persistence of their internal state through the `IPersisableEntity` interface, which is used by persistence layers. An Entity never persists itself. Only Application Layers do that.
 * Entity identifiers are created by a `IIdentifierFactory` which generates the identity for the entity. This is expected to be generated when the entity is first created. 
 * Entities derive from `EntityBase`, and must declare a `EntityFactory<TEntity>` using a `HydrationIdentifierFactory` used for instantiation by persistence layers.

## Aggregates

These are entities which have a hierarchy of other Entities and Value Objects. 

The top most parent is called the *aggregate root*.

Any action that you need to perform on child entity (or value type) must be performed on the aggregate root. Which means that you cannot access or instruct a child entity/value to do anything directly.

In practice:

* Aggregates are entities so are implemented in the same way.
* Aggregates should not expose child entities as getter properties on the aggregate entity, as those child entities then become callable to do things without having to go through the aggregate root.
*  Expose new methods on the aggregate root to execute methods on child entities/value objects.
* Use domain services to do things that child entities and value objects don't/can't do

## Domain Services

Stateless services that perform services to entities that would not naturally fit inside of an entity.

## Persistence

### Performance Compromise

Due to the fact that we are using a generic abstraction `IStorage<TEntity>` to remove early complexity (by reducing coupling) from the codebase, 
and because this strategy entails compromising repository technology specific optimizations; 
we accept the fact that no repository technology will be used optimally in this reference implementation. They will be used generically.

No abstraction can possibly be optimal for every single repository technology, since all repository technologies aim to satisfy different performance design goals. However, most of these optimizations are not important to you in the early stages of product development.

We accept that fact, and we accept that these lost optimizations are likely not to be important for 80% of the use cases for repositories for persistence for a software product in its early stages.

We also accept that at the time in the software products life when these optimizations may become important, you can simply optimize that usage by engineering in a more optimal implementation for that part of the product. This has the effect of increasing complexity (making that more optimal part of the codebase harder to change in the future).

For those reasons, the following sub-optimizations on specific repository implementations is acceptable:

* A `1000` result limit on all queries, in: `InProcessInMemRepository`, `RedisInMemRepository`, `AzureTableStorageRepository`, `AzureCosmosSqlApiRepository`, `SqlServerRepository`.
* Loading all entities (into memory) from each container, in: `InProcessInMemRepository`, `RedisInMemRepository`.
* In memory (Where clause) filtering, in: `InProcessInMemRepository`, `RedisInMemRepository`.
* Multiple fetches of individual joined entities, in: `AzureTableStorageRepository`, `AzureCosmosSqlApiRepository`.
* In memory joining of joined entities, in: `InProcessInMemRepository`, `RedisInMemRepository`, `AzureTableStorageRepository`, `AzureCosmosSqlApiRepository`.
* In memory ordering, limiting, offsetting, in: `InProcessInMemRepository`, `RedisInMemRepository`, `AzureTableStorageRepository`.

### Mapping Shortcuts

Ideally, there would be a mapping between domain entities and DTO's whenever entities traverse over any boundary. Such as from Infrastructure to Application, and from Application to Domain, and visa-versa. 

**For example 1**: When HTTP requests in a REST API invoke domain entities to perform work, the data passed into entities would be in the form of DTO's coming over the wire as JSON. That requires mapping DTO -> Entity and visa versa.

**For example 2**: When domain entities are persisted to storage, the entity would be mapped to a DTO and then passed to a physical repository for persisting to that store. That requires mapping Entity -> DTO and visa versa.

* All these mappings (entity <-> DTO and visa-versa)  require knowledge of those mappings codified somewhere in the code. 
* Typically, in the case of inbound invocation (eg. HTTP requests/responses), this mapping is codified outside the entity in a service operation type.
* Typically, in the case of an outbound invocation (eg. Persistence), this mapping is codified inside the entity itself or in the Application Layer.
* The mapping code may be complex (depending on the entity's complexity), and this code may also be hard to maintain correctly. It is often tedious to maintain, difficult to type check and difficult to make cohesive.

For these reasons and some others, we have taken a design shortcut in persistence mapping by making the entities opt into supporting persistency in a generic way. 

Each domain entity is required to:
 1. Define a name of a logical collection (i.e. the name of a table, for use by database repository) (See: [EntityNameAttribute](https://github.com/jezzsantos/queryany/blob/master/src/QueryAny/EntityNameAttribute.cs))
 1. Define a set of properties (object dictionary) that represents the internal state of the entity (See: [Dehydrate](https://github.com/jezzsantos/queryany/blob/master/samples/ri/CarsDomain/Entities/EntityBase.cs)).
 1. Define a function that can restore the internal state of the entity from a set of properties (See: [Rehydrate](https://github.com/jezzsantos/queryany/blob/master/samples/ri/CarsDomain/Entities/EntityBase.cs)).
 1. Define an entity factory function that can be called to construct a new instance of the entity (See: [EntityFactory](https://github.com/jezzsantos/queryany/blob/master/samples/ri/Storage.Interfaces/IStorage.cs#L13)).

 With this entity policy in effect, domain entities can maintain their full OO behaviors and encapsulation whilst participating in persistence schemes, like the one implemented in this RI. Which is typically very hard to design for most developers. 

 > Note: Again, this is one strategy, there are many.

# Local Development and Manual Testing

1. First: [Getting Started](https://github.com/jezzsantos/queryany/wiki/Getting-Started) for details on what you need installed on your machine.
1. Start the `CarsApi` project locally with F5. (A browser should open to API documentation site).
1. You will need to start the Azure CosmosDB emulator on your machine (from the Start Menu).
1. Ensure that you have manually created a new cosmos database called: "Production" in CosmosDB.

To manually test everything is working, or debug the code:

1. Navigate to: [GET https://localhost:5001/cars/available](https://localhost:5001/cars/available) (you should get an empty array of cars in response)
1. Using a tool like PostMan or other REST client, create a new car by calling: `POST /cars` with a JSON body like this: `{"Year": 2020,"Make": "Honda","Model": "Civic"}`

# Automated Testing

1. First: [Getting Started](https://github.com/jezzsantos/queryany/wiki/Getting-Started) for details on what you need installed on your machine.
1. Build the solution.
1. Run all tests in the solution.

> Note: if the integration tests for one of the repositories fail, its likely due to the fact that you dont have that technology installed on your local machine, or that you are not running your IDE as Administrator, and therefore cant start/stop those local services. Refer to step 1 above.