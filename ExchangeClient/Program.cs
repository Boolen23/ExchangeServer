using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ExchangeClient
{
    class Program
    {
        static void Main(string[] args)
        {
            calc = new ExchangeCalc();
            Receiver = new UdpReceiver(calc);
            Console.WriteLine("To print result press enter, for exit press esc..");
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine(calc);
                }
                else if (keyInfo.Key == ConsoleKey.Escape) break;
            }
        }
        private static ExchangeCalc calc;
        private static UdpReceiver Receiver;
    }
}
