using IPNoticeHub.Domain.Entities.Trademarks;

namespace IPNoticeHub.Infrastructure.Persistence.Seeding
{
    public static class GenerateTrademarkClasses
    {
        public static List<TrademarkClassAssignment> GenerateTrademarkClassAssignments(List<TrademarkEntity> trademarks)
        {
            var classes = new List<TrademarkClassAssignment>();

            foreach (var entity in trademarks)
            {
                var classNumbers = DeriveClasses(entity.GoodsAndServices);

                foreach (var classNumber in classNumbers.Take(2).Distinct())
                {
                    classes.Add(new TrademarkClassAssignment
                    {
                        TrademarkRegistrationId = entity.Id,
                        ClassNumber = classNumber
                    });
                }
            }

            int[] DeriveClasses(string goods)
            {
                goods = goods.ToLowerInvariant();

                if (goods.Contains("clothing") || goods.Contains("footwear")) return new[] { 25, 18 };
                if (goods.Contains("software") || goods.Contains("saas") || goods.Contains("applications")) return new[] { 9, 42 };
                if (goods.Contains("security") || goods.Contains("incident")) return new[] { 42, 45 };
                if (goods.Contains("retail") || goods.Contains("marketplace")) return new[] { 35 };
                if (goods.Contains("beverages") || goods.Contains("soft drinks")) return new[] { 32 };
                if (goods.Contains("furniture") || goods.Contains("home")) return new[] { 20, 11 };
                return new[] { 9 };
            }

            return classes;
        }
    }
}
