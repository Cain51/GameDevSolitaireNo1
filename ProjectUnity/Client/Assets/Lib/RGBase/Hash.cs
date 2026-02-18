using System;
using System.Collections.Generic;
using System.Text;

namespace RG.Basic
{
    public class Hash<TValue>
    {
        const int PRIME_SEED = 1007;
        public class HashNode
        {
            public int key;
            public TValue val;
            public HashNode() { }
            public HashNode(int _key,TValue _val) { key = _key;val = _val; }
        }
        Seq<HashNode>[] hashTable = new Seq<HashNode>[PRIME_SEED + 3];

        public Hash() { }
        public void Clear()
        {
            hashTable = new Seq<HashNode>[PRIME_SEED + 3];
        }

        public bool ContainsKey(int key)
        {
            int index = HashIndex(key);
            if (hashTable[index] != null)
            {
                for (int i = 0; i < hashTable[index].Count; i++)
                    if (hashTable[index][i].key == key)
                        return true;
            }
            return false;
        }

        int HashIndex(int code)
        {
            return code %= PRIME_SEED;
        }
        public TValue GetVal(int key, TValue defaultValue = default(TValue)) { 
            int index = HashIndex(key);
            if (hashTable[index] != null) {
                for (int i = 0; i < hashTable[index].Count; i++)
                    if (hashTable[index][i].key == key)
                        return hashTable[index][i].val;
            }
            return defaultValue;
        }

        public bool TryGetValue(int key, out TValue found)
        {
            int index = HashIndex(key);
            if (hashTable[index] != null)
            {
                for (int i = 0; i < hashTable[index].Count; i++)
                    if (hashTable[index][i].key == key)
                    {
                        found = hashTable[index][i].val;
                        return true;
                    }
            }
            found = default(TValue);
            return false;
        }

        public void SetVal(int key, TValue val)
        {
            int index = HashIndex(key);
            if (hashTable[index] == null) hashTable[index] = new Seq<HashNode>();
            bool alreadyHave = false;
            for (int i = 0; i < hashTable[index].Count; i++)
                if (hashTable[index][i].key == key)
                {
                    alreadyHave = true;
                    hashTable[index][i].val = val;
                }
            if (!alreadyHave) {
                hashTable[index].Add(new HashNode(key, val));
                count += 1;
            }
        }

        public int Count { get { return count; } }
        private int count = 0;

        public TValue this[int key]
        {
            get{ return GetVal(key); }
            set { SetVal(key, value); }
        }

        public void Remove(int key)
        {
            int index = HashIndex(key);
            if (hashTable[index] == null) return;
            int seqLenBeforeRemove = hashTable[index].Count;
            hashTable[index].RemoveX(x => x.key == key);
            count -= (seqLenBeforeRemove - hashTable[index].Count);
        }

        public void RemoveAll()
        {
            for (int i = 0; i < PRIME_SEED; i++)
                hashTable[i] = null;
            count = 0;
        }

        public void Foreach(System.Action<TValue> func)
        {
            for(int i=0;i<PRIME_SEED;i++)
            {
                if(hashTable[i] != null)
                {
                    for(int j=0;j<hashTable[i].Count;j++)
                    {
                        func(hashTable[i][j].val);
                    }
                }
            }
        } 

        public Seq<int> GetKeys() {
            Seq<int> keys = new Seq<int>();
            for (int i = 0; i < PRIME_SEED; i++) if (hashTable[i] != null) keys.Add(i);
            return keys;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder(" ");
            Foreach(v => {
                sb.Append(v).Append(",");
            });
            return base.ToString() + sb.ToString();
        }
    }
}
