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
            calc = exc;
            Init();
        }
        private void Init()
        {
            receiver = new UdpClient();
            ReceiveAsync();
        }
        private ExchangeCalc calc;
        private async void ReceiveAsync()
        {
            await Task.Run(async() =>
            {
                Settings s = Settings.Load();
                UdpClient receiver = new UdpClient(2222); 
                receiver.JoinMulticastGroup(IPAddress.Parse(s.BroadcastGroup), 20);
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
                            continue;
                        }
                        calc.AddData(BitConverter.ToInt32(receiver.Receive(ref ep)));
                        await Task.Delay(s.ReciveDelayMs);
                    }
                }
                catch (Exception ex)
                {
                    Init();
                }
            });
        }
    }
}
