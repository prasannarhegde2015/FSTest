using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using CxCvsLib;
using CXDDSLib;
using CXUISLib;
using CygNet.API.Historian;
using CygNet.API.Points;
using CygNet.COMAPI.Client;
using CygNet.COMAPI.EIE;
using CygNet.Data.Core;
using CygNet.Data.Facilities;
using CygNet.Data.Historian;
using CygNet.Data.Points;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.DynaCardLibrary.API.Enums;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using Weatherford.POP.Quantities;
using Weatherford.POP.Interfaces;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class SurveillanceServiceTests : APIClientTestBase
    {
        protected List<WellDTO> _addedWells;
        protected IPEDashboardService _peDashboardService;
        protected IPEDashboardService PEDashboardService
        {
            get { return _peDashboardService ?? (_peDashboardService = _serviceFactory.GetService<IPEDashboardService>()); }
        }
        private static object s_lockObj = new object();

        private static ClientProxyManager s_clientProxyMgr;

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

        private static DdsClient _ddsClient = new DdsClient();
        private static UisClient _uisClient = new UisClient();
        protected static Client _vhsClient = new Client(GetDomainSiteServiceForSurveillance("VHS"));
        protected static Dictionary<Name, List<HistoricalEntry>> _entriesByName = new Dictionary<Name, List<HistoricalEntry>>();
        protected static List<SinglePoint> GasLiftPoints = new List<SinglePoint>();
        protected static List<SinglePoint> NFWLiftPoints = new List<SinglePoint>();
        protected static List<SinglePoint> GasInjLiftPoints = new List<SinglePoint>();
        protected static List<SinglePoint> WaterInjLiftPoints = new List<SinglePoint>();
        protected static List<SinglePoint> ESPLiftPoints = new List<SinglePoint>();
        protected static List<SinglePoint> RPOCPoints = new List<SinglePoint>();
        protected static List<SinglePoint> SAMPoints = new List<SinglePoint>();
        protected static List<SinglePoint> IWCPoints = new List<SinglePoint>();
        protected static List<SinglePoint> PGLPoints = new List<SinglePoint>();

        List<SinglePoint> pointsList = new List<SinglePoint>();
        private static Dictionary<string, string> udcoriginalvalue = new Dictionary<string, string>();

        public void AddWells(string facility_tag, bool scanWells = true)
        {
            int end = 0;
            if (facility_tag.Contains("RPOC_"))
                end = s_wellsEnd;
            else
                end = 5;//only 5 facilities are created in CygNet for the EPICF,8800,AEPOC controller types and tests for Lufkin SAM taking longer time to complete execution hence just limiting its test for 5 Wells
            string facilityIdFormat = "";
            if (s_isRunningInATS)
                facilityIdFormat = "D5";
            else
                facilityIdFormat = "D4";
            WellDTO[] allwells = WellService.GetAllWells();
            int welCount = 0;
            if (allwells.Length != 0)
            {
                foreach (WellDTO well in allwells)
                {
                    if (well.FacilityId.Contains(facility_tag))
                    {
                        welCount = welCount + 1;
                    }
                }
            }
            if (welCount == 0)
            {
                _addedWells = new List<WellDTO>();
                for (int i = s_wellStart; i <= end; i++)
                {
                    string wellName = facility_tag + i.ToString(facilityIdFormat);
                    WellDTO well = SetDefaultFluidType(new WellDTO()
                    {
                        Name = wellName,
                        FacilityId = wellName,
                        DataConnection = GetDefaultCygNetDataConnection(),
                        WellType = WellTypeId.RRL,
                        Lease = "Lease_" + i.ToString("D2"),
                        Foreman = "Foreman" + i.ToString("D2"),
                        Field = "Field" + i.ToString("D2"),
                        Engineer = "Engineer" + i.ToString("D2"),
                        GaugerBeat = "GaugerBeat" + i.ToString("D2"),
                        GeographicRegion = "GeographicRegion" + i.ToString("D2"),
                        welUserDef01 = "State_" + i.ToString("D2"),
                        welUserDef02 = "User_" + i.ToString("D2"),
                        IntervalAPI = "IntervalAPI" + i.ToString("D2"),
                        SubAssemblyAPI = "SubAssemblyAPI" + i.ToString("D2"),
                        AssemblyAPI = "AssemblyAPI" + i.ToString("D2"),
                        CommissionDate = DateTime.Today.AddDays(i)
                    });
                    WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
                    WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(well.Name));
                    Assert.IsNotNull(addedWell, "Failed to get added well.");
                    _wellsToRemove.Add(addedWell);
                    _addedWells.Add(addedWell);
                    if (scanWells)
                    {
                        SurveillanceService.IssueCommandForSingleWell(addedWell.Id, WellCommand.DemandScan.ToString());
                    }
                }
            }
        }

        private void AddSingleWell(string facilityId, bool scanWells = true)
        {
            WellConfigurationService.AddWellConfig(new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = DefaultWellName,
                    FacilityId = facilityId,
                    DataConnection = GetDefaultCygNetDataConnection(),
                    SubAssemblyAPI = "SubAssemblyAPI",
                    AssemblyAPI = "AssemblyAPI",
                    CommissionDate = DateTime.Today,
                    WellType = WellTypeId.RRL,
                })
            });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            if (scanWells)
            {
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
            }
        }

        public void AddWell(string facility_tag, string facility, bool scanWells = true)
        {
            _addedWells = new List<WellDTO>();
            int end = 0;
            if (facility_tag.Contains("RPOC_") || facility_tag.Contains("SAM_"))
                end = s_wellsEnd;
            else
                end = 5;//only 5 facilities are created in CygNet for the EPICF,8800,AEPOC controller types
            if (s_isRunningInATS)
            {
                for (int i = s_wellStart; i <= end; i++)
                {
                    string wellName = facility + i.ToString("D5");
                    WellDTO well = SetDefaultFluidType(new WellDTO() { Name = wellName, FacilityId = wellName, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI" + i.ToString("D2"), SubAssemblyAPI = "SubAssemblyAPI" + i.ToString("D2"), AssemblyAPI = "AssemblyAPI" + i.ToString("D2"), CommissionDate = DateTime.Today, WellType = WellTypeId.RRL });
                    WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
                    WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(well.Name));
                    Assert.IsNotNull(addedWell);
                    _wellsToRemove.Add(addedWell);
                    _addedWells.Add(addedWell);
                    if (scanWells)
                    {
                        SurveillanceService.IssueCommandForSingleWell(addedWell.Id, "DemandScan");
                    }
                }
            }
            else
            {
                WellDTO well = SetDefaultFluidType(new WellDTO() { Name = "New Well", FacilityId = facility_tag, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL });
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
                WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(well.Name));
                Assert.IsNotNull(addedWell);
                _wellsToRemove.Add(addedWell);
                _addedWells.Add(addedWell);
            }
        }

        public void GetWellGroupStatus(string facilty_tag)
        {
            List<WellDTO> wells = null;
            if (_addedWells != null && _addedWells.Count > 0)
            {
                wells = _addedWells;
            }
            else
            {
                wells = WellService.GetAllWells().ToList();
            }
            var newGroup = new WellGroupStatusQueryDTO();
            newGroup.WellType = WellTypeId.RRL;
            //Get well Filters
            WellFilterDTO filters = WellService.GetWellFilter(null);

            //Well Column Filter
            List<string> columnName = new List<string>();
            List<string> filterConditions = new List<string>();
            List<string> filterDataTypes = new List<string>();
            List<string> filterValues = new List<string>();
            WellGroupStatusColumnFilterDTO columnFilter = new WellGroupStatusColumnFilterDTO();
            columnFilter.ColumnNames = columnName.ToArray();
            columnFilter.FilterConditions = filterConditions.ToArray();
            columnFilter.FilterDataTypes = filterDataTypes.ToArray();
            columnFilter.FilterValues = filterValues.ToArray();

            //No Filters Selected
            WellFilterDTO wellbyFilters = new WellFilterDTO();
            newGroup.WellFilter = wellbyFilters;
            newGroup.wellGroupStatusColumnFilter = columnFilter;
            var wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(newGroup);
            Assert.AreEqual(wells.Count(), wellsbyFilter.TotalRecords);

            //All Filters Selected
            var allGroup = new WellGroupStatusQueryDTO();
            allGroup.WellType = WellTypeId.RRL;
            WellFilterDTO wellbyFilter = new WellFilterDTO();
            wellbyFilter.welEngineerValues = filters?.welEngineerValues;
            wellbyFilter.welFieldNameValues = filters?.welFieldNameValues;
            wellbyFilter.welForemanValues = filters?.welForemanValues;
            wellbyFilter.welGaugerBeatValues = filters?.welGaugerBeatValues;
            wellbyFilter.welGeographicRegionValues = filters?.welGeographicRegionValues;
            wellbyFilter.welLeaseNameValues = filters?.welLeaseNameValues;
            wellbyFilter.welUserDef01Values = filters?.welUserDef01Values;
            wellbyFilter.welUserDef02Values = filters?.welUserDef02Values;
            wellbyFilter.welFK_r_WellTypeValues = filters?.welFK_r_WellTypeValues;
            allGroup.WellFilter = wellbyFilter;
            allGroup.wellGroupStatusColumnFilter = columnFilter;
            var wellsbyFilter_all = SurveillanceServiceClient.GetWellGroupStatus<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(allGroup);
            Assert.AreEqual(wells.Count(), wellsbyFilter_all.TotalRecords);

            //Static Filter 'welForeman' selected
            var welForemanGroup = new WellGroupStatusQueryDTO();
            welForemanGroup.WellType = WellTypeId.RRL;
            WellFilterDTO welForemanFilter = new WellFilterDTO();
            welForemanFilter.welForemanValues = new List<WellFilterValueDTO> { filters?.welForemanValues?[0] };
            welForemanFilter.welFK_r_WellTypeValues = new List<WellFilterValueDTO> { filters?.welFK_r_WellTypeValues?[0] };
            welForemanGroup.WellFilter = welForemanFilter;
            welForemanGroup.wellGroupStatusColumnFilter = columnFilter;
            var wellsbyFilter_welForemanGroup = SurveillanceServiceClient.GetWellGroupStatus<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(welForemanGroup);
            int staticCount = 0;
            foreach (WellDTO staticwells in wells)
            {
                if (staticwells.Foreman == filters?.welForemanValues?[0].Value)
                    staticCount = staticCount + 1;
            }
            Assert.AreEqual(staticCount, wellsbyFilter_welForemanGroup.TotalRecords);

            //Check RunTimeAverage, AvgCyclesYesterday, AvgInferredProductionYesterday for a selected group
            WellDTO[] allWells = WellService.GetWellsByFilter(welForemanFilter);
            string _Totaltime = "00:00:00";
            TimeSpan _Sumtime = TimeSpan.Parse(_Totaltime);
            double sumcyclesYesterday = 0;
            double suminferredproduvtionYesterday = 0;
            foreach (WellDTO well in allWells)
            {
                if (well.FacilityId != null)
                {
                    if (well.FacilityId.Contains(facilty_tag))
                    {
                        SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());

                        RRLWellStatusValueDTO wellGroupStatusdata = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(well.Id.ToString()).Value;
                        TimeSpan _time = TimeSpan.FromSeconds(wellGroupStatusdata.RunTimeYesterday ?? 0.0);
                        _Sumtime = _Sumtime + _time;

                        double cyclesYesterday = wellGroupStatusdata.CyclesYesterday ?? 0;
                        sumcyclesYesterday = sumcyclesYesterday + cyclesYesterday;

                        double inferredproduvtionYesterday = wellGroupStatusdata.InferredProductionYesterday ?? 0;
                        suminferredproduvtionYesterday = suminferredproduvtionYesterday + inferredproduvtionYesterday;
                    }
                }
            }

            //****** the averages in the group status for RRL wells are no longer get calculated ***//

            //string average = Math.Truncate(_Sumtime.TotalSeconds / (staticCount)).ToString();
            //var avgnewGroup = new WellGroupStatusQueryDTO();
            //avgnewGroup.WellType = WellTypeId.RRL;
            //avgnewGroup.WellFilter = welForemanFilter;
            //avgnewGroup.wellGroupStatusColumnFilter = columnFilter;
            //var totalAverage = SurveillanceServiceClient.GetWellGroupStatus<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(avgnewGroup);
            //double Expected = totalAverage?.Average?.RunTimeYesterday ?? 0.0;
            //TimeSpan _Expected = TimeSpan.FromSeconds(Expected);
            //string strsuminferredproduvtionYesterday = Math.Round((suminferredproduvtionYesterday / (staticCount)), 1).ToString();
            //string strsumcyclesYesterday = Math.Round((sumcyclesYesterday / (staticCount)), 2).ToString();

            //Assert.AreEqual(_Expected.TotalSeconds.ToString(), average);
            //Assert.AreEqual(strsuminferredproduvtionYesterday, Math.Round(totalAverage.Average.InferredProductionYesterday.Value, 1).ToString());
            //Assert.AreEqual(strsumcyclesYesterday, totalAverage.Average.CyclesYesterday.ToString());
        }

        public void GetWellStatusData(string facility_tag)
        {
            List<WellDTO> wells = null;
            if (_addedWells != null && _addedWells.Count > 0)
            {
                wells = _addedWells;
            }
            else
            {
                wells = WellService.GetAllWells().ToList();
            }

            //Well Column Filter
            List<string> columnName = new List<string>();
            List<string> filterConditions = new List<string>();
            List<string> filterDataTypes = new List<string>();
            List<string> filterValues = new List<string>();
            WellGroupStatusColumnFilterDTO columnFilter = new WellGroupStatusColumnFilterDTO();
            columnFilter.ColumnNames = columnName.ToArray();
            columnFilter.FilterConditions = filterConditions.ToArray();
            columnFilter.FilterDataTypes = filterDataTypes.ToArray();
            columnFilter.FilterValues = filterValues.ToArray();

            List<WellDTO> wellsData = new List<WellDTO>();
            foreach (WellDTO well in wells)
            {
                if (well.FacilityId != null)
                {
                    if (well.FacilityId.Contains(facility_tag))
                    {
                        RRLWellStatusValueDTO wellGroupStatusdata = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(well.Id.ToString()).Value;
                        if (wellGroupStatusdata.RunTimeYesterday != null)
                        {
                            wellsData.Add(well);
                        }
                    }
                }
            }
            long count = 0;
            foreach (WellDTO well in wellsData)
            {
                if (well.FacilityId != null)
                {
                    if (well.FacilityId.Contains(facility_tag))
                    {
                        RRLWellStatusValueDTO wellGroupStatusdata = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(well.Id.ToString()).Value;
                        Assert.AreEqual(well.Name, wellGroupStatusdata.WellName);
                        count = count + 1;
                        SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                    }
                }
            }
            //Check RunTimeAverage, AvgCyclesYesterday, AvgInferredProductionYesterday
            string _Totaltime = "00:00:00";
            TimeSpan _Sumtime = TimeSpan.Parse(_Totaltime);
            double sumcyclesYesterday = 0;
            double suminferredproduvtionYesterday = 0;
            foreach (WellDTO well in wellsData)
            {
                if (well.FacilityId != null)
                {
                    if (well.FacilityId.Contains(facility_tag))
                    {
                        SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());

                        RRLWellStatusValueDTO wellGroupStatusdata = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(well.Id.ToString()).Value;
                        TimeSpan _time = TimeSpan.FromSeconds(wellGroupStatusdata.RunTimeYesterday ?? 0.0);
                        _Sumtime = _Sumtime + _time;

                        double cyclesYesterday = wellGroupStatusdata.CyclesYesterday ?? 0;
                        sumcyclesYesterday = sumcyclesYesterday + cyclesYesterday;

                        double inferredproduvtionYesterday = wellGroupStatusdata.InferredProductionYesterday ?? 0;
                        suminferredproduvtionYesterday = suminferredproduvtionYesterday + inferredproduvtionYesterday;
                    }
                }
            }
            //****** the averages in the group status for RRL wells are no longer get calculated ***//
            //string average = Math.Truncate(_Sumtime.TotalSeconds / (count)).ToString();
            //WellFilterDTO wellbyFilter = new WellFilterDTO();
            //var newGroup = new WellGroupStatusQueryDTO();
            //newGroup.WellType = WellTypeId.RRL;
            //newGroup.WellFilter = wellbyFilter;
            //newGroup.wellGroupStatusColumnFilter = columnFilter;
            //var totalAverage = SurveillanceServiceClient.GetWellGroupStatus<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(newGroup);
            //double Expected = totalAverage.Average.RunTimeYesterday ?? 0.0;
            //TimeSpan _Expected = TimeSpan.FromSeconds(Expected);
            //string strsuminferredproduvtionYesterday = Math.Round((suminferredproduvtionYesterday / (count)), 1).ToString();
            //string strsumcyclesYesterday = Math.Round((sumcyclesYesterday / (count)), 2).ToString();

            //Assert.AreEqual(_Expected.TotalSeconds.ToString(), average);
            //Assert.AreEqual(strsuminferredproduvtionYesterday, Math.Round(totalAverage.Average.InferredProductionYesterday.Value, 1).ToString());
            //Assert.AreEqual(strsumcyclesYesterday, totalAverage.Average.CyclesYesterday.ToString());
        }

        public void IssueCommandForOneWell(string facility_tag, int sleepTime = 200)
        {
            List<WellDTO> allWells = null;
            if (_addedWells != null && _addedWells.Count > 0)
            {
                allWells = _addedWells;
            }
            else
            {
                allWells = WellService.GetAllWells().ToList();
            }

            // Test Stop and Start command
            foreach (WellDTO well in allWells)
            {
                if (well.FacilityId != null)
                {
                    if (well.FacilityId.Contains(facility_tag))
                    {
                        // Get current well status
                        RRLWellStatusValueDTO beforeWellGroupStatusdata = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(well.Id.ToString()).Value;
                        string beforePocStatus = beforeWellGroupStatusdata.POCMode;

                        // Issue demand scan command
                        SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());

                        // Get latest well status
                        RRLWellStatusValueDTO afterWellGroupStatusdata = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(well.Id.ToString()).Value;
                        string afterPocStatus = afterWellGroupStatusdata.POCMode;
                        Assert.AreEqual(beforePocStatus, afterPocStatus);

                        // Issue Clear Error command
                        SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.ClearErrors.ToString());
                        System.Threading.Thread.Sleep(100);

                        // Issue Stop well command
                        SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.StopAndLeaveDown.ToString());
                        System.Threading.Thread.Sleep(sleepTime);

                        // Issue demand scan command to get the latest well status
                        SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                        System.Threading.Thread.Sleep(sleepTime);

                        // Get latest well status to check if well is stopped
                        afterWellGroupStatusdata = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(well.Id.ToString()).Value;
                        if (afterWellGroupStatusdata.AlarmStatus != null)
                        {
                            if (afterWellGroupStatusdata.AlarmStatus.Contains("\"FAILED:"))
                            {
                                Trace.WriteLine($"{afterWellGroupStatusdata.WellName} is in Comm Failure.");
                                continue;
                            }
                        }
                        string stopStatus = "";
                        if (well.FacilityId.StartsWith("AEPOC_"))
                            stopStatus = "Stop";
                        else if (well.FacilityId.StartsWith("SAM_"))
                            stopStatus = "Down";
                        else
                            stopStatus = "OFF";
                        int i = 0;
                        while (afterWellGroupStatusdata.POCMode != stopStatus && i < 30)
                        {
                            // Wait for some time for UIS to write DDS tx
                            System.Threading.Thread.Sleep(2000);
                            i++;
                            afterWellGroupStatusdata = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(well.Id.ToString()).Value;
                        }

                        Assert.AreEqual(stopStatus, afterWellGroupStatusdata.POCMode, $"Failed to get '{stopStatus}' status for '{afterWellGroupStatusdata.WellName}' even after {i * 500} msec.");
                        Trace.WriteLine($"Expected stop status was fetched after {i * 500 / 1000} seconds.");

                        // Issue start command
                        SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.StartRPC.ToString());
                        System.Threading.Thread.Sleep(sleepTime);

                        // Issue demand scan command
                        SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                        System.Threading.Thread.Sleep(sleepTime);

                        // Get latest well status to check if well is started
                        afterWellGroupStatusdata = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(well.Id.ToString()).Value;

                        if (afterWellGroupStatusdata.AlarmStatus != null)
                        {
                            if (afterWellGroupStatusdata.AlarmStatus.Contains("\"FAILED:"))
                            {
                                Trace.WriteLine($"{afterWellGroupStatusdata.WellName} is in Comm Failure.");
                                continue;
                            }
                        }
                        string startStatus = "";
                        if (well.FacilityId.StartsWith("AEPOC_") || well.FacilityId.StartsWith("SAM_"))
                            startStatus = "PocR";
                        else
                            startStatus = "POC";
                        i = 0;
                        while (afterWellGroupStatusdata.POCMode != startStatus && i < 30)
                        {
                            // Wait for some time for UIS to write DDS tx
                            System.Threading.Thread.Sleep(2000);
                            i++;
                            afterWellGroupStatusdata = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(well.Id.ToString()).Value;
                        }

                        Assert.AreEqual(startStatus, afterWellGroupStatusdata.POCMode, $"Failed to get '{startStatus}' status for '{afterWellGroupStatusdata.WellName}' even after {i * 500} msec.");
                        Trace.WriteLine($"Expected start status was fetched after {i * 500 / 1000} seconds.");
                    }
                }
            }
        }

        public void IssueCommandForOSingleWellLockout(string facility_tag, bool lockoutcheck, int sleepLockTime)
        {
            List<WellDTO> allWells = null;
            if (_addedWells != null && _addedWells.Count > 0)
            {
                allWells = _addedWells;
            }
            else
            {
                allWells = WellService.GetAllWells().ToList();
            }

            // Test Stop and Start command
            foreach (WellDTO well in allWells)
            {
                if (well.FacilityId != null)
                {
                    if (well.FacilityId.Contains(facility_tag))
                    {
                        // Issue demand scan command
                        CheckLockforCommand(well, WellCommand.DemandScan.ToString(), sleepLockTime);
                        // Issue Clear Error command
                        CheckLockforCommand(well, WellCommand.ClearErrors.ToString(), sleepLockTime);
                        // Issue Idle well command
                        CheckLockforCommand(well, WellCommand.IdleRPC.ToString(), sleepLockTime);
                        // Issue Stop well command
                        CheckLockforCommand(well, WellCommand.StopAndLeaveDown.ToString(), sleepLockTime);
                        // Issue start command
                        CheckLockforCommand(well, WellCommand.StartRPC.ToString(), sleepLockTime);
                        // Issue Run Continous  command
                        if (facility_tag.Contains("SAM"))
                        {
                            CheckLockforCommand(well, WellCommand.ContinuousRun.ToString(), sleepLockTime);
                        }
                        // Software Timer
                        CheckLockforCommand(well, WellCommand.SoftwareTimer.ToString(), sleepLockTime);
                        // Control Transfer
                        if (facility_tag.Contains("RPOC"))
                        {
                            CheckLockforCommand(well, WellCommand.ControlTransfer.ToString(), sleepLockTime);
                        }
                    }
                }
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void PGLWellGroupStatus()
        {
            try
            {
                //Add New Plunger Lift Well without Well model for 2 Wells
                //Assumption Padding of Facility on Seirra is 4 digit (D4)
                for (int i = 0; i < 2; i++)
                {
                    string pFacilityId = GetFacilityId("PGLWELL_", (i + 1));
                    AddWell_NonRRL(pFacilityId, WellTypeId.PLift);
                }
                GetPGLWellGroupStatus_NONRRL(WellTypeId.PLift, 2);

                Trace.WriteLine("Plunger Lift Well Group Stauts Check is complete");
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error in Method: [PGLWellGroupStatus]" + e.ToString());
                Assert.Fail("PGLWelGroup Status Service Test Failed");
            }
        }

        public void CheckLockforCommand(WellDTO well, string cmd, int lockdelaytime)
        {
            System.Threading.Thread.Sleep(lockdelaytime);
            // Issue  command
            bool cmdstatus = SurveillanceService.IssueCommandForSingleWell(well.Id, cmd);
            System.Threading.Thread.Sleep(100);
            Assert.IsTrue(cmdstatus, "First time " + cmd + " Command is Locked out");
            System.Threading.Thread.Sleep(2000);
            try
            {
                cmdstatus = SurveillanceService.IssueCommandForSingleWell(well.Id, cmd);
                System.Threading.Thread.Sleep(100);
            }
            catch (Exception e)
            {
                cmdstatus = false;
                Trace.WriteLine("Error after issuing first command due to Lock Timeout " + e);
            }
            Assert.IsFalse(cmdstatus, "Second  time " + cmd + " is NOT Locked out");
            System.Threading.Thread.Sleep(lockdelaytime);
            // Issue Command Again to see it is enabled after lockout delay
            cmdstatus = SurveillanceService.IssueCommandForSingleWell(well.Id, cmd);
            System.Threading.Thread.Sleep(2000);
            Assert.IsTrue(cmdstatus, "User Cannot issue " + cmd + " after completing specicied lockout");
        }

        public void GetDeviceTypeForWell(string facility_tag, string expectedValue)
        {
            List<WellDTO> allwells = null;
            if (_addedWells != null && _addedWells.Count > 0)
            {
                allwells = _addedWells;
            }
            else
            {
                allwells = WellService.GetAllWells().ToList();
            }
            foreach (WellDTO well in allwells)
            {
                if (well.FacilityId != null)
                {
                    if (well.FacilityId.Contains(facility_tag))
                    {
                        string deviceType = SurveillanceService.GetDeviceTypeForWell(well.Id.ToString());
                        Assert.AreEqual(expectedValue, deviceType, $"Device type for well with facility tag {facility_tag} has unexpected value.");
                    }
                }
            }
            //Invalid well
            if (allwells.Any())
            {
                long id = allwells.OrderBy(w => w.Id).LastOrDefault().Id;
                string deviceType = SurveillanceService.GetDeviceTypeForWell((id + 1).ToString());
                Assert.AreEqual("", deviceType, "Device type for invalid well should be empty.");
            }
        }

        public void GetCommunicationStatusForWell(string facility_tag)
        {
            List<WellDTO> allwells = null;
            if (_addedWells != null && _addedWells.Count > 0)
            {
                allwells = _addedWells;
            }
            else
            {
                allwells = WellService.GetAllWells().ToList();
            }
            List<WellDTO> wellsData = new List<WellDTO>();
            foreach (WellDTO well in allwells)
            {
                if (well.FacilityId != null)
                {
                    if (well.FacilityId.Contains(facility_tag))
                    {
                        RRLWellStatusValueDTO wellGroupStatusdata = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(well.Id.ToString()).Value;
                        if (wellGroupStatusdata.RunTimeYesterday != null)
                        {
                            wellsData.Add(well);
                        }
                    }
                }
            }
            foreach (WellDTO well in wellsData)
            {
                if (well.FacilityId != null)
                {
                    if (well.FacilityId.Contains(facility_tag))
                    {
                        RRLWellStatusValueDTO welldata = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(well.Id.ToString()).Value;
                        string commStatus = SurveillanceService.GetCommunicationStatusForWell(well.Id.ToString());
                        if (welldata.AlarmStatus != null)
                        {
                            if (welldata.AlarmStatus.Contains("\"FAILED:"))
                                Assert.IsTrue(commStatus.Contains("FAILED:"));
                        }
                        else
                        {
                            Assert.AreEqual("OK", commStatus, well.Name);
                        }
                    }
                }
            }
            //Invalid well
            if (WellService.GetAllWells().Any())
            {
                long id = WellService.GetAllWells().OrderBy(w => w.Id).LastOrDefault().Id;
                string Status = SurveillanceService.GetCommunicationStatusForWell((id + 1).ToString());
                Assert.IsNull(Status);
            }
        }

        public void GetLastSuccessfulTxForWell(string facility_tag)
        {
            List<WellDTO> allwells = null;
            if (_addedWells != null && _addedWells.Count > 0)
            {
                allwells = _addedWells;
            }
            else
            {
                allwells = WellService.GetAllWells().ToList();
            }
            List<WellDTO> wellsData = new List<WellDTO>();
            foreach (WellDTO well in allwells)
            {
                if (well.FacilityId != null)
                {
                    if (well.FacilityId.Contains(facility_tag))
                    {
                        RRLWellStatusValueDTO wellGroupStatusdata = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(well.Id.ToString()).Value;
                        if (wellGroupStatusdata.RunTimeYesterday != null)
                        {
                            wellsData.Add(well);
                        }
                    }
                }
            }
            foreach (WellDTO well in wellsData)
            {
                if (well.FacilityId != null)
                {
                    if (well.FacilityId.Contains(facility_tag))
                    {
                        string lastSuccessfulScantime = SurveillanceService.GetLastSuccessfulTxForWell(well.Id.ToString());
                        if (lastSuccessfulScantime == null)
                        {
                            const int sleepyTime = 2;
                            Trace.WriteLine(string.Format("Failed to get last successful scan time for well {0} with facility id {1}.  Sleeping for {2} seconds.", well.Name, well.FacilityId, sleepyTime));
                            Wait(sleepyTime);
                            lastSuccessfulScantime = SurveillanceService.GetLastSuccessfulTxForWell(well.Id.ToString());
                        }
                        Assert.AreNotEqual(null, lastSuccessfulScantime, "Failed to get last successful time for well {0} with facility id {1}.", well.Name, well.FacilityId);
                        Assert.AreNotEqual("", lastSuccessfulScantime, "Last successful time for well {0} with facility id {1} should not be empty.", well.Name, well.FacilityId);
                    }
                }
            }
            //Invalid well
            if (allwells.Any())
            {
                long id = allwells.OrderBy(x => x.Id).LastOrDefault().Id;
                string successfulScan = SurveillanceService.GetLastSuccessfulTxForWell((id + 1).ToString());
                Assert.IsNull(successfulScan);
            }
        }

        public void IssueCommandForMultipleWells(string facility_tag)
        {
            List<WellDTO> allWells = null;
            if (_addedWells != null && _addedWells.Count > 0)
            {
                allWells = _addedWells;
            }
            else
            {
                allWells = WellService.GetAllWells().ToList();
            }
            var wellIds = new List<long>();
            foreach (WellDTO well in allWells)
            {
                if (well.FacilityId != null && well.FacilityId.Contains(facility_tag))
                {
                    wellIds.Add(well.Id);
                }
            }
            long[] ids = wellIds.ToArray();
            SurveillanceService.IssueCommandForWells(ids, WellCommand.DemandScan.ToString(), "true");
            var newGroup = new WellGroupStatusQueryDTO();
            newGroup.WellType = WellTypeId.RRL;

            //Well Column Filter
            List<string> columnName = new List<string>();
            List<string> filterConditions = new List<string>();
            List<string> filterDataTypes = new List<string>();
            List<string> filterValues = new List<string>();
            WellGroupStatusColumnFilterDTO columnFilter = new WellGroupStatusColumnFilterDTO();
            columnFilter.ColumnNames = columnName.ToArray();
            columnFilter.FilterConditions = filterConditions.ToArray();
            columnFilter.FilterDataTypes = filterDataTypes.ToArray();
            columnFilter.FilterValues = filterValues.ToArray();

            //Get well Filters
            WellFilterDTO filters = WellService.GetWellFilter(null);

            //No Filters Selected
            WellFilterDTO wellbyFilters = new WellFilterDTO();
            newGroup.WellFilter = wellbyFilters;
            newGroup.wellGroupStatusColumnFilter = columnFilter;

            //Demand Scan
            WellGroupStatusDTO<RRLWellStatusUnitDTO, RRLWellStatusValueDTO> beforeScan = SurveillanceServiceClient.GetWellGroupStatus<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(newGroup);
            string pocStatus = "";
            foreach (RRLWellStatusValueDTO wellStatus in beforeScan.Status)
            {
                pocStatus = pocStatus + wellStatus.POCMode + ",";
            }
            SurveillanceService.IssueCommandForWells(ids, WellCommand.DemandScan.ToString(), "true");
            WellGroupStatusDTO<RRLWellStatusUnitDTO, RRLWellStatusValueDTO> afterScan = SurveillanceServiceClient.GetWellGroupStatus<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(newGroup);
            string afterpocStatus = "";
            foreach (RRLWellStatusValueDTO wellStatus in afterScan.Status)
            {
                afterpocStatus = afterpocStatus + wellStatus.POCMode + ",";
                //Check If this is a MQTT device should return false as non of them are MQTT
                bool mqttcheck = SurveillanceService.CheckDeviceTypeIsMQTTForWell(wellStatus.WellId.ToString());
                Assert.IsFalse(mqttcheck, "Unexpected DeviceType MQTT!");

            }
            Assert.AreEqual(pocStatus, afterpocStatus);
            //Stop and Leave Down
            SurveillanceService.IssueCommandForWells(ids, WellCommand.StopAndLeaveDown.ToString(), "true");
            //StartRPC
            SurveillanceService.IssueCommandForWells(ids, WellCommand.StartRPC.ToString(), "true");
            //Clear Errors
            SurveillanceService.IssueCommandForWells(ids, WellCommand.ClearErrors.ToString(), "true");
        }

        private void TestOneUDCTrend(List<WellDTO> wells, WellQuantity quantity, string udc)
        {
            int count = 0;
            int count1 = 0;
            int count2 = 0;
            // Enter All the Start and End Dates We want to Enter
            string days31dago = DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-31));
            string days29dago = DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-29));
            string daystodaySt = DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime());
            string daystodayEnd = DTOExtensions.ToISO8601(DateTime.Today.AddDays(1).ToUniversalTime());
            string days1dagoSt = DTOExtensions.ToISO8601(DateTime.Today.AddDays(-1).ToUniversalTime());
            string days1dagoEnd = DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime());
            string days2dagoSt = DTOExtensions.ToISO8601(DateTime.Today.AddDays(-2).ToUniversalTime());
            string days2dagoEnd = DTOExtensions.ToISO8601(DateTime.Today.AddDays(-1).ToUniversalTime());
            int udcTagIdNumber = (int)quantity;
            string[] udcTagIdNumberStr = { udcTagIdNumber.ToString() };

            foreach (WellDTO well in wells)
            {
                SurveillanceService.IssueCommandForSingleWell(well.Id, "DemandScan");

                Trace.WriteLine($"Getting trends for Start Time {days31dago} and End time {daystodayEnd}");
                CygNetTrendDTO avgWellTrend = SurveillanceService.GetCygNetTrendsByUDCAndDateRange(udcTagIdNumberStr, well.Id.ToString(), days31dago, daystodayEnd).FirstOrDefault();
                if (avgWellTrend.PointUDC != null)
                {
                    Assert.AreEqual(udc, avgWellTrend.PointUDC, "UDCs do not match for average trend.");
                }
                Trace.WriteLine($"Getting trends for Start Time {days29dago} and End time {daystodayEnd}");
                CygNetTrendDTO wellTrend = SurveillanceService.GetCygNetTrendsByUDCAndDateRange(udcTagIdNumberStr, well.Id.ToString(), days29dago, daystodayEnd).FirstOrDefault();
                if (wellTrend.PointUDC != null)
                {
                    Assert.AreEqual(udc, wellTrend.PointUDC, "UDCs do not match for well trend.");
                }
                if (wellTrend.ErrorMessage == "Success" && avgWellTrend.ErrorMessage == "Success")
                {
                    Trace.WriteLine($"Getting trends for  Today only Start Time {daystodaySt} and End time {daystodayEnd}");
                    CygNetTrendDTO todayWellTrend = SurveillanceService.GetCygNetTrendsByUDCAndDateRange(udcTagIdNumberStr, well.Id.ToString(), daystodaySt, daystodayEnd).FirstOrDefault();
                    if (todayWellTrend.ErrorMessage == "Success")
                    {
                        for (int i = 0; i < todayWellTrend.PointValues.Length; i++)
                        {
                            if (wellTrend.PointValues[i].Timestamp.ToLocalTime().Date == DateTime.Today)
                            {
                                count = count + 1;
                            }
                        }
                        //Assert.AreEqual(count, todayWellTrend.PointValues.Length, "Value count for today from larger sample does not match value count retrieved for today for UDC {0}.", udc);
                    }

                    Trace.WriteLine($"Getting trends for Yesterday Only Start Time {days1dagoSt} and End time {days1dagoEnd}");
                    CygNetTrendDTO yesterdayWellTrend = SurveillanceService.GetCygNetTrendsByUDCAndDateRange(udcTagIdNumberStr, well.Id.ToString(), days1dagoSt, days1dagoEnd).FirstOrDefault();


                    if (yesterdayWellTrend.ErrorMessage == "Success")
                    {
                        for (int i = 0; i < yesterdayWellTrend.PointValues.Length; i++)
                        {
                            if (todayWellTrend.ErrorMessage == "Success")
                            {
                                if (wellTrend.PointValues[i].Timestamp.AddDays(-1).Date == DateTime.UtcNow.AddDays(-1).Date)
                                {
                                    count1 = count1 + 1;
                                }
                            }
                            else
                            {
                                if (wellTrend.PointValues[i].Timestamp.Date == DateTime.UtcNow.AddDays(-1).Date)
                                {
                                    count1 = count1 + 1;
                                }
                            }
                        }
                    }

                    Trace.WriteLine($"Getting trends for beforeYestrday  Start Time {days2dagoSt} and End time {days2dagoEnd}");
                    CygNetTrendDTO beforeyesterdayWellTrend = SurveillanceService.GetCygNetTrendsByUDCAndDateRange(udcTagIdNumberStr, well.Id.ToString(), days2dagoSt, days2dagoEnd).FirstOrDefault();
                    if (beforeyesterdayWellTrend.ErrorMessage == "Success")
                    {
                        for (int i = 0; i < beforeyesterdayWellTrend.PointValues.Length; i++)
                        {
                            if (todayWellTrend.ErrorMessage == "Success")
                            {
                                if (wellTrend.PointValues[i].Timestamp.AddDays(-2).Date == DateTime.UtcNow.AddDays(-2).Date)
                                {
                                    count2 = count2 + 1;
                                }
                            }
                            else
                            {
                                if (wellTrend.PointValues[i].Timestamp.AddDays(-1).Date == DateTime.UtcNow.AddDays(-2).Date)
                                {
                                    count2 = count2 + 1;
                                }
                            }
                        }
                    }
                }
            }
        }

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            SettingType settingType = SettingType.System;
            SettingDTO bypassSPV = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Description == SettingServiceStringConstants.BYPASS_SURFACE_PARAMETER_VALIDATION);
            SystemSettingDTO systemSettingSPV = SettingService.GetSystemSettingByName(bypassSPV.Name);
            systemSettingSPV.NumericValue = 1;
            SettingService.SaveSystemSetting(systemSettingSPV);
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
        }

        [TestCleanup]
        public override void Cleanup()
        {
            SettingType settingType = SettingType.System;
            SettingDTO bypassSPV = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Description == SettingServiceStringConstants.BYPASS_SURFACE_PARAMETER_VALIDATION);
            SystemSettingDTO systemSettingSPV = SettingService.GetSystemSettingByName(bypassSPV.Name);
            systemSettingSPV.NumericValue = 1;
            SettingService.SaveSystemSetting(systemSettingSPV);
            base.Cleanup();
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellGroupStatus_WP()
        {
            AddWells("RPOC_");
            GetWellGroupStatus("RPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellGroupStatus_LufkinSAM()
        {
            AddWells("SAM_");
            GetWellGroupStatus("SAM_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellGroupStatus_8800()
        {
            AddWells("8800_");
            GetWellGroupStatus("8800_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellGroupStatus_AEPOC()
        {
            AddWells("AEPOC_");
            GetWellGroupStatus("AEPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellGroupStatus_EPICFS()
        {
            AddWells("EPICFS_");
            GetWellGroupStatus("EPICFS_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellStatusData_WP()
        {
            AddWells("RPOC_");
            GetWellStatusData("RPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellStatusData_LufkinSAM()
        {
            AddWells("SAM_");
            GetWellStatusData("SAM_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellStatusData_WP_MLD()
        {
            // TODOQA: Enable in ATS once it's configured.
            if (LogNotConfiguredIfRunningInATS())
            {
                return;
            }
            ConfigureForMultipleLinkedDevices();
            AddSingleWell("RPOC_MLD");
            GetWellStatusData("RPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellStatusData_LufkinSAM_MLD()
        {
            // TODOQA: Enable in ATS once it's configured.
            if (LogNotConfiguredIfRunningInATS())
            {
                return;
            }
            ConfigureForMultipleLinkedDevices();
            AddSingleWell("SAM_MLD");
            GetWellStatusData("SAM_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellStatusData_8800()
        {
            AddWells("8800_");
            GetWellStatusData("8800_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellStatusData_AEPOC()
        {
            AddWells("AEPOC_");
            GetWellStatusData("AEPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellStatusData_EPICFS()
        {
            AddWells("EPICFS_");
            GetWellStatusData("EPICFS_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForSingleWell_WP()
        {
            AddWells("RPOC_");
            IssueCommandForOneWell("RPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForSingleWell_LufkinSAM()
        {
            AddWells("SAM_");
            IssueCommandForOneWell("SAM_", 5000);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForSingleWell_WP_MLD()
        {
            // TODOQA: Enable in ATS once it's configured.
            if (LogNotConfiguredIfRunningInATS())
            {
                return;
            }
            ConfigureForMultipleLinkedDevices();
            AddSingleWell("RPOC_MLD");
            IssueCommandForOneWell("RPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForSingleWell_LufkinSAM_MLD()
        {
            // TODOQA: Enable in ATS once it's configured.
            if (LogNotConfiguredIfRunningInATS())
            {
                return;
            }
            ConfigureForMultipleLinkedDevices();
            AddSingleWell("SAM_MLD");
            IssueCommandForOneWell("SAM_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForSingleWell_8800()
        {
            AddWells("8800_");
            IssueCommandForOneWell("8800_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForSingleWell_AEPOC()
        {
            AddWells("AEPOC_");
            IssueCommandForOneWell("AEPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForSingleWell_EPICFS()
        {
            AddWells("EPICFS_");
            IssueCommandForOneWell("EPICFS_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetDeviceTypeForWell_WP()
        {
            AddWells("RPOC_", false);
            GetDeviceTypeForWell("RPOC_", "WellPilotRPOC");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetDeviceTypeForWell_LufkinSAM()
        {
            AddWells("SAM_", false);
            GetDeviceTypeForWell("SAM_", "LufkinSam");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetDeviceTypeForWell_8800()
        {
            AddWells("8800_", false);
            GetDeviceTypeForWell("8800_", "EProd8800");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetDeviceTypeForWell_AEPOC()
        {
            AddWells("AEPOC_", false);
            GetDeviceTypeForWell("AEPOC_", "AEBEAM");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetDeviceTypeForWell_EPICFS()
        {
            AddWells("EPICFS_", false);
            GetDeviceTypeForWell("EPICFS_", "ePICRPOC");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetCommunicationStatusForWell_WP()
        {
            AddWells("RPOC_");
            GetCommunicationStatusForWell("RPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetCommunicationStatusForWell_LufkinSAM()
        {
            AddWells("SAM_");
            GetCommunicationStatusForWell("SAM_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetCommunicationStatusForWell_8800()
        {
            AddWells("8800_");
            GetCommunicationStatusForWell("8800_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetCommunicationStatusForWell_AEPOC()
        {
            AddWells("AEPOC_");
            GetCommunicationStatusForWell("AEPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetCommunicationStatusForWell_EPICFS()
        {
            AddWells("EPICFS_");
            GetCommunicationStatusForWell("EPICFS_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetLastSuccessfulTxForWell_WP()
        {
            AddWells("RPOC_");
            GetLastSuccessfulTxForWell("RPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetLastSuccessfulTxForWell_LufkinSAM()
        {
            AddWells("SAM_");
            GetLastSuccessfulTxForWell("SAM_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetLastSuccessfulTxForWell_8800()
        {
            AddWells("8800_");
            GetLastSuccessfulTxForWell("8800_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetLastSuccessfulTxForWell_AEPOC()
        {
            AddWells("AEPOC_");
            GetLastSuccessfulTxForWell("AEPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetLastSuccessfulTxForWell_EPICFS()
        {
            AddWells("EPICFS_");
            GetLastSuccessfulTxForWell("EPICFS_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForMultipleWells_WP()
        {
            AddWells("RPOC_");
            IssueCommandForMultipleWells("RPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForMultipleWells_LufkinSAM()
        {
            AddWells("SAM_");
            IssueCommandForMultipleWells("SAM_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForMultipleWells_8800()
        {
            AddWells("8800_");
            IssueCommandForMultipleWells("8800_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForMultipleWells_AEPOC()
        {
            AddWells("AEPOC_");
            IssueCommandForMultipleWells("AEPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForMultipleWells_EPICFS()
        {
            AddWells("EPICFS_");
            IssueCommandForMultipleWells("EPICFS_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellTrends_WP()
        {
            AddWell("RPOC_0001", "RPOC_");
            if (_addedWells != null && _addedWells.Count > 0)
            {
                Trace.WriteLine("Wells: " + string.Join(", ", _addedWells.Select(w => w.Name)));
            }
            else
            {
                Trace.WriteLine("No wells found.");
            }
            TestOneUDCTrend(_addedWells, WellQuantity.CardArea, "RRLCRDAREA");
            TestOneUDCTrend(_addedWells, WellQuantity.AveragePumpFillage, "RRLPFILAVG");
            TestOneUDCTrend(_addedWells, WellQuantity.AverageSPM, "RRLSPMAVG");
            TestOneUDCTrend(_addedWells, WellQuantity.AverageCycleTime, "RRLCYCTAVG");
            TestOneUDCTrend(_addedWells, WellQuantity.CurrentSPM, "SPSTRN");
            TestOneUDCTrend(_addedWells, WellQuantity.CyclesToday, "RRLCYCTDY");
            TestOneUDCTrend(_addedWells, WellQuantity.CyclesYesterday, "RRLCYCYEST");
            TestOneUDCTrend(_addedWells, WellQuantity.DailyMaximumLoad, "RRLMXLDDLY");
            TestOneUDCTrend(_addedWells, WellQuantity.DailyMinimumLoad, "RRLMNLDDLY");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellTrends_LufkinSAM()
        {
            AddWell("SAM_0001", "SAM_");
            if (_addedWells != null && _addedWells.Count > 0)
            {
                Trace.WriteLine("Wells: " + string.Join(", ", _addedWells.Select(w => w.Name)));
            }
            else
            {
                Trace.WriteLine("No wells found.");
            }
            TestOneUDCTrend(_addedWells, WellQuantity.CardArea, "RRLCRDAREA");
            TestOneUDCTrend(_addedWells, WellQuantity.AveragePumpFillage, "RRLPFILAVG");
            TestOneUDCTrend(_addedWells, WellQuantity.AverageSPM, "RRLSPMAVG");
            TestOneUDCTrend(_addedWells, WellQuantity.AverageCycleTime, "RRLCYCTAVG");
            TestOneUDCTrend(_addedWells, WellQuantity.CurrentSPM, "SPSTRN");
            TestOneUDCTrend(_addedWells, WellQuantity.CyclesToday, "RRLCYCTDY");
            TestOneUDCTrend(_addedWells, WellQuantity.CyclesYesterday, "RRLCYCYEST");
            TestOneUDCTrend(_addedWells, WellQuantity.DailyMaximumLoad, "RRLMXLDDLY");
            TestOneUDCTrend(_addedWells, WellQuantity.DailyMinimumLoad, "RRLMNLDDLY");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellTrends_8800()
        {
            AddWell("8800_0001", "8800_");
            if (_addedWells != null && _addedWells.Count > 0)
            {
                Trace.WriteLine("Wells: " + string.Join(", ", _addedWells.Select(w => w.Name)));
            }
            else
            {
                Trace.WriteLine("No wells found.");
            }
            TestOneUDCTrend(_addedWells, WellQuantity.CardArea, "RRLCRDAREA");
            TestOneUDCTrend(_addedWells, WellQuantity.AverageSPM, "RRLSPMAVG");
            TestOneUDCTrend(_addedWells, WellQuantity.AverageCycleTime, "RRLCYCTAVG");
            TestOneUDCTrend(_addedWells, WellQuantity.CurrentSPM, "SPSTRN");
            TestOneUDCTrend(_addedWells, WellQuantity.CyclesToday, "RRLCYCTDY");
            TestOneUDCTrend(_addedWells, WellQuantity.CyclesYesterday, "RRLCYCYEST");
            TestOneUDCTrend(_addedWells, WellQuantity.DailyMaximumLoad, "RRLMXLDDLY");
            TestOneUDCTrend(_addedWells, WellQuantity.DailyMinimumLoad, "RRLMNLDDLY");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellTrends_AEPOC()
        {
            AddWell("AEPOC_0001", "AEPOC_");
            if (_addedWells != null && _addedWells.Count > 0)
            {
                Trace.WriteLine("Wells: " + string.Join(", ", _addedWells.Select(w => w.Name)));
            }
            else
            {
                Trace.WriteLine("No wells found.");
            }
            TestOneUDCTrend(_addedWells, WellQuantity.CardArea, "RRLCRDAREA");
            TestOneUDCTrend(_addedWells, WellQuantity.AverageSPM, "RRLSPMAVG");
            TestOneUDCTrend(_addedWells, WellQuantity.AverageCycleTime, "RRLCYCTAVG");
            TestOneUDCTrend(_addedWells, WellQuantity.CurrentSPM, "SPSTRN");
            TestOneUDCTrend(_addedWells, WellQuantity.CyclesToday, "RRLCYCTDY");
            TestOneUDCTrend(_addedWells, WellQuantity.CyclesYesterday, "RRLCYCYEST");
            TestOneUDCTrend(_addedWells, WellQuantity.DailyMaximumLoad, "RRLMXLDDLY");
            TestOneUDCTrend(_addedWells, WellQuantity.DailyMinimumLoad, "RRLMNLDDLY");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellTrends_EPICFS()
        {
            AddWell("EPICFS_0001", "EPICFS_");
            if (_addedWells != null && _addedWells.Count > 0)
            {
                Trace.WriteLine("Wells: " + string.Join(", ", _addedWells.Select(w => w.Name)));
            }
            else
            {
                Trace.WriteLine("No wells found.");
            }
            TestOneUDCTrend(_addedWells, WellQuantity.CardArea, "RRLCRDAREA");
            TestOneUDCTrend(_addedWells, WellQuantity.AverageSPM, "RRLSPMAVG");
            TestOneUDCTrend(_addedWells, WellQuantity.AverageCycleTime, "RRLCYCTAVG");
            TestOneUDCTrend(_addedWells, WellQuantity.CurrentSPM, "SPSTRN");
            TestOneUDCTrend(_addedWells, WellQuantity.CyclesToday, "RRLCYCTDY");
            TestOneUDCTrend(_addedWells, WellQuantity.CyclesYesterday, "RRLCYCYEST");
            TestOneUDCTrend(_addedWells, WellQuantity.DailyMaximumLoad, "RRLMXLDDLY");
            TestOneUDCTrend(_addedWells, WellQuantity.DailyMinimumLoad, "RRLMNLDDLY");
        }

        public RRLWellStatusValueDTO AddWellAndGetDemandScanData(string facility_tag)
        {
            string facilityId = GetFacilityId(facility_tag, 1);

            WellConfigurationService.AddWellConfig(new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = DefaultWellName,
                    FacilityId = facilityId,
                    DataConnection = GetDefaultCygNetDataConnection(),
                    IntervalAPI = "IntervalAPI",
                    SubAssemblyAPI = "SubAssemblyAPI",
                    AssemblyAPI = "AssemblyAPI",
                    CommissionDate = DateTime.Today.AddYears(-2),
                    WellType = WellTypeId.RRL,
                })
            });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            SurveillanceService.IssueCommandForSingleWell(well.Id, "DemandScan");

            // Wait for a second to make sure that Demand Scan transaction data is available in DDS
            // after UIS command is issued
            System.Threading.Thread.Sleep(1000);
            RRLWellStatusValueDTO wellData = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(well.Id.ToString()).Value;
            return wellData;
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellKPIData_WP()
        {
            RRLWellStatusValueDTO wellStatus = AddWellAndGetDemandScanData("RPOC_");
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));

            WellCurrentKPIDTO currWellStatus = SurveillanceService.GetWellKPIData(well.Id.ToString());

            Assert.AreEqual(currWellStatus.CurrentSPM, wellStatus.CurrentSPM);
            Assert.AreEqual(currWellStatus.InferredProductionYesterday, wellStatus.InferredProductionYesterday);
            Assert.AreEqual(currWellStatus.CyclesToday, wellStatus.CyclesToday);
            Assert.AreEqual(currWellStatus.CyclesYesterday, wellStatus.CyclesYesterday);
            Assert.AreEqual(currWellStatus.InferredProduction, wellStatus.InferredProductionToday);
            Assert.AreEqual(currWellStatus.FailureConfidence30Days, wellStatus.WellFailureProb);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellKPIData_LufkinSAM()
        {
            RRLWellStatusValueDTO wellStatus = AddWellAndGetDemandScanData("SAM_");
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));

            WellCurrentKPIDTO currWellStatus = SurveillanceService.GetWellKPIData(well.Id.ToString());

            Assert.AreEqual(currWellStatus.CurrentSPM, wellStatus.CurrentSPM);
            Assert.AreEqual(currWellStatus.InferredProductionYesterday, wellStatus.InferredProductionYesterday);
            Assert.AreEqual(currWellStatus.CyclesToday, wellStatus.CyclesToday);
            Assert.AreEqual(currWellStatus.CyclesYesterday, wellStatus.CyclesYesterday);
            Assert.AreEqual(currWellStatus.InferredProduction, wellStatus.InferredProductionToday);
            Assert.AreEqual(currWellStatus.FailureConfidence30Days, wellStatus.WellFailureProb);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellKPIData_8800()
        {
            RRLWellStatusValueDTO wellStatus = AddWellAndGetDemandScanData("8800_");
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));

            WellCurrentKPIDTO currWellStatus = SurveillanceService.GetWellKPIData(well.Id.ToString());

            Assert.AreEqual(currWellStatus.CurrentSPM, wellStatus.CurrentSPM);
            Assert.AreEqual(currWellStatus.InferredProductionYesterday, wellStatus.InferredProductionYesterday);
            Assert.AreEqual(currWellStatus.CyclesToday, wellStatus.CyclesToday);
            Assert.AreEqual(currWellStatus.CyclesYesterday, wellStatus.CyclesYesterday);
            Assert.AreEqual(currWellStatus.InferredProduction, wellStatus.InferredProductionToday);
            Assert.AreEqual(currWellStatus.FailureConfidence30Days, wellStatus.WellFailureProb);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellKPIData_AEPOC()
        {
            RRLWellStatusValueDTO wellStatus = AddWellAndGetDemandScanData("AEPOC_");
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));

            WellCurrentKPIDTO currWellStatus = SurveillanceService.GetWellKPIData(well.Id.ToString());

            Assert.AreEqual(currWellStatus.CurrentSPM, wellStatus.CurrentSPM);
            Assert.AreEqual(currWellStatus.InferredProductionYesterday, wellStatus.InferredProductionYesterday);
            Assert.AreEqual(currWellStatus.CyclesToday, wellStatus.CyclesToday);
            Assert.AreEqual(currWellStatus.CyclesYesterday, wellStatus.CyclesYesterday);
            Assert.AreEqual(currWellStatus.InferredProduction, wellStatus.InferredProductionToday);
            Assert.AreEqual(currWellStatus.FailureConfidence30Days, wellStatus.WellFailureProb);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellKPIData_EPICFS()
        {
            RRLWellStatusValueDTO wellStatus = AddWellAndGetDemandScanData("EPICFS_");
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));

            WellCurrentKPIDTO currWellStatus = SurveillanceService.GetWellKPIData(well.Id.ToString());

            Assert.AreEqual(currWellStatus.CurrentSPM, wellStatus.CurrentSPM);
            Assert.AreEqual(currWellStatus.InferredProductionYesterday, wellStatus.InferredProductionYesterday);
            Assert.AreEqual(currWellStatus.CyclesToday, wellStatus.CyclesToday);
            Assert.AreEqual(currWellStatus.CyclesYesterday, wellStatus.CyclesYesterday);
            Assert.AreEqual(currWellStatus.InferredProduction, wellStatus.InferredProductionToday);
            Assert.AreEqual(currWellStatus.FailureConfidence30Days, wellStatus.WellFailureProb);
        }

        public void GetAllWellGauge(string facitiTag)
        {
            RRLWellStatusValueDTO wellStatus = AddWellAndGetDemandScanData(facitiTag);
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));

            //add new wellTestData
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDTO testDataDTO = new WellTestDTO
            {
                WellId = well.Id,
                AverageCasingPressure = 1,
                AverageFluidAbovePump = 1,
                AverageTubingPressure = 90,
                AverageTubingTemperature = 100,
                Gas = 0,
                GasGravity = 0,
                Oil = 75,
                OilGravity = 0,
                PumpEfficiency = 0,
                PumpIntakePressure = 100,
                PumpingHours = 10,
                //SPTCode = 1,
                SPTCodeDescription = WellTestQuality.RepresentativeTest.ToString(),
                StrokePerMinute = 0,
                TestDuration = 3,
                Water = 99,
                WaterGravity = 0
            };
            testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(100));
            WellTestDataService.AddWellTestData(new WellTestAndUnitsDTO(units, testDataDTO));

            //get the added well test data
            WellTestDTO[] TestDataArray = WellTestDataService.GetWellTestDataByWellId(well.Id.ToString()).Values;
            WellTestDTO testDataDTOCheck = TestDataArray.FirstOrDefault(a => a.WellId == well.Id);
            WellGaugeDTO wellWaterCut = SurveillanceService.GetWaterCutByWell(well.Id.ToString());
            //Assert.AreEqual((int)Math.Round((double)testDataDTO.Water * 100 / ((testDataDTO.Oil.Value ?? 0.0) + (double)testDataDTO.Water)), wellWaterCut.CurrentVal, "Water cut current value is incorrect.");
            Assert.AreEqual((int)Math.Round((double)testDataDTO.Water * 100 / ((double)testDataDTO.Oil + (double)testDataDTO.Water)), wellWaterCut.CurrentVal, "Water cut current value is incorrect.");

            //this code is commented because of FRI-3389 changes and we have to cover this senario into the UI automation script
            try
            {
                WellGaugeDTO PumpFillageByWell = SurveillanceService.GetPumpFillageByWell(well.Id.ToString()); //this API is obsolete : FRI-3389 changes will throw null refernce exception

                Assert.AreEqual("0", PumpFillageByWell.MinVal.ToString());
                Assert.AreEqual("100", PumpFillageByWell.MaxVal.ToString());
                if (well.FacilityId.Contains("RPOC_") || well.FacilityId.Contains("SAM_"))
                    Assert.AreEqual(PumpFillageByWell.CurrentVal.ToString(), wellStatus.CurrentPumpFillage.ToString());
            }
            catch (NullReferenceException nullrefex)
            {

                Trace.WriteLine($"Framework Error : API reference should have been removed... as API was no longer called by UI : errrr---> {nullrefex.ToString()}");
            }
            catch (System.Net.WebException webex)
            {
                Trace.WriteLine($"Framework Error : API reference should have been removed... as API was no longer called by UI : errrr---> {webex.ToString()}");
            }

            ////Get well EffectiveRunTime
            try
            {
                WellGaugeDTO wellEffectiveRunTime = SurveillanceService.GetEffectiveRunTimeByWell(well.Id.ToString());//this API is obsolete : FRI-3389 changes will throw null refernce exception
                Assert.AreEqual("0", wellEffectiveRunTime.MinVal.ToString());
                Assert.AreEqual("100", wellEffectiveRunTime.MaxVal.ToString());
                Assert.AreEqual((Math.Round((wellStatus.EffectiveRunTimePercentage ?? 0), 2)).ToString(), wellEffectiveRunTime.CurrentVal.ToString());
            }
            catch (NullReferenceException nullrefex)
            {

                Trace.WriteLine($"Framework Error : API reference should have been removed... as API was no longer called by UI : errrr---> {nullrefex.ToString()}");
            }
            catch (System.Net.WebException webex)
            {
                Trace.WriteLine($"Framework Error : API reference should have been removed... as API was no longer called by UI : errrr---> {webex.ToString()}");
            }

            ////Get well RunTime
            try
            {
                WellGaugeDTO wellRunTime = SurveillanceService.GetRunTimeByWell(well.Id.ToString()); //this API is obsolete : FRI-3389 changes will throw null refernce exception
                Assert.AreEqual("0", wellRunTime.MinVal.ToString());
                Assert.AreEqual("24", wellRunTime.MaxVal.ToString());
                double _Totaltime = wellStatus.RunTimeToday ?? 0.0;
                TimeSpan _Sumtime = TimeSpan.FromSeconds(_Totaltime);
                Assert.AreEqual((Math.Round((_Sumtime.TotalSeconds) / 3600, 1)).ToString(), wellRunTime.CurrentVal.ToString());
            }
            catch (NullReferenceException nullrefex)
            {

                Trace.WriteLine($"Framework Error : API reference should have been removed... as API was no longer called by UI : errrr---> {nullrefex.ToString()}");
            }
            catch (System.Net.WebException webex)
            {
                Trace.WriteLine($"Framework Error : API reference should have been removed... as API was no longer called by UI : errrr---> {webex.ToString()}");
            }

            WellTestDataService.DeleteWellTestData(testDataDTOCheck.Id.ToString());
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetAllWellGauge_WP()
        {
            GetAllWellGauge("RPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetAllWellGauge_LufkinSAM()
        {
            GetAllWellGauge("SAM_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetAllWellGauge_8800()
        {
            GetAllWellGauge("8800_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetAllWellGauge_AEPOC()
        {
            GetAllWellGauge("AEPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetAllWellGauge_EPICFS()
        {
            GetAllWellGauge("EPICFS_");
        }

        public void SetPointsConfigurationTest_SingleValue(string facilityId, string wellName, WellTypeId wellType, string dEId, string elementName, double firstValue, double alternateValue)
        {
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = wellName, FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = wellType }) });

            WellDTO[] allWells = WellService.GetAllWells();
            Assert.IsNotNull(allWells, "Failed to get wells.");
            WellDTO well = allWells.FirstOrDefault(w => w.Name.Equals(wellName));
            Assert.IsNotNull(well, "Failed to add well.");
            _wellsToRemove.Add(well);
            Trace.WriteLine($"Added new well id '{well.Id}' associated with facility id '{facilityId}'.");

            if (wellType == WellTypeId.ESP)
            {
                var wellStatusValue = SurveillanceServiceClient.GetWellStatusData<ESPWellStatusUnitDTO, ESPWellStatusValueDTO>(well.Id.ToString()).Value;
                if (wellStatusValue.AlarmMessageString != null && wellStatusValue.AlarmMessageString.Contains("FAILED:"))
                {
                    Trace.WriteLine("Well is in comm failure");
                    return;
                }
            }
            if (wellType == WellTypeId.GLift)
            {
                var wellStatusValue = SurveillanceServiceClient.GetWellStatusData<GLWellStatusUnitDTO, GLWellStatusValueDTO>(well.Id.ToString()).Value;
                if (wellStatusValue.AlarmMessageString != null && wellStatusValue.AlarmMessageString.Contains("FAILED:"))
                {
                    Trace.WriteLine("Well is in comm failure");
                    return;
                }
            }
            //AddSettingWithDefaultLanguageKeyAndDefaultValue(SettingServiceStringConstants.SET_POINT_CONFIG,
            //    SettingValueType.Text, SettingType.System, SettingCategory.None,
            //    "<DeviceTypes><DeviceType Name=\"WFordKS\"><SetPointDG IsPrimary=\"true\" Type=\"GLCtlParms\" UISGetCommandName=\"GETSPDATA\" UISSendCommandName=\"C_SETPOINT\"><ShownElement DEId=\"RmpTgtInjR\" /></SetPointDG></DeviceType></DeviceTypes>",
            //    null, UnitCategory.None, null);

            SystemSettingDTO spSettingDto = SettingService.GetSystemSettingByName(SettingServiceStringConstants.SET_POINT_CONFIG);

            var setPointDgDTO = SurveillanceService.GetSetPointsDataGroups(well.Id.ToString());

            Assert.IsNotNull(setPointDgDTO, "Invalid SetPointsDGDTO object.");
            Assert.IsNotNull(setPointDgDTO.PrimarySPDgType, "Invalid primary set point data group type.");

            // Get the latest set point data
            SetPointsDTO originalSetPointsDTO = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), setPointDgDTO.PrimarySPDgType, "false");
            Assert.IsNotNull(originalSetPointsDTO, $"Failed to get the set point data for the first time from facility {facilityId}.");
            Assert.AreEqual("success", originalSetPointsDTO.ErrorMessage, $"Failed to get the set point data for the first time from facility {facilityId}.");

            SetPointDTO targetElement = originalSetPointsDTO.SetPointsList.FirstOrDefault(t => t.ElementId == dEId);
            Assert.IsNotNull(targetElement, $"Failed to get {elementName} set point.");
            Assert.AreEqual(false, targetElement.IsHidden, $"{elementName} should not be hidden.");
            List<SetPointDTO> otherVisibleSetPoints = originalSetPointsDTO.SetPointsList.Where(t => !t.IsHidden && t != targetElement).ToList();
            Assert.IsTrue(otherVisibleSetPoints.Count == 0, $"Found unexpected visible set point(s): ${string.Join(", ", otherVisibleSetPoints.Select(t => t.ElementId))}.");

            double currentValue = Convert.ToDouble(targetElement.Value);
            double newValue = currentValue == firstValue ? alternateValue : firstValue;
            targetElement.Value = newValue.ToString();
            originalSetPointsDTO.ErrorMessage = null;

            string status = SurveillanceService.SendSetPointsParamsForSingleWell(originalSetPointsDTO, "false");
            Assert.AreEqual("success", status, "Set point send failed.");

            SetPointsDTO updatedSetPointsDTO = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), setPointDgDTO.PrimarySPDgType, "false");
            Assert.AreEqual("success", updatedSetPointsDTO.ErrorMessage, $"Failed to get the set point data for the second time from facility {facilityId}");

            targetElement = originalSetPointsDTO.SetPointsList.FirstOrDefault(t => t.ElementId == dEId);
            Assert.IsNotNull(targetElement, $"Failed to get {elementName} set point.");
            Assert.AreEqual(newValue, Convert.ToDouble(targetElement.Value), $"{elementName} was not updated to expected value.");

            string savedstatus = SurveillanceService.SendSavedSetPointsParamsForSingleWell(originalSetPointsDTO);
            Assert.AreEqual("success", savedstatus, "Set point send failed.");

            SetPointsDTO savedSetPointsDTO = SurveillanceService.GetSavedSetPointsParamsForSingleWell(well.Id.ToString(), setPointDgDTO.PrimarySPDgType, "false");
            Assert.AreEqual("success", savedSetPointsDTO.ErrorMessage, $"Failed to get the set point data for the second time from facility {facilityId}");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void SetPointsConfigurationTest_GasLift()
        {
            string facilityId = "GLWELL_0001";
            string wellName = "GLWELL_0001";

            if (s_isRunningInATS)
                facilityId = GetGLFacilityId(1);

            SetPointsConfigurationTest_SingleValue(facilityId, wellName, WellTypeId.GLift, "RmpTgtInjR", "Target Rate", 1000, 1100);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void SetPointsConfigurationTest_ESP()
        {
            // TODOQA: Enable in ATS once it's configured.
            if (LogNotConfiguredIfRunningInATS())
            {
                return;
            }
            //    string facilityId = GetESPFacilityId(2);
            string facilityId = "ESP_WELL02";
            string wellName = "ESPTest02";
            SetPointsConfigurationTest_SingleValue(facilityId, wellName, WellTypeId.ESP, "VSDTgtFreq", "Target Frequency", 60.15, 65.72);
        }

        //[TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void SetPointsConfigurationTest_InjectionWells()
        {
            string wellName = "Well";
            string facilityId = "IWC_0001";
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = wellName, FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.WInj }) });

            WellDTO[] allWells = WellService.GetAllWells();
            Assert.IsNotNull(allWells, "Failed to get wells.");
            WellDTO well = allWells.FirstOrDefault(w => w.Name.Equals(wellName));
            Assert.IsNotNull(well, "Failed to add well.");
            _wellsToRemove.Add(well);
            Trace.WriteLine($"Added new well id '{well.Id}' associated with facility id '{facilityId}'.");

            var setPointDgDTO = SurveillanceService.GetSetPointsDataGroups(well.Id.ToString());
            Assert.IsNotNull(setPointDgDTO, "Invalid SetPointsDGDTO object.");
            Assert.IsNotNull(setPointDgDTO.PrimarySPDgType, "Invalid primary set point data group type.");

            // Get the latest set point data
            SetPointsDTO originalSetPointsDTO = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), setPointDgDTO.PrimarySPDgType, false.ToString());
            Assert.AreEqual("success", originalSetPointsDTO.ErrorMessage, $"Failed to get the set point data for the first time from facility {facilityId}");
            Assert.IsNotNull(originalSetPointsDTO.SetPointsList, $"Failed to get the set point data for the facility {facilityId}");
            Assert.IsTrue(originalSetPointsDTO.SetPointsList.Count > 0, $"Failed to get the set point data for the facility {facilityId}");
            Assert.AreEqual(DateTime.Today.ToString("mm-dd-yyyy"), originalSetPointsDTO.TransactionTimeStamp.Value.Date.ToLocalTime().ToString("mm-dd-yyyy"), $"Failed to get the transaction timestamp for the {facilityId}");
            foreach (SetPointDTO Item in originalSetPointsDTO.SetPointsList)
            {
                Assert.IsFalse(Item.IsHidden);
                if (Item.Enum.Count == 0)
                {
                    Item.Value = "2";
                }
            }
            string send = SurveillanceService.SendSetPointsParamsForSingleWell(originalSetPointsDTO, "false");
            originalSetPointsDTO = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), setPointDgDTO.PrimarySPDgType, false.ToString());
            foreach (SetPointDTO Item in originalSetPointsDTO.SetPointsList)
            {
                Assert.IsFalse(Item.IsHidden);
                if (Item.Enum.Count == 0)
                {
                    Assert.AreEqual("2", Item.Value);
                }
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void SetPointsConfigurationTest_WellPilot()
        {
            string facilityId = GetWellPilotFacilityId(1);
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = "RPOCTest01", FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });
            WellDTO[] allWells = WellService.GetAllWells();
            Assert.IsNotNull(allWells, "Failed to get wells.");
            WellDTO well = allWells.FirstOrDefault(w => w.Name.Equals("RPOCTest01"));
            Assert.IsNotNull(well, "Failed to add well.");
            _wellsToRemove.Add(well);
            Trace.WriteLine($"Added new well id '{well.Id}' associated with facility id '{facilityId}'.");
            // System setting
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.SET_POINT_CONFIG);
            SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);

            //Set Point config
            string Str1 = settingValue.StringValue;

            try
            {
                string Str2 = Str1.Substring(0, Str1.Length - "</DeviceTypes>".Length);
                StringBuilder sb = new StringBuilder(Str2);

                sb.Append("<DeviceType  Name =\"WellPilotRPOC\"><SetPointDG Name= \"Basic SetPoints\" IsPrimary=\"true\" Type=\"SetPoints\" UISGetCommandName=\"GETSPDATA\" UISSendCommandName=\"C_SETPOINT\" /><SetPointDG Name=\"Control Parameters\" IsPrimary=\"false\" Type=\"CtrlParms\" UISGetCommandName=\"GETCTRLPAR\" UISSendCommandName=\"SENDCTRPAR\" /><SetPointDG Name=\"Sensor Configuration\" IsPrimary=\"false\" Type=\"SensSetup\" UISGetCommandName=\"GETSENSET\" UISSendCommandName=\"SENDSENSET\" /><SetPointDG Name=\"VFD Configuration\" IsPrimary=\"false\" Type=\"VSDCfg\" UISGetCommandName=\"GETVFDCFG\" UISSendCommandName=\"SENDVFDCFG\" /><SetPointDG Name=\"Energy Management\" IsPrimary=\"false\" Type=\"PkEgyMgtCg\" UISGetCommandName=\"GETENGMGM\" UISSendCommandName=\"SENDENGMGM\" /><SetPointDG Name=\"Firmware Information\" IsPrimary=\"false\" Type=\"CtrlInfo\" UISGetCommandName=\"GETFIRMINF\" /></DeviceType></DeviceTypes>");
                settingValue.StringValue = sb.ToString();

                //"savesystemsetting" api is saving a system setting values
                SettingService.SaveSystemSetting(settingValue);

                //"GetSystemSettingByName" api is getting a system setting name
                settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
                SetPointsDGDTO setPointsDG = SurveillanceService.GetSetPointsDataGroups(well.Id.ToString());
                Assert.IsNotNull(setPointsDG, "Invalid SetPointsDGDTO object.");
                Assert.IsNotNull(setPointsDG.PrimarySPDgType, "Invalid primary set point data group type.");
                Assert.IsNotNull(setPointsDG.OtherSetPointsDGList, "Invalid other set point data group list.");
                SetPointsGetSendAPITest(facilityId, well, setPointsDG.PrimarySPDgType);
                string OtherSetPoint_CtrlParms = setPointsDG.OtherSetPointsDGList.FirstOrDefault(x => x.DgType == "CtrlParms").DgType;
                //Getting current value of Control parameter dg type
                SetPointsDTO setPoints = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), OtherSetPoint_CtrlParms, "");
                // setting new values for control parameter  dg type
                setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "DHPOP").Value = "92";
                setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "DHPOS").Value = "100";
                setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "DHCE").Value = "0";
                setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "DHM").Value = "0";
                setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "POS").Value = setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "POS").Enum[1].ToString();
                setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "GTR").Value = "12:11:15";
                setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "LCG").Value = "100";
                setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "PTA").Value = "0";
                setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "SPOP").Value = "100";
                setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "SPOS").Value = "100";
                setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "SPOA").Value = setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "SPOA").Enum[5].ToString();
                setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "PDT").Value = "15:01:40";
                setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "ITFPO").Value = "00:01:40";
                setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "IPVFA").Value = "100";
                setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "IPVAR").Value = setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "IPVAR").Enum[35].ToString();

                //Sending  new values for all parameters of Control Parameters dg type
                SurveillanceService.SendSetPointsParamsForSingleWell(setPoints, "false");
                SetPointsDTO actval = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), OtherSetPoint_CtrlParms, "");
                Assert.AreEqual("92", actval.SetPointsList.FirstOrDefault(x => x.ElementId == "DHPOP").Value, "DHPOP value is mismatched");
                Assert.AreEqual("100", actval.SetPointsList.FirstOrDefault(x => x.ElementId == "DHPOS").Value, "DHPOS value is mismatched");
                Assert.AreEqual("0", actval.SetPointsList.FirstOrDefault(x => x.ElementId == "DHCE").Value, "DHCE value is mismatched");
                Assert.AreEqual("0", actval.SetPointsList.FirstOrDefault(x => x.ElementId == "DHM").Value, "DHM value is mismatched");
                Assert.AreEqual("1", actval.SetPointsList.FirstOrDefault(x => x.ElementId == "POS").Value, "POS value is mismatched");
                Assert.AreEqual("12:11:15", actval.SetPointsList.FirstOrDefault(x => x.ElementId == "GTR").Value, "GTR value is mismatched");
                Assert.AreEqual("100", actval.SetPointsList.FirstOrDefault(x => x.ElementId == "LCG").Value, "LCG value is mismatched");
                Assert.AreEqual("0", actval.SetPointsList.FirstOrDefault(x => x.ElementId == "PTA").Value, "PTA value is mismatched");
                Assert.AreEqual("100", actval.SetPointsList.FirstOrDefault(x => x.ElementId == "SPOP").Value, "SPOP value is mismatched");
                Assert.AreEqual("100", actval.SetPointsList.FirstOrDefault(x => x.ElementId == "SPOS").Value, "SPOS value is mismatched");
                Assert.AreEqual("5", actval.SetPointsList.FirstOrDefault(x => x.ElementId == "SPOA").Value, "SPOA value is mismatched");
                Assert.AreEqual("15:01:40", actval.SetPointsList.FirstOrDefault(x => x.ElementId == "PDT").Value, "PDT value is mismatched");
                Assert.AreEqual("00:01:40", actval.SetPointsList.FirstOrDefault(x => x.ElementId == "ITFPO").Value, "ITFPO value is mismatched");
                Assert.AreEqual("100", actval.SetPointsList.FirstOrDefault(x => x.ElementId == "IPVFA").Value, "IPVFA value is mismatched");
                Assert.AreEqual("35", actval.SetPointsList.FirstOrDefault(x => x.ElementId == "IPVAR").Value, "IPVAR value is mismatched");

                //Sensor CONFIGURATION
                string OtherSetPoint_SensSetup = setPointsDG.OtherSetPointsDGList.FirstOrDefault(x => x.DgType == "SensSetup").DgType;

                SetPointsDTO SensSetup_setPoints = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), OtherSetPoint_SensSetup, "");
                SensSetup_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "LdSensOfs").Value = "3000";
                SensSetup_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "LdSensGain").Value = "3330";
                SensSetup_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "PosInSTR").Value = "0";
                SensSetup_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "PosDatAvR").Value = "0";
                SensSetup_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "DPSLdDlR").Value = "0";
                SensSetup_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "TSPosSwFS").Value = "1000";
                SensSetup_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "RPCSnFAR").Value = "0";
                SensSetup_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "MotSpCTR").Value = "0";
                SurveillanceService.SendSetPointsParamsForSingleWell(SensSetup_setPoints, "false");
                SetPointsDTO actval_SensSetup = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), OtherSetPoint_SensSetup, "");
                Assert.AreEqual("3000", actval_SensSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "LdSensOfs").Value, "LdSensOfs value is mismatched");
                Assert.AreEqual("3330", actval_SensSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "LdSensGain").Value, "LdSensGain value is mismatched");
                Assert.AreEqual("0", actval_SensSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "PosInSTR").Value, "PosInSTR value is mismatched");
                Assert.AreEqual("0", actval_SensSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "PosDatAvR").Value, "PosDatAvR value is mismatched");
                Assert.AreEqual("0", actval_SensSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "DPSLdDlR").Value, "DPSLdDlR value is mismatched");
                Assert.AreEqual("1000", actval_SensSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "TSPosSwFS").Value, "TSPosSwFS value is mismatched");
                Assert.AreEqual("0", actval_SensSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "RPCSnFAR").Value, "RPCSnFAR value is mismatched");
                Assert.AreEqual("0", actval_SensSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "MotSpCTR").Value, "MotSpCTR value is mismatched");

                //VFD Configuration
                string OtherSetPoint_VFD = setPointsDG.OtherSetPointsDGList.FirstOrDefault(x => x.DgType == "VSDCfg").DgType;
                SetPointsDTO VFD_setPoints = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), OtherSetPoint_VFD, "");
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "RC").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "SM").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "AT").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "DT").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "MS").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "MF").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "LS").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "LST").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "LSTS").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "MnS").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "RTD").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "MxC").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "MC").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "OVS").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "OS").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "IL").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "ILT").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "CLF").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "VSG").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "VSGM").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "AG").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "AB").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "SCA").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "SBR").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "SCP").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "CES").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "SFD").Value = "6000";
                VFD_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "XW").Value = "6000";

                SurveillanceService.SendSetPointsParamsForSingleWell(VFD_setPoints, "false");
                SetPointsDTO actval_VFDSetup = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), OtherSetPoint_VFD, "");

                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "RC").Value, "RC value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "SM").Value, "SM value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "AT").Value, "AT value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "DT").Value, "DT value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "MS").Value, "MS value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "MF").Value, "MF value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "LS").Value, "LS value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "LST").Value, "LST value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "LSTS").Value, "LSTS value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "MnS").Value, "MnS value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "RTD").Value, "RTD value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "MxC").Value, "MxC value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "MC").Value, "MC value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "MF").Value, "MF value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "OVS").Value, "OVS value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "OS").Value, "OS value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "IL").Value, "IL value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "ILT").Value, "ILT value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "CLF").Value, "CLF value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "VSG").Value, "VSG value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "VSGM").Value, "VSGM value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "AG").Value, "AG value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "AB").Value, "AB value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "SCA").Value, "SCA value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "SBR").Value, "SBR value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "SCP").Value, "SCP value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "CES").Value, "CES value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "SFD").Value, "SFD value is mismatched");
                Assert.AreEqual("6000", actval_VFDSetup.SetPointsList.FirstOrDefault(x => x.ElementId == "XW").Value, "XW value is mismatched");

                //Energy Managmenet
                string OtherSetPoint_energy = setPointsDG.OtherSetPointsDGList.FirstOrDefault(x => x.DgType == "PkEgyMgtCg").DgType;
                SetPointsDTO Energy_setPoints = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), OtherSetPoint_energy, "");
                Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "STAWR").Value = "12:10:10";
                Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "RMAWR").Value = "1-Normal RPC Run";
                Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "CTAWR").Value = "14:12:12";
                Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "STBWR").Value = "12:12:10";
                Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "RMBWR").Value = "1-Normal RPC Run";
                Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "CTBWR").Value = "14:12:12";
                Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "STAWDR").Value = "16:18:18";
                Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "RMAWDR").Value = "1-Normal RPC Run";
                Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "CTAWDR").Value = "14:12:12";
                Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "TCEnF").Value = "190";
                Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "PkECE").Value = "200";
                Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "BRInhTR").Value = "10:12:12";
                Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "ERInhTR").Value = "19:12:12";

                SurveillanceService.SendSetPointsParamsForSingleWell(Energy_setPoints, "false");

                Assert.AreEqual("12:10:10", Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "STAWR").Value, "STAWR value is mismatched");
                Assert.AreEqual("1-Normal RPC Run", Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "RMAWR").Value, "RMAWR value is mismatched");
                Assert.AreEqual("14:12:12", Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "CTAWR").Value, "CTAWR value is mismatched");
                Assert.AreEqual("12:12:10", Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "STBWR").Value, "STBWR value is mismatched");
                Assert.AreEqual("1-Normal RPC Run", Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "RMBWR").Value, "RMBWR value is mismatched");
                Assert.AreEqual("14:12:12", Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "CTBWR").Value, "CTBWR value is mismatched");
                Assert.AreEqual("16:18:18", Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "STAWDR").Value, "STAWDR value is mismatched");
                Assert.AreEqual("1-Normal RPC Run", Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "RMAWDR").Value, "RMAWDR value is mismatched");
                Assert.AreEqual("14:12:12", Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "CTAWDR").Value, "CTAWDR value is mismatched");
                Assert.AreEqual("190", Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "TCEnF").Value, "TCEnF value is mismatched");
                Assert.AreEqual("200", Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "PkECE").Value, "PkECE value is mismatched");
                Assert.AreEqual("10:12:12", Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "BRInhTR").Value, "BRInhTR value is mismatched");
                Assert.AreEqual("19:12:12", Energy_setPoints.SetPointsList.FirstOrDefault(x => x.ElementId == "ERInhTR").Value, "ERInhTR value is mismatched");

                //Firmware Information
                string OtherSetPoint_FIRMWARE = setPointsDG.OtherSetPointsDGList.FirstOrDefault(x => x.DgType == "CtrlInfo").DgType;
                SetPointsDTO Firmware_Setpoints = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), OtherSetPoint_FIRMWARE, "");
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
            finally
            {
                //"savesystemsetting" api is saving a system setting values
                //Reverting back to Original System Setting (SetPoint Config)
                settingValue.StringValue = Str1;
                SettingService.SaveSystemSetting(settingValue);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void OrdinalizedSetPointTest()
        {
            string facilityId = GetWellPilotFacilityId(1);
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = "RPOCTest01", FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });
            WellDTO[] allWells = WellService.GetAllWells();
            Assert.IsNotNull(allWells, "Failed to get wells.");
            WellDTO well = allWells.FirstOrDefault(w => w.Name.Equals("RPOCTest01"));
            Assert.IsNotNull(well, "Failed to add well.");
            _wellsToRemove.Add(well);
            Trace.WriteLine($"Added new well id '{well.Id}' associated with facility id '{facilityId}'.");
            // System setting
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.SET_POINT_CONFIG);
            SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);

            //Set Point config
            string Str1 = settingValue.StringValue;

            try
            {
                string Str2 = Str1.Substring(0, Str1.Length - "</DeviceTypes>".Length);
                StringBuilder sb = new StringBuilder(Str2);

                sb.Append("<DeviceType Name=\"WellPilotRPOC\"><SetPointDG Name=\"Basic SetPoints\" IsPrimary=\"true\" Type=\"SetPoints\" UISGetCommandName=\"GETSPDATA\" UISSendCommandName=\"C_SETPOINT\"><Ordinal p4:nil=\"true\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\" /></SetPointDG><SetPointDG Name=\"Analog Input1\" IsPrimary=\"false\" Type=\"AnlgInCfg\" UISGetCommandName=\"GETAIC1\" UISSendCommandName=\"SETAIC1\"><Ordinal>1</Ordinal></SetPointDG><SetPointDG Name=\"Analog Input2\" IsPrimary=\"false\" Type=\"AnlgInCfg\" UISGetCommandName=\"GETAIC2\" UISSendCommandName=\"SETAIC2\"><Ordinal>2</Ordinal></SetPointDG></DeviceType></DeviceTypes>");
                settingValue.StringValue = sb.ToString();

                //"savesystemsetting" api is saving a system setting values
                SettingService.SaveSystemSetting(settingValue);

                //"GetSystemSettingByName" api is getting a system setting by name
                settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
                SetPointsDGDTO setPointsDG = SurveillanceService.GetSetPointsDataGroups(well.Id.ToString());
                Assert.IsNotNull(setPointsDG, "Invalid SetPointsDGDTO object.");



                SetPointsDTO AnalogInput1 = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), "AnlgInCfg", "1", "Always");
                foreach (SetPointDTO setPoint in AnalogInput1.SetPointsList)
                {
                    Trace.WriteLine(setPoint.Description + " : " + setPoint.Value);
                }
                string SaveInDBStatus = SurveillanceService.SendSavedSetPointsParamsForSingleWell(AnalogInput1);
                Assert.IsTrue(SaveInDBStatus.ToLower() == "success", "SetPoints are not saved in Database");

                //Verify that datagroup with Ordinal 1 is correctly saved in database
                SetPointsDTO AnalogInput1SavedInDB = SurveillanceService.GetSavedSetPointsParamsForSingleWell(well.Id.ToString(), "AnlgInCfg", "1", "Always");
                VerifySetPoint(AnalogInput1, AnalogInput1SavedInDB);
                Trace.WriteLine("Verified that datagroup with ordinal 1 is correctly saved in database");
                Trace.WriteLine("");

                //Verify that Datagroup with ordinal 2 is not saved in database
                SetPointsDTO AnalogInput2SavedInDB = SurveillanceService.GetSavedSetPointsParamsForSingleWell(well.Id.ToString(), "AnlgInCfg", "2", "Always");
                Assert.IsNull(AnalogInput2SavedInDB);
                Trace.WriteLine("Verified that Datagroup with ordinal 2 is not saved in database");
                Trace.WriteLine("");

                //Verify that changes made in datagroup with ordinal 1 is properly sent to CygNet
                SetPointsDTO ModifiedAnalogInput1 = ModifySetPoints(AnalogInput1);
                Trace.WriteLine(ModifiedAnalogInput1.SetPointsList.FirstOrDefault(x => x.Description == "AI Engineering Unit Label (0-11)").Value);
                string SendToCygNetStatus = SurveillanceService.SendSetPointsParamsForSingleWell(ModifiedAnalogInput1, "false");
                Assert.IsTrue(SendToCygNetStatus.ToLower() == "success");
                SetPointsDTO resSetPoints = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), "AnlgInCfg", "1", "Always");
                VerifySetPoint(ModifiedAnalogInput1, resSetPoints);
                Trace.WriteLine("Verified that changes made in datagroup with ordinal 1 is properly sent to CygNet");
                Trace.WriteLine("");

                // Verify that datagroup with Ordinal 1 is correctly saved in database
                SaveInDBStatus = SurveillanceService.SendSavedSetPointsParamsForSingleWell(resSetPoints);
                Assert.IsTrue(SaveInDBStatus.ToLower() == "success", "SetPoints are not saved in Database");
                AnalogInput1SavedInDB = SurveillanceService.GetSavedSetPointsParamsForSingleWell(well.Id.ToString(), "AnlgInCfg", "1", "Always");
                VerifySetPoint(resSetPoints, AnalogInput1SavedInDB);
                Trace.WriteLine("Verify that datagroup with Ordinal 1 is correctly saved in database");
                Trace.WriteLine("");

                //Verify that Datagroup with Ordinal 2 is not saved in database
                AnalogInput2SavedInDB = SurveillanceService.GetSavedSetPointsParamsForSingleWell(well.Id.ToString(), "AnlgInCfg", "2", "Always");
                Assert.IsNull(AnalogInput2SavedInDB);
                Trace.WriteLine("Verified that Datagroup with ordinal 2 is not saved in database");
                Trace.WriteLine("");

                Trace.WriteLine("Test Completed Successfully");
            }
            catch (AssertFailedException e)
            {
                Assert.Fail(e.Message);
            }
            finally
            {
                //"savesystemsetting" api is saving a system setting values
                //Reverting back to Original System Setting (SetPoint Config)
                settingValue.StringValue = Str1;
                SettingService.SaveSystemSetting(settingValue);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void OrdinalizedSetPointTestForRelativeFacilities()
        {
            AddUpdateRelativeFacilitySystemSettingForSetPoints("RRLMapping", WellTypeId.RRL);

            string facilityId = GetWellPilotFacilityId(1);
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = "RPOCTest01", FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });
            WellDTO[] allWells = WellService.GetAllWells();
            Assert.IsNotNull(allWells, "Failed to get wells.");
            WellDTO well = allWells.FirstOrDefault(w => w.Name.Equals("RPOCTest01"));
            Assert.IsNotNull(well, "Failed to add well.");
            _wellsToRemove.Add(well);
            Trace.WriteLine($"Added new well id '{well.Id}' associated with facility id '{facilityId}'.");
            // System setting
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.SET_POINT_CONFIG);
            SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);

            //Set Point config
            string Str1 = settingValue.StringValue;

            try
            {
                string Str2 = Str1.Substring(0, Str1.Length - "</DeviceTypes>".Length);
                StringBuilder sb = new StringBuilder(Str2);

                sb.Append("<DeviceType Name=\"WellPilotRPOC\"><SetPointDG Name=\"Basic SetPoints\" IsPrimary=\"true\" Type=\"SetPoints\" UISGetCommandName=\"GETSPDATA\" UISSendCommandName=\"C_SETPOINT\"><Ordinal p4:nil=\"true\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema-instance\" /></SetPointDG><SetPointDG Name=\"Analog Input1\" IsPrimary=\"false\" Type=\"AnlgInCfg\" UISGetCommandName=\"GETAIC1\" UISSendCommandName=\"SETAIC1\"><Ordinal>1</Ordinal></SetPointDG><SetPointDG Name=\"Analog Input2\" IsPrimary=\"false\" Type=\"AnlgInCfg\" UISGetCommandName=\"GETAIC2\" UISSendCommandName=\"SETAIC2\"><Ordinal>2</Ordinal></SetPointDG></DeviceType></DeviceTypes>");
                settingValue.StringValue = sb.ToString();

                //"savesystemsetting" api is saving a system setting values
                SettingService.SaveSystemSetting(settingValue);

                //"GetSystemSettingByName" api is getting a system setting by name
                settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
                SetPointsDGDTO setPointsDG = SurveillanceService.GetSetPointsDataGroups(well.Id.ToString());
                Assert.IsNotNull(setPointsDG, "Invalid SetPointsDGDTO object.");

                SetPointsDTO AnalogInput1 = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), "AnlgInCfg", "1", "Always");
                foreach (SetPointDTO setPoint in AnalogInput1.SetPointsList)
                {
                    Trace.WriteLine(setPoint.Description + " : " + setPoint.Value);
                }
                string SaveInDBStatus = SurveillanceService.SendSavedSetPointsParamsForSingleWell(AnalogInput1);
                Assert.IsTrue(SaveInDBStatus.ToLower() == "success", "SetPoints are not saved in Database");

                //Verify that datagroup with Ordinal 1 is correctly saved in database
                SetPointsDTO AnalogInput1SavedInDB = SurveillanceService.GetSavedSetPointsParamsForSingleWell(well.Id.ToString(), "AnlgInCfg", "1", "Always");
                VerifySetPoint(AnalogInput1, AnalogInput1SavedInDB);
                Trace.WriteLine("Verified that datagroup with ordinal 1 is correctly saved in database");
                Trace.WriteLine("");

                //Verify that Datagroup with ordinal 2 is not saved in database
                SetPointsDTO AnalogInput2SavedInDB = SurveillanceService.GetSavedSetPointsParamsForSingleWell(well.Id.ToString(), "AnlgInCfg", "2", "Always");
                Assert.IsNull(AnalogInput2SavedInDB);
                Trace.WriteLine("Verified that Datagroup with ordinal 2 is not saved in database");
                Trace.WriteLine("");

                //Verify that changes made in datagroup with ordinal 1 is properly sent to CygNet
                SetPointsDTO ModifiedAnalogInput1 = ModifySetPoints(AnalogInput1);
                Trace.WriteLine(ModifiedAnalogInput1.SetPointsList.FirstOrDefault(x => x.Description == "AI Engineering Unit Label (0-11)").Value);
                string SendToCygNetStatus = SurveillanceService.SendSetPointsParamsForSingleWell(ModifiedAnalogInput1, "false");
                Assert.IsTrue(SendToCygNetStatus.ToLower() == "success");
                SetPointsDTO resSetPoints = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), "AnlgInCfg", "1", "Always");
                VerifySetPoint(ModifiedAnalogInput1, resSetPoints);
                Trace.WriteLine("Verified that changes made in datagroup with ordinal 1 is properly sent to CygNet");
                Trace.WriteLine("");

                // Verify that datagroup with Ordinal 1 is correctly saved in database
                SaveInDBStatus = SurveillanceService.SendSavedSetPointsParamsForSingleWell(resSetPoints);
                Assert.IsTrue(SaveInDBStatus.ToLower() == "success", "SetPoints are not saved in Database");
                AnalogInput1SavedInDB = SurveillanceService.GetSavedSetPointsParamsForSingleWell(well.Id.ToString(), "AnlgInCfg", "1", "Always");
                VerifySetPoint(resSetPoints, AnalogInput1SavedInDB);
                Trace.WriteLine("Verify that datagroup with Ordinal 1 is correctly saved in database");
                Trace.WriteLine("");

                //Verify that Datagroup with Ordinal 2 is not saved in database
                AnalogInput2SavedInDB = SurveillanceService.GetSavedSetPointsParamsForSingleWell(well.Id.ToString(), "AnlgInCfg", "2", "Always");
                Assert.IsNull(AnalogInput2SavedInDB);
                Trace.WriteLine("Verified that Datagroup with ordinal 2 is not saved in database");
                Trace.WriteLine("");

                Trace.WriteLine("Test Completed Successfully");
            }
            catch (AssertFailedException e)
            {
                Assert.Fail(e.Message);
            }
            finally
            {
                //"savesystemsetting" api is saving a system setting values
                //Reverting back to Original System Setting (SetPoint Config)
                settingValue.StringValue = Str1;
                SettingService.SaveSystemSetting(settingValue);
            }
        }

        public void VerifySetPoint(SetPointsDTO SetPointInput, SetPointsDTO SetPointOutput)
        {
            Assert.AreEqual(SetPointInput.SetPointsList.Count, SetPointOutput.SetPointsList.Count);
            CompareObjectsUsingReflection(SetPointInput, SetPointOutput, "Mismatch Values ", new HashSet<string>() { nameof(SetPointsDTO.ErrorMessage), nameof(SetPointsDTO.TransactionTimeStamp), nameof(SetPointsDTO.SetPointsList) });
            for (int i = 0; i < SetPointOutput.SetPointsList.Count; i++)
            {
                if (SetPointOutput.SetPointsList[i].IsHidden == true)
                {
                    continue;
                }

                Trace.WriteLine($"Comparing values of element :- { SetPointOutput.SetPointsList[i].Description}");
                CompareObjectsUsingReflection(SetPointInput.SetPointsList[i], SetPointOutput.SetPointsList[i], "Mismatch values", new HashSet<string>() { nameof(SetPointDTO.Enum), (nameof(SetPointDTO.OriginalValue)) });

            }
        }

        public SetPointsDTO ModifySetPoints(SetPointsDTO SetPoints)
        {
            foreach (SetPointDTO element in SetPoints.SetPointsList)
            {
                if (element.IsHidden == true || element.IsReadOnly == true)
                    continue;

                if (element.DisplayType == SetPointElementType.Number)
                {
                    Trace.WriteLine("Earlier Value :- " + element.Value);
                    element.Value = random.Next(Convert.ToInt32(element.RangeLow), Convert.ToInt32(element.RangeHigh)).ToString();
                    Trace.WriteLine("Modified Value :- " + element.Value);
                }
                if (element.DisplayType == SetPointElementType.String)
                {
                    Trace.WriteLine("Earlier Value :- " + element.Value);
                    element.Value = element.Enum[random.Next(1, element.Enum.Count - 1)];
                    //value conains "°", then retain original value
                    if (element.Value.Contains("°"))
                    {
                        element.Value = element.OriginalValue;
                    }
                    Trace.WriteLine("Modified Value :- " + element.Value);
                }
            }
            return SetPoints;
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void POC_MalFunctionSetpointForLufkinSam()
        {
            string facilityId = GetLufkinSamFacilityId(1);

            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = facilityId, FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });

            WellDTO[] allWells = WellService.GetAllWells();
            Assert.IsNotNull(allWells, "Failed to get wells.");
            WellDTO well = allWells.FirstOrDefault(w => w.Name.Equals(facilityId));
            Assert.IsNotNull(well, "Failed to add well.");
            _wellsToRemove.Add(well);
            Trace.WriteLine($"Added new well id '{well.Id}' associated with facility id '{facilityId}'.");

            // System setting
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.SET_POINT_CONFIG);
            SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
            //Set Point config
            string Str1 = settingValue.StringValue;

            try
            {
                string Str2 = Str1.Substring(0, Str1.Length - "</DeviceTypes>".Length);
                StringBuilder sb = new StringBuilder(Str2);
                sb.Append("<DeviceType Name=\"LufkinSam\">" +
                    "<SetPointDG Name = \"PumpOff Set Point\" IsPrimary = \"false\" Type = \"POSetPt\" UISGetCommandName = \"GETPUMPOFF\" UISSendCommandName = \"SETPUMPOFF\">" +
                    "<Ordinal>0</Ordinal></SetPointDG>" +
                    "<SetPointDG Name = \"Basic Set Point\" IsPrimary = \"true\" Type = \"SetPoints\" UISGetCommandName = \"GETSPDATA\" UISSendCommandName = \"C_SETPOINT\">" +
                    "<Ordinal>0</Ordinal></SetPointDG>" +
                    "</DeviceType></DeviceTypes>");

                settingValue.StringValue = sb.ToString();
                //"savesystemsetting" api is saving a system setting values
                SettingService.SaveSystemSetting(settingValue);

                //"GetSystemSettingByName" api is getting a system setting name
                settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
                SetPointsDGDTO setPointsDG = SurveillanceService.GetSetPointsDataGroups(well.Id.ToString());
                Assert.IsNotNull(setPointsDG, "Invalid SetPointsDGDTO object.");
                Assert.IsNotNull(setPointsDG.PrimarySPDgType, "Invalid primary set point data group type.");
                Assert.IsNotNull(setPointsDG.OtherSetPointsDGList, "Invalid Other set point data group type.");

                // Modify Mal function set point
                SetPointsMalPOCGetSendAPITest(facilityId, well, setPointsDG.PrimarySPDgType, Convert.ToString(setPointsDG.PrimarySPDgOrdinal));

                // Modify POC function set point
                string OtherSetPoint_POSetPt = setPointsDG.OtherSetPointsDGList.FirstOrDefault(x => x.DgType == "POSetPt").DgType;
                string ordinal_OtherSetPoint = Convert.ToString(setPointsDG.OtherSetPointsDGList.FirstOrDefault(x => x.DgType == "POSetPt").DgOrdinal);
                SetPointsMalPOCGetSendAPITest(facilityId, well, OtherSetPoint_POSetPt, ordinal_OtherSetPoint);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message); ;
                Trace.WriteLine(e.Message);
            }
            finally
            {
                //"savesystemsetting" api is saving a system setting values
                //Reverting back to Original System Setting (SetPoint Config)
                settingValue.StringValue = Str1;
                SettingService.SaveSystemSetting(settingValue);
            }
        }

        private void SetPointsMalPOCGetSendAPITest(string facilityId, WellDTO well, string dgType, string ordinal)
        {
            // Get the latest Set Points data
            var setPointsDTO = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), dgType, ordinal, "false");
            Assert.AreEqual("success", setPointsDTO.ErrorMessage, $"Failed to get the Set Points data for the first time from facility {facilityId}");

            var setPointsList = setPointsDTO.SetPointsList;
            List<SetPointDTO> UpdatedSetPointsList = new List<SetPointDTO>();
            if (dgType == "SetPoints")
            {
                for (int i = 0; i < setPointsDTO.SetPointsList.Count; i++)
                {
                    var updatedspDTO = setPointsDTO.SetPointsList[i];
                    if (setPointsDTO.SetPointsList[i].ElementId.Contains("MalStLoad"))
                        updatedspDTO.Value = "65530";
                    if (setPointsDTO.SetPointsList[i].ElementId.Contains("MalStPosn"))
                        updatedspDTO.Value = "650";
                    UpdatedSetPointsList.Add(updatedspDTO);
                }
            }
            if (dgType == "POSetPt")
            {
                for (int i = 0; i < setPointsDTO.SetPointsList.Count; i++)
                {
                    var updatedspDTO = setPointsDTO.SetPointsList[i];
                    if (setPointsDTO.SetPointsList[i].ElementId.Contains("PocStLoad"))
                        updatedspDTO.Value = "25000";
                    if (setPointsDTO.SetPointsList[i].ElementId.Contains("PocStPosn"))
                        updatedspDTO.Value = "150";
                    UpdatedSetPointsList.Add(updatedspDTO);
                }
            }

            SetPointsDTO modifiedSetPointsDTO = new SetPointsDTO()
            {
                WellId = well.Id,
                SetPointsList = UpdatedSetPointsList,
                DataGroupName = setPointsDTO.DataGroupName,
                DataGroupOrdinal = uint.Parse(ordinal),
            };
            Assert.AreEqual("success", SurveillanceService.SendSetPointsParamsForSingleWell(modifiedSetPointsDTO, "false"), $"Send SetPoints data failed for {facilityId}.");

            // Get again SetPoint data to validate that the data was properly sent to device
            var spDTOAfterSend = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), dgType, ordinal, "false");
            Assert.AreEqual("success", spDTOAfterSend.ErrorMessage, $"Failed to get the Set Points data for the second time from the facility {facilityId}");

            Assert.AreEqual(UpdatedSetPointsList.Count, spDTOAfterSend.SetPointsList.Count, $"Send Set Points data did not work properly for {facilityId}.");

            int misMatchedVals = 0;
            for (int i = 0; i < UpdatedSetPointsList.Count; i++)
            {
                try
                {
                    Assert.AreEqual(UpdatedSetPointsList[i].Value, spDTOAfterSend.SetPointsList[i].Value, $"The device value '{spDTOAfterSend.SetPointsList[i].Value}' for element '{spDTOAfterSend.SetPointsList[i].Description}' does not match with the value '{UpdatedSetPointsList[i].Value}' sent.");
                }
                catch (Exception ex)
                {
                    ++misMatchedVals;
                    Trace.WriteLine($"Value mismatch detail: {ex.Message}.");
                }
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void SetPointsConfigurationTest_LufkinSAM()
        {
            string facilityId = GetLufkinSamFacilityId(1);

            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = "SAMTest01", FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });

            WellDTO[] allWells = WellService.GetAllWells();
            Assert.IsNotNull(allWells, "Failed to get wells.");
            WellDTO well = allWells.FirstOrDefault(w => w.Name.Equals("SAMTest01"));
            Assert.IsNotNull(well, "Failed to add well.");
            _wellsToRemove.Add(well);
            Trace.WriteLine($"Added new well id '{well.Id}' associated with facility id '{facilityId}'.");

            var setPointDgDTO = SurveillanceService.GetSetPointsDataGroups(well.Id.ToString());
            Assert.IsNotNull(setPointDgDTO, "Invalid SetPointsDGDTO object.");
            Assert.IsNotNull(setPointDgDTO.PrimarySPDgType, "Invalid primary set point data group type.");

            SetPointsGetSendAPITest(facilityId, well, setPointDgDTO.PrimarySPDgType);
        }

        private void SetPointsGetSendAPITest(string facilityId, WellDTO well, string dgType)
        {
            // Get the latest Set Points data
            var setPointsDTO = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), dgType, "0", "false");
            Assert.AreEqual("success", setPointsDTO.ErrorMessage, $"Failed to get the Set Points data for the first time from facility {facilityId}");

            var setPointsList = setPointsDTO.SetPointsList;
            List<SetPointDTO> UpdatedSetPointsList = new List<SetPointDTO>();

            // Update the data element values with some different value. Right now, they are getting set with high range value.
            if (well.WellType == WellTypeId.PCP)
                UpdatedSetPointsList = ModifySetPoints(setPointsDTO).SetPointsList.ToList();
            else
            {
                string elementVal = "0";
                for (int i = 0; i < setPointsDTO.SetPointsList.Count; i++)
                {
                    var updatedspDTO = setPointsDTO.SetPointsList[i];

                    if (!setPointsDTO.SetPointsList[i].IsReadOnly)
                    {
                        if (!setPointsDTO.SetPointsList[i].IsHidden)
                        {
                            if (updatedspDTO.UnitKey == UnitKeys.Seconds)
                            {
                                elementVal = "00:10:10";
                            }
                            else
                            {
                                elementVal = updatedspDTO.RangeHigh.ToString();
                            }
                            updatedspDTO.Value = elementVal;
                        }
                    }
                    UpdatedSetPointsList.Add(updatedspDTO);
                }
            }
            SetPointsDTO modifiedSetPointsDTO = new SetPointsDTO()
            {
                WellId = well.Id,
                SetPointsList = UpdatedSetPointsList,
                DataGroupName = setPointsDTO.DataGroupName,
                DataGroupOrdinal = 0,
            };

            Assert.AreEqual("success", SurveillanceService.SendSetPointsParamsForSingleWell(modifiedSetPointsDTO, "false"), $"Send SetPoints data failed for {facilityId}.");

            // Get again SetPoint data to validate that the data was properly sent to device
            var spDTOAfterSend = SurveillanceService.GetSetPointsParamsForSingleWell(well.Id.ToString(), dgType, "0", "false");
            Assert.AreEqual("success", spDTOAfterSend.ErrorMessage, $"Failed to get the Set Points data for the second time from the facility {facilityId}");

            Assert.AreEqual(UpdatedSetPointsList.Count, spDTOAfterSend.SetPointsList.Count, $"Send Set Points data did not work properly for {facilityId}.");

            int misMatchedVals = 0;
            for (int i = 0; i < UpdatedSetPointsList.Count; i++)
            {
                try
                {
                    Assert.AreEqual(UpdatedSetPointsList[i].Value, spDTOAfterSend.SetPointsList[i].Value, $"The device value '{spDTOAfterSend.SetPointsList[i].Value}' for element '{spDTOAfterSend.SetPointsList[i].Description}' does not match with the value '{UpdatedSetPointsList[i].Value}' sent.");
                }
                catch (Exception ex)
                {
                    ++misMatchedVals;
                    Trace.WriteLine($"Value mismatch detail: {ex.Message}.");
                }
            }

        }

        public void GetRunTimeYesterdayForSingleWell(string facilityTag)
        {
            RRLWellStatusValueDTO wellStatus = AddWellAndGetDemandScanData(facilityTag);
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            SurveillanceService.IssueCommandForSingleWell(well.Id, "DemandScan");

            string RunTimeYesterday = SurveillanceService.GetRunTimeYesterdayForSingleWell(well.Id.ToString());
            Trace.WriteLine("RunTimeYesterday: " + RunTimeYesterday);
            double _Totaltime = wellStatus.RunTimeYesterday ?? 0.0;
            TimeSpan _Sumtime = TimeSpan.FromSeconds(_Totaltime);
            Assert.AreEqual((Math.Round((_Sumtime.TotalHours), 1)).ToString(), RunTimeYesterday);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetRunTimeYesterdayForSingleWell_WP()
        {
            GetRunTimeYesterdayForSingleWell("RPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetRunTimeYesterdayForSingleWell_LufkinSAM()
        {
            GetRunTimeYesterdayForSingleWell("SAM_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetRunTimeYesterdayForSingleWell_8800()
        {
            GetRunTimeYesterdayForSingleWell("8800_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetRunTimeYesterdayForSingleWell_AEPOC()
        {
            GetRunTimeYesterdayForSingleWell("AEPOC_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetRunTimeYesterdayForSingleWell_EPICFS()
        {
            GetRunTimeYesterdayForSingleWell("EPICFS_");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void DownloadDownholeConfiguration_WellPilot()
        {
            string facilityId = GetWellPilotFacilityId(1);
            TestDownloadDownholeConfig(facilityId);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void DownloadDownholeConfiguration_LufkinSam()
        {
            string facilityId = GetLufkinSamFacilityId(1);
            TestDownloadDownholeConfig(facilityId);
        }

        private void GetControllerCapabilities(string facilityId, WellTypeId wellType, List<CardType> expectedCardTypes, ControllerFeatures expectedControllerFeatures)
        {
            string wellName = "RRLCapabilityTestWell";
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = wellName, FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = wellType }) });

            WellDTO well = WellService.GetWellByName(wellName);
            Assert.IsNotNull(well, $"Failed to add well {wellName} with facility id {facilityId}.");
            _wellsToRemove.Add(well);
            Trace.WriteLine($"Added new well id '{well.Id}' associated with facility id '{facilityId}'.");

            WellControllerCapabilitiesDTO features = SurveillanceService.GetControllerCapabilities(well.Id.ToString());
            Assert.IsNotNull(features, "Failed to get RRL features.");
            CollectionAssert.AreEquivalent(expectedCardTypes, features.CardTypes, $"Supported card types has unexpected value for facility {facilityId}.");
            Assert.AreEqual(expectedControllerFeatures, features.Features, $"Supported RRL features has unexpected value for facility {facilityId}.");

            WellConfigurationService.RemoveWellConfig(well.Id.ToString());
            _wellsToRemove.Remove(well);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetControllerCapabilities()
        {
            GetControllerCapabilities(GetWellPilotFacilityId(1), WellTypeId.RRL, new List<CardType>() { CardType.Alarm, CardType.Current, CardType.Failure, CardType.Full, CardType.Pumpoff }, ControllerFeatures.SetPoints | ControllerFeatures.DownloadDownholeConfiguration);
            GetControllerCapabilities(GetLufkinSamFacilityId(1), WellTypeId.RRL, new List<CardType>() { CardType.Current, CardType.Full, CardType.Pumpoff, CardType.Reference }, ControllerFeatures.SetPoints | ControllerFeatures.DownloadDownholeConfiguration);
            GetControllerCapabilities(GetEProdFacilityId(1), WellTypeId.RRL, new List<CardType>() { CardType.Current, CardType.Full, CardType.Pumpoff }, ControllerFeatures.None);
            GetControllerCapabilities(GetAEPOCFacilityId(1), WellTypeId.RRL, new List<CardType>() { CardType.Alarm, CardType.Current, CardType.Failure, CardType.Full, CardType.Pumpoff }, ControllerFeatures.None);
            GetControllerCapabilities(GetEPICFSFacilityId(1), WellTypeId.RRL, new List<CardType>() { CardType.Current, CardType.Full, CardType.Pumpoff }, ControllerFeatures.None);
            GetControllerCapabilities(GetGLFacilityId(1), WellTypeId.GLift, null, ControllerFeatures.SetPoints);
            GetControllerCapabilities(GetESPFacilityId(1), WellTypeId.ESP, null, ControllerFeatures.SetPoints);
        }

        private void GetAvailableQuantities(string facilityId, List<WellQuantity> expectedQuantities)
        {
            string wellName = "RRLCapabilityTestWell";
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = wellName, FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });

            WellDTO well = WellService.GetWellByName(wellName);
            Assert.IsNotNull(well, $"Failed to add well {wellName} with facility id {facilityId}.");
            _wellsToRemove.Add(well);
            Trace.WriteLine($"Added new well id '{well.Id}' associated with facility id '{facilityId}'.");

            WellQuantity[] availableQuantities = SurveillanceService.GetAvailableQuantities(well.Id.ToString());

            // Three new quantities have been added for RPOC device
            if (well.FacilityId.Contains("RPOC_"))
                CollectionAssert.AreEquivalent(expectedQuantities, availableQuantities, $"Available quantities has unexpected value for facility {facilityId}.");
            else if (well.FacilityId.Contains("SAM_"))
                Assert.AreEqual(21, availableQuantities.Count(), $"Available quantities has unexpected value for facility {facilityId}.");
            else if (well.FacilityId.Contains("8800_"))
            {
                foreach (WellQuantity wt in availableQuantities)
                {
                    Trace.WriteLine(wt.ToString());
                }
                if (s_isRunningInATS)
                {
                    Assert.AreEqual(15, availableQuantities.Count(), $"Available quantities has unexpected value for facility {facilityId}.");
                }
                else
                {
                    Assert.AreEqual(17, availableQuantities.Count(), $"Available quantities has unexpected value for facility {facilityId}.");
                }

            }
            else if (well.FacilityId.Contains("EPICFS_"))
            {
                Assert.AreEqual(17, availableQuantities.Count(), $"Available quantities has unexpected value for facility {facilityId}.");
            }
            else if (well.FacilityId.Contains("AEPOC_"))
                Assert.AreEqual(15, availableQuantities.Count(), $"Available quantities has unexpected value for facility {facilityId}.");

            WellConfigurationService.RemoveWellConfig(well.Id.ToString());
            _wellsToRemove.Remove(well);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetAvailableQuantities()
        {
            List<WellQuantity> allQuantities = QuantityUDCMapping<WellQuantity>.Instance.GetQuantitiesForWellType(WellTypeId.RRL).ToList();
            GetAvailableQuantities(GetWellPilotFacilityId(1), allQuantities);
            GetAvailableQuantities(GetLufkinSamFacilityId(1), allQuantities);
            GetAvailableQuantities(GetEProdFacilityId(1), allQuantities);
            GetAvailableQuantities(GetAEPOCFacilityId(1), allQuantities);
            GetAvailableQuantities(GetEPICFSFacilityId(1), allQuantities);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForSingleWellGeneral()
        {
            string facilityId = string.Empty;
            _addedWells = new List<WellDTO>();
            try
            {
                foreach (WellTypeId wellTypeId in (WellTypeId[])Enum.GetValues(typeof(WellTypeId)))
                {
                    //(02/28/2019)Skipping this test for OT and PCP well as nothing is implemeneted yet for OT and PCP wells
                    if (wellTypeId == WellTypeId.OT || wellTypeId == WellTypeId.PCP)
                    {
                        continue;
                    }

                    // Take out wellTypeId != WellTypeId.WGInj when WAG well types implement issue commands
                    if (wellTypeId != WellTypeId.Unknown && wellTypeId != WellTypeId.RRL && wellTypeId != WellTypeId.WGInj)
                    {
                        switch (wellTypeId)
                        {
                            case WellTypeId.ESP:
                                facilityId = GetFacilityId("ESPWELL_", 1);
                                break;

                            case WellTypeId.GInj:
                                facilityId = GetFacilityId("GASINJWELL_", 1);
                                break;

                            case WellTypeId.GLift:
                                facilityId = GetFacilityId("GLWELL_", 1);
                                break;

                            case WellTypeId.NF:
                                facilityId = GetFacilityId("NFWWELL_", 1);
                                break;

                            case WellTypeId.WInj:
                                facilityId = GetFacilityId("WATERINJWELL_", 1);
                                break;

                            case WellTypeId.PLift:
                                facilityId = GetFacilityId("PGLWELL_", 5);
                                break;

                            case WellTypeId.OT:
                                break;

                            case WellTypeId.PCP:
                                break;

                            case WellTypeId.All:
                                break;

                            default:
                                Assert.Fail($"Unexpected well type: {wellTypeId}.");
                                break;
                        }
                        if (wellTypeId == WellTypeId.Unknown || wellTypeId == WellTypeId.All)
                        {
                            continue;
                        }
                        WellDTO well = SetDefaultFluidType(new WellDTO() { Name = wellTypeId + "TestWell", FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), AssemblyAPI = wellTypeId + "TestWell", SubAssemblyAPI = wellTypeId + "TestWell", IntervalAPI = wellTypeId + "TestWell", CommissionDate = DateTime.Today, WellType = wellTypeId });
                        WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
                        WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(well.Name));
                        Assert.IsNotNull(addedWell);
                        _wellsToRemove.Add(addedWell);
                        _addedWells.Add(addedWell);
                        // First, assert that the DemandScan scanned
                        bool commandResult = SurveillanceService.IssueCommandForSingleWell(addedWell.Id, WellCommand.DemandScan.ToString());
                        Assert.IsTrue(commandResult, "Failed to send command to well of type " + wellTypeId);

                        // Second, assert that (1) the Last Scan and Last Good Scan are one and the same and (2) that values were scanned for each well type
                        switch (wellTypeId)
                        {
                            case WellTypeId.ESP:
                                var espWellStatusValue = SurveillanceServiceClient.GetWellStatusData<ESPWellStatusUnitDTO, ESPWellStatusValueDTO>(addedWell.Id.ToString()).Value;
                                Assert.AreEqual(espWellStatusValue.LastScanTime, espWellStatusValue.LastGoodScanTime, "Scan times should match for well " + espWellStatusValue.WellName);
                                Assert.IsNotNull(espWellStatusValue.CasingPressure, "Expected non-null casing pressure from well " + espWellStatusValue.WellName);
                                Assert.IsNotNull(espWellStatusValue.PumpDischargePressure, "Expected non-null pump discharge pressure from well " + espWellStatusValue.WellName);
                                Assert.IsNotNull(espWellStatusValue.PumpIntakePressure, "Expected non-null pump intake pressure from well " + espWellStatusValue.WellName);
                                Assert.IsNotNull(espWellStatusValue.TubingPressure, "Expected non-null tubing pressure from well " + espWellStatusValue.WellName);
                                Trace.WriteLine("Verification completed for ESP Well Type");
                                break;

                            case WellTypeId.GInj:
                                var giWwellStatusValue = SurveillanceServiceClient.GetWellStatusData<GIWellStatusUnitDTO, GIWellStatusValueDTO>(addedWell.Id.ToString()).Value;
                                Assert.AreEqual(giWwellStatusValue.LastScanTime, giWwellStatusValue.LastGoodScanTime, "Scan times should match for well " + giWwellStatusValue.WellName);
                                Assert.IsNotNull(giWwellStatusValue.DHGaugePressure, "Expected non-null downhole gauge pressure from well " + giWwellStatusValue.WellName);
                                Assert.IsNotNull(giWwellStatusValue.TubingHeadTemperature, "Expected non-null tubing head temperature from well " + giWwellStatusValue.WellName);
                                Assert.IsNotNull(giWwellStatusValue.TubingPressure, "Expected non-null tubing pressure from well " + giWwellStatusValue.WellName);
                                Trace.WriteLine("Verification completed for GINJ Well Type");
                                break;

                            case WellTypeId.GLift:
                                var glWwellStatusValue = SurveillanceServiceClient.GetWellStatusData<GLWellStatusUnitDTO, GLWellStatusValueDTO>(addedWell.Id.ToString()).Value;
                                Assert.AreEqual(glWwellStatusValue.LastScanTime, glWwellStatusValue.LastGoodScanTime, "Scan times should match for well " + glWwellStatusValue.WellName);
                                Assert.IsNotNull(glWwellStatusValue.DHGaugePressure, "Expected non-null downhole gauge pressure from well " + glWwellStatusValue.WellName);
                                Assert.IsNotNull(glWwellStatusValue.GasInjectionRate, "Expected non-null gas injection rate from well " + glWwellStatusValue.WellName);
                                Assert.IsNotNull(glWwellStatusValue.TubingHeadTemperature, "Expected non-null tubing head temperature from well " + glWwellStatusValue.WellName);
                                Assert.IsNotNull(glWwellStatusValue.TubingPressure, "Expected non-null tubing pressure from well " + glWwellStatusValue.WellName);
                                Trace.WriteLine("Verification completed for Gas Lift Well Type");
                                break;

                            case WellTypeId.NF:
                                var nfWellStatusValue = SurveillanceServiceClient.GetWellStatusData<NFWellStatusUnitDTO, NFWellStatusValueDTO>(addedWell.Id.ToString()).Value;
                                Assert.AreEqual(nfWellStatusValue.LastScanTime, nfWellStatusValue.LastGoodScanTime, "Scan times should match for well " + nfWellStatusValue.WellName);
                                Assert.IsNotNull(nfWellStatusValue.DHGaugePressure, "Expected non-null downhole gauge pressure from well " + nfWellStatusValue.WellName);
                                Assert.IsNotNull(nfWellStatusValue.TubingHeadTemperature, "Expected non-null tubing head temperature from well " + nfWellStatusValue.WellName);
                                Assert.IsNotNull(nfWellStatusValue.TubingPressure, "Expected non-null tubing pressure from well " + nfWellStatusValue.WellName);
                                Trace.WriteLine("Verification completed for NF Well Type");
                                break;

                            case WellTypeId.WInj:
                                var wiWwellStatusValue = SurveillanceServiceClient.GetWellStatusData<WIWellStatusUnitDTO, WIWellStatusValueDTO>(addedWell.Id.ToString()).Value;
                                Assert.AreEqual(wiWwellStatusValue.LastScanTime, wiWwellStatusValue.LastGoodScanTime, "Scan times should match for well " + wiWwellStatusValue.WellName);
                                Assert.IsNotNull(wiWwellStatusValue.DHGaugePressure, "Expected non-null downhole gauge pressure from well " + wiWwellStatusValue.WellName);
                                Assert.IsNotNull(wiWwellStatusValue.TubingHeadTemperature, "Expected non-null tubing head temperature from well " + wiWwellStatusValue.WellName);
                                Assert.IsNotNull(wiWwellStatusValue.TubingPressure, "Expected non-null tubing pressure from well " + wiWwellStatusValue.WellName);
                                Trace.WriteLine("Verification completed for WINJ Well Type");
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetGroupStatusCOMMDevice_Type_RTUAddress()
        {
            string passFacilityId1 = null;

            // RRL Well Type            
            WellDTO wellRPOC = AddRRLWell(GetFacilityId("RPOC_", 1));
            _wellsToRemove.Add(wellRPOC);

            // Well Status data
            RRLWellStatusValueDTO GetRRLWellStatus = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(wellRPOC.Id.ToString()).Value;
            Assert.AreEqual("WellPilotRPOC", GetRRLWellStatus.DeviceType, "The Device Type is mismached");
            Assert.IsNotNull(GetRRLWellStatus.CommDeviceID, "The COMM Device(ORION_COMM) ID is mismatched, Wrong configuration in CygNet");
            Assert.IsNotNull(GetRRLWellStatus.CommDeviceIP, "The Comm Device IP is Null");
            Assert.IsNotNull(GetRRLWellStatus.CommDevicePort, "The Comm Port(10000) is Null");

            // Group Status data
            WellGroupStatusQueryDTO newRRLGroup = GroupStatusQuery(WellTypeId.RRL);
            var GetRRLWellGroupStatus = SurveillanceServiceClient.GetWellGroupStatus<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(newRRLGroup);
            RRLWellStatusValueDTO rrlValDTO = GetRRLWellGroupStatus.Status.FirstOrDefault(x => x.WellName == GetFacilityId("RPOC_", 1));
            Assert.AreEqual("WellPilotRPOC", rrlValDTO.DeviceType, "The Device Type is mismached");
            Assert.IsNotNull(rrlValDTO.CommDeviceID, "The COMM Device(ORION_COMM) ID is mismatched, Wrong configuration in CygNet");
            Assert.IsNotNull(rrlValDTO.CommDeviceIP, "The Comm Device IP is Null");
            Assert.IsNotNull(rrlValDTO.CommDevicePort, "The Comm Port(10000) is Null");

            // NON-RRL Well Type            
            passFacilityId1 = GetFacilityId("ESPWELL_", 1);
            WellDTO wellESP = AddNonRRLWell(passFacilityId1, WellTypeId.ESP, false);
            _wellsToRemove.Add(wellESP);

            // Group Status data
            WellGroupStatusQueryDTO newGroup = GroupStatusQuery(WellTypeId.ESP);
            var GetNONRRLWellGroupStatus = SurveillanceServiceClient.GetWellGroupStatus<ESPWellStatusUnitDTO, ESPWellStatusValueDTO>(newGroup);
            ESPWellStatusValueDTO espValDTO = GetNONRRLWellGroupStatus.Status.FirstOrDefault(x => x.WellName == GetFacilityId("ESPWELL_", 1));
            Assert.AreEqual("WFordKS", espValDTO.DeviceType, "The Device Type is mismached");
            Assert.IsNotNull(espValDTO.CommDeviceID, "The COMM Device(DLQSIM) ID is mismatched, Wrong configuration in CygNet");
            Assert.IsNotNull(espValDTO.CommDeviceIP, "The Comm Device IP is Null");
            Assert.IsNotNull(espValDTO.CommDevicePort, "The Comm Port(20001) is mismatched");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellGroupStatusForNonRRL()
        {
            string passFacilityId1 = "";
            passFacilityId1 = GetFacilityId("ESPWELL_", 1);
            AddWell_NonRRL(passFacilityId1, WellTypeId.ESP, true);
            passFacilityId1 = GetFacilityId("GLWELL_", 1);
            AddWell_NonRRL(passFacilityId1, WellTypeId.GLift, true);
            passFacilityId1 = GetFacilityId("GASINJWELL_", 1);
            AddWell_NonRRL(passFacilityId1, WellTypeId.GInj, true);
            passFacilityId1 = GetFacilityId("NFWWELL_", 1);
            AddWell_NonRRL(passFacilityId1, WellTypeId.NF, true);
            passFacilityId1 = GetFacilityId("WATERINJWELL_", 1);
            AddWell_NonRRL(passFacilityId1, WellTypeId.WInj, true);
            passFacilityId1 = GetFacilityId("IWC_", 1);
            AddWell_NonRRL(passFacilityId1, WellTypeId.WGInj, true);

            GetWellGroupStatus_NONRRL("ESP_", WellTypeId.ESP);
            GetWellGroupStatus_NONRRL("GINJ_", WellTypeId.GInj);
            GetWellGroupStatus_NONRRL("NFW_", WellTypeId.NF);
            GetWellGroupStatus_NONRRL("WINJ_", WellTypeId.WInj);
            GetWellGroupStatus_NONRRL("GL_", WellTypeId.GLift);
            GetWellGroupStatus_NONRRL("WAGI_", WellTypeId.WGInj);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void CheckAlarmStatusGL()
        {
            _addedWells = new List<WellDTO>();
            string facilityId;
            facilityId = GetFacilityId("GLWELL_", 1);

            WellDTO well = SetDefaultFluidType(new WellDTO() { Name = "GL" + "TestWell", FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today.AddYears(-2), WellType = WellTypeId.GLift });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
            WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(well.Name));
            Assert.IsNotNull(addedWell);
            _wellsToRemove.Add(addedWell);
            _addedWells.Add(addedWell);
            //Collect alarm information for Gas Lift wells
            if (addedWell.WellType == WellTypeId.GLift)
            {
                var assemblyDTO = WellboreComponentService.GetAssemblyByWellId(addedWell.Id.ToString());
                Assert.IsNotNull(assemblyDTO);
                //add model file for well test data
                string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
                Tuple<string, WellTypeId, ModelFileOptionDTO> model =
                    Tuple.Create("WellfloGasLiftExampleTuningTrace.WFLX", WellTypeId.GLift, new ModelFileOptionDTO()
                    {
                        CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                        OptionalUpdate = new long[]
                        {
                                (long)OptionalUpdates.UpdateGOR_CGR,
                                (long)OptionalUpdates.UpdateWCT_WGR,
                        }
                    });
                string modelInfo = model.Item1;
                WellTypeId wellType = model.Item2;
                ModelFileOptionDTO options = model.Item3;
                Trace.WriteLine("Testing model: " + model);
                ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };

                options.Comment = "CASETest Upload " + model.Item1;
                modelFile.Options = options;
                modelFile.ApplicableDate = DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(1);
                modelFile.WellId = addedWell.Id;
                byte[] fileAsByteArray = GetByteArray(Path, model.Item1);
                Assert.IsNotNull(fileAsByteArray);
                modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
                ModelFileValidationDataDTO ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                Assert.IsNotNull(ModelFileValidationData);
                //add modelfile
                ModelFileService.AddWellModelFile(modelFile);
                ModelFileDTO newModelFile = ModelFileService.GetCurrentModelFile(addedWell.Id.ToString());
                Assert.IsNotNull(newModelFile);
                //Add valid well test
                WellTestDTO testDataDTO = new WellTestDTO()
                {
                    WellId = addedWell.Id,
                    AverageCasingPressure = 1000,
                    AverageFluidAbovePump = 1,
                    AverageTubingPressure = 200,
                    AverageTubingTemperature = 0,
                    Gas = 750,
                    GasInjectionRate = 1000,
                    GasGravity = 0,
                    Oil = 1498,
                    PumpingHours = 10,
                    //SPTCode = 1,
                    SPTCodeDescription = WellTestQuality.AllocatableTest.ToString(),
                    TestDuration = 3,
                    Water = 2247
                };

                testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime());
                // add wellTest
                WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(addedWell.Id.ToString()).Units;
                WellTestAndUnitsDTO WellTestAndUnits = new WellTestAndUnitsDTO();
                WellTestAndUnits.Units = units;
                WellTestAndUnits.Value = testDataDTO;
                WellTestDataService.SaveWellTest(WellTestAndUnits);
                // First, assert that the DemandScan scanned
                bool commandResult = SurveillanceService.IssueCommandForSingleWell(addedWell.Id, WellCommand.DemandScan.ToString());
                Assert.IsTrue(commandResult, "Failed to send command to well of type GLift ");
                //Calling Well Status API
                SurveillanceServiceClient.GetWellStatusData(addedWell.Id.ToString());



                CurrentAlarmDTO[] alarms = AlarmService.GetCurrentAlarmsByWellId(addedWell.Id.ToString());
                Assert.IsNotNull(alarms);
                Assert.IsTrue(alarms.Length > 6, "Operating alrams for Gas Lift were not triggered ");
                UnitsValuesPairDTO<object, object> welstatusdata = SurveillanceServiceClient.GetWellStatusData(addedWell.Id.ToString());

                GLWellStatusValueDTO glwellstatus = (GLWellStatusValueDTO)welstatusdata.Value;
                List<string> icollection = new List<string>();
                icollection.Add("Casing Pressure");
                icollection.Add("Tubing Pressure");
                icollection.Add("Tubing Temperature");
                icollection.Add("Injection Rate");
                icollection.Add("Oil Rate");
                icollection.Add("Water Rate");
                icollection.Add("Gas Rate");
                string[] glalrammsg = glwellstatus.AlarmMessage;
                VerifyAlarmsOnWellStatusPage(icollection, true, glwellstatus);
                string start_date = (DateTime.Today - TimeSpan.FromDays(30)).ToUniversalTime().ToISO8601();
                string end_date = DateTime.Today.ToUniversalTime().ToISO8601();
                VerifyAlarmsOnForesiteAlarmsHistoryPage(addedWell, icollection, start_date, end_date, true);
                //Delete alarm data for added well
                AlarmService.RemoveAlarmsByWellId(addedWell.Id.ToString());
                //Check alarm for this well is empty
                alarms = AlarmService.GetCurrentAlarmsByWellId(addedWell.Id.ToString());
                Assert.IsFalse(alarms?.Any() ?? false, "There should be no current alarms.");
                WellTestDTO[] validWellTest = WellTestDataService.GetWellTestDataByWellId(addedWell.Id.ToString()).Values;
                testDataDTO = validWellTest.FirstOrDefault(a => a.WellId == addedWell.Id);
                WellTestDataService.DeleteWellTestData(testDataDTO.Id.ToString());
                WellTestDTO[] deletedWellTest = WellTestDataService.GetWellTestDataByWellId(addedWell.Id.ToString()).Values;
                testDataDTO = deletedWellTest.FirstOrDefault(a => a.WellId == addedWell.Id);
                Assert.IsNull(testDataDTO);
            }
        }


        public void VerifyAlarmsOnForesiteAlarmsHistoryPage(WellDTO well, List<string> icollection, string startdate, string enddate, bool alarmON)
        {

            String result = new String(' ', 50);
            AlarmHistoryDTO[] fsalms = IntelligentAlarmService.GetIntelligentAlarmHistoryByWellIdAndDateRange(well.Id.ToString(), startdate, enddate);
            string almtxt = alarmON ? "Alarm SET " : "Alarm Clear";
            Trace.WriteLine($"*******************ForeSite Alarm history records: {almtxt}*****************************");

            Trace.WriteLine($"Alarm Status                                        | Started Time                                       | ClearTime|");
            foreach (AlarmHistoryDTO alamdto in fsalms)
            {
                string fsalm = alamdto.AlarmStatus;
                DateTime alstarttime = alamdto.StartedTime;
                DateTime? alclearttime = alamdto.ClearedTime;
                Trace.WriteLine($"{fsalm} {new String(' ', 50 - fsalm.Length)} | {alstarttime}{new String(' ', 50 - alstarttime.ToString().Length)} |{alclearttime} {new String(' ', 50 - alclearttime.ToString().Length)}|");
                //  CollectionAssert.Contains(icollection, fsalm, $" Expected Intelligent Alarm {fsalm.ToString()} is not updated in ForeSite alarm history");
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

        public void VerifyAlarmsOnWellStatusPage(List<string> icollection, bool alarmON, GLWellStatusValueDTO glwellstatus)
        {
            List<string> ialmlist = glwellstatus.AlarmMessage.ToList();
            CollectionAssert.AllItemsAreNotNull(ialmlist);
            CollectionAssert.AllItemsAreUnique(ialmlist);
            int i = 0;
            foreach (string expalm in icollection)
            {
                foreach (string actalm in ialmlist)
                {
                    if (actalm.Contains(expalm))
                    {
                        i++;
                        break;
                    }
                }

            }
            Trace.WriteLine($"Count of alarms from Well Status: {ialmlist.Count} against Expected count of {icollection.Count}");
            Trace.WriteLine($"Match count with agaist Expected alarms: {i}");
            Assert.AreEqual(icollection.Count, i, "Expected Operating alarms was not found in actual alarm list ");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void NF_OilAlarmMessage()
        {
            var modelFileName = "Black Oil - IPR Auto Tuning.wflx";
            var wellType = WellTypeId.NF;
            var options = new ModelFileOptionDTO
            {
                CalibrationMethod = CalibrationMethodId.LFactor,
                OptionalUpdate = new long[]
                    {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        (long) OptionalUpdates.UpdateGOR_CGR
                    }
            };

            Trace.WriteLine("Testing model: " + modelFileName);

            WellDTO well = AddNonRRLWellGeneralTab("NFWWELL_", wellType, WellFluidType.BlackOil, "N/A", "NA");
            AddNonRRLModelFile(well, modelFileName, options.CalibrationMethod, options.OptionalUpdate);
            Trace.WriteLine("Well Added Successfully");

            WellTestDTO testData = new WellTestDTO
            {
                WellId = well.Id,
                SPTCodeDescription = "RepresentativeTest",
                CalibrationMethod = options.CalibrationMethod,
                WellTestType = WellTestType.WellTest,
                AverageTubingPressure = 1000,
                AverageTubingTemperature = 100,
                Gas = 375,
                Water = 250,
                Oil = 750,
                ChokeSize = 32,
                FlowLinePressure = 1000,
                SeparatorPressure = 1000,
                TestDuration = 24,
                SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime()
            };
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));

            VerificationOfAlarmMessage(well, testData, "Oil", "Condensate");
            VerificationOfAlarmMessageByOperatingLimits(well, testData, "Oil", "Condensate");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void NF_CondensateAlarmMessage()
        {
            var modelFileName = "Condensate Gas - IPR Auto Tuning.wflx";
            var wellType = WellTypeId.NF;
            var options = new ModelFileOptionDTO
            {
                CalibrationMethod = CalibrationMethodId.DarcyFlowCoefficient,
                OptionalUpdate = new long[]
                    {
                        (long) OptionalUpdates.UpdateWCT_WGR
                    }
            };

            Trace.WriteLine("Testing model: " + modelFileName);

            WellDTO well = AddNonRRLWellGeneralTab("NFWWELL_", wellType, WellFluidType.Condensate, "N/A", "NA");
            AddNonRRLModelFile(well, modelFileName, options.CalibrationMethod, options.OptionalUpdate);
            Trace.WriteLine("Well Added Successfully");

            WellTestDTO testData = new WellTestDTO
            {
                WellId = well.Id,
                SPTCodeDescription = "RepresentativeTest",
                CalibrationMethod = options.CalibrationMethod,
                WellTestType = WellTestType.WellTest,
                AverageTubingPressure = 4000,
                AverageTubingTemperature = 100,
                Gas = 35520,
                Water = 5327.6m,
                Oil = 5327.6m,
                ChokeSize = 32,
                FlowLinePressure = 4000,
                SeparatorPressure = 4000,
                TestDuration = 24,
                SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime()
            };
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));


            VerificationOfAlarmMessage(well, testData, "Condensate", "Oil");
            VerificationOfAlarmMessageByOperatingLimits(well, testData, "Condensate", "Oil");

        }

        public void VerificationOfAlarmMessage(WellDTO well, WellTestDTO testData, string expectedAlarm, string unexpectedAlarm)
        {
            //Changing  the UIS value of UDC  for Oil/Condesate Rate from CygNet using API calls
            FacilityTag facilityTag = new FacilityTag(s_site, s_cvsService, well.FacilityId);
            SetPointValue(facilityTag, "ROIL", (testData.Oil * 0.5m).ToString(), DateTime.Now);

            //Sending Scan command
            SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());

            //Retrieving well status
            UnitsValuesPairDTO<object, object> wellStatus = SurveillanceServiceClient.GetWellStatusData(well.Id.ToString());

            var value = wellStatus.Value as NFWellStatusValueDTO;

            //Verifying Whether Low Condensate rate Alarm is generated or not
            Assert.IsTrue(value.AlarmMessage.Contains($"Low {expectedAlarm} Rate"), $"Low {expectedAlarm} Rate Alarm is not generated");

            //Verifying that Low Oil rate Alarm is not generated
            Assert.IsFalse(value.AlarmMessage.Contains($"Low {unexpectedAlarm} Rate"), $"Incorrect Alarm Triggered. Instead, Low {expectedAlarm} Alarm should be generated");

            Trace.WriteLine($"Verification completed for Low {expectedAlarm} Rate");

            //Changing  the UIS value of UDC  for Oil/Condesate Rate from CygNet using API calls
            facilityTag = new FacilityTag(s_site, s_cvsService, well.FacilityId);
            SetPointValue(facilityTag, "ROIL", (testData.Oil * 1.5m).ToString(), DateTime.Now);

            //Sending Scan command
            SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());

            //Retrieving well status
            wellStatus = SurveillanceServiceClient.GetWellStatusData(well.Id.ToString());

            value = wellStatus.Value as NFWellStatusValueDTO;

            Assert.IsTrue(value.AlarmMessage.Contains($"High {expectedAlarm} Rate"), $"High {expectedAlarm} Rate Alarm is not generated");

            Assert.IsFalse(value.AlarmMessage.Contains($"High {unexpectedAlarm} Rate"), $"Incorrect Alarm Triggered. Instead, High {expectedAlarm} Alarm should be generated");
            Trace.WriteLine($"Verification completed for High {expectedAlarm} Rate");

            WellTestDataService.DeleteWellTestData(WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString()).First().Id.ToString());
        }

        public void VerificationOfAlarmMessageByOperatingLimits(WellDTO well, WellTestDTO testData, string expectedAlarm, string unexpectedAlarm)
        {
            //Creating WellGroupStatusQuery DTO to get Group Status data for NF
            WellFilterDTO wellbyFilters = WellService.GetWellFilter(null);
            WellGroupStatusQueryDTO Query = new WellGroupStatusQueryDTO();
            Query.WellType = well.WellType;
            Query.WellFilter = wellbyFilters;


            // --------------------Now doing Condensate/Oil rate Alarm verification using Operating Limis---------------------------------
            //Starting verification for Low Condensate/Oil Rate Alarm
            //retrieving NF Group Status data
            WellGroupStatusDTO<object, object> WellGroups1 = SurveillanceServiceClient.GetWellGroupStatus(Query);
            WellGroups1 = SurveillanceServiceClient.GetWellGroupStatus(Query);
            object[] wellsStatus = WellGroups1.Status;

            //Creating WellTest using WellTestDTO provided in parameter
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));

            //Sending Scan command
            SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());

            double minOilRate = 0;
            double maxOilRate = 0;
            for (int i = 0; i < wellsStatus.Length; i++)
            {
                var wellStatusValue = wellsStatus[i] as NFWellStatusValueDTO;
                if (wellStatusValue.WellName == well.Name)
                {
                    minOilRate = (double)wellStatusValue.OilRateMeasured * 2;
                    maxOilRate = (double)wellStatusValue.OilRateMeasured * 4;
                }
            }

            AddWellSettingWithDoubleValues(well.Id, "Min Oil Rate Operating Limit", minOilRate);
            AddWellSettingWithDoubleValues(well.Id, "Max Oil Rate Operating Limit", maxOilRate);

            //Sending Scan command
            SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());

            //Retrieving well status
            UnitsValuesPairDTO<object, object> wellStatus = SurveillanceServiceClient.GetWellStatusData(well.Id.ToString());

            var value = wellStatus.Value as NFWellStatusValueDTO;

            Assert.IsTrue(value.AlarmMessage.Contains($"Low {expectedAlarm} Rate"), $"Low {expectedAlarm} Rate Alarm is not generated");

            Assert.IsFalse(value.AlarmMessage.Contains($"Low {unexpectedAlarm} Rate"), $"Incorrect Alarm Triggered. Instead, Low {expectedAlarm} Alarm should be generated");
            Trace.WriteLine($"Verification completed for Low {expectedAlarm} Rate");

            WellTestDataService.DeleteWellTestData(WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString()).First().Id.ToString());
            DeleteWellSettings(well.Id, "Min Oil Rate Operating Limit");
            DeleteWellSettings(well.Id, "Max Oil Rate Operating Limit");

            //Now starting verification for High Condensate/Oil rate
            WellGroups1 = SurveillanceServiceClient.GetWellGroupStatus(Query);
            wellsStatus = WellGroups1.Status;

            //Creating WellTest using WellTestDTO provided in parameter
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));

            for (int i = 0; i < wellsStatus.Length; i++)
            {
                var wellStatusValue = wellsStatus[i] as NFWellStatusValueDTO;
                if (wellStatusValue.WellName == well.Name)
                {
                    minOilRate = (double)wellStatusValue.OilRateMeasured * 0.5;
                    maxOilRate = (double)wellStatusValue.OilRateMeasured * 0.75;
                }
            }

            AddWellSettingWithDoubleValues(well.Id, "Min Oil Rate Operating Limit", minOilRate);
            AddWellSettingWithDoubleValues(well.Id, "Max Oil Rate Operating Limit", maxOilRate);

            //Sending Scan command
            bool scanStatus = SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
            Assert.IsTrue(scanStatus, "Scan command failed");

            //Retrieving well status
            wellStatus = SurveillanceServiceClient.GetWellStatusData(well.Id.ToString());

            value = wellStatus.Value as NFWellStatusValueDTO;

            Assert.IsTrue(value.AlarmMessage.Contains($"High {expectedAlarm} Rate"), $"High {expectedAlarm} Rate Alarm is not generated");

            Assert.IsFalse(value.AlarmMessage.Contains($"High {unexpectedAlarm} Rate"), $"Incorrect Alarm Triggered. Instead, High {expectedAlarm} Alarm should be generated");
            Trace.WriteLine($"Verification completed for High {expectedAlarm} Rate");

            WellTestDataService.DeleteWellTestData(WellTestDataService.GetAllValidWellTestByWellId(well.Id.ToString()).First().Id.ToString());
            DeleteWellSettings(well.Id, "Min Oil Rate Operating Limit");
            DeleteWellSettings(well.Id, "Max Oil Rate Operating Limit");

        }


        public void AddWell_NonRRL(string facility_tag, WellTypeId wID, bool scanWells = true)
        {
            WellDTO[] allwells = WellService.GetAllWells();

            int i = allwells.Count() + 1;

            int welCount = 0;

            if (welCount == 0)
            {
                _addedWells = new List<WellDTO>();

                string wellName = facility_tag;

                WellDTO well = SetDefaultFluidType(new WellDTO()
                {
                    Name = wellName,
                    FacilityId = wellName,
                    DataConnection = GetDefaultCygNetDataConnection(),
                    WellType = wID,
                    Lease = "Lease_" + i.ToString("D2"),
                    Foreman = "Foreman" + i.ToString("D2"),
                    Field = "Field" + i.ToString("D2"),
                    Engineer = "Engineer" + i.ToString("D2"),
                    GaugerBeat = "GaugerBeat" + i.ToString("D2"),
                    GeographicRegion = "GeographicRegion" + i.ToString("D2"),
                    welUserDef01 = "State_" + i.ToString("D2"),
                    welUserDef02 = "User_" + i.ToString("D2"),
                    IntervalAPI = "IntervalAPI" + i.ToString("D2"),
                    SubAssemblyAPI = "SubAssemblyAPI" + i.ToString("D2"),
                    AssemblyAPI = "AssemblyAPI" + i.ToString("D2"),
                    CommissionDate = DateTime.Today.AddYears(-2)
                });
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
                WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(well.Name));
                Assert.IsNotNull(addedWell, "Failed to get added well.");
                _wellsToRemove.Add(addedWell);
                _addedWells.Add(addedWell);
                if (scanWells)
                {
                    SurveillanceService.IssueCommandForSingleWell(addedWell.Id, WellCommand.DemandScan.ToString());
                }
            }
        }

        public void GetWellGroupStatus_NONRRL(string facilty_tag, WellTypeId wellType)
        {
            List<WellDTO> wells = null;
            wells = WellService.GetAllWells().ToList();
            var newGroup = new WellGroupStatusQueryDTO();

            //Get well Filters
            WellFilterDTO filters = WellService.GetWellFilter(null);

            //No Filters Selected
            WellFilterDTO wellbyFilters = new WellFilterDTO();
            newGroup.WellFilter = wellbyFilters;
            var wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus(newGroup);
            Assert.AreEqual(0, wellsbyFilter.TotalRecords);

            //All Filters Selected
            var allGroup = new WellGroupStatusQueryDTO();
            allGroup.WellType = wellType;
            WellFilterDTO wellbyFilter = new WellFilterDTO();
            wellbyFilter.welEngineerValues = filters?.welEngineerValues;
            wellbyFilter.welFieldNameValues = filters?.welFieldNameValues;
            wellbyFilter.welForemanValues = filters?.welForemanValues;
            wellbyFilter.welGaugerBeatValues = filters?.welGaugerBeatValues;
            wellbyFilter.welGeographicRegionValues = filters?.welGeographicRegionValues;
            wellbyFilter.welLeaseNameValues = filters?.welLeaseNameValues;
            wellbyFilter.welUserDef01Values = filters?.welUserDef01Values;
            wellbyFilter.welUserDef02Values = filters?.welUserDef02Values;
            wellbyFilter.welFK_r_WellTypeValues = filters?.welFK_r_WellTypeValues;
            allGroup.WellFilter = wellbyFilter;
            var wellsbyFilter_all = SurveillanceServiceClient.GetWellGroupStatus(allGroup);

            List<WellFilterValueDTO> value = new List<WellFilterValueDTO>();
            WellDTO[] well1 = new WellDTO[] { };
            if (facilty_tag == "ESP_")
            {
                value.Add(new WellFilterValueDTO() { Value = WellTypeId.ESP.ToString() });
                filters.welFK_r_WellTypeValues = value;
                well1 = WellService.GetWellsByFilter(filters);
                var welTypeGroup = new WellGroupStatusQueryDTO();
                welTypeGroup.WellFilter = filters;
                //welTypeGroup.PageSize = 1;
                welTypeGroup.TotalRecords = 1;
                welTypeGroup.WellType = WellTypeId.ESP;
                var welFK_r_WellTypeValues = SurveillanceServiceClient.GetWellGroupStatus(welTypeGroup);
                var result = SurveillanceServiceClient.GetWellStatusData<ESPWellStatusUnitDTO, ESPWellStatusValueDTO>(well1[0].Id.ToString());
                Assert.AreEqual(1, welFK_r_WellTypeValues.TotalRecords);
                //Assert.AreEqual(1, welFK_r_WellTypeValues.PageSize);

                var espWellStatusValue = result.Value;
                bool flag = false;
                foreach (string msg in espWellStatusValue.AlarmMessage)
                {
                    if (msg.Contains("FAILED"))
                        flag = true;
                }
                if (!flag)
                    Assert.AreEqual(espWellStatusValue.LastScanTime, espWellStatusValue.LastGoodScanTime, "Scan times should match for well " + espWellStatusValue.WellName);
                Assert.IsNotNull(espWellStatusValue.CasingPressure, "Expected non-null casing pressure from well " + espWellStatusValue.WellName);
                Assert.IsNotNull(espWellStatusValue.PumpDischargePressure, "Expected non-null pump discharge pressure from well " + espWellStatusValue.WellName);
                Assert.IsNotNull(espWellStatusValue.PumpIntakePressure, "Expected non-null pump intake pressure from well " + espWellStatusValue.WellName);
                Assert.IsNotNull(espWellStatusValue.TubingPressure, "Expected non-null tubing pressure from well " + espWellStatusValue.WellName);
            }
            else if (facilty_tag == "GINJ_")
            {
                value.Add(new WellFilterValueDTO() { Value = WellTypeId.GInj.ToString() });
                filters.welFK_r_WellTypeValues = value;
                well1 = WellService.GetWellsByFilter(filters);
                var welTypeGroup = new WellGroupStatusQueryDTO();
                welTypeGroup.WellFilter = filters;
                //welTypeGroup.PageSize = 1;
                welTypeGroup.TotalRecords = 1;
                welTypeGroup.WellType = WellTypeId.GInj;
                var welFK_r_WellTypeValues = SurveillanceServiceClient.GetWellGroupStatus(welTypeGroup);
                Assert.AreEqual(1, welFK_r_WellTypeValues.TotalRecords);
                //Assert.AreEqual(1, welFK_r_WellTypeValues.PageSize);

                var result = SurveillanceServiceClient.GetWellStatusData<GIWellStatusUnitDTO, GIWellStatusValueDTO>(well1[0].Id.ToString());
                var giWwellStatusValue = result.Value;
                bool flag = false;
                foreach (string msg in giWwellStatusValue.AlarmMessage)
                {
                    if (msg.Contains("FAILED"))
                        flag = true;
                }
                if (!flag)
                    Assert.AreEqual(giWwellStatusValue.LastScanTime, giWwellStatusValue.LastGoodScanTime, "Scan times should match for well " + giWwellStatusValue.WellName);
                Assert.IsNotNull(giWwellStatusValue.DHGaugePressure, "Expected non-null downhole gauge pressure from well " + giWwellStatusValue.WellName);
                Assert.IsNotNull(giWwellStatusValue.TubingHeadTemperature, "Expected non-null tubing head temperature from well " + giWwellStatusValue.WellName);
                Assert.IsNotNull(giWwellStatusValue.TubingPressure, "Expected non-null tubing pressure from well " + giWwellStatusValue.WellName);
            }
            else if (facilty_tag == "GL_")
            {
                value.Add(new WellFilterValueDTO() { Value = WellTypeId.GLift.ToString() });
                filters.welFK_r_WellTypeValues = value;
                well1 = WellService.GetWellsByFilter(filters);
                var welTypeGroup = new WellGroupStatusQueryDTO();
                welTypeGroup.WellFilter = filters;
                //welTypeGroup.PageSize = 1;
                welTypeGroup.TotalRecords = 1;
                welTypeGroup.WellType = WellTypeId.GLift;
                var welFK_r_WellTypeValues = SurveillanceServiceClient.GetWellGroupStatus(welTypeGroup);
                Assert.AreEqual(1, welFK_r_WellTypeValues.TotalRecords);
                //Assert.AreEqual(1, welFK_r_WellTypeValues.PageSize);

                var result = SurveillanceServiceClient.GetWellStatusData<GLWellStatusUnitDTO, GLWellStatusValueDTO>(well1[0].Id.ToString());
                var glWwellStatusValue = result.Value;
                bool flag = false;
                foreach (string msg in glWwellStatusValue.AlarmMessage)
                {
                    if (msg.Contains("FAILED"))
                        flag = true;
                }
                if (!flag)
                    Assert.AreEqual(glWwellStatusValue.LastScanTime, glWwellStatusValue.LastGoodScanTime, "Scan times should match for well " + glWwellStatusValue.WellName);
                Assert.IsNotNull(glWwellStatusValue.DHGaugePressure, "Expected non-null downhole gauge pressure from well " + glWwellStatusValue.WellName);
                Assert.IsNotNull(glWwellStatusValue.GasInjectionRate, "Expected non-null gas injection rate from well " + glWwellStatusValue.WellName);
                Assert.IsNotNull(glWwellStatusValue.TubingHeadTemperature, "Expected non-null tubing head temperature from well " + glWwellStatusValue.WellName);
                Assert.IsNotNull(glWwellStatusValue.TubingPressure, "Expected non-null tubing pressure from well " + glWwellStatusValue.WellName);
                Assert.IsNotNull(glWwellStatusValue.GasRateMeasured, "Expected non-null Metered Gas Value from well " + glWwellStatusValue.WellName);
                Assert.IsNotNull(glWwellStatusValue.OilRateMeasured, "Expected non-null Metered Oil Value from well " + glWwellStatusValue.WellName);
                Assert.IsNotNull(glWwellStatusValue.WaterRateMeasured, "Expected non-null Metered Water Value from well " + glWwellStatusValue.WellName);
            }
            else if (facilty_tag == "NFW_")
            {
                value.Add(new WellFilterValueDTO() { Value = WellTypeId.NF.ToString() });
                filters.welFK_r_WellTypeValues = value;
                well1 = WellService.GetWellsByFilter(filters);
                var welTypeGroup = new WellGroupStatusQueryDTO();
                welTypeGroup.WellFilter = filters;
                //welTypeGroup.PageSize = 1;
                welTypeGroup.TotalRecords = 1;
                welTypeGroup.WellType = WellTypeId.NF;
                var welFK_r_WellTypeValues = SurveillanceServiceClient.GetWellGroupStatus(welTypeGroup);
                Assert.AreEqual(1, welFK_r_WellTypeValues.TotalRecords);
                //Assert.AreEqual(1, welFK_r_WellTypeValues.PageSize);

                var result = SurveillanceServiceClient.GetWellStatusData<NFWellStatusUnitDTO, NFWellStatusValueDTO>(well1[0].Id.ToString());
                var nfWwellStatusValue = result.Value;
                bool flag = false;
                foreach (string msg in nfWwellStatusValue.AlarmMessage)
                {
                    if (msg.Contains("FAILED"))
                        flag = true;
                }
                if (!flag)
                    Assert.AreEqual(nfWwellStatusValue.LastScanTime, nfWwellStatusValue.LastGoodScanTime, "Scan times should match for well " + nfWwellStatusValue.WellName);
                Assert.IsNotNull(nfWwellStatusValue.DHGaugePressure, "Expected non-null downhole gauge pressure from well " + nfWwellStatusValue.WellName);
                Assert.IsNotNull(nfWwellStatusValue.TubingHeadTemperature, "Expected non-null tubing head temperature from well " + nfWwellStatusValue.WellName);
                Assert.IsNotNull(nfWwellStatusValue.TubingPressure, "Expected non-null tubing pressure from well " + nfWwellStatusValue.WellName);
            }
            else if (facilty_tag == "WINJ_")
            {
                value.Add(new WellFilterValueDTO() { Value = WellTypeId.WInj.ToString() });
                filters.welFK_r_WellTypeValues = value;
                well1 = WellService.GetWellsByFilter(filters);
                var welTypeGroup = new WellGroupStatusQueryDTO();
                welTypeGroup.WellFilter = filters;
                //welTypeGroup.PageSize = 1;
                welTypeGroup.TotalRecords = 1;
                welTypeGroup.WellType = WellTypeId.WInj;
                var welFK_r_WellTypeValues = SurveillanceServiceClient.GetWellGroupStatus(welTypeGroup);
                Assert.AreEqual(1, welFK_r_WellTypeValues.TotalRecords);
                //Assert.AreEqual(1, welFK_r_WellTypeValues.PageSize);

                var result = SurveillanceServiceClient.GetWellStatusData<WIWellStatusUnitDTO, WIWellStatusValueDTO>(well1[0].Id.ToString());
                var wiWwellStatusValue = result.Value;
                bool flag = false;
                foreach (string msg in wiWwellStatusValue.AlarmMessage)
                {
                    if (msg.Contains("FAILED"))
                        flag = true;
                }
                if (!flag)
                    Assert.AreEqual(wiWwellStatusValue.LastScanTime, wiWwellStatusValue.LastGoodScanTime, "Scan times should match for well " + wiWwellStatusValue.WellName);
                Assert.IsNotNull(wiWwellStatusValue.DHGaugePressure, "Expected non-null downhole gauge pressure from well " + wiWwellStatusValue.WellName);
                Assert.IsNotNull(wiWwellStatusValue.TubingHeadTemperature, "Expected non-null tubing head temperature from well " + wiWwellStatusValue.WellName);
                Assert.IsNotNull(wiWwellStatusValue.TubingPressure, "Expected non-null tubing pressure from well " + wiWwellStatusValue.WellName);
            }
            else if (facilty_tag == "WAGI_")
            {
                value.Add(new WellFilterValueDTO() { Value = WellTypeId.WGInj.ToString() });
                filters.welFK_r_WellTypeValues = value;
                well1 = WellService.GetWellsByFilter(filters);
                var welTypeGroup = new WellGroupStatusQueryDTO();
                welTypeGroup.WellFilter = filters;
                //welTypeGroup.PageSize = 1;
                welTypeGroup.TotalRecords = 1;
                welTypeGroup.WellType = WellTypeId.WGInj;
                var welFK_r_WellTypeValues = SurveillanceServiceClient.GetWellGroupStatus(welTypeGroup);
                Assert.AreEqual(1, welFK_r_WellTypeValues.TotalRecords);
                //Assert.AreEqual(1, welFK_r_WellTypeValues.PageSize);

                var result = SurveillanceServiceClient.GetWellStatusData<WAGWellStatusUnitDTO, WAGWellStatusValueDTO>(well1[0].Id.ToString());
                var wAGWellStatusValue = result.Value;
                bool flag = false;
                foreach (string msg in wAGWellStatusValue.AlarmMessage)
                {
                    if (msg.Contains("FAILED"))
                        flag = true;
                }
                if (!flag)
                    Assert.AreEqual(wAGWellStatusValue.LastScanTime, wAGWellStatusValue.LastGoodScanTime, "Scan times should match for well " + wAGWellStatusValue.WellName);
                Assert.IsNotNull(wAGWellStatusValue.AlarmStatus, "Expected non-null Alarm Status from well " + wAGWellStatusValue.WellName);
                Assert.IsNotNull(wAGWellStatusValue.DifferentialPressure, "Expected non-null Differential Pressure from well " + wAGWellStatusValue.WellName);
                Assert.IsNotNull(wAGWellStatusValue.BatteryVoltage, "Expected non-null Battery Voltage from well " + wAGWellStatusValue.WellName);
                Assert.IsNotNull(wAGWellStatusValue.CasingPressure, "Expected non-null Casing Pressure from well " + wAGWellStatusValue.WellName);
                Assert.IsNotNull(wAGWellStatusValue.FlowLinePressure, "Expected non-null Flow Line Pressure from well " + wAGWellStatusValue.WellName);
                Assert.IsNotNull(wAGWellStatusValue.InjectionState, "Expected non-null InjectionState from well " + wAGWellStatusValue.WellName);
                Assert.IsNotNull(wAGWellStatusValue.InjectionPressureSetpoint, "Expected non-null Alarm Status from well " + wAGWellStatusValue.WellName);
                Assert.IsNotNull(wAGWellStatusValue.SwitchDate, "Expected non-null Switch date from well " + wAGWellStatusValue.WellName);
                Assert.IsNotNull(wAGWellStatusValue.InjectionPressure, "Expected non-null Injecttion Pressure from well " + wAGWellStatusValue.WellName);
                System.Threading.Thread.Sleep(5000);
                if (wAGWellStatusValue.InjectionState.ToString() == "H2O")
                {
                    Assert.IsNotNull(wAGWellStatusValue.WaterInjectionRate, "Expected non-null water Injection rate from well " + wAGWellStatusValue.WellName);
                    Assert.IsNotNull(wAGWellStatusValue.WaterSuggestedRateSP, "Expected non-null water suggested rate from well " + wAGWellStatusValue.WellName);
                }
                else
                {
                    Assert.IsNotNull(wAGWellStatusValue.GasInjectionRate, "Expected non-null gas Injection rate from well " + wAGWellStatusValue.WellName);
                    Assert.IsNotNull(wAGWellStatusValue.GasSuggestedRateSP, "Expected non-null gas suggested rate from well " + wAGWellStatusValue.WellName);
                }
            }
        }

        public void GetPGLWellGroupStatus_NONRRL(WellTypeId wellType, int WellCount)
        {
            List<WellDTO> wells = null;
            wells = WellService.GetAllWells().ToList();
            var newGroup = new WellGroupStatusQueryDTO();

            //Get well Filters
            WellFilterDTO filters = WellService.GetWellFilter(null);

            //No Filters Selected
            WellFilterDTO wellbyFilters = new WellFilterDTO();
            newGroup.WellFilter = wellbyFilters;
            var wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus(newGroup);
            Assert.AreEqual(0, wellsbyFilter.TotalRecords);
            WellFilterDTO wellbyFilter = new WellFilterDTO();
            WellGroupStatusQueryDTO Query = new WellGroupStatusQueryDTO();
            Query.WellType = WellTypeId.PLift;
            //Query.OrderField = "wellName";
            //Query.CurrentPage = 1;
            //Query.PageSize = 20;
            Query.WellFilter = wellbyFilters;

            var welFK_r_WellTypeValues = SurveillanceServiceClient.GetWellGroupStatus(Query);
            Assert.AreEqual(WellCount, welFK_r_WellTypeValues.TotalRecords);
            //Assert.AreEqual(20, welFK_r_WellTypeValues.PageSize);
            object[] wellsStatus = welFK_r_WellTypeValues.Status;
            var unitkeys = welFK_r_WellTypeValues.Units;

            for (int i = 0; i < welFK_r_WellTypeValues.TotalRecords; i++)
            {
                var pglWwellStatusValue = wellsStatus[i] as PGLWellStatusValueDTO;
                Trace.WriteLine("***************For WellName: ******" + pglWwellStatusValue.WellName);
                Trace.WriteLine(string.Format(" Casing Pressure:  {0} FlowLine Pressure:  {1} Differnetail Presure {2} Tubing Pressure : {3}  Gas Rate Measured: {4} Load Factor: {5}  ", pglWwellStatusValue.CasingPressure, pglWwellStatusValue.FlowLinePressure, pglWwellStatusValue.DifferentialPressure, pglWwellStatusValue.TubingPressure, pglWwellStatusValue.GasRateMeasured, pglWwellStatusValue.LoadFactor));
                Assert.AreEqual(pglWwellStatusValue.LastScanTime, pglWwellStatusValue.LastGoodScanTime, "Scan times should match for well " + pglWwellStatusValue.WellName);
                Assert.IsNotNull(pglWwellStatusValue.CasingPressure, "Expected non-null Casing Pressure from well " + pglWwellStatusValue.WellName);
                Assert.IsNotNull(pglWwellStatusValue.DifferentialPressure, "Expected non-null Differential Pressure from well " + pglWwellStatusValue.WellName);
                Assert.IsNotNull(pglWwellStatusValue.FlowLinePressure, "Expected non-null Flow LinePressure from well " + pglWwellStatusValue.WellName);
                Assert.IsNotNull(pglWwellStatusValue.TubingPressure, "Expected non-null tubing pressure from well " + pglWwellStatusValue.WellName);
                Assert.IsNotNull(pglWwellStatusValue.GasRateMeasured, "Expected non-null Gas Rate Measured from well " + pglWwellStatusValue.WellName);
                Assert.IsNotNull(pglWwellStatusValue.LoadFactor, "Expected non-null Load Factor from well " + pglWwellStatusValue.WellName);
            }
        }

        public void plungerliftScanWell(string wellname)
        {
            WellDTO addedWell = _addedWells.FirstOrDefault(x => x.Name == wellname);

            var pglwellStatusValue = SurveillanceServiceClient.GetWellStatusData<PGLWellStatusUnitDTO, PGLWellStatusValueDTO>(addedWell.Id.ToString()).Value;
            if (!pglwellStatusValue.AlarmMessageString.Contains("FAILED:"))
            {
                Assert.AreEqual(pglwellStatusValue.LastScanTime, pglwellStatusValue.LastGoodScanTime, "Scan times should match for well " + pglwellStatusValue.WellName);
                Assert.IsNotNull(pglwellStatusValue.CasingPressure, "Expected non-null casing pressure from well " + pglwellStatusValue.WellName);
                Assert.IsNotNull(pglwellStatusValue.FlowLinePressure, "Expected non-null flow line temperature from well " + pglwellStatusValue.WellName);
                Assert.IsNotNull(pglwellStatusValue.TubingPressure, "Expected non-null tubing pressure from well " + pglwellStatusValue.WellName);
                Assert.IsNotNull(pglwellStatusValue.LoadFactor, "Expected non-null Load Factor from well " + pglwellStatusValue.WellName);
                Trace.WriteLine(string.Format(" Casing Pressure:  {0} FlowLine Pressure:  {1}  Tubing Pressure : {2} Load Factor: {3} ", pglwellStatusValue.CasingPressure, pglwellStatusValue.FlowLinePressure, pglwellStatusValue.TubingPressure, pglwellStatusValue.LoadFactor));
            }
            else
            {
                Trace.WriteLine("Well Status for well is FAILED" + pglwellStatusValue.WellName);
            }
        }

        //[TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        //[TestMethod]
        //public void GetAllocationHistory()
        //{
        //    //Add Oil - Well Allocation Group
        //    WellAllocationGroupDTO newWAGOil = new WellAllocationGroupDTO();
        //    newWAGOil.AllocationGroupName = "Oil";
        //    newWAGOil.DataConnection = GetDefaultCygNetDataConnection();
        //    newWAGOil.Phase = Enums.AllocationGroupPhaseId.Oil;
        //    newWAGOil.FacilityId = "OIL_ALLOCATION_G0001";
        //    newWAGOil.UDC = "ALLOCVOD";

        //    //Add Oil
        //    WellService.AddWellAllocationGroup(newWAGOil);
        //    var wellallOil = WellService.GetWellAllocationGroupByPhase(Convert.ToString(Convert.ToInt32(newWAGOil.Phase)));
        //    var wellgrpOil = wellallOil.Where(s => s.Asset == newWAGOil.Asset).FirstOrDefault();

        //    var WAGdto1 = WellService.GetWellAllocationGroup(wellgrpOil.Id.ToString());
        //    Assert.AreEqual(wellgrpOil.Id, WAGdto1.Id);

        //    //Water
        //    WellAllocationGroupDTO newWAGWater = new WellAllocationGroupDTO();
        //    newWAGWater.AllocationGroupName = "Water";
        //    newWAGWater.DataConnection = GetDefaultCygNetDataConnection();
        //    newWAGWater.Phase = Enums.AllocationGroupPhaseId.Water;
        //    newWAGWater.FacilityId = "H2O_ALLOCATION_G0001";
        //    newWAGWater.UDC = "ALLOCVH2OD";

        //    //Add Well Allocation Group - Water
        //    WellService.AddWellAllocationGroup(newWAGWater);
        //    var wellallWater = WellService.GetWellAllocationGroupByPhase(Convert.ToString(Convert.ToInt32(newWAGWater.Phase)));
        //    var wellgrpWater = wellallWater.Where(s => s.Asset == newWAGWater.Asset).FirstOrDefault();

        //    var WAGdto2 = WellService.GetWellAllocationGroup(wellgrpWater.Id.ToString());
        //    Assert.AreEqual(wellgrpWater.Id, WAGdto2.Id);

        //    //Gas
        //    WellAllocationGroupDTO newWAGGas = new WellAllocationGroupDTO();
        //    newWAGGas.AllocationGroupName = "Gas";
        //    newWAGGas.DataConnection = GetDefaultCygNetDataConnection();
        //    newWAGGas.Phase = Enums.AllocationGroupPhaseId.Gas;
        //    newWAGGas.FacilityId = "GAS_ALLOCATION_G0001";
        //    newWAGGas.UDC = "ALLOCVGD";

        //    // Add Well Allocation Group - GAS
        //    WellService.AddWellAllocationGroup(newWAGGas);

        //    var wellallGas = WellService.GetWellAllocationGroupByPhase(Convert.ToString(Convert.ToInt32(newWAGGas.Phase)));
        //    var wellgrpGas = wellallGas.Where(s => s.Asset == newWAGGas.Asset).FirstOrDefault();

        //    var WAGdto3 = WellService.GetWellAllocationGroup(wellgrpGas.Id.ToString());
        //    Assert.AreEqual(wellgrpGas.Id, WAGdto3.Id);

        //    CygNetTrendDTO GasTrend = SurveillanceService.GetDailyAllocationGroupRecord(WAGdto3.Id.ToString(), "1-1-2017", "2-1-2018");
        //    CygNetTrendDTO WaterTrend = SurveillanceService.GetDailyAllocationGroupRecord(WAGdto2.Id.ToString(), "1-1-2017", "2-1-2018");
        //    CygNetTrendDTO OilTrend = SurveillanceService.GetDailyAllocationGroupRecord(WAGdto1.Id.ToString(), "1-1-2017", "2-1-2018");

        //    //Gas Check
        //    Assert.AreEqual("Success", GasTrend.ErrorMessage);
        //    Assert.IsNotNull(GasTrend.PointUDC, "Point UDC is " + GasTrend.PointUDC);
        //    Assert.IsNotNull(GasTrend.Units, "Units for " + GasTrend.PointUDC + "is " + GasTrend.Units);
        //    Assert.IsTrue(GasTrend.PointValues.Length > 0, "No Point Values returned, CygNet does not have data for this point");
        //    //water Check
        //    Assert.AreEqual("Success", WaterTrend.ErrorMessage);
        //    Assert.IsNotNull(WaterTrend.PointUDC, "Point UDC is " + WaterTrend.PointUDC);
        //    Assert.IsNotNull(WaterTrend.Units, "Units for " + WaterTrend.PointUDC + "is " + WaterTrend.Units);
        //    Assert.IsTrue(WaterTrend.PointValues.Length > 0, "No Point Values returned, CygNet does not have data for this point");
        //    //Oil Check
        //    Assert.AreEqual("Success", OilTrend.ErrorMessage);
        //    Assert.IsNotNull(OilTrend.PointUDC, "Point UDC is " + OilTrend.PointUDC);
        //    Assert.IsNotNull(OilTrend.Units, "Units for " + OilTrend.PointUDC + "is " + OilTrend.Units);
        //    Assert.IsTrue(OilTrend.PointValues.Length > 0, "No Point Values returned, CygNet does not have data for this point");

        //    WellService.RemoveWellAllocationGroup(WAGdto1.Id.ToString());
        //    WellService.RemoveWellAllocationGroup(WAGdto2.Id.ToString());
        //    WellService.RemoveWellAllocationGroup(WAGdto3.Id.ToString());
        //}


        /// <summary>
        /// Update Test for FRWM-6172 : API testing subtask for story : 
        /// FRWM-4220 : Production Overview Tile Maps - Allow displaying net change or percentage change & Add Gross
        /// </summary>
        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetProductionTargetTileMap()
        {
            // ******************** Create the well filter ********************
            // We are going to add a sequence of GL wells for this test, all of which will have the Engineer attribue set to "TileMapTestWell".
            // This filter will let us get only those wells out of the database for the test.
            List<WellFilterValueDTO> wellEngineerValue = new List<WellFilterValueDTO>();
            wellEngineerValue.Add(new WellFilterValueDTO { Value = "TileMapTestWell" });

            WellFilterDTO wellFilter = new WellFilterDTO
            {
                welEngineerTitle = "TileMapTestWell",
                welEngineerValues = wellEngineerValue
            };

            TileMapFilterDTO tileMapFilter = new TileMapFilterDTO
            {
                IsNegative = false,
                TopWellCountRequested = 5,
                UsePercentChangeForRank = true,
                WellFilter = wellFilter,
            };

            // The target rate assigned to a well will be compared to this value to determine if the well falls above or below the target rate.
            double? dailyAverage = 300;

            // Add 5 wells with target rates that will fall below the fixed well daily average of 300. The daily average production
            // rates and the target rates are added in AddGLTargetWell.
            AddGLTargetWell("TileMapTestWell", "1", (decimal)50.0, dailyAverage);     // create a well with target rate for oil, water and gas to 50.
            AddGLTargetWell("TileMapTestWell", "2", (decimal)100.0, dailyAverage);    // etc.
            AddGLTargetWell("TileMapTestWell", "3", (decimal)150.0, dailyAverage);
            AddGLTargetWell("TileMapTestWell", "4", (decimal)200.0, dailyAverage);
            AddGLTargetWell("TileMapTestWell", "5", (decimal)250.0, dailyAverage);

            // Add 5 wells with target rates that will fall above the daily average value which is always set to 300 so that half of the
            // test well fall above it and half fall below it. The daily average production rates and the target rates are added in AddGLTargetWell.
            AddGLTargetWell("TileMapTestWell", "6", (decimal)350.0, dailyAverage);
            AddGLTargetWell("TileMapTestWell", "7", (decimal)400.0, dailyAverage);
            AddGLTargetWell("TileMapTestWell", "8", (decimal)450.0, dailyAverage);
            AddGLTargetWell("TileMapTestWell", "9", (decimal)500.0, dailyAverage);
            AddGLTargetWell("TileMapTestWell", "10", (decimal)550.0, dailyAverage);

            ProductionTargetTileMapAndUnitsDTO tileMapDTO = SurveillanceService.GetProductionTargetTileMap(tileMapFilter);

            Assert.IsNotNull(tileMapDTO, "Failed to retrieve group status production tilemap");

            string mismatchBetweenCalculated = "Mismatch between the calculated ";
            string expectedRate = "rate and the expected rate of ";

            // ------------------------------ Gas production ------------------------------

            // There should be 5 wells with production rates below the target rate.
            Assert.AreEqual(5, tileMapDTO.TopGasDifference.Values.Length, "Mismatch between the number of wells determined to be above or below the gas daily average and the number of wells known to be above or below the gas daily average");

            // This well has a target rate of 550 and a production rate of 300. The difference is 250.
            Assert.AreEqual(tileMapDTO.TopGasDifference.Values[0].ProductionChange, 250, mismatchBetweenCalculated + "gas" + expectedRate + "250");

            // This well has a target rate of 500 and a production rate of 300. The difference is 200.
            Assert.AreEqual(tileMapDTO.TopGasDifference.Values[1].ProductionChange, 200, mismatchBetweenCalculated + "gas" + expectedRate + "200");

            // This well has a target rate of 450 and a production rate of 300. The difference is 150.
            Assert.AreEqual(tileMapDTO.TopGasDifference.Values[2].ProductionChange, 150, mismatchBetweenCalculated + "gas" + expectedRate + "150");

            // This well has a target rate of 400 and a production rate of 300. The difference is 100.
            Assert.AreEqual(tileMapDTO.TopGasDifference.Values[3].ProductionChange, 100, mismatchBetweenCalculated + "gas" + expectedRate + "100");

            // This well has a target rate of 350 and a production rate of 300. The difference is 50.
            Assert.AreEqual(tileMapDTO.TopGasDifference.Values[4].ProductionChange, 50, mismatchBetweenCalculated + "gas" + expectedRate + "50");

            // ------------------------------ Oil production ------------------------------

            Assert.AreEqual(5, tileMapDTO.TopOilDifference.Values.Length, "Mismatch between the number of wells determined to be above or below the oil daily average and the number of wells known to be above or below the oil daily average");
            Assert.AreEqual(tileMapDTO.TopOilDifference.Values[0].ProductionChange, 250, mismatchBetweenCalculated + "Oil" + expectedRate + "250");
            Assert.AreEqual(tileMapDTO.TopOilDifference.Values[1].ProductionChange, 200, mismatchBetweenCalculated + "Oil" + expectedRate + "200");
            Assert.AreEqual(tileMapDTO.TopOilDifference.Values[2].ProductionChange, 150, mismatchBetweenCalculated + "Oil" + expectedRate + "150");
            Assert.AreEqual(tileMapDTO.TopOilDifference.Values[3].ProductionChange, 100, mismatchBetweenCalculated + "Oil" + expectedRate + "100");
            Assert.AreEqual(tileMapDTO.TopOilDifference.Values[4].ProductionChange, 50, mismatchBetweenCalculated + "Oil" + expectedRate + "50");

            // ------------------------------ Water production ------------------------------

            Assert.AreEqual(5, tileMapDTO.TopWaterDifference.Values.Length, "Mismatch between the number of wells determined to be above or below the water daily average and the number of wells known to be above or below the water daily average");
            Assert.AreEqual(tileMapDTO.TopWaterDifference.Values[0].ProductionChange, 250, mismatchBetweenCalculated + "Water" + expectedRate + "250");
            Assert.AreEqual(tileMapDTO.TopWaterDifference.Values[1].ProductionChange, 200, mismatchBetweenCalculated + "Water" + expectedRate + "200");
            Assert.AreEqual(tileMapDTO.TopWaterDifference.Values[2].ProductionChange, 150, mismatchBetweenCalculated + "Water" + expectedRate + "150");
            Assert.AreEqual(tileMapDTO.TopWaterDifference.Values[3].ProductionChange, 100, mismatchBetweenCalculated + "Water" + expectedRate + "100");
            Assert.AreEqual(tileMapDTO.TopWaterDifference.Values[4].ProductionChange, 50, mismatchBetweenCalculated + "Water" + expectedRate + "50");

            // ------------------------------ Liquid production ------------------------------

            Assert.AreEqual(5, tileMapDTO.TopLiquidDifference.Values.Length, "Mismatch between the number of wells determined to be above or below the Liquid daily average and the number of wells known to be above or below the Liquid daily average");
            Assert.AreEqual(tileMapDTO.TopLiquidDifference.Values[0].ProductionChange, 500, mismatchBetweenCalculated + "Water" + expectedRate + "250");
            Assert.AreEqual(tileMapDTO.TopLiquidDifference.Values[1].ProductionChange, 400, mismatchBetweenCalculated + "Water" + expectedRate + "200");
            Assert.AreEqual(tileMapDTO.TopLiquidDifference.Values[2].ProductionChange, 300, mismatchBetweenCalculated + "Water" + expectedRate + "150");
            Assert.AreEqual(tileMapDTO.TopLiquidDifference.Values[3].ProductionChange, 200, mismatchBetweenCalculated + "Water" + expectedRate + "100");
            Assert.AreEqual(tileMapDTO.TopLiquidDifference.Values[4].ProductionChange, 100, mismatchBetweenCalculated + "Water" + expectedRate + "50");


            // We also know that the sum of the differences from 300 for all 3 production rates is 750.
            Assert.AreEqual(750, tileMapDTO.TotalGasDifference, "Mismatch between the determined total gas difference and the known total gas difference");
            Assert.AreEqual(750, tileMapDTO.TotalWaterDifference, "Mismatch between the determined total water difference and the known total water difference");
            Assert.AreEqual(750, tileMapDTO.TotalOilDifference, "Mismatch between the determined total oil difference and the known total oil difference");
            Assert.AreEqual(1500, tileMapDTO.TotalLiquidDifference, "Mismatch between the determined total Liquid difference and the known total Liquid difference");

            // Get the wells with production rate above the target rate with decrease 
            tileMapFilter.IsNegative = true;
            tileMapFilter.UsePercentChangeForRank = true;

            // Get the tile map of wells with production rate above the target rate
            ProductionTargetTileMapAndUnitsDTO tileMapDTO2 = SurveillanceService.GetProductionTargetTileMap(tileMapFilter);


            // ------------------------------ Gas production ------------------------------

            // There should be 5 wells with production rates below the target rate.
            Assert.AreEqual(5, tileMapDTO2.TopGasDifference.Values.Length, "Mismatch between the number of wells determined to be above or below the gas daily average and the number of wells known to be above or below the gas daily average");

            // This well has a target rate of 50 and a production rate of 300. The difference is 250.
            Assert.AreEqual(tileMapDTO2.TopGasDifference.Values[0].ProductionChange, 250, mismatchBetweenCalculated + "gas" + expectedRate + "250");

            // This well has a target rate of 100 and a production rate of 300. The difference is 200.
            Assert.AreEqual(tileMapDTO2.TopGasDifference.Values[1].ProductionChange, 200, mismatchBetweenCalculated + "gas" + expectedRate + "200");

            // This well has a target rate of 150 and a production rate of 300. The difference is 150.
            Assert.AreEqual(tileMapDTO2.TopGasDifference.Values[2].ProductionChange, 150, mismatchBetweenCalculated + "gas" + expectedRate + "150");

            // This well has a target rate of 200 and a production rate of 300. The difference is 100.
            Assert.AreEqual(tileMapDTO2.TopGasDifference.Values[3].ProductionChange, 100, mismatchBetweenCalculated + "gas" + expectedRate + "100");

            // This well has a target rate of 250 and a production rate of 300. The difference is 50.
            Assert.AreEqual(tileMapDTO2.TopGasDifference.Values[4].ProductionChange, 50, mismatchBetweenCalculated + "gas" + expectedRate + "50");

            string[] value = { "-45.45 %", "-40 %", "-33.33 %", "-25 %", "-14.29 %" };
            VerifyPercentageTileMap(tileMapDTO2, value, mismatchBetweenCalculated, expectedRate);
            // ------------------------------ Oil production ------------------------------

            Assert.AreEqual(5, tileMapDTO2.TopOilDifference.Values.Length, "Mismatch between the number of wells determined to be above or below the oil daily average and the number of wells known to be above or below the oil daily average");
            Assert.AreEqual(tileMapDTO2.TopOilDifference.Values[0].ProductionChange, 250, mismatchBetweenCalculated + "Oil" + expectedRate + "250");
            Assert.AreEqual(tileMapDTO2.TopOilDifference.Values[1].ProductionChange, 200, mismatchBetweenCalculated + "Oil" + expectedRate + "200");
            Assert.AreEqual(tileMapDTO2.TopOilDifference.Values[2].ProductionChange, 150, mismatchBetweenCalculated + "Oil" + expectedRate + "150");
            Assert.AreEqual(tileMapDTO2.TopOilDifference.Values[3].ProductionChange, 100, mismatchBetweenCalculated + "Oil" + expectedRate + "100");
            Assert.AreEqual(tileMapDTO2.TopOilDifference.Values[4].ProductionChange, 50, mismatchBetweenCalculated + "Oil" + expectedRate + "50");

            // ------------------------------ Water production ------------------------------

            Assert.AreEqual(5, tileMapDTO2.TopWaterDifference.Values.Length, "Mismatch between the number of wells determined to be above or below the water daily average and the number of wells known to be above or below the water daily average");
            Assert.AreEqual(tileMapDTO2.TopWaterDifference.Values[0].ProductionChange, 250, mismatchBetweenCalculated + "Water" + expectedRate + "250");
            Assert.AreEqual(tileMapDTO2.TopWaterDifference.Values[1].ProductionChange, 200, mismatchBetweenCalculated + "Water" + expectedRate + "200");
            Assert.AreEqual(tileMapDTO2.TopWaterDifference.Values[2].ProductionChange, 150, mismatchBetweenCalculated + "Water" + expectedRate + "150");
            Assert.AreEqual(tileMapDTO2.TopWaterDifference.Values[3].ProductionChange, 100, mismatchBetweenCalculated + "Water" + expectedRate + "100");
            Assert.AreEqual(tileMapDTO2.TopWaterDifference.Values[4].ProductionChange, 50, mismatchBetweenCalculated + "Water" + expectedRate + "50");


            // ------------------------------ Liquid production ------------------------------

            Assert.AreEqual(5, tileMapDTO2.TopLiquidDifference.Values.Length, "Mismatch between the number of wells determined to be above or below the Liquid daily average and the number of wells known to be above or below the Liquid daily average");
            Assert.AreEqual(tileMapDTO2.TopLiquidDifference.Values[0].ProductionChange, 500, mismatchBetweenCalculated + "Water" + expectedRate + "250");
            Assert.AreEqual(tileMapDTO2.TopLiquidDifference.Values[1].ProductionChange, 400, mismatchBetweenCalculated + "Water" + expectedRate + "200");
            Assert.AreEqual(tileMapDTO2.TopLiquidDifference.Values[2].ProductionChange, 300, mismatchBetweenCalculated + "Water" + expectedRate + "150");
            Assert.AreEqual(tileMapDTO2.TopLiquidDifference.Values[3].ProductionChange, 200, mismatchBetweenCalculated + "Water" + expectedRate + "100");
            Assert.AreEqual(tileMapDTO2.TopLiquidDifference.Values[4].ProductionChange, 100, mismatchBetweenCalculated + "Water" + expectedRate + "50");


            // We also know that the sum of the differences from 300 for all 3 production rates is 750.
            Assert.AreEqual(750, tileMapDTO2.TotalGasDifference, "Mismatch between the determined total gas difference and the known total gas difference");
            Assert.AreEqual(750, tileMapDTO2.TotalWaterDifference, "Mismatch between the determined total water difference and the known total water difference");
            Assert.AreEqual(750, tileMapDTO2.TotalOilDifference, "Mismatch between the determined total oil difference and the known total oil difference");
            Assert.AreEqual(1500, tileMapDTO2.TotalLiquidDifference, "Mismatch between the determined total Liquid difference and the known total Liquid difference");

            //scenario check for increase + volume 
            tileMapFilter.IsNegative = false;
            tileMapFilter.UsePercentChangeForRank = false;

            // Get the tile map of wells with production rate above the target rate
            tileMapDTO2 = SurveillanceService.GetProductionTargetTileMap(tileMapFilter);

            string[] volume = { "250 STB/d", "200 STB/d", "150 STB/d", "100 STB/d", "50 STB/d" };
            VerifyVolumeTileMapOil_Water(tileMapDTO2, volume, mismatchBetweenCalculated, expectedRate);

            string[] gasvolume = { "250 Mscf/d", "200 Mscf/d", "150 Mscf/d", "100 Mscf/d", "50 Mscf/d" };
            VerifyVolumeTileMapGas(tileMapDTO2, gasvolume, mismatchBetweenCalculated, expectedRate);

            string[] liquid = { "500 STB/d", "400 STB/d", "300 STB/d", "200 STB/d", "100 STB/d" };
            VerifyVolumeTileMapLiquid(tileMapDTO2, liquid, mismatchBetweenCalculated, expectedRate);

            //scenario check for decrease + volume 
            tileMapFilter.IsNegative = true;
            tileMapFilter.UsePercentChangeForRank = false;

            // Get the tile map of wells with production rate above the target rate
            tileMapDTO2 = SurveillanceService.GetProductionTargetTileMap(tileMapFilter);

            string[] volume1 = { "-250 STB/d", "-200 STB/d", "-150 STB/d", "-100 STB/d", "-50 STB/d" };
            VerifyVolumeTileMapOil_Water(tileMapDTO2, volume1, mismatchBetweenCalculated, expectedRate);

            string[] gasvolume1 = { "-250 Mscf/d", "-200 Mscf/d", "-150 Mscf/d", "-100 Mscf/d", "-50 Mscf/d" };
            VerifyVolumeTileMapGas(tileMapDTO2, gasvolume1, mismatchBetweenCalculated, expectedRate);

            string[] liquid1 = { "-500 STB/d", "-400 STB/d", "-300 STB/d", "-200 STB/d", "-100 STB/d" };
            VerifyVolumeTileMapLiquid(tileMapDTO2, liquid1, mismatchBetweenCalculated, expectedRate);


            // -------------------- Clean up -------------------

            WellDTO[] wells = WellService.GetWellsByFilter(tileMapFilter.WellFilter);

            foreach (WellDTO well in wells)
            {
                WellDailyAverageValueDTO dto = SurveillanceService.GetWellDailyAverageData(well.Id.ToString());
                SurveillanceService.RemoveWellDailyAverageData(dto.Id.ToString());

                WellTestArrayAndUnitsDTO wellTests = WellTestDataService.GetWellTestDataByWellId(well.Id.ToString());
                if (wellTests?.Values?.Any() == true)
                {
                    WellTestDataService.DeleteWellTestDataGroup(wellTests.Values.Select(t => t.Id).ToArray());
                }

                WellService.RemoveWellTargetRate(well.Id.ToString());
            }
        }

        public void VerifyPercentageTileMap(ProductionTargetTileMapAndUnitsDTO tileMapDTO2, string[] value, string mismatchBetweenCalculated, string expectedRate)
        {

            string[] delim = { "</br>" };

            for (int i = 0; i < tileMapDTO2.TopGasDifference.Values.Count(); i++)
            {
                string[] spltd = tileMapDTO2.TopGasDifference.Values[i].WellLabel.Split(delim, StringSplitOptions.None);
                // This well has a target rate of 50 and a production rate of 300. The difference is 250.
                Assert.AreEqual(spltd[1].ToString(), value[i], mismatchBetweenCalculated + "gas" + expectedRate + value[i]);
                Assert.IsTrue(spltd[1].ToString().Contains("%"), "Production vs Target Percenatage data");

            }

            for (int i = 0; i < tileMapDTO2.TopWaterDifference.Values.Count(); i++)
            {
                string[] spltd = tileMapDTO2.TopWaterDifference.Values[i].WellLabel.Split(delim, StringSplitOptions.None);
                // This well has a target rate of 50 and a production rate of 300. The difference is 250.
                Assert.AreEqual(spltd[1].ToString(), value[i], mismatchBetweenCalculated + "Water" + expectedRate + value[i]);
                Assert.IsTrue(spltd[1].ToString().Contains("%"), "Production vs Target Percenatage data");
            }

            for (int i = 0; i < tileMapDTO2.TopOilDifference.Values.Count(); i++)
            {
                string[] spltd = tileMapDTO2.TopOilDifference.Values[i].WellLabel.Split(delim, StringSplitOptions.None);
                // This well has a target rate of 50 and a production rate of 300. The difference is 250.
                Assert.AreEqual(spltd[1].ToString(), value[i], mismatchBetweenCalculated + "Oil" + expectedRate + value[i]);
                Assert.IsTrue(spltd[1].ToString().Contains("%"), "Production vs Target Percenatage data");
            }

            for (int i = 0; i < tileMapDTO2.TopLiquidDifference.Values.Count(); i++)
            {
                string[] spltd = tileMapDTO2.TopLiquidDifference.Values[i].WellLabel.Split(delim, StringSplitOptions.None);
                // This well has a target rate of 50 and a production rate of 300. The difference is 250.
                Assert.AreEqual(spltd[1].ToString(), value[i], mismatchBetweenCalculated + "Liquid" + expectedRate + value[i]);
                Assert.IsTrue(spltd[1].ToString().Contains("%"), "Production vs Target Percenatage data");
            }



        }

        public void VerifyVolumeTileMapOil_Water(ProductionTargetTileMapAndUnitsDTO tileMapDTO2, string[] value, string mismatchBetweenCalculated, string expectedRate)
        {
            string[] delim = { "</br>" };
            for (int i = 0; i < tileMapDTO2.TopWaterDifference.Values.Count(); i++)
            {
                string[] spltd = tileMapDTO2.TopWaterDifference.Values[i].WellLabel.Split(delim, StringSplitOptions.None);
                // This well has a target rate of 50 and a production rate of 300. The difference is 250.
                Assert.AreEqual(spltd[1].ToString(), value[i], mismatchBetweenCalculated + "Water" + expectedRate + value[i]);
                Assert.IsTrue(spltd[1].ToString().Contains("STB/d"), "Production vs Target Volume data");

            }

            for (int i = 0; i < tileMapDTO2.TopOilDifference.Values.Count(); i++)
            {
                string[] spltd = tileMapDTO2.TopOilDifference.Values[i].WellLabel.Split(delim, StringSplitOptions.None);
                // This well has a target rate of 50 and a production rate of 300. The difference is 250.
                Assert.AreEqual(spltd[1].ToString(), value[i], mismatchBetweenCalculated + "Oil" + expectedRate + value[i]);
                Assert.IsTrue(spltd[1].ToString().Contains("STB/d"), "Production vs Target Volume data");
            }


        }

        public void VerifyVolumeTileMapLiquid(ProductionTargetTileMapAndUnitsDTO tileMapDTO2, string[] value, string mismatchBetweenCalculated, string expectedRate)
        {
            string[] delim = { "</br>" };

            for (int i = 0; i < tileMapDTO2.TopLiquidDifference.Values.Count(); i++)
            {
                string[] spltd = tileMapDTO2.TopLiquidDifference.Values[i].WellLabel.Split(delim, StringSplitOptions.None);
                // This well has a target rate of 50 and a production rate of 300. The difference is 250.
                Assert.AreEqual(spltd[1].ToString(), value[i], mismatchBetweenCalculated + "Liquid" + expectedRate + value[i]);
                Assert.IsTrue(spltd[1].ToString().Contains("STB/d"), "Production vs Target Volume data");

            }
        }

        public void VerifyVolumeTileMapGas(ProductionTargetTileMapAndUnitsDTO tileMapDTO2, string[] value, string mismatchBetweenCalculated, string expectedRate)
        {
            string[] delim = { "</br>" };

            for (int i = 0; i < tileMapDTO2.TopGasDifference.Values.Count(); i++)
            {
                string[] spltd = tileMapDTO2.TopGasDifference.Values[i].WellLabel.Split(delim, StringSplitOptions.None);
                // This well has a target rate of 50 and a production rate of 300. The difference is 250.
                Assert.AreEqual(spltd[1].ToString(), value[i], mismatchBetweenCalculated + "gas" + expectedRate + value[i]);
                Assert.IsTrue(spltd[1].ToString().Contains("Mscf/d"), "Production vs Target Volume data");
            }
        }



        /// <summary>
        /// Add a GL well, its target production rate, its well test and a daily production average
        /// </summary>
        /// <param name="engineer"></param>
        /// <param name="sequence"></param>
        /// <param name="targetValue"></param>
        /// <param name="dailyAverage"></param>
        private void AddGLTargetWell(string engineer, string sequence, decimal targetValue, double? dailyAverage)
        {
            // ******************** Add a TileMapTest Glift Well ********************
            WellConfigurationService.AddWellConfig(new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO
                {
                    Name = engineer + sequence + "_Glift",
                    CommissionDate = DateTime.Today.AddYears(-2),
                    WellType = WellTypeId.GLift,
                    Engineer = engineer
                })
            });

            WellDTO well = WellService.GetWellByName(engineer + sequence + "_Glift");
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            DateTime start = DateTime.UtcNow.Date.AddDays(-1);
            DateTime end = DateTime.UtcNow.Date;

            // Add a production target to well 1
            WellService.AddWellTargetRate(new WellTargetRateDTO
            {
                Id = well.Id,
                WellId = well.Id,
                StartDate = start.AddDays(-10),
                EndDate = end.AddDays(1),

                OilMinimum = targetValue,
                OilLowerBound = targetValue,
                OilTarget = targetValue,
                OilUpperBound = targetValue,
                OilTechnicalLimit = targetValue,

                WaterMinimum = targetValue,
                WaterLowerBound = targetValue,
                WaterTarget = targetValue,
                WaterUpperBound = targetValue,
                WaterTechnicalLimit = targetValue,

                GasMinimum = targetValue,
                GasLowerBound = targetValue,
                GasTarget = targetValue,
                GasUpperBound = targetValue,
                GasTechnicalLimit = targetValue,
            });

            // Get the target we just added from the database and test for integrity
            WellTargetRateDTO[] targetRates = WellService.GetWellTargetRates(well.Id.ToString());
            Assert.IsNotNull(targetRates);

            // We know there is only 1 target in the database, so we can always test element 0 in the targetRates array.
            Assert.AreEqual(targetValue, targetRates[0].OilMinimum, "Retrieved Oil Minimum value does not match test Oil Minimum value");
            Assert.AreEqual(targetValue, targetRates[0].OilMinimum, "Retrieved Oil Lower Bound value does not match test Oil Lower Bound value");
            Assert.AreEqual(targetValue, targetRates[0].OilTarget, "Retrieved Oil Target value does not match test Oil Target value");
            Assert.AreEqual(targetValue, targetRates[0].OilUpperBound, "Retrieved Oil Upper Bound value does not match test Oil Upper Bound value");
            Assert.AreEqual(targetValue, targetRates[0].OilTechnicalLimit, "Retrieved Oil Upper Bound value does not match test Oil Technical Limit value");

            Assert.AreEqual(targetValue, targetRates[0].WaterMinimum, "Retrieved Water Minimum value does not match test Water Minimum value");
            Assert.AreEqual(targetValue, targetRates[0].WaterMinimum, "Retrieved Water Lower Bound value does not match test Water Lower Bound value");
            Assert.AreEqual(targetValue, targetRates[0].WaterTarget, "Retrieved Water Target value does not match test Water Target value");
            Assert.AreEqual(targetValue, targetRates[0].WaterUpperBound, "Retrieved Water Upper Bound value does not match test Water Upper Bound value");
            Assert.AreEqual(targetValue, targetRates[0].WaterTechnicalLimit, "Retrieved Water Upper Bound value does not match test Water Technical Limit value");

            Assert.AreEqual(targetValue, targetRates[0].GasMinimum, "Retrieved Water Minimum value does not match test Gas Minimum value");
            Assert.AreEqual(targetValue, targetRates[0].GasMinimum, "Retrieved Water Lower Bound value does not match test Gas Lower Bound value");
            Assert.AreEqual(targetValue, targetRates[0].GasTarget, "Retrieved Water Target value does not match test Water Gas value");
            Assert.AreEqual(targetValue, targetRates[0].GasUpperBound, "Retrieved Water Upper Bound value does not match test Gas Upper Bound value");
            Assert.AreEqual(targetValue, targetRates[0].GasTechnicalLimit, "Retrieved Water Upper Bound value does not match test Gas Technical Limit value");

            var myassembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(myassembly, "Failed to retrieve assembly");

            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;

            // Add a well test
            WellTestDTO testDataDTO = new WellTestDTO
            {
                WellId = well.Id,
                AverageCasingPressure = 100,
                AverageFluidAbovePump = 1000,
                AverageTubingPressure = 90,
                AverageTubingTemperature = 100,
                Gas = 0,
                GasGravity = (decimal)0.5,
                Oil = 75,
                OilGravity = 34,
                PumpEfficiency = 80,
                PumpIntakePressure = 100,
                PumpingHours = 10,
                SPTCodeDescription = WellTestQuality.RepresentativeTest.ToString(),
                StrokePerMinute = 11,
                TestDuration = 3,
                Water = 35,
                WaterGravity = 1
            };

            testDataDTO.SampleDate = start - TimeSpan.FromDays(100);

            // Add the well test to the database
            WellTestDataService.AddWellTestData(new WellTestAndUnitsDTO(units, testDataDTO));

            // Get it back and validate the results.
            WellTestArrayAndUnitsDTO wellTest = WellTestDataService.GetWellTestDataByWellId(well.Id.ToString());
            Assert.IsNotNull(wellTest, "Failed to retrieve latest well test");

            Assert.AreEqual(100, wellTest.Values[0].AverageCasingPressure, "Mismatch between expected average casing pressure and retrieved average casing pressure");
            Assert.AreEqual(1000, wellTest.Values[0].AverageFluidAbovePump, "Mismatch between expected average fluid above pump and retrieved average fluid above pump");
            Assert.AreEqual(90, wellTest.Values[0].AverageTubingPressure, "Mismatch between expected average tubing presure and retrieved average tubing pressure");
            Assert.AreEqual(100, wellTest.Values[0].AverageTubingTemperature, "Mismatch between expected average tubing temperature and retrieved average tubing temperature");
            Assert.AreEqual(0, wellTest.Values[0].Gas, "Mismatch between expected gas rate and retrieved gas rate");
            Assert.AreEqual((decimal?)0.5, wellTest.Values[0].GasGravity, "Mismatch between expected gas gravity and retrieved gas gravity");
            Assert.AreEqual(75, wellTest.Values[0].Oil, "Mismatch between expected oil rate and retrieved oil rate");
            Assert.AreEqual(34, wellTest.Values[0].OilGravity, "Mismatch between expected oil gravity and retrieved oil gravity");
            Assert.AreEqual(80, wellTest.Values[0].PumpEfficiency, "Mismatch between expected pump efficiency and retrieved pump efficiency");
            Assert.AreEqual(100, wellTest.Values[0].PumpIntakePressure, "Mismatch between expected pump intake pressure and retrieved pump intake pressure");
            Assert.AreEqual(10, wellTest.Values[0].PumpingHours, "Mismatch between expected pumping hours and retrieved pumping hours");
            Assert.AreEqual(11, wellTest.Values[0].StrokePerMinute, "Mismatch between expected SPM and retrieved SPM");
            Assert.AreEqual(3, wellTest.Values[0].TestDuration, "Mismatch between expected test duration and retrieved test duration");
            Assert.AreEqual(35, wellTest.Values[0].Water, "Mismatch between expected water rate and retrieved water rate");
            Assert.AreEqual(1, wellTest.Values[0].WaterGravity, "Mismatch between expected water gravity and retrieved water gravity");

            // Add a daily average production rate.
            WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO
            {
                Id = well.Id,
                EndDateTime = end,
                StartDateTime = start,
                GasRateAllocated = dailyAverage,
                GasRateInferred = dailyAverage,
                OilRateAllocated = dailyAverage,
                OilRateInferred = dailyAverage,
                WaterRateAllocated = dailyAverage,
                WaterRateInferred = dailyAverage,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 24,
                THP = 492,
                THT = 213,
                GasInjectionRate = 5280,
                WellId = well.Id,
                DHPG = 300
            };

            // Add the daily average to the database
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);

            // Get it back and validate the results
            WellDailyAverageValueDTO[] retrievedAverageDTO = SurveillanceService.GetDailyAverageTrends(well.Id.ToString(), start.ToISO8601(), end.ToISO8601());
            Assert.IsNotNull(wellTest, "Failed to retrieve latest daily average");

            Assert.AreEqual(dailyAverage, retrievedAverageDTO[0].GasRateAllocated, "Mismatch between expected allocated gas rate and retrieved allocated gas rate");
            Assert.AreEqual(dailyAverage, retrievedAverageDTO[0].GasRateInferred, "Mismatch between expected inferred gas rate and retrieved inferred gas rate");
            Assert.AreEqual(dailyAverage, retrievedAverageDTO[0].OilRateAllocated, "Mismatch between expected allocated oil rate and retrieved allocated oil rate");
            Assert.AreEqual(dailyAverage, retrievedAverageDTO[0].OilRateInferred, "Mismatch between expected inferred oil rate and retrieved inferred oil rate");
            Assert.AreEqual(dailyAverage, retrievedAverageDTO[0].WaterRateAllocated, "Mismatch between expected allocated water rate and retrieved allocated water rate");
            Assert.AreEqual(dailyAverage, retrievedAverageDTO[0].WaterRateInferred, "Mismatch between expected inferred water rate and retrieved inferred water rate");
            Assert.AreEqual(24, retrievedAverageDTO[0].Duration, "Mismatch between expected duration and retrieved duration");
            Assert.AreEqual(1000, retrievedAverageDTO[0].GasJectionDepth, "Mismatch between expected gas injection depth and retrieved gas injection depth");
            Assert.AreEqual(64, retrievedAverageDTO[0].ChokeDiameter, "Mismatch between expected choke diameter and retrieved choke diameter");
            Assert.AreEqual(24, retrievedAverageDTO[0].RunTime, "Mismatch between expected run time and retrieved run time");
            Assert.AreEqual(492, retrievedAverageDTO[0].THP, "Mismatch between expected THP and retrieved THP");
            Assert.AreEqual(213, retrievedAverageDTO[0].THT, "Mismatch between expected THT and retrieved THT");
            Assert.AreEqual(5280, retrievedAverageDTO[0].GasInjectionRate, "Mismatch between expected Qgi and retrieved Qgi");
            Assert.AreEqual(300, retrievedAverageDTO[0].DHPG, "Mismatch between expected DHPG and retrieved DHPG");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void AddWellDailyAverageDataTest()
        {
            //pick up the right model file
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            Tuple<string, WellTypeId, ModelFileOptionDTO> model = Tuple.Create("GL-01-Base.wflx", WellTypeId.GLift, new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.ReservoirPressure, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), (long)OptionalUpdates.UpdateWCT_WGR } });

            var modelFileName = model.Item1;
            var wellType = model.Item2;
            var options = model.Item3;

            Trace.WriteLine("Testing model: " + modelFileName);

            //Create a new well
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO { Name = DefaultWellName + wellType, CommissionDate = DateTime.Today.AddYears(-3), WellType = wellType }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells.FirstOrDefault(w => w.Name.Equals(DefaultWellName + wellType.ToString()));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            var myassembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(myassembly);

            ModelFileBase64DTO modelFile = new ModelFileBase64DTO();

            options.Comment = "CASETest Upload " + model.Item1;
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10);
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

            var testDataDTO = new WellTestDTO
            {
                WellId = well.Id,
                AverageCasingPressure = 1575,
                AverageTubingPressure = 1774,
                AverageTubingTemperature = 65,
                Gas = 2000m,
                GasGravity = 0.6722m,
                GasInjectionRate = 1563,
                producedGOR = 1160,
                Oil = 1749,
                OilGravity = 46.2415m,
                SPTCode = 2,
                SPTCodeDescription = "RepresentativeTest",
                TestDuration = 3,
                Water = 1960,
                WaterGravity = (decimal)1.0239,
                GaugePressure = (decimal)1634,
                ReservoirPressure = 5250.0m,
            };

            AddWellSetting(well.Id, "Min Reservoir Pressure Acceptance Limit", 15);
            AddWellSetting(well.Id, "Max Reservoir Pressure Acceptance Limit", 10000);

            testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(5));
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

            var dailyAverageDto = new WellDailyAverageValueDTO { WellId = well.Id, Status = WellDailyAverageDataStatus.Original, RunTime = 24, StartDateTime = DateTime.UtcNow.AddDays(-1), EndDateTime = DateTime.UtcNow, Duration = 24, THP = 200, THT = 100, ChokeDiameter = 1, FLP = 15, GasInjectionRate = 980, CHP = 1800, GasJectionDepth = 0, DHPG = 2000 };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDto);

            var dto = SurveillanceService.GetWellDailyAverageData(well.Id.ToString());
            Assert.AreEqual(2715.62, dto.OilRateAllocated.Value, 5);
            Assert.AreEqual(2715.62, dto.OilRateInferred.Value, 5);
            Assert.AreEqual(5532.16, dto.GasRateAllocated.Value, 5);
            Assert.AreEqual(5532.16, dto.GasRateInferred.Value, 5);
            Assert.AreEqual(3043.23, dto.WaterRateAllocated.Value, 5);
            Assert.AreEqual(3043.23, dto.WaterRateInferred.Value, 5);

            SurveillanceService.RemoveWellDailyAverageData(dto.Id.ToString());
            ModelFileService.RemoveModelFileOptionByModelFileId(newModelFile.Id.ToString());
            ModelFileService.RemoveModelFile(newModelFile.Id.ToString());
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GasLiftWellInferredProductionAndAllocationTest()
        {
            var _wellDailyAverageToRemove = new List<WellDailyAverageValueDTO>();

            #region find_or_create_default_asset

            var allAsset = SurfaceNetworkService.GetAllAssets()?.ToList();
            var defaultAsset = allAsset?.FirstOrDefault(a => a.Name.Equals("Default"));
            // add an asset if no Default was found
            if (defaultAsset == null)
            {
                var assetName = "Default";
                SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = "default asset" });
                var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
                defaultAsset = allAssets.First(a => a.Name.Equals(assetName));
                Assert.AreEqual(defaultAsset.Name, assetName);
                Assert.IsNotNull(defaultAsset);
            }

            #endregion find_or_create_default_asset

            #region create_an_oil_allocation_group_and_map_it_to_[22224][SIERRA][UIS]

            //create one oil allocation group and save it in the DB
            var oilAllocationGroup = new WellAllocationGroupDTO
            {
                AllocationGroupName = "Oil_Group",
                DataConnection =
                    GetDefaultCygNetDataConnection(),
                Phase = AllocationGroupPhaseId.Oil,
                FacilityId = "OIL_ALLOCATION_G0001",
                UDC = "ALLOCVOD",
                Asset = defaultAsset.Id
            };

            //Add an oil allocation group
            WellService.AddWellAllocationGroup(oilAllocationGroup);
            var oilAllocGroup = WellService.GetWellAllocationGroupByPhase(((int)oilAllocationGroup.Phase).ToString()).OrderByDescending(en => en.ChangeDate).First(s => s.Asset == oilAllocationGroup.Asset);

            var allocationDTO = WellService.GetWellAllocationGroup(oilAllocGroup.Id.ToString());
            Assert.AreEqual(oilAllocGroup.Id, allocationDTO.Id);

            _wellAllocationGroupsToRemove.Add(oilAllocGroup);

            #endregion create_an_oil_allocation_group_and_map_it_to_[22224][SIERRA][UIS]

            #region create_wells_and_upload_corresponding_model_files

            //Gas Lift model in test documents not good for tuning
            var path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";

            //Add Lomond_A-18 well
            Tuple<string, WellTypeId, ModelFileOptionDTO> model =

            Tuple.Create("Lomond_A-18.wflx", WellTypeId.GLift, new ModelFileOptionDTO
            {
                CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                OptionalUpdate = new[]
                {
                    (long) OptionalUpdates.UpdateGOR_CGR,
                    (long) OptionalUpdates.UpdateWCT_WGR,
                    (long) OptionalUpdates.CalculateChokeD_Factor
                }
            });

            var modelFileName = model.Item1;
            var wellType = model.Item2;
            var options = model.Item3;

            Trace.WriteLine("Testing model: " + modelFileName);

            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO { Name = "Lomond_A18", CommissionDate = DateTime.Today.AddYears(-2), WellType = wellType, OilAllocationGroup = oilAllocGroup.Id }) });
            var allWells = WellService.GetAllWells().ToList();
            var well = allWells.FirstOrDefault(w => w.Name.Equals("Lomond_A18"));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            var myassembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(myassembly);

            var modelFile = new ModelFileBase64DTO();

            options.Comment = "CASETest Upload " + model.Item1;
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10);
            modelFile.WellId = well.Id;

            byte[] fileAsByteArray = GetByteArray(path, model.Item1);
            Assert.IsNotNull(fileAsByteArray);

            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            var modelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
            Assert.IsNotNull(modelFileValidationData);
            ModelFileService.AddWellModelFile(modelFile);
            var modelFileDB = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(modelFileDB);

            //add a well test to this welluse
            var testDataDTO = new WellTestDTO
            {
                WellId = well.Id,
                AverageCasingPressure = 1908.30m,
                AverageTubingPressure = 265,
                AverageTubingTemperature = 151,
                Gas = 1140m,
                GasGravity = 0.6722m,
                GasInjectionRate = 2900,
                Oil = 1448,
                OilGravity = 46.2415m,
                SPTCode = 2,
                SPTCodeDescription = "RepresentativeTest",
                Water = 762,
                WaterGravity = (decimal)1.0239,
                GaugePressure = (decimal)1002,
                FlowLinePressure = 57,
                SeparatorPressure = 50,
                ChokeSize = 128,
                SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(5))
            };

            //set acceptance limits
            var acceptanceLimit = SettingService.GetWellSettingsByWellIdAndCategory(well.Id.ToString(), ((int)SettingCategory.AcceptanceLimit).ToString());
            if (!acceptanceLimit.Any())
            {
                var setting = SettingService.GetSettingByName(SettingServiceStringConstants.RESERVOIR_PRESSURE_MAX_AL);
                SettingService.SaveWellSetting(new WellSettingDTO
                {
                    SettingId = setting.Id,
                    NumericValue = 20000,
                    WellId = well.Id
                });

                setting = SettingService.GetSettingByName(SettingServiceStringConstants.RESERVOIR_PRESSURE_MIN_AL);
                SettingService.SaveWellSetting(new WellSettingDTO
                {
                    SettingId = setting.Id,
                    NumericValue = 14,
                    WellId = well.Id
                });
            }
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

            //add the daily average measurement data to the well
            var dailyAverageDto = new WellDailyAverageValueDTO { WellId = well.Id, RunTime = 24, StartDateTime = well.CommissionDate.Value.AddDays(1).ToUniversalTime(), EndDateTime = well.CommissionDate.Value.AddDays(2).ToUniversalTime(), Duration = 24, THP = 339, THT = 140.77, ChokeDiameter = 64, GasInjectionRate = 2820, CHP = 1836 };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDto);
            //var dailyAverDto = SurveillanceService.GetWellDailyAverageData(well.Id.ToString());
            //_wellDailyAverageToRemove.Add(dailyAverDto);

            //Add Lomond_B-18 well
            model =
            Tuple.Create("Lomond_B-18.wflx", WellTypeId.GLift, new ModelFileOptionDTO
            {
                CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                OptionalUpdate = new[]
                {
                    (long) OptionalUpdates.UpdateGOR_CGR,
                    (long) OptionalUpdates.UpdateWCT_WGR,
                    (long) OptionalUpdates.CalculateChokeD_Factor
                }
            });

            modelFileName = model.Item1;
            wellType = model.Item2;
            options = model.Item3;

            Trace.WriteLine("Testing model: " + modelFileName);

            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO { Name = "Lomond_B18", CommissionDate = DateTime.Today.AddYears(-2), WellType = wellType, OilAllocationGroup = oilAllocGroup.Id }) });
            allWells = WellService.GetAllWells().ToList();
            well = allWells.FirstOrDefault(w => w.Name.Equals("Lomond_B18"));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            myassembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(myassembly);

            modelFile = new ModelFileBase64DTO();

            options.Comment = "CASETest Upload " + model.Item1;
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10);
            modelFile.WellId = well.Id;

            fileAsByteArray = GetByteArray(path, model.Item1);
            Assert.IsNotNull(fileAsByteArray);

            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            modelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
            Assert.IsNotNull(modelFileValidationData);
            ModelFileService.AddWellModelFile(modelFile);
            modelFileDB = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(modelFileDB);

            //add a well test to this welluse
            testDataDTO = new WellTestDTO
            {
                WellId = well.Id,
                AverageCasingPressure = 1545.30m,
                AverageTubingPressure = 375,
                AverageTubingTemperature = 142,
                Gas = 950m,
                GasGravity = 0.6722m,
                GasInjectionRate = 1500,
                Oil = 1260,
                OilGravity = 46.2415m,
                SPTCode = 2,
                SPTCodeDescription = "RepresentativeTest",
                Water = 2176,
                WaterGravity = (decimal)1.0239,
                GaugePressure = (decimal)2257,
                FlowLinePressure = 235,
                SeparatorPressure = 50,
                ChokeSize = 128,
                SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(5))
            };

            //set acceptance limits
            acceptanceLimit = SettingService.GetWellSettingsByWellIdAndCategory(well.Id.ToString(), ((int)SettingCategory.AcceptanceLimit).ToString());
            if (!acceptanceLimit.Any())
            {
                var setting = SettingService.GetSettingByName(SettingServiceStringConstants.RESERVOIR_PRESSURE_MAX_AL);
                SettingService.SaveWellSetting(new WellSettingDTO
                {
                    SettingId = setting.Id,
                    NumericValue = 20000,
                    WellId = well.Id
                });

                setting = SettingService.GetSettingByName(SettingServiceStringConstants.RESERVOIR_PRESSURE_MIN_AL);
                SettingService.SaveWellSetting(new WellSettingDTO
                {
                    SettingId = setting.Id,
                    NumericValue = 14,
                    WellId = well.Id
                });
            }
            units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

            //add the daily average measurement data to the well
            dailyAverageDto = new WellDailyAverageValueDTO { WellId = well.Id, RunTime = 24, StartDateTime = well.CommissionDate.Value.AddDays(1).ToUniversalTime(), EndDateTime = well.CommissionDate.Value.AddDays(2).ToUniversalTime(), Duration = 24, THP = 384, THT = 142.91, ChokeDiameter = 64, FLP = 15, GasInjectionRate = 1500, CHP = 1519 };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDto);
            //dailyAverDto = SurveillanceService.GetWellDailyAverageData(well.Id.ToString());
            //_wellDailyAverageToRemove.Add(dailyAverDto);

            //Add Ness_J-20 well
            model =
            Tuple.Create("Ness_J-20.wflx", WellTypeId.GLift, new ModelFileOptionDTO
            {
                CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                OptionalUpdate = new[]
                {
                    (long) OptionalUpdates.UpdateGOR_CGR,
                    (long) OptionalUpdates.UpdateWCT_WGR,
                    (long) OptionalUpdates.CalculateChokeD_Factor
                }
            });

            modelFileName = model.Item1;
            wellType = model.Item2;
            options = model.Item3;

            Trace.WriteLine("Testing model: " + modelFileName);

            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO { Name = "Ness_J20", CommissionDate = DateTime.Today.AddYears(-2), WellType = wellType, OilAllocationGroup = oilAllocGroup.Id }) });
            allWells = WellService.GetAllWells().ToList();
            well = allWells.FirstOrDefault(w => w.Name.Equals("Ness_J20"));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            myassembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(myassembly);

            modelFile = new ModelFileBase64DTO();

            options.Comment = "CASETest Upload " + model.Item1;
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10);
            modelFile.WellId = well.Id;
            fileAsByteArray = GetByteArray(path, model.Item1);
            Assert.IsNotNull(fileAsByteArray);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            modelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
            Assert.IsNotNull(modelFileValidationData);
            ModelFileService.AddWellModelFile(modelFile);
            modelFileDB = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(modelFileDB);

            //add a well test to this welluse
            testDataDTO = new WellTestDTO
            {
                WellId = well.Id,
                AverageCasingPressure = 2202.3m,
                AverageTubingPressure = 565.3m,
                AverageTubingTemperature = 212.05m,
                Gas = 400m,
                GasGravity = 0.6722m,
                GasInjectionRate = 2940,
                Oil = 526,
                OilGravity = 46.2415m,
                SPTCode = 2,
                SPTCodeDescription = "RepresentativeTest",
                Water = 2955,
                WaterGravity = (decimal)1.0239,
                FlowLinePressure = 265,
                SeparatorPressure = 50,
                ChokeSize = 64,
                SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(5))
            };

            //set acceptance limits
            acceptanceLimit = SettingService.GetWellSettingsByWellIdAndCategory(well.Id.ToString(), ((int)SettingCategory.AcceptanceLimit).ToString());
            if (!acceptanceLimit.Any())
            {
                var setting = SettingService.GetSettingByName(SettingServiceStringConstants.RESERVOIR_PRESSURE_MAX_AL);
                SettingService.SaveWellSetting(new WellSettingDTO
                {
                    SettingId = setting.Id,
                    NumericValue = 20000,
                    WellId = well.Id
                });

                setting = SettingService.GetSettingByName(SettingServiceStringConstants.RESERVOIR_PRESSURE_MIN_AL);
                SettingService.SaveWellSetting(new WellSettingDTO
                {
                    SettingId = setting.Id,
                    NumericValue = 14,
                    WellId = well.Id
                });
            }

            units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

            //add the daily average measurement data to the well
            dailyAverageDto = new WellDailyAverageValueDTO { WellId = well.Id, RunTime = 24, StartDateTime = well.CommissionDate.Value.AddDays(1).ToUniversalTime(), EndDateTime = well.CommissionDate.Value.AddDays(2).ToUniversalTime(), Duration = 24, THP = 478, THT = 213.16, ChokeDiameter = 64, GasInjectionRate = 4870, CHP = 2077 };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDto);
            //dailyAverDto = SurveillanceService.GetWellDailyAverageData(well.Id.ToString());
            //_wellDailyAverageToRemove.Add(dailyAverDto);

            //Add Ness_J-44 well
            model =
            Tuple.Create("Ness_J-44.wflx", WellTypeId.GLift, new ModelFileOptionDTO
            {
                CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                OptionalUpdate = new[]
                {
                    (long) OptionalUpdates.UpdateGOR_CGR,
                    (long) OptionalUpdates.UpdateWCT_WGR,
                    (long) OptionalUpdates.CalculateChokeD_Factor
                }
            });

            modelFileName = model.Item1;
            wellType = model.Item2;
            options = model.Item3;

            Trace.WriteLine("Testing model: " + modelFileName);

            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO { Name = "Ness_J44", CommissionDate = DateTime.Today.AddYears(-2), WellType = wellType, OilAllocationGroup = oilAllocGroup.Id }) });
            allWells = WellService.GetAllWells().ToList();
            well = allWells.FirstOrDefault(w => w.Name.Equals("Ness_J44"));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            myassembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(myassembly);

            modelFile = new ModelFileBase64DTO();

            options.Comment = "CASETest Upload " + model.Item1;
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10);
            modelFile.WellId = well.Id;

            fileAsByteArray = GetByteArray(path, model.Item1);
            Assert.IsNotNull(fileAsByteArray);

            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            modelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
            Assert.IsNotNull(modelFileValidationData);
            ModelFileService.AddWellModelFile(modelFile);
            modelFileDB = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(modelFileDB);

            //add a well test to this welluse
            testDataDTO = new WellTestDTO
            {
                WellId = well.Id,
                AverageCasingPressure = 2156.3m,
                AverageTubingPressure = 242.3m,
                AverageTubingTemperature = 202.01m,
                Gas = 700m,
                GasGravity = 0.6722m,
                GasInjectionRate = 3650,
                Oil = 577,
                OilGravity = 46.2415m,
                SPTCode = 2,
                SPTCodeDescription = "RepresentativeTest",
                Water = 2393,
                WaterGravity = (decimal)1.0239,
                FlowLinePressure = 192,
                SeparatorPressure = 50,
                GaugePressure = (decimal)1142,
                ChokeSize = 64,
                SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(5))
            };

            //set acceptance limits
            acceptanceLimit = SettingService.GetWellSettingsByWellIdAndCategory(well.Id.ToString(), ((int)SettingCategory.AcceptanceLimit).ToString());
            if (!acceptanceLimit.Any())
            {
                var setting = SettingService.GetSettingByName(SettingServiceStringConstants.RESERVOIR_PRESSURE_MAX_AL);
                SettingService.SaveWellSetting(new WellSettingDTO
                {
                    SettingId = setting.Id,
                    NumericValue = 20000,
                    WellId = well.Id
                });

                setting = SettingService.GetSettingByName(SettingServiceStringConstants.RESERVOIR_PRESSURE_MIN_AL);
                SettingService.SaveWellSetting(new WellSettingDTO
                {
                    SettingId = setting.Id,
                    NumericValue = 14,
                    WellId = well.Id
                });
            }

            units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

            //add the daily average measurement data to the well
            dailyAverageDto = new WellDailyAverageValueDTO { WellId = well.Id, RunTime = 24, StartDateTime = well.CommissionDate.Value.AddDays(1).ToUniversalTime(), EndDateTime = well.CommissionDate.Value.AddDays(2).ToUniversalTime(), Duration = 24, THP = 356, THT = 203.2, ChokeDiameter = 64, GasInjectionRate = 3560, CHP = 2201 };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDto);
            //dailyAverDto = SurveillanceService.GetWellDailyAverageData(well.Id.ToString());
            //_wellDailyAverageToRemove.Add(dailyAverDto);

            //Add Ness_J-43 well
            model =
            Tuple.Create("Ness_J-43.wflx", WellTypeId.GLift, new ModelFileOptionDTO
            {
                CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                OptionalUpdate = new[]
                {
                    (long) OptionalUpdates.UpdateGOR_CGR,
                    (long) OptionalUpdates.UpdateWCT_WGR,
                    (long) OptionalUpdates.CalculateChokeD_Factor
                }
            });

            modelFileName = model.Item1;
            wellType = model.Item2;
            options = model.Item3;

            Trace.WriteLine("Testing model: " + modelFileName);

            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO { Name = "Ness_J43", CommissionDate = DateTime.Today.AddYears(-2), WellType = wellType, OilAllocationGroup = oilAllocGroup.Id }) });
            allWells = WellService.GetAllWells().ToList();
            well = allWells.FirstOrDefault(w => w.Name.Equals("Ness_J43"));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            myassembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(myassembly);

            modelFile = new ModelFileBase64DTO();

            options.Comment = "CASETest Upload " + model.Item1;
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10);
            modelFile.WellId = well.Id;
            fileAsByteArray = GetByteArray(path, model.Item1);
            Assert.IsNotNull(fileAsByteArray);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            modelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
            Assert.IsNotNull(modelFileValidationData);
            ModelFileService.AddWellModelFile(modelFile);
            modelFileDB = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(modelFileDB);

            //add a well test to this welluse
            testDataDTO = new WellTestDTO
            {
                WellId = well.Id,
                AverageCasingPressure = 2111.3m,
                AverageTubingPressure = 459.3m,
                AverageTubingTemperature = 140.9m,
                Gas = 2900m,
                GasGravity = 0.6722m,
                GasInjectionRate = 4210,
                Oil = 3678,
                OilGravity = 46.2415m,
                SPTCode = 2,
                SPTCodeDescription = "RepresentativeTest",
                Water = 7420,
                WaterGravity = (decimal)1.0239,
                FlowLinePressure = 284,
                SeparatorPressure = 50,
                GaugePressure = (decimal)2544,
                ChokeSize = 25,
                SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(5))
            };

            //set acceptance limits
            acceptanceLimit = SettingService.GetWellSettingsByWellIdAndCategory(well.Id.ToString(), ((int)SettingCategory.AcceptanceLimit).ToString());
            if (!acceptanceLimit.Any())
            {
                var setting = SettingService.GetSettingByName(SettingServiceStringConstants.RESERVOIR_PRESSURE_MAX_AL);
                SettingService.SaveWellSetting(new WellSettingDTO
                {
                    SettingId = setting.Id,
                    NumericValue = 20000,
                    WellId = well.Id
                });

                setting = SettingService.GetSettingByName(SettingServiceStringConstants.RESERVOIR_PRESSURE_MIN_AL);
                SettingService.SaveWellSetting(new WellSettingDTO
                {
                    SettingId = setting.Id,
                    NumericValue = 14,
                    WellId = well.Id
                });

                setting = SettingService.GetSettingByName(SettingServiceStringConstants.LFACTOR_MAX_AL);
                SettingService.SaveWellSetting(new WellSettingDTO
                {
                    SettingId = setting.Id,
                    NumericValue = 1.2,
                    WellId = well.Id
                });
            }

            units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

            //add the daily average measurement data to the well
            dailyAverageDto = new WellDailyAverageValueDTO { WellId = well.Id, RunTime = 24, StartDateTime = well.CommissionDate.Value.AddDays(1).ToUniversalTime(), EndDateTime = well.CommissionDate.Value.AddDays(2).ToUniversalTime(), Duration = 24, THP = 787, THT = 116.89, ChokeDiameter = 64, GasInjectionRate = 2350, CHP = 2202 };
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDto);
            //dailyAverDto = SurveillanceService.GetWellDailyAverageData(well.Id.ToString());
            //_wellDailyAverageToRemove.Add(dailyAverDto);

            #endregion create_wells_and_upload_corresponding_model_files

            //var result = WellTestDataService.CalculateAllocationGroup(allocationDTO.Id.ToString());
            //Assert.IsNotNull(result);
            //Assert.AreEqual(5, result.WellsAllocation.Count());

            //clean up the allocation group and the asset
            foreach (var dto in _wellDailyAverageToRemove)
            {
                SurveillanceService.RemoveWellDailyAverageData(dto.Id.ToString());
            }

            foreach (var item in _wellsToRemove)
            {
                SettingService.RemoveWellSetting(item.Id.ToString());
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetGLWellTrends_DLQ()
        {
            string facilityId;
            facilityId = GetFacilityId("GLWELL_", 1);
            _addedWells = new List<WellDTO>();

            Dictionary<WellQuantity, string> dic = new Dictionary<WellQuantity, string>()
            {
                {WellQuantity.CasingPressure, "PRCASXIN" },
                {WellQuantity.TubingPressure, "PRTUBXIN" },
                {WellQuantity.DHGaugePressure, "DHPGAUGE" },
                {WellQuantity.TubingHeadTemperature, "TEMPTUBXIN" },
                {WellQuantity.GasInjectionRate, "QTGAST" },
                {WellQuantity.GasRateMeasured, "RGAS" },
                {WellQuantity.OilRateMeasured, "ROIL" },
                {WellQuantity.WaterRateMeasured, "RH2O" }
            };

            try
            {
                WellDTO well = SetDefaultFluidType(new WellDTO() { Name = "GL" + "TestWell", FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today, WellType = WellTypeId.GLift });
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
                WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(well.Name));
                Assert.IsNotNull(addedWell);
                _wellsToRemove.Add(addedWell);
                _addedWells.Add(addedWell);

                SurveillanceService.IssueCommandForSingleWell(addedWell.Id, "DemandScan");
                DateTime startTime = DateTime.Today.ToUniversalTime().AddDays(-31);
                DateTime endTime = DateTime.Today.ToUniversalTime();
                foreach (WellQuantity glQuantity in QuantityUDCMapping<WellQuantity>.Instance.GetQuantitiesForWellType(WellTypeId.GLift))
                {
                    int udcTagIdNumber = (int)glQuantity;
                    string[] udcTagIdNumberStr = { udcTagIdNumber.ToString() };

                    CygNetTrendDTO avgWellTrend = SurveillanceService.GetCygNetTrendsByUDCAndDateRange(
                        udcTagIdNumberStr, addedWell.Id.ToString(),
                        DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-31)),
                        DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime())).FirstOrDefault();
                    string trendudcmsg = string.Format("Trend DTO PointUDC from CygNet for specified period from: {0} to: {1} is coming as NULL , means no data in CygNet VHS for that period.", startTime.ToString(), endTime.ToString());

                    if (dic.ContainsKey(glQuantity))
                    {
                        Assert.IsNotNull(avgWellTrend.PointUDC, trendudcmsg);
                        Assert.AreEqual(dic[glQuantity], avgWellTrend.PointUDC, "Mismatch between trendQuantity and PointUDC" + dic[glQuantity].ToString());
                    }

                    if (avgWellTrend?.PointValues != null)
                    {
                        DateTime st = avgWellTrend.PointValues.LastOrDefault().Timestamp.ToUniversalTime();
                        Assert.IsTrue(st >= startTime);

                        DateTime et = avgWellTrend.PointValues.FirstOrDefault().Timestamp.ToUniversalTime();
                        Assert.IsTrue(et <= endTime);
                    }
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void PGLWellTrendsTest()
        {
            string passFacilityId1 = GetFacilityId("PGLWELL_", 1);
            GetPGLWellTrends(passFacilityId1);
            Trace.WriteLine("Well tends test for PGL was completed");
        }

        public void GetPGLWellTrends(string fac_ID)
        {
            _addedWells = new List<WellDTO>();

            Dictionary<WellQuantity, string> dic = new Dictionary<WellQuantity, string>()
            {
                {WellQuantity.CasingPressure, "PRCASXIN" },
                {WellQuantity.TubingPressure, "PRTUBXIN" },
                {WellQuantity.DifferentialPressure, "PRDIFF" },
                {WellQuantity.FlowLinePressure, "PRFLOLINE" },
                {WellQuantity.GasRateMeasured, "RGAS" },
                {WellQuantity.LoadFactor, "FACLOAD" },
            };

            try
            {
                WellDTO well = SetDefaultFluidType(new WellDTO() { Name = fac_ID + "TestWell", FacilityId = fac_ID, DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today, WellType = WellTypeId.PLift });
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
                WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(well.Name));
                Assert.IsNotNull(addedWell);
                _wellsToRemove.Add(addedWell);
                _addedWells.Add(addedWell);

                SurveillanceService.IssueCommandForSingleWell(addedWell.Id, "DemandScan");
                DateTime startTime = DateTime.Today.ToUniversalTime().AddDays(-31);
                DateTime endTime = DateTime.Today.ToUniversalTime();
                foreach (WellQuantity pglQuantity in QuantityUDCMapping<WellQuantity>.Instance.GetQuantitiesForWellType(WellTypeId.PLift))
                {
                    int udcTagIdNumber = (int)pglQuantity;
                    string[] udcTagIdNumberStr = { udcTagIdNumber.ToString() };

                    CygNetTrendDTO avgWellTrend = SurveillanceService.GetCygNetTrendsByUDCAndDateRange(
                        udcTagIdNumberStr, addedWell.Id.ToString(),
                        DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-31)),
                        DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime())).FirstOrDefault();
                    if (dic.ContainsKey(pglQuantity))
                    {
                        Assert.AreEqual(dic[pglQuantity], avgWellTrend.PointUDC);
                    }

                    if (avgWellTrend?.PointValues != null)
                    {
                        DateTime st = avgWellTrend.PointValues.LastOrDefault().Timestamp.ToUniversalTime();
                        Assert.IsTrue(st >= startTime);

                        DateTime et = avgWellTrend.PointValues.FirstOrDefault().Timestamp.ToUniversalTime();
                        Assert.IsTrue(et <= endTime);
                    }
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetDailyAverageTrendsAndUnitsNonRRLTest_ESP()
        {
            //pick up the right model file
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            Tuple<string, WellTypeId, ModelFileOptionDTO> model = Tuple.Create("Esp_ProductionTestData.wflx", WellTypeId.ESP, new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.ReservoirPressure, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), (long)OptionalUpdates.UpdateWCT_WGR } });

            string modelFileName = model.Item1;
            WellTypeId wellType = model.Item2;
            ModelFileOptionDTO options = model.Item3;

            //Create a new well
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName + wellType.ToString(), FacilityId = "ESPWELL_0001", DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today.AddYears(-2), WellType = wellType }) });
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

            //add new wellTestData
            var testDataDTO = new WellTestDTO
            {
                WellId = well.Id,
                AverageTubingPressure = 200.00m,
                AverageTubingTemperature = 100.0m,
                Gas = 1.2m,
                GasGravity = 0.65m,
                producedGOR = 0.5m,
                Oil = 2397.5m,
                OilGravity = 34.97m,
                SPTCode = 2,
                SPTCodeDescription = "RepresentativeTest",
                TestDuration = 3,
                Water = 3596.2m,
                WaterGravity = 1.0198m,
                PumpIntakePressure = 1412.25m,
                PumpDischargePressure = 3067.59m,
                FlowLinePressure = 1412.25m,
                SeparatorPressure = 1412.25m,
                ChokeSize = 32
                //LFactor = 1,
                //ReservoirPressure = 5250,
                //ProductivityIndex = (decimal)1.36,
                //GaugePressure = (decimal)1610
            };

            testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10));
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

            var test = WellTestDataService.GetLatestValidWellTestByWellId(well.Id.ToString());
            Assert.IsNotNull(test);
            Assert.AreEqual(test.Value.Status, WellTestStatus.ACCEPTANCE_LIMITS_RESERVOIR_PRESSURE_TOO_HIGH);

            var status = SurveillanceService.AddDailyAverageFromVHSByDateRange(well.Id.ToString(),
                        (DateTime.Today - TimeSpan.FromDays(10)).ToUniversalTime().ToISO8601(),
                        DateTime.Today.ToUniversalTime().ToISO8601());

            List<string> trendList = new List<string>();
            trendList.Add(((int)DailyAverageQuantity.THT).ToString());
            trendList.Add(((int)DailyAverageQuantity.MotorAmps).ToString());
            trendList.Add(((int)DailyAverageQuantity.MotorFrequency).ToString());
            trendList.Add(((int)DailyAverageQuantity.MotorVolts).ToString());
            var dailyAverages =
                        SurveillanceService.GetDailyAverageTrendsByDateRange(trendList.ToArray(), well.Id.ToString(),
                            (DateTime.Today - TimeSpan.FromDays(10)).ToUniversalTime().ToISO8601(),
                            DateTime.Today.ToUniversalTime().ToISO8601());

            Assert.AreEqual(dailyAverages.Length, 4, "Mismatch between the requested and plotted trends");
            foreach (var value in dailyAverages)
            {
                Assert.IsNotNull(value);
                Assert.IsNotNull(value.PointUDC);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetDailyAverageTrendsAndUnitsNonRRLTest_NF()
        {
            //pick up the right model file
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            Tuple<string, WellTypeId, ModelFileOptionDTO> model = Tuple.Create("ProductionTestData.wflx", WellTypeId.NF, new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.ReservoirPressure, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), (long)OptionalUpdates.UpdateWCT_WGR } });

            string modelFileName = model.Item1;
            WellTypeId wellType = model.Item2;
            ModelFileOptionDTO options = model.Item3;

            //Create a new well
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName + wellType, FacilityId = "NFWWELL_0001", DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today.AddYears(-2), WellType = wellType }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName + wellType.ToString()));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            var myassembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(myassembly);

            ModelFileBase64DTO modelFile = new ModelFileBase64DTO();

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

            //add new wellTestData
            var testDataDTO = new WellTestDTO
            {
                WellId = well.Id,
                AverageTubingPressure = 164.7m,
                AverageTubingTemperature = 100.0m,
                Gas = 880m,
                GasGravity = 0.65m,
                producedGOR = 497.3m,
                Oil = 1769.5m,
                OilGravity = 34.97m,
                SPTCode = 2,
                SPTCodeDescription = "RepresentativeTest",
                Water = 589.8m,
                WaterGravity = 1.0198m,
                GaugePressure = 1412.25m,
                FlowLinePressure = 1412.25m,
                SeparatorPressure = 1412.25m,
                ChokeSize = 32m
            };

            testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10));
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

            var test = WellTestDataService.GetLatestValidWellTestByWellId(well.Id.ToString());
            Assert.IsNotNull(test);
            Assert.AreEqual(test.Value.Status, WellTestStatus.TUNING_SUCCEEDED);

            var status = SurveillanceService.AddDailyAverageFromVHSByDateRange(well.Id.ToString(),
                        (DateTime.Today - TimeSpan.FromDays(10)).ToUniversalTime().ToISO8601(),
                        DateTime.Today.ToUniversalTime().ToISO8601());

            List<string> trendList = new List<string>();
            trendList.Add(((int)DailyAverageQuantity.DHPG).ToString());
            var dailyAverages =
                        SurveillanceService.GetDailyAverageTrendsByDateRange(trendList.ToArray(), well.Id.ToString(),
                            (DateTime.Today - TimeSpan.FromDays(10)).ToUniversalTime().ToISO8601(),
                            DateTime.Today.ToUniversalTime().ToISO8601());
            Assert.AreEqual(1, dailyAverages.Count(), "Mismatch between the requested and plotted trends");
            Assert.IsNotNull(dailyAverages.FirstOrDefault().PointUDC, "Mismatch in the UDCs");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetNonRRLWellStatusTest_GI()
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            Tuple<string, WellTypeId, ModelFileOptionDTO> model = Tuple.Create("WellfloGasInjectionExample1.wflx", WellTypeId.GInj, new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), (long)OptionalUpdates.UpdateWCT_WGR } });
            //gas Injec facility id : GASINJWELL_0001
            //water inj facility id : WATERINJWELL_0001
            string modelFileName = model.Item1;
            WellTypeId wellType = model.Item2;
            ModelFileOptionDTO options = model.Item3;

            //Create a new well
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName + wellType, FacilityId = "GASINJWELL_0001", DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today.AddYears(-2), WellType = wellType }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName + wellType.ToString()));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            var myassembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(myassembly);

            ModelFileBase64DTO modelFile = new ModelFileBase64DTO();

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

            //add new wellTestData
            var testDataDTO = new WellTestDTO
            {
                WellId = well.Id,
                AverageTubingPressure = 164.7m,
                AverageTubingTemperature = 100.0m,
                Gas = 880m,
                GasGravity = 0.65m,
                producedGOR = 497.3m,
                Oil = 1769.5m,
                OilGravity = 34.97m,
                SPTCode = 2,
                SPTCodeDescription = "RepresentativeTest",
                Water = 589.8m,
                WaterGravity = 1.0198m,
                GasInjectionRate = 99m,
                FlowLinePressure = 99m,
                GaugePressure = 99m,
            };

            testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10));
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

            var test = WellTestDataService.GetLatestValidWellTestByWellId(well.Id.ToString());
            Assert.AreNotEqual(test, null);
            Assert.AreEqual(test.Value.Status, WellTestStatus.ACCEPTANCE_LIMITS_LFATCOR_TOO_HIGH);

            var gaugeData = SurveillanceService.GetNonRRLWellGauges(well.Id.ToString());
            Assert.AreNotEqual(gaugeData.Length, 0);

            var productionKpiData = SurveillanceService.GetProductionKPI(well.Id.ToString(), DTOExtensions.ToISO8601(DateTime.UtcNow.Date));
            Assert.IsNotNull(productionKpiData, null);

            var realTimeData = SurveillanceServiceClient.GetWellStatusData(well.Id.ToString());
            Assert.IsNotNull(realTimeData, null);

            var wdaTest = SurveillanceService.GetWellDailyAverageAndTest(well.Id.ToString(), DTOExtensions.ToISO8601(DateTime.UtcNow.Date));
            Assert.IsNull(wdaTest.WellTest, null);

            var prodTrendsData = SurveillanceService.GetProductionTrends(well.Id.ToString(),
                            (DateTime.Today - TimeSpan.FromDays(10)).ToUniversalTime().ToISO8601(),
                            DateTime.Today.ToUniversalTime().ToISO8601());
            Assert.IsNotNull(prodTrendsData, null);

            //var dailyAverages =
            //            SurveillanceService.GetNonRRLDailyAverages(well.Id.ToString(),
            //                (DateTime.Today - TimeSpan.FromDays(10)).ToUniversalTime().ToISO8601(),
            //                DateTime.Today.ToUniversalTime().ToISO8601());

            //Assert.AreNotEqual(dailyAverages.Values.Length, 0);
            //foreach (var value in dailyAverages.Values)
            //{
            //    Assert.AreNotEqual(value, null);
            //}
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetNonRRLWellStatusTest_WI()
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            Tuple<string, WellTypeId, ModelFileOptionDTO> model = Tuple.Create("WellfloWaterInjectionExample1.wflx", WellTypeId.WInj, new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), (long)OptionalUpdates.UpdateWCT_WGR } });

            string modelFileName = model.Item1;
            WellTypeId wellType = model.Item2;
            ModelFileOptionDTO options = model.Item3;

            //Create a new well
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName + wellType, FacilityId = "WATERINJWELL_0001", DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today.AddYears(-2), WellType = wellType }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName + wellType.ToString()));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            var myassembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(myassembly);

            ModelFileBase64DTO modelFile = new ModelFileBase64DTO();

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

            //add new wellTestData
            var testDataDTO = new WellTestDTO
            {
                WellId = well.Id,
                AverageTubingPressure = 3514m,
                AverageTubingTemperature = 99m,
                //Gas = 880m,
                //GasGravity = 0.65m,
                //producedGOR = 497.3m,
                //Oil = 1769.5m,
                //OilGravity = 34.97m,
                SPTCode = 2,
                SPTCodeDescription = "AllocatableTest",
                Water = 6900m,
                //WaterGravity = 1.0198m,
                //GasInjectionRate = 99m,
                FlowLinePressure = 150m,
                GaugePressure = 7600m,
            };

            testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10));
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

            var test = WellTestDataService.GetLatestValidWellTestByWellId(well.Id.ToString());
            Assert.AreNotEqual(test, null);
            //Assert.AreEqual(test.Value.Status, WellTestStatus.TUNING_SUCCEEDED);

            var gaugeData = SurveillanceService.GetNonRRLWellGauges(well.Id.ToString());
            Assert.AreNotEqual(gaugeData.Length, 0);

            var productionKpiData = SurveillanceService.GetProductionKPI(well.Id.ToString(), DTOExtensions.ToISO8601(DateTime.UtcNow.Date));

            //Assert.IsNotNull(productionKpiData.CurrentAllocatedProduction, null);
            //Assert.IsNotNull(productionKpiData.CurrentInferredProduction, null);
            //Assert.IsNotNull(productionKpiData.LastAllocatedProduction, null);
            //Assert.IsNotNull(productionKpiData.LastInferredProduction, null);
            Assert.IsNotNull(productionKpiData.WellTestProduction, null);

            var realTimeData = SurveillanceServiceClient.GetWellStatusData(well.Id.ToString());
            Assert.IsNotNull(realTimeData, null);

            var wdaTest = SurveillanceService.GetWellDailyAverageAndTest(well.Id.ToString(), DTOExtensions.ToISO8601(DateTime.UtcNow.Date));
            Assert.IsNotNull(wdaTest.WellTest, null);

            var prodTrendsData = SurveillanceService.GetProductionTrends(well.Id.ToString(),
                            (DateTime.Today - TimeSpan.FromDays(10)).ToUniversalTime().ToISO8601(),
                            DateTime.Today.ToUniversalTime().ToISO8601());
            Assert.IsNotNull(prodTrendsData, null);

            //var dailyAverages =
            //            SurveillanceService.GetNonRRLDailyAverages(well.Id.ToString(),
            //                (DateTime.Today - TimeSpan.FromDays(10)).ToUniversalTime().ToISO8601(),
            //                DateTime.Today.ToUniversalTime().ToISO8601());

            //Assert.AreNotEqual(dailyAverages.Values.Length, 0);
            //foreach (var value in dailyAverages.Values)
            //{
            //    Assert.AreNotEqual(value, null);
            //}
        }

        #region helper_methods

        private string GetDeviceIdForDownloadDownholeConfig(string facilityId, CygNet.COMAPI.Interfaces.IDdsClient ddsClient)
        {
            const string cmdName = "";
            string cmdInfo = null;
            _ddsClient.Connect(ddsClient.SiteService.GetDomainSiteService());
            try
            {
                cmdInfo = _ddsClient.GetUisCommandInfo(facilityId, cmdName);
                if (!string.IsNullOrWhiteSpace(cmdInfo))
                {
                    return facilityId;
                }
            }
            catch { }
            object temp;
            _ddsClient.GetDeviceIdListFromFacilityId(facilityId, out temp);
            object[] temp2 = temp as object[];
            List<string> deviceIds = new List<string>(temp2.OfType<string>());
            if (deviceIds.Count == 1)
            {
                return deviceIds[0];
            }
            else
            {
                // Whee.
                SystemSettingDTO expectedDeviceTypesSetting = SettingService.GetSystemSettingByName(SettingServiceStringConstants.EXPECTED_DEVICE_TYPES);
                if (expectedDeviceTypesSetting?.StringValue != null)
                {
                    var expectedDeviceTypes = new HashSet<string>(expectedDeviceTypesSetting?.StringValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                    foreach (string oneDevice in deviceIds)
                    {
                        string deviceType = ddsClient.GetDeviceTypeForDevice(oneDevice);
                        if (expectedDeviceTypes.Contains(deviceType))
                        {
                            return oneDevice;
                        }
                    }
                }
            }
            return null;
        }

        private void TestDownloadDownholeConfig(string facilityId)
        {
            bool dcExisted = false;
            try
            {
                // Create a well with model file
                WellDTO well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today, AssemblyAPI = "123456789012", SubAssemblyAPI = "1234567890", WellType = WellTypeId.RRL });
                dcExisted = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == well.DataConnection.ProductionDomain && dc.Site == well.DataConnection.Site && dc.Service == well.DataConnection.Service) != null;
                WellDTO[] wells = WellService.GetAllWells();
                Assert.IsFalse(wells.Any(w => w.Name.Equals(well.Name)), "Well already exists in database.");
                WellConfigDTO wellConfigDTO = new WellConfigDTO();
                wellConfigDTO.Well = well;
                wellConfigDTO.ModelConfig = ReturnFullyPopulatedModel();
                WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
                wells = WellService.GetAllWells();
                WellDTO getwell = wells.FirstOrDefault(w => w.Name == well.Name);
                _wellsToRemove.Add(getwell);
                if (!dcExisted)
                {
                    _dataConnectionsToRemove.Add(getwell.DataConnection);
                }

                var ddsClient = GetDdsClient();

                string deviceId = GetDeviceIdForDownloadDownholeConfig(facilityId, ddsClient);

                Trace.WriteLine("Checking for UIS pending queue before sending any UIS command request.");
                WaitForUISToDrainItsQueue("TestDownloadDownholeConfig_" + facilityId, deviceId);

                // Download downhole config
                var dateTime = DateTime.UtcNow.AddSeconds(-5);
                Trace.WriteLine($"Going to test download downhole config for well id '{getwell.Id}', facility id '{facilityId}' on '{ddsClient.GetAssociatedUIS()}'.");
                string result = SurveillanceService.DownloadDownholeConfig(getwell.Id.ToString());
                Assert.AreEqual(true, result.Contains("Success"), $"Failed to 'Set' DH Setup on well id '{getwell.Id}' and facility id '{facilityId}'. Error: {result}");

                // Wait for few seconds to make sure that dds transactions are available in DDS
                System.Threading.Thread.Sleep(5000);

                List<DgTransaction> dgWrites = new List<DgTransaction>();
                // Get the last transaction of DHSetup data groups.
                var dgInstance1 = ddsClient.GetDataGroupInstanceByFacilityAndDGTypeAndOrdinal(deviceId, "DHSetup1", 0);
                var dgInstance2 = ddsClient.GetDataGroupInstanceByFacilityAndDGTypeAndOrdinal(deviceId, "DHSetup2", 0);
                var dgTxs1 = ddsClient.GetDataGroupTransactionNewerThan(dgInstance1, dateTime, false, 1);
                var dgTxs2 = ddsClient.GetDataGroupTransactionNewerThan(dgInstance2, dateTime, false, 1);

                Assert.AreEqual(1, dgTxs1.Count, "Failed to get last transaction of DHSetup1 datagroup from DDS.");
                Assert.AreEqual(1, dgTxs2.Count, "Failed to get last transaction of DHSetup2 datagroup from DDS.");

                // Check that the transactions have tx type as 'S' i.e. Send
                var dgWriteTx1 = dgTxs1.FirstOrDefault();
                var dgWriteTx2 = dgTxs2.FirstOrDefault();
                Assert.AreEqual("S", dgWriteTx1.GetTxType(), "Unexpected transaction type in the DHSetup1 datagroup transaction.");
                dgWrites.Add(dgWriteTx1);

                Assert.AreEqual("S", dgWriteTx2.GetTxType(), "Unexpected transaction type in the DHSetup2 datagroup transaction.");
                dgWrites.Add(dgWriteTx2);

                if (facilityId.Contains("RPOC"))
                {
                    // For WellPilot RPOC, we download downhole configurations via three different data groups
                    var dgInstance3 = ddsClient.GetDataGroupInstanceByFacilityAndDGTypeAndOrdinal(deviceId, "DHSetup3", 0);
                    var dgTxs3 = ddsClient.GetDataGroupTransactionNewerThan(dgInstance3, dateTime, false, 1);
                    Assert.AreEqual(1, dgTxs3.Count, "Failed to get last transaction of DHSetup3 datagroup from DDS.");
                    var dgWriteTx3 = dgTxs3.FirstOrDefault();
                    Assert.AreEqual("S", dgWriteTx3.GetTxType(), "Unexpected transaction type in the DHSetup3 datagroup transaction.");
                    dgWrites.Add(dgWriteTx3);
                }

                string dgTxHeader = "";
                string dgTxData = "";
                string strError = "";
                string fromDevParms = "";
                var uisClient = GetUisClient();

                Stopwatch sw = new Stopwatch();
                sw.Start();

                // For an RPOC device, when we download the downhole configuration, StrokeLen is also forced which is part of
                // device specific metadata. This results in reload of the device in the UIS. As ATS is a light weight VM, it
                // takes time to reload the device in the UIS and it is required to check whether the device is loaded or not
                // before sending the next UIS command.
                if (facilityId.Contains("RPOC"))
                {
                    int maxIterations = 30;
                    int iterations = 0;
                    var devStats = uisClient.GetDeviceStatistics(deviceId);
                    for (iterations = 0; iterations < maxIterations; iterations++)
                    {
                        if (devStats.DeviceStatusText == "Running")
                            break;

                        System.Threading.Thread.Sleep(1000);
                        devStats = uisClient.GetDeviceStatistics(deviceId);
                    }

                    Assert.AreEqual("Running", devStats.DeviceStatusText, $"Device '{deviceId}' is not loaded in UIS even after waiting for 30 secs. Current DeviceStatus is '{devStats.DeviceStatusText}'.");
                }

                var dgInstance = ddsClient.GetDataGroupInstanceByFacilityAndDGTypeAndOrdinal(deviceId, "DHSetup", 0);
                bool status = uisClient.GetDataGroupTxData(dgInstance.dbKey, 120000, fromDevParms, out dgTxHeader, out dgTxData, out strError);
                sw.Stop();
                TimeSpan ts = sw.Elapsed;

                Assert.AreEqual(true, status, $"Failed to 'Get' DHSetup data from device for '{facilityId}' after waiting '{ts.ToString()}'. Error: {strError}");

                DgTransaction dgReadTx = new DgTransaction(dgTxHeader, dgTxData);
                AssertIfDHSetupReadWriteTxDataNotEqual(dgReadTx, dgWrites);
                WellConfigurationService.RemoveWellConfig(getwell.Id.ToString());
            }
            finally
            {
                if (!_wellsToRemove.Any())
                {
                    var addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(DefaultWellName));
                    if (addedWell != null)
                    {
                        _wellsToRemove.Add(addedWell);
                        if (!dcExisted && !_dataConnectionsToRemove.Any())
                        {
                            _dataConnectionsToRemove.Add(addedWell.DataConnection);
                        }
                    }
                }
            }
        }

        private void AssertIfSetPointsNotEqual(SetPointsDTO obj1, SetPointsDTO obj2, string detail)
        {
            var badValues = new List<Tuple<string, string, string>>();
            List<PropertyInfo> props = typeof(SetPointsDTO).GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
            foreach (PropertyInfo prop in props)
            {
                if (prop.Name.Equals("ErrorMessage")) { continue; }
                object val1 = prop.GetValue(obj1);
                object val2 = prop.GetValue(obj2);
                if (!Equals(val1, val2) && !prop.Name.Equals("TransactionTimeStamp"))
                {
                    badValues.Add(Tuple.Create(prop.Name, val1?.ToString(), val2?.ToString()));
                }
            }
            Assert.AreEqual(0, badValues.Count, "Setpoint data does not match " + detail + ": " + string.Join(", ", badValues.Select(t => t.Item1 + ": \"" + t.Item2 + "\" != \"" + t.Item3 + "\"")));
            Assert.AreNotEqual(obj1.TransactionTimeStamp, obj2.TransactionTimeStamp, "Timestamp should not match for send and get setpoints configuration");
        }

        private void AssertIfDHSetupReadWriteTxDataNotEqual(DgTransaction dgRead, List<DgTransaction> dgWrites)
        {
            string readDeidVal = "";
            string writeDeidVal = "";

            foreach (var dgWriteTx in dgWrites)
            {
                foreach (var deid in dgWriteTx.GetDgElements())
                {
                    if (deid.Name.ToString() == "UISCMDTYPE" || deid.Name.ToString() == "UISCMDPRMS")
                        continue;

                    dgRead.GetAnyAsString(deid.Name.ToString(), out readDeidVal);
                    dgWriteTx.GetAnyAsString(deid.Name.ToString(), out writeDeidVal);
                    double allowableDiff = 0.0001;
                    bool compare = Math.Abs(Convert.ToDouble(writeDeidVal) - Convert.ToDouble(readDeidVal)) < allowableDiff;
                    Assert.AreEqual(true, compare, $"Allowable diff is '{allowableDiff}'. Mismatch between read value '{readDeidVal}' and write value '{writeDeidVal}' for '{deid.Name.ToString()}' deid.");
                }
            }
        }

        private static CygNet.COMAPI.Interfaces.IDdsClient GetDdsClient()
        {
            var dss = GetDomainSiteService("DDS");

            CygNet.COMAPI.Interfaces.IDdsClient ddsClient = ClientProxyMgr.GetDdsClient(dss);

            try
            {
                ddsClient.Connect(dss);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to connect to {dss.GetDomainSiteService()}: {ex.Message}");
                Trace.WriteLine($"Failed to connect to {dss.GetDomainSiteService()}: {ex.ToString()}");
            }

            if (!ddsClient.IsConnected)
            {
                Trace.WriteLine($"Failed to connect to {dss.GetDomainSiteService()}.");
            }

            return ddsClient;
        }

        private static CygNet.COMAPI.Interfaces.IUisClient GetUisClient()
        {
            var dss = GetDomainSiteService("UIS");

            CygNet.COMAPI.Interfaces.IUisClient uisClient = ClientProxyMgr.GetUisClient(dss);
            try
            {
                uisClient.Connect(dss);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to connect to {dss.GetDomainSiteService()}: {ex.Message}");
                Trace.WriteLine($"Failed to connect to {dss.GetDomainSiteService()}: {ex.ToString()}");
            }

            if (!uisClient.IsConnected)
            {
                Trace.WriteLine($"Failed to connect to {dss.GetDomainSiteService()}.");
            }

            return uisClient;
        }

        private static CygNet.COMAPI.Core.DomainSiteService GetDomainSiteService(string service)
        {
            var dss = new CygNet.COMAPI.Core.DomainSiteService(Convert.ToUInt16(s_domain), s_site, service);
            Trace.WriteLine($"DSS for {service} is {dss.GetDomainSiteService()}");
            return dss;
        }
        private static CygNet.Data.Core.DomainSiteService GetDomainSiteService2(string service)
        {
            var dss = new CygNet.Data.Core.DomainSiteService(Convert.ToUInt16(s_domain), s_site, service);
            System.Diagnostics.Trace.WriteLine($"DSS for {service} is [{dss.DomainId}]{dss.SiteService}");
            return dss;
        }
        // This function will add a delay of maximum 30 seconds for UIS to drain its queue with the assumption
        // that even if the queue is not completely drained in this time period, UIS has bandwidth to process
        // the next command.
        private static void WaitForUISToDrainItsQueue(string callingMethod, string deviceId)
        {
            var uisClient = GetUisClient();
            var svcInfo = uisClient.GetServiceInfo(new List<string>() { "UIS_RESP_QUEUE", "UIS_PEND_QUEUE" });
            Trace.WriteLine($"Before waiting - For '{callingMethod}':  UIS_RESP_QUEUE size: '{svcInfo["UIS_RESP_QUEUE"]}', UIS_PEND_QUEUE size: '{svcInfo["UIS_PEND_QUEUE"]}'.");

            int queueSize = Convert.ToInt32(svcInfo["UIS_RESP_QUEUE"]) + Convert.ToInt32(svcInfo["UIS_PEND_QUEUE"]);
            int maxIterations = 30;
            int iterations = 0;
            for (iterations = 0; iterations < maxIterations; iterations++)
            {
                if (queueSize == 0)
                    break;

                System.Threading.Thread.Sleep(1000);
                svcInfo = uisClient.GetServiceInfo(new List<string>() { "UIS_RESP_QUEUE", "UIS_PEND_QUEUE" });
                queueSize = Convert.ToInt32(svcInfo["UIS_RESP_QUEUE"]) + Convert.ToInt32(svcInfo["UIS_PEND_QUEUE"]);
            }

            Trace.WriteLine($"After waiting - For '{callingMethod}':  UIS_RESP_QUEUE size:'{svcInfo["UIS_RESP_QUEUE"]}', UIS_PEND_QUEUE size: '{svcInfo["UIS_PEND_QUEUE"]}' after {iterations} seconds.");

            if (queueSize > 0)
            {
                // Delete the pending queues on the comm device.
                ClearPendingQueueOnCommDevice(deviceId);
            }
        }

        // This function is used to delete any pending queues on a comm device. For ATS and TeamCity,
        // we use same Comm. Device for each RPOC and SAM devices
        private static void ClearPendingQueueOnCommDevice(string deviceId)
        {
            string domainSiteService = $"[{s_domain}]{s_site}.{s_cvsService}";
            _uisClient.Connect(domainSiteService);
            _ddsClient.Connect(_uisClient.GetAssociatedDdsName());

            object commDevice;
            _ddsClient.GetDeviceProperty(deviceId, "DevCommId1", out commDevice);

            var snapShotXML = _uisClient.RetrieveQueueSnapshotXML(commDevice.ToString());

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(snapShotXML);

            var uisToken = xmlDoc.SelectSingleNode("UisQSnapshot").Attributes.GetNamedItem("UISToken").InnerText;
            XmlNodeList queueEntries = xmlDoc.SelectNodes("UisQSnapshot/QueueEntries/QEnt");
            foreach (var nodeQEntry in queueEntries)
            {
                var element = nodeQEntry as XmlElement;
                string commandName = element.GetElementsByTagName("CommandName").Item(0).InnerText;
                string deviceName = element.GetElementsByTagName("DeviceName").Item(0).InnerText;
                string queueId = element.GetElementsByTagName("QueueId").Item(0).InnerText;

                object strError;
                Trace.WriteLine($"Going to delete pending UIS queue {queueId} for device: '{deviceName}' and command: '{commandName}'.");
                bool status = _uisClient.DeleteQueueEntry(commDevice.ToString(), uisToken, queueId, out strError);
                Assert.AreEqual(true, status, $"Failed to delete pending UIS queue {queueId} for device: '{deviceName}' and command: '{commandName}'.");
                Trace.WriteLine($"Deleted pending UIS queue {queueId} for device: '{deviceName}' and command: '{commandName}'.");
            }
        }

        private void AddNonRRLModelFile(string wellId, string fileName, WellTypeId wellType, ModelFileOptionDTO modelOption)
        {
            WellDTO well = WellService.GetWell(wellId);

            var assemblyDTO = WellboreComponentService.GetAssemblyByWellId(wellId);
            Assert.IsNotNull(assemblyDTO);

            //add model file for well test data
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            ModelFileBase64DTO modelFile = new ModelFileBase64DTO()
            {
                Options = modelOption,
                ApplicableDate = well.CommissionDate.Value.AddMinutes(1).ToUniversalTime(),
                WellId = well.Id
            };
            byte[] fileAsByteArray = GetByteArray(Path, fileName);
            Assert.IsNotNull(fileAsByteArray);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            ModelFileValidationDataDTO ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);

            //add modelfile
            ModelFileService.AddWellModelFile(modelFile);

            ModelFileDTO newModelFile = ModelFileService.GetCurrentModelFile(wellId);
            Assert.IsNotNull(newModelFile);
        }

        #endregion helper_methods

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void DowntimeForWell()
        {
            try
            {
                DateTime date = DateTime.Now.ToUniversalTime().AddDays(-3);
                WellDTO well = AddRRLWell(GetFacilityId("RPOC_", 1));
                // Update Well Test Date in history : say 7 days back 

                WellTestDTO latestwelltest = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());
                latestwelltest.SampleDate = latestwelltest.SampleDate.AddDays(10);
                WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
                WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, latestwelltest));
                //   WellTestDataService.UpdateWellTestData

                AddRRLWell(GetFacilityId("RPOC_", 2));
                var downTimeAdd = new WellDowntimeAdditionDTO();
                WellDowntimeDTO downTime = downTimeAdd.Downtime = new WellDowntimeDTO();
                downTimeAdd.WellIds = new long[] { well.Id };
                downTime.CapturedDateTime = date;
                downTime.OutOfServiceCode = SurveillanceService.GetDownTimeStatusCodes().FirstOrDefault().DownTimeStatusId;
                long[] downtimeId = SurveillanceService.AddDownTimeForWell(downTimeAdd);
                Assert.AreEqual(downtimeId.Count(), 1, "Failed to add Downtime for the added well");

                WellFilterDTO wellFilter = WellService.GetWellFilter(null);
                UnitsValuesCollectionDTO<WellDowntimeUnitsDTO, WellDowntimeDTO> getDowntime = SurveillanceService.GetDownTimeMaintenanceForWells(wellFilter);
                Assert.IsNotNull(getDowntime, "Failed to Get downtimes for the added wells");
                Assert.AreEqual(1, getDowntime.Values.Count(), "Mismatch in the count of Downtimes");
                Assert.AreEqual(GetFacilityId("RPOC_", 1), getDowntime.Values.FirstOrDefault().WellName, "Incorrect Well name");
                WellFilterDTO welForemanFilter = new WellFilterDTO();
                welForemanFilter.welForemanValues = new List<WellFilterValueDTO> { wellFilter?.welEngineerValues?[1] };
                wellFilter = welForemanFilter;
                getDowntime = SurveillanceService.GetDownTimeMaintenanceForWells(wellFilter);
                Assert.IsNotNull(getDowntime, "Failed to Get downtimes for the added wells");
                Assert.AreEqual(0, getDowntime.Values.Count(), "Mismatch in the count of Downtimes");
                //Get Downtime for Well
                string wellId = WellService.GetAllWells().FirstOrDefault(x => x.Name == GetFacilityId("RPOC_", 1)).Id.ToString();
                UnitsValuesCollectionDTO<WellDowntimeUnitsDTO, WellDowntimeDTO> getDowntimeHistory = SurveillanceService.GetDownTimeHistoryForWell(wellId);
                Assert.AreEqual(1, getDowntimeHistory.Values.Count(), "Unable to retrieve the records");
                WellDowntimeDTO welldowntime = SurveillanceService.GetDownTimeForWell(well.Id.ToString());
                //Assertions for Add
                Assert.AreEqual(welldowntime.DownTimeCode, "LB", "DownTime Code mismatch");
                Assert.AreEqual(welldowntime.DownTimeDescription, "ARTIFICIAL LIFT EQUIP PREV MAINT", "DownTime Code Descriptoin mismatch");
                Assert.AreEqual(welldowntime.DownTimeStatus, "ON", "DownTime Status  mismatch");

                //Update
                getDowntimeHistory.Values.FirstOrDefault().OutOfServiceCode = SurveillanceService.GetDownTimeStatusCodes().LastOrDefault().DownTimeStatusId;
                getDowntimeHistory.Values.FirstOrDefault().CapturedDateTime = date.AddDays(1);
                long downtimeRecorId = SurveillanceService.UpdateDownTimeRecordForWell(getDowntimeHistory.Values.FirstOrDefault());
                getDowntimeHistory = SurveillanceService.GetDownTimeHistoryForWell(wellId);
                welldowntime = SurveillanceService.GetDownTimeForWell(well.Id.ToString());
                //Assertions for update
                Assert.AreEqual(welldowntime.DownTimeCode, "ZR", "DownTime Code mismatch");
                Assert.AreEqual(welldowntime.DownTimeDescription.Trim(), "ZONE/POOL RECOMPLETED", "DownTime Code Descriptoin mismatch");
                Assert.AreEqual(welldowntime.DownTimeStatus, "OFF", "DownTime Status  mismatch");



                Assert.AreEqual(1, getDowntimeHistory.Values.Count(), "Unable to retrieve the records");
                Assert.AreEqual(SurveillanceService.GetDownTimeStatusCodes().LastOrDefault().DownTimeStatusCode, getDowntimeHistory.Values.FirstOrDefault().DownTimeCode);
                Assert.AreEqual(date.AddDays(1).ToString(), getDowntimeHistory.Values.FirstOrDefault().CapturedDateTime.ToString());


                // Use API AddDownTimeForWellHistorical
                //  Add Another Downtime Now
                var downTimeAdd2 = new WellDowntimeAdditionDTO();
                WellDowntimeDTO downTime2 = downTimeAdd2.Downtime = new WellDowntimeDTO();
                downTimeAdd2.WellIds = new long[] { well.Id };
                downTime2.CapturedDateTime = date.AddDays(-2);
                downTime2.OutOfServiceCode = SurveillanceService.GetDownTimeStatusCodes().FirstOrDefault().DownTimeStatusId;
                long[] downtimeId2 = SurveillanceService.AddDownTimeForWell(downTimeAdd2);
                List<WellDowntimeAdditionDTO> dwtadddtlist = new List<WellDowntimeAdditionDTO>();
                dwtadddtlist.Add(downTimeAdd2);
                List<WellDowntimeAdditionDTO> retdwtlist = SurveillanceService.AddDownTimeForWellHistorical(dwtadddtlist);
                Assert.IsNotNull(retdwtlist, "Historical Downtime List was NULL");
                Assert.IsTrue(retdwtlist.Count == 1, "Historical Downtime Not added ");
                getDowntimeHistory = SurveillanceService.GetDownTimeHistoryForWell(wellId);
                Assert.AreEqual(2, getDowntimeHistory.Values.Count(), "No records should be avilable as per the filter criteria");
                double? oldlostoil = getDowntimeHistory.Values[0].LostOil;
                double? oldlostwater = getDowntimeHistory.Values[0].LostWater;
                double? oldlostgas = getDowntimeHistory.Values[0].LostGas;
                decimal oldtimeinstate = getDowntimeHistory.Values[0].TimeInState;
                Thread.Sleep(60 * 1000);// we cant have new >= oil at least in integration test 
                List<string> wellids = new List<string>();
                wellids.Add(wellId.ToString());
                SurveillanceService.ReCalculateLostProduction(wellids);// New values for Lost Oil, Lost water, Lost gas
                SurveillanceService.UpdateCurrentDowntimeRecords();//new value for Timeinstate updated 
                                                                   //   SurveillanceService.BuildWellCacheDownTime(<list of wellid strings)//used in migration of db one time 
                getDowntimeHistory = SurveillanceService.GetDownTimeHistoryForWell(wellId);
                // Since this API is used in scheduler Lost Oil will not update for same date , 
                // To update API should be caled Next day as it is part of Scheduler .So we can see olval = newval only here 
                double? newlostoil = getDowntimeHistory.Values[0].LostOil;
                double? newlostwater = getDowntimeHistory.Values[0].LostWater;
                double? newlostgas = getDowntimeHistory.Values[0].LostGas;
                decimal newtimeinstate = getDowntimeHistory.Values[0].TimeInState;

                // Ensure that on update values get updated
                Assert.IsTrue(newtimeinstate > oldtimeinstate, "Lost Oil Gas failed");
                Assert.IsTrue(newlostoil >= oldlostoil, "Lost Oil update failed");
                Assert.IsTrue(newlostwater >= oldlostwater, "Lost Water update failed");
                Assert.IsTrue(newlostgas >= oldlostgas, "Lost Oil Gas failed");

                //Remove
                foreach (var dwthistval in getDowntimeHistory.Values)
                {
                    bool remove = SurveillanceService.RemoveDownTimeRecordForWell(dwthistval);
                    Assert.IsTrue(remove, "Failed to remove the Downtime record");
                }
                getDowntimeHistory = SurveillanceService.GetDownTimeHistoryForWell(wellId);
                Assert.AreEqual(0, getDowntimeHistory.Values.Count(), "No records should be avilable as per the filter criteria");
            }
            finally
            {
                RemoveWell(GetFacilityId("RPOC_", 1));
                RemoveWell(GetFacilityId("RPOC_", 2));
            }
        }

        public void DowntimeForWellActiveInactive(string downstatuscode, int activeflag)
        {
            try
            {
                DateTime date = DateTime.Now.ToUniversalTime();
                WellDTO well = AddRRLWell(GetFacilityId("RPOC_", 1));
                AddRRLWell(GetFacilityId("RPOC_", 2));
                AddEditDownCodeWithNewActiveStatus(true, downstatuscode, activeflag);
                // Add New DownTime Record
                Trace.WriteLine("Edited DownCode With Staus " + activeflag);
                var downTimeAdd = new WellDowntimeAdditionDTO();
                WellDowntimeDTO downTime = downTimeAdd.Downtime = new WellDowntimeDTO();
                downTimeAdd.WellIds = new long[] { well.Id };
                downTime.CapturedDateTime = date;
                downTime.OutOfServiceCode = SurveillanceService.GetDownTimeStatusCodes().FirstOrDefault(x => x.DownTimeStatusCode == downstatuscode).DownTimeStatusId;
                WellGroupStatusDTO<object, object> wgsdto = new WellGroupStatusDTO<object, object>();
                wgsdto = SurveillanceService.AddDownTimeForWellWithGroupStatus(downTimeAdd);
                bool outcodeinservice = SurveillanceService.CheckDownTimeCodeIsInService(downTimeAdd);
                if (downstatuscode.ToUpper().Equals("ON"))
                {
                    Assert.IsTrue(outcodeinservice, $"Downcode added {downstatuscode} was OFF expected ON");
                }
                else
                {
                    Assert.IsFalse(outcodeinservice, $"Downcode added {downstatuscode} was ON expected OFF");
                }
                var wellFilter = new WellFilterDTO { };
                UnitsValuesCollectionDTO<WellDowntimeUnitsDTO, WellDowntimeDTO> downrecs = SurveillanceService.GetDownTimeMaintenanceForWells(wellFilter);
                Assert.AreEqual(1, downrecs.Values.Count(), "Failed to add Downtime for the added well");
                Trace.WriteLine("Added DownTime For Well + Active Status" + activeflag);
                //check With No filter Applied , Downtime record is avaialble RPOC-0001 has downtime added
                wellFilter = WellService.GetWellFilter(null);
                UnitsValuesCollectionDTO<WellDowntimeUnitsDTO, WellDowntimeDTO> getDowntime = SurveillanceService.GetDownTimeMaintenanceForWells(wellFilter);
                Assert.IsNotNull(getDowntime, "Failed to Get downtimes for the added wells");
                Assert.AreEqual(1, getDowntime.Values.Count(), "Mismatch in the count of Downtimes");
                Assert.AreEqual(GetFacilityId("RPOC_", 1), getDowntime.Values.FirstOrDefault().WellName, "Incorrect Well name");
                //Well Filter   Downtime record is NOT avaialble RPOC-0002 has downtime added
                WellFilterDTO welDownTimeFilter = new WellFilterDTO();
                welDownTimeFilter.welDownTimeValues = new List<WellFilterValueDTO> { wellFilter?.welEngineerValues?[1] };
                getDowntime = SurveillanceService.GetDownTimeMaintenanceForWells(welDownTimeFilter);
                Assert.IsNotNull(getDowntime, "Failed to Get downtimes for the added wells");
                Assert.AreEqual(0, getDowntime.Values.Count(), "Mismatch in the count of Downtimes");
                //Get Downtime for Well
                try
                {
                    UnitsValuesCollectionDTO<WellDowntimeUnitsDTO, WellDowntimeDTO> downtimeForBadWellId = SurveillanceService.GetDownTimeHistoryForWell("0");
                    Assert.Fail("Getting downtime for a non-existent well should throw an exception.");
                }
                catch { }
                string wellId = WellService.GetAllWells().FirstOrDefault(x => x.Name == GetFacilityId("RPOC_", 1)).Id.ToString();
                UnitsValuesCollectionDTO<WellDowntimeUnitsDTO, WellDowntimeDTO> getDowntimeHistory = SurveillanceService.GetDownTimeHistoryForWell(wellId);
                Assert.AreEqual(1, getDowntimeHistory.Values.Count(), "Unable to retrieve the records");
                //Update
                getDowntimeHistory.Values.FirstOrDefault().OutOfServiceCode = SurveillanceService.GetDownTimeStatusCodes().LastOrDefault().DownTimeStatusId;
                getDowntimeHistory.Values.FirstOrDefault().CapturedDateTime = date.AddDays(1);
                long downtimeRecorId = SurveillanceService.UpdateDownTimeRecordForWell(getDowntimeHistory.Values.FirstOrDefault());
                getDowntimeHistory = SurveillanceService.GetDownTimeHistoryForWell(wellId);
                Assert.AreEqual(1, getDowntimeHistory.Values.Count(), "Unable to retrieve the records");
                Assert.AreEqual(SurveillanceService.GetDownTimeStatusCodes().LastOrDefault().DownTimeStatusCode, getDowntimeHistory.Values.FirstOrDefault().DownTimeCode);
                Assert.AreEqual(date.AddDays(1).ToString(), getDowntimeHistory.Values.FirstOrDefault().CapturedDateTime.ToString());
                Trace.WriteLine("Updated  DownTime For Well + Active Status" + activeflag);
                long[] onwells = SurveillanceService.CheckWellsWithDownStatusON(downTimeAdd);
                if (downstatuscode.ToUpper().Equals("ON"))
                {
                    Assert.IsTrue(onwells.Length >= 1, $"One or more wells with Downcode {downstatuscode} not obtained");
                }
                else
                {
                    Assert.IsTrue(onwells.Length == 0, $"One or more wells with Downcode {downstatuscode}  obtained");
                }
                //Remove
                bool remove = SurveillanceService.RemoveDownTimeRecordForWell(getDowntimeHistory.Values.FirstOrDefault());
                Assert.IsTrue(remove, "Failed to remove the Downtime record");
                getDowntimeHistory = SurveillanceService.GetDownTimeHistoryForWell(wellId);
                Assert.AreEqual(0, getDowntimeHistory.Values.Count(), "No records should be avilable as per the filter criteria");
                Trace.WriteLine("Removed   DownTime For Well + Active Status" + activeflag);

            }
            finally
            {
                RemoveWell(GetFacilityId("RPOC_", 1));
                RemoveWell(GetFacilityId("RPOC_", 2));
                RemoveWellOnDemand(GetFacilityId("RPOC_", 1));
                RemoveWellOnDemand(GetFacilityId("RPOC_", 2));
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void DownTimeAdditionalFilter()
        {
            try
            {
                // Add New DownCode from Reference Table : Method does not stop If Code already existed ; next methods will update the active Fla for that downcode:
                AddEditDownCodeWithNewActiveStatus(false, "BT", 1);
                Trace.WriteLine("Added New DownCode in Ref Table on New DB");
                //Check with IsActive = true DownTime code selected  = Active == DownCode=OFF But Active
                DowntimeForWellActiveInactive("BT", 1);
                Trace.WriteLine("Verficiation completed for DownTime with Active status ");
                //Check with IsActive = false DownTime code selected is = Inactive DownCode=OFF But InActive
                DowntimeForWellActiveInactive("BT", 0);
                Trace.WriteLine("Verficiation completed for DownTime with Inactive status ");
                //Check with IsActive = false DownTime code selected is = Inactive DownCode=ON
                DowntimeForWellActiveInactive("ON", 1);
                Trace.WriteLine("Verficiation completed for DownTime with ON status ");
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                AddEditDownCodeWithNewActiveStatus(true, "ON", 1, true);
            }
        }


        public void AddEditDownCodeWithNewActiveStatus(bool editExistingCode, string existingStatusCode, int isactivecode, bool revert = false)
        {
            // Edit Existing DownCode Active Status
            if (editExistingCode)
            {
                //  long codePrimaryKey = SurveillanceService.GetDownTimeStatusCodes().FirstOrDefault(x => x.DownTimeStatusCode == "DM").DownTimeStatusId;
                long codePrimaryKey = SurveillanceService.GetDownTimeStatusCodes().FirstOrDefault(x => x.DownTimeStatusCode == existingStatusCode).DownTimeStatusId;

                //Get Index from
                DBEntityDTO dbentdto = GetTableData("r_DownCode");
                int indexofAttribute = dbentdto.Attributes.ToList().IndexOf(dbentdto.Attributes.FirstOrDefault(x => x.AttributeName == "rdcIsActive"));
                MetaDataDTO[] updateMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForUpdate("r_DownCode", codePrimaryKey.ToString());
                MetaDataDTO toBeUpdatedDTO = updateMetaDatas.FirstOrDefault(x => x.ColumnName == "rdcIsActive");
                toBeUpdatedDTO.DataValue = isactivecode;
                toBeUpdatedDTO = updateMetaDatas.FirstOrDefault(x => x.ColumnName == "rdcDescription");
                if (isactivecode == 0)
                {
                    toBeUpdatedDTO.DataValue = existingStatusCode + " Code (Inactive)";
                }
                else
                {
                    if (revert == true)
                    {
                        toBeUpdatedDTO.DataValue = "IN SERVICE";
                    }
                    else
                    {
                        toBeUpdatedDTO.DataValue = existingStatusCode + " Code (Active)";
                    }
                }
                DBEntityService.UpdateReferenceData(updateMetaDatas);
            }
            else
            {
                try
                {
                    //Add new DownCode with desired Active Status
                    MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd("r_DownCode");
                    foreach (MetaDataDTO addMetaData in addMetaDatas)
                    {
                        //User Input
                        if (addMetaData.Visible && addMetaData.Editable)
                        {
                            if (addMetaData.Title.Equals("Code Type"))
                            {
                                addMetaData.DataValue = "DownTime";
                            }
                            if (addMetaData.Title.Equals("OS Code"))
                            {
                                addMetaData.DataValue = existingStatusCode;
                            }
                            if (addMetaData.Title.Equals("Description"))
                            {
                                if (isactivecode == 0)
                                {
                                    addMetaData.DataValue = existingStatusCode + " Code (Inactive)";
                                }
                                else
                                {
                                    addMetaData.DataValue = existingStatusCode + " Code (Active)";
                                }
                            }
                        }
                        if (addMetaData.Visible && addMetaData.DataDisplayType == "RADIO")
                        {
                            if (addMetaData.Title.Equals("Is Active"))
                            {
                                addMetaData.DataValue = isactivecode;
                            }
                        }
                    }
                    DBEntityService.AddReferenceData(addMetaDatas);
                }
                catch (Exception)
                {
                    // If Try to Add Same DownCod eGet exception But do not throw //
                }
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void DowntimeForWellforGroupStatus()
        {
            try
            {
                DateTime date = DateTime.Now.ToUniversalTime();
                WellDTO well = AddRRLWell(GetFacilityId("RPOC_", 1));

                var downTimeAdd = new WellDowntimeAdditionDTO();
                WellDowntimeDTO downTime = downTimeAdd.Downtime = new WellDowntimeDTO();

                downTimeAdd.WellIds = new long[] { well.Id };
                downTime.CapturedDateTime = date;
                DownTimeStatusCodeDTO downCode = SurveillanceService.GetDownTimeStatusCodes().LastOrDefault();
                downTime.OutOfServiceCode = downCode.DownTimeStatusId;
                downTime.DownTimeStatus = downCode.IsWellOn;

                //Creating WellGroupStatusQuery DTO to get Group Status data for RRL
                WellGroupStatusQueryDTO query = new WellGroupStatusQueryDTO();
                WellFilterDTO wellbyFilters = WellService.GetWellFilter(null);
                query.WellType = WellTypeId.RRL;
                query.WellFilter = wellbyFilters;

                WellGroupStatusDTO<object, object> wellStatusData = SurveillanceService.AddDownTimeForWellWithGroupStatus(downTimeAdd);

                Assert.IsNotNull(wellStatusData.WellType, "Failed to add well");
                Assert.AreEqual(WellTypeId.RRL, wellStatusData.WellType, "Added well type is not matching");

                object[] wellsStatus = wellStatusData.Status;

                for (int i = 0; i < wellsStatus.Count(); i++)
                {
                    var RRLwellStatusValue = wellsStatus[i] as RRLWellStatusValueDTO;

                    Trace.WriteLine("***************For WellName: ******" + RRLwellStatusValue.WellName);
                    Assert.AreEqual(downCode.DownTimeStatusCode, RRLwellStatusValue.DownTimeCode, "Failed to add Downtime for the added well");
                    Assert.AreEqual(downCode.DownTimeStatus, RRLwellStatusValue.DownTimeDescription, "Failed to add Downtime for the added well");
                    Assert.AreEqual(downCode.IsWellOn, RRLwellStatusValue.DownTimeStatus, "Failed to add Downtime for the added well");
                    Assert.AreEqual("Succeeded", RRLwellStatusValue.CommStatus, "Comm status should match for well ");
                }

            }
            finally
            {
                RemoveWell(GetFacilityId("RPOC_", 1));
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void RemoveMultipleDowntimeRecords()
        {
            {
                string facilityId;
                if (s_isRunningInATS)
                    facilityId = "RPOC_00001";
                else
                    facilityId = "RPOC_0001";
                try
                {

                    WellDTO well = AddRRLWell(facilityId);
                    DateTime date = DateTime.Now.ToUniversalTime();
                    var downTimeAdd = new WellDowntimeAdditionDTO();
                    WellDowntimeDTO downTime = downTimeAdd.Downtime = new WellDowntimeDTO();
                    downTimeAdd.WellIds = new long[] { well.Id };
                    downTime.CapturedDateTime = date;
                    downTime.OutOfServiceCode = SurveillanceService.GetDownTimeStatusCodes().First().DownTimeStatusId;
                    long[] downtimeId = SurveillanceService.AddDownTimeForWell(downTimeAdd);
                    Assert.AreEqual(downtimeId.Count(), 1, "Failed to add Downtime for the added well");
                    WellDowntimeDTO downTime2 = downTimeAdd.Downtime = new WellDowntimeDTO();
                    downTimeAdd.WellIds = new long[] { well.Id };
                    downTime2.CapturedDateTime = date.AddDays(-1);
                    downTime2.OutOfServiceCode = SurveillanceService.GetDownTimeStatusCodes().First().DownTimeStatusId;
                    long[] downtimeId2 = SurveillanceService.AddDownTimeForWell(downTimeAdd);
                    WellFilterDTO filters = WellService.GetWellFilter(null);
                    UnitsValuesCollectionDTO<WellDowntimeUnitsDTO, WellDowntimeDTO> getDowntime = SurveillanceService.GetDownTimeMaintenanceForWells(filters);
                    Assert.IsNotNull(getDowntime, "Failed to Get downtimes for the added wells");
                    string wellId = well.Id.ToString();
                    UnitsValuesCollectionDTO<WellDowntimeUnitsDTO, WellDowntimeDTO> getDowntimeHistory = SurveillanceService.GetDownTimeHistoryForWell(wellId);
                    Assert.AreEqual(2, getDowntimeHistory.Values.Count(), "Failed to retrieve downtime history");
                    wellId = WellService.GetAllWells().FirstOrDefault(x => x.Name == well.Name).Id.ToString();
                    getDowntimeHistory = SurveillanceService.GetDownTimeHistoryForWell(wellId);
                    getDowntimeHistory.Values.First().OutOfServiceCode = SurveillanceService.GetDownTimeStatusCodes().Last().DownTimeStatusId;
                    getDowntimeHistory.Values.Last().OutOfServiceCode = SurveillanceService.GetDownTimeStatusCodes().Last().DownTimeStatusId;
                    SurveillanceService.UpdateMultipleDownTimeRecordForWell(getDowntimeHistory.Values);
                    SurveillanceService.RemoveMultipleDownTimeRecordForWell(getDowntimeHistory.Values);
                }
                finally
                {
                    RemoveWell(facilityId);
                }
            }

        }

        //FRWM - 2144 - Automatic DownTime Calculations
        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetAutomaticDownTimeCalculation()
        {
            string ESPfacilityId = GetFacilityId("ESPWELL_", 1);
            string GLfacilityId = GetFacilityId("GLWELL_", 1);
            string NFWfacilityId = GetFacilityId("NFWWELL_", 1);
            string PGLfacilityId = GetFacilityId("PGLWELL_", 1);
            string RRLfacilityId = GetFacilityId("RPOC_", 1);
            //string PCPfacilityId = GetFacilityId("WFTA1K_",1);
            try
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");

                //Adding RRL / NonRRL Wells
                #region Adding RRL/NonRRL Wells
                WellDTO ESPWell = AddNonRRLWell(ESPfacilityId, WellTypeId.ESP, false, CalibrationMethodId.LFactor);
                WellDTO GLWell = AddNonRRLWell(GLfacilityId, WellTypeId.GLift, false, CalibrationMethodId.LFactor);
                WellDTO NFWWell = AddNonRRLWell(NFWfacilityId, WellTypeId.NF, false, CalibrationMethodId.LFactor);
                WellDTO PGLWell = AddNonRRLWell(PGLfacilityId, WellTypeId.PLift, false, CalibrationMethodId.LFactor);
                WellDTO RRLWell = AddNonRRLWell(RRLfacilityId, WellTypeId.RRL, false, CalibrationMethodId.LFactor);
                //WellDTO PCPWell = AddNonRRLWell(PCPfacilityId, WellTypeId.PCP, false, CalibrationMethodId.LFactor);
                #endregion Adding RRL/NonRRL Wells

                //Getting Default Design Speed Steps
                #region Defaulttoolbox setting
                var getDefaultToleranceLimit = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.AUTOMATIC_DOWNTIME_TOLERANCE_LIMIT);
                Assert.AreEqual((double)5, getDefaultToleranceLimit, "Default DowntTime Tolarnace Mismatched");
                SetValuesInSystemSettings(SettingServiceStringConstants.AUTOMATIC_DOWNTIME_TOLERANCE_LIMIT, "2");

                var getDefaultDownTimeCode = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.AUTOMATIC_DOWNTIME_DEFAULT_DOWNTIME_CODE);
                Assert.AreEqual("DM (DORMANT)", getDefaultDownTimeCode, "Default Downcode Is Mismatched");

                var getDefaultAutomaticDowntimeLogging = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.ENABLE_AUTOMATIC_DOWNTIME_LOGGING);
                Assert.AreEqual((double)0, getDefaultAutomaticDowntimeLogging, "Default Logging Condition Mismatched");
                SetValuesInSystemSettings(SettingServiceStringConstants.ENABLE_AUTOMATIC_DOWNTIME_LOGGING, "1");
                #endregion Defaulttoolbox setting

                //Running AutomaticDownTime Calculations For All Wells
                #region AutomaticDownTimeCommand
                //RunAnalysisTaskScheduler("-runAutomaticDownTimeCalculations");
                bool recordcalculatedforESP = SurveillanceService.CalculateDownTimeForWell(ESPWell.Id.ToString());
                bool recordcalculatedforGL = SurveillanceService.CalculateDownTimeForWell(GLWell.Id.ToString());
                bool recordcalculatedforNFW = SurveillanceService.CalculateDownTimeForWell(NFWWell.Id.ToString());
                bool recordcalculatedforPGL = SurveillanceService.CalculateDownTimeForWell(PGLWell.Id.ToString());
                bool recordcalculatedforRRL = SurveillanceService.CalculateDownTimeForWell(RRLWell.Id.ToString());
                //bool recordcalculatedforPCP = SurveillanceService.CalculateDownTimeForWell(PCPWell.Id.ToString());
                #endregion AutomaticDownTimeCommand

                int i = 0;
                if (recordcalculatedforESP == true) { i = i + 1; }
                if (recordcalculatedforGL == true) { i = i + 1; }
                if (recordcalculatedforNFW == true) { i = i + 1; }
                if (recordcalculatedforPGL == true) { i = i + 1; }
                if (recordcalculatedforRRL == true) { i = i + 1; }

                //Downtime Inservice count on Downtime screen
                #region DowntimeCount
                WellFilterDTO wellFilter = WellService.GetWellFilter(null);
                UnitsValuesCollectionDTO<WellDowntimeUnitsDTO, WellDowntimeDTO> downrecs = SurveillanceService.GetDownTimeMaintenanceForWells(wellFilter);
                Assert.AreEqual(i, downrecs.Values.Count(), "DowntTime records are mismatched");
                #endregion DowntimeCount

            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                SetValuesInSystemSettings(SettingServiceStringConstants.AUTOMATIC_DOWNTIME_TOLERANCE_LIMIT, "5");
                SetValuesInSystemSettings(SettingServiceStringConstants.ENABLE_AUTOMATIC_DOWNTIME_LOGGING, "0");
                RemoveWell(ESPfacilityId);
                RemoveWell(GLfacilityId);
                RemoveWell(NFWfacilityId);
                RemoveWell(PGLfacilityId);
                RemoveWell(RRLfacilityId);
                //RemoveWell(PCPfacilityId);
            }

        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void AdditionalUDCTest()
        {
            //Giving Static Prefix for Non RRL wells so that test will run on ATS & TeamCity as well
            VerificationOfAdditionalUDC("RRL", "RPOC_", "RRLUDCAddedByAPITest", "QTLIQT", WellTypeId.RRL);
            VerificationOfAdditionalUDC("ESP", "ESPWELL_", "ESPUDCAddedByAPITest", "PRTUBXIN", WellTypeId.ESP);
            VerificationOfAdditionalUDC("NFW", "NFWWELL_", "NFWUDCAddedByAPITest", "PRTUBXIN", WellTypeId.NF);
            VerificationOfAdditionalUDC("GL", "GLWELL_", "GLUDCAddedByAPITest", "PRTUBXIN", WellTypeId.GLift);
            VerificationOfAdditionalUDC("GI", "GASINJWELL_", "GIUDCAddedByAPITest", "PRTUBXIN", WellTypeId.GInj);
            VerificationOfAdditionalUDC("WI", "WATERINJWELL_", "WIUDCAddedByAPITest", "PRTUBXIN", WellTypeId.WInj);
            VerificationOfAdditionalUDC("PL", s_prefixForPLFacility, "PLUDCAddedByAPITest", "PRTUBXIN", WellTypeId.PLift);
        }
        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void AdditionalUDCTestForStringType()
        {
            //Giving Static Prefix for Non RRL wells so that test will run on ATS & TeamCity as well
            //Clean up Additional UDC for all Well types to default before this test
            //
            SetValuesInSystemSettings(SettingServiceStringConstants.RRL_GROUP_STATUS_EXTRA_UDCS, "");
            SetValuesInSystemSettings(SettingServiceStringConstants.GL_GROUP_STATUS_EXTRA_UDCS, "");
            SetValuesInSystemSettings(SettingServiceStringConstants.ESP_GROUP_STATUS_EXTRA_UDCS, "");
            SetValuesInSystemSettings(SettingServiceStringConstants.NF_GROUP_STATUS_EXTRA_UDCS, "");
            SetValuesInSystemSettings(SettingServiceStringConstants.GI_GROUP_STATUS_EXTRA_UDCS, "");
            SetValuesInSystemSettings(SettingServiceStringConstants.WI_GROUP_STATUS_EXTRA_UDCS, "");
            SetValuesInSystemSettings(SettingServiceStringConstants.PL_GROUP_STATUS_EXTRA_UDCS, "");
            //Giving Static Prefix for Non RRL wells so that test will run on ATS & TeamCity as well
            //Enssure We Create New String UDC in CygNet
            AddUDCTest();
            VerificationOfAdditionalUDCStringType("RRL", "RPOC_", "RRLStringUDCAPITest", "RRLUSER", WellTypeId.RRL);
            VerificationOfAdditionalUDCStringType("ESP", "ESPWELL_", "ESPStringUDCAPITest", "ESPUSER", WellTypeId.ESP);
            VerificationOfAdditionalUDCStringType("NFW", "NFWWELL_", "NFWStringUDCAPITest", "NFWUSER", WellTypeId.NF);
            VerificationOfAdditionalUDCStringType("GL", "GLWELL_", "GLStringUDCAPITest", "GLUSER", WellTypeId.GLift);
            VerificationOfAdditionalUDCStringType("GI", "GASINJWELL_", "GIStringUDCAPITest", "GIUSER", WellTypeId.GInj);
            VerificationOfAdditionalUDCStringType("WI", "WATERINJWELL_", "WIStringUDCAPITest", "WIUSER", WellTypeId.WInj);
            VerificationOfAdditionalUDCStringType("PL", s_prefixForPLFacility, "PLStringUDCAPITest", "PLUSER", WellTypeId.PLift);
        }

        public void VerificationOfAdditionalUDC(string wellType, string facilityId, string name, string additionalUDC, WellTypeId wellTypeId)
        {
            string facilityId1;

            facilityId1 = GetFacilityId(facilityId, 1);

            // System setting
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = null;

            WellDTO wellDTO = SetDefaultFluidType(new WellDTO()
            {
                Name = DefaultWellName,
                FacilityId = facilityId1,
                DataConnection = GetDefaultCygNetDataConnection(),
                IntervalAPI = "IntervalAPI",
                SubAssemblyAPI = "SubAssemblyAPI",
                AssemblyAPI = "AssemblyAPI",
                CommissionDate = DateTime.Today.AddYears(-2),
                WellType = wellTypeId
            });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = wellDTO });

            switch (wellTypeId)
            {
                case WellTypeId.RRL:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.RRL_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.ESP:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.ESP_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.GLift:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.GL_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.NF:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.NF_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.GInj:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.GI_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.WInj:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.WI_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.PLift:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.PL_GROUP_STATUS_EXTRA_UDCS);
                    break;
            }

            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well1 = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            _wellsToRemove.Add(well1);

            WellFilterDTO wellbyFilters = new WellFilterDTO();
            WellGroupStatusQueryDTO Query = new WellGroupStatusQueryDTO();
            Query.WellType = well1.WellType;
            Query.WellFilter = wellbyFilters;

            SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
            string originalSetting = settingValue.StringValue;
            ConfigurableUDCInfo infos = new ConfigurableUDCInfo();
            infos.Parse(settingValue.StringValue);
            try
            {
                //Clean up if any other UDC was added by some other test
                if (infos.Info.Count > 0)
                {
                    Trace.WriteLine($"There were already {infos.Info.Count}additional UDC'S present which this test is not expecting ..So removing them..");
                    List<int> dickeylist = new List<int>();
                    foreach (var udckyey in infos.Info.Keys)
                    {
                        dickeylist.Add(udckyey);
                        Trace.WriteLine($"Configurable UDC {infos.Info[udckyey].Name} Was Present for this Group");
                    }
                    foreach (var dicKeyt in dickeylist)
                    {
                        infos.Info.Remove(dicKeyt);
                        Trace.WriteLine($"Removed Pre Exisiting Key {dicKeyt.ToString()}for Addtional UDC Group");
                    }
                }
                infos.Info.Add(infos.Info.Count() + 1, new ConfigurableUDC(name, additionalUDC, null, UnitCategory.None, CygNetPointType.Analog, false));
                settingValue.StringValue = infos.GetSettingValue();
                SettingService.SaveSystemSetting(settingValue);
                // **************************  We are now Sending Scan Command to pull data as Group Status wont load 
                SurveillanceService.IssueCommandForSingleWell(well1.Id, WellCommand.DemandScan.ToString());
                Thread.Sleep(2000);
                WellGroupStatusDTO<object, object> WellGroups = SurveillanceServiceClient.GetWellGroupStatus(Query);
                WellStatusOptionalFieldInfoDTO[] aOptionalUDC = WellGroups.OptionalFieldInfo;
                object[] wellsStatus = WellGroups.Status;

                Trace.WriteLine("Additional UDC Name : " + aOptionalUDC.FirstOrDefault(x => x.Name == name).Name);
                Assert.IsNotNull(aOptionalUDC.FirstOrDefault(x => x.Name == name), "Additional UDC is not present after adding new UDC in System Setting");

                switch (wellTypeId)
                {
                    case WellTypeId.RRL:
                        for (int i = 0; i < wellsStatus.Length; i++)
                        {
                            var wellStatusValue = wellsStatus[i] as RRLWellStatusValueDTO;
                            decimal addedUDC = Math.Round(Convert.ToDecimal(wellStatusValue.OptionalField01), 2);
                            decimal existingUDC = Math.Round(Convert.ToDecimal(wellStatusValue.InferredProductionToday.Value), 2);
                            Trace.WriteLine("Value of Added RRL UDC: " + addedUDC);
                            Trace.WriteLine("Value of Existing RRL UDC: " + existingUDC);
                            Trace.WriteLine("Diffrence is " + (Math.Abs(addedUDC - existingUDC)));
                            Assert.IsTrue((Math.Abs(addedUDC - existingUDC) <= (decimal)0.01), "Data value retrieved from new RRL UDC is not correct");
                        }

                        break;

                    case WellTypeId.ESP:
                        for (int i = 0; i < wellsStatus.Length; i++)
                        {
                            var wellStatusValue = wellsStatus[i] as ESPWellStatusValueDTO;
                            decimal addedUDC = Math.Round(Convert.ToDecimal(wellStatusValue.OptionalField01), 2);
                            decimal existingUDC = Math.Round(Convert.ToDecimal(wellStatusValue.TubingPressure.Value), 2);
                            Trace.WriteLine("Value of Added ESP UDC: " + addedUDC);
                            Trace.WriteLine("Value of Existing ESP UDC: " + existingUDC);
                            Trace.WriteLine("Diffrence is " + (Math.Abs(addedUDC - existingUDC)));
                            Assert.IsTrue((Math.Abs(addedUDC - existingUDC) <= (decimal)0.01), "Data value retrieved from new ESP UDC is not correct");
                        }
                        break;

                    case WellTypeId.GLift:
                        for (int i = 0; i < wellsStatus.Length; i++)
                        {
                            bool flagUAddUDCISNULL = true;
                            var wellStatusValue = wellsStatus[i] as GLWellStatusValueDTO;
                            try
                            {
                                object adudc = wellStatusValue.OptionalField01;
                                double? gltubpres = wellStatusValue.TubingPressure;
                                if (gltubpres.HasValue)
                                {
                                    flagUAddUDCISNULL = false;
                                }
                            }
                            catch (NullReferenceException ne)
                            {
                                Trace.WriteLine($"Got NullReferenceException  only on TeamCity{ne.ToString()}");
                            }

                            Trace.WriteLine($"Update GL  Well Status Cache  for Optional UDC  NULL /No Value ?? : {flagUAddUDCISNULL}");


                            int attempt = 0;
                            while (flagUAddUDCISNULL)
                            {
                                SurveillanceService.IssueCommandForSingleWell(well1.Id, WellCommand.DemandScan.ToString());
                                flagUAddUDCISNULL = wellStatusValue.TubingPressure.HasValue ? false : true;
                                Trace.WriteLine($"Update  Well Status Cache : {flagUAddUDCISNULL}  Attemmpt number :  {attempt}");
                                Thread.Sleep(3000);
                                attempt++;
                                if (attempt == 5)
                                {
                                    break;
                                }
                            }
                            if (flagUAddUDCISNULL)
                            {
                                Trace.WriteLine("Well Status Cache not updated after 5 attempts only for GL Well Tubing Pressure : ");
                                Assert.Fail("GL Well status Cashing Issue : Test Was failed as Well Stauts cache not updated after 5 Scan attempts ");
                            }
                            if (!flagUAddUDCISNULL)
                            {
                                Trace.WriteLine($"GL Tubing Pressure was : {wellStatusValue.TubingPressure}");
                                Trace.WriteLine($"GL Addtional UDC Tubing Pressure was : {wellStatusValue.OptionalField01}");
                                decimal addedUDC = Math.Round(Convert.ToDecimal(wellStatusValue.OptionalField01), 2);
                                decimal existingUDC = Math.Round(Convert.ToDecimal(wellStatusValue.TubingPressure.Value), 2);
                                Trace.WriteLine("Value of Added GL UDC: " + addedUDC);
                                Trace.WriteLine("Value of Existing GL UDC: " + existingUDC);
                                Trace.WriteLine("Diffrence is " + (Math.Abs(addedUDC - existingUDC)));
                                Assert.IsTrue((Math.Abs(addedUDC - existingUDC) <= (decimal)0.01), "Data value retrieved from new GL UDC is not correct");
                            }
                        }
                        break;

                    case WellTypeId.NF:
                        for (int i = 0; i < wellsStatus.Length; i++)
                        {
                            var wellStatusValue = wellsStatus[i] as NFWellStatusValueDTO;
                            decimal addedUDC = Math.Round(Convert.ToDecimal(wellStatusValue.OptionalField01), 2);
                            decimal existingUDC = Math.Round(Convert.ToDecimal(wellStatusValue.TubingPressure.Value), 2);
                            Trace.WriteLine("Value of Added NFW UDC: " + addedUDC);
                            Trace.WriteLine("Value of Existing NFW UDC: " + existingUDC);
                            Trace.WriteLine("Diffrence is " + (Math.Abs(addedUDC - existingUDC)));
                            Assert.IsTrue((Math.Abs(addedUDC - existingUDC) <= (decimal)0.01), "Data value retrieved from new NFW UDC is not correct");
                        }
                        break;

                    case WellTypeId.GInj:
                        for (int i = 0; i < wellsStatus.Length; i++)
                        {
                            var wellStatusValue = wellsStatus[i] as GIWellStatusValueDTO;
                            decimal addedUDC = Math.Round(Convert.ToDecimal(wellStatusValue.OptionalField01), 2);
                            decimal existingUDC = Math.Round(Convert.ToDecimal(wellStatusValue.TubingPressure.Value), 2);
                            Trace.WriteLine("Value of Added GI UDC: " + addedUDC);
                            Trace.WriteLine("Value of Existing GI UDC: " + existingUDC);
                            Trace.WriteLine("Diffrence is " + (Math.Abs(addedUDC - existingUDC)));
                            Assert.IsTrue((Math.Abs(addedUDC - existingUDC) <= (decimal)0.01), "Data value retrieved from new GI UDC is not correct");
                        }
                        break;

                    case WellTypeId.WInj:
                        for (int i = 0; i < wellsStatus.Length; i++)
                        {
                            var wellStatusValue = wellsStatus[i] as WIWellStatusValueDTO;
                            decimal addedUDC = Math.Round(Convert.ToDecimal(wellStatusValue.OptionalField01), 2);
                            decimal existingUDC = Math.Round(Convert.ToDecimal(wellStatusValue.TubingPressure.Value), 2);
                            Trace.WriteLine("Value of Added WI UDC: " + addedUDC);
                            Trace.WriteLine("Value of Existing WI UDC: " + existingUDC);
                            Trace.WriteLine("Diffrence is " + (Math.Abs(addedUDC - existingUDC)));
                            Assert.IsTrue((Math.Abs(addedUDC - existingUDC) <= (decimal)0.01), "Data value retrieved from new WI UDC is not correct");
                        }
                        break;

                    case WellTypeId.PLift:
                        for (int i = 0; i < wellsStatus.Length; i++)
                        {
                            var wellStatusValue = wellsStatus[i] as PGLWellStatusValueDTO;
                            decimal addedUDC = Math.Round(Convert.ToDecimal(wellStatusValue.OptionalField01), 2);
                            decimal existingUDC = Math.Round(Convert.ToDecimal(wellStatusValue.TubingPressure.Value), 2);
                            Trace.WriteLine("Value of Added PGL UDC: " + addedUDC);
                            Trace.WriteLine("Value of Existing PGL UDC: " + existingUDC);
                            Trace.WriteLine("Diffrence is " + (Math.Abs(addedUDC - existingUDC)));
                            Assert.IsTrue((Math.Abs(addedUDC - existingUDC) <= (decimal)0.01), "Data value retrieved from new PGL UDC is not correct");
                        }
                        break;
                }
            }
            finally
            {
                //Removing Well
                WellService.RemoveWellByWellId(well1.Id.ToString());
                //reverting System Setting changes
                settingValue.StringValue = originalSetting;
                SettingService.SaveSystemSetting(settingValue);
            }

        }


        public void VerificationOfAdditionalUDCStringType(string wellType, string facilityId, string name, string additionalUDC, WellTypeId wellTypeId)
        {
            string facilityId1;

            facilityId1 = GetFacilityId(facilityId, 1);

            // System setting
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = null;

            WellDTO wellDTO = SetDefaultFluidType(new WellDTO()
            {
                Name = DefaultWellName,
                FacilityId = facilityId1,
                DataConnection = GetDefaultCygNetDataConnection(),
                IntervalAPI = "IntervalAPI",
                SubAssemblyAPI = "SubAssemblyAPI",
                AssemblyAPI = "AssemblyAPI",
                CommissionDate = DateTime.Today.AddYears(-2),
                WellType = wellTypeId
            });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = wellDTO });

            switch (wellTypeId)
            {
                case WellTypeId.RRL:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.RRL_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.ESP:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.ESP_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.GLift:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.GL_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.NF:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.NF_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.GInj:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.GI_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.WInj:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.WI_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.PLift:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.PL_GROUP_STATUS_EXTRA_UDCS);
                    break;
            }

            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well1 = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            _wellsToRemove.Add(well1);

            WellFilterDTO wellbyFilters = new WellFilterDTO();
            WellGroupStatusQueryDTO Query = new WellGroupStatusQueryDTO();
            Query.WellType = well1.WellType;
            Query.WellFilter = wellbyFilters;

            SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
            string originalSetting = settingValue.StringValue;
            ConfigurableUDCInfo infos = new ConfigurableUDCInfo();
            infos.Parse(settingValue.StringValue);
            try
            {
                infos.Info.Add(infos.Info.Count() + 1, new ConfigurableUDC(name, additionalUDC, null, UnitCategory.None, CygNetPointType.String, false));
                settingValue.StringValue = infos.GetSettingValue();
                SettingService.SaveSystemSetting(settingValue);
                // **************************  We are now Sending Scan Command to pull data as Group Status wont load 
                SurveillanceService.IssueCommandForSingleWell(well1.Id, WellCommand.DemandScan.ToString());
                WellGroupStatusDTO<object, object> WellGroups = SurveillanceServiceClient.GetWellGroupStatus(Query);
                WellStatusOptionalFieldInfoDTO[] aOptionalUDC = WellGroups.OptionalFieldInfo;
                object[] wellsStatus = WellGroups.Status;

                Trace.WriteLine("Additional UDC Name : " + aOptionalUDC.FirstOrDefault(x => x.Name == name).Name);
                Assert.IsNotNull(aOptionalUDC.FirstOrDefault(x => x.Name == name), "Additional UDC is not present after adding new UDC in System Setting");


                switch (wellTypeId)
                {
                    case WellTypeId.RRL:
                        for (int i = 0; i < wellsStatus.Length; i++)
                        {
                            var wellStatusValue = wellsStatus[i] as RRLWellStatusValueDTO;
                            string addudcvalue = (string)wellStatusValue.OptionalField01;
                            Trace.WriteLine($"Additional UDC value from RRLWellStatus : {addudcvalue}");
                            if (AuthenticatedUser.Name.Length > 16)
                            {
                                Assert.IsTrue(AuthenticatedUser.Name.Contains(addudcvalue), "Mismatch in Additional String UDC Value");
                            }
                            else
                            {
                                Assert.AreEqual(AuthenticatedUser.Name, addudcvalue, "Mismatch in Additional String UDC Value");
                            }
                        }

                        break;

                    case WellTypeId.ESP:
                        for (int i = 0; i < wellsStatus.Length; i++)
                        {
                            var wellStatusValue = wellsStatus[i] as ESPWellStatusValueDTO;
                            string addudcvalue = (string)wellStatusValue.OptionalField01;
                            Trace.WriteLine($"Additional UDC value from ESPWellStatus : {addudcvalue}");
                            if (AuthenticatedUser.Name.Length > 16)
                            {
                                Assert.IsTrue(AuthenticatedUser.Name.Contains(addudcvalue), "Mismatch in Additional String UDC Value");
                            }
                            else
                            {
                                Assert.AreEqual(AuthenticatedUser.Name, addudcvalue, "Mismatch in Additional String UDC Value");
                            }
                        }
                        break;

                    case WellTypeId.GLift:
                        for (int i = 0; i < wellsStatus.Length; i++)
                        {
                            var wellStatusValue = wellsStatus[i] as GLWellStatusValueDTO;
                            Trace.WriteLine("Addtional UDC value for GL is: " + wellStatusValue.OptionalField01.ToString());
                            string addudcvalue = (string)wellStatusValue.OptionalField01;
                            if (AuthenticatedUser.Name.Length > 16)
                            {
                                Assert.IsTrue(AuthenticatedUser.Name.Contains(addudcvalue), "Mismatch in Additional String UDC Value");
                            }
                            else
                            {
                                Assert.AreEqual(AuthenticatedUser.Name, addudcvalue, "Mismatch in Additional String UDC Value");
                            }
                        }
                        break;

                    case WellTypeId.NF:
                        for (int i = 0; i < wellsStatus.Length; i++)
                        {
                            var wellStatusValue = wellsStatus[i] as NFWellStatusValueDTO;
                            string addudcvalue = (string)wellStatusValue.OptionalField01;
                            Trace.WriteLine($"Additional UDC value from NFWellStatus : {addudcvalue}");
                            if (AuthenticatedUser.Name.Length > 16)
                            {
                                Assert.IsTrue(AuthenticatedUser.Name.Contains(addudcvalue), "Mismatch in Additional String UDC Value");
                            }
                            else
                            {
                                Assert.AreEqual(AuthenticatedUser.Name, addudcvalue, "Mismatch in Additional String UDC Value");
                            }
                        }
                        break;

                    case WellTypeId.GInj:
                        for (int i = 0; i < wellsStatus.Length; i++)
                        {
                            var wellStatusValue = wellsStatus[i] as GIWellStatusValueDTO;
                            string addudcvalue = (string)wellStatusValue.OptionalField01;
                            Trace.WriteLine($"Additional UDC value from GIWellStatus : {addudcvalue}");
                            if (AuthenticatedUser.Name.Length > 16)
                            {
                                Assert.IsTrue(AuthenticatedUser.Name.Contains(addudcvalue), "Mismatch in Additional String UDC Value");
                            }
                            else
                            {
                                Assert.AreEqual(AuthenticatedUser.Name, addudcvalue, "Mismatch in Additional String UDC Value");
                            }
                        }
                        break;

                    case WellTypeId.WInj:
                        for (int i = 0; i < wellsStatus.Length; i++)
                        {
                            var wellStatusValue = wellsStatus[i] as WIWellStatusValueDTO;
                            string addudcvalue = (string)wellStatusValue.OptionalField01;
                            Trace.WriteLine($"Additional UDC value from WIWellStatus : {addudcvalue}");
                            if (AuthenticatedUser.Name.Length > 16)
                            {
                                Assert.IsTrue(AuthenticatedUser.Name.Contains(addudcvalue), "Mismatch in Additional String UDC Value");
                            }
                            else
                            {
                                Assert.AreEqual(AuthenticatedUser.Name, addudcvalue, "Mismatch in Additional String UDC Value");
                            }
                        }
                        break;

                    case WellTypeId.PLift:
                        for (int i = 0; i < wellsStatus.Length; i++)
                        {
                            var wellStatusValue = wellsStatus[i] as PGLWellStatusValueDTO;
                            string addudcvalue = (string)wellStatusValue.OptionalField01;
                            Trace.WriteLine($"Additional UDC value from PGLWellStatus : {addudcvalue}");
                            if (AuthenticatedUser.Name.Length > 16)
                            {
                                Assert.IsTrue(AuthenticatedUser.Name.Contains(addudcvalue), "Mismatch in Additional String UDC Value");
                            }
                            else
                            {
                                Assert.AreEqual(AuthenticatedUser.Name, addudcvalue, "Mismatch in Additional String UDC Value");
                            }
                        }
                        break;
                }
            }
            finally
            {
                //Removing Well
                WellService.RemoveWellByWellId(well1.Id.ToString());
                //reverting System Setting changes
                settingValue.StringValue = originalSetting;
                SettingService.SaveSystemSetting(settingValue);
            }

        }

        public void VerificationOfAdditionalUDCDailyAverageForWellTrend(string facilityId, string name = null, string additionalUDC = null, string unitkey = null, UnitCategory unitcat = UnitCategory.None, WellTypeId wellTypeId = WellTypeId.RRL)
        {
            string facilityId1;
            facilityId1 = GetFacilityId(facilityId, 1);
            // System setting
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = null;
            WellDTO wellDTO = null;
            wellDTO = SetDefaultFluidType(new WellDTO()
            {
                Name = DefaultWellName,
                FacilityId = facilityId1,
                DataConnection = GetDefaultCygNetDataConnection(),
                IntervalAPI = "IntervalAPI",
                SubAssemblyAPI = "SubAssemblyAPI",
                AssemblyAPI = "AssemblyAPI",
                CommissionDate = DateTime.Today.AddYears(-2),
                WellType = wellTypeId
            });

            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = wellDTO });

            switch (wellTypeId)
            {
                case WellTypeId.RRL:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.RRL_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.ESP:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.ESP_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.GLift:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.GL_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.NF:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.NF_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.GInj:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.GI_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.WInj:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.WI_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.PLift:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.PL_GROUP_STATUS_EXTRA_UDCS);
                    break;
                case WellTypeId.PCP:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.PCP_GROUP_STATUS_EXTRA_UDCS);
                    break;
                case WellTypeId.WGInj:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.WAG_GROUP_STATUS_EXTRA_UDCS);
                    break;
                case WellTypeId.OT:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.OT_GROUP_STATUS_EXTRA_UDCS);
                    break;
            }

            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well1 = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            _wellsToRemove.Add(well1);

            WellFilterDTO wellbyFilters = new WellFilterDTO();
            WellGroupStatusQueryDTO Query = new WellGroupStatusQueryDTO();
            Query.WellType = well1.WellType;
            Query.WellFilter = wellbyFilters;

            SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
            string originalSetting = settingValue.StringValue;
            ConfigurableUDCInfo infos = new ConfigurableUDCInfo();
            infos.Parse(settingValue.StringValue);
            try
            {
                infos.Info.Add(infos.Info.Count() + 1, new ConfigurableUDC(name, additionalUDC, unitkey, unitcat, CygNetPointType.Analog, false));
                settingValue.StringValue = infos.GetSettingValue();
                SettingService.SaveSystemSetting(settingValue);


                switch (wellTypeId)
                {
                    case WellTypeId.RRL:
                        {
                            // **************************  Get Additional UDc daily average Data
                            Trace.WriteLine($"Testing Daily Average for Addtional UDC for {wellTypeId.ToString()}");
                            OptionalUDCDailyAvgCheckCommon(well1, name, Query);
                            break;
                        }

                    case WellTypeId.ESP:
                        {
                            // **************************  Get Additional UDc daily average Data
                            Trace.WriteLine($"Testing Daily Average for Addtional UDC for {wellTypeId.ToString()}");
                            OptionalUDCDailyAvgCheckCommon(well1, name, Query);
                            break;
                        }

                    case WellTypeId.GLift:
                        {
                            // **************************  Get Additional UDc daily average Data
                            Trace.WriteLine($"Testing Daily Average for Addtional UDC for {wellTypeId.ToString()}");
                            OptionalUDCDailyAvgCheckCommon(well1, name, Query);
                            break;
                        }

                    case WellTypeId.NF:
                        {
                            // **************************  Get Additional UDc daily average Data
                            Trace.WriteLine($"Testing Daily Average for Addtional UDC for {wellTypeId.ToString()}");
                            OptionalUDCDailyAvgCheckCommon(well1, name, Query);
                            break;
                        }

                    case WellTypeId.GInj:
                        {
                            // **************************  Get Additional UDc daily average Data
                            Trace.WriteLine($"Testing Daily Average for Addtional UDC for {wellTypeId.ToString()}");
                            OptionalUDCDailyAvgCheckCommon(well1, name, Query);
                            break;
                        }

                    case WellTypeId.WInj:
                        {
                            // **************************  Get Additional UDc daily average Data
                            Trace.WriteLine($"Testing Daily Average for Addtional UDC for {wellTypeId.ToString()}");
                            OptionalUDCDailyAvgCheckCommon(well1, name, Query);
                            break;
                        }

                    case WellTypeId.PLift:
                        {
                            // **************************  Get Additional UDc daily average Data
                            Trace.WriteLine($"Testing Daily Average for Addtional UDC for {wellTypeId.ToString()}");
                            OptionalUDCDailyAvgCheckCommon(well1, name, Query);
                            break;
                        }
                    case WellTypeId.PCP:
                        {
                            // **************************  Get Additional UDc daily average Data
                            Trace.WriteLine($"Testing Daily Average for Addtional UDC for {wellTypeId.ToString()}");
                            OptionalUDCDailyAvgCheckCommon(well1, name, Query);
                            break;
                        }
                    case WellTypeId.WGInj:
                        {
                            // **************************  Get Additional UDc daily average Data
                            Trace.WriteLine($"Testing Daily Average for Addtional UDC for {wellTypeId.ToString()}");
                            OptionalUDCDailyAvgCheckCommon(well1, name, Query);
                            break;
                        }
                    case WellTypeId.OT:
                        {
                            // **************************  Get Additional UDc daily average Data
                            Trace.WriteLine($"Testing Daily Average for Addtional UDC for {wellTypeId.ToString()}");
                            OptionalUDCDailyAvgCheckCommon(well1, name, Query);
                            break;
                        }
                }
            }
            finally
            {
                //Removing Well
                WellService.RemoveWellByWellId(well1.Id.ToString());
                //reverting System Setting changes
                settingValue.StringValue = originalSetting;
                SettingService.SaveSystemSetting(settingValue);
            }

        }
        public void VerificationOfAdditionalUDCDailyAverageForWellStatus(string facilityId, string name = null, string additionalUDC = null, string unitkey = null, UnitCategory unitcat = UnitCategory.None, WellTypeId wellTypeId = WellTypeId.RRL)
        {
            string facilityId1;
            facilityId1 = GetFacilityId(facilityId, 1);
            // System setting
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = null;
            WellDTO wellDTO = null;
            wellDTO = SetDefaultFluidType(new WellDTO()
            {
                Name = DefaultWellName,
                FacilityId = facilityId1,
                DataConnection = GetDefaultCygNetDataConnection(),
                IntervalAPI = "IntervalAPI",
                SubAssemblyAPI = "SubAssemblyAPI",
                AssemblyAPI = "AssemblyAPI",
                CommissionDate = DateTime.Today.AddYears(-2),
                WellType = wellTypeId
            });

            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = wellDTO });

            switch (wellTypeId)
            {
                case WellTypeId.RRL:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.RRL_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.ESP:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.ESP_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.GLift:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.GL_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.NF:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.NF_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.GInj:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.GI_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.WInj:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.WI_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.PLift:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.PL_GROUP_STATUS_EXTRA_UDCS);
                    break;
                case WellTypeId.PCP:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.PCP_GROUP_STATUS_EXTRA_UDCS);
                    break;
                case WellTypeId.OT:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.OT_GROUP_STATUS_EXTRA_UDCS);
                    break;
                case WellTypeId.WGInj:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.WAG_GROUP_STATUS_EXTRA_UDCS);
                    break;
            }

            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well1 = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            _wellsToRemove.Add(well1);

            WellFilterDTO wellbyFilters = new WellFilterDTO();
            WellGroupStatusQueryDTO Query = new WellGroupStatusQueryDTO();
            Query.WellType = well1.WellType;
            Query.WellFilter = wellbyFilters;

            SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
            string originalSetting = settingValue.StringValue;
            ConfigurableUDCInfo infos = new ConfigurableUDCInfo();
            infos.Parse(settingValue.StringValue);
            try
            {
                infos.Info.Add(infos.Info.Count() + 1, new ConfigurableUDC(name, additionalUDC, unitkey, unitcat, CygNetPointType.Analog, false));
                settingValue.StringValue = infos.GetSettingValue();
                SettingService.SaveSystemSetting(settingValue);
                OptionalUDCDailyAvgCheckCommonWellStatus(well1, name, additionalUDC, unitkey, unitcat);


            }
            finally
            {
                //Removing Well
                WellService.RemoveWellByWellId(well1.Id.ToString());
                //reverting System Setting changes
                settingValue.StringValue = originalSetting;
                SettingService.SaveSystemSetting(settingValue);
            }

        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GasLift_CondensateAlarmMessage()
        {
            string facilityId;
            facilityId = GetFacilityId("GLWELL_", 2);

            //Creating Well DTO
            WellDTO well = new WellDTO()
            {
                Name = "Well",
                FacilityId = facilityId,
                FluidType = WellFluidType.Condensate,
                DataConnection = GetDefaultCygNetDataConnection(),
                CommissionDate = DateTime.Today.AddYears(-2),
                AssemblyAPI = "GasLift_AssemblyAPI",
                SubAssemblyAPI = "GasLift_SubAssemblyAPI",
                IntervalAPI = "GasLift_IntervalAPI",
                WellType = WellTypeId.GLift,
                GasAllocationGroup = null,
                OilAllocationGroup = null,
                WaterAllocationGroup = null
            };

            //Creating well with null ModelConfig
            WellConfigDTO wellConfig = new WellConfigDTO();
            wellConfig.Well = well;
            wellConfig.ModelConfig = null;
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfig);
            _wellsToRemove.Add(addedWellConfig.Well);

            //Model file is placed at following path
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string modelFileName = "GLDHPGCondensate.wflx";

            ModelFileOptionDTO options = new ModelFileOptionDTO();
            //Selecting Calibration Method
            options.CalibrationMethod = CalibrationMethodId.ReservoirPressureAndLFactor;
            //Adding comment
            options.Comment = "Gas Lift_Codensated Fluid Type_DHPG";
            //Selecting OptionalUpdates
            options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };

            //Uploading Model file and providing Applicable date ,WellId & Options
            byte[] fileAsByteArray;
            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };
            modelFile.ApplicableDate = well.CommissionDate.Value.AddDays(1).ToUniversalTime();
            modelFile.WellId = addedWellConfig.Well.Id;
            fileAsByteArray = GetByteArray(path, modelFileName);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            modelFile.Options = options;
            ModelFileValidationDataDTO ModelFileValidationData;
            ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
            if (ModelFileValidationData != null)
                ModelFileService.AddWellModelFile(modelFile);
            else
                Trace.WriteLine(string.Format("Failed to validate Gas Lift Condensated fluid type with DHPG model file"));

            //Assigning Min L factor value in Acceptance Limit
            SettingDTO setting = SettingService.GetSettingByName("Min L Factor Acceptance Limit");
            Assert.IsNotNull(setting, "Failed to get settings for Min L Factor Acceptance Limit");
            WellSettingDTO wellSetting = new WellSettingDTO();
            wellSetting.Id = 0;
            wellSetting.NumericValue = setting.MinValue;
            wellSetting.SettingId = setting.Id;
            wellSetting.WellId = addedWellConfig.Well.Id;
            SettingService.SaveWellSetting(wellSetting);
            //Assigning Max L factor value in Acceptance Limit
            setting = SettingService.GetSettingByName("Max L Factor Acceptance Limit");
            Assert.IsNotNull(setting, "Failed to get settings for Min L Factor Acceptance Limit");
            wellSetting.Id = 0;
            wellSetting.NumericValue = setting.MaxValue;
            wellSetting.SettingId = setting.Id;
            wellSetting.WellId = addedWellConfig.Well.Id;
            SettingService.SaveWellSetting(wellSetting);

            //Providing WellTest data except Oil in in WellTestDTO
            AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(addedWellConfig.Well.Id.ToString());
            WellTestDTO testDataDTO = new WellTestDTO()
            {
                WellId = addedWellConfig.Well.Id,
                SPTCodeDescription = "AllocatableTest",
                AverageTubingPressure = 500,
                AverageCasingPressure = 2000,
                GasInjectionRate = 2000,
                Oil = (decimal)1.10,
                Gas = 1150,
                Water = (decimal)287.2,
                TotalGasRate = 3150,
            };



            //Creating WellTest using WellTestDTO created above
            testDataDTO.SampleDate = well.CommissionDate.Value.AddDays(2).ToUniversalTime();
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(addedWellConfig.Well.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

            //Changing  the UIS value of UDC  for Oil/Condesate Rate from CygNet using API calls
            FacilityTag facilityTag = new FacilityTag(s_site, s_cvsService, facilityId);
            SetPointValue(facilityTag, "ROIL", (testDataDTO.Oil * 0.5m).ToString(), DateTime.Now);


            //Sending Scan command
            SurveillanceService.IssueCommandForSingleWell(addedWellConfig.Well.Id, WellCommand.DemandScan.ToString());

            //Retrieving well status
            UnitsValuesPairDTO<object, object> wellStatus = SurveillanceServiceClient.GetWellStatusData(addedWellConfig.Well.Id.ToString());

            var value = wellStatus.Value as GLWellStatusValueDTO;

            //Verifying Whether Low Condensate rate Alarm is generated or not
            Assert.IsTrue(value.AlarmMessage.Contains("Low Condensate Rate"), "Low Condensate Rate Alarm is not generated");

            //Verifying that Low Oil rate Alarm is not generated
            Assert.IsFalse(value.AlarmMessage.Contains("Low Oil Rate"), "Incorrect Alarm Triggered. Instead, Low Condensate Alarm should be generated");
            Trace.WriteLine("Verification completed for Low Condensate Rate");
            //Changing  the UIS value of UDC  for Oil/Condesate Rate from CygNet using API calls
            facilityTag = new FacilityTag(s_site, s_cvsService, facilityId);
            SetPointValue(facilityTag, "ROIL", (testDataDTO.Oil * 1.5m).ToString(), DateTime.Now);

            //Sending Scan command
            SurveillanceService.IssueCommandForSingleWell(addedWellConfig.Well.Id, WellCommand.DemandScan.ToString());

            //Retrieving well status
            wellStatus = SurveillanceServiceClient.GetWellStatusData(addedWellConfig.Well.Id.ToString());

            value = wellStatus.Value as GLWellStatusValueDTO;

            //Verifying Whether High Condensate rate Alarm is generated or not
            Assert.IsTrue(value.AlarmMessage.Contains("High Condensate Rate"), "High Condensate Rate Alarm is not generated");

            //Verifying that High Oil rate Alarm is not generated
            Assert.IsFalse(value.AlarmMessage.Contains("High Oil Rate"), "Incorrect Alarm Triggered. Instead, High Condensate Alarm should be generated");
            Trace.WriteLine("Verification completed for High Condensate Rate");
        }


        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void SaveCommentsForMultipleWells()
        {
            WellConfigurationService.AddWellConfig(new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = "test 1",
                    FacilityId = "test 1",
                    DataConnection = GetDefaultCygNetDataConnection(),
                    CommissionDate = DateTime.Today.AddDays(-1),
                    AssemblyAPI = "test 1",
                    SubAssemblyAPI = "test 1",
                    IntervalAPI = "test 1",
                    WellType = WellTypeId.ESP,
                    Engineer = "test 1"
                })
            });

            WellConfigurationService.AddWellConfig(new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = "test 2",
                    FacilityId = "test 2",
                    DataConnection = GetDefaultCygNetDataConnection(),
                    CommissionDate = DateTime.Today.AddDays(-1),
                    AssemblyAPI = "test 2",
                    SubAssemblyAPI = "test 2",
                    IntervalAPI = "test 2",
                    WellType = WellTypeId.RRL,
                    Engineer = "test 2"
                })
            });

            List<WellDTO> allWells = WellService.GetAllWells().ToList();

            WellDTO well1 = allWells?.FirstOrDefault(w => w.Name.Equals("test 1"));
            WellDTO well2 = allWells?.FirstOrDefault(w => w.Name.Equals("test 2"));

            _wellsToRemove.Add(well1);
            _wellsToRemove.Add(well2);

            WellCommentDTO[] comments = new WellCommentDTO[3];
            comments[0] = new WellCommentDTO();
            comments[0].WellId = well1.Id;
            comments[0].WellComment = "new test 1";
            comments[1] = new WellCommentDTO();
            comments[1].WellId = well2.Id;
            comments[1].WellComment = "new test 2";
            comments[2] = new WellCommentDTO();
            comments[2].WellId = well1.Id;
            comments[2].WellComment = "new test 1";

            SurveillanceService.SaveCommentsForMultipleWells(comments);

            int n1 = WellService.GetWellComments(well1.Id.ToString()).Count();
            Assert.AreEqual(n1, 1);

            WellCommentDTO c2 = WellService.GetWellComments(well2.Id.ToString()).FirstOrDefault();
            Assert.AreEqual(c2.WellComment, "new test 2");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void SaveCommentsForMultipleWellsGroupStatus()
        {
            WellConfigurationService.AddWellConfig(new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = "test 4",
                    FacilityId = "test 4",
                    DataConnection = GetDefaultCygNetDataConnection(),
                    CommissionDate = DateTime.Today.AddDays(-1),
                    AssemblyAPI = "test 4",
                    SubAssemblyAPI = "test 4",
                    IntervalAPI = "test 4",
                    WellType = WellTypeId.RRL,
                    Engineer = "test 4"
                })
            });

            WellConfigurationService.AddWellConfig(new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = "test 5",
                    FacilityId = "test 5",
                    DataConnection = GetDefaultCygNetDataConnection(),
                    CommissionDate = DateTime.Today.AddDays(-1),
                    AssemblyAPI = "test 5",
                    SubAssemblyAPI = "test 5",
                    IntervalAPI = "test 5",
                    WellType = WellTypeId.ESP,
                    Engineer = "test 5"
                })
            });

            List<WellDTO> allWells = WellService.GetAllWells().ToList();

            WellDTO well4 = allWells?.FirstOrDefault(w => w.Name.Equals("test 4"));
            WellDTO well5 = allWells?.FirstOrDefault(w => w.Name.Equals("test 5"));

            _wellsToRemove.Add(well4);
            _wellsToRemove.Add(well5);

            WellCommentDTO[] comments = new WellCommentDTO[3];
            comments[0] = new WellCommentDTO();
            comments[0].WellId = well4.Id;
            comments[0].WellComment = "new test 4";
            comments[1] = new WellCommentDTO();
            comments[1].WellId = well5.Id;
            comments[1].WellComment = "new test 5";
            comments[2] = new WellCommentDTO();
            comments[2].WellId = well4.Id;
            comments[2].WellComment = "new test 4";

            SurveillanceService.SaveCommentsForMultipleWells(comments);

            int n1 = WellService.GetWellComments(well4.Id.ToString()).Count();
            Assert.AreEqual(n1, 1);

            WellCommentDTO c2 = WellService.GetWellComments(well5.Id.ToString()).FirstOrDefault();
            Assert.AreEqual(c2.WellComment, "new test 5");
        }

        public void CheckWellGaugeDTO(int value1, WellGaugeDTO dto, double? minValue, double? maxValue)
        {
            Assert.AreEqual((int)(value1 * 1.5m), dto.MaxVal, 2, "Incorrect Max Value");
            Assert.AreEqual((int)(value1 * 0.5m), dto.MinVal, 2, "Incorrect Min Value");
            if (minValue == null && maxValue == null)
            {
                Assert.AreEqual((double)(value1 * 0.8m), dto.RangeValue1.Value, 2, "Incorect RangeValue1 as per Default Operating limit");
                Assert.AreEqual((double)(value1 * 1.2m), dto.RangeValue2.Value, 2, "Incorect RangeValue2 as per Default Operating limit");
            }
            else
            {
                Assert.AreEqual(minValue, dto.RangeValue1, "Incorect RangeValue1 as per User defined Operating limit");
                Assert.AreEqual(maxValue, dto.RangeValue2, "Incorect RangeValue2 as per User defined Operating limit");
            }
        }

        //public void AddWellSetting(long wellId, string settingName, int value = 0)
        //{
        //    SettingDTO setting = SettingService.GetSettingByName(settingName);
        //    Assert.IsNotNull(setting, "Failed to get settings for " + settingName);
        //    if (setting.MinValue == null || setting.MaxValue == null)
        //        return;
        //    WellSettingDTO wellSetting = new WellSettingDTO();
        //    wellSetting.Id = 0;
        //    wellSetting.NumericValue = value == 0 ? random.Next((int)setting.MinValue, (int)setting.MaxValue) : value;
        //    wellSetting.SettingId = setting.Id;
        //    wellSetting.WellId = wellId;
        //    SettingService.SaveWellSetting(wellSetting);
        //}

        public void ValidateWellSettingsforKPIGauges(string wellId, string gaugeName, string operatingLimit, int value)
        {
            WellSettingDTO[] wellSettings = SettingService.GetWellSettingsByWellId(wellId);
            WellGaugeDTO[] KPIGauge = SurveillanceService.GetNonRRLWellGauges(wellId);
            Assert.AreEqual(3, KPIGauge.Count(), "Incorrect number of Gauges retrieved");
            WellGaugeDTO Item = KPIGauge.FirstOrDefault(x => x.Name == gaugeName);
            Assert.IsNotNull(Item, "Unable to get KPI Gauge " + gaugeName);
            double? minValue = wellSettings.FirstOrDefault(x => x.Setting.Name == "Min " + operatingLimit).NumericValue;
            double? maxValue = wellSettings.FirstOrDefault(x => x.Setting.Name == "Max " + operatingLimit).NumericValue;
            CheckWellGaugeDTO(value, Item, minValue, maxValue);
            WellGaugeDTO runTimeYesterday = KPIGauge.FirstOrDefault(x => x.Name == "RunTimeYesterday");
            Assert.IsNotNull(runTimeYesterday, "Unable to get Run time yesterday");
            Assert.AreEqual(24, runTimeYesterday.MaxVal);
            Assert.AreEqual(0, runTimeYesterday.MinVal);
            Assert.AreEqual(12, runTimeYesterday.RangeValue1);
            Assert.AreEqual(18, runTimeYesterday.RangeValue2);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void CheckKPIGauges_ESP()
        {
            string facilityId = null;
            try
            {
                facilityId = GetFacilityId("ESPWELL_", 1);
                ChangeUnitSystem("US");
                WellDTO wellESP = AddNonRRLWell(facilityId, WellTypeId.ESP, false);
                AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(wellESP.Id.ToString());
                WellTestAndUnitsDTO latestWellTest = WellTestDataService.GetLatestValidWellTestByWellId(wellESP.Id.ToString(), "true");
                WellGaugeDTO[] espKPIGauge = SurveillanceService.GetNonRRLWellGauges(wellESP.Id.ToString());
                Assert.AreEqual(3, espKPIGauge.Count(), "Incorrect number of Gauges retrieved");
                //Check on Default operating limits
                WellGaugeDTO pumpIntakePressure = espKPIGauge.FirstOrDefault(x => x.Name == "PumpIntakePressure");
                Assert.IsNotNull(pumpIntakePressure, "Unable to get PIP for ESP KPI Gauge");
                CheckWellGaugeDTO((int)latestWellTest.Value.PumpIntakePressure.Value, pumpIntakePressure, null, null);
                WellGaugeDTO motorFrequency = espKPIGauge.FirstOrDefault(x => x.Name == "MotorFrequency");
                Assert.IsNotNull(motorFrequency, "Unable to get Motor Frequency for KPI Gauge");
                CheckWellGaugeDTO((int)latestWellTest.Value.Frequency.Value, motorFrequency, null, null);
                ChangeUnitSystem("Metric");
                espKPIGauge = SurveillanceService.GetNonRRLWellGauges(wellESP.Id.ToString());
                Assert.AreEqual(3, espKPIGauge.Count(), "Incorrect number of Gauges retrieved");
                latestWellTest = WellTestDataService.GetLatestValidWellTestByWellId(wellESP.Id.ToString());
                pumpIntakePressure = espKPIGauge.FirstOrDefault(x => x.Name == "PumpIntakePressure");
                Assert.IsNotNull(pumpIntakePressure, "Unable to get PIP for ESP KPI Gauge");
                CheckWellGaugeDTO((int)latestWellTest.Value.PumpIntakePressure.Value, pumpIntakePressure, null, null);
                motorFrequency = espKPIGauge.FirstOrDefault(x => x.Name == "MotorFrequency");
                Assert.IsNotNull(motorFrequency, "Unable to get Motor Frequency for KPI Gauge");
                CheckWellGaugeDTO((int)latestWellTest.Value.Frequency.Value, motorFrequency, null, null);
                //Check on User operating limits
                ChangeUnitSystem("US");
                latestWellTest = WellTestDataService.GetLatestValidWellTestByWellId(wellESP.Id.ToString());
                WellSettingDTO[] wellOpeartingLimts = SettingService.GetWellSettingsByWellIdAndCategory(wellESP.Id.ToString(), ((int)SettingCategory.OperatingLimit).ToString());
                Assert.AreEqual(0, wellOpeartingLimts.Count(), "Well settings are available without adding any...");
                AddWellSetting(wellESP.Id, "Min Pump Intake Pressure Operating Limit");
                AddWellSetting(wellESP.Id, "Max Pump Intake Pressure Operating Limit");
                AddWellSetting(wellESP.Id, "Min Motor Frequency Operating Limit");
                AddWellSetting(wellESP.Id, "Max Motor Frequency Operating Limit");
                ValidateWellSettingsforKPIGauges(wellESP.Id.ToString(), "PumpIntakePressure", "Pump Intake Pressure Operating Limit", (int)latestWellTest.Value.PumpIntakePressure.Value);
                ValidateWellSettingsforKPIGauges(wellESP.Id.ToString(), "MotorFrequency", "Motor Frequency Operating Limit", (int)latestWellTest.Value.Frequency.Value);
                ChangeUnitSystem("Metric");
                latestWellTest = WellTestDataService.GetLatestValidWellTestByWellId(wellESP.Id.ToString());
                ValidateWellSettingsforKPIGauges(wellESP.Id.ToString(), "PumpIntakePressure", "Pump Intake Pressure Operating Limit", (int)latestWellTest.Value.PumpIntakePressure.Value);
                ValidateWellSettingsforKPIGauges(wellESP.Id.ToString(), "MotorFrequency", "Motor Frequency Operating Limit", (int)latestWellTest.Value.Frequency.Value);
            }
            finally
            {
                ChangeUnitSystem("US");
                RemoveWell(facilityId);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void CheckKPIGauges_GL()
        {
            string facilityId = null;
            try
            {
                facilityId = GetFacilityId("GLWELL_", 1);
                ChangeUnitSystem("US");
                WellDTO wellGL = AddNonRRLWell(facilityId, WellTypeId.GLift, false);
                AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(wellGL.Id.ToString());
                WellTestAndUnitsDTO latestWellTest = WellTestDataService.GetLatestValidWellTestByWellId(wellGL.Id.ToString(), "true");
                WellGaugeDTO[] glKPIGauge = SurveillanceService.GetNonRRLWellGauges(wellGL.Id.ToString());
                Assert.AreEqual(3, glKPIGauge.Count(), "Incorrect number of Gauges retrieved");
                //Check on Default operating limits
                WellGaugeDTO casingPressure = glKPIGauge.FirstOrDefault(x => x.Name == "CasingPressure");
                Assert.IsNotNull(casingPressure, "Unable to get Casing Pressure for GL KPI Gauge");
                CheckWellGaugeDTO((int)latestWellTest.Value.AverageCasingPressure, casingPressure, null, null);
                WellGaugeDTO gasInjectionRate = glKPIGauge.FirstOrDefault(x => x.Name == "GasInjectionRate");
                Assert.IsNotNull(gasInjectionRate, "Unable to get Gas Injection rate for KPI Gauge");
                CheckWellGaugeDTO((int)latestWellTest.Value.GasInjectionRate.Value, gasInjectionRate, null, null);
                ChangeUnitSystem("Metric");
                latestWellTest = WellTestDataService.GetLatestValidWellTestByWellId(wellGL.Id.ToString());
                glKPIGauge = SurveillanceService.GetNonRRLWellGauges(wellGL.Id.ToString());
                Assert.AreEqual(3, glKPIGauge.Count(), "Incorrect number of Gauges retrieved");
                casingPressure = glKPIGauge.FirstOrDefault(x => x.Name == "CasingPressure");
                Assert.IsNotNull(casingPressure, "Unable to get Casing Pressure for GL KPI Gauge");
                CheckWellGaugeDTO((int)latestWellTest.Value.AverageCasingPressure, casingPressure, null, null);
                gasInjectionRate = glKPIGauge.FirstOrDefault(x => x.Name == "GasInjectionRate");
                Assert.IsNotNull(gasInjectionRate, "Unable to get Gas Injection rate for KPI Gauge");
                CheckWellGaugeDTO((int)latestWellTest.Value.GasInjectionRate.Value, gasInjectionRate, null, null);
                //Check on User operating limits
                ChangeUnitSystem("US");
                latestWellTest = WellTestDataService.GetLatestValidWellTestByWellId(wellGL.Id.ToString());
                WellSettingDTO[] wellOpeartingLimts = SettingService.GetWellSettingsByWellIdAndCategory(wellGL.Id.ToString(), ((int)SettingCategory.OperatingLimit).ToString());
                Assert.AreEqual(0, wellOpeartingLimts.Count(), "Well settings are available without adding any...");
                AddWellSetting(wellGL.Id, "Min Casing Head Pressure Operating Limit");
                AddWellSetting(wellGL.Id, "Max Casing Head Pressure Operating Limit");
                AddWellSetting(wellGL.Id, "Min Gas Injection Flow Rate Operating Limit");
                AddWellSetting(wellGL.Id, "Max Gas Injection Flow Rate Operating Limit");
                ValidateWellSettingsforKPIGauges(wellGL.Id.ToString(), "CasingPressure", "Casing Head Pressure Operating Limit", (int)latestWellTest.Value.AverageCasingPressure);
                ValidateWellSettingsforKPIGauges(wellGL.Id.ToString(), "GasInjectionRate", "Gas Injection Flow Rate Operating Limit", (int)latestWellTest.Value.GasInjectionRate.Value);
                ChangeUnitSystem("Metric");
                latestWellTest = WellTestDataService.GetLatestValidWellTestByWellId(wellGL.Id.ToString());
                ValidateWellSettingsforKPIGauges(wellGL.Id.ToString(), "CasingPressure", "Casing Head Pressure Operating Limit", (int)latestWellTest.Value.AverageCasingPressure);
                ValidateWellSettingsforKPIGauges(wellGL.Id.ToString(), "GasInjectionRate", "Gas Injection Flow Rate Operating Limit", (int)latestWellTest.Value.GasInjectionRate.Value);
            }
            finally
            {
                ChangeUnitSystem("US");
                RemoveWell(facilityId);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void CheckKPIGauges_NF()
        {
            try
            {
                ChangeUnitSystem("US");
                WellDTO wellNF = AddNonRRLWell("NFWWELL_0001", WellTypeId.NF, false);
                AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(wellNF.Id.ToString());
                WellTestAndUnitsDTO latestWellTest = WellTestDataService.GetLatestValidWellTestByWellId(wellNF.Id.ToString(), "true");
                WellGaugeDTO[] nfKPIGauge = SurveillanceService.GetNonRRLWellGauges(wellNF.Id.ToString());
                Assert.AreEqual(3, nfKPIGauge.Count(), "Incorrect number of Gauges retrieved");
                //Check on Default operating limits
                WellGaugeDTO tubingHeadTemperature = nfKPIGauge.FirstOrDefault(x => x.Name == "TubingHeadTemperature");
                Assert.IsNotNull(tubingHeadTemperature, "Unable to get TubingHeadTemperature for KPI Gauge");
                CheckWellGaugeDTO((int)latestWellTest.Value.AverageTubingTemperature.Value, tubingHeadTemperature, null, null);
                WellGaugeDTO tubingPressure = nfKPIGauge.FirstOrDefault(x => x.Name == "TubingPressure");
                Assert.IsNotNull(tubingPressure, "Unable to TubingPressure for KPI Gauge");
                CheckWellGaugeDTO((int)latestWellTest.Value.AverageTubingPressure, tubingPressure, null, null);
                ChangeUnitSystem("Metric");
                latestWellTest = WellTestDataService.GetLatestValidWellTestByWellId(wellNF.Id.ToString());
                nfKPIGauge = SurveillanceService.GetNonRRLWellGauges(wellNF.Id.ToString());
                Assert.AreEqual(3, nfKPIGauge.Count(), "Incorrect number of Gauges retrieved");
                tubingHeadTemperature = nfKPIGauge.FirstOrDefault(x => x.Name == "TubingHeadTemperature");
                Assert.IsNotNull(tubingHeadTemperature, "Unable to get TubingHeadTemperature for KPI Gauge");
                CheckWellGaugeDTO((int)latestWellTest.Value.AverageTubingTemperature.Value, tubingHeadTemperature, null, null);
                tubingPressure = nfKPIGauge.FirstOrDefault(x => x.Name == "TubingPressure");
                Assert.IsNotNull(tubingPressure, "Unable to TubingPressure for KPI Gauge");
                CheckWellGaugeDTO((int)latestWellTest.Value.AverageTubingPressure, tubingPressure, null, null);
                //Check on User operating limits
                ChangeUnitSystem("US");
                latestWellTest = WellTestDataService.GetLatestValidWellTestByWellId(wellNF.Id.ToString());
                WellSettingDTO[] wellOpeartingLimts = SettingService.GetWellSettingsByWellIdAndCategory(wellNF.Id.ToString(), ((int)SettingCategory.OperatingLimit).ToString());
                Assert.AreEqual(0, wellOpeartingLimts.Count(), "Well settings are available without adding any...");
                AddWellSetting(wellNF.Id, "Min Tubing Head Temperature Operating Limit");
                AddWellSetting(wellNF.Id, "Max Tubing Head Temperature Operating Limit");
                AddWellSetting(wellNF.Id, "Min Tubing Head Pressure Operating Limit");
                AddWellSetting(wellNF.Id, "Max Tubing Head Pressure Operating Limit");
                ValidateWellSettingsforKPIGauges(wellNF.Id.ToString(), "TubingHeadTemperature", "Tubing Head Temperature Operating Limit", (int)latestWellTest.Value.AverageTubingTemperature.Value);
                ValidateWellSettingsforKPIGauges(wellNF.Id.ToString(), "TubingPressure", "Tubing Head Pressure Operating Limit", (int)latestWellTest.Value.AverageTubingPressure);
                ChangeUnitSystem("Metric");
                latestWellTest = WellTestDataService.GetLatestValidWellTestByWellId(wellNF.Id.ToString());
                ValidateWellSettingsforKPIGauges(wellNF.Id.ToString(), "TubingHeadTemperature", "Tubing Head Temperature Operating Limit", (int)latestWellTest.Value.AverageTubingTemperature.Value);
                ValidateWellSettingsforKPIGauges(wellNF.Id.ToString(), "TubingPressure", "Tubing Head Pressure Operating Limit", (int)latestWellTest.Value.AverageTubingPressure);
            }
            finally
            {
                ChangeUnitSystem("US");
                RemoveWell("NFWWELL_0001");
            }
        }

        public WellGroupStatusQueryDTO GroupStatusQuery(WellTypeId wType)
        {
            var newGroup = new WellGroupStatusQueryDTO();
            newGroup.WellType = wType;
            //Get well Filters
            WellFilterDTO filters = WellService.GetWellFilter(null);
            //Well Column Filter
            List<string> columnName = new List<string>();
            List<string> filterConditions = new List<string>();
            List<string> filterDataTypes = new List<string>();
            List<string> filterValues = new List<string>();
            WellGroupStatusColumnFilterDTO columnFilter = new WellGroupStatusColumnFilterDTO();
            columnFilter.ColumnNames = columnName.ToArray();
            columnFilter.FilterConditions = filterConditions.ToArray();
            columnFilter.FilterDataTypes = filterDataTypes.ToArray();
            columnFilter.FilterValues = filterValues.ToArray();
            //No Filters Selected
            WellFilterDTO wellbyFilters = new WellFilterDTO();
            newGroup.WellFilter = wellbyFilters;
            newGroup.wellGroupStatusColumnFilter = columnFilter;

            return newGroup;
        }

        public void CompareRRLWellStatus(RRLWellStatusValueDTO expected, RRLWellStatusValueDTO actual, bool check = true)
        {
            Assert.AreEqual(expected.AverageCycleTime, actual.AverageCycleTime);
            Assert.AreEqual(expected.AveragePumpFillage, actual.AveragePumpFillage);
            Assert.AreEqual(expected.AverageSPM, actual.AverageSPM);
            Assert.AreEqual(expected.CurrentPumpFillage, actual.CurrentPumpFillage);
            Assert.AreEqual(expected.CurrentSPM, actual.CurrentSPM);
            Assert.AreEqual(expected.RunTimeToday, actual.RunTimeToday);
            Assert.AreEqual(expected.RunTimeYesterday, actual.RunTimeYesterday);
            Assert.AreEqual(expected.EffectiveRunTimePercentage.Value, actual.EffectiveRunTimePercentage.Value, 1);
            Assert.AreEqual(expected.MotorStatus, actual.MotorStatus);
            Assert.AreEqual(expected.IdleTimeToday, actual.IdleTimeToday);
            Assert.AreEqual(expected.IdleTimeYesterday, actual.IdleTimeYesterday);
            Assert.AreEqual(expected.DeviceType, actual.DeviceType);
            if (check)
            {
                Assert.AreEqual(expected.InferredProductionToday.Value, actual.InferredProductionToday.Value, 1);
                Assert.AreEqual(expected.InferredProductionYesterday.Value, actual.InferredProductionYesterday.Value, 1);
                Assert.AreEqual(expected.CardArea.Value, actual.CardArea.Value, 1);
                Assert.AreEqual(expected.DailyMaximumLoad.Value, actual.DailyMaximumLoad.Value, 1);
                Assert.AreEqual(expected.DailyMinimumLoad.Value, actual.DailyMinimumLoad.Value, 1);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellStatusDataWithUOM_RRL()
        {
            string facilityId;
            if (s_isRunningInATS)
                facilityId = "RPOC_00001";
            else
                facilityId = "RPOC_0001";
            try
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                WellDTO wellRRL = AddRRLWell(facilityId);
                WellGroupStatusQueryDTO newGroup = GroupStatusQuery(WellTypeId.RRL);
                var wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId), "Failed to get the added well");
                RRLWellStatusValueDTO us_rrlWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(wellRRL.Id.ToString(),
                    SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as RRLWellStatusValueDTO;
                Assert.IsNotNull(us_rrlWellStatus, "Failed to get Well status in 'US' units");
                CompareRRLWellStatus(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId), us_rrlWellStatus);
                RRLWellStatusValueDTO metric_rrlWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(wellRRL.Id.ToString(),
                    SettingServiceStringConstants.UNIT_SYSTEM_NAME_METRIC).Value as RRLWellStatusValueDTO;
                Assert.IsNotNull(us_rrlWellStatus, "Failed to get Well status in 'Metric' units");
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");
                CompareRRLWellStatus(us_rrlWellStatus, metric_rrlWellStatus, false);
                wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId), "Failed to get the added well");
                CompareRRLWellStatus(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId), metric_rrlWellStatus);
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");

                RemoveWell(facilityId);
            }
        }

        public void CompareESPWellStatus(ESPWellStatusValueDTO expected, ESPWellStatusValueDTO actual, bool check = true)
        {
            if (expected.AlarmMessage.Count() < actual.AlarmMessage.Count())
            {
                Assert.AreEqual(expected.AlarmMessage.Count() + 1, actual.AlarmMessage.Count());
            }
            else
            {
                Assert.AreEqual(expected.AlarmMessage.Count(), actual.AlarmMessage.Count());
                for (int i = 0; i < actual.AlarmMessage.Count(); i++)
                {
                    Assert.IsTrue(expected.AlarmMessage[i].Equals(actual.AlarmMessage[i]));
                }
            }
            if (!string.IsNullOrEmpty(actual.AlarmMessageString) && !string.IsNullOrEmpty(expected.AlarmMessageString))
                Assert.IsTrue(actual.AlarmMessageString.Contains(expected.AlarmMessageString));
            Assert.AreEqual(expected.CommStatus, actual.CommStatus);
            Assert.AreEqual(expected.FlowStatus, actual.FlowStatus);
            if (expected.LastAlarmTimes != null && actual.LastAlarmTimes != null)
            {
                Assert.AreEqual(expected.LastAlarmTimes.Count(), actual.LastAlarmTimes.Count());
                for (int i = 0; i < expected.LastAlarmTimes.Count(); i++)
                {
                    Assert.IsTrue(expected.LastAlarmTimes[i].Alarm.Equals(actual.LastAlarmTimes[i].Alarm));
                    Assert.IsNotNull(expected.LastAlarmTimes[i].TimeStamp, "Last Alram Time is NULL for Expected DTO");
                    Assert.IsNotNull(actual.LastAlarmTimes[i].TimeStamp);
                    Assert.IsTrue(expected.LastAlarmTimes[i].TimeStamp.Value.Equals(actual.LastAlarmTimes[i].TimeStamp.Value));
                }
            }
            else
            {
                Assert.AreEqual(expected.LastAlarmTimes, actual.LastAlarmTimes);
            }
            Assert.IsNotNull(expected.LastGoodScanTime, "Last Good Scan Time is NULL Expected Time ");
            Assert.IsNotNull(actual.LastGoodScanTime, "Last Good Scan Time is NULL Actual Time");
            Assert.AreEqual(expected.LastGoodScanTime, actual.LastGoodScanTime);
            Assert.IsNotNull(expected.LastScanTime, "Last  Scan Time is NULL");
            Assert.IsNotNull(actual.LastScanTime, "Last Actual Scan Time ");
            Assert.AreEqual(expected.LastScanTime, actual.LastScanTime);
            Assert.AreEqual(expected.OutOfServiceCode, actual.OutOfServiceCode);
            Assert.AreEqual(expected.DeviceType, actual.DeviceType);
            Assert.AreEqual(expected.RunStatus, actual.RunStatus);
            if (check)
            {
                Assert.AreEqual(expected.CasingGasRate.Value, actual.CasingGasRate.Value, 1);
                Assert.AreEqual(expected.CasingPressure.Value, actual.CasingPressure.Value, 1);
                Assert.AreEqual(expected.ChokeDiameter.Value, actual.ChokeDiameter.Value, 1);
                Assert.AreEqual(expected.FlowLinePressure.Value, actual.FlowLinePressure.Value, 1);
                Assert.IsTrue((actual.GasRateMeasured - expected.GasRateMeasured) < 1);
                Assert.AreEqual(expected.MotorAmps.Value, actual.MotorAmps.Value, 1);
                Assert.AreEqual(expected.MotorFrequency.Value, actual.MotorFrequency.Value, 1);
                Assert.AreEqual(expected.MotorTemperature.Value, actual.MotorTemperature.Value, 1);
                Assert.AreEqual(expected.MotorVolts.Value, actual.MotorVolts.Value, 1);
                Assert.IsTrue((actual.OilRateMeasured - expected.OilRateMeasured) < 1);
                Assert.AreEqual(expected.PumpDischargePressure.Value, actual.PumpDischargePressure.Value, 1);
                Assert.AreEqual(expected.PumpIntakePressure.Value, actual.PumpIntakePressure.Value, 1);
                Assert.AreEqual(expected.TubingHeadTemperature.Value, actual.TubingHeadTemperature.Value, 1);
                Assert.AreEqual(expected.TubingPressure.Value, actual.TubingPressure.Value, 1);
                Assert.AreEqual(expected.Vibration.Value, actual.Vibration.Value, 1);
                Assert.IsTrue((actual.WaterRateMeasured - expected.WaterRateMeasured) < 1);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellStatusDataWithUOM_ESP()
        {
            string passFacilityId1 = "";
            try
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                passFacilityId1 = GetFacilityId("ESPWELL_", 1);
                WellDTO wellESP = AddNonRRLWell(passFacilityId1, WellTypeId.ESP, true);
                WellGroupStatusQueryDTO newGroup = GroupStatusQuery(WellTypeId.ESP);
                var wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<ESPWellStatusUnitDTO, ESPWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), "Failed to get the added well");
                ESPWellStatusValueDTO us_espWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(wellESP.Id.ToString(),
                    SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as ESPWellStatusValueDTO;
                Assert.IsNotNull(us_espWellStatus, "Failed to get Well status in 'US' units");
                CompareESPWellStatus(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), us_espWellStatus);
                ESPWellStatusValueDTO metric_espWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(wellESP.Id.ToString(),
                    SettingServiceStringConstants.UNIT_SYSTEM_NAME_METRIC).Value as ESPWellStatusValueDTO;
                Assert.IsNotNull(metric_espWellStatus, "Failed to get Well status in 'US' units");
                CompareESPWellStatus(us_espWellStatus, metric_espWellStatus, false);
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");
                wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<ESPWellStatusUnitDTO, ESPWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), "Failed to get the added well");
                CompareESPWellStatus(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), metric_espWellStatus);
                //CygNetAlarms
                CygNetAlarmDTO cygNetAlarmESP = SurveillanceService.GetCygNetAlarmsIncludeHiLowStatusByWellId(wellESP.Id.ToString());
                Assert.IsNotNull(cygNetAlarmESP, "Failed to get CygNet alarms for ESP");
                ESPWellStatusValueDTO[] arrESPWellStatus = new ESPWellStatusValueDTO[] { us_espWellStatus };
                arrESPWellStatus = SurveillanceService.GetESPCygNetAlarms(arrESPWellStatus);
                Assert.IsNotNull(arrESPWellStatus, "Failed to get all CygNet alarms for ESP");
                Assert.AreEqual(1, arrESPWellStatus.Count());
                Assert.AreEqual(cygNetAlarmESP.CygNetAlarms.Count, arrESPWellStatus.FirstOrDefault().CygNetAlarmMessage.Count());
                for (int i = 0; i < cygNetAlarmESP.CygNetAlarms.Count; i++)
                {
                    Assert.AreEqual(cygNetAlarmESP.CygNetAlarms[i], arrESPWellStatus.FirstOrDefault().CygNetAlarmMessage[i]);
                }
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                RemoveWell(passFacilityId1);
            }
        }

        public void CompareGLWellStatus(GLWellStatusValueDTO expected, GLWellStatusValueDTO actual, bool check = true)
        {
            if (expected.AlarmMessage.Count() < actual.AlarmMessage.Count())
            {
                Assert.AreEqual(expected.AlarmMessage.Count() + 1, actual.AlarmMessage.Count());
            }
            else
            {
                Assert.AreEqual(expected.AlarmMessage.Count(), actual.AlarmMessage.Count());
                for (int i = 0; i < actual.AlarmMessage.Count(); i++)
                {
                    Assert.IsTrue(expected.AlarmMessage[i].Equals(actual.AlarmMessage[i]));
                }
            }
            if (!string.IsNullOrEmpty(actual.AlarmMessageString) && !string.IsNullOrEmpty(expected.AlarmMessageString))
                Assert.IsTrue(actual.AlarmMessageString.Contains(expected.AlarmMessageString));
            Assert.AreEqual(expected.CommStatus, actual.CommStatus);
            Assert.AreEqual(expected.FlowStatus, actual.FlowStatus);
            if (expected.LastAlarmTimes != null && actual.LastAlarmTimes != null)
            {
                Assert.AreEqual(expected.LastAlarmTimes.Count(), actual.LastAlarmTimes.Count());
                for (int i = 0; i < expected.LastAlarmTimes.Count(); i++)
                {
                    Assert.IsTrue(expected.LastAlarmTimes[i].Alarm.Equals(actual.LastAlarmTimes[i].Alarm));
                    Assert.IsNotNull(expected.LastAlarmTimes[i].TimeStamp);
                    Assert.IsNotNull(actual.LastAlarmTimes[i].TimeStamp);
                    Assert.IsTrue(expected.LastAlarmTimes[i].TimeStamp.Value.Equals(actual.LastAlarmTimes[i].TimeStamp.Value));
                }
            }
            else
            {
                Assert.AreEqual(expected.LastAlarmTimes, actual.LastAlarmTimes);
            }
            Assert.IsNotNull(expected.LastGoodScanTime);
            Assert.IsNotNull(actual.LastGoodScanTime);
            Assert.AreEqual(expected.LastGoodScanTime, actual.LastGoodScanTime);
            Assert.IsNotNull(expected.LastScanTime);
            Assert.IsNotNull(actual.LastScanTime);
            Assert.AreEqual(expected.LastScanTime, actual.LastScanTime);
            Assert.AreEqual(expected.OutOfServiceCode, actual.OutOfServiceCode);
            Assert.AreEqual(expected.DeviceType, actual.DeviceType);
            Assert.AreEqual(expected.RunStatus, actual.RunStatus);
            if (check)
            {
                Assert.AreEqual(expected.CasingPressure.Value, actual.CasingPressure.Value, 1);
                Assert.AreEqual(expected.DHGaugePressure.Value, actual.DHGaugePressure.Value, 1);
                Assert.AreEqual(expected.GasInjectionRate.Value, actual.GasInjectionRate.Value, 1);
                Assert.AreEqual(expected.TubingHeadTemperature.Value, actual.TubingHeadTemperature.Value, 1);
                Assert.AreEqual(expected.TubingPressure.Value, actual.TubingPressure.Value, 1);
                Assert.IsTrue((actual.WaterRateMeasured - expected.WaterRateMeasured) < 1);
                Assert.IsTrue((actual.GasRateMeasured - expected.GasRateMeasured) < 1);
                Assert.IsTrue((actual.OilRateMeasured - expected.OilRateMeasured) < 1);
            }
        }

        public void ComparePGLWellStatus(PGLWellStatusValueDTO expected, PGLWellStatusValueDTO actual, bool check = true, string unitsystem = "US")
        {
            if (expected.AlarmMessage.Count() < actual.AlarmMessage.Count())
            {
                Assert.AreEqual(expected.AlarmMessage.Count() + 1, actual.AlarmMessage.Count(), "Mismatch in Alarm Message Count");
            }
            else
            {
                Assert.AreEqual(expected.AlarmMessage.Count(), actual.AlarmMessage.Count(), "Mismatch in Alarm Message Count condition 2");
                for (int i = 0; i < actual.AlarmMessage.Count(); i++)
                {
                    Assert.IsTrue(expected.AlarmMessage[i].Equals(actual.AlarmMessage[i]), "Alarm Message are not same");
                }
            }
            if (!string.IsNullOrEmpty(actual.AlarmMessageString) && !string.IsNullOrEmpty(expected.AlarmMessageString))
                Assert.IsTrue(actual.AlarmMessageString.Contains(expected.AlarmMessageString), "Actual Alarm is not containg Expected Alarm");
            Assert.AreEqual(expected.CommStatus, actual.CommStatus, "Mismatch in Comm Status");
            Assert.AreEqual(expected.FlowStatus, actual.FlowStatus, "Mismatch in Flow Status");
            if (expected.LastAlarmTimes != null && actual.LastAlarmTimes != null)
            {
                Assert.AreEqual(expected.LastAlarmTimes.Count(), actual.LastAlarmTimes.Count(), "Mismatch in Last Alarm count");
                for (int i = 0; i < expected.LastAlarmTimes.Count(); i++)
                {
                    Assert.IsTrue(expected.LastAlarmTimes[i].Alarm.Equals(actual.LastAlarmTimes[i].Alarm), "Mismatch in Last Alarm Time Mesasge ");
                    Assert.IsNotNull(expected.LastAlarmTimes[i].TimeStamp, "Expected Last Alarm Time is Null");
                    Assert.IsNotNull(actual.LastAlarmTimes[i].TimeStamp, "Actual  Last Alarm Time is Null");
                    Assert.IsTrue(expected.LastAlarmTimes[i].TimeStamp.Value.Equals(actual.LastAlarmTimes[i].TimeStamp.Value), "Mismatch in Last Alarm Time,tiemestamp Mesasge ");
                }
            }
            else
            {
                Assert.AreEqual(expected.LastAlarmTimes, actual.LastAlarmTimes, "Mismatch in Last Alarm count");
            }
            Assert.IsNotNull(expected.LastGoodScanTime, "Expected Last Good Scan Time is Null");
            Assert.IsNotNull(actual.LastGoodScanTime, "Actual  Last Good Scan Time is Null");
            Assert.AreEqual(expected.LastGoodScanTime, actual.LastGoodScanTime, "Msimatch in Last Good Scan Time");
            Assert.IsNotNull(expected.LastScanTime, "Expected Last Scan Time is Null");
            Assert.IsNotNull(actual.LastScanTime, "Actual Last Scan Time is Null");
            Assert.AreEqual(expected.LastScanTime, actual.LastScanTime, "Mismatch in Last Scan Time");
            Assert.AreEqual(expected.OutOfServiceCode, actual.OutOfServiceCode, "Mismatch in Service Code");
            Assert.AreEqual(expected.DeviceType, actual.DeviceType, "Mismatch in RTU Type");

            if (check)
            {
                Assert.AreEqual(expected.CasingPressure, GetTruncatedValueforDouble((double)actual.CasingPressure, CountDigitsAfterDecimal((double)expected.CasingPressure)), "Casing Pressure Mismatch");
                Assert.AreEqual(expected.TubingPressure, GetTruncatedValueforDouble((double)actual.TubingPressure, CountDigitsAfterDecimal((double)expected.TubingPressure)), "Tubing  Pressure Mismatch");
                Trace.WriteLine("Exp gas Rate" + (double)expected.GasRateMeasured);
                Trace.WriteLine("Act gas Rate" + (double)actual.GasRateMeasured);
                Assert.AreEqual((double)expected.GasRateMeasured, GetTruncatedValueforDouble((double)actual.GasRateMeasured, CountDigitsAfterDecimal((double)expected.GasRateMeasured)), "Gas Rate Measured Mismatch");
                Assert.AreEqual(expected.LoadFactor, GetTruncatedValueforDouble((double)actual.LoadFactor, CountDigitsAfterDecimal((double)expected.LoadFactor)), "Load Factor Measured Mismatch");
                Assert.AreEqual(expected.DifferentialPressure, GetTruncatedValueforDouble((double)actual.DifferentialPressure, CountDigitsAfterDecimal((double)expected.DifferentialPressure)), "Differential Pressure Measured Mismatch");
                Assert.AreEqual(expected.FlowLinePressure, GetTruncatedValueforDouble((double)actual.FlowLinePressure, CountDigitsAfterDecimal((double)expected.FlowLinePressure)), "FlowLine Pressure Measured Mismatch");
                Assert.AreEqual(expected.ProductionValveStatus, actual.ProductionValveStatus, "Production ValveStatus Measured Mismatch");
                Assert.AreEqual(expected.RemainingTime, actual.RemainingTime, "Remaining Time Measured Mismatch");
                Assert.AreEqual(expected.PlungerArrivalTime, actual.PlungerArrivalTime, "FlowLine Pressure Measured Mismatch");
            }
            //this code is commented because of FRI-3389 changes and we have to cover this senario into the UI automation script
            //// Verify if the Values for Guages are not Null
            //WellGaugeDTO[] pglWellGauge = SurveillanceService.GetNonRRLWellGauges(actual.WellId.ToString());
            //Assert.IsNotNull(pglWellGauge.FirstOrDefault(x => x.Name.Equals("LastCycleTime")).CurrentVal, "Last Cycle Time value is  null");
            //Assert.IsNotNull(pglWellGauge.FirstOrDefault(x => x.Name.Equals("LastCycleTime")).MinVal, "Min value for Last Cycle Time value is  null");
            //Assert.IsNotNull(pglWellGauge.FirstOrDefault(x => x.Name.Equals("LastCycleTime")).MaxVal, "Max value for Last Cycle Time value is  null");
            //Assert.AreEqual("min", pglWellGauge.FirstOrDefault(x => x.Name.Equals("LastCycleTime")).Unit.ToString(), "Unit Text mismatch for Cycle Last Cycle Time");
            //Assert.IsNotNull(pglWellGauge.FirstOrDefault(x => x.Name.Equals("CycleGasVolume")).CurrentVal, "Cycle Gas Volume value is  null");
            //Assert.IsNotNull(pglWellGauge.FirstOrDefault(x => x.Name.Equals("CycleGasVolume")).MinVal, "Min value forCycle Gas Volume value is  null");
            //Assert.IsNotNull(pglWellGauge.FirstOrDefault(x => x.Name.Equals("CycleGasVolume")).MaxVal, "Max value for Cycle Gas Volume value is  null");

            //Assert.IsNotNull(pglWellGauge.FirstOrDefault(x => x.Name.Equals("CycleSlugVolume")).CurrentVal, "Cycle Slug Volume  value is  null");
            //Assert.IsNotNull(pglWellGauge.FirstOrDefault(x => x.Name.Equals("CycleSlugVolume")).MinVal, "Min value for Cycle Slug Volume  value is  null");
            //Assert.IsNotNull(pglWellGauge.FirstOrDefault(x => x.Name.Equals("CycleSlugVolume")).MaxVal, "Max value for  Cycle Slug Volume  value is  null");

            //Verify Production KPI Values  for Actaul Gas Rate , adn Well Test gas Rate
            if (unitsystem.ToUpper() == "US")
            {
                ProductionKPIDTO productionkpidto = SurveillanceService.GetProductionKPI(actual.WellId.ToString(), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime()));
                Assert.IsNotNull(productionkpidto, "Prodution KPI is null");
                if (productionkpidto.CurrentAllocatedProduction.Gas != null)
                {
                    Trace.WriteLine("Current Allocation for Production is NON NULL " + DateTime.Now.ToString());
                    Assert.AreEqual(5.00, productionkpidto.CurrentAllocatedProduction.Gas, "Actaul Gas Rate For today Mistmatch");
                }
                else
                {
                    Trace.WriteLine("Current Allocation for Production is coming as NULL " + DateTime.Now.ToString());
                    Trace.WriteLine("Current Allocation for Last day gas " + productionkpidto.LastAllocatedProduction.Gas);
                }

                Assert.AreEqual(89.31, productionkpidto.WellTestProduction.Gas, "Well Test Gas Rate For Plunger Mismatch");
                //Assert.AreEqual("STB", pglWellGauge.FirstOrDefault(x => x.Name.Equals("CycleSlugVolume")).Unit.ToString(), "Unit Text Mismatch for Cycle Slug Volume US");
                //Assert.AreEqual("Mscf", pglWellGauge.FirstOrDefault(x => x.Name.Equals("CycleGasVolume")).Unit.ToString(), "Unit Text Mismatch for Cycle Gas Volume US");
                //Verify Daily Averge Value:
                WellDailyAverageAndTestDTO wellstatsudaulydto = SurveillanceService.GetWellDailyAverageAndTest(actual.WellId.ToString(), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime()));

                #region AssertForDailyAvgValues

                Assert.AreEqual(120, wellstatsudaulydto.DailyAverage.Value.AverageMaximumCasingPressure, "Maximum Casing Pressure  Mismatch");
                Assert.AreEqual(120, wellstatsudaulydto.DailyAverage.Value.AverageMaximumTubingPressure, "Maximum Tubing Pressure  Mismatch");
                Assert.AreEqual(100, wellstatsudaulydto.DailyAverage.Value.AverageMinimumCasingPressure, "Minimum Casing Pressure  Mismatch");
                Assert.AreEqual(100, wellstatsudaulydto.DailyAverage.Value.AverageMinimumTubingPressure, " Minimum Tubing Pressure  Mismatch");
                Assert.AreEqual(100, wellstatsudaulydto.DailyAverage.Value.AverageMinimumLoadFactor, "Minimum Load Factor  Mismatch");
                Assert.AreEqual(10, wellstatsudaulydto.DailyAverage.Value.FLP, "Avg Flow Line Presure   Mismatch");
                //slug Volume from where it comes ??
                double? slugdailyvolume = wellstatsudaulydto.DailyAverage.Value.AverageCycleWaterVolume + wellstatsudaulydto.DailyAverage.Value.AverageCycleOilVolume;
                Trace.WriteLine(" Slug = Oil Volume + Water Volume " + (wellstatsudaulydto.DailyAverage.Value.AverageCycleWaterVolume + wellstatsudaulydto.DailyAverage.Value.AverageCycleOilVolume));
                Assert.AreEqual(139, slugdailyvolume, "Daily Avg Slug Voume Mismatch [ US units]");
                Assert.AreEqual(10, wellstatsudaulydto.DailyAverage.Value.FLP, "Avg Flow Line Presure   Mismatch");
                Assert.AreEqual(80, wellstatsudaulydto.DailyAverage.Value.AverageCycleGasVolume, "Cycle Gas Volume  Mismatch");

                Assert.AreEqual(75, wellstatsudaulydto.DailyAverage.Value.AverageCycleTime, "AverageCycleTime  Mismatch");
                Assert.AreEqual(60, wellstatsudaulydto.DailyAverage.Value.AverageCycleBuildTime, "Cycle Build Time  Mismatch");
                Assert.AreEqual(120, wellstatsudaulydto.DailyAverage.Value.AverageCycleFallTime, "Cycle FallTime  Mismatch");
                Assert.AreEqual(60, wellstatsudaulydto.DailyAverage.Value.AverageCycleRiseTime, "CycleRiseTime  Mismatch");
                Assert.AreEqual(90, wellstatsudaulydto.DailyAverage.Value.AverageCycleAfterFlowTime, "Cycle After FlowTime  Mismatch");

                Assert.AreEqual(8, wellstatsudaulydto.DailyAverage.Value.OilRateAllocated, "Oil RateAllocated  Mismatch");
                Assert.AreEqual(10, wellstatsudaulydto.DailyAverage.Value.WaterRateAllocated, "Water Rate Allocated  Mismatch");
                Assert.AreEqual(5, wellstatsudaulydto.DailyAverage.Value.GasRateAllocated, "Gas Rate Allocated  Mismatch");

                Assert.AreEqual(80, wellstatsudaulydto.DailyAverage.Value.AveragePlungerRiseSpeed, "PlungerRise Speed  Mismatch");

                #endregion AssertForDailyAvgValues

                //Verify Well Test Value:

                #region AssertWellTestValues

                Assert.AreEqual(1502.00m, wellstatsudaulydto.WellTest.Value.MaximumCasingPressure, "Maximum Casing Pressure  Mismatch");
                Assert.AreEqual(797.00m, wellstatsudaulydto.WellTest.Value.MaximumTubingPressure, "Maximum Tubing Pressure  Mismatch");
                Assert.AreEqual(943.00m, wellstatsudaulydto.WellTest.Value.MinimumCasingPressure, "Minimum Casing Pressure  Mismatch");
                Assert.AreEqual(516.00m, wellstatsudaulydto.WellTest.Value.MinimumTubingPressure, " Minimum Tubing Pressure  Mismatch");
                Assert.AreEqual(0.6103, GetTruncatedValueforDouble((double)wellstatsudaulydto.WellTest.Value.MinimumLoadFactor, CountDigitsAfterDecimal(0.6103)), "Minimum Load Factor  Mismatch");
                Assert.AreEqual(347.00m, wellstatsudaulydto.WellTest.Value.FlowLinePressure, "Avg Flow Line Presure   Mismatch");
                Assert.AreEqual(630.00m, wellstatsudaulydto.WellTest.Value.CycleSlugVolume, "Cycle Slug Volume  Mismatch");

                Assert.AreEqual(1291.0000m, wellstatsudaulydto.WellTest.Value.CycleGasVolume, "Cycle Gas Volume  Mismatch");
                Assert.AreEqual(20810, GetTruncatedValueforDouble((double)wellstatsudaulydto.WellTest.Value.CycleTime, 2), "CycleTime  Mismatch");

                Assert.AreEqual(6408.00, GetTruncatedValueforDouble((double)wellstatsudaulydto.WellTest.Value.BuildTime, 2), "Cycle Build Time  Mismatch");
                Assert.AreEqual(5.00, (double)wellstatsudaulydto.WellTest.Value.FallTime, 0.01, "Cycle FallTime  Mismatch");

                Assert.AreEqual(2186.00, GetTruncatedValueforDouble((double)wellstatsudaulydto.WellTest.Value.RiseTime, 2), "CycleRiseTime  Mismatch");
                Assert.AreEqual(12216.00, GetTruncatedValueforDouble((double)wellstatsudaulydto.WellTest.Value.AfterFlowTime, 2), "Cycle After FlowTime  Mismatch");

                Assert.AreEqual(0.0m, wellstatsudaulydto.WellTest.Value.Oil, "Oil RateAllocated  Mismatch");
                Assert.AreEqual(43.58, GetTruncatedValueforDouble((double)wellstatsudaulydto.WellTest.Value.Water, 2), "Water Rate Allocated  Mismatch");
                Assert.AreEqual(89.31, GetTruncatedValueforDouble((double)wellstatsudaulydto.WellTest.Value.Gas, 2), "Gas Rate Allocated  Mismatch");

                #endregion AssertWellTestValues

                PlungerLiftCycleAndUnitsDTO plcCcyle = SurveillanceService.GetLatestPGLCycle(actual.WellId.ToString());

                #region AsserCycleData

                Assert.AreEqual(375.00m, plcCcyle.Value.MaximumCasingHeadPressure, "Maximum Casing Pressure  Mismatch");
                Assert.AreEqual(260.00m, plcCcyle.Value.MaximumTubingHeadPressure, "Maximum Tubing Pressure  Mismatch");
                Assert.AreEqual(300.00m, plcCcyle.Value.MinimumCasingHeadPressure, "Minimum Casing Pressure  Mismatch");
                Assert.AreEqual(86.00m, plcCcyle.Value.MinimumTubingHeadPressure, " Minimum Tubing Pressure  Mismatch");
                Assert.AreEqual(0.34m, plcCcyle.Value.MinimumLoadFactor, "Minimum Load Factor  Mismatch");
                Assert.AreEqual(79.93, GetTruncatedValueforDouble((double)plcCcyle.Value.FlowLinePressure, 2), "Avg Flow Line Presure   Mismatch");
                //slug Volume from where it comes ??

                Assert.AreEqual(135.38, (double)plcCcyle.Value.SlugVolume * 60, 0.01, "Slug Volume  Mismatch");
                Assert.AreEqual(22.5625, (double)plcCcyle.Value.Gas, 0.0001, "Cycle Gas Volume  Mismatch");
                Assert.AreEqual(240, (double)plcCcyle.Value.CycleTime, 0.01, "CycleTime  Mismatch");

                Assert.AreEqual(40, (double)plcCcyle.Value.BuildTime, 0.05, "Cycle Build Time  Mismatch");
                Assert.AreEqual(5, (double)plcCcyle.Value.FallTime, 0.01, "Cycle FallTime  Mismatch");
                Assert.AreEqual(15, (double)plcCcyle.Value.RiseTime, 0.01, "CycleRiseTime  Mismatch");
                Assert.AreEqual(180, (double)plcCcyle.Value.AfterFlowTime, 0.01, "Cycle After FlowTime  Mismatch");

                Assert.AreEqual(4.1, (double)plcCcyle.Value.OilRate, 0.1, "Oil RateAllocated  Mismatch");
                Assert.AreEqual(9.5, (double)plcCcyle.Value.WaterRate, 0.1, "Water Rate Allocated  Mismatch");
                Assert.AreEqual(135.38, (double)plcCcyle.Value.GasRate, 0.01, "Gas Rate Allocated  Mismatch");

                Assert.AreEqual(642.27, (double)plcCcyle.Value.PlungerRiseSpeed, 0.01, "PlungerRise Speed  Mismatch");
                Assert.AreEqual(200.00, (double)plcCcyle.Value.PlungerFallSpeedGas, 0.01, "Plunger Fall Speed Gas  Mismatch");
                Assert.AreEqual(75.00, (double)plcCcyle.Value.PlungerFallSpeedLiquid, 0.01, "Plunger Fall SpeedLiquid Mismatch");

                #endregion AsserCycleData
            }
            else //Metric Validation
            {
                //Assert.AreEqual("sm3", pglWellGauge.FirstOrDefault(x => x.Name.Equals("CycleSlugVolume")).Unit.ToString(), "Unit Text Mismatch for  Radial Gauge Cycle Slug Volume US");
                //Assert.AreEqual("sm3", pglWellGauge.FirstOrDefault(x => x.Name.Equals("CycleGasVolume")).Unit.ToString(), "Unit Text Mismatch for Radial Gauge Cycle Gas Volume US");
                ProductionKPIDTO productionkpidto = SurveillanceService.GetProductionKPI(actual.WellId.ToString(), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime()));
                Assert.IsNotNull(productionkpidto, "Prodution KPI is null");
                if (productionkpidto.CurrentAllocatedProduction.Gas != null)
                {
                    Assert.AreEqual((double)UnitsConversion("Mscf/d", 5.00), (double)productionkpidto.CurrentAllocatedProduction.Gas, 0.01, "Actaul Gas Rate For today Mistmatch [ Metric units] ");
                }
                else
                {
                    Trace.WriteLine("Current Allocation for Production is coming as NULL in Metric Units" + DateTime.Now.ToString());
                    Trace.WriteLine("Current Allocation for Last day gas (Metric) " + productionkpidto.LastAllocatedProduction.Gas);
                }
                Assert.AreEqual((double)UnitsConversion("Mscf/d", 89.31), (double)productionkpidto.WellTestProduction.Gas, 0.2, "Well Test Gas Rate For Plunger Mismatch [ Metric units]");

                //Verify Daily Averge Value:Metric:
                //WellDailyAverageAndTestDTO wellstatsudaulydto = SurveillanceService.GetWellDailyAverageAndTest(actual.WellId.ToString(), DTOExtensions.ToISO8601(DateTime.UtcNow.Date));
                WellDailyAverageAndTestDTO wellstatsudaulydto = SurveillanceService.GetWellDailyAverageAndTest(actual.WellId.ToString(), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime()));

                #region AssertForDailyAvgValues

                Assert.AreEqual((double)UnitsConversion("psia", 120), (double)wellstatsudaulydto.DailyAverage.Value.AverageMaximumCasingPressure, 0.1, "Maximum Casing Pressure  Mismatch [ Metric units]");
                Assert.AreEqual((double)UnitsConversion("psia", 120), (double)wellstatsudaulydto.DailyAverage.Value.AverageMaximumTubingPressure, 0.1, "Maximum Tubing Pressure  Mismatch [ Metric units]");
                Assert.AreEqual((double)UnitsConversion("psia", 100), (double)wellstatsudaulydto.DailyAverage.Value.AverageMinimumCasingPressure, 0.1, "Minimum Casing Pressure  Mismatch [ Metric units]");
                Assert.AreEqual((double)UnitsConversion("psia", 100), (double)wellstatsudaulydto.DailyAverage.Value.AverageMinimumTubingPressure, 0.1, " Minimum Tubing Pressure  Mismatch [ Metric units]");
                Assert.AreEqual((double)100, (double)wellstatsudaulydto.DailyAverage.Value.AverageMinimumLoadFactor, 0.1, "Minimum Load Factor  Mismatch [ Metric units]");
                Assert.AreEqual((double)UnitsConversion("psia", 10), (double)wellstatsudaulydto.DailyAverage.Value.FLP, 0.1, "Avg Flow Line Presure   Mismatch [ Metric units]");
                //slug Volume from where it comes ??

                double? slugdailyvolume = wellstatsudaulydto.DailyAverage.Value.AverageCycleWaterVolume + wellstatsudaulydto.DailyAverage.Value.AverageCycleOilVolume;
                Trace.WriteLine(" Slug = Oil Volume + Water Vloume " + (wellstatsudaulydto.DailyAverage.Value.AverageCycleWaterVolume + wellstatsudaulydto.DailyAverage.Value.AverageCycleOilVolume));
                Assert.AreEqual((double)UnitsConversion("STB/d", 139), (double)slugdailyvolume, 0.1, "Daily Avg Slug Voume Mismatch [ Metric units]");
                Assert.AreEqual((double)UnitsConversion("Mcf", 80), (double)wellstatsudaulydto.DailyAverage.Value.AverageCycleGasVolume, 0.2, "Cycle Gas Volume  Mismatch [ Metric units]");
                Assert.AreEqual(75, wellstatsudaulydto.DailyAverage.Value.AverageCycleTime, "AverageCycleTime  Mismatch [ Metric units]");
                //this Unit is in Minutes Need to Dividied by 60 When FRWM-3460 gets fixed
                Assert.AreEqual(60, wellstatsudaulydto.DailyAverage.Value.AverageCycleBuildTime, "Cycle Build Time  Mismatch [ Metric units]");
                Assert.AreEqual(120, wellstatsudaulydto.DailyAverage.Value.AverageCycleFallTime, "Cycle FallTime  Mismatch [ Metric units]");
                Assert.AreEqual(60, wellstatsudaulydto.DailyAverage.Value.AverageCycleRiseTime, "CycleRiseTime  Mismatch [ Metric units]");
                Assert.AreEqual(90, wellstatsudaulydto.DailyAverage.Value.AverageCycleAfterFlowTime, "Cycle After FlowTime  Mismatch [ Metric units]");

                Assert.AreEqual((double)UnitsConversion("STB/d", 8), (double)wellstatsudaulydto.DailyAverage.Value.OilRateAllocated, 0.1, "Oil RateAllocated  Mismatch [ Metric units]");
                Assert.AreEqual((double)UnitsConversion("STB/d", 10), (double)wellstatsudaulydto.DailyAverage.Value.WaterRateAllocated, 0.1, "Water Rate Allocated  Mismatch [ Metric units]");
                Assert.AreEqual((double)UnitsConversion("Mscf/d", 5), (double)wellstatsudaulydto.DailyAverage.Value.GasRateAllocated, 0.1, "Gas Rate Allocated  Mismatch [ Metric units]");
                //For Test Purpose only
                //  Assert.AreEqual(80, wellstatsudaulydto.DailyAverage.Value.AveragePlungerRiseSpeed, "PlungerRise Speed  Mismatch [ Metric units]");
                Assert.AreEqual("m/min", wellstatsudaulydto.DailyAverage.Units.AveragePlungerRiseSpeed.UnitKey, "Mismatch in Unit Text For Average Plunger RiseSpeed [Metric]");
                Assert.AreEqual(UnitsConversion("ft", 80), wellstatsudaulydto.DailyAverage.Value.AveragePlungerRiseSpeed, "PlungerRise Speed  Mismatch [ Metric units]");

                #endregion AssertForDailyAvgValues

                // Verify Unit Key text
                Assert.AreEqual("kPa", wellstatsudaulydto.DailyAverage.Units.AverageMaximumTubingPressure.UnitKey, "Mismatch in Unit Text For Average Maximum Tubing Pressure [Metric]");
                // Assert.AreEqual("ft/min", wellstatsudaulydto.DailyAverage.Units.AveragePlungerRiseSpeed.UnitKey, "Mismatch in Unit Text For Average Plunger RiseSpeed [Metric]");

                //Verify Well Test Value Metric:

                #region AssertWellTestValues

                Assert.AreEqual((double)UnitsConversion("psia", 1502.00), (double)wellstatsudaulydto.WellTest.Value.MaximumCasingPressure, 0.1, "Maximum Casing Pressure  Mismatch");
                Assert.AreEqual((double)UnitsConversion("psia", 797.00), (double)wellstatsudaulydto.WellTest.Value.MaximumTubingPressure, 0.1, "Maximum Tubing Pressure  Mismatch");
                Assert.AreEqual((double)UnitsConversion("psia", 943.00), (double)wellstatsudaulydto.WellTest.Value.MinimumCasingPressure, 0.1, "Minimum Casing Pressure  Mismatch");
                Assert.AreEqual((double)UnitsConversion("psia", 516.00), (double)wellstatsudaulydto.WellTest.Value.MinimumTubingPressure, 0.1, " Minimum Tubing Pressure  Mismatch");
                Assert.AreEqual((double)0.6103, GetTruncatedValueforDouble((double)wellstatsudaulydto.WellTest.Value.MinimumLoadFactor, CountDigitsAfterDecimal(0.6104)), 0.1, "Minimum Load Factor  Mismatch");
                Assert.AreEqual((double)UnitsConversion("psia", 347.00), (double)wellstatsudaulydto.WellTest.Value.FlowLinePressure, 0.1, "Avg Flow Line Presure   Mismatch");
                Assert.AreEqual((double)UnitsConversion("STB/d", 630.00), (double)wellstatsudaulydto.WellTest.Value.CycleSlugVolume, 0.1, "Cycle Slug Volume  Mismatch");

                Assert.AreEqual((double)UnitsConversion("Mcf", 1291.0000), (double)wellstatsudaulydto.WellTest.Value.CycleGasVolume, 0.1, "Cycle Gas Volume  Mismatch");
                Assert.AreEqual(20810, GetTruncatedValueforDouble((double)wellstatsudaulydto.WellTest.Value.CycleTime, 2), 0.1, "CycleTime  Mismatch");
                //this Unit is in Minutes Need to Dividied by 60 When FRWM-3460 gets fixed
                Assert.AreEqual(6408.00, GetTruncatedValueforDouble((double)wellstatsudaulydto.WellTest.Value.BuildTime, 2), "Cycle Build Time  Mismatch");
                Assert.AreEqual(5.00, (double)wellstatsudaulydto.WellTest.Value.FallTime, 0.01, "Cycle FallTime  Mismatch");
                Assert.AreEqual(2186.00, GetTruncatedValueforDouble((double)wellstatsudaulydto.WellTest.Value.RiseTime, 2), "CycleRiseTime  Mismatch");
                Assert.AreEqual(12216.00, GetTruncatedValueforDouble((double)wellstatsudaulydto.WellTest.Value.AfterFlowTime, 2), "Cycle After FlowTime  Mismatch");

                Assert.AreEqual(UnitsConversion("STB/d", 0.0), (double)wellstatsudaulydto.WellTest.Value.Oil, "Oil RateAllocated  Mismatch");
                Assert.AreEqual((double)UnitsConversion("STB/d", 43.58), GetTruncatedValueforDouble((double)wellstatsudaulydto.WellTest.Value.Water, 2), 0.2, "Water Rate Allocated  Mismatch");
                Assert.AreEqual((double)UnitsConversion("Mscf/d", 89.31), GetTruncatedValueforDouble((double)wellstatsudaulydto.WellTest.Value.Gas, 2), 0.1, "Gas Rate Allocated  Mismatch");

                #endregion AssertWellTestValues

                PlungerLiftCycleAndUnitsDTO plcCcyle = SurveillanceService.GetLatestPGLCycle(actual.WellId.ToString());

                #region AssertCycleData Metric:

                Assert.AreEqual((double)UnitsConversion("psia", 375.00), (double)plcCcyle.Value.MaximumCasingHeadPressure, 0.2, "Maximum Casing Pressure  Mismatch  [ Metric units] ");
                Assert.AreEqual((double)UnitsConversion("psia", 260.00), (double)plcCcyle.Value.MaximumTubingHeadPressure, 0.1, "Maximum Tubing Pressure  Mismatch [ Metric units]");
                Assert.AreEqual((double)UnitsConversion("psia", 300.00), (double)plcCcyle.Value.MinimumCasingHeadPressure, 0.1, "Minimum Casing Pressure  Mismatch [ Metric units]");
                Assert.AreEqual((double)UnitsConversion("psia", 86.00), (double)plcCcyle.Value.MinimumTubingHeadPressure, 0.1, " Minimum Tubing Pressure  Mismatch [ Metric units]");
                Assert.AreEqual(0.34m, plcCcyle.Value.MinimumLoadFactor, "Minimum Load Factor  Mismatch");
                Assert.AreEqual((double)UnitsConversion("psia", 79.93), GetTruncatedValueforDouble((double)plcCcyle.Value.FlowLinePressure, 2), 0.1, "Avg Flow Line Presure   Mismatch [ Metric units]");
                //slug Volume from where it comes ??

                Assert.AreEqual((double)UnitsConversion("STB/d", 135.38), (double)plcCcyle.Value.SlugVolume * 60, 0.01, "Cycle Slug Volume  Mismatch [ Metric units]");
                Assert.AreEqual((double)UnitsConversion("Mcf", 22.5625), (double)plcCcyle.Value.Gas, 0.0001, "Cycle Gas Volume  Mismatch [ Metric units]");
                Assert.AreEqual(240, (double)plcCcyle.Value.CycleTime, 0.01, "CycleTime  Mismatch [ Metric units]");

                Assert.AreEqual(40, (double)plcCcyle.Value.BuildTime, 0.1, "Cycle Build Time  Mismatch [ Metric units]");
                Assert.AreEqual(5, (double)plcCcyle.Value.FallTime, 0.01, "Cycle FallTime  Mismatch [ Metric units]");
                Assert.AreEqual(15, (double)plcCcyle.Value.RiseTime, 0.01, "CycleRiseTime  Mismatch [ Metric units]");
                Assert.AreEqual(180, (double)plcCcyle.Value.AfterFlowTime, 0.01, "Cycle After FlowTime  Mismatch [ Metric units]");

                Assert.AreEqual((double)UnitsConversion("STB/d", 4.1), (double)plcCcyle.Value.OilRate, 0.1, "Oil RateAllocated  Mismatch [ Metric units]");
                Assert.AreEqual((double)UnitsConversion("STB/d", 9.5), (double)plcCcyle.Value.WaterRate, 0.1, "Water Rate Allocated  Mismatch [ Metric units]");
                Assert.AreEqual((double)UnitsConversion("Mscf/d", 135.38), (double)plcCcyle.Value.GasRate, 0.15, "Gas Rate Allocated  Mismatch [ Metric units]");

                Assert.AreEqual((double)UnitsConversion("ft", 642.27), (double)plcCcyle.Value.PlungerRiseSpeed, 0.01, "PlungerRise Speed  Mismatch [ Metric units]");
                Assert.AreEqual((double)UnitsConversion("ft", 200.00), (double)plcCcyle.Value.PlungerFallSpeedGas, 0.01, "Plunger Fall Speed Gas  Mismatch [ Metric units]");
                Assert.AreEqual((double)UnitsConversion("ft", 75.00), (double)plcCcyle.Value.PlungerFallSpeedLiquid, 0.01, "Plunger Fall SpeedLiquid Mismatch [ Metric units]");

                #endregion AssertCycleData Metric:
            }

            //verify Plunger Cycle Trends Return Success for Bewlo UDCS for Performance Chart
            string[] UCDLIst = { ((int)DailyAverageQuantity.THP).ToString(), ((int)DailyAverageQuantity.CHP).ToString(), ((int)DailyAverageQuantity.DifferentialPressure).ToString(), ((int)DailyAverageQuantity.FLP).ToString(), ((int)DailyAverageQuantity.GasRateAllocated).ToString() };

            CygNetTrendDTO[] pglperfdto = SurveillanceService.GetPGLWellStatusTrends(UCDLIst, actual.WellId.ToString(), DTOExtensions.ToISO8601(DateTime.UtcNow.Date.AddDays(-30)).ToString(), DTOExtensions.ToISO8601(DateTime.UtcNow.Date).ToString());
            foreach (CygNetTrendDTO indudc in pglperfdto)
            {
                Assert.AreEqual("Success", indudc.ErrorMessage, "For Plunger Perfoamnce " + indudc.PointUDC + "Data was Not Success");
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellStatusDataWithUOM_GasLift()
        {
            string passFacilityId1 = "";
            try
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                passFacilityId1 = GetFacilityId("GLWELL_", 1);
                WellDTO wellGL = AddNonRRLWell(passFacilityId1, WellTypeId.GLift, true);
                WellGroupStatusQueryDTO newGroup = GroupStatusQuery(WellTypeId.GLift);
                var wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<GLWellStatusUnitDTO, GLWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), "Failed to get the added well");
                GLWellStatusValueDTO us_glWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(wellGL.Id.ToString(),
                    SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as GLWellStatusValueDTO;
                Assert.IsNotNull(us_glWellStatus, "Failed to get Well status in 'US' units");
                CompareGLWellStatus(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), us_glWellStatus);
                GLWellStatusValueDTO metric_glWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(wellGL.Id.ToString(),
                    SettingServiceStringConstants.UNIT_SYSTEM_NAME_METRIC).Value as GLWellStatusValueDTO;
                Assert.IsNotNull(metric_glWellStatus, "Failed to get Well status in 'US' units");
                CompareGLWellStatus(us_glWellStatus, metric_glWellStatus, false);
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");
                wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<GLWellStatusUnitDTO, GLWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), "Failed to get the added well");
                CompareGLWellStatus(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), metric_glWellStatus);
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                RemoveWell(passFacilityId1);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellStatusDataWithUOM_PlungerLift()
        {
            string facilityId1 = GetFacilityId("PGLWELL_", 1);
            try
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                //Add a New PGL Well .....

                WellDTO wellPGL = AddNonRRLWell(facilityId1, WellTypeId.PLift, true);
                //  Add Well Test and Well Daily Aveage Values direclty from datavbase and Run Cycle Data As Well

                #region ADDWellTest

                WellTestDTO testDataDTO = new WellTestDTO()
                {
                    WellId = wellPGL.Id,
                    SPTCodeDescription = "AllocatableTest",
                    SampleDate = wellPGL.CommissionDate.Value.AddDays(5).ToUniversalTime(),
                    TestDuration = random.Next(12, 24),
                    MaximumCasingPressure = 1502.00m,
                    MaximumTubingPressure = 797.00m,
                    MinimumCasingPressure = 943.00m,
                    MinimumTubingPressure = 516.00m,
                    FlowLinePressure = 347.0m,
                    BuildTime = 6408.00m,
                    AfterFlowTime = 12216.00m,
                    FallTime = 5.00m,
                    RiseTime = 2186.00m,
                    CycleGasVolume = 1291.0000m,
                    CycleWaterVolume = 630.00m,
                    CycleOilVolume = 0
                };
                //Saved WellTest Data
                WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellPGL.Id.ToString()).Units;
                WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                //Run Plunger Lift Cycle data

                #endregion ADDWellTest

                //Run Plunger Lift Cycle data
                SurveillanceService.GetLatestPGLCycleData(wellPGL, "5");
                var pglCycleDataFromDb = SurveillanceService.GetLatestPGLCycle(wellPGL.Id.ToString());

                //Enter WellDailyAverge from DB

                #region WellDailyAverage

                WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO
                {
                    EndDateTime = DateTime.Today.ToUniversalTime(),
                    StartDateTime = DateTime.Today.AddDays(-1).ToUniversalTime(),
                    AverageMaximumCasingPressure = 120,
                    AverageMaximumTubingPressure = 120,
                    AverageMinimumCasingPressure = 100,
                    AverageMinimumTubingPressure = 100,
                    AverageCycleGasVolume = 80,
                    AverageCycleTime = 75,
                    AverageCycleBuildTime = 60,
                    AverageCycleRiseTime = 60,
                    AverageCycleAfterFlowTime = 90,
                    AverageCycleOilVolume = 90,
                    AveragePlungerRiseSpeed = 80,
                    AverageCycleFallTime = 120,
                    AverageCycleWaterVolume = 49,
                    OilRateAllocated = 8,
                    WaterRateAllocated = 10,
                    GasRateAllocated = 5,
                    FLP = 10,
                    CHP = 329,
                    DifferentialPressure = 200,
                    THP = 100,
                    AverageMinimumLoadFactor = 100,
                    Status = WellDailyAverageDataStatus.Original,
                    RunTime = 24,
                    THT = 0,
                    WellId = wellPGL.Id,
                    Id = 0,
                };
                bool addDailyAvgDataForPGL = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
                Assert.IsTrue(addDailyAvgDataForPGL, "Failed to add Daily Average data for day 1");

                dailyAverageDTO.EndDateTime = DateTime.Today.AddDays(-1).ToUniversalTime();
                dailyAverageDTO.StartDateTime = DateTime.Today.AddDays(-2).ToUniversalTime();
                addDailyAvgDataForPGL = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
                Assert.IsTrue(addDailyAvgDataForPGL, "Failed to add Daily Average data for day 2");

                #endregion WellDailyAverage

                WellGroupStatusQueryDTO newGroup = GroupStatusQuery(WellTypeId.PLift);
                var wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<PGLWellStatusUnitDTO, PGLWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords, "WellFilter record is not 1");
                CygNetAlarmDTO cygNetAlarmPGL = SurveillanceService.GetCygNetAlarmsIncludeHiLowStatusByWellId(wellPGL.Id.ToString());
                Assert.IsNotNull(cygNetAlarmPGL, "Failed to get CygNet alarms for PGL");
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId1), "Failed to get the added well");
                PGLWellStatusValueDTO us_pglWellStatus = SurveillanceServiceClient.GetWellStatusData(wellPGL.Id.ToString()).Value as PGLWellStatusValueDTO;
                Assert.IsNotNull(us_pglWellStatus, "Failed to get Well status in 'US' units");
                //We are Essentialy Verifying if the Data from Group Status DTO for Selected Well is same as Data for Well in Well Status
                //......Also We check if the KPI data machtes with WellTest and DailyAverge Table Values
                ComparePGLWellStatus(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId1), us_pglWellStatus);
                //Change Unit System To 'METRIC'
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");
                PGLWellStatusValueDTO metric_pglWellStatus = SurveillanceServiceClient.GetWellStatusData(wellPGL.Id.ToString()).Value as PGLWellStatusValueDTO;
                Assert.IsNotNull(metric_pglWellStatus, "Failed to get Well status in 'Metric' units");
                wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<PGLWellStatusUnitDTO, PGLWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords, "WellFilter record is not 1");
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId1), "Failed to get the added well");
                ComparePGLWellStatus(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId1), metric_pglWellStatus, true, "METRIC");
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                RemoveWell(facilityId1);
            }
        }

        //FRWM-6500 -- Written test method for PCP Well Analysis - Operating Envelope Feature
        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetPCPWellAnalysisOperatingEnvelopechart()
        {
            //if (s_isRunningInATS == false)
            //{
            //    return;
            //}
            string facilityId = GetFacilityId("WFTA1K_", 1);
            try
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");

                //Adding PCP Well along with Well Test Data.....
                #region AddWell
                WellDTO pcpWell = AddNonRRLWell(facilityId, WellTypeId.PCP, true, CalibrationMethodId.LFactor);
                #endregion AddWell

                //Entering Valid Tune Successfull Well Test
                #region WellTest Tuning
                AddWellSettingWithDoubleValues(pcpWell.Id, "Min L Factor Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max L Factor Acceptance Limit", 2.0);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Min Water Cut Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max Water Cut Acceptance Limit", 1.0);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Min GOR Acceptance Limit", 1.0);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max GOR Acceptance Limit", 50.0);

                WellTestDTO latestTestData_PCP = WellTestDataService.GetLatestWellTestDataByWellId(pcpWell.Id.ToString());
                //Well Test Tuning
                WellTestDataService.TuneSelectedWellTests(new long[] { latestTestData_PCP.Id });
                WellTestDTO latestTestDataAfterTune_PCP = WellTestDataService.GetLatestWellTestDataByWellId(pcpWell.Id.ToString());
                Assert.AreEqual("TUNING_SUCCEEDED", latestTestDataAfterTune_PCP.Status.ToString(), "Well Test Status is not Success");
                Trace.WriteLine("Well Test Tuned Successfully:-> " + latestTestDataAfterTune_PCP.Status.ToString());
                #endregion WellTest Tuning

                //Entering Daily Average Reocrd
                #region WellDailyAverage
                WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO
                {
                    EndDateTime = DateTime.Today.ToUniversalTime(),
                    StartDateTime = DateTime.Today.AddDays(-1).ToUniversalTime(),
                    WellId = pcpWell.Id,
                    Id = 0,
                    RunTime = 24,
                    Status = WellDailyAverageDataStatus.Original,
                    THP = 119.09,
                    THT = 99.1,
                    CHP = 146.77,
                    PIP = 204.98,
                    PDP = 3.18,
                    CasingGasRate = 1.40,
                    FLP = 8.35,
                    OilRateAllocated = 268.69,
                    WaterRateAllocated = 177.8,
                    GasRateAllocated = 5,
                    MotorAmps = 10.37,
                    MotorVolts = 38.13,
                    MotorTemperature = 196.5,
                    PumpSpeed = 130,
                    PumpTorque = 90,
                    FBHP = 141.19,
                    OilRateInferred = 279.73,
                    WaterRateInferred = 187.90,
                    GasRateInferred = 13.84,
                };
                bool addDailyAvgDataForPCP = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
                Assert.IsTrue(addDailyAvgDataForPCP, "Failed to add Daily Average Record");
                Trace.WriteLine("Daily Average Record Added Successfully");

                for (int i = 0; i < 3; i++)
                {
                    dailyAverageDTO.PumpSpeed = dailyAverageDTO.PumpSpeed + 10;
                    dailyAverageDTO.PumpTorque = dailyAverageDTO.PumpTorque + 10;
                    dailyAverageDTO.EndDateTime = DateTime.Today.AddDays(-i - 1).ToUniversalTime();
                    dailyAverageDTO.StartDateTime = DateTime.Today.AddDays(-i - 2).ToUniversalTime();
                    addDailyAvgDataForPCP = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
                    Assert.IsTrue(addDailyAvgDataForPCP, "Failed to add Daily Average Record");
                    Trace.WriteLine("Daily Average Record Added Successfully");
                }
                #endregion WellDailyAverage

                //Getting Default Design Speed Steps
                #region Default Steps Modifying
                var getDefaultSpeedSteps = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.DEFAULT_DESIGN_SPEED_STEPS);
                Assert.AreEqual((double)5, getDefaultSpeedSteps, "Default design steps are correct");

                //Setting Default Design Speed Steps to 3
                SetValuesInSystemSettings(SettingServiceStringConstants.DEFAULT_DESIGN_SPEED_STEPS, "3");
                var modifiedDefaultSpeedSteps = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.DEFAULT_DESIGN_SPEED_STEPS);
                Assert.AreEqual((double)3, modifiedDefaultSpeedSteps, "Default design steps are correct");
                #endregion Default Steps Modifying

                //Running Operating Envelope Scheduler Command for the PCP Wells
                #region envelopeCommand
                // RunAnalysisTaskScheduler("-UpdateOperatingEnvelopeForPCP");
                WellTestDataService.PreparePCPOperatingEnvelope(pcpWell.Id.ToString());

                PCPOperatingEnvelopeDTO GetPCPOperatingEnvelope = WellTestDataService.GetOperatingEnvelopeByWellId(pcpWell.Id.ToString());
                Assert.IsNotNull(GetPCPOperatingEnvelope);  //It is covering TorqueOperatingEnvelope data and FluidAbovePumpCurve data. GetPCPWellOperatingEnvelopeCurveData already covered. It is written below only.
                #endregion envelopeCommand

                //US System -- Getting PCP OperatingEnvelope response 
                #region PCP Operating Envelope -- US System
                //Getting Design Operating Limits
                var TorqueDesignOperatingLimitIteam_US = WellTestDataService.GetOperatingEnvelopeTorqueLimitsByWellId(pcpWell.Id.ToString());
                Assert.IsNotNull(TorqueDesignOperatingLimitIteam_US);
                Assert.AreEqual((double)121.5, TorqueDesignOperatingLimitIteam_US.Item1, "Mismatch in Item 1 for Design Operating Limit");
                Assert.AreEqual((double)499.5, TorqueDesignOperatingLimitIteam_US.Item2, "Mismatch in Item 2 for Design Operating Limit");

                var startDate = DTOExtensions.ToISO8601(DateTime.Today.AddDays(-7).ToUniversalTime());
                var endDate = DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime());
                PCPWellOperatingEnvelopeCurveAndUnitsDTO getPCPWellOperatingEnvelopeCurveData = SurveillanceService.GetPCPWellOperatingEnvelopeCurveData(pcpWell.Id.ToString(), startDate.ToString(), endDate.ToString(), false.ToString());

                //Comparing Design Operating Limit Curve
                Assert.AreEqual((double)50, getPCPWellOperatingEnvelopeCurveData.DesignOperatingLimits[0].Value.PumpSpeed, "Mismatch 1st Speed Record For Design Operating Limit");
                Assert.AreEqual((double)121.5, getPCPWellOperatingEnvelopeCurveData.DesignOperatingLimits[0].Value.Torque, "Mismatch 1st Torque Record For Design Operating Limit");
                Assert.AreEqual((double)500, getPCPWellOperatingEnvelopeCurveData.DesignOperatingLimits[1].Value.PumpSpeed, "Mismatch 2nd Speed Record For Design Operating Limit");
                Assert.AreEqual((double)121.5, getPCPWellOperatingEnvelopeCurveData.DesignOperatingLimits[1].Value.Torque, "Mismatch 2nd Torque Record For Design Operating Limit");
                Assert.AreEqual((double)500, getPCPWellOperatingEnvelopeCurveData.DesignOperatingLimits[2].Value.PumpSpeed, "Mismatch 3rd Speed Record For Design Operating Limit");
                Assert.AreEqual((double)499.5, getPCPWellOperatingEnvelopeCurveData.DesignOperatingLimits[2].Value.Torque, "Mismatch 3rd Torque Record For Design Operating Limit");
                Assert.AreEqual((double)50, getPCPWellOperatingEnvelopeCurveData.DesignOperatingLimits[3].Value.PumpSpeed, "Mismatch 4th Speed Record For Design Operating Limit");
                Assert.AreEqual((double)499.5, getPCPWellOperatingEnvelopeCurveData.DesignOperatingLimits[3].Value.Torque, "Mismatch 4th Torque Record For Design Operating Limit");
                Assert.AreEqual((double)50, getPCPWellOperatingEnvelopeCurveData.DesignOperatingLimits[4].Value.PumpSpeed, "Mismatch 5th Speed Record For Design Operating Limit");
                Assert.AreEqual((double)121.5, getPCPWellOperatingEnvelopeCurveData.DesignOperatingLimits[4].Value.Torque, "Mismatch 5th Torque Record For Design Operating Limit");

                //Comparing Historical Points Curve
                Assert.AreEqual((double)160, getPCPWellOperatingEnvelopeCurveData.HistoricalPoints[0].Value.PumpSpeed, "Mismatch 1st Speed Record For Historical Points");
                Assert.AreEqual((double)319.08, getPCPWellOperatingEnvelopeCurveData.HistoricalPoints[0].Value.Torque, "Mismatch 1st Torque For Historical Points");
                Assert.AreEqual((double)150, getPCPWellOperatingEnvelopeCurveData.HistoricalPoints[1].Value.PumpSpeed, "Mismatch 2nd Speed For Historical Points");
                Assert.AreEqual((double)308.72, getPCPWellOperatingEnvelopeCurveData.HistoricalPoints[1].Value.Torque, "Mismatch 2nd Torque For Historical Points");
                Assert.AreEqual((double)140, getPCPWellOperatingEnvelopeCurveData.HistoricalPoints[2].Value.PumpSpeed, "Mismatch 3rd Speed For Historical Points");
                Assert.AreEqual((double)298.38, getPCPWellOperatingEnvelopeCurveData.HistoricalPoints[2].Value.Torque, "Mismatch 3rd Torque For Historical Points");
                Assert.AreEqual((double)130, getPCPWellOperatingEnvelopeCurveData.HistoricalPoints[3].Value.PumpSpeed, "Mismatch 4th Speed For Historical Points");
                Assert.AreEqual((double)288.25, getPCPWellOperatingEnvelopeCurveData.HistoricalPoints[3].Value.Torque, "Mismatch 4th Torque For Historical Points");

                //Comparing Torque Operating Envelope Curve
                Assert.AreEqual((double)80, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[0].Value.PumpSpeed, "Mismatch 1st Speed Record For Torque Operating Envelope");
                Assert.AreEqual((double)186.944, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[0].Value.Torque, "Mismatch 1st  Min Torque Record For Torque Operating Envelope");
                Assert.AreEqual((double)80, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[7].Value.PumpSpeed, "Mismatch 8th Speed Record For Torque Operating Envelope");
                Assert.AreEqual((double)280.416, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[7].Value.Torque, "Mismatch 8th Max Torque Record For Torque Operating Envelope");

                Assert.AreEqual((double)153.33, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[1].Value.PumpSpeed, "Mismatch 2nd Speed Record For Torque Operating Envelope");
                Assert.AreEqual((double)246.504, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[1].Value.Torque, "Mismatch 2nd Min Torque Record For Torque Operating Envelope");
                Assert.AreEqual((double)153.33, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[6].Value.PumpSpeed, "Mismatch 7th Speed Record For Torque Operating Envelope");
                Assert.AreEqual((double)369.756, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[6].Value.Torque, "Mismatch 7th Max Torque Record For Torque Operating Envelope");

                Assert.AreEqual((double)226.66, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[2].Value.PumpSpeed, "Mismatch 3rd Speed Record For Torque Operating Envelope");
                Assert.AreEqual((double)306.84, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[2].Value.Torque, "Mismatch 3rd Min Torque Record For Torque Operating Envelope");
                Assert.AreEqual((double)226.66, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[5].Value.PumpSpeed, "Mismatch 6th Speed Record For Torque Operating Envelope");
                Assert.AreEqual((double)460.26, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[5].Value.Torque, "Mismatch 6th Max Torque Record For Torque Operating Envelope");

                Assert.AreEqual((double)300, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[3].Value.PumpSpeed, "Mismatch 4th Speed Record For Torque Operating Envelope");
                Assert.AreEqual((double)368.424, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[3].Value.Torque, "Mismatch 4th Min Torque Record ForTorque Operating Envelope");
                Assert.AreEqual((double)300, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[4].Value.PumpSpeed, "Mismatch 5th Speed Record For Torque Operating Envelope");
                Assert.AreEqual((double)552.636, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[4].Value.Torque, "Mismatch 5th Max Torque Record For Torque Operating Envelope");

                Assert.AreEqual((double)80, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[8].Value.PumpSpeed, "Mismatch 9th Speed Record For Torque Operating Envelope");
                Assert.AreEqual((double)186.944, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.TorqueOperatingEnvelope[8].Value.Torque, "Mismatch 9th Torque Record For Torque Operating Envelope");

                //Comparing FAP Operating Envelope Curve
                Assert.AreEqual((double)80, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.FluidAbovePumpCurve[0].Value.PumpSpeed, "Mismatch 1st Speed Record For FAP Operating Envelope");
                Assert.AreEqual((double)4111.43, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.FluidAbovePumpCurve[0].Value.FluidAbovePump, "Mismatch 1st FAP Record For FAP Operating Envelope");
                Assert.AreEqual((double)153.33, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.FluidAbovePumpCurve[1].Value.PumpSpeed, "Mismatch 2nd Speed Record For FAP Operating Envelope");
                Assert.AreEqual((double)2939.72, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.FluidAbovePumpCurve[1].Value.FluidAbovePump, "Mismatch 2nd FAP Record For FAP Operating Envelope");

                Assert.AreEqual((double)226.66, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.FluidAbovePumpCurve[2].Value.PumpSpeed, "Mismatch 3rd Speed Record For FAP Operating Envelope");
                Assert.AreEqual((double)1641.01, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.FluidAbovePumpCurve[2].Value.FluidAbovePump, "Mismatch 3rd FAP Record For FAP Operating Envelope");
                Assert.AreEqual((double)300, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.FluidAbovePumpCurve[3].Value.PumpSpeed, "Mismatch 7th Speed Record For FAP Operating Envelope");
                Assert.AreEqual((double)283.33, getPCPWellOperatingEnvelopeCurveData.OperatingEnvelopeData.FluidAbovePumpCurve[3].Value.FluidAbovePump, "Mismatch 7th FAP Record For FAP Operating Envelope");

                //Comparing Current Value for Speed and Torque
                PCPWellStatusValueDTO pcpWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(pcpWell.Id.ToString(),
                                                         SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as PCPWellStatusValueDTO;
                Assert.AreEqual((double)pcpWellStatus.PumpSpeed, getPCPWellOperatingEnvelopeCurveData.CurrentPoint.Value.PumpSpeed, "Mistmatch in actual value for Spped For Current Point");
                Assert.AreEqual((double)pcpWellStatus.PumpTorque, getPCPWellOperatingEnvelopeCurveData.CurrentPoint.Value.Torque, "Mistmatch in actual value for Torque For Current Point");
                #endregion PCP Operating Envelope -- US System

                //Metric System -- Getting PCP OperatingEnvelope response
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");

                #region PCP Operating Envelope -- Metric System
                PCPWellOperatingEnvelopeCurveAndUnitsDTO getPCPWellOperatingEnvelopeCurveDataMetric = SurveillanceService.GetPCPWellOperatingEnvelopeCurveData(pcpWell.Id.ToString(), startDate.ToString(), endDate.ToString(), false.ToString());
                double fct = 1.355818;
                double tol = 0.02;

                var TorqueDesignOperatingLimitIteam_Metric = WellTestDataService.GetOperatingEnvelopeTorqueLimitsByWellId(pcpWell.Id.ToString());
                Assert.IsNotNull(TorqueDesignOperatingLimitIteam_Metric);
                Assert.AreEqual(GetTruncatedValueforDouble((double)(121.5 * fct), 2), TorqueDesignOperatingLimitIteam_Metric.Item1, tol, "Mismatch in Item 1 for Design Operating Limit");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(499.5 * fct), 2), TorqueDesignOperatingLimitIteam_Metric.Item2, tol, "Mismatch in Item 2 for Design Operating Limit");

                //Comparing Design Operating Limit Curve - Doesn't require Assert statement for Speed because no unit is changing for Metric
                Assert.AreEqual(GetTruncatedValueforDouble((double)(121.5 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.DesignOperatingLimits[0].Value.Torque, tol, "Mismatch 1st Torque Record For Design Operating Limit");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(121.5 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.DesignOperatingLimits[1].Value.Torque, tol, "Mismatch 2nd Torque Record For Design Operating Limit");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(499.5 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.DesignOperatingLimits[2].Value.Torque, tol, "Mismatch 3rd Torque Record For Design Operating Limit");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(499.5 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.DesignOperatingLimits[3].Value.Torque, tol, "Mismatch 4th Torque Record For Design Operating Limit");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(121.5 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.DesignOperatingLimits[4].Value.Torque, tol, "Mismatch 5th Torque Record For Design Operating Limit");

                //Comparing Historical Points Curve - Doesn't require Assert statement for Speed because no unit is changing for Metric
                Assert.AreEqual(GetTruncatedValueforDouble((double)(319.08 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.HistoricalPoints[0].Value.Torque, tol, "Mismatch 1st Torque For Historical Points");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(308.72 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.HistoricalPoints[1].Value.Torque, tol, "Mismatch 2nd Torque For Historical Points");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(298.38 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.HistoricalPoints[2].Value.Torque, tol, "Mismatch 3rd Torque For Historical Points");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(288.25 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.HistoricalPoints[3].Value.Torque, tol, "Mismatch 4th Torque For Historical Points");

                //Comparing Torque Operating Envelope Curve - Doesn't require Assert statement for Speed because no unit is changing for Metric
                Assert.AreEqual(GetTruncatedValueforDouble((double)(186.944 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.OperatingEnvelopeData.TorqueOperatingEnvelope[0].Value.Torque, tol, "Mismatch 1st Min Torque Record For Torque Operating Envelope");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(280.416 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.OperatingEnvelopeData.TorqueOperatingEnvelope[7].Value.Torque, tol, "Mismatch 8th Max Torque Record For Torque Operating Envelope");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(246.504 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.OperatingEnvelopeData.TorqueOperatingEnvelope[1].Value.Torque, tol, "Mismatch 2nd Min Torque Record For Torque Operating Envelope");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(369.756 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.OperatingEnvelopeData.TorqueOperatingEnvelope[6].Value.Torque, tol, "Mismatch 7th Max Torque Record For Torque Operating Envelope");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(306.84 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.OperatingEnvelopeData.TorqueOperatingEnvelope[2].Value.Torque, tol, "Mismatch 3rd Min Torque Record For Torque Operating Envelope");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(460.26 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.OperatingEnvelopeData.TorqueOperatingEnvelope[5].Value.Torque, tol, "Mismatch 6th Max Torque Record For Torque Operating Envelope");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(368.424 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.OperatingEnvelopeData.TorqueOperatingEnvelope[3].Value.Torque, tol, "Mismatch 4th Min Torque Record ForTorque Operating Envelope");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(552.636 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.OperatingEnvelopeData.TorqueOperatingEnvelope[4].Value.Torque, tol, "Mismatch 5th Max Torque Record For Torque Operating Envelope");
                Assert.AreEqual(GetTruncatedValueforDouble((double)(186.944 * fct), 2), getPCPWellOperatingEnvelopeCurveDataMetric.OperatingEnvelopeData.TorqueOperatingEnvelope[8].Value.Torque, tol, "Mismatch 9th Torque Record For Torque Operating Envelope");

                //Comparing FAP Operating Envelope Curve - Doesn't require Assert statement for Speed because no unit is changing for Metric
                Assert.AreEqual((double)(4111.43 * 0.3048), getPCPWellOperatingEnvelopeCurveDataMetric.OperatingEnvelopeData.FluidAbovePumpCurve[0].Value.FluidAbovePump, tol, "Mismatch 1st FAP Record For FAP Operating Envelope");
                Assert.AreEqual((double)(2939.72 * 0.3048), getPCPWellOperatingEnvelopeCurveDataMetric.OperatingEnvelopeData.FluidAbovePumpCurve[1].Value.FluidAbovePump, tol, "Mismatch 2nd FAP Record For FAP Operating Envelope");
                Assert.AreEqual((double)(1641.01 * 0.3048), getPCPWellOperatingEnvelopeCurveDataMetric.OperatingEnvelopeData.FluidAbovePumpCurve[2].Value.FluidAbovePump, tol, "Mismatch 3rd FAP Record For FAP Operating Envelope");
                Assert.AreEqual((double)(283.33 * 0.3048), getPCPWellOperatingEnvelopeCurveDataMetric.OperatingEnvelopeData.FluidAbovePumpCurve[3].Value.FluidAbovePump, tol, "Mismatch 7th FAP Record For FAP Operating Envelope");

                //Comparing Current Value for Speed and Torque -- Doesn't require Assert statement for Speed because no unit is changing for Metric
                PCPWellStatusValueDTO pcpWellStatusMetric = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(pcpWell.Id.ToString(),
                                                         SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as PCPWellStatusValueDTO;
                Assert.AreEqual((double)(pcpWellStatusMetric.PumpTorque * fct), getPCPWellOperatingEnvelopeCurveDataMetric.CurrentPoint.Value.Torque, "Mistmatch in actual value for Torque For Current Point");

                #endregion PCP Operating Envelope -- Metric System
            }
            finally
            {
                //Setting Default Design Speed Steps to 5
                SetValuesInSystemSettings(SettingServiceStringConstants.DEFAULT_DESIGN_SPEED_STEPS, "5");
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                RemoveWell(facilityId);
            }
        }


        //Written test method for PCP Well Status Validation (KPIs, Status Section and Current Values)
        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellStatusDataWithUOM_PCP()
        {
            string facilityId = GetFacilityId("WFTA1K_", 1);
            try
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");

                //Adding PCP Well along with Well Test Data.....
                #region AddWell
                WellDTO pcpWell = AddNonRRLWell(facilityId, WellTypeId.PCP, true, CalibrationMethodId.LFactor);
                #endregion AddWell

                #region WellTest Tuning
                AddWellSettingWithDoubleValues(pcpWell.Id, "Min L Factor Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max L Factor Acceptance Limit", 2.0);

                AddWellSettingWithDoubleValues(pcpWell.Id, "Min Water Cut Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max Water Cut Acceptance Limit", 1.0);

                AddWellSettingWithDoubleValues(pcpWell.Id, "Min GOR Acceptance Limit", 1.0);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max GOR Acceptance Limit", 50.0);

                WellTestDTO latestTestData_PCP = WellTestDataService.GetLatestWellTestDataByWellId(pcpWell.Id.ToString());
                WellTestDataService.TuneSelectedWellTests(new long[] { latestTestData_PCP.Id });
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
                    THP = 119.09,
                    THT = 99.1,
                    CHP = 146.77,
                    PIP = 204.98,
                    PDP = 3.18,
                    CasingGasRate = 1.40,
                    FLP = 8.35,
                    OilRateAllocated = 268.69,
                    WaterRateAllocated = 177.8,
                    //GasRateAllocated = 5,
                    MotorAmps = 10.37,
                    MotorVolts = 38.13,
                    MotorTemperature = 196.5,
                    PumpSpeed = 129.2,
                    PumpTorque = 88.84,
                    FBHP = 141.19,
                    //OilRateInferred = 279.73,
                    //WaterRateInferred = 187.90,
                    //GasRateInferred = 13.84,
                };
                bool addDailyAvgDataForPCP = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
                Assert.IsTrue(addDailyAvgDataForPCP, "Failed to add Daily Average data for day 1");

                dailyAverageDTO.EndDateTime = DateTime.Today.AddDays(-1).ToUniversalTime();
                dailyAverageDTO.StartDateTime = DateTime.Today.AddDays(-2).ToUniversalTime();
                addDailyAvgDataForPCP = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
                Assert.IsTrue(addDailyAvgDataForPCP, "Failed to add Daily Average data for day 2");

                #endregion WellDailyAverage

                //Filtering Group Status
                WellGroupStatusQueryDTO newGroup = GroupStatusQuery(WellTypeId.PCP);
                var wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<PCPWellStatusUnitDTO, PCPWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords, "WellFilter record is not 1");
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId), "Failed to get the added well");

                //CygNet Alarms Check     
                CygNetAlarmDTO cygNetAlarmPCP = SurveillanceService.GetCygNetAlarmsIncludeHiLowStatusByWellId(pcpWell.Id.ToString());
                Assert.IsNotNull(cygNetAlarmPCP, "Failed to get CygNet alarms for PCP");

                //Getting Well Staus in US System
                PCPWellStatusValueDTO us_pcpWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(pcpWell.Id.ToString(),
                                                         SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as PCPWellStatusValueDTO;
                Assert.IsNotNull(us_pcpWellStatus, "Failed to get Well status in 'US' units");

                //We are Essentialy Verifying if the Data from Group Status DTO for Selected Well is same as Data for Well in Well Status
                ComparePCPWellStatus(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId), us_pcpWellStatus);

                //Changing System to Metric
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");


                //Filtering Group Status
                wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<PCPWellStatusUnitDTO, PCPWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId), "Failed to get the added well");

                //Getting Well Staus in Metric System
                PCPWellStatusValueDTO metric_pcpWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(pcpWell.Id.ToString(),
                                                               SettingServiceStringConstants.UNIT_SYSTEM_NAME_METRIC).Value as PCPWellStatusValueDTO;
                Assert.IsNotNull(metric_pcpWellStatus, "Failed to get Well status in 'Metric' units");
                //We are Essentialy Verifying if the Data from Group Status DTO for Selected Well is same as Data for Well in Well Status
                ComparePCPWellStatus(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId), metric_pcpWellStatus, true, "METRIC");
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                RemoveWell(facilityId);
            }
        }

        public void ComparePCPWellStatus(PCPWellStatusValueDTO expected, PCPWellStatusValueDTO actual, bool check = true, string unitsystem = "US")
        {
            if (expected.AlarmMessage.Count() < actual.AlarmMessage.Count())
            {
                Assert.AreEqual(expected.AlarmMessage.Count() + 1, actual.AlarmMessage.Count(), "Mismatch in Alarm Message Count");
            }
            else
            {
                Assert.AreEqual(expected.AlarmMessage.Count(), actual.AlarmMessage.Count(), "Mismatch in Alarm Message Count condition 2");
                for (int i = 0; i < actual.AlarmMessage.Count(); i++)
                {
                    Assert.IsTrue(expected.AlarmMessage[i].Equals(actual.AlarmMessage[i]), "Alarm Message are not same");
                }
            }
            if (!string.IsNullOrEmpty(actual.AlarmMessageString) && !string.IsNullOrEmpty(expected.AlarmMessageString))
                Assert.IsTrue(actual.AlarmMessageString.Contains(expected.AlarmMessageString), "Actual Alarm is not containg Expected Alarm");
            Assert.AreEqual(expected.CommStatus, actual.CommStatus, "Mismatch in Comm Status");
            Assert.AreEqual(expected.WellStatus, actual.WellStatus, "Mismatch in Well Status");
            if (expected.LastAlarmTimes != null && actual.LastAlarmTimes != null)
            {
                Assert.AreEqual(expected.LastAlarmTimes.Count(), actual.LastAlarmTimes.Count(), "Mismatch in Last Alarm count");
                for (int i = 0; i < expected.LastAlarmTimes.Count(); i++)
                {
                    Assert.IsTrue(expected.LastAlarmTimes[i].Alarm.Equals(actual.LastAlarmTimes[i].Alarm), "Mismatch in Last Alarm Time Mesasge ");
                    Assert.IsNotNull(expected.LastAlarmTimes[i].TimeStamp, "Expected Last Alarm Time is Null");
                    Assert.IsNotNull(actual.LastAlarmTimes[i].TimeStamp, "Actual  Last Alarm Time is Null");
                    Assert.IsTrue(expected.LastAlarmTimes[i].TimeStamp.Value.Equals(actual.LastAlarmTimes[i].TimeStamp.Value), "Mismatch in Last Alarm Time,tiemestamp Mesasge ");
                }
            }
            else
            {
                Assert.AreEqual(expected.LastAlarmTimes, actual.LastAlarmTimes, "Mismatch in Last Alarm count");
            }
            //Verifying Well Status - RTU Status Section
            Assert.IsNotNull(expected.LastGoodScanTime, "Expected Last Good Scan Time is Null");
            Assert.IsNotNull(actual.LastGoodScanTime, "Actual  Last Good Scan Time is Null");
            Assert.AreEqual(expected.LastGoodScanTime, actual.LastGoodScanTime, "Msimatch in Last Good Scan Time");
            Assert.IsNotNull(expected.LastScanTime, "Expected Last Scan Time is Null");
            Assert.IsNotNull(actual.LastScanTime, "Actual Last Scan Time is Null");
            Assert.AreEqual(expected.LastScanTime, actual.LastScanTime, "Mismatch in Last Scan Time");
            Assert.AreEqual(expected.OutOfServiceCode, actual.OutOfServiceCode, "Mismatch in Service Code");
            Assert.AreEqual(expected.DeviceType, actual.DeviceType, "Mismatch in RTU Type");

            //Verifying Currnet Values under Performance Section
            if (check)
            {
                Assert.AreEqual((double)expected.TubingHeadTemperature, GetTruncatedValueforDouble((double)actual.TubingHeadTemperature, CountDigitsAfterDecimal((double)expected.TubingHeadTemperature)), "Tubing  Head Temperature Mismatch");
                Assert.AreEqual((double)expected.TubingPressure, GetTruncatedValueforDouble((double)actual.TubingPressure, CountDigitsAfterDecimal((double)expected.TubingPressure)), "Tubing  Head Pressure Mismatch");

                Assert.AreEqual((double)expected.CasingPressure, GetTruncatedValueforDouble((double)actual.CasingPressure, CountDigitsAfterDecimal((double)expected.CasingPressure)), "Casing Pressure Mismatch");
                Assert.AreEqual((double)expected.FlowLinePressure, GetTruncatedValueforDouble((double)actual.FlowLinePressure, CountDigitsAfterDecimal((double)expected.FlowLinePressure)), "Flow Line Pressure Mismatch");
                Assert.AreEqual((double)expected.PumpIntakePressure, GetTruncatedValueforDouble((double)actual.PumpIntakePressure, CountDigitsAfterDecimal((double)expected.PumpIntakePressure)), "Pump Intake Pressure Mismatch");
                Assert.AreEqual((double)expected.PumpDischargePressure, GetTruncatedValueforDouble((double)actual.PumpDischargePressure, CountDigitsAfterDecimal((double)expected.PumpDischargePressure)), "Pump Discharge Pressure Mismatch");

                Assert.AreEqual((double)expected.CasingGasRate, GetTruncatedValueforDouble((double)actual.CasingGasRate, CountDigitsAfterDecimal((double)expected.CasingGasRate)), "Gas Rate Mismatch");
                Assert.AreEqual((double)expected.OilRateMeasured, GetTruncatedValueforDouble((double)actual.OilRateMeasured, CountDigitsAfterDecimal((double)expected.OilRateMeasured)), "Oil Rate Mismatch");
                Assert.AreEqual((double)expected.WaterRateMeasured, GetTruncatedValueforDouble((double)actual.WaterRateMeasured, CountDigitsAfterDecimal((double)expected.WaterRateMeasured)), "Water Rate Mismatch");

                Assert.AreEqual((double)expected.PumpSpeed, GetTruncatedValueforDouble((double)actual.PumpSpeed, CountDigitsAfterDecimal((double)expected.PumpSpeed)), "Pump Speed Mismatch");
                Assert.AreEqual((double)expected.PumpTorque, GetTruncatedValueforDouble((double)actual.PumpTorque, CountDigitsAfterDecimal((double)expected.PumpTorque)), "Pump Speed Mismatch");

                Assert.AreEqual((double)expected.MotorAmps, GetTruncatedValueforDouble((double)actual.MotorAmps, CountDigitsAfterDecimal((double)expected.MotorAmps)), "Motor Amps Mismatch");
                Assert.AreEqual((double)expected.MotorTemperature, GetTruncatedValueforDouble((double)actual.MotorTemperature, CountDigitsAfterDecimal((double)expected.MotorTemperature)), "Motor Temperature Mismatch");
                Assert.AreEqual((double)expected.MotorVolts, GetTruncatedValueforDouble((double)actual.MotorVolts, CountDigitsAfterDecimal((double)expected.MotorVolts)), "Motor Volts Mismatch");
            }

            // Verify if the Values for Guages are not Null
            WellGaugeDTO[] pcpWellGauge = SurveillanceService.GetNonRRLWellGauges(actual.WellId.ToString());
            Assert.IsNotNull(pcpWellGauge.FirstOrDefault(x => x.Name.Equals("RunTimeYesterday")).CurrentVal, "Runtime value is null");
            Assert.IsNotNull(pcpWellGauge.FirstOrDefault(x => x.Name.Equals("RunTimeYesterday")).MinVal, "Min value for Runtime value is null");
            Assert.IsNotNull(pcpWellGauge.FirstOrDefault(x => x.Name.Equals("RunTimeYesterday")).MaxVal, "Max value for Runtime value is null");

            Assert.IsNotNull(pcpWellGauge.FirstOrDefault(x => x.Name.Equals("PumpIntakePressure")).CurrentVal, "PIP value is null");
            Assert.IsNotNull(pcpWellGauge.FirstOrDefault(x => x.Name.Equals("PumpIntakePressure")).MinVal, "Min value for PIP value is null");
            Assert.IsNotNull(pcpWellGauge.FirstOrDefault(x => x.Name.Equals("PumpIntakePressure")).MaxVal, "Max value for PIP value is null");

            Assert.IsNotNull(pcpWellGauge.FirstOrDefault(x => x.Name.Equals("PumpSpeed")).CurrentVal, "Speed value is null");
            Assert.IsNotNull(pcpWellGauge.FirstOrDefault(x => x.Name.Equals("PumpSpeed")).MinVal, "Speed value is null");
            Assert.IsNotNull(pcpWellGauge.FirstOrDefault(x => x.Name.Equals("PumpSpeed")).MaxVal, "Speed value is null");

            //Verify Daily Averge Value:
            WellDailyAverageAndTestDTO wellstatsudaulydto = SurveillanceService.GetWellDailyAverageAndTest(actual.WellId.ToString(), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime()));

            //US System Validation
            if (unitsystem.ToUpper() == "US")
            {
                #region AssertForDailyAvgValues
                Assert.AreEqual(24.0, wellstatsudaulydto.DailyAverage.Value.RunTime, "RunTime Mismatch");
                Assert.AreEqual(99.1, wellstatsudaulydto.DailyAverage.Value.THT, "THT Mismatch");
                Assert.AreEqual(119.09, wellstatsudaulydto.DailyAverage.Value.THP, "THP Mismatch");
                Assert.AreEqual(146.77, wellstatsudaulydto.DailyAverage.Value.CHP, "CHP Mismatch");
                Assert.AreEqual(204.98, wellstatsudaulydto.DailyAverage.Value.PIP, "PIP Mismatch");
                Assert.AreEqual(3.18, wellstatsudaulydto.DailyAverage.Value.PDP, "PDP Mismatch");
                Assert.AreEqual(1.40, wellstatsudaulydto.DailyAverage.Value.CasingGasRate, "Casing Gas Rate Mismatch");
                Assert.AreEqual(8.35, wellstatsudaulydto.DailyAverage.Value.FLP, "FLP Mismatch");
                Assert.AreEqual(268.69, wellstatsudaulydto.DailyAverage.Value.OilRateAllocated, "Actual Oil Rate Mismatch");
                Assert.AreEqual(177.8, wellstatsudaulydto.DailyAverage.Value.WaterRateAllocated, "Actual Water Rate Mismatch");
                Assert.AreEqual(10.37, wellstatsudaulydto.DailyAverage.Value.MotorAmps, "Motor Amp Mismatch");
                Assert.AreEqual(38.13, wellstatsudaulydto.DailyAverage.Value.MotorVolts, "Motor Volts Mismatch");
                Assert.AreEqual(196.5, wellstatsudaulydto.DailyAverage.Value.MotorTemperature, "Motor Temp Mismatch");
                Assert.AreEqual(129.2, wellstatsudaulydto.DailyAverage.Value.PumpSpeed, "Pump Speed Mismatch");
                Assert.AreEqual(88.84, wellstatsudaulydto.DailyAverage.Value.PumpTorque, "Pump Torque Mismatch");
                Assert.AreEqual((double)120.67, (double)wellstatsudaulydto.DailyAverage.Value.OilRateInferred, 0.01, "Inferred Oil Rate Mismatch");
                Assert.AreEqual((double)81.06, (double)wellstatsudaulydto.DailyAverage.Value.WaterRateInferred, 0.01, "Inferred Water Rate Mismatch");
                #endregion AssertForDailyAvgValues
            }

            //Validation for Metric
            else
            {
                #region AssertForDailyAvgValues
                Assert.AreEqual((double)UnitsConversion("psia", 119.09), (double)wellstatsudaulydto.DailyAverage.Value.THP, "THP Mismatch [Metric units]");
                Assert.AreEqual((double)UnitsConversion("F", 99.1), wellstatsudaulydto.DailyAverage.Value.THT, "THT Mismatch");
                Assert.AreEqual((double)UnitsConversion("psia", 146.77), wellstatsudaulydto.DailyAverage.Value.CHP, "CHP Mismatch");
                Assert.AreEqual((double)UnitsConversion("psia", 204.98), wellstatsudaulydto.DailyAverage.Value.PIP, "PIP Mismatch");
                Assert.AreEqual((double)UnitsConversion("psia", 3.18), wellstatsudaulydto.DailyAverage.Value.PDP, "PDP Mismatch");
                Assert.AreEqual((double)UnitsConversion("Mscf/d", 1.40), wellstatsudaulydto.DailyAverage.Value.CasingGasRate, "Casing Gas Rate Mismatch");
                Assert.AreEqual((double)UnitsConversion("psia", 8.35), wellstatsudaulydto.DailyAverage.Value.FLP, "FLP Mismatch");
                Assert.AreEqual((double)UnitsConversion("STB/d", 268.69), wellstatsudaulydto.DailyAverage.Value.OilRateAllocated, "Actual Oil Rate Mismatch");
                Assert.AreEqual((double)UnitsConversion("STB/d", 177.8), wellstatsudaulydto.DailyAverage.Value.WaterRateAllocated, "Actual Water Rate Mismatch");
                Assert.AreEqual(10.37, wellstatsudaulydto.DailyAverage.Value.MotorAmps, "Motor Amp Mismatch");
                Assert.AreEqual(38.13, wellstatsudaulydto.DailyAverage.Value.MotorVolts, "Motor Volts Mismatch");
                Assert.AreEqual((double)UnitsConversion("F", 196.5), wellstatsudaulydto.DailyAverage.Value.MotorTemperature, "Motor Temp Mismatch");
                Assert.AreEqual((double)(88.84 * 1.355818), wellstatsudaulydto.DailyAverage.Value.PumpTorque, "Pump Torque Mismatch");
                Assert.AreEqual((double)UnitsConversion("STB/d", 120.67), (double)wellstatsudaulydto.DailyAverage.Value.OilRateInferred, 0.01, "Inferred Oil Rate Mismatch");
                Assert.AreEqual((double)UnitsConversion("STB/d", 81.06), (double)wellstatsudaulydto.DailyAverage.Value.WaterRateInferred, 0.01, "Inferred Water Rate Mismatch");
                #endregion AssertForDailyAvgValues
            }

            //Verifying Last Valid Well Test under Perormance section
            #region AssertForValidWellTest

            WellTestArrayAndUnitsDTO getPCPWellTestData = WellTestDataService.GetWellTestDataByWellId(actual.WellId.ToString());
            //Assert.AreEqual((decimal)23.25, wellstatsudaulydto.WellTest.Value.Oil, "Entered Actual Oil is Mismatched");
            Assert.AreEqual(getPCPWellTestData.Values.FirstOrDefault().Oil.Value, wellstatsudaulydto.WellTest.Value.Oil, "Entered Actual Oil is Mismatched");
            Assert.AreEqual(getPCPWellTestData.Values.FirstOrDefault().Water.Value, wellstatsudaulydto.WellTest.Value.Water, "Entered Actaul Water is Mismatched");
            Assert.AreEqual(getPCPWellTestData.Values.FirstOrDefault().AverageTubingPressure.Value, wellstatsudaulydto.WellTest.Value.AverageTubingPressure, "Entered TubingPressure  is Mismatched");
            Assert.AreEqual(getPCPWellTestData.Values.FirstOrDefault().AverageTubingTemperature.Value, wellstatsudaulydto.WellTest.Value.AverageTubingTemperature, "Entered Tubing Temperature is Mismatched");
            Assert.AreEqual(getPCPWellTestData.Values.FirstOrDefault().AverageCasingPressure.Value, wellstatsudaulydto.WellTest.Value.AverageCasingPressure, "Entered Casing Pressure is Mismatched");
            Assert.AreEqual(getPCPWellTestData.Values.FirstOrDefault().PumpIntakePressure.Value, wellstatsudaulydto.WellTest.Value.PumpIntakePressure, "Entered Pump Intake Pressure is Mismatched");

            Assert.AreEqual(getPCPWellTestData.Values.FirstOrDefault().PumpDischargePressure.Value, wellstatsudaulydto.WellTest.Value.PumpDischargePressure, "Entered Pump Intake Pressure is Mismatched");
            Assert.AreEqual(getPCPWellTestData.Values.FirstOrDefault().FlowLinePressure.Value, wellstatsudaulydto.WellTest.Value.FlowLinePressure, "Entered Flow Line Pressure is Mismatched");
            Assert.AreEqual(getPCPWellTestData.Values.FirstOrDefault().MotorCurrent.Value, wellstatsudaulydto.WellTest.Value.MotorCurrent, "Entered Motor Current is Mismatched");
            Assert.AreEqual(getPCPWellTestData.Values.FirstOrDefault().MotorVolts.Value, wellstatsudaulydto.WellTest.Value.MotorVolts, "Entered Motor Volts is Mismatched");
            Assert.AreEqual(getPCPWellTestData.Values.FirstOrDefault().PumpTorque.Value, wellstatsudaulydto.WellTest.Value.PumpTorque, "Entered Pump Torque is Mismatched");
            Assert.AreEqual(getPCPWellTestData.Values.FirstOrDefault().PumpSpeed.Value, wellstatsudaulydto.WellTest.Value.PumpSpeed, "Entered Pump Speed is Mismatched");
            #endregion AssertForValidWellTest

        }

        public void CompareNFWellStatus(NFWellStatusValueDTO expected, NFWellStatusValueDTO actual, bool check = true)
        {
            if (expected.AlarmMessage.Count() < actual.AlarmMessage.Count())
            {
                Assert.AreEqual(expected.AlarmMessage.Count() + 1, actual.AlarmMessage.Count());
            }
            else
            {
                Assert.AreEqual(expected.AlarmMessage.Count(), actual.AlarmMessage.Count());
            }
            Assert.AreEqual(expected.CommStatus, actual.CommStatus);
            Assert.AreEqual(expected.FlowStatus, actual.FlowStatus);
            if (expected.LastAlarmTimes != null && actual.LastAlarmTimes != null)
            {
                Assert.AreEqual(expected.LastAlarmTimes.Count(), actual.LastAlarmTimes.Count());
                for (int i = 0; i < expected.LastAlarmTimes.Count(); i++)
                {
                    Assert.IsTrue(expected.LastAlarmTimes[i].Alarm.Equals(actual.LastAlarmTimes[i].Alarm));
                    Assert.IsNotNull(expected.LastAlarmTimes[i].TimeStamp);
                    Assert.IsNotNull(actual.LastAlarmTimes[i].TimeStamp);
                    Assert.IsTrue(expected.LastAlarmTimes[i].TimeStamp.Value.Equals(actual.LastAlarmTimes[i].TimeStamp.Value));
                }
            }
            else
            {
                Assert.AreEqual(expected.LastAlarmTimes, actual.LastAlarmTimes);
            }
            Assert.IsNotNull(expected.LastGoodScanTime);
            Assert.IsNotNull(actual.LastGoodScanTime);
            Assert.AreEqual(expected.LastGoodScanTime, actual.LastGoodScanTime);
            Assert.IsNotNull(expected.LastScanTime);
            Assert.IsNotNull(actual.LastScanTime);
            Assert.AreEqual(expected.LastScanTime, actual.LastScanTime);
            Assert.AreEqual(expected.OutOfServiceCode, actual.OutOfServiceCode);
            Assert.AreEqual(expected.DeviceType, actual.DeviceType);
            Assert.AreEqual(expected.RunStatus, actual.RunStatus);
            if (check)
            {
                Assert.AreEqual(expected.ChokeDiameter.Value, actual.ChokeDiameter.Value, 1);
                Assert.AreEqual(expected.DHGaugePressure.Value, actual.DHGaugePressure.Value, 1);
                Assert.AreEqual(expected.FlowLinePressure.Value, actual.FlowLinePressure.Value, 1);
                Assert.AreEqual(expected.TubingHeadTemperature.Value, actual.TubingHeadTemperature.Value, 1);
                Assert.AreEqual(expected.TubingPressure.Value, actual.TubingPressure.Value, 1);
                Assert.IsTrue((actual.WaterRateMeasured - expected.WaterRateMeasured) < 1);
                Assert.IsTrue((actual.GasRateMeasured - expected.GasRateMeasured) < 1);
                Assert.IsTrue((actual.OilRateMeasured - expected.OilRateMeasured) < 1);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellStatusDataWithUOM_NF()
        {
            string passFacilityId1 = "";
            try
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                passFacilityId1 = GetFacilityId("NFWWELL_", 1);
                WellDTO wellNF = AddNonRRLWell(passFacilityId1, WellTypeId.NF, true);
                WellGroupStatusQueryDTO newGroup = GroupStatusQuery(WellTypeId.NF);
                var wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<NFWellStatusUnitDTO, NFWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), "Failed to get the added well");
                NFWellStatusValueDTO us_nfWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(wellNF.Id.ToString(),
                    SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as NFWellStatusValueDTO;
                Assert.IsNotNull(us_nfWellStatus, "Failed to get Well status in 'US' units");
                CompareNFWellStatus(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), us_nfWellStatus);
                NFWellStatusValueDTO metric_nfWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(wellNF.Id.ToString(),
                    SettingServiceStringConstants.UNIT_SYSTEM_NAME_METRIC).Value as NFWellStatusValueDTO;
                Assert.IsNotNull(metric_nfWellStatus, "Failed to get Well status in 'US' units");
                CompareNFWellStatus(us_nfWellStatus, metric_nfWellStatus, false);
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");
                wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<NFWellStatusUnitDTO, NFWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), "Failed to get the added well");
                CompareNFWellStatus(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), metric_nfWellStatus);
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                RemoveWell(passFacilityId1);
            }
        }

        public void CompareGIWellStatus(GIWellStatusValueDTO expected, GIWellStatusValueDTO actual, bool check = true)
        {
            if (expected.AlarmMessage.Count() < actual.AlarmMessage.Count())
            {
                Assert.AreEqual(expected.AlarmMessage.Count() + 1, actual.AlarmMessage.Count());
            }
            else
            {
                Assert.AreEqual(expected.AlarmMessage.Count(), actual.AlarmMessage.Count());
            }
            Assert.AreEqual(expected.CommStatus, actual.CommStatus);
            if (expected.LastAlarmTimes != null && actual.LastAlarmTimes != null)
            {
                Assert.AreEqual(expected.LastAlarmTimes.Count(), actual.LastAlarmTimes.Count());
                for (int i = 0; i < expected.LastAlarmTimes.Count(); i++)
                {
                    Assert.IsTrue(expected.LastAlarmTimes[i].Alarm.Equals(actual.LastAlarmTimes[i].Alarm));
                    Assert.IsNotNull(expected.LastAlarmTimes[i].TimeStamp);
                    Assert.IsNotNull(actual.LastAlarmTimes[i].TimeStamp);
                    Assert.IsTrue(expected.LastAlarmTimes[i].TimeStamp.Value.Equals(actual.LastAlarmTimes[i].TimeStamp.Value));
                }
            }
            else
            {
                Assert.AreEqual(expected.LastAlarmTimes, actual.LastAlarmTimes);
            }
            Assert.IsNotNull(expected.LastGoodScanTime);
            Assert.IsNotNull(actual.LastGoodScanTime);
            Assert.AreEqual(expected.LastGoodScanTime, actual.LastGoodScanTime);
            Assert.IsNotNull(expected.LastScanTime);
            Assert.IsNotNull(actual.LastScanTime);
            Assert.AreEqual(expected.LastScanTime, actual.LastScanTime);
            Assert.AreEqual(expected.OutOfServiceCode, actual.OutOfServiceCode);
            Assert.AreEqual(expected.DeviceType, actual.DeviceType);
            if (check)
            {
                Assert.AreEqual(expected.DHGaugePressure.Value, actual.DHGaugePressure.Value, 1);
                Assert.AreEqual(expected.TubingHeadTemperature.Value, actual.TubingHeadTemperature.Value, 1);
                Assert.AreEqual(expected.TubingPressure.Value, actual.TubingPressure.Value, 1);
                Assert.AreEqual((double)(expected.GasInjectionRate), (double)(actual.GasInjectionRate), 2);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellStatusDataWithUOM_GasInjection()
        {
            string passFacilityId1 = "";
            try
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                passFacilityId1 = GetFacilityId("GASINJWELL_", 1);
                WellDTO wellGInj = AddNonRRLWell(passFacilityId1, WellTypeId.GInj, true);
                WellGroupStatusQueryDTO newGroup = GroupStatusQuery(WellTypeId.GInj);
                var wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<GIWellStatusUnitDTO, GIWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), "Failed to get the added well");
                GIWellStatusValueDTO us_giWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(wellGInj.Id.ToString(),
                    SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as GIWellStatusValueDTO;
                Assert.IsNotNull(us_giWellStatus, "Failed to get Well status in 'US' units");
                CompareGIWellStatus(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), us_giWellStatus);
                GIWellStatusValueDTO metric_giWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(wellGInj.Id.ToString(),
                    SettingServiceStringConstants.UNIT_SYSTEM_NAME_METRIC).Value as GIWellStatusValueDTO;
                Assert.IsNotNull(metric_giWellStatus, "Failed to get Well status in 'US' units");
                CompareGIWellStatus(us_giWellStatus, metric_giWellStatus, false);
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");
                wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<GIWellStatusUnitDTO, GIWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), "Failed to get the added well");
                CompareGIWellStatus(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), metric_giWellStatus);
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                RemoveWell(passFacilityId1);
            }
        }

        public void CompareWIWellStatus(WIWellStatusValueDTO expected, WIWellStatusValueDTO actual, bool check = true)
        {
            if (expected.AlarmMessage.Count() < actual.AlarmMessage.Count())
            {
                Assert.AreEqual(expected.AlarmMessage.Count() + 1, actual.AlarmMessage.Count());
            }
            else
            {
                Assert.AreEqual(expected.AlarmMessage.Count(), actual.AlarmMessage.Count());
            }
            Assert.AreEqual(expected.CommStatus, actual.CommStatus);
            if (expected.LastAlarmTimes != null && actual.LastAlarmTimes != null)
            {
                Assert.AreEqual(expected.LastAlarmTimes.Count(), actual.LastAlarmTimes.Count());
                for (int i = 0; i < expected.LastAlarmTimes.Count(); i++)
                {
                    Assert.IsTrue(expected.LastAlarmTimes[i].Alarm.Equals(actual.LastAlarmTimes[i].Alarm));
                    Assert.IsNotNull(expected.LastAlarmTimes[i].TimeStamp);
                    Assert.IsNotNull(actual.LastAlarmTimes[i].TimeStamp);
                    Assert.IsTrue(expected.LastAlarmTimes[i].TimeStamp.Value.Equals(actual.LastAlarmTimes[i].TimeStamp.Value));
                }
            }
            else
            {
                Assert.AreEqual(expected.LastAlarmTimes, actual.LastAlarmTimes);
            }
            Assert.IsNotNull(expected.LastGoodScanTime);
            Assert.IsNotNull(actual.LastGoodScanTime);
            Assert.AreEqual(expected.LastGoodScanTime, actual.LastGoodScanTime);
            Assert.IsNotNull(expected.LastScanTime);
            Assert.IsNotNull(actual.LastScanTime);
            Assert.AreEqual(expected.LastScanTime, actual.LastScanTime);
            Assert.AreEqual(expected.OutOfServiceCode, actual.OutOfServiceCode);
            Assert.AreEqual(expected.DeviceType, actual.DeviceType);
            if (check)
            {
                Assert.AreEqual(expected.DHGaugePressure.Value, actual.DHGaugePressure.Value, 1);
                Assert.AreEqual(expected.TubingHeadTemperature.Value, actual.TubingHeadTemperature.Value, 1);
                Assert.AreEqual(expected.TubingPressure.Value, actual.TubingPressure.Value, 1);
                Assert.AreEqual(expected.WaterInjectionRate, actual.WaterInjectionRate, 1);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellStatusDataWithUOM_WaterInjection()
        {
            string passFacilityId1 = "";
            try
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                passFacilityId1 = GetFacilityId("WATERINJWELL_", 1);
                WellDTO wellWInj = AddNonRRLWell(passFacilityId1, WellTypeId.WInj, true);
                WellGroupStatusQueryDTO newGroup = GroupStatusQuery(WellTypeId.WInj);
                var wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<WIWellStatusUnitDTO, WIWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), "Failed to get the added well");
                WIWellStatusValueDTO us_wiWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(wellWInj.Id.ToString(),
                    SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as WIWellStatusValueDTO;
                Assert.IsNotNull(us_wiWellStatus, "Failed to get Well status in 'US' units");
                CompareWIWellStatus(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), us_wiWellStatus);
                WIWellStatusValueDTO metric_wiWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(wellWInj.Id.ToString(),
                    SettingServiceStringConstants.UNIT_SYSTEM_NAME_METRIC).Value as WIWellStatusValueDTO;
                Assert.IsNotNull(metric_wiWellStatus, "Failed to get Well status in 'US' units");
                CompareWIWellStatus(us_wiWellStatus, metric_wiWellStatus, false);
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");
                wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<WIWellStatusUnitDTO, WIWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), "Failed to get the added well");
                CompareWIWellStatus(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == passFacilityId1), metric_wiWellStatus);
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                RemoveWell(passFacilityId1);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetDownTimeCodeForON()
        {
            DownTimeStatusCodeDTO downCodeON = SurveillanceService.GetDownTimeStatusCodeForOn();
            Assert.IsNotNull(downCodeON, "Failed to get ON code");
            Assert.AreEqual("IN SERVICE", downCodeON.DownTimeStatus, "Failed to get Downtime status for ON code");
            Assert.AreEqual("ON", downCodeON.DownTimeStatusCode, "Failed to get DowntimeStatusCode for ON code");
            Assert.IsNotNull(downCodeON.DownTimeStatusId, "Downcode Id should not ne null");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellKPIDataWithUnits()
        {
            string facilityId;
            if (s_isRunningInATS)
                facilityId = "RPOC_00004";
            else
                facilityId = "RPOC_0001";
            try
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                WellDTO wellRRL = AddRRLWell(facilityId);
                WellGroupStatusQueryDTO newGroup = GroupStatusQuery(WellTypeId.RRL);
                //Issue a forced scan command since there were no DG transactions in CygNet datagroup on ATS for "first time" case
                SurveillanceServiceClient.IssueCommandForSingleWell(wellRRL.Id, WellCommand.DemandScan.ToString());
                Thread.Sleep(2000);
                var wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId), "Failed to get the added well");
                WellCurrentKPIAndUnitsDTO wellKPI = SurveillanceService.GetWellKPIAndUnitsData(wellRRL.Id.ToString());
                Assert.IsNotNull(wellKPI, "Failed get well KPI information");
                Assert.IsNotNull(wellKPI.Units, "Failed get well KPI Units information");
                Assert.IsNotNull(wellKPI.Value, "Failed get well KPI Value information");
                Assert.AreEqual(wellsbyFilter.Units.InferredProductionToday.Precision, wellKPI.Units.InferredProduction.Precision);
                Assert.AreEqual(wellsbyFilter.Units.InferredProductionYesterday.Precision, wellKPI.Units.InferredProductionYesterday.Precision);
                Assert.AreEqual(wellsbyFilter.Units.InferredProductionToday.UnitKey, wellKPI.Units.InferredProduction.UnitKey);
                Assert.AreEqual(wellsbyFilter.Units.InferredProductionYesterday.UnitKey, wellKPI.Units.InferredProductionYesterday.UnitKey);
                Assert.AreEqual(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId).InferredProductionToday, wellKPI.Value.InferredProduction);
                Assert.AreEqual(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId).InferredProductionYesterday, wellKPI.Value.InferredProductionYesterday);
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");
                wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(newGroup);
                Assert.AreEqual(1, wellsbyFilter.TotalRecords);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId), "Failed to get the added well");
                wellKPI = SurveillanceService.GetWellKPIAndUnitsData(wellRRL.Id.ToString());
                Assert.IsNotNull(wellKPI, "Failed get well KPI information");
                Assert.IsNotNull(wellKPI.Units, "Failed get well KPI Units information");
                Assert.IsNotNull(wellKPI.Value, "Failed get well KPI Value information");
                Assert.AreEqual(wellsbyFilter.Units.InferredProductionToday.Precision, wellKPI.Units.InferredProduction.Precision);
                Assert.AreEqual(wellsbyFilter.Units.InferredProductionYesterday.Precision, wellKPI.Units.InferredProductionYesterday.Precision);
                Assert.AreEqual(wellsbyFilter.Units.InferredProductionToday.UnitKey, wellKPI.Units.InferredProduction.UnitKey);
                Assert.AreEqual(wellsbyFilter.Units.InferredProductionYesterday.UnitKey, wellKPI.Units.InferredProductionYesterday.UnitKey);
                Assert.AreEqual(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId).InferredProductionToday, wellKPI.Value.InferredProduction);
                Assert.AreEqual(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == facilityId).InferredProductionYesterday, wellKPI.Value.InferredProductionYesterday);
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                RemoveWell(facilityId);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void AddWellDailyAverageData()
        {
            try
            {
                WellDTO wellGL = AddNonRRLWell("GLWELL_0001", WellTypeId.GLift, true);
                WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO
                {
                    EndDateTime = DateTime.Now.ToUniversalTime(),
                    StartDateTime = DateTime.Now.AddDays(-3).ToUniversalTime(),
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
                    WellId = wellGL.Id,
                    DHPG = 300,
                    PDP = 2000,
                    PIP = 100,
                    MotorAmps = 34,
                    FLP = 678,
                };
                bool addDailyAvgData = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
                Assert.IsTrue(addDailyAvgData, "Failed to add Daily Average data");
                WellDailyAverageValueDTO getdailyAverageDTO = SurveillanceService.GetWellDailyAverageData(wellGL.Id.ToString());
                Assert.IsNotNull(getdailyAverageDTO);
                Assert.IsNotNull(getdailyAverageDTO.Id);
                Assert.IsNull(getdailyAverageDTO.TestDate);
                Assert.IsNull(getdailyAverageDTO.WellTestId);
                Assert.AreEqual(dailyAverageDTO.WellId, getdailyAverageDTO.WellId);
                Assert.AreEqual(dailyAverageDTO.ChokeDiameter, getdailyAverageDTO.ChokeDiameter);
                Assert.AreEqual(dailyAverageDTO.Duration, getdailyAverageDTO.Duration);
                Assert.AreEqual(dailyAverageDTO.DHPG, getdailyAverageDTO.DHPG);
                Assert.AreEqual(dailyAverageDTO.EndDateTime.ToString(), getdailyAverageDTO.EndDateTime.ToString());
                Assert.AreEqual(dailyAverageDTO.StartDateTime.ToString(), getdailyAverageDTO.StartDateTime.ToString());
                Assert.AreEqual(dailyAverageDTO.FLP, getdailyAverageDTO.FLP);
                Assert.AreEqual(dailyAverageDTO.GasJectionDepth, getdailyAverageDTO.GasJectionDepth);
                Assert.AreEqual(dailyAverageDTO.GasRateAllocated, getdailyAverageDTO.GasRateAllocated);
                Assert.AreEqual(dailyAverageDTO.GasRateInferred, getdailyAverageDTO.GasRateInferred);
                Assert.AreEqual(dailyAverageDTO.OilRateAllocated, getdailyAverageDTO.OilRateAllocated);
                Assert.AreEqual(dailyAverageDTO.OilRateInferred, getdailyAverageDTO.OilRateInferred);
                Assert.AreEqual(dailyAverageDTO.WaterRateAllocated, getdailyAverageDTO.WaterRateAllocated);
                Assert.AreEqual(dailyAverageDTO.WaterRateInferred, getdailyAverageDTO.WaterRateInferred);
                getdailyAverageDTO.ChokeDiameter = 65.0;
                addDailyAvgData = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(getdailyAverageDTO);
                getdailyAverageDTO = SurveillanceService.GetWellDailyAverageData(wellGL.Id.ToString());
                Assert.IsNotNull(getdailyAverageDTO);
                Assert.IsNotNull(getdailyAverageDTO.Id);
                Assert.IsNull(getdailyAverageDTO.TestDate);
                Assert.IsNull(getdailyAverageDTO.WellTestId);
                Assert.AreEqual(dailyAverageDTO.WellId, getdailyAverageDTO.WellId);
                Assert.AreEqual(65.0, getdailyAverageDTO.ChokeDiameter);
                Assert.AreEqual(dailyAverageDTO.Duration, getdailyAverageDTO.Duration);
                Assert.AreEqual(dailyAverageDTO.DHPG, getdailyAverageDTO.DHPG);
                Assert.AreEqual(dailyAverageDTO.EndDateTime.ToString(), getdailyAverageDTO.EndDateTime.ToString());
                Assert.AreEqual(dailyAverageDTO.StartDateTime.ToString(), getdailyAverageDTO.StartDateTime.ToString());
                Assert.AreEqual(dailyAverageDTO.FLP, getdailyAverageDTO.FLP);
                Assert.AreEqual(dailyAverageDTO.GasJectionDepth, getdailyAverageDTO.GasJectionDepth);
                Assert.AreEqual(dailyAverageDTO.GasRateAllocated, getdailyAverageDTO.GasRateAllocated);
                Assert.AreEqual(dailyAverageDTO.GasRateInferred, getdailyAverageDTO.GasRateInferred);
                Assert.AreEqual(dailyAverageDTO.OilRateAllocated, getdailyAverageDTO.OilRateAllocated);
                Assert.AreEqual(dailyAverageDTO.OilRateInferred, getdailyAverageDTO.OilRateInferred);
                Assert.AreEqual(dailyAverageDTO.WaterRateAllocated, getdailyAverageDTO.WaterRateAllocated);
                Assert.AreEqual(dailyAverageDTO.WaterRateInferred, getdailyAverageDTO.WaterRateInferred);
            }
            finally
            {
                RemoveWell("GLWELL_0001");
            }
        }

        public void AddWells(WellTypeId wType)
        {
            string facilityId = "";
            string facilityIdFormat = "";
            switch (wType)
            {
                case WellTypeId.ESP:
                    facilityId = "ESPWELL_000";
                    break;

                case WellTypeId.GLift:
                    facilityId = "GLWELL_000";
                    break;

                case WellTypeId.NF:
                    facilityId = "NFWWELL_000";
                    break;

                case WellTypeId.RRL:
                    {
                        if (s_isRunningInATS)
                            facilityIdFormat = "D5";
                        else
                            facilityIdFormat = "D4";
                        for (int i = 1; i <= 5; i++)
                        {
                            AddRRLWell("RPOC_" + i.ToString(facilityIdFormat));
                            RemoveWell("RPOC_" + i.ToString(facilityIdFormat));
                        }
                    }
                    break;
            }
            if (wType != WellTypeId.RRL)
            {
                for (int i = 1; i <= 5; i++)
                {
                    AddNonRRLWell(facilityId + i.ToString(), wType, true);
                    RemoveWell(facilityId + i.ToString());
                }
            }
        }

        public void AddWellsAndCalculateDailyAverage()
        {
            bool runDailyAvgCalculation = false;
            AddWells(WellTypeId.RRL);
            AddWells(WellTypeId.ESP);
            AddWells(WellTypeId.GLift);
            AddWells(WellTypeId.NF);
            WellDTO[] wells = WellService.GetAllWells().ToArray();
            int duration = 5;
            foreach (WellDTO well in wells)
            {
                Trace.WriteLine($"Adding Daily Averge for Well : {well.Name}");
                for (int i = duration; i > 0; i--)
                {
                    string startDate = DTOExtensions.ToISO8601(DateTime.Now.Date.AddHours(-i * 24).ToUniversalTime());
                    string endDate = DTOExtensions.ToISO8601(DateTime.Now.Date.AddHours(-(i - 1) * 24).AddMilliseconds(-10).ToUniversalTime());
                    runDailyAvgCalculation = SurveillanceService.AddDailyAverageFromVHSByDateRange(well.Id.ToString(), startDate, endDate);
                    Assert.IsTrue(runDailyAvgCalculation, "Failed to calculate dailyAverage for the well : " + well.Name);
                }
            }
        }

        public void ValidateProductionChangeTileMap(bool change, bool usePercent)
        {
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
            TileMapFilterDTO tileMap = new TileMapFilterDTO();
            tileMap.DateRequested = DateTime.Now.Date.ToUniversalTime();
            tileMap.IsNegative = change;
            tileMap.TopWellCountRequested = 25;
            tileMap.UsePercentChangeForRank = usePercent;
            tileMap.WellFilter = new WellFilterDTO();
            ProductionChangeTileMapAndUnitsDTO us_getProductionTileMap = SurveillanceService.GetProductionChangeTileMap(tileMap);
            Assert.IsNotNull(us_getProductionTileMap, "Failed to get Production Tilemap");
            Assert.AreEqual(change, us_getProductionTileMap.IsDecreasedProduction, "Failed to get Production Tilemap");
            Assert.IsNotNull(us_getProductionTileMap.TopGasChangeRate, "Failed to get Gas Change in Production Tilemap");
            Assert.IsNotNull(us_getProductionTileMap.TopOilChangeRate, "Failed to get Oil Change in Production Tilemap");
            Assert.IsNotNull(us_getProductionTileMap.TopWaterChangeRate, "Failed to get Water Change in Production Tilemap");
            Assert.IsNotNull(us_getProductionTileMap.TopLiquidChangeRate, "Failed to get Liquid Change in Production Tilemap");
            Assert.IsNotNull(us_getProductionTileMap.TopGasChangeRate.Units, "Failed to get Gas Units in Production Tilemap");
            Assert.IsNotNull(us_getProductionTileMap.TopOilChangeRate.Units, "Failed to get Oil Units in Production Tilemap");
            Assert.IsNotNull(us_getProductionTileMap.TopWaterChangeRate.Units, "Failed to get Water Units in Production Tilemap");
            Assert.IsNotNull(us_getProductionTileMap.TopLiquidChangeRate.Units, "Failed to get Liquid Units in Production Tilemap");
            //Check for Gas in US Unit:
            Assert.AreEqual(2, (int)us_getProductionTileMap.TopGasChangeRate.Units.BaseProduction.Precision);
            Assert.AreEqual(2, (int)us_getProductionTileMap.TopGasChangeRate.Units.ChangeRate.Precision);
            Assert.AreEqual(2, (int)us_getProductionTileMap.TopGasChangeRate.Units.CurrentProduction.Precision);
            Assert.AreEqual(2, (int)us_getProductionTileMap.TopGasChangeRate.Units.ProductionChange.Precision);
            Assert.AreEqual("Mscf/d", us_getProductionTileMap.TopGasChangeRate.Units.BaseProduction.UnitKey);
            Assert.AreEqual("%", us_getProductionTileMap.TopGasChangeRate.Units.ChangeRate.UnitKey);
            Assert.AreEqual("Mscf/d", us_getProductionTileMap.TopGasChangeRate.Units.CurrentProduction.UnitKey);
            Assert.AreEqual("Mscf/d", us_getProductionTileMap.TopGasChangeRate.Units.ProductionChange.UnitKey);
            //Check for Oil in US Unit:
            Assert.AreEqual(1, (int)us_getProductionTileMap.TopOilChangeRate.Units.BaseProduction.Precision);
            Assert.AreEqual(2, (int)us_getProductionTileMap.TopOilChangeRate.Units.ChangeRate.Precision);
            Assert.AreEqual(1, (int)us_getProductionTileMap.TopOilChangeRate.Units.CurrentProduction.Precision);
            Assert.AreEqual(1, (int)us_getProductionTileMap.TopOilChangeRate.Units.ProductionChange.Precision);
            Assert.AreEqual("STB/d", us_getProductionTileMap.TopOilChangeRate.Units.BaseProduction.UnitKey);
            Assert.AreEqual("%", us_getProductionTileMap.TopOilChangeRate.Units.ChangeRate.UnitKey);
            Assert.AreEqual("STB/d", us_getProductionTileMap.TopOilChangeRate.Units.CurrentProduction.UnitKey);
            Assert.AreEqual("STB/d", us_getProductionTileMap.TopOilChangeRate.Units.ProductionChange.UnitKey);
            //Check for Water in US Unit:
            Assert.AreEqual(1, (int)us_getProductionTileMap.TopWaterChangeRate.Units.BaseProduction.Precision);
            Assert.AreEqual(2, (int)us_getProductionTileMap.TopWaterChangeRate.Units.ChangeRate.Precision);
            Assert.AreEqual(1, (int)us_getProductionTileMap.TopWaterChangeRate.Units.CurrentProduction.Precision);
            Assert.AreEqual(1, (int)us_getProductionTileMap.TopWaterChangeRate.Units.ProductionChange.Precision);
            Assert.AreEqual("STB/d", us_getProductionTileMap.TopWaterChangeRate.Units.BaseProduction.UnitKey);
            Assert.AreEqual("%", us_getProductionTileMap.TopWaterChangeRate.Units.ChangeRate.UnitKey);
            Assert.AreEqual("STB/d", us_getProductionTileMap.TopWaterChangeRate.Units.CurrentProduction.UnitKey);
            Assert.AreEqual("STB/d", us_getProductionTileMap.TopWaterChangeRate.Units.ProductionChange.UnitKey);
            //Check for Liquid in US Unit:
            Assert.AreEqual(1, (int)us_getProductionTileMap.TopLiquidChangeRate.Units.BaseProduction.Precision);
            Assert.AreEqual(2, (int)us_getProductionTileMap.TopLiquidChangeRate.Units.ChangeRate.Precision);
            Assert.AreEqual(1, (int)us_getProductionTileMap.TopLiquidChangeRate.Units.CurrentProduction.Precision);
            Assert.AreEqual(1, (int)us_getProductionTileMap.TopLiquidChangeRate.Units.ProductionChange.Precision);
            Assert.AreEqual("STB/d", us_getProductionTileMap.TopLiquidChangeRate.Units.BaseProduction.UnitKey);
            Assert.AreEqual("%", us_getProductionTileMap.TopLiquidChangeRate.Units.ChangeRate.UnitKey);
            Assert.AreEqual("STB/d", us_getProductionTileMap.TopLiquidChangeRate.Units.CurrentProduction.UnitKey);
            Assert.AreEqual("STB/d", us_getProductionTileMap.TopLiquidChangeRate.Units.ProductionChange.UnitKey);
            Assert.IsNotNull(us_getProductionTileMap.TopGasChangeRate.Values, "Failed to get Gas Values in Production Tilemap");
            Assert.IsNotNull(us_getProductionTileMap.TopOilChangeRate.Values, "Failed to get Oil Values in Production Tilemap");
            Assert.IsNotNull(us_getProductionTileMap.TopWaterChangeRate.Values, "Failed to get Water Values in Production Tilemap");
            Assert.IsNotNull(us_getProductionTileMap.TopLiquidChangeRate.Values, "Failed to get Liquid Values in Production Tilemap");
            ChangeUnitSystem("Metric");
            ChangeUnitSystemUserSetting("Metric");
            Trace.WriteLine("Checking Precisions in Metric...");
            ProductionChangeTileMapAndUnitsDTO metric_getProductionTileMap = SurveillanceService.GetProductionChangeTileMap(tileMap);
            Assert.IsNotNull(metric_getProductionTileMap, "Failed to get Production Tilemap");
            Assert.AreEqual(change, metric_getProductionTileMap.IsDecreasedProduction, "Failed to get Production Tilemap");
            Assert.IsNotNull(metric_getProductionTileMap.TopGasChangeRate, "Failed to get Gas Change in Production Tilemap");
            Assert.IsNotNull(metric_getProductionTileMap.TopOilChangeRate, "Failed to get Oil Change in Production Tilemap");
            Assert.IsNotNull(metric_getProductionTileMap.TopWaterChangeRate, "Failed to get Water Change in Production Tilemap");
            Assert.IsNotNull(metric_getProductionTileMap.TopLiquidChangeRate, "Failed to get Liquid Change in Production Tilemap");
            Assert.IsNotNull(metric_getProductionTileMap.TopGasChangeRate.Units, "Failed to get Gas Units in Production Tilemap");
            Assert.IsNotNull(metric_getProductionTileMap.TopOilChangeRate.Units, "Failed to get Oil Units in Production Tilemap");
            Assert.IsNotNull(metric_getProductionTileMap.TopWaterChangeRate.Units, "Failed to get Water Units in Production Tilemap");
            Assert.IsNotNull(metric_getProductionTileMap.TopLiquidChangeRate.Units, "Failed to get Liquid Units in Production Tilemap");
            //Check for Gas in metric:
            Assert.AreEqual(3, (int)metric_getProductionTileMap.TopGasChangeRate.Units.BaseProduction.Precision, "Precision check Failed for TopGasChangeRate BaseProduction");
            Assert.AreEqual(2, (int)metric_getProductionTileMap.TopGasChangeRate.Units.ChangeRate.Precision);
            Assert.AreEqual(3, (int)metric_getProductionTileMap.TopGasChangeRate.Units.CurrentProduction.Precision);
            Assert.AreEqual(3, (int)metric_getProductionTileMap.TopGasChangeRate.Units.ProductionChange.Precision);
            Assert.AreEqual("sm3/d", metric_getProductionTileMap.TopGasChangeRate.Units.BaseProduction.UnitKey);
            Assert.AreEqual("%", metric_getProductionTileMap.TopGasChangeRate.Units.ChangeRate.UnitKey);
            Assert.AreEqual("sm3/d", metric_getProductionTileMap.TopGasChangeRate.Units.CurrentProduction.UnitKey);
            Assert.AreEqual("sm3/d", metric_getProductionTileMap.TopGasChangeRate.Units.ProductionChange.UnitKey);
            //Check for Oil in metric:
            Assert.AreEqual(3, (int)metric_getProductionTileMap.TopOilChangeRate.Units.BaseProduction.Precision);
            Assert.AreEqual(2, (int)metric_getProductionTileMap.TopOilChangeRate.Units.ChangeRate.Precision);
            Assert.AreEqual(3, (int)metric_getProductionTileMap.TopOilChangeRate.Units.CurrentProduction.Precision);
            Assert.AreEqual(3, (int)metric_getProductionTileMap.TopOilChangeRate.Units.ProductionChange.Precision);
            Assert.AreEqual("sm3/d", metric_getProductionTileMap.TopOilChangeRate.Units.BaseProduction.UnitKey);
            Assert.AreEqual("%", metric_getProductionTileMap.TopOilChangeRate.Units.ChangeRate.UnitKey);
            Assert.AreEqual("sm3/d", metric_getProductionTileMap.TopOilChangeRate.Units.CurrentProduction.UnitKey);
            Assert.AreEqual("sm3/d", metric_getProductionTileMap.TopOilChangeRate.Units.ProductionChange.UnitKey);
            //Check for Water in metric:
            Assert.AreEqual(3, (int)metric_getProductionTileMap.TopWaterChangeRate.Units.BaseProduction.Precision);
            Assert.AreEqual(2, (int)metric_getProductionTileMap.TopWaterChangeRate.Units.ChangeRate.Precision);
            Assert.AreEqual(3, (int)metric_getProductionTileMap.TopWaterChangeRate.Units.CurrentProduction.Precision);
            Assert.AreEqual(3, (int)metric_getProductionTileMap.TopWaterChangeRate.Units.ProductionChange.Precision);
            Assert.AreEqual("sm3/d", metric_getProductionTileMap.TopWaterChangeRate.Units.BaseProduction.UnitKey);
            Assert.AreEqual("%", metric_getProductionTileMap.TopWaterChangeRate.Units.ChangeRate.UnitKey);
            Assert.AreEqual("sm3/d", metric_getProductionTileMap.TopWaterChangeRate.Units.CurrentProduction.UnitKey);
            Assert.AreEqual("sm3/d", metric_getProductionTileMap.TopWaterChangeRate.Units.ProductionChange.UnitKey);
            //Check for Liquid in metric:
            Assert.AreEqual(3, (int)metric_getProductionTileMap.TopLiquidChangeRate.Units.BaseProduction.Precision);
            Assert.AreEqual(2, (int)metric_getProductionTileMap.TopLiquidChangeRate.Units.ChangeRate.Precision);
            Assert.AreEqual(3, (int)metric_getProductionTileMap.TopLiquidChangeRate.Units.CurrentProduction.Precision);
            Assert.AreEqual(3, (int)metric_getProductionTileMap.TopLiquidChangeRate.Units.ProductionChange.Precision);
            Assert.AreEqual("sm3/d", metric_getProductionTileMap.TopLiquidChangeRate.Units.BaseProduction.UnitKey);
            Assert.AreEqual("%", metric_getProductionTileMap.TopLiquidChangeRate.Units.ChangeRate.UnitKey);
            Assert.AreEqual("sm3/d", metric_getProductionTileMap.TopLiquidChangeRate.Units.CurrentProduction.UnitKey);
            Assert.AreEqual("sm3/d", metric_getProductionTileMap.TopLiquidChangeRate.Units.ProductionChange.UnitKey);

            Assert.IsNotNull(metric_getProductionTileMap.TopGasChangeRate.Values, " Metric Units: Failed to get Gas Values in Production Tilemap");
            Assert.IsNotNull(metric_getProductionTileMap.TopOilChangeRate.Values, "Metric Units:Failed to get Oil Values in Production Tilemap");
            Assert.IsNotNull(metric_getProductionTileMap.TopWaterChangeRate.Values, "Metric Units:Failed to get Water Values in Production Tilemap");
            Assert.IsNotNull(metric_getProductionTileMap.TopLiquidChangeRate.Values, "Metric Units: Failed to get Liquid Values in Production Tilemap");

            Assert.AreEqual(us_getProductionTileMap.TopGasChangeRate.Values.Count(), metric_getProductionTileMap.TopGasChangeRate.Values.Count());
            Assert.AreEqual(us_getProductionTileMap.TopGasChangeRate.Values.Count(), metric_getProductionTileMap.TopGasChangeRate.Values.Count());
            Assert.AreEqual(us_getProductionTileMap.TopGasChangeRate.Values.Count(), metric_getProductionTileMap.TopGasChangeRate.Values.Count());
            for (int i = 0; i < us_getProductionTileMap.TopGasChangeRate.Values.Count(); i++)
            {
                Assert.IsNotNull(us_getProductionTileMap.TopGasChangeRate.Values[i].WellId);
                Assert.IsNotNull(us_getProductionTileMap.TopGasChangeRate.Values[i].WellName);
                Assert.IsNotNull(us_getProductionTileMap.TopGasChangeRate.Values[i].WellLabel);
                if (usePercent)
                {
                    Assert.IsTrue(us_getProductionTileMap.TopGasChangeRate.Values[i].WellLabel.Contains("%"), "% Label not shown when percent was selected");
                }
                else
                {
                    Assert.IsTrue(us_getProductionTileMap.TopGasChangeRate.Values[i].WellLabel.Contains("Mscf/d"), "Volume unit not shown when voulme was selected");
                    Assert.IsTrue(metric_getProductionTileMap.TopGasChangeRate.Values[i].WellLabel.Contains("sm3/d"), "Volume unit not shown when volume was selected");
                }
                Assert.AreEqual(UnitsConversion("Mscf/d", us_getProductionTileMap.TopGasChangeRate.Values[i].BaseProduction).Value,
                    metric_getProductionTileMap.TopGasChangeRate.Values[i].BaseProduction.Value, 1);
                Assert.AreEqual(us_getProductionTileMap.TopGasChangeRate.Values[i].ChangeRate.Value,
                    metric_getProductionTileMap.TopGasChangeRate.Values[i].ChangeRate.Value, 1);
                Assert.AreEqual(UnitsConversion("Mscf/d", us_getProductionTileMap.TopGasChangeRate.Values[i].CurrentProduction).Value,
                    metric_getProductionTileMap.TopGasChangeRate.Values[i].CurrentProduction.Value, 1);
                Assert.AreEqual(UnitsConversion("Mscf/d", us_getProductionTileMap.TopGasChangeRate.Values[i].ProductionChange).Value,
                    metric_getProductionTileMap.TopGasChangeRate.Values[i].ProductionChange.Value, 1);
            }
            for (int i = 0; i < us_getProductionTileMap.TopOilChangeRate.Values.Count(); i++)
            {
                Assert.IsNotNull(us_getProductionTileMap.TopOilChangeRate.Values[i].WellId);
                Assert.IsNotNull(us_getProductionTileMap.TopOilChangeRate.Values[i].WellName);
                Assert.IsNotNull(us_getProductionTileMap.TopOilChangeRate.Values[i].WellLabel);
                if (usePercent)
                {
                    Assert.IsTrue(us_getProductionTileMap.TopOilChangeRate.Values[i].WellLabel.Contains("%"), "% Label not shown when percent was selected");
                }
                else
                {
                    Assert.IsTrue(us_getProductionTileMap.TopOilChangeRate.Values[i].WellLabel.Contains("STB/d"), "Volume unit not shown when voulme was selected");
                    Assert.IsTrue(metric_getProductionTileMap.TopOilChangeRate.Values[i].WellLabel.Contains("sm3/d"), "Volume unit not shown when voulme was selected");
                }
                Assert.AreEqual(UnitsConversion("STB/d", us_getProductionTileMap.TopOilChangeRate.Values[i].BaseProduction).Value,
                    metric_getProductionTileMap.TopOilChangeRate.Values[i].BaseProduction.Value, 1);
                Assert.AreEqual(us_getProductionTileMap.TopOilChangeRate.Values[i].ChangeRate.Value,
                    metric_getProductionTileMap.TopOilChangeRate.Values[i].ChangeRate.Value, 1);
                Assert.AreEqual(UnitsConversion("STB/d", us_getProductionTileMap.TopOilChangeRate.Values[i].CurrentProduction).Value,
                    metric_getProductionTileMap.TopOilChangeRate.Values[i].CurrentProduction.Value, 1);
                Assert.AreEqual(UnitsConversion("STB/d", us_getProductionTileMap.TopOilChangeRate.Values[i].ProductionChange).Value,
                    metric_getProductionTileMap.TopOilChangeRate.Values[i].ProductionChange.Value, 1);

                //Verify For Increased Production Tile Maps as per given Data

            }
            for (int i = 0; i < us_getProductionTileMap.TopWaterChangeRate.Values.Count(); i++)
            {
                Assert.IsNotNull(us_getProductionTileMap.TopWaterChangeRate.Values[i].WellId);
                Assert.IsNotNull(us_getProductionTileMap.TopWaterChangeRate.Values[i].WellName);
                Assert.IsNotNull(us_getProductionTileMap.TopWaterChangeRate.Values[i].WellLabel);
                if (usePercent)
                {
                    Assert.IsTrue(us_getProductionTileMap.TopWaterChangeRate.Values[i].WellLabel.Contains("%"), "% Label not shown when percent was selected");
                }
                else
                {
                    Assert.IsTrue(us_getProductionTileMap.TopWaterChangeRate.Values[i].WellLabel.Contains("STB/d"), "Volume unit not shown when voulme was selected");
                    Assert.IsTrue(metric_getProductionTileMap.TopWaterChangeRate.Values[i].WellLabel.Contains("sm3/d"), "Volume unit not shown when voulme was selected");
                }
                Assert.AreEqual(UnitsConversion("STB/d", us_getProductionTileMap.TopWaterChangeRate.Values[i].BaseProduction).Value,
                    metric_getProductionTileMap.TopWaterChangeRate.Values[i].BaseProduction.Value, 1);
                Assert.AreEqual(us_getProductionTileMap.TopWaterChangeRate.Values[i].ChangeRate.Value,
                    metric_getProductionTileMap.TopWaterChangeRate.Values[i].ChangeRate.Value, 1);
                Assert.AreEqual(UnitsConversion("STB/d", us_getProductionTileMap.TopWaterChangeRate.Values[i].CurrentProduction).Value,
                    metric_getProductionTileMap.TopWaterChangeRate.Values[i].CurrentProduction.Value, 1);
                Assert.AreEqual(UnitsConversion("STB/d", us_getProductionTileMap.TopWaterChangeRate.Values[i].ProductionChange).Value,
                    metric_getProductionTileMap.TopWaterChangeRate.Values[i].ProductionChange.Value, 1);
            }

            for (int i = 0; i < us_getProductionTileMap.TopLiquidChangeRate.Values.Count(); i++)
            {
                Assert.IsNotNull(us_getProductionTileMap.TopLiquidChangeRate.Values[i].WellId);
                Assert.IsNotNull(us_getProductionTileMap.TopLiquidChangeRate.Values[i].WellName);
                Assert.IsNotNull(us_getProductionTileMap.TopLiquidChangeRate.Values[i].WellLabel);
                if (usePercent)
                {
                    Assert.IsTrue(us_getProductionTileMap.TopLiquidChangeRate.Values[i].WellLabel.Contains("%"), "% Label not shown when percent was selected");
                }
                else
                {
                    Assert.IsTrue(us_getProductionTileMap.TopLiquidChangeRate.Values[i].WellLabel.Contains("STB/d"), "Volume unit not shown when voulme was selected");
                    Assert.IsTrue(metric_getProductionTileMap.TopLiquidChangeRate.Values[i].WellLabel.Contains("sm3/d"), "Volume unit not shown when voulme was selected");
                }
                Assert.AreEqual(UnitsConversion("STB/d", us_getProductionTileMap.TopLiquidChangeRate.Values[i].BaseProduction).Value,
                    metric_getProductionTileMap.TopLiquidChangeRate.Values[i].BaseProduction.Value, 1);
                Assert.AreEqual(us_getProductionTileMap.TopLiquidChangeRate.Values[i].ChangeRate.Value,
                    metric_getProductionTileMap.TopLiquidChangeRate.Values[i].ChangeRate.Value, 1);
                Assert.AreEqual(UnitsConversion("STB/d", us_getProductionTileMap.TopLiquidChangeRate.Values[i].CurrentProduction).Value,
                    metric_getProductionTileMap.TopLiquidChangeRate.Values[i].CurrentProduction.Value, 1);
                Assert.AreEqual(UnitsConversion("STB/d", us_getProductionTileMap.TopLiquidChangeRate.Values[i].ProductionChange).Value,
                    metric_getProductionTileMap.TopLiquidChangeRate.Values[i].ProductionChange.Value, 1);
            }
            //checking if % change Rate is same in both US and Metric
            Trace.WriteLine("checking if % change Rate is same in both US and Metric...");
            Assert.AreEqual(us_getProductionTileMap.TotalGasChangeRate, metric_getProductionTileMap.TotalGasChangeRate);
            Assert.AreEqual(us_getProductionTileMap.TotalOilChangeRate, metric_getProductionTileMap.TotalOilChangeRate);
            Assert.AreEqual(us_getProductionTileMap.TotalWaterChangeRate, metric_getProductionTileMap.TotalWaterChangeRate);
            Assert.AreEqual(us_getProductionTileMap.TotalLiquidChangeRate, metric_getProductionTileMap.TotalLiquidChangeRate);

            Assert.AreEqual(us_getProductionTileMap.TopGasChangeRate.Values.Sum(x => x.ChangeRate).Value, metric_getProductionTileMap.TotalGasChangeRate);
            Assert.AreEqual(us_getProductionTileMap.TopOilChangeRate.Values.Sum(x => x.ChangeRate).Value, metric_getProductionTileMap.TotalOilChangeRate);
            Assert.AreEqual(us_getProductionTileMap.TopWaterChangeRate.Values.Sum(x => x.ChangeRate).Value, metric_getProductionTileMap.TotalWaterChangeRate);
            Assert.AreEqual(us_getProductionTileMap.TopLiquidChangeRate.Values.Sum(x => x.ChangeRate).Value, metric_getProductionTileMap.TotalLiquidChangeRate);
            Trace.WriteLine("checking completed for Check Change =" + change);
        }
        /// <summary>
        /// Update Test for FRWM-6172 : API testing subtask for story : 
        /// FRWM-4220 : Production Overview Tile Maps - Allow displaying net change or percentage change & Add Gross
        /// </summary>
        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetProductionChangeTileMap()
        {
            try
            {
                AddWellsAndCalculateDailyAverage();
                Trace.WriteLine(" testing when there is Change in Percentage ");
                ValidateProductionChangeTileMap(true, true);
                Trace.WriteLine(" testing when there is No Change in  Percentage");
                ValidateProductionChangeTileMap(false, true);
                Trace.WriteLine(" testing when there is Change in Volume");
                ValidateProductionChangeTileMap(true, false);
                Trace.WriteLine(" testing when there is No Change in Volume");
                ValidateProductionChangeTileMap(false, false);
                Trace.WriteLine(" test was Complete ***");
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
            }
        }

        public void CheckCumulativeValues(TileMapFilterDTO tileMap, string startDate, string endDate, ProductionType prodType, string usKey, string metricKey)
        {
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
            WellCumulativeProductionAndUnitsDTO us_cumulativeProduction = SurveillanceService.GetWellDailyCumulativeData(tileMap, ((int)prodType).ToString(),
                    startDate, endDate);
            Assert.IsNotNull(us_cumulativeProduction, "Failed to get cumulative data for the available wells");
            Assert.IsNotNull(us_cumulativeProduction.Units, "Failed to get units in cumulative data for the available wells");
            Assert.IsNotNull(us_cumulativeProduction.Values, "Failed to get values in cumulative data for the available wells");
            Assert.AreEqual((int)prodType, (int)us_cumulativeProduction.Values.ProductionType);
            if (us_cumulativeProduction.Values.DailyCumulativeProduction.Count() > 0)
            {
                Assert.AreEqual(usKey, us_cumulativeProduction.Units.AverageProductionUnit);
                Assert.AreEqual(usKey, us_cumulativeProduction.Units.CumulativeProductionUnit);
                Assert.AreEqual(us_cumulativeProduction.Values.AverageCumulativeProduction,
               us_cumulativeProduction.Values.DailyCumulativeProduction.LastOrDefault().DailyCumulativeProduction / us_cumulativeProduction.Values.DailyCumulativeProduction.Count());
            }
            ChangeUnitSystem("Metric");
            ChangeUnitSystemUserSetting("Metric");
            WellCumulativeProductionAndUnitsDTO metric_cumulativeProduction = SurveillanceService.GetWellDailyCumulativeData(tileMap, ((int)prodType).ToString(),
                startDate, endDate);
            Assert.IsNotNull(metric_cumulativeProduction, "Failed to get cumulative data for the available wells");
            Assert.IsNotNull(metric_cumulativeProduction.Units, "Failed to get units in cumulative data for the available wells");
            Assert.IsNotNull(metric_cumulativeProduction.Values, "Failed to get values in cumulative data for the available wells");
            Assert.AreEqual((int)prodType, (int)metric_cumulativeProduction.Values.ProductionType);
            if (metric_cumulativeProduction.Values.DailyCumulativeProduction.Count() > 0)
            {
                Assert.AreEqual(metricKey, metric_cumulativeProduction.Units.AverageProductionUnit);
                Assert.AreEqual(metricKey, metric_cumulativeProduction.Units.CumulativeProductionUnit);
                Assert.AreEqual(metric_cumulativeProduction.Values.AverageCumulativeProduction,
                metric_cumulativeProduction.Values.DailyCumulativeProduction.LastOrDefault().DailyCumulativeProduction / metric_cumulativeProduction.Values.DailyCumulativeProduction.Count());
            }
            Assert.AreEqual(us_cumulativeProduction.Values.DailyCumulativeProduction.Count(), metric_cumulativeProduction.Values.DailyCumulativeProduction.Count());
            Assert.AreEqual(us_cumulativeProduction.Values.CumulativeTargetProduction.Count(), metric_cumulativeProduction.Values.CumulativeTargetProduction.Count());
            for (int i = 0; i < metric_cumulativeProduction.Values.CumulativeTargetProduction.Count(); i++)
            {
                Assert.AreEqual(UnitsConversion(usKey, us_cumulativeProduction.Values.CumulativeTargetProduction[i].DailyCumulativeProduction.Value).Value,
                    metric_cumulativeProduction.Values.CumulativeTargetProduction[i].DailyCumulativeProduction.Value, 1);
            }
            for (int i = 0; i < metric_cumulativeProduction.Values.DailyCumulativeProduction.Count(); i++)
            {
                Assert.AreEqual(UnitsConversion(usKey, us_cumulativeProduction.Values.DailyCumulativeProduction[i].DailyCumulativeProduction.Value).Value,
                    metric_cumulativeProduction.Values.DailyCumulativeProduction[i].DailyCumulativeProduction.Value, 1);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetWellCumulativeData()
        {
            try
            {
                AddWellsAndCalculateDailyAverage();
                TileMapFilterDTO tileMap = new TileMapFilterDTO();
                tileMap.WellFilter = new WellFilterDTO();
                tileMap.UsePercentChangeForRank = true;
                string startDate = DTOExtensions.ToISO8601(DateTime.Now.Date.AddDays(-7).ToUniversalTime());
                string endDate = DTOExtensions.ToISO8601(DateTime.Now.Date.AddMilliseconds(-10).ToUniversalTime());
                CheckCumulativeValues(tileMap, startDate, endDate, ProductionType.Oil, "STB", "sm3");
                CheckCumulativeValues(tileMap, startDate, endDate, ProductionType.Water, "STB", "sm3");
                CheckCumulativeValues(tileMap, startDate, endDate, ProductionType.Gas, "Mscf", "sm3");
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetDailyAverageQuantities_ESP()
        {
            DailyAverageQuantity[] dailyAvgQuantity = SurveillanceService.GetDailyAverageQuantities(((int)WellTypeId.ESP).ToString());
            Assert.IsNotNull(dailyAvgQuantity, "Failed to get the available daily average quantities");
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.RunTime));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.THP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.THT));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.CHP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.PIP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.PDP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.MotorAmps));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.MotorFrequency));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.MotorVolts));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.OilRateAllocated));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.OilRateInferred));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.GasRateAllocated));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.GasRateInferred));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.WaterRateAllocated));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.WaterRateInferred));
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetDailyAverageQuantities_GasLift()
        {
            DailyAverageQuantity[] dailyAvgQuantity = SurveillanceService.GetDailyAverageQuantities(((int)WellTypeId.GLift).ToString());
            Assert.IsNotNull(dailyAvgQuantity, "Failed to get the available daily average quantities");
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.RunTime));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.THP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.THT));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.FLP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.ChokeDiameter));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.CHP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.Qgi));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.DHPG));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.FBHP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.OilRateAllocated));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.OilRateInferred));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.GasRateAllocated));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.GasRateInferred));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.WaterRateAllocated));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.WaterRateInferred));
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetDailyAverageQuantities_NaturallyFlowing()
        {
            DailyAverageQuantity[] dailyAvgQuantity = SurveillanceService.GetDailyAverageQuantities(((int)WellTypeId.NF).ToString());
            Assert.IsNotNull(dailyAvgQuantity, "Failed to get the available daily average quantities");
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.RunTime));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.THP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.ChokeDiameter));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.FLP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.DHPG));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.FBHP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.OilRateAllocated));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.OilRateInferred));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.GasRateAllocated));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.GasRateInferred));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.WaterRateAllocated));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.WaterRateInferred));
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetDailyAverageQuantities_GInj()
        {
            DailyAverageQuantity[] dailyAvgQuantity = SurveillanceService.GetDailyAverageQuantities(((int)WellTypeId.GInj).ToString());
            Assert.IsNotNull(dailyAvgQuantity, "Failed to get the available daily average quantities");
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.RunTime));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.THP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.THT));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.CHP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.FLP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.DHPG));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.GasInjectionRate));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.GasInjectionVolumeYesterday));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.InjectionPressure));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.GasRateAllocated));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.GasRateInferred));
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetDailyAverageQuantities_WInj()
        {
            DailyAverageQuantity[] dailyAvgQuantity = SurveillanceService.GetDailyAverageQuantities(((int)WellTypeId.WInj).ToString());
            Assert.IsNotNull(dailyAvgQuantity, "Failed to get the available daily average quantities");
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.RunTime));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.THP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.THT));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.FLP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.CHP));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.DHPG));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.WaterInjectionRate));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.WaterInjectionVolumeYesterday));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.WaterRateAllocated));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.WaterRateInferred));
            Assert.IsTrue(dailyAvgQuantity.Contains(DailyAverageQuantity.InjectionPressure));
        }

        public decimal GetTruncatedValueforDecimal(decimal dblval, int count)
        {
            Trace.WriteLine("Decimal Value: " + dblval);
            decimal GetTruncatedValueforDecimal = 0m;
            System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex("^\\d+\\.?\\d{" + count + "}");
            System.Text.RegularExpressions.Match mt = re.Match(dblval.ToString());
            string strmth = mt.ToString();
            GetTruncatedValueforDecimal = Convert.ToDecimal(strmth);
            return GetTruncatedValueforDecimal;
        }

        public double GetTruncatedValueforDouble(double dblval, int count)
        {
            Trace.WriteLine("Double Value " + dblval);
            double GetTruncatedValueforDouble = 0.0;
            System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex("^\\d+\\.?\\d{" + count + "}");
            System.Text.RegularExpressions.Match mt = re.Match(dblval.ToString());
            string strmth = mt.ToString();
            GetTruncatedValueforDouble = Convert.ToDouble(strmth);
            return GetTruncatedValueforDouble;
        }

        private static int CountDigitsAfterDecimal(double value)
        {
            bool start = false;
            int count = 0;
            foreach (var s in value.ToString())
            {
                if (s == '.')
                {
                    start = true;
                }
                else if (start)
                {
                    count++;
                }
            }

            return count;
        }

        [TestMethod]
        public void GetRRLAutonomousControlHistory()
        {
            // First In CygNet Seed historical Values
            try
            {
                RRLAutonomousSeedHistoricalValuesInVHS();
                //Settings for RRL Idle Time Optimization
                //Desired Number of Cycles : 2
                //Incremental Idel Time change in Seconds : 300
                SetValuesInSystemSettings(SettingServiceStringConstants.DESIRED_NUMBER_OF_CYCLES, "2");
                SetValuesInSystemSettings(SettingServiceStringConstants.INCREMENTAL_IDLE_TIME_CHANGE_IN_SECONDS, "300");

                //Add 3 new RRL Wells and Try to Find out OptimizingIdleTime Opportunity
                for (int i = 1; i < 4; i++)
                {
                    //Add New RRL Wells
                    string facilityId1 = GetFacilityId("RPOC_", (i + 1));
                    AddRRLWell(facilityId1);
                    WellDTO[] wells = WellService.GetAllWells();
                    WellDTO well = wells.FirstOrDefault(w => w.Name.Equals(facilityId1));
                    _wellsToRemove.Add(well);
                    string WellId = well.Id.ToString();
                    Trace.WriteLine("Well Id is " + WellId);
                    //Determine if there is Optimizing RRL Idle Time Opportunity

                    bool optimizing = SurveillanceService.OptimizingRRLIdleTimeForSingleWell(WellId);
                    if (optimizing) //We can Optimize Idle Time
                    {
                        Trace.WriteLine(string.Format("Well with Well ID {0} has  OptimizingRRLIdleTime ", WellId));
                        //    RRLAutonomousControlHistoryDTO rrloutputdto = SurveillanceService.GetRRLAutonomousControlHistory(new RRLAutonomousControlHistoryInputDTO() { StartDateTime = DateTime.Today.ToUniversalTime(), EndDateTime = DateTime.Today.AddDays(-30).ToUniversalTime(), WellFilter = new WellFilterDTO { welFK_r_WellTypeValues = new[] { new WellFilterValueDTO { Value = WellTypeId.RRL.ToString() } } } });
                        RRLAutonomousControlHistoryInputDTO inptdto = new RRLAutonomousControlHistoryInputDTO()
                        {
                            StartDateTime = DateTime.Today.AddDays(-30).ToUniversalTime(),
                            EndDateTime = DateTime.Today.AddDays(1).ToUniversalTime(),
                            WellFilter = new WellFilterDTO
                            {
                                welFK_r_WellTypeValues = new[] {
                             new WellFilterValueDTO { Value = WellTypeId.RRL.ToString() } }
                            }
                        };

                        RRLAutonomousControlHistoryDTO rrloutputdto = SurveillanceService.GetRRLAutonomousControlHistory(inptdto);
                        Assert.IsNotNull(rrloutputdto, "The Autonomous Control DTO was Returned as Null..Possible causes CygNet Misconfigured..");
                        Assert.IsNotNull(rrloutputdto.Records, "The Autonomous Control DTO contains no records..Possible causes CygNet Misconfigured..");

                        Assert.IsTrue(rrloutputdto.Records.Length > 0, "RRL Autonomous Records count was Zero even when One Well was Optimized");
                        Assert.IsNotNull(rrloutputdto.CumulativeRuntimeChangePercentage, "Cumulative Runtime Change Percentage  was null");
                        Assert.IsNotNull(rrloutputdto.CumulativeRuntimeMonetaryGain, "Cumulative Runtime Monetary Gain  was null");
                        Assert.IsNotNull(rrloutputdto.InferredProductionChangePercentage, "Inferred Production Change Percentage  was null");
                        Assert.IsNotNull(rrloutputdto.InferredProductionMonetaryGain, "Inferred Production Monetary Gain  was null");
                        Assert.IsNotNull(rrloutputdto.TotalStrokesChangePercentage, "Total Strokes Change Percentage  was null");
                        Assert.IsNotNull(rrloutputdto.TotalStrokesMonetaryGain, "Total Strokes Monetary Gain  was null");
                    }
                    else
                    {
                        Trace.WriteLine(string.Format("Well with Well ID {0} has no OptimizingRRLIdleTime ", WellId));
                        Assert.Fail("Well Could Not be Optmized for Given Expected  Optimizing Conditions");
                    }
                }
                Trace.WriteLine("Removing the added Wells");

                //Remove Added Wells in Test Cleanup as WellDTO is added in RemoveList

                //Remove History from CygNet VHS for RRL Autonomous
            }
            catch (Exception e)
            {
                Assert.Fail($"test execution failed due to error :  {e.ToString()}");
            }
            finally
            {
                DeleteHistoricalValuesFromVHS();
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void DataReliabilityCheckWhenDerivingRunStatusForSingleWellTest()
        {
            //if (!s_isRunningInATS)
            //{
            //    Trace.WriteLine("Scheduler can only run on ATS VM. skipping the test on TeamCity");
            //    return;
            //}
            try
            {
                SetValuesInSystemSettings(SettingServiceStringConstants.RUN_STATUS_FROM_FLOW_RATE, "1");
                SetValuesInSystemSettings(SettingServiceStringConstants.IS_GAS_WELL, "0");
                SetValuesInSystemSettings(SettingServiceStringConstants.SAMPLE_DATA_SIZE, "30");
                SetValuesInSystemSettings(SettingServiceStringConstants.SAMPLE_DATA_FREQUENCY, "1.0");
                SetValuesInSystemSettings(SettingServiceStringConstants.EXPECTED_LIQUID_RATE, "2500.00");
                SetValuesInSystemSettings(SettingServiceStringConstants.MINIMUM_MEAN_PERCENTAGE_BASED_ON_VALID_WELL_TEST, "90");

                string facilityId = GetGLFacilityId(1);
                AddNonRRLWell(facilityId, WellTypeId.GLift, true, CalibrationMethodId.LFactor);

                WellDTO[] wells = WellService.GetAllWells();
                WellDTO well = wells.FirstOrDefault(w => w.Name.Equals(facilityId));
                _wellsToRemove.Add(well);
                string WellId = well.Id.ToString();

                //add new wellTestData
                var testDataDTO = new WellTestDTO
                {
                    WellId = well.Id,
                    AverageCasingPressure = 2200,
                    AverageTubingPressure = 600,
                    AverageTubingTemperature = 100,
                    Gas = 2204m,
                    GasGravity = 0.6722m,
                    producedGOR = 1160,
                    Oil = 1900,
                    OilGravity = 46.2415m,
                    SPTCode = 2,
                    SPTCodeDescription = "RepresentativeTest",
                    TestDuration = 3,
                    Water = 1900,
                    WaterGravity = (decimal)1.0239,
                    GaugePressure = (decimal)1610
                };

                testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() + TimeSpan.FromDays(3));
                WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
                WellTestDataService.AddWellTestData(new WellTestAndUnitsDTO(units, testDataDTO));
                var latestValidWellTest = WellTestDataService.GetLatestValidWellTestByWellId(WellId)?.Value;

                //run the well run status job with reliable data in VHS
                NonRRLSeedingHistoricalValuesInVHS(well.Id, new string[] { facilityId }, true);
                //RunAnalysisTaskScheduler("-runWellRunStatusDerivation");
                SurveillanceService.DerivingRunStatusForSingleWell(well.Id.ToString());
                System.Threading.Thread.Sleep(40 * 1000);

                AlarmTypeDTO[] alarmTypeList = AlarmService.GetAlarmTypes();
                var alarmTypeDescription = "Derived Well Run Status: Stopped";
                var alarmType = alarmTypeList.First(en => en.AlarmType == alarmTypeDescription);
                CurrentAlarmDTO alarmWithReliableData = AlarmService.GetActiveAlarmByWellIdAndAlarmType(WellId, alarmType.Id.ToString());

                Assert.IsNotNull(alarmWithReliableData);
                AlarmService.RemoveAlarmsByWellId(WellId);

                //run the well run status job with unreliable data in VHS
                NonRRLSeedingHistoricalValuesInVHS(well.Id, new string[] { facilityId }, false);

                //    RunAnalysisTaskScheduler("-runWellRunStatusDerivation");
                SurveillanceService.DerivingRunStatusForSingleWell(well.Id.ToString());
                CurrentAlarmDTO alarmWithUnreliableData = AlarmService.GetActiveAlarmByWellIdAndAlarmType(WellId, alarmType.Id.ToString());
                Assert.IsNull(alarmWithUnreliableData);
            }
            catch (Exception e)
            {
                Assert.Fail($"DataReliabilityCheckWhenDerivingRunStatusForSingleWellTest execution failed due to error :  {e.ToString()}");
            }
            finally
            {
                DeleteHistoricalValuesFromVHS();
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]

        public void RRLAutonomousFlowfromSchedular()
        {

            try
            {
                if (s_isRunningInATS == false)
                {
                    Trace.WriteLine(" test was skipped on Team City due to admin rights privilege  missing");
                    return;
                }
                RRLAutonomousSeedHistoricalValuesInVHS();
                //Settings for RRL Idle Time Optimization
                //Desired Number of Cycles : 2
                //Incremental Idle Time change in Seconds : 300
                SetValuesInSystemSettings(SettingServiceStringConstants.DESIRED_NUMBER_OF_CYCLES, "2");
                SetValuesInSystemSettings(SettingServiceStringConstants.INCREMENTAL_IDLE_TIME_CHANGE_IN_SECONDS, "300");
                string facilityId1 = GetFacilityId("RPOC_", 3);
                WellDTO well = AddRRLWell(facilityId1);
                _wellsToRemove.Add(well);
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                //Run for First Time 
                //  report on PDf Exported on build
                RunAnalysisTaskScheduler("-runAutonomousControlRRL");
                Thread.Sleep(40 * 1000);
                RRLAutonomousControlHistoryInputDTO inptdto = new RRLAutonomousControlHistoryInputDTO()
                {
                    StartDateTime = DateTime.Today.AddDays(-30).ToUniversalTime(),
                    EndDateTime = DateTime.Today.AddDays(1).ToUniversalTime(),
                    WellFilter = new WellFilterDTO
                    {
                        welFK_r_WellTypeValues = new[] {
                             new WellFilterValueDTO { Value = WellTypeId.RRL.ToString() } }
                    }
                };

                RRLAutonomousControlHistoryDTO rrloutputdto = SurveillanceService.GetRRLAutonomousControlHistory(inptdto);
                int firstrec = rrloutputdto.Records.Length;
                Assert.IsNotNull(rrloutputdto, "The Autonomous Control DTO was Returned as Null..Possible causes CygNet Misconfigured..");
                Assert.IsNotNull(rrloutputdto.Records, "The Autonomous Control DTO contains no records..Possible causes CygNet Misconfigured..");
                Assert.IsTrue(rrloutputdto.Records.Length > 0, "RRL Autonomous Records count was Zero even when One Well was Optimized");
                Assert.IsNotNull(rrloutputdto.CumulativeRuntimeChangePercentage, "Cumulative Runtime Change Percentage  was null");
                Trace.WriteLine($"RRL Autonmous Values for donut chart : CumulativeRuntimeChangePercentage : {rrloutputdto.CumulativeRuntimeChangePercentage} ");
                Assert.IsNotNull(rrloutputdto.CumulativeRuntimeMonetaryGain, "Cumulative Runtime Monetary Gain  was null");
                Trace.WriteLine($"RRL Autonmous Values for donut chart : CumulativeRuntimeMonetaryGain : {rrloutputdto.CumulativeRuntimeMonetaryGain} ");
                Trace.WriteLine($"RRL Autonmous Values for donut chart : InferredProductionMonetaryGain : {rrloutputdto.InferredProductionMonetaryGain} ");
                Assert.IsNotNull(rrloutputdto.InferredProductionChangePercentage, "Inferred Production Change Percentage  was null");
                Trace.WriteLine($"RRL Autonmous Values for donut chart : InferredProductionChangePercentage : {rrloutputdto.InferredProductionChangePercentage} ");
                Assert.IsNotNull(rrloutputdto.InferredProductionMonetaryGain, "Inferred Production Monetary Gain  was null");
                Assert.IsNotNull(rrloutputdto.TotalStrokesChangePercentage, "Total Strokes Change Percentage  was null");
                Trace.WriteLine($"RRL Autonmous Values for donut chart : TotalStrokesChangePercentage : {rrloutputdto.TotalStrokesChangePercentage} ");
                Assert.IsNotNull(rrloutputdto.TotalStrokesMonetaryGain, "Total Strokes Monetary Gain  was null");
                Trace.WriteLine($"RRL Autonmous Values for donut chart : TotalStrokesMonetaryGain : {rrloutputdto.TotalStrokesMonetaryGain} ");
                Trace.WriteLine(" Assertions were passed till first Scheduler Run ");
                //Run for Second Time 
                RunAnalysisTaskScheduler("-runAutonomousControlRRL");
                Thread.Sleep(40 * 1000);
                //update RRL Autonomous Record value for increment 2
                rrloutputdto = SurveillanceService.GetRRLAutonomousControlHistory(inptdto);
                int sectrec = rrloutputdto.Records.Length;
                if (sectrec != firstrec + 1)
                {
                    //We are still not finding updated  record in VHS for some reason give it a second try 
                    Thread.Sleep(40 * 1000);
                    rrloutputdto = SurveillanceService.GetRRLAutonomousControlHistory(inptdto);

                }
                Assert.AreEqual(sectrec, firstrec + 1, "Records did not increment ");
                Trace.WriteLine(" Records count was incremented on second run ");
                long idletimeinc = rrloutputdto.Records.OrderByDescending(x => x.RecordDateTime).FirstOrDefault().NewIdleTimeInSeconds;
                long idletimeold = rrloutputdto.Records.OrderByDescending(x => x.RecordDateTime).FirstOrDefault().OrigianlIdleTimeInSeconds;
                Assert.AreEqual(idletimeinc, idletimeold + 300, "Idle Time Was not  incremented correctly");
                Trace.WriteLine($"Idle Time Was   incremented correctly.. Original Idle Time : {idletimeold} and new Idle Time {idletimeinc}");
                //Make sure that Idle Time is 


            }
            catch (Exception e)
            {
                Assert.Fail($"test execution failed due to error :  {e.ToString()}");
            }
            finally
            {
                DeleteHistoricalValuesFromVHS();
            }
        }

        public void RRLAutonomousSeedHistoricalValuesInVHS()
        {
            for (int i = 1; i < 4; i++)
            {
                //Get Facility from CygNet
                string facilityId = GetFacilityId("RPOC_", (i + 1));
                // Use the required 4 UDCS to whihc we need to force Values in VHS
                // Run Time Yestrday reduce by 500 secs ,
                // Strokes Yestrday reduce by 100
                //Cycles Yetrday Keep Constant non zero values
                //Infered Production Yestrday // increase by 3 bbl
                string[] UDCs = { "TMRUNYD", "RRLSTKYEST", "RRLCYCYEST", "QTLIQYD" };

                List<string> facUDCs = new List<string>();
                foreach (var udc in UDCs)
                {
                    facUDCs.Add(facilityId + "." + udc);
                }
                // Creatling a List for UDCS for single
                object input = facUDCs.Select(t => t as object).ToArray();
                object pointIdsList = null;
                List<Tuple<string, string>> UDCNameIDPair = new List<Tuple<string, string>>();

                try
                {
                    Trace.WriteLine($"SeedHistoricalValuesInVHS: Going to create CVS Client for [{s_domain}]{s_site}.{s_cvsService}.");
                    var cvsClient = CreateCvsClient();
                    cvsClient.ResolvePoints(ref input, out pointIdsList);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"SeedHistoricalValuesInVHS: CvsClient.ResolvePoints failed: {ex.Message}.");
                }

                int itr = 0;
                foreach (object d in ((object[])input))
                {
                    UDCNameIDPair.Add(new Tuple<string, string>(d.ToString(), ((object[])pointIdsList)[itr].ToString()));
                    itr++;
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
                    string udcname = "";
                    foreach (var tpl in UDCNameIDPair)
                    {
                        if (name.ID.Contains(tpl.Item2))
                        {
                            udcname = tpl.Item1.Replace(facilityId, "");
                            udcname = udcname.Replace(".", "");
                            break;
                        }
                    }
                    //ensure We write Values for last three days (3) at 00:00 hours for Each UDC
                    var histEntries = GetHistoricalEntries(25, 3, udcname);
                    _entriesByName[name] = histEntries.ToList();
                }
                // Storing Values in VHS on CygNet
                foreach (var name in _entriesByName.Keys)
                {
                    Trace.WriteLine($"SeedHistoricalValuesInVHS: Seeding values in VHS for {name.ID} point.");
                    try
                    {
                        _vhsClient.StoreHistoricalEntries(name, _entriesByName[name]);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"SeedHistoricalValuesInVHS: Failed to seed VHS values for {name} point: {ex.Message}.");
                    }
                }

                // DeleteHistoricalValuesFromVHS();
            }
        }

        public void NonRRLSeedingHistoricalValuesInVHS(long wellId, IList<string> facIds, bool reliable)
        {
            WellSettingDTO sampleDataSize = SettingService.GetWellSettingsByWellIdAndCategory(wellId.ToString(), SettingCategory.DerivedRunStatus.ToString()).First(en => en.Setting.Name == SettingServiceStringConstants.SAMPLE_DATA_SIZE);
            WellSettingDTO sampleDataFrequency = SettingService.GetWellSettingsByWellIdAndCategory(wellId.ToString(), SettingCategory.DerivedRunStatus.ToString()).First(en => en.Setting.Name == SettingServiceStringConstants.SAMPLE_DATA_FREQUENCY);
            foreach (var facId in facIds)
            {
                string[] UDCs = { "ROIL", "RH2O" };

                List<string> facUDCs = new List<string>();
                foreach (var udc in UDCs)
                {
                    facUDCs.Add(facId + "." + udc);
                }

                // Creating a List for UDCS for single
                object input = facUDCs.Select(t => t as object).ToArray();
                object pointIdsList = null;
                List<Tuple<string, string>> UDCNameIDPair = new List<Tuple<string, string>>();

                try
                {
                    Trace.WriteLine($"NonRRLSeedingHistoricalValuesInVHS: Going to create CVS Client for [{s_domain}]{s_site}.{s_cvsService}.");
                    var cvsClient = CreateCvsClient();
                    cvsClient.ResolvePoints(ref input, out pointIdsList);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"NonRRLSeedingHistoricalValuesInVHS: CvsClient.ResolvePoints failed: {ex.Message}.");
                }

                int itr = 0;
                foreach (object d in ((object[])input))
                {
                    UDCNameIDPair.Add(new Tuple<string, string>(d.ToString(), ((object[])pointIdsList)[itr].ToString()));
                    itr++;
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
                    string udcname = "";
                    foreach (var tpl in UDCNameIDPair)
                    {
                        if (name.ID.Contains(tpl.Item2))
                        {
                            udcname = tpl.Item1.Replace(facId, "");
                            udcname = udcname.Replace(".", "");
                            break;
                        }
                    }
                    //ensure we write values for last three days (3) at 00:00 hours for Each UDC
                    var histEntries = CreateNonRRLHistoricalEntries(sampleDataFrequency.NumericValue.Value, Convert.ToInt32(sampleDataSize.NumericValue.Value), udcname, reliable);
                    _entriesByName[name] = histEntries.ToList();
                }
                // Storing Values in VHS on CygNet
                foreach (var name in _entriesByName.Keys)
                {
                    Trace.WriteLine($"NonRRLSeedingHistoricalValuesInVHS: Seeding values in VHS for {name.ID} point.");
                    try
                    {
                        _vhsClient.StoreHistoricalEntries(name, _entriesByName[name]);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"NonRRLSeedingHistoricalValuesInVHS: Failed to seed VHS values for {name} point: {ex.Message}.");
                    }
                }
            }
        }


        private static IEnumerable<HistoricalEntry> GetHistoricalEntries(double baseValue, int numofdays, string udcname)
        {
            // Create entries spreaded in a span of numofdays days
            List<HistoricalEntry> entries = new List<HistoricalEntry>();
            double TMRUNYDval = 24000;
            double RRLSTKYESTval = 62000;
            double RRLCYCYESTval = 5;
            double QTLIQYDval = 109;

            DateTime today = DateTime.Today;
            for (int i = 0; i < numofdays; i++)
            {
                RRLSTKYESTval = RRLSTKYESTval + 1000.00;
                TMRUNYDval = TMRUNYDval + 500;
                QTLIQYDval = QTLIQYDval - 3;
                switch (udcname.ToUpper())
                {
                    case "TMRUNYD":
                        {
                            HistoricalEntry entry = new HistoricalEntry();
                            entry.ValueType = HistoricalEntryValueType.UTF8;
                            entry.SetValue((TMRUNYDval).ToString());
                            entry.Timestamp = today.AddDays(-i);
                            entry.BaseStatus = CygNet.Data.Core.BaseStatusFlags.Initialized | CygNet.Data.Core.BaseStatusFlags.Updated;
                            entries.Add(entry);
                            break;
                        }
                    case "RRLSTKYEST":
                        {
                            HistoricalEntry entry = new HistoricalEntry();
                            entry.ValueType = HistoricalEntryValueType.UTF8;
                            entry.SetValue((RRLSTKYESTval).ToString());
                            entry.Timestamp = today.AddDays(-i);
                            entry.BaseStatus = CygNet.Data.Core.BaseStatusFlags.Initialized | CygNet.Data.Core.BaseStatusFlags.Updated;
                            entries.Add(entry);
                            break;
                        }
                    case "RRLCYCYEST":
                        {
                            baseValue = 24000;
                            HistoricalEntry entry = new HistoricalEntry();
                            entry.ValueType = HistoricalEntryValueType.UTF8;
                            entry.SetValue(RRLCYCYESTval.ToString());
                            entry.Timestamp = today.AddDays(-i);
                            entry.BaseStatus = CygNet.Data.Core.BaseStatusFlags.Initialized | CygNet.Data.Core.BaseStatusFlags.Updated;
                            entries.Add(entry);
                            break;
                        }
                    case "QTLIQYD":
                        {
                            baseValue = 100;
                            HistoricalEntry entry = new HistoricalEntry();
                            entry.ValueType = HistoricalEntryValueType.UTF8;
                            entry.SetValue(QTLIQYDval.ToString());
                            entry.Timestamp = today.AddDays(-i);
                            entry.BaseStatus = CygNet.Data.Core.BaseStatusFlags.Initialized | CygNet.Data.Core.BaseStatusFlags.Updated;
                            entries.Add(entry);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            return entries;
        }

        private static IEnumerable<HistoricalEntry> CreateNonRRLHistoricalEntries(double frequency, int numberOfDataPoints, string udcName, bool reliable)
        {
            List<HistoricalEntry> entries = new List<HistoricalEntry>();
            var oilRate = 756.0;
            var waterRate = 452.0;
            if (frequency <= 0)
            {
                frequency = 1.0;
            }
            var interval = 1.0 / frequency;
            var duration = numberOfDataPoints * interval;

            DateTime starting = DateTime.Now - TimeSpan.FromSeconds(duration);
            for (int i = 0; i < numberOfDataPoints; i++)
            {
                oilRate += 0.1;
                waterRate -= 0.1;
                starting += TimeSpan.FromSeconds(interval);
                switch (udcName.ToUpper())
                {
                    case "ROIL":
                        {
                            HistoricalEntry entry = new HistoricalEntry();
                            if (reliable)
                            {
                                entry.BaseStatus = CygNet.Data.Core.BaseStatusFlags.Initialized | CygNet.Data.Core.BaseStatusFlags.Updated;
                            }
                            else
                            {
                                entry.BaseStatus = BaseStatusFlags.Unreliable;
                            }
                            entry.ValueType = HistoricalEntryValueType.UTF8;
                            entry.SetValue((oilRate).ToString());
                            entry.Timestamp = starting;

                            entries.Add(entry);
                            break;
                        }
                    case "RH2O":
                        {
                            HistoricalEntry entry = new HistoricalEntry();
                            if (reliable)
                            {
                                entry.BaseStatus = CygNet.Data.Core.BaseStatusFlags.Initialized | CygNet.Data.Core.BaseStatusFlags.Updated;
                            }
                            else
                            {
                                entry.BaseStatus = BaseStatusFlags.Unreliable;
                            }
                            entry.ValueType = HistoricalEntryValueType.UTF8;
                            entry.SetValue((waterRate).ToString());
                            entry.Timestamp = starting;
                            entries.Add(entry);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            return entries;
        }
        private static DomainSiteService GetDomainSiteServiceForSurveillance(string service)
        {
            var dss = new CygNet.Data.Core.DomainSiteService(Convert.ToUInt16(s_domain), s_site, service);
            Trace.WriteLine($"DSS for {service} is [{dss.DomainId}]{dss.SiteService}");
            return dss;
        }

        private static void DeleteHistoricalValuesFromVHS()
        {
            foreach (var name in _entriesByName.Keys)
            {
                try
                {
                    Trace.WriteLine($"Deleting seeded values for {name.ID} from {_vhsClient.SiteService}.");
                    var entries = _entriesByName[name];
                    foreach (var item in entries)
                    {
                        object cvsValue = null;
                        if (item.ValueType == HistoricalEntryValueType.Double)
                            cvsValue = item.GetValueAsDouble();
                        else if (item.ValueType == HistoricalEntryValueType.Int64)
                            cvsValue = item.GetValueAsInt64();
                        else if (item.ValueType == HistoricalEntryValueType.UTF8)
                            cvsValue = item.GetValueAsString();
                        Trace.WriteLine(string.Format("UDC name {0} ,CVS time stamp to delete: {1}", name.ID.ToString(), item.Timestamp.ToLocalTime().ToString()));
                        Trace.WriteLine($"CVS Value: {cvsValue}.");
                    }
                    _vhsClient.DeleteHistoricalEntries(name, _entriesByName[name], true, new System.Threading.CancellationToken());
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Failed to delete seeded values for {name.ID} from {_vhsClient.SiteService}: {ex.Message}.");
                }
            }
        }

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
                Trace.WriteLine($"Failed to connect to {cvsDomainSiteSvc.GetDomainSiteService()}: {ex.Message}");
                Trace.WriteLine($"Failed to connect to {cvsDomainSiteSvc.GetDomainSiteService()}: {ex.ToString()}");
            }

            if (!cvsClient.IsConnected)
            {
                Trace.WriteLine($"Failed to connect to {cvsDomainSiteSvc.GetDomainSiteService()}.");
            }

            return cvsClient;
        }

        //This method will create plunger lift well with model file
        public WellConfigDTO AddPlungerLiftWellWithModel(string passFacilityId, string modelFileName)
        {
            string passFacilityId1 = GetFacilityId(passFacilityId, 1);

            WellDTO well = SetDefaultFluidType(new WellDTO()
            {
                Name = passFacilityId1,
                FacilityId = passFacilityId1,
                WellType = WellTypeId.PLift,
                DataConnection = GetDefaultCygNetDataConnection(),
                CommissionDate = DateTime.Today.AddYears(-2),
                AssemblyAPI = passFacilityId1,
                SubAssemblyAPI = passFacilityId1,
                IntervalAPI = passFacilityId1,
                GasAllocationGroup = null,
                OilAllocationGroup = null,
                WaterAllocationGroup = null
            });

            //Creating well with null ModelConfig
            WellConfigDTO wellConfig = new WellConfigDTO();
            wellConfig.Well = well;
            wellConfig.ModelConfig = null;

            //Well
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfig);
            Assert.IsNotNull(addedWellConfig, "Failed to Add Well");
            Assert.IsNotNull(addedWellConfig.Well, "Failed to Add Well");
            Assert.IsNotNull(addedWellConfig.Well.Id, "Failed to Add Well");
            _wellsToRemove.Add(addedWellConfig.Well);

            //Model file is placed at following path
            string path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";

            byte[] fileAsByteArray;
            ModelFileValidationDataDTO ModelFileValidationData;
            ModelFileOptionDTO options = new ModelFileOptionDTO();
            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };
            modelFile.ApplicableDate = well.CommissionDate.Value.AddDays(1).ToUniversalTime();
            modelFile.WellId = addedWellConfig.Well.Id;

            fileAsByteArray = GetByteArray(path, modelFileName);
            options.Comment = "PLift";
            options.OptionalUpdate = new long[] { };
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            modelFile.Options = options;
            ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
            if (ModelFileValidationData != null)
                ModelFileService.AddWellModelFile(modelFile);
            else
                Trace.WriteLine(string.Format("Failed to validate PL model file"));

            return WellConfigurationService.GetWellConfig(addedWellConfig.Well.Id.ToString());
        }

        //Test Method is written for verify PCP Well Operating Limit
        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void PCPWellOperatingLimit()
        {
            string facilityId;
            if (s_isRunningInATS)
                facilityId = "WFTA1K_00001";
            else
                facilityId = "WFTA1K_0001";

            // Set system to US units
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
            Trace.WriteLine("System set for US unit system");

            try
            {
                //Adding Well along with WellTest Data
                WellDTO pcpWell = AddNonRRLWell(facilityId, WellTypeId.PCP, false, CalibrationMethodId.ReservoirPressure);

                //Adding Operating Limit
                WellSettingDTO[] operatingLimitUS = AddOperatingLimit(WellTypeId.PCP, pcpWell.Id);

                Assert.AreEqual(101, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Motor Amps Operating Limit").NumericValue, "Min Amp Mismatched");
                Assert.AreEqual(401, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Motor Amps Operating Limit").NumericValue, "Max Amp Mismatched");

                Assert.AreEqual(102, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Pump Discharge Pressure Operating Limit").NumericValue, "Min Pump Discharge Pressure Mismatched");
                Assert.AreEqual(402, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Pump Discharge Pressure Operating Limit").NumericValue, "Max Pump Discharge Pressure Mismatched");

                Assert.AreEqual(103, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Pump Intake Pressure Operating Limit").NumericValue, "Min Pump Intake Pressure Mismatched");
                Assert.AreEqual(403, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Pump Intake Pressure Operating Limit").NumericValue, "Max Pump Intake Pressure Mismatched");

                Assert.AreEqual(104, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Casing Head Pressure Operating Limit").NumericValue, "Min Casing Head Pressure Mismatched");
                Assert.AreEqual(404, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Casing Head Pressure Operating Limit").NumericValue, "Max Casing Head Pressure Mismatched");

                Assert.AreEqual(105, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Flow Line Pressure Operating Limit").NumericValue, "Min Flow Line Pressure Mismatched");
                Assert.AreEqual(405, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Flow Line Pressure Operating Limit").NumericValue, "Max Flow Line Pressure Mismatched");

                Assert.AreEqual(106, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Tubing Head Pressure Operating Limit").NumericValue, "Min Tubing Head Pressure Mismatched");
                Assert.AreEqual(406, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Tubing Head Pressure Operating Limit").NumericValue, "Max Tubing Head Pressure Mismatched");

                Assert.AreEqual(107, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Water Rate Operating Limit").NumericValue, "Min Water Rate Mismatched");
                Assert.AreEqual(407, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Water Rate Operating Limit").NumericValue, "Max Water Rate Mismatched");

                Assert.AreEqual(108, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Oil Rate Operating Limit").NumericValue, "Min Oil Rate Mismatched");
                Assert.AreEqual(408, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Oil Rate Operating Limit").NumericValue, "Max Oil Rate Mismatched");

                Assert.AreEqual(109, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Speed Operating Limit").NumericValue, "Min Speed Mismatched");
                Assert.AreEqual(409, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Speed Operating Limit").NumericValue, "Max Speed Mismatched");

                Assert.AreEqual(110, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Tubing Head Temperature Operating Limit").NumericValue, "Min Tubing Head Temperature Mismatched");
                Assert.AreEqual(410, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Tubing Head Temperature Operating Limit").NumericValue, "Max Tubing Head Temperature Mismatched");

                Assert.AreEqual(111, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Pump Torque Operating Limit").NumericValue, "Min Flow Line Pressure Mismatched");
                Assert.AreEqual(411, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Pump Torque Operating Limit").NumericValue, "Max Flow Line Pressure Mismatched");

                Assert.AreEqual(112, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Motor Volts Operating Limit").NumericValue, "Min Motor Volts Mismatched");
                Assert.AreEqual(412, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Motor Volts Operating Limit").NumericValue, "Max Motor Volts Mismatched");

                Assert.AreEqual(13, operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Run Time Operating Limit").NumericValue, "Min RunTime Mismatched");

                // Set system to US units
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");
                Trace.WriteLine("System set for Metric unit system");

                //Get Entered Operating Limit and validating it
                WellSettingDTO[] operatingLimitMetric = AddOperatingLimit(WellTypeId.PCP, pcpWell.Id);

                Assert.AreEqual(101, operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Min Motor Amps Operating Limit").NumericValue, "Min Amp Mismatched");
                Assert.AreEqual(401, operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Max Motor Amps Operating Limit").NumericValue, "Max Amp Mismatched");

                Assert.AreEqual((102 * 6.894757), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Min Pump Discharge Pressure Operating Limit").NumericValue, "Min Pump Discharge Pressure Mismatched");
                Assert.AreEqual((402 * 6.894757), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Max Pump Discharge Pressure Operating Limit").NumericValue, "Max Pump Discharge Pressure Mismatched");

                Assert.AreEqual((103 * 6.894757), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Min Pump Intake Pressure Operating Limit").NumericValue, "Min Pump Intake Pressure Mismatched");
                Assert.AreEqual((403 * 6.894757), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Max Pump Intake Pressure Operating Limit").NumericValue, "Max Pump Intake Pressure Mismatched");

                Assert.AreEqual((104 * 6.894757), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Min Casing Head Pressure Operating Limit").NumericValue, "Min Casing Head Pressure Mismatched");
                Assert.AreEqual((404 * 6.894757), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Max Casing Head Pressure Operating Limit").NumericValue, "Max Casing Head Pressure Mismatched");

                Assert.AreEqual((105 * 6.894757), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Min Flow Line Pressure Operating Limit").NumericValue, "Min Flow Line Pressure Mismatched");
                Assert.AreEqual((405 * 6.894757), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Max Flow Line Pressure Operating Limit").NumericValue, "Max Flow Line Pressure Mismatched");

                Assert.AreEqual((106 * 6.894757), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Min Tubing Head Pressure Operating Limit").NumericValue, "Min Tubing Head Pressure Mismatched");
                Assert.AreEqual((406 * 6.894757), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Max Tubing Head Pressure Operating Limit").NumericValue, "Max Tubing Head Pressure Mismatched");

                Assert.AreEqual((107 * 0.1589873), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Min Water Rate Operating Limit").NumericValue, "Min Water Rate Mismatched");
                Assert.AreEqual((407 * 0.1589873), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Max Water Rate Operating Limit").NumericValue, "Max Water Rate Mismatched");

                Assert.AreEqual((108 * 0.1589873), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Min Oil Rate Operating Limit").NumericValue, "Min Oil Rate Mismatched");
                Assert.AreEqual((408 * 0.1589873), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Max Oil Rate Operating Limit").NumericValue, "Max Oil Rate Mismatched");

                Assert.AreEqual(109, operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Min Speed Operating Limit").NumericValue, "Min Speed Mismatched");
                Assert.AreEqual(409, operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Max Speed Operating Limit").NumericValue, "Max Speed Mismatched");

                Assert.AreEqual(((110 - 32) * 0.55555555555555555555555555555556), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Min Tubing Head Temperature Operating Limit").NumericValue, "Min Tubing Head Temperature Mismatched");
                Assert.AreEqual(((410 - 32) * 0.55555555555555555555555555555556), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Max Tubing Head Temperature Operating Limit").NumericValue, "Max Tubing Head Temperature Mismatched");

                Assert.AreEqual((111 * 1.355818), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Min Pump Torque Operating Limit").NumericValue, "Min Flow Line Pressure Mismatched");
                Assert.AreEqual((411 * 1.355818), operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Max Pump Torque Operating Limit").NumericValue, "Max Flow Line Pressure Mismatched");

                Assert.AreEqual(112, operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Min Motor Volts Operating Limit").NumericValue, "Min Motor Volts Mismatched");
                Assert.AreEqual(412, operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Max Motor Volts Operating Limit").NumericValue, "Max Motor Volts Mismatched");

                Assert.AreEqual(13, operatingLimitMetric.FirstOrDefault(x => x.Setting.Name == "Min Run Time Operating Limit").NumericValue, "Min RunTime Mismatched");
            }

            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                RemoveWell(facilityId);
            }
        }

        //Test Method is written for PCP  - Operating Limits Overide - ForeSIte Alams
        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void PCPForeSiteAlarmForOperatingLimit()
        {
            string facilityId = GetFacilityId("WFTA1K_", 1);
            try
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");

                //Adding Well along with WellTest Data
                WellDTO pcpWell = AddNonRRLWell(facilityId, WellTypeId.PCP, false, CalibrationMethodId.ReservoirPressure);

                AddWellSettingWithDoubleValues(pcpWell.Id, "Min Reservoir Pressure Acceptance Limit", 21.5);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max Reservoir Pressure Acceptance Limit", 2100.5);

                AddWellSettingWithDoubleValues(pcpWell.Id, "Min Water Cut Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max Water Cut Acceptance Limit", 1.0);

                AddWellSettingWithDoubleValues(pcpWell.Id, "Min GOR Acceptance Limit", 1.0);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max GOR Acceptance Limit", 50.0);

                WellTestDTO latestTestData_PCP = WellTestDataService.GetLatestWellTestDataByWellId(pcpWell.Id.ToString());
                WellTestDataService.TuneSelectedWellTests(new long[] { latestTestData_PCP.Id });

                WellTestDTO latestTestDataAfterTune_PCP = WellTestDataService.GetLatestWellTestDataByWellId(pcpWell.Id.ToString());
                Assert.AreEqual("TUNING_SUCCEEDED", latestTestDataAfterTune_PCP.Status.ToString(), "Well Test Status is not Success");

                //Adding Operating Limit
                WellSettingDTO[] operatingLimitUS = AddOperatingLimit(WellTypeId.PCP, pcpWell.Id);
                var minMAmp = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Motor Amps Operating Limit").NumericValue;
                var maxMAmp = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Motor Amps Operating Limit").NumericValue;

                var minPDP = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Pump Discharge Pressure Operating Limit").NumericValue;
                var maxPDP = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Pump Discharge Pressure Operating Limit").NumericValue;

                var minPIP = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Pump Intake Pressure Operating Limit").NumericValue;
                var maxPIP = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Pump Intake Pressure Operating Limit").NumericValue;

                var minCHP = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Casing Head Pressure Operating Limit").NumericValue;
                var maxCHP = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Casing Head Pressure Operating Limit").NumericValue;

                var minFLP = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Flow Line Pressure Operating Limit").NumericValue;
                var maxFLP = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Flow Line Pressure Operating Limit").NumericValue;

                var minTHP = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Tubing Head Pressure Operating Limit").NumericValue;
                var maxTHP = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Tubing Head Pressure Operating Limit").NumericValue;

                var minWRate = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Water Rate Operating Limit").NumericValue;
                var maxWRate = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Water Rate Operating Limit").NumericValue;

                var minORate = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Oil Rate Operating Limit").NumericValue;
                var maxORate = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Oil Rate Operating Limit").NumericValue;

                var minPSpeed = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Speed Operating Limit").NumericValue;
                var maxPSpeed = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Speed Operating Limit").NumericValue;

                var minTHT = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Tubing Head Temperature Operating Limit").NumericValue;
                var maxTHT = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Tubing Head Temperature Operating Limit").NumericValue;

                var minPTorque = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Pump Torque Operating Limit").NumericValue;
                var maxPTorque = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Pump Torque Operating Limit").NumericValue;

                var minMVolt = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Motor Volts Operating Limit").NumericValue;
                var maxMVolt = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Max Motor Volts Operating Limit").NumericValue;

                //var minRuntime = operatingLimitUS.FirstOrDefault(x => x.Setting.Name == "Min Run Time Operating Limit").NumericValue;


                //Getting Well Staus in US System
                PCPWellStatusValueDTO us_pcpWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(pcpWell.Id.ToString(),
                                                         SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as PCPWellStatusValueDTO;
                Assert.IsNotNull(us_pcpWellStatus, "Failed to get Well status in 'US' units");

                var cMAmp = us_pcpWellStatus.MotorAmps;
                var cPDP = us_pcpWellStatus.PumpDischargePressure;
                var cPIP = us_pcpWellStatus.PumpIntakePressure;
                var cCHP = us_pcpWellStatus.CasingPressure;
                var cFLP = us_pcpWellStatus.FlowLinePressure;
                var cTHP = us_pcpWellStatus.TubingPressure;
                var cTHT = us_pcpWellStatus.TubingHeadTemperature;
                var cWRate = us_pcpWellStatus.WaterRateMeasured;
                var cORate = us_pcpWellStatus.OilRateMeasured;
                var cPSpeed = us_pcpWellStatus.PumpSpeed;
                var cPTorque = us_pcpWellStatus.PumpTorque;
                var cMVolt = us_pcpWellStatus.MotorVolts;

                //Comparing operating limit with latest plunger cycle data
                int cnt = 0;
                if ((double)cMAmp < minMAmp || (double)cMAmp > maxMAmp)
                {
                    cnt = cnt + 1;
                    Trace.WriteLine("Min Motor Amp: " + minMAmp + "Max Motor Amp:" + maxMAmp + "C Motor Amp:" + cMAmp);
                }
                if ((double)cMVolt < minMVolt || (double)cMVolt > maxMVolt)
                {
                    cnt = cnt + 1;
                    Trace.WriteLine("Min Motor Volt: " + minMVolt + "Max Motor Volt:" + maxMVolt + "C Motor Volt:" + cMVolt);
                }
                if ((double)cPDP < minPDP || (double)cPDP > maxPDP)
                {
                    cnt = cnt + 1;
                    Trace.WriteLine("Min PDP: " + minPDP + "Max PDP:" + maxPDP + "C PDP:" + cPDP);
                }
                if ((double)cPIP < minPIP || (double)cPIP > maxPIP)
                {
                    cnt = cnt + 1;
                    Trace.WriteLine("Min PIP: " + minPIP + "Max PIP:" + maxPIP + "C PIP:" + cPIP);
                }
                if ((double)cFLP < minFLP || (double)cFLP > maxFLP)
                {
                    cnt = cnt + 1;
                    Trace.WriteLine("Min FLP: " + minFLP + "Max FLP:" + maxFLP + "C FLP:" + cFLP);
                }
                if ((double)cTHP < minTHP || (double)cTHP > maxTHP)
                {
                    cnt = cnt + 1;
                    Trace.WriteLine("Min THP: " + minTHP + "Max THP:" + maxTHP + "C THP:" + cTHP);
                }
                if ((double)cCHP < minCHP || (double)cCHP > maxCHP)
                {
                    cnt = cnt + 1;
                    Trace.WriteLine("Min CHP: " + minCHP + "Max CHP:" + maxCHP + "C CHP:" + cCHP);
                }
                if ((double)cORate < minORate || (double)cORate > maxORate)
                {
                    cnt = cnt + 1;
                    Trace.WriteLine("Min Oil Rate: " + minORate + "Max Oil Rate:" + maxORate + "C Oil Rate:" + cORate);
                }
                if ((double)cWRate < minWRate || (double)cORate > maxWRate)
                {
                    cnt = cnt + 1;
                    Trace.WriteLine("Min Water Rate: " + minWRate + "Max Water Rate:" + maxWRate + "C Water Rate:" + cWRate);
                }
                if ((double)cPSpeed < minPSpeed || (double)cPSpeed > maxPSpeed)
                {
                    cnt = cnt + 1;
                    Trace.WriteLine("Min Pump Speed: " + minPSpeed + "Max Pump Speed:" + maxPSpeed + "C Pump Speed:" + cPSpeed);
                }
                if ((double)cPTorque < minPTorque || (double)cPTorque > maxPTorque)
                {
                    cnt = cnt + 1;
                    Trace.WriteLine("Min Pump torque: " + minPSpeed + "Max Pump Torque:" + maxPSpeed + "C Pump Torque:" + cPTorque);
                }
                if ((double)cTHT < minTHT || (double)cTHT > maxTHT)
                {
                    cnt = cnt + 1;
                    Trace.WriteLine("Min THT: " + minTHT + "Max THT:" + maxTHT + "C THT:" + cTHT);
                }

                CurrentAlarmDTO[] alarms = AlarmService.GetCurrentAlarmsByWellId(pcpWell.Id.ToString());

                foreach (CurrentAlarmDTO alarm in alarms)
                {
                    Trace.WriteLine("Alarm Date:" + alarm.StartTime + ", Alarm Type is : " + alarm.AlarmType.AlarmType + ", Alarm Value is:" + alarm.NumericValue ?? alarm.StringValue);
                }
                //Compare Alarms -- Latest plunger cycle data & Operating
                Assert.AreEqual(cnt, alarms.Count(), "Total counts are matched for PCP operating limit");
                Trace.WriteLine("");
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                RemoveWell(facilityId);
            }

        }

        //Test Method is written for PCP  - Default Well Test Limit - ForeSIte Alams
        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void PCPForeSiteAlarmForDefaultWellTest()
        {
            string facilityId = GetFacilityId("WFTA1K_", 1);
            try
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");

                //Adding Well along with WellTest Data
                WellDTO pcpWell = AddNonRRLWell(facilityId, WellTypeId.PCP, false, CalibrationMethodId.ReservoirPressure);

                AddWellSettingWithDoubleValues(pcpWell.Id, "Min Reservoir Pressure Acceptance Limit", 21.5);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max Reservoir Pressure Acceptance Limit", 2100.5);

                AddWellSettingWithDoubleValues(pcpWell.Id, "Min Water Cut Acceptance Limit", 0.1);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max Water Cut Acceptance Limit", 1.0);

                AddWellSettingWithDoubleValues(pcpWell.Id, "Min GOR Acceptance Limit", 1.0);
                AddWellSettingWithDoubleValues(pcpWell.Id, "Max GOR Acceptance Limit", 50.0);

                WellTestDTO latestTestData_PCP = WellTestDataService.GetLatestWellTestDataByWellId(pcpWell.Id.ToString());
                WellTestDataService.TuneSelectedWellTests(new long[] { latestTestData_PCP.Id });

                WellTestDTO latestTestDataAfterTune_PCP = WellTestDataService.GetLatestWellTestDataByWellId(pcpWell.Id.ToString());
                Assert.AreEqual("TUNING_SUCCEEDED", latestTestDataAfterTune_PCP.Status.ToString(), "Well Test Status is not Success");

                //Get Well Test Data - US System
                WellTestArrayAndUnitsDTO getPCPWellTestData = WellTestDataService.GetWellTestDataByWellId(pcpWell.Id.ToString());
                var MAmp = getPCPWellTestData.Values[0].MotorCurrent;
                var maxMAmp = MAmp + (MAmp * Convert.ToDecimal(0.2));
                var minMAmp = MAmp - (MAmp * Convert.ToDecimal(0.2));

                var MVolt = getPCPWellTestData.Values[0].MotorVolts;
                var maxMVolt = MVolt + (MVolt * Convert.ToDecimal(0.2));
                var minMVolt = MVolt - (MVolt * Convert.ToDecimal(0.2));

                var PDP = getPCPWellTestData.Values[0].PumpDischargePressure;
                var maxPDP = PDP + (PDP * Convert.ToDecimal(0.2));
                var minPDP = PDP - (PDP * Convert.ToDecimal(0.2));

                var PIP = getPCPWellTestData.Values[0].PumpIntakePressure;
                var maxPIP = PIP + (PIP * Convert.ToDecimal(0.2));
                var minPIP = PIP - (PIP * Convert.ToDecimal(0.2));

                var CHP = getPCPWellTestData.Values[0].AverageCasingPressure;
                var maxCHP = CHP + (CHP * Convert.ToDecimal(0.2));
                var minCHP = CHP - (CHP * Convert.ToDecimal(0.2));

                var FLP = getPCPWellTestData.Values[0].FlowLinePressure;
                var maxFLP = FLP + (FLP * Convert.ToDecimal(0.2));
                var minFLP = FLP - (FLP * Convert.ToDecimal(0.2));

                var THP = getPCPWellTestData.Values[0].AverageTubingPressure;
                var maxTHP = THP + (THP * Convert.ToDecimal(0.2));
                var minTHP = THP - (THP * Convert.ToDecimal(0.2));

                var THT = getPCPWellTestData.Values[0].AverageTubingTemperature;
                var maxTHT = THT + (THT * Convert.ToDecimal(0.2));
                var minTHT = THT - (THT * Convert.ToDecimal(0.2));

                var PSpeed = getPCPWellTestData.Values[0].PumpSpeed;
                var maxPSpeed = PSpeed + (PSpeed * Convert.ToDecimal(0.2));
                var minPSpeed = PSpeed - (PSpeed * Convert.ToDecimal(0.2));

                var PTorque = getPCPWellTestData.Values[0].PumpTorque;
                var maxPTorque = PTorque + (PTorque * Convert.ToDecimal(0.2));
                var minPTorque = PTorque - (PTorque * Convert.ToDecimal(0.2));

                var WRate = getPCPWellTestData.Values[0].Water;
                var maxWRate = WRate + (WRate * Convert.ToDecimal(0.2));
                var minWRate = WRate - (WRate * Convert.ToDecimal(0.2));

                var ORate = getPCPWellTestData.Values[0].Oil;
                var maxORate = ORate + (ORate * Convert.ToDecimal(0.2));
                var minORate = ORate - (ORate * Convert.ToDecimal(0.2));

                //Getting Well Staus in US System
                PCPWellStatusValueDTO us_pcpWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(pcpWell.Id.ToString(),
                                                         SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as PCPWellStatusValueDTO;
                Assert.IsNotNull(us_pcpWellStatus, "Failed to get Well status in 'US' units");

                var cMAmp = us_pcpWellStatus.MotorAmps;
                var cMVolt = us_pcpWellStatus.MotorVolts;
                var cPDP = us_pcpWellStatus.PumpDischargePressure;
                var cPIP = us_pcpWellStatus.PumpIntakePressure;
                var cCHP = us_pcpWellStatus.CasingPressure;
                var cFLP = us_pcpWellStatus.FlowLinePressure;
                var cTHP = us_pcpWellStatus.TubingPressure;
                var cTHT = us_pcpWellStatus.TubingHeadTemperature;
                var cWRate = us_pcpWellStatus.WaterRateMeasured;
                var cORate = us_pcpWellStatus.OilRateMeasured;
                var cPSpeed = us_pcpWellStatus.PumpSpeed;
                var cPTorque = us_pcpWellStatus.PumpTorque;



                int cnt = 0;
                List<string> expectedAlarms = new List<string>();

                if (cMAmp > (double)maxMAmp)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("High Motor Amps");
                    Trace.WriteLine(" High Motor Amps: " + maxMAmp + "--" + "CygNet Motor Amp: " + cMAmp);
                }
                else if (cMAmp < (double)minMAmp)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("Low Motor Amps");
                    Trace.WriteLine(" Low Motor Amps: " + minMAmp + "--" + "CygNet Motor Amps: " + cMAmp);
                }

                if (cMVolt > (double)maxMVolt)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("High Motor Volts");
                    Trace.WriteLine(" High Motor Volts: " + maxMVolt + "--" + "CygNet Motor Volts: " + cMVolt);
                }
                else if (cMVolt < (double)minMVolt)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("Low Motor Volts");
                    Trace.WriteLine(" Low Motor Volt: " + minMVolt + "--" + "CygNet Motor Volts: " + cMVolt);
                }

                if (cPDP > (double)maxPDP)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("High PDP");
                    Trace.WriteLine(" High PDP: " + maxPDP + "--" + "CygNet PDP: " + cPDP);
                }
                else if (cPDP < (double)minPDP)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("Low PDP");
                    Trace.WriteLine(" Low PDP: " + minPDP + "--" + "CygNet PDP: " + cPDP);
                }

                if (cPIP > (double)maxPIP)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("High PIP");
                    Trace.WriteLine(" High PIP: " + maxPIP + "--" + "CygNet PIP: " + cPIP);
                }
                else if (cPIP < (double)minPIP)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("Low PIP");
                    Trace.WriteLine(" Low PIP: " + minPIP + "--" + "CygNet PIP: " + cPIP);
                }

                if (cCHP > (double)maxCHP)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("High Casing Pressure");
                    Trace.WriteLine(" High Casing Pressure: " + maxCHP + "--" + "CygNet Casing Pressure: " + cCHP);
                }
                else if (cCHP < (double)minPDP)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("Low Casing Pressure");
                    Trace.WriteLine(" Low Casing Pressure: " + minCHP + "--" + "CygNet Casing Pressure: " + cCHP);
                }

                if (cFLP > (double)maxFLP)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("High Flow Line Pressure");
                    Trace.WriteLine(" High Flow Line Pressure: " + maxFLP + "--" + "CygNet Flow Line Pressure: " + cFLP);
                }
                else if (cFLP < (double)minFLP)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("Low Flow Line Pressure");
                    Trace.WriteLine(" Low Flow Line Pressure: " + minFLP + "--" + "CygNet Flow Line Pressure: " + cFLP);
                }

                if (cTHP > (double)maxTHP)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("High Tubing Pressure");
                    Trace.WriteLine(" High Tubing Pressure: " + maxTHP + "--" + "CygNet Tubing Pressure: " + cTHP);
                }
                else if (cTHP < (double)minTHP)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("Low Tubing Pressure");
                    Trace.WriteLine(" Low Tubing Pressure: " + minTHP + "--" + "CygNet Tubing Pressure: " + cTHP);
                }

                if (cTHT > (double)maxTHT)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("High Tubing Temperature");
                    Trace.WriteLine(" High Tubing Temperature: " + maxTHT + "--" + "CygNet Tubing Temperature: " + cTHT);
                }
                else if (cTHP < (double)minTHT)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("Low Tubing Temperature");
                    Trace.WriteLine(" Low Tubing Temperature: " + minTHT + "--" + "CygNet Tubing Temperature: " + cTHT);
                }

                if (cWRate > maxWRate)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("High Water Rate");
                    Trace.WriteLine(" High Water Rate: " + maxWRate + "--" + "CygNet Water Rate: " + cWRate);
                }
                else if (cWRate < minWRate)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("Low Water Rate");
                    Trace.WriteLine(" Low Water Rate: " + minWRate + "--" + "CygNet Water Rate: " + cWRate);
                }

                if (cORate > maxORate)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("High Oil Rate");
                    Trace.WriteLine(" High Oil Rate: " + maxORate + "--" + "CygNet Oil Rate: " + cORate);
                }
                else if (cORate < minORate)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("Low Oil Rate");
                    Trace.WriteLine(" Low Oil Rate: " + minORate + "--" + "CygNet Oil Rate: " + cORate);
                }
                if (cPSpeed > (double)maxPSpeed)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("High Pump Speed");
                    Trace.WriteLine(" High Pump Speed: " + maxPSpeed + "--" + "CygNet Pump Speed: " + cPSpeed);
                }
                else if (cPSpeed < (double)minPSpeed)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("Low Pump Speed");
                    Trace.WriteLine(" Low Pump Speed: " + minPSpeed + "--" + "CygNet Pump Speed: " + cPSpeed);
                }
                if (cPTorque > (double)maxPTorque)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("High Pump Torque");
                    Trace.WriteLine(" High Pump Torque: " + maxPTorque + "--" + "CygNet Pump Torque: " + cPTorque);
                }
                else if (cPTorque < (double)minPTorque)
                {
                    cnt = cnt + 1;
                    expectedAlarms.Add("Low Pump Torque");
                    Trace.WriteLine(" Low Pump Torque: " + minPTorque + "--" + "CygNet Pump Torque: " + cPTorque);
                }

                //Alarm History for PCP WELL
                //AlarmHistoryDTO[] alarms = AlarmService.GetNonCygNetAlarmHistoryByWellIdAndDateRange(addedWellConfig.Well.Id.ToString(), DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'"), DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'"));
                CurrentAlarmDTO[] alarms = AlarmService.GetCurrentAlarmsByWellId(pcpWell.Id.ToString());

                foreach (CurrentAlarmDTO alarm in alarms)
                {
                    Trace.WriteLine("Alarm Date:" + alarm.StartTime + ", Alarm Type is : " + alarm.AlarmType.AlarmType + ", Alarm Value is:" + alarm.NumericValue ?? alarm.StringValue);
                }
                Assert.AreEqual(cnt, alarms.Count(), "Total Alarms counts are not matching for PCP well test");
                foreach (string expAlarm in expectedAlarms)
                {
                    bool alarmPresent = false;
                    foreach (CurrentAlarmDTO alarm in alarms)
                    {
                        if (expAlarm == alarm.AlarmType.AlarmType)
                        {
                            alarmPresent = true;
                            break;
                        }
                    }
                    Assert.IsTrue(alarmPresent, expAlarm + " is not generated");
                }
                Trace.WriteLine("");
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                RemoveWell(facilityId);
            }
        }
        //Test Method is written for PlungerLift - Operating Limits - with override
        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void PlungerLiftForeSiteAlarmForOperatingLimit()
        {
            //Add PGL Well With Model
            WellConfigDTO addedWellConfig = AddPlungerLiftWellWithModel("PGLWELL_", "PL-631.wflx");

            //Adding WellTest
            WellTestDTO testDataDTO = new WellTestDTO()
            {
                WellId = addedWellConfig.Well.Id,
                SPTCodeDescription = "AllocatableTest",
                SampleDate = addedWellConfig.Well.CommissionDate.Value.AddDays(5).ToUniversalTime(),
                TestDuration = random.Next(12, 24),
                MaximumCasingPressure = random.Next(1000, 1900),
                MaximumTubingPressure = random.Next(600, 799),
                MinimumCasingPressure = random.Next(800, 999),
                MinimumTubingPressure = random.Next(500, 599),
                FlowLinePressure = random.Next(300, 400),
                BuildTime = random.Next(100, 14400),
                AfterFlowTime = random.Next(100, 14400),
                FallTime = random.Next(100, 14400),
                RiseTime = random.Next(100, 14400),
                CycleGasVolume = random.Next(500, 1900),
                CycleWaterVolume = random.Next(500, 1900),
                CycleOilVolume = 0
            };
            //Saved WellTest Data
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(addedWellConfig.Well.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
            Assert.AreEqual(1, WellTestDataService.GetWellTestDataByWellId(addedWellConfig.Well.Id.ToString()).Values.Length, "Well Test is not saved successfully");
            Trace.WriteLine("Well test saved successfully");

            //Overriding -- Adding Operating Limit
            AddWellSetting(addedWellConfig.Well.Id, "Min Oil Rate Operating Limit", random.Next(50, 500));
            AddWellSetting(addedWellConfig.Well.Id, "Max Oil Rate Operating Limit", random.Next(510, 49000));

            AddWellSetting(addedWellConfig.Well.Id, "Min Water Rate Operating Limit", random.Next(50, 500));
            AddWellSetting(addedWellConfig.Well.Id, "Max Water Rate Operating Limit", random.Next(510, 14000));

            AddWellSetting(addedWellConfig.Well.Id, "Min Gas Rate Operating Limit", random.Next(50, 500));
            AddWellSetting(addedWellConfig.Well.Id, "Max Gas Rate Operating Limit", random.Next(510, 14000));

            AddWellSetting(addedWellConfig.Well.Id, "Min Maximum Casing Head Pressure Operating Limit", random.Next(50, 500));
            AddWellSetting(addedWellConfig.Well.Id, "Max Maximum Casing Head Pressure Operating Limit", random.Next(510, 14000));

            AddWellSetting(addedWellConfig.Well.Id, "Min Minimum Casing Head Pressure Operating Limit", random.Next(50, 500));
            AddWellSetting(addedWellConfig.Well.Id, "Max Minimum Casing Head Pressure Operating Limit", random.Next(510, 14000));

            AddWellSetting(addedWellConfig.Well.Id, "Min Maximum Tubing Head Pressure Operating Limit", random.Next(50, 500));
            AddWellSetting(addedWellConfig.Well.Id, "Max Maximum Tubing Head Pressure Operating Limit", random.Next(510, 14000));

            AddWellSetting(addedWellConfig.Well.Id, "Min Minimum Tubing Head Pressure Operating Limit", random.Next(50, 500));
            AddWellSetting(addedWellConfig.Well.Id, "Max Minimum Tubing Head Pressure Operating Limit", random.Next(510, 14000));

            AddWellSetting(addedWellConfig.Well.Id, "Min Cycle Time Operating Limit", random.Next(10, 500));
            AddWellSetting(addedWellConfig.Well.Id, "Max Cycle Time Operating Limit", random.Next(510, 1000));

            //Get Operating Limit

            WellSettingDTO[] settings = SettingService.GetWellSettingsByWellId(addedWellConfig.Well.Id.ToString());
            var minOilRate = settings.FirstOrDefault(x => x.Setting.Name == "Min Oil Rate Operating Limit").NumericValue;
            var maxOilRate = settings.FirstOrDefault(x => x.Setting.Name == "Max Oil Rate Operating Limit").NumericValue;

            var minWaterRate = settings.FirstOrDefault(x => x.Setting.Name == "Min Water Rate Operating Limit").NumericValue;
            var maxWaterRate = settings.FirstOrDefault(x => x.Setting.Name == "Max Water Rate Operating Limit").NumericValue;

            var minGasRate = settings.FirstOrDefault(x => x.Setting.Name == "Min Gas Rate Operating Limit").NumericValue;
            var maxGasRate = settings.FirstOrDefault(x => x.Setting.Name == "Max Gas Rate Operating Limit").NumericValue;

            var minMaxCHP = settings.FirstOrDefault(x => x.Setting.Name == "Min Maximum Casing Head Pressure Operating Limit").NumericValue;
            var maxMaxCHP = settings.FirstOrDefault(x => x.Setting.Name == "Max Maximum Casing Head Pressure Operating Limit").NumericValue;

            var minMinCHP = settings.FirstOrDefault(x => x.Setting.Name == "Min Minimum Casing Head Pressure Operating Limit").NumericValue;
            var maxMinCHP = settings.FirstOrDefault(x => x.Setting.Name == "Max Minimum Casing Head Pressure Operating Limit").NumericValue;

            var minMaxTHP = settings.FirstOrDefault(x => x.Setting.Name == "Min Maximum Tubing Head Pressure Operating Limit").NumericValue;
            var maxMaxTHP = settings.FirstOrDefault(x => x.Setting.Name == "Max Maximum Tubing Head Pressure Operating Limit").NumericValue;

            var minMinTHP = settings.FirstOrDefault(x => x.Setting.Name == "Min Minimum Tubing Head Pressure Operating Limit").NumericValue;
            var maxMinTHP = settings.FirstOrDefault(x => x.Setting.Name == "Max Minimum Tubing Head Pressure Operating Limit").NumericValue;

            var minCycleTime = settings.FirstOrDefault(x => x.Setting.Name == "Min Cycle Time Operating Limit").NumericValue;
            var maxCycleTime = settings.FirstOrDefault(x => x.Setting.Name == "Max Cycle Time Operating Limit").NumericValue;

            //Run Plunger Lift Cycle data
            Trace.WriteLine("Running Plunger Lift Cycle data for last 5 days");
            SurveillanceService.GetLatestPGLCycleData(addedWellConfig.Well, "5");

            //Get Latest Plunger Lift Cycle Data
            Trace.WriteLine("Getting Plunger Lift Cycle data");
            var pglCycleDataFromDb = SurveillanceService.GetLatestPGLCycle(addedWellConfig.Well.Id.ToString());
            var oilRate = pglCycleDataFromDb.Value.OilRate;
            var gasRate = pglCycleDataFromDb.Value.GasRate;
            var waterRate = pglCycleDataFromDb.Value.WaterRate;
            var minCHP = pglCycleDataFromDb.Value.MinimumCasingHeadPressure;
            var maxCHP = pglCycleDataFromDb.Value.MaximumCasingHeadPressure;
            var minTHP = pglCycleDataFromDb.Value.MinimumTubingHeadPressure;
            var maxTHP = pglCycleDataFromDb.Value.MaximumTubingHeadPressure;
            //Since Cycle time Operating limits are in minutes and Cycle time retrieved from PGL Cycle is in seconds, Converting Cycle time into minutes.
            var cycleTime = pglCycleDataFromDb.Value.CycleTime;

            //Comparing operating limit with latest plunger cycle data
            int cnt = 0;
            if ((double)gasRate < minGasRate || (double)gasRate > maxGasRate)
            {
                cnt = cnt + 1;
                Trace.WriteLine("Min Gas Rate: " + minGasRate + " Max Gas Rate: " + maxGasRate + " Gas Rate: " + gasRate);
            }
            if ((double)waterRate < minWaterRate || (double)waterRate > maxWaterRate)
            {
                cnt = cnt + 1;
                Trace.WriteLine("Min Water Rate: " + minWaterRate + " Max Water Rate: " + maxWaterRate + " Water Rate: " + waterRate);
            }
            if ((double)minCHP < minMinCHP || (double)minCHP > maxMinCHP)
            {
                cnt = cnt + 1;
                Trace.WriteLine("Min Min CHP: " + minMinCHP + " Max Min CHP: " + maxMinCHP + " Min CHP: " + minCHP);
            }
            if ((double)maxCHP < minMaxCHP || (double)maxCHP > maxMaxCHP)
            {
                cnt = cnt + 1;
                Trace.WriteLine("Min Max CHP: " + minMaxCHP + " Max Max CHP: " + maxMaxCHP + " Max CHP: " + maxCHP);
            }
            if ((double)minTHP < minMinTHP || (double)minTHP > maxMinTHP)
            {
                cnt = cnt + 1;
                Trace.WriteLine("Min Min THP: " + minMinTHP + " Max Min THP: " + maxMinTHP + " Min THP: " + minTHP);
            }
            if ((double)maxTHP < minMaxTHP || (double)maxTHP > maxMaxTHP)
            {
                cnt = cnt + 1;
                Trace.WriteLine("Min Max THP: " + minMaxTHP + " Max Max THP: " + maxMaxTHP + " Max THP: " + maxTHP);
            }
            if ((double)cycleTime < minCycleTime || (double)cycleTime > maxCycleTime)
            {
                cnt = cnt + 1;
                Trace.WriteLine("Min Cycle Time: " + minCycleTime + " Max Cycle Time: " + maxCycleTime + " Cycle Time: " + cycleTime);
            }

            //Alarm History for PGL WELL
            //AlarmHistoryDTO[] alarms = AlarmService.GetNonCygNetAlarmHistoryByWellIdAndDateRange(addedWellConfig.Well.Id.ToString(), DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'"), DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'"));
            CurrentAlarmDTO[] alarms = AlarmService.GetCurrentAlarmsByWellId(addedWellConfig.Well.Id.ToString());

            foreach (CurrentAlarmDTO alarm in alarms)
            {
                Trace.WriteLine("Alarm Date:" + alarm.StringValue + ", Alarm Type is : " + alarm.AlarmType.AlarmType + ", Alarm Value is:" + alarm.NumericValue ?? alarm.StringValue);
            }

            //Compare Alarms -- Latest plunger cycle data & Operating
            Assert.AreEqual(cnt, alarms.Count(), "Total counts are matched for plunger lift operating high limit");
            Trace.WriteLine("");
        }

        //Test Method is written for PlungerLift - Operating Limits - default 20% - with Well Test
        [TestMethod]
        public void PlungerLiftForeSiteAlarmForWellTest()
        {
            //Add PGL Well With Model
            WellConfigDTO addedWellConfig = AddPlungerLiftWellWithModel("PGLWELL_", "PL-631.wflx");

            //Adding WellTest
            WellTestDTO testDataDTO = new WellTestDTO()
            {
                WellId = addedWellConfig.Well.Id,
                SPTCodeDescription = "AllocatableTest",
                SampleDate = addedWellConfig.Well.CommissionDate.Value.AddDays(2).ToUniversalTime(),
                TestDuration = random.Next(12, 24),
                MaximumCasingPressure = 250,
                MinimumCasingPressure = 210,

                MaximumTubingPressure = 240,
                MinimumTubingPressure = 200,

                FlowLinePressure = 190,
                BuildTime = 5421,
                AfterFlowTime = 4526,
                FallTime = 1452,
                RiseTime = 1423,
                CycleGasVolume = random.Next(500, 1900),
                CycleWaterVolume = random.Next(500, 1900),
                CycleOilVolume = 0
            };

            //Saved WellTest Data
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(addedWellConfig.Well.Id.ToString()).Units;
            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

            // Get wellTest data and assigned to variable
            WellTestArrayAndUnitsDTO wellTest = WellTestDataService.GetWellTestDataByWellId(addedWellConfig.Well.Id.ToString());
            Assert.IsNotNull(wellTest, "Failed to retrieve latest well test");

            var wellTestWaterRate = wellTest.Values[0].Water;
            var wellTestWaterRateHi = wellTestWaterRate + (wellTestWaterRate * Convert.ToDecimal(0.2));
            var wellTestWaterRateLo = wellTestWaterRate - (wellTestWaterRate * Convert.ToDecimal(0.2));

            var wellTestGasRate = wellTest.Values[0].Gas;
            var wellTestGasRateHi = wellTestGasRate + (wellTestGasRate * Convert.ToDecimal(0.2));
            var wellTestGasRateLo = wellTestGasRate - (wellTestGasRate * Convert.ToDecimal(0.2));

            var wellTestMaxCHP = wellTest.Values[0].MaximumCasingPressure;
            var wellTestMaxCHPHi = wellTestMaxCHP + (wellTestMaxCHP * Convert.ToDecimal(0.2));
            var wellTestMaxCHPLo = wellTestMaxCHP - (wellTestMaxCHP * Convert.ToDecimal(0.2));

            var wellTestMinCHP = wellTest.Values[0].MinimumCasingPressure;
            var wellTestMinCHPHi = wellTestMinCHP + (wellTestMinCHP * Convert.ToDecimal(0.2));
            var wellTestMinCHPLo = wellTestMinCHP - (wellTestMinCHP * Convert.ToDecimal(0.2));

            var wellTestMaxTHP = wellTest.Values[0].MaximumTubingPressure;
            var wellTestMaxTHPHi = wellTestMaxTHP + (wellTestMaxTHP * Convert.ToDecimal(0.2));
            var wellTestMaxTHPLo = wellTestMaxTHP - (wellTestMaxTHP * Convert.ToDecimal(0.2));

            var wellTestMinTHP = wellTest.Values[0].MinimumTubingPressure;
            var wellTestMinTHPHi = wellTestMinTHP + (wellTestMinTHP * Convert.ToDecimal(0.2));
            var wellTestMinTHPLo = wellTestMinTHP - (wellTestMinTHP * Convert.ToDecimal(0.2));

            var wellTestCycleTime = wellTest.Values[0].CycleTime;
            var wellTestCycleTimeHi = wellTestCycleTime + (wellTestCycleTime * Convert.ToDecimal(0.2));
            var wellTestCycleTimeLo = wellTestCycleTime - (wellTestCycleTime * Convert.ToDecimal(0.2));

            //Run Plunger Lift Cycle data
            Trace.WriteLine("Running Plunger Lift Cycle data for last 5 days");
            SurveillanceService.GetLatestPGLCycleData(addedWellConfig.Well, "5");

            //Get Latest Plunger Lift Cycle Data
            Trace.WriteLine("Getting Plunger Lift Cycle data");
            var pglCycleDataFromDb = SurveillanceService.GetLatestPGLCycle(addedWellConfig.Well.Id.ToString());

            //var oilRate = pglCycleDataFromDb.Value.OilRate;
            var gasRate = pglCycleDataFromDb.Value.GasRate;
            var waterRate = pglCycleDataFromDb.Value.WaterRate;
            var minCHP = pglCycleDataFromDb.Value.MinimumCasingHeadPressure;
            var maxCHP = pglCycleDataFromDb.Value.MaximumCasingHeadPressure;
            var minTHP = pglCycleDataFromDb.Value.MinimumTubingHeadPressure;
            var maxTHP = pglCycleDataFromDb.Value.MaximumTubingHeadPressure;
            var cycleTime = pglCycleDataFromDb.Value.CycleTime;

            //Comparing operating limit with latest plunger cycle data
            int cnt = 0;
            List<string> expectedAlarms = new List<string>();
            //if (oilRate < wellTestOilRateLo || oilRate > wellTestOilRateHi)
            //{
            //    cnt = cnt + 1;
            //    Trace.WriteLine("Min Oil Rate: " + wellTestOilRateLo + " Max Oil Rate: " + wellTestOilRateHi + " Oil Rate: " + oilRate);
            //}
            if (gasRate < wellTestGasRateLo)
            {
                cnt = cnt + 1;
                expectedAlarms.Add("Low Gas Rate");
                Trace.WriteLine("Min Gas Rate: " + wellTestGasRateLo + " Max Gas Rate: " + wellTestGasRateHi + " Gas Rate: " + gasRate);
            }
            else if (gasRate > wellTestGasRateHi)
            {
                cnt = cnt + 1;
                expectedAlarms.Add("High Gas Rate");
                Trace.WriteLine("Min Gas Rate: " + wellTestGasRateLo + " Max Gas Rate: " + wellTestGasRateHi + " Gas Rate: " + gasRate);
            }

            if (waterRate < wellTestWaterRateLo)
            {
                cnt = cnt + 1;
                expectedAlarms.Add("Low Water Rate");
                Trace.WriteLine("Min Water Rate: " + wellTestWaterRateLo + " Max Water Rate: " + wellTestWaterRateHi + " Water Rate: " + waterRate);
            }
            else if (waterRate > wellTestWaterRateHi)
            {
                cnt = cnt + 1;
                expectedAlarms.Add("High Water Rate");
                Trace.WriteLine("Min Water Rate: " + wellTestWaterRateLo + " Max Water Rate: " + wellTestWaterRateHi + " Water Rate: " + waterRate);
            }

            if (minCHP < wellTestMinCHPLo)
            {
                cnt = cnt + 1;
                expectedAlarms.Add("Low Minimum Casing Head Pressure");
                Trace.WriteLine("Min Min CHP: " + wellTestMinCHPLo + " Max Min CHP: " + wellTestMinCHPHi + " Min CHP: " + minCHP);
            }
            else if (minCHP > wellTestMinCHPHi)
            {
                cnt = cnt + 1;
                expectedAlarms.Add("High Minimum Casing Head Pressure");
                Trace.WriteLine("Min Min CHP: " + wellTestMinCHPLo + " Max Min CHP: " + wellTestMinCHPHi + " Min CHP: " + minCHP);
            }

            if (maxCHP < wellTestMaxCHPLo)
            {
                cnt = cnt + 1;
                expectedAlarms.Add("Low Maximum Casing Head Pressure");
                Trace.WriteLine("Min Max CHP: " + wellTestMaxCHPLo + " Max Max CHP: " + wellTestMaxCHPHi + " Max CHP: " + maxCHP);
            }
            else if (maxCHP > wellTestMaxCHPHi)
            {
                cnt = cnt + 1;
                expectedAlarms.Add("High Maximum Casing Head Pressure");
                Trace.WriteLine("Min Max CHP: " + wellTestMaxCHPLo + " Max Max CHP: " + wellTestMaxCHPHi + " Max CHP: " + maxCHP);
            }

            if (minTHP < wellTestMinTHPLo)
            {
                cnt = cnt + 1;
                expectedAlarms.Add("Low Minimum Tubing Head Pressure");
                Trace.WriteLine("Min Min THP: " + wellTestMinTHPLo + " Max Min THP: " + wellTestMinTHPHi + " Min THP: " + minTHP);
            }
            else if (minTHP > wellTestMinTHPHi)
            {
                cnt = cnt + 1;
                expectedAlarms.Add("High Minimum Tubing Head Pressure");
                Trace.WriteLine("Min Min THP: " + wellTestMinTHPLo + " Max Min THP: " + wellTestMinTHPHi + " Min THP: " + minTHP);
            }

            if (maxTHP < wellTestMaxTHPLo)
            {
                cnt = cnt + 1;
                expectedAlarms.Add("Low Maximum Tubing Head Pressure");
                Trace.WriteLine("Min Max THP: " + wellTestMaxTHPLo + " Max Max THP: " + wellTestMaxTHPHi + " Max THP: " + maxTHP);
            }
            else if (maxTHP > wellTestMaxTHPHi)
            {
                cnt = cnt + 1;
                expectedAlarms.Add("High Maximum Tubing Head Pressure");
                Trace.WriteLine("Min Max THP: " + wellTestMaxTHPLo + " Max Max THP: " + wellTestMaxTHPHi + " Max THP: " + maxTHP);
            }

            if (cycleTime < wellTestCycleTimeLo)
            {
                cnt = cnt + 1;
                expectedAlarms.Add("Low Cycle Time");
                Trace.WriteLine("Min Cycle Time: " + wellTestCycleTimeLo + " Max Cycle Time: " + wellTestCycleTimeHi + " Cycle Time: " + cycleTime);
            }
            else if (cycleTime > wellTestCycleTimeHi)
            {
                cnt = cnt + 1;
                expectedAlarms.Add("High Cycle Time");
                Trace.WriteLine("Min Cycle Time: " + wellTestCycleTimeLo + " Max Cycle Time: " + wellTestCycleTimeHi + " Cycle Time: " + cycleTime);
            }

            //Alarm History for PGL WELL
            //AlarmHistoryDTO[] alarms = AlarmService.GetNonCygNetAlarmHistoryByWellIdAndDateRange(addedWellConfig.Well.Id.ToString(), DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'"), DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'"));
            CurrentAlarmDTO[] alarms = AlarmService.GetCurrentAlarmsByWellId(addedWellConfig.Well.Id.ToString());

            foreach (CurrentAlarmDTO alarm in alarms)
            {
                Trace.WriteLine("Alarm Date:" + alarm.StringValue + ", Alarm Type is : " + alarm.AlarmType.AlarmType + ", Alarm Value is:" + alarm.NumericValue ?? alarm.StringValue);
            }
            Assert.AreEqual(cnt, alarms.Count(), "Total Alarms counts are not matching for plunger lift well test");
            foreach (string expAlarm in expectedAlarms)
            {
                bool alarmPresent = false;
                foreach (CurrentAlarmDTO alarm in alarms)
                {
                    if (expAlarm == alarm.AlarmType.AlarmType)
                    {
                        alarmPresent = true;
                        break;
                    }
                }

                Assert.IsTrue(alarmPresent, expAlarm + " is not generated");
            }

            Trace.WriteLine("");
        }

        protected WellDTO AddNonRRLWell_PGL(string BaseFacTag, WellTypeId wellType, bool scan = true, CalibrationMethodId tuningMethod = CalibrationMethodId.ReservoirPressure)
        {
            string facilityID = s_isRunningInATS ? BaseFacTag + "00001" : BaseFacTag + "0001";
            Authenticate();
            WellDTO well = SetDefaultFluidType(new WellDTO()
            {
                Name = DefaultWellName,
                FacilityId = BaseFacTag,
                DataConnection = GetDefaultCygNetDataConnection(),
                CommissionDate = DateTime.Today.AddYears(-2),
                AssemblyAPI = BaseFacTag,
                SubAssemblyAPI = BaseFacTag,
                IntervalAPI = BaseFacTag,
                WellType = wellType,
                GasAllocationGroup = null,
                OilAllocationGroup = null,
                WaterAllocationGroup = null
            });
            WellConfigDTO wellConfig = new WellConfigDTO();
            wellConfig.Well = well;
            wellConfig.ModelConfig = ReturnBlankModel();
            //Well
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfig);
            return addedWellConfig.Well;
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void AddPGLWellDailyAverageData()
        {
            try
            {
                //Add PGL Well With Model
                WellConfigDTO addedWellConfig = AddPlungerLiftWellWithModel("PGLWELL_", "PL-631.wflx");

                //Adding WellTest
                WellTestDTO testDataDTO = new WellTestDTO()
                {
                    WellId = addedWellConfig.Well.Id,
                    SPTCodeDescription = "AllocatableTest",
                    SampleDate = addedWellConfig.Well.CommissionDate.Value.AddDays(5).ToUniversalTime(),
                    TestDuration = random.Next(12, 24),
                    MaximumCasingPressure = random.Next(1000, 1900),
                    MaximumTubingPressure = random.Next(600, 799),
                    MinimumCasingPressure = random.Next(800, 999),
                    MinimumTubingPressure = random.Next(500, 599),
                    FlowLinePressure = random.Next(300, 400),
                    BuildTime = random.Next(100, 14400),
                    AfterFlowTime = random.Next(100, 14400),
                    FallTime = random.Next(100, 14400),
                    RiseTime = random.Next(100, 14400),
                    CycleGasVolume = random.Next(500, 1900),
                    CycleWaterVolume = random.Next(500, 1900),
                    CycleOilVolume = 0
                };
                //Saved WellTest Data
                WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(addedWellConfig.Well.Id.ToString()).Units;
                WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                //Run Plunger Lift Cycle data
                SurveillanceService.GetLatestPGLCycleData(addedWellConfig.Well, "5");
                //Get Latest Plunger Lift Cycle Data
                var pglCycleDataFromDb = SurveillanceService.GetLatestPGLCycle(addedWellConfig.Well.Id.ToString());

                WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO
                {
                    EndDateTime = DateTime.Today.ToUniversalTime(),
                    StartDateTime = DateTime.Today.AddDays(-1).ToUniversalTime(),
                    AverageMaximumCasingPressure = 120,
                    AverageMaximumTubingPressure = 120,
                    AverageMinimumCasingPressure = 100,
                    AverageMinimumTubingPressure = 100,
                    AverageCycleGasVolume = 80,
                    AverageCycleTime = 75,
                    AverageCycleBuildTime = 50,
                    AverageCycleRiseTime = 66,
                    AverageCycleAfterFlowTime = 78,
                    AverageCycleOilVolume = 90,
                    AveragePlungerRiseSpeed = 80,
                    AverageCycleFallTime = 96,
                    AverageCycleWaterVolume = 49,
                    OilRateAllocated = 8,
                    WaterRateAllocated = 10,
                    GasRateAllocated = 5,
                    FLP = 10,
                    CHP = 329,
                    DifferentialPressure = 200,
                    THP = 100,
                    AverageMinimumLoadFactor = 100,
                    Status = WellDailyAverageDataStatus.Original,
                    RunTime = 24,
                    THT = 0,
                    WellId = addedWellConfig.Well.Id,
                    Id = 0,
                };
                bool addDailyAvgDataForPGL = SurveillanceService.AddUpdateWellDailyAverageDataNonRRL(dailyAverageDTO);
                Assert.IsTrue(addDailyAvgDataForPGL, "Failed to add Daily Average data");

                WellDailyAverageAndTestDTO dailyAveragedata = SurveillanceService.GetWellDailyAverageAndTest(addedWellConfig.Well.Id.ToString(), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime()));
                Assert.IsNotNull(dailyAveragedata);
                Assert.IsNotNull(dailyAveragedata.DailyAverage.Value.Id);
                Assert.AreEqual(dailyAverageDTO.WellId, dailyAveragedata.DailyAverage.Value.WellId);
                Assert.AreEqual(dailyAverageDTO.AverageMaximumCasingPressure, dailyAveragedata.DailyAverage.Value.AverageMaximumCasingPressure);
                Assert.AreEqual(dailyAverageDTO.AverageMaximumTubingPressure, dailyAveragedata.DailyAverage.Value.AverageMaximumTubingPressure);
                Assert.AreEqual(dailyAverageDTO.AverageMinimumCasingPressure, dailyAveragedata.DailyAverage.Value.AverageMinimumCasingPressure);
                Assert.AreEqual(dailyAverageDTO.EndDateTime.ToString(), dailyAveragedata.DailyAverage.Value.EndDateTime.ToString());
                Assert.AreEqual(dailyAverageDTO.StartDateTime.ToString(), dailyAveragedata.DailyAverage.Value.StartDateTime.ToString());
                Assert.AreEqual(dailyAverageDTO.FLP, dailyAveragedata.DailyAverage.Value.FLP);
                Assert.AreEqual(dailyAverageDTO.AverageMinimumTubingPressure, dailyAveragedata.DailyAverage.Value.AverageMinimumTubingPressure);
                Assert.AreEqual(dailyAverageDTO.AverageCycleFallTime, dailyAveragedata.DailyAverage.Value.AverageCycleFallTime);
                Assert.AreEqual(dailyAverageDTO.AverageCycleGasVolume, dailyAveragedata.DailyAverage.Value.AverageCycleGasVolume);
                Assert.AreEqual(dailyAverageDTO.AverageCycleTime, dailyAveragedata.DailyAverage.Value.AverageCycleTime);
                Assert.AreEqual(dailyAverageDTO.AverageCycleBuildTime, dailyAveragedata.DailyAverage.Value.AverageCycleBuildTime);
                Assert.AreEqual(dailyAverageDTO.AverageCycleOilVolume, dailyAveragedata.DailyAverage.Value.AverageCycleOilVolume);
                Assert.AreEqual(dailyAverageDTO.AveragePlungerRiseSpeed, dailyAveragedata.DailyAverage.Value.AveragePlungerRiseSpeed);
                Assert.AreEqual(dailyAverageDTO.AverageCycleWaterVolume, dailyAveragedata.DailyAverage.Value.AverageCycleWaterVolume);
                Assert.AreEqual(dailyAverageDTO.AverageCycleRiseTime, dailyAveragedata.DailyAverage.Value.AverageCycleRiseTime);
                Assert.AreEqual(dailyAverageDTO.AverageCycleAfterFlowTime, dailyAveragedata.DailyAverage.Value.AverageCycleAfterFlowTime);
                Assert.AreEqual(dailyAverageDTO.OilRateAllocated, dailyAveragedata.DailyAverage.Value.OilRateAllocated);
                Assert.AreEqual(dailyAverageDTO.WaterRateAllocated, dailyAveragedata.DailyAverage.Value.WaterRateAllocated);
                Assert.AreEqual(dailyAverageDTO.GasRateAllocated, dailyAveragedata.DailyAverage.Value.GasRateAllocated);
                Assert.AreEqual(dailyAverageDTO.THT, dailyAveragedata.DailyAverage.Value.THT);
                Assert.AreEqual(dailyAverageDTO.THP, dailyAveragedata.DailyAverage.Value.THP);
                Assert.AreEqual(dailyAverageDTO.RunTime, dailyAveragedata.DailyAverage.Value.RunTime);
                Assert.AreEqual(dailyAverageDTO.AverageMinimumLoadFactor, dailyAveragedata.DailyAverage.Value.AverageMinimumLoadFactor);
                Assert.AreEqual(dailyAverageDTO.CHP, dailyAveragedata.DailyAverage.Value.CHP);
                Assert.AreEqual(dailyAverageDTO.DifferentialPressure, dailyAveragedata.DailyAverage.Value.DifferentialPressure);
                Assert.AreEqual(dailyAverageDTO.Status, dailyAveragedata.DailyAverage.Value.Status);
            }
            finally
            {
                RemoveWell(DefaultWellName);
            }
        }

        [TestMethod] //Script not added to ATS - Need to add config for ATS settings
        public void AWTValidations()
        {
            string wellFacility = GetFacilityId("IWC_", 1);
            //Create a WellTest in VHS
            string uis = $"{s_site}.{s_cvsService}";
            string pointName = uis + ":AWT_CONFIG";
            Console.WriteLine($"{pointName} AWT_CONFIG Used for CYGNET with domain {s_domain}.");
            SystemSettingDTO awtPointPathSetting = SettingService.GetSystemSettingByName(SettingServiceStringConstants.AWT_POINT_PATH);

            if (ushort.TryParse(s_domain, out ushort result))
            {
                try
                {
                    var pointTag = new PointTag(pointName);
                    var pntDss = new RealtimeClient(new DomainSiteService(result, s_site, s_cvsService)).GetAssociatedPointConfigurationService();
                    if (new ConfigClient(pntDss).PointRecordExists(pointTag))
                    {
                        Console.WriteLine($"AWT_CONFIG point {pointTag} exists on {pntDss}.");
                        var pntConfig = new CxPntLib.PntClientClass();
                        pntConfig.Connect(pntDss.SiteService.ToString());
                        if (pntConfig.IsConnected)
                        {
                            Console.WriteLine($"Connected to PNT service at {pntDss.SiteService}.");
                        }
                    }
                    else
                    {
                        throw new Exception($"AWT_CONFIG point {pointTag} does not exist on {pntDss}");
                    }
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine($"Failed to initialize a CygNet PntClient.  There may be CygNet DLLs that are not registered.");
                    throw;
                }
                catch
                {
                    Console.WriteLine($"Could not pull the AWT_CONFIG point information for {pointName} on [{s_domain}]{s_site}.{s_cvsService}.");
                    throw;
                }
            }
            else
            {
                throw new Exception($"Domain {s_domain} is not a valid CygNet domain.");
            }

            awtPointPathSetting.StringValue = pointName;
            SettingService.SaveSystemSetting(awtPointPathSetting);
            _systemSettingNamesToRemove.Add(awtPointPathSetting.Setting.Name);

            DateTime awtRecordDate = DateTime.Now.AddHours(-1);

            AWT_FS.Program.AwtVHSAddPoints(s_domain, uis, wellFacility, awtRecordDate);
            //Create a Well
            AddWell_NonRRL(wellFacility, WellTypeId.ESP, true);
            WellDTO[] allWells = WellService.GetAllWells();
            WellDTO well = allWells.FirstOrDefault(w => w.Name.Equals(wellFacility));

            // Not sure how the CygNet APIs handle UTC time, but after experimenting it seems that the start date is treated as a local time
            // no matter what and the end date is converted to local time from UTC.  The below combination of start and end date seems to be able 
            // to retrieve the record that was just inserted.  Also, the date query runs against the timestamps for the VHS entries, not the 
            // well test dates.  Consequently, an AWT test record date should always have the same timestamp as its VHS record
            DateTime awtRecordDateUtc = awtRecordDate.ToUniversalTime();
            var utcOffSetHours = TimeZoneInfo.Local.GetUtcOffset(awtRecordDateUtc).Hours;
            string startTime = DTOExtensions.ToISO8601(awtRecordDateUtc.AddHours(utcOffSetHours).AddMinutes(-1));
            string endTime = DTOExtensions.ToISO8601(awtRecordDateUtc.AddMinutes(1));

            Console.WriteLine($"{startTime} is startTime of DateRange preconversion.");
            Console.WriteLine($"{endTime} is endTime of DateRange preconversion.");
            SurveillanceService.AddWellTestRecordsFromAWTByDataRange(well.Id.ToString(), startTime, endTime);
            //Validate if WellTest has been created for the Well with Quality Code AWT RAW
            WellTestArrayAndUnitsDTO wellTest = WellTestDataService.GetWellTestDataByWellId(well.Id.ToString());
            Assert.IsNotNull(wellTest, "Unexpected null well test object.");
            Assert.IsNotNull(wellTest.Values, "Unexpected null well test values array.");
            Assert.AreNotEqual(0, wellTest.Values.Length, "Unexpected empty well test values array.");
            Assert.AreEqual(25.2, (double)wellTest.Values[0].Oil, 0.01, "Mismatch between expected oil rate and actual oil rate.");
            Assert.AreEqual(2.22, (double)wellTest.Values[0].Water, 0.01, "Mismatch between expected water rate and actual water rate.");
            Assert.AreEqual(19.22, (double)wellTest.Values[0].Gas, 0.01, "Mismatch between expected gas rate and actual gas rate.");
            Assert.AreEqual("AWT_RAW", wellTest.Values[0].SPTCodeDescription, "Mismatch between expected quality code and actual quality code.");
        }

        [TestMethod] //Script not added to ATS - Need to add config for ATS settings
        public void CreateAWTVHSData()
        {
            string wellFacility = GetFacilityId("RPOC_", 1);
            //Create a WellTest in VHS
            string uis = $"{s_site}.{s_cvsService}";
            string pointName = uis + ":AWT_CONFIG";
            Console.WriteLine($"{pointName} AWT_CONFIG Used for CYGNET with doain {s_domain}.");
            SystemSettingDTO awtPointPathSetting = SettingService.GetSystemSettingByName(SettingServiceStringConstants.AWT_POINT_PATH);
            awtPointPathSetting.StringValue = pointName;
            SettingService.SaveSystemSetting(awtPointPathSetting);
            _systemSettingNamesToRemove.Add(awtPointPathSetting.Setting.Name);
            DateTime awtRecordDate = DateTime.Now.AddHours(-1);
            AWT_FS.Program.AwtVHSAddPoints(s_domain, uis, wellFacility, awtRecordDate);
        }
        //

        public WellDTO AddWellsWithWellStatus(string facilityId, WellTypeId wellType, long wellStatus, string scadaType)
        {
            var dataConnection2 = new DataConnectionDTO();
            dataConnection2.ProductionDomain = "6000";
            dataConnection2.Site = "Testing";
            dataConnection2.Service = "UIS";
            dataConnection2.ScadaSourceType = Enums.ScadaSourceType.Concentrator;
            dataConnection2.ScadaSourceURL = "http://localhost:88";

            var wellConfigDTO = new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = facilityId,
                    FacilityId = facilityId,
                    //DataConnection = GetDefaultCygNetDataConnection(),
                    CommissionDate = DateTime.Today.AddYears(-2),
                    AssemblyAPI = facilityId,
                    SubAssemblyAPI = facilityId,
                    IntervalAPI = facilityId,
                    WellType = wellType,
                    WellStatusId = wellStatus
                })
            };

            if (scadaType.ToUpper() == "CYGNET")
            {
                wellConfigDTO.Well.DataConnection = GetDefaultCygNetDataConnection();
            }
            if (scadaType.ToUpper() == "CONCENTRATOR")
            {
                wellConfigDTO.Well.DataConnection = dataConnection2;
            }
            //wellConfigDTO.ModelConfig = ReturnFullyPopulatedModel(); // test fully configured model
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
            return addedWellConfig.Well;
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void GroupFilter_other_attributes()
        {
            EntityGridSettingDTO setting = new EntityGridSettingDTO();
            setting.GridSetting = new GridSettingDTO { PageSize = 20, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
            setting.EntityName = "r_WellStatus";
            int wellStatusRecords = DBEntityService.GetTableDataCount(setting);
            string facilityId;
            long wellCount = 1;

            List<Tuple<string, long, string>> createdWellsInfo = new List<Tuple<string, long, string>>();
            List<Tuple<string, WellTypeId, string>> wellsToCreate = new List<Tuple<string, WellTypeId, string>>();
            wellsToCreate.Add(Tuple.Create("NFWELL_", WellTypeId.NF, "Concentrator"));
            wellsToCreate.Add(Tuple.Create("RPOC_", WellTypeId.RRL, "CygNet"));
            wellsToCreate.Add(Tuple.Create("ESPWELL_", WellTypeId.ESP, "None"));
            wellsToCreate.Add(Tuple.Create("GASINJWELL_", WellTypeId.GInj, "None"));
            wellsToCreate.Add(Tuple.Create("GLWELL_", WellTypeId.GLift, "CygNet"));
            wellsToCreate.Add(Tuple.Create("WATERINJWELL_", WellTypeId.WInj, "None"));
            WellDTO well = new WellDTO();

            int addwellcount = wellsToCreate.Count * 3;
            int j = 0;
            Trace.WriteLine("Well Creation Started..");
            for (int i = 1; i < addwellcount + 1; i++)
            {
                facilityId = GetFacilityId(wellsToCreate.ElementAt(j).Item1, (i / wellsToCreate.Count) + 1);
                Trace.WriteLine("Creating Well : " + facilityId);
                well = AddWellsWithWellStatus(facilityId, wellsToCreate.ElementAt(j).Item2, wellCount, wellsToCreate.ElementAt(j).Item3);
                createdWellsInfo.Add(Tuple.Create(facilityId, wellCount++, wellsToCreate.ElementAt(j).Item3));
                _wellsToRemove.Add(well);

                if (j == wellsToCreate.Count - 1)
                    j = 0;
                else
                    j = j + 1;

                if (i > wellStatusRecords)
                { break; }
            }
            Trace.WriteLine("Added Wells  ");
            //Adding downtime to wells
            DateTime date = DateTime.Now.ToUniversalTime();
            var allWells = WellService.GetAllWells().ToList();
            Authenticate();
            // DownTime OFF --> Active:  8 Wells 
            var downTimeAdd = new WellDowntimeAdditionDTO();
            WellDowntimeDTO downTime = downTimeAdd.Downtime = new WellDowntimeDTO();
            List<long> wellsidoffactive = new List<long>();
            List<long> wellsidoffinactive = new List<long>();
            for (int i = 1; i <= allWells.Count - 10; i++)
            {
                wellsidoffactive.Add(allWells[i].Id);
            }
            for (int i = allWells.Count - 1; i > 15; i--)
            {
                wellsidoffinactive.Add(allWells[i].Id);
            }
            downTimeAdd.WellIds = wellsidoffactive.ToArray();
            downTime.CapturedDateTime = date;
            downTime.OutOfServiceCode = SurveillanceService.GetDownTimeStatusCodes().FirstOrDefault(x => x.DownTimeStatusCode == "CZ").DownTimeStatusId;
            SurveillanceService.AddDownTimeForWellWithGroupStatus(downTimeAdd);
            Trace.WriteLine("Added Out of Service Active ");
            var downTimeAdd2 = new WellDowntimeAdditionDTO();
            WellDowntimeDTO downTime2 = downTimeAdd2.Downtime = new WellDowntimeDTO();
            // DownTime OFF InActive -- 2 

            downTimeAdd2.WellIds = wellsidoffinactive.ToArray();
            downTime2.CapturedDateTime = date;
            downTime2.OutOfServiceCode = SurveillanceService.GetDownTimeStatusCodes().FirstOrDefault(x => x.DownTimeStatusCode == "AD").DownTimeStatusId;
            SurveillanceService.AddDownTimeForWellWithGroupStatus(downTimeAdd2);
            Trace.WriteLine("Added Out of Service InActive ");
            // Rest Are inService witjh ON code 
            // 8 In Service , 8  Out of service(active) ; 2 Out of SErvice (inactive )
            VerifyScadaTypeWellFilter(createdWellsInfo, new List<string>() { "CygNet" });
            VerifyScadaTypeWellFilter(createdWellsInfo, new List<string>() { "Concentrator" });
            VerifyScadaTypeWellFilter(createdWellsInfo, new List<string>() { "Concentrator", "CygNet" });
            VerifyScadaTypeWellFilter(createdWellsInfo, new List<string>() { "Concentrator", "None" });
            VerifyWellStatusWellFilter(createdWellsInfo, new List<string>() { "Active", "Completed" });
            VerifyWellStatusWellFilter(createdWellsInfo, new List<string>() { "Abandoned", "Permitted", "Sold", "Shut In" });
            VerifyWellStatusWellFilter(createdWellsInfo, new List<string>() { "Down", "Drilling", "Flowing" });
            //VerifyDowntimeWellFilter(createdWellsInfo, new List<string>() { "In Service" });
            //VerifyDowntimeWellFilter(createdWellsInfo, new List<string>() { "Out of Service (Active)" });
            //VerifyDowntimeWellFilter(createdWellsInfo, new List<string>() { "Out of Service (Inactive)" });
            //VerifyDowntimeWellFilter(createdWellsInfo, new List<string>() { "In Service", "Out of Service (Active)" });
            //VerifyDowntimeWellFilter(createdWellsInfo, new List<string>() { "In Service", "Out of Service (Inactive)" });
            //Quick work aorund to test filters ...
            VerifyDowntimeWellFilterActiveInactive(8, new List<string>() { "In Service" });
            VerifyDowntimeWellFilterActiveInactive(8, new List<string>() { "Out of Service (Active)" });
            VerifyDowntimeWellFilterActiveInactive(2, new List<string>() { "Out of Service (Inactive)" });
            VerifyDowntimeWellFilterActiveInactive(16, new List<string>() { "In Service", "Out of Service (Active)" });
            VerifyDowntimeWellFilterActiveInactive(10, new List<string>() { "In Service", "Out of Service (Inactive)" });
        }

        public void VerifyScadaTypeWellFilter(List<Tuple<string, long, string>> createdWellsInfo, List<string> scadaTypeList)
        {
            //long?[] assetIds = null;
            WellFilterDTO wellFilter = new WellFilterDTO();
            List<WellFilterValueDTO> scadaTypes = new List<WellFilterValueDTO>();

            foreach (string scadaType in scadaTypeList)
            {
                scadaTypes.Add(new WellFilterValueDTO() { Value = scadaType });
            }

            wellFilter.welScadaTypeTitle = "Scada Type";

            wellFilter.welScadaTypeValues = scadaTypes;
            WellDTO[] filtered_Wells = WellService.GetWellsByFilter(wellFilter);
            Trace.WriteLine("Verifying Wellfilter for SCADA type : " + scadaTypes);
            int count = 0;

            foreach (WellFilterValueDTO s in scadaTypes)
            {
                foreach (Tuple<string, long, string> t in createdWellsInfo)
                {
                    if ((t.Item3.ToString()).Contains(s.Value.ToString()))
                    {
                        count = count + 1;
                    }
                }
            }

            Assert.AreEqual(count, filtered_Wells.Count(), " wells count is incorrect");

            Trace.WriteLine(" ");
        }

        public void VerifyWellStatusWellFilter(List<Tuple<string, long, string>> createdWellsInfo, List<string> wellstatusList)
        {
            var allWells = WellService.GetAllWells().ToList();
            List<string> welllist = new List<string>(allWells.Count());
            foreach (WellDTO w in allWells)
            {
                int WellStatus = Convert.ToInt32(w.WellStatusId.Value);
                try
                {
                    switch (WellStatus)
                    {
                        case 1:
                            welllist.Add("N/A");
                            break;

                        case 2:
                            welllist.Add("Active");
                            break;

                        case 3:
                            welllist.Add("Down");
                            break;

                        case 4:
                            welllist.Add("Drilling");
                            break;

                        case 5:
                            welllist.Add("Flowing");
                            break;

                        case 6:
                            welllist.Add("InActive");
                            break;

                        case 7:
                            welllist.Add("Injecting");
                            break;

                        case 8:
                            welllist.Add("Plugged & Abandoned");
                            break;

                        case 9:
                            welllist.Add("Planned");
                            break;

                        case 10:
                            welllist.Add("Producing");
                            break;

                        case 11:
                            welllist.Add("Shut In");
                            break;

                        case 12:
                            welllist.Add("Sold");
                            break;

                        case 13:
                            welllist.Add("Temporarily Abandoned");
                            break;

                        case 14:
                            welllist.Add("Unknown");
                            break;

                        case 15:
                            welllist.Add("Abandoned");
                            break;

                        case 16:
                            welllist.Add("Completed");
                            break;

                        case 17:
                            welllist.Add("Permitted");
                            break;

                        case 18:
                            welllist.Add("Testing");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            WellFilterDTO wellFilter = new WellFilterDTO();
            List<WellFilterValueDTO> wellstatus = new List<WellFilterValueDTO>();

            foreach (string wellstatuss in wellstatusList)
            {
                wellstatus.Add(new WellFilterValueDTO() { Value = wellstatuss });
            }
            wellFilter.welStatusTitle = "Well Status";

            wellFilter.welStatusValues = wellstatus;

            WellDTO[] filtered_Wells = WellService.GetWellsByFilter(wellFilter);
            Trace.WriteLine("Verifying Wellfilter for Well Status : " + wellstatus);
            int count = 0;

            foreach (WellFilterValueDTO s in wellstatus)
            {
                foreach (string s1 in welllist)
                {
                    if (s.Value == s1.ToString())

                    {
                        count = count + 1;
                    }
                }
            }

            Assert.AreEqual(count, filtered_Wells.Count(), " wells count is incorrect");

            Trace.WriteLine(" ");
        }

        public void VerifyDowntimeWellFilterActiveInactive(int expcount, List<string> downtimelist)
        {
            WellFilterDTO wellFilter = new WellFilterDTO();
            List<WellFilterValueDTO> downtimes = new List<WellFilterValueDTO>();
            foreach (string downtime in downtimelist)
            {
                downtimes.Add(new WellFilterValueDTO() { Value = downtime });
            }
            wellFilter.welDownTimeTitle = "Downtime";

            wellFilter.welDownTimeValues = downtimes;

            WellDTO[] filtered_Wells = WellService.GetWellsByFilter(wellFilter);
            Trace.WriteLine("Verifying Wellfilter for Downtime : " + downtimes);
            Assert.AreEqual(expcount, filtered_Wells.Count(), $"Filter Count Did not match for {String.Join("-", downtimelist.ToArray())}");
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void WellTrend_OT()
        {
            string facilityId;

            // System setting
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = null;

            if (s_isRunningInATS)
                facilityId = "PGLWELL_00001";
            else
                facilityId = "PGLWELL_0001";

            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
            WellDTO wellOT = AddOTWell(facilityId);
            // WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = wellOT});
            Trace.WriteLine("Added OT Well " + wellOT.Name);
            systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.OT_GROUP_STATUS_EXTRA_UDCS);

            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well1 = allWells?.FirstOrDefault(w => w.Name.Equals(wellOT.Name));
            _wellsToRemove.Add(well1);

            WellFilterDTO wellbyFilters = new WellFilterDTO();
            WellGroupStatusQueryDTO Query = new WellGroupStatusQueryDTO();
            Query.WellType = well1.WellType;
            Query.WellFilter = wellbyFilters;

            SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
            string originalSetting = settingValue.StringValue;
            ConfigurableUDCInfo infos = new ConfigurableUDCInfo();
            infos.Parse(settingValue.StringValue);

            try
            {
                infos.Info.Add(infos.Info.Count() + 1, new ConfigurableUDC("OTUDCAddedByAPITest", "ROIL", "STB/d", UnitCategory.None, CygNetPointType.Analog, false));
                settingValue.StringValue = infos.GetSettingValue();
                SettingService.SaveSystemSetting(settingValue);

                WellGroupStatusDTO<object, object> WellGroups = SurveillanceServiceClient.GetWellGroupStatus(Query);

                WellStatusOptionalFieldInfoDTO[] aOptionalUDC = WellGroups.OptionalFieldInfo;
                object[] wellsStatus = WellGroups.OptionalFieldInfo;

                Trace.WriteLine("UDC Name : " + aOptionalUDC.FirstOrDefault(x => x.Name == "OTUDCAddedByAPITest").Name);

                var wellStatusValue = wellsStatus[0] as WellStatusOptionalFieldInfoDTO;

                Assert.IsNotNull(wellStatusValue.UDC, "Added udc is null");

                SurveillanceService.IssueCommandForSingleWell(wellOT.Id, WellCommand.DemandScan.ToString());
                OTWellStatusValueDTO wellGroupStatusdata = SurveillanceServiceClient.GetWellStatusData<OTWellStatusUnitDTO, OTWellStatusValueDTO>(wellOT.Id.ToString()).Value;
                if (wellGroupStatusdata.AlarmMessageString != null)
                {
                    Trace.WriteLine("Alarm message for added OTWell " + wellGroupStatusdata.AlarmMessageString);
                }
                Assert.IsNotNull(wellGroupStatusdata.OptionalField01, "Value for added UTC is null");
                Assert.AreEqual(wellGroupStatusdata.LastScanTime, wellGroupStatusdata.LastGoodScanTime, "Scan times should match for well " + wellGroupStatusdata.WellName);
                Assert.IsNotNull(wellGroupStatusdata.WellName, "Expected addded OT well name " + wellGroupStatusdata.WellName);

                WellQuantity quantity = WellQuantity.OilRateMeasured;

                int udcTagIdNumber = (int)quantity;
                string[] udcTagIdNumberStr = { udcTagIdNumber.ToString() };

                CygNetTrendDTO wellTrend = SurveillanceService.GetCygNetTrendsByUDCAndDateRange(udcTagIdNumberStr, wellOT.Id.ToString(), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-29)), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime())).FirstOrDefault();

                if (wellTrend != null)
                {
                    Trace.WriteLine("Verifying point udc added for OT well in US unit system" + wellTrend.PointUDC.ToString());

                    Assert.AreEqual("ROIL", wellTrend.PointUDC, "UDCs do not match for well trend.");
                }
                {
                    Trace.WriteLine("Verifying point udc value added for OT well in US unit system");
                    Assert.IsNotNull(wellTrend.PointValues[0].Value, "UDCs point value is null");
                    Trace.WriteLine("Value for point udc added in US unit system" + wellTrend.PointValues[0].Value);
                }
                {
                    Trace.WriteLine("Verifying scan successfull for OT well in US unit system");
                    Assert.AreEqual("Success", wellTrend.ErrorMessage, "Scan was not successful");
                    Trace.WriteLine("Scan was " + wellTrend.ErrorMessage);
                }
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");
                CygNetTrendDTO wellTrend1 = SurveillanceService.GetCygNetTrendsByUDCAndDateRange(udcTagIdNumberStr, wellOT.Id.ToString(), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-29)), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime())).FirstOrDefault();

                if (wellTrend1 != null)
                {
                    Trace.WriteLine("Verifying point udc added for OT well in metric unit system");
                    Assert.AreEqual("ROIL", wellTrend.PointUDC, "UDCs do not match for well trend.");
                }
                {
                    Trace.WriteLine("Verifying point udc value added for OT well in metric unit system");
                    Assert.IsNotNull(wellTrend1.PointValues[0].Value, "UDCs point value is null");

                    Trace.WriteLine("Value for point udc added in metric unit system" + wellTrend1.PointValues[0].Value);
                }
                {
                    Trace.WriteLine("Verifying scan successfull for OT well in metric unit system");
                    Assert.AreEqual("Success", wellTrend1.ErrorMessage, "Scan was not successful");
                    Trace.WriteLine("Scan was " + wellTrend1.ErrorMessage);
                }
            }
            finally
            {
                settingValue.StringValue = originalSetting;
                SettingService.SaveSystemSetting(settingValue);
            }
        }


        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GroupStatusAlarm_commfailure()
        {
            WellDTO wellRRL = null;
            string facilityId;
            facilityId = GetFacilityId("RPOC_", 30);
            try
            {
                {
                    wellRRL = AddRRLWell(facilityId);
                    List<WellDTO> wells = null;
                    if (_addedWells != null && _addedWells.Count > 0)
                    {
                        wells = _addedWells;
                    }
                    else
                    {
                        wells = WellService.GetAllWells().ToList();
                    }
                    _wellsToRemove.Add(wellRRL);

                    //Well Column Filter

                    WellGroupStatusColumnFilterDTO columnFilter = new WellGroupStatusColumnFilterDTO();

                    //Verifying Whether FAILED: Comm Unable to Connect Alarm is generated or not
                    if (wellRRL.FacilityId.Contains(facilityId))
                    {
                        SurveillanceService.IssueCommandForSingleWell(wellRRL.Id, WellCommand.DemandScan.ToString());
                        RRLWellStatusValueDTO wellGroupStatusdata = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(wellRRL.Id.ToString()).Value;
                        Assert.AreEqual(wellRRL.Name, wellGroupStatusdata.WellName);
                        var value = wellGroupStatusdata.AlarmStatus;
                        Trace.WriteLine($"Alarm message obtained is {value.ToString()}");
                        Assert.IsTrue(value.Contains("Comm Failure"), "FAILED: Comm Unable to Connect Alarm is not generated");
                    }
                }
            }
            finally
            {
                _wellsToRemove.Add(wellRRL);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void VerifyCommandLockOut()
        {
            // Specify Lockout for all commands
            string setAllToHighRiskCommand = "DemandScan|LO_STATUS|False;StartRPC|C_STARTWEL|False;StopAndLeaveDown|C_FORCEOFF|False;IdleRPC|C_IDLE|False;ControlTransfer|C_CTRLXFER|False;SoftwareTimer|C_SWTIMER|False;ClearErrors|C_CLEARERR|False;ContinuousRun|C_RUNCONT|False";
            CheckLockoutForCommandName(setAllToHighRiskCommand);
            try
            {
                SetLockoutPeriod("Single", 8);
                SetLockoutPeriod("Multi", 45);
                setAllToHighRiskCommand = "DemandScan|LO_STATUS|True;StartRPC|C_STARTWEL|True;StopAndLeaveDown|C_FORCEOFF|True;IdleRPC|C_IDLE|True;ControlTransfer|C_CTRLXFER|True;SoftwareTimer|C_SWTIMER|True;ClearErrors|C_CLEARERR|True;ContinuousRun|C_RUNCONT|True";
                CheckLockoutForCommandName(setAllToHighRiskCommand);
                AddSingleWell(GetFacilityId("RPOC_", 1), false);
                IssueCommandForOSingleWellLockout(GetFacilityId("RPOC_", 1), true, 8000);
                RemoveWellOnDemand(DefaultWellName);
                AddSingleWell(GetFacilityId("SAM_", 1), false);
                IssueCommandForOSingleWellLockout(GetFacilityId("SAM_", 1), true, 8000);
                RemoveWellOnDemand(DefaultWellName);
                // Clear the Well commands Settings after test was run
                setAllToHighRiskCommand = "DemandScan|LO_STATUS|False;StartRPC|C_STARTWEL|False;StopAndLeaveDown|C_FORCEOFF|False;IdleRPC|C_IDLE|False;ControlTransfer|C_CTRLXFER|False;SoftwareTimer|C_SWTIMER|False;ClearErrors|C_CLEARERR|False;ContinuousRun|C_RUNCONT|False";
                CheckLockoutForCommandName(setAllToHighRiskCommand);
            }
            catch (Exception ex)
            {
                // Clear the Well commands Settings after test was run
                Trace.WriteLine("Commnd Set " + setAllToHighRiskCommand);
                setAllToHighRiskCommand = "DemandScan|LO_STATUS|False;StartRPC|C_STARTWEL|False;StopAndLeaveDown|C_FORCEOFF|False;IdleRPC|C_IDLE|False;ControlTransfer|C_CTRLXFER|False;SoftwareTimer|C_SWTIMER|False;ClearErrors|C_CLEARERR|False;ContinuousRun|C_RUNCONT|False";
                CheckLockoutForCommandName(setAllToHighRiskCommand);
                throw ex;
            }
            finally
            {
                // Clear the Well commands Settings after test was run
                setAllToHighRiskCommand = "DemandScan|LO_STATUS|False;StartRPC|C_STARTWEL|False;StopAndLeaveDown|C_FORCEOFF|False;IdleRPC|C_IDLE|False;ControlTransfer|C_CTRLXFER|False;SoftwareTimer|C_SWTIMER|False;ClearErrors|C_CLEARERR|False;ContinuousRun|C_RUNCONT|False";
                CheckLockoutForCommandName(setAllToHighRiskCommand);
            }
        }

        public void SetLockoutPeriod(string type, double? value)
        {
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = null;
            if (type.ToUpper() == "SINGLE")
            {
                systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.LOCKOUT_SINGLE_COMMAND_DELAY);
            }
            else if (type.ToUpper() == "MULTI")
            {
                systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.LOCKOUT_MULTI_COMMAND_DELAY);
            }
            SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
            settingValue.NumericValue = value;
            SettingService.SaveSystemSetting(settingValue);
            settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
            Assert.AreEqual(value, settingValue.NumericValue, "Unable to Change the Lockout Setting Value");
        }

        public void CheckLockoutForCommandName(string CommandName)
        {
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = null;
            systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.WELL_COMMAND_MAPPING);
            SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
            settingValue.ChangeDate = DateTime.UtcNow;
            settingValue.ChangeUser = AuthenticatedUser.Name;
            settingValue.Setting = systemSettings;
            settingValue.StringValue = CommandName;
            SystemSettingDTO[] settingValues = new SystemSettingDTO[] { settingValue };
            SettingService.SaveSystemSettings(settingValues);
        }

        public void IssueCommandForRelativeFacilities(string wellName, WellTypeId wellTypeId)
        {
            AddUpdateRelativeFacilitySystemSettingForWellCommands(wellName + "Mapping", wellTypeId, null);

            string facilityId;
            facilityId = GetFacilityId("GLWELL_", 1);
            _addedWells = new List<WellDTO>();

            WellDTO well = SetDefaultFluidType(new WellDTO() { Name = wellName + "TestWell", FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today, WellType = WellTypeId.GLift });
            var relativeMappingNames = SettingService.GetRelativeFacilityMappingNames();
            well.RelativeFacilityMapping = relativeMappingNames.FirstOrDefault(x => x.Item2 == wellTypeId).Item1.ToString();
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
            WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(well.Name));
            Assert.IsNotNull(addedWell);
            _wellsToRemove.Add(addedWell);
            _addedWells.Add(addedWell);

            bool isScanSuccessful = SurveillanceService.IssueCommandForSingleWell(addedWell.Id, "DemandScan");
            Assert.IsTrue(isScanSuccessful, "Relative Facilities - Issue with Scan command");

            RemoveRelativeFacilitySystemSetting();
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForRelativeFacilities_GLiftWell()
        {
            if (s_isRunningInATS)
            {
                Trace.WriteLine("Test requires additional configuration to run in ATS. Skipping test for timebeing.");
                return;
            }
            IssueCommandForRelativeFacilities("GL", WellTypeId.GLift);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForRelativeFacilities_ESPWell()
        {
            if (s_isRunningInATS)
            {
                Trace.WriteLine("Test requires additional configuration to run in ATS. Skipping test for timebeing.");
                return;
            }
            IssueCommandForRelativeFacilities("ESP", WellTypeId.GLift);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForRelativeFacilities_PLiftWell()
        {
            if (s_isRunningInATS)
            {
                Trace.WriteLine("Test requires additional configuration to run in ATS. Skipping test for timebeing.");
                return;
            }
            IssueCommandForRelativeFacilities("PL", WellTypeId.GLift);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void IssueCommandForRelativeFacilities_NFWell()
        {
            if (s_isRunningInATS)
            {
                Trace.WriteLine("Test requires additional configuration to run in ATS. Skipping test for timebeing.");
                return;
            }
            IssueCommandForRelativeFacilities("NF", WellTypeId.GLift);
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void VerifyRelativeFacilityMappingQuantity()
        {
            if (s_isRunningInATS)
            {
                Trace.WriteLine("Test requires additional configuration to run in ATS. Skipping test for timebeing.");
                return;
            }
            string gsfile = "BSS\\TEST\\LocalCygNet.gsf";
            try
            {

                string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
                Trace.WriteLine($"New gsf file :{ConfigurationManager.AppSettings.Get("RelativeFacilityGlobalSettingsPath")}"); ;
                //string xmlstring = ReadFromXMlFile(Path + "RRLAdditionalUDC.xml");
                //SetValuesInSystemSettings(SettingServiceStringConstants.RRL_GROUP_STATUS_EXTRA_UDCS, xmlstring);
                string xmlstring = ReadFromXMlFile(Path + "GLRelativeFacilityMappingUDC.xml");
                SetValuesInSystemSettings(SettingServiceStringConstants.RELATIVE_FACILITY_MAPPING_SCHEMES, xmlstring);
                //Update Rel Mapping for Quantities


                Dictionary<WellQuantity, string> wellQuantities = new Dictionary<WellQuantity, string>();
                wellQuantities[WellQuantity.TubingPressure] = "TANK";
                AddUpdateRelativeFacilitySystemSettingForWellQuantities("GLMapping", WellTypeId.GLift, wellQuantities, gsfile);
                // Check on Group status for Relative Facility Quantity &  UDC Values
                //Write Values to UDC Tubing Presure for  Faciliteis
                FacilityTag facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("TANK_", 2));
                SetPointValue(facilityTag, "PRTUBXIN", "500", DateTime.Now.AddMinutes(-1));
                Trace.WriteLine("Child Facility TANK  was Set to Valueof 500 psi for Tubing Prssure.");
                facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("GLWELL_", 1));
                SetPointValue(facilityTag, "PRTUBXIN", "400", DateTime.Now.AddMinutes(-1));
                Trace.WriteLine("Default Facility WELL  was Set to Value : 400 psi for Tubing Prssure.");
                // Add New Non RRL Well
                WellDTO addedwell = AddNonRRLWell(GetFacilityId("GLWELL_", 1), WellTypeId.GLift, false);
                _wellsToRemove.Add(addedwell);
                WellConfigDTO updateconfig = WellConfigurationService.GetWellConfig(addedwell.Id.ToString());
                updateconfig.Well.WellDepthDatumId = 1;
                Guid mapid;
                CheckIfRelativeFacilityMappingExistForWellType(WellTypeId.GLift, out mapid);

                updateconfig.Well.RelativeFacilityMapping = mapid.ToString();
                WellConfigurationService.UpdateWellConfig(updateconfig);

                //    GLWellStatusValueDTO  glvaluedto = SurveillanceService.GetWellStat(addedwell.Id.ToString())
                WellGroupStatusQueryDTO newGroup = GroupStatusQuery(WellTypeId.GLift);
                var wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<GLWellStatusUnitDTO, GLWellStatusValueDTO>(newGroup);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == GetFacilityId("GLWELL_", 1)), "Failed to get the added well");
                GLWellStatusValueDTO us_glWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(addedwell.Id.ToString(),
                    SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as GLWellStatusValueDTO;
                Assert.AreEqual("500", us_glWellStatus.TubingPressure.ToString(), "Tubing Pressure Value matchd with Relative Tank Facility Relation as TANK");
                wellQuantities[WellQuantity.TubingPressure] = "";
                AddUpdateRelativeFacilitySystemSettingForWellQuantities("GLMapping", WellTypeId.GLift, wellQuantities, gsfile);
                SurveillanceService.IssueCommandForSingleWell(addedwell.Id, WellCommand.DemandScan.ToString());
                Thread.Sleep(2000);
                us_glWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(addedwell.Id.ToString(),
                    SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as GLWellStatusValueDTO;
                Assert.IsNotNull(us_glWellStatus.TubingPressure.ToString(), "Tubing Pressure Value was NULL when defaulted to default facility.");
                Assert.AreNotEqual("500", us_glWellStatus.TubingPressure.ToString(), "Tubing Pressure Value was differnt from TANK Facility.");
                //Check on Well Status for Relative Facility Quantity &  UDC Values
            }
            finally
            {
                RemoveRelativeFacilitySystemSetting();

            }
            //Check on Well  Trend for Relative Facility Quantity &  UDC Values


        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void VerifyRelativeFacilityMappingAdditionalUDC()
        {
            if (s_isRunningInATS)
            {
                Trace.WriteLine("Test requires additional configuration to run in ATS. Skipping test for timebeing.");
                return;
            }
            string gsfile = "BSS\\TEST\\LocalCygNet.gsf";
            try
            {

                string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
                Trace.WriteLine($"New gsf file :{ConfigurationManager.AppSettings.Get("RelativeFacilityGlobalSettingsPath")}"); ;
                string xmlstring = ReadFromXMlFile(Path + "GLAdditionalUDC.xml");
                SetValuesInSystemSettings(SettingServiceStringConstants.GL_GROUP_STATUS_EXTRA_UDCS, xmlstring);
                xmlstring = ReadFromXMlFile(Path + "GLRelativeFacilityMappingUDC.xml");
                SetValuesInSystemSettings(SettingServiceStringConstants.RELATIVE_FACILITY_MAPPING_SCHEMES, xmlstring);
                //Update Rel Mapping for Quantities

                int relfacvalue = 500;
                int defaultfacval = 300;

                AddUpdateRelativeFacilitySystemSettingForAdditionalUDCs("GLMapping", WellTypeId.GLift, gsfile, "TANKPR", "TANK");
                // Check on Group status for Relative Facility Quantity &  UDC Values
                //Write Values to UDC Tubing Presure for  Faciliteis
                FacilityTag facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("TANK_", 2));
                SetPointValue(facilityTag, "TANKPR", relfacvalue.ToString(), DateTime.Now.AddMinutes(-1));
                Trace.WriteLine($"Child Facility TANK  was Set to Valueof {relfacvalue.ToString()} psia for Additional UDC.");

                // Add New Non RRL Well
                WellDTO addedwell = AddNonRRLWell(GetFacilityId("GLWELL_", 1), WellTypeId.GLift, false);
                _wellsToRemove.Add(addedwell);
                WellConfigDTO updateconfig = WellConfigurationService.GetWellConfig(addedwell.Id.ToString());
                updateconfig.Well.WellDepthDatumId = 1;
                Guid mapid;
                CheckIfRelativeFacilityMappingExistForWellType(WellTypeId.GLift, out mapid);

                updateconfig.Well.RelativeFacilityMapping = mapid.ToString();
                WellConfigurationService.UpdateWellConfig(updateconfig);

                //    GLWellStatusValueDTO  glvaluedto = SurveillanceService.GetWellStat(addedwell.Id.ToString())
                WellGroupStatusQueryDTO newGroup = GroupStatusQuery(WellTypeId.GLift);
                var wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<GLWellStatusUnitDTO, GLWellStatusValueDTO>(newGroup);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == GetFacilityId("GLWELL_", 1)), "Failed to get the added well");
                GLWellStatusValueDTO us_glWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(addedwell.Id.ToString(),
                    SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as GLWellStatusValueDTO;
                Assert.AreEqual(relfacvalue.ToString(), us_glWellStatus.OptionalField01.ToString(), "Additional UDC Value matchd with Relative Tank Facility Relation as TANK");
                facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("GLWELL_", 1));
                SetPointValue(facilityTag, "TANKPR", defaultfacval.ToString(), DateTime.Now.AddMinutes(-1));
                Trace.WriteLine($"Default Facility WELL  was Set to Value : {defaultfacval.ToString()}psia for Additional UDC.");
                AddUpdateRelativeFacilitySystemSettingForAdditionalUDCs("GLMapping", WellTypeId.GLift, gsfile, "TANKPR", "");
                SurveillanceService.IssueCommandForSingleWell(addedwell.Id, WellCommand.DemandScan.ToString());
                Thread.Sleep(2000);
                us_glWellStatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(addedwell.Id.ToString(),
                    SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as GLWellStatusValueDTO;
                Assert.IsNotNull(us_glWellStatus.OptionalField01.ToString(), "Additional UDC Value NULL when defaulted to default facility.");
                Assert.AreEqual(defaultfacval.ToString(), us_glWellStatus.OptionalField01.ToString(), "TAdditional UDC Value was updated when relation was made as null to default facility");
                //Check on Well Status for Relative Facility Quantity &  UDC Values
            }
            finally
            {
                SetValuesInSystemSettings(SettingServiceStringConstants.GL_GROUP_STATUS_EXTRA_UDCS, "");
                RemoveRelativeFacilitySystemSetting();

            }
            //Check on Well  Trend for Relative Facility Quantity &  UDC Values


        }

        public void AddNewFacility(string facbase, string factype)
        {
            FacilityRecord facRec = new FacilityRecord();
            FacilityTag facilityTag = new FacilityTag(s_site, s_cvsService, facbase);
            facRec.Tag = facilityTag;
            facRec.Description = GetFacilityId(facbase, 1);
            facRec.Type = factype;
            CygNet.API.Facilities.Client facclient = new CygNet.API.Facilities.Client(new CygNet.Data.Core.DomainSiteService(Convert.ToUInt16(s_domain), s_site, "UIS"));
            facclient.AddFacility(facRec);


        }

        public static void SetPointValue(FacilityTag facility, string udc, object value, DateTime dateTime)
        {
            var realTimeClient = new RealtimeClient(new CygNet.Data.Core.DomainSiteService(Convert.ToUInt16(s_domain), s_site, "UIS"));
            try
            {
                realTimeClient.SetPointValue(new PointTag(facility, udc), value, dateTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set value for '{facility.FacilityId}:{udc}'. " + ex.Message);
                throw;
            }
        }

        public static void UpdateAlarmPointValue(CygNet.COMAPI.Interfaces.IUisClient uisClient, FacilityTag facility, string udc, string almval)
        {
            var pnt = new DomainSiteService(uisClient.GetAssociatedPNT().GetDomainSiteService());
            var pntClient = new CygNet.API.Points.ConfigClient(pnt);
            PointConfigRecord existingPointRec = pntClient.ReadPointRecord(new PointTag(facility, udc));
            PointConfigRecord newPointRec = existingPointRec.DeepCopy();
            newPointRec.MaxSetPoint = 10000;
            newPointRec.MinSetPoint = 0;
            newPointRec.ConfigBitCalculationParameter1[4] = almval;
            newPointRec.ConfigBitReportCAS[4] = true;
            newPointRec.ConfigBitEnabled[4] = true;
            pntClient.UpdatePointRecord(newPointRec);
        }



        public void AddUDCTest()
        {
            AddNewPoint("RRL");
            AddNewPoint("GL");
            AddNewPoint("ESP");
            AddNewPoint("NFW");
            AddNewPoint("GASINJ");
            AddNewPoint("WATERINJ");
            AddNewPoint("PGL");
            //Set Values in UDC
            FacilityTag facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("RPOC_", 1));
            SetPointValue(facilityTag, "RRLUSER", AuthenticatedUser.Name, DateTime.Now);
            //ESP Well Type
            facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("ESPWELL_", 1));
            SetPointValue(facilityTag, "ESPUSER", AuthenticatedUser.Name, DateTime.Now);

            facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("GLWELL_", 1));
            SetPointValue(facilityTag, "GLUSER", AuthenticatedUser.Name, DateTime.Now);

            facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("NFWWELL_", 1));
            SetPointValue(facilityTag, "NFWUSER", AuthenticatedUser.Name, DateTime.Now);

            facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("GASINJWELL_", 1));
            SetPointValue(facilityTag, "GIUSER", AuthenticatedUser.Name, DateTime.Now);

            facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("WATERINJWELL_", 1));
            SetPointValue(facilityTag, "WIUSER", AuthenticatedUser.Name, DateTime.Now);

            facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("PGLWELL_", 1));
            SetPointValue(facilityTag, "PLUSER", AuthenticatedUser.Name, DateTime.Now);
        }

        public void AddNewPoint(string liftType)
        {
            ICollection<FacilityTag> factags = null;
            switch (liftType)
            {
                case "RRL":
                    {
                        factags = GetFacilitiesList(liftType, GetUisClient(), "WELL", "RPOC_");
                        break;
                    }
                case "GL":
                    {
                        factags = GetFacilitiesList(liftType, GetUisClient(), "WELL", "GLWELL_");
                        break;
                    }
                case "ESP":
                    {
                        factags = GetFacilitiesList(liftType, GetUisClient(), "WELL", "ESPWELL_");
                        break;
                    }
                case "NFW":
                    {
                        factags = GetFacilitiesList(liftType, GetUisClient(), "WELL", "NFWWELL_");
                        break;
                    }
                case "PGL":
                    {
                        factags = GetFacilitiesList(liftType, GetUisClient(), "WELL", "PGLWELL_");
                        break;
                    }
                case "GASINJ":
                    {
                        factags = GetFacilitiesList(liftType, GetUisClient(), "WELL", "GASINJWELL_");
                        break;
                    }
                case "WATERINJ":
                    {
                        factags = GetFacilitiesList(liftType, GetUisClient(), "WELL", "WATERINJWELL_");
                        break;
                    }
                default:
                    break;
            }
            ConfigureFacilityPoints(GetUisClient(), factags.ToList(), liftType);
        }
        public void ConfigureFacilityPoints(CygNet.COMAPI.Interfaces.IUisClient uisClient, List<FacilityTag> facTagList, string liftType)
        {
            LiftInitialize();
            var pointsList = GetLiftSpecificPointsList(liftType);

            if (pointsList != null && pointsList.Count > 0)
            {
                var pnt = new DomainSiteService(uisClient.GetAssociatedPNT().GetDomainSiteService());
                var pntClient = new CygNet.API.Points.ConfigClient(pnt);

                Trace.WriteLine($"Begin Add/Update '{pointsList.Count}' points for {liftType} facilites.");
                foreach (FacilityTag facTag in facTagList)
                {
                    int addedPoints = 0;
                    int updatedPoints = 0;
                    bool shouldUpdateExisting = true;
                    foreach (SinglePoint point in pointsList)
                    {
                        var pointTag = new PointTag(facTag, point.UDC);
                        var newPointRec = new PointConfigRecord();
                        PointConfigRecord existingPointRec = null;
                        if (pntClient.PointRecordExists(pointTag))
                        {
                            existingPointRec = pntClient.ReadPointRecord(pointTag);
                            newPointRec = existingPointRec.DeepCopy();
                        }
                        else
                        {
                            newPointRec.Tag = pointTag;
                        }

                        newPointRec.ReportVHS = true;
                        newPointRec.ForceHistoryOnUpdate = true;
                        newPointRec.PointDataType = point.PointType;
                        newPointRec.HistoryOnLineDays = Convert.ToUInt32(365);
                        if (point.Description.Length > 24)
                        {
                            point.Description = point.Description.Substring(0, 23);
                        }
                        newPointRec.Description = point.Description;
                        newPointRec.LongDescription = point.Description;
                        newPointRec.IsActive = true;
                        newPointRec.PrimaryUnits = point.PrimaryUnits;
                        newPointRec.AlternateUnits = point.AlternateUnits;
                        newPointRec.PointScheme = GetPointScheme();


                        if (existingPointRec == null)
                        {
                            pntClient.AddPointRecord(newPointRec);
                            addedPoints += 1;
                        }
                        else if (shouldUpdateExisting && !newPointRec.Equals(existingPointRec))
                        {
                            pntClient.UpdatePointRecord(newPointRec);
                            updatedPoints += 1;
                        }

                        // Wait for sometime to ensure that point is added in the PNT
                        Thread.Sleep(200);
                    }
                    Console.WriteLine($"Added '{addedPoints}' and updated '{updatedPoints}' points for '{facTag.GetFacilityTag()}'.");
                }
                Console.WriteLine($"Finished Add/Update '{pointsList.Count}' points for {liftType} facilites.");
            }

        }

        public void UpdateFacilityPoints(CygNet.COMAPI.Interfaces.IUisClient uisClient, FacilityTag facTag, string liftType)
        {
            var pnt = new DomainSiteService(uisClient.GetAssociatedPNT().GetDomainSiteService());
            var pntClient = new CygNet.API.Points.ConfigClient(pnt);
            string filter = String.Format("facilityid='{0}'", facTag.FacilityId);
            System.Threading.CancellationToken token;
            var pntTags = pntClient.GetPointTagList(filter, token);
            foreach (PointTag pntTag in pntTags)
            {
                string.Format($"Point value {pntTag.UDC}");
            }

        }

        //SetPoint test for PCP Well
        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void PCP_SetPointsConfigurationTest()
        {
            string facilityId = GetFacilityId("WFTA1K_", 1);
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = "PCPTest01", FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.PCP }) });
            WellDTO[] allWells = WellService.GetAllWells();
            Assert.IsNotNull(allWells, "Failed to get wells.");
            WellDTO well = allWells.FirstOrDefault(w => w.Name.Equals("PCPTest01"));
            Assert.IsNotNull(well, "Failed to add well.");
            _wellsToRemove.Add(well);
            Trace.WriteLine($"Added new well id '{well.Id}' associated with facility id '{facilityId}'.");
            // System setting
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.SET_POINT_CONFIG);
            SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);

            //Set Point config
            string Str1 = settingValue.StringValue;

            try
            {
                string Str2 = Str1.Substring(0, Str1.Length - "</DeviceTypes>".Length);
                StringBuilder sb = new StringBuilder(Str2);
                sb.Append("<DeviceType Name =\"WFTA1K\"><SetPointDG Name =\"Control Parameters\" IsPrimary=\"true\" Type=\"CtrlParm\" UISGetCommandName =\"GETCTRLPAR\" UISSendCommandName=\"SENDCTRPAR\"><Ordinal>0</Ordinal></SetPointDG><SetPointDG Name =\"Flow Parameters\" IsPrimary =\"false\" Type=\"FlwParm\" UISGetCommandName =\"GETFLWPAR\" UISSendCommandName=\"SENDFLWPAR\" ><Ordinal>0</Ordinal></SetPointDG><SetPointDG Name =\"Motor Settings\" IsPrimary =\"false\" Type =\"MtrStng\" UISGetCommandName =\"GETMTRSET\" UISSendCommandName =\"SENDMTRSET\"><Ordinal>0</Ordinal></SetPointDG></DeviceType></DeviceTypes>");
                //sb.Append("<DeviceType  Name =\"WFTA1K\"><SetPointDG Name= \"Control Parameters\" IsPrimary=\"true\" Type=\"CtrlParm\" UISGetCommandName=\"GETCTRLPAR\" UISSendCommandName=\"SENDCTRPAR\" /><SetPointDG Name=\"Control Parameters\" IsPrimary=\"false\" Type=\"CtrlParms\" UISGetCommandName=\"GETCTRLPAR\" UISSendCommandName=\"SENDCTRPAR\" /><SetPointDG Name=\"Sensor Configuration\" IsPrimary=\"false\" Type=\"SensSetup\" UISGetCommandName=\"GETSENSET\" UISSendCommandName=\"SENDSENSET\" /><SetPointDG Name=\"VFD Configuration\" IsPrimary=\"false\" Type=\"VSDCfg\" UISGetCommandName=\"GETVFDCFG\" UISSendCommandName=\"SENDVFDCFG\" /><SetPointDG Name=\"Energy Management\" IsPrimary=\"false\" Type=\"PkEgyMgtCg\" UISGetCommandName=\"GETENGMGM\" UISSendCommandName=\"SENDENGMGM\" /><SetPointDG Name=\"Firmware Information\" IsPrimary=\"false\" Type=\"CtrlInfo\" UISGetCommandName=\"GETFIRMINF\" /></DeviceType></DeviceTypes>");
                settingValue.StringValue = sb.ToString();

                //"savesystemsetting" api is saving a system setting values
                SettingService.SaveSystemSetting(settingValue);

                //"GetSystemSettingByName" api is getting a system setting name
                settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
                SetPointsDGDTO setPointsDG = SurveillanceService.GetSetPointsDataGroups(well.Id.ToString());
                Assert.IsNotNull(setPointsDG, "Invalid SetPointsDGDTO object.");
                Assert.IsNotNull(setPointsDG.PrimarySPDgType, "Invalid primary set point data group type.");
                Assert.IsNotNull(setPointsDG.OtherSetPointsDGList, "Invalid other set point data group list.");
                //Control Parameters dg type
                SetPointsGetSendAPITest(facilityId, well, setPointsDG.PrimarySPDgType);
                //Flow Parameters dg type         
                SetPointsGetSendAPITest(facilityId, well, setPointsDG.OtherSetPointsDGList[0].DgType);
                //Motor string dg type
                SetPointsGetSendAPITest(facilityId, well, setPointsDG.OtherSetPointsDGList[1].DgType);

            }
            catch (Exception e)
            {
                Assert.Fail(e.Message); ;
                Trace.WriteLine(e.Message);
            }
            finally
            {
                //"savesystemsetting" api is saving a system setting values
                //Reverting back to Original System Setting (SetPoint Config)
                settingValue.StringValue = Str1;
                SettingService.SaveSystemSetting(settingValue);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void VerifyGetWellStatusCommunicationCode()
        {

            WellDTO well = AddRRLWell(GetFacilityId("RPOC_", 1));
            SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(SettingServiceStringConstants.ESP_GROUP_STATUS_EXTRA_UDCS);
            string originalSetting = settingValue.StringValue;
            ConfigurableUDCInfo infos = new ConfigurableUDCInfo();
            object welludcmap = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.WELL_UDC_CONFIG);
            try
            {
                // USer Story : FRWM-5752  : API story : FRWM-6100
                // Create New Well and Run Well through differnt Status
                //1.Running  2. Well Down 3 : Not Communicating 4.Stopped 5. Cygnet alram 6 ForeSite Alarm
                WellFilterDTO filters = new WellFilterDTO();
                //************************* 1.  Verify Communication Failure ***********************************************
                WellDTO wellccommfail = AddRRLWell(GetFacilityId("RPOC_", 30));
                _wellsToRemove.Add(wellccommfail);
                _wellsToRemove.Add(well);
                WellConfigDTO updatewellcfg = WellConfigurationService.GetWellConfig(wellccommfail.Id.ToString());
                updatewellcfg.Well.SurfaceLatitude = 19;
                updatewellcfg.Well.SurfaceLongitude = 80;
                WellConfigurationService.UpdateWellConfig(updatewellcfg);

                updatewellcfg = WellConfigurationService.GetWellConfig(well.Id.ToString());
                updatewellcfg.Well.SurfaceLatitude = 19;
                updatewellcfg.Well.SurfaceLongitude = 78;
                updatewellcfg.Well.WellDepthDatumId = 1;
                SurveillanceService.IssueCommandForSingleWell(wellccommfail.Id, WellCommand.DemandScan.ToString());
                WellStatusCommunicationDTO[] wellcommdto = SurveillanceService.GetWellStatusCommunication(filters);
                WellStatusCommunicationDTO commfaillwel = wellcommdto.FirstOrDefault(x => x.WellName == GetFacilityId("RPOC_", 30));
                Assert.AreEqual(WellStatusCommunicationCode.CommunicationFailure, commfaillwel.WellStatusCommunication, "GetWellStatusCommunicationCode is incorrect");
                //************************* 2. Verify Running Status ***********************************************
                WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellccommfail.Id.ToString()).Units;
                //add new wellTestData
                var testDataDTO = new WellTestDTO
                {
                    WellId = well.Id,
                    AverageCasingPressure = 200,
                    AverageFluidAbovePump = 5100,
                    AverageTubingPressure = 200,
                    AverageTubingTemperature = 80,
                    Gas = 300,
                    GasGravity = 0.65m,
                    Oil = 300,
                    OilGravity = 35,
                    PumpEfficiency = 0,
                    PumpingHours = 10,
                    SPTCode = 1,
                    TestDuration = 3,
                    Water = 300,
                    WaterGravity = 1.05m
                };
                testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime());
                WellTestDataService.AddWellTestData(new WellTestAndUnitsDTO(units, testDataDTO));

                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                wellcommdto = SurveillanceService.GetWellStatusCommunication(filters);
                WellStatusCommunicationDTO normalwel = wellcommdto.FirstOrDefault(x => x.WellName == GetFacilityId("RPOC_", 1));
                Assert.AreEqual(WellStatusCommunicationCode.RunningNormal, normalwel.WellStatusCommunication, "GetWellStatusCommunicationCode is incorrect");
                //************************* 3. Verify Well Stopped Statusa ***********************************************
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.StopAndLeaveDown.ToString());
                wellcommdto = SurveillanceService.GetWellStatusCommunication(filters);
                normalwel = wellcommdto.FirstOrDefault(x => x.WellName == GetFacilityId("RPOC_", 1));
                Assert.AreEqual(WellStatusCommunicationCode.Down, normalwel.WellStatusCommunication, "GetWellStatusCommunicationCode is incorrect");
                //************************* 4. Verify Down Well Code ***********************************************
                WellDowntimeAdditionDTO welldwtadddto = new WellDowntimeAdditionDTO();
                welldwtadddto.WellIds = new long[] { normalwel.WellId };
                WellDowntimeDTO welldwtdto = new WellDowntimeDTO();
                welldwtdto.OutOfServiceCode = 55;
                welldwtdto.CapturedDateTime = DateTime.Now.AddDays(-30).ToUniversalTime();
                welldwtadddto.Downtime = welldwtdto;
                SurveillanceService.AddDownTimeForWellWithGroupStatus(welldwtadddto);
                wellcommdto = SurveillanceService.GetWellStatusCommunication(filters);
                normalwel = wellcommdto.FirstOrDefault(x => x.WellName == GetFacilityId("RPOC_", 1));
                Assert.AreEqual(WellStatusCommunicationCode.OutOfService, normalwel.WellStatusCommunication, "GetWellStatusCommunicationCode is incorrect");

                //Revert back status back to "In Service"
                welldwtdto.OutOfServiceCode = 1;
                welldwtdto.CapturedDateTime = DateTime.Now.AddDays(-2).ToUniversalTime();
                welldwtadddto.Downtime = welldwtdto;
                SurveillanceService.AddDownTimeForWellWithGroupStatus(welldwtadddto);

                // back to in service and Start 
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.StartRPC.ToString());
                Thread.Sleep(5000);
                //************************* 5. Verify Server Controller /CygNet alarm ***********************************************

                FacilityTag facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("RPOC_", 1));
                UpdateAlarmPointValue(GetUisClient(), facilityTag, "RRLCRDAREA", "25");
                SurveillanceService.IssueCommandForWells(new long[] { well.Id }, WellCommand.DemandScan.ToString(), "true");
                //****these lines are workaround for test not fix for actual issue !!!!
                wellcommdto = SurveillanceService.GetWellStatusCommunication(filters);
                normalwel = wellcommdto.FirstOrDefault(x => x.WellName == GetFacilityId("RPOC_", 1));
                updatewellcfg = WellConfigurationService.GetWellConfig(well.Id.ToString());
                updatewellcfg.Well.SurfaceLatitude = 15;
                updatewellcfg.Well.SurfaceLongitude = 70;
                WellConfigurationService.UpdateWellConfig(updatewellcfg);
                Assert.AreEqual(WellStatusCommunicationCode.CygNetAlarm, normalwel.WellStatusCommunication, "GetWellStatusCommunicationCode is incorrect");
                // FRWM-FRWM-6004  Sub task : FRWM-6423
                //Test for CygNet Alarm Status
                string[] activeCygNetalarms = normalwel.ActiveCygNetAlarm;
                List<string> allCygNetAlarms = activeCygNetalarms.ToList();
                CollectionAssert.AllItemsAreNotNull(allCygNetAlarms, "All Items are not Null");
                string startDate = DateTime.Today.AddDays(-30).ToUniversalTime().ToISO8601();
                string endDate = DateTime.Today.AddDays(1).ToUniversalTime().ToISO8601();
                CollectionAssert.AllItemsAreUnique(allCygNetAlarms, "All Items within ForeSite alarms were not unique");
                // Get CygNet Alarm for Well 
                CygNetAlarmDTO cygnetalarms = SurveillanceService.GetCygNetAlarmsByWellId(well.Id.ToString());
                Assert.IsNotNull(cygnetalarms);
                Assert.IsTrue(cygnetalarms.CygNetAlarms.Contains("Card Area"), "High Card Area CygNet Alarm did not appear");
                CygNetAlarmDTO highestpriotoyCygNetalarm = SurveillanceService.GetCygNetHighestPiorityAlarmByWellId(well.Id.ToString());
                Assert.IsNotNull(highestpriotoyCygNetalarm);
                Assert.IsTrue(highestpriotoyCygNetalarm.CygNetAlarms.Count == 1, "No Highest Priority alarm was retrived");
                CygNetAlarmHistoryDTO[] allCygnetalarmsforwell = AlarmService.GetCygNetAlarmHistoryByWellIdAndDateRange(well.Id.ToString(), startDate, endDate);
                Assert.IsNotNull(allCygnetalarmsforwell, "No CygNet Alarm hostory was obtained");
                Assert.IsTrue(allCygnetalarmsforwell.Length > 0, "CygNet Alarm history records count was not udated");
                //************************* 6. Verify Server ForeSite alarm  Non RRL Well Type ESP  ***********************************************
                //Non RRL Well type
                //Make sure CygNet facility UIS point STFLOW is always FLOW ; If NOFLOW , The Status Stopped will get higher priority;

                facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("ESPWELL_", 1));
                SetPointValue(facilityTag, "STFLOW", "FLOW", DateTime.Now);
                WellDTO nonrrlwell = AddNonRRLWell(GetFacilityId("ESPWELL_", 1), WellTypeId.ESP);
                updatewellcfg = WellConfigurationService.GetWellConfig(nonrrlwell.Id.ToString());
                updatewellcfg.Well.SurfaceLatitude = 19;
                updatewellcfg.Well.SurfaceLongitude = 70;
                WellConfigurationService.UpdateWellConfig(updatewellcfg);
                wellcommdto = SurveillanceService.GetWellStatusCommunication(filters);
                normalwel = wellcommdto.FirstOrDefault(x => x.WellName == GetFacilityId("ESPWELL_", 1));
                Assert.AreEqual(WellStatusCommunicationCode.IntelligentAlarm, normalwel.WellStatusCommunication, "GetWellStatusCommunicationCode is incorrect");
                // FRWM-FRWM-6004  Sub task : FRWM-6423
                //Test for ForeSite Alarm Status :
                Trace.WriteLine("Testing for ForeSiteAlarm Status information");
                string[] activeForesiteAlarms = normalwel.ActiveForeSiteAlarm;
                List<string> allForesiteAlarms = activeForesiteAlarms.ToList();
                CollectionAssert.AllItemsAreNotNull(allForesiteAlarms, "All Items are not Null");
                CollectionAssert.AllItemsAreUnique(allForesiteAlarms, "All Items within ForeSite alarms were not unique");
                //Add Standard UDC
                Trace.WriteLine("Testing for Point and UDC information");
                string stdudcconfig = (string)welludcmap;
                if (stdudcconfig.Contains("PumpIntakePressure|PIP|False"))
                {
                    stdudcconfig = stdudcconfig.Replace("PumpIntakePressure|PIP|False", "PumpIntakePressure|PIP|True");
                }
                else
                {
                    stdudcconfig = stdudcconfig.Replace("PumpIntakePressure|PIP", "PumpIntakePressure|PIP|True");
                }
                SetValuesInSystemSettings(SettingServiceStringConstants.WELL_UDC_CONFIG, stdudcconfig);
                //Add an Standard UDC to disaply in Map View as Well
                infos.Parse(settingValue.StringValue);
                infos.Info.Add(infos.Info.Count() + 1, new ConfigurableUDC("Additional Tubing Head Pressure", "PRTUBXIN", "psia", UnitCategory.Pressure, CygNetPointType.Analog, true));
                infos.Info.Add(infos.Info.Count() + 1, new ConfigurableUDC("Flow Status", "STFLOW", null, UnitCategory.None, CygNetPointType.String, true));
                settingValue.StringValue = infos.GetSettingValue();
                SettingService.SaveSystemSetting(settingValue);
                //Add Additional  UDC
                //Scan and Get the Mapped information :
                SetPointValue(facilityTag, "PIP", "500", DateTime.Now);
                SetPointValue(facilityTag, "PRTUBXIN", "200", DateTime.Now);
                SetPointValue(facilityTag, "STFLOW", "FLOW", DateTime.Now);
                SurveillanceService.IssueCommandForSingleWell(nonrrlwell.Id, WellCommand.DemandScan.ToString());
                Thread.Sleep(2000);
                wellcommdto = SurveillanceService.GetWellStatusCommunication(filters);
                normalwel = wellcommdto.FirstOrDefault(x => x.WellName == GetFacilityId("ESPWELL_", 1));

                MappedPointInfo[] mappedUDCs = normalwel.MappedPointInfos;
                Trace.WriteLine("Mapped UDC Information are : ");
                foreach (MappedPointInfo indmap in mappedUDCs)
                {
                    Trace.WriteLine($"Found the Mapped UDC information with description {indmap.Description}");
                }
                MappedPointInfo stdUDCInfo = mappedUDCs.FirstOrDefault(x => x.Description == "Pump Intake Pressure");
                MappedPointInfo addUDCInfo = mappedUDCs.FirstOrDefault(x => x.Description == "Additional Tubing Head Pressure");

                //Also Add a Additional String UDC
                MappedPointInfo addStringUDCInfo = mappedUDCs.FirstOrDefault(x => x.Description == "Flow Status");
                MappedPointInfo addAnalogUDCInfo = mappedUDCs.FirstOrDefault(x => x.Description == "Additional Tubing Head Pressure");
                //Standard UDC information in US Units
                Assert.AreEqual("Pump Intake Pressure", stdUDCInfo.Description, "UDC description was not found in Map View ");
                Assert.AreEqual("PIP", stdUDCInfo.UDC, "UDC Name was not matched ");
                Assert.AreEqual("psia", stdUDCInfo.Unit, "Unit System US was not matched");
                Assert.AreEqual("500", stdUDCInfo.Value, "Value for UDC mismatched");
                //String Addtional UDC 
                Assert.AreEqual("Flow Status", addStringUDCInfo.Description, "UDC description was not found in Map View ");
                Assert.AreEqual("STFLOW", addStringUDCInfo.UDC, "UDC Name was not matched ");
                Assert.AreEqual("FLOW", addStringUDCInfo.Value, "Value for UDC mismatched");
                //Analog  Addtional UDC 
                Assert.AreEqual("Additional Tubing Head Pressure", addAnalogUDCInfo.Description, "UDC description was not found in Map View ");
                Assert.AreEqual("PRTUBXIN", addAnalogUDCInfo.UDC, "UDC Name was not matched ");
                Assert.AreEqual("200", addAnalogUDCInfo.Value, "Value for UDC mismatched");
                //Change UOM
                ChangeUnitSystemUserSetting("Metric");
                //Get Fresh object again
                SurveillanceService.IssueCommandForSingleWell(nonrrlwell.Id, WellCommand.DemandScan.ToString());
                Thread.Sleep(2000);
                wellcommdto = SurveillanceService.GetWellStatusCommunication(filters);
                normalwel = wellcommdto.FirstOrDefault(x => x.WellName == GetFacilityId("ESPWELL_", 1));
                mappedUDCs = normalwel.MappedPointInfos;
                stdUDCInfo = mappedUDCs.FirstOrDefault(x => x.Description == "Pump Intake Pressure");
                Assert.AreEqual("Pump Intake Pressure", stdUDCInfo.Description, "UDC description was not found in Map View ");
                Assert.AreEqual("PIP", stdUDCInfo.UDC, "UDC Name was not matched ");
                Assert.AreEqual("kPa", stdUDCInfo.Unit, "Unit System Metric Label was not matched");
                Assert.AreEqual("3447.4", stdUDCInfo.Value, "Unit System Metric Vlaue was not matched");



            }
            catch (Exception e)
            {
                Trace.WriteLine("Error Encountered " + e.ToString());
                throw;

            }
            finally
            {
                settingValue.StringValue = originalSetting;
                SettingService.SaveSystemSetting(settingValue);
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.StartRPC.ToString());
                SetValuesInSystemSettings(SettingServiceStringConstants.WELL_UDC_CONFIG, (string)welludcmap);
                FacilityTag facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("RPOC_", 1));
                UpdateAlarmPointValue(GetUisClient(), facilityTag, "RRLCRDAREA", "30");
                ChangeUnitSystemUserSetting("US");
            }

        }
        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void VerifyAllWellGroupStatus()
        {
            //FRWM - 5474 API testing subtask :FRWM-6157
            //Get AllWellGroupStatus for al Well types
            // Add 3 wells for each Well Type
            for (int i = 0; i < 3; i++)
            {
                WellDTO rrlwell = AddRRLWell(GetFacilityId("RPOC_", (i + 1)));
                WellDTO espwell = AddNonRRLWell(GetFacilityId("ESPWELL_", (i + 1)), WellTypeId.ESP);
                WellDTO glwell = AddNonRRLWell(GetFacilityId("GLWELL_", (i + 1)), WellTypeId.GLift);
                WellDTO nfwwell = AddNonRRLWell(GetFacilityId("NFWWELL_", (i + 1)), WellTypeId.NF);
                WellDTO ginjwell = AddNonRRLWell(GetFacilityId("GASINJWELL_", (i + 1)), WellTypeId.GInj);
                WellDTO waterinjwell = AddNonRRLWell(GetFacilityId("WATERINJWELL_", (i + 1)), WellTypeId.WInj);
                WellDTO plungerliftwell = AddNonRRLWell(GetFacilityId("PGLWELL_", (i + 1)), WellTypeId.PLift);
                WellDTO waginjwel = AddNonRRLWell(GetFacilityId("IWC_", (i + 1)), WellTypeId.WGInj);
                WellDTO OTWell = AddNonRRLWell(GetFacilityId("RPOC_", (i + 6)), WellTypeId.OT);
                WellDTO pcpwell = AddNonRRLWell(GetFacilityId("WFTA1K_", (i + 1)), WellTypeId.PCP);
                _wellsToRemove.Add(rrlwell);
                _wellsToRemove.Add(espwell);
                _wellsToRemove.Add(glwell);
                _wellsToRemove.Add(nfwwell);
                _wellsToRemove.Add(ginjwell);
                _wellsToRemove.Add(waterinjwell);
                _wellsToRemove.Add(plungerliftwell);
                _wellsToRemove.Add(waginjwel);
                _wellsToRemove.Add(OTWell);
                _wellsToRemove.Add(pcpwell);
            }

            var newGroup = new WellGroupStatusQueryDTO();
            //Get well Filters
            WellFilterDTO filters = WellService.GetWellFilter(null);

            var allwellgroupstatusdto = SurveillanceService.GetAllWellGroupStatus(filters);
            Assert.IsNotNull(allwellgroupstatusdto, "All Well Group Status returned no records");
            // Assert.AreEqual(10, allwellgroupstatusdto.Count, "All Well group status does not have all Well Types in it");
            int allwellscount = 0;
            List<WellCommentDTO> cmtdtolist = new List<WellCommentDTO>();
            foreach (var wellgroupstatusdto in allwellgroupstatusdto)
            {
                if (wellgroupstatusdto.Status != null)
                {
                    Assert.AreEqual(3, wellgroupstatusdto.Status.Length, $"Well was not added for Lift Type {wellgroupstatusdto.WellType.ToString()}");


                    for (int k = 0; k < wellgroupstatusdto.Status.Length; k++)
                    {
                        switch (wellgroupstatusdto.WellType)
                        {
                            case WellTypeId.RRL:
                                {

                                    RRLWellStatusValueDTO wellrecordvalues = (RRLWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    //  SurveillanceService.IssueCommandForSingleWell(wellrecordvalues.WellId, WellCommand.DemandScan.ToString());
                                    Assert.IsTrue(wellrecordvalues.WellName.StartsWith("RPOC_"), "RRL Well name was not found");
                                    Assert.IsNotNull(wellrecordvalues.LastScanTime, "Last Good Time is NULL");
                                    Assert.IsNotNull(wellrecordvalues.LastGoodScanTime, "Last Good Time is NULL");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    WellCommentDTO cmt = new WellCommentDTO { WellComment = $"WellComment for RPOC Well", WellId = wellrecordvalues.WellId };
                                    cmtdtolist.Add(cmt);

                                    break;
                                }
                            case WellTypeId.ESP:
                                {
                                    ESPWellStatusValueDTO wellrecordvalues = (ESPWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    Assert.IsTrue(wellrecordvalues.WellName.StartsWith("ESPWELL_"), "ESP Well name was not found");
                                    Assert.IsNotNull(wellrecordvalues.LastScanTime, "Last Good Time is NULL");
                                    Assert.IsNotNull(wellrecordvalues.LastGoodScanTime, "Last Good Time is NULL");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            case WellTypeId.NF:
                                {
                                    NFWellStatusValueDTO wellrecordvalues = (NFWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    Assert.IsTrue(wellrecordvalues.WellName.StartsWith("NFWWELL_"), "NFW Well name was not found");
                                    Assert.IsNotNull(wellrecordvalues.LastScanTime, "Last Good Time is NULL");
                                    Assert.IsNotNull(wellrecordvalues.LastGoodScanTime, "Last Good Time is NULL");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            case WellTypeId.GInj:
                                {
                                    GIWellStatusValueDTO wellrecordvalues = (GIWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    Assert.IsTrue(wellrecordvalues.WellName.StartsWith("GASINJWELL_"), "GasInjection Well name was not found");
                                    Assert.IsNotNull(wellrecordvalues.LastScanTime, "Last Good Time is NULL");
                                    Assert.IsNotNull(wellrecordvalues.LastGoodScanTime, "Last Good Time is NULL");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            case WellTypeId.WInj:
                                {
                                    WIWellStatusValueDTO wellrecordvalues = (WIWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    Assert.IsTrue(wellrecordvalues.WellName.StartsWith("WATERINJWELL_"), "Water Injection name was not found");
                                    Assert.IsNotNull(wellrecordvalues.LastScanTime, "Last Good Time is NULL");
                                    Assert.IsNotNull(wellrecordvalues.LastGoodScanTime, "Last Good Time is NULL");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            case WellTypeId.WGInj:
                                {
                                    WAGWellStatusValueDTO wellrecordvalues = (WAGWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    Assert.IsTrue(wellrecordvalues.WellName.StartsWith("IWC_"), "WAG Well name was not found");
                                    Assert.IsNotNull(wellrecordvalues.LastScanTime, "Last Good Time is NULL");
                                    Assert.IsNotNull(wellrecordvalues.LastGoodScanTime, "Last Good Time is NULL");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            case WellTypeId.PLift:
                                {
                                    PGLWellStatusValueDTO wellrecordvalues = (PGLWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    Assert.IsTrue(wellrecordvalues.WellName.StartsWith("PGLWELL_"), "PGL Well name was not found");
                                    Assert.IsNotNull(wellrecordvalues.LastScanTime, "Last Good Time is NULL");
                                    Assert.IsNotNull(wellrecordvalues.LastGoodScanTime, "Last Good Time is NULL");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            case WellTypeId.GLift:
                                {
                                    GLWellStatusValueDTO wellrecordvalues = (GLWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    Assert.IsTrue(wellrecordvalues.WellName.StartsWith("GLWELL_"), "GL Well name was not found");
                                    Assert.IsNotNull(wellrecordvalues.LastScanTime, "Last Good Time is NULL");
                                    Assert.IsNotNull(wellrecordvalues.LastGoodScanTime, "Last Good Time is NULL");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            case WellTypeId.OT:
                                {
                                    OTWellStatusValueDTO wellrecordvalues = (OTWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    Assert.IsTrue(wellrecordvalues.WellName.StartsWith("RPOC_"), "OT Well name was not found");
                                    Assert.IsNotNull(wellrecordvalues.LastScanTime, "Last Good Time is NULL");
                                    Assert.IsNotNull(wellrecordvalues.LastGoodScanTime, "Last Good Time is NULL");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            case WellTypeId.PCP:
                                {
                                    PCPWellStatusValueDTO wellrecordvalues = (PCPWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    Assert.IsTrue(wellrecordvalues.WellName.StartsWith("WFTA1K_"), "PCP Well name was not found");
                                    Assert.IsNotNull(wellrecordvalues.LastScanTime, "Last Good Time is NULL");
                                    Assert.IsNotNull(wellrecordvalues.LastGoodScanTime, "Last Good Time is NULL");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            default:
                                {
                                    Trace.WriteLine("Well Type was not known");
                                    Assert.Fail("Unknown WellType Encountred");
                                    break;
                                }
                        }
                        allwellscount++;
                    }

                }

            }
            Assert.AreEqual(30, allwellscount, "All Well Group Status count mismatch with Expected Count");
            //Verify Multiple Coments Grid save
            var updatedgroupstatus = SurveillanceService.SaveCommentsForMultipleWellsWithGroupStatus(cmtdtolist.ToArray());

            for (int k = 0; k < updatedgroupstatus.Status.Length; k++)
            {
                if (updatedgroupstatus.WellType == WellTypeId.RRL)
                {
                    RRLWellStatusValueDTO rrldto = (RRLWellStatusValueDTO)updatedgroupstatus.Status[k];
                    Assert.AreEqual($"WellComment for RPOC Well", rrldto.LastComment, $"Comments for Well {GetFacilityId("RPOC_", k + 1)} was not saved ");
                }
            }
            List<WellFilterValueDTO> value = new List<WellFilterValueDTO>();
            value.Add(new WellFilterValueDTO { Value = GetFacilityId("RPOC_", 1) });
            value.Add(new WellFilterValueDTO { Value = GetFacilityId("RPOC_", 2) });
            value.Add(new WellFilterValueDTO { Value = GetFacilityId("RPOC_", 3) });
            filters.welEngineerValues = value;
            allwellgroupstatusdto = SurveillanceService.GetAllWellGroupStatus(filters);
            allwellscount = 0;
            foreach (var wellgroupstatusdto in allwellgroupstatusdto)
            {
                if (wellgroupstatusdto.Status != null)
                {
                    Assert.AreEqual(3, wellgroupstatusdto.Status.Length, $"Well was not added for Lift Type {wellgroupstatusdto.WellType.ToString()}");


                    for (int k = 0; k < wellgroupstatusdto.Status.Length; k++)
                    {
                        switch (wellgroupstatusdto.WellType)
                        {
                            case WellTypeId.RRL:
                                {

                                    RRLWellStatusValueDTO wellrecordvalues = (RRLWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    //  SurveillanceService.IssueCommandForSingleWell(wellrecordvalues.WellId, WellCommand.DemandScan.ToString());
                                    Assert.IsTrue(wellrecordvalues.WellName.StartsWith("RPOC_"), "RRL Well name was not found");
                                    Assert.IsNotNull(wellrecordvalues.LastScanTime, "Last Good Time is NULL");
                                    Assert.IsNotNull(wellrecordvalues.LastGoodScanTime, "Last Good Time is NULL");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                        }
                        allwellscount++;
                    }
                }
            }
            Assert.AreEqual(3, allwellscount, "All Well Group Status count mismatch with Expected Count when Group Filter was applied");
        }
        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void VerifyGroupAlarmHistory()
        {
            if (s_isRunningInATS == false)
            {
                Trace.WriteLine("Test Execution was skipped as Team City does not Support Scheuduler run ");
                return;
            }
            try
            {
                // Create Wells of All type and 
                //FRWM - 6339 API testing subtask :FRWM-6430
                //Get GetAlllAlarmHistory for al Well types
                //Add 2 wells for each Well Type
                for (int i = 0; i < 2; i++)
                {
                    WellDTO rrlwell = AddRRLWell(GetFacilityId("RPOC_", (i + 1)));
                    WellDTO espwell = AddNonRRLWell(GetFacilityId("ESPWELL_", (i + 1)), WellTypeId.ESP);
                    WellDTO glwell = AddNonRRLWell(GetFacilityId("GLWELL_", (i + 1)), WellTypeId.GLift);
                    WellDTO nfwwell = AddNonRRLWell(GetFacilityId("NFWWELL_", (i + 1)), WellTypeId.NF);
                    WellDTO ginjwell = AddNonRRLWell(GetFacilityId("GASINJWELL_", (i + 1)), WellTypeId.GInj);
                    WellDTO waterinjwell = AddNonRRLWell(GetFacilityId("WATERINJWELL_", (i + 1)), WellTypeId.WInj);
                    WellDTO plungerliftwell = AddNonRRLWell(GetFacilityId("PGLWELL_", (i + 1)), WellTypeId.PLift);
                    WellDTO waginjwel = AddNonRRLWell(GetFacilityId("IWC_", (i + 1)), WellTypeId.WGInj);
                    WellDTO OTWell = AddNonRRLWell(GetFacilityId("RPOC_", (i + 6)), WellTypeId.OT);
                    WellDTO pcpwell = AddNonRRLWell(GetFacilityId("WFTA1K_", (i + 1)), WellTypeId.PCP);
                    _wellsToRemove.Add(rrlwell);
                    _wellsToRemove.Add(espwell);
                    _wellsToRemove.Add(glwell);
                    _wellsToRemove.Add(nfwwell);
                    _wellsToRemove.Add(ginjwell);
                    _wellsToRemove.Add(waterinjwell);
                    _wellsToRemove.Add(plungerliftwell);
                    _wellsToRemove.Add(waginjwel);
                    _wellsToRemove.Add(OTWell);
                    _wellsToRemove.Add(pcpwell);
                }

                var newGroup = new WellGroupStatusQueryDTO();
                //Get well Filters
                WellFilterDTO filters = WellService.GetWellFilter(null);
                //Get All Wells 
                WellDTO[] allwells = WellService.GetAllWells();
                //Get Well to test for Alar
                WellDTO alamWell = allwells.FirstOrDefault(x => x.Name == GetFacilityId("RPOC_", 1));
                FacilityTag facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("RPOC_", 1));
                //***************** Get All CygNet alarms **********************
                VerifyCygNetAlarmHistorySetClearforFacility(facilityTag, filters, alamWell, GetFacilityId("RPOC_", 1), "RRLCRDAREA", "High Card Area Alarm", "25", "30");
                alamWell = allwells.FirstOrDefault(x => x.Name == GetFacilityId("ESPWELL_", 1));
                facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("ESPWELL_", 1));
                VerifyCygNetAlarmHistorySetClearforFacility(facilityTag, filters, alamWell, GetFacilityId("ESPWELL_", 1), "PRTUBXIN", "High Tubing Head Pressure Alarm", "90", "300");
                //When we generate ForeSite alarm
                alamWell = allwells.FirstOrDefault(x => x.Name == GetFacilityId("RPOC_", 1));
                var uisClient = GetUisClient();
                //Set RRL Intelligent  Alert
                Trace.WriteLine("****************Testing ForeSite RRL Intelligent Alarm update in Group Alarm History ***************************");
                List<WellSettingDTO> alarmsettings = new List<WellSettingDTO>
                    {
                        new WellSettingDTO
                        {
                        Setting = new SettingDTO { Name = "High Beam Load", Key="High Beam Load", SettingType = (SettingType)5  , SettingValueType = SettingValueType.Number} ,
                        NumericValue =60,
                        SettingId =SettingService.GetSettingByName("High Beam Load").Id,
                        WellId = alamWell.Id
                        },
                    };
                IntelligentAlarmService.UpdateIntelligentAlarmWellSettings(alarmsettings.ToArray());
                SurveillanceService.IssueCommandForSingleWell(alamWell.Id, WellCommand.DemandScan.ToString());
                uisClient.SendUISCommand(GetFacilityId("RPOC_", 1), "CARDPUP", null, null);
                Thread.Sleep(5 * 1000);
                //Perform -RunAnalysis Task from command line 
                RunAnalysisTaskScheduler("-runAnalysis");
                Thread.Sleep(25 * 1000);
                VerifyForeSiteAlarmHistorySetClearforFacility(filters, alamWell, GetFacilityId("RPOC_", 1), "High Beam Load", "SET");
                //Clear RRL Alert:
                alarmsettings = new List<WellSettingDTO>
                    {
                        new WellSettingDTO
                        {
                        Setting = new SettingDTO { Name = "High Beam Load", Key="High Beam Load", SettingType = (SettingType)5  , SettingValueType = SettingValueType.Number} ,
                        NumericValue =90,
                        SettingId =SettingService.GetSettingByName("High Beam Load").Id,
                        WellId = alamWell.Id
                        },
                    };
                IntelligentAlarmService.UpdateIntelligentAlarmWellSettings(alarmsettings.ToArray());
                SurveillanceService.IssueCommandForSingleWell(alamWell.Id, WellCommand.DemandScan.ToString());
                uisClient.SendUISCommand(GetFacilityId("RPOC_", 1), "CARDPUP", null, null);
                Thread.Sleep(5 * 1000);
                //Perform -RunAnalysis Task from command line 
                RunAnalysisTaskScheduler("-runAnalysis");
                Thread.Sleep(25 * 1000);
                VerifyForeSiteAlarmHistorySetClearforFacility(filters, alamWell, GetFacilityId("RPOC_", 1), "High Beam Load", "CLEAR");
                Trace.WriteLine("****************Tested ForeSite RRL Intelligent Alarm update in Group Alarm History Successfully ***************************");
                //Clear RRL Alert:
                //Generate Operating LAert for ESP Well
                Trace.WriteLine("****************Testing ForeSite ESP  Operating Alarm update in Group Alarm History ***************************");
                alamWell = allwells.FirstOrDefault(x => x.Name == GetFacilityId("ESPWELL_", 1));
                facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("ESPWELL_", 1));
                //update Well test data to get Success Tuuning
                WellTestDTO testDataDTO = new WellTestDTO()
                {
                    WellId = alamWell.Id,
                    SPTCodeDescription = "AllocatableTest",
                    AverageTubingPressure = 200,
                    AverageCasingPressure = 0,
                    AverageTubingTemperature = 0,
                    Oil = 2396,
                    Gas = 1200,
                    Water = 3596,
                    ChokeSize = 64,
                    FlowLinePressure = 150,
                    SeparatorPressure = 100,
                    Frequency = 70,
                };

                DateTime WellCommisionDate = DateTime.Today.AddYears(-4);
                testDataDTO.SampleDate = WellCommisionDate.AddDays(2).ToUniversalTime();
                WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(alamWell.Id.ToString()).Units;
                WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                //We know Well test tubing head pressure
                SetPointValue(facilityTag, "PRTUBXIN", "100", DateTime.Now);
                Thread.Sleep(2000);
                //Check If value is updated in Well Status Cache for ESP Well

                WellGroupStatusQueryDTO espgp = GroupStatusQuery(WellTypeId.ESP);
                var wellsbyFilter = SurveillanceServiceClient.GetWellGroupStatus<ESPWellStatusUnitDTO, ESPWellStatusValueDTO>(espgp);
                Assert.IsNotNull(wellsbyFilter.Status.FirstOrDefault(x => x.WellName == GetFacilityId("ESPWELL_", 1)), "Failed to get the added well");
                ESPWellStatusValueDTO wellstatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(alamWell.Id.ToString(),
                SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as ESPWellStatusValueDTO;

                bool flagUDCValueUpdateCheck = wellstatus.TubingPressure.ToString().Contains("100") ? true : false;
                Trace.WriteLine($"Update flag for Well Status Cache : {flagUDCValueUpdateCheck}");
                int attempt = 0;
                while (flagUDCValueUpdateCheck == false)
                {
                    SurveillanceService.IssueCommandForSingleWell(alamWell.Id, WellCommand.DemandScan.ToString());
                    flagUDCValueUpdateCheck = wellstatus.TubingPressure.ToString().Contains("100") ? true : false;
                    Trace.WriteLine($"Update flag for Well Status Cache : {flagUDCValueUpdateCheck}  Attemmpt number :  {attempt}");
                    Thread.Sleep(5000);
                    attempt++;
                    if (attempt == 5)
                    {
                        break;
                    }
                }
                if (flagUDCValueUpdateCheck == false)
                {
                    Trace.WriteLine("Well Status Cache not updated after 5 attempts : ");
                    Assert.Fail("Test Was failed as Well Stauts cache not updated after 5 attempts");
                }
                VerifyForeSiteAlarmHistorySetClearforFacility(filters, alamWell, GetFacilityId("ESPWELL_", 1), "Low Tubing Pressure", "SET");
                SetPointValue(facilityTag, "PRTUBXIN", "200", DateTime.Now);

                VerifyForeSiteAlarmHistorySetClearforFacility(filters, alamWell, GetFacilityId("ESPWELL_", 1), "Low Tubing Pressure", "CLEAR");
                Trace.WriteLine("****************Tested  ForeSite ESP  Operating Alarm update in Group Alarm History Successfully ***************************");
                //Verify Well Filters 
                WellFilterDTO welTypeFilter = new WellFilterDTO();
                welTypeFilter.welFK_r_WellTypeValues = new[] { new WellFilterValueDTO { Value = WellTypeId.ESP.ToString() } };
                GroupAlarmHistoryDTO[] ForeSiteAlarmDTO = AlarmService.GetForeSiteAlarmHistoryByWellFilterAndDateRange(welTypeFilter, DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-31)), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime()));
                Trace.WriteLine("Testing Group Filter  with Well Type = ESP ");
                foreach (GroupAlarmHistoryDTO record in ForeSiteAlarmDTO)
                {
                    if (record.Well.Name.Contains("ESPWELL_") == false)
                    {
                        Trace.WriteLine($"ForeSite alarm history record  has Well name with {record.Well.Name},  When only Filtered Wells with  'ESP Well Type  were expected ");
                        Assert.Fail("When ESP Well Type Filter was Applied records had well name of Other Well Types ");
                    }
                }
                Trace.WriteLine("Testing Group Filter  with Well Type = ESP Completed Succesfully");
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error Encountered " + e.ToString());
                throw;
            }
            finally
            {
                //Revert CygNet UIS values to Original Values for test facility -- GetFacilityId("ESPWELL_", 1)
                FacilityTag facilityTag = new FacilityTag(s_site, s_cvsService, GetFacilityId("ESPWELL_", 1));
                SetPointValue(facilityTag, "PRTUBXIN", "100", DateTime.Now);
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void VerifyDailyAverageForOptionalUDCS()
        {
            //Giving Static Prefix for Non RRL wells so that test will run on ATS & TeamCity as well
            //FRWM-6525 API testing subtask :FRWM-6406 :Additional UDC doesn't generate daily average : Well Trends Support
            //Create Additional UDC on All Well Types and Check for Daily Averge Data in Well Trends Screen
            VerificationOfAdditionalUDCDailyAverageForWellTrend("RPOC_", "RRLUDCAddedByAPITest", "QTLIQT", "STB/d", UnitCategory.FlowRate, WellTypeId.RRL);
            VerificationOfAdditionalUDCDailyAverageForWellTrend("ESPWELL_", "ESPUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.None, WellTypeId.ESP);
            VerificationOfAdditionalUDCDailyAverageForWellTrend("NFWWELL_", "NFWUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.Pressure, WellTypeId.NF);
            VerificationOfAdditionalUDCDailyAverageForWellTrend("GLWELL_", "GLUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.Pressure, WellTypeId.GLift);
            VerificationOfAdditionalUDCDailyAverageForWellTrend("GASINJWELL_", "GIUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.Pressure, WellTypeId.GInj);
            VerificationOfAdditionalUDCDailyAverageForWellTrend("WATERINJWELL_", "WIUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.Pressure, WellTypeId.WInj);
            VerificationOfAdditionalUDCDailyAverageForWellTrend(s_prefixForPLFacility, "PLUDCAddedByAPITest", "PRTUBXIN", null, UnitCategory.Pressure, WellTypeId.PLift);
            VerificationOfAdditionalUDCDailyAverageForWellTrend("WFTA1K_", "PCPUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.Pressure, WellTypeId.PCP);
            VerificationOfAdditionalUDCDailyAverageForWellTrend("WFTA1K_", "PCPUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.Pressure, WellTypeId.OT);
            //Daily Average for WAG Wells is not supported 
            // VerificationOfAdditionalUDCDailyAverageForWellTrend( "ICW_", "PCPUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.Pressure, WellTypeId.WGInj);
        }

        private void VerifyCygNetAlarmHistorySetClearforFacility(FacilityTag facilityTag, WellFilterDTO filters, WellDTO well, string facID, string udcName, string almDesc, string almLimitSet, string almLimitclear)
        {
            //Expect alarm value to be present in CygNetAlarmDTO
            try
            {
                //************  Check for Alarm Set Condition 
                UpdateAlarmPointValue(GetUisClient(), facilityTag, udcName, almLimitSet);
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                Stopwatch stp = new Stopwatch();
                stp.Start();
                GroupAlarmHistoryDTO[] CygNetAlarmDTO = AlarmService.GetCygNetAlarmHistoryByWellFilterAndDateRange(filters, DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-31)), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime()));
                stp.Stop();
                Trace.WriteLine($"Total Records fetched for CygNet Alarms {CygNetAlarmDTO.Length}  and Total Time taken : {stp.Elapsed.TotalSeconds}  Seconds.");
                GroupAlarmHistoryDTO alarmrecorddto = CygNetAlarmDTO.FirstOrDefault(x => x.Well.Name == facID);
                Trace.WriteLine($" **************  CygNet  Alarm History Status  When alarm limit was SET to trigger an CygNet Alarm  for Well {well.Name} ");
                Trace.WriteLine($"Well Name  {new String(' ', 50 - "Well Name".Length)}|Alarm Status {new String(' ', 50 - "Alarm Status".Length)} | Alarm Type {new String(' ', 50 - "Alarm Type".Length)}| Alarm Value {new String(' ', 50 - "Alarm Value".Length)} | Started Time{new String(' ', 50 - "Started Time".ToString().Length)} |ClearTime {new String(' ', 50 - "ClearTime".ToString().Length)}|");
                string wellname = alarmrecorddto.Well.Name;
                string fsalm = alarmrecorddto.AlarmStatus;
                string almtype = alarmrecorddto.AlarmType.AlarmType;
                string almval = alarmrecorddto.StringValue;
                DateTime alstarttime = alarmrecorddto.StartTime;
                DateTime? alclearttime = alarmrecorddto.ClearedTime;
                Trace.WriteLine($"{wellname} {new String(' ', 50 - wellname.Length)} |{fsalm} {new String(' ', 50 - fsalm.Length)} | {almtype} {new String(' ', 50 - almtype.Length)}| {almval} {new String(' ', 50 - almval.Length)} | {alstarttime}{new String(' ', 50 - alstarttime.ToString().Length)} |{alclearttime} {new String(' ', 50 - alclearttime.ToString().Length)}|");

                Assert.IsNotNull(alarmrecorddto, $"CygNet alarm record for {facID} Well was NULL");
                Assert.IsNotNull(alarmrecorddto.AlarmStatus);
                Assert.AreEqual(almDesc, alarmrecorddto.AlarmStatus, "Mismatch with Alarm status");
                Assert.IsNotNull(alarmrecorddto.StartTime, "Alarm Start Time was NULL");
                Assert.IsNull(alarmrecorddto.ClearedTime, "Alarm Cleared Time was NOT  NULL");
                Assert.AreEqual("CygNet", almtype, "Alarm Type was not matched ");
                Assert.AreEqual(well.Name, wellname, "Well Name was not matched in Group Alarm History");

                //************  Chek for Alarm Clear Condition
                UpdateAlarmPointValue(GetUisClient(), facilityTag, udcName, almLimitclear);
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                CygNetAlarmDTO = AlarmService.GetCygNetAlarmHistoryByWellFilterAndDateRange(filters, DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-31)), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime()));
                alarmrecorddto = CygNetAlarmDTO.FirstOrDefault(x => x.Well.Name == facID);
                wellname = alarmrecorddto.Well.Name;
                fsalm = alarmrecorddto.AlarmStatus;
                almtype = alarmrecorddto.AlarmType.AlarmType;
                almval = alarmrecorddto.StringValue;
                alstarttime = alarmrecorddto.StartTime;
                alclearttime = alarmrecorddto.ClearedTime;
                //Expect alarm value to be present in cygnetAlarmDTO
                alarmrecorddto = CygNetAlarmDTO.FirstOrDefault(x => x.Well.Name == facID);
                Trace.WriteLine($" **************  CygNet  Alarm History Status  When alarm limit was Cleared to trigger an CygNet Alarm  for Well {well.Name} ");
                Trace.WriteLine($"Well Name {new String(' ', 50 - "Well Name".Length)}|Alarm Status {new String(' ', 50 - "Alarm Status".Length)} | Alarm Type {new String(' ', 50 - "Alarm Type".Length)} |Alarm Value {new String(' ', 50 - "Alarm Value".Length)} | Started Time{new String(' ', 50 - "Started Time".ToString().Length)} |ClearTime {new String(' ', 50 - "ClearTime".ToString().Length)}|");
                Trace.WriteLine($"{wellname} {new String(' ', 50 - wellname.Length)}|{fsalm} {new String(' ', 50 - fsalm.Length)} | {almtype} {new String(' ', 50 - almtype.Length)}| {almval} {new String(' ', 50 - almval.Length)} | {alstarttime}{new String(' ', 50 - alstarttime.ToString().Length)} |{alclearttime} {new String(' ', 50 - alclearttime.ToString().Length)}|");
                Assert.IsNotNull(alarmrecorddto, $"CygNet alarm record for {facID} Well was NULL");
                Assert.IsNotNull(alarmrecorddto.AlarmStatus);
                Assert.AreEqual(almDesc, alarmrecorddto.AlarmStatus, "Mismatch with Alarm status");
                Assert.IsNotNull(alarmrecorddto.StartTime, "Alarm Start Time was NULL");
                Assert.IsNotNull(alarmrecorddto.ClearedTime, "Alarm Cleared Time was NOT  NULL");
                Assert.AreEqual("CygNet", almtype, "Alarm Type was not matched ");
                Assert.AreEqual(well.Name, wellname, "Well Name was not matched in Group Alarm History");
            }
            finally
            {
                UpdateAlarmPointValue(GetUisClient(), facilityTag, udcName, almLimitclear);
            }
        }
        private void VerifyForeSiteAlarmHistorySetClearforFacility(WellFilterDTO filters, WellDTO well, string facID, string almDesc, string flagSetClear)
        {
            //Expect alarm value to be present in ForeSiteAlarmDTO
            try
            {
                //************  Check for Alarm Set Condition 
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());

                Stopwatch stp = new Stopwatch();
                stp.Start();
                GroupAlarmHistoryDTO[] ForeSiteAlarmDTO = AlarmService.GetForeSiteAlarmHistoryByWellFilterAndDateRange(filters, DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-31)), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime()));
                stp.Stop();
                Trace.WriteLine($"Total Records fetched for ForeSite Alarms {ForeSiteAlarmDTO.Length}  and Total Time taken : {stp.Elapsed.TotalSeconds}  Seconds.");
                //Filter For Selected Well and Selected Alarm Status Record
                ForeSiteAlarmDTO = ForeSiteAlarmDTO.Where(x => (x.Well.Name == well.Name)).ToArray();
                ForeSiteAlarmDTO = ForeSiteAlarmDTO.Where(x => (x.AlarmStatus == almDesc)).ToArray();

                GroupAlarmHistoryDTO alarmrecorddto = ForeSiteAlarmDTO.First();
                //Make Sure we have Selelcted Well record avaialable
                Assert.AreEqual(well.Name, alarmrecorddto.Well.Name);
                Trace.WriteLine($" **************  ForeSite  Alarm History Status  When alarm limit was {flagSetClear} to trigger an ForeSite Alarm  for Well {well.Name} ");
                Trace.WriteLine($"Well Name  {new String(' ', 50 - "Well Name".Length)}|Alarm Status {new String(' ', 50 - "Alarm Status".Length)} | Alarm Type {new String(' ', 50 - "Alarm Type".Length)}| Alarm Value {new String(' ', 50 - "Alarm Value".Length)} | Started Time{new String(' ', 50 - "Started Time".ToString().Length)} |ClearTime {new String(' ', 50 - "ClearTime".ToString().Length)}|");
                string wellname = alarmrecorddto.Well.Name;
                string fsalm = alarmrecorddto.AlarmStatus;
                string almtype = alarmrecorddto.AlarmType.AlarmType;
                string almval = (alarmrecorddto.StringValue == null) ? alarmrecorddto.NumericValue.ToString() : alarmrecorddto.StringValue;
                DateTime alstarttime = alarmrecorddto.StartTime;
                DateTime? alclearttime = alarmrecorddto.ClearedTime;
                Trace.WriteLine($"{wellname} {new String(' ', 50 - wellname.Length)} |{fsalm} {new String(' ', 50 - fsalm.Length)} | {almtype} {new String(' ', 50 - almtype.Length)}| {almval} {new String(' ', 50 - almval.Length)} | {alstarttime}{new String(' ', 50 - alstarttime.ToString().Length)} |{alclearttime} {new String(' ', 50 - alclearttime.ToString().Length)}|");

                Assert.IsNotNull(alarmrecorddto, $"ForeSite alarm record for {facID} Well was NULL");
                Assert.IsNotNull(alarmrecorddto.AlarmStatus);
                Assert.AreEqual(almDesc, alarmrecorddto.AlarmStatus, "Mismatch with Alarm status");
                Assert.IsNotNull(alarmrecorddto.StartTime, "Alarm Start Time was NULL");
                Assert.AreEqual("ForeSite", almtype, "Alarm Type was not matched ");
                Assert.AreEqual(well.Name, wellname, "Well Name was not matched in Group Alarm History");
                if (flagSetClear.ToUpper().Equals("SET"))
                {
                    Assert.IsNull(alarmrecorddto.ClearedTime, "Alarm Cleared Time was NOT  NULL");
                }
                else if (flagSetClear.ToUpper().Equals("CLEAR"))


                {
                    Assert.IsNotNull(alarmrecorddto.ClearedTime, "Alarm Cleared Time was NOT  NULL");
                }

            }
            finally
            {

            }
        }


        private void OptionalUDCDailyAvgCheckCommon(WellDTO well, string wellname, WellGroupStatusQueryDTO Query)
        {
            // **************************  Get Additional UDc daily average Data
            WellGroupStatusDTO<object, object> WellGroups = SurveillanceServiceClient.GetWellGroupStatus(Query);
            WellStatusOptionalFieldInfoDTO[] aOptionalUDC = WellGroups.OptionalFieldInfo;
            string[] trendlist = new string[] { aOptionalUDC.FirstOrDefault(x => x.Name == wellname).UDC };
            var optionaldailyavgdto = SurveillanceService.GetOptionalUDCDailyAverageTrendsByDateRange(trendlist, well.Id.ToString(), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-31)), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime()));
            Assert.IsNotNull(optionaldailyavgdto, "Optional UDC Daily Average was null for " + wellname);
            Assert.IsTrue(optionaldailyavgdto.Length > 0, "Addtional UDC for Daily Average was not obatined ");
            for (int i = 0; i < optionaldailyavgdto.Length; i++)
            {
                var dailyavgtrenduoptionaludc = optionaldailyavgdto[i] as CygNetTrendDTO;
                Assert.AreEqual(aOptionalUDC.FirstOrDefault(x => x.Name == wellname).UDC, dailyavgtrenduoptionaludc.PointUDC, "Optional UDC is null in Daily Average");
                Assert.IsNotNull(dailyavgtrenduoptionaludc.PointValues, "Point Values for Daily Average is NULL");
                Assert.IsTrue(dailyavgtrenduoptionaludc.PointValues.Length > 0, "Daily Average points are Zero for Optional UDCS ");
                int lastpointval = dailyavgtrenduoptionaludc.PointValues.Length - 1;
                TimeSpan timediffbetweenlastconsecutirvepoints = dailyavgtrenduoptionaludc.PointValues[lastpointval].Timestamp - dailyavgtrenduoptionaludc.PointValues[lastpointval - 1].Timestamp;
                Trace.WriteLine($"Last Point  Information : Time span : {dailyavgtrenduoptionaludc.PointValues[lastpointval].Timestamp} Value : {dailyavgtrenduoptionaludc.PointValues[lastpointval].Value} ");
                Assert.IsTrue(timediffbetweenlastconsecutirvepoints.TotalHours == 24);
                //Compare UTC with UTC 
                Assert.IsFalse(dailyavgtrenduoptionaludc.PointValues[0].Timestamp < DateTime.Today.ToUniversalTime().AddDays(-30), "The data was retrieved Before Start Time Filter Set ");
                Assert.IsFalse(dailyavgtrenduoptionaludc.PointValues[lastpointval].Timestamp > DateTime.Today.ToUniversalTime(), "Optional UDC Well Trend Daily Average Time stamp is in future ");

            }
        }

        private void OptionalUDCDailyAvgCheckCommonWellStatus(WellDTO well, string udcdesc, string addudc, string unitkey, UnitCategory unitcat)
        {
            // **************************  Get Additional UDC daily average Data
            var wdaWell = SurveillanceService.GetWellDailyAverageAndTest(well.Id.ToString(), DTOExtensions.ToISO8601(DateTime.UtcNow.Date));
            OptionalDailyAverageUDCDTO[] dailyaveragewellstatusdto = wdaWell.OptionalUDCDTOs;
            var wellstatusdata = SurveillanceService.GetWellStatusData(well.Id.ToString());

            switch (well.WellType)
            {
                case WellTypeId.RRL:
                    {
                        RRLWellStatusUnitDTO unitsdto = (RRLWellStatusUnitDTO)wellstatusdata.Units;
                        RRLWellStatusValueDTO valuedto = (RRLWellStatusValueDTO)wellstatusdata.Value;
                        for (int i = 0; i < dailyaveragewellstatusdto.Length; i++)
                        {
                            Trace.WriteLine($"Well Status Additional UDC Description for Well Type {well.WellType.ToString()} : {dailyaveragewellstatusdto[i].Description}");
                            Trace.WriteLine($"Well Status Additional UDC UDC name : {dailyaveragewellstatusdto[i].UDC}");
                            Trace.WriteLine($"Well Status Additional UDC Description Unit : {unitsdto.OptionalField01?.UnitKey }");
                            Trace.WriteLine($"Well Status Additional UDC Description Value: {dailyaveragewellstatusdto[i].NumericValue}");
                            Assert.AreEqual(udcdesc, dailyaveragewellstatusdto[i].Description, " Additional UDC Description Mismatch");
                            Assert.AreEqual(addudc, dailyaveragewellstatusdto[i].UDC, " Additional UDC name Mismatch");
                            Assert.AreEqual(unitkey, unitsdto.OptionalField01.UnitKey, " Additional UDC Unit Mismatch");
                            Assert.IsNotNull(dailyaveragewellstatusdto[i].NumericValue, " Additional UDC Value us NULL");

                        }
                        break;
                    }
                case WellTypeId.ESP:
                    {
                        ESPWellStatusUnitDTO unitsdto = (ESPWellStatusUnitDTO)wellstatusdata.Units;
                        ESPWellStatusValueDTO valuedto = (ESPWellStatusValueDTO)wellstatusdata.Value;
                        for (int i = 0; i < dailyaveragewellstatusdto.Length; i++)
                        {
                            Trace.WriteLine($"Well Status Additional UDC Description for Well Type {well.WellType.ToString()} : {dailyaveragewellstatusdto[i].Description}");
                            Trace.WriteLine($"Well Status Additional UDC UDC name : {dailyaveragewellstatusdto[i].UDC}");
                            Trace.WriteLine($"Well Status Additional UDC Description Unit : {unitsdto.OptionalField01?.UnitKey }");
                            Trace.WriteLine($"Well Status Additional UDC Description Value: {dailyaveragewellstatusdto[i].NumericValue}");
                            Assert.AreEqual(udcdesc, dailyaveragewellstatusdto[i].Description, " Additional UDC Description Mismatch");
                            Assert.AreEqual(addudc, dailyaveragewellstatusdto[i].UDC, " Additional UDC name Mismatch");
                            Assert.AreEqual(unitkey, unitsdto.OptionalField01.UnitKey, " Additional UDC Unit Mismatch");
                            Assert.IsNotNull(dailyaveragewellstatusdto[i].NumericValue, " Additional UDC Value us NULL");

                        }
                        break;
                    }
                case WellTypeId.GLift:
                    {
                        GLWellStatusUnitDTO unitsdto = (GLWellStatusUnitDTO)wellstatusdata.Units;
                        GLWellStatusValueDTO valuedto = (GLWellStatusValueDTO)wellstatusdata.Value;
                        for (int i = 0; i < dailyaveragewellstatusdto.Length; i++)
                        {
                            Trace.WriteLine($"Well Status Additional UDC Description for Well Type {well.WellType.ToString()} : {dailyaveragewellstatusdto[i].Description}");
                            Trace.WriteLine($"Well Status Additional UDC UDC name : {dailyaveragewellstatusdto[i].UDC}");
                            Trace.WriteLine($"Well Status Additional UDC Description Unit : {unitsdto.OptionalField01?.UnitKey }");
                            Trace.WriteLine($"Well Status Additional UDC Description Value: {dailyaveragewellstatusdto[i].NumericValue}");
                            Assert.AreEqual(udcdesc, dailyaveragewellstatusdto[i].Description, " Additional UDC Description Mismatch");
                            Assert.AreEqual(addudc, dailyaveragewellstatusdto[i].UDC, " Additional UDC name Mismatch");
                            Assert.AreEqual(unitkey, unitsdto.OptionalField01.UnitKey, " Additional UDC Unit Mismatch");
                            Assert.IsNotNull(dailyaveragewellstatusdto[i].NumericValue, " Additional UDC Value us NULL");

                        }
                        break;
                    }
                case WellTypeId.NF:
                    {
                        NFWellStatusUnitDTO unitsdto = (NFWellStatusUnitDTO)wellstatusdata.Units;
                        NFWellStatusValueDTO valuedto = (NFWellStatusValueDTO)wellstatusdata.Value;
                        for (int i = 0; i < dailyaveragewellstatusdto.Length; i++)
                        {
                            Trace.WriteLine($"Well Status Additional UDC Description for Well Type {well.WellType.ToString()} : {dailyaveragewellstatusdto[i].Description}");
                            Trace.WriteLine($"Well Status Additional UDC UDC name : {dailyaveragewellstatusdto[i].UDC}");
                            Trace.WriteLine($"Well Status Additional UDC Description Unit : {unitsdto.OptionalField01?.UnitKey }");
                            Trace.WriteLine($"Well Status Additional UDC Description Value: {dailyaveragewellstatusdto[i].NumericValue}");
                            Assert.AreEqual(udcdesc, dailyaveragewellstatusdto[i].Description, " Additional UDC Description Mismatch");
                            Assert.AreEqual(addudc, dailyaveragewellstatusdto[i].UDC, " Additional UDC name Mismatch");
                            Assert.AreEqual(unitkey, unitsdto.OptionalField01.UnitKey, " Additional UDC Unit Mismatch");
                            Assert.IsNotNull(dailyaveragewellstatusdto[i].NumericValue, " Additional UDC Value us NULL");

                        }
                        break;
                    }
                case WellTypeId.GInj:
                    {
                        GIWellStatusUnitDTO unitsdto = (GIWellStatusUnitDTO)wellstatusdata.Units;
                        GIWellStatusValueDTO valuedto = (GIWellStatusValueDTO)wellstatusdata.Value;
                        for (int i = 0; i < dailyaveragewellstatusdto.Length; i++)
                        {
                            Trace.WriteLine($"Well Status Additional UDC Description for Well Type {well.WellType.ToString()} : {dailyaveragewellstatusdto[i].Description}");
                            Trace.WriteLine($"Well Status Additional UDC UDC name : {dailyaveragewellstatusdto[i].UDC}");
                            Trace.WriteLine($"Well Status Additional UDC Description Unit : {unitsdto.OptionalField01?.UnitKey }");
                            Trace.WriteLine($"Well Status Additional UDC Description Value: {dailyaveragewellstatusdto[i].NumericValue}");
                            Assert.AreEqual(udcdesc, dailyaveragewellstatusdto[i].Description, " Additional UDC Description Mismatch");
                            Assert.AreEqual(addudc, dailyaveragewellstatusdto[i].UDC, " Additional UDC name Mismatch");
                            Assert.AreEqual(unitkey, unitsdto.OptionalField01.UnitKey, " Additional UDC Unit Mismatch");
                            Assert.IsNotNull(dailyaveragewellstatusdto[i].NumericValue, " Additional UDC Value us NULL");

                        }
                        break;
                    }
                case WellTypeId.WInj:
                    {
                        WIWellStatusUnitDTO unitsdto = (WIWellStatusUnitDTO)wellstatusdata.Units;
                        WIWellStatusValueDTO valuedto = (WIWellStatusValueDTO)wellstatusdata.Value;
                        for (int i = 0; i < dailyaveragewellstatusdto.Length; i++)
                        {
                            Trace.WriteLine($"Well Status Additional UDC Description for Well Type {well.WellType.ToString()} : {dailyaveragewellstatusdto[i].Description}");
                            Trace.WriteLine($"Well Status Additional UDC UDC name : {dailyaveragewellstatusdto[i].UDC}");
                            Trace.WriteLine($"Well Status Additional UDC Description Unit : {unitsdto.OptionalField01?.UnitKey }");
                            Trace.WriteLine($"Well Status Additional UDC Description Value: {dailyaveragewellstatusdto[i].NumericValue}");
                            Assert.AreEqual(udcdesc, dailyaveragewellstatusdto[i].Description, " Additional UDC Description Mismatch");
                            Assert.AreEqual(addudc, dailyaveragewellstatusdto[i].UDC, " Additional UDC name Mismatch");
                            Assert.AreEqual(unitkey, unitsdto.OptionalField01.UnitKey, " Additional UDC Unit Mismatch");
                            Assert.IsNotNull(dailyaveragewellstatusdto[i].NumericValue, " Additional UDC Value us NULL");

                        }
                        break;
                    }
                case WellTypeId.WGInj:
                    {
                        WAGWellStatusUnitDTO unitsdto = (WAGWellStatusUnitDTO)wellstatusdata.Units;
                        WAGWellStatusValueDTO valuedto = (WAGWellStatusValueDTO)wellstatusdata.Value;
                        for (int i = 0; i < dailyaveragewellstatusdto.Length; i++)
                        {
                            Trace.WriteLine($"Well Status Additional UDC Description for Well Type {well.WellType.ToString()} : {dailyaveragewellstatusdto[i].Description}");
                            Trace.WriteLine($"Well Status Additional UDC UDC name : {dailyaveragewellstatusdto[i].UDC}");
                            Trace.WriteLine($"Well Status Additional UDC Description Unit : {unitsdto.OptionalField01?.UnitKey }");
                            Trace.WriteLine($"Well Status Additional UDC Description Value: {dailyaveragewellstatusdto[i].NumericValue}");
                            Assert.AreEqual(udcdesc, dailyaveragewellstatusdto[i].Description, " Additional UDC Description Mismatch");
                            Assert.AreEqual(addudc, dailyaveragewellstatusdto[i].UDC, " Additional UDC name Mismatch");
                            Assert.AreEqual(unitkey, unitsdto.OptionalField01.UnitKey, " Additional UDC Unit Mismatch");
                            Assert.IsNotNull(dailyaveragewellstatusdto[i].NumericValue, " Additional UDC Value us NULL");

                        }
                        break;
                    }
                case WellTypeId.PLift:
                    {
                        PGLWellStatusUnitDTO unitsdto = (PGLWellStatusUnitDTO)wellstatusdata.Units;
                        PGLWellStatusValueDTO valuedto = (PGLWellStatusValueDTO)wellstatusdata.Value;
                        for (int i = 0; i < dailyaveragewellstatusdto.Length; i++)
                        {
                            Trace.WriteLine($"Well Status Additional UDC Description for Well Type {well.WellType.ToString()} : {dailyaveragewellstatusdto[i].Description}");
                            Trace.WriteLine($"Well Status Additional UDC UDC name : {dailyaveragewellstatusdto[i].UDC}");
                            Trace.WriteLine($"Well Status Additional UDC Description Unit : {unitsdto.OptionalField01?.UnitKey }");
                            Trace.WriteLine($"Well Status Additional UDC Description Value: {dailyaveragewellstatusdto[i].NumericValue}");
                            Assert.AreEqual(udcdesc, dailyaveragewellstatusdto[i].Description, " Additional UDC Description Mismatch");
                            Assert.AreEqual(addudc, dailyaveragewellstatusdto[i].UDC, " Additional UDC name Mismatch");
                            Assert.AreEqual(unitkey, unitsdto.OptionalField01?.UnitKey, " Additional UDC Unit Mismatch");
                            Assert.IsNotNull(dailyaveragewellstatusdto[i].NumericValue, " Additional UDC Value us NULL");

                        }
                        break;
                    }

                case WellTypeId.PCP:
                    {
                        PCPWellStatusUnitDTO unitsdto = (PCPWellStatusUnitDTO)wellstatusdata.Units;
                        PCPWellStatusValueDTO valuedto = (PCPWellStatusValueDTO)wellstatusdata.Value;
                        for (int i = 0; i < dailyaveragewellstatusdto.Length; i++)
                        {
                            Trace.WriteLine($"Well Status Additional UDC Description for Well Type {well.WellType.ToString()} : {dailyaveragewellstatusdto[i].Description}");
                            Trace.WriteLine($"Well Status Additional UDC UDC name : {dailyaveragewellstatusdto[i].UDC}");
                            Trace.WriteLine($"Well Status Additional UDC Description Unit : {unitsdto.OptionalField01?.UnitKey }");
                            Trace.WriteLine($"Well Status Additional UDC Description Value: {dailyaveragewellstatusdto[i].NumericValue}");
                            Assert.AreEqual(udcdesc, dailyaveragewellstatusdto[i].Description, " Additional UDC Description Mismatch");
                            Assert.AreEqual(addudc, dailyaveragewellstatusdto[i].UDC, " Additional UDC name Mismatch");
                            Assert.AreEqual(unitkey, unitsdto.OptionalField01?.UnitKey, " Additional UDC Unit Mismatch");
                            Assert.IsNotNull(dailyaveragewellstatusdto[i].NumericValue, " Additional UDC Value us NULL");

                        }
                        break;
                    }
                case WellTypeId.OT:
                    {
                        OTWellStatusUnitDTO unitsdto = (OTWellStatusUnitDTO)wellstatusdata.Units;
                        OTWellStatusValueDTO valuedto = (OTWellStatusValueDTO)wellstatusdata.Value;
                        for (int i = 0; i < dailyaveragewellstatusdto.Length; i++)
                        {
                            Trace.WriteLine($"Well Status Additional UDC Description for Well Type {well.WellType.ToString()} : {dailyaveragewellstatusdto[i].Description}");
                            Trace.WriteLine($"Well Status Additional UDC UDC name : {dailyaveragewellstatusdto[i].UDC}");
                            Trace.WriteLine($"Well Status Additional UDC Description Unit : {unitsdto.OptionalField01?.UnitKey }");
                            Trace.WriteLine($"Well Status Additional UDC Description Value: {dailyaveragewellstatusdto[i].NumericValue}");
                            Assert.AreEqual(udcdesc, dailyaveragewellstatusdto[i].Description, " Additional UDC Description Mismatch");
                            Assert.AreEqual(addudc, dailyaveragewellstatusdto[i].UDC, " Additional UDC name Mismatch");
                            Assert.AreEqual(unitkey, unitsdto.OptionalField01.UnitKey, " Additional UDC Unit Mismatch");
                            Assert.IsNotNull(dailyaveragewellstatusdto[i].NumericValue, " Additional UDC Value us NULL");

                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }


        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void VerifyAsyncUpdate()
        {
            //FRWM-6446 API testing subtask :FRWM-4223 :Async Grid Update 
            //Check Group Status update for Standard UDC and Adtional UDC as Well
            VerificationOfAsyncUpdate("ESP", "ESPWELL_", "ESPUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.None, WellTypeId.ESP);
        }

        public void VerificationOfAsyncUpdate(string wellType, string facilityId, string name = null, string additionalUDC = null, string unitkey = null, UnitCategory unitcat = UnitCategory.None, WellTypeId wellTypeId = WellTypeId.RRL)
        {
            string facilityId1;
            facilityId1 = GetFacilityId(facilityId, 1);
            // System setting
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = null;
            WellDTO wellDTO = null;
            wellDTO = SetDefaultFluidType(new WellDTO()
            {
                Name = DefaultWellName,
                FacilityId = facilityId1,
                DataConnection = GetDefaultCygNetDataConnection(),
                IntervalAPI = "IntervalAPI",
                SubAssemblyAPI = "SubAssemblyAPI",
                AssemblyAPI = "AssemblyAPI",
                CommissionDate = DateTime.Today.AddYears(-2),
                WellType = wellTypeId
            });

            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = wellDTO });

            switch (wellTypeId)
            {
                case WellTypeId.RRL:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.RRL_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.ESP:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.ESP_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.GLift:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.GL_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.NF:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.NF_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.GInj:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.GI_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.WInj:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.WI_GROUP_STATUS_EXTRA_UDCS);
                    break;

                case WellTypeId.PLift:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.PL_GROUP_STATUS_EXTRA_UDCS);
                    break;
                case WellTypeId.PCP:
                    systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.PCP_GROUP_STATUS_EXTRA_UDCS);
                    break;
            }

            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well1 = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            _wellsToRemove.Add(well1);

            WellFilterDTO wellbyFilters = new WellFilterDTO();
            WellGroupStatusQueryDTO Query = new WellGroupStatusQueryDTO();
            Query.WellType = well1.WellType;
            Query.WellFilter = wellbyFilters;
            SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
            string originalSetting = settingValue.StringValue;
            string oldvalue = String.Empty;
            string addudcname = String.Empty;
            FacilityTag facilityTag = new FacilityTag(s_site, s_cvsService, facilityId1);
            ConfigurableUDCInfo infos = new ConfigurableUDCInfo();
            double originalMax_Time = (double)GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.ASYNC_GRID_UPDATES_MAXIMUM_TIME);
            infos.Parse(settingValue.StringValue);
            try
            {
                infos.Info.Add(infos.Info.Count() + 1, new ConfigurableUDC(name, additionalUDC, unitkey, unitcat, CygNetPointType.Analog, false));
                settingValue.StringValue = infos.GetSettingValue();
                SettingService.SaveSystemSetting(settingValue);

                WellGroupStatusDTO<object, object> WellGroups = SurveillanceServiceClient.GetWellGroupStatus(Query);
                WellStatusOptionalFieldInfoDTO[] aOptionalUDC = WellGroups.OptionalFieldInfo;
                string sequencebase = WellGroups.SequenceBase;
                string sequencenumber = WellGroups.SequenceNumber;
                switch (wellTypeId)
                {

                    case WellTypeId.ESP:
                        {
                            // **************************  Get WellStatus Cache data
                            SurveillanceServiceClient.IssueCommandForSingleWell(well1.Id, WellCommand.DemandScan.ToString());
                            Trace.WriteLine($"Getting Well Status Data for : {well1.Name}");
                            ESPWellStatusValueDTO wellstatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(well1.Id.ToString(),
                            SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as ESPWellStatusValueDTO;
                            oldvalue = wellstatus.TubingPressure.ToString();
                            string adddUDColdvalue = wellstatus.OptionalField01.ToString();
                            addudcname = aOptionalUDC.FirstOrDefault(x => x.Name == name).UDC;
                            UserSettingDTO userdto = SettingService.GetCurrentUserSettingByName(SettingServiceStringConstants.ASYNC_GRID_UPDATES_ENABLED);
                            //Set Max time to 10 Secs for Poll
                            SetValuesInSystemSettings(SettingServiceStringConstants.ASYNC_GRID_UPDATES_MAXIMUM_TIME, "20000");
                            Assert.IsTrue(userdto.NumericValue == 1, "Asyn cgrid was enabled");
                            WellGroups = SurveillanceServiceClient.GetWellGroupStatus(Query);
                            sequencebase = WellGroups.SequenceBase;
                            sequencenumber = WellGroups.SequenceNumber;
                            //Just ensure we are changing the Value for CygNet UIS for Sure to get updates 
                            if (oldvalue.Equals("215.00"))
                            {
                                SetPointValue(facilityTag, "PRTUBXIN", "216", DateTime.Now);
                                Trace.WriteLine($"Set a New Value in CygNet UIS : 216 ");
                            }
                            else
                            {
                                SetPointValue(facilityTag, "PRTUBXIN", "215", DateTime.Now);
                                Trace.WriteLine($"Set a New Value in CygNet UIS : 215 ");
                            }

                            // Issue Scan Command to see of Sequnce base updated when we set Async to true
                            SurveillanceServiceClient.IssueCommandForSingleWell(well1.Id, WellCommand.DemandScan.ToString());
                            Trace.WriteLine($"Issued Scan Command to Get Updated Value ");
                            Stopwatch st = new Stopwatch();
                            st.Start();
                            Trace.WriteLine($"Calling API PollWellStatusUpdates(Async) updates ");
                            AsyncUpdateDTO asyncdto = SurveillanceService.PollWellStatusUpdates(sequencebase, sequencenumber);
                            st.Stop();
                            Trace.WriteLine($"Async Update Finished for US Units in {st.Elapsed.TotalSeconds}");
                            Assert.IsNotNull(asyncdto.SequenceBase, "Sequnce Base is NULL");
                            Assert.IsNotNull(asyncdto.SequenceNumber, "Sequnce Base  is NULL");
                            int maxAttempt = 0;
                            while (asyncdto.Ids.Count == 0 && maxAttempt < 5)
                            {
                                Trace.WriteLine($"*****  Re Issue Scan since first scan did not work ***** Waiting for Cache still to get updated !! attempt number :{maxAttempt} ");
                                SurveillanceServiceClient.IssueCommandForSingleWell(well1.Id, WellCommand.DemandScan.ToString());
                                Trace.WriteLine($"Re - Issued Scan Command to Get Updated Value ");
                                maxAttempt++;
                                asyncdto = SurveillanceService.PollWellStatusUpdates(sequencebase, sequencenumber);
                                Thread.Sleep(2000);
                            }
                            if (asyncdto.Ids.Count == 0)
                            {
                                Assert.Fail($"Test Failed Since it could not get any response for updated cache value even after {maxAttempt} attempts");
                            }
                            Assert.IsTrue(asyncdto.Ids.Contains(well1.Id), "Passed Well ID was not Present in Response");
                            List<WellGroupStatusDTO<object, object>> listwellgroupstatusdto = SurveillanceService.GetWellStatusByIds(asyncdto.Ids);
                            WellGroupStatusDTO<object, object> PolledWellgroup = listwellgroupstatusdto.FirstOrDefault(x => x.SequenceBase == asyncdto.SequenceBase);

                            object[] statusarray = PolledWellgroup.Status;
                            foreach (var statsuobj in statusarray)
                            {
                                var statusforwell = (ESPWellStatusValueDTO)statsuobj;
                                if (statusforwell.WellId != well1.Id)
                                {
                                    //Check for the WEllId passed
                                    Trace.WriteLine("Not checking for other Well Id's ");
                                    break;
                                }
                                string newvalue = statusforwell.TubingPressure.ToString();
                                string adddUDCnewvalue = statusforwell.OptionalField01.ToString();
                                if (oldvalue.Equals("215.00"))
                                {
                                    Assert.IsFalse(oldvalue == newvalue, "The Value did not update on Polling Status for Standard UDC  ");
                                    Assert.IsFalse(adddUDColdvalue == adddUDCnewvalue, "The Value did not update on Polling Status for Additional UDC  ");
                                }
                                else
                                {
                                    Assert.IsFalse(oldvalue == newvalue, "The Value did not update on Polling Status for Standard UDC  ");
                                    Assert.IsFalse(adddUDColdvalue == adddUDCnewvalue, "The Value did not update on Polling Status for Additional UDC  ");
                                }
                            }

                            //Check for Unit Sytem Metric
                            Trace.WriteLine($"Doing Metric Unit Verification ");
                            ChangeUnitSystemUserSetting("Metric");
                            string oldvaluemtric = UnitsConversion("psi", double.Parse(oldvalue)).ToString();
                            SurveillanceServiceClient.IssueCommandForSingleWell(well1.Id, WellCommand.DemandScan.ToString());
                            WellGroups = SurveillanceServiceClient.GetWellGroupStatus(Query);
                            sequencebase = WellGroups.SequenceBase;
                            sequencenumber = WellGroups.SequenceNumber;
                            //Call API to Poll Status

                            st.Start();
                            asyncdto = SurveillanceService.PollWellStatusUpdates(sequencebase, sequencenumber);
                            st.Stop();
                            Trace.WriteLine($"Async Update Finished for Metric Units in {st.Elapsed.TotalSeconds}");
                            Assert.IsNotNull(asyncdto.SequenceBase, "Sequnce Base is NULL");
                            Assert.IsNotNull(asyncdto.SequenceNumber, "Sequnce Base  is NULL");
                            maxAttempt = 0;
                            while (asyncdto.Ids.Count == 0 && maxAttempt < 5)
                            {
                                Trace.WriteLine($"*****  Re -Issue Scan since first scan did not work(Metric) ***** Waiting for Cache still to get updated !! attempt number :{maxAttempt} ");
                                SurveillanceServiceClient.IssueCommandForSingleWell(well1.Id, WellCommand.DemandScan.ToString());
                                Trace.WriteLine($"Re - Issued Scan Command to Get Updated Value ");
                                maxAttempt++;
                                asyncdto = SurveillanceService.PollWellStatusUpdates(sequencebase, sequencenumber);
                                Thread.Sleep(2000);
                            }
                            if (asyncdto.Ids.Count == 0)
                            {
                                Assert.Fail($"Test Failed Since it could not get any response for updated cache value even after {maxAttempt} attempts");
                            }

                            Assert.IsTrue(asyncdto.Ids.Contains(well1.Id), "Passed Well ID was not Present in Response");
                            listwellgroupstatusdto = SurveillanceService.GetWellStatusByIds(asyncdto.Ids);
                            PolledWellgroup = listwellgroupstatusdto.FirstOrDefault(x => x.SequenceBase == asyncdto.SequenceBase);

                            statusarray = PolledWellgroup.Status;
                            foreach (var statsuobj in statusarray)
                            {
                                var statusforwell = (ESPWellStatusValueDTO)statsuobj;
                                if (statusforwell.WellId != well1.Id)
                                {
                                    //Check for the Well Id passed
                                    Trace.WriteLine("Not checking for other Well Id's ");
                                    break;
                                }
                                string newvaluemetric = statusforwell.TubingPressure.ToString();

                                //Unit Conversion for only Standard UDC
                                if (oldvaluemtric.Equals("1482.4"))
                                {
                                    Assert.IsFalse(oldvaluemtric == newvaluemetric, "The Value did not update on Polling Status for Standard UDC for Metric  ");

                                }
                                else
                                {
                                    Assert.IsFalse(oldvaluemtric == newvaluemetric, "The Value did not update on Polling Status for Standard UDC for Metric ");
                                }
                            }
                            break;
                        }
                }
            }
            finally
            {
                //Removing Well
                WellService.RemoveWellByWellId(well1.Id.ToString());
                //reverting System Setting changes
                settingValue.StringValue = originalSetting;
                SettingService.SaveSystemSetting(settingValue);
                SetValuesInSystemSettings(SettingServiceStringConstants.ASYNC_GRID_UPDATES_MAXIMUM_TIME, originalMax_Time.ToString());
                SetPointValue(facilityTag, "PRTUBXIN", oldvalue, DateTime.Now);
                ChangeUnitSystemUserSetting("US");
            }
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetPointFieldListByWellType()
        {
            // To cover addtional APIS' FRI-4282
            // Add All Well Types 
            // Some APIS are called by OSI PI
            long[] wellids = new long[] { };
            for (int i = 0; i < 1; i++)
            {
                WellDTO rrlwell = AddRRLWell(GetFacilityId("RPOC_", (i + 1)));
                WellDTO espwell = AddNonRRLWell(GetFacilityId("ESPWELL_", (i + 1)), WellTypeId.ESP);
                WellDTO glwell = AddNonRRLWell(GetFacilityId("GLWELL_", (i + 1)), WellTypeId.GLift);
                WellDTO nfwwell = AddNonRRLWell(GetFacilityId("NFWWELL_", (i + 1)), WellTypeId.NF);
                WellDTO ginjwell = AddNonRRLWell(GetFacilityId("GASINJWELL_", (i + 1)), WellTypeId.GInj);
                WellDTO waterinjwell = AddNonRRLWell(GetFacilityId("WATERINJWELL_", (i + 1)), WellTypeId.WInj);
                WellDTO plungerliftwell = AddNonRRLWell(GetFacilityId("PGLWELL_", (i + 1)), WellTypeId.PLift);
                WellDTO waginjwel = AddNonRRLWell(GetFacilityId("IWC_", (i + 1)), WellTypeId.WGInj);
                WellDTO OTWell = AddNonRRLWell(GetFacilityId("RPOC_", (i + 6)), WellTypeId.OT);
                WellDTO pcpwell = AddNonRRLWell(GetFacilityId("WFTA1K_", (i + 1)), WellTypeId.PCP);
                _wellsToRemove.Add(rrlwell);
                _wellsToRemove.Add(espwell);
                _wellsToRemove.Add(glwell);
                _wellsToRemove.Add(nfwwell);
                _wellsToRemove.Add(ginjwell);
                _wellsToRemove.Add(waterinjwell);
                _wellsToRemove.Add(plungerliftwell);
                _wellsToRemove.Add(waginjwel);
                _wellsToRemove.Add(OTWell);
                _wellsToRemove.Add(pcpwell);
                wellids = new long[] { rrlwell.Id, espwell.Id, glwell.Id, nfwwell.Id, ginjwell.Id, waterinjwell.Id, plungerliftwell.Id, waginjwel.Id, OTWell.Id, pcpwell.Id };
            }

            var newGroup = new WellGroupStatusQueryDTO();
            //Get well Filters
            WellFilterDTO filters = WellService.GetWellFilter(null);
            var allwellgroupstatusdto = SurveillanceService.GetAllWellGroupStatus(filters);
            var multiscantuple = SurveillanceService.IssueCommandForWellListWithReturn(wellids, WellCommand.DemandScan.ToString(), "true");

            object[] wgrpstatus = multiscantuple.Item1.Status;
            foreach (object wgp in wgrpstatus)
            {
                switch (multiscantuple.Item1.WellType)
                {
                    case WellTypeId.RRL:
                        {
                            Assert.IsTrue(((RRLWellStatusValueDTO)wgp).CommStatus.ToLower().Equals("succeeded"), $"Scan was not Success on Multiple Scan for WellType {multiscantuple.Item1.WellType.ToString()}");
                            break;
                        }
                    case WellTypeId.ESP:
                        {
                            Assert.IsTrue(((ESPWellStatusValueDTO)wgp).CommStatus.ToLower().Equals("succeeded"), $"Scan was not Success on Multiple Scan for WellType {multiscantuple.Item1.WellType.ToString()}");
                            break;
                        }
                    case WellTypeId.GLift:
                        {
                            Assert.IsTrue(((GLWellStatusValueDTO)wgp).CommStatus.ToLower().Equals("succeeded"), $"Scan was not Success on Multiple Scan for WellType {multiscantuple.Item1.WellType.ToString()}");
                            break;
                        }
                    case WellTypeId.NF:
                        {
                            Assert.IsTrue(((NFWellStatusValueDTO)wgp).CommStatus.ToLower().Equals("succeeded"), $"Scan was not Success on Multiple Scan for WellType {multiscantuple.Item1.WellType.ToString()}");
                            break;
                        }
                    case WellTypeId.GInj:
                        {
                            Assert.IsTrue(((GIWellStatusValueDTO)wgp).CommStatus.ToLower().Equals("succeeded"), $"Scan was not Success on Multiple Scan for WellType {multiscantuple.Item1.WellType.ToString()}");
                            break;
                        }
                    case WellTypeId.WInj:
                        {
                            Assert.IsTrue(((WIWellStatusValueDTO)wgp).CommStatus.ToLower().Equals("succeeded"), $"Scan was not Success on Multiple Scan for WellType {multiscantuple.Item1.WellType.ToString()}");
                            break;
                        }
                    case WellTypeId.WGInj:
                        {
                            Assert.IsTrue(((WAGWellStatusValueDTO)wgp).CommStatus.ToLower().Equals("succeeded"), $"Scan was not Success on Multiple Scan for WellType {multiscantuple.Item1.WellType.ToString()}");
                            break;
                        }
                    case WellTypeId.PLift:
                        {
                            Assert.IsTrue(((PGLWellStatusValueDTO)wgp).CommStatus.ToLower().Equals("succeeded"), $"Scan was not Success on Multiple Scan for WellType {multiscantuple.Item1.WellType.ToString()}");
                            break;
                        }
                    case WellTypeId.PCP:
                        {
                            Assert.IsTrue(((PCPWellStatusValueDTO)wgp).CommStatus.ToLower().Equals("succeeded"), $"Scan was not Success on Multiple Scan for WellType {multiscantuple.Item1.WellType.ToString()}");
                            break;
                        }
                    case WellTypeId.OT:
                        {
                            Assert.IsTrue(((OTWellStatusValueDTO)wgp).CommStatus.ToLower().Equals("succeeded"), $"Scan was not Success on Multiple Scan for WellType {multiscantuple.Item1.WellType.ToString()}");
                            break;
                        }

                }
            }

            Assert.IsNotNull(allwellgroupstatusdto, "All Well Group Status returned no records");
            string[] unitkeys = SurveillanceService.GetUnitKeys();
            Assert.AreEqual(106, unitkeys.Length, "Unit Keys Length was not:  106 ");
            // Assert.AreEqual(10, allwellgroupstatusdto.Count, "All Well group status does not have all Well Types in it");
            foreach (var wellgroupstatusdto in allwellgroupstatusdto)
            {
                if (wellgroupstatusdto.Status != null)
                {
                    Assert.AreEqual(1, wellgroupstatusdto.Status.Length, $"Well was not added for Lift Type {wellgroupstatusdto.WellType.ToString()}");
                    for (int k = 0; k < wellgroupstatusdto.Status.Length; k++)
                    {
                        switch (wellgroupstatusdto.WellType)
                        {
                            case WellTypeId.RRL:
                                {

                                    RRLWellStatusValueDTO wellrecordvalues = (RRLWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    string[] defaultpointtypes = SurveillanceService.GetPointFieldListByWellType(WellTypeId.RRL.ToString());
                                    Trace.WriteLine($"{wellgroupstatusdto.WellType.ToString()}  Point types count = {defaultpointtypes.Length}");
                                    Assert.AreEqual(33, defaultpointtypes.Length, $"{wellgroupstatusdto.WellType.ToString()}  Point types count mismatch");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            case WellTypeId.ESP:
                                {
                                    ESPWellStatusValueDTO wellrecordvalues = (ESPWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    string[] defaultpointtypes = SurveillanceService.GetPointFieldListByWellType(WellTypeId.ESP.ToString());
                                    Trace.WriteLine($"{wellgroupstatusdto.WellType.ToString()}  Point types count = {defaultpointtypes.Length}");
                                    Assert.AreEqual(29, defaultpointtypes.Length, $"{wellgroupstatusdto.WellType.ToString()}  Point types count mismatch");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            case WellTypeId.GLift:
                                {
                                    GLWellStatusValueDTO wellrecordvalues = (GLWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    string[] defaultpointtypes = SurveillanceService.GetPointFieldListByWellType(WellTypeId.GLift.ToString());
                                    Trace.WriteLine($"{wellgroupstatusdto.WellType.ToString()}  Point types count = {defaultpointtypes.Length}");
                                    Assert.AreEqual(21, defaultpointtypes.Length, $"{wellgroupstatusdto.WellType.ToString()}  Point types count mismatch");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }

                            case WellTypeId.NF:
                                {
                                    NFWellStatusValueDTO wellrecordvalues = (NFWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    string[] defaultpointtypes = SurveillanceService.GetPointFieldListByWellType(WellTypeId.NF.ToString());
                                    Trace.WriteLine($"{wellgroupstatusdto.WellType.ToString()}  Point types count = {defaultpointtypes.Length}");
                                    Assert.AreEqual(19, defaultpointtypes.Length, $"{wellgroupstatusdto.WellType.ToString()}  Point types count mismatch");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            case WellTypeId.GInj:
                                {
                                    GIWellStatusValueDTO wellrecordvalues = (GIWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    string[] defaultpointtypes = SurveillanceService.GetPointFieldListByWellType(WellTypeId.GInj.ToString());
                                    Trace.WriteLine($"{wellgroupstatusdto.WellType.ToString()}  Point types count = {defaultpointtypes.Length}");
                                    Assert.AreEqual(21, defaultpointtypes.Length, $"{wellgroupstatusdto.WellType.ToString()}  Point types count mismatch");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            case WellTypeId.WInj:
                                {
                                    WIWellStatusValueDTO wellrecordvalues = (WIWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    string[] defaultpointtypes = SurveillanceService.GetPointFieldListByWellType(WellTypeId.WInj.ToString());
                                    Trace.WriteLine($"{wellgroupstatusdto.WellType.ToString()}  Point types count = {defaultpointtypes.Length}");
                                    Assert.AreEqual(21, defaultpointtypes.Length, $"{wellgroupstatusdto.WellType.ToString()}  Point types count mismatch");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            case WellTypeId.WGInj:
                                {
                                    WAGWellStatusValueDTO wellrecordvalues = (WAGWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    string[] defaultpointtypes = SurveillanceService.GetPointFieldListByWellType(WellTypeId.WGInj.ToString());
                                    Trace.WriteLine($"{wellgroupstatusdto.WellType.ToString()}  Point types count = {defaultpointtypes.Length}");
                                    Assert.AreEqual(26, defaultpointtypes.Length, $"{wellgroupstatusdto.WellType.ToString()}  Point types count mismatch");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            case WellTypeId.PLift:
                                {
                                    PGLWellStatusValueDTO wellrecordvalues = (PGLWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    string[] defaultpointtypes = SurveillanceService.GetPointFieldListByWellType(WellTypeId.PLift.ToString());
                                    Trace.WriteLine($"{wellgroupstatusdto.WellType.ToString()}  Point types count = {defaultpointtypes.Length}");
                                    Assert.AreEqual(19, defaultpointtypes.Length, $"{wellgroupstatusdto.WellType.ToString()}  Point types count mismatch");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            case WellTypeId.PCP:
                                {
                                    PCPWellStatusValueDTO wellrecordvalues = (PCPWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    string[] defaultpointtypes = SurveillanceService.GetPointFieldListByWellType(WellTypeId.PCP.ToString());
                                    Trace.WriteLine($"{wellgroupstatusdto.WellType.ToString()}  Point types count = {defaultpointtypes.Length}");
                                    Assert.AreEqual(30, defaultpointtypes.Length, $"{wellgroupstatusdto.WellType.ToString()}  Point types count mismatch");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }
                            case WellTypeId.OT:
                                {
                                    OTWellStatusValueDTO wellrecordvalues = (OTWellStatusValueDTO)wellgroupstatusdto.Status[k];
                                    string[] defaultpointtypes = SurveillanceService.GetPointFieldListByWellType(WellTypeId.OT.ToString());
                                    Trace.WriteLine($"{wellgroupstatusdto.WellType.ToString()} OT  Point types count provided in form of = {defaultpointtypes.Length}");
                                    Assert.AreEqual(8, defaultpointtypes.Length, $"{wellgroupstatusdto.WellType.ToString()}  Point types count mismatch");
                                    Trace.WriteLine($"Verfied Well with Well Name: {wellrecordvalues.WellName} for Well Name , Last Scan and Last Good Scan time ");
                                    break;
                                }


                        }
                    }
                }
            }

        }



        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void GetSaveRemovePointsAlarmConfigTest()
        {
            //Create New Well 
            string origvalrrludc = String.Empty;
            string origvalespudc = String.Empty;
            string origvalpcpudc = String.Empty;
            CreateAdditionalUDCAnalogPoint(SettingServiceStringConstants.RRL_GROUP_STATUS_EXTRA_UDCS, "RRLADDUDC", "RRLCRDAREA", out origvalrrludc, "hp", UnitCategory.Torque);
            CreateAdditionalUDCAnalogPoint(SettingServiceStringConstants.ESP_GROUP_STATUS_EXTRA_UDCS, "ESPUDCAddedByAPITest", "PRTUBXIN", out origvalespudc, "psi", UnitCategory.Pressure);
            CreateAdditionalUDCAnalogPoint(SettingServiceStringConstants.PCP_GROUP_STATUS_EXTRA_UDCS, "PCPUDCAddedByAPITest", "PRTUBXIN", out origvalespudc, "psi", UnitCategory.Pressure);
            try
            {
                WellDTO rrlwell = AddRRLWell(GetFacilityId("RPOC_", 1));
                WellDTO espwell = AddNonRRLWell(GetFacilityId("ESPWELL_", 1), WellTypeId.ESP);
                WellDTO pcpwell = AddNonRRLWell(GetFacilityId("WFTA1K_", 1), WellTypeId.PCP);

                _wellsToRemove.Add(rrlwell);
                _wellsToRemove.Add(espwell);
                _wellsToRemove.Add(pcpwell);

                var resultrrlWell = SurveillanceService.GetPointsAlarmConfig(rrlwell.Id.ToString());
                var resultespwell = SurveillanceService.GetPointsAlarmConfig(espwell.Id.ToString());
                var resultpcpwell = SurveillanceService.GetPointsAlarmConfig(pcpwell.Id.ToString());

                Assert.IsNotNull(resultrrlWell);

                // Randomize alarm limits and active flag and seed it into db.
                Random random = new Random();
                int loLoLimitMin = 10;
                int loLoLimitMax = 50;
                int loLimitMin = 50;
                int loLimitMax = 100;
                int hiLimitMin = 500;
                int hiLimitMax = 800;
                int hiHiLimitMin = 800;
                int hiHiLimitMax = 1000;
                // ************ Check for RRL Device Type WellPilot RPOC  ***********************************
                foreach (var item in resultrrlWell)
                {
                    item.LoLoLimit = random.Next(loLoLimitMin, loLoLimitMax);
                    item.LoLimit = random.Next(loLimitMin, loLimitMax);
                    item.HiLimit = random.Next(hiLimitMin, hiLimitMax);
                    item.HiHiLimit = random.Next(hiHiLimitMin, hiHiLimitMax);
                    item.IsActive = random.Next(100) % 2 == 0;
                }

                Assert.IsTrue(SurveillanceService.SavePointsAlarmConfig(rrlwell.Id.ToString(), resultrrlWell));
                var afterSaveResult = SurveillanceService.GetPointsAlarmConfig(rrlwell.Id.ToString());

                // We can also check the content of result and aftersaveResult. They should be same in count and each
                // properties.
                Assert.AreEqual(resultrrlWell.Count(), afterSaveResult.Count());

                // Remove the saved point alarm config from db.
                Assert.IsTrue(SurveillanceService.RemovePointsAlarmConfig(rrlwell.Id.ToString(), afterSaveResult));

                // Again get the point alarms config to ensure that there is no active point alarm in db
                var afterDelete = SurveillanceService.GetPointsAlarmConfig(rrlwell.Id.ToString());
                var pointAlarmConfig = afterDelete.Where(pac => pac.IsActive == true).ToList();
                Assert.AreEqual(0, pointAlarmConfig.Count);
                // ************ Check for ESP Device Type WKFord DLQ  ***********************************
                foreach (var item in resultespwell)
                {
                    item.LoLoLimit = random.Next(loLoLimitMin, loLoLimitMax);
                    item.LoLimit = random.Next(loLimitMin, loLimitMax);
                    item.HiLimit = random.Next(hiLimitMin, hiLimitMax);
                    item.HiHiLimit = random.Next(hiHiLimitMin, hiHiLimitMax);
                    item.IsActive = random.Next(100) % 2 == 0;
                }
                Assert.IsTrue(SurveillanceService.SavePointsAlarmConfig(espwell.Id.ToString(), resultespwell));
                afterSaveResult = SurveillanceService.GetPointsAlarmConfig(espwell.Id.ToString());

                // We can also check the content of result and aftersaveResult. They should be same in count and each
                // properties.
                Assert.AreEqual(resultespwell.Count(), afterSaveResult.Count());

                // Remove the saved point alarm config from db.
                Assert.IsTrue(SurveillanceService.RemovePointsAlarmConfig(espwell.Id.ToString(), afterSaveResult));

                // Again get the point alarms config to ensure that there is no active point alarm in db
                afterDelete = SurveillanceService.GetPointsAlarmConfig(espwell.Id.ToString());
                pointAlarmConfig = afterDelete.Where(pac => pac.IsActive == true).ToList();
                Assert.AreEqual(0, pointAlarmConfig.Count);
                // ************ Check for PCP Device Type WFTA1K  ***********************************

                foreach (var item in resultpcpwell)
                {
                    item.LoLoLimit = random.Next(loLoLimitMin, loLoLimitMax);
                    item.LoLimit = random.Next(loLimitMin, loLimitMax);
                    item.HiLimit = random.Next(hiLimitMin, hiLimitMax);
                    item.HiHiLimit = random.Next(hiHiLimitMin, hiHiLimitMax);
                    item.IsActive = random.Next(100) % 2 == 0;
                }

                Assert.IsTrue(SurveillanceService.SavePointsAlarmConfig(pcpwell.Id.ToString(), resultpcpwell));
                afterSaveResult = SurveillanceService.GetPointsAlarmConfig(pcpwell.Id.ToString());

                // We can also check the content of result and aftersaveResult. They should be same in count and each
                // properties.
                Assert.AreEqual(resultpcpwell.Count(), afterSaveResult.Count());
                ChangeUnitSystemUserSetting("Metric");
                afterSaveResult = SurveillanceService.GetPointsAlarmConfig(pcpwell.Id.ToString());
                afterSaveResult.FirstOrDefault(x => x.Name == "PCPUDCAddedByAPITest").LoLoLimit = 100;
                afterSaveResult.FirstOrDefault(x => x.Name == "PCPUDCAddedByAPITest").LoLimit = 200;
                afterSaveResult.FirstOrDefault(x => x.Name == "PCPUDCAddedByAPITest").HiLimit = 300;
                afterSaveResult.FirstOrDefault(x => x.Name == "PCPUDCAddedByAPITest").HiHiLimit = 400;

                afterSaveResult.FirstOrDefault(x => x.Quantity.ToString() == "CasingPressure").LoLoLimit = 100;
                afterSaveResult.FirstOrDefault(x => x.Quantity.ToString() == "CasingPressure").LoLimit = 200;
                afterSaveResult.FirstOrDefault(x => x.Quantity.ToString() == "CasingPressure").HiLimit = 300;
                afterSaveResult.FirstOrDefault(x => x.Quantity.ToString() == "CasingPressure").HiHiLimit = 400;

                Assert.IsTrue(SurveillanceService.SavePointsAlarmConfig(pcpwell.Id.ToString(), afterSaveResult));
                afterSaveResult = SurveillanceService.GetPointsAlarmConfig(pcpwell.Id.ToString());
                Assert.AreEqual(400, afterSaveResult.FirstOrDefault(x => x.Name == "PCPUDCAddedByAPITest").HiHiLimit, "Mismatch in value for HiHi Limit for Additional UDC");
                Assert.AreEqual(400, afterSaveResult.FirstOrDefault(x => x.Quantity.ToString() == "CasingPressure").HiHiLimit, "Mismatch in value for HiHi Limit for Casing Pressure");

                // Remove the saved point alarm config from db.
                Assert.IsTrue(SurveillanceService.RemovePointsAlarmConfig(pcpwell.Id.ToString(), afterSaveResult));

                // Again get the point alarms config to ensure that there is no active point alarm in db
                afterDelete = SurveillanceService.GetPointsAlarmConfig(pcpwell.Id.ToString());
                pointAlarmConfig = afterDelete.Where(pac => pac.IsActive == true).ToList();
                Assert.AreEqual(0, pointAlarmConfig.Count);
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                SetValuesInSystemSettings(SettingServiceStringConstants.RRL_GROUP_STATUS_EXTRA_UDCS, origvalrrludc);
                SetValuesInSystemSettings(SettingServiceStringConstants.ESP_GROUP_STATUS_EXTRA_UDCS, origvalespudc);
                SetValuesInSystemSettings(SettingServiceStringConstants.PCP_GROUP_STATUS_EXTRA_UDCS, origvalpcpudc);
            }
        }
        /// <summary>
        /// Story : FRWM-6778 Point Alarm Configuration 
        /// API task story : FRWM-6779 Verification of Point Alarm Procesing 
        /// </summary>

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void VerifyForeSitePointsConfigAlarms()
        {
            //Create All Well Types:  To be modified once PointConfig Alarm Code is ready

            VerifyPointAlarmForWell(GetFacilityId("ESPWELL_", 1), WellTypeId.ESP, "ESPUDCAddedByAPITest", "PRTUBXIN");
            VerifyPointAlarmForWell(GetFacilityId("GLWELL_", 1), WellTypeId.GLift, "GLUDCAddedByAPITest", "PRTUBXIN");
            // Handle Duplciate Alarm text to do
            VerifyPointAlarmForWell(GetFacilityId("NFWWELL_", 1), WellTypeId.NF, "NFUDCAddedByAPITest", "PRTUBXIN");
            VerifyPointAlarmForWell(GetFacilityId("GASINJWELL_", 1), WellTypeId.GInj, "GIUDCAddedByAPITest", "PRTUBXIN");
            VerifyPointAlarmForWell(GetFacilityId("WATERINJWELL_", 1), WellTypeId.WInj, "WIUDCAddedByAPITest", "PRTUBXIN");
            VerifyPointAlarmForWell(GetFacilityId("IWC_", 1), WellTypeId.WGInj, "WAGUDCAddedByAPITest", "PRESINJ");
            VerifyPointAlarmForWell(GetFacilityId("PGLWELL_", 1), WellTypeId.PLift, "PGLUDCAddedByAPITest", "PRTUBXIN");
            VerifyPointAlarmForWell(GetFacilityId("WFTA1K_", 1), WellTypeId.PCP, "PCPAddedByAPITest", "PRTUBXIN");
            VerifyPointAlarmForWell(GetFacilityId("NFWWELL_", 2), WellTypeId.OT, "OTUDCAddedByAPITest", "PRTUBXIN");
            //----------------Need to uncomment when the RRL Status alarm text prefix is available
            VerifyPointAlarmForWell(GetFacilityId("RPOC_", 1), WellTypeId.RRL, "RRLAddUDC", "QTLIQT");


        }


        /// <summary>
        /// FRWM-7052 : Daily Actual Rate calculations for production and injection wells
        /// API testing task : FRWM-7162
        /// </summary>
        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void VerifyDailyActualRateCalculationsForProductionInjectionWells()
        {
            try
            {
                //Add Wells with Run Time Less than 24 hours for Daily Avg Data

                DateTime day1ago = DateTime.Today.ToUniversalTime().AddDays(-1);
                DateTime day2ago = DateTime.Today.ToUniversalTime().AddDays(-2);
                DateTime end = DateTime.Today.ToUniversalTime();
                //// The Daily Average rate assigned to a well 
                double? dailyAverage = 300;
                int i = 0;
                #region 1. Add Wells of All Types
                WellDTO rpocwelltoadd = AddRRLWell(GetFacilityId("RPOC_", i + 1));
                WellDTO espwelltoadd = AddNonRRLWell(GetFacilityId("ESPWELL_", i + 1), WellTypeId.ESP);
                WellDTO glwelltoadd = AddNonRRLWell(GetFacilityId("GLWELL_", i + 1), WellTypeId.GLift);
                WellDTO nfwelltoadd = AddNonRRLWell(GetFacilityId("NFWWELL_", i + 1), WellTypeId.NF);
                WellDTO pcpwelltoadd = AddNonRRLWell(GetFacilityId("WFTA1K_", i + 1), WellTypeId.PCP);
                WellDTO giwelltoadd = AddNonRRLWell(GetFacilityId("GASINJWELL_", i + 1), WellTypeId.GInj);
                WellDTO wiwelltoadd = AddNonRRLWell(GetFacilityId("wATERINJWELL_", i + 1), WellTypeId.WInj);
                WellDTO wagwelltoadd = AddNonRRLWell(GetFacilityId("IWC_", i + 1), WellTypeId.WGInj);
                WellDTO pglwelltoadd = AddNonRRLWell(GetFacilityId("PGLWELL_", i + 1), WellTypeId.PLift);
                WellDTO OTwelltoadd = AddNonRRLWell(GetFacilityId("PGLWELL_", 2), WellTypeId.OT);
                _wellsToRemove.Add(rpocwelltoadd);
                _wellsToRemove.Add(espwelltoadd);
                _wellsToRemove.Add(glwelltoadd);
                _wellsToRemove.Add(nfwelltoadd);
                _wellsToRemove.Add(pcpwelltoadd);
                _wellsToRemove.Add(giwelltoadd);
                _wellsToRemove.Add(wiwelltoadd);
                _wellsToRemove.Add(wagwelltoadd);
                _wellsToRemove.Add(pglwelltoadd);
                #endregion

                #region 2. Add Daily Averages to All Well Types 
                //Add Daily Averages with Run tIme Less than 24 hours ESP Well
                AddWellDailyAvergeDataRunTimeLessthan24Hour(espwelltoadd, day1ago, end, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                AddWellDailyAvergeDataRunTimeLessthan24Hour(espwelltoadd, day2ago, day1ago, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                //Add Daily Averages with Run tIme Less than 24 hours GL Well
                AddWellDailyAvergeDataRunTimeLessthan24Hour(glwelltoadd, day1ago, end, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                AddWellDailyAvergeDataRunTimeLessthan24Hour(glwelltoadd, day2ago, day1ago, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;

                //Add Daily Averages with Run tIme Less than 24 hours NF BO Well
                AddWellDailyAvergeDataRunTimeLessthan24Hour(nfwelltoadd, day1ago, end, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                AddWellDailyAvergeDataRunTimeLessthan24Hour(nfwelltoadd, day2ago, day1ago, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;

                //Add Daily Averages with Run tIme Less than 24 hours RPOC Well
                AddWellDailyAvergeDataRunTimeLessthan24Hour(rpocwelltoadd, day1ago, end, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                AddWellDailyAvergeDataRunTimeLessthan24Hour(rpocwelltoadd, day2ago, day1ago, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                //Add Daily Averages with Run tIme Less than 24 hours PGL Well
                AddWellDailyAvergeDataRunTimeLessthan24Hour(pglwelltoadd, day1ago, end, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                AddWellDailyAvergeDataRunTimeLessthan24Hour(pglwelltoadd, day2ago, day1ago, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                //Add Daily Averages with Run tIme Less than 24 hours PCP Well
                AddWellDailyAvergeDataRunTimeLessthan24Hour(pcpwelltoadd, day1ago, end, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                AddWellDailyAvergeDataRunTimeLessthan24Hour(pcpwelltoadd, day2ago, day1ago, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                //Add Daily Averages with Run tIme Less than 24 hours GI Well

                AddWellDailyAvergeDataRunTimeLessthan24Hour(giwelltoadd, day1ago, end, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                AddWellDailyAvergeDataRunTimeLessthan24Hour(giwelltoadd, day2ago, day1ago, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                //Add Daily Averages with Run tIme Less than 24 hours WI Well
                AddWellDailyAvergeDataRunTimeLessthan24Hour(wiwelltoadd, day1ago, end, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                AddWellDailyAvergeDataRunTimeLessthan24Hour(wiwelltoadd, day2ago, day1ago, dailyAverage, dailyAverage + 100, dailyAverage + 200);
                dailyAverage = dailyAverage + 50;
                #endregion

                //Ensure WAG and OT  types do not throw Error on Well Status 
                #region 3. Compare Production KPI numbers from Well Status for all Well Types
                CompareScaledValueofRateinProdKPI(espwelltoadd, end, "US");
                CompareScaledValueofRateinProdKPI(glwelltoadd, end, "US");
                CompareScaledValueofRateinProdKPI(nfwelltoadd, end, "US");

                CompareScaledValueofRateinProdKPI(giwelltoadd, end, "US");
                CompareScaledValueofRateinProdKPI(wiwelltoadd, end, "US");
                CompareScaledValueofRateinProdKPI(pcpwelltoadd, end, "US");
                CompareScaledValueofRateinProdKPI(rpocwelltoadd, end, "US");
                CompareScaledValueofRateinProdKPI(pglwelltoadd, end, "US");
                //      CompareScaledValueofRateinProdKPI(wagwelltoadd, end, "US");
                //      CompareScaledValueofRateinProdKPI(OTwelltoadd, end, "US");

                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");

                CompareScaledValueofRateinProdKPI(espwelltoadd, end, "Metric");
                CompareScaledValueofRateinProdKPI(glwelltoadd, end, "Metric");
                CompareScaledValueofRateinProdKPI(nfwelltoadd, end, "Metric");

                CompareScaledValueofRateinProdKPI(giwelltoadd, end, "Metric");
                CompareScaledValueofRateinProdKPI(wiwelltoadd, end, "Metric");
                CompareScaledValueofRateinProdKPI(pcpwelltoadd, end, "Metric");
                CompareScaledValueofRateinProdKPI(rpocwelltoadd, end, "Metric");
                CompareScaledValueofRateinProdKPI(pglwelltoadd, end, "Metric");
                //     CompareScaledValueofRateinProdKPI(wagwelltoadd, end, "Metric");
                //    CompareScaledValueofRateinProdKPI(OTwelltoadd, end, "Metric");
                #endregion

                //Now Do All Required Verifications
                //Well Status Production KPI  figures for each well Type ; ensuer that API does not throw error for RPOC , WAG 
                //All Wells Filters
                #region 4. Verify PE Dashbaord Units 
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
                VerifyPEdashboardWithWellStatusProductionKPI(end, "US", 0.1);
                ChangeUnitSystem("Metric");
                ChangeUnitSystemUserSetting("Metric");
                VerifyPEdashboardWithWellStatusProductionKPI(end, "Metric", 0.01);


                #endregion

                //PE Dashboard Data
                // Production Overview Cumulative Chart

            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
            }
        }

        public void AddWellDailyAvergeDataRunTimeLessthan24Hour(WellDTO well, DateTime start, DateTime end, double? oilallocated, double? wateralloated, double? gasallocated)
        {
            Authenticate();
            WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO
            {
                EndDateTime = end,
                StartDateTime = start,
                GasRateAllocated = gasallocated,
                GasRateInferred = gasallocated + 10,
                OilRateAllocated = oilallocated,
                OilRateInferred = oilallocated + 10,
                WaterRateAllocated = wateralloated,
                WaterRateInferred = wateralloated + 10,
                Status = WellDailyAverageDataStatus.Calculated,
                Duration = 24,
                GasJectionDepth = 1000,
                ChokeDiameter = 64.0,
                RunTime = 14,
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
            // FRWM-7027 : API testing subtask FRWM-7244
            dailyAverageDTO.OilMeasuredRate = oilallocated;
            dailyAverageDTO.WaterMeasuredRate = wateralloated;
            dailyAverageDTO.GasMeasuredRate = gasallocated;
            if (well.WellType == WellTypeId.GInj)
            {
                dailyAverageDTO.OilRateAllocated = 0.0;
                dailyAverageDTO.WaterRateAllocated = 0.0;
                dailyAverageDTO.OilRateInferred = 0.0;
                dailyAverageDTO.WaterRateInferred = 0.0;
                dailyAverageDTO.GasRateInferred = 0.0;

            }
            else if (well.WellType == WellTypeId.WInj)
            {
                dailyAverageDTO.OilRateAllocated = 0.0;
                dailyAverageDTO.GasRateAllocated = 0.0;
                dailyAverageDTO.OilRateInferred = 0.0;
                dailyAverageDTO.WaterRateInferred = 0.0;
                dailyAverageDTO.GasRateInferred = 0.0;
            }
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
        }

        public void CompareScaledValueofRateinProdKPI(WellDTO well, DateTime wdaendate, string unitsystem)
        {
            Trace.WriteLine($"Comparing the Prodcution KPI for Well {well.Name} and Well Type : {well.WellType.ToString()} ");
            string startdate = DateTime.Today.ToUniversalTime().AddDays(-4).ToISO8601().ToString();
            string enddate = DateTime.Today.ToUniversalTime().AddDays(1).ToISO8601().ToString();

            var wellstatusdailyavgdata = SurveillanceService.GetDailyAverages(well.Id.ToString(), startdate, enddate);
            // Yesterday data

            double actualoilrate = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate).OilRateAllocated;
            double actualwaterate = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate).WaterRateAllocated;
            double actualgasrate = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate).GasRateAllocated;
            double runtime = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate).RunTime;
            double actualoilvol = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate).AllocatedOilVolume;
            double actualwatervol = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate).AllocatedWaterVolume;
            double actualgasvol = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate).AllocatedGasVolume;

            double inferedoilrate = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate).OilRateInferred;
            double inferedwaterate = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate).WaterRateInferred;
            double inferedgasrate = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate).GasRateInferred;

            double inferedoilvol = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate).InferredOilVolume;
            double inferedwatervol = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate).InferredWaterVolume;
            double inferedgasvol = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate).InferredGasVolume;

            //Day before yesterdday data
            double actualoilrate2d = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate.AddDays(-1)).OilRateAllocated;
            double actualwaterate2d = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate.AddDays(-1)).WaterRateAllocated;
            double actualgasrate2d = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate.AddDays(-1)).GasRateAllocated;
            double runtime2d = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate.AddDays(-1)).RunTime;
            double actualoilvol2d = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate.AddDays(-1)).AllocatedOilVolume;
            double actualwatervol2d = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate.AddDays(-1)).AllocatedWaterVolume;
            double actualgasvol2d = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate.AddDays(-1)).AllocatedGasVolume;
            double inferedoilrate2d = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate.AddDays(-1)).OilRateInferred;
            double inferedwaterate2d = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate.AddDays(-1)).WaterRateInferred;
            double inferedgasrate2d = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate.AddDays(-1)).GasRateInferred;
            double inferedoilvol2d = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate.AddDays(-1)).InferredOilVolume;
            double inferedwatervol2d = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate.AddDays(-1)).InferredWaterVolume;
            double inferedgasvol2d = (double)wellstatusdailyavgdata.Values.FirstOrDefault(x => x.EndDateTime == wdaendate.AddDays(-1)).InferredGasVolume;



            WellDTO giwelltoadd1 = WellService.GetWell(well.Id.ToString());
            var prodkpi = SurveillanceService.GetProductionKPI(well.Id.ToString(), wdaendate.ToISO8601().ToString());
            if (unitsystem.ToLower() == "us")
            {
                Assert.AreEqual("Mscf", prodkpi.Units.Gas, "Production gas Unit was not MScf");
                Assert.AreEqual("STB", prodkpi.Units.Water, "Production Water Unit was not STB");
                Assert.AreEqual("STB", prodkpi.Units.Oil, "Production Oil  Unit was not STB");
                Assert.AreEqual("STB", prodkpi.Units.Liquid, "Production Liquid  Unit was not STB");
            }
            else if (unitsystem.ToLower() == "metric")
            {
                Assert.AreEqual("sm3", prodkpi.Units.Gas, "Production gas Unit was not sm3");
                Assert.AreEqual("sm3", prodkpi.Units.Water, "Production Water Unit was not sm3");
                Assert.AreEqual("sm3", prodkpi.Units.Oil, "Production Oil  Unit was not sm3");
                Assert.AreEqual("sm3", prodkpi.Units.Liquid, "Production Liquid  Unit was not sm3");
            }

            //Assert that values are scaled by RunTime for Yesterday Actual

            Assert.AreEqual(actualoilrate * (runtime / 24), (double)prodkpi.CurrentAllocatedProduction.Oil, 0.1, " Actual Oll Rate Was not scaled as per RunTime ");
            Assert.AreEqual(actualwaterate * (runtime / 24), (double)prodkpi.CurrentAllocatedProduction.Water, 0.1, "Actual Water Rate Was not scaled as per RunTime ");
            Assert.AreEqual(actualgasrate * (runtime / 24), (double)prodkpi.CurrentAllocatedProduction.Gas, 0.1, "Actual Gas Rate Was not scaled as per RunTime ");
            Assert.AreEqual((actualoilrate * (runtime / 24) + actualwaterate * (runtime / 24)), (double)prodkpi.CurrentAllocatedProduction.Liquid, 0.1, "Actual Liquid   Rate Was not scaled as per RunTime ");

            Assert.AreEqual(actualoilvol, (double)prodkpi.CurrentAllocatedProduction.Oil, 0.1, " Actual Oll Rate Was not scaled as per RunTime ");
            Assert.AreEqual(actualwatervol, (double)prodkpi.CurrentAllocatedProduction.Water, 0.1, "Actual Water Rate Was not scaled as per RunTime ");
            Assert.AreEqual(actualgasvol, (double)prodkpi.CurrentAllocatedProduction.Gas, 0.1, "Actual Gas Rate Was not scaled as per RunTime ");


            //Assert that values are scaled by RunTime for Yesterday Inferred

            Assert.AreEqual(inferedoilrate * (runtime / 24), (double)prodkpi.CurrentInferredProduction.Oil, 0.1, " Actual Oll Rate Was not scaled as per RunTime ");
            Assert.AreEqual(inferedwaterate * (runtime / 24), (double)prodkpi.CurrentInferredProduction.Water, 0.1, "Actual Water Rate Was not scaled as per RunTime ");
            Assert.AreEqual(inferedgasrate * (runtime / 24), (double)prodkpi.CurrentInferredProduction.Gas, 0.1, "Actual Gas Rate Was not scaled as per RunTime ");
            Assert.AreEqual((inferedoilrate * (runtime / 24) + inferedwaterate * (runtime / 24)), (double)prodkpi.CurrentInferredProduction.Liquid, 0.1, "Actual Liquid   Rate Was not scaled as per RunTime ");
            Assert.AreEqual(inferedoilvol, (double)prodkpi.CurrentInferredProduction.Oil, 0.1, " Actual Oll Rate Was not scaled as per RunTime ");
            Assert.AreEqual(inferedwatervol, (double)prodkpi.CurrentInferredProduction.Water, 0.1, "Actual Water Rate Was not scaled as per RunTime ");
            Assert.AreEqual(inferedgasvol, (double)prodkpi.CurrentInferredProduction.Gas, 0.1, "Actual Gas Rate Was not scaled as per RunTime ");


            //Asert that values are scaled by RunTime for DaybeforeYesterday Actual

            Assert.AreEqual(actualoilrate2d * (runtime / 24), (double)prodkpi.LastAllocatedProduction.Oil, 0.1, " Actual Oll Rate Was not scaled as per RunTime ");
            Assert.AreEqual(actualwaterate2d * (runtime / 24), (double)prodkpi.LastAllocatedProduction.Water, 0.1, "Actual Water Rate Was not scaled as per RunTime ");
            Assert.AreEqual(actualgasrate2d * (runtime / 24), (double)prodkpi.LastAllocatedProduction.Gas, 0.1, "Actual Gas Rate Was not scaled as per RunTime ");
            Assert.AreEqual((actualoilrate2d * (runtime / 24) + actualwaterate2d * (runtime / 24)), (double)prodkpi.LastAllocatedProduction.Liquid, 0.1, "Actual Liquid   Rate Was not scaled as per RunTime ");
            Assert.AreEqual(actualoilvol2d, (double)prodkpi.LastAllocatedProduction.Oil, 0.1, " Actual Oll Rate Was not scaled as per RunTime ");
            Assert.AreEqual(actualwatervol2d, (double)prodkpi.LastAllocatedProduction.Water, 0.1, "Actual Water Rate Was not scaled as per RunTime ");
            Assert.AreEqual(actualgasvol2d, (double)prodkpi.LastAllocatedProduction.Gas, 0.1, "Actual Gas Rate Was not scaled as per RunTime ");
            //Asert that values are scaled by RunTime for DaybeforeYesterday Inferred

            Assert.AreEqual(inferedoilrate2d * (runtime / 24), (double)prodkpi.LastInferredProduction.Oil, 0.1, " Actual Oll Rate Was not scaled as per RunTime ");
            Assert.AreEqual(inferedwaterate2d * (runtime / 24), (double)prodkpi.LastInferredProduction.Water, 0.1, "Actual Water Rate Was not scaled as per RunTime ");
            Assert.AreEqual(inferedgasrate2d * (runtime / 24), (double)prodkpi.LastInferredProduction.Gas, 0.1, "Actual Gas Rate Was not scaled as per RunTime ");
            Assert.AreEqual((inferedoilrate2d * (runtime / 24) + inferedwaterate2d * (runtime / 24)), (double)prodkpi.LastInferredProduction.Liquid, 0.1, "Actual Liquid   Rate Was not scaled as per RunTime ");
            Assert.AreEqual(inferedoilvol2d, (double)prodkpi.LastInferredProduction.Oil, 0.1, " Actual Oll Rate Was not scaled as per RunTime ");
            Assert.AreEqual(inferedwatervol2d, (double)prodkpi.LastInferredProduction.Water, 0.1, "Actual Water Rate Was not scaled as per RunTime ");
            Assert.AreEqual(inferedgasvol2d, (double)prodkpi.LastInferredProduction.Gas, 0.1, "Actual Gas Rate Was not scaled as per RunTime ");

            Trace.WriteLine($" Prodcution KPI Check  for Well {well.Name} and Well Type : {well.WellType.ToString()}  Completed ");

        }

        public void VerifyPEdashboardWithWellStatusProductionKPI(DateTime endtime, string unitsys, double precisioncheck)
        {
            // From Production Overview ;
            Authenticate();
            WellDTO[] allwells = WellService.GetAllWells();
            List<double> oiltodayvalyes = new List<double>();
            List<double> watertodayvalyes = new List<double>();
            List<double> gastodayvalyes = new List<double>();
            List<double> liquidtodayvalyes = new List<double>();

            List<double> oilyestvalyes = new List<double>();
            List<double> wateryestyvalyes = new List<double>();
            List<double> gasyestvalyes = new List<double>();
            List<double> liquidyestvalyes = new List<double>();

            foreach (WellDTO well in allwells)
            {
                if (well.WellType == WellTypeId.GInj || well.WellType == WellTypeId.WInj || well.WellType == WellTypeId.WGInj || well.WellType == WellTypeId.OT)
                {
                    continue;
                }

                var prodkpidto = SurveillanceService.GetProductionKPI(well.Id.ToString(), endtime.ToISO8601().ToString());
                oiltodayvalyes.Add((double)prodkpidto.CurrentAllocatedProduction.Oil);
                watertodayvalyes.Add((double)prodkpidto.CurrentAllocatedProduction.Water);
                gastodayvalyes.Add((double)prodkpidto.CurrentAllocatedProduction.Gas);
                liquidtodayvalyes.Add((double)prodkpidto.CurrentAllocatedProduction.Liquid);

                oilyestvalyes.Add((double)prodkpidto.LastAllocatedProduction.Oil);
                wateryestyvalyes.Add((double)prodkpidto.LastAllocatedProduction.Water);
                gasyestvalyes.Add((double)prodkpidto.LastAllocatedProduction.Gas);
                liquidyestvalyes.Add((double)prodkpidto.LastAllocatedProduction.Liquid);

            }
            WellFilterDTO wellFilter = WellService.GetWellFilter(null);
            TileMapFilterDTO tileMap = new TileMapFilterDTO();
            tileMap.WellFilter = new WellFilterDTO();
            var cumdata = SurveillanceService.GetWellDailyCumulativeData(tileMap, "3", endtime.AddDays(-7).ToISO8601().ToString(), endtime.AddDays(1).ToISO8601().ToString());
            if (unitsys.ToLower() == "us")
            {
                Assert.AreEqual("STB", cumdata.Units.AverageProductionUnit, "Liquid Avg Prod Unit is not STB");
                Assert.AreEqual("STB", cumdata.Units.CumulativeProductionUnit, "Liquid Cumulative Prod Unit is not STB");
            }
            else if (unitsys.ToLower() == "metric")
            {
                Assert.AreEqual("sm3", cumdata.Units.AverageProductionUnit, "Liquid Avg Prod Unit is not sm3");
                Assert.AreEqual("sm3", cumdata.Units.CumulativeProductionUnit, "Liquid Cumulative Prod Unit is not sm3");
            }

            Assert.AreEqual(liquidtodayvalyes.Sum(), (double)(cumdata.Values.DailyCumulativeProduction.FirstOrDefault(x => x.Date == endtime.AddDays(-1)).DailyCumulativeProduction - cumdata.Values.DailyCumulativeProduction.FirstOrDefault(x => x.Date == endtime.AddDays(-2)).DailyCumulativeProduction)
                                              , precisioncheck, "Liquid Sum Mismatch in Cumulavtie Chart");

            cumdata = SurveillanceService.GetWellDailyCumulativeData(tileMap, ProductionType.Oil.ToString(), endtime.AddDays(-7).ToISO8601().ToString(), endtime.AddDays(1).ToISO8601().ToString());
            if (unitsys.ToLower() == "us")
            {
                Assert.AreEqual("STB", cumdata.Units.AverageProductionUnit, "Oil Avg Prod Unit is not STB");
                Assert.AreEqual("STB", cumdata.Units.CumulativeProductionUnit, "Oil Cumulative Prod Unit is not STB");
            }
            else if (unitsys.ToLower() == "metric")
            {
                Assert.AreEqual("sm3", cumdata.Units.AverageProductionUnit, "Oil Avg Prod Unit is not sm3");
                Assert.AreEqual("sm3", cumdata.Units.CumulativeProductionUnit, "Oil Cumulative Prod Unit is not sm3");
            }

            Assert.AreEqual(oiltodayvalyes.Sum(), (double)(cumdata.Values.DailyCumulativeProduction.FirstOrDefault(x => x.Date == endtime.AddDays(-1)).DailyCumulativeProduction - cumdata.Values.DailyCumulativeProduction.FirstOrDefault(x => x.Date == endtime.AddDays(-2)).DailyCumulativeProduction), precisioncheck, "Oil Sum Mismatch in Cumulavtie Chart");

            cumdata = SurveillanceService.GetWellDailyCumulativeData(tileMap, ProductionType.Water.ToString(), endtime.AddDays(-7).ToISO8601().ToString(), endtime.AddDays(1).ToISO8601().ToString());
            if (unitsys.ToLower() == "us")
            {
                Assert.AreEqual("STB", cumdata.Units.AverageProductionUnit, "Water Avg Prod Unit is not STB");
                Assert.AreEqual("STB", cumdata.Units.CumulativeProductionUnit, "Water Cumulative Prod Unit is not STB");
            }
            else if (unitsys.ToLower() == "metric")
            {
                Assert.AreEqual("sm3", cumdata.Units.AverageProductionUnit, "Water Avg Prod Unit is not sm3");
                Assert.AreEqual("sm3", cumdata.Units.CumulativeProductionUnit, "Water Cumulative Prod Unit is not sm3");
            }

            Assert.AreEqual(watertodayvalyes.Sum(), (double)(cumdata.Values.DailyCumulativeProduction.FirstOrDefault(x => x.Date == endtime.AddDays(-1)).DailyCumulativeProduction - cumdata.Values.DailyCumulativeProduction.FirstOrDefault(x => x.Date == endtime.AddDays(-2)).DailyCumulativeProduction), precisioncheck, "Water Sum Mismatch in Cumulavtie Chart");

            cumdata = SurveillanceService.GetWellDailyCumulativeData(tileMap, ProductionType.Gas.ToString(), endtime.AddDays(-7).ToISO8601().ToString(), endtime.AddDays(1).ToISO8601().ToString());
            if (unitsys.ToLower() == "us")
            {
                Assert.AreEqual("Mscf", cumdata.Units.AverageProductionUnit, "Gas Avg Prod Unit is not Mscf");
                Assert.AreEqual("Mscf", cumdata.Units.CumulativeProductionUnit, "Gas Cumulative Prod Unit is not Mscf");
            }
            else if (unitsys.ToLower() == "metric")
            {
                Assert.AreEqual("sm3", cumdata.Units.AverageProductionUnit, "Gas Avg Prod Unit is not sm3");
                Assert.AreEqual("sm3", cumdata.Units.CumulativeProductionUnit, "Gas Cumulative Prod Unit is not sm3");
            }

            Assert.AreEqual(gastodayvalyes.Sum(), (double)(cumdata.Values.DailyCumulativeProduction.FirstOrDefault(x => x.Date == endtime.AddDays(-1)).DailyCumulativeProduction - cumdata.Values.DailyCumulativeProduction.FirstOrDefault(x => x.Date == endtime.AddDays(-2)).DailyCumulativeProduction), precisioncheck, "Gas Sum Mismatch in Cumulavtie Chart");

            Trace.WriteLine("***********Verified Production Overview Cumultive Data with Sum  of Individual  Wells Data from that  Group Filter for each Production Phase : Liquid ,Oil Water and  Gas along with respective Units...!! ");
            PETrendPointsDTO allTrends = PEDashboardService.GetAllTrends(wellFilter);
            if (unitsys.ToLower() == "us")
            {
                Trace.WriteLine($"Verifying the PE Dashnboard with Unit Syste {unitsys}");
                Assert.AreEqual("STB", allTrends.Units.OilProduction.UnitKey, "PE dashboard Trends chart Unit for Oil is not STB");
                Assert.AreEqual("STB", allTrends.Units.WaterProduction.UnitKey, "PE dashboard Trends chart Unit for Water is not STB");
                Assert.AreEqual("Mscf", allTrends.Units.GasProduction.UnitKey, "PE dashboard Trends chart Unit for Gas is not Mscf");
                Assert.AreEqual("STB", allTrends.Units.LiquidProduction.UnitKey, "PE dashboard Trends chart Unit for Liquid is not STB");
            }
            else if (unitsys.ToLower() == "metric")
            {
                Trace.WriteLine($"Verifying the PE Dashnboard with Unit Syste {unitsys}");
                Assert.AreEqual("sm3", allTrends.Units.OilProduction.UnitKey, "PE dashboard Trends chart Unit for Oil is not sm3");
                Assert.AreEqual("sm3", allTrends.Units.WaterProduction.UnitKey, "PE dashboard Trends chart Unit for Water is not sm3");
                Assert.AreEqual("sm3", allTrends.Units.GasProduction.UnitKey, "PE dashboard Trends chart Unit for Gas is not sm3");
                Assert.AreEqual("sm3", allTrends.Units.LiquidProduction.UnitKey, "PE dashboard Trends chart Unit for Liquid is not sm3");
            }

            double? lqtodyval = allTrends.Values.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-1).ToShortDateString()).Production;
            double? lqyestday = allTrends.Values.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-2).ToShortDateString()).Production;
            //make sure value is scaled Volume as per Run time
            // in this case It should be 

            //Trend Chart Values with KPI for Oil
            double? oiltodyval = allTrends.Values.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-1).ToShortDateString()).OilProduction;
            double? oilyestday = allTrends.Values.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-2).ToShortDateString()).OilProduction;

            //Trend Chart Values with KPI for Water
            double? wttodyval = allTrends.Values.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-1).ToShortDateString()).WaterProduction;
            double? wtqyestday = allTrends.Values.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-2).ToShortDateString()).WaterProduction;

            //Trend Chart Values with KPI for Gas
            double? gstodyval = allTrends.Values.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-1).ToShortDateString()).GasProduction;
            double? gsyestday = allTrends.Values.FirstOrDefault(x => x.Timestamp.ToLocalTime().ToShortDateString() == DateTime.Today.AddDays(-2).ToShortDateString()).GasProduction;
            //Get List Of All Wells Volume Sum them to match




            Assert.AreEqual(oiltodayvalyes.Sum(), (double)oiltodyval, precisioncheck, "Oil Today Mismatch in PE Dashboard Trends Chart value");
            Assert.AreEqual(watertodayvalyes.Sum(), (double)wttodyval, precisioncheck, "Water Today Mismatch in PE Dashboard Trends Chart value");
            Assert.AreEqual(gastodayvalyes.Sum(), (double)gstodyval, precisioncheck, "Gas Today Mismatch in PE Dashboard Trends Chart value");
            Assert.AreEqual(oiltodyval + wttodyval, lqtodyval, "Liquid != Oil + Water !!! in PE dashboard KPI");
            Assert.AreEqual(liquidtodayvalyes.Sum(), (double)lqtodyval, precisioncheck, "Liquid Today Mismatch in PE Dashboard Trends Chart value");

            Assert.AreEqual(oilyestvalyes.Sum(), (double)oilyestday, precisioncheck, "Oil Yesterday  Mismatch");
            Assert.AreEqual(wateryestyvalyes.Sum(), (double)wtqyestday, precisioncheck, "Water Yesterday Mismatch");
            Assert.AreEqual(gasyestvalyes.Sum(), (double)gsyestday, precisioncheck, "Gas Yesterday Mismatch");
            Assert.AreEqual(liquidyestvalyes.Sum(), (double)lqyestday, precisioncheck, "Liquid Yesterday Mismatch");

            Trace.WriteLine($"Verification Done for {unitsys} with precision of {precisioncheck}");


            // For Production OverView Make Sure that...
        }



        public void VerifyPointAlarmForWell(string facid, WellTypeId welltype, string AddUDCName, string AddUDC)
        {
            string origadditionaludcstringvalue = String.Empty;
            try
            {
                switch (welltype)
                {
                    case WellTypeId.RRL:
                        {
                            try
                            {
                                //Add Well
                                //Add Addtional UDC 
                                CreateAdditionalUDCAnalogPoint(SettingServiceStringConstants.RRL_GROUP_STATUS_EXTRA_UDCS, AddUDCName, AddUDC, out origadditionaludcstringvalue);
                                WellDTO rrlWell = AddRRLWell(facid);
                                _wellsToRemove.Add(rrlWell);
                                //Set Point Alarms Config For RRL :

                                Trace.WriteLine($"Verifying the Point Config Alarm for Well Type : {welltype.ToString()}");
                                SetClearPointAlarmConfig(rrlWell, "CardArea", 5, 10, 20, 25, true);

                                // Change or Inactive
                                //Scan for RESET
                                SurveillanceService.IssueCommandForSingleWell(rrlWell.Id, WellCommand.DemandScan.ToString());
                                var rrlwellstatusdata = SurveillanceService.GetWellStatusData(rrlWell.Id.ToString());
                                RRLWellStatusValueDTO rrlvalue = (RRLWellStatusValueDTO)rrlwellstatusdata.Value;
                                IntelligentAlarmsServiceTests ialmtest = new IntelligentAlarmsServiceTests();
                                List<string> expalams = new List<string>();
                                string start_date = (DateTime.Today - TimeSpan.FromDays(30)).ToUniversalTime().ToISO8601();
                                string end_date = DateTime.Today.ToUniversalTime().ToISO8601();
                                expalams.Add("HighHigh Card Area");
                                Trace.WriteLine($"Verifying If Alarm is Set on Well Status Page , Alarm History ");
                                //Verify If Set
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(rrlvalue.IntelligentAlarmMessage, expalams, true);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(rrlWell, expalams, start_date, end_date, true);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(rrlWell, expalams, start_date, end_date, true);
                                //Verify If Clear
                                Trace.WriteLine($"Verifying If Alarm is Cleared on Well Status Page , Alarm History ");
                                SetClearPointAlarmConfig(rrlWell, "CardArea", 5, 10, 30, 35, false);
                                SurveillanceService.IssueCommandForSingleWell(rrlWell.Id, WellCommand.DemandScan.ToString());
                                rrlwellstatusdata = SurveillanceService.GetWellStatusData(rrlWell.Id.ToString());
                                rrlvalue = (RRLWellStatusValueDTO)rrlwellstatusdata.Value;
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(rrlvalue.IntelligentAlarmMessage, expalams, false);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(rrlWell, expalams, start_date, end_date, false);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(rrlWell, expalams, start_date, end_date, false);
                            }
                            finally
                            {
                                //Clean for RRL
                                SetValuesInSystemSettings(SettingServiceStringConstants.RRL_GROUP_STATUS_EXTRA_UDCS, origadditionaludcstringvalue);
                            }
                            break;
                        }
                    case WellTypeId.ESP:

                        {
                            try
                            {
                                CreateAdditionalUDCAnalogPoint(SettingServiceStringConstants.ESP_GROUP_STATUS_EXTRA_UDCS, AddUDCName, AddUDC, out origadditionaludcstringvalue);
                                WellDTO espWell = AddNonRRLWell(facid, WellTypeId.ESP);
                                _wellsToRemove.Add(espWell);
                                //Set Point Alarms Config For ESP :
                                Trace.WriteLine($"Verifying the Point Config Alarm for Well Type : {welltype.ToString()}");
                                SetClearPointAlarmConfig(espWell, "TubingPressure", 50, 100, 200, 250, true);
                                SetClearPointAlarmConfig(espWell, AddUDCName, 50, 100, 200, 250, true);
                                SetPointValue(new FacilityTag(s_site, s_cvsService, facid), "PRTUBXIN", "40", DateTime.Now);

                                // Change or Inactive
                                //Scan for RESET
                                SurveillanceService.IssueCommandForSingleWell(espWell.Id, WellCommand.DemandScan.ToString());
                                var rrlwellstatusdata = SurveillanceService.GetWellStatusData(espWell.Id.ToString());
                                ESPWellStatusValueDTO espvalue = (ESPWellStatusValueDTO)rrlwellstatusdata.Value;
                                IntelligentAlarmsServiceTests ialmtest = new IntelligentAlarmsServiceTests();
                                List<string> expalams = new List<string>();
                                string start_date = (DateTime.Today - TimeSpan.FromDays(30)).ToUniversalTime().ToISO8601();
                                string end_date = DateTime.Today.ToUniversalTime().ToISO8601();
                                expalams.Add("LowLow Tubing Pressure");
                                expalams.Add($"LowLow {AddUDCName}");
                                Trace.WriteLine($"Verifying If Alarm is Set on Well Status Page , Alarm History ");
                                //Verify If Set
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, true);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(espWell, expalams, start_date, end_date, true);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(espWell, expalams, start_date, end_date, true);
                                //Verify If Clear
                                Trace.WriteLine($"Verifying If Alarm is Cleared on Well Status Page , Alarm History ");
                                //   SetClearPointAlarmConfig(espWell, "TubingPressure", 5, 10, 20, 25, false);
                                SetPointValue(new FacilityTag(s_site, s_cvsService, facid), "PRTUBXIN", "150", DateTime.Now);
                                // Change or Inactive
                                //Scan for RESET
                                SurveillanceService.IssueCommandForSingleWell(espWell.Id, WellCommand.DemandScan.ToString());
                                rrlwellstatusdata = SurveillanceService.GetWellStatusData(espWell.Id.ToString());
                                espvalue = (ESPWellStatusValueDTO)rrlwellstatusdata.Value;
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, false);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(espWell, expalams, start_date, end_date, false);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(espWell, expalams, start_date, end_date, false);
                                break;
                            }
                            finally
                            {
                                SetValuesInSystemSettings(SettingServiceStringConstants.ESP_GROUP_STATUS_EXTRA_UDCS, origadditionaludcstringvalue);
                            }
                        }
                    case WellTypeId.GLift:
                        {
                            try
                            {
                                CreateAdditionalUDCAnalogPoint(SettingServiceStringConstants.GL_GROUP_STATUS_EXTRA_UDCS, AddUDCName, AddUDC, out origadditionaludcstringvalue);
                                WellDTO glWell = AddNonRRLWell(facid, WellTypeId.GLift);
                                _wellsToRemove.Add(glWell);
                                //Set Point Alarms Config For GL :
                                Trace.WriteLine($"Verifying the Point Config Alarm for Well Type : {welltype.ToString()}");
                                ChangeUnitSystemUserSetting("Metric");
                                SetClearPointAlarmConfig(glWell, "TubingPressure", (decimal?)UnitsConversion("psia", 50), (decimal?)UnitsConversion("psia", 100), (decimal?)UnitsConversion("psia", 200), (decimal?)UnitsConversion("psia", 250), true);
                                SetPointValue(new FacilityTag(s_site, s_cvsService, facid), "PRTUBXIN", "45", DateTime.Now);
                                // Change or Inactive
                                //Scan for RESET
                                SurveillanceService.IssueCommandForSingleWell(glWell.Id, WellCommand.DemandScan.ToString());
                                var rrlwellstatusdata = SurveillanceService.GetWellStatusData(glWell.Id.ToString());
                                GLWellStatusValueDTO espvalue = (GLWellStatusValueDTO)rrlwellstatusdata.Value;
                                IntelligentAlarmsServiceTests ialmtest = new IntelligentAlarmsServiceTests();
                                List<string> expalams = new List<string>();
                                string start_date = (DateTime.Today - TimeSpan.FromDays(30)).ToUniversalTime().ToISO8601();
                                string end_date = DateTime.Today.ToUniversalTime().ToISO8601();
                                expalams.Add("LowLow Tubing Pressure");
                                Trace.WriteLine($"Verifying If Alarm is Set on Well Status Page , Alarm History ");
                                //Verify If Set
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, true);
                                
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(glWell, expalams, start_date, end_date, true);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(glWell, expalams, start_date, end_date, true, "45");
                                //Verify If Clear
                                Trace.WriteLine($"Verifying If Alarm is Cleared on Well Status Page , Alarm History ");
                                SetClearPointAlarmConfig(glWell, "TubingPressure", (decimal?)UnitsConversion("psia", 30), (decimal?)UnitsConversion("psia", 40), (decimal?)UnitsConversion("psia", 200), (decimal?)UnitsConversion("psia", 250), true);
                                SurveillanceService.IssueCommandForSingleWell(glWell.Id, WellCommand.DemandScan.ToString());
                                rrlwellstatusdata = SurveillanceService.GetWellStatusData(glWell.Id.ToString());
                                espvalue = (GLWellStatusValueDTO)rrlwellstatusdata.Value;
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, false);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(glWell, expalams, start_date, end_date, false);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(glWell, expalams, start_date, end_date, false);
                            }
                            finally
                            {
                                SetValuesInSystemSettings(SettingServiceStringConstants.GL_GROUP_STATUS_EXTRA_UDCS, origadditionaludcstringvalue);
                                ChangeUnitSystemUserSetting("US");
                            }
                            break;
                        }
                    case WellTypeId.NF:
                        {
                            try
                            {
                                CreateAdditionalUDCAnalogPoint(SettingServiceStringConstants.NF_GROUP_STATUS_EXTRA_UDCS, AddUDCName, AddUDC, out origadditionaludcstringvalue);
                                WellDTO nfWell = AddNonRRLWell(facid, WellTypeId.NF);
                                _wellsToRemove.Add(nfWell);
                                //Set Point Alarms Config For NF :
                                Trace.WriteLine($"Verifying the Point Config Alarm for Well Type : {welltype.ToString()}");
                                SetClearPointAlarmConfig(nfWell, "TubingPressure", 50, 100, 200, 250, true);
                                SetPointValue(new FacilityTag(s_site, s_cvsService, facid), "PRTUBXIN", "260", DateTime.Now);
                                // Change or Inactive
                                //Scan for RESET
                                SurveillanceService.IssueCommandForSingleWell(nfWell.Id, WellCommand.DemandScan.ToString());
                                var rrlwellstatusdata = SurveillanceService.GetWellStatusData(nfWell.Id.ToString());
                                NFWellStatusValueDTO espvalue = (NFWellStatusValueDTO)rrlwellstatusdata.Value;
                                IntelligentAlarmsServiceTests ialmtest = new IntelligentAlarmsServiceTests();
                                List<string> expalams = new List<string>();
                                string start_date = (DateTime.Today - TimeSpan.FromDays(30)).ToUniversalTime().ToISO8601();
                                string end_date = DateTime.Today.ToUniversalTime().ToISO8601();
                                expalams.Add("HighHigh Tubing Pressure");
                                Trace.WriteLine($"Verifying If Alarm is Set on Well Status Page , Alarm History ");
                                //Verify If Set
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, true);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(nfWell, expalams, start_date, end_date, true);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(nfWell, expalams, start_date, end_date, true);
                                //Verify If Clear
                                Trace.WriteLine($"Verifying If Alarm is Cleared on Well Status Page , Alarm History ");
                                SetClearPointAlarmConfig(nfWell, "TubingPressure", 50, 100, 200, 250, false);
                                SurveillanceService.IssueCommandForSingleWell(nfWell.Id, WellCommand.DemandScan.ToString());
                                rrlwellstatusdata = SurveillanceService.GetWellStatusData(nfWell.Id.ToString());
                                espvalue = (NFWellStatusValueDTO)rrlwellstatusdata.Value;
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, false);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(nfWell, expalams, start_date, end_date, false);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(nfWell, expalams, start_date, end_date, false);
                            }
                            finally
                            {
                                SetValuesInSystemSettings(SettingServiceStringConstants.NF_GROUP_STATUS_EXTRA_UDCS, origadditionaludcstringvalue);
                            }
                            break;
                        }
                    case WellTypeId.GInj:
                        {
                            try
                            {
                                CreateAdditionalUDCAnalogPoint(SettingServiceStringConstants.GI_GROUP_STATUS_EXTRA_UDCS, AddUDCName, AddUDC, out origadditionaludcstringvalue);
                                WellDTO gInjWell = AddNonRRLWell(facid, WellTypeId.GInj);
                                _wellsToRemove.Add(gInjWell);
                                //Set Point Alarms Config For Gas Injection :
                                Trace.WriteLine($"Verifying the Point Config Alarm for Well Type : {welltype.ToString()}");
                                SetClearPointAlarmConfig(gInjWell, "TubingPressure", 50, 100, 200, 250, true);
                                SetPointValue(new FacilityTag(s_site, s_cvsService, facid), "PRTUBXIN", "40", DateTime.Now);

                                // Change or Inactive
                                //Scan for RESET
                                SurveillanceService.IssueCommandForSingleWell(gInjWell.Id, WellCommand.DemandScan.ToString());
                                var rrlwellstatusdata = SurveillanceService.GetWellStatusData(gInjWell.Id.ToString());
                                GIWellStatusValueDTO espvalue = (GIWellStatusValueDTO)rrlwellstatusdata.Value;
                                IntelligentAlarmsServiceTests ialmtest = new IntelligentAlarmsServiceTests();
                                List<string> expalams = new List<string>();
                                string start_date = (DateTime.Today - TimeSpan.FromDays(30)).ToUniversalTime().ToISO8601();
                                string end_date = DateTime.Today.ToUniversalTime().ToISO8601();
                                expalams.Add("LowLow Tubing Pressure");
                                Trace.WriteLine($"Verifying If Alarm is Set on Well Status Page , Alarm History ");
                                //Verify If Set
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, true);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(gInjWell, expalams, start_date, end_date, true);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(gInjWell, expalams, start_date, end_date, true);
                                //Verify If Clear
                                Trace.WriteLine($"Verifying If Alarm is Cleared on Well Status Page , Alarm History ");
                                //Clear by Deactiviang the flag
                                SetClearPointAlarmConfig(gInjWell, "TubingPressure", 50, 100, 200, 250, false);
                                SurveillanceService.IssueCommandForSingleWell(gInjWell.Id, WellCommand.DemandScan.ToString());
                                rrlwellstatusdata = SurveillanceService.GetWellStatusData(gInjWell.Id.ToString());
                                espvalue = (GIWellStatusValueDTO)rrlwellstatusdata.Value;
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, false);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(gInjWell, expalams, start_date, end_date, false);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(gInjWell, expalams, start_date, end_date, false);
                            }
                            finally
                            {
                                SetValuesInSystemSettings(SettingServiceStringConstants.GI_GROUP_STATUS_EXTRA_UDCS, origadditionaludcstringvalue);
                            }
                            break;
                        }
                    case WellTypeId.WInj:
                        {
                            try
                            {
                                CreateAdditionalUDCAnalogPoint(SettingServiceStringConstants.WI_GROUP_STATUS_EXTRA_UDCS, AddUDCName, AddUDC, out origadditionaludcstringvalue);
                                WellDTO wInjWell = AddNonRRLWell(facid, WellTypeId.WInj);
                                _wellsToRemove.Add(wInjWell);
                                //Set Point Alarms Config For Water Injection :
                                Trace.WriteLine($"Verifying the Point Config Alarm for Well Type : {welltype.ToString()}");
                                SetClearPointAlarmConfig(wInjWell, "TubingPressure", 50, 100, 200, 250, true);
                                SetPointValue(new FacilityTag(s_site, s_cvsService, facid), "PRTUBXIN", "40", DateTime.Now);
                                // Change or Inactive
                                //Scan for RESET
                                SurveillanceService.IssueCommandForSingleWell(wInjWell.Id, WellCommand.DemandScan.ToString());
                                var rrlwellstatusdata = SurveillanceService.GetWellStatusData(wInjWell.Id.ToString());
                                WIWellStatusValueDTO espvalue = (WIWellStatusValueDTO)rrlwellstatusdata.Value;
                                IntelligentAlarmsServiceTests ialmtest = new IntelligentAlarmsServiceTests();
                                List<string> expalams = new List<string>();
                                string start_date = (DateTime.Today - TimeSpan.FromDays(30)).ToUniversalTime().ToISO8601();
                                string end_date = DateTime.Today.ToUniversalTime().ToISO8601();
                                expalams.Add("LowLow Tubing Pressure");
                                Trace.WriteLine($"Verifying If Alarm is Set on Well Status Page , Alarm History ");
                                //Verify If Set
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, true);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(wInjWell, expalams, start_date, end_date, true);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(wInjWell, expalams, start_date, end_date, true);
                                //Verify If Clear
                                SetPointValue(new FacilityTag(s_site, s_cvsService, facid), "PRTUBXIN", "150", DateTime.Now);
                                Trace.WriteLine($"Verifying If Alarm is Cleared on Well Status Page , Alarm History ");
                                SurveillanceService.IssueCommandForSingleWell(wInjWell.Id, WellCommand.DemandScan.ToString());
                                rrlwellstatusdata = SurveillanceService.GetWellStatusData(wInjWell.Id.ToString());
                                espvalue = (WIWellStatusValueDTO)rrlwellstatusdata.Value;

                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, false);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(wInjWell, expalams, start_date, end_date, false);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(wInjWell, expalams, start_date, end_date, false);
                            }
                            finally
                            {
                                SetValuesInSystemSettings(SettingServiceStringConstants.WI_GROUP_STATUS_EXTRA_UDCS, origadditionaludcstringvalue);
                            }
                            break;
                        }
                    case WellTypeId.WGInj:
                        {
                            try
                            {
                                CreateAdditionalUDCAnalogPoint(SettingServiceStringConstants.WAG_GROUP_STATUS_EXTRA_UDCS, AddUDCName, AddUDC, out origadditionaludcstringvalue);
                                WellDTO wagWell = AddNonRRLWell(facid, WellTypeId.WGInj);
                                _wellsToRemove.Add(wagWell);
                                //Set Point Alarms Config For WAG Injection :
                                Trace.WriteLine($"Verifying the Point Config Alarm for Well Type : {welltype.ToString()}");
                                //Default is 180 or could be 0 ( If totaly not initilzed as in ATS)
                                SurveillanceService.IssueCommandForSingleWell(wagWell.Id, WellCommand.DemandScan.ToString());
                                var rrlwellstatusdata = SurveillanceService.GetWellStatusData(wagWell.Id.ToString());
                                WAGWellStatusValueDTO espvalue = (WAGWellStatusValueDTO)rrlwellstatusdata.Value;
                                var originjpressure = espvalue.InjectionPressure;
                                SetClearPointAlarmConfig(wagWell, "InjectionPressure", 50, 100, 150, 250, true);
                                SetClearPointAlarmConfig(wagWell, AddUDCName, 50, 100, 150, 250, true);
                                //   SetPointValue(new FacilityTag(s_site, s_cvsService, facid), "PRESINJ", "240", DateTime.Now);
                                // Change or Inactive
                                //Scan for RESET
                                SurveillanceService.IssueCommandForSingleWell(wagWell.Id, WellCommand.DemandScan.ToString());
                                rrlwellstatusdata = SurveillanceService.GetWellStatusData(wagWell.Id.ToString());
                                espvalue = (WAGWellStatusValueDTO)rrlwellstatusdata.Value;
                                IntelligentAlarmsServiceTests ialmtest = new IntelligentAlarmsServiceTests();
                                List<string> expalams = new List<string>();
                                string start_date = (DateTime.Today - TimeSpan.FromDays(30)).ToUniversalTime().ToISO8601();
                                string end_date = DateTime.Today.ToUniversalTime().ToISO8601();
                                Trace.WriteLine($"For WAG Well Original Injection Pressure was {originjpressure}");
                                if (originjpressure < 50)
                                {
                                    expalams.Add("LowLow Injection Pressure");
                                    expalams.Add($"LowLow {AddUDCName}");
                                }
                                else
                                {
                                    expalams.Add("High Injection Pressure");
                                    expalams.Add($"High {AddUDCName}");
                                }
                                Trace.WriteLine($"Verifying If Alarm is Set on Well Status Page , Alarm History ");
                                //Verify If Set
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, true);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(wagWell, expalams, start_date, end_date, true);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(wagWell, expalams, start_date, end_date, true);
                                //Verify If Clear
                                Trace.WriteLine($"Verifying If Alarm is Cleared on Well Status Page , Alarm History ");
                                SetClearPointAlarmConfig(wagWell, "InjectionPressure", 30, 38, 242, 250, false);
                                SetClearPointAlarmConfig(wagWell, AddUDCName, 50, 100, 200, 230, false);
                                SurveillanceService.IssueCommandForSingleWell(wagWell.Id, WellCommand.DemandScan.ToString());
                                rrlwellstatusdata = SurveillanceService.GetWellStatusData(wagWell.Id.ToString());
                                espvalue = (WAGWellStatusValueDTO)rrlwellstatusdata.Value;
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, false);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(wagWell, expalams, start_date, end_date, false);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(wagWell, expalams, start_date, end_date, false);
                            }
                            finally
                            {
                                SetValuesInSystemSettings(SettingServiceStringConstants.WAG_GROUP_STATUS_EXTRA_UDCS, origadditionaludcstringvalue);
                            }
                            break;
                        }
                    case WellTypeId.PLift:
                        {
                            try
                            {
                                CreateAdditionalUDCAnalogPoint(SettingServiceStringConstants.PL_GROUP_STATUS_EXTRA_UDCS, AddUDCName, AddUDC, out origadditionaludcstringvalue);
                                WellDTO plWell = AddNonRRLWell(facid, WellTypeId.PLift);
                                _wellsToRemove.Add(plWell);
                                //Set Point Alarms Config For Plunger Lift :
                                Trace.WriteLine($"Verifying the Point Config Alarm for Well Type : {welltype.ToString()}");
                                SetClearPointAlarmConfig(plWell, "TubingPressure", 50, 100, 200, 250, true);
                                SetPointValue(new FacilityTag(s_site, s_cvsService, facid), "PRTUBXIN", "240", DateTime.Now);
                                // Change or Inactive
                                //Scan for RESET
                                SurveillanceService.IssueCommandForSingleWell(plWell.Id, WellCommand.DemandScan.ToString());
                                var rrlwellstatusdata = SurveillanceService.GetWellStatusData(plWell.Id.ToString());
                                PGLWellStatusValueDTO espvalue = (PGLWellStatusValueDTO)rrlwellstatusdata.Value;
                                IntelligentAlarmsServiceTests ialmtest = new IntelligentAlarmsServiceTests();
                                List<string> expalams = new List<string>();
                                string start_date = (DateTime.Today - TimeSpan.FromDays(30)).ToUniversalTime().ToISO8601();
                                string end_date = DateTime.Today.ToUniversalTime().ToISO8601();
                                expalams.Add("High Tubing Pressure");
                                Trace.WriteLine($"Verifying If Alarm is Set on Well Status Page , Alarm History ");
                                //Verify If Set
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, true);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(plWell, expalams, start_date, end_date, true);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(plWell, expalams, start_date, end_date, true);
                                //Verify If Clear
                                Trace.WriteLine($"Verifying If Alarm is Cleared on Well Status Page , Alarm History ");
                                SetClearPointAlarmConfig(plWell, "TubingPressure", 50, 100, 300, 350, true);
                                SurveillanceService.IssueCommandForSingleWell(plWell.Id, WellCommand.DemandScan.ToString());
                                rrlwellstatusdata = SurveillanceService.GetWellStatusData(plWell.Id.ToString());
                                espvalue = (PGLWellStatusValueDTO)rrlwellstatusdata.Value;
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, false);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(plWell, expalams, start_date, end_date, false);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(plWell, expalams, start_date, end_date, false);
                            }
                            finally
                            {
                                SetValuesInSystemSettings(SettingServiceStringConstants.PL_GROUP_STATUS_EXTRA_UDCS, origadditionaludcstringvalue);
                            }
                            break;
                        }
                    case WellTypeId.PCP:
                        {
                            try
                            {
                                CreateAdditionalUDCAnalogPoint(SettingServiceStringConstants.PCP_GROUP_STATUS_EXTRA_UDCS, AddUDCName, AddUDC, out origadditionaludcstringvalue);
                                WellDTO pcpWell = AddNonRRLWell(facid, WellTypeId.PCP);
                                _wellsToRemove.Add(pcpWell);
                                //Set Point Alarms Config For PCP :
                                Trace.WriteLine($"Verifying the Point Config Alarm for Well Type : {welltype.ToString()}");
                                SetClearPointAlarmConfig(pcpWell, "TubingPressure", 50, 60, 75, 90, true);
                                // Change or Inactive
                                //Scan for RESET
                                SurveillanceService.IssueCommandForSingleWell(pcpWell.Id, WellCommand.DemandScan.ToString());
                                var rrlwellstatusdata = SurveillanceService.GetWellStatusData(pcpWell.Id.ToString());
                                PCPWellStatusValueDTO espvalue = (PCPWellStatusValueDTO)rrlwellstatusdata.Value;
                                IntelligentAlarmsServiceTests ialmtest = new IntelligentAlarmsServiceTests();
                                List<string> expalams = new List<string>();
                                string start_date = (DateTime.Today - TimeSpan.FromDays(30)).ToUniversalTime().ToISO8601();
                                string end_date = DateTime.Today.ToUniversalTime().ToISO8601();
                                expalams.Add("HighHigh Tubing Pressure");
                                Trace.WriteLine($"Verifying If Alarm is Set on Well Status Page , Alarm History ");
                                //Verify If Set
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, true);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(pcpWell, expalams, start_date, end_date, true);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(pcpWell, expalams, start_date, end_date, true);
                                //Verify If Clear
                                Trace.WriteLine($"Verifying If Alarm is Cleared on Well Status Page , Alarm History ");
                                SetClearPointAlarmConfig(pcpWell, "TubingPressure", 50, 60, 175, 190, true);
                                SurveillanceService.IssueCommandForSingleWell(pcpWell.Id, WellCommand.DemandScan.ToString());
                                rrlwellstatusdata = SurveillanceService.GetWellStatusData(pcpWell.Id.ToString());
                                espvalue = (PCPWellStatusValueDTO)rrlwellstatusdata.Value;
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, false);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(pcpWell, expalams, start_date, end_date, false);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(pcpWell, expalams, start_date, end_date, false);
                            }
                            finally
                            {
                                SetValuesInSystemSettings(SettingServiceStringConstants.PCP_GROUP_STATUS_EXTRA_UDCS, origadditionaludcstringvalue);
                            }
                            break;
                        }
                    case WellTypeId.OT:
                        {
                            try
                            {
                                CreateAdditionalUDCAnalogPoint(SettingServiceStringConstants.OT_GROUP_STATUS_EXTRA_UDCS, AddUDCName, AddUDC, out origadditionaludcstringvalue);
                                WellDTO otWell = AddNonRRLWell(facid, WellTypeId.OT);
                                _wellsToRemove.Add(otWell);
                                //Set Point Alarms Config For OT :
                                Trace.WriteLine($"Verifying the Point Config Alarm for Well Type : {welltype.ToString()}");
                                // No default UDCs are avaialble for OT Well Type we have to use Ational UDC onnly
                                SetClearPointAlarmConfig(otWell, AddUDCName, 50, 100, 200, 250, true);
                                SetPointValue(new FacilityTag(s_site, s_cvsService, facid), "PRTUBXIN", "40", DateTime.Now);
                                // Change or Inactive
                                //Scan for RESET
                                SurveillanceService.IssueCommandForSingleWell(otWell.Id, WellCommand.DemandScan.ToString());
                                var rrlwellstatusdata = SurveillanceService.GetWellStatusData(otWell.Id.ToString());
                                OTWellStatusValueDTO espvalue = (OTWellStatusValueDTO)rrlwellstatusdata.Value;
                                IntelligentAlarmsServiceTests ialmtest = new IntelligentAlarmsServiceTests();
                                List<string> expalams = new List<string>();
                                string start_date = (DateTime.Today - TimeSpan.FromDays(30)).ToUniversalTime().ToISO8601();
                                string end_date = DateTime.Today.ToUniversalTime().ToISO8601();
                                expalams.Add($"LowLow {AddUDCName}");
                                Trace.WriteLine($"Verifying If Alarm is Set on Well Status Page , Alarm History ");
                                //Verify If Set
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, true);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(otWell, expalams, start_date, end_date, true);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(otWell, expalams, start_date, end_date, true);
                                //Verify If Clear
                                Trace.WriteLine($"Verifying If Alarm is Cleared on Well Status Page , Alarm History ");
                                SetClearPointAlarmConfig(otWell, AddUDCName, 50, 100, 200, 250, false);
                                SurveillanceService.IssueCommandForSingleWell(otWell.Id, WellCommand.DemandScan.ToString());
                                rrlwellstatusdata = SurveillanceService.GetWellStatusData(otWell.Id.ToString());
                                espvalue = (OTWellStatusValueDTO)rrlwellstatusdata.Value;
                                ialmtest.VerifyForeSiteAlarmsOnWellStatusPage(espvalue.IntelligentAlarmMessage, expalams, false);
                                ialmtest.VerifyAlarmsOnAllAlarmsHistoryPage(otWell, expalams, start_date, end_date, false);
                                ialmtest.VerifyAlarmsOnForesiteAlarmsHistoryPage(otWell, expalams, start_date, end_date, false);
                            }
                            finally
                            {
                                SetValuesInSystemSettings(SettingServiceStringConstants.OT_GROUP_STATUS_EXTRA_UDCS, origadditionaludcstringvalue);
                            }
                            break;
                        }
                }
            }
            finally
            {
                //clear System Settings for Additional UDC

            }
        }

        private void SetClearPointAlarmConfig(WellDTO well, string pointname, decimal? lolo, decimal? lo, decimal? hi, decimal? hihi, bool active)
        {
            var pointconfig = SurveillanceService.GetPointsAlarmConfig(well.Id.ToString());
            if (pointname.Contains("UDCAddedByAPITest"))
            {
                pointconfig.FirstOrDefault(x => x.Name == pointname).LoLoLimit = lolo;
                pointconfig.FirstOrDefault(x => x.Name == pointname).LoLimit = lo;
                pointconfig.FirstOrDefault(x => x.Name == pointname).HiLimit = hi;
                pointconfig.FirstOrDefault(x => x.Name == pointname).HiHiLimit = hihi;
                pointconfig.FirstOrDefault(x => x.Name == pointname).IsActive = active;
                Assert.IsTrue(SurveillanceService.SavePointsAlarmConfig(well.Id.ToString(), pointconfig));

            }
            else
            {
                pointconfig.FirstOrDefault(x => x.Quantity.ToString() == pointname).LoLoLimit = lolo;
                pointconfig.FirstOrDefault(x => x.Quantity.ToString() == pointname).LoLimit = lo;
                pointconfig.FirstOrDefault(x => x.Quantity.ToString() == pointname).HiLimit = hi;
                pointconfig.FirstOrDefault(x => x.Quantity.ToString() == pointname).HiHiLimit = hihi;
                pointconfig.FirstOrDefault(x => x.Quantity.ToString() == pointname).IsActive = active;
                Assert.IsTrue(SurveillanceService.SavePointsAlarmConfig(well.Id.ToString(), pointconfig));
            }
        }

        private void CreateAdditionalUDCAnalogPoint(string SettingName, string name, string additionalUDC, out string origvalue, string unitkey = null, UnitCategory unitcat = UnitCategory.None)
        {
            SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(SettingName);
            origvalue = settingValue.StringValue;
            ConfigurableUDCInfo infos = new ConfigurableUDCInfo();
            infos.Parse(settingValue.StringValue);
            try
            {
                infos.Info.Add(infos.Info.Count() + 1, new ConfigurableUDC(name, additionalUDC, unitkey, unitcat, CygNetPointType.Analog, false));
                settingValue.StringValue = infos.GetSettingValue();
                SettingService.SaveSystemSetting(settingValue);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Expcetion " + ex.ToString());
            }
        }
        public void UpdateCygNetUISValues(WellDTO well, string udc, string value)
        {
            Authenticate();
            ESPWellStatusValueDTO wellstatus = SurveillanceServiceClient.GetWellStatusDataWithUnitSystem(well.Id.ToString(),
                            SettingServiceStringConstants.UNIT_SYSTEM_NAME_US).Value as ESPWellStatusValueDTO;
            string oldvalue = String.Empty;
            switch (udc)
            {
                case "PIP":
                    {
                        oldvalue = wellstatus.PumpIntakePressure.ToString();
                        udcoriginalvalue.Add(udc, oldvalue);
                        break;
                    }
                case "PDP":
                    {
                        oldvalue = wellstatus.PumpDischargePressure.ToString();
                        udcoriginalvalue.Add(udc, oldvalue);
                        break;
                    }
                case "AMPMOTOR":
                    {
                        oldvalue = wellstatus.MotorAmps.ToString();
                        udcoriginalvalue.Add(udc, oldvalue);
                        break;
                    }
                //Add UDCS we want to store and revert back toits old value;
                default:
                    {
                        break;
                    }
            }

            FacilityTag facilityTag = new FacilityTag(s_site, s_cvsService, well.FacilityId);
            if (oldvalue.Equals(value))
            {
                double dblvalue = double.Parse(value);
                dblvalue++;
                SetPointValue(facilityTag, udc, dblvalue.ToString(), DateTime.Now);
                Trace.WriteLine($"Set a New Value in CygNet UIS :{dblvalue.ToString()} ");
            }
            else
            {
                SetPointValue(facilityTag, udc, value, DateTime.Now);
                Trace.WriteLine($"Set a New Value in CygNet UIS : {value.ToString()} ");
            }

        }
        public void RevertCygNetUISValue(WellDTO well)
        {
            Authenticate();
            //List<string> keystorem = new List<string>();
            FacilityTag facilityTag = new FacilityTag(s_site, s_cvsService, well.FacilityId);
            foreach (var skey in udcoriginalvalue.Keys)
            {
                SetPointValue(facilityTag, skey, udcoriginalvalue[skey], DateTime.Now);
                Trace.WriteLine($"Reverted Value in CygNet UIS for udc : {skey} : {udcoriginalvalue[skey].ToString()} ");
                //keystorem.Add(skey);
            }
            //foreach (string indkey in keystorem)
            //{
            //    udcoriginalvalue.Remove(indkey);
            // }
            udcoriginalvalue.Clear();
        }

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void VerifyDailyAverageForWellStatusOptionalUDCS()
        {
            //FRWM-6405: Additional UDC doesn't generate daily average : Well Status Support' ==> API testing subtask :FRWM-6656 :
            //Create Additional UDC on All Well Types and Check for Daily Averge Data in Well Status Screen
            VerificationOfAdditionalUDCDailyAverageForWellStatus("RPOC_", "RRLUDCAddedByAPITest", "QTLIQT", "STB/d", UnitCategory.FlowRate, WellTypeId.RRL);
            VerificationOfAdditionalUDCDailyAverageForWellStatus("ESPWELL_", "ESPUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.None, WellTypeId.ESP);
            VerificationOfAdditionalUDCDailyAverageForWellStatus("NFWWELL_", "NFWUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.Pressure, WellTypeId.NF);
            VerificationOfAdditionalUDCDailyAverageForWellStatus("GLWELL_", "GLUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.Pressure, WellTypeId.GLift);
            VerificationOfAdditionalUDCDailyAverageForWellStatus("GASINJWELL_", "GIUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.Pressure, WellTypeId.GInj);
            VerificationOfAdditionalUDCDailyAverageForWellStatus("WATERINJWELL_", "WIUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.Pressure, WellTypeId.WInj);
            VerificationOfAdditionalUDCDailyAverageForWellStatus(s_prefixForPLFacility, "PLUDCAddedByAPITest", "PRTUBXIN", null, UnitCategory.Pressure, WellTypeId.PLift);
            VerificationOfAdditionalUDCDailyAverageForWellStatus("WFTA1K_", "PCPUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.Pressure, WellTypeId.PCP);
            VerificationOfAdditionalUDCDailyAverageForWellStatus("WFTA1K_", "PCPUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.Pressure, WellTypeId.OT);
            VerificationOfAdditionalUDCDailyAverageForWellStatus("IWC_", "WAGPUDCAddedByAPITest", "PRTUBXIN", "psia", UnitCategory.Pressure, WellTypeId.WGInj);
        }


        /// <summary>
        /// Test was Added as part of Additional API coverge FRI-4282 for ISurveillanceService
        /// </summary>

        [TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
        public void VerifyWAGCycleDataCycleWellTrends()
        {
            WellDTO waginjwel = AddNonRRLWell(GetFacilityId("IWC_", 1), WellTypeId.WGInj);
            _wellsToRemove.Add(waginjwel);
            WellTargetRateDTO wagwellwatertargerate = new WellTargetRateDTO();
            wagwellwatertargerate.InjWaterTarget = 2000;
            wagwellwatertargerate.InjWaterTargetThresholdInPercentage = 10;
            wagwellwatertargerate.ProductionType = ProductionType.Water;
            wagwellwatertargerate.Comments = "Water Rate";
            wagwellwatertargerate.StartDate = DateTime.Today.AddDays(-31).ToUniversalTime();
            wagwellwatertargerate.EndDate = DateTime.Today.AddDays(1).ToUniversalTime();
            wagwellwatertargerate.WellId = waginjwel.Id;
            WellService.AddWellTargetRate(wagwellwatertargerate);

            //Add gas rate target also
            WellTargetRateDTO wagwellgastargerate = new WellTargetRateDTO();
            wagwellgastargerate.InjGasTarget = 2000;
            wagwellgastargerate.InjGasTargetThresholdInPercentage = 10;
            wagwellgastargerate.ProductionType = ProductionType.Gas;
            wagwellgastargerate.Comments = "Gas Rate";
            wagwellgastargerate.StartDate = DateTime.Today.AddDays(-31).ToUniversalTime();
            wagwellgastargerate.EndDate = DateTime.Today.AddDays(1).ToUniversalTime();
            wagwellgastargerate.WellId = waginjwel.Id;
            WellService.AddWellTargetRate(wagwellgastargerate);
            SurveillanceService.UpdateLatestWAGCycleData(waginjwel);
            //This API call is made from Well Status WAG Injection Volume chart 
            WAGCycleChartDataDTO[] wagcycledatadtoarr = SurveillanceService.GetWAGCycleChartData(waginjwel.Id.ToString(), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-31)), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime()));

            Assert.IsNotNull(wagcycledatadtoarr);
            // Verify The WAG Injection Volume chart 
            foreach (WAGCycleChartDataDTO WAGCycleChartDataDTO in wagcycledatadtoarr)

            {
                Assert.IsNotNull(WAGCycleChartDataDTO.Points.Units, "WAG Units were null");
                if (WAGCycleChartDataDTO.Points.Values.Length > 0)
                {
                    WAGCycleChartDataPointDTO[] wagcharts = WAGCycleChartDataDTO.Points.Values;
                    foreach (WAGCycleChartDataPointDTO wagchart in wagcharts)
                    {
                        Assert.IsNotNull(wagchart);
                        Assert.IsNotNull(wagchart.Timestamp, "WAG Chart Time stamp was NULL");
                        Assert.IsNotNull(wagchart.Value, "WAG Chart Value was NULL");
                        Assert.IsNotNull(wagchart.Target.Value, "WAG target rate volume was NULL");
                        Trace.WriteLine($"Time Stamp { wagchart.Timestamp}  and Value {wagchart.Value} and Target volune {wagchart.Target.Value}");
                        Assert.IsTrue(wagchart.Value > 0, "Volume value was non zero ");
                        Assert.IsTrue(wagchart.Target.Value > 0, "Volume Target value was non zero ");
                    }
                }
            }

            // Get Surveillance Trends
            //Get  Cycle Trends 
            CycleQuantity[] wagcycles = SurveillanceService.GetCycleQuantities(WellTypeId.WGInj.ToString());
            StringBuilder st = new StringBuilder();

            foreach (var qaun in wagcycles)
            {
                st.Append(qaun.ToString() + ";");
            }
            st.Remove(st.ToString().Length - 1, 1);
            Trace.WriteLine(st.ToString());
            Assert.IsTrue(wagcycles.Length == 17, "Unexpected WAG Cyles quantities");
            string[] cycletrendlist = new string[]
            { ((int)CycleQuantity.CycleNumber).ToString(),
             ((int)CycleQuantity.GasSuggestedRateSP).ToString(),
             ((int)CycleQuantity.GasVolCycleToDate).ToString(),
             ((int)CycleQuantity.GasVolTargetToDate).ToString(),
             ((int)CycleQuantity.GasVolumeOverUnder).ToString(),
             ((int)CycleQuantity.GasVolumeOverUnderFromYesterday).ToString(),
             ((int)CycleQuantity.GasVolumeTarget).ToString(),
             ((int)CycleQuantity.GasVolumeTargetRemaining).ToString(),
             ((int)CycleQuantity.WaterSuggestedRateSP).ToString(),
             ((int)CycleQuantity.WaterVolCycleToDate).ToString(),
             ((int)CycleQuantity.WaterVolTargetToDate).ToString(),
             ((int)CycleQuantity.WaterVolumeOverUnder).ToString(),
             ((int)CycleQuantity.WaterVolumeOverUnderFromYesterday).ToString(),
             ((int)CycleQuantity.WaterVolumeTarget).ToString(),
             ((int)CycleQuantity.WaterVolumeTargetRemaining).ToString(),
             ((int)CycleQuantity.PercentCycleVolumeOverUnder).ToString(),
             ((int)CycleQuantity.PercentOverUnderFromYesterday).ToString()
            };
            CygNetTrendDTO[] WAgCycleTrends = SurveillanceService.GetCycleTrendsDataByDateRange(cycletrendlist, waginjwel.Id.ToString(), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-31)), DTOExtensions.ToISO8601(DateTime.Today.AddDays(1).ToUniversalTime()));
            Assert.IsNotNull(WAgCycleTrends, "WAG Cycles Trend was NULL");
            foreach (CygNetTrendDTO wacgcyledto in WAgCycleTrends)
            {
                Assert.AreEqual("Success", wacgcyledto.ErrorMessage, $"Error Message was not suceess for{wacgcyledto.PointUDC.ToString()} ");
                Trace.WriteLine($"Verified Point UDC for WAG Cycle : {wacgcyledto.PointUDC.ToString()}");

            }

            Trace.WriteLine("test completed");

        }

        private static void LiftInitialize()
        {
            List<string> LiftTypesList = new List<string> { "GL", "ESP", "NFW", "GASINJ", "WATERINJ", "RRL", "IWC", "PGL" };

            foreach (var lift in LiftTypesList)
            {
                string liftFilePath = GetLiftTypeBaseFile(lift);

                if (string.IsNullOrWhiteSpace(liftFilePath))
                {
                    Console.WriteLine($"File path not initialized for '{lift}' base file.");
                    continue;
                }

                //Console.WriteLine($"Initialize started for '{lift}' base file. ");
                if (File.Exists(liftFilePath))
                {
                    string[] lines = File.ReadAllLines(liftFilePath);

                    var headers = lines.Take(1).FirstOrDefault();
                    string[] columns = headers.Split(',');

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (i == 0)
                            continue;

                        var line = lines[i];
                        string[] Fields = line.Split(',');

                        AddSinglePointToLiftPointList(InitializeSinglePoint(columns, Fields), lift);
                    }
                    Console.WriteLine($"Initialize: Point List initialized from '{lift}' base file.");
                }
                else
                {
                    Console.WriteLine($"File path '{liftFilePath}' not found for '{lift}' base file.");
                }
            }
        }

        private static string GetLiftTypeBaseFile(string lift)
        {
            List<SinglePoint> GasLiftPoints = new List<SinglePoint>();
            List<SinglePoint> NFWLiftPoints = new List<SinglePoint>();
            List<SinglePoint> GasInjLiftPoints = new List<SinglePoint>();
            List<SinglePoint> WaterInjLiftPoints = new List<SinglePoint>();
            List<SinglePoint> ESPLiftPoints = new List<SinglePoint>();
            List<SinglePoint> RPOCPoints = new List<SinglePoint>();
            List<SinglePoint> SAMPoints = new List<SinglePoint>();
            List<SinglePoint> IWCPoints = new List<SinglePoint>();
            List<SinglePoint> PGLPoints = new List<SinglePoint>();

            List<SinglePoint> pointsList = new List<SinglePoint>();
            string liftFile = null;
            switch (lift)
            {
                case "GL":
                    liftFile = GetFileFromAssemblyStream("GasLiftPoints.csv");
                    break;
                case "ESP":
                    liftFile = GetFileFromAssemblyStream("ESPLiftPoints.csv");
                    break;
                case "NFW":
                    liftFile = GetFileFromAssemblyStream("NFWLiftPoints.csv");
                    break;
                case "GASINJ":
                    liftFile = GetFileFromAssemblyStream("GasInjLiftPoints.csv");
                    break;
                case "WATERINJ":
                    liftFile = GetFileFromAssemblyStream("WaterInjLiftPoints.csv");
                    break;
                case "RRL":
                    liftFile = GetFileFromAssemblyStream("RPOCPoints.csv");
                    break;
                case "IWC":
                    liftFile = GetFileFromAssemblyStream("GasLiftPoints.csv");
                    break;
                case "PGL":
                    liftFile = GetFileFromAssemblyStream("PGLLiftPoints.csv");
                    break;
                default:
                    break;
            }
            return liftFile;
        }
        private static SinglePoint InitializeSinglePoint(string[] columns, string[] fields)
        {
            SinglePoint sp = new SinglePoint();
            int i = 0;
            foreach (var col in columns)
            {
                string val = fields[i++];
                double result = 0.0;
                int intVal = 0;
                string colName = col.Contains("Value") ? "Value" : col;
                switch (colName)
                {
                    case "UDC":
                        sp.UDC = val;
                        break;
                    case "Description":
                        sp.Description = val;
                        break;
                    case "PrimaryUnits":
                        sp.PrimaryUnits = val;
                        break;
                    case "PointType":
                        sp.PointType = val;
                        break;
                    case "AlternateUnits":
                        sp.AlternateUnits = val;
                        break;
                    case "Base":
                        sp.Base = double.TryParse(val, out result) ? result : 0;
                        break;
                    case "InitialVal":
                        sp.InitialValue = val != null ? val : string.Empty;
                        break;
                    case "Minimum":
                        sp.Mininum = double.TryParse(val, out result) ? result : 0;
                        break;
                    case "Maximum":
                        sp.Maximum = double.TryParse(val, out result) ? result : 0;
                        break;
                    case "LowerBound":
                        sp.LowerBound = double.TryParse(val, out result) ? result : 0;
                        break;
                    case "UpperBound":
                        sp.UpperBound = double.TryParse(val, out result) ? result : 0;
                        break;
                    case "DataType":
                        sp.DataType = GetValueDataType(val);
                        break;
                    case "Calculated":
                        int calc = 0;
                        sp.Calculated = int.TryParse(val, out calc) ? calc : 0;
                        break;
                    case "IsAlarmPoint":
                        sp.IsAlarmPoint = int.TryParse(val, out intVal) ? intVal : 0;
                        break;
                    case "LowAlarmSP":
                        sp.LowAlarmSP = double.TryParse(val, out result) ? result : 0;
                        break;
                    case "HighAlarmSP":
                        sp.HighAlarmSP = double.TryParse(val, out result) ? result : 0;
                        break;
                    case "Value":
                        sp.DataSet.Add(val);
                        break;
                }
            }
            return sp;
        }
        private static void AddSinglePointToLiftPointList(SinglePoint sp, string lift)
        {
            switch (lift)
            {
                case "GL":
                    GasLiftPoints.Add(sp);
                    break;
                case "ESP":
                    ESPLiftPoints.Add(sp);
                    break;
                case "NFW":
                    NFWLiftPoints.Add(sp);
                    break;
                case "GASINJ":
                    GasInjLiftPoints.Add(sp);
                    break;
                case "WATERINJ":
                    WaterInjLiftPoints.Add(sp);
                    break;
                case "RRL":
                    RPOCPoints.Add(sp);
                    break;
                case "IWC":
                    IWCPoints.Add(sp);
                    break;
                case "PGL":
                    PGLPoints.Add(sp);
                    break;
            }
        }
        private static List<SinglePoint> GetValidPointListForFacility(FacilityTag fac, string liftType, bool waitForPointExist = false)
        {
            // Get the point records
            var pointsByLift = GetLiftSpecificPointsList(liftType);

            if (pointsByLift == null || pointsByLift.Count < 1)
            {
                System.Diagnostics.Trace.WriteLine($"GetValidPointListForFacility: No points for the {liftType} found.");
                return null;
            }

            List<string> facUDCs = new List<string>();
            foreach (var point in pointsByLift)
            {
                facUDCs.Add(fac.FacilityId + "." + point.UDC);
            }

            object input = facUDCs.Select(t => t as object).ToArray();
            object pointIDsList = null;

            try
            {
                System.Diagnostics.Trace.WriteLine($"GetValidPointListForFacility: Going to create CVS Client for [{s_domain}]{s_site}.{s_cvsService}.");
                var cvsClient = CreateCvsClient();
                cvsClient.ResolvePoints(ref input, out pointIDsList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"GetValidPointListForFacility: CvsClient.ResolvePoints failed: {ex.Message}.");
                return null;
            }

            List<string> tagIDs = new List<string>();
            if (pointIDsList.GetType().IsArray)
            {
                tagIDs = ((object[])pointIDsList).Select(obj => Convert.ToString(obj)).ToList();
            }

            int i = 0;
            List<SinglePoint> validPointListByFac = new List<SinglePoint>();
            foreach (var item in pointsByLift)
            {
                if (!string.IsNullOrWhiteSpace(tagIDs[i]))
                {
                    item.PointId = tagIDs[i];// delete this line after verifying
                    validPointListByFac.Add(item);
                }
                else
                {
                    Console.WriteLine($"GetValidPointListForFacility: PointTag was not found for {item.UDC} and facility {fac.FacilityId}");
                }
                ++i;
            }

            if (waitForPointExist && (validPointListByFac.Count() != tagIDs.Count()))
            {
                Console.WriteLine($"All point records not found. Wait and refetech the point records.");
                Thread.Sleep(200);
                GetValidPointListForFacility(fac, liftType, waitForPointExist);
            }
            else if (!waitForPointExist && (validPointListByFac.Count() != tagIDs.Count()))
            {
                Console.WriteLine($"Not waiting for the points to exist on facility '{fac.FacilityId}'. Returning only existing point list.");
            }

            return validPointListByFac;
        }
        private static uint GetPointScheme()
        {
            uint defPointScheme = 0;
            CygNet.API.Core.PointMetadata pntMetadata = new CygNet.API.Core.PointMetadata();
            if (!pntMetadata.IsLoaded)
            {
                IEnumerable<string> errors;
                IEnumerable<string> warnings;
                pntMetadata.LoadMetadataFromService(GetDomainSiteService2("ARS"), out errors, out warnings);
            }
            var psList = pntMetadata.GetPointSchemes();

            if (psList.Count == 1)
            {
                var ps = psList.FirstOrDefault();
                return ps.Id;
            }
            else if (psList.Count > 1)
            {
                // CygNet system supports multiple CvsMetadata. Search for the default one first
                var defPs = psList.Where(ps => ps.Id == 0).FirstOrDefault();
                if (defPs != null)
                    return defPs.Id;
                else
                {
                    var ps = psList.FirstOrDefault();
                    return ps.Id;
                }
            }

            return defPointScheme;
        }

        private static ICollection<FacilityTag> GetFacilitiesList(string lift, CygNet.COMAPI.Interfaces.IUisClient uisClient, string s_facilityType, string facPrefix)
        {
            var fac = new CygNet.Data.Core.DomainSiteService(uisClient.GetAssociatedFAC().GetDomainSiteService());
            var facClient = new CygNet.API.Facilities.Client(fac);

            string query = string.Format($"facility_site = '{s_site}' AND facility_service = '{s_cvsService}' AND facility_type = '{s_facilityType}'");
            System.Threading.CancellationToken ct;
            ICollection<FacilityTag> facilityTags = facClient.GetFacilityTagList(query, ct);
            List<string> facList = new List<string>();
            facList.Add(GetFacilityId(facPrefix, 1));
            List<FacilityTag> filteredFacList = new List<FacilityTag>();
            foreach (var facility in facList)
            {
                var liftFac = facilityTags.Where(f => f.FacilityId.Equals(facility)).FirstOrDefault();
                if (liftFac != null)
                    filteredFacList.Add(liftFac);
            }

            return filteredFacList;
        }

        private static List<SinglePoint> GetLiftSpecificPointsList(string liftType)
        {
            List<SinglePoint> pointsList = new List<SinglePoint>();
            switch (liftType)
            {
                case "GL":
                    pointsList = GasLiftPoints;
                    break;
                case "RRL":
                    pointsList = RPOCPoints;
                    break;
                case "ESP":
                    pointsList = ESPLiftPoints;
                    break;
                case "NFW":
                    pointsList = NFWLiftPoints;
                    break;
                case "GASINJ":
                    pointsList = GasInjLiftPoints;
                    break;
                case "WATERINJ":
                    pointsList = WaterInjLiftPoints;
                    break;
                case "IWC":
                    pointsList = IWCPoints;
                    break;
                case "PGL":
                    pointsList = PGLPoints;
                    break;
            }

            return pointsList;
        }
        private static TypeCode GetValueDataType(string type)
        {
            TypeCode dt = TypeCode.Empty;
            switch (type)
            {
                case "Int":
                    dt = TypeCode.Int32;
                    break;
                case "Double":
                    dt = TypeCode.Double;
                    break;
                case "String":
                    dt = TypeCode.String;
                    break;
                case "Bool":
                    dt = TypeCode.Boolean;
                    break;
            }

            return dt;
        }
    }

    public class SinglePoint
    {
        public SinglePoint()
        {
            DataSet = new List<object>();
        }

        public SinglePoint DeepCopy()
        {
            SinglePoint other = (SinglePoint)this.MemberwiseClone();
            other.DataSet = this.DataSet.ToList();
            return other;
        }

        public string UDC { get; set; }
        public string Description { get; set; }
        public string PointType { get; set; }
        public TypeCode DataType { get; set; }
        public string PrimaryUnits { get; set; }
        public string AlternateUnits { get; set; }
        public string InitialValue { get; set; }
        public double Base { get; set; }
        public double Mininum { get; set; }
        public double Maximum { get; set; }
        public double LowerBound { get; set; }
        public double UpperBound { get; set; }
        public string PointId { get; set; }
        public int Calculated { get; set; }
        public int? IsAlarmPoint { get; set; }
        public double? LowAlarmSP { get; set; }
        public double? HighAlarmSP { get; set; }
        public List<object> DataSet { get; set; }
    }


}


// This script to be added based on new story replacing FRWM-3582
/*[TestCategory(TestCategories.SurveillanceServiceTests), TestMethod]
public void GLWellRuntimeOperatingLimit()
{
    string facilityId;
    facilityId = GetFacilityId("GLWELL_", 1);

    //pick up the right model file
    string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
    Tuple<string, WellTypeId, ModelFileOptionDTO> model = Tuple.Create("GL-01-Base.wflx", WellTypeId.GLift, new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.ReservoirPressure, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), (long)OptionalUpdates.UpdateWCT_WGR, ((long)OptionalUpdates.CalculateChokeD_Factor) } });

    var modelFileName = model.Item1;
    var wellType = model.Item2;
    var options = model.Item3;

    Trace.WriteLine("Testing model: " + modelFileName);

    //Create a new well
    WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO { Name = DefaultWellName + wellType, FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today.AddYears(-3), WellType = wellType }) });
    var allWells = WellService.GetAllWells().ToList();
    WellDTO well = allWells.FirstOrDefault(w => w.Name.Equals(DefaultWellName + wellType.ToString()));
    Assert.IsNotNull(well);
    _wellsToRemove.Add(well);
    var myassembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
    Assert.IsNotNull(myassembly);

    ModelFileBase64DTO modelFile = new ModelFileBase64DTO();

    options.Comment = "CASETest Upload " + model.Item1;
    modelFile.Options = options;
    modelFile.ApplicableDate = DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10);
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

    AddWellSetting(well.Id, "Min Run Time Operating Limit", 100);

    var testDataDTO = new WellTestDTO
    {
        WellId = well.Id,
        AverageCasingPressure = 1575,
        AverageTubingPressure = 1774,
        AverageTubingTemperature = 65,
        Gas = 2000m,
        GasGravity = 0.6722m,
        GasInjectionRate = 1563,
        producedGOR = 1160,
        Oil = 1749,
        OilGravity = 46.2415m,
        SPTCode = 2,
        SPTCodeDescription = "RepresentativeTest",
        TestDuration = 3,
        Water = 1960,
        WaterGravity = (decimal)1.0239,
        GaugePressure = (decimal)1634,
        ReservoirPressure = 5250.0m,
    };

    AddWellSetting(well.Id, "Min Reservoir Pressure Acceptance Limit", 15);
    AddWellSetting(well.Id, "Max Reservoir Pressure Acceptance Limit", 10000);

    testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(5));
    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
    WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));

    bool commandResult = SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
    Assert.IsTrue(commandResult, "Failed to send command to well of type GLift ");
    //Calling Well Status API
    SurveillanceServiceClient.GetWellStatusData(well.Id.ToString());
    AlarmDTO[] alarms = AlarmService.GetAlarmsByWellId(well.Id.ToString());
    Assert.IsNotNull(alarms);
    Assert.IsTrue(alarms[0].AlarmValue.Contains("Low Run Time"));

    string Test = null;
    foreach (var alarmName in alarms)
    {
        if (alarmName.AlarmValue.Contains("Low Run Time"))
        {
            Trace.WriteLine("As Expected, Low Run Time Alarm Detected");
            Test = "Passed";
        }
    }
    if (Test == null)
    {
        Assert.Fail("Low Run Time alarm not detected");
    }
}*/
