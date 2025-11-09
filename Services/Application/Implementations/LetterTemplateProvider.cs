using IPNoticeHub.Services.Application.Abstractions;
using IPNoticeHub.Common.EnumConstants;

namespace IPNoticeHub.Services.Application.Implementations
{
    public sealed class LetterTemplateProvider : ILetterTemplateProvider
    {
        private static readonly List<LetterTemplatePreset> Presets = new List<LetterTemplatePreset>()
        {
            // --- Cease & Desist (General) ---

            new LetterTemplatePreset(LetterTemplateType.CeaseDesist,"CND-General", "Cease & Desist (General)",
                BodyTemplate: @"{{Date}}

                {{RecipientName}}
                {{RecipientAddress}}

                Subject: Cease and Desist – Unauthorized Use of “{{WorkTitle}}” ({{RegistrationNumber}})

                Dear {{RecipientName}},

                I am writing to notify you that your activities involving “{{WorkTitle}}” (registration: {{RegistrationNumber}}) are unauthorized and infringe upon rights I control. This notice demands that you cease and desist from any further use, display, distribution, advertising, or sale related to the Work.

                We identified the unauthorized use at the location(s) under your control. {{AdditionalFacts}}

                You are required to immediately remove the infringing material, disable any listings or advertisements, and refrain from re-posting or otherwise using the Work without written permission.

                Please provide written confirmation within seven (7) days from the date of this letter that the infringing material has been removed and that you will refrain from further unauthorized use. Failure to comply may result in pursuit of all available legal remedies, including injunctive relief and damages.

                Nothing in this letter constitutes a waiver of any rights or remedies, all of which are expressly reserved.

                Sincerely,

                {{SenderName}}
                {{SenderAddress}}")
            ,
            // --- Cease & Desist (Copyright) ---

            new LetterTemplatePreset (LetterTemplateType.CeaseDesist, "CND-Copyright", "Cease & Desist (Copyright)",
                BodyTemplate: @"{{Date}}

                {{RecipientName}}
                {{RecipientAddress}}

                Subject: Cease and Desist – Copyright Infringement of “{{WorkTitle}}” ({{RegistrationNumber}})

                Dear {{RecipientName}},

                This letter concerns your unauthorized use, reproduction, distribution, and/or public display of the copyrighted work titled “{{WorkTitle}}” (registration: {{RegistrationNumber}}). Such conduct infringes my exclusive rights under Title 17 of the United States Code.

                We have identified infringing material under your control. {{AdditionalFacts}}

                Demand is hereby made that you immediately: (1) remove all infringing copies and listings; (2) cease any further reproduction, distribution, public display, or derivative uses; and (3) confirm in writing within seven (7) days that you have complied and will refrain from future infringement.

                Be advised that continued infringement may expose you to statutory damages, up to the amounts available under 17 U.S.C. § 504, as well as injunctive relief and recovery of costs and attorneys’ fees where permitted.

                All rights and remedies are reserved.

                Sincerely,

                {{SenderName}}
                {{SenderAddress}}"),
            
            // --- Cease & Desist (Trademark) ---

            new(LetterTemplateType.CeaseDesist, "CND-Trademark", "Cease & Desist (Trademark)",
                BodyTemplate: @"{{Date}}

                {{RecipientName}}
                {{RecipientAddress}}

                Subject: Cease and Desist – Unauthorized Use of Mark Related to “{{WorkTitle}}” ({{RegistrationNumber}})

                Dear {{RecipientName}},

                It has come to our attention that you are using a mark, logo, name, or other source identifier confusingly similar to our protected mark associated with “{{WorkTitle}}” (registration: {{RegistrationNumber}}). Your use is likely to cause consumer confusion, mistake, or deception as to source, sponsorship, or affiliation, in violation of the Lanham Act (15 U.S.C. §§ 1114, 1125) and applicable law.

                The unauthorized use has been observed in your listings, product packaging, advertising, or related materials. {{AdditionalFacts}}

                You are required to immediately: (1) remove the infringing branding from all products, listings, and advertisements; (2) cease any further use of the mark or confusingly similar variations; and (3) provide written confirmation within seven (7) days of compliance and your agreement to avoid future misuse.

                Failure to comply may result in legal action seeking injunctive relief, damages, disgorgement of profits, and other remedies.

                All rights are reserved.

                Sincerely,

                {{SenderName}}
                {{SenderAddress}}"),
            
            // --- DMCA (General) ---

            new(LetterTemplateType.Dmca, "DMCA-General", "DMCA Takedown (General)",
                BodyTemplate: @"{{Date}}

                {{RecipientName}}
                {{RecipientAddress}}

                **Subject:** DMCA Takedown Notice – “{{WorkTitle}}” ({{RegistrationNumber}})

                To the Designated DMCA Agent,

                I am the rights holder (or authorized agent) for the copyrighted work titled “{{WorkTitle}}” (registration: {{RegistrationNumber}}).  
                I request **immediate removal or disabling of access** to material that infringes my rights, identified at the following URL or location under your control:  
                {{InfringingUrl}}

                **Work information:**
                • Title: {{WorkTitle}}  
                • Registration Number: {{RegistrationNumber}}  
                • Year of Creation: {{YearOfCreation}}  
                • Date of Publication: {{DateOfPublication}}  
                • Nation of First Publication: {{NationOfFirstPublication}}

                **Contact information for this notice:**
                • Name: {{SenderName}}  
                • Email: {{SenderEmail}}  
                • Address: {{SenderAddress}}  
                • Additional recipient email (if applicable): {{RecipientEmail}}

                **Good-faith statement:**  
                {{GoodFaithStatement}}

                I state under **penalty of perjury** that I am the copyright owner or authorized to act on the owner’s behalf, and that the information in this notice is accurate.

                Please notify me when the material has been removed or disabled.  
                Thank you for your prompt attention.

                **Sincerely,**  
                {{SenderName}}  
                {{SenderAddress}}
                ")
        };

        public IReadOnlyList<LetterTemplatePreset> GetLetterTemplatePresets(LetterTemplateType type)
    {
            return Presets.Where(p => p.Type == type).ToList();
    }

        public LetterTemplatePreset? GetTemplateByKey(string key)
        {
            return Presets.FirstOrDefault(p => string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase));
        }
    }
}
