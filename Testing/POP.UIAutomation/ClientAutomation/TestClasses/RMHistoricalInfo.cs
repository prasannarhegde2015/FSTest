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
    public class RMHistoricalInfo : ForeSiteAutoBase
    {
        public RMHistoricalInfo()
        {
            //PageEquipmentConfiguration pageEqConfig = new PageEquipmentConfiguration();
        }

        public void RM_HistoricalInfo_Script()
        {
            try
            {

                ////test data initialize
                string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "RMHistoricalInfo.xml");

                //used for local setup testing
                //string testdatafile = Path.Combine("C:\\tanvi.amin_USDCVDIFSTDV027_9727\\UQA\\AssetsDev\\UIAutomation\\TestData\\RMHistoricalInfo.xml");
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

                //historical data
                string histinfo_jobType = dt.Rows[0]["HistInfo_JobType"].ToString();
                string histinfo_serialNo = dt.Rows[0]["HistInfo_SerialNo"].ToString();
                string histinfo_wellName = dt.Rows[0]["HistInfo_WellName"].ToString();
                string histinfo_installDate = dt.Rows[0]["HistInfo_InstallDate"].ToString();
                string histinfo_pullDate = dt.Rows[0]["HistInfo_PullDate"].ToString();
                string histinfo_IPinstallDate = dt.Rows[0]["HistInfo_IP_InstallDate"].ToString();
                string histinfo_IPpullDate = dt.Rows[0]["HistInfo_IP_PullDate"].ToString();
                string histinfo_createSerialno = dt.Rows[0]["HistInfo_CreateSerialNo"].ToString();
                string histinfo_createWell = dt.Rows[0]["HistInfo_CreateSerialNo"].ToString();

                //reference data
                string compName = dt.Rows[0]["RefData_Company"].ToString();
                string location = dt.Rows[0]["RefData_Location"].ToString();
                string busOrgName = dt.Rows[0]["RefData_BusOrgName"].ToString();
                string country = dt.Rows[0]["RefData_Country"].ToString();
                string state = dt.Rows[0]["RefData_State"].ToString();
                //string wellname = "Well 1";
                #endregion Test Data


                //launch browser
                TelerikObject.InitializeManager();

                Thread.Sleep(15 * 1000);

                //click configuration tab
                TelerikObject.Click(PageEquipmentConfiguration.eqconfig_Configtab);

                //click equipment configuration
                TelerikObject.Click(PageEquipmentConfiguration.eqconfig_EquipmentConfigtab);

                Thread.Sleep(5 * 1000);

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

                //click on Add button
                TelerikObject.Click(PageEquipmentConfiguration.eqconfig_Addbtn);

                Thread.Sleep(5 * 1000);

                //verify Add New window
                Assert.IsNotNull(PageEquipmentConfiguration.eqconfig_addNew_div);
                Helper.CommonHelper.TraceLine("Add New product structure window is displayed");

                //add new product structure data
                string[] eqconfig_prodstruct = addNew_productStructure.Split(',');
                string[] eqconfig_qty = addNew_qty.Split(',');
                string[] oemerp = new string[eqconfig_prodstruct.Length];

                try
                {
                    //add product structures
                    for (int i = 0; i < eqconfig_prodstruct.Length; i++)
                    {
                        string ERP_OEM = PageEquipmentConfiguration.addProdStructure(eqconfig_prodstruct[i], oemerp[i], eqconfig_qty[i]);

                        //splitting OEM[0] and ERP[1]
                        string[] erp_oem = ERP_OEM.Split('|');

                        //verify table
                        PageEquipmentConfiguration.verifyProductStructureGrid(i, eqconfig_prodstruct[i], erp_oem[0], erp_oem[1], eqconfig_qty[i]);

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

                //click on Save
                TelerikObject.Click(PageEquipmentConfiguration.eqconfig_Savebtn);
                Thread.Sleep(5 * 1000);

                //click on condition details tab
                TelerikObject.Click(PageEquipmentConfiguration.eqconfig_ConditionDetails);

                Thread.Sleep(5 * 1000);

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

                ////click Add

                //additional info data
                string[] addInfo_label = addInfo_text.Split(',');
                string[] addInfo_showHide = addInfo_showhide.Split(',');
                string[] addinfo_mandate = addInfo_mandatory.Split(',');

                //additional info
                PageEquipmentConfiguration.additionalInfo_AddFields(addInfo_label[0], addInfo_showHide[0], addinfo_mandate[0]);

                Thread.Sleep(5 * 1000);

                //select field service configuration
                PageEquipmentConfiguration.refData();

                //create new business organization
                PageEquipmentConfiguration.createBusinessOrg(busOrgName, country, state);

                Thread.Sleep(8 * 1000);

                //press pagedown
                TelerikObject.Keyboard_Send(System.Windows.Forms.Keys.PageDown);
                Thread.Sleep(3 * 1000);


                //create new service location
                PageEquipmentConfiguration.createServiceLoc(equipmentType, compName, location);

                Thread.Sleep(5 * 1000);

                //historical information
                ////click RM tab
                //TelerikObject.Click(PageEquipmentConfiguration.rm_RMtab);

                //click historical info tab
                TelerikObject.Click(PageEquipmentConfiguration.rm_HistoricalTab);
                Thread.Sleep(5 * 1000);

                ////histinfo job
                //jobtype,well,serial number,install date, pull date,IP install, IP pull
                string[] histInfo_jobType = histinfo_jobType.Split(',');
                string[] histInfo_createSerialNo = histinfo_createSerialno.Split(',');
                string[] histInfo_serialNo = histinfo_serialNo.Split(',');
                string[] histInfo_wellName = histinfo_wellName.Split(',');
                string[] histInfo_createWell = histinfo_createWell.Split(',');

                for (int i = 0; i < histInfo_jobType.Length; i++)
                {
                    if (histInfo_jobType[i].Contains("and"))
                    {
                        string temp = histInfo_jobType[i].Replace("and", "&");
                        histInfo_jobType[i] = temp;
                    }
                    PageEquipmentConfiguration.histInfo_Job(equipmentType, equipmentSubType, histInfo_jobType[i], location, histInfo_createSerialNo[i], histInfo_serialNo[i],
                    busOrgName, histInfo_createWell[i], histInfo_wellName[i], histinfo_installDate, histinfo_pullDate, histinfo_IPinstallDate, histinfo_IPpullDate);

                    Thread.Sleep(2 * 1000);
                }

                Thread.Sleep(5 * 1000);
            }
            catch (Exception e)
            {
                Helper.CommonHelper.PrintScreen("Historical Information flow");
                Helper.CommonHelper.TraceLine("Error " + e);
                Assert.Fail("Historical Information Test has failed");

            }
            finally
            {
                //Helper.CommonHelper.DeleteWell();
                TelerikObject.mgr.Dispose();
            }


        }
    }
}
