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
            BadCount = 0;
            GoCalcAsync();
        }
        //public override string ToString() => $"Total packets: {HandledData.Count}, MissingPackets: {BadCount},  avg: {HandledData.avg}, dev: {HandledData.StdDev}, mod: {HandledData.Mod}, median: {HandledData.Median}";
        public override string ToString() => $"Total packets: {TotalCount}, Missing:{BadCount}, avg: {Average}, mod: {Mod}, median: {Median(ValuesCount.Select(i=>i.value))}";

        private ConcurrentBag<int> RecivedData;
        private (int Count, double avg, double StdDev, int Mod, double Median) HandledData;
        private double Average = -1;
        private long TotalCount = 0;
        private List<(int value, int Count)> ValuesCount;
        private long BadCount;
        private async void GoCalcAsync()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    if (RecivedData.Count <10 ) continue;
                    CalcAverageData();
                    //HandledData = (RecivedData.Count, RecivedData.Average(), CalculateStdDev(RecivedData), Mod(RecivedData), Median(RecivedData));
                }
            });
        }
        private int Mod => ValuesCount is null ? 0 : ValuesCount.OrderByDescending(x => x.Count).FirstOrDefault().value;
        private void CalcAverageData()
        {
            List<int> tmp = new List<int>(RecivedData);
            RecivedData.Clear();
            if (Average == -1) Average = tmp.First();
            
            for (int i = 0; i < tmp.Count - 1; i++)
            {
                TotalCount++;
                double diff = tmp[i + 1] - Average;
                Average += diff / TotalCount;
            }
            if(ValuesCount is null) 
                ValuesCount = tmp.GroupBy(x => x).Select(x => (value: x.Key, Count: x.Count())).ToList();
            else
            {
                var ConcateGroup = tmp.GroupBy(x => x).Select(x => (value: x.Key, Count: x.Count())).Concat(ValuesCount).ToList();
                ValuesCount = ConcateGroup.GroupBy(x => x.value).Select(x => (value: x.Key, Count: x.Sum(x => x.Count))).ToList();
            }

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
