using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class ReportServiceTests : APIClientTestBase
    {
        [TestInitialize]
        public override void Init()
        {
            base.Init();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            base.Cleanup();
        }

        [TestCategory(TestCategories.ReportServiceTests), TestMethod]
        public void ExternalReportCRUDTest()
        {
            //https://www.guidgenerator.com/online-guid-generator.aspx
            const string updateColumnName = "welPrimaryKey";

            //Adding SystemSetting
            string applicationID = "30463508-a009-4f2a-aece-f89d6cf9f500";
            string tenantID = "dd63fb60-07f6-4d96-8d40-ebeca61a524e";

            SystemSettingDTO reportApplicationID = SettingService.GetSystemSettingByName(SettingServiceStringConstants.EXTERNAL_REPORT_APPLICATION_ID);
            reportApplicationID.StringValue = applicationID;
            SettingService.SaveSystemSetting(reportApplicationID);
            SystemSettingDTO reportTenantID = SettingService.GetSystemSettingByName(SettingServiceStringConstants.EXTERNAL_REPORT_TENANT_ID);
            reportTenantID.StringValue = tenantID;
            SettingService.SaveSystemSetting(reportTenantID);

            //Add 1
            ExternalReportConfigurationDTO externalReport1 = new ExternalReportConfigurationDTO();

            externalReport1.ReportId = "c7d4fb03-bc65-437d-897f-b6661ccaafff";
            externalReport1.WorkspaceId = "3fd17771-6e0c-4404-aea4-4fe7e0a85106";
            externalReport1.ReportName = "Customer Profitability";
            externalReport1.FilterColumnName = "WellPrimaryKey";
            externalReport1.FilterTableName = "Well";
            externalReport1.IsClientReportFilterEnabled = false;
            externalReport1.IsFilteredByWellSelection = true;
            ReportService.AddExternalReport(externalReport1);

            //Get All
            ExternalReportConfigurationDTO[] externalReportList = ReportService.GetAllExternalReports();
            Assert.IsNotNull(externalReportList);
            Assert.AreEqual(1, externalReportList.Length);
            externalReport1 = externalReportList?.FirstOrDefault();
            Assert.IsNotNull(externalReport1);

            //Update 1
            externalReport1.FilterColumnName = updateColumnName;
            ReportService.UpdateExternalReport(externalReport1);

            //Get All
            externalReportList = ReportService.GetAllExternalReports();
            Assert.IsNotNull(externalReportList);
            Assert.AreEqual(1, externalReportList.Length);
            externalReport1 = externalReportList?.FirstOrDefault();
            Assert.IsNotNull(externalReport1);
            Assert.AreEqual(updateColumnName, externalReport1.FilterColumnName);

            //Add 2
            ExternalReportConfigurationDTO externalReport2 = new ExternalReportConfigurationDTO();
            externalReport2.ReportId = "ef965008-19d2-4df2-be47-d8bae1a711b5";
            externalReport2.WorkspaceId = "3fd17771-6e0c-4404-aea4-4fe7e0a85106";
            externalReport2.ReportName = "IT Spend Analysis";
            externalReport2.FilterColumnName = "welPrimaryKey";
            externalReport2.FilterTableName = "Well";
            externalReport2.IsClientReportFilterEnabled = false;
            externalReport2.IsFilteredByWellSelection = true;
            ReportService.AddExternalReport(externalReport2);

            //Get All
            externalReportList = ReportService.GetAllExternalReports();
            Assert.IsNotNull(externalReportList);
            Assert.AreEqual(2, externalReportList.Length);

            //Remove 1
            ReportService.RemoveExternalReport(externalReport1.Id.ToString());

            //Get All
            externalReportList = ReportService.GetAllExternalReports();
            Assert.IsNotNull(externalReportList);
            Assert.AreEqual(1, externalReportList.Length);
            externalReport2 = externalReportList?.FirstOrDefault();
            Assert.IsNotNull(externalReport2);

            //Remove 2
            ReportService.RemoveExternalReport(externalReport2.Id.ToString());

            //Remove SystemSetting
            string applicationID_r = "";
            string tenantID_r = "";
            reportApplicationID.StringValue = applicationID_r;
            SettingService.SaveSystemSetting(reportApplicationID);
            reportTenantID.StringValue = tenantID_r;
            SettingService.SaveSystemSetting(reportTenantID);

            ////Get All
            externalReportList = ReportService.GetAllExternalReports();
            Assert.IsNotNull(externalReportList);
            Assert.AreEqual(0, externalReportList.Length);

            /*Report 3 Details
            PowerBI_Test
            c77b98aa - 22f5 - 4a24 - 819c - 3f74f477c350
            3fd17771 - 6e0c - 4404 - aea4 - 4fe7e0a85106
            Well
            welPrimaryKey*/
        }


        [TestCategory(TestCategories.ReportServiceTests), TestMethod]
        public void ExternalReportCRUDTestSpotfire()
        {

            const string updateColumnName = "welPrimaryKey";

            // User Spotfire Server ; Do not know is we need to set up a separate server
            SetValuesInSystemSettings(SettingServiceStringConstants.EXTERNAL_REPORT_URI, "http://USDCFSTAPPTS005.wft.root.loc");

            //Add 1
            ExternalReportConfigurationDTO externalReportSpotfire = new ExternalReportConfigurationDTO();
            externalReportSpotfire.ReportProvider = ExternalReportProvider.Spotfire;
            externalReportSpotfire.ReportName = "All Well Status";
            externalReportSpotfire.ReportId = "/ForeSiteDB/AllWellStatus";
            externalReportSpotfire.WorkspaceId = "Well";
            externalReportSpotfire.FilterColumnName = "Well";
            externalReportSpotfire.FilterTableName = "Well";
            externalReportSpotfire.IsClientReportFilterEnabled = true;
            externalReportSpotfire.IsFilteredByWellSelection = true;
            ReportService.AddExternalReport(externalReportSpotfire);

            //Get All
            ExternalReportConfigurationDTO[] externalReportList = ReportService.GetAllExternalReports();
            Assert.IsNotNull(externalReportList);
            Assert.AreEqual(1, externalReportList.Length);
            externalReportSpotfire = externalReportList?.FirstOrDefault();
            Assert.IsNotNull(externalReportSpotfire);

            //Update 1
            externalReportSpotfire.FilterColumnName = updateColumnName;
            ReportService.UpdateExternalReport(externalReportSpotfire);

            //Get All
            externalReportList = ReportService.GetAllExternalReports();
            Assert.IsNotNull(externalReportList);
            Assert.AreEqual(1, externalReportList.Length);
            externalReportSpotfire = externalReportList?.FirstOrDefault();
            Assert.IsNotNull(externalReportSpotfire);
            Assert.AreEqual(updateColumnName, externalReportSpotfire.FilterColumnName);

            //Add 2
            ExternalReportConfigurationDTO externalReportSpotfire2 = new ExternalReportConfigurationDTO();
            externalReportSpotfire2.ReportProvider = ExternalReportProvider.Spotfire;
            externalReportSpotfire2.ReportName = "Sales performance";
            externalReportSpotfire2.ReportId = "/ForeSiteDB/Sales and Marketing";
            externalReportSpotfire2.WorkspaceId = "Sales performance";
            externalReportSpotfire2.FilterColumnName = "SalesandMarketing";
            externalReportSpotfire2.FilterTableName = "Rank";
            externalReportSpotfire2.IsClientReportFilterEnabled = false;
            externalReportSpotfire2.IsFilteredByWellSelection = false;
            ReportService.AddExternalReport(externalReportSpotfire2);

            //Get All
            externalReportList = ReportService.GetAllExternalReports();
            Assert.IsNotNull(externalReportList);
            Assert.AreEqual(2, externalReportList.Length);

            //Remove 1
            ReportService.RemoveExternalReport(externalReportSpotfire.Id.ToString());

            //Get All
            externalReportList = ReportService.GetAllExternalReports();
            Assert.IsNotNull(externalReportList);
            Assert.AreEqual(1, externalReportList.Length);
            externalReportSpotfire2 = externalReportList?.FirstOrDefault();
            Assert.IsNotNull(externalReportSpotfire2);

            //Remove 2
            ReportService.RemoveExternalReport(externalReportSpotfire2.Id.ToString());

            //Remove SystemSetting
            SetValuesInSystemSettings(SettingServiceStringConstants.EXTERNAL_REPORT_URI, "");

            ////Get All
            externalReportList = ReportService.GetAllExternalReports();
            Assert.IsNotNull(externalReportList);
            Assert.AreEqual(0, externalReportList.Length);


        }
    }
}