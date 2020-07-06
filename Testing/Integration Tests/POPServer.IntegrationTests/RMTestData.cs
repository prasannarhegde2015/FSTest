using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;

namespace Weatherford.POP.Server.IntegrationTests
{
    public class RMTestData : APIClientTestBase
    {
        public void SaveServiceJobData()
        {
            bool allowBulkDataInsert = false;

            if (allowBulkDataInsert)
            {
                string downholeEquipmentTypeConstant = "DOWNHOLE";
                string surfaceEquipmentTypeConstant = "SURFACE";
                string jobTypeConstant = "INSTALL";

                string wellName = "TestWell";
                long wellNumber = 1;

                long jobStartNum = 1;
                long jobEndNum = 10000;

                long downholeJobStartNum = jobStartNum;
                long downholeJobEndNum = (jobEndNum * 70) / 100;

                long surfaceJobStartNum = downholeJobEndNum + 1;
                long surfaceJobEndNum = jobEndNum;//(jobEndNum * 30) / 100;

                ReferenceDataConfiguration();

                UserManagement();

                wellNumber = CreateNSJ(downholeEquipmentTypeConstant, jobTypeConstant, wellName, "DSN_", downholeJobStartNum, downholeJobEndNum, wellNumber);

                CreateNSJ(surfaceEquipmentTypeConstant, jobTypeConstant, wellName, "SSN_", surfaceJobStartNum, surfaceJobEndNum, wellNumber);

                //CreateNSJ(surfaceEquipmentTypeConstant, jobTypeConstant, wellName, "SSN_", 1, 1, wellNumber);

            }
        }

        public long CreateNSJ(string equipmentTypeConstant, string jobTypeConstant, string wellNamePrefix, string serialNumberPrefix, long jobStartNum, long jobEndNum, long wellNumber)
        {
            string serialNumber = string.Empty;
            long serviceLocationId = 0;
            long serviceUserId = 0;
            long repairUserId = 0;
            string componentAPIDescription = string.Empty;
            long assemblyId = 0;
            bool isAPIDescriptionRequired = false;
            bool isDisplayWearTabRequired = false;
            long componentId = 0;
            long jobTypeId = 0;
            long equipmentTypeId = 0;
            long equipmentSubTypeId = 0;
            long businessOrganizationId = 0;
            long surfaceUnitManufacturerId = 0;
            long surfaceUnitBrandId = 0;

            JobStatusDTO[] jobStatusDTOs = JobAndEventService.GetJobStatuses();

            JobStatusDTO jobStatusDTO = jobStatusDTOs.FirstOrDefault();

            //Save Well Site
            WellDTO wellDTO = SaveWellSite(wellNamePrefix + wellNumber);
            assemblyId = wellDTO.AssemblyId;

            EquipmentTypeGroupDTO groupData = EquipmentConfigurationService.GetEquipmentGroupData();

            EquipmentTypeDTO equipmentTypeDTO = groupData.EquipmentType.Where(x => x.ConstantId == equipmentTypeConstant).FirstOrDefault();

            EquipmentSubTypeDTO equipmentSubTypeDTO = groupData.EquipmentSubType.Where(x => x.EquipmentTypeId == equipmentTypeDTO.EquipmentTypeId).FirstOrDefault();

            equipmentSubTypeId = equipmentSubTypeDTO.EquipmentSubTypeId;

            FSCRefrenceData(equipmentTypeDTO, equipmentSubTypeId, out serviceLocationId, out serviceUserId, out repairUserId);

            //Save Equipment Configuration
            SaveEquipmentConfiguration(equipmentSubTypeDTO, equipmentTypeDTO.Description, out isAPIDescriptionRequired, out isDisplayWearTabRequired);

            JobTypeByEquipmentDTO[] jobTypeByEquipments = EquipmentJobService.GetJobTypeByEquipment(Convert.ToString(equipmentSubTypeDTO.EquipmentSubTypeId), "false");

            JobTypeByEquipmentDTO jobTypeByEquipment = jobTypeByEquipments.Where(x => x.JobTypeConstant == jobTypeConstant).FirstOrDefault();
            jobTypeId = jobTypeByEquipment.FK_JobTypeId;

            JobReasonDTO[] JobReasonDTOs = JobAndEventService.GetJobReasonsForJobType(Convert.ToString(jobTypeId));

            JobReasonDTO jobReasonDTO = JobReasonDTOs.FirstOrDefault();

            EquipmentConfigDTO equipmentConfig = EquipmentConfigurationService.GetEquipmentConfigToggles(Convert.ToString(equipmentSubTypeDTO.EquipmentTypeId), Convert.ToString(equipmentSubTypeDTO.EquipmentSubTypeId));

            BusinessOrganizationDTO[] businessOrganizationDTOS = EquipmentJobService.GetBuisnessOrganization();

            BusinessOrganizationDTO businessOrganizationDTO = businessOrganizationDTOS.FirstOrDefault();

            businessOrganizationId = businessOrganizationDTO.Id;

            for (long number = jobStartNum; number <= jobEndNum; number++)
            {
                if (number % 100 == 1)
                {
                    wellDTO = SaveWellSite(wellNamePrefix + wellNumber);
                    assemblyId = wellDTO.AssemblyId;

                    wellNumber++;
                }

                if (equipmentTypeDTO.ConstantId == "SURFACE")
                {
                    Random rnd = new Random();

                    POPRRLManufacturerDTO[] POPRRLManufacturers = CatalogService.GetSurfaceUnitManufacturers();

                    int randomNumber = rnd.Next(1, POPRRLManufacturers.Count() + 1);

                    POPRRLManufacturerDTO POPRRLManufacturer = POPRRLManufacturers[randomNumber - 1];//POPRRLManufacturers.FirstOrDefault();

                    POPRRLSurfaceUnitDTO[] surfaceUnitsMfgs = CatalogService.GetSurfaceUnitsByManufacturer(Convert.ToString(POPRRLManufacturer.PrimaryKey));

                    rnd = new Random();

                    randomNumber = rnd.Next(1, surfaceUnitsMfgs.Count() + 1);

                    POPRRLSurfaceUnitDTO surfaceUnitsMfg = surfaceUnitsMfgs[randomNumber - 1];

                    if (POPRRLManufacturer != null)
                    {
                        surfaceUnitManufacturerId = POPRRLManufacturer.PrimaryKey;
                    }

                    if (surfaceUnitsMfgs != null)
                    {
                        surfaceUnitBrandId = surfaceUnitsMfg.PK_rrlSurfaceBase;
                    }
                }

                serialNumber = serialNumberPrefix + Convert.ToString(number);

                //Save Serial Number
                ComponentDTO componentDTO = SaveSerialNumber(equipmentSubTypeDTO, businessOrganizationId, serialNumber, isAPIDescriptionRequired);

                componentId = componentDTO.Id;
                componentAPIDescription = componentDTO.APIDescription;

                equipmentTypeId = equipmentTypeDTO.EquipmentTypeId;

                BarrelPlungerDTO[] barrelPlungerDTOs = new List<BarrelPlungerDTO>().ToArray();

                if (isAPIDescriptionRequired)
                {
                    barrelPlungerDTOs = BarrelPlungerTabForJob(componentId, componentAPIDescription);
                }

                ServiceJobDTO serviceJobDTO = new ServiceJobDTO();

                serviceJobDTO.JobBridgeDTO = new JobBridgeRMDTO();
                serviceJobDTO.ServiceRMJobDTO = new ServiceRMJobDTO();
                serviceJobDTO.AdditionalInfoDTO = new List<AdditionalInfoFieldDTO>().ToArray();
                serviceJobDTO.ConditionalInfoDTO = new List<ServiceJobConditionDTO>().ToArray();

                //Assign data for Start tab
                serviceJobDTO.JobBridgeDTO.EquipmentTypeId = equipmentTypeDTO.EquipmentTypeId;
                serviceJobDTO.JobBridgeDTO.EquipmentSubTypeId = equipmentSubTypeId;
                serviceJobDTO.JobBridgeDTO.PumpAPI = componentAPIDescription;

                serviceJobDTO.JobBridgeDTO.SurfaceUnitManufacturerId = surfaceUnitManufacturerId;
                serviceJobDTO.JobBridgeDTO.SurfaceUnitBrandId = surfaceUnitBrandId;

                serviceJobDTO.ServiceRMJobDTO.JobTypeId = jobTypeId;
                serviceJobDTO.JobBridgeDTO.ComponentId = componentId;
                serviceJobDTO.ServiceRMJobDTO.BusinessOrganizationId = businessOrganizationId;
                serviceJobDTO.ServiceRMJobDTO.AssemblyId = assemblyId;

                serviceJobDTO.ServiceRMJobDTO.InstallDate = DateTime.Now.AddMonths(-5);
                serviceJobDTO.ServiceRMJobDTO.PullDate = DateTime.Now.AddMonths(-5);

                //Assign data for Service tab
                serviceJobDTO.ServiceRMJobDTO.JobBegDateTime = DateTime.Now.AddMonths(-5);
                serviceJobDTO.ServiceRMJobDTO.JobEndDateTime = DateTime.Now.AddMonths(-5);
                serviceJobDTO.JobBridgeDTO.ServiceLocationId = serviceLocationId;
                serviceJobDTO.JobBridgeDTO.ServiceUserId = serviceUserId;
                serviceJobDTO.ServiceRMJobDTO.OriginKey = "QN 1";
                serviceJobDTO.ServiceRMJobDTO.AccountingRef = "Billing Information 1";

                //Assign data for Service info tab
                serviceJobDTO.JobBridgeDTO.WONumber = "ERP WOR 1";
                serviceJobDTO.JobBridgeDTO.FieldTicketNumber = "FTN 1";
                serviceJobDTO.JobBridgeDTO.RepairUserId = repairUserId;
                serviceJobDTO.ServiceRMJobDTO.JobReasonId = jobReasonDTO.Id;
                serviceJobDTO.ServiceRMJobDTO.StatusId = jobStatusDTO.Id;

                long jobId = EquipmentJobService.SaveServiceJobData(serviceJobDTO);

                serviceJobDTO.ServiceRMJobDTO.JobId = jobId;

                //Assign data for barrel & plunger wear tab
                serviceJobDTO.BarrelPlungerDTO = barrelPlungerDTOs;

                serviceJobDTO.IsConditionalSave = true;

                serviceJobDTO.ConditionalInfoDTO = this.ConditionalDetails(Convert.ToString(jobId), Convert.ToString(equipmentTypeId), Convert.ToString(equipmentSubTypeId));

                jobId = EquipmentJobService.SaveServiceJobData(serviceJobDTO);

                serviceJobDTO.IsConditionalSave = false;

                serviceJobDTO.JobBridgeDTO.ServiceJobRemarks = "Additional Info - Details Job Notes";

                serviceJobDTO.ServiceRMJobDTO.Remarks = "Service Info - Notes";

                jobId = EquipmentJobService.SaveServiceJobData(serviceJobDTO);

                UploadDocumnet(jobId);
            }

            return wellNumber;
        }

