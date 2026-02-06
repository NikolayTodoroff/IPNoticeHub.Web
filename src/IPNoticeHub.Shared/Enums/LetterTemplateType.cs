using System.ComponentModel.DataAnnotations;

namespace IPNoticeHub.Shared.Enums
{
    public enum LetterTemplateType
    {
        [Display(Name = "Cease & Desist")]
        CeaseAndDesist = 1,

        [Display(Name = "DMCA")]
        Dmca = 2
    }
}
