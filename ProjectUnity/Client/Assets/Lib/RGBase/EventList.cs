using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RG.Basic {
    public class EventList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable {

        protected List<T> innerList = null;

        public enum SeqEventType {
            Add,
            Xor,
            Replace,
            Remove,
            Exclude,
            Alter,
            Sort,
            ReplaceWhole,
            AddRange
        }

        public delegate void EventListChangeDelegate(SeqEventType eventType, int index, object obj);
        public event EventListChangeDelegate OnChange;
        public void ClearOnChangeCallbacks() {
            OnChange = null;
        }
        internal void CallOnChange(SeqEventType type, int index, object obj) { if (OnChange != null) OnChange(type, index, obj); }
        internal bool HasCallOnChangeMethods { get { return OnChange != null; } }
        #region Structure

        public EventList() : base() { innerList = new List<T>(); }

        public EventList(int length) { innerList = new List<T>(length); }

        public EventList(IEnumerable<T> list) { innerList = new List<T>(list); }

        public EventList(T defaultValue, int length = 1) : this(length) { for (int i = 0; i < innerList.Count; i++) innerList[i] = defaultValue; }


        #endregion

        #region Getter

        public int Count { get { return innerList.Count; } }

        public T GetItem(int index) {
            return innerList[index];
        }

        public bool IsEmpty { get { return Count <= 0; } }

        public T[] AsArray { get { return innerList.ToArray(); } }

        public bool IsReadOnly { get { return false; } }

        public int IndexOf(T item) {
            return innerList.IndexOf(item);
        }
        public bool Contains(T item) {
            return innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            innerList.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator() {
            return innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return innerList.GetEnumerator();
        }

        /// <summary>
        /// Get default valute of type 'T'
        /// </summary>
        public T Default { get { return default(T); } }

        /// <summary>
        /// Get the first item of the seq
        /// </summary>
        public T First { get { return innerList[0]; } }

        /// <summary>
        /// Get the first item of the seq, return Default when empty 
        /// </summary>
        public T FirstOrDefault { get { return IsEmpty ? Default : First; } }

        /// <summary>
        /// Get the last item of the seq
        /// </summary>
        public T Last { get { return innerList[Count - 1]; } }

        /// <summary>
        /// Get the last item of the seq, return Default when empty 
        /// </summary>
        public T LastOrDefault { get { return IsEmpty ? Default : Last; } }

        /// <summary>
        /// Get a random item from the seq
        /// </summary>
        public T Rand { get { return innerList[Random.Range(0, Count)]; } }

        /// <summary>
        /// Get a random item feom the seq, return Default when empty
        /// </summary>
        public T RandOrDefault { get { return IsEmpty ? Default : Rand; } }

        /// <summary>
        /// Get a random item feom the seq
        /// </summary>
        /// <param name="defaultValue">defaultValue is Default if not be given</param>
        /// <returns>defaultValue when empty</returns>
        public T RandItem(T defaultValue = default(T)) { return IsEmpty ? defaultValue : Rand; }

        public T GetOrDefault(int index) {
            if (Count > index) return this[index];
            else return Default;
        }

        public T GetIfExist(int index, T defaultValue) {
            if (Count > index) return this[index];
            else return defaultValue;
        }

        /// <summary>
        /// Try to get an item at given index
        /// </summary>
        /// <param name="index">index of the item</param>
        /// <param name="item">return value</param>
        /// <returns>is the operation successful</returns>
        public bool TryGet(int index, out T item) {
            item = default(T);
            try { if (Count > index) { item = this[index]; return true; } } finally { }
            return false;
        }

        /// <summary>
        /// Get the 'Max' value of the seq, the specific rule of 'Max' should be given by the arg
        /// </summary>
        /// <param name="compFn">the rule of 'Max'</param>
        /// <returns>if the seq is empty, Default will be returned</returns>
        public T GetMaxBy(Comparison<T> compFn) {
            T returnValue = FirstOrDefault;
            for (int i = 0; i < Count; i++) if (compFn(this[i], returnValue) >= 0) returnValue = this[i];
            return returnValue;
        }

        public bool Exists(Predicate<T> PredExist) {
            return innerList.Exists(PredExist);
        }

        #endregion

        #region Setter

        public T this[int index] {
            get {
                return innerList[index];
            }
            set {
                innerList[index] = value;
                CallOnChange(SeqEventType.Alter, index, value);
            }
        }

        public void Insert(int index, T item) {
            innerList.Insert(index, item);
            CallOnChange(SeqEventType.Add, index, item);
        }



        public void Add(T item) {
            innerList.Add(item);
            CallOnChange(SeqEventType.Add, innerList.Count - 1, item);
        }

        public EventList<T> Add(IEnumerable<T> items) {
            innerList.AddRange(items);
            CallOnChange(SeqEventType.AddRange, innerList.Count - items.Count(), items);
            return this;
        }
        public EventList<T> ReplaceWhole(IEnumerable<T> items) {
            innerList.Clear();
            innerList.AddRange(items);
            CallOnChange(SeqEventType.ReplaceWhole, innerList.Count, items);
            return this;
        }
        public void Clear() {
            innerList.Clear();
            CallOnChange(SeqEventType.Remove, -1, null);
        }



        public bool Remove(T item) {
            int index = innerList.IndexOf(item);
            if (innerList.Remove(item)) {
                CallOnChange(SeqEventType.Remove, index, item);
                return true;
            }
            return false;
        }

        public EventList<T> Remove(Predicate<T> PredRemove) {
            int count = innerList.RemoveAll(PredRemove);
            CallOnChange(SeqEventType.Remove, -1, count);
            return this;
        }

        public EventList<T> Exclude(IList<T> containsSeq, bool isExcludeContains = true) {
            if (!isExcludeContains) {
                Seq<T> _removes = new Seq<T>();
                innerList.RemoveAll(n => {
                    if (!containsSeq.Contains(n)) {
                        if (HasCallOnChangeMethods) _removes.Add(n);
                        return true;
                    }
                    return false;
                });
                CallOnChange(SeqEventType.Exclude, _removes.Count, _removes);
            } else {
                int count = innerList.RemoveAll(n => containsSeq.Contains(n));
                CallOnChange(SeqEventType.Exclude, count, containsSeq);
            }
            return this;
        }

        public void RemoveAt(int index) {
            if (index < Count) {
                T item = innerList[index];
                innerList.RemoveAt(index);
                CallOnChange(SeqEventType.Remove, index, item);
            } else {
                throw new Exception("RemoveAt : Index " + index + "is out of range .");
            }
        }

        public bool Replace(T item, T newItem) {
            if (!innerList.Contains(item)) {
                Append(newItem);
                CallOnChange(SeqEventType.Add, -1, newItem);
                return false;
            }
            int index = IndexOf(item);
            innerList.Insert(index, newItem);
            innerList.Remove(item);
            CallOnChange(SeqEventType.Replace, -1, newItem);
            return true;
        }

        public EventList<T> Xor(T item) {
            if (!Contains(item)) innerList.Add(item);
            else innerList.Remove(item);
            CallOnChange(SeqEventType.Xor, -1, item);
            return this;
        }

        public EventList<T> Append(T item) {
            if (!Contains(item)) { Add(item); }
            return this;
        }

        public EventList<T> Append(IEnumerable<T> items) {
            bool appended = false;
            foreach (T t in items)
                if (!Contains(t)) { innerList.Add(t); appended = true; }
            if (appended) CallOnChange(SeqEventType.Add, -1, items);
            return this;
        }

        public EventList<T> Xor(IEnumerable<T> items) {
            foreach (T item in items)
                if (!Contains(item)) innerList.Add(item);
                else innerList.Remove(item); ;
            CallOnChange(SeqEventType.Xor, -1, items);
            return this;
        }

        public EventList<T> Sort(Comparison<T> comp) {
            innerList.Sort(comp);
            CallOnChange(SeqEventType.Sort, -1, null);
            return this;
        }

        public EventList<T> Sort(Func<T, IComparable> compFn) {
            Sort((x, y) => compFn(x).CompareTo(compFn(y)));
            CallOnChange(SeqEventType.Sort, -1, null);
            return this;
        }

        public EventList<T> MergeWith(Func<T, T, T> f, Seq<T> s) {
            for (int i = 0; i < Count && i < s.Count; i++) this[i] = f(this[i], s[i]);
            CallOnChange(SeqEventType.Alter, -1, this);
            return this;
        }

        public EventList<T> Convert(Converter<T, T> f, Seq<T> s) {
            for (int i = 0; i < Count && i < s.Count; i++) this[i] = f(this[i]);
            CallOnChange(SeqEventType.Alter, -1, this);
            return this;
        }

        #endregion

        #region Seq Functions

        public EventList<T> SeqDo(Action<T, int> actionWithCount) { for (int i = 0; i < Count; i++) actionWithCount(this[i], i); return this; }

        public RetnT SeqReduce<RetnT>(Func<RetnT, T, RetnT> reduceFunc, RetnT initValue) { for (int i = 0; i < Count; i++) initValue = reduceFunc(initValue, this[i]); return initValue; }

        public T FirstMatch(Predicate<T> predFn, T defaultValue = default(T)) { for (int i = 0; i < Count; i++) if (predFn(this[i])) return this[i]; return defaultValue; }

        public bool HasEvery(Predicate<T> predFn) { return !Exists(t => !predFn(t)); }

        public bool HasNone(Predicate<T> predFn) { return !Exists(t => predFn(t)); }

        public bool HasAny(Predicate<T> predFn) { return Exists(predFn); }

        #endregion

        public override string ToString() {
            return SeqReduce<string>((ret, n) => {
                return ret + '\n' + n.ToString();
            }, "Seq(" + Count + "):");
        }

    }
}