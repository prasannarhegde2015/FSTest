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
using Weatherford.POP.DTOs;
using Weatherford.POP.Enums;
using Weatherford.POP.Interfaces;
using Weatherford.POP.APIClient;


namespace SmokeTests
{
    public class APIUITestBase
    {
        public Random random = new Random();
        public string s_hostname = ConfigurationManager.AppSettings.Get("HostName");
        public static bool s_suppressWebErrorMessages;
        public static int s_currentId = 0;
        public static object s_lock = new object();
        public string s_domain = ConfigurationManager.AppSettings.Get("Domain");
        public string s_site = ConfigurationManager.AppSettings.Get("CygNetSite");
        public string s_cvsService = ConfigurationManager.AppSettings.Get("CVSService");
        //public static bool s_isImpersonatingForAll = ConfigurationManager.AppSettings.Get("ImpersonateForAllTests").Equals("true", StringComparison.CurrentCultureIgnoreCase);
        //public static string s_impersonationDomain = ConfigurationManager.AppSettings.Get("ImpersonationDomain");
        //public static string s_impersonationUser = ConfigurationManager.AppSettings.Get("ImpersonationUser");
        //public static string s_impersonationPassword = ConfigurationManager.AppSettings.Get("ImpersonationPassword");


        public ClientServiceFactory _serviceFactory;
        public List<WellDTO> _wellsToRemove;
        public List<DataConnectionDTO> _dataConnectionsToRemove;
        public List<SystemSettingDTO> _systemSettingsToRemove;
        public List<string> _systemSettingNamesToRemove;
        public List<string> _userSettingNamesToRemove;
        public List<UserSettingDTO> _userSettingsToRemove;
        public List<WellSettingDTO> _wellSettingsToRestore;
        public List<SystemSettingDTO> _systemSettingsToRestore;
        public List<UserSettingDTO> _userSettingsToRestore;
        public List<Tuple<long, string>> _wellSettingNamesToRemove;
        public List<WellSettingDTO> _wellSettingsToRemove;
        public List<UserDTO> _usersToRemove;
        public List<RoleDTO> _rolesToRemove;
        public List<WellDTO> _wellsAtStart;
        public List<WellAllocationGroupDTO> _allocationGroupsToRemove;
        public List<AssetDTO> _assetsToRemove;
        public List<string> _headerFooterRowsToRemove;

        #region Services

        private ITokenService _tokenService;

        public ITokenService TokenService
        {
            get { return _tokenService ?? (_tokenService = _serviceFactory.GetService<ITokenService>()); }
        }

        public IPEDashboardService _peDashboardService;

        public IPEDashboardService PEDashboardService
        {
            get { return _peDashboardService ?? (_peDashboardService = _serviceFactory.GetService<IPEDashboardService>()); }
        }

        private IDataConnectionService _dataConnectionService;

        public IDataConnectionService DataConnectionService
        {
            get { return _dataConnectionService ?? (_dataConnectionService = _serviceFactory.GetService<IDataConnectionService>()); }
        }

        private IWellService _wellService;

        public IWellService WellService
        {
            get { return _wellService ?? (_wellService = _serviceFactory.GetService<IWellService>()); }
        }

        private ISettingService _settingService;

        public ISettingService SettingService
        {
            get { return _settingService ?? (_settingService = _serviceFactory.GetService<ISettingService>()); }
        }

        private IAdministrationService _administrationService;

        public IAdministrationService AdministrationService
        {
            get { return _administrationService ?? (_administrationService = _serviceFactory.GetService<IAdministrationService>()); }
        }

        private IModelFileService _modelFileService;

        public IModelFileService ModelFileService
        {
            get { return _modelFileService ?? (_modelFileService = _serviceFactory.GetService<IModelFileService>()); }
        }

        private IWellConfigurationService _wellConfigurationService;

        public IWellConfigurationService WellConfigurationService
        {
            get { return _wellConfigurationService ?? (_wellConfigurationService = _serviceFactory.GetService<IWellConfigurationService>()); }
        }

