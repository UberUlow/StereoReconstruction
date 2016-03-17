using System;
using System.Drawing;

namespace StereoReconstruction.RegionGrowing
{
    /// <summary>
    /// Класс для различных вычислений
    /// </summary>
    public static class Calculation
    {
        /// <summary>
        /// Вычисление цветовых дескрипторов
        /// </summary>
        /// <param name="point">Точка, принадлежащая искомому региону</param>
        /// <param name="image">Изображение</param>
        /// <param name="percentageIdent">Процент отступа от центра (как далеко от центра выбирать новые точки)</param>
        /// <param name="dispersionDeviation">Отклонение дисперсии</param>
        /// <returns>Вычесленные цветовые дескрипторы</returns>
        public static ColorDescriptors CalculateColorDescriptors(Point point, Bitmap image, int percentageIdent, int dispersionDeviation)
        {
            int offsetX = image.Width * (percentageIdent / 100); // Смещение по X в пикселях
            int offsetY = image.Height * (percentageIdent / 100); // Смещение по Y в пикселях

            Point[] points = new Point[] // Массив точек: сама точка, слева, слева сверху, сверху, справа сверху, справа, снизу справа, снизу, слева снизу от неё
            {
                new Point(point.X, point.Y),
                new Point(point.X - offsetX, point.Y - offsetY),
                new Point(point.X + offsetX, point.Y + offsetY),
                new Point(point.X - offsetX, point.Y + offsetY),
                new Point(point.X + offsetX, point.Y - offsetY),
                new Point(point.X + offsetX, point.Y),
                new Point(point.X - offsetX, point.Y),
                new Point(point.X, point.Y + offsetY),
                new Point(point.X, point.Y - offsetY)
            };

            double expectedR = 0, expectedG = 0, expectedB = 0, dispersionR = 0, dispersionG = 0, dispersionB = 0; // Иницилизация дескрипторов
            // Вычисление мат. ожидания
            foreach (Point p in points)
            {
                expectedR += image.GetPixel(p.X, p.Y).R;
                expectedG += image.GetPixel(p.X, p.Y).G;
                expectedB += image.GetPixel(p.X, p.Y).B;
            }
            expectedR = expectedR / points.Length;
            expectedG = expectedG / points.Length;
            expectedB = expectedB / points.Length;
            // Вычисление дисперсии
            foreach (Point p in points)
            {
                dispersionR += Math.Abs(image.GetPixel(p.X, p.Y).R - expectedR);
                dispersionG += Math.Abs(image.GetPixel(p.X, p.Y).G - expectedG);
                dispersionB += Math.Abs(image.GetPixel(p.X, p.Y).B - expectedB);
            }
            dispersionR = dispersionR / points.Length + dispersionDeviation;
            dispersionG = dispersionG / points.Length + dispersionDeviation;
            dispersionB = dispersionB / points.Length + dispersionDeviation;

            return new ColorDescriptors(expectedR, expectedG, expectedB, dispersionR, dispersionG, dispersionB);
        }
    }
}
