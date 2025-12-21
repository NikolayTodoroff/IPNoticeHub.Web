using IPNoticeHub.Application.Services.DraftServices.Abstractions;
using System.Collections.Concurrent;
using System.Text.Json;

namespace IPNoticeHub.Infrastructure.Persistence.Services.DraftServices.Implementations
{
    public class UserInputDraftStore : IUserInputDraftStore
    {
        private record Envelope(
            string UserId, 
            string KeySpace, 
            DateTimeOffset ExpiresAt, 
            string InputJson);

        private static readonly ConcurrentDictionary<Guid, Envelope> draftStore =
            new ConcurrentDictionary<Guid, Envelope>();

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public Task<T?> GetAsync<T>(
            string userId, 
            Guid draftId, 
            string keySpace, 
            CancellationToken cancellationToken = default)
        {
            if (draftStore.TryGetValue(draftId, out var envelope))
            {
                if (envelope.UserId == userId && envelope.KeySpace == keySpace && 
                    envelope.ExpiresAt > DateTimeOffset.UtcNow)
                {
                    var payload = JsonSerializer.Deserialize<T>(envelope.InputJson, _jsonOptions);
                    return Task.FromResult(payload);
                }
            }

            return Task.FromResult<T?>(default);
        }

        public Task<Guid> SaveAsync<T>(
            string userId, 
            string keySpace, 
            T payload, 
            TimeSpan ttl, 
            CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid();
            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            var expires = DateTimeOffset.UtcNow.Add(ttl);

            draftStore[id] = new Envelope(userId, keySpace, expires, json);

            return Task.FromResult(id);
        }

        public Task RemoveAsync(string userId, Guid draftId, string keySpace, CancellationToken cancellationToken = default)
        {
            draftStore.TryRemove(draftId, out _);
            return Task.CompletedTask;
        }
    }
}
