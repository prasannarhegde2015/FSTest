using System;
using System.Linq;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;

namespace POP.Performance
{
    public class NonRRLDataLoad : APIPerfTestBase
    {
        public ModelConfigDTO ReturnBlankModel()
        {
            //Empty Surface
            SurfaceConfigDTO SampleSurfaceConfig = new SurfaceConfigDTO();
            //Empty Weights
            CrankWeightsConfigDTO SampleWeightsConfig = new CrankWeightsConfigDTO();
            SampleWeightsConfig.Crank_1_Primary = new RRLPrimaryWeightDTO();
            SampleWeightsConfig.Crank_2_Primary = new RRLPrimaryWeightDTO();
            SampleWeightsConfig.Crank_1_Auxiliary = new RRLAuxiliaryWeightDTO();
            SampleWeightsConfig.Crank_2_Auxiliary = new RRLAuxiliaryWeightDTO();
            //Empty Downhole
            DownholeConfigDTO SampleDownholeConfig = new DownholeConfigDTO();
            //Empty Rods
            RodStringConfigDTO SampleRodsConfig = new RodStringConfigDTO();
            RodTaperConfigDTO[] RodTaperArray = Array.Empty<RodTaperConfigDTO>();
            SampleRodsConfig.RodTapers = RodTaperArray;
            //Model File
            ModelConfigDTO SampleModel = new ModelConfigDTO();
            SampleModel.Weights = SampleWeightsConfig;
            SampleModel.Rods = SampleRodsConfig;
            SampleModel.Downhole = SampleDownholeConfig;
            SampleModel.Surface = SampleSurfaceConfig;

            return SampleModel;
        }

        public WellTestDTO TestData(WellTypeId type, long wellId)
        {
            WellTestDTO wellTest = new WellTestDTO();
            switch (type)
            {
                case WellTypeId.ESP:
                    {
                        wellTest = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "AllocatableTest",
                            AverageTubingPressure = random.Next(200, 210),
                            AverageTubingTemperature = random.Next(60, 100),
                            //  PumpIntakePressure = random.Next(1500, 1900),
                            //   PumpDischargePressure = random.Next(1500, 1900),
                            //    GaugePressure = random.Next(1500, 1900),
                            Oil = random.Next(2400, 2410),
                            Gas = random.Next(1200, 1210),
                            Water = random.Next(3600, 3610),
                            ChokeSize = random.Next(50, 100),
                            FlowLinePressure = random.Next(150, 160),
                            SeparatorPressure = random.Next(100, 120),
                            Frequency = random.Next(70, 75),
                        };
                    }
                    break;

                case WellTypeId.NF:
                    {
                        wellTest = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "RepresentativeTest",
                            AverageTubingPressure = random.Next(165, 170),
                            AverageTubingTemperature = random.Next(80, 90),
                            //   GaugePressure = random.Next(1500, 1900),
                            Oil = random.Next(1770, 1780),
                            Gas = random.Next(880, 890),
                            Water = random.Next(590, 596),
                            ChokeSize = random.Next(45, 50),
                            FlowLinePressure = random.Next(140, 150),
                            SeparatorPressure = random.Next(100, 110),
                        };
                    }
                    break;

                case WellTypeId.GLift:
                    {
                        wellTest = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "RepresentativeTest",
                            AverageTubingPressure = random.Next(200, 200),
                            AverageTubingTemperature = random.Next(80, 80),
                            AverageCasingPressure = random.Next(1000, 1020),
                            GasInjectionRate = random.Next(1000, 1010),
                            FlowLinePressure = random.Next(150, 160),
                            SeparatorPressure = random.Next(100, 110),
                            GaugePressure = random.Next(1000, 1050),
                            Oil = random.Next(1048, 1057),
                            Gas = random.Next(520, 530),
                            Water = random.Next(1571, 1590),
                            ChokeSize = random.Next(50, 55),
                        };
                    }
                    break;

