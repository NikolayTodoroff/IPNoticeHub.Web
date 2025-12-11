using IPNoticeHub.Domain.Entities.Trademarks;
using IPNoticeHub.Application.Repositories.TrademarkRepository;
using IPNoticeHub.Application.Trademarks.Abstractions;
using IPNoticeHub.Shared.Support;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;

namespace IPNoticeHub.Application.Services.TrademarkService.Implementations
{
    public sealed class TrademarkSearchService : ITrademarkSearchService
    {
        private readonly ITrademarkRepository trademarkRepository;

        public TrademarkSearchService(ITrademarkRepository trademarks)
        {
            this.trademarkRepository = trademarks;
        }

        public async Task<TrademarkDetailsDto?>GetDetailsAsync(
            Guid publicId, 
            CancellationToken cancellationToken = default)
        {
            TrademarkEntity? entity = await trademarkRepository.
                GetByPublicIdAsync(
                publicId, 
                cancellationToken: cancellationToken);

            if (entity is null) return null;

            return new TrademarkDetailsDto
            {
                Id = entity.Id,
                PublicId = entity.PublicId,
                Wordmark = entity.Wordmark,
                Owner = entity.Owner,
                SourceId = entity.SourceId,
                RegistrationNumber = entity.RegistrationNumber,
                Status = entity.StatusCategory,
                GoodsAndServices = entity.GoodsAndServices,
                FilingDate = entity.FilingDate,
                RegistrationDate = entity.RegistrationDate,
                MarkImageUrl = entity.MarkImageUrl,
                Provider = entity.Source,
                Classes = entity.Classes.
                    Select(c => c.ClassNumber).
                    ToList(),
                Events = entity.Events.
                    OrderByDescending(e => e.EventDate).
                        Select(e => (e.EventDate, e.Code, e.Description)).
                        ToList()
            };
        }

        public async Task<PagedResult<TrademarkSingleItemDto>>SearchAsync(
            TrademarkFilterDto dto, 
            int currentPage, 
            int resultsPerPage, 
            CancellationToken cancellationToken = default)
        {
            var (normalizedPage, normalizedPageSize) = 
                PagingConfiguration.NormalizePaging(
                    currentPage, 
                    resultsPerPage);

            var searchFilter = new TrademarkSearchFilter
            {
                Provider = dto.Provider,
                ClassNumbers = dto.ClassNumbers,
                Status = dto.Status,
                SearchBy = dto.SearchBy,
                SearchTerm = dto.SearchTerm,
                ExactMatch = dto.ExactMatch
            };

            IOrderedQueryable<TrademarkEntity>? query = trademarkRepository.
                Query(searchFilter, includeNav: false).
                OrderBy(t => t.Wordmark).
                ThenBy(t => t.Id);

            int resultsCount = await query.CountAsync(cancellationToken);

            List<TrademarkSingleItemDto>? searchResults = await query.
                Skip((normalizedPage - 1) * normalizedPageSize).
                Take(normalizedPageSize).
                Select(t => new TrademarkSingleItemDto
                {
                    Id = t.Id,
                    PublicId = t.PublicId,
                    Wordmark = t.Wordmark,
                    Owner = t.Owner,
                    SourceId = t.SourceId,
                    Status = t.StatusCategory,
                    Classes = t.Classes.
                        Select(c => c.ClassNumber).
                        ToArray(),
                    Provider = t.Source
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<TrademarkSingleItemDto>
            {
                Results = searchResults,
                ResultsCount = resultsCount,
                CurrentPage = normalizedPage,
                ResultsCountPerPage = normalizedPageSize
            };
        }
    }
}
