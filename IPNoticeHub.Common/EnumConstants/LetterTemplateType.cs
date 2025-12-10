using System.ComponentModel.DataAnnotations;

namespace IPNoticeHub.Shared.EnumConstants
{
    public enum LetterTemplateType
    {
        [Display(Name = "Cease & Desist")]
        CeaseAndDesist = 1,

        [Display(Name = "DMCA")]
        Dmca = 2
    }
}
