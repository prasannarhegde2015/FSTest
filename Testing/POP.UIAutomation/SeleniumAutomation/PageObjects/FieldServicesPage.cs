using OpenQA.Selenium;
using Protractor;
using SeleniumAutomation.SeleniumObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumAutomation.PageObjects
{
    class FieldServicesPage
    {

        public static int iDynamicIndex = -1;
        public static int iDynamicValue = -1;

        public static By Toaseter { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(@class,'toast-message')]", "Toast"); } }

        public static By fieldServicesTab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Field Services']", " Field Services Tab"); } }
        public static By jobManagementTab { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Job Management']", " Job Management Tab"); } }
        public static By btnAddJob { get { return SeleniumActions.getByLocator("Xpath", "//span[contains(text(),'Add')]//parent::button", " Add Job Button"); } }

        public static By btnDeleteJob { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Update')]//following-sibling::button", "Delete Job Button"); } }

        public static By btnUpdateJob { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Update')]", "Update Job Button"); } }
        // --  //button[contains(text(),'Add')]
        public static By btnAddJobCostDetails { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Add')]", " Add Job Cost Details Button"); } }
        public static By tabAddEvent { get { return SeleniumActions.getByLocator("Xpath", "//li[@id='tabList']/descendant::div/a[text()='Events']", " Add Events Tab"); } }

        public static By tabAddJobCostDetails { get { return SeleniumActions.getByLocator("Xpath", "//li[@id='tabList']/descendant::div/a[text()='Job Cost Details']", " Job cost details Tab"); } }

        //--  //button[@class='btn btn-default smallerButton']
        public static By kendosearchbarEventType { get { return SeleniumActions.getByLocator("Xpath", "//kendo-searchbar/input", "Events Type Searchbar"); } }

        public static By txtEvtdurationHours { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@id='evcDurationHours']/descendant::input", "Duration Hours"); } }

        public static By txtEvtBegindate { get { return SeleniumActions.getByLocator("Xpath", "//kendo-datepicker[@id='evcEventBegDtTm']/descendant::input", " Event Begin Date"); } }
        public static By txtEvtEnddate { get { return SeleniumActions.getByLocator("Xpath", "//kendo-datepicker[@id='evcEventEndDtTm']/descendant::input", "Event End Date"); } }

        public static By ddkendoServiceProviderEvnt { get { return SeleniumActions.getByLocator("Xpath", "//kendo-combobox[@id='evcFK_BusinessOrganization']/descendant::input", "Service Provide"); } }


        public static By btnAddSingleEventSave { get { return SeleniumActions.getByLocator("Xpath", "//button[@class='btn btn-default smallerButton']", "Save Single Event button"); } }
        public static By lnkAddJob { get { return SeleniumActions.getByLocator("Xpath", "//li[text()=' Add  ']", " Add Job Link"); } }



        public static By ddKendoJobType { get { return SeleniumActions.getByLocator("Xpath", "//span[contains(text(),'Select Job Type')]", " Job Type"); } }
        public static By ddKendoJobTypeby { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dropdownlist[@name='jobType']", " Job Type"); } }

        public static By ddKendoJobReason { get { return SeleniumActions.getByLocator("Xpath", "//span[text()='Select Job Reason']", " Select Job Reason"); } }

        public static By ddKendoJobDriver { get { return SeleniumActions.getByLocator("Xpath", "//span[contains(text(),'Select Job Driver')]", " Select Job Driver"); } }

        public static By ddKendoJobStatus { get { return SeleniumActions.getByLocator("Xpath", "//span[text()='Select Status']", " Select Status"); } }

        public static By ddKendoServieceProvider { get { return SeleniumActions.getByLocator("Xpath", "//span[text()='Select Service Provider']", " Select Service Providern"); } }

        public static By txtJobStartDate { get { return SeleniumActions.getByLocator("Xpath", "//label[text()=' Begin Date']/following-sibling::div/kendo-datepicker/descendant::input", " Begin Date"); } }
        public static By txtJobEndDate { get { return SeleniumActions.getByLocator("Xpath", "//label[text()=' End Date']/following-sibling::div/kendo-datepicker/descendant::input", " End Date"); } }
        public static By btnJobSave { get { return SeleniumActions.getByLocator("Xpath", "//button[text()=' Save ' and @class='btn btn-default smallerButton ng-star-inserted']", " Job Save button"); } }

        public static By btnAttcahment0 { get { return SeleniumActions.getByLocator("Xpath", "//span[text()='Manage Files']", " Job Attachment button"); } }

        public static By btnDeleteAttcahment { get { return SeleniumActions.getByLocator("Xpath", "//button[contains(text(),'Download')]//following-sibling::button", " Delete Attachment button"); } }

        public static System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> documentManager_Categories { get { return SeleniumActions.CollectionOfControls("Xpath", "//div[@class='folderNames']", "Collection of Attachment Folders"); } }

        public static By documentManager_CategoriesBy { get { return SeleniumActions.getElementByIndexXpath("Xpath", "//div[@class='folderNames']", iDynamicIndex, "Collection of Attachment Folders"); } }


        //  public static System.Collections.ObjectModel.ReadOnlyCollection<By> documentManager_CategoriesBy { get { return SeleniumActions.CollectionOfControls("Xpath", "//div[@class='folderNames']", "Collection of Attachment Folders"); } }

        public static System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> btns_AddFile { get { return SeleniumActions.CollectionOfControls("Xpath", "//span[text()='Add File']", "Add File Button Collection"); } }
        public static System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> checkboxs_FileSelection { get { return SeleniumActions.CollectionOfControls("Xpath", "//div[@class='uploadbarSection']//input[@type='checkbox']", "File Selection checkboxs in Document Manager Model"); } }


        public static By btns_AddFileBy { get { return SeleniumActions.getElementByIndexXpath("Xpath", "//span[text()='Add File']/preceding-sibling::*", iDynamicIndex, "Add File Button Collection"); } }

        public static By btns_AddFileByNS { get { return SeleniumActions.getElementByIndexXpathNextSibling("Xpath", "//span[text()='Add File']/preceding-sibling::*", iDynamicIndex, "Add File Button Collection"); } }

        public static By btnFileUploadClose { get { return SeleniumActions.getByLocator("Xpath", "//a[contains(@class,'k-dialog-close')]", "Close Attachment dialog Button"); } }

        public static By btn_AddTabBy { get { return SeleniumActions.getByLocator("Xpath", "//li[@id='wsmAddTab-dropdown']", "FSM Add Tab Button"); } }

        public static By wellboreTabBy { get { return SeleniumActions.getByLocator("Xpath", "//a[text()='Wellbore']", "Wellbore Tab Button"); } }

        public static By btn_componentAdd { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='wellbore-container-inner']//button[text()=' Add ']", "Add button on Wellbore tab"); } }

        public static By lnk_AddJobCostdetails { get { return SeleniumActions.getByLocator("Xpath", "//a[text()='Job Cost Details']", "Add Job Cost Details Link"); } }

        public static By btn_componentQuickAdd { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='wellbore-container-inner']//button[text()=' Quick Add ']", "Quick Add button on Wellbore tab"); } }

        #region JobCostDetails
        // --  //kendo-datepicker[@name='costDate']/span/kendo-dateinput/span/input totalCost
        public static By txt_jobcostDate { get { return SeleniumActions.getByLocator("Xpath", "//kendo-datepicker[@name='costDate']/span/kendo-dateinput/span/input", "Cost Dat"); } }
        public static By ddVendor { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Vendor']/../following-sibling::div/select", "Vendor Dropdown"); } }
        public static By ddCatalogItem { get { return SeleniumActions.getByLocator("Xpath", "//label[text()='Catalog Item']/../following-sibling::div/select", "Catalog Item Dropdown"); } }
        // --quantity --unitPrice
        public static By txt_KendoNumQuality { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@name='quantity']//input", "Quantity"); } }
        public static By txt_KendoNumUnitPrice { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@name='unitPrice']//input", "Unit Price"); } }
        public static By txt_KendoNumDiscountAmount { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@name='discount']//input", "Discount Amount"); } }
        public static By txt_KendoNumCost { get { return SeleniumActions.getByLocator("Xpath", "//kendo-numerictextbox[@name='totalCost']//input", "Total Cost"); } }
        public static By txt_Remarks { get { return SeleniumActions.getByLocator("Xpath", "//textarea[@name='Remarks']", "Remarks Textbox "); } }
        // //button[text()='Save']

        public static By btn_SaveJobCostDetails { get { return SeleniumActions.getByLocator("Xpath", "//button[text()='Save']", "Save button Add Job Cost detail"); } }

        #endregion
    }
}
