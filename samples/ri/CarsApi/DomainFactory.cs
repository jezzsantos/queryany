using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CarsApi.Properties;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;
using ServiceStack;

namespace CarsApi
{
    public class DomainFactory : IDomainFactory
    {
        private readonly IDependencyContainer container;
        private readonly Dictionary<Type, EntityFactory<IPersistableEntity>> entityFactories;
        private readonly Dictionary<Type, ValueObjectFactory<IPersistableValueObject>> valueObjectFactories;

        public DomainFactory(IDependencyContainer container)
        {
            container.GuardAgainstNull(nameof(container));
            this.container = container;
            this.entityFactories = new Dictionary<Type, EntityFactory<IPersistableEntity>>();
            this.valueObjectFactories = new Dictionary<Type, ValueObjectFactory<IPersistableValueObject>>();
        }

        public IReadOnlyDictionary<Type, EntityFactory<IPersistableEntity>> EntityFactories => this.entityFactories;

        public IReadOnlyDictionary<Type, ValueObjectFactory<IPersistableValueObject>> ValueObjectFactories =>
            this.valueObjectFactories;

        public IPersistableEntity RehydrateEntity(Type entityType,
            IReadOnlyDictionary<string, object> rehydratingPropertyValues)
        {
            rehydratingPropertyValues.GuardAgainstNull(nameof(rehydratingPropertyValues));

            if (!this.entityFactories.ContainsKey(entityType))
            {
                throw new InvalidOperationException(Resources.DomainFactory_EntityTypeNotFound.Format(entityType.Name));
            }

            var entity = this.entityFactories[entityType](rehydratingPropertyValues, this.container);
            entity.Rehydrate(rehydratingPropertyValues);
            return entity;
        }

        public IPersistableValueObject RehydrateValueObject(Type valueObjectType, string rehydratingPropertyValue)
        {
            valueObjectType.GuardAgainstNull(nameof(valueObjectType));
            rehydratingPropertyValue.GuardAgainstNull(nameof(rehydratingPropertyValue));

            if (!this.valueObjectFactories.ContainsKey(valueObjectType))
            {
                throw new InvalidOperationException(
                    Resources.DomainFactory_ValueObjectTypeNotFound.Format(valueObjectType.Name));
            }

            var valueObject = this.valueObjectFactories[valueObjectType](rehydratingPropertyValue, this.container);
            valueObject.Rehydrate(rehydratingPropertyValue);
            return valueObject;
        }

        public void RegisterTypesFromAssemblies(params Assembly[] assembliesContainingFactories)
        {
            assembliesContainingFactories.GuardAgainstNull(nameof(assembliesContainingFactories));

            if (assembliesContainingFactories.Length <= 0)
            {
                return;
            }

            foreach (var assembly in assembliesContainingFactories)
            {
                foreach (var type in assembly.GetTypes().Where(t => IsEntity(t) || IsValueObject(t)))
                {
                    if (IsEntity(type))
                    {
                        var factoryMethod = GetEntityFactoryMethod(type);
                        if (factoryMethod != null)
                        {
                            if (IsWrongNamedOrHasParameters(factoryMethod))
                            {
                                throw new InvalidOperationException(
                                    Resources.DomainFactory_FactoryMethodHasParameters.Format(type.Name,
                                        factoryMethod.Name, nameof(IPersistableEntity.Rehydrate)));
                            }

                            if (EntityFactories.ContainsKey(type))
                            {
                                throw new InvalidOperationException(
                                    Resources.DomainFactory_EntityFactoryMethodExists.Format(type.Name,
                                        factoryMethod.Name));
                            }

                            var @delegate = (EntityFactory<IPersistableEntity>) factoryMethod.Invoke(null, null);
                            this.entityFactories.Add(type, @delegate);
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                Resources.DomainFactory_EntityFactoryMethodNotFound.Format(type.Name,
                                    nameof(IPersistableEntity.Rehydrate)));
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
                                    Resources.DomainFactory_FactoryMethodHasParameters.Format(type.Name,
                                        factoryMethod.Name, nameof(IPersistableValueObject.Rehydrate)));
                            }

                            if (ValueObjectFactories.ContainsKey(type))
                            {
                                throw new InvalidOperationException(
                                    Resources.DomainFactory_EntityFactoryMethodExists.Format(type.Name,
                                        factoryMethod.Name));
                            }

                            var @delegate =
                                (ValueObjectFactory<IPersistableValueObject>) factoryMethod.Invoke(null, null);
                            this.valueObjectFactories.Add(type, @delegate);
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                Resources.DomainFactory_ValueObjectFactoryMethodNotFound.Format(type.Name,
                                    nameof(IPersistableValueObject.Rehydrate)));
                        }
                    }
                }
            }
        }

        private static bool IsWrongNamedOrHasParameters(MethodInfo method)
        {
            return method.Name.NotEqualsOrdinal(nameof(IPersistableEntity.Rehydrate))
                   || method.GetParameters().Length != 0;
        }

        private static bool IsEntity(Type type)
        {
            return typeof(EntityBase).IsAssignableFrom(type);
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
                                          && typeof(EntityBase).IsAssignableFrom(
                                              method.ReturnType.GenericTypeArguments[0])
                                          && method.ReturnType.GetGenericTypeDefinition()
                                              .IsAssignableFrom(typeof(EntityFactory<>)));
        }

        private static bool IsValueObject(Type type)
        {
            return type.IsGenericInterfaceTypeOf(typeof(ValueObjectBase<>));
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
                                              .IsGenericInterfaceTypeOf(typeof(ValueObjectBase<>))
                                          && method.ReturnType.GetGenericTypeDefinition()
                                              .IsAssignableFrom(typeof(ValueObjectFactory<>)));
        }
    }

    public static class ReflectionExtensions
    {
        public static bool IsGenericInterfaceTypeOf(this Type type, Type genericTypeDefinition)
        {
            return type.GetTypeWithGenericTypeDefinitionOf(genericTypeDefinition) != null;
        }
    }
}