using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RGBase.DataType {
    public class IntPair<V> {
        public IntPair(int _key, V _value) {
            Key = _key;
            Value = _value;
        }
        public int Key;
        public V Value;
    }
    public class IntMap<V> {
        private List<IntPair<V>> _pairs;    // 有序对
        private int _count;
        public int Count {
            get { if (_pairs == null) return 0; return _pairs.Count; }
        }
        public IntMap(int capacity) {
            _pairs = ListPool<IntPair<V>>.Inst.Fetch(capacity);
        }

        public IntMap() {
            _pairs =ListPool<IntPair<V>>.Inst.Fetch();
        }

        public int Find(int key) {
            int mid;
            return Find(key, out mid);
        }

        public int Find(int key, out int mid) {
            mid = 0;
            if (_pairs == null || _pairs.Count == 0) return -1;
            int left = 0;
            int right = _pairs.Count - 1;
            mid = (left + right) / 2;
            do {
                var midStr = _pairs[mid].Key;
                if (midStr == key) {
                    return mid;
                } else if (midStr < key) {
                    left = mid + 1;
                } else {
                    right = mid - 1;
                }
                mid = (left + right) / 2;
            } while (left <= right);

            return -1;
        }

        public IntPair<V> At(int index) {
            if (index >= _count) return null;
            return _pairs[index];
        }


        public V this[int index] {
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
                var count = _count;
                int mid = 0;
                var ind = Find(index, out mid);
                if (ind == -1) {
                    if (count == 0) {
                        _pairs.Add(new IntPair<V>(index, value));
                        _count++;
                        return;
                    }
                    if (_pairs[mid].Key < index) { //_pairs[mid].Key.CompareTo(index) == -1) {
                        _pairs.Insert(mid + 1, new IntPair<V>(index, value));
                        _count++;
                        return;
                    }
                    _pairs.Insert(mid, new IntPair<V>(index, value));
                    _count++;
                } else {
                    _pairs[ind] = new IntPair<V>(index, value);
                }
            }
        }

        public bool ContainsKey(int key) {
            if (_pairs == null) return false;
            var ind = Find(key);
            return ind != -1;
        }

        public void Remove(int key) {
            if (_pairs == null) return;
            var ind = Find(key);
            if (ind != -1) {
                _pairs.RemoveAt(ind);
                _count--;
            }
        }

        public void Clear() {
            if (_pairs == null) return;
            var count = _pairs.Count;
            for (int i = count - 1; i >= 0; i--) {
                var pair = _pairs[i];
            }
            _count = 0;
        }

        public V GetIfExist(int key, V defaultValue) {
            if (_pairs == null) return defaultValue;
            var ind = Find(key);
            if (ind == -1)
                return defaultValue;
            else
                return _pairs[ind].Value;
        }

        public bool TryGetValueWithIndex(int key, out V val, out int ind) {
            val = default(V);
            ind = -1;
            if (_pairs == null) return false;
            ind = Find(key);
            if (ind == -1) {
                return false;
            } else {
                val = _pairs[ind].Value;
                return true;
            }
        }

        public bool TryGetValue(int key, out V val) {
            val = default(V);
            if (_pairs == null) return false;
            var ind = Find(key);
            if (ind == -1) {
                return false;
            } else {
                val = _pairs[ind].Value;
                return true;
            }
        }

        public void Dispose() {
            ListPool<IntPair<V>>.Inst.Return(_pairs);
        }
    }
}
