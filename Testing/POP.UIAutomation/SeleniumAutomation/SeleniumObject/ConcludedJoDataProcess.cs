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
    class ConcludedJoDataProcess : IGridScenarios<TypeProspectivenConcludedJobsGrid>
    {
        double height;
        double width;
        public string scrollp;
        public int scrollsize;
        public ConcludedJoDataProcess()
        {
            height = SystemParameters.PrimaryScreenHeight;
            width = SystemParameters.PrimaryScreenWidth;

            Console.Write("Screen size found width:-" + width);

            double half = width / 2;
            scrollp = half.ToString();

        }
        public TypeProspectivenConcludedJobsGrid Colorcode(string columnname, string comparison, string value, int hex, string target)
        {
            throw new NotImplementedException();
        }

        public TypeProspectivenConcludedJobsGrid CreateNewView(string viewname)
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
        public TypeProspectivenConcludedJobsGrid[] FetchdataScreen()
        {

            string style = SeleniumActions.getAttribute(PageObjects.JobManagementPage.scrollhorizontalcontainerindex4, "style");
            string[] splitstyle = style.Split(':');
            string[] splitstyle2 = splitstyle[1].Split(';');
            Console.WriteLine("style1->" + splitstyle[1]);
            Console.WriteLine("style->" + splitstyle2[0]);
            scrollsize = int.Parse(splitstyle2[0].Substring(0, splitstyle2[0].Length - 2));
            Console.WriteLine("szie->" + scrollsize);
            //scrollsize = Convert.ToDouble(splitstyle2[0].Trim());   
            List<string> jobId = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobidconcjob);
            List<string> assetname = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.assetNameconjob);
            double i = Convert.ToDouble(scrollp);
            while (assetname == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex4, scrollp, "0");
                assetname = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.assetNameconjob);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> wellname = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.WellNameconjobs);

            while (wellname == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex4, scrollp, "0");
                wellname = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.WellNameconjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    // Assert.Fail();
                    break;
                }
            }
            List<string> status = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.StatusIdconjobs);

            while (status == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex4, scrollp, "0");
                status = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.StatusIdconjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> JobType = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobidconcjob);

            while (JobType == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex4, scrollp, "0");
                JobType = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobidconcjob);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> jobreason = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.JobReasonconjobs);

            while (jobreason == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex4, scrollp, "0");
                jobreason = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.JobReasonconjobs);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> begindate = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.BeginDateJSconjobs);

            while (begindate == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex4, scrollp, "0");
                begindate = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.BeginDateJSconjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    // Assert.Fail();
                    break;
                }
            }
            List<string> enddate = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EndDateJSconjobs);

            while (enddate == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex4, scrollp, "0");
                enddate = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EndDateJSconjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> totalcost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.TotalCostconjobs);

            while (totalcost == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex4, scrollp, "0");
                totalcost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.TotalCostconjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> dpi = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EconomicAnalysisDPIconjobs);

            while (dpi == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex4, scrollp, "0");
                dpi = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EconomicAnalysisDPIconjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }

            List<string> netvalue = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EconomicAnalysisNPVconjobs);

            while (netvalue == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex4, scrollp, "0");
                netvalue = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EconomicAnalysisNPVconjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> afe = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.AFEIdDescconobs);

            while (afe == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontawellborereport, scrollp, "0");
                afe = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.AFEIdDescconobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> accresp = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.AccountRefconjobs);

            while (accresp == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex4, scrollp, "0");
                accresp = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.AccountRefconjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> servprov = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.BusinessOrganizationconjobs);

            while (servprov == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex4, scrollp, "0");
                servprov = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.BusinessOrganizationconjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> responsibleperson = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.ResponsiblePersonconjobs);

            while (responsibleperson == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex4, scrollp, "0");
                responsibleperson = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.ResponsiblePersonconjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> truckunit = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.TruckUnitconjobs);

            while (truckunit == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex4, scrollp, "0");
                truckunit = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.TruckUnitconjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }

            List<string> PrimaryMotivationForJob = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.PrimaryMotivationForJob);

            while (PrimaryMotivationForJob == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex4, scrollp, "0");
                truckunit = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.PrimaryMotivationForJob);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }

            List<string> Remarksconcluded = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.Remarksconcluded);

            while (Remarksconcluded == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex4, scrollp, "0");
                truckunit = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.Remarksconcluded);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }

            /* **************Have to add more columns in future depending on test reuirment*************** */

            List<TypeProspectivenConcludedJobsGrid> Datahome = new List<TypeProspectivenConcludedJobsGrid>();

            for (int a = 0; a < jobId.Count; a++)
            {
                Datahome.Add(new TypeProspectivenConcludedJobsGrid()
                {

                    jobId = jobId[a],
                    assetName = assetname[a],
                    wellName = wellname[a],
                    jobType = JobType[a],
                    jobReason = jobreason[a],
                    beginDate = begindate[a],
                    endDate = enddate[a],
                    totalCost = totalcost[a],
                    dpi = dpi[a],
                    netValue = netvalue[a],
                    afeid = afe[a],
                    accountingrefernce = accresp[a],
                    serviceProvider = servprov[a],
                    responsiblePerson = responsibleperson[a],
                    truckUnit = truckunit[a],

                });


            }
            return Datahome.ToArray();

        }



        public TypeProspectivenConcludedJobsGrid[] Search(string text)
        {
            throw new NotImplementedException();
        }



        public TypeProspectivenConcludedJobsGrid[] Sort(By columnsorticonlocator, string sorttype, By columnhover)
        {
            throw new NotImplementedException();
        }

        public TypeProspectivenConcludedJobsGrid[] Filter(string text, By columnmenuicon, By columnheader)
        {
            throw new NotImplementedException();
        }
    }
}
