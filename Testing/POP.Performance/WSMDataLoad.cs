using System;
using System.Collections.Generic;
using System.Linq;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;

namespace POP.Performance
{
    public class WSMDataLoad : APIPerfTestBase
    {
        public ModelConfigDTO ReturnBlankModel()
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

        public void AddDowntimeRecordsforAllWells(int welStart, int welEnd)
        {
            Authenticate();
            List<WellDTO> allWells = new List<WellDTO>();
            DownTimeStatusCodeDTO[] downCodes = SurveillanceService.GetDownTimeStatusCodes();
            for (int i = welStart; i <= welEnd; i++)
            {
                WellDTO singleWell = WellService.GetWell(i.ToString());
                if (singleWell != null)
                    allWells.Add(singleWell);
            }
            foreach (WellDTO well in allWells)
            {
                try
                {
                    int days = 0;
                    foreach (DownTimeStatusCodeDTO downCode in downCodes)
                    {
                        days = days + 1;
                        var downTimeAdd = new WellDowntimeAdditionDTO();
                        var downTime = downTimeAdd.Downtime = new WellDowntimeDTO();
                        downTimeAdd.WellIds = new long[] { well.Id };
                        downTime.CapturedDateTime = well.CommissionDate.Value.AddDays(days).ToUniversalTime();
                        downTime.OutOfServiceCode = downCode.DownTimeStatusId;
                        long[] downtimeId = SurveillanceService.AddDownTimeForWell(downTimeAdd);
                    }
                }
                catch
                {
                    WriteLogFile(string.Format("Failed to Add Downtime records for Well : " + well.Name));
                }
                Console.WriteLine("Added down time records for Well : " + well.Name);
            }
        }

