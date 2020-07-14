using System;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions.Execution;
using FluentAssertions.Specialized;
using QueryAny.Primitives;

// ReSharper disable once CheckNamespace
public static class ExceptionAssertionExtensions
{
    public static ExceptionAssertions<TException> WithMessageLike<TException>(
        this ExceptionAssertions<TException> @throw, string messageWithFormatters, string because = "",
        params object[] becauseArgs) where TException : Exception
    {
        if (messageWithFormatters.HasValue())
        {
            var exception = @throw.Subject.Single();
            var expectedFormat = messageWithFormatters.Replace("{", "{{").Replace("}", "}}");
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .UsingLineBreaks
                .ForCondition(IsFormattedFrom(exception.Message, messageWithFormatters))
                .FailWith(
                    $"Expected exception message to match the equivalent of\n\"{expectedFormat}\", but\n\"{exception.Message}\" does not.")
                ;
        }

        return new ExceptionAssertions<TException>(@throw.Subject);
    }

    private static bool IsFormattedFrom(string actualExceptionMessage, string expectedMessageWithFormatters)
    {
        var escapedPattern = expectedMessageWithFormatters
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace(".", "\\.")
            .Replace("<", "\\<")
            .Replace(">", "\\>");

        var pattern = Regex.Replace(escapedPattern, @"\{\d+\}", ".*")
            .Replace(" ", @"\s");

        return new Regex(pattern).IsMatch(actualExceptionMessage);
    }
}