using ServiceStack;

namespace Api.Interfaces.ServiceOperations
{
    public abstract class GetOperation<TResponse> : IReturn<TResponse>, IHasGetOptions, IGet
    {
        public string Embed { get; set; }
    }
}