        private IWellboreComponentService _wellboreComponentService;

        public IWellboreComponentService WellboreComponentService
        {
            get { return _wellboreComponentService ?? (_wellboreComponentService = _serviceFactory.GetService<IWellboreComponentService>()); }
        }

        private IWellTestService _wellTestDataService;

        public IWellTestService WellTestDataService
        {
            get { return _wellTestDataService ?? (_wellTestDataService = _serviceFactory.GetService<IWellTestService>()); }
        }

        private ICatalogService _catalogService;

        public ICatalogService CatalogService
        {
            get { return _catalogService ?? (_catalogService = _serviceFactory.GetService<ICatalogService>()); }
        }

        private IDynacardService _dynacardService;

        public IDynacardService DynacardService
        {
            get { return _dynacardService ?? (_dynacardService = _serviceFactory.GetService<IDynacardService>()); }
        }

        private ISurveillanceService _surveillanceService;

        public ISurveillanceService SurveillanceService
        {
            get { return _surveillanceService ?? (_surveillanceService = _serviceFactory.GetService<ISurveillanceService>()); }
        }

        private IWellMTBFService _wellMTBFService;

        public IWellMTBFService WellMTBFService
        {
            get { return _wellMTBFService ?? (_wellMTBFService = _serviceFactory.GetService<IWellMTBFService>()); }
        }

        private IComponentService _componentService;

        public IComponentService ComponentService
        {
            get { return _componentService ?? (_componentService = _serviceFactory.GetService<IComponentService>()); }
        }

        private IJobAndEventService _jobAndEventService;

        public IJobAndEventService JobAndEventService
        {
            get { return _jobAndEventService ?? (_jobAndEventService = _serviceFactory.GetService<IJobAndEventService>()); }
        }

        private IDocumentService _documentService;

        public IDocumentService DocumentService
        {
            get { return _documentService ?? (_documentService = _serviceFactory.GetService<IDocumentService>()); }
        }

        private ISurfaceNetworkService _surfaceNetworkService;

        public ISurfaceNetworkService SurfaceNetworkService
        {
            get { return _surfaceNetworkService ?? (_surfaceNetworkService = _serviceFactory.GetService<ISurfaceNetworkService>()); }
        }

        private IAlarmService _alarmService;

        public IAlarmService alarmService
        {
            get { return _alarmService ?? (_alarmService = _serviceFactory.GetService<IAlarmService>()); }
        }

        protected IHeatMapService _heatMapService;

        protected IHeatMapService HeatMapService
        {
            get { return _heatMapService ?? (_heatMapService = _serviceFactory.GetService<IHeatMapService>()); }
        }

        #endregion Services

        public AuthenticatedUserDTO AuthenticatedUser { get; private set; }

        public void Authenticate()
        {
            AuthenticatedUser = TokenService.CreateToken();
        }
        //private Impersonator _impersonator;
        private HandleWebException _webExceptionHandler;

        public bool TraceWebException(string url, WebException wex, string errorMessage)
        {
            if (!s_suppressWebErrorMessages)
            {
                Console.WriteLine(string.Format("{0} to {1} failed with error {2}{3}.", (wex.Response as HttpWebResponse)?.Method ?? "Unknown", url, wex.Message, errorMessage != null ? " || " + errorMessage : ""));
            }
            return false;
        }

        public APIUITestBase()
        {
            _webExceptionHandler = TraceWebException;
            Trace.WriteLine("using Hostname as for ClientServiceFactory " + s_hostname);
            _serviceFactory = new ClientServiceFactory(s_hostname, _webExceptionHandler);
        }
        protected void ChangeLocale(string locale = null)
        {
            if (string.IsNullOrEmpty(locale))
                locale = "en-US";
            TokenService.SetLocale(locale);
        }

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
            _rolesToRemove = new List<RoleDTO>();
            _allocationGroupsToRemove = new List<WellAllocationGroupDTO>();
            _assetsToRemove = new List<AssetDTO>();
            _headerFooterRowsToRemove = new List<string>();

