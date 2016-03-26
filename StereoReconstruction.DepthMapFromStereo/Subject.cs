using System.Collections.Generic;

namespace StereoReconstruction.DepthMapFromStereo
{
    /// <summary>
    /// Субъект съемки
    /// </summary>
    public class Subject
    {
        public string SubjectName; // Название субъекта
        public string InputDataFolder; // Путь к папке с входными данными
        public string OutputDataFolder; // Путь к папке с выходными данными
        public List<StereoPair> Pairs; // Список стереопар с параметрами камер, их координатами в момент съемки и размером шаблона

        public Subject() { }

        public Subject(string subjectName, string inputDataFolder, string outputDataFolder, List<StereoPair> pairs)
        {
            SubjectName = subjectName;
            InputDataFolder = inputDataFolder;
            OutputDataFolder = outputDataFolder;
            Pairs = pairs;
        }

        /// <summary>
        /// Пара изображений с параметрами камеры и размером шаблона
        /// </summary>
        public class StereoPair
        {
            public string NameImage1; // Путь к изображени 1
            public string NameImage2; // Путь к изображению 2
            public Coordinates CameraCoordinates; // Координаты камер в момент съемки
            public AngleRotation CameraAngleRotation; // Углы поворота камеры
            public Settings Properties; // Параметры камер

            public StereoPair() { }

            public StereoPair(string nameImage1, string nameImage2, Coordinates cameraCoordinates, AngleRotation cameraAngleRotation, Settings properties)
            {
                NameImage1 = nameImage1;
                NameImage2 = nameImage2;
                CameraCoordinates = cameraCoordinates;
                CameraAngleRotation = cameraAngleRotation;
                Properties = properties;
            }

            /// <summary>
            /// Координаты камеры в момент съемки
            /// </summary>
            public class Coordinates
            {
                public double X; // Координата X
                public double Y; // Координата Y
                public double Z; // Координата Z

                public Coordinates() { }

                public Coordinates(double x, double y, double z)
                {
                    X = x;
                    Y = y;
                    Z = z;
                }
            }

            /// <summary>
            /// Углы поворота камеры относительно оси X, Y, Z
            /// </summary>
            public class AngleRotation
            {
                public int XAngle; // Угол поворота камеры относительно оси X
                public int YAngle; // Угол поворота камеры относительно оси Y
                public int ZAngle; // Угол поворота камеры относительно оси Z

                public AngleRotation() { }

                public AngleRotation(int xAngle, int yAngle, int zAngle)
                {
                    XAngle = xAngle;
                    YAngle = yAngle;
                    ZAngle = zAngle;
                }
            }

            /// <summary>
            /// Параметры камеры и размер шаблона дял поиска
            /// </summary>
            public class Settings
            {
                public double FocalLength; // Фокусное расстояние
                public double Distance; // Расстояние между камерами
                public int TemplateSize; // Размер шаблона

                public Settings() { }

                public Settings(double focalLength, double distance, int templateSize)
                {
                    FocalLength = focalLength;
                    Distance = distance;
                    TemplateSize = templateSize;
                }
            }
        }
    }
}
