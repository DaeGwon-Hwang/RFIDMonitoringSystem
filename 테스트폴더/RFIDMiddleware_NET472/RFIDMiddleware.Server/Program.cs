using System;

namespace RFID.Middleware.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("RFID Middleware Server (Net472) starting...");
            var server = new MiddlewareServer(20013);
            server.Start();
            Console.WriteLine("Press ENTER to stop server.");
            Console.ReadLine();
            server.Stop();
        }
    }
}
