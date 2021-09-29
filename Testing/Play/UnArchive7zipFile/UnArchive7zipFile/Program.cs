using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnArchive7zipFile
{
    class Program
    {
        static void Main(string[] args)
        {
            ExtractFile extract = new ExtractFile();
            string sourcearhivename = ConfigurationManager.AppSettings.Get("SourceArhiveName");
            string destinationfolder = ConfigurationManager.AppSettings.Get("DestinationFolder");
            string filetoarchivesfolderpath = ConfigurationManager.AppSettings.Get("Filetoarchivesfolderpath");
            string action = ConfigurationManager.AppSettings.Get("Action");
            switch (action.ToLower())
            {
                case "archive":
                    {
                        DirectoryInfo dinfo = new DirectoryInfo(filetoarchivesfolderpath);
                       // var files = dinfo.GetFiles();
                        var files =(from f in dinfo.GetFiles()
                         orderby f.CreationTime descending
                         select f).ToList();
                        string filename = files[0].Name;
                        extract.ArchiveFile7Zip(Path.Combine(filetoarchivesfolderpath, filename), Path.Combine(filetoarchivesfolderpath, filename.Replace(".BAK",".7z")));
                        // If 7 Z is created delete the .BAK file to save on disk space
                        if (File.Exists(Path.Combine(filetoarchivesfolderpath, filename.Replace(".BAK", ".7z"))))
                        {
                            File.Delete(Path.Combine(filetoarchivesfolderpath, filename));
                        }

                        break;
                    }

                case "extract":
                    {
                        extract.ExtractFile7Zip(sourcearhivename, destinationfolder);
                        break;
                    }
                default:
                    throw new Exception("Unknown Action");
            }

        }



    }
}