        public void FSCRefrenceData(EquipmentTypeDTO equipmentTypeDTO, long equipmentSubTypeId, out long serviceLocationId, out long serviceUserId, out long repairUserId)
        {
            serviceLocationId = 0;
            serviceUserId = 0;
            repairUserId = 0;

            string serviceSalesPersonUserNameStr = "Service Sales Person";

            string repairServiceTechnicianUserNameStr = "Repair Service Technician";

            #region Service Location

            UserDTO[] userDTOs = AdministrationService.GetUsers();

            ServiceLocationDTO[] serviceLocationDTOs = EquipmentJobService.GetServiceLocation(Convert.ToString(equipmentTypeDTO.EquipmentTypeId));

            ReferenceDataMaintenanceEntityDTO[] allReferenceTables = DBEntityService.GetReferenceDataMaintenanceEntities();

            if (serviceLocationDTOs.Count() == 0)
            {
                serviceLocationId = Convert.ToInt64(this.SaveServiceLocation(allReferenceTables, equipmentTypeDTO));
            }
            else
            {
                ServiceLocationDTO serviceLocationDTO = serviceLocationDTOs.Where(x => x.FK_EquipmentTypeId == equipmentTypeDTO.EquipmentTypeId).FirstOrDefault();

                serviceLocationId = serviceLocationDTO.ServiceLocationId;
            }

            UserDTO[] serviceSalesPersonDTOs = EquipmentJobService.GetServiceTechnician(Convert.ToString(serviceLocationId));

            if (serviceSalesPersonDTOs.Count() == 0)
            {
                UserDTO userDTO = userDTOs.Where(x => x.Name.Equals(serviceSalesPersonUserNameStr)).FirstOrDefault();
                this.SaveUserServiceLocation(allReferenceTables, userDTO.Id, serviceLocationId);

                userDTO = userDTOs.Where(x => x.Name.Equals(repairServiceTechnicianUserNameStr)).FirstOrDefault();
                this.SaveUserServiceLocation(allReferenceTables, userDTO.Id, serviceLocationId);

                serviceSalesPersonDTOs = EquipmentJobService.GetServiceTechnician(Convert.ToString(serviceLocationId));
            }

            UserDTO serviceSalesPersonDTO = serviceSalesPersonDTOs.FirstOrDefault();
            serviceUserId = serviceSalesPersonDTO.Id;

            UserDTO serviceTechnicianDTO = serviceSalesPersonDTOs.LastOrDefault();
            repairUserId = serviceTechnicianDTO.Id;

            #endregion

            this.SaveCriticality(allReferenceTables, equipmentSubTypeId);

            this.SaveFailureCondition(allReferenceTables, equipmentSubTypeId);

            this.SaveFailureCause(allReferenceTables, equipmentSubTypeId);

            this.SaveForeignMaterial(allReferenceTables, equipmentSubTypeId);

            this.SavePartAction(allReferenceTables, equipmentSubTypeId);
        }

