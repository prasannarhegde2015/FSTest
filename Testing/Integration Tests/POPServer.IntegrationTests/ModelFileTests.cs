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
    public class ModelFileTests : APIClientTestBase
    {
        private List<string> _modelFilesToRemove;

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            _modelFilesToRemove = new List<string>();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            foreach (var modelFileId in _modelFilesToRemove)
            {
                ModelFileService.RemoveModelFileOptionByModelFileId(modelFileId);
                ModelFileService.RemoveModelFile(modelFileId);
            }

            base.Cleanup();
        }

        private static void CompareModelFiles(ModelFileDTO expected, ModelFileDTO actual)
        {
            Assert.AreEqual(expected.ApplicableDate, actual.ApplicableDate, "Applicable date has unexpected value.");
            CollectionAssert.AreEquivalent(expected.Contents, actual.Contents, "Contents has unexpected value.");
            Assert.AreNotEqual(0, actual.Id, "Model file id should not be zero.");
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void AddGetCurrentDeleteGetCurrent()
        {
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today.AddYears(-2), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.RRL }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            WellDTO wellCompare = WellService.GetWell(well.Id.ToString());
            Assert.IsNotNull(wellCompare, "Failed to get added well.");
            Assert.AreEqual(well.Name, wellCompare.Name, "Well name has unexpected value.");

            var modelFile = new ModelFileDTO();
            modelFile.WellId = well.Id;
            modelFile.ApplicableDate = Truncate(DateTime.UtcNow.AddDays(-1));
            modelFile.Contents = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            ModelFileService.AddModelFile(modelFile);

            ModelFileDTO modelCompare = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(modelCompare, "Failed to get added model file.");
            _modelFilesToRemove.Add(modelCompare.Id.ToString());
            CompareModelFiles(modelFile, modelCompare);

            ModelFileHeaderDTO header = ModelFileService.GetCurrentModelHeader(well.Id.ToString());
            Assert.IsNotNull(header, "Failed to get header.");
            Assert.AreEqual(modelCompare.Id, header.Id, "Header id has unexpected value.");
            Assert.AreEqual(modelCompare.ApplicableDate, header.ApplicableDate, "Header applicable date has unexpected value.");
            Assert.AreEqual(modelCompare.WellId, header.WellId, "Header well id has unexpected value.");
            Assert.AreEqual(modelCompare.ChangeDate, header.ChangeDate, "Header change date has unexpected value.");
            Assert.AreEqual(modelCompare.ChangeUser, header.ChangeUser, "Header change user has unexpected value.");

            ModelFileService.RemoveModelFile(modelCompare.Id.ToString());
            modelFile = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNull(modelFile);

            _modelFilesToRemove.Remove(modelCompare.Id.ToString());
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void AddGetCurrentUpdateDeleteGetCurrent()
        {
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today.AddYears(-2), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.RRL }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            WellDTO wellCompare = WellService.GetWell(well.Id.ToString());
            Assert.IsNotNull(wellCompare);
            Assert.AreEqual(well.Name, wellCompare.Name);

            var modelFile = new ModelFileDTO();
            modelFile.WellId = well.Id;
            modelFile.ApplicableDate = Truncate(DateTime.UtcNow.AddDays(-1));
            modelFile.Contents = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            ModelFileService.AddModelFile(modelFile);

            ModelFileDTO modelCompare = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(modelCompare);
            _modelFilesToRemove.Add(modelCompare.Id.ToString());
            CompareModelFiles(modelFile, modelCompare);

            modelFile = modelCompare;
            modelFile.Contents = new byte[] { 0x23, 0x58, 0x34, 0x37, 0x78, 0x32, 0x12, 0x89 };
            ModelFileService.UpdateModelFile(modelFile);
            modelCompare = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(modelCompare);
            CompareModelFiles(modelFile, modelCompare);

            modelFile = modelCompare;
            modelFile.Contents = new byte[] { 0x17, 0x72, 0x31, 0x07, 0x58, 0x12, 0x16, 0x09, 0x73, 0x78 };
            modelFile.ApplicableDate = modelFile.ApplicableDate.AddDays(-10);
            ModelFileService.UpdateModelFile(modelFile);
            modelCompare = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(modelCompare);
            CompareModelFiles(modelFile, modelCompare);

            ModelFileService.RemoveModelFile(modelCompare.Id.ToString());
            modelFile = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNull(modelFile);

            _modelFilesToRemove.Remove(modelCompare.Id.ToString());
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void GetCommonModelConfig()
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            Tuple<string, WellTypeId, ModelFileOptionDTO>[] models = { Tuple.Create("WellfloGasLiftExample1.wflx", WellTypeId.GLift, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.CalculateChokeD_Factor) } }),
                Tuple.Create("WellfloGasInjectionExample1.wflx", WellTypeId.GInj, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.PIAndLFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.CalculateChokeD_Factor) } }),
                Tuple.Create("WellfloESPExample1.wflx", WellTypeId.ESP, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.ReservoirPressure, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.PumpWearFactor) } }),
                Tuple.Create("WellfloNFWExample1.wflx", WellTypeId.NF, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.DarcyFlowCoefficient, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.CalculateChokeD_Factor) } }),
                Tuple.Create("WellfloWaterInjectionExample1.wflx", WellTypeId.WInj, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.InjectivityIndexAndLFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.CalculateChokeD_Factor), ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.UpdateWCT_WGR) } }) };
            foreach (Tuple<string, WellTypeId, ModelFileOptionDTO> modelInfo in models)
            {
                string model = modelInfo.Item1;
                WellTypeId wellType = modelInfo.Item2;
                ModelFileOptionDTO options = modelInfo.Item3;

                Trace.WriteLine("Testing model: " + model);
                //Create a new well
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName + wellType.ToString(), CommissionDate = DateTime.Today, AssemblyAPI = DefaultWellName + wellType.ToString(), SubAssemblyAPI = DefaultWellName + wellType.ToString(), IntervalAPI = DefaultWellName + wellType.ToString(), WellType = wellType, WellStatusId = 2, WellDepthDatumId = 2, WellDepthDatumElevation = 2, WellGroundElevation = 1 }) });
                var allWells = WellService.GetAllWells().ToList();
                WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName + wellType.ToString()));
                Assert.IsNotNull(well);
                _wellsToRemove.Add(well);

                ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };

                options.Comment = "CASETest Upload " + modelInfo.Item1;
                modelFile.Options = options;
                modelFile.ApplicableDate = DateTime.Today.ToUniversalTime();
                modelFile.WellId = well.Id;

                byte[] fileAsByteArray = GetByteArray(Path, modelInfo.Item1);
                Assert.IsNotNull(fileAsByteArray);

                modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
                ModelFileService.AddWellModelFile(modelFile);
                ModelFileDTO newModelFile = ModelFileService.GetCurrentModelFile(well.Id.ToString());
                _modelFilesToRemove.Add(newModelFile.Id.ToString());
                CommonModelConfigDTO commonModelConfig = ModelFileService.GetCommonModelConfig(well.Id.ToString());
                Assert.IsNotNull(commonModelConfig);
                ModelFileOptionDTO ReturnedOptions = newModelFile.Options;
                Assert.AreEqual(options.CalibrationMethod, ReturnedOptions.CalibrationMethod);
                Assert.AreEqual(options.OptionalUpdate.Length, ReturnedOptions.OptionalUpdate.Length);
                Assert.AreEqual(well.Id.ToString(), commonModelConfig.WellId);
                if (wellType == WellTypeId.GLift)
                {
                    Assert.IsNotNull(commonModelConfig.FluidDataAndUnits.Value);
                    Assert.IsTrue(commonModelConfig.FluidDataAndUnits.Value.CO2Fraction == 0);
                    Assert.IsNotNull(commonModelConfig.ReferenceDepthData);
                    Assert.AreEqual(commonModelConfig.ReferenceDepthData.WellLocation, WellLocation.Platform);
                    Assert.IsNotNull(commonModelConfig.ReservoirData.LayerDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.ReservoirData.LayerDataAndUnits.Values.Length == 1);
                    Assert.IsNotNull(commonModelConfig.WellboreData);
                    Assert.IsNotNull(commonModelConfig.WellboreData.WellboreDeviationDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.WellboreData.WellboreDeviationDataAndUnits.Values.Length == 40);
                    Assert.IsNotNull(commonModelConfig.WellboreData.CasingDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.WellboreData.CasingDataAndUnits.Values.Length == 10);
                    Assert.IsNotNull(commonModelConfig.WellboreData.TubingDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.WellboreData.TubingDataAndUnits.Values.Length == 7);
                    Assert.IsNotNull(commonModelConfig.WellboreData.RestrictionDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.WellboreData.RestrictionDataAndUnits.Values.Length == 2);
                    Assert.IsNull(commonModelConfig.WellboreData.TracePointsDataAndUnits);
                }
                else if (wellType == WellTypeId.ESP)
                {
                    Assert.IsNotNull(commonModelConfig.FluidDataAndUnits.Value);
                    Assert.IsTrue(commonModelConfig.FluidDataAndUnits.Value.CO2Fraction == 0);
                    Assert.IsNotNull(commonModelConfig.ReferenceDepthData);
                    Assert.AreEqual(commonModelConfig.ReferenceDepthData.WellLocation, WellLocation.Onshore);
                    Assert.IsNotNull(commonModelConfig.ReservoirData.LayerDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.ReservoirData.LayerDataAndUnits.Values.Length == 1);
                    Assert.IsNotNull(commonModelConfig.WellboreData);
                    Assert.IsNotNull(commonModelConfig.WellboreData.WellboreDeviationDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.WellboreData.WellboreDeviationDataAndUnits.Values.First().DeviationAngle == 0);
                    Assert.IsNotNull(commonModelConfig.WellboreData.CasingDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.WellboreData.CasingDataAndUnits.Values.Length == 3);
                    Assert.IsNotNull(commonModelConfig.WellboreData.TubingDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.WellboreData.TubingDataAndUnits.Values.Length == 2);
                    Assert.IsNull(commonModelConfig.WellboreData.RestrictionDataAndUnits);
                    Assert.IsNull(commonModelConfig.WellboreData.TracePointsDataAndUnits);
                }
                else if (wellType == WellTypeId.GInj)
                {
                    Assert.IsNotNull(commonModelConfig.FluidDataAndUnits.Value);
                    Assert.IsTrue(commonModelConfig.FluidDataAndUnits.Value.CO2Fraction == 0);
                    Assert.IsNotNull(commonModelConfig.ReferenceDepthData);
                    Assert.AreEqual(commonModelConfig.ReferenceDepthData.WellLocation, WellLocation.Onshore);
                    Assert.IsNotNull(commonModelConfig.ReservoirData.LayerDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.ReservoirData.LayerDataAndUnits.Values.Length == 1);
                    Assert.IsNotNull(commonModelConfig.WellboreData);
                    Assert.IsNotNull(commonModelConfig.WellboreData.WellboreDeviationDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.WellboreData.WellboreDeviationDataAndUnits.Values.Length == 2);
                    Assert.IsNotNull(commonModelConfig.WellboreData.CasingDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.WellboreData.CasingDataAndUnits.Values.First().Name == "Casing");
                    Assert.IsNotNull(commonModelConfig.WellboreData.TubingDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.WellboreData.TubingDataAndUnits.Values.First().Name == "New Tubing_0");
                    Assert.IsNull(commonModelConfig.WellboreData.TracePointsDataAndUnits);
                }
                else if (wellType == WellTypeId.NF)
                {
                    Assert.IsNotNull(commonModelConfig.FluidDataAndUnits.Value);
                    Assert.IsTrue(commonModelConfig.FluidDataAndUnits.Value.CO2Fraction == 0);
                    Assert.IsNotNull(commonModelConfig.ReferenceDepthData);
                    Assert.AreEqual(commonModelConfig.ReferenceDepthData.WellLocation, WellLocation.Onshore);
                    Assert.IsNotNull(commonModelConfig.ReservoirData);
                    Assert.IsTrue(commonModelConfig.ReservoirData.LayerDataAndUnits.Values.Length == 1);
                    Assert.IsNotNull(commonModelConfig.WellboreData);
                    Assert.IsNotNull(commonModelConfig.WellboreData.WellboreDeviationDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.WellboreData.WellboreDeviationDataAndUnits.Values.Length == 1);
                    Assert.IsNotNull(commonModelConfig.WellboreData.CasingDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.WellboreData.CasingDataAndUnits.Values.First().Name == "Tubing");
                    Assert.IsNotNull(commonModelConfig.WellboreData.TubingDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.WellboreData.TubingDataAndUnits.Values.First().Name == "Tubing");
                    Assert.IsNull(commonModelConfig.WellboreData.TracePointsDataAndUnits);
                }
                else if (wellType == WellTypeId.WInj)
                {
                    Assert.IsNotNull(commonModelConfig.FluidDataAndUnits.Value);
                    Assert.IsTrue(commonModelConfig.FluidDataAndUnits.Value.CO2Fraction == 0);
                    Assert.IsNotNull(commonModelConfig.ReferenceDepthData);
                    Assert.AreEqual(commonModelConfig.ReferenceDepthData.WellLocation, WellLocation.Platform);
                    Assert.IsNotNull(commonModelConfig.ReservoirData);
                    Assert.AreEqual(1, commonModelConfig.ReservoirData.LayerDataAndUnits.Values.Length);
                    Assert.IsNotNull(commonModelConfig.WellboreData);
                    Assert.IsNotNull(commonModelConfig.WellboreData.WellboreDeviationDataAndUnits.Values);
                    Assert.AreEqual(48, commonModelConfig.WellboreData.WellboreDeviationDataAndUnits.Values.Length);
                    Assert.IsNotNull(commonModelConfig.WellboreData.CasingDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.WellboreData.CasingDataAndUnits.Values.First().Name == "Casing");
                    Assert.IsNotNull(commonModelConfig.WellboreData.TubingDataAndUnits.Values);
                    Assert.IsTrue(commonModelConfig.WellboreData.TubingDataAndUnits.Values.First().Name == "Tubing");
                    Assert.IsNull(commonModelConfig.WellboreData.TracePointsDataAndUnits);
                }
                //Remove Model File
                ModelFileService.RemoveModelFileOptionByModelFileId(newModelFile.Id.ToString());
                ModelFileService.RemoveModelFile(newModelFile.Id.ToString());
            }

            //Additional coverage for "GetWellsByIds" API
            WellDTO[] allWellList = WellService.GetAllWells();
            List<long> wellidlist = new List<long>();

            foreach (var indWell in allWellList)
            {
                wellidlist.Add(indWell.Id);
            }

            WellDTO[] allwelllistid = WellService.GetWellsByIds(wellidlist.ToArray());
            Assert.AreEqual(allwelllistid.Length, allWellList.Length, "Mismatch in well count");
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void SaveWellboreComponentsRemoveWellboreComponents()
        {
            var allWells = WellService.GetAllWells()?.ToList();
            WellDTO well = null;
            if (allWells.Count != 0)
            {
                well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            }
            // If there aren't any defined wells, make a test well.
            if (well == null)
            {
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });
                allWells = WellService.GetAllWells()?.ToList();
                well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
                _wellsToRemove.Add(well);
            }

            //Define the SubAssamblyAPI value that is stored inside the WellDTO object
            well.SubAssemblyAPI = "TestAPI";
            _wellsToRemove.Add(well);

            WellConfigDTO wellDTO = new WellConfigDTO();
            wellDTO.Well = well;

            // Here is our test downhole configuration.
            ModelConfigDTO modelDTO = new ModelConfigDTO();
            wellDTO.ModelConfig = modelDTO;

            modelDTO.WellId = well.Id.ToString();
            modelDTO.Rods = new RodStringConfigDTO();
            modelDTO.Rods.TotalRodLength = 1111;

            modelDTO.Rods.RodTapers = new RodTaperConfigDTO[1];
            modelDTO.Rods.RodTapers[0] = new RodTaperConfigDTO
            {
                Size = 1.0,
                Grade = "D",
                Manufacturer = "Weatherford, Inc.",
                NumberOfRods = 25
            };

            modelDTO.Weights = new CrankWeightsConfigDTO()
            {
                CrankId = "106110C",
                CBT = 5600,
                Crank_1_Primary = new RRLPrimaryWeightDTO()
                {
                    LagDistance = 63.8,
                    LagWeight = 0,
                    LagId = "N/A",
                    LeadDistance = 77.4,
                    LeadWeight = 5942,
                    LeadId = "00H",
                },
                Crank_1_Auxiliary = new RRLAuxiliaryWeightDTO()
                {
                    LagWeight = 0,
                    LagId = "N/A",
                    LeadWeight = 4334,
                    LeadId = "0AH",
                },

                Crank_2_Primary = new RRLPrimaryWeightDTO()
                {
                    LagDistance = 73.8,
                    LagWeight = 0,
                    LagId = "N/A",
                    LeadDistance = 77.4,
                    LeadWeight = 5395,
                    LeadId = "0H",
                },
                Crank_2_Auxiliary = new RRLAuxiliaryWeightDTO()
                {
                    LagWeight = 0,
                    LagId = "N/A",
                    LeadWeight = 3393,
                    LeadId = "1H",
                }
            };

            modelDTO.Surface = new SurfaceConfigDTO()
            {
                PumpingUnit = new PumpingUnitDTO()
                {
                    Id = 1,
                    Description = "CU-160-173-86 LEG Legrand C160-176-86 (7337) Uni-Frm",
                    MaxStrokeLength = 86,
                    NumberOfWristPins = 1,
                },
                ActualStrokeLength = 86,
                PumpingUnitType = new PumpingUnitTypeDTO()
                {
                    AbbreviatedName = "CU",
                    PK_rrlType = 69
                },
                MotorSize = new RRLMotorSizeDTO(5.0),
                MotorType = new RRLMotorTypeDTO()
                {
                    Name = "Nema B Electric",
                },
                SlipTorque = new RRLMotorSlipDTO()
                {
                    Rating = 2,
                },
            };

            modelDTO.Downhole = new DownholeConfigDTO()
            {
                PumpDiameter = 2.25,
                PumpDepth = 5070,
                TubingOD = 2.35,
                TubingID = 2.14,
                TubingAnchorDepth = 3000,
                CasingOD = 5.5,
                CasingWeight = 15.5,
                TopPerforation = 4000,
                BottomPerforation = 6000,
            };

            WellboreComponentService.SaveWellboreComponents(wellDTO);

            wellDTO.ModelConfig.Downhole.PumpDepth = 5071;

            bool flag = WellboreComponentService.UpdateWellboreComponents(wellDTO);
            Assert.IsTrue(flag);

            AssemblyDTO a = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(a);

            SubAssemblyDTO[] sas = WellboreComponentService.GetSubAssembliesByWellId(well.Id.ToString());
            Assert.IsNotNull(sas);
            Assert.AreEqual(sas.Count(), 1);
            Assert.AreEqual(sas[0].AssemblyId, a.Id);

            ReportDTO rpt = WellboreComponentService.GetReportByWellId(well.Id.ToString());
            Assert.IsNotNull(rpt);
            Assert.AreEqual(rpt.AssemblyId, a.Id);

            AssemblyComponentDTO[] acs = WellboreComponentService.GetAssemblyComponentsByWellId(well.Id.ToString());
            Assert.IsNotNull(acs);

            ComponentDTO[] cs = WellboreComponentService.GetComponentsByWellId(well.Id.ToString());
            Assert.IsNotNull(cs);

            //Now DELETE all the Wellbore Components
            ModelFileService.RemoveWellboreComponents(modelDTO);
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void CalculateCBT()
        {
            var allWells = WellService.GetAllWells()?.ToList();
            // Just take the first well. It doesn't matter to this test.
            WellDTO well = allWells?.FirstOrDefault();

            // If there aren't any defined wells, make a test well.
            if (well == null)
            {
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.RRL }) });
                allWells = WellService.GetAllWells()?.ToList();
                well = allWells.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
                _wellsToRemove.Add(well);
            }
            Assert.IsNotNull(well, "Failed to get a well.");

            // Here is our test data
            POPRRLCranksWeightsDTO testPOPRRLDTO = new POPRRLCranksWeightsDTO();
            testPOPRRLDTO.Auxiliary = new List<double>() { 0.0, 0.0, 0.0, 0.0 };
            testPOPRRLDTO.AuxiliaryIdentifier = new List<string>() { "none", "none", "none", "none" };
            testPOPRRLDTO.CrankCBT = 0.0;
            testPOPRRLDTO.CrankId = "";
            testPOPRRLDTO.DistanceM = new List<double>() { 94.7, 0.0, 94.7, 0.0 };
            testPOPRRLDTO.DistanceT = new List<double>() { 0.0, 0.0, 0.0, 0.0 };
            testPOPRRLDTO.FK_Crank = 0;
            testPOPRRLDTO.IsCrankHidden = false;
            testPOPRRLDTO.IsUserDefinedCrank = false;
            testPOPRRLDTO.PK_AuxiliaryWeights = new List<long>() { };
            testPOPRRLDTO.PK_PrimaryWeights = new List<long>() { };
            testPOPRRLDTO.Primary = new List<double>() { 504.0, 0.0, 504.0, 0.0 };
            testPOPRRLDTO.PrimaryIdentifier = new List<string> { "6RO", "none", "6RO", "none" };
            testPOPRRLDTO.PumpingUnitCrankCBT = 470810.0;
            testPOPRRLDTO.WellId = well.Id.ToString();

            double CBTValue = ModelFileService.CalculateCBT(testPOPRRLDTO);
            Trace.WriteLine("CBT value is: " + CBTValue);
            Assert.AreEqual(566.2676, CBTValue, "CBT value did not match");
            //TODO add tolerance if this comparison becomes a problem
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void CalculateRotaflexCBE()
        {
            RotaflexStackWeightsConfigDTO rotaflexWeights = new RotaflexStackWeightsConfigDTO();
            WeightStackConfigDTO stack1 = new WeightStackConfigDTO();
            WeightStackConfigDTO stack2 = new WeightStackConfigDTO();
            WeightStackConfigDTO stack3 = new WeightStackConfigDTO();
            WeightStackConfigDTO stack4 = new WeightStackConfigDTO();
            stack1.Height = 5;
            stack1.Width = 5;
            stack1.Depth = 5;

            List<WeightStackConfigDTO> listStack = new List<WeightStackConfigDTO>();
            listStack.Add(stack1);
            listStack.Add(stack2);
            listStack.Add(stack3);
            listStack.Add(stack4);
            WeightStackConfigDTO[] arrStack = listStack.ToArray();
            rotaflexWeights.Stacks = arrStack;
            rotaflexWeights.StdCBE = 0;
            double CBTValue = ModelFileService.CalculateRotaflexCBE("0", rotaflexWeights);
            Assert.AreEqual("35.41", Math.Round(CBTValue, 2).ToString());
            rotaflexWeights.StdCBE = 100;
            double newCBTValue = ModelFileService.CalculateRotaflexCBE("0", rotaflexWeights);
            Assert.AreEqual("135.41", Math.Round(newCBTValue, 2).ToString());
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void RemoveModelFilebyWellId()
        {
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today.AddYears(-2), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.RRL }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            WellDTO wellCompare = WellService.GetWell(well.Id.ToString());
            Assert.IsNotNull(wellCompare);
            Assert.AreEqual(well.Name, wellCompare.Name);

            var modelFile = new ModelFileDTO();
            modelFile.WellId = well.Id;
            modelFile.ApplicableDate = Truncate(DateTime.UtcNow.AddDays(-1));
            modelFile.Contents = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            ModelFileService.AddModelFile(modelFile);

            ModelFileDTO model = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(model);
            ModelFileService.RemoveModelFileByWellId(well.Id.ToString());
            ModelFileDTO aftermodel = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNull(aftermodel);
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void GetLiftTypeCalibrationMethodsAndOptionalUpdates()
        {
            int i = 1;
            foreach (WellTypeId wellTypeId in (WellTypeId[])Enum.GetValues(typeof(WellTypeId)))
            {
                if (wellTypeId == WellTypeId.Unknown || wellTypeId == WellTypeId.RRL || wellTypeId == WellTypeId.All)
                {
                    continue;
                }
                WellDTO well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName + i, AssemblyAPI = DefaultWellName + i, SubAssemblyAPI = DefaultWellName + i, IntervalAPI = DefaultWellName + i, CommissionDate = DateTime.Today, WellType = wellTypeId });
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
                WellDTO wellReturned = WellService.GetWellByName(well.Name);
                Assert.IsNotNull(wellReturned);
                _wellsToRemove.Add(wellReturned);
                switch (well.WellType)
                {
                    case WellTypeId.ESP:
                        {
                            CalibrationMethodId[] CalibrationMethodsHasDHPG = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.TrueString, null);
                            Assert.AreEqual(4, CalibrationMethodsHasDHPG.Count());
                            Assert.IsTrue(CalibrationMethodsHasDHPG.Contains(CalibrationMethodId.ReservoirPressureAndLFactor));
                            Assert.IsTrue(CalibrationMethodsHasDHPG.Contains(CalibrationMethodId.PIAndLFactor));
                            Assert.IsTrue(CalibrationMethodsHasDHPG.Contains(CalibrationMethodId.PumpWearFactorAndProductivityIndex));
                            Assert.IsTrue(CalibrationMethodsHasDHPG.Contains(CalibrationMethodId.PumpWearFactorAndReservoirPressure));

                            CalibrationMethodId[] CalibrationMethodsNoDHPG = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.FalseString, null);
                            Assert.AreEqual(7, CalibrationMethodsNoDHPG.Count());
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.LFactor));
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.PI));
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.ReservoirPressure));
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.ReservoirPressureAndLFactor));
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.PIAndLFactor));
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.PumpWearFactorAndProductivityIndex));
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.PumpWearFactorAndReservoirPressure));

                            OptionalUpdates[] OptionalUpdates = ModelFileService.GetOptionalUpdates(wellTypeId.ToString());
                            //Assert.AreEqual(4, OptionalUpdates.Count());
                            //Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.CalculateChokeD_Factor));
                            Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.PumpWearFactor));
                            Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.UpdateGOR_CGR));
                            Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.UpdateWCT_WGR));

                            break;
                        }
                    case WellTypeId.GLift:
                        {
                            CalibrationMethodId[] CalibrationMethodsHasDHPG = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.TrueString, WellFluidType.BlackOil.ToString());
                            Assert.AreEqual(2, CalibrationMethodsHasDHPG.Count());
                            Assert.IsTrue(CalibrationMethodsHasDHPG.Contains(CalibrationMethodId.ReservoirPressureAndLFactor));
                            Assert.IsTrue(CalibrationMethodsHasDHPG.Contains(CalibrationMethodId.PIAndLFactor));

                            CalibrationMethodId[] CalibrationMethodsNoDHPG = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.FalseString, WellFluidType.BlackOil.ToString());
                            Assert.AreEqual(3, CalibrationMethodsNoDHPG.Count());
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.PI));
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.ReservoirPressure));
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.LFactor));

                            CalibrationMethodId[] CalibrationMethodsHasDHPG1 = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.TrueString, WellFluidType.Condensate.ToString());
                            Assert.AreEqual(2, CalibrationMethodsHasDHPG1.Count());
                            Assert.IsTrue(CalibrationMethodsHasDHPG1.Contains(CalibrationMethodId.DarcyFlowCoefficientAndLFactor));
                            Assert.IsTrue(CalibrationMethodsHasDHPG1.Contains(CalibrationMethodId.ReservoirPressureAndLFactor));

                            CalibrationMethodId[] CalibrationMethodsNoDHPG1 = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.FalseString, WellFluidType.Condensate.ToString());
                            Assert.AreEqual(3, CalibrationMethodsNoDHPG1.Count());
                            Assert.IsTrue(CalibrationMethodsNoDHPG1.Contains(CalibrationMethodId.DarcyFlowCoefficient));
                            Assert.IsTrue(CalibrationMethodsNoDHPG1.Contains(CalibrationMethodId.ReservoirPressure));
                            Assert.IsTrue(CalibrationMethodsNoDHPG1.Contains(CalibrationMethodId.LFactor));


                            OptionalUpdates[] OptionalUpdates = ModelFileService.GetOptionalUpdates(wellTypeId.ToString());
                            //Assert.AreEqual(4, OptionalUpdates.Count());
                            //Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.CalculateChokeD_Factor));
                            Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.UpdateGOR_CGR));
                            Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.UpdateWCT_WGR));
                            Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.CalibrateOrificeSize));
                            break;
                        }
                    case WellTypeId.NF:
                        {  //updated integration test for FRWM-4448
                            CalibrationMethodId[] CalibrationMethodsHasDHPG = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.TrueString, WellFluidType.BlackOil.ToString());
                            Assert.AreEqual(2, CalibrationMethodsHasDHPG.Count());
                            Assert.IsTrue(CalibrationMethodsHasDHPG.Contains(CalibrationMethodId.ReservoirPressureAndLFactor));
                            Assert.IsTrue(CalibrationMethodsHasDHPG.Contains(CalibrationMethodId.PIAndLFactor));

                            CalibrationMethodId[] CalibrationMethodsNoDHPG = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.FalseString, WellFluidType.BlackOil.ToString());
                            Assert.AreEqual(3, CalibrationMethodsNoDHPG.Count());
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.PI));
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.ReservoirPressure));
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.LFactor));

                            CalibrationMethodId[] CalibrationMethodsHasDHPG1 = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.TrueString, WellFluidType.Condensate.ToString());
                            Assert.AreEqual(2, CalibrationMethodsHasDHPG1.Count());
                            Assert.IsTrue(CalibrationMethodsHasDHPG1.Contains(CalibrationMethodId.DarcyFlowCoefficientAndLFactor));
                            Assert.IsTrue(CalibrationMethodsHasDHPG1.Contains(CalibrationMethodId.ReservoirPressureAndLFactor));

                            CalibrationMethodId[] CalibrationMethodsNoDHPG1 = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.FalseString, WellFluidType.Condensate.ToString());
                            Assert.AreEqual(3, CalibrationMethodsNoDHPG1.Count());
                            Assert.IsTrue(CalibrationMethodsNoDHPG1.Contains(CalibrationMethodId.DarcyFlowCoefficient));
                            Assert.IsTrue(CalibrationMethodsNoDHPG1.Contains(CalibrationMethodId.ReservoirPressure));
                            Assert.IsTrue(CalibrationMethodsNoDHPG1.Contains(CalibrationMethodId.LFactor));

                            CalibrationMethodId[] CalibrationMethodsHasDHPG2 = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.TrueString, WellFluidType.DryGas.ToString());
                            Assert.AreEqual(2, CalibrationMethodsHasDHPG2.Count());
                            Assert.IsTrue(CalibrationMethodsHasDHPG2.Contains(CalibrationMethodId.DarcyFlowCoefficientAndLFactor));
                            Assert.IsTrue(CalibrationMethodsHasDHPG2.Contains(CalibrationMethodId.ReservoirPressureAndLFactor));

                            CalibrationMethodId[] CalibrationMethodsNoDHPG2 = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.FalseString, WellFluidType.DryGas.ToString());
                            Assert.AreEqual(3, CalibrationMethodsNoDHPG2.Count());
                            Assert.IsTrue(CalibrationMethodsNoDHPG2.Contains(CalibrationMethodId.DarcyFlowCoefficient));
                            Assert.IsTrue(CalibrationMethodsNoDHPG2.Contains(CalibrationMethodId.ReservoirPressure));
                            Assert.IsTrue(CalibrationMethodsNoDHPG2.Contains(CalibrationMethodId.LFactor));

                            OptionalUpdates[] OptionalUpdates = ModelFileService.GetOptionalUpdates(wellTypeId.ToString());
                            Assert.AreEqual(3, OptionalUpdates.Count());
                            Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.CalculateChokeD_Factor));
                            Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.UpdateGOR_CGR));
                            Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.UpdateWCT_WGR));
                            break;
                        }
                    case WellTypeId.GInj:
                        {
                            CalibrationMethodId[] CalibrationMethodsHasDHPG = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.TrueString, null);
                            Assert.AreEqual(2, CalibrationMethodsHasDHPG.Count());
                            Assert.IsTrue(CalibrationMethodsHasDHPG.Contains(CalibrationMethodId.ReservoirPressureAndLFactor));
                            Assert.IsTrue(CalibrationMethodsHasDHPG.Contains(CalibrationMethodId.DarcyFlowCoefficientAndLFactor));

                            CalibrationMethodId[] CalibrationMethodsNoDHPG = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.FalseString, null);
                            Assert.AreEqual(3, CalibrationMethodsNoDHPG.Count());
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.LFactor));
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.ReservoirPressure));
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.DarcyFlowCoefficient));

                            OptionalUpdates[] OptionalUpdates = ModelFileService.GetOptionalUpdates(wellTypeId.ToString());
                            Assert.AreEqual(2, OptionalUpdates.Count());
                            //Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.CalculateChokeD_Factor));
                            Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.UpdateGOR_CGR));
                            Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.UpdateWCT_WGR));

                            break;
                        }
                    case WellTypeId.WGInj:
                        {
                            CalibrationMethodId[] CalibrationMethodsHasDHPG = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.TrueString, null);
                            Assert.AreEqual(0, CalibrationMethodsHasDHPG.Count());

                            CalibrationMethodId[] CalibrationMethodsNoDHPG = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.FalseString, null);
                            Assert.AreEqual(3, CalibrationMethodsNoDHPG.Count());
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.LFactor));
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.ReservoirPressure));
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.InjectivityIndex));

                            OptionalUpdates[] OptionalUpdates = ModelFileService.GetOptionalUpdates(wellTypeId.ToString());
                            Assert.AreEqual(2, OptionalUpdates.Count());
                            //Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.CalculateChokeD_Factor));
                            Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.UpdateGOR_CGR));
                            Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.UpdateWCT_WGR));

                            break;
                        }
                    case WellTypeId.WInj:
                        {
                            CalibrationMethodId[] CalibrationMethodsHasDHPG = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.TrueString, null);
                            Assert.AreEqual(2, CalibrationMethodsHasDHPG.Count());
                            Assert.IsTrue(CalibrationMethodsHasDHPG.Contains(CalibrationMethodId.InjectivityIndexAndLFactor));
                            Assert.IsTrue(CalibrationMethodsHasDHPG.Contains(CalibrationMethodId.ReservoirPressureAndLFactor));

                            CalibrationMethodId[] CalibrationMethodsNoDHPG = ModelFileService.GetCalibrationMethods(wellReturned.WellType.ToString(), bool.FalseString, null);
                            Assert.AreEqual(3, CalibrationMethodsNoDHPG.Count());
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.ReservoirPressure));
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.InjectivityIndex));
                            Assert.IsTrue(CalibrationMethodsNoDHPG.Contains(CalibrationMethodId.LFactor));

                            OptionalUpdates[] OptionalUpdates = ModelFileService.GetOptionalUpdates(wellTypeId.ToString());
                            Assert.AreEqual(2, OptionalUpdates.Count());
                            //Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.CalculateChokeD_Factor));
                            Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.UpdateGOR_CGR));
                            Assert.IsTrue(OptionalUpdates.Contains(Enums.OptionalUpdates.UpdateWCT_WGR));

                            break;
                        }
                    case WellTypeId.PLift:
                        {
                            Trace.WriteLine("There is no Calibration Method implemented for Plunger Lift");
                            break;
                        }
                    case WellTypeId.OT:
                        {
                            Trace.WriteLine("There is no Calibration Method implemented for OT wells");
                            break;
                        }
                    case WellTypeId.PCP:
                        {
                            Trace.WriteLine("Not Yet Implemented for PCP Well");
                            break;
                        }
                    case WellTypeId.All:
                        {
                            Trace.WriteLine("Not Yet Implemented for ALL Type Well");
                            break;
                        }
                    default:
                        {
                            Assert.Fail($"Unknown well type {wellTypeId}.");
                            break;
                        }
                }
                i++;
            }
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void UploadModelFile()
        {
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.GLift }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);

            ModelFileBase64DTO modelFile = new ModelFileBase64DTO();
            ModelFileOptionDTO options = new ModelFileOptionDTO();

            options.Comment = "CASETest Upload WellfloGasLiftExample1.wflx";
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime();
            modelFile.WellId = well.Id;

            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string fileName = "WellfloGasLiftExample1.wflx";
            byte[] fileAsByteArray = GetByteArray(Path, fileName);
            Assert.IsNotNull(fileAsByteArray);
            //Convert byte array to Base64 string
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            ModelFileService.AddWellModelFile(modelFile);
            ModelFileDTO newModelFile = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(newModelFile);

            _modelFilesToRemove.Add(newModelFile.Id.ToString());

            ModelFileService.RemoveModelFileOptionByModelFileId(newModelFile.Id.ToString());
            ModelFileService.RemoveModelFile(newModelFile.Id.ToString());

            _modelFilesToRemove.Remove(newModelFile.Id.ToString());
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void RemoveModelFileOptionbyModelFileId()
        {
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today.AddYears(-2), WellType = WellTypeId.GLift }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            WellDTO wellCompare = WellService.GetWell(well.Id.ToString());
            Assert.IsNotNull(wellCompare);
            Assert.AreEqual(well.Name, wellCompare.Name);

            var modelFile = new ModelFileDTO();
            modelFile.WellId = well.Id;
            modelFile.ApplicableDate = Truncate(DateTime.UtcNow.AddDays(-1));
            modelFile.Contents = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            ModelFileService.AddModelFile(modelFile);
            ModelFileDTO model = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(model);

            ModelFileOptionDTO options = new ModelFileOptionDTO();
            options.Comment = "CASETest option delete test.";
            options.ModelFileId = model.Id;
            ModelFileService.AddModelFileOptions(options);
            ModelFileService.RemoveModelFileOptionByModelFileId(model.Id.ToString());

            ModelFileDTO aftermodel = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNull(aftermodel.Options);
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void RemoveModelFileOptionsByWellId()
        {
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today.AddYears(-2), WellType = WellTypeId.GLift }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            WellDTO wellCompare = WellService.GetWell(well.Id.ToString());
            Assert.IsNotNull(wellCompare);
            Assert.AreEqual(well.Name, wellCompare.Name);

            var modelFile = new ModelFileDTO();
            modelFile.WellId = well.Id;
            modelFile.ApplicableDate = Truncate(DateTime.UtcNow.AddDays(-1));
            modelFile.Contents = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            ModelFileService.AddModelFile(modelFile);
            ModelFileDTO model = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(model);

            ModelFileOptionDTO options = new ModelFileOptionDTO();
            options.Comment = "CASETest option delete test.";
            options.ModelFileId = model.Id;
            ModelFileService.AddModelFileOptions(options);
            ModelFileService.RemoveModelFileOptionsByWellId(well.Id.ToString());

            ModelFileDTO aftermodel = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNull(aftermodel.Options);
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void GetGasLiftModelConfig()
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string model = "WellfloGasLiftExample1.wflx";
            WellTypeId wellType = WellTypeId.GLift;
            ModelFileOptionDTO options = new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.CalculateChokeD_Factor) } };

            Trace.WriteLine("Testing model: " + model);
            //Create a new well
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName + wellType.ToString(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = (DateTime.Today - TimeSpan.FromDays(100)), WellType = wellType, WellDepthDatumElevation = 1, WellDepthDatumId = 2, WellStatusId = 2 }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName + wellType.ToString()));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);

            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };

            options.Comment = "CASETest Upload " + model;
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime();
            modelFile.WellId = well.Id;

            byte[] fileAsByteArray = GetByteArray(Path, model);
            Assert.IsNotNull(fileAsByteArray);

            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            ModelFileService.AddWellModelFile(modelFile);
            ModelFileDTO newModelFile = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            _modelFilesToRemove.Add(newModelFile.Id.ToString());
            GasLiftConfigDTO gasLiftConfig = ModelFileService.GetGasLiftModelConfig(well.Id.ToString());
            Assert.IsNotNull(gasLiftConfig);

            //ModelFile Validation
            var GLModelFileValidation = ModelFileService.GetModelFileValidationByModelId(newModelFile.Id.ToString());
            Assert.AreEqual(0, GLModelFileValidation.DHPG, "Mismatch in DHPG selection");
            Assert.AreEqual("None", GLModelFileValidation.FluidPhase.ToString(), "Mismatch in Fluid Phase");
            Assert.AreEqual("BlackOil", GLModelFileValidation.FluidType.ToString(), "Mismatch in Fluid Type");
            Assert.AreEqual("GLift", GLModelFileValidation.WellType.ToString(), "Mismatch in Well Type");

            Assert.IsNotNull(gasLiftConfig.GasLiftDataAndUnits.Values);
            GasLiftDataDTO[] gasLiftData = gasLiftConfig.GasLiftDataAndUnits.Values;
            Assert.IsTrue(gasLiftData.Count() == 3);

            //Remove Model File
            ModelFileService.RemoveModelFileOptionByModelFileId(newModelFile.Id.ToString());
            ModelFileService.RemoveModelFile(newModelFile.Id.ToString());
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void GetESPModelConfig()
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string model = "WellfloESPExample1.wflx";
            WellTypeId wellType = WellTypeId.ESP;
            ModelFileOptionDTO options = new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.ReservoirPressure, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR) } };

            Trace.WriteLine("Testing model: " + model);
            //Create a new well
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName + wellType.ToString(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = (DateTime.Today - TimeSpan.FromDays(100)), WellType = wellType, WellDepthDatumElevation = 1, WellDepthDatumId = 2, WellStatusId = 2 }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName + wellType.ToString()));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);

            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };

            options.Comment = "CASETest Upload " + model;
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime();
            modelFile.WellId = well.Id;

            byte[] fileAsByteArray = GetByteArray(Path, model);
            Assert.IsNotNull(fileAsByteArray);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            ModelFileService.AddWellModelFile(modelFile);
            ModelFileDTO newModelFile = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            _modelFilesToRemove.Add(newModelFile.Id.ToString());
            ESPConfigDTO espConfig = ModelFileService.GetESPModelConfig(well.Id.ToString());

            //ModelFile Validation
            var ESPModelFileValidation = ModelFileService.GetModelFileValidationByModelId(newModelFile.Id.ToString());
            Assert.AreEqual(0, ESPModelFileValidation.DHPG, "Mismatch in DHPG selection");
            Assert.AreEqual("None", ESPModelFileValidation.FluidPhase.ToString(), "Mismatch in Fluid Phase");
            Assert.AreEqual("BlackOil", ESPModelFileValidation.FluidType.ToString(), "Mismatch in Fluid Type");
            Assert.AreEqual("ESP", ESPModelFileValidation.WellType.ToString(), "Mismatch in Well Type");

            //ESP Config
            Assert.IsNotNull(espConfig);
            Assert.IsNotNull(espConfig.ESPDataAndUnits);
            Assert.IsNotNull(espConfig.ESPDataAndUnits.Value);

            //Analysis
            Assert.IsNotNull(espConfig.ESPDataAndUnits.Value.Analysis);
            Assert.IsNotNull(espConfig.ESPDataAndUnits.Value.Analysis.Value);
            Assert.IsNotNull(espConfig.ESPDataAndUnits.Value.Analysis.Units);
            Assert.IsTrue(espConfig.ESPDataAndUnits.Value.Analysis.Value.GasSeparatorPresent == true);
            Assert.IsTrue(espConfig.ESPDataAndUnits.Value.Analysis.Units.SeparatorEfficiency.Precision == 4);

            //Pumps array
            Assert.IsNotNull(espConfig.ESPDataAndUnits.Value.Pumps);
            Assert.IsTrue(espConfig.ESPDataAndUnits.Value.Pumps.Values.Length == 1);
            Assert.IsNotNull(espConfig.ESPDataAndUnits.Value.Pumps.Units);
            Assert.IsTrue(espConfig.ESPDataAndUnits.Value.Pumps.Values[0].PumpWearFactor == 1);
            Assert.IsTrue(espConfig.ESPDataAndUnits.Value.Pumps.Units.PumpWearFactor.Precision == 4);

            //Enviroment
            Assert.IsNotNull(espConfig.ESPDataAndUnits.Value.Environment);
            Assert.IsNotNull(espConfig.ESPDataAndUnits.Value.Environment.Value);
            Assert.IsNotNull(espConfig.ESPDataAndUnits.Value.Environment.Units);
            Assert.IsTrue(espConfig.ESPDataAndUnits.Value.Environment.Value.PumpDepth == 8000);
            Assert.IsTrue(espConfig.ESPDataAndUnits.Value.Environment.Units.PumpDepth.Precision == 2);

            //Motor
            Assert.IsNotNull(espConfig.ESPDataAndUnits.Value.Motor);
            Assert.IsNotNull(espConfig.ESPDataAndUnits.Value.Motor.Value);
            Assert.IsNotNull(espConfig.ESPDataAndUnits.Value.Motor.Units);
            Assert.IsTrue(espConfig.ESPDataAndUnits.Value.Motor.Value.MotorWearFactor == 1);
            Assert.IsTrue(espConfig.ESPDataAndUnits.Value.Motor.Units.MotorWearFactor.Precision == 4);
            Assert.IsTrue(espConfig.ESPDataAndUnits.Value.Motor.Value.CableSize == "#2");

            //Remove Model File
            ModelFileService.RemoveModelFileOptionByModelFileId(newModelFile.Id.ToString());
            ModelFileService.RemoveModelFile(newModelFile.Id.ToString());
        }


        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void GetPCPModelConfig()
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string model = "PCP-Multiphase.wflx";
            WellDTO PCPWell = new WellDTO();

            #region Creating PCP Well with Model File
            WellTypeId wellType = WellTypeId.PCP;
            ModelFileOptionDTO options = new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) } };

            Trace.WriteLine("Testing model: " + model);
            //Create a new well
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidTypeAndPhase(new WellDTO() { Name = "PCPWELL_00001", CommissionDate = (DateTime.Today - TimeSpan.FromDays(100)), WellType = WellTypeId.PCP, FluidPhase = WellFluidPhase.MultiPhase, AssemblyAPI = "PCPWELL_00001", SubAssemblyAPI = "PCPWELL_00001", IntervalAPI = "PCPWELL_00001", WellDepthDatumElevation = 1, WellDepthDatumId = 2, WellStatusId = 2 }) });

            PCPWell = WellService.GetWellByName("PCPWELL_00001");
            Assert.IsNotNull(PCPWell);
            _wellsToRemove.Add(PCPWell);

            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };

            options.Comment = "CASETest Upload " + model;
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime();
            modelFile.WellId = PCPWell.Id;

            byte[] fileAsByteArray = GetByteArray(Path, model);
            Assert.IsNotNull(fileAsByteArray);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            ModelFileService.AddWellModelFile(modelFile);
            ModelFileDTO newModelFile = ModelFileService.GetCurrentModelFile(PCPWell.Id.ToString());
            _modelFilesToRemove.Add(newModelFile.Id.ToString());
            #endregion Creating PCP Well with Model File

            //Getting PCP ModelConfiguration
            PCPConfigDTO pcpConfig = ModelFileService.GetPCPModelConfig(PCPWell.Id.ToString());

            //ModelFile Validation
            var PCPModelFileValidation = ModelFileService.GetModelFileValidationByModelId(newModelFile.Id.ToString());
            Assert.AreEqual(0, PCPModelFileValidation.DHPG, "Mismatch in DHPG selection");
            Assert.AreEqual("MultiPhase", PCPModelFileValidation.FluidPhase.ToString(), "Mismatch in Fluid Phase");
            Assert.AreEqual("BlackOil", PCPModelFileValidation.FluidType.ToString(), "Mismatch in Fluid Type");
            Assert.AreEqual("PCP", PCPModelFileValidation.WellType.ToString(), "Mismatch in Well Type");

            #region ModelConfiguration - FluidData and Units
            //FluidDataAndUnits
            Assert.IsNotNull(pcpConfig);
            Assert.IsNotNull(pcpConfig.FluidDataAndUnits);
            Assert.IsNotNull(pcpConfig.FluidDataAndUnits.Value);
            Assert.IsNotNull(pcpConfig.FluidDataAndUnits.Units);
            #endregion ModelConfiguration - FluidData and Units

            #region ModelConfiguration - PCPData and Units
            //PCPDataAndUnits
            Assert.IsNotNull(pcpConfig);
            Assert.IsNotNull(pcpConfig.PCPDataAndUnits);
            Assert.IsNotNull(pcpConfig.PCPDataAndUnits.Value);
            Assert.IsNotNull(pcpConfig.PCPDataAndUnits.Units);

            //PCPDataAndUnits - BeltsAndSheaves
            Assert.IsNotNull(pcpConfig.PCPDataAndUnits.Value.BeltsAndSheaves);
            Assert.IsNotNull(pcpConfig.PCPDataAndUnits.Value.BeltsAndSheaves.Value);
            Assert.IsNotNull(pcpConfig.PCPDataAndUnits.Value.BeltsAndSheaves.Units);

            //PCPDataAndUnits - Drivehead
            Assert.IsNotNull(pcpConfig.PCPDataAndUnits.Value.Drivehead);
            Assert.IsNotNull(pcpConfig.PCPDataAndUnits.Value.Drivehead.Value);
            Assert.IsNotNull(pcpConfig.PCPDataAndUnits.Value.Drivehead.Units);

            //PCPDataAndUnits - Pump
            Assert.IsNotNull(pcpConfig.PCPDataAndUnits.Value.Pump);
            Assert.IsNotNull(pcpConfig.PCPDataAndUnits.Value.Pump.Value);
            Assert.IsNotNull(pcpConfig.PCPDataAndUnits.Value.Pump.Units);

            //PCPDataAndUnits - Motor
            Assert.IsNotNull(pcpConfig.PCPDataAndUnits.Value.Motor);
            Assert.IsNotNull(pcpConfig.PCPDataAndUnits.Value.Motor.Value);
            Assert.IsNotNull(pcpConfig.PCPDataAndUnits.Value.Motor.Units);
            #endregion ModelConfiguration - PCPData and Units

            #region ModelConfiguration - FluidData and Units
            //FluidDataAndUnits
            Assert.IsNotNull(pcpConfig);
            Assert.IsNotNull(pcpConfig.ReferenceDepthData.WellLocation);
            #endregion ModelConfiguration - FluidData and Units

            #region ModelConfiguration - ReservoirData
            //ReservoirData
            Assert.IsNotNull(pcpConfig);
            Assert.IsNotNull(pcpConfig.ReservoirData);
            Assert.IsNotNull(pcpConfig.ReservoirData.LayerDataAndUnits);
            Assert.IsNotNull(pcpConfig.ReservoirData.LayerDataAndUnits.Values);
            Assert.IsNotNull(pcpConfig.ReservoirData.LayerDataAndUnits.Units);
            #endregion ModelConfiguration - ReservoirData

            #region ModelConfiguration - WellboreData
            //WellboreData
            Assert.IsNotNull(pcpConfig);
            Assert.IsNotNull(pcpConfig.WellboreData);
            Assert.IsNotNull(pcpConfig.WellboreData.CasingDataAndUnits);
            Assert.IsNotNull(pcpConfig.WellboreData.CasingDataAndUnits.Units);
            Assert.IsNotNull(pcpConfig.WellboreData.CasingDataAndUnits.Values);

            Assert.IsNull(pcpConfig.WellboreData.RestrictionDataAndUnits);

            Assert.IsNull(pcpConfig.WellboreData.TracePointsDataAndUnits);

            Assert.IsNotNull(pcpConfig.WellboreData.TubingDataAndUnits);
            Assert.IsNotNull(pcpConfig.WellboreData.TubingDataAndUnits.Units);
            Assert.IsNotNull(pcpConfig.WellboreData.TubingDataAndUnits.Values);

            Assert.IsNotNull(pcpConfig.WellboreData.WellboreDeviationDataAndUnits);
            Assert.IsNotNull(pcpConfig.WellboreData.WellboreDeviationDataAndUnits.Units);
            Assert.IsNotNull(pcpConfig.WellboreData.WellboreDeviationDataAndUnits.Values);
            #endregion ModelConfiguration - WellboreData          

            //Remove Model File
            ModelFileService.RemoveModelFileOptionByModelFileId(newModelFile.Id.ToString());
            ModelFileService.RemoveModelFile(newModelFile.Id.ToString());
        }


        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void GetPGLModelConfig()
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string model = "PL-631.wflx";
            WellDTO PGLWell = new WellDTO();

            #region Creating PGL Well with Model File
            WellTypeId wellType = WellTypeId.PLift;
            ModelFileOptionDTO options = new ModelFileOptionDTO();

            options.OptionalUpdate = new long[] { };

            Trace.WriteLine("Testing model: " + model);
            //Create a new well
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidTypeAndPhase(new WellDTO() { Name = "PGLWELL_00001", CommissionDate = (DateTime.Today - TimeSpan.FromDays(100)), WellType = WellTypeId.PLift, AssemblyAPI = "PGLWELL_00001", SubAssemblyAPI = "PGLWELL_00001", IntervalAPI = "PCPWELL_00001", WellStatusId = 2, WellDepthDatumElevation = 1, WellDepthDatumId = 2 }) });

            PGLWell = WellService.GetWellByName("PGLWELL_00001");
            Assert.IsNotNull(PGLWell);
            _wellsToRemove.Add(PGLWell);

            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };

            options.Comment = "CASETest Upload " + model;
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime();
            modelFile.WellId = PGLWell.Id;

            byte[] fileAsByteArray = GetByteArray(Path, model);
            Assert.IsNotNull(fileAsByteArray);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            ModelFileService.AddWellModelFile(modelFile);
            ModelFileDTO newModelFile = ModelFileService.GetCurrentModelFile(PGLWell.Id.ToString());
            _modelFilesToRemove.Add(newModelFile.Id.ToString());
            #endregion Creating PGL Well with Model File

            //Getting PGL ModelConfiguration
            PGLConfigDTO PGLConfig = ModelFileService.GetPGLModelConfig(PGLWell.Id.ToString());

            //ModelFile Validation
            var PGLModelFileValidation = ModelFileService.GetModelFileValidationByModelId(newModelFile.Id.ToString());
            Assert.AreEqual(0, PGLModelFileValidation.DHPG, "Mismatch in DHPG selection");
            Assert.AreEqual("None", PGLModelFileValidation.FluidPhase.ToString(), "Mismatch in Fluid Phase");
            Assert.AreEqual("DryGas", PGLModelFileValidation.FluidType.ToString(), "Mismatch in Fluid Type");
            Assert.AreEqual("PLift", PGLModelFileValidation.WellType.ToString(), "Mismatch in Well Type");

            #region ModelConfiguration - FluidData and Units
            //FluidDataAndUnits
            Assert.IsNotNull(PGLConfig);
            Assert.IsNotNull(PGLConfig.FluidDataAndUnits);
            Assert.IsNotNull(PGLConfig.FluidDataAndUnits.Value);
            Assert.IsNotNull(PGLConfig.FluidDataAndUnits.Units);
            #endregion ModelConfiguration - FluidData and Units

            #region ModelConfiguration - PGLData and Units
            //PGLDataAndUnits
            Assert.IsNotNull(PGLConfig);
            Assert.IsNotNull(PGLConfig.PGLDataAndUnits);
            Assert.IsNotNull(PGLConfig.PGLDataAndUnits.Value);
            Assert.IsNotNull(PGLConfig.PGLDataAndUnits.Units);

            //PGLDataAndUnits -Conditions
            Assert.IsNotNull(PGLConfig.PGLDataAndUnits.Value.Conditions);
            Assert.IsNotNull(PGLConfig.PGLDataAndUnits.Value.Conditions.Value);
            Assert.IsNotNull(PGLConfig.PGLDataAndUnits.Value.Conditions.Units);

            //PGLDataAndUnits - Environment
            Assert.IsNotNull(PGLConfig.PGLDataAndUnits.Value.Environment);
            Assert.IsNotNull(PGLConfig.PGLDataAndUnits.Value.Environment.Value);
            Assert.IsNotNull(PGLConfig.PGLDataAndUnits.Value.Environment.Units);
            #endregion ModelConfiguration - PGLData and Units

            #region ModelConfiguration - FluidData and Units
            //FluidDataAndUnits
            Assert.IsNotNull(PGLConfig);
            Assert.IsNotNull(PGLConfig.ReferenceDepthData.WellLocation);
            #endregion ModelConfiguration - FluidData and Units

            #region ModelConfiguration - ReservoirData
            //ReservoirData
            Assert.IsNotNull(PGLConfig);
            Assert.IsNotNull(PGLConfig.ReservoirData);
            Assert.IsNotNull(PGLConfig.ReservoirData.LayerDataAndUnits);
            Assert.IsNotNull(PGLConfig.ReservoirData.LayerDataAndUnits.Values);
            Assert.IsNotNull(PGLConfig.ReservoirData.LayerDataAndUnits.Units);
            #endregion ModelConfiguration - ReservoirData

            #region ModelConfiguration - WellboreData
            //WellboreData
            Assert.IsNotNull(PGLConfig);
            Assert.IsNotNull(PGLConfig.WellboreData);
            Assert.IsNotNull(PGLConfig.WellboreData.CasingDataAndUnits);
            Assert.IsNotNull(PGLConfig.WellboreData.CasingDataAndUnits.Units);
            Assert.IsNotNull(PGLConfig.WellboreData.CasingDataAndUnits.Values);

            Assert.IsNull(PGLConfig.WellboreData.RestrictionDataAndUnits);

            Assert.IsNull(PGLConfig.WellboreData.TracePointsDataAndUnits);

            Assert.IsNotNull(PGLConfig.WellboreData.TubingDataAndUnits);
            Assert.IsNotNull(PGLConfig.WellboreData.TubingDataAndUnits.Units);
            Assert.IsNotNull(PGLConfig.WellboreData.TubingDataAndUnits.Values);

            Assert.IsNotNull(PGLConfig.WellboreData.WellboreDeviationDataAndUnits);
            Assert.IsNotNull(PGLConfig.WellboreData.WellboreDeviationDataAndUnits.Units);
            Assert.IsNotNull(PGLConfig.WellboreData.WellboreDeviationDataAndUnits.Values);
            #endregion ModelConfiguration - WellboreData          

            //Remove Model File
            ModelFileService.RemoveModelFileOptionByModelFileId(newModelFile.Id.ToString());
            ModelFileService.RemoveModelFile(newModelFile.Id.ToString());
        }


        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void GetCommonModelConfigFactory()
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            Tuple<string, WellTypeId, ModelFileOptionDTO>[] models = { Tuple.Create("WellfloGasLiftExample1.wflx", WellTypeId.GLift, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.CalculateChokeD_Factor) } }),
                Tuple.Create("WellfloGasInjectionExample1.wflx", WellTypeId.GInj, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.PIAndLFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.CalculateChokeD_Factor) } }),
                Tuple.Create("WellfloESPExample1.wflx", WellTypeId.ESP, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.ReservoirPressure, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR)} }),
                Tuple.Create("WellfloNFWExample1.wflx", WellTypeId.NF, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.DarcyFlowCoefficient, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.CalculateChokeD_Factor) } }),
                Tuple.Create("WellfloWaterInjectionExample1.wflx", WellTypeId.WInj, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.InjectivityIndexAndLFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.CalculateChokeD_Factor), ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.UpdateWCT_WGR) } }) };
            foreach (Tuple<string, WellTypeId, ModelFileOptionDTO> modelInfo in models)
            {
                string model = modelInfo.Item1;
                WellTypeId wellType = modelInfo.Item2;
                ModelFileOptionDTO options = modelInfo.Item3;

                Trace.WriteLine("Testing model: " + model);
                //Create a new well
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName + wellType.ToString(), AssemblyAPI = DefaultWellName + wellType.ToString(), SubAssemblyAPI = DefaultWellName + wellType.ToString(), IntervalAPI = DefaultWellName + wellType.ToString(), CommissionDate = DateTime.Today, WellType = wellType }) });
                var allWells = WellService.GetAllWells().ToList();
                WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName + wellType.ToString()));
                Assert.IsNotNull(well);
                _wellsToRemove.Add(well);

                ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };

                options.Comment = "CASETest Upload " + modelInfo.Item1;
                modelFile.Options = options;
                modelFile.ApplicableDate = DateTime.Today.ToUniversalTime();
                modelFile.WellId = well.Id;
                byte[] fileAsByteArray = GetByteArray(Path, modelInfo.Item1);
                Assert.IsNotNull(fileAsByteArray);
                modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
                ModelFileService.AddWellModelFile(modelFile);
                _modelFilesToRemove.Add(modelFile.Id.ToString());

                CommonModelConfigDTO openModel = ModelFileService.CommonModelConfigFactory(well.Id.ToString());
                switch (wellType)
                {
                    case Enums.WellTypeId.ESP:
                        Assert.IsInstanceOfType(openModel, typeof(ESPConfigDTO));
                        ESPConfigDTO espModel = (ESPConfigDTO)openModel;
                        Assert.IsNotNull(espModel);
                        Assert.IsNotNull(espModel.ESPDataAndUnits);
                        Assert.IsTrue(espModel.ESPDataAndUnits.Value.Motor.Value.CableSize == "#2");
                        break;

                    case Enums.WellTypeId.GLift:
                        Assert.IsInstanceOfType(openModel, typeof(GasLiftConfigDTO));
                        GasLiftConfigDTO glModel = (GasLiftConfigDTO)openModel;
                        Assert.IsNotNull(glModel);
                        Assert.IsNotNull(glModel.GasLiftDataAndUnits.Values);
                        Assert.IsTrue(glModel.GasLiftDataAndUnits.Values.Count() == 3);
                        break;

                    default:
                        Assert.IsNotNull(openModel);
                        break;
                }
            }

            CommonModelConfigDTO nullModelABC = ModelFileService.CommonModelConfigFactory("abc");
            Assert.IsNull(nullModelABC);

            CommonModelConfigDTO nullModel99 = ModelFileService.CommonModelConfigFactory("-99");
            Assert.IsNull(nullModel99);
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void GetListModelFileHeaderByWell()
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string model = "WellfloGasLiftExample1.wflx";
            WellTypeId wellType = WellTypeId.GLift;
            ModelFileOptionDTO options = new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.CalculateChokeD_Factor) } };

            Trace.WriteLine("Testing model: " + model);
            //Create a new well
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName + wellType.ToString(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = wellType }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName + wellType.ToString()));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);

            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };

            options.Comment = "CASETest Upload " + model;
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime();
            modelFile.WellId = well.Id;
            byte[] fileAsByteArray = GetByteArray(Path, model);
            Assert.IsNotNull(fileAsByteArray);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            //First File
            ModelFileService.AddWellModelFile(modelFile);
            //Second File
            ModelFileService.AddWellModelFile(modelFile);
            //Third File
            ModelFileService.AddWellModelFile(modelFile);

            ModelFileHeaderDTO[] modelFiles = ModelFileService.GetListModelFileHeaderByWell(well.Id.ToString());
            Assert.IsNotNull(modelFiles);
            Assert.IsTrue(modelFiles.Length == 3);
            foreach (ModelFileHeaderDTO modelFileHeader in modelFiles)
            {
                Assert.IsNotNull(modelFileHeader.Options);
                Assert.IsTrue(modelFileHeader.Options.CalibrationMethod == CalibrationMethodId.LFactor);
            }
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void GetBase64ModelFile()
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string model = "WellfloGasLiftExample1.wflx";
            WellTypeId wellType = WellTypeId.GLift;
            ModelFileOptionDTO options = new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.CalculateChokeD_Factor) } };

            Trace.WriteLine("Testing model: " + model);
            //Create a new well
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName + wellType.ToString(), IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = wellType }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName + wellType.ToString()));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);

            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };

            options.Comment = "CASETest Upload " + model;
            modelFile.Options = options;
            modelFile.ApplicableDate = DateTime.Today.ToUniversalTime();
            modelFile.WellId = well.Id;
            byte[] fileAsByteArray = GetByteArray(Path, model);
            Assert.IsNotNull(fileAsByteArray);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            Assert.AreEqual(wellType, ModelFileService.GetWellModelType(modelFile), "Model File type mismatch");
            //First File
            ModelFileService.AddWellModelFile(modelFile);
            //Second File
            ModelFileService.AddWellModelFile(modelFile);
            //Third File
            ModelFileService.AddWellModelFile(modelFile);

            ModelFileHeaderDTO[] modelFiles = ModelFileService.GetListModelFileHeaderByWell(well.Id.ToString());
            Assert.IsNotNull(modelFiles);
            Assert.IsTrue(modelFiles.Length == 3);
            foreach (ModelFileHeaderDTO modelFileHeader in modelFiles)
            {
                ModelFileBase64DTO returnedModelFile = ModelFileService.GetBase64ModelFile(modelFileHeader.Id.ToString());
                Assert.IsNotNull(returnedModelFile);
                Assert.IsNotNull(returnedModelFile.Base64Contents);
                byte[] returnedByteArray = Convert.FromBase64String(returnedModelFile.Base64Contents);
                Assert.IsNotNull(returnedByteArray);
            }
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void AddUpdateModelFileOptions()
        {
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today.AddYears(-2), DataConnection = GetDefaultCygNetDataConnection(), WellType = WellTypeId.RRL }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);

            var modelFile = new ModelFileDTO();
            modelFile.WellId = well.Id;
            modelFile.ApplicableDate = Truncate(DateTime.UtcNow.AddDays(-1));
            modelFile.Contents = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            ModelFileService.AddModelFile(modelFile);

            ModelFileDTO modelCompare = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(modelCompare);

            var modelFileOptions = new ModelFileOptionDTO();
            modelFileOptions.ModelFileId = modelCompare.Id;
            modelFileOptions.CalibrationMethod = CalibrationMethodId.ReservoirPressure;
            modelFileOptions.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.PumpWearFactor) };
            modelFileOptions.Comment = "TEST";
            ModelFileService.SaveModelFileOptions(modelFileOptions);

            ModelFileHeaderDTO modelFileHeader = ModelFileService.GetCurrentModelHeader(well.Id.ToString());
            Assert.IsNotNull(modelFileHeader);
            Assert.IsNotNull(modelFileHeader.Options);
            Assert.IsTrue(modelFileHeader.Options.CalibrationMethod == CalibrationMethodId.ReservoirPressure);

            modelFileHeader.Options.CalibrationMethod = CalibrationMethodId.ReservoirPressureAndLFactor;
            ModelFileService.SaveModelFileOptions(modelFileHeader.Options);
            ModelFileHeaderDTO modelFileHeaderCompare = ModelFileService.GetCurrentModelHeader(well.Id.ToString());
            Assert.IsNotNull(modelFileHeaderCompare);
            Assert.IsNotNull(modelFileHeaderCompare.Options);
            Assert.IsTrue(modelFileHeader.Options.CalibrationMethod == CalibrationMethodId.ReservoirPressureAndLFactor);
        }

        //Adding two options PIP and PDP for ESP well
        public void AddtwoOptionsPIPandPDPForESP()

        {
            //Create a new well
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, CommissionDate = DateTime.Today, WellType = WellTypeId.ESP }) });
            var allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            WellDTO wellCompare = WellService.GetWell(well.Id.ToString());
            Assert.IsNotNull(wellCompare);
            Assert.AreEqual(well.Name, wellCompare.Name);
            //Uploading model file
            var modelFile = new ModelFileDTO();
            modelFile.WellId = well.Id;
            modelFile.ApplicableDate = Truncate(DateTime.UtcNow.AddDays(-1));
            modelFile.Contents = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            ModelFileService.AddModelFile(modelFile);
            ModelFileDTO model = ModelFileService.GetCurrentModelFile(well.Id.ToString());
            Assert.IsNotNull(model);

            ModelFileOptionDTO options = new ModelFileOptionDTO();
            options.Comment = "CASETest option delete test.";
            options.ModelFileId = model.Id;

            //select PIP and PDP
            PIPandPDPOption[] espPIPandPDP = ModelFileService.GetPIPandPDPOption(WellTypeId.ESP.ToString());
            Assert.AreEqual(2, espPIPandPDP.Count());
            Assert.IsTrue(espPIPandPDP.Contains(Enums.PIPandPDPOption.Has_PumpIntakePressure));
            Assert.IsTrue(espPIPandPDP.Contains(Enums.PIPandPDPOption.Has_PumpDischargePressure));
            ModelFileService.SaveModelFileOptions(options);
        }

        /// <summary>
        /// This test is related to IModelFileService.GetPIPandPDPOption(Sudhir)
        /// </summary>
        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void GetPIPandPDPOptionEsp()
        {
            PIPandPDPOption[] espPIPandPDP =
            ModelFileService.GetPIPandPDPOption(WellTypeId.ESP.ToString());
        }

        public WellConfigDTO AddWellWithModelFile(int addDate)
        {
            WellDTO well = SetDefaultFluidType(new WellDTO()
            {
                Name = "RPOC_0001",
                FacilityId = "RPOC_0001",
                DataConnection = GetDefaultCygNetDataConnection(),
                CommissionDate = DateTime.Today.AddYears(-2),
                AssemblyAPI = "RPOC_0001",
                SubAssemblyAPI = "RPOC_0001",
                WellType = WellTypeId.NF,
                GasAllocationGroup = null,
                OilAllocationGroup = null,
                WaterAllocationGroup = null
            });
            WellConfigDTO wellConfig = new WellConfigDTO();
            wellConfig.Well = well;
            wellConfig.ModelConfig = ReturnBlankModel();
            //Well
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfig);
            Assert.IsNotNull(addedWellConfig);
            Assert.IsNotNull(addedWellConfig.Well);
            _wellsToRemove.Add(addedWellConfig.Well);

            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            byte[] fileAsByteArray;
            ModelFileValidationDataDTO ModelFileValidationData;
            AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(addedWellConfig.Well.Id.ToString());
            ModelFileOptionDTO options = new ModelFileOptionDTO();
            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };
            modelFile.ApplicableDate = addedWellConfig.Well.CommissionDate.Value.AddDays(addDate).ToUniversalTime();
            Trace.WriteLine($"Model file applicable date is {DTOExtensions.ToISO8601(modelFile.ApplicableDate)}.");
            modelFile.WellId = addedWellConfig.Well.Id;
            fileAsByteArray = GetByteArray(Path, "WellfloNFWExample1.wflx");
            options.CalibrationMethod = CalibrationMethodId.ReservoirPressure;
            options.Comment = "NF";
            options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            modelFile.Options = options;
            ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
            if (ModelFileValidationData != null)
                ModelFileService.AddWellModelFile(modelFile);
            else
                Assert.Fail(string.Format("Failed to validate NF model file"));

            return addedWellConfig;
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void ModelFileValidation()
        {
            WellConfigDTO wellConfig = AddWellWithModelFile(-1);
            ModelFileDTO modelFile = ModelFileService.GetCurrentModelFile(wellConfig.Well.Id.ToString());
            Assert.IsNull(modelFile, "Model file can be saved before the well commission date.");
        }

        [TestCategory(TestCategories.ModelFileTests), TestMethod]
        public void GetCurrentModelFileWellType()
        {
            WellConfigDTO wellConfig = AddWellWithModelFile(1);
            ModelFileDTO modelFile = ModelFileService.GetCurrentModelFile(wellConfig.Well.Id.ToString());
            Assert.IsNotNull(modelFile, "Unable to Save model file");
            Assert.AreEqual(wellConfig.Well.WellType, modelFile.WellType, "Mismatch between model file & WellConfig well type");

            //Change MOP
            WellMoPHistoryDTO wellMoPHistoryDTO = new WellMoPHistoryDTO();
            wellMoPHistoryDTO.WellId = wellConfig.Well.Id;
            wellMoPHistoryDTO.WellType = WellTypeId.RRL;
            wellMoPHistoryDTO.ChangeDate = wellConfig.Well.CommissionDate.Value.AddDays(10).ToUniversalTime();
            wellMoPHistoryDTO.Comment = "Change Well type to RRL";
            WellService.ChangeWellType(wellMoPHistoryDTO);

            modelFile = ModelFileService.GetCurrentModelFile(wellConfig.Well.Id.ToString());
            Assert.IsNull(modelFile, "Previous well type Model file retrieved");
        }
    }
}