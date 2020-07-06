using ArtOfTest.WebAii.Core;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using Weatherford.POP.Enums;
using System;
using ClientAutomation.TestClasses;
using ClientAutomation.Helper;

namespace ClientAutomation
{
    [CodedUITest]
    public class ForeSiteAuto : ForeSiteAutoBase
    {
        private WellConfiguration wellConfiguration = new WellConfiguration();
        private UITestData TestData = new UITestData();
        private JobManagement jobManagement = new JobManagement();
        private WellTestFlow welltestflow = new WellTestFlow();
        private WellAnalysis wellanalysis = new WellAnalysis();
        private EquipmentConfiguration eqconfig = new EquipmentConfiguration();
        private RMHistoricalInfo histinfo = new RMHistoricalInfo();


        [TestInitialize]
        public void Init()
        {
            CopyFile(DllsLocation, DeploymentItem);
            if (RMExecution != "true")
            {
                CommonHelper.DeleteWellsByAPI();
                CommonHelper.ChangeUnitSystemUserSetting("US");
            }

        }

        [TestCategory(ForeSiteWorkflow_RRL), TestMethod, DeploymentItem(ExportLocation + DeploymentItem), Timeout(180 * 60 * 1000)]
        public void CreateWellWorkflow_RRL()
        {
            CreateWellWorkflow(WellTypeId.RRL);
        }


        [TestCategory(ForeSiteWorkflow_RRLWellAnalysis), TestMethod, DeploymentItem(ExportLocation + DeploymentItem), Timeout(90 * 60 * 1000)]
        public void CreateWellWorkflow_RRLWellAnalysis()
        {
            CommonHelper.CreateNewRRLWellwithFullData();
            wellanalysis.AnalysisFlow();
        }


        [TestCategory(ForeSiteWorkflow_GLWellTestFlow), TestMethod, DeploymentItem(ExportLocation + DeploymentItem), Timeout(90 * 60 * 1000)]
        public void CreateWellWorkflow_GLWellTestFlow()
        {
            welltestflow.GLWellTestWorkFlow();
        }


        [TestCategory(ForeSiteWorkflow_GLWellAnalysisFlow), TestMethod, DeploymentItem(ExportLocation + DeploymentItem), Timeout(90 * 60 * 1000)]
        public void CreateWellWorkflow_GLWellAnalysisFlow()
        {
            wellanalysis.GLAnalysisWorkFlow();
        }

        [TestCategory(ForeSiteWorkflow_ESPWellAnalysisFlow), TestMethod, DeploymentItem(ExportLocation + DeploymentItem), Timeout(90 * 60 * 1000)]
        public void CreateWellWorkflow_ESPWellAnalysisFlow()
        {
            wellanalysis.ESPAnalysisWorkFlow();
        }


        public void WellSettings_RRL()
        {
            Manager ForeSiteUIAutoManager;
            try
            {
                ForeSiteUIAutoManager = StartApplication();
            }
            catch (Exception e)
            {
                CommonHelper.TraceLine("Failed to start >> " + e.ToString());

                //try again... ?
                //we might need a null-job in the ATS that simply tries to create a browser launch and succeeds/fails.
                //if it fails, at least it'll work the second/next time we try.  --timc
                //let's see if this works...

                System.Threading.Thread.Sleep(1000);
                ForeSiteUIAutoManager = StartApplication();
            }

            ForeSiteUIAutoManager.ActiveBrowser.AutoDomRefresh = true;
            try
            {
                ExpandNavigationItems(ForeSiteUIAutoManager);
                wellConfiguration.Configuration_General_UI(ForeSiteUIAutoManager, TestData.WellConfigData(WellTypeId.RRL));
                wellConfiguration.SaveWell(ForeSiteUIAutoManager, TestData.WellConfigData(WellTypeId.RRL));
                wellConfiguration.Settings_UI(ForeSiteUIAutoManager, TestData.IntelligentAlarmData());
                wellConfiguration.DownLoadDownholeConfig_UI(ForeSiteUIAutoManager);
                wellConfiguration.DeleteWell_UI(ForeSiteUIAutoManager);
            }
            catch (Exception e)
            {
                PrintScreen("Err_WellSettings_RRL");
                wellConfiguration.DeleteWell_UI(ForeSiteUIAutoManager);
                Assert.Fail(e.ToString());
            }
            finally
            {
                CommonHelper.TraceLine("In finally block ...");
                ForeSiteUIAutoManager.Dispose();
            }
        }

        [TestCategory(ForeSiteWorkflow_ESP), TestMethod, DeploymentItem(ExportLocation + DeploymentItem), Timeout(90 * 60 * 1000)]
        public void CreateWellWorkflow_ESP()
        {
            CreateWellWorkflow(WellTypeId.ESP);
        }

