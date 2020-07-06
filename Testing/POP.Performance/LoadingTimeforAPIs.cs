using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Weatherford.DynaCardLibrary.API.Enums;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;

namespace POP.Performance
{
    public class LoadingTimeforAPIs : APIPerfTestBase
    {
        public void LoadingtimeforAllScreens()
        {
            Authenticate();
            DashboardLoadTime();
            RRLWellConfigurationLoadTime();
            NonRRLWellConfigurationLoadTime();
            TemplateJobsLoadTime();
            DocumentGroupingLoadTime();
            QuickAddLoadTime();
            RRLGroupStatusLoadTime();
            NonRRLGroupStatusLoadTime(WellTypeId.ESP);
            NonRRLGroupStatusLoadTime(WellTypeId.GLift);
            NonRRLGroupStatusLoadTime(WellTypeId.NF);
            NonRRLGroupStatusLoadTime(WellTypeId.WInj);
            NonRRLGroupStatusLoadTime(WellTypeId.GInj);
            ProductionOverviewLoadTime();
            CumulativeProductionChartLoadTime(365, ProductionType.Oil);
            CumulativeProductionChartLoadTime(365, ProductionType.Gas);
            CumulativeProductionChartLoadTime(365, ProductionType.Water);
            ProductionTileMapLoadTime(25, true);
            ProductionTileMapLoadTime(25, false);
            TargetTileMapLoadTime(25, true);
            TargetTileMapLoadTime(25, false);
            RRLWellStatusLoadTime();
            NonRRLWellStatusLoadTime(WellTypeId.ESP);
            NonRRLWellStatusLoadTime(WellTypeId.GLift);
            NonRRLWellStatusLoadTime(WellTypeId.NF);
            WellAnalysisLoadTime(WellTypeId.ESP);
            WellAnalysisLoadTime(WellTypeId.GLift);
            WellAnalysisLoadTime(WellTypeId.NF);
            JobManagementLoadTime();
            JobStatusViewLoadTime();
            CurrentWellboreLoadTime();
            FailureOverviewLoadTime();
            CardSliderLoadTime();
            DemandScanLoadTime(WellTypeId.RRL);
            DemandScanLoadTime(WellTypeId.ESP);
            DemandScanLoadTime(WellTypeId.GLift);
            DemandScanLoadTime(WellTypeId.NF);
            DemandScanLoadTime(WellTypeId.WInj);
            DemandScanLoadTime(WellTypeId.GInj);
        }

