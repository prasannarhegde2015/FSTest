using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Getting path of directories");
            string arxbasepath = ConfigurationManager.AppSettings.Get("arxbasepath");
            string uqasubdirectoreis = ConfigurationManager.AppSettings.Get("uqasubdirectoreis");
            string origtokenvalues = ConfigurationManager.AppSettings.Get("origtokenvalues");
            string newtokenvalues = ConfigurationManager.AppSettings.Get("newtokenvalues");
            string[] origtokenarr = origtokenvalues.Split(new char[] { ';' });
            string[] newtokearr = newtokenvalues.Split(new char[] { ';' });
            string[] parsefolders = uqasubdirectoreis.Split(new char[] { ';' });
            foreach (string parsefolder in parsefolders)
            {
                DirectoryInfo subdir = new DirectoryInfo(Path.Combine(arxbasepath, parsefolder));
                FileInfo[] files = subdir.GetFiles("*.arx");


                foreach (FileInfo file in files)
                {
                    FileAttributes attributes = File.GetAttributes(file.FullName);

                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        // Make the file RW
                        attributes = RemoveAttribute(attributes, FileAttributes.ReadOnly);
                        File.SetAttributes(file.FullName, attributes);
                        Console.WriteLine("The {0} file is no longer RO.", file.FullName);
                    }
                    int i = 0;
                    string text = File.ReadAllText(file.FullName);

                    foreach (string indtoke in origtokenarr)
                    {
                        text = text.Replace(indtoke, newtokearr[i]);
                        File.WriteAllText(file.FullName, text);
                        i++;
                    }
                }
            }
        }

        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }
    }
}