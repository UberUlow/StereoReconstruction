using StereoReconstruction.Common.Helpers;
using StereoReconstruction.Common.Logging;
using StereoReconstruction.DepthMapFromStereo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace StereoReconstruction.RegionGrowing
{
    /// <summary>
    /// Класс сегментации изображения по значению глубины
    /// </summary>
    public static class DepthSegmentation
    {
        /// <summary>
        /// Автоматическое наращивание регионов по значению глубины
        /// </summary>
        /// <param name="depthMapValues">Значения карт глубины и размер шаблона для каждой карты глубины</param>
        /// <param name="depthDeviation">Отклонение глубины</param>
        /// <param name="MinRegionSize">Минимальный размер региона (количество шаблонов ,которые помещаются в регион)</param>
        /// <param name="writeToFile">Флаг записи результатов в файл</param>
        /// <param name="path">Путь (если не задан, то "")</param>
        public static List<int[,]> AutoRegionGrowing(List<ResultsFromRegionGrowing> depthMapValues, double depthDeviation, int minRegionSize, bool writeToFile, string path = "")
        {
            List<int[,]> matrixRegions = new List<int[,]>();
            int count = 1; // Счетчик карт глубины
            foreach (var depthMapValue in depthMapValues)
            {
                Stopwatch timer = Stopwatch.StartNew(); // Старт секундомера для диагностики работы алгоритма
                Tracer.Info($"\nНаращивание регионов для {count}-й карты глубины...");

                List<int> deleteRegions = new List<int>();
                // Иницилизация матрицы и изображения с регионами
                int[,] regionsMatrix = new int[depthMapValue.DepthMapValue.GetLength(0), depthMapValue.DepthMapValue.GetLength(1)];
                Bitmap regionsImage = new Bitmap(depthMapValue.DepthMapValue.GetLength(0), depthMapValue.DepthMapValue.GetLength(1));

                Random rnd = new Random(); // Рандом для задания случайного цвета для каждого региона
                int regionValue = 1; // Первый регион = 1, затем они увеличиваются на 1
                for (int i = 0; i < regionsMatrix.GetLength(0); i += depthMapValue.TemplateSize)
                {
                    for (int j = 0; j < regionsMatrix.GetLength(1); j += depthMapValue.TemplateSize)
                    {
                        if (regionsMatrix[i, j] == 0) // Если это не отмеченный регион
                        {
                            // Создание уникального региона
                            RegionUniqueness regionUniqueness = new RegionUniqueness(regionValue, Color.FromArgb(rnd.Next(20, 236), rnd.Next(20, 236), rnd.Next(20, 236)));
                            // Наращивание региона
                            GrowRegion(new Point(i, j), depthMapValue.DepthMapValue, depthDeviation, (minRegionSize * (depthMapValue.TemplateSize * depthMapValue.TemplateSize)),
                                regionUniqueness, ref regionsMatrix, ref regionsImage, ref deleteRegions);
                            regionValue++; // Следующий регион будет на 1 больше
                        }
                    }
                }
                regionsImage.Save($@"{path}\RG\imageRegion{count + 1}.bmp");
                ClearSmallRegions(ref regionsMatrix, ref regionsImage, depthMapValue.TemplateSize, deleteRegions);
                matrixRegions.Add(regionsMatrix);
                timer.Stop(); // Остановка таймера
                Tracer.Info($"Наращивание регионов для {count}-й карты глубины завершено. Это заняло {timer.ElapsedMilliseconds} мс.");
                if (writeToFile)
                {
                    string outputMatrixRegion = $@"{path}\RG\matrixRegion{count}.txt"; // Путь к матрицы с отмеченными регионами
                    Tracer.Info($"Запись {count}-й матрицы с регионами в файл {outputMatrixRegion}...");
                    FileHelper.WriteMatrix(outputMatrixRegion, regionsMatrix, ' '); // Сохранение матрицы с регионами в файл
                    string outputImageRegion = $@"{path}\RG\imageRegion{count}.bmp"; // Путь к изображению с отмеченными регионами
                    Tracer.Info($"Запись {count}-го изображения с регионами в файл {outputImageRegion}...\n");
                    regionsImage.Save(outputImageRegion); // Сохранение изображения в файл
                }
                count++; // Увеличение счетчика карт глубины
            }
            return matrixRegions;
        }

        /// <summary>
        /// Разрастание области по значению глубины
        /// </summary>
        /// <param name="point">Точка с потенциальным регионом</param>
        /// <param name="depthMapValue">Значение карты глубины</param>
        /// <param name="depthDeviation">Отклонение глубины</param>
        /// <param name="minRegionSize">Минимальный размер региона</param>
        /// <param name="regionUniqueness">Уникальность региона (значение и цвет)</param>
        /// <param name="regionsMatrix">Матрица регионов</param>
        /// <param name="regionsImage">Изображение регионов</param>
        /// <param name="deleteRegions">Регионы, которые нужно будет удалить</param>
        public static void GrowRegion(Point point, double[,] depthMapValue, double depthDeviation, int minRegionSize, RegionUniqueness regionUniqueness, ref int[,] regionsMatrix, ref Bitmap regionsImage, ref List<int> deleteRegions)
        {
            Queue<Point> points = new Queue<Point>(); // Очередь точек (они постояянно добавляются в методе CheckPoint, если это точка региона)
            points.Enqueue(point); // Добавление стратовой точки в очередь
            
            regionsMatrix[point.X, point.Y] = regionUniqueness.RegionValue; // Стартовая точка принадлежит к искомому региону
            regionsImage.SetPixel(point.X, point.Y, regionUniqueness.ColorValue); // Закрашиваем стартовую точку
            double depthValue = depthMapValue[point.X, point.Y]; // Считаем, что значение глубины в точке эталонное
            
            int pixelsInRegion = 0;

            double[] parameters = new double[] // Массив параметров для проверки приндлежности точки к искомому региону
            {
                depthValue - depthDeviation, // Левая граница параметра проверки глубины
                depthValue + depthDeviation, // Правая граница параметра проверки глубины
            };

            while (true) // Цикл закончится тогда, когда закочатся все точки в очереди
            {
                Point tempPoint;
                if (points.Count > 0) // Пока есть точки в очереди
                {
                    tempPoint = points.Dequeue();
                }
                else // Иначе выход из цикла
                {
                    break;
                }
                if (tempPoint.X > 0) // Тогда проверяем точку слева от центра
                    CheckPoint(new Point(tempPoint.X - 1, tempPoint.Y), parameters, points, depthMapValue, regionsMatrix, regionsImage, regionUniqueness, ref pixelsInRegion);
                if (tempPoint.X > 0 && tempPoint.Y > 0) // Тогда проверяем точку слева сверху от центра
                    CheckPoint(new Point(tempPoint.X - 1, tempPoint.Y - 1), parameters, points, depthMapValue, regionsMatrix, regionsImage, regionUniqueness, ref pixelsInRegion);
                if (tempPoint.Y > 0) // Тогда проверяем точку сверху от центра
                    CheckPoint(new Point(tempPoint.X, tempPoint.Y - 1), parameters, points, depthMapValue, regionsMatrix, regionsImage, regionUniqueness, ref pixelsInRegion);
                if (tempPoint.X < depthMapValue.GetLength(0) - 1 && tempPoint.Y > 0) // Тогда проверяем точку справа сверху от центра
                    CheckPoint(new Point(tempPoint.X + 1, tempPoint.Y - 1), parameters, points, depthMapValue, regionsMatrix, regionsImage, regionUniqueness, ref pixelsInRegion);
                if (tempPoint.X < depthMapValue.GetLength(0) - 1) // Тогда проверяем точку справа от центра
                    CheckPoint(new Point(tempPoint.X + 1, tempPoint.Y), parameters, points, depthMapValue, regionsMatrix, regionsImage, regionUniqueness, ref pixelsInRegion);
                if (tempPoint.X < depthMapValue.GetLength(0) - 1 && tempPoint.Y < depthMapValue.GetLength(1) - 1) // Тогда проверяем точку справа снизу от центра
                    CheckPoint(new Point(tempPoint.X + 1, tempPoint.Y + 1), parameters, points, depthMapValue, regionsMatrix, regionsImage, regionUniqueness, ref pixelsInRegion);
                if (tempPoint.Y < depthMapValue.GetLength(1) - 1) // Тогда проверяем точку снизу от центра
                    CheckPoint(new Point(tempPoint.X, tempPoint.Y + 1), parameters, points, depthMapValue, regionsMatrix, regionsImage, regionUniqueness, ref pixelsInRegion);
                if (tempPoint.X > 0 && tempPoint.Y < depthMapValue.GetLength(1) - 1) // тогда проверяем точку слева снизу от центра
                    CheckPoint(new Point(tempPoint.X - 1, tempPoint.Y + 1), parameters, points, depthMapValue, regionsMatrix, regionsImage, regionUniqueness, ref pixelsInRegion);
            }

            if (pixelsInRegion < minRegionSize)
            {
                deleteRegions.Add(regionUniqueness.RegionValue); // Добавление региона 
            }
        }

        /// <summary>
        /// Проверка точки на принадлежность к искомому региону
        /// </summary>
        /// <param name="point">Точка, которую необходимо проверить</param>
        /// <param name="parameters">Параметры для проверки</param>
        /// <param name="points">Очередь точек (если точка удовлетворяет условию, то добавить её в эту очередь)</param>
        /// <param name="depthValues">Значения глубины</param>
        /// <param name="regionsMatrix">Матрица, в которой будет выделяться регион</param>
        /// <param name="regionsImage">Изображение регионов</param>
        /// <param name="regionUniqueness">Уникальность региона (цвет и номер)</param>
        /// <param name="pixelsInRegion">Пикселей в регионе</param>
        private static void CheckPoint(Point point, double[] parameters, Queue<Point> points, double[,] depthValues, int[,] regionsMatrix, Bitmap regionsImage, RegionUniqueness regionUniqueness, ref int pixelsInRegion)
        {
            if (regionsMatrix[point.X, point.Y] != 0) return; // Если точка помечена, то выйти из метода

            if (depthValues[point.X, point.Y] >= parameters[0] && depthValues[point.X, point.Y] <= parameters[1]) // Если глубина в точке удовлетворяют условию, то это искомый регион
            {
                pixelsInRegion += 1; // Счетчик пикселей для этого региона
                regionsMatrix[point.X, point.Y] = regionUniqueness.RegionValue; // Присваиваем точке номер региона
                regionsImage.SetPixel(point.X, point.Y, regionUniqueness.ColorValue); // Рисуем пиксель, принадлежащий этому региону с уникальным цветом
                points.Enqueue(new Point(point.X, point.Y)); // Добавление точки в очередь, чтобы из неё строились новые точки и регионы продалжали расти
            }
            else
            {
                regionsMatrix[point.X, point.Y] = 0; // Иначе это не наш регион
            }
        }

        /// <summary>
        /// Очистка маленьких регионов
        /// </summary>
        /// <param name="regionsMatrix">Матрица регионов</param>
        /// <param name="regionsImage">Изображение регионов</param>
        /// <param name="templateSize">Размер шаблона</param>
        /// <param name="deleteRegions">Регионы, которые необходимо удалить</param>
        private static void ClearSmallRegions(ref int[,] regionsMatrix, ref Bitmap regionsImage, int templateSize, List<int> deleteRegions)
        {
            for (int i = 0; i < regionsMatrix.GetLength(0); i+= templateSize)
            {
                for (int j = 0; j < regionsMatrix.GetLength(1); j+= templateSize)
                {
                    foreach (var regionValue in deleteRegions)
                    {
                        if (regionsMatrix[i,j] == regionValue) // Если это тот регион, который нужно удалить
                        {
                            for (int m = i; m < i + templateSize; m++)
                            {
                                for (int n = j; n < j + templateSize; n++)
                                {
                                    if (m >= regionsMatrix.GetLength(0)) // Если шаблон выходит за правую границу, то сместить его влево
                                    {
                                        break;
                                    }
                                    else if (n >= regionsMatrix.GetLength(1)) // Если шаблон выходит за нижнюю границу, то сместить его вверх
                                    {
                                        break;
                                    }
                                    regionsMatrix[m, n] = 0;
                                    regionsImage.SetPixel(m, n, Color.White);
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}
