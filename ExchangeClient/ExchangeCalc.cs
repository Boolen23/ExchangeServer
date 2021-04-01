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
            GoCalcAsync();
        }
        public override string ToString() => $"Total packets: {TotalCount}, Missing: {BadCount}, avg: {Average}, dev: {StDev}, mod: {Mod}, median: {Median(ValuesCount.Keys)}";
        private ConcurrentDictionary<int, uint> ValuesCount;
        private ConcurrentBag<int> RecivedData;

        #region ResultProperties
        private double Average { get; set; } = -1;
        private ulong TotalCount { get; set; } = 0;
        private ulong BadCount { get; set; } = 0;
        public double StDev => Math.Sqrt(_stDevSum / (TotalCount - 1));
        private int Mod => ValuesCount.Count() > 0 ? ValuesCount.Aggregate((x, y) => x.Value > y.Value ? x : y).Key : 0;
        #endregion

        #region Calculations
        private double _stDevMean = 0;
        private double _stDevSum = 0;
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
                            //рассчет среднего
                            double diff = tmp[i] - Average;
                            Average += diff / TotalCount;

                            //составление таблицы вхождений
                            if (ValuesCount.ContainsKey(tmp[i]))
                                ValuesCount.TryUpdate(tmp[i], ValuesCount[tmp[i]]++, ValuesCount[tmp[i]]++);
                            else ValuesCount.TryAdd(tmp[i], 1);

                            //рассчет среднеквадратичного отклонения
                            double delta = tmp[i] - _stDevMean;
                            _stDevMean += delta / TotalCount;
                            _stDevSum += delta * (tmp[i] - _stDevMean);
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            });
        }
        #endregion

        #region Methods
        public void AddBad() => BadCount++;
        public void AddData(int data) => RecivedData.Add(data);
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
        #endregion
    }
}
