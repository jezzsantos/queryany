using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using FluentAssertions.Execution;
using FluentAssertions.Specialized;
using ServiceStack;

// ReSharper disable once CheckNamespace
namespace IntegrationTesting.Common
{
    [ExcludeFromCodeCoverage]
    public static class WebServiceExceptionAssertionExtensions
    {
        public static ExceptionAssertions<WebServiceException> WithStatusCode(
            this ExceptionAssertions<WebServiceException> @throw, HttpStatusCode statusCode, string because = "",
            params object[] becauseArgs)
        {
            var exception = @throw.Subject.Single();

            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(exception != null)
                .FailWith("Expected {context} to throw {0}{reason}, but found <null>.", typeof(WebServiceException))
                .Then
                .Given(() => exception.GetStatus().GetValueOrDefault(HttpStatusCode.OK))
                .ForCondition(thrownStatusCode => thrownStatusCode == statusCode)
                .FailWith("Expected {context} to throw {0}{reason} with {1}, but was {2}.",
                    _ => typeof(WebServiceException),
                    _ => statusCode, thrownStatusCode => thrownStatusCode);

            return new ExceptionAssertions<WebServiceException>(@throw.Subject);
        }
    }
}