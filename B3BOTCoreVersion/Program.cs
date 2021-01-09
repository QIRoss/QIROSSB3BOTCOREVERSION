using System;
using Microsoft.Extensions.Configuration;
using System.Timers;
using System.IO;

namespace B3BOTCoreVersion
{
    class Program
    {
        private static Timer _aTimer;
        public static IConfigurationRoot Configuration { get; set; }
        public static int Main(string[] arg)
        {
            if (arg.Length != 3)
            {
                Console.WriteLine(
                    "Use: ./Program <Active stock to monitor> <Price to sell (xx.xx)> <Price to buy (xx.xx)>"
                );
                return 1;
            }
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
            
            var user = Configuration["username"];
            var password = Configuration["password"];
            var apiUrl = Configuration["apiurl"];
            var host = Configuration["host"];
            var from = Configuration["from"];
            var to = Configuration["recipients"];
            var subject = Configuration["subject"];
            var defaultBody = Configuration["defaultBody"];
            var onBuying = Configuration["bodyOnBuying"];
            var onSelling = Configuration["bodyOnSelling"];

            var connection = new Api(arg[0], float.Parse(arg[1]), float.Parse(arg[2]), apiUrl);
            var mail = new Mail(host, user, password, from, to, subject, defaultBody + '\n' + '\n');
            
            void OnTimedEvent(Object source, ElapsedEventArgs e)
            {
                Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
                DateTime date = new DateTime();
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday
                                                         && date.Hour > 10 && date.Hour < 17)
                {
                    int result = connection.GetPrice();
                    if (result != 0)
                    {
                        if (result < 0)
                            mail.body += onBuying + arg[0];
                        else
                            mail.body += onSelling + arg[0];
                        mail.SendMail();
                    }
                }
            }

            _aTimer = new Timer {Interval = 30000};
            _aTimer.Elapsed += OnTimedEvent;
            _aTimer.AutoReset = true;
            _aTimer.Enabled = true;
            Console.ReadLine();
            return 0;
        }
    }
}