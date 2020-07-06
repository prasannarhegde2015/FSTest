using OpenQA.Selenium;
using SeleniumAutomation.SeleniumObject;

namespace SeleniumAutomation.PageObjects
{
    class JobManagementPage
    {
        public static string strDynamicValue = string.Empty;
        #region GeneralTab
        public static By fieldserviceTab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Field Services']", "Field Services Tab"); } }
        public static By jobManagementTab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Job Management']", "JobManagement Tab"); } }
        public static By scrollhorizontawellborereport { get { return SeleniumActions.getByLocator("Xpath", "(//div[@ref='eBodyHorizontalScrollViewport'])[3]", " Scrollbar"); } }
        public static By scrollhorizontaindex2 { get { return SeleniumActions.getByLocator("Xpath", "(//div[@ref='eBodyHorizontalScrollViewport'])[2]", " Scrollbar"); } }
        public static By scrollhorizontaindex4 { get { return SeleniumActions.getByLocator("Xpath", "(//div[@ref='eBodyHorizontalScrollViewport'])[4]", " Scrollbar"); } }

        public static By scrollhorizontalcontainerwellborereport { get { return SeleniumActions.getByLocator("Xpath", "(//div[@ref='eBodyHorizontalScrollContainer'])[3]", " Scrollbar container"); } }
        public static By scrollhorizontalcontainerindex2 { get { return SeleniumActions.getByLocator("Xpath", "(//div[@ref='eBodyHorizontalScrollContainer'])[2]", " Scrollbar container"); } }
        public static By scrollhorizontalcontainerindex4 { get { return SeleniumActions.getByLocator("Xpath", "(//div[@ref='eBodyHorizontalScrollContainer'])[4]", " Scrollbar container"); } }

        public static By firstjobingrid { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='JobId']//span[@class='cell-layer-1']/span[2]", "JobManagement Tab"); } }
        public static By generalTab { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'General')]", "General Tab"); } }
        public static By jobstatusview { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(text(),'Job Status View')]", "JobStatusView"); } }
        public static By entertoursheet { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(text(),'Enter Tour Sheet')]", "Enter Tour Sheet"); } }
        public static By morningreporttab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Morning Report']", "Morning Report"); } }
        public static By currentwellboretab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Current Wellbore']", "Current Wellbore Tab"); } }
        public static By wellborehistorytab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Wellbore History']", "Wellbore history Tab"); } }
        public static By prospectivetab { get { return SeleniumActions.getByLocator("Xpath", "//span[contains(text(),'Prospective Jobs')]", "Prospective tab"); } }
        public static By readyjobsetab { get { return SeleniumActions.getByLocator("Xpath", "//span[contains(text(),'Ready Jobs')]", "Ready Jobs tab"); } }
        public static By concludedjobstab { get { return SeleniumActions.getByLocator("Xpath", "//span[contains(text(),'Concluded Jobs')]", "Concluded Jobs tab"); } }
        public static By jobPlanWizard { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(text(),'Job Plan Wizard')]", "Job Plan Wizard"); } }
        public static By jobwithouttemplate { get { return SeleniumActions.getByLocator("Xpath", "//label[contains(text(),'Create a job plan with no template.')]", "Radio select for create job without template"); } }
        public static By jobwithtemplate { get { return SeleniumActions.getByLocator("Xpath", "//label[contains(text(),'Create a job plan based on a template.')]", "Radio select for create job with template"); } }
        public static By modifyjobviajobplan { get { return SeleniumActions.getByLocator("Xpath", "//label[contains(text(),'Modify the job plan of an existing job for this we')]", "Radio select for update job via jobplan"); } }
        //public static By addbutton { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Add')]", "Add button"); } }
        public static By addbutton { get { return SeleniumActions.getByLocator("Xpath", "//span[contains(text(),'Add')]", "Add button"); } }

        public static By addhyperlink { get { return SeleniumActions.getByLocator("Xpath", "(//li[contains(text(), 'Add')])[1]", "Add Hyperlink"); } }

        public static By multiadd { get { return SeleniumActions.getByLocator("Xpath", "(//li[contains(text(), 'Add')])[2]", "Add Multi Event"); } }
        // public static By addjob { get { return SeleniumActions.getByLocator("Xpath", " //a[@translate='add']", "Add job"); } }
        public static By addjob { get { return SeleniumActions.getByLocator("Xpath", "//li[text()=' Add  ']", "Add job"); } }

        //public static By addjobfromtemplate { get { return SeleniumActions.getByLocator("Xpath", "//a[@translate='Add from Template']", "Add job from template"); } }

        public static By addjobfromtemplate { get { return SeleniumActions.getByLocator("Xpath", "//li[contains(text(),'Add from Template')]", "Add job from template"); } }

