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
    public class WellBoreComponentsKLandKD : APIClientTestBase
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
        //SP - Work in progress : adding more scenario 
        [TestCategory(TestCategories.WellBoreComponentsKLandKDTests), TestMethod]
        public void VerifyknownlengthKnowDepth()
        {
            //Creating a well
            WellDTO well = AddWell("RPOC_");

            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();
            string subassemblyId = well.SubAssemblyId.ToString();

            //Creating a Job
            string jobId = AddJob("Approved");

            //Adding Wellbore tab to Job
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long wllboreEventId = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(wllboreEventId > 0, "Wellbore report tab not added successfully");

            List<TestComponent> componentsData = GetTestComponents(wellId, jobId, wllboreEventId, assemblyId, subassemblyId);
            //Adding Wellbore components. 
            foreach (var comp in componentsData)
            {
                AddComponent(comp, wllboreEventId, subassemblyId);
            }

            WellboreGridDTO[] wellboreComponents = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, wllboreEventId.ToString());
            WellboreGridGroupDTO[] retrievedComponents = wellboreComponents[0].WellboreGridGroup;
            IEnumerable<ComponentMetaDataGroupDTO> components = retrievedComponents.SelectMany(x => x.ComponentMetadata).ToList().Where(x => x.CategoryName == "Required").ToList();

            Assert.AreEqual(components.Count(), 5);

            //Update Component
            bool editFlag = UpdateComponent(jobId, wllboreEventId, "Wellbore Hole", "Borehole", 200, 0, 200);
            Assert.IsTrue(editFlag, "component update fail");

            //Remove Component
            var removeComponent = components.FirstOrDefault(x => x.PartType == "Casing/Casing Liner OD 20.000" && x.CategoryName == "Required");
            if (removeComponent != null)
            {
                bool removeFlag = RemoveComponent(removeComponent, Convert.ToInt32(jobId), wllboreEventId);
                Assert.IsTrue(removeFlag, "component remove fail");
            }
            //Verifying Component 

            WellboreGridDTO[] allComponents = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, wllboreEventId.ToString());

            Assert.AreEqual(allComponents[0].WellboreGridGroup.Count(), 4);
            ComponentMetaDataGroupDTO compWellbore = allComponents.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup.SelectMany(x => x.ComponentMetadata).ToList().FirstOrDefault(x => x.CategoryName == "Required" && x.PartType == "Wellbore Hole");
            Assert.AreEqual(Convert.ToDecimal(200.00), compWellbore.Fields.Single(x => x.ColumnName.ToUpper() == "ASCLENGTH").DataValue);
            Assert.AreEqual(Convert.ToDecimal(200.00), compWellbore.Fields.Single(x => x.ColumnName.ToUpper() == "ASCBOTTOMDEPTH").DataValue);
            Assert.AreEqual(Convert.ToDecimal(0), compWellbore.Fields.Single(x => x.ColumnName.ToUpper() == "ASCTOPDEPTH").DataValue);

            ComponentMetaDataGroupDTO compTubing1 = allComponents.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup.SelectMany(x => x.ComponentMetadata).ToList().FirstOrDefault(x => x.CategoryName == "Required" && x.PartType == "Tubing - OD  7.625");
            Assert.AreEqual(Convert.ToDecimal(100.00), compTubing1.Fields.Single(x => x.ColumnName.ToUpper() == "ASCLENGTH").DataValue);
            Assert.AreEqual(Convert.ToDecimal(100.00), compTubing1.Fields.Single(x => x.ColumnName.ToUpper() == "ASCBOTTOMDEPTH").DataValue);
            Assert.AreEqual(Convert.ToDecimal(0), compTubing1.Fields.Single(x => x.ColumnName.ToUpper() == "ASCTOPDEPTH").DataValue);

            ComponentMetaDataGroupDTO compTubing2 = allComponents.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup.SelectMany(x => x.ComponentMetadata).ToList().FirstOrDefault(x => x.CategoryName == "Required" && x.PartType == "Tubing - OD  6.000");
            Assert.AreEqual(Convert.ToDecimal(100.00), compTubing2.Fields.Single(x => x.ColumnName.ToUpper() == "ASCLENGTH").DataValue);
            Assert.AreEqual(Convert.ToDecimal(200.00), compTubing2.Fields.Single(x => x.ColumnName.ToUpper() == "ASCBOTTOMDEPTH").DataValue);
            Assert.AreEqual(Convert.ToDecimal(100.00), compTubing2.Fields.Single(x => x.ColumnName.ToUpper() == "ASCTOPDEPTH").DataValue);

            ComponentMetaDataGroupDTO compPolishRod = allComponents.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup.SelectMany(x => x.ComponentMetadata).ToList().FirstOrDefault(x => x.CategoryName == "Required" && x.PartType == "Polished Rod");
            Assert.AreEqual(Convert.ToDecimal(100.00), compPolishRod.Fields.Single(x => x.ColumnName.ToUpper() == "ASCLENGTH").DataValue);
            Assert.AreEqual(Convert.ToDecimal(100.00), compPolishRod.Fields.Single(x => x.ColumnName.ToUpper() == "ASCBOTTOMDEPTH").DataValue);
            Assert.AreEqual(Convert.ToDecimal(0), compPolishRod.Fields.Single(x => x.ColumnName.ToUpper() == "ASCTOPDEPTH").DataValue);

        }
        public List<TestComponent> GetTestComponents(string wellId, string jobId, long eventId, string assemblyId, string subassemblyId)
        {
            return new List<TestComponent>()
            {
                new TestComponent (){WellId = wellId,AssemblyId = assemblyId,SubassemblyId = subassemblyId,JobId = jobId,CompGp = "Borehole",
                Partype = "Wellbore Hole",CompName ="", CompRemarks= "",CatalogItemDes ="1428" , CatManufacturers ="279",
                EventId = eventId ,InnerDiameter= 20,OuterDiameter = 20,Quantity = 1,Length = 300,TopDepth = 0,KnownLength = true ,KnownTopDepth = true
                }
                ,new TestComponent (){WellId = wellId,AssemblyId = assemblyId,SubassemblyId = subassemblyId,JobId = jobId,CompGp = "Tubing String",
                Partype = "Tubing - OD  7.625",CompName ="", CompRemarks= "",CatalogItemDes ="1428" , CatManufacturers ="279",
                EventId = eventId ,InnerDiameter= 0,OuterDiameter = 7.625,Quantity = 1,Length = 100,TopDepth = 0,KnownLength = true ,KnownTopDepth = true
                },
                new TestComponent (){WellId = wellId,AssemblyId = assemblyId,SubassemblyId = subassemblyId,JobId = jobId,CompGp = "Tubing String",
                Partype = "Tubing - OD  6.000",CompName ="", CompRemarks= "",CatalogItemDes ="28120" , CatManufacturers ="279",
                EventId = eventId ,InnerDiameter= 0,OuterDiameter = 6.000,Quantity = 1,Length = 100,TopDepth =100,KnownLength = true ,KnownTopDepth = true
                },
                new TestComponent (){WellId = wellId,AssemblyId = assemblyId,SubassemblyId = subassemblyId,JobId = jobId,CompGp = "Surface Casing",
                Partype = "Casing/Casing Liner OD 20.000",CompName ="", CompRemarks= "",CatalogItemDes ="11494" , CatManufacturers ="279",
                EventId = eventId ,InnerDiameter= 0,OuterDiameter = 20.000,Quantity = 1,Length = 300,TopDepth =0,KnownLength = false ,KnownTopDepth = true
                },
                new TestComponent (){WellId = wellId,AssemblyId = assemblyId,SubassemblyId = subassemblyId,JobId = jobId,CompGp = "Rod String",
                Partype = "Polished Rod",CompName ="", CompRemarks= "",CatalogItemDes ="968" , CatManufacturers ="279",
                EventId = eventId ,InnerDiameter= 0,OuterDiameter = 1.2,Quantity = 1,Length = 100,TopDepth =0,KnownLength = true ,KnownTopDepth = true
                }
            };
        }
        public WellDTO AddWell(string facilityIdBase)
        {
            string facilityId = s_isRunningInATS ? facilityIdBase + "00001" : facilityIdBase + "0001";
            WellConfigDTO wellConfig = WellConfigurationService.AddWellConfig(new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = DefaultWellName,
                    FacilityId = facilityId,
                    DataConnection = GetDefaultCygNetDataConnection(),
                    SubAssemblyAPI = "SubAssemblyAPI",
                    AssemblyAPI = "AssemblyAPI",
                    IntervalAPI = "IntervalAPI",
                    CommissionDate = DateTime.Today.AddYears(-3),
                    WellType = WellTypeId.RRL,
                    DepthCorrectionFactor = 50,
                    WellGroundElevation = 50,
                })
            });
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well);
            Assert.AreEqual(50, well.DepthCorrectionFactor, "Unable to save depth correction factor");
            Assert.AreEqual(50, well.WellGroundElevation, "Unable to save Well ground elevation");
            return well;
        }

        public string AddJob(string jobStatus, int startDate = 0)
        {
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));

            JobLightDTO job = new JobLightDTO();
            job.WellId = well.Id;
            job.WellName = well.Name;

            // job.BeginDate = DateTime.Today.AddDays(0);

            job.BeginDate = DateTime.Today.AddDays(startDate).ToUniversalTime();
            if (startDate == 0)
                job.EndDate = DateTime.Today.AddDays(30).ToUniversalTime();
            else
                job.EndDate = DateTime.Today.AddDays(startDate + 30).ToUniversalTime();

            // job.EndDate = DateTime.Today.AddDays(30);
            job.ActualCost = (decimal)100000.00;
            job.ActualJobDurationDays = (decimal)20.5;
            job.TotalCost = (decimal)150000.00;
            job.JobRemarks = "TestJobRemarks - " + jobStatus;
            job.JobOrigin = "TestJobOrigin ";
            job.AssemblyId = well.AssemblyId;
            //For Below fields User can select any value in the dropdown menu
            job.AFEId = JobAndEventService.GetAFEs().FirstOrDefault().Id;
            job.StatusId = JobAndEventService.GetJobStatuses().FirstOrDefault(j => j.Name == jobStatus).Id;
            job.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault(x => x.JobType == "Artificial Lift Revision").id;
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

        public void AddComponent(TestComponent comp, long eventWellbore, string subassemblyId)
        {
            JobLightDTO getJob = JobAndEventService.GetJobById(comp.JobId);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long eventId = eventWellbore;// JobAndEventService.AddEventForJobEventType(evt);

            Assert.IsTrue(eventId > 0, "Failed to add Wellbore Report");

            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");

            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == comp.CompGp).ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");

            ComponentMetaDataGroupDTO[] arrComponent = null;

            arrComponent = AddComponentGeneric(comp, eventId);

            ComponentPartTypeDTO details = new ComponentPartTypeDTO();
            details.JobId = Convert.ToInt64(comp.JobId);
            details.ComponentId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == comp.CompGp).ptgFK_c_MfgCat_ComponentGrouping;
            details.PartTypeId = partTypes.FirstOrDefault(x => x.ptyPartType == comp.Partype).ptgFK_c_MfgCat_PartType;

            ComponentMetaDataGroupBatchCollectionDTO batchDetailsComp = new ComponentMetaDataGroupBatchCollectionDTO();
            batchDetailsComp.ActionPerformed = CRUDOperationTypes.Add;
            batchDetailsComp.ComponentMetadataCollection = arrComponent;
            batchDetailsComp.ComponentPartType = details;

            List<ComponentMetaDataGroupBatchCollectionDTO> listComp = new List<ComponentMetaDataGroupBatchCollectionDTO>();
            listComp.Add(batchDetailsComp);
            ComponentMetaDataGroupBatchCollectionDTO[] arrComp = listComp.ToArray();

            bool saveBatch = ComponentService.SaveWellboreComponent(arrComp);
            Assert.IsTrue(saveBatch);
            Trace.WriteLine("Component Added with Component group :" + comp.CompGp + " and Part type " + comp.Partype);
            Trace.WriteLine("----------------------------------------------------------");
        }

        public bool UpdateComponent(string jobId, long evtId, string partTypeValue, string compGroupValue, decimal length, decimal topDepth, decimal bottomDepth)
        {
            WellboreGridDTO[] wellboreComponent = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, evtId.ToString());
            Assert.IsNotNull(wellboreComponent);
            Assert.AreEqual(1, wellboreComponent.Count());
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");

            string assemblyCompTableName = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compGroupValue).ptyExtendedAssemblyComponentTableName;
            string compTableName = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compGroupValue).ptyExtendedComponentTableName;
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compGroupValue).ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");
            int ptyId = partTypes.FirstOrDefault(x => x.ptyPartType == partTypeValue).ptgFK_c_MfgCat_PartType;
            string partType = partTypes.FirstOrDefault(x => x.ptyPartType == partTypeValue).ptyPartType;
            ComponentMetaDataGroupDTO mdComp = wellboreComponent.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup.SelectMany(x => x.ComponentMetadata).ToList().FirstOrDefault(x => x.CategoryName == "Required" && x.PartType == partTypeValue);
            string cId = mdComp.ComponentPrimaryKey.ToString();
            string ascId = mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascPrimaryKey").DataValue.ToString();
            ComponentMetaDataGroupDTO[] metadataUpdate = ComponentService.GetComponentMetaDataForUpdate(ptyId.ToString(), cId, ascId);
            string catName = metadataUpdate.FirstOrDefault(x => x.CategoryName == "Required").CategoryName;
            foreach (ComponentMetaDataGroupDTO md in metadataUpdate)
            {
                md.JobId = Convert.ToInt64(jobId);
                md.EventId = evtId;
                md.CategoryName = catName;
                md.ComponentGroupingPrimaryKey = partfilter.TypeId;
                md.ExtendedAssemblyComponentTable = assemblyCompTableName;
                md.ExtendedComponentTable = compTableName;
                md.PartTypePrimaryKey = ptyId;
                md.PartType = partType;
            }
            MetaDataDTO[] reqFields = metadataUpdate.FirstOrDefault(x => x.CategoryName == "Required").Fields;
            reqFields.FirstOrDefault(x => x.ColumnName == "ascRemark").DataValue = "Component added by Automation Test";
            reqFields.FirstOrDefault(x => x.ColumnName.ToUpper() == "ASCLENGTH").DataValue = length;
            reqFields.FirstOrDefault(x => x.ColumnName.ToUpper() == "ASCTOPDEPTH").DataValue = topDepth;
            reqFields.FirstOrDefault(x => x.ColumnName.ToUpper() == "ASCBOTTOMDEPTH").DataValue = bottomDepth;
            return ComponentService.UpdateComponent(metadataUpdate);
        }

        protected bool RemoveComponent(ComponentMetaDataGroupDTO mdComp, long jobId, long eventId)
        {
            List<ComponentMetaDataGroupBatchCollectionDTO> listComp = new List<ComponentMetaDataGroupBatchCollectionDTO>();
            ComponentMetaDataGroupBatchCollectionDTO[] arrComp = null;

            ComponentMetaDataGroupBatchCollectionDTO batchDetailsComp = new ComponentMetaDataGroupBatchCollectionDTO();
            batchDetailsComp.ActionPerformed = CRUDOperationTypes.Remove;

            ComponentPartTypeDTO Ids = new ComponentPartTypeDTO();
            //ComponentMetaDataGroupDTO mdComp = comp.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required");
            var ascId = mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascPrimaryKey").DataValue;
            Ids.JobId = Convert.ToInt64(jobId);
            Ids.AssemblyComponentId = Convert.ToInt64(ascId);
            Ids.PartTypeId = (int)mdComp.PartTypePrimaryKey;
            Ids.ComponentId = mdComp.ComponentPrimaryKey;
            Ids.EventId = eventId;
            batchDetailsComp.ComponentPartType = Ids;
            listComp.Add(batchDetailsComp);
            arrComp = listComp.ToArray();

            bool rComp = ComponentService.SaveWellboreComponent(arrComp);
            Assert.IsTrue(rComp);
            //WellboreGridDTO[] wellboreComponent1 = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId.ToString(), eventId.ToString());
            //Assert.IsTrue(wellboreComponent1.Count() == 0, "All components are not removed");
            //Trace.WriteLine("All the components are removed");
            return rComp;
        }

        public ComponentMetaDataGroupDTO[] AddComponentGeneric(TestComponent component, long eventId)
        {
            //Get job details
            JobLightDTO getJob = JobAndEventService.GetJobById(component.JobId);

            //Get Components groups            
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(new ComponentFilterDTO()
            {
                ContainsSearchText = "",
                TypeOfFilter = ComponentFilterTypes.GroupType,
            });
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");

            //Get Typeid using component group
            int typeID = componentGroups.FirstOrDefault(x => x.strComponentGrouping == component.CompGp).ptgFK_c_MfgCat_ComponentGrouping;

            string assemblyCompTableName = componentGroups.FirstOrDefault(x => x.strComponentGrouping == component.CompGp).ptyExtendedAssemblyComponentTableName;
            string compTableName = componentGroups.FirstOrDefault(x => x.strComponentGrouping == component.CompGp).ptyExtendedComponentTableName;
            //Get Part types
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(new ComponentFilterDTO()
            {
                ContainsSearchText = "",
                TypeId = typeID,
                TypeOfFilter = ComponentFilterTypes.GroupType
            });
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");

            //Get Part type id using component part type
            int partTypeID = partTypes.FirstOrDefault(x => x.ptyPartType == component.Partype).ptgFK_c_MfgCat_PartType;
            string partType = partTypes.FirstOrDefault(x => x.ptyPartType == component.Partype).ptyPartType;

            //Get component Meta data using typeid & parttype id
            ComponentMetaDataGroupDTO[] cmpMetaData = ComponentService.GetComponentMetaDataForAdd(partTypeID.ToString(), typeID.ToString());
            Assert.IsNotNull(cmpMetaData);
            Trace.WriteLine("Adding Component with Comp Name : " + component.CompGp + " Part Type : " + component.Partype);
            Trace.WriteLine("Component has " + cmpMetaData.Count() + " categories");

            //string partTypeSpecific = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Part Type Specific").CategoryName;
            //Get Meta data for the Catalog Item description
            // Get Required section
            MetaDataDTO[] cdReference = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;
            MetaDataReferenceData cd = new MetaDataReferenceData();
            cd.MetaData = cdReference.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCatalogItem");
            List<MetaDataFilterDTO> listcdFilter = new List<MetaDataFilterDTO>()
            {
                 new MetaDataFilterDTO(){ColumnValue = partTypeID.ToString() ,MetaDataFilterToken = cd.MetaData.ExtendedFilterInput }
            };
            cd.UIFilterValues = listcdFilter.ToArray();
            //Get meta data referance data
            ControlIdTextDTO[] cdMetaData = JobAndEventService.GetMetaDataReferenceData(cd);
            ControlIdTextDTO cdm = cdMetaData.FirstOrDefault(x => x.ControlText == cdMetaData[0].ControlText);

            long catManufacturerId = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;

            ComponentMetaDataGroupDTO reqComponent = new ComponentMetaDataGroupDTO()
            {
                CategoryName = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").CategoryName,
                JobId = Convert.ToInt64(component.JobId),
                EventId = eventId,
                ComponentGroupingPrimaryKey = typeID,
                ExtendedAssemblyComponentTable = assemblyCompTableName,
                ExtendedComponentTable = compTableName,
                PartTypePrimaryKey = partTypeID,
                Order = 1,
                PartType = partType,
                Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields
            };
            SetRequiredMetaData(component, reqComponent.Fields);
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCatalogItem").DataValue = cdm.ControlId;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_Manufacturers").DataValue = catManufacturerId;

            //set additional data
            ComponentMetaDataGroupDTO addComponent = new ComponentMetaDataGroupDTO()
            {
                CategoryName = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Additional").CategoryName,
                JobId = Convert.ToInt64(component.JobId),
                ComponentGroupingPrimaryKey = typeID,
                PartTypePrimaryKey = partTypeID,
                EventId = eventId,
                Order = 1,
                PartType = partType,
                Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Additional").Fields
            };
            addComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcRemark").DataValue = "Additional Remark";
            addComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascTotalWeight").DataValue = 125;

            List<ComponentMetaDataGroupDTO> listComponent = new List<ComponentMetaDataGroupDTO>()
            {
                reqComponent,
                addComponent
            };
            //set Part type data
            if (cmpMetaData.Count() > 2)
            {
                ComponentMetaDataGroupDTO partTypeSpecificComponent = new ComponentMetaDataGroupDTO()
                {
                    JobId = Convert.ToInt64(component.JobId),
                    ComponentGroupingPrimaryKey = typeID,
                    ExtendedAssemblyComponentTable = assemblyCompTableName,
                    ExtendedComponentTable = compTableName,
                    //CategoryName = partTypeSpecific,
                    Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Part Type Specific").Fields
                };
                listComponent.Add(partTypeSpecificComponent);
            }

            ComponentMetaDataGroupDTO[] arrComponent = listComponent.ToArray();

            return arrComponent;
        }

        public void SetRequiredMetaData(TestComponent component, MetaDataDTO[] cmpMetaData)
        {
            cmpMetaData.FirstOrDefault(x => x.ColumnName == "cmcInnerDiameter").DataValue = component.InnerDiameter;
            cmpMetaData.FirstOrDefault(x => x.ColumnName == "cmcOuterDiameter").DataValue = component.OuterDiameter;
            cmpMetaData.FirstOrDefault(x => x.ColumnName == "ascFK_Assembly").DataValue = component.AssemblyId;
            cmpMetaData.FirstOrDefault(x => x.ColumnName == "ascFK_SubAssembly").DataValue = component.SubassemblyId;
            cmpMetaData.FirstOrDefault(x => x.ColumnName == "ascTopDepth").DataValue = component.TopDepth;
            cmpMetaData.FirstOrDefault(x => x.ColumnName == "ascLength").DataValue = component.Length;
            cmpMetaData.FirstOrDefault(x => x.ColumnName == "ascQuantity").DataValue = component.Quantity;
            decimal length = Convert.ToDecimal(cmpMetaData.FirstOrDefault(x => x.ColumnName == "ascLength").DataValue);
            decimal topDepth = Convert.ToDecimal(cmpMetaData.FirstOrDefault(x => x.ColumnName == "ascTopDepth").DataValue);
            cmpMetaData.FirstOrDefault(x => x.ColumnName == "ascBottomDepth").DataValue = Convert.ToDecimal(length + topDepth);
            cmpMetaData.FirstOrDefault(x => x.ColumnName == "ascKnownLength").DataValue = component.KnownLength;
            cmpMetaData.FirstOrDefault(x => x.ColumnName == "ascKnownTopDepth").DataValue = component.KnownTopDepth;
        }
    }

    public class TestComponent
    {
        public long EventId { get; set; }
        public string WellId { get; set; }
        public string AssemblyId { get; set; }
        public string SubassemblyId { get; set; }
        public string JobId { get; set; }
        public string CompGp { get; set; }
        public string Partype { get; set; }
        public string CompName { get; set; }
        public string CompRemarks { get; set; }
        public double InnerDiameter { get; set; }
        public double OuterDiameter { get; set; }
        public double Quantity { get; set; }
        public double Length { get; set; }
        public double TopDepth { get; set; }
        public bool KnownLength { get; set; }
        public bool KnownTopDepth { get; set; }
        public double KnownLengthVal { get; set; }
        public double KnownTopDepthVal { get; set; }

        public string CatalogItemDes { get; set; }
        public string CatManufacturers { get; set; }



    }
}

