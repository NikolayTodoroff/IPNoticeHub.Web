using System.ComponentModel.DataAnnotations;

namespace IPNoticeHub.Data.EnumConstants
{
    /// <summary>
    /// Represents the WIPO Nice Classification system, which categorizes international classes for USPTO trademarks.
    /// Classes 1–34 pertain to goods, while classes 35–45 pertain to services.
    /// </summary>
    public enum TrademarkClass
    {
        // Goods
        [Display(Name = "Class 1 – Chemicals")] Chemicals = 1,
        [Display(Name = "Class 2 – Paints")] Paints = 2,
        [Display(Name = "Class 3 – Cosmetics and cleaning preparations")] CosmeticsAndCleaning = 3,
        [Display(Name = "Class 4 – Lubricants and fuels")] LubricantsAndFuels = 4,
        [Display(Name = "Class 5 – Pharmaceuticals")] Pharmaceuticals = 5,
        [Display(Name = "Class 6 – Metal goods")] MetalGoods = 6,
        [Display(Name = "Class 7 – Machinery")] Machinery = 7,
        [Display(Name = "Class 8 – Hand tools")] HandTools = 8,
        [Display(Name = "Class 9 – Electrical and scientific apparatus")] ElectricalAndScientificApparatus = 9,
        [Display(Name = "Class 10 – Medical apparatus")] MedicalApparatus = 10,
        [Display(Name = "Class 11 – Environmental control apparatus")] EnvironmentalControlApparatus = 11,
        [Display(Name = "Class 12 – Vehicles")] Vehicles = 12,
        [Display(Name = "Class 13 – Firearms")] Firearms = 13,
        [Display(Name = "Class 14 – Jewelry")] Jewelry = 14,
        [Display(Name = "Class 15 – Musical instruments")] MusicalInstruments = 15,
        [Display(Name = "Class 16 – Paper goods and printed matter")] PaperAndPrintedMatter = 16,
        [Display(Name = "Class 17 – Rubber goods")] RubberAndPlastics = 17,
        [Display(Name = "Class 18 – Leather goods")] LeatherGoods = 18,
        [Display(Name = "Class 19 – Non-metallic building materials")] BuildingMaterialsNonMetal = 19,
        [Display(Name = "Class 20 – Furniture and articles not otherwise classified")] Furniture = 20,
        [Display(Name = "Class 21 – Housewares and glass")] HousewaresAndGlass = 21,
        [Display(Name = "Class 22 – Cordage and fibers")] RopesCordageAndFibers = 22,
        [Display(Name = "Class 23 – Yarns and threads")] YarnsAndThreads = 23,
        [Display(Name = "Class 24 – Fabrics")] Fabrics = 24,
        [Display(Name = "Class 25 – Clothing")] ClothingFootwearHeadgear = 25,
        [Display(Name = "Class 26 – Fancy good")] LaceRibbonsNotions = 26,
        [Display(Name = "Class 27 – Floor coverings")] CarpetsAndFloorCoverings = 27,
        [Display(Name = "Class 28 – Toys and sporting goods")] ToysAndSportingGoods = 28,
        [Display(Name = "Class 29 – Meats and processed foods")] MeatsAndProcessedFoods = 29,
        [Display(Name = "Class 30 – Staple foods")] StapleFoods = 30,
        [Display(Name = "Class 31 – Natural agricultural products")] NaturalAgriculturalProducts = 31,
        [Display(Name = "Class 32 – Light beverages")] LightBeverages = 32,
        [Display(Name = "Class 33 – Wines and spirits")] WinesAndSpirits = 33,
        [Display(Name = "Class 34 – Smokers’ articles")] SmokersArticles = 34,

        // Services
        [Display(Name = "Class 35 – Advertising and business")] AdvertisingAndBusiness = 35,
        [Display(Name = "Class 36 – Insurance and financial")] InsuranceAndFinancial = 36,
        [Display(Name = "Class 37 – Building construction and repair")] BuildingConstructionAndRepair = 37,
        [Display(Name = "Class 38 – Telecommunications")] Telecommunications = 38,
        [Display(Name = "Class 39 – Transportation and storage")] TransportAndStorage = 39,
        [Display(Name = "Class 40 – Treatment of materials")] TreatmentOfMaterials = 40,
        [Display(Name = "Class 41 – Education and entertainment")] EducationAndEntertainment = 41,
        [Display(Name = "Class 42 – Computer and scientific")] ComputerAndScientific = 42,
        [Display(Name = "Class 43 – Hotels and restaurants")] FoodServicesAndLodging = 43,
        [Display(Name = "Class 44 – Medical, beauty and agricultural")] MedicalBeautyAndAgriculturalServices = 44,
        [Display(Name = "Class 45 – Personal and legal")] LegalAndSecurity = 45
    }
}
