using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
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
            Console.WriteLine("Please Enter Source Excel File , DataSetName , TableName and Target XML file name");
            Console.WriteLine("Note that Utility will look for Excel  file Sheet name as 'mapping'");
            Console.WriteLine("First Argument is ExcelFile Path ,Second is DataSetName third is tablename and fourht is  xml path");
            Console.WriteLine("ExampleUsage : ExcelToXmlUtility.exe <excelfilepath> <datasetname> <tablename> <xmloutpupath>");
            if (args.Length != 4)
            {
                Console.WriteLine("Invalid Argments List Please Correct it as per above help message");
            }
            excelfile = args[0];
            strdatasetname = args[1];
            strtablname = args[2];
            xmlfile = args[3];
            generateXmlFilefromDataTable(strdatasetname, strtablname);
            // Test Target Consumer Method
            DataTable dtnew = BuildDataTableFromXml(xmlfile);
            Console.WriteLine("End to End Testing was completed");
        }


        static DataTable exceltoDataTable(string xlsfilepath)
        {
            OdbcConnection conn2 = new OdbcConnection();
            try
            {
                DataTable dt2 = new DataTable();

                conn2.ConnectionString = @"Driver={Microsoft Excel Driver (*.xls)};DriverId=790;ReadOnly=0;Dbq=" + xlsfilepath;
                conn2.Open();
                string strcmdText = "Select * from [mapping$]";
                OdbcCommand cmd = new OdbcCommand(strcmdText);
                cmd.Connection = conn2;
                //OdbcDataReader reder = cmd.ExecuteReader();
                OdbcDataAdapter da = new OdbcDataAdapter(cmd);
                da.Fill(dt2);
                return dt2;
            }
            finally
            {
                conn2.Close();
                conn2.Dispose();
            }
        }

        static void generateXmlFilefromDataTable(string datasetname, string tblname)
        {
            DataTable tbl = exceltoDataTable(excelfile);
            DataSet dataSet = new DataSet(datasetname);
            tbl.TableName = tblname;
            dataSet.Tables.Add(tbl);
            dataSet.WriteXml(xmlfile);
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
    }
}