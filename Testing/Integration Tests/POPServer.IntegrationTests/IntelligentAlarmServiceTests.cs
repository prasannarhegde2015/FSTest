using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CxCvsLib;
using CygNet.API.Historian;
using CygNet.COMAPI.Client;
using CygNet.COMAPI.Interfaces;
using CygNet.Data.Historian;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.DynaCardLibrary.API.Enums;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using Weatherford.POP.Interfaces;
using ICvsClient = CxCvsLib.ICvsClient;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class IntelligentAlarmsServiceTests : APIClientTestBase
    {
        protected static ICvsClient _cvsClient;
        protected static ICvsClient CvsClient { get { return _cvsClient ?? (_cvsClient = CreateCvsClient()); } }
        private static object s_lockObj = new object();
        private static ClientProxyManager s_clientProxyMgr;
        private int failcount = 0;
        private bool isEdge = false;
        private static ClientProxyManager ClientProxyMgr
        {
            get
            {
                lock (s_lockObj)
                {
                    return s_clientProxyMgr ?? (s_clientProxyMgr = new ClientProxyManager());
                }
            }
        }


        public object SuccessMessage { get; private set; }

        protected static ICvsClient CreateCvsClient()
        {
            var cvsDomainSiteSvc = GetDomainSiteService("UIS");

            ICvsClient cvsClient = new CvsClient();
            try
            {
                cvsClient.Connect(cvsDomainSiteSvc.GetDomainSiteService());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to connect to {cvsDomainSiteSvc.GetDomainSiteService()}: {ex.Message}");
                System.Diagnostics.Trace.WriteLine($"Failed to connect to {cvsDomainSiteSvc.GetDomainSiteService()}: {ex.ToString()}");
            }

            if (!cvsClient.IsConnected)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to connect to {cvsDomainSiteSvc.GetDomainSiteService()}.");
            }

            return cvsClient;
        }

        protected static Client _vhsClient = new Client(GetDomainSiteService("VHS"));
        protected static Dictionary<Name, List<HistoricalEntry>> _entriesByName = new Dictionary<Name, List<HistoricalEntry>>();
        List<AlarmTypeDTO> _alarmTypesToRemove;

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            WellDTO addWell = SetDefaultFluidType(new WellDTO { Name = "new_Well", FacilityId = "new_Well", DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI_1", SubAssemblyAPI = "SubAssemblyAPI_1", AssemblyAPI = "AssemblyAPI_2", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = addWell });
            WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(addWell.Name));
            Assert.IsNotNull(addedWell);
            _wellsToRemove.Add(addedWell);

            WellDTO addEspWell = SetDefaultFluidType(new WellDTO { Name = "new_Esp_Well", FacilityId = GetFacilityId("ESPWELL_", 1), DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "EspIntervalAPI_1", SubAssemblyAPI = "EspSubAssemblyAPI_1", AssemblyAPI = "EspAssemblyAPI_2", CommissionDate = DateTime.Today, WellType = WellTypeId.ESP });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = addEspWell });
            WellDTO addedEspWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(addEspWell.Name));
            Assert.IsNotNull(addedEspWell);
            _wellsToRemove.Add(addedEspWell);

            WellDTO addGlWell = SetDefaultFluidType(new WellDTO { Name = "new_Gl_Well", FacilityId = "new_Gl_Well", DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "GlIntervalAPI_1", SubAssemblyAPI = "GlSubAssemblyAPI_1", AssemblyAPI = "GlAssemblyAPI_2", CommissionDate = DateTime.Today, WellType = WellTypeId.GLift });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = addGlWell });
            WellDTO addedGlWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(addGlWell.Name));
            Assert.IsNotNull(addedGlWell);
            _wellsToRemove.Add(addedGlWell);

            _alarmTypesToRemove = new List<AlarmTypeDTO>();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            WellDTO addedwell = WellService.GetAllWells().FirstOrDefault(aw => aw.Name == "new_Well");
            if (addedwell != null)
            {
                WellSettingDTO[] wellSettings = SettingService.GetWellSettingsByWellId(addedwell.Id.ToString());
                if (wellSettings != null && wellSettings.Length > 0)
                {
                    foreach (WellSettingDTO wellstng in wellSettings)
                    {
                        if (wellstng.WellId == addedwell.Id)
                        {
                            SettingService.RemoveWellSetting(wellstng.Id.ToString());
                        }
                    }
                }
            }

            WellDTO addedEspwell = WellService.GetAllWells().FirstOrDefault(aw => aw.Name == "new_Esp_Well");
            if (addedEspwell != null)
            {
                WellSettingDTO[] espWellSettings = SettingService.GetWellSettingsByWellId(addedEspwell.Id.ToString());
                if (espWellSettings != null && espWellSettings.Length > 0)
                {
                    foreach (WellSettingDTO wellstng in espWellSettings)
                    {
                        if (wellstng.WellId == addedEspwell.Id)
                        {
                            SettingService.RemoveWellSetting(wellstng.Id.ToString());
                        }
                    }
                }
            }

            WellDTO addedGlwell = WellService.GetAllWells().FirstOrDefault(aw => aw.Name == "new_Gl_Well");
            if (addedEspwell != null)
            {
                WellSettingDTO[] glWellSettings = SettingService.GetWellSettingsByWellId(addedGlwell.Id.ToString());
                if (glWellSettings != null && glWellSettings.Length > 0)
                {
                    foreach (WellSettingDTO wellstng in glWellSettings)
                    {
                        if (wellstng.WellId == addedGlwell.Id)
                        {
                            SettingService.RemoveWellSetting(wellstng.Id.ToString());
                        }
                    }
                }
            }

            //DeleteHistoricalValuesFromVHS();
            base.Cleanup();
            foreach (AlarmTypeDTO alarmTypeToRemove in _alarmTypesToRemove)
            {
                try
                {
                    AlarmService.RemoveAlarmType(alarmTypeToRemove.Id.ToString());
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Failed to remove alarm type {alarmTypeToRemove.AlarmType} ({alarmTypeToRemove.Id}): {ex.ToString()}");
                }
            }
        }

        /// <summary>
        /// Tests the algorithms that generate the following ESP intelligent alarms:
        /// 1. Gas lock
        /// 2. Broken shaft
        /// 3. Intake plugged
        /// 4. Reduced inflow
        /// </summary>
        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void IntelligentAlarmCheckForSingleWellNonRRLTest()
        {
            AlarmTypeDTO[] alarmTypes = AlarmService.GetAlarmTypes();
            _intelligentAlarmService = _serviceFactory.GetService<IIntelligentAlarmService>();

            WellDTO[] allwells = WellService.GetAllWells();
            WellDTO well = allwells.FirstOrDefault(w => w.Name == "new_Esp_Well");

            // ************************** FORCE GAS_LOCK ALARM ************************************
            //Set Value from  Well level and see if it works !!
            //FRWM-6267 :  EDGE - Expose Intelligent Alarm Timespans & Thresholds ( Implement for ESP )
            // FRWM-6666 : API testing Sub task update to include limits via Well level
            string[] ESPIntAlmSettingNames = new string[]
            {
                 "ESP Gas Lock: PDP Multiplier",
                 "ESP Gas Lock: PIP Multiplier",
                 "ESP Gas Lock:MA Multiplier",
                 "ESP Broken Shaft: PIP Multiplier",
                 "ESP Broken Shaft: MA Multiplier",
                 "ESP Intake Plugged: PDP Multiplier",
                 "ESP Intake Plugged: PIP Multiplier",
                 "ESP Intake Plugged: FTP Multiplier",
                 "ESP Reduced Inflow: PDP Multiplier",
                 "ESP Reduced Inflow: PIP Multiplier",
                 "ESP Reduced Inflow: FTP Multiplier",
                 "ESP Choke Plugged: FTP & FLP Multiplier",
                 "ESP Choke Plugged: PDP Multiplier",
                 "ESP Outage Block: DFP Multiplier",
                 "ESP Outage Block: PDP Multiplier",
                 "ESP Outage Block: FTP Multiplier",
                 "ESP Sand Bridge: DFP Multiplier",
                 "ESP Fast Draw Down: PIP Multiplier",
                 "ESP High Casing Choke Differential Pressure: FLP Multiplier",
                 "ESP Low Casing Choke Differential Pressure: FLP Multiplier",
                 "ESP High Flowing Differential Pressure: MA Multiplier",
                 "ESP High Flowing Differential Pressure: FTP & FLP Multiplier",
                 "ESP Reverse Rotation: PIP Multiplier",
                 "ESP Reverse Rotation: MT Multiplier"
            };
            double[] numericvalues = new double[]
            {
                0.1,
                0.1,
                0.1,
                0.4,
                0.4,
                0.1,
                0.1,
                1.05,
                0.1,
                0.1,
                0.1,
                1.5,
                0.1,
                0.1,
                0.1,
                0.1,
                50,
                100,
                50,
                10,
                0.1,
                1.5,
                0.4,
                0.2
            };
            //Send Default Values and chekc if alarm Trigggered or not
            UpdateForeSiteESPAlarmValuesToClearAlarms(well, ESPIntAlmSettingNames, numericvalues);
            // Add the daily average data to generate the alarm for GAS_LOCK
            WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = well.Id,
                EndDateTime = DateTime.Now.ToUniversalTime(),                   // today
                StartDateTime = DateTime.Now.AddDays(-3).ToUniversalTime(),     // 3 days ago
                GasRateAllocated = 4000,
                GasRateInferred = 4100,
                OilRateAllocated = 50,
                OilRateInferred = 55,
                WaterRateAllocated = 32,
                WaterRateInferred = 35,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = well.Id,
                DHPG = 300,
                PDP = 2000,
                PIP = 100,
                MotorAmps = 34,
                FLP = 678,
            };
            bool result = SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
            Assert.IsTrue(result, "SurveillanceService.AddUpdateWellDailyAverageData failed while attempting to add a daily average record for gas lock alarm.");

            // Add daily average data from 5 days ago to 3 days ago
            dailyAverageDTO.StartDateTime = DateTime.Now.AddDays(-5).ToUniversalTime(); // 5 days ago
            dailyAverageDTO.EndDateTime = DateTime.Now.AddDays(-3).ToUniversalTime();   // 3 days ago
            result = SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
            Assert.IsTrue(result, "SurveillanceService.AddUpdateWellDailyAverageData failed while attempting to add a daily average record for gas lock alarm.");

            // Generate an ESP intelligent alarm for GAS_LOCK
            result = _intelligentAlarmService.RunESPStatusCheck(well);
            Assert.IsTrue(result, "_intelligentAlarmService.RunESPStatusCheck failed while attempting to produce a 'Gas Lock' alarm.");

            // Read the intelligent alarm that was generated. There should only be 1 in the alarm table.
            CurrentAlarmDTO[] gasLockAlarms = AlarmService.GetCurrentAlarmsByWellId(well.Id.ToString());

            // Find the gas lock alarm type Id.
            AlarmTypeDTO gasLockAlarmType = alarmTypes.FirstOrDefault(at => at.AlarmType.Equals("Gas Lock"));

            // Make sure the alarm was generated and that it was a gas lock alarm.
            Assert.IsNotNull(gasLockAlarms, "Gas lock alarm was not generated.");

            // Check for equality between the expected alarm type and the generated alarm type.
            //Assert.AreEqual(gasLockAlarms[0].AlarmTypeId, gasLockAlarmType.Id, "Expected alarm type was " + gasLockAlarmType.Id.ToString() + " (gas lock). Generated alarm type was " + gasLockAlarms[0].AlarmTypeId, ToString());

            // Remove the gas lock alarm
            AlarmService.RemoveAlarmsByWellId(well.Id.ToString());

            // Remove the GAS_LOCK daily averages
            SurveillanceService.RemoveDailyAverageByWellId(well.Id.ToString());

            // ************************** FORCE 'Broken Shaft' ALARM ************************************

            // Add the daily averages that will generate an ESP alarm for BROKEN_SHAFT
            dailyAverageDTO.MotorAmps = 80;
            dailyAverageDTO.EndDateTime = DateTime.Now.ToUniversalTime();               // today
            dailyAverageDTO.StartDateTime = DateTime.Now.AddDays(-3).ToUniversalTime(); // 3 days ago
            result = SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
            Assert.IsTrue(result, "SurveillanceService.AddUpdateWellDailyAverageData failed while attempting to add a daily average record for broken shaft alarm.");

            dailyAverageDTO.StartDateTime = DateTime.Now.AddDays(-5).ToUniversalTime(); // 5 days ago
            dailyAverageDTO.EndDateTime = DateTime.Now.AddDays(-3).ToUniversalTime();   // 3 days ago
            result = SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
            Assert.IsTrue(result, "SurveillanceService.AddUpdateWellDailyAverageData failed while attempting to add a daily average record for broken shaft alarm.");

            // Generate an ESP intelligent alarm for 'Broken Shaft'
            result = _intelligentAlarmService.RunESPStatusCheck(well);
            Assert.IsTrue(result, "_intelligentAlarmService.RunESPStatusCheck failed while attempting to produce a 'Broken Shaft' alarm.");

            // Read the intelligent alarm that was generated.
            CurrentAlarmDTO[] brokenShaftAlarms = AlarmService.GetCurrentAlarmsByWellId(well.Id.ToString());

            // Make sure the alarm was generated and that it was a broken shaft alarm.
            Assert.IsNotNull(brokenShaftAlarms, "Broken shaft alarm was not generated.");

            // Find the broken shaft alarm type Id.
            AlarmTypeDTO brokenShaftAlarmType = alarmTypes.FirstOrDefault(at => at.AlarmType.Equals("Broken Shaft"));

            //Assert.AreEqual(brokenShaftAlarms.FirstOrDefault(x => x.AlarmType.AlarmType == "Broken Shaft").AlarmTypeId, brokenShaftAlarmType.Id, "Expected alarm type was " + brokenShaftAlarmType.Id.ToString() + " (broken shaft). Generated alarm type was " + brokenShaftAlarms[0].AlarmTypeId, ToString());

            // Remove the broken shaft alarm
            AlarmService.RemoveAlarmsByWellId(well.Id.ToString());

            // Delete of the BROKEN_SHAFT daily averages
            SurveillanceService.RemoveDailyAverageByWellId(well.Id.ToString());

            // ************************** FORCE 'Intake Plugged' ALARM ************************************

            // Add the daily averages to generate the intake plugged alarm
            dailyAverageDTO.PDP = 500;
            dailyAverageDTO.PIP = 1200;
            dailyAverageDTO.EndDateTime = DateTime.Now.ToUniversalTime();               // today
            dailyAverageDTO.StartDateTime = DateTime.Now.AddDays(-3).ToUniversalTime(); // 3 days ago
            result = SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
            Assert.IsTrue(result, "SurveillanceService.AddUpdateWellDailyAverageData failed while attempting to add a daily average record for intake plugged alarm.");

            dailyAverageDTO.StartDateTime = DateTime.Now.AddDays(-5).ToUniversalTime(); // 5 days ago
            dailyAverageDTO.EndDateTime = DateTime.Now.AddDays(-3).ToUniversalTime();   // 3 days ago
            result = SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
            Assert.IsTrue(result, "SurveillanceService.AddUpdateWellDailyAverageData failed while attempting to add a daily average record for intake plugged alarm.");

            // Generate the ESP intelligent alarm for 'Intake Plugged'
            result = _intelligentAlarmService.RunESPStatusCheck(well);
            Assert.IsTrue(result, "_intelligentAlarmService.RunESPStatusCheck failed while attempting to produce an 'Intake Plugged' alarm.");

            // Read the intelligent alarm that was generated.
            CurrentAlarmDTO[] intakePluggedAlarms = AlarmService.GetCurrentAlarmsByWellId(well.Id.ToString());

            // Make sure the alarm was generated and that it was a intake plugged alarm.
            Assert.IsNotNull(intakePluggedAlarms, "Intake plugged alarm was not generated.");

            // Find the broken shaft alarm type Id.
            AlarmTypeDTO intakePluggedAlarmType = alarmTypes.FirstOrDefault(at => at.AlarmType.Equals("Intake Plugged"));

            // Test for equality between the known intake plugged alarm type and the generated alarm type.
            //Assert.AreEqual(intakePluggedAlarms[0].AlarmTypeId, intakePluggedAlarmType.Id, "Expected alarm type was " + intakePluggedAlarmType.Id + " (intake plugged). Generated alarm type was " + intakePluggedAlarms[0].AlarmTypeId, ToString());

            // Remove the 'intake plugged' alarm
            AlarmService.RemoveAlarmsByWellId(well.Id.ToString());

            // Delete the 'intake plugged' daily averages
            SurveillanceService.RemoveDailyAverageByWellId(well.Id.ToString());

            // ************************** FORCE 'Reduced Inflow' ALARM ************************************

            // Add the daily averages to generate the 'reduced inflow' alarm
            dailyAverageDTO.PDP = 1500;
            dailyAverageDTO.PIP = 1200;
            dailyAverageDTO.EndDateTime = DateTime.Now.ToUniversalTime();               // today
            dailyAverageDTO.StartDateTime = DateTime.Now.AddDays(-3).ToUniversalTime(); // 3 days ago
            result = SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
            Assert.IsTrue(result, "SurveillanceService.AddUpdateWellDailyAverageData failed while attempting to add a daily average record for reduced inflow alarm.");

            dailyAverageDTO.StartDateTime = DateTime.Now.AddDays(-5).ToUniversalTime(); // 5 days ago
            dailyAverageDTO.EndDateTime = DateTime.Now.AddDays(-3).ToUniversalTime();   // 3 days ago
            result = SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
            Assert.IsTrue(result, "SurveillanceService.AddUpdateWellDailyAverageData failed while attempting to add a daily average record for reduced inflow alarm.");

            // Generate the ESP intelligent alarm for 'Reduced Inflow'
            result = _intelligentAlarmService.RunESPStatusCheck(well);
            Assert.IsTrue(result, "_intelligentAlarmService.RunESPStatusCheck failed while attempting to produce a 'Reduced Inflow' alarm.");

            // Read the intelligent alarm that was generated.
            CurrentAlarmDTO[] reducedInflowAlarms = AlarmService.GetCurrentAlarmsByWellId(well.Id.ToString());

            // Make sure the alarm was generated and that it was a reduced inflow alarm.
            Assert.IsNotNull(reducedInflowAlarms, "Reduced inflow alarm was not generated.");

            // Find the reduced inflow alarm type Id.
            AlarmTypeDTO reducedInflowAlarmType = alarmTypes.FirstOrDefault(at => at.AlarmType.Equals("Reduced Inflow"));

            //Assert.AreEqual(reducedInflowAlarms[0].AlarmTypeId, reducedInflowAlarmType.Id, "Expected alarm type was " + reducedInflowAlarmType.Id.ToString() + " (reduced inflow). Generated alarm type was " + reducedInflowAlarms[0].AlarmTypeId, ToString());

            // Remove the reduced inflow alarm
            AlarmService.RemoveAlarmsByWellId(well.Id.ToString());

            // Delete the 'Reduced Inflow' daily averages
            SurveillanceService.RemoveDailyAverageByWellId(well.Id.ToString());
        }

        /// <summary>
        /// Tests the algorithms that generate the following ESP intelligent alarms:
        /// 1. Gas lock
        /// 2. Broken shaft
        /// 3. Intake plugged
        /// 4. Reduced inflow
        /// </summary>
        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void ESPIntelligentAlarmCheckForSingleWellAlarmSettingsModified()
        {
            SurveillanceServiceTests svtst = new SurveillanceServiceTests();


            AlarmTypeDTO[] alarmTypes = AlarmService.GetAlarmTypes();
            _intelligentAlarmService = _serviceFactory.GetService<IIntelligentAlarmService>();
            string start_date = (DateTime.Today - TimeSpan.FromDays(30)).ToUniversalTime().ToISO8601();
            string end_date = DateTime.Today.ToUniversalTime().ToISO8601();
            WellDTO[] allwells = WellService.GetAllWells();
            WellDTO well = allwells.FirstOrDefault(w => w.Name == "new_Esp_Well");
            try
            {
                // ************************** FORCE GAS_LOCK ALARM ************************************
                //Set Value from  Well level and see if it works !!
                //FRWM-6267 :  EDGE - Expose Intelligent Alarm Timespans & Thresholds ( Implement for ESP )
                // FRWM-6666 : API testing Sub task update to include limits via Well level
                string[] ESPIntAlmSettingNames = new string[]
                {
                 "ESP Gas Lock: PDP Multiplier",
                 "ESP Gas Lock: PIP Multiplier",
                 "ESP Gas Lock:MA Multiplier",
                 "ESP Broken Shaft: PIP Multiplier",
                 "ESP Broken Shaft: MA Multiplier",
                 "ESP Intake Plugged: PDP Multiplier",
                 "ESP Intake Plugged: PIP Multiplier",
                 "ESP Intake Plugged: FTP Multiplier",
                 "ESP Reduced Inflow: PDP Multiplier",
                 "ESP Reduced Inflow: PIP Multiplier",
                 "ESP Reduced Inflow: FTP Multiplier",
                 "ESP Choke Plugged: FTP & FLP Multiplier",
                 "ESP Choke Plugged: PDP Multiplier",
                 "ESP Outage Block: DFP Multiplier",
                 "ESP Outage Block: PDP Multiplier",
                 "ESP Outage Block: FTP Multiplier",
                 "ESP Sand Bridge: DFP Multiplier",
                 "ESP Fast Draw Down: PIP Multiplier",
                 "ESP High Casing Choke Differential Pressure: FLP Multiplier",
                 "ESP Low Casing Choke Differential Pressure: FLP Multiplier",
                 "ESP High Flowing Differential Pressure: MA Multiplier",
                 "ESP High Flowing Differential Pressure: FTP & FLP Multiplier",
                 "ESP Reverse Rotation: PIP Multiplier",
                 "ESP Reverse Rotation: MT Multiplier"
                };
                double[] numericvalues = new double[]
                {
                0.3,
                0.1,
                0.1,
                0.4,
                0.4,
                0.1,
                0.1,
                1.05,
                0.1,
                0.1,
                0.1,
                1.5,
                0.1,
                0.1,
                0.1,
                0.1,
                50,
                100,
                50,
                10,
                0.1,
                1.5,
                0.4,
                0.2
                };
                //Send Default Values and chekc if alarm Trigggered or not
                UpdateForeSiteESPAlarmValuesToClearAlarms(well, ESPIntAlmSettingNames, numericvalues);
                // Add the daily average data to generate the alarm for GAS_LOCK
                WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO
                {
                    Id = well.Id,
                    EndDateTime = DateTime.Today.ToUniversalTime(),                    // Yesterday
                    StartDateTime = DateTime.Today.AddDays(-1).ToUniversalTime(),     // 2 days ago
                    GasRateAllocated = 4000,
                    GasRateInferred = 4100,
                    OilRateAllocated = 50,
                    OilRateInferred = 55,
                    WaterRateAllocated = 32,
                    WaterRateInferred = 35,
                    Status = WellDailyAverageDataStatus.Calculated,
                    Duration = 24,
                    GasJectionDepth = 1000,
                    ChokeDiameter = 64.0,
                    RunTime = 24,
                    THP = 492,
                    THT = 213,
                    GasInjectionRate = 5280,
                    WellId = well.Id,
                    DHPG = 300,
                    PDP = 2000,
                    PIP = 100,
                    MotorAmps = 34,
                    FLP = 678,
                };
                bool result = SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
                Assert.IsTrue(result, "SurveillanceService.AddUpdateWellDailyAverageData failed while attempting to add a daily average record for gas lock alarm.");
                //408 ,1148, 17

                svtst.UpdateCygNetUISValues(well, "PIP", "408");
                svtst.UpdateCygNetUISValues(well, "PDP", "1148");
                svtst.UpdateCygNetUISValues(well, "AMPMOTOR", "17");
                SurveillanceServiceClient.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                // Generate an ESP intelligent alarm for GAS_LOCK
                result = _intelligentAlarmService.RunESPStatusCheck(well);
                Assert.IsTrue(result, "_intelligentAlarmService.RunESPStatusCheck failed while attempting to produce a 'Gas Lock' alarm.");
                SurveillanceServiceClient.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                // Read the intelligent alarm that was generated. There should only be 1 in the alarm table.
                CurrentAlarmDTO[] gasLockAlarms = AlarmService.GetCurrentAlarmsByWellId(well.Id.ToString());

                // Find the gas lock alarm type Id.
                AlarmTypeDTO gasLockAlarmType = alarmTypes.FirstOrDefault(at => at.AlarmType.Equals("Gas Lock"));

                // Make sure the alarm was generated and that it was a gas lock alarm.
                Assert.IsNotNull(gasLockAlarms, "Gas lock alarm was not generated.");


                var wellstatus = SurveillanceService.GetWellStatusData(well.Id.ToString());
                ESPWellStatusValueDTO espval = (ESPWellStatusValueDTO)wellstatus.Value;
                List<string> expalamlist = new List<string>();
                expalamlist.Add("Gas Lock");
                //expalamlist.Add("Broken Shaft");
                //expalamlist.Add("Low Casing Choke Differential Pressure");
                //expalamlist.Add("Low Flowing Differential Pressure");
                //
                VerifyForeSiteAlarmsOnWellStatusPage(espval.AlarmMessage, expalamlist, true);
                VerifyAlarmsOnAllAlarmsHistoryPage(well, expalamlist, start_date, end_date, true);
                VerifyAlarmsOnForesiteAlarmsHistoryPage(well, expalamlist, start_date, end_date, true);
                // Check for equality between the expected alarm type and the generated alarm type.
                //Assert.AreEqual(gasLockAlarms[0].AlarmTypeId, gasLockAlarmType.Id, "Expected alarm type was " + gasLockAlarmType.Id.ToString() + " (gas lock). Generated alarm type was " + gasLockAlarms[0].AlarmTypeId, ToString());
                expalamlist.Remove("Gas Lock");
                // Remove the gas lock alarm
                AlarmService.RemoveAlarmsByWellId(well.Id.ToString());

                // Remove the GAS_LOCK daily averages
                SurveillanceService.RemoveDailyAverageByWellId(well.Id.ToString());

                // ************************** FORCE 'Broken Shaft' ALARM ************************************

                // Add the daily averages that will generate an ESP alarm for BROKEN_SHAFT
                dailyAverageDTO.MotorAmps = 80;
                dailyAverageDTO.EndDateTime = DateTime.Now.ToUniversalTime();               // today
                dailyAverageDTO.StartDateTime = DateTime.Now.AddDays(-3).ToUniversalTime(); // 3 days ago
                result = SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
                Assert.IsTrue(result, "SurveillanceService.AddUpdateWellDailyAverageData failed while attempting to add a daily average record for broken shaft alarm.");

                dailyAverageDTO.StartDateTime = DateTime.Now.AddDays(-5).ToUniversalTime(); // 5 days ago
                dailyAverageDTO.EndDateTime = DateTime.Now.AddDays(-3).ToUniversalTime();   // 3 days ago
                result = SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
                Assert.IsTrue(result, "SurveillanceService.AddUpdateWellDailyAverageData failed while attempting to add a daily average record for broken shaft alarm.");
                expalamlist.Add("Broken Shaft");
                // Generate an ESP intelligent alarm for 'Broken Shaft'
                result = _intelligentAlarmService.RunESPStatusCheck(well);
                SurveillanceServiceClient.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                wellstatus = SurveillanceService.GetWellStatusData(well.Id.ToString());
                espval = (ESPWellStatusValueDTO)wellstatus.Value;
                VerifyForeSiteAlarmsOnWellStatusPage(espval.AlarmMessage, expalamlist, true);
                VerifyAlarmsOnAllAlarmsHistoryPage(well, expalamlist, start_date, end_date, true);
                VerifyAlarmsOnForesiteAlarmsHistoryPage(well, expalamlist, start_date, end_date, true);
                Assert.IsTrue(result, "_intelligentAlarmService.RunESPStatusCheck failed while attempting to produce a 'Broken Shaft' alarm.");

                // Read the intelligent alarm that was generated.
                CurrentAlarmDTO[] brokenShaftAlarms = AlarmService.GetCurrentAlarmsByWellId(well.Id.ToString());

                // Make sure the alarm was generated and that it was a broken shaft alarm.
                Assert.IsNotNull(brokenShaftAlarms, "Broken shaft alarm was not generated.");

                // Find the broken shaft alarm type Id.
                AlarmTypeDTO brokenShaftAlarmType = alarmTypes.FirstOrDefault(at => at.AlarmType.Equals("Broken Shaft"));

                //Assert.AreEqual(brokenShaftAlarms.FirstOrDefault(x => x.AlarmType.AlarmType == "Broken Shaft").AlarmTypeId, brokenShaftAlarmType.Id, "Expected alarm type was " + brokenShaftAlarmType.Id.ToString() + " (broken shaft). Generated alarm type was " + brokenShaftAlarms[0].AlarmTypeId, ToString());

                // Remove the broken shaft alarm
                AlarmService.RemoveAlarmsByWellId(well.Id.ToString());

                // Delete of the BROKEN_SHAFT daily averages
                SurveillanceService.RemoveDailyAverageByWellId(well.Id.ToString());

                // ************************** FORCE 'Intake Plugged' ALARM ************************************

                // Add the daily averages to generate the intake plugged alarm
                dailyAverageDTO.PDP = 500;
                dailyAverageDTO.PIP = 1200;
                dailyAverageDTO.EndDateTime = DateTime.Now.ToUniversalTime();               // today
                dailyAverageDTO.StartDateTime = DateTime.Now.AddDays(-3).ToUniversalTime(); // 3 days ago
                result = SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
                Assert.IsTrue(result, "SurveillanceService.AddUpdateWellDailyAverageData failed while attempting to add a daily average record for intake plugged alarm.");

                dailyAverageDTO.StartDateTime = DateTime.Now.AddDays(-5).ToUniversalTime(); // 5 days ago
                dailyAverageDTO.EndDateTime = DateTime.Now.AddDays(-3).ToUniversalTime();   // 3 days ago
                result = SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
                Assert.IsTrue(result, "SurveillanceService.AddUpdateWellDailyAverageData failed while attempting to add a daily average record for intake plugged alarm.");

                // Generate the ESP intelligent alarm for 'Intake Plugged'
                result = _intelligentAlarmService.RunESPStatusCheck(well);
                Assert.IsTrue(result, "_intelligentAlarmService.RunESPStatusCheck failed while attempting to produce an 'Intake Plugged' alarm.");

                // Read the intelligent alarm that was generated.
                CurrentAlarmDTO[] intakePluggedAlarms = AlarmService.GetCurrentAlarmsByWellId(well.Id.ToString());

                // Make sure the alarm was generated and that it was a intake plugged alarm.
                Assert.IsNotNull(intakePluggedAlarms, "Intake plugged alarm was not generated.");

                // Find the broken shaft alarm type Id.
                AlarmTypeDTO intakePluggedAlarmType = alarmTypes.FirstOrDefault(at => at.AlarmType.Equals("Intake Plugged"));

                // Test for equality between the known intake plugged alarm type and the generated alarm type.
                //Assert.AreEqual(intakePluggedAlarms[0].AlarmTypeId, intakePluggedAlarmType.Id, "Expected alarm type was " + intakePluggedAlarmType.Id + " (intake plugged). Generated alarm type was " + intakePluggedAlarms[0].AlarmTypeId, ToString());

                // Remove the 'intake plugged' alarm
                AlarmService.RemoveAlarmsByWellId(well.Id.ToString());

                // Delete the 'intake plugged' daily averages
                SurveillanceService.RemoveDailyAverageByWellId(well.Id.ToString());

                // ************************** FORCE 'Reduced Inflow' ALARM ************************************

                // Add the daily averages to generate the 'reduced inflow' alarm
                dailyAverageDTO.PDP = 1500;
                dailyAverageDTO.PIP = 1200;
                dailyAverageDTO.EndDateTime = DateTime.Now.ToUniversalTime();               // today
                dailyAverageDTO.StartDateTime = DateTime.Now.AddDays(-3).ToUniversalTime(); // 3 days ago
                result = SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
                Assert.IsTrue(result, "SurveillanceService.AddUpdateWellDailyAverageData failed while attempting to add a daily average record for reduced inflow alarm.");

                dailyAverageDTO.StartDateTime = DateTime.Now.AddDays(-5).ToUniversalTime(); // 5 days ago
                dailyAverageDTO.EndDateTime = DateTime.Now.AddDays(-3).ToUniversalTime();   // 3 days ago
                result = SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
                Assert.IsTrue(result, "SurveillanceService.AddUpdateWellDailyAverageData failed while attempting to add a daily average record for reduced inflow alarm.");

                // Generate the ESP intelligent alarm for 'Reduced Inflow'
                result = _intelligentAlarmService.RunESPStatusCheck(well);
                Assert.IsTrue(result, "_intelligentAlarmService.RunESPStatusCheck failed while attempting to produce a 'Reduced Inflow' alarm.");

                // Read the intelligent alarm that was generated.
                CurrentAlarmDTO[] reducedInflowAlarms = AlarmService.GetCurrentAlarmsByWellId(well.Id.ToString());

                // Make sure the alarm was generated and that it was a reduced inflow alarm.
                Assert.IsNotNull(reducedInflowAlarms, "Reduced inflow alarm was not generated.");

                // Find the reduced inflow alarm type Id.
                AlarmTypeDTO reducedInflowAlarmType = alarmTypes.FirstOrDefault(at => at.AlarmType.Equals("Reduced Inflow"));

                //Assert.AreEqual(reducedInflowAlarms[0].AlarmTypeId, reducedInflowAlarmType.Id, "Expected alarm type was " + reducedInflowAlarmType.Id.ToString() + " (reduced inflow). Generated alarm type was " + reducedInflowAlarms[0].AlarmTypeId, ToString());

                // Remove the reduced inflow alarm
                AlarmService.RemoveAlarmsByWellId(well.Id.ToString());

                // Delete the 'Reduced Inflow' daily averages
                SurveillanceService.RemoveDailyAverageByWellId(well.Id.ToString());
            }
            finally
            {
                svtst.RevertCygNetUISValue(well);
            }
        }



        /// <summary>
        /// User Story : FRWM-6690 : EDGE - Expose Intelligent Alarm Timespans & Thresholds (PCP )
        /// API Story : FRWM-6833 :
        /// </summary>
        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void PCPIntelligentAlarmCheckwithConfigurableMultipliers()
        {

            string facilityId = GetFacilityId("WFTA1K_", 2);
            try
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");

                //Adding PCP Well along with Well Test Data.....
                #region AddWell
                //Adding Well along with WellTest Data with PI Tuning Method
                WellDTO pcpWell = AddNonRRLWell(facilityId, WellTypeId.PCP, false, CalibrationMethodId.PI);
                _wellsToRemove.Add(pcpWell);

                string[] PCPIntAlmSettingNames = new string[]
                {
                "PCP Sand Bridge: MA Multiplier",
                "PCP Sand Bridge: DP Multiplier",
                "PCP Intake Plugged: PDP Multiplier",
                "PCP Intake Plugged: PIP Multiplier",
                "PCP Intake Plugged: FTP Multiplier",
                "PCP Reduced Inflow: PIP Multiplier",
                "PCP Reduced Inflow: PDP Multiplier",
                "PCP Reduced Inflow: FTP Multiplier",
                "PCP Fast Draw Down: PIP Multiplier",
                "PCP High Torque: PIP Multiplier",
                "PCP High Torque: PS Multiplier",
                "PCP Pump Press Untuned: DPG Multiplier",
                "PCP Pump Press Untuned: DPL Multiplier",
                "PCP Pump Press Untuned: PDPG Multiplier",
                "PCP Pump Press Untuned: PDPL Multiplier"
                };
                //Update with mulipliers 
                double[] numericvalues = new double[]
                {
                        0,100,0.1,0.1,0.1,0.1,1,0.1,70,0.05,0.05,2,2,2,2
                };
                //Send Default Values and chekc if alarm Trigggered or not
                UpdateForeSiteESPAlarmValuesToClearAlarms(pcpWell, PCPIntAlmSettingNames, numericvalues);
                if (s_isRunningInATS && s_domain.Equals("9077") == false)
                {
                    // Since No DG Trasnscatiosn are found on Fresh CygNet install for WFTA1K, this step is must on ATS CygNet
                    Trace.WriteLine($"Running In ATS: Scan Command Sent  By Force since  Running ATS = {s_isRunningInATS}  and CygNet Domain {s_domain}");
                    SurveillanceService.IssueCommandForSingleWell(pcpWell.Id, WellCommand.DemandScan.ToString());
                }
                #endregion AddWell

                #region WellTest Tuning

                AddWellSettingWithDoubleValues(pcpWell.Id, "Min Productivity Index Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max Productivity Index Acceptance Limit", 1.0);

                AddWellSettingWithDoubleValues(pcpWell.Id, "Min Water Cut Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max Water Cut Acceptance Limit", 1.0);

                AddWellSettingWithDoubleValues(pcpWell.Id, "Min GOR Acceptance Limit", 1.0);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max GOR Acceptance Limit", 50.0);

                WellTestDTO latestTestData_PCP = WellTestDataService.GetLatestWellTestDataByWellId(pcpWell.Id.ToString());
                WellTestDataService.TuneSelectedWellTests(new long[] { latestTestData_PCP.Id });

                WellTestDTO latestTestDataAfterTune_PCP = WellTestDataService.GetLatestWellTestDataByWellId(pcpWell.Id.ToString());
                Assert.AreEqual("TUNING_SUCCEEDED", latestTestDataAfterTune_PCP.Status.ToString(), "Well Test Status is not Success");

                #endregion WellTest Tuning

                //Enter WellDailyAverge from DB
                #region WellDailyAverage

                WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO
                {
                    EndDateTime = DateTime.Today.ToUniversalTime(),
                    StartDateTime = DateTime.Today.AddDays(-1).ToUniversalTime(),
                    WellId = pcpWell.Id,
                    Id = 0,
                    RunTime = 24,
                    Status = WellDailyAverageDataStatus.Original,
                    THP = 860,
                    THT = 99.1,
                    CHP = 146.77,
                    PIP = 2100,
                    PDP = 320,
                    CasingGasRate = 1.40,
                    FLP = 8.35,
                    OilRateAllocated = 268.69,
                    WaterRateAllocated = 177.8,
                    MotorAmps = 10.37,
                    MotorVolts = 38.13,
                    MotorTemperature = 196.5,
                    PumpSpeed = 129.2,
                    PumpTorque = 88.84,
                    FBHP = 141.19,
                    OilRateInferred = 279.73,
                    WaterRateInferred = 187.90
                };
                bool addDailyAvgDataForPCP = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
                Assert.IsTrue(addDailyAvgDataForPCP, "Failed to add Daily Average data for day 1");

                dailyAverageDTO.EndDateTime = DateTime.Today.AddDays(-1).ToUniversalTime();
                dailyAverageDTO.StartDateTime = DateTime.Today.AddDays(-2).ToUniversalTime();

                dailyAverageDTO.THP = 862;
                dailyAverageDTO.PIP = 2105;
                dailyAverageDTO.PDP = 322;

                addDailyAvgDataForPCP = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
                Assert.IsTrue(addDailyAvgDataForPCP, "Failed to add Daily Average data for day 2");

                dailyAverageDTO.EndDateTime = DateTime.Today.AddDays(-2).ToUniversalTime();
                dailyAverageDTO.StartDateTime = DateTime.Today.AddDays(-3).ToUniversalTime();

                dailyAverageDTO.THP = 864;
                dailyAverageDTO.PIP = 2109;
                dailyAverageDTO.PDP = 325;

                addDailyAvgDataForPCP = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
                Assert.IsTrue(addDailyAvgDataForPCP, "Failed to add Daily Average data for day 3");

                #endregion WellDailyAverage

                bool result = IntelligentAlarmService.IntelligentAlarmCheckForSingleWellNonRRL(pcpWell.Id.ToString());
                CurrentAlarmDTO[] activeAlarms = IntelligentAlarmService.GetActiveIntelligentAlarmsByWellId(pcpWell.Id.ToString(), WellTypeId.PCP.ToString());
                Assert.AreEqual(2, activeAlarms.Length, "Expected intelligent setting count does not match.");
                List<string> expalamlist = new List<string>();
                expalamlist.Add("Intake Plugged");
                expalamlist.Add("Pump Press Untuned");
                SurveillanceService.IssueCommandForSingleWell(pcpWell.Id, WellCommand.DemandScan.ToString());
                Thread.Sleep(1000);
                var wellstatus = SurveillanceService.GetWellStatusData(pcpWell.Id.ToString());
                PCPWellStatusValueDTO pcpval = (PCPWellStatusValueDTO)wellstatus.Value;
                VerifyForeSiteAlarmsOnWellStatusPage(pcpval.AlarmMessage, expalamlist, true);

                //Verify If High Torue is disaplyed :
                //  PIP 200 RTUEMU default ; Speed 5 Torque : 20  torque limit set oit  default 20 1/5 of adialy
                dailyAverageDTO.EndDateTime = DateTime.Today.ToUniversalTime();
                dailyAverageDTO.StartDateTime = DateTime.Today.AddDays(-1).ToUniversalTime();
                dailyAverageDTO.PumpSpeed = 7;
                dailyAverageDTO.PIP = 200;
                dailyAverageDTO.MotorAmps = 0;
                dailyAverageDTO.PumpTorque = 10;
                addDailyAvgDataForPCP = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
                Assert.IsTrue(addDailyAvgDataForPCP, "Failed to add Daily Average data for day 1 modified ");

                dailyAverageDTO.EndDateTime = DateTime.Today.AddDays(-1).ToUniversalTime();
                dailyAverageDTO.StartDateTime = DateTime.Today.AddDays(-2).ToUniversalTime();
                dailyAverageDTO.PumpSpeed = 4;
                dailyAverageDTO.MotorAmps = 0;
                dailyAverageDTO.PIP = 100;
                dailyAverageDTO.PumpTorque = 10;

                addDailyAvgDataForPCP = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
                Assert.IsTrue(addDailyAvgDataForPCP, "Failed to add Daily Average data for 2 day before  modified ");

                result = IntelligentAlarmService.IntelligentAlarmCheckForSingleWellNonRRL(pcpWell.Id.ToString());
                activeAlarms = IntelligentAlarmService.GetActiveIntelligentAlarmsByWellId(pcpWell.Id.ToString(), WellTypeId.PCP.ToString());
                SurveillanceService.IssueCommandForSingleWell(pcpWell.Id, WellCommand.DemandScan.ToString());
                Thread.Sleep(1000);
                wellstatus = SurveillanceService.GetWellStatusData(pcpWell.Id.ToString());
                pcpval = (PCPWellStatusValueDTO)wellstatus.Value;
                expalamlist.Remove("Intake Plugged");
                expalamlist.Remove("Pump Press Untuned");
                expalamlist.Add("Fast Draw Down");
                expalamlist.Add("High Torque");
                expalamlist.Add("Sand Bridge");
                VerifyForeSiteAlarmsOnWellStatusPage(pcpval.AlarmMessage, expalamlist, true);
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                RemoveWell(facilityId);
            }
        }

        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void IntelligentAlarmServiceSettingTest()
        {
            WellDTO[] allwells = WellService.GetAllWells();
            WellDTO well = allwells.FirstOrDefault(w => w.Name == "new_Well");

            //Get Well Settings
            WellSettingDTO[] beforeWellSettings = IntelligentAlarmService.GetIntelligentAlarmSettings(well.Id.ToString()).OrderBy(s => s.SettingId).ToArray();
            Assert.AreEqual(15, beforeWellSettings.Length, "Expected intelligent setting count does not match.");

            WellSettingDTO[] twoDecimalSettings = beforeWellSettings.Where(s => s.Setting.SettingValueType == SettingValueType.DecimalNumber).Take(2).ToArray();

            WellSettingDTO expected = twoDecimalSettings[0];
            expected.NumericValue = 203.86;
            IntelligentAlarmService.AddIntelligentAlarmWellSetting(expected);
            WellSettingDTO actual = IntelligentAlarmService.GetIntelligentAlarmSettings(well.Id.ToString()).FirstOrDefault(s => s.Setting.Name == expected.Setting.Name);
            Assert.IsNotNull(actual, "Failed to get setting back after add.");
            Assert.AreEqual(expected.NumericValue, actual.NumericValue, "Numeric value does not match after add.");
            Assert.AreNotEqual(0, actual.Id, "Id should be non-zero after add.");

            twoDecimalSettings[0].NumericValue = 7382.382;
            IntelligentAlarmService.UpdateIntelligentAlarmWellSetting(expected);
            actual = IntelligentAlarmService.GetIntelligentAlarmSettings(well.Id.ToString()).FirstOrDefault(s => s.Setting.Name == expected.Setting.Name);
            Assert.IsNotNull(actual, "Failed to get setting back after update.");
            Assert.AreEqual(expected.NumericValue, actual.NumericValue, "Numeric value does not match after update.");
            Assert.AreNotEqual(0, actual.Id, "Id should be non-zero after update.");

            twoDecimalSettings[0].NumericValue = 4389.432;
            twoDecimalSettings[1].NumericValue = 832.4821;
            WellSettingDTO[] expectedArray = twoDecimalSettings;
            IntelligentAlarmService.UpdateIntelligentAlarmWellSettings(expectedArray);
            WellSettingDTO[] actualArray = IntelligentAlarmService.GetIntelligentAlarmSettings(well.Id.ToString()).Where(s => expectedArray.FirstOrDefault(e => e.SettingId == s.SettingId) != null).OrderBy(s => s.SettingId).ToArray();
            for (int ii = 0; ii < expectedArray.Length; ii++)
            {
                Assert.AreEqual(expectedArray[ii].NumericValue, actualArray[ii].NumericValue, $"Numeric value does not match after update multiple for item {ii}.");
                Assert.AreNotEqual(0, actualArray[ii].Id, $"Id should be non-zero after update multiple for item {ii}.");
            }

            foreach (WellSettingDTO toRemove in actualArray)
            {
                IntelligentAlarmService.RemoveIntelligentAlarmWellSetting(toRemove.Id.ToString());
            }
            WellSettingDTO[] afterRemoveWellSettings = IntelligentAlarmService.GetIntelligentAlarmSettings(well.Id.ToString()).OrderBy(s => s.SettingId).ToArray();
            foreach (WellSettingDTO shouldBeDefault in afterRemoveWellSettings)
            {
                Assert.AreEqual(0, shouldBeDefault.Id, $"Id for setting {shouldBeDefault.Setting.Name} should be zero after removing.");
            }
        }

        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void IntelligentSystemDefault()
        {
            WellDTO[] allwells = WellService.GetAllWells();
            WellDTO well = allwells.FirstOrDefault(w => w.Name == "new_Well");
            //Get Well Settings
            WellSettingDTO[] beforewellsettingsbyPK = IntelligentAlarmService.GetIntelligentAlarmSettings(well.Id.ToString());
            foreach (WellSettingDTO welStng in beforewellsettingsbyPK)
            {
                Assert.AreEqual(0, welStng.Id);
            }
            Assert.AreEqual(15, beforewellsettingsbyPK.Length, "Expected intelligent setting count does not match.");
            SystemSettingDTO[] sysSetting = IntelligentAlarmService.GetIntelligentAlarmSystemSettings();
            foreach (SystemSettingDTO intSysSetng in sysSetting)
            {
                SettingCategory setngType = intSysSetng.Setting.SettingCategory;
                Assert.AreEqual(SettingCategory.IntelligentAlarm, setngType);
            }
        }

        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void CheckIntelligentAlarmsandActiveAlarms()
        {
            WellDTO[] allwells = WellService.GetAllWells();
            WellDTO well = allwells.FirstOrDefault(w => w.Name == "new_Well");

            object objSettigvaue = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.FORESITE_FOR_EDGE);
            bool isEdge = (objSettigvaue.ToString() == "0") ? false : true;

            List<double> valueList = GetSystemSettingsForIntelligentAlarms(isEdge);

            IntelligentAlarmCheckDTO intelligentAlarmCheckDto = new IntelligentAlarmCheckDTO();
            intelligentAlarmCheckDto.IntelligentAlarmInfos = GetTestDataForIntelligentAlarm(isEdge);
            intelligentAlarmCheckDto.WellId = well.Id.ToString();
            intelligentAlarmCheckDto.CardType = "Current";

            int count = GetActiveIntelligentAlarmsFromTestData(valueList, isEdge);
            bool intCheck = IntelligentAlarmService.IntelligentAlarmsCheck(intelligentAlarmCheckDto);
            Assert.IsTrue(intCheck, $"{nameof(IIntelligentAlarmService.IntelligentAlarmsCheck)} returned an unexpected value.");
            CurrentAlarmDTO[] activeAlarms = IntelligentAlarmService.GetActiveIntelligentAlarmsByWellId(well.Id.ToString(), WellTypeId.RRL.ToString());
            Assert.AreEqual(count, activeAlarms.Count());
        }

        private List<double> GetSystemSettingsForIntelligentAlarms(bool isEdge)
        {
            List<double> valueList = new List<double>();
            List<SystemSettingDTO> sysSetting = IntelligentAlarmService.GetIntelligentAlarmSystemSettings().ToList();

            //Remove Edge specific system settings
            if (!isEdge)
            {
                sysSetting.RemoveAll(x => x.Setting.Key == "Desired Minimum Pump Fill");
                sysSetting.RemoveAll(x => x.Setting.Key == "Desired Pump Off Strokes");
            }
            foreach (SystemSettingDTO intSysSetng in sysSetting)
            {
                SettingCategory setngType = intSysSetng.Setting.SettingCategory;
                Assert.AreEqual(SettingCategory.IntelligentAlarm, setngType);
                if (intSysSetng.Setting.WellTypes.Length == 1 && intSysSetng.Setting.WellTypes[0].ToString() == "RRL")
                {
                    valueList.Add(Convert.ToDouble(intSysSetng.NumericValue));
                }
            }

            return valueList;
        }

        private List<IntelligentAlarmInfoDTO> GetTestDataForIntelligentAlarm(bool isEdge)
        {
            List<IntelligentAlarmInfoDTO> intAlarmInfos = new List<IntelligentAlarmInfoDTO>();
            intAlarmInfos.Add(GetIntelligentAlarmInfo(IntelligentAlarmTypes.HighGearboxTorque.ToString(), 92));
            intAlarmInfos.Add(GetIntelligentAlarmInfo(IntelligentAlarmTypes.HighRodStress.ToString(), 98));
            intAlarmInfos.Add(GetIntelligentAlarmInfo(IntelligentAlarmTypes.HighStructureLoad.ToString(), 75));
            intAlarmInfos.Add(GetIntelligentAlarmInfo(IntelligentAlarmTypes.LowPumpEfficiency.ToString(), 50));

            if (isEdge)
            {
                intAlarmInfos.Add(GetIntelligentAlarmInfo(IntelligentAlarmTypes.LowPumpFillage.ToString(), 85));
                intAlarmInfos.Add(GetIntelligentAlarmInfo(IntelligentAlarmTypes.LowPumpFillageDummy.ToString(), 2));
            }

            return intAlarmInfos;
        }

        private int GetActiveIntelligentAlarmsFromTestData(List<double> valueList, bool isEdge)
        {
            List<long> arr;
            List<int> lowAlmIndexes;
            if (isEdge)
            {
                arr = new List<long>() { 95, 0, 92, 98, 75, 50, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                lowAlmIndexes = new List<int>() { 0, 1, 5 };
            }
            else
            {
                arr = new List<long>() { 92, 98, 75, 50, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                lowAlmIndexes = new List<int>() { 3 };
            }

            int count = 0;
            if (arr.Count == valueList.Count)
            {
                for (int i = 0; i < valueList.Count; i++)
                {
                    if (!lowAlmIndexes.Contains(i)) //For Desired Minimum Pump Fill, Desired Pump Off Strokes and Pump Efficiency
                    {
                        if (arr[i] > valueList[i])
                            count = count + 1;
                    }
                    else
                    {
                        if (arr[i] < valueList[i])
                            count = count + 1;
                    }
                }
            }
            return count;
        }

        private IntelligentAlarmInfoDTO GetIntelligentAlarmInfo(string intelligentAlarmType, double current, double mean = 0.0, double stdDev = 0.0)
        {
            IntelligentAlarmInfoDTO intelligentAlarmInfo = new IntelligentAlarmInfoDTO();
            intelligentAlarmInfo.IntelligentAlarmType = intelligentAlarmType;
            intelligentAlarmInfo.CurrentValue = current;
            intelligentAlarmInfo.MeanValue = mean;
            intelligentAlarmInfo.StdDevValue = stdDev;
            return intelligentAlarmInfo;
        }

        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void GetIntelligentAlarmHistory()
        {
            WellDTO[] allwells = WellService.GetAllWells();
            WellDTO well = allwells.FirstOrDefault(w => w.Name == "new_Well");

            object objSettigvaue = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.FORESITE_FOR_EDGE);
            bool isEdge = (objSettigvaue.ToString() == "0") ? false : true;

            List<double> valueList = GetSystemSettingsForIntelligentAlarms(isEdge);

            IntelligentAlarmCheckDTO intelligentAlarmCheckDto = new IntelligentAlarmCheckDTO();
            intelligentAlarmCheckDto.IntelligentAlarmInfos = GetTestDataForIntelligentAlarm(isEdge);
            intelligentAlarmCheckDto.WellId = well.Id.ToString();
            intelligentAlarmCheckDto.CardType = "Current";

            int count = GetActiveIntelligentAlarmsFromTestData(valueList, isEdge);
            bool intCheck = IntelligentAlarmService.IntelligentAlarmsCheck(intelligentAlarmCheckDto);
            Assert.IsTrue(intCheck, $"{nameof(IIntelligentAlarmService.IntelligentAlarmsCheck)} returned an unexpected value.");
            CurrentAlarmDTO[] activeAlarms = IntelligentAlarmService.GetActiveIntelligentAlarmsByWellId(well.Id.ToString(), WellTypeId.RRL.ToString());
            Assert.AreEqual(count, activeAlarms.Count());

            DateTime endDate = DateTime.UtcNow;
            // Add an hour in future for end date because the below intelligent alarms once set in db will
            // have alarm time stamp after the current UTC time.
            endDate = endDate.AddHours(1);
            DateTime startDate = endDate.AddDays(-1);

            AlarmHistoryDTO[] alarmHistory =
                IntelligentAlarmService.GetIntelligentAlarmHistoryByWellIdAndDateRange(well.Id.ToString(), DTOExtensions.ToISO8601(startDate), DTOExtensions.ToISO8601(endDate));
            Assert.AreEqual(count, alarmHistory.Count());

            foreach (AlarmHistoryDTO alarm in alarmHistory)
            {
                Assert.IsNull(alarm.ClearedTime);
            }

            List<IntelligentAlarmInfoDTO> intAlarmInfos2 = new List<IntelligentAlarmInfoDTO>();
            intAlarmInfos2.Add(GetIntelligentAlarmInfo(IntelligentAlarmTypes.HighGearboxTorque.ToString(), 88));

            IntelligentAlarmCheckDTO intelligentAlarmCheckDto2 = new IntelligentAlarmCheckDTO();
            intelligentAlarmCheckDto2.WellId = well.Id.ToString();
            intelligentAlarmCheckDto2.IntelligentAlarmInfos = intAlarmInfos2;
            intelligentAlarmCheckDto.CardType = "Current";

            // Wait one second to make sure alarm times don't overlap.
            Thread.Sleep(1000);
            bool afterintCheck = IntelligentAlarmService.IntelligentAlarmsCheck(intelligentAlarmCheckDto2);
            AlarmHistoryDTO[] afteralarmHistory = IntelligentAlarmService.GetIntelligentAlarmHistoryByWellIdAndDateRange(well.Id.ToString(), DTOExtensions.ToISO8601(startDate), DTOExtensions.ToISO8601(endDate));
            Assert.AreEqual(count, afteralarmHistory.Count());
            foreach (AlarmHistoryDTO alarm in afteralarmHistory)
            {
                if (alarm.AlarmStatus == "High Gearbox Torque")
                    Assert.IsNotNull(alarm.ClearedTime);
                else
                    Assert.IsNull(alarm.ClearedTime);
            }

            List<IntelligentAlarmInfoDTO> intAlarmInfos3 = new List<IntelligentAlarmInfoDTO>();
            intAlarmInfos3.Add(GetIntelligentAlarmInfo(IntelligentAlarmTypes.HighGearboxTorque.ToString(), 95));

            IntelligentAlarmCheckDTO intelligentAlarmCheckDto3 = new IntelligentAlarmCheckDTO();
            intelligentAlarmCheckDto3.WellId = well.Id.ToString();
            intelligentAlarmCheckDto3.IntelligentAlarmInfos = intAlarmInfos3;
            intelligentAlarmCheckDto.CardType = "Current";

            // Wait one second to make sure alarm times don't overlap.
            Thread.Sleep(1000);
            bool newintCheck = IntelligentAlarmService.IntelligentAlarmsCheck(intelligentAlarmCheckDto3);
            AlarmHistoryDTO[] newalarmHistory = IntelligentAlarmService.GetIntelligentAlarmHistoryByWellIdAndDateRange(well.Id.ToString(), DTOExtensions.ToISO8601(startDate), DTOExtensions.ToISO8601(endDate));
            Assert.AreEqual(count + 1, newalarmHistory.Count());
            foreach (AlarmHistoryDTO alarm in newalarmHistory)
            {
                if ((alarm.AlarmStatus != "High Gearbox Torque"))
                {
                    Assert.IsNull(alarm.ClearedTime);
                }
            }
        }

        private static CygNet.Data.Core.DomainSiteService GetDomainSiteService(string service)
        {
            var dss = new CygNet.Data.Core.DomainSiteService(Convert.ToUInt16(s_domain), s_site, service);
            System.Diagnostics.Trace.WriteLine($"DSS for {service} is [{dss.DomainId}]{dss.SiteService}");
            return dss;
        }

        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void SeedHistoricalValuesInVHS()
        {
            // We are going to seed values in just one RPOC device for the testing
            string facilityId = s_isRunningInATS == true ? "RPOC_00001" : "RPOC_0001";

            // For intelligent alarms, UDCs for CardArea, RunTimeYesterday, DailyMinimumLoad, DailyMaximumLoad, InferredProductionYesterday
            // and CyclesYesterday are used for which we need to seed values in the VHS
            string[] UDCs = { "RRLCRDAREA", "TMRUNYD", "RRLMNLDDLY", "RRLMXLDDLY", "RRLIFPYEST", "QTLIQYD" };

            List<string> facUDCs = new List<string>();
            foreach (var udc in UDCs)
            {
                facUDCs.Add(facilityId + "." + udc);
            }

            object input = facUDCs.Select(t => t as object).ToArray();
            object pointIdsList = null;

            try
            {
                System.Diagnostics.Trace.WriteLine($"SeedHistoricalValuesInVHS: Going to create CVS Client for [{s_domain}]{s_site}.{s_cvsService}.");
                var cvsClient = CreateCvsClient();
                cvsClient.ResolvePoints(ref input, out pointIdsList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"SeedHistoricalValuesInVHS: CvsClient.ResolvePoints failed: {ex.Message}.");
            }

            List<string> tagIds = new List<string>();
            if (pointIdsList.GetType().IsArray)
            {
                tagIds = ((object[])pointIdsList).Select(obj => Convert.ToString(obj)).ToList();
            }

            List<string> names = new List<string>();
            string site = ConfigurationManager.AppSettings.Get("Site");
            foreach (var pointId in tagIds)
            {
                names.Add(site + "." + "UIS" + "." + pointId);
            }

            foreach (var item in names)
            {
                var name = new Name { ID = item };
                var histEntries = GetHistoricalEntries(25);
                _entriesByName[name] = histEntries.ToList();
            }

            foreach (var name in _entriesByName.Keys)
            {
                System.Diagnostics.Trace.WriteLine($"SeedHistoricalValuesInVHS: Seeding values in VHS for {name.ID} point.");
                try
                {
                    _vhsClient.StoreHistoricalEntries(name, _entriesByName[name]);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"SeedHistoricalValuesInVHS: Failed to seed VHS values for {name} point: {ex.Message}.");
                }
            }

            DeleteHistoricalValuesFromVHS();
        }

        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void IntelligentAlarmCheckForSingleWellNonRRLTest_MultiPointInjectionGasLift()
        {
            var testDataDTO = new WellTestDTO
            {
                AverageCasingPressure = 800.35m,
                AverageTubingPressure = 208.88m,
                AverageTubingTemperature = 100,
                Gas = 397m,
                GasGravity = 0.74m,
                producedGOR = 5729,
                GasInjectionRate = 230,
                Oil = 69.3m,
                OilGravity = 60m,
                SPTCode = 2,
                SPTCodeDescription = "RepresentativeTest",
                TestDuration = 3,
                Water = 0,
                WaterGravity = 1.04m
            };
            ConfigureGasLiftWellWithTestDataForIntelligentAlarmTest("MultiInjection_GL.wflx", testDataDTO);
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName + WellTypeId.GLift.ToString()));
            var myassembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(myassembly);
            //get the newly added (latest) well test data
            WellTestAndUnitsDTO latestValidTestDataPair = WellTestDataService.GetLatestValidWellTestByWellId(well.Id.ToString());
            Assert.IsNotNull(latestValidTestDataPair);

            //Validating PressureDropAnalysis results
            NodalAnalysisInputAndUnitsDTO glAnalysisInput = WellTestDataService.GetAnalysisInputDataAndUnits(latestValidTestDataPair.Value.Id.ToString());
            GasLiftPressureDropAnalysisResultsAndUnitsDTO glPressureDropAnalysis = WellTestDataService.PerformGasLiftPressureDrop(glAnalysisInput, "IntelligentAlarmRun");

            var alarmTypeList = AlarmService.GetAlarmTypes();
            var typeId = alarmTypeList.First(en => en.AlarmType == "Multi Points Injection").Id;
            CurrentAlarmDTO intelligentAlarm = AlarmService.GetActiveAlarmByWellIdAndAlarmType(well.Id.ToString(), typeId.ToString());
            Assert.IsNotNull(intelligentAlarm, "Failed to trigger the Multi Points Injection alarm.");
            CurrentAlarmDTO[] intelligentAlarmGeneral = IntelligentAlarmService.GetActiveIntelligentAlarmsByWellId(well.Id.ToString(), well.WellType.ToString());
            Assert.IsNotNull(intelligentAlarmGeneral, "Failed to get active intelligent alarms.");
            Assert.AreEqual(1, intelligentAlarmGeneral.Count());
            Assert.AreEqual(intelligentAlarm.AlarmTypeId, intelligentAlarmGeneral.FirstOrDefault().AlarmTypeId, "Generated alarm type mismatch.");
        }

        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void CheckIntelligentAlarmforSingleWellGL()
        {
            var testDataDTO = new WellTestDTO
            {
                AverageCasingPressure = 800.35m,
                AverageTubingPressure = 208.88m,
                AverageTubingTemperature = 100,
                Gas = 397m,
                GasGravity = 0.74m,
                producedGOR = 5729,
                GasInjectionRate = 230,
                Oil = 69.3m,
                OilGravity = 60m,
                SPTCode = 2,
                SPTCodeDescription = "RepresentativeTest",
                TestDuration = 3,
                Water = 0,
                WaterGravity = 1.04m
            };
            ConfigureGasLiftWellWithTestDataForIntelligentAlarmTest("MultiInjection_GL.wflx", testDataDTO);
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName + WellTypeId.GLift.ToString()));
            bool alarmCheck = IntelligentAlarmService.IntelligentAlarmCheckForSingleWellGL(well.Id.ToString());
            Assert.IsTrue(alarmCheck, "Failed to generate alarm.");
            CurrentAlarmDTO[] intelligentAlarmGeneral = IntelligentAlarmService.GetActiveIntelligentAlarmsByWellId(well.Id.ToString(), well.WellType.ToString());
            Assert.IsNotNull(intelligentAlarmGeneral, "Failed to get Active intelligent alarms.");
            Assert.AreEqual(1, intelligentAlarmGeneral.Count());
        }

        //Test case written to check the PCP intellegenet alarms.
        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void CheckIntelligentAlarmforSingleWellPCP()
        {
            string facilityId = GetFacilityId("WFTA1K_", 1);
            try
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");

                //Adding PCP Well along with Well Test Data.....
                #region AddWell
                //Adding Well along with WellTest Data with PI Tuning Method
                WellDTO pcpWell = AddNonRRLWell(facilityId, WellTypeId.PCP, false, CalibrationMethodId.PI);
                _wellsToRemove.Add(pcpWell);
                if (s_isRunningInATS && s_domain.Equals("9077") == false)
                {
                    // Since No DG Trasnscatiosn are found on Fresh CygNet install for WFTA1K, this step is must on ATS CygNet
                    Trace.WriteLine($"Running In ATS: Scan Command Sent  By Force since  Running ATS = {s_isRunningInATS}  and CygNet Domain {s_domain}");
                    SurveillanceService.IssueCommandForSingleWell(pcpWell.Id, WellCommand.DemandScan.ToString());
                }
                #endregion AddWell

                #region WellTest Tuning

                AddWellSettingWithDoubleValues(pcpWell.Id, "Min Productivity Index Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max Productivity Index Acceptance Limit", 1.0);

                AddWellSettingWithDoubleValues(pcpWell.Id, "Min Water Cut Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max Water Cut Acceptance Limit", 1.0);

                AddWellSettingWithDoubleValues(pcpWell.Id, "Min GOR Acceptance Limit", 1.0);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max GOR Acceptance Limit", 50.0);

                WellTestDTO latestTestData_PCP = WellTestDataService.GetLatestWellTestDataByWellId(pcpWell.Id.ToString());
                WellTestDataService.TuneSelectedWellTests(new long[] { latestTestData_PCP.Id });

                WellTestDTO latestTestDataAfterTune_PCP = WellTestDataService.GetLatestWellTestDataByWellId(pcpWell.Id.ToString());
                Assert.AreEqual("TUNING_SUCCEEDED", latestTestDataAfterTune_PCP.Status.ToString(), "Well Test Status is not Success");

                #endregion WellTest Tuning

                //Enter WellDailyAverge from DB
                #region WellDailyAverage

                WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO
                {
                    EndDateTime = DateTime.Today.ToUniversalTime(),
                    StartDateTime = DateTime.Today.AddDays(-1).ToUniversalTime(),
                    WellId = pcpWell.Id,
                    Id = 0,
                    RunTime = 24,
                    Status = WellDailyAverageDataStatus.Original,
                    THP = 860,
                    THT = 99.1,
                    CHP = 146.77,
                    PIP = 2100,
                    PDP = 320,
                    CasingGasRate = 1.40,
                    FLP = 8.35,
                    OilRateAllocated = 268.69,
                    WaterRateAllocated = 177.8,
                    MotorAmps = 10.37,
                    MotorVolts = 38.13,
                    MotorTemperature = 196.5,
                    PumpSpeed = 129.2,
                    PumpTorque = 88.84,
                    FBHP = 141.19,
                    OilRateInferred = 279.73,
                    WaterRateInferred = 187.90
                };
                bool addDailyAvgDataForPCP = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
                Assert.IsTrue(addDailyAvgDataForPCP, "Failed to add Daily Average data for day 1");

                dailyAverageDTO.EndDateTime = DateTime.Today.AddDays(-1).ToUniversalTime();
                dailyAverageDTO.StartDateTime = DateTime.Today.AddDays(-2).ToUniversalTime();

                dailyAverageDTO.THP = 862;
                dailyAverageDTO.PIP = 2105;
                dailyAverageDTO.PDP = 322;

                addDailyAvgDataForPCP = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
                Assert.IsTrue(addDailyAvgDataForPCP, "Failed to add Daily Average data for day 2");

                dailyAverageDTO.EndDateTime = DateTime.Today.AddDays(-2).ToUniversalTime();
                dailyAverageDTO.StartDateTime = DateTime.Today.AddDays(-3).ToUniversalTime();

                dailyAverageDTO.THP = 864;
                dailyAverageDTO.PIP = 2109;
                dailyAverageDTO.PDP = 325;

                addDailyAvgDataForPCP = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
                Assert.IsTrue(addDailyAvgDataForPCP, "Failed to add Daily Average data for day 3");

                #endregion WellDailyAverage

                bool result = IntelligentAlarmService.IntelligentAlarmCheckForSingleWellNonRRL(pcpWell.Id.ToString());
                CurrentAlarmDTO[] activeAlarms = IntelligentAlarmService.GetActiveIntelligentAlarmsByWellId(pcpWell.Id.ToString(), WellTypeId.PCP.ToString());
                Assert.AreEqual(2, activeAlarms.Length, "Expected intelligent setting count does not match.");
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                RemoveWell(facilityId);
            }
        }

        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void IntelligentAlarmCheckForSingleWellNonRRLTest_ShallowInjectionGasLift()
        {
            var testDataDTO = new WellTestDTO
            {
                AverageCasingPressure = 2202.3m,
                AverageTubingPressure = 565.30m,
                AverageTubingTemperature = 212.1m,
                Gas = 3280m,
                GasGravity = 0.8723m,
                producedGOR = 5729,
                GasInjectionRate = 2940,
                Oil = 526.0m,
                OilGravity = 36.43m,
                SPTCode = 2,
                SPTCodeDescription = "RepresentativeTest",
                TestDuration = 3,
                Water = 2955,
                WaterGravity = 1.0314m
            };
            ConfigureGasLiftWellWithTestDataForIntelligentAlarmTest("ShallowInjection_GL.wflx", testDataDTO);
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName + WellTypeId.GLift.ToString()));
            var myassembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(myassembly);
            //get the newly added (latest) well test data
            WellTestAndUnitsDTO latestValidTestDataPair = WellTestDataService.GetLatestValidWellTestByWellId(well.Id.ToString());
            Assert.IsNotNull(latestValidTestDataPair);

            //Validating PressureDropAnalysis results
            NodalAnalysisInputAndUnitsDTO glAnalysisInput = WellTestDataService.GetAnalysisInputDataAndUnits(latestValidTestDataPair.Value.Id.ToString());
            GasLiftPressureDropAnalysisResultsAndUnitsDTO glPressureDropAnalysis = WellTestDataService.PerformGasLiftPressureDrop(glAnalysisInput, "IntelligentAlarmRun");

            var alarmTypeList = AlarmService.GetAlarmTypes();
            var typeId = alarmTypeList.First(en => en.AlarmType == "Shallow Injection Depth").Id;
            var intelligentAlarm = AlarmService.GetActiveAlarmByWellIdAndAlarmType(well.Id.ToString(), typeId.ToString());
            Assert.IsNotNull(intelligentAlarm);
        }

        private static IEnumerable<HistoricalEntry> GetHistoricalEntries(double baseValue)
        {
            // Create entries spreaded in a span of 30 days
            List<HistoricalEntry> entries = new List<HistoricalEntry>();
            DateTime now = DateTime.UtcNow;
            for (int i = 0; i < 30; i++)
            {
                HistoricalEntry entry = new HistoricalEntry();
                entry.ValueType = HistoricalEntryValueType.UTF8;
                entry.SetValue((baseValue + i * 0.5).ToString());
                entry.Timestamp = now.AddDays(-i);
                entry.BaseStatus = CygNet.Data.Core.BaseStatusFlags.Initialized | CygNet.Data.Core.BaseStatusFlags.Updated;
                entries.Add(entry);
            }

            return entries;
        }

        private static void DeleteHistoricalValuesFromVHS()
        {
            foreach (var name in _entriesByName.Keys)
            {
                try
                {
                    System.Diagnostics.Trace.WriteLine($"Deleting seeded values for {name.ID} from {_vhsClient.SiteService}.");
                    _vhsClient.DeleteHistoricalEntries(name, _entriesByName[name], true, new System.Threading.CancellationToken());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"Failed to delete seeded values for {name.ID} from {_vhsClient.SiteService}: {ex.Message}.");
                }
            }
        }

        private void ConfigureGasLiftWellWithTestDataForIntelligentAlarmTest(string modelName, WellTestDTO testDataDTO)
        {
            //pick up the right model file
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            Tuple<string, WellTypeId, ModelFileOptionDTO> model = Tuple.Create(modelName, WellTypeId.GLift, new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.ReservoirPressure, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), (long)OptionalUpdates.UpdateWCT_WGR, ((long)OptionalUpdates.CalculateChokeD_Factor) } });

            string modelFileName = model.Item1;
            WellTypeId wellType = model.Item2;
            ModelFileOptionDTO options = model.Item3;

            //Create a new well
            string wellName = DefaultWellName + wellType.ToString();
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = wellName, AssemblyAPI = wellName, SubAssemblyAPI = wellName, IntervalAPI = wellName, CommissionDate = DateTime.Today.AddYears(-3), WellType = wellType }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName + wellType.ToString()));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            var myassembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(myassembly);

            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };

            options.Comment = "CASETest Upload " + model.Item1;
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(30);
            modelFile.WellId = well.Id;

            byte[] fileAsByteArray = GetByteArray(Path, model.Item1);
            Assert.IsNotNull(fileAsByteArray);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            ModelFileValidationDataDTO ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
            Assert.IsNotNull(ModelFileValidationData);
            ModelFileService.AddWellModelFile(modelFile);
            ModelFileDTO newModelFile = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(newModelFile);
            testDataDTO.WellId = well.Id;
            testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10));
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDataService.AddWellTestData(new WellTestAndUnitsDTO(units, testDataDTO));
        }

        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void PatternMatchingWithAlarm()
        {
            // Add fully configured well
            WellConfigDTO wellconfig = AddNewRRLWellConfigFullModel("RPOC_");
            WellDTO well = wellconfig.Well;
            _wellsToRemove.Add(well);
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;

            //add new wellTestData
            var testDataDTO = new WellTestDTO
            {
                WellId = well.Id,
                AverageCasingPressure = 1,
                AverageFluidAbovePump = 1,
                AverageTubingPressure = 90,
                AverageTubingTemperature = 100,
                Gas = 0,
                GasGravity = 0,
                Oil = 83,
                OilGravity = 25,
                PumpEfficiency = 0,
                PumpIntakePressure = 100,
                PumpingHours = 10,
                //SPTCode = 1,
                SPTCodeDescription = WellTestQuality.RepresentativeTest.ToString(),
                StrokePerMinute = 0,
                TestDuration = 3,
                Water = 75,
                WaterGravity = 1.010M,
            };
            testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10));
            WellTestDataService.AddWellTestData(new WellTestAndUnitsDTO(units, testDataDTO));

            //Get the added well test data
            var TestDataArray = WellTestDataService.GetWellTestDataByWellId(well.Id.ToString());
            WellTestDTO testDataDTOCheck = TestDataArray.Values.FirstOrDefault(a => a.WellId == well.Id);
            Assert.IsNotNull(testDataDTOCheck);


            //Get System settings
            SystemSettingDTO[] sysSetting = IntelligentAlarmService.GetIntelligentAlarmSystemSettings();
            foreach (SystemSettingDTO intSysSetng in sysSetting)
            {
                SettingCategory setngType = intSysSetng.Setting.SettingCategory;
                Assert.AreEqual(SettingCategory.IntelligentAlarm, setngType);

            }

            //Get Well Settings
            WellSettingDTO[] beforewellsettingsbyPK = IntelligentAlarmService.GetIntelligentAlarmSettings(well.Id.ToString()).OrderBy(s => s.SettingId).ToArray();
            foreach (WellSettingDTO welStng in beforewellsettingsbyPK)
            {
                Assert.AreEqual(0, welStng.Id);
            }
            Assert.AreEqual(15, beforewellsettingsbyPK.Length, "Expected intelligent setting count does not match.");

            WellSettingDTO[] UpdateValue_PatternCard = beforewellsettingsbyPK.Where(s => s.Setting.Description == "Minimum Card Match").ToArray();

            WellSettingDTO expected = UpdateValue_PatternCard.FirstOrDefault(s => s.Setting.Description == "Minimum Card Match");
            expected.NumericValue = 10;
            expected.InternalNumericValue = 10;

            IntelligentAlarmService.AddIntelligentAlarmWellSetting(expected);
            WellSettingDTO actual = IntelligentAlarmService.GetIntelligentAlarmSettings(well.Id.ToString()).FirstOrDefault(s => s.Setting.Name == expected.Setting.Name);
            Assert.IsNotNull(actual, "Failed to get setting back after add.");
            Assert.AreEqual(expected.NumericValue, actual.NumericValue, "Number value does not match after add.");
            Assert.AreNotEqual(0, actual.Id, "Id should be non-zero after add.");

            actual.NumericValue = 10;
            actual.InternalNumericValue = 10;
            IntelligentAlarmService.UpdateIntelligentAlarmWellSetting(expected);
            actual = IntelligentAlarmService.GetIntelligentAlarmSettings(well.Id.ToString()).FirstOrDefault(s => s.Setting.Name == expected.Setting.Name);
            Assert.IsNotNull(actual, "Failed to get setting back after update.");
            Assert.AreEqual(expected.NumericValue, actual.NumericValue, "Numeric value does not match after update.");
            Assert.AreNotEqual(0, actual.Id, "Id should be non-zero after update.");

            // Scan Card
            SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
            Wait(5);

            int cardtype = (int)CardType.Full;
            DynaCardEntryDTO FullDynacard = DynacardService.ScanDynacard(well.Id.ToString(), cardtype.ToString());
            Trace.WriteLine(FullDynacard.ErrorMessage);
            Assert.IsNotNull(FullDynacard.SurfaceCards, "Surface Card is null");
            Assert.AreEqual("Success", FullDynacard.ErrorMessage);
            if (well.FacilityId.Contains("RPOC_") || well.FacilityId.Contains("SAM_"))
                Assert.IsTrue(FullDynacard.DownholeCards.Length > 0, "Downhole Card DTO is null");
            int downholeCardSrc = 0;
            if (well.FacilityId.Contains("RPOC_") || well.FacilityId.Contains("SAM_"))
                downholeCardSrc = (int)FullDynacard.DownholeCards[0].CardSource;
            else
                downholeCardSrc = (int)DownholeCardSource.UnknownSource;
            RRLPatternMatchInputs inputs = new RRLPatternMatchInputs()
            {
                WellId = well.Id.ToString(),
                TimestampInTicks = FullDynacard.TimestampInTicks,
                CardType = cardtype.ToString(),
                DownholeCardSource = downholeCardSrc.ToString(),
                IsActive = true,
                MatchDownhole = true,
                MatchSurface = true,
                Comment = "Comment",
                GenerateAlarm = true,

            };
            // Add pattern match card
            RRLLibraryCardDTO addPatternCard = DynacardService.AddPatternMatchLibraryCard(inputs);

            addPatternCard.CardComment = "updated_comment";
            RRLLibraryCardDTO afterupdatePatternMatchCard = DynacardService.UpdatePatternMatchCard(addPatternCard);
            RRLLibraryCardDTO afterpatternMatchCard = DynacardService.GetPatternMatchLibraryCard(addPatternCard.CardId.ToString());
            Assert.AreEqual("updated_comment", afterpatternMatchCard.CardComment);

            // Scan and Analyse card
            DynacardEntryAndUnitsDTO[] scanAnalyseCard = DynacardService.ScanAndAnalyzeDynacardWithUnits(well.Id.ToString(), ((int)CardType.Full).ToString(), "True");
            // Get intelligentAlarms
            IntelligentAlarmDTO LatestAlarm = DynacardService.GetLatestIntelligentAlarmQuantitiesByWellId(well.Id.ToString());
            Assert.IsNotNull(LatestAlarm.IntelligentAlarmStatus, "Intelligent Alarm status is null");

            // Get alarm status from Group status screen
            WellGroupStatusDTO<object, object> groupStatus = SurveillanceService.GetWellGroupStatusByWellIds(new[] { well.Id }, "false");

            object[] objArray = groupStatus.Status;

            foreach (object obj in objArray)
            {
                RRLWellStatusValueDTO wellStatus = obj as RRLWellStatusValueDTO;

                Trace.WriteLine(wellStatus.AlarmStatus);
                Assert.IsNotNull(wellStatus.AlarmStatus, "Failed to get Alarmstatus from group status screen");
            };

            // Delete pattern match card
            RRLLibraryCardDTO deletePatternmatchCard = DynacardService.DeletePatternMatchCard(afterpatternMatchCard.CardId.ToString());
            Assert.IsTrue(deletePatternmatchCard.ErrorMessage.Contains(afterpatternMatchCard.CardId.ToString()));
            RRLLibraryCardDTO inputs1 = new RRLLibraryCardDTO()
            {

                IsActive = true,
                GenerateAlarm = true,
                PatternMatchFlags = "Downhole",
                MatchDownholeCard = true,
                ErrorMessage = "Success",
                CardType = cardtype.ToString(),
            };

        }

        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void VerifyEdgeRRLIntelligentAlarms()
        {
            //Create RRL Well with full config and Well test...
            if (CheckIfCanExecuteForeSiteSchedulerJob() == false)
            {
                Trace.WriteLine("Test could not run as could not find path of ForeSite Schedular.exe");
                return;
            }
            object objSettigvaue = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.FORESITE_FOR_EDGE);
            isEdge = (objSettigvaue.ToString() == "0") ? false : true;
            Trace.WriteLine($"Test Run Mode for Edge was {isEdge}");
            WellConfigDTO wellconfig = AddNewRRLWellConfigFullModel("RPOC_");
            WellDTO well = wellconfig.Well;
            _wellsToRemove.Add(well);
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;

            //Add new wellTestData
            var testDataDTO = new WellTestDTO
            {
                WellId = well.Id,
                AverageCasingPressure = 1,
                AverageFluidAbovePump = 1,
                AverageTubingPressure = 90,
                AverageTubingTemperature = 100,
                Gas = 0,
                GasGravity = 0,
                Oil = 83,
                OilGravity = 25,
                PumpEfficiency = 0,
                PumpIntakePressure = 100,
                PumpingHours = 10,
                //SPTCode = 1,
                SPTCodeDescription = WellTestQuality.RepresentativeTest.ToString(),
                StrokePerMinute = 0,
                TestDuration = 3,
                Water = 75,
                WaterGravity = 1.010M,
            };
            testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10));
            WellTestDataService.AddWellTestData(new WellTestAndUnitsDTO(units, testDataDTO));

            //Get the added well test data
            var TestDataArray = WellTestDataService.GetWellTestDataByWellId(well.Id.ToString());
            WellTestDTO testDataDTOCheck = TestDataArray.Values.FirstOrDefault(a => a.WellId == well.Id);
            Assert.IsNotNull(testDataDTOCheck);
            //Update Welll to use 'None' As Crank Weights for getting GearBox Unbalance
            UpdateWellConfigforWell(well);

            //update ForSite Alarms Configution to match the expected alarm condition for given data ;
            //We should get all non-Trend based alrams 
            #region VerifyAllAlarmsAreSet
            UpdateForeSiteAlarmValuesToSetAlarms(well, isEdge);
            // Send UIS Command to CygNet to get PumpUp Or Full Card.
            var uisClient = GetUisClient();
            uisClient.SendUISCommand(GetFacilityId("RPOC_", 1), "CARDPUP", null, null);
            //Perform -RunAnalysis Task from command line 
            RunAnalysisTaskScheduler("-runAnalysis");

            List<string> icollection = new List<string>();
            string ptnalm = "Full Fillage, Slight Upstroke Wear (97)";
            icollection.Add("High Rod Stress");
            icollection.Add("High Gearbox Torque");
            icollection.Add("High Beam Load");
            icollection.Add("Low Pump Efficiency");
            icollection.Add("Gearbox Unbalance");
            icollection.Add("Rod String Unbalance");
            icollection.Add(ptnalm);
            string start_date = (DateTime.Today - TimeSpan.FromDays(30)).ToUniversalTime().ToISO8601();
            string end_date = DateTime.Today.ToUniversalTime().ToISO8601();
            string end_dateoneday = DateTime.Today.AddDays(1).ToUniversalTime().ToISO8601();
            IntelligentAlarmDTO ialarmdto = DynacardService.GetLatestIntelligentAlarmQuantitiesByWellId(well.Id.ToString());

            //Verify Alarms on Well Status Page
            VerifyAlarmsOnWellStatusPage(ialarmdto, icollection, true);
            //Verify Alarms on All Alarms History Status Page
            VerifyAlarmsOnAllAlarmsHistoryPage(well, icollection, start_date, end_date, true);
            //Verify Alarms on ForeSite Alarms History Status Page
            VerifyAlarmsOnForesiteAlarmsHistoryPage(well, icollection, start_date, end_date, true);

            HashSet<string> valuetypes = AlarmService.GetValueAlarmTypeNames();
            string ptcard = valuetypes.FirstOrDefault(x => x.ToString() == "Pattern Card Match").ToString();
            Assert.AreEqual("Pattern Card Match", ptcard, "Pattern card Alram Type was not there in Value Alarm Types");
            WellDTO[] allwells = WellService.GetAllWells();
            List<long> lnwellids = new List<long>();
            foreach (WellDTO indwell in allwells)
            {
                lnwellids.Add(indwell.Id);
            }
            long[] alarmtest = AlarmService.GetWellIdsWithActiveAlarmsByDate(lnwellids.ToArray(), end_dateoneday);
            Assert.IsTrue(alarmtest.Length > 0, "Active Alarms by WellIDs not retrived ");
            AlarmKPIWellIds alarmkpi = AlarmService.GetWellIdsWithActiveAlarmsTodayAndYesterday(alarmtest);
            Assert.IsNotNull(alarmkpi.TodayAlarmIds, "Today Alarm IDs were not NULL");

            #endregion

            #region VerifyAllAlarmsAreCleared
            UpdateForeSiteAlarmValuesToClearAlarms(well);
            // Send UIS Command to CygNet to get PumpUp Or Full Card.
            uisClient.SendUISCommand(GetFacilityId("RPOC_", 1), "CARDPUP", null, null);
            //Perform -RunAnalysis Task from command line 
            RunAnalysisTaskScheduler("-runAnalysis");
            ialarmdto = DynacardService.GetLatestIntelligentAlarmQuantitiesByWellId(well.Id.ToString());
            VerifyAlarmsOnWellStatusPage(ialarmdto, icollection, false);
            VerifyAlarmsOnAllAlarmsHistoryPage(well, icollection, start_date, end_date, false);
            VerifyAlarmsOnForesiteAlarmsHistoryPage(well, icollection, start_date, end_date, false);
            #endregion

            #region VerifyLowPumpFillageAlarm
            if (isEdge)
            {
                DynacardService.ScanAndAnalyzeDynacardWithUnits(well.Id.ToString(), "1", "true");
                DynacardService.ScanAndAnalyzeDynacardWithUnits(well.Id.ToString(), "1", "true");
                //uisClient.SendUISCommand(GetFacilityId("RPOC_", 1), "CARDCRNT", null, null);
                //RunAnalysisTaskSchedular("-runAnalysis");
                IntelligentAlarmDTO iAlarms = DynacardService.GetLatestIntelligentAlarmQuantitiesByWellId(well.Id.ToString());
                Assert.IsNotNull(ialarmdto);
                List<string> ialmlist = iAlarms.IntelligentAlarmStatus.ToList();
                CollectionAssert.Contains(ialmlist, "Low Pump Fillage", $" Expected Intelligent Alarm Low Pump Fillage is Missing");
            }
            #endregion


            if (failcount > 0)
            {
                if (failcount == 2)
                {
                    Trace.WriteLine($"Intelligent alarm check is complete for RRL well for Non Trend based alarms...Fail count: {failcount} ");
                    Assert.Fail($"Tested was failed due to FRI-2330 FailCount = {failcount}");
                }
                else if ((failcount > 2))
                {
                    Assert.Fail($"Tested was failed due to Other than FRI-2330 FailCount = {failcount}");
                }
            }
            else
            {
                Trace.WriteLine($"Intelligent alarm check is complete for RRL well for Non Trend based alarms...Fail count: {failcount} ");
            }

        }

        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void VerifyEdgeRRLSendingCalculatedPIPtoController()
        {
            Cleanup();
            _wellsToRemove.Clear();
            //Create RRL Well with full config and Well test...
            if (CheckIfCanExecuteForeSiteSchedulerJob() == false)
            {
                Trace.WriteLine("Test could not run as could not find path of ForeSite Schedular.exe");
                return;
            }

            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string setpointdata = ReadFromXMlFile(Path + "RRLDefaultSetPoints.xml");
            SetValuesInSystemSettings(SettingServiceStringConstants.SET_POINT_CONFIG, setpointdata);
            SetValuesInSystemSettings(SettingServiceStringConstants.SEND_CPIP_TO_CONTROLLER, "1");
            WellConfigDTO wellconfig = AddNewRRLWellConfigFullModel("RPOC_");
            WellDTO well = wellconfig.Well;
            _wellsToRemove.Add(well);
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;

            //Add new wellTestData
            var testDataDTO = new WellTestDTO
            {
                WellId = well.Id,
                AverageCasingPressure = 1,
                AverageFluidAbovePump = 1,
                AverageTubingPressure = 90,
                AverageTubingTemperature = 100,
                Gas = 0,
                GasGravity = 0,
                Oil = 83,
                OilGravity = 25,
                PumpEfficiency = 0,
                PumpIntakePressure = 100,
                PumpingHours = 10,
                //SPTCode = 1,
                SPTCodeDescription = WellTestQuality.RepresentativeTest.ToString(),
                StrokePerMinute = 0,
                TestDuration = 3,
                Water = 75,
                WaterGravity = 1.010M,
            };
            testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10));
            WellTestDataService.AddWellTestData(new WellTestAndUnitsDTO(units, testDataDTO));

            //Get the added well test data
            var TestDataArray = WellTestDataService.GetWellTestDataByWellId(well.Id.ToString());
            WellTestDTO testDataDTOCheck = TestDataArray.Values.FirstOrDefault(a => a.WellId == well.Id);
            Assert.IsNotNull(testDataDTOCheck);
            //Update Welll to use 'None' As Crank Weights for getting GearBox Unbalance
            //UpdateWellConfigforWell(well);
            //Perform -RunAnalysis Task from command line 
            CopyAlarmsCSVDefault();
            RunAnalysisTaskScheduler("-runIntelligentAlarmSendRRL");
            SetPointsDTO rrlIADTO = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), "RRLIAlms");

            // We are going to scan only the current card.
            string cardType = ((int)CardType.Current).ToString();
            var dhCardSource = ((int)DownholeCardSource.CalculatedEverittJennings).ToString();
            var dynaCardEntry = DynacardService.GetCardEntriesByDateRange(well.Id.ToString(), cardType, DTOExtensions.ToISO8601(DateTime.Today.AddDays(-2).ToUniversalTime()), DTOExtensions.ToISO8601(DateTime.Today.AddDays(1).ToUniversalTime()))?.Values.LastOrDefault();
            //Assert.IsNotNull(dynaCardEntry);
            var cardTicks = dynaCardEntry.TimestampInTicks;
            var analysisReport = DynacardService.GetDownholeAnalysisReport(well.Id.ToString(), cardType, cardTicks, dhCardSource);
            double.TryParse(rrlIADTO.SetPointsList.First(en => en.Description == "Cur. Pump Intake Pres Calculated").Value, out double setPointPIPValue);
            var epsilon = Math.Abs(setPointPIPValue - analysisReport.PumpingUnitDataDTO.Value.CalculatedPumpIntakePressure.Value);
            Assert.IsTrue(epsilon < 1.0);
        }

        public void UpdateWellConfigforWell(WellDTO well)
        {
            //Neded to pass 
            WellConfigDTO wellconfigdto = WellConfigurationService.GetWellConfig(well.Id.ToString());
            CrankWeightsConfigDTO SampleWeightsConfig = wellconfigdto.ModelConfig.Weights;
            WellConfigUnitDTO wellcfgunitdto = WellConfigurationService.GetWellConfigUnits();
            POPRRLCranksWeightsDTO crankwiegdto = CatalogService.GetCrankWeightsByCrankId("94110C");
            SampleWeightsConfig.TorqCalcMethod = TorqueCalculationMethod.API;
            SampleWeightsConfig.Crank_1_Auxiliary = new RRLAuxiliaryWeightDTO { LagId = "None", LeadId = "None" };
            SampleWeightsConfig.Crank_1_Primary = new RRLPrimaryWeightDTO { LagId = "None", LeadId = "None", LagDistance = 0, LeadDistance = 0 };
            SampleWeightsConfig.Crank_2_Auxiliary = new RRLAuxiliaryWeightDTO { LagId = "None", LeadId = "None" };
            SampleWeightsConfig.Crank_2_Primary = new RRLPrimaryWeightDTO { LagId = "None", LeadId = "None", LagDistance = 0, LeadDistance = 0 };
            wellconfigdto.ModelConfig.Weights = SampleWeightsConfig;
            UnitsValuesPairDTO<CrankWeightsConfigUnitDTO, CrankWeightsConfigDTO> weightsData = new UnitsValuesPairDTO<CrankWeightsConfigUnitDTO, CrankWeightsConfigDTO>();
            weightsData.Units = wellcfgunitdto.ModelConfig.Weights;
            weightsData.Value = wellconfigdto.ModelConfig.Weights;
            double newcbt = ModelFileService.CalculateCBTUoM(well.Id.ToString(), weightsData);
            SampleWeightsConfig.CBT = newcbt;
            wellconfigdto.ModelConfig.Weights = SampleWeightsConfig;
            WellConfigurationService.UpdateWellConfig(wellconfigdto);
            Assert.AreEqual(470.81, newcbt, "Mismtach in Torque Calculation");


        }

        public void UpdateForeSiteAlarmValuesToSetAlarms(WellDTO well, bool isEdge)
        {
            //update Alrams settings to ensure we get alrams for given st of data

            List<WellSettingDTO> alarmsettings = new List<WellSettingDTO>
            {
                new WellSettingDTO
                {
                Setting = new SettingDTO { Name = "High Gearbox Torque" , Key="High Gearbox Torque" ,  SettingType = (SettingType)5, SettingValueType = SettingValueType.Number} ,
                NumericValue =68,
                SettingId = SettingService.GetSettingByName("High Gearbox Torque").Id,
                WellId = well.Id
                },

                new WellSettingDTO
                {
                Setting = new SettingDTO { Name = "High Beam Load", Key="High Beam Load", SettingType = (SettingType)5  , SettingValueType = SettingValueType.Number} ,
                NumericValue =60,
                SettingId =SettingService.GetSettingByName("High Beam Load").Id,
                WellId = well.Id
                },

                 new WellSettingDTO
                {
                Setting = new SettingDTO { Name = "High Rod Stress" ,Key="High Rod Stress" , SettingType = (SettingType)5, SettingValueType = SettingValueType.Number} ,
                NumericValue =50,
                SettingId =SettingService.GetSettingByName("High Rod Stress").Id,
                WellId = well.Id
                },

                new WellSettingDTO
                {
                Setting = new SettingDTO { Name = "Rod String Unbalance", Key="Rod String Unbalance", SettingType = (SettingType)5, SettingValueType = SettingValueType.Number } ,
                NumericValue =3,
                SettingId =SettingService.GetSettingByName("Rod String Unbalance").Id,
                WellId = well.Id
                },

                new WellSettingDTO
                {
                Setting = new SettingDTO { Name = "Gearbox Unbalance" , Key="Gearbox Unbalance", SettingType = (SettingType)5 , SettingValueType = SettingValueType.Number} ,
                NumericValue =55,
                SettingId =SettingService.GetSettingByName("Gearbox Unbalance").Id,
                WellId = well.Id,
                },

                  new WellSettingDTO
                {
                Setting = new SettingDTO { Name = "Low Pump Efficiency" , Key="Low Pump Efficiency", SettingType = (SettingType)5 , SettingValueType = SettingValueType.Number} ,
                NumericValue =145,
                SettingId =SettingService.GetSettingByName("Low Pump Efficiency").Id,
                WellId = well.Id,
                },
                  new WellSettingDTO
                {
                Setting = new SettingDTO { Name = "Minimum Card Match" , Key="Minimum Card Match", SettingType = (SettingType)5 , SettingValueType = SettingValueType.Number} ,
                NumericValue =95,
                SettingId =SettingService.GetSettingByName("Minimum Card Match").Id,
                WellId = well.Id
                }
            };
            if (isEdge)
            {
                alarmsettings.Add(new WellSettingDTO
                {
                    Setting = new SettingDTO { Name = "Desired Minimum Pump Fill", Key = "Desired Minimum Pump Fill", SettingType = (SettingType)5, SettingValueType = SettingValueType.Number },
                    NumericValue = 97,
                    SettingId = SettingService.GetSettingByName("Desired Minimum Pump Fill").Id,
                    WellId = well.Id
                });
                alarmsettings.Add(new WellSettingDTO
                {
                    Setting = new SettingDTO { Name = "Desired Pump Off Strokes", Key = "Desired Pump Off Strokes", SettingType = (SettingType)5, SettingValueType = SettingValueType.Number },
                    NumericValue = 1,
                    SettingId = SettingService.GetSettingByName("Desired Pump Off Strokes").Id,
                    WellId = well.Id
                });
            }
            IntelligentAlarmService.UpdateIntelligentAlarmWellSettings(alarmsettings.ToArray());
        }


        public void UpdateForeSiteAlarmValuesToClearAlarms(WellDTO well)
        {
            //update Alrams settings to ensure we get alrams for given st of data

            WellSettingDTO[] alramsettings = new WellSettingDTO[]
            {
                new WellSettingDTO
                {
                Setting = new SettingDTO { Name = "High Gearbox Torque" , Key="High Gearbox Torque" ,  SettingType = (SettingType)5, SettingValueType = SettingValueType.Number} ,
                NumericValue =125,
                SettingId = SettingService.GetSettingByName("High Gearbox Torque").Id,
                WellId = well.Id
                },

                new WellSettingDTO
                {
                Setting = new SettingDTO { Name = "High Beam Load", Key="High Beam Load", SettingType = (SettingType)5  , SettingValueType = SettingValueType.Number} ,
                NumericValue =95,
                SettingId =SettingService.GetSettingByName("High Beam Load").Id,
                WellId = well.Id
                },

                 new WellSettingDTO
                {
                Setting = new SettingDTO { Name = "High Rod Stress" ,Key="High Rod Stress" , SettingType = (SettingType)5, SettingValueType = SettingValueType.Number} ,
                NumericValue =107,
                SettingId =SettingService.GetSettingByName("High Rod Stress").Id,
                WellId = well.Id
                },

                new WellSettingDTO
                {
                Setting = new SettingDTO { Name = "Rod String Unbalance", Key="Rod String Unbalance", SettingType = (SettingType)5, SettingValueType = SettingValueType.Number } ,
                NumericValue =20,
                SettingId =SettingService.GetSettingByName("Rod String Unbalance").Id,
                WellId = well.Id
                },

                new WellSettingDTO
                {
                Setting = new SettingDTO { Name = "Gearbox Unbalance" , Key="Gearbox Unbalance", SettingType = (SettingType)5 , SettingValueType = SettingValueType.Number} ,
                NumericValue =125,
                SettingId =SettingService.GetSettingByName("Gearbox Unbalance").Id,
                WellId = well.Id,
                },

                  new WellSettingDTO
                {
                Setting = new SettingDTO { Name = "Low Pump Efficiency" , Key="Low Pump Efficiency", SettingType = (SettingType)5 , SettingValueType = SettingValueType.Number} ,
                NumericValue =60,
                SettingId =SettingService.GetSettingByName("Low Pump Efficiency").Id,
                WellId = well.Id
                  },

                   new WellSettingDTO
                {
                Setting = new SettingDTO { Name = "Minimum Card Match" , Key="Minimum Card Match", SettingType = (SettingType)5 , SettingValueType = SettingValueType.Number} ,
                NumericValue =98,
                SettingId =SettingService.GetSettingByName("Minimum Card Match").Id,
                WellId = well.Id
                }
            };
            IntelligentAlarmService.UpdateIntelligentAlarmWellSettings(alramsettings);
        }

        public void UpdateForeSiteESPAlarmValuesToClearAlarms(WellDTO well, string[] settingNameArray, double[] settingValueArray)
        {
            //update Alrams settings to ensure we get alrams for given st of data

            List<WellSettingDTO> wellsettings = new List<WellSettingDTO>();
            for (int i = 0; i < settingNameArray.Length; i++)
            {
                WellSettingDTO indsetting = new WellSettingDTO
                {
                    Setting = new SettingDTO { Name = settingNameArray[i], Key = settingNameArray[i], SettingType = (SettingType)5, SettingValueType = SettingValueType.Number },
                    NumericValue = settingValueArray[i],
                    SettingId = SettingService.GetSettingByName(settingNameArray[i]).Id,
                    WellId = well.Id
                };
                wellsettings.Add(indsetting);
            }


            IntelligentAlarmService.UpdateIntelligentAlarmWellSettings(wellsettings.ToArray());
        }

        public void VerifyAlarmsOnWellStatusPage(IntelligentAlarmDTO ialarmdto, List<string> icollection, bool alarmON)
        {
            Assert.IsNotNull(ialarmdto);
            IList<string> ialmlist = ialarmdto.IntelligentAlarmStatus;
            //take only those alarms which we are epecting to show up 


            List<object> actualalms = new List<object>(ialmlist);
            List<string> skilalarmlist = new List<string>();
            foreach (var indalrn in actualalms)
            {
                int skipalmcoumnt = 0;
                foreach (string expalm in icollection)
                {
                    if (indalrn.ToString().Contains(expalm) == false)
                    {
                        skipalmcoumnt++;
                    }
                }
                if (skipalmcoumnt == icollection.Count)
                //We dont want to match those alarms from actual list which we are not expecting 
                //So remove them from compariosn list ;
                {
                    skilalarmlist.Add(indalrn.ToString());
                    Trace.WriteLine($"Added to Skip   alarm collection {indalrn.ToString()}  as it was outside the expected set of trigger events ");
                }
            }
            //Now Remove from the Actual collection
            foreach (var indskipalrn in skilalarmlist)
            {
                actualalms.Remove(indskipalrn);
            }
            CollectionAssert.AllItemsAreUnique(actualalms);
            if (alarmON)
            {
                if (icollection.Count == actualalms.Count)
                {
                    Trace.WriteLine($"Intelligent alarm count is matched. Expected Alarm count: {icollection.Count} and actual alarm count:{actualalms.Count} ");
                    foreach (var actobj in actualalms)
                    {
                        CollectionAssert.Contains(icollection, actobj, $" Expected Intelligent Alarm {actobj.ToString()} is Missing");
                        Trace.WriteLine($"Alarm generated :{actobj.ToString()}");
                    }
                }
                else
                {

                    Trace.WriteLine($"Intelligent alarm count is NOT matched. Expected Alarm count: {icollection.Count} and actual alarm count:{actualalms.Count} ");
                    if (isEdge.Equals("True"))
                    {
                        // We should not see Trend based alram If Test gets executed on Edge Box itself
                        failcount++;
                        Trace.WriteLine("Test is executed on EDGE Box itself and Trend based alarms appeared on Edge.. ");
                    }
                    foreach (var actalm in actualalms)
                    {
                        Trace.WriteLine($"Alarm generated When count mismatched :{actualalms.ToString()}");
                    }

                    foreach (var expalam in icollection)
                    {
                        CollectionAssert.Contains(actualalms, expalam, $" Expected Intelligent Alarm {expalam.ToString()} is Missing");
                    }
                }
            }
            else
            {
                if (icollection.Count == actualalms.Count)
                {
                    Trace.WriteLine($" Alarm Clear Case :  Expected Alarm count: 0 and actual alarm count:{actualalms.Count} ");
                    Assert.Fail("Intelligent Alarms were not cleared");
                }
                else
                {

                    Trace.WriteLine($" Alarm Clear Case : Expected Alarm count: 0 and actual alarm count:{actualalms.Count} ");
                    Assert.AreEqual(actualalms.Count, 0, "Alarm count was not zero when alarms got cleared");
                    foreach (var expalam in icollection)
                    {
                        CollectionAssert.DoesNotContain(actualalms, expalam, $" Expected Intelligent Alarm {expalam.ToString()} is seen when cleared");
                    }
                }
            }
        }

        public void VerifyForeSiteAlarmsOnWellStatusPage(string[] ialarmarray, List<string> icollection, bool alarmON)
        {
            Authenticate();
            Assert.IsNotNull(ialarmarray);
            IList<string> ialmlist = ialarmarray;
            //take only those alarms which we are epecting to show up 


            List<object> actualalms = new List<object>(ialmlist);
            List<string> skilalarmlist = new List<string>();
            foreach (var indalrn in actualalms)
            {
                int skipalmcoumnt = 0;
                foreach (string expalm in icollection)
                {
                    if (indalrn.ToString().Contains(expalm) == false)
                    {
                        skipalmcoumnt++;
                    }
                }
                if (skipalmcoumnt == icollection.Count)
                //We dont want to match those alarms from actual list which we are not expecting 
                //So remove them from compariosn list ;
                {
                    skilalarmlist.Add(indalrn.ToString());
                    Trace.WriteLine($"Added to Skip   alarm collection {indalrn.ToString()}  as it was outside the expected set of trigger events ");
                }
            }
            //Now Remove from the Actual collection
            foreach (var indskipalrn in skilalarmlist)
            {
                actualalms.Remove(indskipalrn);
            }
            CollectionAssert.AllItemsAreUnique(actualalms);
            if (alarmON)
            {
                if (icollection.Count == actualalms.Count)
                {
                    Trace.WriteLine($"Intelligent alarm count is matched. Expected Alarm count: {icollection.Count} and actual alarm count:{actualalms.Count} ");
                    foreach (var actobj in actualalms)
                    {
                        CollectionAssert.Contains(icollection, actobj, $" Expected Intelligent Alarm {actobj.ToString()} is Missing");
                        Trace.WriteLine($"Alarm generated :{actobj.ToString()}");
                    }
                }
                else
                {

                    Trace.WriteLine($"Intelligent alarm count is NOT matched. Expected Alarm count: {icollection.Count} and actual alarm count:{actualalms.Count} ");
                    if (isEdge.Equals("True"))
                    {
                        // We should not see Trend based alram If Test gets executed on Edge Box itself
                        failcount++;
                        Trace.WriteLine("Test is executed on EDGE Box itself and Trend based alarms appeared on Edge.. ");
                    }
                    foreach (var actalm in actualalms)
                    {
                        Trace.WriteLine($"Alarm generated When count mismatched :{actualalms.ToString()}");
                    }

                    foreach (var expalam in icollection)
                    {
                        CollectionAssert.Contains(actualalms, expalam, $" Expected Intelligent Alarm {expalam.ToString()} is Missing");
                    }
                }
            }
            else
            {
                if (icollection.Count == actualalms.Count)
                {
                    Trace.WriteLine($" Alarm Clear Case :  Expected Alarm count: 0 and actual alarm count:{actualalms.Count} ");
                    Assert.Fail("Intelligent Alarms were not cleared");
                }
                else
                {

                    Trace.WriteLine($" Alarm Clear Case : Expected Alarm count: 0 and actual alarm count:{actualalms.Count} ");
                    Assert.AreEqual(actualalms.Count, 0, "Alarm count was not zero when alarms got cleared");
                    foreach (var expalam in icollection)
                    {
                        CollectionAssert.DoesNotContain(actualalms, expalam, $" Expected Intelligent Alarm {expalam.ToString()} is seen when cleared");
                    }
                }
            }
        }
        public void VerifyAlarmsOnForesiteAlarmsHistoryPage(WellDTO well, List<string> icollection, string startdate, string enddate, bool alarmON)
        {

            String result = new String(' ', 50);
            AlarmHistoryDTO[] fsalms = IntelligentAlarmService.GetIntelligentAlarmHistoryByWellIdAndDateRange(well.Id.ToString(), startdate, enddate);
            string almtxt = alarmON ? "Alarm SET " : "Alarm Clear";
            Trace.WriteLine($"*******************ForeSite Alarm history records: {almtxt}*****************************");
            int alarmstring_length = 75;
            Trace.WriteLine($"Alarm Status {new String(' ', alarmstring_length - "Alarm Status".Length)} | Started Time{new String(' ', alarmstring_length - "Started Time".Length)} |ClearTime {new String(' ', alarmstring_length - "ClearTime".Length)}|");
            foreach (AlarmHistoryDTO alamdto in fsalms)
            {
                string fsalm = alamdto.AlarmStatus;
                if (icollection.Contains(fsalm) == false)
                {
                    Trace.WriteLine($"The Alarm {fsalm} was additional alarm and is  Skipped ");
                    continue;
                }
                DateTime alstarttime = alamdto.StartedTime;
                DateTime? alclearttime = alamdto.ClearedTime;

                Trace.WriteLine($"{fsalm} {new String(' ', alarmstring_length - fsalm.Length)} | {alstarttime}{new String(' ', alarmstring_length - alstarttime.ToString().Length)} |{alclearttime} {new String(' ', alarmstring_length - alclearttime.ToString().Length)}|");

                if (fsalm.Equals("Pattern Card Match"))
                {
                    Trace.WriteLine("Pattern Card Alarm text was NOT matched KNOWN Issue  :FRI-2330");
                    failcount++;

                }
                else
                {
                    CollectionAssert.Contains(icollection, fsalm, $" Expected Intelligent Alarm {fsalm.ToString()} is not updated in ForeSite alarm history");
                    Assert.IsNotNull(alstarttime, $" Expected Intelligent Alarm {fsalm.ToString()} Start Date time stamp is NULL");
                    if (alarmON)
                    {
                        Assert.IsNull(alclearttime, $" Expected Intelligent Alarm {fsalm.ToString()} Clear Date time stamp is NOT NULL");
                    }
                    else
                    {
                        Assert.IsNotNull(alclearttime, $" Expected Intelligent Alarm {fsalm.ToString()} Clear Date time stamp is  NULL");
                    }
                }


            }
        }

        public void VerifyAlarmsOnAllAlarmsHistoryPage(WellDTO well, List<string> icollection, string startdate, string enddate, bool alarmON)
        {
            AlarmHistoryDTO[] allalms = IntelligentAlarmService.GetIntelligentAlarmHistoryByWellIdAndDateRange(well.Id.ToString(), (DateTime.Today - TimeSpan.FromDays(30)).ToUniversalTime().ToISO8601(),
                    DateTime.Today.ToUniversalTime().ToISO8601());
            String result = new String(' ', 50);
            string almtxt = alarmON ? "Alarm SET " : "Alarm Clear";
            int alarmstring_length = 75;
            Trace.WriteLine($"*******************All Alarm history records: {almtxt}*****************************");
            Trace.WriteLine($"Alarm Status {new String(' ', alarmstring_length - "Alarm Status".Length)} | Started Time{new String(' ', alarmstring_length - "Started Time".Length)} |ClearTime {new String(' ', alarmstring_length - "ClearTime".Length)}|");
            foreach (AlarmHistoryDTO alamdto in allalms)
            {
                string fsalm = alamdto.AlarmStatus;
                if (icollection.Contains(fsalm) == false)
                {
                    Trace.WriteLine($"The Alarm {fsalm} was additional alarm and is  Skipped ");
                    continue;
                }
                DateTime alstarttime = alamdto.StartedTime;
                DateTime? alclearttime = alamdto.ClearedTime;


                Trace.WriteLine($"{fsalm} {new String(' ', alarmstring_length - fsalm.Length)} | {alstarttime}{new String(' ', alarmstring_length - alstarttime.ToString().Length)} |{alclearttime} {new String(' ', alarmstring_length - alclearttime.ToString().Length)}|");
                if (fsalm.Equals("Pattern Card Match"))
                {
                    Trace.WriteLine("Pattern Card Alarm text was NOT matched KNOWN Issue  :FRI-2330");
                    failcount++;

                }
                else
                {
                    CollectionAssert.Contains(icollection, fsalm, $" Expected Intelligent Alarm {fsalm.ToString()} is not updated in ForeSite alarm history");

                    Assert.IsNotNull(alstarttime, $" Expected Intelligent Alarm {fsalm.ToString()} Start Date time stamp is NULL");
                    if (alarmON)
                    {
                        Assert.IsNull(alclearttime, $" Expected Intelligent Alarm {fsalm.ToString()} Clear Date time stamp is NOT NULL");
                    }
                    else
                    {
                        Assert.IsNotNull(alclearttime, $" Expected Intelligent Alarm {fsalm.ToString()} Clear Date time stamp is  NULL");
                    }
                }


            }
        }



        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void VerifyCustomizedIntelligentAlarmAPI()
        {
            // *************  FRWM-127 :Customized intelligent alerts - new API for setting and clearing ForeSite alarm
            // *************API testing Sub task : FRWM- 6682 
            // ************* New API  :   AlarmTypeDTO AddOrUpdateAlarmType(AlarmTypeDTO alarmType);
            //              void RemoveAlarmType(string alarmTypeId);
            //               CurrentAlarmDTO[] SetWellAlarms(string wellId, IEnumerable<AlarmValueDTO> alarmsToSet);

            //This API already existed and covers the requirement of being able to clear alarms:
            //                      void ClearWellAlarmsByTypeIds(string wellId, IEnumerable<long> alarmTypeIdsToClear);


            // Step 1 : Create a New Alarm Type and check whether it gets Added and Can be updated
            string newcustomalarmtext1 = "CUSTOM_ALERT DECLINE DUE TO HEADER CHANGE";
            string newcustomalarmtext2 = "CUSTOM_ALERT RATE DECLINE AND WC DECREASE AFTER HEADER CHANGE";
            string newcustomalarmtext3 = "CUSTOM_ALERT DECLINE AND WC INCREASE AFTER HEADER CHANGE";
            AlarmTypeDTO alarmtypedto1 = new AlarmTypeDTO();
            alarmtypedto1.AlarmType = newcustomalarmtext1;
            AlarmTypeDTO alarmtypedto2 = new AlarmTypeDTO();
            alarmtypedto2.AlarmType = newcustomalarmtext2;
            AlarmTypeDTO alarmtypedto3 = new AlarmTypeDTO();
            alarmtypedto3.AlarmType = newcustomalarmtext3;
            // Add Custom alarms 
            AlarmService.AddOrUpdateAlarmType(alarmtypedto1);
            AlarmService.AddOrUpdateAlarmType(alarmtypedto2);
            AlarmService.AddOrUpdateAlarmType(alarmtypedto3);

            AlarmTypeDTO[] alarmTypes = AlarmService.GetAlarmTypes();
            Assert.IsNotNull(alarmTypes.FirstOrDefault(x => x.AlarmType == newcustomalarmtext1), "Custom Intelligent alarm type could not be added");
            Assert.IsNotNull(alarmTypes.FirstOrDefault(x => x.AlarmType == newcustomalarmtext2), "Custom Intelligent alarm type could not be added");
            Assert.IsNotNull(alarmTypes.FirstOrDefault(x => x.AlarmType == newcustomalarmtext3), "Custom Intelligent alarm type could not be added");

            long alarmtypeId1 = alarmTypes.FirstOrDefault(x => x.AlarmType == newcustomalarmtext1).Id;
            long alarmtypeId2 = alarmTypes.FirstOrDefault(x => x.AlarmType == newcustomalarmtext2).Id;
            long alarmtypeId3 = alarmTypes.FirstOrDefault(x => x.AlarmType == newcustomalarmtext3).Id;

            alarmtypedto1.Id = alarmtypeId1;
            alarmtypedto2.Id = alarmtypeId2;
            alarmtypedto3.Id = alarmtypeId3;
            // Add to Test AlarmType Clean up list :
            _alarmTypesToRemove.Add(alarmtypedto1);
            _alarmTypesToRemove.Add(alarmtypedto2);
            _alarmTypesToRemove.Add(alarmtypedto3);
            WellDTO well = AddNonRRLWell(GetFacilityId("ESPWELL_", 2), WellTypeId.ESP);
            _wellsToRemove.Add(well);
            try
            {
                //Create new Well

                //Set the Newly created Intelligent Alarm Type
                List<AlarmValueDTO> list = new List<AlarmValueDTO>();
                AlarmValueDTO alarmvalueTHP = new AlarmValueDTO { AlarmTypeId = alarmtypeId1, NumericValue = 150, StringValue = "THP more than threshold" };
                AlarmValueDTO alarmvalueFLP = new AlarmValueDTO { AlarmTypeId = alarmtypeId2, NumericValue = 120, StringValue = "FLP more than threshold" };
                AlarmValueDTO alarmvalueWC = new AlarmValueDTO { AlarmTypeId = alarmtypeId3, NumericValue = 1.5, StringValue = "WC more than threshold" };

                list.Add(alarmvalueTHP);
                list.Add(alarmvalueFLP);
                list.Add(alarmvalueWC);

                IEnumerable<AlarmValueDTO> alarmValuetoSet = list;
                CurrentAlarmDTO[] newcurrentalarm = AlarmService.SetWellAlarms(well.Id.ToString(), alarmValuetoSet);

                Trace.WriteLine($"New Alarms have been added with { newcurrentalarm.Length} ");
                //Verify if the newly Set Custom  Alarm shows up 

                List<string> icollection = new List<string>();
                icollection.Add(newcustomalarmtext1);
                icollection.Add(newcustomalarmtext2);
                icollection.Add(newcustomalarmtext3);
                string start_date = (DateTime.Today - TimeSpan.FromDays(30)).ToUniversalTime().ToISO8601();
                string end_date = DateTime.Today.ToUniversalTime().ToISO8601();
                //  Verify Alarm on Well status Page
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                var wellstatusdata = SurveillanceService.GetWellStatusData(well.Id.ToString());
                ESPWellStatusValueDTO espwellldata = (ESPWellStatusValueDTO)wellstatusdata.Value;

                string[] ialarmdto = espwellldata.IntelligentAlarmMessage;
                VerifyForeSiteAlarmsOnWellStatusPage(ialarmdto, icollection, true);
                VerifyAlarmsOnAllAlarmsHistoryPage(well, icollection, start_date, end_date, true);
                //Verify Alarms on ForeSite Alarms History Status Page
                VerifyAlarmsOnForesiteAlarmsHistoryPage(well, icollection, start_date, end_date, true);
                //Finally CLear all alarm TypesDto  
                List<long> almtypelisttoclear = new List<long>();
                almtypelisttoclear.Add(alarmTypes.FirstOrDefault(x => x.AlarmType == newcustomalarmtext1).Id);
                almtypelisttoclear.Add(alarmTypes.FirstOrDefault(x => x.AlarmType == newcustomalarmtext2).Id);
                almtypelisttoclear.Add(alarmTypes.FirstOrDefault(x => x.AlarmType == newcustomalarmtext3).Id);
                IEnumerable<long> alarmTypeDtoToClear = almtypelisttoclear;
                AlarmService.ClearWellAlarmsByTypeIds(well.Id.ToString(), alarmTypeDtoToClear);

                Trace.WriteLine($"New Alarms have been Cleared  with { newcurrentalarm.Length} ");
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                wellstatusdata = SurveillanceService.GetWellStatusData(well.Id.ToString());

                espwellldata = (ESPWellStatusValueDTO)wellstatusdata.Value;
                ialarmdto = espwellldata.IntelligentAlarmMessage;

                //Verify Alarms on Well Status Page for Clear
                VerifyForeSiteAlarmsOnWellStatusPage(ialarmdto, icollection, false);
                //Verify if the newly Set alarms are  Cleared
                VerifyAlarmsOnAllAlarmsHistoryPage(well, icollection, start_date, end_date, false);
                //Verify Alarms on ForeSite Alarms History Status Page
                VerifyAlarmsOnForesiteAlarmsHistoryPage(well, icollection, start_date, end_date, false);
                WellConfigurationService.RemoveWellConfig(well.Id.ToString());
                //Remove Alarms 
                AlarmService.RemoveAlarmType(alarmtypedto1.Id.ToString());
                AlarmService.RemoveAlarmType(alarmtypedto2.Id.ToString());
                AlarmService.RemoveAlarmType(alarmtypedto3.Id.ToString());

                alarmTypes = AlarmService.GetAlarmTypes();
                Assert.IsNull(alarmTypes.FirstOrDefault(x => x.AlarmType == newcustomalarmtext1), "Custom Intelligent alarm type could not be added");
                Assert.IsNull(alarmTypes.FirstOrDefault(x => x.AlarmType == newcustomalarmtext2), "Custom Intelligent alarm type could not be added");
                Assert.IsNull(alarmTypes.FirstOrDefault(x => x.AlarmType == newcustomalarmtext3), "Custom Intelligent alarm type could not be added");
                //Remove from clean uo list as we are removing manually within test method as we have ensured alarm deletion
                _alarmTypesToRemove.Remove(alarmtypedto1);
                _alarmTypesToRemove.Remove(alarmtypedto2);
                _alarmTypesToRemove.Remove(alarmtypedto3);

            }
            finally
            {
                // SurveillanceService.RemoveWellStatusCacheEntry(well.Id.ToString());
            }



        }


        public static IUisClient GetUisClient()
        {
            var dss = GetDomainSiteService1("UIS");
            Console.WriteLine($"Going to use {dss.GetDomainSiteService().ToString()}");

            IUisClient uisClient = ClientProxyMgr.GetUisClient(dss);
            try
            {
                uisClient.Connect(dss);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to {dss.GetDomainSiteService()}: {ex.Message}.");
                Console.WriteLine($"Failed to connect to {dss.GetDomainSiteService()}: {ex.ToString()}.");
            }

            if (!uisClient.IsConnected)
            {
                Console.WriteLine($"Failed to connect to {dss.GetDomainSiteService()}.");
            }

            return uisClient;
        }

        private static CygNet.COMAPI.Core.DomainSiteService GetDomainSiteService1(string service)
        {
            var dss = new CygNet.COMAPI.Core.DomainSiteService(Convert.ToUInt16(s_domain), s_site, service);
            return dss;
        }

        /* This test will be uncommented once the entire test API of GL Edge Automation is ready
        [TestCategory(TestCategories.IntelligentAlarmsServiceTests), TestMethod]
        public void VerifyEdgeGLIntelligentAlarms()
        {
            if (!CheckIfCanExecuteForeSiteSchedulerJob())
            {
                Trace.WriteLine("Test could not run as could not find path of ForeSite Schedular.exe");
                return;
            }

            object objSettigvaue = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.FORESITE_FOR_EDGE);
            isEdge = (objSettigvaue.ToString() == "0") ? false : true;
            Trace.WriteLine($"Test Run Mode for Edge was {isEdge}");

            string facidgl = GetFacilityId("GLWELL_", 1);
            WellDTO wellDTO = AddNonRRLWell(facidgl, WellTypeId.GLift);
            _wellsToRemove.Add(wellDTO);

            string startDate = DateTime.Today.AddDays(-30).ToUniversalTime().ToISO8601();
            string endDate = DateTime.Today.ToUniversalTime().ToISO8601();

            WellDailyAverageArrayAndUnitsDTO wellDailyAvgData = SurveillanceService.GetDailyAverages(wellDTO.Id.ToString(),startDate, endDate);
            WellTestAndUnitsDTO latestValidWellTest = WellTestDataService.GetLatestValidWellTestByWellId(wellDTO.Id.ToString());

            //Check if daily average data does not exist for last 30 days or we have a well test after the last daily average date
            if (wellDailyAvgData.Values == null || (wellDailyAvgData.Values != null && latestValidWellTest.Value.SampleDate > wellDailyAvgData.Values.FirstOrDefault().TestDate))
            {
#region First Time Check
                RunAnalysisTaskSchedular("runAllocationTimeSpan");

                //Check if performance curve is generated
                NodalAnalysisInputAndUnitsDTO glAnalysisInput = WellTestDataService.GetAnalysisInputDataAndUnits(latestValidWellTest.Value.Id.ToString());
                Assert.IsNotNull(glAnalysisInput, "Failed to get Analysis input with the provided test data");
                Assert.IsNotNull(glAnalysisInput.CalibrationData, "Calibration data is not available for the obtained Analysis input");
                Assert.IsNotNull(glAnalysisInput.ModelData, "Model data is not available for the obtained Analysis input");
                Assert.IsNotNull(glAnalysisInput.TestData, "Test data is not available for the obtained Analysis input");

                //GasLiftAnalysisResultsAndUnitsDTO glAnalysisResult = WellTestDataService.PerformGasLiftAnalysis(glAnalysisInput);
                //Assert.IsNotNull(glAnalysisResult.OperatingPointResults, "Unable to get Operating point results after GL Analysis");

                wellDailyAvgData = SurveillanceService.GetDailyAverages(wellDTO.Id.ToString(),startDate,endDate);
                Assert.IsNotNull(wellDailyAvgData);
                Assert.IsNotNull(wellDailyAvgData.Values[0].GasInjectionRateOptimized);

#endregion
            }
            else
            {
                if(wellDailyAvgData.Values[0].GasJectionDepthActual!=wellDailyAvgData.Values[1].GasInjectionRateOptimized)
                {

                }
            }
        }
        */
    }
}
