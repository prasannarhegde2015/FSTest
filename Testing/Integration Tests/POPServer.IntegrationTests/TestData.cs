using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using Weatherford.POP.Quantities;
using Weatherford.POP.Settings;

namespace Weatherford.POP.Server.IntegrationTests
{
    public abstract partial class APIClientTestBase
    {
        protected static ModelConfigDTO ReturnBlankModel()
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


        protected WellConfigDTO AddWellWithSurfaceAndWeight(string facilityId1)
        {

            WellDTO well1 = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = facilityId1, DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today.AddYears(-1), AssemblyAPI = "1234567890", SubAssemblyAPI = "123456789012", IntervalAPI = "12345678901234", WellType = WellTypeId.RRL, WellDepthDatumId = 1 });
            WellConfigDTO wellConfigDTO = new WellConfigDTO();
            wellConfigDTO.Well = well1;
            wellConfigDTO.ModelConfig = ReturnModelWithSurfaceAndWeight(); // test partially configured model
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
            _wellsToRemove.Add(addedWellConfig.Well);
            return addedWellConfig;
        }
        protected static ModelConfigDTO ReturnModelWithSurfaceAndWeight()
        {
            //Configured Surface
            SurfaceConfigDTO SampleSurfaceConfig = new SurfaceConfigDTO();
            PumpingUnitDTO pumpingUnit = new PumpingUnitDTO() { Description = "C-40-76-48 L LUFKIN C40-76-48 (3644B)", Discriminator = "RRLSurfaceUnit", Manufacturer = "Lufkin", Type = "C", ABDimD = 0, ABDimF = 0, ABDimH = 0, APIA = 64, APIC = 48.16, APII = 48, APIK = 78.01, APIP = 57.5, AbbreviatedManufacturerName = "L", GearBoxRating = 40, Id = 2010, MaxStrokeLength = 48, NumberOfWristPins = 3, PhaseAngle = 0, RotationDirection = PumpingUnitRotationDirection.Both, StructUnbalance = 0, StructureRating = 76 };
            SampleSurfaceConfig.ActualStrokeLength = 48;
            SampleSurfaceConfig.PumpingUnit = pumpingUnit;
            SampleSurfaceConfig.PumpingUnitType = null;
            SampleSurfaceConfig.ClockwiseRotation = PumpingUnitRotationDirection.Counterclockwise;
            SampleSurfaceConfig.MotorAmpsDown = 90;
            SampleSurfaceConfig.MotorAmpsUp = 70;
            SampleSurfaceConfig.MotorType = new RRLMotorTypeDTO() { Id = 0, Name = "Nema B Electric" };
            SampleSurfaceConfig.MotorSize = new RRLMotorSizeDTO(10, 50);
            SampleSurfaceConfig.SlipTorque = new RRLMotorSlipDTO() { Id = 3, Rating = 2 };
            SampleSurfaceConfig.WristPinPosition = 0;


            //Configured Weights
            CrankWeightsConfigDTO SampleWeightsConfig = new CrankWeightsConfigDTO();
            SampleWeightsConfig.CrankId = "94110C";
            SampleWeightsConfig.CBT = 831.08;
            SampleWeightsConfig.TorqCalcMethod = TorqueCalculationMethod.API;
            SampleWeightsConfig.Crank_1_Primary = new RRLPrimaryWeightDTO() { LagDistance = 15, LagId = "OORO", LagMDistance = 77.4, LagWeight = 3894, LeadDistance = 12, LeadId = "6RO", LeadMDistance = 94.7, LeadWeight = 504 };
            SampleWeightsConfig.Crank_2_Primary = new RRLPrimaryWeightDTO() { LagDistance = 0, LagId = "None", LagMDistance = 0, LagWeight = 0, LeadDistance = 0, LeadId = "None", LeadMDistance = 0, LeadWeight = 0 };
            SampleWeightsConfig.Crank_1_Auxiliary = new RRLAuxiliaryWeightDTO() { LagId = "5A", LagWeight = 366, LeadId = "1S", LeadWeight = 638 };
            SampleWeightsConfig.Crank_2_Auxiliary = new RRLAuxiliaryWeightDTO() { LagId = "None", LagWeight = 0, LeadId = "None", LeadWeight = 0 };
            SampleWeightsConfig.PumpingUnitCrankCBT = 470810;

            //Model File
            ModelConfigDTO SampleModel = new ModelConfigDTO();
            //SampleModel.WellId = well.Id.ToString();
            SampleModel.Weights = SampleWeightsConfig;
            SampleModel.Surface = SampleSurfaceConfig;

            return SampleModel;
        }


        public WellDTO AddNonRRLWellGeneral(string wellName, string Facility, WellTypeId WellType, WellFluidType FluidType, WellFluidPhase FluidPhase, string WellDepthReference, long Asset)
        {
            ReferenceTableItemDTO[] wellDepthDatums = WellConfigurationService.GetReferenceTableItems("r_WellDepthDatum", "false");
            ReferenceTableItemDTO wellDepthDatum = wellDepthDatums.FirstOrDefault(t => t.ConstantId == WellDepthReference) ?? wellDepthDatums.FirstOrDefault();

            WellDTO well = new WellDTO();
            well = new WellDTO()
            {
                Name = wellName,
                FacilityId = Facility,
                DataConnection = GetDefaultCygNetDataConnection(),
                CommissionDate = DateTime.Today.AddYears(-4),
                AssemblyAPI = wellName,
                SubAssemblyAPI = wellName,
                IntervalAPI = wellName,
                WellType = WellType,
                FluidType = null,
                FluidPhase = null,
                DepthCorrectionFactor = 2,
                WellDepthDatumElevation = 1,
                WellGroundElevation = 1,
                WellDepthDatumId = wellDepthDatum?.Id,
                GasAllocationGroup = null,
                OilAllocationGroup = null,
                WaterAllocationGroup = null,
                AssetId = Asset,
                Lease = wellName + "_LN",
                Foreman = wellName + "_FR",
                Field = wellName + "_Field",
                Engineer = wellName + "_Engineer",
                GaugerBeat = wellName + "_GB",
                GeographicRegion = wellName + "_GR"
            };
            if (WellType == WellTypeId.NF || WellType == WellTypeId.GLift || WellType == WellTypeId.PCP || WellType == WellTypeId.PLift) // Added GLift as scope of FRWM-6777
            { well.FluidType = FluidType; }

            if (WellType == WellTypeId.PCP)
            {
                well.FluidPhase = FluidPhase;
            }
            WellConfigDTO wellConfig = new WellConfigDTO();
            wellConfig.Well = well;
            wellConfig.ModelConfig = ReturnBlankModel();
            //Well
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfig);
            Assert.IsTrue(addedWellConfig.Well.Id > 0);
            _wellsToRemove.Add(addedWellConfig.Well);

            return addedWellConfig.Well;
        }

        protected WellDTO AddNonRRLWellGeneralTab(string facilityIdBase, WellTypeId WellType, WellFluidType FluidType, string WellStatus, string WellDepthReference)
        {
            Authenticate();
            string facilityId = s_isRunningInATS ? facilityIdBase + "00003" : facilityIdBase + "0003";
            ReferenceTableItemDTO[] wellDepthDatums = WellConfigurationService.GetReferenceTableItems("r_WellDepthDatum", "false");
            ReferenceTableItemDTO wellDepthDatum = wellDepthDatums.FirstOrDefault(t => t.ConstantId == WellDepthReference) ?? wellDepthDatums.FirstOrDefault();
            ReferenceTableItemDTO[] wellStatus = WellConfigurationService.GetReferenceTableItems("r_WellStatus", "false");
            ReferenceTableItemDTO wellStatusId = wellStatus.FirstOrDefault(t => t.ConstantId == WellStatus) ?? wellDepthDatums.FirstOrDefault();

            WellDTO well = new WellDTO();
            well = new WellDTO()
            {
                Name = facilityId,
                FacilityId = facilityId,
                DataConnection = GetDefaultCygNetDataConnection(),
                CommissionDate = DateTime.Today.AddYears(-4),
                AssemblyAPI = facilityId,
                SubAssemblyAPI = facilityId,
                IntervalAPI = facilityId,
                WellType = WellType,
                FluidType = FluidType,
                DepthCorrectionFactor = 2,
                WellDepthDatumElevation = 1,
                WellGroundElevation = 1,
                WellDepthDatumId = wellDepthDatum?.Id,
                WellStatusId = wellStatusId?.Id,
                GasAllocationGroup = null,
                OilAllocationGroup = null,
                WaterAllocationGroup = null
            };

            WellConfigDTO wellConfig = new WellConfigDTO();
            wellConfig.Well = well;
            wellConfig.ModelConfig = ReturnBlankModel();
            //Well
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfig);
            Assert.IsTrue(addedWellConfig.Well.Id > 0);
            _wellsToRemove.Add(addedWellConfig.Well);

            return addedWellConfig.Well;
        }

        protected void AddNonRRLModelFile(WellDTO Well, string FileName, CalibrationMethodId tuningMethod, long[] OptionalUpdate)
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            byte[] fileAsByteArray;
            ModelFileValidationDataDTO ModelFileValidationData;
            ModelFileOptionDTO options = new ModelFileOptionDTO();
            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };
            modelFile.ApplicableDate = Well.CommissionDate.Value.AddMonths(2).ToUniversalTime();
            modelFile.WellId = Well.Id;
            fileAsByteArray = GetByteArray(Path, FileName);
            options.CalibrationMethod = tuningMethod;
            options.Comment = FileName;
            options.OptionalUpdate = OptionalUpdate;
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            modelFile.Options = options;
            ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
            if (ModelFileValidationData != null)
                ModelFileService.AddWellModelFile(modelFile);
            else
                Trace.WriteLine($"Failed to validate {FileName} model file");
        }

        protected WellDTO AddNonRRLWell(string BaseFacTag, WellTypeId wellType, bool scan = true, CalibrationMethodId tuningMethod = CalibrationMethodId.ReservoirPressure)
        {
            Authenticate();
            WellDTO well = new WellDTO();
            ReferenceTableItemDTO[] wellDepthDatums = WellConfigurationService.GetReferenceTableItems("r_WellDepthDatum", "false");
            ReferenceTableItemDTO wellDepthDatum = wellDepthDatums.FirstOrDefault(t => t.ConstantId == "GROUND_LEVEL") ?? wellDepthDatums.FirstOrDefault();
            ReferenceTableItemDTO[] wellStatus = WellConfigurationService.GetReferenceTableItems("r_WellStatus", "false");
            ReferenceTableItemDTO wellStatusId = wellStatus.FirstOrDefault(t => t.ConstantId == "ACTIVE") ?? wellDepthDatums.FirstOrDefault();
            well = SetDefaultFluidType(new WellDTO()
            {
                Name = BaseFacTag,
                FacilityId = BaseFacTag,
                DataConnection = GetDefaultCygNetDataConnection(),
                CommissionDate = DateTime.Today.AddYears(-4),
                AssemblyAPI = BaseFacTag,
                SubAssemblyAPI = BaseFacTag,
                IntervalAPI = BaseFacTag,
                WellType = wellType,
                DepthCorrectionFactor = 2,
                WellDepthDatumElevation = 1,
                WellGroundElevation = 1,
                WellDepthDatumId = wellDepthDatum?.Id,
                WellStatusId = wellStatusId?.Id,
                GasAllocationGroup = null,
                OilAllocationGroup = null,
                SurfaceLatitude = (decimal?)random.Next(-90, 90),
                SurfaceLongitude = (decimal?)random.Next(-180, 180),
                WaterAllocationGroup = null
            });

            WellConfigDTO wellConfig = new WellConfigDTO();
            wellConfig.Well = well;
            wellConfig.ModelConfig = ReturnBlankModel();
            //Well
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfig);
            Assert.IsTrue(addedWellConfig.Well.Id > 0);
            //Assert that default Fluid Type was Black Oil :
            WellFluidType[] fluidtypes = WellConfigurationService.GetModelFileFluidType(wellType.ToString());

            switch (wellType)
            {
                case WellTypeId.Unknown:
                    break;
                case WellTypeId.RRL:
                    {
                        Assert.IsTrue(fluidtypes.Length == 0, $"Fluid Type was not 0 for : { wellType.ToString() } ");
                        break;
                    }

                case WellTypeId.ESP:
                    {
                        Assert.IsTrue(fluidtypes.Length == 0, $"Fluid Type was not 0 for : { wellType.ToString() } ");
                        break;
                    }

                case WellTypeId.GInj:
                    {
                        Assert.IsTrue(fluidtypes.Length == 0, $"Fluid Type was not 0 for : { wellType.ToString() } ");
                        break;
                    }

                case WellTypeId.GLift:
                    {
                        Assert.IsTrue(fluidtypes.Length == 2, $"Fluid Type was not 2 for : { wellType.ToString() } ");
                        Assert.IsNotNull(fluidtypes.FirstOrDefault(x => x.ToString() == "Black Oil"), " Black Oil Not found ");
                        Assert.IsNotNull(fluidtypes.FirstOrDefault(x => x.ToString() == "Condensate"), " Condensate Not found ");
                        break;
                    }

                case WellTypeId.NF:
                    {
                        Assert.IsTrue(fluidtypes.Length == 3, $"Fluid Type was not 3 for : { wellType.ToString() } ");
                        Assert.IsNotNull(fluidtypes.FirstOrDefault(x => x.ToString() == "Black Oil"), " Black Oil Not found ");
                        Assert.IsNotNull(fluidtypes.FirstOrDefault(x => x.ToString() == "Condensate"), " Condensate Not found ");
                        Assert.IsNotNull(fluidtypes.FirstOrDefault(x => x.ToString() == "Dry Gas"), " Dry Gas Not found ");
                        break;

                    }
                case WellTypeId.WInj:
                    {
                        Assert.IsTrue(fluidtypes.Length == 0, $"Fluid Type was not 0 for : { wellType.ToString() } ");
                        break;
                    }
                case WellTypeId.WGInj:
                    {
                        Assert.IsTrue(fluidtypes.Length == 0, $"Fluid Type was not 0 for : { wellType.ToString() } ");
                        break;
                    }
                case WellTypeId.PLift:
                    {
                        Assert.IsTrue(fluidtypes.Length == 2, $"Fluid Type was not 2 for : { wellType.ToString() } ");
                        Assert.IsNotNull(fluidtypes.FirstOrDefault(x => x.ToString() == "Condensate"), " Condensate Not found ");
                        Assert.IsNotNull(fluidtypes.FirstOrDefault(x => x.ToString() == "Dry Gas"), " Dry Gas Not found ");
                        break;
                    }
                case WellTypeId.OT:
                    {
                        break;
                    }
                case WellTypeId.PCP:
                    {
                        Assert.IsTrue(fluidtypes.Length == 1, $"Fluid Type was not 2 for : { wellType.ToString() } ");
                        Assert.IsNotNull(fluidtypes.FirstOrDefault(x => x.ToString() == "Black Oil"), " Black Oil Not found ");
                        break;
                    }
                case WellTypeId.All:
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            //Model file
            AddModelFile(addedWellConfig.Well.CommissionDate.Value, wellType, addedWellConfig.Well.Id, tuningMethod);
            if (scan)
            {
                SurveillanceService.IssueCommandForSingleWell(addedWellConfig.Well.Id, WellCommand.DemandScan.ToString());
                SurveillanceService.IssueCommandForSingleWell(addedWellConfig.Well.Id, WellCommand.DemandScan.ToString());
                SurveillanceService.IssueCommandForSingleWell(addedWellConfig.Well.Id, WellCommand.DemandScan.ToString());
            }
            _wellsToRemove.Add(addedWellConfig.Well);
            return addedWellConfig.Well;
        }

        protected void AddModelFile(DateTime date, WellTypeId type, long wellId, CalibrationMethodId tuningMethod)
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            byte[] fileAsByteArray;
            ModelFileValidationDataDTO ModelFileValidationData;
            AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(wellId.ToString());
            ModelFileOptionDTO options = new ModelFileOptionDTO();
            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };
            modelFile.ApplicableDate = date.AddDays(1).ToUniversalTime();
            modelFile.WellId = wellId;
            switch (type)
            {
                case WellTypeId.NF:
                    {
                        fileAsByteArray = GetByteArray(Path, "WellfloNFWExample1.wflx");
                        options.CalibrationMethod = tuningMethod;
                        options.Comment = "NF";
                        options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        modelFile.Options = options;
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                        else
                            Trace.WriteLine(string.Format("Failed to validate NF model file"));
                        //Add Well Test
                        WellTestDTO testDataDTO = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "RepresentativeTest",
                            AverageTubingPressure = 164.7m,
                            AverageTubingTemperature = 100,
                            GaugePressure = 5800,
                            Oil = 1769.5m,
                            Gas = 880,
                            Water = 589.8m,
                            ChokeSize = 50,
                            FlowLinePressure = 50,
                            SeparatorPressure = 10000,
                        };
                        testDataDTO.SampleDate = date.AddDays(2).ToUniversalTime();
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                        break;
                    }
                case WellTypeId.GLift:
                    {
                        fileAsByteArray = GetByteArray(Path, "GL-01-Base.wflx");
                        options.CalibrationMethod = tuningMethod;
                        options.Comment = "Gas Lift";
                        options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        modelFile.Options = options;
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                        else
                            Trace.WriteLine(string.Format("Failed to validate NF model file"));
                        //Add Well Test
                        WellTestDTO testDataDTO = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "AllocatableTest",
                            AverageTubingPressure = 600,
                            AverageTubingTemperature = 98,
                            AverageCasingPressure = 1800,
                            GasInjectionRate = 400,
                            //FlowLinePressure = 50,
                            //SeparatorPressure = 30,
                            //GaugePressure = 12000,
                            Oil = 1500,
                            Gas = 1600,
                            Water = 1500,
                            ChokeSize = 28,
                        };
                        testDataDTO.SampleDate = date.AddDays(2).ToUniversalTime();
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                        break;
                    }
                case WellTypeId.ESP:
                    {
                        fileAsByteArray = GetByteArray(Path, "WellfloESPExample1.wflx");
                        options.CalibrationMethod = tuningMethod;
                        options.Comment = "ESP";
                        options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        modelFile.Options = options;
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                        else
                            Trace.WriteLine(string.Format("Failed to validate NF model file"));
                        //Add Well Test
                        WellTestDTO testDataDTO = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "AllocatableTest",
                            AverageTubingPressure = 1795,
                            AverageCasingPressure = 0,
                            AverageTubingTemperature = 100,
                            PumpIntakePressure = 2674,
                            //PumpDischargePressure = 3067.59m,
                            //GaugePressure = 2900,
                            Oil = 1157,
                            Gas = 596,
                            Water = 1367,
                            ChokeSize = 64,
                            FlowLinePressure = 1300,
                            SeparatorPressure = 987,
                            Frequency = 57,
                        };
                        testDataDTO.SampleDate = date.AddDays(1).ToUniversalTime();
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                        break;
                    }
                case WellTypeId.WInj:
                    {
                        fileAsByteArray = GetByteArray(Path, "WellfloWaterInjectionExample1.wflx");
                        options.CalibrationMethod = tuningMethod;
                        options.Comment = "WInj";
                        options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        modelFile.Options = options;
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);

                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        WellTestDTO testDataDTO = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "AllocatableTest",
                            //AverageTubingPressure = random.Next(500, 1500),
                            AverageTubingPressure = (decimal)3514.70,
                            AverageTubingTemperature = (decimal)80.0,
                            //AverageTubingTemperature = random.Next(50, 100),
                            GaugePressure = random.Next(500, 1900),
                            Water = (decimal)490.40,
                            //Water = random.Next(500, 1900),
                            FlowLinePressure = random.Next(50, 100)
                        };
                        testDataDTO.SampleDate = date.AddDays(2).ToUniversalTime();
                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                    }
                    break;

                case WellTypeId.GInj:
                    {
                        fileAsByteArray = GetByteArray(Path, "WellfloGasInjectionExample1.wflx");
                        options.CalibrationMethod = tuningMethod;
                        options.Comment = "GInj";
                        options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        modelFile.Options = options;
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        WellTestDTO testDataDTO = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "AllocatableTest",
                            //AverageTubingPressure = random.Next(500, 1900),
                            AverageTubingPressure = (decimal)1514.70,
                            AverageTubingTemperature = (decimal)85.0,
                            //AverageTubingTemperature = random.Next(50, 100),
                            GaugePressure = random.Next(500, 1900),
                            //Gas = random.Next(500, 1900),
                            Gas = (decimal)250.0,
                            FlowLinePressure = random.Next(50, 100)
                        };
                        testDataDTO.SampleDate = date.AddDays(2).ToUniversalTime();
                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                    }
                    break;

                case WellTypeId.PLift:
                    {
                        fileAsByteArray = GetByteArray(Path, "PL-631.wflx");
                        options.Comment = "PLift";
                        options.OptionalUpdate = new long[] { };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                    }
                    break;
                case WellTypeId.PCP:
                    {
                        fileAsByteArray = GetByteArray(Path, "PCP-Multiphase.wflx");
                        options.CalibrationMethod = tuningMethod;
                        options.Comment = "PCP";
                        options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
                        modelFile.Options = options;
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                        else
                            Trace.WriteLine(string.Format("Failed to validate PCP model file"));

                        WellConfigDTO wellConfigPCP = WellConfigurationService.GetWellConfig(wellId.ToString());
                        //Adding WellTest
                        WellTestDTO testDataDTO = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "Allocatable Test",
                            SampleDate = wellConfigPCP.Well.CommissionDate.Value.AddDays(5).ToUniversalTime(),
                            TestDuration = 12,
                            Oil = (decimal)210.2,
                            Water = (decimal)141.2,
                            Gas = (decimal)10.40,
                            AverageTubingPressure = (decimal)100.00,
                            AverageTubingTemperature = (decimal)80.00,
                            GaugePressure = (decimal)423.00,
                            AverageCasingPressure = (decimal)45.23,
                            PumpIntakePressure = (decimal)161.19,
                            PumpDischargePressure = (decimal)2130.69,
                            PolishedRodTorque = (decimal)361.35,
                            PumpTorque = (decimal)185.23,
                            PumpSpeed = (decimal)225.00,
                            FlowLinePressure = (decimal)1862.00,
                            FlowLineTemperature = (decimal)80.00,
                            SeparatorPressure = (decimal)1523.00,
                            ChokeSize = (decimal)64.00,
                            Comment = "PCPWellTest_Comment_Check",
                            MotorCurrent = (decimal)180.0,
                            MotorVolts = (decimal)230.0
                        };

                        //Saved WellTest Data
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                        Assert.AreEqual(1, WellTestDataService.GetWellTestDataByWellId(wellId.ToString()).Values.Length, "Well Test is not saved successfully");
                        Trace.WriteLine("Well test saved successfully");
                    }
                    break;
                case WellTypeId.WGInj:
                    {
                        //  No model File for WAG
                        break;
                    }
                default:
                    Trace.WriteLine(string.Format("Invalid Well type"));
                    break;
            }
        }

        public void AddComponentForWellConfig(string wellId, long assemblyId, long subassemblyId, string jobId, long eventId, string CompName, string CompGp, string parttype, string manufacturer, string CatalogItem, decimal InnerDiameter, decimal OuterDiameter, int Quantity, int Length, int TopDepth, int wellBorePerfStatus = 7)
        {
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");

            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == CompGp).ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");

            ComponentMetaDataGroupDTO[] cmpMetaData = ComponentService.GetComponentMetaDataForAdd(partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType.ToString(), partfilter.TypeId.ToString());
            Trace.WriteLine("Adding Component with Comp Name : +" + CompGp + " Part Type :" + parttype);
            Trace.WriteLine("Component has " + cmpMetaData.Count() + " categories");

            ComponentMetaDataGroupDTO reqComponent = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required");
            reqComponent.JobId = Convert.ToInt64(jobId);
            reqComponent.EventId = eventId;
            reqComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
            // reqComponent.ExtendedAssemblyComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == CompGp).ptyExtendedAssemblyComponentTableName;
            reqComponent.ExtendedComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == CompGp).ptyExtendedComponentTableName;
            reqComponent.ComponentGroupingName = CompGp;
            reqComponent.PartType = parttype;
            reqComponent.PartTypePrimaryKey = partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType;
            MetaDataDTO mdManufaturer = reqComponent.Fields.FirstOrDefault(x => x.Title == "Manufacturer");
            ControlIdTextDTO[] listManufacturers = GetMetadataReferenceDataDDL(mdManufaturer, partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType.ToString());
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_Manufacturers").DataValue = listManufacturers.FirstOrDefault(x => x.ControlText == manufacturer).ControlId;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcCompName").DataValue = CompName;
            MetaDataDTO mdCatDescription = reqComponent.Fields.FirstOrDefault(x => x.Title == "Catalog Item Description");
            ControlIdTextDTO[] listCatDescription = GetMetadataReferenceDataDDL(mdCatDescription, partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType.ToString(), listManufacturers.FirstOrDefault(x => x.ControlText == manufacturer).ControlId.ToString());
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCatalogItem").DataValue = listCatDescription.FirstOrDefault(x => x.ControlText == CatalogItem).ControlId;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcInnerDiameter").DataValue = InnerDiameter;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcOuterDiameter").DataValue = OuterDiameter;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Assembly").DataValue = assemblyId;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_SubAssembly").DataValue = subassemblyId;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascQuantity").DataValue = Quantity.ToString();
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascLength").DataValue = Length.ToString();
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascTopDepth").DataValue = TopDepth.ToString();
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascBottomDepth").DataValue = (Length + TopDepth).ToString();
            List<ComponentMetaDataGroupDTO> listComponent = new List<ComponentMetaDataGroupDTO>();
            listComponent.Add(reqComponent);

            ComponentMetaDataGroupDTO addtnlComponent = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Additional");
            addtnlComponent.JobId = Convert.ToInt64(jobId);
            addtnlComponent.EventId = eventId;
            addtnlComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
            // reqComponent.ExtendedAssemblyComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == CompGp).ptyExtendedAssemblyComponentTableName;
            addtnlComponent.ExtendedComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == CompGp).ptyExtendedComponentTableName;
            addtnlComponent.ComponentGroupingName = CompGp;
            addtnlComponent.PartType = parttype;
            addtnlComponent.PartTypePrimaryKey = partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType;
            listComponent.Add(addtnlComponent);

            if (cmpMetaData.Count() > 2)
            {
                ComponentMetaDataGroupDTO partTypeSpecificComponent = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Part Type Specific");
                partTypeSpecificComponent.JobId = Convert.ToInt64(jobId);
                partTypeSpecificComponent.EventId = eventId;
                partTypeSpecificComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
                // reqComponent.ExtendedAssemblyComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == CompGp).ptyExtendedAssemblyComponentTableName;
                partTypeSpecificComponent.ExtendedComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == CompGp).ptyExtendedComponentTableName;
                partTypeSpecificComponent.ComponentGroupingName = CompGp;
                partTypeSpecificComponent.PartType = parttype;
                partTypeSpecificComponent.PartTypePrimaryKey = partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType;
                if (CompGp.ToUpper() == "BOREHOLE" && parttype.ToUpper() == "WELLBORE COMPLETION DETAIL (PERFORATIONS, ETC.)")
                {
                    if (wellBorePerfStatus != 1)
                        partTypeSpecificComponent.Fields.FirstOrDefault(x => x.ColumnName == "apsFK_r_WellPerforationStatus").DataValue = wellBorePerfStatus;
                }

                listComponent.Add(partTypeSpecificComponent);
            }

            if (cmpMetaData.Count() > 3)
            {
                ComponentMetaDataGroupDTO pumpDetailsComponent = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Pump Details");
                pumpDetailsComponent.JobId = Convert.ToInt64(jobId);
                pumpDetailsComponent.EventId = eventId;
                pumpDetailsComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
                pumpDetailsComponent.ExtendedComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == CompGp).ptyExtendedComponentTableName;
                pumpDetailsComponent.ComponentGroupingName = CompGp;
                pumpDetailsComponent.PartType = parttype;
                pumpDetailsComponent.PartTypePrimaryKey = partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType;

                listComponent.Add(pumpDetailsComponent);
            }

            ComponentMetaDataGroupDTO[] arrComponent = listComponent.ToArray();

            //ComponentPartTypeDTO details = new ComponentPartTypeDTO();
            //details.JobId = Convert.ToInt64(jobId);
            //details.ComponentId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == CompGp).ptgFK_c_MfgCat_ComponentGrouping;
            //details.PartTypeId = partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType;

            ComponentMetaDataGroupBatchCollectionDTO batchDetailsComp = new ComponentMetaDataGroupBatchCollectionDTO();
            batchDetailsComp.ActionPerformed = CRUDOperationTypes.Add;
            batchDetailsComp.ComponentMetadataCollection = arrComponent;
            //batchDetailsComp.ComponentPartType = details;

            List<ComponentMetaDataGroupBatchCollectionDTO> listComp = new List<ComponentMetaDataGroupBatchCollectionDTO>();
            listComp.Add(batchDetailsComp);
            ComponentMetaDataGroupBatchCollectionDTO[] arrComp = listComp.ToArray();

            bool saveBatch = ComponentService.SaveWellboreComponent(arrComp);
        }


        public void AddWellSetting(long wellId, string settingName, int value = 0)
        {
            SettingDTO setting = SettingService.GetSettingByName(settingName);
            Assert.IsNotNull(setting, "Failed to get settings for " + settingName);
            if (setting.MinValue == null || setting.MaxValue == null)
                return;
            WellSettingDTO wellSetting = new WellSettingDTO();
            wellSetting.Id = 0;
            wellSetting.NumericValue = value == 0 ? random.Next((int)setting.MinValue, (int)setting.MaxValue) : value;
            wellSetting.SettingId = setting.Id;
            wellSetting.WellId = wellId;
            SettingService.SaveWellSetting(wellSetting);
        }

        public void AddWellSettingWithDoubleValues(long wellId, string settingName, double value = 0)
        {
            SettingDTO setting = SettingService.GetSettingByName(settingName);
            Assert.IsNotNull(setting, "Failed to get settings for " + settingName);
            if (setting.MinValue == null || setting.MaxValue == null)
                return;
            WellSettingDTO wellSetting = new WellSettingDTO();
            wellSetting.Id = 0;
            wellSetting.NumericValue = value == 0 ? random.Next((int)setting.MinValue, (int)setting.MaxValue) : value;
            wellSetting.SettingId = setting.Id;
            wellSetting.WellId = wellId;
            long settingId = SettingService.SaveWellSetting(wellSetting);
            Assert.IsTrue(settingId > 0, "Setting is not added successfully");
            Trace.WriteLine("Added Setting:" + settingName);
        }

        public void DeleteWellSettings(long wellId, string settingName)
        {
            WellSettingDTO[] wellSettings = SettingService.GetWellSettingsByWellId(wellId.ToString());

            foreach (WellSettingDTO setting in wellSettings)
            {
                if (setting.Setting.Name == settingName)
                {
                    SettingService.RemoveWellSetting(setting.Id.ToString());
                }
            }
            Trace.WriteLine("Deleted Setting:" + settingName);
        }

        protected WellSettingDTO[] AddOperatingLimit(WellTypeId wellType, long wellid)
        {
            switch (wellType)
            {
                case WellTypeId.PCP:
                    {

                        //Overriding -- Adding Operating Limit
                        AddWellSetting(wellid, "Min Motor Amps Operating Limit", 101);
                        AddWellSetting(wellid, "Max Motor Amps Operating Limit", 401);

                        AddWellSetting(wellid, "Min Pump Discharge Pressure Operating Limit", 102);
                        AddWellSetting(wellid, "Max Pump Discharge Pressure Operating Limit", 402);

                        AddWellSetting(wellid, "Min Pump Intake Pressure Operating Limit", 103);
                        AddWellSetting(wellid, "Max Pump Intake Pressure Operating Limit", 403);

                        AddWellSetting(wellid, "Min Casing Head Pressure Operating Limit", 104);
                        AddWellSetting(wellid, "Max Casing Head Pressure Operating Limit", 404);

                        AddWellSetting(wellid, "Min Flow Line Pressure Operating Limit", 105);
                        AddWellSetting(wellid, "Max Flow Line Pressure Operating Limit", 405);

                        AddWellSetting(wellid, "Min Tubing Head Pressure Operating Limit", 106);
                        AddWellSetting(wellid, "Max Tubing Head Pressure Operating Limit", 406);

                        AddWellSetting(wellid, "Min Water Rate Operating Limit", 107);
                        AddWellSetting(wellid, "Max Water Rate Operating Limit", 407);

                        AddWellSetting(wellid, "Min Oil Rate Operating Limit", 108);
                        AddWellSetting(wellid, "Max Oil Rate Operating Limit", 408);

                        AddWellSetting(wellid, "Min Speed Operating Limit", 109);
                        AddWellSetting(wellid, "Max Speed Operating Limit", 409);

                        AddWellSetting(wellid, "Min Tubing Head Temperature Operating Limit", 110);
                        AddWellSetting(wellid, "Max Tubing Head Temperature Operating Limit", 410);

                        AddWellSetting(wellid, "Min Pump Torque Operating Limit", 111);
                        AddWellSetting(wellid, "Max Pump Torque Operating Limit", 411);

                        AddWellSetting(wellid, "Min Motor Volts Operating Limit", 112);
                        AddWellSetting(wellid, "Max Motor Volts Operating Limit", 412);

                        AddWellSetting(wellid, "Min Run Time Operating Limit", 13);
                    }
                    break;

                default:
                    Trace.WriteLine(string.Format("Invalid Well Type"));
                    break;
            }
            WellSettingDTO[] operatingLimit = SettingService.GetWellSettingsByWellId(wellid.ToString());
            return operatingLimit;

        }

        protected ModelConfigDTO ReturnFullyPopulatedModel(double tubingAnchorDepth = 0.0, double casingOD = 0.0)
        {
            // Surface.
            SurfaceConfigDTO surfaceConfig = new SurfaceConfigDTO();
            PumpingUnitManufacturerDTO[] manufacturers = CatalogService.GetAllPumpingUnitManufacturers();
            PumpingUnitManufacturerDTO pumpingUnitManufacturer = manufacturers.FirstOrDefault(pumt => pumt.Name.Equals("Lufkin"));
            Assert.IsNotNull(pumpingUnitManufacturer, "Failed to get pumping unit manufacturer.");
            PumpingUnitTypeDTO[] pumpingUnitTypes = CatalogService.GetPumpingUnitTypesByManufacturer(pumpingUnitManufacturer.Name);
            PumpingUnitTypeDTO pumpingUnitType = pumpingUnitTypes.FirstOrDefault(t => t.AbbreviatedName.Equals("C"));
            Assert.IsNotNull(pumpingUnitType, "Failed to get pumping unit type.");
            PumpingUnitDTO[] pumpingUnits = CatalogService.GetPumpingUnitsByManufacturerAndType(pumpingUnitManufacturer.Name, pumpingUnitType.AbbreviatedName);
            PumpingUnitDTO pumpingUnitBase = pumpingUnits.FirstOrDefault(pu => pu.Description.Equals("C-912-365-168 L LUFKIN C912-365-168 (94110C)"));
            Assert.IsNotNull(pumpingUnitBase, "Failed to get pumping unit (base).");
            PumpingUnitDTO pumpingUnit = CatalogService.GetPumpingUnitByManufacturerTypeAndDescription(pumpingUnitManufacturer.Name, pumpingUnitType.AbbreviatedName, pumpingUnitBase.Description);
            Assert.IsNotNull(pumpingUnit, "Failed to get pumping unit.");
            surfaceConfig.PumpingUnit = pumpingUnit;
            surfaceConfig.PumpingUnitType = pumpingUnitType;
            surfaceConfig.ClockwiseRotation = PumpingUnitRotationDirection.Counterclockwise;
            surfaceConfig.MotorAmpsDown = 120;
            surfaceConfig.MotorAmpsUp = 144;
            RRLMotorTypeDTO[] motorType = CatalogService.GetAllMotorTypes();
            surfaceConfig.MotorType = motorType.FirstOrDefault(mt => mt.Name == "Nema D");
            RRLMotorSizeDTO[] motorSize = CatalogService.GetAllMotorSizes();
            surfaceConfig.MotorSize = motorSize.FirstOrDefault(ms => ms.SizeInHP == 50);
            RRLMotorSlipDTO[] motorSlip = CatalogService.GetAllMotorSlips();
            surfaceConfig.SlipTorque = motorSlip.FirstOrDefault(ms => ms.Rating == 1);
            surfaceConfig.WristPinPosition = 2; // Equivalent to 3 in the UI.
            surfaceConfig.ActualStrokeLength = pumpingUnit.StrokeLengthsAtPins[2];

            // Weights.
            CrankWeightsConfigDTO weightsConfig = new CrankWeightsConfigDTO();
            POPRRLCranksDTO[] cranks = CatalogService.GetCranksByPumpingUnitPK(pumpingUnitManufacturer.Name, pumpingUnitType.AbbreviatedName, pumpingUnit.Description);
            POPRRLCranksDTO crank = cranks.FirstOrDefault(c => c.CrankId.Equals("94110C"));
            weightsConfig.CrankId = crank.CrankId;
            POPRRLCranksWeightsDTO cranksWeights = CatalogService.GetCrankWeightsByCrankId(crank.CrankId);
            weightsConfig.CBT = cranksWeights.CrankCBT;
            weightsConfig.TorqCalcMethod = TorqueCalculationMethod.API;
            const string primaryWeightName = "OORO";
            const string noneWeightName = "None";
            int primaryWeightIndex = cranksWeights.PrimaryIdentifier.IndexOf(primaryWeightName);
            int primaryNoneIndex = cranksWeights.PrimaryIdentifier.IndexOf(noneWeightName);
            int auxNoneIndex = cranksWeights.AuxiliaryIdentifier.IndexOf(noneWeightName);
            weightsConfig.Crank_1_Auxiliary = new RRLAuxiliaryWeightDTO { LagId = cranksWeights.AuxiliaryIdentifier[auxNoneIndex], LeadId = cranksWeights.AuxiliaryIdentifier[auxNoneIndex] };
            weightsConfig.Crank_1_Primary = new RRLPrimaryWeightDTO
            {
                LagId = cranksWeights.PrimaryIdentifier[primaryNoneIndex],
                LeadId = cranksWeights.PrimaryIdentifier[primaryWeightIndex],
                LeadMDistance = cranksWeights.DistanceM[primaryWeightIndex],
                LeadWeight = (int)cranksWeights.Primary[primaryWeightIndex],
                LeadDistance = 0.0,
                LagDistance = 0.0
            };
            weightsConfig.Crank_2_Auxiliary = new RRLAuxiliaryWeightDTO { LagId = cranksWeights.AuxiliaryIdentifier[auxNoneIndex], LeadId = cranksWeights.AuxiliaryIdentifier[auxNoneIndex] };
            weightsConfig.Crank_2_Primary = new RRLPrimaryWeightDTO
            {
                LagId = cranksWeights.PrimaryIdentifier[primaryNoneIndex],
                LeadId = cranksWeights.PrimaryIdentifier[primaryWeightIndex],
                LeadMDistance = cranksWeights.DistanceM[primaryWeightIndex],
                LeadWeight = (int)cranksWeights.Primary[primaryWeightIndex],
                LeadDistance = 0.0,
                LagDistance = 0.0

            };
            weightsConfig.PumpingUnitCrankCBT = cranksWeights.PumpingUnitCrankCBT;
            POPRRLCranksWeightsDTO jackedUpWeightInfoForCBT = new POPRRLCranksWeightsDTO();
            jackedUpWeightInfoForCBT.Auxiliary = new List<double>() { weightsConfig.Crank_1_Auxiliary.LeadWeight, weightsConfig.Crank_1_Auxiliary.LagWeight, weightsConfig.Crank_2_Auxiliary.LeadWeight, weightsConfig.Crank_2_Auxiliary.LagWeight };
            jackedUpWeightInfoForCBT.AuxiliaryIdentifier = new List<string>() { weightsConfig.Crank_1_Auxiliary.LeadId, weightsConfig.Crank_1_Auxiliary.LagId, weightsConfig.Crank_2_Auxiliary.LeadId, weightsConfig.Crank_2_Auxiliary.LagId };
            jackedUpWeightInfoForCBT.CrankId = weightsConfig.CrankId;
            jackedUpWeightInfoForCBT.DistanceM = new List<double>() { weightsConfig.Crank_1_Primary.LeadMDistance ?? 0, weightsConfig.Crank_1_Primary.LagMDistance ?? 0, weightsConfig.Crank_2_Primary.LeadMDistance ?? 0, weightsConfig.Crank_2_Primary.LagMDistance ?? 0 };
            jackedUpWeightInfoForCBT.DistanceT = new List<double>() { weightsConfig.Crank_1_Primary.LeadDistance ?? 0, weightsConfig.Crank_1_Primary.LagDistance ?? 0, weightsConfig.Crank_2_Primary.LeadDistance ?? 0, weightsConfig.Crank_2_Primary.LagDistance ?? 0 };
            jackedUpWeightInfoForCBT.Primary = new List<double>() { weightsConfig.Crank_1_Primary.LeadWeight, weightsConfig.Crank_1_Primary.LagWeight, weightsConfig.Crank_2_Primary.LeadWeight, weightsConfig.Crank_2_Primary.LagWeight };
            jackedUpWeightInfoForCBT.PrimaryIdentifier = new List<string>() { weightsConfig.Crank_1_Primary.LeadId, weightsConfig.Crank_1_Primary.LagId, weightsConfig.Crank_2_Primary.LeadId, weightsConfig.Crank_2_Primary.LagId };
            jackedUpWeightInfoForCBT.PumpingUnitCrankCBT = weightsConfig.PumpingUnitCrankCBT;
            weightsConfig.CBT = ModelFileService.CalculateCBT(jackedUpWeightInfoForCBT);

            // Downhole.
            DownholeConfigDTO downholeConfig = new DownholeConfigDTO();
            downholeConfig.PumpDiameter = 2.0;
            downholeConfig.PumpDepth = 5081;
            downholeConfig.TubingAnchorDepth = tubingAnchorDepth;
            downholeConfig.CasingOD = casingOD;
            downholeConfig.TubingID = 2.441;
            downholeConfig.TubingOD = 2.875;
            downholeConfig.TopPerforation = 4558.0;
            downholeConfig.BottomPerforation = 5220.0;

            // Rods.
            RodStringConfigDTO rodStringConfig = new RodStringConfigDTO();
            List<RodTaperConfigDTO> rodTapers = new List<RodTaperConfigDTO>();
            RodTaperConfigDTO taper1 = new RodTaperConfigDTO();
            taper1.Grade = "D";
            taper1.Manufacturer = "Weatherford, Inc.";
            taper1.NumberOfRods = 56;
            taper1.RodGuid = "";
            taper1.RodLength = 30.0;
            taper1.ServiceFactor = 0.9;
            taper1.Size = 1.0;
            taper1.TaperLength = taper1.NumberOfRods * taper1.RodLength;
            rodTapers.Add(taper1);
            RodTaperConfigDTO taper2 = new RodTaperConfigDTO();
            taper2.Grade = "D";
            taper2.Manufacturer = "Weatherford, Inc.";
            taper2.NumberOfRods = 57;
            taper2.RodGuid = "";
            taper2.RodLength = 30.0;
            taper2.ServiceFactor = 0.9;
            taper2.Size = 0.875;
            taper2.TaperLength = taper2.NumberOfRods * taper2.RodLength;
            rodTapers.Add(taper2);
            RodTaperConfigDTO taper3 = new RodTaperConfigDTO();
            taper3.Grade = "D";
            taper3.Manufacturer = "Weatherford, Inc.";
            taper3.NumberOfRods = 56;
            taper3.RodGuid = "";
            taper3.RodLength = 30.0;
            taper3.ServiceFactor = 0.9;
            taper3.Size = 0.75;
            taper3.TaperLength = taper3.NumberOfRods * taper3.RodLength;
            rodTapers.Add(taper3);
            rodStringConfig.TotalRodLength = 0;
            foreach (RodTaperConfigDTO taper in rodTapers) { rodStringConfig.TotalRodLength += taper.TaperLength; }
            rodStringConfig.RodTapers = rodTapers.ToArray();

            //Model File
            ModelConfigDTO SampleModel = new ModelConfigDTO();
            SampleModel.Weights = weightsConfig;
            SampleModel.Rods = rodStringConfig;
            SampleModel.Downhole = downholeConfig;
            SampleModel.Surface = surfaceConfig;

            return SampleModel;
        }

        protected WellDTO AddRRLWell(string BaseFacTag)
        {
            Authenticate();
            WellDTO[] wells = WellService.GetAllWells();
            var wellConfigDTO = new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = BaseFacTag,
                    FacilityId = BaseFacTag,
                    DataConnection = GetDefaultCygNetDataConnection(),
                    CommissionDate = DateTime.Today.AddYears(-4),
                    AssemblyAPI = BaseFacTag,
                    SubAssemblyAPI = BaseFacTag,
                    IntervalAPI = BaseFacTag,
                    WellType = WellTypeId.RRL,
                    SurfaceLatitude = (decimal?)random.Next(-90, 90),
                    SurfaceLongitude = (decimal?)random.Next(-180, 180),
                    WellDepthDatumId = 1,
                    Engineer = BaseFacTag
                })
            };
            wellConfigDTO.ModelConfig = ReturnFullyPopulatedModel(); // test fully configured model
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(addedWellConfig.Well.Id.ToString()).Units;
            //Add Well Test
            WellTestDTO testDataDTO = new WellTestDTO
            {
                WellId = addedWellConfig.Well.Id,
                //WellTestType = WellTestType.WellTest.ToString(),
                AverageCasingPressure = 120,
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
                SPTCodeDescription = WellTestQuality.RepresentativeTest.ToString(),
                StrokePerMinute = 0,
                TestDuration = 3,
                Water = 75,
                WaterGravity = 1.010M,
            };
            testDataDTO.SampleDate = addedWellConfig.Well.CommissionDate.Value.AddDays(2).ToUniversalTime();
            WellTestDataService.AddWellTestData(new WellTestAndUnitsDTO(units, testDataDTO));

            return addedWellConfig.Well;
        }

        public int GetIndexOfAttribute(DBEntityDTO table, string attributeName)
        {
            int index = 0;
            for (index = 0; index < table.Attributes.Length; index++)
            {
                if (table.Attributes[index].AttributeName.Contains(attributeName))
                    break;

            }
            return index;
        }

        public DBEntityDTO GetTableData(string tableName)
        {
            EntityGridSettingDTO setting = new EntityGridSettingDTO();
            setting.GridSetting = new GridSettingDTO { PageSize = 20, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
            setting.EntityName = tableName;
            int tableDataCount = DBEntityService.GetTableDataCount(setting);
            setting.GridSetting = new GridSettingDTO { PageSize = tableDataCount, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
            DBEntityDTO tableData = DBEntityService.GetTableData(setting);
            return tableData;
        }
        protected WellDTO AddRRLWellNonZeroGasrate(string BaseFacTag)
        {
            Authenticate();
            WellDTO[] wells = WellService.GetAllWells();
            var wellConfigDTO = new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = BaseFacTag,
                    FacilityId = BaseFacTag,
                    DataConnection = GetDefaultCygNetDataConnection(),
                    CommissionDate = DateTime.Today.AddYears(-2),
                    AssemblyAPI = BaseFacTag,
                    SubAssemblyAPI = BaseFacTag,
                    IntervalAPI = BaseFacTag,
                    WellType = WellTypeId.RRL,
                    Engineer = BaseFacTag
                })
            };
            wellConfigDTO.ModelConfig = ReturnFullyPopulatedModel(); // test fully configured model
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(addedWellConfig.Well.Id.ToString()).Units;
            //Add Well Test
            WellTestDTO testDataDTO = new WellTestDTO
            {
                WellId = addedWellConfig.Well.Id,
                AverageCasingPressure = 1,
                AverageFluidAbovePump = 1,
                AverageTubingPressure = 90,
                AverageTubingTemperature = 100,
                Gas = 25,
                GasGravity = 0,
                Oil = 83,
                OilGravity = 25,
                PumpEfficiency = 0,
                PumpIntakePressure = 100,
                PumpingHours = 10,
                SPTCodeDescription = WellTestQuality.RepresentativeTest.ToString(),
                StrokePerMinute = 0,
                TestDuration = 3,
                Water = 75,
                WaterGravity = 1.010M,
            };
            testDataDTO.SampleDate = addedWellConfig.Well.CommissionDate.Value.AddDays(1).ToUniversalTime();
            WellTestDataService.AddWellTestData(new WellTestAndUnitsDTO(units, testDataDTO));

            return addedWellConfig.Well;
        }

        protected void AddQAtoComponentforJob(string JobId, string SubAsemblyId, decimal[] details, string cg, string pType, int mf, int cid)
        {
            Authenticate();
            AddQAComponent(cg, pType, mf, cid);
            AddComponent(JobId, SubAsemblyId, details);
            RemoveQA();
        }

        protected void AddComponent(string JobId, string SubAsemblyId, decimal[] details)
        {
            QuickAddComponentGroupingDTO QaComps = ComponentService.GetQuickAddComponents().FirstOrDefault(x => x.GroupingCategory == "Add Components Grouping");
            List<QuickAddComponentDTO> componentQA = new List<QuickAddComponentDTO>();
            //Add Component through Quick Add
            foreach (QuickAddComponentDTO qaComp in QaComps.QuickAddComponents)
            {
                componentQA.Add(qaComp);
                qaComp.JobId = Convert.ToInt64(JobId);
                qaComp.SubAssemblyId = Convert.ToInt64(SubAsemblyId);
                ComponentMetaDataGroupBatchCollectionDTO[] qaMetadatagroup = ComponentService.GetMetadataFromQuickAddComponents(componentQA.ToArray());
                MetaDataDTO[] reqFileds = qaMetadatagroup.FirstOrDefault().ComponentMetadataCollection.FirstOrDefault(x => x.CategoryName == "Required").Fields;
                MetaDataDTO Quantity = reqFileds.FirstOrDefault(x => x.Title == "Quantity");
                MetaDataDTO Length = reqFileds.FirstOrDefault(x => x.Title == "Length");
                MetaDataDTO TopDepth = reqFileds.FirstOrDefault(x => x.Title == "Top Depth");
                MetaDataDTO BottomDepth = reqFileds.FirstOrDefault(x => x.Title == "Bottom Depth");
                if (Quantity != null)
                    Quantity.DataValue = details[0];
                if (Length != null)
                    Length.DataValue = details[1];
                if (TopDepth != null)
                    TopDepth.DataValue = details[2];
                if (BottomDepth != null)
                    BottomDepth.DataValue = details[1] + details[2];
                //Add Batch Component
                bool saveBatch = ComponentService.SaveWellboreComponent(qaMetadatagroup);
                componentQA.Clear();
            }
        }

        protected void RemoveComponents(string jobId, long eventId)
        {
            WellboreGridDTO[] wellboreComponent = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, eventId.ToString());
            List<ComponentMetaDataGroupBatchCollectionDTO> listComp = new List<ComponentMetaDataGroupBatchCollectionDTO>();
            ComponentMetaDataGroupBatchCollectionDTO[] arrComp = null;
            foreach (WellboreGridGroupDTO comp in wellboreComponent.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup)
            {

                ComponentMetaDataGroupBatchCollectionDTO batchDetailsComp = new ComponentMetaDataGroupBatchCollectionDTO();
                batchDetailsComp.ActionPerformed = CRUDOperationTypes.Remove;

                ComponentPartTypeDTO Ids = new ComponentPartTypeDTO();
                ComponentMetaDataGroupDTO mdComp = comp.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required");
                var ascId = mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascPrimaryKey").DataValue;
                Ids.JobId = Convert.ToInt64(jobId);
                Ids.AssemblyComponentId = Convert.ToInt64(ascId);
                Ids.PartTypeId = (int)mdComp.PartTypePrimaryKey;
                Ids.ComponentId = mdComp.ComponentPrimaryKey;
                Ids.EventId = eventId;
                batchDetailsComp.ComponentPartType = Ids;
                listComp.Add(batchDetailsComp);
                arrComp = listComp.ToArray();
            }
            bool rComp = ComponentService.SaveWellboreComponent(arrComp);
            Assert.IsTrue(rComp);
            WellboreGridDTO[] wellboreComponent1 = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, eventId.ToString());
            Assert.IsTrue(wellboreComponent1.Count() == 0, "All components are not removed");
            Trace.WriteLine("All the components are removed");
        }

        protected void RemoveQA()
        {
            QuickAddComponentGroupingDTO QaComp = ComponentService.GetQuickAddComponents().FirstOrDefault(x => x.GroupingCategory == "Add Components Grouping");
            foreach (QuickAddComponentDTO qaComp in QaComp.QuickAddComponents)
            {
                bool removeQAcomp = ComponentService.RemoveQuickAddComponent(qaComp.QuickAddComponentId.ToString());
            }
        }

        protected void AddQAComponent(string cg, string pType, int mf, int cid)
        {
            //cg - strComponentGrouping, pType - ptyPartType
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            RRLPartTypeComponentGroupingTypeDTO compGrp = componentGroups.FirstOrDefault(x => x.strComponentGrouping.Trim().ToUpper() == cg.Trim().ToUpper());
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = compGrp.ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            RRLPartTypeComponentGroupingTypeDTO ptype = partTypes.FirstOrDefault(x => x.ptyPartType.Trim().ToUpper() == pType.Trim().ToUpper());
            ComponentMetaDataGroupDTO[] cmpMetaData = ComponentService.GetMetaDataForQuickAddComponentByPartTypeId(ptype.ptgFK_c_MfgCat_PartType.ToString());
            MetaDataDTO[] cdReference = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;

            //Get Meta data for the Catalog Item description
            MetaDataReferenceData cd = new MetaDataReferenceData();
            cd.MetaData = cdReference.FirstOrDefault(x => x.Title == "Manufacturer");
            //Manufacturer
            MetaDataDTO mdManufacturer = cd.MetaData;
            ControlIdTextDTO[] listManufacturers = GetMetadataReferenceDataDDL(mdManufacturer, ptype.ptgFK_c_MfgCat_PartType.ToString());
            ControlIdTextDTO manufacturer = listManufacturers.FirstOrDefault(x => x.ControlId == mf);
            MetaDataFilterDTO cdFilter = new MetaDataFilterDTO();
            cdFilter.ColumnValue = manufacturer.ControlId.ToString();
            cdFilter.MetaDataFilterToken = cd.MetaData.ExtendedFilterInput;
            //CatalogItem Description
            MetaDataDTO mdCatDescription = cdReference.FirstOrDefault(x => x.Title == "Catalog Item Description");
            ControlIdTextDTO[] listCatDescription = GetMetadataReferenceDataDDL(mdCatDescription, ptype.ptgFK_c_MfgCat_PartType.ToString(), manufacturer.ControlId.ToString());
            ControlIdTextDTO catDescription = listCatDescription.FirstOrDefault(x => x.ControlId == cid);
            //Add Quick Add Component
            ComponentMetaDataGroupDTO reqComponent = new ComponentMetaDataGroupDTO();
            reqComponent.QuickAddCategory = "Add Components Grouping";
            reqComponent.PartTypePrimaryKey = ptype.ptgFK_c_MfgCat_PartType;
            reqComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
            reqComponent.PartType = ptype.ptyPartType;
            reqComponent.ExtendedComponentTable = ptype.ptyExtendedComponentTableName;
            reqComponent.CategoryName = "Required";
            reqComponent.Order = 1;
            reqComponent.Fields = cdReference;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCatalogItem").DataValue = catDescription.ControlId;// Catalog Item description
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcCompName").DataValue = ptype.ptyPartType;//Name
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_Manufacturers").DataValue = manufacturer.ControlId;//Manufacturer
            ComponentMetaDataGroupDTO addComponent = new ComponentMetaDataGroupDTO();
            addComponent.Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Additional").Fields;
            addComponent.CategoryName = "Additional";
            List<ComponentMetaDataGroupDTO> listComponent = new List<ComponentMetaDataGroupDTO>();
            listComponent.Add(reqComponent);
            listComponent.Add(addComponent);
            ComponentMetaDataGroupDTO psComponent = new ComponentMetaDataGroupDTO();
            if (cmpMetaData.Count() > 2)
            {
                psComponent.Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Part Type Specific").Fields;
                psComponent.CategoryName = "Part Type Specific";
                listComponent.Add(psComponent);
            }
            ComponentMetaDataGroupDTO[] arrComponent = listComponent.ToArray();
            string addQuickComponent = ComponentService.AddQuickAddComponent(arrComponent);
        }

        protected ControlIdTextDTO[] GetMetadataReferenceDataDDL(MetaDataDTO metaData, string columnValue = "", string columnValue1 = "")
        {
            MetaDataReferenceData cd = new MetaDataReferenceData();
            cd.MetaData = metaData;
            MetaDataFilterDTO cdFilter = new MetaDataFilterDTO();
            List<MetaDataFilterDTO> listcdFilter = new List<MetaDataFilterDTO>();
            if (columnValue != "")
            {
                if (columnValue1 != "")
                {
                    MetaDataFilterDTO cdFilter1 = new MetaDataFilterDTO();
                    cdFilter1.ColumnValue = columnValue1;
                    listcdFilter.Add(cdFilter1);
                    cd.UIFilterValues = listcdFilter.ToArray();
                }
                cdFilter.ColumnValue = columnValue;
                cdFilter.MetaDataFilterToken = cd.MetaData.ExtendedFilterInput;
                listcdFilter.Add(cdFilter);
                cd.UIFilterValues = listcdFilter.ToArray();
            }
            else
                cd.UIFilterValues = null;

            ControlIdTextDTO[] cdMetaData = JobAndEventService.GetMetaDataReferenceData(cd);

            return cdMetaData;
        }

        protected void ChangeUnitSystemUserSetting(string type)
        {

            Authenticate();
            SettingType settingType = SettingType.User;
            SettingDTO settings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.UNIT_SYSTEM);
            UserSettingDTO[] userSettings = SettingService.GetUserSettingsByUserId(AuthenticatedUser.Id.ToString());
            userSettings[0].StringValue = type;
            SettingService.SaveUserSetting(userSettings[0]);
            UserSettingDTO[] userSettings1 = SettingService.GetUserSettingsByUserId(AuthenticatedUser.Id.ToString());
            Assert.AreEqual(type, userSettings1[0].StringValue, "Unable to change the Unit System");
        }
        public void ChangeUnitSystem(string type)
        {
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name == SettingServiceStringConstants.UNIT_SYSTEM);
            SystemSettingDTO settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
            settingValue.StringValue = type;
            SettingService.SaveSystemSetting(settingValue);
            settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
            Assert.AreEqual(type, settingValue.StringValue, "Unable to Change the Unit System");
        }
        protected void SetValuesInSystemSettings(string settingName, string settingValue)
        {
            Authenticate();
            SystemSettingDTO systemSetting = SettingService.GetSystemSettingByName(settingName);
            SettingValueType settingValueType = systemSetting.Setting.SettingValueType;
            switch (settingValueType)
            {
                case SettingValueType.DecimalNumber:
                    if (!string.IsNullOrEmpty(settingValue))
                    {
                        systemSetting.NumericValue = Convert.ToDouble(decimal.Parse(settingValue));
                        SettingService.SaveSystemSetting(systemSetting);
                    }
                    break;
                case SettingValueType.Number:
                case SettingValueType.TrueOrFalse:
                    systemSetting.NumericValue = int.Parse(settingValue);
                    SettingService.SaveSystemSetting(systemSetting);
                    Assert.AreEqual(settingValue, systemSetting.NumericValue.ToString(), "Unable to Change the System Setting Value for : " + settingName);
                    break;
                default:
                    systemSetting.StringValue = settingValue;
                    SettingService.SaveSystemSetting(systemSetting);
                    Assert.AreEqual(settingValue, systemSetting.StringValue, "Unable to Change the System Setting Value for : " + settingName);
                    break;
            }
        }
        protected WellDTO AddOTWell(string BaseFacTag)
        {
            Authenticate();
            WellDTO[] wells = WellService.GetAllWells();
            var wellConfigDTO = new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = "OTTestwell",
                    FacilityId = BaseFacTag,
                    DataConnection = GetDefaultCygNetDataConnection(),
                    CommissionDate = DateTime.Today.AddYears(-2),
                    AssemblyAPI = BaseFacTag,
                    SubAssemblyAPI = BaseFacTag,
                    IntervalAPI = BaseFacTag,
                    WellType = WellTypeId.OT,
                    Engineer = BaseFacTag
                })
            };
            wellConfigDTO.ModelConfig = ReturnFullyPopulatedModel(); // test fully configured model
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);


            return addedWellConfig.Well;
        }

        protected WellConfigDTO AddNewRRLWellConfigFullModel(string facility_tag)
        {
            bool dcExisted = false;
            string facilityId;
            if (s_isRunningInATS)
                facilityId = facility_tag + "00001";
            else
                facilityId = facility_tag + "0001";
            WellDTO well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = facilityId, DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = (DateTime.Today - TimeSpan.FromDays(100)), AssemblyAPI = "1234567890", SubAssemblyAPI = "123456789012", IntervalAPI = "12345678901234", WellType = WellTypeId.RRL });
            dcExisted = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == well.DataConnection.ProductionDomain && dc.Site == well.DataConnection.Site && dc.Service == well.DataConnection.Service) != null;
            WellDTO[] wells = WellService.GetAllWells();
            Assert.IsFalse(wells.Any(w => w.Name.Equals(well.Name)), "Well already exists in database.");
            WellConfigDTO wellConfigDTO = new WellConfigDTO();
            wellConfigDTO.Well = well;
            wellConfigDTO.ModelConfig = ReturnFullyPopulatedModel(); // test fully configured model
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);

            return addedWellConfig;

        }

        public static class DynaCardTypes
        {
            public const int UnknownType = 0;
            public const int Current = 1;
            public const int Full = 2;
            public const int Pumpoff = 3;
            public const int Alarm = 4;
            public const int Failure = 5;
            public const int Ideal = 6;
            public const int Start = 7;
            public const int Reference = 8;
            public const int Pattern = 99;

            public const int CurrentBuffer = 9;
            public const int FullBuffer = 10;
            public const int PumpoffBuffer = 11;
            public const int AlarmBuffer = 12;
            public const int FailureBuffer = 13;

        }


        public void DeleteLeftOverWellsBeforeTest()
        {
            bool failedelvtn = false;
            try
            {
                Authenticate();
                WellDTO[] wells = WellService.GetAllWells();

                if (wells.Length != 0)
                {
                    foreach (WellDTO well in wells)
                    {
                        try
                        {
                            WellConfigurationService.RemoveWellConfig(well.Id.ToString());
                        }
                        catch (Exception)
                        {
                            failedelvtn = true;

                        }
                        if (failedelvtn == false)
                        {
                            Trace.WriteLine(well.Name + " deleted successfully before test start");
                        }
                        else
                        {
                            Trace.WriteLine("Error encountered on deleting Well " + well.Name);
                        }
                    }
                }
                else
                    Trace.WriteLine("There is no well present in ForeSite before test start");
            }
            catch (Exception e)
            {
                Assert.Fail("Exception : " + e.ToString() + (e.InnerException == null ? "" : Environment.NewLine + e.InnerException.ToString()));
            }
        }

        public void RunAnalysisTaskScheduler(string args)
        {
            try
            {
                string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string topparent = Directory.GetParent(assemblyPath).ToString();
                Trace.WriteLine($"Location for Foresite Scheduler from Dyncamic Parent : {LocateFileInGivenDirectiory(topparent, "ForeSite.Scheduler.Jobs.exe")}");
                Trace.WriteLine($"Run Time path for Test Assembly is: {assemblyPath} ");
                string strFolder = ConfigurationManager.AppSettings.Get("ForeSiteServerPath");
                string strCommand = (s_isRunningInATS) ? Path.Combine(strFolder, "ForeSite.Scheduler.Jobs.exe") : strCommand = Path.Combine(assemblyPath, "ForeSite.Scheduler.Jobs.exe");

                Process FSScheduler = new Process();
                //    FSScheduler.StartInfo.UseShellExecute = true;
                FSScheduler.StartInfo.FileName = strCommand;
                FSScheduler.StartInfo.Arguments = args;
                FSScheduler.StartInfo.Verb = "runas";
                Console.WriteLine($"ForeSite Scheduler Path: {strCommand}");
                FSScheduler.Start();
                if (args.Contains("runVRR"))
                {
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(15));
                    return;
                }
                if (FSScheduler.Responding)
                {
                    Console.WriteLine("FSScheduler " + FSScheduler.ProcessName + " is running at process id  " + FSScheduler.Id + ". ");
                }
                FSScheduler.WaitForExit();
                Console.WriteLine($"FSScheduler {FSScheduler.ProcessName} has completed .");

            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to start FSScheduler process: {0}", e.ToString());
            }
        }

        public object GetForeSiteToolBoxSettingValue(string settingNameEnum)
        {
            object rettype = null;
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = null;
            SystemSettingDTO settingValue = null;

            systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Name.ToUpper() == settingNameEnum.ToUpper());
            if (systemSettings != null)
            {
                 settingValue = SettingService.GetSystemSettingByName(systemSettings.Name);
            }
            
            if (settingValue == null)
            {
                settingValue = SettingService.GetSystemSettingByName(settingNameEnum);
            }
            if (settingValue == null)
            {
                Trace.WriteLine($"Did not get System SEtting by name {settingNameEnum}");
            }
            if (settingValue.StringValue != null)
            {
                rettype = settingValue.StringValue;
            }
            else if (settingValue.NumericValue != null)
            {
                rettype = settingValue.NumericValue;
            }
            return rettype;
        }

        public bool CheckIfCanExecuteForeSiteSchedulerJob()
        {
            string strCommand = "";
            string strFolder = ConfigurationManager.AppSettings.Get("ForeSiteServerPath");
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string topparent = Directory.GetParent(assemblyPath).ToString();
            Trace.WriteLine($"Location for Foresite Scheduler from Dyncamic Parent : {LocateFileInGivenDirectiory(topparent, "ForeSite.Scheduler.Jobs.exe")}");
            Trace.WriteLine($"Run Time path for Test Assembly is: {assemblyPath} ");
            strCommand = (s_isRunningInATS) ? Path.Combine(strFolder, "ForeSite.Scheduler.Jobs.exe") : strCommand = Path.Combine(topparent, "POPServer\\ForeSite.Scheduler.Jobs.exe");
            bool execheck = (File.Exists(strCommand)) ? true : false;
            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            Trace.WriteLine($" Scheduler EXE EXIST IN SPECIFIED PATH : {strCommand} = {execheck}  and is User having Elevated rights to run it {isElevated}");
            return execheck && isElevated;
        }

        public string LocateFileInGivenDirectiory(string sDir, string filename)
        {
            string[] files = Directory.GetFiles(sDir, "*.exe", SearchOption.AllDirectories);
            string filelocation = (files.FirstOrDefault(x => x.ToString().Contains(filename)) == null) ? "Notfound" : files.FirstOrDefault(x => x.ToString().Contains(filename));
            return filelocation;


        }

        public string ReadFromXMlFile(string path)
        {
            string[] resarrary = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            Stream xmlstream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            string result = string.Empty;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }

        public void AddUpdateRelativeFacilitySystemSettingForWellQuantities(string mappingName, WellTypeId wellTypeId, Dictionary<WellQuantity, string> wellQuantities, string globalsettingfile = "BSS\\SETTINGS\\GlobalSettings.gsf")
        {
            //Set Facility mapping mode
            SetFacilityMappingMode();

            //Check if a mapping exist for the well type
            Guid mappingId;
            CheckIfRelativeFacilityMappingExistForWellType(wellTypeId, out mappingId);

            //Save system setting for Relative facility mapping 
            string relativeFacilityMappingConst = SettingServiceStringConstants.RELATIVE_FACILITY_MAPPING_SCHEMES;
            SettingDTO settingDTO = SettingService.GetSettingByName(relativeFacilityMappingConst);
            Assert.AreEqual(settingDTO.Name, relativeFacilityMappingConst, "Setting does not exist.");

            string fromXML = ReadFromXMlFile("Weatherford.POP.Server.IntegrationTests.TestDocuments.GLRelativeFacilityMapping.xml");

            RelativeFacilityConfiguration relativeFacilityConfig = RelativeFacilityConfiguration.FromXml(fromXML);
            RelativeFacilityDefinitionFile relativeFacilityDefinitionFile = relativeFacilityConfig.Files.FirstOrDefault();
            RelativeFacilityMapping relativeFacilityMapping = relativeFacilityConfig.Mappings.FirstOrDefault();

            relativeFacilityDefinitionFile.Path = ConfigurationManager.AppSettings.Get("Site") + "." + globalsettingfile;
            relativeFacilityMapping.Id = mappingId;
            relativeFacilityMapping.Name = mappingName;
            relativeFacilityMapping.WellType = wellTypeId;
            foreach (KeyValuePair<WellQuantity, string> wellQuantity in wellQuantities)
            {
                relativeFacilityMapping.Quantities.Where(q => q.Quantity == wellQuantity.Key).FirstOrDefault().Relation = wellQuantity.Value;
            }

            SystemSettingDTO systemSettingRelFacility = new SystemSettingDTO();
            systemSettingRelFacility.Setting = settingDTO;
            systemSettingRelFacility.StringValue = relativeFacilityConfig.ToXml();

            long pk = SettingService.SaveSystemSetting(systemSettingRelFacility);
            Assert.AreNotEqual(0, pk, $"Returned value primary key should not be zero for setting {settingDTO.Name} after add.");

            systemSettingRelFacility = SettingService.GetSystemSettingByName(SettingServiceStringConstants.RELATIVE_FACILITY_MAPPING_SCHEMES);
            Assert.IsNotNull(systemSettingRelFacility);
            RelativeFacilityConfiguration getRelativeFacilityConfig = RelativeFacilityConfiguration.FromXml(systemSettingRelFacility.StringValue);
            Assert.AreEqual(getRelativeFacilityConfig.Mappings.Count, 1, "Relative facility mapping was not saved.");
            Assert.AreEqual(getRelativeFacilityConfig.Mappings.FirstOrDefault().Quantities.FirstOrDefault(x => x.Quantity == wellQuantities.FirstOrDefault().Key).Relation, wellQuantities.FirstOrDefault().Value, "Relative facility mapping does not match for Well Quantity");
        }

        public void RemoveRelativeFacilitySystemSetting()
        {
            SystemSettingDTO relFacilitySystemSetting = SettingService.GetSystemSettingByName(SettingServiceStringConstants.RELATIVE_FACILITY_MAPPING_SCHEMES);
            RelativeFacilityConfiguration relativeFacilityConfiguration = RelativeFacilityConfiguration.FromXml(relFacilitySystemSetting.StringValue);
            relativeFacilityConfiguration.Mappings = new List<RelativeFacilityMapping>();
            relFacilitySystemSetting.StringValue = relativeFacilityConfiguration.ToXml();
            SettingService.SaveSystemSetting(relFacilitySystemSetting);

            relFacilitySystemSetting = SettingService.GetSystemSettingByName(SettingServiceStringConstants.RELATIVE_FACILITY_MAPPING_SCHEMES);
            relativeFacilityConfiguration = RelativeFacilityConfiguration.FromXml(relFacilitySystemSetting.StringValue);
            Assert.AreEqual(relativeFacilityConfiguration.Mappings.Count, 0, "");

            SystemSettingDTO extrelFacilitySystemSetting = SettingService.GetSystemSettingByName(SettingServiceStringConstants.EXTENDED_FACILITY_MAPPING_MODE);
            SettingService.RemoveSystemSetting(extrelFacilitySystemSetting.Id.ToString());

            extrelFacilitySystemSetting = SettingService.GetSystemSettingByName(SettingServiceStringConstants.EXTENDED_FACILITY_MAPPING_MODE);
            Assert.AreEqual(extrelFacilitySystemSetting.StringValue, "None", "System system was not removed.");
        }

        public bool CheckIfRelativeFacilityMappingExistForWellType(WellTypeId wellTypeId, out Guid mappingId)
        {
            SystemSettingDTO systemSettingDTO = SettingService.GetSystemSettingByName(SettingServiceStringConstants.RELATIVE_FACILITY_MAPPING_SCHEMES);
            RelativeFacilityConfiguration relativeFacilityConfig = !string.IsNullOrWhiteSpace(systemSettingDTO.StringValue) ? RelativeFacilityConfiguration.FromXml(systemSettingDTO.StringValue) : null;
            RelativeFacilityMapping mapping = relativeFacilityConfig?.Mappings?.Where(m => m.WellType == wellTypeId)?.FirstOrDefault();
            mappingId = (mapping == null) ? Guid.NewGuid() : mapping.Id;
            return (mapping != null);
        }

        public void AddUpdateRelativeFacilitySystemSettingForAdditionalUDCs(string mappingName, WellTypeId wellTypeId)
        {
            //Set Facility mapping mode
            SetFacilityMappingMode();

            //Check if a mapping exist for the well type
            Guid mappingId;
            bool mappingExists = CheckIfRelativeFacilityMappingExistForWellType(wellTypeId, out mappingId);

            //Save system setting for Relative facility mapping 
            string relativeFacilityMappingConst = SettingServiceStringConstants.RELATIVE_FACILITY_MAPPING_SCHEMES;
            SettingDTO settingDTO = SettingService.GetSettingByName(relativeFacilityMappingConst);
            Assert.AreEqual(settingDTO.Name, relativeFacilityMappingConst, "Setting does not exist.");

            RelativeFacilityConfiguration relativeFacilityConfig;
            if (!mappingExists)
            {
                string fromXML = ReadFromXMlFile("Weatherford.POP.Server.IntegrationTests.TestDocuments.GLRelativeFacilityMapping.xml");
                relativeFacilityConfig = RelativeFacilityConfiguration.FromXml(fromXML);
            }
            else
            {
                SystemSettingDTO systemSettingDTO = SettingService.GetSystemSettingByName(SettingServiceStringConstants.RELATIVE_FACILITY_MAPPING_SCHEMES);
                relativeFacilityConfig = RelativeFacilityConfiguration.FromXml(systemSettingDTO.StringValue);
            }

            RelativeFacilityDefinitionFile relativeFacilityDefinitionFile = relativeFacilityConfig.Files.FirstOrDefault();
            RelativeFacilityMapping relativeFacilityMapping = relativeFacilityConfig.Mappings.FirstOrDefault();

            relativeFacilityDefinitionFile.Path = ConfigurationManager.AppSettings.Get("Site") + "." + ConfigurationManager.AppSettings.Get("RelativeFacilityGlobalSettingsPath");
            relativeFacilityMapping.Id = mappingId;
            relativeFacilityMapping.Name = mappingName;
            relativeFacilityMapping.WellType = wellTypeId;

            if (relativeFacilityMapping.AdditionalUDCs.Count == 0)
            {
                SystemSettingDTO systemSettingAdditionalUDC = SettingService.GetSystemSettingByName(SettingServiceStringConstants.GL_GROUP_STATUS_EXTRA_UDCS);
                ConfigurableUDCInfo configurableUDC = new ConfigurableUDCInfo();
                configurableUDC.Parse(systemSettingAdditionalUDC.StringValue);

                foreach (var configurableUD in configurableUDC.Info)
                {
                    relativeFacilityMapping.AdditionalUDCs.Add(new RelativeAdditionalUDCMapping() { UDC = configurableUD.Value.Name });
                }
            }

            relativeFacilityMapping.AdditionalUDCs.LastOrDefault().Relation = "Gas Meter";

            SystemSettingDTO systemSettingRelFacility = new SystemSettingDTO();
            systemSettingRelFacility.Setting = settingDTO;
            systemSettingRelFacility.StringValue = relativeFacilityConfig.ToXml();

            long pk = SettingService.SaveSystemSetting(systemSettingRelFacility);
            Assert.AreNotEqual(0, pk, $"Returned value primary key should not be zero for setting {settingDTO.Name} after add.");
        }

        public void AddUpdateRelativeFacilitySystemSettingForAdditionalUDCs(string mappingName, WellTypeId wellTypeId, string gsffile, string optudcname, string relationname)
        {
            //Set Facility mapping mode
            SetFacilityMappingMode();

            //Check if a mapping exist for the well type
            Guid mappingId;
            bool mappingExists = CheckIfRelativeFacilityMappingExistForWellType(wellTypeId, out mappingId);

            //Save system setting for Relative facility mapping 
            string relativeFacilityMappingConst = SettingServiceStringConstants.RELATIVE_FACILITY_MAPPING_SCHEMES;
            SettingDTO settingDTO = SettingService.GetSettingByName(relativeFacilityMappingConst);
            Assert.AreEqual(settingDTO.Name, relativeFacilityMappingConst, "Setting does not exist.");

            RelativeFacilityConfiguration relativeFacilityConfig;
            if (!mappingExists)
            {
                string fromXML = ReadFromXMlFile("Weatherford.POP.Server.IntegrationTests.TestDocuments.GLRelativeFacilityMapping.xml");
                relativeFacilityConfig = RelativeFacilityConfiguration.FromXml(fromXML);
            }
            else
            {
                SystemSettingDTO systemSettingDTO = SettingService.GetSystemSettingByName(SettingServiceStringConstants.RELATIVE_FACILITY_MAPPING_SCHEMES);
                relativeFacilityConfig = RelativeFacilityConfiguration.FromXml(systemSettingDTO.StringValue);
            }

            RelativeFacilityDefinitionFile relativeFacilityDefinitionFile = relativeFacilityConfig.Files.FirstOrDefault();
            RelativeFacilityMapping relativeFacilityMapping = relativeFacilityConfig.Mappings.FirstOrDefault();

            relativeFacilityDefinitionFile.Path = ConfigurationManager.AppSettings.Get("Site") + "." + gsffile;
            relativeFacilityMapping.Id = mappingId;
            relativeFacilityMapping.Name = mappingName;
            relativeFacilityMapping.WellType = wellTypeId;
            if (relationname != "")
            {
                relativeFacilityMapping.AdditionalUDCs.Add(new RelativeAdditionalUDCMapping() { UDC = optudcname, Relation = relationname });
            }
            if (relationname == "")
            {
                relativeFacilityMapping.AdditionalUDCs.FirstOrDefault(u => u.UDC == optudcname).Relation = "";
                relativeFacilityMapping.AdditionalUDCs.Remove(relativeFacilityMapping.AdditionalUDCs.FirstOrDefault(u => u.UDC == "PRTUBXIN"));
            }
            List<RelativeFacilityMapping> relist = new List<RelativeFacilityMapping>();
            relist.Add(relativeFacilityMapping);
            SystemSettingDTO systemSettingRelFacility = new SystemSettingDTO();
            systemSettingRelFacility.Setting = settingDTO;
            relativeFacilityConfig.Mappings = relist;

            systemSettingRelFacility.StringValue = relativeFacilityConfig.ToXml();

            long pk = SettingService.SaveSystemSetting(systemSettingRelFacility);
            Assert.AreNotEqual(0, pk, $"Returned value primary key should not be zero for setting {settingDTO.Name} after add.");
        }

        public void AddUpdateRelativeFacilitySystemSettingForWellCommands(string mappingName, WellTypeId wellTypeId, Dictionary<WellCommand, string> wellCommands)
        {
            //Set Facility mapping mode
            SetFacilityMappingMode();

            //Check if a mapping exist for the well type
            Guid mappingId;
            bool mappingExists = CheckIfRelativeFacilityMappingExistForWellType(wellTypeId, out mappingId);

            //Save system setting for Relative facility mapping 
            string relativeFacilityMappingConst = SettingServiceStringConstants.RELATIVE_FACILITY_MAPPING_SCHEMES;
            SettingDTO settingDTO = SettingService.GetSettingByName(relativeFacilityMappingConst);
            Assert.AreEqual(settingDTO.Name, relativeFacilityMappingConst, "Setting does not exist.");

            string fromXML = ReadFromXMlFile("Weatherford.POP.Server.IntegrationTests.TestDocuments.GLRelativeFacilityMapping.xml");

            RelativeFacilityConfiguration relativeFacilityConfig = RelativeFacilityConfiguration.FromXml(fromXML);
            RelativeFacilityDefinitionFile relativeFacilityDefinitionFile = relativeFacilityConfig.Files.FirstOrDefault();
            RelativeFacilityMapping relativeFacilityMapping = relativeFacilityConfig.Mappings.FirstOrDefault();

            relativeFacilityDefinitionFile.Path = ConfigurationManager.AppSettings.Get("Site") + "." + ConfigurationManager.AppSettings.Get("RelativeFacilityGlobalSettingsPath");
            relativeFacilityMapping.Id = mappingId;
            relativeFacilityMapping.Name = mappingName;
            relativeFacilityMapping.WellType = wellTypeId;
            if (wellCommands != null)
            {
                foreach (KeyValuePair<WellCommand, string> wellCommand in wellCommands)
                {
                    relativeFacilityMapping.Commands.Where(q => q.Command == wellCommand.Key).FirstOrDefault().Relation = wellCommand.Value;
                }
            }

            SystemSettingDTO systemSettingRelFacility = new SystemSettingDTO();
            systemSettingRelFacility.Setting = settingDTO;
            systemSettingRelFacility.StringValue = relativeFacilityConfig.ToXml();

            long pk = SettingService.SaveSystemSetting(systemSettingRelFacility);
            Assert.AreNotEqual(0, pk, $"Returned value primary key should not be zero for setting {settingDTO.Name} after add.");

            systemSettingRelFacility = SettingService.GetSystemSettingByName(SettingServiceStringConstants.RELATIVE_FACILITY_MAPPING_SCHEMES);
            Assert.IsNotNull(systemSettingRelFacility);
            RelativeFacilityConfiguration getRelativeFacilityConfig = RelativeFacilityConfiguration.FromXml(systemSettingRelFacility.StringValue);
            Assert.AreEqual(getRelativeFacilityConfig.Mappings.Count, 1, "Relative facility mapping was not saved.");
            if (wellCommands != null)
                Assert.AreEqual(getRelativeFacilityConfig.Mappings.FirstOrDefault().Commands.FirstOrDefault(x => x.Command == wellCommands.FirstOrDefault().Key).Relation, wellCommands.FirstOrDefault().Value, "Relative facility mapping does not match for Well Command");
        }

        public void AddUpdateRelativeFacilitySystemSettingForSetPoints(string mappingName, WellTypeId wellTypeId)
        {
            //Set Facility mapping mode
            SetFacilityMappingMode();

            //Check if a mapping exist for the well type
            Guid mappingId;
            bool mappingExists = CheckIfRelativeFacilityMappingExistForWellType(wellTypeId, out mappingId);

            //Save system setting for Relative facility mapping 
            string relativeFacilityMappingConst = SettingServiceStringConstants.RELATIVE_FACILITY_MAPPING_SCHEMES;
            SettingDTO settingDTO = SettingService.GetSettingByName(relativeFacilityMappingConst);
            Assert.AreEqual(settingDTO.Name, relativeFacilityMappingConst, "Setting does not exist.");

            string fromXML = ReadFromXMlFile("Weatherford.POP.Server.IntegrationTests.TestDocuments.GLRelativeFacilityMapping.xml");

            RelativeFacilityConfiguration relativeFacilityConfig = RelativeFacilityConfiguration.FromXml(fromXML);
            RelativeFacilityDefinitionFile relativeFacilityDefinitionFile = relativeFacilityConfig.Files.FirstOrDefault();
            RelativeFacilityMapping relativeFacilityMapping = relativeFacilityConfig.Mappings.FirstOrDefault();

            relativeFacilityDefinitionFile.Path = ConfigurationManager.AppSettings.Get("Site") + "." + ConfigurationManager.AppSettings.Get("RelativeFacilityGlobalSettingsPath");
            relativeFacilityMapping.Id = mappingId;
            relativeFacilityMapping.Name = mappingName;
            relativeFacilityMapping.WellType = wellTypeId;
            relativeFacilityMapping.SetPointInfo.PrimarySetPointDG = new SetPointDGInfo("SetPoints", "Basic SetPoints", "GETSPDATA", "C_SETPOINT", null);

            SystemSettingDTO systemSettingRelFacility = new SystemSettingDTO();
            systemSettingRelFacility.Setting = settingDTO;
            systemSettingRelFacility.StringValue = relativeFacilityConfig.ToXml();

            long pk = SettingService.SaveSystemSetting(systemSettingRelFacility);
            Assert.AreNotEqual(0, pk, $"Returned value primary key should not be zero for setting {settingDTO.Name} after add.");

            systemSettingRelFacility = SettingService.GetSystemSettingByName(SettingServiceStringConstants.RELATIVE_FACILITY_MAPPING_SCHEMES);
            Assert.IsNotNull(systemSettingRelFacility);
            RelativeFacilityConfiguration getRelativeFacilityConfig = RelativeFacilityConfiguration.FromXml(systemSettingRelFacility.StringValue);
            Assert.AreEqual(getRelativeFacilityConfig.Mappings.Count, 1, "Relative facility mapping was not saved.");

            Assert.IsNotNull(getRelativeFacilityConfig.Mappings.FirstOrDefault().SetPointInfo, "Relative facility mapping does not match for Well Command");
        }


        public void SetFacilityMappingMode()
        {
            string relativeFacilityModeConst = SettingServiceStringConstants.EXTENDED_FACILITY_MAPPING_MODE;
            SettingDTO settingDTO = SettingService.GetSettingByName(relativeFacilityModeConst);
            Assert.AreEqual(settingDTO.Name, relativeFacilityModeConst, "Setting does not exist.");

            SystemSettingDTO sysSettingRelFacMode = new SystemSettingDTO() { Setting = settingDTO, StringValue = "RelativeFacilities" };
            long pk = SettingService.SaveSystemSetting(sysSettingRelFacMode);
            Assert.AreNotEqual(0, pk, $"Returned value primary key should not be zero for setting {settingDTO.Name} after add.");
        }

        public void CopyAlarmsCSVDefault()
        {
            string[] resarrary = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            string RRLAlarmRecordFolderPath = @"C:\EdgeAlarms";
            string alamfile = Path.Combine(RRLAlarmRecordFolderPath, "AlarmTypes.csv");
            if (!Directory.Exists(RRLAlarmRecordFolderPath))
            {
                Directory.CreateDirectory(RRLAlarmRecordFolderPath);
            }
            string alarmfile = resarrary.FirstOrDefault(f => f.Contains("AlarmTypes.csv")).ToString();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream resourceStream = assembly.GetManifestResourceStream(alarmfile);
            if (resourceStream != null)
            {
                using (Stream input = resourceStream)
                {
                    using (Stream output = File.Create(alamfile))
                    {
                        input.CopyTo(output);
                    }
                }
            }
        }

        public static string GetFileFromAssemblyStream(string filename)
        {
            string[] resarrary = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            string inputfile = resarrary.FirstOrDefault(f => f.Contains(filename)).ToString();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream resourceStream = assembly.GetManifestResourceStream(inputfile);
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(path), "TestDocuments\\" + filename)))
            {
                if (!Directory.Exists(Path.Combine(Path.GetDirectoryName(path), "TestDocuments\\")))
                {
                    Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(path), "TestDocuments\\"));
                }
                if (resourceStream != null)
                {
                    using (Stream input = resourceStream)
                    {
                        using (Stream output = File.Create(Path.Combine(Path.GetDirectoryName(path), "TestDocuments\\" + filename)))
                        {
                            input.CopyTo(output);
                        }
                    }
                }
            }
            return Path.Combine(Path.GetDirectoryName(path), "TestDocuments\\" + filename);


        }

        public void UpdateSystemSettings(WellDTO well, string[] settingNameArray, double[] settingValueArray)
        {
            //update   system settings
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
            SettingService.SaveWellSettings(wellsettings.ToArray());

        }

        protected static WellDTO SetNFCondensate(WellDTO well)
        {
            well.FluidType = WellFluidType.Condensate;
            well.WellType = WellTypeId.NF;
            return well;
        }



        protected void AddModelFile(DateTime date, WellTypeId type, long wellId, CalibrationMethodId tuningMethod, string wellname)
        {
            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            byte[] fileAsByteArray;
            ModelFileValidationDataDTO ModelFileValidationData;
            AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(wellId.ToString());
            ModelFileOptionDTO options = new ModelFileOptionDTO();
            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };
            modelFile.ApplicableDate = date.AddDays(1).ToUniversalTime();
            modelFile.WellId = wellId;
            switch (type)
            {
                case WellTypeId.NF:
                    {
                        fileAsByteArray = GetByteArray(Path, wellname);
                        options.CalibrationMethod = tuningMethod;
                        options.Comment = "NF";
                        options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        modelFile.Options = options;
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                        else
                            Trace.WriteLine(string.Format("Failed to validate NF model file"));
                        //Add Well Test
                        WellTestDTO testDataDTO = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "RepresentativeTest",
                            AverageTubingPressure = 164.7m,
                            AverageTubingTemperature = 100,
                            GaugePressure = 5800,
                            Oil = 1769.5m,
                            Gas = 880,
                            Water = 589.8m,
                            ChokeSize = 50,
                            FlowLinePressure = 50,
                            SeparatorPressure = 10000,
                        };
                        testDataDTO.SampleDate = date.AddDays(2).ToUniversalTime();
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                        break;
                    }
                case WellTypeId.GLift:
                    {
                        fileAsByteArray = GetByteArray(Path, "GL-01-Base.wflx");
                        options.CalibrationMethod = tuningMethod;
                        options.Comment = "Gas Lift";
                        options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        modelFile.Options = options;
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                        else
                            Trace.WriteLine(string.Format("Failed to validate NF model file"));
                        //Add Well Test
                        WellTestDTO testDataDTO = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "AllocatableTest",
                            AverageTubingPressure = 600,
                            AverageTubingTemperature = 98,
                            AverageCasingPressure = 1800,
                            GasInjectionRate = 400,
                            //FlowLinePressure = 50,
                            //SeparatorPressure = 30,
                            //GaugePressure = 12000,
                            Oil = 1500,
                            Gas = 1600,
                            Water = 1500,
                            ChokeSize = 28,
                        };
                        testDataDTO.SampleDate = date.AddDays(2).ToUniversalTime();
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                        break;
                    }
                case WellTypeId.ESP:
                    {
                        fileAsByteArray = GetByteArray(Path, "WellfloESPExample1.wflx");
                        options.CalibrationMethod = tuningMethod;
                        options.Comment = "ESP";
                        options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        modelFile.Options = options;
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                        else
                            Trace.WriteLine(string.Format("Failed to validate NF model file"));
                        //Add Well Test
                        WellTestDTO testDataDTO = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "AllocatableTest",
                            AverageTubingPressure = 1795,
                            AverageCasingPressure = 0,
                            AverageTubingTemperature = 100,
                            PumpIntakePressure = 2674,
                            //PumpDischargePressure = 3067.59m,
                            //GaugePressure = 2900,
                            Oil = 1157,
                            Gas = 596,
                            Water = 1367,
                            ChokeSize = 64,
                            FlowLinePressure = 1300,
                            SeparatorPressure = 987,
                            Frequency = 57,
                        };
                        testDataDTO.SampleDate = date.AddDays(1).ToUniversalTime();
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                        break;
                    }
                case WellTypeId.WInj:
                    {
                        fileAsByteArray = GetByteArray(Path, "WellfloWaterInjectionExample1.wflx");
                        options.CalibrationMethod = CalibrationMethodId.LFactor;
                        options.Comment = "WInj";
                        options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        modelFile.Options = options;
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);

                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        WellTestDTO testDataDTO = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "AllocatableTest",
                            AverageTubingPressure = random.Next(500, 1500),
                            AverageTubingTemperature = random.Next(50, 100),
                            GaugePressure = random.Next(500, 1900),
                            Water = random.Next(500, 1900),
                            FlowLinePressure = random.Next(50, 100)
                        };
                        testDataDTO.SampleDate = date.AddDays(2).ToUniversalTime();
                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                    }
                    break;

                case WellTypeId.GInj:
                    {
                        fileAsByteArray = GetByteArray(Path, wellname);
                        options.CalibrationMethod = CalibrationMethodId.LFactor;
                        options.Comment = "GInj";
                        options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        modelFile.Options = options;
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        WellTestDTO testDataDTO = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "AllocatableTest",
                            AverageTubingPressure = random.Next(500, 1900),
                            AverageTubingTemperature = random.Next(50, 100),
                            GaugePressure = random.Next(500, 1900),
                            Gas = random.Next(500, 1900),
                            FlowLinePressure = random.Next(50, 100)
                        };
                        testDataDTO.SampleDate = date.AddDays(2).ToUniversalTime();
                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                    }
                    break;

                case WellTypeId.PLift:
                    {
                        fileAsByteArray = GetByteArray(Path, "PL-631.wflx");
                        options.Comment = "PLift";
                        options.OptionalUpdate = new long[] { };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                    }
                    break;
                case WellTypeId.PCP:
                    {
                        fileAsByteArray = GetByteArray(Path, "PCP-Multiphase.wflx");
                        options.CalibrationMethod = tuningMethod;
                        options.Comment = "PCP";
                        options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
                        modelFile.Options = options;
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                        else
                            Trace.WriteLine(string.Format("Failed to validate PCP model file"));

                        WellConfigDTO wellConfigPCP = WellConfigurationService.GetWellConfig(wellId.ToString());
                        //Adding WellTest
                        WellTestDTO testDataDTO = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "Allocatable Test",
                            SampleDate = wellConfigPCP.Well.CommissionDate.Value.AddDays(5).ToUniversalTime(),
                            TestDuration = 12,
                            Oil = (decimal)210.2,
                            Water = (decimal)141.2,
                            Gas = (decimal)10.40,
                            AverageTubingPressure = (decimal)100.00,
                            AverageTubingTemperature = (decimal)80.00,
                            GaugePressure = (decimal)423.00,
                            AverageCasingPressure = (decimal)45.23,
                            PumpIntakePressure = (decimal)161.19,
                            PumpDischargePressure = (decimal)2130.69,
                            PolishedRodTorque = (decimal)361.35,
                            PumpTorque = (decimal)185.23,
                            PumpSpeed = (decimal)225.00,
                            FlowLinePressure = (decimal)1862.00,
                            FlowLineTemperature = (decimal)80.00,
                            SeparatorPressure = (decimal)1523.00,
                            ChokeSize = (decimal)64.00,
                            Comment = "PCPWellTest_Comment_Check",
                            MotorCurrent = (decimal)180.0,
                            MotorVolts = (decimal)230.0
                        };

                        //Saved WellTest Data
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                        Assert.AreEqual(1, WellTestDataService.GetWellTestDataByWellId(wellId.ToString()).Values.Length, "Well Test is not saved successfully");
                        Trace.WriteLine("Well test saved successfully");
                    }
                    break;

                default:
                    Trace.WriteLine(string.Format("Invalid Well type"));
                    break;
            }
        }

        public void UdateWelltestDateForLatestWelltest(WellDTO well, DateTime welltestdate)
        {

        }

        /// <summary>
        /// This API is specifically build for ADNOC configuration.
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="productionWellCount"></param>
        /// <param name="injectionWellCount"></param>
        public void ADNOC_Well_With_WellTest_Configuration(long assetId, long productionWellCount = 0, long injectionWellCount = 0)
        {
            ChangeUnitSystem("US");
            ChangeUnitSystemUserSetting("US");
            try
            {
                #region create Production and Injection Wells with Asset
                List<String> ProductionModelFileName = new List<String> { "GP-1.wflx", "GP-2.wflx", "GP-3.wflx", "GP-4.wflx" };
                List<String> InjectionModelFileName = new List<String> { "GI-1.wflx", "GI-2.wflx" };

                //NFW Condennsate wells creation
                var options_cond = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                            (long) OptionalUpdates.UpdateWCT_WGR,
                            (long) OptionalUpdates.UpdateGOR_CGR
                      }
                };

                //Gas Injection wells creation
                var options_Injection = new ModelFileOptionDTO
                {
                    CalibrationMethod = CalibrationMethodId.ReservoirPressure,
                    OptionalUpdate = new long[]
                      {
                            (long) OptionalUpdates.UpdateWCT_WGR,
                            (long) OptionalUpdates.UpdateGOR_CGR
                      }
                };

                List<WellDTO> NFW_Con = new List<WellDTO>();
                List<WellDTO> Gas_Injection = new List<WellDTO>();

                int j = 0;  // Model Files
                int k = 0;  // Model data
                for (int i = 1; i <= productionWellCount; i++)
                {
                    WellDTO NFWell_Con = AddNonRRLWellGeneral("PTW" + (i), GetFacilityId("NFWWELL_", (i)), WellTypeId.NF, WellFluidType.Condensate, WellFluidPhase.None, "1", assetId);

                    var modelFileName_cond = ProductionModelFileName.ElementAt(j);
                    AddNonRRLModelFile(NFWell_Con, modelFileName_cond, options_cond.CalibrationMethod, options_cond.OptionalUpdate);

                    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(NFWell_Con.Id.ToString()).Units;
                    WellTestDTO testData = new WellTestDTO();
                    if (k == 0)
                    {
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.RESERVOIR_PRESSURE_MIN_AL, 2500);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.RESERVOIR_PRESSURE_MIN_AL, 3000);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.LFACTOR_MIN_AL, 0.6);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.LFACTOR_MAX_AL, 1);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MIN_AL, 0.6);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MAX_AL, 10);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MIN_AL, 1.5);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MAX_AL, 300);

                        testData.WellId = NFWell_Con.Id;
                        testData.SPTCodeDescription = "AllocatableTest";
                        testData.WellTestType = WellTestType.WellTest;
                        testData.AverageTubingPressure = 1566;
                        testData.AverageTubingTemperature = 100;
                        testData.Gas = 45312;
                        testData.Water = 206;
                        testData.Oil = 118.9m;
                        testData.ChokeSize = 50;
                        testData.GaugePressure = 5800;
                        testData.SeparatorPressure = 10000;
                        testData.TestDuration = 24;
                        testData.SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime();

                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));
                        WellTestDTO latestTestData_NF = WellTestDataService.GetLatestWellTestDataByWellId(NFWell_Con.Id.ToString());
                        Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_NF.Status.ToString(), "Well Test Status is not Success");
                    }
                    if (k == 1)
                    {
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MIN_AL, 0.4);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MAX_AL, 10);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MIN_AL, 15);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MAX_AL, 300);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.LFACTOR_MIN_AL, 0.88);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.LFACTOR_MAX_AL, 1.28);

                        testData.WellId = NFWell_Con.Id;
                        testData.SPTCodeDescription = "AllocatableTest";
                        testData.WellTestType = WellTestType.WellTest;
                        testData.AverageTubingPressure = 2920;
                        testData.AverageTubingTemperature = 100;
                        testData.Gas = 53657.60m;
                        testData.Water = 63.4m;
                        testData.Oil = 2820.8m;
                        testData.ChokeSize = 50;
                        testData.GaugePressure = 5800;
                        testData.SeparatorPressure = 10000;
                        testData.FlowLinePressure = 50;
                        testData.TestDuration = 24;
                        testData.SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime();

                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));
                        WellTestDTO latestTestData_NF = WellTestDataService.GetLatestWellTestDataByWellId(NFWell_Con.Id.ToString());
                        Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_NF.Status.ToString(), "Well Test Status is not Success");

                    }
                    if (k == 2)
                    {
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MIN_AL, 0.002);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MAX_AL, 10);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MIN_AL, 15);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MAX_AL, 300);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.LFACTOR_MIN_AL, 0.88);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.LFACTOR_MAX_AL, 1.28);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.RESERVOIR_PRESSURE_MIN_AL, 3000);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.RESERVOIR_PRESSURE_MAX_AL, 4000);

                        testData.WellId = NFWell_Con.Id;
                        testData.SPTCodeDescription = "AllocatableTest";
                        testData.WellTestType = WellTestType.WellTest;
                        testData.AverageTubingPressure = 1559;
                        testData.AverageTubingTemperature = 100;
                        testData.Gas = 26982.40m;
                        testData.Water = 0.1m;
                        testData.Oil = 2103.3m;
                        testData.ChokeSize = 50;
                        testData.GaugePressure = 5800;
                        testData.SeparatorPressure = 10000;
                        testData.FlowLinePressure = 50;
                        testData.TestDuration = 24;
                        testData.SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime();

                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));
                        WellTestDTO latestTestData_NF = WellTestDataService.GetLatestWellTestDataByWellId(NFWell_Con.Id.ToString());
                        Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_NF.Status.ToString(), "Well Test Status is not Success");

                    }
                    if (k == 3)
                    {
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MIN_AL, 0.4);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.WATER_GAS_RATIO_MAX_AL, 10);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MIN_AL, 15);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.CONDENSATE_GAS_RATIO_MAX_AL, 300);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.LFACTOR_MIN_AL, 0.88);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.LFACTOR_MAX_AL, 1.28);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.RESERVOIR_PRESSURE_MIN_AL, 2500);
                        AddWellSettingWithDoubleValues(NFWell_Con.Id, SettingServiceStringConstants.RESERVOIR_PRESSURE_MAX_AL, 3000);

                        testData.WellId = NFWell_Con.Id;
                        testData.SPTCodeDescription = "AllocatableTest";
                        testData.WellTestType = WellTestType.WellTest;
                        testData.AverageTubingPressure = 1800;
                        testData.AverageTubingTemperature = 100;
                        testData.Gas = 49612.80m;
                        testData.Water = 53.8m;
                        testData.Oil = 1564.4m;
                        testData.ChokeSize = 50;
                        testData.GaugePressure = 5800;
                        testData.SeparatorPressure = 10000;
                        testData.FlowLinePressure = 50;
                        testData.TestDuration = 24;
                        testData.SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime();

                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));
                        WellTestDTO latestTestData_NF = WellTestDataService.GetLatestWellTestDataByWellId(NFWell_Con.Id.ToString());
                        Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_NF.Status.ToString(), "Well Test Status is not Success");
                    }

                    if (j == 3)
                    {
                        j = 0;
                        k = 0;
                    }
                    else
                    {
                        j = j + 1;
                        k = k + 1;
                    }

                    NFW_Con.Add(NFWell_Con);
                }
                Trace.WriteLine("Added total no of Production wells : " + NFW_Con.Count().ToString());
                j = 0;
                k = 0;
                for (int i = 1; i <= injectionWellCount; i++)
                {
                    WellDTO GasInjection_Con = AddNonRRLWellGeneral("ITW" + (i), GetFacilityId("INJWELL_", (i)), WellTypeId.GInj, WellFluidType.None, WellFluidPhase.None, "1", assetId);

                    var modelFileName_cond = InjectionModelFileName.ElementAt(j);
                    AddNonRRLModelFile(GasInjection_Con, modelFileName_cond, options_Injection.CalibrationMethod, options_Injection.OptionalUpdate);

                    WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(GasInjection_Con.Id.ToString()).Units;
                    WellTestDTO testData = new WellTestDTO();
                    if (k == 0)
                    {
                        AddWellSettingWithDoubleValues(GasInjection_Con.Id, SettingServiceStringConstants.LFACTOR_MIN_AL, 0.51);
                        AddWellSettingWithDoubleValues(GasInjection_Con.Id, SettingServiceStringConstants.LFACTOR_MAX_AL, 1.28);
                        AddWellSettingWithDoubleValues(GasInjection_Con.Id, SettingServiceStringConstants.RESERVOIR_PRESSURE_MIN_AL, 4500);
                        AddWellSettingWithDoubleValues(GasInjection_Con.Id, SettingServiceStringConstants.RESERVOIR_PRESSURE_MAX_AL, 5000);

                        testData.WellId = GasInjection_Con.Id;
                        testData.SPTCodeDescription = "AllocatableTest";
                        testData.WellTestType = WellTestType.WellTest;
                        testData.AverageTubingPressure = 4277;
                        testData.AverageTubingTemperature = 0;
                        testData.Gas = 32400;
                        testData.Water = 0;
                        testData.Oil = 0;
                        testData.FlowLinePressure = 1366;
                        testData.GaugePressure = 1567;
                        testData.TestDuration = 24;
                        testData.SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime();

                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));
                        WellTestDTO latestTestData_GI = WellTestDataService.GetLatestWellTestDataByWellId(GasInjection_Con.Id.ToString());
                        Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_GI.Status.ToString(), "Well Test Status is not Success");
                    }
                    if (k == 1)
                    {
                        AddWellSettingWithDoubleValues(GasInjection_Con.Id, SettingServiceStringConstants.LFACTOR_MIN_AL, 0.51);
                        AddWellSettingWithDoubleValues(GasInjection_Con.Id, SettingServiceStringConstants.LFACTOR_MAX_AL, 1.70);

                        testData.WellId = GasInjection_Con.Id;
                        testData.SPTCodeDescription = "AllocatableTest";
                        testData.WellTestType = WellTestType.WellTest;
                        testData.AverageTubingPressure = 4290;
                        testData.AverageTubingTemperature = 0;
                        testData.Gas = 46180;
                        testData.Water = 0;
                        testData.Oil = 0;
                        testData.GaugePressure = 1567;
                        testData.FlowLinePressure = 50;
                        testData.TestDuration = 24;
                        testData.SampleDate = DateTime.Today.AddDays(-5).ToUniversalTime();

                        WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testData));
                        WellTestDTO latestTestData_GI = WellTestDataService.GetLatestWellTestDataByWellId(GasInjection_Con.Id.ToString());
                        Assert.AreEqual("TUNING_SUCCEEDED", latestTestData_GI.Status.ToString(), "Well Test Status is not Success");
                    }

                    if (j == 1)
                    {
                        j = 0;
                        k = 0;
                    }
                    else
                    {
                        j = j + 1;
                        k = k + 1;
                    }

                    Gas_Injection.Add(GasInjection_Con);
                }
                Trace.WriteLine("Added total no of Injection wells : " + Gas_Injection.Count().ToString());
                #endregion create Production Wells with Asset
            }
            finally
            {
                ChangeUnitSystem("US");
                ChangeUnitSystemUserSetting("US");
            }
        }

        /// <summary>
        /// This method is developed for preparing ADNOC DB
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="productionWellCount"></param>
        /// <param name="injectionWellCount"></param>
        public void ADNOC_Well_DailyAverageData_Configuration(long assetId, long productionWellCount = 0, long injectionWellCount = 0)
        {
            // Add Daily Average data for those wells 
            DateTime day1ago = DateTime.Today.ToUniversalTime();
            DateTime day2ago = DateTime.Today.ToUniversalTime().AddDays(-1);
            DateTime day3ago = DateTime.Today.ToUniversalTime().AddDays(-2);
            DateTime day4ago = DateTime.Today.ToUniversalTime().AddDays(-3);
            DateTime end = DateTime.Today.ToUniversalTime().AddDays(1);

            int k = 0;
            for (int i = 1; i <= productionWellCount; i++)
            {

                WellDTO getNFWWellId = WellService.GetAllWells().FirstOrDefault(w => w.Name.Contains("PTW" + i));
                var welltestdata = WellTestDataService.GetWellTestDataByWellId(getNFWWellId.Id.ToString());
                if (k == 0)
                {
                    // GP1 well Daily Average Values
                    AddDailyAvergeDataForADNOCWells(getNFWWellId.Id, day4ago, day3ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 12, 0.92, 59.465, 103, 22656);
                    AddDailyAvergeDataForADNOCWells(getNFWWellId.Id, day3ago, day2ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 16, 0.92, 79.286, 137.333, 30208);
                    AddDailyAvergeDataForADNOCWells(getNFWWellId.Id, day2ago, day1ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 20, 0.92, 99.108, 171.666, 37760);
                    AddDailyAvergeDataForADNOCWells(getNFWWellId.Id, day1ago, end, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 24, 0.92, 118.93, 206, 45312);
                }
                if (k == 1)
                {
                    // GP2 well Daily Average Values
                    AddDailyAvergeDataForADNOCWells(getNFWWellId.Id, day4ago, day3ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 12, 0.94, 1524.55, 31.685, 29000);
                    AddDailyAvergeDataForADNOCWells(getNFWWellId.Id, day3ago, day2ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 16, 0.94, 2032.73, 42.246, 38666.666);
                    AddDailyAvergeDataForADNOCWells(getNFWWellId.Id, day2ago, day1ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 22, 0.94, 2795.01, 58.089, 53166.666);
                    AddDailyAvergeDataForADNOCWells(getNFWWellId.Id, day1ago, end, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 24, 0.94, 2820.82, 63.37, 58000);

                }
                if (k == 2)
                {
                    // GP3 well Daily Average Values
                    AddDailyAvergeDataForADNOCWells(getNFWWellId.Id, day4ago, day3ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 16, 0.96, 2338.57, 0.0666666666666667, 30000);
                    AddDailyAvergeDataForADNOCWells(getNFWWellId.Id, day3ago, day2ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 20, 0.96, 2923.22, 0.0833333333333333, 37500);
                    AddDailyAvergeDataForADNOCWells(getNFWWellId.Id, day2ago, day1ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 22, 0.96, 3215.54, 0.0916666666666667, 41250);
                    AddDailyAvergeDataForADNOCWells(getNFWWellId.Id, day1ago, end, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 24, 0.96, 2103.35, 0.1, 45000);
                }
                if (k == 3)
                {
                    // GP4 well Daily Average Values
                    AddDailyAvergeDataForADNOCWells(getNFWWellId.Id, day4ago, day3ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 12, 0.98, 772.523, 26.92, 24500);
                    AddDailyAvergeDataForADNOCWells(getNFWWellId.Id, day3ago, day2ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 16, 0.98, 1030.031, 35.89333, 32666.6666);
                    AddDailyAvergeDataForADNOCWells(getNFWWellId.Id, day2ago, day1ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 24, 0.98, 1545.047, 53.84, 49000);
                    AddDailyAvergeDataForADNOCWells(getNFWWellId.Id, day1ago, end, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 24, 0.98, 1564.37, 53.84, 49000);
                }
                if (k == 3)
                    k = 0;
                else
                    k = k + 1;
            }

            k = 0;
            for (int i = 1; i <= injectionWellCount; i++)
            {

                WellDTO getGIWellId = WellService.GetAllWells().FirstOrDefault(w => w.Name.Contains("ITW" + i));
                var welltestdata = WellTestDataService.GetWellTestDataByWellId(getGIWellId.Id.ToString());
                if (k == 0)
                {
                    // GI1 well Daily Average Values
                    AddDailyAvergeDataForADNOCWells(getGIWellId.Id, day4ago, day3ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 12, null, 0, 0, 34525);
                    AddDailyAvergeDataForADNOCWells(getGIWellId.Id, day3ago, day2ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 24, null, 0, 0, 69050);
                    AddDailyAvergeDataForADNOCWells(getGIWellId.Id, day2ago, day1ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 24, null, 0, 0, 69050);
                    AddDailyAvergeDataForADNOCWells(getGIWellId.Id, day1ago, end, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 24, null, 0, 0, 69050);
                }
                if (k == 1)
                {
                    // GI2 well Daily Average Values
                    AddDailyAvergeDataForADNOCWells(getGIWellId.Id, day4ago, day3ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 12, null, 0, 0, 13860);
                    AddDailyAvergeDataForADNOCWells(getGIWellId.Id, day3ago, day2ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 24, null, 0, 0, 27720);
                    AddDailyAvergeDataForADNOCWells(getGIWellId.Id, day2ago, day1ago, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 24, null, 0, 0, 27720);
                    AddDailyAvergeDataForADNOCWells(getGIWellId.Id, day1ago, end, welltestdata.Values[0].Id, welltestdata.Values[0].LastChangedDT, 24, null, 0, 0, 27720);
                }
                if (k == 1)
                    k = 0;
                else
                    k = k + 1;
            }
        }

        /// <summary>
        /// This API is specifically build for ADNOC configuration.
        /// 1 Reservoir contains 10 Zones. Each Zone will contain 5 patterns.
        /// </summary>
        /// <param name="NoOfResPerAsset"></param>
        /// <param name="NoOfZonePerRes"></param>
        /// <param name="NoOfPatternPerZone"></param>
        /// <param name="existingAsset"></param>
        /// <param name="NoOfAsset"></param>
        public void CreateVRRHierarchy(int NoOfResPerAsset, int NoOfZonePerRes, int NoOfPatternPerZone, List<AssetDTO> existingAsset = null, int NoOfAsset = 0)
        {
            List<AssetDTO> assets = new List<AssetDTO>();
            long getReservoirId=0, getZoneId = 0;
            if (existingAsset != null)
                assets = existingAsset;

            if (NoOfAsset !=0)
                assets = CreateAsset(NoOfAsset);

            for (int j = 0; j <= NoOfAsset; j++)
            {
                for (int k = 0; k < NoOfResPerAsset; k++)
                {
                    AddReservoirToAsset("Reservoir" + (k + 1), assets.ElementAt(j).Id);
                    ReservoirArrayAndUnitsDTO ReservoirIDsOfAsset = WellAllocationService.GetReservoirsByAssetId(assets.ElementAt(j).Id.ToString());
                    foreach (ReservoirDTO reservoir in ReservoirIDsOfAsset.Values)
                    {
                        if (reservoir.Name.Contains("Reservoir" + (k+1)))
                        {
                            getReservoirId = reservoir.Id;
                            break;
                        }
                    }
                    for (int l = 0; l < NoOfZonePerRes; l++)
                    {
                        AddZoneToReservoir("Zone" + (l + 1), getReservoirId);
                        ZoneArrayAndUnitsDTO ZoneIDs = WellAllocationService.GetZonesByReservoirId(getReservoirId.ToString());
                        foreach (ZoneDTO zone in ZoneIDs.Values)
                        {
                            if (zone.Name.Contains("Zone" + (l+1)))
                            {
                                getZoneId = zone.Id;
                                break;
                            }
                        }
                        for (int m = 0; m < NoOfPatternPerZone; m++)
                        {
                            AddPatternToZoneOfReservoir("Z"+(l+1) + "Pattern" + (m + 1), getZoneId);
                        }
                    }
                }
                _vrrHierarchyToRemove.Add(assets[j]);
            }
        }

        public void VRR_TargetConfiguration(List<AssetDTO> existingAsset)
        {
            foreach (AssetDTO asset in existingAsset)
            {
                // GetReservoir IDs
                ReservoirArrayAndUnitsDTO getReservoirByAsset = WellAllocationService.GetReservoirsByAssetId(asset.Id.ToString());

                foreach (ReservoirDTO reservoir in getReservoirByAsset.Values)
                {
                    // Reservoir Value DTO
                    SubsurfaceEntityTargetBoundDTO reservoirTargetBoundValueDTO = new SubsurfaceEntityTargetBoundDTO
                    {
                        ReservoirId = reservoir.Id,
                        ZoneId = null,
                        PatternId = null,
                        StartDate = reservoir.StartDate,
                        EndDate = null,
                        BoundPreference = SubsurfaceEntityTargetType.FixedValue,
                        TargetValue = 1,
                        FixedLowerBoundTolerance = (decimal)0.50,
                        FixedUpperBoundTolerance = 1,
                        PercentageLowerBoundTolerance = null,
                        PercentageUpperBoundTolerance = null
                    };
                    reservoir.ApplicableDate = reservoir.StartDate;
                    // Get Units for Reservoir Target 
                    SubsurfaceEntityTargetBoundAndUnitsDTO addReservoirTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(reservoir.Id.ToString(), SubsurfaceResultEntityType.Reservoir.ToString(), reservoir.ApplicableDate.ToISO8601Date());
                    addReservoirTargets.Value = reservoirTargetBoundValueDTO;
                    // Add Targets for Reservoir
                    WellAllocationService.AddOrUpdateReservoirTargetBound(addReservoirTargets);
                    Trace.WriteLine("Added Targets for Reservoir");

                    // Verify that all the added Targets for Reservoir
                    SubsurfaceEntityTargetBoundAndUnitsDTO verifyReservoirTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(reservoir.Id.ToString(), SubsurfaceResultEntityType.Reservoir.ToString(), reservoir.ApplicableDate.ToISO8601Date());
                    Assert.IsNotNull(verifyReservoirTargets);

                    // Get Zone IDs based on reservoir
                    ZoneArrayAndUnitsDTO getZoneByReservoir = WellAllocationService.GetZonesByReservoirId(reservoir.Id.ToString());

                    foreach (ZoneDTO zone in getZoneByReservoir.Values)
                    {
                        // Zone Value DTO
                        SubsurfaceEntityTargetBoundDTO zoneTargetBoundValueDTO = new SubsurfaceEntityTargetBoundDTO
                        {
                            ReservoirId = null,
                            ZoneId = zone.Id,
                            PatternId = null,
                            StartDate = zone.StartDate,
                            EndDate = null,
                            BoundPreference = SubsurfaceEntityTargetType.FixedValue,
                            TargetValue = 1,
                            FixedLowerBoundTolerance = (decimal)0.40,
                            FixedUpperBoundTolerance = 1,
                            PercentageLowerBoundTolerance = null,
                            PercentageUpperBoundTolerance = null
                        };
                        zone.ApplicableDate = zone.StartDate;
                        // Get Units for Zone Target 
                        SubsurfaceEntityTargetBoundAndUnitsDTO addZoneTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(zone.Id.ToString(), SubsurfaceResultEntityType.Zone.ToString(), zone.ApplicableDate.ToISO8601Date());
                        addZoneTargets.Value = zoneTargetBoundValueDTO;
                        // Add Targets for Zone
                        WellAllocationService.AddOrUpdateZoneTargetBound(addZoneTargets);
                        Trace.WriteLine("Added Targets for Zone");
                        // Verify that all the added Targets for Zone
                        SubsurfaceEntityTargetBoundAndUnitsDTO verifyZoneTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(zone.Id.ToString(), SubsurfaceResultEntityType.Zone.ToString(), zone.ApplicableDate.ToISO8601Date());
                        Assert.IsNotNull(verifyZoneTargets);

                        // Get Pattern Ids based on reservoir            
                        List<PatternDTO> getPatternByZoneOfReservoir = WellAllocationService.GetPatternsByZoneId(zone.Id.ToString());

                        foreach (PatternDTO pat in getPatternByZoneOfReservoir)
                        {
                            // Pattern Value DTO
                            SubsurfaceEntityTargetBoundDTO patternTargetBoundValueDTO = new SubsurfaceEntityTargetBoundDTO
                            {
                                ReservoirId = null,
                                ZoneId = null,
                                PatternId = pat.Id,
                                StartDate = pat.StartDate,
                                EndDate = null,
                                BoundPreference = SubsurfaceEntityTargetType.FixedValue,
                                TargetValue = 1,
                                FixedLowerBoundTolerance = (decimal)0.30,
                                FixedUpperBoundTolerance = 1,
                                PercentageLowerBoundTolerance = null,
                                PercentageUpperBoundTolerance = null
                            };
                            // Get Units for Pattern Target 
                            SubsurfaceEntityTargetBoundAndUnitsDTO addPatternTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(pat.Id.ToString(), SubsurfaceResultEntityType.Pattern.ToString(), zone.ApplicableDate.ToISO8601Date());
                            addPatternTargets.Value = patternTargetBoundValueDTO;
                            // Add Targets for Pattern
                            WellAllocationService.AddOrUpdatePatternTargetBound(addPatternTargets);
                            Trace.WriteLine("Added Targets for Pattern");

                            // Verify that all the added Targets for Pattern
                            SubsurfaceEntityTargetBoundAndUnitsDTO verifyPatternTargets = WellAllocationService.GetSubsurfaceEntityByEntityId(pat.Id.ToString(), SubsurfaceResultEntityType.Pattern.ToString(), zone.ApplicableDate.ToISO8601Date());
                            Assert.IsNotNull(verifyPatternTargets);
                        }
                    }
                }
            }
        }

        public List<AssetDTO> CreateAsset(int NumbersOfAssets)
        {
            string assetName = null, description = null;
            for (int i = 1; i <= NumbersOfAssets; i++)
            {
                assetName = "Asset Test" + i;
                description = "Test Description" + i;

                SurfaceNetworkService.AddAsset(new AssetDTO() { Name = assetName, Description = description });
                var allAssets = SurfaceNetworkService.GetAllAssets().ToList();
                AssetDTO asset = allAssets?.FirstOrDefault(a => a.Name.Equals(assetName));
                Assert.IsNotNull(asset);
                _assetsToRemove.Add(asset);
                if (assetName != null)
                    UpdateUserWithGivenAsset(assetName);
            }
            return SurfaceNetworkService.GetAllAssets().ToList();
        }

        public ReservoirAndUnitsDTO AddReservoirToAsset(string resName, long assetId)
        {
            ReservoirDTO reservoir = new ReservoirDTO
            {
                Name = resName,
                AssetId = assetId,
                Description = resName + " reservoir testing",
                ApplicableDate = DateTime.Today.AddDays(-2).ToLocalTime().Date,
                StartDate = DateTime.Today.AddDays(-2).ToLocalTime().Date,
                EndDate = null,
                InitialOilProductionVolume = 50,
                InitialWaterInjectionVolume = 0,
                InitialWaterProductionVolume = 50,
                InitialGasInjectionVolume = 700,
                InitialGasProductionVolume = 100
            };

            ReservoirAndUnitsDTO getRreservoir = WellAllocationService.GetReservoirById("0");
            getRreservoir.Value = reservoir;
            WellAllocationService.AddOrUpdateReservoir(getRreservoir);
            Assert.IsNotNull(getRreservoir);
            Trace.WriteLine(resName + " reservoir created successfully");

            return getRreservoir;
        }

        public ZoneAndUnitsDTO AddZoneToReservoir(string zoneName, long reservorId)
        {
            ZoneDTO zone = new ZoneDTO
            {
                Name = zoneName,
                ReservoirId = reservorId,
                Description = zoneName + " zone testing",
                ApplicableDate = DateTime.Today.AddDays(-2).ToLocalTime().Date,
                StartDate = DateTime.Today.AddDays(-2).ToLocalTime().Date,
                EndDate = null,
                InitialOilProductionVolume = 50,
                InitialWaterInjectionVolume = 0,
                InitialWaterProductionVolume = 50,
                InitialGasInjectionVolume = 700,
                InitialGasProductionVolume = 100
            };

            ZoneAndUnitsDTO getZone = WellAllocationService.GetZoneById("0");
            getZone.Value = zone;
            WellAllocationService.AddOrUpdateZone(getZone);
            Assert.IsNotNull(getZone);
            Trace.WriteLine(zoneName + " zone created successfully");

            return getZone;
        }

        public PatternDTO AddPatternToZoneOfReservoir(string patName, long zoneId)
        {
            PatternDTO pattern = new PatternDTO
            {
                Name = patName,
                ZoneId = zoneId,
                StartDate = DateTime.Today.AddDays(-2).ToLocalTime(),
                EndDate = null
            };

            WellAllocationService.AddOrUpdatePattern(pattern);
            Assert.IsNotNull(pattern);
            Trace.WriteLine(patName + " pattern created successfully");
            return pattern;
        }

        public WellToZoneDTO AddWellToZone(long wellID, long zoneId)
        {
            WellToZoneDTO well = new WellToZoneDTO
            {
                ZoneId = zoneId,
                WellId = wellID,
                EndDate = null,
                StartDate = DateTime.Today.AddDays(-2).ToLocalTime()
            };
            WellAllocationService.AddWellToZone(well);
            Trace.WriteLine("Well Added to Zone Successfully");
            Assert.IsNotNull(well);
            return well;
        }

        public WellToPatternDTO AddWellToPattern(long wellId, long patternId)
        {
            WellToPatternDTO well = new WellToPatternDTO
            {
                PatternId = patternId,
                WellId = wellId,
                EndDate = null,
                StartDate = DateTime.Today.AddDays(-2).ToLocalTime()
            };
            WellAllocationService.AddWellToPattern(well);
            Trace.WriteLine("Well Added to Pattern Successfully");
            Assert.IsNotNull(well);
            return well;
        }

        /// <summary>
        /// This method will create scenario configuration for TutorialExample3.Reo model  
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public SNScenarioDTO ScenarioConfiguration(long assetId)
        {

            string Path = "Weatherford.POP.Server.IntegrationTests.TestDocuments.";

            var options = new ModelFileOptionDTO
            {
                CalibrationMethod = CalibrationMethodId.LFactor,
                OptionalUpdate = new long[]
                 {
                        (long) OptionalUpdates.UpdateWCT_WGR,
                        (long) OptionalUpdates.UpdateGOR_CGR
                 }
            };

            //Create the wells in ForeSite and assign them to the asset created above
            WellDTO well1 = AddNonRRLWellGeneral("GLWELL_2", GetFacilityId("GLWELL_", 2), WellTypeId.GLift, WellFluidType.BlackOil, WellFluidPhase.None, "1", assetId);
            WellDTO well2 = AddNonRRLWellGeneral("NFWWELL_2", GetFacilityId("NFWWELL_", 2), WellTypeId.NF, WellFluidType.BlackOil, WellFluidPhase.None, "1", assetId);
            WellDTO well3 = AddNonRRLWellGeneral("NFWWELL_3", GetFacilityId("NFWWELL_", 3), WellTypeId.NF, WellFluidType.BlackOil, WellFluidPhase.None, "1", assetId);
            WellDTO well4 = AddNonRRLWellGeneral("NFWWELL_4", GetFacilityId("NFWWELL_", 4), WellTypeId.NF, WellFluidType.BlackOil, WellFluidPhase.None, "1", assetId);


            AddNonRRLModelFile(well1, "GLWell.wflx", options.CalibrationMethod, options.OptionalUpdate);
            AddNonRRLModelFile(well2, "OilWell_01.wflx", options.CalibrationMethod, options.OptionalUpdate);
            AddNonRRLModelFile(well3, "OilWell_02.wflx", options.CalibrationMethod, options.OptionalUpdate);
            AddNonRRLModelFile(well4, "OilWell_03.wflx", options.CalibrationMethod, options.OptionalUpdate);
            Trace.WriteLine("Four Wells are added for Tutorial3");

            //Add a surface network model.
            SNModelAndModelFileDTO modelAndFile = new SNModelAndModelFileDTO();
            modelAndFile.SNModel = new SNModelDTO() { Name = "Tutorial Example 3", AssetId = assetId };

            SNModelFileDTO modelFile = new SNModelFileDTO();
            modelFile.Comments = "Test Model File;";
            byte[] fileAsByteArray = GetByteArray(Path, "TutorialExample3.reo");
            Assert.IsNotNull(fileAsByteArray);
            modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
            modelAndFile.SNModelFile = modelFile;

            //Create Initial Model And ModelFile
            SurfaceNetworkService.SaveReOFile(modelAndFile);
            Trace.WriteLine("One ReO File is added for Tutorial3");

            //Map the ForeSite wells to the Reo wells within the surface network model.
            WellDTO[] wells = WellService.GetWellsByUserAssetIds(new long[] { assetId });
            SNWellMappingDTO[] wellmappings = SurfaceNetworkService.GetSNWellMappingsByAssetIds(new long[] { assetId });
            foreach (SNWellMappingDTO wellToMap in wellmappings)
            {
                foreach (WellDTO well in wells)
                {
                    if (well.Name.ToLower().Equals(wellToMap.SNWellName.ToLower()))
                    {
                        wellToMap.Well = well;
                        wellToMap.WellId = well.Id;
                    }
                }
            }
            var allModels = SurfaceNetworkService.GetSNModelsByAssetIds(new long[] { assetId });
            SNModelDTO model = allModels?.FirstOrDefault(m => m.Name.Equals("Tutorial Example 3"));
            _snModelsToRemove.Add(model);
            Assert.IsNotNull(model);
            SurfaceNetworkService.UpdateSNWellMappings(wellmappings);
            Trace.WriteLine("Well Mapping is updated for Tutorial3");

            //Create SN scenario
            SNScenarioDTO addScenarioObj = new SNScenarioDTO()
            {
                Name = "TutorialExample3SNScenario",
                Description = "TutorialExample3 Description",
                SNModelId = model.Id,
                ColorCode = "#FF0000",
                AllowableScenario = true,
                Implementable = false,
                ModelMatch = false,
                OptimumScenario = false,
                IsForecastScenario = true
            };
            SurfaceNetworkService.AddSNScenario(addScenarioObj);
            Trace.WriteLine("Network Scenario created for Tutorial3");
            var allScenarios = SurfaceNetworkService.GetSNScenariosBySNModel(model.Id.ToString());
            SNScenarioDTO scenario = allScenarios?.FirstOrDefault(sn => sn.Name.Equals("TutorialExample3SNScenario"));
            Assert.IsNotNull(scenario);
            _snScenariosToRemove.Add(scenario);

            SurfaceNetworkService.InsertSNScenarioScheduleManualRun(scenario.Id.ToString());
            Trace.WriteLine("Network Scenario scheduled for Tutorial3");

            bool IsPFFeatureLicensed = ProductionForecastService.IsPFFeatureLicensed();
            Assert.IsTrue(IsPFFeatureLicensed, "PF Feature is not licensed.");
            Trace.WriteLine("PF Feature license checking successful.");

            return scenario;
        }

        /// <summary>
        /// Add Daily Average data for ADNOC wells for VRR Calculation
        /// </summary>
        /// <param name="wellId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="successfulWellTestId">Welltest Id</param>
        /// <param name="lastSuccessfulWelltestDate">Last successful Welltest date</param>
        /// <param name="runTime"></param>
        /// <param name="fieldFactor"></param>
        /// <param name="oilInferred"></param>
        /// <param name="waterInferred"></param>
        /// <param name="gasInferred"></param>
        public void AddDailyAvergeDataForADNOCWells(long wellId, DateTime start, DateTime end, long successfulWellTestId, DateTime lastSuccessfulWelltestDate, double runTime, double? fieldFactor, double? oilInferred, double? waterInferred, double? gasInferred)
        {
            WellDailyAverageValueDTO dailyAverageDTO = new WellDailyAverageValueDTO();
            if (fieldFactor != null)
            {
                // Production wells
                dailyAverageDTO = new WellDailyAverageValueDTO
                {
                    StartDateTime = start,
                    EndDateTime = end,
                    WellTestId = successfulWellTestId,
                    TestDate = lastSuccessfulWelltestDate,
                    OilRateAllocated = oilInferred * (runTime / 24) * fieldFactor,
                    OilRateInferred = oilInferred,
                    WaterRateAllocated = waterInferred * (runTime / 24) * fieldFactor,
                    WaterRateInferred = waterInferred,
                    GasRateAllocated = gasInferred * (runTime / 24) * fieldFactor,
                    GasRateInferred = gasInferred,
                    Status = WellDailyAverageDataStatus.Calculated,
                    Duration = 24,
                    GasJectionDepth = 1000,
                    ChokeDiameter = 64.0,
                    RunTime = runTime,
                    THP = 492,
                    THT = 213,
                    GasInjectionRate = 5280,
                    WellId = wellId,
                    DHPG = 300,
                    PDP = 2000,
                    PIP = 100,
                    MotorAmps = 34,
                    FLP = 678,
                };
            }
            else
            {
                // Injection wells
                dailyAverageDTO = new WellDailyAverageValueDTO
                {
                    StartDateTime = start,
                    EndDateTime = end,
                    WellTestId = successfulWellTestId,
                    TestDate = lastSuccessfulWelltestDate,
                    OilRateAllocated = oilInferred * (runTime / 24),
                    OilRateInferred = oilInferred,
                    WaterRateAllocated = waterInferred * (runTime / 24),
                    WaterRateInferred = waterInferred,
                    GasRateAllocated = gasInferred * (runTime / 24),
                    GasRateInferred = gasInferred,
                    Status = WellDailyAverageDataStatus.Calculated,
                    Duration = 24,
                    GasJectionDepth = 1000,
                    ChokeDiameter = 64.0,
                    RunTime = runTime,
                    THP = 492,
                    THT = 213,
                    GasInjectionRate = 5280,
                    WellId = wellId,
                    DHPG = 300,
                    PDP = 2000,
                    PIP = 100,
                    MotorAmps = 34,
                    FLP = 678,
                };
            }
            SurveillanceService.AddUpdateWellDailyAverageData(dailyAverageDTO);
        }

        /// <summary>
        /// Add 6 ADNOC wells (4 Production Wells and 2 Injection wells).
        /// </summary>
        /// <param name="assetId"></param>
        public void AddADNOCWells(long assetId)
        {
            string[] modelnames = new string[] { "GP1.wflx", "GP2.wflx", "GP3.wflx", "GP4.wflx", "GI1.wflx", "GI2.wflx" };
            List<string> modelfilepath = new List<string>();
            foreach (string mdfile in modelnames)
            {
                modelfilepath.Add(GetFileFromAssemblyStream(mdfile));
            }
            string[] filenames = modelfilepath.ToArray();
            foreach (string fname in modelnames)
            {
                //Well model files are copied to TestDocuments folder : 
                WellDTO well1 = null;
                string wellname = fname.Trim();
                wellname = wellname.Replace("\\", "");
                if (wellname.Contains(".wflx") && wellname.Contains("GP"))
                {
                    Trace.WriteLine("Well name Going to create :  NF Condensate  " + wellname);
                    well1 = SetNFCondensate(new WellDTO()
                    {
                        Name = wellname.Replace(".wflx", ""),
                        FacilityId = null,
                        DataConnection = null,
                        CommissionDate = DateTime.Today.AddYears(-4),
                        AssemblyAPI = wellname.Replace(".wflx", ""),
                        SubAssemblyAPI = wellname.Replace(".wflx", ""),
                        IntervalAPI = wellname.Replace(".wflx", ""),
                        WellDepthDatumId = 1,
                        WellType = WellTypeId.NF
                    });
                }
                else if (wellname.Contains(".wflx") && (wellname.Contains("GI")))
                {
                    Trace.WriteLine("Well name Going to create : GI  " + wellname);
                    well1 = SetDefaultFluidType(new WellDTO()
                    {
                        Name = wellname.Replace(".wflx", ""),
                        FacilityId = null,
                        DataConnection = null,
                        CommissionDate = DateTime.Today.AddYears(-4),
                        AssemblyAPI = wellname.Replace(".wflx", ""),
                        SubAssemblyAPI = wellname.Replace(".wflx", ""),
                        IntervalAPI = wellname.Replace(".wflx", ""),
                        WellDepthDatumId = 1,
                        WellType = WellTypeId.GInj
                    });
                }
                else
                {
                    continue;
                }
                well1.WellStatusId = WellConfigurationService.GetReferenceTableItems("r_WellStatus").ElementAt(1).Id;
                well1.AssetId = assetId;
                well1.SurfaceLatitude = 19.076090m;
                well1.SurfaceLongitude = 72.877426m;
                well1.Lease = "Lease Name1";
                well1.Field = "Field Name1";
                well1.Engineer = "Engineer Name1";
                well1.GeographicRegion = "Geographic Region1";
                well1.Foreman = "Foreman Name1";
                well1.GaugerBeat = "Gauger Beat1";
                WellConfigDTO wellConfigDTO1 = new WellConfigDTO
                {
                    Well = well1,
                    ModelConfig = ReturnBlankModel()
                };
                WellConfigDTO addedWellConfig1 = WellConfigurationService.AddWellConfig(wellConfigDTO1);
                Trace.WriteLine(wellname + " Added Successfully");
                AddModelFile(addedWellConfig1.Well.CommissionDate.Value.AddDays(1), addedWellConfig1.Well.WellType, addedWellConfig1.Well.Id, CalibrationMethodId.ReservoirPressure, wellname);
                Trace.WriteLine("Model File added in Well: " + wellname);
                addedWellConfig1 = WellConfigurationService.GetWellConfig(addedWellConfig1.Well.Id.ToString());
                _wellsToRemove.Add(addedWellConfig1.Well);
            }
        }

        //This method will get SWA WellBound records for specific Asset Settings  
        public SWAWellBasedDTO getWellBoundRecordsForSnapshot(long SnapshotID, long AssetID, WellTypeCategory WellCategory, WellAllowablePhase Phase)
        {
            SWAWellBasedDTO getWellBasedDTO = new SWAWellBasedDTO();
            getWellBasedDTO.SnapshotId = SnapshotID;
            getWellBasedDTO.AssetId = AssetID;
            getWellBasedDTO.WellCategory = WellCategory;
            getWellBasedDTO.FluidPhase = Phase;

            return getWellBasedDTO;
        }

        //This method will get SWA AssetBound records for specific Asset Settings  
        public SWAAssetBasedRequestDTO getAssetBoundRecordsForSnapshot(long SnapshotID, long AssetID, WellTypeCategory WellCategory, WellAllowablePhase Phase)
        {
            SWAAssetBasedRequestDTO getAssetBasedDTO = new SWAAssetBasedRequestDTO();
            getAssetBasedDTO.SnapshotId = SnapshotID;
            getAssetBasedDTO.AssetId = AssetID;
            getAssetBasedDTO.WellCategory = WellCategory;
            getAssetBasedDTO.FluidPhase = Phase;

            return getAssetBasedDTO;
        }
    }
}

