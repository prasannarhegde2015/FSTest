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
    [TestClass]
    public class EquipmentJobServiceTests : APIClientTestBase
    {
        [TestInitialize]
        public override void Init()
        {
            Trace.WriteLine("Execution started for \"Init\" method");

            base.Init();

            Trace.WriteLine("Execution ended for \"Init\" method");
        }

        #region Test Cases


        [TestCategory(TestCategories.EquipmentJobServiceTests), TestMethod]
        public void GetJobTypeByEquipment()
        {
            EquipmentTypeGroupDTO groupData = EquipmentConfigurationService.GetEquipmentGroupData();
            Assert.IsNotNull(groupData, "Equipment Group Data does not exist.");
            Assert.IsNotNull(groupData.EquipmentType, "Equipment type does not exist.");
            Assert.IsNotNull(groupData.EquipmentSubType, "Equipment sub-type does not exist.");

            EquipmentTypeDTO downholeEquipmentType = groupData.EquipmentType.Where(x => x.ConstantId == "DOWNHOLE").FirstOrDefault();
            Assert.IsNotNull(downholeEquipmentType, "Downhole Equipment type does not exist.");
            EquipmentTypeDTO surfaceEquipmentType = groupData.EquipmentType.Where(x => x.ConstantId == "SURFACE").FirstOrDefault();
            Assert.IsNotNull(surfaceEquipmentType, "Surface Equipment type does not exist.");

            long equipmentSutypeId = groupData.EquipmentSubType.OrderBy(x => x.EquipmentSubTypeId).FirstOrDefault().EquipmentSubTypeId - 1;
            JobTypeByEquipmentDTO[] jobTypeByEquipments = EquipmentJobService.GetJobTypeByEquipment(Convert.ToString(equipmentSutypeId), "true");
            Assert.AreEqual(jobTypeByEquipments.Length, 0, "Job type \"" + equipmentSutypeId + "\" does not exist.");

            equipmentSutypeId = groupData.EquipmentSubType.OrderByDescending(x => x.EquipmentSubTypeId).FirstOrDefault().EquipmentSubTypeId + 1;
            jobTypeByEquipments = EquipmentJobService.GetJobTypeByEquipment(Convert.ToString(equipmentSutypeId), "false");
            Assert.AreEqual(jobTypeByEquipments.Length, 0, "Job type \"" + equipmentSutypeId + "\" does not exist.");


            //Save Equipment Configurations
            foreach (EquipmentTypeDTO equipmentTypeDTO in groupData.EquipmentType)
            {
                IEnumerable<EquipmentSubTypeDTO> equipmentSubTypeDTOs = groupData.EquipmentSubType.Where(x => x.EquipmentTypeId == equipmentTypeDTO.EquipmentTypeId);

                foreach (EquipmentSubTypeDTO equipmentSubTypeDTO in equipmentSubTypeDTOs)
                {
                    EquipmentConfigProductStructureDTO equipConfigProductStructureDTO = EquipmentConfigurationService.GetEquipmentConfigData(Convert.ToString(equipmentSubTypeDTO.EquipmentTypeId), Convert.ToString(equipmentSubTypeDTO.EquipmentSubTypeId));

                    if (equipConfigProductStructureDTO.EquipmentConfig.EquipmentTypeId == 0)//If no Config Exists then we need Equipment Type and Sub Type passed for saving
                    {
                        equipConfigProductStructureDTO.EquipmentConfig = new EquipmentConfigDTO();
                        equipConfigProductStructureDTO.EquipmentConfig.EquipmentTypeId = equipmentTypeDTO.EquipmentTypeId;
                        equipConfigProductStructureDTO.EquipmentConfig.EquipmentSubTypeId = equipmentSubTypeDTO.EquipmentSubTypeId;

                        EquipmentConfigurationService.EquipmentConfigSaveData(equipConfigProductStructureDTO);
                    }
                }
            }

            groupData = EquipmentConfigurationService.GetEquipmentGroupData();

            foreach (EquipmentTypeDTO equipmentTypeDTO in groupData.EquipmentType)
            {
                IEnumerable<EquipmentSubTypeDTO> equipmentSubTypeDTOs = groupData.EquipmentSubType.Where(x => x.EquipmentTypeId == equipmentTypeDTO.EquipmentTypeId);

                foreach (EquipmentSubTypeDTO equipmentSubType in equipmentSubTypeDTOs)
                {
                    Assert.AreEqual(equipmentSubType.RecordExistsInEquipmentConfig, true, "Configuration does not exist for Equipment Type - " + equipmentTypeDTO.Description + ",  Equipment Subtype - " + equipmentSubType.Description);
                }
            }

        }


        //  [TestCategory(TestCategories.EquipmentJobServiceTests), TestMethod]
        public void SaveServiceJobData()
        {
            string downholeEquipmentTypeConstant = "DOWNHOLE";
            string downholeSerialNumber = "DSN_01";

            //ForeSite Toolbox user and role addition
            UserManagement();

            //Save service job for Downhole equipment
            SaveServiceJobForEquipmentSubtype(downholeEquipmentTypeConstant, downholeSerialNumber);

            //Save service job for Surface equipment
            string surfaceEquipmentTypeConstant = "SURFACE";
            string surfaceSerialNumber = "SSN_01";
            SaveServiceJobForEquipmentSubtype(surfaceEquipmentTypeConstant, surfaceSerialNumber);
        }

        [TestCategory(TestCategories.EquipmentJobServiceTests), TestMethod]
        public void SaveSerializedComponent()
        {
            string serialNumber = "DSN43";
            //create component
            List<ComponentDTO> lstcomponentdto = new List<ComponentDTO>();
            ComponentDTO component = new ComponentDTO();
            component.BusinessOrganizationId = 116;
            component.SerialNumber = serialNumber;
            component.APIDescription = "15-125-R.H.A.M-20-9-0-0";
            component.PartType = "Rod Pump";
            component.EquipmentSubTypeId = 1;

            //component added
            lstcomponentdto.Add(component);

            ComponentDTO[] componentDTOs = EquipmentJobService.GetComponentByEquipmentSubType("1");

            if (componentDTOs.Where(x => x.SerialNumber.Equals(serialNumber)).Count() == 0)
            {
                //save component
                ComponentDTO newComponent = EquipmentJobService.SaveComponent(component);

                Assert.IsNotNull(newComponent, "No Job Type present");
                Assert.AreEqual(serialNumber, newComponent.SerialNumber);

            }
        }

        // [TestCategory(TestCategories.EquipmentJobServiceTests), TestMethod]
        public void SaveHistoricalServiceJobData()
        {
            string downholeEquipmentTypeConstant = "DOWNHOLE";
            string downholeSerialNumber = "DSN_02";

            //ForeSite Toolbox user and role addition
            UserManagement();

            //Save service job for Downhole equipment
            SaveHistoricalServiceJobForEquipmentSubtype(downholeEquipmentTypeConstant, downholeSerialNumber);

            //Save service job for Surface equipment
            string surfaceEquipmentTypeConstant = "SURFACE";
            string surfaceSerialNumber = "SSN_02";
            SaveHistoricalServiceJobForEquipmentSubtype(surfaceEquipmentTypeConstant, surfaceSerialNumber);
        }


        [TestCategory(TestCategories.EquipmentJobServiceTests), TestMethod]
        public void SaveBulkDataNSJ()
        {
            RMTestData rmTestData = new RMTestData();

            rmTestData.SaveServiceJobData();
        }


        #endregion

        #region New Service Job

        public void SaveServiceJobForEquipmentSubtype(string equipmentTypeConstant, string serialNumber)
        {
            Trace.WriteLine("Execution started for \"SaveServiceJobForEquipmentSubtype\" method");

            DateTime installDate = DateTime.Now.AddMonths(-5);
            DateTime? pullDate = null;

            //Create data for Equipment Configuration and Reference data tables
            EquipmentSubTypeDTO equipmentSubTypeDTO = CreateConfigurationAndReferenceData(equipmentTypeConstant);

            //Get Equipment Config toggles
            EquipmentConfigDTO equipmentConfigDTO = EquipmentConfigurationService.GetEquipmentConfigToggles(equipmentSubTypeDTO.EquipmentTypeId.ToString(), equipmentSubTypeDTO.EquipmentSubTypeId.ToString());
            Assert.IsNotNull(equipmentConfigDTO, "Equipment Configuration does not exist for sub type - {0}", equipmentSubTypeDTO.EquipmentSubTypeId);

            //Load Business Organization
            long businessOrganizationId = EquipmentJobService.GetBuisnessOrganization().FirstOrDefault()?.Id ?? 0;
            Assert.AreNotEqual(0, businessOrganizationId, "Error retrieving Business Organization.");

            //Save Serial Number
            ComponentDTO componentDTO = SaveSerialNumber(equipmentSubTypeDTO, businessOrganizationId, serialNumber, equipmentTypeConstant == "DOWNHOLE");

            //Save Well Site
            string wellName = "TestWell1";
            WellDTO wellDTO = SaveWellSite(wellName);

            #region Create Job

            #region Create Install job

            string jobTypeConstant = "INSTALL";
            long jobTypeId = GetJobTypeIdByConstant(jobTypeConstant, equipmentSubTypeDTO, "false");

            AddJob(equipmentSubTypeDTO, componentDTO, wellDTO, jobTypeId, equipmentConfigDTO, jobTypeConstant, installDate, pullDate);

            #endregion

            #region Create Pull job

            jobTypeConstant = "PULL & REPAIR";
            jobTypeId = GetJobTypeIdByConstant(jobTypeConstant, equipmentSubTypeDTO, "false");

            pullDate = installDate.AddDays(10);

            AddJob(equipmentSubTypeDTO, componentDTO, wellDTO, jobTypeId, equipmentConfigDTO, jobTypeConstant, installDate, pullDate);

            #endregion

            #endregion

            Trace.WriteLine("Execution ended for \"SaveServiceJobForEquipmentSubtype\" method");
        }

        #region Set up configuration and reference data

        public EquipmentSubTypeDTO CreateConfigurationAndReferenceData(string equipmentTypeConstant)
        {
            #region Save Equipment Configuration

            EquipmentTypeGroupDTO groupData = EquipmentConfigurationService.GetEquipmentGroupData();
            Assert.IsNotNull(groupData, "Equipment Group Data does not exist.");
            Assert.IsNotNull(groupData.EquipmentType, "Equipment type does not exist.");
            Assert.IsNotNull(groupData.EquipmentSubType, "Equipment sub-type does not exist.");

            EquipmentTypeDTO equipmentType = groupData.EquipmentType.Where(x => x.ConstantId == equipmentTypeConstant).FirstOrDefault();
            Assert.IsNotNull(equipmentType, equipmentTypeConstant + " equipment type does not exist.");

            EquipmentSubTypeDTO equipmentSubTypeDTO = groupData.EquipmentSubType.Where(x => x.EquipmentTypeId == equipmentType.EquipmentTypeId).FirstOrDefault();
            Assert.IsNotNull(equipmentSubTypeDTO, "Equipment sub-type does not exist for " + equipmentType.Description + ".");

            //Save Equipment Configuration
            SaveEquipmentConfiguration(equipmentSubTypeDTO, equipmentType.Description);

            #endregion

            #region Add Reference Data

            AddReferenceData(groupData);

            #endregion

            return equipmentSubTypeDTO;
        }

        public void SaveEquipmentConfiguration(EquipmentSubTypeDTO equipmentSubTypeDTO, string equipmentTypeDescription)
        {
            EquipmentConfigProductStructureDTO equipConfigProductStructureDTO = EquipmentConfigurationService.GetEquipmentConfigData(
                                                                    equipmentSubTypeDTO.EquipmentTypeId.ToString(), equipmentSubTypeDTO.EquipmentSubTypeId.ToString());

            if (!equipmentSubTypeDTO.RecordExistsInEquipmentConfig)
            {
                ///Save Equipment Configuration

                if (equipConfigProductStructureDTO.EquipmentConfig.EquipmentTypeId == 0) //If no config exists then we need Equipment Type and Sub Type passed for saving
                {
                    equipConfigProductStructureDTO.EquipmentConfig = new EquipmentConfigDTO();
                    equipConfigProductStructureDTO.EquipmentConfig.EquipmentTypeId = equipmentSubTypeDTO.EquipmentTypeId;
                    equipConfigProductStructureDTO.EquipmentConfig.EquipmentSubTypeId = equipmentSubTypeDTO.EquipmentSubTypeId;
                    equipConfigProductStructureDTO.EquipmentConfig.IsAPIDescriptionRequired = (equipmentTypeDescription == "Downhole");
                    equipConfigProductStructureDTO.EquipmentConfig.IsDisplayWearTabRequired = (equipmentTypeDescription == "Downhole");
                    equipConfigProductStructureDTO.EquipmentConfig.IsSingleInstallationRequired = false;
                    equipConfigProductStructureDTO.EquipmentConfig.IsSerialNumberRequired = true;

                    equipConfigProductStructureDTO.ProductStructure = new List<ProductStructureDTO>();

                    ProductStructureDTO[] productStructureDTOs = EquipmentConfigurationService.GetProductStructureData(equipmentSubTypeDTO.EquipmentTypeId.ToString());
                    Assert.IsTrue((productStructureDTOs.Count() > 0), "No product structure reference data exists for equipment type {0}.", equipmentSubTypeDTO.EquipmentTypeId);

                    ProductStructureDTO productStructureDTO = productStructureDTOs.FirstOrDefault();
                    if (productStructureDTO != null)
                    {
                        productStructureDTO.ActionPerformed = CRUDOperationTypes.Add;
                        equipConfigProductStructureDTO.ProductStructure.Add(productStructureDTO);
                    }

                    productStructureDTO = productStructureDTOs.LastOrDefault();
                    if (productStructureDTO != null)
                    {
                        productStructureDTO.ActionPerformed = CRUDOperationTypes.Add;
                        equipConfigProductStructureDTO.ProductStructure.Add(productStructureDTO);
                    }

                    bool isEquipmentConfigSaveSuccess = EquipmentConfigurationService.EquipmentConfigSaveData(equipConfigProductStructureDTO);
                    Assert.IsTrue(isEquipmentConfigSaveSuccess, "Error saving Equipment Configuration data.");

                    equipConfigProductStructureDTO = EquipmentConfigurationService.GetEquipmentConfigData(equipmentSubTypeDTO.EquipmentTypeId.ToString(), equipmentSubTypeDTO.EquipmentSubTypeId.ToString());

                    string equipmentConfigId = equipConfigProductStructureDTO.EquipmentConfig.EquipmentConfigId.ToString();

                    EquipmentConditionDTO equipmentConditionDTO = EquipmentConfigurationService.GetEquipmentConditionData(equipmentConfigId);

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

                    isEquipmentConfigSaveSuccess = EquipmentConfigurationService.EquipmentConditionalDataSave(equipmentConditionDTO);
                    Assert.IsTrue(isEquipmentConfigSaveSuccess, "Error saving Equipment Configuration data.");

                    AdditionalInfoFieldDTO[] additionalInfoFieldDTOs = new AdditionalInfoFieldDTO[]{ new AdditionalInfoFieldDTO()
                    { DisplayLabel = "Additional Info Label 1", FieldValue = "Additional Info Value 1", ActionPerformed = CRUDOperationTypes.Add}};

                    isEquipmentConfigSaveSuccess = EquipmentConfigurationService.EquipmentAdditionalInfoDataSave(additionalInfoFieldDTOs);
                    Assert.IsTrue(isEquipmentConfigSaveSuccess, "Error saving Equipment Configuration data.");
                }

                EquipmentTypeGroupDTO groupData = EquipmentConfigurationService.GetEquipmentGroupData();

                equipmentSubTypeDTO = groupData.EquipmentSubType.Where(x => x.EquipmentSubTypeId == equipmentSubTypeDTO.EquipmentSubTypeId).FirstOrDefault();
                Assert.IsTrue(equipmentSubTypeDTO.RecordExistsInEquipmentConfig, "Configuration does not exist for equipment type - " + equipmentTypeDescription + ", equipment subtype - " + equipmentSubTypeDTO.Description);
            }
        }

        public void AddReferenceData(EquipmentTypeGroupDTO equipmentTypeGroupDTO)
        {
            /*Currently we are adding reference data for tables - BusinessOrganization, r_ServiceLocation_RM, UserServiceLocation_RM */

            ReferenceDataMaintenanceEntityDTO[] allReferenceTables = DBEntityService.GetReferenceDataMaintenanceEntities();

            AddBusinessOrgRefData(allReferenceTables);

            foreach (EquipmentTypeDTO equipmentTypeDTO in equipmentTypeGroupDTO.EquipmentType)
            {
                long serviceLocationId = AddServiceLocationRefData(equipmentTypeDTO, allReferenceTables);

                AddUserServiceLocationRefData(allReferenceTables, serviceLocationId);
            }
        }

        public void AddBusinessOrgRefData(ReferenceDataMaintenanceEntityDTO[] allReferenceTables)
        {
            BusinessOrganizationDTO[] businessOrganizationDTOs = EquipmentJobService.GetBuisnessOrganization();

            if (businessOrganizationDTOs.Count() == 0) //Add only when we do not have data through Seed
            {
                SaveBusinessOrganization(allReferenceTables);
            }
        }

        public long AddServiceLocationRefData(EquipmentTypeDTO equipmentTypeDTO, ReferenceDataMaintenanceEntityDTO[] allReferenceTables)
        {
            long serviceLocationId = 0;

            ServiceLocationDTO[] serviceLocationDTOs = EquipmentJobService.GetServiceLocation(equipmentTypeDTO.EquipmentTypeId.ToString());

            if (serviceLocationDTOs.Count() == 0)
            {
                serviceLocationId = Convert.ToInt64(this.SaveServiceLocation(allReferenceTables, equipmentTypeDTO));
            }
            else
            {
                ServiceLocationDTO serviceLocationDTO = serviceLocationDTOs.Where(x => x.FK_EquipmentTypeId == equipmentTypeDTO.EquipmentTypeId).FirstOrDefault();
                serviceLocationId = serviceLocationDTO.ServiceLocationId;
            }

            return serviceLocationId;
        }

        public void AddUserServiceLocationRefData(ReferenceDataMaintenanceEntityDTO[] allReferenceTables, long serviceLocationId)
        {
            string repairServiceTechnicianUserNameStr = "Repair Service Technician";

            UserDTO[] userDTOs = AdministrationService.GetUsers();

            UserDTO[] repairServieTechDTOs = EquipmentJobService.GetServiceTechnician(serviceLocationId.ToString());

            if (repairServieTechDTOs.Count() == 0)
            {
                UserDTO userDTO = userDTOs.Where(x => x.Name.Equals(repairServiceTechnicianUserNameStr)).FirstOrDefault();
                this.SaveUserServiceLocation(allReferenceTables, userDTO.Id, serviceLocationId);
            }
        }

        public void SaveBusinessOrganization(ReferenceDataMaintenanceEntityDTO[] allReferenceTables)
        {
            ReferenceDataMaintenanceEntityDTO referenceTable = allReferenceTables.Where(x => x.EntityName.Equals("BusinessOrganization")).FirstOrDefault();
            Assert.IsNotNull(referenceTable, "Reference Data does not have an entry for BusinessOrganization table.");

            MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(referenceTable.EntityName);

            MetaDataReferenceData cd = new MetaDataReferenceData();
            cd.MetaData = addMetaDatas.FirstOrDefault(x => x.ReferenceDataSource.Equals("POP"));

            ControlIdTextDTO[] cdMetaDatas = JobAndEventService.GetMetaDataReferenceData(cd);

            foreach (MetaDataDTO addMetaData in addMetaDatas.Where(x => !x.PKey && x.Required))
            {
                if (addMetaData.ColumnName.Equals("venFK_r_BusinessOrganizationType"))
                {
                    addMetaData.DataValue = 6;
                }
                else if (addMetaData.ColumnName.Equals("venFK_r_StateProvince"))
                {
                    addMetaData.DataValue = 1;
                }
                else if (addMetaData.ColumnName.Equals("venFK_r_Country"))
                {
                    addMetaData.DataValue = 1;
                }
                else if (addMetaData.ColumnName.Equals("venBusinessOrganizationName"))
                {
                    addMetaData.DataValue = "Test BU";
                }
                else if (addMetaData.ColumnName.Equals("venTaxRate"))
                {
                    addMetaData.DataValue = 0.00;
                }
                else if (addMetaData.ColumnName.Equals("venDftExpGL"))
                {
                    addMetaData.DataValue = 0.00;
                }
                else if (addMetaData.ColumnName.Equals("venPORequired"))
                {
                    addMetaData.DataValue = 0;
                }
                else if (addMetaData.ColumnName.Equals("venConstantId"))
                {
                    addMetaData.DataValue = "Test BU";
                }
                else if (addMetaData.ColumnName.Equals("venChangeTime"))
                {
                    addMetaData.DataValue = "Now";
                }
            }

            string pKey = DBEntityService.AddReferenceData(addMetaDatas);
            Assert.IsNotNull(pKey, "Error adding Business organization reference data.");
        }

        public string SaveServiceLocation(ReferenceDataMaintenanceEntityDTO[] allReferenceTables, EquipmentTypeDTO equipmentTypeDTO)
        {
            ReferenceDataMaintenanceEntityDTO referenceTable = allReferenceTables.Where(x => x.EntityName.Equals("r_ServiceLocation_RM")).FirstOrDefault();
            Assert.IsNotNull(referenceTable, "Reference Data does not have an entry for r_ServiceLocation_RM table.");

            MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(referenceTable.EntityName);

            MetaDataReferenceData cd = new MetaDataReferenceData();
            cd.MetaData = addMetaDatas.FirstOrDefault(x => x.ColumnName.Equals("rslFK_r_EquipmentType"));

            ControlIdTextDTO[] cdMetaDatas = JobAndEventService.GetMetaDataReferenceData(cd);

            foreach (MetaDataDTO addMetaData in addMetaDatas.Where(x => !x.PKey && x.Required))
            {
                if (addMetaData.ColumnName.Equals("rslFK_r_EquipmentType"))
                {
                    addMetaData.DataValue = equipmentTypeDTO.EquipmentTypeId;
                }
                else if (addMetaData.ColumnName.Equals("rslCompanyName"))
                {
                    addMetaData.DataValue = "WFT Company";
                }
                else if (addMetaData.ColumnName.Equals("rslLocation"))
                {
                    addMetaData.DataValue = equipmentTypeDTO.Description + " Location";
                }
                else if (addMetaData.ColumnName.Equals("rslChangeTime"))
                {
                    addMetaData.DataValue = "Now";
                }
            }

            string pKey = DBEntityService.AddReferenceData(addMetaDatas);
            Assert.IsNotNull(pKey, "Error adding Service location reference data.");

            return pKey;
        }

        public void SaveUserServiceLocation(ReferenceDataMaintenanceEntityDTO[] allReferenceTables, long userId, long serviceLocationId)
        {
            ReferenceDataMaintenanceEntityDTO referenceTable = allReferenceTables.Where(x => x.EntityName.Equals("UserServiceLocation_RM")).FirstOrDefault();
            Assert.IsNotNull(referenceTable, "Reference Data does not have an entry for UserServiceLocation_RM table.");

            MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(referenceTable.EntityName);

            foreach (MetaDataDTO addMetaData in addMetaDatas.Where(x => !x.PKey && x.Required))
            {
                if (addMetaData.ColumnName.Equals("uslFK_User"))
                {
                    addMetaData.DataValue = userId;
                }
                else if (addMetaData.ColumnName.Equals("uslFK_r_ServiceLocation_RM"))
                {
                    addMetaData.DataValue = serviceLocationId;
                }
                else if (addMetaData.ColumnName.Equals("uslChangeTime"))
                {
                    addMetaData.DataValue = "Now";
                }
            }

            string pKey = DBEntityService.AddReferenceData(addMetaDatas);
            Assert.IsNotNull(pKey, "Error adding User Service location reference data.");
        }

        public long AddJobTypeForSubType(string jobTypeConstant, EquipmentSubTypeDTO equipmentSubTypeDTO)
        {
            Trace.WriteLine("Execution started for \"AddJobTypeForSubType\" method");

            ReferenceDataMaintenanceEntityDTO[] allReferenceTables = DBEntityService.GetReferenceDataMaintenanceEntities();

            ReferenceDataMaintenanceEntityDTO referenceTable = allReferenceTables.Where(x => x.EntityName.Equals("r_JobTypeByEquipment")).FirstOrDefault();
            Assert.IsNotNull(referenceTable, "Reference Data does not have an entry for r_JobTypeByEquipment table.");

            MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(referenceTable.EntityName);

            #region Get Job Type ID

            long jobTypeId = 0;

            JobTypeDTO[] jobTypeDTOs = JobAndEventService.GetJobTypes().Where(x => x.JobType.ToUpper().Equals(jobTypeConstant)).ToArray();

            if (jobTypeDTOs != null && jobTypeDTOs.Length > 0)
            {
                JobTypeDTO jobTypeDTO = jobTypeDTOs.FirstOrDefault();
                jobTypeId = jobTypeDTO.id;
            }

            MetaDataReferenceData cd = new MetaDataReferenceData();
            cd.MetaData = addMetaDatas.FirstOrDefault(x => x.ColumnName.Equals("jtqFK_r_JobType"));

            ControlIdTextDTO[] cdMetaDatas = JobAndEventService.GetMetaDataReferenceData(cd);

            #endregion



            foreach (MetaDataDTO addMetaData in addMetaDatas.Where(x => !x.PKey))
            {
                if (addMetaData.ColumnName.Equals("jtqFK_r_JobType"))
                {
                    addMetaData.DataValue = jobTypeId;
                }
                else if (addMetaData.ColumnName.Equals("jtqFK_r_EquipmentSubType"))
                {
                    addMetaData.DataValue = equipmentSubTypeDTO.EquipmentSubTypeId;
                }
                else if (addMetaData.ColumnName.Equals("jtqJobDateMandatory"))
                {
                    addMetaData.DataValue = true;
                }
                else if (addMetaData.ColumnName.Equals("jtqPullDateMandatory"))
                {
                    addMetaData.DataValue = (jobTypeConstant == "PULL & REPAIR") ? true : false;
                }
                else if (addMetaData.ColumnName.Equals("jtqRepairDateMandatory"))
                {
                    addMetaData.DataValue = (jobTypeConstant == "PULL & REPAIR") ? true : false;
                }
                else if (addMetaData.ColumnName.Equals("jtqInstallDateMandatory"))
                {
                    addMetaData.DataValue = (jobTypeConstant == "INSTALL") ? true : false;
                }
                else if (addMetaData.ColumnName.Equals("jtqWellsiteMandatory"))
                {
                    addMetaData.DataValue = (jobTypeConstant == "INSTALL" || jobTypeConstant == "PULL & REPAIR") ? true : false;
                }
                else if (addMetaData.ColumnName.Equals("rslChangeTime"))
                {
                    addMetaData.DataValue = "Now";
                }
            }

            string pKey = DBEntityService.AddReferenceData(addMetaDatas);
            Assert.IsNotNull(pKey, "Error adding User Service location reference data.");

            Trace.WriteLine("Execution ended for \"AddJobTypeForSubType\" method");

            return jobTypeId;
        }

        #endregion

        public WellDTO SaveWellSite(string wellName)
        {
            WellDTO[] wellDTOs = WellService.GetAllWells();
            Assert.IsNotNull(wellDTOs, "Well site does not exist.");

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

                Trace.WriteLine("New well created = " + wellName);
            }
            else
            {
                wellDTO = wellDTOs.Where(x => x.Name.Equals(wellName)).LastOrDefault();

                Trace.WriteLine("Fetched data for " + wellName + " well");
            }

            Assert.IsNotNull(wellDTO, "Well site does not exist.");

            return wellDTO;
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

                Trace.WriteLine("New component created = " + serialNumber);
            }
            else
            {
                componentDTO = componentDTOs.Where(x => x.SerialNumber.Equals(serialNumber)).FirstOrDefault();
                Assert.IsNotNull(componentDTO, "Serial Number does not exist.");

                Trace.WriteLine("Fetch from existing component = " + serialNumber);
            }

            return componentDTO;
        }

        public void AddJob(EquipmentSubTypeDTO equipmentSubTypeDTO, ComponentDTO componentDTO, WellDTO wellDTO, long jobTypeId, EquipmentConfigDTO equipmentConfigDTO, string jobTypeConstant, DateTime? installDate, DateTime? pullDate)
        {
            Trace.WriteLine("Execution started for \"AddJob\" method");
            Trace.WriteLine("Job type = " + jobTypeConstant);

            ServiceJobDTO serviceJobDTO = new ServiceJobDTO();

            serviceJobDTO.ServiceRMJobDTO = new ServiceRMJobDTO();
            serviceJobDTO.JobBridgeDTO = new JobBridgeRMDTO();
            serviceJobDTO.BarrelPlungerDTO = new List<BarrelPlungerDTO>().ToArray();
            serviceJobDTO.ConditionalInfoDTO = new List<ServiceJobConditionDTO>().ToArray();
            serviceJobDTO.AdditionalInfoDTO = new List<AdditionalInfoFieldDTO>().ToArray();

            #region 1st Save of Multiple Saves
            SetDataForStartTab(serviceJobDTO, equipmentSubTypeDTO, componentDTO, wellDTO, jobTypeId, installDate, pullDate);
            SetDataForBothServiceTabs(serviceJobDTO, equipmentSubTypeDTO, jobTypeId);

            ServiceJobDetailsDTO serviceJobDetailsDTO = new ServiceJobDetailsDTO();
            serviceJobDetailsDTO.ComponentId = componentDTO.Id;
            if (installDate != null)
            {
                serviceJobDetailsDTO.InstallDate = installDate;
            }

            serviceJobDetailsDTO.JobTypeConstant = jobTypeConstant;

            if (pullDate != null)
            {
                serviceJobDetailsDTO.PullDate = pullDate;
            }

            serviceJobDetailsDTO.PumpSerialNumber = componentDTO.Name;
            //serviceJobDetailsDTO.ReceivedAtShopDate = null;
            //serviceJobDetailsDTO.RepairDate = null;
            serviceJobDetailsDTO.WellNumber = wellDTO.Name;

            NewServiceJobValidationDTO newServiceJobValidationDTO = EquipmentJobService.ValidateHistoricalJob(serviceJobDetailsDTO);

            if (newServiceJobValidationDTO.CanProceed)
            {
                serviceJobDTO.ServiceRMJobDTO.JobId = EquipmentJobService.SaveServiceJobData(serviceJobDTO);

                //Test saved data for job till first multiple save
                GetDataForStartAndServiceTabs(serviceJobDTO);

                #region 2nd Save of Multiple Saves

                if (equipmentConfigDTO.IsDisplayWearTabRequired)
                    SetDataForBarrelAndPlungerTab(serviceJobDTO, componentDTO);

                SetDataForConditionTab(serviceJobDTO, equipmentSubTypeDTO);

                serviceJobDTO.IsConditionalSave = true;
                EquipmentJobService.SaveServiceJobData(serviceJobDTO);

                #endregion

                #region 3rd Save of Multiple Saves

                SetDataForAdditionalInfoTab(serviceJobDTO, equipmentSubTypeDTO);
                serviceJobDTO.IsConditionalSave = false;
                EquipmentJobService.SaveServiceJobData(serviceJobDTO);

                #endregion

                UploadDocument(serviceJobDTO.ServiceRMJobDTO.JobId);

                Trace.WriteLine("Execution ended for \"AddJob\" method");
            }

            #endregion
        }

        #region Set data for different tabs in New Service Job

        public void SetDataForStartTab(ServiceJobDTO serviceJobDTO, EquipmentSubTypeDTO equipmentSubTypeDTO, ComponentDTO componentDTO, WellDTO wellDTO, long jobTypeId, DateTime? installDate, DateTime? pullDate)
        {
            Trace.WriteLine("Execution started for \"SetDataForStartTab\" method");

            serviceJobDTO.JobBridgeDTO.EquipmentTypeId = equipmentSubTypeDTO.EquipmentTypeId;
            serviceJobDTO.JobBridgeDTO.EquipmentSubTypeId = equipmentSubTypeDTO.EquipmentSubTypeId;
            serviceJobDTO.ServiceRMJobDTO.JobTypeId = jobTypeId;

            serviceJobDTO.JobBridgeDTO.ComponentId = componentDTO.Id;
            serviceJobDTO.JobBridgeDTO.PumpAPI = componentDTO.APIDescription;

            serviceJobDTO.ServiceRMJobDTO.AssemblyId = wellDTO.AssemblyId;

            if (installDate != null)
            {
                serviceJobDTO.ServiceRMJobDTO.InstallDate = Convert.ToDateTime(installDate);
            }

            if (pullDate != null)
            {
                serviceJobDTO.ServiceRMJobDTO.PullDate = Convert.ToDateTime(pullDate);
            }

            Trace.WriteLine("Execution ended for \"SetDataForStartTab\" method");
        }

        public void SetDataForBothServiceTabs(ServiceJobDTO serviceJobDTO, EquipmentSubTypeDTO equipmentSubTypeDTO, long jobTypeId)
        {
            JobStatusDTO jobStatusDTO = JobAndEventService.GetJobStatuses()?.FirstOrDefault();
            Assert.IsNotNull(jobStatusDTO, "Job Status does not exist.");

            JobReasonDTO jobReasonDTO = JobAndEventService.GetJobReasonsForJobType(Convert.ToString(jobTypeId))?.FirstOrDefault();
            Assert.IsNotNull(jobReasonDTO, "Job reason does not exist.");

            ServiceLocationDTO serviceLocationDTO = EquipmentJobService.GetServiceLocation(Convert.ToString(equipmentSubTypeDTO.EquipmentTypeId))?.FirstOrDefault();
            Assert.IsNotNull(serviceLocationDTO, "Service location does not exist.");

            UserDTO repairServiceTechnicianDTO = EquipmentJobService.GetServiceTechnician(Convert.ToString(serviceLocationDTO.ServiceLocationId))?.FirstOrDefault();
            Assert.IsNotNull(repairServiceTechnicianDTO, "Repair Service Technician does not exist.");

            //Assign data for Service tab
            serviceJobDTO.ServiceRMJobDTO.JobBegDateTime = DateTime.Now.AddMonths(-5);
            serviceJobDTO.ServiceRMJobDTO.JobEndDateTime = DateTime.Now.AddMonths(-5);
            serviceJobDTO.JobBridgeDTO.ServiceLocationId = serviceLocationDTO.ServiceLocationId;
            serviceJobDTO.JobBridgeDTO.ServiceUserId = repairServiceTechnicianDTO.Id;
            serviceJobDTO.ServiceRMJobDTO.OriginKey = "QN 1";
            serviceJobDTO.ServiceRMJobDTO.AccountingRef = "Billing Information 1";

            //Assign data for Service info tab
            serviceJobDTO.JobBridgeDTO.WONumber = "ERP WO 1";
            serviceJobDTO.JobBridgeDTO.FieldTicketNumber = "FTN 1";
            serviceJobDTO.JobBridgeDTO.RepairUserId = repairServiceTechnicianDTO.Id;
            serviceJobDTO.ServiceRMJobDTO.JobReasonId = jobReasonDTO.Id;
            serviceJobDTO.ServiceRMJobDTO.StatusId = jobStatusDTO.Id;
        }

        public void SetDataForBarrelAndPlungerTab(ServiceJobDTO serviceJobDTO, ComponentDTO componentDTO)
        {
            List<BarrelPlungerDTO> barrelPlungerDTOs = EquipmentJobService.GetBarrelPlunger(Convert.ToString(componentDTO.Id)).ToList();

            if (barrelPlungerDTOs.Count() > 0)
            {
                foreach (BarrelPlungerDTO barrelPlungerDTO in barrelPlungerDTOs)
                {
                    barrelPlungerDTO.Measurement = barrelPlungerDTO.Position + 1;
                }
            }
            else
            {
                string[] trimmedPumpAPI = componentDTO.APIDescription.Split('-');

                int barrelLength = Convert.ToInt32(trimmedPumpAPI[3]);
                int plungerLength = Convert.ToInt32(trimmedPumpAPI[4]);
                int upperExtnLength = Convert.ToInt32(trimmedPumpAPI[5]);
                int lowerExtnLength = Convert.ToInt32(trimmedPumpAPI[6]);

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

            serviceJobDTO.BarrelPlungerDTO = barrelPlungerDTOs.ToArray();
        }

        public void SetDataForConditionTab(ServiceJobDTO serviceJobDTO, EquipmentSubTypeDTO equipmentSubTypeDTO)
        {
            CriticalityDTO criticalityDTO = new CriticalityDTO();
            ServiceJobConditionDropDownDTO failureConditionDTO = new ServiceJobConditionDropDownDTO();
            ServiceJobConditionDropDownDTO failureCauseDTO = new ServiceJobConditionDropDownDTO();
            ServiceJobConditionDropDownDTO foreignMaterialDTO = new ServiceJobConditionDropDownDTO();
            ServiceJobConditionDropDownDTO surfaceCoatingDTO = new ServiceJobConditionDropDownDTO();
            ServiceJobConditionDropDownDTO metalTypeDTO = new ServiceJobConditionDropDownDTO();
            ServiceJobCriticalityActionDTO actionDTO = new ServiceJobCriticalityActionDTO();

            List<ServiceJobConditionDTO> serviceJobConditionDTOs = new List<ServiceJobConditionDTO>();

            ServiceJobConditionWrapperDTO serviceJobConditionWrapperDTO = EquipmentJobService.GetServiceJobConditionalDetails(serviceJobDTO.ServiceRMJobDTO.JobId.ToString(), "false");

            #region Fetch equipment configuration

            EquipmentConfigProductStructureDTO equipConfigProductStructureDTO =
                EquipmentConfigurationService.GetEquipmentConfigData(equipmentSubTypeDTO.EquipmentTypeId.ToString(), equipmentSubTypeDTO.EquipmentSubTypeId.ToString());

            string equipmentConfigId = equipConfigProductStructureDTO.EquipmentConfig.EquipmentConfigId.ToString();

            EquipmentConditionDTO equipmentConditionDTO = EquipmentConfigurationService.GetEquipmentConditionData(equipmentConfigId);

            #endregion

            #region Fetch dropdown values

            CriticalityDTO[] criticalityDTOs = EquipmentJobService.GetCriticality(equipmentSubTypeDTO.EquipmentSubTypeId.ToString());

            ServiceJobConditionDropDownDTO[] failureConditionDTOs = EquipmentJobService.GetFailureCondition(equipmentSubTypeDTO.EquipmentSubTypeId.ToString());

            ServiceJobConditionDropDownDTO[] failureCauseDTOs = EquipmentJobService.GetFailureCause(equipmentSubTypeDTO.EquipmentSubTypeId.ToString());

            ServiceJobConditionDropDownDTO[] foreignMaterialDTOs = EquipmentJobService.GetForeignMaterial(equipmentSubTypeDTO.EquipmentSubTypeId.ToString());

            ServiceJobConditionDropDownDTO[] surfaceCoatingDTOs = EquipmentJobService.GetSurfaceCoating();

            ServiceJobConditionDropDownDTO[] metalTypeDTOs = EquipmentJobService.GetMetalType();

            if (criticalityDTOs.Count() > 0)
            {
                criticalityDTO = criticalityDTOs.FirstOrDefault();
            }

            if (failureConditionDTOs.Count() > 0 && criticalityDTO.EnableFailureDetails)
            {
                failureConditionDTO = failureConditionDTOs.FirstOrDefault();
            }

            if (failureCauseDTOs.Count() > 0 && criticalityDTO.EnableFailureDetails)
            {
                failureCauseDTO = failureCauseDTOs.FirstOrDefault();
            }

            if (foreignMaterialDTOs.Count() > 0 && criticalityDTO.EnableFailureDetails)
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

                #region Assign Values To Manufacturer

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
        }

        public void SetDataForAdditionalInfoTab(ServiceJobDTO serviceJobDTO, EquipmentSubTypeDTO equipmentSubTypeDTO)
        {
            AdditionalInfoFieldDTO[] additionalInfoLabels = EquipmentJobService.GetAdditionalInfoLabels(equipmentSubTypeDTO.EquipmentTypeId.ToString(), equipmentSubTypeDTO.EquipmentSubTypeId.ToString());

            if (additionalInfoLabels.Count() > 0)
            {
                AdditionalInfoFieldDTO additionalInfoFieldDTO = new AdditionalInfoFieldDTO();
                additionalInfoFieldDTO.EquipmentConfigAdditionalInfoId = additionalInfoLabels.FirstOrDefault().EquipmentConfigAdditionalInfoId;
                additionalInfoFieldDTO.FieldValue = "Additional Info Text Value";
            }

            serviceJobDTO.JobBridgeDTO.ServiceJobRemarks = "Additional Info - Detailed Job Notes";
        }

        #endregion

        public void UploadDocument(long jobId)
        {
            string Path = @"Weatherford.POP.Server.IntegrationTests.TestDocuments.";
            string fileName = "ForeSiteSplashTrans.png";
            byte[] byteArray = GetByteArray(Path, fileName);
            Assert.IsNotNull(byteArray);
            string base64String = Convert.ToBase64String(byteArray);

            #region Upload Document

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

            #endregion

            #region Delete Document 

            DocumentSectionDTO ds = new DocumentSectionDTO();
            ds.SectionKey = Convert.ToInt64(jobId);
            ds.SectionName = "Job";

            DocumentGroupingDTO[] allDocuments = DocumentService.GetAllDocuments(ds);

            List<DocumentDTO> docs = new List<DocumentDTO>();
            foreach (DocumentGroupingDTO dg in allDocuments)
            {
                foreach (DocumentDTO d in dg.Documents)
                {
                    docs.Add(d);
                }
            }

            //Delete
            DocumentDTO[] deleteDocs = DocumentService.DeleteDocuments(docs.ToArray());
            foreach (DocumentDTO document in deleteDocs)
            {
                Assert.IsTrue(document.UploadDownloadStatus, "Failed to delete Documents");
            }

            #endregion
        }

        #region Test data after save for different tabs in New Service Job

        public void GetDataForStartAndServiceTabs(ServiceJobDTO serviceJobDTO)
        {
            ServiceJobDetailsDTO serviceJobDetailsDTO = EquipmentJobService.GetServiceJobDetails(serviceJobDTO.ServiceRMJobDTO.JobId.ToString());
            Assert.AreEqual(serviceJobDetailsDTO.JobNumber, serviceJobDTO.ServiceRMJobDTO.JobId.ToString());
            Assert.AreEqual(serviceJobDetailsDTO.JobDate.Date, serviceJobDTO.ServiceRMJobDTO.JobBegDateTime.Date);
            Assert.AreEqual(serviceJobDetailsDTO.JobTypeId, serviceJobDTO.ServiceRMJobDTO.JobTypeId);
            Assert.AreEqual(serviceJobDetailsDTO.ReasonforServiceId, serviceJobDTO.ServiceRMJobDTO.JobReasonId);
            Assert.AreEqual(serviceJobDetailsDTO.JobStatusId, serviceJobDTO.ServiceRMJobDTO.StatusId);
            Assert.AreEqual(serviceJobDetailsDTO.CustomerID, serviceJobDTO.ServiceRMJobDTO.BusinessOrganizationId);
            if (serviceJobDetailsDTO.JobTypeConstant == "INSTALL")
                Assert.AreEqual((serviceJobDetailsDTO.InstallDate ?? DateTime.MinValue).Date, serviceJobDTO.ServiceRMJobDTO.InstallDate.Date);
            if (serviceJobDetailsDTO.JobTypeConstant == "PULL & REPAIR")
                Assert.AreEqual((serviceJobDetailsDTO.PullDate ?? DateTime.MinValue).Date, serviceJobDTO.ServiceRMJobDTO.PullDate);
            Assert.AreEqual(serviceJobDetailsDTO.Note, serviceJobDTO.ServiceRMJobDTO.Remarks);
            Assert.AreEqual(serviceJobDetailsDTO.AssemblyId, serviceJobDTO.ServiceRMJobDTO.AssemblyId ?? 0);
            Assert.AreEqual(serviceJobDetailsDTO.BillingInformation, serviceJobDTO.ServiceRMJobDTO.AccountingRef);
            Assert.AreEqual(serviceJobDetailsDTO.QuoteNumber, serviceJobDTO.ServiceRMJobDTO.OriginKey);
            Assert.AreEqual(serviceJobDetailsDTO.EquipmentTypeId, serviceJobDTO.JobBridgeDTO.EquipmentTypeId);
            Assert.AreEqual(serviceJobDetailsDTO.EquipmentSubTypeId, serviceJobDTO.JobBridgeDTO.EquipmentSubTypeId);
            Assert.AreEqual(serviceJobDetailsDTO.ComponentId, serviceJobDTO.JobBridgeDTO.ComponentId);
            Assert.AreEqual(serviceJobDetailsDTO.serviceLocationId, serviceJobDTO.JobBridgeDTO.ServiceLocationId ?? 0);
            Assert.AreEqual(serviceJobDetailsDTO.ServiceSalesPersonId ?? 0, serviceJobDTO.JobBridgeDTO.ServiceUserId ?? 0);
            Assert.AreEqual(serviceJobDetailsDTO.RepairServiceTechnicianId, serviceJobDTO.JobBridgeDTO.RepairUserId ?? 0);
            Assert.AreEqual(serviceJobDetailsDTO.ManufacturerId ?? 0, serviceJobDTO.JobBridgeDTO.SurfaceUnitManufacturerId);
            Assert.AreEqual(serviceJobDetailsDTO.SurfaceUnitBrandId ?? 0, serviceJobDTO.JobBridgeDTO.SurfaceUnitBrandId ?? 0);
            Assert.AreEqual(serviceJobDetailsDTO.ERPWorkOrderNumber, serviceJobDTO.JobBridgeDTO.WONumber);
            Assert.AreEqual(serviceJobDetailsDTO.FieldTicketNumber, serviceJobDTO.JobBridgeDTO.FieldTicketNumber);
            Assert.AreEqual((serviceJobDetailsDTO.ReceivedAtShopDate ?? DateTime.MinValue).Date, serviceJobDTO.JobBridgeDTO.ReceivedDate.Date);
            Assert.AreEqual((serviceJobDetailsDTO.RepairDate ?? DateTime.MinValue).Date, serviceJobDTO.JobBridgeDTO.RepairDate);
            Assert.AreEqual(serviceJobDetailsDTO.DetailedJobNotes, serviceJobDTO.JobBridgeDTO.ServiceJobRemarks);
        }

        #endregion

        public long GetJobTypeIdByConstant(string jobTypeConstant, EquipmentSubTypeDTO equipmentSubTypeDTO, string isHistorical)
        {

            Trace.WriteLine("Execution started for \"GetJobTypeIdByConstant\" method");

            Trace.WriteLine("EquipmentSubType = " + equipmentSubTypeDTO.Description + "JobTypeConstant = " + jobTypeConstant + ", IsHistorical = " + isHistorical);

            long jobTypeId = 0;

            JobTypeByEquipmentDTO[] jobTypeByEquipments = EquipmentJobService.GetJobTypeByEquipment(equipmentSubTypeDTO.EquipmentSubTypeId.ToString(), isHistorical);
            Assert.IsNotNull(jobTypeByEquipments, "Job type does not exist.");

            JobTypeByEquipmentDTO jobTypeByEquipment = jobTypeByEquipments.Where(x => x.JobTypeConstant == jobTypeConstant).FirstOrDefault();

            if (jobTypeByEquipment == null)
                jobTypeId = AddJobTypeForSubType(jobTypeConstant, equipmentSubTypeDTO);
            else
                jobTypeId = jobTypeByEquipment.FK_JobTypeId;

            if (!Convert.ToBoolean(isHistorical) || (Convert.ToBoolean(isHistorical) && !jobTypeConstant.Equals("INSTALL & PULL")))
            {
                Assert.AreNotEqual(jobTypeId, 0, jobTypeConstant + " job type does not exist.");
            }

            EquipmentConfigDTO equipmentConfig = EquipmentConfigurationService.GetEquipmentConfigToggles(Convert.ToString(equipmentSubTypeDTO.EquipmentTypeId), Convert.ToString(equipmentSubTypeDTO.EquipmentSubTypeId));
            Assert.IsNotNull(equipmentConfig, "Downhole Equipment configuration does not exist.");

            Trace.WriteLine("Execution ended for \"GetJobTypeIdByConstant\" method");

            return jobTypeId;
        }

        #region User Management

        public void UserManagement()
        {
            #region Save User Defined Roles

            string repairServiceTechnicianRoleStr = "Repair Service Technician Role";
            string customerRoleStr = "Customer Role";

            RoleDTO[] roleDTOs = AdministrationService.GetRoles();
            RoleDTO[] repairServiceTechnicianRoles = roleDTOs.Where(x => x.Name.Equals(repairServiceTechnicianRoleStr)).ToArray();
            RoleDTO[] customerRoles = roleDTOs.Where(x => x.Name.Equals(customerRoleStr)).ToArray();

            this.SaveRole(repairServiceTechnicianRoles, repairServiceTechnicianRoleStr);
            this.SaveRole(customerRoles, customerRoleStr);

            #endregion

            #region Save Users

            string repairServiceTechnicianUserNameStr = "Repair Service Technician";
            string customerUserNameStr = "Customer User";

            UserDTO[] userDTOs = AdministrationService.GetUsers();

            roleDTOs = AdministrationService.GetRoles();
            repairServiceTechnicianRoles = roleDTOs.Where(x => x.Name.Equals(repairServiceTechnicianRoleStr)).ToArray();
            customerRoles = roleDTOs.Where(x => x.Name.Equals(customerRoleStr)).ToArray();

            this.SaveUser(userDTOs, customerUserNameStr, customerRoles);
            this.SaveUser(userDTOs, repairServiceTechnicianUserNameStr, repairServiceTechnicianRoles);

            #endregion

            #region Save System Settings

            this.SaveSystemSettings(SettingServiceStringConstants.SERVICE_TECHNICIAN_ROLE, repairServiceTechnicianRoleStr);
            this.SaveSystemSettings(SettingServiceStringConstants.CUSTOMER_ROLE, customerRoleStr);

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

        #endregion

        #endregion

        #region Historical Service Job

        public void SaveHistoricalServiceJobForEquipmentSubtype(string equipmentTypeConstant, string serialNumber)
        {
            DateTime installDate = DateTime.Now.AddMonths(-5);
            DateTime? pullDate = null;

            //Create data for Equipment Configuration and Reference data tables
            EquipmentSubTypeDTO equipmentSubTypeDTO = CreateConfigurationAndReferenceData(equipmentTypeConstant);

            //Get Equipment Config toggles
            EquipmentConfigDTO equipmentConfigDTO = EquipmentConfigurationService.GetEquipmentConfigToggles(equipmentSubTypeDTO.EquipmentTypeId.ToString(), equipmentSubTypeDTO.EquipmentSubTypeId.ToString());
            Assert.IsNotNull(equipmentConfigDTO, "Equipment Configuration does not exist for sub type - {0}", equipmentSubTypeDTO.EquipmentSubTypeId);

            //Load Business Organization
            long businessOrganizationId = EquipmentJobService.GetBuisnessOrganization().FirstOrDefault()?.Id ?? 0;
            Assert.AreNotEqual(0, businessOrganizationId, "Error retrieving Business Organization.");

            //Save Serial Number
            ComponentDTO componentDTO = SaveSerialNumber(equipmentSubTypeDTO, businessOrganizationId, serialNumber, equipmentTypeConstant == "DOWNHOLE");

            //Save Well Site
            string wellName = "TestWell1";
            WellDTO wellDTO = SaveWellSite(wellName);

            #region Create Job

            #region Create Install job

            string jobTypeConstant = "INSTALL";
            long jobTypeId = GetJobTypeIdByConstant(jobTypeConstant, equipmentSubTypeDTO, "true");

            AddHistoricalJob(equipmentSubTypeDTO, componentDTO, wellDTO, jobTypeId, jobTypeConstant, equipmentConfigDTO, installDate, pullDate);

            #endregion

            #region Create Pull job

            jobTypeConstant = "PULL & REPAIR";
            jobTypeId = GetJobTypeIdByConstant(jobTypeConstant, equipmentSubTypeDTO, "true");

            pullDate = installDate.AddDays(10);

            AddHistoricalJob(equipmentSubTypeDTO, componentDTO, wellDTO, jobTypeId, jobTypeConstant, equipmentConfigDTO, null, pullDate);

            #endregion


            #region Create Install & Pull job

            jobTypeConstant = "INSTALL & PULL";
            jobTypeId = 0;

            installDate = Convert.ToDateTime(pullDate).AddDays(1);
            pullDate = installDate.AddDays(10);

            AddHistoricalJob(equipmentSubTypeDTO, componentDTO, wellDTO, jobTypeId, jobTypeConstant, equipmentConfigDTO, installDate, pullDate);

            #endregion

            #endregion
        }


        public void AddHistoricalJob(EquipmentSubTypeDTO equipmentSubTypeDTO, ComponentDTO componentDTO, WellDTO wellDTO, long jobTypeId, string jobTypeConstant, EquipmentConfigDTO equipmentConfigDTO, DateTime? installDate, DateTime? pullDate)
        {
            Trace.WriteLine("Execution started for \"AddHistoricalJob\" method");

            ServiceJobDTO serviceJobDTO = new ServiceJobDTO();

            serviceJobDTO.ServiceRMJobDTO = new ServiceRMJobDTO();
            serviceJobDTO.JobBridgeDTO = new JobBridgeRMDTO();
            serviceJobDTO.BarrelPlungerDTO = new List<BarrelPlungerDTO>().ToArray();
            serviceJobDTO.ConditionalInfoDTO = new List<ServiceJobConditionDTO>().ToArray();
            serviceJobDTO.AdditionalInfoDTO = new List<AdditionalInfoFieldDTO>().ToArray();

            #region 1st Save of Multiple Saves
            SetDataForHistoricalTab(serviceJobDTO, equipmentSubTypeDTO, componentDTO, wellDTO, jobTypeId, jobTypeConstant, installDate, pullDate);


            ServiceJobDetailsDTO serviceJobDetailsDTO = new ServiceJobDetailsDTO();
            serviceJobDetailsDTO.ComponentId = componentDTO.Id;
            if (installDate != null)
            {
                serviceJobDetailsDTO.InstallDate = installDate;
            }

            serviceJobDetailsDTO.JobTypeConstant = jobTypeConstant;

            if (pullDate != null)
            {
                serviceJobDetailsDTO.PullDate = pullDate;
            }

            serviceJobDetailsDTO.PumpSerialNumber = componentDTO.Name;
            //serviceJobDetailsDTO.ReceivedAtShopDate = null;
            //serviceJobDetailsDTO.RepairDate = null;
            serviceJobDetailsDTO.WellNumber = wellDTO.Name;

            NewServiceJobValidationDTO newServiceJobValidationDTO = EquipmentJobService.ValidateHistoricalJob(serviceJobDetailsDTO);

            if (newServiceJobValidationDTO.CanProceed)
            {
                Trace.WriteLine("Execution started for \"SaveHistoricalJob\" method");

                serviceJobDTO.ServiceRMJobDTO.JobId = EquipmentJobService.SaveHistoricalJob(serviceJobDTO);

                Trace.WriteLine("Execution started for \"SaveHistoricalJob\" method");
            }

            Trace.WriteLine("Execution end for \"AddHistoricalJob\" method");

            //Test saved data for job till first multiple save
            //GetDataForStartAndServiceTabs(serviceJobDTO);

            #endregion

        }

        public void SetDataForHistoricalTab(ServiceJobDTO serviceJobDTO, EquipmentSubTypeDTO equipmentSubTypeDTO, ComponentDTO componentDTO, WellDTO wellDTO, long jobTypeId, string jobTypeConstant, DateTime? installDate, DateTime? pullDate)
        {
            Trace.WriteLine("Execution started for \"SetDataForHistoricalTab\" method");

            serviceJobDTO.JobBridgeDTO.EquipmentTypeId = equipmentSubTypeDTO.EquipmentTypeId;
            serviceJobDTO.JobBridgeDTO.EquipmentSubTypeId = equipmentSubTypeDTO.EquipmentSubTypeId;
            serviceJobDTO.ServiceRMJobDTO.JobTypeId = jobTypeId;

            serviceJobDTO.JobBridgeDTO.ComponentId = componentDTO.Id;
            serviceJobDTO.JobBridgeDTO.PumpAPI = componentDTO.APIDescription;

            serviceJobDTO.ServiceRMJobDTO.AssemblyId = wellDTO.AssemblyId;
            serviceJobDTO.ServiceRMJobDTO.JobTypeConstant = jobTypeConstant;

            if (installDate != null)
            {
                serviceJobDTO.ServiceRMJobDTO.InstallDate = Convert.ToDateTime(installDate);
            }

            if (pullDate != null)
            {
                serviceJobDTO.ServiceRMJobDTO.PullDate = Convert.ToDateTime(pullDate);
            }

            ServiceLocationDTO serviceLocationDTO = EquipmentJobService.GetServiceLocation(Convert.ToString(equipmentSubTypeDTO.EquipmentTypeId))?.FirstOrDefault();
            Assert.IsNotNull(serviceLocationDTO, "Service location does not exist.");

            //Assign data for Service tab
            serviceJobDTO.JobBridgeDTO.ServiceLocationId = serviceLocationDTO.ServiceLocationId;

            Trace.WriteLine("Execution end for \"SetDataForHistoricalTab\" method");
        }

        #endregion

        [TestCleanup]
        public override void Cleanup()
        {
            Trace.WriteLine("Execution started for \"Cleanup\" method");

            base.Cleanup();

            Trace.WriteLine("Execution ended for \"Cleanup\" method");
        }
    }
}