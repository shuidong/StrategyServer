using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace StrategyServer
{
    public partial class App : Application
    {
        private Server server;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            server = new Server();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(1, 1, 0);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            server.Update();
        }
    }
}