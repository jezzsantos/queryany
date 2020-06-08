using ServiceStack;

namespace Services.Interfaces.Apis
{
    public abstract class GetOperation<TResponse> : IReturn<TResponse>, IGet
    {
        public string Embed { get; set; }
    }
}