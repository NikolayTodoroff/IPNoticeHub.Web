using IPNoticeHub.Application.DTOs.PdfDTOs;

namespace IPNoticeHub.Application.LetterComposition.Abstractions
{
    public interface ILetterAssembler
    {
        PdfLetterDto RebuildLetterInput(LetterInputDto input);
    }
}