        public void DashboardLoadTime()
        {
            WellFilterDTO WellFilter = new WellFilterDTO();
            WellDTO well = WellService.GetAllWells().FirstOrDefault();
            AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            string assemblyId = assembly.Id.ToString();
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                () => WellService.GetWellsByFilter(WellFilter),
                () => PEDashboardService.GetAllKPIs(WellFilter),
                () => WellService.GetGroupFilterDisplay(),
                () => PEDashboardService.GetAllTrends(WellFilter),
                () => WellService.GetStickyNote(assemblyId),
                () => WellService.GetStickyNoteType(),
                () => WellService.GetStickyNoteStatus(),
                () => WellService.GetPriority(),
                () => WellService.GetUserDisplayName()
                );
                sw.Stop();
                Console.WriteLine("Dashboard Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Dashboard Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Dashboard while checking for the Response time : " + e.ToString());
            }
        }

        public void RRLWellConfigurationLoadTime()
        {
            WellFilterDTO WellFilter = new WellFilterDTO();
            WellDTO well = WellService.GetAllWells().FirstOrDefault(w => w.WellType == WellTypeId.RRL);
            string wellId = well.Id.ToString();
            WellConfigDTO wellConfig = WellConfigurationService.GetWellConfig(wellId);
            string manufacturer = wellConfig.ModelConfig.Surface.PumpingUnit.Manufacturer;
            string type = wellConfig.ModelConfig.Surface.PumpingUnit.Type;
            string description = wellConfig.ModelConfig.Surface.PumpingUnit.Description;
            string domainId = wellConfig.Well.DataConnection.ProductionDomain;
            string crankId = wellConfig.ModelConfig.Weights.CrankId;
            AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(wellId);
            string assemblyId = assembly.Id.ToString();
            IList<WellboreComponentDTO> wellboreData = WellboreComponentService.GetWellboreComponentData(wellId).WellboreGrid.FirstOrDefault(w => w.Grouping == "Rod String").Wellboredata;
            List<string> RodIds = new List<string>();
            foreach (WellboreComponentDTO Rod in wellboreData)
            {
                RodIds.Add(Rod.Id.ToString());
            }
            Stopwatch sw = new Stopwatch();
            try
            {
                long?[] assetIds = null;
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                () => SurveillanceService.GetControllerCapabilities(wellId),
                () => WellConfigurationService.GetMetaDataForWellAttributes(wellId),
                () => WellMTBFService.GetTubingDetailsforWellboreDiagram(wellId, "0", "null"),
                () => WellMTBFService.GetRodDetailsforWellboreDiagram(wellId, "0", "null"),
                () => WellMTBFService.GetAdditionalRodDetailsforToolTip(RodIds[0]),
                () => WellMTBFService.GetAdditionalRodDetailsforToolTip(RodIds[1]),
                () => WellMTBFService.GetAdditionalRodDetailsforToolTip(RodIds[2]),
                () => WellMTBFService.GetAdditionalRodDetailsforToolTip(RodIds[3]),
                () => WellService.GetWellAllocationGroupByPhase("1"),
                () => WellService.GetWellAllocationGroupByPhase("2"),
                () => WellService.GetWellAllocationGroupByPhase("3"),
                () => WellService.GetWellAttributeValues("lease", assetIds),
                () => WellService.GetWellAttributeValues("geographicRegion", assetIds),
                () => WellService.GetWellAttributeValues("field", assetIds),
                () => WellService.GetWellAttributeValues("foreman", assetIds),
                () => WellService.GetWellAttributeValues("engineer", assetIds),
                () => WellService.GetWellAttributeValues("gaugerBeat", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef01", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef02", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef03", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef04", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef05", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef06", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef07", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef08", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef09", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef10", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef11", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef12", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef13", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef14", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef15", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef16", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef17", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef18", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef19", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef20", assetIds),
                () => CatalogService.GetAllMotorTypes(),
                () => CatalogService.GetAllMotorSizes(),
                () => CatalogService.GetAllMotorSlips(),
                () => CatalogService.GetCranksByPumpingUnitPK(manufacturer, type, description),
                () => CatalogService.GetRRLRodManufacturers(),
                () => WellService.GetLicensedWellTypes(),
                () => DataConnectionService.GetAllProductionDomains(),
                () => WellConfigurationService.GetWellConfig(wellId),
                () => ModelFileService.GetCurrentModelHeader(wellId),
                () => DataConnectionService.GetCurrentValueServices(domainId),
                () => CatalogService.GetCrankWeightsByCrankId(crankId),
                () => WellboreComponentService.GetSubAssembliesByWellId(wellId),
                () => WellboreComponentService.GetWellboreComponentData(wellId)
                );
                sw.Stop();
                Console.WriteLine("RRL Wellconfiguration Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("RRL Wellconfiguration Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on RRL Wellconfiguration while checking for the Response time : " + e.ToString());
            }
        }

        public void NonRRLWellConfigurationLoadTime()
        {
            WellFilterDTO WellFilter = new WellFilterDTO();
            WellDTO well = WellService.GetAllWells().FirstOrDefault(w => w.WellType == WellTypeId.GLift);//pick any Non-RRL lift type(does not matter here)
            string wellId = well.Id.ToString();
            WellConfigDTO wellConfig = WellConfigurationService.GetWellConfig(wellId);
            string domainId = wellConfig.Well.DataConnection.ProductionDomain;
            AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(wellId);
            string assemblyId = assembly.Id.ToString();
            Stopwatch sw = new Stopwatch();
            try
            {
                long?[] assetIds = null;
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                () => SurveillanceService.GetControllerCapabilities(wellId),
                () => WellConfigurationService.GetMetaDataForWellAttributes(wellId),
                () => WellMTBFService.GetTubingDetailsforWellboreDiagram(wellId, "0", "null"),
                () => WellService.GetWellAllocationGroupByPhase("1"),
                () => WellService.GetWellAllocationGroupByPhase("2"),
                () => WellService.GetWellAllocationGroupByPhase("3"),
                () => WellService.GetWellAttributeValues("lease", assetIds),
                () => WellService.GetWellAttributeValues("geographicRegion", assetIds),
                () => WellService.GetWellAttributeValues("field", assetIds),
                () => WellService.GetWellAttributeValues("foreman", assetIds),
                () => WellService.GetWellAttributeValues("engineer", assetIds),
                () => WellService.GetWellAttributeValues("gaugerBeat", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef01", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef02", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef03", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef04", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef05", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef06", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef07", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef08", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef09", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef10", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef11", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef12", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef13", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef14", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef15", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef16", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef17", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef18", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef19", assetIds),
                () => WellService.GetWellAttributeValues("welUserDef20", assetIds),
                () => CatalogService.GetAllMotorTypes(),
                () => CatalogService.GetAllMotorSizes(),
                () => CatalogService.GetAllMotorSlips(),
                () => CatalogService.GetRRLRodManufacturers(),
                () => WellService.GetLicensedWellTypes(),
                () => DataConnectionService.GetAllProductionDomains(),
                () => WellConfigurationService.GetWellConfig(wellId),
                () => ModelFileService.GetCurrentModelHeader(wellId),
                () => DataConnectionService.GetCurrentValueServices(domainId),
                () => WellboreComponentService.GetSubAssembliesByWellId(wellId),
                () => ModelFileService.DownholePumpDepth(wellId)
                );
                sw.Stop();
                Console.WriteLine("Non-RRL Wellconfiguration Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Non-RRL Wellconfiguration Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Non-RRL Wellconfiguration while checking for the Response time : " + e.ToString());
            }
        }

        public void TemplateJobsLoadTime()
        {
            string jobId = JobAndEventService.GetTemplateJobs().FirstOrDefault().TemplateJobs.FirstOrDefault().JobId.ToString();
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                    () => JobAndEventService.GetTemplateJobs(),
                    () => JobAndEventService.GetPrimaryMotivationsForJob(),
                    () => JobAndEventService.GetCatalogItemGroupData(),
                    () => WellboreComponentService.GetListforWorkoverDropdown(),
                    () => JobAndEventService.GetUnitsOfMeasure(),
                    () => JobAndEventService.GetJobPlanDetails(jobId)
                    );
                sw.Stop();
                Console.WriteLine("Template Jobs Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Template Jobs Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Template Job screen while checking for the Response time : " + e.ToString());
            }
        }

        public void DocumentGroupingLoadTime()
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                DocumentService.GetDocumentGroupings();
                sw.Stop();
                Console.WriteLine("Document Grouping Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Document Grouping Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Document Grouping screen while checking for the Response time : " + e.ToString());
            }
        }

        public MetaDataReferenceData GetMetadataReferenceDataDDL(MetaDataDTO metaData, string columnValue = "", string columnValue1 = "")
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

            return cd;
        }

        public void QuickAddLoadTime()
        {
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            RRLPartTypeComponentGroupingTypeDTO compGrp = componentGroups.FirstOrDefault();
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = compGrp.ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            QuickAddComponentDTO qaComp = ComponentService.GetQuickAddComponents().FirstOrDefault().QuickAddComponents.FirstOrDefault();
            string partId = qaComp.Component.MfgCat_PartType.ToString();
            string compId = qaComp.Component.Id.ToString();
            ComponentMetaDataGroupDTO[] cmpMetaData = ComponentService.GetMetaDataForQuickAddComponentByPartTypeId(compGrp.ptgFK_c_MfgCat_PartType.ToString());
            MetaDataDTO[] cdReference = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;
            //Manufacturer
            MetaDataDTO mdManufacturer = cdReference.FirstOrDefault(x => x.Title == "Manufacturer");
            MetaDataReferenceData Manufacturer = GetMetadataReferenceDataDDL(mdManufacturer, compGrp.ptgFK_c_MfgCat_PartType.ToString());
            ControlIdTextDTO manufacturer = JobAndEventService.GetMetaDataReferenceData(Manufacturer).FirstOrDefault();
            //CatalogItem Description
            MetaDataDTO mdCatDescription = cdReference.FirstOrDefault(x => x.Title == "Catalog Item Description");
            MetaDataReferenceData CatlogDescription = GetMetadataReferenceDataDDL(mdCatDescription, compGrp.ptgFK_c_MfgCat_PartType.ToString(), manufacturer.ControlId.ToString());
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                    () => ComponentService.GetQuickAddComponents(),
                    () => ComponentService.GetPartTypes(partfilter),
                    () => ComponentService.GetComponentGroups(groupfilter),
                    () => ComponentService.GetComponentGroups(groupfilter),
                    () => ComponentService.GetMetaDataForUpdateQuickAddComponent(partId, compId),
                    () => JobAndEventService.GetMetaDataReferenceData(Manufacturer),
                    () => JobAndEventService.GetMetaDataReferenceData(CatlogDescription)
                    );
                sw.Stop();
                Console.WriteLine("Quick Add Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Quick Add Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Quick Add screen while checking for the Response time : " + e.ToString());
            }
        }

        public void RRLGroupStatusLoadTime()
        {
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Description == "Down POC Modes");
            string settingName = systemSettings.Name;
            var newGroup = new WellGroupStatusQueryDTO();
            newGroup.WellType = WellTypeId.RRL;
            List<string> columnName = new List<string>();
            List<string> filterConditions = new List<string>();
            List<string> filterDataTypes = new List<string>();
            List<string> filterValues = new List<string>();
            WellGroupStatusColumnFilterDTO columnFilter = new WellGroupStatusColumnFilterDTO();
            columnFilter.ColumnNames = columnName.ToArray();
            columnFilter.FilterConditions = filterConditions.ToArray();
            columnFilter.FilterDataTypes = filterDataTypes.ToArray();
            columnFilter.FilterValues = filterValues.ToArray();
            WellFilterDTO wellbyFilters = new WellFilterDTO();
            newGroup.WellFilter = wellbyFilters;
            newGroup.wellGroupStatusColumnFilter = columnFilter;
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                    () => SettingService.GetSystemSettingByName(settingName),
                    () => SurveillanceService.GetWellGroupStatus(newGroup)
                    );
                sw.Stop();
                Console.WriteLine("RRL Group Status Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("RRL Group Status Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on RRL Group Status screen while checking for the Response time : " + e.ToString());
            }
        }

        public void NonRRLGroupStatusLoadTime(WellTypeId type)
        {
            var GroupStatus = new WellGroupStatusQueryDTO();
            GroupStatus.WellType = type;
            GroupStatus.WellFilter = new WellFilterDTO();
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                SurveillanceService.GetWellGroupStatus(GroupStatus);
                sw.Stop();
                Console.WriteLine(type.ToString() + " Group Status Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile(type.ToString() + " Group Status Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on" + type.ToString() + " Group Status screen while checking for the Response time : " + e.ToString());
            }
        }

        public void ProductionOverviewLoadTime()
        {
            TileMapFilterDTO prodTileMap = new TileMapFilterDTO();
            prodTileMap.DateRequested = DateTime.Today.ToUniversalTime();
            prodTileMap.IsNegative = true;
            prodTileMap.TopWellCountRequested = 5;
            prodTileMap.WellFilter = new WellFilterDTO();
            TileMapFilterDTO targetTileMap = new TileMapFilterDTO();
            targetTileMap.DateRequested = DateTime.Today.ToUniversalTime();
            targetTileMap.IsNegative = true;
            targetTileMap.TopWellCountRequested = 5;
            targetTileMap.WellFilter = new WellFilterDTO();
            TileMapFilterDTO prodChart = new TileMapFilterDTO();
            prodChart.WellFilter = new WellFilterDTO();
            string startDate = DateTime.Today.AddDays(-7).ToString("yyyy-mm-dd");
            string endDate = DateTime.Today.ToString("yyyy-mm-dd");
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                    () => SurveillanceService.GetProductionChangeTileMap(prodTileMap),
                    () => SurveillanceService.GetProductionTargetTileMap(targetTileMap),
                    () => SurveillanceService.GetWellDailyCumulativeData(prodChart, ProductionType.Oil.ToString(), startDate, endDate)
                    );
                sw.Stop();
                Console.WriteLine("Production Overview Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Production Overview Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Production Overview screen while checking for the Response time : " + e.ToString());
            }
        }

        public void CumulativeProductionChartLoadTime(int days, ProductionType type)
        {
            TileMapFilterDTO prodChart = new TileMapFilterDTO();
            prodChart.WellFilter = new WellFilterDTO();
            string startDate = DateTime.Today.AddDays(-days).ToString("yyyy-mm-dd");
            string endDate = DateTime.Today.ToString("yyyy-mm-dd");
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                SurveillanceService.GetWellDailyCumulativeData(prodChart, type.ToString(), startDate, endDate);
                sw.Stop();
                Console.WriteLine("Cumulative Production tilemap for " + days.ToString() + " days : " + sw.ElapsedMilliseconds);
                WriteLogFile("Cumulative Production tilemap for " + days.ToString() + " days : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Cumulative Production while checking for the Response time : " + e.ToString());
            }
        }

        public void ProductionTileMapLoadTime(int count, bool IsDecreased)
        {
            TileMapFilterDTO prodTileMap = new TileMapFilterDTO();
            prodTileMap.DateRequested = DateTime.Today.ToUniversalTime();
            prodTileMap.IsNegative = IsDecreased;
            prodTileMap.TopWellCountRequested = count;
            prodTileMap.WellFilter = new WellFilterDTO();
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                SurveillanceService.GetProductionChangeTileMap(prodTileMap);
                sw.Stop();
                Console.WriteLine("Production Tilemap Load Time for " + count.ToString() + " Wells : " + sw.ElapsedMilliseconds);
                WriteLogFile("Production TileMap Load Time for " + count.ToString() + " Wells : " + +sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Production Tilemap while checking for the Response time : " + e.ToString());
            }
        }

        public void TargetTileMapLoadTime(int count, bool IsBelow)
        {
            TileMapFilterDTO prodTileMap = new TileMapFilterDTO();
            prodTileMap.DateRequested = DateTime.Today.ToUniversalTime();
            prodTileMap.IsNegative = IsBelow;
            prodTileMap.TopWellCountRequested = count;
            prodTileMap.WellFilter = new WellFilterDTO();
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                SurveillanceService.GetProductionChangeTileMap(prodTileMap);
                sw.Stop();
                Console.WriteLine("Target Tilemap Load Time for " + count.ToString() + " Wells : " + sw.ElapsedMilliseconds);
                WriteLogFile("Target TileMap Load Time for " + count.ToString() + " Wells : " + +sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Target tilemap while checking for the Response time : " + e.ToString());
            }
        }

        public void RRLWellStatusLoadTime()
        {
            SettingType settingType = SettingType.System;
            SettingDTO systemSettings = SettingService.GetSettingsByType(settingType.ToString()).FirstOrDefault(x => x.Description == "Link Scan To Analysis");
            string settingName = systemSettings.Name;
            WellFilterDTO WellFilter = new WellFilterDTO();
            WellDTO well = WellService.GetAllWells().FirstOrDefault(w => w.WellType == WellTypeId.RRL);
            string wellId = well.Id.ToString();
            string startDate = DTOExtensions.ToISO8601(DateTime.UtcNow.AddDays(-30));
            string endDate = DTOExtensions.ToISO8601(DateTime.UtcNow);
            DynaCardEntryDTO FullDynaCard = DynacardService.GetLatestDynacardFromLibraryByType(wellId, ((int)CardType.Current).ToString());
            string cardType = ((int)CardType.Current).ToString();
            string ticks = FullDynaCard.TimestampInTicks;
            string source = ((int)FullDynaCard.DownholeCards.FirstOrDefault().CardSource).ToString();
            AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(wellId);
            string assemblyId = assembly.Id.ToString();
            IList<WellboreComponentDTO> wellboreData = WellboreComponentService.GetWellboreComponentData(wellId).WellboreGrid.FirstOrDefault(w => w.Grouping == "Rod String").Wellboredata;
            List<string> RodIds = new List<string>();
            foreach (WellboreComponentDTO Rod in wellboreData)
            {
                RodIds.Add(Rod.Id.ToString());
            }
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                    () => SurveillanceService.GetWellKPIData(wellId),
                    () => DynacardService.GetLatestIntelligentAlarmQuantitiesByWellId(wellId),
                    () => SurveillanceService.GetCygNetAlarmsByWellId(wellId),
                    () => SurveillanceService.GetPumpFillageByWell(wellId),
                    () => SurveillanceService.GetEffectiveRunTimeByWell(wellId),
                    () => SurveillanceService.GetRunTimeByWell(wellId),
                    () => WellMTBFService.GetTrends(wellId, startDate, endDate),
                    () => SurveillanceService.GetDeviceTypeForWell(wellId),
                    () => SurveillanceService.GetCommunicationStatusForWell(wellId),
                    () => SurveillanceService.GetLastSuccessfulTxForWell(wellId),
                    () => SurveillanceService.GetControllerCapabilities(wellId),
                    () => SettingService.GetSystemSettingByName(settingName),
                    () => DynacardService.GetLatestDynacardFromLibraryByType(wellId, ((int)CardType.Current).ToString()),
                    () => DynacardService.GetLatestDynacardFromLibraryByType(wellId, ((int)CardType.Full).ToString()),
                    () => DynacardService.GetLatestDynacardFromLibraryByType(wellId, ((int)CardType.Pumpoff).ToString()),
                    () => WellService.GetWellComments(wellId),
                    () => WellMTBFService.GetTubingDetailsforWellboreDiagram(wellId, "0", "null"),
                    () => SettingService.GetWellSettingsByWellId(wellId),
                    () => WellMTBFService.GetRodDetailsforWellboreDiagram(wellId, "0", "null"),
                    () => DynacardService.GetDownholeAnalysisMiniReportwithFluidLoads(wellId, cardType, ticks, source),
                    () => DynacardService.GetOptimalCBT(wellId, cardType, ticks, source),
                    () => WellMTBFService.GetAdditionalRodDetailsforToolTip(RodIds[0]),
                    () => WellMTBFService.GetAdditionalRodDetailsforToolTip(RodIds[1]),
                    () => WellMTBFService.GetAdditionalRodDetailsforToolTip(RodIds[2]),
                    () => WellMTBFService.GetAdditionalRodDetailsforToolTip(RodIds[3])
                    );
                sw.Stop();
                Console.WriteLine("RRL Well Status Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("RRL Well Status Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on RRL Well Status screen while checking for the Response time : " + e.ToString());
            }
        }

        public void NonRRLWellStatusLoadTime(WellTypeId type)
        {
            WellFilterDTO WellFilter = new WellFilterDTO();
            WellDTO well = WellService.GetAllWells().FirstOrDefault(w => w.WellType == type);
            string wellId = well.Id.ToString();
            string startDate = DTOExtensions.ToISO8601(DateTime.UtcNow.AddDays(-30));
            string endDate = DTOExtensions.ToISO8601(DateTime.UtcNow);
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                    () => SurveillanceService.GetProductionTrends(wellId, startDate, endDate),
                    () => SurveillanceService.GetWellStatusData(wellId),
                    () => SurveillanceService.GetNonRRLWellGauges(wellId),
                    () => SurveillanceService.GetProductionKPI(wellId, DTOExtensions.ToISO8601(DateTime.UtcNow.Date)),
                    () => SurveillanceService.GetWellDailyAverageAndTest(wellId, DTOExtensions.ToISO8601(DateTime.UtcNow.Date)),
                    () => WellService.GetWellComments(wellId),
                    () => SurveillanceService.GetControllerCapabilities(wellId)
                    );
                sw.Stop();
                Console.WriteLine("NonRRL Well Status Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("NonRRL Well Status Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on NonRRL Well Status screen while checking for the Response time : " + e.ToString());
            }
        }

        public void WellAnalysisLoadTime(WellTypeId type)
        {
            WellFilterDTO WellFilter = new WellFilterDTO();
            WellDTO well = WellService.GetAllWells().FirstOrDefault(w => w.WellType == type);
            string wellId = well.Id.ToString();
            AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            string assemblyId = assembly.Id.ToString();
            WellTestHeaderDTO[] wellTestHeaders = WellTestDataService.GetValidWellTestHeadersByWellId(well.Id.ToString());
            string testId = wellTestHeaders.FirstOrDefault().Id.ToString();
            NodalAnalysisInputAndUnitsDTO input = WellTestDataService.GetAnalysisInputDataAndUnits(testId);
            Stopwatch sw = new Stopwatch();
            try
            {
                switch (type)
                {
                    case WellTypeId.ESP:
                        {
                            sw.Start();
                            System.Threading.Tasks.Parallel.Invoke(
                                () => WellTestDataService.GetValidWellTestHeadersByWellId(well.Id.ToString()),
                                () => WellTestDataService.GetAnalysisInputDataAndUnits(testId),
                                () => WellTestDataService.PerformESPAnalysis(input)
                                );
                            sw.Stop();
                        }
                        break;

                    case WellTypeId.GLift:
                        {
                            sw.Start();
                            System.Threading.Tasks.Parallel.Invoke(
                                () => WellTestDataService.GetValidWellTestHeadersByWellId(well.Id.ToString()),
                                () => WellTestDataService.GetAnalysisInputDataAndUnits(testId),
                                () => WellTestDataService.PerformGasLiftAnalysis(input)
                                );
                            sw.Stop();
                        }
                        break;

                    case WellTypeId.NF:
                        {
                            sw.Start();
                            System.Threading.Tasks.Parallel.Invoke(
                                () => WellTestDataService.GetValidWellTestHeadersByWellId(well.Id.ToString()),
                                () => WellTestDataService.GetAnalysisInputDataAndUnits(testId),
                                () => WellTestDataService.PerformAnalysis(input)
                                );
                            sw.Stop();
                        }
                        break;

                    default:
                        Console.WriteLine("No Well Analysis screen for the well type : " + type.ToString());
                        break;
                }
                Console.WriteLine("NonRRL Well Analysis Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("NonRRL Well Analysis Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on NonRRL Well Analysis screen while checking for the Response time : " + e.ToString());
            }
        }

        public void JobManagementLoadTime()
        {
            WellFilterDTO WellFilter = new WellFilterDTO();
            WellDTO well = WellService.GetAllWells().FirstOrDefault(w => w.Name.Contains("WSM"));
            string wellId = well.Id.ToString();
            AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            string assemblyId = assembly.Id.ToString();
            JobTypeDTO[] jobTypes = JobAndEventService.GetJobTypes();
            string jobtypeId = jobTypes.FirstOrDefault().id.ToString();
            JobLightDTO jobs = JobAndEventService.GetJobsByWell(wellId).FirstOrDefault();
            string jobId = jobs.JobId.ToString();
            ControlIdTextDTO subAssemby = ComponentService.GetSubAssembliesByAssemblyId(assemblyId).FirstOrDefault();
            ComponentVerticalSchematicInputDTO inputSchematic = new ComponentVerticalSchematicInputDTO();
            inputSchematic.AssemblyId = Convert.ToInt64(assemblyId);
            inputSchematic.SubAssemblyId = subAssemby.ControlId;
            inputSchematic.EndDate = jobs.EndDate;
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                    () => JobAndEventService.GetJobsByWell(wellId),
                    () => JobAndEventService.GetTemplateJobs(),
                    () => JobAndEventService.GetJobTypes(),
                    () => JobAndEventService.GetJobStatuses(),
                    () => JobAndEventService.GetCatalogItemGroupData(),
                    () => JobAndEventService.GetPrimaryMotivationsForJob(),
                    () => JobAndEventService.GetAFEs(),
                    () => JobAndEventService.GetJobReasonsForJobType(jobtypeId),
                    () => JobAndEventService.GetJobApprovals(jobId)
                    );
                sw.Stop();
                Console.WriteLine("Job Management General Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Job Management General Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Job Management General screen while checking for the Response time : " + e.ToString());
            }
            sw.Reset();
            try
            {
                sw.Start();
                JobAndEventService.GetEvents(jobId);
                sw.Stop();
                Console.WriteLine("Job Management Events Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Job Management Events Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Job Management Events screen while checking for the Response time : " + e.ToString());
            }
            sw.Reset();
            try
            {
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                    () => WellMTBFService.GetProductionFigures(wellId),
                    () => WellTestDataService.GetWellTestTrend(wellId),
                    () => JobAndEventService.GetProductPriceScenarios("9"),
                    () => JobAndEventService.GetProductPriceScenarios("2"),
                    () => JobAndEventService.GetProductPriceScenarios("3"),
                    () => JobAndEventService.GetStateTax(),
                    () => JobAndEventService.GetJobEconomicAnalysis(jobId, wellId)
                    );
                sw.Stop();
                Console.WriteLine("Job Management Economic Analysis Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Job Management Economic Analysis Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Job Management Economic Analysis screen while checking for the Response time : " + e.ToString());
            }
            sw.Reset();
            try
            {
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                    () => JobAndEventService.GetCatalogItemGroupData(),
                    () => JobAndEventService.GetJobCostDetailsForJob(jobId)
                    );
                sw.Stop();
                Console.WriteLine("Job Management JobCost Details Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Job Management JobCost Details Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Job Management JobCost Details screen while checking for the Response time : " + e.ToString());
            }
            sw.Reset();
            try
            {
                sw.Start();
                JobAndEventService.GetJobComponentFailure(jobId);
                sw.Stop();
                Console.WriteLine("Job Management Failure Report Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Job Management Failure Report Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Job Management Failure Report screen while checking for the Response time : " + e.ToString());
            }
            sw.Reset();
            try
            {
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                    () => JobAndEventService.GetCatalogItemGroupData(),
                    () => JobAndEventService.GetJobPlanDetails(jobId),
                    () => JobAndEventService.GetUnitsOfMeasure()
                    );
                sw.Stop();
                Console.WriteLine("Job Management Job Plan Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Job Management Job Plan Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Job Management Job Plan screen while checking for the Response time : " + e.ToString());
            }
            sw.Reset();
            try
            {
                sw.Start();
                var eventId = JobAndEventService.GetEvents(jobId)?.First()?.EventData?.First()?.Id;
                if (eventId != null)
                {
                    JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, eventId.ToString());
                }
                sw.Stop();
                Console.WriteLine("Job Management Wellbore Report Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Job Management Wellbore Report Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Job Management Wellbore Report screen while checking for the Response time : " + e.ToString());
            }
            sw.Reset();
            try
            {
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                    () => ComponentService.GetSubAssembliesByAssemblyId(assemblyId),
                    () => ComponentService.GetComponentVerticalSchematic(inputSchematic)
                    );
                sw.Stop();
                Console.WriteLine("Job Management Wellbore Diagram Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Job Management Wellbore Diagram Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Job Management Wellbore Diagram screen while checking for the Response time : " + e.ToString());
            }
        }

        public void JobStatusViewLoadTime()
        {
            WellDTO well = WellService.GetAllWells().FirstOrDefault();
            WellFilterDTO wellFilter = new WellFilterDTO();
            JobStatusViewDTO[] prospectiveJobs = JobAndEventService.GetJobsByCategory(wellFilter, JobStatusCategory.Prospective.ToString("d"));
            JobStatusViewDTO jobs = prospectiveJobs.FirstOrDefault();
            string jobId = jobs.JobId.ToString();
            string wellId = well.Id.ToString();
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                    () => JobAndEventService.GetJobsByCategory(wellFilter, JobStatusCategory.Prospective.ToString("d")),
                    () => JobAndEventService.GetJobById(jobId),
                    () => JobAndEventService.GetJobApprovals(jobId),
                    () => JobAndEventService.IsJobApprovable(jobId),
                    () => JobAndEventService.GetJobEconomicAnalysis(jobId, wellId)
                    );
                sw.Stop();
                Console.WriteLine("Prospective Jobs Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Prospective Jobs Diagram Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Prospective Jobs screen while checking for the Response time : " + e.ToString());
            }
            sw.Reset();
            try
            {
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                    () => JobAndEventService.GetJobsByCategory(wellFilter, JobStatusCategory.Ready.ToString("d")),
                    () => JobAndEventService.GetJobById(jobId),
                    () => JobAndEventService.GetJobApprovals(jobId),
                    () => JobAndEventService.GetJobEconomicAnalysis(jobId, wellId)
                    );
                sw.Stop();
                Console.WriteLine("Ready Jobs Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Ready Jobs Diagram Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Ready Jobs screen while checking for the Response time : " + e.ToString());
            }
            sw.Reset();
            try
            {
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                    () => JobAndEventService.GetJobsByCategory(wellFilter, JobStatusCategory.Concluded.ToString("d")),
                    () => JobAndEventService.GetJobById(jobId),
                    () => JobAndEventService.GetJobApprovals(jobId),
                    () => JobAndEventService.GetJobEconomicAnalysis(jobId, wellId)
                    );
                sw.Stop();
                Console.WriteLine("Concluded Jobs Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Concluded Jobs Diagram Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Concluded Jobs screen while checking for the Response time : " + e.ToString());
            }
        }

        public void CurrentWellboreLoadTime()
        {
            WellDTO well = WellService.GetAllWells().FirstOrDefault(w => w.Name.Contains("WSM"));
            string wellId = well.Id.ToString();
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                WellboreComponentService.GetWellboreComponentData(wellId);
                sw.Stop();
                Console.WriteLine("Current Wellbore Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Current Wellbore Diagram Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Current Wellbore screen while checking for the Response time : " + e.ToString());
            }
        }

        public void FailureOverviewLoadTime()
        {
            WellFilterDTO filter = new WellFilterDTO();
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                System.Threading.Tasks.Parallel.Invoke(
                    () => HeatMapService.GetGroupComponentMTBF(filter, WellTypeId.RRL.ToString()),
                    () => HeatMapService.GetWellsForHeatMap(filter, "1", "7"),
                    () => HeatMapService.GetTopWellsToFail(filter, WellTypeId.RRL.ToString(), "7", "15")
                    );
                sw.Stop();
                Console.WriteLine("Failure Overview Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Failure Overview Diagram Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Failure Overview screen while checking for the Response time : " + e.ToString());
            }
        }

        public void CardSliderLoadTime()
        {
            WellDTO well = WellService.GetAllWells().Where(x => x.WellType == WellTypeId.RRL).FirstOrDefault();
            string ccType = ((int)CardType.Current).ToString();
            string fcType = ((int)CardType.Full).ToString();
            string pcType = ((int)CardType.Pumpoff).ToString();
            string acType = ((int)CardType.Alarm).ToString();
            string failureType = ((int)CardType.Failure).ToString();
            string cards = ccType + "," + fcType + "," + pcType + "," + acType + "," + failureType;
            Stopwatch sw = new Stopwatch();
            try
            {
                sw.Start();
                DynacardEntryCollectionAndUnitsDTO cardEntries = DynacardService.GetCardEntriesByDateRange(well.Id.ToString(), cards, DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddYears(-1)), DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime()));
                sw.Stop();
                Console.WriteLine("Card Slider Load Time : " + sw.ElapsedMilliseconds);
                WriteLogFile("Card Slider Load Time : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Card Slider while checking for the Response time : " + e.ToString());
            }
        }

        public void DemandScanLoadTime(WellTypeId type)
        {
            long[] wellIds;
            var GroupStatus = new WellGroupStatusQueryDTO();
            GroupStatus.WellType = type;
            Stopwatch sw = new Stopwatch();
            try
            {
                switch (type)
                {
                    case WellTypeId.RRL:
                        {
                            var newGroup = new WellGroupStatusQueryDTO();
                            List<string> columnName = new List<string>();
                            List<string> filterConditions = new List<string>();
                            List<string> filterDataTypes = new List<string>();
                            List<string> filterValues = new List<string>();
                            WellGroupStatusColumnFilterDTO columnFilter = new WellGroupStatusColumnFilterDTO();
                            columnFilter.ColumnNames = columnName.ToArray();
                            columnFilter.FilterConditions = filterConditions.ToArray();
                            columnFilter.FilterDataTypes = filterDataTypes.ToArray();
                            columnFilter.FilterValues = filterValues.ToArray();
                            WellFilterDTO wellbyFilters = new WellFilterDTO();
                            newGroup.WellFilter = wellbyFilters;
                            newGroup.WellType = type;
                            newGroup.wellGroupStatusColumnFilter = columnFilter;
                            var rrl_wells = WellService.GetAllWells().Where(x => x.WellType == WellTypeId.RRL).Take(20);
                            wellIds = rrl_wells.Select(t => t.Id).ToArray();
                            sw.Start();
                            System.Threading.Tasks.Parallel.Invoke(
                                () => SurveillanceService.IssueCommandForWells(wellIds, WellCommand.DemandScan.ToString(), "true"),
                                () => SurveillanceService.GetWellGroupStatus(newGroup)
                                );
                            sw.Stop();
                        }
                        break;

                    case WellTypeId.ESP:
                        {
                            var esp_wells = WellService.GetAllWells().Where(x => x.WellType == WellTypeId.ESP).Take(20);
                            wellIds = esp_wells.Select(t => t.Id).ToArray();
                            sw.Start();
                            System.Threading.Tasks.Parallel.Invoke(
                                () => SurveillanceService.IssueCommandForWells(wellIds, WellCommand.DemandScan.ToString(), "true"),
                                () => SurveillanceService.GetWellGroupStatus(GroupStatus)
                                );
                            sw.Stop();
                        }
                        break;

                    case WellTypeId.GLift:
                        {
                            var gl_wells = WellService.GetAllWells().Where(x => x.WellType == WellTypeId.GLift).Take(20);
                            wellIds = gl_wells.Select(t => t.Id).ToArray();
                            sw.Start();
                            System.Threading.Tasks.Parallel.Invoke(
                                () => SurveillanceService.IssueCommandForWells(wellIds, WellCommand.DemandScan.ToString(), "true"),
                                () => SurveillanceService.GetWellGroupStatus(GroupStatus)
                                );
                            sw.Stop();
                        }
                        break;

                    case WellTypeId.NF:
                        {
                            var nfw_wells = WellService.GetAllWells().Where(x => x.WellType == WellTypeId.NF).Take(20);
                            wellIds = nfw_wells.Select(t => t.Id).ToArray();
                            sw.Start();
                            System.Threading.Tasks.Parallel.Invoke(
                                () => SurveillanceService.IssueCommandForWells(wellIds, WellCommand.DemandScan.ToString(), "true"),
                                () => SurveillanceService.GetWellGroupStatus(GroupStatus)
                                );
                            sw.Stop();
                        }
                        break;

                    case WellTypeId.WInj:
                        {
                            var wi_wells = WellService.GetAllWells().Where(x => x.WellType == WellTypeId.WInj).Take(20);
                            wellIds = wi_wells.Select(t => t.Id).ToArray();
                            sw.Start();
                            System.Threading.Tasks.Parallel.Invoke(
                                () => SurveillanceService.IssueCommandForWells(wellIds, WellCommand.DemandScan.ToString(), "true"),
                                () => SurveillanceService.GetWellGroupStatus(GroupStatus)
                                );
                            sw.Stop();
                        }
                        break;

                    case WellTypeId.GInj:
                        {
                            var gi_wells = WellService.GetAllWells().Where(x => x.WellType == WellTypeId.GInj).Take(20);
                            wellIds = gi_wells.Select(t => t.Id).ToArray();
                            sw.Start();
                            System.Threading.Tasks.Parallel.Invoke(
                                () => SurveillanceService.IssueCommandForWells(wellIds, WellCommand.DemandScan.ToString(), "true"),
                                () => SurveillanceService.GetWellGroupStatus(GroupStatus)
                                );
                            sw.Stop();
                        }
                        break;

                    default:
                        Console.WriteLine("Demand scan cannot be issued for the well type : " + type.ToString());
                        break;
                }
                Console.WriteLine("Group Status Load Time after issuing the Deman scan : " + sw.ElapsedMilliseconds);
                WriteLogFile("Group Status Load Time after issuing the Deman scan : " + sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception thrown on Demand Scan while checking for the Response time : " + e.ToString());
            }
        }
    }
}