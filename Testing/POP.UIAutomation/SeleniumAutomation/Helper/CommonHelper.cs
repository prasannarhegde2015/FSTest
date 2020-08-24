
using AventStack.ExtentReports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Xml;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;


namespace SeleniumAutomation.Helper
{
    public static class CommonHelper 
    {
        public static string IsRunningInATS = ConfigurationManager.AppSettings.Get("IsRunningInATS");
        public static string Exports = ConfigurationManager.AppSettings.Get("Exports");
        public static string CurrentUserId = ConfigurationManager.AppSettings.Get("CurrentUserId");
        public static string CygnetFaility = ConfigurationManager.AppSettings.Get("CygNetFacility");
        public static string CygnetFailityGL = ConfigurationManager.AppSettings.Get("CygNetFacilityGL");
        public static string CygnetFailityESP = ConfigurationManager.AppSettings.Get("CygNetFacilityESP");
        public static string CygnetFailityNFW = ConfigurationManager.AppSettings.Get("CygNetFacilityNFW");
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        public static string WellName = ConfigurationManager.AppSettings.Get("WellName");
        public static APIUITestBase ApiUITestBase = new APIUITestBase();

        private static long _reportId;

        public static long reportId
        {
            get { return _reportId; }
            set { _reportId = value; }
        }

        public static string AssetName { get; set; }


        public static void PrintScreen(string filename, int screenNum = 0)
        {
            int i = 0;
            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                Bitmap screenshot = new Bitmap(screen.Bounds.Width, screen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                // Create a graphics object from the bitmap
                Graphics gfxScreenshot = Graphics.FromImage(screenshot);
                // Take the screenshot from the upper left corner to the right bottom corner
                gfxScreenshot.CopyFromScreen(
                    screen.Bounds.X,
                    screen.Bounds.Y,
                    0,
                    0,
                    screen.Bounds.Size,
                    CopyPixelOperation.SourceCopy);
                // Save the screenshot
                if (screenNum == i)
                {
                    screenshot.Save(Exports + "\\" + filename + ".jpg", ImageFormat.Jpeg);
                    i++;
                }
            }

        }

        public static void ReturnDailyAverageData(WellTypeId wType)
        {
            var allWells = ApiUITestBase.WellService.GetAllWells().ToList();
            WellDTO well = null;
            switch (wType)
            {
                case WellTypeId.GLift:
                    {
                        well = allWells?.FirstOrDefault(w => w.Name.Contains("GL"));

                        break;
                    }
                case WellTypeId.ESP:
                    {
                        well = allWells?.FirstOrDefault(w => w.Name.Contains("ESP"));

                        break;
                    }
            }

            WellTestAndUnitsDTO wellTest = ApiUITestBase.WellTestDataService.GetLatestValidWellTestByWellId(well.Id.ToString());
            Assert.IsNotNull(wellTest, "Failed to get well test.");
            var dailyAverage = new WellDailyAverageValueDTO()
            {
                OilRateInferred = (double?)wellTest.Value.Oil,
                WaterRateInferred = (double?)wellTest.Value.Water,
                GasRateInferred = (double?)wellTest.Value.Gas,
                Status = WellDailyAverageDataStatus.Calculated,
                ChokeDiameter = (double?)wellTest.Value.ChokeSize,
                CHP = (double)(wellTest.Value.AverageCasingPressure ?? 0),
                DHPG = (double?)wellTest.Value.GaugePressure,
                Duration = null,
                EndDateTime = wellTest.Value.SampleDate.AddDays(1),
                StartDateTime = wellTest.Value.SampleDate,
                WellId = well.Id,
                WellTestId = wellTest.Value.Id,
                MotorFrequency = (double?)wellTest.Value.Frequency,
                PIP = (double?)wellTest.Value.PumpIntakePressure,
                PDP = (double?)wellTest.Value.PumpDischargePressure,
                THP = (double)wellTest.Value.AverageTubingPressure,
                THT = (double)wellTest.Value.AverageTubingTemperature,
            };
            bool result = ApiUITestBase.SurveillanceService.AddUpdateWellDailyAverageData(dailyAverage);


        }
        public static void RunAnalysisTaskSchedular(string args)
        {
            try
            {
                string strFolder = ConfigurationManager.AppSettings.Get("ForeSiteServerPath");
                string strCommand = Path.Combine(strFolder, "ForeSite.Scheduler.Jobs.exe");

                Process FSschedular = new Process();

                FSschedular.StartInfo.UseShellExecute = true;
                FSschedular.StartInfo.FileName = strCommand;
                FSschedular.StartInfo.Arguments = args;
                //  FSschedular.StartInfo.Arguments = "-runAnalysis";

                Console.WriteLine(strCommand);
                FSschedular.Start();
                //Assert.IsTrue(emulator.Start());

                System.Threading.Thread.Sleep(9000);

                if (FSschedular.Responding)
                {
                    Console.WriteLine("FSschedular " + FSschedular.ProcessName + " is running at process id  " + FSschedular.Id + ". ");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to start FSschedular process: {0}", e.ToString());
            }
        }

        public static void ScanPumpUpCardOnlyForNewDDStranscation()
        {
            ApiUITestBase.Authenticate();
            ApiUITestBase.scanPumpUpDynaCardfromCygNetDDS(TestData.WellConfigData.RRFACName);

        }
        public static void TraceLine(string msg)
        {
            msg = DateTime.Now + " | " + msg;
            Trace.WriteLine(msg);
        }

        public static DataTable BuildDataTableFromXml(string XMLString)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XMLString);
            DataTable Dt = new DataTable();
            Dt.Clear();
            try
            {

                XmlNodeList NodoEstructura = doc.GetElementsByTagName("record");
                int i = 1;
                foreach (XmlNode indnode in NodoEstructura)
                {
                    XmlNodeList subnodes = indnode.ChildNodes;
                    //  Table structure (columns definition) 
                    foreach (XmlNode columna in subnodes)
                    {
                        if (i > 1) break;
                        Dt.Columns.Add(columna.Name, typeof(String));
                    }

                    XmlNode Filas = doc.FirstChild;
                    //  Data Rows 
                    List<string> Valores = new List<string>();
                    foreach (XmlNode Columna in subnodes)
                    {
                        Valores.Add(Columna.InnerText);
                    }
                    Dt.Rows.Add(Valores.ToArray());
                    i++;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return Dt;
        }

        public static DataTable BuildDataTable(string XMLString)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XMLString);
            DataTable Dt = new DataTable();
            Dt.Clear();
            try
            {

                XmlNodeList NodoEstructura = doc.GetElementsByTagName("record");
                int i = 1;
                foreach (XmlNode indnode in NodoEstructura)
                {
                    XmlNodeList subnodes = indnode.ChildNodes;
                    //  Table structure (columns definition) 
                    foreach (XmlNode columna in subnodes)
                    {
                        if (i > 1) break;
                        Dt.Columns.Add(columna.Name, typeof(String));
                    }

                    XmlNode Filas = doc.FirstChild;
                    //  Data Rows 
                    List<string> Valores = new List<string>();
                    foreach (XmlNode Columna in subnodes)
                    {
                        Valores.Add(Columna.InnerText);
                    }
                    Dt.Rows.Add(Valores.ToArray());
                    i++;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return Dt;
        }


        public static string getDatetoformat(string strdtinpu)
        {
            TraceLine("dateTime value is :" + strdtinpu);
            string strdt = strdtinpu.Replace("GMT+0", "");
            strdt = strdt.Replace("GMT-0", "");
            DateTime dt = Convert.ToDateTime(strdt);
            string op = dt.ToString("MM/dd/yyyy");
            return op;
        }


        public static void ChangeUnitSystem(string type)
        {
            ApiUITestBase.Authenticate();
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = ApiUITestBase.SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.UNIT_SYSTEM);
            SystemSettingDTO settingValue = ApiUITestBase.SettingService.GetSystemSettingByName(systemSettings.Name);
            settingValue.StringValue = type;
            ApiUITestBase.SettingService.SaveSystemSetting(settingValue);
            settingValue = ApiUITestBase.SettingService.GetSystemSettingByName(systemSettings.Name);
            Assert.AreEqual(type, settingValue.StringValue, "Unable to Change the Unit System");
            Thread.Sleep(3000);
        }


