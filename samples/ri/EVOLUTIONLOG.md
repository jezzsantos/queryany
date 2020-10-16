This is a high-level story log file recording the evolution of the QueryAny Reference Implementation (RI).

### Overview

This log has been intentionally put together to give developers, interested in this subject matter, an understanding and confidence and possibly even a plan on how they might go about achieving the same outcome as this reference implementation, but on an existing code base they work on. Clearly, if you started from scratch, the RI would be an accelerator, but this is often not the case. What we want to do is encourage developers to evolve what they have already.

The intention of this log is to tell the story of how this repo laid down its foundation as a typical monolithic API architecture, that you may typically build, and then how it could change over a few staged evolutions into a distributed, eventing architecture, that uses event sourcing and CQRS to manage decoupling your various services and abstractions as you scale your complexity up (think larger domains) and as you scale your deployment model out (think splitting out and deploying services separately).

The journey described in this log was intentionally coded and recorded in the way that it is to help/encourage developers to follow a similar path. But we cannot claim that the history you see here was choreographed precisely every step of the way. It wasn't. We simply built a typical starting implementation which was very simple, with a specific evolution in mind, and coded the journey in the way that felt the most natural at the time. Some of this journey was learned during the process, but most was designed with the explicit intention of teaching those following some new skills.

The log below tells the story and identifies the key git commits that can be examined in more detail.

> This historical log was actually created when reaching Evolution 6 in this story, so there are some errata in the journey which can be expected in any venture. They are noted where found.

# The Log

