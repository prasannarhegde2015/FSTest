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
                ForeSiteCleanInstalltion = ApplicationUnderTest.Launch(files[0].FullName);
                //CygNet.WaitForControlEnabled();
                Playback.Wait(6000);

            }
            catch (Exception ex)
            {

            }
        }

        public void foresitescreen(string Server, string database)
        {

            string domain = ConfigurationManager.AppSettings.Get("domain");
            string sitename = ConfigurationManager.AppSettings.Get("sitename");
            string vhssite = ConfigurationManager.AppSettings.Get("vhssite");
            
            if (PageObjects.iagree.Exists)
            {
                PageObjects.iagree.DrawHighlight();
                PageObjects.iagree.SetFocus();
                Playback.Wait(2000);
                Mouse.Click(PageObjects.iagree);
            }
            Mouse.Click(PageObjects.next);
            Playback.Wait(2000);
            WpfPane servciesection = new WpfPane(fosite);
            servciesection.SearchProperties[WpfPane.PropertyNames.ControlType] = "Pane";
            servciesection.SearchProperties[WpfPane.PropertyNames.ClassName] = "Uia.ScrollViewer";
            servciesection.DrawHighlight();
            Playback.Wait(2000);

            WpfCheckBox fiagree = new WpfCheckBox(servciesection);
            fiagree.SearchProperties[WpfCheckBox.PropertyNames.ControlType] = "CheckBox";
            fiagree.SearchProperties[WpfCheckBox.PropertyNames.AutomationId] = "ForeSiteClientCheck";
            fiagree.TechnologyName = "UIA";

            try
            {
                if (fiagree.Exists)
                {
                    fiagree.DrawHighlight();
                    fiagree.SetFocus();
                    Mouse.Click(fiagree);
                    Playback.Wait(2000);
                }
            }
            catch (Exception ex)
            {

            }
            WpfCheckBox fsiagree = new WpfCheckBox(servciesection);
            fsiagree.SearchProperties[WpfCheckBox.PropertyNames.ControlType] = "CheckBox";
            fsiagree.SearchProperties[WpfCheckBox.PropertyNames.AutomationId] = "ForeSiteServerCheck";
            try
            {
                if (fsiagree.Exists)
                {
                    fsiagree.DrawHighlight();
                    fsiagree.SetFocus();
                    Mouse.Click(fsiagree);
                    Playback.Wait(2000);
                }
            }
            catch (Exception ex)
            {

            }
            WpfCheckBox WAMICheck = new WpfCheckBox(servciesection);
            WAMICheck.SearchProperties[WpfCheckBox.PropertyNames.ControlType] = "CheckBox";
            WAMICheck.SearchProperties[WpfCheckBox.PropertyNames.AutomationId] = "WAMICheck";
            try
            {
                if (WAMICheck.Exists)
                {
                    WAMICheck.DrawHighlight();
                    WAMICheck.SetFocus();
                    Mouse.Click(WAMICheck);
                    Playback.Wait(2000);
                }
            }
            catch (Exception ex)
            {

            }
            WpfCheckBox DynaCardCheck = new WpfCheckBox(servciesection);
            DynaCardCheck.SearchProperties[WpfCheckBox.PropertyNames.ControlType] = "CheckBox";
            DynaCardCheck.SearchProperties[WpfCheckBox.PropertyNames.AutomationId] = "DynaCardCheck";
            try
            {
                if (DynaCardCheck.Exists)
                {
                    DynaCardCheck.DrawHighlight();
                    Playback.Wait(2000);
                    DynaCardCheck.SetFocus();
                    Mouse.Click(DynaCardCheck);

                    Playback.Wait(2000);
                }
            }
            catch (Exception ex)
            {

            }
            WpfCheckBox CataLogCheck = new WpfCheckBox(servciesection);
            CataLogCheck.SearchProperties[WpfCheckBox.PropertyNames.ControlType] = "CheckBox";
            CataLogCheck.SearchProperties[WpfCheckBox.PropertyNames.AutomationId] = "CataLogCheck";
            try
            {
                if (CataLogCheck.Exists)
                {
                    CataLogCheck.DrawHighlight();
                    CataLogCheck.SetFocus();
                    Mouse.Click(CataLogCheck);

                    Playback.Wait(2000);
                }
            }
            catch (Exception ex)
            {

            }
            WpfCheckBox ReOCheck = new WpfCheckBox(servciesection);
            ReOCheck.SearchProperties[WpfCheckBox.PropertyNames.ControlType] = "CheckBox";
            ReOCheck.SearchProperties[WpfCheckBox.PropertyNames.AutomationId] = "ReOCheck";
            try
            {
                if (ReOCheck.Exists)
                {
                    ReOCheck.DrawHighlight();
                    ReOCheck.SetFocus();
                    Mouse.Click(ReOCheck);
                    Playback.Wait(2000);
                }
            }
            catch (Exception ex)
            {

            }
            WpfCheckBox ReOCOMCheck = new WpfCheckBox(servciesection);
            ReOCOMCheck.SearchProperties[WpfCheckBox.PropertyNames.ControlType] = "CheckBox";
            ReOCOMCheck.SearchProperties[WpfCheckBox.PropertyNames.AutomationId] = "ReOCOMCheck";
            try
            {
                if (ReOCOMCheck.Exists)
                {

                    ReOCOMCheck.DrawHighlight();
                    ReOCOMCheck.SetFocus();
                    Mouse.Click(ReOCOMCheck);
                    Playback.Wait(2000);
                }
            }
            catch
            {

            }
            WpfButton next1 = new WpfButton(servciesection);
            next1.SearchProperties[WpfButton.PropertyNames.ControlType] = "Button";
            next1.SearchProperties[WpfButton.PropertyNames.Name] = "Next";

            if (next1.Exists)
            {
                next1.DrawHighlight();
                Mouse.Click(next1);
            }

            WpfRadioButton SelectionPage = new WpfRadioButton();
            SelectionPage.SearchProperties[WpfRadioButton.PropertyNames.ControlType] = "RadioButton";
            SelectionPage.SearchProperties[WpfRadioButton.PropertyNames.AutomationId] = "SQL_Check";
            SelectionPage.DrawHighlight();
            try
            {
                if (SelectionPage.Exists)
                {
                    SelectionPage.SetFocus();
                    Mouse.Click(SelectionPage);
                }
            }
            catch (Exception ex)
            {

            }
            Playback.Wait(2000);
            WpfButton next2 = new WpfButton(SelectionPage);
            next2.SearchProperties[WpfButton.PropertyNames.ControlType] = "Button";
            next2.SearchProperties[WpfButton.PropertyNames.Name] = "Next";
            if (next2.Exists)
            {
                next2.DrawHighlight();
                Mouse.Click(next2);

            }

            WpfText server = new WpfText();
            server.SearchProperties[WpfText.PropertyNames.ControlType] = "Text";
            server.SearchProperties[WpfText.PropertyNames.Name] = "Server name:";
            server.DrawHighlight();
            try
            {
                if (server.Exists)
                {
                    Mouse.Click(server);
                    Playback.Wait(1000);

                    Keyboard.SendKeys(Server);
                }
            }
            catch (Exception ex)
            {

            }
            WpfText datab = new WpfText();
            datab.SearchProperties[WpfText.PropertyNames.ControlType] = "Text";
            datab.SearchProperties[WpfText.PropertyNames.Name] = "Enter a database:";
            datab.DrawHighlight();
            try
            {
                if (datab.Exists)
                {
                    Mouse.Click(datab);
                    Playback.Wait(1000);

                    Keyboard.SendKeys(database);
                }
            }
            catch (Exception ex)
            {

            }

            WpfButton testconn = new WpfButton();
            testconn.SearchProperties[WpfButton.PropertyNames.ControlType] = "Button";
            testconn.SearchProperties[WpfButton.PropertyNames.Name] = "Test Connection";
            testconn.DrawHighlight();
            try
            {
                if (testconn.Exists)
                {
                    Mouse.Click(testconn);
                }
            }
            catch (Exception ex)
            {

            }

            WpfButton NextButton = new WpfButton();
            NextButton.SearchProperties[WpfButton.PropertyNames.ControlType] = "Button";
            NextButton.SearchProperties[WpfButton.PropertyNames.Name] = "Next";
            if (NextButton.Exists)
            {
                NextButton.DrawHighlight();
                Mouse.Click(NextButton);

            }

        }

    }
}

