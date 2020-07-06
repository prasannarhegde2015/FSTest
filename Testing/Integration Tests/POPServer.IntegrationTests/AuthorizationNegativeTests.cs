using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;

namespace Weatherford.POP.Server.IntegrationTests
{
    [TestClass]
    public class AuthorizationNegativeTests : APIClientTestBase
    {
        private static string s_negativeAuthDomain = ConfigurationManager.AppSettings.Get("NegativeAuthDomain");
        private static string s_negativeAuthUser = ConfigurationManager.AppSettings.Get("NegativeAuthUser");
        private static string s_negativeAuthPassword = ConfigurationManager.AppSettings.Get("NegativeAuthPassword");

        private List<GenericGridViewDTO> _viewsToRemove = new List<GenericGridViewDTO>();

        private static string NegativeAuthUserName
        {
            get { return s_negativeAuthDomain + "\\" + s_negativeAuthUser; }
        }

        static AuthorizationNegativeTests()
        {
            if (string.IsNullOrEmpty(s_negativeAuthDomain) && string.IsNullOrEmpty(s_negativeAuthUser) && string.IsNullOrEmpty(s_negativeAuthPassword))
            {
                s_negativeAuthDomain = "VSI";
                s_negativeAuthUser = "ATSTest1";
                s_negativeAuthPassword = "CygNet4Fun";
            }
        }

        private UserDTO _userToRestore;

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            string userName = NegativeAuthUserName;
            UserDTO negativeAuthUser = AdministrationService.GetUsers().Where(x => x.Name == userName).FirstOrDefault();
            if (negativeAuthUser == null)
            {
                UserDTO newBasicUser = new UserDTO { Name = userName };
                AdministrationService.AddUser(newBasicUser);
                Trace.WriteLine("Added user " + newBasicUser.Name);
                newBasicUser = AdministrationService.GetUsers().FirstOrDefault(u => u.Name == newBasicUser.Name) ?? newBasicUser;
                Assert.AreNotEqual(0, newBasicUser.Id, "Failed to add new user for test.");
            }
            else if ((negativeAuthUser.Roles?.Count ?? 0) > 0)
            {
                // Okay... this is trickier.
                _userToRestore = negativeAuthUser;
                negativeAuthUser = AdministrationService.GetUsers().Where(x => x.Name == userName).FirstOrDefault();
                negativeAuthUser.Roles.Clear();
                AdministrationService.UpdateUser(negativeAuthUser);
                negativeAuthUser = AdministrationService.GetUsers().Where(x => x.Name == userName).FirstOrDefault();
                Assert.IsTrue(negativeAuthUser.Roles == null || negativeAuthUser.Roles.Count == 0, "Failed to remove roles from user {0}.", negativeAuthUser.Name);
            }
        }

