using FluentAssertions;
using IPNoticeHub.Application.DTOs.PdfDTOs;
using IPNoticeHub.Application.LetterComposition.TokenBuilder;
using NUnit.Framework;
using static IPNoticeHub.Shared.Constants.DateTimeFormats;

namespace IPNoticeHub.Tests.UnitTests.LetterCompositionTests
{
    public class LetterTokenBuilderTests
    {
        [Test]
        public void BuildTokens_ShouldIncludeRequiredTokens_AndFormatDate()
        {
            var dto = new PdfLetterDto
            {
                WorkTitle = "My Work",
                RegistrationNumber = "123",
                DateUtc = new DateTime(
                    2025, 12, 14, 10, 30, 0, DateTimeKind.Utc),

                SenderName = "Alice",
                SenderAddress = "Sofia",
                SenderEmail = "a@a.com",

                RecipientName = "Bob",
                RecipientAddress = "Plovdiv",
                RecipientEmail = "b@b.com",

                InfringingUrl = "https://example.com",
                AdditionalFacts = "Facts"
            };

            var tokens = LetterTokenBuilder.BuildTokens(dto);

            tokens.Should().ContainKey("WorkTitle").
                WhoseValue.Should().Be("My Work");

            tokens.Should().ContainKey("RegistrationNumber").
                WhoseValue.Should().Be("123");

            tokens.Should().ContainKey("Date").
                WhoseValue.Should().Be(
                dto.DateUtc.ToString(DefaultDateTimeFormat.DateTimeFormat));

            tokens.Should().ContainKey("SenderName").
                WhoseValue.Should().Be("Alice");

            tokens.Should().ContainKey("SenderAddress").
                WhoseValue.Should().Be("Sofia");

            tokens.Should().ContainKey("SenderEmail").
                WhoseValue.Should().Be("a@a.com");

            tokens.Should().ContainKey("RecipientName").
                WhoseValue.Should().Be("Bob");

            tokens.Should().ContainKey("RecipientAddress").
                WhoseValue.Should().Be("Plovdiv");

            tokens.Should().ContainKey("RecipientEmail").
                WhoseValue.Should().Be("b@b.com");

            tokens.Should().ContainKey("InfringingUrl").
                WhoseValue.Should().Be("https://example.com");

            tokens.Should().ContainKey("AdditionalFacts").
                WhoseValue.Should().Be("Facts");
        }

        [Test]
        public void BuildTokens_ShouldAddYearOfCreation_WhenProvided()
        {
            var dto = new PdfLetterDto
            {
                WorkTitle = "W",
                DateUtc = DateTime.UtcNow,
                SenderName = "S",
                YearOfCreation = 2020
            };

            var tokens = LetterTokenBuilder.BuildTokens(dto);

            tokens.Should().ContainKey("YearOfCreation").
                WhoseValue.Should().Be("2020");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void BuildTokens_ShouldNotAddKey_WhenValueIsNullOrWhitespace(string? value)
        {
            var dto = new PdfLetterDto
            {
                WorkTitle = "W",
                DateUtc = DateTime.UtcNow,
                SenderName = "S",
                SenderEmail = value
            };

            var tokens = LetterTokenBuilder.BuildTokens(dto);
            tokens.Should().NotContainKey("SenderEmail");
        }

        [Test]
        public void BuildTokens_ShouldNotAddYearOfCreation_WhenNull()
        {
            var dto = new PdfLetterDto
            {
                WorkTitle = "W",
                DateUtc = DateTime.UtcNow,
                SenderName = "S",
                YearOfCreation = null
            };

            var tokens = LetterTokenBuilder.BuildTokens(dto);
            tokens.Should().NotContainKey("YearOfCreation");
        }

        [Test]
        public void BuildTokens_ShouldAddDateOfPublication_WhenProvided_AndFormatIt()
        {
            var date = 
                new DateTime(
                    2020, 1, 2, 0, 0, 0, DateTimeKind.Utc);

            var dto = new PdfLetterDto
            {
                WorkTitle = "W",
                DateUtc = DateTime.UtcNow,
                SenderName = "S",
                DateOfPublication = date
            };

            var tokens = LetterTokenBuilder.BuildTokens(dto);

            tokens.Should().ContainKey("DateOfPublication").
                WhoseValue.Should().Be(
                date.ToString(DefaultDateTimeFormat.DateTimeFormat));
        }

        [Test]
        public void BuildTokens_ShouldNotAddDateOfPublication_WhenNull()
        {
            var dto = new PdfLetterDto
            {
                WorkTitle = "W",
                DateUtc = DateTime.UtcNow,
                SenderName = "S",
                DateOfPublication = null
            };

            var tokens = LetterTokenBuilder.BuildTokens(dto);

            tokens.Should().NotContainKey("DateOfPublication");
        }
    }
}
