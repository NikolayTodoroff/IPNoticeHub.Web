using IPNoticeHub.Application.Repositories.TrademarkRepository;
using System.Text.RegularExpressions;
using IPNoticeHub.Application.Repositories.WatchlistRepository;
using IPNoticeHub.Application.Services.WatchlistService.Abstractions;
using IPNoticeHub.Application.DTOs.WatchlistDTOs;

namespace IPNoticeHub.Application.Services.WatchlistService.Implementations
{
    public sealed class WatchlistService : IWatchlistService
    {
        private readonly IWatchlistRepository watchlistRepo;
        private readonly ITrademarkStatusSnapshotRepository snapshotRepo;

        private readonly IStatusLabelProvider statusLabels;

        public WatchlistService(IWatchlistRepository watchlistRepo, 
            ITrademarkStatusSnapshotRepository snapshotRepo, IStatusLabelProvider statusLabels)
        {
            this.watchlistRepo = watchlistRepo;
            this.snapshotRepo = snapshotRepo;
            this.statusLabels = statusLabels;
        }

        public async Task AddAsync(string userId, int trademarkId, CancellationToken cancellationToken)
        {
            var statusSnapshot = 
                await snapshotRepo.GetStatusSnapshotAsync(trademarkId, cancellationToken);

            if (statusSnapshot is null)
            {
                throw new InvalidOperationException($"No status snapshot found for trademarkId {trademarkId}.");
            } 

            await watchlistRepo.AddOrUndeleteAsync(userId, trademarkId, statusSnapshot.Value.StatusCodeRaw,
                statusSnapshot.Value.StatusDetail, statusSnapshot.Value.StatusDateUtc, cancellationToken);
        }

        public async Task<IReadOnlyList<WatchlistItemDto>> GetListByUserAsync(string userId, CancellationToken cancellationToken)
        {
            var links = await watchlistRepo.ListByUserAsync(userId, 0, 200, cancellationToken);

            var items = links.Select(l =>
            {
                int? currentStatusCode = l.Trademark.StatusCodeRaw;
                string? currentStatusText = l.Trademark.StatusDetail ?? "";

                int? initialStatusCode = l.InitialStatusCodeRaw;
                string? initialStatusText = l.InitialStatusText;

                bool isStatusChanged = ComputeStatusChange(initialStatusCode, currentStatusCode, initialStatusText, currentStatusText);

                return new WatchlistItemDto
                {
                    Id = l.TrademarkId,
                    PublicId = l.Trademark.PublicId,
                    RegistrationNumber = l.Trademark.RegistrationNumber ?? "",
                    Wordmark = l.Trademark.Wordmark,
                    Owner = l.Trademark.Owner,
                    AddedOnDate = l.AddedOnUtc,
                    InitialStatus = initialStatusText ?? LabelFromCode(initialStatusCode),
                    CurrentStatus = string.IsNullOrWhiteSpace(currentStatusText) ? LabelFromCode(currentStatusCode) : currentStatusText,
                    HasStatusChange = isStatusChanged,
                    NotificationsEnabled = l.NotificationsEnabled
                };
            }).
            ToList();

            return items;
        }
        public async Task RemoveAsync(string userId, int trademarkId, CancellationToken cancellationToken)
        {
            await watchlistRepo.SoftRemoveAsync(userId, trademarkId, cancellationToken);
        }

        public async Task ToggleNotificationsAsync(string userId, int trademarkId, bool notificationsEnabled, CancellationToken cancellationToken)
        {
            await watchlistRepo.ToggleNotificationsAsync(userId, trademarkId, notificationsEnabled, cancellationToken);
        }

        public async Task<bool> ExistsAsync(string userId, int trademarkId, CancellationToken cancellationToken)
        {
            return await watchlistRepo.ExistsAsync(userId, trademarkId, cancellationToken);
        }

        /// <summary>
        /// Determines whether the trademark status has changed between the initial and current values.
        /// Rules for determining status change:
        /// 1) If both status codes are available, compare them directly.
        /// 2) If status codes are unavailable but both status texts exist, compare their normalized forms.
        /// 3) If neither condition is met, there is insufficient information to determine a status change.
        /// </summary>
        private static bool ComputeStatusChange(int? initialStatusCode, int? currentStatusCode, string? initialStatusText, string? currentStatusText)
        {
            if (initialStatusCode.HasValue && currentStatusCode.HasValue)
            {
                return initialStatusCode.Value != currentStatusCode.Value;
            }

            if (!string.IsNullOrWhiteSpace(initialStatusText) && !string.IsNullOrWhiteSpace(currentStatusText))
            {
                return !Normalize(initialStatusText).Equals(Normalize(currentStatusText), StringComparison.Ordinal);
            }

            return false;
        }

        private static string Normalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            return Regex.Replace(text.Trim().ToLowerInvariant(), pattern: @"\s+", replacement: " ");
        }

        private string LabelFromCode(int? statusCode)
        {
            return statusCode.HasValue ? statusLabels.GetStatusLabel("USPTO", statusCode.Value) : string.Empty;
        }
    }
}
