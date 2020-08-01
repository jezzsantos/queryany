using CarsApi.Auth;
using Services.Interfaces;
using ServiceStack.Web;

namespace CarsApi
{
    public static class RequestExtensions
    {
        public static ICurrentCaller ToCaller(this IRequest request)
        {
            return new FakeCaller();
        }
    }
}