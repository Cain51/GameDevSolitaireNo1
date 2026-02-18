
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RG.Basic
{
    /// <summary>
    /// Seq  
    /// </summary>
    /// <remarks>
    ///     Create Time : Kinghand @ 2014/05/01
    ///     Current Edition : 2.0
    ///     Lastest Change : 2015/11/18
    /// </remarks>
    public class Seq<T> : List<T>, IComparable, IEnumerable
    {
        public static readonly Seq<T> Empty = new Seq<T>();

        #region Base

        public Seq() : base() { }
         
        public Seq(List<T> list) : base(list) { }

        public Seq(T[] list) : base(list) { }

	    public Seq(Seq<T> list) : base(list) { }
	    
	    public Seq(HashSet<T> list) : base(list) { }

        public Seq(T defaultValue, int length = 1) : this(new T[length]) { 
            for (int i = 0; i < Count; i++) {
                if (i < Count) this[i] = defaultValue;
                else Add(defaultValue);
            }
        }

        public bool IsEmpty { get { return Count <= 0; } }
        public int LastIndex { get { return Count - 1; } }

        #endregion Base

        #region Value

        public Seq<T> Clone { get { return new Seq<T>(this); } }

        public T[] AsArray { get { return base.ToArray(); } }

        /// <summary>
        /// Get default valute of type 'T'
        /// </summary>
        public static T Default { get { return default(T); } }

        /// <summary>
        /// Get the first item of the seq
        /// </summary>
        public T First { get { return this[0]; } }
        public T Second { get { return this[1]; } }
        public T Third { get { return this[2]; } }
        public T The4th { get { return this[3]; } }
        /// <summary>
        /// Get the first item of the seq, return Default when empty 
        /// </summary>
        public T FirstOrDefault { get { return IsEmpty ? Default : First; } }
        public T SecondOrDefault { get { return IsEmpty ? Default : Second; } }
        public T ThirdOrDefault { get { return IsEmpty ? Default : Third; } } 
        public T The4thOrDefault { get { return IsEmpty ? Default : The4th; } }
        /// <summary>
        /// Get the last item of the seq
        /// </summary>
        public T Last { get { return this[Count - 1]; } }

        /// <summary>
        /// Get the last item of the seq, return Default when empty 
        /// </summary>
        public T LastOrDefault { get { return IsEmpty ? Default : Last; } }

        /// <summary>
        /// Get a random item from the seq
        /// </summary>
        public T Rand { get { if (Count <= 0) throw new ArgumentOutOfRangeException("Rand Method Cannot Apply To Seq(0) ."); else  return this[Random.Range(0, Count)]; } }

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

        /// <summary>
        /// Get a continuous part of the seq
        /// </summary>
        /// <param name="begin">begin index of the part</param>
        /// <param name="length">length index of the part</param>
        /// <remarks> 
        /// no exception would be thrown when the part is exceeded the seq
        /// </remarks>
        /// <returns>if the part is exceeded the seq, the real length of the part will be shorten</returns>
        public Seq<T> Sub(int begin, int length) {
            Seq<T> split = new Seq<T>();
            for (int i = 0; i < System.Math.Min(begin + length, Count); i++) split.Add(this[i]); 
            return split;
        }

        /// <summary>
        /// Get a continous part from the left of the seq
        /// </summary>
        /// <param name="amount">items amount of the part</param>
        /// <remarks> 
        /// no exception would be thrown when the part is exceeded the seq
        /// </remarks>
        /// <returns>if amount is larger than the count of the seq, the real length of the part will be shorten</returns>
        public Seq<T> Left(int amount) {
            if (amount >= Count) return Clone;
            return Sub(0, amount);
        }

        /// <summary>
        /// Get a continous part from the right of the seq
        /// </summary>
        /// <param name="amount">items amount of the part</param>
        /// <remarks> 
        /// no exception would be thrown when the part is exceeded the seq
        /// </remarks>
        /// <returns>if amount is larger than the count of the seq, the real length of the part will be shorten</returns>
        public Seq<T> Right(int amount) {
            if (amount >= Count) return Clone;
            return Sub(Count - amount - 1, Count - 1);
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
        public bool ContainsInt(int item)
        {
            for (int i = 0; i < Count; i++)
            {
                if ((this[i] as IComparable).CompareTo(item as IComparable) == 0)
                    return true;
            }
            return false;
        }
        public bool Has(T item) { for (int i = 0; i < Count; i++) if ((this[i] as IComparable).CompareTo(item as IComparable) == 0) return true; return false; }

        public T GetIfExist(int index, T _defaultValue) {
            if(Count > index && index >= 0) return this[index];
            else return _defaultValue;
        }
        public T TryGetItem(int index, T val)
        {
            if (index > -1 && index < Count)
                return this[index];
            return val;
        }
        public T GetItem(int index) { 
            return this[index];
        }

        /// <summary>
        /// Get the 'Max' value of the seq, the specific rule of 'Max' should be given by the arg
        /// </summary>
        /// <param name="compFn">the rule of 'Max'</param>
        /// <returns>if the seq is empty, Default will be returned</returns>
        public T GetMaxBy(Comparison<T> compFn)
        {
            T returnValue = FirstOrDefault;
            for (int i = 0; i < Count; i++) if (compFn(this[i], returnValue) >= 0) returnValue = this[i];
            return returnValue;
        }
          

        #endregion Value

        #region Edit
         
        /// <summary>
        /// Clear the seq
        /// </summary> 
        /// <returns>return self for pipeline</returns>
        public new virtual Seq<T> Clear() { base.Clear(); return this; }
         
        /// <summary>
        /// Export the seq to a array
        /// </summary>
        /// <param name="array">output array</param>
        /// <param name="arrayIndex">the zero-based index in array at which copying begins</param>
        /// <returns>return self for pipeline</returns>
        public new Seq<T> CopyTo(T[] array, int arrayIndex) { base.CopyTo(array, arrayIndex); return this; }

        /// <summary>
        /// Insert an item at specific index
        /// </summary>
        /// <param name="index">index to insert</param>
        /// <param name="item">item to insert</param>
        /// <returns>return self for pipeline</returns>
        public new Seq<T> Insert(int index, T item) { base.Insert(index, item); return this; }

        /// <summary>
        /// Remove an item at index of the seq
        /// </summary>
        /// <param name="index">index of item to remove</param>
        /// <returns>return self for pipeline</returns>
        public new Seq<T> RemoveAt(int index) { base.RemoveAt(index); return this; }

        /// <summary>
        /// Remove an specific item form the seq
        /// </summary>
        /// <param name="item">the item to remove</param>
        /// <returns>return self for pipeline</returns>
        public Seq<T> RemoveItem(T item) { base.Remove(item); return this; }

        /// <summary>
        /// Pop the head item of the seq
        /// </summary>
        /// <param name="item">the out value</param>
        /// <returns>return self for pipeline</returns>
        public Seq<T> PopFirst(out T item) { item = FirstOrDefault; if (Count > 0) this.RemoveAt(0); return this; }

        public T PopFirst() {
            T item = FirstOrDefault;
            if (Count > 0) RemoveAt(0);
            return item;
        }

        /// <summary>
        /// Pop the tile item of the seq
        /// </summary>
        /// <param name="item">the out value</param>
        /// <returns>return self for pipeline</returns>
        public Seq<T> PopLast(out T t) { t = LastOrDefault; if (Count > 0) this.RemoveAt(Count - 1); return this; }

        /// <summary>
        /// push an item at the head of the seq
        /// </summary>
        /// <param name="item">the item</param>
        /// <returns>return self for pipeline</returns>
        public Seq<T> PushFirst(T t) { return this.Insert(0, t); }

        /// <summary>
        /// push an item at the tail of the seq
        /// </summary>
        /// <param name="item">the item</param>
        /// <returns>return self for pipeline</returns>
        public Seq<T> PushLast(T t) { return Add(t); }
         
        /// <summary>
        /// push an item to the stack(seq)
        /// </summary>
        /// <param name="item">the item</param>
        /// <returns>the item self</returns>
        public T StackPush(T t) { PushLast(t); return t; }

        /// <summary>
        /// pop an item from the stack(seq)
        /// </summary> 
        /// <returns>the item</returns>
        public T StackPop() { T t = Default; PopLast(out t); return t; }

        /// <summary>
        /// push an item to the queue(seq)
        /// </summary>
        /// <param name="item">the item</param>
        /// <returns>the item self</returns>
        public T QueuePush(T t) { PushFirst(t); return t; }

        /// <summary>
        /// pop an item from the queue(seq)
        /// </summary> 
        /// <returns>the item</returns>
        public T QueuePop() { T t = Default; PopLast(out t); return t; }

        public int CompareTo(object aim) { //kh. 未测试
            IList<T> aimSeq = aim as IList<T>;
            int result = aimSeq == null ? -1 : 0;
            if (result != 0) return result;
            result = Count - aimSeq.Count;
            if (result != 0) return result;
            for(int i = 0; i < Count; i++) {
                IComparable c = (GetItem(i) as IComparable);
                result = c == null ? -1 : c.CompareTo(aimSeq[i] as IComparable);
                if (result != 0) return result;
            }
            return result;
        }

        /// <summary>
        /// add an item to the end of the seq
        /// </summary>
        /// <param name="item"></param>
        /// <returns>return self for pipeline</returns>
        public new Seq<T> Add(T item) { base.Add(item); return this; }
         
        /// <summary>
        /// Add elements fo specific collection to the end of the seq
        /// </summary>
        /// <returns>return self for pipeline</returns>
        public Seq<T> Add(IEnumerable<T> items) { base.AddRange(items); return this; }

        /// <summary>
        /// Add element of specific collection, which not exist in the seq, to the end of the seq
        /// </summary>
        /// <returns>return self for pipeline</returns>
        public Seq<T> Append(T t) { if (!Contains(t)) Add(t); return this; }




        /// <summary>
        /// Add elements of specific collection, which not exist in the seq, to the end of the seq
        /// </summary>
        /// <returns>return self for pipeline</returns>
        public Seq<T> Append(IEnumerable<T> items) { foreach(T t in items) if (!Contains(t)) Add(t); return this; }


        /// <summary>
        /// Add element which not exist in the seq of specific collection, and remove it when already exist
        /// </summary>
        /// <returns>return self for pipeline</returns>
        public Seq<T> Xor(T t) { if(!Contains(t)) Add(t); else RemoveItem(t); return this; }

        /// <summary>
        /// Add elements which not exist in the seq of specific collection, and remove elements which already exist in the seq
        /// </summary>
        /// <returns>return self for pipeline</returns>
        public Seq<T> Xor(IEnumerable<T> items) { foreach (T t in items) if (!Contains(t)) Add(t); else RemoveItem(t); return this; } 

        #endregion

        #region Find

        /// <summary>
        /// Determines whether all items in the seq are match the condition
        /// </summary>
        /// <param name="predFn">the specific condition</param> 
        public bool HasEvery(Predicate<T> predFn) { return !Exists(t => !predFn(t)); }

        /// <summary>
        /// Determines whether none items in the seq are match the condition
        /// </summary>
        /// <param name="predFn">the specific condition</param> 
        public bool HasNone(Predicate<T> predFn) { return !Exists(t => predFn(t)); }

        /// <summary>
        /// Determines whether any items in the seq are match the condition
        /// </summary>
        /// <param name="predFn">the specific condition</param> 
        public bool HasAny(Predicate<T> predFn) { return Exists(predFn); }

        /// <summary>
        /// Count the items which are match the condition
        /// </summary>
        /// <param name="predFn">the specific condition</param>  
        public int AmountOf(Predicate<T> predFn) { int amount = 0; for (int i = 0; i < Count; i++) amount += predFn(this[i]) ? 1 : 0; return amount; }

        #endregion Find

        #region inmutable seq operation

        /// <summary>
        /// Execute action for every items in the seq
        /// </summary>
        /// <param name="action">specific action</param>
        /// <returns>return self for pipeline</returns>
        public Seq<T> DoSeq(Action<T> action) { for (int i = 0; i < Count; i++) action(this[i]); DoSeqRemover_Excute(); return this; }

        /// <summary>
        /// Execute action for every items and theirs index in the seq
        /// </summary>
        /// <param name="actionWithCount">specific action </param>
        /// <returns>return self for pipeline</returns>
        public Seq<T> DoSeq(Action<T, int> actionWithCount) { for (int i = 0; i < Count; i++) actionWithCount(this[i], i); return this; }

        /// <summary>
        /// Do a function of the whole seq
        /// </summary>
        /// <param name="SeqFunc">specific action </param>
        /// <returns>return self for pipeline</returns>
        public Seq<T> Do(Func<Seq<T>, Seq<T>> SeqFunc) { return SeqFunc(this.Clone); }

        /// <summary>
        /// map the seq to a new copy
        /// </summary>
        /// <param name="mapFunc">the map function</param>
        /// <returns>the new copy</returns>
        public Seq<T> Map(Converter<T, T> mapFunc) { return Map<T>(mapFunc); }

        /// <summary>
        /// map the seq to a new copy of another type template
        /// </summary>
        /// <typeparam name="ReturnType">type of the new seq</typeparam>
        /// <param name="mapFn">the map function</param>
        /// <returns>the new copy</returns>
        public Seq<ReturnType> Map<ReturnType>(Converter<T, ReturnType> mapFn) {
            Seq<ReturnType> seqReturn = new Seq<ReturnType>();
            for (int i = 0; i < Count; i ++ ) seqReturn.Add(mapFn(this[i])); 
            return seqReturn;
        }
        
        /// <summary>
        /// copy of the seq that filted items matching specific condition of the seq
        /// </summary>
        /// <param name="predFn">specific condition</param>
        /// <returns>return self for pipeline</returns>
        public Seq<T> Filter(Predicate<T> predFn) {
            Seq<T> seqReturn = new Seq<T>();
            for (int i = 0; i < Count; i++) if (predFn(this[i])) seqReturn.Add(this[i]);
            return seqReturn;
        }

        /// <summary>
        /// get a copy of the seq contained items that not matching specific condition of the seq
        /// </summary>
        /// <param name="predFn">specific condition</param>
        /// <returns>return self for pipeline</returns>
        public Seq<T> Remove(Predicate<T> predFn) {
            Seq<T> seqReturn = new Seq<T>();
            for (int i = 0; i < Count; i++) if (!predFn(this[i])) seqReturn.Add(this[i]);
            return seqReturn;
        }

        /// <summary>
        /// get sorted copy of the seq by specific Comparison
        /// </summary>
        /// <param name="compFn">specific compeison</param>
        /// <returns>return self for pipeline</returns>
        public Seq<T> SortBy(Comparison<T> compFn) { Seq<T> seq = Clone; Clone.QuickSort(compFn, 0, Count - 1); return seq; }

        /// <summary>
        /// get sorted copy of the seq by specific mapping of items 
        /// </summary>
        /// <param name="compFn">specific mapping</param>
        /// <returns>return self for pipeline</returns>
        public Seq<T> SortBy(Func<T, IComparable> compFn) { return SortBy((x, y) => compFn(x).CompareTo(compFn(y))); }
         
        /// <summary>
        /// get Unique copy of the seq
        /// </summary>
        public Seq<T> Unique {
            get {  
                Seq<T> nesSeq = new Seq<T>();
                for (int i = 0; i < Count; i++) { if (!nesSeq.Contains(this[i])) nesSeq.Add(this[i]); };
                return nesSeq;
            }
        }

        public Seq<T> Reversal {
            get {
                Seq<T> nesSeq = Clone;
                nesSeq.Reverse();
                return nesSeq;
            }
        }

        /// <summary>
        /// get Unique copy of the seq
        /// </summary>
        public Seq<T> Shuffle {
            get {
                Seq<T> nesSeq = Clone; 
                for (int i = 0; i < Count; i++) { int pos = Random.Range(0, nesSeq.Count); if (pos != i) { T tmp = nesSeq[i]; nesSeq[i] = nesSeq[pos]; nesSeq[pos] = tmp; } } 
                return nesSeq;
            }
        }

        public Seq<T> Scroll(int num) { 
            Seq<T> nesSeq = new Seq<T>();
            for(int i = 0; i < Count; i++) {
                int j = (num % Count);
                nesSeq.Add(GetItem(j));
                num++;
            } 
            return nesSeq; 
        }

        public Seq<T> Distinct(System.Converter<T, T> mapFunc) { return Map(mapFunc).Unique; }

        public Map<string, Seq<T>> Classify(Func<T, IComparable> classifier, bool hashCodeKey = true)
        {
            Map<string, Seq<T>> hashSet = new Map<string, Seq<T>>();
            DoSeq(t =>
                hashSet.AppendValue<T>(
                    hashCodeKey ?
                    classifier(t).GetHashCode().ToString() : classifier(t).ToString(),
                    t, new Seq<T>(), (s, ts) => s + ts));
            return hashSet;
        }

        public Seq<T> MergeBy(Func<T, IComparable> classifier, Func<T, T, T> reduceFunc)
        {
            Map<string, Seq<T>> map = Classify(classifier);
            return map.ValueSeq.Map<T>(seq => seq.Reduce(reduceFunc));
        }

        public Seq<ReturnType> MergeWith<ReturnType>(Func<T, T, ReturnType> f, Seq<T> s) { return MergeWith<T, ReturnType>(f, s); }

        public Seq<ReturnType> MergeWith<TargetType, ReturnType>(Func<T, TargetType, ReturnType> fnAdd2, Seq<TargetType> seqTarget)
        {
            Seq<ReturnType> rMerge = new Seq<ReturnType>();
            int _count = System.Math.Min(Count, seqTarget.Count);
            for (int i = 0; i < _count; i++) rMerge.Add(fnAdd2(this[i], seqTarget[i]));
            return rMerge;
        }

        public Seq<ReturnType> MergeWith<TargetType1, TargetType2, ReturnType>(Func<T, TargetType1, TargetType2, ReturnType> fnAdd3, Seq<TargetType1> seqTarget1, Seq<TargetType2> seqTarget2)
        {
            Seq<ReturnType> rMerge = new Seq<ReturnType>();
            int _count = System.Math.Min(Count, System.Math.Min(seqTarget1.Count, seqTarget2.Count));
            for (int i = 0; i < _count; i++) rMerge.Add(fnAdd3(this[i], seqTarget1[i], seqTarget2[i]));
            return rMerge;
        }

        public Seq<Seq<T>> Partition(int rowLength)
        {
            Seq<T> _tempSentence = new Seq<T>();
            Seq<Seq<T>> partitionGroup = new Seq<Seq<T>>(); 
            for (int i = 0; i < Count; i++) {
                _tempSentence.Add((T)this[i]);
                if ((i + 1) % rowLength == 0) { partitionGroup.Add(_tempSentence); _tempSentence = new Seq<T>(); }
            } 
            return partitionGroup;
        } 
         
        #endregion inmutable seq operation

        #region inmutable value operation

        public ReturnType Reduce<ReturnType>(Func<ReturnType, T, ReturnType> reduceFunc, ReturnType initValue) { 
            for (int i = 0; i < Count; i++) initValue = reduceFunc(initValue, this[i]); return initValue;
        }

        public T Reduce(Func<T, T, T> reduceFunc, T initValue) { return Reduce<T>(reduceFunc, initValue); }

        public T Reduce(Func<T, T, T> reduceFunc)
        {
            if (Count <= 0) throw new Exception("Seq: Cannot apply reduce to a empty array without initValue"); 
            T initValue = First;
            for (int i = 1; i < Count; i++) initValue = reduceFunc(initValue, this[i]); 
            return initValue;
        }

        public T FirstMatch(Predicate<T> predFn, T defaultValue = default(T)) { return FirstMatch<T>(predFn, t => t, defaultValue); }
        public T LastMatch(Predicate<T> predFn, T defaultValue = default(T)) { return LastMatch<T>(predFn, t => t, defaultValue); }

        public ReturnType FirstMatch<ReturnType>(Predicate<T> predFn, Converter<T, ReturnType> convertFunc, ReturnType defaultValue = default(ReturnType)) {
            for (int i = 0; i < Count; i++) if (predFn(this[i])) return convertFunc(this[i]); return defaultValue;
        }

        public ReturnType LastMatch<ReturnType>(Predicate<T> predFn, Converter<T, ReturnType> convertFunc, ReturnType defaultValue = default(ReturnType)) {
            for(int i = LastIndex; i > 0; i--) if(predFn(this[i])) return convertFunc(this[i]); return defaultValue;
        }

        #endregion inmutable value operation

        #region mutable operation

        

        public Seq<T> DoX(Func<Seq<T>, Seq<T>> f) { f(this); return this; }

        public Seq<T> RemoveX(Predicate<T> predFn) { RemoveAll(predFn); return this; }

        public Seq<T> RemoveX(IEnumerable<T> seq) { RemoveAll(item => seq.Contains(item)); return this; }

        public Seq<T> FilterX(Predicate<T> predFn) { return RemoveX(t => !predFn(t)); }

        public Seq<T> MergeWithX(Func<T, T, T> f, Seq<T> s) { for (int i = 0; i < Count && i < s.Count; i++) this[i] = f(this[i], s[i]); return this; }

        public Seq<T> SortByX(Comparison<T> compFn) { QuickSort(compFn, 0, Count - 1); return this; }

        public Seq<T> SortByX(Func<T, IComparable> compFn) { return SortByX((x, y) => compFn(x).CompareTo(compFn(y))); }

        public Seq<T> Replace(T item, T newItem)
        {
            if (!Contains(item)) { Append(newItem); }
            else { 
                int index = IndexOf(item);
                Insert(index, newItem);
                Remove(item);
            }
            return this;
        }

        public void QuickSort(Comparison<T> comparison, int left, int right) {
            if (left < right) {
                int middle = (left + right) >> 1;
                int i = left - 1;
                int j = right + 1;
                T middleV = this[middle];
                while (true) {
                    while (comparison(this[++i], middleV) < 0);
                    while (comparison(this[--j], middleV) > 0);

                    if (i >= j)
                        break;

                    Swap(i, j);
                }

                QuickSort(comparison, left, i - 1);
                QuickSort(comparison, j + 1, right);
            }
        }

        public void Swap(int indexA, int indexB) {
            T tmp = this[indexA];
            this[indexA] = this[indexB];
            this[indexB] = tmp;
        }

        public int GetCount()
        {
            return this.Count;
        }
        #endregion

        #region Batch Remover

        private List<T> _doSeqRemover = null;
        public void DoSeqRemover_Reg(int i) {
            if (_doSeqRemover == null) _doSeqRemover = new List<T>(Count);
            T item = this[i];
            if (!_doSeqRemover.Contains(item)) _doSeqRemover.Add(item);
        }

        private void DoSeqRemover_Excute() { 
            if(_doSeqRemover != null && _doSeqRemover.Count > 0) {
                RemoveX(t => _doSeqRemover.Contains(t));
                _doSeqRemover.Clear();
            } 
        }

        #endregion

        #region Statistics

        public double Frequency(Predicate<T> predFn) { return AmountOf(predFn) / (double)Count; }

        #endregion

        #region Static Method
         
        public static implicit operator Seq<T>(T[] array) { return new Seq<T>(array); }

        public static Seq<T> Parse(params T[] array) { return new Seq<T>(array); }

        public static Seq<T> Parse(List<T> array) { return new Seq<T>(array); }

        public static Seq<T> Parse(int times, Func<int, T> func)
        {
            Seq<T> seq = new Seq<T>(default(T), times); 
            for (int i = 0; i < times; i++) seq.Add(func(i)); 
            return seq;
        }

        public static Seq<T> Combine(params IEnumerable<T>[] args)
        {
            Seq<T> seq = new Seq<T>();
            for (int i = 0; i < args.Length; i++) seq.Add(args[i]); 
            return seq;
        }

        public static Seq<T> operator +(Seq<T> s1, T s2) { return s1.Clone.Add(s2); }

        public static Seq<T> operator -(Seq<T> s1, T s2) { return s1.Clone.RemoveItem(s2); }

        public static Seq<T> operator +(Seq<T> s1, IEnumerable<T> s2) { return Combine(s1, s2); }

        public static Seq<T> operator -(Seq<T> s1, IEnumerable<T> s2) { return s1.Clone.RemoveX(s2); }

        #endregion
        

        public override string ToString()
        {
            return Reduce<string>((s, t) => s + " " + (t == null ? "null" : t.ToString()), "Seq<" + typeof(T).Name + ">(" + Count + "):");
        }

        /// <summary>
        /// G:返回随机排序的num个项的Seq
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public Seq<T> RandItems(int num)
        {
            Seq<int> indexs = new Seq<int>();
            for (int i = 0; i < Count; i++)
            {
                indexs.Add(i);
            }
            for (int i = 0; i < Count; i++)
            {
                int k = RG.Basic.Random.Range(0, Count);
                int v = indexs[i];
                indexs[i] = indexs[k];
                indexs[k] = v;
            }
            Seq<T> lst = new Seq<T>();
            for (int i = 0; i < num && i < Count; i++)
            {
                lst.Add(this[indexs[i]]);
            }
            return lst;
        }
    }
}
