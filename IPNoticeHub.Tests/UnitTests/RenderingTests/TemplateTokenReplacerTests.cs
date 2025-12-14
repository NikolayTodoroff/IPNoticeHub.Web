using FluentAssertions;
using IPNoticeHub.Infrastructure.Rendering.Implementation;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RenderingTests
{
    public class TemplateTokenReplacerTests
    {
        [Test]
        public void ReplaceTemplate_WhenTemplateIsNull_ReturnsEmptyString()
        {
            var sut = new RegexTemplateTokenReplacer();

            var result = sut.ReplaceTemplate(null!, new Dictionary<string, string>());

            result.Should().BeEmpty();
        }

        [Test]
        public void ReplaceTemplate_WhenTokenExists_ReplacesIt()
        {
            var sut = new RegexTemplateTokenReplacer();

            var tokens = new Dictionary<string, string>
            {
                ["Name"] = "Nikolay"
            };

            var result = sut.ReplaceTemplate("Hello {{Name}}!", tokens);

            result.Should().Be("Hello Nikolay!");
        }

        [Test]
        public void ReplaceTemplate_AllowsWhitespaceInsideBraces()
        {
            var sut = new RegexTemplateTokenReplacer();

            var tokens = new Dictionary<string, string>
            {
                ["Name"] = "Nikolay"
            };

            var result = sut.ReplaceTemplate("Hello {{   Name   }}!", tokens);

            result.Should().Be("Hello Nikolay!");
        }

        [Test]
        public void ReplaceTemplate_WhenTokenMissing_LeavesPlaceholderUnchanged()
        {
            var sut = new RegexTemplateTokenReplacer();

            var result = sut.ReplaceTemplate("Hello {{Missing}}!", new Dictionary<string, string>());

            result.Should().Be("Hello {{Missing}}!");
        }

        [Test]
        public void ReplaceTemplate_ReplacesMultipleTokens()
        {
            var sut = new RegexTemplateTokenReplacer();

            var tokens = new Dictionary<string, string>
            {
                ["First"] = "A",
                ["Second"] = "B"
            };

            var result = sut.ReplaceTemplate("{{First}}-{{Second}}", tokens);

            result.Should().Be("A-B");
        }

        [Test]
        public void ReplaceTemplate_DoesNotReplaceNonWordTokenNames()
        {
            var sut = new RegexTemplateTokenReplacer();

            var tokens = new Dictionary<string, string>
            {
                ["First-Name"] = "ShouldNotApply"
            };

            var result = sut.ReplaceTemplate("Hello {{First-Name}}!", tokens);

            result.Should().Be("Hello {{First-Name}}!");
        }

        [Test]
        public void ReplaceTemplate_WhenTokenValueIsEmpty_ReplacesWithEmptyString()
        {
            var sut = new RegexTemplateTokenReplacer();

            var tokens = new Dictionary<string, string>
            {
                ["Name"] = ""
            };

            var result = sut.ReplaceTemplate("Hello {{Name}}!", tokens);

            result.Should().Be("Hello !");
        }
    }
}
