using ClientAutomation.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Weatherford.POP.APIClient;
using Weatherford.POP.DTOs;
using Weatherford.POP.Interfaces;

namespace ClientAutomation
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
            CommonHelper.TraceLine("using Hostname as for ClientServiceFactory " + s_hostname);
            _serviceFactory = new ClientServiceFactory(s_hostname, _webExceptionHandler);
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