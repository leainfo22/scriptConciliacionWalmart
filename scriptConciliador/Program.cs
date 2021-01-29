using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace scriptConciliador
{
    class Program
    {
        public static DirectoryInfo pathConciliacion, pathResutlPartial;
        public static List<string> filtro = new List<string> { "CAPTURA_TBK_CREDITO", "EVE_TBK", "EVE_OMS", "CAPTURA_PRESTO", "EVE_PRESTO", "EVE_MIDAS" };
        public static List<string> ordenesRepetidas = new List<string>();
        public static DateTime now, yesterday;
        public static CultureInfo enUS = new CultureInfo("en-US");

        static void Main(string[] arg)
        {
            var os = Environment.OSVersion;
            if (arg.Length > 1)
            {
                if (string.IsNullOrEmpty(arg[0]))
                    now = DateTime.Now;
                else if (!DateTime.TryParseExact(arg[0],"yyyyMMdd", enUS, DateTimeStyles.None, out now))
                    now = DateTime.Now;

                if (string.IsNullOrEmpty(arg[1]))
                    yesterday = DateTime.Now;
                else if (!DateTime.TryParseExact(arg[1],"yyyyMMdd", enUS, DateTimeStyles.None,out yesterday))
                    yesterday = DateTime.Now.AddDays(-1);
            }
            else 
            {
                now = DateTime.Now;
                yesterday = DateTime.Now.AddDays(-1);
            }          

            if (os.ToString().Contains("Windows"))
            {
                pathConciliacion = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\conciliados\\");
                pathResutlPartial = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\resultados\\");
            }
            else
            {
                pathConciliacion = new DirectoryInfo("/home/usrEVEMQ/procesos_diarios_eve/resultados/");
                pathResutlPartial = new DirectoryInfo("/home/usrEVEMQ/procesos_diarios_eve/resultados/");
            }
            try
            {
                FileInfo[] fileEntries = pathConciliacion.GetFiles();      
                fileEntries.OrderBy(x => x.Name);
                //CAPTURA_PRESTO
                //CAPTURA_TBK
                //EVE_MIDAS
                //EVE_OMS
                //EVE_PRESTO
                //EVE_TBK
                string pathResult = pathResutlPartial.ToString() + "conciliacion2_" +now.ToString("yyyyMMdd") + ".txt";

                foreach (FileInfo fileName in fileEntries)
                {
                    if (fileName.ToString().Contains(yesterday.ToString("yyyyMMdd")) && (fileName.ToString().Contains("EVE_PRESTO") || fileName.ToString().Contains("CAPTURA_PRESTO")))
                     {
                         using (StreamReader file = new StreamReader(fileName.FullName))
                         {
                             string ln;
                             using (StreamWriter fileWrite = File.AppendText(pathResult))
                             {
                                 if (fileName.ToString().Contains("CAPTURA_PRESTO"))
                                    fileWrite.Write("CAPTURA_PRESTO" + "\n");                                 

                                 else if (fileName.ToString().Contains("EVE_PRESTO"))
                                    fileWrite.Write("EVE_PRESTO" + "\n");
                            }
                             while ((ln = file.ReadLine()) != null)
                             {

                                 String[] strlist = ln.Split(':');
                                 string strToWrite = "";
                                 for (int i = 1; i < strlist.Length; i++)
                                 {
                                     if (!string.IsNullOrEmpty(strlist[i]))
                                         strToWrite += strlist[i] + " ";
                                 }
                                 using (StreamWriter fileWrite = File.AppendText(pathResult))
                                 {
                                     fileWrite.Write(strToWrite + "\n");
                                 }
                             }
                         }
                     }
                     else if (fileName.ToString().Contains(now.ToString("yyyyMMdd")) && (fileName.ToString().Contains("CAPTURA_TBK_CREDITO") || fileName.ToString().Contains("EVE_TBK") || fileName.ToString().Contains("EVE_OMS") || fileName.ToString().Contains("EVE_MIDAS")))
                     {
                        using (StreamReader file = new StreamReader(fileName.FullName))
                         {
                             string ln;
                             using (StreamWriter fileWrite = File.AppendText(pathResult))
                             {
                                 if (fileName.ToString().Contains("CAPTURA_TBK_CREDITO"))
                                     fileWrite.Write("CAPTURA_TBK_CREDITO" + "\n");
                                 else if (fileName.ToString().Contains("EVE_MIDAS"))
                                    fileWrite.Write("EVE_MIDAS" + "\n");
                                 else if (fileName.ToString().Contains("EVE_OMS"))
                                     fileWrite.Write("EVE_OMS" + "\n");
                                 else if (fileName.ToString().Contains("EVE_TBK"))
                                    fileWrite.Write("EVE_TBK" + "\n");
                            }
                             while ((ln = file.ReadLine()) != null)
                             {
                                 String[] strlist = ln.Split(':');
                                 string strToWrite = "";
                                 for (int i = 1; i < strlist.Length; i++)
                                 {
                                     if (!string.IsNullOrEmpty(strlist[i]))
                                         strToWrite += strlist[i] + " ";
                                 }
                                 using (StreamWriter fileWrite = File.AppendText(pathResult))
                                 {
                                     fileWrite.Write(strToWrite + "\n");
                                 }
                             }
                         }

                     }
                 }
                createFileTmp();
                createFileWithoutDuplicatesEve();
                createFileWithoutDuplicatesPresto();
                createFinalFile();
                if(File.Exists(pathResutlPartial.ToString() + "conciliacion2_" + now.ToString("yyyyMMdd") + ".txt"))
                    File.Delete(pathResutlPartial.ToString() + "conciliacion2_" + now.ToString("yyyyMMdd") + ".txt");
                //File.Move(pathResutlPartial.ToString() + "conciliacion2_filter_" + DateTime.Now.ToString("yyyyMMdd") + ".txt", pathResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
        public static int findLineInConciliado(string line, string path) 
        {
            using (TextReader reader = File.OpenText(path))
            {
                string ln;
                while ((ln = reader.ReadLine()) != null)
                {
                    if (line == ln)
                    {
                        return 1;
                    }
                }
            }
            return 0;
        }
        public static void createFileWithoutDuplicatesPresto ()
        {
            //[^,]*,o[0 - 9] *,[0 - 9]*,sg[0 - 9] *,[0 - 9]*,.*$
            Regex rx = new Regex(@"\b[0-9]*\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex rx2 = new Regex(@"\bo+[0-9]*\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            Match m;
            string pathFilter = pathResutlPartial.ToString() + "conciliados_temporales_presto_" + now.ToString("yyyyMMdd") + ".txt";
            string pathFilterTmp = pathResutlPartial.ToString() + "conciliados_temporales_presto_tmp_" + now.ToString("yyyyMMdd") + ".txt";
            File.Move(pathFilter, pathFilterTmp);
            using (TextReader reader = File.OpenText(pathFilterTmp))
            {
                string ln;
                while ((ln = reader.ReadLine()) != null)
                {
                    String[] strlist = ln.Split(',');
                    foreach (string str in strlist)
                    {
                        if (!str.Contains("expirada") && !str.Contains("Autorizacion")) 
                        {
                            if (rx.IsMatch(str))
                            {
                                m = rx.Match(str);
                                if (m.Index < str.Length)
                                    writeInPresto(str.Substring(m.Index), pathFilterTmp, pathFilter);
                            }
                            else if (rx2.IsMatch(str))
                            {
                                m = rx.Match(str);
                                if (m.Index < str.Length)
                                    writeInPresto(str.Substring(m.Index), pathFilterTmp, pathFilter);
                            }
                        }                        
                    }
                }
            }

        }
        public static void createFileWithoutDuplicatesEve() 
        {
            //[^,]*,o[0 - 9] *,[0 - 9]*,sg[0 - 9] *,[0 - 9]*,.*$
            Regex rx = new Regex(@"\bo+[0-9]*\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Match m;
            string pathFilter = pathResutlPartial.ToString() + "conciliados_temporales_eve_" + now.ToString("yyyyMMdd") + ".txt";
            string pathFilterTmp = pathResutlPartial.ToString() + "conciliados_temporales_eve_tmp_" + now.ToString("yyyyMMdd") + ".txt";
            File.Move(pathFilter, pathFilterTmp);
            using (TextReader reader = File.OpenText(pathFilterTmp)) 
            {
                string ln;
                while ((ln = reader.ReadLine()) != null)
                {
                    String[] strlist = ln.Split(',');
                    foreach (string str in strlist) 
                    {
                        if (rx.IsMatch(str))
                        {
                            m = rx.Match(str);
                            if(m.Index < str.Length)
                                writeInEve(str.Substring(m.Index), pathFilterTmp, pathFilter);
                        }
                    }
                }
            }

        }
        public static void writeInEve(string str, string eveTempRead, string eveTempWrite) 
        {
            string lineToWrite = "";
            int count = 0;
            if (!ordenesRepetidas.Contains(str)) 
            {
                using (TextReader reader = File.OpenText(eveTempRead))
                {
                    string currentLine;
                    while ((currentLine = reader.ReadLine()) != null)
                    {
                        if (currentLine.Contains(str))
                        {
                            count++;
                            if (count > 1) 
                            {
                                ordenesRepetidas.Add(str);
                            }
                            lineToWrite = currentLine;
                        }
                    }
                }
                using (StreamWriter fileWrite = File.AppendText(eveTempWrite))
                {
                    fileWrite.Write(lineToWrite + "\n");
                }
            }
        }
        public static void writeInPresto(string str, string prestoTempRead, string prestoTempWrite)
        {
            string lineToWrite = "";
            int count = 0;
            if (!ordenesRepetidas.Contains(str))
            {
                using (TextReader reader = File.OpenText(prestoTempRead))
                {
                    string currentLine;
                    while ((currentLine = reader.ReadLine()) != null)
                    {
                        if (currentLine.Contains(str))
                        {
                            count++;
                            if (count > 1)
                            {
                                ordenesRepetidas.Add(str);
                            }
                            lineToWrite = currentLine;
                        }
                    }
                }
                using (StreamWriter fileWrite = File.AppendText(prestoTempWrite))
                {
                    fileWrite.Write(lineToWrite + "\n");
                }
            }
        }
        public static void createFileTmp()
        {
            string pathResult = pathResutlPartial.ToString() + "conciliacion2_" + now.ToString("yyyyMMdd") + ".txt";
            string pathFilterEve = pathResutlPartial.ToString() + "conciliados_temporales_eve_" + now.ToString("yyyyMMdd") + ".txt";
            string pathFilterPresto = pathResutlPartial.ToString() + "conciliados_temporales_presto_" + now.ToString("yyyyMMdd") + ".txt";
            using (StreamWriter fileWriteEve = File.AppendText(pathFilterEve))
            using (StreamWriter fileWritePresto = File.AppendText(pathFilterPresto))
                if (File.Exists(pathResult))
                {
                    using (TextReader reader = File.OpenText(pathResult))
                    {
                        string currentLine;
                        string type = "";
                        while ((currentLine = reader.ReadLine()) != null)
                        {

                            if (currentLine == "CAPTURA_TBK_CREDITO" || currentLine == "EVE_TBK" || currentLine == "EVE_OMS" || currentLine == "EVE_MIDAS" || currentLine == "CAPTURA_PRESTO" || currentLine == "EVE_PRESTO")
                            {
                                type = currentLine;
                                continue;
                            }
                            else
                            {
                                if (type == "EVE_TBK" || type == "EVE_OMS" || type == "EVE_MIDAS" || type == "CAPTURA_TBK_CREDITO")
                                {
                                    fileWriteEve.Write(currentLine + "\n");
                                }
                                else if (type == "CAPTURA_PRESTO" || type == "EVE_PRESTO")
                                {
                                    fileWritePresto.Write(currentLine + "\n");
                                }
                            }
                        }
                    }
                }
                else 
                {
                    Console.WriteLine("PROGRAMA FINALIZADO. ");
                    System.Environment.Exit(1);
                }

        }
        public static void createFinalFile()
        {
            string finalFile = pathResutlPartial.ToString() + "conciliado2_" + now.ToString("yyyyMMdd") + ".txt";
            string finalFileYesterday = pathResutlPartial.ToString() + "conciliado2_" + yesterday.ToString("yyyyMMdd") + ".txt";
            string eveFileRead = pathResutlPartial.ToString() + "conciliados_temporales_eve_" + now.ToString("yyyyMMdd") + ".txt";
            string prestoFileRead = pathResutlPartial.ToString() + "conciliados_temporales_presto_" + now.ToString("yyyyMMdd") + ".txt";

            if (File.Exists(finalFileYesterday))
            {
                if (File.Exists(eveFileRead))
                {
                    using (TextReader reader = File.OpenText(eveFileRead))
                    {
                        string ln;
                        while ((ln = reader.ReadLine()) != null)
                        {
                            if (ln != "EVE_TBK" && ln != "EVE_MIDAS" && ln != "EVE_OMS" && ln != "CAPTURA_TBK_CREDITO")
                            {
                                if (findLineInConciliado(ln, finalFileYesterday) == 0)
                                {
                                    using (StreamWriter fileWrite = File.AppendText(finalFile))
                                    {
                                        fileWrite.Write(ln + "\n");
                                    }
                                }
                            }
                        }
                    }
                }
                if (File.Exists(prestoFileRead))
                {
                    using (TextReader reader = File.OpenText(prestoFileRead))
                    {
                        string ln;
                        while ((ln = reader.ReadLine()) != null)
                        {
                            if (ln != "CAPTURA_PRESTO" && ln != "EVE_PRESTO")
                            {
                                if (findLineInConciliado(ln, finalFileYesterday) == 0)
                                {
                                    using (StreamWriter fileWrite = File.AppendText(finalFile))
                                    {
                                        fileWrite.Write(ln + "\n");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (File.Exists(eveFileRead))
                {
                    using (TextReader reader = File.OpenText(eveFileRead))
                    {
                        string ln;
                        while ((ln = reader.ReadLine()) != null)
                        {
                            if (ln != "EVE_TBK" && ln != "EVE_MIDAS" && ln != "EVE_OMS" && ln != "CAPTURA_TBK_CREDITO")
                            {
                                using (StreamWriter fileWrite = File.AppendText(finalFile))
                                {
                                    fileWrite.Write(ln + "\n");
                                    continue;
                                }
                            }
                        }
                    }
                }
                if (File.Exists(prestoFileRead))
                {
                    using (TextReader reader = File.OpenText(prestoFileRead))
                    {
                        string ln;
                        while ((ln = reader.ReadLine()) != null)
                        {
                            if (ln != "CAPTURA_PRESTO" && ln != "EVE_PRESTO")
                            {
                                using (StreamWriter fileWrite = File.AppendText(finalFile))
                                {
                                    fileWrite.Write(ln + "\n");
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
