using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace RG.Basic {  
    public class Map<TKey, TValue> : Dictionary<TKey, TValue> {

        public Map() : base() { }
        public Map(bool cacheKeySeq) : base() { m_cacheKeySeq = cacheKeySeq; }
        public Map(int capacity, bool cacheKeySeq = false) : base(capacity) { m_cacheKeySeq = cacheKeySeq; }
        public Map(IDictionary<TKey, TValue> origin, bool cacheKeySeq = false) : base(origin) { m_cacheKeySeq = cacheKeySeq; }

        private bool CacheKeySeq { get { return m_cacheKeySeq; } }
        private bool m_cacheKeySeq = false;

        public event Action<TKey, TValue> OnAdd;
        public event Action<TKey, TValue> OnSet;
        public event Action<TKey> OnDel;
        public event Action OnClear;
        public event Action<TKey> OnChange;
        public void ClearEvents() {
            OnAdd = null;
            OnSet = null;
            OnDel = null;
            OnClear = null;
            OnChange = null;
        }

        public delegate TValue _AppendFunc(TValue t1, TValue t2);
        public delegate TValue _AppendFunc<TAppend>(TValue t1, TAppend t2);


        public new virtual TValue this[TKey key] {
            get { return base[key]; }
            set {
                if (ContainsKey(key)) {
                    base[key] = value;
                    if(OnChange != null) OnChange(key);
                    if (OnSet != null) OnSet(key, value); 
                } else Add(key, value);
            }
        }

        public TValue Ensure(TKey key) {
            if (!ContainsKey(key)) Add(key, default(TValue));
            return this[key];
        }

        public TValue EnsureDefault(TKey key, TValue defaultValue) {
            if (!ContainsKey(key)) Add(key, defaultValue);
            return this[key];
        }

        public int GetCount() { return Count; }


        public TValue GetIfExist(TKey key, TValue defaultValue) {
            if (!ContainsKey(key)) return defaultValue;
            return this[key];
        }

        public virtual new void Add(TKey key, TValue value) {
            base.Add(key, value);
            if (m_cacheKeySeq) { KeySeq.Add(key); }
            if (OnChange != null) OnChange(key);
            if (OnAdd != null) OnAdd(key, value); 
        }

        public virtual new bool Remove(TKey key) {
            bool result = base.Remove(key);
            if (m_cacheKeySeq && result) { KeySeq.Remove(key); }
            if (OnChange != null) OnChange(key);
            if (OnDel != null) OnDel(key); 
            return result;
        }

        public virtual new void Clear() {
            base.Clear();
            if (m_cacheKeySeq) { KeySeq.Clear(); }
            if (OnClear != null) OnClear(); 
        }

        public bool SafeRemove(TKey key) { if (ContainsKey(key)) return Remove(key); return false; }

        public Map<TKey, TValue> RemoveAll(Func<TKey, TValue, bool> PredForPair) {
            if (PredForPair != null) {
                Seq<TKey> matchKeys = new Seq<TKey>();
                foreach (var p in this) { if (PredForPair(p.Key, p.Value)) matchKeys.Add(p.Key); }
                matchKeys.DoSeq(k => Remove(k));
            }
            return this;
        }

        public bool Exists(Func<TKey, TValue, bool> PredForPair) {
            if (PredForPair != null) foreach (var p in this) if (PredForPair(p.Key, p.Value)) return true;
            return false;
        }

        public bool HasEvery(Func<TKey, TValue, bool> predFn) { return !Exists((k, v) => !predFn(k, v)); }

        public bool HasNone(Func<TKey, TValue, bool> predFn) { return !Exists((k, v) => predFn(k, v)); }

        public bool HasAny(Func<TKey, TValue, bool> predFn) { return Exists(predFn); }

        public virtual Seq<TKey> KeySeq {
            get {
                if (m_cacheKeySeq) {
                    if (keySeq == null) keySeq = new Seq<TKey>();
                    return keySeq;
                } else {
                    TKey[] keys = new TKey[Count];
                    Keys.CopyTo(keys, 0);
                    return new Seq<TKey>(keys);
                }
            }
        }
        private Seq<TKey> keySeq;

        public Seq<TValue> ValueSeq {
            get {
                TValue[] values = new TValue[Count];
                Values.CopyTo(values, 0);
                return new Seq<TValue>(values);
            }
        }

        public Map<TKey, TValue> Clone {
            get {
                Map<TKey, TValue> mapClone = new Map<TKey, TValue>();
                ForEachPairs((k, v) => mapClone[k] = v);
                return mapClone;
            }
        }

        public void ForEachPairs(Action<TKey, TValue> Func) {
            if (Func == null) return;
            if (m_cacheKeySeq) {
                for(int i = 0; i < KeySeq.Count; i++) {
                    Func(KeySeq[i], this[KeySeq[i]]);
                }
            }
            else {
                Enumerator e = GetEnumerator();
                while (e.MoveNext()) {
                    if (e.Current.Key != null)
                        Func(e.Current.Key, e.Current.Value);
                }
            }
        }

        /// <summary>
        /// 在一个关键字上按照输入的“附加函数”附加一个值：如果该关键字本不存在或者“附加函数”为空，直接将该值放到关键字上；如果关键字存在且存在“附加函数”，则按照该附加函数修改关键字位置的值
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="value">输入值</param>
        /// <param name="appendFunc">“附加函数”，第一个参数为关键字所在位置的值，第二个参数为输入值</param>
        /// <returns>返回Attributes本身</returns>
        public Map<TKey, TValue> AppendValue(TKey key, TValue value, _AppendFunc appendFunc = null) {
            if (!ContainsKey(key) || appendFunc == null) this[key] = value;
            else this[key] = appendFunc(this[key], value);
            if (OnChange != null) OnChange(key);
            return this;
        }

        /// <summary>
        /// 在一个关键字上按照输入的“附加函数”附加一个值，按照该附加函数修改关键字位置的值
        /// </summary>
        public Map<TKey, TValue> AppendValue<TAppend>(TKey key, TAppend appendValue, TValue init, _AppendFunc<TAppend> appendFunc) {
            if (!ContainsKey(key) || appendFunc == null) this[key] = init;
            else this[key] = appendFunc(this[key], appendValue);
            if (OnChange != null) OnChange(key);
            return this;
        }

        public override string ToString() {
            return KeySeq.Reduce((str, k) => str + ", {\"" + k + "\" : \"" + this[k] + "\"}", "[[\"Map\"]") + "]";
        }

    }
}