        public void ReferenceDataConfiguration()
        {
            ReferenceDataMaintenanceEntityDTO[] allReferenceTables = DBEntityService.GetReferenceDataMaintenanceEntities();

            BusinessOrganizationDTO[] businessOrganizationDTOS = EquipmentJobService.GetBuisnessOrganization();

            if (businessOrganizationDTOS.Count() == 0)
            {
                //Bususiness Organization   
                this.SaveBusinessOrganization(allReferenceTables);
            }
        }
        public string SaveBusinessOrganization(ReferenceDataMaintenanceEntityDTO[] allReferenceTables)
        {
            ReferenceDataMaintenanceEntityDTO ReferenceTable = allReferenceTables.Where(x => x.EntityName.Equals("BusinessOrganization")).FirstOrDefault();

            MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(ReferenceTable.EntityName);

            foreach (MetaDataDTO addMetaData in addMetaDatas.Where(x => !x.PKey && x.Required))
            {
                switch (addMetaData.ColumnName)
                {
                    case "venFK_r_BusinessOrganizationType":
                        addMetaData.DataValue = 6;
                        break;
                    case "venFK_r_StateProvince":
                        addMetaData.DataValue = 1;
                        break;
                    case "venFK_r_Country":
                        addMetaData.DataValue = 1;
                        break;
                    case "venBusinessOrganizationName":
                        addMetaData.DataValue = "Test BU";
                        break;
                    case "venTaxRate":
                        addMetaData.DataValue = 0.00;
                        break;
                    case "venDftExpGL":
                        addMetaData.DataValue = 0.00;
                        break;
                    case "venPORequired":
                        addMetaData.DataValue = 0;
                        break;
                    case "venConstantId":
                        addMetaData.DataValue = "Test BU";
                        break;
                    case "venChangeTime":
                        addMetaData.DataValue = "Now";
                        break;
                    default:
                        break;
                }

                //if (addMetaData.ColumnName.Equals("venFK_r_BusinessOrganizationType"))
                //{
                //    addMetaData.DataValue = 6;
                //}
                //else if (addMetaData.ColumnName.Equals("venFK_r_StateProvince"))
                //{
                //    addMetaData.DataValue = 1;
                //}
                //else if (addMetaData.ColumnName.Equals("venFK_r_Country"))
                //{
                //    addMetaData.DataValue = 1;
                //}
                //else if (addMetaData.ColumnName.Equals("venBusinessOrganizationName"))
                //{
                //    addMetaData.DataValue = "Test BU";
                //}
                //else if (addMetaData.ColumnName.Equals("venTaxRate"))
                //{
                //    addMetaData.DataValue = 0.00;
                //}
                //else if (addMetaData.ColumnName.Equals("venDftExpGL"))
                //{
                //    addMetaData.DataValue = 0.00;
                //}
                //else if (addMetaData.ColumnName.Equals("venPORequired"))
                //{
                //    addMetaData.DataValue = 0;
                //}
                //else if (addMetaData.ColumnName.Equals("venConstantId"))
                //{
                //    addMetaData.DataValue = "Test BU";
                //}
                //else if (addMetaData.ColumnName.Equals("venChangeTime"))
                //{
                //    addMetaData.DataValue = "Now";
                //}
            }

            string pKey = DBEntityService.AddReferenceData(addMetaDatas);
            //Assert.IsNotNull(pKey, "Business organization does not exist.");

            return pKey;
        }

        public void SaveUserServiceLocation(ReferenceDataMaintenanceEntityDTO[] allReferenceTables, long userId, long serviceLocationId)
        {
            ReferenceDataMaintenanceEntityDTO ReferenceTable = allReferenceTables.Where(x => x.EntityName.Equals("UserServiceLocation_RM")).FirstOrDefault();
            //Assert.IsNotNull(ReferenceTable, "r_ServiceLocation_RM reference table are available inside the database.");

            MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(ReferenceTable.EntityName);

            foreach (MetaDataDTO addMetaData in addMetaDatas.Where(x => !x.PKey && x.Required))
            {
                switch (addMetaData.ColumnName)
                {
                    case "uslFK_User":
                        addMetaData.DataValue = userId;
                        break;
                    case "uslFK_r_ServiceLocation_RM":
                        addMetaData.DataValue = serviceLocationId;
                        break;
                    case "uslChangeTime":
                        addMetaData.DataValue = "Now";
                        break;
                    default:
                        break;
                }

                //if (addMetaData.ColumnName.Equals("uslFK_User"))
                //{
                //    addMetaData.DataValue = userId;
                //}
                //else if (addMetaData.ColumnName.Equals("uslFK_r_ServiceLocation_RM"))
                //{
                //    addMetaData.DataValue = serviceLocationId;
                //}
                //else if (addMetaData.ColumnName.Equals("uslChangeTime"))
                //{
                //    addMetaData.DataValue = "Now";
                //}
            }

            string pKey = DBEntityService.AddReferenceData(addMetaDatas);
        }

        public string SaveServiceLocation(ReferenceDataMaintenanceEntityDTO[] allReferenceTables, EquipmentTypeDTO equipmentTypeDTO)
        {
            ReferenceDataMaintenanceEntityDTO ReferenceTable = allReferenceTables.Where(x => x.EntityName.Equals("r_ServiceLocation_RM")).FirstOrDefault();
            //Assert.IsNotNull(ReferenceTable, "r_ServiceLocation_RM reference table are available inside the database.");

            MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(ReferenceTable.EntityName);

            foreach (MetaDataDTO addMetaData in addMetaDatas.Where(x => !x.PKey && x.Required))
            {
                switch (addMetaData.ColumnName)
                {
                    case "rslFK_r_EquipmentType":
                        addMetaData.DataValue = equipmentTypeDTO.EquipmentTypeId;
                        break;
                    case "rslCompanyName":
                        addMetaData.DataValue = "WFT Company";
                        break;
                    case "rslLocation":
                        addMetaData.DataValue = equipmentTypeDTO.Description + " Location";
                        break;
                    case "uslChangeTime":
                        addMetaData.DataValue = "Now";
                        break;
                    default:
                        break;
                }

                //if (addMetaData.ColumnName.Equals("rslFK_r_EquipmentType"))
                //{
                //    addMetaData.DataValue = equipmentTypeDTO.EquipmentTypeId;
                //}
                //else if (addMetaData.ColumnName.Equals("rslCompanyName"))
                //{
                //    addMetaData.DataValue = "WFT Company";
                //}
                //else if (addMetaData.ColumnName.Equals("rslLocation"))
                //{
                //    addMetaData.DataValue = equipmentTypeDTO.Description + " Location";
                //}
                //else if (addMetaData.ColumnName.Equals("rslChangeTime"))
                //{
                //    addMetaData.DataValue = "Now";
                //}
            }

            string pKey = DBEntityService.AddReferenceData(addMetaDatas);
            //Assert.IsNotNull(pKey, "Service location does not exist.");

            return pKey;
        }


