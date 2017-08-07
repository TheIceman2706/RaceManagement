using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Net = System.Net;

namespace Strafrunden.Server
{
    public interface IResourceHandler
    {
        string HandledLocalPath { get; }
        string GetHandledLocalPath();

        void HandleContext(Net.HttpListenerContext context);
    }
}
