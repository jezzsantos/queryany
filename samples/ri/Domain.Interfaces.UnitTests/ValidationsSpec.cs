using System;
using FluentAssertions;
using Xunit;

namespace Domain.Interfaces.UnitTests
{
    [Trait("Category", "Unit")]
    public class ValidationsSpec
    {
        [Fact]
        public void WhenValidationFormatCtorWithNull_ThenThrows()
        {
            FluentActions.Invoking(() => new ValidationFormat(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenMatchesHasFunction_ThenReturnsTrue()
        {
            var validationFormat = new ValidationFormat(x => true);

            var result = validationFormat.Matches("avalue");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenMatchesHasFunction_ThenReturnsFalse()
        {
            var validationFormat = new ValidationFormat(x => false);

            var result = validationFormat.Matches("avalue");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenMatchesHasExpression_ThenReturnsTrue()
        {
            var validationFormat = new ValidationFormat(@"^avalue$");

            var result = validationFormat.Matches("avalue");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenMatchesHasExpressionAndIsNotTooLong_ThenReturnsTrue()
        {
            var validationFormat = new ValidationFormat(@"^a*$", 1, 10);

            var result = validationFormat.Matches("aaaaaaa");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenMatchesHasExpressionAndIsTooLong_ThenReturnsFalse()
        {
            var validationFormat = new ValidationFormat(@"^a*$", 1, 1);

            var result = validationFormat.Matches("aaaaaaa");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenEmailWithNoName_ThenReturnsFalse()
        {
            var result = Validations.Email.Matches("@company.com");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenEmailWithWhitespaceName_ThenReturnsFalse()
        {
            var result = Validations.Email.Matches(" @company.com");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenEmailWithCommonFormat_ThenReturnsTrue()
        {
            var result = Validations.Email.Matches("aname@acompany.com");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenEmailWithMultiLevelDomainFormat_ThenReturnsTrue()
        {
            var result = Validations.Email.Matches("aname@anaustraliancompany.com.au");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenDescriptiveNameWithTooShort_ThenReturnsFalse()
        {
            var result = Validations.DescriptiveName(2, 10).Matches("a");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenDescriptiveNameIsEmpty_ThenReturnsFalse()
        {
            var result = Validations.DescriptiveName().Matches("");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenDescriptiveNameWithTooLong_ThenReturnsFalse()
        {
            var result = Validations.DescriptiveName(2, 10).Matches("atoolongstring");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenDescriptiveNameWithInvalidPrintableChar_ThenReturnsFalse()
        {
            var result = Validations.DescriptiveName(2, 10).Matches("^aninvalidstring");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenDescriptiveNameWithLeastChars_ThenReturnsTrue()
        {
            var result = Validations.DescriptiveName(6, 10).Matches("avalue");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenDescriptiveNameWithMaxChars_ThenReturnsTrue()
        {
            var result = Validations.DescriptiveName(2, 6).Matches("avalue");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenDescriptiveNameWithValidChars_ThenReturnsTrue()
        {
            var result = Validations.DescriptiveName().Matches("avalue");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenTimezoneWithUnknownIana_ThenReturnsFalse()
        {
            var result = Validations.Timezone.Matches("unknown");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenTimezoneWithIanaZone_ThenReturnsTrue()
        {
            var result = Validations.Timezone.Matches("Pacific/Auckland");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenTimezoneWithWindowsZone_ThenReturnsFalse()
        {
            var result = Validations.Timezone.Matches("Central Standard Time");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenFreeFormTextWithTooShort_ThenReturnsFalse()
        {
            var result = Validations.FreeformText(2, 10).Matches("a");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenFreeFormTextWithTooLong_ThenReturnsFalse()
        {
            var result = Validations.FreeformText(2, 10).Matches("atoolongstring");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenFreeFormTextWithInvalidPrintableChar_ThenReturnsFalse()
        {
            var result = Validations.FreeformText(2, 10).Matches("^aninvalidstring");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenFreeFormTextWithLeastChars_ThenReturnsTrue()
        {
            var result = Validations.FreeformText(6, 10).Matches("avalue");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenFreeFormTextWithMaxChars_ThenReturnsTrue()
        {
            var result = Validations.FreeformText(2, 6).Matches("avalue");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenFreeFormTextWithValidChars_ThenReturnsTrue()
        {
            var result = Validations.FreeformText().Matches("avalue");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenFreeFormTextWithMultiLineInWindows_ThenReturnsTrue()
        {
            var result = Validations.FreeformText().Matches("\r\naline1\r\naline2\r\n");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenFreeFormTextWithMultiLineInUnix_ThenReturnsTrue()
        {
            var result = Validations.FreeformText().Matches("\raline1\raline2\r");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenFreeFormTextWithZeroMinAndEmpty_ThenReturnsTrue()
        {
            var result = Validations.FreeformText(0, 10).Matches("");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenAnythingWithTooShort_ThenReturnsFalse()
        {
            var result = Validations.Anything(2, 10).Matches("a");
            result.Should().BeFalse();
        }

        [Fact]
        public void WhenAnythingWithTooLong_ThenReturnsFalse()
        {
            var result = Validations.Anything(2, 10).Matches("atoolongstring");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenAnythingWithLeastChars_ThenReturnsTrue()
        {
            var result = Validations.Anything(6, 10).Matches("avalue");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenAnythingWithMaxChars_ThenReturnsTrue()
        {
            var result = Validations.Anything(2, 6).Matches("avalue");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenAnythingWithSpecialCharacters_ThenReturnsTrue()
        {
            var result = Validations.Anything().Matches("atext^是⎈𐂯؄💩⚡");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenMarkdownTextWithTooShort_ThenReturnsFalse()
        {
            var result = Validations.Markdown(2, 10).Matches("a");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenMarkdownTextWithTooLong_ThenReturnsFalse()
        {
            var result = Validations.Markdown(2, 10).Matches("atoolongstring");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenMarkdownTextWithInvalidPrintableChar_ThenReturnsFalse()
        {
            var result = Validations.Markdown(2, 10).Matches("^aninvalidstring");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenMarkdownTextWithLeastChars_ThenReturnsTrue()
        {
            var result = Validations.Markdown(6, 10).Matches("avalue");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenMarkdownTextWithMaxChars_ThenReturnsTrue()
        {
            var result = Validations.Markdown(2, 6).Matches("avalue");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenMarkdownTextWithValidChars_ThenReturnsTrue()
        {
            var result = Validations.Markdown().Matches("avalue");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenMarkdownTextWithMultiLineInWindows_ThenReturnsTrue()
        {
            var result = Validations.Markdown().Matches("\r\naline1\r\naline2\r\n");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenMarkdownTextWithMultiLineInUnix_ThenReturnsTrue()
        {
            var result = Validations.Markdown().Matches("\raline1\raline2\r");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenMarkdownTextWithZeroMinAndEmpty_ThenReturnsTrue()
        {
            var result = Validations.Markdown(0, 10).Matches("");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenMarkdownWithValidChars_ThenReturnsTrue()
        {
            var result = Validations.Markdown().Matches("avalue😛");

            result.Should().BeTrue();
        }
    }

    [Trait("Category", "Unit")]
    public class PasswordValidationsSpec
    {
        [Fact]
        public void WhenStrictAndTooShort_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("1Short");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenStrictAndTooLong_ThenReturnsFalse()
        {
            var result =
                Validations.Password.Strict.Function("1Password!" + new string('a', Validations.Password.MaxLength));

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenStrictAndJustNumberAndSpecial_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("1234!@#$");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenStrictAndJustNumberAndLower_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("1234abcd");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenStrictAndJustNumberAndUpper_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("1234ABCD");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenStrictAndJustUpperAndLower_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("abcdABCD");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenStrictAndJustUpperAndSpecial_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("ABCD!@#$");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenStrictAndJustLowerAndSpecial_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("abcd!@#$");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenStrictAndJustSpecial_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("!@#$%^&*");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenStrictAndJustNumber_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("12345678");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenStrictAndJustLower_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("abcdefgh");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenStrictAndJustUpper_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("ABCDEFGH");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenStrictAndHasAllClasses_ThenReturnsTrue()
        {
            var result = Validations.Password.Strict.Function("1Password!");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenStrictAndOmitsNumber_ThenReturnsTrue()
        {
            var result = Validations.Password.Strict.Function("Password!");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenStrictAndOmitsSpecialChar_ThenReturnsTrue()
        {
            var result = Validations.Password.Strict.Function("1Password");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenStrictAndOmitsUppercase_ThenReturnsTrue()
        {
            var result = Validations.Password.Strict.Function("1password!");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenStrictAndOmitsLowercase_ThenReturnsTrue()
        {
            var result = Validations.Password.Strict.Function("1PASSWORD!");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenLooseAndTooShort_ThenReturnsFalse()
        {
            var result = Validations.Password.Loose.Matches("1234");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenLooseAndOnlyNumbers_ThenReturnsTrue()
        {
            var result = Validations.Password.Loose.Matches("12345678");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenLooseAndOnlyLowercase_ThenReturnsTrue()
        {
            var result = Validations.Password.Loose.Matches("apassword");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenLooseAndTooLong_ThenReturnsFalse()
        {
            var result = Validations.Password.Loose.Matches(new string('a', Validations.Password.MaxLength + 1));

            result.Should().BeFalse();
        }
    }
}