## 1. Beginnings

 * Commit: [f3d070](https://github.com/jezzsantos/queryany/commit/f3d07064f7c2ea745d04dc1b360a8d694d718afd)
 * Version 1.0 of accompanying [QueryAny nuget library](https://www.nuget.org/packages/QueryAny/).

Established the base library of QueryAny with some basic functionality, giving us a initial generic query language for defining generic queries on any repository interface.

```
var query = Query.From<CustomerEntity>()
	.Where(customer => customer.Id, EqualTo, "25")
	.OrWhere(subQuery =>
		subQuery.Where(customer => customer.Id, NotEqualTo, "25")
		.AndWhere(customer => customer.CreatedDateUc, GreaterThan, now));
```

### Structural Patterns

At this point we have a pretty typical implementation of a REST API (using the ServiceStack framework), along with an anemic domain model (lacking behavior), and a 'transactional script' for an 'Application Layer'. 

> An anemic domain model is devoid of any behaviour, validation and processing of domain concepts. Which is typically found instead encoded into a transactional script. The domain objects are just simply POCOs (or DTO's), designed for CRUD processing. Which is all too common.

We have a "generic repository" pattern defined by `IStorage<TEntity>` and several storage adapters to real repository technologies such as: InMemory, and TableStorage at this point. (more of those coming later)

```
    public interface IStorage<TEntity> where TEntity : IKeyedEntity, new()
    {
        string Add(TEntity entity);

        TEntity Update(TEntity entity, bool ignoreConcurrency);

        void Delete(string id, bool ignoreConcurrency);

        TEntity Get(string id);

        QueryResults<TEntity> Query(QueryClause<TEntity> query, SearchOptions options);

        long Count();

        void DestroyAll();
    }
```

 This is a very common starting point for most API architectures, and a good foundation for making significant changes.

> However, since it is an RI (and not a real product), the size and complexity of this domain is not realized here to any level of maturity. In a real product, we would expect to see 10's - 100's of API's and resources. As such, this stage does not yet exhibit insidious things like: In-process coupling between various API's and their associated application layers. Which are the things that in practice makes the next set of transitions far harder to pull off without some evolutionary stages.

However, at this point we do demonstrate good disciplined de-coupling of a repository layer, and good separations of concerns between Web, Service Layer and the Domain (albeit anemic). 

Functionally it is not much more than just one query and no commands.

 ## 2. Repositories and Domain Foundation

 * Commit: [2e667b](https://github.com/jezzsantos/queryany/commit/2e667b3061f5d0f5d517b3ef175947bd8a794c46)

### Structural Patterns

At this point, we have a couple more storage adapters to real repository technologies: InMemory, Redis, CosmosDB and TableStorage at this point. (more coming later) . 

There is a adapter interface`IRepository` for each of these implementations, and a generic storage interface `IStorage<TEntity>` where `TEntity` is a type that supports persistence `IPersistableEntity` which has a bi-directional dehydration pattern to an object dictionary.

![Evolution 2](https://raw.githubusercontent.com/wiki/jezzsantos/queryany/Images/Evo2.png)

### Domain Patterns

To go along with that we have the introduction of Domain entities defined by a base class `EntityBase` that don't do persistence but that do support being persisted through `IPersistableEntity`. This means that all commands and queries through `IStorage<Tentity>` are able to read and write these entities to and from storage. But at this stage rely on the fact that all domain Entities types must have a public parameter-less constructor, to allow repositories to instantiate new instances of them when making instances of them (i.e. in gets and queries). 

> Noting that, instantiation is one of the key design constraints when dealing with persistence of .net types in generic repository interfaces.

On the domain entity side, we have moved from anemic entities to reconciling behavior from the application layer into the entities themselves. However, at this point validation is not there yet.  The application layer is getting thinner as processing moves into entities, and the domain is slightly expanded to include some commands (like: Create and Occupy cars.)

>  The domain itself at this point is very thin, and this gets a little more fleshed out in the next milestone. 

## 3. DDD Entities and Value Objects

*  Commit: [9bce55](https://github.com/jezzsantos/queryany/commit/9bce55441ae44ef04695883989e098c3e147fad5)

This is a large leap into more strict DDD patterns and principles, and more explicit project structure. 

>  The QueryAny library is pretty much complete at this point, and sees no more development for the next few evolutions.

### Structural Patterns

We have restructured the entire solution to be more explicit about the boundaries between Infrastructure, Application and Domain projects and now focus on managing their dependencies properly.

![Evolution 3](https://raw.githubusercontent.com/wiki/jezzsantos/queryany/Images/Evo3.png) 

The Application Layer has been separated out of the domain explicitly and now has thinned out into its final form and patterns.

We now start to get an appreciation that the vast majority of the code and thus complexity is in the Infrastructure projects (both API and Persistence). However, our domain is still pretty small. 

> In a real product, this domain would certainly be larger in terms of number of entities and value objects, and possibly have more than just one bounded context. 

We have added a bunch of additional attributes to the whole architecture at this point: logging, fluent assertions for testing, and the QueryAny library has been extended to include: ordering, offset and limiting (orderby, take and skip). 

### Domain Patterns

Along with that we now define DDD composite ValueObjects and have to deal with persisting their internal values.  

Entity identification has moved from repository layer into the constructor of each entity (`IIdentiferFactory`), so that new temporal instances of entities, also have an identifier and thus can be valid at all times in the domain. Identification becomes a concern of the domain, not the infrastructure.

There are no notions of DDD aggregates that this point in the domain as the domain is still pretty lean. 

On the other hand the domain has expanded to include the ValueObjects and their mappings to and from DTOs, and they now include validation rules and handle state change rules that were previously handled in the application layer.

### Persistence Patterns

We now have added the final `SqlServerRepository` implementation.

In terms of entity instancing entities from persistence, we have a new pattern that permits entities to have their own forms of their own constructors removing the `new()` constraint on their types. 

We use are using  (a transitionary) `EntityFactory<TEntity>` pattern that is baked into the `IRepository` abstraction, and instantiation of entities from storage is moved into the repository layer.

## 4. Eventing Genesis

*  Commit: [d68245](https://github.com/jezzsantos/queryany/commit/d682454be4a748cd756c15faa7b8f54661447f02)

In this evolution we start to introduce Root Aggregates and the use of events to mark the change the state of Aggregates, and their child entities and value objects. This being the first step to an eventing architecture.

### Structural Patterns

Not a great deal of change made in the structural makeup of the architecture. The kinds of projects used to describe each of the main layers of the architecture have already taken shape in the respective Infrastructure, Application, and Domain areas.

What is new in this evolution is the expansion of the domain to demonstrate aggregates and domain services. We decided to introduce another separate API to manage persons, and that has resulted in a number of new projects in each of the layers specific to persons. These new projects follow the same patterns as the cars API. 

> We did this because we wanted to demonstrate aggregates and we wanted to demonstrate how aggregate roots and domains would communicate. 

Now in terms of de-coupling, we are aiming at (in a future evolution) to use events to communicate between domains (i.e. between persons and cars) But right now, we aren't able to support that de-coupling that way, so we need to do it gradually over the next couple of evolutions.

> As would be expected if you were re-architecting an existing product.

Since we don't yet have the ability to decouple services from each other using events and some eventing architecture (i.e. a bus or queue or event stream), we need to do the next best thing and decouple domains from each other using remote REST calls. For this we create our first domain service `IPersonsService` and implement it as a synchronous HTTP REST call. 

> The undesirable side effect of this is that we inadvertently introduce potential synchronous call latency in any calls from the cars service that need information from the person service.  We accept this risk for now. 

![Evolution 4](https://raw.githubusercontent.com/wiki/jezzsantos/queryany/Images/Evo4.png)

> Note: There is an unexpected rogue project reference from the test projects of the Domain to the Infrastructure Layer in this diagram which is corrected in evolution 6. (this wasn't discovered until later evolutions). Which explains why the Infrastructure layer is at the top of the diagram rather than the Domain layer which should be at the top.

### Domain Patterns

In the domain we've made some pretty significant advancements.

We have now forced ourselves to define aggregate roots so that we can manage child entities. Aggregate roots have special rules, which include things like:

- The aggregate root defines a transactional boundary (i.e. the aggregate root cannot affect a change in state to anything outside it itself and its child objects)
- Which means that any change of state to any child entity or value object must be enacted directly through the aggregate root. Which means that, you cannot navigate down to any child entity or value object and tell it to do something without doing that thing through the aggregate root. All state changing operations must be surfaced to the aggregate root.
- And furthermore should there be a change of state from any entity or value object anywhere within the aggregate root hierarchy, the aggregate root should be notified of that change.

Furthermore, to achieve all this, and because we are taking steps to move towards an eventing architecture, we want all changes in state from anywhere in the aggregate root (whether they are directly caused by the aggregate root or side-effects of changes to the aggregate root) to raise events on the aggregate root. 

For that we have implemented an event raising pattern in aggregate roots, and added an event notification pattern in child entities, so that all change events in any entity or value object anywhere within the aggregate root raises change events. 

> At this point in the evolution the aggregate root simple caches these changes in an events collection.

### Persistence Patterns

At this point in the evolution, we are persisting changes to aggregates and entities through a domain specific storage interface.(e.g. `ICarStorage`) 

```
public interface ICarStorage
{
    CarEntity Create(CarEntity car);

    CarEntity Get(Identifier toIdentifier);

    CarEntity Update(CarEntity car);

    List<CarEntity> SearchAvailable(DateTime fromUtc, DateTime toUtc, SearchOptions options);
}
```

Instead of the generic counterpart:

```
public interface IStorage<TEntity> where TEntity : IPersistableEntity
{
    IDomainFactory DomainFactory { get; }

    TEntity Add(TEntity entity);

    TEntity Upsert(TEntity entity);

    void Delete(Identifier id);

    TEntity Get(Identifier id);

    QueryResults<TEntity> Query(QueryClause<TEntity> query);

    long Count();

    void DestroyAll();
}
```

We have moved away from using a generic storage interface like `IStorage<CarEntity>` to a non-generic one like `ICarStorage` that implements only the storage methods that are required by the cars application layer.  

Although the specific interface provides methods specific to this application which make it more aligned with the domain, under the covers it actually wraps the generic interface `IStorage<TEntity>` for each of the child entites in the aggregate. The outcome is the same as what you would get if you build the traditional repository pattern from scratch. Either way this is an implementation detail.

What is interesting about persisting aggregate roots, is that when you persist the aggregate root, you must also persist all of its changed child entities and value objects in the same logical unit or work/transaction. And when you read an aggregate root from persistence you need to read all its child entities at the same time.

> This constraint can lead to N+1 problems if you chose to model your relationships as unbounded collections in your domain. Generally, you try not to design such relationships in your domain, because they are unlikely to be really needed in your domain as general lists with all items in them. But nonetheless, they can be difficult to optimize. This problem will be solved in later evolutions when we move to eventing storage. But for now, its potentially a big problem for existing codebases to transition to.  

The last thing to mention about storage at this evolution is how the repositories are instantiating aggregate roots and entities. Previously, we were defining a `EntityFactory<TEntity>` function in each entity that provides the custom constructor for the entity. IN the previous evolution we were coupling the factory to the storage implementation which constrained us to have one storage abstraction per entity. In this evolution, we want to be able to persist multiple entities per storage abstraction, so we provide a central `DomainFactory` to which we register all the `EntityFactory<Tentity>` factories and all the `ValueObjectFactory<TValueObject>` factories. So that at runtime, the repository layer can ask for and use the appropriate factory to create an instance of the appropriate entity.   

## 5. Event Persistence

*  Commit: [6aecf5](https://github.com/jezzsantos/queryany/commit/6aecf5c8944017db9e16516a67aff6471e9df6b0)

This evolution is an intermediate step towards full CQRS but completes the eventing persistence pattern.

### Structural Patterns

In this evolution we have defined our first domain service `IEmailService` which is used when creating persons, to verify uniqueness across all persons in the domain.

We have also moved the `ServiceClients` project to the Infrastructure layer.

![Evolution 5](https://raw.githubusercontent.com/wiki/jezzsantos/queryany/Images/Evo5.png)

>  Note: There is still an unexpected rogue project reference from the test projects of the Domain to the Infrastructure Layer in this diagram which is corrected in evolution 6. (this wasn't discovered until later evolutions). Which explains why the Infrastructure layer is at the top of the diagram rather than the Domain layer which should be at the top.

### Domain Patterns

  We've re-worked how domain entities and aggregates get their identifiers, and now use a naming pattern inspired by Stripe.com. Instead of using GUIDs for every instance of every entity type, the identifier of a specific entity starts with a short prefix abbreviation of the entity, followed by an encoded GUID. 

For example: all cars have identifiers like: `car_sDTGyx96Xc` and all persons have identifiers like `per_34fXcIy69sOP`  

All state changes in the aggregates now raise events, and the current state of the entity is defined by the change events. This means that when we load the entity from event storage the current state is rebuilt by replaying the change events on top of each other. A process referred to a *left-folding*. It also means that when an entity changes, only the events that were raise are persisted to event storage.

### Persistence Patterns

In this evolution, we have split the incumbent storage interface `IStorage<TEntity>` into three interfaces: `ICommandStorage<TEntity>`, `IQueryStorage<TDto>`, and `IEventingStorage<TAggregateRoot>` , and we are preparing the infrastructure for building read models with `IReadModelSubscription`.

We no longer use `ICommandStorage<TEntity>` in any command that changes the state of the aggregates, since all these state changes are recorded as events.

> At this stage we, still do need to use `ICommandStorage<TEntity>` to save a usual "snapshot" of the state in the repository so that queries still have data to query. In the next evolution we will be switching to read models for all queries.

## 6. CQRS plus Event Sourcing

*  Commit: [1e7a3f](https://github.com/jezzsantos/queryany/commit/1e7a3f0f85263a3bee34ba5b3e7878242cfb23d4)

### Structural Patterns

In this evolution we have moved to a full CQRS pattern with Event Sourcing in the infrastructural layers, which now also include full CQRS Read Models.

This means that we are storing aggregate changes in event storage, and use those events to build and update read models for querying.

This kind of separation and interaction has some key implications:

1. The current state of any entity is built (from scratch) whenever it is read from storage. **Note:** Most entities in practice, over their lifetime will have far fewer changes to them than most developers may anticipate. So the latency to download and rebuild their state is generally relatively small. (unless of course you have entities with hundreds or thousands of state changes to load. Which may be an early indication of poor design in the domain, or you may need to consider event snapshots.)
2.  Whenever the state of an entity changes, events describe the change, and they (and only they) are persisted back to the event store (as they are immutable).
3. The change events must then be relayed to all registered projections to keep read models up to date. **Note**: In a synchronous model there is no latency in that update. In future evolutions, this latency may climb as these mechanisms may become asynchronous - moving towards eventual consistency.
4. Aggregates roots and domains themselves now have an effective means to communicate through change which very *effectively* de-couples them.

>  At first look, this pattern may seem inefficient, relative to what you are used to if you are comparing to CRUD based patterns. Since we now store the state of any particular entity in two places. (1) the event store, (2) the read models. And it is true that it is a lot more work to set this up and maintain. 
>  Recommend reading about CQRS and its benefits long terms as software complexity starts to increase, and become more distributed as they need to scale out. 

![Evolution 6](https://raw.githubusercontent.com/wiki/jezzsantos/queryany/Images/Evo6.png)

### Domain Patterns

Not much in the way of change has occurred in this evolution in the domain, only some minor refactoring.

### Persistence Patterns

We've now replaced the use of `ICommandStorage<TEntity>` with `IEventStreamStorage<TEntity>` for persisting state in aggregates and their child entities. We have switched to `IQueryStorage<TEntity>` for all queries. 

> Note: Command storage is now only used in utilities.

Also, note that it is now only root aggregates that require any persistence. Which means that child entities no longer need to support persistence at all! This makes the codebase remarkably thinner than before, eliminating any persistence mapping and various storage updates for each entity.

This evolution has necessarily changed the interface of `IRepository` since in this evolution we are still using the same `IRepository` implementation for `ICommanStorage<TEntity>`, `IQueryStorage<TDto>` and `IEventStreamStorage<TAggregateRoot>` which each have different constraints on the DTO types being passed to and from them. 

Essentially, the interface to and from `IRepository` is now defined in terms of an object dictionary of properties (`IReadOnlyDictionary<string,object>`) containing the property values of each entity. This is accompanied with metadata to describe the property value types (i.e. when null values are used). These collections are expressed as either a `CommandEntity` or as a `QueryEntity`, and metadata expressed as `EntityMetadata`.

> This is necessary so that the repository implementations can make their own decisions about how to persist the various data types that must be supported: `string`, `bool`, `byte[]`, `Guid`, `int`, `long`, `double`, `DateTime`, `DateTimeOffset`, or any complex type. 