        public static void ChangeLandingPageToBusniessIntelligence(string settingNameEnum)
        {
            ApiUITestBase.Authenticate();
            string hiddensettingvalue = "BasicLandingPage:Default;ActionTracking:Visible;AllocationGroups:Visible;AuditLogs:Visible;BusinessIntelligence:Visible;CurrentWellbore:Visible;DowntimeHistory:Visible;DowntimeMaintenance:Visible;EnterTourSheet:Visible;FailureAnalysis:Visible;FieldServiceConfiguration:Visible;ForecastResult:Visible;ForecastScenario:Visible;GroupAlarmHistory:Visible;GroupAllocationStatus:Visible;GroupConfiguration:Visible;GroupStatus:Visible;JobManagement:Visible;JobPlanWizard:Visible;JobStatusView:Visible;MapView:Visible;MorningReport:Visible;NetworkConfiguration:Visible;ProductionDashboard:Visible;ProductionOverview:Visible;StickyNotes:Visible;SurfaceNetworkOptimization:Visible;VoidageReplacement:Visible;WellAllowables:Visible;WellAnalysis:Visible;WellConfiguration:Visible;WellDesign:Visible;WellStatus:Visible;WellTest:Visible;WellTrend:Visible;WellboreCorrection:Visible;WellboreHistory:Visible";
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = ApiUITestBase.SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.PAGE_CONFIGURATION);
            SystemSettingDTO settingValue = ApiUITestBase.SettingService.GetSystemSettingByName(systemSettings.Name);
            settingValue.StringValue = hiddensettingvalue;
            ApiUITestBase.SettingService.SaveSystemSetting(settingValue);
            settingValue = ApiUITestBase.SettingService.GetSystemSettingByName(systemSettings.Name);
            string newsettingval = settingValue.StringValue.Replace("BasicLandingPage:Default", "BusinessIntelligence:Default");
            settingValue.StringValue = newsettingval;
            ApiUITestBase.SettingService.SaveSystemSetting(settingValue);
            SetValuesInSystemSettings(SettingServiceStringConstants.EXTERNAL_REPORT_URI, "http://meinwesswks7:82");
            //Add 1  Reoprt in toolbox
            ExternalReportConfigurationDTO externalReportSpotfire = new ExternalReportConfigurationDTO();
            externalReportSpotfire.ReportProvider = ExternalReportProvider.Spotfire;
            externalReportSpotfire.ReportName = "All Well Status";
            externalReportSpotfire.ReportId = "/ForeSiteSotfireReport";
            externalReportSpotfire.WorkspaceId = "Well";
            externalReportSpotfire.FilterColumnName = "welPrimaryKey";
            externalReportSpotfire.FilterTableName = "Well";
            externalReportSpotfire.IsClientReportFilterEnabled = true;
            externalReportSpotfire.IsFilteredByWellSelection = true;
            ApiUITestBase.ReportService.AddExternalReport(externalReportSpotfire);
            reportId = ApiUITestBase.ReportService.GetAllExternalReports().FirstOrDefault(x => x.ReportName == "All Well Status").Id;

        }

