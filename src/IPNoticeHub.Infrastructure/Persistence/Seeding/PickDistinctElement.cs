using Bogus;

namespace IPNoticeHub.Infrastructure.Persistence.Seeding
{
    public static class PickDistinctElement
    {
        public static List<T> PickDistinct<T>(List<T> source, int count)
        {
            var faker = new Faker("en");

            if (count <= 0) return new List<T>();
            if (count >= source.Count) return source.ToList();

            return faker.Random.Shuffle(source).Take(count).ToList();
        }
    }
}
