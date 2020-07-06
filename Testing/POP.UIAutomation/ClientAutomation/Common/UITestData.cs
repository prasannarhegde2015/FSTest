using System;
using System.Collections.Generic;
using System.Diagnostics;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;

namespace ClientAutomation
{
    public class UITestData : ForeSiteAutoBase
    {
        public WellDTO WellData(WellTypeId wType)
        {
            WellDTO well = null;
            switch (wType)
            {
                case WellTypeId.RRL:
                    well = new WellDTO() { Name = "RRL", FacilityId = CygNetFacility, DataConnection = new DataConnectionDTO { ProductionDomain = Domain, Site = Site, Service = Service }, AssemblyAPI = "TestAssemblyAPI_RRL", SubAssemblyAPI = "TestSubAssemblyAPI_RRL", IntervalAPI = "TestIntervalAPI_RRL", WellType = wType };
                    break;

                case WellTypeId.ESP:
                    well = new WellDTO() { Name = "ESP", FacilityId = CygNetFacility, DataConnection = new DataConnectionDTO { ProductionDomain = Domain, Site = Site, Service = Service }, AssemblyAPI = "TestAssemblyAPI_ESP", SubAssemblyAPI = "Esp_ProductionTestData.wflx", IntervalAPI = "TestIntervalAPI_ESP", WellType = wType };
                    break;

                case WellTypeId.GLift:
                    well = new WellDTO() { Name = "Gas Lift", FacilityId = CygNetFacility, DataConnection = new DataConnectionDTO { ProductionDomain = Domain, Site = Site, Service = Service }, AssemblyAPI = "TestAssemblyAPI_GLift", SubAssemblyAPI = "GasLift-LFactor1.wflx", IntervalAPI = "TestIntervalAPI_GLift", WellType = wType };
                    break;

                case WellTypeId.NF:
                    well = new WellDTO() { Name = "Naturally Flowing", FacilityId = CygNetFacility, DataConnection = new DataConnectionDTO { ProductionDomain = Domain, Site = Site, Service = Service }, AssemblyAPI = "TestAssemblyAPI_NF", SubAssemblyAPI = "WellfloNFWExample1.wflx", IntervalAPI = "TestIntervalAPI_NF", WellType = wType };
                    break;

                case WellTypeId.WInj:
                    well = new WellDTO() { Name = "Water Injection", FacilityId = CygNetFacility, DataConnection = new DataConnectionDTO { ProductionDomain = Domain, Site = Site, Service = Service }, AssemblyAPI = "TestAssemblyAPI_WInj", SubAssemblyAPI = "WellfloWaterInjectionExample1.wflx", IntervalAPI = "TestIntervalAPI_WInj", WellType = wType };
                    break;

                case WellTypeId.GInj:
                    well = new WellDTO() { Name = "Gas Injection", FacilityId = CygNetFacility, DataConnection = new DataConnectionDTO { ProductionDomain = Domain, Site = Site, Service = Service }, AssemblyAPI = "TestAssemblyAPI_GInj", SubAssemblyAPI = "WellfloGasInjectionExample1.wflx", IntervalAPI = "TestIntervalAPI_GInj", WellType = wType };
                    break;

                case WellTypeId.PLift:
                    well = new WellDTO() { Name = "Plunger Lift", FacilityId = CygNetFacility, DataConnection = new DataConnectionDTO { ProductionDomain = Domain, Site = Site, Service = Service }, AssemblyAPI = "TestAssemblyAPI_PLift", SubAssemblyAPI = "PL-631.wflx", IntervalAPI = "TestIntervalAPI_PLift", WellType = wType };
                    break;

                default:
                    Trace.WriteLine("Well Type Not expected");
                    break;
            }

            return well;
        }

        public WellConfigDTO WellConfigData(WellTypeId wType)
        {
            WellDTO well = WellData(wType);
            well.CommissionDate = new DateTime(2016, 4, 18);
            well.Engineer = "Engineer";
            well.Field = "Field";
            well.Foreman = "Foreman";
            well.GaugerBeat = "GaugerBeat";
            well.GeographicRegion = "GeographicRegion";
            well.Lease = "Lease";
            well.SurfaceLatitude = (decimal)32.444403;
            well.SurfaceLongitude = (decimal)-102.422178;

            ModelConfigDTO Model = new ModelConfigDTO();

            #region ModelData

            //Surface
            SurfaceConfigDTO SampleSurfaceConfig = new SurfaceConfigDTO();
            PumpingUnitManufacturerDTO Manufac = new PumpingUnitManufacturerDTO() { Name = "Lufkin" };
            PumpingUnitTypeDTO pumpingUnitType = new PumpingUnitTypeDTO() { AbbreviatedName = "C" };
            PumpingUnitDTO pumpingUnit = new PumpingUnitDTO() { Manufacturer = Manufac.Name, Type = pumpingUnitType.AbbreviatedName, Description = "C-57-109-48 L LUFKIN C57-109-48 (4246B)" };
            SampleSurfaceConfig.PumpingUnit = pumpingUnit;
            SampleSurfaceConfig.PumpingUnitType = pumpingUnitType;
            SampleSurfaceConfig.ClockwiseRotation = PumpingUnitRotationDirection.Clockwise;
            SampleSurfaceConfig.MotorAmpsDown = 120;
            SampleSurfaceConfig.MotorAmpsUp = 144;
            SampleSurfaceConfig.WristPinPosition = 3;
            SampleSurfaceConfig.ActualStrokeLength = 25;
            SampleSurfaceConfig.MotorType = new RRLMotorTypeDTO() { Name = "Nema B Electric" };
            SampleSurfaceConfig.MotorSize = new RRLMotorSizeDTO(50);
            SampleSurfaceConfig.SlipTorque = new RRLMotorSlipDTO() { Rating = 2 };
            Model.Surface = SampleSurfaceConfig;

            //Weights
            CrankWeightsConfigDTO SampleWeightsConfig = new CrankWeightsConfigDTO();
            SampleWeightsConfig.CrankId = "9411OC";
            SampleWeightsConfig.Crank_1_Primary = new RRLPrimaryWeightDTO { LagId = "OORO", LeadId = "6RO" };
            SampleWeightsConfig.Crank_2_Primary = new RRLPrimaryWeightDTO { LagId = "OORO", LeadId = "6RO" };
            SampleWeightsConfig.TorqCalcMethod = TorqueCalculationMethod.Mills;
            Model.Weights = SampleWeightsConfig;

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
            Model.Downhole = SampleDownholeConfig;

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
            Model.Rods = SampleRodsConfig;

            #endregion ModelData

            WellConfigDTO wellConfigDTO = new WellConfigDTO();
            wellConfigDTO.Well = well;
            wellConfigDTO.ModelConfig = Model;

            return wellConfigDTO;
        }

        public IntelligentAlarmDTO IntelligentAlarmData()
        {
            IntelligentAlarmDTO Idata = new IntelligentAlarmDTO();
            Idata.HighGearboxTorque = "88";
            Idata.HighRodStress = "92";
            Idata.HighStructureLoad = "64";
            Idata.LowPumpEfficiency = "55";
            List<string> arr = new List<string>() { "50", "60", "2.5", "2.5", "2.5", "2.5", "2.5", "2.5" };
            Idata.IntelligentAlarmStatus = arr;
            return Idata;
        }
    }
}