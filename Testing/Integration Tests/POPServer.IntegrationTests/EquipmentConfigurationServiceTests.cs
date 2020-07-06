using System;
using System.Collections.Generic;
using System.IO;
using Weatherford.POP.Enums;
using Weatherford.POP.Units;

namespace Weatherford.POP.Server.IntegrationTests
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Weatherford.POP.DTOs;
    [TestClass]
    public class EquipmentConfigurationServiceTests : APIClientTestBase
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





        [TestCategory(TestCategories.EquipmentConfigurationServiceTests), TestMethod]
        public void GetEquipmentGroupData()
        {
            EquipmentTypeGroupDTO groupData = EquipmentConfigurationService.GetEquipmentGroupData();
            Assert.IsNotNull(groupData, "No Values Present in Equipment Group Data");
            Assert.IsNotNull(groupData.EquipmentType, "No Equipment Type is Present ");
            Assert.AreEqual(2, groupData.EquipmentType.Count, "Equipment Type Count is not matching");
            Assert.IsNotNull(groupData.EquipmentSubType, "No Equipment Subtype is Present");
            EquipmentTypeDTO equipmentType = groupData.EquipmentType.Where(x => x.Description == "Downhole").FirstOrDefault();
            Assert.IsNotNull(equipmentType, "Downhole Equipment Type Not Present");
            EquipmentTypeDTO equipmentType_1 = groupData.EquipmentType.Where(x => x.Description == "Surface").FirstOrDefault();
            Assert.IsNotNull(equipmentType_1, "Surface Equipment Type Not Present");

            EntityGridSettingDTO setting = new EntityGridSettingDTO();
            setting.GridSetting = new GridSettingDTO { PageSize = 20, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
            setting.EntityName = "r_EquipmentSubType";
            int tableDataCount = DBEntityService.GetTableDataCount(setting);
            int EquipmentSubtypeCount = groupData.EquipmentSubType.Count;
            Assert.IsTrue(tableDataCount >= EquipmentSubtypeCount);
            object[][] dataValues = DBEntityService.GetTableData(setting).DataValues;
            int count = 0;
            foreach (object[] dataValue in dataValues)
            {
                if (dataValue[5].ToString() == "0")
                    count = count + 1;
            }

            Assert.AreEqual(count, EquipmentSubtypeCount, "Mismatch in Equipment Subtype Count");
        }


        [TestCategory(TestCategories.EquipmentConfigurationServiceTests), TestMethod]
        public void getProductStructureData()
        {
            EquipmentTypeGroupDTO groupData = EquipmentConfigurationService.GetEquipmentGroupData();
            Assert.IsNotNull(groupData, "No Values Present in Equipment Group Data");
            Assert.IsNotNull(groupData.EquipmentType, "No Equipment Type is Present ");
            EquipmentTypeDTO equipmentType = groupData.EquipmentType.Where(x => x.Description == "Downhole").FirstOrDefault();
            Assert.IsNotNull(equipmentType, "Downhole Equipment Type Not Present");
            EquipmentTypeDTO equipmentType_1 = groupData.EquipmentType.Where(x => x.Description == "Surface").FirstOrDefault();
            Assert.IsNotNull(equipmentType_1, "Surface Equipment Type Not Present");
            ProductStructureDTO[] productData = EquipmentConfigurationService.GetProductStructureData("Downhole");
            Assert.IsNotNull(productData, "Product Strcture against Downhole Equipment Type Not Present");
            productData = EquipmentConfigurationService.GetProductStructureData("Surface");
            Assert.IsNotNull(productData, "Product Strcture against Surface Equipment Type Not Present");

            EntityGridSettingDTO setting = new EntityGridSettingDTO();
            setting.GridSetting = new GridSettingDTO { PageSize = 50, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
            setting.EntityName = "r_ProductStructure";

            object[][] dataValues = DBEntityService.GetTableData(setting).DataValues;
            int downholeCount = 0;
            int surfaceCount = 0;
            foreach (object[] dataValue in dataValues)
            {
                if (dataValue[8].ToString() == "0" && dataValue[1].ToString() == "Downhole")
                    downholeCount = downholeCount + 1;

                if (dataValue[8].ToString() == "0" && dataValue[1].ToString() == "Surface")
                    surfaceCount = surfaceCount + 1;

            }



        }



        [TestCategory(TestCategories.EquipmentConfigurationServiceTests), TestMethod]
        public void EquipmentConfigSaveData()
        {

            EquipmentConfigProductStructureDTO equipConfigProductStructureDTO = EquipmentConfigurationService.GetEquipmentConfigData("1", "1");

            if (equipConfigProductStructureDTO.EquipmentConfig.EquipmentTypeId == 0)//If no Config Exists then we need Equipment Type and Sub Type passed for saving
            {
                equipConfigProductStructureDTO.EquipmentConfig.EquipmentTypeId = 1;
                equipConfigProductStructureDTO.EquipmentConfig.EquipmentSubTypeId = 1;
            }
            List<ProductStructureDTO> lstproductstructuredto = new List<ProductStructureDTO>();


            ProductStructureDTO productStructuredto1 = new ProductStructureDTO();
            productStructuredto1.ProductStructureId = 1;
            productStructuredto1.Description = "Barrel";
            productStructuredto1.ErpNumber = "1113874";
            productStructuredto1.Fk_ErpPartNumberId = 2;
            productStructuredto1.Quantity = 2;
            productStructuredto1.MfgNumber = "123";
            productStructuredto1.ActionPerformed = CRUDOperationTypes.Add;

            lstproductstructuredto.Add(productStructuredto1);

            ProductStructureDTO productstructuredto2 = new ProductStructureDTO();
            productstructuredto2.Description = "Barrel";
            productstructuredto2.ProductStructureId = 2;
            productstructuredto2.ErpNumber = "1113874";
            productstructuredto2.Fk_ErpPartNumberId = 2;
            productstructuredto2.Quantity = 5;
            productstructuredto2.MfgNumber = "12345";
            productstructuredto2.ActionPerformed = CRUDOperationTypes.Add;
            lstproductstructuredto.Add(productstructuredto2);



            equipConfigProductStructureDTO.ProductStructure = lstproductstructuredto;

            //Add ProductStructureDTO 
            EquipmentConfigurationService.EquipmentConfigSaveData(equipConfigProductStructureDTO);

            //update ProductStructureDTO
            EquipmentConfigProductStructureDTO equipConfigProductStructureDTO1 = EquipmentConfigurationService.GetEquipmentConfigData("1", "1");

            var updateproductstructuredto = equipConfigProductStructureDTO1.ProductStructure.ToList();


            updateproductstructuredto[0].Description = "Bushing";
            updateproductstructuredto[0].ProductStructureId = 2;
            updateproductstructuredto[0].Fk_ErpPartNumberId = 2;
            updateproductstructuredto[0].ErpNumber = "1113874";
            updateproductstructuredto[0].Quantity = 8;
            updateproductstructuredto[0].ActionPerformed = CRUDOperationTypes.Update;


            equipConfigProductStructureDTO1.ProductStructure = updateproductstructuredto;

            EquipmentConfigurationService.EquipmentConfigSaveData(equipConfigProductStructureDTO1);

            //Remove  Product Structure

            var updateproductstructuredto1 = equipConfigProductStructureDTO1.ProductStructure.ToList();
            updateproductstructuredto1[0].Description = "Bushing";
            updateproductstructuredto1[0].ProductStructureId = 2;
            updateproductstructuredto[0].Fk_ErpPartNumberId = 2;
            updateproductstructuredto[0].ErpNumber = "1113874";
            updateproductstructuredto1[0].ActionPerformed = CRUDOperationTypes.Remove;

            equipConfigProductStructureDTO1.ProductStructure = updateproductstructuredto1;

            EquipmentConfigurationService.EquipmentConfigSaveData(equipConfigProductStructureDTO1);


        }

        [TestCategory(TestCategories.EquipmentConfigurationServiceTests), TestMethod]
        public void equipmentConditionalDataSave()
        {
            EquipmentConfigProductStructureDTO equipConfigProductStructureDTO = EquipmentConfigurationService.GetEquipmentConfigData("1", "1");

            if (equipConfigProductStructureDTO.EquipmentConfig.EquipmentTypeId == 0)//If no Config Exists then we need Equipment Type and Sub Type passed for saving
            {
                equipConfigProductStructureDTO.EquipmentConfig.EquipmentTypeId = 1;
                equipConfigProductStructureDTO.EquipmentConfig.EquipmentSubTypeId = 1;
            }

            List<ProductStructureDTO> lstproductstructuredto = new List<ProductStructureDTO>();


            ProductStructureDTO productStructuredto1 = new ProductStructureDTO();
            productStructuredto1.ProductStructureId = 1;
            productStructuredto1.Description = "Barrel";
            productStructuredto1.ErpNumber = "1113874";
            productStructuredto1.Fk_ErpPartNumberId = 2;
            productStructuredto1.Quantity = 2;
            productStructuredto1.MfgNumber = "123";
            productStructuredto1.ActionPerformed = CRUDOperationTypes.Add;

            lstproductstructuredto.Add(productStructuredto1);


            //Add ProductStructureDTO 
            EquipmentConfigurationService.EquipmentConfigSaveData(equipConfigProductStructureDTO);

            string id = equipConfigProductStructureDTO.EquipmentConfig.EquipmentConfigId.ToString();

            EquipmentConditionDTO equipmentConfigData = EquipmentConfigurationService.GetEquipmentConditionData(id);

            Assert.IsNotNull(equipmentConfigData, "Equipment condition tab is not enabled");

            equipmentConfigData.FailureCause1 = (int)FieldDisposition.VisibleAndRequired;
            equipmentConfigData.FailureCause2 = (int)FieldDisposition.VisibleAndRequired;
            equipmentConfigData.FailureCondition1 = (int)FieldDisposition.VisibleAndRequired;
            equipmentConfigData.FailureCondition2 = (int)FieldDisposition.Hidden;
            equipmentConfigData.ForeignMaterial1 = (int)FieldDisposition.Visible;
            equipmentConfigData.ForeignMaterial2 = (int)FieldDisposition.VisibleAndRequired;
            equipmentConfigData.ForeignMaterialSample = (int)FieldDisposition.VisibleAndRequired;
            equipmentConfigData.PrimaryFailureCause = (int)FieldDisposition.VisibleAndRequired;
            equipmentConfigData.ForeignMaterialSample = (int)FieldDisposition.VisibleAndRequired;
            equipmentConfigData.Manufacturer = (int)FieldDisposition.VisibleAndRequired;
            equipmentConfigData.ManufacturerPartNumber = (int)FieldDisposition.VisibleAndRequired;
            equipmentConfigData.ERPPartNumber = (int)FieldDisposition.VisibleAndRequired;
            equipmentConfigData.SpareStock = (int)FieldDisposition.VisibleAndRequired;

            EquipmentConfigurationService.EquipmentConditionalDataSave(equipmentConfigData);

            //Check for Updated Values
            equipmentConfigData = EquipmentConfigurationService.GetEquipmentConditionData(id);

            Assert.IsNotNull(equipmentConfigData, "Condition Detail is not Present");


        }


        [TestCategory(TestCategories.EquipmentConfigurationServiceTests), TestMethod]
        public void equipmentAdditionalInfoDataSave()
        {

            EquipmentConfigProductStructureDTO equipConfigProductStructureDTO = EquipmentConfigurationService.GetEquipmentConfigData("1", "1");

            if (equipConfigProductStructureDTO.EquipmentConfig.EquipmentTypeId == 0)//If no Config Exists then we need Equipment Type and Sub Type passed for saving
            {
                equipConfigProductStructureDTO.EquipmentConfig.EquipmentTypeId = 1;
                equipConfigProductStructureDTO.EquipmentConfig.EquipmentSubTypeId = 1;
            }
            List<ProductStructureDTO> lstproductstructuredto = new List<ProductStructureDTO>();


            ProductStructureDTO productStructuredto1 = new ProductStructureDTO();
            productStructuredto1.ProductStructureId = 1;
            productStructuredto1.Description = "Barrel";
            productStructuredto1.ErpNumber = "1113874";
            productStructuredto1.Fk_ErpPartNumberId = 2;
            productStructuredto1.Quantity = 2;
            productStructuredto1.MfgNumber = "123";
            productStructuredto1.ActionPerformed = CRUDOperationTypes.Add;

            lstproductstructuredto.Add(productStructuredto1);


            //Add ProductStructureDTO 
            EquipmentConfigurationService.EquipmentConfigSaveData(equipConfigProductStructureDTO);

            long id = equipConfigProductStructureDTO.EquipmentConfig.EquipmentConfigId;
            AdditionalInfoFieldDTO[] equipmentAdditionalData = new AdditionalInfoFieldDTO[1];
            equipmentAdditionalData[0] = new AdditionalInfoFieldDTO();
            equipmentAdditionalData[0].DisplayLabel = "Add Test";
            equipmentAdditionalData[0].ActionPerformed = CRUDOperationTypes.Add;
            equipmentAdditionalData[0].DisplaySetting = (int)FieldDisposition.VisibleAndRequired;
            equipmentAdditionalData[0].EquipmentConfigId = id;

            //Add Additional info detail 
            EquipmentConfigurationService.EquipmentAdditionalInfoDataSave(equipmentAdditionalData);

            //Get Additional Info Detail
            equipmentAdditionalData = EquipmentConfigurationService.GetEquipmentAdditionalInfoData(id.ToString());

            Assert.IsNotNull(equipmentAdditionalData, "Additional Info Not Present");



        }



    }
}
