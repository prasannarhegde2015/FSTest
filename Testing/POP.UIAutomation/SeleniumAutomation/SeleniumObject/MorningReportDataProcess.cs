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
    class MorningReportDataProcess : IGridScenarios<TypeMorningReportGrid>
    {
        double height;
        double width;
        public string scrollp;
        public int scrollsize;
        public MorningReportDataProcess()
        {
            height = SystemParameters.PrimaryScreenHeight;
            width = SystemParameters.PrimaryScreenWidth;

            Console.Write("Screen size found width:-" + width);

            double half = width / 2;
            scrollp = half.ToString();


        }
        public TypeMorningReportGrid Colorcode(string columnname, string comparison, string value, int hex, string target)
        {
            throw new NotImplementedException();
        }

        public TypeMorningReportGrid CreateNewView(string viewname)
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
        public TypeMorningReportGrid[] FetchdataScreen()
        {

            string style = SeleniumActions.getAttribute(PageObjects.JobManagementPage.scrollhorizontalcontainerindex2, "style");
            string[] splitstyle = style.Split(':');
            string[] splitstyle2 = splitstyle[1].Split(';');
            Console.WriteLine("style1->" + splitstyle[1]);
            Console.WriteLine("style->" + splitstyle2[0]);
            scrollsize = int.Parse(splitstyle2[0].Substring(0, splitstyle2[0].Length - 2));
            Console.WriteLine("szie->" + scrollsize);
            //scrollsize = Convert.ToDouble(splitstyle2[0].Trim());   
            List<string> assetname = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='AssetName']//span[@class='cell-layer-1']/span[2]");
            List<string> WellName = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='WellName']//span[@class='cell-layer-1']/span[2]");
            double i = Convert.ToDouble(scrollp);
            while (WellName == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                WellName = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='WellName']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> JobReason = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='JobReason']//span[@class='cell-layer-1']/span[2]");

            while (JobReason == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                JobReason = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='JobReason']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> EventType = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='EventType']//span[@class='cell-layer-1']/span[2]");

            while (EventType == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                EventType = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='EventType']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> BeginTimeJS = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='BeginTimeJS']//span[@class='cell-layer-1']/span[2]");

            while (BeginTimeJS == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                BeginTimeJS = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='BeginTimeJS']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> EndTimeJS = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='EndTimeJS']//span[@class='cell-layer-1']/span[2]");

            while (EndTimeJS == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                EndTimeJS = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='EndTimeJS']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> ServiceProvider = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='ServiceProvider']//span[@class='cell-layer-1']/span[2]");

            while (ServiceProvider == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                ServiceProvider = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='ServiceProvider']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> TruckUnit = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='TruckUnit']//span[@class='cell-layer-1']/span[2]");

            while (TruckUnit == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                TruckUnit = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='TruckUnit']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> Remarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='Remarks']//span[@class='cell-layer-1']/span[2]");

            while (Remarks == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                Remarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='Remarks']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                /*  if (i > scrollsize)
                  {
                      Console.WriteLine("Maximum scrolling point reached, Failing the test");
                      Assert.Fail();
                  }*/
            }
            List<string> Cost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='Cost']//span[@class='cell-layer-1']/span[2]");

            while (Cost == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                Cost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", "//div[@col-id='Cost']//span[@class='cell-layer-1']/span[2]");
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }


            List<TypeMorningReportGrid> Datahome = new List<TypeMorningReportGrid>();

            for (int a = 0; a < EventType.Count; a++)
            {
                Datahome.Add(new TypeMorningReportGrid()
                {

                    assetName = assetname[a],
                    wellName = WellName[a],
                    jobReason = JobReason[a],
                    truckUnit = TruckUnit[a],
                    eventType = EventType[a],
                    beginDate = BeginTimeJS[a],
                    endDate = EndTimeJS[a],
                    serviceProvider = ServiceProvider[a],
                    remarks = Remarks[a],
                    cost = Cost[a]
                });


            }
            return Datahome.ToArray();

        }



        public TypeMorningReportGrid[] Search(string text)
        {
            throw new NotImplementedException();
        }


        public TypeMorningReportGrid[] Sort(By columnsorticonlocator, string sorttype, By columnhover)
        {
            throw new NotImplementedException();
        }

        public TypeMorningReportGrid[] Filter(string text, By columnmenuicon, By columnheader)
        {
            throw new NotImplementedException();
        }
    }
}
