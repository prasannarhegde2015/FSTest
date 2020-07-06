using ArtOfTest.WebAii.Controls.HtmlControls;
using ArtOfTest.WebAii.ObjectModel;
using ArtOfTest.WebAii.Core;
using Telerik.TestingFramework.Controls.KendoUI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Weatherford.POP.DTOs;
using ClientAutomation.TelerikCoreUtils;
using System.Configuration;
using System.IO;
using System.Threading;


namespace ClientAutomation.PageObjects
{
    class PageEquipmentConfiguration : PageMasterDefinition
    {
        public static string DynamicValue = null;
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        //private static Manager mgr = null;

        #region PageControls

        public static HtmlControl eqconfig_Configtab { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "Content", "Configuration", "Configuration tab"); } }

        public static HtmlControl eqconfig_EquipmentConfigtab { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Equipment Configuration", "Equipment Configuration tab"); } }

        public static HtmlControl rm_RMtab { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "Content", "Repair & Maintenance", "Repair & Maintenance tab"); } }

        public static HtmlControl fieldserviceConfig_tab { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "Content", "Field Service Configuration", "Field Service Configuration tab"); } }

        public static HtmlControl rm_HistoricalTab { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "Content", "Historical Service Job", "Historical Service Job tab"); } }

        public static HtmlDiv refData_div { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "attribute", "class=fsc-container", "Reference data Div"); } }

        public static HtmlDiv refdataTable_div { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "attribute", "class=jobCategoryBody", "Reference data table Div"); } }

        public static HtmlDiv refdataBusOrg_div { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "attribute", "id=dynamicModel", "Reference data Business Org Div"); } }

        public static HtmlButton refData_Addbtn { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Add", "Add Button"); } }

        //public static HtmlControl refData_busOrgType { get { return TelerikObject.GetElement("kendo-combobox", "id", "venFK_r_BusinessOrganizationTy", "Business organisation type combobox"); } }


        public static HtmlControl refData_busOrgState { get { return TelerikObject.GetElement("kendo-combobox", "id", "venFK_r_StateProvince", "Business organisation state/province combobox"); } }

        public static HtmlControl refData_busOrgCountry { get { return TelerikObject.GetElement("kendo-combobox", "id", "venFK_r_Country", "Business organisation Country combobox"); } }

        public static HtmlInputText refData_busOrgName { get { return (HtmlInputText)TelerikObject.GetElement(CONTROLTYPE_HTMLINPUTTEXT, "id", "venBusinessOrganizationName", "Business organization name text field"); } }

        public static HtmlButton refData_Savebtn { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Save", "Save Button"); } }

        public static HtmlControl refData_servLoc_EquipmentType { get { return TelerikObject.GetElement("kendo-combobox", "id", "rslFK_r_EquipmentType", "Service-Location Equipment Type combobox"); } }

        public static HtmlInputText refData_servLoc_CompanyName { get { return (HtmlInputText)TelerikObject.GetElement(CONTROLTYPE_HTMLINPUTTEXT, "id", "rslCompanyName", "service location company name  text field"); } }

        public static HtmlInputText refData_servLoc_Location { get { return (HtmlInputText)TelerikObject.GetElement(CONTROLTYPE_HTMLINPUTTEXT, "id", "rslLocation", "service location name text field"); } }

        public static HtmlControl eqconfig_EquipmentSelect { get { return TelerikObject.GetElement("kendo-dropdownlist", "name", "EquipmentType", "Equipment Selection"); } }

        public static HtmlControl eqconfig_EquipmentSubSelect { get { return TelerikObject.GetElement("kendo-dropdownlist", "name", "EquipmentSubType", "Equipment Selection"); } }

        //public static HtmlControl eqconfig_Addbtn { get { return TelerikObject.GetElement("kendobutton", "content", " Add  ", "Add Button"); } }

        public static HtmlButton eqconfig_Addbtn { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Add", "Add Button"); } }

        public static HtmlButton eqconfig_Savebtn { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Save", "Save Button"); } }

        public static HtmlButton eqconfig_Cancelbtn { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Cancel", "Cancel Button"); } }


        public static HtmlDiv eqconfig_addNew_div { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "attribute", "class=k-window-title k-dialog-title", "Add New"); } }

        public static HtmlControl addNew_ProdStructureSelect { get { return TelerikObject.GetElement("kendo-dropdownlist", "name", "productStructureId", "Product Structure Selection"); } }

        public static HtmlInputText addNew_OEMPartNo_text { get { return (HtmlInputText)TelerikObject.GetElement(CONTROLTYPE_HTMLINPUTTEXT, "name", "mfgNumber", "OEM Part Number text field"); } }

        public static HtmlInputText addNew_ERPPartNo_text { get { return (HtmlInputText)TelerikObject.GetElement(CONTROLTYPE_HTMLINPUTTEXT, "name", "erpNumber", "ERP Part Number text field"); } }

        public static HtmlInputText addNew_Quantity_text { get { return (HtmlInputText)TelerikObject.GetElement(CONTROLTYPE_HTMLINPUTTEXT, "name", "productQuantity", "Quantity text field"); } }

        public static HtmlButton addNew_Cancelbtn { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Cancel", "Cancel Buttonin Add New Product Structure"); } }

        public static HtmlButton addNew_Addbtn { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Add", "Add Buttonin Add New Product Structure"); } }

        public static HtmlControl eqconfig_ToggleButtons { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", DynamicValue, DynamicValue); } }

        public static HtmlControl eqconfig_DispWeartab { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Display Wear Tab:", "Display Wear Tab toggle button"); } }

        public static HtmlControl eqconfig_SingleInstall { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Single Installation:", "Single installation Tab toggle button"); } }

        public static HtmlControl eqconfig_SerialNumberReqd { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Serial Number Required:", "Serial Number Reqd toggle button"); } }

        public static HtmlControl eqconfig_ConditionDetails { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Condition Details", "Condition Details tab"); } }

        public static HtmlControl eqconfig_PopupConfirm { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Confirmation", "Delete Confirmation window"); } }

        public static HtmlControl eqconfig_PopupEdit { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Edit", "Edit window"); } }

        public static HtmlControl conditionDetails_FailureCond1 { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Failure Condition 1", "Failure condition 1"); } }

        public static HtmlControl conditionDetails_FailureCond2 { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Failure Condition 2", "Failure condition 2"); } }

        public static HtmlControl conditionDetails_FailureCause1 { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Failure Cause 1", "Failure cause 1"); } }

        public static HtmlControl conditionDetails_FailureCause2 { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Failure Cause 2", " Failure Cause 2 "); } }

        public static HtmlControl conditionDetails_PrimaryFailCause { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Primary Failure Cause", "Primary Failure Cause"); } }

        public static HtmlControl conditionDetails { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", DynamicValue, DynamicValue); } }

        public static HtmlControl conditionDetailsForeignMaterial2 { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Foreign Material 2", "Foreign Material 2"); } }

        public static HtmlControl conditionDetails_ForeignMatSample { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Foreign Material Sample", "Foreign material sample"); } }

        public static HtmlControl conditionDetails_Manufacturer { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", " Manufacturer ", "Manufacturer"); } }

        public static HtmlControl conditionDetails_OEMPartNo { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", " OEM Part No. ", "OEM Part number"); } }

        public static HtmlControl conditionDetails_ERPPartNo { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", " ERP Part No. ", "ERP Part number"); } }

        public static HtmlControl conditionDetails_Action { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", " Action ", "Action"); } }

        public static HtmlButton conditionDetails_Savebtn { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Save", "Save Button"); } }

        public static HtmlButton conditionDetails_Cancelbtn { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Cancel", "Cancel Button"); } }

        public static HtmlControl eqconfig_AdditionalInfo { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "content", "Additional Information", "Additional Information"); } }

        public static HtmlButton additionalInfo_Addbtn { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Add", "Add Button"); } }

        public static HtmlButton additionalInfo_Savebtn { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Save", "Save Button"); } }

        public static HtmlButton additionalInfo_Cancelbtn { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Cancel", "Cancel Button"); } }

        public static HtmlTable additionalInfo_table { get { return (HtmlTable)TelerikObject.GetElement("HtmlTable", "attribute", "class=ng-star-inserted", "Additional Info table"); } }

        public static HtmlControl additionalInfo_grid { get { return (HtmlControl)TelerikObject.GetElement("HtmlControl", "attribute", "id=addInfo-tableAdditionalInfo", "Additional Info grid"); } }

        public static HtmlControl histInfo_EquipmentSelect { get { return TelerikObject.GetElement("kendo-dropdownlist", "name", "equipmentType", "Historical Equipment type Selection"); } }

        public static HtmlControl histInfo_EquipmentSubSelect { get { return TelerikObject.GetElement("kendo-dropdownlist", "name", "equipmentSubType", "Historical Equipment subtype Selection"); } }

        public static HtmlControl histInfo_JobType { get { return TelerikObject.GetElement("kendo-dropdownlist", "name", "jobType", "Historical Job Type Selection"); } }

        public static HtmlButton histInfo_CreateNewbtn { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "attribute", "class=serviceJob-add-icon ng-star-inserted", "Add new Service Number serial number"); } }

        public static HtmlInputText histInfo_serialnoText { get { return (HtmlInputText)TelerikObject.GetElement(CONTROLTYPE_HTMLINPUTTEXT, "name", "serialNumber", "serialNumber  text field"); } }

        public static HtmlControl histInfo_customerName { get { return TelerikObject.GetElement("kendo-combobox", "name", "businessOrganization", "customer name combobox"); } }

        public static HtmlButton histInfo_Savebtn { get { return (HtmlButton)TelerikObject.GetElement(CONTROLTYPE_HTMLBUTTON, "content", "Save", "Save Button"); } }

        public static HtmlControl histInfo_wellName_combo { get { return TelerikObject.GetElement("kendo-combobox", "name", "wellId", "Well name combobox"); } }

        public static HtmlInputText histInfo_wellNameText { get { return (HtmlInputText)TelerikObject.GetElement(CONTROLTYPE_HTMLINPUTTEXT, "name", "newWellName", "New Well name text field"); } }

        public static HtmlControl histInfo_wellType { get { return TelerikObject.GetElement("kendo-dropdownlist", "name", "WellType", "Historical Job Type Selection"); } }

        public static HtmlControl histInfo_commissionDate { get { return TelerikObject.GetElement("kendo-dateinput", "id", "comissionDate", "Commission Date"); } }

        //public static Element histInfo_commissionDate { get { return TelerikObject.GetElement("id", "comissionDate", "Commission Date"); } }

        public static HtmlControl histInfo_installDate { get { return TelerikObject.GetElement("kendo-dateinput", "id", "installDate", "Install Date"); } }

        //public static Element histInfo_installDate { get { return TelerikObject.GetElement("id", "beginDate", "Install Date"); } }

        public static HtmlControl histInfo_pullDate { get { return TelerikObject.GetElement("kendo-dateinput", "id", "pullDate", "Pull Date"); } }

        //public static Element histInfo_pullDate { get { return TelerikObject.GetElement("id", "beginDate", "Pull Date"); } }

        public static HtmlControl histInfo_serviceLocation { get { return TelerikObject.GetElement("kendo-dropdownlist", "name", "serviceLocation", "Historical service location Selection"); } }

        public static HtmlDiv histInfo_div { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "attribute", "id=divServiceJobStart", "Historical Info Div"); } }

        public static HtmlDiv histInfo_serialNodiv { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "attribute", "id=service-serialNumberKendoPopUp", "Historical Info Serial Number Div"); } }

        public static HtmlDiv histInfo_createWelldiv { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "attribute", "id=divNewWell", "Historical Info Create Well Div"); } }

        #endregion PageControls


        #region Functions Equipment Config

        public static void additionalInfo_AddFields(string additionalinfo, string actionShowHide, string actionRequired)
        {
            try
            {
                Thread.Sleep(5 * 1000);

                //previous row count
                int prev_rowcount = TelerikObject.getTableByColumnName("Action").Rows.Count;

                //click add button
                TelerikObject.Click(additionalInfo_Addbtn);

                Thread.Sleep(5 * 1000);

                HtmlTable table = TelerikObject.getTableByColumnName("Display Label");

                int index = TelerikObject.getIndexofColumnName(table, "Display Label");

                //enter text
                TelerikObject.Sendkeys(table.AllRows[1].Cells[index], additionalinfo);

                Thread.Sleep(7 * 1000);

                //for show/hide and mandatory
                action_additionalInfo(1, actionShowHide, actionRequired);

                Thread.Sleep(5 * 1000);

                //click Add
                additionalInfo_Icons("Add", 1, index - 1);

                //verify table count
                int rowcount = TelerikObject.getTableByColumnName("Action").Rows.Count;

                if (rowcount == prev_rowcount + 1)
                {
                    Helper.CommonHelper.TraceLine("The new additional info field is added to the grid");
                }
                else
                {
                    Helper.CommonHelper.TraceLine("The new additional info field is not added to the grid");
                }
            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Additional Information Failed. " + ex);
            }

            Thread.Sleep(5 * 1000);

            // click save
            TelerikObject.Click(additionalInfo_Savebtn);

        }

        public static void AddNew_AddCancelButtons(string button_name)
        {
            try
            {
                Thread.Sleep(3 * 1000);

                HtmlControl add_div = TelerikObject.GetParent(eqconfig_addNew_div, 1);

                HtmlControl buttondiv = TelerikObject.GetNextSibling(add_div, 2);

                Thread.Sleep(5 * 1000);

                for (int i = 0; i < buttondiv.ChildNodes.Count; i++)
                {
                    if (buttondiv.ChildNodes[i].InnerText.Equals(button_name))
                    {
                        TelerikObject.Click(buttondiv.ChildNodes[i]);
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Failed to click on Add button. " + ex);
            }
        }

        public static void getToggleButton(string labelname, string action)
        {
            try
            {
                DynamicValue = labelname;
                Thread.Sleep(5000);

                HtmlControl parent = TelerikObject.GetParent(eqconfig_ToggleButtons, 1);

                //for API Description
                HtmlControl immediate_parent = TelerikObject.GetChildrenControl(parent, "0");

                if (labelname == "Display Wear Tab")
                {
                    immediate_parent = TelerikObject.GetNextSibling(immediate_parent, 1);
                }
                else if (labelname == "Single Installation")
                {
                    immediate_parent = TelerikObject.GetNextSibling(immediate_parent, 2);
                }
                else if (labelname == "Serial Number Required")
                {
                    immediate_parent = TelerikObject.GetNextSibling(immediate_parent, 3);
                }

                //    HtmlControl parent_ctl = TelerikObject.GetChildrenControl(immediate_parent, "0;0;0");
                HtmlControl parent_ctl = TelerikObject.GetChildrenControl(immediate_parent, "0;0");


                if (action == "ON")
                {
                    TelerikObject.Click(parent_ctl.ChildNodes[1]);
                    Helper.CommonHelper.TraceLine("Toggle button " + labelname + " is selected successfully");

                }
                else
                {
                    TelerikObject.Click(parent_ctl.ChildNodes[0]);
                    Helper.CommonHelper.TraceLine("Toggle button " + labelname + " is de-selected successfully");
                }


            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Unable to select toggle button. " + ex);
            }
        }

        public static void VerifyAction_ERP_Disabled()
        {
            try
            {
                //For ERP Part number
                HtmlControl parent_eyeOpenClose = TelerikObject.GetNextSibling(conditionDetails_ERPPartNo, 1);

                HtmlControl eyeOpenClose = TelerikObject.GetChildrenControl(parent_eyeOpenClose, "0;0");

                HtmlControl reqdIcon_parent = TelerikObject.GetNextSibling(parent_eyeOpenClose, 2);

                HtmlControl reqdIcon = TelerikObject.GetChildrenControl(reqdIcon_parent, "0");

                //verifying ERP number is shown and mandatory
                if (eyeOpenClose.BaseElement.GetAttribute("class").Value.Contains("close") &&
                    reqdIcon.BaseElement.GetAttribute("class").Value.Contains("switch-off"))
                {
                    Helper.CommonHelper.TraceLine("In condition details, the ERP number field is marked as shown and mandatory");
                }
                else
                {
                    Helper.CommonHelper.TraceLine("In condition details, the ERP number field is not marked as shown and mandatory");
                }

                // For Action field
                HtmlControl parent_eyeOpenClose1 = TelerikObject.GetNextSibling(conditionDetails_ERPPartNo, 1);

                HtmlControl eyeOpenClose1 = TelerikObject.GetChildrenControl(parent_eyeOpenClose1, "0;0");

                HtmlControl reqdIcon_parent1 = TelerikObject.GetNextSibling(parent_eyeOpenClose1, 2);

                HtmlControl reqdIcon1 = TelerikObject.GetChildrenControl(reqdIcon_parent1, "0");

                //verifying Action is shown and mandatory
                if (eyeOpenClose1.BaseElement.GetAttribute("class").Value.Contains("close") &&
                    reqdIcon1.BaseElement.GetAttribute("class").Value.Contains("switch-off"))
                {
                    Helper.CommonHelper.TraceLine("In condition details, the Action field is marked as shown and mandatory");
                }
                else
                {
                    Helper.CommonHelper.TraceLine("In condition details, the Action field is not marked as shown and mandatory");
                }
            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("The validation for ERP number and Action toggle buttons failed. " + ex);
            }
        }


        public static void gridConditionDetails(string label, string actionShowHide, string actionRequired)
        {
            try
            {
                DynamicValue = label;

                if (conditionDetails.BaseElement.InnerText == label)
                {
                    HtmlControl parentsibling = TelerikObject.GetNextSibling(conditionDetails, 1);

                    HtmlControl eyeOpenClose = TelerikObject.GetChildrenControl(parentsibling, "0;0");

                    //to hide or show a condition detail label
                    if ((actionShowHide.ToUpper() == "SHOW" && eyeOpenClose.BaseElement.GetAttribute("class").Value.Contains("close"))
                    || (actionShowHide.ToUpper() == "HIDE" && eyeOpenClose.BaseElement.GetAttribute("class").Value.Contains("open")))
                    {
                        eyeOpenClose.Click();
                    }

                    Thread.Sleep(5 * 1000);
                    //to mark the label mandatory or not mandatory
                    HtmlControl reqdIcon_parent = TelerikObject.GetNextSibling(conditionDetails, 2);

                    //HtmlControl reqdIcon = TelerikObject.GetChildrenControl(reqdIcon_parent, "0;0;0");
                    HtmlControl reqdIcon = TelerikObject.GetChildrenControl(reqdIcon_parent, "0;0");

                    if (actionRequired.ToUpper() == "ON")
                    {
                        TelerikObject.Click(reqdIcon.ChildNodes[1]);
                        Helper.CommonHelper.TraceLine("Condition Details toggle button " + label + " is selected successfully");

                    }
                    else
                    {
                        TelerikObject.Click(reqdIcon.ChildNodes[0]);
                        Helper.CommonHelper.TraceLine("Condition Details toggle button " + label + " is de-selected successfully");
                    }

                }
            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Condition Details selection failed. " + ex);
            }
        }

        public static string addProdStructure(string eq_prodStruct, string oemerp, string quantity)
        {
            try
            {
                //add product structures

                //select product structure
                TelerikObject.Click(PageEquipmentConfiguration.addNew_ProdStructureSelect);
                TelerikObject.Select_KendoUI_Listitem(eq_prodStruct, true);

                Thread.Sleep(2 * 1000);

                //enter ERP number
                string erpno = PageEquipmentConfiguration.addNew_ERPPartNo_text.Text;

                oemerp = erpno;

                //enter OEM Part number
                string oemno = PageEquipmentConfiguration.addNew_OEMPartNo_text.Text;

                //enter quantity
                TelerikObject.Sendkeys(PageEquipmentConfiguration.addNew_Quantity_text, quantity);

                //click on Add
                PageEquipmentConfiguration.AddNew_AddCancelButtons("Add");

                Thread.Sleep(7 * 1000);

                return oemno + "|" + erpno;


            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Add New Product Structure Failed. " + ex);
                return null;
            }

        }

        public static void verifyProductStructureGrid(int rowindex, string prodStructure, string OEMPartNo, string erpPartNo, string qty)
        {
            string colnames = "Action;Product Structure;OEM Part Number;ERP Part Number;Part Description;Metal Type;Surface Coating;UOM;QTY";

            string action = "ignorevalue";
            string prodstructure = prodStructure;
            string oemPartNo = OEMPartNo;
            string erpPartno = erpPartNo;
            string partDesc = "ignorevalue";
            string metalType = "ignorevalue";
            string surfaceCoating = "ignorevalue";
            string UOM = "ignorevalue";
            string QTY = qty;

            string colvals = action + ";" + prodstructure + ";" + oemPartNo + ";" + erpPartno + ";" + partDesc + ";" + metalType + ";" + surfaceCoating + ";" + UOM + ";" + QTY;

            TelerikObject.verifyGridCellValues(colnames, colvals, rowindex);

        }

        public static void editProductStructure(string prodStructure, string oemNo, string erpNo, string qty,
                        string prodStrEdit = "", string oemNoEdit = "", string erpNoEdit = "", string qtyEdit = "")
        {
            try
            {
                //int rowscount = 0;
                Manager mg = TelerikObject.mgr;
                mg.ActiveBrowser.RefreshDomTree();
                ReadOnlyCollection<HtmlTable> allHtmlTables = mg.ActiveBrowser.Find.AllByExpression<HtmlTable>("tagname=table");
                HtmlTable tbl = allHtmlTables.FirstOrDefault(t => t.InnerText.Contains("presentation"));
                if (tbl == null)
                {
                    //more than one table
                    //column nos.
                    int prod_Col = TelerikObject.getIndexofColumnName(allHtmlTables[0], "Product Structure");
                    int oem_Col = TelerikObject.getIndexofColumnName(allHtmlTables[0], "OEM Part Number");
                    int erp_Col = TelerikObject.getIndexofColumnName(allHtmlTables[0], "ERP Part Number");
                    int qty_Col = TelerikObject.getIndexofColumnName(allHtmlTables[0], "QTY");

                    for (int i = 0; i < allHtmlTables[1].Rows.Count; i++)
                    {
                        if (allHtmlTables[1].AllRows[i].Cells[prod_Col].InnerText == prodStructure &&
                            allHtmlTables[1].AllRows[i].Cells[oem_Col].InnerText == oemNo &&
                            allHtmlTables[1].AllRows[i].Cells[erp_Col].InnerText == erpNo &&
                            allHtmlTables[1].AllRows[i].Cells[qty_Col].InnerText == qty)
                        {
                            if (allHtmlTables[1].AllRows[i].Cells[0].BaseElement.Children[0].Children[0].GetAttribute("class").Value.Contains("edit"))
                            {
                                //click edit icon
                                TelerikObject.Click(allHtmlTables[1].AllRows[i].Cells[0].BaseElement.Children[0].Children[0]);
                            }

                            Thread.Sleep(5 * 1000);

                            //edit window is displayed
                            if (prodStrEdit != "")
                            {
                                //edit dropdown
                                TelerikObject.Click(addNew_ProdStructureSelect);
                                TelerikObject.Select_KendoUI_Listitem(prodStrEdit, true);
                            }

                            Thread.Sleep(3 * 1000);

                            if (oemNoEdit != "")
                            {
                                string j = addNew_OEMPartNo_text.Text;
                                addNew_OEMPartNo_text.Text = oemNoEdit;
                            }

                            if (erpNoEdit != "")
                            {
                                addNew_ERPPartNo_text.Text = erpNoEdit;
                            }

                            if (qtyEdit != "")
                            {
                                addNew_Quantity_text.Text = qtyEdit;
                            }

                            Thread.Sleep(5 * 1000);
                            HtmlControl edit_popup = TelerikObject.GetParent(eqconfig_PopupEdit, 1);
                            HtmlControl buttondiv = TelerikObject.GetNextSibling(edit_popup, 2);

                            for (int j = 0; j < buttondiv.ChildNodes.Count; j++)
                            {
                                if (buttondiv.ChildNodes[j].InnerText == "Update")
                                {
                                    //click update
                                    TelerikObject.Click(buttondiv.ChildNodes[j]);
                                    break;
                                }
                            }

                            Thread.Sleep(3 * 1000);

                            //click Save
                            TelerikObject.Click(eqconfig_Savebtn);
                            Thread.Sleep(5 * 1000);

                            if (qtyEdit == "")
                            {
                                qtyEdit = "0";
                            }

                            //verify edited values
                            verifyProductStructureGrid(i, prodStrEdit, oemNoEdit, erpNoEdit, qtyEdit);
                            Thread.Sleep(5 * 1000);

                            break;
                        }
                    }
                }
                else
                {
                    Assert.Fail("No product structures available in the table");
                }
            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Product structure Delete functionality failed. " + ex);
            }
        }

        public static void delProductStructure(string prodStruct, string oemNo, string erpNo, string qty)
        {
            try
            {
                int rowscount = 0;
                Manager mg = TelerikObject.mgr;
                mg.ActiveBrowser.RefreshDomTree();
                ReadOnlyCollection<HtmlTable> allHtmlTables = mg.ActiveBrowser.Find.AllByExpression<HtmlTable>("tagname=table");
                HtmlTable tbl = allHtmlTables.FirstOrDefault(t => t.InnerText.Contains("presentation"));
                if (tbl == null)
                {
                    //more than one table
                    rowscount = allHtmlTables[1].Rows.Count;
                }
                else
                {
                    Assert.Fail("No product structures available in the table");
                }

                //column nos.
                int prod_Col = TelerikObject.getIndexofColumnName(allHtmlTables[0], "Product Structure");
                int oem_Col = TelerikObject.getIndexofColumnName(allHtmlTables[0], "OEM Part Number");
                int erp_Col = TelerikObject.getIndexofColumnName(allHtmlTables[0], "ERP Part Number");
                int qty_Col = TelerikObject.getIndexofColumnName(allHtmlTables[0], "QTY");

                for (int i = 0; i < allHtmlTables[1].Rows.Count; i++)
                {
                    if (allHtmlTables[1].AllRows[i].Cells[prod_Col].InnerText == prodStruct &&
                        allHtmlTables[1].AllRows[i].Cells[oem_Col].InnerText == oemNo &&
                        allHtmlTables[1].AllRows[i].Cells[erp_Col].InnerText == erpNo &&
                        allHtmlTables[1].AllRows[i].Cells[qty_Col].InnerText == qty)
                    {
                        if (allHtmlTables[1].AllRows[i].Cells[0].BaseElement.Children[1].Children[0].GetAttribute("class").Value.Contains("trash"))
                        {
                            TelerikObject.Click(allHtmlTables[1].AllRows[i].Cells[0].BaseElement.Children[1].Children[0]);
                        }
                        break;
                    }
                }

                //confirmation pop-up
                confirmPopup("Yes");

                Thread.Sleep(5 * 1000);

                TelerikObject.Click(eqconfig_Savebtn);

                Thread.Sleep(5 * 1000);

                int after_rowcount = 0;
                Manager mg1 = TelerikObject.mgr;
                mg1.ActiveBrowser.RefreshDomTree();
                ReadOnlyCollection<HtmlTable> allHtmlTables1 = mg1.ActiveBrowser.Find.AllByExpression<HtmlTable>("tagname=table");
                HtmlTable tbls = allHtmlTables1.FirstOrDefault(t => t.InnerText.Contains("presentation"));
                if (tbls == null)
                {
                    //code to verify deleted row
                    //after delete row count
                    after_rowcount = allHtmlTables1[1].Rows.Count;
                    if (after_rowcount == rowscount - 1)
                    {
                        Helper.CommonHelper.TraceLine("The product structure is deleted successfully");
                    }
                    else
                    {
                        Helper.CommonHelper.TraceLine("The product structure is not deleted successfully");
                    }
                }
                else
                {
                    //no product structures available in the table
                    Helper.CommonHelper.TraceLine("The product structure is deleted successfully");
                }
                Thread.Sleep(5 * 1000);
            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Product Structure Delete functionality failed. " + ex);
            }
        }

        public static void delEdit_AdditionalInfo(string action, int rowToEdit_Del, string addinfoEdit = "", string actionShowHide = "",
                                            string actionMandatory = "")
        {
            try
            {
                //verify previous table count
                HtmlTable table = TelerikObject.getTableByColumnName("Display Label");
                int prev_rowcount = table.Rows.Count;
                int rowcount = 0;

                if (prev_rowcount == 0)
                {
                    Assert.Fail("No records present in the Additional info table");
                }
                else
                {
                    Thread.Sleep(5 * 1000);

                    int index = TelerikObject.getIndexofColumnName(table, "Display Label");

                    for (int i = 0; i <= prev_rowcount; i++)
                    {
                        if (i + 1 == rowToEdit_Del)
                        {
                            if (action == "Edit")
                            {
                                //click Edit
                                additionalInfo_Icons("Edit", i + 1, index - 1);

                                //Edit functionality
                                if (addinfoEdit != "")
                                {
                                    //edit additional info field
                                    TelerikObject.Sendkeys(table.AllRows[i + 1].Cells[index], addinfoEdit);
                                }

                                //for show/hide and mandatory
                                action_additionalInfo(i + 1, actionShowHide, actionMandatory);

                                //click Update
                                additionalInfo_Icons("Update", i + 1, index - 1);

                                Thread.Sleep(5 * 1000);
                                TelerikObject.Click(additionalInfo_Savebtn);
                                Thread.Sleep(5 * 1000);
                                break;
                            }
                            else
                            {
                                //delete functionality
                                //click delete
                                additionalInfo_Icons("Delete", i + 1, index - 1);

                                Thread.Sleep(5 * 1000);

                                //confirmation pop-up
                                confirmPopup("Yes");

                                //verify table count
                                rowcount = TelerikObject.getTableByColumnName("Action").Rows.Count;

                                // click save
                                TelerikObject.Click(additionalInfo_Savebtn);
                                Thread.Sleep(5 * 1000);
                                if (rowcount == prev_rowcount - 1)
                                {
                                    Helper.CommonHelper.TraceLine("The record is deleted successfully");
                                }
                                else
                                {
                                    Helper.CommonHelper.TraceLine("The record is not deleted successfully");
                                }
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Additional Info Edit/Delete functionality failed. " + ex);
            }
        }

        public static void additionalInfo_Icons(string action, int row, int index)
        {
            HtmlTable table = TelerikObject.getTableByColumnName("Display Label");

            for (int j = 0; j < table.AllRows[row].Cells[index].BaseElement.Children.Count; j++)
            {
                string jl = table.AllRows[row].Cells[index].BaseElement.Children[j].GetAttribute("title").Value;
                if (table.AllRows[row].Cells[index].BaseElement.Children[j].GetAttribute("title").Value == action)
                {
                    TelerikObject.Click(table.AllRows[row].Cells[index].BaseElement.Children[j]);
                    break;
                }
            }
        }

        public static void confirmPopup(string action)
        {
            //confirmation pop-up
            HtmlControl confirm_popup = TelerikObject.GetParent(eqconfig_PopupConfirm, 1);

            HtmlControl buttondiv = TelerikObject.GetNextSibling(confirm_popup, 2);

            for (int i = 0; i < buttondiv.ChildNodes.Count; i++)
            {
                if (buttondiv.ChildNodes[i].InnerText == action)
                {
                    TelerikObject.Click(buttondiv.ChildNodes[i]);
                    break;
                }
            }

            Thread.Sleep(5 * 1000);
        }

        public static void action_additionalInfo(int row, string actionShowHide, string actionMandatory)
        {
            Thread.Sleep(5 * 1000);
            HtmlTable table = TelerikObject.getTableByColumnName("Display Label");
            int index = TelerikObject.getIndexofColumnName(table, "Display Label");

            if (actionShowHide != "")
            {
                string r = table.AllRows[row].Cells[index + 1].BaseElement.Children[0].Children[0].GetAttribute("class").Value;
                string rr = table.AllRows[row].Cells[index + 1].BaseElement.Children[0].Children[0].GetAttribute("class").Value;
                //open close values 
                if ((actionShowHide.ToUpper() == "SHOW" && table.AllRows[row].Cells[index + 1].BaseElement.Children[0].Children[0].GetAttribute("class").Value.Contains("close"))
                    || (actionShowHide.ToUpper() == "HIDE" && table.AllRows[row].Cells[index + 1].BaseElement.Children[0].Children[0].GetAttribute("class").Value.Contains("open")))
                {
                    TelerikObject.Click(table.AllRows[row].Cells[index + 1].BaseElement.Children[0]);
                }
            }

            Thread.Sleep(5 * 1000);

            if (actionMandatory != "")
            {
                //for required
                if (actionMandatory.ToUpper() == "ON")
                {
                    TelerikObject.Click(table.AllRows[row].Cells[index + 2].BaseElement.Children[0].Children[0].ChildNodes[1]);
                    //TelerikObject.Click(table.AllRows[row].Cells[index + 2].BaseElement.Children[0].Children[0].Children[0].ChildNodes[1]);
                }
                else if (actionMandatory.ToUpper() == "OFF")
                {
                    TelerikObject.Click(table.AllRows[row].Cells[index + 2].BaseElement.Children[0].Children[0].ChildNodes[0]);
                    //TelerikObject.Click(table.AllRows[row].Cells[index + 2].BaseElement.Children[0].Children[0].Children[0].ChildNodes[0]);
                }
            }
            Thread.Sleep(5 * 1000);
        }

        public static void equipconfig_Create(string equipmentType, string equipmentSubType)
        {
            //select equipment type 
            TelerikObject.Click(PageEquipmentConfiguration.eqconfig_EquipmentSelect);
            TelerikObject.Select_KendoUI_Listitem(equipmentType, true);

            Thread.Sleep(2 * 1000);

            //select equipment subtype
            TelerikObject.Click(PageEquipmentConfiguration.eqconfig_EquipmentSubSelect);
            TelerikObject.Select_KendoUI_Listitem(equipmentSubType, true);


        }

        public static void refData()
        {
            try
            {
                //click field service config
                TelerikObject.Click(PageEquipmentConfiguration.fieldserviceConfig_tab);

                Thread.Sleep(2 * 1000);

                //for ref data link
                HtmlControl sibling = TelerikObject.GetChildrenControl(refData_div, "0;0;0");

                HtmlControl immediate_parent = TelerikObject.GetNextSibling(sibling, 4);

                TelerikObject.Click(immediate_parent.ChildNodes[0]);

                Thread.Sleep(5 * 1000);
            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Reference Data tab Failed. " + ex);
                Assert.Fail();
            }
        }

        public static void selectRefDataTab(string tabname)
        {
            try
            {
                HtmlControl control = TelerikObject.GetChildrenControl(refdataTable_div, "0");

                HtmlControl parent = TelerikObject.GetNextSibling(control, 1);

                HtmlControl immediate_parent = TelerikObject.GetChildrenControl(parent, "0;0");

                for (int i = 1; i <= immediate_parent.ChildNodes.Count; i++)
                {
                    if (immediate_parent.ChildNodes[i].Children[0].InnerText.Equals(tabname))
                    {
                        TelerikObject.Click(immediate_parent.ChildNodes[i].Children[0]);
                        break;
                    }
                }

                Thread.Sleep(3 * 1000);

                //press home
                TelerikObject.Keyboard_Send(System.Windows.Forms.Keys.Home);

                Thread.Sleep(2 * 1000);

            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Failed to click Reference data tab. " + ex);
                Assert.Fail();
            }
        }

        public static void refData_controls(string fieldName, string value, string control = "")
        {
            try
            {
                HtmlControl immediate_parent = TelerikObject.GetChildrenControl(refdataBusOrg_div, "0");

                for (int i = 1; i < immediate_parent.ChildNodes.Count; i++)
                {
                    string j = immediate_parent.ChildNodes[i].Children[0].InnerText;
                    string k = immediate_parent.ChildNodes[i].Children[0].Children[0].InnerText;

                    //immediate_parent.ChildNodes[i].Children[1];
                    if (immediate_parent.ChildNodes[i].Children[0].InnerText == fieldName)
                    {
                        Element inputparent = immediate_parent.ChildNodes[i].Children[0].GetNextSibling();
                        //if (control == "input")
                        //{
                        //	//for edit text box
                        //	Element editbox = inputparent.Children[0];
                        //	TelerikObject.Sendkeys(editbox, value);
                        //	break;
                        //}

                        //for combobox
                        Element inputText = inputparent.Children[0].Children[0].Children[0].Children[0];
                        TelerikObject.Click(inputText);
                        Thread.Sleep(2 * 1000);
                        TelerikObject.Sendkeys(inputText, value);
                        Thread.Sleep(2 * 1000);
                        TelerikObject.Select_KendoUI_Listitem(value, true);
                        Thread.Sleep(1 * 1000);

                        break;

                    }
                }
            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Failed to find refernce data control. " + ex);
                Assert.Fail();
            }
        }

        public static void createBusinessOrg(string busOrgName, string country, string state)
        {
            try
            {
                //select Business Organization tab
                selectRefDataTab("Business Organization");

                //click Add
                TelerikObject.Click(refData_Addbtn);
                Thread.Sleep(5 * 1000);

                //select business organization type
                refData_controls("Business Organization Type", "Oil & Gas Producer");

                Thread.Sleep(5 * 1000);

                //enter business organization name 
                TelerikObject.Sendkeys(refData_busOrgName, busOrgName);

                Thread.Sleep(5 * 1000);

                //select country
                refData_controls("Country", country);

                Thread.Sleep(12 * 1000);

                //select state
                refData_controls("State/Province", state);

                Thread.Sleep(3 * 1000);

                //click Save
                TelerikObject.Click(PageEquipmentConfiguration.refData_Savebtn);
                Thread.Sleep(5 * 1000);
            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Create business organization Failed. " + ex);
                Assert.Fail();
            }
        }

        public static void createServiceLoc(string eqType, string compName, string location)
        {
            try
            {
                //select Service location tab
                selectRefDataTab("Service Location");
                Thread.Sleep(3 * 1000);

                TelerikObject.Click(PageEquipmentConfiguration.refData_div);
                Thread.Sleep(3 * 1000);
                TelerikObject.Keyboard_Send(System.Windows.Forms.Keys.Home);
                Thread.Sleep(3 * 1000);

                //click Add
                TelerikObject.Click(refData_Addbtn);
                Thread.Sleep(5 * 1000);

                //select equipment type
                refData_controls("Equipment Type", eqType);

                Thread.Sleep(2 * 1000);

                //enter company name
                TelerikObject.Click(refData_servLoc_CompanyName);
                TelerikObject.Sendkeys(refData_servLoc_CompanyName, compName);

                Thread.Sleep(2 * 1000);

                //enter location
                TelerikObject.Click(refData_servLoc_Location);
                TelerikObject.Sendkeys(refData_servLoc_Location, location);

                Thread.Sleep(2 * 1000);

                //click Save
                TelerikObject.Click(refData_Savebtn);
            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Create service location Failed. " + ex);
                Assert.Fail();
            }
        }


        #endregion


        #region Functions Historical Information


        public static void histInfo_createWell(string fieldName, string value, string datevalue)
        {
            try
            {
                for (int i = 1; i < histInfo_createWelldiv.ChildNodes.Count; i++)
                {
                    Element control = histInfo_createWelldiv.ChildNodes[i].Children[0];

                    if (control.InnerText == fieldName)
                    {
                        Element input = control.GetNextSibling().Children[0];

                        TelerikObject.Click(input);
                        TelerikObject.Sendkeys(input, value);

                        Element control_date = histInfo_createWelldiv.ChildNodes[i + 1].Children[0];
                        Element commDate = control_date.GetNextSibling().Children[0].Children[0].Children[0];

                        TelerikObject.Click(commDate);
                        TelerikObject.Sendkeys(commDate, "01032007");

                        TelerikObject.Click(histInfo_Savebtn);
                        Thread.Sleep(5 * 1000);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Create Well Failed. " + ex);
            }

        }



        /*
         in progress ----*/
        public static void histInfo_controls(string fieldName, string value, string control, string action = "")
        {
            try
            {
                HtmlControl immediate_parent = TelerikObject.GetChildrenControl(histInfo_div, "0;0;0;0");

                for (int i = 0; i < immediate_parent.BaseElement.Children.Count; i++)
                {
                    if (immediate_parent.BaseElement.Children[i].Children[0].InnerText.Contains(fieldName))
                    {
                        Element inputparent = immediate_parent.BaseElement.Children[i].Children[0].GetNextSibling();

                        if (control == "dropdown")
                        {
                            //for dropdown
                            Element spanParent = inputparent.Children[0].Children[0].Children[1];
                            TelerikObject.Click(spanParent);
                            Thread.Sleep(1 * 1000);
                            TelerikObject.Select_KendoUI_Listitem(value, true);
                            Thread.Sleep(1 * 1000);
                        }
                        else if (control == "combobox" || control == "datepicker")
                        {
                            if (action == "click")
                            {
                                Element buttonctrl = inputparent.Children[1];
                                TelerikObject.Click(buttonctrl);
                                break;
                            }
                            Thread.Sleep(3 * 1000);

                            Element inputcontrol = inputparent.Children[0].Children[0].Children[0];
                            TelerikObject.Click(inputcontrol);
                            Thread.Sleep(2 * 1000);

                            if (fieldName == "Serial No." || fieldName == "Well Name")
                            {
                                TelerikObject.TypeText(value);
                                Thread.Sleep(2 * 1000);
                                TelerikObject.Select_KendoUI_Listitem(value, true);
                                Thread.Sleep(2 * 1000);
                                break;
                            }

                            TelerikObject.Sendkeys(inputcontrol, value);
                            TelerikObject.Select_KendoUI_Listitem(value, true);
                            Thread.Sleep(2 * 1000);
                        }

                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Historical Info- Failed to find control. " + ex);

            }
        }


        public static void histInfo_serialNo(string fieldName, string value, string isCombo = "")
        {
            try
            {
                //click add - serial number
                TelerikObject.Click(histInfo_CreateNewbtn);

                Thread.Sleep(3 * 1000);

                for (int i = 1; i < histInfo_serialNodiv.ChildNodes.Count; i++)
                {
                    Element control = histInfo_serialNodiv.ChildNodes[i].Children[0];

                    if (control.InnerText.Contains(fieldName))
                    {
                        Element input = control.GetNextSibling().Children[0];

                        if (isCombo != "")
                        {
                            input = control.GetNextSibling().Children[0].Children[0].Children[0].Children[0];
                        }
                        TelerikObject.Click(input);
                        TelerikObject.Sendkeys(input, value);
                        TelerikObject.Select_KendoUI_Listitem(value, true);
                        Thread.Sleep(3 * 1000);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Historical Info - Failed to create serial number. " + ex);
            }
        }


        public static void histInfo_assert(string jobType, string fieldName, string well)
        {
            try
            {
                if (jobType == "Pull")
                {
                    HtmlControl immediate_parent = TelerikObject.GetChildrenControl(histInfo_div, "0;0;0;0");

                    for (int i = 0; i < immediate_parent.BaseElement.Children.Count; i++)
                    {
                        if (immediate_parent.BaseElement.Children[i].Children[0].InnerText.Contains(fieldName))
                        {
                            Element inputparent = immediate_parent.BaseElement.Children[i].Children[0].GetNextSibling();

                            Element inputcontrol = inputparent.Children[0].Children[0].Children[0];

                            if (inputcontrol.InnerText == well)
                            {
                                Assert.IsTrue(true, "The correct well name is displayed");
                                break;
                            }
                            else
                            {
                                Assert.Fail("The correct well name is not displayed");
                                break;
                            }

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Historical Info Job save Failed. " + ex);
            }
        }


        public static void histInfo_Job(string equipment, string subtype, string jobType, string servLoc, string createSerialno, string serialNo,
            string custName, string createWell, string wellName, string installDate = "", string pullDate = "", string IP_installDate = "", string IP_pullDate = "")
        {
            try
            {
                //select equipment
                histInfo_controls("Equipment", equipment, "dropdown");

                //select subtype
                histInfo_controls("Subtype", subtype, "dropdown");

                //select histinfo job 
                histInfo_controls("HistoricalJob Type", jobType, "dropdown");

                Thread.Sleep(2 * 1000);

                if (createSerialno == "true")
                {
                    //create serial number
                    histInfo_serialNo("Serial No.", serialNo);

                    histInfo_serialNo("Customer Name", custName, "yes");

                    Thread.Sleep(2 * 1000);

                    TelerikObject.Click(histInfo_Savebtn);
                    //TelerikObject.Click(histInfo_Savebtn);
                }
                else
                {
                    histInfo_controls("Serial No.", serialNo, "combobox");
                }
                Thread.Sleep(5 * 1000);

                if (createWell == "true")
                {
                    //click on new well button
                    histInfo_controls("Well Name", "", "combobox", "click");

                    //create well
                    histInfo_createWell("Well Name", wellName, "02032004");
                }
                else if (jobType != "Pull")
                {
                    histInfo_controls("Well Name", wellName, "combobox");
                }
                Thread.Sleep(3 * 1000);

                if (jobType == "Install")
                {
                    histInfo_controls("Install Date", installDate, "datepicker");
                }
                else if (jobType == "Pull")
                {
                    //verify auto-populated well name
                    histInfo_assert(jobType, "Well Name", wellName);
                    Thread.Sleep(2 * 1000);

                    //pull date
                    histInfo_controls("Pull Date", pullDate, "datepicker");

                    //if(histInfo_controls("Well Name", wellName, "combobox"))
                }
                else if (jobType == "Install & Pull")
                {
                    histInfo_controls("Install Date", IP_installDate, "datepicker");
                    histInfo_controls("Pull Date", IP_pullDate, "datepicker");
                }

                Thread.Sleep(3 * 1000);

                //service location
                histInfo_controls("Service Location", servLoc, "combobox");

                Thread.Sleep(2 * 1000);

                //click add
                TelerikObject.Click(eqconfig_Addbtn);
                Thread.Sleep(5 * 1000);
                TelerikObject.Keyboard_Send(System.Windows.Forms.Keys.F5);
                Thread.Sleep(10 * 1000);
            }
            catch (Exception ex)
            {
                Helper.CommonHelper.TraceLine("Historical Info Job save Failed. " + ex);
            }
        }


        #endregion Functions Historical Information
    }
}
