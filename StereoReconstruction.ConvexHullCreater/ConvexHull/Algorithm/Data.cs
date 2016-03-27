using System.Collections.Generic;

namespace StereoReconstruction.ConvexHullCreater
{
    /*
     * Эта часть реализации определяет данные, используемые алгоритмом
     */
    internal partial class ConvexHullInternal
    {
        /// <summary>
        /// Соответствует размерности данных
        /// 
        /// Когда "поднятый" корпус вычисляется, измерение автоматически увеличивается на единицу
        /// </summary>
        internal readonly int Dimension;

        /// <summary>
        /// Действительно ли мы на параболоиде?
        /// </summary>
        readonly bool IsLifted;

        /// <summary>
        /// Разъяснение в вычислении конфига выпуклой оболочки
        /// </summary>
        readonly double PlaneDistanceTolerance;

        /*
         * Представление входных вершин
         * 
         * - В алгоритме, вершина представляет его индекс в массиве Вершин
         *   Это делает алгоритм намного быстрее (до 30%), чем при использовании ссылки на объект
         * - Позиции сохраняются в виде единого массива значений. Координаты для вершины с индексом i
         *   сохраняются при индексе <i * Dimension, (i + 1) * Dimension)
         * - Вершинные метки используются алгоритмом, чтобы помочь идентифицировать множество вершин, которое "выше" (или "за пределами")
         *   конкретной грани
         */
        IVertex[] Vertices;
        double[] Positions;
        bool[] VertexMarks;

        /*
         * Грани триангуляции представлены в едином бассейне для объектов, которые повторно используются
         * Это позволяет представлять грани как целые числа, а также значительно ускоряет много вычислений
         * Флаг пораженных граней используется для обозначения затрагиваемых граней
         */
        internal ConvexFaceInternal[] FacePool;
        internal bool[] AffectedFaceFlags;

        /// <summary>
        /// Используется для отслеживания размера текущего корпуса в обновлении/отката центра функции
        /// </summary>
        int ConvexHullSize;

        /// <summary>
        /// Список граней, которые не являются частью конечного выпуклой оболочки и по-прежнему должны быть обработаны
        /// </summary>
        FaceList UnprocessedFaces;

        /// <summary>
        /// Список граней, которые образуют выпуклую оболочку
        /// </summary>
        IndexBuffer ConvexFaces;

        /// <summary>
        /// Вершина, которая в данный момент обрабатывается
        /// </summary>
        int CurrentVertex;

        /// <summary>
        /// Переменная помощник для определения самой дальней вершины для конкретной выпуклой поверхности
        /// </summary>
        double MaxDistance;

        /// <summary>
        /// Переменная помощник, для определения индекса вершины, которая удалена с грани, которая в данный момент обрабатывается
        /// </summary>
        int FurthestVertex;

        /// <summary>
        /// Центроид текущего вычисленного корпуса
        /// </summary>
        double[] Center;

        /*
         * Вспомогательный массив для хранения грани для обновления смежности
         * Это нужно, для предотвращения ненужного распределения
         */
        int[] UpdateBuffer;
        int[] UpdateIndices;

        /// <summary>
        /// Используется для определения того, какие грани должны быть обновлены на каждом шаге алгоритма
        /// </summary>
        IndexBuffer TraverseStack;

        /// <summary>
        /// Используется для вершин, которые находятся за предалми грани, которые находятся на выпуклой оболочке
        /// </summary>
        IndexBuffer EmptyBuffer;

        /// <summary>
        /// Используется для определения того, какие вершины "выше" (или "за") гранью
        /// </summary>
        IndexBuffer BeyondBuffer;

        /// <summary>
        /// Хранилище граней, которые видны из текущей вершины
        /// </summary>
        IndexBuffer AffectedFaceBuffer;

        /// <summary>
        /// Хранилище граней, которые образуют "конус", созданный путем добавления новой вершины
        /// </summary>
        SimpleList<DeferredFace> ConeFaceBuffer;

