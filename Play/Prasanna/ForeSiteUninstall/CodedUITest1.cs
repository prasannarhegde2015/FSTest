using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System.Windows.Automation;

namespace ForeSiteUninstall
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class CodedUITest1
    {
        
        [TestCategory("MSUI"), TestMethod]
        public void ForeSiteUninstall()
        {
            Uninstall();
            Applicationtobeuninsatll();
            dotest();
        }
        public void Uninstall()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("appwiz.cpl");
            startInfo.UseShellExecute = true;
            Process.Start(startInfo);
       
        }

        public void Applicationtobeuninsatll()
        {
            WinWindow cwindow1 = new WinWindow();
            cwindow1.SearchProperties[WinWindow.PropertyNames.ControlType] = "Window";
            cwindow1.SearchProperties[WinWindow.PropertyNames.Name] = "Programs and Features";
            cwindow1.SearchProperties[WinWindow.PropertyNames.ClassName] = "CabinetWClass";
            cwindow1.DrawHighlight();
            cwindow1.Maximized = true;
            WinPane searchbox = new WinPane(cwindow1);
            searchbox.SearchProperties[WinPane.PropertyNames.ControlType] = "Pane";
            searchbox.SearchProperties[WinPane.PropertyNames.Name] = " Search Programs and Features";
            UITestControlCollection colofpane = searchbox.GetChildren();
            UITestControl trt = null;
            foreach(UITestControl ctl1searchbox in colofpane)
            {
                
                if (ctl1searchbox.Name == "Search Box")
                {
                    trt = ctl1searchbox;
                    break;
                }
                
            }
            WinEdit txtsearchbox = (WinEdit)trt;
            Mouse.Click(txtsearchbox);
            

            try
            {  
                    Playback.Wait(2000);
                    Keyboard.SendKeys("Weatherford ForeSite Bundle");
                    Playback.Wait(1000);
               
            }
            catch (Exception ex)
            {

            }
            WinList InstalledApplist = new WinList(cwindow1);
            InstalledApplist.SearchProperties[WinList.PropertyNames.ControlType] = "List";
            InstalledApplist.SearchProperties[WinList.PropertyNames.Name] = "Folder View";
            //  InstalledApplist.SearchProperties[WinList.PropertyNames.ClassName] = "SysListView32";


            if (InstalledApplist.Exists)
            {
                InstalledApplist.DrawHighlight();
                UITestControlCollection Listcount = InstalledApplist.Items;
                int n = Listcount.Count;
                for (int i = 0; i < n; i++)
                {
                    WinListItem Items = new WinListItem(InstalledApplist);

                    Items.SearchProperties[WinListItem.PropertyNames.ControlType] = "ListItem";
                    Items.DrawHighlight();
                    try
                    {
                        if (Items.Exists)
                        {
                            Mouse.Click(Items, MouseButtons.Right);
                            Keyboard.SendKeys("{DOWN}");
                            Keyboard.SendKeys("{ENTER}");



                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }

               

            }
        }

        public void dotest()
        {
           
            Trace.WriteLine(AutomationElement.RootElement.Current.ProcessId.ToString());
            AutomationElement root = AutomationElement.RootElement;
            AutomationElement bundlwwin = null;
            do
            {
                AutomationElementCollection allwins = root.FindAll(TreeScope.Descendants,
                     new System.Windows.Automation.PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Window));

                foreach (AutomationElement indelm in allwins)
                {
                    Trace.WriteLine(indelm.Current.Name);
                    if (indelm.Current.Name.Equals("Weatherford ForeSite Bundle"))
                    {
                        bundlwwin = indelm;
                        break;
                    }
                }
                Playback.Wait(1000);
            } while (bundlwwin == null);


            AutomationElement btnuninstall = null;

           AutomationElementCollection allbtns = bundlwwin.FindAll(TreeScope.Descendants,
               new System.Windows.Automation.PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Button));
            foreach (AutomationElement indelm in allbtns)
            {
                Trace.WriteLine(indelm.Current.Name);
                if (indelm.Current.Name.Equals("Uninstall All"))
                {
                    btnuninstall = indelm;
                    break;
                }
            }

            Point p = btnuninstall.GetClickablePoint();
            Mouse.Click(p);
            AutomationElement btnclose = null;
            do
            {
                allbtns = bundlwwin.FindAll(TreeScope.Descendants,
                  new System.Windows.Automation.PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Button));
                foreach (AutomationElement indelm in allbtns)
                {
                    Trace.WriteLine(indelm.Current.Name);
                    if (indelm.Current.Name.Equals("Close"))
                    {
                        btnclose = indelm;
                        break;
                    }
                }
                Playback.Wait(1000);
            } while (btnclose == null);
            p = btnclose.GetClickablePoint();
            Mouse.Click(p);
            Playback.Wait(5000);
            Keyboard.SendKeys("%{F4}");
            Playback.Wait(2000);

        }
        public UIMap UIMap
        {
            get
            {
                if ((this.map == null))
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        private UIMap map;
    }
}
