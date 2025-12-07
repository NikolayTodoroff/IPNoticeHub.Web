namespace IPNoticeHub.Services.Application.Abstractions
{
    public interface IPdfService
    {
        Task<byte[]> GenerateCopyrightDMCAAsync(
            DMCAInput data, 
            CancellationToken cancellation = default);

        Task<byte[]> GenerateCopyrightCeaseDesistAsync(
            CeaseDesistInput data, 
            CancellationToken cancellation = default);

        Task<byte[]> GenerateTrademarkCeaseDesistAsync(
            CeaseDesistInput data, 
            CancellationToken cancellation = default);

        Task<byte[]> CaptureDocumentSnapshotAsync(
            string title,
            string body,
            DateTime createdOn,
            CancellationToken cancellation = default);
    }

    public sealed record CeaseDesistInput(
        string SenderName,
        string SenderAddress,
        string RecipientName,
        string RecipientAddress,
        DateTime Date,
        string WorkTitle,
        string RegistrationNumber,
        string? AdditionalFacts,
        string BodyTemplate
        );

    public sealed record DMCAInput(
        string SenderName,
        string SenderEmail,
        string SenderAddress,
        string RecipientName,
        string RecipientEmail,
        string RecipientAddress,
        DateTime Date,
        string WorkTitle,
        string RegistrationNumber,
        int? YearOfCreation,
        DateTime? DateOfPublication,
        string? NationOfFirstPublication,
        string InfringingUrl,
        string GoodFaithStatement,
        string BodyTemplate
    );
}
