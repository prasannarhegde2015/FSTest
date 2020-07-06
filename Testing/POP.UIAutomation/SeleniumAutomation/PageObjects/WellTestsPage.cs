using OpenQA.Selenium;
using SeleniumAutomation.SeleniumObject;

namespace SeleniumAutomation.PageObjects
{
    class WellTestPage
    {
        public static string strDynamicValue = string.Empty;
        public static By btnCreateNewWellTest { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Create New')]", "Create new Well test"); } }
        // //button[text()=' Yes, Send Command ']
        public static By txtWellTestDate { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-today']", "Well test date"); } }
        public static By txtWellTestDuration
        {
            get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='testDuration']//input", "testDuration"); }
        }

        public static By txtWellTestmonth { get { return SeleniumActions.getByLocator("Xpath", "//kendo-calendar//span[contains(text(),'" + SeleniumActions.getmonthname() + "')]", "Well test month"); } }
        public static By txtWellTestDay
        {
            get { return SeleniumActions.getByLocator("Xpath", "(//tbody)[1]//*[@class='k-link' and text()='" + SeleniumActions.getday(-2) + "']", "welltestday"); }
        }
        public static By WellTestDate
        {
            get { return SeleniumActions.getByLocator("Xpath", "(//*[@col-id='SampleDateJS']//*[@class='cell-layer-1']//span)[2]", "welltestdate"); }
        }

        public static By testdateclndr { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-icon k-i-calendar']", "Well test date calendar"); } }
        public static By kendoddWellTestType { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dropdownlist[@formcontrolname='WellTestType']", "Well test Type"); } }
        public static By kendoddWellTestQCode { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dropdownlist[@formcontrolname='SPTCode']", "Quaility code"); } }
        public static By txtWellTestOilRate { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='oilRate']//input", "oilRate"); } }
        public static By txtWellTestWaterRate { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='waterRate']//input", "waterRate"); } }
        public static By txtWellTestGasRate { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='gasRate']//input", "gasRate"); } }
        public static By txtWellTestOilGravity { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='oilGravity']//input", "oilGravity"); } }
        public static By txtWellTestWaterGravity { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='waterGravity']//input", "waterGravity"); } }
        public static By txtWellTestGasGravity { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='gasGravity']//input", "gasGravity"); } }
        public static By txtWellTestTHP { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='averageTubingPressure']//input", "averageTubingPressure"); } }
        public static By txtWellTestTHT { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='averageTubingTemperature']//input", "averageTubingTemperature"); } }
        public static By txtWellTestCHP { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='averageCasingPressure']//input", "averageCasingPressure"); } }
        public static By txtWellTestFAP { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='averageFluidAbovePump']//input", "averageFluidAbovePump"); } }
        public static By txtWellTestPIP { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='pumpIntakePressure']//input", "pumpIntakePressure"); } }
        public static By txtWellTestSPM { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='strokePerMinute']//input", "strokePerMinute"); } }


        public static By txtWellTestPumpHours { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='pumpingHours']//input", "pumpingHours"); } }

        public static By txtWellTestPumpEffeciency { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='pumpEfficiency']//input", "pumpEfficiency"); } }
        // //span[text()='Communication Status: ']/following-sibling::span[1]

        // 
        public static By lblCommStatusBy { get { return SeleniumActions.getByLocator("Xpath", "//span[text()='Communication Status: ']/following-sibling::span[1]", "CommStaus Label"); } }
        public static By btnWellTestSave { get { return SeleniumActions.getByLocator("Xpath", "//button[text()='Save']", "Well test Save"); } }

        #region welltestgl
        public static By btnqualitycode { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='SPTCode']//*[@class='k-input']", "Quality code dropdown"); } }
        public static By lnkqualitycode { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Allocatable Test')]", "Quality code"); } }
        public static By txtoilrate { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='oilRate']//input", "Oil rate"); } }
        public static By txtwaterrate { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='waterRate']//input", "water rate"); } }
        public static By txtwelltestgasrate { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='gasRate']//input", "gas rate"); } }
        public static By txtwelltestthp { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='averageTubingPressure']//input", "THP"); } }
        public static By txtwelltestthT { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='averageTubingTemperature']//input", "THT"); } }

        public static By txtavgcasingpr { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='averageCasingPressure']//input", "Average casing pressure"); } }
        public static By txtgasinjectionrate { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='gasInjectionRate']//input", "Gas injection rate"); } }

        public static By txtDPG { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='GaugePressure']//input", "Downhole pressure gauge"); } }
        public static By txtflowlinepressure { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='FlowLinePressure']//input", "Flowline pressure"); } }
        public static By txtseparatorpressure { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='SeparatorPressure']//input", "Seperator pressure"); } }
        public static By txtchokesize { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='ChokeSize']//input", "choke size"); } }
        public static string TestDate { get { return "//div[@col-id='SampleDateJS']//span[@class='cell-layer-1']/span[2]"; } }
        public static string TestDuration { get { return "//div[@col-id='TestDuration']//span[@class='cell-layer-1']/span[2]"; } }
        public static string SPTCode { get { return "//div[@col-id='SPTCode']//span[@class='cell-layer-1']/span[2]"; } }
        public static string CalibrationMethod { get { return "//div[@col-id='CalibrationMethod']//span[@class='cell-layer-1']/span[2]"; } }
        public static string TuningStatus { get { return "//div[@col-id='TuningStatus']//span[@class='cell-layer-1']/span[2]"; } }
        public static string Message { get { return "//div[@col-id='Message']//span[@class='cell-layer-1']/span[2]"; } }
        public static string AverageTubingPressure { get { return "//div[@col-id='AverageTubingPressure']//span[@class='cell-layer-1']/span[2]"; } }
        public static string AverageTubingTemperature { get { return "//div[@col-id='AverageTubingTemperature']//span[@class='cell-layer-1']/span[2]"; } }

        public static string AverageCasingPressure { get { return "//div[@col-id='AverageCasingPressure']//span[@class='cell-layer-1']/span[2]"; } }

        public static string GasInjectionRate { get { return "//div[@col-id='GasInjectionRate']//span[@class='cell-layer-1']/span[2]"; } }

        public static string Oil { get { return "//div[@col-id='Oil']//span[@class='cell-layer-1']/span[2]"; } }

        public static string TotalGasRate { get { return "//div[@col-id='TotalGasRate']//span[@class='cell-layer-1']/span[2]"; } }
        public static string Gas { get { return "//div[@col-id='Gas']//span[@class='cell-layer-1']/span[2]"; } }
        public static string Water { get { return "//div[@col-id='Water']//span[@class='cell-layer-1']/span[2]"; } }

        public static string GOR { get { return "//div[@col-id='GOR']//span[@class='cell-layer-1']/span[2]"; } }
        public static string WaterCut { get { return "//div[@col-id='WaterCut']//span[@class='cell-layer-1']/span[2]"; } }
        public static string InjectionGLR { get { return "//div[@col-id='InjectionGLR']//span[@class='cell-layer-1']/span[2]"; } }
        public static string FlowingBottomholePressure { get { return "//div[@col-id='FlowingBottomholePressure']//span[@class='cell-layer-1']/span[2]"; } }

        public static string LFactor { get { return "//div[@col-id='LFactor']//span[@class='cell-layer-1']/span[2]"; } }

        public static string ProductivityIndex { get { return "//div[@col-id='ProductivityIndex']//span[@class='cell-layer-1']/span[2]"; } }

        //div[@col-id='ProductivityIndex']//span[@class='cell-layer-1-uom']/span[1]

        public static string ReservoirPressure { get { return "//div[@col-id='ReservoirPressure']//span[@class='cell-layer-1']/span[2]"; } }
        public static By scrollhorizontalcontainerindex2 { get { return SeleniumActions.getByLocator("Xpath", "(//div[@ref='eBodyHorizontalScrollContainer'])[2]", " Scrollbar container"); } }

        public static By scrollhorizontaindex2 { get { return SeleniumActions.getByLocator("Xpath", "(//div[@ref='eBodyHorizontalScrollViewport'])[2]", " Scrollbar"); } }
        #endregion

        #region welltestesp
        public static By PIP { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='pumpIntakePressure']//input", "Pumpintakepressure"); } }
        public static By Motorvolts { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='motorVolts']//input", "Motorvolts"); } }
        public static By Motoramps { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='motorCurrent']//input", "MotorAmps"); } }
        public static By PDP { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='pumpDischargePressure']//input", "PumpDischargepressure"); } }
        public static By Flowlinepr { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='flowLinePressure']//input", "Flowlinepressure"); } }
        public static By Seperatorpr { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='separatorPressure']//input", "Seperatorpressure"); } }
        public static By Frequency { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='frequency']//input", "Frequency"); } }
        public static string Flowlinepr_2 { get { return "//div[@col-id='FlowLinePressure']//span[@class='cell-layer-1']/span[2]"; } }
        public static string Seppr_2 { get { return "//div[@col-id='SeparatorPressure']//span[@class='cell-layer-1']/span[2]"; } }
        public static string Frequency_2 { get { return "//div[@col-id='Frequency']//span[@class='cell-layer-1']/span[2]"; } }
        public static string motorvolts_2 { get { return "//div[@col-id='MotorVolts']//span[@class='cell-layer-1']/span[2]"; } }
        public static string motoramps_2 { get { return "//div[@col-id='MotorCurrent']//span[@class='cell-layer-1']/span[2]"; } }
        public static string PIP_2 { get { return "//div[@col-id='PumpIntakePressure']//span[@class='cell-layer-1']/span[2]"; } }
        public static string PDP_2 { get { return "//div[@col-id='PumpDischargePressure']//span[@class='cell-layer-1']/span[2]"; } }
        public static string FBHP { get { return "//div[@col-id='FlowingBottomholePressure']//span[@class='cell-layer-1']/span[2]"; } }
        public static string Headtuningfactor { get { return "//div[@col-id='PumpWearFactor']//span[@class='cell-layer-1']/span[2]"; } }
        public static string ChokeSize { get { return "//div[@col-id='ChokeSize']//span[@class='cell-layer-1']/span[2]"; } }

        #endregion

    }
}
