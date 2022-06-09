using ServiceStack;

namespace Infrastructure.Api.Interfaces.ServiceOperations.Appointments
{
    public class CreateAppointmentResponse
    {
        public ResponseStatus ResponseStatus { get; set; }

        public Application.Interfaces.Resources.Appointment Appointment { get; set; }
    }
}