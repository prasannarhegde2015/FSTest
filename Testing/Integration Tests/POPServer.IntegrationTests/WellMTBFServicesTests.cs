using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class WellMTBFServicesTests : APIClientTestBase
    {
        [TestInitialize]
        public override void Init()
        {
            base.Init();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            base.Cleanup();
        }

        //Test Method for WellMTBF Failure Probability Grid
        [TestCategory(TestCategories.WellMTBFServicesTests), TestMethod]
        public void GetCompsFailedByWell()
        {
            long wellId = AddWell("RPOC_");

            TopCompsFailByWell WellMTBFFailureConfidence = WellMTBFService.GetCompsFailedByWell(wellId.ToString());
            FailureConfidence[] WellFailureConfidencedetails = WellMTBFFailureConfidence.WellFailureProbs;
            Assert.AreEqual(4, WellFailureConfidencedetails.Length, "Count is not equal to expected value");
            //mock data to be replaced
            foreach (FailureConfidence Welldata in WellFailureConfidencedetails)
            {
                Assert.IsNotNull(Welldata.Id);
                Trace.WriteLine(Welldata.Id);
                Assert.IsNotNull(Welldata.Name);
                Trace.WriteLine(Welldata.Name);
                Assert.IsNotNull(Welldata.FailureConfidenceIn15Days);
                Trace.WriteLine(Welldata.FailureConfidenceIn15Days);
                Assert.IsNotNull(Welldata.FailureConfidenceIn30Days);
                Trace.WriteLine(Welldata.FailureConfidenceIn30Days);
                Assert.IsNotNull(Welldata.FailureConfidenceIn60Days);
                Trace.WriteLine(Welldata.FailureConfidenceIn60Days);
            }
        }

        //Test Method for WellMTBF POST/Get Added Comments
        [TestCategory(TestCategories.WellMTBFServicesTests), TestMethod]
        public void SaveGetWellComments()
        {
            WellCommentDTO AddGetWellComments = new WellCommentDTO();
            //Add new Well in Database
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });
            WellDTO[] allWells = WellService.GetAllWells();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            //Add comments to the newly added Well
            WellCommentDTO wdto = new WellCommentDTO();
            wdto.WellId = well.Id;
            wdto.WellComment = "testing comments";
            //   Assert.IsTrue(wdto.WellComment.Length < 500, "Saved Successfully");
            WellService.SaveComments(wdto);
            wdto.WellComment = "testing comments two";
            Assert.IsTrue(wdto.WellComment.Length < 500, "Saved Successfully");
            WellService.SaveComments(wdto);
            //Get the Comments added in well
            WellCommentDTO[] WellCommentsData = WellService.GetWellComments(well.Id.ToString());
            Assert.IsNotNull(WellCommentsData);
            Assert.AreEqual("testing comments", WellCommentsData[1].WellComment); // I added only 2 so checking only 2
            Assert.AreEqual("testing comments two", WellCommentsData[0].WellComment);
        }

        //Test Method for Get Trend for SPM, Cycles and Run time
        [TestCategory(TestCategories.WellMTBFServicesTests), TestMethod]
        public void GetTrends()
        {
            //WellService.AddWell(SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "RPOC_0001", GetDefaultCygNetDataConnection()));
            string facilityId;
            if (s_isRunningInATS)
                facilityId = "RPOC_00001";
            else
                facilityId = "RPOC_0002";
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = facilityId, IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.RRL }) });

            WellDTO[] allWells = WellService.GetAllWells();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);

            MTBFTrendDTO[] mtbfTrend = WellMTBFService.GetTrends(well.Id.ToString(), DTOExtensions.ToISO8601(DateTime.UtcNow.AddDays(-1)), DTOExtensions.ToISO8601(DateTime.UtcNow.AddDays(-1)));

            // MTBF trend data is averaged 'hourly' in the backend API. If we want to compare the MTBF trend
            // data with the some expected results, the actual VHS data needs to go with same averaging before comparing.
            string errorMessage = string.Empty;
            string[] avgSPM = { ((int)WellQuantity.AverageSPM).ToString() };
            CygNetTrendDTO spmTrend = SurveillanceService.GetCygNetTrendsByUDCAndDateRange(avgSPM, well.Id.ToString(), DTOExtensions.ToISO8601(DateTime.UtcNow.AddDays(-1)), DTOExtensions.ToISO8601(DateTime.UtcNow)).FirstOrDefault();
            Assert.IsNotNull(spmTrend.PointValues, "Average SPM has no trend values.");
            var avgSPMTrend = GetAverageDailyValues(spmTrend, out errorMessage, "hour");

            string[] runTimeYest = { ((int)WellQuantity.RunTimeYesterday).ToString() };
            CygNetTrendDTO runtimeTrend = SurveillanceService.GetCygNetTrendsByUDCAndDateRange(runTimeYest, well.Id.ToString(), DTOExtensions.ToISO8601(DateTime.UtcNow.AddDays(-1)), DTOExtensions.ToISO8601(DateTime.UtcNow)).FirstOrDefault();
            Assert.IsNotNull(runtimeTrend.PointValues, "Run Time has no trend values.");
            var avgRunTimeTrend = GetAverageDailyValues(runtimeTrend, out errorMessage, "hour");

            string[] infProdYest = { ((int)WellQuantity.InferredProductionYesterday).ToString() };
            CygNetTrendDTO inferredTrend = SurveillanceService.GetCygNetTrendsByUDCAndDateRange(infProdYest, well.Id.ToString(), DTOExtensions.ToISO8601(DateTime.UtcNow.AddDays(-1)), DTOExtensions.ToISO8601(DateTime.UtcNow)).FirstOrDefault();
            Assert.IsNotNull(inferredTrend.PointValues, "Inferred Production has no trend values.");
            var avgInferredProdTrend = GetAverageDailyValues(inferredTrend, out errorMessage, "hour");

            string[] cyclesYest = { ((int)WellQuantity.CyclesYesterday).ToString() };
            CygNetTrendDTO cycleTrend = SurveillanceService.GetCygNetTrendsByUDCAndDateRange(cyclesYest, well.Id.ToString(), DTOExtensions.ToISO8601(DateTime.UtcNow.AddDays(-1)), DTOExtensions.ToISO8601(DateTime.UtcNow)).FirstOrDefault();
            Assert.IsNotNull(cycleTrend.PointValues, "Cycles Yesterday has no trend values.");
            var avgCycleYestTrend = GetAverageDailyValues(cycleTrend, out errorMessage, "hour");

            double mtbfSPMVal = Math.Round((double)mtbfTrend[0].SPMValue, 2, MidpointRounding.AwayFromZero);
            double mtbfRunTimeVal = Math.Round((double)mtbfTrend[0].RunTimeValue, 2, MidpointRounding.AwayFromZero);
            double mtbfIPVal = Math.Round((double)mtbfTrend[0].InferredProductionValue, 2, MidpointRounding.AwayFromZero);
            Assert.AreEqual((double)avgSPMTrend.First().Value, mtbfSPMVal, 0.1, "MTBF SPM Trend Mismatch");
            Assert.AreEqual((double)avgRunTimeTrend.First().Value, mtbfRunTimeVal, 0.1, "MTBF Run value Mimatch");
            Assert.AreEqual((double)avgInferredProdTrend.First().Value, mtbfIPVal, 0.1, "MTBF Inferred Production value Mismatch");
            //To chk for the Failure/Off date of the Well we need to get that Off date from Report Table and compare that Off date with FailedSPMValue.(Currently no method to get data from Report table)
            //When both Off date and Failed SPM values are equal then dots will be shown on SPM Trend
        }

        private List<GeneralTrendPointDTO> GetAverageDailyValues(CygNetTrendDTO trendDTO, out string errorMessage, string averageBy = "date", sbyte? precision = 2)
        {
            errorMessage = "success";
            List<GeneralTrendPointDTO> trendPointList = new List<GeneralTrendPointDTO>();
            List<DateTime> uniqDates = new List<DateTime>();

            foreach (GeneralTrendPointDTO point in trendDTO.PointValues)
            {
                DateTime dt = point.Timestamp.Date;
                if (averageBy.ToLower() == "hour")
                    dt = dt.AddHours(point.Timestamp.Hour);

                if (!uniqDates.Contains(dt))
                    uniqDates.Add(dt);
            }

            foreach (DateTime uniqDate in uniqDates)
            {
                double total = 0;
                int count = 0;
                double average = 0;
                foreach (GeneralTrendPointDTO point in trendDTO.PointValues)
                {
                    if (averageBy.ToLower() == "hour")
                    {
                        if (point.Timestamp.Date == uniqDate.Date && point.Timestamp.Hour == uniqDate.Hour)
                        {
                            total = total + point.Value.Value;
                            count++;
                            average = total / count;
                        }
                    }
                    else if (averageBy.ToLower() == "date")
                    {
                        if (point.Timestamp.Date == uniqDate.Date)
                        {
                            total = total + point.Value.Value;
                            count++;
                            average = total / count;
                        }
                    }
                }
                //create new obj with date and average value
                sbyte prec = precision == null ? (sbyte)2 : (sbyte)precision;

                GeneralTrendPointDTO trendPoint = new GeneralTrendPointDTO()
                {
                    Timestamp = uniqDate,
                    Value = Math.Round(average, prec, MidpointRounding.AwayFromZero)
                };
                trendPointList.Add(trendPoint);
            }

            return trendPointList;
        }

        //Test Method for Get Failure Details
        [TestCategory(TestCategories.WellMTBFServicesTests), TestMethod]
        public void GetFailureDetails()
        {
            int i = 1;
            string wellName;
            if (s_isRunningInATS)
            {
                wellName = "RPOC_" + (i).ToString("D5");
            }
            else
            {
                wellName = "RPOC_" + (i).ToString("D4");
            }

            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = "RPOCTest01", FacilityId = wellName, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });

            WellDTO[] allWells = WellService.GetAllWells();
            Assert.IsNotNull(allWells, "Failed to get wells.");
            WellDTO well = allWells.FirstOrDefault(w => w.Name.Equals("RPOCTest01"));
            Assert.IsNotNull(well, "Failed to add well.");
            _wellsToRemove.Add(well);

            //Test add SubAssembly
            AssemblyDTO getAssembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            SubAssemblyDTO newSubAssembly = new SubAssemblyDTO
            {
                AssemblyId = getAssembly.Id,
                SubAssembly_ParentId = null,
                SubAssemblyType = 2,
                SAId = "WELLBORE - " + well.Name + well.Id.ToString(), //Need to fillup this field
                WellDepthDatum = 11,// for groundlevel by now
                UserId = 1,
                StateTime = DateTime.Today,
            };
            WellboreComponentService.AddSubAssembly(newSubAssembly);
            SubAssemblyDTO getSubAssemblies = WellboreComponentService.GetSubAssembliesByWellId(well.Id.ToString()).FirstOrDefault();

            //test add report With No Failure Workover
            ReportDTO newReport = new ReportDTO
            {
                WellId = well.Id,
                AssemblyId = getAssembly.Id,
                JobReasonId = 309,
                JobTypeId = 12,
                ReportTypeId = 3,
                OffDate = (DateTime.Today),
                WorkoverDate = (DateTime.Today.AddDays(5)),
                Comment = "Add Report With No Failure",
            };

            WellboreComponentService.AddReport(newReport);
            //ReportDTO reportExists = WellboreComponentService.GetReportByAssemblyId(getAssembly.Id.ToString());
            ReportDTO reportExists = WellboreComponentService.GetReportByWellId(well.Id.ToString());
            Assert.IsNotNull(reportExists);
            Assert.AreEqual(reportExists.ReportTypeId, 3);

            //test to add Component and Assembly Component
            ComponentDTO newComponent = new ComponentDTO
            {
                CatalogItem = -1,
                MfgCatalogItem = -1,
                ComponentUsage = -1,
                TubularConnectionSpecs_Top = -1,
                TubularConnectionSpecs_Bottom = -1,
                SurfaceCondition = 1,
                ComponentOrigin = -1,
                //CurrentOwnerId = 1,
                BusinessOrganizationId = -1,
                MfgCat_PartType = 20,
                PartType = "Rod",
                OuterDiameter = 1,
                InnerDiameter = 4,
            };

            //WellboreComponentService.AddComponent(newComponent);
            //long getcomponent = WellboreComponentService.getLatestComponentId();

            //test add Assembly Component
            AssemblyComponentDTO newAssemblyComponent = new AssemblyComponentDTO
            {
                AssemblyId = getAssembly.Id,
                BeginEventDT = DateTime.Today.ToLocalTime(),
                BottomDepth = 100,
                DataQualityCode = 0,
                EndEventDT = new DateTime(2038, 1, 18, 0, 0, 1, DateTimeKind.Local),
                // ComponentId = getcomponent,
                ReportId = reportExists.Id,
                ComponentCouplingType = 1,
                ComponentGrouping = 2,//2-Rod String, 4 - Pump string ,15 - production Casing
                                      //  Grouping = "Rod String",
                SubassemblyId = getSubAssemblies.Id,
                InstallDate = DateTime.Today,
                KnownLength = false,
                KnownTopDepth = false,
                PreviousRunDays = 0,
                Quantity = 1,
                TopDepth = 0,
                TrueVerticalDepth = 0,
                TrueVerticalDepthBottom = 0,
                AssemblyOrder = 0,
                Length = 100,
            };
            WellboreComponentService.AddAssemblyComponentComponent(new AssemblyComponentAndComponentPairDTO(newAssemblyComponent, newComponent));
            AssemblyComponentDTO[] getassemblycomponents = WellboreComponentService.GetAssemblyComponentsByWellId(well.Id.ToString());

            WellboreComponentService.AddAssemblyComponentComponent(new AssemblyComponentAndComponentPairDTO(newAssemblyComponent, newComponent));
            //WellboreComponentService.AddAssemblyComponent(newAssemblyComponent);
            AssemblyComponentDTO getassemblycomponent = WellboreComponentService.GetAssemblyComponentsByWellId(well.Id.ToString()).FirstOrDefault();
            Assert.IsNotNull(getassemblycomponent);

            WellboreImageDTO[] RodInfo = WellMTBFService.GetRodDetailsforWellboreDiagram(well.Id.ToString(), "0", "null");
            Assert.IsNotNull(RodInfo);
            foreach (WellboreImageDTO RodData in RodInfo)
            {
                Assert.IsNotNull(RodData.PartId);
                Assert.IsFalse(RodData.IsFailed);
                Assert.AreEqual(RodData.BottomDepth, 100);
                Assert.AreEqual(RodData.InnerDiameter, 4);
                Assert.AreEqual(RodData.PartType, "Rod");
                Assert.AreEqual(RodData.PartStatus, 1);
            }

            //Test to add Rod Details
            RodDetailsDTO AddRodDetails = new RodDetailsDTO
            {
                FK_AssemblyComponent = getassemblycomponents[getassemblycomponents.Length - 1].Id,
                Grade = "750N",
                Guides = 3,
                LengthPerRod = 2000,
                Manufacturer = "Norris",
                NumberOfRods = 2,
                ServiceFactor = 1,
                TaperNum = 1,
            };
            WellboreComponentService.AddRodDetails(AddRodDetails);

            DetailAssemblyComponentFailureDTO[] list = new DetailAssemblyComponentFailureDTO[1];
            DetailAssemblyComponentFailureDTO obj = new DetailAssemblyComponentFailureDTO();

            foreach (AssemblyComponentDTO assemblyComp in getassemblycomponents)
            {
                obj.AscPrimaryKey = assemblyComp.Id;
                obj.FailureDate = DateTime.SpecifyKind(Convert.ToDateTime("11/14/2017"), DateTimeKind.Local);
                obj.WorkoverDate = DateTime.SpecifyKind(Convert.ToDateTime("11/20/2017"), DateTimeKind.Local);
                obj.Comments = "Failed the component";
                obj.FailedDepth = 756.00m;
                obj.FailureCorrosionType = 1;
                obj.FailureLocation = 1;
                obj.FailureObservation = 1;
                obj.CorrosionAmount = 1;
                obj.FailureCorrosionType = 1;
            }
            list[0] = obj;

            Assert.AreEqual(obj.Comments, "Failed the component");
            Assert.AreEqual(obj.CorrosionAmount, 1);
            Assert.AreEqual(obj.FailedDepth, 756);
            Assert.AreEqual(obj.FailureCorrosionType, 1);
            Assert.AreEqual(obj.FailureLocation, 1);
            Assert.AreEqual(obj.FailureDate, Convert.ToDateTime("11/14/2017"));

            //test add report with Failure workover
            ReportDTO FailReport = new ReportDTO
            {
                WellId = well.Id,
                AssemblyId = getAssembly.Id,
                JobReasonId = 309,
                JobTypeId = 12,
                ReportTypeId = 2,
                OffDate = (DateTime.Today),
                WorkoverDate = (DateTime.Today.AddDays(5)),
                Comment = "Yes Failure",
                FailedComponents = list
            };

            WellboreComponentService.AddReport(FailReport);

            ReportDTO reportExistsF = WellboreComponentService.GetReportByWellId(well.Id.ToString());
            Assert.IsNotNull(reportExistsF);
            Assert.AreEqual(2, reportExistsF.ReportTypeId);
            Assert.AreEqual(reportExistsF.Comment, "Yes Failure");
            Assert.AreEqual(reportExistsF.JobReasonId, 309);
            Assert.AreEqual(reportExistsF.JobTypeId, 12);
            Assert.AreEqual(reportExistsF.Assembly.Name, well.AssemblyAPI);

            //test Finalize Workover withFailure
            ReportDTO finalize = new ReportDTO
            {
                Id = reportExistsF.Id,
                Comment = "Test Failure",
                OnDate = (DateTime.Today)
            };
            ReportDTO dtoNew = WellboreComponentService.FinalizeWorkover(finalize);
            Assert.AreEqual(dtoNew.OnDate, finalize.OnDate);

            DetailAssemblyComponentFailureDTO failure = WellMTBFService.GetFailureDetails(obj.AscPrimaryKey.ToString());
            Assert.AreEqual("Failed the component", failure.Comments, "Comments in failure not equal");
            Assert.AreEqual(failure.CorrosionAmount, 1);
            Assert.AreEqual(failure.FailedDepth, 756);
            Assert.AreEqual(failure.FailureCorrosionType, 1);
        }

        [TestCategory(TestCategories.WellMTBFServicesTests), TestMethod]
        public void GetRodDetailsforWellboreDiagram()
        {
            int i = 1;
            string wellName;
            if (s_isRunningInATS)
            {
                wellName = "RPOC_" + (i).ToString("D5");
            }
            else
            {
                wellName = "RPOC_0252";
            }

            WellDTO[] allWellsBefore = WellService.GetAllWells();
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = "new_TestWell", FacilityId = wellName, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });

            WellDTO[] allWells = WellService.GetAllWells();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals("new_TestWell"));
            _wellsToRemove.Add(well);

            SubAssemblyDTO getSubAssemblies = WellboreComponentService.GetSubAssembliesByWellId(well.Id.ToString()).FirstOrDefault();

            //test add report
            ReportDTO newReport = new ReportDTO
            {
                WellId = well.Id,
                AssemblyId = well.AssemblyId,
                JobReasonId = 309,
                JobTypeId = 12,
                ReportTypeId = 3,
                OffDate = (DateTime.Today),
                WorkoverDate = (DateTime.Today.AddDays(5)),
                Comment = "Add Report",
            };

            WellboreComponentService.AddReport(newReport);
            ReportDTO reportExists = WellboreComponentService.GetReportByWellId(well.Id.ToString());
            Assert.IsNotNull(reportExists);
            Assert.AreEqual(reportExists.ReportTypeId, 3);
            Assert.AreEqual(well.AssemblyId, reportExists.AssemblyId);

            //test to add Component
            ComponentDTO newComponent = new ComponentDTO
            {
                CatalogItem = -1,
                MfgCatalogItem = -1,
                ComponentUsage = -1,
                TubularConnectionSpecs_Top = -1,
                TubularConnectionSpecs_Bottom = -1,
                SurfaceCondition = 1,
                ComponentOrigin = -1,
                //CurrentOwnerId = 1,
                BusinessOrganizationId = -1,
                MfgCat_PartType = 20,       //{ 20, 49 } : { "Rod", "Pump (Downhole)" }
                PartType = "Rod",
                OuterDiameter = 1,
                InnerDiameter = 4,
            };

            //WellboreComponentService.AddComponent(newComponent);
            //long getcomponent = WellboreComponentService.getLatestComponentId();

            //test add Assembly Component
            AssemblyComponentDTO newAssemblyComponent = new AssemblyComponentDTO
            {
                AssemblyId = well.AssemblyId,
                BeginEventDT = DateTime.Today.ToLocalTime(),
                BottomDepth = 100,
                DataQualityCode = 0,
                EndEventDT = new DateTime(2038, 1, 18, 0, 0, 1, DateTimeKind.Local),
                //ComponentId = getcomponent,
                ReportId = reportExists.Id,
                ComponentCouplingType = 1,
                ComponentGrouping = 2,//2-Rod String, 2 - Pump string ,15 - production Casing
                                      //  Grouping = "Rod String",
                SubassemblyId = getSubAssemblies.Id,
                InstallDate = DateTime.Today,
                KnownLength = false,
                KnownTopDepth = false,
                PreviousRunDays = 0,
                Quantity = 1,
                TopDepth = 0,
                TrueVerticalDepth = 0,
                TrueVerticalDepthBottom = 0,
            };
            WellboreComponentService.AddAssemblyComponentComponent(new AssemblyComponentAndComponentPairDTO(newAssemblyComponent, newComponent));
            //WellboreComponentService.AddAssemblyComponent(newAssemblyComponent);
            AssemblyComponentDTO getassemblycomponent = WellboreComponentService.GetAssemblyComponentsByWellId(well.Id.ToString()).FirstOrDefault();
            Assert.IsNotNull(getassemblycomponent);

            WellboreImageDTO[] RodInfo = WellMTBFService.GetRodDetailsforWellboreDiagram(well.Id.ToString(), "0", "null");
            Assert.IsNotNull(RodInfo);
            foreach (WellboreImageDTO RodData in RodInfo)
            {
                Assert.IsNotNull(RodData.PartId);
                Assert.IsFalse(RodData.IsFailed);
                Assert.AreEqual(RodData.BottomDepth, 100);
                Assert.AreEqual(RodData.InnerDiameter, 4);
                Assert.AreEqual(RodData.PartType, "Rod");
                Assert.AreEqual(RodData.PartStatus, 1);
            }
        }

        [TestCategory(TestCategories.WellMTBFServicesTests), TestMethod]
        public void GetTubingDetailsforWellboreDiagram()
        {
            int i = 1;
            string wellName;
            if (s_isRunningInATS)
            {
                wellName = "RPOC_" + (i).ToString("D5");
            }
            else
            {
                wellName = "RPOC_0252";
            }

            WellDTO[] allWellsBefore = WellService.GetAllWells();
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = "new_TestWell", FacilityId = wellName, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });

            WellDTO[] allWells = WellService.GetAllWells();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals("new_TestWell"));
            _wellsToRemove.Add(well);

            SubAssemblyDTO getSubAssemblies = WellboreComponentService.GetSubAssembliesByWellId(well.Id.ToString()).FirstOrDefault();

            //test add report
            ReportDTO newReport = new ReportDTO
            {
                WellId = well.Id,
                AssemblyId = well.AssemblyId,
                JobReasonId = 309,
                JobTypeId = 12,
                ReportTypeId = 3,
                OffDate = (DateTime.Today),
                WorkoverDate = (DateTime.Today.AddDays(5)),
                Comment = "Add Report",
            };

            WellboreComponentService.AddReport(newReport);
            ReportDTO reportExists = WellboreComponentService.GetReportByWellId(well.Id.ToString());
            Assert.IsNotNull(reportExists);
            Assert.AreEqual(reportExists.ReportTypeId, 3);
            Assert.AreEqual(well.AssemblyId, reportExists.AssemblyId);

            //test to add Component
            ComponentDTO newComponent = new ComponentDTO
            {
                CatalogItem = -1,
                MfgCatalogItem = -1,
                ComponentUsage = -1,
                TubularConnectionSpecs_Top = -1,
                TubularConnectionSpecs_Bottom = -1,
                SurfaceCondition = 1,
                ComponentOrigin = -1,
                //CurrentOwnerId = 1,
                BusinessOrganizationId = -1,
                MfgCat_PartType = 394,         //{ 394, 49, 12, 18 }:
                                               //{ "Tubing ", "Pump (Downhole)" ,"Perforations"," Tubing Anchor/Catcher"}
                PartType = "Tubing",
                OuterDiameter = 4,
                InnerDiameter = 1,
            };

            //WellboreComponentService.AddComponent(newComponent);
            //long getcomponent = WellboreComponentService.getLatestComponentId();

            //test add Assembly Component
            AssemblyComponentDTO newAssemblyComponent = new AssemblyComponentDTO
            {
                AssemblyId = well.AssemblyId,
                BeginEventDT = DateTime.Today.ToLocalTime(),
                BottomDepth = 200,
                DataQualityCode = 0,
                EndEventDT = new DateTime(2038, 1, 18, 0, 0, 1, DateTimeKind.Local),
                //ComponentId = getcomponent,
                ReportId = reportExists.Id,
                ComponentCouplingType = 1,
                ComponentGrouping = 4,//2-Rod String, 4 - Tubing ,15 - production Casing
                                      //  Grouping = "Rod String",
                SubassemblyId = getSubAssemblies.Id,
                InstallDate = DateTime.Today,
                KnownLength = false,
                KnownTopDepth = false,
                PreviousRunDays = 0,
                Quantity = 1,
                TopDepth = 0,
                TrueVerticalDepth = 0,
                TrueVerticalDepthBottom = 0,
            };
            //WellboreComponentService.AddAssemblyComponent(newAssemblyComponent);
            WellboreComponentService.AddAssemblyComponentComponent(new AssemblyComponentAndComponentPairDTO(newAssemblyComponent, newComponent));
            AssemblyComponentDTO getassemblycomponent = WellboreComponentService.GetAssemblyComponentsByWellId(well.Id.ToString()).FirstOrDefault();

            WellboreImageDTO[] TubingInfo = WellMTBFService.GetTubingDetailsforWellboreDiagram(well.Id.ToString(), "0", "null");
            Assert.IsNotNull(TubingInfo);

            foreach (WellboreImageDTO TubingData in TubingInfo)
            {
                Assert.IsNotNull(TubingData.PartId);
                Assert.IsFalse(TubingData.IsFailed);
                Assert.AreEqual(TubingData.TopDepth, 0);
                Assert.AreEqual(TubingData.BottomDepth, 200);
                Assert.AreEqual(TubingData.InnerDiameter, 1);
                Assert.AreEqual(TubingData.OuterDiameter, 4);
                Assert.AreEqual(TubingData.PartType, "Tubing");
                Assert.AreEqual(TubingData.PartStatus, 1);
            }
        }

        [TestCategory(TestCategories.WellMTBFServicesTests), TestMethod]
        public void GetVersionDetails()
        {
            int i = 1;
            string wellName;
            if (s_isRunningInATS)
                wellName = "RPOC_" + (i).ToString("D5");
            else
                wellName = "RPOC_0252";

            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = "new_TestWell", FacilityId = wellName, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });
            WellDTO[] allWells = WellService.GetAllWells();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals("new_TestWell"));
            Assert.AreEqual("new_TestWell", well.Name);
            _wellsToRemove.Add(well);

            //Test add SubAssembly
            AssemblyDTO getAssembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            SubAssemblyDTO newSubAssembly = new SubAssemblyDTO
            {
                AssemblyId = getAssembly.Id,
                SubAssembly_ParentId = null,
                SubAssemblyType = 2,
                SAId = "WELLBORE - " + well.Name + well.Id.ToString(), //Need to fillup this field
                WellDepthDatum = 11,// for groundlevel by now
                UserId = 1,
                StateTime = DateTime.Today,
            };
            WellboreComponentService.AddSubAssembly(newSubAssembly);
            SubAssemblyDTO getSubAssemblies = WellboreComponentService.GetSubAssembliesByWellId(well.Id.ToString()).FirstOrDefault();

            //test add report With No Failure Workover
            ReportDTO newReport = new ReportDTO
            {
                WellId = well.Id,
                AssemblyId = getAssembly.Id,
                JobReasonId = 309,
                JobTypeId = 12,
                ReportTypeId = 3,
                OffDate = (DateTime.Today),
                WorkoverDate = (DateTime.Today.AddDays(5)),
                Comment = "Add Report With No Failure",
            };

            WellboreComponentService.AddReport(newReport);
            ReportDTO reportExists = WellboreComponentService.GetReportByAssemblyId(getAssembly.Id.ToString());
            Assert.IsNotNull(reportExists);
            Assert.AreEqual(reportExists.ReportTypeId, 3);

            //test to add Component and Assembly Component
            ComponentDTO newComponent = new ComponentDTO
            {
                CatalogItem = -1,
                MfgCatalogItem = -1,
                ComponentUsage = -1,
                TubularConnectionSpecs_Top = -1,
                TubularConnectionSpecs_Bottom = -1,
                SurfaceCondition = 1,
                ComponentOrigin = -1,
                //CurrentOwnerId = 1,
                BusinessOrganizationId = -1,
                MfgCat_PartType = 20,
                PartType = "Rod",
                OuterDiameter = 1,
                InnerDiameter = 4,
            };

            //WellboreComponentService.AddComponent(newComponent);
            //long getcomponent = WellboreComponentService.getLatestComponentId();

            //test add Assembly Component
            AssemblyComponentDTO newAssemblyComponent = new AssemblyComponentDTO
            {
                AssemblyId = getAssembly.Id,
                BeginEventDT = DateTime.Today.ToLocalTime(),
                BottomDepth = 0,
                DataQualityCode = 0,
                EndEventDT = new DateTime(2038, 1, 18, 0, 0, 1, DateTimeKind.Local),
                // ComponentId = getcomponent,
                ReportId = reportExists.Id,
                ComponentCouplingType = 1,
                ComponentGrouping = 2,//2-Rod String, 4 - Pump string ,15 - production Casing
                                      //  Grouping = "Rod String",
                SubassemblyId = getSubAssemblies.Id,
                InstallDate = DateTime.Today,
                KnownLength = false,
                KnownTopDepth = false,
                PreviousRunDays = 0,
                Quantity = 1,
                TopDepth = 0,
                TrueVerticalDepth = 0,
                TrueVerticalDepthBottom = 0,
                AssemblyOrder = 0,
                Length = 100,
            };
            WellboreComponentService.AddAssemblyComponentComponent(new AssemblyComponentAndComponentPairDTO(newAssemblyComponent, newComponent));
            AssemblyComponentDTO[] getassemblycomponents = WellboreComponentService.GetAssemblyComponentsByWellId(well.Id.ToString());

            //Test to add Rod Details
            RodDetailsDTO AddRodDetails = new RodDetailsDTO
            {
                FK_AssemblyComponent = getassemblycomponents[getassemblycomponents.Length - 1].Id,
                Grade = "750N",
                Guides = 3,
                LengthPerRod = 2000,
                Manufacturer = "Norris",
                NumberOfRods = 2,
                ServiceFactor = 1,
                TaperNum = 1,
            };
            WellboreComponentService.AddRodDetails(AddRodDetails);

            DetailAssemblyComponentFailureDTO[] list = new DetailAssemblyComponentFailureDTO[1];
            DetailAssemblyComponentFailureDTO obj = new DetailAssemblyComponentFailureDTO();
            foreach (AssemblyComponentDTO assemblyComp in getassemblycomponents)
            {
                obj.AscPrimaryKey = assemblyComp.Id;
                obj.FailureDate = DateTime.SpecifyKind(Convert.ToDateTime("11/14/2017"), DateTimeKind.Local);
                obj.WorkoverDate = DateTime.SpecifyKind(Convert.ToDateTime("11/20/2017"), DateTimeKind.Local);
                obj.Comments = "Failed the component";
                obj.FailedDepth = 756.00m;
                obj.FailureCorrosionType = 1;
                obj.FailureLocation = 1;
                obj.FailureObservation = 1;
                obj.CorrosionAmount = 1;
                obj.FailureCorrosionType = 1;
            }
            list[0] = obj;

            Assert.AreEqual(obj.Comments, "Failed the component");
            Assert.AreEqual(obj.CorrosionAmount, 1);
            Assert.AreEqual(obj.FailedDepth, 756);
            Assert.AreEqual(obj.FailureCorrosionType, 1);
            Assert.AreEqual(obj.FailureLocation, 1);
            Assert.AreEqual(obj.FailureDate, Convert.ToDateTime("11/14/2017"));

            //test add report with Failure workover
            ReportDTO FailReport = new ReportDTO
            {
                WellId = well.Id,
                AssemblyId = getAssembly.Id,
                JobReasonId = 309,
                JobTypeId = 12,
                ReportTypeId = 2,
                OffDate = (DateTime.Today),
                WorkoverDate = (DateTime.Today.AddDays(5)),
                Comment = "Yes Failure",
                FailedComponents = list
            };

            WellboreComponentService.AddReport(FailReport);

            ReportDTO reportExistsF = WellboreComponentService.GetReportByWellId(well.Id.ToString());
            Assert.IsNotNull(reportExistsF);
            Assert.AreEqual(2, reportExistsF.ReportTypeId);
            Assert.AreEqual(reportExistsF.Comment, "Yes Failure");
            Assert.AreEqual(reportExistsF.JobReasonId, 309);
            Assert.AreEqual(reportExistsF.JobTypeId, 12);
            //As per API 10-12-14 implementation, Assembly name & Well name won't be same. Wellbore Id provided on UI will be assembly Id.
            Assert.AreEqual(reportExistsF.Assembly.Name, well.AssemblyAPI);

            //test Finalize Workover withFailure
            ReportDTO finalize = new ReportDTO
            {
                Id = reportExistsF.Id,
                Comment = "Test Failure",
                OnDate = (DateTime.Today)
            };
            ReportDTO dtoNew = WellboreComponentService.FinalizeWorkover(finalize);
            Assert.AreEqual(dtoNew.OnDate, finalize.OnDate);

            foreach (WellDTO NewWell in allWells)
            {
                VersionDTO[] versions = WellMTBFService.GetVersionDetails(well.Id.ToString());

                foreach (VersionDTO ver in versions)
                {
                    Assert.IsNotNull(ver);
                }
            }
        }

        [TestCategory(TestCategories.WellMTBFServicesTests), TestMethod]
        public void GetAdditionalRodDetailsforToolTip()
        {
            int i = 1;
            string wellName;
            if (s_isRunningInATS)
            {
                wellName = "RPOC_" + (i).ToString("D5");
            }
            else
            {
                wellName = "RPOC_0252";
            }
            WellDTO well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = wellName, DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today, AssemblyAPI = "1234567890", SubAssemblyAPI = "123456789012", IntervalAPI = "12345678901234", WellType = WellTypeId.RRL });

            #region model

            //Surface
            SurfaceConfigDTO SampleSurfaceConfig = new SurfaceConfigDTO();
            PumpingUnitManufacturerDTO[] manufacturers = CatalogService.GetAllPumpingUnitManufacturers();
            PumpingUnitManufacturerDTO pumpingUnitManufacturer = manufacturers.FirstOrDefault(pumt => pumt.Name.Equals("Lufkin"));
            Assert.IsNotNull(pumpingUnitManufacturer);
            PumpingUnitTypeDTO[] pumpingUnitTypes = CatalogService.GetPumpingUnitTypesByManufacturer(pumpingUnitManufacturer.Name);
            PumpingUnitTypeDTO pumpingUnitType = pumpingUnitTypes.FirstOrDefault(t => t.AbbreviatedName.Equals("C"));
            Assert.IsNotNull(pumpingUnitType);
            PumpingUnitDTO[] pumpingUnits = CatalogService.GetPumpingUnitsByManufacturerAndType(pumpingUnitManufacturer.Name, pumpingUnitType.AbbreviatedName);
            PumpingUnitDTO pumpingUnitBase = pumpingUnits.FirstOrDefault(pu => pu.Description.Equals("C-912-365-168 L LUFKIN C912-365-168 (94110C)"));
            Assert.IsNotNull(pumpingUnitBase);
            PumpingUnitDTO pumpingUnit = CatalogService.GetPumpingUnitByManufacturerTypeAndDescription(pumpingUnitManufacturer.Name, pumpingUnitType.AbbreviatedName, pumpingUnitBase.Description);
            SampleSurfaceConfig.PumpingUnit = pumpingUnit;
            SampleSurfaceConfig.PumpingUnitType = pumpingUnitType;
            SampleSurfaceConfig.ClockwiseRotation = PumpingUnitRotationDirection.Counterclockwise;
            SampleSurfaceConfig.MotorAmpsDown = 120;
            SampleSurfaceConfig.MotorAmpsUp = 144;
            RRLMotorTypeDTO[] motorType = CatalogService.GetAllMotorTypes();
            SampleSurfaceConfig.MotorType = motorType[0];
            RRLMotorSizeDTO[] motorSize = CatalogService.GetAllMotorSizes();
            SampleSurfaceConfig.MotorSize = motorSize[0];
            RRLMotorSlipDTO[] motorSlip = CatalogService.GetAllMotorSlips();
            SampleSurfaceConfig.SlipTorque = motorSlip[0];
            SampleSurfaceConfig.WristPinPosition = 2; //is 3 in UI

            //Weights
            CrankWeightsConfigDTO SampleWeightsConfig = new CrankWeightsConfigDTO();
            POPRRLCranksDTO[] crankId = CatalogService.GetCranksByPumpingUnitPK(pumpingUnitManufacturer.Name, pumpingUnitType.AbbreviatedName, pumpingUnit.Description);
            SampleWeightsConfig.CrankId = crankId[1].CrankId;
            if (SampleWeightsConfig.CrankId != "N/A")
            {
                POPRRLCranksWeightsDTO crankCBT = CatalogService.GetCrankWeightsByCrankId(crankId[1].CrankId);
                //SampleWeightsConfig.CBT = crankCBT.CrankCBT;
                SampleWeightsConfig.TorqCalcMethod = TorqueCalculationMethod.API;
                SampleWeightsConfig.Crank_1_Auxiliary = new RRLAuxiliaryWeightDTO { LagId = crankCBT.AuxiliaryIdentifier[0], LeadId = crankCBT.AuxiliaryIdentifier[0] };
                SampleWeightsConfig.Crank_1_Primary = new RRLPrimaryWeightDTO { LagId = crankCBT.PrimaryIdentifier[0], LeadId = crankCBT.PrimaryIdentifier[0] };
                SampleWeightsConfig.Crank_2_Auxiliary = new RRLAuxiliaryWeightDTO { LagId = crankCBT.AuxiliaryIdentifier[0], LeadId = crankCBT.AuxiliaryIdentifier[0] };
                SampleWeightsConfig.Crank_2_Primary = new RRLPrimaryWeightDTO { LagDistance = crankCBT.DistanceT[0], LagId = crankCBT.PrimaryIdentifier[0], LagMDistance = crankCBT.DistanceM[0], LeadDistance = crankCBT.DistanceT[0], LeadId = crankCBT.PrimaryIdentifier[0], LeadMDistance = crankCBT.DistanceM[0] };
                //SampleWeightsConfig.PumpingUnitCrankCBT = crankCBT.PumpingUnitCrankCBT;
            }

            //Downhole
            DownholeConfigDTO SampleDownholeConfig = new DownholeConfigDTO();
            //SampleDownholeConfig.WellId = well.Id.ToString();
            SampleDownholeConfig.PumpDiameter = 1.5;
            SampleDownholeConfig.PumpDepth = 5130;
            SampleDownholeConfig.TubingID = 1.5;
            SampleDownholeConfig.TubingOD = 2.88;
            SampleDownholeConfig.TubingAnchorDepth = 5130;
            SampleDownholeConfig.CasingOD = 7.00;
            SampleDownholeConfig.CasingWeight = 32;
            SampleDownholeConfig.TopPerforation = 5100.0;
            SampleDownholeConfig.BottomPerforation = 5120.0;

            //Rods
            RodStringConfigDTO SampleRodsConfig = new RodStringConfigDTO();
            SampleRodsConfig.TotalRodLength = 5100;
            RodTaperConfigDTO[] RodTaperArray = new RodTaperConfigDTO[3];
            SampleRodsConfig.RodTapers = RodTaperArray;
            RodTaperConfigDTO Taper1 = new RodTaperConfigDTO();
            Taper1.Grade = "D";
            Taper1.Manufacturer = "Weatherford, Inc.";
            Taper1.NumberOfRods = 57;
            Taper1.RodGuid = "";
            Taper1.RodLength = 30.0;
            Taper1.ServiceFactor = 0.9;
            Taper1.Size = 1.0;
            Taper1.TaperLength = 1710;
            Taper1.RodDampingDown = 0.5;
            Taper1.RodDampingUp = 0.2;
            RodTaperArray[0] = Taper1;
            RodTaperConfigDTO Taper2 = new RodTaperConfigDTO();
            Taper2.Grade = "D";
            Taper2.Manufacturer = "Weatherford, Inc.";
            Taper2.NumberOfRods = 57;
            Taper2.RodGuid = "";
            Taper2.RodLength = 30.0;
            Taper2.ServiceFactor = 0.9;
            Taper2.Size = 0.875;
            Taper2.TaperLength = 1710;
            Taper2.RodDampingDown = 0.8;
            Taper2.RodDampingUp = 0.1;
            RodTaperArray[1] = Taper2;
            RodTaperConfigDTO Taper3 = new RodTaperConfigDTO();
            Taper3.Grade = "D";
            Taper3.Manufacturer = "Weatherford, Inc.";
            Taper3.NumberOfRods = 56;
            Taper3.RodGuid = "";
            Taper3.RodLength = 30.0;
            Taper3.ServiceFactor = 0.9;
            Taper3.Size = 0.75;
            Taper3.TaperLength = 1680;
            Taper3.RodDampingDown = 0.7;
            Taper3.RodDampingUp = 0.2;
            RodTaperArray[2] = Taper3;

            //Model File
            ModelConfigDTO SampleModel = new ModelConfigDTO();
            SampleModel.Weights = SampleWeightsConfig;
            SampleModel.Rods = SampleRodsConfig;
            SampleModel.Downhole = SampleDownholeConfig;
            SampleModel.Surface = SampleSurfaceConfig;

            #endregion model

            WellDTO wellBefore = WellService.GetWellByName(well.Name);
            Assert.AreEqual(null, wellBefore, "AddWellConfigWithTransaction failed because well '{0}' already exists.", well.Name);
            WellConfigDTO wellConfigDTO = new WellConfigDTO();
            wellConfigDTO.Well = well;
            wellConfigDTO.ModelConfig = SampleModel; // test fully configured model

            try
            {
                WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
                _wellsToRemove.Add(addedWellConfig.Well);
                AssemblyDTO getAssembly = WellboreComponentService.GetAssemblyByWellId(addedWellConfig.Well.Id.ToString());
                AssemblyComponentDTO[] getassemblycomponent = WellboreComponentService.GetAssemblyComponentsByWellId(addedWellConfig.Well.Id.ToString());
                Assert.IsNotNull(getassemblycomponent);
                foreach (AssemblyComponentDTO assemblycomponent in getassemblycomponent)
                {
                    if (assemblycomponent.Grouping == "Rod String" && assemblycomponent.PartType == "Rod")
                    {
                        RodTaperConfigDTO AddRodInfo = WellMTBFService.GetAdditionalRodDetailsforToolTip(assemblycomponent.Id.ToString());
                        Assert.IsNotNull(AddRodInfo);
                        Trace.WriteLine("Rod found " + AddRodInfo.Manufacturer + " TaperLength: " + AddRodInfo.TaperLength);
                        Assert.AreEqual("Weatherford, Inc.", AddRodInfo.Manufacturer);
                    }
                }
            }
            catch (Exception e)
            {
                Assert.Fail("Exception " + e);
            }
        }

        [TestCategory(TestCategories.WellMTBFServicesTests), TestMethod]
        public void GetRodDetailsforWellboreDiagramUOM()
        {
            int i = 1;
            string wellName;
            if (s_isRunningInATS)
            {
                wellName = "RPOC_" + (i).ToString("D5");
            }
            else
            {
                wellName = "RPOC_0252";
            }

            WellDTO[] allWellsBefore = WellService.GetAllWells();
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = "new_TestWell", FacilityId = wellName, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });

            WellDTO[] allWells = WellService.GetAllWells();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals("new_TestWell"));
            _wellsToRemove.Add(well);

            SubAssemblyDTO getSubAssemblies = WellboreComponentService.GetSubAssembliesByWellId(well.Id.ToString()).FirstOrDefault();

            //test add report
            ReportDTO newReport = new ReportDTO
            {
                WellId = well.Id,
                AssemblyId = well.AssemblyId,
                JobReasonId = 309,
                JobTypeId = 12,
                ReportTypeId = 3,
                OffDate = (DateTime.Today),
                WorkoverDate = (DateTime.Today.AddDays(5)),
                Comment = "Add Report",
            };

            WellboreComponentService.AddReport(newReport);
            ReportDTO reportExists = WellboreComponentService.GetReportByWellId(well.Id.ToString());


            //test to add Component
            ComponentDTO newComponent = new ComponentDTO
            {
                CatalogItem = -1,
                MfgCatalogItem = -1,
                ComponentUsage = -1,
                TubularConnectionSpecs_Top = -1,
                TubularConnectionSpecs_Bottom = -1,
                SurfaceCondition = 1,
                ComponentOrigin = -1,
                BusinessOrganizationId = -1,
                MfgCat_PartType = 20,
                PartType = "Rod",
                OuterDiameter = 1,
                InnerDiameter = 4,
            };

            //test add Assembly Component
            AssemblyComponentDTO newAssemblyComponent = new AssemblyComponentDTO
            {
                AssemblyId = well.AssemblyId,
                BeginEventDT = DateTime.Today.ToLocalTime(),
                BottomDepth = 100,
                DataQualityCode = 0,
                EndEventDT = new DateTime(2038, 1, 18, 0, 0, 1, DateTimeKind.Local),
                ReportId = reportExists.Id,
                ComponentCouplingType = 1,
                ComponentGrouping = 2,
                SubassemblyId = getSubAssemblies.Id,
                InstallDate = DateTime.Today,
                KnownLength = false,
                KnownTopDepth = false,
                PreviousRunDays = 0,
                Quantity = 1,
                TopDepth = 0,
                TrueVerticalDepth = 0,
                TrueVerticalDepthBottom = 0,
            };
            WellboreComponentService.AddAssemblyComponentComponent(new AssemblyComponentAndComponentPairDTO(newAssemblyComponent, newComponent));
            AssemblyComponentDTO getassemblycomponent = WellboreComponentService.GetAssemblyComponentsByWellId(well.Id.ToString()).FirstOrDefault();
            WellboreImageDTO[] RodInfo = WellMTBFService.GetRodDetailsforWellboreDiagram(well.Id.ToString(), "0", "null");
            // Set system to US units
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
            UnitsValuesCollectionDTO<WellboreImageUnitDTO, WellboreImageDTO> RodInfoNew = WellMTBFService.GetRodDetailsforWellboreDiagramUoM(well.Id.ToString(), "0", "null");
            Assert.IsNotNull(RodInfoNew);
            Assert.AreEqual("ft", RodInfoNew.Units.TopDepth.UnitKey);
            Assert.AreEqual("ft", RodInfoNew.Units.BottomDepth.UnitKey);
            UnitsValuesPairDTO<RodTaperConfigUnitDTO, RodTaperConfigDTO> RodInfoNewTooltip = WellMTBFService.GetAdditionalRodDetailsforToolTipUoM(well.Id.ToString());
            Assert.IsNotNull(RodInfoNewTooltip);
            Assert.AreEqual("ft", RodInfoNewTooltip.Units.TaperLength.UnitKey);
            Assert.AreEqual("ft", RodInfoNewTooltip.Units.RodLength.UnitKey);
            Assert.AreEqual(2, (int)RodInfoNewTooltip.Units.RodLength.Precision);
            Assert.AreEqual(2, (int)RodInfoNewTooltip.Units.TaperLength.Precision);

            ChangeUnitSystem("Metric");
            ChangeUnitSystemUserSetting("Metric");
            UnitsValuesCollectionDTO<WellboreImageUnitDTO, WellboreImageDTO> RodInfoMetric = WellMTBFService.GetRodDetailsforWellboreDiagramUoM(well.Id.ToString(), "0", "null");
            Assert.IsNotNull(RodInfoMetric);
            Assert.AreEqual("m", RodInfoMetric.Units.TopDepth.UnitKey);
            Assert.AreEqual("m", RodInfoMetric.Units.BottomDepth.UnitKey);
            UnitsValuesPairDTO<RodTaperConfigUnitDTO, RodTaperConfigDTO> RodInfoTooltipMetric = WellMTBFService.GetAdditionalRodDetailsforToolTipUoM(well.Id.ToString());
            Assert.IsNotNull(RodInfoTooltipMetric);
            Assert.AreEqual("m", RodInfoTooltipMetric.Units.TaperLength.UnitKey);
            Assert.AreEqual("m", RodInfoTooltipMetric.Units.RodLength.UnitKey);
        }

        [TestCategory(TestCategories.WellMTBFServicesTests), TestMethod]
        public void GetTubingDetailsforWellboreDiagramUOM()
        {
            int i = 1;
            string wellName;
            if (s_isRunningInATS)
            {
                wellName = "RPOC_" + (i).ToString("D5");
            }
            else
            {
                wellName = "RPOC_0252";
            }

            WellDTO[] allWellsBefore = WellService.GetAllWells();
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = "new_TestWell", FacilityId = wellName, DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });

            WellDTO[] allWells = WellService.GetAllWells();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals("new_TestWell"));
            _wellsToRemove.Add(well);

            SubAssemblyDTO getSubAssemblies = WellboreComponentService.GetSubAssembliesByWellId(well.Id.ToString()).FirstOrDefault();

            //test add report
            ReportDTO newReport = new ReportDTO
            {
                WellId = well.Id,
                AssemblyId = well.AssemblyId,
                JobReasonId = 309,
                JobTypeId = 12,
                ReportTypeId = 3,
                OffDate = (DateTime.Today),
                WorkoverDate = (DateTime.Today.AddDays(5)),
                Comment = "Add Report",
            };

            WellboreComponentService.AddReport(newReport);
            ReportDTO reportExists = WellboreComponentService.GetReportByWellId(well.Id.ToString());

            //test to add Component
            ComponentDTO newComponent = new ComponentDTO
            {
                CatalogItem = -1,
                MfgCatalogItem = -1,
                ComponentUsage = -1,
                TubularConnectionSpecs_Top = -1,
                TubularConnectionSpecs_Bottom = -1,
                SurfaceCondition = 1,
                ComponentOrigin = -1,
                BusinessOrganizationId = -1,
                MfgCat_PartType = 394,
                PartType = "Tubing",
                OuterDiameter = 4,
                InnerDiameter = 1,
            };

            //test add Assembly Component
            AssemblyComponentDTO newAssemblyComponent = new AssemblyComponentDTO
            {
                AssemblyId = well.AssemblyId,
                BeginEventDT = DateTime.Today.ToLocalTime(),
                BottomDepth = 200,
                DataQualityCode = 0,
                EndEventDT = new DateTime(2038, 1, 18, 0, 0, 1, DateTimeKind.Local),
                ReportId = reportExists.Id,
                ComponentCouplingType = 1,
                ComponentGrouping = 4,
                SubassemblyId = getSubAssemblies.Id,
                InstallDate = DateTime.Today,
                KnownLength = false,
                KnownTopDepth = false,
                PreviousRunDays = 0,
                Quantity = 1,
                TopDepth = 0,
                TrueVerticalDepth = 0,
                TrueVerticalDepthBottom = 0,
            };
            WellboreComponentService.AddAssemblyComponentComponent(new AssemblyComponentAndComponentPairDTO(newAssemblyComponent, newComponent));
            AssemblyComponentDTO getassemblycomponent = WellboreComponentService.GetAssemblyComponentsByWellId(well.Id.ToString()).FirstOrDefault();

            WellboreImageDTO[] TubingInfo = WellMTBFService.GetTubingDetailsforWellboreDiagram(well.Id.ToString(), "0", "null");
            // Set system to US units
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
            UnitsValuesCollectionDTO<WellboreImageUnitDTO, WellboreImageDTO> TubingInfoNew = WellMTBFService.GetTubingDetailsforWellboreDiagramUoM(well.Id.ToString(), "0", "null");
            Assert.IsNotNull(TubingInfoNew);
            Assert.IsNotNull(TubingInfoNew.Units.TopDepth);
            Assert.IsNotNull(TubingInfoNew.Units.FailLocation);
            Assert.IsNotNull(TubingInfoNew.Units.BottomDepth);
            Assert.AreEqual("ft", TubingInfoNew.Units.TopDepth.UnitKey);
            Assert.AreEqual("ft", TubingInfoNew.Units.BottomDepth.UnitKey);
            Assert.AreEqual(2, (int)TubingInfoNew.Units.BottomDepth.Precision);
            Assert.AreEqual(2, (int)TubingInfoNew.Units.TopDepth.Precision);

            // Set system to metric units
            ChangeUnitSystem("Metric");
            ChangeUnitSystemUserSetting("Metric");
            UnitsValuesCollectionDTO<WellboreImageUnitDTO, WellboreImageDTO> TubingInfoMetric = WellMTBFService.GetTubingDetailsforWellboreDiagramUoM(well.Id.ToString(), "0", "null");
            Assert.IsNotNull(TubingInfoMetric);
            Assert.IsNotNull(TubingInfoMetric.Units.TopDepth);
            Assert.IsNotNull(TubingInfoMetric.Units.FailLocation);
            Assert.IsNotNull(TubingInfoMetric.Units.BottomDepth);
            Assert.AreEqual("m", TubingInfoMetric.Units.TopDepth.UnitKey);
            Assert.AreEqual("m", TubingInfoMetric.Units.BottomDepth.UnitKey);
        }

        [TestCategory(TestCategories.WellMTBFServicesTests), TestMethod]
        public void DeleteComments()
        {
            WellDTO addWell = SetDefaultFluidType(new WellDTO { Name = "new_Well", FacilityId = "new_Well", DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = addWell });
            WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(addWell.Name));
            Assert.IsNotNull(addedWell);
            _wellsToRemove.Add(addedWell);
            //Add comments
            WellCommentDTO wdto = new WellCommentDTO();
            wdto.WellId = addedWell.Id;
            wdto.WellComment = "This is a comment";
            WellService.SaveComments(wdto);
            //Get the Comments added in well
            WellCommentDTO[] WellCommentsData = WellService.GetWellComments(addedWell.Id.ToString());
            Assert.IsNotNull(WellCommentsData);
            Assert.AreEqual(1, WellCommentsData.Count(), "Comment not posted while saving");
            //Delete a comment
            WellService.RemoveCommentByWellId(WellCommentsData[0].Id.ToString(), addedWell.Id.ToString());
            WellCommentDTO[] afterWellCommentsData = WellService.GetWellComments(addedWell.Id.ToString());
            Assert.AreEqual(0, afterWellCommentsData.Count(), "Error occurred while deleting a comment");
        }

        [TestCategory(TestCategories.WellMTBFServicesTests), TestMethod]
        public void GetCommentsbyDateRange()
        {
            WellDTO addWell = SetDefaultFluidType(new WellDTO { Name = "new_Well", FacilityId = "new_Well", DataConnection = GetDefaultCygNetDataConnection(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = addWell });
            WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(addWell.Name));
            Assert.IsNotNull(addedWell);
            _wellsToRemove.Add(addedWell);
            //Add comments
            WellCommentDTO wdto = new WellCommentDTO();
            wdto.WellId = addedWell.Id;
            wdto.WellComment = "This is a comment";
            for (int i = 0; i < 10; i++)
            {
                WellService.SaveComments(wdto);
            }
            //Get the Comments added in well
            CygNetTrendDTO WellCommentsData = WellMTBFService.GetWellCommentsbyDateRange(addedWell.Id.ToString(), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime()), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime()));
            Assert.IsNotNull(WellCommentsData);
            Assert.AreEqual(1, WellCommentsData.PointValues.Count(), "Comment not posted while saving");
        }

        [TestCategory(TestCategories.WellMTBFServicesTests), TestMethod]
        public void GetProductionFigures()
        {
            string facilityId;
            if (s_isRunningInATS)
            {
                facilityId = "RPOC_00001";
            }
            else
            {
                facilityId = "RPOC_0001";
            }
            try
            {
                WellDTO addedWell = AddRRLWell(facilityId);
                ProductionFiguresDTO prodFigures = WellMTBFService.GetProductionFigures(addedWell.Id.ToString());
                Assert.IsNotNull(prodFigures);
            }
            finally
            {
                RemoveWell(facilityId);
            }
        }

        public long AddWell(string facilityIdBase)
        {
            string facilityId = s_isRunningInATS ? facilityIdBase + "00001" : facilityIdBase + "0001";
            WellDTO well = SetDefaultFluidType(new WellDTO()
            {
                Name = DefaultWellName,
                FacilityId = facilityId,
                DataConnection = GetDefaultCygNetDataConnection(),
                CommissionDate = DateTime.Today.AddYears(-2),
                AssemblyAPI = "1234567890",
                SubAssemblyAPI = "123456789012",
                IntervalAPI = "12345678901234",
                WellType = WellTypeId.RRL,
                SurfaceLatitude = 29.686619m,
                SurfaceLongitude = -95.399334m
            });
            WellConfigDTO wellConfigDTO = new WellConfigDTO();
            wellConfigDTO.Well = well;
            wellConfigDTO.ModelConfig = ReturnFullyPopulatedModel();
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
            WellDTO[] wells = WellService.GetAllWells();
            wells = WellService.GetAllWells();
            WellDTO getwell = wells.FirstOrDefault(w => w.Name == well.Name);
            _wellsToRemove.Add(getwell);

            return getwell.Id;
        }
    }
}
