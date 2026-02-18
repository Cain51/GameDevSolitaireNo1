using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RGBase.DataType {
    public class ListPool<V> {
        public const int POOL_SIZE = 100;
        public const int BUFFER_SIZE = 20;
        private static ListPool<V> _inst;
        public static ListPool<V> Inst {
            get {
                if (_inst == null) {
                    _inst = new ListPool<V>();
                    _inst.Init();
                }
                return _inst;
            }
        }

        private Stack<List<V>> _pool;
        private bool inited = false;
        private void Init() {
            if (!inited) {
                _pool = new Stack<List<V>>(POOL_SIZE);
                for(int i = 0; i<POOL_SIZE; i++) {
                    _pool.Push(new List<V>(BUFFER_SIZE));
                }
                inited = true;
            }
        }
        public void Return(List<V> list) {
            if (_pool.Count <= POOL_SIZE) {
                list.Clear();
                _pool.Push(list);
            }
        }

        public List<V> Fetch(int capacity = 0) {
            if (capacity == 0) capacity = BUFFER_SIZE;
            if (_pool.Count == 0) {
                return new List<V>(capacity);
            }
            if (_pool.Peek().Count >= capacity) {
                return _pool.Pop();
            }
            return new List<V>(capacity);
        }
    }
}
