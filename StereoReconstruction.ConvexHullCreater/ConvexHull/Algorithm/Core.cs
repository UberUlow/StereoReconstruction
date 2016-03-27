namespace StereoReconstruction.ConvexHullCreater
{
    internal partial class ConvexHullInternal
    {
        /// <summary>
        /// Пометить все грани при просмотре текущей вершины с 1
        /// </summary>
        void TagAffectedFaces(ConvexFaceInternal currentFace)
        {
            AffectedFaceBuffer.Clear();
            AffectedFaceBuffer.Add(currentFace.Index);
            TraverseAffectedFaces(currentFace.Index);
        }

        /// <summary>
        /// Рекурсивный проход всех соответствующих граней
        /// </summary>
        void TraverseAffectedFaces(int currentFace)
        {
            TraverseStack.Clear();
            TraverseStack.Push(currentFace);
            AffectedFaceFlags[currentFace] = true;

            while (TraverseStack.Count > 0)
            {
                var top = FacePool[TraverseStack.Pop()];
                for (int i = 0; i < Dimension; i++)
                {
                    var adjFace = top.AdjacentFaces[i];

                    if (!AffectedFaceFlags[adjFace] && MathHelper.GetVertexDistance(CurrentVertex, FacePool[adjFace]) >= PlaneDistanceTolerance)
                    {
                        AffectedFaceBuffer.Add(adjFace);
                        AffectedFaceFlags[adjFace] = true;
                        TraverseStack.Push(adjFace);
                    }
                }
            }
        }

        /// <summary>
        /// Создание новой отложенной грани
        /// </summary>
        DeferredFace MakeDeferredFace(ConvexFaceInternal face, int faceIndex, ConvexFaceInternal pivot, int pivotIndex, ConvexFaceInternal oldFace)
        {
            var ret = ObjectManager.GetDeferredFace();

            ret.Face = face;
            ret.FaceIndex = faceIndex;
            ret.Pivot = pivot;
            ret.PivotIndex = pivotIndex;
            ret.OldFace = oldFace;

            return ret;
        }

        /// <summary>
        /// Подключение грание при помощи соеденителя
        /// </summary>
        void ConnectFace(FaceConnector connector)
        {
            var index = connector.HashCode % ConnectorTableSize;
            var list = ConnectorTable[index];

            for (var current = list.First; current != null; current = current.Next)
            {
                if (FaceConnector.AreConnectable(connector, current, Dimension))
                {
                    list.Remove(current);
                    FaceConnector.Connect(current, connector);
                    current.Face = null;
                    connector.Face = null;
                    ObjectManager.DepositConnector(current);
                    ObjectManager.DepositConnector(connector);
                    return;
                }
            }

            list.Add(connector);
        }

        /// <summary>
        /// Удаляет закрытые грание текущей вершины и добовляет к новосозданным
        /// </summary>
        private bool CreateCone()
        {
            var currentVertexIndex = CurrentVertex;
            ConeFaceBuffer.Clear();

            for (int fIndex = 0; fIndex < AffectedFaceBuffer.Count; fIndex++)
            {
                var oldFaceIndex = AffectedFaceBuffer[fIndex];
                var oldFace = FacePool[oldFaceIndex];

                // Найти грани, которые должны быть обновлены
                int updateCount = 0;
                for (int i = 0; i < Dimension; i++)
                {
                    var af = oldFace.AdjacentFaces[i];
                    if (!AffectedFaceFlags[af]) // Tag == false , когда oldFaces не содержит af
                    {
                        UpdateBuffer[updateCount] = af;
                        UpdateIndices[updateCount] = i;
                        ++updateCount;
                    }
                }

                for (int i = 0; i < updateCount; i++)
                {
                    var adjacentFace = FacePool[UpdateBuffer[i]];

                    int oldFaceAdjacentIndex = 0;
                    var adjFaceAdjacency = adjacentFace.AdjacentFaces;
                    for (int j = 0; j < adjFaceAdjacency.Length; j++)
                    {
                        if (oldFaceIndex == adjFaceAdjacency[j])
                        {
                            oldFaceAdjacentIndex = j;
                            break;
                        }
                    }

                    var forbidden = UpdateIndices[i]; // Индекс грани, который соответствует этой смежной грани

                    ConvexFaceInternal newFace;

                    int oldVertexIndex;
                    int[] vertices;

                    var newFaceIndex = ObjectManager.GetFace();
                    newFace = FacePool[newFaceIndex];
                    vertices = newFace.Vertices;
                    for (int j = 0; j < Dimension; j++) vertices[j] = oldFace.Vertices[j];
                    oldVertexIndex = vertices[forbidden];

                    int orderedPivotIndex;

                    // скорректировать порядок
                    if (currentVertexIndex < oldVertexIndex)
                    {
                        orderedPivotIndex = 0;
                        for (int j = forbidden - 1; j >= 0; j--)
                        {
                            if (vertices[j] > currentVertexIndex) vertices[j + 1] = vertices[j];
                            else
                            {
                                orderedPivotIndex = j + 1;
                                break;
                            }
                        }
                    }
                    else
                    {
                        orderedPivotIndex = Dimension - 1;
                        for (int j = forbidden + 1; j < Dimension; j++)
                        {
                            if (vertices[j] < currentVertexIndex) vertices[j - 1] = vertices[j];
                            else
                            {
                                orderedPivotIndex = j - 1;
                                break;
                            }
                        }
                    }

                    vertices[orderedPivotIndex] = CurrentVertex;

                    if (!MathHelper.CalculateFacePlane(newFace, Center))
                    {
                        return false;
                    }

                    ConeFaceBuffer.Add(MakeDeferredFace(newFace, orderedPivotIndex, adjacentFace, oldFaceAdjacentIndex, oldFace));
                }
            }

            return true;
        }

        /// <summary>
        /// Фиксирует конус и добавляет вершину к выпуклой оболочки
        /// </summary>
        void CommitCone()
        {
            // Заполнение смежностей
            for (int i = 0; i < ConeFaceBuffer.Count; i++)
            {
                var face = ConeFaceBuffer[i];

                var newFace = face.Face;
                var adjacentFace = face.Pivot;
                var oldFace = face.OldFace;
                var orderedPivotIndex = face.FaceIndex;

                newFace.AdjacentFaces[orderedPivotIndex] = adjacentFace.Index;
                adjacentFace.AdjacentFaces[face.PivotIndex] = newFace.Index;

                // Пусть здесь будет соединение
                for (int j = 0; j < Dimension; j++)
                {
                    if (j == orderedPivotIndex) continue;
                    var connector = ObjectManager.GetConnector();
                    connector.Update(newFace, j, Dimension);
                    ConnectFace(connector);
                }

                // Идентификатор смежной грани на корпусе? Если да, то мы можем использовать простой метод, чтобы найти вершины за пределами
                if (adjacentFace.VerticesBeyond.Count == 0)
                {
                    FindBeyondVertices(newFace, oldFace.VerticesBeyond);
                }
                // Это более эффективно, если грань с меньшим числом вершин не приходит первой
                else if (adjacentFace.VerticesBeyond.Count < oldFace.VerticesBeyond.Count)
                {
                    FindBeyondVertices(newFace, adjacentFace.VerticesBeyond, oldFace.VerticesBeyond);
                }
                else
                {
                    FindBeyondVertices(newFace, oldFace.VerticesBeyond, adjacentFace.VerticesBeyond);
                }

                // Это лицо, грань, лежит на холме
                if (newFace.VerticesBeyond.Count == 0)
                {
                    ConvexFaces.Add(newFace.Index);
                    UnprocessedFaces.Remove(newFace);
                    ObjectManager.DepositVertexBuffer(newFace.VerticesBeyond);
                    newFace.VerticesBeyond = EmptyBuffer;
                }
                else // Добавить грань в список
                {
                    UnprocessedFaces.Add(newFace);
                }

                // Утилизировать объект
                ObjectManager.DepositDeferredFace(face);
            }

            // Утилизировать поврежденные грани
            for (int fIndex = 0; fIndex < AffectedFaceBuffer.Count; fIndex++)
            {
                var face = AffectedFaceBuffer[fIndex];
                UnprocessedFaces.Remove(FacePool[face]);
                ObjectManager.DepositFace(face);
            }
        }

        /// <summary>
        /// Проверить, правильно ли вершина v находится за пределами данноой грани. Если это так, добавить её за пределы вершины
        /// </summary>
        void IsBeyond(ConvexFaceInternal face, IndexBuffer beyondVertices, int v)
        {
            double distance = MathHelper.GetVertexDistance(v, face);
            if (distance >= PlaneDistanceTolerance)
            {
                if (distance > MaxDistance)
                {
                    // If it's within the tolerance distance, use the lex. larger point
                    if (distance - MaxDistance < PlaneDistanceTolerance)
                    {
                        if (LexCompare(v, FurthestVertex) > 0)
                        {
                            MaxDistance = distance;
                            FurthestVertex = v;
                        }
                    }
                    else
                    {
                        MaxDistance = distance;
                        FurthestVertex = v;
                    }
                }
                beyondVertices.Add(v);
            }
        }

        /// <summary>
        /// Используется для обновления граней
        /// </summary>
        void FindBeyondVertices(ConvexFaceInternal face, IndexBuffer beyond, IndexBuffer beyond1)
        {
            var beyondVertices = BeyondBuffer;

            MaxDistance = double.NegativeInfinity;
            FurthestVertex = 0;
            int v;

            for (int i = 0; i < beyond1.Count; i++) VertexMarks[beyond1[i]] = true;
            VertexMarks[CurrentVertex] = false;
            for (int i = 0; i < beyond.Count; i++)
            {
                v = beyond[i];
                if (v == CurrentVertex) continue;
                VertexMarks[v] = false;
                IsBeyond(face, beyondVertices, v);
            }

            for (int i = 0; i < beyond1.Count; i++)
            {
                v = beyond1[i];
                if (VertexMarks[v]) IsBeyond(face, beyondVertices, v);
            }

            face.FurthestVertex = FurthestVertex;

            // Переключить грань за пределы буфера
            var temp = face.VerticesBeyond;
            face.VerticesBeyond = beyondVertices;
            if (temp.Count > 0) temp.Clear();
            BeyondBuffer = temp;
        }

        void FindBeyondVertices(ConvexFaceInternal face, IndexBuffer beyond)
        {
            var beyondVertices = BeyondBuffer;

            MaxDistance = double.NegativeInfinity;
            FurthestVertex = 0;
            int v;

            for (int i = 0; i < beyond.Count; i++)
            {
                v = beyond[i];
                if (v == CurrentVertex) continue;
                IsBeyond(face, beyondVertices, v);
            }

            face.FurthestVertex = FurthestVertex;

            // Переключить грань за пределы буфера
            var temp = face.VerticesBeyond;
            face.VerticesBeyond = beyondVertices;
            if (temp.Count > 0) temp.Clear();
            BeyondBuffer = temp;
        }

        /// <summary>
        /// Пересчитывает центроиду текущего корпуса
        /// </summary>
        void UpdateCenter()
        {
            for (int i = 0; i < Dimension; i++) Center[i] *= ConvexHullSize;
            ConvexHullSize += 1;
            double f = 1.0 / ConvexHullSize;
            var co = CurrentVertex * Dimension;
            for (int i = 0; i < Dimension; i++) Center[i] = f * (Center[i] + Positions[co + i]);
        }

        /// <summary>
        /// Удаляет последнюю вершину из центра
        /// </summary>
        void RollbackCenter()
        {
            for (int i = 0; i < Dimension; i++) Center[i] *= ConvexHullSize;
            ConvexHullSize -= 1;
            double f = ConvexHullSize > 0 ? 1.0 / ConvexHullSize : 0.0;
            var co = CurrentVertex * Dimension;
            for (int i = 0; i < Dimension; i++) Center[i] = f * (Center[i] - Positions[co + i]);
        }

        /// <summary>
        /// Рукоятки исключительных вершин
        /// </summary>
        void HandleSingular()
        {
            RollbackCenter();
            SingularVertices.Add(CurrentVertex);

            // Это означает, что все затронутые грани должны находиться на корпусе и что все "вершины за пределами" единичны
            for (int fIndex = 0; fIndex < AffectedFaceBuffer.Count; fIndex++)
            {
                var face = FacePool[AffectedFaceBuffer[fIndex]];
                var vb = face.VerticesBeyond;
                for (int i = 0; i < vb.Count; i++)
                {
                    SingularVertices.Add(vb[i]);
                }

                ConvexFaces.Add(face.Index);
                UnprocessedFaces.Remove(face);
                ObjectManager.DepositVertexBuffer(face.VerticesBeyond);
                face.VerticesBeyond = EmptyBuffer;
            }
        }

        /// <summary>
        /// Найти выпуклую оболочку
        /// </summary>
        void FindConvexHull()
        {
            // Найти (размер + 1) начальной точки и создать симплексы
            InitConvexHull();

            // Развернут ьвыпуклую оболочку и грани
            while (UnprocessedFaces.First != null)
            {
                var currentFace = UnprocessedFaces.First;
                CurrentVertex = currentFace.FurthestVertex;

                UpdateCenter();

                // Затронутые грание помечаются
                TagAffectedFaces(currentFace);

                // Создать конус из текущей вершины и затронуть грани горизонта
                if (!SingularVertices.Contains(CurrentVertex) && CreateCone()) CommitCone();
                else HandleSingular();

                // Нужно для очистки тегов
                int count = AffectedFaceBuffer.Count;
                for (int i = 0; i < count; i++) AffectedFaceFlags[AffectedFaceBuffer[i]] = false;
            }
        }
    }
}