        public static void RevertLandingPageToDefault(string settingNameEnum)
        {
            ApiUITestBase.Authenticate();
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = ApiUITestBase.SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.PAGE_CONFIGURATION);
            SystemSettingDTO settingValue = ApiUITestBase.SettingService.GetSystemSettingByName(systemSettings.Name);
            string newsettingval = settingValue.StringValue.Replace("BusinessIntelligence:Default", "BasicLandingPage:Default");
            settingValue.StringValue = newsettingval;
            ApiUITestBase.SettingService.SaveSystemSetting(settingValue);
            SetValuesInSystemSettings(SettingServiceStringConstants.EXTERNAL_REPORT_URI, "");
            ApiUITestBase.ReportService.RemoveExternalReport(reportId.ToString());

        }

        public static void SetValuesInSystemSettings(string settingName, string settingValue)
        {
            ApiUITestBase.Authenticate();
            SystemSettingDTO systemSetting = ApiUITestBase.SettingService.GetSystemSettingByName(settingName);
            SettingValueType settingValueType = systemSetting.Setting.SettingValueType;
            switch (settingValueType)
            {
                case SettingValueType.DecimalNumber:
                    if (!string.IsNullOrEmpty(settingValue))
                    {
                        systemSetting.NumericValue = Convert.ToDouble(decimal.Parse(settingValue));
                        ApiUITestBase.SettingService.SaveSystemSetting(systemSetting);
                    }
                    break;
                case SettingValueType.Number:
                case SettingValueType.TrueOrFalse:
                    systemSetting.NumericValue = int.Parse(settingValue);
                    ApiUITestBase.SettingService.SaveSystemSetting(systemSetting);
                    Assert.AreEqual(settingValue, systemSetting.NumericValue.ToString(), "Unable to Change the System Setting Value for : " + settingName);
                    break;
                default:
                    systemSetting.StringValue = settingValue;
                    ApiUITestBase.SettingService.SaveSystemSetting(systemSetting);
                    Assert.AreEqual(settingValue, systemSetting.StringValue, "Unable to Change the System Setting Value for : " + settingName);
                    break;
            }
        }

        public static void ChangeUnitSystemUserSetting(string type)
        {
            ApiUITestBase.Authenticate();
            SettingType settingType = SettingType.User;
            SettingDTO settings = ApiUITestBase.SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.UNIT_SYSTEM);
            UserSettingDTO[] userSettings = ApiUITestBase.SettingService.GetUserSettingsByUserId(ApiUITestBase.AuthenticatedUser.Id.ToString());

            userSettings[0].StringValue = type;
            ApiUITestBase.SettingService.SaveUserSetting(userSettings[0]);

            UserSettingDTO[] userSettings1 = ApiUITestBase.SettingService.GetUserSettingsByUserId(ApiUITestBase.AuthenticatedUser.Id.ToString());
            Assert.AreEqual(type, userSettings1[0].StringValue, "Unable to change the Unit System");
            Thread.Sleep(3000);
        }

        public static void WriteLogFile(string msg)
        {
            string uprofilepath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            System.IO.File.AppendAllText(Path.Combine(uprofilepath, "ClientAutoamtion.log"), "[" + DateTime.Now.ToString() + "]: " + msg + Environment.NewLine);
        }

        public static void SetLockoutPeriod(string type, double? value)
        {
            ApiUITestBase.Authenticate();
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = null;
            if (type.ToUpper() == "SINGLE")
            {
                systemSettings = ApiUITestBase.SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.LOCKOUT_SINGLE_COMMAND_DELAY);
            }
            else if (type.ToUpper() == "MULTI")
            {
                systemSettings = ApiUITestBase.SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.LOCKOUT_MULTI_COMMAND_DELAY);
            }
            SystemSettingDTO settingValue = ApiUITestBase.SettingService.GetSystemSettingByName(systemSettings.Name);
            settingValue.NumericValue = value;
            ApiUITestBase.SettingService.SaveSystemSetting(settingValue);
            settingValue = ApiUITestBase.SettingService.GetSystemSettingByName(systemSettings.Name);
            Assert.AreEqual(value, settingValue.NumericValue, "Unable to Change the Lockout Setting Value");
        }

        public static void CheckLockoutForCommandName(string CommandName)
        {
            ApiUITestBase.Authenticate();
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = null;
            systemSettings = ApiUITestBase.SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.WELL_COMMAND_MAPPING);
            SystemSettingDTO settingValue = ApiUITestBase.SettingService.GetSystemSettingByName(systemSettings.Name);
            settingValue.ChangeDate = DateTime.UtcNow;
            settingValue.ChangeUser = ApiUITestBase.AuthenticatedUser.Name;
            settingValue.Setting = systemSettings;
            settingValue.StringValue = CommandName;
            SystemSettingDTO[] settingValues = new SystemSettingDTO[] { settingValue };
            ApiUITestBase.SettingService.SaveSystemSettings(settingValues);

        }
        public static void CreateNewRRLWellwithFullData()
        {
            bool dcExisted = false;
            try
            {
                ApiUITestBase.Authenticate();
                ApiUITestBase.Init();
                #region CreateRRLWellFromApiUITestBases
                WellDTO well = new WellDTO() { Name = "RPOC_00001", FacilityId = CygnetFaility, DataConnection = new DataConnectionDTO { ProductionDomain = ApiUITestBase.s_domain, Site = ApiUITestBase.s_site, Service = ApiUITestBase.s_cvsService }, CommissionDate = new DateTime(2016, 4, 18).ToLocalTime(), AssemblyAPI = "API12", SubAssemblyAPI = "API1233", IntervalAPI = "API123456789", WellType = WellTypeId.RRL };
                dcExisted = ApiUITestBase.DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == well.DataConnection.ProductionDomain && dc.Site == well.DataConnection.Site && dc.Service == well.DataConnection.Service) != null;
                WellDTO[] wells = ApiUITestBase.WellService.GetAllWells();
                Assert.IsFalse(wells.Any(w => w.Name.Equals(well.Name)), "Well already exists in database.");
                WellConfigDTO wellConfigDTO = new WellConfigDTO();
                wellConfigDTO.Well = well;
                wellConfigDTO.ModelConfig = ReturnUITestdataDTO(); // test fully configured model
                WellConfigDTO addedWellConfig = ApiUITestBase.WellConfigurationService.AddWellConfig(wellConfigDTO);
                Assert.IsTrue(addedWellConfig.Well.Id > 0, "Well Created does not have ID");
                if (addedWellConfig.Well.Id > 0)
                    TraceLine("Well Created Sucessfully");
                else
                    TraceLine("Well Creation Failed");
                ApiUITestBase._wellsToRemove.Add(addedWellConfig.Well);

                #endregion

            }
            catch (Exception e)
            {
                Assert.Fail("Exception : " + e.ToString() + (e.InnerException == null ? "" : Environment.NewLine + e.InnerException.ToString()));
            }
        }


        public static void CreateGLWellwithFullData(string modelfilename = "GasLift-LFactor1.wflx")
        {
            try
            {
                ApiUITestBase.Authenticate();
                #region CreateRRLWellFromApiUITestBases
                var wells = ApiUITestBase.WellService.GetAllWells().ToList();

                Assert.IsFalse(wells.Any(w => w.Name.Equals(WellName)), "Well already exists in database.");
                WellDTO well = new WellDTO()
                {
                    Name = WellName,
                    FacilityId = CygnetFailityGL,
                    DataConnection = new DataConnectionDTO { ProductionDomain = ApiUITestBase.s_domain, Site = ApiUITestBase.s_site, Service = ApiUITestBase.s_cvsService },
                    FluidType = WellFluidType.BlackOil,
                    CommissionDate = new DateTime(2016, 4, 18).ToLocalTime(),
                    AssemblyAPI = "GasLift_AssemblyAPI" + WellName,
                    SubAssemblyAPI = "GasLift_SubAssemblyAPI" + WellName,
                    IntervalAPI = "GasLift_SubAssemblyAPI" + WellName,
                    WellType = WellTypeId.GLift,
                    GasAllocationGroup = null,
                    OilAllocationGroup = null,
                    WaterAllocationGroup = null
                };
                Assert.IsNotNull(well);
                WellConfigDTO wellConfig = new WellConfigDTO();
                wellConfig.Well = well;
                wellConfig.ModelConfig = null;
                WellConfigDTO addedWellConfig = ApiUITestBase.WellConfigurationService.AddWellConfig(wellConfig);
                ReturnNONRRLModelConfigData(WellTypeId.GLift, addedWellConfig, well, modelfilename);



                #endregion

            }
            catch (Exception e)
            {
                Assert.Fail("Exception : " + e.ToString() + (e.InnerException == null ? "" : Environment.NewLine + e.InnerException.ToString()));
            }
        }

        public static void CreateESPWellwithFullData(string modelfilename = "Esp_ProductionTestData.wflx")
        {
            try
            {
                ApiUITestBase.Authenticate();
                #region CreateESPWellFromApiUITestBases
                var wells = ApiUITestBase.WellService.GetAllWells().ToList();
                Assert.IsFalse(wells.Any(w => w.Name.Equals("ESPWELL_00001")), "Well already exists in database.");
                WellDTO well = new WellDTO()
                {
                    Name = "ESPWELL_00001",
                    FacilityId = CygnetFailityESP,
                    DataConnection = new DataConnectionDTO { ProductionDomain = ApiUITestBase.s_domain, Site = ApiUITestBase.s_site, Service = ApiUITestBase.s_cvsService },
                    CommissionDate = new DateTime(2016, 4, 18).ToLocalTime(),
                    AssemblyAPI = "ESP_AssemblyAPI",
                    SubAssemblyAPI = "ESP_SubAssemblyAPI",
                    IntervalAPI = "ESP_IntervalAPI",
                    WellType = WellTypeId.ESP,
                    GasAllocationGroup = null,
                    OilAllocationGroup = null,
                    WaterAllocationGroup = null
                };
                Assert.IsNotNull(well);
                WellConfigDTO wellConfig = new WellConfigDTO();
                wellConfig.Well = well;
                wellConfig.ModelConfig = null;
                WellConfigDTO addedWellConfig = ApiUITestBase.WellConfigurationService.AddWellConfig(wellConfig);
                ReturnNONRRLModelConfigData(WellTypeId.ESP, addedWellConfig, well, modelfilename);
                wells = ApiUITestBase.WellService.GetAllWells().ToList();
                Assert.IsTrue(wells.Any(w => w.Name.Equals("ESPWELL_00001")), "Well not created from API.");
            }
            catch (Exception e)
            {
                Assert.Fail("Exception : " + e.ToString() + (e.InnerException == null ? "" : Environment.NewLine + e.InnerException.ToString()));
            }
            #endregion
        }

        public static void CreateESPWell()
        {
            try
            {
                ApiUITestBase.Authenticate();
                #region CreateESPWellFromApiUITestBases
                var wells = ApiUITestBase.WellService.GetAllWells().ToList();
                ReferenceTableItemDTO[] wellDepthDatums = ApiUITestBase.WellConfigurationService.GetReferenceTableItems("r_WellDepthDatum", "false");
                ReferenceTableItemDTO wellDepthDatum = wellDepthDatums.FirstOrDefault(t => t.ConstantId == "GROUND_LEVEL") ?? wellDepthDatums.FirstOrDefault();
                ReferenceTableItemDTO[] wellStatus = ApiUITestBase.WellConfigurationService.GetReferenceTableItems("r_WellStatus", "false");
                ReferenceTableItemDTO wellStatusId = wellStatus.FirstOrDefault(t => t.ConstantId == "ACTIVE") ?? wellDepthDatums.FirstOrDefault();
                Assert.IsFalse(wells.Any(w => w.Name.Equals("ESPWELL_00001")), "Well already exists in database.");
                Random random = new Random();
                WellDTO well = new WellDTO()
                {
                    Name = "ESPWELL_00001",
                    FacilityId = CygnetFailityESP,
                    DataConnection = new DataConnectionDTO { ProductionDomain = ApiUITestBase.s_domain, Site = ApiUITestBase.s_site, Service = ApiUITestBase.s_cvsService },
                    CommissionDate = new DateTime(2016, 4, 18).ToLocalTime(),
                    AssemblyAPI = "ESP_AssemblyAPI",
                    SubAssemblyAPI = "ESP_SubAssemblyAPI",
                    IntervalAPI = "ESP_IntervalAPI",
                    WellType = WellTypeId.ESP,
                    DepthCorrectionFactor = 2,
                    WellDepthDatumElevation = 1,
                    WellGroundElevation = 1,
                    WellDepthDatumId = wellDepthDatum?.Id,
                    WellStatusId = wellStatusId?.Id,
                    GasAllocationGroup = null,
                    OilAllocationGroup = null,
                    SurfaceLatitude = (decimal?)random.Next(-90, 90),
                    SurfaceLongitude = (decimal?)random.Next(-180, 180),
                    WaterAllocationGroup = null,
                    AssetId = ApiUITestBase.SurfaceNetworkService.GetAllAssets().FirstOrDefault(x => x.Name.Contains("TestAsset")).Id
                };
                Assert.IsNotNull(well);
                WellConfigDTO wellConfig = new WellConfigDTO();
                wellConfig.Well = well;
                wellConfig.ModelConfig = null;
                WellConfigDTO addedWellConfig = ApiUITestBase.WellConfigurationService.AddWellConfig(wellConfig);

            }
            catch (Exception e)
            {
                Assert.Fail("Exception : " + e.ToString() + (e.InnerException == null ? "" : Environment.NewLine + e.InnerException.ToString()));
            }
            #endregion
        }

        public static void CreateNFWellwithFullData(string modelfilename = "WellfloNFWExample1.wflx")
        {
            try
            {
                ApiUITestBase.Authenticate();
                #region CreateRRLWellFromAPICalls
                var wells = ApiUITestBase.WellService.GetAllWells().ToList();

                Assert.IsFalse(wells.Any(w => w.Name.Equals("NFWWELL_00001")), "Well already exists in database.");
                WellDTO well = new WellDTO()
                {
                    Name = "NFWWELL_00001",
                    FacilityId = CygnetFailityGL,
                    DataConnection = new DataConnectionDTO { ProductionDomain = ApiUITestBase.s_domain, Site = ApiUITestBase.s_site, Service = ApiUITestBase.s_cvsService },
                    CommissionDate = new DateTime(2016, 4, 18).ToLocalTime(),
                    AssemblyAPI = "NFW_AssemblyAPI",
                    SubAssemblyAPI = "NFW_SubAssemblyAPI",
                    IntervalAPI = "NFW_IntervalAPI",
                    WellType = WellTypeId.NF,
                    GasAllocationGroup = null,
                    OilAllocationGroup = null,
                    WaterAllocationGroup = null
                };
                Assert.IsNotNull(well);
                WellConfigDTO wellConfig = new WellConfigDTO();
                wellConfig.Well = well;
                wellConfig.ModelConfig = null;
                WellConfigDTO addedWellConfig = ApiUITestBase.WellConfigurationService.AddWellConfig(wellConfig);
                ReturnNONRRLModelConfigData(WellTypeId.GLift, addedWellConfig, well, modelfilename);
            }
            catch (Exception e)
            {
                Assert.Fail("Exception : " + e.ToString() + (e.InnerException == null ? "" : Environment.NewLine + e.InnerException.ToString()));
            }
            #endregion
        }

        public static bool AreAsssetsPresent()
        {
            bool AreAsssetsPresent = true;
            ApiUITestBase.Authenticate();
            AreAsssetsPresent = ApiUITestBase.SurfaceNetworkService.GetAllAssets().Length == 0 ? false : true;
            return AreAsssetsPresent;
        }


        public static ModelConfigDTO ReturnUITestdataDTO()
        {
            ApiUITestBase.Authenticate();
            SurfaceConfigDTO SampleSurfaceConfig = new SurfaceConfigDTO();
            PumpingUnitManufacturerDTO[] manufacturers = ApiUITestBase.CatalogService.GetAllPumpingUnitManufacturers();
            PumpingUnitManufacturerDTO pumpingUnitManufacturer = manufacturers.FirstOrDefault(pumt => pumt.Name.Equals("Lufkin"));
            Assert.IsNotNull(pumpingUnitManufacturer);
            PumpingUnitTypeDTO[] pumpingUnitTypes = ApiUITestBase.CatalogService.GetPumpingUnitTypesByManufacturer(pumpingUnitManufacturer.Name);
            PumpingUnitTypeDTO pumpingUnitType = pumpingUnitTypes.FirstOrDefault(t => t.AbbreviatedName.Equals("C"));
            Assert.IsNotNull(pumpingUnitType);
            PumpingUnitDTO[] pumpingUnits = ApiUITestBase.CatalogService.GetPumpingUnitsByManufacturerAndType(pumpingUnitManufacturer.Name, pumpingUnitType.AbbreviatedName);
            PumpingUnitDTO pumpingUnitBase = pumpingUnits.FirstOrDefault(pu => pu.Description.Equals("C-57-109-48 L LUFKIN C57-109-48 (4246B)"));
            Assert.IsNotNull(pumpingUnitBase);
            PumpingUnitDTO pumpingUnit = ApiUITestBase.CatalogService.GetPumpingUnitByManufacturerTypeAndDescription(pumpingUnitManufacturer.Name, pumpingUnitType.AbbreviatedName, pumpingUnitBase.Description);
            SampleSurfaceConfig.PumpingUnit = pumpingUnit;
            SampleSurfaceConfig.PumpingUnitType = pumpingUnitType;
            SampleSurfaceConfig.ClockwiseRotation = PumpingUnitRotationDirection.Clockwise;
            SampleSurfaceConfig.MotorAmpsDown = 120;
            SampleSurfaceConfig.MotorAmpsUp = 144;
            SampleSurfaceConfig.WristPinPosition = 2;
            SampleSurfaceConfig.ActualStrokeLength = 25.00;
            SampleSurfaceConfig.MotorType = new RRLMotorTypeDTO() { Name = "Nema B Electric" };
            SampleSurfaceConfig.MotorSize = new RRLMotorSizeDTO(50);
            SampleSurfaceConfig.SlipTorque = new RRLMotorSlipDTO() { Rating = 2 };

            //Weights
            //Weights
            CrankWeightsConfigDTO SampleWeightsConfig = new CrankWeightsConfigDTO();
            POPRRLCranksDTO[] crankId = ApiUITestBase.CatalogService.GetCranksByPumpingUnitPK(pumpingUnitManufacturer.Name, pumpingUnitType.AbbreviatedName, pumpingUnit.Description);
            SampleWeightsConfig.CrankId = crankId[1].CrankId;
            if (SampleWeightsConfig.CrankId != "N/A")
            {
                POPRRLCranksWeightsDTO crankCBT = ApiUITestBase.CatalogService.GetCrankWeightsByCrankId("9411OC");
                SampleWeightsConfig.CBT = crankCBT.CrankCBT;
                SampleWeightsConfig.CrankId = crankCBT.CrankId;
                SampleWeightsConfig.TorqCalcMethod = TorqueCalculationMethod.Mills;
                SampleWeightsConfig.Crank_1_Auxiliary = new RRLAuxiliaryWeightDTO { LagId = crankCBT.AuxiliaryIdentifier[0], LeadId = crankCBT.AuxiliaryIdentifier[0] };
                SampleWeightsConfig.Crank_1_Primary = new RRLPrimaryWeightDTO { LagId = "OORO", LeadId = "6RO" };
                SampleWeightsConfig.Crank_2_Auxiliary = new RRLAuxiliaryWeightDTO { LagId = crankCBT.AuxiliaryIdentifier[0], LeadId = crankCBT.AuxiliaryIdentifier[0] };
                SampleWeightsConfig.Crank_2_Primary = new RRLPrimaryWeightDTO { LagId = "OORO", LeadId = "6RO" };


                //SampleWeightsConfig.CrankId = "9411OC";
                //SampleWeightsConfig.Crank_1_Primary = new RRLPrimaryWeightDTO { LagId = "OORO", LeadId = "6RO" };
                //SampleWeightsConfig.Crank_2_Primary = new RRLPrimaryWeightDTO { LagId = "OORO", LeadId = "6RO" };
                //SampleWeightsConfig.TorqCalcMethod = TorqueCalculationMethod.Mills;
                //SampleWeightsConfig.PumpingUnitCrankCBT = crankCBT.PumpingUnitCrankCBT;
            }


            //DownHole
            DownholeConfigDTO SampleDownholeConfig = new DownholeConfigDTO();
            SampleDownholeConfig.PumpDiameter = 3.25;
            SampleDownholeConfig.PumpDepth = 5070;
            SampleDownholeConfig.TubingID = 2.14;
            SampleDownholeConfig.TubingOD = 2.35;
            SampleDownholeConfig.TubingAnchorDepth = 3000;
            SampleDownholeConfig.CasingOD = 5.5;
            SampleDownholeConfig.CasingWeight = 15.5;
            SampleDownholeConfig.TopPerforation = 4000.0;
            SampleDownholeConfig.BottomPerforation = 4500;


            //Rods
            RodStringConfigDTO SampleRodsConfig = new RodStringConfigDTO();
            SampleRodsConfig.TotalRodLength = 5100;
            RodTaperConfigDTO[] RodTaperArray = new RodTaperConfigDTO[3];
            SampleRodsConfig.RodTapers = RodTaperArray;
            RodTaperConfigDTO Taper1 = new RodTaperConfigDTO();
            Taper1.Grade = "D";
            Taper1.Manufacturer = "_Generic Manufacturer";
            Taper1.NumberOfRods = 56;
            Taper1.RodGuid = "0";
            Taper1.RodLength = 30.0;
            Taper1.ServiceFactor = 0.9;
            Taper1.Size = 1.0;  //Taper1.TaperLength = 1710;
            RodTaperArray[0] = Taper1;
            RodTaperConfigDTO Taper2 = new RodTaperConfigDTO();
            Taper2.Grade = "N-78";
            Taper2.Manufacturer = "Alberta Oil Tools";
            Taper2.NumberOfRods = 57;
            Taper2.RodGuid = "0";
            Taper2.RodLength = 30.0;
            Taper2.ServiceFactor = 0.9;
            Taper2.Size = 0.875; // Taper2.TaperLength = 1710;
            RodTaperArray[1] = Taper2;
            RodTaperConfigDTO Taper3 = new RodTaperConfigDTO();
            Taper3.Grade = "EL";
            Taper3.Manufacturer = "Weatherford, Inc.";
            Taper3.NumberOfRods = 56;
            Taper3.RodGuid = "0";
            Taper3.RodLength = 30.0;
            Taper3.ServiceFactor = 0.9;
            Taper3.Size = 0.75;  //Taper3.TaperLength = 1680;
            RodTaperArray[2] = Taper3;
            //Model File
            ModelConfigDTO SampleModel = new ModelConfigDTO();
            //SampleModel.WellId = well.Id.ToString();
            SampleModel.Weights = SampleWeightsConfig;
            SampleModel.Rods = SampleRodsConfig;
            SampleModel.Downhole = SampleDownholeConfig;
            SampleModel.Surface = SampleSurfaceConfig;

            return SampleModel;
        }

        public static void ReturnNONRRLModelConfigData(WellTypeId wType, WellConfigDTO AddedWellConfig, WellDTO well, string modelfileName)
        {
            //   string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string Path = FilesLocation + "\\UIAutomation\\TestData";
            ApiUITestBase.Authenticate();
            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };
            switch (wType)
            {
                case WellTypeId.GLift:
                    {
                        ModelFileOptionDTO options = new ModelFileOptionDTO()
                        {
                            CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                            OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.UpdateWCT_WGR) },

                        };
                        options.Comment = "Gas Lift model Name  Uploaded " + modelfileName;
                        modelFile.Options = options;
                        modelFile.ApplicableDate = new DateTime(2018, 7, 01).ToUniversalTime();
                        modelFile.WellId = AddedWellConfig.Well.Id;
                        break;
                    }
                case WellTypeId.ESP:
                    {
                        ModelFileOptionDTO options = new ModelFileOptionDTO()
                        {
                            CalibrationMethod = CalibrationMethodId.LFactor,
                            OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.UpdateWCT_WGR) },

                        };
                        options.Comment = "ESP model Name  Uploaded " + modelfileName;
                        modelFile.Options = options;
                        modelFile.ApplicableDate = new DateTime(2018, 7, 01).ToUniversalTime();
                        modelFile.WellId = AddedWellConfig.Well.Id;
                        break;

                    }
                case WellTypeId.NF:
                    {
                        ModelFileOptionDTO options = new ModelFileOptionDTO()
                        {
                            CalibrationMethod = CalibrationMethodId.LFactor,
                            OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.UpdateWCT_WGR) },
                        };
                        options.Comment = "Natrually lfowing Well model Name  Uploaded " + modelfileName;
                        modelFile.Options = options;
                        modelFile.ApplicableDate = new DateTime(2018, 7, 01).ToUniversalTime();
                        modelFile.WellId = AddedWellConfig.Well.Id;
                        break;
                    }
            }
            var di = new DirectoryInfo(Path);
            foreach (var file in di.GetFiles("*", SearchOption.AllDirectories))
                file.Attributes &= ~FileAttributes.ReadOnly;
            TraceLine(string.Format("Path for Model File is {0} , and Model Name is {1}", Path, modelfileName));
            if (System.IO.File.Exists(System.IO.Path.Combine(Path, modelfileName)))
            {
                TraceLine("File Exists , now trying to read the data from " + System.IO.Path.Combine(Path, modelfileName));
            }
            else
            {
                TraceLine("Aborting Test as File was not found");
                return;
            }
            byte[] fileAsByteArray = APIUITestBase.GetByteArray(Path, modelfileName);
            Assert.IsNotNull(fileAsByteArray);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wType, ApiUITestBase.ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            ApiUITestBase.ModelFileService.AddWellModelFile(modelFile);

            WellConfigDTO openWell = ApiUITestBase.WellConfigurationService.GetWellConfig(well.Id.ToString());

        }
        public static void AddWellSettings(String wellname, string settingName, double value = 0)
        {
            var allWells = ApiUITestBase.WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Contains(wellname));
            ApiUITestBase.AddWellSettingWithDoubleValues(well.Id, settingName, value);
        }
        public static void createasset(string name= "TestAsset")
        {
            UserDTO user = ApiUITestBase.AdministrationService.GetUser(ApiUITestBase.AuthenticatedUser.Id.ToString());
            AssetDTO[] assets = ApiUITestBase.SurfaceNetworkService.GetAllAssets();
            if (assets.Count() == 0)
            {
                AssetDTO assetToAdd = new AssetDTO() { Name = name, Description = "TestAsset" };
                AssetDTO asset = ApiUITestBase.AddAndGetAsset(assetToAdd);
                user.Assets.Add(asset);
                ApiUITestBase._assetsToRemove.Add(asset);
                ApiUITestBase.AdministrationService.UpdateUser(user);
            }
            //Assume that Previous test did not clear Test assets
            else if (assets.FirstOrDefault( x => x.Name == "TestAsset") != null)
            {
                if ( name != "TestAsset")
                {
                    AssetDTO assetToAdd = new AssetDTO() { Name = name, Description = "TestAsset" };
                    AssetDTO asset = ApiUITestBase.AddAndGetAsset(assetToAdd);
                    user.Assets.Add(asset);
                    ApiUITestBase._assetsToRemove.Add(asset);
                    ApiUITestBase.AdministrationService.UpdateUser(user);
                }
            }
            else
            {
                foreach (AssetDTO a in assets)
                {
                    if (a.Name == "TestAsset")
                    {
                        //  AssetDTO asset = ApiUITestBase.AddAndGetAsset(a);
                        user.Assets.Add(a);
                        ApiUITestBase.AdministrationService.UpdateUser(user);
                        break;
                    }
                    else
                    {
                        AssetDTO assetToAdd = new AssetDTO() { Name = name, Description = "TestAsset" };
                        AssetDTO asset = ApiUITestBase.AddAndGetAsset(assetToAdd);
                        user.Assets.Add(asset);
                        ApiUITestBase._assetsToRemove.Add(asset);
                        ApiUITestBase.AdministrationService.UpdateUser(user);
                    }
                }
            }



        }
        public static void Deleteasset()
        {
            UserDTO userDetails = ApiUITestBase.AdministrationService.GetUser(ApiUITestBase.AuthenticatedUser.Id.ToString());
            userDetails.Assets.Clear();
            ApiUITestBase.AdministrationService.UpdateUser(userDetails);
            AssetDTO[] assetDetails = ApiUITestBase.SurfaceNetworkService.GetAllAssets();
            foreach (AssetDTO assetDTO in assetDetails)
            {
                if (assetDTO.Name == "TestAsset" || assetDTO.Name == AssetName)
                {
                    ApiUITestBase.SurfaceNetworkService.RemoveAsset(assetDTO.Id.ToString());

                }

            }


        }

        public static void DeleteWellsByAPI()
        {
            try
            {
                ApiUITestBase.Authenticate();
                WellDTO[] wells = ApiUITestBase.WellService.GetAllWells();
                if (wells.Length != 0)
                {
                    foreach (WellDTO well in wells)
                    {
                        ApiUITestBase.WellConfigurationService.RemoveWellConfig(well.Id.ToString());
                        TraceLine(well.Name + " deleted successfully");
                    }
                }
                else
                    TraceLine("There is no well present in ForeSite");
            }
            catch (Exception e)
            {
                Assert.Fail("Exception : " + e.ToString() + (e.InnerException == null ? "" : Environment.NewLine + e.InnerException.ToString()));
            }
        }


      
        public static void CreateRole(string roleName)
        {

            ApiUITestBase.Authenticate();
            IList<PermissionDTO> permissions = ApiUITestBase.AdministrationService.GetPermissions();
            RoleDTO role = new RoleDTO();
            role.Name = roleName;
            role.Permissions = permissions.ToList();
            ApiUITestBase.AdministrationService.AddRole(role);
            RoleDTO addedRole = ApiUITestBase.AdministrationService.GetRoles().FirstOrDefault(x => x.Name == roleName);
            ApiUITestBase._rolesToRemove.Add(addedRole);
        }
        public static void RemovePermissionFromRole(string roleName, PermissionId permissionId, string permissionName, ExtentTest test)
        {
            ApiUITestBase.Authenticate();
            RoleDTO role = ApiUITestBase.AdministrationService.GetRoles().FirstOrDefault(x => x.Name == roleName);
            IList<PermissionDTO> permissions = ApiUITestBase.AdministrationService.GetPermissions();
            role.Permissions = permissions.Where(p => p.Id != permissionId).ToList();
            ApiUITestBase.AdministrationService.UpdateRole(role);
            TraceLine(permissionName + " permission is removed from " + roleName + " role");
            test.Pass(permissionName + " permission is removed from " + roleName + " role");
        }

        public static void AddPermissioninRole(string roleName, PermissionId permissionId, string permissionName, ExtentTest test)
        {
            ApiUITestBase.Authenticate();
            RoleDTO toBeUpdateRole = ApiUITestBase.AdministrationService.GetRoles().FirstOrDefault(x => x.Name == roleName);
            PermissionDTO toBeAddedPermission = ApiUITestBase.AdministrationService.GetPermissions().FirstOrDefault(x => x.Id == permissionId);
            toBeUpdateRole.Permissions.Add(toBeAddedPermission);
            ApiUITestBase.AdministrationService.UpdateRole(toBeUpdateRole);
            TraceLine(permissionName + " permission is added in " + roleName + " role");
            test.Pass(permissionName + " permission is added in " + roleName + " role");
        }

        public static void RemoveRole(string roleName)
        {
            ApiUITestBase.Authenticate();
            RoleDTO ToBeRemovedRole = ApiUITestBase.AdministrationService.GetRoles().FirstOrDefault(x => x.Name == roleName);
            ApiUITestBase.AdministrationService.RemoveRole(ToBeRemovedRole);
            TraceLine("Role Removed");
        }

        public static void updateUserWithGivenRole(string roleName)
        {
            ApiUITestBase.Authenticate();
            RoleDTO RoleToBeAssignedToUser = ApiUITestBase.AdministrationService.GetRoles().FirstOrDefault(x => x.Name == roleName);
            UserDTO user = ApiUITestBase.AdministrationService.GetUser(ApiUITestBase.AuthenticatedUser.Id.ToString());
            user.Roles.Clear();
            user.Roles.Add(RoleToBeAssignedToUser);
            ApiUITestBase.AdministrationService.UpdateUser(user);
            TraceLine("Role" + roleName + "Assigned to " + ApiUITestBase.AuthenticatedUser.Name);
        }

        public static int PermiId(string perm)
        {
            var number = (int)((PermissionId)Enum.Parse(typeof(PermissionId), perm));
            return number;
        }
        public static void RemovePermission(string perm)
        {

        }
        #region GetMetrics
        public static string repeat = string.Empty;
        enum ProcessorInfo
        {
            AddressWidth,
            Caption,
            MaxClockSpeed,
            NumberOfCores,
            Name
        }
        enum SystemDetail
        {
            SystemType,
            TotalPhysicalMemory,
            UserName,
            Manufacturer,
            Model
        }
        enum OSDetials
        {
            OSArchitecture,
            Caption,
            Manufacturer,
            BuildNumber
        }

        public static void GetMachineDetails()
        {

            GetCPUDetails();
            GetRAMDetails();
            GetDiskSpaceDetails();
            GetOSDetails();
            //AddressWidth Caption MaxClockSpeed NumberOfCores SystemName
        }

        static void GetCPUDetails()
        {

            repeat = new String('*', 25);
            int i = 5;
            string D4 = i.ToString("D4");
            TraceLine(D4);
            TraceLine(repeat + "System Processor Details" + repeat);
            string Key = "Win32_Processor";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + Key);
            foreach (ManagementObject share in searcher.Get())
            {
                // Some Codes ...
                PropertyDataCollection propdatacollecion = share.Properties;
                foreach (PropertyData pdata in propdatacollecion)
                {

                    switch (pdata.Name)
                    {
                        //  case "AddressWidth": { TraceLine(string.Format("******* Processor Acrchitecture 64 bit / 32 Bit --     {1}: bit *******", pdata.Name, pdata.Value));  break; }
                        //   case "Caption": { TraceLine(string.Format("******* {0}:     {1}:  *******", pdata.Name, pdata.Value)); break; }
                        //   case "MaxClockSpeed": { TraceLine(string.Format("******* {0}:     {1}:  *******", pdata.Name, pdata.Value)); break; }
                        case "NumberOfCores": { TraceLine(string.Format("******* {0}:     {1}:  *******", pdata.Name, pdata.Value)); break; }
                        case "Name": { TraceLine(string.Format("******* Processor {0}:     {1}:  *******", pdata.Name, pdata.Value)); break; }

                    }
                }


            }
        }

        static void GetRAMDetails()
        {
            TraceLine(" ****************** System Memory and Other Details  ********************************");
            string Key = "Win32_ComputerSystem";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + Key);
            foreach (ManagementObject share in searcher.Get())
            {
                // Some Codes ...
                PropertyDataCollection propdatacollecion = share.Properties;
                foreach (PropertyData pdata in propdatacollecion)
                {
                    switch (pdata.Name)
                    {
                        case "SystemType": { TraceLine(string.Format("******* Processor Acrchitecture 64 bit / 32 Bit --     {1}: bit *******", pdata.Name, pdata.Value)); break; }
                        case "TotalPhysicalMemory": { TraceLine(string.Format("******* {0}:     {1} GB  *******", pdata.Name, getSizeinGB(pdata.Value.ToString()))); break; }
                        case "UserName": { TraceLine(string.Format("******* {0}:     {1}:  *******", pdata.Name, pdata.Value)); break; }
                        case "Manufacturer": { TraceLine(string.Format("******* {0}:     {1}:  *******", pdata.Name, pdata.Value)); break; }
                        case "Model": { TraceLine(string.Format("******* {0}:     {1}:  *******", pdata.Name, pdata.Value)); break; }
                    }
                    //TraceLine(string.Format("******* {0}:     {1}:  *******", pdata.Name, pdata.Value));
                }
            }
        }
        static void GetDiskSpaceDetails()


        {
            TraceLine(" ****************** System Hard Disk Size Details:  ********************************");
            foreach (System.IO.DriveInfo label in System.IO.DriveInfo.GetDrives())
            {

                if (label.IsReady)
                {
                    TraceLine(string.Format("*** Label Name : {0} TotalSize: {1} GB   FreeSpace: {2} GB ", label.Name, getSizeinGB(label.TotalSize.ToString()), getSizeinGB(label.TotalFreeSpace.ToString())));
                }
            }
        }


        static void GetOSDetails()
        {

            TraceLine(" ****************** System Operating System Details  ********************************");
            string Key = "Win32_OperatingSystem";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + Key);
            foreach (ManagementObject share in searcher.Get())
            {
                // Some Codes ...
                PropertyDataCollection propdatacollecion = share.Properties;
                foreach (PropertyData pdata in propdatacollecion)
                {
                    switch (pdata.Name)
                    {
                        case "BuildNumber": { TraceLine(string.Format("******* {0}:     {1}:  *******", pdata.Name, pdata.Value)); break; }
                        case "Caption": { TraceLine(string.Format("******* {0}:     {1}:  *******", pdata.Name, pdata.Value)); break; }
                        case "Manufacturer": { TraceLine(string.Format("******* {0}:     {1}:  *******", pdata.Name, pdata.Value)); break; }
                        case "OSArchitecture": { TraceLine(string.Format("******* {0}:     {1}:  *******", pdata.Name, pdata.Value)); break; }
                    }
                    // TraceLine(string.Format("******* {0}:     {1}:  *******", pdata.Name, pdata.Value));
                }
            }
        }

        private static string getSizeinGB(string size)

        {
            string getSizeinGB = string.Empty;
            double sz;
            double.TryParse(size, out sz);
            sz = sz / Math.Pow(10, 9);
            sz = sz / 1.074;
            getSizeinGB = sz.ToString();
            return getSizeinGB;
        }

        public static void AddActionTrackingTypeSubType()
        {
            ApiUITestBase.Authenticate();
            ReferenceDataMaintenanceEntityDTO[] dataEntities = ApiUITestBase.DBEntityService.GetReferenceDataMaintenanceEntities();
            ReferenceDataMaintenanceEntityDTO refTrackingItemDTO = dataEntities.FirstOrDefault(x => x.EntityTitle == "Tracking Item Type");
            MetaDataDTO[] addMetaDatas = ApiUITestBase.DBEntityService.GetRefereneceMetaDataEntityForAdd("r_TrackingItemType");
            MetaDataDTO trackitem1 = addMetaDatas.FirstOrDefault(x => x.ColumnName == "tiyName");
            string itemtype = "UIAType";
            trackitem1.DataValue = itemtype;
            EntityGridSettingDTO enty = new EntityGridSettingDTO();
            enty.EntityName = "r_TrackingItemType";
            enty.GridSetting = new GridSettingDTO { NumberOfPages = 100, PageSize =10000};
            DBEntityDTO datas1 = ApiUITestBase.DBEntityService.GetTableData(enty);
            bool valuepresnt = false;
            string addeitendata = "";
            foreach ( var objt in datas1.DataValues)
            {
               object objfound = objt.FirstOrDefault(x => x.ToString() == "UIAType");
                if (objfound != null)
                {
                    valuepresnt = true;
                    break;
                }
            }
            if (valuepresnt == false) //Process for UIAType & subType
            {
                addeitendata = ApiUITestBase.DBEntityService.AddReferenceData(addMetaDatas);
           
            #endregion

            #region  1.2 GetTrackingItemSubType from Reference Table 
            ReferenceDataMaintenanceEntityDTO refTrackingItemsubtypeDTO = dataEntities.FirstOrDefault(x => x.EntityTitle == "Tracking Item Suntype");
            MetaDataDTO[] addMetaDatas2 = ApiUITestBase.DBEntityService.GetRefereneceMetaDataEntityForAdd("r_TrackingItemSubtype");
            MetaDataDTO trackitemsubype = addMetaDatas2.FirstOrDefault(x => x.ColumnName == "tisName");
            string itemsubtype = "UIASubType";
            trackitemsubype.DataValue = itemsubtype;
            MetaDataDTO trackitemid = addMetaDatas2.FirstOrDefault(x => x.ColumnName == "tisFK_r_TrackingItemType");
            trackitemid.DataValue = Int32.Parse(addeitendata);
            addeitendata = ApiUITestBase.DBEntityService.AddReferenceData(addMetaDatas2);
            }

        }

        public static string GetAuthuser()
        {
            ApiUITestBase.Authenticate();
            return ApiUITestBase.TrackingItemService.GetTrackingItemUser().FirstOrDefault(x => x.ControlId == ApiUITestBase.AuthenticatedUser.Id).ControlText;
        }

        public static string GetAuthuserID()
        {
            ApiUITestBase.Authenticate();
            return ApiUITestBase.AuthenticatedUser.Name;
        }
    }
    #endregion

}

