using System;
using StereoReconstruction.DepthMapFromStereo;

using Emgu.CV;
using Emgu.CV.Structure;

namespace StereoReconstruction.TestDepthMap
{
    class Program
    {
        static void Main(string[] args)
        {
            if (ArgumentsProcessing(args))
            {
                //DepthMap.Create(AppConfig.ConfigPath);
                Console.WriteLine(AppConfig.ConfigPath);
            }
            Console.ReadKey();
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
                            AppConfig.ConfigPath = args[i + 1];
                            break;
                        }
                        else
                        {
                            Console.WriteLine("У аргумента '-config' нету пути к файлу.");
                            return false;
                        }
                }
            }
            return true;
        }
    }
}
