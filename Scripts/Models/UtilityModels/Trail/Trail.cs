using System;
using System.Collections.Generic;
using Godot;

namespace NBodySimulation.Models
{
    /// <summary>
    /// Структура для следа от движения
    /// <para>
    /// По сути -- кольцевой буффер
    /// </para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Trail<T>
    {
        private T[] data;
        private int idx;
        public int Capacity { get; private set; }
        public int Count { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public T this[int i]
        {
            get
            {
                int real_i = (idx - i + Capacity - 1) % Capacity;
                //if (real_i < 0) real_i = Capacity+real_i;

                return data[real_i];
            }
        }

        public Trail(int lenth)
        {
            if (lenth < 0) lenth = 0;
            data = new T[lenth];
            Capacity = lenth;
            Count = 0;
            idx = 0;
        }

        public void Add(T value)
        {
            if (Count < Capacity) Count++;
            data[idx] = value;
            idx = (idx + 1) % Capacity;
        }

        public List<T> ToList()
        {
            List<T> arr = [];
            for (int i=0; i<Count; i++)
                arr.Add(this[i]);

            return arr;
        }
    }
}