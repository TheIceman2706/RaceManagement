using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Threading;
using System.IO;
using System.Data.SqlClient;

namespace Strafrunden.Server
{
    public class TCPServer
    {
        protected TcpListener listener = new TcpListener(IPAddress.Any,6525);
        protected Task serverTask;
        protected List<Task> conections = new List<Task>();

        protected SqlConnection sql;

        protected bool shouldRun = false;
        protected bool isRunning = false;

        public TCPServer(SqlConnection c)
        {
            sql = c;
        }

        protected void HandleMessage(object state)
        {
            string msg = state as string;
            Logging.Log.Instance.Info("[TCP]Message received:" + msg);
            string tc = msg.Split(';')[0];
            int stnr = Resources.TransponderLookup.Find(tc);
            if (stnr == 0)
                return;
            using (SqlCommand com = sql.CreateCommand())
            {

                com.CommandText = String.Format("INSERT INTO registrations (startnummer,timestamp) Output Inserted.id VALUES ({0},'{1}');", stnr, msg.Split(';')[2].Replace("-","."));
                com.ExecuteScalar();
            }
        }

        protected void HandleConnection(TcpClient client)
        {
            if (!client.Connected)
                return;
            var str = new StreamReader(client.GetStream());
            while (client.Connected && shouldRun)
            {
                //format: [tc];[seq];[ddt];[src]\r\n
                string msg = str.ReadLine();
                if(msg != null)
                    ThreadPool.QueueUserWorkItem(HandleMessage, msg);
            }
        }

        protected void Run()
        {
            isRunning = true;

            try
            {
                listener.Start();
            }
            catch(Exception e)
            {
                MessageBox.Show("Can not start TCP listener.");
                Logging.Log.Instance.Error("[TCP/IP]Cant start listener.");
                isRunning = false;
                return;
            }

            while (shouldRun)
            {
                var client = listener.AcceptTcpClient();
                var t = new Task(() => { HandleConnection(client); });
                t.Start();
                conections.Add(t);
            }
            listener.Stop();
            isRunning = false;
        }

        public void Start()
        {
            shouldRun = true;
            if (!isRunning)
            {
                serverTask = new Task(Run);
                serverTask.Start();
            }
        }

        public void Stop()
        {
            shouldRun = false;
        }
    }
}
