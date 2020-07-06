using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using System.Reflection;


namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class WellConfigurationServiceTests : APIClientTestBase
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

        public ModelConfigDTO EmptyDownHoleRods()
        {
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

            //Empty Downhole
            DownholeConfigDTO SampleDownholeConfig = new DownholeConfigDTO();
            //SampleDownholeConfig.WellId = null;
            SampleDownholeConfig.PumpDiameter = 0;
            SampleDownholeConfig.PumpDepth = 0;
            SampleDownholeConfig.TubingID = 0.0;
            SampleDownholeConfig.TubingOD = 0.00;
            SampleDownholeConfig.TubingAnchorDepth = 0;
            SampleDownholeConfig.CasingOD = 0.00;
            SampleDownholeConfig.CasingWeight = 0;
            SampleDownholeConfig.TopPerforation = 0.0;
            SampleDownholeConfig.BottomPerforation = 0.0;

            //Empty Rods
            RodStringConfigDTO SampleRodsConfig = new RodStringConfigDTO();
            SampleRodsConfig.TotalRodLength = 0;
            RodTaperConfigDTO[] RodTaperArray = Array.Empty<RodTaperConfigDTO>();
            SampleRodsConfig.RodTapers = RodTaperArray;

            //Model File
            ModelConfigDTO SampleModel = new ModelConfigDTO();
            //SampleModel.WellId = well.Id.ToString();
            SampleModel.Weights = SampleWeightsConfig;
            SampleModel.Rods = SampleRodsConfig;
            SampleModel.Downhole = SampleDownholeConfig;
            SampleModel.Surface = SampleSurfaceConfig;

            return SampleModel;
        }

        [TestCategory(TestCategories.WellConfigurationServiceTests), TestMethod]
        public void AddWellWithTransaction()
        {
            var allDataConnectionsBefore = DataConnectionService.GetAllDataConnections().ToList();
            var allWellsBefore = WellService.GetAllWells().ToList();

            try
            {
                //Missing Well Name to fail the well add
                var toAdd = SetDefaultFluidType(new WellDTO() { FacilityId = "CASETEST", SurfaceLatitude = 1.232m, SurfaceLongitude = 2.3232m, IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL });
                toAdd.DataConnection = new DataConnectionDTO() { ProductionDomain = "5412", Site = "CASESRV", Service = s_cvsService };
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = toAdd });
            }
            catch
            {
                var allDataConnections = DataConnectionService.GetAllDataConnections().ToList();
                var allWells = WellService.GetAllWells().ToList();
                Assert.AreEqual(allDataConnectionsBefore.Count, allDataConnections.Count);
                Assert.AreEqual(allWellsBefore.Count, allWells.Count);
            }
        }

        //06/25 commenting this test to check its impact on other tests
        // [TestCategory(TestCategories.WellConfigurationServiceTests), TestMethod]
        public void AddWellConfigWithTransaction()
        {
            WellDTO well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today, AssemblyAPI = "1234567890", SubAssemblyAPI = "123456789012", IntervalAPI = "12345678901234", WellType = WellTypeId.RRL });
            WellDTO wellBefore = WellService.GetWellByName(well.Name);
            Assert.AreEqual(null, wellBefore, "AddWellConfigWithTransaction failed because well '{0}' already exists.", well.Name);
            WellConfigDTO wellConfigDTO = new WellConfigDTO();
            wellConfigDTO.Well = well;
            wellConfigDTO.ModelConfig = ReturnFullyPopulatedModel(); // test fully configured model
            wellConfigDTO.Well.CommissionDate = null; //Try to make the SaveWellboreComponents function fail
            try
            {
                WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
                Assert.Fail("Adding well should throw an exception.");
            }
            catch
            {
                WellDTO wellAfter = WellService.GetWellByName(well.Name);
                Assert.AreEqual(null, wellAfter, "Should have failed to add a well in this test.");
            }
            finally
            {
                // If we fail to fail to add the well make sure it is removed.
                WellDTO wellAfter = WellService.GetWellByName(well.Name);
                if (wellAfter != null)
                {
                    _wellsToRemove.Add(wellAfter);
                }
            }
        }

        [TestCategory(TestCategories.WellConfigurationServiceTests), TestMethod]
        public void AddGetNewWellConfigFullModel()
        {

            string facilityId = s_isRunningInATS ? "RPOC_" + "00001" : "RPOC_" + "0001";

            bool dcExisted = false;
            try
            {
                WellDTO well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today, AssemblyAPI = "1234567890", SubAssemblyAPI = "123456789012", IntervalAPI = "12345678901234", WellType = WellTypeId.RRL });
                dcExisted = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == well.DataConnection.ProductionDomain && dc.Site == well.DataConnection.Site && dc.Service == well.DataConnection.Service) != null;
                WellDTO[] wells = WellService.GetAllWells();
                Assert.IsFalse(wells.Any(w => w.Name.Equals(well.Name)), "Well already exists in database.");
                WellConfigDTO wellConfigDTO = new WellConfigDTO();
                wellConfigDTO.Well = well;
                wellConfigDTO.ModelConfig = ReturnFullyPopulatedModel(); // test fully configured model
                WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
                wells = WellService.GetAllWells();
                WellDTO getwell = wells.FirstOrDefault(w => w.Name == well.Name);
                _wellsToRemove.Add(getwell);
                if (!dcExisted)
                {
                    _dataConnectionsToRemove.Add(getwell.DataConnection);
                }
                WellConfigDTO GottenBackwellconfig = WellConfigurationService.GetWellConfig(getwell.Id.ToString());
                GottenBackwellconfig.ModelConfig.Downhole.CasingOD = 13.00;
                bool status = WellConfigurationService.UpdateWellConfig(GottenBackwellconfig);
                Assert.AreEqual(true, status, "Status is not true , status returned as : " + status);
                WellConfigDTO ReturnedWellConfig = WellConfigurationService.GetWellConfig(getwell.Id.ToString());
                Assert.AreEqual(13.00, ReturnedWellConfig.ModelConfig.Downhole.CasingOD, "Not expected OD, OD is " + ReturnedWellConfig.ModelConfig.Downhole.CasingOD);
                //WellService.RemoveWell(getwell.Id.ToString());
                WellConfigurationService.RemoveWellConfig(getwell.Id.ToString());
                _wellsToRemove.Remove(getwell);
            }
            catch (Exception e)
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
                Assert.Fail("Exception : " + e.ToString() + (e.InnerException == null ? "" : Environment.NewLine + e.InnerException.ToString()));
            }
        }



        [TestCategory(TestCategories.WellConfigurationServiceTests), TestMethod]
        public void AddGetNewWellConfigBlankModel()
        {

            string facilityId = s_isRunningInATS ? "RPOC_" + "00001" : "RPOC_" + "0001";
            bool dcExisted = false;
            try
            {
                WellDTO well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today, AssemblyAPI = "1234567890", SubAssemblyAPI = "123456789012", IntervalAPI = "12345678901234", WellType = WellTypeId.RRL });
                dcExisted = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == well.DataConnection.ProductionDomain && dc.Site == well.DataConnection.Site && dc.Service == well.DataConnection.Service) != null;
                WellDTO[] wells = WellService.GetAllWells();
                Assert.IsFalse(wells.Any(w => w.Name.Equals(well.Name)), "Well already exists in database.");
                WellConfigDTO wellConfigDTO = new WellConfigDTO();
                wellConfigDTO.Well = well;
                wellConfigDTO.ModelConfig = ReturnBlankModel();
                WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
                wells = WellService.GetAllWells();
                WellDTO getwell = wells.FirstOrDefault(w => w.Name == well.Name);
                _wellsToRemove.Add(getwell);
                if (!dcExisted)
                {
                    _dataConnectionsToRemove.Add(getwell.DataConnection);
                }
                WellConfigDTO GottenBackwellconfig = WellConfigurationService.GetWellConfig(getwell.Id.ToString());
                GottenBackwellconfig.ModelConfig.Downhole.CasingOD = 12.00;
                bool status = WellConfigurationService.UpdateWellConfig(GottenBackwellconfig);
                Assert.AreEqual(true, status, "Status is not true , status returned as : " + status);
                WellConfigDTO ReturnedWellConfig = WellConfigurationService.GetWellConfig(getwell.Id.ToString());
                Assert.AreEqual(12.00, ReturnedWellConfig.ModelConfig.Downhole.CasingOD, "Not expected OD, OD is " + ReturnedWellConfig.ModelConfig.Downhole.CasingOD);
                //TODO add more asserts
                //WellService.RemoveWell(getwell.Id.ToString());
                WellConfigurationService.RemoveWellConfig(getwell.Id.ToString());
                _wellsToRemove.Remove(getwell);
            }
            catch (Exception e)
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
                Assert.Fail("Exception : " + e.ToString() + (e.InnerException == null ? "" : Environment.NewLine + e.InnerException.ToString()));
            }
        }

        [TestCategory(TestCategories.WellConfigurationServiceTests), TestMethod]
        public void AddGetNewWellConfigPartialModel()
        {
            string facilityId = s_isRunningInATS ? "RPOC_" + "00001" : "RPOC_" + "0001";

            bool dcExisted = false;
            try
            {
                WellDTO well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today, AssemblyAPI = "1234567890", SubAssemblyAPI = "123456789012", IntervalAPI = "12345678901234", WellType = WellTypeId.RRL });
                dcExisted = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == well.DataConnection.ProductionDomain && dc.Site == well.DataConnection.Site && dc.Service == well.DataConnection.Service) != null;
                WellDTO[] wells = WellService.GetAllWells();
                Assert.IsFalse(wells.Any(w => w.Name.Equals(well.Name)), "Well already exists in database.");
                WellConfigDTO wellConfigDTO = new WellConfigDTO();
                wellConfigDTO.Well = well;
                wellConfigDTO.ModelConfig = ReturnPartialPopulatedModel(); // test partially configured model
                WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
                wells = WellService.GetAllWells();
                WellDTO getwell = wells.FirstOrDefault(w => w.Name == well.Name);
                _wellsToRemove.Add(getwell);
                if (!dcExisted)
                {
                    _dataConnectionsToRemove.Add(getwell.DataConnection);
                }
                WellConfigDTO GottenBackwellconfig = WellConfigurationService.GetWellConfig(getwell.Id.ToString());
                GottenBackwellconfig.ModelConfig.Downhole.CasingOD = 14.00;
                bool status = WellConfigurationService.UpdateWellConfig(GottenBackwellconfig);
                Assert.AreEqual(true, status, "Status is not true , status returned as : " + status);
                WellConfigDTO ReturnedWellConfig = WellConfigurationService.GetWellConfig(getwell.Id.ToString());
                Assert.AreEqual(14.00, ReturnedWellConfig.ModelConfig.Downhole.CasingOD, "Not expected OD, OD is " + ReturnedWellConfig.ModelConfig.Downhole.CasingOD);
                //TODO add more asserts
                //WellService.RemoveWell(getwell.Id.ToString());
                WellConfigurationService.RemoveWellConfig(getwell.Id.ToString());
                _wellsToRemove.Remove(getwell);
            }
            catch (Exception e)
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
                Assert.Fail("Exception : " + e.ToString() + (e.InnerException == null ? "" : Environment.NewLine + e.InnerException.ToString()));
            }
        }

        [TestCategory(TestCategories.WellConfigurationServiceTests), TestMethod]
        public void AddNewWellConfigGeneratesLotsOfNewWellboreData()
        {

            string facilityId = s_isRunningInATS ? "RPOC_" + "00001" : "RPOC_" + "0001";

            bool dcExisted = false;
            try
            {
                WellDTO well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today, AssemblyAPI = "1234567890", SubAssemblyAPI = "123456789012", IntervalAPI = "12345678901234", WellType = WellTypeId.RRL });
                dcExisted = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == well.DataConnection.ProductionDomain && dc.Site == well.DataConnection.Site && dc.Service == well.DataConnection.Service) != null;
                WellDTO[] wells = WellService.GetAllWells();
                Assert.IsFalse(wells.Any(w => w.Name.Equals(well.Name)), "Well already exists in database.");
                WellConfigDTO wellConfigDTO = new WellConfigDTO();
                wellConfigDTO.Well = well;
                wellConfigDTO.ModelConfig = ReturnFullyPopulatedModel(); // test fully configured model
                WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
                wells = WellService.GetAllWells();
                WellDTO getwell = wells.FirstOrDefault(w => w.Name == well.Name);
                _wellsToRemove.Add(getwell);
                if (!dcExisted)
                {
                    _dataConnectionsToRemove.Add(getwell.DataConnection);
                }

                // Obtain and assert assembly data
                AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(getwell.Id.ToString());
                Assert.AreNotEqual(0, assembly.Id, "Assembly Id should not be zero.");
                //As per API 10-12-14 implementation, Assembly name & Well name won't be same. Wellbore Id provided on Ui will be assembly Id.
                Assert.AreEqual(well.AssemblyAPI, assembly.Name, string.Format("Assembly name '{0}' does not equal Well name '{1}'.", assembly.Name, getwell.Name));

                // Obtain and assert report data
                ReportDTO report = WellboreComponentService.GetReportByWellId(getwell.Id.ToString());
                Assert.IsTrue(EqualAssemblyDTOs(report.Assembly, assembly), string.Format("Report Assembly Id '{0}' does not correspond to Assembly Id '{1}'.", report.Id.ToString(), assembly.Id.ToString()));
                Assert.AreEqual(assembly.Id, report.AssemblyId, "Report assembly id is not correct.");
                Assert.AreEqual("Create new well", report.Comment, string.Format("Report Id '{0}' comment has unexpected value.", report.Id.ToString()));
                Assert.IsNull(report.FailedComponents, string.Format("Report Id '{0}' FailedComponents should be initialized to null.", report.Id.ToString()));
                Assert.AreNotEqual(0, report.Id, "Report Id should not be zero.");
                Assert.AreNotEqual(0, report.JobReason.Id, "Report JobReason Id should not be zero.");
                Assert.AreEqual("Data Imported", report.JobReason.JobReason, "Report JobReason should be 'Data Imported'.");
                Assert.AreNotEqual(0, report.JobReasonId, "Report JobReasonId should not be zero.");
                Assert.AreEqual("1st Time Data Import", report.JobType.JobType, "Report JobType should be '1st Time Data Import'.");
                Assert.AreNotEqual(0, report.JobTypeId, "Report JobTypeId should not be zero.");
                Assert.IsNull(report.ReportType, string.Format("Report Id '{0}' ReportType should be initialized to null.", report.Id.ToString()));
                Assert.AreNotEqual(0, report.ReportTypeId, "Report ReportTypeId should not be zero.");
                Assert.AreEqual(0, report.WellId, string.Format("Report Id '{0}' WellId should be initialized to 0.", report.Id));

                // Obtain and assert subassembly data
                SubAssemblyDTO[] subAssemblies = WellboreComponentService.GetSubAssembliesByWellId(getwell.Id.ToString());
                subAssemblies.ToList().ForEach(s => Assert.AreEqual(assembly.Id, s.Assembly.Id, string.Format("SubAssembly Id '{0}' does not correspond to Assembly Id '{1}'.", s.Assembly.Id.ToString(), assembly.Id.ToString())));

                AssemblyComponentDTO[] assemblyComponents = WellboreComponentService.GetAssemblyComponentsByWellId(getwell.Id.ToString());
                //assemblyComponents.ToList().ForEach(ac => Assert.IsTrue(ac.ReportId.Equals(assembly.Id), string.Format("Assembly Component Report Id '{0}' does not correspond to Assembly Id '{1}'.", ac.AssemblyId, assembly.Id.ToString())));

                ComponentDTO[] components = WellboreComponentService.GetComponentsByWellId(getwell.Id.ToString());
                components.ToList().ForEach(c => Assert.IsTrue(assemblyComponents.Any(a => a.CmcPrimaryKey.Equals(c.Id)), string.Format("Component Assembly Id '{0}' does not correspond to Assembly Id '{1}'.", c.APIDescription, assembly.Id.ToString())));

                WellConfigDTO GottenBackwellconfig = WellConfigurationService.GetWellConfig(getwell.Id.ToString());
                GottenBackwellconfig.Well.Name = "CASETestUpdated";
                bool status = WellConfigurationService.UpdateWellConfig(GottenBackwellconfig);
                Assert.AreEqual(true, status, "Status is not true , status returned as : " + status);

                report = WellboreComponentService.GetReportByWellId(getwell.Id.ToString());
                subAssemblies = WellboreComponentService.GetSubAssembliesByWellId(getwell.Id.ToString());
                assemblyComponents = WellboreComponentService.GetAssemblyComponentsByWellId(getwell.Id.ToString());
                components = WellboreComponentService.GetComponentsByWellId(getwell.Id.ToString());

                WellConfigDTO ReturnedWellConfig = WellConfigurationService.GetWellConfig(getwell.Id.ToString());
                Assert.AreEqual("CASETestUpdated", ReturnedWellConfig.Well.Name, "Expected well name is should be 'CASETestUpdated'");
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch (Exception e)
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
                Assert.Fail("Exception : " + e.ToString() + (e.InnerException == null ? "" : Environment.NewLine + e.InnerException.ToString()));
            }
        }

        [TestCategory(TestCategories.WellConfigurationServiceTests), TestMethod]
        public void AddNewWellConfigGeneratesNewWellboreDataWithPartialWellModel()
        {

            string facilityId = s_isRunningInATS ? "RPOC_" + "00001" : "RPOC_" + "0001";
            bool dcExisted = false;
            try
            {
                WellDTO well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today, AssemblyAPI = "1234567890", SubAssemblyAPI = "123456789012", IntervalAPI = "12345678901234", WellType = WellTypeId.RRL });
                dcExisted = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == well.DataConnection.ProductionDomain && dc.Site == well.DataConnection.Site && dc.Service == well.DataConnection.Service) != null;
                WellDTO[] wells = WellService.GetAllWells();
                Assert.IsFalse(wells.Any(w => w.Name.Equals(well.Name)), "Well already exists in database.");
                WellConfigDTO wellConfigDTO = new WellConfigDTO();
                wellConfigDTO.Well = well;
                wellConfigDTO.ModelConfig = ReturnFullyPopulatedModel(); // test fully configured model
                WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
                wells = WellService.GetAllWells();
                WellDTO getwell = wells.FirstOrDefault(w => w.Name == well.Name);
                _wellsToRemove.Add(getwell);
                if (!dcExisted)
                {
                    _dataConnectionsToRemove.Add(getwell.DataConnection);
                }

                // Obtain and assert assembly data
                AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(getwell.Id.ToString());
                Assert.AreNotEqual(0, assembly.Id, "Assembly Id should not be zero.");
                //As per API 10-12-14 implementation, Assembly name & Well name won't be same. Wellbore Id provided on Ui will be assembly Id.
                Assert.AreEqual(well.AssemblyAPI, assembly.Name, string.Format("Assembly name '{0}' does not equal Well name '{1}'.", assembly.Name, getwell.Name));

                // Obtain and assert report data
                ReportDTO report = WellboreComponentService.GetReportByWellId(getwell.Id.ToString());
                Assert.IsTrue(EqualAssemblyDTOs(report.Assembly, assembly), string.Format("Report Assembly Id '{0}' does not correspond to Assembly Id '{1}'.", report.Id.ToString(), assembly.Id.ToString()));
                Assert.AreEqual(assembly.Id, report.AssemblyId, "Report assembly id is not correct.");
                Assert.AreEqual("Create new well", report.Comment, string.Format("Report Id '{0}' comment has unexpected value.", report.Id.ToString()));
                Assert.IsNull(report.FailedComponents, string.Format("Report Id '{0}' FailedComponents should be initialized to null.", report.Id.ToString()));
                Assert.AreNotEqual(0, report.Id, "Report Id should not be zero.");
                Assert.AreNotEqual(0, report.JobReason.Id, "Report JobReason Id should not be zero.");
                Assert.AreEqual("Data Imported", report.JobReason.JobReason, "Report JobReason should be 'Data Imported'.");
                Assert.AreNotEqual(0, report.JobReasonId, "Report JobReasonId should not be zero.");
                Assert.AreEqual("1st Time Data Import", report.JobType.JobType, "Report JobType should be '1st Time Data Import'.");
                Assert.AreNotEqual(0, report.JobTypeId, "Report JobTypeId should not be zero.");
                Assert.IsNull(report.ReportType, string.Format("Report Id '{0}' ReportType should be initialized to null.", report.Id.ToString()));
                Assert.AreNotEqual(0, report.ReportTypeId, "Report ReportTypeId should not be zero.");
                Assert.AreEqual(0, report.WellId, string.Format("Report Id '{0}' WellId should be initialized to 0.", report.Id));

                // Obtain and assert subassembly data
                SubAssemblyDTO[] subAssemblies = WellboreComponentService.GetSubAssembliesByWellId(getwell.Id.ToString());
                subAssemblies.ToList().ForEach(s => Assert.AreEqual(assembly.Id, s.Assembly.Id, string.Format("SubAssembly Id '{0}' does not correspond to Assembly Id '{1}'.", s.Assembly.Id.ToString(), assembly.Id.ToString())));

                AssemblyComponentDTO[] assemblyComponents = WellboreComponentService.GetAssemblyComponentsByWellId(getwell.Id.ToString());
                //assemblyComponents.ToList().ForEach(ac => Assert.IsTrue(ac.ReportId.Equals(assembly.Id), string.Format("Assembly Component Report Id '{0}' does not correspond to Assembly Id '{1}'.", ac.AssemblyId, assembly.Id.ToString())));

                ComponentDTO[] components = WellboreComponentService.GetComponentsByWellId(getwell.Id.ToString());
                components.ToList().ForEach(c => Assert.IsTrue(assemblyComponents.Any(a => a.CmcPrimaryKey.Equals(c.Id)), string.Format("Component Assembly Id '{0}' does not correspond to Assembly Id '{1}'.", c.APIDescription, assembly.Id.ToString())));

                WellConfigDTO GottenBackwellconfig = WellConfigurationService.GetWellConfig(getwell.Id.ToString());
                GottenBackwellconfig.ModelConfig.Downhole.CasingOD = 13.50;
                bool status = WellConfigurationService.UpdateWellConfig(GottenBackwellconfig);
                Assert.AreEqual(true, status, "Status is not true , status returned as : " + status);
                WellConfigDTO ReturnedWellConfig = WellConfigurationService.GetWellConfig(getwell.Id.ToString());
                Assert.AreEqual(13.50, ReturnedWellConfig.ModelConfig.Downhole.CasingOD, "Not expected OD, OD is " + ReturnedWellConfig.ModelConfig.Downhole.CasingOD);
            }
            catch (AssertFailedException)
            {
                throw;
            }
            catch (Exception e)
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
                Assert.Fail("Exception : " + e.ToString() + (e.InnerException == null ? "" : Environment.NewLine + e.InnerException.ToString()));
            }
        }


        [TestCategory(TestCategories.WellConfigurationServiceTests), TestMethod]
        public void GroupConfigTest_RRL()
        {
            List<WellConfigDTO> wells = new List<WellConfigDTO>();

            //Adding Well1
            string facilityId1 = GetFacilityId("RPOC_", 1);
            WellDTO well1 = SetDefaultFluidType(new WellDTO() { Name = facilityId1, FacilityId = facilityId1, DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today - TimeSpan.FromDays(3), AssemblyAPI = "1234567890", SubAssemblyAPI = "123456789012", IntervalAPI = "12345678901234", WellType = WellTypeId.RRL });
            well1.WellStatusId = WellConfigurationService.GetReferenceTableItems("r_WellStatus").ElementAt(1).Id;
            well1.SurfaceLatitude = 19.076090m;
            well1.SurfaceLongitude = 72.877426m;
            well1.WellDepthDatumId = 2;
            well1.WellDepthDatumElevation = 100;
            well1.WellGroundElevation = 2000;
            well1.DepthCorrectionFactor = 1.1m;
            well1.Lease = "Lease Name1";
            well1.Field = "Field Name1";
            well1.Engineer = "Engineer Name1";
            well1.GeographicRegion = "Geographic Region1";
            well1.Foreman = "Foreman Name1";
            well1.GaugerBeat = "Gauger Beat1";
            WellConfigDTO wellConfigDTO1 = new WellConfigDTO();
            wellConfigDTO1.Well = well1;
            wellConfigDTO1.ModelConfig = ReturnFullyPopulatedModel();
            WellConfigDTO addedWellConfig1 = WellConfigurationService.AddWellConfig(wellConfigDTO1);
            _wellsToRemove.Add(addedWellConfig1.Well);
            wells.Add(addedWellConfig1);
            Trace.WriteLine(facilityId1 + " Added Successfully");

            //Adding Well2
            string facilityId2 = GetFacilityId("RPOC_", 2);
            WellDTO well2 = SetDefaultFluidType(new WellDTO() { Name = facilityId2, CommissionDate = DateTime.Today, AssemblyAPI = "1234567891", SubAssemblyAPI = "123456789013", IntervalAPI = "12345678901235", WellType = WellTypeId.RRL });
            well2.WellStatusId = WellConfigurationService.GetReferenceTableItems("r_WellStatus").ElementAt(2).Id;
            well2.SurfaceLatitude = 29.076090m;
            well2.SurfaceLongitude = 62.877426m;
            well2.WellDepthDatumId = 4;
            well2.WellDepthDatumElevation = 10;
            well2.WellGroundElevation = 1000;
            well2.DepthCorrectionFactor = 1.05m;
            WellConfigDTO wellConfigDTO2 = new WellConfigDTO();
            wellConfigDTO2.Well = well2;
            wellConfigDTO2.ModelConfig = ReturnFullyPopulatedModel();
            WellConfigDTO addedWellConfig2 = WellConfigurationService.AddWellConfig(wellConfigDTO2);
            _wellsToRemove.Add(addedWellConfig2.Well);
            wells.Add(addedWellConfig2);
            Trace.WriteLine(facilityId2 + " Added Successfully");
            Trace.WriteLine("");

            //Calling GetWellGroupConfigurationUoM API
            WellFilterDTO welFilter = new WellFilterDTO();
            UnitsValuesCollectionDTO<WellConfigUnitDTO, WellConfigDTO> responseGroupConfig = WellConfigurationService.GetWellGroupConfigurationUoM(welFilter, WellTypeId.RRL.ToString(), false.ToString());

            Assert.AreEqual(2, responseGroupConfig.Values.Count(), "Data retrieved for Incorrect number of wells");

            var response = responseGroupConfig.Values as WellConfigDTO[];

            for (int i = 0; i < wells.Count; i++)
            {
                Trace.WriteLine("Starting Verification of Well : " + wells.ElementAt(i).Well.Name);
                VerifyGetWellGroupConfigResponse(wells.ElementAt(i), response[i], "true");
                Trace.WriteLine("----------------------Verification completed for well " + wells.ElementAt(i).Well.Name + "------------------------");
                Trace.WriteLine("");
            }

            //Now updating the WellConfigDTO of first well
            addedWellConfig1.Well.SurfaceLatitude = 39.076090m;
            addedWellConfig1.Well.SurfaceLongitude = 82.877426m;
            addedWellConfig1.Well.CommissionDate = DateTime.Today;
            addedWellConfig1.Well.WellDepthDatumId = 4;
            addedWellConfig1.Well.WellDepthDatumElevation = 10;
            addedWellConfig1.Well.WellGroundElevation = 1000;
            addedWellConfig1.Well.DepthCorrectionFactor = 1.05m;
            addedWellConfig1.Well.Lease = "Lease Name11";
            addedWellConfig1.Well.Field = "Field Name11";
            addedWellConfig1.Well.Engineer = "Engineer Name11";
            addedWellConfig1.Well.GeographicRegion = "Geographic Region11";
            addedWellConfig1.Well.Foreman = "Foreman Name11";
            addedWellConfig1.Well.GaugerBeat = "Gauger Beat11";
            Trace.WriteLine(addedWellConfig1.Well.Name + " is updated with different configuration");

            //Now updating the WellConfigDTO of second well
            addedWellConfig2.Well.CommissionDate = DateTime.Today - TimeSpan.FromDays(3);
            addedWellConfig2.Well.WellDepthDatumId = 2;
            addedWellConfig2.Well.WellDepthDatumElevation = 100;
            addedWellConfig2.Well.WellGroundElevation = 2000;
            addedWellConfig2.Well.DepthCorrectionFactor = 1.1m;
            addedWellConfig2.ModelConfig.Downhole.PumpDepth = 6000;
            addedWellConfig2.ModelConfig.Downhole.TubingOD = 3;
            addedWellConfig2.ModelConfig.Downhole.TubingID = 2.75;
            addedWellConfig2.ModelConfig.Downhole.TubingAnchorDepth = 150;
            addedWellConfig2.ModelConfig.Downhole.CasingOD = 3.5;
            addedWellConfig2.ModelConfig.Downhole.CasingWeight = 400;
            addedWellConfig2.ModelConfig.Downhole.TopPerforation = 300;
            addedWellConfig2.ModelConfig.Downhole.BottomPerforation = 350;
            addedWellConfig2.ModelConfig.Weights.CBT = 1900;
            Trace.WriteLine(addedWellConfig2.Well.Name + " is updated with different configuration");

            UnitsValuesCollectionDTO<WellConfigUnitDTO, WellConfigDTO> updateGroupConfig = new UnitsValuesCollectionDTO<WellConfigUnitDTO, WellConfigDTO>();

            WellConfigDTO[] wellConfigs = { addedWellConfig1, addedWellConfig2 };
            updateGroupConfig.Units = responseGroupConfig.Units;
            updateGroupConfig.Values = wellConfigs;
            //Calling UpdateWellConfigurationUOM method
            WellConfigurationService.UpdateWellConfigurationsUoM(updateGroupConfig, false.ToString());
            Trace.WriteLine("Updated Well Configuration");

            //Calling GetWellGroupConfigurationUoM API after update
            UnitsValuesCollectionDTO<WellConfigUnitDTO, WellConfigDTO> responseGroupConfig1 = WellConfigurationService.GetWellGroupConfigurationUoM(welFilter, WellTypeId.RRL.ToString(), false.ToString());

            Assert.AreEqual(2, responseGroupConfig1.Values.Count(), "Data retrieved for Incorrect number of wells");

            var response1 = responseGroupConfig1.Values as WellConfigDTO[];

            for (int i = 0; i < wells.Count; i++)
            {
                Trace.WriteLine("Starting Verification of Well : " + wells.ElementAt(i).Well.Name + " after update");
                VerifyGetWellGroupConfigResponse(wells.ElementAt(i), response1[i], "true");
                Trace.WriteLine("----------------------Verification completed for well " + wells.ElementAt(i).Well.Name + "------------------------");
                Trace.WriteLine("");
            }

        }

        [TestCategory(TestCategories.WellConfigurationServiceTests), TestMethod]
        public void GroupConfigTest_NonRRL()
        {
            List<WellConfigDTO> wells = new List<WellConfigDTO>();

            //Adding Well1
            string facilityId1 = GetFacilityId("ESPWELL_", 1);
            WellDTO well1 = SetDefaultFluidType(new WellDTO() { Name = facilityId1, FacilityId = facilityId1, DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today.AddYears(-4), AssemblyAPI = "1234567890", SubAssemblyAPI = "123456789012", IntervalAPI = "12345678901234", WellType = WellTypeId.ESP });
            well1.WellStatusId = WellConfigurationService.GetReferenceTableItems("r_WellStatus").ElementAt(1).Id;
            well1.SurfaceLatitude = 19.076090m;
            well1.SurfaceLongitude = 72.877426m;
            well1.WellDepthDatumId = 2;
            well1.WellDepthDatumElevation = 100;
            well1.WellGroundElevation = 2000;
            well1.DepthCorrectionFactor = 1.1m;
            well1.Lease = "Lease Name1";
            well1.Field = "Field Name1";
            well1.Engineer = "Engineer Name1";
            well1.GeographicRegion = "Geographic Region1";
            well1.Foreman = "Foreman Name1";
            well1.GaugerBeat = "Gauger Beat1";
            WellConfigDTO wellConfigDTO1 = new WellConfigDTO();
            wellConfigDTO1.Well = well1;
            wellConfigDTO1.ModelConfig = ReturnBlankModel();
            WellConfigDTO addedWellConfig1 = WellConfigurationService.AddWellConfig(wellConfigDTO1);
            _wellsToRemove.Add(addedWellConfig1.Well);
            Trace.WriteLine(facilityId1 + " Added Successfully");
            AddModelFile(addedWellConfig1.Well.CommissionDate.Value.AddDays(1), addedWellConfig1.Well.WellType, addedWellConfig1.Well.Id, CalibrationMethodId.ReservoirPressure);
            Trace.WriteLine("Model File added in Well: " + facilityId1);
            addedWellConfig1 = WellConfigurationService.GetWellConfig(addedWellConfig1.Well.Id.ToString());
            wells.Add(addedWellConfig1);


            //Adding Well2
            string facilityId2 = GetFacilityId("ESPWELL_", 2);
            WellDTO well2 = SetDefaultFluidType(new WellDTO() { Name = facilityId2, CommissionDate = DateTime.Today, AssemblyAPI = "1234567891", SubAssemblyAPI = "123456789013", IntervalAPI = "12345678901235", WellType = WellTypeId.ESP });
            well2.WellStatusId = WellConfigurationService.GetReferenceTableItems("r_WellStatus").ElementAt(2).Id;
            well2.SurfaceLatitude = 29.076090m;
            well2.SurfaceLongitude = 62.877426m;
            well2.WellDepthDatumId = 4;
            well2.WellDepthDatumElevation = 10;
            well2.WellGroundElevation = 1000;
            well2.DepthCorrectionFactor = 1.05m;
            WellConfigDTO wellConfigDTO2 = new WellConfigDTO();
            wellConfigDTO2.Well = well2;
            wellConfigDTO2.ModelConfig = ReturnBlankModel();
            WellConfigDTO addedWellConfig2 = WellConfigurationService.AddWellConfig(wellConfigDTO2);
            _wellsToRemove.Add(addedWellConfig2.Well);
            Trace.WriteLine(facilityId2 + " Added Successfully");
            AddModelFile(addedWellConfig2.Well.CommissionDate.Value, addedWellConfig2.Well.WellType, addedWellConfig2.Well.Id, CalibrationMethodId.ReservoirPressure);
            Trace.WriteLine("Model File added in Well: " + facilityId2);
            addedWellConfig2 = WellConfigurationService.GetWellConfig(addedWellConfig2.Well.Id.ToString());
            wells.Add(addedWellConfig2);
            Trace.WriteLine("");

            //Calling GetWellGroupConfigurationUoM API
            WellFilterDTO welFilter = new WellFilterDTO();
            UnitsValuesCollectionDTO<WellConfigUnitDTO, WellConfigDTO> responseGroupConfig = WellConfigurationService.GetWellGroupConfigurationUoM(welFilter, WellTypeId.ESP.ToString(), false.ToString());

            Assert.AreEqual(2, responseGroupConfig.Values.Count(), "Data retrieved for Incorrect number of wells");

            var response = responseGroupConfig.Values as WellConfigDTO[];

            for (int i = 0; i < wells.Count; i++)
            {
                Trace.WriteLine("Starting Verification of Well : " + wells.ElementAt(i).Well.Name);
                VerifyGetWellGroupConfigResponse(wells.ElementAt(i), response[i]);
                Trace.WriteLine("----------------------Verification completed for well " + wells.ElementAt(i).Well.Name + "------------------------");
                Trace.WriteLine("");
            }

            //Now updating the WellConfigDTO of first well
            addedWellConfig1.Well.CommissionDate = DateTime.Today;
            addedWellConfig1.Well.SurfaceLatitude = 39.076090m;
            addedWellConfig1.Well.SurfaceLongitude = 82.877426m;
            addedWellConfig1.Well.Lease = "Lease Name11";
            addedWellConfig1.Well.Field = "Field Name11";
            addedWellConfig1.Well.Engineer = "Engineer Name11";
            addedWellConfig1.Well.GeographicRegion = "Geographic Region11";
            addedWellConfig1.Well.Foreman = "Foreman Name11";
            addedWellConfig1.Well.GaugerBeat = "Gauger Beat11";
            Trace.WriteLine(addedWellConfig1.Well.Name + " is updated with different configuration");

            //Now updating the WellConfigDTO of second well
            addedWellConfig2.Well.CommissionDate = DateTime.Today.AddYears(-4);
            addedWellConfig2.Well.Lease = "Lease Name2";
            addedWellConfig2.Well.Field = "Field Name2";
            addedWellConfig2.Well.Engineer = "Engineer Name2";
            addedWellConfig2.Well.GeographicRegion = "Geographic Region2";
            addedWellConfig2.Well.Foreman = "Foreman Name2";
            addedWellConfig2.Well.GaugerBeat = "Gauger Beat2";
            Trace.WriteLine(addedWellConfig2.Well.Name + " is updated with different configuration");

            UnitsValuesCollectionDTO<WellConfigUnitDTO, WellConfigDTO> updateGroupConfig = new UnitsValuesCollectionDTO<WellConfigUnitDTO, WellConfigDTO>();

            WellConfigDTO[] wellConfigs = { addedWellConfig1, addedWellConfig2 };
            updateGroupConfig.Units = responseGroupConfig.Units;
            updateGroupConfig.Values = wellConfigs;
            //Calling UpdateWellConfigurationUOM method
            WellConfigurationService.UpdateWellConfigurationsUoM(updateGroupConfig, false.ToString());
            Trace.WriteLine("Updated Well Configuration");

            //Calling GetWellGroupConfigurationUoM API after update
            UnitsValuesCollectionDTO<WellConfigUnitDTO, WellConfigDTO> responseGroupConfig1 = WellConfigurationService.GetWellGroupConfigurationUoM(welFilter, WellTypeId.ESP.ToString(), false.ToString());

            Assert.AreEqual(2, responseGroupConfig1.Values.Count(), "Data retrieved for Incorrect number of wells");

            var response1 = responseGroupConfig1.Values as WellConfigDTO[];

            for (int i = 0; i < wells.Count; i++)
            {
                Trace.WriteLine("Starting Verification of Well : " + wells.ElementAt(i).Well.Name + " after update");
                VerifyGetWellGroupConfigResponse(wells.ElementAt(i), response1[i]);
                Trace.WriteLine("----------------------Verification completed for well " + wells.ElementAt(i).Well.Name + "------------------------");
                Trace.WriteLine("");
            }

        }


        public void VerifyGetWellGroupConfigResponse(WellConfigDTO reqWellConfig, WellConfigDTO resWellConfig, string RRL = "false")
        {
            Trace.WriteLine("Verifying WellDTO");
            ValidateInputOutput(reqWellConfig.Well, resWellConfig.Well);
            Trace.WriteLine("Verifying AssemblyDTO");
            ValidateInputOutput(reqWellConfig.Well.Assembly, resWellConfig.Well.Assembly);
            Trace.WriteLine("Verifying DataConnectionDTO");
            ValidateInputOutput(reqWellConfig.Well.DataConnection, resWellConfig.Well.DataConnection);
            if (RRL.ToUpper() == "TRUE")
            {
                Trace.WriteLine("Verifying DownholeDTO");
                ValidateInputOutput(reqWellConfig.ModelConfig.Downhole, resWellConfig.ModelConfig.Downhole);
                Trace.WriteLine("Verifying SurfaceDTO");
                ValidateInputOutput(reqWellConfig.ModelConfig.Surface, resWellConfig.ModelConfig.Surface);
                Trace.WriteLine("Verifying MotorSizeDTO");
                ValidateInputOutput(reqWellConfig.ModelConfig.Surface.MotorSize, resWellConfig.ModelConfig.Surface.MotorSize);
                Trace.WriteLine("Verifying PumpingUnitDTO");
                ValidateInputOutput(reqWellConfig.ModelConfig.Surface.PumpingUnit, resWellConfig.ModelConfig.Surface.PumpingUnit);
                Trace.WriteLine("Verifying SlipTorqueDTO");
                ValidateInputOutput(reqWellConfig.ModelConfig.Surface.SlipTorque, resWellConfig.ModelConfig.Surface.SlipTorque);
                Trace.WriteLine("Verifying WeightDTO");
                ValidateInputOutput(reqWellConfig.ModelConfig.Weights, resWellConfig.ModelConfig.Weights);
                Trace.WriteLine("Verifying Crank1_AuxiliaryDTO");
                ValidateInputOutput(reqWellConfig.ModelConfig.Weights.Crank_1_Auxiliary, resWellConfig.ModelConfig.Weights.Crank_1_Auxiliary);
                Trace.WriteLine("Verifying Crank1_PrimaryDTO");
                ValidateInputOutput(reqWellConfig.ModelConfig.Weights.Crank_1_Primary, resWellConfig.ModelConfig.Weights.Crank_1_Primary);
                Trace.WriteLine("Verifying Crank2_AuxiliaryDTO");
                ValidateInputOutput(reqWellConfig.ModelConfig.Weights.Crank_2_Auxiliary, resWellConfig.ModelConfig.Weights.Crank_2_Auxiliary);
                Trace.WriteLine("Verifying Crank2_PrimaryDTO");
                ValidateInputOutput(reqWellConfig.ModelConfig.Weights.Crank_2_Primary, resWellConfig.ModelConfig.Weights.Crank_2_Primary);
                Trace.WriteLine("Verifying CBT");
                Trace.WriteLine("Input Value " + Math.Round(reqWellConfig.ModelConfig.Weights.CBT, 1));
                Trace.WriteLine("Output Value " + Math.Round(resWellConfig.ModelConfig.Weights.CBT, 1));
                Assert.AreEqual(Math.Round(reqWellConfig.ModelConfig.Weights.CBT, 1), Math.Round(resWellConfig.ModelConfig.Weights.CBT, 1), "CBT value mismatch observed");
            }
        }


        public void ValidateInputOutput(object a, object b)
        {
            if ((a == null && b != null) || (a != null && b == null))
            {
                Assert.Fail("One of the object is null");
            }
            else if (a == null || b == null)
            {
                Trace.WriteLine("Both the objects are null");
            }
            else
            {
                PropertyInfo[] properties = a.GetType().GetProperties();

                foreach (PropertyInfo pi in properties)
                {
                    if (pi.CanWrite && !pi.PropertyType.Name.Contains("DTO") && pi.Name != "ChangeDate" && pi.Name != "ChangeUser" && pi.Name != "CrankRadii" && pi.Name != "StrokeLengthsAtPins" && pi.Name != "CBT" && pi.Name != "LagMDistance" && pi.Name != "MoPStartDate")
                    {
                        Trace.WriteLine("Property Name :- " + pi.Name);
                        object firstValue = pi.GetValue(a, null);
                        object secondValue = pi.GetValue(b, null);
                        Trace.WriteLine("Input Value :- " + firstValue);
                        Trace.WriteLine("Output Value :- " + secondValue);
                        if (firstValue == null && secondValue == null)
                        {

                        }
                        else if (firstValue == null || secondValue == null)
                        {
                            Assert.Fail(pi.Name + " is different in response");
                        }
                        else
                        {
                            Assert.IsTrue(firstValue.Equals(secondValue), pi.Name + " is different in response");
                        }
                        Trace.WriteLine("");

                    }

                }
            }
        }


        [TestCategory(TestCategories.WellConfigurationServiceTests), TestMethod]
        public void CheckUpdateWellConfig()
        {

            string facilityId = s_isRunningInATS ? "RPOC_" + "00001" : "RPOC_" + "0001";
            try
            {
                #region General, WellAttributes

                //General
                WellDTO well = SetDefaultFluidType(new WellDTO()
                {
                    Name = "BEAM - 0277",
                    FacilityId = facilityId,
                    DataConnection = GetDefaultCygNetDataConnection(),
                    WellType = WellTypeId.RRL,
                    Lease = "Lease_0277",
                    Foreman = "Foreman_0277",
                    Field = "Field_0277",
                    Engineer = "Engineer_0277",
                    GaugerBeat = "GaugerBeat_0277",
                    GeographicRegion = "GeographicRegion_0277",
                    welUserDef01 = "State_0277",
                    welUserDef02 = "User_0277",
                    SubAssemblyAPI = "SubAssemblyAPI_0277",
                    AssemblyAPI = "AssemblyAPI_0277",
                    CommissionDate = DateTime.Today.AddDays(-250),
                });
                WellDTO[] wells = WellService.GetAllWells();
                Assert.IsFalse(wells.Any(w => w.Name.Equals(well.Name)), "Well already exists in database.");
                WellConfigDTO wellConfigDTO = new WellConfigDTO();
                wellConfigDTO.Well = well;
                wellConfigDTO.ModelConfig = ReturnBlankModel();
                WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
                wells = WellService.GetAllWells();
                WellDTO getwell = wells.FirstOrDefault(w => w.Name == well.Name);
                WellConfigDTO GottenBackwellconfig = WellConfigurationService.GetWellConfig(getwell.Id.ToString());

                #endregion General, WellAttributes

                GottenBackwellconfig.ModelConfig = EmptyDownHoleRods();
                bool updateWellConfig = WellConfigurationService.UpdateWellConfig(GottenBackwellconfig);
                Assert.AreEqual(true, updateWellConfig, "updateWellConfig is not true , updateWellConfig returned as : " + updateWellConfig);

                WellConfigDTO afterUpadateConfig = WellConfigurationService.GetWellConfig(getwell.Id.ToString());
                afterUpadateConfig.ModelConfig = ReturnFullyPopulatedModel();
                bool secondUpdateConfig = WellConfigurationService.UpdateWellConfig(afterUpadateConfig);
                Assert.AreEqual(true, secondUpdateConfig, "secondUpdateConfig is not true , secondUpdateConfig returned as : " + secondUpdateConfig);

                WellConfigDTO aftersecUpadateConfig = WellConfigurationService.GetWellConfig(getwell.Id.ToString());
                aftersecUpadateConfig.ModelConfig.Downhole.PumpDepth = 5180;
                bool thirdUpdateConfig = WellConfigurationService.UpdateWellConfig(aftersecUpadateConfig);
                Assert.AreEqual(true, thirdUpdateConfig, "thirdUpdateConfig is not true , thirdUpdateConfig returned as : " + thirdUpdateConfig);

                //WellService.RemoveWell(getwell.Id.ToString());
                WellConfigurationService.RemoveWellConfig(getwell.Id.ToString());
                _wellsToRemove.Remove(getwell);
            }
            catch (Exception e)
            {
                if (!_wellsToRemove.Any())
                {
                    var addedWell = WellService.GetAllWells().FirstOrDefault(w => w.Name.Equals("BEAM - 0277"));
                    if (addedWell != null)
                    {
                        _wellsToRemove.Add(addedWell);
                    }
                }
                Assert.Fail("Exception : " + e.ToString() + (e.InnerException == null ? "" : Environment.NewLine + e.InnerException.ToString()));
            }
        }

        //  [TestCategory(TestCategories.WellConfigurationServiceTests), TestMethod]
        public void AddNewWellRollbackOnException()
        {
            const string GettysburgAddressIntro = "Four score and seven years ago our fathers brought forth on this continent, a new nation, conceived in Liberty, and dedicated to the proposition that all men are created equal.";
            string wellId = "0";

            try
            {
                // Create test well and Assert it has a unqiue name
                WellDTO well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today, AssemblyAPI = "1234567890", SubAssemblyAPI = "123456789012", IntervalAPI = "12345678901234", WellType = WellTypeId.RRL });
                WellDTO[] wells = WellService.GetAllWells();
                Assert.IsFalse(wells.Any(w => w.Name.Equals(well.Name)), "Well already exists in database.");

                // Create a well configuration and add the test well to it
                WellConfigDTO wellConfigDTO = new WellConfigDTO();
                wellConfigDTO.Well = well;
                wellId = well.Id.ToString();  //Needed in finally block to Assert components not saved

                // Create a model config and add it to the well configuration also
                wellConfigDTO.ModelConfig = ReturnFullyPopulatedModel(); // test fully configured model

                // Modify the well model, changing the last rod manufacturer name to an invalid value (field too long - varchar(100) in the DB)
                wellConfigDTO.ModelConfig.Rods.RodTapers[wellConfigDTO.ModelConfig.Rods.RodTapers.Count() - 1].Manufacturer = GettysburgAddressIntro;
                WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);  // should trigger an exception near the end of the process

                // If exception wasn't thrown in previous statement, this test fails
                Assert.Fail("Attempt to AddWellConfig did not thrown the expected exception.");
            }
            catch
            { }
            finally
            {
                // Assert that the well was not saved
                WellDTO[] wells = WellService.GetAllWells();
                WellDTO getwell = wells.FirstOrDefault(w => w.Name == DefaultWellName);
                Assert.IsNull(getwell, "Expected well to be null but was not.");
            }

            // Assert that no assembly exists for the unsaved test well
            try
            {
                AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(wellId);
                Assert.Fail("Attempt to GetAssemblyByWellId did not thrown the expected exception.");
            }
            catch
            { }

            // Assert that no report exists for the unsaved test well
            try
            {
                ReportDTO report = WellboreComponentService.GetReportByWellId(wellId);
                Assert.Fail("Attempt to GetReportByWellId did not thrown the expected exception.");
            }
            catch
            { }

            SubAssemblyDTO[] subAssemblies = WellboreComponentService.GetSubAssembliesByWellId(wellId);
            Assert.IsNull(subAssemblies, "Expected subAssemblies to be null but was not.");

            // Assert that no assemblyComponents exist for the unsaved test well
            AssemblyComponentDTO[] assemblyComponents = WellboreComponentService.GetAssemblyComponentsByWellId(wellId);
            Assert.IsNull(assemblyComponents, "Expected assemblyComponents to be null but was not.");

            // Assert that no components exist for the unsaved test well
            ComponentDTO[] components = WellboreComponentService.GetComponentsByWellId(wellId);
            Assert.IsNull(components, "Expected components to be null but was not.");

            WellConfigDTO wellConfig = WellConfigurationService.GetWellConfig(wellId);
            Assert.IsNotNull(wellConfig, "Expected wellConfig to not be null but it was.");
            Assert.IsNull(wellConfig.Well, "Expected well to be null but was not.");
            Assert.IsNull(wellConfig.ModelConfig, "Expected modelConfig to be null but was not.");
            Assert.IsNull(wellConfig.CommonModelConfig, "Expected commonmodelConfig to be null but was not.");
        }

        [TestCategory(TestCategories.WellConfigurationServiceTests), TestMethod]
        public void AddGetNonRRLWellConfig()
        {
            int i = 1;
            foreach (WellTypeId wellTypeId in (WellTypeId[])Enum.GetValues(typeof(WellTypeId)))
            {
                if (wellTypeId == WellTypeId.Unknown || wellTypeId == WellTypeId.RRL || wellTypeId == WellTypeId.All)
                {
                    continue;
                }
                WellDTO well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName + i, CommissionDate = DateTime.Today, WellType = wellTypeId });
                WellDTO wellBefore = WellService.GetWellByName(well.Name);
                Assert.AreEqual(null, wellBefore, "AddWellConfig failed because well '{0}' already exists.", well.Name);
                WellConfigDTO wellConfigDTO = new WellConfigDTO();
                wellConfigDTO.Well = well;
                WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
                _wellsToRemove.Add(addedWellConfig.Well);
                WellDTO wellAfter = WellService.GetWellByName(well.Name);
                Assert.IsNull(wellAfter.AssemblyAPI);
                Assert.IsNull(wellAfter.SubAssemblyAPI);
                Assert.IsNull(wellAfter.DataConnection);
                Assert.IsNotNull(wellAfter.AssemblyId); Trace.WriteLine("Well Name : " + wellAfter.Name + "Well Type: " + wellAfter.WellType + "Well Assembly id: " + wellAfter.AssemblyId);
                ModelFileDTO Model = ModelFileService.GetCurrentModelFile(addedWellConfig.Well.Id.ToString());
                Assert.IsNull(Model, "There should be no model with this well");
                AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(addedWellConfig.Well.Id.ToString());
                Assert.IsNotNull(assembly, "Assembly is null when it should not be");
                SubAssemblyDTO[] SubAssembly = WellboreComponentService.GetSubAssembliesByWellId(addedWellConfig.Well.Id.ToString());
                Assert.IsNotNull(SubAssembly, "SubAssembly is null when it should not be");
                i++;
            }
        }

        [TestCategory(TestCategories.WellConfigurationServiceTests), TestMethod]
        public void AddGetNonRRLWellConfigCommonModelFiles()
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            Tuple<string, WellTypeId, ModelFileOptionDTO>[] models = { Tuple.Create("WellfloGasLiftExample1.wflx", WellTypeId.GLift, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.LFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.CalculateChokeD_Factor) } }),
                Tuple.Create("WellfloGasInjectionExample1.wflx", WellTypeId.GInj, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.PIAndLFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.CalculateChokeD_Factor) } }),
                Tuple.Create("WellfloESPExample1.wflx", WellTypeId.ESP, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.ReservoirPressure, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR)} }),
                Tuple.Create("WellfloNFWExample1.wflx", WellTypeId.NF, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.DarcyFlowCoefficient, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.CalculateChokeD_Factor) } }),
                Tuple.Create("WellfloWaterInjectionExample1.wflx", WellTypeId.WInj, new ModelFileOptionDTO(){ CalibrationMethod = CalibrationMethodId.InjectivityIndexAndLFactor, OptionalUpdate = new long[] { ((long)OptionalUpdates.CalculateChokeD_Factor), ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.UpdateWCT_WGR) } }),
                Tuple.Create("PCP-SinglePhase.wflx", WellTypeId.PCP, new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.ReservoirPressure, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.UpdateWCT_WGR) } }),
                Tuple.Create("PCP-Multiphase.wflx", WellTypeId.PCP, new ModelFileOptionDTO() { CalibrationMethod = CalibrationMethodId.ReservoirPressure, OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateGOR_CGR), ((long)OptionalUpdates.UpdateWCT_WGR) } })};
            foreach (Tuple<string, WellTypeId, ModelFileOptionDTO> modelInfo in models)
            {
                string model = modelInfo.Item1;
                WellTypeId wellType = modelInfo.Item2;
                ModelFileOptionDTO options = modelInfo.Item3;

                Trace.WriteLine("Testing model: " + model);
                //Create a new well
                WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = modelInfo.Item1.Split('.')[0] + wellType.ToString(), CommissionDate = DateTime.Today, WellType = wellType }) });
                var allWells = WellService.GetAllWells().ToList();
                WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(modelInfo.Item1.Split('.')[0] + wellType.ToString()));
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

                WellConfigDTO openWell = WellConfigurationService.GetWellConfig(well.Id.ToString());

                switch (wellType)
                {
                    case Enums.WellTypeId.ESP:
                        Assert.IsInstanceOfType(openWell.CommonModelConfig, typeof(ESPConfigDTO));
                        ESPConfigDTO espModel = (ESPConfigDTO)openWell.CommonModelConfig;
                        Assert.IsNotNull(espModel);
                        Assert.IsNotNull(espModel.ESPDataAndUnits);
                        Assert.IsTrue(espModel.ESPDataAndUnits.Value.Motor.Value.CableSize == "#2");
                        Assert.IsNull(openWell.ModelConfig);
                        break;

                    case Enums.WellTypeId.GLift:
                        Assert.IsInstanceOfType(openWell.CommonModelConfig, typeof(GasLiftConfigDTO));
                        GasLiftConfigDTO glModel = (GasLiftConfigDTO)openWell.CommonModelConfig;
                        Assert.IsNotNull(glModel);
                        Assert.IsNotNull(glModel.GasLiftDataAndUnits.Values);
                        Assert.IsTrue(glModel.GasLiftDataAndUnits.Values.Count() == 3);
                        Assert.IsNull(openWell.ModelConfig);
                        break;
                    case Enums.WellTypeId.PCP:
                        Assert.IsInstanceOfType(openWell.CommonModelConfig, typeof(PCPConfigDTO));
                        PCPConfigDTO pcpModel = (PCPConfigDTO)openWell.CommonModelConfig;
                        Assert.IsNotNull(pcpModel);
                        Assert.IsNotNull(pcpModel.PCPDataAndUnits.Value);
                        Assert.IsNull(openWell.ModelConfig);
                        break;

                    default:
                        CommonModelConfigDTO commonModel = openWell.CommonModelConfig;
                        Assert.IsNotNull(commonModel);
                        Assert.IsNull(openWell.ModelConfig);
                        break;
                }
            }
        }

        #region Additional Well Attributes Test Method

        [TestCategory(TestCategories.WellConfigurationServiceTests), TestMethod]
        public void AdditionalWellAttributes()
        {
            WellConfigurationService.AddWellConfig(new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = DefaultWellName,
                    FacilityId = "CaseTest",
                    DataConnection = GetDefaultCygNetDataConnection(),
                    IntervalAPI = "IntervalAPI",
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

            WellAttributeMetaDataDTO[] mdWellAttributes = WellConfigurationService.GetMetaDataForWellAttributes(well.Id.ToString());
            Assert.IsNotNull(mdWellAttributes, "Failed to retrieve the meta data for the well attributes");
            Assert.AreEqual(7, mdWellAttributes.Count(), "Incorrect number of well Attributes categories");
            foreach (WellAttributeMetaDataDTO attr in mdWellAttributes)
            {
                foreach (MetaDataDTO md in attr.Fields)
                {
                    Assert.AreEqual(attr.GroupName, md.ColumnCategory, "Mimatch between the column and group category");
                }
            }

            //Add
            foreach (WellAttributeMetaDataDTO attr in mdWellAttributes)
            {
                switch (attr.GroupName)
                {
                    case " ": //For our lovely Oracle
                        {
                            Assert.AreEqual((int)well.Id, attr.Fields.FirstOrDefault(x => x.ColumnName == "welPrimaryKey").DataValue);
                            break;
                        }
                    case "":
                        {
                            attr.Fields.FirstOrDefault(x => x.ColumnName == "welPrimaryKey").DataValue = (int)well.Id;
                            break;
                        }
                    case "Important Dates":
                        {
                            //SPUD Date has been moved to Well Configuration on FRI-3334
                            //attr.Fields.FirstOrDefault(x => x.ColumnName == "welSpudDate").DataValue = well.CommissionDate.ToString();
                            attr.Fields.FirstOrDefault(x => x.ColumnName == "welAbandonmentDate").DataValue = well.CommissionDate.ToString();
                            attr.Fields.FirstOrDefault(x => x.ColumnName == "welFirstProdDate").DataValue = well.CommissionDate.ToString();
                            break;
                        }
                    case "Finance and Electric":
                        {
                            attr.Fields.FirstOrDefault(x => x.ColumnName == "welTaxCreditCode").DataValue = "ZADERTYV";
                            attr.Fields.FirstOrDefault(x => x.ColumnName == "welPTTaxableStatus").DataValue = true;
                            break;
                        }
                    case "Congressional Carter Location":
                        {
                            attr.Fields.FirstOrDefault(x => x.ColumnName == "welCCLTownshipDirection").DataValue = "2";
                            attr.Fields.FirstOrDefault(x => x.ColumnName == "welCCLTownshipNumber").DataValue = "88";
                            break;
                        }
                    case "Plot Information":
                        {
                            attr.Fields.FirstOrDefault(x => x.ColumnName == "welSurfaceNodeId").DataValue = "SurfaceNodeId";
                            attr.Fields.FirstOrDefault(x => x.ColumnName == "welTexAbstractNumber").DataValue = "22";
                            break;
                        }
                    case "Offshore Information":
                        {
                            attr.Fields.FirstOrDefault(x => x.ColumnName == "welGovPlatformId").DataValue = "GovPlatformId";
                            attr.Fields.FirstOrDefault(x => x.ColumnName == "welPlatformId").DataValue = "PlatformId";
                            break;
                        }
                    case "Other":
                        {
                            attr.Fields.FirstOrDefault(x => x.ColumnName == "welLongWellName").DataValue = "LongWellName";
                            attr.Fields.FirstOrDefault(x => x.ColumnName == "welDiscovery").DataValue = true;
                            break;
                        }
                    default:
                        {
                            Assert.Fail("Group Category not expected");
                            break;
                        }
                }
            }

            bool saveCheck = WellConfigurationService.SaveAdditionalWellAttributes(mdWellAttributes);
            Assert.IsTrue(saveCheck, "Failed to save additional well attributes");

            //Verification -- Add
            mdWellAttributes = WellConfigurationService.GetMetaDataForWellAttributes(well.Id.ToString());
            Assert.AreEqual(7, mdWellAttributes.Count(), "Incorrect number of well Attributes categories");
            foreach (WellAttributeMetaDataDTO attr in mdWellAttributes)
            {
                foreach (MetaDataDTO md in attr.Fields)
                {
                    Assert.AreEqual(attr.GroupName, md.ColumnCategory, "Mimatch between the column and group category");
                }
            }
            foreach (WellAttributeMetaDataDTO attr in mdWellAttributes)
            {
                switch (attr.GroupName)
                {
                    case " ": //For our lovely Oracle
                        {
                            Assert.AreEqual((int)well.Id, attr.Fields.FirstOrDefault(x => x.ColumnName == "welPrimaryKey").DataValue);
                            break;
                        }
                    case "":
                        {
                            Assert.AreEqual((int)well.Id, attr.Fields.FirstOrDefault(x => x.ColumnName == "welPrimaryKey").DataValue);
                            break;
                        }
                    case "Important Dates":
                        {
                            //SPUD Date has been moved to Well Configuration on FRI-3334
                            //Assert.IsNotNull(attr.Fields.FirstOrDefault(x => x.ColumnName == "welSpudDate").DataValue);
                            Assert.IsNotNull(attr.Fields.FirstOrDefault(x => x.ColumnName == "welAbandonmentDate").DataValue);
                            Assert.IsNotNull(attr.Fields.FirstOrDefault(x => x.ColumnName == "welFirstProdDate").DataValue);
                            break;
                        }
                    case "Finance and Electric":
                        {
                            Assert.AreEqual("ZADERTYV", attr.Fields.FirstOrDefault(x => x.ColumnName == "welTaxCreditCode").DataValue);
                            Assert.AreEqual(1, attr.Fields.FirstOrDefault(x => x.ColumnName == "welPTTaxableStatus").DataValue);
                            break;
                        }
                    case "Congressional Carter Location":
                        {
                            Assert.AreEqual("2", attr.Fields.FirstOrDefault(x => x.ColumnName == "welCCLTownshipDirection").DataValue);
                            Assert.AreEqual("88", attr.Fields.FirstOrDefault(x => x.ColumnName == "welCCLTownshipNumber").DataValue);
                            break;
                        }
                    case "Plot Information":
                        {
                            Assert.AreEqual("SurfaceNodeId", attr.Fields.FirstOrDefault(x => x.ColumnName == "welSurfaceNodeId").DataValue);
                            Assert.AreEqual("22", attr.Fields.FirstOrDefault(x => x.ColumnName == "welTexAbstractNumber").DataValue);
                            break;
                        }
                    case "Offshore Information":
                        {
                            Assert.AreEqual("GovPlatformId", attr.Fields.FirstOrDefault(x => x.ColumnName == "welGovPlatformId").DataValue);
                            Assert.AreEqual("PlatformId", attr.Fields.FirstOrDefault(x => x.ColumnName == "welPlatformId").DataValue);
                            break;
                        }
                    case "Other":
                        {
                            Assert.AreEqual("LongWellName", attr.Fields.FirstOrDefault(x => x.ColumnName == "welLongWellName").DataValue);
                            Assert.AreEqual(1, attr.Fields.FirstOrDefault(x => x.ColumnName == "welDiscovery").DataValue);
                            break;
                        }
                    default:
                        {
                            Assert.Fail("Group Category not expected after Add");
                            break;
                        }
                }
            }

            //Update
            mdWellAttributes.FirstOrDefault(x => x.GroupName == "Other").Fields.FirstOrDefault(x => x.ColumnName == "welDiscovery").DataValue = false;
            bool updateCheck = WellConfigurationService.SaveAdditionalWellAttributes(mdWellAttributes);

            mdWellAttributes = WellConfigurationService.GetMetaDataForWellAttributes(well.Id.ToString());
            Assert.AreEqual(7, mdWellAttributes.Count(), "Incorrect number of well Attributes categories");
            foreach (WellAttributeMetaDataDTO attr in mdWellAttributes)
            {
                foreach (MetaDataDTO md in attr.Fields)
                {
                    Assert.AreEqual(attr.GroupName, md.ColumnCategory, "Mimatch between the column and group category");
                }
            }
            foreach (WellAttributeMetaDataDTO attr in mdWellAttributes)
            {
                switch (attr.GroupName)
                {
                    case " ": //For our lovely Oracle
                        {
                            Assert.AreEqual((int)well.Id, attr.Fields.FirstOrDefault(x => x.ColumnName == "welPrimaryKey").DataValue);
                            break;
                        }
                    case "":
                        {
                            Assert.AreEqual((int)well.Id, attr.Fields.FirstOrDefault(x => x.ColumnName == "welPrimaryKey").DataValue);
                            break;
                        }
                    case "Important Dates":
                        {
                            //SPUD Date has been moved to Well Configuration on FRI-3334
                            //Assert.IsNotNull(attr.Fields.FirstOrDefault(x => x.ColumnName == "welSpudDate").DataValue);
                            Assert.IsNotNull(attr.Fields.FirstOrDefault(x => x.ColumnName == "welAbandonmentDate").DataValue);
                            Assert.IsNotNull(attr.Fields.FirstOrDefault(x => x.ColumnName == "welFirstProdDate").DataValue);
                            break;
                        }
                    case "Finance and Electric":
                        {
                            Assert.AreEqual("ZADERTYV", attr.Fields.FirstOrDefault(x => x.ColumnName == "welTaxCreditCode").DataValue);
                            Assert.AreEqual(1, attr.Fields.FirstOrDefault(x => x.ColumnName == "welPTTaxableStatus").DataValue);
                            break;
                        }
                    case "Congressional Carter Location":
                        {
                            Assert.AreEqual("2", attr.Fields.FirstOrDefault(x => x.ColumnName == "welCCLTownshipDirection").DataValue);
                            Assert.AreEqual("88", attr.Fields.FirstOrDefault(x => x.ColumnName == "welCCLTownshipNumber").DataValue);
                            break;
                        }
                    case "Plot Information":
                        {
                            Assert.AreEqual("SurfaceNodeId", attr.Fields.FirstOrDefault(x => x.ColumnName == "welSurfaceNodeId").DataValue);
                            Assert.AreEqual("22", attr.Fields.FirstOrDefault(x => x.ColumnName == "welTexAbstractNumber").DataValue);
                            break;
                        }
                    case "Offshore Information":
                        {
                            Assert.AreEqual("GovPlatformId", attr.Fields.FirstOrDefault(x => x.ColumnName == "welGovPlatformId").DataValue);
                            Assert.AreEqual("PlatformId", attr.Fields.FirstOrDefault(x => x.ColumnName == "welPlatformId").DataValue);
                            break;
                        }
                    case "Other":
                        {
                            Assert.AreEqual("LongWellName", attr.Fields.FirstOrDefault(x => x.ColumnName == "welLongWellName").DataValue);
                            Assert.AreEqual(0, attr.Fields.FirstOrDefault(x => x.ColumnName == "welDiscovery").DataValue);
                            break;
                        }
                    default:
                        {
                            Assert.Fail("Group Category not expected after Update");
                            break;
                        }
                }
            }
        }

        #endregion Additional Well Attributes Test Method

        [TestCategory(TestCategories.WellConfigurationServiceTests), TestMethod]
        public void AddPlungerLiftWell()
        {
            WellDTO well = SetDefaultFluidType(new WellDTO()
            {
                Name = "Plunger Lift",
                FacilityId = "PLWELL_0001",
                DataConnection = GetDefaultCygNetDataConnection(),
                CommissionDate = DateTime.Today.AddYears(-2),
                AssemblyAPI = "PLWELL_0001",
                SubAssemblyAPI = "PLWELL_0001",
                IntervalAPI = "PLWELL_0001",
                WellType = WellTypeId.PLift,
                GasAllocationGroup = null,
                OilAllocationGroup = null,
                WaterAllocationGroup = null
            });
            WellConfigDTO wellConfig = new WellConfigDTO();
            wellConfig.Well = well;
            wellConfig.ModelConfig = ReturnBlankModel();
            //Well
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfig);
            Assert.IsNotNull(addedWellConfig, "Failed to Add Well");
            Assert.IsNotNull(addedWellConfig.Well, "Failed to Add Well");
            Assert.IsNotNull(addedWellConfig.Well.Id, "Failed to Add Well");
            _wellsToRemove.Add(addedWellConfig.Well);
            //Model file
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            byte[] fileAsByteArray;
            ModelFileValidationDataDTO ModelFileValidationData;
            AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(addedWellConfig.Well.Id.ToString());
            ModelFileOptionDTO options = new ModelFileOptionDTO();
            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };
            modelFile.ApplicableDate = addedWellConfig.Well.CommissionDate.Value.AddDays(1).ToUniversalTime();
            modelFile.WellId = addedWellConfig.Well.Id;
            fileAsByteArray = GetByteArray(Path, "PL-631.wflx");
            options.Comment = "PLift";
            options.OptionalUpdate = new long[] { };
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
            Assert.IsNotNull(ModelFileValidationData, "Failed to validate the Model file");
            Assert.AreEqual(WellTypeId.PLift, ModelFileValidationData.WellType, "Failed to validate the Model file");
            ModelFileService.AddWellModelFile(modelFile);
            addedWellConfig = WellConfigurationService.GetWellConfig(addedWellConfig.Well.Id.ToString());
            Assert.IsNotNull(addedWellConfig, "Failed to get added Well with model file");
            Assert.IsNotNull(addedWellConfig.Well, "Failed to get added Well with model file");
            Assert.IsNotNull(addedWellConfig.Well.Id, "Failed to get added Well with model file");
            Assert.IsNotNull(addedWellConfig.CommonModelConfig, "Failed to get added model file configuration for the added well");
            Assert.IsNull(addedWellConfig.ModelConfig, "Model file data should be null as this is a Non-RRL well");
            Assert.AreEqual(addedWellConfig.Well.Id.ToString(), addedWellConfig.CommonModelConfig.WellId, "Incorrect model file");
        }

        [TestCategory(TestCategories.WellConfigurationServiceTests), TestMethod]
        public void WellsLinkedToTheSameAssembly()
        {
            string firstAPI10 = "5510782574";
            WellConfigUnitDTO units = WellConfigurationService.GetWellConfigUnits();
            var wellConfig1 = new WellConfigDTO()
            {
                Well = new WellDTO()
                {
                    Name = "Test 1",
                    AssemblyAPI = firstAPI10,
                    SubAssemblyAPI = firstAPI10 + "00",
                    IntervalAPI = firstAPI10 + "0000",
                    WellType = WellTypeId.ESP,
                    CommissionDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local),
                }
            };
            wellConfig1 = WellConfigurationService.AddWellConfigUoM(new UnitsValuesPairDTO<WellConfigUnitDTO, WellConfigDTO>(units, wellConfig1)).Value;
            _wellsToRemove.Add(wellConfig1.Well);

            SubAssemblyDTO[] well1SubAssemblies = WellboreComponentService.GetSubAssembliesByWellId(wellConfig1.Well.Id.ToString());
            Assert.AreEqual(1, well1SubAssemblies.Length, "First well should only have one subassembly.");
            Assert.AreEqual(wellConfig1.Well.SubAssemblyAPI, well1SubAssemblies[0].SAId, $"First well subassembly SAId should match SAId from {nameof(WellboreComponentService.GetSubAssembliesByWellId)}.");

            var wellConfig2 = new WellConfigDTO()
            {
                Well = new WellDTO()
                {
                    Name = "Test 2",
                    AssemblyAPI = wellConfig1.Well.AssemblyAPI,
                    AssemblyId = wellConfig1.Well.AssemblyId,
                    SubAssemblyAPI = wellConfig1.Well.SubAssemblyAPI,
                    SubAssemblyId = wellConfig1.Well.SubAssemblyId,
                    IntervalAPI = wellConfig1.Well.SubAssemblyAPI + "01",
                    WellType = WellTypeId.ESP,
                    CommissionDate = wellConfig1.Well.CommissionDate,
                }
            };
            wellConfig2 = WellConfigurationService.AddWellConfigUoM(new UnitsValuesPairDTO<WellConfigUnitDTO, WellConfigDTO>(units, wellConfig2)).Value;
            _wellsToRemove.Add(wellConfig2.Well);

            SubAssemblyDTO[] well2SubAssemblies = WellboreComponentService.GetSubAssembliesByWellId(wellConfig1.Well.Id.ToString());
            Assert.AreEqual(1, well2SubAssemblies.Length, "Second well should have one subassembly.");
            Assert.AreEqual(wellConfig2.Well.SubAssemblyAPI, well2SubAssemblies[0].SAId, $"Second well subassembly SAId should match SAId from {nameof(WellboreComponentService.GetSubAssembliesByWellId)}.");

            JobTypeDTO[] jobTypes = JobAndEventService.GetJobTypes();
            JobTypeDTO jobType = jobTypes.First(t => t.JobType == "1st Time Data Import");
            JobStatusDTO[] jobStatuses = JobAndEventService.GetJobStatuses();
            JobReasonDTO[] jobReasons = JobAndEventService.GetJobReasonsForJobType(jobType.id.ToString());
            JobAndEventService.AddJob(new JobLightDTO()
            {
                AssemblyId = wellConfig1.Well.AssemblyId,
                WellId = wellConfig1.Well.Id,
                JobTypeId = jobType.id,
                StatusId = jobStatuses.First(t => t.Name == "Approved").Id,
                BeginDate = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Local).ToUniversalTime(),
                EndDate = new DateTime(2018, 1, 31, 0, 0, 0, DateTimeKind.Local).ToUniversalTime(),
                JobReasonId = jobReasons.First(t => t.JobReason == "Data Imported").Id,
            });

            JobLightDTO[] well1Jobs = JobAndEventService.GetJobsByWell(wellConfig1.Well.Id.ToString());
            JobLightDTO[] well2Jobs = JobAndEventService.GetJobsByWell(wellConfig2.Well.Id.ToString());

            Assert.AreEqual(1, well1Jobs.Length, "First well should have one job.");
            Assert.AreEqual(well1Jobs.Length, well2Jobs.Length, "First and second wells should have the same number of jobs.");
            Assert.AreEqual(wellConfig1.Well.Id, well1Jobs[0].WellId, "Job should be linked to the first well.");

            var wellConfig3Original = new WellConfigDTO()
            {
                Well = new WellDTO()
                {
                    Name = "Test 3",
                    AssemblyAPI = wellConfig1.Well.AssemblyAPI,
                    AssemblyId = wellConfig1.Well.AssemblyId,
                    SubAssemblyAPI = wellConfig1.Well.AssemblyAPI + "01",
                    IntervalAPI = wellConfig1.Well.AssemblyAPI + "0100",
                    WellType = WellTypeId.ESP,
                    CommissionDate = wellConfig1.Well.CommissionDate,
                }
            };
            WellConfigDTO wellConfig3 = WellConfigurationService.AddWellConfigUoM(new UnitsValuesPairDTO<WellConfigUnitDTO, WellConfigDTO>(units, wellConfig3Original)).Value;
            _wellsToRemove.Add(wellConfig3.Well);
            Assert.AreEqual(wellConfig3Original.Well.AssemblyAPI, wellConfig3.Well.AssemblyAPI, "Third well assembly API has an unexpected value.");

            SubAssemblyDTO[] well3SubAssemblies = WellboreComponentService.GetSubAssembliesByWellId(wellConfig3.Well.Id.ToString());
            SubAssemblyDTO firstAPI10SecondSubassembly = well3SubAssemblies[1];
            Assert.AreEqual(2, well3SubAssemblies.Length, "Third well should have two subassemblies.");
            Assert.AreEqual(wellConfig3Original.Well.SubAssemblyAPI, firstAPI10SecondSubassembly.SAId, $"Third well subassembly SAId should match SAId from {nameof(WellboreComponentService.GetSubAssembliesByWellId)}.");

            JobLightDTO[] well3Jobs = JobAndEventService.GetJobsByWell(wellConfig3.Well.Id.ToString());
            Assert.AreEqual(well1Jobs.Length, well3Jobs.Length, "First and third wells should have the same number of jobs.");

            // Okay, now move well 3 to another (new) assembly.
            string secondAPI10 = "5510711142";
            wellConfig3.Well.AssemblyId = 0;
            wellConfig3.Well.AssemblyAPI = secondAPI10;
            wellConfig3.Well.SubAssemblyId = 0;
            string secondAPI10FirstAPI12 = wellConfig3.Well.SubAssemblyAPI = secondAPI10 + "00";
            string secondAPI10FirstAPI14 = wellConfig3.Well.IntervalAPI = wellConfig3.Well.SubAssemblyAPI + "00";
            WellConfigurationService.UpdateWellConfig(wellConfig3);
            wellConfig3 = WellConfigurationService.GetWellConfig(wellConfig3.Well.Id.ToString());
            Assert.AreEqual(secondAPI10, wellConfig3.Well.AssemblyAPI, "Third well assembly API has an unexpected value after moving to a new assembly.");
            Assert.AreEqual(secondAPI10FirstAPI12, wellConfig3.Well.SubAssemblyAPI, "Third well subassembly API has an unexpected value after moving to a new assembly.");
            Assert.AreEqual(secondAPI10FirstAPI14, wellConfig3.Well.IntervalAPI, "Third well interval API has an unexpected value after moving to a new assembly.");

            // Well 3 should no longer have any jobs.
            well3Jobs = JobAndEventService.GetJobsByWell(wellConfig3.Well.Id.ToString());
            Assert.AreEqual(0, well3Jobs.Length, "Third well should have no jobs after being linked to a different assembly.");

            well3SubAssemblies = WellboreComponentService.GetSubAssembliesByWellId(wellConfig3.Well.Id.ToString());
            Assert.AreEqual(1, well3SubAssemblies.Length, "Third well should now have one subassembly.");
            Assert.AreEqual(wellConfig3.Well.SubAssemblyAPI, well3SubAssemblies[0].SAId, $"Third well subassembly SAId should match SAId from {nameof(WellboreComponentService.GetSubAssembliesByWellId)} after assembly change.");

            // Create a fourth well linked to the third's assembly/subassembly.
            var wellConfig4 = new WellConfigDTO()
            {
                Well = new WellDTO()
                {
                    Name = "Test 4",
                    AssemblyAPI = wellConfig3.Well.AssemblyAPI,
                    AssemblyId = wellConfig3.Well.AssemblyId,
                    SubAssemblyId = wellConfig3.Well.SubAssemblyId,
                    SubAssemblyAPI = wellConfig3.Well.SubAssemblyAPI,
                    IntervalAPI = wellConfig3.Well.SubAssemblyAPI + "01",
                    WellType = WellTypeId.ESP,
                    CommissionDate = wellConfig3.Well.CommissionDate,
                }
            };
            string secondAPI10SecondAPI14 = wellConfig4.Well.IntervalAPI;
            wellConfig4 = WellConfigurationService.AddWellConfigUoM(new UnitsValuesPairDTO<WellConfigUnitDTO, WellConfigDTO>(units, wellConfig4)).Value;
            _wellsToRemove.Add(wellConfig4.Well);

            Assert.AreEqual(wellConfig3.Well.AssemblyId, wellConfig4.Well.AssemblyId, "Fourth well assembly id has an unexpected value.");
            Assert.AreEqual(secondAPI10, wellConfig4.Well.AssemblyAPI, "Fourth well assembly API has an unexpected value.");
            Assert.AreEqual(wellConfig3.Well.SubAssemblyId, wellConfig4.Well.SubAssemblyId, "Fourth well subassembly id has an unexpected value.");
            Assert.AreEqual(secondAPI10FirstAPI12, wellConfig4.Well.SubAssemblyAPI, "Fourth well subassembly API has an unexpected value.");
            Assert.AreEqual(secondAPI10SecondAPI14, wellConfig4.Well.IntervalAPI, "Fourth well interval API has an unexpected value.");

            // Now link well 4 to well 1's assembly/subassembly.
            wellConfig4.Well.AssemblyId = wellConfig1.Well.AssemblyId;
            wellConfig4.Well.AssemblyAPI = wellConfig1.Well.AssemblyAPI;
            wellConfig4.Well.SubAssemblyId = wellConfig1.Well.SubAssemblyId;
            wellConfig4.Well.SubAssemblyAPI = wellConfig1.Well.SubAssemblyAPI;
            WellConfigurationService.UpdateWellConfig(wellConfig4);
            WellConfigDTO wellConfig4Check = WellConfigurationService.GetWellConfig(wellConfig4.Well.Id.ToString());
            Assert.AreEqual(wellConfig4.Well.AssemblyId, wellConfig4Check.Well.AssemblyId, "Fourth well assembly id has unexpected value after linking to well 1's assembly.");
            Assert.AreEqual(wellConfig4.Well.AssemblyAPI, wellConfig4Check.Well.AssemblyAPI, "Fourth well assembly API has unexpected value after linking to well 1's assembly.");
            Assert.AreEqual(wellConfig4.Well.AssemblyAPI, wellConfig4Check.Well.Assembly.Name, "Fourth well assembly name has unexpected value after linking to well 1's assembly.");
            Assert.AreEqual(wellConfig4.Well.SubAssemblyId, wellConfig4Check.Well.SubAssemblyId, "Fourth well subassembly id has unexpected value after linking to well 1's assembly.");
            Assert.AreEqual(wellConfig4.Well.SubAssemblyAPI, wellConfig4Check.Well.SubAssemblyAPI, "Fourth well subassembly API has unexpected value after linking to well 1's assembly.");
            Assert.AreEqual(wellConfig4.Well.IntervalAPI, wellConfig4Check.Well.IntervalAPI, "Fourth well interval API has unexpected value after linking to well 1's assembly.");

            // Verify well 3's assembly/subassembly information is unchanged.
            WellConfigDTO wellConfig3Check = WellConfigurationService.GetWellConfig(wellConfig3.Well.Id.ToString());
            Assert.AreEqual(wellConfig3.Well.AssemblyId, wellConfig3Check.Well.AssemblyId, "Third well assembly id should be unchanged after linking well 4 to well 1's assembly.");
            Assert.AreEqual(wellConfig3.Well.AssemblyAPI, wellConfig3Check.Well.AssemblyAPI, "Third well assembly API should be unchanged after linking well 4 to well 1's assembly.");
            Assert.AreEqual(wellConfig3.Well.Assembly.Name, wellConfig3Check.Well.Assembly.Name, "Third well assembly name should be unchanged after linking well 4 to well 1's assembly.");
            Assert.AreEqual(wellConfig3.Well.Assembly.UWBId, wellConfig3Check.Well.Assembly.UWBId, "Third well assembly UWBId should be unchanged after linking well 4 to well 1's assembly.");
            Assert.AreEqual(wellConfig3.Well.SubAssemblyAPI, wellConfig3Check.Well.SubAssemblyAPI, "Third well subassembly id should be unchanged after linking well 4 to well 1's assembly.");
            Assert.AreEqual(wellConfig3.Well.SubAssemblyId, wellConfig3Check.Well.SubAssemblyId, "Third well subassembly API should be unchanged after linking well 4 to well 1's assembly.");

            // Verify well 1's assembly/subassembly information is unchanged.
            WellConfigDTO wellConfig1Check = WellConfigurationService.GetWellConfig(wellConfig1.Well.Id.ToString());
            Assert.AreEqual(wellConfig1.Well.AssemblyId, wellConfig1Check.Well.AssemblyId, "First well assembly id should be unchanged after linking well 4 to well 1's assembly.");
            Assert.AreEqual(wellConfig1.Well.AssemblyAPI, wellConfig1Check.Well.AssemblyAPI, "First well assembly API should be unchanged after linking well 4 to well 1's assembly.");
            Assert.AreEqual(wellConfig1.Well.Assembly.Name, wellConfig1Check.Well.Assembly.Name, "First well assembly name should be unchanged after linking well 4 to well 1's assembly.");
            Assert.AreEqual(wellConfig1.Well.Assembly.UWBId, wellConfig1Check.Well.Assembly.UWBId, "First well assembly UWBId should be unchanged after linking well 4 to well 1's assembly.");
            Assert.AreEqual(wellConfig1.Well.SubAssemblyAPI, wellConfig1Check.Well.SubAssemblyAPI, "First well subassembly id should be unchanged after linking well 4 to well 1's assembly.");
            Assert.AreEqual(wellConfig1.Well.SubAssemblyId, wellConfig1Check.Well.SubAssemblyId, "First well subassembly API should be unchanged after linking well 4 to well 1's assembly.");

            // Link well 4 to a different subassembly and verify.
            wellConfig4 = wellConfig4Check;
            wellConfig4.Well.SubAssemblyId = firstAPI10SecondSubassembly.Id;
            wellConfig4.Well.SubAssemblyAPI = firstAPI10SecondSubassembly.SAId;
            WellConfigurationService.UpdateWellConfig(wellConfig4);
            wellConfig4Check = WellConfigurationService.GetWellConfig(wellConfig4.Well.Id.ToString());
            Assert.AreEqual(wellConfig4.Well.AssemblyId, wellConfig4Check.Well.AssemblyId, "Fourth well assembly id has unexpected value after linking to a different subassembly.");
            Assert.AreEqual(wellConfig4.Well.AssemblyAPI, wellConfig4Check.Well.AssemblyAPI, "Fourth well assembly API has unexpected value after linking to a different subassembly.");
            Assert.AreEqual(wellConfig4.Well.AssemblyAPI, wellConfig4Check.Well.Assembly.Name, "Fourth well assembly name has unexpected value after linking to a different subassembly.");
            Assert.AreEqual(wellConfig4.Well.SubAssemblyId, wellConfig4Check.Well.SubAssemblyId, "Fourth well subassembly id has unexpected value after linking to a different subassembly.");
            Assert.AreEqual(wellConfig4.Well.SubAssemblyAPI, wellConfig4Check.Well.SubAssemblyAPI, "Fourth well subassembly API has unexpected value after linking to a different subassembly.");
            Assert.AreEqual(wellConfig4.Well.IntervalAPI, wellConfig4Check.Well.IntervalAPI, "Fourth well interval API has unexpected value after linking to a different subassembly.");

            // Verify well 1's assembly/subassembly information is unchanged.
            wellConfig1Check = WellConfigurationService.GetWellConfig(wellConfig1.Well.Id.ToString());
            Assert.AreEqual(wellConfig1.Well.AssemblyId, wellConfig1Check.Well.AssemblyId, "First well assembly id should be unchanged after linking well 4 to a different subassembly.");
            Assert.AreEqual(wellConfig1.Well.AssemblyAPI, wellConfig1Check.Well.AssemblyAPI, "First well assembly API should be unchanged after linking well 4 to a different subassembly.");
            Assert.AreEqual(wellConfig1.Well.Assembly.Name, wellConfig1Check.Well.Assembly.Name, "First well assembly name should be unchanged after linking well 4 to a different subassembly.");
            Assert.AreEqual(wellConfig1.Well.Assembly.UWBId, wellConfig1Check.Well.Assembly.UWBId, "First well assembly UWBId should be unchanged after linking well 4 to a different subassembly.");
            Assert.AreEqual(wellConfig1.Well.SubAssemblyAPI, wellConfig1Check.Well.SubAssemblyAPI, "First well subassembly id should be unchanged after linking well 4 to a different subassembly.");
            Assert.AreEqual(wellConfig1.Well.SubAssemblyId, wellConfig1Check.Well.SubAssemblyId, "First well subassembly API should be unchanged after linking well 4 to a different subassembly.");

            string thirdAPI10 = "5510701315";
            wellConfig1.Well.AssemblyAPI = thirdAPI10;
            wellConfig1.Well.AssemblyId = 0;
            wellConfig1.Well.SubAssemblyAPI = thirdAPI10 + "00";
            wellConfig1.Well.SubAssemblyId = 0;
            wellConfig1.Well.IntervalAPI = wellConfig1.Well.SubAssemblyAPI + "00";
            WellConfigurationService.UpdateWellConfig(wellConfig1);

            // Well 1 should no longer have any jobs.
            well1Jobs = JobAndEventService.GetJobsByWell(wellConfig1.Well.Id.ToString());
            Assert.AreEqual(0, well1Jobs.Length, "First well should have no jobs after being linked to a different assembly.");

            well1SubAssemblies = WellboreComponentService.GetSubAssembliesByWellId(wellConfig1.Well.Id.ToString());
            Assert.AreEqual(1, well1SubAssemblies.Length, "First well should now have one subassembly.");
            Assert.AreEqual(wellConfig1.Well.SubAssemblyAPI, well1SubAssemblies[0].SAId, $"First well subassembly SAId should match SAId from {nameof(WellboreComponentService.GetSubAssembliesByWellId)} after assembly change.");

            // Well 2 should still have the added job, but the well id should be null.
            well2Jobs = JobAndEventService.GetJobsByWell(wellConfig2.Well.Id.ToString());
            Assert.AreEqual(1, well2Jobs.Length, "Well 2 should still have one job.");
            Assert.AreEqual(null, well2Jobs[0].WellId, "Well 2's job should have a null well id since well 1 was unlinked from its assembly.");

            well2SubAssemblies = WellboreComponentService.GetSubAssembliesByWellId(wellConfig2.Well.Id.ToString());
            Assert.AreEqual(2, well2SubAssemblies.Length, "Second well should still have two subassemblies.");
            Assert.AreEqual(wellConfig2.Well.SubAssemblyAPI, well2SubAssemblies[0].SAId, $"Second well subassembly SAId should still match SAId from {nameof(WellboreComponentService.GetSubAssembliesByWellId)}.");
        }

        #region Private Methods

        private bool EqualWellDTOs(WellDTO config, WellDTO compareConfig)
        {
            if (config == null && compareConfig == null)
                return true;

            if (config == null || compareConfig == null)
                return false;

            if (config.Id == compareConfig.Id &&
                config.Name == compareConfig.Name &&
                //config.DataConnection == this.DataConnection && -- Assembly does NOT contain DataConnection info in its wellDTO; rather, it'll be null instead
                config.FacilityId == compareConfig.FacilityId &&
                config.CommissionDate == compareConfig.CommissionDate &&
                config.AssemblyAPI == compareConfig.AssemblyAPI &&
                config.SubAssemblyAPI == compareConfig.SubAssemblyAPI &&
                config.SurfaceLatitude == compareConfig.SurfaceLatitude &&
                config.SurfaceLongitude == compareConfig.SurfaceLongitude &&
                config.WellType == compareConfig.WellType &&
                config.Lease == compareConfig.Lease &&
                config.Field == compareConfig.Field &&
                config.Foreman == compareConfig.Foreman &&
                config.Engineer == compareConfig.Engineer &&
                config.GaugerBeat == compareConfig.GaugerBeat &&
                config.GeographicRegion == compareConfig.GeographicRegion &&
                config.welUserDef01 == compareConfig.welUserDef01 &&
                config.welUserDef02 == compareConfig.welUserDef02 &&
                config.welUserDef03 == compareConfig.welUserDef03 &&
                config.welUserDef04 == compareConfig.welUserDef04 &&
                config.welUserDef05 == compareConfig.welUserDef05 &&
                config.welUserDef06 == compareConfig.welUserDef06 &&
                config.welUserDef07 == compareConfig.welUserDef07 &&
                config.welUserDef08 == compareConfig.welUserDef08 &&
                config.welUserDef09 == compareConfig.welUserDef09 &&
                config.welUserDef10 == compareConfig.welUserDef10 &&
                config.welUserDef11 == compareConfig.welUserDef11 &&
                config.welUserDef12 == compareConfig.welUserDef12 &&
                config.welUserDef13 == compareConfig.welUserDef13 &&
                config.welUserDef14 == compareConfig.welUserDef14 &&
                config.welUserDef15 == compareConfig.welUserDef15 &&
                config.welUserDef16 == compareConfig.welUserDef16 &&
                config.welUserDef17 == compareConfig.welUserDef17 &&
                config.welUserDef18 == compareConfig.welUserDef18 &&
                config.welUserDef19 == compareConfig.welUserDef19 &&
                config.welUserDef20 == compareConfig.welUserDef20)
                return true;
            return false;
        }

        private bool EqualAssemblyDTOs(AssemblyDTO assembly, AssemblyDTO compareAssembly)
        {
            if (assembly == null && compareAssembly == null)
            {
                return true;
            }

            if (assembly == null || compareAssembly == null)
            {
                return false;
            }

            if (assembly.Id == compareAssembly.Id &&
                assembly.Name == compareAssembly.Name &&
                assembly.ChangeUser == compareAssembly.ChangeUser)
            {
                return true;
            }

            return false;
        }

        private bool EqualReportDTOs(ReportDTO report, ReportDTO compareReport)
        {
            if (report == null && compareReport == null)
                return true;

            if (report == null || compareReport == null)
                return false;

            if (report.Id == compareReport.Id &&
                report.ReportTypeId == compareReport.ReportTypeId &&
                report.ReportType == compareReport.ReportType &&
                report.JobTypeId == compareReport.JobTypeId &&
                report.JobType == compareReport.JobType &&
                report.JobReasonId == compareReport.JobReasonId &&
                report.JobReason == compareReport.JobReason &&
                report.AssemblyId == compareReport.AssemblyId &&
                report.Assembly == compareReport.Assembly &&
                report.ChangeUser == compareReport.ChangeUser &&
                report.ChangeDate == compareReport.ChangeDate &&
                report.Comment == compareReport.Comment &&
                report.OffDate == compareReport.OffDate &&
                report.WorkoverDate == compareReport.WorkoverDate &&
                report.OnDate == compareReport.OnDate &&
                report.FailedComponents == compareReport.FailedComponents &&
                report.WellId == compareReport.WellId)
            {
                return true;
            }

            return false;
        }

        #endregion Private Methods



    }
}

