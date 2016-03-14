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
        /// Вычисление значений дескрипторов
        /// </summary>
        /// <param name="image">Изображение</param>
        /// <param name="percentOffset">Процент отступа от центра (как далеко от центра выбирать новые точки)</param>
        /// <param name="deviationDisp">Отклонение дисперсии</param>
        /// <returns></returns>
        public static Descriptors CalculateDescriptors(Bitmap image, int percentOffset, int deviationDisp)
        {
            int offsetX = image.Width / percentOffset; // Смещение по X в пикселях
            int offsetY = image.Height / percentOffset; // Смещение по Y в пикселях
            int centerX = image.Width / 2; // Центр изображения по X (предполагается, что объект в фокусе)
            int centerY = image.Height / 2; // Центр изображения по Y (предполагается, что объект в фокусе)

            Point[] points = new Point[] // Массив точек: в центре, слева, слева сверху, сверху, справа сверху, справа, снизу справа, снизу, слева снизу от центра соответственно
            {
                new Point(centerX, centerY),
                new Point(centerX - offsetX, centerY - offsetY),
                new Point(centerX + offsetX, centerY + offsetY),
                new Point(centerX - offsetX, centerY + offsetY),
                new Point(centerX + offsetX, centerY - offsetY),
                new Point(centerX + offsetX, centerY),
                new Point(centerX - offsetX, centerY),
                new Point(centerX, centerY + offsetY),
                new Point(centerX, centerY - offsetY)

            };

            double eR = 0, eG = 0, eB = 0, dR = 0, dG = 0, dB = 0; // Иницилизация дескрипторов
            // Вычисление мат. ожидания
            foreach (Point p in points)
            {
                eR += image.GetPixel(p.X, p.Y).R;
                eG += image.GetPixel(p.X, p.Y).G;
                eB += image.GetPixel(p.X, p.Y).B;
            }
            eR = eR / points.Length;
            eG = eG / points.Length;
            eB = eB / points.Length;
            // Вычисление дисперсии
            foreach (Point p in points)
            {
                dR += Math.Abs(image.GetPixel(p.X, p.Y).R - eR);
                dG += Math.Abs(image.GetPixel(p.X, p.Y).G - eG);
                dB += Math.Abs(image.GetPixel(p.X, p.Y).B - eB);
            }
            dR = dR / points.Length + deviationDisp;
            dG = dG / points.Length + deviationDisp;
            dB = dB / points.Length + deviationDisp;

            return new Descriptors(eR, eG, eB, dR, dG, dB);
        }
    }
}