        public void SaveCriticality(ReferenceDataMaintenanceEntityDTO[] allReferenceTables, long equipmentSubTypeId)
        {
            ReferenceDataMaintenanceEntityDTO ReferenceTable = allReferenceTables.Where(x => x.EntityName.Equals("r_CriticalityByEquipmentSubType_RM")).FirstOrDefault();

            MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(ReferenceTable.EntityName);

            MetaDataReferenceData cd = new MetaDataReferenceData();

            cd.MetaData = addMetaDatas.FirstOrDefault(x => x.ColumnName.Equals("rce_FK_r_Criticality_RM"));

            ControlIdTextDTO[] cdMetaDatas = JobAndEventService.GetMetaDataReferenceData(cd);

            CriticalityDTO[] criticalityDTOs = EquipmentJobService.GetCriticality(Convert.ToString(equipmentSubTypeId));

            //EntityGridSettingDTO setting = new EntityGridSettingDTO();
            //setting.GridSetting = new GridSettingDTO { PageSize = 20, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
            //setting.EntityName = "r_CriticalityByEquipmentSubType_RM";
            //DBEntityDTO gridData = DBEntityService.GetTableData(setting);

            foreach (ControlIdTextDTO cdMetaData in cdMetaDatas)
            {
                if (cdMetaData != null)
                {
                    if (criticalityDTOs != null && criticalityDTOs.Where(x => x.Id.Equals(cdMetaData.ControlId)).Count() == 0)
                    {
                        foreach (MetaDataDTO addMetaData in addMetaDatas.Where(x => !x.PKey && x.Required))
                        {
                            switch (addMetaData.ColumnName)
                            {
                                case "rce_FK_r_Criticality_RM":
                                    addMetaData.DataValue = cdMetaData.ControlId;
                                    break;
                                case "rce_FK_r_EquipmentSubType":
                                    addMetaData.DataValue = equipmentSubTypeId;
                                    break;
                                case "rceChangeTime":
                                    addMetaData.DataValue = "Now";
                                    break;
                                default:
                                    break;
                            }

                            //if (addMetaData.ColumnName.Equals("rce_FK_r_Criticality_RM"))
                            //{
                            //    addMetaData.DataValue = cdMetaData.ControlId;
                            //}
                            //else if (addMetaData.ColumnName.Equals("rce_FK_r_EquipmentSubType"))
                            //{
                            //    addMetaData.DataValue = equipmentSubTypeId;
                            //}
                            //else if (addMetaData.ColumnName.Equals("rceChangeTime"))
                            //{
                            //    addMetaData.DataValue = "Now";
                            //}
                        }

                        string pKey = DBEntityService.AddReferenceData(addMetaDatas);
                    }
                }

            }
        }

        public void SaveFailureCondition(ReferenceDataMaintenanceEntityDTO[] allReferenceTables, long equipmentSubTypeId)
        {
            ReferenceDataMaintenanceEntityDTO ReferenceTable = allReferenceTables.Where(x => x.EntityName.Equals("r_FailureConditionByEquipmentSubType_RM")).FirstOrDefault();

            MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(ReferenceTable.EntityName);

            MetaDataReferenceData cd = new MetaDataReferenceData();

            cd.MetaData = addMetaDatas.FirstOrDefault(x => x.ColumnName.Equals("roe_FK_r_FailureCondition_RM"));

            ControlIdTextDTO[] cdMetaDatas = JobAndEventService.GetMetaDataReferenceData(cd);

            ServiceJobConditionDropDownDTO[] failureConditionDTOs = EquipmentJobService.GetFailureCondition(Convert.ToString(equipmentSubTypeId));

            foreach (ControlIdTextDTO cdMetaData in cdMetaDatas)
            {
                if (cdMetaData != null)
                {
                    if (failureConditionDTOs != null && failureConditionDTOs.Where(x => x.Id.Equals(cdMetaData.ControlId)).Count() == 0)
                    {
                        foreach (MetaDataDTO addMetaData in addMetaDatas.Where(x => !x.PKey && x.Required))
                        {
                            switch (addMetaData.ColumnName)
                            {
                                case "roe_FK_r_FailureCondition_RM":
                                    addMetaData.DataValue = cdMetaData.ControlId;
                                    break;
                                case "roe_FK_r_EquipmentSubType":
                                    addMetaData.DataValue = equipmentSubTypeId;
                                    break;
                                case "roeChangeTime":
                                    addMetaData.DataValue = "Now";
                                    break;
                                default:
                                    break;
                            }

                            //if (addMetaData.ColumnName.Equals("roe_FK_r_FailureCondition_RM"))
                            //{
                            //    addMetaData.DataValue = cdMetaData.ControlId;
                            //}
                            //else if (addMetaData.ColumnName.Equals("roe_FK_r_EquipmentSubType"))
                            //{
                            //    addMetaData.DataValue = equipmentSubTypeId;
                            //}
                            //else if (addMetaData.ColumnName.Equals("roeChangeTime"))
                            //{
                            //    addMetaData.DataValue = "Now";
                            //}
                        }

                        string pKey = DBEntityService.AddReferenceData(addMetaDatas);
                    }
                }

            }
        }

        public void SaveFailureCause(ReferenceDataMaintenanceEntityDTO[] allReferenceTables, long equipmentSubTypeId)
        {
            ReferenceDataMaintenanceEntityDTO ReferenceTable = allReferenceTables.Where(x => x.EntityName.Equals("r_FailureCauseByEquipmentSubType_RM")).FirstOrDefault();

            MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(ReferenceTable.EntityName);

            MetaDataReferenceData cd = new MetaDataReferenceData();

            cd.MetaData = addMetaDatas.FirstOrDefault(x => x.ColumnName.Equals("rae_FK_r_FailureCause_RM"));

            ControlIdTextDTO[] cdMetaDatas = JobAndEventService.GetMetaDataReferenceData(cd);

            ServiceJobConditionDropDownDTO[] failureCauseDTOs = EquipmentJobService.GetFailureCause(Convert.ToString(equipmentSubTypeId));

            foreach (ControlIdTextDTO cdMetaData in cdMetaDatas)
            {
                if (cdMetaData != null)
                {
                    if (failureCauseDTOs != null && failureCauseDTOs.Where(x => x.Id.Equals(cdMetaData.ControlId)).Count() == 0)
                    {
                        foreach (MetaDataDTO addMetaData in addMetaDatas.Where(x => !x.PKey && x.Required))
                        {
                            switch (addMetaData.ColumnName)
                            {
                                case "rae_FK_r_FailureCause_RM":
                                    addMetaData.DataValue = cdMetaData.ControlId;
                                    break;
                                case "rae_FK_r_EquipmentSubType":
                                    addMetaData.DataValue = equipmentSubTypeId;
                                    break;
                                case "raeChangeTime":
                                    addMetaData.DataValue = "Now";
                                    break;
                                default:
                                    break;
                            }

                            //if (addMetaData.ColumnName.Equals("rae_FK_r_FailureCause_RM"))
                            //{
                            //    addMetaData.DataValue = cdMetaData.ControlId;
                            //}
                            //else if (addMetaData.ColumnName.Equals("rae_FK_r_EquipmentSubType"))
                            //{
                            //    addMetaData.DataValue = equipmentSubTypeId;
                            //}
                            //else if (addMetaData.ColumnName.Equals("raeChangeTime"))
                            //{
                            //    addMetaData.DataValue = "Now";
                            //}
                        }

                        string pKey = DBEntityService.AddReferenceData(addMetaDatas);
                    }
                }

            }
        }

