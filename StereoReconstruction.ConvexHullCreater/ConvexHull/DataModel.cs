namespace StereoReconstruction.ConvexHullCreater
{
    /// <summary>
    /// Для отложенной грани
    /// </summary>
    sealed class DeferredFace
    {
        /// <summary>
        /// Грань
        /// </summary>
        public ConvexFaceInternal Face, Pivot, OldFace;

        /// <summary>
        /// Индексы
        /// </summary>
        public int FaceIndex, PivotIndex;
    }

    /// <summary>
    /// Вспомогательный класс, используемый для соединения граней
    /// </summary>
    sealed class FaceConnector
    {
        /// <summary>
        /// Грань
        /// </summary>
        public ConvexFaceInternal Face;

        /// <summary>
        /// Кромка для подключения
        /// </summary>
        public int EdgeIndex;

        /// <summary>
        /// Индексы вершин
        /// </summary>
        public int[] Vertices;

        /// <summary>
        /// Хэш-код вычисляется из индексов
        /// </summary>
        public uint HashCode;

        /// <summary>
        /// Предыдущий узел в списке
        /// </summary>
        public FaceConnector Previous;

        /// <summary>
        /// Следующий узел в списке
        /// </summary>
        public FaceConnector Next;

        /// <summary>
        /// Ctor
        /// </summary>
        public FaceConnector(int dimension)
        {
            Vertices = new int[dimension - 1];
        }

        /// <summary>
        /// Обновление коннекторов
        /// </summary>
        public void Update(ConvexFaceInternal face, int edgeIndex, int dim)
        {
            Face = face;
            EdgeIndex = edgeIndex;

            uint hashCode = 23;

            unchecked
            {
                var vs = face.Vertices;
                int i, c = 0;
                for (i = 0; i < edgeIndex; i++)
                {
                    Vertices[c++] = vs[i];
                    hashCode += 31 * hashCode + (uint)vs[i];
                }
                for (i = edgeIndex + 1; i < vs.Length; i++)
                {
                    Vertices[c++] = vs[i];
                    hashCode += 31 * hashCode + (uint)vs[i];
                }
            }

            HashCode = hashCode;
        }

        /// <summary>
        /// Могут ли две грани быть связаны между собой
        /// </summary>
        public static bool AreConnectable(FaceConnector a, FaceConnector b, int dim)
        {
            if (a.HashCode != b.HashCode) return false;

            var n = dim - 1;
            var av = a.Vertices;
            var bv = b.Vertices;
            for (int i = 0; i < av.Length; i++)
            {
                if (av[i] != bv[i]) return false;
            }

            return true;
        }

        /// <summary>
        /// Соединение двух граней
        /// </summary>
        public static void Connect(FaceConnector a, FaceConnector b)
        {
            a.Face.AdjacentFaces[a.EdgeIndex] = b.Face.Index;
            b.Face.AdjacentFaces[b.EdgeIndex] = a.Face.Index;
        }
    }

    /// <summary>
    /// Этот внутренний класс управляет выпуклой гранью оболочки
    /// </summary>
    sealed class ConvexFaceInternal
    {
        /// <summary>
        /// Инициализирует новый экземпляр <see cref="ConvexFaceInternal"/> класса.
        /// </summary>
        public ConvexFaceInternal(int dimension, int index, IndexBuffer beyondList)
        {
            Index = index;
            AdjacentFaces = new int[dimension];
            VerticesBeyond = beyondList;
            Normal = new double[dimension];
            Vertices = new int[dimension];
        }

        /// <summary>
        /// Индекс грани внутри "бассейна"
        /// </summary>
        public int Index;

        /// <summary>
        /// Получает или задает прилегающую данные грани
        /// </summary>
        public int[] AdjacentFaces;

        /// <summary>
        /// Получает или задает вершины за его пределами
        /// </summary>
        public IndexBuffer VerticesBeyond;

        /// <summary>
        /// Дальние вершины
        /// </summary>
        public int FurthestVertex;

        /// <summary>
        /// Получает или задает вершины
        /// </summary>
        public int[] Vertices;

        /// <summary>
        /// Возвращает или задает вектор нормали
        /// </summary>
        public double[] Normal;

        /// <summary>
        /// Норамально переворачивается?
        /// </summary>
        public bool IsNormalFlipped;

        /// <summary>
        /// Плоскость грани (константа)
        /// </summary>
        public double Offset;

        /// <summary>
        /// Используется для обхода пострадавших граней и создать представление Делоне.
        /// </summary>
        public int Tag;

        /// <summary>
        /// Предыдущий узел в списке
        /// </summary>
        public ConvexFaceInternal Previous;

        /// <summary>
        /// Следующий узел в списке
        /// </summary>
        public ConvexFaceInternal Next;

        /// <summary>
        /// Есть ли в списке
        /// </summary>
        public bool InList;
    }
}
