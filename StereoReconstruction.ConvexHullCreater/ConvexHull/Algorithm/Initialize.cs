using System;
using System.Collections.Generic;
using System.Linq;

namespace StereoReconstruction.ConvexHullCreater
{
    internal partial class ConvexHullInternal
    {
        /// <summary>
        /// Обертка вершины и определения размера, если он неизвесен
        /// </summary>
        private ConvexHullInternal(IVertex[] vertices, bool lift, ConvexHullComputationConfig config)
        {
            if (config.PointTranslationType != PointTranslationType.None && config.PointTranslationGenerator == null)
            {
                throw new InvalidOperationException("PointTranslationGenerator cannot be null if PointTranslationType is enabled.");
            }

            this.IsLifted = lift;
            this.Vertices = vertices;
            this.PlaneDistanceTolerance = config.PlaneDistanceTolerance;

            Dimension = DetermineDimension();
            if (Dimension < 2) throw new InvalidOperationException("Dimension of the input must be 2 or greater.");

            if (lift) Dimension++;
            InitializeData(config);
        }

        /// <summary>
        /// Проверьте размерность входных данных
        /// </summary>
        int DetermineDimension()
        {
            var r = new Random();
            var vCount = Vertices.Length;
            var dimensions = new List<int>();
            for (var i = 0; i < 10; i++)
                dimensions.Add(Vertices[r.Next(vCount)].Position.Length);
            var dimension = dimensions.Min();
            if (dimension != dimensions.Max()) throw new ArgumentException("Invalid input data (non-uniform dimension).");
            return dimension;
        }

        /// <summary>
        /// Создание первых граней из (размер + 1) вершин
        /// </summary>
        int[] CreateInitialHull(List<int> initialPoints)
        {
            var faces = new int[Dimension + 1];

            for (var i = 0; i < Dimension + 1; i++)
            {
                var vertices = new int[Dimension];
                for (int j = 0, k = 0; j <= Dimension; j++)
                {
                    if (i != j) vertices[k++] = initialPoints[j];
                }
                var newFace = FacePool[ObjectManager.GetFace()];
                newFace.Vertices = vertices;
                Array.Sort(vertices);
                MathHelper.CalculateFacePlane(newFace, Center);
                faces[i] = newFace.Index;
            }

            // Обновить соседство (проверьте все пары граней)
            for (var i = 0; i < Dimension; i++)
            {
                for (var j = i + 1; j < Dimension + 1; j++) UpdateAdjacency(FacePool[faces[i]], FacePool[faces[j]]);
            }

            return faces;
        }


        /// <summary>
        /// Проверить, если 2 грани находятся рядом, и если да, то обновить массива соседних граней
        /// </summary>
        void UpdateAdjacency(ConvexFaceInternal l, ConvexFaceInternal r)
        {
            var lv = l.Vertices;
            var rv = r.Vertices;
            int i;

            // Сброс метки на 1-й грани
            for (i = 0; i < lv.Length; i++) VertexMarks[lv[i]] = false;

            // Пометить все вершины на 2-й грани
            for (i = 0; i < rv.Length; i++) VertexMarks[rv[i]] = true;

            // Найти 1 - ый ложный индекс
            for (i = 0; i < lv.Length; i++) if (!VertexMarks[lv[i]]) break;

            // Никакая вершина не была отмечена
            if (i == Dimension) return;

            // Проверить, если только 1 вершина не была отмечена
            for (int j = i + 1; j < lv.Length; j++) if (!VertexMarks[lv[j]]) return;

            // Если мы здесь, две грани разделяют ребро
            l.AdjacentFaces[i] = r.Index;

            // Найти вершину, которая остается отмеченой
            for (i = 0; i < lv.Length; i++) VertexMarks[lv[i]] = false;
            for (i = 0; i < rv.Length; i++)
            {
                if (VertexMarks[rv[i]]) break;
            }
            r.AdjacentFaces[i] = l.Index;
        }

