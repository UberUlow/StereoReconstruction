using System;

namespace StereoReconstruction.ConvexHullCreater
{
    /// <summary>
    /// Вспомогательный класс для размещения и хранения объектов
    /// Это помогает GC т.к. препятствует созданию 75% новыъ объектов граней (в случае выпуклой поверхности)
    /// </summary>
    class ObjectManager
    {
        readonly int Dimension;

        ConvexHullInternal Hull;
        int FacePoolSize, FacePoolCapacity;
        ConvexFaceInternal[] FacePool;
        IndexBuffer FreeFaceIndices;
        FaceConnector ConnectorStack;
        SimpleList<IndexBuffer> EmptyBufferStack;
        SimpleList<DeferredFace> DeferredFaceStack;

        /// <summary>
        /// Вернуть грань в бассейн для последующего использования
        /// </summary>
        public void DepositFace(int faceIndex)
        {
            var face = FacePool[faceIndex];
            var af = face.AdjacentFaces;
            for (int i = 0; i < af.Length; i++)
            {
                af[i] = -1;
            }
            FreeFaceIndices.Push(faceIndex);
        }

        /// <summary>
        /// Перераспределить грани в бассейне, в том числе поврежденные грани
        /// </summary>
        void ReallocateFacePool()
        {
            var newPool = new ConvexFaceInternal[2 * FacePoolCapacity];
            var newTags = new bool[2 * FacePoolCapacity];
            Array.Copy(FacePool, newPool, FacePoolCapacity);
            Buffer.BlockCopy(Hull.AffectedFaceFlags, 0, newTags, 0, FacePoolCapacity * sizeof(bool));
            FacePoolCapacity = 2 * FacePoolCapacity;
            Hull.FacePool = newPool;
            FacePool = newPool;
            Hull.AffectedFaceFlags = newTags;
        }

        /// <summary>
        /// Создание новой грани и помещение её в бассейн
        /// </summary>
        int CreateFace()
        {
            var index = FacePoolSize;
            var face = new ConvexFaceInternal(Dimension, index, GetVertexBuffer());
            FacePoolSize++;
            if (FacePoolSize > FacePoolCapacity) ReallocateFacePool();
            FacePool[index] = face;
            return index;
        }

        /// <summary>
        /// Возвращение индекса неиспользованной грани или создание новый
        /// </summary>
        public int GetFace()
        {
            if (FreeFaceIndices.Count > 0) return FreeFaceIndices.Pop();
            return CreateFace();
        }

        /// <summary>
        /// Хранение разъема для грани в "встроенных" связанном списке
        /// </summary>
        public void DepositConnector(FaceConnector connector)
        {
            if (ConnectorStack == null)
            {
                connector.Next = null;
                ConnectorStack = connector;
            }
            else
            {
                connector.Next = ConnectorStack;
                ConnectorStack = connector;
            }
        }

        /// <summary>
        /// Получить неиспользуемые разъем для грани. Если ни один не доступен, создать его.
        /// </summary>
        public FaceConnector GetConnector()
        {
            if (ConnectorStack == null) return new FaceConnector(Dimension);

            var ret = ConnectorStack;
            ConnectorStack = ConnectorStack.Next;
            ret.Next = null;
            return ret;
        }

        /// <summary>
        /// Буфер индексов депозитов
        /// </summary>
        public void DepositVertexBuffer(IndexBuffer buffer)
        {
            buffer.Clear();
            EmptyBufferStack.Push(buffer);
        }

        /// <summary>
        /// Получить индекс буфера хранения или создать новый экземпляр
        /// </summary>
        public IndexBuffer GetVertexBuffer()
        {
            return EmptyBufferStack.Count != 0 ? EmptyBufferStack.Pop() : new IndexBuffer();
        }

        /// <summary>
        /// Депозит отложенной грани
        /// </summary>
        public void DepositDeferredFace(DeferredFace face)
        {
            DeferredFaceStack.Push(face);
        }

        /// <summary>
        /// Получить отложенную грань
        /// </summary>
        public DeferredFace GetDeferredFace()
        {
            return DeferredFaceStack.Count != 0 ? DeferredFaceStack.Pop() : new DeferredFace();
        }

        /// <summary>
        /// Создание менеджера
        /// </summary>
        public ObjectManager(ConvexHullInternal hull)
        {
            Dimension = hull.Dimension;
            Hull = hull;
            FacePool = hull.FacePool;
            FacePoolSize = 0;
            FacePoolCapacity = hull.FacePool.Length;
            FreeFaceIndices = new IndexBuffer();

            EmptyBufferStack = new SimpleList<IndexBuffer>();
            DeferredFaceStack = new SimpleList<DeferredFace>();
        }
    }
}
