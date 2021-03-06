﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Api.Common.Properties;
using Common;
using Domain.Interfaces.Entities;
using ServiceStack;

namespace Api.Common
{
    public class DomainFactory : IDomainFactory
    {
        private const string FactoryMethodName = "Rehydrate";
        private readonly Dictionary<Type, AggregateRootFactory<IPersistableAggregateRoot>> aggregateRootFactories;
        private readonly IDependencyContainer container;
        private readonly Dictionary<Type, EntityFactory<IPersistableEntity>> entityFactories;
        private readonly Dictionary<Type, ValueObjectFactory<IPersistableValueObject>> valueObjectFactories;

        public DomainFactory(IDependencyContainer container)
        {
            container.GuardAgainstNull(nameof(container));
            this.container = container;
            this.aggregateRootFactories = new Dictionary<Type, AggregateRootFactory<IPersistableAggregateRoot>>();
            this.entityFactories = new Dictionary<Type, EntityFactory<IPersistableEntity>>();
            this.valueObjectFactories = new Dictionary<Type, ValueObjectFactory<IPersistableValueObject>>();
        }

        public IReadOnlyDictionary<Type, AggregateRootFactory<IPersistableAggregateRoot>> AggregateRootFactories =>
            this.aggregateRootFactories;

        public IReadOnlyDictionary<Type, EntityFactory<IPersistableEntity>> EntityFactories => this.entityFactories;

        public IReadOnlyDictionary<Type, ValueObjectFactory<IPersistableValueObject>> ValueObjectFactories =>
            this.valueObjectFactories;

        public IPersistableAggregateRoot RehydrateAggregateRoot(Type entityType,
            IReadOnlyDictionary<string, object> rehydratingPropertyValues)
        {
            rehydratingPropertyValues.GuardAgainstNull(nameof(rehydratingPropertyValues));

            if (!this.aggregateRootFactories.ContainsKey(entityType))
            {
                throw new InvalidOperationException(Resources.DomainFactory_EntityTypeNotFound.Fmt(entityType.Name));
            }

            var identifier = rehydratingPropertyValues.GetValueOrDefault<Identifier>(nameof(IIdentifiableEntity.Id));
            var factory = this.aggregateRootFactories[entityType];
            return factory(identifier, this.container, rehydratingPropertyValues);
        }

        public IPersistableEntity RehydrateEntity(Type entityType,
            IReadOnlyDictionary<string, object> rehydratingPropertyValues)
        {
            rehydratingPropertyValues.GuardAgainstNull(nameof(rehydratingPropertyValues));

            if (!this.entityFactories.ContainsKey(entityType))
            {
                throw new InvalidOperationException(Resources.DomainFactory_EntityTypeNotFound.Fmt(entityType.Name));
            }

            var identifier = rehydratingPropertyValues.GetValueOrDefault<Identifier>(nameof(IIdentifiableEntity.Id));
            var factory = this.entityFactories[entityType];
            return factory(identifier, this.container, rehydratingPropertyValues);
        }

        public IPersistableValueObject RehydrateValueObject(Type valueObjectType, string rehydratingPropertyValue)
        {
            valueObjectType.GuardAgainstNull(nameof(valueObjectType));
            rehydratingPropertyValue.GuardAgainstNull(nameof(rehydratingPropertyValue));

            if (!this.valueObjectFactories.ContainsKey(valueObjectType))
            {
                throw new InvalidOperationException(
                    Resources.DomainFactory_ValueObjectTypeNotFound.Fmt(valueObjectType.Name));
            }

            return this.valueObjectFactories[valueObjectType](rehydratingPropertyValue, this.container);
        }

