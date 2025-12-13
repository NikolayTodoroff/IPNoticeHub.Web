using IPNoticeHub.Application.DTOs.PdfDTOs;

namespace IPNoticeHub.Application.Rendering.Abstractions
{
    public interface IPdfGenerator
    {
        byte[] GenerateDocument(PdfLetterDto dto);
    }
}
