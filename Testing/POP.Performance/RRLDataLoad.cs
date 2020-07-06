using System;
using System.Collections.Generic;
using System.Linq;
using Weatherford.DynaCardLibrary.API.Enums;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using Weatherford.POP.CygNetWrapper;
using System.Xml.Linq;
using CygNet.COMAPI.EIE;
using Weatherford.DynaCardLibrary.API.Data;
using Weatherford.DynaCardLibrary.API.Operations;

namespace POP.Performance
{
    public class RRLDataLoad : APIPerfTestBase
    {
        PumpingUnitManufacturerDTO[] manufacturers;
        PumpingUnitManufacturerDTO pumpingUnitManufacturer;
        PumpingUnitTypeDTO[] pumpingUnitTypes;
        PumpingUnitTypeDTO pumpingUnitType;
        PumpingUnitDTO[] pumpingUnits;
        PumpingUnitDTO pumpingUnitBase;
        PumpingUnitDTO pumpingUnit;
        RRLMotorTypeDTO[] motorType;
        RRLMotorSizeDTO[] motorSize;
        RRLMotorSlipDTO[] motorSlip;
        POPRRLCranksDTO[] cranks;
        POPRRLCranksDTO crank;
        POPRRLCranksWeightsDTO cranksWeights;

        public void LoadRRLCatalogData()
        {
            manufacturers = CatalogService.GetAllPumpingUnitManufacturers();
            pumpingUnitManufacturer = manufacturers.FirstOrDefault(pumt => pumt.Name.Equals("Lufkin"));
            pumpingUnitTypes = CatalogService.GetPumpingUnitTypesByManufacturer(pumpingUnitManufacturer.Name);
            pumpingUnitType = pumpingUnitTypes.FirstOrDefault(t => t.AbbreviatedName.Equals("C"));
            pumpingUnits = CatalogService.GetPumpingUnitsByManufacturerAndType(pumpingUnitManufacturer.Name, pumpingUnitType.AbbreviatedName);
            pumpingUnitBase = pumpingUnits.FirstOrDefault(pu => pu.Description.Equals("C-912-365-168 L LUFKIN C912-365-168 (94110C)"));
            pumpingUnit = CatalogService.GetPumpingUnitByManufacturerTypeAndDescription(pumpingUnitManufacturer.Name, pumpingUnitType.AbbreviatedName, pumpingUnitBase.Description);
            motorType = CatalogService.GetAllMotorTypes();
            motorSize = CatalogService.GetAllMotorSizes();
            motorSlip = CatalogService.GetAllMotorSlips();
            cranks = CatalogService.GetCranksByPumpingUnitPK(pumpingUnitManufacturer.Name, pumpingUnitType.AbbreviatedName, pumpingUnit.Description);
            crank = cranks.FirstOrDefault(c => c.CrankId.Equals("94110C"));
            cranksWeights = CatalogService.GetCrankWeightsByCrankId(crank.CrankId);
        }

