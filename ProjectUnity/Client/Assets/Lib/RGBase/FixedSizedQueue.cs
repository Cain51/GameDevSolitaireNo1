using System.Collections.Generic;

namespace RG.Basic
{
    public class FixedSizedQueue<T> : List<T>
    {
        private int maxCount;
        public FixedSizedQueue(int _maxCount) : base(_maxCount)
        {
            maxCount = _maxCount;
        }
        public void Enqueue(T item)
        {
            this.Add(item);
            while (this.Count > maxCount)
                this.RemoveAt(this.Count-1);
        }
    }
}