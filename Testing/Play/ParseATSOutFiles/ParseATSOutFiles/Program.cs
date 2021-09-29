using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Net.Mail;
using System.Net;
using Microsoft.Win32;
using MimeKit;
using MailKit.Security;
using System.Data;
using System.Text.RegularExpressions;
using System.Xml;

namespace ParseATSOutFiles
{
    class Program
    {
        public static string abftformat = String.Empty;
        public static string arstformat = String.Empty;
        public static string abftlocation = String.Empty;
        public static string arstlocation = String.Empty;
        public static string outfilelocation = String.Empty;
        public static string SEND_EMAILS = String.Empty;
        public static List<string> timeouttests_incompletejob = new List<string>();

        public static List<int> cumtotaltest = new List<int>();
        public static List<int> cumtotalpass = new List<int>();
        public static List<int> cumtotalfail = new List<int>();
        public static List<int> cumtotalskip = new List<int>();

        static void Main(string[] args)
        {
            string source = ConfigurationManager.AppSettings.Get("Source");
            if (source.Equals("local"))
            {
                ExtractFromLocal();
            }
            else if (source.Equals("remote"))
            {
                GetDatafromRemoteShare();
            }

        }

        static void ExtractFromLocal()
        {
            abftformat = ConfigurationManager.AppSettings.Get("RunBucket");
            Console.WriteLine($"Bucket used {abftformat}");
            outfilelocation = ConfigurationManager.AppSettings.Get("OutLocation");
            if (abftformat.Equals("ABFT"))
            {
                string ABFTSVDHostPath = @"C:\ATS\Export\UQAScripts";
                string buildversion = GetInstalledAppsVersion("Weatherford ForeSite Bundle");
                string vmname = Environment.MachineName;
                ParseATSCloudData(ABFTSVDHostPath.ToString(), buildversion, vmname, "ABFTSVD");
                GenerateFinalSummary(buildversion, new string[] { "ABFTSVD", "", "" });
            }
            else if (abftformat.Equals("ARST"))
            {
                string ABFTSVDHostPath = @"C:\ATS\Export\UQAScripts";
                string buildversion = GetInstalledAppsVersion("Weatherford ForeSite Bundle");
                string vmname = Environment.MachineName;
                ParseATSCloudData(ABFTSVDHostPath.ToString(), buildversion, vmname, "ARSTSVD");
                GenerateFinalSummary(buildversion, new string[] { "ARSTSVD", "", "" });
            }
        }
        static string GetUpdateLastVersionInfo(bool update, string updateval)
        {
            string val = String.Empty;
            XmlDocument doc = new XmlDocument();
            doc.Load("LatestVersion.xml"); //Keep it at root of binary
            XmlNode node = doc.SelectSingleNode("DHPump/record/LastBuildNumber");
            val = node.InnerText;
            if (update)
            {
                node.InnerText = updateval;
                doc.Save("LatestVersion.xml");
            }
            return val;
        }
        static void GetDatafromRemoteShare()
        {
            string branch = ConfigurationManager.AppSettings.Get("Branch");
            string ReportLocation = @"\\SLOABFTVM.vsi.dom\Reports\" + branch;
            string ABFTBasePath = Path.Combine(ReportLocation, "ABFT");
            string ARSTBasePath = Path.Combine(ReportLocation, "ARST");
            outfilelocation = ConfigurationManager.AppSettings.Get("OutLocation");
            //Get Latest Folder By using Latest Iteratively till we reach path for Target

            string latestyear = GetLatestFromDirecotoryPath(ABFTBasePath);
            string latestmonth = GetLatestFromDirecotoryPath(Path.Combine(ABFTBasePath, latestyear));
            string latestbuild = GetLatestFromDirecotoryPath(Path.Combine(ABFTBasePath, latestyear, latestmonth));
            string SVDDynaVM = GetLatestFromDirecotoryPath(Path.Combine(ABFTBasePath, latestyear, latestmonth, latestbuild, "SVD"));
            string ABFTSVDHostPath = Path.Combine(ABFTBasePath, latestyear, latestmonth, latestbuild, "SVD", SVDDynaVM, "ATS", "UQAScripts");

            string UPDDynaVM = GetLatestFromDirecotoryPath(Path.Combine(ABFTBasePath, latestyear, latestmonth, latestbuild, "UPD"));
            string ABFTUPDHostPath = Path.Combine(ABFTBasePath, latestyear, latestmonth, latestbuild, "UPD", UPDDynaVM, "ATS", "UQAScripts");

            string arstlatestyear = GetLatestFromDirecotoryPath(ARSTBasePath);
            string arstlatestmonth = GetLatestFromDirecotoryPath(Path.Combine(ARSTBasePath, latestyear));
            string arstlatestbuild = GetLatestFromDirecotoryPath(Path.Combine(ARSTBasePath, latestyear, latestmonth));
            string arstSVDDynaVM = GetLatestFromDirecotoryPath(Path.Combine(ARSTBasePath, latestyear, latestmonth, latestbuild, "SVD"));
            string ARSTSVDHostPath = Path.Combine(ARSTBasePath, latestyear, latestmonth, latestbuild, "SVD", arstSVDDynaVM, "ATS", "UQAScripts");
            string prevbuild = GetUpdateLastVersionInfo(false, "");
            if (prevbuild.Equals(latestbuild))
            {
                File.AppendAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ATSEmailPraser.txt"), $" {DateTime.Now.ToString()} : Current Version {latestbuild} and Prevous Version {prevbuild} are same Exiting utlity. {Environment.NewLine} ");
                Console.WriteLine($"Exiting as Current Version {latestbuild} and Prevous Version {prevbuild} are same exiting utlity. ");
                return;
            }
            if (!ABFTSVDHostPath.Contains(prevbuild) && !ABFTUPDHostPath.Contains(prevbuild) && !ARSTSVDHostPath.Contains(prevbuild))
            {
                Console.WriteLine($"All 3 Buckets are latest Proceesing with Parsing..");
                Console.WriteLine($"ABFT SVD Latest : {ABFTSVDHostPath.ToString()}");
                Console.WriteLine($"ABFT UPD Latest : {ABFTUPDHostPath}");
                Console.WriteLine($"ARST SVD Latest : {ARSTSVDHostPath}");
                GetUpdateLastVersionInfo(true, latestbuild);
                ParseATSCloudData(ABFTSVDHostPath.ToString(), latestbuild, UPDDynaVM, "ABFTSVD");
                ParseATSCloudData(ABFTUPDHostPath.ToString(), latestbuild, UPDDynaVM, "ABFTUPD");
                ParseATSCloudData(ARSTSVDHostPath.ToString(), latestbuild, UPDDynaVM, "ARSTSVD");
                GenerateFinalSummary(latestbuild, new string[] { "ABFTSVD", "ABFTUPD", "ARSTSVD" });
            }
            else
            {
                Console.WriteLine($"Not All 3 Buckets are latest Will be Parsed when all 3 are Latest.");
                File.AppendAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "ATSEmailPraser.txt"), $"Not All 3 Buckets are latest Will be Parsed when all 3 are Latest. ABFT: {ABFTSVDHostPath} ABFT UPD{ABFTUPDHostPath} , ARST {ARSTSVDHostPath}]");
            }

        }
        static void ParseATSCloudData(string pathABFT, string buildversion, string vmname, string bucket)
        {
            System.Console.WriteLine("Parsing Report ...");
            string path = pathABFT;
            DirectoryInfo dirinf = new DirectoryInfo(path);
            DirectoryInfo[] allsubdirs = dirinf.GetDirectories();
            StringBuilder rept = new StringBuilder();
            rept.AppendLine(string.Format("{0},{1},{2}", "Test Name", "Status", "Test Category"));
            string totalcount = String.Empty;
            string passcount = String.Empty;
            string failcount = String.Empty;
            string skipcount = String.Empty;

            foreach (DirectoryInfo inddir in allsubdirs)
            {
                FileInfo[] outfiles = inddir.GetFiles("*.out");

                foreach (FileInfo outfile in outfiles)
                {
                    // Parse File
                    IEnumerable<string> lines = File.ReadLines(outfile.FullName);
                    bool completetest = lines.Any(x => x.Contains("Total tests:"));
                    bool isMStestoutfile = lines.Any(x => x.Contains("Microsoft (R) Test Execution Command Line Tool Version"));
                    string passstringvs2019 = " �";
                    if (isMStestoutfile && !completetest)
                    {
                        timeouttests_incompletejob.Add(outfile.Name + ":" + bucket);
                    }
                    foreach (string sline in lines)
                    {

                        if (sline.Contains("Failed ") || sline.Contains("X "))
                        {
                            if (sline.Contains("Failed ") && sline.Contains("[") && sline.Contains("]"))
                            {
                                rept.AppendLine(string.Format("{0},{1},{2}", sline.Replace("Failed ", ""), "Failed", outfile.Name.ToString().Replace(".arx", "")));
                            }
                            else if (sline.Contains("X ") && sline.Contains("[") && sline.Contains("]"))
                            {
                                rept.AppendLine(string.Format("{0},{1},{2}", sline.Replace("X ", ""), "Failed", outfile.Name.ToString().Replace(".arx", "")));
                            }
                        }
                        if (sline.Contains("Passed ") || sline.Contains(passstringvs2019))
                        {
                            if (sline.Contains("Passed "))
                            {
                                rept.AppendLine(string.Format("{0},{1},{2}", sline.Replace("Passed ", ""), "Passed", outfile.Name.ToString().Replace(".arx", "")));
                            }
                            else if (sline.Contains(passstringvs2019))
                            {
                                rept.AppendLine(string.Format("{0},{1},{2}", sline.Replace(passstringvs2019, ""), "Passed", outfile.Name.ToString().Replace(".arx", "")));
                            }
                        }
                        if (sline.Contains("Skipped ") || sline.Contains("! "))
                        {
                            if (sline.Contains("Skipped "))
                            {
                                rept.AppendLine(string.Format("{0},{1},{2}", sline.Replace("Skipped ", ""), "Skipped", outfile.Name.ToString().Replace(".arx", "")));
                            }
                            else if (sline.Contains("! "))
                            {
                                rept.AppendLine(string.Format("{0},{1},{2}", sline.Replace("! ", ""), "Skipped", outfile.Name.ToString().Replace(".arx", "")));
                            }
                        }
                        //Sanple summary Total tests: 8. Passed: 6. Failed: 2. Skipped: 0.
                        if (sline.Contains("Total tests:") && sline.Contains("Passed:") && sline.Contains("Failed:") && sline.Contains("Skipped:")) //2017 in one go
                        {
                            totalcount = GetStringBetween(sline, "Total tests:", '.');
                            passcount = GetStringBetween(sline, "Passed:", '.');
                            failcount = GetStringBetween(sline, "Failed:", '.');
                            skipcount = GetStringBetween(sline, "Skipped:", '.');
                            cumtotaltest.Add(Convert.ToInt32(totalcount));
                            cumtotalpass.Add(Convert.ToInt32(passcount));
                            cumtotalfail.Add(Convert.ToInt32(failcount));
                            cumtotalskip.Add(Convert.ToInt32(skipcount));
                        }
                        else if (sline.Contains("Total tests:"))
                        {
                            int inttotoalcount = 0;
                            totalcount = GetStringBetween(sline, "Total tests:", '#');
                            if (totalcount != null && totalcount != String.Empty)
                            {
                                if (Int32.TryParse(totalcount, out inttotoalcount))
                                {
                                    cumtotaltest.Add(inttotoalcount);
                                }
                            }
                        }
                        else if (sline.Contains("Passed:"))
                        {
                            int intpasscount = 0;
                            passcount = GetStringBetween(sline, "Passed:", '#');
                            if (passcount != null && passcount != String.Empty)
                            {
                                if (Int32.TryParse(passcount, out intpasscount))
                                {
                                    cumtotalpass.Add(intpasscount);
                                }
                            }
                        }
                        else if (sline.Contains("Failed:"))
                        {
                            int intfailcount = 0;
                            failcount = GetStringBetween(sline, "Failed:", '#');
                            if (failcount != null && failcount != String.Empty)
                            {
                                if (Int32.TryParse(failcount, out intfailcount))
                                {
                                    cumtotalfail.Add(intfailcount);
                                }
                            }
                        }
                        else if (sline.Contains("Skipped:"))
                        {
                            int intskipcount = 0;
                            skipcount = GetStringBetween(sline, "Skipped:", '#');
                            if (skipcount != null && skipcount != String.Empty)
                            {
                                if (Int32.TryParse(skipcount, out intskipcount))
                                {
                                    cumtotalskip.Add(intskipcount);
                                }
                            }
                        }

                    }

                }
            }
            rept.AppendLine(string.Format("Total Test Count {0}: ", cumtotaltest.Sum()));
            rept.AppendLine(string.Format("Total Pass Count {0}: ", cumtotalpass.Sum()));
            rept.AppendLine(string.Format("Total Fail Count {0}: ", cumtotalfail.Sum()));
            rept.AppendLine(string.Format("Total Skip Count {0}: ", cumtotalskip.Sum()));
            //  System.Console.WriteLine(rept);
            String asterik = new String('*', 100);
            if (File.Exists(Path.Combine(outfilelocation, bucket + ".csv")))
            {
                File.Delete(Path.Combine(outfilelocation, bucket + ".csv"));
            }
            File.AppendAllText(Path.Combine(outfilelocation, bucket + ".csv"), rept.ToString());




        }
        static string GetStringBetween(string mainstring, string StartString, char endstring)
        {
            string res = mainstring.Substring(mainstring.IndexOf(StartString));
            if (endstring.Equals('.'))
            {
                res = res.Substring(0, res.IndexOf(endstring));
            }
            else
            {
                res = res.Substring(0, res.Length);
            }
            res = res.Replace(StartString, "");
            return res;
        }
        private static void SendEmail(string subject, string body)
        {
            int port = Convert.ToInt32(ConfigurationManager.AppSettings.Get("SMTPPORT"));
            string host = ConfigurationManager.AppSettings.Get("SMTPSERVER");
            string username = ConfigurationManager.AppSettings.Get("SMTPUSER");
            string fromdisplayName = "MTC";
            string password = ConfigurationManager.AppSettings.Get("SMTPPWD");
            string mailFrom = "noreply@ATS.com";
            string[] arrnames = ConfigurationManager.AppSettings.Get("NameList").Split(new char[] { ';' });
            string[] emails = ConfigurationManager.AppSettings.Get("EmailList").Split(new char[] { ';' });
            List<MailboxAddress> addresslist = new List<MailboxAddress>();
            int i = 0;
            foreach (var arranme in arrnames)
            {
                addresslist.Add(new MailboxAddress(arranme, emails[i]));
                i++;
            }
            foreach (var item in addresslist)
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromdisplayName, mailFrom));
                message.To.Add(item);
                message.Subject = subject;
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = body;
                message.Body = bodyBuilder.ToMessageBody();
                //   message.Body = new TextPart("plain") { Text = body };
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect(host, port, SecureSocketOptions.None);
                    //NetworkCredential networkCredential = new NetworkCredential(username, password);
                    //client.Authenticate(networkCredential);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
        }
        public static string GetInstalledAppsVersion(string Appname)
        {
            //Weatherford ForeSite Bundle
            string reqdisplayversion = String.Empty;
            List<RegistryKey> lstInstalled = new List<RegistryKey>();
            string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {
                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        try
                        {
                            //   Console.WriteLine($"Regsitery Key Disaply Name: {sk.GetValue("DisplayName")}");
                            if (sk.GetValue("DisplayName").ToString().ToLower() == Appname.ToLower())
                            {
                                reqdisplayversion = sk.GetValue("DisplayVersion").ToString();
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            return reqdisplayversion;

        }
        public static string GetLatestFromDirecotoryPath(string path)
        {
            var directory = new DirectoryInfo(path);
            string latest = (from f in directory.GetDirectories()
                             orderby f.CreationTime descending
                             select f.Name).First();
            return latest;
        }
        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            StreamReader sr = new StreamReader(strFilePath);
            string[] headers = sr.ReadLine().Split(',');
            DataTable dt = new DataTable();
            foreach (string header in headers)
            {
                dt.Columns.Add(header);
            }
            while (!sr.EndOfStream)
            {

                string[] rows = sr.ReadLine().Split(',');
                if (rows.Length == 3)
                {
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }
        public static string ProcessInformationForFailureData(string csvpath, string bucket)
        {
            DataTable failedones = ConvertCSVtoDataTable(csvpath);
            DataTable failuretestdetails = failedones.AsEnumerable().Where(row => row.Field<String>("Status") == "Failed").CopyToDataTable();
            StringBuilder erept = new StringBuilder();
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"<p> Failed Test Details : {bucket} : ( {failuretestdetails.Rows.Count.ToString()} out of  {failedones.Rows.Count.ToString()} Tests ) </p>");
            sb.AppendLine("<table class='details'>");
            sb.AppendLine("<tr><th>Test Name</th><th>Test Category</th></tr>");
            foreach (DataRow drow in failuretestdetails.Rows)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td style='width: 150px'>{drow["Test Name"].ToString()}</td>");
                sb.AppendLine($"<td style='width: 500px'>{drow["Test Category"].ToString()}</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table><br/>");
            string msg = sb.ToString();
            return msg;
        }
        public static void GenerateFinalSummary(string buildversion, string[] buckets)
        {
            SEND_EMAILS = ConfigurationManager.AppSettings.Get("SendEmail");
            string status = cumtotalfail.Sum() == 0 ? "Passed" : "Failed";
            StringBuilder erept = new StringBuilder();
            String asterik = new String('*', 100);
            string emailsub = $"Quick Summary for  ATS Email Execution results for ForeSite build ABFT Clean ,ABFT UPD and ARST Clean {buildversion}:  {status}";
            StringBuilder failedtestdetails = new StringBuilder();
            foreach (var bucket in buckets)
            {
                string bucketdesc = String.Empty;
                switch (bucket)
                {
                    case "ABFTSVD":
                        bucketdesc = "ABFTSVD : Clean Install";
                        break;
                    case "ABFTUPD":
                        bucketdesc = "ABFTUPD : Upgrade Tests";
                        break;
                    case "ARSTSVD":
                        bucketdesc = "ARSTSVD : ARST Regression";
                        break;
                    default: break;
                }
                if (bucketdesc.Length > 0)
                {
                    failedtestdetails.AppendLine(ProcessInformationForFailureData(Path.Combine(outfilelocation, bucket + ".csv"), bucketdesc));
                }
            }
            erept.AppendLine("<html>");
            erept.AppendLine("<head><style>");
            erept.AppendLine("table { border-collapse: collapse; table-layout: fixed; }");
            erept.AppendLine("td, th { border: thin solid black }");
            erept.AppendLine("th { background-color: #82a0d1; color: white; }");
            erept.AppendLine("table.details td { width: 100px }");
            erept.AppendLine("</head></style>");
            erept.AppendLine("<body>");
            erept.AppendLine("<table class='details'>");
            erept.AppendLine("<tr>");
            erept.AppendLine($"<td style='width: 150px'>Total Test Count: </td>");
            erept.AppendLine($"<td style='width: 300px'>{cumtotaltest.Sum()}</td>");
            erept.AppendLine("</tr>");
            erept.AppendLine("<tr>");
            erept.AppendLine($"<td style='width: 150px'>Total Pass Count: </td>");
            erept.AppendLine($"<td style='width: 300px'>{cumtotalpass.Sum()}</td>");
            erept.AppendLine("</tr>");
            erept.AppendLine("<tr>");
            erept.AppendLine($"<td style='width: 150px'>Total Fail Count: </td>");
            erept.AppendLine($"<td style='width: 300px'>{cumtotalfail.Sum()}</td>");
            erept.AppendLine("</tr>");
            erept.AppendLine("<tr>");
            erept.AppendLine($"<td style='width: 150px'>Total Skip Count: </td>");
            erept.AppendLine($"<td style='width: 300px'>{cumtotalskip.Sum()}</td>");
            erept.AppendLine("</tr>");
            erept.AppendLine("</table><br/>");
            if (timeouttests_incompletejob.Count > 0)
            {
                erept.AppendLine("<table class='details'>");
                erept.AppendLine("<tr><th> InComplete / Timed Out Test Category Name </th></tr>");
                foreach (string jobname in timeouttests_incompletejob)
                {
                    erept.AppendLine("<tr>");
                    erept.AppendLine($"<td style='width: 150px'>{jobname}</td>");
                    erept.AppendLine("</tr>");
                }
                erept.AppendLine("</table><br/>");
            }
            erept.AppendLine($"<br>");
            erept.AppendLine($"{asterik.ToString()}");
            erept.AppendLine(failedtestdetails.ToString());
            erept.AppendLine($"<br>");
            erept.AppendLine($"{asterik.ToString()}");
            erept.AppendLine($"<br>");
            erept.AppendLine($"This is Automated Test Email.Do not Reply to this email. Mail is sent from :{ Environment.MachineName}");
            erept.AppendLine($"<br>");
            erept.AppendLine(asterik.ToString());
            erept.AppendLine("</body>");
            erept.AppendLine("</html>");
            string msg = erept.ToString();
            if (SEND_EMAILS.ToLower().Equals("false"))
            {
                File.AppendAllText(Path.Combine(outfilelocation, "Report.html"), erept.ToString());
            }
            else
            {
                SendEmail(emailsub, msg);
            }

        }
    }
}
