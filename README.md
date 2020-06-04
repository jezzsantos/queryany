# QueryAny

**QueryAny** is a .NET Standard fluent data store query language that can be used to abstract away the 
idiosyncrasies of your chosen persistence store - your database. 

You can now choose to persist data in whatever store you like: SQL, MongoDB, NoSQL, CosmosDB, Redis or even JSON files, and not care a damn where your data is coming from or how it is persisted. Which is exactly what you want to do if you are crafting a new software product or tool. 

You can now completely de couple your persistence store from your domain code, and focus where you should on building out your domain.

For example:

`Query.From("Orders").Where("OrderId", Operator.EQ, "25").Select("OrderId", "Description")`

Can fetch this data from a SQL database, or from a No-SQL database, or from a JSON file just as easily. Why would you need to care?

# Motivation

**QueryAny** was developed to provide developers an easy path to mature from being bound to their favorite database technology to venture out and consider other persistence technologies. Especially those developers who have gotten stuck at designing every piece of software they build from a data model upwards. (Move from 'data-modeler' to 'domain-modeler').

**QueryAny** will help them de-couple their code from the implementation details and features specific of certain kinds of repository technology. For example, relational data structures in a SQL database. 

**QueryAny** prevents them having to *leak* those assumptions and dependencies into their domain code, as is common practice when using  ORM libraries and frameworks (eg. MS Entity Framework), and helps developers to be able to swap out their persistence stores when doing various test runs (eg. unit testing versus integration testing).

We wanted developers to be able to define their own simple storage interface (eg. `IStorage<TEntity>`), and have that as the only dependency in their doamin code. Then, be able to implement that interface in any chosen database technology at runtime.

For example, they may define a generic storage interface like this in their code:

```
    public interface IStorage<TEntity>
    {
        void Add(IKeyedEntity entity);

        void AddRange(IEnumerable<IKeyedEntity> entities);

        TEntity Update<TEntity>(TEntity entity, bool ignoreConcurrency)
            where TEntity : IKeyedEntity, new();

        void Delete(IKeyedEntity entity, bool ignoreConcurrency);

        void DeleteAll<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : IKeyedEntity;

        SearchResults<TEntity> Query<TEntity>(Query query, SearchOptions options)
            where TEntity : IKeyedEntity, new();

        int Count();

        int Count<TEntity>(Query query)
            where TEntity : IKeyedEntity, new();

        TEntity Get<TEntity>(string id)
            where TEntity : IKeyedEntity, new();
    }
```

And then implement that interface in an In-Memory store (in `Dictionary<TEntity>` or in Redis) for unit testing, or for SQLServerDB or CosmosDB or MongoDB in production.

But to realise that vision, we quickly realized that developers needed an effective way of querying collections from their repositories, and a query language is required to do that effectively.

Generalising and abstracting away the database's query language details is not a trivial matter. Idosyncracies that you definately don't wan't to pollute your domain objects with. Afterall, why should your domain objects even know that their state is held in a SQL database? or even that there are any relationships defined there at all!

**QueryAny** provides that abstraction with a fluent query language to access data from any repository. Its discoverable, its easy to use, and its extensible.

You just need to implement the language for a store of your choice.

## Credits

QueryAny was inspired by the work done on the http://funql.org/ project. But is not yet an implementation of the [FUNQL specification](http://funql.org/index.php/language-specification.html).

Follow us as we build this out for the community.
