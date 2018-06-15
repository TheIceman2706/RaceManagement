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
    class StrafrundenPageHandler : Handler, IDisposable
    {
        private SqlConnection sql;
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
            return String.Format(Properties.templates.Strafrunden, startNr == 0 ? "style='display:none'" : "", startNr, failed, retID,
                Properties.strings.Starter,Properties.strings.SaveFails,Properties.strings.Fails,Properties.strings.Save,Properties.strings.ThrowID,Properties.strings.LastEnteredData,Properties.strings.RememberIfWrong);
        }

        public StrafrundenPageHandler():this(new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"C:\\Users\\Friedrich May\\Documents\\GitHub\\RaceManagement\\Strafrunden\\Strafrunden.mdf\";Integrated Security=True"))
        {
        }

        public StrafrundenPageHandler(SqlConnection con)
        {
            sql = con;
            if (sql.State != System.Data.ConnectionState.Open)
                sql.Open();


            HandleContext += (sender, e) =>
            {
                int failed = 0;
                int startNr = 0;
                int retID = -1;
                if (e.Context.Request.HttpMethod == "POST")
                {
                    string input = "";
                    int tmp = 0;
                    while (tmp != -1)
                    {
                        tmp = e.Context.Request.InputStream.ReadByte();
                        if (tmp != -1)
                        {
                            input += (char)(byte)tmp;
                        }
                    }

                    if (TryParseArgs(input.Split('&'), ref failed, ref startNr))
                    {
                        TrySaveData(failed, startNr, ref retID);
                    }
                }
                e.Context.Response.StatusCode = 200;
                string html = FormHTMLResopnse(startNr, failed, retID);
                byte[] buf = ToByteArray(html);

                e.Context.Response.OutputStream.Write(buf, 0, buf.Length);
                e.Context.Response.Close();
            };
            HandledLocalPath = "/strafrunden/";

    }

    public void Dispose()
        {
            sql.Close();
            sql.Dispose();
        }
    }
}
