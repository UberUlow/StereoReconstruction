using System;
using System.Collections.Generic;
using StereoReconstruction.DepthMapFromStereo;
using StereoReconstruction.Common.Helpers;
using StereoReconstruction.RegionGrowing;
using StereoReconstruction.СoordinateСonverter;
using StereoReconstruction.Triangulation;

namespace StereoReconstruction.TestDepthMap
{
    class Program
    {
        static void Main(string[] args)
        {
            if (ArgumentsProcessing(args))
            {
                Subject subject = SerializerHelper.DeserializeFromXml<Subject>(AppConfig.ConfigPath); // Считывание данных из xml-файла
                /*List<ResultsFromRegionGrowing> depthMapResults =  DepthMap.Create(subject, true); // Построение карт глубины для субъекта
                List<int[,]> regionsMatrix =  DepthSegmentation.AutoRegionGrowing(depthMapResults, 9, 40, true, subject.OutputDataFolder); // Наращивание регионов по значениям карт глубин
                CoordinateTransform.TwoDMatricesInto3DSpace(subject, depthMapResults, regionsMatrix, true);*/
                List<Point3D> points3D = new List<Point3D>();
                
                points3D.Add(new Point3D(0, 0, 0));
                points3D.Add(new Point3D(0, 0, 10));
                points3D.Add(new Point3D(0, 10, 0));
                points3D.Add(new Point3D(0, 10, 10));
                points3D.Add(new Point3D(10, 10, 0));
                points3D.Add(new Point3D(10, 10, 10));
                points3D.Add(new Point3D(10, 0, 0));
                points3D.Add(new Point3D(10, 0, 10));
                points3D.Add(new Point3D(5, 5, 20));

                Builder3DModel.Create3DModel(points3D, subject.SubjectName, subject.OutputDataFolder);
            }
            Console.ReadKey();
        }

        /// <summary>
        /// Проверка аргументов командной строки
        /// </summary>
        /// <param name="args">Аргументы командной строки</param>
        /// <returns>Результат проверки командной строки (true - нет ошибок, - false - есть ошибки)</returns>
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
