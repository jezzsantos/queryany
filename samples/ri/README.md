# QueryAny Reference Implementation

This folder contains a close to real world example of a REST based API for managing Cars for a Car Sharing software product.

The example is split into several layers to mimic a real world implementation. 
It has tried not to be too opinionated about those layers other than to assume that you would split your projects into logical layers for reuse and structure.



The RI solution is structured as follows:

## CarsApi

This is the web host, in this case its ASP.NET Core running the [ServiceStack](http://www.servicestack.net) framework. We chose ServiceStack for two main reasons. (1) it makes defining and configuring services so much easier than WebApi, (2) it includes an `auto-mapper` essential for creating abstractions between service operations, domain logic and storage layers.

It defines the actual REST endpoints (service operations) and their contracts. 

It handles the conversion between HTTP requests <-> Domain Logic. Including: formats, exception mapping, routes, validation etc.

It contains the `AppHost` class (specific to ServiceStack) which loads all service endpoints, and injects all runtime dependencies.

## CarsDomain

Essentially the domain logic layer.

It defines the domain logic classes, and domain entities.

It contains domain specific logic, rules, etc.

## Services.Interfaces

Contains shared definitions of the service operations, intended to be shared across all services.

Also intended to be shared to clients.

## Storage.Interfaces

Contains shared definitions for consumers of the storage layer (i.e. domain logic).
Also intended to be shared with implementers of specific storage databases, and repositories.

## Storage

Concrete implementations of `IStorage<TEntity>` for the various storage technologies.

This is where all QueryAny implementations will exist for this sample.

> Generally speaking there would probably be a separate project for each implementation of the `IStorage<TEntity>` interface. eg. one for SqlServer, one for Redis, one for JSON file. 