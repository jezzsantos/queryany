using System;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using PersonsApplication.ReadModels;
using PersonsDomain;
using QueryAny.Primitives;
using Storage;
using Storage.Interfaces.ReadModels;

namespace PersonsStorage
{
    public class PersonEntityReadModelProjection : IReadModelProjection
    {
        private readonly ILogger logger;
        private readonly IReadModelStorage<Person> personStorage;

        public PersonEntityReadModelProjection(ILogger logger, IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            repository.GuardAgainstNull(nameof(repository));

            this.logger = logger;
            this.personStorage = new GeneralReadModelStorage<Person>(logger, repository);
        }

        public Type EntityType => typeof(PersonEntity);

        public bool Project(IChangeEvent originalEvent)
        {
            switch (originalEvent)
            {
                case Events.Person.Created e:
                    this.personStorage.Create(e.EntityId.ToIdentifier());
                    break;

                case Events.Person.EmailChanged e:
                    this.personStorage.Update(e.EntityId, dto => { dto.Email = e.EmailAddress; });
                    break;

                case Events.Person.PhoneNumberChanged e:
                    this.personStorage.Update(e.EntityId, dto => { dto.Phone = e.PhoneNumber; });
                    break;
                case Events.Person.NameChanged e:
                    this.personStorage.Update(e.EntityId, dto =>
                    {
                        dto.FirstName = e.FirstName;
                        dto.LastName = e.LastName;
                    });
                    break;

                case Events.Person.DisplayNameChanged e:
                    this.personStorage.Update(e.EntityId, dto => { dto.DisplayName = e.DisplayName; });
                    break;

                default:
                    this.logger.LogDebug($"Unknown entity type '{originalEvent.GetType().Name}'");
                    return false;
            }

            return true;
        }
    }
}