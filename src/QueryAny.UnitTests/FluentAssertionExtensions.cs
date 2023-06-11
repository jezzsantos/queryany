﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using FluentAssertions.Specialized;

namespace QueryAny.UnitTests
{
    // ReSharper disable once CheckNamespace
    [ExcludeFromCodeCoverage]
    public static class ExceptionAssertionExtensions
    {
        public static ExceptionAssertions<TException> WithMessageLike<TException>(
            this ExceptionAssertions<TException> @throw, string messageWithFormatters, string because = "",
            params object[] becauseArgs) where TException : Exception
        {
            if (!string.IsNullOrEmpty(messageWithFormatters))
            {
                var exception = @throw.Subject.Single();
                var expectedFormat = messageWithFormatters.Replace("{", "{{").Replace("}", "}}");
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .ForCondition(IsFormattedFrom(exception.Message, messageWithFormatters))
                    .UsingLineBreaks
                    .FailWith(
                        $"Expected exception message to match the equivalent of\n\"{expectedFormat}\", but\n\"{exception.Message}\" does not.")
                    ;
            }

            return new ExceptionAssertions<TException>(@throw.Subject);
        }

        internal static bool IsFormattedFrom(string actualExceptionMessage, string expectedMessageWithFormatters)
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

    [ExcludeFromCodeCoverage]
    public static class DateTimeAssertionExtensions
    {
        public static AndConstraint<DateTimeAssertions> BeNear(this DateTimeAssertions assertions,
            DateTime nearbyTime,
            int precision = 850,
            string because = "",
            params object[] becauseArgs)
        {
            return assertions.BeCloseTo(nearbyTime, TimeSpan.FromMilliseconds(precision), because, becauseArgs);
        }
    }
}