using ArtOfTest.WebAii.Controls.HtmlControls;
using ClientAutomation.PageObjects;
using ClientAutomation.TelerikCoreUtils;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;

namespace ClientAutomation.Helper
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


        public static void PrintScreen(string filename, int screenNum = 0)
        {
            int i = 0;
            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                Bitmap screenshot = new Bitmap(screen.Bounds.Width, screen.Bounds.Height, PixelFormat.Format32bppArgb);
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
            APIUITestBase obj = new APIUITestBase();
            obj.Authenticate();
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = obj.SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.UNIT_SYSTEM);
            SystemSettingDTO settingValue = obj.SettingService.GetSystemSettingByName(systemSettings.Name);
            settingValue.StringValue = type;
            obj.SettingService.SaveSystemSetting(settingValue);
            settingValue = obj.SettingService.GetSystemSettingByName(systemSettings.Name);
            Assert.AreEqual(type, settingValue.StringValue, "Unable to Change the Unit System");
            Thread.Sleep(3000);
        }

        public static void ChangeUnitSystemUserSetting(string type)
        {
            APIUITestBase obj = new APIUITestBase();
            obj.Authenticate();
            SettingType settingType = SettingType.User;
            SettingDTO settings = obj.SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.UNIT_SYSTEM);
            UserSettingDTO[] userSettings = obj.SettingService.GetUserSettingsByUserId(obj.AuthenticatedUser.Id.ToString());

            userSettings[0].StringValue = type;
            obj.SettingService.SaveUserSetting(userSettings[0]);

            UserSettingDTO[] userSettings1 = obj.SettingService.GetUserSettingsByUserId(obj.AuthenticatedUser.Id.ToString());
            Assert.AreEqual(type, userSettings1[0].StringValue, "Unable to change the Unit System");
            Thread.Sleep(3000);
        }

        public static void WriteLogFile(string msg)
        {
            string uprofilepath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            File.AppendAllText(Path.Combine(uprofilepath, "ClientAutoamtion.log"), "[" + DateTime.Now.ToString() + "]: " + msg + Environment.NewLine);
        }


        public static void CreateNewRRLWellwithFullData()
        {
            APIUITestBase apicall = new APIUITestBase();
            bool dcExisted = false;
            try
            {
                apicall.Authenticate();
                #region CreateRRLWellFromAPICalls
                WellDTO well = new WellDTO() { Name = "RPOC_00001", FacilityId = CygnetFaility, DataConnection = new DataConnectionDTO { ProductionDomain = apicall.s_domain, Site = apicall.s_site, Service = apicall.s_cvsService }, CommissionDate = new DateTime(2016, 4, 18).ToLocalTime(), AssemblyAPI = "API12", SubAssemblyAPI = "API1233", IntervalAPI = "API123456789", WellType = WellTypeId.RRL };
                dcExisted = apicall.DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == well.DataConnection.ProductionDomain && dc.Site == well.DataConnection.Site && dc.Service == well.DataConnection.Service) != null;
                WellDTO[] wells = apicall.WellService.GetAllWells();
                Assert.IsFalse(wells.Any(w => w.Name.Equals(well.Name)), "Well already exists in database.");
                WellConfigDTO wellConfigDTO = new WellConfigDTO();
                wellConfigDTO.Well = well;
                wellConfigDTO.ModelConfig = ReturnUITestdataDTO(); // test fully configured model
                WellConfigDTO addedWellConfig = apicall.WellConfigurationService.AddWellConfig(wellConfigDTO);
                #endregion

            }
            catch (Exception e)
            {
                Assert.Fail("Exception : " + e.ToString() + (e.InnerException == null ? "" : Environment.NewLine + e.InnerException.ToString()));
            }
        }


        public static void CreateGLWellwithFullData(string modelfilename = "GasLift-LFactor1.wflx")
        {
            APIUITestBase apicall = new APIUITestBase();
            try
            {
                apicall.Authenticate();
                #region CreateRRLWellFromAPICalls
                var wells = apicall.WellService.GetAllWells().ToList();

                Assert.IsFalse(wells.Any(w => w.Name.Equals(WellName)), "Well already exists in database.");
                WellDTO well = new WellDTO()
                {
                    Name = WellName,
                    FacilityId = CygnetFailityGL,
                    DataConnection = new DataConnectionDTO { ProductionDomain = apicall.s_domain, Site = apicall.s_site, Service = apicall.s_cvsService },
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
                WellConfigDTO addedWellConfig = apicall.WellConfigurationService.AddWellConfig(wellConfig);
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
            APIUITestBase apicall = new APIUITestBase();
            try
            {
                apicall.Authenticate();
                #region CreateESPWellFromAPICalls
                var wells = apicall.WellService.GetAllWells().ToList();

                Assert.IsFalse(wells.Any(w => w.Name.Equals("ESPWELL_00001")), "Well already exists in database.");
                WellDTO well = new WellDTO()
                {
                    Name = "ESPWELL_00001",
                    FacilityId = CygnetFailityESP,
                    DataConnection = new DataConnectionDTO { ProductionDomain = apicall.s_domain, Site = apicall.s_site, Service = apicall.s_cvsService },
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
                WellConfigDTO addedWellConfig = apicall.WellConfigurationService.AddWellConfig(wellConfig);
                ReturnNONRRLModelConfigData(WellTypeId.ESP, addedWellConfig, well, modelfilename);
            }
            catch (Exception e)
            {
                Assert.Fail("Exception : " + e.ToString() + (e.InnerException == null ? "" : Environment.NewLine + e.InnerException.ToString()));
            }
            #endregion
        }


        public static void CreateNFWellwithFullData(string modelfilename = "WellfloNFWExample1.wflx")
        {
            APIUITestBase apicall = new APIUITestBase();
            try
            {
                apicall.Authenticate();
                #region CreateRRLWellFromAPICalls
                var wells = apicall.WellService.GetAllWells().ToList();

                Assert.IsFalse(wells.Any(w => w.Name.Equals("NFWWELL_00001")), "Well already exists in database.");
                WellDTO well = new WellDTO()
                {
                    Name = "NFWWELL_00001",
                    FacilityId = CygnetFailityGL,
                    DataConnection = new DataConnectionDTO { ProductionDomain = apicall.s_domain, Site = apicall.s_site, Service = apicall.s_cvsService },
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
                WellConfigDTO addedWellConfig = apicall.WellConfigurationService.AddWellConfig(wellConfig);
                ReturnNONRRLModelConfigData(WellTypeId.GLift, addedWellConfig, well, modelfilename);
            }
            catch (Exception e)
            {
                Assert.Fail("Exception : " + e.ToString() + (e.InnerException == null ? "" : Environment.NewLine + e.InnerException.ToString()));
            }
            #endregion
        }




        public static ModelConfigDTO ReturnUITestdataDTO()
        {
            APIUITestBase apicall3 = new APIUITestBase();
            apicall3.Authenticate();
            SurfaceConfigDTO SampleSurfaceConfig = new SurfaceConfigDTO();
            PumpingUnitManufacturerDTO[] manufacturers = apicall3.CatalogService.GetAllPumpingUnitManufacturers();
            PumpingUnitManufacturerDTO pumpingUnitManufacturer = manufacturers.FirstOrDefault(pumt => pumt.Name.Equals("Lufkin"));
            Assert.IsNotNull(pumpingUnitManufacturer);
            PumpingUnitTypeDTO[] pumpingUnitTypes = apicall3.CatalogService.GetPumpingUnitTypesByManufacturer(pumpingUnitManufacturer.Name);
            PumpingUnitTypeDTO pumpingUnitType = pumpingUnitTypes.FirstOrDefault(t => t.AbbreviatedName.Equals("C"));
            Assert.IsNotNull(pumpingUnitType);
            PumpingUnitDTO[] pumpingUnits = apicall3.CatalogService.GetPumpingUnitsByManufacturerAndType(pumpingUnitManufacturer.Name, pumpingUnitType.AbbreviatedName);
            PumpingUnitDTO pumpingUnitBase = pumpingUnits.FirstOrDefault(pu => pu.Description.Equals("C-57-109-48 L LUFKIN C57-109-48 (4246B)"));
            Assert.IsNotNull(pumpingUnitBase);
            PumpingUnitDTO pumpingUnit = apicall3.CatalogService.GetPumpingUnitByManufacturerTypeAndDescription(pumpingUnitManufacturer.Name, pumpingUnitType.AbbreviatedName, pumpingUnitBase.Description);
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
            POPRRLCranksDTO[] crankId = apicall3.CatalogService.GetCranksByPumpingUnitPK(pumpingUnitManufacturer.Name, pumpingUnitType.AbbreviatedName, pumpingUnit.Description);
            SampleWeightsConfig.CrankId = crankId[1].CrankId;
            if (SampleWeightsConfig.CrankId != "N/A")
            {
                POPRRLCranksWeightsDTO crankCBT = apicall3.CatalogService.GetCrankWeightsByCrankId("9411OC");
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
            APIUITestBase apicall4 = new APIUITestBase();
            //   string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string Path = FilesLocation + "\\UIAutomation\\TestData";
            apicall4.Authenticate();
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
            if (File.Exists(System.IO.Path.Combine(Path, modelfileName)))
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
            Assert.AreEqual(wType, apicall4.ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            apicall4.ModelFileService.AddWellModelFile(modelFile);

            WellConfigDTO openWell = apicall4.WellConfigurationService.GetWellConfig(well.Id.ToString());

        }

        public static void DeleteWell()
        {
            try
            {
                TelerikObject.Click(PageDashboard.configurationtab);
                Thread.Sleep(1000);
                TelerikObject.Click(PageDashboard.wellConfigurationtab);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                TelerikObject.Click(PageWellConfig.btnDeleteWell);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                TelerikObject.Click(PageWellConfig.btnConfirmDeleteWell);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                TelerikObject.Click(PageWellConfig.btnConfirmDeleteWell);
                TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));
                TelerikObject.DoStaticWait();
                string wellcount = PageDashboard.WellCounter.BaseElement.InnerText;
                if (wellcount == "0")
                {
                    TraceLine("Well was deleted and Wel counter is " + wellcount);
                }
                else
                {
                    TraceLine("Well was NOT  deleted and Wel counter is " + wellcount);
                }
            }
            catch (Exception e)
            {
                TraceLine("Error Deleting Well from UI " + e.ToString());
                PrintScreen("Err_WellDeletion");
            }
        }

        public static void DeleteWellsByAPI()
        {
            APIUITestBase apicall = new APIUITestBase();
            try
            {
                apicall.Authenticate();
                WellDTO[] wells = apicall.WellService.GetAllWells();
                if (wells.Length != 0)
                {
                    foreach (WellDTO well in wells)
                    {
                        apicall.WellConfigurationService.RemoveWellConfig(well.Id.ToString());
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
                        //  case "AddressWidth": { ClientAutomation.Helper.CommonHelper.TraceLine(string.Format("******* Processor Acrchitecture 64 bit / 32 Bit --     {1}: bit *******", pdata.Name, pdata.Value));  break; }
                        //   case "Caption": { ClientAutomation.Helper.CommonHelper.TraceLine(string.Format("******* {0}:     {1}:  *******", pdata.Name, pdata.Value)); break; }
                        //   case "MaxClockSpeed": { ClientAutomation.Helper.CommonHelper.TraceLine(string.Format("******* {0}:     {1}:  *******", pdata.Name, pdata.Value)); break; }
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
                    //ClientAutomation.Helper.CommonHelper.TraceLine(string.Format("******* {0}:     {1}:  *******", pdata.Name, pdata.Value));
                }
            }
        }
        static void GetDiskSpaceDetails()


        {
            TraceLine(" ****************** System Hard Disk Size Details:  ********************************");
            foreach (System.IO.DriveInfo label in DriveInfo.GetDrives())
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
                    // ClientAutomation.Helper.CommonHelper.TraceLine(string.Format("******* {0}:     {1}:  *******", pdata.Name, pdata.Value));
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
    }


}

