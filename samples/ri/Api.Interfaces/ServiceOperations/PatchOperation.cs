using ServiceStack;

namespace Api.Interfaces.ServiceOperations
{
    public abstract class PatchOperation<TResponse> : IReturn<TResponse>, IPatch
    {
    }
}