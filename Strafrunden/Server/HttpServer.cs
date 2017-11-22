using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Http;
using System.Windows;

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

            _baseServer.Prefixes.Add(Properties.Settings.Default.urlPrefix);
        }

        public void Start() //TODO: make pulling context into loop (!)
        {
            try
            {
                _baseServer.Start();
            }
            catch(HttpListenerException e)
            {
                MessageBox.Show("Server konnte nicht gestertet werden:\n" + e.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown();
            }
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