        /// <summary>
        /// Инициализировать корпус, если Vertices.Length == Dimension.
        /// </summary>
        void InitSingle()
        {
            var vertices = new int[Dimension];
            for (int i = 0; i < Vertices.Length; i++)
            {
                vertices[i] = i;
            }

            var newFace = FacePool[ObjectManager.GetFace()];
            newFace.Vertices = vertices;
            Array.Sort(vertices);
            MathHelper.CalculateFacePlane(newFace, Center);

            // Убедиться, что нормальная точка внизу в случае, если это используется для триангуляции
            if (newFace.Normal[Dimension - 1] >= 0.0)
            {
                for (int i = 0; i < Dimension; i++)
                {
                    newFace.Normal[i] *= -1.0;
                }
                newFace.Offset = -newFace.Offset;
                newFace.IsNormalFlipped = !newFace.IsNormalFlipped;
            }

            ConvexFaces.Add(newFace.Index);
        }

        /// <summary>
        /// Найти (размер + 1) начальных точек и создать симплексы
        /// </summary>
        void InitConvexHull()
        {
            if (Vertices.Length < Dimension)
            {
                // В этом случае не может быть одной выпуклой поверхности, так что мы возвращаем пустой результат            
                return;
            }
            else if (Vertices.Length == Dimension)
            {
                // Все вершины на корпусе и образуют единый симплекс
                InitSingle();
                return;
            }

            var extremes = FindExtremes();
            var initialPoints = FindInitialPoints(extremes);

            // Добавление начальных точек в выпуклую оболочку
            foreach (var vertex in initialPoints)
            {
                CurrentVertex = vertex;
                // Центр обновления должен быть вызван, прежде чем добавить вершину
                UpdateCenter();

                // Отметьте вершину так, чтобы она не была включена за пределы
                VertexMarks[vertex] = true;
            }

            // Создать начальный симплекс
            var faces = CreateInitialHull(initialPoints);

            // Инициализировать вершины за пределами буфера
            foreach (var faceIndex in faces)
            {
                var face = FacePool[faceIndex];
                FindBeyondVertices(face);
                if (face.VerticesBeyond.Count == 0) ConvexFaces.Add(face.Index); // The face is on the hull
                else UnprocessedFaces.Add(face);
            }

            // Снять выделение с вершин
            foreach (var vertex in initialPoints) VertexMarks[vertex] = false;

        }


        /// <summary>
        /// Используется в "инициализации" кода
        /// </summary>
        void FindBeyondVertices(ConvexFaceInternal face)
        {
            var beyondVertices = face.VerticesBeyond;

            MaxDistance = double.NegativeInfinity;
            FurthestVertex = 0;

            int count = Vertices.Length;
            for (int i = 0; i < count; i++)
            {
                if (VertexMarks[i]) continue;
                IsBeyond(face, beyondVertices, i);
            }

            face.FurthestVertex = FurthestVertex;
        }

        /// <summary>
        /// Находит (размер + 1) начальные точки
        /// </summary>
        /// <param name="extremes"></param>
        /// <returns></returns>
        private List<int> FindInitialPoints(List<int> extremes)
        {
            List<int> initialPoints = new List<int>();

            int first = -1, second = -1;
            double maxDist = 0;
            double[] temp = new double[Dimension];
            for (int i = 0; i < extremes.Count - 1; i++)
            {
                var a = extremes[i];
                for (int j = i + 1; j < extremes.Count; j++)
                {
                    var b = extremes[j];
                    MathHelper.SubtractFast(a, b, temp);
                    var dist = MathHelper.LengthSquared(temp);
                    if (dist > maxDist)
                    {
                        first = a;
                        second = b;
                        maxDist = dist;
                    }
                }
            }

            initialPoints.Add(first);
            initialPoints.Add(second);

            for (int i = 2; i <= Dimension; i++)
            {
                double maximum = double.NegativeInfinity;
                int maxPoint = -1;
                for (int j = 0; j < extremes.Count; j++)
                {
                    var extreme = extremes[j];
                    if (initialPoints.Contains(extreme)) continue;

                    var val = GetSquaredDistanceSum(extreme, initialPoints);

                    if (val > maximum)
                    {
                        maximum = val;
                        maxPoint = extreme;
                    }
                }

                if (maxPoint >= 0) initialPoints.Add(maxPoint);
                else
                {
                    int vCount = Vertices.Length;
                    for (int j = 0; j < vCount; j++)
                    {
                        if (initialPoints.Contains(j)) continue;

                        var val = GetSquaredDistanceSum(j, initialPoints);

                        if (val > maximum)
                        {
                            maximum = val;
                            maxPoint = j;
                        }
                    }

                    if (maxPoint >= 0) initialPoints.Add(maxPoint);
                    else ThrowSingular();
                }
            }
            return initialPoints;
        }

