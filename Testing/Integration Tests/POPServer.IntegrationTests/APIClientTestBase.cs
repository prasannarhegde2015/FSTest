using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Weatherford.POP.APIClient;
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using Weatherford.POP.Interfaces;
using Weatherford.ReoService.Interfaces;

namespace Weatherford.POP.Server.IntegrationTests
{
    public abstract partial class APIClientTestBase
    {
        protected const string DefaultWellName = "CASETest";
        protected static string s_hostname = ConfigurationManager.AppSettings.Get("Server");
        protected static bool s_isRunningInATS = ConfigurationManager.AppSettings.Get("IsRunningATS").Equals("true", StringComparison.CurrentCultureIgnoreCase);
        protected static string s_domain = ConfigurationManager.AppSettings.Get("Domain");
        protected static string s_site = ConfigurationManager.AppSettings.Get("Site");
        protected static string s_cvsService = ConfigurationManager.AppSettings.Get("CVSService");
        protected static int s_wellStart = Convert.ToInt32(ConfigurationManager.AppSettings.Get("WellStart"));
        protected static int s_wellsEnd = Convert.ToInt32(ConfigurationManager.AppSettings.Get("WellsEnd"));
        protected static bool s_isImpersonatingForAll = ConfigurationManager.AppSettings.Get("ImpersonateForAllTests").Equals("true", StringComparison.CurrentCultureIgnoreCase);
        protected static string s_impersonationDomain = ConfigurationManager.AppSettings.Get("ImpersonationDomain");
        protected static string s_impersonationUser = ConfigurationManager.AppSettings.Get("ImpersonationUser");
        protected static string s_impersonationPassword = ConfigurationManager.AppSettings.Get("ImpersonationPassword");
        protected static string s_prefixForESPFacility = ConfigurationManager.AppSettings.Get("SearchTxtForESP");
        protected static string s_prefixForGLFacility = ConfigurationManager.AppSettings.Get("SearchTxtForGL");
        protected static string s_prefixForNFWFacility = ConfigurationManager.AppSettings.Get("SearchTxtForNFW");
        protected static string s_prefixForGIFacility = ConfigurationManager.AppSettings.Get("SearchTxtForGI");
        protected static string s_prefixForWIFacility = ConfigurationManager.AppSettings.Get("SearchTxtForWI");
        protected static string s_prefixForPLFacility = ConfigurationManager.AppSettings.Get("SearchTxtForPL");
        protected static string s_deleteAllWellsBeforeTestExecution = ConfigurationManager.AppSettings.Get("DeleteAllWellsBeforeTestExecution");

        protected static bool s_suppressWebErrorMessages;
        protected static int s_currentId = 0;
        protected static object s_lock = new object();
        protected static int extendTimeout = 900000;//15 min.

        protected ClientServiceFactory _serviceFactory;
        protected List<WellDTO> _wellsToRemove;
        protected List<DataConnectionDTO> _dataConnectionsToRemove;
        protected List<SystemSettingDTO> _systemSettingsToRemove;
        protected List<string> _systemSettingNamesToRemove;
        protected List<string> _userSettingNamesToRemove;
        protected List<UserSettingDTO> _userSettingsToRemove;
        protected List<WellSettingDTO> _wellSettingsToRestore;
        protected List<SystemSettingDTO> _systemSettingsToRestore;
        protected List<UserSettingDTO> _userSettingsToRestore;
        protected List<Tuple<long, string>> _wellSettingNamesToRemove;
        protected List<WellSettingDTO> _wellSettingsToRemove;
        protected List<AssetSettingDTO> _assetSettingsToRemove;
        protected List<AssetSettingDTO> _assetSettingsToRestore;
        protected List<Tuple<long, string>> _assetSettingNamesToRemove;
        protected List<UserDTO> _usersToRemove;
        protected List<ADGroupDTO> _adGroupsToRemove;
        protected List<RoleDTO> _rolesToRemove;
        protected List<WellDTO> _wellsAtStart;
        protected List<WellAllocationGroupDTO> _wellAllocationGroupsToRemove;
        protected List<AllocationGroupDTO> _allocationGroupsToRemove;
        protected List<AssetDTO> _assetsToRemove;
        protected List<AssetDTO> _vrrHierarchyToRemove;
        protected List<ReservoirAndUnitsDTO> _reservoirsToRemove;
        protected List<string> _headerFooterRowsToRemove;
        protected List<SNScenarioDTO> _snScenariosToRemove;
        protected List<PFScenarioDTO> _pfScenariosToRemove;
        protected List<SNModelDTO> _snModelsToRemove;
        protected List<PFModelDTO> _pfModelsToRemove;
        protected List<SNScenarioScheduleDTO> _scheduleScenariosToRemove;
        protected List<string> _directoriesToRemove;
        protected List<SWASnapshotDTO> _setWellAlowableToRemove;

        static APIClientTestBase()
        {
            if (string.IsNullOrEmpty(s_impersonationDomain) && string.IsNullOrEmpty(s_impersonationUser) && string.IsNullOrEmpty(s_impersonationPassword))
            {
                s_impersonationDomain = "VSI";
                s_impersonationUser = "ATSAdmin1";
                s_impersonationPassword = "CygNet4Fun";
            }
        }

        protected static DataConnectionDTO GetDefaultCygNetDataConnection()
        {
            return new DataConnectionDTO { ProductionDomain = s_domain, Site = s_site, Service = s_cvsService, ScadaSourceType = ScadaSourceType.CygNet };
        }

        #region Services

        private ITokenService _tokenService;

        protected ITokenService TokenService
        {
            get { return _tokenService ?? (_tokenService = _serviceFactory.GetService<ITokenService>()); }
        }

        private IEquipmentConfigService _equipmentConfigurationService;

        protected IEquipmentConfigService EquipmentConfigurationService
        {
            get { return _equipmentConfigurationService ?? (_equipmentConfigurationService = _serviceFactory.GetService<IEquipmentConfigService>()); }
        }

        private IEquipmentJobService _equipmentJobService;

        protected IEquipmentJobService EquipmentJobService
        {
            get { return _equipmentJobService ?? (_equipmentJobService = _serviceFactory.GetService<IEquipmentJobService>()); }
        }

        private IDataConnectionService _dataConnectionService;

        protected IDataConnectionService DataConnectionService
        {
            get { return _dataConnectionService ?? (_dataConnectionService = _serviceFactory.GetService<IDataConnectionService>()); }
        }

