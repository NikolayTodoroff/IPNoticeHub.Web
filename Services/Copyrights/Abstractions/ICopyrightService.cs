using IPNoticeHub.Services.Copyrights.DTOs;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Common;

namespace IPNoticeHub.Services.Copyrights.Abstractions
{
    public interface ICopyrightService
    {
        Task<Guid> CreateAsync(string userId, CopyrightCreateDTO dto, CancellationToken cancellationToken = default);

        Task<bool> EditAsync(string userId, Guid publicId, CopyrightEditDTO dto, CancellationToken cancellationToken = default);

        Task<bool> RemoveAsync(string userId, Guid publicId, CancellationToken cancellationToken = default);

        Task<CopyrightDetailsDTO?> GetDetailsAsync(string userId, Guid publicId, CancellationToken cancellationToken = default);

        Task<PagedResult<CopyrightListItemDTO>> GetUserCollectionAsync(string userId,CollectionSortBy sortBy,
            int page, int resultsPerPage, CancellationToken cancellationToken = default);
    }
}
