using System;
using ServiceStack;

namespace Infrastructure.Api.Interfaces.ServiceOperations.Appointments
{
    [Route("/appointments/{Id}", "GET")]
    public class GetAppointmentRequest : GetOperationTenanted<GetAppointmentResponse>
    {
        public string Id { get; set; }

    }
}