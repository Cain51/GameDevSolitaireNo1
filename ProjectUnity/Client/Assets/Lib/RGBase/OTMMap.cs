using System;
using System.Collections;
using System.Collections.Generic;


namespace RG.Basic {

    public class OTMMapException : Exception { public OTMMapException(string msg) : base(msg) { } }

    public class OTMMap<TKey, TValue> {
        readonly Map<TKey, Seq<TValue>> _KeyToValue ;
        readonly Map<TValue, TKey> _ValueToKey;

        public TValue this[TKey key] { 
            set { Set(key, value); } 
        }

        public OTMMap() {
            _KeyToValue = new Map<TKey, Seq<TValue>>();
            _ValueToKey = new Map<TValue, TKey>();
        }

        public OTMMap(bool cached) {
            _KeyToValue = new Map<TKey, Seq<TValue>>(cached);
            _ValueToKey = new Map<TValue, TKey>(cached);
        }

        public int Count { get { return _KeyToValue.Count; } }

        public int KeyCount { get { return _KeyToValue.Count; } }

        public int ValueCount { get { return _ValueToKey.Count; } }

        public Seq<TKey> KeySeq { get { return _KeyToValue.KeySeq; } }

        public Seq<TValue> ValueSeq { get { return _ValueToKey.KeySeq; } } 

        public ICollection<TKey> Keys { get { return  _KeyToValue.Keys; } }

        public ICollection<TValue> Values { get { return _ValueToKey.Keys; } } 

        public void Add(KeyValuePair<TKey, TValue> item) {
            lock (this) {
                if (_ValueToKey.ContainsKey(item.Value)) throw new OTMMapException("BiMap Add Error : the value " + item.Value + " is already exist. ");
                if (_KeyToValue.ContainsKey(item.Key) && _KeyToValue[item.Key] != null) _KeyToValue[item.Key].Add(item.Value);
                else _KeyToValue[item.Key] = new Seq<TValue>(item.Value); 
                _ValueToKey.Add(item.Value, item.Key);
            }
        } 

        public void Add(TKey key, TValue value) {
            lock (this) {
                if (_ValueToKey.ContainsKey(value)) throw new OTMMapException("BiMap Add Error : the value " + value + " is already exist. ");
                if (_KeyToValue.ContainsKey(key) && _KeyToValue[key] != null) _KeyToValue[key].Add(value);
                else _KeyToValue[key] = new Seq<TValue>(value);
                _ValueToKey.Add(value, key); 
            }
        }

        public void Clear() {
            lock (this) {
                _KeyToValue.Clear();
                _ValueToKey.Clear();
            } 
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) { return ContainsKey(item.Key) && ContainsValue(item.Value); }

        public bool ContainsKey(TKey key) { return _KeyToValue.ContainsKey(key); }

        public bool ContainsValue(TValue value) { return _ValueToKey.ContainsKey(value); }
         

        public bool Remove(KeyValuePair<TKey, TValue> item) { return RemoveByKey(item.Key); }


        public bool Remove(TKey key) { return RemoveByKey(key); }

        public bool RemoveByKey(TKey key) {
            lock (this) {
                if (ContainsKey(key)) {
                    Seq<TValue> value = _KeyToValue[key];
                    bool succed = true;
                    succed &= _KeyToValue.Remove(key);
                    for (int i = 0; i < value.Count; i++) {
                        succed &= _ValueToKey.Remove(value[i]);
                    }
                    return succed;
                }
                return false;
            }
        }

        public bool RemoveByValue(TValue value) {
            lock (this) {
                if (ContainsValue(value)) {
                    TKey key = _ValueToKey[value];
                    bool succed = true; 
                    succed &= _ValueToKey.Remove(value);
                    succed &= _KeyToValue[key].Remove(value);
                    if (_KeyToValue[key].Count <= 0) _KeyToValue.Remove(key);
                    return succed;
                }
                return false;
            }
        }

        public OTMMap<TKey, TValue> RemoveByValue(Predicate<TValue> condition) {
            lock (this) {
                Seq<TValue> toRemove = _ValueToKey.KeySeq.FilterX(condition);
                for(int i = 0;i < toRemove.Count; i++) {
                    RemoveByValue(toRemove[i]);
                } 
                return this;
            }
        }

        public Seq<TValue> GetValueIfExist(TKey key, Seq<TValue> defValue) {
            return _KeyToValue.ContainsKey(key) ? _KeyToValue[key] : defValue;
        }

        public Seq<TValue> GetValue(TKey key) {
            return _KeyToValue[key];
        }

        public TKey GetKey(TValue value) {
            return _ValueToKey[value];
        }
          
        public OTMMap<TKey, TValue> Set(TKey key, TValue value) {
            lock (this) {
                if (ContainsValue(value)) {
                    TKey orgKey = _ValueToKey[value]; 
                    _KeyToValue[orgKey].Remove(value);
                    if (_KeyToValue[orgKey].Count <= 0) _KeyToValue.Remove(orgKey); 
                }
                if (_KeyToValue.ContainsKey(key) && _KeyToValue[key] != null) _KeyToValue[key].Add(value);
                else _KeyToValue[key] = new Seq<TValue>(value);
                _ValueToKey[value] = key;
            }
            return this;
        }  

        public bool TryGetKey(TValue value, out TKey key) {
            return _ValueToKey.TryGetValue(value, out key);
        }

        public override string ToString() { 
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("OTMMap(").Append(KeyCount).Append(',').Append(ValueCount).Append("):");
            foreach(var key in Keys) sb.Append(key).Append(":").Append(GetValue(key)); 
            return sb.ToString();
        }
    }

}