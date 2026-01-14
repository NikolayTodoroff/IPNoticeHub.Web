using Bogus;
using IPNoticeHub.Domain.Entities.Copyrights;

namespace IPNoticeHub.Infrastructure.Persistence.Seeding
{
    public static class GenerateCopyrightEntities
    {
        public static List<CopyrightEntity> GenerateCopyrights(int count)
        {
            var faker = new Faker("en");

            var workTypes = new[]
            {
                "Literary Work",
                "Visual Material",
                "Sound Recording",
                "Performing Arts",
                "Computer Software",
                "Motion Pictures",
                "Audiovisual Works",
                "Single Serial Issue"
            };

            return new Faker<CopyrightEntity>("en").
                RuleFor(x => x.PublicId, _ => Guid.NewGuid()).
                RuleFor(x => x.RegistrationNumber, f => f.PickRandom(
                    "TX", "VA", "SR", "PA") + f.Random.ReplaceNumbers("##########")).
                RuleFor(x => x.TypeOfWork, f => f.PickRandom(workTypes)).
                RuleFor(x => x.Title, f => $"{f.Commerce.ProductName()} {f.Hacker.Noun()} Collection").
                RuleFor(x => x.YearOfCreation, f => f.Date.Past(10).Year).
                RuleFor(x => x.DateOfPublication, f => f.Date.Past(5)).
                RuleFor(x => x.Owner, f => f.Name.FullName()).
                RuleFor(x => x.NationOfFirstPublication, _ => "United States").
                Generate(count);
        }
    }
}
