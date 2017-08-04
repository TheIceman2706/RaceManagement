using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Net = System.Net;
using Http = System.Net.Http;

namespace Strafrunden.Server
{
    class HttpHandler
    {
        Net.HttpListener
        public void Run()
        {
            while (true)
            {
                Net.HttpListenerContext context = listener.EndGetContext(ar);

                if (context.Request.HttpMethod == "GET" && context.Request.Url.LocalPath == "/strafrunden")
                {
                    context.Response.StatusCode = 200;
                    string htmlTemplate = "<html><body>OK</body></html>";
                    byte[] buf = new byte[htmlTemplate.Length];

                    for (int i = 0; i < htmlTemplate.Length; i++)
                    {
                        buf[i] = System.Convert.ToByte(htmlTemplate[i]);
                    }
                    context.Response.OutputStream.Write(buf, 0, buf.Length);
                }
            }
        }
    }
}
