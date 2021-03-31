using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeClient
{
    public class ExchangeCalc
    {
        public ExchangeCalc()
        {
            RecivedData = new ConcurrentBag<int>();
            ValuesCount = new ConcurrentDictionary<int, uint>();
            BadCount = 0;
            GoCalcAsync();
        }
        public override string ToString() => $"Total packets: {TotalCount}, Missing: {BadCount}, avg: {Average}, dev: {CalculateStdDev(ValuesCount.Keys)}, mod: {Mod}, median: {Median(ValuesCount.Keys)}";
        private ConcurrentDictionary<int, uint> ValuesCount;
        private ConcurrentBag<int> RecivedData;

        private double Average = -1;
        private ulong TotalCount = 0;
        private ulong BadCount;     
        private int Mod => ValuesCount.Count() > 0 ? ValuesCount.Aggregate((x, y) => x.Value > y.Value ? x : y).Key : 0;

        private async void GoCalcAsync()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (RecivedData.Count < 1) continue;

                        List<int> tmp = new List<int>(RecivedData);
                        RecivedData.Clear();
                        if (Average == -1) Average = tmp.First();
                        for (int i = 0; i < tmp.Count; i++)
                        {
                            TotalCount++;
                            double diff = tmp[i] - Average;
                            Average += diff / TotalCount;

                            if (ValuesCount.ContainsKey(tmp[i]))
                                ValuesCount.TryUpdate(tmp[i], ValuesCount[tmp[i]]++, ValuesCount[tmp[i]]++);
                            else ValuesCount.TryAdd(tmp[i], 1);
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
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
        private double Median(IEnumerable<int> values)
        {
            if (values is null) return 0;
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