        public static By category { get { return SeleniumActions.getByLocator("Xpath", "//kendo-panelbar-item/span[contains(text(),'" + TestData.Fieldservice.JobCat + "')]", "category"); } }
        public static By panelitemjobtype { get { return SeleniumActions.getByLocator("Xpath", "//kendo-panelbar-item//div[contains(text(),'" + TestData.JobDet.JobType + "')]", "Job type"); } }
        public static By selectbutton { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Select')]", "Select Button"); } }
        public static By jobtypedrpdwn { get { return SeleniumActions.getByLocator("Xpath", "//span[contains(text(),'Select Job Type')]", "Job Type Dropdown"); } }

        public static By jobtype { get { return SeleniumActions.getByLocator("Xpath", "//li[text()='" + TestData.JobDet.JobType + "']", "Job Type"); } }

        public static By jobreasondrpdwn { get { return SeleniumActions.getByLocator("Xpath", "//span[contains(text(),'Select Job Reason')]", "Job Reason Dropdown"); } }

        public static By jobreason { get { return SeleniumActions.getByLocator("Xpath", "//li[text()='" + TestData.JobDet.JobReason + "']", "Job Reason"); } }


        public static By jobstatusdrpdwn { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dropdownlist[@name='status']", "Job Status Dropdown"); } }
        public static By jobstatusdrpdwninjobplan { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-dropdownlist)[4]", "Job Status Dropdown"); } }

        public static By jobstatus { get { return SeleniumActions.getByLocator("Xpath", "//li[text()='" + TestData.JobDet.JobStatus + "']", "Job status"); } }

        public static By begindate { get { return SeleniumActions.getByLocator("Xpath", "//kendo-datepicker[@name='beginDate']//input[@placeholder='null']", "Bgin Date"); } }
        public static By begindateforjobinjobplan { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-datepicker//input[@placeholder='null'])[1]", "Bgin Date"); } }

        public static By enddate { get { return SeleniumActions.getByLocator("Xpath", "//kendo-datepicker[@name='endDate']//input[@placeholder='null']", "End Date"); } }
        public static By enddateforjobinjobplan { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-datepicker//input[@placeholder='null'])[2]", "End Date"); } }

        public static By save { get { return SeleniumActions.getByLocator("Xpath", "//button[@type='button'][contains(text(),'Save')]", "Save"); } }

        public static By savewellboregridcomponets { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(@class,'control-rail')][contains(text(),'Save')]", "Save"); } }
        public static By saveevent { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='control-rail']//button[text()=' Save ']", "Save"); } }
        public static By savebuttonjobplanwizard { get { return SeleniumActions.getByLocator("Xpath", "//input[@value='Save']", "Save"); } }

        public static By savebutton { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='rightButtons']//button[@type='button'][contains(text(),'Save')]", "Save"); } }

        public static By toaster { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(@class,'toast-message')]", "Toast message"); } }

        // public static By jobidgrid { get { return SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr/td[2]", "Job id TD"); } }

        //public static By jobidgrid { get { return SeleniumActions.getByLocator("Xpath", "//ag-grid-angular//div[@col-id='JobId']/span/span/span/span", "Job id TD"); } }

        public static By firstJobId { get { return SeleniumActions.getByLocator("Xpath", "//div[@row-index='0']//div[@col-id='JobId']//span[@class='cell-layer-1']/span[2]", "First Job Id"); } }

        public static By firstEvent { get { return SeleniumActions.getByLocator("Xpath", "//jobeventgrid//kendo-grid-list", "First Event in the Grid"); } }

        public static By jobidgrid { get { return SeleniumActions.getByLocator("Xpath", "(//generic-grid)[1]//ag-grid-angular//div[@col-id='JobId']/span/span/span/span[2]", "Job id TD"); } }
        public static By jobidgridwhenviewcreatedjobfromjobplan { get { return SeleniumActions.getByLocator("Xpath", "(//generic-grid)[1]//ag-grid-angular//div[@col-id='JobId_1']/span/span/span/span[2]", "Job id TD"); } }
        public static string jobidgridwhenviewcreatedjobfromjobplans { get { return "(//generic-grid)[1]//ag-grid-angular//div[@col-id='JobId_1']/span/span/span/span[2]"; } }

        public static By jobtypegrid { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[2]/tbody/tr/td/span", "Job type TD"); } }
        public static By jobreasongrid { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[2]/tbody/tr/td[2]/span", "JobReason TD"); } }
        public static By parttypegrid { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[2]/tbody/tr/td[3]/span", "Comp group"); } }
        public static By compnamegrid { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[2]/tbody/tr/td[4]/span", "Comp name grid"); } }
        public static By complengrid { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[2]/tbody/tr/td[8]", "Comp length grid"); } }
        public static By compdepgrid { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[2]/tbody/tr/td[9]/span", "Comp dep grid"); } }
        public static By bottomdepgrid { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[2]/tbody/tr/td[10]/span", "Bottom dep grid"); } }

        public static By jobstatusgrid { get { return SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr/td[4]", "JobStatus TD"); } }

        public static By jobbegindategrid { get { return SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr/td[8]", "Begindate TD"); } }

        public static By jobenddategrid { get { return SeleniumActions.getByLocator("Xpath", "//kendo-grid-list//tbody/tr/td[9]", "Enddate TD"); } }

        public static By updatebutton { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Update')]", "Update Button"); } }
        public static By copybutton { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Copy')]", "Copy Button"); } }
        public static By pastebutton { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Paste')]", "Paste Button"); } }

        public static By deleteventbutton { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='control-rail']//button[contains(text(),'Delete')]", "Delete Event Button"); } }

        //public static By deletejobbutton { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='col-lg-12']//button[@type='button'][contains(text(),'Delete')]", "Delete Button"); } }

        public static By deletejobbutton { get { return SeleniumActions.getByLocator("Xpath", "//button[@class='control-rail-button-jm k-button'][contains(text(),' Delete ')]", "Delete Button"); } }
        public static By confirmdialogyes { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog-actions//button[contains(text(), 'Yes')]", "Yes Button"); } }

        public static By confirmdialogyes2 { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Yes, Delete this Job')]", "Yes Button"); } }
        public static By confirmdialogyesdelete { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Yes, Delete the selected event(s).')]", "Yes Delete Button"); } }
        public static By jobhistorybutton { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Job Status History')]", "Job History Button"); } }


        public static By jobidjobhistorydialog { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog[@id='jobHistoryModalTitle']//table/tbody/tr/td[1]", "Job ID in job history dialog"); } }

        public static By jobstatusjobhistorydialog { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog[@id='jobHistoryModalTitle']//table/tbody/tr/td[3]", "Job status in job history dialog"); } }

        public static By userjobhistorydialog { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog[@id='jobHistoryModalTitle']//table/tbody/tr/td[4]", "Domain user in job history dialog"); } }

        public static By eventtab { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Events')]", "EventTab"); } }

        public static By eventcombobox { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='wellboreComponent']//kendo-combobox//span[@class='k-select']", "Combobox event type"); } }

        public static By eventbdate { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-dateinput)[1]//input", "Begindateevent"); } }

        public static By eventedate { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-dateinput)[2]//input", "Enddateevent"); } }

        // public static By eventserviceprovider { get { return SeleniumActions.getByLocator("Xpath", "//kendo-combobox[@id='evcFK_BusinessOrganization']//span[@class='k-select']", "Service provider drpodwn"); } }

        public static By eventcostcategory { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-combobox)[4]", "Cost Category drpodwn"); } }
        public static By eventtruckunit { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-combobox)[5]", "Truck Unit drpodwn"); } }

        public static By grideventtype { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[2]//tr/td[2]/span", "Event type in grid"); } }

        public static By grideventbegindate { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr/td[1]/span", "Event Begin Date in grid"); } }
        public static By grideventbegindateinput { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr/td[1]//input", "Event Begin Date in grid input text field"); } }
        public static By grideventenddate { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr/td[2]/span", "Event end Date in grid"); } }

        public static By eventbdatespecific { get { return SeleniumActions.getByLocator("Xpath", "//kendo-datepicker[@id='evcEventBegDtTm']//input", "Begindateevent"); } }
        public static By closedialogbox { get { return SeleniumActions.getByLocator("Xpath", "//a[@aria-label='Close']", "Close Dialog"); } }
        public static By Additionallink { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(text(),'Additional')]", "Additional link"); } }
        public static By Requiredlink { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(text(),'Required')]", "Required link"); } }
        public static By hours { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='evcDurationHours']//input", "Hours"); } }
        public static By cost { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='evcTotalCost']//input", "Cost"); } }
        public static By totalhourslabel { get { return SeleniumActions.getByLocator("Xpath", "(//span[@class='cell-layer-1-group-duration'])[1]", "Total hours"); } }
        public static By totalcostlabel { get { return SeleniumActions.getByLocator("Xpath", "(//span[@class='cell-layer-1-group-duration'])[2]", "Total cost"); } }

        public static By eventserviceprovider { get { return SeleniumActions.getByLocator("Xpath", "//kendo-combobox[@id='evcFK_BusinessOrganization']//input", "Service provider drpodwn"); } }
        public static By grideventserviceprovider { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr/td[3]/span", "Event service provider in grid"); } }

        public static string EventTypeCol { get { return "//div[@col-id='EventType']//span[@class='cell-layer-1']/span[2]"; } }
        public static string BeginTime { get { return "//div[@col-id='BeginTimeJS']//span[@class='cell-layer-1']/span[2]"; } }
        public static string EndTime { get { return "//div[@col-id='EndTimeJS']//span[@class='cell-layer-1']/span[2]"; } }
        public static string Order { get { return "//div[@col-id='Order']//span[@class='cell-layer-1']/span[2]"; } }
        public static string ServiceProvider { get { return "//div[@col-id='BusinessOrganizationControlIdText']//span[@class='cell-layer-1']/span[2]"; } }
        public static string TruckUnitID { get { return "//div[@col-id='TruckUnitControlIdText']//span[@class='cell-layer-1']/span[2]"; } }
        public static string Personperforming { get { return "//div[@col-id='PersonPerformingTask']//span[@class='cell-layer-1']/span[2]"; } }
        public static string CostCat { get { return "//div[@col-id='CatalogItemControlIdText']//span[@class='cell-layer-1']/span[2]"; } }
        public static string HisRate { get { return "//div[@col-id='HistoricalRate']//span[@class='cell-layer-1']/span[2]"; } }
        public static string Duration { get { return "//div[@col-id='Duration']//span[@class='cell-layer-1']/span[2]"; } }
        public static string TotalCOst { get { return "//div[@col-id='TotalCost']//span[@class='cell-layer-1']/span[2]"; } }
        public static string AFE { get { return "//div[@col-id='AFEControlIdText']//span[@class='cell-layer-1']/span[2]"; } }
        public static string workorder { get { return "//div[@col-id='WorkorderID']//span[@class='cell-layer-1']/span[2]"; } }
        public static string remarks { get { return "//div[@col-id='Remarks']//span[@class='cell-layer-1']/span[2]"; } }
        public static string changedate { get { return "//div[@col-id='ChangeDateJS']//span[@class='cell-layer-1']/span[2]"; } }
        public static string changeuser { get { return "//div[@col-id='ChangeUser']//span[@class='cell-layer-1']/span[2]"; } }
        public static By grideventtruckunit { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr/td[4]/span", "Truck unit in grid"); } }
        public static By grideventcostcategory { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr/td[6]/span", "Cost category in grid"); } }
        public static By grideventtyperow2 { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[2]//tr[2]/td[2]/span", "Event type in grid"); } }

        public static By grideventbegindaterow2 { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr[2]/td[1]/span", "Event Begin Date in grid"); } }
        public static By grideventbegindateinputrow2 { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr[2]/td[1]//input", "Event Begin Date in grid input text field"); } }
        public static By grideventenddaterow2 { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr[2]/td[2]/span", "Event end Date in grid"); } }

        public static By grideventserviceproviderrow2 { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr[2]/td[3]/span", "Event service provider in grid"); } }

        public static By grideventtruckunitrow2 { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr[2]/td[4]/span", "Truck unit in grid"); } }
        public static By grideventcostcategoryrow2 { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr[2]/td[6]/span", "Cost category in grid"); } }

        public static By cloneventbutton { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Clone Events')]", "Clone Event grid"); } }
        public static By deleteeventbutton { get { return SeleniumActions.getByLocator("Xpath", "//button[text()=' Delete ']", "Delete Event grid"); } }



        //event row 2
        public static By grideventtype2 { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[2]//tr[2]/td[2]/span", "Event type in grid"); } }

        public static By grideventbegindate2 { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr[2]/td[1]/span", "Event Begin Date in grid"); } }

        public static By grideventenddate2 { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr[2]/td[2]/span", "Event end Date in grid"); } }

        public static By grideventserviceprovider2 { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr[2]/td[3]/span", "Event service provider in grid"); } }

        public static By grideventtruckunit2 { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr[2]/td[4]/span", "Truck unit in grid"); } }
        public static By grideventcostcategory2 { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[3]//tr[2]/td[6]/span", "Cost category in grid"); } }

        public static By acidizeevent { get { return SeleniumActions.getByLocator("Xpath", "//label[contains(text(),'Acidize')]", "Event type acidize"); } }

        public static By acidizecoiledevent { get { return SeleniumActions.getByLocator("Xpath", "//label[contains(text(),'Acidize - Coiled Tubing')]", "Event type acidize coiled"); } }
        public static By addbuttonformultievent { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Add Events')]", "Add button inside multievent dialog"); } }

        public static By serviceproviderdropdowninmultievent { get { return SeleniumActions.getByLocator("Xpath", "//select[@name='vendorname']", "Service provider drpdwn inside multievent dialog"); } }
        public static By serviceproviderdropdowninmultieventforjobplan { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-dropdownlist)[2]", "Service provider drpdwn inside multievent dialog for jobplan"); } }
        public static By truckuniteproviderdropdowninmultievent { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dropdownlist[@name='TruckUnitC']", "Truck unit drpdown inside multievent dialog"); } }
        public static By truckuniteproviderdropdowninsingleevent { get { return SeleniumActions.getByLocator("Xpath", "//kendo-combobox[@id='evcFK_r_TruckUnit']//input", "Truck unit drpdown inside single event dialog"); } }
        public static By costcatdropdowninmultievent { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dropdownlist[@name='catalog']", "Cost Cat drpdown inside multievent dialog"); } }

        public static By costcatdropdowninsingleevent { get { return SeleniumActions.getByLocator("Xpath", "//kendo-combobox[@id='evcFK_r_CatalogItem']//input", "Cost Cat drpdown inside single event dialog"); } }


        public static By rateinputinmultievent { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox//input", "Rate input inside multievent dialog"); } }

        public static By savebuttoninsidedialog { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog//button[contains(text(), 'Save')]", "Rate input inside multievent dialog"); } }

        public static By dateinputinsidedialog { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dateinput//input", "Begin date input inside multievent dialog"); } }

        public static By plusbutton { get { return SeleniumActions.getByLocator("Xpath", "//span[@class='glyphicon glyphicon-plus-sign']", "Plus tab"); } }

        public static By drillingtab { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Drilling')]", "Drilling tab"); } }
        public static By Wellboretab { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Wellbore')]", "Wellbore tab"); } }

        public static By jobplanoption { get { return SeleniumActions.getByLocator("Xpath", "//li[@class='ng-star-inserted']//a[contains(text(),'Job Plan')]", "JobPlanlistoption tab"); } }
        public static By jobcostoption { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Job Cost Details')]", "Jobcostlistoption tab"); } }
        public static By failurereportoption { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Failure Report')]", "Failure report tab"); } }

        public static By addbuttonjobcost { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(), 'Add')]", "Add button"); } }
        public static By addbuttonjobplan { get { return SeleniumActions.getByLocator("Xpath", "//span[text()='Add']", "Add button"); } }

        public static By eventypejobplan { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dropdownlist[@name='EventType']//span", "Eventtype "); } }

        public static By vendorname { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dropdownlist[@name='vendorname']//span", "vendor name "); } }

        public static By estimatedhours { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@name='estimatedHours']//input", "estimated hours"); } }
        public static By unitprice { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@name='unitPrice']//input", "unitprice"); } }
        public static By unitpriceedjobplan { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@name='unitPriceEdit']//input", "unitprice"); } }
        public static By unitpriceedit { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@name='unitPriceE']//input", "unitprice"); } }
        public static By totalcost { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@name='totalCost']//input", "totalCost"); } }
        public static By order { get { return SeleniumActions.getByLocator("Xpath", "//input[@name='sortOrder']", "sortorder"); } }
        public static By estimatedhoursedit { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@name='estimatedHoursE']//input", "estimated hours"); } }


        public static By totalcostedit { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@name='totalCostEdit']//input", "totalCost"); } }

        public static By savebuttonJobPlan { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dialog-actions//button[text()=' Save ']", "SaveButton"); } }

        public static By catalogitemjobplan { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dropdownlist[@name='catalog']//span", "Catalogitem"); } }

        public static By truckunitjobplan { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dropdownlist[@name='TruckUnitC']//span", "Truck Unit"); } }

        public static By jobplantypegrid { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[2]//tr/td[2]/span", "Job plan grid value"); } }
        public static By jobplanhoursgrid { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[2]//tr/td[3]/span", "Job plan grid value"); } }

        public static By jobplancostgrid { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[2]//tr/td[4]/span", "Job plan grid value"); } }

        public static By jobplanvendorgrid { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[2]//tr/td[5]/span", "Job plan grid value"); } }

        public static By jobplantruckunitgrid { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[2]//tr/td[6]/span", "Job plan grid value"); } }

        public static By jobplancataloggrid { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//table)[2]//tr/td[7]/span", "Job plan grid value"); } }

        public static By deletebin { get { return SeleniumActions.getByLocator("Xpath", "//span[@title='Delete']", "Delete bin icon button"); } }

        public static By editicon { get { return SeleniumActions.getByLocator("Xpath", "//span[@title='Update']", "Update icon button"); } }

        public static By confirmdeletejobplan { get { return SeleniumActions.getByLocator("Xpath", "//button[@class='k-button ng-star-inserted'][contains(text(),'Delete')]", "Delete confirm"); } }


        public static By vendorjobcost { get { return SeleniumActions.getByLocator("Xpath", "//Select[@name='sel']", "Vendor dropdown"); } }

        public static By catalogjobcost { get { return SeleniumActions.getByLocator("Xpath", "//Select[@name='catalog']", "Catalog dropdown"); } }

        public static By quantityjobcost { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@name='quantity']//input", "Quantoty input"); } }
        public static By quantityjobcostedit { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@name='quantityE']//input", "Quantity input"); } }

        public static By savebut { get { return SeleniumActions.getByLocator("Xpath", "//button[@class='btn btn-primary'][contains(text(),'Save')]", "Save Button"); } }


        public static By jobcostdetailstab { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Job Cost Details')]", "JobCOst Tab"); } }

        public static By eventtabo { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Events')]", "Event Tab"); } }

        public static By jobplantab { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Job Plan')]", "JobPlan Tab"); } }

        public static By confirmationdialogdeletejobcost { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Yes, Delete this Cost')]", "Delete confirmation"); } }
        public static By editiconjc { get { return SeleniumActions.getByLocator("Xpath", "//span[@class='k-icon k-i-pencil']", "Edit icon"); } }
        #endregion

        #region eventgrid
        public static By txtdurationhrs { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='evcDurationHours']//input", "Duration in add event"); } }
        public static By txttotalcost { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='evcTotalCost']//input", "Totalcost in add event"); } }
        public static By editeventclose { get { return SeleniumActions.getByLocator("Xpath", "//*[@class='k-icon k-i-x']", "close icon in edit event"); } }
        public static string eventediticon = "(//*[@title='Modify'])";


        public static IWebElement txttotalcostt { get { return SeleniumActions.getElement("Xpath", "//kendo-numerictextbox[@id='evcTotalCost']//input", "Totalcost in add event"); } }


        public static string eventgridhdr = "(//table)[4]/thead";
        public static By totalhrscost { get { return SeleniumActions.getByLocator("Xpath", "//kendo-panelbar-item/span", "Totalcost in add event"); } }

        public static By drillingreportoption { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Drilling')]", "Jobcostlistoption tab"); } }

        public static By wellborereportoption { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Wellbore')]", "Wellbore tab"); } }

        public static By createdrillreport { get { return SeleniumActions.getByLocator("Xpath", "//button[@aria-label='Create Report splitbutton']", "Create report button"); } }

        // public static By drillingreportdrpdwn { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dropdownlist/span", "Drilling report dropdowm"); } }

        public static By drillingreportdrpdwn { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='drilling-report-toolbar ng-star-inserted']//kendo-dropdownlist/span", "Drilling report - Time dropdown"); } }
        //public static By drillingreportactiveoption { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dropdownlist/span/span", "Drilling report active dropdown"); } }

        public static By drillingreportactiveoption { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='drilling-report-toolbar ng-star-inserted']/kendo-dropdownlist/span/span", "Drilling report active dropdown"); } }
        public static By drillingdate { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-dateinput)[1]//input", "Drilling report date"); } }

        public static By drillingtime { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-dateinput)[2]//input", "Drilling report time"); } }

        public static By drillireportcreatebutton { get { return SeleniumActions.getByLocator("Xpath", "//button[text()='Create']", "Drilling report Create"); } }
        //public static By splitbuttondownarrow { get { return SeleniumActions.getByLocator("Xpath", "//kendo-splitbutton//button[@class='k-button-icon k-button']", "Split button Down arrow "); } }

        public static By splitbuttondownarrow { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='drilling-report-toolbar ng-star-inserted']/kendo-splitbutton//button[@class='k-button-icon k-button']", "Split button Down arrow "); } }
        public static By deletelistsplit { get { return SeleniumActions.getByLocator("Xpath", "//li[contains(text(),'Delete Report')]", "Delete report "); } }
        //public static By splitbutton { get { return SeleniumActions.getByLocator("Xpath", "//kendo-splitbutton", "splitbutton"); } }

        public static By splitbutton { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='drilling-report-toolbar ng-star-inserted']//kendo-splitbutton", "splitbutton"); } }
        //public static By addbuttondrillingarea { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='headerButtonsToolbar drilling-rail']//button[text()=' Add ']", "Addbutton drilling area"); } }

        public static By addbuttondrillingarea { get { return SeleniumActions.getByLocator("Xpath", "//button[text()=' Add ']", "Addbutton drilling area"); } }
        public static By componentgrpcombobox { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-combobox)[1]//input", "Combo Box Component group"); } }
        public static By parttypecombobox { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-combobox)[2]//input", "Combo Box Component group"); } }
        public static By parttypecomboboxicon { get { return SeleniumActions.getByLocator("Xpath", "(//*[@class='wellboreComponent']//span[@class='k-select'])[2]", "Combo Box Component group"); } }

        public static By manufacturer { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='cmcFK_c_MfgCat_Manufacturers']//input", "Manufacturer"); } }
        public static By manufacturericon { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='cmcFK_c_MfgCat_Manufacturers']//span[@class='k-select']", "Manufacturer"); } }

        public static By parttypespecific { get { return SeleniumActions.getByLocator("Xpath", "//*[contains(text(),' Part Type Specific (24) ')]", "Parttypespecific"); } }
        public static By compctlgdes { get { return SeleniumActions.getByLocator("Xpath", "//kendo-combobox[@id='cmcFK_c_MfgCatalogItem']//input", "Catalog des in add comp"); } }
        public static By compctlgdesicon { get { return SeleniumActions.getByLocator("Xpath", "//kendo-combobox[@id='cmcFK_c_MfgCatalogItem']//*[@class='k-select']", "Catalog des in add comp"); } }

        public static By compname { get { return SeleniumActions.getByLocator("Xpath", "//input[@id='cmcCompName']", "Component Name"); } }
        public static By compquantity { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='ascQuantity']//input", "Component quantity"); } }
        public static By complength { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='ascLength']//input", "Component length"); } }
        public static By compdepth { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='ascTopDepth']//input", "Component Top  depth"); } }

        public static By compBottomdepth { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='ascBottomDepth']//input", "Component Bottom  depth"); } }
        public static By smallerbut { get { return SeleniumActions.getByLocator("Xpath", "//button[@class='btn btn-default smallerButton']", "Smaller but"); } }
        public static By wellperfstatus { get { return SeleniumActions.getByLocator("Xpath", "//*[@id='apsFK_r_WellPerforationStatus']//input", "Wellbore perforation status"); } }
        // public static By savebut2 { get { return SeleniumActions.getByLocator("Xpath", "//button[@class='btn btn-default ng-star-inserted'][contains(text(),'Save')]", "Save but"); } }
        public static By savebut2 { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='main-toolbar']//button[text()=' Save ']", "Save but"); } }

        public static By removebtn { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='main-toolbar']//button[text()=' Remove ']", "remove button"); } }

        public static By removebtn_jobcost { get { return SeleniumActions.getByLocator("Xpath", "//button[text()=' Remove ']", "remove button"); } }

        public static By editbtn { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='main-toolbar']//button[text()=' Edit ']", "edit button"); } }
        public static By editbtn_jobcost { get { return SeleniumActions.getByLocator("Xpath", "//button[text()=' Edit ']", "edit button"); } }

        public static By deletecomp { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Yes, Delete this Component')]", "Delete confirm"); } }
        public static By specifyjobtab { get { return SeleniumActions.getByLocator("Xpath", "//a[text()='Specify Job']", "Specify Job"); } }
        public static By specifytemtab { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Select Template')]", "Specify Template"); } }
        public static By selectjobtab { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Select Job')]", "Select Job Tab"); } }
        public static By jobtypeinjobplan { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-dropdownlist)[1]", "Job Type drowdown in Job Plan wizard"); } }
        public static By jobreasoninjobplan { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-dropdownlist)[2]", "Job Reason drowdown in Job Plan wizard"); } }
        public static By serviceprovinjobplan { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-dropdownlist)[4]", "Job service provider drowdown in Job Plan wizard"); } }
        public static By selectplannedeventtab { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Select Planned Event')]", "select planned event"); } }
        public static By economicanalysistabinjobplan { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Create Economic Analysis')]", "economic analysis tab"); } }
        public static By specifyjobdettabinjobplan { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Specify Job Details')]", "Job Details tab"); } }
        public static By jobidreturnjobplanwizard { get { return SeleniumActions.getByLocator("Xpath", "(//div[@class='modal-body ng-star-inserted']/p/strong)[1]", "Jobid string"); } }
        public static By viewcreatedjobbutton { get { return SeleniumActions.getByLocator("Xpath", "//button[text()=' View Created Job ']", "viewcreatedjob "); } }
        public static By aggridcolumn { get { return SeleniumActions.getByLocator("Xpath", "//span[text()='Columns']", "Grid column option "); } }
        public static By aggridcolumnallselectdeselect { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='ag-column-select-panel']//span[@class='ag-icon ag-icon-checkbox-unchecked'][1]", "Grid column select all "); } }
        public static By viewupdatedjobbutton { get { return SeleniumActions.getByLocator("Xpath", "//button[text()=' View Updated Job ']", "viewupdatedjob "); } }
        public static By templateRemedial { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Remedial - Acid Job']", "Remedial template "); } }
        public static By aggridwellnamereadyjobs { get { return SeleniumActions.getByLocator("Xpath", "((//div[@class='zero-pad-mar'])[2]//div[@col-id='WellName']/span/span/span/span)[2]", "Well name col in AG grid "); } }
        public static By aggridwellnameprosjobs { get { return SeleniumActions.getByLocator("Xpath", "((//div[@class='zero-pad-mar'])[1]//div[@col-id='WellName']/span/span/span/span)[2]", "Well name col in AG grid "); } }
        public static By aggridwellnameconjobs { get { return SeleniumActions.getByLocator("Xpath", "((//div[@class='zero-pad-mar'])[3]//div[@col-id='WellName']/span/span/span/span)[2]", "Well name col in AG grid "); } }
        public static By morningreportwellname { get { return SeleniumActions.getByLocator("Xpath", "(//table)[2]//tr[1]//td[1]/span", "Well name col in  grid "); } }
        public static By morningreportjobreason { get { return SeleniumActions.getByLocator("Xpath", "(//table)[2]//tr[1]//td[2]/span", "Job Reason col in  grid "); } }
        public static By morningreporteventtype { get { return SeleniumActions.getByLocator("Xpath", "(//table)[2]//tr[1]//td[3]/span", "Event type col in  grid "); } }
        public static By morningreportbegindate { get { return SeleniumActions.getByLocator("Xpath", "(//table)[2]//tr[1]//td[4]/span", "begindate date col in  grid "); } }
        public static By morningreportendate { get { return SeleniumActions.getByLocator("Xpath", "(//table)[2]//tr[1]//td[5]/span", "enddate date col in  grid "); } }
        public static By morningreportservprov { get { return SeleniumActions.getByLocator("Xpath", "(//table)[2]//tr[1]//td[6]/span", "Service provider col in  grid "); } }
        public static By panelhead { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='panel-heading']", "panelheading"); } }
        public static By toursheetdailyactivitytab { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Enter Daily Activity')]", "Daily Activity Tab"); } }
        public static By okbutton { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='modal-footer2']//button[@class='btn btn-default'][contains(text(),'OK')]", "Ok button"); } }
        public static By jobstatusoption { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(text(),'Job Status')]", "Job status"); } }
        public static By activerowaddobservationbutton { get { return SeleniumActions.getByLocator("Xpath", "(//kendo-grid-list//tbody/tr[contains(@class,'k-state-selected')])[2]//button/span[@title='Add Observation']", "Active row add observation button in faailure report screen"); } }
        public static By observationrow { get { return SeleniumActions.getByLocator("Xpath", "//tr[contains(@class,' hasObservations')]//td[2]/span", "HAs observation row"); } }

        public static By failuredateinput { get { return SeleniumActions.getByLocator("Xpath", "(//input)[8]", "Failure date input"); } }
        public static By submitbutton { get { return SeleniumActions.getByLocator("Xpath", "//button[text()='Submit']", "Submit Button"); } }
        public static string jobidprosjob { get { return "(//ag-grid-angular)[2]//div[@col-id='JobId']//span[@class='cell-layer-1']/a"; } }
        public static string jobidreadyjob { get { return "(//ag-grid-angular)[3]//div[@col-id='JobId']//span[@class='cell-layer-1']/a"; } }
        public static string jobidconcjob { get { return "(//ag-grid-angular)[4]//div[@col-id='JobId']//span[@class='cell-layer-1']/a"; } }
        public static string assetNameprosjob { get { return "//div[@col-id='AssetName']//span[@class='cell-layer-1']/span[2]"; } }
        public static string assetNamereadyjob { get { return "(//ag-grid-angular)[3]//div[@col-id='AssetName']//span[@class='cell-layer-1']/span[2]"; } }
        public static string assetNameconjob { get { return "(//ag-grid-angular)[4]//div[@col-id='AssetName']//span[@class='cell-layer-1']/span[2]"; } }
        public static string WellNameprosjobs { get { return "//div[@col-id='WellName']//span[@class='cell-layer-1']/span[2]"; } }
        public static string WellNamereadyjobs { get { return "(//ag-grid-angular)[3]//div[@col-id='WellName']//span[@class='cell-layer-1']/span[2]"; } }
        public static string WellNameconjobs { get { return "(//ag-grid-angular)[4]//div[@col-id='WellName']//span[@class='cell-layer-1']/span[2]"; } }
        public static string StatusIdprojobs { get { return "//div[@col-id='StatusId']//span[@class='cell-layer-1']/span[2]"; } }
        public static string StatusIdreadyjobs { get { return "(//ag-grid-angular)[3]//div[@col-id='StatusId']//span[@class='cell-layer-1']/span[2]"; } }
        public static string StatusIdconjobs { get { return "(//ag-grid-angular)[4]//div[@col-id='StatusId']//span[@class='cell-layer-1']/span[2]"; } }
        public static string JobTypeprosjobs { get { return "//div[@col-id='JobTypeId']//span[@class='cell-layer-1']/span[2]"; } }
        public static string JobTypereadyjobs { get { return "(//ag-grid-angular)[3]//div[@col-id='JobTypeId']//span[@class='cell-layer-1']/span[2]"; } }
        public static string JobTypeconjobs { get { return "(//ag-grid-angular)[4]//div[@col-id='JobTypeId']//span[@class='cell-layer-1']/span[2]"; } }
        public static string JobReasonprosjobs { get { return "//div[@col-id='JobReasonId']//span[@class='cell-layer-1']/span[2]"; } }
        public static string JobReasonreadyjobs { get { return "(//ag-grid-angular)[3]//div[@col-id='JobReasonId']//span[@class='cell-layer-1']/span[2]"; } }
        public static string JobReasonconjobs { get { return "(//ag-grid-angular)[4]//div[@col-id='JobReasonId']//span[@class='cell-layer-1']/span[2]"; } }
        public static string BeginDateJSprosjobs { get { return "//div[@col-id='BeginDateJS']//span[@class='cell-layer-1']/span[2]"; } }
        public static string BeginDateJSreadyjobs { get { return "(//ag-grid-angular)[3]//div[@col-id='BeginDateJS']//span[@class='cell-layer-1']/span[2]"; } }
        public static string BeginDateJSconjobs { get { return "(//ag-grid-angular)[4]//div[@col-id='BeginDateJS']//span[@class='cell-layer-1']/span[2]"; } }
        public static string EndDateJSprosjobs { get { return "//div[@col-id='EndDateJS']//span[@class='cell-layer-1']/span[2]"; } }
        public static string EndDateJSreadyjobs { get { return "(//ag-grid-angular)[3]//div[@col-id='EndDateJS']//span[@class='cell-layer-1']/span[2]"; } }
        public static string EndDateJSconjobs { get { return "(//ag-grid-angular)[4]//div[@col-id='EndDateJS']//span[@class='cell-layer-1']/span[2]"; } }
        public static string TotalCostprosjobs { get { return "//div[@col-id='TotalCost']//span[@class='cell-layer-1']/span[2]"; } }
        public static string TotalCostreadyjobs { get { return "(//ag-grid-angular)[3]//div[@col-id='TotalCost']//span[@class='cell-layer-1']/span[2]"; } }
        public static string TotalCostconjobs { get { return "(//ag-grid-angular)[4]//div[@col-id='TotalCost']//span[@class='cell-layer-1']/span[2]"; } }
        public static string EconomicAnalysisDPIprosjobs { get { return "//div[@col-id='EconomicAnalysisDPI']//span[@class='cell-layer-1']/span[2]"; } }
        public static string EconomicAnalysisDPIreadyjobs { get { return "(//ag-grid-angular)[3]//div[@col-id='EconomicAnalysisDPI']//span[@class='cell-layer-1']/span[2]"; } }
        public static string EconomicAnalysisDPIconjobs { get { return "(//ag-grid-angular)[4]//div[@col-id='EconomicAnalysisDPI']//span[@class='cell-layer-1']/span[2]"; } }
        public static string EconomicAnalysisNPVprosjobs { get { return "//div[@col-id='EconomicAnalysisNPV']//span[@class='cell-layer-1']/span[2]"; } }
        public static string EconomicAnalysisNPVreadyjobs { get { return "(//ag-grid-angular)[3]//div[@col-id='EconomicAnalysisNPV']//span[@class='cell-layer-1']/span[2]"; } }
        public static string EconomicAnalysisNPVconjobs { get { return "(//ag-grid-angular)[4]//div[@col-id='EconomicAnalysisNPV']//span[@class='cell-layer-1']/span[2]"; } }
        public static string AFEIdDescprosjobs { get { return "//div[@col-id='AFEIdDesc']//span[@class='cell-layer-1']/span[2]"; } }
        public static string AFEIdDescreadyjobs { get { return "(//ag-grid-angular)[3]//div[@col-id='AFEIdDesc']//span[@class='cell-layer-1']/span[2]"; } }
        public static string AFEIdDescconobs { get { return "(//ag-grid-angular)[4]//div[@col-id='AFEIdDesc']//span[@class='cell-layer-1']/span[2]"; } }
        public static string AccountRefprosjobs { get { return "//div[@col-id='AccountRef']//span[@class='cell-layer-1']/span[2]"; } }
        public static string AccountRefreadyjobs { get { return "(//ag-grid-angular)[3]//div[@col-id='AccountRef']//span[@class='cell-layer-1']/span[2]"; } }
        public static string AccountRefconjobs { get { return "(//ag-grid-angular)[4]//div[@col-id='AccountRef']//span[@class='cell-layer-1']/span[2]"; } }
        public static string BusinessOrganizationprosjobs { get { return "//div[@col-id='BusinessOrganizationControlIdText']//span[@class='cell-layer-1']/span[2]"; } }
        public static string BusinessOrganizationreadyjobs { get { return "(//ag-grid-angular)[3]//div[@col-id='BusinessOrganizationControlIdText']//span[@class='cell-layer-1']/span[2]"; } }
        public static string BusinessOrganizationconjobs { get { return "(//ag-grid-angular)[4]//div[@col-id='BusinessOrganizationControlIdText']//span[@class='cell-layer-1']/span[2]"; } }
        public static string ResponsiblePersonprosjobs { get { return "//div[@col-id='ResponsiblePerson']//span[@class='cell-layer-1']/span[2]"; } }
        public static string ResponsiblePersonreadyjobs { get { return "(//ag-grid-angular)[3]//div[@col-id='ResponsiblePerson']//span[@class='cell-layer-1']/span[2]"; } }
        public static string ResponsiblePersonconjobs { get { return "(//ag-grid-angular)[4]//div[@col-id='ResponsiblePerson']//span[@class='cell-layer-1']/span[2]"; } }
        public static string TruckUnitprosjobs { get { return "//div[@col-id='TruckUnitControlIdText']//span[@class='cell-layer-1']/span[2]"; } }
        public static string TruckUnitreadyjobs { get { return "(//ag-grid-angular)[3]//div[@col-id='TruckUnitControlIdText']//span[@class='cell-layer-1']/span[2]"; } }

        public static string JobPlanEstimatedDuration { get { return "*//div[@col-id='JobPlanEstimatedDuration']//span[@class='cell-layer-1']/span[2]"; } }
        public static string TruckUnitconjobs { get { return "(//ag-grid-angular)[4]//div[@col-id='TruckUnitControlIdText']//span[@class='cell-layer-1']/span[2]"; } }
        public static string PrimaryMotivationForJob { get { return "*//div[@col-id='JobDriverId']//span[@class='cell-layer-1']/span[2]"; } }

        public static string Remarksconcluded { get { return "*//div[@col-id='JobRemarks']//span[@class='cell-layer-1']/span[2]"; } }
        public static string PrimaryMotivationForJobprosjobs { get { return "//div[@col-id='JobDriverId']//span[@class='cell-layer-1']/span[2]"; } }
        public static string JobRemarksprosjobs { get { return "//div[@col-id='JobRemarks']//span[@class='cell-layer-1']/span[2]"; } }
        public static string approvpers { get { return "//div[@col-id='ApproverNames']//span[@class='cell-layer-1']/span[2]"; } }


        #endregion

        #region AGGRID

        #region Wellbore
        public static string compgroup { get { return "//div[@col-id='ComponentGroupName']//span[@class='cell-layer-1']/span[2]"; } }
        public static string parttypegridcell { get { return "//div[@col-id='PartType']//span[@class='cell-layer-1']/span[2]"; } }
        public static string componentnamegridcell { get { return "//div[@col-id='ComponentName']//span[@class='cell-layer-1']/span[2]"; } }
        public static string serialnumbergridcell { get { return "//div[@col-id='SerialNumber']//span[@class='cell-layer-1']/span[2]"; } }
        public static string perforationstatusgridcell { get { return "///div[@col-id='apsFK_r_WellPerforationStatus']//span[@class='cell-layer-1']/span[2]"; } }
        public static string installdategridcell { get { return "//div[@col-id='InstallDateJS']//span[@class='cell-layer-1']/span[2]"; } }
        public static string lengthgridcell { get { return "//div[@col-id='ascLength']//span[@class='cell-layer-1']/span[2]"; } }
        public static string topdepthgridcell { get { return "//div[@col-id='TopDepth']//span[@class='cell-layer-1']/span[2]"; } }
        public static string bottomdepthgridcell { get { return "//div[@col-id='BottomDepth']//span[@class='cell-layer-1']/span[2]"; } }
        public static string quantitygridcell { get { return "//div[@col-id='Quantity']//span[@class='cell-layer-1']/span[2]"; } }
        public static string componentorigingridcell { get { return "//div[@col-id='ComponentOrigin']//span[@class='cell-layer-1']/span[2]"; } }
        public static string innerdiametergridcell { get { return "//div[@col-id='InnerDiameter']//span[@class='cell-layer-1']/span[2]"; } }
        public static string outerdiametergridcell { get { return "//div[@col-id='OuterDiameter']//span[@class='cell-layer-1']/span[2]"; } }
        public static string remarksgridcell { get { return "//div[@col-id='ascRemark']//span[@class='cell-layer-1']/span[2]"; } }
        public static string permremarksgridcell { get { return "//div[@col-id='cmcRemark']//span[@class='cell-layer-1']/span[2]"; } }
        public static string errorsgridcell { get { return "//div[@col-id='ValidationErrorsTypeCount']//span[@class='cell-layer-1']/span[2]"; } }
        #endregion

        #region General
        public static string jobidgridcell { get { return "//div[@col-id='JobId']//span[@class='cell-layer-1']/span[2]"; } }
        public static string jobtypegridcell { get { return "//div[@col-id='JobTypeId']//span[@class='cell-layer-1']/span[2]"; } }
        public static string jobreasongridcell { get { return "//div[@col-id='JobReasonId']//span[@class='cell-layer-1']/span[2]"; } }
        public static string jobdrivergridcell { get { return "//div[@col-id='JobDriverId']//span[@class='cell-layer-1']/span[2]"; } }
        public static string jobstatusgridcell { get { return "//div[@col-id='StatusId']//span[@class='cell-layer-1']/span[2]"; } }
        public static string afegridcell { get { return "//div[@col-id='AFE']//span[@class='cell-layer-1']/span[2]"; } }
        public static string servprovgridcell { get { return "//div[@col-id='BusinessOrganization']//span[@class='cell-layer-1']/span[2]"; } }
        public static string actualcostgridcell { get { return "//div[@col-id='ActualCost']//span[@class='cell-layer-1']/span[2]"; } }
        public static string begindategridcell { get { return "//div[@col-id='BeginDateJS']//span[@class='cell-layer-1']/span[2]"; } }
        public static string enddategridcell { get { return "//div[@col-id='EndDateJS']//span[@class='cell-layer-1']/span[2]"; } }
        public static string actualjobdurationgridcell { get { return "//div[@col-id='ActualJobDurationDays']//span[@class='cell-layer-1']/span[2]"; } }
        public static string accountrefgridcell { get { return "//div[@col-id='AccountRef']//span[@class='cell-layer-1']/span[2]"; } }
        public static string jobplancostgridcell { get { return "//div[@col-id='JobPlanCost']//span[@class='cell-layer-1']/span[2]"; } }
        public static string totalcostgridcell { get { return "//div[@col-id='TotalCost']//span[@class='cell-layer-1']/span[2]"; } }
        public static string jobremarksgridcell { get { return "//div[@col-id='JobRemarks']//span[@class='cell-layer-1']/span[2]"; } }
        public static By jobidheader { get { return SeleniumActions.getByLocator("Xpath", "//div[@ref='eLabel']//span[text()='Job Id']", "JobId"); } }
        public static By begindateheader { get { return SeleniumActions.getByLocator("Xpath", "//div[@ref='eLabel']//span[text()='Begin Date']", "Begin Date Header"); } }
        public static By serviceprovheader { get { return SeleniumActions.getByLocator("Xpath", "//div[@ref='eLabel']//span[text()='Service Provider']", "Servcie provider Header"); } }
        public static By jobtypeheader { get { return SeleniumActions.getByLocator("Xpath", "//div[@ref='eLabel']//span[text()='Job Type']", "Job Type Header"); } }
        public static By assetNameheader { get { return SeleniumActions.getByLocator("Xpath", "//div[@ref='eLabel']//span[text()='Asset Name']", "Asset Name"); } }
        public static By filteroption { get { return SeleniumActions.getByLocator("Xpath", "//div[@ref='tabHeader']//span[@class='ag-icon ag-icon-filter']", "Filter"); } }
        public static By filtertext { get { return SeleniumActions.getByLocator("Xpath", "//input[@class='ag-filter-filter']", "FilterText"); } }
        public static By gridsearchbox { get { return SeleniumActions.getByLocator("Xpath", "//input[contains(@class,'quick-filter')]", "Search Box"); } }
        public static By beginDatecolumnsorticon { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='BeginDateJS']//span[@ref='eSortDesc']", "Begin Date"); } }
        public static By serviceproviderrighticon { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='BusinessOrganization']//span[@ref='eMenu']", "ServiceProv"); } }
        public static By jobtyperighticon { get { return SeleniumActions.getByLocator("Xpath", "//div[@col-id='JobType']//span[@ref='eMenu']", "JobType"); } }


        // public static string columnmenu(string columnindex) { return "(//span[@ref='eMenu'])["+columnindex+"]"; }
        // public static By sorticondesc(string columnindex) { string xpath = "(//span[@ref='eSortDesc'])[" + columnindex+"]"; return SeleniumActions.getByLocator("Xpath", xpath, ""); }
        // public static By sorticondesc(string columnindex) { string xpath = "(//div[contains(@class, 'grid-column')])[" + columnindex+ "]//span[@ref='eSortDesc']"; return SeleniumActions.getByLocator("Xpath", xpath, ""); }
        //public static By sorticonasc(string columnindex) { string xpath = "(//span[@ref='eSortAsc'])[" + columnindex + "]"; return SeleniumActions.getByLocator("Xpath", xpath, ""); }

        #endregion

        #endregion

    }
}