        public ModelConfigDTO ReturnFullyPopulatedModel()
        {
            // Surface.
            SurfaceConfigDTO surfaceConfig = new SurfaceConfigDTO();
            surfaceConfig.PumpingUnit = pumpingUnit;
            surfaceConfig.PumpingUnitType = pumpingUnitType;
            surfaceConfig.ClockwiseRotation = PumpingUnitRotationDirection.Counterclockwise;
            surfaceConfig.MotorAmpsDown = 120;
            surfaceConfig.MotorAmpsUp = 144;
            surfaceConfig.MotorType = motorType.FirstOrDefault(mt => mt.Name == "Nema D");
            surfaceConfig.MotorSize = motorSize.FirstOrDefault(ms => ms.SizeInHP == 50);
            surfaceConfig.SlipTorque = motorSlip.FirstOrDefault(ms => ms.Rating == 1);
            surfaceConfig.WristPinPosition = 2;
            surfaceConfig.ActualStrokeLength = pumpingUnit.StrokeLengthsAtPins[2];

            // Weights.
            CrankWeightsConfigDTO weightsConfig = new CrankWeightsConfigDTO();
            weightsConfig.CrankId = crank.CrankId;
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
                LeadDistance = 0.0
            };
            weightsConfig.Crank_2_Auxiliary = new RRLAuxiliaryWeightDTO { LagId = cranksWeights.AuxiliaryIdentifier[auxNoneIndex], LeadId = cranksWeights.AuxiliaryIdentifier[auxNoneIndex] };
            weightsConfig.Crank_2_Primary = new RRLPrimaryWeightDTO
            {
                LagId = cranksWeights.PrimaryIdentifier[primaryNoneIndex],
                LeadId = cranksWeights.PrimaryIdentifier[primaryWeightIndex],
                LeadMDistance = cranksWeights.DistanceM[primaryWeightIndex],
                LeadWeight = (int)cranksWeights.Primary[primaryWeightIndex],
                LeadDistance = 0.0
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
            downholeConfig.TubingID = 2.75;
            downholeConfig.TubingOD = 2.875;
            downholeConfig.TopPerforation = 4558.0;
            downholeConfig.BottomPerforation = 5220.0;
            downholeConfig.CasingWeight = 20.0;
            downholeConfig.CasingOD = 4.25;

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

        public void WellData_RRL(string BaseFacTag, int welStart, int welEnd, string Domain, string Site, string Service)
        {
            Authenticate();
            LoadRRLCatalogData();
            for (int i = welStart; i <= welEnd; i++)
            {
                WellDTO well = new WellDTO()
                {
                    Name = BaseFacTag + "-" + i.ToString(FacilityPadding),
                    FacilityId = BaseFacTag + "_" + i.ToString(FacilityPadding),
                    DataConnection = new DataConnectionDTO { ProductionDomain = Domain, Site = Site, Service = Service },
                    CommissionDate = DateTime.Today.AddYears(-Convert.ToInt32(YearsOfData)),
                    AssemblyAPI = BaseFacTag + "-" + i.ToString(FacilityPadding),
                    SubAssemblyAPI = BaseFacTag + "-" + i.ToString(FacilityPadding),
                    IntervalAPI = BaseFacTag + "-" + i.ToString(FacilityPadding),
                    WellType = WellTypeId.RRL
                };
                WellConfigDTO wellConfigDTO = new WellConfigDTO();
                wellConfigDTO.Well = well;
                wellConfigDTO.ModelConfig = ReturnFullyPopulatedModel();
                WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
                WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(addedWellConfig.Well.Id.ToString()).Units;
                //Add Well Tests
                for (int j = 0; j < Convert.ToInt32(WellTestCount); j++)
                {
                    //add new wellTestData
                    WellTestDTO testDataDTO = new WellTestDTO
                    {
                        WellId = addedWellConfig.Well.Id,
                        AverageCasingPressure = random.Next(1500, 1900),
                        AverageFluidAbovePump = random.Next(5000, 7000),
                        AverageTubingPressure = random.Next(1500, 1900),
                        AverageTubingTemperature = random.Next(60, 100),
                        Gas = random.Next(1500, 1900),
                        GasGravity = 1,
                        Oil = random.Next(1500, 1900),
                        OilGravity = random.Next(10, 150),
                        PumpEfficiency = 0,
                        PumpIntakePressure = 100,
                        PumpingHours = 10,
                        SPTCodeDescription = WellTestQuality.RepresentativeTest.ToString(),
                        StrokePerMinute = 0,
                        TestDuration = random.Next(1, 24),
                        Water = random.Next(1500, 1900),
                        WaterGravity = 1.010M,
                    };
                    testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(j * 15));
                    WellTestDataService.AddWellTestData(new WellTestAndUnitsDTO(units, testDataDTO));
                }
                Console.WriteLine("Well Added : " + addedWellConfig.Well.Name);
            }
        }

