using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace copyonlylatest
{
    class Program
    {
        static void Main(string[] args)
        {
           // BuildCopy();
            UQACopy();
        }

        static void BuildCopy()
        {
            LogMessage("************************Copy Process Started **************** ");
            string src = System.Configuration.ConfigurationManager.AppSettings["src"];
            string dst = System.Configuration.ConfigurationManager.AppSettings["dst"];
            string attempts = System.Configuration.ConfigurationManager.AppSettings["attempts"];
            string islatest = System.Configuration.ConfigurationManager.AppSettings["latest"];
            int chkdays = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["Hourstocheck"]);
            string ForeSiteDepFolders = System.Configuration.ConfigurationManager.AppSettings["desiredFolders"];
            string targetsiglefile = System.Configuration.ConfigurationManager.AppSettings["targetfile"];
            string skipfolders = System.Configuration.ConfigurationManager.AppSettings["skipfolders"];
            var directory = new DirectoryInfo(src);
            DateTime dtnow = DateTime.Now;
            DateTime dt3dbfr = dtnow.Subtract(TimeSpan.FromHours(chkdays));
            List<string> todayBuilds = (from f in directory.GetDirectories()
                                        orderby f.LastWriteTime descending
                                        where f.CreationTime > dt3dbfr
                                        select f.Name).ToList();


            //get Refined folder list you may want to omit certain folder/s
            string[] skiparr = skipfolders.Split(new char[] { ';' });
            foreach (string indskipfld in skiparr)
            {
                todayBuilds.Remove(indskipfld);
            }


            if (todayBuilds.Count == 0)
            {
                LogMessage("No Latest Files were Avialble whne checked this time: between:   " + dtnow.ToString() + "  and " + dt3dbfr.ToString());
            }
            else
            {
                LogMessage("Today files count whne checked this time: between :   " + dtnow.ToString() + "  and " + dt3dbfr.ToString() + "  " + todayBuilds.Count.ToString());
                foreach (var iii in todayBuilds)
                {
                    LogMessage("Folder Name: " + iii);
                }
            }
            LogMessage("Source Location used" + src);
            if (islatest.ToLower() == "y") //copy only latest
            {
                LogMessage("Copying only latest  ******************");
                string indfolder = todayBuilds[0];
                Console.WriteLine("Folder  Name : = " + indfolder);

                string[] buildDirectory = Directory.GetDirectories(Path.Combine(src, indfolder));
                string[] allfiles = Directory.GetFiles(Path.Combine(src, indfolder));
                string[] reqfolders = ForeSiteDepFolders.Split(new char[] { ';' });
                string reqfile = "";
                if (targetsiglefile.Length > 0)
                {
                    foreach (string file in allfiles)
                    {
                        if (file.Contains(targetsiglefile))
                        {
                            reqfile = file;
                            break;
                        }
                    }
                    src = Path.Combine(src, indfolder);
                    reqfile = reqfile.Replace(src, "");
                    reqfile = reqfile.Replace("\\", "");
                    dorobocopy(src, dst, reqfile);
                }
                else
                {
                    foreach (string fldr in reqfolders)
                    {
                        LogMessage("File Name : = " + fldr);
                        if (!Directory.Exists(Path.Combine(dst, indfolder, fldr)))
                        {
                            Directory.CreateDirectory(Path.Combine(dst, indfolder, fldr));
                        }
                        LogMessage("Copying only latest from : ******************" + Path.Combine(src, indfolder, fldr));
                        dorobocopyFolders(Path.Combine(src, indfolder, fldr), Path.Combine(dst, indfolder, fldr));
                    }
                }
                LogMessage("************************Copy Process Completed Copied latest build only !!!**************** ");
            }
            else //copy all 
            {
                LogMessage("Copying All items as per Time Filter set in app.config  ******************");
                foreach (var indfolder in todayBuilds)
                {
                    Console.WriteLine("Folder  Name : = " + indfolder);
                    string[] buildDirectory = Directory.GetDirectories(Path.Combine(src, indfolder));
                    string[] reqfolders = ForeSiteDepFolders.Split(new char[] { ';' });
                    foreach (string fldr in reqfolders)
                    {
                        LogMessage("File Name : = " + fldr);
                        if (!Directory.Exists(Path.Combine(dst, indfolder, fldr)))
                        {
                            Directory.CreateDirectory(Path.Combine(dst, indfolder, fldr));
                        }

                        dorobocopyFolders(Path.Combine(src, indfolder, fldr), Path.Combine(dst, indfolder, fldr));
                    }
                    LogMessage("************************Copy Process Completed !!!**************** ");
                }
            }
        }

        static void UQACopy()
        {
            LogMessage("************************Copy UQA Process Started **************** ");
            string src = System.Configuration.ConfigurationManager.AppSettings["UQAsrc"];
            string dst = System.Configuration.ConfigurationManager.AppSettings["UQAdst"];
            string attempts = System.Configuration.ConfigurationManager.AppSettings["attempts"];
            string islatest = System.Configuration.ConfigurationManager.AppSettings["latest"];
            int chkdays = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["Hourstocheck"]);
            string ForeSiteDepFolders = System.Configuration.ConfigurationManager.AppSettings["desiredFolders"];
            string targetsiglefile = System.Configuration.ConfigurationManager.AppSettings["targetfile"];
            targetsiglefile = "";
            string skipfolders = System.Configuration.ConfigurationManager.AppSettings["skipfolders"];
            var directory = new DirectoryInfo(src);
            DateTime dtnow = DateTime.Now;
            DateTime dt3dbfr = dtnow.Subtract(TimeSpan.FromHours(chkdays));
            List<string> todayBuilds = (from f in directory.GetDirectories()
                                        orderby f.LastWriteTime descending
                                        where f.CreationTime > dt3dbfr
                                        select f.Name).ToList();


            //get Refined folder list you may want to omit certain folder/s
            string[] skiparr = skipfolders.Split(new char[] { ';' });
            foreach (string indskipfld in skiparr)
            {
                todayBuilds.Remove(indskipfld);
            }


            if (todayBuilds.Count == 0)
            {
                LogMessage("No Latest Files were Avialble whne checked this time: between:   " + dtnow.ToString() + "  and " + dt3dbfr.ToString());
            }
            else
            {
                LogMessage("Today files count whne checked this time: between :   " + dtnow.ToString() + "  and " + dt3dbfr.ToString() + "  " + todayBuilds.Count.ToString());
                foreach (var iii in todayBuilds)
                {
                    LogMessage("Folder Name: " + iii);
                }
            }
            LogMessage("Source Location used" + src);
            if (islatest.ToLower() == "y") //copy only latest
            {
                LogMessage("Copying only latest  ******************");
                string indfolder = todayBuilds[0];
                Console.WriteLine("Folder  Name : = " + indfolder);

                string[] buildDirectory = Directory.GetDirectories(Path.Combine(src, indfolder));
                string[] allfiles = Directory.GetFiles(Path.Combine(src, indfolder));
                string[] reqfolders = ForeSiteDepFolders.Split(new char[] { ';' });
                string reqfile = "";
                if (targetsiglefile.Length > 0)
                {
                    foreach (string file in allfiles)
                    {
                        if (file.Contains(targetsiglefile))
                        {
                            reqfile = file;
                            break;
                        }
                    }
                    src = Path.Combine(src, indfolder);
                    reqfile = reqfile.Replace(src, "");
                    reqfile = reqfile.Replace("\\", "");
                    dorobocopy(src, dst, reqfile);
                }
                else
                {
                    foreach (string fldr in reqfolders)
                    {
                        LogMessage("File Name : = " + fldr);
                        if (!Directory.Exists(Path.Combine(dst, indfolder, fldr)))
                        {
                            Directory.CreateDirectory(Path.Combine(dst, indfolder, fldr));
                        }
                        LogMessage("Copying only latest from : ******************" + Path.Combine(src, indfolder, fldr));
                        dorobocopyFolders(Path.Combine(src, indfolder, fldr), Path.Combine(dst, indfolder, fldr));
                    }
                }
                LogMessage("************************Copy Process Completed Copied latest build only !!!**************** ");
            }
            else //copy all 
            {
                LogMessage("Copying All items as per Time Filter set in app.config  ******************");
                foreach (var indfolder in todayBuilds)
                {
                    Console.WriteLine("Folder  Name : = " + indfolder);
                    string[] buildDirectory = Directory.GetDirectories(Path.Combine(src, indfolder));
                    string[] reqfolders = ForeSiteDepFolders.Split(new char[] { ';' });
                    foreach (string fldr in reqfolders)
                    {
                        LogMessage("File Name : = " + fldr);
                        if (!Directory.Exists(Path.Combine(dst, indfolder, fldr)))
                        {
                            Directory.CreateDirectory(Path.Combine(dst, indfolder, fldr));
                        }

                        dorobocopyFolders(Path.Combine(src, indfolder, fldr), Path.Combine(dst, indfolder, fldr));
                    }
                    LogMessage("************************Copy Process Completed !!!**************** ");
                }
            }
        }

        #region RequiredFunctions
        private static void dorobocopy(string src, string dst, string fln)
        {
            LogMessage("File Name : = " + fln);
            if (src != "" && dst != "" && fln != "")
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.FileName = "robocopy.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //  startInfo.Arguments = " \"" + src + "\"" + " \"" + dst + "  " + fln +"/ z";
                string args = "  \"" + src + "\"" + " \"" + dst + "\"" + "  " + "  \"" + fln + "\"" + " /z ";
                Console.WriteLine(args);
                startInfo.Arguments = args;

                try
                {
                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();
                    }
                }
                catch
                {
                    // Log error.
                }
            }
        }
        private static void dorobocopyFolders(string src, string dst)
        {

            if (src != "" && dst != "")
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.FileName = "robocopy.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.Arguments = "  \"" + src + "\"" + " \"" + dst + "\"" + " *.* /e /z ";
                try
                {
                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();
                    }
                }
                catch
                {
                    // Log error.
                }
            }
        }
        private static void LogMessage(string txt)
        {
            System.IO.File.AppendAllText(Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "CopyLog.txt"), "[" + System.DateTime.Now.ToString() + "] : " + txt + Environment.NewLine);
        }

        #endregion
    }



}