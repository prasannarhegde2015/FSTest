using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class ComponentServiceTests : APIClientTestBase
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

        public WellConfigDTO AddWell(string facilityIdBase)
        {
            string facilityId = s_isRunningInATS ? facilityIdBase + "00001" : facilityIdBase + "0001";
            WellConfigDTO well = WellConfigurationService.AddWellConfig(new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = DefaultWellName,
                    FacilityId = facilityId,
                    DataConnection = GetDefaultCygNetDataConnection(),
                    IntervalAPI = "IntervalAPI",
                    SubAssemblyAPI = "SubAssemblyAPI",
                    AssemblyAPI = "AssemblyAPI",
                    CommissionDate = DateTime.Today,
                    WellType = WellTypeId.RRL,
                })
            });
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well.Well);
            return well;
        }

        public WellConfigDTO AddWellWithDefinedConfiguration(WellDTO wellDTO)
        {
            WellConfigDTO well = WellConfigurationService.AddWellConfig(new WellConfigDTO()
            {
                Well = SetDefaultFluidType(wellDTO)
            });

            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well.Well);
            return well;
        }

        public string AddJob(string jobStatus, int startDate = 0)
        {
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));

            JobLightDTO job = new JobLightDTO();
            job.WellId = well.Id;
            job.WellName = well.Name;
            job.BeginDate = DateTime.Today.AddDays(startDate).ToUniversalTime();
            if (startDate == 0)
                job.EndDate = DateTime.Today.AddDays(30).ToUniversalTime();
            else
                job.EndDate = DateTime.Today.AddDays(startDate + 30).ToUniversalTime();
            job.ActualCost = (decimal)100000.00;
            job.ActualJobDurationDays = (decimal)20.5;
            job.TotalCost = (decimal)150000.00;
            job.JobRemarks = "TestJobRemarks " + DateTime.UtcNow.ToString();
            job.JobOrigin = "TestJobOrigin ";
            job.AssemblyId = well.AssemblyId;
            job.WellId = well.Id;
            //For Below fields User can select any value in the dropdown menu
            job.AFEId = JobAndEventService.GetAFEs().FirstOrDefault().Id;
            job.StatusId = JobAndEventService.GetJobStatuses().FirstOrDefault(j => j.Name == jobStatus).Id;
            job.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault().id;
            job.BusinessOrganizationId = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            job.AccountRef = "1";
            //JobReasonId drop down selection is based on the JobTypeId
            job.JobReasonId = JobAndEventService.GetJobReasonsForJobType(job.JobTypeId.ToString()).FirstOrDefault().Id;
            job.JobDriverId = (int)JobAndEventService.GetPrimaryMotivationsForJob().FirstOrDefault().Id;
            //Add Job
            string addJob = JobAndEventService.AddJob(job);
            Assert.IsNotNull(addJob, "Failed to add a Job");
            //Get Job
            JobLightDTO getJob = JobAndEventService.GetJobById(addJob);
            Assert.AreEqual(job.WellId, getJob.WellId);
            Assert.AreEqual(job.WellName, getJob.WellName);
            Assert.AreEqual(job.BeginDate, getJob.BeginDate);
            Assert.AreEqual(job.EndDate, getJob.EndDate);
            Assert.AreEqual(job.ActualCost, getJob.ActualCost);
            Assert.AreEqual(job.ActualJobDurationDays, getJob.ActualJobDurationDays);
            Assert.AreEqual(job.TotalCost, getJob.TotalCost);
            Assert.AreEqual(job.JobRemarks, getJob.JobRemarks);
            Assert.AreEqual(job.JobOrigin, getJob.JobOrigin);
            Assert.AreEqual(job.AssemblyId, getJob.AssemblyId);
            Assert.AreEqual(job.AFEId, getJob.AFEId);
            Assert.AreEqual(job.StatusId, getJob.StatusId);
            Assert.AreEqual(job.JobTypeId, getJob.JobTypeId);
            Assert.AreEqual(job.BusinessOrganizationId, job.BusinessOrganizationId);
            Assert.AreEqual(job.AccountRef, getJob.AccountRef);
            Assert.AreEqual(job.JobReasonId, getJob.JobReasonId);
            Assert.AreEqual(job.JobDriverId, getJob.JobDriverId);
            return addJob;
        }

        public string AddJobWithUserDefinedConfiguration(string jobStatus, string definedName, int startDate = 0)
        {
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(definedName));

            JobLightDTO job = new JobLightDTO();
            job.WellId = well.Id;
            job.WellName = well.Name;
            job.BeginDate = DateTime.Today.AddDays(startDate).ToUniversalTime();
            if (startDate == 0)
                job.EndDate = DateTime.Today.AddDays(30).ToUniversalTime();
            else
                job.EndDate = DateTime.Today.AddDays(startDate + 30).ToUniversalTime();
            job.ActualCost = (decimal)100000.00;
            job.ActualJobDurationDays = (decimal)20.5;
            job.TotalCost = (decimal)150000.00;
            job.JobRemarks = "TestJobRemarks " + DateTime.UtcNow.ToString();
            job.JobOrigin = "TestJobOrigin ";
            job.AssemblyId = well.AssemblyId;
            job.WellId = well.Id;
            //For Below fields User can select any value in the dropdown menu
            job.AFEId = JobAndEventService.GetAFEs().FirstOrDefault().Id;
            job.StatusId = JobAndEventService.GetJobStatuses().FirstOrDefault(j => j.Name == jobStatus).Id;
            job.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault().id;
            job.BusinessOrganizationId = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            job.AccountRef = "1";
            //JobReasonId drop down selection is based on the JobTypeId
            job.JobReasonId = JobAndEventService.GetJobReasonsForJobType(job.JobTypeId.ToString()).FirstOrDefault().Id;
            job.JobDriverId = (int)JobAndEventService.GetPrimaryMotivationsForJob().FirstOrDefault().Id;
            //Add Job
            string addJob = JobAndEventService.AddJob(job);
            Assert.IsNotNull(addJob, "Failed to add a Job");
            //Get Job
            JobLightDTO getJob = JobAndEventService.GetJobById(addJob);
            Assert.AreEqual(job.WellId, getJob.WellId);
            Assert.AreEqual(job.WellName, getJob.WellName);
            Assert.AreEqual(job.BeginDate, getJob.BeginDate);
            Assert.AreEqual(job.EndDate, getJob.EndDate);
            Assert.AreEqual(job.ActualCost, getJob.ActualCost);
            Assert.AreEqual(job.ActualJobDurationDays, getJob.ActualJobDurationDays);
            Assert.AreEqual(job.TotalCost, getJob.TotalCost);
            Assert.AreEqual(job.JobRemarks, getJob.JobRemarks);
            Assert.AreEqual(job.JobOrigin, getJob.JobOrigin);
            Assert.AreEqual(job.AssemblyId, getJob.AssemblyId);
            Assert.AreEqual(job.AFEId, getJob.AFEId);
            Assert.AreEqual(job.StatusId, getJob.StatusId);
            Assert.AreEqual(job.JobTypeId, getJob.JobTypeId);
            Assert.AreEqual(job.BusinessOrganizationId, job.BusinessOrganizationId);
            Assert.AreEqual(job.AccountRef, getJob.AccountRef);
            Assert.AreEqual(job.JobReasonId, getJob.JobReasonId);
            Assert.AreEqual(job.JobDriverId, getJob.JobDriverId);
            return addJob;
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void GetComponentGroupsGetPartTypes()
        {
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            foreach (RRLPartTypeComponentGroupingTypeDTO cg in componentGroups)
            {
                partfilter.TypeId = cg.ptgFK_c_MfgCat_ComponentGrouping;
                RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
                Assert.IsNotNull(partTypes, "Failed to retrieve Part types");
                foreach (RRLPartTypeComponentGroupingTypeDTO pt in partTypes)
                {
                    Assert.AreEqual(cg.strComponentGrouping, pt.strComponentGrouping);
                }
            }
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void GetComponentMetadataforPartType()
        {
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeOfFilter = ComponentFilterTypes.PartType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");
            foreach (RRLPartTypeComponentGroupingTypeDTO pt in partTypes)
            {
                //9/3/2019 - To avoid compilation error, passed an added parameter as null. Integration test needs to be fixed according to FRWM-5489
                //string hardCodedComponentGroupId = null;
                ComponentMetaDataGroupDTO[] compMetaData = ComponentService.GetComponentMetaDataForAdd(pt.ptgFK_c_MfgCat_PartType.ToString(), pt.ptgFK_c_MfgCat_ComponentGrouping.ToString());
                Assert.IsNotNull(compMetaData, "Failed retrieve Metadata for partType : " + pt.ptyPartType);
                Assert.IsTrue(compMetaData.Count() >= 1, "Failed to Retrieve MetaData");
                Assert.AreEqual("Required", compMetaData.FirstOrDefault().CategoryName, "Does not have Required Category");
            }
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void GetWellboreComponents()
        {
            bool dcExisted = false;
            WellDTO well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", DataConnection = GetDefaultCygNetDataConnection(), CommissionDate = DateTime.Today, AssemblyAPI = "1234567890", SubAssemblyAPI = "123456789012", IntervalAPI = "12345678901234", WellType = WellTypeId.RRL });
            dcExisted = DataConnectionService.GetAllDataConnections().FirstOrDefault(dc => dc.ProductionDomain == well.DataConnection.ProductionDomain && dc.Site == well.DataConnection.Site && dc.Service == well.DataConnection.Service) != null;
            WellDTO[] wells = WellService.GetAllWells();
            Assert.IsFalse(wells.Any(w => w.Name.Equals(well.Name)), "Well already exists in database.");
            WellConfigDTO wellConfigDTO = new WellConfigDTO();
            wellConfigDTO.Well = well;
            wellConfigDTO.ModelConfig = ReturnFullyPopulatedModel(tubingAnchorDepth: 5130, casingOD: 7);
            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfigDTO);
            wells = WellService.GetAllWells();
            WellDTO getwell = wells.FirstOrDefault(w => w.Name == well.Name);
            _wellsToRemove.Add(getwell);
            if (!dcExisted)
            {
                _dataConnectionsToRemove.Add(getwell.DataConnection);
            }
            string jobId = AddJob("Approved");
            //Get Components
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.WellBoreReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            var check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
            WellboreGridDTO[] wellboreComponent = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());
            Assert.IsNotNull(wellboreComponent);
            //since the wellbore components created during well configuration are excluded, null will be returned
            Assert.AreEqual(null, wellboreComponent.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore")?.WellboreGridGroup.Count());
            bool rcheck = JobAndEventService.RemoveJob(jobId);
            Assert.IsTrue(rcheck, "Failed to remove Job");
            WellConfigurationService.RemoveWellConfig(getwell.Id.ToString());
            _wellsToRemove.Remove(getwell);
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void QuickAddComponentCRUD()
        {
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");
            foreach (RRLPartTypeComponentGroupingTypeDTO cg in partTypes)
            {
                Assert.AreEqual("Surface Equipment", cg.strComponentGrouping);
            }

            //Meta Data for Add quick add component
            ComponentMetaDataGroupDTO[] cmpMetaData = ComponentService.GetMetaDataForQuickAddComponentByPartTypeId(partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType.ToString());
            Assert.IsNotNull(cmpMetaData);
            MetaDataDTO[] cdReference = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;

            //Get Meta data for the Catalog Item description
            MetaDataReferenceData cd = new MetaDataReferenceData();
            cd.MetaData = cdReference.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCatalogItem");
            MetaDataFilterDTO cdFilter = new MetaDataFilterDTO();
            cdFilter.ColumnValue = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType.ToString();
            cdFilter.MetaDataFilterToken = cd.MetaData.ExtendedFilterInput;
            List<MetaDataFilterDTO> listcdFilter = new List<MetaDataFilterDTO>();
            listcdFilter.Add(cdFilter);
            cd.UIFilterValues = listcdFilter.ToArray();
            ControlIdTextDTO[] cdMetaData = JobAndEventService.GetMetaDataReferenceData(cd);
            Assert.AreEqual(1, cdMetaData.Count());
            ControlIdTextDTO cdm = cdMetaData.FirstOrDefault(x => x.ControlText == "Transformer");

            //Add Quick Add Component
            ComponentMetaDataGroupDTO reqComponent = new ComponentMetaDataGroupDTO();
            reqComponent.QuickAddCategory = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").CategoryName;
            reqComponent.PartTypePrimaryKey = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;
            reqComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
            reqComponent.PartType = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptyPartType;
            reqComponent.ExtendedComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptyExtendedComponentTableName;
            reqComponent.CategoryName = "Required";
            reqComponent.Order = 1;
            reqComponent.Fields = cdReference;
            reqComponent.Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCatalogItem").DataValue = cdm.ControlId;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcCompName").DataValue = "Transformer";
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_Manufacturers").DataValue = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_PartType").DataValue = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;
            ComponentMetaDataGroupDTO addComponent = new ComponentMetaDataGroupDTO();
            addComponent.Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Additional").Fields;
            addComponent.CategoryName = "Additional";
            List<ComponentMetaDataGroupDTO> listComponent = new List<ComponentMetaDataGroupDTO>();
            listComponent.Add(reqComponent);
            listComponent.Add(addComponent);
            ComponentMetaDataGroupDTO[] arrComponent = listComponent.ToArray();
            string addQuickComponent = ComponentService.AddQuickAddComponent(arrComponent);
            Assert.IsNotNull(addQuickComponent, "Failed to add component");

            //Get Quick Add Components
            QuickAddComponentGroupingDTO[] quickaddComps = ComponentService.GetQuickAddComponents();
            Assert.AreEqual(1, quickaddComps.Count(), "Error adding the quick add component under the given category");
            foreach (QuickAddComponentGroupingDTO qacg in quickaddComps)
            {
                Assert.AreEqual("Required", qacg.GroupingCategory, "Mismatch in added category");
                Assert.IsNotNull(qacg.QuickAddComponents, "Failed to retrive added Quick Add Components");
                Assert.AreEqual(1, qacg.QuickAddComponents.Count(), "Failed to retrieve added Quick Add Components");
                foreach (QuickAddComponentDTO qac in qacg.QuickAddComponents)
                {
                    Assert.IsNotNull(qac.Component, "Failed to add Components by adding Quick Adding component");
                    Assert.AreEqual(JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id, qac.Component.BusinessOrganizationId, "Mismatch in Business organisation for the added quick add component");
                    Assert.AreEqual(partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptyPartType, qac.Component.PartType, "Mismatch in Part type for the added quick add component");
                    Assert.AreEqual(cdm.ControlId, qac.Component.MfgCatalogItem, "Mismatch in Catalog Item for the added quick add component");
                }
            }

            //Update quick add components
            //Metadata for Update quick add component
            ComponentMetaDataGroupDTO[] updatecmpMetaData = ComponentService.GetMetaDataForUpdateQuickAddComponent(partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType.ToString(), quickaddComps.FirstOrDefault().QuickAddComponents.FirstOrDefault().Component.Id.ToString());
            Assert.IsNotNull(updatecmpMetaData);
            MetaDataDTO[] updatecdReference = updatecmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;

            ComponentMetaDataGroupDTO ureqComponent = new ComponentMetaDataGroupDTO();
            ureqComponent.ComponentPrimaryKey = quickaddComps.FirstOrDefault().QuickAddComponents.FirstOrDefault().Component.Id;
            ureqComponent.QuickAddComponentId = quickaddComps.FirstOrDefault().QuickAddComponents.FirstOrDefault().QuickAddComponentId;
            ureqComponent.QuickAddCategory = "Updated";
            ureqComponent.PartTypePrimaryKey = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;
            ureqComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
            ureqComponent.PartType = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptyPartType;
            ureqComponent.ExtendedComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptyExtendedComponentTableName;
            ureqComponent.Order = 1;
            ureqComponent.Fields = updatecdReference;
            ureqComponent.Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;
            ureqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcPrimaryKey").DataValue = quickaddComps.FirstOrDefault().QuickAddComponents.FirstOrDefault().Component.Id;
            ureqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCatalogItem").DataValue = cdm.ControlId;
            ureqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcCompName").DataValue = "Transformer";
            ureqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_Manufacturers").DataValue = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            ureqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_PartType").DataValue = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;
            ureqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcPartType").DataValue = "Transformer";
            ComponentMetaDataGroupDTO uaddComponent = new ComponentMetaDataGroupDTO();
            uaddComponent.Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Additional").Fields;
            List<ComponentMetaDataGroupDTO> ulistComponent = new List<ComponentMetaDataGroupDTO>();
            ulistComponent.Add(ureqComponent);
            ulistComponent.Add(uaddComponent);
            ComponentMetaDataGroupDTO[] uarrComponent = ulistComponent.ToArray();
            bool updateQuickComponent = ComponentService.UpdateQuickAddComponent(uarrComponent);
            Assert.IsTrue(updateQuickComponent, "Failed to update component");

            //Get Updated Quick add component
            quickaddComps = ComponentService.GetQuickAddComponents();
            Assert.AreEqual(1, quickaddComps.Count(), "Error adding the quick add component under the given category");
            foreach (QuickAddComponentGroupingDTO qacg in quickaddComps)
            {
                Assert.AreEqual("Updated", qacg.GroupingCategory, "Mismatch in added category");
                Assert.IsNotNull(qacg.QuickAddComponents, "Failed to retrive added Quick Add Components");
                Assert.AreEqual(1, qacg.QuickAddComponents.Count(), "Failed to retrieve added Quick Add Components");
                foreach (QuickAddComponentDTO qac in qacg.QuickAddComponents)
                {
                    Assert.IsNotNull(qac.Component, "Failed to add Components by adding Quick Adding component");
                    Assert.AreEqual(JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id, qac.Component.BusinessOrganizationId, "Mismatch in Business organisation for the added quick add component");
                    Assert.AreEqual(partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptyPartType, qac.Component.PartType, "Mismatch in Part type for the added quick add component");
                    Assert.AreEqual(cdm.ControlId, qac.Component.MfgCatalogItem, "Mismatch in Catalog Item for the added quick add component");
                }
            }

            //Remove quick add component
            foreach (QuickAddComponentGroupingDTO qacg in quickaddComps)
            {
                foreach (QuickAddComponentDTO qac in qacg.QuickAddComponents)
                {
                    bool check = ComponentService.RemoveQuickAddComponent(qac.QuickAddComponentId.ToString());
                    Assert.IsTrue(check, "Failed to remove Quick Add component");
                }
            }
        }

        public EventDTO SetEventDTO(JobLightDTO getJob, long eId)
        {
            EventDTO e = new EventDTO();
            e.EndTime = DateTime.UtcNow.AddDays(5);
            e.EventTypeId = eId;
            e.PK_AFE = getJob.AFEId;
            e.PK_Assembly = getJob.AssemblyId;
            e.PK_BusinessOrganization = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            e.PK_CatalogItem = JobAndEventService.GetCatalogItemGroupData().CatalogItems.FirstOrDefault().CatalogItemId;
            e.PK_Job = getJob.JobId;
            e.PK_TruckUnit = JobAndEventService.GetCatalogItemGroupData().TruckUnits.FirstOrDefault().TruckUnitId;
            e.TotalCost = getJob.TotalCost;

            return e;
        }

        public ComponentMetaDataGroupDTO[] AddComponent(string jobId, string wellId, string assemblyId, string subassemblyId, long eventId, int topdepth = 0, int bottomdepth = 500)
        {
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);

            //Add Components
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");
            foreach (RRLPartTypeComponentGroupingTypeDTO cg in partTypes)
            {
                Assert.AreEqual("Surface Equipment", cg.strComponentGrouping);
            }

            //Meta Data
            //9/3/2019 - To avoid compilation error, passed an added parameter as null. Integration test needs to be fixed according to FRWM-5489
            //string hardCodedComponentGroupId = null;
            ComponentMetaDataGroupDTO[] cmpMetaData = ComponentService.GetComponentMetaDataForAdd(partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType.ToString(), partfilter.TypeId.ToString());
            Assert.IsNotNull(cmpMetaData);
            MetaDataDTO[] cdReference = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;

            //Get Meta data for the Catalog Item description
            MetaDataReferenceData cd = new MetaDataReferenceData();
            cd.MetaData = cdReference.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCatalogItem");
            MetaDataFilterDTO cdFilter = new MetaDataFilterDTO();
            cdFilter.ColumnValue = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType.ToString();
            cdFilter.MetaDataFilterToken = cd.MetaData.ExtendedFilterInput;
            List<MetaDataFilterDTO> listcdFilter = new List<MetaDataFilterDTO>();
            listcdFilter.Add(cdFilter);
            cd.UIFilterValues = listcdFilter.ToArray();
            ControlIdTextDTO[] cdMetaData = JobAndEventService.GetMetaDataReferenceData(cd);
            Assert.AreEqual(1, cdMetaData.Count());
            ControlIdTextDTO cdm = cdMetaData.FirstOrDefault(x => x.ControlText == "Transformer");

            //Get Meta data for the Component Grouping
            cd.MetaData = cdReference.FirstOrDefault(x => x.ColumnName == "ascFK_c_ComponentGrouping");
            cd.UIFilterValues = null;
            ControlIdTextDTO[] cgMetaData = JobAndEventService.GetMetaDataReferenceData(cd);
            Assert.IsNotNull(cgMetaData);

            //Get Meta data for the Sub Assembly
            cd.MetaData = cdReference.FirstOrDefault(x => x.ColumnName == "ascFK_SubAssembly");
            SubAssemblyDTO[] getSubAssemblies = WellboreComponentService.GetSubAssembliesByWellId(wellId);
            Assert.IsNotNull(getSubAssemblies);
            MetaDataFilterDTO saFilter = new MetaDataFilterDTO();
            cdFilter.ColumnValue = assemblyId;
            cdFilter.MetaDataFilterToken = cd.MetaData.ExtendedFilterInput;
            List<MetaDataFilterDTO> listsaFilter = new List<MetaDataFilterDTO>();
            listsaFilter.Add(cdFilter);
            cd.UIFilterValues = listsaFilter.ToArray();
            ControlIdTextDTO[] saMetaData = JobAndEventService.GetMetaDataReferenceData(cd);
            Assert.IsNotNull(saMetaData);
            Assert.AreEqual(getSubAssemblies.Count(), saMetaData.Count());

            ComponentMetaDataGroupDTO reqComponent = new ComponentMetaDataGroupDTO();
            reqComponent.CategoryName = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").CategoryName;
            reqComponent.JobId = Convert.ToInt64(jobId);
            reqComponent.EventId = eventId;
            reqComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
            reqComponent.ExtendedAssemblyComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptyExtendedAssemblyComponentTableName;
            reqComponent.ExtendedComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptyExtendedComponentTableName;
            reqComponent.Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCatalogItem").DataValue = cdm.ControlId;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcCompName").DataValue = "Transformer";
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_Manufacturers").DataValue = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_PartType").DataValue = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcInnerDiameter").DataValue = 1.5m;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcOuterDiameter").DataValue = 2.5m;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Assembly").DataValue = assemblyId;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascTopDepth").DataValue = topdepth;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascBottomDepth").DataValue = bottomdepth;
            //reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascInstallDate").DataValue = getJob.BeginDate.ToString();
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascEndEventDT").DataValue = getJob.EndDate.AddDays(50).ToString();
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_SubAssembly").DataValue = subassemblyId;
            reqComponent.PartTypePrimaryKey = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;
            reqComponent.Order = 1;
            reqComponent.PartType = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptyPartType;
            ComponentMetaDataGroupDTO addComponent = new ComponentMetaDataGroupDTO();
            addComponent.CategoryName = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Additional").CategoryName;
            addComponent.JobId = Convert.ToInt64(jobId);
            addComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
            addComponent.ExtendedAssemblyComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptyExtendedAssemblyComponentTableName;
            addComponent.ExtendedComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptyExtendedComponentTableName;
            addComponent.Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Additional").Fields;
            addComponent.PartTypePrimaryKey = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;
            addComponent.Order = 1;
            addComponent.PartType = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptyPartType;
            addComponent.Fields.FirstOrDefault(x => x.ColumnName.ToUpper() == "ASCINSTALLDATE").DataValue = getJob.EndDate.ToString();
            List<ComponentMetaDataGroupDTO> listComponent = new List<ComponentMetaDataGroupDTO>();
            listComponent.Add(reqComponent);
            listComponent.Add(addComponent);
            ComponentMetaDataGroupDTO[] arrComponent = listComponent.ToArray();

            return arrComponent;
        }


        public ComponentMetaDataGroupDTO[] AddComponent_Generic(string jobId, string wellId, string assemblyId, string subassemblyId, long eventId, string compgroup, string parttype, int ctrlid, double ID, double OD, int topdepth = 0, int bottomdepth = 500)
        {
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);

            //Add Components
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compgroup).ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");
            foreach (RRLPartTypeComponentGroupingTypeDTO cg in partTypes)
            {
                Assert.AreEqual(compgroup, cg.strComponentGrouping);
            }

            //Meta Data
            //string hardCodedComponentGroupId = "test";
            ComponentMetaDataGroupDTO[] cmpMetaData = ComponentService.GetComponentMetaDataForAdd(partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType.ToString(), partfilter.TypeId.ToString());
            Assert.IsNotNull(cmpMetaData);
            MetaDataDTO[] cdReference = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;

            //ComponentMetaDataGroupDTO[] cmpMetaData = ComponentService.GetComponentMetaDataForAddByPartTypeId(partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType.ToString());
            //Assert.IsNotNull(cmpMetaData);
            //MetaDataDTO[] cdReference = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;

            //Get Meta data for the Catalog Item description
            MetaDataReferenceData cd = new MetaDataReferenceData();
            cd.MetaData = cdReference.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCatalogItem");
            MetaDataFilterDTO cdFilter = new MetaDataFilterDTO();
            cdFilter.ColumnValue = partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType.ToString();
            cdFilter.MetaDataFilterToken = cd.MetaData.ExtendedFilterInput;
            List<MetaDataFilterDTO> listcdFilter = new List<MetaDataFilterDTO>();
            listcdFilter.Add(cdFilter);
            cd.UIFilterValues = listcdFilter.ToArray();
            ControlIdTextDTO[] cdMetaData = JobAndEventService.GetMetaDataReferenceData(cd);

            ControlIdTextDTO cdm = cdMetaData.FirstOrDefault(x => x.ControlId == ctrlid);
            string catalogname = cdm.ControlText;
            //Get Meta data for the Component Grouping
            cd.MetaData = cdReference.FirstOrDefault(x => x.ColumnName == "ascFK_c_ComponentGrouping");
            cd.UIFilterValues = null;
            ControlIdTextDTO[] cgMetaData = JobAndEventService.GetMetaDataReferenceData(cd);
            Assert.IsNotNull(cgMetaData);

            //Get Meta data for the Sub Assembly
            cd.MetaData = cdReference.FirstOrDefault(x => x.ColumnName == "ascFK_SubAssembly");
            SubAssemblyDTO[] getSubAssemblies = WellboreComponentService.GetSubAssembliesByWellId(wellId);
            Assert.IsNotNull(getSubAssemblies);
            MetaDataFilterDTO saFilter = new MetaDataFilterDTO();
            cdFilter.ColumnValue = assemblyId;
            cdFilter.MetaDataFilterToken = cd.MetaData.ExtendedFilterInput;
            List<MetaDataFilterDTO> listsaFilter = new List<MetaDataFilterDTO>();
            listsaFilter.Add(cdFilter);
            cd.UIFilterValues = listsaFilter.ToArray();
            ControlIdTextDTO[] saMetaData = JobAndEventService.GetMetaDataReferenceData(cd);
            Assert.IsNotNull(saMetaData);
            Assert.AreEqual(getSubAssemblies.Count(), saMetaData.Count());

            ComponentMetaDataGroupDTO reqComponent = new ComponentMetaDataGroupDTO();
            reqComponent.CategoryName = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").CategoryName;
            reqComponent.JobId = Convert.ToInt64(jobId);
            reqComponent.EventId = eventId;
            reqComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
            reqComponent.ExtendedAssemblyComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compgroup).ptyExtendedAssemblyComponentTableName;
            reqComponent.ExtendedComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compgroup).ptyExtendedComponentTableName;
            reqComponent.Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCatalogItem").DataValue = cdm.ControlId;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcCompName").DataValue = parttype;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_Manufacturers").DataValue = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_PartType").DataValue = partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcInnerDiameter").DataValue = ID;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcOuterDiameter").DataValue = OD;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Assembly").DataValue = assemblyId;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascTopDepth").DataValue = topdepth;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascBottomDepth").DataValue = bottomdepth;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascEndEventDT").DataValue = getJob.EndDate.AddDays(50).ToString();
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_SubAssembly").DataValue = subassemblyId;
            reqComponent.PartTypePrimaryKey = partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType;
            reqComponent.Order = 1;
            reqComponent.PartType = partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptyPartType;
            ComponentMetaDataGroupDTO addComponent = new ComponentMetaDataGroupDTO();
            addComponent.CategoryName = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Additional").CategoryName;
            addComponent.JobId = Convert.ToInt64(jobId);
            addComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
            addComponent.ExtendedAssemblyComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compgroup).ptyExtendedAssemblyComponentTableName;
            addComponent.ExtendedComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compgroup).ptyExtendedComponentTableName;
            addComponent.Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Additional").Fields;
            addComponent.PartTypePrimaryKey = partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType;
            addComponent.Order = 1;
            addComponent.PartType = partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptyPartType;
            List<ComponentMetaDataGroupDTO> listComponent = new List<ComponentMetaDataGroupDTO>();
            listComponent.Add(reqComponent);
            listComponent.Add(addComponent);
            ComponentMetaDataGroupDTO[] arrComponent = listComponent.ToArray();

            return arrComponent;
        }




        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void GetDatesGetHistoryYearsRemoveHistory()
        {
            WellConfigDTO well = AddWell("RPOC_");
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.WellBoreReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");
            ComponentMetaDataGroupDTO[] arrComponent = AddComponent(jobId, wellId, assemblyId, subassemblyId, check);
            ComponentPartTypeDTO details = new ComponentPartTypeDTO();
            details.JobId = Convert.ToInt64(jobId);
            details.ComponentId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptgFK_c_MfgCat_ComponentGrouping;
            details.PartTypeId = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;
            ComponentMetaDataGroupBatchCollectionDTO batchDetailsComp = new ComponentMetaDataGroupBatchCollectionDTO();
            batchDetailsComp.ActionPerformed = CRUDOperationTypes.Add;
            batchDetailsComp.ComponentMetadataCollection = arrComponent;
            batchDetailsComp.ComponentPartType = details;
            List<ComponentMetaDataGroupBatchCollectionDTO> listComp = new List<ComponentMetaDataGroupBatchCollectionDTO>();
            listComp.Add(batchDetailsComp);
            ComponentMetaDataGroupBatchCollectionDTO[] arrComp = listComp.ToArray();
            //Add Batch Component
            bool saveBatch = ComponentService.SaveWellboreComponent(arrComp);
            Assert.IsTrue(saveBatch);
            //Get Component
            WellboreGridDTO[] wellboreComponent = wellboreComponent = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());
            Assert.IsNotNull(wellboreComponent);
            Assert.AreEqual(1, wellboreComponent.Count());

            //Testing the wellbore history with the new implementation as per WSM(FSM)
            HistoryYearDTO years = WellboreComponentService.GetHistoryYears(wellId);
            Assert.AreEqual(getJob.EndDate.Year.ToString(), years.years.FirstOrDefault());
            Assert.AreEqual(1, years.years.Count);
            int componentCount = 0;
            //Get Dates
            for (int i = years.years.Count - 1; i >= 0; i--)
            {
                HistoryDatesDTO DatesbyYear = WellboreComponentService.GetDates(years.years[i], wellId);
                Assert.IsNotNull(DatesbyYear);

                string[] dates = DatesbyYear.Dates.ToArray();
                foreach (string date in dates)
                {
                    //Get All Components by Date
                    HistoryDataDTO allComponents = WellboreComponentService.GetAllComponent(date, wellId);
                    AssemblyComponentDTO[] assemblyComponents = allComponents.HistoryData.ToArray();
                    foreach (AssemblyComponentDTO assemblyComponent in assemblyComponents)
                    {
                        if (assemblyComponent.IsFailed == true)
                        {
                            componentCount = componentCount + 1;
                        }
                    }
                }
            }
            Assert.AreEqual(0, componentCount, "Failure count cannot be greater than zero without adding a failure");
            //Remove History by date
            for (int i = years.years.Count - 1; i >= 0; i--)
            {
                HistoryDatesDTO DatesbyYear = WellboreComponentService.GetDates(years.years[i], wellId);
                string[] dates = DatesbyYear.Dates.ToArray();
                // Remove Component History by Date
                foreach (string date in dates)
                {
                    WellboreComponentService.RemoveComponentHistoryByDate(date, wellId);
                    HistoryDataDTO afterRemove = WellboreComponentService.GetAllComponent(date, wellId);
                    AssemblyComponentDTO[] assemblyComponents = afterRemove.HistoryData.ToArray();
                    Assert.AreEqual(0, assemblyComponents.Count());
                }
            }
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void SaveComponent()
        {
            try
            {
                WellConfigDTO well = AddWell("RPOC_");
                string wellId = well.Well.Id.ToString();
                string assemblyId = well.Well.AssemblyId.ToString();
                string subassemblyId = well.Well.SubAssemblyId.ToString();
                string jobId = AddJob("Approved");
                JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
                //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.WellBoreReport);
                //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
                JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
                EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
                long check = JobAndEventService.AddEventForJobEventType(evt);
                Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
                ComponentFilterDTO groupfilter = new ComponentFilterDTO();
                groupfilter.ContainsSearchText = "";
                groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
                RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
                Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");
                ComponentFilterDTO partfilter = new ComponentFilterDTO();
                partfilter.ContainsSearchText = "";
                partfilter.TypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptgFK_c_MfgCat_ComponentGrouping;
                partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
                RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
                Assert.IsNotNull(partTypes, "Failed to retrieve Part types");
                ComponentMetaDataGroupDTO[] arrComponent = AddComponent(jobId, wellId, assemblyId, subassemblyId, check);
                ComponentPartTypeDTO details = new ComponentPartTypeDTO();
                details.JobId = Convert.ToInt64(jobId);
                details.ComponentId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptgFK_c_MfgCat_ComponentGrouping;
                details.PartTypeId = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;
                ComponentMetaDataGroupBatchCollectionDTO batchDetailsComp = new ComponentMetaDataGroupBatchCollectionDTO();
                batchDetailsComp.ActionPerformed = CRUDOperationTypes.Add;
                batchDetailsComp.ComponentMetadataCollection = arrComponent;
                batchDetailsComp.ComponentPartType = details;
                List<ComponentMetaDataGroupBatchCollectionDTO> listComp = new List<ComponentMetaDataGroupBatchCollectionDTO>();
                listComp.Add(batchDetailsComp);
                ComponentMetaDataGroupBatchCollectionDTO[] arrComp = listComp.ToArray();
                //Add Batch Component
                bool saveBatch = ComponentService.SaveWellboreComponent(arrComp);
                Assert.IsTrue(saveBatch);
                //Get Component
                WellboreGridDTO[] wellboreComponent = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());
                Assert.IsNotNull(wellboreComponent);
                Assert.AreEqual(1, wellboreComponent.Count());

                //Remove Component
                ComponentPartTypeDTO Ids = new ComponentPartTypeDTO();
                Ids.JobId = getJob.JobId;
                Ids.PartTypeId = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;

                foreach (WellboreGridGroupDTO comp in wellboreComponent.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup)
                {
                    ComponentMetaDataGroupDTO mdComp = comp.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required");
                    Assert.IsNotNull(mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Event_Beginning").DataValue);
                    Assert.IsNull(mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Event_Ending").DataValue);
                    var ascId = mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascPrimaryKey").DataValue;
                    Ids.AssemblyComponentId = Convert.ToInt64(ascId);
                    Ids.ComponentId = mdComp.ComponentPrimaryKey;
                    Ids.EventId = check;
                    bool rComp = ComponentService.RemoveComponent(Ids);
                    Assert.IsTrue(rComp);
                }

                wellboreComponent = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());
                Assert.IsNotNull(wellboreComponent);
                Assert.AreEqual(0, wellboreComponent.Count());
                // Remove Event and its extended information
                evt.Id = check;
                bool rcheck = JobAndEventService.RemoveEvent(evt);
                Assert.IsTrue(rcheck, "Failed to remove Economic Analysis report");
            }
            catch (Exception e)
            {
                Trace.WriteLine("=========Scanning Services running=========");
                ServiceController[] services = ServiceController.GetServices();

                foreach (ServiceController service in services)
                {
                    Trace.WriteLine(service.ServiceName + " service is currently " + service.Status + " .");
                }
                Trace.WriteLine("=========Scanning End here=========");

                Assert.Fail("Some exception occured Details " + e);

            }
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void GetWellboreComponentsByComponentIdsVerification()
        {
            /*-----------------------------------------------------------------------------------------------------------------------------
            1	Source: Destination = Same well, Same Job  => Copy multiple components from a well and paste them to same well and same job
                Here TargetJobId, TargetAssemblyId and TargetSubAssemblyId will be identical
            2	Source: Destination = Same well, Different Job  => Copy multiple components from well and paste it to same well, different job
                Here TargetJobId will be different from SourceJobId, while TargetAssemblyId and TargetSubAssemblyId will be identical
            3	Source: Destination = Different well, Different Job  =>Copy multiple components from well and paste it to different well
                Here all 3 id will be different
             ------------------------------------------------------------------------------------------------------------------------------*/

            string facilityId = s_isRunningInATS ? "RPOC_" + "00001" : "RPOC_" + "0001";
            WellDTO wellDTO = new WellDTO
            {
                Name = "W1",
                FacilityId = facilityId,
                DataConnection = GetDefaultCygNetDataConnection(),
                IntervalAPI = "IntervalAPI",
                SubAssemblyAPI = "SubAssemblyAPI",
                AssemblyAPI = "AssemblyAPI ",
                CommissionDate = DateTime.Today,
                WellType = WellTypeId.RRL,
            };

            WellConfigDTO well = AddWellWithDefinedConfiguration(wellDTO);
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();
            string jobId = AddJobWithUserDefinedConfiguration("Approved", "W1");
            string targetJobId = AddJobWithUserDefinedConfiguration("Planned", "W1", 20);

            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");
            ComponentMetaDataGroupDTO[] arrComponent = AddComponent(jobId, wellId, assemblyId, subassemblyId, check);
            ComponentPartTypeDTO details = new ComponentPartTypeDTO();
            details.JobId = Convert.ToInt64(jobId);
            details.ComponentId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptgFK_c_MfgCat_ComponentGrouping;
            details.PartTypeId = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;
            ComponentMetaDataGroupBatchCollectionDTO batchDetailsComp = new ComponentMetaDataGroupBatchCollectionDTO();
            batchDetailsComp.ActionPerformed = CRUDOperationTypes.Add;
            batchDetailsComp.ComponentMetadataCollection = arrComponent;
            batchDetailsComp.ComponentPartType = details;
            List<ComponentMetaDataGroupBatchCollectionDTO> listComp = new List<ComponentMetaDataGroupBatchCollectionDTO>();
            listComp.Add(batchDetailsComp);
            ComponentMetaDataGroupBatchCollectionDTO[] arrComp = listComp.ToArray();
            //Add Batch Component
            bool saveBatch = ComponentService.SaveWellboreComponent(arrComp);
            Assert.IsTrue(saveBatch);

            //Get Component
            WellboreGridDTO[] wellboreComponent = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());
            Assert.IsNotNull(wellboreComponent);
            Assert.AreEqual(1, wellboreComponent.Count());

            ComponentPartTypeDTO Ids = new ComponentPartTypeDTO();
            Ids.JobId = getJob.JobId;
            Ids.PartTypeId = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;

            //Different Well
            string facilityId1 = s_isRunningInATS ? "RPOC-1_" + "00001" : "RPOC-1_" + "0001";
            WellDTO wellDTO1 = new WellDTO
            {
                Name = "W2",
                FacilityId = facilityId1,
                DataConnection = GetDefaultCygNetDataConnection(),
                IntervalAPI = "IntervalAPI1",
                SubAssemblyAPI = "SubAssemblyAPI1",
                AssemblyAPI = "AssemblyAPI1 ",
                CommissionDate = DateTime.Today,
                WellType = WellTypeId.RRL,
            };

            WellConfigDTO well1 = AddWellWithDefinedConfiguration(wellDTO1);
            string wellId1 = well1.Well.Id.ToString();
            string assemblyId1 = well1.Well.AssemblyId.ToString();
            string subassemblyId1 = well1.Well.SubAssemblyId.ToString();
            string jobId1 = AddJobWithUserDefinedConfiguration("Approved", "W2");

            foreach (WellboreGridGroupDTO comp in wellboreComponent.First().WellboreGridGroup)
            {
                ComponentMetaDataGroupDTO mdComp = comp.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required");
                Assert.IsNotNull(mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Event_Beginning").DataValue);
                Assert.IsNull(mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Event_Ending").DataValue);
                var ascId = mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascPrimaryKey").DataValue;
                Ids.AssemblyComponentId = Convert.ToInt64(ascId);
                Ids.ComponentId = mdComp.ComponentPrimaryKey;
                Ids.EventId = check;

                //Copy details
                WellboreReportCopyDTO copiedWellboreReport = new WellboreReportCopyDTO();
                copiedWellboreReport.ComponentId = mdComp.ComponentPrimaryKey;
                copiedWellboreReport.AssemblyComponentId = Convert.ToInt64(ascId);
                List<WellboreReportCopyDTO> listSourceWelBoreReports = new List<WellboreReportCopyDTO>();
                listSourceWelBoreReports.Add(copiedWellboreReport);
                WellboreReportCopyDTO[] WellboreReportCopyComponents = listSourceWelBoreReports.ToArray();

                //Source [Copy] and Destination [Paste] are for same well and same job
                WellboreReportPasteDTO pasteWellboreReport = new WellboreReportPasteDTO();
                pasteWellboreReport.TargetJobId = mdComp.ComponentPrimaryKey;
                pasteWellboreReport.TargetAssemblyId = Convert.ToInt64(ascId);
                pasteWellboreReport.TargetSubAssemblyId = Convert.ToInt64(subassemblyId);
                pasteWellboreReport.WellboreReportCopyComponents = WellboreReportCopyComponents;

                WellboreGridDTO[] wellboreGridDTO = ComponentService.GetWellboreComponentsByComponentIds(pasteWellboreReport);
                Assert.IsNotNull(wellboreGridDTO);
                Assert.AreEqual(1, wellboreGridDTO.Count());

                Assert.AreEqual(pasteWellboreReport.TargetSubAssemblyId, wellboreGridDTO[0].SubAssemblyId, "Incorrect value of SubAssemblyId");

                //Source [Copy] and Destination [Paste] are for same well and different job
                WellboreReportPasteDTO pasteWellboreReport1 = new WellboreReportPasteDTO();
                pasteWellboreReport1.TargetJobId = Convert.ToInt64(targetJobId);
                pasteWellboreReport1.TargetAssemblyId = Convert.ToInt64(ascId);
                pasteWellboreReport1.TargetSubAssemblyId = Convert.ToInt64(subassemblyId);
                pasteWellboreReport1.WellboreReportCopyComponents = WellboreReportCopyComponents;

                WellboreGridDTO[] wellboreGridDTO1 = ComponentService.GetWellboreComponentsByComponentIds(pasteWellboreReport1);
                Assert.IsNotNull(wellboreGridDTO1);
                Assert.AreEqual(1, wellboreGridDTO1.Count());
                Assert.AreEqual(pasteWellboreReport1.TargetSubAssemblyId, wellboreGridDTO[0].SubAssemblyId, "Incorrect value of SubAssemblyId");

                //Source [Copy] and Destination [Paste] are for different well and different job
                WellboreReportPasteDTO pasteWellboreReport2 = new WellboreReportPasteDTO();
                pasteWellboreReport2.TargetJobId = Convert.ToInt64(jobId1);
                pasteWellboreReport2.TargetAssemblyId = Convert.ToInt64(assemblyId1);
                pasteWellboreReport2.TargetSubAssemblyId = Convert.ToInt64(subassemblyId1);
                pasteWellboreReport2.WellboreReportCopyComponents = WellboreReportCopyComponents;

                WellboreGridDTO[] wellboreGridDTO2 = ComponentService.GetWellboreComponentsByComponentIds(pasteWellboreReport2);
                Assert.IsNotNull(wellboreGridDTO2);
                Assert.AreEqual(1, wellboreGridDTO2.Count());
                Assert.AreNotEqual(pasteWellboreReport2.TargetSubAssemblyId, wellboreGridDTO2[0].SubAssemblyId, "Incorrect value of SubAssemblyId");

                //Remove Component
                bool rComp = ComponentService.RemoveComponent(Ids);
                Assert.IsTrue(rComp);
            }

            //Remove Job
            bool removeJob = JobAndEventService.RemoveJob(jobId);
            Assert.IsTrue(removeJob, "Failed to remove Job");
            removeJob = JobAndEventService.RemoveJob(jobId1);
            Assert.IsTrue(removeJob, "Failed to remove Job");
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void FailureobservationCRUD()
        {
            WellConfigDTO well = AddWell("RPOC_");
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO wbevt = SetEventDTO(getJob, (int)JobEventTypes.WellBoreReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO wbevt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long wbcheck = JobAndEventService.AddEventForJobEventType(wbevt);
            Assert.IsTrue(wbcheck > 0, "Failed to add Wellbore Report");
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.FailureReport);
            var check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Failure Report");
            //Add Component
            ComponentMetaDataGroupDTO[] arrComponent = AddComponent(jobId, wellId, assemblyId, subassemblyId, wbcheck);
            bool addComp = ComponentService.AddComponent(arrComponent);
            Assert.IsTrue(addComp);

            string failCompJobId = AddJob("Approved", 31);

            //Get Failure Component
            JobComponentFailureDTO[] Comps = JobAndEventService.GetJobComponentFailure(failCompJobId);
            Assert.IsNotNull(Comps, "Failed to get components");
            Assert.AreEqual(1, Comps.Count());

            //Get Metadata for observation
            JobComponentFailureObservationDTO mdFailObserv = JobAndEventService.GetMetaDataForAddObservation(Comps.FirstOrDefault().AssemblyComponentId.ToString(), jobId);
            Assert.IsNotNull(mdFailObserv, "Failed to get Observation for the added component");
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue = false;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFailureDate").DataValue = DateTime.Today.ToString();
            //Add observation
            string failObservationCompId = JobAndEventService.AddObservation(mdFailObserv);
            Assert.IsNotNull(failObservationCompId);

            //Get observations
            foreach (JobComponentFailureDTO comp in Comps)
            {
                JobComponentFailureObservationDTO[] failObservationComp = JobAndEventService.GetObservationsByAssemblyComponentId(comp.AssemblyComponentId.ToString(), jobId);
                Assert.AreEqual(mdFailObserv.AssemblyComponentId, failObservationComp.FirstOrDefault().AssemblyComponentId);
                Assert.AreEqual(mdFailObserv.EventId, failObservationComp.FirstOrDefault().EventId);
                Assert.AreEqual(mdFailObserv.JobId, failObservationComp.FirstOrDefault().JobId);
                Assert.AreEqual(mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue, failObservationComp.FirstOrDefault().ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue);
                Assert.IsNotNull(failObservationComp.FirstOrDefault().Title);
                Assert.AreEqual(DateTime.Today.ToString("MM/dd/yyyy"), failObservationComp.FirstOrDefault().Title);
            }

            //Update observations (making the added observation as a primary cause of failure)
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue = true;
            //Get Meta data for the PrimaryFailureClass
            MetaDataReferenceData cd = new MetaDataReferenceData();
            cd.MetaData = mdFailObserv.PrimaryCauseFailures.FirstOrDefault(x => x.ColumnName == "wsyFK_r_PrimaryFailureClass");
            cd.UIFilterValues = null;
            ControlIdTextDTO[] cdMetaData = JobAndEventService.GetMetaDataReferenceData(cd);
            Assert.IsNotNull(cdMetaData);
            Assert.AreEqual(9, cdMetaData.Count());
            mdFailObserv.PrimaryCauseFailures.FirstOrDefault(x => x.ColumnName == "wsyFK_r_PrimaryFailureClass").DataValue = cdMetaData.FirstOrDefault().ControlId;
            //Get Meta data for the FailureSolution
            cd.MetaData = mdFailObserv.PrimaryCauseFailures.FirstOrDefault(x => x.ColumnName == "wsyFK_r_FailureSolution");
            cdMetaData = JobAndEventService.GetMetaDataReferenceData(cd);
            Assert.IsNotNull(cdMetaData);
            Assert.AreEqual(42, cdMetaData.Count());
            mdFailObserv.PrimaryCauseFailures.FirstOrDefault(x => x.ColumnName == "wsyFK_r_FailureSolution").DataValue = cdMetaData.FirstOrDefault().ControlId;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryKey").DataValue = failObservationCompId;
            mdFailObserv.AssemblyComponentFailureId = Convert.ToInt64(failObservationCompId);

            bool updatefailObservationCompId = JobAndEventService.UpdateObservation(mdFailObserv);
            Assert.IsTrue(updatefailObservationCompId);

            //Get observations
            foreach (JobComponentFailureDTO comp in Comps)
            {
                JobComponentFailureObservationDTO[] failObservationComp = JobAndEventService.GetObservationsByAssemblyComponentId(comp.AssemblyComponentId.ToString(), jobId);
                Assert.AreEqual(mdFailObserv.AssemblyComponentId, failObservationComp.FirstOrDefault().AssemblyComponentId);
                Assert.AreEqual(mdFailObserv.EventId, failObservationComp.FirstOrDefault().EventId);
                Assert.AreEqual(mdFailObserv.JobId, failObservationComp.FirstOrDefault().JobId);
                Assert.AreEqual(mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue, failObservationComp.FirstOrDefault().ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue);
                Assert.IsNotNull(failObservationComp.FirstOrDefault().Title);
            }

            //Update observation (remove the primary cause)
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue = false;
            updatefailObservationCompId = JobAndEventService.UpdateObservation(mdFailObserv);
            //Get observations
            foreach (JobComponentFailureDTO comp in Comps)
            {
                JobComponentFailureObservationDTO[] failObservationComp = JobAndEventService.GetObservationsByAssemblyComponentId(comp.AssemblyComponentId.ToString(), jobId);
                Assert.AreEqual(mdFailObserv.AssemblyComponentId, failObservationComp.FirstOrDefault().AssemblyComponentId);
                Assert.AreEqual(mdFailObserv.EventId, failObservationComp.FirstOrDefault().EventId);
                Assert.AreEqual(mdFailObserv.JobId, failObservationComp.FirstOrDefault().JobId);
                Assert.AreEqual(mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue, failObservationComp.FirstOrDefault().ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue);
                Assert.IsNotNull(failObservationComp.FirstOrDefault().Title);
            }

            //Remove observation
            bool removeObservation = JobAndEventService.RemoveObservation(failObservationCompId);
            Assert.IsTrue(removeObservation);
            //Get observations
            foreach (JobComponentFailureDTO comp in Comps)
            {
                JobComponentFailureObservationDTO[] failObservationComp = JobAndEventService.GetObservationsByAssemblyComponentId(comp.AssemblyComponentId.ToString(), jobId);
                Assert.AreEqual(0, failObservationComp.Count());
            }

            WellboreGridDTO[] wellboreComponents = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, wbcheck.ToString());
            Assert.IsNotNull(wellboreComponents);
            Assert.AreEqual(1, wellboreComponents.Count());
            //Remove Component
            ComponentPartTypeDTO Ids = new ComponentPartTypeDTO();
            Ids.JobId = getJob.JobId;

            foreach (WellboreGridGroupDTO comp in wellboreComponents.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup)
            {
                ComponentMetaDataGroupDTO mdComp = comp.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required");
                Assert.IsNotNull(mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Event_Beginning").DataValue);
                Assert.IsNull(mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Event_Ending").DataValue);
                var ascId = mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascPrimaryKey").DataValue;
                Ids.AssemblyComponentId = Convert.ToInt64(ascId);
                Ids.ComponentId = mdComp.ComponentPrimaryKey;
                Ids.PartTypeId = (int)mdComp.PartTypePrimaryKey;
                Ids.EventId = wbcheck;
                bool rComp = ComponentService.RemoveComponent(Ids);
                Assert.IsTrue(rComp);
            }

            wellboreComponents = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, wbcheck.ToString());
            Assert.IsNotNull(wellboreComponents);
            Assert.AreEqual(0, wellboreComponents.Count());
            // Remove Event and its extended information
            evt.Id = check;
            bool rcheck = JobAndEventService.RemoveEvent(evt);
            Assert.IsTrue(rcheck, "Failed to remove Failure report");
            wbevt.Id = wbcheck;
            bool wbrcheck = JobAndEventService.RemoveEvent(wbevt);
            Assert.IsTrue(rcheck, "Failed to remove Wellbore report");
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void VerticalSchematic()
        {
            WellConfigDTO well = AddWell("RPOC_");
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.WellBoreReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
            //Empty Schematic
            ComponentVerticalSchematicInputDTO inputSchematic = new ComponentVerticalSchematicInputDTO();
            inputSchematic.AssemblyId = Convert.ToInt64(assemblyId);
            inputSchematic.SubAssemblyId = Convert.ToInt64(subassemblyId);
            inputSchematic.EndDate = getJob.EndDate.ToLocalTime();
            inputSchematic.EventId = check;
            inputSchematic.JobId = Convert.ToInt64(jobId);
            ComponentVerticalSchematicDTO[] componentSchematic = ComponentService.GetComponentVerticalSchematic(inputSchematic);
            Assert.AreEqual(0, componentSchematic.Count());
            //Add Component
            ComponentMetaDataGroupDTO[] arrComponent = AddComponent(jobId, wellId, assemblyId, subassemblyId, check);
            bool addComp = ComponentService.AddComponent(arrComponent);
            Assert.IsTrue(addComp);
            //Get Component
            WellboreGridDTO[] wellboreComponent = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());
            Assert.IsNotNull(wellboreComponent);
            Assert.AreEqual(1, wellboreComponent.Count());
            Assert.AreEqual(1, wellboreComponent.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup.Count());
            //Single Component Schematic
            componentSchematic = ComponentService.GetComponentVerticalSchematic(inputSchematic);
            Assert.AreEqual(1, componentSchematic.Count(), "mismatch between the added Component & Component Schematic");
            Assert.AreEqual("well head", componentSchematic[0].PartTypeName, "mismatch between the added Component & Component Schematic");
            Assert.AreEqual("Transformer", componentSchematic[0].DescriptionAndGeometry.Description, "mismatch between the added Component & Component Schematic");
            Assert.AreEqual(0, componentSchematic[0].DescriptionAndGeometry.Geometry.Depth.TopDepth, "mismatch between the added Component & Component Schematic");
            Assert.AreEqual(500, componentSchematic[0].DescriptionAndGeometry.Geometry.Depth.BottomDepth, "mismatch between the added Component & Component Schematic");
            //Remove Component
            ComponentPartTypeDTO Ids = new ComponentPartTypeDTO();
            Ids.JobId = getJob.JobId;
            Ids.PartTypeId = (int)arrComponent.FirstOrDefault().PartTypePrimaryKey;

            foreach (WellboreGridGroupDTO comp in wellboreComponent.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup)
            {
                ComponentMetaDataGroupDTO mdComp = comp.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required");
                Assert.IsNotNull(mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Event_Beginning").DataValue);
                Assert.IsNull(mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Event_Ending").DataValue);
                var ascId = mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascPrimaryKey").DataValue;
                Ids.AssemblyComponentId = Convert.ToInt64(ascId);
                Ids.ComponentId = mdComp.ComponentPrimaryKey;
                Ids.EventId = check;
                bool rComp = ComponentService.RemoveComponent(Ids);
                Assert.IsTrue(rComp);
            }
            wellboreComponent = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());
            Assert.IsNotNull(wellboreComponent);
            Assert.AreEqual(0, wellboreComponent.Count());
            // Remove Event and its extended information
            evt.Id = check;
            bool rcheck = JobAndEventService.RemoveEvent(evt);
            Assert.IsTrue(rcheck, "Failed to remove Economic Analysis report");
        }


        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void MultipleCasings()
        {
            WellConfigDTO well = AddWell("RPOC_");
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.WellBoreReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
            //Empty Schematic
            ComponentVerticalSchematicInputDTO inputSchematic = new ComponentVerticalSchematicInputDTO();
            inputSchematic.AssemblyId = Convert.ToInt64(assemblyId);
            inputSchematic.SubAssemblyId = Convert.ToInt64(subassemblyId);
            inputSchematic.EndDate = getJob.EndDate.ToLocalTime();
            inputSchematic.EventId = check;
            inputSchematic.JobId = Convert.ToInt64(jobId);
            ComponentVerticalSchematicDTO[] componentSchematic = ComponentService.GetComponentVerticalSchematic(inputSchematic);
            Assert.AreEqual(0, componentSchematic.Count());
            //Add Component

            ComponentMetaDataGroupDTO[] arrComponent = AddComponent_Generic(jobId, wellId, assemblyId, subassemblyId, check, "Production Casing", "Casing/Casing Liner OD  2.063", 22642, 1.751, 2.063);
            ComponentMetaDataGroupDTO[] arrComponent1 = AddComponent_Generic(jobId, wellId, assemblyId, subassemblyId, check, "Production Casing", "Casing/Casing Liner OD  2.375", 448, 1.995, 2.375);
            ComponentMetaDataGroupDTO[] arrComponent2 = AddComponent_Generic(jobId, wellId, assemblyId, subassemblyId, check, "Tubing String", "Capillary Tubing", 6230, 0.375, 0.5);

            bool addComp = ComponentService.AddComponent(arrComponent);
            Assert.IsTrue(addComp);
            bool addComp1 = ComponentService.AddComponent(arrComponent1);
            Assert.IsTrue(addComp1);
            bool addComp2 = ComponentService.AddComponent(arrComponent2);
            Assert.IsTrue(addComp2);
            //Get Component
            WellboreGridDTO[] wellboreComponent = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());
            Assert.IsNotNull(wellboreComponent);
            Assert.AreEqual(1, wellboreComponent.Count());
            Assert.AreEqual(3, wellboreComponent.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup.Count());
            //Component Schematic
            componentSchematic = ComponentService.GetComponentVerticalSchematic(inputSchematic);
            Assert.AreEqual(3, componentSchematic.Count(), "mismatch between the added Component & Component Schematic");
            Assert.AreEqual("casing brush", componentSchematic[0].PartTypeName, "mismatch between the added Component & Component Schematic");
            Assert.AreEqual("casing brush", componentSchematic[1].PartTypeName, "mismatch between the added Component & Component Schematic");
            Assert.AreEqual("pony rod", componentSchematic[2].PartTypeName, "mismatch between the added Component & Component Schematic");

            Assert.AreEqual("Casing/Casing Liner OD  2.375", componentSchematic[0].DescriptionAndGeometry.Description, "mismatch between the added Component & Component Schematic");
            Assert.AreEqual("Casing/Casing Liner OD  2.063", componentSchematic[1].DescriptionAndGeometry.Description, "mismatch between the added Component & Component Schematic");
            Assert.AreEqual("Capillary Tubing", componentSchematic[2].DescriptionAndGeometry.Description, "mismatch between the added Component & Component Schematic");

            //Assert.AreEqual((6.366).ToString(), componentSchematic[0].DescriptionAndGeometry.Geometry.Diameter.InnerDiameter.ToString(), "mismatch between the added Component & Component Schematic");
            //Assert.AreEqual((7.623).ToString(), componentSchematic[1].DescriptionAndGeometry.Geometry.Diameter.InnerDiameter.ToString(), "mismatch between the added Component & Component Schematic");
            //Assert.AreEqual((0.375).ToString(), componentSchematic[2].DescriptionAndGeometry.Geometry.Diameter.InnerDiameter.ToString(), "mismatch between the added Component & Component Schematic");

            //Assert.AreEqual((6.366).ToString(), componentSchematic[0].DescriptionAndGeometry.Geometry.Diameter.InnerDiameter.ToString(), "mismatch between the added Component & Component Schematic");
            //Assert.AreEqual((7.623).ToString(), componentSchematic[1].DescriptionAndGeometry.Geometry.Diameter.InnerDiameter.ToString(), "mismatch between the added Component & Component Schematic");
            //Assert.AreEqual((0.375).ToString(), componentSchematic[2].DescriptionAndGeometry.Geometry.Diameter.InnerDiameter.ToString(), "mismatch between the added Component & Component Schematic");

            if (componentSchematic[0].DescriptionAndGeometry.Geometry.Diameter.OuterDiameter > componentSchematic[2].DescriptionAndGeometry.Geometry.Diameter.OuterDiameter &&
                componentSchematic[1].DescriptionAndGeometry.Geometry.Diameter.OuterDiameter > componentSchematic[2].DescriptionAndGeometry.Geometry.Diameter.OuterDiameter)
            {
                Trace.WriteLine("The Tubing string component is placed within multiple Production Casing components");
                Assert.IsTrue(true);
            }
            else
            {
                Assert.Fail();
            }


            //Remove Component
            ComponentPartTypeDTO Ids = new ComponentPartTypeDTO();
            Ids.JobId = getJob.JobId;
            Ids.PartTypeId = (int)arrComponent.FirstOrDefault().PartTypePrimaryKey;

            foreach (WellboreGridGroupDTO comp in wellboreComponent.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup)
            {
                ComponentMetaDataGroupDTO mdComp = comp.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required");
                Assert.IsNotNull(mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Event_Beginning").DataValue);
                Assert.IsNull(mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Event_Ending").DataValue);
                var ascId = mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascPrimaryKey").DataValue;
                Ids.AssemblyComponentId = Convert.ToInt64(ascId);
                Ids.ComponentId = mdComp.ComponentPrimaryKey;
                Ids.EventId = check;
                bool rComp = ComponentService.RemoveComponent(Ids);
                Assert.IsTrue(rComp);
            }
            wellboreComponent = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());
            Assert.IsNotNull(wellboreComponent);
            Assert.AreEqual(0, wellboreComponent.Count());
            // Remove Event and its extended information
            evt.Id = check;
            bool rcheck = JobAndEventService.RemoveEvent(evt);
            Assert.IsTrue(rcheck, "Failed to remove Economic Analysis report");
        }




        #region Wellbore Validations

        public ComponentMetaDataGroupDTO[] AddTubingString(string compGrouping, string partType, string wellId, string assemblyId, string jobId, decimal[] details, long eventId)
        {
            //Component Grouping
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compGrouping).ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");
            foreach (RRLPartTypeComponentGroupingTypeDTO cg in partTypes)
            {
                Assert.AreEqual(compGrouping, cg.strComponentGrouping);
            }

            //Meta data for the PartType that belongs to selected component group
            string ptypeId = partTypes.FirstOrDefault(x => x.ptyPartType == partType).ptgFK_c_MfgCat_PartType.ToString();
            //9/3/2019 - To avoid compilation error, passed an added parameter as null. Integration test needs to be fixed according to FRWM-5489
            //string hardCodedComponentGroupId = null;
            ComponentMetaDataGroupDTO[] cmpMetaData = ComponentService.GetComponentMetaDataForAdd(ptypeId, partfilter.TypeId.ToString());
            Assert.IsNotNull(cmpMetaData);
            MetaDataDTO[] reqFileds = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;
            MetaDataDTO[] addFileds = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Additional").Fields;
            //Manufacturer
            MetaDataDTO mdManufacturer = reqFileds.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_Manufacturers");
            ControlIdTextDTO[] listManufacturers = GetMetadataReferenceDataDDL(mdManufacturer, ptypeId);
            ControlIdTextDTO manufacturer = listManufacturers.FirstOrDefault(x => x.ControlId == details[7]);//Manufacturer
            //CatalogItem Description
            MetaDataDTO mdCatDescription = reqFileds.FirstOrDefault(x => x.Title == "Catalog Item Description");
            ControlIdTextDTO[] listCatDescription = GetMetadataReferenceDataDDL(mdCatDescription, ptypeId, manufacturer.ControlId.ToString());
            ControlIdTextDTO catDescription = listCatDescription.FirstOrDefault(x => x.ControlId == details[0]);//"J-55  2.875" OD/6.50#  2.441" ID  2.347" Drift"--27158
            //Assembly
            MetaDataDTO mdAssembly = reqFileds.FirstOrDefault(x => x.Title == "Assembly");
            ControlIdTextDTO[] listAssembly = GetMetadataReferenceDataDDL(mdAssembly, assemblyId);
            //SubAssembly
            MetaDataDTO mdsubAssembly = reqFileds.FirstOrDefault(x => x.ColumnName == "ascFK_SubAssembly");
            ControlIdTextDTO[] listsubAssembly = GetMetadataReferenceDataDDL(mdsubAssembly, assemblyId);
            //Required field Inputs
            reqFileds.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_Manufacturers").DataValue = manufacturer.ControlId;
            reqFileds.FirstOrDefault(x => x.Title == "Catalog Item Description").DataValue = catDescription.ControlId;
            reqFileds.FirstOrDefault(x => x.Title == "Component Name").DataValue = partType;
            reqFileds.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_PartType").DataValue = ptypeId;
            reqFileds.FirstOrDefault(x => x.Title == "Inner Diameter").DataValue = details[1];
            reqFileds.FirstOrDefault(x => x.Title == "Outer Diameter").DataValue = details[2];
            reqFileds.FirstOrDefault(x => x.ColumnName == "ascFK_SubAssembly").DataValue = listsubAssembly.FirstOrDefault().ControlId;
            reqFileds.FirstOrDefault(x => x.ColumnName == "cmcPartType").DataValue = partType;
            reqFileds.FirstOrDefault(x => x.Title == "Assembly").DataValue = listAssembly.FirstOrDefault().ControlId;
            reqFileds.FirstOrDefault(x => x.Title == "Component Grouping").DataValue = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compGrouping).ptgFK_c_MfgCat_ComponentGrouping;
            reqFileds.FirstOrDefault(x => x.Title == "Quantity").DataValue = details[3];
            reqFileds.FirstOrDefault(x => x.Title == "Length").DataValue = details[4];
            reqFileds.FirstOrDefault(x => x.Title == "Top Depth").DataValue = details[5];
            reqFileds.FirstOrDefault(x => x.Title == "Bottom Depth").DataValue = details[6];
            //Add Component
            ComponentMetaDataGroupDTO reqComponent = new ComponentMetaDataGroupDTO();
            reqComponent.CategoryName = "Required";
            reqComponent.JobId = Convert.ToInt64(jobId);
            reqComponent.EventId = eventId;
            reqComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
            reqComponent.ExtendedAssemblyComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compGrouping).ptyExtendedAssemblyComponentTableName;
            reqComponent.ExtendedComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compGrouping).ptyExtendedComponentTableName;
            reqComponent.Fields = reqFileds;
            reqComponent.Order = 1;
            reqComponent.PartType = partTypes.FirstOrDefault(x => x.ptyPartType == partType).ptyPartType;
            reqComponent.PartTypePrimaryKey = Convert.ToInt64(ptypeId);
            ComponentMetaDataGroupDTO addComponent = new ComponentMetaDataGroupDTO();
            addComponent.CategoryName = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Additional").CategoryName;
            addComponent.JobId = Convert.ToInt64(jobId);
            addComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
            addComponent.ExtendedAssemblyComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compGrouping).ptyExtendedAssemblyComponentTableName;
            addComponent.ExtendedComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compGrouping).ptyExtendedComponentTableName;
            addComponent.Fields = addFileds;
            addComponent.PartTypePrimaryKey = partTypes.FirstOrDefault(x => x.ptyPartType == partType).ptgFK_c_MfgCat_PartType;
            addComponent.Order = 1;
            addComponent.PartType = partTypes.FirstOrDefault(x => x.ptyPartType == partType).ptyPartType;
            List<ComponentMetaDataGroupDTO> listComponent = new List<ComponentMetaDataGroupDTO>();
            listComponent.Add(reqComponent);
            listComponent.Add(addComponent);
            ComponentMetaDataGroupDTO[] arrComponent = listComponent.ToArray();

            return arrComponent;
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void WellboreComponentsValidations()
        {
            WellConfigDTO well = AddWell("RPOC_");
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.WellBoreReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
            //Tubing String(catDescriptionID,ID,OD,quantity,length,TopDepth,BottomDepth)
            decimal[] detailsTubing = { 27158, 2.441m, 2.875m, 31, 885.05m, 101, 986.05m, 279 };
            ComponentMetaDataGroupDTO[] tubingString = AddTubingString("Tubing String", "Tubing - OD  2.875", wellId, assemblyId, jobId, detailsTubing, check);
            bool addTubing = ComponentService.AddComponent(tubingString);
            Assert.IsTrue(addTubing);
            //Conductor Casing
            decimal[] detailsConductorCasing = { 10294, 12.5m, 14m, 1, 11, 0, 11, 279 };
            ComponentMetaDataGroupDTO[] conductorCasing = AddTubingString("Conductor Casing", "Casing/Casing Liner OD 14.000", wellId, assemblyId, jobId, detailsConductorCasing, check);
            bool addConductorCasing = ComponentService.AddComponent(conductorCasing);
            Assert.IsTrue(addConductorCasing);
            //Production Casing -- Casing/Casing Liner OD  7.000
            decimal[] detailsprodCasing = { 23902, 6.366m, 7m, 45, 1353, 0, 1353, 279 };
            ComponentMetaDataGroupDTO[] prodCasing = AddTubingString("Production Casing", "Casing/Casing Liner OD  7.000", wellId, assemblyId, jobId, detailsprodCasing, check);
            bool addprodCasingOD7 = ComponentService.AddComponent(prodCasing);
            Assert.IsTrue(addprodCasingOD7);
            //Production Casing -- Cement(Behind Casing)
            decimal[] detailscement = { 1213, 0, 0, 45, 1366, 0, 1366, 279 };
            ComponentMetaDataGroupDTO[] prodCement = AddTubingString("Production Casing", "Cement (behind Casing)", wellId, assemblyId, jobId, detailscement, check);
            bool addprodCasingCement = ComponentService.AddComponent(prodCement);
            Assert.IsTrue(addprodCasingCement);
            //Production Casing -- Bridge Plug
            decimal[] detailsbridgePlug = { 2075, 0, 4, 1, 6, 95, 101, 279 };
            ComponentMetaDataGroupDTO[] bridgePlug = AddTubingString("Production Casing", "Bridge Plug", wellId, assemblyId, jobId, detailsbridgePlug, check);
            bool addbridgePlug = ComponentService.AddComponent(bridgePlug);
            Assert.IsTrue(addbridgePlug);
            //Production Casing -- Wellbore Completion Detail (Perforations, etc.)
            decimal[] detailswp1 = { 1062, 0, 0, 0, 14, 1095, 1109, 279 };
            ComponentMetaDataGroupDTO[] wp1 = AddTubingString("Production Casing", "Wellbore Completion Detail (Perforations, etc.)", wellId, assemblyId, jobId, detailswp1, check);
            bool addwp1 = ComponentService.AddComponent(wp1);
            Assert.IsTrue(addwp1);
            //Production Casing -- Wellbore Completion Detail (Perforations, etc.)
            decimal[] detailswp2 = { 1062, 0, 0, 1, 10, 1124, 1134, 279 };
            ComponentMetaDataGroupDTO[] wp2 = AddTubingString("Production Casing", "Wellbore Completion Detail (Perforations, etc.)", wellId, assemblyId, jobId, detailswp2, check);
            bool addwp2 = ComponentService.AddComponent(wp2);
            Assert.IsTrue(addwp2);
            //Production Casing -- Plug Back Cement
            decimal[] detailsplugBack = { 1457, 0, 0, 57, 57, 1309, 1366, 279 };
            ComponentMetaDataGroupDTO[] plugBack = AddTubingString("Production Casing", "Plug - Cement", wellId, assemblyId, jobId, detailsplugBack, check);
            bool addplugBack = ComponentService.AddComponent(plugBack);
            Assert.IsTrue(addplugBack);
            //Production Casing -- Wellbore Completion Detail (Perforations, etc.)
            decimal[] detailswp3 = { 1062, 0, 0, 0, 2, 1159, 1161, 279 };
            ComponentMetaDataGroupDTO[] wp3 = AddTubingString("Production Casing", "Wellbore Completion Detail (Perforations, etc.)", wellId, assemblyId, jobId, detailswp3, check);
            bool addwp3 = ComponentService.AddComponent(wp3);
            Assert.IsTrue(addwp3);
            //Production Casing -- Wellbore Completion Detail (Perforations, etc.)
            decimal[] detailswp4 = { 1062, 0, 0, 0, 12, 1163, 1175, 279 };
            ComponentMetaDataGroupDTO[] wp4 = AddTubingString("Production Casing", "Wellbore Completion Detail (Perforations, etc.)", wellId, assemblyId, jobId, detailswp4, check);
            bool addwp4 = ComponentService.AddComponent(wp4);
            Assert.IsTrue(addwp4);
            //Production Casing -- Wellbore Completion Detail (Perforations, etc.)
            decimal[] detailswp5 = { 1062, 0, 0, 1, 10, 1180, 1190, 279 };
            ComponentMetaDataGroupDTO[] wp5 = AddTubingString("Production Casing", "Wellbore Completion Detail (Perforations, etc.)", wellId, assemblyId, jobId, detailswp5, check);
            bool addwp5 = ComponentService.AddComponent(wp5);
            Assert.IsTrue(addwp5);
            //Production Casing -- Wellbore Completion Detail (Perforations, etc.)
            decimal[] detailswp6 = { 1062, 0, 0, 1, 10, 1216, 1226, 279 };
            ComponentMetaDataGroupDTO[] wp6 = AddTubingString("Production Casing", "Wellbore Completion Detail (Perforations, etc.)", wellId, assemblyId, jobId, detailswp6, check);
            bool addwp6 = ComponentService.AddComponent(wp6);
            Assert.IsTrue(addwp6);
            //Production Casing -- Wellbore Completion(Production Interval)
            decimal[] detailsPI = { 1560, 0, 0, 0, 131, 1095, 1226, 279 };
            ComponentMetaDataGroupDTO[] PInterval = AddTubingString("Production Casing", "Wellbore Completion (Producing Interval)", wellId, assemblyId, jobId, detailsPI, check);
            bool addPI = ComponentService.AddComponent(PInterval);
            Assert.IsTrue(addPI);
            //Production Liner -- Casing/Casing Liner OD  5.500
            decimal[] detailsprodCasingLiner = { 1967, 4.892m, 5.5m, 7, 224, 1085, 1309, 279 };
            ComponentMetaDataGroupDTO[] casingLiner = AddTubingString("Production Liner", "Casing/Casing Liner OD  5.500", wellId, assemblyId, jobId, detailsprodCasingLiner, check);
            bool addcasingLiner = ComponentService.AddComponent(casingLiner);
            Assert.IsTrue(addcasingLiner);
            //Production Liner -- Wellbore Completion Detail (Perforations, etc.)
            decimal[] detailswp7 = { 1065, 0, 0, 0, 85, 1095, 1180, 279 };
            ComponentMetaDataGroupDTO[] wp7 = AddTubingString("Production Liner", "Wellbore Completion Detail (Perforations, etc.)", wellId, assemblyId, jobId, detailswp7, check);
            bool addwp7 = ComponentService.AddComponent(wp7);
            Assert.IsTrue(addwp7);
            //Production Liner -- Wellbore Completion Detail (Perforations, etc.)
            decimal[] detailswp8 = { 1062, 0, 0, 1, 10, 1124, 1134, 279 };
            ComponentMetaDataGroupDTO[] wp8 = AddTubingString("Production Liner", "Wellbore Completion Detail (Perforations, etc.)", wellId, assemblyId, jobId, detailswp8, check);
            bool addwp8 = ComponentService.AddComponent(wp8);
            Assert.IsTrue(addwp8);
            //Production Liner -- Wellbore Completion Detail (Perforations, etc.)
            decimal[] detailswp9 = { 1062, 0, 0, 1, 10, 1180, 1190, 279 };
            ComponentMetaDataGroupDTO[] wp9 = AddTubingString("Production Liner", "Wellbore Completion Detail (Perforations, etc.)", wellId, assemblyId, jobId, detailswp9, check);
            bool addwp9 = ComponentService.AddComponent(wp9);
            Assert.IsTrue(addwp9);
            //Production Liner -- Wellbore Completion Detail (Perforations, etc.)
            decimal[] detailswp10 = { 1062, 0, 0, 1, 10, 1216, 1226, 279 };
            ComponentMetaDataGroupDTO[] wp10 = AddTubingString("Production Liner", "Wellbore Completion Detail (Perforations, etc.)", wellId, assemblyId, jobId, detailswp10, check);
            bool addwp10 = ComponentService.AddComponent(wp10);
            Assert.IsTrue(addwp10);
            //Wellbore Notes
            decimal[] detailswn1 = { 1793, 0, 0, 1, 0.01m, 1366, 1366.01m, 279 };
            ComponentMetaDataGroupDTO[] wn1 = AddTubingString("Wellbore Notes", "Wellbore Notes", wellId, assemblyId, jobId, detailswn1, check);
            bool addwn1 = ComponentService.AddComponent(wn1);
            Assert.IsTrue(addwn1);
            //Wellbore Notes
            decimal[] detailswn2 = { 1793, 0, 0, 1, 0, 20, 20, 279 };
            ComponentMetaDataGroupDTO[] wn2 = AddTubingString("Wellbore Notes", "Wellbore Notes", wellId, assemblyId, jobId, detailswn2, check);
            bool addwn2 = ComponentService.AddComponent(wn2);
            Assert.IsTrue(addwn2);
            //Wellbore Notes
            decimal[] detailswn3 = { 1189, 0, 0, 1, 0, 26, 26, 279 };
            ComponentMetaDataGroupDTO[] wn3 = AddTubingString("Wellbore Notes", "Wellbore Notes", wellId, assemblyId, jobId, detailswn3, check);
            bool addwn3 = ComponentService.AddComponent(wn3);
            Assert.IsTrue(addwn3);
            //Borehole
            decimal[] detailsBhole = { 1428, 20, 20, 1, 11, 0, 11, 279 };
            ComponentMetaDataGroupDTO[] bh = AddTubingString("Borehole", "Wellbore Hole", wellId, assemblyId, jobId, detailsBhole, check);
            bool addbh = ComponentService.AddComponent(bh);
            Assert.IsTrue(addbh);
            //Borehole
            decimal[] detailsBhole1 = { 1613, 8.75m, 8.75m, 1, 1355, 11, 1366, 279 };
            ComponentMetaDataGroupDTO[] bh1 = AddTubingString("Borehole", "Wellbore Hole", wellId, assemblyId, jobId, detailsBhole1, check);
            bool addbh1 = ComponentService.AddComponent(bh1);
            Assert.IsTrue(addbh1);

            //Get Validations for the components
            WellboreGridDTO[] wellboreComponents = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());
            Assert.IsNotNull(wellboreComponents);
            Assert.AreEqual(1, wellboreComponents.Count());
            Assert.AreEqual(23, wellboreComponents.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup.Count());
            foreach (WellboreGridGroupDTO comp in wellboreComponents.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup)
            {
                ComponentMetaDataGroupDTO mdComp = comp.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required");
                Assert.IsNotNull(mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Event_Beginning").DataValue);
                Assert.IsNull(mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Event_Ending").DataValue);
                Trace.WriteLine("Validations Errors for: " + mdComp.Fields.FirstOrDefault(x => x.ColumnName == "cmcCompName").DataValue);
                WellboreComponentValidationErrorsDTO[] errors = mdComp.ValidationErrors;
                foreach (WellboreComponentValidationErrorsDTO validationError in errors)
                {
                    Trace.WriteLine(validationError.DynamicDescription);
                }
                Trace.WriteLine("Validation Error count is: " + mdComp.ValidationErrorsCount);
                if (mdComp.ComponentGroupingName.Contains("Wellbore Notes"))
                    Assert.AreEqual(0, mdComp.ValidationErrorsCount);
            }

            //Remove Component
            ComponentPartTypeDTO Ids = new ComponentPartTypeDTO();
            Ids.JobId = getJob.JobId;

            foreach (WellboreGridGroupDTO comp in wellboreComponents.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup)
            {
                ComponentMetaDataGroupDTO mdComp = comp.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required");
                var ascId = mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascPrimaryKey").DataValue;
                Ids.AssemblyComponentId = Convert.ToInt64(ascId);
                Ids.PartTypeId = (int)mdComp.PartTypePrimaryKey;
                Ids.ComponentId = mdComp.ComponentPrimaryKey;
                Ids.EventId = check;
                bool rComp = ComponentService.RemoveComponent(Ids);
                Assert.IsTrue(rComp);
            }
            wellboreComponents = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());
            Assert.IsNotNull(wellboreComponents);
            Assert.AreEqual(0, wellboreComponents.Count());
            // Remove Event and its extended information
            evt.Id = check;
            bool rcheck = JobAndEventService.RemoveEvent(evt);
            Assert.IsTrue(rcheck, "Failed to remove Economic Analysis report");
        }

        #endregion Wellbore Validations

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void VerticalSchematicforMultipleSubAssemblies()
        {
            string facilityIdBase = "RPOC_";
            string facilityId = s_isRunningInATS ? facilityIdBase + "00001" : facilityIdBase + "0001";
            ReferenceTableItemDTO[] depthDatums = WellConfigurationService.GetReferenceTableItems("r_WellDepthDatum");
            ReferenceTableItemDTO depthDatum = depthDatums.FirstOrDefault(t => t.ConstantId == "GROUND_LEVEL") ?? depthDatums.FirstOrDefault();
            WellConfigDTO wellConfig = WellConfigurationService.AddWellConfig(new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = DefaultWellName,
                    FacilityId = facilityId,
                    DataConnection = GetDefaultCygNetDataConnection(),
                    IntervalAPI = "IntervalAPI",
                    SubAssemblyAPI = "SubAssemblyAPI 01",
                    AssemblyAPI = facilityId,
                    CommissionDate = DateTime.Today,
                    WellType = WellTypeId.RRL,
                    WellDepthDatumId = depthDatum?.Id,
                })
            });
            WellDTO well = wellConfig?.Well;
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            AssemblyDTO getAssembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(getAssembly);
            SubAssemblyDTO getSubAssemblies01 = WellboreComponentService.GetSubAssembliesByWellId(well.Id.ToString()).FirstOrDefault(x => x.SAId == "SubAssemblyAPI 01");
            Assert.IsNotNull(getSubAssemblies01);
            SubAssemblyDTO newSubAssembly = new SubAssemblyDTO
            {
                AssemblyId = getAssembly.Id,
                SAId = "SubAssemblyAPI 02",
                SubAssembly_ParentId = getSubAssemblies01.Id,
                SubAssemblyType = 3,
                SubAssemblyDesc = SubAssemblyTypeId.Sidetrack_Lateral_1.ToString(),
                WellDepthDatum = 1,
                UserId = 3,
                StateTime = DateTime.Today,
            };
            WellboreComponentService.AddSubAssembly(newSubAssembly);
            SubAssemblyDTO getSubAssemblies02 = WellboreComponentService.GetSubAssembliesByWellId(well.Id.ToString()).FirstOrDefault(x => x.SAId == "SubAssemblyAPI 02");
            Assert.IsNotNull(getSubAssemblies02);
            newSubAssembly.SAId = "SubAssemblyAPI 0201";
            newSubAssembly.SubAssemblyDesc = SubAssemblyTypeId.Sidetrack_Lateral_2.ToString();
            newSubAssembly.SubAssembly_ParentId = getSubAssemblies02.Id;
            newSubAssembly.SubAssemblyType = 4;
            WellboreComponentService.AddSubAssembly(newSubAssembly);
            SubAssemblyDTO getSubAssemblies0201 = WellboreComponentService.GetSubAssembliesByWellId(well.Id.ToString()).FirstOrDefault(x => x.SAId == "SubAssemblyAPI 0201");
            Assert.IsNotNull(getSubAssemblies0201);
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.WellBoreReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            var check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
            //Empty Schematic
            ComponentVerticalSchematicInputDTO inputSchematic = new ComponentVerticalSchematicInputDTO();
            inputSchematic.AssemblyId = getAssembly.Id;
            inputSchematic.SubAssemblyId = getSubAssemblies01.Id;
            inputSchematic.EndDate = getJob.EndDate.ToLocalTime();
            inputSchematic.EventId = check;
            inputSchematic.JobId = Convert.ToInt64(jobId);
            ComponentVerticalSchematicDTO[] componentSchematic = ComponentService.GetComponentVerticalSchematic(inputSchematic);
            Assert.AreEqual(0, componentSchematic.Count());
            //Add Component - 01
            ComponentMetaDataGroupDTO[] arrComponent = AddComponent(jobId, well.Id.ToString(), getAssembly.Id.ToString(), getSubAssemblies01.Id.ToString(), check, 0, 500);
            bool addComp = ComponentService.AddComponent(arrComponent);
            Assert.IsTrue(addComp);
            //Add Component - 01
            arrComponent = AddComponent(jobId, well.Id.ToString(), getAssembly.Id.ToString(), getSubAssemblies01.Id.ToString(), check, 500, 1000);
            addComp = ComponentService.AddComponent(arrComponent);
            Assert.IsTrue(addComp);
            //Add Component - 01/cut-off for sidetrack 01
            arrComponent = AddComponent(jobId, well.Id.ToString(), getAssembly.Id.ToString(), getSubAssemblies01.Id.ToString(), check, 1000, 1200);
            addComp = ComponentService.AddComponent(arrComponent);
            Assert.IsTrue(addComp);
            //Add Component -02
            arrComponent = AddComponent(jobId, well.Id.ToString(), getAssembly.Id.ToString(), getSubAssemblies02.Id.ToString(), check, 501, 1000);
            addComp = ComponentService.AddComponent(arrComponent);
            Assert.IsTrue(addComp);
            //Add Component -02/cut-off for side track 02
            arrComponent = AddComponent(jobId, well.Id.ToString(), getAssembly.Id.ToString(), getSubAssemblies02.Id.ToString(), check, 1000, 1200);
            addComp = ComponentService.AddComponent(arrComponent);
            Assert.IsTrue(addComp);
            //Add Component
            arrComponent = AddComponent(jobId, well.Id.ToString(), getAssembly.Id.ToString(), getSubAssemblies0201.Id.ToString(), check, 999, 1500);
            addComp = ComponentService.AddComponent(arrComponent);
            Assert.IsTrue(addComp);
            //Single Component Schematic
            componentSchematic = ComponentService.GetComponentVerticalSchematic(inputSchematic);
            Assert.AreEqual(3, componentSchematic.Count());
            //Component Schematic
            inputSchematic.SubAssemblyId = getSubAssemblies02.Id;
            componentSchematic = ComponentService.GetComponentVerticalSchematic(inputSchematic);
            Assert.AreEqual(2, componentSchematic.Count());
            Assert.AreEqual(1200, componentSchematic[1].DescriptionAndGeometry.Geometry.Depth.BottomDepth);
            //Component Schematic
            inputSchematic.SubAssemblyId = getSubAssemblies0201.Id;
            componentSchematic = ComponentService.GetComponentVerticalSchematic(inputSchematic);
            Assert.AreEqual(1, componentSchematic.Count());
            Assert.AreEqual(1500, componentSchematic[0].DescriptionAndGeometry.Geometry.Depth.BottomDepth);
            //Get Component
            WellboreGridDTO[] wellboreComponent = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());
            Assert.IsNotNull(wellboreComponent);
            Assert.AreEqual(3, wellboreComponent.Count());
            Assert.AreEqual(3, wellboreComponent.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup.Count());
            Assert.AreEqual(2, wellboreComponent.FirstOrDefault(x => x.SubAssemblyDesc == "Sidetrack_Lateral_1").WellboreGridGroup.Count());
            Assert.AreEqual(1, wellboreComponent.FirstOrDefault(x => x.SubAssemblyDesc == "Sidetrack_Lateral_2").WellboreGridGroup.Count());
            //Remove Component
            ComponentPartTypeDTO Ids = new ComponentPartTypeDTO();
            Ids.JobId = getJob.JobId;
            Ids.PartTypeId = (int)arrComponent.FirstOrDefault().PartTypePrimaryKey;
            foreach (WellboreGridDTO sa in wellboreComponent)
            {
                foreach (WellboreGridGroupDTO comp in sa.WellboreGridGroup)
                {
                    ComponentMetaDataGroupDTO mdComp = comp.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required");
                    Assert.IsNotNull(mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Event_Beginning").DataValue);
                    Assert.IsNull(mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Event_Ending").DataValue);
                    var ascId = mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascPrimaryKey").DataValue;
                    Ids.AssemblyComponentId = Convert.ToInt64(ascId);
                    Ids.ComponentId = mdComp.ComponentPrimaryKey;
                    Ids.EventId = check;
                    bool rComp = ComponentService.RemoveComponent(Ids);
                    Assert.IsTrue(rComp);
                }
            }
            wellboreComponent = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());
            Assert.IsNotNull(wellboreComponent);
            Assert.AreEqual(0, wellboreComponent.Count());
            // Remove Event and its extended information
            evt.Id = check;
            bool rcheck = JobAndEventService.RemoveEvent(evt);
            Assert.IsTrue(rcheck, "Failed to remove Economic Analysis report");
        }

        #region Add Components Check
        /// <summary>
        /// this has been trouble some script since 4/18/2020 Changes Need to know what got changed
        /// </summary>
        [Ignore]
        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void QuickAddtoComponentCheck()
        {
            if (s_isRunningInATS)
            {
                Trace.WriteLine("");
                ComponentCheck();
            }
            else
            {
                Trace.WriteLine("This test is too long for Team city");
            }
        }

        public void QAtoComponent(RRLPartTypeComponentGroupingTypeDTO[] componentGroups, string jobId, string subassemblyId, long eventId)
        {
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            int addQAComps = 0;
            int failQAComps = 0;
            foreach (RRLPartTypeComponentGroupingTypeDTO compGrp in componentGroups)
            {
                partfilter.TypeId = compGrp.ptgFK_c_MfgCat_ComponentGrouping;
                partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
                RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
                Assert.IsNotNull(partTypes, "Failed to retrieve Part types");

                //Meta Data for Add quick add component
                foreach (RRLPartTypeComponentGroupingTypeDTO ptype in partTypes)
                {
                    ComponentMetaDataGroupDTO[] cmpMetaData = ComponentService.GetMetaDataForQuickAddComponentByPartTypeId(ptype.ptgFK_c_MfgCat_PartType.ToString());
                    Assert.IsNotNull(cmpMetaData);
                    MetaDataDTO[] cdReference = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;

                    //Get Meta data for the Catalog Item description
                    MetaDataReferenceData cd = new MetaDataReferenceData();
                    cd.MetaData = cdReference.FirstOrDefault(x => x.Title == "Manufacturer");
                    //Manufacturer
                    MetaDataDTO mdManufacturer = cd.MetaData;
                    ControlIdTextDTO[] listManufacturers = GetMetadataReferenceDataDDL(mdManufacturer, ptype.ptgFK_c_MfgCat_PartType.ToString());
                    ControlIdTextDTO manufacturer = listManufacturers.FirstOrDefault();
                    MetaDataFilterDTO cdFilter = new MetaDataFilterDTO();
                    cdFilter.ColumnValue = manufacturer.ControlId.ToString();
                    cdFilter.MetaDataFilterToken = cd.MetaData.ExtendedFilterInput;
                    //CatalogItem Description
                    MetaDataDTO mdCatDescription = cdReference.FirstOrDefault(x => x.Title == "Catalog Item Description");
                    ControlIdTextDTO[] listCatDescription = GetMetadataReferenceDataDDL(mdCatDescription, ptype.ptgFK_c_MfgCat_PartType.ToString(), manufacturer.ControlId.ToString());
                    if (listCatDescription.Count() == 0)
                    {
                        foreach (ControlIdTextDTO mf in listManufacturers)
                        {
                            listCatDescription = GetMetadataReferenceDataDDL(mdCatDescription, ptype.ptgFK_c_MfgCat_PartType.ToString(), mf.ControlId.ToString());
                            if (listCatDescription.Count() > 0)
                            {
                                manufacturer = mf;
                                break;
                            }
                        }
                        if (listCatDescription.Count() == 0)
                        {
                            Trace.WriteLine("Failed to get Catalog description for the combination of QA :" + compGrp.strComponentGrouping + " : : " + ptype.ptyPartType);
                        }
                    }
                    try
                    {
                        ControlIdTextDTO catDescription = listCatDescription.FirstOrDefault();

                        //Add Quick Add Component
                        ComponentMetaDataGroupDTO reqComponent = new ComponentMetaDataGroupDTO();
                        reqComponent.QuickAddCategory = compGrp.strComponentGrouping;
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
                        if (addQuickComponent == null)
                        {
                            Trace.WriteLine("Failed to Add QA :" + compGrp.strComponentGrouping + " : : " + ptype.ptyPartType);
                            failQAComps = failQAComps + 1;
                        }
                        else
                        {
                            addQAComps = addQAComps + 1;
                        }
                        if (addQAComps == 50)
                        {
                            break;
                        }
                    }
                    catch
                    {
                        Trace.WriteLine("Failed to Add QA :" + compGrp.strComponentGrouping + " : : " + ptype.ptyPartType);
                        failQAComps = failQAComps + 1;
                    }
                }
            }

            QuickAddComponentGroupingDTO[] QaCompscheck = ComponentService.GetQuickAddComponents();

            foreach (QuickAddComponentGroupingDTO qcomp in QaCompscheck)
            {
                foreach (QuickAddComponentDTO qaComp in qcomp.QuickAddComponents)
                {
                    try
                    {
                        ComponentMetaDataGroupDTO[] addedQAComp = ComponentService.GetMetaDataForUpdateQuickAddComponent(qaComp.Component.MfgCat_PartType.ToString(), qaComp.Component.Id.ToString());
                        if (addedQAComp.Count() < 2)
                            Trace.WriteLine("Failed to get Metadata for Update QA: " + qaComp.ComponentGroupingName.ToString() + " : : " + qaComp.Component.MfgCat_PartType.ToString());
                    }
                    catch
                    {
                        Trace.WriteLine("Failed to get Metadata for Update QA:" + qaComp.ComponentGroupingName.ToString() + " : : " + qaComp.Component.MfgCat_PartType.ToString());
                    }
                }
            }
            int total = 0;
            foreach (QuickAddComponentGroupingDTO qcomp in QaCompscheck)
            {
                total = total + qcomp.QuickAddComponents.Count();
            }
            List<QuickAddComponentDTO> componentQA = new List<QuickAddComponentDTO>();
            //Add Component through Quick Add
            int addComps = 0;
            int obtainedComps = 0;
            foreach (QuickAddComponentGroupingDTO qagroup in QaCompscheck)
            {
                foreach (QuickAddComponentDTO qaComp in qagroup.QuickAddComponents)
                {
                    componentQA.Add(qaComp);
                    qaComp.JobId = Convert.ToInt64(jobId);
                    qaComp.SubAssemblyId = Convert.ToInt64(subassemblyId);
                    try
                    {
                        ComponentMetaDataGroupBatchCollectionDTO[] qaMetadatagroup = ComponentService.GetMetadataFromQuickAddComponents(componentQA.ToArray());
                        foreach (ComponentMetaDataGroupBatchCollectionDTO collection in qaMetadatagroup)
                        {
                            collection.ComponentMetadataCollection.FirstOrDefault().EventId = eventId;
                        }
                        //Add Batch Component
                        bool saveBatch = ComponentService.SaveWellboreComponent(qaMetadatagroup);
                        if (!saveBatch)
                            Trace.WriteLine("Failed to Add Component through Quick Add : " + qaComp.ComponentGroupingName.ToString() + " : : " + qaComp.Component.MfgCat_PartType.ToString());
                        else
                            addComps = addComps + 1;
                    }
                    catch
                    {
                        Trace.WriteLine("Failed to Add Component through Quick Add : " + qaComp.ComponentGroupingName.ToString() + " : : " + qaComp.Component.MfgCat_PartType.ToString());
                    }
                    componentQA.Clear();
                }
            }
            int removeComp = 0;
            //Get Components added through Quick add
            try
            {
                Trace.WriteLine("API called for Get Components");
                WellboreGridDTO[] comps = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, eventId.ToString());
                Trace.WriteLine("Responce recieved from Get Components API");
                obtainedComps = comps.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup.Count();

                foreach (WellboreGridGroupDTO comp in comps.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup)
                {
                    ComponentMetaDataGroupDTO metadata = comp.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required");
                    Assert.IsNotNull(metadata.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Event_Beginning").DataValue);
                    Assert.IsNull(metadata.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Event_Ending").DataValue);
                    try
                    {
                        ComponentMetaDataGroupDTO[] mdComp = ComponentService.GetComponentMetaDataForUpdate(metadata.PartTypePrimaryKey.ToString(), metadata.ComponentPrimaryKey.ToString(), metadata.Fields.FirstOrDefault(x => x.ColumnName == "ascPrimaryKey").DataValue.ToString());
                        if (mdComp.Count() < 2)
                            Trace.WriteLine("Failed to get Metadata for Update: " + metadata.Fields.FirstOrDefault(x => x.ColumnName == "cmcCompName").DataValue);
                    }
                    catch
                    {
                        Trace.WriteLine("Failed to get Metadata for Update: " + metadata.Fields.FirstOrDefault(x => x.ColumnName == "cmcCompName").DataValue);
                    }
                }
                //Remove Added Component
                Trace.WriteLine("Remove Components Started");
                ComponentPartTypeDTO Ids = new ComponentPartTypeDTO();
                Ids.JobId = Convert.ToInt64(jobId);
                while (comps.Count() != 0)
                {
                    foreach (WellboreGridGroupDTO comp in comps.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup)
                    {
                        ComponentMetaDataGroupDTO metadata = comp.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required");
                        var ascId = metadata.Fields.FirstOrDefault(x => x.ColumnName == "ascPrimaryKey").DataValue;
                        Ids.AssemblyComponentId = Convert.ToInt64(ascId);
                        Ids.ComponentId = metadata.ComponentPrimaryKey;
                        Ids.PartTypeId = (int)metadata.PartTypePrimaryKey;
                        Ids.EventId = eventId;
                        try
                        {
                            bool rComp = ComponentService.RemoveComponent(Ids);
                            if (!rComp)
                                Trace.WriteLine("Failed to remove component that added through Quick Add : " + metadata.Fields.FirstOrDefault(x => x.ColumnName == "cmcCompName").DataValue + " : : " + metadata.PartType);
                            else
                                removeComp = removeComp + 1;
                        }
                        catch
                        {
                            Trace.WriteLine("Failed to remove component that added through Quick Add : " + metadata.Fields.FirstOrDefault(x => x.ColumnName == "cmcCompName").DataValue + " : : " + metadata.PartType);
                        }
                    }
                    comps = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, eventId.ToString());
                }
                Trace.WriteLine("Remove Components completed");
            }
            catch
            {
                Trace.WriteLine("Failed to get Components for Added Job");
            }
            //Remove Quick Add components
            int removeQAComp = 0;
            foreach (QuickAddComponentGroupingDTO qcomp in QaCompscheck)
            {
                foreach (QuickAddComponentDTO qaComp in qcomp.QuickAddComponents)
                {
                    try
                    {
                        bool removeQAcomp = ComponentService.RemoveQuickAddComponent(qaComp.QuickAddComponentId.ToString());
                        if (!removeQAcomp)
                        {
                            Trace.WriteLine("Failed to remove QA : " + qaComp.ComponentGroupingName.ToString() + " : : " + qaComp.Component.MfgCat_PartType.ToString());
                            removeQAComp = removeQAComp + 1;
                        }
                    }
                    catch
                    {
                        Trace.WriteLine("Failed to remove QA :" + qaComp.ComponentGroupingName.ToString() + " : : " + qaComp.Component.MfgCat_PartType.ToString());
                        removeQAComp = removeQAComp + 1;
                    }
                }
            }
            //Verification
            Assert.AreEqual(addComps, obtainedComps, "Mismatch between added and obtained Components");
            Assert.AreEqual(addComps, removeComp, "No. of components added should be equal to removed");
            Assert.AreEqual(addQAComps, total, "Mismatch between added and obtained QA components");
            Assert.AreEqual(addComps, addQAComps, "Failed to add all available components through Quick add");
            Assert.AreEqual(0, failQAComps, "No of components that are failed to add : " + failQAComps.ToString());
            Assert.AreEqual(0, removeQAComp, "No of components that are failed to remove : " + removeQAComp.ToString());
        }

        public void ComponentCheck()
        {
            //Add Well,Job
            WellConfigDTO well = AddWell("RPOC_");
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.WellBoreReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
            int i = 0;
            int j = 0;
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");

            //Check one after other
            List<RRLPartTypeComponentGroupingTypeDTO> cg = new List<RRLPartTypeComponentGroupingTypeDTO>();
            foreach (RRLPartTypeComponentGroupingTypeDTO compGrp in componentGroups)
            {
                Trace.WriteLine("Started : " + compGrp.strComponentGrouping);
                cg.Add(compGrp);
                try
                {
                    QAtoComponent(cg.ToArray(), jobId, subassemblyId, check);
                    j = j + 1;
                }
                catch (AssertFailedException e)
                {
                    Trace.WriteLine("<==============================================================>");
                    Trace.WriteLine("At the time of failing component, total component was:- " + j);
                    Trace.WriteLine("<==============================================================>");
                    i = i + 1;
                    Trace.WriteLine("Failed on : " + compGrp.strComponentGrouping + e.ToString());
                }
                cg.Clear();
                Trace.WriteLine("Completed : " + compGrp.strComponentGrouping);
            }
            Assert.AreEqual(0, i, "Failed to Add " + i.ToString() + " components through quick add");
        }

        #endregion Add Components Check

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void WellboreReportCheckonAddEvent()
        {
            WellConfigDTO well = AddWell("RPOC_");
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.WellBoreReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            var check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Wellbore Report");

            ReportDTO reportExists = WellboreComponentService.GetReportByWellId(wellId);
            Assert.IsNotNull(reportExists);
            Assert.AreEqual(reportExists.ReportTypeId, 3);
            Assert.AreEqual(reportExists.JobTypeId, getJob.JobTypeId);
            Assert.AreEqual(reportExists.JobReasonId, getJob.JobReasonId);
            Assert.AreEqual(reportExists.OffDate, getJob.BeginDate.Date);
            Assert.AreEqual(reportExists.WorkoverDate, getJob.EndDate.Date);
            Assert.AreEqual(reportExists.OnDate, getJob.EndDate.Date);
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void FailureReportCheckonAddEvent()
        {
            WellConfigDTO well = AddWell("RPOC_");
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.FailureReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.FailureReport);
            var check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");

            ReportDTO reportExists = WellboreComponentService.GetReportByWellId(wellId);
            Assert.IsNotNull(reportExists);
            Assert.AreEqual(2, reportExists.ReportTypeId, "Report Type Id is not matching");
            Assert.AreEqual(reportExists.JobTypeId, getJob.JobTypeId, "Job Type id mismatch found");
            Assert.AreEqual(reportExists.JobReasonId, getJob.JobReasonId, "Job Reason id mismatch found");
            //Assert.AreEqual(reportExists.OffDate, getJob.BeginDate,"Off date mismatch found");
            Assert.AreEqual(reportExists.WorkoverDate, getJob.EndDate.Date, "Workover date mismatch found");
            Assert.AreEqual(reportExists.OnDate, getJob.EndDate.Date, "On date mismatch found");
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void GetComponentODandID()
        {
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeOfFilter = ComponentFilterTypes.PartType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");
            foreach (RRLPartTypeComponentGroupingTypeDTO pt in partTypes)
            {
                //9/3/2019 - To avoid compilation error, passed an added parameter as null. Integration test needs to be fixed according to FRWM-5489
                //string hardCodedComponentGroupId = null;
                ComponentMetaDataGroupDTO[] compMetaData = ComponentService.GetComponentMetaDataForAdd(pt.ptgFK_c_MfgCat_PartType.ToString(), pt.ptgFK_c_MfgCat_ComponentGrouping.ToString());
                compMetaData.FirstOrDefault(x => x.CategoryName.ToUpper() == "REQUIRED").PartTypePrimaryKey = pt.ptgFK_c_MfgCat_PartType;
                Assert.IsNotNull(compMetaData, "Failed retrieve Metadata for partType : " + pt.ptyPartType);
                Assert.IsTrue(compMetaData.Count() >= 1, "Failed to Retrieve MetaData");
                Assert.AreEqual("Required", compMetaData.FirstOrDefault().CategoryName, "Does not have Required Category");
                compMetaData = ComponentService.GetComponentIDAndOD(compMetaData);
                Assert.IsNotNull(compMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields.FirstOrDefault(x => x.ColumnName == "cmcOuterDiameter").DataValue);
                Assert.IsNotNull(compMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields.FirstOrDefault(x => x.ColumnName == "cmcInnerDiameter").DataValue);
            }
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void HeaderFooterCRUD()
        {
            //Get Header Config
            int header = (int)HeaderFooterConfigurationType.Header;
            HeaderFooterConfigDTO[] headerConfig = ComponentService.GetHeaderFooterConfiguration(header.ToString());
            Assert.AreEqual(0, headerConfig.Count());
            //Data for Header Footer
            HeaderFooterDataDTO[] data = ComponentService.GetDataForHeaderFooter();
            Assert.AreNotEqual(0, data.Count(), "No data obtained for the Header footer Configuration");
            //Header Info
            HeaderFooterConfigDTO headerInfo = new HeaderFooterConfigDTO();
            headerInfo.RowNumber = 1;
            headerInfo.SizeID = HeaderFooterColumnSize.Large;
            headerInfo.HeaderFooterConfig = HeaderFooterConfigurationType.Header;
            headerInfo.DisplayName = "Automated Header";
            headerInfo.DataID = data.FirstOrDefault().DataID;
            headerInfo.Order = 1;
            headerInfo.IsWordwrapped = false;
            //Add Header
            string headerId = ComponentService.AddUpdateHeaderFooterConfiguration(headerInfo);
            Assert.IsNotNull(headerId);
            headerConfig = ComponentService.GetHeaderFooterConfiguration(header.ToString());
            Assert.AreEqual(1, headerConfig.Count());
            Assert.AreEqual("Automated Header", headerConfig.FirstOrDefault().DisplayName);
            Assert.AreEqual(data.FirstOrDefault().DataID, headerConfig.FirstOrDefault().DataID);
            Assert.AreEqual(HeaderFooterColumnSize.Large, headerConfig.FirstOrDefault().SizeID);
            Assert.IsFalse(headerConfig.FirstOrDefault().IsWordwrapped);
            Assert.AreEqual(1, headerConfig.FirstOrDefault().RowNumber);
            Assert.AreEqual(HeaderFooterConfigurationType.Header, headerConfig.FirstOrDefault().HeaderFooterConfig);
            //Update Header
            headerInfo.DisplayName = "Updated Header";
            headerInfo.Id = Convert.ToInt64(headerId);
            headerInfo.HeaderFooterConfig = HeaderFooterConfigurationType.Footer;
            headerId = ComponentService.AddUpdateHeaderFooterConfiguration(headerInfo);
            Assert.IsNotNull(headerId);
            headerConfig = ComponentService.GetHeaderFooterConfiguration(header.ToString());
            Assert.AreEqual(0, headerConfig.Count());
            int footer = (int)HeaderFooterConfigurationType.Footer;
            headerConfig = ComponentService.GetHeaderFooterConfiguration(footer.ToString());
            Assert.AreEqual(1, headerConfig.Count());
            Assert.AreEqual("Updated Header", headerConfig.FirstOrDefault().DisplayName);
            Assert.AreEqual(data.FirstOrDefault().DataID, headerConfig.FirstOrDefault().DataID);
            Assert.AreEqual(HeaderFooterColumnSize.Large, headerConfig.FirstOrDefault().SizeID);
            Assert.IsFalse(headerConfig.FirstOrDefault().IsWordwrapped);
            Assert.AreEqual(1, headerConfig.FirstOrDefault().RowNumber);
            Assert.AreEqual(HeaderFooterConfigurationType.Footer, headerConfig.FirstOrDefault().HeaderFooterConfig);

            //Remove Header
            bool rcheck = ComponentService.RemoveHeaderFooterConfiguration(headerId);
            Assert.IsTrue(rcheck, "Failed to remove Header footer config");
            headerConfig = ComponentService.GetHeaderFooterConfiguration(header.ToString());
            Assert.AreEqual(0, headerConfig.Count());
            headerConfig = ComponentService.GetHeaderFooterConfiguration(footer.ToString());
            Assert.AreEqual(0, headerConfig.Count());
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void TitleforHeader()
        {
            //Data for Title
            HeaderFooterDataDTO[] data = ComponentService.GetDataForHeaderFooter();
            Assert.AreNotEqual(0, data.Count(), "No data obtained for the Header footer Configuration");
            //Title Info
            HeaderFooterConfigDTO titleInfo = new HeaderFooterConfigDTO();
            titleInfo.HeaderFooterConfig = HeaderFooterConfigurationType.Title;
            titleInfo.DisplayName = "Automated Title";
            titleInfo.DataID = data.FirstOrDefault().DataID;
            //Add title
            string titleId = ComponentService.AddUpdateHeaderFooterConfiguration(titleInfo);
            Assert.IsNotNull(titleId, "Failed to add Title");
            HeaderFooterConfigDTO addedTitle = ComponentService.GetTitleOfHeaderFooterConfiguration();
            Assert.IsNotNull(addedTitle, "Failed to retrieve added title");
            Assert.AreEqual("Automated Title", addedTitle.DisplayName);
            Assert.AreEqual(data.FirstOrDefault().DataID, addedTitle.DataID);
            Assert.AreEqual(HeaderFooterConfigurationType.Title, addedTitle.HeaderFooterConfig);
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void GetPullReportForFailureHistory()
        {
            WellConfigDTO well = AddWell("RPOC_");
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();

            string jobId = AddJob("Planned");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO wbevt = SetEventDTO(getJob, (int)JobEventTypes.WellBoreReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO wbevt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long wbcheck = JobAndEventService.AddEventForJobEventType(wbevt);
            Assert.IsTrue(wbcheck > 0, "Failed to add Wellbore Report");

            //Add Component
            ComponentMetaDataGroupDTO[] arrComponent = AddComponent(jobId, wellId, assemblyId, subassemblyId, wbcheck);
            bool addComp = ComponentService.AddComponent(arrComponent);
            Assert.IsTrue(addComp);

            string failCompJobId = AddJob("Planned", 31);
            JobLightDTO getFailedJob = JobAndEventService.GetJobById(failCompJobId);
            EventDTO evt = SetEventDTO(getFailedJob, jobEventTypes.FailureReport);
            var check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Failure Report");

            //Get Failure Component
            JobComponentFailureDTO[] Comps = JobAndEventService.GetJobComponentFailure(failCompJobId);
            Assert.IsNotNull(Comps, "Failed to get components");
            Assert.AreEqual(1, Comps.Count());

            //Get Metadata for observation
            JobComponentFailureObservationDTO mdFailObserv = JobAndEventService.GetMetaDataForAddObservation(Comps.FirstOrDefault().AssemblyComponentId.ToString(), failCompJobId);
            Assert.IsNotNull(mdFailObserv, "Failed to get Observation for the added component");
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue = true;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFailureDate").DataValue = DateTime.Today.ToString();
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfRemarks").DataValue = "Failed Comp";
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFK_r_ComponentDisposition").DataValue = 2;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFK_r_FailureInternalExternal").DataValue = 315;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFK_r_FailureObservation").DataValue = 1614;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFK_r_FailureLocation").DataValue = 776;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFailedDepth").DataValue = 6;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFailedJoint").DataValue = 8;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFK_r_FailureObservation").DataValue = 1614;
            mdFailObserv.PrimaryCauseFailures.FirstOrDefault(x => x.ColumnName == "wsyFK_r_FailureSolution").DataValue = 41;
            mdFailObserv.PrimaryCauseFailures.FirstOrDefault(x => x.ColumnName == "wsyFK_r_PrimaryFailureClass").DataValue = 2;
            mdFailObserv.PrimaryCauseFailures.FirstOrDefault(x => x.ColumnName == "wsyFK_r_RootFailureCause").DataValue = 11;

            //Add observation
            string failObservationCompId = JobAndEventService.AddObservation(mdFailObserv);
            Assert.IsNotNull(failObservationCompId);

            //Get Observation
            JobComponentFailureObservationDTO[] getFailureComponent = JobAndEventService.GetObservationsByAssemblyComponentId(Comps.FirstOrDefault().AssemblyComponentId.ToString(), jobId);

            string failureEventCount = "3";

            DetailAssemblyComponentFailureDTO[] getFailureHistory = JobAndEventService.GetFailureHistory(assemblyId, failureEventCount);

            int count = 0;
            foreach (DetailAssemblyComponentFailureDTO ftd in getFailureHistory)
            {
                Trace.WriteLine("Job Id : " + ftd.JobId);
                Trace.WriteLine("Failure Date : " + ftd.FailureDate);
                Trace.WriteLine("Failed Component: " + ftd.ComponentNameRemarks);
                Trace.WriteLine("Observation : " + ftd.StrFailureObservation);
                Trace.WriteLine("Failure Location : " + ftd.StrFailureLocation);
                Trace.WriteLine("Depth : " + ftd.FailedDepth);
                Trace.WriteLine("Joint : " + ftd.FailedJoint);
                Trace.WriteLine("Primary Detail : " + ftd.StrFailureObservation);
                Trace.WriteLine("Root Cause : " + ftd.RootCause);
                Trace.WriteLine("-------------------------------------------");
                count = count + 1;
            }

            Assert.AreEqual(failCompJobId, getFailureHistory[0].JobId.ToString(), "JobId is not matching");
            Assert.IsNotNull(getFailureHistory[0].StrFailureObservation.ToUpper(), "Observation is not populated");
            Assert.IsNotNull(getFailureHistory[0].StrFailureLocation.ToUpper(), "Failure location is not populated");
            Assert.AreEqual(6m, getFailureHistory[0].FailedDepth, "Failed depth is not matching");
            Assert.AreEqual(8m, getFailureHistory[0].FailedJoint, "Failed joint is not matching");
            Assert.IsNotNull(getFailureHistory[0].StrFailureObservation.ToUpper(), "Primary detail is not populated");
            Assert.IsNotNull(getFailureHistory[0].RootCause.ToUpper(), "Root cause is not populated");
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void VerifyFailureStatusColumnForWellbore()
        {
            WellConfigDTO well = AddWell("RPOC_");

            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();

            string jobId = AddJob("Planned");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO wbevt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long wbcheck = JobAndEventService.AddEventForJobEventType(wbevt);
            Assert.IsTrue(wbcheck > 0, "Failed to add Wellbore Report");

            //Add Component
            ComponentMetaDataGroupDTO[] arrComponent = AddComponent(jobId, wellId, assemblyId, subassemblyId, wbcheck);
            bool addComp = ComponentService.AddComponent(arrComponent);
            Assert.IsTrue(addComp);

            string failCompJobId = AddJob("Planned", 31);
            JobLightDTO getFailedJob = JobAndEventService.GetJobById(failCompJobId);
            EventDTO evt = SetEventDTO(getFailedJob, jobEventTypes.FailureReport);
            var check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Failure Report");

            //Get Failure Component
            JobComponentFailureDTO[] Comps = JobAndEventService.GetJobComponentFailure(failCompJobId);
            Assert.IsNotNull(Comps, "Failed to get components");
            //Assert.AreEqual(1, Comps.Count());

            //Get Metadata for observation
            JobComponentFailureObservationDTO mdFailObserv = JobAndEventService.GetMetaDataForAddObservation(Comps.LastOrDefault().AssemblyComponentId.ToString(), failCompJobId);
            Assert.IsNotNull(mdFailObserv, "Failed to get Observation for the added component");
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue = true;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFailureDate").DataValue = DateTime.Today.AddDays(-2).ToString();
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfRemarks").DataValue = "Failed Comp";
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFK_r_ComponentDisposition").DataValue = 2;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFK_r_FailureInternalExternal").DataValue = 315;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFK_r_FailureObservation").DataValue = 1614;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFK_r_FailureLocation").DataValue = 776;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFailedDepth").DataValue = 6;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFailedJoint").DataValue = 8;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFK_r_FailureObservation").DataValue = 1614;
            mdFailObserv.PrimaryCauseFailures.FirstOrDefault(x => x.ColumnName == "wsyFK_r_FailureSolution").DataValue = 41;
            mdFailObserv.PrimaryCauseFailures.FirstOrDefault(x => x.ColumnName == "wsyFK_r_PrimaryFailureClass").DataValue = 2;
            mdFailObserv.PrimaryCauseFailures.FirstOrDefault(x => x.ColumnName == "wsyFK_r_RootFailureCause").DataValue = 11;
            //mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFK_AssemblyComponent").DataValue = true;
            //Add observation
            string failObservationCompId = JobAndEventService.AddObservation(mdFailObserv);
            Assert.IsNotNull(failObservationCompId);

            //Get Observation
            JobComponentFailureObservationDTO[] getFailureComponent = JobAndEventService.GetObservationsByAssemblyComponentId(Comps.FirstOrDefault().AssemblyComponentId.ToString(), jobId);

            WellboreGridDTO[] wellboreComponents = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, wbcheck.ToString());

            WellboreGridGroupDTO[] retrievedComponents = wellboreComponents[0].WellboreGridGroup;

            foreach (WellboreGridGroupDTO component in retrievedComponents)
            {
                var retrievedCompname = component.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required").Fields.FirstOrDefault(x => x.ColumnName == "FailureStatus").DataValue;

                if (retrievedCompname != null)
                {
                    Assert.AreEqual(retrievedCompname, "Failed");
                    Trace.WriteLine(retrievedCompname + " Failed Component ");
                }
                else
                {
                    Trace.WriteLine(retrievedCompname + " Component doesn't Failed");
                }
            }
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void GetWellboreComponentCorrection()
        {
            //Add well
            WellConfigDTO well = AddWell("RPOC_");
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();

            //Add job to well
            string jobId = AddJob("Planned");
            JobLightDTO getJobs = JobAndEventService.GetJobById(jobId);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO wbevt = SetEventDTO(getJobs, jobEventTypes.WellBoreReport);
            long wbcheck = JobAndEventService.AddEventForJobEventType(wbevt);
            Assert.IsTrue(wbcheck > 0, "Failed to add Wellbore Report");

            //Add Component to the job
            ComponentMetaDataGroupDTO[] arrComponent = AddComponent(jobId, wellId, assemblyId, subassemblyId, wbcheck);
            bool addComp = ComponentService.AddComponent(arrComponent);
            Assert.IsTrue(addComp);

            //Adding data to Values
            HistoricalWellboreDTO historicalJobWellbore = new HistoricalWellboreDTO();

            HistoricalWellboreDTO[] historicalReports = ComponentService.GetHistoricalWellboreComponents(subassemblyId);
            int count = 0;
            foreach (HistoricalWellboreDTO wellboreCorr in historicalReports)
            {
                Trace.WriteLine("AssemblyComponentId : " + wellboreCorr.AssemblyComponentId);
                Trace.WriteLine("ComponentGroupId : " + wellboreCorr.ComponentGroupId);
                Trace.WriteLine("BottomDepth: " + wellboreCorr.BottomDepth);
                Trace.WriteLine("StartDate : " + wellboreCorr.StartDate);
                Trace.WriteLine("PartType : " + wellboreCorr.PartType);
                Trace.WriteLine("Remarks : " + wellboreCorr.Remarks);
                Trace.WriteLine("PermanantRemarks: " + wellboreCorr.PermanantRemarks);
                Trace.WriteLine("TopDepth : " + wellboreCorr.TopDepth);
                Trace.WriteLine("ComponentName : " + wellboreCorr.ComponentName);
                Trace.WriteLine("-------------------------------------------");
                count = count + 1;
            }

            //Checking and asserting the values
            Assert.IsNotNull(historicalReports, "Failed to retrieve Job with details");
            Assert.AreEqual(500, historicalReports[0].BottomDepth, "Bottom depth is not matching");
            Assert.AreEqual("Transformer", historicalReports[0].ComponentName, "Component name is not matching");
            Assert.AreEqual("Surface Equipment", historicalReports[0].ComponentGrouping, "Component grouping is not matching");
            Assert.IsNotNull(historicalReports[0].StartDate, "Start date is not null");
            Assert.IsNotNull(historicalReports[0].EndDate, "End date not matching");
            Assert.AreEqual("*", historicalReports[0].Remarks, "Remarks are not matching");
            Assert.AreEqual("*", historicalReports[0].PermanantRemarks, "Permanent Remarks are not matching");
            Assert.AreEqual("Transformer", historicalReports[0].PartType, "Part type is not matching");
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void AddMultipleLayerComponentCRUD()
        {
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            ComponentFilterDTO partfilter2 = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Borehole").ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes2 = ComponentService.GetPartTypes(partfilter);
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");
            LayerComponentPartTypeMappingDTO reqComponents = new LayerComponentPartTypeMappingDTO();
            LayerComponentPartTypeMappingDTO[] getAllLayers = null;
            LayerComponentPartTypeMappingDTO[] getlayerDetails = null;
            reqComponents.ComponentTypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptgFK_c_MfgCat_ComponentGrouping;
            reqComponents.LayerName = "TestLayerName01";
            reqComponents.LayerNameId = 0;
            reqComponents.selectedPartTypeId = new long[] { (partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType), (partTypes.FirstOrDefault(j => j.ptyPartType == "PC Pump-Brake").ptgFK_c_MfgCat_PartType), (partTypes.FirstOrDefault(j => j.ptyPartType == "Sheave").ptgFK_c_MfgCat_PartType), (partTypes2.FirstOrDefault(j => j.ptyPartType == "Whipstock").ptgFK_c_MfgCat_PartType) };
            try
            {
                getAllLayers = ComponentService.GetAllLayers();
                Assert.AreEqual(0, getAllLayers.Count(), "A layer(s) is already created. Delete this/these layer(s) and test " + getAllLayers);
                bool saveMultilayer = ComponentService.SaveMultiLayerDetails(reqComponents);
                Assert.IsTrue(saveMultilayer, "Need Defect, Unable to save Multiple layer");
                getAllLayers = ComponentService.GetAllLayers();
                getlayerDetails = ComponentService.GetLayerDetails(getAllLayers.FirstOrDefault(x => x.LayerName == "TestLayerName01").LayerNameId.ToString());
                Assert.AreEqual(4, getlayerDetails.Count(), "Created layer doesn't have all the details");
                getAllLayers = ComponentService.GetAllLayers();
                bool deletelayer = ComponentService.DeleteLayer(getAllLayers.FirstOrDefault(x => x.LayerName == "TestLayerName01").LayerNameId.ToString());
                getlayerDetails = ComponentService.GetLayerDetails(getAllLayers.FirstOrDefault(x => x.LayerName == "TestLayerName01").LayerNameId.ToString());
                Assert.AreEqual(0, getlayerDetails.Count(), "Unable to delete created later");
            }
            catch (AssertFailedException ex)
            {
                getAllLayers = ComponentService.GetAllLayers();
                if (getAllLayers.Count() != 0)
                {
                    bool deletelayer = ComponentService.DeleteLayer(getAllLayers.FirstOrDefault(x => x.LayerName == "TestLayerName01").LayerNameId.ToString());
                }
                Assert.Fail(ex + " is the failure line. Please debug the code");
            }
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void HeaderFooterOnWellboreReportVerification()
        {
            WellConfigDTO well = AddWell("RPOC_");

            string jobId = AddJob("Planned");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO wbevt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long wbcheck = JobAndEventService.AddEventForJobEventType(wbevt);
            Assert.IsTrue(wbcheck > 0, "Failed to add Wellbore Report");

            //Add Component
            ComponentMetaDataGroupDTO[] arrComponent = AddComponent(jobId, well.Well.Id.ToString(), well.Well.AssemblyId.ToString(), well.Well.SubAssemblyId.ToString(), wbcheck);
            bool addComp = ComponentService.AddComponent(arrComponent);
            Assert.IsTrue(addComp);

            GetHeaderFooterForWellboreReport(well, getJob, wbcheck.ToString(), "Header");
            GetHeaderFooterForWellboreReport(well, getJob, wbcheck.ToString(), "Footer");
        }

        public void GetHeaderFooterForWellboreReport(WellConfigDTO well, JobLightDTO job, string eventId, string headerFooter)
        {
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();

            int headerFooterId = 0;
            if (headerFooter == "Header")
            {
                headerFooterId = (int)HeaderFooterConfigurationType.Header;
            }
            else
            {
                headerFooterId = (int)HeaderFooterConfigurationType.Footer;
            }

            //Get Header Config
            HeaderFooterConfigDTO[] headerFooterConfig = ComponentService.GetHeaderFooterConfiguration(headerFooterId.ToString());
            Assert.AreEqual(0, headerFooterConfig.Count());

            //Data for Header Footer
            HeaderFooterDataDTO[] data = ComponentService.GetDataForHeaderFooter();
            Assert.AreNotEqual(0, data.Count(), "No data obtained for the Header footer Configuration");
            //Header Info of Row1
            HeaderFooterConfigDTO headerInfo1 = new HeaderFooterConfigDTO();
            headerInfo1.RowNumber = 1;
            headerInfo1.SizeID = HeaderFooterColumnSize.Large;
            headerInfo1.HeaderFooterConfig = (HeaderFooterConfigurationType)headerFooterId;
            headerInfo1.DisplayName = "Automated Header";
            headerInfo1.DataID = data.FirstOrDefault(x => x.DataName == "Unique Well Name").DataID;
            headerInfo1.Order = 1;
            headerInfo1.IsWordwrapped = false;
            //Add Row1 Header
            string headerFooterId1 = ComponentService.AddUpdateHeaderFooterConfiguration(headerInfo1);
            Assert.IsNotNull(headerFooterId1);
            _headerFooterRowsToRemove.Add(headerFooterId1);

            //Adding One medium size item in RowNumber2 at Position2
            HeaderFooterConfigDTO headerInfo2 = new HeaderFooterConfigDTO();
            headerInfo2.RowNumber = 2;
            headerInfo2.SizeID = HeaderFooterColumnSize.Medium;
            headerInfo2.HeaderFooterConfig = (HeaderFooterConfigurationType)headerFooterId;
            headerInfo2.DisplayName = "Automated Header2";
            headerInfo2.DataID = data.FirstOrDefault(x => x.DataName == "Unique Wellbore Hole Identification (API12)").DataID;
            headerInfo2.Order = 2;
            headerInfo2.IsWordwrapped = true;
            //Add Row2 Header
            string headerFooterId2 = ComponentService.AddUpdateHeaderFooterConfiguration(headerInfo2);
            Assert.IsNotNull(headerFooterId2);
            _headerFooterRowsToRemove.Add(headerFooterId2);

            //Adding one small size item in RowNumber2 at Position4
            HeaderFooterConfigDTO headerInfo3 = new HeaderFooterConfigDTO();
            headerInfo3.RowNumber = 2;
            headerInfo3.SizeID = HeaderFooterColumnSize.Small;
            headerInfo3.HeaderFooterConfig = (HeaderFooterConfigurationType)headerFooterId;
            headerInfo3.DisplayName = "Automated Header3";
            headerInfo3.DataID = data.FirstOrDefault(x => x.DataName == "Unique Wellbore Identification (API10)").DataID;
            headerInfo3.Order = 4;
            headerInfo3.IsWordwrapped = true;
            string headerFooterId3 = ComponentService.AddUpdateHeaderFooterConfiguration(headerInfo3);
            Assert.IsNotNull(headerFooterId3);
            _headerFooterRowsToRemove.Add(headerFooterId3);

            List<HeaderFooterConfigDTO> headerRows = new List<HeaderFooterConfigDTO>();
            headerRows.Add(headerInfo1);
            headerRows.Add(headerInfo2);
            headerRows.Add(headerInfo3);

            WellboreHeaderFooterInputDTO input = new WellboreHeaderFooterInputDTO();
            input.AssemblyId = assemblyId;
            input.JobId = job.JobId.ToString();
            input.EventId = eventId;
            input.SubAssemblyId = subassemblyId;
            input.TypeId = headerFooterId.ToString();
            input.WellId = wellId;

            WellboreReportHeaderFooterRowDTO[] resHeaderConfig = ComponentService.GetHeaderFooterForWellBoreReport(input);

            //headerConfig = ComponentService.GetHeaderFooterConfiguration(header.ToString());
            Assert.AreEqual(2, resHeaderConfig.Count());

            //Verifying content of 1st row
            Assert.AreEqual(headerInfo1.SizeID.ToString(), resHeaderConfig[0].HeaderFooterColumnDTO[0].ColumnSize);
            Assert.AreEqual(headerInfo1.DisplayName, resHeaderConfig[0].HeaderFooterColumnDTO[0].Label);
            Assert.AreEqual(headerInfo1.Order, resHeaderConfig[0].HeaderFooterColumnDTO[0].ColumnOrder);
            Assert.AreEqual(well.Well.Name, resHeaderConfig[0].HeaderFooterColumnDTO[0].Value.ToString());

            //Verifying content of 2nd row
            //Verifying the content of item at Position 2
            Assert.AreEqual(headerInfo2.SizeID.ToString(), resHeaderConfig[1].HeaderFooterColumnDTO[0].ColumnSize);
            Assert.AreEqual(headerInfo2.DisplayName, resHeaderConfig[1].HeaderFooterColumnDTO[0].Label);
            Assert.AreEqual(headerInfo2.Order, resHeaderConfig[1].HeaderFooterColumnDTO[0].ColumnOrder);
            Assert.AreEqual(well.Well.SubAssemblyAPI, resHeaderConfig[1].HeaderFooterColumnDTO[0].Value.ToString());

            //Verifying the content of item at Position 4
            Assert.AreEqual(headerInfo3.SizeID.ToString(), resHeaderConfig[1].HeaderFooterColumnDTO[1].ColumnSize);
            Assert.AreEqual(headerInfo3.DisplayName, resHeaderConfig[1].HeaderFooterColumnDTO[1].Label);
            Assert.AreEqual(headerInfo3.Order, resHeaderConfig[1].HeaderFooterColumnDTO[1].ColumnOrder);
            Assert.AreEqual(well.Well.AssemblyAPI, resHeaderConfig[1].HeaderFooterColumnDTO[1].Value.ToString());

            //Since the first item of 2nd row is starting from Positio2, Position 1 should be empty with Small Size
            Assert.AreEqual(HeaderFooterColumnSize.Small.ToString(), resHeaderConfig[1].HeaderFooterColumnDTO[2].ColumnSize);
            Assert.AreEqual("", resHeaderConfig[1].HeaderFooterColumnDTO[2].Label);
            Assert.AreEqual(0, resHeaderConfig[1].HeaderFooterColumnDTO[2].ColumnOrder);
        }

        public bool addMultiLayer(string layerName, string compGrpName, string[] partTypeName)
        {
            //Add Components
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");

            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compGrpName).ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");

            LayerComponentPartTypeMappingDTO layer1 = new LayerComponentPartTypeMappingDTO();
            layer1.ComponentTypeId = partfilter.TypeId;
            layer1.LayerName = layerName;
            List<long> selectedPartTypeId = new List<long>();

            foreach (string partType in partTypeName)
            {
                long partType1 = partTypes.FirstOrDefault(x => x.ptyPartType == partType).ptgFK_c_MfgCat_PartType;
                selectedPartTypeId.Add(partType1);
            }
            layer1.selectedPartTypeId = selectedPartTypeId.ToArray();
            return ComponentService.SaveMultiLayerDetails(layer1);
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void WellBoreDiagramMultiLayer()
        {
            WellConfigDTO well = AddWell("RPOC_");
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);

            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");

            LayerComponentPartTypeMappingDTO[] layersdto = ComponentService.GetAllLayers();
            //CleanUp Layer Records Before starting
            foreach (var indlayer in layersdto)
            {
                bool deletelayer = ComponentService.DeleteLayer(indlayer.LayerNameId.ToString());
                Assert.IsTrue(deletelayer);
            }

            //Adding Layer in Field Service Configuration
            addMultiLayer("Layer1 - Tubing Casing", "Tubing String", new string[] { "Tubing - OD  2.875" });
            addMultiLayer("Layer2 - Conductor Casing", "Conductor Casing", new string[] { "Casing/Casing Liner OD 14.000" });
            addMultiLayer("Layer3 - Production Casing", "Production Casing", new string[] { "Casing/Casing Liner OD  7.000" });

            //Add Component to WellBore
            //Tubing String(catDescriptionID,ID,OD,quantity,length,TopDepth,BottomDepth)
            decimal[] detailsTubing = { 27158, 2.441m, 2.875m, 31, 885.05m, 101, 986.05m, 279 };
            ComponentMetaDataGroupDTO[] tubingString = AddTubingString("Tubing String", "Tubing - OD  2.875", wellId, assemblyId, jobId, detailsTubing, check);
            bool addTubing = ComponentService.AddComponent(tubingString);
            Assert.IsTrue(addTubing);

            //Conductor Casing
            decimal[] detailsConductorCasing = { 10294, 12.5m, 14m, 1, 11, 0, 11, 279 };
            ComponentMetaDataGroupDTO[] conductorCasing = AddTubingString("Conductor Casing", "Casing/Casing Liner OD 14.000", wellId, assemblyId, jobId, detailsConductorCasing, check);
            bool addConductorCasing = ComponentService.AddComponent(conductorCasing);
            Assert.IsTrue(addConductorCasing);

            //Production Casing -- Casing/Casing Liner OD  7.000
            decimal[] detailsprodCasing = { 23902, 6.366m, 7m, 45, 1353, 0, 1353, 279 };
            ComponentMetaDataGroupDTO[] prodCasing = AddTubingString("Production Casing", "Casing/Casing Liner OD  7.000", wellId, assemblyId, jobId, detailsprodCasing, check);
            bool addprodCasingOD7 = ComponentService.AddComponent(prodCasing);
            Assert.IsTrue(addprodCasingOD7);

            //Get No Of Added Layers in Configuration and Prepareing List
            layersdto = ComponentService.GetAllLayers();

            ComponentVerticalSchematicInputDTO inputSchematic = new ComponentVerticalSchematicInputDTO();
            inputSchematic.AssemblyId = Convert.ToInt64(assemblyId);
            inputSchematic.SubAssemblyId = Convert.ToInt64(subassemblyId);
            inputSchematic.EndDate = getJob.EndDate.ToLocalTime();
            inputSchematic.EventId = check;
            inputSchematic.JobId = Convert.ToInt64(jobId);

            //Get Default Verticle Schematic Component Count
            ComponentVerticalSchematicDTO[] componentSchematicDefault = ComponentService.GetComponentVerticalSchematic(inputSchematic);
            Assert.AreEqual(3, componentSchematicDefault.Count(), "Mistmatch Component Count Between WellBoreGrid and WellBore Diagram");

            // inputSchematic.LayerIdsToHide = layerIdList.ToArray();

            //Select 1st Layer and Checking Count
            inputSchematic.LayerIdsToHide = new long[] { layersdto[0].LayerNameId };
            ComponentVerticalSchematicDTO[] componentSchematicSelectedLayer1 = ComponentService.GetComponentVerticalSchematic(inputSchematic);
            Assert.AreEqual(2, componentSchematicSelectedLayer1.Count(), "Mistmatch Component Count Between Selected and Actual");

            //Select 2nd Layer and Checking Count
            inputSchematic.LayerIdsToHide = new long[] { layersdto[0].LayerNameId, layersdto[1].LayerNameId };
            ComponentVerticalSchematicDTO[] componentSchematicSelectedLayer2 = ComponentService.GetComponentVerticalSchematic(inputSchematic);
            Assert.AreEqual(1, componentSchematicSelectedLayer2.Count(), "Mistmatch Component Count Between Selected and Actual");

            //All Layer selected and Checking Count
            inputSchematic.LayerIdsToHide = new long[] { layersdto[0].LayerNameId, layersdto[1].LayerNameId, layersdto[2].LayerNameId };
            ComponentVerticalSchematicDTO[] componentSchematicSelectedLayer3 = ComponentService.GetComponentVerticalSchematic(inputSchematic);
            Assert.AreEqual(0, componentSchematicSelectedLayer3.Count(), "Mistmatch Component Count Between Selected and Actual");

            //CleanUp Layer Records From Configuration
            foreach (var indlayer in layersdto)
            {
                bool deletelayer = ComponentService.DeleteLayer(indlayer.LayerNameId.ToString());
                Assert.IsTrue(deletelayer);
            }
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void AddMultipleDrillingReports()
        {
            WellConfigDTO well = AddWell("RPOC_");
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();
            //Creating a Job
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            //Adding Drilling Reports
            EventDTO evt1 = SetEventDTO(getJob, jobEventTypes.DrillingReport);
            evt1.BeginTime = TimeZoneInfo.ConvertTimeToUtc(well.Well.CommissionDate.Value.AddDays(-6), TimeZoneInfo.Local);
            long addevent = JobAndEventService.AddEventForJobEventType(evt1);
            AddComponentstoDrillingReport(jobId, wellId, assemblyId, subassemblyId, addevent);
            AddComponentstoDrillingReport(jobId, wellId, assemblyId, subassemblyId, addevent);
            EventDTO evt2 = SetEventDTO(getJob, jobEventTypes.DrillingReport);
            evt2.BeginTime = TimeZoneInfo.ConvertTimeToUtc(well.Well.CommissionDate.Value.AddDays(-7), TimeZoneInfo.Local);
            JobAndEventService.AddEventForJobEventType(evt2);
            IEnumerable<EventDTO> evntslist = JobAndEventService.GetEventsByJobIdAndEventTypeId(jobId, evt1.EventTypeId.ToString());
            Assert.IsTrue(evntslist.Count() > 1, "Multiple drilling report could not be added");
            //Removing 2nd Report
            JobAndEventService.RemoveEvent(evntslist.ElementAt(1));
            IEnumerable<EventDTO> evntslist2 = JobAndEventService.GetEventsByJobIdAndEventTypeId(jobId, evt1.EventTypeId.ToString());
            Assert.IsTrue(evntslist2.Count() == 1, "Drilling report could not be removed");
        }

        public void AddComponentstoDrillingReport(string jobId, string wellId, string assemblyId, string subassemblyId, long addevent)
        {
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");
            ComponentMetaDataGroupDTO[] arrComponent = AddComponent(jobId, wellId, assemblyId, subassemblyId, addevent);
            ComponentPartTypeDTO details = new ComponentPartTypeDTO();
            details.JobId = Convert.ToInt64(jobId);
            details.ComponentId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptgFK_c_MfgCat_ComponentGrouping;
            details.PartTypeId = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;
            ComponentMetaDataGroupBatchCollectionDTO batchDetailsComp = new ComponentMetaDataGroupBatchCollectionDTO();
            batchDetailsComp.ActionPerformed = CRUDOperationTypes.Add;
            batchDetailsComp.ComponentMetadataCollection = arrComponent;
            batchDetailsComp.ComponentPartType = details;
            List<ComponentMetaDataGroupBatchCollectionDTO> listComp = new List<ComponentMetaDataGroupBatchCollectionDTO>();
            listComp.Add(batchDetailsComp);
            ComponentMetaDataGroupBatchCollectionDTO[] arrComp = listComp.ToArray();
            //Add Batch Component
            bool saveBatch = ComponentService.SaveWellboreComponent(arrComp);
            Assert.IsTrue(saveBatch);
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void Wellbore_FilterComponents()
        {
            WellConfigDTO well = AddWell("RPOC_");
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            //Tubing String(catDescriptionID,ID,OD,quantity,length,TopDepth,BottomDepth)
            decimal[] detailsTubing = { 27158, 2.441m, 2.875m, 31, 885.05m, 101, 986.05m, 279 };
            ComponentMetaDataGroupDTO[] tubingString = AddTubingString("Tubing String", "Tubing - OD  2.875", wellId, assemblyId, jobId, detailsTubing, check);
            bool addTubing = ComponentService.AddComponent(tubingString);
            Assert.IsTrue(addTubing);
            //Conductor Casing
            decimal[] detailsConductorCasing = { 10294, 12.5m, 14m, 1, 11, 0, 11, 279 };
            ComponentMetaDataGroupDTO[] conductorCasing = AddTubingString("Conductor Casing", "Casing/Casing Liner OD 14.000", wellId, assemblyId, jobId, detailsConductorCasing, check);
            bool addConductorCasing = ComponentService.AddComponent(conductorCasing);
            Assert.IsTrue(addConductorCasing);
            //Production Casing -- Casing/Casing Liner OD  7.000
            decimal[] detailsprodCasing = { 23902, 6.366m, 7m, 45, 1353, 0, 1353, 279 };
            ComponentMetaDataGroupDTO[] prodCasing = AddTubingString("Production Casing", "Casing/Casing Liner OD  7.000", wellId, assemblyId, jobId, detailsprodCasing, check);
            bool addprodCasingOD7 = ComponentService.AddComponent(prodCasing);
            Assert.IsTrue(addprodCasingOD7);
            //Production Liner -- Casing/Casing Liner OD  5.500
            decimal[] detailsprodCasingLiner = { 1967, 4.892m, 5.5m, 7, 224, 1085, 1309, 279 };
            ComponentMetaDataGroupDTO[] casingLiner = AddTubingString("Production Liner", "Casing/Casing Liner OD  5.500", wellId, assemblyId, jobId, detailsprodCasingLiner, check);
            bool addcasingLiner = ComponentService.AddComponent(casingLiner);
            Assert.IsTrue(addcasingLiner);
            //Wellbore Notes
            decimal[] detailswn1 = { 1793, 0, 0, 1, 0.01m, 1366, 1366.01m, 279 };
            ComponentMetaDataGroupDTO[] wn1 = AddTubingString("Wellbore Notes", "Wellbore Notes", wellId, assemblyId, jobId, detailswn1, check);
            bool addwn1 = ComponentService.AddComponent(wn1);
            Assert.IsTrue(addwn1);
            //Borehole
            decimal[] detailsBhole = { 1428, 20, 20, 1, 11, 0, 11, 279 };
            ComponentMetaDataGroupDTO[] bh = AddTubingString("Borehole", "Wellbore Hole", wellId, assemblyId, jobId, detailsBhole, check);
            bool addbh = ComponentService.AddComponent(bh);
            Assert.IsTrue(addbh);
            WellboreGridDTO[] wellboreComponents = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());
            ComponentVerticalSchematicInputDTO inputSchematic = new ComponentVerticalSchematicInputDTO();
            inputSchematic.AssemblyId = Convert.ToInt64(assemblyId);
            inputSchematic.SubAssemblyId = Convert.ToInt64(subassemblyId);
            inputSchematic.EndDate = getJob.EndDate.ToLocalTime();
            inputSchematic.EventId = check;
            inputSchematic.JobId = Convert.ToInt64(jobId);
            ComponentVerticalSchematicDTO[] componentSchematic = ComponentService.GetComponentVerticalSchematic(inputSchematic);

            List<WellboreComponentFilterDTO> compfilterlist = new List<WellboreComponentFilterDTO>();
            //Adding filters to wellbore components.
            compfilterlist.Add(new WellboreComponentFilterDTO { UserId = AuthenticatedUser.Id, ComponentId = componentSchematic[0].ComponentId, ShowImage = true, ShowLabel = true, AlignLeft = true });
            compfilterlist.Add(new WellboreComponentFilterDTO { UserId = AuthenticatedUser.Id, ComponentId = componentSchematic[1].ComponentId, ShowImage = false, ShowLabel = true, AlignLeft = false });
            compfilterlist.Add(new WellboreComponentFilterDTO { UserId = AuthenticatedUser.Id, ComponentId = componentSchematic[2].ComponentId, ShowImage = true, ShowLabel = false, AlignLeft = true });
            compfilterlist.Add(new WellboreComponentFilterDTO { UserId = AuthenticatedUser.Id, ComponentId = componentSchematic[3].ComponentId, ShowImage = true, ShowLabel = false, AlignLeft = true });
            compfilterlist.Add(new WellboreComponentFilterDTO { UserId = AuthenticatedUser.Id, ComponentId = componentSchematic[4].ComponentId, ShowImage = true, ShowLabel = false, AlignLeft = true });
            compfilterlist.Add(new WellboreComponentFilterDTO { UserId = AuthenticatedUser.Id, ComponentId = componentSchematic[5].ComponentId, ShowImage = true, ShowLabel = false, AlignLeft = false });

            ComponentService.PutWellboreComponentFilers(compfilterlist.ToArray());
            WellboreComponentFilterDTO[] f = ComponentService.GetWellboreComponentFilers(AuthenticatedUser.Id.ToString(), inputSchematic.SubAssemblyId.ToString());
            Assert.AreEqual(f.Count(), 6, "Component filters count does not match");

            evt.Id = check;
            bool rcheck = JobAndEventService.RemoveEvent(evt);
            bool rcheck1 = JobAndEventService.RemoveJob(jobId);
            Assert.IsTrue(rcheck1, "Failed to remove Job");
        }
    }
}
