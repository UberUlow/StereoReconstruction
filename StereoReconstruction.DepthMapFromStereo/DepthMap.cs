﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using StereoReconstruction.Common.Helpers;
using StereoReconstruction.Common.Logging;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace StereoReconstruction.DepthMapFromStereo
{
    /// <summary>
    /// Класс для вычисления и создания карты глубины. А также для поиска шаблона на втором изображении.
    /// </summary>
    public static class DepthMap
    {
        /// <summary>
        /// Создание карты глубины
        /// </summary>
        /// <param name="subject">Субъект съемки</param>
        public static void Create(Subject subject)
        {
            try
            {
                SubjectResults results = new SubjectResults(); // Иницилизация результатов построения карты глубины
                results.SubjectName = subject.SubjectName;
                results.DepthMapResults = new List<SubjectResults.DepthMapResult>(); // Иницилизация списка результатов карты глубины для каждой пары

                int count = 1; // Счетчик для нумерации карт глубины и их изображений
                foreach (var pair in subject.Pairs) // Проход по всем стереопарам
                {
                    Tracer.Info("Инициализация входных данных...");
                    Image<Gray, byte> image1 = new Image<Gray, byte>($@"{subject.InputDataFolder}\{pair.NameImage1}").SmoothMedian(11); // Считывание 1-го изображения
                    Image<Gray, byte> image2 = new Image<Gray, byte>($@"{subject.InputDataFolder}\{pair.NameImage2}").SmoothMedian(11); // Считывание 2-го изображения

                    double[,] depthMap = new double[image1.Width, image1.Height]; // Иницилизация карты глубины
                    Image<Gray, byte> depthMapImg = new Image<Gray, byte>(image1.Width, image1.Height); // Иницилизация изображение карты глубины

                    Tracer.Info($"Построение карты глубины для {count}-й пары изображений...");
                    Stopwatch timer = Stopwatch.StartNew(); // Старт секундомера для диагностики работы алгоритма
                    PassageAcrossImage(image1, image2, pair.Properties.TemplateSize, pair.Properties.FocalLength, pair.Properties.Distance, depthMap, depthMapImg); // Проход по всему изображению

                    string outputDepthMapPath = $@"{subject.OutputDataFolder}\depthMap{count}.txt"; // Путь к карте глубины
                    FileHelper.WriteMatrix(outputDepthMapPath, depthMap);
                    Tracer.Info($"Карта глубины записана в файл {outputDepthMapPath}");

                    string outputDepthMapImagePath = $@"{subject.OutputDataFolder}\_depthMap{count}.jpg"; // Путь к изображению карты глубины
                    depthMapImg.Save(outputDepthMapImagePath); // Запись изображения карты глубины в файл
                    Tracer.Info($"Изображение карты глубины записано в файл {outputDepthMapImagePath}\n");

                    // Добавление всей информации о результатах построения карты глубины в список
                    results.DepthMapResults.Add(new SubjectResults.DepthMapResult(timer.ElapsedMilliseconds, outputDepthMapPath, outputDepthMapImagePath, pair.Properties, pair.CameraCoordinates));
                    count++;
                    timer.Stop(); // Остановка секундомера
                }
                SerializerHelper.SerializeToXml(results, $@"{subject.OutputDataFolder}\results.xml"); // Сериализация (запись) результатов в xml-файл
                Tracer.Info($@"Результаты построения карт глубин для объекта '{subject.SubjectName}' записаны в файл {subject.OutputDataFolder}\results.xml");
            }
            catch (Exception ex)
            {
                Tracer.Error("Ошибка построения карты глубины.", ex);
            }
        }

        /// <summary>
        /// Проход по всему изображению с шагом равным размеру шаблона
        /// </summary>
        /// <param name="image1">Изображение 1</param>
        /// <param name="image2">Изображение 2</param>
        /// <param name="templateSize">Размер шаблона</param>
        /// <param name="focalLength">Фокусное расстояние</param>
        /// <param name="distance">Расстояние между камерами</param>
        /// <param name="depthMap">Карта глубины</param>
        /// <param name="depthMapImg">Изображение карты глубины</param>
        private static void PassageAcrossImage(Image<Gray, byte> image1, Image<Gray, byte> image2, int templateSize, double focalLength, double distance, double[,] depthMap, Image<Gray, byte> depthMapImg)
        {
            for (int i = 0; i < image2.Width; i += templateSize)
            {
                for (int j = 0; j < image2.Height; j += templateSize)
                {
                    if (i + templateSize > image2.Width) // Если шаблон выходит за правую границу изображения, то сместить его влево
                    {
                        i = image2.Width - templateSize;
                    }
                    else if (j + templateSize > image2.Height) // Если шаблон выходит за нижнюю границу изображения, то сместить его вверх
                    {
                        j = image2.Height - templateSize;
                    }

                    Rectangle template1 = new Rectangle(i, j, templateSize, templateSize); // Создание квадрата, который определяет границы искомого шаблона
                    Point? templateLocation = MatchTemplate(image2, image1.Copy(template1));
                    if (templateLocation != null) // Если шаблон найден на втором изображении, то вычисилть глубину
                    {
                        Rectangle template2 = new Rectangle(templateLocation.Value.X, templateLocation.Value.Y, templateSize, templateSize); // Создание квадрата, который определяет границы найденного шаблона
                        CalculateDepth(depthMap, depthMapImg, template1, template2, focalLength, distance); // Вычисление глубины
                    }

                }
            }
        }

        /// <summary>
        /// Определение местоположения шаблона на втором изображении при помощи быстрого преобразования фурье
        /// </summary>
        /// <param name="image">Изображение</param>
        /// <param name="templateImage">Изображение шаблона</param>
        /// <returns>Результат поиска шаблона</returns>
        private static Point? MatchTemplate(Image<Gray, byte> image, Image<Gray, byte> templateImage)
        {
            using (Image<Gray, float> resultMatrix = image.MatchTemplate(templateImage, TemplateMatchingType.CcoeffNormed)) // Определения местоположения шаблона на втором изображении
            {
                Point[] maxLocations, minLocations;
                double[] minValues, maxValues;
                resultMatrix.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                if (maxValues[0] > 0.4)
                {
                    return maxLocations[0];
                }
            }
            return null;
        }

        /// <summary>
        /// Вычисление расстояния до найденного шаблона
        /// </summary>
        /// <param name="depthMap">Значения карты глубины</param>
        /// <param name="depthMapImage">Изображение карты глубины</param>
        /// <param name="template1">Квадрат шаблона 1</param>
        /// <param name="template2">Квадрат шаблона 2</param>
        /// <param name="focalLength">Фокусное расстояние</param>
        /// <param name="distance">Расстояние между камерами</param>
        private static void CalculateDepth(double[,] depthMap, Image<Gray, byte> depthMapImage, Rectangle template1, Rectangle template2, double focalLength, double distance)
        {
            double distanceFromTemplate = Math.Round((focalLength * distance) / (Math.Abs((template1.X - template2.X) / 2)), 2); // Вычисление расстояния до шаблона
            // Проход по всему шаблону и присваивание каждому пикселю с координатами шаблона значение глубины
            for (int i = template1.X; i < template1.X + template1.Width; i++)
            {
                for (int j = template1.Y; j < template1.Y + template1.Height; j++)
                {
                    depthMap[i, j] = distanceFromTemplate;
                    depthMapImage[j, i] = new Gray(depthMap[i, j]);
                }
            }
        }
    }
}
