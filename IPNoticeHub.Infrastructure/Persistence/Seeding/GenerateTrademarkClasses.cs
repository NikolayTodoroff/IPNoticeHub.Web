using IPNoticeHub.Domain.Entities.Trademarks;

namespace IPNoticeHub.Infrastructure.Persistence.Seeding
{
    public static class GenerateTrademarkClasses
    {
        private const int DefaultClassNumber = 9;
        private const int MaxClassesPerTrademark = 2;

        public static List<TrademarkClassAssignment> GenerateTrademarkClassAssignments(
            IReadOnlyCollection<TrademarkEntity> trademarks)
        {
            if (trademarks is null) throw new ArgumentNullException(nameof(trademarks));

            var classes = 
                new List<TrademarkClassAssignment>(trademarks.Count * MaxClassesPerTrademark);

            foreach (var trademark in trademarks)
            {
                foreach (var classNumber in DeriveClasses(trademark.GoodsAndServices).
                    Take(MaxClassesPerTrademark))
                {
                    classes.Add(new TrademarkClassAssignment
                    {
                        TrademarkRegistrationId = trademark.Id,
                        ClassNumber = classNumber
                    });
                }
            }

            return classes;
        }

        private const int PrimaryClassWeight = 4;
        private const int SecondaryClassWeight = 2;

        private static IEnumerable<int> DeriveClasses(string? goodsAndServices)
        {
            if (string.IsNullOrWhiteSpace(goodsAndServices))
                return new[] { DefaultClassNumber };

            var scores = new Dictionary<int, int>(capacity: 8);

            foreach (var rule in Rules)
            {
                if (!ContainsAny(goodsAndServices, rule.Keywords))
                    continue;

                for (var i = 0; i < rule.Classes.Length; i++)
                {
                    var classNumber = rule.Classes[i];
                    var weight = (i == 0) ? PrimaryClassWeight : SecondaryClassWeight;

                    scores[classNumber] = scores.TryGetValue(classNumber, out var current) ? 
                        current + weight : weight;
                }
            }

            if (scores.Count == 0) return new[] { DefaultClassNumber };

            return scores.
                OrderByDescending(kv => kv.Value).
                ThenBy(kv => kv.Key).
                Select(kv => kv.Key).
                Take(3);
        }

        private static bool ContainsAny(string text, string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }

        private sealed record Rule(string[] Keywords, int[] Classes);

        private static readonly Rule[] Rules =
        {
            new(
                Keywords: new[]
                {
                    "clothing", "apparel", "t-shirt", "shirt", "hoodie", "jacket", "pants", "jeans", "dress", "skirt",
                    "footwear", "shoes", "sneakers", "headgear", "hat", "cap"
                }, Classes: new[] { 25, 18 }),

            new(
                Keywords: new[]
                {
                    "software", "saas", "application", "app", "mobile", "downloadable", "computer program", "firmware"
                }, Classes: new[] { 9, 42 }),

            new(
                Keywords: new[]
                {
                    "cloud", "hosting", "platform", "api", "development", "devops", "consulting", "it services",
                    "software as a service"
                }, Classes: new[] { 42, 35 }),

            new(
                Keywords: new[]
                {
                    "legal", "trademark", "copyright", "dmca", "compliance", "privacy",
                    "investigation", "security services", "licensing"
                }, Classes: new[] { 45 }),

            new(
                Keywords: new[]
                {
                    "advertising", "marketing", "business", "retail", "e-commerce", "ecommerce",
                    "marketplace", "online store", "wholesale", "subscriptions"
                }, Classes: new[] { 35 }),

            new(
                Keywords: new[]
                {
                    "beverage", "beverages", "soft drink", "soda", "juice", "sparkling water", "energy drink", "isotonic"
                }, Classes: new[] { 32 }),

            new(
                Keywords: new[]
                {
                    "wine", "wines", "spirits", "vodka", "whiskey", "rum", "gin", "liqueur", "champagne"
                }, Classes: new[] { 33 }),

            new(
                Keywords: new[]
                {
                    "coffee", "tea", "cocoa", "chocolate", "bread", "pastry", "pasta", "spices", "condiments"
                }, Classes: new[] { 30 }),

            new(
                Keywords: new[]
                {
                    "meat", "fish", "seafood", "dairy", "cheese", "eggs", "processed food", "preserved"
                }, Classes: new[] { 29 }),

            new(
                Keywords: new[]
                {
                    "fresh", "fruit", "fruits", "vegetable", "vegetables", "grain", "seeds", "plants", "animal feed"
                }, Classes: new[] { 31 }),

            new(
                Keywords: new[]
                {
                    "paper", "printed", "poster", "book", "magazine", "stationery", "notebook", "calendar", "brochure",
                    "label"
                }, Classes: new[] { 16 }),

            new(
                Keywords: new[]
                {
                    "jewelry", "jewellery", "watch", "watches", "bracelet", "necklace", "ring"
                }, Classes: new[] { 14 }),

            new(
                Keywords: new[]
                {
                    "musical instrument", "guitar", "piano", "drum", "violin"
                }, Classes: new[] { 15 }),

            new(
                Keywords: new[]
                {
                    "toy", "toys", "game", "games", "board game", "sporting", "sports", "fitness equipment", "gym"
                }, Classes: new[] { 28 }),

            new(
                Keywords: new[]
                {
                    "furniture", "chair", "table", "sofa", "mattress"
                }, Classes: new[] { 20 }),

            new(
                Keywords: new[]
                {
                    "housewares", "kitchen", "cookware", "utensil", "glassware", "mug", "cup", "plate"
                }, Classes: new[] { 21 }),

            new(
                Keywords: new[]
                {
                    "plastic", "rubber", "polymer", "silicone", "packing material", "insulation"
                }, Classes: new[] { 17 }),

            new(
                Keywords: new[]
                {
                    "building material", "cement", "concrete", "brick", "tile", "stone", "wood panel"
                }, Classes: new[] { 19 }),

            new(
                Keywords: new[]
                {
                    "construction", "repair", "installation", "maintenance"
                }, Classes: new[] { 37 }),

            new(
                Keywords: new[]
                {
                    "transport", "shipping", "delivery", "logistics", "storage", "warehousing"
                }, Classes: new[] { 39 }),

            new(
                Keywords: new[]
                {
                    "education", "training", "course", "workshop", "entertainment", "events", "streaming"
                }, Classes: new[] { 41 }),

            new(
                Keywords: new[]
                {
                    "restaurant", "catering", "food service", "hotel", "lodging"
                }, Classes: new[] { 43 }),

            new(
                Keywords: new[]
                {
                    "medical", "clinic", "healthcare", "beauty", "salon", "spa", "agricultural services"
                }, Classes: new[] { 44 }),

            new(
                Keywords: new[]
                {
                    "smoker", "smokers", "cigarette", "cigar", "tobacco", "vape", "hookah"
                }, Classes: new[] { 34 }),

            new(
                Keywords: new[]
                {
                    "firearm", "firearms", "gun", "guns", "ammunition"
                }, Classes: new[] { 13 }),
        };
    }
}
