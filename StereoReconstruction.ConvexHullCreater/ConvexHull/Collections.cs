using System;

namespace StereoReconstruction.ConvexHullCreater
{
    /// <summary>
    /// Более легкая альтернатива List T
    /// Включает в себя функциональность стека
    /// </summary>
    class SimpleList<T>
    {
        T[] items;
        int capacity;

        public int Count;

        /// <summary>
        /// Получить i-й элемент
        /// </summary>
        public T this[int i]
        {
            get { return items[i]; }
            set { items[i] = value; }
        }

        /// <summary>
        /// Размер имеет значение
        /// </summary>
        void EnsureCapacity()
        {
            if (capacity == 0)
            {
                capacity = 32;
                items = new T[32];
            }
            else
            {
                var newItems = new T[capacity * 2];
                Array.Copy(items, newItems, capacity);
                capacity = 2 * capacity;
                items = newItems;
            }
        }

        /// <summary>
        /// Добавление вершин в буфер
        /// </summary>
        public void Add(T item)
        {
            if (Count + 1 > capacity) EnsureCapacity();
            items[Count++] = item;
        }

        /// <summary>
        /// Помещает значение в конец списка
        /// </summary>
        public void Push(T item)
        {
            if (Count + 1 > capacity) EnsureCapacity();
            items[Count++] = item;
        }

        /// <summary>
        /// Получает последнее значение из списка
        /// </summary>
        public T Pop()
        {
            return items[--Count];
        }

        /// <summary>
        /// Устанавливает счетчик в 0, иначе ничего не делает
        /// </summary>
        public void Clear()
        {
            Count = 0;
        }
    }

    /// <summary>
    /// Список целых значений
    /// </summary>
    class IndexBuffer : SimpleList<int>
    {

    }

    /// <summary>
    /// Один из приоритетов на основе связанного списка
    /// </summary>
    sealed class FaceList
    {
        ConvexFaceInternal first, last;

        /// <summary>
        ///Получить первый элемент
        /// </summary>
        public ConvexFaceInternal First { get { return first; } }

        /// <summary>
        /// Добавляет элемент в начало.
        /// </summary>
        void AddFirst(ConvexFaceInternal face)
        {
            face.InList = true;
            first.Previous = face;
            face.Next = first;
            first = face;
        }

        /// <summary>
        /// Добавляет грань в список
        /// </summary>
        public void Add(ConvexFaceInternal face)
        {
            if (face.InList)
            {
                if (first.VerticesBeyond.Count < face.VerticesBeyond.Count)
                {
                    Remove(face);
                    AddFirst(face);
                }
                return;
            }

            face.InList = true;

            if (first != null && first.VerticesBeyond.Count < face.VerticesBeyond.Count)
            {
                first.Previous = face;
                face.Next = first;
                first = face;
            }
            else
            {
                if (last != null)
                {
                    last.Next = face;
                }
                face.Previous = last;
                last = face;
                if (first == null)
                {
                    first = face;
                }
            }
        }

        /// <summary>
        /// Удаляет элемент из списка
        /// </summary>
        public void Remove(ConvexFaceInternal face)
        {
            if (!face.InList) return;

            face.InList = false;

            if (face.Previous != null)
            {
                face.Previous.Next = face.Next;
            }
            else if (face.Previous == null)
            {
                first = face.Next;
            }

            if (face.Next != null)
            {
                face.Next.Previous = face.Previous;
            }
            else if (face.Next == null)
            {
                last = face.Previous;
            }

            face.Next = null;
            face.Previous = null;
        }
    }

    /// <summary>
    /// Коннектор списка
    /// </summary>
    sealed class ConnectorList
    {
        FaceConnector first, last;

        /// <summary>
        /// олучить первый элемент
        /// </summary>
        public FaceConnector First { get { return first; } }

        /// <summary>
        /// Добавляет элемент в начало
        /// </summary>
        void AddFirst(FaceConnector connector)
        {
            first.Previous = connector;
            connector.Next = first;
            first = connector;
        }

        /// <summary>
        /// Добавляет грань в список
        /// </summary>
        public void Add(FaceConnector element)
        {
            if (last != null)
            {
                last.Next = element;
            }
            element.Previous = last;
            last = element;
            if (first == null)
            {
                first = element;
            }
        }

        /// <summary>
        /// Удаляет элемент из списка
        /// </summary>
        public void Remove(FaceConnector connector)
        {
            if (connector.Previous != null)
            {
                connector.Previous.Next = connector.Next;
            }
            else if (connector.Previous == null)
            {
                first = connector.Next;
            }

            if (connector.Next != null)
            {
                connector.Next.Previous = connector.Previous;
            }
            else if (connector.Next == null)
            {
                last = connector.Previous;
            }

            connector.Next = null;
            connector.Previous = null;
        }
    }
}