        private IWellService _wellService;

        protected IWellService WellService
        {
            get { return _wellService ?? (_wellService = _serviceFactory.GetService<IWellService>()); }
        }

        private IWellInferredProductionAndAllocationService _wellInferredProductionAndAllocationService;

        protected IWellInferredProductionAndAllocationService WellInferredProductionAndAllocationService
        {
            get { return _wellInferredProductionAndAllocationService ?? (_wellInferredProductionAndAllocationService = _serviceFactory.GetService<IWellInferredProductionAndAllocationService>()); }
        }

        private ILicenseService _licenseService;

        protected ILicenseService LicenseService
        {
            get { return _licenseService ?? (_licenseService = _serviceFactory.GetService<ILicenseService>()); }
        }

        private ISettingService _settingService;

        protected ISettingService SettingService
        {
            get { return _settingService ?? (_settingService = _serviceFactory.GetService<ISettingService>()); }
        }

        private IProductionForecastService _procutionForecastService;

        protected IProductionForecastService ProductionForecastService
        {
            get { return _procutionForecastService ?? (_procutionForecastService = _serviceFactory.GetService<IProductionForecastService>()); }
        }

        private IWellAllocationService _wellAllocationService;

        protected IWellAllocationService WellAllocationService
        {
            get { return _wellAllocationService ?? (_wellAllocationService = _serviceFactory.GetService<IWellAllocationService>()); }
        }

        private IAdministrationService _administrationService;

        protected IAdministrationService AdministrationService
        {
            get { return _administrationService ?? (_administrationService = _serviceFactory.GetService<IAdministrationService>()); }
        }

        private IAuthenticationService _authenticationService;

        protected IAuthenticationService AuthenticationService
        {
            get { return _authenticationService ?? (_authenticationService = _serviceFactory.GetService<IAuthenticationService>()); }
        }

        private IModelFileService _modelFileService;

        protected IModelFileService ModelFileService
        {
            get { return _modelFileService ?? (_modelFileService = _serviceFactory.GetService<IModelFileService>()); }
        }

        private IWellConfigurationService _wellConfigurationService;

        protected IWellConfigurationService WellConfigurationService
        {
            get { return _wellConfigurationService ?? (_wellConfigurationService = _serviceFactory.GetService<IWellConfigurationService>()); }
        }

        private IWellboreComponentService _wellboreComponentService;

        protected IWellboreComponentService WellboreComponentService
        {
            get { return _wellboreComponentService ?? (_wellboreComponentService = _serviceFactory.GetService<IWellboreComponentService>()); }
        }

        private IWellTestService _wellTestDataService;

        protected IWellTestService WellTestDataService
        {
            get { return _wellTestDataService ?? (_wellTestDataService = _serviceFactory.GetService<IWellTestService>()); }
        }

        private ICatalogService _catalogService;

        protected ICatalogService CatalogService
        {
            get { return _catalogService ?? (_catalogService = _serviceFactory.GetService<ICatalogService>()); }
        }

        private IDynacardService _dynacardService;

        protected IDynacardService DynacardService
        {
            get { return _dynacardService ?? (_dynacardService = _serviceFactory.GetService<IDynacardService>()); }
        }

        private ISurveillanceService _surveillanceService;

        protected ISurveillanceService SurveillanceService
        {
            get { return _surveillanceService ?? (_surveillanceService = _serviceFactory.GetService<ISurveillanceService>()); }
        }

        // Only use this if you need access to the generic well/group status methods.
        protected SurveillanceServiceClient SurveillanceServiceClient
        {
            get { return (SurveillanceServiceClient)SurveillanceService; }
        }

        private IWellMTBFService _wellMTBFService;

        protected IWellMTBFService WellMTBFService
        {
            get { return _wellMTBFService ?? (_wellMTBFService = _serviceFactory.GetService<IWellMTBFService>()); }
        }

        private IComponentService _componentService;

        protected IComponentService ComponentService
        {
            get { return _componentService ?? (_componentService = _serviceFactory.GetService<IComponentService>()); }
        }

        private IJobAndEventService _jobAndEventService;

        protected IJobAndEventService JobAndEventService
        {
            get { return _jobAndEventService ?? (_jobAndEventService = _serviceFactory.GetService<IJobAndEventService>()); }
        }

        private IDocumentService _documentService;

        protected IDocumentService DocumentService
        {
            get { return _documentService ?? (_documentService = _serviceFactory.GetService<IDocumentService>()); }
        }

        private ISurfaceNetworkService _surfaceNetworkService;

        protected ISurfaceNetworkService SurfaceNetworkService
        {
            get
            {
                if (_surfaceNetworkService == null)
                {
                    _surfaceNetworkService = _serviceFactory.GetService<ISurfaceNetworkService>();
                    ClientServiceFactory.SetTimeout(_surfaceNetworkService, extendTimeout);
                }
                return _surfaceNetworkService;
            }
        }

        private IEquipmentService _reOEquipmentService;

        protected IEquipmentService ReOEquipmentService
        {
            get { return _reOEquipmentService ?? (_reOEquipmentService = SurfaceNetworkUtils.CreateSurfaceNetworkClient<IEquipmentService>("SurfaceNetworkEquipment")); }
        }

        private IAlarmService _alarmService;

        protected IAlarmService AlarmService
        {
            get { return _alarmService ?? (_alarmService = _serviceFactory.GetService<IAlarmService>()); }
        }

        private IAuditLogService _auditLogService;

        protected IAuditLogService AuditLogService
        {
            get { return _auditLogService ?? (_auditLogService = _serviceFactory.GetService<IAuditLogService>()); }
        }

        private IDBEntityService _dbEntityService;

        protected IDBEntityService DBEntityService
        {
            get { return _dbEntityService ?? (_dbEntityService = _serviceFactory.GetService<IDBEntityService>()); }
        }

        protected IIntelligentAlarmService _intelligentAlarmService;

        protected IIntelligentAlarmService IntelligentAlarmService
        {
            get { return _intelligentAlarmService ?? (_intelligentAlarmService = _serviceFactory.GetService<IIntelligentAlarmService>()); }
        }

        private IReportService _reportService;

        protected IReportService ReportService
        {
            get { return _reportService ?? (_reportService = _serviceFactory.GetService<IReportService>()); }
        }

        protected ITrackingItemService _trackingItemService;
        protected ITrackingItemService TrackingItemService
        {
            get { return _trackingItemService ?? (_trackingItemService = _serviceFactory.GetService<ITrackingItemService>()); }
        }
        #endregion Services