        public void SaveForeignMaterial(ReferenceDataMaintenanceEntityDTO[] allReferenceTables, long equipmentSubTypeId)
        {
            ReferenceDataMaintenanceEntityDTO ReferenceTable = allReferenceTables.Where(x => x.EntityName.Equals("r_ForeignMaterialByEquipmentSubType_RM")).FirstOrDefault();

            MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(ReferenceTable.EntityName);

            MetaDataReferenceData cd = new MetaDataReferenceData();

            cd.MetaData = addMetaDatas.FirstOrDefault(x => x.ColumnName.Equals("rfe_FK_r_ForeignMaterialRM"));

            ControlIdTextDTO[] cdMetaDatas = JobAndEventService.GetMetaDataReferenceData(cd);

            ServiceJobConditionDropDownDTO[] foreignMaterialDTOs = EquipmentJobService.GetForeignMaterial(Convert.ToString(equipmentSubTypeId));

            foreach (ControlIdTextDTO cdMetaData in cdMetaDatas)
            {
                if (cdMetaData != null)
                {
                    if (foreignMaterialDTOs != null && foreignMaterialDTOs.Where(x => x.Id.Equals(cdMetaData.ControlId)).Count() == 0)
                    {
                        foreach (MetaDataDTO addMetaData in addMetaDatas.Where(x => !x.PKey && x.Required))
                        {
                            switch (addMetaData.ColumnName)
                            {
                                case "rfe_FK_r_ForeignMaterialRM":
                                    addMetaData.DataValue = cdMetaData.ControlId;
                                    break;
                                case "rfe_FK_r_EquipmentSubType":
                                    addMetaData.DataValue = equipmentSubTypeId;
                                    break;
                                case "rfeChangeTime":
                                    addMetaData.DataValue = "Now";
                                    break;
                                default:
                                    break;
                            }
                        }

                        string pKey = DBEntityService.AddReferenceData(addMetaDatas);
                    }
                }

            }
        }

        public void SavePartAction(ReferenceDataMaintenanceEntityDTO[] allReferenceTables, long equipmentSubTypeId)
        {
            ReferenceDataMaintenanceEntityDTO ReferenceTable = allReferenceTables.Where(x => x.EntityName.Equals("r_ActionByCriticality_RM")).FirstOrDefault();

            MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(ReferenceTable.EntityName);

            CriticalityDTO[] criticalityDTOs = EquipmentJobService.GetCriticality(Convert.ToString(equipmentSubTypeId));

            foreach (CriticalityDTO criticalityDTO in criticalityDTOs)
            {
                if (criticalityDTO != null)
                {
                    ServiceJobCriticalityActionDTO[] actionDTOs = EquipmentJobService.GetActionByCriticality(Convert.ToString(criticalityDTO.Id));

                    if (actionDTOs != null && actionDTOs.Count() == 0)
                    {
                        foreach (MetaDataDTO addMetaData in addMetaDatas.Where(x => !x.PKey && x.Required))
                        {
                            switch (addMetaData.ColumnName)
                            {
                                case "racFK_r_Criticality_RM":
                                    addMetaData.DataValue = criticalityDTO.Id;
                                    break;
                                case "racFK_r_PartAction_RM":
                                    addMetaData.DataValue = equipmentSubTypeId;
                                    break;
                                case "racChangeTime":
                                    addMetaData.DataValue = "Now";
                                    break;
                                default:
                                    break;
                            }
                        }

                        //string pKey = DBEntityService.AddReferenceData(addMetaDatas);
                    }
                }

            }
        }

        public void UserManagement()
        {
            string serviceSalespersonRoleStr = "Service Salesperson Role";
            string repairServiceTechnicianRoleStr = "Repair Service Technician Role";

            #region Save User Defined Roles

            RoleDTO[] RoleDTOs = AdministrationService.GetRoles();
            RoleDTO[] serviceSalespersonRoles = RoleDTOs.Where(x => x.Name.Equals(serviceSalespersonRoleStr)).ToArray();
            RoleDTO[] repairServiceTechnicianRoles = RoleDTOs.Where(x => x.Name.Equals(repairServiceTechnicianRoleStr)).ToArray();

            this.SaveRole(serviceSalespersonRoles, serviceSalespersonRoleStr);

            this.SaveRole(repairServiceTechnicianRoles, repairServiceTechnicianRoleStr);

            #endregion

            #region Save Users

            string serviceSalesPersonUserNameStr = "Service Sales Person";
            string repairServiceTechnicianUserNameStr = "Repair Service Technician";
            UserDTO[] userDTOs = AdministrationService.GetUsers();

            RoleDTOs = AdministrationService.GetRoles();
            serviceSalespersonRoles = RoleDTOs.Where(x => x.Name.Equals(serviceSalespersonRoleStr)).ToArray();
            repairServiceTechnicianRoles = RoleDTOs.Where(x => x.Name.Equals(repairServiceTechnicianRoleStr)).ToArray();

            this.SaveUser(userDTOs, serviceSalesPersonUserNameStr, serviceSalespersonRoles);
            this.SaveUser(userDTOs, repairServiceTechnicianUserNameStr, repairServiceTechnicianRoles);

            #endregion

            #region Save System Settings

            this.SaveSystemSettings(SettingServiceStringConstants.SERVICE_TECHNICIAN_ROLE, serviceSalespersonRoleStr);
            this.SaveSystemSettings(SettingServiceStringConstants.SERVICE_TECHNICIAN_ROLE, repairServiceTechnicianRoleStr);

            #endregion
        }

        public void SaveRole(RoleDTO[] rolesDTO, string roleName)
        {
            if (rolesDTO.Count() == 0)
            {
                RoleDTO roleDTO = new RoleDTO();
                roleDTO.Name = roleName;
                AdministrationService.AddRole(roleDTO);
            }
        }

        public void SaveUser(UserDTO[] userDTOs, string userName, RoleDTO[] rolesDTOs)
        {
            UserDTO[] users = userDTOs.Where(x => x.Name.Equals(userName)).ToArray();

            UserDTO userDTO = new UserDTO();

            if (users.Count() == 0)
            {
                userDTO.Name = userName;

                userDTO.Roles = rolesDTOs.ToList();

                AdministrationService.AddUser(userDTO);
            }
            else
            {
                userDTO = users.FirstOrDefault();

                userDTO.Roles = rolesDTOs.ToList();

                AdministrationService.UpdateUser(userDTO);
            }
        }

        public void SaveSystemSettings(string systemRoleName, string userDefinedRoleName)
        {
            SystemSettingDTO systemSettingDTO = SettingService.GetSystemSettingByName(systemRoleName);
            systemSettingDTO.StringValue = userDefinedRoleName;
            SettingService.SaveSystemSetting(systemSettingDTO);
        }