        /// <summary>
        /// Вычисляет сумму квадратов расстояний до начальных точек
        /// </summary>
        /// <param name="pivot"></param>
        /// <param name="initialPoints"></param>
        /// <returns></returns>
        double GetSquaredDistanceSum(int pivot, List<int> initialPoints)
        {
            var initPtsNum = initialPoints.Count;
            var sum = 0.0;

            for (int i = 0; i < initPtsNum; i++)
            {
                var initPt = initialPoints[i];
                for (int j = 0; j < Dimension; j++)
                {
                    double t = GetCoordinate(initPt, j) - GetCoordinate(pivot, j);
                    sum += t * t;
                }
            }

            return sum;
        }

        private int LexCompare(int u, int v)
        {
            int uOffset = u * Dimension, vOffset = v * Dimension;
            for (int i = 0; i < Dimension; i++)
            {
                double x = Positions[uOffset + i], y = Positions[vOffset + i];
                int comp = x.CompareTo(y);
                if (comp != 0) return comp;
            }
            return 0;
        }

        /// <summary>
        /// Находит крайности во всех измерениях
        /// </summary>
        /// <returns></returns>
        private List<int> FindExtremes()
        {
            var extremes = new List<int>(2 * Dimension);

            int vCount = Vertices.Length;
            for (int i = 0; i < Dimension; i++)
            {
                double min = double.MaxValue, max = double.MinValue;
                int minInd = 0, maxInd = 0;
                for (int j = 0; j < vCount; j++)
                {
                    var v = GetCoordinate(j, i);
                    var diff = min - v;
                    if (diff >= 0.0)
                    {
                        if (diff < PlaneDistanceTolerance)
                        {
                            if (LexCompare(j, minInd) > 0)
                            {
                                min = v;
                                minInd = j;
                            }
                        }
                        else
                        {
                            min = v;
                            minInd = j;
                        }
                    }

                    diff = v - max;
                    if (diff >= 0.0)
                    {
                        if (diff < PlaneDistanceTolerance)
                        {
                            if (LexCompare(j, maxInd) > 0)
                            {
                                max = v;
                                maxInd = j;
                            }
                        }
                        else
                        {
                            max = v;
                            maxInd = j;
                        }
                    }
                }

                if (minInd != maxInd)
                {
                    extremes.Add(minInd);
                    extremes.Add(maxInd);
                }
                else extremes.Add(minInd);
            }

            // У нас есть достаточно уникальных экстремальныъ точек?
            var set = new HashSet<int>(extremes);
            if (set.Count <= Dimension)
            {
                // Если нет, то просто добавить "первую" не-включенную из них
                int i = 0;
                while (i < vCount && set.Count <= Dimension)
                {
                    set.Add(i);
                    i++;
                }
            }

            return set.ToList();
        }

        /// <summary>
        /// Исключение генерируется, если обнаруживаются особые входные данные.
        /// </summary>
        void ThrowSingular()
        {
            throw new InvalidOperationException(
                    "Сингулярные входные данные (т.е. попытка триангуляции с данными, которые содержат регулярную решетку точек) обнаружены. "
                    + "Добавление некоторого шума к данным может решить эту проблему.");
        }
    }
}
