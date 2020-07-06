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
    class ProspectiveJobDataProcess : IGridScenarios<TypeProspectivenConcludedJobsGrid>
    {
        double height;
        double width;
        public string scrollp;
        public int scrollsize;
        public ProspectiveJobDataProcess()
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

            string style = SeleniumActions.getAttribute(PageObjects.JobManagementPage.scrollhorizontalcontainerindex2, "style");
            string[] splitstyle = style.Split(':');
            string[] splitstyle2 = splitstyle[1].Split(';');
            Console.WriteLine("style1->" + splitstyle[1]);
            Console.WriteLine("style->" + splitstyle2[0]);
            scrollsize = int.Parse(splitstyle2[0].Substring(0, splitstyle2[0].Length - 2));
            Console.WriteLine("szie->" + scrollsize);
            //scrollsize = Convert.ToDouble(splitstyle2[0].Trim());   
            List<string> jobId = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.jobidprosjob);
            List<string> assetname = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.assetNameprosjob);
            double i = Convert.ToDouble(scrollp);



            while (assetname == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                assetname = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.assetNameprosjob);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached and asset name is not found, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }


            List<string> wellname = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.WellNameprosjobs);

            while (wellname == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                wellname = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.WellNameprosjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached and well name is not found, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> status = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.StatusIdprojobs);

            while (status == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                status = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.StatusIdprojobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached and status is not found , Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> JobType = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.JobTypeprosjobs);

            while (JobType == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                JobType = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.JobTypeprosjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached and JobType is not found, Failing the test");
                    // Assert.Fail();
                    break;
                }
            }
            List<string> jobreason = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.JobReasonprosjobs);

            while (jobreason == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                jobreason = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.JobReasonprosjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> begindate = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.BeginDateJSprosjobs);

            while (begindate == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                begindate = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.BeginDateJSprosjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> enddate = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EndDateJSprosjobs);

            while (enddate == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                enddate = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EndDateJSprosjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    // Assert.Fail();
                    break;
                }
            }
            List<string> totalcost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.TotalCostprosjobs);

            while (totalcost == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                totalcost = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.TotalCostprosjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> dpi = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EconomicAnalysisDPIprosjobs);

            while (dpi == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                dpi = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EconomicAnalysisDPIprosjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    // Assert.Fail();
                    break;
                }
            }

            List<string> netvalue = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EconomicAnalysisNPVprosjobs);

            while (netvalue == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                netvalue = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.EconomicAnalysisNPVprosjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> afe = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.AFEIdDescprosjobs);

            while (afe == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                afe = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.AFEIdDescprosjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> accresp = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.AccountRefprosjobs);

            while (accresp == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                accresp = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.AccountRefprosjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> servprov = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.BusinessOrganizationprosjobs);

            while (servprov == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                servprov = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.BusinessOrganizationprosjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> responsibleperson = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.ResponsiblePersonprosjobs);

            while (responsibleperson == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                responsibleperson = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.ResponsiblePersonprosjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> truckunit = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.TruckUnitprosjobs);

            while (truckunit == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                truckunit = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.TruckUnitprosjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    // Assert.Fail();
                    break;
                }
            }
            List<string> motivation = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.PrimaryMotivationForJobprosjobs);

            while (motivation == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                motivation = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.PrimaryMotivationForJobprosjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }
            }
            List<string> remarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.JobRemarksprosjobs);

            while (remarks == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                remarks = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.JobRemarksprosjobs);
                i = i + Convert.ToDouble(scrollp);
                if (i > scrollsize)
                {
                    Console.WriteLine("Maximum scrolling point reached, Failing the test");
                    //Assert.Fail();
                    break;
                }

            }

            List<string> apprpr = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.approvpers);

            while (apprpr == null)
            {
                SeleniumActions.scrollbyel(PageObjects.JobManagementPage.scrollhorizontaindex2, scrollp, "0");
                apprpr = SeleniumActions.Gettotalrecordsinlistelemnevisisble("xpath", PageObjects.JobManagementPage.approvpers);
                i = i + Convert.ToDouble(scrollp);

            }


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
                    primarymotivation = motivation[a],
                    remarks = remarks[a],
                    approvernames = apprpr[a]
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