        public ComponentDTO SaveSerialNumber(EquipmentSubTypeDTO equipmentSubTypeDTO, long businessOrganizationId, string serialNumber, bool IsAPIDescriptionRequired)
        {

            ComponentDTO[] componentDTOs = EquipmentJobService.GetComponentByEquipmentSubType(Convert.ToString(equipmentSubTypeDTO.EquipmentSubTypeId));

            ComponentDTO componentDTO = new ComponentDTO();

            if (componentDTOs.Where(x => x.SerialNumber.Equals(serialNumber)).Count() == 0)
            {
                //create component
                componentDTO.BusinessOrganizationId = businessOrganizationId;
                componentDTO.SerialNumber = serialNumber;

                if (IsAPIDescriptionRequired)
                {
                    componentDTO.APIDescription = "15-125-R.H.A.M-20-9-0-0";
                }

                componentDTO.PartType = equipmentSubTypeDTO.Description;
                componentDTO.EquipmentSubTypeId = equipmentSubTypeDTO.EquipmentSubTypeId;

                //save component
                componentDTO = EquipmentJobService.SaveComponent(componentDTO);
            }
            else
            {
                componentDTO = componentDTOs.Where(x => x.SerialNumber.Equals(serialNumber)).FirstOrDefault();
            }

            return componentDTO;
        }

        public WellDTO SaveWellSite(string wellName)
        {
            WellDTO[] wellDTOs = WellService.GetAllWells();

            WellDTO wellDTO = new WellDTO();

            if (wellDTOs.Where(x => x.Name.Equals(wellName)).Count() == 0)
            {
                //Create Well
                wellDTO = new WellDTO()
                {
                    Name = wellName,
                    CommissionDate = new DateTime(2018, 10, 1).ToLocalTime(),
                    WellType = WellTypeId.RRL
                };

                WellConfigDTO wellConfig = new WellConfigDTO();
                wellConfig.Well = wellDTO;
                wellConfig.ModelConfig = null;
                WellConfigDTO addedWellConfig = WellConfigurationService.AddWellConfig(wellConfig);


                wellDTO = addedWellConfig.Well;
            }
            else
            {
                wellDTO = wellDTOs.Where(x => x.Name.Equals(wellName)).LastOrDefault();
            }

            return wellDTO;
        }

        public void SaveEquipmentConfiguration(
                                                EquipmentSubTypeDTO equipmentSubTypeDTO, string equipmentTypeDescription,
                                                out bool IsAPIDescriptionRequired, out bool IsDisplayWearTabRequired)
        {
            EquipmentConfigProductStructureDTO equipConfigProductStructureDTO =
                            EquipmentConfigurationService.GetEquipmentConfigData(
                                                                                Convert.ToString(equipmentSubTypeDTO.EquipmentTypeId),
                                                                                Convert.ToString(equipmentSubTypeDTO.EquipmentSubTypeId));

            if (equipmentSubTypeDTO.RecordExistsInEquipmentConfig == false)
            {
                ///Save Equipment Configurations

                if (equipConfigProductStructureDTO.EquipmentConfig.EquipmentTypeId == 0)//If no Config Exists then we need Equipment Type and Sub Type passed for saving
                {
                    equipConfigProductStructureDTO.EquipmentConfig = new EquipmentConfigDTO();
                    equipConfigProductStructureDTO.EquipmentConfig.EquipmentTypeId = equipmentSubTypeDTO.EquipmentTypeId;
                    equipConfigProductStructureDTO.EquipmentConfig.EquipmentSubTypeId = equipmentSubTypeDTO.EquipmentSubTypeId;
                    equipConfigProductStructureDTO.EquipmentConfig.IsAPIDescriptionRequired = true;
                    equipConfigProductStructureDTO.EquipmentConfig.IsDisplayWearTabRequired = true;
                    equipConfigProductStructureDTO.EquipmentConfig.IsSingleInstallationRequired = false;
                    equipConfigProductStructureDTO.EquipmentConfig.IsSerialNumberRequired = true;

                    equipConfigProductStructureDTO.ProductStructure = new List<ProductStructureDTO>();

                    ProductStructureDTO[] productStructureDTOs = EquipmentConfigurationService.GetProductStructureData(Convert.ToString(equipmentSubTypeDTO.EquipmentTypeId));
                    ProductStructureDTO productStructureDTO = productStructureDTOs.FirstOrDefault();
                    productStructureDTO.ActionPerformed = CRUDOperationTypes.Add;

                    equipConfigProductStructureDTO.ProductStructure.Add(productStructureDTO);

                    bool flag = EquipmentConfigurationService.EquipmentConfigSaveData(equipConfigProductStructureDTO);

                    if (flag)
                    {
                        equipConfigProductStructureDTO =
                            EquipmentConfigurationService.GetEquipmentConfigData(
                                                                                Convert.ToString(equipmentSubTypeDTO.EquipmentTypeId),
                                                                                Convert.ToString(equipmentSubTypeDTO.EquipmentSubTypeId));

                        string id = equipConfigProductStructureDTO.EquipmentConfig.EquipmentConfigId.ToString();

                        EquipmentConditionDTO equipmentConditionDTO = EquipmentConfigurationService.GetEquipmentConditionData(id);

                        equipmentConditionDTO.FailureCondition1 = (int)FieldDisposition.VisibleAndRequired;
                        equipmentConditionDTO.FailureCondition2 = (int)FieldDisposition.VisibleAndRequired;
                        equipmentConditionDTO.FailureCause1 = (int)FieldDisposition.VisibleAndRequired;
                        equipmentConditionDTO.FailureCause2 = (int)FieldDisposition.VisibleAndRequired;

                        equipmentConditionDTO.PrimaryFailureCause = (int)FieldDisposition.VisibleAndRequired;

                        equipmentConditionDTO.ForeignMaterial1 = (int)FieldDisposition.VisibleAndRequired;
                        equipmentConditionDTO.ForeignMaterial2 = (int)FieldDisposition.VisibleAndRequired;

                        equipmentConditionDTO.ForeignMaterialSample = (int)FieldDisposition.VisibleAndRequired;
                        equipmentConditionDTO.Manufacturer = (int)FieldDisposition.VisibleAndRequired;
                        equipmentConditionDTO.ManufacturerPartNumber = (int)FieldDisposition.VisibleAndRequired;
                        equipmentConditionDTO.ERPPartNumber = (int)FieldDisposition.VisibleAndRequired;
                        equipmentConditionDTO.SpareStock = (int)FieldDisposition.VisibleAndRequired;
                        equipmentConditionDTO.MetalType = (int)FieldDisposition.VisibleAndRequired;
                        equipmentConditionDTO.SurfaceCoating = (int)FieldDisposition.VisibleAndRequired;

                        EquipmentConfigurationService.EquipmentConditionalDataSave(equipmentConditionDTO);
                    }
                }
            }

            IsAPIDescriptionRequired = equipConfigProductStructureDTO.EquipmentConfig.IsAPIDescriptionRequired;
            IsDisplayWearTabRequired = equipConfigProductStructureDTO.EquipmentConfig.IsDisplayWearTabRequired;
        }

