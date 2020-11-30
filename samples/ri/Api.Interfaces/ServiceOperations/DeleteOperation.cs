using ServiceStack;

namespace Api.Interfaces.ServiceOperations
{
    public abstract class DeleteOperation<TResponse> : IReturn<TResponse>, IDelete
    {
    }
}