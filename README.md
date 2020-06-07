# QueryAny

**QueryAny** is a .NET Standard fluent data store query language that can be used to abstract away the 
idiosyncrasies of your chosen persistence store - ie. your database technology.

You choose to persist data in whatever store you like: SQL, MongoDB, NoSQL, CosmosDB, Redis or even JSON files, and your domain logic will not care a damn where your data is coming from or how it is persisted. Which is exactly what you want to do if you are crafting a new software product or tool, and you want to use [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) principles.

You can now completely decouple your persistence store from your domain code, and focus where you should on building out your domain.

For example a query in code like this:

Why would you need to care when all you care about is that the data is persisted somewhere?

or even better a fully typed query, like this:

`Query.From<MyEntity>(e => e.Orders).Where<MyEntity>(e => e.OrderId, Operator.EQ, "25").Select<MyEntity>(e => e.OrderId, e => e .Description)`

can fetch this data from a SQL database, or from a No-SQL database, or from a JSON file just as easily. Why should your domain logic need to care?

## Motivation

**QueryAny** was developed to provide developers an easy path to mature from being bound to their favorite database technology to venture out and consider other persistence technologies. Especially as their product scales. Especially for those developers who have gotten stuck at designing every piece of software they build from a database model upwards. (Moving from 'data-modelers' to 'domain-modelers').

**QueryAny** will help developers de-couple their code from the implementation details and features specific of certain kinds of database technologies. For example, relational data structures in a SQL database. 

**QueryAny** prevents developers from having to *leak* database assumptions and dependencies into their domain code, as is a common practice when using  ORM libraries and frameworks (eg. MS Entity Framework). Also, essential for testability, helps developers to swap out their persistence stores when doing various test runs (eg. unit testing versus integration testing).

We wanted developers to be able to define their own simple storage interface (eg. `IStorage<TEntity>`), and have that as the only dependency in their domain code. Then, be able to implement that interface in any chosen database technology at runtime.

We wanted developers to be able to define their own simple storage interface (eg. `IStorage<TEntity>`), and have that as the only dependency in their domain code. Then, be able to implement that interface in any chosen database technology, and plug it in at runtime.

For example, they may define a generic storage interface like this in their code:

```
    public interface IStorage<TEntity> where TEntity : IKeyedEntity, new()
    {
        void Add(TEntity entity);

        TEntity Update(TEntity entity, bool ignoreConcurrency);

        void Delete(string id, bool ignoreConcurrency);

        TEntity Get(string id);

        QueryResults<TEntity> Query(Query query, SearchOptions options);

        long Count();
    }
```

And then implement that interface in an In-Memory store (eg. `ConcurrentDictionary<TEntity>` or in Redis or MemCache) for unit testing, or for SQLServerDB, Postgres, CosmosDB, MongoDB etc. in production.

But to realze that vision, we quickly realized that developers needed an effective way of querying collections from their repositories, and a query language is required to do that effectively.

Generalizing and abstracting away the database's query language details is not a trivial matter. Idiosyncrasies that you definitely don't want to pollute your domain objects with. After all, why should your domain objects even know that their state is held in a SQL database? or even that there are any relationships defined there at all!

**QueryAny** provides a useful query language to access data from any repository. It's discoverable, its easy to use, and its extensible. You just need to implement the language for a store of your choice.

## Example Usage

See our samples, as guidance for your data store of choice.

## Credits

QueryAny was inspired by the work done on the http://funql.org/ project. But is not yet an implementation of the [FUNQL specification](http://funql.org/index.php/language-specification.html).

Follow us as we build this out for the community.