        public void AddWellWithAssemblyandSubassemblies(string BaseFacTag, int welStart, int welEnd, string Domain, string Site, string Service, string welltype)
        {
            Authenticate();
            ReferenceTableItemDTO[] wellDepthDatums = WellConfigurationService.GetReferenceTableItems("r_WellDepthDatum", "false");
            ReferenceTableItemDTO wellDepthDatum = wellDepthDatums.FirstOrDefault(t => t.ConstantId == "GROUND_LEVEL") ?? wellDepthDatums.FirstOrDefault();
            WellTypeId type = WellTypeId.Unknown;
            switch (welltype)
            {
                case "NFW":
                    type = WellTypeId.NF;
                    break;

                case "ESP":
                    type = WellTypeId.ESP;
                    break;

                case "GL":
                    type = WellTypeId.GLift;
                    break;

                case "WInj":
                    type = WellTypeId.WInj;
                    break;

                case "GInj":
                    type = WellTypeId.GInj;
                    break;

                case "RRL":
                    type = WellTypeId.RRL;
                    break;

                default:
                    WriteLogFile(string.Format("Invalid Well type"));
                    break;
            }
            for (int i = welStart; i <= welEnd; i++)
            {
                SubAssemblyDTO SubAssembly01 = new SubAssemblyDTO
                {
                    SubAssembly_ParentId = null,
                    SubAssemblyType = (int)SubAssemblyTypeId.Primary_Wellbore,
                    SAId = BaseFacTag + "-01-" + i.ToString(FacilityPadding),
                    DepthCorrectionFactor = 25,
                    ActionPerformed = CRUDOperationTypes.Add,
                };
                SubAssemblyDTO SubAssembly02 = new SubAssemblyDTO
                {
                    SubAssembly_ParentId = (int)SubAssemblyTypeId.Primary_Wellbore,
                    SubAssemblyType = (int)SubAssemblyTypeId.Sidetrack_Lateral_1,
                    SAId = BaseFacTag + "-02-" + i.ToString(FacilityPadding),
                    DepthCorrectionFactor = 0,
                    ActionPerformed = CRUDOperationTypes.Add,
                };
                SubAssemblyDTO SubAssembly03 = new SubAssemblyDTO
                {
                    SubAssembly_ParentId = (int)SubAssemblyTypeId.Sidetrack_Lateral_1,
                    SubAssemblyType = (int)SubAssemblyTypeId.Sidetrack_Lateral_2,
                    SAId = BaseFacTag + "-03-" + i.ToString(FacilityPadding),
                    DepthCorrectionFactor = 0,
                    ActionPerformed = CRUDOperationTypes.Add,
                };
                List<SubAssemblyDTO> subAssemblies = new List<SubAssemblyDTO>();
                subAssemblies.Add(SubAssembly01);
                subAssemblies.Add(SubAssembly02);
                subAssemblies.Add(SubAssembly03);
                switch (type)
                {
                    case WellTypeId.RRL:
                        {
                            WellDTO well = new WellDTO()
                            {
                                Name = "WSM" + "-" + i.ToString(FacilityPadding),
                                FacilityId = BaseFacTag + "_" + i.ToString(FacilityPadding),
                                DataConnection = new DataConnectionDTO { ProductionDomain = Domain, Site = Site, Service = Service },
                                CommissionDate = DateTime.Today.AddYears(-Convert.ToInt32(YearsOfData)),
                                AssemblyAPI = BaseFacTag + "-" + i.ToString(FacilityPadding),
                                SubAssemblyAPI = BaseFacTag + "-" + i.ToString(FacilityPadding),
                                IntervalAPI = BaseFacTag + "-" + i.ToString(FacilityPadding),
                                WellType = type,
                                WellDepthDatumId = wellDepthDatum?.Id,
                                WellDepthDatumElevation = 20,
                                SubAssemblies = subAssemblies.ToArray(),
                            };
                            WellConfigDTO wellConfig = new WellConfigDTO();
                            wellConfig.Well = well;
                            wellConfig.ModelConfig = ReturnBlankModel();
                            //Add Well
                            WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfig);
                            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
                            //Add Well Tests
                            for (int j = 0; j < Convert.ToInt32(WellTestCount); j++)
                            {
                                //add new wellTestData
                                var testDataDTO = new WellTestDTO
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
                        break;

                    default:
                        WriteLogFile(string.Format("Invalid Well Type"));
                        break;
                }
            }
        }

        public string AddJob(WellDTO well, long jobStatusId, int i, long jobTypeId, long AFEId, long BusinessOrgId, PrimaryMotivationForJobId jobMotiveId)
        {
            JobLightDTO job = new JobLightDTO();
            job.WellId = well.Id;
            job.WellName = well.Name;
            job.BeginDate = well.CommissionDate.Value.AddDays(i * 34);
            job.EndDate = well.CommissionDate.Value.AddDays((i * 34) + 30);
            job.ActualCost = (decimal)100000.00;
            job.ActualJobDurationDays = (decimal)20.5;
            job.TotalCost = (decimal)150000.00;
            job.JobRemarks = "TestJobRemarks " + DateTime.Today.AddDays(-i * 15);
            job.JobOrigin = "TestJobOrigin ";
            job.AssemblyId = well.AssemblyId;
            job.AFEId = AFEId;
            job.StatusId = jobStatusId;
            job.JobTypeId = jobTypeId;
            job.BusinessOrganizationId = BusinessOrgId;
            job.AccountRef = "1";
            job.JobReasonId = JobAndEventService.GetJobReasonsForJobType(job.JobTypeId.ToString()).OrderBy(x => random.Next()).FirstOrDefault().Id;
            job.JobDriverId = (long)jobMotiveId;
            //Add Job
            string addJob = JobAndEventService.AddJob(job);

            return addJob;
        }

        public EventDTO SetEventDTO(JobLightDTO getJob, long eId, long catalogId, long truckId)
        {
            EventDTO e = new EventDTO();
            e.EndTime = getJob.BeginDate.ToUniversalTime().AddDays(5);
            e.EventTypeId = eId;
            e.PK_AFE = getJob.AFEId;
            e.PK_Assembly = getJob.AssemblyId;
            e.PK_BusinessOrganization = getJob.BusinessOrganizationId == null ? 0 : (long)getJob.BusinessOrganizationId;
            e.PK_CatalogItem = catalogId;
            e.PK_Job = getJob.JobId;
            e.PK_TruckUnit = truckId;
            e.TotalCost = getJob.TotalCost;

            return e;
        }

        public void AddJobsforAllWells(long wellStart, long wellEnd)
        {
            Authenticate();
            JobLightDTO[] allJobs = null;
            JobStatusDTO[] js = JobAndEventService.GetJobStatuses();
            JobTypeDTO[] jobTypes = JobAndEventService.GetJobTypes();
            JobEventTypeDTO jobEventType = JobAndEventService.GetJobEventTypes();
            IList<BusinessOrganizationDTO> businessOrgs = JobAndEventService.GetCatalogItemGroupData().Vendors;
            long AFEId = JobAndEventService.GetAFEs().FirstOrDefault().Id;
            PrimaryMotivationForJobDTO[] primaryMotives = JobAndEventService.GetPrimaryMotivationsForJob();
            CatalogItemGroupDTO catalogDTO = JobAndEventService.GetCatalogItemGroupData();
            IList<CatalogItemDTO> CatalogItems = catalogDTO.CatalogItems;
            IList<TruckUnitDTO> TruckUnits = catalogDTO.TruckUnits;
            long ppScenarioId = JobAndEventService.GetProductPriceScenarios("9").FirstOrDefault(x => x.Id == 60).Id;
            long ppScenarioWaterId = JobAndEventService.GetProductPriceScenarios("3").FirstOrDefault(x => x.Id == 1162).Id;
            long ppScenarioGasId = JobAndEventService.GetProductPriceScenarios("2").FirstOrDefault(x => x.Id == 5).Id;
            long txRateId = JobAndEventService.GetStateTax().FirstOrDefault(x => x.Id == 2).Id;
            long oilProductTypeId = JobAndEventService.GetCommonProductTypes()?.OilProductTypeId ?? 0;

            for (long i = wellStart; i <= wellEnd; i++)
            {
                WellDTO well = WellService.GetWell(i.ToString());
                if (well != null)
                {
                    //Get Job Count if already exist per well as JobsPerWell count
                    allJobs = JobAndEventService.GetJobsByWell(well.Id.ToString());
                }
                if (well == null || Convert.ToInt32(allJobs.Count()) >= Convert.ToInt32(JobsPerWell))
                    continue;

                int TotalRemainingJobs = Convert.ToInt32(JobsPerWell) - Convert.ToInt32(allJobs.Count());

                for (int j = 1; j <= Convert.ToInt32(TotalRemainingJobs); j++)
                {
                    try
                    {
                        string addJob = AddJob(well, js.OrderBy(x => random.Next()).FirstOrDefault().Id, j, jobTypes.OrderBy(x => random.Next()).FirstOrDefault().id, AFEId, businessOrgs.OrderBy(x => random.Next()).FirstOrDefault().Id, primaryMotives.OrderBy(x => random.Next()).FirstOrDefault().Id);
                        JobLightDTO getJob = JobAndEventService.GetJobById(addJob);
                        EventDTO JobPlan = SetEventDTO(getJob, jobEventType.JobPlan, CatalogItems.OrderBy(x => random.Next()).FirstOrDefault().CatalogItemId, TruckUnits.OrderBy(x => random.Next()).FirstOrDefault().TruckUnitId);
                        EventDTO EconomicAnalysisPlan = SetEventDTO(getJob, jobEventType.EconomicAnalysisPlan, CatalogItems.OrderBy(x => random.Next()).FirstOrDefault().CatalogItemId, TruckUnits.OrderBy(x => random.Next()).FirstOrDefault().TruckUnitId);
                        EventDTO JobCostDetailReport = SetEventDTO(getJob, jobEventType.JobCostDetailReport, CatalogItems.OrderBy(x => random.Next()).FirstOrDefault().CatalogItemId, TruckUnits.OrderBy(x => random.Next()).FirstOrDefault().TruckUnitId);
                        EventDTO FailureReport = SetEventDTO(getJob, jobEventType.FailureReport, CatalogItems.OrderBy(x => random.Next()).FirstOrDefault().CatalogItemId, TruckUnits.OrderBy(x => random.Next()).FirstOrDefault().TruckUnitId);
                        EventDTO WellBoreReport = SetEventDTO(getJob, jobEventType.WellBoreReport, CatalogItems.OrderBy(x => random.Next()).FirstOrDefault().CatalogItemId, TruckUnits.OrderBy(x => random.Next()).FirstOrDefault().TruckUnitId);

                        var check = JobAndEventService.AddEventForJobEventType(JobPlan);
                        if (check <= 0)
                            WriteLogFile(string.Format("Failed to Add JobPlan for : " + addJob));
                        else
                            AddJopPlan(getJob, catalogDTO);
                        check = JobAndEventService.AddEventForJobEventType(EconomicAnalysisPlan);
                        if (check <= 0)
                            WriteLogFile(string.Format("Failed to Add EconomicAnalysisPlan for : " + addJob));
                        else
                            AddEconomicAnalysis(getJob, ppScenarioId, ppScenarioWaterId, ppScenarioGasId, txRateId, oilProductTypeId);
                        check = JobAndEventService.AddEventForJobEventType(JobCostDetailReport);
                        if (check <= 0)
                            WriteLogFile(string.Format("Failed to Add JobCostDetailReport for : " + addJob));
                        else
                            AddJobCostDetails(getJob, catalogDTO);
                        check = JobAndEventService.AddEventForJobEventType(FailureReport);
                        if (check <= 0)
                            WriteLogFile(string.Format("Failed to Add FailureReport for : " + addJob));
                        check = JobAndEventService.AddEventForJobEventType(WellBoreReport);
                        if (check <= 0)
                            WriteLogFile(string.Format("Failed to Add WellBoreReport for : " + addJob));
                    }
                    catch
                    {
                        WriteLogFile(string.Format("Failed to Add Job with for well : " + well.Name));
                    }
                }
                Console.WriteLine(JobsPerWell + " Jobs added for the Well " + well.Name);
            }
            //AddEventsForAllJobs(wellStart, wellEnd);
            //AddComponentsForOldestJob(wellStart, wellEnd);
            //AddObservation(wellStart, wellEnd);
            /* Created SQL script for Sticky Notes so commented this method*/
            //AddStickyNotes((int)wellStart, (int)wellEnd);
        }

        public void AddObservation(long wellStart, long wellEnd)
        {
            Authenticate();
            for (long i = wellStart; i <= wellEnd; i++)
            {
                WellDTO well = WellService.GetWell(i.ToString());
                if (well == null)
                    continue;
                JobLightDTO[] allJobds = JobAndEventService.GetJobsByWell(well.Id.ToString());
                List<JobLightDTO> LatestJobs = allJobds.OrderByDescending(x => random.Next()).Take(Convert.ToInt32(FailureJobsPerWell)).ToList();
                foreach (JobLightDTO Job in LatestJobs)
                {
                    if (Job.BeginDate < DateTime.Today)
                    {
                        try
                        {
                            JobComponentFailureDTO[] Comps = JobAndEventService.GetJobComponentFailure(Job.JobId.ToString());
                            if (Comps.Count() > 0)
                            {
                                //Add observation for Tubing
                                JobComponentFailureObservationDTO mdFailObserv = JobAndEventService.GetMetaDataForAddObservation(Comps.FirstOrDefault(x => x.PartType == "Tubing - OD  2.875").AssemblyComponentId.ToString(), Job.JobId.ToString());
                                mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue = false;
                                mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFailureDate").DataValue = Job.BeginDate.ToString();
                                string failObservationCompId = JobAndEventService.AddObservation(mdFailObserv);
                                //Add observation for Rod
                                mdFailObserv = JobAndEventService.GetMetaDataForAddObservation(Comps.FirstOrDefault(x => x.PartType == "Polished Rod").AssemblyComponentId.ToString(), Job.JobId.ToString());
                                mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue = false;
                                mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFailureDate").DataValue = Job.BeginDate.ToString();
                                failObservationCompId = JobAndEventService.AddObservation(mdFailObserv);
                                //Add observation for Casing
                                mdFailObserv = JobAndEventService.GetMetaDataForAddObservation(Comps.FirstOrDefault(x => x.PartType == "Casing/Casing Liner OD  5.500").AssemblyComponentId.ToString(), Job.JobId.ToString());
                                mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue = false;
                                mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFailureDate").DataValue = Job.BeginDate.ToString();
                                failObservationCompId = JobAndEventService.AddObservation(mdFailObserv);
                            }
                            else
                            {
                                WriteLogFile(string.Format("No Components available for the Failure Report in Job : " + Job.JobId));
                            }
                        }
                        catch
                        {
                            WriteLogFile(string.Format("Failed to Add observations for the Failure Report in Job : " + Job.JobId));
                        }
                    }
                }
                Console.WriteLine("Failures added for the well : " + well.Name);
            }
        }

        public void AddStickyNotes(int wellStart, int wellEnd)
        {
            Authenticate();
            StickyNoteTypeDTO[] stickyNoteTypes = WellService.GetStickyNoteType();
            StickyNoteStatusDTO[] stickyNoteStatuses = WellService.GetStickyNoteStatus();
            UserDTO[] users = WellService.GetUserDisplayName();
            PriorityDTO[] priorities = WellService.GetPriority();
            for (int j = wellStart; j <= wellEnd; j++)
            {
                WellDTO well = WellService.GetWell(j.ToString());
                if (well == null)
                    continue;
                AssemblyDTO getAssembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
                for (int i = 1; i <= Convert.ToInt32(NumberofNotes); i++)
                {
                    StickyNoteDTO stickyNotes = new StickyNoteDTO();
                    stickyNotes.UserRecipientId = users.OrderBy(x => random.Next()).FirstOrDefault().Id;
                    stickyNotes.PK_Assembly = getAssembly.Id;
                    stickyNotes.StickyNoteTypeId = stickyNoteTypes.OrderBy(x => random.Next()).FirstOrDefault().Id;
                    stickyNotes.StickyNoteStatusId = stickyNoteStatuses.OrderBy(x => random.Next()).FirstOrDefault().Id;
                    stickyNotes.PriorityId = priorities.OrderBy(x => random.Next()).FirstOrDefault().Id;
                    stickyNotes.BriefDescription = "Automated Brief Description";
                    stickyNotes.StickyNoteComments = "Automated Sticky note comments";
                    stickyNotes.OriginDate = DateTime.Today.AddDays(-i * 1);
                    stickyNotes.CompletionDate = stickyNotes.OriginDate.AddDays(5);
                    WellService.AddUpdateStickyNote(stickyNotes);
                }
                Console.WriteLine("Sticky notes added for the Well : " + well.Name);
            }
        }

        public void AddComponentsForOldestJob(long wellStart, long wellEnd)
        {
            Authenticate();
            JobEventTypeDTO jobEventType = JobAndEventService.GetJobEventTypes();
            for (long i = wellStart; i <= wellEnd; i++)
            {
                WellDTO well = WellService.GetWell(i.ToString());
                if (well == null)
                    continue;
                JobLightDTO[] allJobds = JobAndEventService.GetJobsByWell(well.Id.ToString());
                if (allJobds != null && allJobds.Count() > 0)
                {
                    SubAssemblyDTO[] subAssemblies = WellboreComponentService.GetSubAssembliesByWellId(well.Id.ToString());
                    JobLightDTO oldestJob = allJobds.OrderBy(x => x.BeginDate).FirstOrDefault();
                    AddComponentsForJob(oldestJob.JobId.ToString(), subAssemblies.FirstOrDefault().Id.ToString());

                    List<JobLightDTO> randomJobs = allJobds.OrderBy(x => random.Next()).Take(Convert.ToInt32(ComponentHistory)).ToList();
                    foreach (JobLightDTO job in randomJobs.OrderBy(x => x.BeginDate))
                    {
                        if (job.JobId != oldestJob.JobId)
                        {
                            long eventId = JobAndEventService.GetLatestEventIdByJobIdAndEventTypeId(job.JobId.ToString(), ((int)jobEventType.WellBoreReport).ToString());
                            if (eventId <= 0)
                            {
                                WriteLogFile(string.Format("Failed to get wellbore Report in Job : " + job.JobId));
                                continue;
                            }
                            WellboreGridDTO[] gridComps = JobAndEventService.GetWellboreComponentByJobIdAndEventId(job.JobId.ToString(), eventId.ToString());

                            if (gridComps != null && gridComps.FirstOrDefault().WellboreGridGroup.Count() > 0)
                            {
                                ComponentPartTypeDTO Ids = new ComponentPartTypeDTO();
                                Ids.JobId = job.JobId;
                                foreach (WellboreGridGroupDTO comp in gridComps.FirstOrDefault().WellboreGridGroup)
                                {
                                    ComponentMetaDataGroupDTO mdComp = comp.ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required");
                                    var ascId = mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascPrimaryKey").DataValue;
                                    Ids.AssemblyComponentId = Convert.ToInt64(ascId);
                                    Ids.PartTypeId = (int)mdComp.PartTypePrimaryKey;
                                    Ids.ComponentId = mdComp.ComponentPrimaryKey;
                                    Ids.EventId = eventId;
                                    bool rComp = ComponentService.RemoveComponent(Ids);
                                }
                                AddComponentsForJob(job.JobId.ToString(), subAssemblies.FirstOrDefault().Id.ToString());
                            }
                            else
                                AddComponentsForJob(job.JobId.ToString(), subAssemblies.FirstOrDefault().Id.ToString());
                        }
                    }
                }
                else
                    WriteLogFile(string.Format("No Jobs available for well : " + well.Name));
                Console.WriteLine("Components history is complete for the well : " + well.Name);
            }
        }

        public void AddComponentsForJob(string JobId, string subAssemblyId)
        {
            decimal[] details = new decimal[] { 1, 12834, 16 };//Quantity, Lenght, TopDepth
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Production Casing", "Casing/Casing Liner OD  5.500", 279, 1967);
            details = new decimal[] { 1, 3, 10919 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Tubing String", "Tubing Anchor/Catcher", 279, 1109);
            details = new decimal[] { 1, 3, 10919 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Tubing String", "Tubing Anchor/Catcher", 279, 1109);
            details = new decimal[] { 1, 1, 11111 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Tubing String", "Seat Nipple / Shoe", 279, 1734);
            details = new decimal[] { 1, 3357, 16 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Intermediate Casing", "Casing/Casing Liner OD  8.625", 279, 24808);
            details = new decimal[] { 1, 1641, 16 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Surface Casing", "Casing/Casing Liner OD 13.375", 279, 25987);
            details = new decimal[] { 1, 20, 16 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Rod String", "Polished Rod", 279, 518);
            details = new decimal[] { 1, 72, 10960 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Production Casing", "Wellbore Completion Detail (Perforations, etc.)", 279, 1062);
            details = new decimal[] { 1, 16, 12624 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Production Casing", "Wellbore Completion Detail (Perforations, etc.)", 279, 1062);
            details = new decimal[] { 1, 1, 11530 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Production Casing", "Wellbore Completion Detail (Perforations, etc.)", 279, 1062);
            details = new decimal[] { 1, 1, 8710 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Production Casing", "Wellbore Completion Detail (Perforations, etc.)", 279, 1062);
            details = new decimal[] { 1, 183, 8501 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Production Casing", "Wellbore Completion Detail (Perforations, etc.)", 279, 1062);
            details = new decimal[] { 1, 265, 8501 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Production Casing", "Wellbore Completion Detail (Perforations, etc.)", 279, 1062);
            details = new decimal[] { 1, 25, 8807 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Production Casing", "Wellbore Completion Detail (Perforations, etc.)", 279, 1062);
            details = new decimal[] { 1, 14, 8738 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Production Casing", "Wellbore Completion Detail (Perforations, etc.)", 279, 1062);
            details = new decimal[] { 1, 32, 8902 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Production Casing", "Wellbore Completion Detail (Perforations, etc.)", 279, 1062);
            details = new decimal[] { 1, 3, 10919 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Tubing String", "Tubing Anchor/Catcher", 279, 1109);
            details = new decimal[] { 1, 3357, 1657 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Intermediate Casing", "Cement (behind Casing)", 279, 1213);
            details = new decimal[] { 1, 1641, 16 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Surface Casing", "Cement (behind Casing)", 279, 1213);
            details = new decimal[] { 1, 10370, 2480 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Production Casing", "Cement (behind Casing)", 279, 1213);
            details = new decimal[] { 1, 4139, 8501 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Production Casing", "Wellbore Completion (Producing Interval)", 279, 1560);
            details = new decimal[] { 1, 7836, 5014 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Borehole", "Wellbore Hole", 279, 1609);
            details = new decimal[] { 1, 3357, 1657 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Borehole", "Wellbore Hole", 279, 1623);
            details = new decimal[] { 1, 1641, 16 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Borehole", "Wellbore Hole", 279, 1630);
            details = new decimal[] { 1, 1, 11111 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Tubing String", "Seat Nipple / Shoe", 279, 1734);
            details = new decimal[] { 1, 0.5m, 12845 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Production Casing", "Bull Plug (Casing)", 279, 3338);
            details = new decimal[] { 1, 4, 11400 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Production Casing", "Bridge Plug", 279, 6236);
            details = new decimal[] { 1, 4, 12500 };
            AddQAtoComponentforJob(JobId, subAssemblyId, details, "Production Casing", "Bridge Plug", 279, 6236);
        }

        public void AddQAtoComponentforJob(string JobId, string SubAsemblyId, decimal[] details, string cg, string pType, int mf, int cid, string quickAddCategory = "Add Components Grouping")
        {
            AddQAComponent(cg, pType, mf, cid, quickAddCategory + JobId);
            AddComponent(JobId, SubAsemblyId, details, quickAddCategory + JobId);
            RemoveQA(quickAddCategory + JobId);
        }

        public void AddComponent(string JobId, string SubAsemblyId, decimal[] details, string quickAddCategory)
        {
            long eventId = JobAndEventService.GetLatestEventIdByJobIdAndEventTypeId(JobId.ToString(), ((int)JobAndEventService.GetJobEventTypes().WellBoreReport).ToString());
            if (eventId <= 0)
            {
                WriteLogFile(string.Format("Failed to get wellbore Report in Job : " + JobId));
                return;
            }
            QuickAddComponentGroupingDTO QaComps = ComponentService.GetQuickAddComponents().FirstOrDefault(x => x.GroupingCategory == quickAddCategory);
            List<QuickAddComponentDTO> componentQA = new List<QuickAddComponentDTO>();
            //Add Component through Quick Add
            foreach (QuickAddComponentDTO qaComp in QaComps.QuickAddComponents)
            {
                componentQA.Add(qaComp);
                qaComp.JobId = Convert.ToInt64(JobId);
                qaComp.SubAssemblyId = Convert.ToInt64(SubAsemblyId);
                try
                {
                    ComponentMetaDataGroupBatchCollectionDTO[] qaMetadatagroups = ComponentService.GetMetadataFromQuickAddComponents(componentQA.ToArray());
                    foreach (ComponentMetaDataGroupBatchCollectionDTO qaMetadatagroup in qaMetadatagroups)
                    {
                        foreach (ComponentMetaDataGroupDTO group in qaMetadatagroup.ComponentMetadataCollection)
                        {
                            group.EventId = eventId;
                        }
                    }
                    MetaDataDTO[] reqFileds = qaMetadatagroups.FirstOrDefault().ComponentMetadataCollection.FirstOrDefault(x => x.CategoryName == "Required").Fields;
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
                    bool saveBatch = ComponentService.SaveWellboreComponent(qaMetadatagroups);
                    if (!saveBatch)
                        WriteLogFile(string.Format("Failed to Add Component through Quick Add : " + qaComp.ComponentGroupingName.ToString() + " : : " + qaComp.Component.MfgCat_PartType.ToString()));
                }
                catch
                {
                    WriteLogFile(string.Format("Failed to Add Component through Quick Add : " + qaComp.ComponentGroupingName.ToString() + " : : " + qaComp.Component.MfgCat_PartType.ToString()));
                }
                componentQA.Clear();
            }
        }

        public void RemoveQA(string quickAddCategory)
        {
            QuickAddComponentGroupingDTO QaComp = ComponentService.GetQuickAddComponents().FirstOrDefault(x => x.GroupingCategory == quickAddCategory);
            foreach (QuickAddComponentDTO qaComp in QaComp.QuickAddComponents)
            {
                try
                {
                    bool removeQAcomp = ComponentService.RemoveQuickAddComponent(qaComp.QuickAddComponentId.ToString());
                    if (!removeQAcomp)
                    {
                        WriteLogFile(string.Format("Failed to remove QA : " + qaComp.ComponentGroupingName.ToString() + " : : " + qaComp.Component.MfgCat_PartType.ToString()));
                    }
                }
                catch
                {
                    WriteLogFile(string.Format("Failed to remove QA :" + qaComp.ComponentGroupingName.ToString() + " : : " + qaComp.Component.MfgCat_PartType.ToString()));
                }
            }
        }

        public void AddQAComponent(string cg, string pType, int mf, int cid, string quickAddCategory)
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

            try
            {
                ControlIdTextDTO catDescription = listCatDescription.FirstOrDefault(x => x.ControlId == cid);

                //Add Quick Add Component
                ComponentMetaDataGroupDTO reqComponent = new ComponentMetaDataGroupDTO();
                reqComponent.QuickAddCategory = quickAddCategory;
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
                    WriteLogFile(string.Format("Failed to Add QA :" + compGrp.strComponentGrouping + " : : " + ptype.ptyPartType));
            }
            catch
            {
                WriteLogFile(string.Format("Failed to Add QA :" + compGrp.strComponentGrouping + " : : " + ptype.ptyPartType));
            }
        }

        public void AddQuickAddComponents()
        {
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            List<RRLPartTypeComponentGroupingTypeDTO> cg = new List<RRLPartTypeComponentGroupingTypeDTO>();
            List<string> CmpGrpgs = new List<string>() { "Borehole", "Production Casing", "Production Liner", "Rod String", "Surface Casing", "Intermediate Casing", "Tubing String" };
            foreach (string compGrp in CmpGrpgs)
            {
                cg.Add(componentGroups.FirstOrDefault(x => x.strComponentGrouping == compGrp));
            }
            try
            {
                QAtoComponent(cg.ToArray());
            }
            catch
            {
                WriteLogFile(string.Format("Failed to Add Quick Add components "));
            }
        }

        public ControlIdTextDTO[] GetMetadataReferenceDataDDL(MetaDataDTO metaData, string columnValue = "", string columnValue1 = "")
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

        public void QAtoComponent(RRLPartTypeComponentGroupingTypeDTO[] componentGroups)
        {
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            foreach (RRLPartTypeComponentGroupingTypeDTO compGrp in componentGroups)
            {
                partfilter.TypeId = compGrp.ptgFK_c_MfgCat_ComponentGrouping;
                partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
                RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);

                //Meta Data for Add quick add component
                foreach (RRLPartTypeComponentGroupingTypeDTO ptype in partTypes)
                {
                    ComponentMetaDataGroupDTO[] cmpMetaData = ComponentService.GetMetaDataForQuickAddComponentByPartTypeId(ptype.ptgFK_c_MfgCat_PartType.ToString());
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
                            WriteLogFile(string.Format("Failed to Add QA :" + compGrp.strComponentGrouping + " : : " + ptype.ptyPartType));
                    }
                    catch
                    {
                        WriteLogFile(string.Format("Failed to Add QA :" + compGrp.strComponentGrouping + " : : " + ptype.ptyPartType));
                    }
                }
            }
        }

        public void AddTemplateJobs(string wellId)
        {
            Authenticate();
            WellDTO well = WellService.GetAllWells().FirstOrDefault(x => x.Id == Convert.ToInt64(wellId));
            JobLightDTO[] JobsbyWell = JobAndEventService.GetJobsByWell(well.Id.ToString());
            TemplateJobDTO tj = new TemplateJobDTO();
            foreach (JobLightDTO job in JobsbyWell)
            {
                tj.JobId = job.JobId;
                tj.Job = job;
                tj.Category = job.EndDate.ToString("MM/dd/yyyy");
                try
                {
                    bool addTemplateJob = JobAndEventService.AddTemplateJobFromPlannedEvents(tj);
                    if (!addTemplateJob)
                        WriteLogFile(string.Format("Failed to Add WellBoreReport for : " + job.JobId.ToString()));
                }
                catch
                {
                    WriteLogFile(string.Format("Failed to Add WellBoreReport for : " + job.JobId.ToString()));
                }
            }
        }

        public EventDTO SetMultiAddEventDTO(JobLightDTO Job, EventDetailScheduledActivityDTO Event)
        {
            EventDTO evtMultiAdd = new EventDTO();
            evtMultiAdd.BeginTime = Job.BeginDate.AddDays(10).ToUniversalTime();
            evtMultiAdd.EndTime = Job.EndDate.AddDays(-2).ToUniversalTime();
            evtMultiAdd.EventType = Event.EventTypeName;
            evtMultiAdd.EventTypeId = Event.EventTypeId;
            evtMultiAdd.PK_BusinessOrganization = Job.BusinessOrganizationId == null ? 0 : (long)Job.BusinessOrganizationId;
            evtMultiAdd.PK_TruckUnit = Event.TruckUnitId;
            evtMultiAdd.HistoricalRate = 234;
            evtMultiAdd.PK_CatalogItem = Event.CatalogItemId;
            evtMultiAdd.PK_Job = Job.JobId;

            return evtMultiAdd;
        }

        public EventDTO SetEventDTO(JobLightDTO Job, EventTypeDTO Event)
        {
            EventDTO evtMultiAdd = new EventDTO();
            evtMultiAdd.BeginTime = Job.BeginDate.AddDays(13).ToUniversalTime();
            evtMultiAdd.EndTime = Job.EndDate.AddDays(-3).ToUniversalTime();
            evtMultiAdd.EventType = Event.EventTypeName;
            evtMultiAdd.EventTypeId = Event.EventTypeId;
            evtMultiAdd.PK_BusinessOrganization = Job.BusinessOrganizationId == null ? 0 : (long)Job.BusinessOrganizationId; ;
            evtMultiAdd.PK_TruckUnit = 1;
            evtMultiAdd.HistoricalRate = 234;
            evtMultiAdd.PK_CatalogItem = 1;
            evtMultiAdd.PK_Job = Job.JobId;

            return evtMultiAdd;
        }

        public void AddEventsForAllJobs(long wellStart, long wellEnd)
        {
            Authenticate();
            List<EventDTO> MultiAddEvents = new List<EventDTO>();
            for (long i = wellStart; i <= wellEnd; i++)
            {
                WellDTO well = WellService.GetWell(i.ToString());
                if (well == null)
                    continue;
                JobLightDTO[] allJobs = JobAndEventService.GetJobsByWell(well.Id.ToString());
                foreach (JobLightDTO job in allJobs)
                {
                    EventTypeGroupDTO Events = JobAndEventService.GetEventTypesByGrouping(job.JobTypeId.ToString(), job.JobId.ToString());
                    EventDetailScheduledActivityDTO[] arrSchPlannedEvents = Events.PlannedEvents;
                    foreach (EventDetailScheduledActivityDTO plannedEvent in arrSchPlannedEvents)
                    {
                        EventDTO setEvent = SetMultiAddEventDTO(job, plannedEvent);
                        MultiAddEvents.Add(setEvent);
                    }
                    EventDTO[] arrplannedEvents = MultiAddEvents.ToArray();
                    try
                    {
                        bool multiAdd = JobAndEventService.AddMultipleEventForJobEventType(arrplannedEvents);
                        if (!multiAdd)
                            WriteLogFile(string.Format("Failed to Add Planned Events for : " + job.JobId));
                    }
                    catch
                    {
                        WriteLogFile(string.Format("Failed to Add Planned Events for : " + job.JobId));
                    }
                    MultiAddEvents.Clear();
                    EventTypeDTO[] allEvents = Events.AllEvents;
                    List<string> EventNames = new List<string>() { "Cement Retainer - Composite - Set", "Acidize - Spot", "BOPE - Nipple Down", "BOPE Annular - Nipple Down" };
                    foreach (string Evt in EventNames)
                    {
                        EventTypeDTO addEvt = allEvents.FirstOrDefault(x => x.EventTypeName == Evt);
                        EventDTO setEvent = SetEventDTO(job, addEvt);
                        MultiAddEvents.Add(setEvent);
                    }
                    EventDTO[] userEvents = MultiAddEvents.ToArray();
                    try
                    {
                        bool multiAdd = JobAndEventService.AddMultipleEventForJobEventType(userEvents);
                        if (!multiAdd)
                            WriteLogFile(string.Format("Failed to Add User Events for : " + job.JobId));
                    }
                    catch
                    {
                        WriteLogFile(string.Format("Failed to Add User Events for : " + job.JobId));
                    }
                    MultiAddEvents.Clear();
                }
                Console.WriteLine("Events added to all the Jobs for a Well : " + well.Name);
            }
        }

        public EventDetailCostDTO CreateEventDetailCostDTO(long jobId)
        {
            EventDetailCostDTO eventCost = new EventDetailCostDTO();

            eventCost.JobId = jobId;
            eventCost.Cost = 2312;
            eventCost.CostRemarks = "Automated Cost Remark";
            eventCost.Discount = 62;
            eventCost.Quantity = 2;
            eventCost.UnitPrice = 642;

            return eventCost;
        }

        public void AddJobCostDetails(JobLightDTO getJob, CatalogItemGroupDTO catalogDTO)
        {
            string jobId = getJob.ToString();
            EventDetailCostDTO anEventCostDetails = CreateEventDetailCostDTO(getJob.JobId);
            List<string> Vendors = new List<string>() { "_Generic Manufacturer", "Absolute Completion Technologies", "Advantage Products, Inc", "American",
            "Bukaka", "Big O", "Cabot", "Dansco"};
            foreach (string vendor in Vendors)
            {
                anEventCostDetails.VendorId = catalogDTO.Vendors.FirstOrDefault(x => x.BusinessOrganizationName == vendor).Id;
                anEventCostDetails.CatalogItemId = catalogDTO.CatalogItems.FirstOrDefault(x => x.VendorId == anEventCostDetails.VendorId).CatalogItemId;
                anEventCostDetails.CostDate = getJob.EndDate.AddDays(-4);
                try
                {
                    string addedEventCostId = JobAndEventService.AddEventDetailCost(anEventCostDetails);
                    if (addedEventCostId == null)
                        WriteLogFile(string.Format("Failed to Add Job Cost Details for : " + jobId));
                }
                catch
                {
                    WriteLogFile(string.Format("Failed to Add Job Cost Details for : " + jobId));
                }
            }
        }

        public EventDetailScheduledActivityDTO CreateJobPlanDetailDTO(string jobId, long evtTypeId, CatalogItemGroupDTO catalogDTO)
        {
            EventDetailScheduledActivityDTO eventJobPlan = new EventDetailScheduledActivityDTO();
            eventJobPlan.AccountingId = "WFT0001";
            eventJobPlan.AssignedTo = "Automated User";
            eventJobPlan.ResponsiblePerson = "Automated User";
            eventJobPlan.TotalCost = 1516m;
            eventJobPlan.Discount = 62m;
            eventJobPlan.EstimatedHours = 42;
            eventJobPlan.Quantity = 3;
            eventJobPlan.UnitPrice = 789;
            eventJobPlan.JobId = Convert.ToInt64(jobId);
            eventJobPlan.VendorId = catalogDTO.CatalogItems.OrderBy(x => x.CatalogItemId).FirstOrDefault(x => x.VendorId == 116).VendorId;
            eventJobPlan.CatalogItemId = catalogDTO.CatalogItems.OrderBy(x => x.CatalogItemId).FirstOrDefault(x => x.CatalogItemId == 117).CatalogItemId;
            eventJobPlan.TruckUnitId = catalogDTO.TruckUnits.Single(x => x.VendorId == eventJobPlan.VendorId).TruckUnitId;
            eventJobPlan.EventTypeId = evtTypeId;
            eventJobPlan.Remarks = "Automated Remarks";
            eventJobPlan.Description = "Automated Description";
            return eventJobPlan;
        }

        public void AddJopPlan(JobLightDTO getJob, CatalogItemGroupDTO catalogDTO)
        {
            string jobId = getJob.JobId.ToString();
            int order = 0;
            List<string> EventTypes = new List<string>() { "Safety Meeting", "Tubing - Check Pressure", "Casing - Check Pressure", "Pulling Unit - Rig Up",
                "Pumping Unit - Remove Head", "Pump/Rods - Pull", "Wellhead - Nipple Down", "Tubing Anchor - Release", "BOPE - Nipple Up", "Tubing - Pull",
            "Tubing - Scanalog", "Tag Fill", "Wellbore Cleanout", "Tubing - Hydrotest", "Tubing - Run", "BOPE - Nipple Down", "Tubing Anchor - Set", "Wellhead - Nipple Up",
            "Pump/Rods - Run", "Pump - Pressure Test", "Pumping Unit - Hang on Head", "Pulling Unit - Rig Down", "Job Plan Cost Adjustment"};
            foreach (string evtType in EventTypes)
            {
                long evtTypeId = JobAndEventService.GetAllEventTypesForJobType(getJob.JobTypeId.ToString()).FirstOrDefault(x => x.EventTypeName.Trim().ToUpper() == evtType.Trim().ToUpper()).EventTypeId;
                EventDetailScheduledActivityDTO addJobPlanDetail = CreateJobPlanDetailDTO(jobId, evtTypeId, catalogDTO);
                addJobPlanDetail.Order = order + 1;
                try
                {
                    string addedJobPlanId = JobAndEventService.AddJobPlanDetail(addJobPlanDetail);
                    if (addedJobPlanId == null)
                        WriteLogFile(string.Format("Failed to Add Job Plan for : " + jobId));
                }
                catch
                {
                    WriteLogFile(string.Format("Failed to Add Job Plan for : " + jobId));
                }
            }
        }

        public void AddEconomicAnalysis(JobLightDTO getJob, long ppScenarioId, long ppScenarioWaterId, long ppScenarioGasId, long txRateId, long oilProductTypeId)
        {
            string jobId = getJob.JobId.ToString();
            JobEconomicAnalysisDTO jobEconomicAnalysis = new JobEconomicAnalysisDTO();
            jobEconomicAnalysis.AnalysisDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Local);
            jobEconomicAnalysis.AnalysisPeriod = 180;
            jobEconomicAnalysis.CumeGas = 450.55m;
            jobEconomicAnalysis.CumeOil = 360.44m;
            jobEconomicAnalysis.CumeWater = null;
            jobEconomicAnalysis.CurrentProductionDeclineGas = null;
            jobEconomicAnalysis.CurrentProductionDeclineOil = 0.21m;
            jobEconomicAnalysis.CurrentProductionDeclineWater = null;
            jobEconomicAnalysis.CurrentProductionRateGas = 80m;
            jobEconomicAnalysis.CurrentProductionRateOil = 64m;
            jobEconomicAnalysis.CurrentProductionRateWater = null;
            jobEconomicAnalysis.DPICalcOverriden = false;
            jobEconomicAnalysis.DiscountRate = null;
            jobEconomicAnalysis.DiscountedProfitabilityIndex = 0.443m;
            jobEconomicAnalysis.DownTimeDays = 2;
            jobEconomicAnalysis.EstimatedLostProductionCost = 1312;
            jobEconomicAnalysis.EstimatedRecurringVariableCostsOil = 2;
            jobEconomicAnalysis.EstimatedRecurringVariableCostsWater = null;
            jobEconomicAnalysis.EstimatedSteamQuantity = 24;
            jobEconomicAnalysis.EstimatedSteamUnitCost = 27;
            jobEconomicAnalysis.EstimatedTaxes = 0.01m;
            jobEconomicAnalysis.EstimatedTotalJobCosts = 2354;
            jobEconomicAnalysis.EstimatedTotalRecurringFixedCosts = 345;
            jobEconomicAnalysis.ExpectedDecline = 0.2m;
            jobEconomicAnalysis.ExpectedRate = 66m;
            jobEconomicAnalysis.FinalDate = DateTime.Today;
            jobEconomicAnalysis.FinalGasRate = null;
            jobEconomicAnalysis.FinalOilRate = 64;
            jobEconomicAnalysis.FirstYearCashFlow = null;
            jobEconomicAnalysis.GOR = 1.25m;
            jobEconomicAnalysis.InitialDate = DateTime.Today.AddDays(-375);
            jobEconomicAnalysis.JobId = getJob.JobId;
            jobEconomicAnalysis.InitialGasRate = null;
            jobEconomicAnalysis.InitialOilRate = 79;
            jobEconomicAnalysis.JobNPV = null;
            jobEconomicAnalysis.NetReserves = null;
            jobEconomicAnalysis.PlannedRoyaltyInterest = 4;
            jobEconomicAnalysis.PlannedWorkingInterest = 2.5m;
            jobEconomicAnalysis.ProbabilityOfSuccess = null;
            jobEconomicAnalysis.ProductionForecastScenarioId = 1;
            jobEconomicAnalysis.ProductForecastScenarioWaterId = 1;
            jobEconomicAnalysis.ProductForecastScenarioGasId = 1;
            jobEconomicAnalysis.ProductPriceScenarioId = ppScenarioId;
            jobEconomicAnalysis.ProductPriceScenarioWaterId = ppScenarioWaterId;
            jobEconomicAnalysis.ProductPriceScenarioGasId = ppScenarioGasId;
            jobEconomicAnalysis.TaxRateId = txRateId;
            jobEconomicAnalysis.ProductTypeId = oilProductTypeId;
            jobEconomicAnalysis.RiskFactor = null;
            jobEconomicAnalysis.RoyaltyBurden = 3.694m;
            jobEconomicAnalysis.UniqueId = null;
            jobEconomicAnalysis.WOR = 1.67m;
            try
            {
                bool status = JobAndEventService.AddUpdateJobEconomicAnalysis(jobEconomicAnalysis);
                if (!status)
                    WriteLogFile(string.Format("Failed to Add EconomicAnalysisPlan for : " + jobId));
            }
            catch
            {
                WriteLogFile(string.Format("Failed to Add EconomicAnalysisPlan for : " + jobId));
            }
        }
    }
}