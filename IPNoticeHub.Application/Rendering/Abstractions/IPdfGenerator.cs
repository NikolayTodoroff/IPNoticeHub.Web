using IPNoticeHub.Application.DTOs.PdfDTOs;

namespace IPNoticeHub.Application.Rendering.Abstractions
{
    public class IPdfGenerator
    {
        byte[] GenerateDocument(PdfLetterDto dto);
    }
}