        /// <summary>
        /// Сохраняет список "особых" (или "сгенерированных", "плоских", и т.д.) вершин, которые не могут являться частью корпуса
        /// </summary>
        HashSet<int> SingularVertices;

        /// <summary>
        /// Таблица коннектор помогает определить смежности выпуклых граней
        /// Хэш используется вместо парного сравнения. Это значительно ускоряет вычисления, особенно для более высоких измерений
        /// </summary>
        ConnectorList[] ConnectorTable;
        const int ConnectorTableSize = 2017;

        /// <summary>
        /// Менеджер управления и распределения неисползуемых объектов памяти
        /// Сохраняет сборщик мусора много работы
        /// </summary>
        ObjectManager ObjectManager;

        /// <summary>
        /// Вспомогательный класс для обработки математически связанных вещей
        /// </summary>
        MathHelper MathHelper;

        /// <summary>
        /// Инициализировать буферы и списки
        /// </summary>
        void InitializeData(ConvexHullComputationConfig config)
        {
            UnprocessedFaces = new FaceList();
            ConvexFaces = new IndexBuffer();

            FacePool = new ConvexFaceInternal[(Dimension + 1) * 10]; // Должен быть инициализирован перед менеджером объектов
            AffectedFaceFlags = new bool[(Dimension + 1) * 10];
            ObjectManager = new ObjectManager(this);

            Center = new double[Dimension];
            TraverseStack = new IndexBuffer();
            UpdateBuffer = new int[Dimension];
            UpdateIndices = new int[Dimension];
            EmptyBuffer = new IndexBuffer();
            AffectedFaceBuffer = new IndexBuffer();
            ConeFaceBuffer = new SimpleList<DeferredFace>();
            SingularVertices = new HashSet<int>();
            BeyondBuffer = new IndexBuffer();

            ConnectorTable = new ConnectorList[ConnectorTableSize];
            for (int i = 0; i < ConnectorTableSize; i++) ConnectorTable[i] = new ConnectorList();

            VertexMarks = new bool[Vertices.Length];
            InitializePositions(config);

            MathHelper = new MathHelper(Dimension, Positions);
        }

        /// <summary>
        /// Проинициализируем позиции вершин на основе типа перевода от конфигурации
        /// </summary>
        void InitializePositions(ConvexHullComputationConfig config)
        {
            Positions = new double[Vertices.Length * Dimension];
            int index = 0;
            if (IsLifted)
            {
                var origDim = Dimension - 1;
                var tf = config.PointTranslationGenerator;
                switch (config.PointTranslationType)
                {
                    case PointTranslationType.None:
                        foreach (var v in Vertices)
                        {
                            double lifted = 0.0;
                            for (int i = 0; i < origDim; i++)
                            {
                                var t = v.Position[i];
                                Positions[index++] = t;
                                lifted += t * t;
                            }
                            Positions[index++] = lifted;
                        }
                        break;
                    case PointTranslationType.TranslateInternal:
                        foreach (var v in Vertices)
                        {
                            double lifted = 0.0;
                            for (int i = 0; i < origDim; i++)
                            {
                                var t = v.Position[i] + tf();
                                Positions[index++] = t;
                                lifted += t * t;
                            }
                            Positions[index++] = lifted;
                        }
                        break;
                }
            }
            else
            {
                var tf = config.PointTranslationGenerator;
                switch (config.PointTranslationType)
                {
                    case PointTranslationType.None:
                        foreach (var v in Vertices)
                        {
                            for (int i = 0; i < Dimension; i++) Positions[index++] = v.Position[i];
                        }
                        break;
                    case PointTranslationType.TranslateInternal:
                        foreach (var v in Vertices)
                        {
                            for (int i = 0; i < Dimension; i++) Positions[index++] = v.Position[i] + tf();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Получить вершину координат. Используется только в инициализации функций
        /// </summary>
        double GetCoordinate(int v, int i)
        {
            return Positions[v * Dimension + i];
        }
    }
}