        protected AuthenticatedUserDTO AuthenticatedUser { get; private set; }

        protected void Authenticate()
        {
            AuthenticatedUser = TokenService.CreateToken();
        }

        protected void ChangeLocale(string locale = null)
        {
            if (string.IsNullOrEmpty(locale))
                locale = "en-US";
            TokenService.SetLocale(locale);
        }

        private HandleWebException _webExceptionHandler;

        private object defaultsetpointval;

        protected bool TraceWebException(string url, WebException wex, string errorMessage)
        {
            if (!s_suppressWebErrorMessages)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("{0} to {1} failed with error {2}{3}.", (wex.Response as HttpWebResponse)?.Method ?? "Unknown", url, wex.Message, errorMessage != null ? " || " + errorMessage : ""));
            }
            return false;
        }

        public APIClientTestBase()
        {
            _webExceptionHandler = TraceWebException;
            _serviceFactory = new ClientServiceFactory(s_hostname, _webExceptionHandler);
            _serviceFactory.DefaultTimeout = 120000;
        }

        private Impersonator _impersonator;

        public virtual void Init()
        {
            DTOExtensions.ThrowInsteadOfDebug = true;
            s_suppressWebErrorMessages = false;
            _wellsToRemove = new List<WellDTO>();
            _dataConnectionsToRemove = new List<DataConnectionDTO>();
            _systemSettingsToRemove = new List<SystemSettingDTO>();
            _systemSettingNamesToRemove = new List<string>();
            _userSettingNamesToRemove = new List<string>();
            _wellSettingsToRestore = new List<WellSettingDTO>();
            _userSettingsToRemove = new List<UserSettingDTO>();
            _systemSettingsToRestore = new List<SystemSettingDTO>();
            _userSettingsToRestore = new List<UserSettingDTO>();
            _wellSettingNamesToRemove = new List<Tuple<long, string>>();
            _wellSettingsToRemove = new List<WellSettingDTO>();
            _usersToRemove = new List<UserDTO>();
            _adGroupsToRemove = new List<ADGroupDTO>();
            _rolesToRemove = new List<RoleDTO>();
            _wellAllocationGroupsToRemove = new List<WellAllocationGroupDTO>();
            _allocationGroupsToRemove = new List<AllocationGroupDTO>();
            _snScenariosToRemove = new List<SNScenarioDTO>();
            _pfScenariosToRemove = new List<PFScenarioDTO>();
            _snModelsToRemove = new List<SNModelDTO>();
            _pfModelsToRemove = new List<PFModelDTO>();
            _scheduleScenariosToRemove = new List<SNScenarioScheduleDTO>();
            _directoriesToRemove = new List<string>();
            _assetsToRemove = new List<AssetDTO>();
            _vrrHierarchyToRemove = new List<AssetDTO>();
            _headerFooterRowsToRemove = new List<string>();
            _assetSettingsToRemove = new List<AssetSettingDTO>();
            _assetSettingsToRestore = new List<AssetSettingDTO>();
            _assetSettingNamesToRemove = new List<Tuple<long, string>>();
            _reservoirsToRemove = new List<ReservoirAndUnitsDTO>();
            _setWellAlowableToRemove = new List<SWASnapshotDTO>();

            if (s_isImpersonatingForAll)
            {
                _impersonator = new Impersonator();
                _impersonator.Impersonate(s_impersonationDomain, s_impersonationUser, s_impersonationPassword);
            }
            Authenticate();
            ChangeLocale();
            if (s_deleteAllWellsBeforeTestExecution == null || s_deleteAllWellsBeforeTestExecution == string.Empty)
            {
                s_deleteAllWellsBeforeTestExecution = "true"; //for ATS we use arx instead of app.config 
            }
            if (s_deleteAllWellsBeforeTestExecution.ToLower().Equals("true"))
            //for TeamCity and ATS test execution we would always like to clean up all Left Over  Wells from previuos  tests
            //If once wants to run tests on local and prevent wells from being cleaned up  set it  to false in app.config
            //Default would always be true;
            {
                DeleteLeftOverWellsBeforeTest();
            }
            _wellsAtStart = WellService.GetAllWells().ToList();
            defaultsetpointval = GetForeSiteToolBoxSettingValue(SettingServiceStringConstants.SET_POINT_CONFIG);

        }

        public virtual void Cleanup()
        {
            try
            {
                s_suppressWebErrorMessages = false;
                var exceptions = new List<Exception>();

                if (_setWellAlowableToRemove.Any())
                {
                    //Now remove Snapshot based on SnapshotID
                    foreach (SWASnapshotDTO Snapshot in _setWellAlowableToRemove)
                    {
                        try
                        {
                            WellAllocationService.DeleteWellAllowableSnapshot(Snapshot.Id.ToString());

                            Trace.WriteLine("Entry deleted successfully for: " + Snapshot.Name);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"Failed to remove snapshot id for {Snapshot.Name} ({Snapshot.Id}): {ex.Message}");
                        }
                    }
                }

                if (_vrrHierarchyToRemove.Any())
                {
                    // Now remove the VRR Hierarchy.
                    foreach (AssetDTO asset in _vrrHierarchyToRemove)
                    {
                        try
                        {
                            WellAllocationService.DeleteHierarchyByAssetId(asset.Id.ToString());

                            Trace.WriteLine("VRR Hierarchy deleted successfully for: " + asset.Name);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"Failed to remove VRR hierarchy for {asset.Name} ({asset.Id}): {ex.Message}");

                        }
                    }
                }

                foreach (var well in _wellsToRemove)
                {
                    try
                    {
                        WellConfigurationService.RemoveWellConfig(well.Id.ToString());
                        Trace.WriteLine(well.Name + " deleted successfully");
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }
                _wellsToRemove.Clear();

                foreach (var systemSetting in _systemSettingsToRemove)
                {
                    try
                    {
                        SettingService.RemoveSystemSetting(systemSetting.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Failed to remove system setting {0}: {1}", systemSetting.Id, ex.Message));

                    }
                }

                foreach (string name in _systemSettingNamesToRemove)
                {
                    try
                    {
                        SystemSettingDTO value = SettingService.GetSystemSettingByName(name);
                        SettingService.RemoveSystemSetting(value.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Failed to remove system setting {name}: {ex.Message}");

                    }
                }

                foreach (var systemSetting in _systemSettingsToRestore)
                {
                    try
                    {
                        SettingService.SaveSystemSetting(systemSetting);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Failed to restore system setting {systemSetting.Id} ({systemSetting?.Setting?.Name}): {ex.Message}");

                    }
                }

                foreach (var userSetting in _userSettingsToRemove)
                {
                    try
                    {
                        SettingService.RemoveUserSetting(userSetting.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Failed to remove user setting {0}: {1}", userSetting.Id, ex.Message));

                    }
                }

                foreach (string name in _userSettingNamesToRemove)
                {
                    try
                    {
                        SettingDTO setting = SettingService.GetSettingByName(name);
                        UserSettingDTO value = SettingService.GetUserSettingByUserIdAndSettingId(AuthenticatedUser.Id.ToString(), setting.Id.ToString());
                        if (value != null)
                        {
                            SettingService.RemoveUserSetting(value.Id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Failed to remove user setting {name}: {ex.Message}");

                    }
                }

                foreach (var userSetting in _userSettingsToRestore)
                {
                    try
                    {
                        SettingService.SaveUserSetting(userSetting);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Failed to restore user setting {userSetting.Id} ({userSetting?.Setting?.Name}): {ex.Message}");

                    }
                }

                foreach (var wellSetting in _wellSettingsToRemove)
                {
                    try
                    {
                        SettingService.RemoveWellSetting(wellSetting.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Failed to remove well setting {0}: {1}", wellSetting.Id, ex.Message));

                    }
                }

                foreach (Tuple<long, string> wellIdAndSettingName in _wellSettingNamesToRemove)
                {
                    try
                    {
                        WellConfigDTO wellConfig = WellConfigurationService.GetWellConfig(wellIdAndSettingName.Item1.ToString());
                        if (wellConfig.Well != null)
                        {
                            SettingDTO setting = SettingService.GetSettingByName(wellIdAndSettingName.Item2);
                            WellSettingDTO value = SettingService.GetWellSettingsByWellId(wellIdAndSettingName.Item1.ToString()).FirstOrDefault(ws => ws.SettingId == setting.Id);
                            SettingService.RemoveWellSetting(value.Id.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Failed to remove user setting {wellIdAndSettingName.Item2}: {ex.Message}");

                    }
                }

                foreach (var wellSetting in _wellSettingsToRestore)
                {
                    try
                    {
                        SettingService.SaveWellSetting(wellSetting);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Failed to restore well setting {wellSetting.Id} ({wellSetting?.Setting?.Name}): {ex.Message}");

                    }
                }

                foreach (var directory in _directoriesToRemove)
                {
                    try
                    {
                        Directory.Delete(directory, true);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Failed to remove directory {directory}): {ex.Message}");

                    }
                }
                foreach (var pfScenario in _pfScenariosToRemove)
                {
                    try
                    {
                        ProductionForecastService.RemovePFScenario(pfScenario.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Failed to remove PFScenario name :  {pfScenario.Name} ({pfScenario.Id}): {ex.Message}");

                    }
                }
                foreach (var pfModel in _pfModelsToRemove)
                {
                    try
                    {
                        ProductionForecastService.RemovePFModel(pfModel.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Failed to remove PFModel {pfModel.Name} ({pfModel.Id}): {ex.Message}");

                    }
                }
                foreach (var scheduleScenario in _scheduleScenariosToRemove)
                {
                    try
                    {

                        SurfaceNetworkService.RemoveSNScenarioSchedule(scheduleScenario);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Failed to remove Schedule Scenario id :   ({scheduleScenario.Id}): {ex.Message}");

                    }
                }

                foreach (var snScenario in _snScenariosToRemove)
                {
                    try
                    {
                        SurfaceNetworkService.RemoveSNScenario(snScenario.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Failed to remove SNScenario name : {snScenario.Name} ({snScenario.Id}): {ex.Message}");

                    }
                }

                foreach (var snModel in _snModelsToRemove)
                {
                    try
                    {
                        SurfaceNetworkService.RemoveSNModel(snModel.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Failed to remove SNModel name : {snModel.Name} ({snModel.Id}): {ex.Message}");

                    }
                }

                foreach (var assetSetting in _assetSettingsToRemove)
                {
                    try
                    {
                        SettingService.RemoveAssetSetting(assetSetting.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Failed to remove asset setting {0}: {1}", assetSetting.Id, ex.Message));

                    }
                }

                foreach (var assetSetting in _assetSettingsToRestore)
                {
                    try
                    {
                        SettingService.SaveAssetSetting(assetSetting);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Failed to restore asset setting {assetSetting.Id} ({assetSetting?.Setting?.Name}): {ex.Message}");

                    }
                }

                foreach (Tuple<long, string> assetIdAndSettingName in _assetSettingNamesToRemove)
                {
                    try
                    {
                        SettingDTO setting = SettingService.GetSettingByName(assetIdAndSettingName.Item2);
                        AssetSettingDTO value = SettingService.GetAssetSettingsByAssetId(assetIdAndSettingName.Item1.ToString()).FirstOrDefault(ws => ws.SettingId == setting.Id);
                        SettingService.RemoveAssetSetting(value.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"Failed to remove asset setting {assetIdAndSettingName.Item2}: {ex.Message}");

                    }
                }

                #region Reservoirs Clean up
                if (_reservoirsToRemove.Any())
                {
                    // Remove the reserviors. Removing reservoirs via WellAllocationService takes care of removing associated zones and patterns as well
                    foreach (ReservoirAndUnitsDTO reservoir in _reservoirsToRemove)
                    {
                        try
                        {
                            WellAllocationService.RemoveReservoir(reservoir.Value);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"Failed to remove reservoir {reservoir.Value.Name} ({reservoir.Value.Id}): {ex.Message}");
                        }
                    }
                }
                #endregion Reservoirs Clean up

                if (_assetsToRemove.Any())
                {
                    // First remove the assets from any users.
                    UserDTO[] users = AdministrationService.GetUsers();
                    foreach (UserDTO userToUpdate in users.Where(t => t.Assets != null && t.Assets.Any(a => _assetsToRemove.Any(atr => a.Id == atr.Id))))
                    {
                        userToUpdate.Assets.Where(t => _assetsToRemove.Any(atr => t.Id == atr.Id)).ToList().ForEach(t => userToUpdate.Assets.Remove(t));
                        try
                        {
                            AdministrationService.UpdateUser(userToUpdate);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"Failed to remove assets from user {userToUpdate.Name}: {ex.Message}");

                        }
                    }
                    // Now remove the assets.
                    foreach (AssetDTO asset in _assetsToRemove)
                    {
                        try
                        {
                            SurfaceNetworkService.RemoveAsset(asset.Id.ToString());
                            Trace.WriteLine(asset.Name + " deleted successfully");
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"Failed to remove asset {asset.Name} ({asset.Id}): {ex.Message}");

                        }
                    }
                }



                if (_headerFooterRowsToRemove.Any())
                {
                    foreach (string headerFooterRow in _headerFooterRowsToRemove)
                    {
                        try
                        {
                            ComponentService.RemoveHeaderFooterConfiguration(headerFooterRow);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"Failed to remove HeaderFooter id {headerFooterRow} : {ex.Message}");

                        }
                    }
                }

                foreach (var dc in _dataConnectionsToRemove)
                {
                    try
                    {
                        Console.WriteLine("Domain: " + dc.ProductionDomain + "Site: " + dc.Site);
                        DataConnectionService.RemoveDataConnection(dc.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);

                    }
                }

                foreach (UserDTO user in _usersToRemove)
                {
                    try
                    {
                        AdministrationService.RemoveUser(user);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Failed to remove user {0}: {1}", user.Name, ex.Message));

                    }
                }
                foreach (ADGroupDTO adgroup in _adGroupsToRemove)
                {
                    try
                    {
                        AdministrationService.RemoveGroup(adgroup.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Failed to remove AdGroup {0}: {1}", adgroup.Name, ex.Message));

                    }
                }
                foreach (RoleDTO role in _rolesToRemove)
                {
                    try
                    {
                        AdministrationService.RemoveRole(role);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Failed to remove role {0}: {1}", role.Name, ex.Message));

                    }
                }

                foreach (AllocationGroupDTO allocationGroup in _allocationGroupsToRemove)
                {
                    try
                    {
                        WellAllocationService.RemoveAllocationGroupsById(allocationGroup.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(string.Format("Failed to remove allocation group {0}: {1}", allocationGroup.Name, ex.Message));

                    }
                }

                foreach (WellAllocationGroupDTO allocationGroup in _wellAllocationGroupsToRemove)
                {
                    try
                    {
                        WellService.RemoveWellAllocationGroup(allocationGroup.Id.ToString());
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(string.Format("Failed to remove allocation group {0}: {1}", allocationGroup.AllocationGroupName, ex.Message));

                    }
                }

                WellDTO[] wellsAtEnd = WellService.GetAllWells();

                if (wellsAtEnd != null && _wellsAtStart != null && wellsAtEnd.Length > 0)
                {
                    List<WellDTO> leftoverWells = wellsAtEnd.Where(wae => _wellsAtStart.FirstOrDefault(was => was.Id == wae.Id) == null).ToList();
                    if (leftoverWells != null && leftoverWells.Count > 0)
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("Test left behind well{0} ({1}): {2}.",
                            leftoverWells.Count == 1 ? "" : "s",
                            leftoverWells.Count,
                            string.Join(" ,", leftoverWells.Select(w => w.Name))));
                    }
                }
                // Revert to default Setting for SetPoints 
                SetValuesInSystemSettings(SettingServiceStringConstants.SET_POINT_CONFIG, defaultsetpointval.ToString());
                if (exceptions.Count > 0)
                {
                    throw new AggregateException(exceptions.ToArray());
                }
            }
            finally
            {
                if (s_isImpersonatingForAll && _impersonator != null)
                {
                    _impersonator.Undo();
                    _impersonator = null;
                }
            }
        }

        protected void ConfigureForMultipleLinkedDevices()
        {
            const string deviceTypesValue = "LufkinSam;WellPilotRPOC";
            SystemSettingDTO expectedDeviceTypes = SettingService.GetSystemSettingByName(SettingServiceStringConstants.EXPECTED_DEVICE_TYPES);
            if (expectedDeviceTypes.StringValue != deviceTypesValue)
            {
                if (expectedDeviceTypes.Id == 0)
                {
                    _systemSettingNamesToRemove.Add(expectedDeviceTypes.Setting.Name);
                }
                else
                {
                    _systemSettingsToRestore.Add(expectedDeviceTypes);
                }
                expectedDeviceTypes = SettingService.GetSystemSettingByName(SettingServiceStringConstants.EXPECTED_DEVICE_TYPES);
                expectedDeviceTypes.StringValue = deviceTypesValue;
                SettingService.SaveSystemSetting(expectedDeviceTypes);
            }
        }

        protected string GetUniqueId()
        {
            lock (s_lock)
            {
                int value = s_currentId++;
                return value.ToString("X16");
            }
        }

        public static DateTime Truncate(DateTime dt)
        {
            return dt.AddTicks(-(dt.Ticks % TimeSpan.TicksPerSecond));
        }

        protected static void DoesContain(string expected, string actual, string message = null)
        {
            if (!actual.Contains(expected))
            {
                Assert.Fail(string.Format("DoesContain failed.  Expected:<{0}>. Actual<{1}>. {2}", expected, actual, message));
            }
        }

        protected static void CheckResponseCode(WebException wex, HttpStatusCode expected, string message = null)
        {
            var response = wex.Response as HttpWebResponse;
            Assert.AreEqual(expected, response?.StatusCode, message ?? "Unexpected HTTP status code.");
        }

        protected static void AreEqual(double? expected, double? actual, double delta, string message, params object[] parameters)
        {
            if (expected == null || actual == null)
            {
                Assert.AreEqual(expected, actual, message, parameters);
            }
            else
            {
                Assert.AreEqual(expected ?? 0, actual ?? 0, delta, message, parameters);
            }
        }

        protected static void AreEqual(double[] expected, double[] actual, double delta, string message, params object[] parameters)
        {
            if (expected == null || actual == null)
            {
                Assert.AreEqual(expected, actual, message, parameters);
            }
            else
            {
                Assert.AreEqual(expected.Length, actual.Length, message + " Array lengths are different.", parameters);
                for (int ii = 0; ii < expected.Length; ii++)
                {
                    Assert.AreEqual(expected[ii], actual[ii], delta, message + $" Element at index {ii} does not match.", parameters);
                }
            }
        }

        protected static void Wait(int x)
        {
            DateTime t = DateTime.Now;
            DateTime tf = DateTime.Now.AddSeconds(x);

            while (t < tf)
            {
                t = DateTime.Now;
            }
        }

        protected static byte[] GetByteArray(string path, string file)
        {
            Assembly assembly;
            byte[] fileAsByteArray;

            assembly = Assembly.GetExecutingAssembly();

            using (Stream fileStream = assembly.GetManifestResourceStream(path + file))
            {
                Assert.IsNotNull(fileStream);

                //Convert to byte array
                using (var memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);
                    fileAsByteArray = memoryStream.ToArray();
                }
            }
            return fileAsByteArray;
        }

        protected static string GetJsonString(string filePath)
        {
            return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(filePath)).ReadToEnd();
        }


        protected static void TestThrow<T>(string throwFailureMessage, Action action) where T : Exception
        {
            try
            {
                action();
                Assert.Fail(throwFailureMessage);
            }
            catch (T)
            {
            }
        }
        protected static void TestThrowForbidden(string throwFailureMessage, Action action)
        {
            TestThrowWebException(HttpStatusCode.Forbidden, throwFailureMessage, action);
        }

        protected static void TestThrowUnauthorized(string throwFailureMessage, Action action)
        {
            TestThrowWebException(HttpStatusCode.Unauthorized, throwFailureMessage, action);
        }

        protected static void TestThrowBadRequest(string throwFailureMessage, Action action)
        {
            TestThrowWebException(HttpStatusCode.BadRequest, throwFailureMessage, action);
        }

        protected static void TestThrowInternalServerError(string throwFailureMessage, Action action)
        {
            TestThrowWebException(HttpStatusCode.InternalServerError, throwFailureMessage, action);
        }

        protected static void TestThrowWebException(HttpStatusCode expectedStatusCode, string throwFailureMessage, Action action)
        {
            bool originalValue = s_suppressWebErrorMessages;
            s_suppressWebErrorMessages = true;
            try
            {
                action();
                Assert.Fail(throwFailureMessage);
            }
            catch (WebException ex)
            {
                CheckResponseCode(ex, expectedStatusCode);
            }
            finally
            {
                s_suppressWebErrorMessages = originalValue;
            }
        }

        protected static string GetFacilityId(string baseFacilityId, int number)
        {
            if (baseFacilityId.Contains("WFTA1K") && s_isRunningInATS && s_domain.Equals("9077") == false)
            //RTU Address is already taken for Device Id 1 to 15  for Other Modbus  i.e SAMVFD
            {
                number = number + 50;
            }
            string facilityId = baseFacilityId + (s_isRunningInATS ? number.ToString("D5") : number.ToString("D4"));
            return facilityId;
        }

        protected static string GetWellPilotFacilityId(int number)
        {
            return GetFacilityId("RPOC_", number);
        }

        protected static string GetLufkinSamFacilityId(int number)
        {
            return GetFacilityId("SAM_", number);
        }

        protected static string GetEProdFacilityId(int number)
        {
            return GetFacilityId("8800_", number);
        }

        protected static string GetAEPOCFacilityId(int number)
        {
            return GetFacilityId("AEPOC_", number);
        }

        protected static string GetEPICFSFacilityId(int number)
        {
            return GetFacilityId("EPICFS_", number);
        }

        protected static string GetGLFacilityId(int number)
        {
            return "GLWELL_" + (s_isRunningInATS ? number.ToString("D5") : number.ToString("D4"));
        }

        protected static string GetESPFacilityId(int number)
        {
            return "ESPWELL_" + (s_isRunningInATS ? number.ToString("D5") : number.ToString("D4"));
        }

        /// <summary>
        /// Checks the system-level unit system and if it is not metric it sets it to metric and configures test cleanup to restore original value.
        /// </summary>
        protected void SetUnitSystemToMetric()
        {
            //const string metricSystemName = SettingServiceStringConstants.UNIT_SYSTEM_NAME_METRIC;
            //SystemSettingDTO unitSystemSetting = SettingService.GetSystemSettingByName(SettingServiceStringConstants.UNIT_SYSTEM);
            //Assert.IsNotNull(unitSystemSetting, "Failed to get system setting for unit system.");
            //if (unitSystemSetting.StringValue != metricSystemName)
            //{
            //    if (unitSystemSetting.Id == 0)
            //    {
            //        _systemSettingNamesToRemove.Add(unitSystemSetting.Setting.Name);
            //    }
            //    else
            //    {
            //        _systemSettingsToRestore.Add(SettingService.GetSystemSettingByName(SettingServiceStringConstants.UNIT_SYSTEM));
            //    }
            //    unitSystemSetting.StringValue = metricSystemName;
            //    SettingService.SaveSystemSetting(unitSystemSetting);
            //}
            ChangeUnitSystem("Metric");
            ChangeUnitSystemUserSetting("Metric");
        }

        protected static void CompareObjectsUsingReflection(object first, object second, string message, HashSet<string> propertiesToIgnore = null, double? allowableDifference = null)
        {
            if (first == null && second == null)
            {
                return;
            }
            if (first == null && second != null)
            {
                Assert.Fail($"{message}: first object is null and second is not.");
            }
            if (first != null && second == null)
            {
                Assert.Fail($"{message}: second object is null and first is not.");
            }
            if (first.GetType() != second.GetType())
            {
                Assert.Fail($"{message}: first object has type {first.GetType().Name} and second has type {second.GetType().Name}");
            }
            if (propertiesToIgnore == null)
            {
                propertiesToIgnore = new HashSet<string>();
            }
            var badValues = new List<Tuple<string, string, string>>();
            List<PropertyInfo> props = first.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
            foreach (PropertyInfo prop in props)
            {
                Trace.WriteLine($"Verifiying the Property {prop.Name} ");
                if (propertiesToIgnore.Contains(prop.Name))
                {
                    continue;
                }
                object val1 = prop.GetValue(first);
                object val2 = prop.GetValue(second);
                bool compared = false;
                if (allowableDifference != null && (prop.PropertyType == typeof(decimal?) || prop.PropertyType == typeof(decimal)))
                {
                    decimal allowableDifferenceDec = Convert.ToDecimal(allowableDifference ?? 0.0);
                    if (prop.PropertyType == typeof(decimal?))
                    {
                        decimal? val1Dec = (decimal?)val1;
                        decimal? val2Dec = (decimal?)val2;
                        compared = true;
                        if ((val1 == null && val2 != null) || (val1 != null && val2 == null))
                        {
                            badValues.Add(Tuple.Create(prop.Name, val1?.ToString(), val2?.ToString()));
                        }
                        else if (val1 != null && Math.Abs((val1Dec ?? 0.0m) - (val2Dec ?? 0.0m)) > allowableDifferenceDec)
                        {
                            badValues.Add(Tuple.Create(prop.Name, val1?.ToString(), val2?.ToString()));
                        }
                    }
                    else if (prop.PropertyType == typeof(decimal))
                    {
                        decimal val1Dec = (decimal)val1;
                        decimal val2Dec = (decimal)val2;
                        compared = true;
                        if ((val1 == null && val2 != null) || (val1 != null && val2 == null))
                        {
                            badValues.Add(Tuple.Create(prop.Name, val1?.ToString(), val2?.ToString()));
                        }
                        else if (val1 != null && Math.Abs(val1Dec - val2Dec) > allowableDifferenceDec)
                        {
                            badValues.Add(Tuple.Create(prop.Name, val1?.ToString(), val2?.ToString()));
                        }
                    }
                }

                if (allowableDifference != null && (prop.PropertyType == typeof(double?) || prop.PropertyType == typeof(double)))
                {
                    double allowableDifferenceDec = Convert.ToDouble(allowableDifference ?? 0.0);
                    if (prop.PropertyType == typeof(double?))
                    {
                        double? val1Dec = (double?)val1;
                        double? val2Dec = (double?)val2;
                        compared = true;
                        if ((val1 == null && val2 != null) || (val1 != null && val2 == null))
                        {
                            badValues.Add(Tuple.Create(prop.Name, val1?.ToString(), val2?.ToString()));
                        }
                        else if (val1 != null && Math.Abs((val1Dec ?? 0.0) - (val2Dec ?? 0.0)) > allowableDifferenceDec)
                        {
                            badValues.Add(Tuple.Create(prop.Name, val1?.ToString(), val2?.ToString()));
                        }
                    }
                    else if (prop.PropertyType == typeof(double))
                    {
                        double val1Dec = (double)val1;
                        double val2Dec = (double)val2;
                        compared = true;
                        if ((val1 == null && val2 != null) || (val1 != null && val2 == null))
                        {
                            badValues.Add(Tuple.Create(prop.Name, val1?.ToString(), val2?.ToString()));
                        }
                        else if (val1 != null && Math.Abs(val1Dec - val2Dec) > allowableDifferenceDec)
                        {
                            badValues.Add(Tuple.Create(prop.Name, val1?.ToString(), val2?.ToString()));
                        }
                    }
                }

                if (!compared && !Equals(val1, val2))
                {
                    badValues.Add(Tuple.Create(prop.Name, val1?.ToString(), val2?.ToString()));
                }

                Trace.WriteLine($"Verified the Property {prop.Name} ");
            }
            Assert.AreEqual(0, badValues.Count, $"{message}: data does not match: " + string.Join(", ", badValues.Select(t => t.Item1 + ": \"" + t.Item2 + "\" != \"" + t.Item3 + "\"")));
        }

        protected static void AssertNullEquivalency(object expected, object actual, string name, string message)
        {
            if (expected == null)
            {
                Assert.IsNull(actual, message + $"Expected {name} is null and actual is not.");
            }
            else
            {
                Assert.IsNotNull(actual, message + $"Expected {name} is not null and actual is.");
            }
        }

        protected static bool LogNotConfiguredIfRunningInATS([CallerMemberName] string memberName = "")
        {
            if (s_isRunningInATS)
            {
                Trace.WriteLine($"Test method {memberName} not configured to run in ATS.");
                return true;
            }
            return false;
        }

        // Return the Well Type Asscoiated with the passed in SystemFeature; Optionally set the desired Injection well type if necessary, default is Gas Injection
        protected static WellTypeId GetWellTypeFromSystemFeature(SystemFeature feature, WellTypeId defaultInjType = WellTypeId.GInj)
        {
            switch (feature)
            {
                case SystemFeature.ESP:
                    {
                        return WellTypeId.ESP;
                    }
                case SystemFeature.GL:
                    {
                        return WellTypeId.GLift;
                    }
                case SystemFeature.Injection:
                    {
                        return defaultInjType;
                    }
                case SystemFeature.NF:
                    {
                        return WellTypeId.NF;
                    }
                case SystemFeature.RRL:
                    {
                        return WellTypeId.RRL;
                    }
                case SystemFeature.Plunger:
                    {
                        return WellTypeId.PLift;
                    }
                default:
                    {
                        return WellTypeId.Unknown;
                    }
            }
        }

        protected static Random random = new Random();

        protected static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        protected static string RandomNumber(int length)
        {
            var random = new Random();
            string s = string.Empty;
            for (int i = 0; i < length; i++)
                s = string.Concat(s, random.Next(10).ToString());
            return s;
        }

        protected static void CheckPOPUserException(WebException ex, string errorMessage)
        {
            CheckResponseCode(ex, HttpStatusCode.InternalServerError);
            var rawResponse = string.Empty;
            var alreadyClosedStream = ex.Response.GetResponseStream() as MemoryStream;
            using (var brandNewStream = new MemoryStream(alreadyClosedStream.ToArray()))
            using (var reader = new StreamReader(brandNewStream))
                rawResponse = reader.ReadToEnd();
            Assert.IsTrue(rawResponse.Contains(errorMessage), "Failed to get the POP User Exception");
        }

        protected static double? UnitsConversion(string unitKey, double? value)
        {
            double? Metricvalue = null;
            switch (unitKey)
            {
                case "ft"://to m
                    Metricvalue = value * 0.3048;
                    break;

                case "psia":// to kPa
                case "psi":// to kPa
                    Metricvalue = value * 6.894757;
                    break;

                case "F":// to C
                    Metricvalue = (value - 32) * 0.55555555555555555555555555555556;
                    break;

                case "Mscf":
                case "Mscf/d":// to sm3/d
                    Metricvalue = value * 28.3168466;
                    break;

                case "STB":
                case "STB/d":// to sm3/d
                    Metricvalue = value * 0.1589873;
                    break;

                case "STB/d/psi":// to STB/d/kg/cm^2
                    Metricvalue = value * 14.22475;
                    break;

                case "scf/STB":// to sm3/sm3
                    Metricvalue = value * 0.1781076;
                    break;

                case "bbl/d":// to m3/d
                    Metricvalue = value * 0.1589873;
                    break;

                case "lb":// to N
                    Metricvalue = value * 4.44822;
                    break;

                case "hp":// to ="kW"
                    Metricvalue = value * 0.746;
                    break;

                case "in"://to cm  To="cm"
                    Metricvalue = value * 2.54;
                    break;

                case "psi/ft": //to ="kPa/m"
                    Metricvalue = value * 22.62059;
                    break;

                case "Mcf"://to ="m3"
                    Metricvalue = value * 28.3168466;
                    break;

                case "APIG": // To = "SG"
                    Metricvalue = 141.5 / (value + 131.5);
                    break;

                case "kinlbs": // To = "kNm"
                    Metricvalue = value * 0.112984829;
                    break;

                case "clb": // To = "N"
                    Metricvalue = value * 444.82216282509006;
                    break;

                case "bbl"://to m3
                    Metricvalue = value * 0.1589873;
                    break;

                case "1/64in":// to mm
                    Metricvalue = value * 0.396875;
                    break;
            }
            return Metricvalue;
        }

        public void RemoveWell(string wellname)
        {
            WellDTO well = WellService.GetAllWells().FirstOrDefault(x => x.Name == wellname);
            if (well != null)
                _wellsToRemove.Add(well);
        }
        public void RemoveWellOnDemand(string wellname)
        {
            Authenticate();
            try
            {
                WellDTO[] allwells = WellService.GetAllWells();
                WellDTO well = allwells.FirstOrDefault(x => x.Name == wellname);
                Trace.WriteLine("Well to be deleted on the fly within test method : " + well.Name);
                WellConfigurationService.RemoveWellConfig(well.Id.ToString());
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error in deleting well: ===> " + ex.Message);
                throw ex;
            }
        }

        public void CreateRole(string roleName)
        {
            Authenticate();
            IList<PermissionDTO> permissions = AdministrationService.GetPermissions();
            RoleDTO role = new RoleDTO();
            role.Name = roleName;
            role.Permissions = permissions.ToList();
            AdministrationService.AddRole(role);
            RoleDTO addedRole = AdministrationService.GetRoles().FirstOrDefault(x => x.Name == roleName);
            _rolesToRemove.Add(addedRole);
        }
        public void RemovePermissionsFromRole(string roleName, PermissionId[] permissionIds)
        {
            Authenticate();
            RoleDTO role = AdministrationService.GetRoles().FirstOrDefault(x => x.Name == roleName);
            IList<PermissionDTO> permissions = AdministrationService.GetPermissions();

            role.Permissions = permissions.ToList();

            foreach (PermissionId permissionId in permissionIds)
            {
                role.Permissions = role.Permissions.Where(p => p.Id != permissionId).ToList();
            }

            AdministrationService.UpdateRole(role);
            Trace.WriteLine("Permission Removed");
        }

        public void AddPermissioninRole(string roleName, PermissionId permissionId)
        {
            Authenticate();
            RoleDTO toBeUpdateRole = AdministrationService.GetRoles().FirstOrDefault(x => x.Name == roleName);
            PermissionDTO toBeAddedPermission = AdministrationService.GetPermissions().FirstOrDefault(x => x.Id == permissionId);
            toBeUpdateRole.Permissions.Add(toBeAddedPermission);
            AdministrationService.UpdateRole(toBeUpdateRole);
            Trace.WriteLine("Permission Added");
        }

        public void UpdateUserWithGivenRole(string roleName)
        {
            Authenticate();
            RoleDTO RoleToBeAssignedToUser = AdministrationService.GetRoles().FirstOrDefault(x => x.Name == roleName);
            UserDTO user = AdministrationService.GetUser(AuthenticatedUser.Id.ToString());
            user.Roles.Clear();
            user.Roles.Add(RoleToBeAssignedToUser);
            AdministrationService.UpdateUser(user);
            Trace.WriteLine("Role" + roleName + "Assigned to " + AuthenticatedUser.Name);
        }

        public void UpdateUserWithGivenAsset(string assetName)
        {
            Authenticate();
            AssetDTO[] assets = SurfaceNetworkService.GetAllAssets();
            AssetDTO addedAsset = assets.FirstOrDefault(t => t.Name == assetName);
            UserDTO user = AdministrationService.GetUser(AuthenticatedUser.Id.ToString());
            user.Assets.Add(addedAsset);
            AdministrationService.UpdateUser(user);
            Trace.WriteLine("Asset: " + assetName + " Assigned to user: " + AuthenticatedUser.Name);
        }

        protected WellDTO AddAndGetWell(WellDTO wellToAdd)
        {
            WellConfigurationService.AddWellConfig(new WellConfigDTO() { Well = wellToAdd });
            WellDTO addedWell = WellService.GetWellByName(wellToAdd.Name);
            Assert.IsNotNull(addedWell, $"Failed to add well {wellToAdd.Name}.");
            _wellsToRemove.Add(addedWell);
            return addedWell;
        }

        protected AssetDTO AddAndGetAsset(AssetDTO assetToAdd)
        {
            SurfaceNetworkService.AddAsset(assetToAdd);
            AssetDTO[] assets = SurfaceNetworkService.GetAllAssets();
            AssetDTO addedAsset = assets.FirstOrDefault(t => t.Name == assetToAdd.Name);
            Assert.IsNotNull(addedAsset, $"Failed to get added asset {assetToAdd.Name}.");
            _assetsToRemove.Add(addedAsset);
            return addedAsset;
        }

        protected static WellFluidType? GetDefaultFluidType(WellTypeId wellType)
        {
            if (wellType == WellTypeId.GLift)
                return WellFluidType.BlackOil;
            else if (wellType == WellTypeId.PLift)
                return WellFluidType.DryGas;
            else if (wellType == WellTypeId.NF)
                return WellFluidType.BlackOil;
            else if (wellType == WellTypeId.PCP)
                return WellFluidType.BlackOil;
            else
                return (WellFluidType?)null;
        }

        protected static WellFluidPhase? GetDefaultFluidphasefor_multi(WellTypeId wellType)
        {

            if (wellType == WellTypeId.PCP)
                return WellFluidPhase.MultiPhase;
            else
                return (WellFluidPhase?)null;
        }

        protected static WellDTO SetDefaultFluidType(WellDTO well)
        {
            well.FluidType = GetDefaultFluidType(well.WellType);
            well.FluidPhase = GetDefaultFluidphasefor_multi(well.WellType);
            return well;
        }

        protected static WellDTO SetDefaultFluidTypeAndPhase(WellDTO well)
        {
            well.FluidType = GetDefaultFluidType(well.WellType);
            well.FluidPhase = GetDefaultFluidphasefor_multi(well.WellType);
            return well;
        }

    }
}