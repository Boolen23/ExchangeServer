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
            UdpClient sender = new UdpClient(); // создаем UdpClient для отправки сообщений
            try
            {
                while (true)
                {
                    byte[] data = BitConverter.GetBytes(r.Next());
                    sender.EnableBroadcast = true;
                    sender.Send(data, data.Length, "255.255.255.255", 2222); // отправка
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }
    }
}
