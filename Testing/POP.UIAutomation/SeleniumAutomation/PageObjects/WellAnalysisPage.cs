using OpenQA.Selenium;
using SeleniumAutomation.SeleniumObject;

namespace SeleniumAutomation.PageObjects
{
    class WellAnalysisPage
    {
        public static string strDynamicValue = string.Empty;

        #region RRLWELL
        public static By btnScanCards { get { return SeleniumActions.getByLocator("Xpath", "//button[@id='collectCards']", "Scan Cards Button"); } }
        public static By lnkCurrentCard { get { return SeleniumActions.getByLocator("Xpath", "//a[@class='dropdown-item' and contains(text(),'Current Card')]", "Current card"); } }
        #endregion
        //  
        #region RRLWell Analysis
        public static By taboptimization { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Optimization')]", "Optimization"); } }
        public static By tabanalysis { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Well Analysis')]", "Analysis"); } }
        public static By txtstartdate { get { return SeleniumActions.getByLocator("Xpath", "(//*[@class='k-dateinput-wrap']//input)[1]", "Analysis start date"); } }
        public static By dynacardoverlayprecard { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='overlay']/div", "Dynacard overlay"); } }

        public static By lnkcardlib { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='historycardsDropDown']", "Cardlibrary"); } }

        public static string lnkcardlibrow = "(//*[@id='cardLibraryGridNG']//table)[2]/tbody/tr";

        public static By txttotalitems { get { return SeleniumActions.getByLocator("Xpath", "(//*[contains(text(),'Total')])[1]", "Total cardlibrary items"); } }
        public static By lnkscancards { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='collectCards']", "Scan cards"); } }
        public static By lnkcontinuouscollection { get { return SeleniumActions.getByLocator("Xpath", "(//*[@data-target='#continousCard'])[1]", "Continuous card"); } }
        public static By lnkcardscollected { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Cards Collected: 20')]", "Continuous card 20"); } }

        public static By txtcollectionintrvl { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='timePeriod']", "Collection interval"); } }
        public static By txtnoofcards { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='noOfCards']", "no. of cards"); } }
        public static By txtcompfails { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='commFails']", "no. of comm fails"); } }
        public static By btnstart { get { return SeleniumActions.getByLocator("Xpath", "//span[contains(text(),'Start')]", "start button"); } }
        public static By divCardCollectionStatus { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Cards Collected:')]", "cards collected"); } }
        public static By divCardCollectioncommfl { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Comms Fail:')]", "Comm fails"); } }
        public static By btnstop { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Stop')]", "Stop button"); } }
        public static By btnclearcards { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='clearCards']", "Clear cards"); } }
        public static By minireportpane { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='miniReportRow']//span", "Mini Report"); } }
        public static By cardlibtable { get { return SeleniumActions.getByLocator("Xpath", "(//*[@id='cardLibraryGridNG']//*[@class='k-grid-table']//tbody/tr/td)[1]", "Card library grid"); } }

        public static By patternmatching { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='patternMatching']", "Pattern matching"); } }
        public static By Analysisoptions { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='analysisOptions']", "Analysis Options"); } }
        public static string patternmatchingrows = "//*[@id='patternMatchingForm']//tbody/tr[@role='row']";
        public static By tabpatternmatchinglib { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='pm-tabstrip-container']//*[@id='k-tabstrip-tab-1']", "Pattern matching"); } }
        public static string patternmatchinglibrows = "(//*[@class='pattern-matching-container']//table)[2]//tr";
        public static By tabpatternmatchingresults { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Results')]", "Pattern matching Results"); } }
        public static By btnsavecollectedcard { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Save Collected')]", "Save button"); } }

        public static By txtsavecarddesc { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='pmsaveCardDescription']", "Card Description"); } }
        public static By btnpatternmatchingsave { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog-actions//button[contains(text(),'Save')]", "Save button"); } }
        public static By btnUpdate { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Update')]", "Update button"); } }
        public static By chkboxsurfacecardcomp { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='comparesurfaceCheckBoxUpdate']", "Compare surface card checkbox"); } }
        public static By chkboxdownholecardcomp { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='comparedownholeCheckBoxUpdate']", "Compare downhole card checkbox"); } }
        public static By chkboxgeneratealarm { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='generateAlarmCheckBoxUpdate']", "Compare downhole card checkbox"); } }
        public static By btnupdatecardlibrary { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog-actions//button[contains(text(),'Update')]", "Update card library"); } }
        public static By btnpatternmatchingdelete { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Delete')]", "Delete button"); } }

        public static By txtcarddeletecfm { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(text(),'Are you sure')]", "Card delete model text"); } }
        public static By txtcardname { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog//*[contains(text(),'Samplecardcollected')]", "Card name"); } }

        public static By btncarddelete { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog//*[contains(text(),'Save')]", "Card name"); } }
        public static By btnrunanalysis { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(@id, 'runAnalysis')]", "Run Analysis"); } }
        public static By btnrunanalysiscfm { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog-actions//button[contains(text(),'Yes')]", "Run Analysis confirm"); } }
        public static By lblminireport { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='miniReportRow']/div", "Minireport data"); } }

        #endregion

        #region RRLwelltest
        public static By tabwelltest { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(text(),'Well Test')]", "Well Test"); } }



        #endregion

        #region Scancards options
        public static By lnkcurrentcard { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Current Card')]", "Current card"); } }
        public static By lnkfullcard { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Full Card')]", "Full card"); } }
        public static By lnkpumpoffcard { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Pumpoff Card')]", "Pumpoff card"); } }

        public static By lnkalarmcard { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Alarm Card')]", "Alarm card"); } }
        public static By lnkfailurecard { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Failure Card')]", "Failure card"); } }
        #endregion

        #region RRLWellAnalysisreport
        public static By btnanalysisreport { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='analysisReportBtn']", "Analysis Report"); } }
        public static By btnanalysisreportdownload { get { return SeleniumActions.getByLocator("Xpath", "//button[@id='rrlAnalysisReport']", "Analysis Report Download"); } }
        public static By analysisreportgeneral { get { return SeleniumActions.getByLocator("Xpath", "(//*[contains(@class, 'ar-general')])[1]", "Analysis Report General information"); } }



        #endregion

        #region calibrationcard
        public static By btncalibrate { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='calibrateBtn']", "Calibrate button"); } }
        public static By txtpumpfilage { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='pumpFillage']", "Pump Filage"); } }
        public static By btnget { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Get')]", "Get Button"); } }
        public static By lblcalibrationsurface { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Calibration: Surface')]", "Calibration Surface label"); } }
        public static By lblcalibrationdownhole { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Calibration: Controller Downhole')]", "Calibration downhole label"); } }
        #endregion

        #region showhide
        public static By btnshowhide { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='dynagraphShowHide']", "Show hide button"); } }
        public static By chkboxcardrangeslider { get { return SeleniumActions.getByLocator("Xpath", "//li[contains(text(),'Card Range Slider')]//input", "Card Range Slider"); } }

        public static By btnrightcard { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='rightCardClick']", "Next card button"); } }
        public static By btnleftCard { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='leftCardClickButton']", "Next card button"); } }

        public static By pumpfillageper { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='dynagraphViewerKendoChart']//*[contains(text(),'0%')]", "Pump Fillage percentage"); } }

        public static By vsdtoleranceper { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='dynagraphViewerKendoChart']//*[contains(text(),'8%')]", "VSD Tolerance percentage"); } }


        public static By chkboxgridlines { get { return SeleniumActions.getByLocator("Xpath", "(//*[@id='showHideAnalysisOption']//input)[1]", "Grid Lines"); } }
        public static By chkboxpoints { get { return SeleniumActions.getByLocator("Xpath", "(//*[@id='showHideAnalysisOption']//input)[2]", "Points"); } }
        public static By chkboxcursorposition { get { return SeleniumActions.getByLocator("Xpath", "(//*[@id='showHideAnalysisOption']//input)[3]", "Cursor Position"); } }
        public static By chkboxpumpfillage { get { return SeleniumActions.getByLocator("Xpath", "(//*[@id='showHideAnalysisOption']//input)[5]", "Pump Fillage"); } }
        public static By chkboxvsdtolerance { get { return SeleniumActions.getByLocator("Xpath", "(//*[@id='showHideAnalysisOption']//input)[6]", "VSD Tolerance"); } }
        public static By chkboxshutdownlimits { get { return SeleniumActions.getByLocator("Xpath", "(//*[@id='showHideAnalysisOption']//input)[7]", "Shutdown limits"); } }
        public static By chkboxfluidloadlines { get { return SeleniumActions.getByLocator("Xpath", "(//*[@id='showHideAnalysisOption']//input)[8]", "Fluid load lines"); } }
        public static By chkboxcomments { get { return SeleniumActions.getByLocator("Xpath", "(//*[@id='showHideAnalysisOption']//input)[11]", "Comments"); } }
        public static By chkboxadjusttoolbar { get { return SeleniumActions.getByLocator("Xpath", "(//*[@id='showHideAnalysisOption']//input)[12]", "Adjust toolbar"); } }
        public static By shutdownlimitshighhigh { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='dynagraphViewerKendoChart']//*[contains(text(),'High High 25000')]", "Shut down limit High High"); } }
        public static By shutdownlimitshigh { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='dynagraphViewerKendoChart']//*[contains(text(),'High 22000')]", "Shut down limit High"); } }

        public static By shutdownlimitslow { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='dynagraphViewerKendoChart']//*[contains(text(),'Low 7000')]", "Shut down low"); } }
        public static By shutdownlimitslowlow { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='dynagraphViewerKendoChart']//*[contains(text(),'Low Low 6000')]", "Shut down low low"); } }
        public static By foup { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='dynagraphViewerKendoChart']//*[contains(text(),'Fo Up')]", "Fo Up"); } }

        public static By fomax { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='dynagraphViewerKendoChart']//*[contains(text(),'Fo Max')]", "Fo Max"); } }
        public static By fodown { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='dynagraphViewerKendoChart']//*[contains(text(),'Fo Down')]", "Fo Down"); } }
        public static By txtareacomment { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='wcTextArea']", "Comments Text Area"); } }
        public static By btnsubmit { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Submit')]", "Comments Submit"); } }
        public static By btndelete { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='Delete']", "Delete button"); } }
        public static By txtcommentdelete { get { return SeleniumActions.getByLocator("Xpath", "//*[@name='commentDeletion']//*[@class='modal-sub-header']", "Delete text"); } }
        public static By btndeletecfm { get { return SeleniumActions.getByLocator("Xpath", "//*[@name='commentDeletion']//button[contains(text(),'Yes')]", "Delete confirm"); } }
        public static By btncardrotateplus { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='glyphicon glyphicon-plus']", "Card rotate plus"); } }

        public static By btncardrotateminus { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='glyphicon glyphicon-minus']", "Card rotate plus"); } }
        public static By txtrotatecard { get { return SeleniumActions.getByLocator("Xpath", "//*[@name='txtRotateCard']", "Card rotate"); } }

        #endregion
        #region unitbalancing
        public static By btnunitbalancing { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='unitBalancing']", "Unit Balancing"); } }
        public static By txtCBT { get { return SeleniumActions.getByLocator("Xpath", "//*[@name='optimalCBTVal']", "CBT"); } }
        public static By btncalculatecbt { get { return SeleniumActions.getByLocator("Xpath", "//*[@name='getUnitBal']", "Calculate CBT"); } }
        public static By lblunitbalancing { get { return SeleniumActions.getByLocator("Xpath", "//*[@label-default='Unit Balancing Details']", "Unit Balancing Details label"); } }

        public static IWebElement tbl { get { return SeleniumActions.getElement("Xpath", "(//*[@id='saveBalModal']//table)[1]", "navFrame"); } }

        public static IWebElement tblcardlib { get { return SeleniumActions.getElement("Xpath", "(//*[@id='cardLibraryGridNG']//table)[2]", "card library"); } }


        public static By lnkcardrangeslider { get { return SeleniumActions.getByLocator("Xpath", "//*[@name='points']", "Card Range Slider"); } }
        public static By lnkcardtypeandtimestamp { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='carddesc1']/span[1]", "Card Range Slider"); } }
        public static By lnkcardtypetext { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='carddesc1']/span[2]", "Card Range Slider"); } }

        public static string tblexistcrank1leadlag = "(//*[@id='saveBalModal']//table)[1]";
        public static string tblexistcrank1prim = "(//*[@id='saveBalModal']//table)[2]";
        public static string tblexistcrank1aux = "(//*[@id='saveBalModal']//table)[3]";
        public static string tblexistcrank2leadlag = "(//*[@id='saveBalModal']//table)[4]";
        public static string tblexistcrank2prim = "(//*[@id='saveBalModal']//table)[5]";
        public static string tblexistcrank2aux = "(//*[@id='saveBalModal']//table)[6]";
        public static string tbldescrank1leadlag = "(//*[@id='saveBalModal']//table)[7]";
        public static string tbldescrank1prim = "(//*[@id='saveBalModal']//table)[8]";
        public static string tbldescrank1aux = "(//*[@id='saveBalModal']//table)[9]";
        public static string tbldescrank2leadlag = "(//*[@id='saveBalModal']//table)[10]";
        public static string tbldescrank2prim = "(//*[@id='saveBalModal']//table)[11]";
        public static string tbldescrank2aux = "(//*[@id='saveBalModal']//table)[12]";



        #endregion

        #region Analysisreport
        public static string lblpumpingunit = "//*[contains(text(),'Pumping Unit:')]";
        public static string lblpumpperiod = "//*[contains(text(),'Pump Period:')]";
        #endregion

        #region GLWell
        public static By toggleWelltest { get { return SeleniumActions.getByLocator("Xpath", "(//*[@class='k-switch-label-on'])[1]", "Well test toggle"); } }
        public static By drpdowntest { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='selectedTestDropdown']//*[@class='k-input']/div", "Well test date"); } }
        public static By qualitycode { get { return SeleniumActions.getByLocator("Xpath", "(//*[@class='form-control ng-untouched ng-pristine ng-valid'])[2]", "Quality code"); } }
        public static By lfactor { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='lFactorInput']//input", "lfactor"); } }
        public static By pressure { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='wellheadPresureInput']//input", "pressure"); } }
        public static By temperature { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='wellheadTemperatureInput']//input", "temperature"); } }
        public static By casingheadpressure { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='casingheadPresureInput']//input", "casing here pressure"); } }
        public static By gasinjection { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='gasInjectionRateInput']//input", "gas injection"); } }
        public static By oilrate { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='oilRateInput']//input", "oilrate"); } }
        public static By waterrate { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='waterRateInput']//input", "oilrate"); } }
        public static By gasrate { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='producedGasRateInput']//input", "gasrate"); } }
        public static By liquidrate { get { return SeleniumActions.getByLocator("Xpath", "(//*[@class='form-control glawb-calculated-field-uom'])[1]", "liquidrate"); } }
        public static By formationgas { get { return SeleniumActions.getByLocator("Xpath", "(//*[@class='form-control glawb-calculated-field-uom'])[2]", "formationgas"); } }
        public static By formationgor { get { return SeleniumActions.getByLocator("Xpath", "(//*[@class='form-control glawb-calculated-field-uom'])[3]", "formationgor"); } }
        //public static By watercut { get { return SeleniumActions.getByLocator("Xpath", "(//*[@class='padding-zero-level glawb-flex-item-35'])[6]", "watercut"); } }
        public static By watercut { get { return SeleniumActions.getByLocator("Xpath", "(//label[text()='Water Cut']/ancestor::div/following-sibling::div[2]/descendant::div[2])[1]", "watercut"); } }

        public static By productivityindex { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='prodIndexInput']//input", "productivityindex"); } }

        #endregion

        #region Dailyaveragedata
        public static By dailyavgsbhp { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='sbhpInput']//input", "SBHP"); } }
        public static By dailyavgpi { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='prodIndexInput']//input", "PI"); } }
        public static By dailyavgstartnode { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='glawbCurrStartNodeDropdown']//*[@class='k-input']", "Start node"); } }
        public static By dailyavgsolutionnode { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='glawbCurrSolutionNodeDropdown']//*[@class='k-input']", "Solution node"); } }
        public static By dailyavgFBHP { get { return SeleniumActions.getByLocator("Xpath", "(//label[text()='Flowing Bottom Hole Pressure']/ancestor::div/following-sibling::div[2]/descendant::div[2])[1]", "fbhp"); } }
        public static By dailyavgsolutionnodepressure { get { return SeleniumActions.getByLocator("Xpath", "(//*[contains(@class,'glawb-calculated-field-uom')])[5]", "Solution node pressure"); } }

        public static By dailyavggasinjdepth { get { return SeleniumActions.getByLocator("Xpath", "(//label[text()='Gas Injection Depth']/ancestor::div/following-sibling::div[2])[1]/descendant::input", "Gas injection depth"); } }

        public static By dailyavgliquidrateipr { get { return SeleniumActions.getByLocator("Xpath", "(//label[contains(text(),'Liquid Rate (from IPR)')]/ancestor::div/following-sibling::div[2])[1]/descendant::div[contains(@class,'glawb-calculated-field-uom')]", "Liquid rate from ipr"); } }
        public static By dailyavgdepth { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='glawbFixedDepthDropdown']//*[@class='k-input']", "Daily avg depth"); } }
        public static By dailyavgdownholegauge { get { return SeleniumActions.getByLocator("Xpath", "(//*[@id='glawbMethodDDUOM'])[3]", "downhole gauge"); } }
        public static By dailyavjinjectionmethod { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='glawbMethodDD']//*[contains(text(),'Deepest Mandrel')]", "injection method"); } }
        public static By dailyavjdepth { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='glawbMethodDDUOM']//*[@class='k-input']", "depth"); } }
        public static By dailyavgmultiphaseflowcorrelation { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='glawbCurrMPFCDropdown']//*[@class='k-input']", "Multiphaseflowcorrelation"); } }
        public static By dailyavgheadtuningfactor { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='headTuningFactorInput']//input", "HeadTuningFactor"); } }
        public static By dailyavgpip { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='pumpIntakePressureInput']//input", "Pumpintakepressure"); } }
        public static By dailyavgpdp { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='pumpDischargePressureInput']//input", "Pumpdischargepressure"); } }
        public static By dailyavgfrequency { get { return SeleniumActions.getByLocator("Xpath", "//*[@formcontrolname='frequencyInput']//input", "Frequency"); } }
        #endregion

        #region GLCurves
        public static By drpgradientcurves { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='ddl_awbCharts']//*[@class='k-input']", "Gradientcurves"); } }
        public static By gradientcurves { get { return SeleniumActions.getByLocator("Xpath", "//li[text()='Gradient Curves']", "Gradientcurves"); } }
        public static By gradientdepth { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Depth (ft)')]", "Gradientdepth"); } }
        public static By gradienttemperature { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Temperature (°F)']", "GradientTemperature"); } }
        public static By gradientpressure { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Pressure (psia)')]", "Gradientpressure"); } }
        public static By gradienttubtemp { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),'Tubing Head Temperature')]", "GradientTubingtemperature"); } }
        public static By gradienttubpressure { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Tubing Head Pressure']", "GradientTubingpressure"); } }
        public static By gradientcaspressure { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Casing Head Pressure']", "Gradientcasingpressure"); } }
        public static By gradientmidperfdepth { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='MidPerf Depth (MD)']", "Midperfdepth"); } }
        public static By gradientcasingpressuretoopen { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Casing Pressure to Open']", "Casingpressuretoopen"); } }
        public static By gradienttubpressuretoopen { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Tubing Head Pressure to Open']", "Tubingpressuretoopen"); } }
        public static By gradientmeasureddepthtoggle { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Measured Depth']/following-sibling::div/descendant::span[@class='k-switch-handle']", "Measureddepthtoggle"); } }

        public static By performancecurves { get { return SeleniumActions.getByLocator("Xpath", "//li[text()='Performance Curves']", "Performancecurves"); } }

        public static By prfcurvqlstb { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Ql (STB/d)']", "Ql (STB/d)"); } }
        public static By prfcurvqgi { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Qgi (Mscf/d)']", "Qgi (Mscf/d)"); } }
        public static By prfcurvqostb { get { return SeleniumActions.getByLocator("Xpath", " //*[text()='Qo (STB/d)']", "Qo (STB/d)"); } }

        public static By prfcurvval1 { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='91.19 psia']", "91.19 psia"); } }
        public static By prfcurvval2 { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='95.99 psia']", "95.99 psia"); } }
        public static By prfcurvval3 { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='100.79 psia']", "100.79 psia"); } }
        public static By prfcurvwelltestpoint { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Well Test Point']", "Welltestpoint"); } }
        public static By prfcurvliqprodntoggle { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Liquid Production']/following-sibling::div/descendant::span[@class='k-switch-handle']", "Liquid Production toggle"); } }

        public static By InflowOutflowcurves { get { return SeleniumActions.getByLocator("Xpath", "//li[text()='Inflow/Outflow Curves']", "InflowOutflowcurves"); } }

        public static By iocrvBHP { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='BHP (psia)']", "BHP (psia)"); } }
        public static By iocrvQL { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Ql (STB/d)']", "Ql (STB/d)"); } }
        public static By iofcurvinflow { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Inflow']", ""); } }
        public static By iofcurvoutflow { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Outflow']", "Outflow"); } }
        public static By iocurvOpeartingPoint { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Operating Point']", "OpeartingPoint"); } }
        public static By iocurvbubblepoint { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Bubble Point']", "Bubble Point"); } }
        public static By iocurvqtech { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Q Tech Min-Max']", "Q Tech Min-Max"); } }

        public static By iocrvQo { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Qo (STB/d)']", "Qo (STB/d)"); } }

        public static By WellboreProfile { get { return SeleniumActions.getByLocator("Xpath", "//li[text()='Wellbore Profile']", "Wellbore Profile"); } }

        public static By wpdepthft { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Depth (ft)']", "Depth(ft)"); } }
        public static By wpvelocityfts { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Velocity (ft/s)']", "Velocity(fts)"); } }

        public static By wpcriticunvelocity { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Critical Unloading Velocity']", "Critical Unloading Velocity"); } }
        public static By wpgasinsitvelocity { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Gas Insitu Velocity']", "Gas Insitu Velocity"); } }
        public static By wperosionalvelocity { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Erosional Velocity']", "Erosional velocity"); } }



        #endregion

        #region ESPCurves
        public static By prfpressure { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Pressure (psia)']", "Pressure"); } }
        public static By prfof { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Operating Frequency: 20']", "Operating frequency"); } }
        public static By iocrvPIP { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='PIP (psia)']", "PIP (psia)"); } }

        public static By Gassinesscurves { get { return SeleniumActions.getByLocator("Xpath", "//li[text()='Gassiness Curves']", "Gassiness Curves"); } }
        public static By Gcrvpip { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Pump Intake Pressure (psia)']", "Pump intake pressure"); } }
        public static By Gcrvvlr { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='In-Situ Vapor Liquid Ratio (fraction)']", "Vapor liquid ratio"); } }
        public static By Gcrvlgc { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Lower Gassiness Curve']", "Lower gassiness curve"); } }
        public static By Gcrvugc { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Upper Gassiness Curve']", "Upper gassiness curve"); } }

        public static By Pumpcurves { get { return SeleniumActions.getByLocator("Xpath", "//li[text()='Pump Curves']", "Pump Curves"); } }

        public static By wellcrv { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Well Curve']", "Well curve"); } }
        public static By Head { get { return SeleniumActions.getByLocator("Xpath", "(//*[text()='Head'])[2]", "Head"); } }
        public static By Power { get { return SeleniumActions.getByLocator("Xpath", "(//*[text()='Power'])[2]", "Power"); } }
        public static By Efficiency { get { return SeleniumActions.getByLocator("Xpath", "(//*[text()='Efficiency'])[2]", "Efficiency"); } }
        public static By Minimumrate { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Minimum Rate']", "Minimumrate"); } }
        public static By Maximumrate { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Maximum Rate']", "Minimumrate"); } }
        public static By BestEfficiencyrate { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Best Efficiency Rate']", "Bestefficiencyrate"); } }
        public static By Headft { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Head (ft)']", "Head(ft)"); } }
        public static By Pumphorespower { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Pump Horespower (hp)']", "Pump Horespower"); } }
        public static By PumpEfficiency { get { return SeleniumActions.getByLocator("Xpath", "//*[text()='Pump Efficiency']", "Pump Efficiency"); } }
        public static By chkboxpower { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-checkbox-label' and text()='Power']", "Power checkbox"); } }
        public static By chkboxEfficiency { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-checkbox-label' and text()='Efficiency']", "Efficiency checkbox"); } }
        public static By chkboxHead { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-checkbox-label' and text()='Head']", "Head checkbox"); } }
        public static By chkboxWell { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-checkbox-label' and text()='Well']", "Well checkbox"); } }

        public static By chkboxHeadsensitivity { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-checkbox-label' and text()='Head Sensitivity']", "Head Sensitivity"); } }
        public static By chkboxPowersensitivity { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-checkbox-label' and text()='Power Sensitivity']", "Power Sensitivity"); } }
        public static By chkboxEfficiencysensitivity { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-checkbox-label' and text()='Efficiency Sensitivity']", "Efficiency Sensitivity"); } }
        #endregion
    }
}
