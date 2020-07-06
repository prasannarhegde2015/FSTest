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
    public class WellboreComponentsTests : APIClientTestBase
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

        [TestCategory(TestCategories.WellboreComponentsTests), TestMethod]
        public void AssemblyCRUD()
        {
            //Add well
            WellDTO addWell = SetDefaultFluidType(new WellDTO { Name = "Extra_Well", FacilityId = "RPOC_0016", DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.RRL });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = addWell });
            WellDTO addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(addWell.Name));
            _wellsToRemove.Add(addedWell);
            //test GetAssembly
            AssemblyDTO getAssemblies = WellboreComponentService.GetAssemblyByWellId(addedWell.Id.ToString());
            Assert.IsNotNull(getAssemblies);

            //test UpdateAssembly
            string assemblyName = "updateAssembly_" + (addedWell.Id).ToString();
            AssemblyDTO updateAssembly = WellboreComponentService.GetAssemblyByWellId(addedWell.Id.ToString());
            updateAssembly.Name = assemblyName;
            WellboreComponentService.UpdateAssembly(updateAssembly);
            Assert.AreEqual(assemblyName, updateAssembly.Name);
        }

        [TestCategory(TestCategories.WellboreComponentsTests), TestMethod]
        public void WellboreComponentCRUD()
        {
            //Add well
            WellDTO addWell = SetDefaultFluidType(new WellDTO
            {
                Name = "Extra_Well",
                FacilityId = "RPOC_0016",
                DataConnection = GetDefaultCygNetDataConnection(),
                CommissionDate = DateTime.Today.AddYears(-2),
                WellType = WellTypeId.RRL
            });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = addWell });
            WellDTO well = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(addWell.Name));
            _wellsToRemove.Add(well);

            AssemblyDTO getAssembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(getAssembly, "Failed to get newly added well assembly.");

            //test add SubAssembly
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
            Assert.IsNotNull(getSubAssemblies, "Failed to get newly added well subassembly.");

            //test add report
            ReportDTO newReport = new ReportDTO
            {
                WellId = well.Id,
                AssemblyId = getAssembly.Id,
                JobReasonId = 309,
                JobTypeId = 12,
                ReportTypeId = 3,
                OffDate = (DateTime.Today.AddDays(-5)),
                WorkoverDate = (DateTime.Today),
                Comment = "Add Report",
            };

            WellboreComponentService.AddReport(newReport);
            ReportDTO reportExists = WellboreComponentService.GetReportByWellId(well.Id.ToString());
            Assert.IsNotNull(reportExists, "Failed to get newly added report.");
            Assert.AreEqual(newReport.ReportTypeId, reportExists.ReportTypeId, "Newly added report has incorrect report type id.");

            //Test to add Component and Assembly Component
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
            AssemblyComponentDTO newAssemblyComponent = new AssemblyComponentDTO
            {
                AssemblyId = getAssembly.Id,
                BeginEventDT = DateTime.Today.ToLocalTime(),
                BottomDepth = 0,
                DataQualityCode = 0,
                EndEventDT = new DateTime(2038, 1, 18, 0, 0, 1, DateTimeKind.Local),
                //ComponentId = getcomponent,
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
            Assert.IsNotNull(getassemblycomponents, "Failed to get newly added assembly components.");
            AssemblyComponentDTO[] getactiveassemblycomponents = WellboreComponentService.GetActiveAssemblyComponentsByWellId(well.Id.ToString());
            Assert.IsNotNull(getactiveassemblycomponents, "Failed to get newly added active assembly components.");

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
                OilGravity = 0,
                PumpEfficiency = 0,
                PumpIntakePressure = 100,
                PumpingHours = 10,
                //SPTCode = 1,
                SPTCodeDescription = WellTestQuality.RepresentativeTest.ToString(),
                StrokePerMinute = 0,
                TestDuration = 3,
                Water = 75,
                WaterGravity = 0
            };
            testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(100));
            WellTestDataService.AddWellTestData(new WellTestAndUnitsDTO(units, testDataDTO));
            WellTestDTO[] wellTest = WellTestDataService.GetWellTestDataByWellId(well.Id.ToString()).Values;
            Assert.IsNotNull(wellTest, "Failed to get newly added well test.");

            //Add comments to the newly added Well
            WellCommentDTO wdto = new WellCommentDTO();
            wdto.WellId = well.Id;
            wdto.WellComment = "testing comments";
            WellService.SaveComments(wdto);
            wdto.WellComment = "testing comments two";
            WellService.SaveComments(wdto);
            WellCommentDTO[] wellCommentsExist = WellService.GetWellComments(well.Id.ToString());
            Assert.IsNotNull(wellCommentsExist, "Failed to get newly well comments.");

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
            RodDetailsDTO[] getRodDetails = WellboreComponentService.GetActiveRodDetailsByAssemblyComponents(getassemblycomponents);
            Assert.IsNotNull(getRodDetails, "Failed to get newly added rod details.");

            //Failure

            DetailAssemblyComponentFailureDTO[] list = new DetailAssemblyComponentFailureDTO[1];
            DetailAssemblyComponentFailureDTO obj = new DetailAssemblyComponentFailureDTO();

            // TODOQA: Figure out of this foreach loop has a reason for existing.
            foreach (AssemblyComponentDTO assemblyComp in getassemblycomponents)
            {
                obj.AscPrimaryKey = assemblyComp.Id;
                obj.FailureDate = Convert.ToDateTime("11/14/2017");
                obj.WorkoverDate = Convert.ToDateTime("11/20/2017");
                obj.Comments = "Failed the component";
                obj.FailedDepth = 756.00m;
                obj.FailureCorrosionType = 1;
                obj.FailureLocation = 1;
                obj.FailureObservation = 1;
                obj.CorrosionAmount = 1;
                obj.FailureCorrosionType = 1;
            }
            list[0] = obj;

            // TODOQA: Figure out if these assertions have a reason for existing.
            Assert.AreEqual(obj.Comments, "Failed the component");
            Assert.AreEqual(obj.CorrosionAmount, 1);
            Assert.AreEqual(obj.FailedDepth, 756);
            Assert.AreEqual(obj.FailureCorrosionType, 1);
            Assert.AreEqual(obj.FailureLocation, 1);
            Assert.AreEqual(obj.FailureDate, Convert.ToDateTime("11/14/2017"));

            //WellTest Data
            WellTestDTO[] wellTestExist = WellTestDataService.GetWellTestDataByWellId(well.Id.ToString()).Values;
            Assert.AreEqual(1, wellTestExist.Count());

            WellConfigurationService.RemoveWellConfig(well.Id.ToString());

            //check all records deleted
            AssemblyDTO getAssembliesAfterRemove = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNull(getAssembliesAfterRemove, "Failed to remove well assembly.");

            //Make sure rod details empty
            RodDetailsDTO[] getRodDeatilsExists = WellboreComponentService.GetActiveRodDetailsByAssemblyComponents(getassemblycomponents);
            Assert.AreEqual(0, getRodDeatilsExists.Count());

            //Make sure Comments empty
            WellCommentDTO[] commentsExist = WellService.GetWellComments(well.Id.ToString());
            Assert.AreEqual(0, commentsExist.Count());

            //SubAssemblies Data
            SubAssemblyDTO[] subAssembliesExist = WellboreComponentService.GetSubAssembliesByWellId(well.Id.ToString());
            Assert.IsNull(subAssembliesExist);

            //Assembly Component Data
            AssemblyComponentDTO[] assemblycomponentsExist = WellboreComponentService.GetAssemblyComponentsByWellId(well.Id.ToString());
            Assert.IsNull(assemblycomponentsExist);

            //Report Data
            //ReportDTO reportDetailsExists = WellboreComponentService.GetReportByWellId(well.Id.ToString());
            //Assert.IsNull(reportDetailsExists);

            AssemblyDTO assembliExist = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNull(assembliExist);

            //Check Well Deleted
            WellConfigDTO wellInfo = WellConfigurationService.GetWellConfig(well.Id.ToString());
            Assert.IsNull(wellInfo.Well);
        }

        [TestCategory(TestCategories.WellboreComponentsTests), TestMethod]
        public void ConfigModelAndWellboreComponents()
        {
            bool dcExisted = false;

            // Check for existing data connection.
            DataConnectionDTO dataConnection = GetDefaultCygNetDataConnection();
            dcExisted = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == dataConnection.ProductionDomain && dc.Site == dataConnection.Site && dc.Service == dataConnection.Service) != null;
            // Well to add later on.
            WellDTO well = SetDefaultFluidType(new WellDTO() { Name = "ConfigComp2", FacilityId = GetFacilityId("RPOC_", 1), DataConnection = dataConnection, CommissionDate = DateTime.Today, AssemblyAPI = "123456789012", SubAssemblyAPI = "ConfigComp21234567890", IntervalAPI = "ConfigComp21234567890", WellType = WellTypeId.RRL });

            WellDTO[] wells = WellService.GetAllWells();
            Assert.IsFalse(wells.Any(w => w.Name.Equals(well.Name)), "Well already exists in database.");

            //config downhole
            //Downhole
            DownholeConfigDTO sampleDownholeConfig = new DownholeConfigDTO();
            sampleDownholeConfig.PumpDiameter = 1.5;
            sampleDownholeConfig.PumpDepth = 5130;
            sampleDownholeConfig.TubingID = 1.5;
            sampleDownholeConfig.TubingOD = 2.88;
            sampleDownholeConfig.TubingAnchorDepth = 5130;
            sampleDownholeConfig.CasingOD = 7.00;
            sampleDownholeConfig.CasingWeight = 32;
            sampleDownholeConfig.TopPerforation = 5100.0;
            sampleDownholeConfig.BottomPerforation = 5120.0;
            //Rods
            RodStringConfigDTO sampleRodsConfig = new RodStringConfigDTO();
            sampleRodsConfig.TotalRodLength = 5100;
            RodTaperConfigDTO[] rodTaperArray = new RodTaperConfigDTO[3];
            sampleRodsConfig.RodTapers = rodTaperArray;
            RodTaperConfigDTO taper1 = new RodTaperConfigDTO();
            taper1.Grade = "D";
            taper1.Manufacturer = "Weatherford, Inc.";
            taper1.NumberOfRods = 57;
            taper1.RodGuid = "";
            taper1.RodLength = 30.0;
            taper1.ServiceFactor = 0.9;
            taper1.Size = 1.0;
            taper1.TaperLength = 1710;
            taper1.RodDampingDown = 0.8;
            taper1.RodDampingUp = 0.2;
            rodTaperArray[0] = taper1;
            RodTaperConfigDTO taper2 = new RodTaperConfigDTO();
            taper2.Grade = "D";
            taper2.Manufacturer = "Weatherford, Inc.";
            taper2.NumberOfRods = 57;
            taper2.RodGuid = "";
            taper2.RodLength = 30.0;
            taper2.ServiceFactor = 0.9;
            taper2.Size = 0.875;
            taper2.TaperLength = 1710;
            taper2.RodDampingDown = 0.8;
            taper2.RodDampingUp = 0.3;
            rodTaperArray[1] = taper2;
            RodTaperConfigDTO taper3 = new RodTaperConfigDTO();
            RodTaperConfigDTO taper4 = new RodTaperConfigDTO();
            taper3.Grade = "D";
            taper3.Manufacturer = "Weatherford, Inc.";
            taper3.NumberOfRods = 56;
            taper3.RodGuid = "";
            taper3.RodLength = 30.0;
            taper3.ServiceFactor = 0.9;
            taper3.Size = 0.75;
            taper3.TaperLength = 1680;
            taper3.RodDampingDown = 0.8;
            taper3.RodDampingUp = 0.1;
            rodTaperArray[2] = taper3;
            //Model File
            ModelConfigDTO sampleModel = new ModelConfigDTO();
            sampleModel.Rods = sampleRodsConfig;
            sampleModel.Downhole = sampleDownholeConfig;

            WellConfigDTO wellConfigDTO = new WellConfigDTO();
            wellConfigDTO.Well = well;
            wellConfigDTO.ModelConfig = sampleModel;

            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);

            wells = WellService.GetAllWells();
            WellDTO getwell = wells.FirstOrDefault(w => w.Name == well.Name);
            _wellsToRemove.Add(getwell);
            if (!dcExisted)
            {
                _dataConnectionsToRemove.Add(getwell.DataConnection);
            }

            //save new
            List<AssemblyComponentDTO> ascList = WellboreComponentService.GetActiveAssemblyComponentsByWellId(addedWellConfig.Well.Id.ToString()).ToList();
            List<ComponentDTO> cmcList = WellboreComponentService.GetActiveComponentsByAssemblyComponents(ascList.ToArray()).ToList();

            Assert.AreEqual(8, ascList.Count(), "Assembly component list has unexpected length.");
            Assert.AreEqual(8, cmcList.Count(), "Component list has unexpected length.");

            List<AssemblyComponentDTO> ascListRod = new List<AssemblyComponentDTO>();
            for (int i = 0; i < ascList.Count(); i++)
            {
                if (cmcList[i].PartType == "Rod")
                {
                    ascListRod.Add(ascList[i]);
                }
                else if (cmcList[i].PartType == "Tubing Anchor/Catcher" || cmcList[i].PartType == "Pump (Downhole)")
                {
                    Assert.AreEqual(5130, ascList[i].TopDepth, $"Top depth mismatch for ({i}) {cmcList[i].PartType}.");
                    Assert.AreEqual(5130, ascList[i].BottomDepth, $"Bottom depth mismatch for ({i}) {cmcList[i].PartType}.");
                }
                else if (cmcList[i].PartType == "Tubing")
                {
                    Assert.AreEqual(2.88m, ascList[i].OuterDiameter, $"Outer diameter mismatch for ({i}) {cmcList[i].PartType}.");
                    Assert.AreEqual(1.5m, ascList[i].InnerDiameter, $"Inner diameter mismatch for ({i}) {cmcList[i].PartType}.");
                    Assert.AreEqual(5130, ascList[i].Length, $"Length mistmatch for ({i}) {cmcList[i].PartType}.");
                }
                else if (cmcList[i].PartType == "Casing/Casing Liner")
                {
                    Assert.AreEqual(7.000m, ascList[i].OuterDiameter, $"Outer diameter mismatch for ({i}) {cmcList[i].PartType}.");
                    Assert.AreEqual(32.000m, ascList[i].TotalWeight, $"Total weight mismatch for ({i}) {cmcList[i].PartType}.");
                }
                else if (cmcList[i].PartType == "Wellbore Completion Detail (Perforations, etc.)")
                {
                    Assert.AreEqual(20, ascList[i].Length, $"Length mistmatch for ({i}) {cmcList[i].PartType}.");
                }
            }

            List<RodDetailsDTO> rods = WellboreComponentService.GetActiveRodDetailsByAssemblyComponents(ascListRod.ToArray()).ToList();
            for (int idx = 0; idx < rodTaperArray.Length; idx++)
            {
                Assert.AreEqual(rodTaperArray[idx].Grade, rods[idx].Grade, $"Grade mismatch for rod taper {idx + 1}.");
                Assert.AreEqual(rodTaperArray[idx].Manufacturer, rods[idx].Manufacturer, $"Manufacturer mismatch for rod taper {idx + 1}.");
                Assert.AreEqual(rodTaperArray[idx].NumberOfRods, rods[idx].NumberOfRods, $"Number of rods mismatch for rod taper {idx + 1}.");
                Assert.AreEqual(rodTaperArray[idx].RodLength, rods[idx].LengthPerRod, $"Length per rod mismatch for rod taper {idx + 1}.");
            }

            rods.Clear();

            //update 1, change downhole, add rod taper
            //Downhole
            sampleDownholeConfig.PumpDiameter = 1.59;
            sampleDownholeConfig.PumpDepth = 5930;
            sampleDownholeConfig.TubingID = 1.59;
            sampleDownholeConfig.TubingOD = 2.89;
            sampleDownholeConfig.TubingAnchorDepth = 5930;
            sampleDownholeConfig.CasingOD = 7.09;
            sampleDownholeConfig.CasingWeight = 39;
            sampleDownholeConfig.TopPerforation = 5109.0;
            sampleDownholeConfig.BottomPerforation = 5129.0;
            //Rods
            rodTaperArray = new RodTaperConfigDTO[4];
            sampleRodsConfig.RodTapers = rodTaperArray;
            sampleRodsConfig.TotalRodLength = 5900;
            taper1.Grade = "N-97";
            taper1.Manufacturer = "Alberta Oil Tools";
            taper1.NumberOfRods = 57;
            taper1.RodGuid = "";
            taper1.RodLength = 30.0;
            taper1.ServiceFactor = 0.9;
            taper1.Size = 1.5;
            taper1.TaperLength = 1710;
            taper1.RodDampingDown = 0.2;
            taper1.RodDampingUp = 0.6;
            rodTaperArray[0] = taper1;
            taper2.Grade = "D";
            taper2.Manufacturer = "Weatherford, Inc.";
            taper2.NumberOfRods = 57;
            taper2.RodGuid = "";
            taper2.RodLength = 30.0;
            taper2.ServiceFactor = 0.9;
            taper2.Size = 0.875;
            taper2.TaperLength = 1710;
            taper2.RodDampingDown = 0.8;
            taper2.RodDampingUp = 0.2;
            rodTaperArray[1] = taper2;
            taper3.Grade = "D";
            taper3.Manufacturer = "Weatherford, Inc.";
            taper3.NumberOfRods = 56;
            taper3.RodGuid = "";
            taper3.RodLength = 30.0;
            taper3.ServiceFactor = 0.9;
            taper3.Size = 0.75;
            taper3.TaperLength = 1680;
            taper3.RodDampingDown = 0.8;
            taper3.RodDampingUp = 0.2;
            rodTaperArray[2] = taper3;
            taper4.Grade = "D";
            taper4.Manufacturer = "Weatherford, Inc.";
            taper4.NumberOfRods = 20;
            taper4.RodGuid = "";
            taper4.RodLength = 40.0;
            taper4.ServiceFactor = 0.9;
            taper4.Size = 0.75;
            taper4.TaperLength = 1680;
            taper4.RodDampingDown = 0.8;
            taper4.RodDampingUp = 0.2;
            rodTaperArray[3] = taper4;

            sampleModel.Rods = sampleRodsConfig;
            sampleModel.Downhole = sampleDownholeConfig;

            addedWellConfig.ModelConfig = sampleModel;

            WellConfigurationService.UpdateWellConfig(addedWellConfig);

            ascList = WellboreComponentService.GetAssemblyComponentsByWellId(addedWellConfig.Well.Id.ToString()).ToList();
            cmcList = WellboreComponentService.GetComponentsByWellId(addedWellConfig.Well.Id.ToString()).ToList();

            Assert.AreEqual(9, ascList.Count(), "Assembly component list has unexpected length.");
            Assert.AreEqual(9, cmcList.Count(), "Component list has unexpected length.");

            ascListRod = new List<AssemblyComponentDTO>();
            for (int i = 0; i < ascList.Count(); i++)
            {
                if (cmcList[i].PartType == "Rod")
                {
                    ascListRod.Add(ascList[i]);
                }
                else if (cmcList[i].PartType == "Tubing Anchor/Catcher" || cmcList[i].PartType == "Pump (Downhole)")
                {
                    Assert.AreEqual(5930, ascList[i].TopDepth, $"Top depth mismatch for ({i}) {cmcList[i].PartType}.");
                    Assert.AreEqual(5930, ascList[i].BottomDepth, $"Bottom depth mismatch for ({i}) {cmcList[i].PartType}.");
                }
                else if (cmcList[i].PartType == "Tubing")
                {
                    Assert.AreEqual(2.89m, ascList[i].OuterDiameter, $"Outer diameter mismatch for ({i}) {cmcList[i].PartType}.");
                    Assert.AreEqual(1.59m, ascList[i].InnerDiameter, $"Inner diameter mismatch for ({i}) {cmcList[i].PartType}.");
                    Assert.AreEqual(5930, ascList[i].Length, $"Length mistmatch for ({i}) {cmcList[i].PartType}.");
                }
                else if (cmcList[i].PartType == "Casing/Casing Liner")
                {
                    Assert.AreEqual(7.09m, ascList[i].OuterDiameter, $"Outer diameter mismatch for ({i}) {cmcList[i].PartType}.");
                    Assert.AreEqual(39m, ascList[i].TotalWeight, $"Total weight mismatch for ({i}) {cmcList[i].PartType}.");
                }
                else if (cmcList[i].PartType == "Wellbore Completion Detail (Perforations, etc.)")
                {
                    Assert.AreEqual(20, ascList[i].Length, $"Length mistmatch for ({i}) {cmcList[i].PartType}.");
                }
            }
            rods = WellboreComponentService.GetActiveRodDetailsByAssemblyComponents(ascListRod.ToArray()).ToList();
            for (int idx = 0; idx < rodTaperArray.Length; idx++)
            {
                Assert.AreEqual(rodTaperArray[idx].Grade, rods[idx].Grade, $"Grade mismatch for rod taper {idx + 1}.");
                Assert.AreEqual(rodTaperArray[idx].Manufacturer, rods[idx].Manufacturer, $"Manufacturer mismatch for rod taper {idx + 1}.");
                Assert.AreEqual(rodTaperArray[idx].NumberOfRods, rods[idx].NumberOfRods, $"Number of rods mismatch for rod taper {idx + 1}.");
                Assert.AreEqual(rodTaperArray[idx].RodLength, rods[idx].LengthPerRod, $"Length per rod mismatch for rod taper {idx + 1}.");
            }

            rods.Clear();

            //update 2, remove rod taper
            //Downhole
            sampleDownholeConfig.PumpDiameter = 1.59;
            sampleDownholeConfig.PumpDepth = 3420;
            sampleDownholeConfig.TubingID = 1.59;
            sampleDownholeConfig.TubingOD = 2.89;
            sampleDownholeConfig.TubingAnchorDepth = 3400;
            sampleDownholeConfig.CasingOD = 7.09;
            sampleDownholeConfig.CasingWeight = 39;
            sampleDownholeConfig.TopPerforation = 3000;
            sampleDownholeConfig.BottomPerforation = 3400;
            //Rods
            rodTaperArray = new RodTaperConfigDTO[2];
            sampleRodsConfig.RodTapers = rodTaperArray;
            sampleRodsConfig.TotalRodLength = 3420;
            taper1.Grade = "N-97";
            taper1.Manufacturer = "Alberta Oil Tools";
            taper1.NumberOfRods = 57;
            taper1.RodGuid = "";
            taper1.RodLength = 30.0;
            taper1.ServiceFactor = 0.9;
            taper1.Size = 1.5;
            taper1.TaperLength = 1710;
            taper1.RodDampingDown = 0.8;
            taper1.RodDampingUp = 0.2;
            rodTaperArray[0] = taper1;
            taper2.Grade = "D";
            taper2.Manufacturer = "Weatherford, Inc.";
            taper2.NumberOfRods = 57;
            taper2.RodGuid = "";
            taper2.RodLength = 30.0;
            taper2.ServiceFactor = 0.9;
            taper2.Size = 0.875;
            taper2.TaperLength = 1710;
            taper2.RodDampingDown = 0.8;
            taper2.RodDampingUp = 0.1;
            rodTaperArray[1] = taper2;

            sampleModel.Rods = sampleRodsConfig;
            sampleModel.Downhole = sampleDownholeConfig;

            addedWellConfig.ModelConfig = sampleModel;

            WellConfigurationService.UpdateWellConfig(addedWellConfig);

            ascList = WellboreComponentService.GetAssemblyComponentsByWellId(addedWellConfig.Well.Id.ToString()).ToList();
            cmcList = WellboreComponentService.GetComponentsByWellId(addedWellConfig.Well.Id.ToString()).ToList();

            Assert.AreEqual(7, ascList.Count(), "Assembly component list has unexpected length.");
            Assert.AreEqual(7, cmcList.Count(), "Component list has unexpected length.");

            ascListRod = new List<AssemblyComponentDTO>();
            for (int i = 0; i < ascList.Count(); i++)
            {
                if (cmcList[i].PartType == "Rod" && ascList[i].EndEventDT != DateTime.Today)
                {
                    ascListRod.Add(ascList[i]);
                }
            }
            rods = WellboreComponentService.GetActiveRodDetailsByAssemblyComponents(ascListRod.ToArray()).ToList();
            for (int idx = 0; idx < rodTaperArray.Length; idx++)
            {
                Assert.AreEqual(rodTaperArray[idx].Grade, rods[idx].Grade, $"Grade mismatch for rod taper {idx + 1}.");
                Assert.AreEqual(rodTaperArray[idx].Manufacturer, rods[idx].Manufacturer, $"Manufacturer mismatch for rod taper {idx + 1}.");
                Assert.AreEqual(rodTaperArray[idx].NumberOfRods, rods[idx].NumberOfRods, $"Number of rods mismatch for rod taper {idx + 1}.");
                Assert.AreEqual(rodTaperArray[idx].RodLength, rods[idx].LengthPerRod, $"Length per rod mismatch for rod taper {idx + 1}.");
            }

            rods.Clear();

            //update 3, remove all
            //Downhole
            sampleDownholeConfig.PumpDiameter = 0;
            sampleDownholeConfig.PumpDepth = 0;
            sampleDownholeConfig.TubingID = 0;
            sampleDownholeConfig.TubingOD = 0;
            sampleDownholeConfig.TubingAnchorDepth = 0;
            sampleDownholeConfig.CasingOD = 0;
            sampleDownholeConfig.CasingWeight = 0;
            sampleDownholeConfig.TopPerforation = 0;
            sampleDownholeConfig.BottomPerforation = 0;
            //Rods
            RodTaperConfigDTO[] RodTaperArray3 = new RodTaperConfigDTO[0];
            sampleRodsConfig.RodTapers = RodTaperArray3;
            sampleModel.Rods = sampleRodsConfig;
            sampleModel.Downhole = sampleDownholeConfig;

            addedWellConfig.ModelConfig = sampleModel;

            WellConfigurationService.UpdateWellConfig(addedWellConfig);

            Assert.AreEqual(WellboreComponentService.GetAssemblyComponentsByWellId(addedWellConfig.Well.Id.ToString()), null);
            Assert.AreEqual(WellboreComponentService.GetComponentsByWellId(addedWellConfig.Well.Id.ToString()), null);
        }

        //Adding Workover for creating new Assembly Component and then Failing it
        [TestCategory(TestCategories.WellboreComponentsTests), TestMethod]
        public void Failure()
        {
            //Add well
            WellDTO addWell = SetDefaultFluidType(new WellDTO { Name = "Extra_Well", FacilityId = "RPOC_0016", DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.RRL });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = addWell });
            WellDTO well = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals(addWell.Name));
            _wellsToRemove.Add(well);

            //test GetAssembly
            AssemblyDTO getAssemblies = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(getAssemblies);

            //test add SubAssembly
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

            //test UpdateSubAssembly
            getSubAssemblies.SubAssemblyDesc = "subassemblydesc";
            WellboreComponentService.UpdateSubAssembly(getSubAssemblies);
            Assert.AreEqual("subassemblydesc", getSubAssemblies.SubAssemblyDesc);

            //test add report with No Failure workover
            ReportDTO newReport = new ReportDTO
            {
                WellId = well.Id,
                AssemblyId = getAssembly.Id,
                JobReasonId = 309,
                JobTypeId = 12,
                ReportTypeId = 3,
                OffDate = (DateTime.Today.AddDays(-15)),
                WorkoverDate = (DateTime.Today.AddDays(-5)),
                Comment = "No Failure",
            };

            WellboreComponentService.AddReport(newReport);
            ReportDTO reportExists = WellboreComponentService.GetReportByAssemblyId(getAssembly.Id.ToString());
            Assert.IsNotNull(reportExists);
            Assert.AreEqual(reportExists.ReportTypeId, 3);

            //test Finalize Workover without Failure
            ReportDTO finalize = new ReportDTO
            {
                Id = reportExists.Id,
                Comment = "Test Failure",
                OnDate = (DateTime.Today.AddDays(1))
            };
            ReportDTO dtoNew = WellboreComponentService.FinalizeWorkover(finalize);
            Assert.AreEqual(dtoNew.OnDate, finalize.OnDate);

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
            AssemblyComponentDTO newAssemblyComponent = new AssemblyComponentDTO
            {
                AssemblyId = getAssembly.Id,
                BeginEventDT = (DateTime.Today.ToLocalTime().AddDays(-5)),
                BottomDepth = 0,
                DataQualityCode = 0,
                EndEventDT = new DateTime(2038, 1, 18, 0, 0, 1, DateTimeKind.Local),
                //ComponentId = getcomponent,
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

            RodDetailsDTO[] rodDeatils = WellboreComponentService.GetActiveRodDetailsByAssemblyComponents(getassemblycomponents);
            Assert.AreEqual("750N", rodDeatils[0].Grade);
            Assert.AreEqual(2, rodDeatils[0].NumberOfRods);
            Assert.AreEqual(3, rodDeatils[0].Guides);
            rodDeatils[0].Guides = 2;

            WellboreComponentService.UpdateRodDetails(rodDeatils[0]);

            RodDetailsDTO[] getrodDeatils = WellboreComponentService.GetActiveRodDetailsByAssemblyComponents(getassemblycomponents);
            Assert.AreEqual(2, getrodDeatils[0].Guides);

            DetailAssemblyComponentFailureDTO[] list = new DetailAssemblyComponentFailureDTO[1];
            DetailAssemblyComponentFailureDTO obj = new DetailAssemblyComponentFailureDTO();
            foreach (AssemblyComponentDTO assemblyComp in getassemblycomponents)
            {
                obj.AscPrimaryKey = assemblyComp.Id;
                obj.FailureDate = (DateTime.Today.AddDays(-1));
                obj.WorkoverDate = (DateTime.Today);
                obj.Comments = "Failed the component";
                obj.FailedDepth = 756.00m;
                obj.FailureCorrosionType = 1;
                obj.FailureLocation = 1;
                obj.FailureObservation = 1;
                obj.CorrosionAmount = 1;
                obj.FailureCorrosionType = 1;
            }
            list[0] = obj;

            //test add report with Failure workover
            ReportDTO FailReport = new ReportDTO
            {
                WellId = well.Id,
                AssemblyId = getAssembly.Id,
                JobReasonId = 309,
                JobTypeId = 12,
                ReportTypeId = 2,
                OffDate = (DateTime.Today.AddDays(-1)),
                WorkoverDate = (DateTime.Today),
                Comment = "Yes Failure",
                FailedComponents = list
            };

            WellboreComponentService.AddReport(FailReport);

            ReportDTO reportExistsF = WellboreComponentService.GetReportByWellId(well.Id.ToString());
            Assert.IsNotNull(reportExistsF);
            Assert.AreEqual(2, reportExistsF.ReportTypeId);
            FailureDataHandlingDTO ListforDropdown = WellboreComponentService.GetListforDropdown(newComponent.MfgCat_PartType.ToString());
            Assert.IsNotNull(ListforDropdown);
            FailureDataHandlingDTO ListforWorkoverDropdown = WellboreComponentService.GetListforWorkoverDropdown();
            Assert.IsNotNull(ListforWorkoverDropdown);

            //test Finalize Workover withFailure
            ReportDTO finalizeF = new ReportDTO
            {
                Id = reportExistsF.Id,
                Comment = "Test Failure",
                OnDate = (DateTime.Today.AddDays(5))
            };
            ReportDTO dtoNewF = WellboreComponentService.FinalizeWorkover(finalizeF);
            Assert.AreEqual(dtoNewF.OnDate, finalizeF.OnDate);
            WellboreDTO wellboredata = WellboreComponentService.GetWellboreComponentData(well.Id.ToString());
            Assert.AreEqual(1, wellboredata.IsFinalized);

            //test AddUpdateFailureInfo
            DetailAssemblyComponentFailureDTO objInPut = new DetailAssemblyComponentFailureDTO();
            foreach (AssemblyComponentDTO assemblyComp in getassemblycomponents)
            {
                DetailAssemblyComponentFailureDTO objInput = WellboreComponentService.GetFailedComponent(assemblyComp.Id.ToString());
                objInput.Comments = "Failure Update";
                WellboreComponentService.AddUpdateFailureInfo(objInput);
                DetailAssemblyComponentFailureDTO objOutput = WellboreComponentService.GetFailedComponent(assemblyComp.Id.ToString());
                Assert.AreEqual("Failure Update", objOutput.Comments);
            }
        }

        public ModelConfigDTO ReturnPartialPopulatedModel()
        {
            //Empty Surface
            SurfaceConfigDTO SampleSurfaceConfig = new SurfaceConfigDTO();
            PumpingUnitDTO pumpingUnit = new PumpingUnitDTO() { Description = null, Discriminator = null, Manufacturer = null, Type = "", ABDimD = 0, ABDimF = 0, ABDimH = 0, APIA = 0, APIC = 0, APII = 0, APIK = 0, APIP = 0, AbbreviatedManufacturerName = null, GearBoxRating = 0, Id = 0, MaxStrokeLength = 0, NumberOfWristPins = 0, PhaseAngle = 0, RotationDirection = 0, StructUnbalance = null, StructureRating = 0 };
            SampleSurfaceConfig.ActualStrokeLength = 0;
            SampleSurfaceConfig.PumpingUnit = pumpingUnit;
            SampleSurfaceConfig.PumpingUnitType = null;
            SampleSurfaceConfig.ClockwiseRotation = PumpingUnitRotationDirection.NotApplicable;
            SampleSurfaceConfig.MotorAmpsDown = 0;
            SampleSurfaceConfig.MotorAmpsUp = 0;
            SampleSurfaceConfig.MotorType = new RRLMotorTypeDTO() { Id = 0, Name = "Nema B Electric" };
            SampleSurfaceConfig.MotorSize = new RRLMotorSizeDTO(0, 0);
            SampleSurfaceConfig.SlipTorque = new RRLMotorSlipDTO() { Id = 0, Rating = 0 };
            SampleSurfaceConfig.WristPinPosition = 0;

            //Empty Weights
            CrankWeightsConfigDTO SampleWeightsConfig = new CrankWeightsConfigDTO();
            SampleWeightsConfig.CrankId = "";
            SampleWeightsConfig.CBT = 0;
            SampleWeightsConfig.TorqCalcMethod = TorqueCalculationMethod.Mills;
            SampleWeightsConfig.Crank_1_Primary = new RRLPrimaryWeightDTO() { LagDistance = 0, LagId = "None", LagMDistance = 0, LagWeight = 0, LeadDistance = 0, LeadId = "None", LeadMDistance = 0, LeadWeight = 0 };
            SampleWeightsConfig.Crank_2_Primary = new RRLPrimaryWeightDTO() { LagDistance = 0, LagId = "None", LagMDistance = 0, LagWeight = 0, LeadDistance = 0, LeadId = "None", LeadMDistance = 0, LeadWeight = 0 };
            SampleWeightsConfig.Crank_1_Auxiliary = new RRLAuxiliaryWeightDTO() { LagId = "None", LagWeight = 0, LeadId = "None", LeadWeight = 0 };
            SampleWeightsConfig.Crank_2_Auxiliary = new RRLAuxiliaryWeightDTO() { LagId = "None", LagWeight = 0, LeadId = "None", LeadWeight = 0 };
            SampleWeightsConfig.PumpingUnitCrankCBT = 0;

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
            Taper1.RodDampingDown = 0.8;
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
            Taper3.RodDampingDown = 0.8;
            Taper3.RodDampingUp = 0.3;
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

        public void AddWorkover_RRL(long wellId)
        {
            WellDTO well = WellService.GetWell(wellId.ToString());
            //Add Workover with No Completion Date
            WellboreDTO wellComponents = WellboreComponentService.GetWellboreComponentData(well.Id.ToString());
            if (wellComponents != null)
            {
                var rod = wellComponents.WellboreGrid.FirstOrDefault(x => x.Grouping == "Rod String").Wellboredata.FirstOrDefault(x => x.Name == "PCP-W  - D  - 1.000 ");
                List<DetailAssemblyComponentFailureDTO> listDetails = new List<DetailAssemblyComponentFailureDTO>();
                DetailAssemblyComponentFailureDTO details = new DetailAssemblyComponentFailureDTO();
                details.AscPrimaryKey = rod.Id;
                details.Comments = "Rod Failure";
                details.CorrosionAmount = WellboreComponentService.GetListforDropdown(rod.PartTypeId.ToString()).CorrosionAmount.FirstOrDefault(x => x.Severity == "Moderate").Id;
                details.FailedDepth = 525m;
                details.FailureCorrosionType = WellboreComponentService.GetListforDropdown(rod.PartTypeId.ToString()).CorrosionType.FirstOrDefault(x => x.CorrosionType == "Bacteria").Id;
                details.FailureLocation = WellboreComponentService.GetListforDropdown(rod.PartTypeId.ToString()).FailureLocation.FirstOrDefault(x => x.Location == "Coupling").Id;
                details.FailureObservation = WellboreComponentService.GetListforDropdown(rod.PartTypeId.ToString()).FailureObservation.FirstOrDefault(x => x.Observation == "Corrosion").Id;
                details.FailureDate = well.CommissionDate.Value.AddDays(30);
                details.WorkoverDate = well.CommissionDate.Value.AddDays(45);
                listDetails.Add(details);

                ReportDTO failureReport = new ReportDTO();
                failureReport.ReportTypeId = (int)ReportTypes.Failure;
                failureReport.JobTypeId = WellboreComponentService.GetListforWorkoverDropdown().JobType.FirstOrDefault(x => x.JobType == "Artificial Lift Revision").id;
                failureReport.JobReasonId = WellboreComponentService.GetListforWorkoverDropdown().JobReason.FirstOrDefault(x => x.JobReason == "Convert to ESP").Id;
                failureReport.WellId = well.Id;
                failureReport.Comment = "Create Failure Report";
                failureReport.OffDate = well.CommissionDate.Value.AddDays(30);
                failureReport.WorkoverDate = well.CommissionDate.Value.AddDays(45);
                failureReport.FailedComponents = listDetails.ToArray();
                WellboreComponentService.AddReport(failureReport);

                //Get Failed Component
                DetailAssemblyComponentFailureDTO FailedComponent = WellboreComponentService.GetFailedComponent(rod.Id.ToString());
                Assert.IsNotNull(FailedComponent);
                Assert.AreEqual(details.Comments, FailedComponent.Comments);
                Assert.AreEqual(details.CorrosionAmount, FailedComponent.CorrosionAmount);
                Assert.AreEqual(details.FailedDepth, FailedComponent.FailedDepth);
                Assert.AreEqual(details.FailureCorrosionType, FailedComponent.FailureCorrosionType);
                Assert.AreEqual(details.FailureLocation, FailedComponent.FailureLocation);
                Assert.AreEqual(details.FailureObservation, FailedComponent.FailureObservation);
                Assert.AreEqual(details.FailureDate, FailedComponent.FailureDate);
            }
            else
                Trace.WriteLine(string.Format("Failed to retrieve wellbore components"));
        }

        [TestCategory(TestCategories.WellboreComponentsTests), TestMethod]
        public void GetFailedComponent()
        {
            long wellId = AddWell("RPOC_");
            AddWorkover_RRL(wellId);
            ReportDTO FailureReport = WellboreComponentService.GetReportByWellId(wellId.ToString());
            Assert.IsNotNull(FailureReport);
            Assert.IsNull(FailureReport.OnDate);
            Assert.AreEqual(2, FailureReport.ReportTypeId);

            //On date for the Failure
            ReportDTO finalize = new ReportDTO
            {
                Id = FailureReport.Id,
                Comment = "Rod Failure",
                OnDate = FailureReport.WorkoverDate
            };
            FailureReport = WellboreComponentService.FinalizeWorkover(finalize);
            Assert.AreEqual(FailureReport.OnDate, finalize.OnDate);
        }

        //To get Alarm type
        private static AlarmTypeDTO GetNamedAlarmType(IEnumerable<AlarmTypeDTO> alarmTypes, string alarmTypeName)
        {
            AlarmTypeDTO alarmType = alarmTypes.FirstOrDefault(at => at.AlarmType.Equals(alarmTypeName));
            Assert.IsNotNull(alarmType, "Failed to get alarm type '{0}'.", alarmTypeName);
            return alarmType;
        }

        [TestCategory(TestCategories.WellboreComponentsTests), TestMethod]
        public void GetAssemblyByWellboreIdTest()
        {
            //Creating 2 wells having unique wellbore,Borehole & perforation Interval Id
            //Creating 1st well
            WellDTO well1 = SetDefaultFluidType(new WellDTO() { Name = "Well1", FacilityId = GetFacilityId("ESPWELL_", 1), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.ESP, IntervalAPI = "IntervalAPI1", SubAssemblyAPI = "SubAssemblyAPI1", AssemblyAPI = "AssemblyAPI1", CommissionDate = DateTime.Today });
            WellConfigDTO well1_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well1 });
            Trace.WriteLine("Well1 created successfully");
            //Adding created wells in _wellsToRemove list so that once test is completed , created wells will be removed from the database
            _wellsToRemove.Add(well1_Config.Well);

            //Creating 2nd well
            WellDTO well2 = SetDefaultFluidType(new WellDTO() { Name = "Well2", FacilityId = GetFacilityId("ESPWELL_", 2), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.ESP, IntervalAPI = "IntervalAPI2", SubAssemblyAPI = "SubAssemblyAPI2", AssemblyAPI = "AssemblyAPI2", CommissionDate = DateTime.Today });
            WellConfigDTO well2_Config = WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well2 });
            Trace.WriteLine("Well2 created successfully");
            _wellsToRemove.Add(well2_Config.Well);

            Trace.WriteLine("Starting verification of GetAssemblyById API for each well");
            foreach (WellDTO well in _wellsToRemove)
            {
                //Calling GetAssemblyById API by providing Assembly Id of each well
                AssemblyDTO resAssembly = WellboreComponentService.GetAssemblyById(well.AssemblyAPI);
                Trace.WriteLine("");

                Trace.WriteLine("Verification started for " + well.Name);
                Assert.AreEqual(well.AssemblyAPI, resAssembly.Name, "AssemblyAPI retreived is not matching");
                Trace.WriteLine("Assembly API Name " + well.AssemblyAPI);
                Assert.AreEqual(well.AssemblyId, resAssembly.Id, "AssemblyId retreived is not matching");
                Trace.WriteLine("Assembly Id is " + well.AssemblyId);

            }

            Trace.WriteLine("Verifying GetAssemblyByID should return Null if it doesn't find anything");
            AssemblyDTO resAssembly1 = WellboreComponentService.GetAssemblyById("UnknownAssemblyAPI");
            Assert.IsNull(resAssembly1, "GetAssemblyById should return null if provided AssemblyAPI is not present");

            Trace.WriteLine("Verification of GetAssemblyById API for each well is completed");
        }
    }
}
