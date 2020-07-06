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
    class EventsDataProcess : IGridScenarios<TypeEventGrid>
    {
        double height;
        double width;
        public string scrollp;
        public int scrollsize;
        public EventsDataProcess()
        {
            height = SystemParameters.PrimaryScreenHeight;
            width = SystemParameters.PrimaryScreenWidth;


            double half = width / 2;
            scrollp = half.ToString();

        }
        public TypeEventGrid Colorcode(string columnname, string comparison, string value, int hex, string target)
        {
            throw new NotImplementedException();
        }

        public TypeJobCostDetailsGrid CreateNewView(string viewname)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fetch all the attribute values from Event grid 
        /// </summary>
        /// <returns>Returns TypeEventGrid properties, which specifies attributes of datas of event found in grid</returns>
        /*
                         * Get all the elemnts for individual columns                         * 
                         * Assign each attribute to properties
                         * return the class array
        */
        public TypeEventGrid[] FetchdataScreen()
        {

            string style = SeleniumActions.getAttribute(PageObjects.JobManagementPage.scrollhorizontalcontainerwellborereport, "style");
            string[] splitstyle = style.Split(':');
            string[] splitstyle2 = splitstyle[1].Split(';');
            Console.WriteLine("style1->" + splitstyle[1]);
            Console.WriteLine("style->" + splitstyle2[0]);
            scrollsize = int.Parse(splitstyle2[0].Substring(0, splitstyle2[0].Length - 2));
            Console.WriteLine("szie->" + scrollsize);
            //scrollsize = Convert.ToDouble(splitstyle2[0].Trim());   
            List<string> eventType = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EventTypeCol);
            List<string> Begintime = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.BeginTime);
            double i = Convert.ToDouble(scrollp);
            while (Begintime == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                Begintime = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.BeginTime);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> EndTime = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EndTime);

            while (EndTime == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                EndTime = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EndTime);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> Order = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.Order);

            while (Order == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                Order = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.Order);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> ServProv = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.ServiceProvider);

            while (ServProv == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                ServProv = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.ServiceProvider);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> TruckID = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.TruckUnitID);

            while (TruckID == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                TruckID = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.TruckUnitID);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            List<string> Personperforming = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.Personperforming);

            while (Personperforming == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                Personperforming = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.Personperforming);
                i = i + Convert.ToDouble(scrollp);
                /*     if (i > scrollsize)
                     {
                         Console.WriteLine("Maximum scrolling point reached, Failing the test");
                         Assert.Fail();
                     }*/
            }
            List<string> CostCat = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.CostCat);

            while (CostCat == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                CostCat = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.CostCat);
                i = i + Convert.ToDouble(scrollp);
                /*     if (i > scrollsize)
                     {
                         Console.WriteLine("Maximum scrolling point reached, Failing the test");
                         Assert.Fail();
                     }*/
            }
            List<string> HistoricalRate = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.HisRate);

            while (HistoricalRate == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                HistoricalRate = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.HisRate);
                i = i + Convert.ToDouble(scrollp);
                /*     if (i > scrollsize)
                     {
                         Console.WriteLine("Maximum scrolling point reached, Failing the test");
                         Assert.Fail();
                     }*/
            }
            List<string> Duration = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.Duration);

            while (Duration == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                Duration = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.Duration);
                i = i + Convert.ToDouble(scrollp);
                /*     if (i > scrollsize)
                     {
                         Console.WriteLine("Maximum scrolling point reached, Failing the test");
                         Assert.Fail();
                     }*/
            }
            List<string> TotalCost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.TotalCOst);

            while (TotalCost == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                TotalCost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.TotalCOst);
                i = i + Convert.ToDouble(scrollp);
                /*     if (i > scrollsize)
                     {
                         Console.WriteLine("Maximum scrolling point reached, Failing the test");
                         Assert.Fail();
                     }*/
            }
            List<string> AFE = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.AFE);

            while (AFE == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                AFE = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.AFE);
                i = i + Convert.ToDouble(scrollp);
                /*     if (i > scrollsize)
                     {
                         Console.WriteLine("Maximum scrolling point reached, Failing the test");
                         Assert.Fail();
                     }*/
            }
            List<string> workorder = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.workorder);

            while (workorder == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                workorder = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.workorder);
                i = i + Convert.ToDouble(scrollp);
                /*     if (i > scrollsize)
                     {
                         Console.WriteLine("Maximum scrolling point reached, Failing the test");
                         Assert.Fail();
                     }*/
            }
            List<string> remarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.remarks);

            while (remarks == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                remarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.remarks);
                i = i + Convert.ToDouble(scrollp);
                /*     if (i > scrollsize)
                     {
                         Console.WriteLine("Maximum scrolling point reached, Failing the test");
                         Assert.Fail();
                     }*/
            }
            List<string> changedate = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.changedate);

            while (changedate == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                changedate = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.changedate);
                i = i + Convert.ToDouble(scrollp);
                /*     if (i > scrollsize)
                     {
                         Console.WriteLine("Maximum scrolling point reached, Failing the test");
                         Assert.Fail();
                     }*/
            }
            List<string> changeuser = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.changeuser);

            while (changeuser == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                changeuser = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.changeuser);
                i = i + Convert.ToDouble(scrollp);
                /*     if (i > scrollsize)
                     {
                         Console.WriteLine("Maximum scrolling point reached, Failing the test");
                         Assert.Fail();
                     }*/
            }


            List<TypeEventGrid> Datahome = new List<TypeEventGrid>();

            for (int a = 0; a < eventType.Count; a++)
            {
                Datahome.Add(new TypeEventGrid()
                {

                    EventType = eventType[a],
                    BeginTime = Begintime[a],
                    EndTime = EndTime[a],
                    Order = Order[a],
                    ServiceProvider = ServProv[a],
                    TruckID = TruckID[a],
                    PersonPerformingTask = Personperforming[a],
                    CostCategory = CostCat[a],
                    Rate = HistoricalRate[a],
                    Duration = Duration[a],
                    TotalCost = TotalCost[a],
                    AFE = AFE[a],
                    WorkOrderID = workorder[a],
                    Remarks = remarks[a],
                    ChangeDate = changedate[a],
                    ChangeUser = changeuser[a],
                });


            }
            return Datahome.ToArray();

        }


        public TypeEventGrid[] Search(string text)
        {
            throw new NotImplementedException();
        }


        public TypeEventGrid[] Sort(By columnsorticonlocator, string sorttype, By columnhover)
        {
            throw new NotImplementedException();
        }

        public TypeEventGrid[] Filter(string text, By columnmenuicon, By columnheader)
        {
            throw new NotImplementedException();
        }

        TypeEventGrid IGridScenarios<TypeEventGrid>.CreateNewView(string viewname)
        {
            throw new NotImplementedException();
        }
    }
}
