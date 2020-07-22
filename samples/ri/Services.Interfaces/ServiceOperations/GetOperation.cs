using ServiceStack;

namespace Services.Interfaces.ServiceOperations
{
    public abstract class GetOperation<TResponse> : IReturn<TResponse>, IGet
    {
        public string Embed { get; set; }
    }
}