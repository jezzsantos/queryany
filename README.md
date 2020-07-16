# QueryAny
[![Build status](https://ci.appveyor.com/api/projects/status/qwg1wen94kfe52jp/branch/master?svg=true)](https://ci.appveyor.com/project/JezzSantos/queryany/branch/master) [![NuGet](https://img.shields.io/nuget/v/QueryAny.svg?label=QueryAny)](https://www.nuget.org/packages/QueryAny) [![Release Notes](https://img.shields.io/nuget/v/QueryAny.svg?label=Release%20Notes&colorB=green)](https://github.com/jezzsantos/QueryAny/wiki/Release-Notes)

**QueryAny** is a query language for use with any database or data store technology. Use it as a `<generic>` repository pattern, and never leak database details into your domain logic again. Never take a dependency from your domain logic on any database or ORM technology again.

## Why?

Becuase things change, and you should protect your core domain from those external changes and implementation details.

* Don't you want to learn how to abstract away your persistence layers? (**Get rid of those ugly SQL statements in your business logic?**)
* Don't you want to discover how to use other data repositories than the only one you know now? (**Ever wanted to explore what a NoSQL database or InMemory database was good for?**)
* Don't you want to run your unit/integration tests faster than your database can run them? (**Then stop using databases in your testing!**)
* Have you considered the possibility that an In-mem persistence store (like Redis) is a better performing alternative to using a clumsy database server? 

OK, then you are going to need help to abstract your persistence layer from your domain objects properly.

**QueryAny** was developed to provide developers an easy path to mature from being bound to their favorite database technology to venture out and consider other persistence technologies. Especially for those developers who have gotten stuck at designing every piece of software they build from a database model upwards. And don't yet know how to de-couple their domains from their persistence layers.

**QueryAny** prevents developers from having to *leak* database assumptions and dependencies into their domain code, as is a common practice when using  ORM libraries and frameworks (eg. Entity Framework, etc). Also, essential for testability, helps developers to swap out their persistence stores when doing various test runs (eg. unit testing versus integration testing). Speed up your testing by a couple orders of magnitude.

We wanted developers to be able to define their own simple repository interface (eg. `IStorage<TEntity>`), and have that as the only dependency in their domain code. Then, be able to implement that interface in any chosen database technology, and plug it in at runtime.

For example, they may define a generic repository interface like this in their code:

```
    public interface IStorage<TEntity>
    {
        void Add(TEntity entity);

        TEntity Update(TEntity entity);

        void Delete(string id);

        TEntity Get(string id);

        QueryResults<TEntity> Query(Query query, QueryOptions options);

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
Query.From<OrderEntity>()
    .Join<CustomerEntity, OrderEntity>(c => c.Id, o => o.CustomerId)
    .Where<OrderEntity>(o => o.Id, Operator.EQ, "25")
    .Select(c => c.Id).Select(c => c.Name)
```

Can fetch this data from a SQL database, or from a Non-SQL database, or from JSON files just as easily. Why should your domain logic need to care where the data comes from?

> Note: that this example query uses joins to two different entities together to create a final result-set, but that does not require the database to implement joins natively at all, just like No-SQL databases do not.

> Note: This is NOT a new version of LINQ or intended to be like LINQ at all. Its simply a language for defining queries that can work across any datastore.

## Documentation

See our [reference architecture](https://github.com/jezzsantos/queryany/wiki/Reference-Architecture), and [our design notes](https://github.com/jezzsantos/queryany/wiki/Design) to learn more about how **QueryAny** works.

## Contributing

If you wish to contribute, please do, please first see our [Contributing Guidelines](CONTRIBUTING.md)

## Credits

QueryAny was inspired by the work done on the http://funql.org/ project. But is not an implementation of the [FUNQL specification](http://funql.org/index.php/language-specification.html).
