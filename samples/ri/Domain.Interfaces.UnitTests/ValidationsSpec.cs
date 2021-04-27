using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Domain.Interfaces.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class ValidationsSpec
    {
        [TestMethod]
        public void WhenValidationFormatCtorWithNull_ThenThrows()
        {
            FluentActions.Invoking(() => new ValidationFormat(null))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenMatchesHasFunction_ThenReturnsTrue()
        {
            var validationFormat = new ValidationFormat(x => true);

            var result = validationFormat.Matches("avalue");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenMatchesHasFunction_ThenReturnsFalse()
        {
            var validationFormat = new ValidationFormat(x => false);

            var result = validationFormat.Matches("avalue");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenMatchesHasExpression_ThenReturnsTrue()
        {
            var validationFormat = new ValidationFormat(@"^avalue$");

            var result = validationFormat.Matches("avalue");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenMatchesHasExpressionAndIsNotTooLong_ThenReturnsTrue()
        {
            var validationFormat = new ValidationFormat(@"^a*$", 1, 10);

            var result = validationFormat.Matches("aaaaaaa");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenMatchesHasExpressionAndIsTooLong_ThenReturnsFalse()
        {
            var validationFormat = new ValidationFormat(@"^a*$", 1, 1);

            var result = validationFormat.Matches("aaaaaaa");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenEmailWithNoName_ThenReturnsFalse()
        {
            var result = Validations.Email.Matches("@company.com");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenEmailWithWhitespaceName_ThenReturnsFalse()
        {
            var result = Validations.Email.Matches(" @company.com");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenEmailWithCommonFormat_ThenReturnsTrue()
        {
            var result = Validations.Email.Matches("aname@acompany.com");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenEmailWithMultiLevelDomainFormat_ThenReturnsTrue()
        {
            var result = Validations.Email.Matches("aname@anaustraliancompany.com.au");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenDescriptiveNameWithTooShort_ThenReturnsFalse()
        {
            var result = Validations.DescriptiveName(2, 10).Matches("a");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenDescriptiveNameIsEmpty_ThenReturnsFalse()
        {
            var result = Validations.DescriptiveName().Matches("");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenDescriptiveNameWithTooLong_ThenReturnsFalse()
        {
            var result = Validations.DescriptiveName(2, 10).Matches("atoolongstring");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenDescriptiveNameWithInvalidPrintableChar_ThenReturnsFalse()
        {
            var result = Validations.DescriptiveName(2, 10).Matches("^aninvalidstring");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenDescriptiveNameWithLeastChars_ThenReturnsTrue()
        {
            var result = Validations.DescriptiveName(6, 10).Matches("avalue");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenDescriptiveNameWithMaxChars_ThenReturnsTrue()
        {
            var result = Validations.DescriptiveName(2, 6).Matches("avalue");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenDescriptiveNameWithValidChars_ThenReturnsTrue()
        {
            var result = Validations.DescriptiveName().Matches("avalue");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenTimezoneWithUnknownIana_ThenReturnsFalse()
        {
            var result = Validations.Timezone.Matches("unknown");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenTimezoneWithIanaZone_ThenReturnsTrue()
        {
            var result = Validations.Timezone.Matches("Pacific/Auckland");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenTimezoneWithWindowsZone_ThenReturnsFalse()
        {
            var result = Validations.Timezone.Matches("Central Standard Time");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenFreeFormTextWithTooShort_ThenReturnsFalse()
        {
            var result = Validations.FreeformText(2, 10).Matches("a");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenFreeFormTextWithTooLong_ThenReturnsFalse()
        {
            var result = Validations.FreeformText(2, 10).Matches("atoolongstring");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenFreeFormTextWithInvalidPrintableChar_ThenReturnsFalse()
        {
            var result = Validations.FreeformText(2, 10).Matches("^aninvalidstring");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenFreeFormTextWithLeastChars_ThenReturnsTrue()
        {
            var result = Validations.FreeformText(6, 10).Matches("avalue");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenFreeFormTextWithMaxChars_ThenReturnsTrue()
        {
            var result = Validations.FreeformText(2, 6).Matches("avalue");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenFreeFormTextWithValidChars_ThenReturnsTrue()
        {
            var result = Validations.FreeformText().Matches("avalue");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenFreeFormTextWithMultiLineInWindows_ThenReturnsTrue()
        {
            var result = Validations.FreeformText().Matches("\r\naline1\r\naline2\r\n");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenFreeFormTextWithMultiLineInUnix_ThenReturnsTrue()
        {
            var result = Validations.FreeformText().Matches("\raline1\raline2\r");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenFreeFormTextWithZeroMinAndEmpty_ThenReturnsTrue()
        {
            var result = Validations.FreeformText(0, 10).Matches("");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenAnythingWithTooShort_ThenReturnsFalse()
        {
            var result = Validations.Anything(2, 10).Matches("a");
            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenAnythingWithTooLong_ThenReturnsFalse()
        {
            var result = Validations.Anything(2, 10).Matches("atoolongstring");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenAnythingWithLeastChars_ThenReturnsTrue()
        {
            var result = Validations.Anything(6, 10).Matches("avalue");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenAnythingWithMaxChars_ThenReturnsTrue()
        {
            var result = Validations.Anything(2, 6).Matches("avalue");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenAnythingWithSpecialCharacters_ThenReturnsTrue()
        {
            var result = Validations.Anything().Matches("atext^是⎈𐂯؄💩⚡");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenMarkdownTextWithTooShort_ThenReturnsFalse()
        {
            var result = Validations.Markdown(2, 10).Matches("a");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenMarkdownTextWithTooLong_ThenReturnsFalse()
        {
            var result = Validations.Markdown(2, 10).Matches("atoolongstring");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenMarkdownTextWithInvalidPrintableChar_ThenReturnsFalse()
        {
            var result = Validations.Markdown(2, 10).Matches("^aninvalidstring");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenMarkdownTextWithLeastChars_ThenReturnsTrue()
        {
            var result = Validations.Markdown(6, 10).Matches("avalue");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenMarkdownTextWithMaxChars_ThenReturnsTrue()
        {
            var result = Validations.Markdown(2, 6).Matches("avalue");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenMarkdownTextWithValidChars_ThenReturnsTrue()
        {
            var result = Validations.Markdown().Matches("avalue");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenMarkdownTextWithMultiLineInWindows_ThenReturnsTrue()
        {
            var result = Validations.Markdown().Matches("\r\naline1\r\naline2\r\n");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenMarkdownTextWithMultiLineInUnix_ThenReturnsTrue()
        {
            var result = Validations.Markdown().Matches("\raline1\raline2\r");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenMarkdownTextWithZeroMinAndEmpty_ThenReturnsTrue()
        {
            var result = Validations.Markdown(0, 10).Matches("");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenMarkdownWithValidChars_ThenReturnsTrue()
        {
            var result = Validations.Markdown().Matches("avalue😛");

            result.Should().BeTrue();
        }
    }

    [TestClass, TestCategory("Unit")]
    public class PasswordValidationsSpec
    {
        [TestMethod]
        public void WhenStrictAndTooShort_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("1Short");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenStrictAndTooLong_ThenReturnsFalse()
        {
            var result =
                Validations.Password.Strict.Function("1Password!" + new string('a', Validations.Password.MaxLength));

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenStrictAndJustNumberAndSpecial_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("1234!@#$");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenStrictAndJustNumberAndLower_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("1234abcd");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenStrictAndJustNumberAndUpper_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("1234ABCD");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenStrictAndJustUpperAndLower_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("abcdABCD");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenStrictAndJustUpperAndSpecial_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("ABCD!@#$");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenStrictAndJustLowerAndSpecial_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("abcd!@#$");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenStrictAndJustSpecial_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("!@#$%^&*");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenStrictAndJustNumber_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("12345678");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenStrictAndJustLower_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("abcdefgh");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenStrictAndJustUpper_ThenReturnsFalse()
        {
            var result = Validations.Password.Strict.Function("ABCDEFGH");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenStrictAndHasAllClasses_ThenReturnsTrue()
        {
            var result = Validations.Password.Strict.Function("1Password!");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenStrictAndOmitsNumber_ThenReturnsTrue()
        {
            var result = Validations.Password.Strict.Function("Password!");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenStrictAndOmitsSpecialChar_ThenReturnsTrue()
        {
            var result = Validations.Password.Strict.Function("1Password");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenStrictAndOmitsUppercase_ThenReturnsTrue()
        {
            var result = Validations.Password.Strict.Function("1password!");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenStrictAndOmitsLowercase_ThenReturnsTrue()
        {
            var result = Validations.Password.Strict.Function("1PASSWORD!");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenLooseAndTooShort_ThenReturnsFalse()
        {
            var result = Validations.Password.Loose.Matches("1234");

            result.Should().BeFalse();
        }

        [TestMethod]
        public void WhenLooseAndOnlyNumbers_ThenReturnsTrue()
        {
            var result = Validations.Password.Loose.Matches("12345678");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenLooseAndOnlyLowercase_ThenReturnsTrue()
        {
            var result = Validations.Password.Loose.Matches("apassword");

            result.Should().BeTrue();
        }

        [TestMethod]
        public void WhenLooseAndTooLong_ThenReturnsFalse()
        {
            var result = Validations.Password.Loose.Matches(new string('a', Validations.Password.MaxLength + 1));

            result.Should().BeFalse();
        }
    }
}