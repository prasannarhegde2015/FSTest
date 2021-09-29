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
using System.IO;
using ForeSiteIsntall;
using System.Diagnostics;

namespace CygNet
{
    /// <summary>
    /// Summary description for ForeSite
    /// </summary>
    [CodedUITest]
    public class ForeSite
    {
        ApplicationUnderTest ForeSiteCleanInstalltion = new ApplicationUnderTest();
        public ForeSite()
        {
        }


        public void LaunchApplicationtestcase()
        {
            try
            {
                string path = ConfigurationManager.AppSettings.Get("buildpath");
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] files = dir.GetFiles("*.exe");
                Process p = new Process();
                p.StartInfo.FileName = files[0].FullName;
                p.Start();


            }
            catch (Exception ex)
            {
                Trace.WriteLine("LGot Error "+ex.ToString());
            }
        }

        public void ForeSiteScreen(string Server, string database)
        {

            try
            {
                string domain = ConfigurationManager.AppSettings.Get("domain");
                string sitename = ConfigurationManager.AppSettings.Get("sitename");
                string vhssite = ConfigurationManager.AppSettings.Get("vhssite");
                string sqlservername = ConfigurationManager.AppSettings.Get("sqlservername");
                string dbname = ConfigurationManager.AppSettings.Get("dbname");
                string licserver = ConfigurationManager.AppSettings.Get("licserver");
                int sleeptime = 3000;
                Trace.WriteLine("Looking for check box I agreee");
                Trace.WriteLine($"SQL server name:{sqlservername}");
                if (!PageObjects.FSWindow.Exists )
                {
                    Trace.WriteLine("No ForeSite Bundle Found");
                    return;
                }
                PageObjects.iagree.SetFocus();
                Playback.Wait(2000);
                Mouse.Click(PageObjects.iagree);

                Mouse.Click(PageObjects.next);
                Trace.WriteLine("Clcicked Next");
                Playback.Wait(sleeptime);
                Mouse.Click(PageObjects.foresiteclient);
                Mouse.Click(PageObjects.Foresiteserver);
                Mouse.Click(PageObjects.wellmodeling);
                Mouse.Click(PageObjects.dynacard);
                Mouse.Click(PageObjects.reocomm);
                Mouse.Click(PageObjects.reoserivce);
                Mouse.Click(PageObjects.catalogservice);
                Mouse.Click(PageObjects.next);
                Playback.Wait(sleeptime);
                Mouse.Click(PageObjects.next);
                Playback.Wait(sleeptime);
                PageObjects.sqlserver.Text = sqlservername;
                PageObjects.dbname.Text = dbname;
                Mouse.Click(PageObjects.testconnection);
                Mouse.Click(PageObjects.testconnection);
                for (int i = 0; i < 4; i++)
                {
                    Mouse.Click(PageObjects.next);
                    Playback.Wait(sleeptime);
                }
                PageObjects.licserver.Text = licserver;
                Mouse.Click(PageObjects.next);
                Playback.Wait(sleeptime);
                PageObjects.domain.Text = domain;
                PageObjects.site.Text = sitename;
                PageObjects.vhs.Text = vhssite;
                Mouse.Click(PageObjects.next);
                Playback.Wait(sleeptime);
                Mouse.Click(PageObjects.next);
                Playback.Wait(sleeptime);
                Mouse.Click(PageObjects.install);
                PageObjects.WaitTillClose();

            }
            catch (Exception  ex)
            {

                Trace.WriteLine("Generic Error" + ex);
               throw ex;
            }





        }
    }
}

