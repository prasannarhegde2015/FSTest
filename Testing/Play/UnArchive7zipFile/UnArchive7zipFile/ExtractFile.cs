using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnArchive7zipFile
{
    class ExtractFile
    {

        public void ExtractFile7Zip(string sourceArchive, string destination)
        {
            string zPath = "7ZaExe\\7za.exe"; //add to proj and set CopyToOuputDir
            try
            {
                ProcessStartInfo pro = new ProcessStartInfo();
                pro.WindowStyle = ProcessWindowStyle.Hidden;
                pro.FileName = zPath;
                pro.Arguments = string.Format("x \"{0}\" -y -o\"{1}\"", sourceArchive, destination);
                Process x = Process.Start(pro);
                x.WaitForExit();
            }
            catch (System.Exception Ex)
            {
                //handle error
            }
        }

        public void ArchiveFile7Zip(string sourceArchive, string destination)
        {
            string zPath = "7ZaExe\\7za.exe"; //add to proj and set CopyToOuputDir
            try
            {
                ProcessStartInfo pro = new ProcessStartInfo();
                pro.WindowStyle = ProcessWindowStyle.Hidden;
                pro.FileName = zPath;
                pro.Arguments = $"a \"{destination}\"  \"{sourceArchive}\" ";
                Process x = Process.Start(pro);
                x.WaitForExit();
            }
            catch (System.Exception Ex)
            {
                //handle error
                Console.WriteLine($"Got Error {Ex.Message}");
            }
        }
    }
}
