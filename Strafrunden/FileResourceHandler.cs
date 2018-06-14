using Strafrunden.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Strafrunden
{
    class FileResourceHandler : Handler
    {
        private string uriLocalPath;
        private string fileLocalPath;
        private string mime;

        public FileResourceHandler(string uriSegment, string localFile, string mimeType)
        {
            uriLocalPath = uriSegment;
            fileLocalPath = localFile;
            HandledLocalPath = uriLocalPath;
            mime = mimeType;

            HandleContext += (sender, e) =>
            {
                if (e.Context.Request.HttpMethod == "GET")
                {
                    try
                    {
                        byte[] buffer = System.IO.File.ReadAllBytes(fileLocalPath);
                        e.Context.Response.StatusCode = 200;
                        e.Context.Response.ContentType = mime;
                        e.Context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        e.Context.Response.Close();
                    }
                    catch (System.IO.FileNotFoundException ex)
                    {
                        Handlers.HandleError(Handlers.EnumErrorType.NotFound, e.Context);
                    }
                }
            };
        }

        public FileResourceHandler(string uriSegment, string localFile) : this(uriSegment, localFile, "text/plain")
        {

        }
    }
}
