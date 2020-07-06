using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using SeleniumAutomation.AGGridScreenDatas;
using SeleniumAutomation.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SeleniumAutomation.SeleniumObject
{
    class JobManagementGeneralDataProcess : IGridScenarios<TypeJobManagementGeneralGrid>
    {
        double height;
        double width;
        public string scrollp;
        public int scrollsize;
        public JobManagementGeneralDataProcess()
        {
            height = SystemParameters.PrimaryScreenHeight;
            width = SystemParameters.PrimaryScreenWidth;

            Console.Write("Screen size found width:-" + width);

            double half = width / 2;
            scrollp = half.ToString();


        }
        public TypeJobManagementGeneralGrid Colorcode(string columnname, string comparison, string value, int hex, string target)
        {
            throw new NotImplementedException();
        }

        public TypeJobManagementGeneralGrid CreateNewView(string viewname)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fetch all the attribute values in Job Management general grid based on the Tab and Wellname we pass
        /// </summary>
        /// <returns>Returns TypeJobManagementGeneralGrid properties, which specifies attributes of datas of well found in grid</returns>
        /*
                         * Get all the elemnts for individual columns                         * 
                         * Assign each attribute to properties
                         * return the class array
        */
        public TypeJobManagementGeneralGrid[] FetchdataScreen()
        {

            string style = SeleniumActions.getAttribute(PageObjects.GroupConfigurationPage.scrollhorizontalcontainer, "style");
            string[] splitstyle = style.Split(':');
            string[] splitstyle2 = splitstyle[1].Split(';');
            Console.WriteLine("style1->" + splitstyle[1]);
            Console.WriteLine("style->" + splitstyle2[0]);
            scrollsize = int.Parse(splitstyle2[0].Substring(0, splitstyle2[0].Length - 2));
            Console.WriteLine("szie->" + scrollsize);
            //scrollsize = Convert.ToDouble(splitstyle2[0].Trim());   
            List<string> totalrecrds = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobidgridcell);
            CommonHelper.TraceLine("Found Job Id, Count->" + totalrecrds.Count);
            List<string> jobtype = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobtypegridcell);
            double i = Convert.ToDouble(scrollp);
            while (jobtype == null)
            {
                SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, scrollp, "0");
                jobtype = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobtypegridcell);
                if (jobtype != null)
                    break;
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            CommonHelper.TraceLine("Found Job type, Count->" + jobtype.Count);
            List<string> jobreason = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobreasongridcell);

            while (jobreason == null)
            {
                SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, scrollp, "0");
                jobreason = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobreasongridcell);
                if (jobreason != null)
                    break;
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            CommonHelper.TraceLine("Found Job reason, Count->" + jobreason.Count);
            List<string> jobdriver = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobdrivergridcell);

            while (jobdriver == null)
            {
                SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, scrollp, "0");
                jobdriver = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobdrivergridcell);
                if (jobdriver != null)
                    break;
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            CommonHelper.TraceLine("Found Job driver, Count->" + jobdriver.Count);
            List<string> status = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobstatusgridcell);

            while (status == null)
            {
                SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, scrollp, "0");
                status = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobstatusgridcell);
                if (status != null)
                    break;
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            CommonHelper.TraceLine("Found Job status, Count->" + status.Count);
            List<string> afe = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.afegridcell);

            while (afe == null)
            {
                SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, scrollp, "0");
                afe = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.afegridcell);
                if (afe != null)
                    break;
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            CommonHelper.TraceLine("Found AFE, Count->" + afe.Count);
            List<string> serviceprov = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.servprovgridcell);

            while (serviceprov == null)
            {
                SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, scrollp, "0");
                serviceprov = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.servprovgridcell);
                if (serviceprov != null)
                    break;
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            CommonHelper.TraceLine("Found service provider, Count->" + serviceprov.Count);
            List<string> ActualCost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.actualcostgridcell);

            while (ActualCost == null)
            {
                SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, scrollp, "0");
                ActualCost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.actualcostgridcell);
                if (ActualCost != null)
                    break;
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            CommonHelper.TraceLine("Found actual cost, Count->" + ActualCost.Count);
            List<string> BeginDateJS = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.begindategridcell);

            while (BeginDateJS == null)
            {
                SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, scrollp, "0");
                BeginDateJS = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.begindategridcell);
                if (BeginDateJS != null)
                    break;
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            CommonHelper.TraceLine("Found begindate, Count->" + BeginDateJS.Count);
            List<string> EndDateJS = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.enddategridcell);

            while (EndDateJS == null)
            {
                SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, scrollp, "0");
                EndDateJS = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.enddategridcell);
                if (EndDateJS != null)
                    break;
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            CommonHelper.TraceLine("Found enddate, Count->" + EndDateJS.Count);
            List<string> ActualJobDurationDays = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.actualjobdurationgridcell);

            while (ActualJobDurationDays == null)
            {
                SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, scrollp, "0");
                ActualJobDurationDays = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.actualjobdurationgridcell);
                if (ActualJobDurationDays != null)
                    break;
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            CommonHelper.TraceLine("Found actual job duration, Count->" + ActualJobDurationDays.Count);
            List<string> AccountRef = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.accountrefgridcell);

            while (AccountRef == null)
            {
                SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, scrollp, "0");
                AccountRef = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.accountrefgridcell);
                if (AccountRef != null)
                    break;
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            CommonHelper.TraceLine("Found account ref, Count->" + AccountRef.Count);
            List<string> JobPlanCost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobplancostgridcell);

            while (JobPlanCost == null)
            {
                SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, scrollp, "0");
                JobPlanCost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobplancostgridcell);
                if (JobPlanCost != null)
                    break;
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            CommonHelper.TraceLine("Found Job plan cost, Count->" + JobPlanCost.Count);
            List<string> TotalCost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.totalcostgridcell);

            while (TotalCost == null)
            {
                SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, scrollp, "0");
                TotalCost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.totalcostgridcell);
                if (TotalCost != null)
                    break;

                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    Assert.Fail();
                }
            }
            CommonHelper.TraceLine("Found total cost, Count->" + TotalCost.Count);
            List<string> JobRemarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobremarksgridcell);

            while (JobRemarks == null)
            {
                SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, scrollp, "0");
                JobRemarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobremarksgridcell);
                if (JobRemarks != null)
                    break;
                i = i + Convert.ToDouble(scrollp);
                /*   if (i > scrollsize)
                   {
                       Console.WriteLine("Maximum scrolling point reached, Failing the test, job remarks count->"+JobRemarks.Count);
                       Assert.Fail();
                   }
                   */
            }
            CommonHelper.TraceLine("Found Job remarks, Count->" + JobRemarks.Count);
            List<TypeJobManagementGeneralGrid> Datahome = new List<TypeJobManagementGeneralGrid>(totalrecrds.Count);

            for (int a = 0; a < totalrecrds.Count; a++)
            {
                Datahome.Add(new TypeJobManagementGeneralGrid()
                {

                    jobId = totalrecrds[a],
                    jobType = jobtype[a],
                    jobReason = jobreason[a],
                    jobPlanCost = JobPlanCost[a],
                    totalCost = TotalCost[a],
                    afe = afe[a],
                    serviceprovider = serviceprov[a],
                    remarks = JobRemarks[a],
                    status = status[a],
                    endDate = EndDateJS[a],
                    beginDate = BeginDateJS[a],
                    actualCost = ActualCost[a],
                    jobdurationHours = ActualJobDurationDays[a],
                    accountReference = AccountRef[a],
                    jobDriver = jobdriver[a]

                });


            }
            return Datahome.ToArray();

        }

        public TypeJobManagementGeneralGrid[] Filter(string text, By columnmenuicon, By columnhoverheader)
        {
            double totalscroll = 0;
            SeleniumActions.WaitForLoad();
            bool res = SeleniumActions.mousehover(columnhoverheader);

            while (res == false)
            {
                SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, scrollp, "0");
                totalscroll = double.Parse(scrollp) + totalscroll;
                res = SeleniumActions.mousehover(columnhoverheader);
            }
            SeleniumActions.waitClick(columnmenuicon);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.filteroption);
            SeleniumActions.sendText(PageObjects.JobManagementPage.filtertext, text);
            SeleniumActions.waitClick(PageObjects.JobManagementPage.filteroption);
            SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, "-" + totalscroll.ToString(), "0");
            Thread.Sleep(1000);
            return FetchdataScreen();
        }

        public TypeJobManagementGeneralGrid[] Search(string text)
        {
            SeleniumActions.WaitForLoad();
            SeleniumActions.sendText(PageObjects.JobManagementPage.gridsearchbox, text);
            Thread.Sleep(2000); //static wait for grid refresh
            return FetchdataScreen();
        }

        public TypeJobManagementGeneralGrid[] Sort(By columnsorticonlocator, string sorttype, By columnhover)
        {
            string sorttypedetails;
            double totalscroll = 0;
            switch (sorttype.ToLower().Trim())
            {
                case "ascending":
                    sorttypedetails = SeleniumActions.getAttributeJS(columnsorticonlocator, "class");
                    if (!sorttypedetails.ToLower().Trim().Contains("hidden"))
                    {
                        CommonHelper.TraceLine("Column is already sorted in ascending order");
                    }
                    else
                    {
                        while (sorttypedetails.ToLower().Trim().Contains("hidden"))
                        {
                            SeleniumActions.waitClick(columnhover);
                            Thread.Sleep(500);
                            sorttypedetails = SeleniumActions.getAttributeJS(columnsorticonlocator, "class");
                        }
                    }
                    break;
                case "descending":

                    sorttypedetails = SeleniumActions.getAttributeJS(columnsorticonlocator, "class");
                    while (sorttypedetails == null)
                    {
                        SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, scrollp, "0");
                        totalscroll = double.Parse(scrollp) + totalscroll;
                        sorttypedetails = SeleniumActions.getAttributeJS(columnsorticonlocator, "class");
                    }
                    if (!sorttypedetails.ToLower().Trim().Contains("hidden"))
                    {
                        CommonHelper.TraceLine("Column is already sorted in descending order");
                    }
                    else
                    {
                        while (sorttypedetails.ToLower().Trim().Contains("hidden"))
                        {
                            SeleniumActions.waitClick(columnhover);
                            Thread.Sleep(500);
                            sorttypedetails = SeleniumActions.getAttributeJS(columnsorticonlocator, "class");
                        }
                    }
                    break;
                default:
                    CommonHelper.TraceLine("Please pass sort type arguement as ascending or descending"); break;
            }
            SeleniumActions.scrollbyel(PageObjects.GroupConfigurationPage.scrollhorizontal, "-" + totalscroll.ToString(), "0");
            return FetchdataScreen();
        }
    }
}