        public void RegisterDomainTypesFromAssemblies(params Assembly[] assembliesContainingFactories)
        {
            assembliesContainingFactories.GuardAgainstNull(nameof(assembliesContainingFactories));

            if (assembliesContainingFactories.Length <= 0)
            {
                return;
            }

            foreach (var assembly in assembliesContainingFactories)
            {
                var domainTypes = assembly.GetTypes()
                    .Where(t => IsAggregateRoot(t) || IsEntity(t) || IsValueObject(t))
                    .ToList();
                foreach (var type in domainTypes)
                {
                    if (IsAggregateRoot(type))
                    {
                        var factoryMethod = GetAggregateRootFactoryMethod(type);
                        if (factoryMethod != null)
                        {
                            if (IsWrongNamedOrHasParameters(factoryMethod))
                            {
                                throw new InvalidOperationException(
                                    Resources.DomainFactory_FactoryMethodHasParameters.Fmt(type.Name,
                                        factoryMethod.Name, FactoryMethodName));
                            }

                            var @delegate =
                                (AggregateRootFactory<IPersistableAggregateRoot>) factoryMethod.Invoke(null, null);
                            this.aggregateRootFactories[type] = @delegate;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                Resources.DomainFactory_AggregateRootFactoryMethodNotFound.Fmt(type.Name,
                                    FactoryMethodName));
                        }
                    }

                    if (IsEntity(type))
                    {
                        var factoryMethod = GetEntityFactoryMethod(type);
                        if (factoryMethod != null)
                        {
                            if (IsWrongNamedOrHasParameters(factoryMethod))
                            {
                                throw new InvalidOperationException(
                                    Resources.DomainFactory_FactoryMethodHasParameters.Fmt(type.Name,
                                        factoryMethod.Name, FactoryMethodName));
                            }

                            var @delegate = (EntityFactory<IPersistableEntity>) factoryMethod.Invoke(null, null);
                            this.entityFactories[type] = @delegate;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                Resources.DomainFactory_EntityFactoryMethodNotFound.Fmt(type.Name,
                                    FactoryMethodName));
                        }
                    }

                    if (IsValueObject(type))
                    {
                        var factoryMethod = GetValueObjectFactoryMethod(type);
                        if (factoryMethod != null)
                        {
                            if (IsWrongNamedOrHasParameters(factoryMethod))
                            {
                                throw new InvalidOperationException(
                                    Resources.DomainFactory_FactoryMethodHasParameters.Fmt(type.Name,
                                        factoryMethod.Name, FactoryMethodName));
                            }

                            var @delegate =
                                (ValueObjectFactory<IPersistableValueObject>) factoryMethod.Invoke(null, null);
                            this.valueObjectFactories[type] = @delegate;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                Resources.DomainFactory_ValueObjectFactoryMethodNotFound.Fmt(type.Name,
                                    FactoryMethodName));
                        }
                    }
                }
            }
        }

        public static DomainFactory CreateRegistered(IDependencyContainer container,
            params Assembly[] assembliesContainingFactories)
        {
            var domainFactory = new DomainFactory(container);
            domainFactory.RegisterDomainTypesFromAssemblies(assembliesContainingFactories);
            return domainFactory;
        }

        private static bool IsWrongNamedOrHasParameters(MethodBase method)
        {
            return method.Name.NotEqualsOrdinal(FactoryMethodName)
                   || method.GetParameters().Length != 0;
        }

        private static bool IsAggregateRoot(Type type)
        {
            return !type.IsAbstract && typeof(IPersistableAggregateRoot).IsAssignableFrom(type);
        }

        private static bool IsEntity(Type type)
        {
            return !type.IsAbstract && typeof(IPersistableEntity).IsAssignableFrom(type);
        }

        private static MethodInfo GetAggregateRootFactoryMethod(Type type)
        {
            return type
                .GetMethods()
                .FirstOrDefault(method => method.IsStatic
                                          && method.IsPublic
                                          && method.ReturnType != typeof(void)
                                          && method.ReturnType.BaseType == typeof(MulticastDelegate)
                                          && method.ReturnType.IsGenericType
                                          && method.ReturnType.GenericTypeArguments.Any()
                                          && typeof(IPersistableAggregateRoot).IsAssignableFrom(
                                              method.ReturnType.GenericTypeArguments[0])
                                          && method.ReturnType.GetGenericTypeDefinition()
                                              .IsAssignableFrom(typeof(AggregateRootFactory<>)));
        }

        private static MethodInfo GetEntityFactoryMethod(Type type)
        {
            return type
                .GetMethods()
                .FirstOrDefault(method => method.IsStatic
                                          && method.IsPublic
                                          && method.ReturnType != typeof(void)
                                          && method.ReturnType.BaseType == typeof(MulticastDelegate)
                                          && method.ReturnType.IsGenericType
                                          && method.ReturnType.GenericTypeArguments.Any()
                                          && typeof(IPersistableEntity).IsAssignableFrom(
                                              method.ReturnType.GenericTypeArguments[0])
                                          && method.ReturnType.GetGenericTypeDefinition()
                                              .IsAssignableFrom(typeof(EntityFactory<>)));
        }

        private static bool IsValueObject(Type type)
        {
            return !type.IsAbstract
                   && type.IsSubclassOfRawGeneric(typeof(ValueObjectBase<>));
        }

        private static MethodInfo GetValueObjectFactoryMethod(Type type)
        {
            return type
                .GetMethods()
                .FirstOrDefault(method => method.IsStatic
                                          && method.IsPublic
                                          && method.ReturnType != typeof(void)
                                          && method.ReturnType.BaseType == typeof(MulticastDelegate)
                                          && method.ReturnType.IsGenericType
                                          && method.ReturnType.GenericTypeArguments.Any()
                                          && method.ReturnType.GenericTypeArguments[0]
                                              .IsSubclassOfRawGeneric(typeof(ValueObjectBase<>))
                                          && method.ReturnType.GetGenericTypeDefinition()
                                              .IsAssignableFrom(typeof(ValueObjectFactory<>)));
        }
    }
}