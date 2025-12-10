using IPNoticeHub.Services.Copyrights.DTOs;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Shared.Infrastructure.Paging;

namespace IPNoticeHub.Services.Copyrights.Abstractions
{
    public interface ICopyrightService
    {
        Task<Guid>CreateAsync(
            string userId, 
            CopyrightCreateDto dto, 
            CancellationToken cancellationToken = default);

        Task<bool>EditAsync(
            string userId, 
            Guid publicId, 
            CopyrightEditDto dto, 
            CancellationToken cancellationToken = default);

        Task<bool>RemoveAsync(
            string userId, 
            Guid publicId, 
            CancellationToken cancellationToken = default);

        Task<CopyrightDetailsDto?>GetDetailsAsync(
            string userId, 
            Guid publicId, 
            CancellationToken cancellationToken = default);

        Task<PagedResult<CopyrightSingleItemDto>>GetUserCollectionAsync(
            string userId,
            CollectionSortBy sortBy,
            int page, 
            int resultsPerPage, 
            CancellationToken cancellationToken = default);
    }
}
