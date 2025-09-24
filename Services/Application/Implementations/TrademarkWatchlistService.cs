using IPNoticeHub.Data.Repositories.Application.Abstractions;
using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Services.Application.DTOs;
using System.Text;
using System.Text.RegularExpressions;

namespace IPNoticeHub.Services.Application.Implementations
{
    public sealed class TrademarkWatchlistService : ITrademarkWatchlistService
    {
        private readonly IUserTrademarkWatchlistRepository watchlistRepo;
        private readonly ITrademarkStatusSnapshotRepository snapshotRepo;

        public TrademarkWatchlistService(IUserTrademarkWatchlistRepository watchlistRepo, ITrademarkStatusSnapshotRepository snapshotRepo)
        {
            this.watchlistRepo = watchlistRepo;
            this.snapshotRepo = snapshotRepo;
        }

        public async Task AddAsync(string userId, int trademarkId, CancellationToken cancellationToken)
        {
            var statusSnapshot = await snapshotRepo.GetStatusSnapshotAsync(trademarkId, cancellationToken);

            if (statusSnapshot is null)
                throw new InvalidOperationException($"No status snapshot found for trademarkId {trademarkId}.");

            await watchlistRepo.AddOrUndeleteAsync(userId, trademarkId, statusSnapshot.Value.StatusCodeRaw,
                statusSnapshot.Value.StatusDetail, statusSnapshot.Value.StatusDateUtc, cancellationToken
            );
        }

        public async Task<IReadOnlyList<TrademarkWatchlistItemDTO>> GetListByUserAsync(string userId, CancellationToken cancellationToken)
        {
            var links = await watchlistRepo.ListByUserAsync(userId, 0, 200, cancellationToken);

            var items = links.Select(l =>
            {
                int? currentStatusCode = l.TrademarkRegistration.StatusCodeRaw;
                string? currentStatusText = l.TrademarkRegistration.StatusDetail ?? "";

                int? initialStatusCode = l.WatchlistInitialStatusCodeRaw;
                string? initialStatusText = l.WatchlistInitialStatusText;

                bool isStatusChanged = ComputeStatusChange(initialStatusCode, currentStatusCode, initialStatusText, currentStatusText);

                return new TrademarkWatchlistItemDTO
                {
                    Id = l.TrademarkRegistrationId,
                    RegistrationNumber = l.TrademarkRegistration.RegistrationNumber ?? "",
                    Wordmark = l.TrademarkRegistration.Wordmark,
                    Owner = l.TrademarkRegistration.Owner,
                    AddedOnDate = l.WatchlistAddedOnUtc,
                    InitialStatus = initialStatusText ?? LabelFromCode(initialStatusCode),
                    CurrentStatus = string.IsNullOrWhiteSpace(currentStatusText) ? LabelFromCode(currentStatusCode) : currentStatusText,
                    HasStatusChange = isStatusChanged,
                    NotificationsEnabled = l.WatchlistNotificationsEnabled
                };
            }).
            ToList();

            return items;
        }

        public async Task RemoveAsync(string userId, int trademarkId, CancellationToken cancellationToken)
        {
            await watchlistRepo.SoftRemoveAsync(userId, trademarkId, cancellationToken);
        }

        public async Task ToggleNotificationsAsync(string userId, int trademarkId, bool enabled, CancellationToken cancellationToken)
        {
            await watchlistRepo.ToggleNotificationsAsync(userId, trademarkId, enabled, cancellationToken);
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
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Lowercase, trim ends, and collapse internal whitespace to single spaces
            return Regex.Replace(text.Trim().ToLowerInvariant(), @"\s+", " ");
        }

        private static string LabelFromCode(int? code)
        {
            if (!code.HasValue) return "";

            return code.Value switch
            {
                630 => "New application entered",
                638 => "Assigned to examiner",
                _ => $"Status {code.Value}"
            };
        }
    }
}
