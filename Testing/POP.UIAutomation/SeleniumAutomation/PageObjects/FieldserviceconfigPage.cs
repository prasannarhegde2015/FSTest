using OpenQA.Selenium;
using SeleniumAutomation.SeleniumObject;

namespace SeleniumAutomation.PageObjects
{
    class FieldserviceconfigPage
    {

        public static By jobtype { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dropdownlist[@name='jobType']", " Job Type Dropdown"); } }
        public static By categoryinput { get { return SeleniumActions.getByLocator("Xpath", "//kendo-combobox[@id='category']//input", "Category Input"); } }
        public static By addnewtempbut { get { return SeleniumActions.getByLocator("Xpath", "//button[@class='templateJobAddButton']", "Add template job Tab"); } }
        public static By referencedatatab { get { return SeleniumActions.getByLocator("Xpath", "//span[contains(text(),'Reference Data')]", "Reference Data Tab"); } }
        public static By referencetableheading { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='jobCategoryHeader']", "Reference Data header"); } }
        public static By jobstatusoption { get { return SeleniumActions.getByLocator("Xpath", "(//div[@class='ps-content'])[2]/div/div[contains(text(),'Job Status')]", "Jobstatus connfig Tab"); } }
        public static By editiconforcancel { get { return SeleniumActions.getByLocator("Xpath", "//tr//td//span[@title='Cancelled']/preceding::span[@title='Modify'][09]", "edit icon"); } }
        public static By concludedjobradio { get { return SeleniumActions.getByLocator("Xpath", "//label[contains(text(),'Concluded Jobs')]/preceding-sibling::input", "radion icon"); } }
        public static By readyjobradio { get { return SeleniumActions.getByLocator("Xpath", "//label[contains(text(),'Ready Jobs')]/preceding-sibling::input", "radion icon"); } }

        public static By fieldServicesconfigTab { get { return SeleniumActions.getByLocator("Xpath", "//div[contains(text(),'Field Service Configuration')]", " Field Services config Tab"); } }

        public static By Templatejobheaderlabel { get { return SeleniumActions.getByLocator("Xpath", "//div[@class='headerTitleTemplate']", " Header Template"); } }

        public static By jobreason { get { return SeleniumActions.getByLocator("Xpath", "//kendo-dropdownlist[@id='jobReason']", " Job Reason"); } }

        public static By savebutton { get { return SeleniumActions.getByLocator("Xpath", "//button//span[text()='Save']", "Save"); } }



    }
}
