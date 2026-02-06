using FluentAssertions;
using IPNoticeHub.Application.DTOs.PdfDTOs;
using IPNoticeHub.Application.LetterComposition.Implementations;
using IPNoticeHub.Application.Templates.Abstractions;
using Moq;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.DateTimeFormats;
using static IPNoticeHub.Shared.Constants.LetterTemplateKeys.TemplateTypeKeys;

namespace IPNoticeHub.Tests.UnitTests.LetterCompositionTests
{
    public class LetterAssemblerTests
    {
        [Test]
        public void RebuildLetterInput_ShouldUseInputBodyTemplate_WhenProvided_AndNotCallTemplateProvider()
        {
            var provider = 
                new Mock<ILetterTemplateProvider>(MockBehavior.Strict);

            var sut = new LetterAssembler(provider.Object);

            var input = new LetterInputDto
            {
                DocumentType = TrademarkCeaseDesistKey,
                DocumentTitle = "Cease & Desist",
                WorkTitle = "Some Work",
                SenderName = "Alice",
                RecipientName = "Bob",
                BodyTemplate = "Hello {{RecipientName}}"
            };

            var result = sut.RebuildLetterInput(input);

            result.BodyTemplate.Should().Be("Hello {{RecipientName}}");

            provider.Verify(
                p => p.GetTemplateByKey(
                    It.IsAny<string>()), 
                Times.Never);
        }

        [Test]
        public void RebuildLetterInput_ShouldFallbackToTemplateProvider_WhenInputBodyTemplateIsNull()
        {
            var preset = new LetterTemplatePreset(
                Type: default,
                Key: TrademarkCeaseDesistKey,
                DisplayName: "CND",
                BodyTemplate: "Template body");

            var provider = 
                new Mock<ILetterTemplateProvider>(MockBehavior.Strict);

            provider.Setup(
                p => p.GetTemplateByKey(TrademarkCeaseDesistKey)).
                Returns(preset);

            var sut = new LetterAssembler(provider.Object);

            var input = new LetterInputDto
            {
                DocumentType = TrademarkCeaseDesistKey,
                DocumentTitle = "Cease & Desist",
                WorkTitle = "Some Work",
                SenderName = "Alice",
                RecipientName = "Bob",
                BodyTemplate = null
            };

            var result = sut.RebuildLetterInput(input);

            result.BodyTemplate.Should().Be("Template body");

            provider.Verify(
                p => p.GetTemplateByKey(TrademarkCeaseDesistKey), Times.Once);
        }

        [Test]
        public void RebuildLetterInput_ShouldUseEmptyString_WhenInputBodyTemplateIsNull_AndProviderReturnsNull()
        {
            var provider = 
                new Mock<ILetterTemplateProvider>(MockBehavior.Strict);

            provider.Setup(
                p => p.GetTemplateByKey(TrademarkCeaseDesistKey)).
                Returns((LetterTemplatePreset?)null);

            var sut = new LetterAssembler(provider.Object);

            var input = new LetterInputDto
            {
                DocumentType = TrademarkCeaseDesistKey,
                DocumentTitle = "Cease & Desist",
                WorkTitle = "Some Work",
                SenderName = "Alice",
                RecipientName = "Bob",
                BodyTemplate = null
            };

            var result = sut.RebuildLetterInput(input);

            result.BodyTemplate.Should().BeEmpty();

            provider.Verify(
                p => p.GetTemplateByKey(TrademarkCeaseDesistKey), Times.Once);
        }

        [Test]
        public void RebuildLetterInput_ShouldUseProvidedLetterDateUtc_WhenPresent()
        {
            var provider = 
                new Mock<ILetterTemplateProvider>(MockBehavior.Strict);

            var sut = new LetterAssembler(provider.Object);

            var date = new DateTime(
                2025, 12, 14, 10, 30, 0, DateTimeKind.Utc);

            var input = new LetterInputDto
            {
                DocumentType = "DMCA-Copyright",
                DocumentTitle = "DMCA",
                WorkTitle = "Work",
                SenderName = "Alice",
                RecipientName = "Bob",
                LetterDateUtc = date,
                BodyTemplate = "Body"
            };

            var result = sut.RebuildLetterInput(input);

            result.DateUtc.Should().Be(date);
            provider.Verify(
                p => p.GetTemplateByKey(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void RebuildLetterInput_ShouldDefaultToUtcNow_WhenLetterDateUtcIsNull()
        {
            var provider = 
                new Mock<ILetterTemplateProvider>(MockBehavior.Strict);

            var sut = new LetterAssembler(provider.Object);

            var before = DateTime.UtcNow;

            var input = new LetterInputDto
            {
                DocumentType = "DMCA-Copyright",
                DocumentTitle = "DMCA",
                WorkTitle = "Work",
                SenderName = "Alice",
                RecipientName = "Bob",
                LetterDateUtc = null,
                BodyTemplate = "Body"
            };

            var result = sut.RebuildLetterInput(input);
            var after = DateTime.UtcNow;

            result.DateUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        }

        [Test]
        public void RebuildLetterInput_ShouldBuildTokens_FromRebuiltDtoFields()
        {
            var provider = 
                new Mock<ILetterTemplateProvider>(MockBehavior.Strict);

            var sut = new LetterAssembler(provider.Object);

            var date = new DateTime(
                2025, 12, 14, 10, 30, 0, DateTimeKind.Utc);

            var input = new LetterInputDto
            {
                DocumentType = TrademarkCeaseDesistKey,
                DocumentTitle = "Cease & Desist",
                WorkTitle = "My Work",
                RegistrationNumber = "123",
                SenderName = "Alice",
                SenderAddress = "Sofia",
                SenderEmail = "a@a.com",
                RecipientName = "Bob",
                RecipientAddress = "Plovdiv",
                RecipientEmail = "b@b.com",
                InfringingUrl = "https://example.com",
                AdditionalFacts = "Facts",
                LetterDateUtc = date,
                BodyTemplate = "Body"
            };

            var result = sut.RebuildLetterInput(input);

            result.Tokens.Should().ContainKey("WorkTitle").
                WhoseValue.Should().Be("My Work");

            result.Tokens.Should().ContainKey("RegistrationNumber").
                WhoseValue.Should().Be("123");

            result.Tokens.Should().ContainKey("SenderName").
                WhoseValue.Should().Be("Alice");

            result.Tokens.Should().ContainKey("RecipientName").
                WhoseValue.Should().Be("Bob");

            result.Tokens.Should().ContainKey("Date")
                .WhoseValue.Should().Be(date.ToString(DefaultDateTimeFormat.DateTimeFormat));
        }
    }
}
