using Strafrunden.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows;
using System.Data.SqlClient;

namespace Strafrunden
{
    class StrafrundenPageHandler : IResourceHandler, IDisposable
    {
        private SqlConnection sql;

        public string HandledLocalPath => "/strafrunden/";

        public string GetHandledLocalPath()
        {
            return HandledLocalPath;
        }

        public void HandleContext(HttpListenerContext context)
        {
            int failed = 0;
            int startNr = 0;
            int retID = -1;
            if (context.Request.HttpMethod == "POST")
            {
                string input = "";
                int tmp = 0;
                while(tmp != -1)
                {
                    tmp = context.Request.InputStream.ReadByte();
                    if(tmp != -1)
                    {
                        input += (char)(byte)tmp;
                    }
                }

                if(TryParseArgs(input.Split('&'), ref failed, ref startNr))
                {
                    TrySaveData(failed, startNr, ref retID);
                }
            }
            context.Response.StatusCode = 200;
            string html = FormHTMLResopnse(startNr, failed, retID);
            byte[] buf = ToByteArray(html);

            context.Response.OutputStream.Write(buf, 0, buf.Length);
            context.Response.Close();
        }

        private byte[] ToByteArray(string html)
        {
            byte[] buf = new byte[html.Length];

            for (int i = 0; i < html.Length; i++)
            {
                buf[i] = System.Convert.ToByte(html[i]);
            }
            return buf;
        }

        private bool TrySaveData(int failed, int startNr, ref int retID)
        {
            try
            {
                if (startNr != 0)
                {
                    SqlTransaction trans = sql.BeginTransaction();
                    SqlCommand com = sql.CreateCommand();

                    com.Transaction = trans;

                    com.CommandText = String.Format("INSERT INTO strafrunden (startnummer,fehler) Output Inserted.id VALUES ({0},{1});", startNr, failed);
                    retID = (int)com.ExecuteScalar();

                    trans.Commit();
                }
            }
            catch
            { 
                return false;
            }
            return true;
        }

        private bool TryParseArgs(string[] args, ref int failed, ref int startNr)
        {
            try
            {
                foreach (string str in args)
                {
                    if (str.StartsWith("thrown="))
                    {
                        failed = Convert.ToInt32(str.Substring(7));
                    }
                    if (str.StartsWith("nr="))
                    {
                        startNr = Convert.ToInt32(str.Substring(3));
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                failed = 0;
                startNr = 0;
                return false;
            }
        }

        private string FormHTMLResopnse(int startNr, int failed, int retID)
        {
            string response =
                @"<html><head><title>Strafrundeneingabe</title>
<link rel='stylesheet' href='main.css'></script>
<meta name='viewport' content='width = device-width, initial-scale = 1'/></head>
            <body>
        <div class = 'strafrundenForm'>
            <h2>Strafrunden Speichern:</h2>
            <form action='/strafrunden/' method='post'>
                <label for='nr'>Startnummer:</label>
                <input type='number' name='nr' autocomplete='off'><br/>
                <label for='trown'>Fehlwürfe:</label>
                <input type='number' name='thrown' autocomplete='off' min='0' max='3'><br/>
                <input type='submit' value='Speichern'>
            </form>
        </div>";
            if (startNr != 0)
            {
                response += "<div class='lastSavedDisplay'> <h3> Letzte von Ihnen eingegebene Daten:</h3><p> Startnummer: " + startNr + "</p><p> Fehlwürfe: " + failed + "</p><p> Wurfrunden-ID: " + retID + " (bitte bei Falschen eingaben bereithalten)</p> </div>";
            }
            response += "</body></html>";
            return response;
        }

        public StrafrundenPageHandler()
        {
            sql = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"C:\\Users\\Friedrich May\\Documents\\GitHub\\RaceManagement\\Strafrunden\\Strafrunden.mdf\";Integrated Security=True");
            sql.Open();
        }

        public StrafrundenPageHandler(SqlConnection con)
        {
            sql = con;
            if (sql.State != System.Data.ConnectionState.Open)
                sql.Open();
        }

        public void Dispose()
        {
            sql.Close();
            sql.Dispose();
        }
    }
}
