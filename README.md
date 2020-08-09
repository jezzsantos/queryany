# QueryAny
[![Build status](https://ci.appveyor.com/api/projects/status/qwg1wen94kfe52jp/branch/master?svg=true)](https://ci.appveyor.com/project/JezzSantos/queryany/branch/master) [![NuGet](https://img.shields.io/nuget/v/QueryAny.svg?label=QueryAny)](https://www.nuget.org/packages/QueryAny) [![Release Notes](https://img.shields.io/nuget/v/QueryAny.svg?label=Release%20Notes&colorB=green)](https://github.com/jezzsantos/QueryAny/wiki/Release-Notes)

**QueryAny** is a .NET agnostic query language for use with any database technology. 

Use it in your `IRepository<T>` pattern implementation, and never leak database query language details (like SQL) into your domain logic again. 

Apply clean architecture principles correctly, and never take a dependency from your domain logic on any database or ORM technology ever again.

## Why?

Because code changes, and you should protect your core domain from those external changes and implementation details.

* Don't you want to learn how to abstract away your persistence layers? (**Get rid of those ugly SQL statements in your business logic?**)
* Don't you want to discover how to use other data repositories than the only one you know now? (**Ever wanted to explore what a NoSQL database or In-Memory database was best used for?**)
* Don't you want to run your unit tests/integration tests faster than you are now without involving your database? (**Then stop testing your databases!**)
* Have you considered the possibility that an In-memory persistence store (like: Redis/MemCached) is a better performing alternative to using a clumsy database server? 

OK then, you are going to need help to abstract your persistence layer from your domain objects properly, so that you can write testable domain logic, decouple your architecture and pick and choose your repositories.

**QueryAny** was developed to provide developers an easy path to mature from being bound by their incumbent database technology to venture out and consider other persistence technologies. Especially for those developers who have gotten stuck at designing every piece of software they build from a database model upwards (Data Modelers). And don't yet know how to de-couple their domains from their persistence layers.

**QueryAny** prevents developers from having to *leak* database assumptions and dependencies into their domain code, as is a common practice when using  ORM libraries and frameworks (eg. Entity Framework, etc). Also, essential for testability, helps developers to swap out their persistence stores when doing various test runs (eg. unit testing versus integration testing). Speed up your testing by a couple orders of magnitude.

We wanted developers to be able to define their own simple repository interface (eg. `IStorage<TEntity>`), and have that as the only dependency in their domain code. Then, be able to implement that interface in any chosen database technology, and plug it in at runtime.

For example, they may define a generic repository interface like this in their code:

```
    public interface IRepository<TEntity>
    {
        void Create(TEntity entity);

        TEntity Update(TEntity entity);

        void Delete(string id);

        TEntity Get(string id);

        QueryResults<TEntity> Query(Query query);

        long Count();
    }
```

And then implement that interface in an In-Memory store (eg. using a collection of `ConcurrentDictionary<TEntity>` or in Redis or MemCache or whatever DB they like) for unit testing, or for SQLServerDB, Postgres, CosmosDB, MongoDB, Redis etc. in production.

But to realize that vision, developers need an effective way of defining queries from their repositories, and a query language is required to do that effectively.

Generalizing and abstracting away the database's query language details is not a trivial matter. Especially when trying to accommodate the differences between relational databases and non-relational databases.

## How?

**QueryAny** is a .NET Standard fluent data store query language, that can be used in any repository interface.

You choose to persist data in whatever store you like: SQL, MongoDB, NoSQL, CosmosDB, Redis or even JSON files, and your domain logic will not care a damn where your data is coming from or how it is persisted. Which is exactly what you want to do if you are crafting a new software product or tool, and you want to use [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) principles.

You can now completely decouple your persistence store from your domain code, and focus where you should on building out your domain.

For example, define a query in code like this:

```
var query = Query.From<OrderEntity>()
    .Join<OrderEntity>(c => c.Id, o => o.CustomerId)
    .Where<CustomerEntity>(c => c.Id, Operator.EqualTo, "25")
    .Select(c => c.Id).Select(c => c.Name)
```

Can fetch this data from a SQL database, or from a Non-SQL database, or from JSON files just as easily. 

Why should your domain logic need to care where the data/state comes from at all?

> Note: that this query uses joins to two different entities to create a final result-set, but that does not require the database to implement joins natively at all, just like No-SQL databases do not.

> Note: This is NOT a new version of LINQ or intended to be like LINQ at all. Its simply a fluent language for defining queries that can work across any persistence store. SQL is not the center of your universe.

## Documentation

See our [Reference Architecture](https://github.com/jezzsantos/queryany/wiki/Reference-Architecture), and [our design notes](https://github.com/jezzsantos/queryany/wiki/Design) to learn more about how **QueryAny** works.

Our [Reference Architecture](https://github.com/jezzsantos/queryany/wiki/Reference-Architecture) has fast become a very useful example of how to implement a decoupled implementation of a REST API that implements separate domains that follow many of the principles of DDD, but is a good stepping stone for novice/intermediate developers to understand these principles in practice. 

## Contributing

If you wish to contribute, please do, please first see our [Contributing Guidelines](CONTRIBUTING.md)

## Credits

**QueryAny** was inspired by the work done on the http://funql.org/ project. But is not an implementation of the [FUNQL specification](http://funql.org/index.php/language-specification.html).