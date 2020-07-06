using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelerikTest;
using System.Configuration;
using System.Diagnostics;

namespace ClientAutomation.TelerikCoreUtils
{
    public static class CommonActions
    {
        public static void select_default_well()
        {
            string wellname = ConfigurationManager.AppSettings["Wellname"];
            TelerikObject.waitforInvisible(Page_Dashboard.page_Loader);

            TelerikObject.Click(Page_Dashboard.cmb_WellSelect);
            Thread.Sleep(2000);
            TelerikObject.AutoDomrefresh();
            Thread.Sleep(2000);
            TelerikObject.Click(Page_Dashboard.txt_wellSelect);
            //   TelerikObject.Sendkeys(Page_Dashboard.txt_wellSelect, wellname);
            System.Windows.Forms.SendKeys.SendWait(wellname);
            Page_Dashboard.DynamicValue = wellname;
            if (TelerikObject.isWellPresentInDB(wellname) == false)
            {
                Trace.WriteLine("Well To select is NOT present in databse Aborting test");
                return;
            }
            else
            {
                Trace.WriteLine("Well To select is present in databse Confirmed......");
            }
            TelerikObject.Click(Page_Dashboard.listitems_wellname);
        }

       public static void createWellGeneralonly()
        {
            TelerikObject.Click(Page_Dashboard.configurationtab);
            TelerikObject.Click(Page_Dashboard.wellConfigurationtab);
            TelerikObject.Click(Page_WellConfig.btn_createNewWell);
            TelerikObject.createwellgeneraltab();
        }
}
}
