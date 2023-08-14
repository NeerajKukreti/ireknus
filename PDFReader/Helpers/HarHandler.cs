using HarSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PDFReader.Helpers
{
    internal class HarHandler
    {
        public static string GetJsonFromHARFile(string filePath) {
            try
            {
                string fileName = $"{filePath}";

                StreamWriter writer = new StreamWriter(fileName.ToString().Replace(".har", ".txt"));

                var har = HarConvert.DeserializeFromFile(fileName);

                String strFinal = null;
                String strTemp = null;
                String[] strTempArr;

                foreach (var entry in har.Log.Entries)
                {
                    if (entry.Response.Content.MimeType == "application/json")
                    {
                        Console.WriteLine(entry.Response.Content.Text);
                        strTemp = null;
                        strTemp = entry.Response.Content.Text;

                        strTemp = strTemp.Replace("{\"Table\":[", "");

                        string[] stringSeparators = new string[] { "}]," };

                        strTempArr = strTemp.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                        strFinal = strFinal + strTempArr[0] + "},";

                    }

                }

                strFinal = strFinal.Remove(strFinal.Length - 1, 1);

                strFinal = "{\"Table\":[" + strFinal + "]}";

                writer.Flush();

                return strFinal;
            }
            catch (Exception ex) {
                return "";
            }
        }
    }
}