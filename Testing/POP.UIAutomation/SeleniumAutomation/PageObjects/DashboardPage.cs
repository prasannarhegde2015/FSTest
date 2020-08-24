using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using SeleniumAutomation.SeleniumObject;

namespace SeleniumAutomation.PageObjects
{
    class DashboardPage
    {

        //   public static IWebElement frmNavFrame { get { return SeleniumActions.getElement("Xpath", "//iframe[@id='gsft_main']", "navFrame"); } }
        public static By tabDashboards { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Dashboards']", "Dashboards Tab"); } }
        public static By tabProdDashboard { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Production Dashboard']", "Production Dashboard Tab"); } }
        public static By tabSurveillace { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Surveillance']", "Surveillance Tab"); } }

        public static By tabWellStatus { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Well Status']", "Well Status Tab"); } }
        public static string Dynatext = "";
        public static By frmNavFrame { get { return SeleniumActions.getByLocator("Xpath", "//iframe[@id='gsft_main']", "navFrame"); } }
        public static By tabOptimization { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Optimization']", "Optimization"); } }
        public static By tabAnalysis { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Well Analysis']", "Well Analysis"); } }

        public static By tabWellTest { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Well Test']", "Well Test"); } }
        // //label[@class='well-counter-label']
        public static By lblWelCount { get { return SeleniumActions.getByLocator("Xpath", "//label[@id='lblWellCount']", "Well Count"); } }
        //span[text()='BJ_PGL_Well_00003']
        public static By lstWellName { get { return SeleniumActions.getByLocator("Xpath", "(//span[text()='" + Dynatext + "'])[2]", "Well List with well name" + Dynatext); } }
        public static By frmSpotFireLogin { get { return SeleniumActions.getByLocator("Xpath", "//iframe[@id='spotfire-auth']", "SpotFire Frame"); } }
        public static By btnSpotFireLogin { get { return SeleniumActions.getByLocator("Xpath", "//div[@id='launch_spotfire_login']", "SpotFire Login button"); } }
    }
}
