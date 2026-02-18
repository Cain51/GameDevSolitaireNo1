using RG.Basic.Helper;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RG.Basic {
    public class GapSeq<T> : IEnumerable<T>, IEnumerable, ICollection<T>, IList<T> where T : class {

        public event Action<int, T> OnAdd;
        public event Action<int, T> OnDel;
        public event Action OnClear;
        public void ClearEvents() {
            OnAdd = null;
            OnDel = null;
            OnClear = null;
        }

        private Seq<int> usedKeys = new Seq<int>();
        private List<T> data = new List<T>(2);
        private int _freeRow = 0;

        internal int FreeRow {
            get {
                int index = _freeRow;
                while (!IsFree(index)) index++;
                return index;
            }
        }

        public T this[int key] {
            get { return IsFree(key) ? null : data[key]; }
            set { Insert(key, value); }
        }

        public List<T> Raw { get { return data; } }
        public Seq<int> UsedKeys { get { return usedKeys; } }
        public int Count { get { return usedKeys.Count; } }

        protected bool IsFree(int row) { return data.Count <= row || data[row] == null; }

        public bool IsReadOnly { get { return false; } }
        private void TryExpandSize(int key) {
            if (key >= data.Capacity) { data.Capacity = Mth.Max(data.Capacity * 2, key + 1); }
            for (int i = 0; i < key - Count + 1; i++) data.Add(null);
        }
        private void AddUsedKey(int key) { usedKeys.Append(key); }
        private void RemoveUsedKey(int key) { if (usedKeys.Contains(key)) usedKeys.RemoveItem(key); }
         
        public virtual void Clear() {
            _freeRow = 0;
            data.Clear();
            usedKeys.Clear();
            if (OnClear != null) OnClear();
        }
         
        /// <summary>
        /// 在某个位置设值, 如果值存在则覆盖, *不会*导致其他项的移动
        /// </summary>
        public virtual void Insert(int key, T item) {
            if (item == null) RemoveByInd(key);
            else {
                TryExpandSize(key);

                if (IsFree(key)) {
                    AddUsedKey(key);
                    data[key] = item;
                    if (OnAdd != null) OnAdd(key, item);
                } else {
                    T old = data[key];
                    data[key] = item;
                    
                    if (OnDel != null) OnDel(key, data[key]);
                    if (OnAdd != null) OnAdd(key, item);
                }
            }
        }

        public virtual int Add(T item) {
            int free = FreeRow;
            Insert(free, item);
            return free;
        }

        public virtual bool RemoveByInd(int key) {
            if (IsFree(key)) return false;
            T old = data[key];
            data[key] = null;
            RemoveUsedKey(key);
            _freeRow = Mth.Min(key, _freeRow);
            if (OnDel != null) OnDel(key, old);
            return true;
        }

        public virtual bool Remove(T item) {
            int index = IndexOf(item);
            return RemoveByInd(index);
        }

        public int IndexOf(T item) { return data.IndexOf(item); }

        public bool Contains(T item) { return data.Contains(item); }

        public void CopyTo(T[] array, int index) { data.CopyTo(array, index); }

        public IEnumerator<T> GetEnumerator() { return data.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return data.GetEnumerator(); }

        void IList<T>.RemoveAt(int index) { RemoveByInd(index); }

        void ICollection<T>.Add(T item) { Add(item); }

        public int FindIndex(Predicate<T> match) { return data.FindIndex(match); }

        public GapSeq<T> DoSeq(Action<T> action) { for (int i = 0; i < Count; i++) action(this[usedKeys[i]]); return this; }

        public GapSeq<T> DoSeq(Action<T, int> actionWithCount) { for (int i = 0; i < Count; i++) actionWithCount(this[usedKeys[i]], usedKeys[i]); return this; }

        public Seq<T> Map(Converter<T, T> mapFunc) { return Map<T>(mapFunc); }

        public Seq<ReturnType> Map<ReturnType>(Converter<T, ReturnType> mapFn) {
            Seq<ReturnType> seqReturn = new Seq<ReturnType>();
            for (int i = 0; i < Count; i++) seqReturn.Add(mapFn(this[usedKeys[i]]));
            return seqReturn;
        }

        public TypeInit Reduce<TypeInit>(TypeInit initValue, Func<TypeInit, T, TypeInit> reduceFunc) { 
            for (int i = 0; i < Count; i++) initValue = reduceFunc(initValue, this[UsedKeys[i]]);
            return initValue;
        }

        public override string ToString() { 
            return Reduce(StrGen.Start("GapSeq<{0}>({1}):", typeof(T).Name, Count), (s, t) => s[" "][t]).End; 
        }
    }
}