        [TestCategory(ForeSiteWorkflow_GLift), TestMethod, DeploymentItem(ExportLocation + DeploymentItem), Timeout(90 * 60 * 1000)]
        public void CreateWellWorkflow_GL()
        {
            CreateWellWorkflow(WellTypeId.GLift);
        }

        [TestCategory(ForeSiteWorkflow_NF), TestMethod, DeploymentItem(ExportLocation + DeploymentItem), Timeout(90 * 60 * 1000)]
        public void CreateWellWorkflow_NF()
        {
            CreateWellWorkflow(WellTypeId.NF);
        }

        [TestCategory(ForeSiteWorkflow_GInj), TestMethod, DeploymentItem(ExportLocation + DeploymentItem), Timeout(90 * 60 * 1000)]
        public void CreateWellWorkflow_GInj()
        {
            CreateWellWorkflow(WellTypeId.GInj);
        }

        //Since Water Injection workflow takes longer time to complete execution, increasing timeout period to 180 mins
        [TestCategory(ForeSiteWorkflow_WInj), TestMethod, DeploymentItem(ExportLocation + DeploymentItem), Timeout(180 * 60 * 1000)]
        public void CreateWellWorkflow_WInj()
        {
            CreateWellWorkflow(WellTypeId.WInj);
        }

        [TestCategory(ForeSiteWorkflow_PLift), TestMethod, DeploymentItem(ExportLocation + DeploymentItem), Timeout(90 * 60 * 1000)]
        public void CreateWellWorkflow_PLift()
        {
            CreateWellWorkflow(WellTypeId.PLift);
        }

        [TestCategory(RMWorkflow_EquipmentConfiguration), TestMethod, DeploymentItem(ExportLocation + DeploymentItem), Timeout(90 * 60 * 1000)]
        public void EquipmentConfiguration_Script()
        {
            eqconfig.EquipmentConfiguration_Script();
        }

        [TestCategory(RMWorkflow_EquipmentConfiguration), TestMethod, DeploymentItem(ExportLocation + DeploymentItem), Timeout(90 * 60 * 1000)]
        public void RM_HistoricalInfo_Script()
        {
            histinfo.RM_HistoricalInfo_Script();
        }

        public void CreateWellWorkflow(WellTypeId wType)
        {
            Manager ForeSiteUIAutoManager = StartApplication();
            ForeSiteUIAutoManager.ActiveBrowser.AutoDomRefresh = true;
            try
            {
                ExpandNavigationItems(ForeSiteUIAutoManager);
                wellConfiguration.CreateWell_UI(ForeSiteUIAutoManager, TestData.WellConfigData(wType));
                wellConfiguration.DeleteWell_UI(ForeSiteUIAutoManager);
            }
            catch (Exception e)
            {
                PrintScreen("Err_CreateWellWorkflow" + wType.ToString());
                wellConfiguration.DeleteWell_UI(ForeSiteUIAutoManager);
                Assert.Fail(e.ToString());
            }
            finally
            {
                CommonHelper.TraceLine("In finally block ...");
                //Setting Unit System to US
                CommonHelper.ChangeUnitSystemUserSetting("US");
                ForeSiteUIAutoManager.Dispose();
            }
        }

        [TestCategory(ForeSite_JobManagementWorkflow), TestMethod, DeploymentItem(ExportLocation + DeploymentItem), Timeout(90 * 60 * 1000)]
        public void JobManagemtWorkflow()
        {
            try
            {
                if (IsRunningInATS.ToUpper() == "TRUE")
                {
                    jobManagement.CreateNewWellonBlankDB();
                }
                else
                {
                    jobManagement.LaunchForeSiteNoWellCreation();
                }
                CommonHelper.TraceLine("****** START : Job Management Flow execution  **************");
                jobManagement.GotoJobManagement();
                jobManagement.CreateNewJob();
                PageObjects.PageJobManagement.UpdateJob();
                ////jobManagement.CreateEvent();
                //jobManagement.CreateJobPlan();
                //jobManagement.JobCostDetailsflow();
                jobManagement.AttachDocument();
                PageObjects.PageJobManagement.DeleteSelectedJob();
                if (IsRunningInATS.ToUpper() == "TRUE")
                {
                    jobManagement.DeleteNewWellonBlankDB();
                }

                CommonHelper.TraceLine("****** FINISH : Job Management Flow execution  **************");

            }
            catch (Exception e)
            {
                PrintScreen("Err_JobManagementWorkflow");
                wellConfiguration.DeleteWell_UI(TelerikCoreUtils.TelerikObject.mgr);
                Assert.Fail(e.ToString());
            }
            finally
            {
                CommonHelper.TraceLine("In finally block ...");
                TelerikCoreUtils.TelerikObject.mgr.Dispose();
            }

        }

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private TestContext testContextInstance;
    }
}
