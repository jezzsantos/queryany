using CarsApi.Auth;
using Domain.Interfaces;
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