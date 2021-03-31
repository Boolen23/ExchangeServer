using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeClient
{
    public class UdpReceiver
    {
        private UdpClient receiver;
        public UdpReceiver(ExchangeCalc exc)
        {
            receiver = new UdpClient();
            calc = exc;
            ReceiveAsync();
        }
        private ExchangeCalc calc;
        private async void ReceiveAsync()
        {
            await Task.Run(() =>
            {

                UdpClient receiver = new UdpClient(2222); // UdpClient для получения данных
                receiver.JoinMulticastGroup(IPAddress.Parse("235.5.5.11"), 20);
                IPEndPoint ep = null;
                try
                {
                    long Ticks = DateTime.Now.Ticks;
                    while (true)
                    {
                        if (DateTime.Now.Ticks - Ticks > 1000000 * 2)
                        {
                            calc.AddBad();
                            Ticks = DateTime.Now.Ticks;
                        }
                        calc.AddData(BitConverter.ToInt32(receiver.Receive(ref ep)));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    receiver.Close();
                }
            });
        }
    }
}
