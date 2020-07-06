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
    public class DBEntityServiceTests : APIClientTestBase
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

        [TestCategory(TestCategories.DBEntityServiceTests), TestMethod]
        public void ReferenceDataCRUD()
        {
            ReferenceDataMaintenanceEntityDTO[] dataEntities = DBEntityService.GetReferenceDataMaintenanceEntities();
            Assert.IsTrue(dataEntities.Length > 0);

            EntityGridSettingDTO setting = new EntityGridSettingDTO();
            setting.GridSetting = new GridSettingDTO { PageSize = 20, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
            foreach (ReferenceDataMaintenanceEntityDTO dataEntity in dataEntities)
            {
                setting.EntityName = dataEntity.EntityName;
                DBEntityDTO gridData = DBEntityService.GetTableData(setting);

                Assert.AreEqual(dataEntity.EntityName, gridData.EntityName, "Grid data - entity name to be same");
                Assert.AreEqual(dataEntity.EntityTitle, gridData.EntityTitle, "Grid data - entity title to be same");
                Assert.IsNotNull(gridData.Attributes, "Meta data attributes should not be null");
                Assert.IsTrue(gridData.Attributes.Length > 0, "Meta data attributes count should be more than 0");

                //Update scenario test
                if (!(gridData.DataValues.Length > 0)) { continue; }
                int columnCounter = 0; MetaDataDTO primaryKey = null;
                for (columnCounter = 0; columnCounter < gridData.Attributes.Length; columnCounter++)
                {
                    if (gridData.Attributes[columnCounter].AttributeDefinition.PKey)
                    {
                        primaryKey = gridData.Attributes[columnCounter].AttributeDefinition;
                        break;
                    }
                }

                MetaDataDTO[] updateMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForUpdate(primaryKey.TableName, gridData.DataValues[0][columnCounter].ToString());
                Assert.IsTrue(updateMetaDatas.Length > 0, "Update meta data list length should be greater than 0");
                MetaDataDTO toBeUpdatedMetaData = updateMetaDatas.FirstOrDefault(x => x.PKey == false && x.DataDisplayType.ToUpper() == "TEXTBOX" && x.DataType.ToUpper() == "STRING");
                string originalValue = string.Empty;
                string newValue = "^";
                if (toBeUpdatedMetaData != null)
                {
                    if (toBeUpdatedMetaData.DataValue != null)
                    {
                        originalValue = toBeUpdatedMetaData.DataValue.ToString();
                    }
                    toBeUpdatedMetaData.DataValue = newValue;

                    Assert.IsTrue(DBEntityService.UpdateReferenceData(updateMetaDatas), "Update reference data with new value");
                    updateMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForUpdate(primaryKey.TableName, gridData.DataValues[0][columnCounter].ToString());
                    MetaDataDTO updatedMetaData = updateMetaDatas.FirstOrDefault(x => x.ColumnName == toBeUpdatedMetaData.ColumnName);
                    Assert.AreEqual(updatedMetaData.DataValue.ToString(), newValue, "Reference data value updated");

                    //Updating original value back
                    updatedMetaData.DataValue = originalValue;
                    Assert.IsTrue(DBEntityService.UpdateReferenceData(updateMetaDatas), "Update reference data with original value");
                }
            }
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

        [TestCategory(TestCategories.DBEntityServiceTests), TestMethod]
        public void AddReferenceMetadataForAllEntities()
        {
            var failureList = new List<string>();
            ReferenceDataMaintenanceEntityDTO[] allReferenceTables = DBEntityService.GetReferenceDataMaintenanceEntities();
            Assert.IsTrue(allReferenceTables.Length > 0, "No reference tables are available inside the database.");

            EntityGridSettingDTO setting = new EntityGridSettingDTO();
            setting.GridSetting = new GridSettingDTO { PageSize = 20, NumberOfPages = 1, CurrentPage = 1, Sorts = null };

            foreach (ReferenceDataMaintenanceEntityDTO ReferenceTable in allReferenceTables)
            {
                try
                {
                    setting.EntityName = ReferenceTable.EntityName;

                    if (ReferenceTable.EntityName.Equals("r_JobTypeEventType") ||
                        ReferenceTable.EntityName.Equals("r_StickyNoteStatus") ||
                        ReferenceTable.EntityName.Equals("r_ServiceLocation_RM") ||
                        ReferenceTable.EntityName.Equals("UserServiceLocation_RM") ||
                        ReferenceTable.EntityName.Equals("UserBusinessOrganization_RM"))
                    {
                        //A separate method is required to check uniqueness for adding r_JobTypeEventType
                        //The adding ConstantId method inside the function of GenerateInsertQueryForReferenceTable
                        //  in MetaDataUtils.cs is not working for r_StickyNoteStatus
                        //ControlIdTextDTO[] ctrlIds = GetMetadataReferenceDataDDL get a zero array
                        continue;
                    }

                    //Get Table Data
                    DBEntityDTO tableData = DBEntityService.GetTableData(setting);
                    int tableDataCount = DBEntityService.GetTableDataCount(setting);
                    if (tableDataCount > 20)//20 - Page Size
                        Assert.AreEqual(20, tableData.DataValues.Count(), "Table data incorrectly returned from server.");
                    else
                        Assert.AreEqual(tableDataCount, tableData.DataValues.Count(), "Table data incorrectly returned from server.");

                    if (tableDataCount > 0)
                        Assert.AreEqual(tableData.Attributes.Count(), tableData.DataValues.FirstOrDefault().Count(), "Mismatch in count between the columns and content.");
                    DBEntityAttributeDTO primaryKey = tableData.Attributes.FirstOrDefault(x => x.AttributeName.Contains("PrimaryKey"));
                    DBEntityAttributeDTO constantID = tableData.Attributes.FirstOrDefault(x => x.AttributeName.Contains("ConstantId"));
                    DBEntityAttributeDTO refUserDeleted = tableData.Attributes.FirstOrDefault(x => x.AttributeName.Contains("RefUserDeleted"));

                    //Get Metadata for Add
                    bool bHasRefUserDeleted = true;
                    MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(ReferenceTable.EntityName);
                    foreach (MetaDataDTO addMetaData in addMetaDatas)
                    {
                        if (addMetaData.Visible && addMetaData.Required)
                            Assert.IsTrue(addMetaData.Editable, "Unable to edit the Required and Visible field on the UI for Add model.");
                        MetaDataDTO mdPrimaryKey = addMetaDatas.FirstOrDefault(x => x.ColumnName.Contains("PrimaryKey"));
                        MetaDataDTO mdChangeTime = addMetaDatas.FirstOrDefault(x => x.ColumnName.Contains("ChangeTime"));
                        MetaDataDTO mdChangeUserId = addMetaDatas.FirstOrDefault(x => x.ColumnName.Contains("ChangeUserId"));
                        MetaDataDTO mdRefUserDeleted = addMetaDatas.FirstOrDefault(x => x.ColumnName.Contains("RefUserDeleted"));
                        if (mdRefUserDeleted == null)
                        {
                            bHasRefUserDeleted = false;
                        }
                        if (addMetaData.ColumnName.Contains("FK") && (!string.IsNullOrWhiteSpace(addMetaData.ReferenceTableName) || addMetaData.ReferenceKeyColumn.Contains("PrimaryKey")))
                            Assert.AreEqual("DDL", addMetaData.DataDisplayType, "Mismatch in the data display type for the foreign key.");
                        if (addMetaData.ColumnName.Substring(addMetaData.ColumnName.Length - 2).Trim().ToUpper() == "DT")
                            Assert.IsTrue(addMetaData.DataDisplayType.Contains("DATE"), $"Mismatch in the data display type for the Date: {addMetaData.ColumnName}.");
                        //User Input
                        if (addMetaData.Visible && addMetaData.Editable)
                        {
                            addMetaData.DataValue = DataValue(addMetaData);
                            if (addMetaData.DataDisplayType.Contains("RADIO"))
                                addMetaData.DataValue = 1;
                        }
                        if (mdPrimaryKey != null)
                            Assert.IsFalse(mdPrimaryKey.Visible, "Primary Key should not be marked as visible in Add model meta data.");
                        if (mdChangeTime != null)
                            Assert.IsFalse(mdChangeTime.Visible, "Change time should not be marked as visible in Add model meta data.");
                        if (mdChangeUserId != null)
                            Assert.IsFalse(mdChangeUserId.Visible, "Change User should not be marked as visible in Add model meta data.");
                        if (ReferenceTable.EntityName == "AFE")
                        {
                            if (addMetaData.DataDisplayType.Contains("DATE") && addMetaData.Required)
                            {
                                addMetaData.DataValue = DateTime.UtcNow.ToISO8601();
                            }
                        }
                        if (ReferenceTable.EntityName.Equals("r_PrimaryFailureClass"))
                        {
                            if (addMetaData.DataDisplayType.Contains("TEXTBOX") && addMetaData.Required)
                            {
                                addMetaData.DataValue = DateTime.UtcNow.ToISO8601();
                            }
                        }

                        if (ReferenceTable.EntityName.Equals("r_DownCode"))
                        {
                            if (addMetaData.DataDisplayType.Contains("TEXTBOX") && addMetaData.ColumnName.Contains("rdcFK_Job"))
                            {
                                addMetaData.DataValue = null;
                            }
                        }
                    }
                    //Add Data for the Reference Table
                    string addTableData = DBEntityService.AddReferenceData(addMetaDatas);
                    Assert.IsNotNull(addTableData, $"Unable to add record for the reference table: {ReferenceTable.EntityName}.");
                    //Get Data for the Reference Table
                    tableData = DBEntityService.GetTableData(setting);
                    int afterAdd = DBEntityService.GetTableDataCount(setting);
                    Trace.WriteLine($"Add reference data is successful for the entity : {tableData.EntityName}.");
                    //Validating the reference data
                    Assert.AreEqual(tableDataCount + 1, afterAdd, "Unable to add record for the reference table.");
                    Assert.IsNotNull(primaryKey, "Unable to find the primary key column.");
                    //Assert.IsNotNull(constantID, "No Constant Id column found in the table");
                    if (bHasRefUserDeleted)
                    {
                        Assert.IsNotNull(refUserDeleted, "No Ref-User deleted column found in the table.");
                        Assert.IsTrue(refUserDeleted.AttributeDefinition.Visible, "RefUserDeleted should be marked as visible.");
                    }
                    Assert.IsFalse(primaryKey.AttributeDefinition.Visible, "Primary Key should not be marked as visible.");
                    Assert.IsFalse(primaryKey.AttributeDefinition.Visible, "Constant Id should not be marked as visible.");

                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Table Name :" + ReferenceTable.EntityName);
                    Trace.WriteLine("Error message :" + ex.Message);
                    failureList.Add(ReferenceTable.EntityName);
                }
            }
            Assert.AreEqual(0, failureList.Count, $"Operation not completed succesfully for {failureList.Count} entit{(failureList.Count == 1 ? "y" : "ies")} of {allReferenceTables.Length}." + Environment.NewLine + string.Join(", ", failureList));
        }

        [TestCategory(TestCategories.DBEntityServiceTests), TestMethod]
        public void GetReferenceDataCount()
        {
            //Mapping all Entities list to allRefTables
            ReferenceDataMaintenanceEntityDTO[] allRefTables = DBEntityService.GetReferenceDataMaintenanceEntities();
            Assert.IsTrue(allRefTables.Length > 0, "No reference tables are available inside the database");

            //Mapping for table data records, grid setting, table counts.
            EntityGridSettingDTO setting = new EntityGridSettingDTO();
            setting.GridSetting = new GridSettingDTO { PageSize = 20, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
            setting.EntityName = allRefTables.FirstOrDefault().EntityName;
            DBEntityDTO tableData = DBEntityService.GetTableData(setting);
            int beforeTableDataCount = DBEntityService.GetTableDataCount(setting);

            Trace.WriteLine("Ref Table:" + setting.EntityName + "----" + "Count:" + beforeTableDataCount);
            //Add a record
            MetaDataDTO[] getAddMetaData = DBEntityService.GetRefereneceMetaDataEntityForAdd(setting.EntityName);

            foreach (MetaDataDTO item in getAddMetaData)
            {
                if (item.Visible && item.Editable)
                {
                    item.DataValue = DataValue(item);
                    if (item.DataDisplayType.Contains("RADIO"))
                        item.DataValue = 0;
                }
            }

            string addData = DBEntityService.AddReferenceData(getAddMetaData);
            Assert.IsNotNull(addData, "Record Not Added Successfully");

            int afterTableDataCount = DBEntityService.GetTableDataCount(setting);
            Assert.AreEqual(beforeTableDataCount + 1, afterTableDataCount, "Records Are Not Getting Incremented by 1 After Adding New Record to Entity");

            Trace.WriteLine("Ref Table:" + setting.EntityName + "----" + "Count:" + afterTableDataCount);
        }

        /// <summary>
        /// This method will test the Well Status drop down functionality.
        /// </summary>
        /// Jira Story FRWM-1780 Sub Task FRWM-1841
        [TestCategory(TestCategories.DBEntityServiceTests), TestMethod]
        public void CheckWellStatusDropDownFunctionality()
        {
            string wellName = "RRLWELL_TEST", wellStatus = "", tableName = "r_WellStatus", wellStatusRecord = "TestingAPIStatus2";
            long newAddedWellStatus = 0;
            int counter = 0;

            //Get r_WellStatus reference table.
            ReferenceDataMaintenanceEntityDTO ReferenceTable = DBEntityService.GetReferenceDataMaintenanceEntities().FirstOrDefault(x => x.EntityName == tableName);
            Assert.IsNotNull(ReferenceTable, "Failed to get " + tableName);

            //Get Grid setting
            EntityGridSettingDTO setting = new EntityGridSettingDTO();
            setting.GridSetting = new GridSettingDTO { PageSize = 20, NumberOfPages = 1, CurrentPage = 1, Sorts = null };

            try
            {
                setting.EntityName = ReferenceTable.EntityName;

                //Get Table Data
                int tableDataCount = DBEntityService.GetTableDataCount(setting);
                setting.GridSetting = new GridSettingDTO { PageSize = tableDataCount, NumberOfPages = 1, CurrentPage = 1, Sorts = null };
                DBEntityDTO tableData = DBEntityService.GetTableData(setting);
                Assert.IsTrue(tableDataCount > 0, "Failed to obtain the dat from r_WellStatus Table");

                // Checking new well status exist in Database
                for (int index = 0; index < tableDataCount; index++)
                {
                    if (tableData.DataValues[index][1].ToString().Equals(wellStatusRecord))
                    {
                        long.TryParse(tableData.DataValues[index][0].ToString(), out newAddedWellStatus);
                        counter++;
                        break;
                    }
                }
                // Add Well Status record if not exist in the r_WellStatus Table.
                if (counter == 0)
                {
                    //Get Metadata for Add
                    MetaDataDTO[] addMetaDatas = DBEntityService.GetRefereneceMetaDataEntityForAdd(ReferenceTable.EntityName);
                    addMetaDatas.FirstOrDefault(x => x.ColumnName.Equals("wstWellStatus")).DataValue = wellStatusRecord;
                    addMetaDatas.FirstOrDefault(x => x.ColumnName.Equals("wstWellStatusDesc")).DataValue = "Testing well status functionality";
                    addMetaDatas.FirstOrDefault(x => x.ColumnName.Contains("RefUserDeleted")).DataValue = 1;
                    addMetaDatas.FirstOrDefault(x => x.ColumnName.Contains("wstOilProduction")).DataValue = 0;
                    addMetaDatas.FirstOrDefault(x => x.ColumnName.Contains("wstGasProduction")).DataValue = 0;
                    addMetaDatas.FirstOrDefault(x => x.ColumnName.Contains("wstWaterProduction")).DataValue = 0;
                    addMetaDatas.FirstOrDefault(x => x.ColumnName.Contains("wstCondensateProduction")).DataValue = 0;
                    addMetaDatas.FirstOrDefault(x => x.ColumnName.Contains("wstEmulsionProduction")).DataValue = 0;
                    addMetaDatas.FirstOrDefault(x => x.ColumnName.Contains("wstGasInjection")).DataValue = 0;
                    addMetaDatas.FirstOrDefault(x => x.ColumnName.Contains("wstWaterInjection")).DataValue = 0;
                    addMetaDatas.FirstOrDefault(x => x.ColumnName.Contains("wstSteamInjection")).DataValue = 0;
                    addMetaDatas.FirstOrDefault(x => x.ColumnName.Contains("wstOilInjection")).DataValue = 0;
                    addMetaDatas.FirstOrDefault(x => x.ColumnName.Contains("wstOtherInjection")).DataValue = 0;

                    //Add Data for the Reference Table
                    string addTableData = DBEntityService.AddReferenceData(addMetaDatas);
                    Assert.IsNotNull(addTableData, "Unable to add record for the reference table : " + ReferenceTable.EntityName);

                    //Get latest added primary key in newAddedWellStatus variable.
                    long.TryParse(addTableData, out newAddedWellStatus);

                    //Get Data for the Reference Table
                    tableData = DBEntityService.GetTableData(setting);
                    int afterAdd = DBEntityService.GetTableDataCount(setting);
                    Trace.WriteLine($"Add reference data is successful for the entity : {tableData.EntityName}.");

                    //Validating the reference data count incremented by 1 or not.
                    Assert.AreEqual(tableDataCount + 1, afterAdd, "Unable to add record for the reference table");
                }
                // Add new RRL Well
                WellConfigurationService.AddWellConfig(new WellConfigDTO()
                {
                    Well = SetDefaultFluidType(new WellDTO()
                    {
                        Name = wellName,
                        FacilityId = wellName,
                        DataConnection = GetDefaultCygNetDataConnection(),
                        SubAssemblyAPI = "SubAssemblyAPI_" + wellName,
                        AssemblyAPI = "AssemblyAPI_" + wellName,
                        CommissionDate = DateTime.Today,
                        WellType = WellTypeId.RRL,
                        WellStatusId = newAddedWellStatus, // TestingAPIStatus
                    })
                });
                // Get Well information from System
                var allWells = WellService.GetAllWells().ToList();
                WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(wellName));
                Assert.IsNotNull(well);
                _wellsToRemove.Add(well);

                // Check Well status functionality - We need to pull all the records in referenceTableItemDTO so pass showAllRecords as true.
                ReferenceTableItemDTO[] referenceTableItemDTO = WellConfigurationService.GetReferenceTableItems("r_WellStatus", "true");
                Assert.IsNotNull(referenceTableItemDTO);
                foreach (var item in referenceTableItemDTO)
                {
                    if (item.Id == newAddedWellStatus)
                    {
                        wellStatus = item.Name;
                        break;
                    }
                }
                Assert.AreEqual(well.WellStatusId, newAddedWellStatus, "Well Status Id is not matching");
                Assert.AreEqual(wellStatus, wellStatusRecord, "Well status is not matching");
            }
            catch (Exception ex)
            {
                Assert.Fail("Error message :" + ex.Message);
            }
        }
    }
}