using ArtOfTest.WebAii.Controls.HtmlControls;
using ArtOfTest.WebAii.ObjectModel;
using ClientAutomation.TelerikCoreUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ClientAutomation.PageObjects
{
    class PageDashboard : PageMasterDefinition
    {

        public static string DynamicValue = null;
        public static Element configurationtab { get { return TelerikObject.GetElement("Content", "Configuration", "Configuration Tab"); } }

        public static Element pedashboardtab { get { return TelerikObject.GetElement("Content", "Production Dashboard", "Production Dashboard"); } }
        public static Element wellConfigurationtab { get { return TelerikObject.GetElement("Content", "Well Configuration", "Well Configuration Tab"); } }


        public static Element optimizationtab { get { return TelerikObject.GetElement("Content", "Optimization", "Optimization Tab"); } }

        public static Element trackingtab { get { return TelerikObject.GetElement("Content", "Field Services", "Field Services Tab"); } }

        public static Element Surveillancetab { get { return TelerikObject.GetElement("Content", "Surveillance", "Tracking Tab"); } }

        public static Element jobManagementTab { get { return TelerikObject.GetElement("Content", "Job Management", "Job management Tab"); } }

        public static Element jobStatusView { get { return TelerikObject.GetElement("Content", "Job Status View", "Job Status View"); } }
        public static Element welltesttab { get { return TelerikObject.GetElement("Content", "Well Test", "Well test Tab"); } }

        public static Element Analysistesttab { get { return TelerikObject.GetElement("Content", "Well Analysis", "AnalysisTab"); } }
        //
        public static HtmlControl cmb_WellSelect { get { return TelerikObject.GetElement("kendo-dropdownlist", "name", "wellSelection", "Well Selelction"); } }
        //    public static Element txt_wellSelect {  get { return TelerikObject.GetElementByIndex("Xpath", "//input[@class='k-textbox'][@role='listbox']", 0,"Txt WellSelelction"); } }
        ////span[@class='k-list-filter ng-star-inserted'] &&  //input[@class='k-textbox ng-pristine ng-valid ng-touched']
        public static Element txt_wellSelect { get { return TelerikObject.GetElementByIndex("Xpath", "//span[@class='k-list-filter ng-star-inserted']", 0, "Txt WellSelelction"); } }
        //'

        public static Element listitems_wellname { get { return TelerikObject.GetElement("Xpath", "//li[contains(text(),'" + DynamicValue + "')]", "Txt WellSelelction"); } }
        public static Element page_Loader { get { return TelerikObject.GetElement("Xpath", "//div[@class='loader']", "Page Loader"); } }
        public static HtmlControl WellSelectionDropDown { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "attribute", "class=k-dropdown-wrap k-state-default", "Well Selelction drop down"); } }
        public static HtmlControl WellCounter { get { return TelerikObject.GetElement(CONTROLTYPE_HTMLCONTROL, "attribute", "class=well-counter-label", "Well Counter Label"); } }
        //

    }

}
