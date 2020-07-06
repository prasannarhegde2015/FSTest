using ArtOfTest.WebAii.Controls.HtmlControls;
using ArtOfTest.WebAii.ObjectModel;
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
    class PageJobManagement
    {
        public static string FilesLocation = ConfigurationManager.AppSettings.Get("FilesLocation");
        #region ObjectProperties_General
        public static string DynamicValue = null;
        public static HtmlDiv jobListPane { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "attribute", "class=noJob ng-star-inserted", "Job Listing Pane"); } }


        public static HtmlButton btnAddJob { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "attribute", "id=btnAdd", "Add Job button"); } }

        //public static Element btnDeleteJob { get { return TelerikObject.GetElement("Xpath", "//button[contains(text(),'Delete')]", "Delete Job button"); } }

        public static HtmlControl btnDeleteJob { get { return TelerikObject.GetPreviousSibling((HtmlControl)btnUpdateJob, 1); } }
        public static HtmlButton btnUpdateJob { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "content", "Update", "Update Job button"); } }

        public static HtmlButton btnJobStatusHistory { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "content", "Job Status History", "Job Status History button"); } }

        public static HtmlAnchor addNewJob { get { return (HtmlAnchor)TelerikObject.GetElement("HtmlAnchor", "attribute", "translate=add", "Add New Job button on Job management Screen"); } }

        public static ReadOnlyCollection<Element> tables_GeneralTab { get { return TelerikObject.GetElementCollection("tagName", "table", "Collection of Tables of General tab"); } }

        public static HtmlControl btnAttachDoc { get { return (HtmlControl)TelerikObject.GetElement("HtmlButton", "Xpath", "//tr[1]/td/button/span", "Add Attach documents button"); } }

        public static HtmlButton btnAddTemplatJob { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "attribute", "title=Add job from template", "Add Job from template button"); } }
        //
        public static HtmlDiv newlyCreatedJob { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "attribute", "ng-reflect-ng-class=activeJob", "Newly Created Job"); } }

        public static Element btnDeleteOnPopup { get { return TelerikObject.GetElement("Xpath", "//button[contains(text(),'Delete this Job')]", "Delete Button on confirmation pop up"); } }
        //    public static HtmlButton btnDeleteOnPopup { get { return (HtmlButton) TelerikObject.GetElement("HtmlButton", "Content","Delete this Job", "Delete Button on confirmation pop up"); } }


        //  public static Element dd_jobType { get { return TelerikObject.GetElement("Xpath", "//kendo-dropdownlist[@name='jobType']", "Job Type List"); } }

        public static HtmlControl dd_jobType { get { return TelerikObject.GetElement("kendo-dropdownlist", "name", "jobType", "Job Type List"); } }
        public static HtmlControl dd_jobReason { get { return TelerikObject.GetElement("kendo-dropdownlist", "name", "jobReason", "Job Reason"); } }

        public static HtmlListItem listItem_afe { get { return (HtmlListItem)TelerikObject.GetElement("HtmlListItem", "Content", DynamicValue, "AFE List Item: " + DynamicValue); } }

        public static HtmlControl dd_jobDriver { get { return TelerikObject.GetElement("kendo-dropdownlist", "name", "jobDriver", "Job Driver"); } }
        public static HtmlControl dd_jobStatus { get { return TelerikObject.GetElement("kendo-dropdownlist", "name", "status", " Job Status "); } }
        public static HtmlControl dd_jobServiceProvider { get { return TelerikObject.GetElement("kendo-dropdownlist", "name", "businessOrg", "Servie Provider"); } }
        //    public static HtmlControl dd_afe { get { return TelerikObject.GetElement("kendo-dropdownlist", "name", "afe", "AFE"); } }
        public static Element dd_afe { get { return TelerikObject.GetElement("Xpath", "//label[@for='afe']/following-sibling::*/descendant::input", "AFE Provider"); } }
        public static Element label_afe { get { return TelerikObject.GetElement("Xpath", "//label[contains(text(),'AFE')]", "AFE Label"); } }


        //
        public static Element dd_jobType_text { get { return TelerikObject.GetElement("Xpath", "//kendo-dropdownlist[@name='jobType']/span/span[@class='k-input']", "JobType Text"); } }
        public static Element dd_jobReason_text { get { return TelerikObject.GetElement("Xpath", "//kendo-dropdownlist[@name='jobReason']/span/span[@class='k-input']", "JobReason Text"); } }
        public static Element dd_jobDriver_text { get { return TelerikObject.GetElement("Xpath", "//kendo-dropdownlist[@name='jobDriver']/span/span[@class='k-input']", "JobDriver Text"); } }
        public static Element dd_jobStatus_text { get { return TelerikObject.GetElement("Xpath", "//kendo-dropdownlist[@name='status']/span/span[@class='k-input']", "JobStatus Text"); } }
        public static Element dd_jobProviderr_text { get { return TelerikObject.GetElement("Xpath", "//kendo-dropdownlist[@name='businessOrg']/span/span[@class='k-input']", "Job Service Provider Text"); } }
        public static Element dd_afe_text { get { return TelerikObject.GetElement("Xpath", "//kendo-dropdownlist[@name='afe']/span/span[@class='k-input']", "AFE Provider Text"); } }
        public static HtmlAnchor tabJobPlan { get { return (HtmlAnchor)TelerikObject.GetElement("HtmlAnchor", "Content", "Job Plan", "Job Plan tab on Job management screen"); } }

        //actualCost
        public static Element txt_actualCost { get { return TelerikObject.GetElement("id", "actualCost", "actualCost"); } }
        //    public static HtmlControl txt_beginDate { get { return (HtmlControl)TelerikObject.GetElement("kendo-dateinput", "attribute", "ng-reflect-id=beginDate", "Begin Date"); } }
        //   public static HtmlInputText txt_beginDate { get { return (HtmlInputText)TelerikObject.GetElement("HtmlInputText", "id", "beginDate", "Begin Date"); } }
        public static Element txt_beginDate { get { return TelerikObject.GetElement("id", "beginDate", "Begin Date"); } }
        public static Element txt_endDate { get { return TelerikObject.GetElement("id", "endDate", "end Date"); } }
        public static Element txt_acctRef { get { return TelerikObject.GetElement("id", "accountRef", "accountRef"); } }
        public static Element txt_remarks { get { return TelerikObject.GetElement("id", "remarks", "Remarks"); } }

        public static ReadOnlyCollection<Element> tablist_jobManagement { get { return TelerikObject.GetElementCollection("Xpath", "//li[@id='tabList']", "Job Tab List"); } }

        public static Element tabList { get { return TelerikObject.GetElement("Xpath", "//li[@id='tabList']/parent::ul", "Job Tab List"); } }
        //updateJob
        public static Element btn_Save_job { get { return TelerikObject.GetElement("Xpath", "//kendo-dialog[@id='jobAddition']//button", "Save Button"); } }
        public static Element btn_Cancel_job { get { return TelerikObject.GetElement("id", "CancelJob", "Cancel Job Button"); } }
        public static ReadOnlyCollection<Element> attachedFileCollection { get { return TelerikObject.GetElementCollection("tagname", "b", "Attached File Collection"); } }
        public static HtmlButton btnYesOnDeleteAttach { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "Content", "Yes", "Yes Button on Delete Attachment pop up"); } }
        public static HtmlDiv toasterMsgDeleteAttachDocSucc { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "Content", DynamicValue + " deleted Successfully", "Delete Document Toaster Message Success"); } }

        //actualJobDuration
        public static Element txt_jobDurationDays { get { return TelerikObject.GetElement("id", "actualJobDuration", "Job Duration"); } }

        public static Element txt_jobId { get { return TelerikObject.GetElement("id", "jobId", "Job ID"); } }
        public static Element txt_jobPlanCost { get { return TelerikObject.GetElement("id", "jobPlanCost", "job Plan Cost"); } }
        public static Element txt_jobTotalCost { get { return TelerikObject.GetElement("id", "totalCost", "total Cost"); } }
        //]
        public static Element tab_Evernt { get { return TelerikObject.GetElement("Xpath", "//a[text()='Events']", "Events Tab"); } }
        public static Element btn_Download_attachment { get { return TelerikObject.GetElement("Xpath", "//button[@class='btn btn-primary']", "Download Attachment Button"); } }
        public static Element btn_Delete_attachment { get { return TelerikObject.GetElement("Xpath", "//div[@class='headerButtonRailDM ng-star-inserted']/button[2]", "Delete Attachment Button"); } }

        // public static ReadOnlyCollection<Element> documentManager_Categories { get { return TelerikObject.GetElementCollection("attribute", "class=folderNames", "Document Manager Categories"); } }

        public static List<HtmlControl> documentManager_Categories { get { return TelerikObject.CollectionOfControls("htmldiv", "classname", "folderNames", "Document Manager Categories"); } }
        public static Element toaster_message_addSuccess { get { return TelerikObject.GetElement("Xpath", "//div[contains(text(),'Added Successfully')]", "Toaster Message for new Job Addition "); } }
        public static Element toaster_message_updateSuccess { get { return TelerikObject.GetElement("Xpath", "//div[contains(text(),'Updated Successfully')]", "Toaster Message for Job Update"); } }
        public static Element toaster_message_deleteSuccess { get { return TelerikObject.GetElement("Xpath", "//div[contains(text(),'Deleted Successfully')]", "Toaster Message for Job Deletion"); } }

        public static List<HtmlControl> btns_AddFile { get { return TelerikObject.CollectionOfControls("htmlspan", "text", "Add File", "Add File Button Collection"); } }


        public static HtmlDiv toasterMsgAttachDocSucc { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "Content", "Uploaded document successfully", "Attach Document Toaster Message Success"); } }
        public static HtmlSpan closeAttachDocModal { get { return (HtmlSpan)TelerikObject.GetElement("HtmlSpan", "attribute", "class=k-icon k-i-x", "Close Attach document Modal"); } }

        public static Element attachmentCount { get { return TelerikObject.GetElement("Xpath", "//span[@class='k-icon k-i-attachment k-i-clip']", "Attachment Count After Attachment Addition"); } }
        public static Element attachmentCountAfterDeletion { get { return TelerikObject.GetElement("Xpath", "//span[@class='k-icon k-i-document-manager']", "Attachment Count After Attachment Deletion"); } }

        public static HtmlAnchor addTab { get { return (HtmlAnchor)TelerikObject.GetElement("HtmlAnchor", "attribute", "class=dropdown-toggle", "Add tab button on Job management Screen"); } }

        public static HtmlAnchor jobPlanItem { get { return (HtmlAnchor)TelerikObject.GetElement("HtmlAnchor", "Content", "Job Plan", "Job Plan Item on Job management screen"); } }

        //   public static HtmlInputControl txt_afe { get { return TelerikObject.GetElementCollection("HtmlInputControl", "", "Document Manager Categories"); } }
        #endregion


        #region ObjectEvents
        public static HtmlAnchor tabGeneral { get { return (HtmlAnchor)TelerikObject.GetElement("HtmlAnchor", "content", "General", "General Tab"); } }
        public static HtmlAnchor tabEvents { get { return (HtmlAnchor)TelerikObject.GetElement("HtmlAnchor", "content", "Events", "Job Events Tab"); } }

        public static HtmlAnchor btnplusforjob { get { return (HtmlAnchor)TelerikObject.GetElement("HtmlAnchor", "attribute", "aria-haspopup=true", "Job Plus Button"); } }

        public static HtmlAnchor tabEconomicAnalysis { get { return (HtmlAnchor)TelerikObject.GetElement("HtmlAnchor", "content", "Economic Analysis", "Economic Analysis"); } }
        public static HtmlAnchor tabJobCostDetails { get { return (HtmlAnchor)TelerikObject.GetElement("HtmlAnchor", "content", "Job Cost Details", "Job Cost Details"); } }
        public static HtmlAnchor tabFailureReport { get { return (HtmlAnchor)TelerikObject.GetElement("HtmlAnchor", "content", "Failure Report", "Failure Report"); } }
        public static HtmlAnchor tabJobplan { get { return (HtmlAnchor)TelerikObject.GetElement("HtmlAnchor", "content", "Job Plan", "Job Plan"); } }
        public static HtmlAnchor tabWellBore { get { return (HtmlAnchor)TelerikObject.GetElement("HtmlAnchor", "content", "Wellbore", "Wellbore"); } }


        public static HtmlButton btnAddEvent { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "content", "Add", "Add Event Button"); } }
        public static HtmlButton btnMultiAddEvent { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "content", "Multi Add", "Multi Add Event Button"); } }
        public static HtmlButton btnPasteEvent { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "content", "Paste Event", "Paste Job Events Button"); } }
        public static HtmlButton btnCancelEvent { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "content", "Cancel", "Cancel Events Button"); } }
        public static HtmlButton btnSaveEventMain { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "expression", "class=btn btn-primary spacingButtonJobEvent;InnerText=Save", "Save Events Button on Top Page"); } }
        public static HtmlControl txt_EventType { get { return new HtmlControl(TelerikObject.GetElement("Xpath", "//label[contains(text(),'Event Type')]/ancestor::div[1]/following-sibling::*[1]/descendant::input", "Event Type Text Box")); } }
        //    public static HtmlControl txt_EventType {  get { return (HtmlControl)TelerikObject.GetElement("htmlcontrolnextsibling", "content", "Event Type", "Event Type Text Field"); } } 
        //   public static HtmlListItem listItem_eventType { get { return (HtmlListItem)TelerikObject.GetElement("HtmlListItem", "content", DynamicValue, "List item for Event Type"); } }
        ////li[text()='Acidize']
        //  public static HtmlListItem listItem_eventType {  get { return new HtmlListItem(TelerikObject.GetElement("Xpath", "//li[text()='"+DynamicValue+"']", "List item for Event Type")); } }
        //    public static Element listItem_eventType { get { return TelerikObject.GetElement("Xpath", "//li[text()='Acidize']", "List item for Event Type"); } }
        //   public static HtmlControl txt_duartionDays { get { return TelerikObject.GetElement("HtmlControl", "id", "evcDurationHours", "Event duration days"); } }
        public static Element txt_duartionDays { get { return TelerikObject.GetElement("id", "evcDurationHours", "Event duration days"); } }

        public static KendoInput txt_duationDaysKendoInput { get { return TelerikObject.mgr.ActiveBrowser.Find.AllControls<KendoInput>().FirstOrDefault(x => x.ID == "evcDurationHours"); } }
        public static HtmlControl txt_eventBeginDate { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "attribute", "for=evcEventBegDtTm", "Event Begin Date/Time"); } }

        public static HtmlControl txt_eventEndDate { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "attribute", "for=evcEventEndDtTm", "Event End Date/Time"); } }
        public static HtmlControl txt_totalCost { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "attribute", "for=evcTotalCost", "Event Total Cost"); } }

        public static HtmlControl dd_autocomplete { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "attribute", "for=" + DynamicValue, "Drop down with for attribute: " + DynamicValue); } }
        public static HtmlButton btn_saveEvent { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "expression", "class=btn btn-default smallerButton;InnerText=Save", "Save Events Button on Add New Event modal dialog"); } }

        public static HtmlSpan toaster_msgAddEventSuccess { get { return (HtmlSpan)TelerikObject.GetElement("HtmlSpan", "content", "Events Saved Successfully", "Save Events Success Toaster Message"); } }

        public static HtmlSpan toaster_msgModifyEventSuccess { get { return (HtmlSpan)TelerikObject.GetElement("HtmlSpan", "content", "Updated Successfully", "Modify Events Success Toaster Message"); } }

        public static Element accordian_eventDate { get { return TelerikObject.GetElement("Xpath", "//span[contains(.,'" + DynamicValue + "')][contains(@class,'k-header')]", "Accordian with given date" + DynamicValue); } }


        //      public static HtmlDiv link_Additional { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "content" ,"Additional" ,"Addtional Details Link"); } }
        public static Element link_Additional { get { return TelerikObject.GetElement("Xpath", "//div[contains(text(),'Additional')][@class='metaGroupItem ng-star-inserted']", "Addtional Details Link"); } }
        public static Element link_Extedned { get { return TelerikObject.GetElement("Xpath", "//div[contains(text(),'Extended Event')][@class='metaGroupItem ng-star-inserted']", "Extended Event Link"); } }

        public static HtmlControl txt_fieldServiceOrderID { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "attribute", "for=evcFieldServiceOrderID", "Field Service Order Id"); } }

        public static HtmlControl txt_perosnPerformingTask { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "attribute", "for=evcPersonPerformingTask", "Person Performing Task"); } }
        public static HtmlControl radiogp_preventiveRadioGroup { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "attribute", "for=evcPreventiveMaintenance", "Preventive Maintenance"); } }
        public static HtmlControl txt_evntquantity { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "attribute", "for=evcQuantity", "Quantity"); } }
        public static HtmlControl txt_evntremarks { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "attribute", "for=evcRemarks", "Remakrks"); } }
        public static HtmlControl txt_responsilbePerson { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "attribute", "for=evcResponsiblePerson", "Responsible Perso"); } }
        public static HtmlControl radio_trouble { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "attribute", "for=evcTrouble", "Trouble ?"); } }
        public static HtmlControl radio_unplannedEvent { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "attribute", "for=evcUnPlanned", "UnPlanned Event"); } }
        //evcWorkorderID
        public static HtmlControl txt_evcWorkorderID { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "attribute", "for=evcWorkorderID", "WorkorderID"); } }
        //evcHistoricalRate
        public static HtmlControl txt_evcHistoricalRate { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "attribute", "for=evcHistoricalRate", "Historical /Event Cost  Rate"); } }
        //eacVendorProduct
        public static HtmlControl txt_VendorProduct { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "attribute", "for=eacVendorProduct", "Vendor Product"); } }

        public static HtmlSpan toastermsg_EventSaveSuccess { get { return (HtmlSpan)TelerikObject.GetElement("HtmlSpan", "content", "Event Data Saved Successfully", "Toaster Message for Mass event Sucesss"); } }

        public static HtmlSpan btn_gridCellModify { get { return (HtmlSpan)TelerikObject.GetElement("HtmlSpan", "attribute", "title=Modify", "Grid  Cell Modify Button"); } }

        public static HtmlSpan btn_gridCellDelete { get { return (HtmlSpan)TelerikObject.GetElement("HtmlSpan", "attribute", "title=Delete", "Grid  Cell Delete Button"); } }

        public static HtmlSpan btn_gridCellClone { get { return (HtmlSpan)TelerikObject.GetElement("HtmlSpan", "attribute", "title=Clone", "Grid  Cell Clone Button"); } }

        public static HtmlControl txt_BeginDateMultiAdd { get { return TelerikObject.GetElement("HtmlControlNextSibling", "content", "Begin Date", "Multi Add begin Date"); } }
        //    public static HtmlControl dd_ServiceProviderMultiAdd { get { return TelerikObject.GetElement("HtmlControlNextSibling", "content", "Service Provider", "Multi Add Service Provider"); } }
        public static HtmlSelect dd_ServiceProviderMultiAdd { get { return (HtmlSelect)TelerikObject.GetElement("HtmlSelect", "name", "vendorname", "Multi Add Service Provider"); } }
        //Truck Unit
        public static HtmlSelect dd_truckUnitMultiAdd { get { return (HtmlSelect)TelerikObject.GetElement("HtmlSelect", "name", "TruckUnitC", "Truck Unit C"); } }
        public static HtmlSelect dd_costCategory_MultiAdd { get { return (HtmlSelect)TelerikObject.GetElement("HtmlSelect", "name", "catalog", "Cost Category"); } }

        public static HtmlControl txt_rate_MultiAdd { get { return TelerikObject.GetElement("HtmlControlNextSibling", "content", "Rate", "Event Rate"); } }

        // Acidize ,Bundle Test , Chemical analysis , Wireline - Perforate
        public static HtmlControl chbox_bundletest { get { return TelerikObject.GetElement("HtmlControl", "content", "Bundle Test", "Bundle test"); } }
        public static HtmlControl chbox_chemAnalysis { get { return TelerikObject.GetElement("HtmlControl", "content", "Chemical Analysis", "Chemical Analysis"); } }
        public static HtmlControl chbox_wirelienPerforate { get { return TelerikObject.GetElement("HtmlControl", "content", "Wireline - Perforate", "Wireline - Perforate"); } }

        public static HtmlButton btn_addEventMultiAdd { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "content", "Add Events", "Add Events Button "); } }
        public static HtmlButton btn_removeEventMultiAdd { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "content", "Remove Events", "Remove Events Button"); } }
        public static HtmlButton btn_saveEventMultiAdd { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "expression", "class=btn btn-default;InnerText=Save", "Save Event Button Multi Add"); } }
        // Select All
        //        public static HtmlControl chbox_selectedEventsSelectall { get { return TelerikObject.GetElement("HtmlControl", "content", "Select All", "Select All"); } }
        ////div/*[contains(text(),"Selected Events")]/following-sibling::*/descendant::label[contains(.,'Select All')]
        public static HtmlControl chbox_selectedEventsSelectall { get { return new HtmlControl(TelerikObject.GetElement("xpath", "//div/*[contains(text(),'Selected Events')]/following-sibling::*/descendant::label[contains(.,'Select All')]", "Selelct All for Selected Events")); } }

        public static Element row_selector_Modifyfor_EventType { get { return TelerikObject.GetElement("xpath", "//span[contains(text(),'" + DynamicValue + "')]/preceding::span[@title='Modify']", "Modify button for EventType " + DynamicValue + " Row"); } }
        public static Element row_selector_Deletefor_EventType { get { return TelerikObject.GetElement("xpath", "//span[contains(text(),'" + DynamicValue + "')]/preceding::span[@title='Delete']", "Delete button for EventType " + DynamicValue + " Row"); } }
        public static Element row_selector_Clonefor_EventType { get { return TelerikObject.GetElement("xpath", "//span[contains(text(),'" + DynamicValue + "')]/parent::*/preceding-sibling::*/button/span[@title='Clone']", "Clone button for EventType " + DynamicValue + " Row"); } }

        // btn btn-danger
        public static HtmlButton btn_confirmOKOnQECancel { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "expression", "class=btn btn-danger;InnerText=OK", "Confirm Button ''OK' on Cancel Popup for Cancel Mass Save Event"); } }
        //Delete this Event
        //public static HtmlButton btn_confirmDeleteEvent { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "expression", "class=btn btn-danger;InnerText=Yes, Delete this Event", "Confirm Button on Delete Event"); } }
        //public static HtmlButton btn_confirmDeleteEvent { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "content", "Delete this Event", "Confirm Button on Delete Event"); } }
        public static Element btn_confirmDeleteEvent { get { return TelerikObject.GetElement("Xpath", "//button[contains(text(),'Delete this Event')]", "Confirm Button on Delete Event"); } }


        public static HtmlSpan toaster_msgDeleteEventSuccess { get { return (HtmlSpan)TelerikObject.GetElement("HtmlSpan", "content", "Delete Successfully", "Delete Events Success Toaster Message"); } }
        public static HtmlSpan toaster_msgPasteEventSuccess { get { return (HtmlSpan)TelerikObject.GetElement("HtmlSpan", "content", "Pasted Successfully", "Paste Events Success Toaster Message"); } }

        public static Element text_msgCancelGridEditText { get { return TelerikObject.GetElement("xpath", "//p[text()='" + DynamicValue + "']", "Cancel QE dialog text"); } }

        public static HtmlAnchor link_CloseEditDialog { get { return (HtmlAnchor)TelerikObject.GetElement("HtmlAnchor", "attribute", "aria-label=Close", "Close Link"); } }
        public static string jobTotalCost { get; set; }


        #endregion

        #region ObjectJobPlan
        public static HtmlDiv costnHour { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "attribute", "class=headerDetailsJob ng-star-inserted", "Planned Cost & Planned hours details"); } }
        public static HtmlButton btn_saveAsTemplateJob { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "content", "Save as Template Job", "Save as Templete Job button on Job Plan screen"); } }

        public static HtmlButton btn_addOnJobPlan { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "content", "Add", "Add button on Job Plan screen"); } }

        public static HtmlSelect evtType { get { return (HtmlSelect)TelerikObject.GetElement("HtmlSelect", "attribute", "name=EventType", "Event Type Dropdown"); } }

        public static HtmlSelect vendorName { get { return (HtmlSelect)TelerikObject.GetElement("HtmlSelect", "attribute", "name=vendorname", "Vendorname Dropdown"); } }

        public static HtmlSelect vendorName_Edit { get { return (HtmlSelect)TelerikObject.GetElement("HtmlSelect", "attribute", "name=BusinessOrganizationName", "Vendorname Dropdown on Edit popup"); } }

        public static HtmlSelect catalog { get { return (HtmlSelect)TelerikObject.GetElement("HtmlSelect", "attribute", "name=catalog", "Catalog Dropdown"); } }

        public static HtmlSelect truckUnit { get { return (HtmlSelect)TelerikObject.GetElement("HtmlSelect", "attribute", "name=TruckUnitC", "Truck Unit Dropdown"); } }

        public static HtmlSelect truckUnit_Edit { get { return (HtmlSelect)TelerikObject.GetElement("HtmlSelect", "attribute", "name=Truck UnitE", "Truck Unit Dropdown on Edit pop up"); } }
        // public static HtmlControl txt_estimatedHours { get { return TelerikObject.GetElement("HtmlControl", "attribute", "placeholder=Enter Hours", "Estimated Hours Textbox"); } }

        public static HtmlControl txt_estimatedHours { get { return (HtmlControl)TelerikObject.GetElement("htmlcontrolparentnextsibling", "expression", "innertext=Estimated Hours;tagname=label", "Estimated Hours Textbox"); } }

        public static HtmlControl txt_estimatedHours_Edit { get { return TelerikObject.GetElement("HtmlControl", "xpath", "//kendo-numerictextbox[@name='estimatedHoursE']/span/input", "Estimated Hours Textbox on Edit pop up"); } }

        public static HtmlInputText txt_accountingId { get { return (HtmlInputText)TelerikObject.GetElement("HtmlInputText", "attribute", "name=accountingId", "Accounting Id Textbox"); } }

        public static HtmlInputText txt_accountingId_Edit { get { return (HtmlInputText)TelerikObject.GetElement("HtmlInputText", "attribute", "name=accountingIdE", "Accounting Id Textboxon Edit pop up"); } }
        public static HtmlControl txt_unitPrice { get { return (HtmlControl)TelerikObject.GetElement("htmlcontrolparentnextsibling", "expression", "innertext=Unit Price;tagname=label", "Unit Price Textbox"); } }

        public static HtmlControl txt_unitPrice_Edit { get { return (HtmlControl)TelerikObject.GetElement("HtmlControl", "xpath", "//kendo-numerictextbox[@name='unitPriceEdit']/span/input", "Unit Price Textbox on Edit pop up"); } }
        //  public static HtmlControl txt_estimatedTotalCost { get { return (HtmlControl)TelerikObject.GetElement("HtmlControl", "attribute", "placeholder=Estimated Total Cost", "Estimated Total Cost Textbox"); } }

        public static HtmlControl txt_estimatedTotalCost { get { return (HtmlControl)TelerikObject.GetElement("htmlcontrolparentnextsibling", "expression", "innertext=Estimated Total Cost;tagname=label", "Estimated Total Cost Textbox"); } }

        public static HtmlControl txt_estimatedTotalCost_Edit { get { return (HtmlControl)TelerikObject.GetElement("HtmlControl", "xpath", "//kendo-numerictextbox[@name='totalCostEdit']/span/input", "Estimated Total Cost Textbox on edit pop up"); } }

        public static HtmlInputText txt_respPerson { get { return (HtmlInputText)TelerikObject.GetElement("HtmlInputText", "attribute", "name=ResponsiblePerson", "Responsible person Textbox"); } }

        public static HtmlInputText txt_assignedToPerson { get { return (HtmlInputText)TelerikObject.GetElement("HtmlInputText", "attribute", "name=AssignedTo", "Assigned to person Textbox"); } }

        public static HtmlControl txt_briefDescription { get { return TelerikObject.GetElement("HtmlControl", "attribute", "name=Description", "Brief Description Textbox"); } }

        public static HtmlControl txt_remarks1 { get { return TelerikObject.GetElement("HtmlControl", "attribute", "name=Remarks", "Remarks Textbox"); } }

        public static HtmlButton btn_saveOnPopUp { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "expression", "class=btn btn-primary;InnerText=Save", "Save button"); } }

        public static HtmlDiv titleOnPopup { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "attribute", "class=k-window-title k-dialog-title", "Title on pop up"); } }

        public static HtmlControl txt_PlannedEventId { get { return TelerikObject.GetElement("HtmlControl", "name", "Id", "Planned Event Id Readonly textbox"); } }

        public static HtmlSpan toasterMsg_PlannedEventAddSuccess { get { return (HtmlSpan)TelerikObject.GetElement("HtmlSpan", "attribute", "class=toast-message ng-star-inserted", "Toaster message for planned event addition/updation status"); } }
        //public static HtmlTable table_JobPlan = TelerikObject.getTableByColumnName("Action");
        // public static HtmlTable table_JobPlan { get { return (HtmlTable)TelerikObject.GetElement("HtmlTable", "attribute", "class=k-grid-table", "Job Plan table"); } }
        public static HtmlTable table_JobPlan { get { return (HtmlTable)TelerikObject.GetElement("HtmlTable", "innertext", "Bundle Test", "Job Plan table"); } }

        public static HtmlDiv titleOnDeleteEventPopUp { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "attribute", "class=k-window-title k-dialog-title", "Title on Planned event Delete pop up"); } }
        public static HtmlButton btn_DeleteOnPopUp { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "xpath", "//button[contains(text(),'Yes, Delete this')]", "Delete button on confirmation pop up"); } }

        public static Element verbiageOnDeleteOpUp { get { return TelerikObject.GetElement("xpath", "//div[@class='k-content k-window-content k-dialog-content']/div/p", "Delete Event Message"); } }
        #endregion

        #region JobCostDetails
        public static HtmlDiv jobCostDetailsHeader { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "attribute", "class=fontheader", "Total Job Cost & Total Job Detail Cost"); } }

        public static HtmlButton btn_addOnJobCostDetails { get { return (HtmlButton)TelerikObject.GetElement("HtmlButton", "content", "Add", "Add button on Job Cost Details screen"); } }

        public static HtmlDiv noDataAvailable { get { return (HtmlDiv)TelerikObject.GetElement("HtmlDiv", "attribute", "class=noDataAvailable", "No Data available message when Job Cost Details is not added"); } }

        public static HtmlControl txt_date { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "content", "Date", "Date Field on Job Cost Details screen"); } }

        public static HtmlControl quantityOnJobCostDetails { get { return TelerikObject.GetElement("htmlcontrolparentnextsibling", "expression", "innertext=Quantity;tagname=label", "Quantity Field on Job Cost Details screen"); } }


        public static HtmlSelect vendorNameOnJobCostDetails { get { return (HtmlSelect)TelerikObject.GetElement("HtmlSelect", "attribute", "name=sel", "Vendorname Dropdown in Job Cost Details section"); } }



        //  public static HtmlControl quantityOnJobCostDetails { get { return (HtmlControl)TelerikObject.GetElement("HtmlControl", "attribute", "placeholder=Enter Quantity", "Quantity Field on Job Cost Details screen"); } }

        public static HtmlControl txt_DiscAmt { get { return (HtmlControl)TelerikObject.GetElement("htmlcontrolparentnextsibling", "expression", "innertext=Discount Amount;tagname=label", "Discount amount Textbox"); } }

        public static HtmlControl txt_Cost { get { return (HtmlControl)TelerikObject.GetElement("HtmlControl", "attribute", "placeholder=Enter Cost", "Cost Textbox"); } }
        #endregion




        #region JobManagementMethods
        public static void JobAssertions_BeforeJobCreation()
        {
            Helper.CommonHelper.TraceLine("Ensuring that Default Message No Records Avalaiable is shown");
            HtmlTable jobId = TelerikObject.getTableByColumnName("Job ID");
            Assert.IsNotNull(jobId, "Job Id Column is not present");
            HtmlTable jobTableHeader = TelerikObject.getTableByColumnName("Job Type");
            string colNames = "Job TypenullJob ReasonnullJob DrivernullStatusnullAFEnullService ProvidernullActual CostnullBegin DatenullEnd DatenullJob Duration HoursnullAccount ReferencenullJob Plan CostnullTotal CostnullRemarksnull";
            Assert.AreEqual(colNames, jobTableHeader.InnerText, "Column Names of Job table are incorrect");
            HtmlTable tableContent = TelerikObject.getTableByColumnName("No records available");
            Assert.IsNotNull(tableContent, "Restriction data table has incorrect data");

            Helper.CommonHelper.TraceLine("Ensuring that only General Tab is dispalyed ");
            Assert.IsTrue(tabList.Children.Where(o => o.IdAttributeValue == "tabList").Count() == 1, "More than one tab is available Other than general Tab during new job creation");
            Assert.AreEqual("General", tabList.Children.Where(o => o.IdAttributeValue == "tabList").First().InnerText, "Text of Visible tab is Not General");

            Helper.CommonHelper.TraceLine("Ensuring that Job Status History button is disabled ");
            Assert.IsFalse(btnJobStatusHistory.IsEnabled, "Job Status History button is not Disabled");

            Helper.CommonHelper.TraceLine("Ensuring that Add button is enabled ");
            Assert.IsTrue(btnAddJob.IsEnabled, "Add Job is not Enabled");

            Helper.CommonHelper.TraceLine("Ensuring that Update button is disabled ");
            Assert.IsFalse(btnUpdateJob.IsEnabled, "Update button is not Disabled");

            Helper.CommonHelper.TraceLine("Ensuring that Delete button is disabled ");
            Assert.IsFalse(btnDeleteJob.IsEnabled, "Delete button is not Disabled");

        }


        public static void JobAssertions_AfterJobCreation()
        {

            Helper.CommonHelper.TraceLine("Generated Job Id is " + tables_GeneralTab.ElementAt(2).Children[1].Children[0].Children[1].InnerText);

            Helper.CommonHelper.TraceLine("Ensuring that only General & Events Tabs are dispalyed ");
            Assert.IsTrue(tabList.Children.Where(o => o.IdAttributeValue == "tabList").Count() == 2, "Tabs other than General & Events are also displayed");
            Assert.AreEqual("General", tabList.Children.Where(o => o.IdAttributeValue == "tabList").First().InnerText, "Text of Visible tab is Not General");
            Assert.AreEqual("Events", tabList.Children.Where(o => o.IdAttributeValue == "tabList").ElementAt(1).InnerText, "Text of Visible tab is Not Events");

            Helper.CommonHelper.TraceLine("Ensuring that Job Status History button is enabled ");
            Assert.IsTrue(btnJobStatusHistory.IsEnabled, "Job Status History button is not Enabled");

            Helper.CommonHelper.TraceLine("Ensuring that Add button is enabled ");
            Assert.IsTrue(btnAddJob.IsEnabled, "Add Job is not Enabled");

            Helper.CommonHelper.TraceLine("Ensuring that Update button is enabled ");
            Assert.IsTrue(btnUpdateJob.IsEnabled, "Update button is not Enabled");

            Helper.CommonHelper.TraceLine("Ensuring that Delete button is enabled ");
            Assert.IsTrue(btnDeleteJob.IsEnabled, "Delete button is not Enabled");
        }


        public static void UpdateJob()
        {
            //Select first Job Id 
            TelerikObject.Click(tables_GeneralTab.ElementAt(2).Children[1].Children[0].Children[1]);

            //Click on Update button
            TelerikObject.Click(btnUpdateJob);

            //Wait for loader to disappear
            TelerikObject.WaitForControlDisappear(new HtmlControl(PageDashboard.page_Loader));

            //Verify Update Pop up title
            Assert.AreEqual("Update Job", titleOnPopup.InnerText, "Title on Update Job pop up is incorrect");

            TelerikObject.Click(dd_jobType);
            string testdatafile = Path.Combine(FilesLocation + @"\UIAutomation\TestData", "JobDetails_Edit.xml");
            Helper.CommonHelper.TraceLine("xml file path is " + testdatafile);
            Helper.CommonHelper.TraceLine("Getting Data from XML file ...");
            DataTable dt = Helper.CommonHelper.BuildDataTableFromXml(testdatafile);
            string strjobtype = dt.Rows[0]["jobtype"].ToString();
            TelerikObject.Select_KendoUI_Listitem(strjobtype, true);
            TelerikObject.Click(dd_jobReason);
            TelerikObject.Select_KendoUI_Listitem(dt.Rows[0]["jobreason"].ToString(), true);
            TelerikObject.Click(dd_jobDriver);
            TelerikObject.Select_KendoUI_Listitem(dt.Rows[0]["jobdriver"].ToString(), true);
            TelerikObject.Click(dd_jobStatus);
            TelerikObject.Select_KendoUI_Listitem(dt.Rows[0]["status"].ToString(), true);
            TelerikObject.Click(dd_jobServiceProvider);
            TelerikObject.Select_KendoUI_Listitem(dt.Rows[0]["serviceprovider"].ToString(), true);
            TelerikObject.Sendkeys(txt_actualCost, dt.Rows[0]["actualcost"].ToString());
            TelerikObject.Sendkeys(txt_beginDate, dt.Rows[0]["beindate"].ToString().Replace("/", ""), false);
            TelerikObject.Sendkeys(txt_endDate, dt.Rows[0]["enddate"].ToString().Replace("/", ""), false);
            TelerikObject.Sendkeys(txt_acctRef, dt.Rows[0]["accoutnref"].ToString());
            TelerikObject.Sendkeys(txt_remarks, dt.Rows[0]["remarks"].ToString());
            TelerikObject.Click(btn_Save_job);
            Helper.CommonHelper.TraceLine("Waitng for toaster message...");
            Assert.IsNotNull(toaster_message_updateSuccess, "Toaster message did not appear");
            Helper.CommonHelper.TraceLine(" toaster message has appeared on UI...");
            Thread.Sleep(5000);
            //Scrolling up to see first Job
            btnAddJob.ScrollToVisible();
            JobAssertions_AfterJobCreation();
            Helper.CommonHelper.TraceLine("Navigating away from Job management Page");
            TelerikObject.Click(PageDashboard.jobStatusView);
            Thread.Sleep(10000);
            Helper.CommonHelper.TraceLine("Navigating Back to  Job management Page");
            TelerikObject.Click(PageDashboard.jobManagementTab);
            Helper.CommonHelper.TraceLine("Verfiying Saved Values ");
            TelerikObject.AutoDomrefresh();
            Thread.Sleep(10000);
            JobFieldsSaveAssertion(dt, 0);
        }

        public static void DeleteSelectedJob()
        {
            //Select first Job Id 
            TelerikObject.Click(tables_GeneralTab.ElementAt(2).Children[1].Children[0].Children[1]);
            Thread.Sleep(2000);
            TelerikObject.Click(btnDeleteJob);
            Thread.Sleep(2000);
            TelerikObject.AutoDomrefresh();
            TelerikObject.Click(btnDeleteOnPopup);
            Assert.AreEqual("Deleted Successfully", toaster_message_deleteSuccess.InnerText, "Job is not deleted Successfully");
            JobAssertions_BeforeJobCreation();
        }


        public static void JobFieldsSaveAssertion(DataTable dt, int rownum)
        {

            string colnames = "Job Type;Job Reason;Job Driver;Status;AFE;Service Provider;Actual Cost;Begin Date;End Date;Job Duration Hours;Account Reference;Job Plan Cost;Total Cost;Remarks";
            string jobType = dt.Rows[rownum]["jobtype"].ToString();
            string jobReason = dt.Rows[rownum]["jobreason"].ToString();
            string jobDriver = dt.Rows[rownum]["jobdriver"].ToString();
            string status = dt.Rows[rownum]["status"].ToString();
            string afe = dt.Rows[rownum]["afe"].ToString();
            string serviceprovider = dt.Rows[rownum]["serviceProvider"].ToString();
            string actualCost = dt.Rows[rownum]["actualcost"].ToString();
            string beginDate = dt.Rows[rownum]["beindate"].ToString();
            string endDate = dt.Rows[rownum]["enddate"].ToString();
            string jobDurationHours = "0";
            string accoutnRef = dt.Rows[rownum]["accoutnref"].ToString();
            string jobPlanCost = dt.Rows[rownum]["jobplancost"].ToString();
            string totalCost = dt.Rows[rownum]["totalcost"].ToString();
            string remarks = dt.Rows[rownum]["remarks"].ToString();
            string colvals = jobType + ";" + jobReason + ";" + jobDriver + ";" + status + ";" + afe + ";" + serviceprovider + ";" + actualCost + ";" + beginDate + ";" + endDate + ";" + jobDurationHours + ";" + accoutnRef + ";" + jobPlanCost + ";" + totalCost + ";" + remarks;

            TelerikObject.verifyGridCellValues(colnames, colvals, rownum);
        }
        public static void JobBeforeEventAddAssertions()
        {
            Helper.CommonHelper.TraceLine("Ensuring Add Event is enabled");
            Assert.IsTrue(btnAddEvent.IsEnabled, "Add Event button is disabled");
            Helper.CommonHelper.TraceLine("Ensuring Mutli Add Event is enabled");
            Assert.IsTrue(btnMultiAddEvent.IsEnabled, "Mutli Add Event button is disabled");
            Helper.CommonHelper.TraceLine("Ensuring Paste Event  is disabled ");
            Assert.IsFalse(btnPasteEvent.IsEnabled, "Paste  Event button is enabled");
            Helper.CommonHelper.TraceLine("Ensuring Save Event button is disabled");
            //    string attrivalue = PageJobManagement.btnSaveEvent.Attributes.FirstOrDefault(x => x.Name == "style").Value;
            Assert.IsFalse(btnSaveEventMain.IsEnabled, "Save Event button  is enabled");
            Helper.CommonHelper.TraceLine("Ensuring Cancel Event is disabled");
            Assert.IsFalse(btnCancelEvent.IsEnabled, "Cancel Event button is enabled");

            //Ensure Click on + Button and see if Economic Analysis, Job Cost Details,Failure Report Job Plan and WellBore are seen
            TelerikObject.Click(addTab);
            Assert.IsNotNull(tabEconomicAnalysis, "Economic Analysis is not shown");
            Helper.CommonHelper.TraceLine("Economic Analysis is  shown");
            Assert.IsNotNull(tabJobCostDetails, "Job Cost Details is not shown");
            Helper.CommonHelper.TraceLine("Job Cost Details is shown");
            Assert.IsNotNull(tabFailureReport, "Failure Report  is not shown");
            Helper.CommonHelper.TraceLine("Failure Report is  shown");
            Assert.IsNotNull(tabJobplan, "Job Plan is not shown");
            Helper.CommonHelper.TraceLine("Job Plan is  shown");
            Assert.IsNotNull(tabWellBore, "Wellbore is not shown");
            Helper.CommonHelper.TraceLine("Wellbore is  shown");
            TelerikObject.Click(tabEvents);

        }

        public static void VerifyEventTableCellValues(DataTable dt, bool qeverify = false, int rownum = 0)
        {
            TelerikObject.verifyGridCellValues("Action;Event Type", "IgnoreValue;" + dt.Rows[0]["jobeventtype"].ToString(), rownum);
            string colnames = "Begin Time;End Time;Service Provider;Cost Category;Rate;Duration (Hours);Total Cost;AFE;Order;Workorder ID;Remarks;Change Date;Change User";
            string begindatetime = dt.Rows[0]["eventbeindate"].ToString() + " " + dt.Rows[0]["eventbeintime"].ToString();
            string enddatetime = dt.Rows[0]["eventenddate"].ToString() + " " + dt.Rows[0]["eventendtime"].ToString();
            string serviceprovider = dt.Rows[0]["serviceprovider"].ToString();
            string costcategory = dt.Rows[0]["costcategory"].ToString();
            string rate = dt.Rows[0]["eventcostrate"].ToString();
            string durationhours = dt.Rows[0]["durationhours"].ToString();
            string totalcost = "";
            totalcost = dt.Rows[0]["totalcost"].ToString();
            if (qeverify)
            {
                int intrate = -1;
                int intdurationhours = -1;

                int.TryParse(rate, out intrate);
                int.TryParse(durationhours, out intdurationhours);
                totalcost = (intrate * intdurationhours).ToString();
                jobTotalCost = totalcost;
            }
            //string  AFE;Oder;Workoder ID;Remarks
            string afe = dt.Rows[0]["afe"].ToString();
            string order = "0";
            string workorderid = dt.Rows[0]["workorderid"].ToString();
            string remarks = dt.Rows[0]["remarks"].ToString();
            if (remarks.Contains("Bundle Test Modify Remark") || remarks == "")
            {
                order = "1";
            }
            else if (remarks.Contains("Multi Cell Modify Remarks") || remarks == "")
            {
                order = "3";
            }
            string changedate = DateTime.Today.ToString("MM/dd/yyyy");
            UserPrincipal userPrincipal = UserPrincipal.Current;
            string name = userPrincipal.DisplayName;
            Helper.CommonHelper.TraceLine("Logged in user is " + name);
            string changeuser = name;
            string colvals = begindatetime + ";" + enddatetime + ";" + serviceprovider +
                             ";" + costcategory + ";" + rate + ";" + durationhours +
                             ";" + totalcost + ";" + afe + ";" + order + ";" + workorderid +
                             ";" + remarks + ";" + changedate + ";" + changeuser;

            TelerikObject.verifyGridCellValues(colnames, colvals, rownum);
        }

        public static void EnterGridCellEdit(DataTable dt, DataTable dtprev, int rownum = 0)
        {
            string colnames = "Begin Time;End Time;Service Provider;Cost Category;Rate;Duration (Hours);Total Cost;AFE;Order;Workorder ID;Remarks;Change Date;Change User";

            // DataTable having Old values to get table fetched
            string oldbegindatetime = dtprev.Rows[0]["eventbeindate"].ToString() + " " + dtprev.Rows[0]["eventbeintime"].ToString();
            string oldenddatetime = dtprev.Rows[0]["eventenddate"].ToString() + " " + dtprev.Rows[0]["eventendtime"].ToString();
            string oldserviceprovider = dt.Rows[0]["serviceprovider"].ToString();
            string oldcostcategory = "ignorevalue";
            string oldrate = "ignorevalue";
            string olddurationhours = "ignorevalue";
            string oldtotalcost = "ignorevalue";
            //string  AFE;Oder;Workoder ID;Remarks
            string oldafe = "ignorevalue";
            string oldorder = "ignorevalue";
            string oldworkorderid = dt.Rows[0]["workorderid"].ToString();
            string oldremarks = dt.Rows[0]["remarks"].ToString();
            string oldchangedate = DateTime.Today.ToString("MM/dd/yyyy");
            //      string changeuser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            string oldchangeuser = "ignorevalue";


            string colvalsold = oldbegindatetime + ";" + oldenddatetime + ";" + oldserviceprovider +
                             ";" + oldcostcategory + ";" + oldrate + ";" + olddurationhours +
                             ";" + oldtotalcost + ";" + oldafe + ";" + oldorder + ";" + oldworkorderid +
                             ";" + oldremarks + ";" + oldchangedate + ";" + oldchangeuser;



            // DataTable having New values to be entered:
            string begindatetime = dt.Rows[0]["eventbeindate"].ToString() + " " + dt.Rows[0]["eventbeintime"].ToString();
            string enddatetime = dt.Rows[0]["eventenddate"].ToString() + " " + dt.Rows[0]["eventendtime"].ToString();
            string serviceprovider = dt.Rows[0]["serviceprovider"].ToString();
            string costcategory = "ignorevalue";
            string rate = "ignorevalue";
            string durationhours = dt.Rows[0]["durationhours"].ToString();
            string totalcost = "ignorevalue";
            //string  AFE;Oder;Workoder ID;Remarks
            string afe = "ignorevalue";
            string order = "ignorevalue";
            string workorderid = dt.Rows[0]["workorderid"].ToString();
            string remarks = dt.Rows[0]["remarks"].ToString();
            string changedate = "ignorevalue";
            string changeuser = "ignorevalue";


            string colvalsnew = begindatetime + ";" + enddatetime + ";" + serviceprovider +
                             ";" + costcategory + ";" + rate + ";" + durationhours +
                             ";" + totalcost + ";" + afe + ";" + order + ";" + workorderid +
                             ";" + remarks + ";" + changedate + ";" + changeuser;
            TelerikObject.editGridCells(colnames, colvalsold, colvalsnew, rownum);
        }


        public static void VerifyMultiAdd(DataTable dt, bool qeverify = false)
        {
            TelerikObject.verifyGridCellValues("Action;Event Type", "IgnoreValue;Bundle Test");
            TelerikObject.verifyGridCellValues("Action;Event Type", "IgnoreValue;Chemical Analysis", 1);
            TelerikObject.verifyGridCellValues("Action;Event Type", "IgnoreValue;Wireline - Perforate", 2);
            string colnames = "Begin Time;End Time;Service Provider;Cost Category;Rate;Duration (Hours);Total Cost;AFE;Order;Workorder ID;Remarks;Change Date;Change User";
            string begindatetime = dt.Rows[0]["eventbeindate"].ToString() + " " + "12:00 AM";
            string enddatetime = dt.Rows[0]["eventenddate"].ToString() + " " + "12:00 AM";
            string serviceprovider = dt.Rows[0]["serviceprovider"].ToString();
            string costcategory = dt.Rows[0]["costcategory"].ToString();
            string rate = dt.Rows[0]["eventcostrate"].ToString();
            string durationhours = dt.Rows[0]["durationhours"].ToString();
            string totalcost = "";
            totalcost = dt.Rows[0]["totalcost"].ToString();
            if (qeverify)
            {
                int intrate = -1;
                int intdurationhours = -1;

                int.TryParse(rate, out intrate);
                int.TryParse(durationhours, out intdurationhours);
                totalcost = (intrate * intdurationhours).ToString();
            }
            //string  AFE;Oder;Workoder ID;Remarks
            string afe = dt.Rows[0]["afe"].ToString();
            string order = "1";
            string workorderid = dt.Rows[0]["workorderid"].ToString();
            string remarks = dt.Rows[0]["remarks"].ToString();
            string changedate = DateTime.Today.ToString("MM/dd/yyyy");
            UserPrincipal userPrincipal = UserPrincipal.Current;
            string name = userPrincipal.DisplayName;
            Helper.CommonHelper.TraceLine("Logged in user is " + name);
            string changeuser = name;
            string colvals = begindatetime + ";" + enddatetime + ";" + serviceprovider +
                             ";" + costcategory + ";" + rate + ";" + durationhours +
                             ";" + totalcost + ";" + afe + ";" + order + ";" + workorderid +
                             ";" + remarks + ";" + changedate + ";" + changeuser;

            TelerikObject.verifyGridCellValues(colnames, colvals);
            TelerikObject.verifyGridCellValues(colnames, colvals.Replace(order + ";", "2;"), 1);
            TelerikObject.verifyGridCellValues(colnames, colvals.Replace(order + ";", "3;"), 2);
        }


        public static void VerifyMultiAddwithClone(DataTable dt, bool qeverify = false)
        {
            TelerikObject.verifyGridCellValues("Action;Event Type", "IgnoreValue;Bundle Test");
            TelerikObject.verifyGridCellValues("Action;Event Type", "IgnoreValue;Bundle Test", 1);
            TelerikObject.verifyGridCellValues("Action;Event Type", "IgnoreValue;Chemical Analysis", 2);
            TelerikObject.verifyGridCellValues("Action;Event Type", "IgnoreValue;Chemical Analysis", 3);
            TelerikObject.verifyGridCellValues("Action;Event Type", "IgnoreValue;Wireline - Perforate", 4);
            TelerikObject.verifyGridCellValues("Action;Event Type", "IgnoreValue;Wireline - Perforate", 5);
            string colnames = "Begin Time;End Time;Service Provider;Cost Category;Rate;Duration (Hours);Total Cost;AFE;Order;Workorder ID;Remarks;Change Date;Change User";
            string begindatetime = dt.Rows[0]["eventbeindate"].ToString() + " " + "12:00 AM";
            string enddatetime = dt.Rows[0]["eventenddate"].ToString() + " " + "12:00 AM";
            string serviceprovider = dt.Rows[0]["serviceprovider"].ToString();
            string costcategory = dt.Rows[0]["costcategory"].ToString();
            string rate = dt.Rows[0]["eventcostrate"].ToString();
            string durationhours = dt.Rows[0]["durationhours"].ToString();
            string totalcost = "";
            totalcost = dt.Rows[0]["totalcost"].ToString();
            if (qeverify)
            {
                int intrate = -1;
                int intdurationhours = -1;

                int.TryParse(rate, out intrate);
                int.TryParse(durationhours, out intdurationhours);
                totalcost = (intrate * intdurationhours).ToString();
            }
            //string  AFE;Oder;Workoder ID;Remarks
            string afe = dt.Rows[0]["afe"].ToString();
            string order = "1";
            string workorderid = dt.Rows[0]["workorderid"].ToString();
            string remarks = dt.Rows[0]["remarks"].ToString();
            string changedate = DateTime.Today.ToString("MM/dd/yyyy");
            UserPrincipal userPrincipal = UserPrincipal.Current;
            string name = userPrincipal.DisplayName;
            Helper.CommonHelper.TraceLine("Logged in user is " + name);
            string changeuser = name;
            string colvals = begindatetime + ";" + enddatetime + ";" + serviceprovider +
                             ";" + costcategory + ";" + rate + ";" + durationhours +
                             ";" + totalcost + ";" + afe + ";" + order + ";" + workorderid +
                             ";" + remarks + ";" + changedate + ";" + changeuser;

            TelerikObject.verifyGridCellValues(colnames, colvals);
            TelerikObject.verifyGridCellValues(colnames, colvals.Replace(order + ";", "1;"), 1);
            TelerikObject.verifyGridCellValues(colnames, colvals.Replace(order + ";", "2;"), 2);
            TelerikObject.verifyGridCellValues(colnames, colvals.Replace(order + ";", "2;"), 3);
            TelerikObject.verifyGridCellValues(colnames, colvals.Replace(order + ";", "3;"), 4);
            TelerikObject.verifyGridCellValues(colnames, colvals.Replace(order + ";", "3;"), 5);
        }

        public static string[] GetCostnHour(string cnHourDetails)
        {
            Regex re = new Regex(@"\s{2,}");
            string neww = re.Replace(cnHourDetails, ";");
            string[] arr = neww.Split(new char[] { ';' });
            arr[0] = (arr[0].Substring(arr[0].LastIndexOf(':') + 1).Trim());
            arr[1] = (arr[1].Substring(arr[1].LastIndexOf(':') + 1).Trim());

            return arr;
        }

        /// <summary>
        /// Extract cost & hour value from given string
        /// </summary>
        /// <param name="cnHourDetails">string which contains Cost & Hour value</param>
        /// <returns>Array of string. Cost at 0th index & Hour at 1st index</returns>
        /// <author>Rahul Pingale</author>
        public static string[] GetHeaderDetails(string cnHourDetails)
        {
            Regex re = new Regex(@"\s{2,}");
            string neww = re.Replace(cnHourDetails, ";");
            string[] arr = neww.Split(new char[] { ';' });
            arr[0] = (arr[0].Substring(arr[0].LastIndexOf(':') + 1).Trim());
            arr[1] = (arr[1].Substring(arr[1].LastIndexOf(':') + 1).Trim());

            return arr;
        }

        /// <summary>
        /// Verify Update & delete button are enabled for planned event present at particular rowindex 
        /// </summary>
        /// <param name="index1">event row index on Job Plan grid</param>
        /// <author>Rahul Pingale</author>
        public static void VerifyActionButtonsPresence(int index1)
        {

            HtmlSpan updateButton = (HtmlSpan)TelerikObject.GetElement("HtmlSpan", "xpath", "//td[@ng-reflect-logical-row-index='" + index1 + "']/button/span[@title='Update']", "Update button for Planned Event " + index1);
            HtmlSpan deleteButton = (HtmlSpan)TelerikObject.GetElement("HtmlSpan", "xpath", "//td[@ng-reflect-logical-row-index='" + index1 + "']/button/span[@title='Delete']", "Delete button for Planned Event " + index1);
            Assert.IsTrue(updateButton.IsEnabled, "Update button is not enabled for event " + index1);
            Assert.IsTrue(deleteButton.IsEnabled, "Delete button is not enabled for event " + index1);
            Helper.CommonHelper.TraceLine("====>Update & Delete buttons are enabled for event " + index1);

        }

        public static void VerifyPlannedEventTableCellValues(DataTable dt, int rowindex, string actionName = "CreateEvent")
        {
            int rowindex1;
            if (!actionName.Equals("CreateEvent"))
                rowindex1 = 0;
            else
                rowindex1 = rowindex;

            string colnames = "Action;Event Type;Hours;Cost;Vendor;Truck Unit;Catalog Item;Brief Description";
            string action = "ignorevalue";
            string eventType = dt.Rows[rowindex1]["evttype"].ToString();
            string vendorname = dt.Rows[rowindex1]["vendor"].ToString();
            string catalogItem = dt.Rows[rowindex1]["catalogitem"].ToString();
            string truckUnit = dt.Rows[rowindex1]["truckunit"].ToString();
            string estimatedHours = dt.Rows[rowindex1]["estimatedhours"].ToString();
            //string accountingId = dt.Rows[rowindex1]["accountingid"].ToString();
            // string unitPrice = dt.Rows[rowindex1]["unitprice"].ToString();
            string estimatedTotalCost = dt.Rows[rowindex1]["estimatedtotalcost"].ToString();
            // string responsiblePerson = dt.Rows[rowindex1]["responsibleperson"].ToString();
            //string assignedToPerson = dt.Rows[rowindex1]["assignedtoperson"].ToString();
            string briefDesc = dt.Rows[rowindex1]["briefdesc"].ToString();
            // string remarks = dt.Rows[rowindex1]["remarks"].ToString();

            string colvals = action + ";" + eventType + ";" + estimatedHours +
                             ";" + estimatedTotalCost + ";" + vendorname + ";" + truckUnit +
                             ";" + catalogItem + ";" + briefDesc;

            TelerikObject.verifyGridCellValues(colnames, colvals, rowindex);
        }

        public static void VerifyJobDetailCostValues(DataTable dt, int rowindex, string actionName = "Create")
        {
            Helper.CommonHelper.TraceLine("====>Job Cost Details Grid verification is started");
            int rowindex1;
            if (!actionName.Equals("Create"))
                rowindex1 = 0;
            else
                rowindex1 = rowindex;


            if (rowindex != 0)
            {
                DynamicValue = dt.Rows[rowindex1]["date"].ToString() + "  -  " + dt.Rows[rowindex1]["cost"].ToString();
                TelerikObject.Click(accordian_eventDate);
            }

            string colnames = "Action;Vendor;Catalog Item;Cost;Quantity;Unit Price;Discount;Remarks";
            string action = "ignorevalue";
            string date = dt.Rows[rowindex1]["date"].ToString();
            string vendorname = dt.Rows[rowindex1]["vendor"].ToString();
            string catalogItem = dt.Rows[rowindex1]["catalogitem"].ToString();
            string quantity = dt.Rows[rowindex1]["quantity"].ToString();
            string unitPrice = dt.Rows[rowindex1]["unitprice"].ToString();
            string discAmount = dt.Rows[rowindex1]["discamount"].ToString();
            string cost = dt.Rows[rowindex1]["cost"].ToString();
            string remarks = dt.Rows[rowindex1]["remarks"].ToString();


            string colvals = action + ";" + vendorname +
                             ";" + catalogItem + ";" + cost + ";" + quantity + ";" + unitPrice +
                             ";" + discAmount + ";" + remarks;

            TelerikObject.verifyGridCellValues(colnames, colvals, 0);
        }

        public static void VerifyActionButtonsPresenceForJCD(DataTable dt, int index1)
        {
            HtmlTable oldRecord = TelerikObject.getTableByColumnName(dt.Rows[0]["vendor"].ToString());
            //    Assert.IsNull(oldRecord.BodyRows[0].BaseElement.Children[0].Children[0],);
            Assert.IsTrue(oldRecord.BodyRows[0].BaseElement.Children[0].Children[0].InnerMarkup.ToLower().Contains("update"), "Update button is not present");
            Assert.IsTrue(oldRecord.BodyRows[0].BaseElement.Children[0].Children[1].InnerMarkup.ToLower().Contains("delete"), "Delete button is not present");
            Assert.IsTrue(oldRecord.BodyRows[0].BaseElement.Children[0].Children[2].InnerMarkup.ToLower().Contains("clone"), "Clone button is not present");

        }
        #endregion

    }

}
