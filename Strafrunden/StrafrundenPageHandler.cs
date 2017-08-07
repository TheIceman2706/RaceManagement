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

                string[] args = input.Split('&');
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

                    if(startNr != 0)
                    {
                        SqlTransaction trans = sql.BeginTransaction();
                        SqlCommand com = sql.CreateCommand();

                        com.Transaction = trans;

                        com.CommandText = String.Format("INSERT INTO strafrunden (startnummer,fehler) VALUES ({0},{1});",startNr,failed);
                        com.ExecuteNonQuery();

                        trans.Commit();
                    }
                }
                catch (Exception e)
                {
                    failed = 0;
                    startNr = 0;
                }
#if DEBUG
                MessageBox.Show("Startnummer " + startNr + " hat " + failed + " Würfe verworfen.");
#endif
            }
            context.Response.StatusCode = 200;
            string htmlTemplate =
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
                <input type='number' name='thrown' autocomplete='off'><br/>
                <input type='submit' value='Speichern'>
            </form>
        </div>";
            if(startNr != 0)
            {
                htmlTemplate += "<div class='lastSavedDisplay'> <h3> Letzte von Ihnen eingegebene Daten:</h3><p> Startnummer: "+startNr+"</p><p> Fehlwürfe: "+failed+" </div>" ;
            }
    htmlTemplate += "</body></html>";
            byte[] buf = new byte[htmlTemplate.Length];

            for (int i = 0; i < htmlTemplate.Length; i++)
            {
                buf[i] = System.Convert.ToByte(htmlTemplate[i]);
            }
            context.Response.OutputStream.Write(buf, 0, buf.Length);
            context.Response.Close();
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
