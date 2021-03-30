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
            ReceiveData();
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
        private static async void ReceiveData()
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
    public class ExchangeCalc
    {
        public ExchangeCalc()
        {
            RecivedData = new ConcurrentBag<int>();
            BadCount = 0;
            GoCalcAsync();
        }
        public override string ToString() => $"Total packets: {HandledData.Count}, MissingPackets: {BadCount},  avg: {HandledData.avg}, dev: {HandledData.StdDev}, mod: {HandledData.Mod}, median: {HandledData.Median}";

        private ConcurrentBag<int> RecivedData;
        private (int Count, double avg, double StdDev, int Mod, double Median) HandledData;
        private long BadCount;
        private async void GoCalcAsync()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    if (RecivedData.Count == 0) continue;
                    HandledData = (RecivedData.Count, RecivedData.Average(), CalculateStdDev(RecivedData), Mod(RecivedData), Median(RecivedData));
                }
            });
        }
        public void AddBad() => BadCount++;
        public void AddData(int data) => RecivedData.Add(data);
        private double CalculateStdDev(IEnumerable<int> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                double avg = values.Average();
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }
        private int Mod(IEnumerable<int> values)
        {
            return values.GroupBy(x => x).Select(x => new { Value = x.Key, ValueCount = x.Count() }).OrderByDescending(x => x.ValueCount).FirstOrDefault().Value;
        }
        private double Median(IEnumerable<int> values)
        {
            int[] temp = values.ToArray();
            if (temp.Length == 0) return -1;
            Array.Sort(temp);
            int count = temp.Length;
            if (count % 2 == 0)
            {
                int a = temp[count / 2 - 1];
                int b = temp[count / 2];
                return (a + b) / 2;
            }
            else return temp[count / 2];
        }

    }
}