            //if (s_isImpersonatingForAll)
            //{
            //    _impersonator = new Impersonator();
            //    _impersonator.Impersonate(s_impersonationDomain, s_impersonationUser, s_impersonationPassword);
            //}
            Authenticate();
            ChangeLocale();
            _wellsAtStart = WellService.GetAllWells().ToList();
        }

        public virtual void Cleanup()
        {
            try
            {
                s_suppressWebErrorMessages = false;
                var exceptions = new List<Exception>();
                if (_wellsToRemove != null)
                {
                    foreach (var well in _wellsToRemove)
                    {
                        try
                        {
                            WellConfigurationService.RemoveWellConfig(well.Id.ToString());
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(ex);
                        }
                    }
                    _wellsToRemove.Clear();
                }

                if (_systemSettingsToRemove != null)
                {
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
                }
                if (_systemSettingNamesToRemove != null)
                {
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
                }

                if (_systemSettingsToRestore != null)
                {
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
                }

                if (_userSettingsToRemove != null)
                {
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
                }
                if (_userSettingNamesToRemove != null)
                {
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
                }

                if (_userSettingsToRestore != null)
                {
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
                }

                if (_wellSettingsToRestore != null)
                {
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
                }

                if (_wellSettingNamesToRemove!=null)
                {
                    foreach (Tuple<long, string> wellIdAndSettingName in _wellSettingNamesToRemove)
                    {
                        try
                        {
                            SettingDTO setting = SettingService.GetSettingByName(wellIdAndSettingName.Item2);
                            WellSettingDTO value = SettingService.GetWellSettingsByWellId(wellIdAndSettingName.Item1.ToString()).FirstOrDefault(ws => ws.SettingId == setting.Id);
                            SettingService.RemoveWellSetting(value.Id.ToString());
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine($"Failed to remove user setting {wellIdAndSettingName.Item2}: {ex.Message}");
                        }
                    }
                }

                if (_wellSettingsToRestore!=null)
                {
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
                }

                if (_assetsToRemove!=null)
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
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"Failed to remove asset {asset.Name} ({asset.Id}): {ex.Message}");
                        }
                    }
                }

                if (_headerFooterRowsToRemove!=null)
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

                if (_dataConnectionsToRemove!=null)
                {
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
                }

                if (_usersToRemove!=null)
                {
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
                }

                if (_rolesToRemove!=null)
                {
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
                }

                if (_allocationGroupsToRemove!=null)
                {
                    foreach (WellAllocationGroupDTO allocationGroup in _allocationGroupsToRemove)
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
                }

                if (exceptions.Count > 0)
                {
                    throw new AggregateException(exceptions.ToArray());
                }
            }
            finally
            {
                //if (s_isImpersonatingForAll && _impersonator != null)
                //{
                //    _impersonator.Undo();
                //    _impersonator = null;
                //}
            }
        }

        public static byte[] GetByteArray(string path, string file)
        {
            byte[] fileAsByteArray;
            using (FileStream fileStream = new FileStream(Path.Combine(path, file), FileMode.Open))
            {
                using (var memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);
                    fileAsByteArray = memoryStream.ToArray();
                }
            }
            return fileAsByteArray;
        }

        public static void WriteLogFile(string message)
        {
            string RunningDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                StreamWriter streamWriter;

                if (!File.Exists(RunningDirectory + "\\" + DateTime.Now.ToString("yyyyMMdd") + "_Log.txt"))
                {
                    FileStream LogFile = new FileStream(RunningDirectory + "\\" + DateTime.Now.ToString("yyyyMMdd") + "_Log.txt", FileMode.OpenOrCreate, FileAccess.Write);
                    streamWriter = new StreamWriter(LogFile);
                }
                else
                {
                    streamWriter = File.AppendText(RunningDirectory + "\\" + DateTime.Now.ToString("yyyyMMdd") + "_Log.txt");
                }
                streamWriter.WriteLine((((DateTime.Now + " - ") + " - ") + " - ") + message);
                streamWriter.Close();
            }
            catch { }
        }
    }
}