        public BarrelPlungerDTO[] BarrelPlungerTabForJob(long componentId, string APIDescription)
        {
            List<BarrelPlungerDTO> barrelPlungerDTOs = EquipmentJobService.GetBarrelPlunger(Convert.ToString(componentId)).ToList();

            if (barrelPlungerDTOs.Count() > 0)
            {
                foreach (BarrelPlungerDTO barrelPlungerDTO in barrelPlungerDTOs)
                {
                    barrelPlungerDTO.Measurement = barrelPlungerDTO.Position + 1;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(APIDescription))
                {
                    string[] trimedPumpAPI = APIDescription.Split('-');

                    int barrelLength = Convert.ToInt32(trimedPumpAPI[3]);
                    int plungerLength = Convert.ToInt32(trimedPumpAPI[4]);
                    int upperExtnLength = Convert.ToInt32(trimedPumpAPI[5]);
                    int lowerExtnLength = Convert.ToInt32(trimedPumpAPI[6]);

                    for (int i = 0; i < barrelLength; i++)
                    {
                        BarrelPlungerDTO barrelPlungerDTO = new BarrelPlungerDTO();
                        barrelPlungerDTO.Position = i + 1;
                        barrelPlungerDTO.Measurement = i + 1;
                        barrelPlungerDTO.IsBarrel = true;

                        barrelPlungerDTOs.Add(barrelPlungerDTO);
                    }

                    for (int i = 0; i < plungerLength; i++)
                    {
                        BarrelPlungerDTO barrelPlungerDTO = new BarrelPlungerDTO();
                        barrelPlungerDTO.Position = i + 1;
                        barrelPlungerDTO.Measurement = i + 1;
                        barrelPlungerDTO.IsBarrel = false;

                        barrelPlungerDTOs.Add(barrelPlungerDTO);
                    }
                }

            }
            return barrelPlungerDTOs.ToArray();
        }

