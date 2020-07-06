using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using Weatherford.POP.Interfaces;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;


namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class HeatMapServiceTests : APIClientTestBase
    {
        protected IHeatMapService _heatMapService;
        protected List<WellDTO> _addedWells;

        protected IHeatMapService HeatMapService
        {
            get { return _heatMapService ?? (_heatMapService = _serviceFactory.GetService<IHeatMapService>()); }
        }

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            _addedWells = new List<WellDTO>();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            base.Cleanup();
        }

        public void AddWell(string facilityIdBase, WellTypeId wellType)
        {
            string facilityId = s_isRunningInATS ? facilityIdBase + "00001" : facilityIdBase + "0001";
            WellDTO well = SetDefaultFluidType(new WellDTO()
            {
                Name = DefaultWellName,
                FacilityId = facilityId,
                DataConnection = GetDefaultCygNetDataConnection(),
                CommissionDate = DateTime.Today.AddYears(-2),
                AssemblyAPI = "123456789012",
                SubAssemblyAPI = "1234567890",
                WellType = wellType,
                SurfaceLatitude = 29.686619m,
                SurfaceLongitude = -95.399334m
            });
            WellConfigDTO wellConfigDTO = new WellConfigDTO();
            wellConfigDTO.Well = well;
            wellConfigDTO.ModelConfig = ReturnBlankModel();
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
            WellDTO[] wells = WellService.GetAllWells();
            wells = WellService.GetAllWells();
            WellDTO getwell = wells.FirstOrDefault(w => w.Name == well.Name);
            _wellsToRemove.Add(getwell);
        }

        [TestCategory(TestCategories.HeatMapServiceTests), TestMethod]
        public void GetWellsForHeatMap()
        {
            AddWell("RPOC_", WellTypeId.RRL);
            WellDTO[] allwells = WellService.GetAllWells();
            WellFilterDTO wellbyFilter = new WellFilterDTO();
            //No Filter selected
            WellDTO[] wells = WellService.GetWellsByFilter(wellbyFilter);
            Assert.AreEqual(allwells.Count(), wells.Count());
            //Get Well details to display on Heat Map
            foreach (WellDTO well in wells)
            {
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                var wellstatusWithUnits = SurveillanceServiceClient.GetWellStatusData<RRLWellStatusUnitDTO, RRLWellStatusValueDTO>(well.Id.ToString());
                RRLWellStatusValueDTO wellstatus = wellstatusWithUnits.Value;
                HeatMapDTO wellDetails = HeatMapService.GetWellDetails(well.Id.ToString());
                Assert.AreEqual(wellstatus.WellName, wellDetails.WellName);
                Assert.AreEqual(wellstatus.POCMode, wellDetails.Status);
            }
            HeatMapWellsDTO runningOrdownWellssForHeatMap = HeatMapService.GetWellsForHeatMap(wellbyFilter, "1", "5");
            HeatMapWellsDTO downOrAlarmingWellsForHeatMap = HeatMapService.GetWellsForHeatMap(wellbyFilter, "1", "6");
            Assert.IsNotNull(runningOrdownWellssForHeatMap);
            Assert.IsNotNull(downOrAlarmingWellsForHeatMap);
            if (downOrAlarmingWellsForHeatMap.WellsForHeatMap.Count == 1)
            {
                Assert.AreEqual(1, downOrAlarmingWellsForHeatMap.WellsForHeatMap.Count);
                Assert.AreEqual(allwells.FirstOrDefault().SurfaceLatitude, downOrAlarmingWellsForHeatMap.WellsForHeatMap.FirstOrDefault().SurfaceLatitude);
                Assert.AreEqual(allwells.FirstOrDefault().SurfaceLongitude, downOrAlarmingWellsForHeatMap.WellsForHeatMap.FirstOrDefault().SurfaceLongitude);
            }
            else if (runningOrdownWellssForHeatMap.WellsForHeatMap.Count == 1)
            {
                Assert.AreEqual(1, runningOrdownWellssForHeatMap.WellsForHeatMap.Count);
                Assert.AreEqual(allwells.FirstOrDefault().SurfaceLatitude, runningOrdownWellssForHeatMap.WellsForHeatMap.FirstOrDefault().SurfaceLatitude);
                Assert.AreEqual(allwells.FirstOrDefault().SurfaceLongitude, runningOrdownWellssForHeatMap.WellsForHeatMap.FirstOrDefault().SurfaceLongitude);
            }
        }

        [TestCategory(TestCategories.HeatMapServiceTests), TestMethod]
        public void GetWellsForHeatMap_ESP()
        {
            AddWell("ESPWELL_", WellTypeId.ESP);
            WellDTO[] allwells = WellService.GetAllWells();
            WellFilterDTO wellbyFilter = new WellFilterDTO();
            //No Filter selected
            WellDTO[] wells = WellService.GetWellsByFilter(wellbyFilter);
            Assert.AreEqual(allwells.Count(), wells.Count());
            //Get Well details to display on Heat Map
            foreach (WellDTO well in wells)
            {
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                var wellstatusWithUnits = SurveillanceServiceClient.GetWellStatusData<ESPWellStatusUnitDTO, ESPWellStatusValueDTO>(well.Id.ToString());
                ESPWellStatusValueDTO wellstatus = wellstatusWithUnits.Value;
                HeatMapDTO wellDetails = HeatMapService.GetWellDetails(well.Id.ToString());
                Assert.AreEqual(wellstatus.WellName, wellDetails.WellName);
                //Assert.AreEqual(wellstatus.POCMode, wellDetails.Status);
            }
            HeatMapWellsDTO runningOrdownWellssForHeatMap = HeatMapService.GetWellsForHeatMap(wellbyFilter, "1", "5");
            HeatMapWellsDTO downOrAlarmingWellsForHeatMap = HeatMapService.GetWellsForHeatMap(wellbyFilter, "1", "6");
            Assert.IsNotNull(runningOrdownWellssForHeatMap);
            Assert.IsNotNull(downOrAlarmingWellsForHeatMap);
            if (downOrAlarmingWellsForHeatMap.WellsForHeatMap.Count == 1)
            {
                Assert.AreEqual(1, downOrAlarmingWellsForHeatMap.WellsForHeatMap.Count);
                Assert.AreEqual(allwells.FirstOrDefault().SurfaceLatitude, downOrAlarmingWellsForHeatMap.WellsForHeatMap.FirstOrDefault().SurfaceLatitude);
                Assert.AreEqual(allwells.FirstOrDefault().SurfaceLongitude, downOrAlarmingWellsForHeatMap.WellsForHeatMap.FirstOrDefault().SurfaceLongitude);
            }
            else if (runningOrdownWellssForHeatMap.WellsForHeatMap.Count == 1)
            {
                Assert.AreEqual(1, runningOrdownWellssForHeatMap.WellsForHeatMap.Count);
                Assert.AreEqual(allwells.FirstOrDefault().SurfaceLatitude, runningOrdownWellssForHeatMap.WellsForHeatMap.FirstOrDefault().SurfaceLatitude);
                Assert.AreEqual(allwells.FirstOrDefault().SurfaceLongitude, runningOrdownWellssForHeatMap.WellsForHeatMap.FirstOrDefault().SurfaceLongitude);
            }
        }

        //Test Method to check MTBF Component by Type Grid
        [TestCategory(TestCategories.HeatMapServiceTests), TestMethod]
        public void GetGroupComponentMTBF()
        {
            AddWell("RPOC_", WellTypeId.RRL);
            WellDTO[] allwells = WellService.GetAllWells();
            //Get Wells by Filter
            WellFilterDTO wellbyFilter = new WellFilterDTO();
            WellDTO[] wells = WellService.GetWellsByFilter(wellbyFilter);
            Assert.AreEqual(allwells.Count(), wells.Count());

            GroupComponentMTBF MTBFComponent = HeatMapService.GetGroupComponentMTBF(wellbyFilter, WellTypeId.RRL.ToString());
            var historycount = MTBFComponent.HistoricalMTBF.Count();
            Assert.AreEqual(0, historycount);//Newly added well
            foreach (PredictedComponentFailureCount predictedFailure in MTBFComponent.PredictedFailures)
            {
                //Newly added well
                Assert.AreEqual(0, predictedFailure.FailureCountIn15Days);
                Assert.AreEqual(0, predictedFailure.FailureCountIn30Days);
                Assert.AreEqual(0, predictedFailure.FailureCountIn60Days);
            }
            WellMTBFService.GenerateMTBFData();

            MTBFComponent = HeatMapService.GetGroupComponentMTBF(wellbyFilter, WellTypeId.RRL.ToString());
            historycount = MTBFComponent.HistoricalMTBF.Count();
            Assert.AreEqual(0, historycount);//No failures added
        }

        [TestCategory(TestCategories.HeatMapServiceTests), TestMethod]
        public void GetGroupComponentMTBF_ESP()
        {
            AddWell("ESPWELL_", WellTypeId.ESP);
            WellDTO[] allwells = WellService.GetAllWells();
            //Get Wells by Filter
            WellFilterDTO wellbyFilter = new WellFilterDTO();
            WellDTO[] wells = WellService.GetWellsByFilter(wellbyFilter);
            Assert.AreEqual(allwells.Count(), wells.Count());

            GroupComponentMTBF MTBFComponent = HeatMapService.GetGroupComponentMTBF(wellbyFilter, WellTypeId.ESP.ToString());
            var historycount = MTBFComponent.HistoricalMTBF.Count();
            Assert.AreEqual(0, historycount);//Newly added well
            foreach (PredictedComponentFailureCount predictedFailure in MTBFComponent.PredictedFailures)
            {
                //Newly added well
                Assert.AreEqual(0, predictedFailure.FailureCountIn15Days);
                Assert.AreEqual(0, predictedFailure.FailureCountIn30Days);
                Assert.AreEqual(0, predictedFailure.FailureCountIn60Days);
            }
            WellMTBFService.GenerateMTBFData();

            MTBFComponent = HeatMapService.GetGroupComponentMTBF(wellbyFilter, WellTypeId.ESP.ToString());
            historycount = MTBFComponent.HistoricalMTBF.Count();
            Assert.AreEqual(0, historycount);//No failures added
        }

        [TestCategory(TestCategories.HeatMapServiceTests), TestMethod]
        public void GetTopWellsToFail()
        {
            AddWell("RPOC_", WellTypeId.RRL);
            WellFilterDTO wellbyFilter = new WellFilterDTO();
            GroupComponentMTBF MTBFComponent = HeatMapService.GetGroupComponentMTBF(wellbyFilter, WellTypeId.RRL.ToString());
            foreach (PredictedComponentFailureCount PredictedFailure in MTBFComponent.PredictedFailures)
            {
                TopWellsToFail TopWellsToFailconf = HeatMapService.GetTopWellsToFail(wellbyFilter, WellTypeId.RRL.ToString(), PredictedFailure.ComponentType.ToString(), "15");
                Assert.AreEqual(TopWellsToFailconf.ComponentType, PredictedFailure.ComponentType);
                Assert.AreEqual(FailurePredictionPeriod.Days15, TopWellsToFailconf.PrimaryPredictionPeriod);
                TopWellsToFail TopWellsToFailconf2 = HeatMapService.GetTopWellsToFail(wellbyFilter, WellTypeId.RRL.ToString(), PredictedFailure.ComponentType.ToString(), "30");
                Assert.AreEqual(TopWellsToFailconf2.ComponentType, PredictedFailure.ComponentType);
                Assert.AreEqual(FailurePredictionPeriod.Days30, TopWellsToFailconf2.PrimaryPredictionPeriod);
                TopWellsToFail TopWellsToFailconf3 = HeatMapService.GetTopWellsToFail(wellbyFilter, WellTypeId.RRL.ToString(), PredictedFailure.ComponentType.ToString(), "60");
                Assert.AreEqual(TopWellsToFailconf3.ComponentType, PredictedFailure.ComponentType);
                Assert.AreEqual(FailurePredictionPeriod.Days60, TopWellsToFailconf3.PrimaryPredictionPeriod);
            }
        }

        [TestCategory(TestCategories.HeatMapServiceTests), TestMethod]
        public void GetTopWellsToFail_ESP()
        {
            AddWell("ESPWELL_", WellTypeId.ESP);
            WellFilterDTO wellbyFilter = new WellFilterDTO();
            GroupComponentMTBF MTBFComponent = HeatMapService.GetGroupComponentMTBF(wellbyFilter, WellTypeId.ESP.ToString());
            foreach (PredictedComponentFailureCount PredictedFailure in MTBFComponent.PredictedFailures)
            {
                TopWellsToFail TopWellsToFailconf = HeatMapService.GetTopWellsToFail(wellbyFilter, WellTypeId.ESP.ToString(), PredictedFailure.ComponentType.ToString(), "15");
                Assert.AreEqual(TopWellsToFailconf.ComponentType, PredictedFailure.ComponentType);
                Assert.AreEqual(FailurePredictionPeriod.Days15, TopWellsToFailconf.PrimaryPredictionPeriod);
                TopWellsToFail TopWellsToFailconf2 = HeatMapService.GetTopWellsToFail(wellbyFilter, WellTypeId.ESP.ToString(), PredictedFailure.ComponentType.ToString(), "30");
                Assert.AreEqual(TopWellsToFailconf2.ComponentType, PredictedFailure.ComponentType);
                Assert.AreEqual(FailurePredictionPeriod.Days30, TopWellsToFailconf2.PrimaryPredictionPeriod);
                TopWellsToFail TopWellsToFailconf3 = HeatMapService.GetTopWellsToFail(wellbyFilter, WellTypeId.ESP.ToString(), PredictedFailure.ComponentType.ToString(), "60");
                Assert.AreEqual(TopWellsToFailconf3.ComponentType, PredictedFailure.ComponentType);
                Assert.AreEqual(FailurePredictionPeriod.Days60, TopWellsToFailconf3.PrimaryPredictionPeriod);
            }
        }

        [TestCategory(TestCategories.HeatMapServiceTests), TestMethod]
        public void GetRRLBearingForTopWells()
        {

            // Add 11 RPOC wells
            var wellDTOs = AddRRLWells(10, "RPOC_");


            Assert.AreEqual(wellDTOs.Count, 10, "Desired number of wells were not succefully added.");

            WellFilterDTO wellbyFilter = new WellFilterDTO();
            var fetchedPinResult = HeatMapService.GetRRLBearingWristPinValues(wellbyFilter, "true").ToList();

            // Resort the fetched results again as per the requirements and compare it with fetched results
            List<WristPinDTO> expectedResult = new List<WristPinDTO>();
            var criticalPins = fetchedPinResult.Where(dto => dto.LeftWristPin == 3 || dto.RightWristPin == 3).ToList();
            var inspectionPins = fetchedPinResult.Where(dto => dto.LeftWristPin == 2 || dto.RightWristPin == 2).ToList();
            var goodPins = fetchedPinResult.Where(dto => dto.LeftWristPin == 1 || dto.RightWristPin == 1).ToList();
            var newPins = fetchedPinResult.Where(dto => dto.LeftWristPin == 0 || dto.RightWristPin == 0).ToList();
            var noDataPins = fetchedPinResult.Where(dto => dto.LeftWristPin == null || dto.RightWristPin == null).ToList();

            // Add critical pins first
            expectedResult.AddRange(criticalPins);

            // Then need inspection pins
            foreach (var item in inspectionPins)
            {
                if (!expectedResult.Contains(item))
                {
                    expectedResult.Add(item);
                }
            }

            // Then good pins
            foreach (var item in goodPins)
            {
                if (!expectedResult.Contains(item))
                {
                    expectedResult.Add(item);
                }
            }

            // Then new pins
            foreach (var item in newPins)
            {
                if (!expectedResult.Contains(item))
                {
                    expectedResult.Add(item);
                }
            }

            Assert.AreEqual(expectedResult.Count, fetchedPinResult.Count, "Mismatch in the count");

            for (int i = 0; i < fetchedPinResult.Count; i++)
            {
                Assert.AreEqual(expectedResult[i], fetchedPinResult[i]);
            }


            WellDTO[] allwells = WellService.GetAllWells();
            Trace.WriteLine(allwells.Length);

        }


        [TestCategory(TestCategories.HeatMapServiceTests), TestMethod]
        public void GetRRLUnitBalancingForTopWells()
        {
            // Add 10 RPOC wells
            var wellDTOs = AddRRLWells(10, "RPOC_");
            Assert.AreEqual(wellDTOs.Count, 10, "Desired number of wells were not succefully added.");

            WellFilterDTO wellbyFilter = new WellFilterDTO();

            // Get Unit Balancing values for top ten newly added RPOC wells
            var UnitBalancingTopWells = HeatMapService.GetRRLUnitBalancingValues(wellbyFilter, "true").ToList();

            // UnitBalancingDTO should never return any records having null values for unit balancing
            Assert.AreEqual(UnitBalancingTopWells.Count(), 10, "Unexpected count of UnitBalancingDTO list.");

            // Get a list of fetched Unit Balancing values
            var fetchedDataList = UnitBalancingTopWells.Select(dto => dto.UnitBalancing).ToList();


            // Sort the fetched list per the requirement i.e. Overbalancing, Underbalancing and then balancing.
            List<int?> sortedList = new List<int?>();
            sortedList = fetchedDataList.Where(v => v == 2).ToList();
            sortedList.AddRange(fetchedDataList.Where(v => v == 0).ToList());
            sortedList.AddRange(fetchedDataList.Where(v => v == 1).ToList());

            Assert.AreEqual(fetchedDataList.Count(), sortedList.Count, "Mismatch in fetched UB Vals and expected UB Vals.");

            for (int i = 0; i < fetchedDataList.Count; i++)
            {
                Assert.IsTrue(fetchedDataList[i] == sortedList[i], "The fetched UB values are not sorted properly.");
            }

            // Get UB vals for only single well
            UnitBalancingDTO[] UnitBalanceForSingleWell = HeatMapService.GetRRLUnitBalancingValues(wellbyFilter, "true", UnitBalancingTopWells[0].WellId.ToString());

            // Assuming that CygNet RPOC facility is configured for Unit Balancing point.
            Assert.AreEqual(1, UnitBalanceForSingleWell.Length);
            foreach (UnitBalancingDTO SingleWell in UnitBalanceForSingleWell)
            {
                Trace.WriteLine("Single Well Id is : " + SingleWell.WellId);
                Trace.WriteLine(SingleWell.UnitBalancing);
            }

            Assert.AreEqual(UnitBalancingTopWells[0].WellId.ToString(), UnitBalanceForSingleWell[0].WellId.ToString(), "Well ID is mismatched");

            var UnitBalancingAllWells = HeatMapService.GetRRLUnitBalancingValues(wellbyFilter, "false").ToList();
            WellDTO[] allwells = WellService.GetAllWells();
            Trace.WriteLine("allwells count are :" + allwells.Length);

        }

        public List<WellDTO> AddRRLWells(int wellCount, string facPrefix)
        {
            string facilityId;
            if (s_isRunningInATS)
                facilityId = "D5";
            else
                facilityId = "D4";

            List<WellDTO> wellDTOs = new List<WellDTO>();
            s_wellStart = 1;
            s_wellsEnd = wellCount;
            for (int i = s_wellStart; i <= s_wellsEnd; i++)
            {
                string wellName = facPrefix + (i).ToString(facilityId);
                WellDTO well = SetDefaultFluidType(new WellDTO()
                {
                    Name = wellName,
                    FacilityId = wellName,
                    DataConnection = GetDefaultCygNetDataConnection(),
                    WellType = WellTypeId.RRL,
                    Lease = "Lease_" + (i).ToString("D2"),
                    Foreman = "Foreman" + (i).ToString("D2"),
                    Field = "Field" + (i).ToString("D2"),
                    Engineer = "Engineer" + (i).ToString("D2"),
                    GaugerBeat = "GaugerBeat" + (i).ToString("D2"),
                    GeographicRegion = "GeographicRegion" + (i).ToString("D2"),
                    welUserDef01 = "State_" + (i).ToString("D2"),
                    welUserDef02 = "User_" + (i).ToString("D2"),
                    SubAssemblyAPI = "SubAssemblyAPI" + (i).ToString("D2"),
                    AssemblyAPI = "AssemblyAPI" + (i).ToString("D2"),
                    IntervalAPI = "IntervalAPI" + (i).ToString("D2"),
                    CommissionDate = DateTime.Today.AddDays(i),
                });
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
                WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(well.Name));
                Assert.IsNotNull(addedWell);
                _wellsToRemove.Add(addedWell);
                _addedWells.Add(addedWell);
                wellDTOs.Add(addedWell);
            }

            return wellDTOs;
        }



    }
}