using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeleniumAutomation.SeleniumObject;
using System.Configuration;
using SeleniumAutomation.Helper;
using System.Diagnostics;
using AventStack.ExtentReports;
using System.IO;
using System;
using System.Threading;
using OpenQA.Selenium;
using System.Collections.Generic;
using SeleniumAutomation.AGGridScreenDatas;
using SeleniumAutomation.TestData;
using System.ComponentModel;

namespace SeleniumAutomation.TestClasses
{
    static class TrackingItem
    {
        
        public static void AddTrackingItem(ExtentTest test)
        {
            TrackingItemData testdata = new TrackingItemData();
            AddTypeandSubTypeFromAPI();
            CommonHelper.CreateESPWell();
            AddtrackingItmesUI(testdata);
            AddtrackingItemComment();
            AddtrackingItemAttachment();
            SeleniumActions.waitClick(PageObjects.TrackingItemPage.btnsave);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.TrackingItemPage.btnApplyOnModal);
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.TrackingItemPage.txtFromDate, DateTime.Today.AddDays(-5).ToString("MMddyyyy"));
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.TrackingItemPage.txtToDate, testdata.DueDate.Replace("/", String.Empty));
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.TrackingItemPage.btnRefreshGrid);
            SeleniumActions.WaitForLoad();
            VerifyTrackingItemData(test);
        }

        public static void AddtrackingItemComment()
        {
            SeleniumActions.waitClick(PageObjects.TrackingItemPage.btnAddTrackingItemComment);
            SeleniumActions.sendText(PageObjects.TrackingItemPage.txtCommentText, "UIA Comment 1");
            SeleniumActions.waitClick(PageObjects.TrackingItemPage.btnAddTrackingApplyComment);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.TrackingItemPage.btnAddTrackingItemComment);
            SeleniumActions.sendText(PageObjects.TrackingItemPage.txtCommentText, "UIA Comment 2");
            SeleniumActions.waitClick(PageObjects.TrackingItemPage.btnAddTrackingApplyComment);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.TrackingItemPage.btnAddTrackingItemComment);
            SeleniumActions.sendText(PageObjects.TrackingItemPage.txtCommentText, "UIA Comment 3");
            SeleniumActions.waitClick(PageObjects.TrackingItemPage.btnAddTrackingApplyComment);


        }

        public static void AddtrackingItmesUI(TrackingItemData testdata)
        {
            SeleniumActions.refreshBrowser();
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.TrackingItemPage.lnkTrackingItem);
            SeleniumActions.WaitForLoad();
            SeleniumActions.waitClick(PageObjects.TrackingItemPage.btnAddTrackingItem);
            SeleniumActions.WaitForLoad();
            SeleniumActions.selectKendoDropdownValue(PageObjects.TrackingItemPage.lstStatus, testdata.Status);
            SeleniumActions.selectKendoDropdownValue(PageObjects.TrackingItemPage.lstCategory, testdata.Category);
            SeleniumActions.selectKendoDropdownValue(PageObjects.TrackingItemPage.lstType, testdata.Type);
            SeleniumActions.selectKendoDropdownValue(PageObjects.TrackingItemPage.lstSubType, testdata.Subtype);
            SeleniumActions.selectKendoDropdownValue(PageObjects.TrackingItemPage.lstPlannedTask, testdata.PlannedTask);
            SeleniumActions.selectKendoDropdownValue(PageObjects.TrackingItemPage.lstPriority, testdata.Priority);
            SeleniumActions.selectKendoDropdownValue(PageObjects.TrackingItemPage.lstDistribution, testdata.Distribution);
            SeleniumActions.KendoTypeNSelect(PageObjects.TrackingItemPage.lstAssignedTo, testdata.Assignedto);
            SeleniumActions.selectKendoDropdownValue(PageObjects.TrackingItemPage.lstEntity, testdata.Entity);
            SeleniumActions.KendoTypeNSelect(PageObjects.TrackingItemPage.lstEntityName, testdata.EntityName);
            SeleniumActions.sendText(PageObjects.TrackingItemPage.txtStartDate, testdata.StartDate.Replace("/", String.Empty));
            SeleniumActions.sendText(PageObjects.TrackingItemPage.txtDueDate, testdata.DueDate.Replace("/", String.Empty));
            SeleniumActions.sendText(PageObjects.TrackingItemPage.txtSubject, testdata.Subect);
            SeleniumActions.sendText(PageObjects.TrackingItemPage.txtDescription, testdata.Description);
        }

        public static void EdittrackingItmesUI(TrackingItemData testdata)
        {
            
            SeleniumActions.waitClick(PageObjects.TrackingItemPage.btnAddTrackingItem);
            SeleniumActions.WaitForLoad();
            SeleniumActions.selectKendoDropdownValue(PageObjects.TrackingItemPage.lstStatus, testdata.Status);
            SeleniumActions.selectKendoDropdownValue(PageObjects.TrackingItemPage.lstCategory, testdata.Category);
            SeleniumActions.selectKendoDropdownValue(PageObjects.TrackingItemPage.lstType, testdata.Type);
            SeleniumActions.selectKendoDropdownValue(PageObjects.TrackingItemPage.lstSubType, testdata.Subtype);
            SeleniumActions.selectKendoDropdownValue(PageObjects.TrackingItemPage.lstPlannedTask, testdata.PlannedTask);
            SeleniumActions.selectKendoDropdownValue(PageObjects.TrackingItemPage.lstPriority, testdata.Priority);
            SeleniumActions.selectKendoDropdownValue(PageObjects.TrackingItemPage.lstDistribution, testdata.Distribution);
            SeleniumActions.KendoTypeNSelect(PageObjects.TrackingItemPage.lstAssignedTo, testdata.Assignedto);
            SeleniumActions.selectKendoDropdownValue(PageObjects.TrackingItemPage.lstEntity, testdata.Entity);
            SeleniumActions.KendoTypeNSelect(PageObjects.TrackingItemPage.lstEntityName, testdata.EntityName);
            SeleniumActions.sendText(PageObjects.TrackingItemPage.txtStartDate, testdata.StartDate.Replace("/", String.Empty));
            SeleniumActions.sendText(PageObjects.TrackingItemPage.txtDueDate, testdata.DueDate.Replace("/", String.Empty));
            SeleniumActions.sendText(PageObjects.TrackingItemPage.txtSubject, testdata.Subect);
            SeleniumActions.sendText(PageObjects.TrackingItemPage.txtDescription, testdata.Description);
        }

        public static void AddtrackingItemAttachment()
        {
            SeleniumActions.waitClick(PageObjects.TrackingItemPage.btnAddTrackingItemAttachment);
            string path = Path.Combine(ConfigurationManager.AppSettings.Get("FilesLocation") + @"\UIAutomation\Smoke_TestData", "ESPDataPump.xml");
            SeleniumActions.WaitForLoad();
            SeleniumActions.FileUploadDialog(path);
            SeleniumActions.WaitForLoad();
            // Add attachment 2
            SeleniumActions.waitClick(PageObjects.TrackingItemPage.btnAddTrackingItemAttachment);
            path = Path.Combine(ConfigurationManager.AppSettings.Get("FilesLocation") + @"\UIAutomation\Smoke_TestData", "GLTubingData.xml");
            SeleniumActions.WaitForLoad();
            SeleniumActions.FileUploadDialog(path);
            SeleniumActions.WaitForLoad();
        }

        public static void AddTypeandSubTypeFromAPI()
        {
            CommonHelper.AddActionTrackingTypeSubType();
        }

        public static void VerifyTrackingItemData(ExtentTest test)
        {
            TrackingItemMasterGridDataProcess w = new TrackingItemMasterGridDataProcess();
            TypeTrackingItemMasterGrid[] actualgrid = w.FetchdataScreen();
            Trace.WriteLine($"Grid Records fetched : {actualgrid.Length}");
            int k = 2;  //First Row '0' is header row and Seocnd Row '1' is unit text row
            TrackingItemData testdata = new TrackingItemData();
            SeleniumActions.CustomAssertEqual(testdata.Files, actualgrid[k].Files, "Files Count Mismatch", $"Files count expected {testdata.Files} and actual {actualgrid[k].Files}", test);
            DateTime expcreatedate = DateTime.Parse(testdata.CreatedDate);
            DateTime actstartdate = DateTime.Parse(actualgrid[k].CreatedDate);
            Assert.IsTrue((expcreatedate - actstartdate).Seconds < 60, "The Start Date Deviation does not match");
            SeleniumActions.CustomAssertEqual(testdata.Entity, actualgrid[k].Entity, "Entity Mismatch","Entity field verified.", test);
            SeleniumActions.CustomAssertEqual(testdata.EntityName, actualgrid[k].EntityName, "EntityName Mismatch", "EntityName field verified.",test);
            SeleniumActions.CustomAssertEqual(testdata.Priority, actualgrid[k].Priority, "Priority Mismatch", "Priority field verified.", test);
            SeleniumActions.CustomAssertEqual(testdata.Status, actualgrid[k].Status,"Status Mismatch", "Status field verified.", test);
            SeleniumActions.CustomAssertEqual(testdata.Category, actualgrid[k].Category, "Category Mismatch", "Category field verified.",  test);
            SeleniumActions.CustomAssertEqual(testdata.Type, actualgrid[k].Type, "Type Mismatch", "Type field verified.",  test);
            SeleniumActions.CustomAssertEqual(testdata.Subtype, actualgrid[k].Subtype, "Subtype Mismatch", "Subtype field verified.",  test);
            SeleniumActions.CustomAssertEqual(testdata.PlannedTask, actualgrid[k].PlannedTask, "PlannedTask Mismatch", "PlannedTask field verified.",  test);
            SeleniumActions.CustomAssertEqual(testdata.Subect, actualgrid[k].Subect, "Subect Mismatch","Subect field verified.",  test);
            SeleniumActions.CustomAssertEqual(testdata.Description, actualgrid[k].Description, "Description Mismatch", "Description field verified.",  test);
            SeleniumActions.CustomAssertEqual(testdata.CreatedBy, actualgrid[k].CreatedBy, "CreatedBy Mismatch", "CreatedBy field verified.", test);
            SeleniumActions.CustomAssertEqual(testdata.Distribution, actualgrid[k].Distribution, "Distribution Mismatch", "Distribution field verified.",  test);
            SeleniumActions.CustomAssertEqual(testdata.CreatedBy, actualgrid[k].Assignedto, "Assignedto Mismatch", "Assignedto field verified.", test);
            SeleniumActions.CustomAssertEqual(DateTime.Parse(testdata.StartDate).Date.ToString(), DateTime.Parse(actualgrid[k].StartDate).Date.ToString(), "StartDate", "StartDate Mismatch", test);
            SeleniumActions.CustomAssertEqual(DateTime.Parse(testdata.DueDate).Date.ToString(), DateTime.Parse(actualgrid[k].DueDate).Date.ToString(), "DueDate Mismatch", "DueDate field verified.",  test);
            Assert.IsTrue( (DateTime.Parse(testdata.UpdatedDate).Date - DateTime.Parse(actualgrid[k].UpdatedDate).Date).Seconds < 60, "UpdatedDate Mismatch");
            SeleniumActions.CustomAssertEqual(testdata.ClosingDate, actualgrid[k].ClosingDate, "ClosingDate Mismatch", "ClosingDate field verified.",  test);
            SeleniumActions.CustomAssertEqual(testdata.LastComment, actualgrid[k].LastComment, "LastComment Mismatch", "LastComment field verified.", test);
            SeleniumActions.CustomAssertEqual(testdata.ReportingManager, actualgrid[k].ReportingManager, "ReportingManager Mismatch", "ReportingManager field verified.",  test);
        }



    }
}
