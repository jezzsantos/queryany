using ServiceStack;

namespace Services.Interfaces.Apis
{
    [Route("/cars/available", "GET")]
    public class GetAvailableCars : IReturn<GetAvailableCarsResponse>, IGet
    {
    }
}