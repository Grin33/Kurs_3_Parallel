using BackPack_Parallel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackPack_Parallel
{
    public class Backpack
    {
        static object locker = new(); //замок
        public List<Loot> Most_valuable { get; private set; } = null;
        private decimal max_Weight { get; set; }
        private decimal best_value { get; set; }
        private decimal final_weight { get; set; }

        public decimal Best_value { get { return best_value; } }
        public decimal Final_weight { get { return final_weight; } }
        public Backpack(decimal max_Weight)
        {
            this.max_Weight = max_Weight;
        }
        #region Straight_Methods
        public void all_shuffle(ref List<Loot> loots) //последовательный    
        {
            if (loots.Count > 0) check_shuffle(ref loots);
            for (int i = 0; i < loots.Count; i++)
            {
                var new_loots = new List<Loot>(loots);
                new_loots.RemoveAt(i);
                all_shuffle(ref new_loots);
            }
        }
        private void Is_Requirement(ref List<Loot> loots, ref decimal req_Weight, ref decimal req_Value)
        {
            foreach (Loot i in loots)
            {
                req_Weight += i.Weight;
                req_Value += i.Value;
            }

        }
        private void check_shuffle(ref List<Loot> loots)
        {
            decimal req_Weight = 0;
            decimal req_Value = 0;
            Is_Requirement(ref loots, ref req_Weight, ref req_Value);
            {
                if ((Most_valuable == null) && (req_Weight <= max_Weight))
                {
                    Most_valuable = loots;
                    best_value = req_Value;
                    final_weight = req_Weight;
                }
                else
                {
                    if (req_Weight <= max_Weight && req_Value > best_value)
                    {
                        Most_valuable = loots;
                        best_value = req_Value;
                        final_weight = req_Weight;
                    }
                }
            }
        }
        #endregion
        #region Parallel_Methods
        private void Is_Requirement(ref List<Loot> loots, ref decimal req_Weight, ref decimal req_Value, ref List<Loot> thread_value, ref decimal req_Weight_1, ref decimal req_Value_1)
        {
            foreach (Loot i in loots)
            {
                req_Weight += i.Weight;
                req_Value += i.Value;
            }
            if (thread_value != null)
                foreach (Loot v in thread_value)
                {
                    req_Weight_1 += v.Weight;
                    req_Value_1 += v.Value;
                }
        }
        private void check_shuffle(ref List<Loot> loots, ref List<Loot> thread_value)
        {
            decimal req_Weight = 0;
            decimal req_Value = 0;
            decimal req_Weight_1 = 0;
            decimal req_Value_1 = 0;
            Is_Requirement(ref loots, ref req_Weight, ref req_Value, ref thread_value, ref req_Weight_1, ref req_Value_1);
            {
                if ((thread_value == null) && (req_Weight <= max_Weight))
                {
                    thread_value = loots;
                    req_Value_1 = req_Value;
                    req_Weight_1 = req_Weight;
                }
                else
                {
                    if (req_Weight <= max_Weight && req_Value > req_Value_1)
                    {
                        thread_value = loots;
                        req_Value_1 = req_Value;
                        req_Weight_1 = req_Weight;
                    }
                }
            }
        }

        /// <summary>
        /// Воспроизводит  рекурсивную выборку вещей последовательно
        /// </summary>
        /// <param name="loots">Список в котором нужно найти лучший набор вещей</param>
        public void all_shuffle(ref List<Loot> loots, ref List<Loot> thread_valuable) //последовательный    
        {
            if (loots.Count > 0) check_shuffle(ref loots, ref thread_valuable);
            for (int i = 0; i < loots.Count; i++)
            {
                var new_loots = new List<Loot>(loots);
                new_loots.RemoveAt(i);
                all_shuffle(ref new_loots, ref thread_valuable);
            }
        }
        public void All_shuffle_Parallel_1(List<Loot> loots) //параллельный алгоритм
        {
            if (loots.Count > 0) check_shuffle(ref loots);
            Parallel.For(0, loots.Count, () => Most_valuable, (i, loop, thread_valuable) =>
            {
                thread_valuable = new List<Loot>() { };
                var new_loots = new List<Loot>(loots);
                new_loots.RemoveAt(i);
                all_shuffle(ref new_loots, ref thread_valuable);  //с переходом на all_shuffle(последовательный) работает быстрее и не забивает потоки
                //thread_valuable = new List<Loot> { new Loot("hi",1m,2m) };
                return thread_valuable; //Выполнение цикла
            },
            (x) => //каждый поток сюда закидывает свои лучшие 
            {
                decimal w = 0; decimal v = 0;
                decimal w1 = 0; decimal v1 = 0;
                foreach (var i in x)
                {
                    v += i.Value;
                }
                if (Most_valuable != null)
                    foreach (var j in Most_valuable)
                    {
                        v1 += j.Value;
                    }
                if (v > v1) { Most_valuable = x; }
            }

            );
        }
#endregion


        /// <summary>
        /// Возвращает лучший набор вещей
        /// </summary>
        /// <returns> Возвращает экземпляр Loot </returns>
        public List<Loot> Get_Most_Valuable() => Most_valuable;
    }
}
