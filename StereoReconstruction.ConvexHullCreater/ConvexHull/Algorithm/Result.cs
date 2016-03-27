using System.Collections.Generic;

namespace StereoReconstruction.ConvexHullCreater
{
    /// <summary>
    /// Преобразование результатов к конечному виду
    /// </summary>
    internal partial class ConvexHullInternal
    {
        /// <summary>
        /// Класс выпуклой оболочки
        /// </summary>
        internal static ConvexHull<TVertex, TFace> GetConvexHull<TVertex, TFace>(IList<TVertex> data, ConvexHullComputationConfig config)
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex
        {
            config = config ?? new ConvexHullComputationConfig();

            var vertices = new IVertex[data.Count];
            for (int i = 0; i < data.Count; i++) vertices[i] = data[i];
            ConvexHullInternal ch = new ConvexHullInternal(vertices, false, config);
            ch.FindConvexHull();

            var hull = ch.GetHullVertices(data);

            return new ConvexHull<TVertex, TFace> { Points = hull, Faces = ch.GetConvexFaces<TVertex, TFace>() };
        }

        /// <summary>
        /// Получить вершины оболочки
        /// </summary>
        TVertex[] GetHullVertices<TVertex>(IList<TVertex> data)
        {
            int cellCount = ConvexFaces.Count;
            int hullVertexCount = 0;
            int vertexCount = Vertices.Length;

            for (int i = 0; i < vertexCount; i++) VertexMarks[i] = false;

            for (int i = 0; i < cellCount; i++)
            {
                var vs = FacePool[ConvexFaces[i]].Vertices;
                for (int j = 0; j < vs.Length; j++)
                {
                    var v = vs[j];
                    if (!VertexMarks[v])
                    {
                        VertexMarks[v] = true;
                        hullVertexCount++;
                    }
                }
            }

            var result = new TVertex[hullVertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                if (VertexMarks[i]) result[--hullVertexCount] = data[i];
            }

            return result;
        }

        /// <summary>
        /// Поиск выпуклой оболочки и создание объекта Face
        /// </summary>
        TFace[] GetConvexFaces<TVertex, TFace>()
            where TFace : ConvexFace<TVertex, TFace>, new()
            where TVertex : IVertex
        {
            var faces = ConvexFaces;
            int cellCount = faces.Count;
            var cells = new TFace[cellCount];

            for (int i = 0; i < cellCount; i++)
            {
                var face = FacePool[faces[i]];
                var vertices = new TVertex[Dimension];
                for (int j = 0; j < Dimension; j++)
                {
                    vertices[j] = (TVertex)this.Vertices[face.Vertices[j]];
                }

                cells[i] = new TFace
                {
                    Vertices = vertices,
                    Adjacency = new TFace[Dimension],
                    Normal = IsLifted ? null : face.Normal
                };
                face.Tag = i;
            }

            for (int i = 0; i < cellCount; i++)
            {
                var face = FacePool[faces[i]];
                var cell = cells[i];
                for (int j = 0; j < Dimension; j++)
                {
                    if (face.AdjacentFaces[j] < 0) continue;
                    cell.Adjacency[j] = cells[FacePool[face.AdjacentFaces[j]].Tag];
                }

                // Закрепление ориентации вершин
                if (face.IsNormalFlipped)
                {
                    var tempVert = cell.Vertices[0];
                    cell.Vertices[0] = cell.Vertices[Dimension - 1];
                    cell.Vertices[Dimension - 1] = tempVert;

                    var tempAdj = cell.Adjacency[0];
                    cell.Adjacency[0] = cell.Adjacency[Dimension - 1];
                    cell.Adjacency[Dimension - 1] = tempAdj;
                }
            }

            return cells;
        }
    }
}
