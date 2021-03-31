using System;
using System.Net.Sockets;

namespace ExchangeServer
{
    class Program
    {
        static void Main(string[] args)
        {
            SendMessage(new Random());
        }
        private static void SendMessage(Random r)
        {
            Settings s = Settings.Load();
            UdpClient sender = new UdpClient(); 
            try
            {
                while (true)
                {
                    byte[] data = BitConverter.GetBytes(r.Next(s.MinValue, s.MaxValue));
                    sender.EnableBroadcast = true;
                    sender.Send(data, data.Length, s.BroadcastGroup, 2222); 
                }
            }
            catch (Exception ex)
            {
                SendMessage(new Random());
            }
        }
    }
}
