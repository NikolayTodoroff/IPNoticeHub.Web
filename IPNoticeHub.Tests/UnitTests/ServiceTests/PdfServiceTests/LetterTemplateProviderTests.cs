using FluentAssertions;
using IPNoticeHub.Application.Services.PdfGenerationService.Implementations;
using IPNoticeHub.Shared.Enums;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.ServiceTests.PdfServiceTests
{
    public class LetterTemplateProviderTests
    {
        [Test]
        public void GetLetterTemplatePresets_ShouldReturnOnlyPresetsOfRequestedType()
        {
            var provider = new LetterTemplateProvider();

            var result = provider.GetLetterTemplatePresets(LetterTemplateType.CeaseAndDesist);

            result.Should().NotBeNull();
            result.Should().OnlyContain
                (p => p.Type == LetterTemplateType.CeaseAndDesist);
        }

        [Test]
        public void GetLetterTemplatePresets_WhenNoPresetsForType_ShouldReturnEmptyList_NotNull()
        {
            var provider = new LetterTemplateProvider();

            var typeWithNoPresets = (LetterTemplateType)999;

            var result = 
                provider.GetLetterTemplatePresets(typeWithNoPresets);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public void GetTemplateByKey_ShouldReturnTemplate_WhenKeyMatches_IgnoringCase()
        {
            var provider = new LetterTemplateProvider();

            var keyLower = "dmca-general";
            var keyUpper = "DMCA-GENERAL";

            var lower = provider.GetTemplateByKey(keyLower);
            var upper = provider.GetTemplateByKey(keyUpper);

            lower.Should().NotBeNull();
            upper.Should().NotBeNull();

            lower!.Key.Should().Be("DMCA-General");
            upper!.Key.Should().Be("DMCA-General");

            lower.BodyTemplate.Should().NotBeNullOrWhiteSpace();
            upper.BodyTemplate.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void GetTemplateByKey_ShouldReturnNull_WhenKeyNotFound()
        {
            var provider = new LetterTemplateProvider();

            var result = provider.GetTemplateByKey("NO_SUCH_KEY_12345");

            result.Should().BeNull();
        }
    }
}
