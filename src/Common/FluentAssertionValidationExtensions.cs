using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions.Execution;
using FluentAssertions.Specialized;
using ServiceStack.FluentValidation;

// ReSharper disable once CheckNamespace
[ExcludeFromCodeCoverage]
public static class ValidationExceptionAssertionExtensions
{
    public static ExceptionAssertions<TException> WithValidationMessageLike<TException>(
        this ExceptionAssertions<TException> @throw, string messageWithFormatters, string because = "",
        params object[] becauseArgs) where TException : Exception
    {
        var exception = @throw.Subject.Single();
        var expectedFormat = messageWithFormatters.Replace("{", "{{").Replace("}", "}}");
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .UsingLineBreaks
            .ForCondition(IsValidationException(exception) &&
                          ExceptionAssertionExtensions.IsFormattedFrom(exception.Message, messageWithFormatters))
            .FailWith(
                $"Expected validation exception message to match the equivalent of\n\"{expectedFormat}\", but\n\"{exception.Message}\" does not.")
            ;

        return new ExceptionAssertions<TException>(@throw.Subject);
    }

    public static ExceptionAssertions<TException> WithValidationMessageForNotEmpty<TException>(
        this ExceptionAssertions<TException> @throw, string because = "",
        params object[] becauseArgs) where TException : Exception
    {
        var exception = @throw.Subject.Single();
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .UsingLineBreaks
            .ForCondition(IsValidationExceptionForMessage(exception, " must not be empty."))
            .FailWith(
                $"Expected validation exception message to be\n\"\", but\n\"{exception.Message}\" was not.")
            ;

        return new ExceptionAssertions<TException>(@throw.Subject);
    }

    private static bool IsValidationException(Exception exception)
    {
        return exception is ValidationException && exception.Message.StartsWith("Validation failed: \r\n");
    }

    private static bool IsValidationExceptionForMessage(Exception exception, string expectedMessage)
    {
        if (!(exception is ValidationException))
        {
            return false;
        }

        return exception.Message.StartsWith("Validation failed: \r\n") && exception.Message.EndsWith(expectedMessage);
    }
}