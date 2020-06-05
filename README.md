# QueryAny

**QueryAny** is a .NET Standard fluent data store query language that can be used to abstract away the 
idiosyncrasies of your chosen persistence store - your database. 

With **QueryAny** you can choose to ignore your persistence store when designing your domain objects, and later plug in whatever store you like: SQL, MongoDB, NoSQL, CosmosDB, Redis or even JSON files, and not care a damn where your data is coming from or how it is persisted. Which is exactly what you want to do if you are crafting a new software product or tool these days. 

You can now focus where you should be, on building out your domain and forget obout ORM's, SQL injection and all that crap that gets in the way of focusing on your domain.

For example, you would now write code like this:
```
Query.From<Order>()
    .Where<Order>(e => e.Id, Operator.EQ, "25").And<Order>(e => e.IsAvailable)
    .Select<Order>( e= > e.OrderId, e => e.Amount, e => e.Description);
```
Which can fetch this data from a SQL database, or from a No-SQL database, or from a JSON file just as easily. 

Why would you need to care when all you care about is that the data is persisted somewhere?

## Motivation

**QueryAny** was developed to provide developers an easy path to mature from being bound to their favorite database technology and ORM to venture out and consider other persistence technologies. Especially those developers who have gotten stuck at designing every piece of software they build from a data model upwards. (Moving away from being a 'data-modeler' to being a 'domain-modeler').

**QueryAny** will help them de-couple their code from the implementation details and the idiosyncracies specific of certain kinds of repository technology. For example, relational data structures in a SQL database.

**QueryAny** prevents them having to *leak* those assumptions, the frameworks and dependencies that mirror them into their domain code, as is common practice when using  ORM libraries and frameworks (eg. MS Entity Framework)

**QueryAny** helps developers to easy swap out their persistence stores when doing various test runs (eg. unit testing versus integration testing).

We wanted developers to be able to define their own simple storage interface (eg. `IStorage<TEntity>`), and have that as the only dependency in their domain code. Then, be able to implement that interface in any chosen database technology, and plug it in at runtime.

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

And then implement that interface in an In-Memory store (eg. `ConcurrentDictionary<TEntity>` or in Redis or MemCache) for unit testing, or for SQLServerDB, Postgres, CosmosDB, MongoDB etc. in production.

But to realise that vision, developers need an effective way of querying collections from their repositories, and a query language is required to do that effectively.

Generalising and abstracting away the database's query language capabilities and details is not a trivial matter. Especially resolving the differnces in capabilities between SQL and no-SQL capabilities. Idosyncracies and knowledge that you definately wan't to avoid polluting your domain objects with. Afterall, why should your domain objects even know that their state is held in a relationsal or non-relational database? or even that there are any relationships defined in there at all! Relational databases are not the only way to persist state, nor the most superior way to store state for many systems.

**QueryAny** provides a useful query language to access data from any repository. It's discoverable, its easy to use, and its extensible. You just need to implement the language for a store of your choice.

## Example Usage

We have a [reference architecture](https://github.com/jezzsantos/queryany/wiki/Reference-Architecture) sketched up for those who want to see what it looks like to use QueryAny to de-couple your data access from your domain objects using this pattern.

## Credits

QueryAny was inspired by the work done on the http://funql.org/ project. But is not yet an implementation of the [FUNQL specification](http://funql.org/index.php/language-specification.html).

Follow us as we build this out for the community.
