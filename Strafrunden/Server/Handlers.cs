using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Strafrunden.Server
{
    static class Handlers
    {
        private static Dictionary<string, Handler> handlers = new Dictionary<string, Handler>();

        private static Handler error404handler = new Error404Handler();
        private static Handler error500handler = new Error500Handler();


        public static Handler FindHandler(string uri)
        {
            Handler handler;
            if (!handlers.TryGetValue(uri, out handler))
            {
                return error404handler;
            }
            return handler;
        }

        public static void HandleContext(string uri, HttpListenerContext context)
        {
            var handler = FindHandler(uri);
            handler.Handle(context);
        }


        public static bool RegisterResourceHandler(Handler handler)
        {
            if (handlers.ContainsKey(handler.HandledLocalPath))
                return false;

            handlers.Add(handler.HandledLocalPath, handler);
            return true;
        }

        public static void UnregisterResourceHandler(string localPath)
        {
            handlers.Remove(localPath);
        }

        public static void UnregisterResourceHandler(Handler handler)
        {
            UnregisterResourceHandler(handler.HandledLocalPath);
        }

        public static void HandleError(EnumErrorType type, HttpListenerContext context)
        {
            switch (type)
            {
                case EnumErrorType.NotFound:
                    error404handler.Handle(context);
                    break;
                case EnumErrorType.Internal:
                    error500handler.Handle(context);
                    break;
                default:
                    error500handler.Handle(context);
                    break;
            }
        }

        private class Error404Handler : Handler
        {
            public Error404Handler()
            {
                HandledLocalPath = "404";
                HandleContext += (sender, e) =>
                {
                    string html404Template = "<html><body><div style='width: 500px; height: 300px; margin-top:100px; margin-left:auto; margin-right:auto;'><h1>ERROR 404</h>\n\r<p>The resource you requested is not avaiable.</p></div></body></html>";
                    byte[] err404buf = new byte[html404Template.Length];
                    e.Context.Response.StatusCode = 404;

                    for (int i = 0; i < html404Template.Length; i++)
                    {
                        err404buf[i] = System.Convert.ToByte(html404Template[i]);
                    }
                    e.Context.Response.OutputStream.Write(err404buf, 0, err404buf.Length);
                    e.Context.Response.Close();
                };
            }   
        }

        private class Error500Handler : Handler
        {
            public Error500Handler()
            {
                HandledLocalPath = "500";
                HandleContext += (sender, e) =>
                {
                    string html404Template = "<html><body><div style='width: 500px; height: 300px; margin-top:100px; margin-left:auto; margin-right:auto;'><h1>ERROR 500</h>\n\r<p>Internal server error.</p></div></body></html>";
                    byte[] err404buf = new byte[html404Template.Length];
                    e.Context.Response.StatusCode = 500;

                    for (int i = 0; i < html404Template.Length; i++)
                    {
                        err404buf[i] = System.Convert.ToByte(html404Template[i]);
                    }
                    e.Context.Response.OutputStream.Write(err404buf, 0, err404buf.Length);
                    e.Context.Response.Close();
                };
            }
        }

        internal enum EnumErrorType
        {
            NotFound = 404,
            Internal = 500
        }
    }
}
