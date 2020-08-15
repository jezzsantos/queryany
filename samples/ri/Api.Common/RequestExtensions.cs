using Api.Common.Auth;
using Domain.Interfaces;
using ServiceStack.Web;

namespace Api.Common
{
    public static class RequestExtensions
    {
        public static ICurrentCaller ToCaller(this IRequest request)
        {
            return new AnonymousCaller();
        }
    }
}