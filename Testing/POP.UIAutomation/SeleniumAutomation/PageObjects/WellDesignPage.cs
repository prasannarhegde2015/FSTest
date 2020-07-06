using OpenQA.Selenium;
using SeleniumAutomation.SeleniumObject;

namespace SeleniumAutomation.PageObjects
{
    class WellDesignPage
    {
        public static string strDynamicValue = string.Empty;
        #region WellDesign
        public static By welldesignTab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Well Design']", "Well Design Tab"); } }
        public static By togglesensitivities { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='well-design-data-input']//kendo-switch", "Sensitivities toggle"); } }
        public static By btnsensitivities { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),' Sensitivities')]", "Sensitivities"); } }
        public static By btnaddsensitivities { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-content k-window-content k-dialog-content']//span[@class='k-icon k-i-plus']", "Add button"); } }

        public static string sensdialogtubingheadpr = "//*[@class='k-content k-window-content k-dialog-content']//kendo-numerictextbox//input";
        public static By btnsensitivitiessave { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog-actions//*[contains(text(),'Save')]", "Save"); } }
        public static By scrollwelldesign { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='well-design-input-outer-container']//*[@class='ps__rail-y']", "well design scrollbar"); } }
        public static string txtgasrate = "//*[@formcontrolname='gasRateInput']//input";
        public static string txtliquidrate = "//*[@formcontrolname='liquidRateInput']//input";
        public static string txtthp = "//*[@formcontrolname='designTHPInput']//input";
        public static string txtdiffpressure = "//*[@formcontrolname='designDiffPressureInput']//input";
        public static string txtbuilduptime = "//*[@formcontrolname='buildUpTimeInput']//input";
        public static string txtafterflowtime = "//*[@formcontrolname='afterFlowTimeInput']//input";
        public static string txtplungertype = "//*[@formcontrolname='plungerTypeInput']//input";
        public static string txtfallrateingas = "//*[@formcontrolname='fallRateInGasInput']//input";
        public static string txtfallrateinliquid = "//*[@formcontrolname='fallRateInLiquidInput']//input";
        public static string txtfidealriserate = "//*[@formcontrolname='idealRiseRateInput']//input";
        public static string txtreqrisepressure = "//*[@formcontrolname='reqRisePressureInput']//input";
        public static string weldsgncolheader = "//kendo-grid//th";
        public static string weldsgncolvalues = "//kendo-grid//td";
        public static By gridwelldesign { get { return SeleniumActions.getByLocator("Xpath", "//kendo-grid//td", "Well design grid"); } }
        public static By btncalculate { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Calculate')]", "Calculate"); } }
        #endregion
        #region welltest
        public static By btncreatewelltest { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Create Well Test ')]", "Create well test"); } }
        public static By btncalendarwelltest { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-icon k-i-calendar']", "welltestdate"); } }

        public static By txttestdate { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-today']", "Testdate"); } }
        public static By txttime { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='sampleTime']//span", "Welltesttime"); } }
        //   public static By txttestdate { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-today']", "Testdate"); } }
        public static By txttestduration { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='testDuration']//input", "Testduration"); } }
        public static By txtflowlinepressure { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='flowLinePressure']//input", "Flow line pressure"); } }

        public static By btnsave { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Save')]", "Save button"); } }
        #endregion
        #region operatinglimits/Sensitivities
        public static By taboptimization { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Optimization')]", "Optimization"); } }
        public static By tabwelltest { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(text(),'Well Test')]", "Well Test"); } }
        public static By txtwelltestdate { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='cell-layer-1']", "Well Test date"); } }
        public static By taboperatingmode { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-link' and contains(text(),'Operating Mode')]", "Operating mode"); } }
        public static By tabsensitivities { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-link' and contains(text(),'Sensitivities')]", "Sensitivities"); } }
        public static By lblcriticalrateus { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Critical Rate (Mscf/d)')]", "critical rate label US"); } }
        public static By lblcriticalratemetric { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Critical Rate (sm³/d)')]", "critical rate label Metric"); } }
        public static By lblcriticalrate { get { return SeleniumActions.getByLocator("Xpath", "(//*[contains(text(),'Critical Rate')])[2]", "critical rate label"); } }
        public static By lblcriticalrateAT10 { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Critical Rate @ 10')]", "critical rate label@10"); } }
        public static By lbldesignrate { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Design Rate')]", "Design rate"); } }

        public static By lblthp { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Tubing Head Pressure (psia)')]", "THP"); } }
        public static By lblthpmetric { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Tubing Head Pressure (kPa)')]", "THP Metric"); } }

        public static By lbltimemin { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Time (min)')]", "Time(min)"); } }
        public static By lblcycletime { get { return SeleniumActions.getByLocator("Xpath", "(//*[contains(text(),'Cycle Time')])[1]", "CycleTime"); } }
        public static By lblgasvolume { get { return SeleniumActions.getByLocator("Xpath", "(//*[contains(text(),'Gas Volume')])[1]", "Gas volume"); } }
        public static By lblnecchp { get { return SeleniumActions.getByLocator("Xpath", "(//*[contains(text(),'Nec. CHP')])[1]", "Nec CHP"); } }
        public static By lblnecThp { get { return SeleniumActions.getByLocator("Xpath", "(//*[contains(text(),'Nec. THP')])[1]", "Nec THP"); } }
        public static By lblslugvolume { get { return SeleniumActions.getByLocator("Xpath", "(//*[contains(text(),'Slug Volume')])[1]", "Slug Volume"); } }
        public static By lblcyclesperday { get { return SeleniumActions.getByLocator("Xpath", "(//*[contains(text(),'Cycles/Day')])[1]", "Cycles/Day"); } }
        public static By lbldailygasrate { get { return SeleniumActions.getByLocator("Xpath", "(//*[contains(text(),'Daily Gas Rate')])[1]", "Daily Gas Rate"); } }
        public static By lbldailyliquidrate { get { return SeleniumActions.getByLocator("Xpath", "(//*[contains(text(),'Daily Gas Rate')])[1]", "Daily Liquid Rate"); } }
        public static By lblgasvolumeus { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Gas Volume (Mscf)')]", "Gas Volume (Mscf)"); } }
        public static By lblslugheightmetric { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Slug Height (m)')]", "Slug height Metric"); } }
        public static By lblgasvolumemetric { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Gas Volume (sm³)')]", "Gas volume Metric"); } }
        #endregion
    }
}