                case WellTypeId.WInj:
                    {
                        wellTest = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "AllocatableTest",
                            AverageTubingPressure = random.Next(500, 1500),
                            AverageTubingTemperature = random.Next(50, 100),
                            GaugePressure = random.Next(500, 1900),
                            Water = random.Next(500, 1900),
                            FlowLinePressure = random.Next(50, 100)
                        };
                    }
                    break;

                case WellTypeId.GInj:
                    {
                        wellTest = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "AllocatableTest",
                            AverageTubingPressure = random.Next(500, 1900),
                            AverageTubingTemperature = random.Next(50, 100),
                            GaugePressure = random.Next(500, 1900),
                            Gas = random.Next(500, 1900),
                            FlowLinePressure = random.Next(50, 100)
                        };
                    }
                    break;

                case WellTypeId.PLift:
                    {
                        wellTest = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "AllocatableTest",
                            TestDuration = random.Next(12, 24),
                            MaximumCasingPressure = random.Next(1500, 1900),
                            MaximumTubingPressure = random.Next(1000, 1300),
                            MinimumCasingPressure = random.Next(1000, 1400),
                            MinimumTubingPressure = random.Next(500, 900),
                            FlowLinePressure = random.Next(500, 1400),
                            BuildTime = random.Next(5100, 5500),
                            AfterFlowTime = random.Next(4500, 5000),
                            FallTime = random.Next(1400, 1500),
                            RiseTime = random.Next(1000, 1200),
                            CycleGasVolume = random.Next(500, 1900),
                            CycleWaterVolume = random.Next(500, 1900),
                            CycleOilVolume = 0
                        };
                    }
                    break;

                case WellTypeId.OT:
                    {
                        wellTest = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "AllocatableTest",
                            Oil = random.Next(1500, 1900),
                            Gas = random.Next(1500, 1900),
                            Water = random.Next(1500, 1900),
                            Comment = "TestData"
                        };
                    }
                    break;

                case WellTypeId.PCP:
                    {
                        wellTest = new WellTestDTO()
                        {
                            WellId = wellId,
                            SPTCodeDescription = "AllocatableTest",
                            AverageTubingPressure = random.Next(100, 105),
                            AverageTubingTemperature = random.Next(60, 100),
                            //    PumpIntakePressure = random.Next(1500, 1900),
                            //     PumpDischargePressure = random.Next(1500, 1900),
                            //    GaugePressure = random.Next(1500, 1900),
                            Oil = random.Next(210, 215),
                            Gas = random.Next(0, 0),
                            Water = random.Next(140, 145),
                            ChokeSize = random.Next(50, 100),
                            FlowLinePressure = random.Next(150, 160),
                            SeparatorPressure = random.Next(100, 110),
                            //    Frequency = random.Next(50, 100),
                            TestDuration = 24,
                            //    PolishedRodTorque = random.Next(500, 1500),
                            //    PumpTorque = random.Next(40, 200),
                            PumpSpeed = random.Next(225, 230),
                            Comment = "PCPWellTest_Comment_Check",
                            AverageCasingPressure = 0
                        };
                    }
                    break;
            }
            return wellTest;
        }

        public void AddWell(string BaseFacTag, int welStart, int welEnd, string Domain, string Site, string Service, string type)
        {
            try
            {
                Authenticate();
                ReferenceTableItemDTO[] wellDepthDatums = WellConfigurationService.GetReferenceTableItems("r_WellDepthDatum", "false");
                ReferenceTableItemDTO wellDepthDatum = wellDepthDatums.FirstOrDefault(t => t.ConstantId == "GROUND_LEVEL") ?? wellDepthDatums.FirstOrDefault();
                ReferenceTableItemDTO[] wellStatus = WellConfigurationService.GetReferenceTableItems("r_WellStatus", "false");
                ReferenceTableItemDTO wellStatusId = wellStatus.FirstOrDefault(t => t.ConstantId == "ACTIVE") ?? wellDepthDatums.FirstOrDefault();
                WellDTO well = new WellDTO();
                switch (type)
                {
                    case "NFW":
                        well.WellType = WellTypeId.NF;
                        well.FluidType = WellFluidType.BlackOil;
                        break;

                    case "ESP":
                        well.WellType = WellTypeId.ESP;
                        break;

                    case "GL":
                        {
                            well.WellType = WellTypeId.GLift;
                            well.FluidType = WellFluidType.BlackOil;
                        }
                        break;

                    case "WInj":
                        well.WellType = WellTypeId.WInj;
                        break;

                    case "GInj":
                        well.WellType = WellTypeId.GInj;
                        break;

                    case "PL":
                        {
                            well.WellType = WellTypeId.PLift;
                            well.FluidType = WellFluidType.DryGas;
                        }
                        break;

                    case "WGInj":
                        well.WellType = WellTypeId.WGInj;
                        break;

                    case "OT":
                        well.WellType = WellTypeId.OT;
                        break;

                    case "PCP":
                        well.WellType = WellTypeId.PCP;
                        well.FluidType = WellFluidType.BlackOil;
                        well.FluidPhase = WellFluidPhase.SinglePhase;
                        break;

                    default:
                        WriteLogFile(string.Format("Invalid Well type"));
                        break;
                }
                for (int i = welStart; i <= welEnd; i++)
                {

                    if (type.ToString() != "OT")
                    {
                        well.DataConnection = new DataConnectionDTO { ProductionDomain = Domain, Site = Site, Service = Service };
                    }
                    well.Name = BaseFacTag + "-" + i.ToString(FacilityPadding);
                    well.FacilityId = BaseFacTag + "_" + i.ToString(FacilityPadding);
                    well.CommissionDate = DateTime.Today.AddYears(-Convert.ToInt32(YearsOfData));
                    well.AssemblyAPI = BaseFacTag + "-" + i.ToString(FacilityPadding);
                    well.SubAssemblyAPI = BaseFacTag + "-" + i.ToString(FacilityPadding);
                    well.IntervalAPI = BaseFacTag + "-" + i.ToString(FacilityPadding);
                    well.DepthCorrectionFactor = 2;
                    well.WellDepthDatumElevation = 1;
                    well.WellGroundElevation = 1;
                    well.WellDepthDatumId = wellDepthDatum?.Id;
                    well.GasAllocationGroup = null;
                    well.OilAllocationGroup = null;
                    well.WaterAllocationGroup = null;
                    well.WellStatusId = wellStatusId?.Id;
                    well.SurfaceLatitude = 19m;
                    well.SurfaceLongitude = 19m + i + 1m;
                    WellConfigDTO wellConfig = new WellConfigDTO();
                    wellConfig.Well = well;
                    wellConfig.ModelConfig = ReturnBlankModel();
                    //Well
                    WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfig);
                    //Model file
                    AddModelFile(addedWellConfig.Well.CommissionDate.Value, well.WellType, addedWellConfig.Well.Id);
                    Console.WriteLine("Well added : " + addedWellConfig.Well.Name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void AddModelFile(DateTime date, WellTypeId type, long wellId)
        {
            string Path = "POP.Performance.ModelFiles.";
            byte[] fileAsByteArray;
            ModelFileValidationDataDTO ModelFileValidationData;
            ModelFileOptionDTO options = new ModelFileOptionDTO();
            ModelFileBase64DTO modelFile = new ModelFileBase64DTO() { };
            modelFile.ApplicableDate = date.AddDays(1).ToUniversalTime();
            modelFile.WellId = wellId;
            switch (type)
            {
                case WellTypeId.NF:
                    {
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
                            WriteLogFile(string.Format("Failed to validate NF model file"));
                        //Add Well Test
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        for (int i = 1; i <= Convert.ToInt32(WellTestCount); i++)
                        {
                            WellTestDTO testDataDTO = TestData(WellTypeId.NF, wellId);
                            testDataDTO.SampleDate = date.AddDays(i * 10).ToUniversalTime();
                            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                        }
                        for (int i = 1; i <= Convert.ToInt32(ModelFilesPerYear) - 1; i++)
                        {
                            WellTestAndUnitsDTO test = WellTestDataService.GetLatestValidWellTestByWellId(wellId.ToString());
                            modelFile.ApplicableDate = test.Value.SampleDate.AddDays(i * 10).ToUniversalTime();
                            ModelFileService.AddWellModelFile(modelFile);
                        }
                        break;
                    }
                case WellTypeId.GLift:
                    {
                        fileAsByteArray = GetByteArray(Path, "GasLift - L Factor 1.wflx");
                        options.CalibrationMethod = CalibrationMethodId.ReservoirPressure;
                        options.Comment = "Gas Lift";
                        options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        modelFile.Options = options;
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                        else
                            WriteLogFile(string.Format("Failed to validate Gas Lift model file"));
                        //Add Well Test
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        for (int i = 1; i <= Convert.ToInt32(WellTestCount); i++)
                        {
                            WellTestDTO testDataDTO = TestData(WellTypeId.GLift, wellId);
                            testDataDTO.SampleDate = date.AddDays(i * 10).ToUniversalTime();
                            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                        }
                        for (int i = 1; i <= Convert.ToInt32(ModelFilesPerYear) - 1; i++)
                        {
                            WellTestAndUnitsDTO test = WellTestDataService.GetLatestValidWellTestByWellId(wellId.ToString());
                            modelFile.ApplicableDate = test.Value.SampleDate.AddDays(i * 10).ToUniversalTime();
                            ModelFileService.AddWellModelFile(modelFile);
                        }
                        break;
                    }
                case WellTypeId.ESP:
                    {
                        fileAsByteArray = GetByteArray(Path, "ESP_WELL01.wflx");
                        options.CalibrationMethod = CalibrationMethodId.ReservoirPressure;
                        options.Comment = "ESP";
                        options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        modelFile.Options = options;
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                        else
                            WriteLogFile(string.Format("Failed to validate ESP model file"));
                        //Add Well Test
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        for (int i = 1; i <= Convert.ToInt32(WellTestCount); i++)
                        {
                            WellTestDTO testDataDTO = TestData(WellTypeId.ESP, wellId);
                            testDataDTO.SampleDate = date.AddDays(i * 10).ToUniversalTime();
                            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                        }
                        for (int i = 1; i <= Convert.ToInt32(ModelFilesPerYear) - 1; i++)
                        {
                            WellTestAndUnitsDTO test = WellTestDataService.GetLatestValidWellTestByWellId(wellId.ToString());
                            modelFile.ApplicableDate = test.Value.SampleDate.AddDays(i * 10).ToUniversalTime();
                            ModelFileService.AddWellModelFile(modelFile);
                        }
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
                        else
                            WriteLogFile(string.Format("Failed to validate Water Injection model file"));
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        for (int i = 1; i <= Convert.ToInt32(WellTestCount); i++)
                        {
                            WellTestDTO testDataDTO = TestData(WellTypeId.WInj, wellId);
                            testDataDTO.SampleDate = date.AddDays(i * 15).ToUniversalTime();
                            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                        }
                        for (int i = 1; i <= Convert.ToInt32(ModelFilesPerYear) - 1; i++)
                        {
                            WellTestAndUnitsDTO test = WellTestDataService.GetLatestValidWellTestByWellId(wellId.ToString());
                            modelFile.ApplicableDate = test.Value.SampleDate.AddDays(i * 10).ToUniversalTime();
                            ModelFileService.AddWellModelFile(modelFile);
                        }
                    }
                    break;
                case WellTypeId.GInj:
                    {
                        fileAsByteArray = GetByteArray(Path, "WellfloGasInjectionExample1.wflx");
                        options.CalibrationMethod = CalibrationMethodId.LFactor;
                        options.Comment = "GInj";
                        options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        modelFile.Options = options;
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                        else
                            WriteLogFile(string.Format("Failed to validate GI model file"));
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        for (int i = 1; i <= Convert.ToInt32(WellTestCount); i++)
                        {
                            WellTestDTO testDataDTO = TestData(WellTypeId.GInj, wellId);
                            testDataDTO.SampleDate = date.AddDays(i * 15).ToUniversalTime();
                            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                        }
                        for (int i = 1; i <= Convert.ToInt32(ModelFilesPerYear) - 1; i++)
                        {
                            WellTestAndUnitsDTO test = WellTestDataService.GetLatestValidWellTestByWellId(wellId.ToString());
                            modelFile.ApplicableDate = test.Value.SampleDate.AddDays(i * 10).ToUniversalTime();
                            ModelFileService.AddWellModelFile(modelFile);
                        }
                    }
                    break;
                case WellTypeId.PLift:
                    {
                        fileAsByteArray = GetByteArray(Path, "PL-631.wflx");
                        options.Comment = "PLift";
                        options.OptionalUpdate = new long[] { };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        modelFile.Options = options;
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                        else
                            WriteLogFile(string.Format("Failed to validate PL model file"));
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        for (int i = 1; i <= Convert.ToInt32(WellTestCount); i++)
                        {
                            WellTestDTO testDataDTO = TestData(WellTypeId.PLift, wellId);
                            testDataDTO.SampleDate = date.AddDays(i * 15).ToUniversalTime();
                            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                        }
                        for (int i = 1; i <= Convert.ToInt32(ModelFilesPerYear) - 1; i++)
                        {
                            WellTestAndUnitsDTO test = WellTestDataService.GetLatestValidWellTestByWellId(wellId.ToString());
                            modelFile.ApplicableDate = test.Value.SampleDate.AddDays(i * 10).ToUniversalTime();
                            ModelFileService.AddWellModelFile(modelFile);
                        }
                    }
                    break;
                case WellTypeId.OT:
                    {
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        for (int i = 1; i <= Convert.ToInt32(WellTestCount); i++)
                        {
                            WellTestDTO testDataDTO = TestData(WellTypeId.OT, wellId);
                            testDataDTO.SampleDate = date.AddDays(i * 15).ToUniversalTime();
                            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                        }
                    }
                    break;
                case WellTypeId.PCP:
                    {
                        fileAsByteArray = GetByteArray(Path, "PCP-SinglePhase.wflx");
                        options.CalibrationMethod = CalibrationMethodId.ReservoirPressure;
                        options.Comment = "PCP";
                        options.OptionalUpdate = new long[] { ((long)OptionalUpdates.UpdateWCT_WGR), ((long)OptionalUpdates.UpdateGOR_CGR) };
                        modelFile.Base64Contents = Convert.ToBase64String(fileAsByteArray);
                        modelFile.Options = options;
                        ModelFileValidationData = ModelFileService.GetModelFileValidation(modelFile);
                        if (ModelFileValidationData != null)
                            ModelFileService.AddWellModelFile(modelFile);
                        else
                            WriteLogFile(string.Format("Failed to validate PCP model file"));
                        //Add Well Test
                        WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId.ToString()).Units;
                        for (int i = 1; i <= Convert.ToInt32(WellTestCount); i++)
                        {
                            WellTestDTO testDataDTO = TestData(WellTypeId.PCP, wellId);
                            testDataDTO.SampleDate = date.AddDays(i * 10).ToUniversalTime();
                            WellTestDataService.SaveWellTest(new WellTestAndUnitsDTO(units, testDataDTO));
                        }
                        for (int i = 1; i <= Convert.ToInt32(ModelFilesPerYear) - 1; i++)
                        {
                            WellTestAndUnitsDTO test = WellTestDataService.GetLatestValidWellTestByWellId(wellId.ToString());
                            modelFile.ApplicableDate = test.Value.SampleDate.AddDays(i * 10).ToUniversalTime();
                            ModelFileService.AddWellModelFile(modelFile);
                        }
                        break;
                    }

                default:
                    WriteLogFile(string.Format("Invalid Well type"));
                    break;
            }
        }
    }
}