using OpenQA.Selenium;
using SeleniumAutomation.SeleniumObject;

namespace SeleniumAutomation.PageObjects
{
    static class AdditionalWellAttributes
    {
        public static class Categories
        {
            public static By FinanceAndElectric { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(text(), 'Finance and Electric')]", "Finance and Electric Category"); } }
            public static By CongressionalCarterLocation { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(text(), 'Congressional Carter Location')]", "Congressional Carter Location Category"); } }
            public static By PlotInformation { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(text(), 'Plot Information')]", "Plot Information"); } }
            public static By OffshoreInformation { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(text(), 'Offshore Information')]", "Offshore Information"); } }
            public static By Other { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(text(), 'Other')]", "Other"); } }
        }

        #region Numeric and date inputs
        public static By FirstProductionDate { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='First Production Date']//../following-sibling::div//input", "First Production Date"); } }
        public static By AbandonmentDate { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Abandonment Date']//../following-sibling::div//input", "Abandonment Date"); } }
        public static By PTTaxRate { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='PT Tax Rate']//../following-sibling::div//input", "PT Tax Rate"); } }
        public static By CTTaxRate { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='CT Tax Rate']//../following-sibling::div//input", "CT Tax Rate"); } }
        public static By WTTaxRate { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='WT Tax Rate']//../following-sibling::div//input", "WT Tax Rate"); } }
        public static By EstimatedKWHCost { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Estimated kWh Cost']//../following-sibling::div//input", "Estimated KWH Cost"); } }
        public static By EstimatedKWHPowerCost { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Estimated kWh Uninterruptible Power Cost']//../following-sibling::div//input", "Estimated KWH Uninterruptible Power Cost"); } }
        public static By DefaultDecimalWorkingInterest { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Default Decimal Working Interest']//../following-sibling::div//input", "Default Decimal Working Interest"); } }
        public static By DefaultDecimalRoyaltyInterest { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Default Decimal Royalty Interest']//../following-sibling::div//input", "Default Decimal Royalty Interest"); } }
        public static By PlatformElevation { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Platform Elevation (from Mean Sea Level)']//../following-sibling::div//input", "Platform Elevation"); } }
        public static By WaterLineElevation { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Water Line Elevation (from Mean Sea Level)']//../following-sibling::div//input", "Water Line Elevation"); } }
        public static By WaterDepth { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Water Depth']//../following-sibling::div//input", "Water Depth"); } }
        #endregion

        #region Radio button inputs
        public static By PTTaxableStatusYes { get { return SeleniumActions.getByLocator("Xpath", "(//input[@id='welPTTaxableStatus'])[1]", "PT Taxable Status-Yes"); } }
        public static By CTTaxableStatusYes { get { return SeleniumActions.getByLocator("Xpath", "(//input[@id='welCTTaxableStatus'])[1]", "CT Taxable Status-Yes"); } }
        public static By WTTaxableStatusYes { get { return SeleniumActions.getByLocator("Xpath", "(//input[@id='welWTTaxableStatus'])[1]", "WT Taxable Status-Yes"); } }
        public static By DiscoveryWellYes { get { return SeleniumActions.getByLocator("Xpath", "(//input[@id='welDiscovery'])[1]", "Discovery Well-Yes"); } }
        public static By RadioactiveYes { get { return SeleniumActions.getByLocator("Xpath", "(//input[@id='welRadioactive'])[1]", "Radioactive-Yes"); } }
        public static By AutomationYes { get { return SeleniumActions.getByLocator("Xpath", "(//input[@id='welAutomation'])[1]", "Automation-Yes"); } }
        public static By LeaseHeldByProductionYes { get { return SeleniumActions.getByLocator("Xpath", "(//input[@id='welLeaseHeldByProduction'])[1]", "Lease Held By Production-Yes"); } }
        #endregion

        #region Standard text inputs
        public static By TaxCreditCode { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welTaxCreditCode']", "Tax Credit Code"); } }
        public static By TownshipDirection { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welCCLTownshipDirection']", "Township Direction"); } }
        public static By TownshipNumber { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welCCLTownshipNumber']", "Township Number"); } }
        public static By RangeDirection { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welCCLRangeDirection']", "Range Direction"); } }
        public static By RangeNumber { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welCCLRangeNumber']", "Range Number"); } }
        public static By SectionIndicator { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welCCLSectionIndicator']", "Section Indicator"); } }
        public static By SectionNumber { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welCCLSectionNumber']", "Section Number"); } }
        public static By Unit { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welCCLUnit']", "Unit"); } }
        public static By Spot { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welCCLSpot']", "Spot"); } }
        public static By Code { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welCCLCode']", "Code"); } }
        public static By MeridianCode { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welCCLMeridianCode']", "Meridian Code"); } }
        public static By MeridianName { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welCCLMeridianName']", "Meridian Name"); } }
        public static By SurfaceNodeId { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welSurfaceNodeId']", "Surface Node Id"); } }
        public static By District { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welDistrict']", "District"); } }
        public static By AbstractNumber { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welTexAbstractNumber']", "Abstract Number"); } }
        public static By PlotName { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welPlotName']", "Plot Name"); } }
        public static By PlotSymbol { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welPlotSymbol']", "Plot Symbol"); } }
        public static By WellPlat { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welWellPlat']", "Well Plat"); } }
        public static By DirectionToSite { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welDirectionstoSite']", "Direction to Site"); } }
        public static By GovernmentPlatformId { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welGovPlatformId']", "Government Platform ID"); } }
        public static By PlatformId { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welPlatformId']", "Platform ID"); } }
        public static By OCSNumber { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welOffShoreOCSNumber']", "OCS Number"); } }
        public static By BHBPrefix { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welOffShoreBottomHoleBlockPrefix']", "BHB Prefix"); } }
        public static By BHBNumber { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welOffShoreBottomHoleBlockNumber']", "BHB Number"); } }
        public static By BHBSuffix { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welOffShoreBottomHoleBlockSuffix']", "BHB Suffix"); } }
        public static By AreaName { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welOffShoreAreaName']", "Area Name"); } }
        public static By UTMQuadrant { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welOffShoreUTMQuadrant']", "UTM Quadrant"); } }
        public static By WatersIndicator { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welOffShoreWatersIndicator']", "Waters Indicator"); } }
        public static By WaterBottomZone { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welOffShoreWaterBottomZone']", "Water Bottom Zone"); } }
        public static By LongWellName { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welLongWellName']", "Long Well Name"); } }
        public static By RegulatoryAgency { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welRegulatoryAgency']", "Long Well Name"); } }
        public static By LegalDescription { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welLegalDesc']", "Long Well Name"); } }
        public static By LandId { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welLandId']", "Long Well Name"); } }
        public static By Satellite { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welSatellite']", "Long Well Name"); } }
        public static By CommunitizationId { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welCommunitizationId']", "Long Well Name"); } }
        public static By Remarks { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='welRemarks']", "Long Well Name"); } }
        #endregion

        public static By SaveButton { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(), 'Save')]", "Save Button"); } }
        public static By CloseButton { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(@class,'k-dialog-close')]", "Close Dialog Button"); } }
    }
}
