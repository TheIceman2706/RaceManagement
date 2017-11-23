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
        private static Dictionary<string, IResourceHandler> handlers = new Dictionary<string, IResourceHandler>();

        private static IResourceHandler error404handler = new Error404Handler();
        private static IResourceHandler error500handler = new Error500Handler();


        public static IResourceHandler FindHandler(string uri)
        {
            IResourceHandler handler;
            if (!handlers.TryGetValue(uri, out handler))
            {
                return error404handler;
            }
            return handler;
        }

        public static void HandleContext(string uri, HttpListenerContext context)
        {
            var handler = FindHandler(uri);
            handler.HandleContext(context);
        }


        public static bool RegisterResourceHandler(IResourceHandler handler)
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

        public static void UnregisterResourceHandler(IResourceHandler handler)
        {
            UnregisterResourceHandler(handler.HandledLocalPath);
        }

        public static void HandleError(EnumErrorType type, HttpListenerContext context)
        {
            switch (type)
            {
                case EnumErrorType.NotFound:
                    error404handler.HandleContext(context);
                    break;
                case EnumErrorType.Internal:
                    error500handler.HandleContext(context);
                    break;
                default:
                    error500handler.HandleContext(context);
                    break;
            }
        }

        private class Error404Handler : IResourceHandler
        {
            public string HandledLocalPath => "404";

            public string GetHandledLocalPath()
            {
                return "404";
            }

            public void HandleContext(HttpListenerContext context)
            {
                string html404Template = "<html><body><div style='width: 500px; height: 300px; margin-top:100px; margin-left:auto; margin-right:auto;'><h1>ERROR 404</h>\n\r<p>The resource you requested is not avaiable.</p></div></body></html>";
                byte[] err404buf = new byte[html404Template.Length];
                context.Response.StatusCode = 404;

                for (int i = 0; i < html404Template.Length; i++)
                {
                    err404buf[i] = System.Convert.ToByte(html404Template[i]);
                }
                context.Response.OutputStream.Write(err404buf, 0, err404buf.Length);
                context.Response.Close();
            }
        }

        private class Error500Handler : IResourceHandler
        {
            public string HandledLocalPath => "500";

            public string GetHandledLocalPath()
            {
                return HandledLocalPath;
            }

            public void HandleContext(HttpListenerContext context)
            {
                string html404Template = "<html><body><div style='width: 500px; height: 300px; margin-top:100px; margin-left:auto; margin-right:auto;'><h1>ERROR 500</h>\n\r<p>Internal server error.</p></div></body></html>";
                byte[] err404buf = new byte[html404Template.Length];
                context.Response.StatusCode = 500;

                for (int i = 0; i < html404Template.Length; i++)
                {
                    err404buf[i] = System.Convert.ToByte(html404Template[i]);
                }
                context.Response.OutputStream.Write(err404buf, 0, err404buf.Length);
                context.Response.Close();
            }
        }

        internal enum EnumErrorType
        {
            NotFound = 404,
            Internal = 500
        }
    }
}
