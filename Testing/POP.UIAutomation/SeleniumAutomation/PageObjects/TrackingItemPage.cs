using System;
using OpenQA.Selenium;
using SeleniumAutomation.SeleniumObject;

namespace SeleniumAutomation.PageObjects
{
    class TrackingItemPage
    {
        #region  NonGridControls
        public static By lnkTrackingItem { get { return SeleniumActions.getByLocator("Xpath", "//div[@id='lnktrackingicon']", "Tracking Item Link"); } }
        public static By btnAddTrackingItem { get { return SeleniumActions.getByLocator("Xpath", "//span[@id='btnAddTrackingItem']", "Add Button"); } }

        public static By btnAddTrackingItemComment { get { return SeleniumActions.getByLocator("Xpath", "//span[@id='btnAddComment']", "Add Comment Button"); } }
        public static By btnAddTrackingItemAttachment { get { return SeleniumActions.getByLocator("Xpath", "//div[@role='button']", "Add Attachment Button"); } }

        public static By btnAddTrackingApplyComment { get { return SeleniumActions.getByLocator("Xpath", "//span[@id='btnApplyComment']", "Apply Comment  Button"); } }
        // :  

        public static By btnsave { get { return SeleniumActions.getByLocator("Xpath", "//button/descendant::span[contains(text(),'Save')]", "Save Button"); } }

        //
        public static By btnApplyOnModal { get { return SeleniumActions.getByLocator("Xpath", "//button[text()=' Apply ' ]", "Apply Modal Button"); } }

        //
        public static By lstStatus { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Status']/following-sibling::*/kendo-dropdownlist", "Status Dropdown"); } }

        public static By txtStartDate { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Start Date']/following-sibling::*/descendant::input", "Start Date"); } }

        public static By txtDueDate { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Due Date']/following-sibling::*/descendant::input", "Due Date"); } }

        public static By lstCategory { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Category']/following-sibling::*/kendo-dropdownlist", "Category"); } }

        public static By lstType { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Type']/following-sibling::*/kendo-dropdownlist", "Type"); } }

        public static By lstSubType { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Subtype']/following-sibling::*/kendo-dropdownlist", "SubType Dropdown"); } }

        public static By lstPlannedTask { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Planned Task']/following-sibling::*/kendo-dropdownlist", "PlannedTask Dropdown"); } }

        public static By lstPriority { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Priority']/following-sibling::*/kendo-dropdownlist", "Priority Dropdown"); } }

        public static By lstDistribution { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Distribution']/following-sibling::*/kendo-dropdownlist", "Distribution Dropdown"); } }

        public static By lstAssignedTo { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Assigned To']/following-sibling::*/kendo-combobox/descendant::input", "AssignedTo Dropdown"); } }

        public static By lstEntity { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Entity']/following-sibling::*/kendo-dropdownlist", "Entity Dropdown"); } }

        public static By lstEntityName { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Entity Name']/following-sibling::*/kendo-combobox/descendant::input", "EntityName Dropdown"); } }

        public static By txtSubject { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Subject']/following-sibling::*/textarea", "Subject"); } }

        public static By txtDescription { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Description']/following-sibling::*/textarea", "Description"); } }
        //
        public static By txtFromDate { get { return SeleniumActions.getByLocator("Xpath", "//label[contains(text(),'From')]/parent::*/following-sibling::*/descendant::input", "From Date"); } }

        public static By txtToDate { get { return SeleniumActions.getByLocator("Xpath", "//label[contains(text(),'To')]/parent::*/following-sibling::*/descendant::input", "To Date"); } }

        public static By btnRefreshGrid { get { return SeleniumActions.getByLocator("Xpath", "//span[@id='btnRefresh']", "Refresh button"); } }

        //txtareaComment

        public static By txtCommentText { get { return SeleniumActions.getByLocator("Xpath", "//textarea[@id='txtareaComment']", "Comments Text Box"); } }
        #endregion

        #region  MasterGrid
        public static By scrollHorizontalContainerTrackingItemMasterGird { get { return SeleniumActions.getByLocator("Xpath", "(//div[@ref='eBodyHorizontalScrollViewport'])[2]", " Scrollbar Tracking Item Grid container"); } }
        public static By FileCount { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='AttachmentsCount']", "Column Name AttachmentsCount"); } }
        public static By CreatedDate { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingItemCreatedDateJS']", "Column Name TrackingItemCreatedDateJS"); } }
        public static By Entity { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingEntity']", "Column Name TrackingEntity"); } }
        public static By EntityName { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingEntityName']", "Column Name TrackingEntityName"); } }
        public static By Priority { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingPriority']", "Column Name TrackingPriority"); } }
        public static By Status { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingStatus']", "Column Name TrackingStatus"); } }
        public static By Category { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingCategory']", "Column Name TrackingCategory"); } }
        public static By Type { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingItemType.Name']", "Column Name TrackingItemType"); } }
        public static By Subtype { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingItemSubType.Name']", "Column Name TrackingItemSubType"); } }
        public static By PlannedTask { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingItemPlanningTask']", "Column Name Tracking Item Planning Task"); } }
        public static By Subect { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingItemSubject']", "Column Name TrackingItemSubject"); } }
        public static By Description { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingItemDescription']", "Column Name TrackingItemDescription"); } }
        public static By CreatedBy { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingItemCreatedBy']", "Column Name TrackingItemCreatedBy"); } }
        public static By Distribution { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingItemDistribution']", "Column Name TrackingItemDistribution"); } }
        public static By Assignedto { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingItemAssignTo']", "Column Name TrackingItemAssignTo"); } }
        public static By StartDate { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingItemStartDateJS']", "Column Name TrackingItemStartDateJS"); } }
        public static By DueDate { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingItemDueDateJS']", "Column Name TrackingItemDueDateJS"); } }
        public static By ClosingDate { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingItemClosingDateJS']", "Column Name TrackingItemClosingDateJS"); } }
        public static By UpdatedDate { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='ChangeDateJS']", "Column Name TrackingItemUpdatedDateJS"); } }
        public static By LastComment { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingItemLatestComment']", "Column Name TrackingItemLatestComment"); } }
        public static By ReportingManager { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='TrackingItemReportingManager']", "Column Name TrackingItemReportingManager"); } }


        #endregion

    }
}
