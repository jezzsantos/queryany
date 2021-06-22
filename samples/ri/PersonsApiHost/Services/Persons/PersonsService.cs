using Api.Common;
using Api.Interfaces.ServiceOperations.Persons;
using Application.Interfaces;
using Common;
using PersonsApplication;
using ServiceStack;

namespace PersonsApiHost.Services.Persons
{
    internal class PersonsService : Service
    {
        private readonly IPersonsApplication personsApplication;

        public PersonsService(IPersonsApplication personsApplication)
        {
            personsApplication.GuardAgainstNull(nameof(personsApplication));

            this.personsApplication = personsApplication;
        }

        public CreatePersonResponse Post(CreatePersonRequest request)
        {
            var person = this.personsApplication.Create(Request.ToCaller(), request.FirstName, request.LastName);
            Response.SetLocation(person);
            return new CreatePersonResponse
            {
                Person = person
            };
        }

        public GetPersonResponse Get(GetPersonRequest request)
        {
            return new GetPersonResponse
            {
                Person = this.personsApplication.Get(Request.ToCaller(), request.Id, request.ToGetOptions())
            };
        }
    }
}