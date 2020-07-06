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
    class DrillingReportDataProcess : IGridScenarios<TypeDrillingReportGrid>
    {
        double height;
        double width;
        public string scrollp;
        public int scrollsize;
        public DrillingReportDataProcess()
        {
            height = SystemParameters.PrimaryScreenHeight;
            width = SystemParameters.PrimaryScreenWidth;

            Console.Write("Screen size found width:-" + width);

            double half = width / 2;
            scrollp = half.ToString();


        }
        public TypeDrillingReportGrid Colorcode(string columnname, string comparison, string value, int hex, string target)
        {
            throw new NotImplementedException();
        }

        public TypeDrillingReportGrid CreateNewView(string viewname)
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
        public TypeDrillingReportGrid[] FetchdataScreen()
        {

            string style = SeleniumActions.getAttribute(PageObjects.JobManagementPage.scrollhorizontalcontainerwellborereport, "style");
            string[] splitstyle = style.Split(':');
            string[] splitstyle2 = splitstyle[1].Split(';');
            Console.WriteLine("style1->" + splitstyle[1]);
            Console.WriteLine("style->" + splitstyle2[0]);
            scrollsize = int.Parse(splitstyle2[0].Substring(0, splitstyle2[0].Length - 2));
            Console.WriteLine("szie->" + scrollsize);
            //scrollsize = Convert.ToDouble(splitstyle2[0].Trim());   
            //  List<string> subassembly = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='SubAssemblyDescription']//span[@class='cell-layer-1']/span[2]");
            List<string> componentgroup = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.compgroup);
            double i = Convert.ToDouble(scrollp);
            while (componentgroup == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                componentgroup = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.compgroup);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> PartType = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.parttypegridcell);

            while (PartType == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                PartType = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.parttypegridcell);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> ComponentName = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.componentnamegridcell);

            while (ComponentName == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                ComponentName = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.componentnamegridcell);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> SerialNumber = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.serialnumbergridcell);

            while (SerialNumber == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                SerialNumber = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.serialnumbergridcell);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> WellPerforationStatus = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.perforationstatusgridcell);

            while (WellPerforationStatus == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                WellPerforationStatus = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.perforationstatusgridcell);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> InstallDateJS = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.installdategridcell);

            while (InstallDateJS == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                InstallDateJS = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.installdategridcell);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> ascLength = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.lengthgridcell);

            while (ascLength == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                ascLength = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.lengthgridcell);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> TopDepth = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.topdepthgridcell);

            while (TopDepth == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                TopDepth = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.topdepthgridcell);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> BottomDepth = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.bottomdepthgridcell);

            while (BottomDepth == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                BottomDepth = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.bottomdepthgridcell);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> Quantity = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.quantitygridcell);

            while (Quantity == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                Quantity = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.quantitygridcell);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> ComponentOrigin = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.componentorigingridcell);

            while (ComponentOrigin == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                ComponentOrigin = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.componentorigingridcell);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> InnerDiameter = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.innerdiametergridcell);

            while (InnerDiameter == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                InnerDiameter = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.innerdiametergridcell);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> OuterDiameter = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.outerdiametergridcell);

            while (OuterDiameter == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                OuterDiameter = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.outerdiametergridcell);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> Remarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.remarksgridcell);

            while (Remarks == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                Remarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.remarksgridcell);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> PermanentRemarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.permremarksgridcell);

            while (PermanentRemarks == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                PermanentRemarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.permremarksgridcell);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> Errors = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.errorsgridcell);

            while (Errors == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                Errors = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.errorsgridcell);
                i = i + Convert.ToDouble(scrollp);
                /*  if (i > scrollsize)
                  {
                      Console.WriteLine("Maximum scrolling point reached, Failing the test");
                      Assert.Fail();
                  }
                  */
            }

            List<TypeDrillingReportGrid> Datahome = new List<TypeDrillingReportGrid>();

            for (int a = 0; a < componentgroup.Count; a++)
            {
                Datahome.Add(new TypeDrillingReportGrid()
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



        public TypeDrillingReportGrid[] Search(string text)
        {
            throw new NotImplementedException();
        }




        public TypeDrillingReportGrid[] Sort(By columnsorticonlocator, string sorttype, By columnhover)
        {
            throw new NotImplementedException();
        }

        public TypeDrillingReportGrid[] Filter(string text, By columnmenuicon, By columnheader)
        {
            throw new NotImplementedException();
        }
    }
}