        [TestCleanup]
        public override void Cleanup()
        {
            s_suppressWebErrorMessages = false;
            // Re-authenticate in case we still have a user token from impersonation.
            Authenticate();
            if (_userToRestore != null)
            {
                AdministrationService.UpdateUser(_userToRestore);
                _userToRestore = null;
            }

            TemplateJobGroupingDTO[] tjobs = JobAndEventService.GetTemplateJobs();
            if (tjobs != null && tjobs.Count() > 0)
            {
                foreach (TemplateJobGroupingDTO tj in tjobs)
                {
                    foreach (TemplateJobDTO jobs in tj.TemplateJobs)
                    {
                        JobAndEventService.RemoveTemplateJob(jobs.TemplateJobId.ToString());
                    }
                }
            }
            if (_viewsToRemove.Any())
            {
                try
                {
                    SettingService.RemoveGenericGridViews(_viewsToRemove.Select(t => t.Id).ToArray());
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Failed to remove grid views: {ex.Message}{Environment.NewLine}{ex.ToString()}");
                }
            }

            base.Cleanup();
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthAddWell()
        {
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    WellDTO wellToAdd = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL });
                    WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = wellToAdd }); // this should not add well
                    WellDTO addedWell = WellService.GetWellByName(wellToAdd.Name);
                    if (addedWell != null)
                    {
                        _wellsToRemove.Add(addedWell);
                    }
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthUpdateRemoveWell()
        {
            WellDTO AddedWell;
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", IntervalAPI = "IntervalAPI", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });
            AddedWell = WellService.GetAllWells().Where(x => x.Name == DefaultWellName).FirstOrDefault();
            Assert.IsNotNull(AddedWell);
            Trace.WriteLine(AddedWell.Name);
            AddedWell.FacilityId = "FacilityId";
            _wellsToRemove.Add(AddedWell);

            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                try
                {
                    s_suppressWebErrorMessages = true;
                    WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = AddedWell });
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
                try
                {
                    //WellService.RemoveWell(AddedWell.Id.ToString());
                    WellConfigurationService.RemoveWellConfig(AddedWell.Id.ToString());
                    Assert.Fail("Expected exception not caught");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthAddWorkoverRemoveInstance()
        {
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();

                var report = new ReportDTO();
                try
                {
                    s_suppressWebErrorMessages = true;
                    WellboreComponentService.RemoveComponentHistoryByDate("11-15-2011", report.Id.ToString()); // the date doesn't matter to this test since its all negative test here
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
                try
                {
                    WellboreComponentService.AddReport(report);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthAddWellTest()
        {
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                try
                {
                    s_suppressWebErrorMessages = true;
                    WellTestDataService.AddWellTestData(new WellTestAndUnitsDTO(new WellTestUnitsDTO(), new WellTestDTO())); // this doesn't matter to this test as its a negative test
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthUpdateRemoveWellTest()
        {
            //add new well for test
            var toAdd = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today.AddYears(-2), WellType = WellTypeId.RRL });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = toAdd });
            var allWells = WellService.GetAllWells();
            WellDTO well = allWells.FirstOrDefault(w => w.Name == DefaultWellName);
            Assert.IsNotNull(well, "Failed to get added well.");
            _wellsToRemove.Add(well);

            WellTestUnitsDTO units = WellTestDataService.GetWellTestDefaults(well.Id.ToString()).Units;

            //add new wellTestData
            var testDataDTO = new WellTestDTO
            {
                WellId = well.Id,
                AverageCasingPressure = 1,
                AverageFluidAbovePump = 1,
                AverageTubingPressure = 90,
                AverageTubingTemperature = 100,
                Gas = 0,
                GasGravity = 0,
                Oil = 83,
                OilGravity = 0,
                PumpEfficiency = 0,
                PumpIntakePressure = 100,
                PumpingHours = 10,
                //SPTCode = 1,
                SPTCodeDescription = WellTestQuality.RepresentativeTest.ToString(),
                StrokePerMinute = 0,
                TestDuration = 3,
                Water = 75,
                WaterGravity = 0
            };
            testDataDTO.SampleDate = (DateTime.Today.ToUniversalTime() - TimeSpan.FromDays(100));
            WellTestDataService.AddWellTestData(new WellTestAndUnitsDTO(units, testDataDTO));

            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                WellTestDTO testDataDTOCheck;
                Authenticate();
                try
                {
                    s_suppressWebErrorMessages = true;
                    var WellTestDataArray = WellTestDataService.GetWellTestDataByWellId(well.Id.ToString());
                    testDataDTOCheck = WellTestDataArray.Values.FirstOrDefault(a => a.WellId == well.Id);
                    Assert.IsNotNull(testDataDTOCheck);
                    WellTestDataService.UpdateWellTestData(new WellTestAndUnitsDTO(units, testDataDTOCheck));
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
                try
                {
                    var WellTestDataArray = WellTestDataService.GetWellTestDataByWellId(well.Id.ToString());
                    testDataDTOCheck = WellTestDataArray.Values.FirstOrDefault(a => a.WellId == well.Id);
                    WellTestDataService.DeleteWellTestData(testDataDTOCheck.Id.ToString());
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthScanWellIssueCommand()
        {
            string service = s_cvsService;
            DataConnectionDTO DataConnection = GetDefaultCygNetDataConnection();
            WellDTO AddedWell;
            DataConnectionDTO AddedDC;
            DataConnectionDTO existingDC = DataConnectionService.GetAllDataConnections().Where(dc => dc.ProductionDomain == s_domain && dc.Site == s_site && dc.Service == service).FirstOrDefault();
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", DataConnection = DataConnection, SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });
            AddedDC = existingDC != null ? null : DataConnectionService.GetAllDataConnections().Where(dc => dc.ProductionDomain == s_domain && dc.Site == s_site && dc.Service == service).FirstOrDefault();
            AddedWell = WellService.GetAllWells().Where(x => x.Name == DefaultWellName).FirstOrDefault();
            Assert.IsNotNull(AddedWell);
            _wellsToRemove.Add(AddedWell);
            if (AddedDC != null)
            {
                _dataConnectionsToRemove.Add(AddedDC);
            }

            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                try
                {
                    s_suppressWebErrorMessages = true;
                    SurveillanceService.IssueCommandForSingleWell(AddedWell.Id, "DemandScan"); // this should not scan well
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
                try
                {
                    SurveillanceService.IssueCommandForSingleWell(AddedWell.Id, "StartRPC");  // this should not start well
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthScanWellIssueGeneralCommand()
        {
            string service = s_cvsService;
            DataConnectionDTO DataConnection = GetDefaultCygNetDataConnection();
            WellDTO AddedWell;
            DataConnectionDTO AddedDC;
            DataConnectionDTO existingDC = DataConnectionService.GetAllDataConnections().Where(dc => dc.ProductionDomain == s_domain && dc.Site == s_site && dc.Service == service).FirstOrDefault();
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", DataConnection = DataConnection, SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.ESP }) });
            AddedDC = existingDC != null ? null : DataConnectionService.GetAllDataConnections().Where(dc => dc.ProductionDomain == s_domain && dc.Site == s_site && dc.Service == service).FirstOrDefault();
            AddedWell = WellService.GetAllWells().Where(x => x.Name == DefaultWellName).FirstOrDefault();
            Assert.IsNotNull(AddedWell);
            _wellsToRemove.Add(AddedWell);
            if (AddedDC != null)
            {
                _dataConnectionsToRemove.Add(AddedDC);
            }

            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                try
                {
                    s_suppressWebErrorMessages = true;
                    SurveillanceService.IssueCommandForSingleWell(AddedWell.Id, "DemandScan"); // this should not scan well
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
                try
                {
                    SurveillanceService.IssueCommandForSingleWell(AddedWell.Id, "StartRPC");  // this should not start well
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthAnalysisOptionsRun()
        {
            DataConnectionDTO DataConnection = GetDefaultCygNetDataConnection();
            DataConnectionDTO AddedDC;
            WellDTO AddedWell;

            DataConnectionDTO existingDC = DataConnectionService.GetAllDataConnections().Where(dc => dc.ProductionDomain == s_domain && dc.Site == s_site && dc.Service == s_cvsService).FirstOrDefault();
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", DataConnection = DataConnection, SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });
            AddedDC = existingDC == null ? DataConnectionService.GetAllDataConnections().Where(x => x.ProductionDomain == s_domain).FirstOrDefault() : null;
            if (AddedDC != null)
            {
                _dataConnectionsToRemove.Add(AddedDC);
            }
            AddedWell = WellService.GetAllWells().Where(x => x.Name == DefaultWellName).FirstOrDefault();
            Assert.IsNotNull(AddedWell);
            _wellsToRemove.Add(AddedWell);

            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                try
                {
                    s_suppressWebErrorMessages = true;
                    DynaCardEntryDTO analysisCurrentCard = DynacardService.AnalyzeSelectedSurfaceCard(AddedWell.Id.ToString(), "636172559291060000", "1", "True"); // should not do analysis
                    Trace.WriteLine(analysisCurrentCard.ErrorMessage);
                    Assert.IsTrue(analysisCurrentCard.ErrorMessage.ToLower().Trim().Contains("do not have permission"));
                    // Assert.Fail("Expected exception not thrown");
                    //TODO talk to Sean about return exception in empty object
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
                try
                {
                    WellSettingDTO[] newWellSetting = new[] { new WellSettingDTO { WellId = AddedWell.Id, StringValue = "WellSetting New" } };
                    SettingService.SaveWellSettings(newWellSetting);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void SettingServiceNegativeAuth()
        {
            // Need the original authenticated user to attach to the user setting.
            Assert.IsNotNull(AuthenticatedUser, "Failed to get authenticated user.");
            AuthenticatedUserDTO originalUser = AuthenticatedUser;

            // Add well to use for well setting.
            var well = SetDefaultFluidType(new WellDTO()
            {
                Name = "AuthorizationNegativeTestWell",
                CommissionDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Local),
                AssemblyAPI = "AuthorizationNegativeTestWell",
                SubAssemblyAPI = "AuthorizationNegativeTestWell",
                WellType = WellTypeId.RRL,
            });
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = well });
            well = WellService.GetWellByName(well.Name);
            Assert.IsNotNull(well, "Failed to add well.");
            _wellsToRemove.Add(well);

            // Get settings and values to test remove/update failure.
            SettingDTO setting = SettingService.GetSettingByName(SettingServiceStringConstants.AUTHORIZATION_NEGATIVE_TEST_SETTING);
            Assert.IsNotNull(setting, "Failed to get first setting for test.");
            SettingDTO setting2 = SettingService.GetSettingByName(SettingServiceStringConstants.AUTHORIZATION_NEGATIVE_TEST_SETTING_2);
            Assert.IsNotNull(setting2, "Failed to get second setting for test.");

            var systemSetting = new SystemSettingDTO()
            {
                Setting = setting,
                SettingId = setting.Id,
                StringValue = "System value test.",
            };
            SettingService.SaveSystemSetting(systemSetting);
            systemSetting = SettingService.GetSystemSettingByName(setting.Name) ?? systemSetting;
            Assert.AreNotEqual(0, systemSetting.Id, "Failed to add system setting.");
            _systemSettingsToRemove.Add(systemSetting);

            var userSetting = new UserSettingDTO()
            {
                Setting = setting,
                SettingId = setting.Id,
                StringValue = "User value test.",
                OwnerId = originalUser.Id,
            };
            SettingService.SaveUserSetting(userSetting);
            userSetting = SettingService.GetUserSettingsByUserId(originalUser.Id.ToString()).FirstOrDefault(s => s.SettingId == setting.Id) ?? userSetting;
            Assert.AreNotEqual(0, userSetting.Id, "Failed to add user setting.");
            _userSettingsToRemove.Add(userSetting);

            var wellSetting = new WellSettingDTO()
            {
                Setting = setting,
                SettingId = setting.Id,
                WellId = well.Id,
                StringValue = "Well value test.",
            };
            SettingService.SaveWellSetting(wellSetting);
            wellSetting = SettingService.GetWellSettingsByWellId(well.Id.ToString()).FirstOrDefault(ws => ws.SettingId == setting.Id) ?? wellSetting;
            Assert.AreNotEqual(0, wellSetting.Id, "Failed to add well setting.");

            var failSetting = new SettingDTO() { Name = "AuthorizationNegativeTestSettingFail", SettingCategory = SettingCategory.None, SettingType = SettingType.System, SettingValueType = SettingValueType.Text };
            ImpersonatorScoped impersonatedUser = null;
            try
            {
                impersonatedUser = ImpersonateTestUser();
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();

                // First user settings for the current user which should still work.
                var personalUserSetting = new UserSettingDTO() { Setting = setting, SettingId = setting.Id, OwnerId = AuthenticatedUser.Id, StringValue = "This should not fail." };
                SettingService.SaveUserSetting(personalUserSetting);
                personalUserSetting = SettingService.GetUserSettingsByUserId(AuthenticatedUser.Id.ToString()).FirstOrDefault(us => us.SettingId == setting.Id) ?? personalUserSetting;
                Assert.AreNotEqual(0, personalUserSetting.Id, "Failed to add personal user setting for impersonated user.");
                _userSettingsToRemove.Add(personalUserSetting);
                // Test update.
                personalUserSetting.StringValue = "This should not fail - update.";
                SettingService.SaveUserSetting(personalUserSetting);
                UserSettingDTO updatedPersonalUserSetting = SettingService.GetUserSettingsByUserId(AuthenticatedUser.Id.ToString()).FirstOrDefault(us => us.SettingId == setting.Id);
                Assert.IsNotNull(updatedPersonalUserSetting, "Failed to get updated personal user setting.");
                Assert.AreEqual(updatedPersonalUserSetting.StringValue, personalUserSetting.StringValue, "Failed to update personal user setting.");
                // Test save.
                personalUserSetting.StringValue = "This should not fail - save.";
                SettingService.SaveUserSettings(new[] { personalUserSetting });
                updatedPersonalUserSetting = SettingService.GetUserSettingsByUserId(AuthenticatedUser.Id.ToString()).FirstOrDefault(us => us.SettingId == setting.Id);
                Assert.IsNotNull(updatedPersonalUserSetting, "Failed to get saved personal user setting.");
                Assert.AreEqual(updatedPersonalUserSetting.StringValue, personalUserSetting.StringValue, "Failed to save personal user setting.");
                // Finally test remove.
                SettingService.RemoveUserSetting(personalUserSetting.Id.ToString());
                updatedPersonalUserSetting = SettingService.GetUserSettingsByUserId(AuthenticatedUser.Id.ToString()).FirstOrDefault(us => us.SettingId == setting.Id);
                Assert.IsTrue(updatedPersonalUserSetting == null || updatedPersonalUserSetting.Id == 0, "Failed to remove personal user setting.");
                personalUserSetting = _userSettingsToRemove.FirstOrDefault(us => us.Id == personalUserSetting.Id);
                _userSettingsToRemove.Remove(personalUserSetting);

                // Now start testing stuff that should fail.
                s_suppressWebErrorMessages = true;

                // Test add.
                TestThrowForbidden("Adding a system setting should have failed.", () =>
                {
                    var failSystemSetting = new SystemSettingDTO() { Setting = setting2, SettingId = setting2.Id, StringValue = "This should fail." };
                    SettingService.SaveSystemSetting(failSystemSetting);
                });
                TestThrowForbidden("Adding a well setting should have failed.", () =>
                {
                    var failWellSetting = new WellSettingDTO() { Setting = setting2, SettingId = setting2.Id, WellId = well.Id, StringValue = "This should fail." };
                    SettingService.SaveWellSetting(failWellSetting);
                });
                TestThrowForbidden("Adding a user setting should have failed.", () =>
                {
                    var failUserSetting = new UserSettingDTO() { Setting = setting2, SettingId = setting2.Id, OwnerId = originalUser.Id, StringValue = "This should fail." };
                    SettingService.SaveUserSetting(failUserSetting);
                });

                // Test remove.
                TestThrowForbidden("Removing a system setting should fail.", () =>
                {
                    SettingService.RemoveSystemSetting(systemSetting.Id.ToString());
                });
                TestThrowUnauthorized("Removing a user setting should fail.", () =>
                {
                    SettingService.RemoveUserSetting(userSetting.Id.ToString());
                });
                TestThrowForbidden("Removing a well setting should fail.", () =>
                {
                    SettingService.RemoveWellSetting(wellSetting.Id.ToString());
                });
            }
            finally
            {
                impersonatedUser?.Dispose();
                impersonatedUser = null;

                // Mark stuff for cleanup that shouldn't have been added.
                UserSettingDTO failUserSetting = SettingService.GetUserSettingsByUserId(AuthenticatedUser.Id.ToString()).FirstOrDefault(us => us.SettingId == setting2.Id);
                if (failUserSetting != null) { _userSettingsToRemove.Add(failUserSetting); }

                WellSettingDTO failWellSetting = SettingService.GetWellSettingsByWellId(well.Id.ToString()).FirstOrDefault(ws => ws.SettingId == setting2.Id);
                if (failWellSetting != null) { _wellSettingsToRemove.Add(failWellSetting); }

                SystemSettingDTO failSystemSetting = SettingService.GetSystemSettingByName(setting2.Name);
                if (failSystemSetting != null) { _systemSettingsToRemove.Add(failSystemSetting); }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void DisabledUser()
        {
            UserDTO negativeAuthUser = AdministrationService.GetUsers().Where(x => x.Name == NegativeAuthUserName).FirstOrDefault();
            Assert.IsNotNull(negativeAuthUser, "Failed to get test user.");
            negativeAuthUser.IsDisabled = true;
            AdministrationService.UpdateUser(negativeAuthUser);
            ImpersonatorScoped impersonator = null;
            try
            {
                impersonator = ImpersonateTestUser();
                TestThrowUnauthorized("User should not be authorized.", () => Authenticate());
            }
            finally
            {
                impersonator?.Dispose();
                impersonator = null;
                // Authenticate and reset disabled flag on test user.
                Authenticate();
                negativeAuthUser.IsDisabled = false;
                AdministrationService.UpdateUser(negativeAuthUser);
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void AddRemoveAnalysisReport()
        {
            ImpersonatorScoped impersonator = null;
            try
            {
                impersonator = ImpersonateTestUser();
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();

                // Suppress error messages since we're expecting failures.
                s_suppressWebErrorMessages = true;

                // Test add.
                TestThrowForbidden("Adding an analysis report should fail.", () =>
                {
                    DynacardService.AddAnalysisReport(new AnalysisReportDTO() { WellId = 1, CardTime = DateTime.UtcNow.Ticks, AnalysisMethod = DynaCardLibrary.API.Enums.DownholeCardSource.CalculatedEverittJennings, CardType = DynaCardLibrary.API.Enums.CardType.Alarm, Timestamp = DateTime.UtcNow });
                });

                // Test remove.
                TestThrowForbidden("Removing an analysis report should fail.", () =>
                {
                    DynacardService.RemoveAnalysisReport(new AnalysisReportDTO() { WellId = 1, CardTime = DateTime.UtcNow.Ticks, AnalysisMethod = DynaCardLibrary.API.Enums.DownholeCardSource.CalculatedEverittJennings, CardType = DynaCardLibrary.API.Enums.CardType.Alarm, Timestamp = DateTime.UtcNow });
                });
            }
            finally
            {
                impersonator?.Dispose();
                impersonator = null;
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthChangeMOP()
        {
            WellConfigDTO well = AddWell("RPOC_");
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    WellMoPHistoryDTO wellMoPHistoryDTO = new WellMoPHistoryDTO();
                    wellMoPHistoryDTO.WellId = well.Well.Id;
                    wellMoPHistoryDTO.WellType = WellTypeId.RRL;
                    wellMoPHistoryDTO.ChangeDate = DateTime.Today.AddDays(2).ToUniversalTime();
                    wellMoPHistoryDTO.Comment = "ESP to RRL";
                    WellService.ChangeWellType(wellMoPHistoryDTO);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        #region Authorization Tests for WSM

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthAddJob()
        {
            AssemblyDTO assembly;
            WellDTO AddedWell;
            //Add Well
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });
            AddedWell = WellService.GetAllWells().Where(x => x.Name == DefaultWellName).FirstOrDefault();
            Assert.IsNotNull(AddedWell);
            Trace.WriteLine(AddedWell.Name);
            AddedWell.FacilityId = "FacilityId";
            _wellsToRemove.Add(AddedWell);
            assembly = WellboreComponentService.GetAssemblyByWellId(AddedWell.Id.ToString());

            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    JobLightDTO Job = new JobLightDTO() { WellId = AddedWell.Id, AssemblyId = assembly.Id, JobTypeId = 1, JobReasonId = 1, StatusId = 1, BusinessOrganizationId = 1, BeginDate = DateTime.Today.ToUniversalTime(), EndDate = DateTime.Today.ToUniversalTime(), AFEId = 1 };
                    JobAndEventService.AddJob(Job);
                    JobLightDTO[] getJob = JobAndEventService.GetJobsByWell(AddedWell.Id.ToString());
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        public JobLightDTO AddWellAssemblyJob()
        {
            AssemblyDTO assembly;
            WellDTO AddedWell;
            //Add Well
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = SetDefaultFluidType(new WellDTO() { Name = DefaultWellName, FacilityId = "CASETEST", SubAssemblyAPI = "SubAssemblyAPI", AssemblyAPI = "AssemblyAPI", CommissionDate = DateTime.Today, WellType = WellTypeId.RRL }) });
            AddedWell = WellService.GetAllWells().Where(x => x.Name == DefaultWellName).FirstOrDefault();
            Assert.IsNotNull(AddedWell);
            Trace.WriteLine(AddedWell.Name);
            AddedWell.FacilityId = "FacilityId";
            _wellsToRemove.Add(AddedWell);
            assembly = WellboreComponentService.GetAssemblyByWellId(AddedWell.Id.ToString());

            //Add Job
            JobLightDTO job = new JobLightDTO();
            job.WellId = AddedWell.Id;
            job.WellName = AddedWell.Name;
            job.BeginDate = DateTime.Today.AddDays(0).ToUniversalTime();
            job.EndDate = DateTime.Today.AddDays(30).ToUniversalTime();
            job.ActualCost = (decimal)100000.00;
            job.ActualJobDurationDays = (decimal)20.5;
            job.TotalCost = (decimal)150000.00;
            job.JobRemarks = "TestJobRemarks " + DateTime.UtcNow.ToString();
            job.JobOrigin = "TestJobOrigin ";
            job.AssemblyId = assembly.Id;
            //For Below fields User can select any value in the dropdown menu
            job.AFEId = JobAndEventService.GetAFEs().FirstOrDefault().Id;
            job.StatusId = JobAndEventService.GetJobStatuses().FirstOrDefault(j => j.Name == "Approved").Id;
            job.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault().id;
            job.BusinessOrganizationId = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            job.AccountRef = "1";
            //JobReasonId drop down selection is based on the JobTypeId
            job.JobReasonId = JobAndEventService.GetJobReasonsForJobType(job.JobTypeId.ToString()).FirstOrDefault().Id;
            job.JobDriverId = (int)JobAndEventService.GetPrimaryMotivationsForJob().FirstOrDefault().Id;

            return job;
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthUpdateRemoveApproveJob()
        {
            JobLightDTO job = AddWellAssemblyJob();
            //Add Job
            string addJob = JobAndEventService.AddJob(job);
            Assert.IsNotNull(addJob, "Failed to add a Job");
            job.JobRemarks = "Updating Job through Unauthorized user";
            job.JobId = Convert.ToInt64(addJob);
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    JobAndEventService.UpdateJob(job);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
                try
                {
                    s_suppressWebErrorMessages = true;
                    JobAndEventService.ApproveJob(addJob);
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        public MetaDataDTO[] SetMetadata(MetaDataDTO[] eventMetadata, JobLightDTO getJob)
        {
            long id = 0;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_JOB").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.JobId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_AFE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.AFEId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_ASSEMBLY").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.AssemblyId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_BUSINESSORGANIZATION").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 2; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_R_TRUCKUNIT").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = JobAndEventService.GetCatalogItemGroupData().TruckUnits.FirstOrDefault().TruckUnitId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_R_CATALOGITEM").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = JobAndEventService.GetCatalogItemGroupData().CatalogItems.FirstOrDefault().CatalogItemId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_EVENT_PARENT").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 10;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTBEGDTTM").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = DateTime.UtcNow.ToString();
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTENDDTTM").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = DateTime.UtcNow.AddDays(10).ToString();
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCDURATIONHOURS").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.ActualJobDurationDays;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTORDER").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 10;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCWORKORDERID").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "WRKORDID_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFIELDSERVICEORDERID").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "FLDSRVID_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCRESPONSIBLEPERSON").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "RESPONSIBLE_PERSON_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCPERSONPERFORMINGTASK").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "PERFORMING_TASK_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCQUANTITY").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 50m;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCTOTALCOST").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 100m;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCDOCUMENTFILENAME").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "EVCDOCUMENTFILENAME_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCORIGINKEY").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "EVCORIGINKEY_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCUNPLANNED").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = true;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCTROUBLE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = true;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCPREVENTIVEMAINTENANCE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = true;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCREMARKS").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.JobRemarks;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_BUSINESSORGANIZATION").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.BusinessOrganizationId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_WBFLOWAREA").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_GASTYPE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_DIVERTERTYPE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_DISPLACEMENTFLUID_PREFLUSH").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_ACIDTYPE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_DISPLACEMENTFLUID").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value

            return eventMetadata;
        }

        public MetaDataDTO[] SetUpdateMetadata(MetaDataDTO[] eventMetadata, JobLightDTO getJob)
        {
            long id = 0;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFK_EVENT_PARENT").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 20;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTBEGDTTM").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = DateTime.UtcNow.ToString();
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTENDDTTM").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = DateTime.UtcNow.AddDays(10).ToString();
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCDURATIONHOURS").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.ActualJobDurationDays;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCEVENTORDER").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 20;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCWORKORDERID").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "WRKORDID_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCFIELDSERVICEORDERID").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "FLDSRVID_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCRESPONSIBLEPERSON").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "RESPONSIBLE_PERSON_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCPERSONPERFORMINGTASK").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "PERFORMING_TASK_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCQUANTITY").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 60m;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCTOTALCOST").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 200m;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCDOCUMENTFILENAME").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "EVCDOCUMENTFILENAME_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCORIGINKEY").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = "EVCORIGINKEY_";
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCUNPLANNED").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = true;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCTROUBLE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = true;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCPREVENTIVEMAINTENANCE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = true;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EVCREMARKS").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.JobRemarks;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_BUSINESSORGANIZATION").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = getJob.BusinessOrganizationId;
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_WBFLOWAREA").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_GASTYPE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_DIVERTERTYPE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_DISPLACEMENTFLUID_PREFLUSH").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_ACIDTYPE").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value
            id = eventMetadata.SingleOrDefault(x => x.ColumnName.Trim().ToUpper() == "EACFK_R_DISPLACEMENTFLUID").ID;
            eventMetadata.SingleOrDefault(x => x.ID == id).DataValue = 1; // Todo - Method to Get Value

            return eventMetadata.ToArray();
        }

        public MetaDataDTO[] CreateEvent(string jobId, int totalEvents = 1)
        {
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            EventTypeDTO EventsbyJobType = JobAndEventService.GetAllEventTypesForJobType(getJob.JobTypeId.ToString()).FirstOrDefault();
            MetaDataDTO[] eventMetadata = JobAndEventService.GetMetaDatasForCreateEventType(EventsbyJobType.EventTypeId.ToString());
            Trace.WriteLine("No. of input fields for the Event type_ : " + EventsbyJobType.EventTypeName + " is : " + eventMetadata.Count());
            eventMetadata = SetMetadata(eventMetadata, getJob);
            foreach (MetaDataDTO e in eventMetadata)
            {
                e.InstanceId = getJob.JobId;
                e.InstanceTypeId = EventsbyJobType.EventTypeId;
            }
            int count = 0;
            while (totalEvents > 0)
            {
                string eventId = JobAndEventService.AddEvent(eventMetadata);
                Assert.IsNotNull(eventId, "Failed to create Event");
                totalEvents = totalEvents - 1;
                count = count + 1;
            }
            EventGroupDTO[] allEvents = JobAndEventService.GetEvents(getJob.JobId.ToString());
            Assert.AreEqual(count, allEvents.FirstOrDefault().EventData.Count());

            return eventMetadata;
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthAddEvent()
        {
            JobLightDTO job = AddWellAssemblyJob();
            //Add Job
            string addJob = JobAndEventService.AddJob(job);
            Assert.IsNotNull(addJob, "Failed to add a Job");
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    CreateEvent(addJob);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthUpdateRemoveEvent()
        {
            JobLightDTO job = AddWellAssemblyJob();
            //Add Job
            string addJob = JobAndEventService.AddJob(job);
            Assert.IsNotNull(addJob, "Failed to add a Job");
            CreateEvent(addJob);
            EventGroupDTO[] addedEvents = JobAndEventService.GetEvents(addJob);
            Assert.AreEqual(1, addedEvents.Count(), "Failed to Add Event");
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    //Update Event
                    JobLightDTO getJob = JobAndEventService.GetJobById(addJob);
                    EventGroupDTO[] updatedEvents = JobAndEventService.GetEvents(addJob);
                    MetaDataDTO[] updatedeventMetadata = JobAndEventService.GetMetaDatasForUpdateEvent(updatedEvents.FirstOrDefault().EventData.FirstOrDefault().Id.ToString());
                    updatedeventMetadata = SetUpdateMetadata(updatedeventMetadata, getJob);
                    foreach (MetaDataDTO e in updatedeventMetadata)
                    {
                        e.InstanceId = getJob.JobId;
                        e.InstanceTypeId = updatedEvents.FirstOrDefault().EventData.FirstOrDefault().EventTypeId;
                    }
                    JobAndEventService.UpdateEvent(updatedeventMetadata);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
                try
                {
                    s_suppressWebErrorMessages = true;
                    //Remove Event
                    foreach (EventGroupDTO eg in addedEvents)
                    {
                        foreach (EventDTO e in eg.EventData)
                        {
                            bool removeEvent = JobAndEventService.RemoveJobEventData(e.Id.ToString());
                            Assert.IsTrue(removeEvent, "Failed to remove event");
                        }
                    }
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        public EventDetailCostDTO CreateEventDetailCostDTO(string jobId)
        {
            EventDetailCostDTO eventCost = new EventDetailCostDTO();

            eventCost.JobId = Convert.ToInt64(jobId);
            eventCost.Cost = 13;
            eventCost.CostRemarks = "Test Cost Remark";
            eventCost.Discount = 3;
            eventCost.Quantity = 8;
            eventCost.UnitPrice = 2;
            eventCost.VendorId = 3;
            eventCost.CostDate = DateTime.Today.AddDays(-10);

            return eventCost;
        }

        public EventDTO SetEventDTO(JobLightDTO getJob, long eId)
        {
            EventDTO e = new EventDTO();
            e.EndTime = DateTime.UtcNow.AddDays(5);
            e.EventTypeId = eId;
            e.PK_AFE = getJob.AFEId;
            e.PK_Assembly = getJob.AssemblyId;
            e.PK_BusinessOrganization = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            e.PK_CatalogItem = JobAndEventService.GetCatalogItemGroupData().CatalogItems.FirstOrDefault().CatalogItemId;
            e.PK_Job = getJob.JobId;
            e.PK_TruckUnit = JobAndEventService.GetCatalogItemGroupData().TruckUnits.FirstOrDefault().TruckUnitId;
            e.TotalCost = getJob.TotalCost;

            return e;
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthAddJobCostDetails()
        {
            JobLightDTO job = AddWellAssemblyJob();
            //Add Job
            string addJob = JobAndEventService.AddJob(job);
            Assert.IsNotNull(addJob, "Failed to add a Job");
            JobLightDTO getJob = JobAndEventService.GetJobById(addJob);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.JobCostDetailReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.JobCostDetailReport);
            long check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
            JobLightDTO[] Job = JobAndEventService.GetJobsByWell(getJob.WellId.ToString());
            Assert.IsTrue(Job.FirstOrDefault(j => j.JobId == getJob.JobId).ExistingJobEvents.HasJobCostDetailEvent);
            EventDetailCostDTO anEventCostDetails = CreateEventDetailCostDTO(addJob);
            CatalogItemGroupDTO catalogDTO = JobAndEventService.GetCatalogItemGroupData();
            long catalogItemId = catalogDTO.CatalogItems.OrderBy(x => x.CatalogItemId).FirstOrDefault().CatalogItemId;
            anEventCostDetails.CatalogItemId = catalogItemId;
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    string addedEventCostId = JobAndEventService.AddEventDetailCost(anEventCostDetails);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthUpdateRemoveJobCostDetails()
        {
            JobLightDTO job = AddWellAssemblyJob();
            //Add Job
            string addJob = JobAndEventService.AddJob(job);
            Assert.IsNotNull(addJob, "Failed to add a Job");
            JobLightDTO getJob = JobAndEventService.GetJobById(addJob);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.JobCostDetailReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.JobCostDetailReport);
            var check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Cost Detail Report");
            JobLightDTO[] Job = JobAndEventService.GetJobsByWell(getJob.WellId.ToString());
            Assert.IsTrue(Job.FirstOrDefault(j => j.JobId == getJob.JobId).ExistingJobEvents.HasJobCostDetailEvent);
            EventDetailCostDTO anEventCostDetails = CreateEventDetailCostDTO(addJob);
            CatalogItemGroupDTO catalogDTO = JobAndEventService.GetCatalogItemGroupData();
            long catalogItemId = catalogDTO.CatalogItems.OrderBy(x => x.CatalogItemId).FirstOrDefault().CatalogItemId;
            anEventCostDetails.CatalogItemId = catalogItemId;
            string addedEventCostId = JobAndEventService.AddEventDetailCost(anEventCostDetails);
            JobCostDetailsDTO addedJobCost = JobAndEventService.GetJobCostDetailsForJob(addJob);
            EventDetailCostDTO getEventDetailCost = addedJobCost.EventDetailCostGroupData[0].EventDetailCosts.FirstOrDefault();
            //Update EventDetailCost
            EventDetailCostDTO updateEventDetailCost = new EventDetailCostDTO();
            updateEventDetailCost.Id = Convert.ToInt64(addedEventCostId);
            updateEventDetailCost.EventId = getEventDetailCost.EventId;
            updateEventDetailCost.JobId = Convert.ToInt64(addJob);
            updateEventDetailCost.CatalogItemId = catalogItemId;
            updateEventDetailCost.Cost = 20;
            updateEventDetailCost.CostRemarks = "Test Update Cost Remark";
            updateEventDetailCost.Discount = 1;
            updateEventDetailCost.Quantity = 7;
            updateEventDetailCost.UnitPrice = 3;
            updateEventDetailCost.VendorId = 3;
            updateEventDetailCost.CostDate = DateTime.Today.AddDays(-8);
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    JobAndEventService.UpdateEventDetailCost(updateEventDetailCost);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
                try
                {
                    s_suppressWebErrorMessages = true;
                    JobAndEventService.RemoveEventDetailCost(addedEventCostId);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        public JobEconomicAnalysisDTO EconomicAnalysisDTO(JobLightDTO getJob, CommonProductTypesDTO commonProductTypes)
        {
            JobEconomicAnalysisDTO jobEconomicAnalysis = new JobEconomicAnalysisDTO();
            jobEconomicAnalysis.JobId = getJob.JobId;
            jobEconomicAnalysis.ProductionForecastScenarioId = 1;
            jobEconomicAnalysis.ProductForecastScenarioWaterId = 1;
            jobEconomicAnalysis.ProductForecastScenarioGasId = 1;
            jobEconomicAnalysis.ProductPriceScenarioId = JobAndEventService.GetProductPriceScenarios(commonProductTypes.OilProductTypeId.Value.ToString()).FirstOrDefault().Id;
            jobEconomicAnalysis.ProductPriceScenarioWaterId = JobAndEventService.GetProductPriceScenarios(commonProductTypes.WaterProductTypeId.Value.ToString()).FirstOrDefault().Id;
            jobEconomicAnalysis.ProductPriceScenarioGasId = JobAndEventService.GetProductPriceScenarios(commonProductTypes.GasProductTypeId.Value.ToString()).FirstOrDefault().Id;
            jobEconomicAnalysis.TaxRateId = JobAndEventService.GetStateTax().FirstOrDefault().Id;
            jobEconomicAnalysis.ProductTypeId = commonProductTypes.OilProductTypeId.Value;
            jobEconomicAnalysis.AnalysisDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Local);
            jobEconomicAnalysis.DPICalcOverriden = true;

            return jobEconomicAnalysis;
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthAddEconomicAnalysis()
        {
            JobLightDTO job = AddWellAssemblyJob();
            //Add Job
            string addJob = JobAndEventService.AddJob(job);
            Assert.IsNotNull(addJob, "Failed to add a Job");
            JobLightDTO getJob = JobAndEventService.GetJobById(addJob);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.EconomicAnalysisPlan);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.EconomicAnalysisPlan);
            var check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Economic Analysis Plan");
            JobLightDTO[] Job = JobAndEventService.GetJobsByWell(getJob.WellId.ToString());
            Assert.IsTrue(Job.FirstOrDefault(j => j.JobId == getJob.JobId).ExistingJobEvents.HasEconomicAnalysisEvent);
            CommonProductTypesDTO commonProductTypes = JobAndEventService.GetCommonProductTypes();
            JobEconomicAnalysisDTO jobEconomicAnalysis = EconomicAnalysisDTO(getJob, commonProductTypes);
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    bool status = JobAndEventService.AddUpdateJobEconomicAnalysis(jobEconomicAnalysis);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        public EventDetailScheduledActivityDTO CreateJobPlanDetailDTO(string jobId)
        {
            EventDetailScheduledActivityDTO eventJobPlan = new EventDetailScheduledActivityDTO();
            eventJobPlan.JobId = Convert.ToInt64(jobId);
            CatalogItemGroupDTO catalogDTO = JobAndEventService.GetCatalogItemGroupData();
            eventJobPlan.VendorId = catalogDTO.CatalogItems.OrderBy(x => x.CatalogItemId).FirstOrDefault().VendorId;
            eventJobPlan.CatalogItemId = catalogDTO.CatalogItems.OrderBy(x => x.CatalogItemId).FirstOrDefault().CatalogItemId;
            eventJobPlan.TruckUnitId = catalogDTO.TruckUnits.Single(x => x.VendorId == eventJobPlan.VendorId).TruckUnitId;
            eventJobPlan.UOMId = JobAndEventService.GetUnitsOfMeasure().FirstOrDefault().UOMId;
            eventJobPlan.Remarks = "Test Remarks";
            eventJobPlan.Description = "Test Description";
            return eventJobPlan;
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthAddJobPlan()
        {
            JobLightDTO job = AddWellAssemblyJob();
            //Add Job
            string addJob = JobAndEventService.AddJob(job);
            Assert.IsNotNull(addJob, "Failed to add a Job");
            JobLightDTO getJob = JobAndEventService.GetJobById(addJob);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.JobPlan);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.JobPlan);
            var check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Plan");
            JobLightDTO[] Job = JobAndEventService.GetJobsByWell(getJob.WellId.ToString());
            Assert.IsTrue(Job.FirstOrDefault(j => j.JobId == getJob.JobId).ExistingJobEvents.HasJobPlanEvent);
            EventDetailScheduledActivityDTO addJobPlanDetail = CreateJobPlanDetailDTO(addJob);
            addJobPlanDetail.EventTypeId = JobAndEventService.GetAllEventTypesForJobType(getJob.JobTypeId.ToString()).FirstOrDefault().EventTypeId;
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    JobAndEventService.AddJobPlanDetail(addJobPlanDetail);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthUpdateRemoveJobPlan()
        {
            JobLightDTO job = AddWellAssemblyJob();
            //Add Job
            string addJob = JobAndEventService.AddJob(job);
            Assert.IsNotNull(addJob, "Failed to add a Job");
            JobLightDTO getJob = JobAndEventService.GetJobById(addJob);
            //EventDTO evt = SetEventDTO(getJob, (int)JobEventTypes.JobPlan);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.JobPlan);
            var check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Job Plan");
            JobLightDTO[] Job = JobAndEventService.GetJobsByWell(getJob.WellId.ToString());
            Assert.IsTrue(Job.FirstOrDefault(j => j.JobId == getJob.JobId).ExistingJobEvents.HasJobPlanEvent);
            EventDetailScheduledActivityDTO addJobPlanDetail = CreateJobPlanDetailDTO(addJob);
            addJobPlanDetail.EventTypeId = JobAndEventService.GetAllEventTypesForJobType(getJob.JobTypeId.ToString()).FirstOrDefault().EventTypeId;
            string addedJobPlanId = JobAndEventService.AddJobPlanDetail(addJobPlanDetail);
            JobPlanDetailsDTO addedJobPlanDetails = JobAndEventService.GetJobPlanDetails(addJob);
            EventDetailScheduledActivityDTO getJobPlanDetail = addedJobPlanDetails.EventPlanDetails[0];
            getJobPlanDetail.Remarks = "Test Update Remark";
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    List<EventDetailScheduledActivityDTO> getJobPlanDetails = new List<EventDetailScheduledActivityDTO>();
                    getJobPlanDetails.Add(getJobPlanDetail);

                    JobAndEventService.UpdateJobPlanDetail(getJobPlanDetails.ToArray());
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
                try
                {
                    s_suppressWebErrorMessages = true;
                    JobAndEventService.RemoveJobPlanDetail(getJobPlanDetail.Id.ToString());
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        public WellConfigDTO AddWell(string facilityIdBase)
        {
            string facilityId = s_isRunningInATS ? facilityIdBase + "00001" : facilityIdBase + "0001";
            WellConfigDTO well = WellConfigurationService.AddWellConfig(new WellConfigDTO()
            {
                Well = SetDefaultFluidType(new WellDTO()
                {
                    Name = DefaultWellName,
                    FacilityId = facilityId,
                    DataConnection = GetDefaultCygNetDataConnection(),
                    SubAssemblyAPI = "SubAssemblyAPI",
                    AssemblyAPI = "AssemblyAPI",
                    CommissionDate = DateTime.Today,
                    WellType = WellTypeId.RRL,
                })
            });
            Assert.IsNotNull(well);
            _wellsToRemove.Add(well.Well);
            return well;
        }

        public ComponentMetaDataGroupDTO[] AddComponent(string jobId, string wellId, string assemblyId, string subassemblyId, long eventId, int topdepth = 0, int bottomdepth = 500)
        {
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);

            //Add Components
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");
            foreach (RRLPartTypeComponentGroupingTypeDTO cg in partTypes)
            {
                Assert.AreEqual("Surface Equipment", cg.strComponentGrouping);
            }

            //Meta Data
            //9/3/2019 - To avoid compilation error, passed an added parameter as null. Integration test needs to be fixed according to FRWM-5489
            //string hardCodedComponentGroupId = null;
            ComponentMetaDataGroupDTO[] cmpMetaData = ComponentService.GetComponentMetaDataForAdd(partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType.ToString(), partfilter.TypeId.ToString());
            Assert.IsNotNull(cmpMetaData);
            MetaDataDTO[] cdReference = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;

            //Get Meta data for the Catalog Item description
            MetaDataReferenceData cd = new MetaDataReferenceData();
            cd.MetaData = cdReference.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCatalogItem");
            MetaDataFilterDTO cdFilter = new MetaDataFilterDTO();
            cdFilter.ColumnValue = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType.ToString();
            cdFilter.MetaDataFilterToken = cd.MetaData.ExtendedFilterInput;
            List<MetaDataFilterDTO> listcdFilter = new List<MetaDataFilterDTO>();
            listcdFilter.Add(cdFilter);
            cd.UIFilterValues = listcdFilter.ToArray();
            ControlIdTextDTO[] cdMetaData = JobAndEventService.GetMetaDataReferenceData(cd);
            Assert.AreEqual(1, cdMetaData.Count());
            ControlIdTextDTO cdm = cdMetaData.FirstOrDefault(x => x.ControlText == "Transformer");

            //Get Meta data for the Component Grouping
            cd.MetaData = cdReference.FirstOrDefault(x => x.ColumnName == "ascFK_c_ComponentGrouping");
            cd.UIFilterValues = null;
            ControlIdTextDTO[] cgMetaData = JobAndEventService.GetMetaDataReferenceData(cd);
            Assert.IsNotNull(cgMetaData);

            //Get Meta data for the Sub Assembly
            cd.MetaData = cdReference.FirstOrDefault(x => x.ColumnName == "ascFK_SubAssembly");
            SubAssemblyDTO[] getSubAssemblies = WellboreComponentService.GetSubAssembliesByWellId(wellId);
            Assert.IsNotNull(getSubAssemblies);
            MetaDataFilterDTO saFilter = new MetaDataFilterDTO();
            cdFilter.ColumnValue = assemblyId;
            cdFilter.MetaDataFilterToken = cd.MetaData.ExtendedFilterInput;
            List<MetaDataFilterDTO> listsaFilter = new List<MetaDataFilterDTO>();
            listsaFilter.Add(cdFilter);
            cd.UIFilterValues = listsaFilter.ToArray();
            ControlIdTextDTO[] saMetaData = JobAndEventService.GetMetaDataReferenceData(cd);
            Assert.IsNotNull(saMetaData);
            Assert.AreEqual(getSubAssemblies.Count(), saMetaData.Count());

            ComponentMetaDataGroupDTO reqComponent = new ComponentMetaDataGroupDTO();
            reqComponent.CategoryName = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").CategoryName;
            reqComponent.JobId = Convert.ToInt64(jobId);
            reqComponent.EventId = eventId;
            reqComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
            reqComponent.ExtendedAssemblyComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptyExtendedAssemblyComponentTableName;
            reqComponent.ExtendedComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptyExtendedComponentTableName;
            reqComponent.Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Required").Fields;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCatalogItem").DataValue = cdm.ControlId;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcCompName").DataValue = "Transformer";
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_Manufacturers").DataValue = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcFK_c_MfgCat_PartType").DataValue = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcInnerDiameter").DataValue = 1.5m;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "cmcOuterDiameter").DataValue = 2.5m;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_Assembly").DataValue = assemblyId;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascTopDepth").DataValue = topdepth;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascBottomDepth").DataValue = bottomdepth;
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascEndEventDT").DataValue = getJob.EndDate.AddDays(50).ToString();
            reqComponent.Fields.FirstOrDefault(x => x.ColumnName == "ascFK_SubAssembly").DataValue = subassemblyId;
            reqComponent.PartTypePrimaryKey = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;
            reqComponent.Order = 1;
            reqComponent.PartType = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptyPartType;
            ComponentMetaDataGroupDTO addComponent = new ComponentMetaDataGroupDTO();
            addComponent.CategoryName = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Additional").CategoryName;
            addComponent.JobId = Convert.ToInt64(jobId);
            addComponent.ComponentGroupingPrimaryKey = partfilter.TypeId;
            addComponent.ExtendedAssemblyComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptyExtendedAssemblyComponentTableName;
            addComponent.ExtendedComponentTable = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptyExtendedComponentTableName;
            addComponent.Fields = cmpMetaData.FirstOrDefault(x => x.CategoryName == "Additional").Fields;
            addComponent.PartTypePrimaryKey = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;
            addComponent.Order = 1;
            addComponent.PartType = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptyPartType;
            List<ComponentMetaDataGroupDTO> listComponent = new List<ComponentMetaDataGroupDTO>();
            listComponent.Add(reqComponent);
            listComponent.Add(addComponent);
            ComponentMetaDataGroupDTO[] arrComponent = listComponent.ToArray();

            return arrComponent;
        }

        public string AddReport()
        {
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));
            Assert.IsNotNull(well);
            AssemblyDTO getAssembly = WellboreComponentService.GetAssemblyByWellId(well.Id.ToString());
            Assert.IsNotNull(getAssembly);
            ReportDTO newReport = new ReportDTO
            {
                WellId = well.Id,
                AssemblyId = getAssembly.Id,
                JobReasonId = 309,
                JobTypeId = 12,
                ReportTypeId = 3,
                OffDate = DateTime.Today.AddDays(-5),
                WorkoverDate = DateTime.Today,
                Comment = "Add Report",
            };

            WellboreComponentService.AddReport(newReport);
            ReportDTO reportExists = WellboreComponentService.GetReportByWellId(well.Id.ToString());
            Assert.IsNotNull(reportExists);
            Assert.AreEqual(reportExists.ReportTypeId, 3);

            return reportExists.Id.ToString();
        }

        public string AddJob(string jobStatus, int startDate = 0)
        {
            List<WellDTO> allWells = WellService.GetAllWells().ToList();
            WellDTO well = allWells?.FirstOrDefault(w => w.Name.Equals(DefaultWellName));

            JobLightDTO job = new JobLightDTO();
            job.WellId = well.Id;
            job.WellName = well.Name;
            job.BeginDate = DateTime.Today.AddDays(startDate).ToUniversalTime();
            if (startDate == 0)
                job.EndDate = DateTime.Today.AddDays(30).ToUniversalTime();
            else
                job.EndDate = DateTime.Today.AddDays(startDate + 30).ToUniversalTime();
            job.ActualCost = (decimal)100000.00;
            job.ActualJobDurationDays = (decimal)20.5;
            job.TotalCost = (decimal)150000.00;
            job.JobRemarks = "TestJobRemarks " + DateTime.UtcNow.ToString();
            job.JobOrigin = "TestJobOrigin ";
            job.AssemblyId = well.AssemblyId;
            //For Below fields User can select any value in the dropdown menu
            job.AFEId = JobAndEventService.GetAFEs().FirstOrDefault().Id;
            job.StatusId = JobAndEventService.GetJobStatuses().FirstOrDefault(j => j.Name == jobStatus).Id;
            job.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault().id;
            job.BusinessOrganizationId = JobAndEventService.GetCatalogItemGroupData().Vendors.FirstOrDefault().Id;
            job.AccountRef = "1";
            //JobReasonId drop down selection is based on the JobTypeId
            job.JobReasonId = JobAndEventService.GetJobReasonsForJobType(job.JobTypeId.ToString()).FirstOrDefault().Id;
            job.JobDriverId = (int)JobAndEventService.GetPrimaryMotivationsForJob().FirstOrDefault().Id;
            //Add Job
            string addJob = JobAndEventService.AddJob(job);
            Assert.IsNotNull(addJob, "Failed to add a Job");

            return addJob;
        }

        public string Addcomponent(ref long wbcheck)
        {
            WellConfigDTO well = AddWell("RPOC_");
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO wbevt = SetEventDTO(getJob, (int)JobEventTypes.WellBoreReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO wbevt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            wbcheck = JobAndEventService.AddEventForJobEventType(wbevt);
            Assert.IsTrue(wbcheck > 0, "Failed to add Wellbore Report");
            EventDTO evt = SetEventDTO(getJob, jobEventTypes.FailureReport);
            var check = JobAndEventService.AddEventForJobEventType(evt);
            Assert.IsTrue(check > 0, "Failed to add Failure Report");
            //Add Component
            ComponentMetaDataGroupDTO[] arrComponent = AddComponent(jobId, wellId, assemblyId, subassemblyId, wbcheck);
            bool addComp = ComponentService.AddComponent(arrComponent);
            Assert.IsTrue(addComp);

            return jobId;
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthAddFailureObservation()
        {
            long evtId = 0;
            string jobId = Addcomponent(ref evtId);
            string failCompJobId = AddJob("Approved", 31);

            //Get Failure Component
            JobComponentFailureDTO[] Comps = JobAndEventService.GetJobComponentFailure(failCompJobId);
            Assert.IsNotNull(Comps, "Failed to get components");
            Assert.AreEqual(1, Comps.Count());
            //Get Metadata for observation
            JobComponentFailureObservationDTO mdFailObserv = JobAndEventService.GetMetaDataForAddObservation(Comps.FirstOrDefault().AssemblyComponentId.ToString(), jobId);
            Assert.IsNotNull(mdFailObserv, "Failed to get Observation for the added component");
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue = false;

            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    string failObservationCompId = JobAndEventService.AddObservation(mdFailObserv);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthAddComponent()
        {
            WellConfigDTO well = AddWell("RPOC_");
            string wellId = well.Well.Id.ToString();
            string assemblyId = well.Well.AssemblyId.ToString();
            string subassemblyId = well.Well.SubAssemblyId.ToString();
            string jobId = AddJob("Approved");
            JobLightDTO getJob = JobAndEventService.GetJobById(jobId);
            //EventDTO wbevt = SetEventDTO(getJob, (int)JobEventTypes.WellBoreReport);
            //It seems that JobEventTypes enumerator is removed instead GetJobEventTypes() method is developed to get the jobEventTypes
            JobEventTypeDTO jobEventTypes = JobAndEventService.GetJobEventTypes();
            EventDTO wbevt = SetEventDTO(getJob, jobEventTypes.WellBoreReport);
            var wbcheck = JobAndEventService.AddEventForJobEventType(wbevt);
            Assert.IsTrue(wbcheck > 0, "Failed to add Wellbore Report");
            ComponentMetaDataGroupDTO[] arrComponent = AddComponent(jobId, wellId, assemblyId, subassemblyId, wbcheck);

            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    ComponentService.AddComponent(arrComponent);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthUpdateRemoveComponent()
        {
            long evtId = 0;
            string jobId = Addcomponent(ref evtId);
            WellboreGridDTO[] wellboreComponent = JobAndEventService.GetWellboreComponentByJobIdAndEventId(jobId, evtId.ToString());
            Assert.IsNotNull(wellboreComponent);
            Assert.AreEqual(1, wellboreComponent.Count());
            ComponentFilterDTO groupfilter = new ComponentFilterDTO();
            groupfilter.ContainsSearchText = "";
            groupfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] componentGroups = ComponentService.GetComponentGroups(groupfilter);
            Assert.IsNotNull(componentGroups, "Failed to retrieve Component groups");
            ComponentFilterDTO partfilter = new ComponentFilterDTO();
            partfilter.ContainsSearchText = "";
            partfilter.TypeId = componentGroups.FirstOrDefault(x => x.strComponentGrouping == "Surface Equipment").ptgFK_c_MfgCat_ComponentGrouping;
            partfilter.TypeOfFilter = ComponentFilterTypes.GroupType;
            RRLPartTypeComponentGroupingTypeDTO[] partTypes = ComponentService.GetPartTypes(partfilter);
            Assert.IsNotNull(partTypes, "Failed to retrieve Part types");
            string ptyId = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType.ToString();
            ComponentMetaDataGroupDTO mdComp = wellboreComponent.FirstOrDefault(x => x.SubAssemblyDesc == "Primary Wellbore").WellboreGridGroup.FirstOrDefault().ComponentMetadata.FirstOrDefault(x => x.CategoryName == "Required");
            string cId = mdComp.ComponentPrimaryKey.ToString();
            string ascId = mdComp.Fields.FirstOrDefault(x => x.ColumnName == "ascPrimaryKey").DataValue.ToString();
            ComponentMetaDataGroupDTO[] metadataUpdate = ComponentService.GetComponentMetaDataForUpdate(ptyId, cId, ascId);
            foreach (ComponentMetaDataGroupDTO md in metadataUpdate)
            {
                md.JobId = Convert.ToInt64(jobId);
            }
            MetaDataDTO[] reqFields = metadataUpdate.FirstOrDefault(x => x.CategoryName == "Required").Fields;
            reqFields.FirstOrDefault(x => x.ColumnName == "ascRemark").DataValue = "Component added by Automation Test";

            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    ComponentService.UpdateComponent(metadataUpdate);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
                try
                {
                    s_suppressWebErrorMessages = true;
                    //Remove Component
                    ComponentPartTypeDTO Ids = new ComponentPartTypeDTO();
                    Ids.JobId = Convert.ToInt64(jobId);
                    Ids.PartTypeId = partTypes.FirstOrDefault(x => x.ptyPartType == "Transformer").ptgFK_c_MfgCat_PartType;
                    Ids.ComponentId = mdComp.ComponentPrimaryKey;
                    Ids.EventId = evtId;
                    ComponentService.RemoveComponent(Ids);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthUpdateRemoveFailureObservation()
        {
            long evtId = 0;
            string jobId = Addcomponent(ref evtId);
            string failCompJobId = AddJob("Approved", 31);

            //Get Failure Component
            JobComponentFailureDTO[] Comps = JobAndEventService.GetJobComponentFailure(failCompJobId);
            Assert.IsNotNull(Comps, "Failed to get components");
            Assert.AreEqual(1, Comps.Count());
            //Get Metadata for observation
            JobComponentFailureObservationDTO mdFailObserv = JobAndEventService.GetMetaDataForAddObservation(Comps.FirstOrDefault().AssemblyComponentId.ToString(), jobId);
            Assert.IsNotNull(mdFailObserv, "Failed to get Observation for the added component");
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue = false;
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfFailureDate").DataValue = DateTime.Today.ToString();
            string failObservationCompId = JobAndEventService.AddObservation(mdFailObserv);
            Assert.IsNotNull(failObservationCompId);
            //Update observations (making the added observation as a primary cause of failure)
            mdFailObserv.ObservationFields.FirstOrDefault(x => x.ColumnName == "acfPrimaryCauseOfFailure").DataValue = true;

            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    JobAndEventService.UpdateObservation(mdFailObserv);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
                try
                {
                    s_suppressWebErrorMessages = true;
                    JobAndEventService.RemoveObservation(failObservationCompId);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthDocumentGrouping()
        {
            DocumentGroupingDTO[] documentGroupings = DocumentService.GetDocumentGroupings();
            DocumentGroupingDTO dgpng = new DocumentGroupingDTO();
            dgpng.DocMaxFileSize = 200;
            dgpng.FilePath = documentGroupings.FirstOrDefault().FilePath + "AddedGrouping";
            dgpng.GroupingName = "UserAddedGroup";
            dgpng.RestrictedFileTypes = ".exe";
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    DocumentService.AddUpdateDocumentGrouping(dgpng);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthAddTemplateJob()
        {
            TemplateJobDTO templateJob = new TemplateJobDTO();
            JobLightDTO newJob = new JobLightDTO();
            newJob.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault().id;
            newJob.JobReasonId = JobAndEventService.GetJobReasonsForJobType(JobAndEventService.GetJobTypes().FirstOrDefault().id.ToString()).FirstOrDefault().Id;
            templateJob.Job = newJob;
            templateJob.Category = "JobCategory-1";
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    JobAndEventService.AddTemplateJob(templateJob);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthUpdateRemoveTemplateJob()
        {
            TemplateJobDTO templateJob = new TemplateJobDTO();
            JobLightDTO newJob = new JobLightDTO();
            newJob.JobTypeId = JobAndEventService.GetJobTypes().FirstOrDefault().id;
            newJob.JobReasonId = JobAndEventService.GetJobReasonsForJobType(JobAndEventService.GetJobTypes().FirstOrDefault().id.ToString()).FirstOrDefault().Id;
            templateJob.Job = newJob;
            templateJob.Category = "JobCategory-1";
            string templateID = JobAndEventService.AddTemplateJob(templateJob);
            Assert.IsNotNull(templateID, "Template job not added successsfully");
            templateJob.Category = "JobCategory-2";
            templateJob.TemplateJobId = Convert.ToInt64(templateID);
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();
                Assert.AreEqual(AuthenticatedUser.Id, validateUser.Id, "User id from ValidateToken call does not match id from CreateToken");
                Assert.AreEqual(AuthenticatedUser.Name, validateUser.Name, "User name from ValidateToken call does not match name from CreateToken");
                try
                {
                    s_suppressWebErrorMessages = true;
                    JobAndEventService.UpdateTemplateJob(templateJob);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
                try
                {
                    s_suppressWebErrorMessages = true;
                    JobAndEventService.RemoveTemplateJob(templateID);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }
            }
        }

        #endregion Authorization Tests for WSM

        [TestCategory(TestCategories.AuthorizationNegativeTests), TestMethod]
        public void UserAuthSharedGridViewLayout()
        {
            const GenericGridId gridId = GenericGridId.ReadyJobStatus;

            //Create new Role
            PermissionDTO permission = AdministrationService.GetPermission(((long)PermissionId.SaveSystemGridViews).ToString());
            RoleDTO newRole = new RoleDTO();
            newRole.Name = "Grid View Role";
            newRole.Permissions = new List<PermissionDTO>();
            newRole.Permissions.Add(permission);
            AdministrationService.AddRole(newRole);

            RoleDTO[] roles = AdministrationService.GetRoles();
            RoleDTO addedRole = roles.Where(x => x.Name == newRole.Name).FirstOrDefault();
            _rolesToRemove.Add(addedRole);

            //Create View1
            GenericGridViewDTO view1 = new GenericGridViewDTO();
            view1.GridId = gridId;
            view1.Name = "Shared View";
            GenericGridViewDTO sharedViewFromSave = SettingService.SaveGenericGridView(view1);
            _viewsToRemove.Add(sharedViewFromSave);
            //Update View1 set as Shared
            sharedViewFromSave.UserId = null;
            sharedViewFromSave = SettingService.SaveGenericGridView(sharedViewFromSave);

            //Create View2 as not shared
            GenericGridViewDTO view2 = new GenericGridViewDTO();
            view2.UserId = AuthenticatedUser.Id;
            view2.GridId = gridId;
            view2.Name = "Non-Shared View";
            GenericGridViewDTO nonSharedViewFromSave = SettingService.SaveGenericGridView(view2);
            _viewsToRemove.Add(nonSharedViewFromSave);

            //Check Views
            //Check View1 is shared
            Assert.IsNull(sharedViewFromSave.UserId);

            //Check View2 is not shared
            Assert.IsNotNull(nonSharedViewFromSave.UserId);

            long userId;

            //Change User
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();

                //Make sure user does not have PermissionId.SaveSystemGridViews
                PermissionDTO missingPermission = validateUser.Permissions.Where(x => x.Id == PermissionId.SaveSystemGridViews).FirstOrDefault();
                Assert.IsNull(missingPermission);

                try
                {
                    //Try to update View1 -Should fail.
                    sharedViewFromSave.Name = "Failed Update";
                    sharedViewFromSave = SettingService.SaveGenericGridView(sharedViewFromSave);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }

                try
                {
                    //Try to delete View1 -Should fail.
                    SettingService.RemoveGenericGridViews(new long[] { sharedViewFromSave.Id });
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.Forbidden);
                }

                try
                {
                    //Try to update View2 -Should fail.
                    nonSharedViewFromSave.Name = "Failed Update";
                    nonSharedViewFromSave = SettingService.SaveGenericGridView(nonSharedViewFromSave);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.InternalServerError);
                }

                try
                {
                    //Try to delete View2 -Should fail.
                    SettingService.RemoveGenericGridViews(new long[] { nonSharedViewFromSave.Id });
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.InternalServerError);
                }

                userId = validateUser.Id;
            }

            //Add PermissionId.SaveSystemGridViews
            UserDTO user = AdministrationService.GetUser(userId.ToString());
            user.Roles.Add(addedRole);
            AdministrationService.UpdateUser(user);

            //Change User
            using (var iU = ImpersonateTestUser())
            {
                // Show new user identity of impersonated user
                Trace.WriteLine("Checking Identity AFTER Impersonation.");
                CheckIdentity();
                Authenticate();
                AuthenticatedUserDTO validateUser = TokenService.ValidateToken();

                //Try to update View1 -Should work.
                sharedViewFromSave.Name = "Successful Update";
                sharedViewFromSave = SettingService.SaveGenericGridView(sharedViewFromSave);
                Assert.IsTrue(sharedViewFromSave.Name == "Successful Update");

                //Try to delete View1 -Should work.
                SettingService.RemoveGenericGridViews(new long[] { sharedViewFromSave.Id });
                GenericGridViewDTO[] gridViewList = SettingService.GetGenericGridViews(((long)gridId).ToString());
                Assert.IsTrue(gridViewList.Where(x => x.Id == sharedViewFromSave.Id).Count() == 0);

                try
                {
                    //Try to update View2 -Should fail.
                    nonSharedViewFromSave.Name = "Failed Update";
                    nonSharedViewFromSave = SettingService.SaveGenericGridView(nonSharedViewFromSave);
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.InternalServerError);
                }

                try
                {
                    //Try to delete View2 -Should fail.
                    SettingService.RemoveGenericGridViews(new long[] { nonSharedViewFromSave.Id });
                    Assert.Fail("Expected exception not thrown");
                }
                catch (WebException e)
                {
                    CheckResponseCode(e, HttpStatusCode.InternalServerError);
                }
            }

            UserDTO updatedUser = AdministrationService.GetUser(user.Id.ToString());
            updatedUser.Roles.Clear();
            AdministrationService.UpdateUser(updatedUser);
        }

        private static ImpersonatorScoped ImpersonateTestUser()
        {
            return new ImpersonatorScoped(s_negativeAuthDomain, s_negativeAuthUser, s_negativeAuthPassword);
        }

        public static void CheckIdentity()
        {
            Trace.WriteLine("Current user: " + WindowsIdentity.GetCurrent().Name);
        }
    }

    public class Impersonator
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private extern static bool CloseHandle(IntPtr handle);

        private IntPtr _tokenHandle = new IntPtr(0);
        private WindowsImpersonationContext _impersonatedUser;

        // If you incorporate this code into a DLL, be sure to demand that it
        // runs with FullTrust.
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void Impersonate(string domainName, string userName, string password)
        {
            try
            {
                // Use the unmanaged LogonUser function to get the user token for
                // the specified user, domain, and password.
                const int LOGON32_PROVIDER_DEFAULT = 0;

                // Passing this parameter causes LogonUser to create a primary token.
                const int LOGON32_LOGON_INTERACTIVE = 2;
                _tokenHandle = IntPtr.Zero;

                // Step -1 Call LogonUser to obtain a handle to an access token.
                bool returnValue = LogonUser(
                    userName,
                    domainName,
                    password,
                    LOGON32_LOGON_INTERACTIVE,
                    LOGON32_PROVIDER_DEFAULT,
                    ref _tokenHandle);         // tokenHandle - new security token

                if (false == returnValue)
                {
                    int ret = Marshal.GetLastWin32Error();
                    Trace.WriteLine("LogonUser call failed with error code : " + ret);
                    throw new System.ComponentModel.Win32Exception(ret);
                }

                // Step - 2
                WindowsIdentity newId = new WindowsIdentity(_tokenHandle);
                // Step -3
                _impersonatedUser = newId.Impersonate();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception occurred impersonating user {0}\\{1}: " + ex.Message, domainName, userName));
                throw;
            }
        }

        /// <summary>
        /// Stops impersonation
        /// </summary>
        public void Undo()
        {
            _impersonatedUser.Undo();
            Trace.WriteLine("Undoing impersonation");
            // Free the tokens.
            if (_tokenHandle != IntPtr.Zero)
            {
                CloseHandle(_tokenHandle);
            }
        }
    }

    public class ImpersonatorScoped : IDisposable
    {
        private Impersonator _impersonator;

        public ImpersonatorScoped(string domainName, string userName, string password)
        {
            _impersonator = new Impersonator();
            _impersonator.Impersonate(domainName, userName, password);
        }

        public void Dispose()
        {
            _impersonator.Undo();
        }
    }
}