using ServiceStack;

namespace Infrastructure.Api.Interfaces.ServiceOperations.Appointments
{
    public class GetAppointmentResponse
    {
        public ResponseStatus ResponseStatus { get; set; }

        public Application.Interfaces.Resources.Appointment Appointment { get; set; }
    }
}