        public void ScanAllWells_RRL()
        {
            Authenticate();
            //Scan Full Card and Run Analysis and add Intelligent Alarms
            IEnumerable<WellDTO> allWells = WellService.GetAllWells().Where(x => x.WellType == WellTypeId.RRL);
            if (allWells.Count() > 0)
            {
                foreach (WellDTO well in allWells)
                {
                    try
                    {
                        DynaCardEntryDTO FullDynaCard = DynacardService.ScanDynacard(well.Id.ToString(), ((int)CardType.Full).ToString());
                        if (FullDynaCard.ErrorMessage == "Success")
                        {
                            DynaCardEntryDTO surfaceCard = DynacardService.AnalyzeSelectedSurfaceCardExclusive(well.Id.ToString(), FullDynaCard.TimestampInTicks, ((int)CardType.Full).ToString(), "true");
                            if (surfaceCard.ErrorMessage != "Success")
                                WriteLogFile(string.Format("Unable to Analyze the collected card for : " + well.Name));
                        }
                        else
                        {
                            WriteLogFile(string.Format("Unable to Analyze the collected card for : " + well.Name));
                        }
                    }
                    catch
                    {
                        WriteLogFile(string.Format("Unable to Analyze the collected card for : " + well.Name));
                    }
                }
            }
            else
                WriteLogFile(string.Format("No RRL Wells inside the Database"));
        }

