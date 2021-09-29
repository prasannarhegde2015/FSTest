using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ExcelToXmlUtility
{
    class Program
    {
        public static string excelfile = "";
        public static string xmlfile = "";
        public static string strdatasetname = "";
        public static string strtablname = "";
        static void Main(string[] args)
        {
            Console.WriteLine("*********************Usage : ********************");
            Console.WriteLine("******Required File Format *.xls and sheet name ='mapping' : ********************");
            Console.WriteLine("Enter '1' to Convert Single File**********************");
            Console.WriteLine("Enter '2' for batch Process **********************");
            Console.WriteLine("Enter Processing Option '1' or '2' **********************");
            string userinput =Console.ReadLine();
            if (userinput != "1" && userinput != "2" )
            {
                Console.WriteLine("Invalid User Input ..Please Enter Valid Optiions and Gry again");
                return;
            }

            switch (userinput)
            {
                case "1":
                    {
                        Console.WriteLine($"Enter Absolute Path for Excel File (*.xls) eg 'c:\\logs\\Test.xls' **********************");
                        string inputexcel = Console.ReadLine();
                        if (!File.Exists(inputexcel))
                        {
                            Console.WriteLine("File Not found : Please make sure path entered is valid");
                            return;
                        }
                        CreateXML(inputexcel);
                        break;
                        
                    };
                case "2":
                    {
                        Console.WriteLine($"Enter Absolute Path for Directory having Excel Files (*.xls) eg 'c:\\logs' **********************");
                        string inputexcel = Console.ReadLine();
                        if (!Directory.Exists(inputexcel) && Directory.GetFiles(inputexcel,"*.xls").Length ==0)
                        {
                            Console.WriteLine("Direcoty Not found or No Excel Files in that directory : Please make sure path entered is valid");
                            return;
                        }
                        CreateXmlsinGo(inputexcel);
                        break;

                    }
                default:
                    {
                        break;
                    }
            }

        }

        static void CreateXML(string xlsfile)
        {
            xmlfile = xlsfile.Replace("xls", "xml");
            generateXmlFilefromDataTable(xlsfile,"DataSet", "record");
            DataTable dtnew = BuildDataTableFromXml(xmlfile);
            Console.WriteLine("End to End Testing was completed");
        }

        static DataTable exceltoDataTable(string xlsfilepath)
        {
            OdbcConnection conn2 = new OdbcConnection();
            try
            {
                DataTable dt2 = new DataTable();
                if (File.Exists(xlsfilepath))
                {
                    string filename = Path.GetFileNameWithoutExtension(xlsfilepath);
                    filename = filename + ".xls";
                    Console.WriteLine($"File Name of Excel is: {filename} ");
                    string localpath = Directory.GetCurrentDirectory();
                    Console.WriteLine($"Currnet Path is: {localpath} ");
                    if (!File.Exists(Path.Combine(localpath, filename)))
                    {
                        Console.WriteLine($"Doing Copy ...Source : {xlsfilepath}   Destination {Path.Combine(localpath, filename)} ");
                        File.Copy(xlsfilepath, Path.Combine(localpath, filename));
                    }
                    conn2.ConnectionString = @"Driver={Microsoft Excel Driver (*.xls)};DriverId=790;ReadOnly=0;Dbq=" + filename;
                    conn2.Open();
                    string strcmdText = "Select * from [mapping$]";
                    OdbcCommand cmd = new OdbcCommand(strcmdText);
                    cmd.Connection = conn2;
                    //OdbcDataReader reder = cmd.ExecuteReader();
                    OdbcDataAdapter da = new OdbcDataAdapter(cmd);
                    da.Fill(dt2);
                }
                else
                {
                    Console.WriteLine($"File Name Provided {xlsfilepath} does not exist ");
                }
                return dt2;
            }
            finally
            {
                conn2.Close();
                conn2.Dispose();
            }
        }

        static void generateXmlFilefromDataTable(string excelfile, string datasetname, string tblname)
        {
            DataTable tbl = exceltoDataTable(excelfile);
            DataSet dataSet = new DataSet(datasetname);
            tbl.TableName = tblname;
            dataSet.Tables.Add(tbl);
            dataSet.WriteXml(xmlfile);
            string rawxml = File.ReadAllText(xmlfile);
            string cleanxml = "";
            //We  may need more of such cleanup
            string[] extranouscharctersarray = new string[] { "_x0020_" , "_x0021_" , "_x0022_",
                                                             "_x0023_","_x0024","_x0025",
                                                             "_x0026_","_x0027_","_x0028_",
                                                              "_x0029_","_x002F_d","_x00B3_"};
            foreach (string extran in extranouscharctersarray)
            {
                rawxml = rawxml.Replace(extran, "");
            }
            cleanxml = rawxml;
            File.WriteAllText(xmlfile, cleanxml);
            Console.WriteLine("Output Xml generated at path " + xmlfile);
        }
        //Target Consumer Test Method
        public static DataTable BuildDataTableFromXml(string XMLString)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(XMLString);
            DataTable Dt = new DataTable();
            try
            {

                XmlNodeList NodoEstructura = doc.GetElementsByTagName("record");
                int count = 1;
                foreach (XmlNode indnode in NodoEstructura)
                {

                    XmlNodeList subnodes = indnode.ChildNodes;
                    //  Table structure (columns definition) 
                    foreach (XmlNode columna in subnodes)
                    {
                        if (count > 1)
                        {
                            break;
                        }
                        Dt.Columns.Add(columna.Name, typeof(String));
                    }

                    XmlNode Filas = doc.FirstChild;
                    //  Data Rows 
                    List<string> Valores = new List<string>();
                    foreach (XmlNode Columna in subnodes)
                    {
                        Valores.Add(Columna.InnerText);
                    }
                    Dt.Rows.Add(Valores.ToArray());
                    count++;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return Dt;
        }
        /// <summary>
        /// Need to call this in another Test Project Method if required
        /// </summary>
        public static void CreateXmlsinGo(string rootlocation)
        {
            string utilpath = rootlocation;
            string[] collection = Directory.GetFiles(utilpath, "*.xls");
            foreach (var item in collection)
            {
                CreateXML(item);
            }
        }
    }
}