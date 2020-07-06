using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SendAutoEmail
{
    class Program
    {
        static void Main(string[] args)
        {
            SendEmail();
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
                message.Body =  " downloaded @ location :" + fileName;
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
        private static void SendEmail()
        {
            String userName = "WFT\\E159279";
            String password = "Lowis2020JAN;";
            MailMessage msg = new MailMessage("prasanna.hegde@weatherford.com", "prasanna.hegde@weatherford.com");
            msg.Subject = "Your Subject Name";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Name: " );
            sb.AppendLine("Mobile Number: " );
            sb.AppendLine("Email:" );
            sb.AppendLine("Drop Downlist Name:" );
            msg.Body = sb.ToString();
            //Attachment attach = new Attachment(Server.MapPath("folder/" + ImgName));
            //msg.Attachments.Add(attach);
            SmtpClient SmtpClient = new SmtpClient();
            SmtpClient.Credentials = new System.Net.NetworkCredential(userName, password);
            SmtpClient.Host = "smtp.office365.com";
            SmtpClient.Port = 587;
            SmtpClient.EnableSsl = true;
            SmtpClient.Send(msg);
        }
    }
}
