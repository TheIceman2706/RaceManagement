using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace Strafrunden.Server
{
    public class HttpServer
    {

        private HttpListener _baseServer;
        private HttpHandler _handler;
        public HttpListener BaseServer { get => _baseServer; }

        public HttpServer()
        {
            _handler = new HttpHandler();
            _baseServer = new System.Net.HttpListener();

            _baseServer.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

            _baseServer.Prefixes.Add("http://+:80/strafrunden/");

            
        }

        public void Start() //TODO: make pulling context into loop (!)
        {
            _baseServer.Start();
            _baseServer.BeginGetContext(_handler.GetContextCallback,_baseServer);
        }

        public void Stop()
        {
            _baseServer.Stop();
        }
    }
}
