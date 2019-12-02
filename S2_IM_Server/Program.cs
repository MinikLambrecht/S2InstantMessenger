using System;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;

namespace S2_IM_Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            const string url = "http://localhost:8080/";

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine($"[{DateTime.Now.ToShortDateString()}][{DateTime.Now.ToShortTimeString()}]: Server running at {url}\n");
                Console.ReadLine();
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR("/signalchat", new HubConfiguration());

            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = null;
        }
    }
}
