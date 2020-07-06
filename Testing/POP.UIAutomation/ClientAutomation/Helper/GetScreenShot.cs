using Microsoft.VisualStudio.TestTools.UITesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelerikTest.Helper
{
    public class GetScreenShot
    {
        public static string Capture(string testname, string screenShotName)
        {

            string screenshotPath = ConfigurationManager.AppSettings["ScreenshotPath"];
           // string directory = "C:\\Telelrik Autoamtion Solution\\TelerikTest\\TestResults\\" + testname;
            string directory = Path.Combine(screenshotPath, testname);
            //Create Directory if not present
            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            //Constructing filename
            string FilePath = directory + "\\" + screenShotName + "_" + DateTime.Now.ToString("MMddyyyy_hhmmss") + ".png";
            System.Drawing.Image MyImage = UITestControl.Desktop.CaptureImage();

            //Saving File at FilePath
            MyImage.Save(FilePath, System.Drawing.Imaging.ImageFormat.Png);

            return FilePath;
        }
    }

}
