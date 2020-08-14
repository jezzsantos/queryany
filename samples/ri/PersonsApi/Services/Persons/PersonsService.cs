using Api.Common;
using Api.Interfaces;
using Api.Interfaces.ServiceOperations;
using PersonsApplication;
using QueryAny.Primitives;
using ServiceStack;

namespace PersonsApi.Services.Persons
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
            return new CreatePersonResponse
            {
                Person = this.personsApplication.Create(Request.ToCaller(), request.FirstName, request.LastName)
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