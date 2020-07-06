using System;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using ClientAutomation.PageObjects;
using ArtOfTest.WebAii.Controls.HtmlControls;
using System.Threading;
using System.Data;
using System.Configuration;
using ArtOfTest.WebAii.ObjectModel;
using System.IO;
using ArtOfTest.WebAii.Win32.Dialogs;
using System.Collections.ObjectModel;
using ClientAutomation.TelerikCoreUtils;
using System.Linq;

namespace ClientAutomation
{
    /// <summary>
    /// Summary description for Equipment Configuration
    /// </summary>
    public class EquipmentConfiguration : ForeSiteAutoBase
    {
        public EquipmentConfiguration()
        {
        }

        public void EquipmentConfiguration_Script()
        {
            try
            {

                //test data initialize
                string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "EquipmentConfigData.xml");

                //used for local setup testing
                //string testdatafile = Path.Combine("C:\\tanvi.amin_USDCVDIFSTDV027_9727\\UQA\\AssetsDev\\UIAutomation\\TestData\\EquipmentConfigData.xml");
                DataTable dt = Helper.CommonHelper.BuildDataTableFromXml(testdatafile);

                #region Test Data
                string equipmentType = dt.Rows[0]["EquipmentType"].ToString();
                string equipmentSubType = dt.Rows[0]["EquipmentSubType"].ToString();
                string equipmentToggleButton = dt.Rows[0]["EquipmentToggleButton"].ToString();
                string equipmentToggle_Action = dt.Rows[0]["EquipmentToggleAction"].ToString();
                string addNew_productStructure = dt.Rows[0]["AddNew_ProductStructure"].ToString();
                string addNew_qty = dt.Rows[0]["AddNew_Quantity"].ToString();
                string condDetails_labelname = dt.Rows[0]["ConditionDetail_Label"].ToString();
                string condDetails_showhide = dt.Rows[0]["ConditionDetail_ShowHide"].ToString();
                string condDetails_mandatory = dt.Rows[0]["ConditionDetail_Mandatory"].ToString();
                string addInfo_text = dt.Rows[0]["AdditionalInfo_Label"].ToString();
                string addInfo_showhide = dt.Rows[0]["AdditionalInfo_ShowHide"].ToString();
                string addInfo_mandatory = dt.Rows[0]["AdditionalInfo_Mandatory"].ToString();

                //edit testdata 
                string prodStr = dt.Rows[0]["ProductStructure"].ToString();
                string oemNo = dt.Rows[0]["OEM_Number"].ToString();
                string erpNo = dt.Rows[0]["ERP_PartNumber"].ToString();
                string quantity = dt.Rows[0]["Quantity"].ToString();
                string edit_ProdStr = dt.Rows[0]["Edit_ProductStructure"].ToString();
                string edit_OemNo = dt.Rows[0]["Edit_OEM_Number"].ToString();
                string edit_ErpNo = dt.Rows[0]["Edit_ERP_PartNumber"].ToString();
                string edit_Quantity = dt.Rows[0]["Edit_Quantity"].ToString();
                string del_ProdStr = dt.Rows[0]["Del_ProductStructure"].ToString();
                string del_OemNo = dt.Rows[0]["Del_OEM_Number"].ToString();
                string del_ErpNo = dt.Rows[0]["Del_ERP_PartNumber"].ToString();
                string del_Quantity = dt.Rows[0]["Del_Quantity"].ToString();
                string addInfo_rowToEdit = dt.Rows[0]["AddInfo_rowToEdit"].ToString();
                string addInfo_EditedText = dt.Rows[0]["AddInfo_EditedText"].ToString();
                string addInfo_ShowHide_Edit = dt.Rows[0]["AddInfo_ShowHide_Edit"].ToString();
                string addInfo_ActionMandatory_Edit = dt.Rows[0]["AddInfo_ActionMandatory_Edit"].ToString();
                string addInfo_rowToDel = dt.Rows[0]["AddInfo_rowToDel"].ToString();

                #endregion Test Data

                //launch browser
                TelerikObject.InitializeManager();

                Thread.Sleep(15 * 1000);

                //click configuration tab
                TelerikObject.Click(PageEquipmentConfiguration.eqconfig_Configtab);

                //click equipment configuration
                TelerikObject.Click(PageEquipmentConfiguration.eqconfig_EquipmentConfigtab);

                Thread.Sleep(5 * 1000);

                //assert two drop downs
                Assert.IsNotNull(PageEquipmentConfiguration.eqconfig_EquipmentSelect);

                Assert.IsNotNull(PageEquipmentConfiguration.eqconfig_EquipmentSubSelect);

                //assert 3 buttons
                Helper.CommonHelper.TraceLine("Ensuring that Add, Save and Cancel buttons are Disabled before selecting equipment type");

                Assert.IsFalse((PageEquipmentConfiguration.eqconfig_Addbtn).IsEnabled, "Add Button is Enabled ");
                Assert.IsFalse((PageEquipmentConfiguration.eqconfig_Cancelbtn).IsEnabled, "Cancel Button is Enabled ");
                Assert.IsFalse((PageEquipmentConfiguration.eqconfig_Savebtn).IsEnabled, "Save Button is Enabled ");

                Thread.Sleep(2 * 1000);

                //select equipment type 
                TelerikObject.Click(PageEquipmentConfiguration.eqconfig_EquipmentSelect);
                TelerikObject.Select_KendoUI_Listitem(equipmentType, true);

                Thread.Sleep(2 * 1000);

                //select equipment subtype
                TelerikObject.Click(PageEquipmentConfiguration.eqconfig_EquipmentSubSelect);
                TelerikObject.Select_KendoUI_Listitem(equipmentSubType, true);

                if (equipmentType == null || equipmentSubType == null)
                {
                    Assert.Fail("Equipment Type and/or Equipment Subtype cannot have null values");
                }


                Thread.Sleep(5 * 1000);

                //equipment config toggle buttons data
                string[] eqtogglebutton = equipmentToggleButton.Split(',');
                string[] eqtoggle_action = equipmentToggle_Action.Split(',');
                for (int i = 0; i < eqtogglebutton.Length; i++)
                {
                    if (eqtogglebutton.Length == 0)
                    {
                        break;
                    }

                    //toggle
                    PageEquipmentConfiguration.getToggleButton(eqtogglebutton[i], eqtoggle_action[i]);
                }

                Thread.Sleep(5 * 1000);

                //assert add button
                Helper.CommonHelper.TraceLine("Ensuring that Add button is Enabled after selecting equipment sub-type");
                Assert.IsTrue((PageEquipmentConfiguration.eqconfig_Addbtn).IsEnabled, "Add Button is Disabled ");

                //click on Add button
                TelerikObject.Click(PageEquipmentConfiguration.eqconfig_Addbtn);

                Thread.Sleep(5 * 1000);

                //verify Add New window
                Assert.IsNotNull(PageEquipmentConfiguration.eqconfig_addNew_div);
                Helper.CommonHelper.TraceLine("Add New product structure window is displayed");

                //verify the fields in Add New window
                Assert.IsNotNull(PageEquipmentConfiguration.addNew_ProdStructureSelect);
                Assert.IsNotNull(PageEquipmentConfiguration.addNew_ERPPartNo_text);
                Assert.IsNotNull(PageEquipmentConfiguration.addNew_OEMPartNo_text);
                Assert.IsNotNull(PageEquipmentConfiguration.addNew_Quantity_text);
                //Assert.IsNotNull(PageEquipmentConfiguration.addNew_Addbtn);
                //Assert.IsNotNull(PageEquipmentConfiguration.addNew_Cancelbtn);

                //add new product structure data
                string[] eqconfig_prodstruct = addNew_productStructure.Split(',');
                string[] eqconfig_qty = addNew_qty.Split(',');
                string[] oemerp = new string[eqconfig_prodstruct.Length];

                try
                {
                    //add product structures
                    for (int i = 0; i < eqconfig_prodstruct.Length; i++)
                    {
                        //select product structure
                        TelerikObject.Click(PageEquipmentConfiguration.addNew_ProdStructureSelect);
                        TelerikObject.Select_KendoUI_Listitem(eqconfig_prodstruct[i], true);

                        Thread.Sleep(2 * 1000);

                        //enter ERP number
                        string erpno = PageEquipmentConfiguration.addNew_ERPPartNo_text.Text;

                        oemerp[i] = erpno;

                        //enter OEM Part number
                        string oemno = PageEquipmentConfiguration.addNew_OEMPartNo_text.Text;

                        //enter quantity
                        TelerikObject.Sendkeys(PageEquipmentConfiguration.addNew_Quantity_text, eqconfig_qty[i]);

                        //click on Add
                        PageEquipmentConfiguration.AddNew_AddCancelButtons("Add");

                        Thread.Sleep(7 * 1000);

                        //verify table
                        PageEquipmentConfiguration.verifyProductStructureGrid(i, eqconfig_prodstruct[i], oemno, erpno, eqconfig_qty[i]);

                        Thread.Sleep(3 * 1000);

                        if (i != eqconfig_prodstruct.Length - 1)
                        {
                            //click on Add button to add another product structure
                            TelerikObject.Click(PageEquipmentConfiguration.eqconfig_Addbtn);
                            Thread.Sleep(5 * 1000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Helper.CommonHelper.TraceLine("Add New Product Structure Failed. " + ex);
                }

                Thread.Sleep(5 * 1000);

                //click on Save
                TelerikObject.Click(PageEquipmentConfiguration.eqconfig_Savebtn);

                Thread.Sleep(7 * 1000);

                string[] ProdStr = prodStr.Split(',');
                string[] OemNo = oemNo.Split(',');
                string[] ErpNo = erpNo.Split(',');
                string[] Quantity = quantity.Split(',');
                string[] Edit_ProdStr = edit_ProdStr.Split(',');
                string[] Edit_OEMNo = edit_OemNo.Split(',');
                string[] Edit_ERPNo = edit_ErpNo.Split(',');
                string[] Edit_Quantity = edit_Quantity.Split(',');

                for (int i = 0; i < ProdStr.Length; i++)
                {
                    //edit product structure
                    PageEquipmentConfiguration.editProductStructure(ProdStr[i], OemNo[i], ErpNo[i], Quantity[i],
                        Edit_ProdStr[i], Edit_OEMNo[i], Edit_ERPNo[i], Edit_Quantity[i]);
                }

                Thread.Sleep(5 * 1000);


                //delete product structures
                string[] Del_ProdStr = del_ProdStr.Split(',');
                string[] Del_OemNo = del_OemNo.Split(',');
                string[] Del_ErpNo = del_ErpNo.Split(',');
                string[] Del_Quantity = del_Quantity.Split(',');

                for (int i = 0; i < Del_ProdStr.Length; i++)
                {
                    //delete product structure
                    PageEquipmentConfiguration.delProductStructure(Del_ProdStr[i], Del_OemNo[i], Del_ErpNo[i], Del_Quantity[i]);

                }

                Thread.Sleep(5 * 1000);

                //click on condition details tab
                TelerikObject.Click(PageEquipmentConfiguration.eqconfig_ConditionDetails);

                Thread.Sleep(5 * 1000);

                ////condition details -- verify erp and action fields mandatory and show
                //PageEquipmentConfiguration.VerifyAction_ERP_Disabled();

                ///condition details data
                string[] condDetail_label = condDetails_labelname.Split(',');
                string[] condDetail_action = condDetails_showhide.Split(',');
                string[] condDetail_mandate = condDetails_mandatory.Split(',');

                for (int i = 0; i < condDetail_label.Length; i++)
                {

                    if (condDetail_label.Length == 0)
                    {
                        break;
                    }

                    //select condition details
                    PageEquipmentConfiguration.gridConditionDetails(condDetail_label[i], condDetail_action[i], condDetail_mandate[i]);
                }
                Thread.Sleep(5 * 1000);

                //click Save
                TelerikObject.Click(PageEquipmentConfiguration.conditionDetails_Savebtn);

                Thread.Sleep(5 * 1000);

                //click Additional Information
                TelerikObject.Click(PageEquipmentConfiguration.eqconfig_AdditionalInfo);

                Thread.Sleep(5 * 1000);

                //click Add
                TelerikObject.Click(PageEquipmentConfiguration.additionalInfo_Addbtn);

                Thread.Sleep(5 * 1000);

                //additional info data
                string[] addInfo_label = addInfo_text.Split(',');
                string[] addInfo_showHide = addInfo_showhide.Split(',');
                string[] addinfo_mandate = addInfo_mandatory.Split(',');

                for (int i = 0; i < addInfo_label.Length; i++)
                {
                    if (addInfo_label.Length == 0)
                    {
                        break;
                    }

                    //additional info
                    PageEquipmentConfiguration.additionalInfo_AddFields(addInfo_label[i], addInfo_showHide[i], addinfo_mandate[i]);

                    Thread.Sleep(5 * 1000);
                }

                string[] AddInfo_rowToEdit = addInfo_rowToEdit.Split(',');
                string[] AddInfo_EditedText = addInfo_EditedText.Split(',');
                string[] AddInfo_ShowHide_Edit = addInfo_ShowHide_Edit.Split(',');
                string[] AddInfo_ActionMandatory_Edit = addInfo_ActionMandatory_Edit.Split(',');

                for (int i = 0; i < addInfo_rowToEdit.Length; i++)
                {
                    //edit additional info
                    PageEquipmentConfiguration.delEdit_AdditionalInfo("Edit", Convert.ToInt32(AddInfo_rowToEdit[i]), AddInfo_EditedText[i],
                        AddInfo_ShowHide_Edit[i], AddInfo_ActionMandatory_Edit[i]);
                }

                Thread.Sleep(5 * 1000);
                string[] AddInfo_rowToDel = addInfo_rowToDel.Split(',');

                for (int i = 0; i < addInfo_rowToEdit.Length; i++)
                {
                    //delete additional info
                    PageEquipmentConfiguration.delEdit_AdditionalInfo("Delete", Convert.ToInt32(AddInfo_rowToDel[i]));
                }

                Thread.Sleep(5 * 1000);


            }
            catch (Exception e)
            {
                Helper.CommonHelper.PrintScreen("Err_Equipment Configuration flow");
                Helper.CommonHelper.TraceLine("Error " + e);
                Assert.Fail("Equipment Configuration Test has failed");

            }
            finally
            {
                //Helper.CommonHelper.DeleteWell();
                TelerikObject.mgr.Dispose();
            }


        }
    }
}
