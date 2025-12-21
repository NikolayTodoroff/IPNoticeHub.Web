using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPNoticeHub.Application.Services.DraftServices.Abstractions
{
    public interface IUserInputDraftStore
    {
        Task<Guid> SaveAsync<T>(
            string userId,
            string keySpace,
            T payload,
            TimeSpan ttl,
            CancellationToken cancellationToken = default);

        Task<T?> GetAsync<T>(
            string userId,
            Guid draftId,
            string keySpace,
            CancellationToken cancellationToken = default);

        Task RemoveAsync(
            string userId,
            Guid draftId,
            string keySpace,
            CancellationToken cancellationToken = default);
    }
}
