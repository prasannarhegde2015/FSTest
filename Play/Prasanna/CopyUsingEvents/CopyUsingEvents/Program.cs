using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;
using Outlook = Microsoft.Office.Interop.Outlook;


namespace ConsoleApplication1
{
    public static class globalvar
    {
        private static string _dpath;
        private static string _logpath, _emaillist, _bkbthfile, _buildname;
        private static bool _cfile;
        public static string Dpath
        {
            get { return _dpath; }
            set { _dpath = value; }
        }

        public static string Logpath
        {
            get { return _logpath; }
            set { _logpath = value; }
        }
        public static string EmailList
        {
            get { return _emaillist; }
            set { _emaillist = value; }
        }
        public static string BuildName
        {
            get { return _buildname; }
            set { _buildname = value; }
        }
        public static string bkBacthFile
        {
            get { return _bkbthfile; }
            set { _bkbthfile = value; }
        }

        public static bool chkfile
        {
            get { return _cfile; }
            set { _cfile = value; }
        }

    }
    class Program
    {

        static void Main(string[] args)
        {

            mailtest();
        }

        static void MyApplication_NewMailEx(string anEntryID)
        {

        }
        static void mailtest()
        {
            string folderName = ConfigurationManager.AppSettings["olfoldername"];
            string strSearchText = ConfigurationManager.AppSettings["olsubname"];
            string dstfldrpath = ConfigurationManager.AppSettings["dstpath"];
            string emaillist = ConfigurationManager.AppSettings["EmailList"];
            globalvar.EmailList = emaillist;
            //************************ Declare OutLook Interop Variables ****************************************
            Outlook.Application oApp = new Outlook.Application();
            Outlook.NameSpace oNS = oApp.GetNamespace("MAPI");
            Microsoft.Office.Interop.Outlook.MAPIFolder reqfolder = null;
            Microsoft.Office.Interop.Outlook._Folders oFolders;
            Microsoft.Office.Interop.Outlook.MAPIFolder oPublicFolder =
                oNS.GetDefaultFolder(Microsoft.Office.Interop.Outlook.OlDefaultFolders.olFolderInbox).Parent;
            //Folders at Inbox level
            oFolders = oPublicFolder.Folders;
            foreach (Microsoft.Office.Interop.Outlook.MAPIFolder Folder in oFolders)
            {
                string foldername = Folder.Name;
                if (foldername.ToLower() == folderName.ToLower())
                {
                    reqfolder = Folder;
                    break;
                }

            }

            string msgbody = null;
            foreach (Outlook.MailItem ind_items in reqfolder.Items)
            {
                if (ind_items.Subject.Contains(strSearchText))
                {
                    msgbody = msgbody + ind_items.Body;
                }
                else
                {

                    Console.WriteLine("Message is skipped as Subject line was not matching");
                }
            }

            Console.WriteLine(msgbody);
            string pattn = "\".*.exe\"";
            Console.WriteLine("Parsed String from [ " + folderName + "  ] folder  is: " + regexpmatch(msgbody, pattn));

            string[] srcpaths = regexpmatch(msgbody, pattn).Split(new char[] { ';' });
            int cnt = 1;
            foreach (string indpath in srcpaths)
            {
                Console.WriteLine("Path " + cnt + " is " + indpath);
                cnt++;
            }
            Console.WriteLine("The path parsed from latest email obtained is:  " + srcpaths[srcpaths.Length - 2]);

            string[] cutstrings = getLastCutstrings(srcpaths[srcpaths.Length - 2]).Split(new char[] { ';' });

            Console.WriteLine("Source path from Email parsed: " + cutstrings[0]);
            Console.WriteLine("File Name from Email parsed: " + cutstrings[1]);

            Console.WriteLine("Src " + cutstrings[0]);
            Console.WriteLine("Dst " + dstfldrpath);
            Console.WriteLine("file " + cutstrings[1]);
            //  Console.ReadLine();
            dorobocopy(cutstrings[0], dstfldrpath, cutstrings[1]);
            sendemail(globalvar.EmailList, dstfldrpath + "\\" + cutstrings[1]);
        }

        private static string regexpmatch(string instring, string pttn)
        {
            string regexpmatch = "";
            string regexpmatch1 = "";
            string apos = "\"";
            Regex re = new Regex(pttn);
            MatchCollection omatches = re.Matches(instring);
            foreach (Match indmatch in omatches)
            {
                //  regexpmatch = indmatch.Value;
                regexpmatch = regexpmatch + indmatch.Value + ";";
            }
            //    Console.WriteLine(regexpmatch);
            if (regexpmatch.Contains(apos))
            {
                regexpmatch1 = regexpmatch.Replace(apos, "");
            }
            return regexpmatch1;
        }

        private static void dorobocopy(string src, string dst, string fln)
        {


            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "robocopy.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = "  \"" + src + "\"" + " \"" + dst + "\"" + " \"" + fln + "\"";
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

        private static void sendemail(string ListTo, string fileName)
        {
            try
            {

                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                string[] recipients = ListTo.Split(';');
                foreach (string recipient in recipients)
                {
                    message.To.Add(recipient);
                }
                message.Subject = "New build for " + ConfigurationManager.AppSettings["olfoldername"] + "Automated Copy Process";
                message.From = new System.Net.Mail.MailAddress("noreply@bugnet-vm1.com");
                message.Body = globalvar.BuildName + " downloaded @ location :" + fileName;
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("mail2.weatherford.com");
                smtp.Port = 25;

                /*    foreach (string attachmentFilename in attachments)
                    {
                        if (System.IO.File.Exists(attachmentFilename))
                        {
                            var attachment = new System.Net.Mail.Attachment(attachmentFilename);
                            message.Attachments.Add(attachment);
                        }
                    } */

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Sending Mails.." + ex.Message);
            }
        }
        public static string getLastCutstrings(string strlbksl)
        {
            int lastindexpos = strlbksl.LastIndexOf("\\");
            string srcfldrpath = strlbksl.Substring(0, lastindexpos);
            string strilename = strlbksl.Substring(lastindexpos + 1, strlbksl.Length - lastindexpos - 1);
            string op = "";
            op = srcfldrpath + ";" + strilename;
            return op;
        }
    }
}