        public void AddHistoricalAnalysisReports()
        {
            Authenticate();
            var allWells = WellService.GetAllWells().Where(x => x.WellType == WellTypeId.RRL);
            if (allWells.Count() > 0)
            {
                foreach (WellDTO well in allWells)
                {
                    string ccType = ((int)CardType.Current).ToString();
                    string fcType = ((int)CardType.Full).ToString();
                    string pcType = ((int)CardType.Pumpoff).ToString();
                    string acType = ((int)CardType.Alarm).ToString();
                    string failureType = ((int)CardType.Failure).ToString();
                    string cards = ccType + "," + fcType + "," + pcType + "," + acType + "," + failureType;
                    DynacardEntryCollectionAndUnitsDTO cardEntries = DynacardService.GetCardEntriesByDateRange(well.Id.ToString(), cards, DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddYears(-1)), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime()));
                    if (cardEntries.Values.Count() > 0)
                    {
                        foreach (DynaCardEntryDTO card in cardEntries.Values)
                        {
                            if (card.ErrorMessage == "Success")
                                DynacardService.AnalyzeSelectedSurfaceCardExclusive(well.Id.ToString(), card.TimestampInTicks, ((int)card.CardType).ToString());
                        }
                    }
                    else
                        WriteLogFile(string.Format("No Card entries avilable for the Well : " + well.Name));
                    Console.WriteLine("Added the Report history for : " + well.Name);
                }
            }
            else
                WriteLogFile(string.Format("No RRL Wells inside the Database"));
        }

        public void AddWorkover_RRL(int wellStart, int wellEnd)
        {
            Authenticate();
            for (int i = wellStart; i <= wellEnd; i++)
            {
                WellDTO well = WellService.GetWell(i.ToString());
                try
                {
                    //Add Workover with No Completion Date
                    WellboreDTO wellComponents = WellboreComponentService.GetWellboreComponentData(well.Id.ToString());
                    if (wellComponents != null)
                    {
                        WellboreComponentDTO rod = wellComponents.WellboreGrid.FirstOrDefault(x => x.Grouping == "Rod String").Wellboredata.FirstOrDefault(x => x.Name == "PCP-W  - D  - 1.000 ");
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
                    }
                    else
                        WriteLogFile(string.Format("Failed to retrieve wellbore components"));
                }
                catch
                {
                    WriteLogFile(string.Format("Failed to Add failure report for : " + well.Name));
                }
            }
        }

        public void RemoveWells(int wellStart, int wellEnd)
        {
            Authenticate();
            for (int i = wellStart; i <= wellEnd; i++)
            {
                WellDTO well = WellService.GetWell(i.ToString());
                if (well == null)
                    continue;
                try
                {
                    WellConfigurationService.RemoveWellConfig(well.Id.ToString());
                }
                catch
                {
                    Console.WriteLine("Failed to delete well : " + well.Name);
                }
                Console.WriteLine("Well Removed : " + well.Name);
            }
        }

        public void AddHistoricalDynacards(long wellStart, long wellEnd, string Domain, string Site, string Service)
        {
            Authenticate();
            ushort domain;
            if (!ushort.TryParse(Domain, out domain))
            {
                Console.WriteLine("Unable to convert domain- " + Domain + " to ushort datatype");
                return;
            }
            int dynaCardType = 1;
            if (!string.IsNullOrEmpty(DynaCardType))
                dynaCardType = Convert.ToInt32(DynaCardType);
            for (long i = wellStart; i <= wellEnd; i++)
            {
                int totalHours = 365 * 24 * Convert.ToInt32(YearsOfData);
                WellDTO well = WellService.GetWell(i.ToString());
                if (well == null || well.WellType != WellTypeId.RRL)
                    continue;
                for (int j = 1; j <= (totalHours / 2); j++)
                {
                    try
                    {
                        AddDyanaCard(domain, Site, Service, well.FacilityId, 2 * j, dynaCardType);
                    }
                    catch
                    {
                        WriteLogFile(string.Format("Failed to Add Dynacard for : " + well.Name + " Day: " + DateTime.UtcNow.AddDays(-j).ToString()));
                        Console.WriteLine(string.Format("Failed to Add Dynacard for : " + well.Name + " Day: " + DateTime.UtcNow.AddDays(-j).ToString()));
                    }
                }
                Console.WriteLine("Adding Dynacards complete for the well: " + well.Name);
            }
        }

        public void AddDyanaCard(ushort domain, string Site, string Service, string FacilityId, int hours, int cardType)
        {
            CardType card;
            switch (cardType)
            {
                case 1: card = CardType.Current; break;
                case 2: card = CardType.Full; break;
                case 3: card = CardType.Pumpoff; break;
                case 4: card = CardType.Alarm; break;
                case 5: card = CardType.Failure; break;
                default: card = CardType.Current; break;
            }
            string filePath = DynaCradFilePath;
            string facTag = $"{Site}.{Service}::{FacilityId}";
            CygNetDynaCardAccessor accessor = new CygNetDynaCardAccessor();
            XDocument doc = XDocument.Load(filePath);
            string header = doc.Root.Element("txHdr").ToString();
            XElement dgData = doc.Root.Element("dgData");
            string data = dgData.ToString();
            DgTransaction dgtr = new DgTransaction(header, data);
            string errorMessage = null;
            CygNetWellManager wellManager = new CygNetWellManager(domain, facTag, (Guid?)null);
            var result = accessor.ConvertResponseToDynaCardDictionary(dgtr, new CardScanParameters() { Domain = domain, FacilityTag = facTag }, wellManager, out errorMessage, string.Empty);

            IDynaCardLibrary dynaCardLibraryChannel = accessor.CreateDynacardServiceChannel<IDynaCardLibrary>();
            DynaCardEntry dynaCardEntry = new DynaCardEntry();
            dynaCardEntry.Timestamp = DateTime.UtcNow.AddHours(-hours);
            dynaCardEntry.Type = card;
            dynaCardEntry.SurfaceCards = new List<DynaCard>();
            DynaCard surfaceCard = ConvertCygNetWrapperDynaCardToDynaCardLibraryDynaCard(result["surface"]);
            surfaceCard.DynaCardProperties = new Dictionary<string, object>();
            dynaCardEntry.SurfaceCards.Add(surfaceCard);
            surfaceCard.CardTimestamp = dynaCardEntry.Timestamp;

            double strokePeriod = double.Parse(dgData.Element("StrkPeriod").Value);
            double pumpSpeed = 60.0 / strokePeriod;
            surfaceCard.DynaCardProperties.Add("StrokeLen", double.Parse(dgData.Element("StrokeLen").Value));
            surfaceCard.DynaCardProperties.Add("PumpSpeed", pumpSpeed);
            surfaceCard.DynaCardProperties.Add("HrGgeOff", double.Parse(dgData.Element("HrGgeOff").Value));
            surfaceCard.DynaCardProperties.Add("StrkPeriod", strokePeriod);
            surfaceCard.DynaCardProperties.Add("SLMin", double.Parse(dgData.Element("SLMin").Value));
            surfaceCard.DynaCardProperties.Add("SLMax", double.Parse(dgData.Element("SLMax").Value));
            surfaceCard.DynaCardProperties.Add("RTYest", 24.0);
            surfaceCard.DynaCardProperties.Add("POCMode", "POC");

            if (result.ContainsKey("downhole"))
            {
                dynaCardEntry.DownholeCards = new Dictionary<DownholeCardSource, IDictionary<Int32, IList<DynaCard>>>();
                dynaCardEntry.DownholeCards.Add(DownholeCardSource.Controller, new Dictionary<Int32, IList<DynaCard>>());
                DynaCard downholeCard = ConvertCygNetWrapperDynaCardToDynaCardLibraryDynaCard(result["downhole"]);
                downholeCard.CardTimestamp = dynaCardEntry.Timestamp;
                Dictionary<DownholeCardSource, IDictionary<Int32, IList<DynaCard>>> downholeCards = new Dictionary<DownholeCardSource, IDictionary<Int32, IList<DynaCard>>>();
                downholeCards.Add(DownholeCardSource.Controller, new Dictionary<Int32, IList<DynaCard>>());
                downholeCards.Add(DownholeCardSource.CalculatedGibbs, new Dictionary<Int32, IList<DynaCard>>());
                downholeCards.Add(DownholeCardSource.CalculatedEverittJennings, new Dictionary<Int32, IList<DynaCard>>());
                downholeCards[DownholeCardSource.Controller][0] = new List<DynaCard>();
                downholeCards[DownholeCardSource.CalculatedGibbs][0] = new List<DynaCard>();
                downholeCards[DownholeCardSource.CalculatedEverittJennings][0] = new List<DynaCard>();
                downholeCards[DownholeCardSource.Controller][0].Add(downholeCard);
                dynaCardEntry.DownholeCards = downholeCards;
            }
            DynaCardEntry existing_card = dynaCardLibraryChannel.GetCardEntry(facTag, dynaCardEntry.Timestamp, dynaCardEntry.Type);
            if (existing_card == null)
            {
                // Save to the card storage library
                dynaCardLibraryChannel.AddEntryToLibrary(facTag, dynaCardEntry);
            }
            else
            {
                dynaCardLibraryChannel.UpdateEntryInLibrary(facTag, dynaCardEntry.Timestamp, dynaCardEntry.Type, dynaCardEntry);
            }
        }

        private static DynaCard ConvertCygNetWrapperDynaCardToDynaCardLibraryDynaCard(Weatherford.POP.CygNetWrapper.Models.DynaCard card)
        {
            DynaCard libraryCard = new DynaCard();
            libraryCard.Points = new List<DataPoint>();

            for (int index = 0; index < card.DynaPoints.Count; index++)
            {
                // Get a DynaCardDataPoint from the CygNetWrapper card object
                Weatherford.POP.CygNetWrapper.Models.DynaCardDataPoint point = card.DynaPoints[index];

                // Convert it to a DynaCardLibrary DataPoint
                libraryCard.Points.Add(new DataPoint()
                {
                    Position = point.Position,
                    Load = point.Load
                });
            }

            return libraryCard;
        }

        public void LoadWellTestDataForRRL(string BaseFacTag, int welStart, int welEnd, string Domain, string Site, string Service)
        {
            Authenticate();
            LoadRRLCatalogData();
            for (int i = welStart; i <= welEnd; i++)
            {
                WellDTO well = WellService.GetWell(i.ToString());
                WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
                if (well == null)
                    continue;
                try
                {
                    for (int j = 0; j < Convert.ToInt32(WellTestCount); j++)
                    {
                        //add new wellTestData
                        WellTestDTO testDataDTO = new WellTestDTO
                        {
                            WellId = well.Id,
                            AverageCasingPressure = random.Next(1500, 1900),
                            AverageFluidAbovePump = random.Next(5000, 7000),
                            AverageTubingPressure = random.Next(1500, 1900),
                            AverageTubingTemperature = random.Next(60, 100),
                            Gas = random.Next(1500, 1900),
                            GasGravity = 1,
                            Oil = random.Next(1500, 1900),
                            OilGravity = random.Next(10, 150),
                            PumpEfficiency = 0,
                            PumpIntakePressure = 100,
                            PumpingHours = 10,
                            SPTCode = 0,
                            SPTCodeDescription = WellTestQuality.RepresentativeTest.ToString(),
                            StrokePerMinute = 0,
                            TestDuration = random.Next(1, 24),
                            Water = random.Next(1500, 1900),
                            WaterGravity = 1.010M,
                        };
                        testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(j * 5));
                        WellTestDataService.AddWellTestData(new WellTestAndUnitsDTO(units, testDataDTO));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to Add well test for : {0}" + well.Name, e.Message);
                }
                Console.WriteLine("Well Test Added for : " + well.Name);
            }
        }
    }
}