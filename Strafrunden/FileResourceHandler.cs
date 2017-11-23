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
                    Handlers.HandleError(Handlers.EnumErrorType.NotFound,context);
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