        public ServiceJobConditionDTO[] ConditionalDetails(string jobId, string equipmentTypeId, string equipmentSubTypeId)
        {
            CriticalityDTO criticalityDTO = new CriticalityDTO();
            ServiceJobConditionDropDownDTO failureConditionDTO = new ServiceJobConditionDropDownDTO();
            ServiceJobConditionDropDownDTO failureCauseDTO = new ServiceJobConditionDropDownDTO();
            ServiceJobConditionDropDownDTO foreignMaterialDTO = new ServiceJobConditionDropDownDTO();
            ServiceJobConditionDropDownDTO surfaceCoatingDTO = new ServiceJobConditionDropDownDTO();
            ServiceJobConditionDropDownDTO metalTypeDTO = new ServiceJobConditionDropDownDTO();
            ServiceJobCriticalityActionDTO actionDTO = new ServiceJobCriticalityActionDTO();

            List<ServiceJobConditionDTO> serviceJobConditionDTOs = new List<ServiceJobConditionDTO>();

            ServiceJobConditionWrapperDTO serviceJobConditionWrapperDTO = EquipmentJobService.GetServiceJobConditionalDetails(jobId, "false");

            #region Fetch equipment configuration

            EquipmentConfigProductStructureDTO equipConfigProductStructureDTO =
                EquipmentConfigurationService.GetEquipmentConfigData(
                                                                    Convert.ToString(equipmentTypeId),
                                                                    Convert.ToString(equipmentSubTypeId));

            string id = equipConfigProductStructureDTO.EquipmentConfig.EquipmentConfigId.ToString();

            EquipmentConditionDTO equipmentConditionDTO = EquipmentConfigurationService.GetEquipmentConditionData(id);

            #endregion

            #region Fetch dropdown values

            CriticalityDTO[] criticalityDTOs = EquipmentJobService.GetCriticality(equipmentSubTypeId);

            ServiceJobConditionDropDownDTO[] failureConditionDTOs = EquipmentJobService.GetFailureCondition(equipmentSubTypeId);

            ServiceJobConditionDropDownDTO[] failureCauseDTOs = EquipmentJobService.GetFailureCause(equipmentSubTypeId);

            ServiceJobConditionDropDownDTO[] foreignMaterialDTOs = EquipmentJobService.GetForeignMaterial(equipmentSubTypeId);

            ServiceJobConditionDropDownDTO[] surfaceCoatingDTOs = EquipmentJobService.GetSurfaceCoating();

            ServiceJobConditionDropDownDTO[] metalTypeDTOs = EquipmentJobService.GetMetalType();

            if (criticalityDTOs.Count() > 0)
            {
                criticalityDTO = criticalityDTOs.FirstOrDefault();
            }

            if (failureConditionDTOs.Count() > 0)
            {
                failureConditionDTO = failureConditionDTOs.FirstOrDefault();
            }

            if (failureCauseDTOs.Count() > 0)
            {
                failureCauseDTO = failureCauseDTOs.FirstOrDefault();
            }

            if (foreignMaterialDTOs.Count() > 0)
            {
                foreignMaterialDTO = foreignMaterialDTOs.FirstOrDefault();
            }

            if (surfaceCoatingDTOs.Count() > 0)
            {
                surfaceCoatingDTO = surfaceCoatingDTOs.FirstOrDefault();
            }

            if (metalTypeDTOs.Count() > 0)
            {
                metalTypeDTO = metalTypeDTOs.FirstOrDefault();
            }

            #endregion

            #region Fetch product structure number by ERP part

            ProductStructureDTO productStructureDTO = new ProductStructureDTO();

            productStructureDTO.PartDescription = string.Empty;
            productStructureDTO.ErpNumber = "111";
            productStructureDTO.MfgNumber = string.Empty;

            ProductStructureDTO[] productStructureDTOs = EquipmentConfigurationService.GetProductStructureBySearch(productStructureDTO);

            productStructureDTO = productStructureDTOs.FirstOrDefault();

            #endregion

            for (int i = 0; i < serviceJobConditionWrapperDTO.ServiceJobConditionData.Count(); i++)
            {
                ServiceJobConditionDTO serviceJobConditionDTO = serviceJobConditionWrapperDTO.ServiceJobConditionData[i];

                serviceJobConditionDTO.Criticality = criticalityDTO.Id;
                serviceJobConditionDTO.CriticalityDesc = criticalityDTO.ShortDescription;

                if (equipmentConditionDTO.ERPPartNumber == (int)FieldDisposition.VisibleAndRequired || equipmentConditionDTO.ERPPartNumber == (int)FieldDisposition.Visible)
                {
                    serviceJobConditionDTO.ERPPartNumber = productStructureDTO.Fk_ErpPartNumberId;
                    serviceJobConditionDTO.ERPPartNumberDesc = productStructureDTO.ErpNumber;
                }

                if (!string.IsNullOrEmpty(productStructureDTO.PartDescription))
                {
                    serviceJobConditionDTO.PartDescription = productStructureDTO.PartDescription;
                }

                if (productStructureDTO.UOMId != 0 && !string.IsNullOrEmpty(productStructureDTO.UOMDescription))
                {
                    serviceJobConditionDTO.UnitOfMeasure = productStructureDTO.UOMId;
                    serviceJobConditionDTO.UnitOfMeasureDesc = productStructureDTO.PartDescription;
                }

                if (productStructureDTO.Quantity != null)
                {
                    serviceJobConditionDTO.Quantity = Convert.ToInt32(productStructureDTO.Quantity);
                }
                else
                {
                    serviceJobConditionDTO.Quantity = 1;
                }

                if (!string.IsNullOrEmpty(productStructureDTO.MfgNumber))
                {
                    serviceJobConditionDTO.ManufacturerPartNumber = productStructureDTO.MfgNumber;
                }

                #region Assign Values To Failure Condition

                if (criticalityDTO.EnableFailureDetails && (equipmentConditionDTO.FailureCondition1 == (int)FieldDisposition.VisibleAndRequired || equipmentConditionDTO.FailureCondition1 == (int)FieldDisposition.Visible))
                {
                    serviceJobConditionDTO.FailureCondition1 = failureConditionDTO.Id;
                    serviceJobConditionDTO.FailureCondition1Desc = failureConditionDTO.Description;
                }

                if (criticalityDTO.EnableFailureDetails && (equipmentConditionDTO.FailureCondition2 == (int)FieldDisposition.VisibleAndRequired || equipmentConditionDTO.FailureCondition2 == (int)FieldDisposition.Visible))
                {
                    serviceJobConditionDTO.FailureCondition2 = failureConditionDTO.Id;
                    serviceJobConditionDTO.FailureCondition2Desc = failureConditionDTO.Description;
                }

                #endregion

                #region Assign Values To Failure Cause

                if (criticalityDTO.EnableFailureDetails && (equipmentConditionDTO.FailureCause1 == (int)FieldDisposition.VisibleAndRequired || equipmentConditionDTO.FailureCause1 == (int)FieldDisposition.Visible))
                {
                    serviceJobConditionDTO.FailureCause1 = failureCauseDTO.Id;
                    serviceJobConditionDTO.FailureCause1Desc = failureCauseDTO.Description;
                }

                if (criticalityDTO.EnableFailureDetails && (equipmentConditionDTO.FailureCause2 == (int)FieldDisposition.VisibleAndRequired || equipmentConditionDTO.FailureCause2 == (int)FieldDisposition.Visible))
                {
                    serviceJobConditionDTO.FailureCause2 = failureCauseDTO.Id;
                    serviceJobConditionDTO.FailureCause2Desc = failureCauseDTO.Description;
                }

                #endregion

                #region Assign Values To Primary Failure Cause

                if (criticalityDTO.EnableFailureDetails && (equipmentConditionDTO.PrimaryFailureCause == (int)FieldDisposition.VisibleAndRequired || equipmentConditionDTO.PrimaryFailureCause == (int)FieldDisposition.Visible))
                {
                    serviceJobConditionDTO.PrimaryFailureCause = 1;
                }

                #endregion

                #region Assign Values To Foreign Material

                if (criticalityDTO.EnableFailureDetails && (equipmentConditionDTO.ForeignMaterial1 == (int)FieldDisposition.VisibleAndRequired || equipmentConditionDTO.ForeignMaterial1 == (int)FieldDisposition.Visible))
                {
                    serviceJobConditionDTO.ForeignMaterial1 = foreignMaterialDTO.Id;
                    serviceJobConditionDTO.ForeignMaterial1Desc = foreignMaterialDTO.Description;
                }

                if (criticalityDTO.EnableFailureDetails && (equipmentConditionDTO.ForeignMaterial2 == (int)FieldDisposition.VisibleAndRequired || equipmentConditionDTO.ForeignMaterial2 == (int)FieldDisposition.Visible))
                {
                    serviceJobConditionDTO.ForeignMaterial2 = foreignMaterialDTO.Id;
                    serviceJobConditionDTO.ForeignMaterial2Desc = foreignMaterialDTO.Description;
                }

                #endregion

                #region Assign Values To Foreign Material Sample

                if (criticalityDTO.EnableFailureDetails && (equipmentConditionDTO.ForeignMaterialSample == (int)FieldDisposition.VisibleAndRequired || equipmentConditionDTO.ForeignMaterialSample == (int)FieldDisposition.Visible))
                {
                    serviceJobConditionDTO.ForeignMaterialSample = 1;
                }

                #endregion

                #region Assign Values To Metal Type & Surface Coating

                if (equipmentConditionDTO.MetalType == (int)FieldDisposition.VisibleAndRequired || equipmentConditionDTO.MetalType == (int)FieldDisposition.Visible)
                {
                    if (productStructureDTO.MetalTypeId != 0 && !string.IsNullOrEmpty(productStructureDTO.MetalTypeDescription))
                    {
                        serviceJobConditionDTO.MetalType = metalTypeDTO.Id;
                        serviceJobConditionDTO.MetalTypeDesc = metalTypeDTO.Description;
                    }
                    else
                    {
                        serviceJobConditionDTO.MetalType = metalTypeDTO.Id;
                        serviceJobConditionDTO.MetalTypeDesc = metalTypeDTO.Description;
                    }
                }

                if (equipmentConditionDTO.MetalType == (int)FieldDisposition.VisibleAndRequired || equipmentConditionDTO.MetalType == (int)FieldDisposition.Visible)
                {
                    if (productStructureDTO.UOMId != 0 && !string.IsNullOrEmpty(productStructureDTO.UOMDescription))
                    {
                        serviceJobConditionDTO.SurfaceCoating = productStructureDTO.SurfaceCoatingId;
                        serviceJobConditionDTO.SurfaceCoatingDesc = productStructureDTO.SurfaceCoatingDescription;
                    }
                    else
                    {
                        serviceJobConditionDTO.SurfaceCoating = surfaceCoatingDTO.Id;
                        serviceJobConditionDTO.SurfaceCoatingDesc = surfaceCoatingDTO.Description;
                    }
                }

                #endregion

                #region Assign Values To Manufacture

                if (equipmentConditionDTO.Manufacturer == (int)FieldDisposition.VisibleAndRequired || equipmentConditionDTO.Manufacturer == (int)FieldDisposition.Visible)
                {
                    serviceJobConditionDTO.Manufacturer = "12345";
                }

                if (equipmentConditionDTO.SpareStock == (int)FieldDisposition.VisibleAndRequired || equipmentConditionDTO.SpareStock == (int)FieldDisposition.Visible)
                {
                    ServiceJobCriticalityActionDTO[] actionDTOs = EquipmentJobService.GetActionByCriticality(Convert.ToString(criticalityDTO.Id));

                    if (actionDTOs.Count() > 0)
                    {
                        actionDTO = actionDTOs.FirstOrDefault();
                    }

                    serviceJobConditionDTO.Action = actionDTO.Id;
                    serviceJobConditionDTO.ActionDesc = actionDTO.Description;
                }

                serviceJobConditionDTO.ActionType = CRUDOperationTypes.Add;
                #endregion

                serviceJobConditionDTOs.Add(serviceJobConditionDTO);
            }

            return serviceJobConditionDTOs.ToArray();
        }

        public void UploadDocumnet(long jobId)
        {
            string Path = @"Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string fileName = "ForeSiteSplashTrans.png";
            byte[] byteArray = GetByteArray(Path, fileName);

            string base64String = Convert.ToBase64String(byteArray);

            DocumentDTO documentDTO = new DocumentDTO();
            documentDTO.SectionName = "Job";
            documentDTO.SectionKey = Convert.ToInt64(jobId);
            documentDTO.GroupingId = 1;
            documentDTO.CompleteFileName = fileName;
            documentDTO.DocumentFile = base64String;

            //Multiple Documents
            List<DocumentDTO> listDocs = new List<DocumentDTO>();
            listDocs.Add(documentDTO);
            DocumentDTO[] arrDocs = listDocs.ToArray();
            DocumentDTO[] addDocs = DocumentService.UploadDocuments(arrDocs);
        }
    }
}
