using System;
using StereoReconstruction.DepthMapFromStereo;
using StereoReconstruction.Common.Helpers;
using StereoReconstruction.RegionGrowing;
using System.Drawing;
using System.Collections.Generic;

namespace StereoReconstruction.TestDepthMap
{
    class Program
    {
        static void Main(string[] args)
        {
            if (ArgumentsProcessing(args))
            {
                Subject subject = SerializerHelper.DeserializeFromXml<Subject>(AppConfig.ConfigPath);
                List<ResultsFromRegionGrowing> depthMapResults =  DepthMap.Create(subject, false);
                DepthSegmentation.AutoRegionGrowing(depthMapResults, 8, true, subject.OutputDataFolder);
                //Console.WriteLine(AppConfig.ConfigPath);
            }
            Console.ReadKey();
        }

        public static void GetRandomNumbersAndColors(int[] array, Color[] colors)
        {
            Random rnd = new Random();
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i + 1;
                colors[i] = Color.FromArgb(rnd.Next(20, 236), rnd.Next(20, 236), rnd.Next(20, 236));
            }
        }

        public static bool ArgumentsProcessing(string[] args)
        {
            if (args.GetLength(0) == 0)
            {
                Console.WriteLine("Нету аргументов командной строки.");
                return false;
            }

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-config":
                        if (i + 1 < args.Length)
                        {
                            if (args[i+1].Contains(".xml"))
                            {
                                AppConfig.ConfigPath = args[i + 1];
                                break;
                            }
                            else
                            {
                                Console.WriteLine("У параметра '-config' неверное имя файла.");
                                return false;
                            }
                        }
                        else
                        {
                            Console.WriteLine("У параметра '-config' нету пути к файлу.");
                            return false;
                        }
                }
            }
            return true;
        }
    }
}
