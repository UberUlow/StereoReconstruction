using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace StereoReconstruction.RegionGrowing
{
    public static class Segmentation
    {
        /// <summary>
        /// Выращивание региона
        /// </summary>
        /// <param name="point">Точка - центр искомого региона</param>
        /// <param name="image">Изображение</param>
        /// <param name="descriptors">Дескрипторы</param>
        /// <returns>Матрица с найденным регионом</returns>
        public static int[,] GrowRegion(Point point, Bitmap image, Descriptors descriptors)
        {
            int[,] matrixImage = new int[image.Width, image.Height]; // Иницилизация матрица изображения
            Rectangle imageBorders = new Rectangle(0, 0, image.Width, image.Height); // Создание квадрата-границы изображения

            BitmapData data = image.LockBits(imageBorders, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb); // Атрибуты точечного изображения
            IntPtr intPtr = data.Scan0; // Указатель на первую строку развертки в точечном рисунке
            int numBytes = data.Stride * data.Height; // Ширина шага по индексу изображения * на высоту = размер массива
            byte[] rgbValues = new byte[numBytes]; // Массив для хранения цветов изображения
            Marshal.Copy(intPtr, rgbValues, 0, numBytes);
            
            Queue<Point> points = new Queue<Point>(); // Очередь точек (они постояянно добавляются в методе CheckPoint, если это точка региона)
            points.Enqueue(point); // Добавление стратовой точки в очередь
            matrixImage[point.X, point.Y] = 2; // Стартовая точка принадлежит к искомому региону

            double[] parameters = new double[] // Массив параметров для проверки приндлежности точки к искомому региону
            {
                descriptors.ER - descriptors.DR, // Левая граница мат. ожидания красного цвета
                descriptors.ER + descriptors.DR, // Правая граница мат. ожидания красного цвета
                descriptors.EG - descriptors.DG, // Левая граница мат. ожидания зеленого цвета
                descriptors.EG + descriptors.DG, // Правая граница мат. ожидания зеленого цвета
                descriptors.EB - descriptors.DB, // Левая граница мат. ожидания синего цвета
                descriptors.EB + descriptors.DB  // Правая граница мат. ожидания синего цвета
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

                if (point.X > 0) // Тогда проверяем точку слева от центра
                    CheckPoint(new Point(point.X - 1, point.Y), parameters, points, rgbValues, data.Stride, matrixImage);
                if (point.X > 0 && point.Y > 0) // Тогда проверяем точку слева сверху от центра
                    CheckPoint(new Point(point.X - 1, point.Y - 1), parameters, points, rgbValues, data.Stride, matrixImage);
                if (point.Y > 0) // Тогда проверяем точку сверху от центра
                    CheckPoint(new Point(point.X, point.Y - 1), parameters, points, rgbValues, data.Stride, matrixImage);
                if (point.X < image.Width - 1 && point.Y > 0) // Тогда проверяем точку справа сверху от центра
                    CheckPoint(new Point(point.X + 1, point.Y - 1), parameters, points, rgbValues, data.Stride, matrixImage);
                if (point.X < image.Width - 1) // Тогда проверяем точку справа от центра
                    CheckPoint(new Point(point.X + 1, point.Y), parameters, points, rgbValues, data.Stride, matrixImage);
                if (point.X < image.Width - 1 && point.Y < image.Height - 1) // Тогда проверяем точку справа снизу от центра
                    CheckPoint(new Point(point.X + 1, point.Y + 1), parameters, points, rgbValues, data.Stride, matrixImage);
                if (point.Y < image.Height - 1) // Тогда проверяем точку снизу от центра
                    CheckPoint(new Point(point.X, point.Y + 1), parameters, points, rgbValues, data.Stride, matrixImage);
                if (point.X > 0 && point.Y < image.Height - 1) // тогда проверяем точку слева снизу от центра
                    CheckPoint(new Point(point.X - 1, point.Y + 1), parameters, points, rgbValues, data.Stride, matrixImage);
            }
            return matrixImage;
        }

        /// <summary>
        /// Проверка точки на принадлежность к искомому региону
        /// </summary>
        /// <param name="point">Точка, которую необходимо проверить</param>
        /// <param name="parameters">Параметры для проверки</param>
        /// <param name="points">Очередь точек (если точка удовлетворяет условию, то добавить её в эту очередь)</param>
        /// <param name="rgbValues">Значения цвета изображения</param>
        /// <param name="dataStride">Ширина шага по индексу изображения</param>
        /// <param name="matrix">Матрица, в которой будет отмечатся регион</param>
        private static void CheckPoint(Point point, double[] parameters, Queue<Point> points, byte[] rgbValues, int dataStride, int[,] matrix)
        {
            if (matrix[point.X, point.Y] != 0) return; // Если точка помечена, то выйти из метода

            var r = rgbValues[dataStride * point.Y + 3 * point.X + 2]; // Значение красного цвета в точке
            var g = rgbValues[dataStride * point.Y + 3 * point.X + 1]; // Значение зеленого цвета в точке
            var b = rgbValues[dataStride * point.Y + 3 * point.X]; // Значение синего цвета в точке
            if (r >= parameters[0] && r <= parameters[1] && g >= parameters[2] && g <= parameters[3] && b >= parameters[4] && b <= parameters[5]) // Если все цвета удовлетворяют условию, то это искомый регион
            {
                matrix[point.X, point.Y] = 2;
                points.Enqueue(new Point(point.X, point.Y)); // Добавление точки в очередь, чтобы из неё строились новые точки и регионы продалжали расти
            }
            else
            {
                matrix[point.X, point.Y] = 1;
            }
        }
    }
}
