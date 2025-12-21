using IPNoticeHub.Application.Services.DraftServices.Abstractions;
using System.Collections.Concurrent;
using System.Text.Json;

namespace IPNoticeHub.Infrastructure.Persistence.Services.DraftServices.Implementations
{
    public class UserInputDraftStore : IUserInputDraftStore
    {


        public Task<T?> GetAsync<T>(string userId, Guid draftId, string keySpace, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string userId, Guid draftId, string keySpace, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> SaveAsync<T>(string userId, string keySpace, T payload, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
