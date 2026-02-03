using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Application.Templates.Abstractions;
using static IPNoticeHub.Shared.Constants.LetterTemplateKeys.TemplateTypeKeys;

namespace IPNoticeHub.Application.Services.PdfGenerationService.Implementations
{
    public sealed class LetterTemplateProvider : ILetterTemplateProvider
    {
        private static readonly List<LetterTemplatePreset> Presets = new List<LetterTemplatePreset>()
        {
            new LetterTemplatePreset (
                LetterTemplateType.CeaseAndDesist, 
                CopyrightCeaseDesistKey, 
                "Cease & Desist (Copyright)",
                BodyTemplate: @"{{Date}}

                {{RecipientName}}
                {{RecipientAddress}}

                Subject: Cease and Desist – Copyright Infringement of “{{WorkTitle}}” ({{RegistrationNumber}})

                Dear {{RecipientName}},

                This letter concerns your unauthorized use, reproduction, distribution, and/or public display of the copyrighted work titled “{{WorkTitle}}” (registration: {{RegistrationNumber}}). Such conduct infringes my exclusive rights under Title 17 of the United States Code.

                We have identified infringing material under your control at the following location: {{InfringingUrl}}. {{AdditionalFacts}}

                Demand is hereby made that you immediately: (1) remove all infringing copies and listings; (2) cease any further reproduction, distribution, public display, or derivative uses; and (3) confirm in writing within seven (7) days that you have complied and will refrain from future infringement.

                Be advised that continued infringement may expose you to statutory damages, up to the amounts available under 17 U.S.C. § 504, as well as injunctive relief and recovery of costs and attorneys’ fees where permitted.

                All rights and remedies are reserved.

                Sincerely,

                {{SenderName}}
                {{SenderAddress}}"),

            new LetterTemplatePreset(
                LetterTemplateType.CeaseAndDesist, 
                TrademarkCeaseDesistKey, 
                "Cease & Desist (Trademark)",
                BodyTemplate: @"{{Date}}

                {{RecipientName}}
                {{RecipientAddress}}

                Subject: Cease and Desist – Unauthorized Use of Mark Related to “{{WorkTitle}}” ({{RegistrationNumber}})

                Dear {{RecipientName}},

                It has come to our attention that you are using a mark, logo, name, or other source identifier confusingly similar to our protected mark associated with “{{WorkTitle}}” (registration: {{RegistrationNumber}}). Your use is likely to cause consumer confusion, mistake, or deception as to source, sponsorship, or affiliation, in violation of the Lanham Act (15 U.S.C. §§ 1114, 1125) and applicable law.

                The unauthorized use has been observed in your listings, product packaging, advertising, or related materials at the following location: {{InfringingUrl}}. {{AdditionalFacts}}

                You are required to immediately: (1) remove the infringing branding from all products, listings, and advertisements; (2) cease any further use of the mark or confusingly similar variations; and (3) provide written confirmation within seven (7) days of compliance and your agreement to avoid future misuse.

                Failure to comply may result in legal action seeking injunctive relief, damages, disgorgement of profits, and other remedies.

                All rights are reserved.

                Sincerely,

                {{SenderName}}
                {{SenderAddress}}"),

            new LetterTemplatePreset(
                LetterTemplateType.Dmca, 
                CopyrightDmcaKey, 
                "DMCA Takedown (General)",
                BodyTemplate: @"{{Date}}

                {{RecipientName}}
                {{RecipientAddress}}

                Subject: DMCA Takedown Notice – “{{WorkTitle}}” ({{RegistrationNumber}})

                To the Designated DMCA Agent,
                I am the rights holder (or authorized agent) for the work titled “{WorkTitle}” ({RegistrationNumber}). 
                I request immediate removal of the infringing material at: {InfringingUrl}

                **Work Details:**
                • Title: {WorkTitle} | Registration: {RegistrationNumber}
                • Created: {YearOfCreation}
                • Published: {DateOfPublication} ({NationOfFirstPublication})

                **Contact Information:**
                • Name: {SenderName} | Email: {SenderEmail}
                • Address: {SenderAddress}
                • CC: {RecipientEmail}

                **Good-faith Statement:** {GoodFaithStatement}

                I state under penalty of perjury that I am the copyright owner (or authorized agent), and the information in this notice is accurate. Please notify me once the material is removed.

                Sincerely,
                {{SenderName}}
                {{SenderAddress}}")
        };

        public IReadOnlyList<LetterTemplatePreset> GetLetterTemplatePresets(LetterTemplateType type)
    {
            return Presets.Where(
                p => p.Type == type).ToList();
    }

        public LetterTemplatePreset? GetTemplateByKey(string key)
        {
            return Presets.FirstOrDefault(
                p => string.Equals(
                    p.Key, 
                    key, 
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}
