using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Http;

namespace Strafrunden.Server
{
    public class HttpServer
    {

        private HttpListener _baseServer;
        public HttpHandler Handler { get; internal set; }
        private System.Threading.Thread _responderThread;
        public HttpListener BaseServer { get => _baseServer; }

        public bool Running { get; internal set; }

        public HttpServer()
        {
            _baseServer = new System.Net.HttpListener();

            Handler = new HttpHandler(_baseServer, this);
            _responderThread = new Thread(new ThreadStart(Handler.Run));


            _baseServer.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            _baseServer.Prefixes.Add("http://*:80/strafrunden/");
            _baseServer.Prefixes.Add("http://*:8080/strafrunden/");
        }

        public void Start() //TODO: make pulling context into loop (!)
        {
            _baseServer.Start();
            Running = true;
            _responderThread.Start();
        }

        public void Stop()
        {
            _baseServer.Stop();
            Running = false;
            //_responderThread.Abort();
        }
    }
}
