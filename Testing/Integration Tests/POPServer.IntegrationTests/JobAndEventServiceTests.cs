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
    public class JobAndEventServiceTests : APIClientTestBase
    {
        [TestInitialize]
        public override void Init()
        {
            base.Init();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            TemplateJobGroupingDTO[] tjobs = JobAndEventService.GetTemplateJobs();
            if (tjobs != null && tjobs.Count() > 0)
            {
                foreach (TemplateJobGroupingDTO tj in tjobs)
                {
                    foreach (TemplateJobDTO jobs in tj.TemplateJobs)
                    {
                        JobAndEventService.RemoveTemplateJob(jobs.TemplateJobId.ToString());
                    }
                }
            }
            base.Cleanup();
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

        public string AddJobNew(string jobStatus, int startDate = 0)
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
            string addJobNew = JobAndEventService.AddJob(job);
            Assert.IsNotNull(addJobNew, "Failed to add a Job");
            //Get Job
            JobLightDTO getJob = JobAndEventService.GetJobById(addJobNew);
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
            return addJobNew;
        }

        public string[] AddMultiJob(string jobStatus, int TotalJob = 5)
        {
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            int count = 0;
            string[] addJob = new string[TotalJob];

            while (TotalJob > 0)
            {
                JobLightDTO job = new JobLightDTO();
                job.WellId = well.Id;
                job.WellName = well.Name;
                job.BeginDate = DateTime.Today.AddDays(count).ToUniversalTime();
                job.EndDate = DateTime.Today.AddDays(count + 30).ToUniversalTime();
                job.ActualCost = (decimal)100000.00 + count;
                job.ActualJobDurationDays = (decimal)20.5 + count;
                job.TotalCost = (decimal)150000.00 + count;
                job.JobRemarks = "TestJobRemarks_ " + count + "_" + DateTime.UtcNow.ToString();
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
                addJob[count] = JobAndEventService.AddJob(job);
                Assert.IsNotNull(addJob, "Failed to add a Job");

                TotalJob = TotalJob - 1;
                count = count + 1;
            }
            return addJob;
        }

        public MetaDataDTO[] SetMetadataForCleanFillEvent(MetaDataDTO[] eventMetadata, JobLightDTO getJob)
        {
            long id = 0;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_JOB").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.JobId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_AFE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.AFEId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_ASSEMBLY").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.AssemblyId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_BUSINESSORGANIZATION").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 2; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_R_TRUCKUNIT").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = JobAndEventService.GetCatalogItemGroupData().TruckUnits.FirstOrDefault().TruckUnitId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_R_CATALOGITEM").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = JobAndEventService.GetCatalogItemGroupData().CatalogItems.FirstOrDefault().CatalogItemId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_EVENT_PARENT").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 10;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTBEGDTTM").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = DateTime.Today.ToUniversalTime().ToISO8601();
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTENDDTTM").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = DateTime.Today.AddDays(10).ToUniversalTime().ToISO8601();
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCDURATIONHOURS").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.ActualJobDurationDays;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTORDER").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 10;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCWORKORDERID").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "WRKORDID_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFIELDSERVICEORDERID").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "FLDSRVID_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCRESPONSIBLEPERSON").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "RESPONSIBLE_PERSON_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCPERSONPERFORMINGTASK").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "PERFORMING_TASK_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCQUANTITY").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 50m;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCTOTALCOST").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 100m;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCDOCUMENTFILENAME").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "EVCDOCUMENTFILENAME_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCORIGINKEY").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "EVCORIGINKEY_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCUNPLANNED").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = true;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCTROUBLE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = true;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCPREVENTIVEMAINTENANCE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = true;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCREMARKS").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.JobRemarks;

            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "ERFTOOLSIZE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 24;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "ERFTAGGEDFILLDEPTH").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 16;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "ERFCLEANEDTODEPTH").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 8;

            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "ERFFK_R_FILLCLEANINGMETHOD").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 6;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "ERFFK_R_WELLDEPTHDATUM").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 5;
            return eventMetadata;
        }

        public MetaDataDTO[] SetMetadata(MetaDataDTO[] eventMetadata, JobLightDTO getJob)
        {
            long id = 0;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_JOB").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.JobId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_AFE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.AFEId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_ASSEMBLY").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.AssemblyId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_BUSINESSORGANIZATION").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 2; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_R_TRUCKUNIT").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = JobAndEventService.GetCatalogItemGroupData().TruckUnits.FirstOrDefault().TruckUnitId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_R_CATALOGITEM").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = JobAndEventService.GetCatalogItemGroupData().CatalogItems.FirstOrDefault().CatalogItemId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_EVENT_PARENT").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 10;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTBEGDTTM").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = DateTime.Today.ToString();
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTENDDTTM").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = DateTime.Today.AddDays(10).ToString();
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCDURATIONHOURS").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.ActualJobDurationDays;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTORDER").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 10;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCWORKORDERID").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "WRKORDID_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFIELDSERVICEORDERID").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "FLDSRVID_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCRESPONSIBLEPERSON").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "RESPONSIBLE_PERSON_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCPERSONPERFORMINGTASK").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "PERFORMING_TASK_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCQUANTITY").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 50m;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCTOTALCOST").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 100m;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCDOCUMENTFILENAME").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "EVCDOCUMENTFILENAME_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCORIGINKEY").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "EVCORIGINKEY_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCUNPLANNED").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = true;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCTROUBLE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = true;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCPREVENTIVEMAINTENANCE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = true;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCREMARKS").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.JobRemarks;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_BUSINESSORGANIZATION").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.BusinessOrganizationId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_WBFLOWAREA").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_GASTYPE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_DIVERTERTYPE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_DISPLACEMENTFLUID_PREFLUSH").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_ACIDTYPE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_DISPLACEMENTFLUID").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            return eventMetadata;
        }

        public MetaDataDTO[] SetUpdateMetadata(MetaDataDTO[] eventMetadata, JobLightDTO getJob)
        {
            long id = 0;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_EVENT_PARENT").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 20;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTBEGDTTM").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = DateTime.Today.ToString();
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTENDDTTM").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = DateTime.Today.AddDays(10).ToString();
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCDURATIONHOURS").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.ActualJobDurationDays;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTORDER").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 20;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCWORKORDERID").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "WRKORDID_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFIELDSERVICEORDERID").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "FLDSRVID_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCRESPONSIBLEPERSON").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "RESPONSIBLE_PERSON_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCPERSONPERFORMINGTASK").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "PERFORMING_TASK_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCQUANTITY").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 60m;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCTOTALCOST").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 200m;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCDOCUMENTFILENAME").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "EVCDOCUMENTFILENAME_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCORIGINKEY").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "EVCORIGINKEY_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCUNPLANNED").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = true;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCTROUBLE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = true;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCPREVENTIVEMAINTENANCE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = true;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCREMARKS").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.JobRemarks;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_BUSINESSORGANIZATION").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.BusinessOrganizationId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_WBFLOWAREA").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_GASTYPE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_DIVERTERTYPE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_DISPLACEMENTFLUID_PREFLUSH").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_ACIDTYPE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_DISPLACEMENTFLUID").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value

            return eventMetadata.ToArray();
        }

        public MetaDataDTO[] CreateEventForCleanFillEvent(string jobId, int totalEvents = 1)
        {
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            EventTypeDTO EventsbyJobType = JobAndEventService.GetAllEventTypesForJobType(getJob.JobTypeId.ToString()).FirstOrDefault(x => x.EventTypeName == "Tag Fill");
            MetaDataDTO[] eventMetadata = JobAndEventService.GetMetaDatasForCreateEventType(EventsbyJobType.EventTypeId.ToString());
            Trace.WriteLine("No. of input fields for the Event type_ : " + EventsbyJobType.EventTypeName + " is : " + eventMetadata.Count());
            eventMetadata = SetMetadataForCleanFillEvent(eventMetadata, getJob);
            eventMetadata.FirstOrDefault(x => x.ColumnName == "evcFK_r_EventType").DataValue = EventsbyJobType.EventTypeId;
            foreach (MetaDataDTO e in eventMetadata)
            {
                e.InstanceId = getJob.JobId;
                e.InstanceTypeId = EventsbyJobType.EventTypeId;
            }
            int count = 0;
            while (totalEvents > 0)
            {
                string eventId = JobAndEventService.AddEvent(eventMetadata);
                Assert.IsNotNull(eventId, "Failed to create Event");
                totalEvents = totalEvents - 1;
                count = count + 1;
            }
            EventGroupDTO[] allEvents = JobAndEventService.GetEvents(getJob.JobId.ToString());
            Assert.AreEqual(count, allEvents.FirstOrDefault().EventData.Count());
            foreach (EventGroupDTO eg in allEvents)
            {
                foreach (EventDTO evt in eg.EventData)
                {
                    Assert.AreEqual(getJob.AFE, evt.AFE);
                    Assert.AreEqual(eventMetadata.FirstOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTBEGDTTM").DataValue, evt.BeginTime.ToUniversalTime().ToISO8601());
                    Assert.AreEqual(eventMetadata.FirstOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTENDDTTM").DataValue, evt.EndTime.ToUniversalTime().ToISO8601());
                    Assert.AreEqual(evt.Duration, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCDURATIONHOURS").DataValue);
                    Assert.AreEqual(evt.Order, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTORDER").DataValue);
                    Assert.AreEqual(evt.WorkorderID, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCWORKORDERID").DataValue);
                    Assert.AreEqual(evt.FieldServiceOrderID, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFIELDSERVICEORDERID").DataValue);
                    Assert.AreEqual(evt.ResponsiblePerson, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCRESPONSIBLEPERSON").DataValue);
                    Assert.AreEqual(evt.PersonPerformingTask, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCPERSONPERFORMINGTASK").DataValue);
                    Assert.AreEqual(evt.Quantity, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCQUANTITY").DataValue);
                    Assert.AreEqual(evt.TotalCost, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCTOTALCOST").DataValue);
                    Assert.AreEqual(evt.DocumentFileName, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCDOCUMENTFILENAME").DataValue);
                    Assert.AreEqual(evt.OriginKey, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCORIGINKEY").DataValue);
                    Assert.AreEqual(evt.UnPlanned, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCUNPLANNED").DataValue);
                    Assert.AreEqual(evt.Trouble, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCTROUBLE").DataValue);
                    Assert.AreEqual(evt.PreventiveMaintenance, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCPREVENTIVEMAINTENANCE").DataValue);
                    Assert.AreEqual(evt.Remarks, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCREMARKS").DataValue);
                }
            }

            return eventMetadata;
        }

        public MetaDataDTO[] CreateEvent(string jobId, int totalEvents = 5)
        {
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            EventTypeDTO EventsbyJobType = JobAndEventService.GetAllEventTypesForJobType(getJob.JobTypeId.ToString()).FirstOrDefault();
            MetaDataDTO[] eventMetadata = JobAndEventService.GetMetaDatasForCreateEventType(EventsbyJobType.EventTypeId.ToString());
            Trace.WriteLine("No. of input fields for the Event type_ : " + EventsbyJobType.EventTypeName + " is : " + eventMetadata.Count());
            eventMetadata = SetMetadata(eventMetadata, getJob);
            eventMetadata.FirstOrDefault(x => x.ColumnName == "evcFK_r_EventType").DataValue = EventsbyJobType.EventTypeId;
            foreach (MetaDataDTO e in eventMetadata)
            {
                e.InstanceId = getJob.JobId;
                e.InstanceTypeId = EventsbyJobType.EventTypeId;
            }
            int count = 0;
            while (totalEvents > 0)
            {
                string eventId = JobAndEventService.AddEvent(eventMetadata);
                Assert.IsNotNull(eventId, "Failed to create Event");
                totalEvents = totalEvents - 1;
                count = count + 1;
            }
            EventGroupDTO[] allEvents = JobAndEventService.GetEvents(getJob.JobId.ToString());
            Assert.AreEqual(count, allEvents.FirstOrDefault().EventData.Count());
            foreach (EventGroupDTO eg in allEvents)
            {
                foreach (EventDTO evt in eg.EventData)
                {
                    Assert.AreEqual(getJob.AFE, evt.AFE);
                    Assert.AreEqual(eventMetadata.FirstOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTBEGDTTM").DataValue, evt.BeginTime.ToLocalTime().ToString());
                    Assert.AreEqual(eventMetadata.FirstOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTENDDTTM").DataValue, evt.EndTime.ToLocalTime().ToString());
                    Assert.AreEqual(evt.Duration, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCDURATIONHOURS").DataValue);
                    Assert.AreEqual(evt.Order, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTORDER").DataValue);
                    Assert.AreEqual(evt.WorkorderID, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCWORKORDERID").DataValue);
                    Assert.AreEqual(evt.FieldServiceOrderID, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFIELDSERVICEORDERID").DataValue);
                    Assert.AreEqual(evt.ResponsiblePerson, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCRESPONSIBLEPERSON").DataValue);
                    Assert.AreEqual(evt.PersonPerformingTask, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCPERSONPERFORMINGTASK").DataValue);
                    Assert.AreEqual(evt.Quantity, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCQUANTITY").DataValue);
                    Assert.AreEqual(evt.TotalCost, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCTOTALCOST").DataValue);
                    Assert.AreEqual(evt.DocumentFileName, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCDOCUMENTFILENAME").DataValue);
                    Assert.AreEqual(evt.OriginKey, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCORIGINKEY").DataValue);
                    Assert.AreEqual(evt.UnPlanned, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCUNPLANNED").DataValue);
                    Assert.AreEqual(evt.Trouble, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCTROUBLE").DataValue);
                    Assert.AreEqual(evt.PreventiveMaintenance, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCPREVENTIVEMAINTENANCE").DataValue);
                    Assert.AreEqual(evt.Remarks, eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCREMARKS").DataValue);
                }
            }

            return eventMetadata;
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

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void AddUpdateJob()
        {
            string wellId = AddWell("RPOC_").Id.ToString();
            Assert.IsNotNull(wellId, "Failed to add well");
            string jobId = AddJob("Approved");
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            //Update Job
            JobLightDTO job = new JobLightDTO();
            job.JobId = Convert.ToInt64(jobId);
            job.BeginDate = DateTime.Today.AddDays(0).ToUniversalTime();
            job.EndDate = DateTime.Today.AddDays(30).ToUniversalTime();
            job.ActualCost = (decimal)100000.00;
            job.ActualJobDurationDays = (decimal)20.5;
            job.TotalCost = (decimal)150000.00;
            job.JobRemarks = "updated remarks";
            job.JobOrigin = "TestJobOrigin ";
            job.AssemblyId = well.AssemblyId;
            job.WellId = well.Id;
            //For Below fields User can select any value in the dropdown menu
            job.AFEId = JobAndEventService.GetAFEs().FirstOrDefault().Id;
            job.StatusId = JobAndEventService.GetJobStatuses().FirstOrDefault(j => j.Name == "Approved").Id;
            job.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault(x => x.JobType == "Artificial Lift Revision").id;
            job.BusinessOrganizationId = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            job.AccountRef = "1";
            //JobReasonId drop down selection is based on the JobTypeId
            job.JobReasonId = JobAndEventService.GetJobReasonsForJobType(job.JobTypeId.ToString()).FirstOrDefault().Id;
            job.JobDriverId = (int)JobAndEventService.GetPrimaryMotivationsForJob().FirstOrDefault().Id;
            JobLightDTO JobbeforeUpdate = JobAndEventService.GetJobById(jobId.ToString());
            JobAndEventService.UpdateJob(job);
            JobLightDTO JobafterUpdate = JobAndEventService.GetJobById(jobId.ToString());
            Assert.AreEqual(job.JobRemarks, JobafterUpdate.JobRemarks, "Job not updated");
            Assert.AreEqual(JobbeforeUpdate.WellId, JobafterUpdate.WellId);
            Assert.AreEqual(JobbeforeUpdate.WellName, JobafterUpdate.WellName);
            Assert.AreEqual(JobbeforeUpdate.BeginDate, JobafterUpdate.BeginDate);
            Assert.AreEqual(JobbeforeUpdate.EndDate, JobafterUpdate.EndDate);
            Assert.AreEqual(JobbeforeUpdate.ActualCost, JobafterUpdate.ActualCost);
            Assert.AreEqual(JobbeforeUpdate.ActualJobDurationDays, JobafterUpdate.ActualJobDurationDays);
            Assert.AreEqual(JobbeforeUpdate.TotalCost, JobafterUpdate.TotalCost);
            Assert.AreNotEqual(JobbeforeUpdate.JobRemarks, JobafterUpdate.JobRemarks);
            Assert.AreEqual(JobbeforeUpdate.JobOrigin, JobafterUpdate.JobOrigin);
            Assert.AreEqual(JobbeforeUpdate.AssemblyId, JobafterUpdate.AssemblyId);
            Assert.AreEqual(JobbeforeUpdate.AFEId, JobafterUpdate.AFEId);
            Assert.AreEqual(JobbeforeUpdate.StatusId, JobafterUpdate.StatusId);
            Assert.AreEqual(JobbeforeUpdate.JobTypeId, JobafterUpdate.JobTypeId);
            Assert.AreEqual(JobbeforeUpdate.BusinessOrganizationId, JobafterUpdate.BusinessOrganizationId);
            Assert.AreEqual(JobbeforeUpdate.AccountRef, JobafterUpdate.AccountRef);
            Assert.AreEqual(JobbeforeUpdate.JobReasonId, JobafterUpdate.JobReasonId);
            Assert.AreEqual(JobbeforeUpdate.JobDriverId, JobafterUpdate.JobDriverId);
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void AddUpdateJobNew()
        {
            string wellId = AddWell("RPOC_").Id.ToString();
            Assert.IsNotNull(wellId, "Failed to add well");
            string jobId = AddJobNew("Approved");
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            //Update Job
            JobLightDTO job = new JobLightDTO();
            job.JobId = Convert.ToInt64(jobId);
            job.BeginDate = DateTime.Today.AddDays(0).ToUniversalTime();
            job.EndDate = DateTime.Today.AddDays(30).ToUniversalTime();
            job.ActualCost = (decimal)100000.00;
            job.ActualJobDurationDays = (decimal)20.5;
            job.TotalCost = (decimal)150000.00;
            job.JobRemarks = "updated remarks";
            job.JobOrigin = "TestJobOrigin ";
            job.AssemblyId = well.AssemblyId;
            job.WellId = well.Id;
            //For Below fields User can select any value in the dropdown menu
            job.AFEId = JobAndEventService.GetAFEs().FirstOrDefault().Id;
            job.StatusId = JobAndEventService.GetJobStatuses().FirstOrDefault(j => j.Name == "Approved").Id;
            job.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault(x => x.JobType == "Artificial Lift Revision").id;
            job.BusinessOrganizationId = JobAndEventService.GetBusinessOrganization().FirstOrDefault().ControlId;
            job.AccountRef = "1";
            //JobReasonId drop down selection is based on the JobTypeId
            job.JobReasonId = JobAndEventService.GetJobReasonsForJobType(job.JobTypeId.ToString()).FirstOrDefault().Id;
            job.JobDriverId = (int)JobAndEventService.GetPrimaryMotivationsForJob().FirstOrDefault().Id;
            JobLightDTO JobbeforeUpdate = JobAndEventService.GetJobById(jobId.ToString());
            JobAndEventService.UpdateJob(job);
            JobLightDTO JobafterUpdate = JobAndEventService.GetJobById(jobId.ToString());
            Assert.AreEqual(job.JobRemarks, JobafterUpdate.JobRemarks, "Job not updated");
            Assert.AreEqual(JobbeforeUpdate.WellId, JobafterUpdate.WellId);
            Assert.AreEqual(JobbeforeUpdate.WellName, JobafterUpdate.WellName);
            Assert.AreEqual(JobbeforeUpdate.BeginDate, JobafterUpdate.BeginDate);
            Assert.AreEqual(JobbeforeUpdate.EndDate, JobafterUpdate.EndDate);
            Assert.AreEqual(JobbeforeUpdate.ActualCost, JobafterUpdate.ActualCost);
            Assert.AreEqual(JobbeforeUpdate.ActualJobDurationDays, JobafterUpdate.ActualJobDurationDays);
            Assert.AreEqual(JobbeforeUpdate.TotalCost, JobafterUpdate.TotalCost);
            Assert.AreNotEqual(JobbeforeUpdate.JobRemarks, JobafterUpdate.JobRemarks);
            Assert.AreEqual(JobbeforeUpdate.JobOrigin, JobafterUpdate.JobOrigin);
            Assert.AreEqual(JobbeforeUpdate.AssemblyId, JobafterUpdate.AssemblyId);
            Assert.AreEqual(JobbeforeUpdate.AFEId, JobafterUpdate.AFEId);
            Assert.AreEqual(JobbeforeUpdate.StatusId, JobafterUpdate.StatusId);
            Assert.AreEqual(JobbeforeUpdate.JobTypeId, JobafterUpdate.JobTypeId);
            Assert.AreEqual(JobbeforeUpdate.BusinessOrganizationId, JobafterUpdate.BusinessOrganizationId);
            Assert.AreEqual(JobbeforeUpdate.AccountRef, JobafterUpdate.AccountRef);
            Assert.AreEqual(JobbeforeUpdate.JobReasonId, JobafterUpdate.JobReasonId);
            Assert.AreEqual(JobbeforeUpdate.JobDriverId, JobafterUpdate.JobDriverId);
        }

        /*
         * Below Test is developed By Mintu Mukherjee
         */

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void EnterTourSheetStartPageDataPopulate()
        {
            string facilityId = s_isRunningInATS ? "RPOC_00001" : "RPOC_0001";
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
            Console.WriteLine("Well Created!");
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            string wellid = well.Id.ToString();
            _wellsToRemove.Add(well);
            GridSettingDTO grid = new GridSettingDTO();
            grid.CurrentPage = 1;
            grid.NumberOfPages = 1;
            grid.PageSize = 20;
            EntityGridSettingDTO setting = new EntityGridSettingDTO();
            setting.EntityName = "r_JobStatus";
            setting.GridSetting = grid;
            DBEntityDTO obj = DBEntityService.GetTableData(setting);
            Console.WriteLine("Getting all available job status in reference Data");
            List<MetaDataDTO[]> metaslist = new List<MetaDataDTO[]>();
            for (int j = 1; j <= 21; j++)
            {
                metaslist.Add(DBEntityService.GetRefereneceMetaDataEntityForUpdate(setting.EntityName, j.ToString()));
                //  metaslist.Add(DBEntityService.GetRefereneceMetaDataEntityForUpdate(setting.EntityName, "18"));
            }
            Console.WriteLine("Checking tour sheet visibility property in job status reference data config..");
            foreach (var metas in metaslist)
            {
                bool flag1 = false;
                foreach (MetaDataDTO prop in metas)
                {
                    if (prop.ColumnName.ToString().ToLower().Trim().Equals("mgstoursheetvisible"))
                    {
                        // Console.WriteLine(prop.Visible_InAttribute);
                        flag1 = true;
                        prop.DataValue = true;
                        Console.WriteLine("Updating Reference data, Activating Visible in Tour Sheet property for a Job status...");
                        bool resp = DBEntityService.UpdateReferenceData(metas);
                        Assert.IsTrue(resp, "Update reference data cannot be done");
                        Console.WriteLine("Reference data updated successfully");
                    }
                }
                Assert.IsTrue(flag1, "Tour sheet visible property could not be found!, Terminating next steps...");
            }
            Console.WriteLine("Adding JOBS for all available job status...");
            string approvejobId = AddJobNew("Approved");
            string canceljobId = AddJobNew("Cancelled");
            string completejobId = AddJobNew("Completed");
            string cancelledinprogressjobId = AddJobNew("Cancelled In Progress");
            string continousjobId = AddJobNew("Continuous");
            string steamingjobId = AddJobNew("Currently Steaming");
            string progressjobId = AddJobNew("In Progress");
            string lockeddownjobId = AddJobNew("Locked Down");
            string plannedjobId = AddJobNew("Planned");
            string scheduledjobId = AddJobNew("Scheduled");
            string NAjobid = AddJobNew("N/A");
            string midpointjobId = AddJobNew("Steam-Midpoint");
            string putonjobId = AddJobNew("Steam-Put on Production");
            string steamSoakid = AddJobNew("Steam-Soak");
            string suspendedid = AddJobNew("Suspended");
            string toursheetcompleteid = AddJobNew("Tour Sheet Complete");
            string afeid = AddJobNew("Waiting on AFE");
            string externalsystemid = AddJobNew("Waiting on External System");
            string waionappid = AddJobNew("Waiting on Approval");
            string maintenanceid = AddJobNew("Waiting on Maintenance");
            string toursheetid = AddJobNew("Waiting on Tour Sheet");
            Console.WriteLine("Completed Adding all JOBS!");
            Console.WriteLine("Fetching Tour Sheet Start Page populated JOBS...");
            JobLightDTO[] jobsin = JobAndEventService.GetTourSheetJobs(null);
            List<bool> flags = new List<bool>(21);
            for (int k = 1; k <= 21; k++)
            {
                flags.Add(false);
            }
            Console.WriteLine("Checking if all JOBS are populated in Tour Sheet Page...");
            int i = 0;
            foreach (JobLightDTO job in jobsin)
            {
                if (job.JobId == Convert.ToInt64(approvejobId))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(canceljobId))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(completejobId))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(cancelledinprogressjobId))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(continousjobId))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(steamingjobId))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(progressjobId))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(lockeddownjobId))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(plannedjobId))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(scheduledjobId))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(NAjobid))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(midpointjobId))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(steamSoakid))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(putonjobId))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(suspendedid))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(toursheetcompleteid))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(afeid))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(waionappid))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(externalsystemid))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(maintenanceid))
                {
                    flags[i] = true;
                }
                if (job.JobId == Convert.ToInt64(toursheetid))
                {
                    flags[i] = true;
                }

                i++;
            }
            Console.WriteLine("Checking Complete!");
            foreach (var flag in flags)
            {
                // Console.WriteLine(flag);
                Assert.IsTrue(flag, "One of the Job type is not populating in Tour sheet");
            }
            Console.WriteLine("All JOBS for all available JOB Status are populating in Tour SHeet start page");
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetAllJobsbyWellId()
        {
            string wellId = AddWell("RPOC_").Id.ToString();
            Assert.IsNotNull(wellId, "Failed to add well");
            int numberofJobs = 5;
            for (int i = 1; i <= numberofJobs; i++)
            {
                AddJob("Approved");
            }
            JobLightDTO[] allJobs = JobAndEventService.GetJobsByWell(wellId);
            Assert.AreEqual(numberofJobs, allJobs.Count());
            foreach (JobLightDTO job in allJobs)
            {
                Assert.AreEqual(wellId, job.WellId.ToString());
            }
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetSingleJob()
        {
            AddWell("RPOC_");
            string jobId = AddJob("Approved");
            JobDTO getSingleJob = JobAndEventService.GetSingleJobDetails(jobId);
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            Assert.AreEqual(getJob.AccountRef, getSingleJob.AccountingRef);
            Assert.AreEqual(getJob.ActualCost, getSingleJob.ActualJobCost);
            Assert.AreEqual(getJob.ActualJobDurationDays, getSingleJob.ActualJobDurationDays);
            Assert.AreEqual(getJob.AFE, getSingleJob.AFE);
            Assert.AreEqual(getJob.AssemblyId, getSingleJob.Assembly);
            Assert.AreEqual(getJob.BusinessOrganization, getSingleJob.BusinessOrganization);
            Assert.AreEqual(getJob.TotalCost, getSingleJob.EstimatedCost);
            Assert.AreEqual(getJob.JobId, getSingleJob.Id);
            Assert.AreEqual(getJob.BeginDate.ToString(), getSingleJob.JobBegDateTime.ToString());
            Assert.AreEqual(getJob.EndDate.ToString(), getSingleJob.JobEndDateTime.ToString());
            Assert.AreEqual(getJob.JobReason, getSingleJob.JobReason);
            Assert.AreEqual(getJob.JobType, getSingleJob.JobType);
            Assert.AreEqual(getJob.JobOrigin, getSingleJob.OriginKey);
            Assert.AreEqual(getJob.JobDriver, getSingleJob.PrimaryMotivationForJob);
            Assert.AreEqual(getJob.JobRemarks, getSingleJob.Remarks);
            Assert.AreEqual(getJob.Status, getSingleJob.Status.ToString());
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetallEventstypes()
        {
            JobTypeDTO[] allJobtypes = JobAndEventService.GetJobTypes();
            foreach (JobTypeDTO jobType in allJobtypes)
            {
                //Getting all event types
                EventTypeDTO[] allEventsbyJobType = JobAndEventService.GetAllEventTypesForJobType(jobType.id.ToString());
                Trace.WriteLine("No. of Events types for Jobtype_" + jobType.JobType + " is : " + allEventsbyJobType.Count());
                Assert.AreEqual(610, allEventsbyJobType.Count());
                Trace.WriteLine("");

                //Getting filtered events
                EventTypeDTO[] allfilteredEventsbyJobType = JobAndEventService.GetFilteredEventTypesForJobType(jobType.id.ToString());
                Trace.WriteLine("No. of Filtered Events types for Jobtype_" + jobType.JobType + " is : " + allfilteredEventsbyJobType.Count());
                int jobSpecialEventCount = 6; //Economic Analysis, Job Cost, Job Plan, Failure Report, Wellbore Report, Drilling Report
                Assert.AreEqual(allEventsbyJobType.Count() - jobSpecialEventCount, allfilteredEventsbyJobType.Count());
            }
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetMetaDataforEventType()
        {
            JobTypeDTO[] allJobtypes = JobAndEventService.GetJobTypes();
            EventTypeDTO[] allEventsbyJobType = JobAndEventService.GetAllEventTypesForJobType(allJobtypes.FirstOrDefault().id.ToString());
            foreach (EventTypeDTO eventtype in allEventsbyJobType)
            {
                MetaDataDTO[] eventTypeMetadata = JobAndEventService.GetMetaDatasForCreateEventType(eventtype.EventTypeId.ToString());
                Trace.WriteLine("No. of input fields for the Event type_ : " + eventtype.EventTypeName + " is : " + eventTypeMetadata.Count());
            }
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void AddUpdateDeleteEvent()
        {
            AddWell("RPOC_");
            string jobId = AddJob("Approved");
            CreateEvent(jobId);
            //Update Event
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            EventGroupDTO[] updatedEvents = JobAndEventService.GetEvents(jobId);
            MetaDataDTO[] updatedeventMetadata = JobAndEventService.GetMetaDatasForUpdateEvent(updatedEvents.FirstOrDefault().EventData.FirstOrDefault().Id.ToString());
            updatedeventMetadata = SetUpdateMetadata(updatedeventMetadata, getJob);
            foreach (MetaDataDTO e in updatedeventMetadata)
            {
                e.InstanceId = getJob.JobId;
                e.InstanceTypeId = updatedEvents.FirstOrDefault().EventData.FirstOrDefault().EventTypeId;
            }
            JobAndEventService.UpdateEvent(updatedeventMetadata);
            EventGroupDTO[] afterupdatedEvents = JobAndEventService.GetEvents(jobId);
            EventGroupDTO et = afterupdatedEvents.FirstOrDefault();//added only one event
            EventDTO evt = et.EventData.FirstOrDefault();//added only one event
            Assert.IsNotNull(evt);
            Assert.AreEqual(evt.BeginTime.ToLocalTime().ToString(), updatedeventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTBEGDTTM").DataValue);
            Assert.AreEqual(evt.EndTime.ToLocalTime().ToString(), updatedeventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTENDDTTM").DataValue);
            Assert.AreEqual(evt.Duration, updatedeventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCDURATIONHOURS").DataValue);
            Assert.AreEqual(evt.Order, updatedeventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTORDER").DataValue);
            Assert.AreEqual(evt.WorkorderID, updatedeventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCWORKORDERID").DataValue);
            Assert.AreEqual(evt.FieldServiceOrderID, updatedeventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFIELDSERVICEORDERID").DataValue);
            Assert.AreEqual(evt.ResponsiblePerson, updatedeventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCRESPONSIBLEPERSON").DataValue);
            Assert.AreEqual(evt.PersonPerformingTask, updatedeventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCPERSONPERFORMINGTASK").DataValue);
            Assert.AreEqual(evt.Quantity, updatedeventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCQUANTITY").DataValue);
            Assert.AreEqual(evt.TotalCost, updatedeventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCTOTALCOST").DataValue);
            Assert.AreEqual(evt.DocumentFileName, updatedeventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCDOCUMENTFILENAME").DataValue);
            Assert.AreEqual(evt.OriginKey, updatedeventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCORIGINKEY").DataValue);
            Assert.AreEqual(evt.UnPlanned, updatedeventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCUNPLANNED").DataValue);
            Assert.AreEqual(evt.Trouble, updatedeventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCTROUBLE").DataValue);
            Assert.AreEqual(evt.PreventiveMaintenance, updatedeventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCPREVENTIVEMAINTENANCE").DataValue);
            Assert.AreEqual(evt.Remarks, updatedeventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCREMARKS").DataValue);
            //Remove Event
            foreach (EventGroupDTO eg in afterupdatedEvents)
            {
                foreach (EventDTO e in eg.EventData)
                {
                    bool removeEvent = JobAndEventService.RemoveJobEventData(e.Id.ToString());
                    Assert.IsTrue(removeEvent, "Failed to remove event");
                }
            }
        }

        //This test method is used for testing of mass update event record in single batch save.
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetEventRecordForMultiUpdateEvents()
        {
            int i = 0;
            int j = 0;
            int k = 0;

            //creating well, job and event records
            AddWell("RPOC_");
            string jobId = AddJob("Approved");
            CreateEvent(jobId);

            EventGroupDTO[] getMultiEventRecords = JobAndEventService.GetEvents(jobId);
            Trace.WriteLine("EventType                    BeginTime                            Endtime                       ServiceProvider                     Duration          TotalCost             OrderId         WorkerID                                          Remark");

            foreach (EventGroupDTO eg in getMultiEventRecords)
            {
                foreach (EventDTO evt in eg.EventData)
                {
                    i = i + 1;
                    Trace.WriteLine(evt.EventType + "    " + evt.BeginTime + "    " + evt.EndTime + "              " + evt.BusinessOrganization.Substring(0, 4) + "                                       " + evt.Duration + "                " + evt.TotalCost + "                " + evt.Order + "             " + evt.WorkorderID + "         " + evt.Remarks);
                }
            }

            Trace.WriteLine(" ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- ");
            Trace.WriteLine("Total Added Event Records:" + i);
            Trace.WriteLine(" ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- ");

            //upDateMultiEvent[] is used for mapping of event array records
            EventDTO[] upDateMultiEvent = new EventDTO[i];
            DateTime BTime = DateTime.UtcNow.AddDays(0);

            foreach (EventGroupDTO eg in getMultiEventRecords)
            {
                foreach (EventDTO evt in eg.EventData)
                {
                    evt.BeginTime = BTime.AddDays(j + 1);
                    evt.EndTime = BTime.AddDays(j + 11);
                    // evt.PK_BusinessOrganization = JobAndEventService.GetCatalogItemGroupData().Vendors[j].Id;
                    evt.BusinessOrganizationControlIdText.ControlId = JobAndEventService.GetCatalogItemGroupData().Vendors[j].Id;
                    //evt.HistoricalRate = 10 + j;
                    evt.Duration = 10 + j;
                    evt.TotalCost = 20 + j;
                    evt.Order = 30 + j;
                    evt.WorkorderID = "WorkerID:" + j;
                    evt.Remarks = "AutoUpdate_Remark: " + j;
                    upDateMultiEvent[j] = evt;
                    j = j + 1;
                }
            }

            //Calling MassUpdateEvent API for single batch save
            JobAndEventService.MassUpdateEvent(upDateMultiEvent);

            Trace.WriteLine(" ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- ");
            EventGroupDTO[] getMultiEventRecordsUpdate = JobAndEventService.GetEvents(jobId);

            foreach (EventGroupDTO eg in getMultiEventRecordsUpdate)
            {
                foreach (EventDTO evt in eg.EventData)
                {
                    Assert.AreEqual(evt.BeginTime.ToString(), BTime.AddDays(k + 1).ToString(), "BeginTime is not matching");
                    Assert.AreEqual(evt.EndTime.ToString(), BTime.AddDays(k + 11).ToString(), "EndTime is not matching");
                    Assert.AreEqual(evt.PK_BusinessOrganization, JobAndEventService.GetCatalogItemGroupData().Vendors[k].Id, "Service Provider is not matching");
                    Assert.AreEqual(evt.Duration, 10 + k, "Duration is not matching");
                    //Assert.AreEqual(evt.HistoricalRate, 10 + k, "Rate is not matching");
                    Assert.AreEqual(evt.TotalCost, 20 + k, "Total Cost is not matching");
                    Assert.AreEqual(evt.Order, 30 + k, "Order Id is not matching");
                    Assert.AreEqual(evt.WorkorderID, "WorkerID:" + k, "Worker Id is not matching");
                    Assert.AreEqual(evt.Remarks, "AutoUpdate_Remark: " + k, "Remarks is not matching");
                    Trace.WriteLine(evt.EventType + "    " + evt.BeginTime + "    " + evt.EndTime + "              " + evt.BusinessOrganization.Substring(0, 4) + "                                       " + evt.Duration + "                " + evt.TotalCost + "                " + evt.Order + "             " + evt.WorkorderID + "         " + evt.Remarks);
                    k = k + 1;
                }
            }
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void MorningReport()
        {
            AddWell("RPOC_");
            string jobId = AddJob("Approved");
            int evtCount = 5;
            CreateEvent(jobId, evtCount);
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            EventGroupDTO[] allEvents = JobAndEventService.GetEvents(jobId);
            Assert.AreEqual(evtCount, allEvents.FirstOrDefault().EventData.Count());
            MorningReportVDTO mngReport = new MorningReportVDTO();
            mngReport.StartDate = DateTime.SpecifyKind(allEvents.FirstOrDefault().EventData.FirstOrDefault().BeginTime, DateTimeKind.Local);
            MorningReportVDTO reports = JobAndEventService.GetMorningReport(mngReport);
            Assert.AreEqual(evtCount, reports.MorningReports.Count());
            Assert.AreEqual(allEvents.FirstOrDefault().EventData.Count, reports.MorningReports.Count());
            for (int i = 0; i < reports.MorningReports.Count(); i++)
            {
                Assert.AreEqual(allEvents.FirstOrDefault().EventData[i].BeginTime.Date, reports.MorningReports[i].BeginTime.Date);
                Assert.AreEqual(allEvents.FirstOrDefault().EventData[i].TotalCost, reports.MorningReports[i].Cost);
                Assert.AreEqual(allEvents.FirstOrDefault().EventData[i].EndTime.Date, reports.MorningReports[i].EndTime.Date);
                Assert.AreEqual(allEvents.FirstOrDefault().EventData[i].EventType, reports.MorningReports[i].EventType);
                Assert.AreEqual(allEvents.FirstOrDefault().EventData[i].PK_Job, reports.MorningReports[i].JobId);
                Assert.AreEqual(getJob.JobReason, reports.MorningReports[i].JobReason);
                Assert.AreEqual(allEvents.FirstOrDefault().EventData[i].Remarks, reports.MorningReports[i].Remarks);
                Assert.AreEqual(allEvents.FirstOrDefault().EventData[i].BusinessOrganization, reports.MorningReports[i].ServiceProvider);
                Assert.AreEqual(getJob.WellId, reports.MorningReports[i].WellId);
                Assert.AreEqual(getJob.WellName, reports.MorningReports[i].WellName);
            }
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void AddUpdateEconomicAnalysis()
        {
            AddWell("RPOC_");
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.EconomicAnalysisPlan);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.EconomicAnalysisPlan);
            long evtStatus = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(evtStatus > 0, "Failed to create Event");
            JobLightDTO[] Job = JobAndEventService.GetJobsByWell(getJob.WellId.ToString());
            Assert.IsTrue(Job.FirstOrDefault(j => j.JobId == getJob.JobId).ExistingJobEvents.HasEconomicAnalysisEvent);
            CommonProductTypesDTO commonProductTypes = JobAndEventService.GetCommonProductTypes();
            JobEconomicAnalysisDTO jobEconomicAnalysis = new JobEconomicAnalysisDTO();
            jobEconomicAnalysis.JobId = getJob.JobId;
            jobEconomicAnalysis.ProductionForecastScenarioId = 1;
            jobEconomicAnalysis.ProductForecastScenarioWaterId = 1;
            jobEconomicAnalysis.ProductForecastScenarioGasId = 1;
            jobEconomicAnalysis.ProductPriceScenarioId = JobAndEventService.GetProductPriceScenarios(commonProductTypes.OilProductTypeId.Value.ToString()).FirstOrDefault().Id;
            jobEconomicAnalysis.ProductPriceScenarioWaterId = JobAndEventService.GetProductPriceScenarios(commonProductTypes.WaterProductTypeId.Value.ToString()).FirstOrDefault().Id;
            jobEconomicAnalysis.ProductPriceScenarioGasId = JobAndEventService.GetProductPriceScenarios(commonProductTypes.GasProductTypeId.Value.ToString()).FirstOrDefault().Id;
            jobEconomicAnalysis.TaxRateId = JobAndEventService.GetStateTax().FirstOrDefault().Id;
            jobEconomicAnalysis.ProductTypeId = commonProductTypes.OilProductTypeId.Value;
            jobEconomicAnalysis.AnalysisDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Local);
            jobEconomicAnalysis.DPICalcOverriden = true;
            bool status = JobAndEventService.AddUpdateJobEconomicAnalysis(jobEconomicAnalysis);
            Assert.IsTrue(status, "Failed to Add Economic analysis");
            // Get Economic Analysis
            JobEconomicAnalysisDTO addedEA = JobAndEventService.GetJobEconomicAnalysis(jobId, getJob.WellId.ToString());
            Assert.IsNotNull(addedEA, "Get Economic Analysis failed");
            Assert.AreEqual(getJob.JobId, addedEA.JobId);

            // Verify add
            Assert.IsTrue(status, "Add Economic Analysis failed");
            Assert.AreEqual(addedEA.ProductionForecastScenarioId, jobEconomicAnalysis.ProductionForecastScenarioId, "ProductionForecastScenarioId does not match");
            Assert.AreEqual(addedEA.ProductForecastScenarioWaterId, jobEconomicAnalysis.ProductForecastScenarioWaterId, "ProductForecastScenarioWaterId does not match");
            Assert.AreEqual(addedEA.ProductForecastScenarioGasId, jobEconomicAnalysis.ProductForecastScenarioGasId, "ProductForecastScenarioGasId does not match");
            Assert.AreEqual(addedEA.ProductPriceScenarioId, jobEconomicAnalysis.ProductPriceScenarioId, "ProductPriceScenarioId does not match");
            Assert.AreEqual(addedEA.ProductPriceScenarioWaterId, jobEconomicAnalysis.ProductPriceScenarioWaterId, "ProductPriceScenarioWaterId does not match");
            Assert.AreEqual(addedEA.ProductPriceScenarioGasId, jobEconomicAnalysis.ProductPriceScenarioGasId, "ProductPriceScenarioGasId does not match");
            Assert.AreEqual(addedEA.TaxRateId, jobEconomicAnalysis.TaxRateId, "TaxRateId does not match");
            Assert.AreEqual(addedEA.ProductTypeId, jobEconomicAnalysis.ProductTypeId, "ProductTypeId does not match");
            Assert.IsTrue(addedEA.DPICalcOverriden);

            // Update Economic Analysis
            addedEA.ProductPriceScenarioId = 2;
            addedEA.ProductPriceScenarioWaterId = 2;
            addedEA.ProductPriceScenarioGasId = 2;
            addedEA.TaxRateId = 2;
            addedEA.DPICalcOverriden = false;
            status = JobAndEventService.AddUpdateJobEconomicAnalysis(addedEA);
            JobEconomicAnalysisDTO updatedEA = JobAndEventService.GetJobEconomicAnalysis(jobId, getJob.WellId.ToString());

            // Verify update
            Assert.IsTrue(status, "Update Economic Analysis failed");
            Assert.AreEqual(updatedEA.ProductPriceScenarioId, addedEA.ProductPriceScenarioId, "ProductPriceScenarioId does not match");
            Assert.AreEqual(updatedEA.ProductPriceScenarioWaterId, addedEA.ProductPriceScenarioWaterId, "ProductPriceScenarioWaterId does not match");
            Assert.AreEqual(updatedEA.ProductPriceScenarioGasId, addedEA.ProductPriceScenarioGasId, "ProductPriceScenarioGasId does not match");
            Assert.AreEqual(updatedEA.TaxRateId, addedEA.TaxRateId, "TaxRateId does not match");
            Assert.AreEqual(updatedEA.DPICalcOverriden, addedEA.DPICalcOverriden, "DPICalc does not match");

            // Remove Event and its extended information
            evt.Id = evtStatus;
            bool check = JobAndEventService.RemoveEvent(evt);
            Assert.IsTrue(check, "Failed to remove Economic Analysis report");
        }

        [TestCategory("JobAndEventServiceTests"), TestMethod]
        public void JobApprovalProcess()
        {
            AddWell("RPOC_");
            JobStatusDTO[] js = JobAndEventService.GetJobStatuses();
            for (int i = 0; i < js.Count(); i++)
            {
                string jobId = AddJob(js[i].Name);
                bool check = JobAndEventService.IsJobApprovable(jobId);
                Trace.WriteLine(js[i].Name + "---" + check.ToString());
            }
        }

        public EventDetailCostDTO CreateEventDetailCostDTO(string jobId)
        {
            EventDetailCostDTO eventCost = new EventDetailCostDTO();

            eventCost.JobId = Convert.ToInt64(jobId);
            eventCost.Cost = 13;
            eventCost.CostRemarks = "Test Cost Remark";
            eventCost.Discount = 3;
            eventCost.Quantity = 8;
            eventCost.UnitPrice = 2;
            eventCost.VendorId = 3;
            eventCost.CostDate = DateTime.Today.AddDays(-10);

            return eventCost;
        }

        [TestCategory("JobAndEventServiceTests"), TestMethod]
        public void AddUpdateDeleteEventDetailsCost()
        {
            AddWell("RPOC_");
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.JobCostDetailReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.JobCostDetailReport);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
            JobLightDTO[] Job = JobAndEventService.GetJobsByWell(getJob.WellId.ToString());
            Assert.IsTrue(Job.FirstOrDefault(j => j.JobId == getJob.JobId).ExistingJobEvents.HasJobCostDetailEvent);
            EventDetailCostDTO anEventCostDetails = CreateEventDetailCostDTO(jobId);
            CatalogItemGroupDTO catalogDTO = JobAndEventService.GetCatalogItemGroupData();
            long catalogItemId = catalogDTO.CatalogItems.OrderBy(x => x.CatalogItemId).FirstOrDefault().CatalogItemId;
            anEventCostDetails.CatalogItemId = catalogItemId;

            //Call Web Service to Add New EventDetailCost record into Database
            string addedEventCostId = JobAndEventService.AddEventDetailCost(anEventCostDetails);

            // Get the EventDetailCost Record details using the Web Service using JobId
            JobCostDetailsDTO addedJobCost = JobAndEventService.GetJobCostDetailsForJob(jobId);

            EventDetailCostDTO getEventDetailCost = addedJobCost.EventDetailCostGroupData[0].EventDetailCosts.FirstOrDefault();

            //Verify Addition of EventDetailCost Record
            Assert.AreEqual(anEventCostDetails.JobId, getEventDetailCost.JobId, "New EventDetailCost record's Job Id field is not matching");
            Assert.AreEqual(anEventCostDetails.CatalogItemId, getEventDetailCost.CatalogItemId, "New EventDetailCost record's Catalog Item Id Cost field is not matching");
            Assert.AreEqual(anEventCostDetails.Cost, getEventDetailCost.Cost, "New EventDetailCost record's Cost field is not matching");
            Assert.AreEqual(anEventCostDetails.CostRemarks, getEventDetailCost.CostRemarks, "New EventDetailCost record's Cost Remarks field is not matching");
            Assert.AreEqual(anEventCostDetails.Discount, getEventDetailCost.Discount, "New EventDetailCost record's Discount field is not matching");
            Assert.AreEqual(anEventCostDetails.Quantity, getEventDetailCost.Quantity, "New EventDetailCost record's Quantity field is not matching");
            Assert.AreEqual(anEventCostDetails.UnitPrice, getEventDetailCost.UnitPrice, "New EventDetailCost record's Unit Price field is not matching");
            Assert.AreEqual(anEventCostDetails.VendorId, getEventDetailCost.VendorId, "New EventDetailCost record's Vendor Id field is not matching");
            Assert.AreEqual(anEventCostDetails.CostDate, getEventDetailCost.CostDate, "New EventDetailCost record's Cost Date field is not matching");

            //Update EventDetailCost
            EventDetailCostDTO updateEventDetailCost = new EventDetailCostDTO();
            updateEventDetailCost.Id = Convert.ToInt64(addedEventCostId);
            updateEventDetailCost.EventId = getEventDetailCost.EventId;
            updateEventDetailCost.JobId = Convert.ToInt64(jobId);
            updateEventDetailCost.CatalogItemId = catalogItemId;
            updateEventDetailCost.Cost = 20;
            updateEventDetailCost.CostRemarks = "Test Update Cost Remark";
            updateEventDetailCost.Discount = 1;
            updateEventDetailCost.Quantity = 7;
            updateEventDetailCost.UnitPrice = 3;
            updateEventDetailCost.VendorId = 3;
            updateEventDetailCost.CostDate = DateTime.Today.AddDays(-8);

            JobAndEventService.UpdateEventDetailCost(updateEventDetailCost);

            // Get the EventDetailCost Updated Record details using the Web Service using JobId
            JobCostDetailsDTO getUpdateJobCost = JobAndEventService.GetJobCostDetailsForJob(jobId);
            EventDetailCostDTO getUpdateEventDetailCost = getUpdateJobCost.EventDetailCostGroupData[0].EventDetailCosts.FirstOrDefault();

            //Verify Updation of EventDetailCost Record
            Assert.AreEqual(updateEventDetailCost.JobId, getUpdateEventDetailCost.JobId, "Updated EventDetailCost record's Job Id field is not matching");
            Assert.AreEqual(updateEventDetailCost.CatalogItemId, getUpdateEventDetailCost.CatalogItemId, "Updated EventDetailCost record's Catalog Item Id Cost field is not matching");
            Assert.AreEqual(updateEventDetailCost.Cost, getUpdateEventDetailCost.Cost, "Updated EventDetailCost record's Cost field is not matching");
            Assert.AreEqual(updateEventDetailCost.CostRemarks, getUpdateEventDetailCost.CostRemarks, "Updated EventDetailCost record's Cost Remarks field is not matching");
            Assert.AreEqual(updateEventDetailCost.Discount, getUpdateEventDetailCost.Discount, "Updated EventDetailCost record's Discount field is not matching");
            Assert.AreEqual(updateEventDetailCost.Quantity, getUpdateEventDetailCost.Quantity, "Updated EventDetailCost record's Quantity field is not matching");
            Assert.AreEqual(updateEventDetailCost.UnitPrice, getUpdateEventDetailCost.UnitPrice, "Updated EventDetailCost record's Unit Price field is not matching");
            Assert.AreEqual(updateEventDetailCost.VendorId, getUpdateEventDetailCost.VendorId, "Updated EventDetailCost record's Vendor Id field is not matching");
            Assert.AreEqual(updateEventDetailCost.CostDate, getUpdateEventDetailCost.CostDate, "Updated EventDetailCost record's Cost Date field is not matching");

            // Clone the updated EventDetailCost record
            string clonedEventDetailCostId = JobAndEventService.CloneEventDetailCost(getUpdateEventDetailCost.Id.ToString());

            // Get the EventDetailCost Cloned Record details using the Web Service using JobId
            JobCostDetailsDTO getClonedJobCost = JobAndEventService.GetJobCostDetailsForJob(jobId);
            EventDetailCostDTO getClonedEventDetailCost = getClonedJobCost.EventDetailCostGroupData[0].EventDetailCosts.Skip(1).FirstOrDefault();

            // Verify cloning of EventDetailCost
            Assert.AreEqual(updateEventDetailCost.JobId, getClonedEventDetailCost.JobId, "Updated EventDetailCost record's Job Id field is not matching");
            Assert.AreEqual(updateEventDetailCost.CatalogItemId, getClonedEventDetailCost.CatalogItemId, "Updated EventDetailCost record's Catalog Item Id Cost field is not matching");
            Assert.AreEqual(updateEventDetailCost.Cost, getClonedEventDetailCost.Cost, "Updated EventDetailCost record's Cost field is not matching");
            Assert.AreEqual(updateEventDetailCost.CostRemarks, getClonedEventDetailCost.CostRemarks, "Updated EventDetailCost record's Cost Remarks field is not matching");
            Assert.AreEqual(updateEventDetailCost.Discount, getClonedEventDetailCost.Discount, "Updated EventDetailCost record's Discount field is not matching");
            Assert.AreEqual(updateEventDetailCost.Quantity, getClonedEventDetailCost.Quantity, "Updated EventDetailCost record's Quantity field is not matching");
            Assert.AreEqual(updateEventDetailCost.UnitPrice, getClonedEventDetailCost.UnitPrice, "Updated EventDetailCost record's Unit Price field is not matching");
            Assert.AreEqual(updateEventDetailCost.VendorId, getClonedEventDetailCost.VendorId, "Updated EventDetailCost record's Vendor Id field is not matching");
            Assert.AreEqual(updateEventDetailCost.CostDate, getClonedEventDetailCost.CostDate, "Updated EventDetailCost record's Cost Date field is not matching");

            //Remove EventDetailCost
            JobAndEventService.RemoveEventDetailCost(clonedEventDetailCostId);
            bool isDeleted = JobAndEventService.RemoveEventDetailCost(getEventDetailCost.Id.ToString());
            Assert.IsTrue(isDeleted);

            // Remove Event and its extended information
            evt.Id = check;
            bool rcheck = JobAndEventService.RemoveEvent(evt);
            Assert.IsTrue(rcheck, "Failed to remove Economic Analysis report");
        }

        public EventDetailScheduledActivityDTO CreateJobPlanDetailDTO(string jobId = null)
        {
            EventDetailScheduledActivityDTO eventJobPlan = new EventDetailScheduledActivityDTO();
            if (!string.IsNullOrEmpty(jobId))
                eventJobPlan.JobId = Convert.ToInt64(jobId);
            CatalogItemGroupDTO catalogDTO = JobAndEventService.GetCatalogItemGroupData();
            eventJobPlan.VendorId = catalogDTO.CatalogItems.OrderBy(x => x.CatalogItemId).FirstOrDefault().VendorId;
            eventJobPlan.CatalogItemId = catalogDTO.CatalogItems.OrderBy(x => x.CatalogItemId).FirstOrDefault().CatalogItemId;
            eventJobPlan.TruckUnitId = catalogDTO.TruckUnits.Single(x => x.VendorId == eventJobPlan.VendorId).TruckUnitId;
            eventJobPlan.UOMId = JobAndEventService.GetUnitsOfMeasure().FirstOrDefault().UOMId;
            eventJobPlan.Remarks = "Test Remarks";
            eventJobPlan.Description = "Test Description";
            return eventJobPlan;
        }

        [TestCategory("JobAndEventServiceTests"), TestMethod]
        public void RepopulateMostCommonEvent()
        {
            string wellId = AddWell("RPOC_").Id.ToString();
            JobStatusDTO[] js = JobAndEventService.GetJobStatuses();
            for (int i = 0; i < js.Count(); i++)
            {
                string jobId = AddJob(js[i].Name);
            }
            List<EventDTO> MultiAddEvents = new List<EventDTO>();
            List<string> EventNames = new List<string>() { "Cement Retainer - Composite - Set", "Acidize - Spot", "BOPE - Nipple Down", "BOPE Annular - Nipple Down" };
            JobLightDTO[] allJobs = JobAndEventService.GetJobsByWell(wellId);
            foreach (JobLightDTO job in allJobs)
            {
                EventTypeGroupDTO Events = JobAndEventService.GetEventTypesByGrouping(job.JobTypeId.ToString(), job.JobId.ToString());
                EventTypeDTO[] allEvents = Events.AllEvents;
                foreach (string Evt in EventNames)
                {
                    EventTypeDTO addEvt = allEvents.FirstOrDefault(x => x.EventTypeName == Evt);
                    EventDTO setEvent = SetMultiAddEventDTO(job, addEvt);
                    MultiAddEvents.Add(setEvent);
                }
                EventDTO[] userEvents = MultiAddEvents.ToArray();
                try
                {
                    bool multiAdd = JobAndEventService.AddMultipleEventForJobEventType(userEvents);
                    if (!multiAdd)
                    {
                        Assert.Fail("Failed to add events through multi add");
                    }
                }
                catch
                {
                    Assert.Fail("Failed to add events through multi add");
                }
            }
            JobAndEventService.RepopulateMostCommonEventType();
            EventTypeGroupDTO CommonEvents = JobAndEventService.GetEventTypesByGrouping(allJobs.FirstOrDefault().JobTypeId.ToString(), allJobs.FirstOrDefault().JobId.ToString());
            Assert.IsTrue(CommonEvents.MostCommonEvents.Count() > 0, "Failed to retrieve most common events");

            //Get Events Since date
            int evtCount = 0;
            foreach (JobLightDTO job in allJobs)
            {
                EventGroupDTO[] evts = JobAndEventService.GetEvents(job.JobId.ToString());
                evtCount += evts.FirstOrDefault().EventData.Count();
            }
            string date = DateTime.UtcNow.AddDays(3).ToString("MM-dd-yyyy");
            JobLightDTO[] getJobsbyDate = JobAndEventService.GetJobsSinceDate(date);
            Assert.AreEqual(0, getJobsbyDate.Count(), "Jobs appeared before the given date");
            EventGroupDTO[] getEventsbyDate = JobAndEventService.GetEventsSinceDate(date);
            Assert.AreEqual(0, getEventsbyDate.Count(), "Event appeared before the given date");

            date = DateTime.UtcNow.AddDays(-1).ToString("MM-dd-yyyy");
            getJobsbyDate = JobAndEventService.GetJobsSinceDate(date);
            Assert.AreEqual(allJobs.Count(), getJobsbyDate.Count(), "Jobs not appeared for the given date");


            //date = DateTime.UtcNow.AddDays(2).ToString("MM-dd-yyyy HH-mm-ss tt");
            //getEventsbyDate = JobAndEventService.GetEventsSinceDate(date);
            EventDTO[] jobevts = JobAndEventService.GetEventsByJobId(allJobs.FirstOrDefault().JobId.ToString());
            Assert.AreNotEqual(0, jobevts.Count(), "Event not appeared for the given Job id");
            //Assert.AreEqual(evtCount, getEventsbyDate.FirstOrDefault().EventData.Count());
        }

        [TestCategory("JobAndEventServiceTests"), TestMethod]
        public void AddUpdateDeleteJobPlanDetail()
        {
            AddWell("RPOC_");
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.JobPlan);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.JobPlan);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
            JobLightDTO[] Job = JobAndEventService.GetJobsByWell(getJob.WellId.ToString());
            Assert.IsTrue(Job.FirstOrDefault(j => j.JobId == getJob.JobId).ExistingJobEvents.HasJobPlanEvent);

            EventDetailScheduledActivityDTO addJobPlanDetail = CreateJobPlanDetailDTO(jobId);
            addJobPlanDetail.EventTypeId = JobAndEventService.GetAllEventTypesForJobType(getJob.JobTypeId.ToString()).FirstOrDefault().EventTypeId;

            //Call Web Service to Add New JobPlanDetail record into Database
            string addedJobPlanId = JobAndEventService.AddJobPlanDetail(addJobPlanDetail);

            // Get the JobPlan Record details using the Web Service using JobId
            JobPlanDetailsDTO addedJobPlanDetails = JobAndEventService.GetJobPlanDetails(jobId);

            EventDetailScheduledActivityDTO getJobPlanDetail = addedJobPlanDetails.EventPlanDetails[0];

            //Verify Addition of JobPlanDetail Record
            Assert.AreEqual(addJobPlanDetail.JobId, getJobPlanDetail.JobId, "New JobPlanDetail record's Job Id field is not matching");
            Assert.AreEqual(addJobPlanDetail.CatalogItemId, getJobPlanDetail.CatalogItemId, "New JobPlanDetail record's Catalog Item Id Cost field is not matching");
            Assert.AreEqual(addJobPlanDetail.VendorId, getJobPlanDetail.VendorId, "New JobPlanDetail record's Vendor Id field is not matching");
            Assert.AreEqual(addJobPlanDetail.TruckUnitId, getJobPlanDetail.TruckUnitId, "New JobPlanDetail record's TruckUnit Id field is not matching");
            //Assert.AreEqual(addJobPlanDetail.UOMId, getJobPlanDetail.UOMId, "New JobPlanDetail record's UOMUnit Id field is not matching");
            Assert.AreEqual(addJobPlanDetail.EventTypeId, getJobPlanDetail.EventTypeId, "New JobPlanDetail record's EventType Id field is not matching");
            Assert.AreEqual(addJobPlanDetail.Remarks, getJobPlanDetail.Remarks, "New JobPlanDetail record's Remarks field is not matching");
            Assert.AreEqual(addJobPlanDetail.Description, getJobPlanDetail.Description, "New JobPlanDetail record's Description field is not matching");

            //Get Event types by Grouping
            EventTypeGroupDTO plannedEvents = JobAndEventService.GetEventTypesByGrouping(getJob.JobTypeId.ToString(), jobId);
            Assert.AreEqual(1, plannedEvents.PlannedEvents.Count(), "Mismatch between the planned Events and added planned events in Job plan");
            Assert.AreEqual(getJobPlanDetail.EventTypeName, plannedEvents.PlannedEvents.FirstOrDefault().EventTypeName, "Mismatch between the planned Events and added planned events in Job plan");

            //Update JobPlanDetail
            getJobPlanDetail.Remarks = "Test Update Remark";
            getJobPlanDetail.Description = "Test Update Description";
            List<EventDetailScheduledActivityDTO> getJobPlanDetails = new List<EventDetailScheduledActivityDTO>();
            getJobPlanDetails.Add(getJobPlanDetail);
            JobAndEventService.UpdateJobPlanDetail(getJobPlanDetails.ToArray());

            // Get the JobPlanDetail Updated Record details using the Web Service using JobId
            JobPlanDetailsDTO getUpdateJobPlanDetails = JobAndEventService.GetJobPlanDetails(jobId);
            EventDetailScheduledActivityDTO getUpdateJobPlanDetail = getUpdateJobPlanDetails.EventPlanDetails[0];

            //Verify Updation of JobPlanDetail Record
            Assert.AreEqual(getJobPlanDetail.Remarks, getUpdateJobPlanDetail.Remarks, "Updated JobPlanDetail record's Remarks field is not matching");
            Assert.AreEqual(getJobPlanDetail.Description, getUpdateJobPlanDetail.Description, "Updated JobPlanDetail record's Description field is not matching");

            //Remove JobPlanDetail
            bool isDeleted = JobAndEventService.RemoveJobPlanDetail(getJobPlanDetail.Id.ToString());
            Assert.IsTrue(isDeleted);

            // Remove Event and its extended information
            evt.Id = check;
            bool rcheck = JobAndEventService.RemoveEvent(evt);
            Assert.IsTrue(rcheck, "Failed to remove Economic Analysis report");
        }

        /// <summary>
        /// Written a integration test for FRWM-4548.
        /// verifying a new API- UpdateJobPlan()
        /// Verifying existing API- GetJobPlanDetails()
        /// </summary>
        /// <author>Rahul Pingale</author>
        [TestCategory("JobAndEventServiceTests"), TestMethod]
        public void JobPlanCRUD()
        {
            AddWell("RPOC_");
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.JobPlan);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.JobPlan);
            long evtId = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(evtId > 0, "Failed to add Job Plan");
            JobLightDTO[] Job = JobAndEventService.GetJobsByWell(getJob.WellId.ToString());
            Assert.IsTrue(Job.FirstOrDefault(j => j.JobId == getJob.JobId).ExistingJobEvents.HasJobPlanEvent);

            CatalogItemGroupDTO catalogDTO = JobAndEventService.GetCatalogItemGroupData();

            //updating Job Plan
            JobPlanDetailsDTO jobPlan = new JobPlanDetailsDTO();
            jobPlan.BusinessOrganziationId = catalogDTO.CatalogItems.OrderBy(x => x.CatalogItemId).FirstOrDefault().VendorId;
            jobPlan.JobId = getJob.JobId;
            jobPlan.ResponsiblePerson = "Test Only";
            jobPlan.TruckUnitId = catalogDTO.TruckUnits.Single(x => x.VendorId == jobPlan.BusinessOrganziationId).TruckUnitId;
            jobPlan.JobPlanEventId = evtId;
            JobPlanDetailsDTO jobPlanCheck = JobAndEventService.UpdateJobPlan(jobPlan);
            Assert.IsTrue(jobPlanCheck.JobId == jobPlan.JobId, "Job Plan is not created successfully");

            //Fetching Job Plan details
            JobPlanDetailsDTO resJobPlan = JobAndEventService.GetJobPlanDetails(jobId);
            Assert.AreEqual(jobPlan.JobId, resJobPlan.JobId, "Retrieved Job Id is not matching with the Job Id sent in request");
            Assert.AreEqual(jobPlan.BusinessOrganziationId, resJobPlan.BusinessOrganziationId, "Retrieved Business organization Id is not matching with the Business organization Id sent in request");
            Assert.AreEqual(jobPlan.ResponsiblePerson, resJobPlan.ResponsiblePerson, "Retrieved Responsible Person is not matching with the Responsible Person sent in request");
            Assert.AreEqual(jobPlan.TruckUnitId, resJobPlan.TruckUnitId, "Retrieved TruckUnit Id is not matching with the TruckUnit Id sent in request");
            Assert.AreEqual(jobPlan.JobPlanEventId, resJobPlan.JobPlanEventId, "Retrieved JobPlanEventId is not matching with the JobPlanEventId sent in request");
            Assert.AreEqual(0, resJobPlan.AggregateHours, "Since Planned events are not added, Aggregate hours should be 0");
            //Assert.IsNull(resJobPlan.EventPlanDetails, "Since Planned events are not added, EventPlanDetails should be null");

            //Removing Job Plan Event
            EventDTO evtDetails = new EventDTO();
            evtDetails.PK_Job = getJob.JobId;
            evtDetails.Id = evtId;
            evtDetails.EventTypeId = jobEventTypes.JobPlan;
            JobAndEventService.RemoveEvent(evtDetails);
        }

        [TestCategory("JobAndEventServiceTests"), TestMethod]
        public void GetJobStatusView()
        {
            long wellId = AddWell("RPOC_").Id;
            JobStatusDTO[] js = JobAndEventService.GetJobStatuses();
            for (int i = 0; i < js.Count(); i++)
            {
                string jobId = AddJob(js[i].Name);
                bool check = JobAndEventService.IsJobApprovable(jobId);
                Trace.WriteLine(js[i].Name + "---" + check.ToString());
            }
            JobLightDTO[] jobs = JobAndEventService.GetJobsByWell(wellId.ToString());
            Assert.AreEqual(js.Count(), jobs.Count());
            WellFilterDTO wellFilter = new WellFilterDTO();

            JobStatusViewDTO[] jsp = JobAndEventService.GetJobsByCategory(wellFilter, JobStatusCategory.Prospective.ToString("d"));
            JobStatusViewDTO[] jsr = JobAndEventService.GetJobsByCategory(wellFilter, JobStatusCategory.Ready.ToString("d"));
            JobStatusViewDTO[] jsc = JobAndEventService.GetJobsByCategory(wellFilter, JobStatusCategory.Concluded.ToString("d"));

            for (int i = 0; i < jsp.Count(); i++)
            {
                Assert.AreEqual("TestJobRemarks - " + jsp[i].Status, jsp[i].JobRemarks, "Incorrect Job Remark retrieved in Prospective Job");
                Trace.WriteLine("Remark of Prospective Job " + (i + 1) + " is: " + jsp[i].JobRemarks);
            }

            for (int i = 0; i < jsr.Count(); i++)
            {
                Assert.AreEqual("TestJobRemarks - " + jsr[i].Status, jsr[i].JobRemarks, "Incorrect Job Remark retrieved in Ready Job");
                Trace.WriteLine("Remark of Ready Job " + (i + 1) + " is: " + jsr[i].JobRemarks);
            }

            for (int i = 0; i < jsc.Count(); i++)
            {
                Assert.AreEqual("TestJobRemarks - " + jsc[i].Status, jsc[i].JobRemarks, "Incorrect Job Remark retrieved in Concluded Job");
                Trace.WriteLine("Remark of Concluded Job " + (i + 1) + " is: " + jsc[i].JobRemarks);
            }
        }

        [TestCategory("JobAndEventServiceTests"), TestMethod]
        public void AddGetUpdateTemplateJob()
        {
            TemplateJobDTO templateJob = new TemplateJobDTO();
            JobLightDTO newJob = new JobLightDTO();
            newJob.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault().id;
            newJob.JobReasonId = JobAndEventService.GetJobReasonsForJobType(JobAndEventService.GetJobTypes().FirstOrDefault().id.ToString()).FirstOrDefault().Id;
            templateJob.Job = newJob;
            templateJob.Category = "JobCategory-1";
            string templateID = JobAndEventService.AddTemplateJob(templateJob);
            Assert.IsNotNull(templateID, "Template job not added successfully");
            TemplateJobGroupingDTO[] getTempJob = JobAndEventService.GetTemplateJobs();
            Assert.IsNotNull(getTempJob, "Template job not added successfully");
            Assert.AreEqual(1, getTempJob.Count(), "Template job category is not added successfully");
            foreach (TemplateJobGroupingDTO tj in getTempJob)
            {
                Assert.IsNotNull(tj.GroupingCategory, "Category is not added successfully");
                Assert.AreEqual("JobCategory-1", tj.GroupingCategory, "Mismatch in added Template Job category");
                Assert.IsNotNull(tj.TemplateJobs, "No Job added to the Template");
                Assert.AreEqual(1, tj.TemplateJobs.Count());
                foreach (TemplateJobDTO job in tj.TemplateJobs)
                {
                    Assert.IsNotNull(job.JobId);
                    Assert.AreEqual(newJob.JobTypeId, job.Job.JobTypeId);
                    Assert.AreEqual(newJob.JobReasonId, job.Job.JobReasonId);
                }
            }
            templateJob.Category = "JobCategory-2";
            templateID = JobAndEventService.AddTemplateJob(templateJob);
            Assert.IsNotNull(templateID, "Template job not added successfully");
            getTempJob = JobAndEventService.GetTemplateJobs();
            Assert.IsNotNull(getTempJob, "Template job not added successfully");
            Assert.AreEqual(2, getTempJob.Count(), "Template job category is not added successfully");
            foreach (TemplateJobGroupingDTO tj in getTempJob)
            {
                foreach (TemplateJobDTO job in tj.TemplateJobs)
                {
                    job.Category = "JobCategory";
                    bool check = JobAndEventService.UpdateTemplateJob(job);
                    Assert.IsTrue(check, "Failed to update Template Job Category");
                }
            }
            getTempJob = JobAndEventService.GetTemplateJobs();
            Assert.IsNotNull(getTempJob, "Template job not updated successfully");
            Assert.AreEqual(1, getTempJob.Count(), "Template job category is not updated successfully");
            foreach (TemplateJobGroupingDTO tj in getTempJob)
            {
                Assert.IsNotNull(tj.GroupingCategory, "Category is not updated successfully");
                Assert.AreEqual("JobCategory", tj.GroupingCategory, "Mismatch in updated Template Job category");
                Assert.IsNotNull(tj.TemplateJobs, "No Job updated to the Template");
                Assert.AreEqual(2, tj.TemplateJobs.Count());
                foreach (TemplateJobDTO job in tj.TemplateJobs)
                {
                    Assert.IsNotNull(job.JobId);
                    Assert.AreEqual(newJob.JobTypeId, job.Job.JobTypeId);
                    Assert.AreEqual(newJob.JobReasonId, job.Job.JobReasonId);
                }
            }
            foreach (TemplateJobGroupingDTO tj in getTempJob)
            {
                foreach (TemplateJobDTO job in tj.TemplateJobs)
                {
                    bool check = JobAndEventService.RemoveTemplateJob(job.TemplateJobId.ToString());
                    Assert.IsTrue(check, "Added/Updated Template job deleted successfully");
                }
            }
        }

        /// <summary>
        /// Integration test created for FRWM-4100.
        /// Author:Rahul Pingale
        /// </summary>
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void FSMSynchWithConfig_Part1()
        {
            string facilityId1 = GetFacilityId("RPOC_", 1);
            WellConfigDTO WellConfig = AddWellWithSurfaceAndWeight(facilityId1);
            string wellId = WellConfig.Well.Id.ToString();
            long assemblyId = WellConfig.Well.AssemblyId;
            long subassemblyId = WellConfig.Well.SubAssemblyId;
            Trace.WriteLine("Well created successfully");

            string jobId = AddJob("Planned", -90);
            var getjob = JobAndEventService.GetJobById(jobId);
            Trace.WriteLine("Job created created successfully");

            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long eventId = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(eventId > 0, "Failed to add Wellbore Report");

            //Test1 :- Verify that WellConfig screen is in synch with FSM wellbore tab. Downhole & Rod tab should be populated based on Wellbore components.
            // Data inserted in Tuple  (Component name,{Caomponent Group name,Part type,Manufacturer,Catalog Item Desc},{Inner Diameter,Outer Diameter},{Quantity,Length,TopDepth,Wellbore Perforation Status})
            var components = new List<Tuple<string, string[], double[], int[]>>
            {
               Tuple.Create("Tubing",new string[4]{"Tubing String","Tubing - OD  1.050","Hydril","C-110  1.050\" OD/1.50#  0.742\" ID  0.648\" Drift" },new double[2]{0.742,1.050 },new int[4]{50,1000,100,1}),
               Tuple.Create("Pump",new string[4]{"Tubing String", "Tubing Pump",  "_Generic Manufacturer", "Tubing Pump -   1.315\" Nominal" },new double[2]{ 0, 1.315},new int[4]{ 1, 100, 600,1 }),
               Tuple.Create("Anchor",new string[4]{"Tubing String", "Tubing Anchor/Catcher",  "_Generic Manufacturer", "Tubing Anchor/Catcher 1.900\"" }, new double[2] { 1.67, 2 }, new int[4] { 1, 200, 300,1 }),
               Tuple.Create("Prod Casing",new string[4]{"Production Casing", "Casing/Casing Liner OD  2.875",  "Hydril", "J-55  2.875\" OD/6.40#  2.441\" ID  2.347\" Drift" }, new double[2] { 2.441, 2.875 }, new int[4] { 2, 1000, 200,1}),
               Tuple.Create("Borehole",new string[4]{"Borehole", "Wellbore Completion Detail (Perforations, etc.)",  "_Generic Manufacturer", "Slotted Casing/Liner - 4.500" }, new double[2] { 0, 4.500}, new int[4] { 1, 400, 900,2 }),
               Tuple.Create("Rod",new string[4]{"Rod String", "Rod",  "Alberta Oil Tools", "AOT-54  - 0.750   " }, new double[2] { 0, 0.750 }, new int[4] { 20, 600, 50,1 })
            };

            // public void AddComponentForWellConfig(string wellId, string assemblyId, string subassemblyId, string jobId, string CompGp, string parttype, string CompName, string manufacturer, string CatalogItem, string InnerDiameter, string OuterDiameter, int Quantity, int Length, int TopDepth, int wellBorePerfStatus = 7)

            foreach (Tuple<string, string[], double[], int[]> comp in components)
            {
                AddComponentForWellConfig(wellId, assemblyId, subassemblyId, jobId, eventId, comp.Item1, comp.Item2.ElementAt(0), comp.Item2.ElementAt(1), comp.Item2.ElementAt(2), comp.Item2.ElementAt(3), Convert.ToDecimal(comp.Item3.ElementAt(0)), Convert.ToDecimal(comp.Item3.ElementAt(1)), comp.Item4.ElementAt(0), comp.Item4.ElementAt(1), comp.Item4.ElementAt(2), comp.Item4.ElementAt(3));
            }

            Trace.WriteLine("All the components are added");

            WellConfigDTO wellConfig = WellConfigurationService.GetWellConfig(wellId);

            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Pump").Item3.ElementAt(1), wellConfig.ModelConfig.Downhole.PumpDiameter);
            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Pump").Item4.ElementAt(2), wellConfig.ModelConfig.Downhole.PumpDepth);
            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Tubing").Item3.ElementAt(0), wellConfig.ModelConfig.Downhole.TubingID);
            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Tubing").Item3.ElementAt(1), wellConfig.ModelConfig.Downhole.TubingOD);
            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Anchor").Item4.ElementAt(2), wellConfig.ModelConfig.Downhole.TubingAnchorDepth);
            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Prod Casing").Item3.ElementAt(1), wellConfig.ModelConfig.Downhole.CasingOD);
            StringAssert.Contains(components.FirstOrDefault(x => x.Item1 == "Prod Casing").Item2.ElementAt(3) + "#", wellConfig.ModelConfig.Downhole.CasingWeight.ToString());
            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Borehole").Item4.ElementAt(2), wellConfig.ModelConfig.Downhole.TopPerforation);
            Assert.AreEqual((components.FirstOrDefault(x => x.Item1 == "Borehole").Item4.ElementAt(1) + components.FirstOrDefault(x => x.Item1 == "Borehole").Item4.ElementAt(2)), wellConfig.ModelConfig.Downhole.BottomPerforation);
            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Rod").Item2.ElementAt(2), wellConfig.ModelConfig.Rods.RodTapers.FirstOrDefault().Manufacturer);
            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Rod").Item3.ElementAt(1), wellConfig.ModelConfig.Rods.RodTapers.FirstOrDefault().Size);
            Assert.AreEqual((components.FirstOrDefault(x => x.Item1 == "Rod").Item4.ElementAt(1) / components.FirstOrDefault(x => x.Item1 == "Rod").Item4.ElementAt(0)), wellConfig.ModelConfig.Rods.RodTapers.FirstOrDefault().RodLength);
            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Rod").Item4.ElementAt(0), wellConfig.ModelConfig.Rods.RodTapers.FirstOrDefault().NumberOfRods);
            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Rod").Item4.ElementAt(1), wellConfig.ModelConfig.Rods.RodTapers.FirstOrDefault().TaperLength);

            //Test2 :- If Wellbore components are removed then Downhole and Rod tab fields should be set to 0
            RemoveComponents(jobId, eventId);
            WellConfigDTO wellConfig1 = WellConfigurationService.GetWellConfig(wellId);

            Assert.AreEqual(0, wellConfig1.ModelConfig.Downhole.PumpDiameter);
            Assert.AreEqual(0, wellConfig1.ModelConfig.Downhole.PumpDepth);
            Assert.AreEqual(0, wellConfig1.ModelConfig.Downhole.TubingID);
            Assert.AreEqual(0, wellConfig1.ModelConfig.Downhole.TubingOD);
            Assert.AreEqual(0, wellConfig1.ModelConfig.Downhole.TubingAnchorDepth);
            Assert.AreEqual(0, wellConfig1.ModelConfig.Downhole.CasingOD);
            StringAssert.Contains("0", wellConfig1.ModelConfig.Downhole.CasingWeight.ToString());
            Assert.AreEqual(0, wellConfig1.ModelConfig.Downhole.TopPerforation);
            Assert.AreEqual(0, wellConfig1.ModelConfig.Downhole.BottomPerforation);
            Assert.IsTrue(wellConfig1.ModelConfig.Rods.RodTapers.Count() == 0);
            Assert.IsTrue(wellConfig1.ModelConfig.Rods.TotalRodLength == 0);
        }

        /// <summary>
        /// Integration test created for FRWM-4100.
        /// Author:Rahul Pingale
        /// </summary>
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void FSMSynchWithConfig_Part2()
        {
            string facilityId1 = GetFacilityId("RPOC_", 1);
            WellConfigDTO WellConfig = AddWellWithSurfaceAndWeight(facilityId1);
            string wellId = WellConfig.Well.Id.ToString();
            long assemblyId = WellConfig.Well.AssemblyId;
            long subassemblyId = WellConfig.Well.SubAssemblyId;
            Trace.WriteLine("Well created successfully");

            string jobId = AddJob("Planned", -90);
            var getjob = JobAndEventService.GetJobById(jobId);
            Trace.WriteLine("Job created created successfully");

            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long eventId = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(eventId > 0, "Failed to add Wellbore Report");

            //Test3 :- If Wellbore Perforation status is other than "Open" for Wellbore Completetion Detail component then Top and Bottom perforation should be set to 0
            var components = new List<Tuple<string, string[], double[], int[]>>
            {
               Tuple.Create("Tubing",new string[4]{"Tubing String","Tubing - OD  1.050","Hydril","C-110  1.050\" OD/1.50#  0.742\" ID  0.648\" Drift" },new double[2]{0.742,1.050},new int[4]{50,1000,100,1}),
               Tuple.Create("Pump",new string[4]{ "Rod String", "Rod Pump (Insert)",  "_Generic Manufacturer", "Rod Pump (Insert) -  1.115\" Nominal" },new double[2]{ 0, 1.115},new int[4]{ 1, 100, 600,1 }),
               Tuple.Create("Anchor",new string[4]{"Tubing String", "Tubing Anchor/Catcher",  "_Generic Manufacturer", "Tubing Anchor/Catcher 1.900\"" }, new double[2] { 1.67, 2 }, new int[4] { 1, 200, 300,1 }),
               Tuple.Create("Prod Casing",new string[4]{"Production Casing", "Casing/Casing Liner OD  2.875",  "Hydril", "J-55  2.875\" OD/6.40#  2.441\" ID  2.347\" Drift" }, new double[2] { 2.441, 2.875 }, new int[4] { 2, 1000, 200,1}),
               Tuple.Create("Borehole",new string[4]{"Borehole", "Wellbore Completion Detail (Perforations, etc.)",  "_Generic Manufacturer", "Slotted Casing/Liner - 4.500" }, new double[2] { 0, 4.500}, new int[4] { 1, 400, 900,1 }),
               Tuple.Create("Rod",new string[4]{"Rod String", "Rod",  "Alberta Oil Tools", "AOT-54  - 0.750   " }, new double[2] { 0, 0.750 }, new int[4] { 20, 600, 50,1 })
            };

            // public void AddComponentForWellConfig(string wellId, string assemblyId, string subassemblyId, string jobId, string CompGp, string parttype, string CompName, string manufacturer, string CatalogItem, string InnerDiameter, string OuterDiameter, int Quantity, int Length, int TopDepth, int wellBorePerfStatus = 7)

            foreach (Tuple<string, string[], double[], int[]> comp in components)
            {
                AddComponentForWellConfig(wellId, assemblyId, subassemblyId, jobId, eventId, comp.Item1, comp.Item2.ElementAt(0), comp.Item2.ElementAt(1), comp.Item2.ElementAt(2), comp.Item2.ElementAt(3), Convert.ToDecimal(comp.Item3.ElementAt(0)), Convert.ToDecimal(comp.Item3.ElementAt(1)), comp.Item4.ElementAt(0), comp.Item4.ElementAt(1), comp.Item4.ElementAt(2), comp.Item4.ElementAt(3));
            }

            Trace.WriteLine("All the components added");

            WellConfigDTO resWellConfig = WellConfigurationService.GetWellConfig(wellId);

            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Pump").Item3.ElementAt(1), resWellConfig.ModelConfig.Downhole.PumpDiameter);
            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Pump").Item4.ElementAt(2), resWellConfig.ModelConfig.Downhole.PumpDepth);
            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Tubing").Item3.ElementAt(0), resWellConfig.ModelConfig.Downhole.TubingID);
            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Tubing").Item3.ElementAt(1), resWellConfig.ModelConfig.Downhole.TubingOD);
            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Anchor").Item4.ElementAt(2), resWellConfig.ModelConfig.Downhole.TubingAnchorDepth);
            Assert.AreEqual(components.FirstOrDefault(x => x.Item1 == "Prod Casing").Item3.ElementAt(1), resWellConfig.ModelConfig.Downhole.CasingOD);
            StringAssert.Contains(components.FirstOrDefault(x => x.Item1 == "Prod Casing").Item2.ElementAt(3) + "#", resWellConfig.ModelConfig.Downhole.CasingWeight.ToString());
            Assert.AreEqual(0, resWellConfig.ModelConfig.Downhole.TopPerforation);
            Assert.AreEqual(0, resWellConfig.ModelConfig.Downhole.BottomPerforation);

            RemoveComponents(jobId, eventId);

            //Adding components for for Test 4 to 8
            components = new List<Tuple<string, string[], double[], int[]>>
            {
               Tuple.Create("Pump1",new string[4]{"Tubing String", "Tubing Pump", "_Generic Manufacturer", "Tubing Pump -   1.660\" Nominal" },new double[2]{0,1.660 },new int[4]{1,100,600,1}),
               Tuple.Create("Pump2",new string[4]{"Tubing String", "Tubing Pump",  "_Generic Manufacturer", "Tubing Pump -   1.315\" Nominal" },new double[2]{ 0, 1.315},new int[4]{ 1, 90, 700,1 }),
               Tuple.Create("Tubing1",new string[4]{"Tubing String","Tubing - OD  1.050","Hydril","C-110  1.050\" OD/1.50#  0.742\" ID  0.648\" Drift" },new double[2]{0.742,1.050},new int[4]{50,900,100,1}),
               Tuple.Create("Tubing2",new string[4]{"Tubing String", "Tubing - OD  1.315", "_Generic Manufacturer", "Armco-95  1.315\" OD/1.43#  1.097\" ID  1.003\" Drift" },new double[2]{1.097,1.315 },new int[4]{50,1000,100,1}),
               Tuple.Create("Anchor1",new string[4]{"Tubing String", "Tubing Anchor/Catcher",  "_Generic Manufacturer", "Tubing Anchor/Catcher-Hydraulic 1.900\"" }, new double[2] { 1.61, 1.9 }, new int[4] { 1, 100, 350,1 }),
               Tuple.Create("Anchor2",new string[4]{"Tubing String", "Tubing Anchor/Catcher",  "_Generic Manufacturer", "Tubing Anchor/Catcher 1.900\"" }, new double[2] { 1.67, 2 }, new int[4] { 1, 120, 300,1 }),
               Tuple.Create("Prod Casing1",new string[4]{"Production Casing", "Casing/Casing Liner OD  2.875",  "Hydril", "J-55  2.875\" OD/6.40#  2.441\" ID  2.347\" Drift" }, new double[2] { 2.441, 2.875 }, new int[4] { 2, 1000, 200,1}),
               Tuple.Create("Prod Casing2",new string[4]{"Production Casing", "Casing/Casing Liner OD  3.500", "Star Fiberglass", "Fiberglass-Aromatic Amine  3.460\" OD/2.23# Series 1250  3.000\" ID  2.880\" Drift" }, new double[2] { 3.000, 3.460 }, new int[4] { 1, 1200, 100,1}),
               Tuple.Create("Borehole1",new string[4]{"Borehole", "Wellbore Completion Detail (Perforations, etc.)",  "_Generic Manufacturer", "Slotted Casing/Liner - 4.500" }, new double[2] { 0, 4.500}, new int[4] { 1, 50, 500,2 }),
               Tuple.Create("Borehole2",new string[4]{"Borehole", "Wellbore Completion Detail (Perforations, etc.)",  "_Generic Manufacturer", "Slotted Casing/Liner - 4.500" }, new double[2] { 0, 4.500}, new int[4] { 1, 150, 700,2 }),
               Tuple.Create("Rod1",new string[4]{"Rod String", "Rod",  "Alberta Oil Tools", "AOT-54  - 0.750   " }, new double[2] { 0, 0.750 }, new int[4] { 20, 600, 50,1 }),
               Tuple.Create("Rod2",new string[4]{"Rod String", "Rod", "Fibercom", "0.990 (1 in.) FG  x 37.5 Rod" }, new double[2] { 0, 0.99 }, new int[4] { 50, 900, 100,1 })
            };

            // public void AddComponentForWellConfig(string wellId, string assemblyId, string subassemblyId, string jobId, string CompGp, string parttype, string CompName, string manufacturer, string CatalogItem, string InnerDiameter, string OuterDiameter, int Quantity, int Length, int TopDepth, int wellBorePerfStatus = 7)

            foreach (Tuple<string, string[], double[], int[]> comp in components)
            {
                AddComponentForWellConfig(wellId, assemblyId, subassemblyId, jobId, eventId, comp.Item1, comp.Item2.ElementAt(0), comp.Item2.ElementAt(1), comp.Item2.ElementAt(2), comp.Item2.ElementAt(3), Convert.ToDecimal(comp.Item3.ElementAt(0)), Convert.ToDecimal(comp.Item3.ElementAt(1)), comp.Item4.ElementAt(0), comp.Item4.ElementAt(1), comp.Item4.ElementAt(2), comp.Item4.ElementAt(3));
            }

            resWellConfig = WellConfigurationService.GetWellConfig(wellId);
            //Test4 :- If more than one Pump is present then Pump Diameter and Pump Depth will be taken from pump having largest Diameter
            Assert.AreEqual(1.660, resWellConfig.ModelConfig.Downhole.PumpDiameter);
            Assert.AreEqual(600, resWellConfig.ModelConfig.Downhole.PumpDepth);
            //Test5 :- If more than one Tubing is present then TubingID and TubingOD will be taken from Tubing having longest length
            Assert.AreEqual(1.097, resWellConfig.ModelConfig.Downhole.TubingID);
            Assert.AreEqual(1.315, resWellConfig.ModelConfig.Downhole.TubingOD);
            //Test6 :- If more than one Tubing Anchor/catcher is present then TubingAnchorDepth will be taken from Tubing Anchor/catcher having longest BottomDepth
            Assert.AreEqual(350, resWellConfig.ModelConfig.Downhole.TubingAnchorDepth);
            //Test7 :- If more than one Production Casing is present then CasingOD & CasingWeight will be taken from Production Casing having longest length
            Assert.AreEqual(3.460, resWellConfig.ModelConfig.Downhole.CasingOD);
            Assert.AreEqual(2.23, resWellConfig.ModelConfig.Downhole.CasingWeight);
            //Test8 :- Borehole1 component has Top Depth 500 & Bottom Depth 550 . Borehole2 component has Top Depth 700 & Bottom Depth 850
            //so Top perforation on WellConfig will be 500 & Bottom Perforation will be 850
            Assert.AreEqual(500, resWellConfig.ModelConfig.Downhole.TopPerforation);
            Assert.AreEqual(850, resWellConfig.ModelConfig.Downhole.BottomPerforation);
            Assert.AreEqual(components.FindAll(x => x.Item1.StartsWith("Rod")).Count().ToString(), resWellConfig.ModelConfig.Rods.RodTapers.Count().ToString());

            var rodComponents = components.FindAll(x => x.Item1.StartsWith("Rod"));
            int i = 0;
            foreach (Tuple<string, string[], double[], int[]> rod in rodComponents)
            {
                Assert.AreEqual(rod.Item2.ElementAt(2), resWellConfig.ModelConfig.Rods.RodTapers.ElementAt(i).Manufacturer);
                Assert.AreEqual(rod.Item3.ElementAt(1), resWellConfig.ModelConfig.Rods.RodTapers.ElementAt(i).Size);
                Assert.AreEqual(rod.Item4.ElementAt(1) / rod.Item4.ElementAt(0), resWellConfig.ModelConfig.Rods.RodTapers.ElementAt(i).RodLength);
                Assert.AreEqual(rod.Item4.ElementAt(0), resWellConfig.ModelConfig.Rods.RodTapers.ElementAt(i).NumberOfRods);
                Assert.AreEqual(rod.Item4.ElementAt(1), resWellConfig.ModelConfig.Rods.RodTapers.ElementAt(i).TaperLength);
                i = i + 1;
            }
        }

        [TestCategory("JobAndEventServiceTests"), TestMethod]
        public void AddMultiAddEvent()
        {
            AddWell("RPOC_");
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            EventTypeGroupDTO allEvents = JobAndEventService.GetEventTypesByGrouping(getJob.JobTypeId.ToString(), getJob.JobId.ToString());
            EventTypeDTO[] allEventsbyJobType = allEvents.AllEvents;
            int numEvents = 0;
            foreach (EventTypeDTO evt in allEventsbyJobType)
            {
                EventDTO setEvent = SetMultiAddEventDTO(getJob, evt);
                bool addSucceeded = false;
                try
                {
                    EventDTO[] arrEvents = new[] { setEvent };
                    addSucceeded = JobAndEventService.AddMultipleEventForJobEventType(arrEvents);
                    Assert.IsTrue(addSucceeded, "Failed to add Event : " + setEvent.EventType);
                }
                catch
                {
                    Trace.WriteLine("Failed to add event: " + setEvent.EventType);
                }
                if (addSucceeded)
                {
                    EventGroupDTO[] addedEvt = JobAndEventService.GetEvents(jobId);
                    setEvent.Id = addedEvt.FirstOrDefault().EventData.FirstOrDefault().Id;
                    numEvents = numEvents + 1;
                    bool rcheck = JobAndEventService.RemoveEvent(setEvent);
                    Assert.IsTrue(rcheck, "Failed to remove added event: " + setEvent.EventType);
                }
            }
            Trace.WriteLine("Total Events added: " + numEvents.ToString());
            Assert.AreEqual(allEventsbyJobType.Count(), numEvents);
        }

        public EventDTO SetMultiAddEventDTO(JobLightDTO Job, EventTypeDTO Event)
        {
            EventDTO evtMultiAdd = new EventDTO();
            evtMultiAdd.BeginTime = DateTime.UtcNow.AddDays(2);
            evtMultiAdd.EndTime = DateTime.UtcNow.AddDays(3);
            evtMultiAdd.EventType = Event.EventTypeName;
            evtMultiAdd.EventTypeId = Event.EventTypeId;
            evtMultiAdd.PK_BusinessOrganization = Job.BusinessOrganizationId == null ? 0 : (long)Job.BusinessOrganizationId;
            evtMultiAdd.PK_TruckUnit = 1;
            evtMultiAdd.HistoricalRate = 234;
            evtMultiAdd.PK_CatalogItem = 1;
            evtMultiAdd.PK_Job = Job.JobId;

            return evtMultiAdd;
        }

        [TestCategory("JobAndEventServiceTests"), TestMethod]
        public void ApproveJob()
        {
            AddWell("RPOC_");
            string jobId = AddJob("Planned");
            bool approveJob = JobAndEventService.ApproveJob(jobId);
            Assert.IsTrue(approveJob, "Failed to approve planned Job");
            JobApprovalDTO[] jobApprovals = JobAndEventService.GetJobApprovals(jobId);
            Assert.IsNotNull(jobApprovals, "Failed to get Job Approvals");
            Assert.AreEqual(1, jobApprovals.Count(), "Mismatch between the Available Jobs and Job Approvals count");
        }

        [TestCategory("JobAndEventServiceTests"), TestMethod]
        public void AddTemplateJobFromPlannedEvents()
        {
            AddWell("RPOC_");
            string jobId = AddJob("Planned");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.JobPlan);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.JobPlan);
            var check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
            JobLightDTO[] Job = JobAndEventService.GetJobsByWell(getJob.WellId.ToString());
            Assert.IsTrue(Job.FirstOrDefault(j => j.JobId == getJob.JobId).ExistingJobEvents.HasJobPlanEvent);
            EventDetailScheduledActivityDTO addJobPlanDetail = CreateJobPlanDetailDTO(jobId);
            addJobPlanDetail.EventTypeId = JobAndEventService.GetAllEventTypesForJobType(getJob.JobTypeId.ToString()).FirstOrDefault().EventTypeId;
            //Add Job Plan
            string addedJobPlanId = JobAndEventService.AddJobPlanDetail(addJobPlanDetail);
            Assert.IsNotNull(addedJobPlanId, "Failed to add Job plan");
            TemplateJobDTO tj = new TemplateJobDTO();
            tj.JobId = getJob.JobId;
            tj.Job = getJob;
            tj.Category = "Automated Category";
            bool addTemplateJob = JobAndEventService.AddTemplateJobFromPlannedEvents(tj);
            Assert.IsTrue(addTemplateJob, "Failed to Add Template Job from planned events");
            TemplateJobGroupingDTO[] templateJobs = JobAndEventService.GetTemplateJobs();
            Assert.IsNotNull(templateJobs, "Failed to get Template jobs added from planned Events");
            Assert.AreEqual(1, templateJobs.Count(), "Mismatch between the Added and Available categories");
            Assert.AreEqual(1, templateJobs.FirstOrDefault().TemplateJobs.Count(), "Mismatch between the Added and Available template Jobs");
            Assert.AreEqual("Automated Category", templateJobs.FirstOrDefault().GroupingCategory, "Mismatch between the Added and obtained category");
            //Get Planned Events for the added Template Job
            JobPlanDetailsDTO jobPlan = JobAndEventService.GetJobPlanDetails(templateJobs.FirstOrDefault().TemplateJobs.FirstOrDefault().JobId.ToString());
            Assert.IsNotNull(jobPlan, "Failed to retrieve the job plan for the added template from the planned events");
            Assert.AreEqual(templateJobs.FirstOrDefault().TemplateJobs.FirstOrDefault().JobId, jobPlan.EventPlanDetails.FirstOrDefault().JobId, "Mismatch between the JobId for Template Job and planned Event");
            //Remove Template Job
            bool rcheck = JobAndEventService.RemoveTemplateJob(templateJobs.FirstOrDefault().TemplateJobs.FirstOrDefault().TemplateJobId.ToString());
            Assert.IsTrue(rcheck, "Failed to remove added template Job from planned events");
        }

        [TestCategory("JobAndEventServiceTests"), TestMethod]
        public void AddJobfromTemplate()
        {
            //Add Template Job
            TemplateJobDTO templateJob = new TemplateJobDTO();
            JobLightDTO newJob = new JobLightDTO();
            newJob.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault().id;
            newJob.JobReasonId = JobAndEventService.GetJobReasonsForJobType(JobAndEventService.GetJobTypes().FirstOrDefault().id.ToString()).FirstOrDefault().Id;
            templateJob.Job = newJob;
            templateJob.Category = "JobCategory-1";
            string templateID = JobAndEventService.AddTemplateJob(templateJob);
            Assert.IsNotNull(templateID, "Template job not added successfully");
            TemplateJobGroupingDTO[] getTempJob = JobAndEventService.GetTemplateJobs();
            Assert.IsNotNull(getTempJob, "Template job not added successfully");
            Assert.AreEqual(1, getTempJob.Count(), "Template job category is not added successfully");
            //Add Events for Template Job
            EventDetailScheduledActivityDTO addJobPlanDetail = CreateJobPlanDetailDTO(getTempJob.FirstOrDefault().TemplateJobs.FirstOrDefault().JobId.ToString());
            addJobPlanDetail.EventTypeId = JobAndEventService.GetAllEventTypesForJobType(getTempJob.FirstOrDefault().TemplateJobs.FirstOrDefault().Job.JobTypeId.ToString()).FirstOrDefault().EventTypeId;
            string addedJobPlanId = JobAndEventService.AddJobPlanDetail(addJobPlanDetail);
            Assert.IsNotNull(addedJobPlanId, "Failed to add Job plan");
            //Add Job from Template
            long wellId = AddWell("RPOC_").Id;
            AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(wellId.ToString());
            templateJob = getTempJob.FirstOrDefault().TemplateJobs.FirstOrDefault();
            templateJob.Job.AssemblyId = assembly.Id;
            templateJob.Job.WellId = wellId;
            string addjobId = JobAndEventService.AddJobForTemplateJob(templateJob);
            Assert.IsNotNull(addjobId, "Unable to Add job from Template Job");
            //Get Added Job
            JobLightDTO getJob = JobAndEventService.GetJobById(addjobId);
            Assert.IsNotNull(getJob, "Failed to retrieve Job added from Template Job");
            JobPlanDetailsDTO jobPlan = JobAndEventService.GetJobPlanDetails(addjobId);
            Assert.IsNotNull(jobPlan, "Failed retrieve the Job plan for the job that was added through Template");
            Assert.AreEqual(1, jobPlan.EventPlanDetails.Count());
            //Remove Job
            bool rJob = JobAndEventService.RemoveJob(addjobId);
            Assert.IsTrue(rJob, "Failed to remove added Job");
            //Remove Template Job
            foreach (TemplateJobGroupingDTO tj in getTempJob)
            {
                foreach (TemplateJobDTO job in tj.TemplateJobs)
                {
                    bool check = JobAndEventService.RemoveTemplateJob(job.TemplateJobId.ToString());
                    Assert.IsTrue(check, "Added/Updated Template job deleted successfully");
                }
            }
        }

        [TestCategory(TestCategories.WellServiceTests), TestMethod]
        public void StickyNotesCRUD()
        {
            string wellId = AddWell("RPOC_").Id.ToString();
            UserDTO[] user = AdministrationService.GetUsers();
            AssemblyDTO getAssembly = WellboreComponentService.GetAssemblyByWellId(wellId);
            StickyNoteDTO stickyNotes = new StickyNoteDTO();
            stickyNotes.UserOriginatorId = WellService.GetUserDisplayName().FirstOrDefault().Id;
            stickyNotes.UserRecipientId = WellService.GetUserDisplayName().FirstOrDefault().Id;
            stickyNotes.PK_Assembly = getAssembly.Id;
            stickyNotes.StickyNoteTypeId = WellService.GetStickyNoteType().FirstOrDefault().Id;
            stickyNotes.StickyNoteStatusId = WellService.GetStickyNoteStatus().FirstOrDefault().Id;
            stickyNotes.PriorityId = WellService.GetPriority().FirstOrDefault().Id;
            stickyNotes.BriefDescription = "Automated Brief Description";
            stickyNotes.StickyNoteComments = "Automated Sticky note comments";
            stickyNotes.StickyNoteMessage = "Automated Sticky note Message";
            stickyNotes.OriginDate = DateTime.Today;
            stickyNotes.CompletionDate = DateTime.Today.AddDays(5);

            //Add Sticky Notes
            string stickyNotesID = WellService.AddUpdateStickyNote(stickyNotes);

            Assert.IsNotNull(stickyNotesID, "Failed to Add Sticky Notes");
            //Get Sticky Notes
            StickyNoteDTO[] getStickyNotes = WellService.GetStickyNote(getAssembly.Id.ToString());
            Assert.AreEqual(1, getStickyNotes.Count(), "Failed to retrieve the added Sticky Notes");
            StickyNoteDTO addedNotes = getStickyNotes.FirstOrDefault();
            Assert.IsNotNull(addedNotes.Id);
            Assert.IsNotNull(addedNotes.UserOriginatorId);
            Assert.IsNotNull(addedNotes.UserRecipientId);
            Assert.AreEqual(stickyNotes.UserOriginatorId, addedNotes.UserOriginatorId);
            Assert.AreEqual(stickyNotes.UserRecipientId, addedNotes.UserRecipientId);
            Assert.AreEqual(stickyNotes.PK_Assembly, addedNotes.PK_Assembly);
            Assert.AreEqual(stickyNotes.StickyNoteTypeId, addedNotes.StickyNoteTypeId);
            Assert.AreEqual(stickyNotes.StickyNoteStatusId, addedNotes.StickyNoteStatusId);
            Assert.AreEqual(stickyNotes.PriorityId, addedNotes.PriorityId);
            Assert.AreEqual(stickyNotes.BriefDescription, addedNotes.BriefDescription);
            Assert.AreEqual(stickyNotes.StickyNoteComments, addedNotes.StickyNoteComments);
            Assert.AreEqual(stickyNotes.StickyNoteMessage, addedNotes.StickyNoteMessage);
            Assert.AreEqual(stickyNotes.OriginDate, addedNotes.OriginDate);
            Assert.AreEqual(stickyNotes.CompletionDate, addedNotes.CompletionDate);
            //Update Sticky Notes
            stickyNotes.Id = Convert.ToInt64(stickyNotesID);
            stickyNotes.BriefDescription = "Updated Description";
            stickyNotesID = WellService.AddUpdateStickyNote(stickyNotes);
            Assert.IsNotNull(stickyNotesID, "Failed to Update Sticky Notes");
            getStickyNotes = WellService.GetStickyNote(getAssembly.Id.ToString());
            Assert.AreEqual(1, getStickyNotes.Count(), "Failed to retrieve the added Sticky Notes");
            addedNotes = getStickyNotes.FirstOrDefault();
            Assert.IsNotNull(addedNotes.Id);
            Assert.IsNotNull(addedNotes.UserOriginatorId);
            Assert.IsNotNull(addedNotes.UserRecipientId);
            Assert.AreEqual(stickyNotes.UserRecipientId, addedNotes.UserRecipientId);
            Assert.AreEqual(stickyNotes.PK_Assembly, addedNotes.PK_Assembly);
            Assert.AreEqual(stickyNotes.StickyNoteTypeId, addedNotes.StickyNoteTypeId);
            Assert.AreEqual(stickyNotes.StickyNoteStatusId, addedNotes.StickyNoteStatusId);
            Assert.AreEqual(stickyNotes.PriorityId, addedNotes.PriorityId);
            Assert.AreEqual(stickyNotes.BriefDescription, addedNotes.BriefDescription);
            Assert.AreEqual(stickyNotes.StickyNoteComments, addedNotes.StickyNoteComments);
            Assert.AreEqual(stickyNotes.OriginDate, addedNotes.OriginDate);
            Assert.AreEqual(stickyNotes.CompletionDate, addedNotes.CompletionDate);

            // Call the API Multi rows Edit for Sticky Note Grid:
            //Add 2 more sticky notes 
            StickyNoteDTO stickyNotes2 = new StickyNoteDTO();
            stickyNotes2.UserOriginatorId = WellService.GetUserDisplayName().FirstOrDefault().Id;
            stickyNotes2.UserRecipientId = WellService.GetUserDisplayName().FirstOrDefault().Id;
            stickyNotes2.PK_Assembly = getAssembly.Id;
            stickyNotes2.StickyNoteTypeId = WellService.GetStickyNoteType().FirstOrDefault().Id;
            stickyNotes2.StickyNoteStatusId = WellService.GetStickyNoteStatus().FirstOrDefault().Id;
            stickyNotes2.PriorityId = WellService.GetPriority().FirstOrDefault().Id;
            stickyNotes2.BriefDescription = "Automated Brief Description2";
            stickyNotes2.StickyNoteComments = "Automated Sticky note comments2";
            stickyNotes2.StickyNoteMessage = "Automated Sticky note Message2";
            stickyNotes2.OriginDate = DateTime.Today;
            stickyNotes2.CompletionDate = DateTime.Today.AddDays(5);
            //Thrid note
            StickyNoteDTO stickyNotes3 = new StickyNoteDTO();
            stickyNotes3.UserOriginatorId = WellService.GetUserDisplayName().FirstOrDefault().Id;
            stickyNotes3.UserRecipientId = WellService.GetUserDisplayName().FirstOrDefault().Id;
            stickyNotes3.PK_Assembly = getAssembly.Id;
            stickyNotes3.StickyNoteTypeId = WellService.GetStickyNoteType().FirstOrDefault().Id;
            stickyNotes3.StickyNoteStatusId = WellService.GetStickyNoteStatus().FirstOrDefault().Id;
            stickyNotes3.PriorityId = WellService.GetPriority().FirstOrDefault().Id;
            stickyNotes3.BriefDescription = "Automated Brief Description3";
            stickyNotes3.StickyNoteComments = "Automated Sticky note comments3";
            stickyNotes3.StickyNoteMessage = "Automated Sticky note Message3";
            stickyNotes3.OriginDate = DateTime.Today;
            stickyNotes3.CompletionDate = DateTime.Today.AddDays(5);
            string stickyNotesID2 = WellService.AddUpdateStickyNote(stickyNotes2);
            string stickyNotesID3 = WellService.AddUpdateStickyNote(stickyNotes3);

            StickyNoteDTO[] addednotes = WellService.GetStickyNote(getAssembly.Id.ToString());
            stickyNotes2 = addednotes.FirstOrDefault(x => x.StickyNoteComments == "Automated Sticky note comments2");
            stickyNotes3 = addednotes.FirstOrDefault(x => x.StickyNoteComments == "Automated Sticky note comments3");
            //Get updated records and verify that they are seen in buld update API
            StickyNoteVDTO stickyNoteV = new StickyNoteVDTO
            {
                PageSize = 10,
                CurrentPage = 1
            };
            stickyNoteV.AssemblyId = getAssembly.Id;
            var alleditnotes = WellService.GetStickyNotes(stickyNoteV);
            stickyNotes2.StickyNoteComments = "Grid update comment 2";
            stickyNotes3.StickyNoteComments = "Grid update comment 3";

            WellService.AddUpdateStickyNotes(addednotes);
            StickyNoteVDTO editnotes = WellService.GetStickyNotes(stickyNoteV);
            StickyNoteDTO getsecondnote = editnotes.StickyNotes.FirstOrDefault(x => x.StickyNoteComments == "Grid update comment 2");
            Assert.IsNotNull(getsecondnote, "Second note was not updated in Grid Update");
            StickyNoteDTO getthirdnote = editnotes.StickyNotes.FirstOrDefault(x => x.StickyNoteComments == "Grid update comment 3");
            Assert.IsNotNull(getthirdnote, "Second note was not updated in Grid Update");

            //Delete Sticky Notes
            bool rcheck = WellService.RemoveStickyNote(stickyNotesID);
            Assert.IsTrue(rcheck, "Failed to retrieve added Notes");
            getStickyNotes = WellService.GetStickyNote(getAssembly.Id.ToString());
            // 1 note out of 3 removed so count = 2;
            Assert.AreEqual(2, getStickyNotes.Count(), "Failed to retrieve the added Sticky Notes");

            //Delete Sticky notes Multiple using Grid Multi Selection :
            bool multicheck = WellService.RemoveStickyNotes(new long[] { getsecondnote.Id, getthirdnote.Id });
            Assert.IsTrue(multicheck, "Failed to delete added   Notes");
            getStickyNotes = WellService.GetStickyNote(getAssembly.Id.ToString());
            Assert.AreEqual(0, getStickyNotes.Count(), "Failed to delete grid  sticky Notes");

        }

        public void RemoveAddedTemplateJobForTesting(List<string> templateJobIds)
        {
            bool isSuccess = false;
            if (templateJobIds.Count() > 0)
            {
                foreach (string templateJobId in templateJobIds) { isSuccess = JobAndEventService.RemoveTemplateJob(templateJobId); }
                Assert.IsTrue(isSuccess, "New added Template job deleted successfully.");
            }
        }

        [TestCategory("JobAndEventServiceTests"), TestMethod]
        public void Run_GetTemplateJobsForJobPlanWizard_Test()
        {
            string newTemplateId = string.Empty;
            List<string> newTemplateIds = new List<string>();
            try
            {
                TemplateJobDTO newTestTemplateJob = new TemplateJobDTO();
                JobLightDTO newTestJob = new JobLightDTO();

                //Take any job type
                JobTypeDTO[] allJobTypes = JobAndEventService.GetJobTypes();
                Random randomJobType = new Random();
                int randomJobTypeRowIndex = randomJobType.Next(0, allJobTypes.Length);
                newTestJob.JobTypeId = allJobTypes[randomJobTypeRowIndex].id;

                //Take any job reason from the selected job type above
                JobReasonDTO[] allJobTypeJobReasons = JobAndEventService.GetJobReasonsForJobType(newTestJob.JobTypeId.ToString());
                Random randomJobReason = new Random();
                int randomJobReasonRowIndex = randomJobReason.Next(0, allJobTypeJobReasons.Length);
                newTestJob.JobReasonId = allJobTypeJobReasons[randomJobReasonRowIndex].Id;

                //Assign this DTO to the new template
                newTestTemplateJob.Job = newTestJob;
                newTestTemplateJob.Category = "Run_GetTemplateJobsForJobPlanWizard_Test";
                string newTestTemplateJobId = JobAndEventService.AddTemplateJob(newTestTemplateJob); //Add the template to the database
                JobPlanWizardTemplateJobDTO[] templateJobsInDBForTest = JobAndEventService.GetTemplateJobsForJobPlanWizard();
                Assert.IsNotNull(templateJobsInDBForTest, "templateJobs[] returned from GetTemplateJobsForJobPlanWizard() method is null.");

                //Get all the template jobs currently in the DB, count them and compare the count with the method's return
                int expectedTemplateJobCount = 0;
                List<TemplateJobGroupingDTO> templateJobsInDB = JobAndEventService.GetTemplateJobs().ToList();
                if (templateJobsInDB != null)
                {
                    if (templateJobsInDB.Count() > 0)
                    {
                        //For each of these groups, find out how many template it has
                        foreach (TemplateJobGroupingDTO item in templateJobsInDB) { expectedTemplateJobCount += item.TemplateJobs.Count(); }
                    }
                    else
                    {
                        Assert.Fail("Template job was not added successfully. Total count comes back as 0.");
                    }
                }

                int actualTemplateJobCount = templateJobsInDBForTest.Count();
                Assert.AreEqual(expectedTemplateJobCount, actualTemplateJobCount,
                    string.Format("Expecting {0} templates from Database and getting {1} templates from GetTemplateJobsForJobPlanWizard() method.", expectedTemplateJobCount, actualTemplateJobCount));

                //No error, clean up
                //Find out the group that we just added. We know we added only one group in the DB (Run_GetTemplateJobsForJobPlanWizard_Test)
                TemplateJobGroupingDTO newTestTemplateJobGrouping = JobAndEventService.GetTemplateJobs().ToList().FirstOrDefault(g => g.GroupingCategory.ToUpper() == newTestTemplateJob.Category.ToUpper());

                //For this grouping, add all the template job ids into the list
                if (newTestTemplateJobGrouping != null)
                {
                    foreach (TemplateJobDTO addedNewTemplateJob in newTestTemplateJobGrouping.TemplateJobs) { newTemplateIds.Add(addedNewTemplateJob.TemplateJobId.ToString()); }
                }

                RemoveAddedTemplateJobForTesting(newTemplateIds);
            }
            catch (AssertFailedException)
            {
                RemoveAddedTemplateJobForTesting(newTemplateIds);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("Something went wrong on the test. Message: {0}", ex.Message));
            }
        }

        [TestCategory("JobAndEventServiceTests"), TestMethod]
        public void JobPlanWizardValidations()
        {
            JobPlanWizardValidationDTO validation = JobAndEventService.ValidateJobPlanWizardOnLoad(long.MaxValue.ToString());
            Assert.IsFalse(validation.IsAllowedToProceed);
            Assert.AreEqual(JobPlanWizardOptionValidationResultDescription.CJPW_NoWellInSystem, validation.OptionValidationResultDescription);

            JobPlanWizardValidationDTO emptyWellValidation = JobAndEventService.ValidateExistingJobsForWellForJobPlanWizard("0");
            Assert.IsFalse(emptyWellValidation.IsAllowedToProceed);
            Assert.AreEqual(JobPlanWizardOptionValidationResultDescription.CJPW_NoWellInSystem, emptyWellValidation.OptionValidationResultDescription);

            string wellId = AddWell("RPOC_").Id.ToString();

            //Invalid input i.e. non-parsable well id
            validation = JobAndEventService.ValidateJobPlanWizardOnLoad("I cannot be converted to long");
            Assert.IsFalse(validation.IsAllowedToProceed);
            Assert.AreEqual(JobPlanWizardOptionValidationResultDescription.CJPW_InvalidWellId, validation.OptionValidationResultDescription);

            //Note - Active and Non active validation cannot be done through automation as property not exposed via DTO. And as of 2/15/2018, only physical delete option is implemented with well.

            //Well is active, so next button to be visible
            validation = JobAndEventService.ValidateJobPlanWizardOnLoad(wellId);
            Assert.IsTrue(validation.IsAllowedToProceed);

            ///////////////////////////////// Template Job Validation - Starts ///////////////////////////////////////////
            //No template job in the database
            validation = JobAndEventService.ValidateTemplateJobForJobPlanWizard();
            Assert.IsFalse(validation.IsAllowedToProceed);
            Assert.AreEqual(JobPlanWizardOptionValidationResultDescription.CJPW_NoTemplateJobFound, validation.OptionValidationResultDescription);

            TemplateJobDTO newTestTemplateJob = new TemplateJobDTO();
            JobLightDTO newTestJob = new JobLightDTO();
            newTestJob.JobTypeId = JobAndEventService.GetJobTypes()[0].id;
            newTestJob.JobReasonId = JobAndEventService.GetJobReasonsForJobType(newTestJob.JobTypeId.ToString())[0].Id;
            newTestTemplateJob.Job = newTestJob;
            newTestTemplateJob.Category = "Test Category";
            string associatedJobId = JobAndEventService.AddTemplateJob(newTestTemplateJob);

            validation = JobAndEventService.ValidateTemplateJobForJobPlanWizard();
            Assert.IsTrue(validation.IsAllowedToProceed);
            JobAndEventService.RemoveJob(associatedJobId);
            ///////////////////////////////// Template Job Validation - Ends  ///////////////////////////////////////////

            ///////////////////////////////// Job Validation - Starts  ///////////////////////////////////////////
            validation = JobAndEventService.ValidateExistingJobsForJobPlanWizard();
            Assert.IsFalse(validation.IsAllowedToProceed);
            Assert.AreEqual(JobPlanWizardOptionValidationResultDescription.CJPW_NoJobFound, validation.OptionValidationResultDescription);
            string jobId = AddJob("Approved");
            validation = JobAndEventService.ValidateExistingJobsForJobPlanWizard();
            Assert.IsTrue(validation.IsAllowedToProceed);
            JobAndEventService.RemoveJob(jobId);
            ///////////////////////////////// Job Validation - Ends ///////////////////////////////////////////

            ///////////////////////////////// Job and Well Validation - Starts  ///////////////////////////////////////////
            validation = JobAndEventService.ValidateExistingJobsForWellForJobPlanWizard("I cannot be converted into long");
            Assert.IsFalse(validation.IsAllowedToProceed);
            Assert.AreEqual(JobPlanWizardOptionValidationResultDescription.CJPW_InvalidWellId, validation.OptionValidationResultDescription);

            //No plan event created yet
            validation = JobAndEventService.ValidateExistingJobsForWellForJobPlanWizard(wellId);
            Assert.IsFalse(validation.IsAllowedToProceed);
            Assert.AreEqual(JobPlanWizardOptionValidationResultDescription.CJPW_NoJobPlanForWell, validation.OptionValidationResultDescription);

            string plannedJobId = AddJob("Planned");
            JobLightDTO getJob = JobAndEventService.GetJobById(plannedJobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.JobPlan);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.JobPlan);
            var check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
            JobLightDTO[] Job = JobAndEventService.GetJobsByWell(getJob.WellId.ToString());
            Assert.IsTrue(Job.FirstOrDefault(j => j.JobId == getJob.JobId).ExistingJobEvents.HasJobPlanEvent);

            validation = JobAndEventService.ValidateExistingJobsForWellForJobPlanWizard(getJob.WellId.ToString());
            Assert.IsTrue(validation.IsAllowedToProceed);

            JobAndEventService.RemoveJob(plannedJobId);

            ///////////////////////////////// Job and Well Validation - Ends ///////////////////////////////////////////
        }

        public JobLightDTO JobData(string wellId, string jobStatus)
        {
            WellDTO well = WellService.GetWell(wellId);
            JobLightDTO job = new JobLightDTO();
            job.WellId = well.Id;
            job.WellName = well.Name;
            job.BeginDate = DateTime.Today.AddDays(0).ToUniversalTime();
            job.EndDate = DateTime.Today.AddDays(30).ToUniversalTime();
            job.ActualCost = (decimal)100000.00;
            job.ActualJobDurationDays = (decimal)20.5;
            job.TotalCost = (decimal)150000.00;
            job.JobRemarks = "TestJobRemarks " + DateTime.UtcNow.ToString();
            job.JobOrigin = "TestJobOrigin ";
            job.AssemblyId = well.AssemblyId;
            //For Below fields User can select any value in the dropdown menu
            job.AFEId = JobAndEventService.GetAFEs().FirstOrDefault().Id;
            job.StatusId = JobAndEventService.GetJobStatuses().FirstOrDefault(j => j.Name == jobStatus).Id;
            job.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault().id;
            job.BusinessOrganizationId = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            job.AccountRef = "1";
            //JobReasonId drop down selection is based on the JobTypeId
            job.JobReasonId = JobAndEventService.GetJobReasonsForJobType(job.JobTypeId.ToString()).FirstOrDefault().Id;
            job.JobDriverId = (int)JobAndEventService.GetPrimaryMotivationsForJob().FirstOrDefault().Id;

            return job;
        }

        public JobEconomicAnalysisDTO EconomicanalysisData(CommonProductTypesDTO commonProductTypes)
        {
            JobEconomicAnalysisDTO jobEconomicAnalysis = new JobEconomicAnalysisDTO();
            jobEconomicAnalysis.ProductionForecastScenarioId = 1;
            jobEconomicAnalysis.ProductForecastScenarioWaterId = 1;
            jobEconomicAnalysis.ProductForecastScenarioGasId = 1;
            jobEconomicAnalysis.ProductPriceScenarioId = JobAndEventService.GetProductPriceScenarios(commonProductTypes.OilProductTypeId.Value.ToString()).FirstOrDefault().Id;
            jobEconomicAnalysis.ProductPriceScenarioWaterId = JobAndEventService.GetProductPriceScenarios(commonProductTypes.WaterProductTypeId.Value.ToString()).FirstOrDefault().Id;
            jobEconomicAnalysis.ProductPriceScenarioGasId = JobAndEventService.GetProductPriceScenarios(commonProductTypes.GasProductTypeId.Value.ToString()).FirstOrDefault().Id;
            jobEconomicAnalysis.TaxRateId = JobAndEventService.GetStateTax().FirstOrDefault().Id;
            jobEconomicAnalysis.ProductTypeId = commonProductTypes.OilProductTypeId.Value;
            jobEconomicAnalysis.AnalysisDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Local);
            jobEconomicAnalysis.DPICalcOverriden = true;

            return jobEconomicAnalysis;
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]  //created new method for saving the job plan wizard.
        public void SaveDataForJobPlanWizard()
        // *****
        {
            string wellId = AddWell("RPOC_").Id.ToString();
            JobPlanWizardDTO jobWizard = new JobPlanWizardDTO();
            JobLightDTO jobData = JobData(wellId, "Planned");
            CommonProductTypesDTO commonProductTypes = JobAndEventService.GetCommonProductTypes();
            JobEconomicAnalysisDTO economicAnalysisData = EconomicanalysisData(commonProductTypes);
            EventDetailScheduledActivityDTO[] eventsData = new[] { CreateJobPlanDetailDTO() };
            jobWizard.SelectedOption = "withoutTemplate";
            jobWizard.Job = jobData;
            jobWizard.EconomicAnalysisData = economicAnalysisData;
            jobWizard.PlannedEvents = eventsData;

            string jobId = JobAndEventService.SaveDataForJobPlanWizard(jobWizard);

            //comparison of set and get values.
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);

            JobEconomicAnalysisDTO getEconomicAnlysis = JobAndEventService.GetJobEconomicAnalysis(jobId, wellId);

            //JobPlanDetailsDTO addedJobPlanDetails = JobAndEventService.GetJobPlanDetails(jobId);
            //EventDetailScheduledActivityDTO getJobPlanDetail = addedJobPlanDetails.EventPlanDetails[0];

            //Assert.AreEqual(eventsData.Count(), addedJobPlanDetails.EventPlanDetails.Count(), "Mismatch of entered and actual Event count");

            //For comparision of expected and actual jobData
            JobLightDTO getJob1 = JobAndEventService.GetJobById(jobId);
            Assert.AreEqual(jobData.WellId, getJob.WellId, "Mismatch of entered and actual WellId");
            Assert.AreEqual(jobData.WellName, getJob.WellName, "Mismatch of entered and actual WellName");
            Assert.AreEqual(jobData.BeginDate, getJob.BeginDate, "Mismatch of entered and actual BeginDate");
            Assert.AreEqual(jobData.EndDate, getJob.EndDate, "Mismatch of entered and actual EndDate");
            Assert.AreEqual(jobData.ActualCost, getJob.ActualCost, "Mismatch of entered and actual ActualCost");
            Assert.AreEqual(jobData.ActualJobDurationDays, getJob.ActualJobDurationDays, "Mismatch of entered and actual ActualJobDurationDays");
            Assert.AreEqual(jobData.TotalCost, getJob.TotalCost, "Mismatch of entered and actual TotalCost");
            Assert.AreEqual(jobData.JobRemarks, getJob.JobRemarks, "Mismatch of entered and actual JobRemarks");
            Assert.AreEqual(jobData.JobOrigin, getJob.JobOrigin, "Mismatch of entered and actual JobOrigin");
            Assert.AreEqual(jobData.AssemblyId, getJob.AssemblyId, "Mismatch of entered and actual AssemblyId");
            Assert.AreEqual(jobData.AFEId, getJob.AFEId, "Mismatch of entered and actual AFEId");
            Assert.AreEqual(jobData.StatusId, getJob.StatusId, "Mismatch of entered and actual StatusId");
            Assert.AreEqual(jobData.JobTypeId, getJob.JobTypeId, "Mismatch of entered and actual JobTypeId");
            Assert.AreEqual(jobData.BusinessOrganizationId, getJob.BusinessOrganizationId, "Mismatch of entered and actual BusinessOrganizationId");
            Assert.AreEqual(jobData.AccountRef, getJob.AccountRef, "Mismatch of entered and actual AccountRef");
            Assert.AreEqual(jobData.JobReasonId, getJob.JobReasonId, "Mismatch of entered and actual JobReasonId");
            Assert.AreEqual(jobData.JobDriverId, getJob.JobDriverId, "Mismatch of entered and actual JobDriverId");

            //For comparision of expected and actual economicAnalysisData
            Assert.AreEqual(economicAnalysisData.ProductionForecastScenarioId, getEconomicAnlysis.ProductionForecastScenarioId, "Mismatch of entered and actual ProductionForecastScenarioId");
            Assert.AreEqual(economicAnalysisData.ProductForecastScenarioWaterId, getEconomicAnlysis.ProductForecastScenarioWaterId, "Mismatch of entered and actual ProductForecastScenarioWaterId");
            Assert.AreEqual(economicAnalysisData.ProductForecastScenarioGasId, getEconomicAnlysis.ProductForecastScenarioGasId, "Mismatch of entered and actual ProductForecastScenarioGasId");
            Assert.AreEqual(economicAnalysisData.ProductPriceScenarioId, getEconomicAnlysis.ProductPriceScenarioId, "Mismatch of entered and actual ProductPriceScenarioId");
            Assert.AreEqual(economicAnalysisData.ProductPriceScenarioWaterId, getEconomicAnlysis.ProductPriceScenarioWaterId, "Mismatch of entered and actual ProductPriceScenarioWaterId");
            Assert.AreEqual(economicAnalysisData.ProductPriceScenarioGasId, getEconomicAnlysis.ProductPriceScenarioGasId, "Mismatch of entered and actual ProductPriceScenarioGasId");
            Assert.AreEqual(economicAnalysisData.TaxRateId, getEconomicAnlysis.TaxRateId, "Mismatch of entered and actual TaxRateId");
            Assert.AreEqual(economicAnalysisData.ProductTypeId, getEconomicAnlysis.ProductTypeId, "Mismatch of entered and actual ProductTypeId");
            //Assert.AreEqual(economicAnalysisData.AnalysisDate, getEconomicAnlysis.AnalysisDate, "Mismatch of entered and actual AnalysisDate");

            //Assert.AreEqual(eventsData[0].VendorId, getJobPlanDetail.VendorId, "Mismatch of entered and actual Vendor Id");
            //Assert.AreEqual(eventsData[0].CatalogItemId, getJobPlanDetail.CatalogItemId, "Mismatch of entered and actual CatalogItemId");
            //Assert.AreEqual(eventsData[0].TruckUnitId, getJobPlanDetail.TruckUnitId, "Mismatch of entered and actual TruckUnitId");
            //Assert.AreEqual(eventsData[0].Remarks, getJobPlanDetail.Remarks, "Mismatch of entered and actual Remarks");
            //Assert.AreEqual(eventsData[0].Description, getJobPlanDetail.Description, "Mismatch of entered and actual Description");

            //----------------------------------------------------------Done with "Create a job plan with no template"------------------------------------------------------------------------------------------------

            //--------------Create a job plan based on template ----------------------
            //  Add template job:
            TemplateJobDTO templateJob = new TemplateJobDTO();
            JobLightDTO newJob = new JobLightDTO();
            newJob.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault().id;
            newJob.JobReasonId = JobAndEventService.GetJobReasonsForJobType(JobAndEventService.GetJobTypes().FirstOrDefault().id.ToString()).FirstOrDefault().Id;
            templateJob.Job = newJob;
            templateJob.Category = "JobCategory-1";
            string templateID = JobAndEventService.AddTemplateJob(templateJob);
            Assert.IsNotNull(templateID, "Template job not added successfully");

            //get first record of template jobs
            JobPlanWizardTemplateJobDTO[] templatedto = JobAndEventService.GetTemplateJobsForJobPlanWizard();
            Trace.WriteLine("first template = " + templatedto[0].JobTemplateName);
            Trace.WriteLine("first template = " + templatedto[0].TemplateJobId);

            Assert.IsTrue(templatedto.Count() == 1, "Template Job is missing");

            // Comparison of expected and actual Job type, Job Reason
            char[] charar = { '-' };
            string[] splitJobTypeReason = templatedto[0].JobTemplateName.Split(charar);
            string getJobtypestring = splitJobTypeReason[0].Trim();
            string getJobReasonstring = splitJobTypeReason[1].Trim();

            JobLightDTO getTemplateJobDetails = JobAndEventService.GetJobById(templatedto[0].JobId.ToString());
            Assert.AreEqual(templatedto[0].JobId, getTemplateJobDetails.JobId, "Job ID mismatch");
            Assert.AreEqual(getJobtypestring, getTemplateJobDetails.JobType, "Job Type mismatch");
            Assert.AreEqual(getJobReasonstring, getTemplateJobDetails.JobReason, "Job Reason mismatch");

            EventTypeGroupDTO allEvents = JobAndEventService.GetEventTypesByGrouping(getTemplateJobDetails.JobTypeId.ToString(), getTemplateJobDetails.JobId.ToString());

            //JobPlanWizardDTO jobDetails = new JobPlanWizardDTO();
            //JobLightDTO jobDetails = JobData(wellId, "Planned");
            //string NewjobId = JobAndEventService.SaveDataForJobPlanWizard(jobDetails);

            //--------------Modify the job plan of an existing job for this well.------------------------

            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));

            JobLightDTO job = new JobLightDTO();
            job.JobId = Convert.ToInt64(jobId);
            job.BeginDate = DateTime.Today.AddDays(0).ToUniversalTime();
            job.EndDate = DateTime.Today.AddDays(30).ToUniversalTime();
            job.ActualCost = (decimal)100000.00;
            job.ActualJobDurationDays = (decimal)20.5;
            job.TotalCost = (decimal)150000.00;
            job.JobRemarks = "updated remarks";
            job.JobOrigin = "TestJobOrigin ";
            job.AssemblyId = well.AssemblyId;
            job.WellId = well.Id;
            //For Below fields User can select any value in the dropdown menu
            job.AFEId = JobAndEventService.GetAFEs().FirstOrDefault().Id;
            job.StatusId = JobAndEventService.GetJobStatuses().FirstOrDefault().Id;
            job.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault().id;
            job.BusinessOrganizationId = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            job.AccountRef = "1";
            //JobReasonId drop down selection is based on the JobTypeId
            job.JobReasonId = JobAndEventService.GetJobReasonsForJobType(job.JobTypeId.ToString()).FirstOrDefault().Id;
            job.JobDriverId = (int)JobAndEventService.GetPrimaryMotivationsForJob().FirstOrDefault().Id;
            JobLightDTO JobbeforeUpdate = JobAndEventService.GetJobById(jobId.ToString());
            JobAndEventService.UpdateJob(job);
            JobLightDTO JobafterUpdate = JobAndEventService.GetJobById(jobId.ToString());
            Assert.AreEqual(job.JobRemarks, JobafterUpdate.JobRemarks, "Job not updated");
            Assert.AreEqual(JobbeforeUpdate.WellId, JobafterUpdate.WellId);
            Assert.AreEqual(JobbeforeUpdate.WellName, JobafterUpdate.WellName);
            Assert.AreEqual(JobbeforeUpdate.BeginDate, JobafterUpdate.BeginDate);
            Assert.AreEqual(JobbeforeUpdate.EndDate, JobafterUpdate.EndDate);
            Assert.AreEqual(JobbeforeUpdate.ActualCost, JobafterUpdate.ActualCost);
            Assert.AreEqual(JobbeforeUpdate.ActualJobDurationDays, JobafterUpdate.ActualJobDurationDays);
            Assert.AreEqual(JobbeforeUpdate.TotalCost, JobafterUpdate.TotalCost);
            Assert.AreNotEqual(JobbeforeUpdate.JobRemarks, JobafterUpdate.JobRemarks);
            Assert.AreEqual(JobbeforeUpdate.JobOrigin, JobafterUpdate.JobOrigin);
            Assert.AreEqual(JobbeforeUpdate.AssemblyId, JobafterUpdate.AssemblyId);
            Assert.AreEqual(JobbeforeUpdate.AFEId, JobafterUpdate.AFEId);
            //Assert.AreEqual(JobbeforeUpdate.StatusId, JobafterUpdate.StatusId);
            Assert.AreEqual(JobbeforeUpdate.JobTypeId, JobafterUpdate.JobTypeId);
            Assert.AreEqual(JobbeforeUpdate.BusinessOrganizationId, JobafterUpdate.BusinessOrganizationId);
            Assert.AreEqual(JobbeforeUpdate.AccountRef, JobafterUpdate.AccountRef);
            Assert.AreEqual(JobbeforeUpdate.JobReasonId, JobafterUpdate.JobReasonId);
            Assert.AreEqual(JobbeforeUpdate.JobDriverId, JobafterUpdate.JobDriverId);

            //Below code will delete added template(s) in earlier steps

            bool var1 = JobAndEventService.RemoveTemplateJob(templatedto[0].TemplateJobId.ToString());

            Trace.WriteLine("Value returned is" + var1);
            Assert.AreEqual("true", var1.ToString().ToLower());

            JobPlanWizardTemplateJobDTO[] templatejobs = JobAndEventService.GetTemplateJobsForJobPlanWizard();

            Assert.IsTrue(templatejobs.Count() == 0, "Not deleted Template Jobs");
        }

        public object DataValue(MetaDataDTO metaData)
        {
            object dataValue = null;

            int length = metaData.Length.HasValue ? metaData.Length.Value : 1;
            int precision = metaData.Precision.HasValue ? metaData.Precision.Value : 0;
            switch (metaData.DataDisplayType)
            {
                case "TEXTBOX":
                    dataValue = RandomString(length);
                    break;

                case "NUMBER":
                    dataValue = RandomNumber(length - 1 - precision);
                    break;

                case "DDL":
                    {
                        ControlIdTextDTO[] ctrlIds = GetMetadataReferenceDataDDL(metaData);
                        dataValue = ctrlIds.FirstOrDefault().ControlId;
                    }
                    break;
            }

            return dataValue;
        }

        private List<string> entityNamesToTest = new List<string> { "r_EventType" , "r_JobType", "r_JobReason" , "r_JobStatus" , "r_CatalogItem" ,
        "r_TruckUnit", "BusinessOrganization", "r_TruckUnitType"};

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void ForeSiteMobileApps()
        {
            string date = DateTime.UtcNow.AddDays(1).ToString("MM-dd-yyyy");
            EventTypeDTO[] bgetEventTypesbyDate = JobAndEventService.GetEventTypesSinceDate(date);
            JobReasonDTO[] bgetJobreasonsbyDate = JobAndEventService.GetJobReasonsSinceDate(date);
            JobTypeDTO[] bgetJobtypebyDate = JobAndEventService.GetJobTypesSinceDate(date);
            JobStatusDTO[] bgetJobstatusbyDate = JobAndEventService.GetJobStatusesSinceDate(date);
            JobTypeEventTypeDTO[] bgetJobtypeEventtypebyDate = JobAndEventService.GetJobTypeEventTypeSinceDate(date);
            CatalogItemDTO[] bgetCatalogItemsbyDate = JobAndEventService.GetCatalogItemsSinceDate(date);
            TruckUnitDTO[] bgetTruckUnitsbyDate = JobAndEventService.GetTruckUnitsSinceDate(date);
            BusinessOrganizationDTO[] bgetBusinessOrganizationbyDate = JobAndEventService.GetBusinessOrganizationSinceDate(date);
            TruckUnitTypeDTO[] bgetTrukUnitTypesbyDate = JobAndEventService.GetTruckUnitTypesSinceDate(date);

            ReferenceDataMaintenanceEntityDTO[] allReferenceTables = DBEntityService.GetReferenceDataMaintenanceEntities();
            Assert.IsTrue(allReferenceTables.Length > 0, "No reference tables are available inside the database");
            EntityGridSettingDTO setting = new EntityGridSettingDTO();
            setting.GridSetting = new GridSettingDTO { PageSize = 20, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
            foreach (ReferenceDataMaintenanceEntityDTO ReferenceTable in allReferenceTables)
            {
                setting.EntityName = ReferenceTable.EntityName;
                MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(ReferenceTable.EntityName);
                if (ReferenceTable.EntityName.Equals("r_JobTypeEventType"))
                    continue;
                if (entityNamesToTest.Contains(ReferenceTable.EntityName))
                {
                    foreach (MetaDataDTO addMetaData in addMetaDatas)
                    {
                        //User Input
                        if (addMetaData.Visible && addMetaData.Editable)
                        {
                            addMetaData.DataValue = DataValue(addMetaData);
                            if (addMetaData.DataDisplayType.Contains("RADIO"))
                                addMetaData.DataValue = 0;
                        }
                    }
                    DBEntityService.AddReferenceData(addMetaDatas);
                }
            }
            MetaDataDTO[] addMetaDatasJE = DBEntityService.GetRefereneceMetaDataEntityForAdd("r_JobTypeEventType");
            foreach (MetaDataDTO addMetaData in addMetaDatasJE)
            {
                //User Input
                if (addMetaData.Visible && addMetaData.Editable)
                {
                    if (addMetaData.DataDisplayType.Contains("RADIO"))
                        addMetaData.DataValue = 0;
                }
            }
            ControlIdTextDTO eventType = GetMetadataReferenceDataDDL(addMetaDatasJE.FirstOrDefault(x => x.ColumnName == "jteFK_r_EventType")).OrderByDescending(x => x.ControlId).FirstOrDefault();
            ControlIdTextDTO JobType = GetMetadataReferenceDataDDL(addMetaDatasJE.FirstOrDefault(x => x.ColumnName == "jteFK_r_JobType")).OrderByDescending(x => x.ControlId).FirstOrDefault();
            addMetaDatasJE.FirstOrDefault(x => x.ColumnName == "jteFK_r_EventType").DataValue = eventType.ControlId;
            addMetaDatasJE.FirstOrDefault(x => x.ColumnName == "jteFK_r_JobType").DataValue = JobType.ControlId;
            DBEntityService.AddReferenceData(addMetaDatasJE);
            date = DateTime.UtcNow.AddDays(-1).ToString("MM-dd-yyyy");
            EventTypeDTO[] getEventTypesbyDate = JobAndEventService.GetEventTypesSinceDate(date);
            Assert.IsTrue(getEventTypesbyDate.Count() > bgetEventTypesbyDate.Count());
            JobReasonDTO[] getJobreasonsbyDate = JobAndEventService.GetJobReasonsSinceDate(date);
            Assert.IsTrue(getJobreasonsbyDate.Count() > bgetJobreasonsbyDate.Count());
            JobTypeDTO[] getJobtypebyDate = JobAndEventService.GetJobTypesSinceDate(date);
            Assert.IsTrue(getJobtypebyDate.Count() > bgetJobtypebyDate.Count());
            JobStatusDTO[] getJobstatusbyDate = JobAndEventService.GetJobStatusesSinceDate(date);
            Assert.IsTrue(getJobstatusbyDate.Count() > bgetJobstatusbyDate.Count());
            JobTypeEventTypeDTO[] getJobtypeEventtypebyDate = JobAndEventService.GetJobTypeEventTypeSinceDate(date);
            Assert.IsTrue(getJobtypeEventtypebyDate.Count() > bgetJobtypeEventtypebyDate.Count());
            CatalogItemDTO[] getCatalogItemsbyDate = JobAndEventService.GetCatalogItemsSinceDate(date);
            Assert.IsTrue(getCatalogItemsbyDate.Count() > bgetCatalogItemsbyDate.Count());
            TruckUnitDTO[] getTruckUnitsbyDate = JobAndEventService.GetTruckUnitsSinceDate(date);
            Assert.IsTrue(getTruckUnitsbyDate.Count() > bgetTruckUnitsbyDate.Count());
            BusinessOrganizationDTO[] getBusinessOrganizationbyDate = JobAndEventService.GetBusinessOrganizationSinceDate(date);
            Assert.IsTrue(getBusinessOrganizationbyDate.Count() > bgetBusinessOrganizationbyDate.Count());
            TruckUnitTypeDTO[] getTrukUnitTypesbyDate = JobAndEventService.GetTruckUnitTypesSinceDate(date);
            Assert.IsTrue(getTrukUnitTypesbyDate.Count() > bgetTrukUnitTypesbyDate.Count());

            //Verifying if date is passed as "0" then all requested items are being returned
            bgetJobtypebyDate = JobAndEventService.GetJobTypesSinceDate("0");
            Assert.AreEqual(getJobtypebyDate.Count(), bgetJobtypebyDate.Count(), "Job Types count is not matching");
            bgetJobstatusbyDate = JobAndEventService.GetJobStatusesSinceDate("0");
            Assert.AreEqual(getJobstatusbyDate.Count(), bgetJobstatusbyDate.Count(), "Job Statuses count is not matching");
            bgetEventTypesbyDate = JobAndEventService.GetEventTypesSinceDate("0");
            Assert.AreEqual(getEventTypesbyDate.Count(), bgetEventTypesbyDate.Count(), "Event Types count is not matching");
            bgetJobreasonsbyDate = JobAndEventService.GetJobReasonsSinceDate("0");
            Assert.AreEqual(getJobreasonsbyDate.Count(), bgetJobreasonsbyDate.Count(), "Job Reason count is not matching");
            bgetJobtypeEventtypebyDate = JobAndEventService.GetJobTypeEventTypeSinceDate("0");
            Assert.AreEqual(getJobtypeEventtypebyDate.Count(), bgetJobtypeEventtypebyDate.Count(), "JobTypeEventType count is not matching");
            bgetCatalogItemsbyDate = JobAndEventService.GetCatalogItemsSinceDate("0");
            Assert.AreEqual(getCatalogItemsbyDate.Count(), bgetCatalogItemsbyDate.Count(), "CatalogItem count is not matching");
            bgetTruckUnitsbyDate = JobAndEventService.GetTruckUnitsSinceDate("0");
            Assert.AreEqual(getTruckUnitsbyDate.Count(), bgetTruckUnitsbyDate.Count(), "TruckUnit count is not matching");
            bgetBusinessOrganizationbyDate = JobAndEventService.GetBusinessOrganizationSinceDate("0");
            Assert.AreEqual(getBusinessOrganizationbyDate.Count(), bgetBusinessOrganizationbyDate.Count(), "BusinessOrganization count is not matching");
            bgetTrukUnitTypesbyDate = JobAndEventService.GetTruckUnitTypesSinceDate(date);
            Assert.AreEqual(getTrukUnitTypesbyDate.Count(), bgetTrukUnitTypesbyDate.Count(), "Truck unit types count is not matching");
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetUpdatedWellsSinceDateTest()
        {
            //Verifying that if no wells found then empty array should be returned
            string date = DateTime.UtcNow.AddDays(-1).ToString("MM-dd-yyyy");
            WellDTO[] bgetWellsbyDate = JobAndEventService.GetUpdatedWellsSinceDate(date);
            Assert.AreEqual(0, bgetWellsbyDate.Count(), "Empty array is not returning when there is no Well present in database");

            //Adding 2 wells
            var toAdd = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", IntervalAPI = "IntervalAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = toAdd });
            toAdd = SetDefaultFluidType(new WellDTO() { Name = "CASETest1", FacilityId = "CASETEST1", SubAssemblyAPI = "SubAssemblyAPI1", AssemblyAPI = "AssemblyAPI1", IntervalAPI = "IntervalAPI1", CommissionDate = DateTime.Today, WellType = WellTypeId.ESP });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = toAdd });
            WellDTO[] allwells = WellService.GetAllWells();
            foreach (WellDTO eachwell in allwells)
            {
                _wellsToRemove.Add(eachwell);
            }

            date = DateTime.UtcNow.AddDays(-1).ToString("MM-dd-yyyy");
            WellDTO[] getWellsbyDate = JobAndEventService.GetUpdatedWellsSinceDate(date);
            Assert.IsTrue(getWellsbyDate.Count() > bgetWellsbyDate.Count());

            //Verifying if date is passed as "0" then all wells are being returned
            int wellcount = allwells.Count();
            getWellsbyDate = JobAndEventService.GetUpdatedWellsSinceDate("0");
            Assert.AreEqual(wellcount, getWellsbyDate.Count(), "Well Count is not matching");
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void AddUpdateEventbyDTOTest()
        {
            AddWell("RPOC_");
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);

            //Creating new EventDTO
            EventDTO evt = new EventDTO()
            {
                AFE = "AFE",
                BeginTime = getJob.BeginDate.AddDays(-2).ToUniversalTime(),
                BusinessOrganization = "Organization",
                CatalogItem = "Catalog Item",
                ChangeDate = DateTime.Now.ToUniversalTime(),
                ChangeUser = "User",
                DocumentFileName = null,
                Duration = (decimal)3.0,
                EndTime = getJob.EndDate.AddDays(2).ToUniversalTime(),
                EventType = "Event Type",
                EventTypeId = JobAndEventService.GetAllEventTypesForJobType(getJob.JobTypeId.ToString()).FirstOrDefault().EventTypeId,
                FieldServiceOrderID = "Order ID",
                HistoricalRate = (decimal)4.0,
                Order = 1,
                PK_AFE = JobAndEventService.GetAFEs().FirstOrDefault().Id,
                PK_Assembly = getJob.AssemblyId,
                PK_BusinessOrganization = JobAndEventService.GetCatalogItemGroupData().Vendors[0].Id,
                PK_CatalogItem = JobAndEventService.GetCatalogItemGroupData().CatalogItems[0].CatalogItemId,
                PK_Job = getJob.JobId,
                PK_TruckUnit = JobAndEventService.GetCatalogItemGroupData().TruckUnits[0].TruckUnitId,
                PersonPerformingTask = "Person",
                PreventiveMaintenance = false,
                Quantity = (decimal)6.0,
                Remarks = "Remark",
                ResponsiblePerson = "Person",
                TotalCost = (decimal)120,
                Trouble = false,
                UnPlanned = false,
                WorkorderID = "Order ID"
            };

            // Adding new event by calling AddEventByDTO method
            string result = JobAndEventService.AddEventByDTO(evt);

            Assert.IsNotNull(result, "Event is not Added successfully");
            EventGroupDTO[] eventGroups = JobAndEventService.GetEvents(jobId);
            //Finding total number of events and saving it in eventCounter variable
            int eventCounter = 0;
            foreach (EventGroupDTO eg in eventGroups)
            {
                foreach (EventDTO eachEvent in eg.EventData)
                {
                    Trace.WriteLine("Event id : " + eachEvent.Id);
                    eventCounter = eventCounter + 1;
                }
            }
            Assert.AreEqual(1, eventCounter, "Event is not added successfully");
            getJob = JobAndEventService.GetJobById(jobId);
            //creating new EventDTO object which will have same job id ,event id and event type id. changing value of other fields.
            evt.Id = eventGroups[0].EventData[0].Id;
            evt.BeginTime = getJob.BeginDate.AddDays(-3).ToUniversalTime();
            evt.EndTime = getJob.EndDate.AddDays(3).ToUniversalTime();
            evt.PK_AFE = JobAndEventService.GetAFEs().FirstOrDefault().Id;
            evt.PK_BusinessOrganization = JobAndEventService.GetCatalogItemGroupData().Vendors[1].Id;
            evt.PK_TruckUnit = JobAndEventService.GetCatalogItemGroupData().TruckUnits[0].TruckUnitId;
            evt.PK_CatalogItem = JobAndEventService.GetCatalogItemGroupData().CatalogItems[0].CatalogItemId;
            evt.EventTypeId = JobAndEventService.GetAllEventTypesForJobType(getJob.JobTypeId.ToString()).FirstOrDefault().EventTypeId;
            evt.PK_Job = getJob.JobId;
            evt.PK_Assembly = getJob.AssemblyId;
            evt.TotalCost = 150;

            Assert.IsTrue(JobAndEventService.UpdateEventByDTO(evt), "Event is not updated successfully");
            EventGroupDTO[] eventGroups1 = JobAndEventService.GetEvents(jobId);
            Assert.AreEqual(eventGroups[0].EventData[0].Id, eventGroups1[0].EventData[0].Id, "Updates are done in incorrect event Id");
            Assert.AreEqual(getJob.JobId, eventGroups1[0].EventData[0].PK_Job, "Updates are done in incorrect Job Id");
            Assert.AreEqual(getJob.BeginDate.AddDays(-3).ToUniversalTime().ToString(), eventGroups1[0].EventData[0].BeginTime.ToString(), "Begin time is not updated correctly");
            Assert.AreEqual(getJob.EndDate.AddDays(3).ToUniversalTime().ToString(), eventGroups1[0].EventData[0].EndTime.ToString(), "End time is not updated correctly");
            Assert.AreEqual(JobAndEventService.GetCatalogItemGroupData().Vendors[1].Id, eventGroups1[0].EventData[0].PK_BusinessOrganization, "Business organization is not updated correctly");
            Assert.AreEqual(150, eventGroups1[0].EventData[0].TotalCost, "Total Cost is not updated correctly");

            //Assertion for Job & Event Begin/End time validation
            getJob = JobAndEventService.GetJobById(jobId);
            Assert.AreEqual(eventGroups1[0].EventData[0].BeginTime.ToString(), getJob.BeginDate.ToString(), "Job and Event Begin day is not same when Event Begin date is lesser than Job end date");
            Assert.AreEqual(eventGroups1[0].EventData[0].EndTime.ToString(), getJob.EndDate.ToString(), "Job and Event End day is not same when Event end date is Greater than Job end date");
        }

        //This method will retrive selection untis for customize pull ticket report
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetChooseUnit()
        {
            WellPlanReportUnitDTO[] wellPlanReport = JobAndEventService.GetUnitsForWellPlanReport();

            foreach (WellPlanReportUnitDTO reportUnit in wellPlanReport)
            {
                switch (reportUnit.Id)
                {
                    case 1:
                        {
                            Assert.AreEqual("PERF DETAILS", reportUnit.Unit.ToUpper(), "Incorrect Unit Value Displayed For Id : 1");
                        }
                        break;

                    case 2:
                        {
                            Assert.AreEqual("PUMPING UNIT", reportUnit.Unit.ToUpper(), "Incorrect Unit Value Displayed For Id : 2");
                        }
                        break;

                    case 3:
                        {
                            Assert.AreEqual("PLUG BACK", reportUnit.Unit.ToUpper(), "Incorrect Unit Value Displayed For Id : 3");
                        }
                        break;

                    case 4:
                        {
                            Assert.AreEqual("MOP, STATUS, TYPE", reportUnit.Unit.ToUpper(), "Incorrect Unit Value Displayed For Id : 4");
                        }
                        break;

                    case 5:
                        {
                            Assert.AreEqual("FILL CLEAN", reportUnit.Unit.ToUpper(), "Incorrect Unit Value Displayed For Id : 5");
                        }
                        break;

                    case 6:
                        {
                            Assert.AreEqual("WELLBORE DIAGRAM", reportUnit.Unit.ToUpper(), "Incorrect Unit Value Displayed For Id : 6");
                        }
                        break;

                    case 7:
                        {
                            Assert.AreEqual("JOBS HISTORY", reportUnit.Unit.ToUpper(), "Incorrect Unit Value Displayed For Id : 7");
                        }
                        break;

                    case 8:
                        {
                            Assert.AreEqual("FLUID LEVEL ", reportUnit.Unit.ToUpper(), "Incorrect Unit Value Displayed For Id : 8");
                        }
                        break;

                    case 9:
                        {
                            Assert.AreEqual("PUMP TAG", reportUnit.Unit.ToUpper(), "Incorrect Unit Value Displayed For Id : 9");
                        }
                        break;
                }
            }
        }

        public string GetPrimaryKeyOfLatestRecord(EntityGridSettingDTO setting)
        {
            int primaryKey = 0;
            int tableDataCount = DBEntityService.GetTableDataCount(setting);
            setting.GridSetting = new GridSettingDTO { PageSize = tableDataCount, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
            DBEntityDTO tableData = DBEntityService.GetTableData(setting);

            int index1 = 0;
            for (index1 = 0; index1 < tableData.Attributes.Length; index1++)
            {
                if (tableData.Attributes[index1].AttributeName.Contains("PrimaryKey"))
                {
                    break;
                }
            }

            int index = 0;
            primaryKey = (Int32)tableData.DataValues[0][index1];
            for (index = 0; index < tableDataCount - 1; index++)
            {
                if (((Int32)tableData.DataValues[index + 1][index1] > (Int32)tableData.DataValues[index][index1]) && ((Int32)tableData.DataValues[index + 1][index1] > primaryKey))
                    primaryKey = (Int32)tableData.DataValues[index + 1][index1];
            }
            return primaryKey.ToString();
        }

        public string AddReferenceData(string tableName, bool refuserDel = true)
        {
            string pKey = "";
            ReferenceDataMaintenanceEntityDTO refTable = DBEntityService.GetReferenceDataMaintenanceEntities().FirstOrDefault(x => x.EntityName == tableName);
            Assert.IsNotNull(refTable, "Failed to get " + tableName);
            EntityGridSettingDTO setting = new EntityGridSettingDTO();
            setting.GridSetting = new GridSettingDTO { PageSize = 20, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
            setting.EntityName = refTable.EntityName;
            MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(refTable.EntityName);
            if (refTable.EntityName.Equals(tableName))
            {
                foreach (MetaDataDTO addMetaData in addMetaDatas)
                {
                    //User Input
                    if (addMetaData.Visible && addMetaData.Editable)
                    {
                        addMetaData.DataValue = DataValue(addMetaData);
                        if (addMetaData.DataDisplayType.Contains("RADIO"))
                        {
                            if (refuserDel == false)
                                addMetaData.DataValue = 0;
                            else
                                addMetaData.DataValue = 1;
                        }
                    }
                    if (tableName == "AFE")
                    {
                        if (addMetaData.DataDisplayType.Contains("DATE") && addMetaData.Required)
                        {
                            addMetaData.DataValue = DateTime.UtcNow.ToISO8601();
                        }
                    }
                }
                Trace.WriteLine("Record added in table :- " + tableName);
                DBEntityService.AddReferenceData(addMetaDatas);
                pKey = GetPrimaryKeyOfLatestRecord(setting);
            }
            return pKey;
        }

        public string AddJobReason(string jobTypeId)
        {
            string pKey = "";
            long jTypeId = 0;
            long.TryParse(jobTypeId, out jTypeId);

            ReferenceDataMaintenanceEntityDTO refTable = DBEntityService.GetReferenceDataMaintenanceEntities().FirstOrDefault(x => x.EntityName == "r_JobReason");
            Assert.IsNotNull(refTable, "Failed to get " + "r_JobReason");
            EntityGridSettingDTO setting = new EntityGridSettingDTO();
            setting.GridSetting = new GridSettingDTO { PageSize = 20, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
            setting.EntityName = refTable.EntityName;
            MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(refTable.EntityName);

            foreach (MetaDataDTO addMetaData in addMetaDatas)
            {
                //User Input
                if (addMetaData.Visible && addMetaData.Editable)
                {
                    if (addMetaData.DataDisplayType.Contains("RADIO"))
                        addMetaData.DataValue = 1;
                }
            }
            addMetaDatas.FirstOrDefault(x => x.ColumnName == "rspFK_r_JobType").DataValue = jTypeId;
            setting.EntityName = "r_JobType";
            int tableDataCount = DBEntityService.GetTableDataCount(setting);
            setting.GridSetting = new GridSettingDTO { PageSize = tableDataCount, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
            DBEntityDTO tableData = DBEntityService.GetTableData(setting);

            int index = 0;
            for (index = 0; index < tableData.Attributes.Length; index++)
            {
                if (tableData.Attributes[index].AttributeName.Contains("PrimaryKey"))
                {
                    break;
                }
            }
            int index2 = 0;
            for (index2 = 0; index2 < tableData.Attributes.Length; index2++)
            {
                if (tableData.Attributes[index2].AttributeName.ToLower().Contains("jobtype"))
                {
                    break;
                }
            }

            int index3 = 0;
            for (index3 = 0; index3 < tableDataCount; index3++)
            {
                if (tableData.DataValues[index3][index].ToString().Equals(jobTypeId))
                {
                    break;
                }
            }

            string jobType = tableData.DataValues[index3][index2].ToString();

            addMetaDatas.FirstOrDefault(x => x.ColumnName == "rspFK_r_JobType").DataValueText = jobType;
            addMetaDatas.FirstOrDefault(x => x.ColumnName == "rspJobReason").DataValue = "JobReason" + DateTime.Now;
            DBEntityService.AddReferenceData(addMetaDatas);
            setting.EntityName = "r_JobReason";

            pKey = GetPrimaryKeyOfLatestRecord(setting);
            return pKey;
        }

        /// <summary>
        /// Added verification for new API GetTruckUnitsForBusinessOrganzationForUpdate on 04/26/2019 by Rahul Pingale (FRWM-4548)
        /// </summary>
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void RefUserDeletedDDL()
        {
            //Verifying GetJobTypesForUpdate
            string jobTypeId = AddReferenceData("r_JobType");
            Assert.IsNotNull(jobTypeId, "New Job Type is not added successfully");
            Assert.AreEqual(JobAndEventService.GetJobTypes().Count() + 1, JobAndEventService.GetJobTypesForUpdate(jobTypeId).Count(), "Failed to get refUserDeleted item for job type update dropdown");

            //Verifying GetJobReasonsForJobTypeForUpdate
            string jobTypeId1 = AddReferenceData("r_JobType", false);
            string jobReasonId = AddJobReason(jobTypeId1);
            Assert.IsNotNull(jobReasonId, "New Job Reason is not added successfully");
            Assert.AreEqual(JobAndEventService.GetJobReasonsForJobType(jobTypeId1).Count() + 1, JobAndEventService.GetJobReasonsForJobTypeForUpdate(jobTypeId1, jobReasonId).Count(), "Failed to get refUserDeleted item for job reason update dropdown");

            //Verifying GetJobStatusesForUpdate
            string jobStatusId = AddReferenceData("r_JobStatus");
            Assert.IsNotNull(jobStatusId, "New Job Status is not added successfully");
            Assert.AreEqual(JobAndEventService.GetJobStatuses().Count() + 1, JobAndEventService.GetJobStatusesForUpdate(jobStatusId).Count(), "Failed to get refUserDeleted item for job status update dropdown");

            //Verifying GetPrimaryMotivationsForJobForUpdate
            string jobDriverId = AddReferenceData("r_PrimaryMotivationForJob");
            Assert.IsNotNull(jobDriverId, "New Job Driver is not added successfully");
            Assert.AreEqual(JobAndEventService.GetPrimaryMotivationsForJob().Count() + 1, JobAndEventService.GetPrimaryMotivationsForJobForUpdate(jobDriverId).Count(), "Failed to get refUserDeleted item for job driver update dropdown");

            //Verifying GetAFEsForUpdate
            string afeId = AddReferenceData("AFE");
            Assert.IsNotNull(afeId, "New AFE is not added successfully");
            Assert.AreEqual(JobAndEventService.GetAFEs().Count() + 1, JobAndEventService.GetAFEsForUpdate(afeId).Count(), "Failed to get refUserDeleted item for AFE id update dropdown");

            //Verifying GetCatalogItemGroupDataForUpdate
            string businessOrganizationId = AddReferenceData("BusinessOrganization");
            Assert.IsNotNull(businessOrganizationId, "New Business organization is not added successfully");
            Assert.AreEqual(JobAndEventService.GetCatalogItemGroupData().Vendors.Count + 1, JobAndEventService.GetCatalogItemGroupDataForUpdate(businessOrganizationId).Vendors.Count, "Failed to get refUserDeleted item for Business organization update dropdown");

            //Verifying GetBusinessOrganizationForUpdate
            string businessOrganizationIdNew = AddReferenceData("BusinessOrganization");
            Assert.IsNotNull(businessOrganizationIdNew, "New Business organization is not added successfully");
            Assert.AreEqual(JobAndEventService.GetBusinessOrganization().Count() + 1, JobAndEventService.GetBusinessOrganizationForUpdate(businessOrganizationIdNew).Count(), "Failed to get refUserDeleted item for Business organization update dropdown");

            //Verifying GetTruckUnitsForBusinessOrganzationForUpdate
            //Adding business organization with RefUserDelete flag No
            string businessOrganizationId1 = AddReferenceData("BusinessOrganization", false);
            //Adding truckUnit Id with RefUserDelete flag No
            AddTruckUnit(businessOrganizationId1, false);
            //Adding truckUnit Id with RefUserdelete flag Yes
            string truckUnitId = AddTruckUnit(businessOrganizationId1);
            Assert.IsNotNull(truckUnitId, "New Truck Unit is not added successfully");
            //Following block of Code is written to get TruckUnitIds mapped against BusinessOrganizationId1 and refUserDelete flag set to No
            MetaDataReferenceData truckUnitData = new MetaDataReferenceData();
            truckUnitData.MetaData = DBEntityService.GetRefereneceMetaDataEntityForAdd("Event").FirstOrDefault(x => x.ColumnName == "evcFK_r_TruckUnit");
            MetaDataFilterDTO filterValue = new MetaDataFilterDTO() { ColumnValue = businessOrganizationId1 };
            MetaDataFilterDTO[] filterArray = new MetaDataFilterDTO[] { filterValue };
            truckUnitData.UIFilterValues = filterArray;
            ControlIdTextDTO[] truckUnitIds = JobAndEventService.GetMetaDataReferenceData(truckUnitData);
            //Verifying result of GetTruckUnitsForBusinessOrganzationForUpdate API
            Assert.AreEqual(truckUnitIds.Count() + 1, JobAndEventService.GetTruckUnitsForBusinessOrganzationForUpdate(businessOrganizationId1, truckUnitId).Count(), "Failed to get refUserDeleted item for TruckUnit update dropdown");
        }

        //******************************  this Block is moved to Generic methods calss TestData
        //public DBEntityDTO GetTableData(string tableName)
        //{
        //    EntityGridSettingDTO setting = new EntityGridSettingDTO();
        //    setting.GridSetting = new GridSettingDTO { PageSize = 20, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
        //    setting.EntityName = tableName;
        //    int tableDataCount = DBEntityService.GetTableDataCount(setting);
        //    setting.GridSetting = new GridSettingDTO { PageSize = tableDataCount, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
        //    DBEntityDTO tableData = DBEntityService.GetTableData(setting);
        //    return tableData;
        //}

        //public int GetIndexOfAttribute(DBEntityDTO table, string attributeName)
        //{
        //    int index = 0;
        //    for (index = 0; index < table.Attributes.Length; index++)
        //    {
        //        if (table.Attributes[index].AttributeName.Contains(attributeName))
        //            break;

        //    }
        //    return index;
        //}
        //******************************  this Block is moved to Generic methods calss TestData
        public string AddTruckUnit(string businessOrganizationId, bool refUserDelete = true)
        {
            string pKey = "";
            long bOrgId = 0;
            long.TryParse(businessOrganizationId, out bOrgId);

            ReferenceDataMaintenanceEntityDTO refTable = DBEntityService.GetReferenceDataMaintenanceEntities().FirstOrDefault(x => x.EntityName == "r_TruckUnit");
            Assert.IsNotNull(refTable, "Failed to get " + "r_TruckUnit");
            EntityGridSettingDTO setting = new EntityGridSettingDTO();
            setting.GridSetting = new GridSettingDTO { PageSize = 20, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
            setting.EntityName = refTable.EntityName;
            MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(refTable.EntityName);

            if (refUserDelete)
            {
                addMetaDatas.FirstOrDefault(x => x.ColumnName == "tumRefUserDeleted").DataValue = 1;
            }
            else
            {
                addMetaDatas.FirstOrDefault(x => x.ColumnName == "tumRefUserDeleted").DataValue = 0;
            }
            addMetaDatas.FirstOrDefault(x => x.ColumnName == "tumInactive").DataValue = 0;
            addMetaDatas.FirstOrDefault(x => x.ColumnName == "tumFK_BusinessOrganization").DataValue = bOrgId;
            DBEntityDTO businessOrganization_TableData = GetTableData("BusinessOrganization");

            int index = GetIndexOfAttribute(businessOrganization_TableData, "PrimaryKey");

            int index2 = GetIndexOfAttribute(businessOrganization_TableData, "venBusinessOrganizationName");

            int index3 = 0;
            for (index3 = 0; index3 < businessOrganization_TableData.DataValues.Count(); index3++)
            {
                if (businessOrganization_TableData.DataValues[index3][index].ToString().Equals(businessOrganizationId))
                {
                    break;
                }
            }

            string businessOrganization = businessOrganization_TableData.DataValues[index3][index2].ToString();

            //Getting index of PrimaryKey & TruckUnitTypeDesc
            DBEntityDTO truckUnitType_TableData = GetTableData("r_TruckUnitType");
            int truckUnitType_PrimaryKey = GetIndexOfAttribute(truckUnitType_TableData, "tutPrimaryKey");
            int truckUnitType_Description = GetIndexOfAttribute(truckUnitType_TableData, "tutTruckUnitType");

            //Getting index of PrimaryKey & truckUnitCapability_Description
            DBEntityDTO truckUnitCapability_TableData = GetTableData("r_TruckUnitCapability");
            int truckUnitCapability_PrimaryKey = GetIndexOfAttribute(truckUnitCapability_TableData, "tucPrimaryKey");
            int truckUnitCapability_Description = GetIndexOfAttribute(truckUnitCapability_TableData, "tucTruckUnitCapability");

            addMetaDatas.FirstOrDefault(x => x.ColumnName == "tumFK_BusinessOrganization").DataValueText = businessOrganization;
            addMetaDatas.FirstOrDefault(x => x.ColumnName == "tumTruckUnit").DataValue = DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + DateTime.Now.Millisecond;
            addMetaDatas.FirstOrDefault(x => x.ColumnName == "tumFK_r_TruckUnitType").DataValue = truckUnitType_TableData.DataValues.FirstOrDefault()[truckUnitType_PrimaryKey];
            addMetaDatas.FirstOrDefault(x => x.ColumnName == "tumFK_r_TruckUnitType").DataValueText = truckUnitType_TableData.DataValues.FirstOrDefault()[truckUnitType_Description].ToString();
            addMetaDatas.FirstOrDefault(x => x.ColumnName == "tumFK_r_TruckUnitCapability").DataValue = truckUnitCapability_TableData.DataValues.FirstOrDefault()[truckUnitCapability_PrimaryKey];
            addMetaDatas.FirstOrDefault(x => x.ColumnName == "tumFK_r_TruckUnitCapability").DataValueText = truckUnitCapability_TableData.DataValues.FirstOrDefault()[truckUnitCapability_Description].ToString();

            DBEntityService.AddReferenceData(addMetaDatas);

            setting.EntityName = "r_TruckUnit";
            pKey = GetPrimaryKeyOfLatestRecord(setting);
            return pKey;
        }

        public void AddComponent(string wellId, string assemblyId, string subassemblyId, string jobId, string CompGp, string Partype, string CompName, string CompRemarks, int wellBorePerfStatus = 7)
        {
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.WellBoreReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Wellbore Report");

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

            string colNames = "cmcCompName;ascRemark";
            string colVals = CompName + ";" + CompRemarks;
            ComponentMetaDataGroupDTO[] arrComponent = null;

            if (Partype.Equals("Wellbore Completion Detail (Perforations, etc.)"))
            {
                arrComponent = AddComponentGeneric(jobId, wellId, assemblyId, subassemblyId, CompGp, Partype, colNames, colVals, check, wellBorePerfStatus);
            }
            else if (Partype.Equals("Perforation Hole/Slot Detail"))
            {
                arrComponent = AddComponentGeneric(jobId, wellId, assemblyId, subassemblyId, CompGp, Partype, colNames, colVals, check, wellBorePerfStatus);
            }
            else
            {
                arrComponent = AddComponentGeneric(jobId, wellId, assemblyId, subassemblyId, CompGp, Partype, colNames, colVals, check);
            }
            ComponentPartTypeDTO details = new ComponentPartTypeDTO();
            details.JobId = Convert.ToInt64(jobId);
            details.ComponentId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == CompGp).ptgFK_c_MfgCat_ComponentGrouping;
            details.PartTypeId = partTypes.FirstOrDefault(x => x.ptyPartType == Partype).ptgFK_c_MfgCat_PartType;

            ComponentMetaDataGroupBatchCollectionDTO batchDetailsComp = new ComponentMetaDataGroupBatchCollectionDTO();
            batchDetailsComp.ActionPerformed = CRUDOperationTypes.Add;
            batchDetailsComp.ComponentMetadataCollection = arrComponent;
            batchDetailsComp.ComponentPartType = details;

            List<ComponentMetaDataGroupBatchCollectionDTO> listComp = new List<ComponentMetaDataGroupBatchCollectionDTO>();
            listComp.Add(batchDetailsComp);
            ComponentMetaDataGroupBatchCollectionDTO[] arrComp = listComp.ToArray();

            bool saveBatch = ComponentService.SaveWellboreComponent(arrComp);
            Assert.IsTrue(saveBatch);
            Trace.WriteLine("Component Added with Component group :" + CompGp + " and Part type " + Partype);
            Trace.WriteLine("----------------------------------------------------------");
        }

        public ComponentMetaDataGroupDTO[] AddComponentGeneric(string jobId, string wellId, string assemblyId, string subassemblyId, string compgroup, string parttype, string colnames, string colvals, long eventId, int wellBorePerfStatus = 7)
        {
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            string[] colnamearay = colnames.Split(new char[] { ';' });
            string[] colvalray = colvals.Split(new char[] { ';' });

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
            //9/3/2019 - To avoid compilation error, passed an added parameter as null. Integration test needs to be fixed according to FRWM-5489
            //string hardCodedComponentGroupId = null;
            ComponentMetaDataGroupDTO[] cmpMetaData = ComponentService.GetComponentMetaDataForAdd(partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType.ToString(), partfilter.TypeId.ToString());
            Assert.IsNotNull(cmpMetaData);
            Trace.WriteLine("Adding Component with Comp Name : +" + compgroup + " Part Type :" + parttype);
            Trace.WriteLine("Component has " + cmpMetaData.Count() + " categories");
            foreach (ComponentMetaDataGroupDTO cmpMD in cmpMetaData)
            {
                Trace.WriteLine("Category Name : " + cmpMD.CategoryName);
            }

            MetaDataDTO[] cdReference = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;

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
            string deffirst = "";
            if (parttype == "Plug - Cement")
            {
                deffirst = cdMetaData.FirstOrDefault(x => x.ControlText.Contains("Plug Back")).ControlText;
                //Trace.WriteLine("Catalog value for Plug Cement :" + deffirst);
            }
            else
            {
                deffirst = cdMetaData[0].ControlText;
                //Trace.WriteLine("Catalog value for Plug Cement :" + deffirst);
            }

            ControlIdTextDTO cdm = cdMetaData.FirstOrDefault(x => x.ControlText == deffirst);

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
            if (parttype.ToUpper().Equals("PERFORATION HOLE/SLOT DETAIL"))
            {
                reqComponent.ExtendedAssemblyComponentTable = "AssemblyComponentWellPerforationStatus";
                reqComponent.ExtendedComponentTable = "ComponentTubingPerforation";

            }
            if (parttype.ToUpper().Equals("GRAVEL PACK"))
            {
                reqComponent.ExtendedAssemblyComponentTable = "";
                reqComponent.ExtendedComponentTable = "";

            }

            reqComponent.Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCatalogItem").DataValue = cdm.ControlId;
            //reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcCompName").DataValue = deffirst;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_Manufacturers").DataValue = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcInnerDiameter").DataValue = 1.5m;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcOuterDiameter").DataValue = 2.5m;

            //cmpMetaData = ComponentService.GetComponentIDAndOD(cmpMetaData);
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Assembly").DataValue = assemblyId;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascTopDepth").DataValue = 100;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascBottomDepth").DataValue = 500;
            //reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascEndEventDT").DataValue = getJob.EndDate.AddDays(50).ToString();
            //reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_SubAssembly").DataValue = subassemblyId;
            reqComponent.PartTypePrimaryKey = partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType;

            for (int i = 0; i < colnamearay.Length; i++)
            {
                int intres = -1;
                if (int.TryParse(colvalray[i], out intres))
                {
                    reqComponent.Fields.FirstOrDefault(x => x.ColumnName == colnamearay[i]).DataValue = intres;
                }
                else
                {
                    reqComponent.Fields.FirstOrDefault(x => x.ColumnName == colnamearay[i]).DataValue = colvalray[i];
                }
            }

            reqComponent.Order = 1;

            reqComponent.PartType = partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptyPartType;

            ComponentMetaDataGroupDTO addComponent = new ComponentMetaDataGroupDTO();
            addComponent.CategoryName = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Additional").CategoryName;
            addComponent.JobId = Convert.ToInt64(jobId);
            addComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
            addComponent.ExtendedAssemblyComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compgroup).ptyExtendedAssemblyComponentTableName;
            addComponent.ExtendedComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compgroup).ptyExtendedComponentTableName;
            if (parttype.ToUpper().Equals("PERFORATION HOLE/SLOT DETAIL"))
            {
                addComponent.ExtendedAssemblyComponentTable = "AssemblyComponentWellPerforationStatus";
                addComponent.ExtendedComponentTable = "ComponentTubingPerforation";
            }
            if (parttype.ToUpper().Equals("GRAVEL PACK"))
            {
                reqComponent.ExtendedAssemblyComponentTable = "";
                reqComponent.ExtendedComponentTable = "";
            }

            addComponent.Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Additional").Fields;
            addComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcRemark").DataValue = "Additional Remark";
            addComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascTotalWeight").DataValue = 125;
            addComponent.Fields.FirstOrDefault(x => x.ColumnName.ToUpper() == "ASCINSTALLDATE").DataValue = getJob.EndDate;

            if (compgroup.ToUpper() == "ROD STRING" && parttype.ToUpper() == "ROD PUMP (INSERT)")
            {
                addComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcSerialNo").DataValue = "123";
                addComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcAPIDescription").DataValue = "API Description";
            }
            addComponent.PartTypePrimaryKey = partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptgFK_c_MfgCat_PartType;
            addComponent.Order = 1;
            addComponent.PartType = partTypes.FirstOrDefault(x => x.ptyPartType == parttype).ptyPartType;
            List<ComponentMetaDataGroupDTO> listComponent = new List<ComponentMetaDataGroupDTO>();

            ComponentMetaDataGroupDTO partTypeSpecificComponent = new ComponentMetaDataGroupDTO();

            listComponent.Add(reqComponent);
            listComponent.Add(addComponent);

            if (cmpMetaData.Count() > 2)
            {
                partTypeSpecificComponent.CategoryName = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Part Type Specific").CategoryName;
                partTypeSpecificComponent.Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Part Type Specific").Fields;
                partTypeSpecificComponent.JobId = Convert.ToInt64(jobId);
                partTypeSpecificComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
                partTypeSpecificComponent.ExtendedAssemblyComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compgroup).ptyExtendedAssemblyComponentTableName;
                partTypeSpecificComponent.ExtendedComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == compgroup).ptyExtendedComponentTableName;
                if (compgroup.ToUpper() == "ROD STRING" && parttype.ToUpper() == "ROD PUMP (INSERT)")
                {
                    partTypeSpecificComponent.Fields.FirstOrDefault(x => x.ColumnName == "cspAPIBarrelLength").DataValue = 12;
                    partTypeSpecificComponent.Fields.FirstOrDefault(x => x.ColumnName == "arpAPIDescription").DataValue = "API Description 1";
                    partTypeSpecificComponent.Fields.FirstOrDefault(x => x.ColumnName == "arpAPIExtraDescriptionText").DataValue = "API Description 2";
                }
                else if (compgroup.ToUpper() == "BOREHOLE" && parttype.ToUpper() == "WELLBORE COMPLETION (PRODUCING INTERVAL)")
                {
                    partTypeSpecificComponent.Fields.FirstOrDefault(x => x.ColumnName == "priUWIntID").DataValue = "123";
                }
                else if (compgroup.ToUpper() == "BOREHOLE" && parttype.ToUpper() == "WELLBORE COMPLETION DETAIL (PERFORATIONS, ETC.)")
                {
                    partTypeSpecificComponent.Fields.FirstOrDefault(x => x.ColumnName == "prfPerfType").DataValue = "Perf Type";
                    partTypeSpecificComponent.Fields.FirstOrDefault(x => x.ColumnName == "prfFK_r_PerforationTool").DataValue = 8;
                    if (wellBorePerfStatus != 1)
                        partTypeSpecificComponent.Fields.FirstOrDefault(x => x.ColumnName == "apsFK_r_WellPerforationStatus").DataValue = wellBorePerfStatus;
                    partTypeSpecificComponent.Fields.FirstOrDefault(x => x.ColumnName == "prfPerfDensity").DataValue = 22.5;
                    partTypeSpecificComponent.Fields.FirstOrDefault(x => x.ColumnName == "prfPerfDiameter").DataValue = 28.5;
                }
                else if (compgroup.ToUpper() == "TUBING STRING" && parttype.ToUpper() == "PERFORATION HOLE/SLOT DETAIL")
                {
                    partTypeSpecificComponent.ExtendedAssemblyComponentTable = "AssemblyComponentWellPerforationStatus";
                    partTypeSpecificComponent.ExtendedComponentTable = "ComponentTubingPerforation";
                    long perforation_status = 2;
                    partTypeSpecificComponent.Fields.FirstOrDefault(x => x.ColumnName == "apsFK_r_WellPerforationStatus").DataValue = perforation_status;

                }
                listComponent.Add(partTypeSpecificComponent);
            }
            ComponentMetaDataGroupDTO[] arrComponent = listComponent.ToArray();

            return arrComponent;
        }

        public void AddComponent_DrillingReport(string wellId, string assemblyId, string subassemblyId, string jobId, string CompGp, string Partype, string CompName, string CompRemarks, int wellBorePerfStatus = 7)
        {
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.WellBoreReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.DrillingReport);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Wellbore Report");

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

            string colNames = "cmcCompName;ascRemark";
            string colVals = CompName + ";" + CompRemarks;
            ComponentMetaDataGroupDTO[] arrComponent = null;

            if (Partype.Equals("Wellbore Completion Detail (Perforations, etc.)"))
            {
                arrComponent = AddComponentGeneric(jobId, wellId, assemblyId, subassemblyId, CompGp, Partype, colNames, colVals, check, wellBorePerfStatus);
            }
            else
            {
                arrComponent = AddComponentGeneric(jobId, wellId, assemblyId, subassemblyId, CompGp, Partype, colNames, colVals, check);
            }
            ComponentPartTypeDTO details = new ComponentPartTypeDTO();
            details.JobId = Convert.ToInt64(jobId);
            details.ComponentId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == CompGp).ptgFK_c_MfgCat_ComponentGrouping;
            details.PartTypeId = partTypes.FirstOrDefault(x => x.ptyPartType == Partype).ptgFK_c_MfgCat_PartType;

            ComponentMetaDataGroupBatchCollectionDTO batchDetailsComp = new ComponentMetaDataGroupBatchCollectionDTO();
            batchDetailsComp.ActionPerformed = CRUDOperationTypes.Add;
            batchDetailsComp.ComponentMetadataCollection = arrComponent;
            batchDetailsComp.ComponentPartType = details;

            List<ComponentMetaDataGroupBatchCollectionDTO> listComp = new List<ComponentMetaDataGroupBatchCollectionDTO>();
            listComp.Add(batchDetailsComp);
            ComponentMetaDataGroupBatchCollectionDTO[] arrComp = listComp.ToArray();

            bool saveBatch = ComponentService.SaveWellboreComponent(arrComp);
            Assert.IsTrue(saveBatch);
            Trace.WriteLine("Component Added with Component group :" + CompGp + " and Part type " + Partype);
            Trace.WriteLine("----------------------------------------------------------");
        }

        //Test method for customized pull ticket report
        //Report for WellBore Notes
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetPullReportForWellBoreNotes()
        {
            WellDTO well = AddWell("RPOC_");
            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();
            string subassemblyId = well.SubAssemblyId.ToString();
            string jobId = AddJob("Planned");
            var getjob = JobAndEventService.GetJobById(jobId);
            AddComponent(wellId, assemblyId, subassemblyId, jobId, "Wellbore Notes", "Wellbore Notes", "WellBore Name", "WellBore Remarks");
            AssemblyComponentDTO[] getWellBoreNotes = JobAndEventService.GetWellboreNotesByJobId(jobId);
            Trace.WriteLine("-------------WellBore Notes----------------");
            Trace.WriteLine("Date : " + getWellBoreNotes[0].InstallDate);
            Trace.WriteLine("Component Name : " + getWellBoreNotes[0].Name);
            Trace.WriteLine("Wellbore Notes : " + getWellBoreNotes[0].ComponentRemarks);
            Trace.WriteLine("Remarks : " + getWellBoreNotes[0].Remark);
            Trace.WriteLine("Top Depth : " + getWellBoreNotes[0].TopDepth);
            Trace.WriteLine("Bottom Depth : " + getWellBoreNotes[0].BottomDepth);

            Assert.AreEqual(getjob.EndDate.Date, getWellBoreNotes[0].InstallDate, "Install date is not matching");
            Assert.AreEqual("WELLBORE NAME", getWellBoreNotes[0].Name.ToUpper(), "Component Name is not matching");
            Assert.AreEqual("ADDITIONAL REMARK", getWellBoreNotes[0].ComponentRemarks.ToUpper(), "Wellbore notes are not matching");
            Assert.AreEqual("WELLBORE REMARKS", getWellBoreNotes[0].Remark.ToUpper(), "Remark is not matching");
            Assert.AreEqual(100, getWellBoreNotes[0].TopDepth, "TopDepth is not mathcing");
            Assert.AreEqual(500, getWellBoreNotes[0].BottomDepth, "BottomDepth is not mathcing");
        }

        //Report for Casing And Linear Details
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetPullReportForCasingAndLinear()
        {
            WellDTO well = AddWell("RPOC_");
            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();
            string subassemblyId = well.SubAssemblyId.ToString();
            string jobId = AddJob("Planned");
            var getjob = JobAndEventService.GetJobById(jobId);
            AddComponent(wellId, assemblyId, subassemblyId, jobId, "Intermediate Casing", "Casing Shoe", "Intermediate Casing", "Casing Remarks");
            AssemblyComponentDTO[] casingaAndLinearDetails = JobAndEventService.GetCasingAndLinerDetailsByJobId(jobId);
            Assert.IsNotNull(casingaAndLinearDetails, "Failed to get Casing details for the added Job");
            Assert.IsTrue(casingaAndLinearDetails.Count() > 0, "Failed to get casing details for the pull ticket details");
            Trace.WriteLine("-------------Casing and Linear Details----------------");
            Trace.WriteLine("Date : " + casingaAndLinearDetails[0].InstallDate);
            Trace.WriteLine("Grouping (String) : " + casingaAndLinearDetails[0].Grouping);
            Trace.WriteLine("Part Type : " + casingaAndLinearDetails[0].PartType);
            Trace.WriteLine("Quantity (Joints): " + casingaAndLinearDetails[0].Quantity);
            Trace.WriteLine("Size : " + casingaAndLinearDetails[0].OuterDiameter); // For Size Purpose
            Trace.WriteLine("Outer Diameter : " + casingaAndLinearDetails[0].OuterDiameter);
            Trace.WriteLine("Inner Diameter : " + casingaAndLinearDetails[0].InnerDiameter);
            Trace.WriteLine("Total Weight : " + casingaAndLinearDetails[0].TotalWeight);
            Trace.WriteLine("Grade : " + casingaAndLinearDetails[0].Grade);
            Trace.WriteLine("Top Depth : " + casingaAndLinearDetails[0].TopDepth);
            Trace.WriteLine("Bottom Depth : " + casingaAndLinearDetails[0].BottomDepth);

            Assert.AreEqual(getjob.EndDate.Date, casingaAndLinearDetails[0].InstallDate, "Date is not matching");
            Assert.AreEqual("INTERMEDIATE CASING", casingaAndLinearDetails[0].Grouping.ToUpper(), "Grouping(String) is not matching");
            Assert.AreEqual("CASING SHOE", casingaAndLinearDetails[0].PartType.ToUpper(), "Part Type is not matching");
            Assert.AreEqual(1, casingaAndLinearDetails[0].Quantity, "Quantity (Joints) is not matching");
            Assert.AreEqual(2.5m, casingaAndLinearDetails[0].OuterDiameter, "Outer diameter (size) is not matching");
            Assert.AreEqual(2.5m, casingaAndLinearDetails[0].OuterDiameter, "Outer diameter is not matching");
            Assert.AreEqual(1.5m, casingaAndLinearDetails[0].InnerDiameter, "Inner diameter is not matching");
            Assert.AreEqual(125, casingaAndLinearDetails[0].TotalWeight, "Total weight is not matching");
            Assert.AreEqual("_N/A", casingaAndLinearDetails[0].Grade, "Grade is not matching");
            Assert.AreEqual(100, casingaAndLinearDetails[0].TopDepth, "TopDepth is not mathcing");
            Assert.AreEqual(500, casingaAndLinearDetails[0].BottomDepth, "BottomDepth is not mathcing");
        }

        //Report for WellBore Perf Details
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetPullReportForWellBorePerforation()
        {
            WellDTO well = AddWell("RPOC_");
            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();
            string subassemblyId = well.SubAssemblyId.ToString();
            string jobId = AddJob("Planned");

            AddComponent(wellId, assemblyId, subassemblyId, jobId, "Borehole", "Wellbore Completion Detail (Perforations, etc.)", "Perf Details Name", "Perf Details Remarks");
            AssemblyComponentDTO[] wellborePerfDetails = JobAndEventService.GetWellborePerfDetailsByJobId(jobId);
            Assert.IsNotNull(wellborePerfDetails, "Failed to get details for the added Job");
            Assert.IsTrue(wellborePerfDetails.Count() > 0, "Failed to get details for the pull ticket details");
            Trace.WriteLine("-------------WellBore Perf Details----------------");
            Trace.WriteLine("Date Shot : " + wellborePerfDetails[0].PerforationDetails.DateShot);
            Trace.WriteLine("Opening Type : " + wellborePerfDetails[0].Name);
            Trace.WriteLine("Status : " + wellborePerfDetails[0].PerforationDetails.Status);
            Trace.WriteLine("Top Depth : " + wellborePerfDetails[0].TopDepth);
            Trace.WriteLine("Bottom Depth : " + wellborePerfDetails[0].BottomDepth);
            Trace.WriteLine("Perf Density (SPF): " + wellborePerfDetails[0].PerforationDetails.PerfDensity);
            Trace.WriteLine("Component Remarks (Perf Details): " + wellborePerfDetails[0].ComponentRemarks);
            Trace.WriteLine("Perf Diameter : " + wellborePerfDetails[0].PerforationDetails.PerfDiameter);
            Trace.WriteLine("Perf Type : " + wellborePerfDetails[0].PerforationDetails.PerfType);
            Trace.WriteLine("Perf Tool : " + wellborePerfDetails[0].PerforationDetails.PerfTool);

            //Not sure whether following statement makes sense, need to check with Hardik Doshi
            //Assert.AreEqual(DateTime.Now.Date, wellborePerfDetails[0].PerforationDetails.DateShot, "Install date is not matching");
            Assert.AreEqual("PERF DETAILS NAME", wellborePerfDetails[0].Name.ToUpper(), "Opening type is not matching");
            Assert.AreEqual("ISOLATED", wellborePerfDetails[0].PerforationDetails.Status.ToUpper(), "Status is not matching");
            Assert.AreEqual(100, wellborePerfDetails[0].TopDepth, "Top depth is not matching");
            Assert.AreEqual(500, wellborePerfDetails[0].BottomDepth, "Bottom depth is not matching");
            Assert.AreEqual(22.50m, wellborePerfDetails[0].PerforationDetails.PerfDensity, "Density(SPF) is not matching");
            Assert.AreEqual("ADDITIONAL REMARK", wellborePerfDetails[0].ComponentRemarks.ToUpper(), "Component remark(Perf details) is not matching");
            Assert.AreEqual(28.5000m, wellborePerfDetails[0].PerforationDetails.PerfDiameter, "Diameter is not matching");
            Assert.AreEqual("PERF TYPE", wellborePerfDetails[0].PerforationDetails.PerfType.ToUpper(), "Type is not matching");
            Assert.AreEqual("DRILLED", wellborePerfDetails[0].PerforationDetails.PerfTool.ToUpper(), "Tool is not matching");
        }

        //Report for Tubing Component
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetPullReportForTubingComponent()
        {
            WellDTO well = AddWell("RPOC_");
            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();
            string subassemblyId = well.SubAssemblyId.ToString();
            string jobId = AddJob("Planned");
            var getjob = JobAndEventService.GetJobById(jobId);
            AddComponent(wellId, assemblyId, subassemblyId, jobId, "Tubing String", "ESP Bolt on Intake (Serialized)", "TubingName", "TubingRemarks");
            AssemblyComponentDTO[] getTubingComponentsForWell = JobAndEventService.GetTubingComponentsByJobId(jobId);
            Assert.IsNotNull(getTubingComponentsForWell, "Failed to get details for the added Job");
            Assert.IsTrue(getTubingComponentsForWell.Count() > 0, "Failed to get details for the pull ticket details");
            Trace.WriteLine("-------------Tubing Components----------------");
            Trace.WriteLine("Date : " + getTubingComponentsForWell[0].InstallDate);
            Trace.WriteLine("Part Type : " + getTubingComponentsForWell[0].PartType);
            Trace.WriteLine("Component Remarks (Tubing Component) : " + getTubingComponentsForWell[0].ComponentRemarks);
            Trace.WriteLine("Quantity : " + getTubingComponentsForWell[0].Quantity);
            Trace.WriteLine("Outer Diameter : " + getTubingComponentsForWell[0].OuterDiameter);
            Trace.WriteLine("Inner Diameter : " + getTubingComponentsForWell[0].InnerDiameter);
            Trace.WriteLine("Total Weight : " + getTubingComponentsForWell[0].TotalWeight);
            Trace.WriteLine("Grade : " + getTubingComponentsForWell[0].Grade);
            Trace.WriteLine("Top Depth : " + getTubingComponentsForWell[0].TopDepth);
            Trace.WriteLine("Bottom Depth : " + getTubingComponentsForWell[0].BottomDepth);

            Assert.AreEqual(getjob.EndDate.Date, getTubingComponentsForWell[0].InstallDate, "Date is not matching");
            Assert.AreEqual("ESP BOLT ON INTAKE (SERIALIZED)", getTubingComponentsForWell[0].PartType.ToUpper(), "Part type is not matching");
            Assert.AreEqual("ADDITIONAL REMARK", getTubingComponentsForWell[0].ComponentRemarks.ToUpper(), "Component remark (tubing component) is not matching");
            Assert.AreEqual(1, getTubingComponentsForWell[0].Quantity, "Quantity is not matching");
            Assert.AreEqual(2.5m, getTubingComponentsForWell[0].OuterDiameter, "Outer diameter is not matching");
            Assert.AreEqual(1.5m, getTubingComponentsForWell[0].InnerDiameter, "Inner diameter is not matching");
            Assert.AreEqual(125, getTubingComponentsForWell[0].TotalWeight, "Wait is not matching");
            Assert.AreEqual("N_A", getTubingComponentsForWell[0].Grade, "Grade is not matching");
            Assert.AreEqual(100, getTubingComponentsForWell[0].TopDepth, "TopDepth is not matching");
            Assert.AreEqual(500, getTubingComponentsForWell[0].BottomDepth, "BottomDepth is not matching");
        }

        //Report for Rod Component
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetPullReportForRodComponent()
        {
            WellDTO well = AddWell("RPOC_");
            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();
            string subassemblyId = well.SubAssemblyId.ToString();
            string jobId = AddJob("Planned");
            var getjob = JobAndEventService.GetJobById(jobId);
            AddComponent(wellId, assemblyId, subassemblyId, jobId, "Rod String", "Polished Rod", "Rod CompName", "Rod CompRemarks");
            AssemblyComponentDTO[] getRodComponents = JobAndEventService.GetRodComponentsByJobId(jobId);
            Assert.IsNotNull(getRodComponents, "Failed to get details for the added Job");
            Assert.IsTrue(getRodComponents.Count() > 0, "Failed to get details for the pull ticket details");
            Trace.WriteLine("-------------Rod Components----------------");
            Trace.WriteLine("Install Date : " + getRodComponents[0].InstallDate);
            Trace.WriteLine("Part Type : " + getRodComponents[0].PartType);
            Trace.WriteLine("Component Remarks(Rod Component) : " + getRodComponents[0].ComponentRemarks);
            Trace.WriteLine("Quantity (Count) : " + getRodComponents[0].Quantity);
            Trace.WriteLine("Grade : " + getRodComponents[0].Grade);
            Trace.WriteLine("OuterDiameter (Size) : " + getRodComponents[0].OuterDiameter);
            Trace.WriteLine("Length : " + getRodComponents[0].Length);
            Trace.WriteLine("Top Depth : " + getRodComponents[0].TopDepth);
            Trace.WriteLine("Bottom Depth : " + getRodComponents[0].BottomDepth);
            Trace.WriteLine("Remarks : " + getRodComponents[0].Remark);

            Assert.AreEqual(getjob.EndDate.Date, getRodComponents[0].InstallDate, "Install date is not matching");
            Assert.AreEqual("POLISHED ROD", getRodComponents[0].PartType.ToUpper(), "Part type is not matching");
            Assert.AreEqual("ADDITIONAL REMARK", getRodComponents[0].ComponentRemarks.ToUpper(), "Component remark (rod component) is not matching");
            Assert.AreEqual(1, getRodComponents[0].Quantity, "Quantity (Count) is not matching");
            Assert.AreEqual("C", getRodComponents[0].Grade, "Grade is not matching");
            Assert.AreEqual(2.5m, getRodComponents[0].OuterDiameter, "Outer diameter is not matching");
            Assert.AreEqual(0, getRodComponents[0].Length, "Length is not matching");
            Assert.AreEqual("ROD COMPREMARKS", getRodComponents[0].Remark.ToUpper(), "Remark is not matching");
            Assert.AreEqual(100, getRodComponents[0].TopDepth, "TopDepth is not matching");
            Assert.AreEqual(500, getRodComponents[0].BottomDepth, "BottomDepth is not matching");
        }

        //Report for Pump Info
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetPullReportForPumpInfo()
        {
            WellDTO well = AddWell("RPOC_");
            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();
            string subassemblyId = well.SubAssemblyId.ToString();
            string jobId = AddJob("Planned");
            var getjob = JobAndEventService.GetJobById(jobId);
            AddComponent(wellId, assemblyId, subassemblyId, jobId, "Rod String", "Rod Pump (Insert)", "Pump Name", "Pump Remarks");
            AssemblyComponentDTO[] getPumpInfo = JobAndEventService.GetPumpInfoByJobId(jobId);
            Assert.IsNotNull(getPumpInfo, "Failed to get details for the added Job");
            Assert.IsTrue(getPumpInfo.Count() > 0, "Failed to get details for the pull ticket details");
            Trace.WriteLine("-------------Pump Infor----------------");
            Trace.WriteLine("Serial No : " + getPumpInfo[0].SerialNo);
            Trace.WriteLine("Name : " + getPumpInfo[0].Name);
            Trace.WriteLine("Install Date : " + getPumpInfo[0].InstallDate);
            Trace.WriteLine("Outer Diameter (Size) : " + getPumpInfo[0].OuterDiameter);
            Trace.WriteLine("API Description : " + getPumpInfo[0].APIDescription);
            Trace.WriteLine("Top Depth : " + getPumpInfo[0].TopDepth);
            Trace.WriteLine("Bottom Depth : " + getPumpInfo[0].BottomDepth);

            Assert.AreEqual(getjob.EndDate.Date, getPumpInfo[0].InstallDate, "Install date is not matching");
            Assert.AreEqual("123", getPumpInfo[0].SerialNo, "Serial No is not matching");
            Assert.AreEqual("PUMP NAME", getPumpInfo[0].Name.ToUpper(), "Name is not matching");
            Assert.AreEqual(2.5m, getPumpInfo[0].OuterDiameter, "Outer diameter(Size) is not matching");
            Assert.AreEqual("API DESCRIPTION", getPumpInfo[0].APIDescription.ToUpper(), "API Description is not matching");
            Assert.AreEqual(100, getPumpInfo[0].TopDepth, "TopDepth is not matching");
            Assert.AreEqual(500, getPumpInfo[0].BottomDepth, "BottomDepth is not matching");
        }

        //Report for PlugBack
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetPullReportForPlugBack()
        {
            WellDTO well = AddWell("RPOC_");
            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();
            string subassemblyId = well.SubAssemblyId.ToString();
            string jobId = AddJob("Planned");
            var getjob = JobAndEventService.GetJobById(jobId);
            AddComponent(wellId, assemblyId, subassemblyId, jobId, "Drilling Liner", "Plug - Cement", "Plug Name", "Plug-Remarks");
            AssemblyComponentDTO[] getPlugBackDetail = JobAndEventService.GetPlugBackComponentsByJobId(jobId);
            Assert.IsNotNull(getPlugBackDetail, "Failed to get details for the added Job");
            Assert.IsTrue(getPlugBackDetail.Count() > 0, "Failed to get details for the pull ticket details");
            Trace.WriteLine("-------------Plug back details----------------");
            Trace.WriteLine("Date : " + getPlugBackDetail[0].InstallDate);
            Trace.WriteLine("Grouping (String) : " + getPlugBackDetail[0].Grouping);
            Trace.WriteLine("Component Name : " + getPlugBackDetail[0].Name);
            Trace.WriteLine("Top Depth : " + getPlugBackDetail[0].TopDepth);
            Trace.WriteLine("Bottom Depth : " + getPlugBackDetail[0].BottomDepth);

            Assert.AreEqual(getjob.EndDate.Date, getPlugBackDetail[0].InstallDate, "Install date is not matching");
            Assert.AreEqual("DRILLING LINER", getPlugBackDetail[0].Grouping.ToUpper(), "Grouping is not matching");
            Assert.AreEqual("PLUG NAME", getPlugBackDetail[0].Name.ToUpper(), "Name is not matching");
            Assert.AreEqual(100, getPlugBackDetail[0].TopDepth, "TopDepth is not matching");
            Assert.AreEqual(500, getPlugBackDetail[0].BottomDepth, "BottomDepth is not matching");
        }

        //Report for FillClean
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetPullReportForFillClean()
        {
            WellDTO well = AddWell("RPOC_");
            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();
            string subassemblyId = well.SubAssemblyId.ToString();
            string jobId = AddJob("Planned");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            CreateEventForCleanFillEvent(jobId);

            EventCleanFillDTO[] getEventCleanFill = JobAndEventService.GetCleanFillEventsForJob(jobId);
            Assert.IsNotNull(getEventCleanFill, "Failed to get Casing for the added Job");
            Assert.IsTrue(getEventCleanFill.Count() > 0, "Failed to get details for the pull ticket details");
            Trace.WriteLine("-------------Fill Clean details----------------");
            Trace.WriteLine("Job ID :" + getEventCleanFill[0].JobId);
            Trace.WriteLine("Begin Date :" + getEventCleanFill[0].BeginDate);
            Trace.WriteLine("End Date :" + getEventCleanFill[0].EndDate);
            Trace.WriteLine("Tagged :" + getEventCleanFill[0].Tagged);
            Trace.WriteLine("Clean To Method :" + getEventCleanFill[0].CleanedTo);
            Trace.WriteLine("Clean Type Method :" + getEventCleanFill[0].CleanType);
            Trace.WriteLine("Tool Size :" + getEventCleanFill[0].ToolSize);
            Trace.WriteLine("Hours :" + getEventCleanFill[0].Hours);
            Trace.WriteLine("Remarks :" + getEventCleanFill[0].Remarks);

            Assert.AreEqual(jobId, getEventCleanFill[0].JobId.ToString(), "Job id is not matching");
            Assert.AreEqual(getJob.BeginDate.Date.ToString(), getEventCleanFill[0].BeginDate.ToString(), "Begin date is not matching");
            Assert.AreEqual(getJob.EndDate.Date.ToString(), getEventCleanFill[0].EndDate.ToString(), "End date is not matching");
            Assert.AreEqual(16m, getEventCleanFill[0].Tagged, "Tagged is not matching");
            Assert.AreEqual(8m, getEventCleanFill[0].CleanedTo, "CleanedTo is not matching");
            Assert.AreEqual("TUBING BAIL", getEventCleanFill[0].CleanType.ToUpper(), "CleanType is not matching");
            Assert.AreEqual(24m, getEventCleanFill[0].ToolSize, "Tool size is not matching");
            Assert.AreEqual(20.5m, getEventCleanFill[0].Hours, "Hours are not matching");
            Assert.AreEqual(getJob.JobRemarks, getEventCleanFill[0].Remarks, "Job remark is not matching");
        }

        //Report for JobHistory
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetPullReportForJobHistory()
        {
            WellDTO well = AddWell("RPOC_");
            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();
            string subassemblyId = well.SubAssemblyId.ToString();
            string[] jobId = AddMultiJob("Planned", 10);
            int count = 0;
            int numberOfJobHistory = 8;

            JobLightDTO[] getJobHistory = JobAndEventService.GetJobsHistory(wellId, numberOfJobHistory.ToString());
            Assert.IsNotNull(getJobHistory, "Failed to get details for the added Job");
            Assert.IsTrue(getJobHistory.Count() > 0, "Failed to get details for the pull ticket details");
            foreach (JobLightDTO jb in getJobHistory)
            {
                Trace.WriteLine("Begin Date For " + count + " : " + jb.BeginDate);
                Trace.WriteLine("End Date For " + count + " : " + jb.EndDate);
                Trace.WriteLine("Job Type For " + count + " : " + jb.JobType);
                Trace.WriteLine("Job Reason For " + count + " : " + jb.JobReason);
                Trace.WriteLine("-------------------------------------------");
                count = count + 1;
            }

            JobLightDTO[] getAllJob = JobAndEventService.GetJobsByWell(wellId);
            if (getAllJob.Count() > getJobHistory.Count())
                Assert.AreEqual(numberOfJobHistory, getJobHistory.Count(), "History");
            else
                Assert.AreEqual(getAllJob.Count(), getJobHistory.Count(), "History");

            for (int i = 0; i < getJobHistory.Count(); i++)
            {
                Assert.AreEqual(getAllJob[i].BeginDate, getJobHistory[i].BeginDate, "Begin date is not matching");
                Assert.AreEqual(getAllJob[i].EndDate, getJobHistory[i].EndDate, "End date is not matching");
                Assert.AreEqual(getAllJob[i].JobType, getJobHistory[i].JobType, "Job type is not matching");
                Assert.AreEqual(getAllJob[i].JobReason, getJobHistory[i].JobReason, "Job reason is not matching");
                Trace.WriteLine("Job history matched for :" + i + " - record");
            }
        }

        //Report for WellTestistory
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetPullReportForWellTestHistory()
        {
            WellDTO well = AddWell("RPOC_");
            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();
            string subassemblyId = well.SubAssemblyId.ToString();

            long lngassemblyid = -1;
            long.TryParse(assemblyId, out lngassemblyid);

            //adding multiple welltest records
            for (int i = 0; i < 5; i++)
            {
                WellTestDTO testDataDTO = new WellTestDTO();

                //testDataDTO.SampleDate = DateTime.Today.AddDays(-100);
                testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(100 + i * (20)));
                testDataDTO.TestDuration = 3 + i;
                testDataDTO.SPTCodeDescription = "RejectedTest";
                testDataDTO.Oil = 50 + (i * 2);
                testDataDTO.OilGravity = 5 + (i * 2);
                testDataDTO.Water = 60 + (i * 2);
                testDataDTO.WaterGravity = 6 + (i * 2);
                testDataDTO.Gas = 70 + (i * 2);
                testDataDTO.GasGravity = 7 + (i * 2);
                testDataDTO.AverageTubingPressure = 90 + (i * 2);
                testDataDTO.AverageTubingTemperature = 100 + (i * 2);
                testDataDTO.AverageCasingPressure = 20 + (i * 2);
                testDataDTO.AverageFluidAbovePump = 10 + (i * 2);
                testDataDTO.PumpIntakePressure = 100 + (i * 2);
                testDataDTO.StrokePerMinute = 15 + (i * 2);
                testDataDTO.PumpingHours = 15 + (i * 2);
                testDataDTO.PumpEfficiency = 15 + (i * 2);
                testDataDTO.WellId = long.Parse(wellId);

                WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(wellId).Units;
                bool saveWellTest = WellTestDataService.AddWellTestData(new WellTestAndUnitsDTO(units, testDataDTO));
                Assert.IsTrue(saveWellTest);
            }

            //get well history
            WellTestDTO[] getWellHistory = WellTestDataService.GetValidWellTestByWellIdForLastFewTests(wellId, "6");
            int count = 0;
            foreach (WellTestDTO wtd in getWellHistory)
            {
                Trace.WriteLine("Test Date For " + count + " : " + wtd.SampleDate);
                Trace.WriteLine("Duration For " + count + " : " + wtd.TestDuration);
                Trace.WriteLine("Oil For " + count + " : " + wtd.Oil);
                Trace.WriteLine("Water For " + count + " : " + wtd.Water);
                Trace.WriteLine("Gas For " + count + " : " + wtd.Gas);
                Trace.WriteLine("Total Fluid For " + count + " : " + wtd.TotalFluid);
                Trace.WriteLine("Pump Efficiency For " + count + " : " + wtd.PumpEfficiency);
                Trace.WriteLine("Pump Hours " + count + " : " + wtd.PumpEfficiency);
                //Trace.WriteLine("API Gravity " + count + " : ");
                //Trace.WriteLine("Remark For " + count + " : " + wtd.Comment);
                Trace.WriteLine("-------------------------------------------");
                count = count + 1;
            }

            for (int i = 0; i < getWellHistory.Count(); i++)
            {
                Assert.AreEqual((decimal)(3 + (i)), getWellHistory[i].TestDuration, "Duration value is not matching");
                Assert.AreEqual((decimal)(50 + (i * 2)), getWellHistory[i].Oil, "Oil value is not matching");
                Assert.AreEqual((decimal)(60 + (i * 2)), getWellHistory[i].Water, "Water value is not matching");
                Assert.AreEqual((decimal)(70 + (i * 2)), getWellHistory[i].Gas, "Gas value is not matching");
                Assert.AreEqual((decimal)((50 + (i * 2)) + (60 + (i * 2))), getWellHistory[i].Oil + getWellHistory[i].Water, "Total fluid value is not matching");
                Assert.AreEqual((decimal)(15 + (i * 2)), getWellHistory[i].PumpEfficiency, "Pump efficiency is not matching");
                Assert.AreEqual((decimal)(15 + (i * 2)), getWellHistory[i].PumpingHours, "Pump hours is not matching");
            }
        }

        //Report for PumpUnit
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetPullReportForPumpUnit()
        {
            WellDTO well = AddRRLWell("RPOC_00001");

            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO deleteWell = allWells?.FirstOrDefault(w => w.Name.Equals("RPOC_00001"));
            Assert.IsNotNull(deleteWell);
            _wellsToRemove.Add(deleteWell);

            ModelConfigDTO getPumpUnitInfo = ModelFileService.GetModelConfig(well.Id.ToString());
            Trace.WriteLine("Pumping unit manufacturer is: " + getPumpUnitInfo.Surface.PumpingUnit.Manufacturer);
            Trace.WriteLine("Pumping unit type is: " + getPumpUnitInfo.Surface.PumpingUnit.Type);
            Trace.WriteLine("Pumping unit gear box rating is: " + getPumpUnitInfo.Surface.PumpingUnit.GearBoxRating);
            Trace.WriteLine("Well name is: " + well.Name);
            Trace.WriteLine("Pumping unit description is: " + getPumpUnitInfo.Surface.PumpingUnit.Description);
            Trace.WriteLine("Pumping unit geom is: " + getPumpUnitInfo.Surface.PumpingUnit.Type);
            Trace.WriteLine("Maximum stroke length is: " + getPumpUnitInfo.Surface.PumpingUnit.MaxStrokeLength);
            Trace.WriteLine("Actual stroke length is :" + getPumpUnitInfo.Surface.ActualStrokeLength);

            Assert.AreEqual("LUFKIN", getPumpUnitInfo.Surface.PumpingUnit.Manufacturer.ToUpper(), "Manufacturer is not matching");
            Assert.AreEqual("C", getPumpUnitInfo.Surface.PumpingUnit.Type, "Type is not matching");
            Assert.AreEqual((double)912, getPumpUnitInfo.Surface.PumpingUnit.GearBoxRating, "Gear box is not matching");
            Assert.AreEqual("RPOC_00001", well.Name, "Well name is not matching");
            Assert.AreEqual("C-912-365-168 L LUFKIN C912-365-168 (94110C)", getPumpUnitInfo.Surface.PumpingUnit.Description, "Description box is not matching");
            Assert.AreEqual("C", getPumpUnitInfo.Surface.PumpingUnit.Type, "GEOM is not matching");
            Assert.AreEqual((double)168, getPumpUnitInfo.Surface.PumpingUnit.MaxStrokeLength, "Maximum stroke length is not matching");
            Assert.AreEqual((double)124.51, getPumpUnitInfo.Surface.ActualStrokeLength, "Actual stroke length is not matching");
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetJobsByAssemblyId()
        {
            string wellId = AddWell("RPOC_").Id.ToString();
            AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(wellId);
            //No Job Condition
            JobTinyDTO[] getJobs = JobAndEventService.GetJobsByAssemblyId(assembly.Id.ToString());
            Assert.IsNotNull(getJobs, "Get Jobs by Assembly API call Failed");
            Assert.AreEqual(0, getJobs.Count(), "Jobs are available without adding any...");
            //With jobs
            AddJob("Approved");
            getJobs = JobAndEventService.GetJobsByAssemblyId(assembly.Id.ToString());
            Assert.IsNotNull(getJobs, "Failed to retrieve Jobs by Assembly");
            Assert.AreEqual(1, getJobs.Count(), "Incorrect number of Job retrieved");
            string jobId = AddJob("Approved");
            getJobs = JobAndEventService.GetJobsByAssemblyId(assembly.Id.ToString());
            Assert.IsNotNull(getJobs, "Failed to retrieve Jobs by Assembly");
            Assert.AreEqual(2, getJobs.Count(), "Incorrect number of Job retrieved");
            //Verifying on Update
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            getJob.JobRemarks = "Updated the Job";
            JobAndEventService.UpdateJob(getJob);
            getJob = JobAndEventService.GetJobById(jobId);
            Assert.AreEqual("Updated the Job", getJob.JobRemarks, "Failed to update Job");
            getJobs = JobAndEventService.GetJobsByAssemblyId(assembly.Id.ToString());
            Assert.IsNotNull(getJobs, "Failed to retrieve Jobs by Assembly");
            Assert.AreEqual(2, getJobs.Count(), "Incorrect number of Job retrieved");
            //Verifying on remove
            bool removeJob = JobAndEventService.RemoveJob(jobId);
            Assert.IsTrue(removeJob, "Failed to remove Job");
            getJobs = JobAndEventService.GetJobsByAssemblyId(assembly.Id.ToString());
            Assert.IsNotNull(getJobs, "Failed to retrieve Jobs by Assembly");
            Assert.AreEqual(1, getJobs.Count(), "Incorrect number of Job retrieved");
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetJobsHistory()
        {
            string wellId = AddWell("RPOC_").Id.ToString();
            AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(wellId);
            //Add job
            string jobId = AddJob("Approved");
            JobHistoryDTO[] getHistory = JobAndEventService.GetJobHistoryByJobId(jobId);
            JobHistoryDTO history = new JobHistoryDTO();
            history.JobStatus = "Approved";
            Assert.IsNotNull(getHistory, "Failed to retrieve Job history");
            Assert.AreEqual(1, getHistory.Count(), "Incorrect number of Job history values");
            Assert.AreEqual("Approved", history.JobStatus, "Status is not matching");

            //Verifying on Update
            JobLightDTO JobbeforeUpdate = JobAndEventService.GetJobById(jobId.ToString());
            JobbeforeUpdate.Status = "Planned";
            JobAndEventService.UpdateJob(JobbeforeUpdate);

            getHistory = JobAndEventService.GetJobHistoryByJobId(jobId);
            Assert.IsNotNull(getHistory, "Failed to retrieve job history values");
            Assert.AreEqual(1, getHistory.Count(), "Incorrect number of Job history values");

            JobHistoryDTO[] getHistoryd = JobAndEventService.GetJobHistoryByJobId(jobId);
            JobHistoryDTO historyN = new JobHistoryDTO();
            historyN.JobStatus = "Planned";
            Assert.AreEqual(JobbeforeUpdate.Status, historyN.JobStatus, "Job Status is not matching");
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetDeletedLogs()
        {
            AddWell("RPOC_");
            string date = DateTime.UtcNow.AddDays(1).ToString("MM-dd-yyyy");
            DeleteAuditDTO[] deleteAudits = JobAndEventService.GetDeletedLogs("Job", date);
            Assert.AreEqual(0, deleteAudits.Count(), "Delete logs available for future date");
            string jobId = AddJob("Approved");
            bool removeJob = JobAndEventService.RemoveJob(jobId);
            Assert.IsTrue(removeJob, "Failed to remove Job");
            date = DateTime.UtcNow.AddDays(-2).ToString("MM-dd-yyyy");
            deleteAudits = JobAndEventService.GetDeletedLogs("Job", date);
            Assert.IsTrue(deleteAudits.Count() > 0, "Failed to get deleted logs from Job table");
        }

        /// <summary>
        /// Verifying that Wellbore Perf status added during component creation is being retreived in GetWellboreComponentByJobIdAndEventId API response
        /// </summary>
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void VerifyStatusColumnForWellbore()
        {
            //Creating a well
            WellDTO well = AddWell("RPOC_");

            //Creating tuple which will hold Component name & Wellbore perf status
            //Since Drilling liner & Rod String doesn't have wellbore perf status field, I have given placeholder i.e. 7
            //Tuple structure :- Comp grouping,Part type name,comp name, wellbore perf status,Remarks
            var componemtNameAndPerfStatus = new List<Tuple<string, string, string, int, string>>
            {
                Tuple.Create("Borehole","Wellbore Completion Detail (Perforations, etc.)","Perf Details Name1",4,"Perf Details Remarks"),
                Tuple.Create("Drilling Liner","Plug - Cement","Plug Name",7,"Plug-Remarks"),
                Tuple.Create("Rod String", "Polished Rod","Rod CompName",7,"Rod CompRemarks"),
                Tuple.Create("Borehole", "Wellbore Completion Detail (Perforations, etc.)","Perf Details Name2",1,"Perf Details Remarks")
            };

            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();
            string subassemblyId = well.SubAssemblyId.ToString();

            //Creating a Job
            string jobId = AddJob("Approved");

            //Adding Wellbore tab to Job
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Wellbore report tab not added successfully");

            //Adding Wellbore components. Part type name "Wellbore Completion Detail (Perforations, etc.)" has Well Perforation Status
            foreach (Tuple<string, string, string, int, string> comp in componemtNameAndPerfStatus)
            {
                AddComponent(wellId, assemblyId, subassemblyId, jobId, comp.Item1, comp.Item2, comp.Item3, comp.Item5, comp.Item4);
            }

            WellboreGridDTO[] wellboreComponents = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());

            WellboreGridGroupDTO[] retrievedComponents = wellboreComponents[0].WellboreGridGroup;

            //Verifying that correct status is being retrieved for respective components
            foreach (WellboreGridGroupDTO component in retrievedComponents)
            {
                string retrievedCompname = component.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required").Fields.FirstOrDefault(x => x.Title == "Component Name").DataValue.ToString();
                foreach (Tuple<string, string, string, int, string> comp in componemtNameAndPerfStatus)
                {
                    if (retrievedCompname.Equals(comp.Item3))
                    {
                        if (component.ComponentMetadata.Count() > 2 && comp.Item2 == "Wellbore Completion Detail (Perforations, etc.)")
                        {
                            string wellborePerfStatus = component.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Part Type Specific").Fields.FirstOrDefault(x => x.Title == "Well Perforation Status").DataValue.ToString();
                            Assert.AreEqual(comp.Item4.ToString(), wellborePerfStatus, "Wellbore status retrieved for " + comp.Item3 + " is not correct");
                            Trace.WriteLine(retrievedCompname + " Component has correct Wellbore perf status");
                        }
                        else
                        {
                            Trace.WriteLine(retrievedCompname + " Component doesn't have Wellbore perf status field");
                        }
                        break;
                    }
                }
            }
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void HistoricalWellboreCorrection()
        {
            //Creating a well
            WellDTO well = AddWell("RPOC_");
            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();
            string subassemblyId = well.SubAssemblyId.ToString();

            //Creating a Job
            string jobId = AddJob("Approved");
            AddComponent(wellId, assemblyId, subassemblyId, jobId, "Borehole", "Wellbore Completion Detail (Perforations, etc.)", "Perf Details Name", "Perf Details Remarks");
            AssemblyComponentDTO[] wellborePerfDetails = JobAndEventService.GetWellborePerfDetailsByJobId(jobId);
            Assert.IsNotNull(wellborePerfDetails, "Failed to get details for the added Job");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);

            //Adding data to Values
            HistoricalWellboreCorrectionJobDTO historicalJob = new HistoricalWellboreCorrectionJobDTO();
            historicalJob.JobId = Convert.ToInt64(getJob.JobId);
            historicalJob.BeginDate = getJob.BeginDate.AddDays(0).ToUniversalTime();
            historicalJob.EndDate = getJob.EndDate.AddDays(0).ToUniversalTime();
            historicalJob.JobReason = getJob.JobReason;
            historicalJob.JobType = getJob.JobType;

            HistoricalWellboreCorrectionJobDTO[] historicalReports = JobAndEventService.GetJobsForHistoricalWellboreCorrection(subassemblyId);
            Assert.IsNotNull(historicalReports, "Failed to retrieve Job with details");

            for (int i = 0; i < historicalReports.Count(); i++)
            {
                Assert.AreEqual(historicalJob.JobId, historicalReports[0].JobId, "Job Id is not matching");
                Assert.AreEqual(historicalJob.BeginDate, historicalReports[0].BeginDate, "Job Begin date is not matching");
                Assert.AreEqual(historicalJob.EndDate, historicalReports[0].EndDate, "Job End date is not matching");
                Assert.AreEqual(historicalJob.JobType, historicalReports[0].JobType, "Job Type is not matching");
                Assert.AreEqual(historicalJob.JobReason, historicalReports[0].JobReason, "Job Reason is not matching");
            }
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void CopyPasteJobTest()
        {
            //Creating a well
            WellDTO well = AddWell("RPOC_");
            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();

            //Creating a Job
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.JobPlan);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
            JobLightDTO[] Job = JobAndEventService.GetJobsByWell(getJob.WellId.ToString());
            Assert.IsTrue(Job.FirstOrDefault(j => j.JobId == getJob.JobId).ExistingJobEvents.HasJobPlanEvent);

            //Adding data to Values
            CopyPasteJobCriteriaDTO copyJob = new CopyPasteJobCriteriaDTO();
            copyJob.SourceWellId = Convert.ToInt64(well.Id);
            copyJob.SourceAssemblyId = Convert.ToInt64(getJob.AssemblyId);
            copyJob.JobToCopyId = Convert.ToInt64(getJob.JobId);
            copyJob.DestinationWellIdId = Convert.ToInt64(well.Id);
            copyJob.DestinationAssemblyId = Convert.ToInt64(getJob.AssemblyId);
            copyJob.BeginDate = getJob.BeginDate;
            copyJob.AddedJobID = getJob.JobId;
            //Copy and paste the job to same well
            CopyPasteJobCriteriaDTO reports = JobAndEventService.CopyPasteJob(copyJob);
            Assert.IsNotNull(reports, "Failed to retrieve Job with details");
            Assert.AreEqual("Success", reports.StatusMessage, "Job not copied and pasted properly");
        }

        [TestCategory(TestCategories.ComponentServiceTests), TestMethod]
        public void VerifyCloneGroup()
        {
            AddWell("RPOC_");
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.JobCostDetailReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.JobCostDetailReport);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
            JobLightDTO[] Job = JobAndEventService.GetJobsByWell(getJob.WellId.ToString());
            Assert.IsTrue(Job.FirstOrDefault(j => j.JobId == getJob.JobId).ExistingJobEvents.HasJobCostDetailEvent);
            EventDetailCostDTO anEventCostDetails = CreateEventDetailCostDTO(jobId);
            CatalogItemGroupDTO catalogDTO = JobAndEventService.GetCatalogItemGroupData();
            long catalogItemId = catalogDTO.CatalogItems.OrderBy(x => x.CatalogItemId).FirstOrDefault().CatalogItemId;
            anEventCostDetails.CatalogItemId = catalogItemId;

            //Call Web Service to Add New EventDetailCost record into Database
            string addedEventCostId = JobAndEventService.AddEventDetailCost(anEventCostDetails);

            // Get the EventDetailCost Record details using the Web Service using JobId
            JobCostDetailsDTO addedJobCost = JobAndEventService.GetJobCostDetailsForJob(jobId);

            EventDetailCostDTO getEventDetailCost = addedJobCost.EventDetailCostGroupData[0].EventDetailCosts.FirstOrDefault();

            //Verify Addition of EventDetailCost Record
            Assert.AreEqual(anEventCostDetails.JobId, getEventDetailCost.JobId, "New EventDetailCost record's Job Id field is not matching");
            Assert.AreEqual(anEventCostDetails.CatalogItemId, getEventDetailCost.CatalogItemId, "New EventDetailCost record's Catalog Item Id Cost field is not matching");
            Assert.AreEqual(anEventCostDetails.Cost, getEventDetailCost.Cost, "New EventDetailCost record's Cost field is not matching");
            Assert.AreEqual(anEventCostDetails.CostRemarks, getEventDetailCost.CostRemarks, "New EventDetailCost record's Cost Remarks field is not matching");
            Assert.AreEqual(anEventCostDetails.Discount, getEventDetailCost.Discount, "New EventDetailCost record's Discount field is not matching");
            Assert.AreEqual(anEventCostDetails.Quantity, getEventDetailCost.Quantity, "New EventDetailCost record's Quantity field is not matching");
            Assert.AreEqual(anEventCostDetails.UnitPrice, getEventDetailCost.UnitPrice, "New EventDetailCost record's Unit Price field is not matching");
            Assert.AreEqual(anEventCostDetails.VendorId, getEventDetailCost.VendorId, "New EventDetailCost record's Vendor Id field is not matching");
            Assert.AreEqual(anEventCostDetails.CostDate, getEventDetailCost.CostDate, "New EventDetailCost record's Cost Date field is not matching");

            //Update EventDetailCost
            EventDetailCostDTO updateEventDetailCost = new EventDetailCostDTO();
            updateEventDetailCost.Id = Convert.ToInt64(addedEventCostId);
            updateEventDetailCost.EventId = getEventDetailCost.EventId;
            updateEventDetailCost.JobId = Convert.ToInt64(jobId);
            updateEventDetailCost.CatalogItemId = catalogItemId;
            updateEventDetailCost.Cost = 20;
            updateEventDetailCost.CostRemarks = "Test Update Cost Remark";
            updateEventDetailCost.Discount = 1;
            updateEventDetailCost.Quantity = 7;
            updateEventDetailCost.UnitPrice = 3;
            updateEventDetailCost.VendorId = 3;
            updateEventDetailCost.CostDate = DateTime.Today.AddDays(-8);

            JobAndEventService.UpdateEventDetailCost(updateEventDetailCost);

            // Get the EventDetailCost Updated Record details using the Web Service using JobId
            JobCostDetailsDTO getUpdateJobCost = JobAndEventService.GetJobCostDetailsForJob(jobId);
            EventDetailCostDTO getUpdateEventDetailCost = getUpdateJobCost.EventDetailCostGroupData[0].EventDetailCosts.FirstOrDefault();

            //Verify Updation of EventDetailCost Record
            Assert.AreEqual(updateEventDetailCost.JobId, getUpdateEventDetailCost.JobId, "Updated EventDetailCost record's Job Id field is not matching");
            Assert.AreEqual(updateEventDetailCost.CatalogItemId, getUpdateEventDetailCost.CatalogItemId, "Updated EventDetailCost record's Catalog Item Id Cost field is not matching");
            Assert.AreEqual(updateEventDetailCost.Cost, getUpdateEventDetailCost.Cost, "Updated EventDetailCost record's Cost field is not matching");
            Assert.AreEqual(updateEventDetailCost.CostRemarks, getUpdateEventDetailCost.CostRemarks, "Updated EventDetailCost record's Cost Remarks field is not matching");
            Assert.AreEqual(updateEventDetailCost.Discount, getUpdateEventDetailCost.Discount, "Updated EventDetailCost record's Discount field is not matching");
            Assert.AreEqual(updateEventDetailCost.Quantity, getUpdateEventDetailCost.Quantity, "Updated EventDetailCost record's Quantity field is not matching");
            Assert.AreEqual(updateEventDetailCost.UnitPrice, getUpdateEventDetailCost.UnitPrice, "Updated EventDetailCost record's Unit Price field is not matching");
            Assert.AreEqual(updateEventDetailCost.VendorId, getUpdateEventDetailCost.VendorId, "Updated EventDetailCost record's Vendor Id field is not matching");
            Assert.AreEqual(updateEventDetailCost.CostDate, getUpdateEventDetailCost.CostDate, "Updated EventDetailCost record's Cost Date field is not matching");

            // Clone the updated EventDetailCost record
            string clonedEventDetailCostId = JobAndEventService.CloneEventDetailCost(getUpdateEventDetailCost.Id.ToString());

            // Get the EventDetailCost Cloned Record details using the Web Service using JobId
            JobCostDetailsDTO getClonedJobCost = JobAndEventService.GetJobCostDetailsForJob(jobId);
            EventDetailCostDTO getClonedEventDetailCost = getClonedJobCost.EventDetailCostGroupData[0].EventDetailCosts.Skip(1).FirstOrDefault();

            // Verify cloning of EventDetailCost
            Assert.AreEqual(updateEventDetailCost.JobId, getClonedEventDetailCost.JobId, "Updated EventDetailCost record's Job Id field is not matching");
            Assert.AreEqual(updateEventDetailCost.CatalogItemId, getClonedEventDetailCost.CatalogItemId, "Updated EventDetailCost record's Catalog Item Id Cost field is not matching");
            Assert.AreEqual(updateEventDetailCost.Cost, getClonedEventDetailCost.Cost, "Updated EventDetailCost record's Cost field is not matching");
            Assert.AreEqual(updateEventDetailCost.CostRemarks, getClonedEventDetailCost.CostRemarks, "Updated EventDetailCost record's Cost Remarks field is not matching");
            Assert.AreEqual(updateEventDetailCost.Discount, getClonedEventDetailCost.Discount, "Updated EventDetailCost record's Discount field is not matching");
            Assert.AreEqual(updateEventDetailCost.Quantity, getClonedEventDetailCost.Quantity, "Updated EventDetailCost record's Quantity field is not matching");
            Assert.AreEqual(updateEventDetailCost.UnitPrice, getClonedEventDetailCost.UnitPrice, "Updated EventDetailCost record's Unit Price field is not matching");
            Assert.AreEqual(updateEventDetailCost.VendorId, getClonedEventDetailCost.VendorId, "Updated EventDetailCost record's Vendor Id field is not matching");
            Assert.AreEqual(updateEventDetailCost.CostDate, getClonedEventDetailCost.CostDate, "Updated EventDetailCost record's Cost Date field is not matching");

            //Perform Clone on Job Cost Details.. Clone Group

            EventDetailCostGroupLightDTO EventDetailsCostGroupValue = new EventDetailCostGroupLightDTO();
            EventDetailsCostGroupValue.JobId = Convert.ToInt64(jobId);
            EventDetailsCostGroupValue.Grouping = getClonedJobCost.EventDetailCostGroupData[0].Grouping;
            var set1 = Convert.ToInt64(getClonedJobCost.EventDetailCostGroupData[0].EventDetailCosts[0].Id);
            var set2 = Convert.ToInt64(getClonedJobCost.EventDetailCostGroupData[0].EventDetailCosts[1].Id);
            List<long> JobCostList = new List<long>();
            JobCostList.Add(set1);
            JobCostList.Add(set2);
            EventDetailsCostGroupValue.EventDetailCostIds = JobCostList;
            JobAndEventService.CloneGroupEventDetailCost(EventDetailsCostGroupValue);
            JobCostDetailsDTO getClonedJobCostRecords = JobAndEventService.GetJobCostDetailsForJob(jobId);
            EventDetailCostDTO getClonedEventDetailCostRecords = getClonedJobCostRecords.EventDetailCostGroupData[1].EventDetailCosts.Skip(1).FirstOrDefault();

            // Verify cloning of Job Groups, note: Cloned event will pick-up UTC date of cloning.
            Assert.AreEqual(updateEventDetailCost.JobId, getClonedEventDetailCostRecords.JobId, "Updated EventDetailCost record's Job Id field is not matching");
            Assert.AreEqual(updateEventDetailCost.CatalogItemId, getClonedEventDetailCostRecords.CatalogItemId, "Updated EventDetailCost record's Catalog Item Id Cost field is not matching");
            Assert.AreEqual(updateEventDetailCost.Cost, getClonedEventDetailCostRecords.Cost, "Updated EventDetailCost record's Cost field is not matching");
            Assert.AreEqual(updateEventDetailCost.CostRemarks, getClonedEventDetailCostRecords.CostRemarks, "Updated EventDetailCost record's Cost Remarks field is not matching");
            Assert.AreEqual(updateEventDetailCost.Discount, getClonedEventDetailCostRecords.Discount, "Updated EventDetailCost record's Discount field is not matching");
            Assert.AreEqual(updateEventDetailCost.Quantity, getClonedEventDetailCostRecords.Quantity, "Updated EventDetailCost record's Quantity field is not matching");
            Assert.AreEqual(updateEventDetailCost.UnitPrice, getClonedEventDetailCostRecords.UnitPrice, "Updated EventDetailCost record's Unit Price field is not matching");
            Assert.AreEqual(updateEventDetailCost.VendorId, getClonedEventDetailCostRecords.VendorId, "Updated EventDetailCost record's Vendor Id field is not matching");
            Assert.AreEqual(DateTime.UtcNow.Date, getClonedEventDetailCostRecords.CostDate, "Updated EventDetailCost record's Cost Date field is not matching");

            //Remove EventDetailCost
            JobAndEventService.RemoveEventDetailCost(clonedEventDetailCostId);
            bool isDeleted = JobAndEventService.RemoveEventDetailCost(getEventDetailCost.Id.ToString());
            Assert.IsTrue(isDeleted);

            // Remove Event and its extended information
            evt.Id = check;
            bool rcheck = JobAndEventService.RemoveEvent(evt);
            Assert.IsTrue(rcheck, "Failed to remove Economic Analysis report");
        }

        /*
       * Below Test developed By Mintu Mukherjee
       */

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void VerfiyCurrentWellboreData()
        {
            string facilityId = s_isRunningInATS ? "RPOC_00001" : "RPOC_0001";
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
            Console.WriteLine("Well Created!");
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            string wellid = well.Id.ToString();
            _wellsToRemove.Add(well);

            //Header-Footer Config
            /*    HeaderFooterConfigDTO checkdata = ComponentService.GetTitleOfHeaderFooterConfiguration();
                HeaderFooterConfigDTO config = new HeaderFooterConfigDTO();
                config.DataID = 3557;
                config.DisplayName = "Integration Title";
                config.HeaderFooterConfig = HeaderFooterConfigurationType.Title;
               if (!checkdata.IsEmpty)
                {
                    config.Id = 1;
                }
                //Adding Title
                ComponentService.AddUpdateHeaderFooterConfiguration(config);
                Console.WriteLine("Title Added");
                HeaderFooterConfigDTO gettitlemetadata = ComponentService.GetTitleOfHeaderFooterConfiguration();
                Assert.AreEqual(config.DataID, gettitlemetadata.DataID, "Title fetch data mismatch at assert 1");
                Assert.AreEqual(config.DisplayName, gettitlemetadata.DisplayName, "Title fetch data mismatch at assert 2");
                Assert.AreEqual(config.HeaderFooterConfig, gettitlemetadata.HeaderFooterConfig, "Title fetch data mismatch at assert 3");
                */
            Console.WriteLine("Header Adding in progress..");
            HeaderFooterConfigDTO rowconfig = new HeaderFooterConfigDTO();
            rowconfig.DataID = dataidheaderfooter("Automation (Y/N)");
            // rowconfig.Id = 0;
            rowconfig.DisplayName = "testrowtitle";
            rowconfig.HeaderFooterConfig = HeaderFooterConfigurationType.Header;
            rowconfig.IsWordwrapped = true;
            rowconfig.Order = 2;
            rowconfig.RowNumber = 5;
            rowconfig.SizeID = HeaderFooterColumnSize.Small;
            //Adding Row to Header
            ComponentService.AddUpdateHeaderFooterConfiguration(rowconfig);
            Console.WriteLine("Header Added");
            HeaderFooterConfigDTO[] getrowdetails = ComponentService.GetHeaderFooterConfiguration("2");
            bool flagheader = false;
            foreach (HeaderFooterConfigDTO datas in getrowdetails)
            {
                // Console.WriteLine("row"+datas.RowNumber);
                if (datas.RowNumber == rowconfig.RowNumber)
                {
                    flagheader = true;
                    Assert.AreEqual(rowconfig.DisplayName, datas.DisplayName, "Display name mismatch");
                    Assert.AreEqual(rowconfig.HeaderFooterConfig, datas.HeaderFooterConfig, "Type Header mismatch");
                    Assert.AreEqual(rowconfig.IsWordwrapped, datas.IsWordwrapped, "Wordwrap mismatch");
                    Assert.AreEqual(rowconfig.Order, datas.Order, "Order mismatch");
                    Assert.AreEqual(rowconfig.SizeID, datas.SizeID, "Font Size mismatch");
                }
                else
                {
                    continue;
                }
            }
            Assert.IsTrue(flagheader, "Header couldnot be found");
            Console.WriteLine("Header Verified");
            Console.WriteLine("Footer Adding in progress..");
            HeaderFooterConfigDTO rowconfigfooter = new HeaderFooterConfigDTO();
            rowconfigfooter.DataID = dataidheaderfooter("Automation (Y/N)");
            rowconfigfooter.DisplayName = "testrowtitlefooter";
            rowconfigfooter.HeaderFooterConfig = HeaderFooterConfigurationType.Footer;
            rowconfigfooter.IsWordwrapped = true;
            rowconfigfooter.Order = 2;
            rowconfigfooter.RowNumber = 5;
            rowconfigfooter.SizeID = HeaderFooterColumnSize.Small;
            //Adding Row to footer
            ComponentService.AddUpdateHeaderFooterConfiguration(rowconfigfooter);
            Console.WriteLine("Footer Added");
            HeaderFooterConfigDTO[] getrowdetailsfooter = ComponentService.GetHeaderFooterConfiguration("3");
            bool flagfooter = false;
            foreach (HeaderFooterConfigDTO datas in getrowdetailsfooter)
            {
                if (datas.RowNumber == rowconfigfooter.RowNumber)
                {
                    flagfooter = true;
                    Assert.AreEqual(rowconfigfooter.DisplayName, datas.DisplayName, "Display name mismatch");
                    Assert.AreEqual(rowconfigfooter.HeaderFooterConfig, datas.HeaderFooterConfig, "Type Header mismatch");
                    Assert.AreEqual(rowconfigfooter.IsWordwrapped, datas.IsWordwrapped, "Wordwrap mismatch");
                    Assert.AreEqual(rowconfigfooter.Order, datas.Order, "Order mismatch");
                    Assert.AreEqual(rowconfigfooter.SizeID, datas.SizeID, "Font Size mismatch");
                }
                else
                {
                    continue;
                }
            }
            Assert.IsTrue(flagfooter, "Footer couldnot be found");
            Console.WriteLine("Adding JOB 1");
            JobLightDTO job = new JobLightDTO();
            job.WellId = well.Id;
            job.WellName = well.Name;
            //job.BeginDate = DateTime.Today.AddDays(0);
            job.BeginDate = DateTime.Today.AddDays(0).ToUniversalTime();
            job.EndDate = DateTime.Today.AddDays(5).ToUniversalTime();
            //job.EndDate = DateTime.Today.AddDays(30);
            job.ActualCost = (decimal)100000.00;
            job.ActualJobDurationDays = (decimal)20.5;
            job.TotalCost = (decimal)150000.00;
            job.JobRemarks = "TestJobRemarks - Approve Job";
            job.JobOrigin = "TestJobOrigin ";
            job.AssemblyId = well.AssemblyId;
            //For Below fields User can select any value in the dropdown menu
            job.AFEId = JobAndEventService.GetAFEs().FirstOrDefault().Id;
            job.StatusId = JobAndEventService.GetJobStatuses().FirstOrDefault(j => j.Name == "Approved").Id;
            job.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault(x => x.JobType == "Artificial Lift Revision").id;
            job.BusinessOrganizationId = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            job.AccountRef = "1";
            //JobReasonId drop down selection is based on the JobTypeId
            job.JobReasonId = JobAndEventService.GetJobReasonsForJobType(job.JobTypeId.ToString()).FirstOrDefault().Id;
            job.JobDriverId = (int)JobAndEventService.GetPrimaryMotivationsForJob().FirstOrDefault().Id;
            //Add Job
            string addJob = JobAndEventService.AddJob(job);
            JobLightDTO getJob = JobAndEventService.GetJobById(addJob);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long eventId = JobAndEventService.AddEventForJobEventType(evt);

            Console.WriteLine("Adding JOB 2");
            JobLightDTO job2 = new JobLightDTO();
            job2.WellId = well.Id;
            job2.WellName = well.Name;
            //job.BeginDate = DateTime.Today.AddDays(0);
            job2.BeginDate = DateTime.Today.AddDays(6).ToUniversalTime();
            job2.EndDate = DateTime.Today.AddDays(11).ToUniversalTime();
            //job.EndDate = DateTime.Today.AddDays(30);
            job2.ActualCost = (decimal)100000.00;
            job2.ActualJobDurationDays = (decimal)20.5;
            job2.TotalCost = (decimal)150000.00;
            job2.JobRemarks = "TestJobRemarks - Approve Job";
            job2.JobOrigin = "TestJobOrigin ";
            job2.AssemblyId = well.AssemblyId;
            //For Below fields User can select any value in the dropdown menu
            job2.AFEId = JobAndEventService.GetAFEs().FirstOrDefault().Id;
            job2.StatusId = JobAndEventService.GetJobStatuses().FirstOrDefault(j => j.Name == "Approved").Id;
            job2.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault(x => x.JobType == "Artificial Lift Revision").id;
            job2.BusinessOrganizationId = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            job2.AccountRef = "1";

            //JobReasonId drop down selection is based on the JobTypeId
            job2.JobReasonId = JobAndEventService.GetJobReasonsForJobType(job.JobTypeId.ToString()).FirstOrDefault().Id;
            job2.JobDriverId = (int)JobAndEventService.GetPrimaryMotivationsForJob().FirstOrDefault().Id;
            //Add Job
            string addJob2 = JobAndEventService.AddJob(job2);
            JobLightDTO getJob2 = JobAndEventService.GetJobById(addJob2);
            JobEventTypeDTO jobEventTypes2 = JobAndEventService.GetJobEventTypes();
            EventDTO evt2 = SetEventDTO(getJob2, jobEventTypes2.WellBoreReport);
            long eventId2 = JobAndEventService.AddEventForJobEventType(evt2);

            var componentsforfirstjob = new List<Tuple<string, string[], double[], int[]>>
            {
               Tuple.Create("Tubing",new string[4]{"Tubing String","Tubing - OD  1.050","Hydril","C-110  1.050\" OD/1.50#  0.742\" ID  0.648\" Drift" },new double[2]{0.742,1.050 },new int[4]{50,1000,100,1}),
               Tuple.Create("Pump",new string[4]{"Tubing String", "Tubing Pump",  "_Generic Manufacturer", "Tubing Pump -   1.315\" Nominal" },new double[2]{ 0, 1.315},new int[4]{ 1, 100, 600,1 }),
               Tuple.Create("Borehole",new string[4]{"Borehole", "Wellbore Completion Detail (Perforations, etc.)",  "_Generic Manufacturer", "Slotted Casing/Liner - 4.500" }, new double[2] { 0, 4.500}, new int[4] { 1, 400, 900,2 }),
               Tuple.Create("Rod",new string[4]{"Rod String", "Rod",  "Alberta Oil Tools", "AOT-54  - 0.750   " }, new double[2] { 0, 0.750 }, new int[4] { 20, 600, 50,1 })
            };

            var componentsfornextjob = new List<Tuple<string, string[], double[], int[]>>
            {
               Tuple.Create("Anchor",new string[4]{"Tubing String", "Tubing Anchor/Catcher",  "_Generic Manufacturer", "Tubing Anchor/Catcher 1.900\"" }, new double[2] { 1.67, 2 }, new int[4] { 1, 200, 300,1 }),
               Tuple.Create("Prod Casing",new string[4]{"Production Casing", "Casing/Casing Liner OD  2.875",  "Hydril", "J-55  2.875\" OD/6.40#  2.441\" ID  2.347\" Drift" }, new double[2] { 2.441, 2.875 }, new int[4] { 2, 1000, 200,1}),
            };

            foreach (Tuple<string, string[], double[], int[]> comp in componentsforfirstjob)
            {
                Console.WriteLine("Adding component in Job 1...");
                AddComponentForWellConfig(well.Id.ToString(), well.AssemblyId, well.SubAssemblyId, addJob, eventId, comp.Item1, comp.Item2.ElementAt(0), comp.Item2.ElementAt(1), comp.Item2.ElementAt(2), comp.Item2.ElementAt(3), Convert.ToDecimal(comp.Item3.ElementAt(0)), Convert.ToDecimal(comp.Item3.ElementAt(1)), comp.Item4.ElementAt(0), comp.Item4.ElementAt(1), comp.Item4.ElementAt(2), comp.Item4.ElementAt(3));
            }
            foreach (Tuple<string, string[], double[], int[]> comp in componentsfornextjob)
            {
                Console.WriteLine("Adding component in Job 2...");
                AddComponentForWellConfig(well.Id.ToString(), well.AssemblyId, well.SubAssemblyId, addJob2, eventId2, comp.Item1, comp.Item2.ElementAt(0), comp.Item2.ElementAt(1), comp.Item2.ElementAt(2), comp.Item2.ElementAt(3), Convert.ToDecimal(comp.Item3.ElementAt(0)), Convert.ToDecimal(comp.Item3.ElementAt(1)), comp.Item4.ElementAt(0), comp.Item4.ElementAt(1), comp.Item4.ElementAt(2), comp.Item4.ElementAt(3));
            }
            ComponentVerticalSchematicInputDTO schematic = new ComponentVerticalSchematicInputDTO();
            schematic.AssemblyId = well.AssemblyId;
            schematic.SubAssemblyId = well.SubAssemblyId;
            Console.WriteLine("Fetching current wellbore diagram components...");
            ComponentVerticalSchematicDTO[] allcompincurrentwellborediagram = ComponentService.GetComponentVerticalSchematic(schematic);
            List<string> componentgroup = new List<string>();
            foreach (var comp in allcompincurrentwellborediagram)
            {
                componentgroup.Add(comp.ComponentGroup);
            }
            //List<string> distinctcomp = componentgroup.Distinct().ToList();
            List<bool> flags = new List<bool>(6) { false, false, false, false, false, false };
            int i = 0;
            Console.WriteLine("Comparing components...");

            foreach (var com in componentgroup)
            {
                Console.WriteLine("Found Component in current wellbore diagram->" + com);
                if (com.ToLower().Trim().Equals("tubing string"))
                {
                    flags[i] = true;
                }
                if (com.ToLower().Trim().Equals("rod string"))
                {
                    flags[i] = true;
                }
                if (com.ToLower().Trim().Equals("borehole"))
                {
                    flags[i] = true;
                }
                if (com.ToLower().Trim().Equals("production casing"))
                {
                    flags[i] = true;
                }

                i++;
            }
            Console.WriteLine("Checking syncronization...");
            foreach (var flag in flags)
            {
                if (flag == false)
                {
                    Assert.Fail("Component not in sync with added component");
                }
            }
            Console.WriteLine("Components are in sync, Current wellbore diagram in sync with Job Management");
            Console.WriteLine("Fetching headers from wellbore report");
            WellboreHeaderFooterInputDTO headerfetchdetails = new WellboreHeaderFooterInputDTO
            {
                AssemblyId = well.AssemblyId.ToString(),
                SubAssemblyId = well.SubAssemblyId.ToString(),
                TypeId = "2",
                WellId = wellid
            };
            Console.WriteLine("Assembly ID" + well.AssemblyId);
            Console.WriteLine("subAssembly ID" + well.SubAssemblyId);
            Console.WriteLine("well ID" + wellid);
            WellboreReportHeaderFooterRowDTO[] headersinwellborereport = ComponentService.GetHeaderFooterForWellBoreReport(headerfetchdetails);
            Console.WriteLine("Fetching footers from wellbore report");
            WellboreHeaderFooterInputDTO footerfetchdetails = new WellboreHeaderFooterInputDTO
            {
                AssemblyId = well.AssemblyId.ToString(),
                SubAssemblyId = well.SubAssemblyId.ToString(),
                TypeId = "3",
                WellId = wellid
            };
            WellboreReportHeaderFooterRowDTO[] footersinwellborereport = ComponentService.GetHeaderFooterForWellBoreReport(footerfetchdetails);
            Console.WriteLine("Verifying headers in wellbore report");
            bool flagcheck = false;
            int row = 1;
            foreach (var datas in headersinwellborereport)
            {
                if (row == rowconfig.RowNumber)
                {
                    foreach (var det in datas.HeaderFooterColumnDTO)
                    {
                        if (det.Label.Equals(rowconfig.DisplayName) && det.ColumnSize.Trim().ToLower().Equals("small") && det.ColumnOrder == rowconfig.Order && det.WordWrap.ToString().ToLower().Equals(rowconfig.IsWordwrapped.ToString().ToLower()) && det.Value.ToString().ToLower().Equals("no"))
                        {
                            flagcheck = true;
                        }
                    }
                }
                row++;
            }
            Assert.IsTrue(flagcheck, "Header value mismatch with wellbore header data");
            Console.WriteLine("Wellbore report header verified");
            Console.Write("Verifying Footer in wellbore report");
            bool flagcheckf = false;
            int rowf = 1;
            foreach (var datas in footersinwellborereport)
            {
                if (rowf == rowconfigfooter.RowNumber)
                {
                    foreach (var det in datas.HeaderFooterColumnDTO)
                    {
                        Console.WriteLine("value footer" + det.Label);
                        if (det.Label.ToString().Equals(rowconfigfooter.DisplayName) && det.ColumnSize.Trim().ToLower().Equals("small") && det.ColumnOrder == rowconfigfooter.Order && det.WordWrap.ToString().ToLower().Equals(rowconfigfooter.IsWordwrapped.ToString().ToLower()) && det.Value.ToString().ToLower().Equals("no"))
                        {
                            flagcheckf = true;
                        }
                    }
                }
                rowf++;
            }

            Assert.IsTrue(flagcheckf, "Footer value mismatch with wellbore footer data");
            Console.WriteLine("Wellbore report footer verified");
            removeallheaderfooter();
            Console.WriteLine("Test Complete with all verification Done");
        }

        /*
       * Below Test developed By Mintu Mukherjee
       */

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void VerfiyHeaderFooterAdditonalDatafield()
        {
            //*Total Depth *Plug Back Total Depth *Well depth reference
            string facilityId = s_isRunningInATS ? "RPOC_00001" : "RPOC_0001";
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
                    WellDepthDatumId = 11,
                })
            });
            Console.WriteLine("Well Created!");
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            string wellid = well.Id.ToString();
            _wellsToRemove.Add(well);

            Console.WriteLine("Header Adding in progress..");
            List<long> dataid = new List<long>() { dataidheaderfooter("Well Depth Datum"), dataidheaderfooter("Total Depth"), dataidheaderfooter("Plug Back Total Depth") };
            int row = 1;

            foreach (var data in dataid)
            {
                HeaderFooterConfigDTO header = new HeaderFooterConfigDTO();
                HeaderFooterConfigDTO footer = new HeaderFooterConfigDTO();
                //Datafield ID
                header.DataID = data;
                footer.DataID = data;
                // rowconfig.Id = 0;
                switch (row)
                {
                    case 1: header.DisplayName = "WellDepthDatum"; footer.DisplayName = "WellDepthDatum"; break;
                    case 2: header.DisplayName = "TotalDepth"; footer.DisplayName = "TotalDepth"; break;
                    case 3: header.DisplayName = "PBTotalDepth"; footer.DisplayName = "PBTotalDepth"; break;
                    default: header.DisplayName = "Test"; footer.DisplayName = "Test"; break;
                }
                header.HeaderFooterConfig = HeaderFooterConfigurationType.Header;
                header.IsWordwrapped = true;
                header.Order = 2;
                header.RowNumber = row;
                header.SizeID = HeaderFooterColumnSize.Small;

                footer.HeaderFooterConfig = HeaderFooterConfigurationType.Footer;
                footer.IsWordwrapped = true;
                footer.Order = 2;
                footer.RowNumber = row;
                footer.SizeID = HeaderFooterColumnSize.Small;
                //Adding Row to Header
                ComponentService.AddUpdateHeaderFooterConfiguration(header);
                Console.WriteLine("Header Added");
                //Adding Row to Footer
                ComponentService.AddUpdateHeaderFooterConfiguration(footer);
                Console.WriteLine("Footer Added");
                HeaderFooterConfigDTO[] getrowdetails = ComponentService.GetHeaderFooterConfiguration("2");
                HeaderFooterConfigDTO[] getrowdetailsfooter = ComponentService.GetHeaderFooterConfiguration("3");
                bool flagheader = false;
                foreach (HeaderFooterConfigDTO datas in getrowdetails)
                {
                    // Console.WriteLine("row"+datas.RowNumber);
                    if (datas.RowNumber == header.RowNumber)
                    {
                        flagheader = true;
                        Assert.AreEqual(header.DisplayName, datas.DisplayName, "Display name mismatch");
                        Assert.AreEqual(header.HeaderFooterConfig, datas.HeaderFooterConfig, "Type Header mismatch");
                        Assert.AreEqual(header.IsWordwrapped, datas.IsWordwrapped, "Wordwrap mismatch");
                        Assert.AreEqual(header.Order, datas.Order, "Order mismatch");
                        Assert.AreEqual(header.SizeID, datas.SizeID, "Font Size mismatch");
                    }
                    else
                    {
                        continue;
                    }
                }
                Assert.IsTrue(flagheader, "Header couldnot be found");
                bool flagfooter = false;
                foreach (HeaderFooterConfigDTO datas in getrowdetailsfooter)
                {
                    // Console.WriteLine("row"+datas.RowNumber);
                    if (datas.RowNumber == footer.RowNumber)
                    {
                        flagfooter = true;
                        Assert.AreEqual(footer.DisplayName, datas.DisplayName, "Display name mismatch");
                        Assert.AreEqual(footer.HeaderFooterConfig, datas.HeaderFooterConfig, "Type Footer mismatch");
                        Assert.AreEqual(footer.IsWordwrapped, datas.IsWordwrapped, "Wordwrap mismatch");
                        Assert.AreEqual(footer.Order, datas.Order, "Order mismatch");
                        Assert.AreEqual(footer.SizeID, datas.SizeID, "Font Size mismatch");
                    }
                    else
                    {
                        continue;
                    }
                }
                Assert.IsTrue(flagfooter, "Footer couldnot be found");
                row++;
            }
            Console.WriteLine("Header added Verified");
            Console.WriteLine("Footer added Verified");

            Console.WriteLine("Footer Adding in progress..");
            Console.WriteLine("Adding JOB 1");
            JobLightDTO job = new JobLightDTO();
            job.WellId = well.Id;
            job.WellName = well.Name;
            //job.BeginDate = DateTime.Today.AddDays(0);
            job.BeginDate = DateTime.Today.AddDays(0).ToUniversalTime();
            job.EndDate = DateTime.Today.AddDays(5).ToUniversalTime();
            //job.EndDate = DateTime.Today.AddDays(30);
            job.ActualCost = (decimal)100000.00;
            job.ActualJobDurationDays = (decimal)20.5;
            job.TotalCost = (decimal)150000.00;
            job.JobRemarks = "TestJobRemarks - Approve Job";
            job.JobOrigin = "TestJobOrigin ";
            job.AssemblyId = well.AssemblyId;
            //For Below fields User can select any value in the dropdown menu
            job.AFEId = JobAndEventService.GetAFEs().FirstOrDefault().Id;
            job.StatusId = JobAndEventService.GetJobStatuses().FirstOrDefault(j => j.Name == "Approved").Id;
            job.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault(x => x.JobType == "Artificial Lift Revision").id;
            job.BusinessOrganizationId = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            job.AccountRef = "1";
            //JobReasonId drop down selection is based on the JobTypeId
            job.JobReasonId = JobAndEventService.GetJobReasonsForJobType(job.JobTypeId.ToString()).FirstOrDefault().Id;
            job.JobDriverId = (int)JobAndEventService.GetPrimaryMotivationsForJob().FirstOrDefault().Id;
            //Add Job
            string addJob = JobAndEventService.AddJob(job);
            JobLightDTO getJob = JobAndEventService.GetJobById(addJob);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long eventId = JobAndEventService.AddEventForJobEventType(evt);

            var componentsforfirstjob = new List<Tuple<string, string[], double[], int[]>>
              {
                 Tuple.Create("Tubing",new string[4]{"Tubing String","Tubing - OD  1.050","Hydril","C-110  1.050\" OD/1.50#  0.742\" ID  0.648\" Drift" },new double[2]{0.742,1.050 },new int[4]{50,1000,100,1}),
                 Tuple.Create("Plug Back",new string[4]{"Tubing String", "Tubing Pump",  "_Generic Manufacturer", "Tubing Pump -   1.315\" Nominal" },new double[2]{ 0, 1.315},new int[4]{ 1, 100, 600,1 }),
                 Tuple.Create("Borehole",new string[4]{"Borehole", "Wellbore Completion Detail (Perforations, etc.)",  "_Generic Manufacturer", "Slotted Casing/Liner - 4.500" }, new double[2] { 0, 4.500}, new int[4] { 1, 400, 900,2 }),
                 Tuple.Create("Rod",new string[4]{"Rod String", "Rod",  "Alberta Oil Tools", "AOT-54  - 0.750   " }, new double[2] { 0, 0.750 }, new int[4] { 20, 600, 50,1 }),
                 Tuple.Create("Borehole",new string[4]{ "Borehole", "Plug - Mud",  "_Generic Manufacturer", "Plug - Mud" },new double[2]{ 0, 1.315},new int[4]{ 1, 100, 700,1 }),
                 Tuple.Create("Borehole",new string[4]{ "Borehole", "Plug - Cement",  "_Generic Manufacturer", "Plug - Cement" },new double[2]{ 0, 1.315},new int[4]{ 1, 100, 500,1 })
              };
            foreach (Tuple<string, string[], double[], int[]> comp in componentsforfirstjob)
            {
                Console.WriteLine("Adding component in Job 1...");
                AddComponentForWellConfig(well.Id.ToString(), well.AssemblyId, well.SubAssemblyId, addJob, eventId, comp.Item1, comp.Item2.ElementAt(0), comp.Item2.ElementAt(1), comp.Item2.ElementAt(2), comp.Item2.ElementAt(3), Convert.ToDecimal(comp.Item3.ElementAt(0)), Convert.ToDecimal(comp.Item3.ElementAt(1)), comp.Item4.ElementAt(0), comp.Item4.ElementAt(1), comp.Item4.ElementAt(2), comp.Item4.ElementAt(3));
            }

            Console.WriteLine("Fetching headers from wellbore report Job Management");
            WellboreHeaderFooterInputDTO headerfetchdetails = new WellboreHeaderFooterInputDTO
            {
                AssemblyId = well.AssemblyId.ToString(),
                SubAssemblyId = well.SubAssemblyId.ToString(),
                TypeId = "2",
                WellId = wellid,
                JobId = addJob,
                EventId = eventId.ToString(),
            };
            Console.WriteLine("Assembly ID" + well.AssemblyId);
            Console.WriteLine("subAssembly ID" + well.SubAssemblyId);
            Console.WriteLine("well ID" + wellid);
            WellboreReportHeaderFooterRowDTO[] headersinwellborereport = ComponentService.GetHeaderFooterForWellBoreReport(headerfetchdetails);
            Console.WriteLine("Fetching footers from wellbore report Job Management");
            WellboreHeaderFooterInputDTO footerfetchdetails = new WellboreHeaderFooterInputDTO
            {
                AssemblyId = well.AssemblyId.ToString(),
                SubAssemblyId = well.SubAssemblyId.ToString(),
                TypeId = "3",
                WellId = wellid,
                JobId = addJob,
                EventId = eventId.ToString(),
            };
            WellboreReportHeaderFooterRowDTO[] footersinwellborereport = ComponentService.GetHeaderFooterForWellBoreReport(footerfetchdetails);
            Console.WriteLine("Verifying headers in wellbore report Job Management");
            List<bool> flagcheck = new List<bool>() { false, false, false };
            int rowheader = 1;
            foreach (var datas in headersinwellborereport)
            {
                if (rowheader == 1)
                {
                    foreach (var det in datas.HeaderFooterColumnDTO)
                    {
                        Console.WriteLine("WellDepth Reference" + det.Value);
                        if (det.Label.Equals("WellDepthDatum") && det.ColumnSize.Trim().ToLower().Equals("small") && det.ColumnOrder == 2 && det.WordWrap == true && det.Value.ToString().Trim().ToLower().Equals("ground level"))
                        {
                            Console.WriteLine("Well Depth Datum header field matched");
                            flagcheck[0] = true;
                        }
                    }
                }
                if (rowheader == 2)
                {
                    foreach (var det in datas.HeaderFooterColumnDTO)
                    {
                        Console.WriteLine("Total Depth Value Found:" + det.Value);
                        if (det.Label.Equals("TotalDepth") && det.ColumnSize.Trim().ToLower().Equals("small") && det.ColumnOrder == 2 && det.WordWrap == true && det.Value.ToString().Trim().ToLower().Equals("1300.00"))
                        {
                            Console.WriteLine("Total Depth  header field matched");
                            flagcheck[1] = true;
                        }
                    }
                }
                if (rowheader == 3)
                {
                    foreach (var det in datas.HeaderFooterColumnDTO)
                    {
                        Console.WriteLine("PB Total Depth Value Found:" + det.Value);
                        if (det.Label.Equals("PBTotalDepth") && det.ColumnSize.Trim().ToLower().Equals("small") && det.ColumnOrder == 2 && det.WordWrap == true && det.Value.ToString().Trim().ToLower().Equals("500.00"))
                        {
                            Console.WriteLine("PBTotalDepth  header field matched");
                            flagcheck[2] = true;
                        }
                    }
                }
                rowheader++;
            }
            foreach (var flag in flagcheck)
            {
                Assert.IsTrue(flag, "Header value mismatch with Job Management wellbore header data");
            }
            Console.WriteLine("Verifying Footers in wellbore report Job Management");
            List<bool> flagcheckf = new List<bool>() { false, false, false };
            int rowfooter = 1;
            foreach (var datas in footersinwellborereport)
            {
                if (rowfooter == 1)
                {
                    foreach (var det in datas.HeaderFooterColumnDTO)
                    {
                        Console.WriteLine("WellDepth Reference" + det.Value);
                        if (det.Label.Equals("WellDepthDatum") && det.ColumnSize.Trim().ToLower().Equals("small") && det.ColumnOrder == 2 && det.WordWrap == true && det.Value.ToString().Trim().ToLower().Equals("ground level"))
                        {
                            Console.WriteLine("Well Depth Datum footer field matched");
                            flagcheckf[0] = true;
                        }
                    }
                }
                if (rowfooter == 2)
                {
                    foreach (var det in datas.HeaderFooterColumnDTO)
                    {
                        Console.WriteLine("Total Depth Value Found:" + det.Value);
                        if (det.Label.Equals("TotalDepth") && det.ColumnSize.Trim().ToLower().Equals("small") && det.ColumnOrder == 2 && det.WordWrap == true && det.Value.ToString().Trim().ToLower().Equals("1300.00"))
                        {
                            Console.WriteLine("Total Depth  footer field matched");
                            flagcheckf[1] = true;
                        }
                    }
                }
                if (rowfooter == 3)
                {
                    foreach (var det in datas.HeaderFooterColumnDTO)
                    {
                        Console.WriteLine("PB Total Depth Value Found:" + det.Value);
                        if (det.Label.Equals("PBTotalDepth") && det.ColumnSize.Trim().ToLower().Equals("small") && det.ColumnOrder == 2 && det.WordWrap == true && det.Value.ToString().Trim().ToLower().Equals("500.00"))
                        {
                            Console.WriteLine("PBTotalDepth  footer field matched");
                            flagcheckf[2] = true;
                        }
                    }
                }
                rowfooter++;
            }
            foreach (var flag in flagcheckf)
            {
                Assert.IsTrue(flag, "Footer value mismatch with Job Management wellbore footer data");
            }
            Console.WriteLine("Job Management Wellbore report header & Footer verified");

            //Verify header footer value in current wellbore diagram
            Console.WriteLine("Verifying Header & Footer value in current wellbore diagram...");
            headerfetchdetails.JobId = null;
            headerfetchdetails.EventId = null;
            footerfetchdetails.JobId = null;
            footerfetchdetails.EventId = null;
            WellboreReportHeaderFooterRowDTO[] headersincurrentwellborereport = ComponentService.GetHeaderFooterForWellBoreReport(headerfetchdetails);
            Console.WriteLine("Verifying headers in current wellbore report...");
            List<bool> flagcheckcw = new List<bool>() { false, false, false };
            int rowheadercw = 1;
            foreach (var datas in headersincurrentwellborereport)
            {
                if (rowheadercw == 1)
                {
                    foreach (var det in datas.HeaderFooterColumnDTO)
                    {
                        Console.WriteLine("WellDepth Reference" + det.Value);
                        if (det.Label.Equals("WellDepthDatum") && det.ColumnSize.Trim().ToLower().Equals("small") && det.ColumnOrder == 2 && det.WordWrap == true && det.Value.ToString().Trim().ToLower().Equals("ground level"))
                        {
                            Console.WriteLine("Well Depth Datum header field matched");
                            flagcheckcw[0] = true;
                        }
                    }
                }
                if (rowheadercw == 2)
                {
                    foreach (var det in datas.HeaderFooterColumnDTO)
                    {
                        Console.WriteLine("Total Depth Value Found:" + det.Value);
                        if (det.Label.Equals("TotalDepth") && det.ColumnSize.Trim().ToLower().Equals("small") && det.ColumnOrder == 2 && det.WordWrap == true && det.Value.ToString().Trim().ToLower().Equals("1300.00"))
                        {
                            Console.WriteLine("Total Depth  header field matched");
                            flagcheckcw[1] = true;
                        }
                    }
                }
                if (rowheadercw == 3)
                {
                    foreach (var det in datas.HeaderFooterColumnDTO)
                    {
                        Console.WriteLine("PB Total Depth Value Found:" + det.Value);
                        if (det.Label.Equals("PBTotalDepth") && det.ColumnSize.Trim().ToLower().Equals("small") && det.ColumnOrder == 2 && det.WordWrap == true && det.Value.ToString().Trim().ToLower().Equals("500.00"))
                        {
                            Console.WriteLine("PBTotalDepth  header field matched");
                            flagcheckcw[2] = true;
                        }
                    }
                }
                rowheadercw++;
            }
            foreach (var flag in flagcheckcw)
            {
                Assert.IsTrue(flag, "Header value mismatch with current wellbore header data");
            }

            WellboreReportHeaderFooterRowDTO[] footersincurrentwellborereport = ComponentService.GetHeaderFooterForWellBoreReport(footerfetchdetails);
            Console.WriteLine("Verifying footers in current wellbore report...");
            List<bool> flagcheckcwf = new List<bool>() { false, false, false };
            int rowheadercwf = 1;
            foreach (var datas in footersincurrentwellborereport)
            {
                if (rowheadercwf == 1)
                {
                    foreach (var det in datas.HeaderFooterColumnDTO)
                    {
                        Console.WriteLine("WellDepth Reference" + det.Value);
                        if (det.Label.Equals("WellDepthDatum") && det.ColumnSize.Trim().ToLower().Equals("small") && det.ColumnOrder == 2 && det.WordWrap == true && det.Value.ToString().Trim().ToLower().Equals("ground level"))
                        {
                            Console.WriteLine("Well Depth Datum footer field matched");
                            flagcheckcwf[0] = true;
                        }
                    }
                }
                if (rowheadercwf == 2)
                {
                    foreach (var det in datas.HeaderFooterColumnDTO)
                    {
                        Console.WriteLine("Total Depth Value Found:" + det.Value);
                        if (det.Label.Equals("TotalDepth") && det.ColumnSize.Trim().ToLower().Equals("small") && det.ColumnOrder == 2 && det.WordWrap == true && det.Value.ToString().Trim().ToLower().Equals("1300.00"))
                        {
                            Console.WriteLine("Total Depth  footer field matched");
                            flagcheckcwf[1] = true;
                        }
                    }
                }
                if (rowheadercwf == 3)
                {
                    foreach (var det in datas.HeaderFooterColumnDTO)
                    {
                        Console.WriteLine("PB Total Depth Value Found:" + det.Value);
                        if (det.Label.Equals("PBTotalDepth") && det.ColumnSize.Trim().ToLower().Equals("small") && det.ColumnOrder == 2 && det.WordWrap == true && det.Value.ToString().Trim().ToLower().Equals("500.00"))
                        {
                            Console.WriteLine("PBTotalDepth  footer field matched");
                            flagcheckcwf[2] = true;
                        }
                    }
                }
                rowheadercwf++;
            }
            foreach (var flag in flagcheckcwf)
            {
                Assert.IsTrue(flag, "Footer value mismatch with current wellbore footer data");
            }
            Console.WriteLine("Header footer in current wellbore verified successfully");
            removeallheaderfooter();
            Console.WriteLine("End of Test");
        }

        public bool removeallheaderfooter()
        {
            try
            {
                Console.WriteLine("Removing added header");
                HeaderFooterConfigDTO[] allheaders = ComponentService.GetHeaderFooterConfiguration("2");
                foreach (var data in allheaders)
                {
                    ComponentService.RemoveHeaderFooterConfiguration(data.Id.ToString());
                }
                Console.WriteLine("Removing added footer");
                HeaderFooterConfigDTO[] allfooters = ComponentService.GetHeaderFooterConfiguration("3");
                foreach (var data in allfooters)
                {
                    ComponentService.RemoveHeaderFooterConfiguration(data.Id.ToString());
                }
                Console.WriteLine("HEader Footers removed");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong, HEader Footers could not be removed, Details->" + e);
                return false;
            }
        }

        public long dataidheaderfooter(string dataname)
        {
            HeaderFooterDataDTO[] datafields = ComponentService.GetDataForHeaderFooter();
            foreach (var d in datafields)
            {
                if (d.DataName.Trim().ToLower() == dataname.Trim().ToLower())
                {
                    return d.DataID;
                }
            }
            return 0;
        }

        /// <summary>
        /// Verifying the data on Current wellbore grid
        /// </summary>
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void CurrentWellboreGridTest()
        {
            //Creating a well
            WellDTO well = AddWell("RPOC_");

            //Creating tuple which will hold Component name & Wellbore perf status
            //Tuple structure :- Comp grouping,Part type name,comp name, wellbore perf status,Remarks
            var components = new List<Tuple<string, string, string, int, string>>
            {
                Tuple.Create("Borehole","Gravel Pack Screen","Borehole Gravel component",7,"Component Borehole added"),
                Tuple.Create("Drilling Liner","Plug - Cement","Drilling Liner component",7,"Component Drilling Liner added"),
                Tuple.Create("Rod String", "Polished Rod","Rod String Component",7,"Component Rod String added"),
                Tuple.Create("Borehole", "Wellbore Completion Detail (Perforations, etc.)","Perf Details Name",1,"Perf Details Remarks")
            };

            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();
            string subassemblyId = well.SubAssemblyId.ToString();

            //Creating a Job
            string jobId = AddJob("Approved");

            //Adding Wellbore tab to Job
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Wellbore report tab not added successfully");

            //Adding Wellbore components.
            foreach (Tuple<string, string, string, int, string> comp in components)
            {
                AddComponent(wellId, assemblyId, subassemblyId, jobId, comp.Item1, comp.Item2, comp.Item3, comp.Item5, comp.Item4);
            }

            WellboreGridDTO[] wellboreComponents = JobAndEventService.GetCurrentWellboreGrid(wellId);

            WellboreGridGroupDTO[] retrievedComponents = wellboreComponents[0].WellboreGridGroup;

            //verify the number of records
            Assert.AreEqual(components.Count, retrievedComponents.Count(), "The total number of records displayed in Current wellbore grid is not correct");
        }

        /// <summary>
        /// Verifying the data on Drilling report grid
        /// </summary>
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void VerifyDrillingReportGrid()
        {
            //Creating a well
            WellDTO well = AddWell("RPOC_");

            //Creating tuple which will hold Component details
            //Tuple structure :- Comp grouping,Part type name,comp name, wellbore perf status,Remarks
            var components = new List<Tuple<string, string, string, int, string>>
            {
                Tuple.Create("Borehole","Gravel Pack Screen","Borehole Gravel component",7,"Component Borehole added"),
                Tuple.Create("Drilling Liner","Plug - Cement","Drilling Liner component",7,"Component Drilling Liner added"),
                Tuple.Create("Rod String", "Polished Rod","Rod String Component",7,"Component Rod String added"),
                Tuple.Create("Borehole", "Wellbore Hole","Borehole component",7,"Wellbore Hole added")
            };

            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();
            string subassemblyId = well.SubAssemblyId.ToString();

            //Creating a Job
            string jobId = AddJob("Approved");

            //Adding Drilling tab to Job
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.DrillingReport);
            evt.BeginTime = DateTime.UtcNow;

            long check = JobAndEventService.AddEventForJobEventType(evt);

            Assert.IsTrue(check > 0, "Drilling report tab not added successfully");

            //Adding Drilling report components.
            foreach (Tuple<string, string, string, int, string> comp in components)
            {
                AddComponent_DrillingReport(wellId, assemblyId, subassemblyId, jobId, comp.Item1, comp.Item2, comp.Item3, comp.Item5, comp.Item4);
            }

            WellboreGridDTO[] wellboreComponents = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());

            WellboreGridGroupDTO[] retrievedComponents = wellboreComponents[0].WellboreGridGroup;

            //verify the number of records
            Assert.AreEqual(components.Count, retrievedComponents.Count(), "The total number of records displayed in Drilling report is not correct");

            int i = 0;
            //Verifying the component grouping and part types for respective components
            foreach (WellboreGridGroupDTO component in retrievedComponents)
            {
                string retrievedCompname = component.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required").Fields.FirstOrDefault(x => x.Title == "Component Name").DataValue.ToString();

                foreach (Tuple<string, string, string, int, string> comp in components)
                {
                    if (retrievedCompname.Equals(comp.Item3))
                    {
                        Assert.AreEqual(comp.Item1.ToString(), retrievedComponents[i].ComponentMetadata[0].ComponentGroupingName, "The Component Group doesnot match");
                        Assert.AreEqual(comp.Item2.ToString(), retrievedComponents[i].ComponentMetadata[0].PartType, "The Part Type doesnot match");

                        break;
                    }
                    else
                    {
                        Trace.WriteLine(retrievedCompname + " doesnot match. Expected value: " + comp.Item3);
                    }
                }
                i = i + 1;
            }
        }

        ///<Summary>
        ///Verification of Asset Colum data present in screen like Enter Tour Sheet, Job Status View, Group Configuration
        ///|| Developed by Mintu Mukherjee
        ///</Summary>
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void VerfiyAssetColumn()
        {
            AssetDTO assetdet = new AssetDTO();
            assetdet.Name = "TestAsset5";
            assetdet.Description = "Description";
            SurfaceNetworkService.AddAsset(assetdet);
            Console.WriteLine("Asset Created");
            assetdet = SurfaceNetworkService.GetAllAssets().FirstOrDefault(a => a.Name == assetdet.Name);
            List<AssetDTO> asset = new List<AssetDTO>() { assetdet };
            UserDTO userdet = new UserDTO();
            UserDTO[] ud = AdministrationService.GetUsers();
            RoleDTO role = AdministrationService.GetRoles().FirstOrDefault(r => r.Name == "Administrator");
            SurfaceNetworkService.GetAllAssets();
            long uid = 0;
            _assetsToRemove.Add(assetdet);
            foreach (var u in ud)
            {
                if (u.Name.ToLower().Trim().Equals(Environment.UserDomainName.ToLower().Trim() + "\\" + Environment.UserName.ToLower().Trim()))
                {
                    Console.WriteLine("ID->" + u.Id);
                    uid = u.Id;
                }
            }
            if (uid == 0)
            {
                Assert.Fail("Domain id not found in database");
            }
            userdet.Id = uid;
            userdet.Roles = new List<RoleDTO>() { role };
            userdet.Assets = asset;
            userdet.Name = Environment.UserDomainName + "\\" + Environment.UserName;
            AdministrationService.UpdateUser(userdet);
            Console.WriteLine("Asset assigned to current user");
            string facilityId = s_isRunningInATS ? "RPOC_00001" : "RPOC_0001";
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
                    WellDepthDatumId = 11,
                    AssetId = assetdet.Id
                })
            });
            Console.WriteLine("Well Created with Asset");
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            string wellid = well.Id.ToString();
            _wellsToRemove.Add(well);
            Console.WriteLine("Checking Asset column for well config screen RRL...");
            WellFilterDTO filter = new WellFilterDTO();

            UnitsValuesCollectionDTO<WellConfigUnitDTO, WellConfigDTO> groupdata = WellConfigurationService.GetWellGroupConfigurationUoM(filter, WellTypeId.RRL.ToString(), false.ToString());
            WellConfigDTO values = groupdata.Values.FirstOrDefault(a => a.Well.Name == wellConfig.Well.Name);
            if (values.Well.AssetId == wellConfig.Well.AssetId)
            {
                Console.WriteLine("Asset is matching with created one");
            }
            else
            {
                Assert.Fail("Asset is not matching with created one");
            }
            Console.WriteLine("Enter Tour Sheet Verifying");
            GridSettingDTO grid = new GridSettingDTO();
            grid.CurrentPage = 1;
            grid.NumberOfPages = 1;
            grid.PageSize = 20;
            EntityGridSettingDTO setting = new EntityGridSettingDTO();
            setting.EntityName = "r_JobStatus";
            setting.GridSetting = grid;
            DBEntityDTO obj = DBEntityService.GetTableData(setting);
            Console.WriteLine("Getting all available job status in reference Data");
            List<MetaDataDTO[]> metaslist = new List<MetaDataDTO[]>();
            for (int j = 1; j <= 21; j++)
            {
                metaslist.Add(DBEntityService.GetRefereneceMetaDataEntityForUpdate(setting.EntityName, j.ToString()));
                //  metaslist.Add(DBEntityService.GetRefereneceMetaDataEntityForUpdate(setting.EntityName, "18"));
            }
            Console.WriteLine("Checking tour sheet visibility property in job status reference data config..");
            foreach (var metas in metaslist)
            {
                bool flag1 = false;
                foreach (MetaDataDTO prop in metas)
                {
                    if (prop.ColumnName.ToString().ToLower().Trim().Equals("mgstoursheetvisible"))
                    {
                        // Console.WriteLine(prop.Visible_InAttribute);
                        flag1 = true;
                        prop.DataValue = true;
                        Console.WriteLine("Updating Reference data, Activating Visible in Tour Sheet property for a Job status...");
                        bool resp = DBEntityService.UpdateReferenceData(metas);
                        Assert.IsTrue(resp, "Update reference data cannot be done");
                        Console.WriteLine("Reference data updated successfully");
                    }
                }
                Assert.IsTrue(flag1, "Tour sheet visible property could not be found!, Terminating next steps...");
            }
            Console.WriteLine("Adding JOBs...");
            string approvejobId = AddJobNew("Approved");
            string plannedjobId = AddJobNew("Planned");
            string canceljobId = AddJobNew("Completed");
            Console.WriteLine("Fetching Tour Sheet Start Page populated JOBS...");
            JobLightDTO[] jobsin = JobAndEventService.GetTourSheetJobs(null);

            int i = 0;
            foreach (JobLightDTO job in jobsin)
            {
                if (job.JobId == Convert.ToInt64(approvejobId))
                {
                    Assert.AreEqual(assetdet.Name.ToLower(), job.AssetName.ToLower());

                    i = 1;
                }
            }
            if (i == 0)
            {
                Assert.Fail("Job Not found in tour sheet grid");
            }

            Console.WriteLine("Tour SHeet asset data verified");
            Console.WriteLine("Verifying Job STatus data ");
            Console.WriteLine("Checking for Planned jobs");
            JobStatusViewDTO prosjob = JobAndEventService.GetJobsByCategory(filter, JobStatusCategory.Prospective.ToString()).FirstOrDefault(a => a.JobId.ToString() == plannedjobId);
            if (prosjob.AssetName.Equals(assetdet.Name))
            {
                Console.WriteLine("Asset data verified for prospective jobs");
            }
            else
            {
                Assert.Fail("Asset data not matching with prospective job data");
            }
            Console.WriteLine("Checking for Ready jobs");
            JobStatusViewDTO readyjob = JobAndEventService.GetJobsByCategory(filter, JobStatusCategory.Ready.ToString()).FirstOrDefault(a => a.JobId.ToString() == approvejobId);
            if (readyjob.AssetName.Equals(assetdet.Name))
            {
                Console.WriteLine("Asset data verified for Ready jobs");
            }
            else
            {
                Assert.Fail("Asset data not matching with Ready job data");
            }
            Console.WriteLine("Checking for Concluded jobs");
            JobStatusViewDTO conjob = JobAndEventService.GetJobsByCategory(filter, JobStatusCategory.Concluded.ToString()).FirstOrDefault(a => a.JobId.ToString() == canceljobId);
            if (conjob.AssetName.Equals(assetdet.Name))
            {
                Console.WriteLine("Asset data verified for Con  jobs");
            }
            else
            {
                Assert.Fail("Asset data not matching with Con job data");
            }
            Console.WriteLine("Asset Column verification for Morning Report");
            Console.WriteLine("Adding Event...");
            CreateEventForCleanFillEvent(plannedjobId);
            Console.WriteLine("Verifying Morning Report");
            MorningReportVDTO mordet = new MorningReportVDTO();
            mordet.StartDate = DateTime.Now.ToLocalTime();
            mordet.WellId = wellConfig.Well.Id;
            MorningReportVDTO gmordet = JobAndEventService.GetMorningReport(mordet);
            var data = gmordet.MorningReports.FirstOrDefault(x => x.JobId.ToString().Trim() == plannedjobId);

            if (data.AssetName == assetdet.Name)
            {
                Console.WriteLine("Asset Id match with morning report data");
            }
            else
            {
                Assert.Fail("Asset not match with morning report data");
            }
            Console.WriteLine("Morning Report verification DOne");
            Console.WriteLine("Deleting Wells..");
            Console.WriteLine("Started Checking if Asset data exist at Job Manangement General grid..");
            JobTinyDTO[] alljobs = JobAndEventService.GetJobsByAssemblyId(well.AssemblyId.ToString());
            foreach (var job in alljobs)
            {
                Assert.AreEqual(assetdet.Name, job.AssetName);
            }
            Console.WriteLine("Asset Column verified for Job Management general grid..");
            WellConfigurationService.RemoveWellConfig(wellConfig.Well.Id.ToString());
            userdet.Assets.Clear();
            AdministrationService.UpdateUser(userdet);
            SurfaceNetworkService.RemoveAsset(assetdet.Id.ToString());
            Console.WriteLine("Asset Removed successfully");
            Console.WriteLine("==End of Test==");
        }

        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void GetSubAssembliesByAssemblyId()
        {
            //Add a well
            string wellId = AddWell("RPOC_").Id.ToString();
            AssemblyDTO assembly = WellboreComponentService.GetAssemblyByWellId(wellId);
            string jobId = AddJob("Approved");
            //Adding Wellbore tab to Job
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //Get Sub assembly by Assembly id
            SubAssemblyDTO getSubAssemblies = WellboreComponentService.GetSubAssembliesByAssemblyId(getJob.AssemblyId.ToString()).FirstOrDefault();
            Assert.AreEqual(getJob.AssemblyId.ToString(), getSubAssemblies.AssemblyId.ToString(), "Failed to get same assembly id.");
            Assert.AreEqual("Primary Wellbore", getSubAssemblies.SubAssemblyDesc.ToString());

        }
        /// <summary>
        /// Verify the wellbore grid record "Perforation status" is editable and to check the perforation status column value count 
        /// </summary>
        [TestCategory(TestCategories.JobAndEventServiceTests), TestMethod]
        public void VerifyWellboreGridEditable()
        {
            //Creating a well
            WellDTO well = AddWell("RPOC_");

            //Creating tuple which will hold Component details
            //Tuple structure :- Comp grouping,Part type name,comp name, wellbore perf status,Remarks

            var components = new List<Tuple<string, string, string, int, string>>
            {
                Tuple.Create("Tubing String","Perforation Hole/Slot Detail","Perforation Hole/Slot Detail",2,"Component with Status field"),
                Tuple.Create("Borehole","Gravel Pack","Gravel Pack",7,"Component Borehole")
            };

            string wellId = well.Id.ToString();
            string assemblyId = well.AssemblyId.ToString();
            string subassemblyId = well.SubAssemblyId.ToString();

            //Creating a Job
            string jobId = AddJob("Approved");

            //Adding Wellbore tab to Job
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Wellbore report tab not added successfully");

            //verify status count
            ControlIdTextDTO[] pfMetaData = JobAndEventService.GetPerforationStatus();

            //assert status count
            Assert.IsTrue(pfMetaData.Count() > 0, "The perforation status values are present");

            //Adding Wellbore components.Part type name "Wellbore Completion Detail (Perforations, etc.)" has Well Perforation Status
            foreach (Tuple<string, string, string, int, string> comp in components)
            {
                AddComponent(wellId, assemblyId, subassemblyId, jobId, comp.Item1, comp.Item2, comp.Item3, comp.Item5, comp.Item4);

            }

            WellboreGridDTO[] wellboreComponents = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, check.ToString());

            //Verifying that correct status is being retrieved for respective components
            bool flag;

            foreach (var subAssemblyGroup in wellboreComponents)
            {
                foreach (var componentsInSubAssembly in subAssemblyGroup.WellboreGridGroup)
                {
                    foreach (var componentCategory in componentsInSubAssembly.ComponentMetadata)
                    {
                        MetaDataDTO status = componentCategory.Fields.FirstOrDefault(x => x.ColumnName.ToUpper() == "APSFK_R_WELLPERFORATIONSTATUS");

                        if (componentCategory.PartType == "Perforation Hole/Slot Detail")
                        {
                            if (componentCategory.Fields.Any(x => x.ColumnName.ToUpper() == "APSFK_R_WELLPERFORATIONSTATUS"))
                            {
                                flag = status.IsGridCellEditable;
                                Assert.IsTrue(flag);
                                Trace.WriteLine("The part-type " + componentCategory.PartType + " contains Perforation status");
                                Trace.WriteLine("The part-type perforation status column is editable");
                            }
                        }
                        else
                        {
                            Assert.IsNull(status);
                            Trace.WriteLine("The part-type " + componentCategory.PartType + " doesnot contain Perforation status");
                        }
                    }
                }
            }
        }
    }
}


