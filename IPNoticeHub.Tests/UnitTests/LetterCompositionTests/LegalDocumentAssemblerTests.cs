using FluentAssertions;
using IPNoticeHub.Application.LetterComposition.Implementations;
using IPNoticeHub.Application.Templates.Abstractions;
using IPNoticeHub.Domain.Entities.LegalDocuments;
using IPNoticeHub.Shared.Enums;
using Moq;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace IPNoticeHub.Tests.UnitTests.LetterCompositionTests
{
    public class LegalDocumentAssemblerTests
    {
        [Test]
        public void RebuildFromSavedDocument_ShouldCallTemplateProvider_WithGeneratedKey_SourceTypeAndTemplateType()
        {
            var provider = 
                new Mock<ILetterTemplateProvider>(MockBehavior.Strict);

            provider.Setup(
                p => p.GetTemplateByKey("Trademark:CeaseAndDesist"))
                .Returns(new LetterTemplatePreset(
                    default,
                    "Trademark:CeaseAndDesist", 
                    "x", 
                    "Template body"));

            var sut = new LegalDocumentAssembler(provider.Object);

            var doc = new LegalDocument
            {
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                DocumentTitle = "Title",
                IpTitle = "Nike",
                RegistrationNumber = "123",
                SenderName = "Alice",
                RecipientName = "Bob",
                LetterDate = new DateTime(
                    2025, 12, 14, 10, 30, 0, DateTimeKind.Utc),
            };

            _ = sut.RebuildFromSavedDocument(doc);

            provider.Verify(
                p => p.GetTemplateByKey("Trademark:CeaseAndDesist"), 
                Times.Once);
        }

        [Test]
        public void RebuildFromSavedDocument_ShouldPreferDocumentBodyTemplate_WhenPresent()
        {
            var provider = 
                new Mock<ILetterTemplateProvider>(MockBehavior.Strict);

            provider.Setup(
                p => p.GetTemplateByKey(It.IsAny<string>())).
                Returns(new LetterTemplatePreset(
                    default, 
                    "x", 
                    "x", 
                    "Template body"));

            var sut = new LegalDocumentAssembler(provider.Object);

            var doc = new LegalDocument
            {
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                DocumentTitle = "Doc Title",
                IpTitle = "IP",
                BodyTemplate = "Document body wins",
                SenderName = "Alice",
                RecipientName = "Bob",
                LetterDate = new DateTime(2025, 12, 14, 10, 30, 0, DateTimeKind.Utc),
            };

            var result = sut.RebuildFromSavedDocument(doc);

            result.BodyTemplate.Should().Be("Document body wins");

            provider.Verify(
                p => p.GetTemplateByKey("Trademark:CeaseAndDesist"), 
                Times.Once);
        }

        [Test]
        public void RebuildFromSavedDocument_ShouldFallbackToPresetBodyTemplate_WhenDocumentBodyTemplateIsNull()
        {
            var provider = 
                new Mock<ILetterTemplateProvider>(MockBehavior.Strict);

            provider.Setup(
                p => p.GetTemplateByKey("Trademark:CeaseAndDesist")).
                Returns(new LetterTemplatePreset(
                    default, 
                    "Trademark:CeaseAndDesist", 
                    "x", 
                    "Preset body"));

            var sut = new LegalDocumentAssembler(provider.Object);

            var doc = new LegalDocument
            {
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                DocumentTitle = "Doc Title",
                IpTitle = "IP",
                BodyTemplate = "Preset body",
                SenderName = "Alice",
                RecipientName = "Bob",
                LetterDate = new DateTime(
                    2025, 12, 14, 10, 30, 0, DateTimeKind.Utc),
            };

            var result = sut.RebuildFromSavedDocument(doc);

            result.BodyTemplate.Should().Be("Preset body");

            provider.Verify(
                p => p.GetTemplateByKey("Trademark:CeaseAndDesist"), 
                Times.Once);
        }

        [Test]
        public void RebuildFromSavedDocument_ShouldUseEmptyString_WhenDocumentBodyTemplateNull_AndPresetNull()
        {
            var provider = 
                new Mock<ILetterTemplateProvider>(MockBehavior.Strict);

            provider.Setup(
                p => p.GetTemplateByKey("Trademark:CeaseAndDesist")).
                Returns((LetterTemplatePreset?)null);

            var sut = new LegalDocumentAssembler(provider.Object);

            var doc = new LegalDocument
            {
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                DocumentTitle = "Doc Title",
                IpTitle = "IP",
                BodyTemplate = null,
                SenderName = "Alice",
                RecipientName = "Bob",
                LetterDate = new DateTime(
                    2025, 12, 14, 10, 30, 0, DateTimeKind.Utc),
            };

            var result = sut.RebuildFromSavedDocument(doc);

            result.BodyTemplate.Should().BeEmpty();

            provider.Verify(
                p => p.GetTemplateByKey("Trademark:CeaseAndDesist"), 
                Times.Once);
        }

        [Test]
        public void RebuildFromSavedDocument_ShouldMapFields_AndBuildTokens()
        {
            var provider = 
                new Mock<ILetterTemplateProvider>(MockBehavior.Strict);

            provider.Setup(
                p => p.GetTemplateByKey("Trademark:CeaseAndDesist")).
                Returns(new LetterTemplatePreset(
                    default, 
                    "Trademark:CeaseAndDesist", 
                    "x", 
                    "Preset body"));

            var sut = new LegalDocumentAssembler(provider.Object);

            var date = new DateTime(
                2025, 12, 14, 10, 30, 0, DateTimeKind.Utc);

            var doc = new LegalDocument
            {
                SourceType = DocumentSourceType.Trademark,
                TemplateType = LetterTemplateType.CeaseAndDesist,
                DocumentTitle = "Cease & Desist",
                IpTitle = null,
                RegistrationNumber = "REG-777",

                SenderName = "Alice",
                SenderEmail = "a@a.com",
                SenderAddress = "Sofia",

                RecipientName = "Bob Inc.",
                RecipientEmail = "b@b.com",
                RecipientAddress = "Plovdiv",

                InfringingUrl = "https://example.com",
                AdditionalFacts = "Facts here",

                BodyTemplate = "Preset body",
                LetterDate = date,

                YearOfCreation = 2020,
                DateOfPublication = new DateTime(2020, 1, 1),
                NationOfFirstPublication = "BG",
                GoodFaithStatement = "I swear this is true."
            };

            var result = sut.RebuildFromSavedDocument(doc);
          
            var expectedDocType = GetDisplayNameOrToString(doc.SourceType);

            result.DocumentType.Should().Be(expectedDocType);
            result.DocumentTitle.Should().Be("Cease & Desist");
            result.WorkTitle.Should().Be("");
            result.RegistrationNumber.Should().Be("REG-777");

            result.SenderName.Should().Be("Alice");
            result.SenderEmail.Should().Be("a@a.com");
            result.SenderAddress.Should().Be("Sofia");

            result.RecipientName.Should().Be("Bob Inc.");
            result.RecipientEmail.Should().Be("b@b.com");
            result.RecipientAddress.Should().Be("Plovdiv");

            result.InfringingUrl.Should().Be("https://example.com");
            result.AdditionalFacts.Should().Be("Facts here");
            result.BodyTemplate.Should().Be("Preset body");
            result.DateUtc.Should().Be(date);

            result.YearOfCreation.Should().Be(2020);
            result.DateOfPublication.Should().Be(new DateTime(2020,01,01));
            result.NationOfFirstPublication.Should().Be("BG");
            result.GoodFaithStatement.Should().Be("I swear this is true.");
            result.Tokens.Should().NotBeNull();

            result.Tokens.Should().ContainKey("SenderName").
                WhoseValue.Should().Be("Alice");

            result.Tokens.Should().ContainKey("RecipientName").
                WhoseValue.Should().Be("Bob Inc.");

            result.Tokens.Should().ContainKey("RegistrationNumber").
                WhoseValue.Should().Be("REG-777");
        }

        private static string GetDisplayNameOrToString(Enum value)
        {
            var member = value.GetType().GetMember(value.ToString())[0];
            var display = member.GetCustomAttribute<DisplayAttribute>();
            return display?.Name ?? value.ToString();
        }
    }
}
