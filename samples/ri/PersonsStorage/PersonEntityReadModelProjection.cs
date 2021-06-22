using System;
using Common;
using Domain.Interfaces.Entities;
using PersonsApplication.ReadModels;
using PersonsDomain;
using Storage;
using Storage.Interfaces.ReadModels;

namespace PersonsStorage
{
    public class PersonEntityReadModelProjection : IReadModelProjection
    {
        private readonly IReadModelStorage<Person> personStorage;
        private readonly IRecorder recorder;

        public PersonEntityReadModelProjection(IRecorder recorder, IRepository repository)
        {
            recorder.GuardAgainstNull(nameof(recorder));
            repository.GuardAgainstNull(nameof(repository));

            this.recorder = recorder;
            this.personStorage = new GeneralReadModelStorage<Person>(recorder, repository);
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
                    this.recorder.TraceDebug($"Unknown entity type '{originalEvent.GetType().Name}'");
                    return false;
            }

            return true;
        }
    }
}