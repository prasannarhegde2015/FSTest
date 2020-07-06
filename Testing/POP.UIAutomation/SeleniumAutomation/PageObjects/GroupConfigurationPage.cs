using OpenQA.Selenium;
using SeleniumAutomation.SeleniumObject;

namespace SeleniumAutomation.PageObjects
{
    class GroupConfigurationPage
    {

        public static string welltypename = "";

        public static int gridnumber = 0;

        public static By welltypetab { get { return SeleniumActions.getByLocator("Xpath", "//span[text()='" + welltypename + "']", " Well types tab"); } }
        public static By scrollhorizontal { get { return SeleniumActions.getByLocator("Xpath", "(//div[@ref='eBodyHorizontalScrollViewport'])[2]", " Scrollbar"); } }

        public static By scrollhorizontalcontainer { get { return SeleniumActions.getByLocator("Xpath", "(//div[@ref='eBodyHorizontalScrollContainer'])[2]", " Scrollbar container"); } }

        public static By perforation { get { return SeleniumActions.getByLocator("Xpath", "(//div[@col-id='Well.IntervalAPI'])[3]/span/span/span/span[2]", " perfora"); } }

        public static By tabGroupConfiguration { get { return SeleniumActions.getByLocator("Xpath", "//div[text()='Group Configuration']", "Group configuration Tab"); } }

        public static By cellGrid { get { return SeleniumActions.getByLocator("Xpath", "(//generic-grid)[" + gridnumber.ToString() + "]//ag-grid-angular//div[@col-id='Well.AssemblyAPI']/span/span/span/span[@class='fas fa-lock cell-locked-icon']", "Cell space in grid"); } }

    }
}
