using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using MouseButtons = System.Windows.Forms.MouseButtons;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System.Configuration;

namespace CygNet
{
    /// <summary>
    /// Summary description for ForeSiteLaunch
    /// </summary>
    [CodedUITest]
    public class ForeSiteLaunch
    {
        ForeSite AutomationForsite = new ForeSite();
        public ForeSiteLaunch()
        {

        }

        [TestMethod]
        public void ForesiteMainMethod()
        {
            AutomationForsite.LaunchApplicationtestcase();
            string server = ConfigurationManager.AppSettings.Get("server");
            string dbname = ConfigurationManager.AppSettings.Get("dbname");
            string buildaction = ConfigurationManager.AppSettings.Get("buildaction");
            if (buildaction.ToLower().Equals("install"))
            {
                AutomationForsite.ForeSiteScreen(server, dbname);
            }
            else
            {
                AutomationForsite.ForeSiteScreenUpgrade(server, dbname);
            }
            
        }

        public void ForesiteMainMethodUIA()
        {
            AutomationForsite.LaunchApplicationtestcase();
            string server = ConfigurationManager.AppSettings.Get("server");
            string dbname = ConfigurationManager.AppSettings.Get("dbname");
            AutomationForsite.ForeSiteScreenUIA(server, dbname);
        }


    }
}