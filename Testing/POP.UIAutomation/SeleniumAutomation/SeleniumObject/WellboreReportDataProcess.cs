using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using SeleniumAutomation.AGGridScreenDatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SeleniumAutomation.SeleniumObject
{
    class WellboreReportDataProcess : IGridScenarios<TypeWellboreReportGrid>
    {
        double height;
        double width;
        public string scrollp;
        public int scrollsize;
        public WellboreReportDataProcess()
        {
            height = SystemParameters.PrimaryScreenHeight;
            width = SystemParameters.PrimaryScreenWidth;

            Console.Write("Screen size found width:-" + width);
            if (width <= 1366)
            {
                double half = width / 2;
                scrollp = half.ToString();
            }
            if (width >= 1600)
            {
                double half = width / 2;
                scrollp = half.ToString();
            }

        }
        public TypeWellboreReportGrid Colorcode(string columnname, string comparison, string value, int hex, string target)
        {
            throw new NotImplementedException();
        }

        public TypeWellboreReportGrid CreateNewView(string viewname)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fetch all the attribute values from Wellbore report grid 
        /// </summary>
        /// <returns>Returns TypeWellboreReportGeneralGrid properties, which specifies attributes of datas of well found in grid</returns>
        /*
                         * Get all the elemnts for individual columns                         * 
                         * Assign each attribute to properties
                         * return the class array
        */
        public TypeWellboreReportGrid[] FetchdataScreen()
        {

            string style = SeleniumActions.getAttribute(PageObjects.JobManagementPage.scrollhorizontalcontainerwellborereport, "style");
            string[] splitstyle = style.Split(':');
            string[] splitstyle2 = splitstyle[1].Split(';');
            Console.WriteLine("style1->" + splitstyle[1]);
            Console.WriteLine("style->" + splitstyle2[0]);
            scrollsize = int.Parse(splitstyle2[0].Substring(0, splitstyle2[0].Length - 2));
            Console.WriteLine("szie->" + scrollsize);
            //scrollsize = Convert.ToDouble(splitstyle2[0].Trim());   
            //   List<string> subassembly = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='SubAssemblyDescription']//span[@class='cell-layer-1']/span[2]");
            List<string> componentgroup = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='ComponentGroupName']//span[@class='cell-layer-1']/span[2]");
            double i = Convert.ToDouble(scrollp);
            while (componentgroup == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                componentgroup = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='ComponentGroupName']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> PartType = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='PartType']//span[@class='cell-layer-1']/span[2]");

            while (PartType == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                PartType = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='PartType']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> ComponentName = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='ComponentName']//span[@class='cell-layer-1']/span[2]");

            while (ComponentName == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                ComponentName = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='ComponentName']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> SerialNumber = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='SerialNumber']//span[@class='cell-layer-1']/span[2]");

            while (SerialNumber == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                SerialNumber = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='SerialNumber']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> WellPerforationStatus = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='apsFK_r_WellPerforationStatus']//span[@class='cell-layer-1']/span[2]");

            while (WellPerforationStatus == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                WellPerforationStatus = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='apsFK_r_WellPerforationStatus']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> InstallDateJS = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='InstallDateJS']//span[@class='cell-layer-1']/span[2]");

            while (InstallDateJS == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                InstallDateJS = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='InstallDateJS']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> ascLength = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='ascLength']//span[@class='cell-layer-1']/span[2]");

            while (ascLength == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                ascLength = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='ascLength']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> TopDepth = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='TopDepth']//span[@class='cell-layer-1']/span[2]");

            while (TopDepth == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                TopDepth = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='TopDepth']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> BottomDepth = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='BottomDepth']//span[@class='cell-layer-1']/span[2]");

            while (BottomDepth == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                BottomDepth = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='BottomDepth']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> Quantity = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='Quantity']//span[@class='cell-layer-1']/span[2]");

            while (Quantity == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                Quantity = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='Quantity']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> ComponentOrigin = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='ComponentOrigin']//span[@class='cell-layer-1']/span[2]");

            while (ComponentOrigin == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                ComponentOrigin = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='ComponentOrigin']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> InnerDiameter = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='InnerDiameter']//span[@class='cell-layer-1']/span[2]");

            while (InnerDiameter == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                InnerDiameter = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='InnerDiameter']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> OuterDiameter = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='OuterDiameter']//span[@class='cell-layer-1']/span[2]");

            while (OuterDiameter == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                OuterDiameter = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='OuterDiameter']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> Remarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='ascRemark']//span[@class='cell-layer-1']/span[2]");

            while (Remarks == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                Remarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='ascRemark']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> PermanentRemarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='cmcRemark']//span[@class='cell-layer-1']/span[2]");

            while (PermanentRemarks == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                PermanentRemarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='cmcRemark']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> Errors = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='ValidationErrorsTypeCount']//span[@class='cell-layer-1']/span[2]");

            while (Errors == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                Errors = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='ValidationErrorsTypeCount']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                /*   if (i > scrollsize)
                   {
                       Console.WriteLine("Maximum scrolling point reached, Failing the test");
                       Assert.Fail();
                   }
                   */
            }

            List<TypeWellboreReportGrid> Datahome = new List<TypeWellboreReportGrid>();

            for (int a = 0; a < componentgroup.Count; a++)
            {
                Datahome.Add(new TypeWellboreReportGrid()
                {


                    compname = ComponentName[a],
                    componentgrouping = componentgroup[a],
                    componentorigin = ComponentOrigin[a],
                    partType = PartType[a],
                    status = WellPerforationStatus[a],
                    serialnumber = SerialNumber[a],
                    installdate = InstallDateJS[a],
                    length = ascLength[a],
                    topdepth = TopDepth[a],
                    bottomdepth = BottomDepth[a],
                    quantity = Quantity[a],
                    innerdiameter = InnerDiameter[a],
                    outerdiameter = OuterDiameter[a],
                    remarks = Remarks[a],
                    permanentremarks = PermanentRemarks[a],
                    errors = Errors[a]

                });


            }
            return Datahome.ToArray();

        }


        public TypeWellboreReportGrid[] Search(string text)
        {
            throw new NotImplementedException();
        }


        public TypeWellboreReportGrid[] Sort(By columnsorticonlocator, string sorttype, By columnhover)
        {
            throw new NotImplementedException();
        }

        public TypeWellboreReportGrid[] Filter(string text, By columnmenuicon, By columnheader)
        {
            throw new NotImplementedException();
        }
    }
}
