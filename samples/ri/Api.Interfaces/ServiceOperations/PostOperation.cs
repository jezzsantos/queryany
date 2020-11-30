using ServiceStack;

namespace Api.Interfaces.ServiceOperations
{
    public abstract class PostOperation<TResponse> : IReturn<TResponse>, IPost
    {
    }
}