using System;
using System.ServiceModel;
using Consoluno.Service;

namespace Consoluno.Host
{
    class Program
    {
        static void Main(string[] args)
        {
             using (var host = new ServiceHost(typeof(UnoService), new Uri(Properties.Settings.Default.baseAddress)))
            {
                host.Open();
                Console.WriteLine("Press any key to terminate server");
                Console.ReadLine();
                host.Close();
            }
        }
    }
}
