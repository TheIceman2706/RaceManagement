using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Net = System.Net;
using Http = System.Net.Http;
using System.Windows;

namespace Strafrunden.Server
{
    public class HttpHandler
    {
        Net.HttpListener listener;

        private Dictionary<string, IResourceHandler> handlers;
        private HttpServer serv;

        public HttpHandler(Net.HttpListener l, HttpServer server)
        {
            listener = l;
            serv = server;
            handlers = new Dictionary<string, IResourceHandler>();
        }
        public void Run()
        {
            while (serv.Running)
            {
                try
                {
                    Net.HttpListenerContext context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem(HandleContext, context);
                }
                catch (Exception)
                {

                }
            }
        }

        public void HandleContext(object state)
        {
            if (state == null | !(state is Net.HttpListenerContext))
                return;
            Net.HttpListenerContext context = state as Net.HttpListenerContext;
#if DEBUG
            if (context.User != null)
                MessageBox.Show(context.User.Identity.AuthenticationType,context.User.Identity.Name);
#endif

            IResourceHandler handler;
            try
            {
                handlers.TryGetValue(context.Request.Url.LocalPath, out handler);
                if (handler == null)
                    Respond404(context);
                else
                    handler.HandleContext(context);
            }
            catch(ArgumentNullException e)
            {
                Respond404(context);
            }
        }

        private void Respond404(Net.HttpListenerContext context)
        {
            string html404Template = "<html><body><div style='width: 500px; height: 300px; margin-top:100px; margin-left:auto; margin-right:auto;'><h1>ERROR 404</h>\n\r<p>The resource you requested is not avaiable</p></div></body></html>";
            byte[] err404buf = new byte[html404Template.Length];
            context.Response.StatusCode = 404;

            for (int i = 0; i < html404Template.Length; i++)
            {
                err404buf[i] = System.Convert.ToByte(html404Template[i]);
            }
            context.Response.OutputStream.Write(err404buf, 0, err404buf.Length);
            context.Response.Close();
        }

        public bool RegisterResourceHandler(IResourceHandler handler)
        {
            if (handlers.ContainsKey(handler.HandledLocalPath))
                return false;

            handlers.Add(handler.HandledLocalPath, handler);
            return true;
        }

        public void UnregisterResourceHandler(string localPath)
        {
            handlers.Remove(localPath);
        }

        public void UnregisterResourceHandler(IResourceHandler handler)
        {
            UnregisterResourceHandler(handler.HandledLocalPath);
        }
    }
}
