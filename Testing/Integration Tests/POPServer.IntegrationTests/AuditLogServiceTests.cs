using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class AuditLogServiceTests : APIClientTestBase
    {
        [TestInitialize]
        public override void Init()
        {
            base.Init();
        }

        [TestCleanup]
        public override void Cleanup()
        {
            var exceptions = new List<Exception>();
            foreach (WellDTO well in _wellsToRemove)
            {
                try
                {
                    AlarmService.RemoveAlarmsByWellId(well.Id.ToString());
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            Trace.WriteLineIf(exceptions.Any(),
                string.Format("Encountered exception{0} removing well alarms: " + string.Join(" || ", exceptions.Select(ex => ex.Message)),
                exceptions.Count == 1 ? "" : "s"));
            base.Cleanup();
        }

        [TestCategory(TestCategories.AuditLogServiceTests), TestMethod]
        public void CygNetAuditLogTest()
        {

            if (s_isRunningInATS)
            {
                Trace.WriteLine("");
                //creating a RRL well
                string facillityId = GetFacilityId("RPOC_", 1);
                var well = AddRRLWell(facillityId);
                _wellsToRemove.Add(well);
                var startDate = DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-5));
                var endDate = DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(1));

                // Issue demand scan command and verifying Cygnet Log command
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.DemandScan.ToString());
                var auditLogs = AuditLogService.GetCygNetAuditLogsByWellIdAndDateRange(well.Id.ToString(), startDate, endDate);
                VerifyCygnetLogs(auditLogs.First(), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);

                // Issue Clear Error command and verifying Cygnet Log command
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.ClearErrors.ToString());
                System.Threading.Thread.Sleep(200);
                auditLogs = AuditLogService.GetCygNetAuditLogsByWellIdAndDateRange(well.Id.ToString(), startDate, endDate);
                VerifyCygnetLogs(auditLogs.ElementAt(0), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(1), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_CLEARERR", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(2), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);

                // Issue Stop well command and verifying Cygnet Log command
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.StopAndLeaveDown.ToString());
                System.Threading.Thread.Sleep(200);
                auditLogs = AuditLogService.GetCygNetAuditLogsByWellIdAndDateRange(well.Id.ToString(), startDate, endDate);
                VerifyCygnetLogs(auditLogs.ElementAt(0), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(1), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_FORCEOFF", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(2), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(3), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_CLEARERR", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(4), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);


                //Issue Start well command and verifying Cygnet Log command
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.StartRPC.ToString());
                System.Threading.Thread.Sleep(200);
                auditLogs = AuditLogService.GetCygNetAuditLogsByWellIdAndDateRange(well.Id.ToString(), startDate, endDate);
                VerifyCygnetLogs(auditLogs.ElementAt(0), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(1), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_STARTWEL", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(2), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(3), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_FORCEOFF", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(4), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(5), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_CLEARERR", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(6), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);

                //Issue Idle well command and verifying Cygnet Log command
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.IdleRPC.ToString());
                System.Threading.Thread.Sleep(200);
                auditLogs = AuditLogService.GetCygNetAuditLogsByWellIdAndDateRange(well.Id.ToString(), startDate, endDate);
                VerifyCygnetLogs(auditLogs.ElementAt(0), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(1), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_IDLE", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(2), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(3), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_STARTWEL", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(4), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(5), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_FORCEOFF", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(6), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(7), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_CLEARERR", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(8), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);

                //Issue Software Timer well command and verifying Cygnet Log command
                SurveillanceService.IssueCommandForSingleWell(well.Id, WellCommand.SoftwareTimer.ToString());
                System.Threading.Thread.Sleep(200);
                auditLogs = AuditLogService.GetCygNetAuditLogsByWellIdAndDateRange(well.Id.ToString(), startDate, endDate);
                VerifyCygnetLogs(auditLogs.ElementAt(0), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(1), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_SWTIMER", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(2), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(3), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_IDLE", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(4), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(5), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_STARTWEL", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(6), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(7), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_FORCEOFF", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(8), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(9), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_CLEARERR", "", "Succeeded", null);
                VerifyCygnetLogs(auditLogs.ElementAt(10), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);

                // Now starting Cygnet Logs testing for Non RRL well
                facillityId = GetFacilityId("ESPWELL_", 1);
                WellDTO wellESP = AddNonRRLWell(facillityId, WellTypeId.ESP);
                _wellsToRemove.Add(wellESP);

                // Issue demand scan command and verifying Cygnet Log command
                SurveillanceService.IssueCommandForSingleWell(wellESP.Id, WellCommand.DemandScan.ToString());
                auditLogs = AuditLogService.GetCygNetAuditLogsByWellIdAndDateRange(wellESP.Id.ToString(), startDate, endDate);
                VerifyCygnetLogs(auditLogs.First(), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);

                // Issue Stop well command and verifying Cygnet Log command
                bool commandSuccess = SurveillanceService.IssueCommandForSingleWell(wellESP.Id, WellCommand.StopAndLeaveDown.ToString());
                System.Threading.Thread.Sleep(200);
                auditLogs = AuditLogService.GetCygNetAuditLogsByWellIdAndDateRange(wellESP.Id.ToString(), startDate, endDate);
                if (commandSuccess)
                {
                    VerifyCygnetLogs(auditLogs.ElementAt(0), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                    VerifyCygnetLogs(auditLogs.ElementAt(1), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_FORCEOFF", "", "Succeeded", null);
                    VerifyCygnetLogs(auditLogs.ElementAt(2), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                }
                //Issue Start well command and verifying Cygnet Log command
                commandSuccess = SurveillanceService.IssueCommandForSingleWell(wellESP.Id, WellCommand.StartRPC.ToString());
                System.Threading.Thread.Sleep(200);
                auditLogs = AuditLogService.GetCygNetAuditLogsByWellIdAndDateRange(wellESP.Id.ToString(), startDate, endDate);
                if (commandSuccess)
                {
                    VerifyCygnetLogs(auditLogs.ElementAt(0), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                    VerifyCygnetLogs(auditLogs.ElementAt(1), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_STARTWEL", "", "Succeeded", null);
                    VerifyCygnetLogs(auditLogs.ElementAt(2), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                    VerifyCygnetLogs(auditLogs.ElementAt(3), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "C_FORCEOFF", "", "Succeeded", null);
                    VerifyCygnetLogs(auditLogs.ElementAt(4), "User", AuthenticatedUser.Id.ToString(), "Executed", "UISCMD_PERFORM", "LO_STATUS", "", "Succeeded", null);
                }

            }
            else
            {
                Trace.WriteLine("ECHO CygNet Configuration is not proper to run this test");
            }

        }

        public void VerifyCygnetLogs(AuditLogForesiteDTO auditLog, string actor, string name, string action, string category, string parameter, string from, string to, string units)
        {
            Assert.AreEqual(actor, auditLog.Actor, "Actor is not matching in Logs");
            //Assert.AreEqual(AuthenticatedUser.Id, auditLog.Name, "Name is not matching in Logs");
            Assert.AreEqual(action, auditLog.Action, "Action is not matching in Logs");
            Assert.AreEqual(category, auditLog.Category, "Category is not matching in Logs");
            Assert.AreEqual(parameter, auditLog.Parameter, "Parameter is not matching in Logs");
            Assert.AreEqual(from, auditLog.From, "From is not matching in Logs");
            // Assert.AreEqual(to, auditLog.To, "To is not matching in Logs");
            Assert.AreEqual(units, auditLog.Units, "UoM is not matching in Logs");
        }

        [TestCategory(TestCategories.AuditLogServiceTests), TestMethod]
        public void ForeSiteAuditLog_WellTest()
        {

            //creating a RRL well
            string facillityId = GetFacilityId("RPOC_", 1);
            var well = AddRRLWell(facillityId);
            _wellsToRemove.Add(well);

            var startDate = DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(-28));
            var endDate = DTOExtensions.ToISO8601(DateTime.Today.ToUniversalTime().AddDays(2));

            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;
            WellTestDTO latestTestData = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());
            //Get the well test audit logs
            List<AuditLogForesiteDTO> ForeSiteAudit = AuditLogService.GetForesiteAuditLogsByWellIdAndDateRange(well.Id.ToString(), startDate, endDate, ForeSiteAuditLogTypeId.WellTestAuditLog.ToString());
            VerifyForeSiteLogs(ForeSiteAudit.First(), "Insert Record", "Well Test Audit Log");

            //change the added well test data and save
            latestTestData.AverageCasingPressure = 500;
            latestTestData.AverageFluidAbovePump = 6;
            latestTestData.Gas = 1;
            latestTestData.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10));
            WellTestDataService.UpdateWellTestData(new WellTestAndUnitsDTO(units, latestTestData));
            latestTestData = WellTestDataService.GetLatestWellTestDataByWellId(well.Id.ToString());
            ForeSiteAudit = AuditLogService.GetForesiteAuditLogsByWellIdAndDateRange(well.Id.ToString(), startDate, endDate, ForeSiteAuditLogTypeId.WellTestAuditLog.ToString());

            VerifyForeSiteLogs(ForeSiteAudit.First(), "Update Record", "Well Test Audit Log");

            // Remove well test and ensure it is gone.

            WellTestDataService.DeleteWellTestDataGroup(new long[] { latestTestData.Id });
            //latestTestData = WellTestDataService.GetWellTestDataByWellId(well.Id.ToString());
            ForeSiteAudit = AuditLogService.GetForesiteAuditLogsByWellIdAndDateRange(well.Id.ToString(), startDate, endDate, ForeSiteAuditLogTypeId.WellTestAuditLog.ToString());
            VerifyForeSiteLogs(ForeSiteAudit.First(), "Remove Record", "Well Test Audit Log");

            // Now starting ForeSite Logs testing for Non RRL well types
            //Creating a Non RRL Well(ESP) and well test
            facillityId = GetFacilityId("ESPWELL_", 1);
            WellDTO wellESP = AddNonRRLWell(facillityId, WellTypeId.ESP);
            _wellsToRemove.Add(wellESP);
            WellTestUnitsDTO units_ESP = WellTestDataService.GetWellTestDefaults(wellESP.Id.ToString()).Units;
            //Get the well test    
            WellTestDTO latestTestData_ESP = WellTestDataService.GetLatestWellTestDataByWellId(wellESP.Id.ToString());
            //Get the well test audit logs
            ForeSiteAudit = AuditLogService.GetForesiteAuditLogsByWellIdAndDateRange(wellESP.Id.ToString(), startDate, endDate, ForeSiteAuditLogTypeId.WellTestAuditLog.ToString());
            //VerifyForeSiteLogs(ForeSiteAudit.First(), "Insert Record", "Well Test Audit Log");

            //change the added well test data and save
            latestTestData_ESP.AverageTubingTemperature = 2005;
            latestTestData_ESP.FlowLinePressure = 108;
            latestTestData_ESP.Gas = 2;
            latestTestData_ESP.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(10));
            WellTestDataService.UpdateWellTestData(new WellTestAndUnitsDTO(units_ESP, latestTestData_ESP));
            //get the well test audit logs
            ForeSiteAudit = AuditLogService.GetForesiteAuditLogsByWellIdAndDateRange(wellESP.Id.ToString(), startDate, endDate, ForeSiteAuditLogTypeId.WellTestAuditLog.ToString());
            VerifyForeSiteLogs(ForeSiteAudit.First(), "Update Record", "Well Test Audit Log");

            //Tune the well test and check logs
            WellTestDataService.TuneSelectedWellTests(new long[] { latestTestData_ESP.Id });

            //get the well test audit logs
            ForeSiteAudit = AuditLogService.GetForesiteAuditLogsByWellIdAndDateRange(wellESP.Id.ToString(), startDate, endDate, ForeSiteAuditLogTypeId.WellTestAuditLog.ToString());
            VerifyForeSiteLogs(ForeSiteAudit.First(), "Update Record", "Well Test Audit Log");

            //Remove the well test and check logs
            WellTestDataService.DeleteWellTestData(latestTestData_ESP.Id.ToString());
            ForeSiteAudit = AuditLogService.GetForesiteAuditLogsByWellIdAndDateRange(wellESP.Id.ToString(), startDate, endDate, ForeSiteAuditLogTypeId.WellTestAuditLog.ToString());
            VerifyForeSiteLogs(ForeSiteAudit.First(), "Remove Record", "Well Test Audit Log");

        }

        public void VerifyForeSiteLogs(AuditLogForesiteDTO auditLog, string action, string WellTestAuditLog)
        {
            Assert.AreEqual(action, auditLog.Action, "Action is not matching in Logs");
            Assert.AreEqual(WellTestAuditLog, auditLog.AuditLogType, "AuditLogType is not matching in Logs");
        }
    }

}

