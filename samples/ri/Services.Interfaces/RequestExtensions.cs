using ServiceStack.Web;

namespace Services.Interfaces
{
    public static class RequestExtensions
    {
        public static ICurrentCaller ToCaller(this IRequest request)
        {
            return new FakeCaller();
        }
    }
}