using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Common.UnitTests
{
    [Trait("Category", "Unit")]
    public class StringExtensionsSpec
    {
        [Fact]
        public void WhenIsOneOfWithNullSource_ThenThrows()
        {
            FluentActions.Invoking(() => ((string) null).IsOneOf("avalue"))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenIsOneOfWithEmptyCollection_ThenReturnsFalse()
        {
            var result = "avalue".IsOneOf();

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsOneOfAndIncluded_ThenReturnsTrue()
        {
            var result = "avalue".IsOneOf("avalue", "anothervalue");

            result.Should().BeTrue();
        }

        [Fact]
        public void WhenIsOneOfAndExcluded_ThenReturnsFalse()
        {
            var result = "avalue".IsOneOf("anothervalue1", "anothervalue2");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenIsOneOfAndIncludedAndWrongCase_ThenReturnsFalse()
        {
            var result = "AVALUE".IsOneOf("avalue", "anothervalue");

            result.Should().BeFalse();
        }

        [Fact]
        public void WhenSplitParagraphsAndValueIsNull_ThenReturnsEmptyArray()
        {
            var result = ((string) null).SplitParagraphs();

            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenSplitParagraphsAndValueIsEmpty_ThenReturnsEmptyArray()
        {
            var result = string.Empty.SplitParagraphs();

            result.Should().BeEmpty();
        }

        [Fact]
        public void WhenSplitParagraphsAndValueContainsSingleLine_ThenReturnsEmptyArray()
        {
            var result = "avalue".SplitParagraphs();

            result.Single().Should().Be("avalue");
        }

        [Fact]
        public void WhenSplitParagraphsAndValueContainsMultipleLinesWithWindowsLineBreaks_ThenReturnsEmptyArray()
        {
            var result = "\r\navalue1\r\navalue2\r\n".SplitParagraphs();

            result.Count().Should().Be(2);
            result[0].Should().Be("avalue1");
            result[1].Should().Be("avalue2");
        }

        [Fact]
        public void WhenSplitParagraphsAndValueContainsMultipleLinesWithUnixLineBreaks_ThenReturnsEmptyArray()
        {
            var result = "\ravalue1\ravalue2\r".SplitParagraphs();

            result.Count().Should().Be(2);
            result[0].Should().Be("avalue1");
            result[1].Should().Be("avalue2");
        }

        [Fact]
        public void WhenSplitParagraphsAndValueContainsMultipleLinesWithUnusualLineBreaks_ThenReturnsEmptyArray()
        {
            var result = "\navalue1\navalue2\n".SplitParagraphs();

            result.Count().Should().Be(2);
            result[0].Should().Be("avalue1");
            result[1].Should().Be("avalue2");
        }
    }
}