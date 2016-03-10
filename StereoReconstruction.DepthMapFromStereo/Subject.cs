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
            public Settings Properties; // Параметры камер

            public StereoPair() { }

            public StereoPair(string nameImage1, string nameImage2, Coordinates cameraCoordinates, Settings properties)
            {
                NameImage1 = nameImage1;
                NameImage2 = nameImage2;
                CameraCoordinates = cameraCoordinates;
                Properties = properties;
            }

            /// <summary>
            /// Координаты камеры в момент съемки
            /// </summary>
            public class Coordinates
            {
                public int X; // Координата X
                public int Y; // Координата Y
                public int Z; // Координата Z

                public Coordinates() { }

                public Coordinates(int x, int y, int z)
                {
                    X = x;
                    Y = y;
                    Z = z;
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
