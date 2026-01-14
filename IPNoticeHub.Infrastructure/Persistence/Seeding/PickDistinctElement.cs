using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPNoticeHub.Infrastructure.Persistence.Seeding
{
    public static class PickDistinctElement
    {
        public static List<T> PickDistinct<T>(List<T> source, int count)
        {
            if (count <= 0) return new List<T>();
            if (count >= source.Count) return source.ToList();

            return source.
                OrderBy(_ => Random.Shared.Next()).
                Take(count).
                ToList();
        }
    }
}
