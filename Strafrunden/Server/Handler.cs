using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Strafrunden.Server
{
    public abstract class Handler
    {
        public EventHandler<ContextEventArgs> HandleContext;
        public EventHandler<ContextEventArgs> ContextHandled;
        public string HandledLocalPath { get; protected set; }
        public string GetHandledLocalPath() { return HandledLocalPath; }

        public void Handle(HttpListenerContext context)
        {
            if(HandleContext != null)
            {
                HandleContext(this,new ContextEventArgs(context));
            }
            if (ContextHandled != null)
            {
                ContextHandled(this, new ContextEventArgs(context));
            }
        }

    }

    public class ContextEventArgs : EventArgs
    {
        public HttpListenerContext Context { get; private set; }

        public ContextEventArgs(HttpListenerContext context)
        {
            Context = context;
        }

        public ContextEventArgs() : this(null)
        {
        }
    }
}
