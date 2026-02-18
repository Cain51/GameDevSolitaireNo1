using System.Collections;
using RG.Basic;
using System;
using System.Collections.Generic;

namespace RG.Basic
{
    /// <summary>
    /// 网上找的优先队列模板，小根堆
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueue<T> where T : IComparable
    {
        public Seq<T> heap = new Seq<T>();

        public int Count { get; set; }

        public PriorityQueue() { Count = 0; }


        public void Clear(bool force = true)
        {
            Count = 0;
            if (force) heap.Clear();
        }

        public void Push(T v)
        {
            if (Count == heap.Count) heap.Add(v);
            else heap[Count] = v;
            SiftUp(Count++);
        }

        public T Pop()
        {
            var v = Top();
            heap[0] = heap[Count - 1];
            heap[--Count] = default(T);
            if (Count > 0) SiftDown(0);
            return v;
        }

        public delegate bool removeCondition(T t);
        public T Remove(removeCondition condition)
        {
            if(condition == null || Count == 0)return default(T);
            if(condition(heap[0]))
            {
                return Pop();
            }
            else if (Count >= 3)
            {
                for (int i = 1; i < Count - 1; i++)
                {
                    if (condition(heap[i]))
                    {
                        T v = heap[i];
                        heap[i] = heap[Count - 1];
                        heap[--Count] = default(T);
                        if (v.CompareTo(heap[i]) > 0)
                        {
                            SiftUp(i);
                        }
                        else
                        {
                            SiftDown(i);
                        }
                        return v;
                    }
                }
                if (condition(heap[Count - 1]))
                {
                    T v = heap[Count - 1];
                    heap[--Count] = default(T);
                    return v;
                }
            }
            return default(T);
        }

        public T Top()
        {
            if (Count > 0) return heap[0];
            throw new InvalidOperationException("优先队列为空");
        }

        public void SiftUp(int n)
        {
            var v = heap[n];
            for (int n2 = (n >> 1); n > 0 && v.CompareTo(heap[n2]) < 0; n = n2, n2 >>= 1) heap[n] = heap[n2];
            heap[n] = v;
        }

        public void SiftDown(int n)
        {
            var v = heap[n];
            for (int n2 = (n << 1); n2 < Count; n = n2, n2 <<= 1)
            {
                int n3 = n2 | 1;
                if(n3 < Count)
                {
                    if(v.CompareTo(heap[n2]) > 0 )
                    {
                        if(v.CompareTo(heap[n3]) > 0) 
                        {
                            if(heap[n3].CompareTo(heap[n2]) > 0)
                            {
                                heap[n] = heap[n2];
                            }
                            else
                            {
                                heap[n] = heap[n3];
                                n2 = n3;
                            }
                        }
                        else
                        {
                            heap[n] = heap[n2];
                        }
                    }
                    else
                    {
                        if(v.CompareTo(heap[n3]) > 0)
                        {
                            heap[n] = heap[n3];
                            n2 = n3;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if (v.CompareTo(heap[n2]) > 0)
                    {
                        heap[n] = heap[n2];
                    }
                    else
                    {
                        break;
                    }
                }
            }
            heap[n] = v;
        }

        
    }

}
