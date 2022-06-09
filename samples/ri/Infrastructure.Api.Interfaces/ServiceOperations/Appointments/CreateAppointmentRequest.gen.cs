using System;
using ServiceStack;

namespace Infrastructure.Api.Interfaces.ServiceOperations.Appointments
{
    [Route("/appointments", "POST")]
    public class CreateAppointmentRequest : PostOperationTenanted<CreateAppointmentResponse>
    {
        public string Title { get; set; }

        public DateTime StartUtc { get; set; }

    }
}