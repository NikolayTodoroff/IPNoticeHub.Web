using System.ComponentModel.DataAnnotations;

namespace IPNoticeHub.Shared.Enums
{
    public enum TrademarkClass
    {
        [Display(Name = "Class 001 – Chemicals")] Chemicals = 1,
        [Display(Name = "Class 002 – Paints")] Paints = 2,
        [Display(Name = "Class 003 – Cosmetics and cleaning preparations")] CosmeticsAndCleaning = 3,
        [Display(Name = "Class 004 – Lubricants and fuels")] LubricantsAndFuels = 4,
        [Display(Name = "Class 005 – Pharmaceuticals")] Pharmaceuticals = 5,
        [Display(Name = "Class 006 – Metal goods")] MetalGoods = 6,
        [Display(Name = "Class 007 – Machinery")] Machinery = 7,
        [Display(Name = "Class 008 – Hand tools")] HandTools = 8,
        [Display(Name = "Class 009 – Electrical and scientific apparatus")] ElectricalAndScientificApparatus = 9,
        [Display(Name = "Class 010 – Medical apparatus")] MedicalApparatus = 10,
        [Display(Name = "Class 011 – Environmental control apparatus")] EnvironmentalControlApparatus = 11,
        [Display(Name = "Class 012 – Vehicles")] Vehicles = 12,
        [Display(Name = "Class 013 – Firearms")] Firearms = 13,
        [Display(Name = "Class 014 – Jewelry")] Jewelry = 14,
        [Display(Name = "Class 015 – Musical instruments")] MusicalInstruments = 15,
        [Display(Name = "Class 016 – Paper goods and printed matter")] PaperAndPrintedMatter = 16,
        [Display(Name = "Class 017 – Rubber goods")] RubberAndPlastics = 17,
        [Display(Name = "Class 018 – Leather goods")] LeatherGoods = 18,
        [Display(Name = "Class 019 – Non-metallic building materials")] BuildingMaterialsNonMetal = 19,
        [Display(Name = "Class 020 – Furniture and articles not otherwise classified")] Furniture = 20,
        [Display(Name = "Class 021 – Housewares and glass")] HousewaresAndGlass = 21,
        [Display(Name = "Class 022 – Cordage and fibers")] RopesCordageAndFibers = 22,
        [Display(Name = "Class 023 – Yarns and threads")] YarnsAndThreads = 23,
        [Display(Name = "Class 024 – Fabrics")] Fabrics = 24,
        [Display(Name = "Class 025 – Clothing")] ClothingFootwearHeadgear = 25,
        [Display(Name = "Class 026 – Fancy good")] LaceRibbonsNotions = 26,
        [Display(Name = "Class 027 – Floor coverings")] CarpetsAndFloorCoverings = 27,
        [Display(Name = "Class 028 – Toys and sporting goods")] ToysAndSportingGoods = 28,
        [Display(Name = "Class 029 – Meats and processed foods")] MeatsAndProcessedFoods = 29,
        [Display(Name = "Class 030 – Staple foods")] StapleFoods = 30,
        [Display(Name = "Class 031 – Natural agricultural products")] NaturalAgriculturalProducts = 31,
        [Display(Name = "Class 032 – Light beverages")] LightBeverages = 32,
        [Display(Name = "Class 033 – Wines and spirits")] WinesAndSpirits = 33,
        [Display(Name = "Class 034 – Smokers’ articles")] SmokersArticles = 34,

        [Display(Name = "Class 035 – Advertising and business")] AdvertisingAndBusiness = 35,
        [Display(Name = "Class 036 – Insurance and financial")] InsuranceAndFinancial = 36,
        [Display(Name = "Class 037 – Building construction and repair")] BuildingConstructionAndRepair = 37,
        [Display(Name = "Class 038 – Telecommunications")] Telecommunications = 38,
        [Display(Name = "Class 039 – Transportation and storage")] TransportAndStorage = 39,
        [Display(Name = "Class 040 – Treatment of materials")] TreatmentOfMaterials = 40,
        [Display(Name = "Class 041 – Education and entertainment")] EducationAndEntertainment = 41,
        [Display(Name = "Class 042 – Computer and scientific")] ComputerAndScientific = 42,
        [Display(Name = "Class 043 – Hotels and restaurants")] FoodServicesAndLodging = 43,
        [Display(Name = "Class 044 – Medical, beauty and agricultural")] MedicalBeautyAndAgriculturalServices = 44,
        [Display(Name = "Class 045 – Personal and legal")] LegalAndSecurity = 45
    }
}
