using System;
using StereoReconstruction.DepthMapFromStereo;
using StereoReconstruction.Common.Helpers;

namespace StereoReconstruction.TestDepthMap
{
    class Program
    {
        static void Main(string[] args)
        {
            if (ArgumentsProcessing(args))
            {
                Subject subject = SerializerHelper.DeserializeFromXml<Subject>(AppConfig.ConfigPath);
                DepthMap.Create(subject);
                //Console.WriteLine(AppConfig.ConfigPath);
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
