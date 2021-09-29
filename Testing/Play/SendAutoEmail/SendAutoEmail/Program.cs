using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SendAutoEmail
{
    class Program
    {
        static void Main(string[] args)
        {
            //  SendEmail();
            //   SendEmailFromSMTP();
            //  GetInstalledApps();
            //   Console.ReadLine();
            SendGmailEmail("Test Email Local", "Ignore for now");


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
                message.Subject = "New build for  Automated Copy Process";
                message.From = new System.Net.Mail.MailAddress("noreply@bugnet-vm1.com");
                message.Body = " downloaded @ location :" + fileName;
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.office365.com");
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
       

       
        private static void SendGmailEmail(string subject, string body)
        {
            int port = 465;
            string host = "smtp.gmail.com";
            string username = "devopsuser2018@gmail.com";
            string fromdisplayName = "MTC";
            string password = "ForeSite430406";
            string mailFrom = "noreply@ATS.com";
            var toAddress = new MailboxAddress( "Prasanna", "prasanna.hegde@weatherford.com");
            var toAddress2 = new MailboxAddress( "Swati", "swati.dumbre@weatherford.com");

            List<MailboxAddress> addresslist = new List<MailboxAddress>();
            addresslist.Add(toAddress);
            addresslist.Add(toAddress2);

            foreach (var item in addresslist)
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromdisplayName, mailFrom));
                message.To.Add(item);
                message.Subject = subject;
                message.Body = new TextPart("plain") { Text = body };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect(host, port, SecureSocketOptions.Auto);
                    NetworkCredential networkCredential = new NetworkCredential(username, password);
                    client.Authenticate(networkCredential);
                    client.Send(message);
                    client.Disconnect(true);
                } 
            }
        }


        public static string GetInstalledApps(string Appname)
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
    }
}
