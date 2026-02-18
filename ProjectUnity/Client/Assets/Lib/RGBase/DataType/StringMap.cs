using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RGBase.DataType {
    public struct Pair<V> {
        public Pair(string _key, V _value) {
            Key = _key;
            Value = _value;
        }
        public string Key;
        public V Value;
    }

    // 用于比较短的字典
    public class StringMap<V> where V : class {
        public event Action<string> OnChange;
        private List<Pair<V>> _pairs;    // 有序对

        public int Count {
            get { if (_pairs == null) return 0; return _pairs.Count; }
        }
        public StringMap() {

        }

        public int Find(string key) {
            int mid;
            return Find(key, out mid);
        }

        public int CompareStr(string a, string b) {
            for(int i = 0; i < a.Length && i < b.Length; i++) {
                int ret = a[i] - b[i];
                if (ret != 0) return ret;
            }
            return a.Length - b.Length;
        }

        public int Find(string key, out int mid) {
            mid = 0;
            if (_pairs == null || _pairs.Count == 0) return -1;
            int left = 0;
            int right = _pairs.Count - 1;
            mid = (left + right) / 2;
            do {
                var midStr = _pairs[mid].Key;
                var res = CompareStr(midStr, key); //string.Compare(midStr, key);
                if (res == 0) {
                    return mid;
                } else if (res < 0) {
                    left = mid + 1;
                } else {
                    right = mid - 1;
                }
                mid = (left + right) / 2;
            } while (left <= right);
            
            return -1;
        }
        

        public void DoKey(Action<string> action) {
            if (_pairs == null) return;
            for (int i = 0; i < _pairs.Count; i++) {
                var pair = _pairs[i];
                action(pair.Key);
            }
        }

        public void ForEachPairs(Action<string, V> action) {
            if (_pairs == null) return;
            for (int i = 0; i < _pairs.Count; i++) {
                var pair = _pairs[i];
                action(pair.Key, pair.Value);
            }
        }

        public bool AllEqual(StringMap<V> map) {
            if (_pairs == null && map == null) return true;
            if (_pairs == null || map == null) return false;
            if (_pairs.Count != map.Count) return false;
            for (int i = 0; i < _pairs.Count; i++) {
                var pair1 = _pairs[i];
                for (int j = i; j < map.Count; j++) {
                    var pair2 = _pairs[j];
                    if (pair1.Key == pair2.Key && pair1.Value != pair2.Value) {
                        return false;
                    }
                }
            }
            return true;
        }

        public void RemoveAll(Func<string, V, bool> func) {
            if (_pairs == null) return;
            int i = 0;
            while (i < _pairs.Count) {
                var pair = _pairs[i];
                if (func(pair.Key, pair.Value)) {
                    _pairs.RemoveAt(i);
                } else {
                    i++;
                }
            }
        }

        public StringMap(StringMap<V> Map) {
            if (Map._pairs != null)
                _pairs = new List<Pair<V>>(Map._pairs);
        }

        public Pair<V> At(int index) {
            return _pairs[index];
        }


        public V this[string index] {
            get {
                if (_pairs == null) throw new Exception("list not content key");
                var count = _pairs.Count;
                var ind = Find(index);
                if (ind == -1)
                    throw new Exception("list not content key");
                else
                    return _pairs[ind].Value;
            }
            set {
                if (_pairs == null) _pairs = new List<Pair<V>>();
                var count = _pairs.Count;
                int mid = 0;
                var ind = Find(index, out mid);
                if (ind == -1) {
                    if (count == 0) {
                        _pairs.Add(new Pair<V>(index, value));
                        if (OnChange != null)
                            OnChange.Invoke(index);
                        return;
                    }
                    if (CompareStr(_pairs[mid].Key, index) < 0){ //_pairs[mid].Key.CompareTo(index) == -1) {
                        _pairs.Insert(mid+1,new Pair<V>(index, value));
                        if (OnChange != null)
                            OnChange.Invoke(index);
                        return;
                    }
                    _pairs.Insert(mid, new Pair<V>(index, value));
                    if (OnChange != null)
                        OnChange.Invoke(index);
                } else {
                    if (_pairs[ind].Value == value) return;

                    _pairs[ind] = new Pair<V>(index, value);
                    if (OnChange != null)
                        OnChange.Invoke(index);
                }
            }
        }

        public bool ContainsKey(string key) {
            if (_pairs == null) return false;
            var ind = Find(key);
            return ind != -1;
        }

        public void Remove(string key) {
            if (_pairs == null) return;
            var ind = Find(key);
            if (ind != -1) {
                _pairs.RemoveAt(ind);
                if (OnChange != null)
                    OnChange.Invoke(key);
            }
        }

        public void Clear() {
            if (_pairs == null) return;
            var count = _pairs.Count;
            for (int i = count - 1; i >= 0; i--) {
                var pair = _pairs[i];
                _pairs.RemoveAt(i);
                if (OnChange != null)
                    OnChange.Invoke(pair.Key);
            }
        }

        public V GetIfExist(string key, V defaultValue) {
            if (_pairs == null) return defaultValue;
            var ind = Find(key);
            if (ind == -1)
                return defaultValue;
            else
                return _pairs[ind].Value;
        }
    }
}
