namespace StereoReconstruction.ConvexHullCreater
{
    /// <summary>
    /// Вспомогательный класс, в основном, для нормального вычисления.
    /// </summary>
    class MathHelper
    {
        readonly int Dimension;

        double[] PositionData;

        double[] ntX, ntY, ntZ;
        double[] nDNormalHelperVector;
        double[] nDMatrix;
        int[] matrixPivots;

        #region Normals

        /// <summary>
        /// Длина вектора в квадрате
        /// </summary>
        public static double LengthSquared(double[] x)
        {
            double norm = 0;
            for (int i = 0; i < x.Length; i++)
            {
                var t = x[i];
                norm += t * t;
            }
            return norm;
        }

        /// <summary>
        /// Вычитает векторы х и у и сохраняет целевой результат
        /// </summary>
        public void SubtractFast(int x, int y, double[] target)
        {
            int u = x * Dimension, v = y * Dimension;
            for (int i = 0; i < target.Length; i++)
            {
                target[i] = PositionData[u + i] - PositionData[v + i];
            }
        }

        /// <summary>
        /// Находит 3D вектор нормали
        /// </summary>
        void FindNormalVector3D(int[] vertices, double[] normal)
        {
            SubtractFast(vertices[1], vertices[0], ntX);
            SubtractFast(vertices[2], vertices[1], ntY);

            var x = ntX;
            var y = ntY;

            var nx = x[1] * y[2] - x[2] * y[1];
            var ny = x[2] * y[0] - x[0] * y[2];
            var nz = x[0] * y[1] - x[1] * y[0];

            double norm = System.Math.Sqrt(nx * nx + ny * ny + nz * nz);

            double f = 1.0 / norm;
            normal[0] = f * nx;
            normal[1] = f * ny;
            normal[2] = f * nz;
        }

        /// <summary>
        /// Находит 2D вектор нормали
        /// </summary>
        void FindNormalVector2D(int[] vertices, double[] normal)
        {
            SubtractFast(vertices[1], vertices[0], ntX);

            var x = ntX;

            var nx = -x[1];
            var ny = x[0];

            double norm = System.Math.Sqrt(nx * nx + ny * ny);

            double f = 1.0 / norm;
            normal[0] = f * nx;
            normal[1] = f * ny;
        }

        /// <summary>
        /// Находит нормальный вектор гиперплоскости, заданной вершинами
        /// Сохраняет результаты в нормальных данных
        /// </summary>
        public void FindNormalVector(int[] vertices, double[] normalData)
        {
            switch (Dimension)
            {
                case 2: FindNormalVector2D(vertices, normalData); break;
                case 3: FindNormalVector3D(vertices, normalData); break;
            }
        }
        #endregion

        /// <summary>
        /// Рассчитывает нормаль и смещение гипер плоскости, заданной гранями вершин
        /// </summary>
        /// <param name="face"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        public bool CalculateFacePlane(ConvexFaceInternal face, double[] center)
        {
            var vertices = face.Vertices;
            var normal = face.Normal;
            FindNormalVector(vertices, normal);

            if (double.IsNaN(normal[0]))
            {
                return false;
            }

            double offset = 0.0;
            double centerDistance = 0.0;
            var fi = vertices[0] * Dimension;
            for (int i = 0; i < Dimension; i++)
            {
                double n = normal[i];
                offset += n * PositionData[fi + i];
                centerDistance += n * center[i];
            }
            face.Offset = -offset;
            centerDistance -= offset;

            if (centerDistance > 0)
            {
                for (int i = 0; i < Dimension; i++) normal[i] = -normal[i];
                face.Offset = offset;
                face.IsNormalFlipped = true;
            }
            else face.IsNormalFlipped = false;

            return true;
        }

        /// <summary>
        /// Проверка, если вершина является "видимой" с грани
        /// </summary>
        public double GetVertexDistance(int v, ConvexFaceInternal f)
        {
            double[] normal = f.Normal;
            int x = v * Dimension;
            double distance = f.Offset;
            for (int i = 0; i < normal.Length; i++) distance += normal[i] * PositionData[x + i];
            return distance;
        }

        public MathHelper(int dimension, double[] positions)
        {
            PositionData = positions;
            Dimension = dimension;

            ntX = new double[Dimension];
            ntY = new double[Dimension];
            ntZ = new double[Dimension];

            nDNormalHelperVector = new double[Dimension];
            nDMatrix = new double[Dimension * Dimension];
            matrixPivots = new int[Dimension];
        }
    }
}
