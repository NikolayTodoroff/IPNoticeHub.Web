using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Web.Models;
using IPNoticeHub.Web.Models.TrademarkSearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static IPNoticeHub.Shared.Constants.PagingConstants.DefaultPagingConstants;
using IPNoticeHub.Application.Services.TrademarkSearchService.Abstractions;
using IPNoticeHub.Application.DTOs.TrademarkDTOs;
using IPNoticeHub.Web.Models.Errors;

namespace IPNoticeHub.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITrademarkSearchQueryService searchService;

        public HomeController(ITrademarkSearchQueryService searchService)
        {
            this.searchService = searchService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Results(
            string? trademark, 
            TrademarkClass? classNumber, 
            TrademarkStatusCategory? status,
            TrademarkSearchBy? searchByItem, 
            DataProvider? office, 
            SearchMode? mode,
            int currentPage = DefaultPage, 
            int pageSize = DefaultPageSize,
            CancellationToken cancellationToken = default)
        {
            var dto = new TrademarkSearchQueryDto
            {
                Query = trademark,
                Class = classNumber,
                Status = status,
                SearchBy = searchByItem,
                Office = office,
                Mode = mode,
                Page = currentPage,
                PageSize = pageSize
            };

            var (searchResults,resultsCount) = 
                await searchService.SearchAsync(
                    dto,
                    cancellationToken);

            var viewModel = 
                new TrademarkSearchResultsViewModel
            {
                Query = trademark,
                Class = classNumber,
                Status = status,
                SearchBy = searchByItem,
                Office = office,
                Mode = mode,
                Results = searchResults.Select(
                    s => new TreademarkSearchResultSingleItemViewModel
                {
                    Id = s.Id,
                    PublicId = s.PublicId,
                    RegistrationNumber = s.RegistrationNumber,
                    Wordmark = s.Wordmark,
                    Owner = s.Owner,
                    Status = s.Status,
                    RegistrationDate = s.RegistrationDate
                }).
                ToList(),
                Total = resultsCount,
                CurrentPage = currentPage,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        [AllowAnonymous]
        public IActionResult ErrorStatus(int statusCode)
        {
            if (statusCode == 404) return View("NotFound");

            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            return View("Error", errorViewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
