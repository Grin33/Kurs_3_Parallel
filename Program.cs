﻿/*
Задача о рюкзаке
Вместимость: 100кг  Даны предметы с разным весом и ценностью (в Файле Loot.txt)
Укомплектовать рюкзак с максимальной ценностью
Использование удельной ценности = использование жадного алгоритма, что запрещено при выполнении
*/


using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Channels;
using BackPack_Parallel;

namespace BackPack_Parallel
{
    class Program
    {
        /// <summary>
        /// Выводит весь список вещей доступных для помещения в рюкзак
        /// </summary>
        /// <param name="loots"> Список вещей для вывода </param>
        static void PrintLoot(List<Loot> loots)
        {
            foreach (Loot oneloot in loots)
            {
                Console.WriteLine(oneloot);
            }
            Console.WriteLine();
        }

        static List<Loot> StandartLoots()
        {
            var loots = new List<Loot>
            {
                new Loot("necklace", 4000m, 40m)
                ,new Loot("ring",2500m,10m)
                ,new Loot("bracelet",2000m,30m)
                ,new Loot("clock",2100m,35m)
                ,new Loot("gold",4000m,45m)
                ,new Loot("silver",3000m,40m)
                ,new Loot("bronze",2500m,40m)
                ,new Loot("earrings",2900m,15m)
                ,new Loot("cufflings",3000m,16m)
                ,new Loot("chain",3000m,15.5m)
                ,new Loot("signet",2700m,11m)
                //,new Loot("pendent",3500m,39m)
            };
            return loots;
        }

        /// <summary>
        /// Чтение и запись из файла Loot.txt пример записанной вещи Necklace/1200/40
        /// </summary>
        /// <returns> Возвращает экезмпляр List с записанными данными </returns>
        static List<Loot> Reader()
        {
            using var file = new StreamReader("Loot.txt", Encoding.UTF8);
            var loots = new List<Loot>();
            string line = "";
            while ((line = file.ReadLine()) != null)
            {
                string[] lines = line.Split('/');
                loots.Add(new Loot(lines[0], decimal.Parse(lines[1], CultureInfo.InvariantCulture), decimal.Parse(lines[2], CultureInfo.InvariantCulture)));
            }
            file.Close();
            return (loots);
        }
        static void AnsPrint(Backpack back, Stopwatch sw)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            if ((back.Get_Most_Valuable()) == null)
            {
                Console.WriteLine("Нет решения");
                Console.WriteLine($"Время выполнения программы {sw.Elapsed}");
            }
            else
            {
                Console.WriteLine("Список лучших вещей");
                PrintLoot((back.Get_Most_Valuable()));
                Console.WriteLine($"Суммарная ценность вещей {back.Best_value}");
                Console.WriteLine($"Суммарный вес вещей {back.Final_weight}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Время выполнения программы {sw.Elapsed}");
            }
        }
        static void Main()
        {
            var cap = new BackpackCap("Default"); //Максимальная вместимость рюкзака (Можно вписать число)

            //var loots = Reader(); //Чтение из файла
            var loots = StandartLoots();
            PrintLoot(loots);

            var sw = Stopwatch.StartNew(); //Запуск таймера
            sw.Start();
            var back = new Backpack(cap.Capacity);
            back.All_shuffle_Parallel_1(loots);
            back.Get_Most_Valuable();
            AnsPrint(back, sw);
            var s1 = sw.Elapsed;
            sw.Stop();

            sw.Restart();
            var back1 = new Backpack(cap.Capacity);
            back1.all_shuffle(ref loots);
            AnsPrint(back1, sw);
            var s2 = sw.Elapsed;
            sw.Stop();

            Console.WriteLine(); Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Parallel Time: {s1}");
            Console.WriteLine($"Straight Time: {s2}");

            Console.ReadKey();
        }
    }
}
