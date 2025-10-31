using System;

namespace RFID.Middleware.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("RFID Middleware Client starting...");
            var client = new MiddlewareClient("127.0.0.1", 20013);
            client.RunTests();
            Console.WriteLine("Client finished. Press ENTER to exit.");
            Console.ReadLine();
        }
    }
}
