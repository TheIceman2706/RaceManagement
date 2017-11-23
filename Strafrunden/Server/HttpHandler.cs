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
        
        private HttpServer serv;

        public HttpHandler(Net.HttpListener l, HttpServer server)
        {
            listener = l;
            serv = server;
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
            
            try
            {
                Handlers.HandleContext(context.Request.Url.LocalPath,context);
            }
            catch(ArgumentNullException e)
            {
                Handlers.HandleError(Handlers.EnumErrorType.Internal,context);
            }
        }

    }
}
