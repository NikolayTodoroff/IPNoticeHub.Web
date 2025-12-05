using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Services.Copyrights.DTOs;
using IPNoticeHub.Services.Trademarks.DTOs;
using IPNoticeHub.Web.Models.PdfGeneration;

namespace IPNoticeHub.Web.Infrastructure
{
    public static class ApplyEntityDetails
    {
        public static void ApplyCopyrightDMCADetails(DMCAViewModel viewModel, CopyrightDetailsDto dto,
            MergeStrategy strategy = MergeStrategy.FillBlanks)
        {
            static string? FillOnly(string? user, string? fromDb)
            {
                return string.IsNullOrWhiteSpace(user) ? fromDb ?? string.Empty : user;
            }

            if (strategy == MergeStrategy.OverwriteAll)
            {
                viewModel.WorkTitle = dto.Title ?? string.Empty;
                viewModel.RegistrationNumber = dto.RegistrationNumber ?? string.Empty;
                viewModel.YearOfCreation = dto.YearOfCreation;
                viewModel.DateOfPublication = dto.DateOfPublication;
                viewModel.NationOfFirstPublication = dto.NationOfFirstPublication;
                return;
            }

            viewModel.WorkTitle = FillOnly(viewModel.WorkTitle, dto.Title) ?? string.Empty;
            viewModel.RegistrationNumber = FillOnly(viewModel.RegistrationNumber, dto.RegistrationNumber);
            viewModel.NationOfFirstPublication = FillOnly(viewModel.NationOfFirstPublication, dto.NationOfFirstPublication);
            viewModel.YearOfCreation ??= dto.YearOfCreation;
            viewModel.DateOfPublication ??= dto.DateOfPublication;
        }

        public static void ApplyCopyrightCeaseDesistDetails(CeaseDesistViewModel viewModel, CopyrightDetailsDto dto,
            MergeStrategy strategy = MergeStrategy.FillBlanks)
        {
            static string? FillOnly(string? user, string? fromDb)
            {
                return string.IsNullOrWhiteSpace(user) ? fromDb : user;
            }

            if (strategy == MergeStrategy.OverwriteAll)
            {
                viewModel.WorkTitle = dto.Title ?? string.Empty;
                viewModel.RegistrationNumber = dto.RegistrationNumber ?? string.Empty;
                return;
            }

            viewModel.WorkTitle = FillOnly(viewModel.WorkTitle, dto.Title) ?? string.Empty;
            viewModel.RegistrationNumber = FillOnly(viewModel.RegistrationNumber, dto.RegistrationNumber) ?? string.Empty;
        }

        public static void ApplyTrademarkCeaseDesistDetails(CeaseDesistViewModel viewModel, TrademarkDetailsDto dto, 
            MergeStrategy strategy = MergeStrategy.FillBlanks)
        {
            static string? FillOnly(string? user, string? fromDb)
            {
                return string.IsNullOrWhiteSpace(user) ? fromDb : user;
            }

            if (strategy == MergeStrategy.OverwriteAll)
            {
                viewModel.WorkTitle = dto.Wordmark ?? string.Empty;
                viewModel.RegistrationNumber = dto.RegistrationNumber ?? string.Empty;
                return;
            }

            viewModel.WorkTitle = FillOnly(viewModel.WorkTitle, dto.Wordmark) ?? string.Empty;
            viewModel.RegistrationNumber = FillOnly(viewModel.RegistrationNumber, dto.RegistrationNumber) ?? string.Empty;
        }
    }
}
