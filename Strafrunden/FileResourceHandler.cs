using Strafrunden.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Strafrunden
{
    class FileResourceHandler : IResourceHandler
    {
        private string uriLocalPath;
        private string fileLocalPath;
        private string mime;
        public string HandledLocalPath => uriLocalPath;

        public string GetHandledLocalPath()
        {
            return HandledLocalPath;
        }

        public void HandleContext(HttpListenerContext context)
        {
            if(context.Request.HttpMethod == "GET")
            {
                try
                {
                    byte[] buffer = System.IO.File.ReadAllBytes(fileLocalPath);
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = mime;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    context.Response.Close();
                }
                catch(System.IO.FileNotFoundException e)
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
            }
        }

        public FileResourceHandler(string uriSegment, string localFile, string mimeType)
        {
            uriLocalPath = uriSegment;
            fileLocalPath = localFile;
            mime = mimeType;
        }

        public FileResourceHandler(string uriSegment, string localFile) : this(uriSegment, localFile, "text/plain")
        {

        }
    }
}
