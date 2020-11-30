using ServiceStack;

namespace Api.Interfaces.ServiceOperations
{
    public abstract class PutOperation<TResponse> : IReturn<TResponse>, IPut
    {
    }
}