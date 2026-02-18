using System;
using System.Collections;
using System.Collections.Generic;


namespace RG.Basic {

    public class BIMapException : Exception { public BIMapException(string msg) : base(msg) { } }

    public class BiMap<TKey, TValue> : IDictionary<TKey, TValue>
        {
        readonly IDictionary<TKey, TValue> _KeyToValue = new Map<TKey, TValue>();
        readonly IDictionary<TValue, TKey> _ValueToKey = new Map<TValue, TKey>();

        public TValue this[TKey key] {
            get { return GetValue(key); } 
            set { Set(key, value); } 
        }

        public int Count { get { return _KeyToValue.Count; } }

        public bool IsReadOnly { get { return _KeyToValue.IsReadOnly; } }

        public ICollection<TKey> Keys { get { return  _KeyToValue.Keys; } }

        public ICollection<TValue> Values { get { return _KeyToValue.Values; } }

        public void Add(KeyValuePair<TKey, TValue> item) {
            lock (this) {
                if (_KeyToValue.ContainsKey(item.Key)) throw new BIMapException("BiMap Add Error : the key " + item.Key + " is already exist. ");
                if (_ValueToKey.ContainsKey(item.Value)) throw new BIMapException("BiMap Add Error : the value " + item.Value + " is already exist. ");
                _KeyToValue.Add(item.Key, item.Value);
                _ValueToKey.Add(item.Value, item.Key);
            }
        } 

        public void Add(TKey key, TValue value) {
            lock (this) {
                if (_KeyToValue.ContainsKey(key)) throw new BIMapException("BiMap Add Error : the key " + key + " is already exist. ");
                if (_ValueToKey.ContainsKey(value)) throw new BIMapException("BiMap Add Error : the value " + value + " is already exist. ");
                _KeyToValue.Add(key, value);
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

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { _KeyToValue.CopyTo(array, arrayIndex); }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return _KeyToValue.GetEnumerator(); }

        public bool Remove(KeyValuePair<TKey, TValue> item) { return RemoveByKey(item.Key); }

        public bool Remove(TKey key) { return RemoveByKey(key); }

        public bool RemoveByKey(TKey key) {
            lock (this) {
                if (ContainsKey(key)) {
                    TValue value = _KeyToValue[key];
                    bool succed = true;
                    succed &= _KeyToValue.Remove(key);
                    succed &= _ValueToKey.Remove(value);
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
                    succed &= _KeyToValue.Remove(key);
                    return succed;
                }
                return false;
            }
        }

        public TValue GetValue(TKey key) {
            return _KeyToValue[key];
        }

        public TKey GetKey(TValue value) {
            return _ValueToKey[value];
        }
          
        public BiMap<TKey, TValue> Set(TKey key, TValue value) {
            lock (this) {

                if (_KeyToValue.ContainsKey(key)) {
                    TValue orgValueOfKey = _KeyToValue[key];
                    _ValueToKey.Remove(orgValueOfKey);
                }
                if (_ValueToKey.ContainsKey(value)) {
                    TKey orgKeyOfValue = _ValueToKey[value];
                    _KeyToValue.Remove(orgKeyOfValue);
                }
                 
                _KeyToValue[key] = value;
                _ValueToKey[value] = key;
                 
            }
            return this;
        } 

        public bool TryGetValue(TKey key, out TValue value) {
            return _KeyToValue.TryGetValue(key, out value);
        }

        public bool TryGetKey(TValue value, out TKey key) {
            return _ValueToKey.TryGetValue(value, out key);
        }

         
        IEnumerator IEnumerable.GetEnumerator() {
            return _KeyToValue.GetEnumerator();
        }
    }

}