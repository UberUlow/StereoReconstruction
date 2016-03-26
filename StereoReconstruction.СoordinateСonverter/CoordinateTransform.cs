using System;
using System.IO;
using System.Collections.Generic;
using StereoReconstruction.DepthMapFromStereo;
using StereoReconstruction.Common.Logging;
using System.Linq;
using System.Diagnostics;

namespace StereoReconstruction.СoordinateСonverter
{
    public static class CoordinateTransform
    {
        /// <summary>
        /// Перевод двумерной системы координат в трехмерную
        /// </summary>
        /// <param name="subject">Информация о субъекте и камерах</param>
        /// <param name="depthMapResults">Результаты построения карт глубины</param>
        /// <param name="regionsMatrix">Матрицы регионов соответствующие картам глубины</param>
        /// <param name="writeToFile">Флаг записи в файл</param>
        public static List<Point3D> TwoDMatricesInto3DSpace(Subject subject, List<ResultsFromRegionGrowing> depthMapResults, List<int[,]> regionsMatrix, bool writeToFile)
        {
            List<Point3D> points3D = new List<Point3D>(); // Инициализация списка точек в трехмерном пространстве

            Tracer.Info("\nПреобразование точек карт глубины в глобальную (трехмерную) систему координат...");
            Stopwatch timer = Stopwatch.StartNew(); // Старт секундомера для диагностики работы алгоритма
            for (int n = 0; n < depthMapResults.Count; n++)
            {
                // Перевод градусов в радианы
                double xRadian = subject.Pairs[n].CameraAngleRotation.XAngle * Math.PI / 180;
                double yRadian = subject.Pairs[n].CameraAngleRotation.YAngle * Math.PI / 180;
                double zRadian = subject.Pairs[n].CameraAngleRotation.ZAngle * Math.PI / 180;

                double widthStep = (double)depthMapResults[n].DepthMapValue.GetLength(1) / 2;

                Tracer.Info($"\nПриведение точек {n + 1}-й карты глубины в глобальную систему координат...");
                for (int i = 0; i < depthMapResults[n].DepthMapValue.GetLength(0); i++)
                {
                    for (int j = 0; j < depthMapResults[n].DepthMapValue.GetLength(1); j++)
                    {
                        if (regionsMatrix[n][i, j] > 0) // Если эта точка принадлежит региону
                        {
                            // Тогда создать тчоку с координатами x - ширина, y - глубина, z - высота
                            Point3D point3D = new Point3D(i - widthStep, subject.Pairs[0].CameraCoordinates.Y + depthMapResults[n].DepthMapValue[i, j], j);
                            // Привести эту точку в глобальную систему координат и добавить в список точек в трехмерном прострастве
                            points3D.Add(GeometricCoordinateTransformation(point3D, xRadian, yRadian, zRadian));
                        }
                    }
                }
            }
            Tracer.Info("Удаление точек с одинаковыми координатами в глобальной системе координат...");
            List<Point3D> uniquePoints3D = SearchDuplicatePoints(points3D);
            if (writeToFile)
            {
                Tracer.Info("\nЗапись неповторяющихся точек в файл...");
                SaveFromFile(points3D, $@"{subject.OutputDataFolder}\3Dpoints.txt", ' ');
            }
            Tracer.Info($"Преобразование точек карт глубины в глобальную (трехмерную) систему координат завершилось. Это заняло {timer.ElapsedMilliseconds} мс.\n");
            return points3D;
        }

        /// <summary>
        /// Приведение точки (в трехмерном пространстве) к глобальной системе координат
        /// </summary>
        /// <param name="point3D">Точка в трехмерном пространстве</param>
        /// <param name="xRadian">Угол поворота относительно оси X в радианах</param>
        /// <param name="yRadian">Угол поворота относительно оси Y в радианах</param>
        /// <param name="zRadian">Угол поворота относительно оси Z в радианах</param>
        private static Point3D GeometricCoordinateTransformation(Point3D point3D, double xRadian, double yRadian, double zRadian)
        {
            // Поворот координат на заданный угол относительно каждой оси
            // Если угол поворота относительно оси X не равен нулю
            if (xRadian != 0)
            {
                double y = point3D.Y * Math.Cos(xRadian) - point3D.Z * Math.Sin(xRadian); ;
                double z = point3D.Y * Math.Sin(xRadian) + point3D.Z * Math.Cos(xRadian); ;
                point3D.Y = y;
                point3D.Z = z;
            }
            // Если угол поворота относительно оси Y не равен нулю
            if (yRadian != 0)
            {
                double x = point3D.X * Math.Cos(yRadian) + point3D.Z * Math.Sin(yRadian);
                double z = -point3D.X * Math.Sin(yRadian) + point3D.Z * Math.Cos(yRadian);
                point3D.X = x;
                point3D.Z = z;
            }
            // Если угол поворота относительно оси Z не равен нулю
            if (zRadian != 0)
            {
                double x = point3D.X * Math.Cos(zRadian) - point3D.Y * Math.Sin(zRadian);
                double y = point3D.X * Math.Sin(zRadian) + point3D.Y * Math.Cos(zRadian);
                point3D.X = x;
                point3D.Y = y;
            }
            point3D.X = Math.Round(point3D.X, 2);
            point3D.Y = Math.Round(point3D.Y, 2);
            point3D.Z = Math.Round(point3D.Z, 2);

            return point3D;
        }

        /// <summary>
        /// Поиск уникальных точек в трехмерном пространстве
        /// </summary>
        /// <param name="points3D">Список точек в трехмерном пространстве</param>
        /// <returns>Список неповторяющихся точек в трехмерном прострастве</returns>
        private static List<Point3D> SearchDuplicatePoints(List<Point3D> points3D)
        {
            return points3D.Distinct(new Point3DComparer()).ToList();
        }

        /// <summary>
        /// Сохранение неповторяющихся точек в файл
        /// </summary>
        /// <param name="points3D">Список неповторяющихся точек в трехмерном пространстве</param>
        /// <param name="path">Путь к файлу</param>
        /// <param name="separator">Разделитель</param>
        private static void SaveFromFile(List<Point3D> points3D, string path, char separator)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write("X: ");
                foreach (var point3D in points3D)
                {
                    writer.Write(string.Concat(point3D.X, separator));
                }

                writer.Write("Y: ");
                foreach (var point3D in points3D)
                {
                    writer.Write(string.Concat(point3D.Y, separator));
                }

                writer.Write("Z: ");
                foreach (var point3D in points3D)
                {
                    writer.Write(string.Concat(point3D.Z, separator));
                }
            }
        }
    }
}