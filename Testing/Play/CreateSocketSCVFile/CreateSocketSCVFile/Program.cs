using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateSocketSCVFile
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("creating new cvs file");
            Dictionary<string, string> udcdic = new Dictionary<string, string>();
            udcdic.Add("RSWATER","bbl/D");
            udcdic.Add("RIWATER","bbl/D");
            udcdic.Add("RSOIL","bbl/D");
            udcdic.Add("RIOIL","bbl/D");
            udcdic.Add("RSLIQUID","bbl/D");
            udcdic.Add("RILIQUID","bbl/D");
            udcdic.Add("RSGAS","MCFD");
            udcdic.Add("RIGAS","MCFD");
            udcdic.Add("TEMP","F");
            udcdic.Add("PRESSURE","psi");
            udcdic.Add("CUBPRESS","psi");
            udcdic.Add("CLBPRESS","psi");
            udcdic.Add("CUBRLIQUID","psi");
            udcdic.Add("CLBRLIQUID","psi");
            udcdic.Add("CUBRGAS","psi");
            udcdic.Add("CLBRGAS","psi");
            StringBuilder sb = new StringBuilder();
            string facprefix ="NM1SOCKET";
            string strFilePath = @"C:\temp\Data.csv";
            string strSeperator =",";
            int faccount = 40;
            for (int i = 0; i < faccount; i++)
            {
                string[] rowdata = new string[] { };
                foreach (string skey in udcdic.Keys)
                {
                    rowdata =  new string[] { "CYGNET", "UIS", facprefix + i.ToString(), skey.ToString(), udcdic[skey].ToString(), "1" };
                    foreach (string arrlem in rowdata)
                    {
                        sb.Append(arrlem);
                        sb.Append(strSeperator);
                    }
                    sb.AppendLine();
                }

               
              }
            System.IO.File.AppendAllText(strFilePath, sb.ToString());

        }
    }
}
